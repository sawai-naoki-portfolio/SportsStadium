using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// バスケットボール 1on1(ハーフコート)の試合進行。
    /// チェックボール → 攻防(ドリブル・シュート・スティール・ブロック・リバウンド) → 得点/ターンオーバーの流れを管理する。
    /// 成功率は BasketballShotModel、操作は IBasketballController、得点は BasketballScore に委ねる。
    /// </summary>
    public class BasketballGame : MatchBase, IBasketballView
    {
        enum Phase { Intro, Check, Live, Dead }
        enum BallState { Held, Shot, Loose }

        const float IntroTime = 2f;
        const float TurnoverPause = 1.4f, ScorePause = 1.7f;
        const float PickupRadius = 0.7f, PlayerSeparation = 0.62f;
        const float BlockWindow = 0.45f, BlockRadius = 0.55f;
        const float ShotCrouch = 0.12f, ShotTimeout = 0.95f;
        const float StealRange = 1.35f;
        /// <summary>レイアップはリングの手前この距離でリリースする。</summary>
        const float LayupReleaseDistance = 0.6f;
        const float NetAnimTime = 0.5f;
        const float PlayerHeight = 1.05f;
        const float LoosePredictInterval = 0.15f;
        const int HintPossessions = 3;
        static readonly Vector3 CheckOffense = new Vector3(0f, 0f, -8.4f);
        static readonly Vector3 CheckDefense = new Vector3(0f, 0f, -6.6f);

        [SerializeField] BasketballConfig config = new BasketballConfig();

        readonly BasketballPlayer[] players = new BasketballPlayer[2];
        readonly IBasketballController[] controllers = new IBasketballController[2];
        readonly BBCommand[] commands = new BBCommand[2];
        readonly BasketballStats stats = new BasketballStats();
        BasketballScore score;
        BasketballHudPresenter presenter;
        BallSim ball;
        BallView ballView;
        Transform net;
        HudMeter shotMeter;

        Phase phase;
        float phaseTime;
        BallState ballState;
        BasketballPlayer holder, shooter;
        ShotKind lastShotKind;
        int offense, lastTouch, shotPoints, humanPossessions;
        float shotTime, shotClock, dribblePhase, prevBallY, netAnim, looseTimer, stuckTimer, skill, perfectWindow;
        bool needsClear, touchedRim, blockRolled, suddenDeathShown;
        Vector3 looseTarget;

        protected override Sport Sport => Sport.Basketball;
        internal BasketballScore Score => score;
        internal BasketballStats Stats => stats;
        internal BasketballPlayer HumanPlayer => players[0];
        internal string PhaseName => phase.ToString();

        // ---- IBasketballView ----
        public BallSim Ball => ball;
        public BasketballPlayer Holder => holder;
        public bool NeedsClear => needsClear;
        public float ShotClock => shotClock;
        public Vector3 LooseBallTarget => looseTarget;
        public Transform CameraTransform => Rig.transform;
        public BasketballPlayer Opponent(BasketballPlayer self) => players[1 - self.Team];

        void Start()
        {
            skill = GameSettings.Skill;
            perfectWindow = BasketballConfig.ForSkill(config.perfectWindow, skill);
            score = CreateScore(GameSettings.MatchLength);
            var root = new GameObject("Basketball").transform;
            var court = BasketballCourtBuilder.Build(root, UITheme.Home);
            net = court.net;
            InitMatch(new MatchSetup
            {
                title = "BASKETBALL", homeName = "YOU", awayName = "CPU",
                buttons = new[] { new ButtonSpec("シュート", new Color(0.95f, 0.5f, 0.12f)), new ButtonSpec("クロス", new Color(0.25f, 0.55f, 0.95f)), new ButtonSpec("ダッシュ", new Color(0.3f, 0.75f, 0.4f)) },
                fov = 48f, background = new Color(0.08f, 0.09f, 0.13f), skybox = false,
                lighting = new SceneLighting.Preset
                {
                    sunEuler = new Vector3(68f, 25f, 0f), sun = new Color(1f, 0.97f, 0.9f), intensity = 1.15f,
                    sky = new Color(0.55f, 0.55f, 0.6f), equator = new Color(0.45f, 0.42f, 0.4f), ground = new Color(0.3f, 0.25f, 0.2f)
                },
                crowds = court.crowds,
                ambienceVolume = 0.3f,
                warmup = new[] { Sfx.Dribble, Sfx.BallFloor, Sfx.Swish, Sfx.Rim, Sfx.Board, Sfx.Buzzer, Sfx.Squeak },
            });

            ball = new BallSim { radius = BasketballDims.BallRadius, restitution = 0.8f, friction = 0.9f, rollDecay = 0.6f };
            ball.colliders.AddRange(BasketballDims.CreateColliders());
            ball.onContact = OnBallContact;
            ballView = BallView.Create(root, "Basketball", BasketballDims.BallRadius, ProcTextures.Ball(Sport.Basketball), 1.15f);

            // 身長はリバウンドの届く高さに直結するので両者で揃える
            players[0] = BasketballPlayer.Create(root, 0, HumanoidLook.Make(UITheme.Home, TeamColors.Dark(UITheme.Home), 7, PlayerHeight), "YOU");
            players[1] = BasketballPlayer.Create(root, 1, HumanoidLook.Make(UITheme.Away, new Color(0.55f, 0.1f, 0.1f), 13, PlayerHeight), "CPU");
            controllers[0] = DebugOptions.AutoPlay ? (IBasketballController)new BasketballAIController(this, config, skill) : new BasketballHumanController(this);
            controllers[1] = new BasketballAIController(this, config, skill);

            shotMeter = Hud.CreateMeter(new Color(1f, 0.6f, 0.2f));
            presenter = new BasketballHudPresenter(Hud);

            offense = Random.value < 0.5f ? 0 : 1;
            PlaceForCheck();
            Rig.Snap(new Vector3(0f, 7.5f, -18f), new Vector3(0f, 1.5f, -4f));
            phase = Phase.Intro;
        }

        static BasketballScore CreateScore(int matchLength)
        {
            switch (matchLength)
            {
                case 1: return new BasketballScore(21, 0f);
                case 2: return new BasketballScore(0, 180f);
                default: return new BasketballScore(11, 0f);
            }
        }

        void Update()
        {
            if (IsPaused) return;
            float dt = FrameDelta;
            if (dt <= 0f) return;
            phaseTime += dt;
            switch (phase)
            {
                case Phase.Intro:
                    IdlePlayers(dt);
                    if (phaseTime > IntroTime) StartCheck(offense);
                    break;
                case Phase.Check:
                    IdlePlayers(dt);
                    if (phaseTime > config.checkTime)
                    {
                        SetPhase(Phase.Live);
                        AudioManager.Play(Sfx.Whistle, 0.5f);
                    }
                    break;
                case Phase.Live:
                    UpdateLive(dt);
                    break;
                case Phase.Dead:
                    IdlePlayers(dt);
                    if (ballState != BallState.Held) ball.Step(dt);
                    break;
            }
            if (ballState == BallState.Held) PositionHeldBall(dt);
            ballView.Sync(ball.pos);
            UpdateNet(dt);
            UpdateCamera();
            UpdateHud();
        }

        void SetPhase(Phase p)
        {
            phase = p;
            phaseTime = 0f;
        }

        void IdlePlayers(float dt)
        {
            foreach (var p in players)
            {
                p.Move(Vector3.zero, 0f, false, config, dt);
                p.Tick(dt);
            }
        }

        // ================= 攻撃開始 =================

        void PlaceForCheck()
        {
            needsClear = false;
            touchedRim = false;
            shooter = null;
            ballState = BallState.Held;
            holder = players[offense];
            holder.Teleport(CheckOffense, Vector3.forward);
            Opponent(holder).Teleport(CheckDefense, Vector3.back);
            shotClock = config.shotClock;
            shotMeter.Hide();
        }

        void StartCheck(int team)
        {
            offense = team;
            PlaceForCheck();
            if (controllers[team].IsHuman) humanPossessions++;
            Hud.Banner.Notice(BasketballHudPresenter.Name(team) + " の攻撃", Color.white, 0.7f);
            SetPhase(Phase.Check);
        }

        // ================= プレー中 =================

        void UpdateLive(float dt)
        {
            for (int i = 0; i < 2; i++) commands[i] = controllers[i].Decide(players[i], dt);
            for (int i = 0; i < 2 && phase == Phase.Live; i++) ApplyCommand(players[i], commands[i], dt);
            BasketballPlayer.Separate(players[0], players[1], PlayerSeparation);
            foreach (var p in players) p.Tick(dt);
            if (phase != Phase.Live) return;

            if (ballState != BallState.Held) UpdateFreeBall(dt);
            if (phase != Phase.Live) return;
            UpdateClocks(dt);
            UpdateHints();
        }

        void ApplyCommand(BasketballPlayer p, BBCommand c, float dt)
        {
            if (p.shooting)
            {
                UpdateShooting(p, c, dt);
                return;
            }
            bool hasBall = holder == p;
            p.Move(c.move, hasBall ? config.dribbleSpeed : config.runSpeed, c.sprint, config, dt);
            FacePlayer(p, c.move, hasBall, dt);
            if (hasBall)
            {
                if (c.actionDown && !p.Airborne)
                {
                    if (needsClear) Hud.Banner.Notice("3Pラインの外へ！", new Color(1f, 0.85f, 0.4f), 0.6f);
                    else StartShot(p);
                }
                else if (c.skillDown && p.crossCooldown <= 0f) Crossover(p, c.move);
                return;
            }
            if (c.actionDown && !p.Airborne)
            {
                p.Jump(config.jumpSpeed);
                p.Body.Play(HAction.Block, 0.7f);
            }
            if (c.skillDown && holder == Opponent(p) && p.stealCooldown <= 0f) TrySteal(p);
        }

        void FacePlayer(BasketballPlayer p, Vector3 move, bool hasBall, float dt)
        {
            Vector3 face;
            if (hasBall) face = BasketballDims.ToHoop(p.Pos) + move * 0.8f;
            else if (holder != null) face = holder.Pos - p.Pos;
            else face = ball.pos - p.Pos;
            p.Face(face, dt);
        }

        // ================= シュート =================

        void StartShot(BasketballPlayer p)
        {
            float d = BasketballDims.HoopDistance(p.Pos);
            Vector3 toHoop = BasketballDims.ToHoop(p.Pos);
            float approach = Vector3.Dot(p.Velocity, toHoop);
            ShotKind kind = ShotKind.Jumper;
            if (d < BasketballShotModel.LayupRange && (approach > 1.5f || d < 1.2f))
                kind = (p.Sprinting || approach > 4.2f) && d < BasketballShotModel.DunkRange ? ShotKind.Dunk : ShotKind.Layup;

            p.shooting = true;
            p.shotKind = kind;
            p.shotTimer = 0f;
            p.shotFeet = p.Pos;
            p.shotJumped = kind != ShotKind.Jumper;
            blockRolled = false;
            switch (kind)
            {
                case ShotKind.Jumper:
                    p.Body.Play(HAction.Shoot, 0.9f);
                    break;
                case ShotKind.Layup:
                    p.Jump(config.jumpSpeed);
                    p.Drift(toHoop * Mathf.Max(0f, d - LayupReleaseDistance) / 0.37f);
                    p.Body.Play(HAction.Layup, 0.8f);
                    break;
                case ShotKind.Dunk:
                    p.Jump(config.dunkJumpSpeed);
                    p.Drift(toHoop * Mathf.Max(0f, d - 0.45f) / 0.45f);
                    p.Body.Play(HAction.Dunk, 0.9f);
                    break;
            }
        }

        void UpdateShooting(BasketballPlayer p, BBCommand c, float dt)
        {
            p.shotTimer += dt;
            p.Move(Vector3.zero, 0f, false, config, dt);
            p.Face(BasketballDims.ToHoop(p.Pos), dt);
            if (p.shotKind == ShotKind.Jumper)
            {
                if (!p.shotJumped && p.shotTimer >= ShotCrouch)
                {
                    p.Jump(config.shotJumpSpeed);
                    p.shotJumped = true;
                }
                float meter = p.shotTimer / config.shotPeakTime;
                if (controllers[p.Team].IsHuman) ShowShotMeter(p, meter);
                bool release = c.actionUp || (!c.actionHeld && p.shotTimer > ShotCrouch) || p.shotTimer > ShotTimeout;
                if (release) Release(p, meter);
                return;
            }
            // レイアップ・ダンク: リリース前の空中でブロックされる可能性がある
            if (TryBlock(Opponent(p), BallOverhead(p), p.shotKind))
            {
                p.shooting = false;
                return;
            }
            if (!p.Rising || p.shotTimer > 0.6f) Release(p, 1f);
        }

        void ShowShotMeter(BasketballPlayer p, float meter)
        {
            BasketballShotModel.TimingQuality(meter, perfectWindow, out bool perfect);
            shotMeter.Show(Hud.WorldToHud(p.Pos + Vector3.up * 2.2f + Rig.transform.right * 0.8f, Rig.Cam), meter, 1f, perfectWindow * 2f,
                perfect ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.6f, 0.2f));
        }

        Vector3 BallOverhead(BasketballPlayer p) => p.Pos + Vector3.up * (p.Body.Height + 0.3f + p.JumpHeight) + p.Facing * 0.2f;

        void Release(BasketballPlayer p, float meter)
        {
            shotMeter.Hide();
            p.shooting = false;
            holder = null;
            shooter = p;
            ballState = BallState.Shot;
            lastShotKind = p.shotKind;
            lastTouch = p.Team;
            shotTime = 0f;
            touchedRim = false;

            var def = Opponent(p);
            float d = BasketballDims.HoopDistance(p.shotFeet);
            bool perfect = false;
            float timing = p.shotKind == ShotKind.Jumper ? BasketballShotModel.TimingQuality(meter, perfectWindow, out perfect) : 1f;
            float contest = BasketballShotModel.Contest(p.Pos, def.Pos, def.Airborne);
            float bonus = controllers[p.Team].IsHuman ? BasketballConfig.ForSkill(config.playerShotBonus, skill) : BasketballConfig.ForSkill(config.aiShotSkill, skill);
            bool made = Random.value < BasketballShotModel.MakeChance(p.shotKind, d, timing, perfect, contest, bonus);
            shotPoints = BasketballShotModel.PointsFor(p.shotKind, p.shotFeet);
            stats.RecordAttempt(p.Team, shotPoints == 3);

            Vector3 toHoop = BasketballDims.ToHoop(p.Pos);
            if (p.shotKind == ShotKind.Dunk && made)
            {
                ball.Launch(BasketballDims.Hoop + Vector3.up * 0.28f - toHoop * 0.05f, new Vector3(0f, -4f, 0f) + toHoop * 0.2f);
                Rig.Shake(0.25f, 0.35f);
                SlowMotion();
            }
            else
            {
                Vector3 start = BallOverhead(p);
                Vector3 target = BasketballShotModel.AimPoint(made, p.Pos);
                float angle = BasketballShotModel.LaunchAngle(p.shotKind, d);
                if (!BallMath.SolveByAngle(start, target, angle, ball.gravity, out var v) && !BallMath.SolveByAngle(start, target, angle + 10f, ball.gravity, out v))
                    v = BallMath.SolveByTime(start, target, 0.9f, ball.gravity);
                ball.Launch(start, v);
            }
            if (perfect && controllers[p.Team].IsHuman) Hud.Banner.Notice("PERFECT!", new Color(0.4f, 1f, 0.5f), 0.5f);
            GameLog.Info("Basketball", $"{BasketballHudPresenter.Name(p.Team)} {p.shotKind} d={d:F1} made={made} perfect={perfect} contest={contest:F2}");
        }

        // ================= こぼれ球・判定 =================

        void UpdateFreeBall(float dt)
        {
            prevBallY = ball.pos.y;
            ball.Step(dt);
            shotTime += dt;
            if (phase != Phase.Live) return;
            if (CheckScore() || phase != Phase.Live) return;
            if (ballState == BallState.Shot && shotTime < BlockWindow && shooter != null && TryBlock(Opponent(shooter), ball.pos, lastShotKind)) return;
            CheckPickup();
            UpdateLooseTarget(dt);
            UnstickBall(dt);
        }

        bool CheckScore()
        {
            if (!BasketballDims.PassedThroughHoop(prevBallY, ball.pos, ball.vel)) return false;
            bool fromShot = ballState == BallState.Shot && shooter != null;
            AwardBasket(fromShot ? shooter.Team : lastTouch, fromShot ? shotPoints : 2, fromShot && lastShotKind == ShotKind.Dunk);
            return true;
        }

        void AwardBasket(int team, int points, bool dunk)
        {
            SetPhase(Phase.Dead);
            ball.vel = new Vector3(ball.vel.x * 0.15f, Mathf.Min(ball.vel.y, -2.2f), ball.vel.z * 0.15f);
            netAnim = NetAnimTime;
            bool ended = score.Add(team, points);
            stats.RecordMake(team, points == 3, dunk);
            string text = dunk ? "DUNK!!" : points == 3 ? "3 POINTS!" : "2 POINTS";
            Hud.Banner.Show(text, team == 0 ? UITheme.Gold : new Color(0.6f, 0.8f, 1f), 1.1f, BasketballHudPresenter.Name(team) + " のゴール");
            Hud.Scoreboard.Punch(team);
            AudioManager.Play(Sfx.Swish, 0.9f);
            PointFeedback(team == 0, dunk || points == 3);
            players[team].Body.Play(HAction.Celebrate, 1.2f);
            GameLog.Info("Basketball", $"score {BasketballHudPresenter.Name(team)} +{points} -> {score.Points(0)}-{score.Points(1)}");
            After(ScorePause, () =>
            {
                if (ended) EndMatch();
                else StartCheck(1 - team);
            });
        }

        /// <summary>ジャンプ中のディフェンスの手がボールに届いたら、1シュートにつき1回だけブロックを判定する。</summary>
        bool TryBlock(BasketballPlayer def, Vector3 ballPos, ShotKind kind)
        {
            if (blockRolled || !def.Airborne) return false;
            float dist = Vector3.Distance(def.HandTop, ballPos);
            if (dist >= BlockRadius) return false;
            blockRolled = true;
            if (Random.value >= BasketballShotModel.BlockChance(dist, BlockRadius, kind)) return false;
            Block(def, ballPos);
            return true;
        }

        void Block(BasketballPlayer def, Vector3 at)
        {
            // はじかれたボールは左右どちらかへ散らばる(シューター側に偏らせない)
            Vector3 dir = Quaternion.Euler(0f, Random.Range(-110f, 110f), 0f) * -BasketballDims.ToHoop(at);
            ball.Launch(at, dir * Random.Range(2f, 3.5f) + Vector3.up * Random.Range(-1f, 1.5f));
            GameLog.Info("Basketball", $"block by {BasketballHudPresenter.Name(def.Team)}");
            ballState = BallState.Loose;
            holder = null;
            shooter = null;
            lastTouch = def.Team;
            stats.blocks[def.Team]++;
            Hud.Banner.Show("BLOCK!", def.Team == 0 ? UITheme.Gold : new Color(0.6f, 0.8f, 1f), 0.8f);
            AudioManager.Play(Sfx.Board, 0.9f);
            Rig.Shake(0.15f, 0.25f);
            if (def.Team == 0) Crowd.Cheer(1.2f);
        }

        void CheckPickup()
        {
            // シュート直後(リングに届く前)は拾えない
            if (ballState == BallState.Shot && !touchedRim && (shotTime < 0.5f || ball.vel.y > 0f)) return;
            float d0 = PickupDistance(players[0]), d1 = PickupDistance(players[1]);
            if (d0 >= PickupRadius && d1 >= PickupRadius) return;
            if (d0 >= PickupRadius) { Pickup(players[1]); return; }
            if (d1 >= PickupRadius) { Pickup(players[0]); return; }
            // 2人とも届くときは近い方が有利な確率で競り合う
            float w0 = PickupRadius - d0, w1 = PickupRadius - d1;
            Pickup(Random.value * (w0 + w1) < w0 ? players[0] : players[1]);
        }

        /// <summary>ボールを拾える状態なら水平距離、拾えなければ無限大。</summary>
        float PickupDistance(BasketballPlayer p)
        {
            if (p.stun > 0.3f || p.shooting || ball.pos.y >= p.Reach + 0.25f) return float.PositiveInfinity;
            return new Vector2(ball.pos.x - p.Pos.x, ball.pos.z - p.Pos.z).magnitude;
        }

        void Pickup(BasketballPlayer p)
        {
            bool fromShot = ballState == BallState.Shot;
            holder = p;
            ballState = BallState.Held;
            lastTouch = p.Team;
            shooter = null;
            if (fromShot) stats.rebounds[p.Team]++;
            if (p.Team != offense)
            {
                offense = p.Team;
                needsClear = !BasketballDims.IsThree(p.Pos);
                shotClock = config.shotClock;
                Hud.Banner.Notice(fromShot ? "リバウンド" : "マイボール", Color.white, 0.6f, BasketballHudPresenter.Name(p.Team));
            }
            else if (fromShot || touchedRim) shotClock = config.shotClock;
            AudioManager.Play(Sfx.Dribble, 0.6f);
        }

        void TrySteal(BasketballPlayer p)
        {
            var h = holder;
            p.stealCooldown = 1f;
            p.Body.Play(HAction.Steal, 0.4f);
            Vector3 to = h.Pos - p.Pos;
            to.y = 0f;
            float d = to.magnitude;
            bool facing = d > 0.01f && Vector3.Dot(p.Facing, to / d) > 0.2f;
            float chance = controllers[p.Team].IsHuman ? config.playerStealChance : BasketballConfig.ForSkill(config.aiStealChance, skill);
            chance *= (h.Sprinting ? 1.35f : 1f) * (d < 0.9f ? 1.2f : 1f);
            if (d > StealRange || !facing || h.shooting || Random.value >= chance)
            {
                p.stun = 0.4f;
                return;
            }
            holder = null;
            ballState = BallState.Loose;
            lastTouch = p.Team;
            ball.Launch(ball.pos, -to / d * 2.5f + Vector3.up * 2f);
            stats.steals[p.Team]++;
            Hud.Banner.Show("STEAL!", p.Team == 0 ? UITheme.Gold : new Color(0.6f, 0.8f, 1f), 0.8f);
            AudioManager.Play(Sfx.Squeak, 0.6f);
            if (p.Team == 0) Crowd.Cheer(1f);
        }

        void Crossover(BasketballPlayer p, Vector3 move)
        {
            Vector3 toHoop = BasketballDims.ToHoop(p.Pos);
            Vector3 lateral = Vector3.Cross(Vector3.up, toHoop);
            float dot = Vector3.Dot(move, lateral);
            float sign = Mathf.Abs(dot) > 0.2f ? Mathf.Sign(dot) : (p.dribbleLeft ? 1f : -1f);
            p.Burst((lateral * sign + toHoop * 0.35f).normalized * config.crossoverSpeed, 0.3f);
            p.dribbleLeft = sign < 0f;
            p.crossCooldown = 1.1f;
            p.Body.Play(HAction.Crossover, 0.32f);
            AudioManager.Play(Sfx.Squeak, 0.7f, 1.1f);
            var def = Opponent(p);
            bool close = Vector3.Distance(def.Pos, p.Pos) < 2.2f;
            if (close && !controllers[def.Team].IsHuman && Random.value < BasketballConfig.ForSkill(config.aiAnkleBreak, skill))
            {
                def.stun = 0.65f;
                Hud.Banner.Notice("ナイスムーブ!", new Color(0.4f, 1f, 0.6f), 0.6f);
            }
        }

        void Turnover(int newOffense, string reason)
        {
            if (phase != Phase.Live) return;
            SetPhase(Phase.Dead);
            if (ballState == BallState.Held)
            {
                // 持っていたボールはその場に落とす
                ballState = BallState.Loose;
                ball.Launch(ball.pos, Vector3.zero);
            }
            holder = null;
            shotMeter.Hide();
            foreach (var p in players) p.shooting = false;
            Hud.Banner.Show(reason, new Color(1f, 0.7f, 0.4f), 1f, BasketballHudPresenter.Name(newOffense) + " ボール");
            After(TurnoverPause, () => StartCheck(newOffense));
        }

        void OnBallContact(BallContact c)
        {
            switch (c.kind)
            {
                case ContactKind.Ground:
                    if (c.speed > 0.5f) AudioManager.Play(Sfx.BallFloor, Mathf.Clamp01(c.speed / 6f));
                    if (phase == Phase.Live && ballState != BallState.Held && !BasketballDims.InBounds(c.point))
                    {
                        AudioManager.Play(Sfx.Whistle, 0.8f);
                        Turnover(1 - lastTouch, "アウト・オブ・バウンズ");
                    }
                    break;
                case ContactKind.Rim:
                    touchedRim = true;
                    AudioManager.Play(Sfx.Rim, Mathf.Clamp01(c.speed / 5f) * 0.8f + 0.2f, Random.Range(0.95f, 1.05f));
                    break;
                case ContactKind.Board:
                    AudioManager.Play(Sfx.Board, Mathf.Clamp01(c.speed / 6f));
                    break;
                default:
                    AudioManager.Play(Sfx.Board, 0.4f, 0.8f);
                    break;
            }
        }

        void UpdateClocks(float dt)
        {
            if (ballState != BallState.Shot)
            {
                shotClock -= dt;
                if (shotClock <= 0f)
                {
                    AudioManager.Play(Sfx.Buzzer, 0.8f);
                    Turnover(1 - offense, "ショットクロック違反");
                    return;
                }
            }
            if (score.Tick(dt))
            {
                AudioManager.Play(Sfx.Buzzer, 0.9f);
                SetPhase(Phase.Dead);
                EndMatch();
                return;
            }
            if (score.SuddenDeath && !suddenDeathShown)
            {
                suddenDeathShown = true;
                AudioManager.Play(Sfx.Buzzer, 0.8f);
                Hud.Banner.Show("延長戦", UITheme.Gold, 1.4f, "次のゴールで決着！");
            }
            if (needsClear && holder != null && BasketballDims.IsThree(holder.Pos))
            {
                needsClear = false;
                if (controllers[holder.Team].IsHuman) Hud.Banner.Notice("クリア OK", new Color(0.4f, 1f, 0.6f), 0.5f);
            }
        }

        /// <summary>リングの上などでボールが止まってしまったら少し弾く。</summary>
        void UnstickBall(float dt)
        {
            bool resting = ball.vel.sqrMagnitude < 0.04f && ball.pos.y > 1f;
            stuckTimer = resting ? stuckTimer + dt : 0f;
            if (stuckTimer < 1.5f) return;
            stuckTimer = 0f;
            ball.Launch(ball.pos, new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1.5f, -0.5f)));
        }

        void UpdateLooseTarget(float dt)
        {
            looseTimer -= dt;
            if (looseTimer > 0f) return;
            looseTimer = LoosePredictInterval;
            Vector3 t = ball.pos;
            BallPredictor.Simulate(ball, 2.5f, s =>
            {
                if ((s.vel.y < 0f && s.pos.y < 2.1f) || s.justBounced)
                {
                    t = s.pos;
                    return true;
                }
                return false;
            });
            looseTarget = new Vector3(t.x, 0f, t.z);
        }

        // ================= 保持中のボール =================

        void PositionHeldBall(float dt)
        {
            foreach (var p in players) p.Body.dribbling = p.Body.holdBall = false;
            if (holder == null) return;
            var h = holder;
            if (phase != Phase.Live)
            {
                h.Body.holdBall = true;
                ball.Hold(h.Pos + Vector3.up * 1.15f + h.Facing * 0.35f);
                return;
            }
            if (h.shooting)
            {
                ball.Hold(BallOverhead(h));
                return;
            }
            float speed = new Vector2(h.Velocity.x, h.Velocity.z).magnitude;
            float prev = dribblePhase;
            dribblePhase += dt * Mathf.PI * (1.6f + speed * 0.22f);
            // cos が 0 になる瞬間がボールが床に着いたとき
            if (Mathf.Floor(prev / Mathf.PI + 0.5f) != Mathf.Floor(dribblePhase / Mathf.PI + 0.5f)) AudioManager.Play(Sfx.Dribble, 0.45f);
            float height = BasketballDims.BallRadius + (0.95f - BasketballDims.BallRadius) * Mathf.Abs(Mathf.Cos(dribblePhase));
            Vector3 side = Vector3.Cross(Vector3.up, h.Facing) * (h.dribbleLeft ? -1f : 1f);
            ball.Hold(h.Pos + side * 0.32f + h.Facing * 0.3f + Vector3.up * height);
            h.Body.dribbling = true;
            h.Body.dribbleLeft = h.dribbleLeft;
            h.Body.dribblePhase = dribblePhase * 2f + Mathf.PI * 0.5f;
        }

        // ================= 終了・表示 =================

        void EndMatch()
        {
            int w = score.Winner;
            bool win = w == 0;
            players[w].Body.Play(HAction.Celebrate, 1.5f, true);
            players[1 - w].Body.Play(HAction.Dejected, 1.5f, true);
            FinishMatch(win, "GAME", players[0].Pos + Vector3.forward * 2f, () => new MatchResult
            {
                humanWon = win,
                scoreLine = $"YOU  {score.Points(0)} - {score.Points(1)}  CPU",
                stats = stats.Lines(),
            });
        }

        void UpdateNet(float dt)
        {
            if (netAnim <= 0f) return;
            netAnim = Mathf.Max(0f, netAnim - dt);
            float k = netAnim / NetAnimTime;
            net.localScale = new Vector3(1f - 0.15f * k, 1f + 0.35f * Mathf.Sin(k * Mathf.PI), 1f - 0.15f * k);
        }

        void UpdateCamera()
        {
            Vector3 focus = ballState == BallState.Held && holder != null ? holder.Pos : ball.pos;
            focus.x = Mathf.Clamp(focus.x, -6.5f, 6.5f);
            focus.z = Mathf.Clamp(focus.z, -11f, 1f);
            Rig.SetTargets(new Vector3(focus.x * 0.4f, 7.2f, Mathf.Min(-14.5f, focus.z - 9.5f)), new Vector3(focus.x * 0.55f, 1.6f, focus.z * 0.5f - 1.2f));
        }

        void UpdateHud()
        {
            presenter.ShowScore(score);
            presenter.ShowShotClock(shotClock, phase == Phase.Live);
            var human = players[0];
            var role = holder == human ? BasketballHudPresenter.Role.Offense
                : holder != null ? BasketballHudPresenter.Role.Defense : BasketballHudPresenter.Role.LooseBall;
            presenter.ShowButtons(role, !needsClear);
        }

        void UpdateHints()
        {
            if (!controllers[0].IsHuman || humanPossessions > HintPossessions) { Hud.Hint(null); return; }
            if (holder == players[0]) Hud.Hint(needsClear ? "3Pラインの外までボールを戻そう" : "シュートは長押し → ゲージが緑のときに離す！");
            else if (holder == players[1]) Hud.Hint("B でスティール / A でブロック");
            else Hud.Hint(null);
        }
    }
}
