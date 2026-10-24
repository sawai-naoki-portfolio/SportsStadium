using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// ビーチバレー 2on2 の試合進行。サーブ → レシーブ → トス → アタック(・ブロック)の流れを管理する。
    /// 誰が触るかは VolleyballPlanner、ルールは VolleyballReferee / VolleyballScore、操作は IVolleyballController に委ねる。
    /// </summary>
    public class VolleyballGame : MatchBase, IVolleyballView
    {
        enum Phase { Intro, PreServe, Toss, Rally, Dead }

        const int HumanIndex = 0;
        const float IntroTime = 2.2f;
        const float PlanInterval = 0.12f, MarkerInterval = 0.1f;
        const float TossSpeed = 4.6f, TossMissHeight = 1.2f;
        const float ServeDepth = 8.8f, ServeX = 1.2f, ServeLineMaxX = 3.5f;
        const float ReceiveDepth = 5.5f, ReceiveX = 2f, PartnerDepth = 3f;
        const float BlockReachX = 0.75f, BlockZone = 0.6f;
        const float DiveDistance = 0.8f;
        const int HintTouches = 6;

        [SerializeField] VolleyballConfig config = new VolleyballConfig();

        readonly VolleyballPlayer[] players = new VolleyballPlayer[4];
        readonly IVolleyballController[] controllers = new IVolleyballController[4];
        readonly VolleyballPlan[] plans = new VolleyballPlan[2];
        readonly VolleyballReferee referee = new VolleyballReferee();
        readonly VolleyballStats stats = new VolleyballStats();
        VolleyballScore score;
        VolleyballShots shots;
        VolleyballHudPresenter presenter;
        BallSim ball;
        BallView ballView;
        GroundMarker landingMarker, targetMarker;
        HudMeter serveMeter;
        Phase phase;
        float phaseTime, planTimer, markerTimer, tossApex;
        int rallyTouches, lastAttacker = -1, humanTouches;
        bool lastWasSpike;

        protected override Sport Sport => Sport.Volleyball;
        internal VolleyballScore Score => score;
        internal VolleyballStats Stats => stats;
        internal string PhaseName => phase.ToString();

        // ---- IVolleyballView ----
        public BallSim Ball => ball;
        public VolleyballReferee Referee => referee;
        public Transform CameraTransform => Rig.transform;
        public float TossApex => tossApex;
        public VolleyballPlan PlanFor(int team) => plans[team];
        public VolleyballPlayer Partner(VolleyballPlayer self) => players[self.Index ^ 1];
        public bool IsHumanControlled(VolleyballPlayer p) => controllers[p.Index].IsHuman;

        VolleyballPlayer Server => players[score.ServingTeam * 2 + score.ServerSlot];
        bool HumanServing => Server.Index == HumanIndex && controllers[HumanIndex].IsHuman;

        void Start()
        {
            float skill = GameSettings.Skill;
            float partnerSkill = Mathf.Max(skill, config.partnerMinSkill);
            score = CreateScore(GameSettings.MatchLength);
            var root = new GameObject("Volleyball").transform;
            var net = VolleyballDims.CreateNet();
            InitMatch(new MatchSetup
            {
                title = "BEACH VOLLEYBALL", homeName = "YOU", awayName = "CPU",
                buttons = new[] { new ButtonSpec("レシーブ", new Color(0.95f, 0.55f, 0.15f)), new ButtonSpec("ジャンプ", new Color(0.25f, 0.55f, 0.95f)) },
                fov = 46f, background = new Color(0.55f, 0.75f, 0.95f), skybox = true,
                lighting = new SceneLighting.Preset
                {
                    sunEuler = new Vector3(48f, -40f, 0f), sun = new Color(1f, 0.95f, 0.85f), intensity = 1.35f,
                    sky = new Color(0.65f, 0.75f, 0.9f), equator = new Color(0.6f, 0.58f, 0.5f), ground = new Color(0.45f, 0.4f, 0.3f)
                },
                crowds = VolleyballCourtBuilder.Build(root, net),
                ambienceVolume = 0.25f,
                warmup = new[] { Sfx.Bump, Sfx.Spike, Sfx.SandHit, Sfx.NetHit },
            });

            ball = new BallSim { radius = VolleyballDims.BallRadius, restitution = 0.35f, friction = 0.5f, rollDecay = 3f };
            ball.colliders.Add(net);
            ball.onContact = OnBallContact;
            shots = new VolleyballShots(config, net, ball.gravity);
            ballView = BallView.Create(root, "Volleyball", VolleyballDims.BallRadius, ProcTextures.Ball(Sport.Volleyball), 1.6f, new Color(1f, 1f, 1f, 0.5f));

            float awaySpeed = VolleyballConfig.ForSkill(config.aiSpeed, skill), partnerSpeed = VolleyballConfig.ForSkill(config.aiSpeed, partnerSkill);
            string[] names = { "YOU", "パートナー", "CPU1", "CPU2" };
            for (int i = 0; i < 4; i++)
            {
                bool home = i < 2;
                var look = HumanoidLook.Make(home ? UITheme.Home : UITheme.Away, home ? TeamColors.Dark(UITheme.Home) : new Color(0.15f, 0.15f, 0.2f), 40 + i);
                float speed = i == HumanIndex ? config.playerSpeed : home ? partnerSpeed : awaySpeed;
                players[i] = VolleyballPlayer.Create(root, i, look, names[i], speed);
                controllers[i] = i == HumanIndex && !DebugOptions.AutoPlay
                    ? (IVolleyballController)new VolleyballHumanController(this, config)
                    : new VolleyballAIController(this, config, home ? partnerSkill : skill);
            }

            landingMarker = GroundMarker.Create(root, "LandingMarker", 0.45f, new Color(1f, 0.92f, 0.2f, 0.9f), 0.08f);
            targetMarker = GroundMarker.Create(root, "TargetMarker", 0.6f, new Color(0.3f, 1f, 0.45f, 0.9f), 0.1f);
            serveMeter = Hud.CreateMeter(new Color(1f, 0.82f, 0.2f));
            presenter = new VolleyballHudPresenter(Hud);

            PlaceForServe();
            HoldBallAtServer();
            Rig.Snap(new Vector3(0f, 8f, -19f), new Vector3(0f, 1.4f, 0.8f));
            phase = Phase.Intro;
        }

        static VolleyballScore CreateScore(int matchLength)
        {
            int first = Random.value < 0.5f ? 0 : 1;
            switch (matchLength)
            {
                case 1: return new VolleyballScore(21, 1, 15, first);
                case 2: return new VolleyballScore(21, 2, 15, first);
                default: return new VolleyballScore(15, 1, 15, first);
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
                    Idle(dt);
                    HoldBallAtServer();
                    if (phaseTime > IntroTime) BeginRally();
                    break;
                case Phase.PreServe: UpdatePreServe(dt); break;
                case Phase.Toss: UpdateToss(dt); break;
                case Phase.Rally: UpdateRally(dt); break;
                case Phase.Dead:
                    ball.Step(dt);
                    Idle(dt);
                    break;
            }
            foreach (var p in players) p.Tick(dt);
            ballView.Sync(ball.pos);
            UpdateCamera();
            UpdateHud();
        }

        void SetPhase(Phase p)
        {
            phase = p;
            phaseTime = 0f;
        }

        void Idle(float dt)
        {
            foreach (var p in players) p.Move(Vector3.zero, dt, ball.pos);
        }

        // ================= サーブ =================

        void BeginRally()
        {
            plans[0] = plans[1] = default;
            rallyTouches = 0;
            lastAttacker = -1;
            lastWasSpike = false;
            PlaceForServe();
            PrepareServe();
        }

        void PlaceForServe()
        {
            var s = Server;
            float f = s.Forward;
            s.Teleport(new Vector3(ServeX, 0f, -f * ServeDepth));
            Partner(s).Teleport(new Vector3(-ServeX, 0f, -f * PartnerDepth));
            int rt = 1 - s.Team;
            float fr = VolleyballDims.Forward(rt);
            players[rt * 2].Teleport(new Vector3(-ReceiveX, 0f, -fr * ReceiveDepth));
            players[rt * 2 + 1].Teleport(new Vector3(ReceiveX, 0f, -fr * ReceiveDepth));
        }

        void PrepareServe()
        {
            foreach (var p in players) p.ClearIntent();
            HoldBallAtServer();
            ballView.SetTrail(false);
            landingMarker.Hide();
            targetMarker.Hide();
            serveMeter.Hide();
            SetPhase(Phase.PreServe);
        }

        Vector3 ServeHand(VolleyballPlayer s) => s.Pos + s.ForwardDir * 0.35f + Vector3.up * 1.1f + new Vector3(0.15f, 0f, 0f);

        void HoldBallAtServer() => ball.Hold(ServeHand(Server));

        void UpdatePreServe(float dt)
        {
            var s = Server;
            Vector3 mv = controllers[s.Index].PreServeMove(s);
            mv.z = 0f;
            foreach (var p in players) p.Move(p == s ? mv : Vector3.zero, dt, ball.pos);
            s.ConstrainToServeLine(-s.Forward * ServeDepth, ServeLineMaxX);
            Hud.Hint(HumanServing && humanTouches < HintTouches ? "A でトス → ボールが一番高いときにもう一度 A でサーブ！" : null);
            if (controllers[s.Index].WantsToss(s, dt)) Toss();
            else HoldBallAtServer();
        }

        void Toss()
        {
            var s = Server;
            Vector3 start = ServeHand(s) + Vector3.up * 0.3f;
            tossApex = start.y + TossSpeed * TossSpeed / (2f * ball.gravity);
            ball.Launch(start, new Vector3(0f, TossSpeed, 0f));
            s.Body.Play(HAction.Spike, 1f);
            SetPhase(Phase.Toss);
        }

        void UpdateToss(float dt)
        {
            ball.Step(dt);
            Idle(dt);
            var s = Server;
            if (HumanServing)
            {
                float v = Mathf.InverseLerp(1.4f, tossApex, ball.pos.y);
                bool good = Mathf.Abs(v - 0.95f) < 0.08f;
                serveMeter.Show(Hud.WorldToHud(s.Pos + Vector3.up * 1.6f + Vector3.right, Rig.Cam), v, 0.95f, 0.16f, good ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.82f, 0.2f));
            }
            if (controllers[s.Index].WantsServeHit(s, out float quality, out Vector2 aim))
            {
                HitServe(s, quality, aim);
                return;
            }
            if (ball.vel.y < 0f && ball.pos.y < TossMissHeight)
            {
                Hud.Banner.Notice("トスやり直し", Color.white, 0.6f);
                PrepareServe();
            }
        }

        void HitServe(VolleyballPlayer s, float quality, Vector2 aim)
        {
            serveMeter.Hide();
            float sigma = controllers[s.Index].ServeError * (1.3f - quality);
            Vector3 target = VolleyballShots.CourtTarget(s.Team, aim, sigma);
            ball.Launch(ball.pos, shots.Serve(ball.pos, target, Mathf.Lerp(0.3f, 1f, quality)));
            referee.BeginServe(s.Team, s.Index);
            rallyTouches = 1;
            s.Body.Play(HAction.Spike, 0.5f);
            AudioManager.Play(Sfx.Spike, 0.6f, 1.1f);
            if (controllers[s.Index].IsHuman) humanTouches++;
            planTimer = 0f;
            SetPhase(Phase.Rally);
        }

        // ================= ラリー =================

        void UpdateRally(float dt)
        {
            float prevZ = ball.pos.z;
            ball.Step(dt);
            if (phase != Phase.Rally) return;
            if (prevZ != 0f && Mathf.Sign(prevZ) != Mathf.Sign(ball.pos.z) && !CheckNetCrossing()) return;

            planTimer -= dt;
            if (planTimer <= 0f) RecomputePlans();
            for (int i = 0; i < players.Length; i++) controllers[i].TickRally(players[i], dt);
            for (int i = 0; i < players.Length && phase == Phase.Rally; i++) TryTouch(players[i]);
            if (phase != Phase.Rally) return;

            if (Mathf.Abs(ball.pos.z) > 30f || Mathf.Abs(ball.pos.x) > 30f) EndRally(referee.RegisterGround(ball.pos));
            UpdateMarkers(dt);
            UpdateRallyHint();
        }

        /// <summary>ネットを越えたときの判定。ラリーが続くなら true。</summary>
        bool CheckNetCrossing()
        {
            var d = referee.RegisterNetCrossing(ball.pos);
            if (d.point)
            {
                EndRally(d);
                return false;
            }
            int receiving = VolleyballDims.SideOf(ball.pos);
            for (int i = receiving * 2; i < receiving * 2 + 2; i++) controllers[i].OnBallCrossed(players[i]);
            planTimer = 0f;
            return true;
        }

        void RecomputePlans()
        {
            planTimer = PlanInterval;
            for (int t = 0; t < 2; t++) plans[t] = VolleyballPlanner.Compute(ball, referee, players, t, config);
        }

        void TryTouch(VolleyballPlayer p)
        {
            Vector3 b = ball.pos;
            if (p.Airborne && lastWasSpike && referee.LastTeam != p.Team && CanBlock(p, b))
            {
                Block(p);
                return;
            }
            if (VolleyballDims.SideOf(b) != p.Team) return;
            if (referee.LastTeam == p.Team && referee.LastPlayer == p.Index && !referee.IsServe) return;
            int index = referee.TouchIndexFor(p.Team);
            if (index > 2) return;

            if (p.Intent == VbIntent.Spike && p.Airborne && Vector3.Distance(b, p.HandPoint) < config.spikeReach)
            {
                Attack(p, true);
                return;
            }
            float reach = config.passReach + (controllers[p.Index].IsHuman && GameSettings.MoveAssist ? config.assistReachBonus : 0f);
            float horiz = new Vector2(b.x - p.Pos.x, b.z - p.Pos.z).magnitude;
            bool inZone = horiz < reach && b.y > 0.15f && b.y < 2.9f && (ball.vel.y < 0f || b.y < 1.2f);
            if (p.Intent != VbIntent.Pass || !inZone) return;
            float quality = 1f - Mathf.Clamp01(horiz / reach) * 0.6f;
            if (index == 2) Attack(p, false);
            else if (index == 1) SetBall(p, quality);
            else Receive(p, quality, horiz);
        }

        bool CanBlock(VolleyballPlayer p, Vector3 b)
            => Mathf.Abs(p.Pos.z) < 1.3f && Mathf.Abs(b.z) < BlockZone && Mathf.Abs(b.x - p.Pos.x) < BlockReachX
               && b.y > VolleyballDims.NetTop - 0.1f && b.y < p.HandPoint.y + 0.25f && ball.vel.z * p.Forward < 0f;

        /// <summary>タッチを審判へ伝える。反則で終わったら false。</summary>
        bool RegisterTouch(VolleyballPlayer p, bool block)
        {
            var d = referee.RegisterTouch(p.Team, p.Index, block);
            rallyTouches++;
            stats.longestRally = Mathf.Max(stats.longestRally, rallyTouches);
            p.ClearIntent();
            planTimer = 0f;
            if (controllers[p.Index].IsHuman) humanTouches++;
            if (!d.point) return true;
            EndRally(d);
            return false;
        }

        void Receive(VolleyballPlayer p, float quality, float horiz)
        {
            bool dig = lastWasSpike && referee.LastTeam != p.Team;
            float incoming = ball.vel.magnitude;
            Vector3 v = shots.Receive(ball.pos, VolleyballShots.SetSpot(Partner(p)), quality, controllers[p.Index].PassError, incoming);
            if (!RegisterTouch(p, false)) return;
            ball.Launch(ball.pos, v);
            p.Body.Play(horiz > DiveDistance ? HAction.Dive : HAction.Bump, 0.5f);
            AudioManager.Play(Sfx.Bump, 0.8f);
            if (dig)
            {
                stats.digs[p.Team]++;
                if (p.Team == 0) Hud.Banner.Notice("ナイスレシーブ!", new Color(0.4f, 1f, 0.6f), 0.6f);
            }
            lastWasSpike = false;
            ballView.SetTrail(false);
        }

        void SetBall(VolleyballPlayer p, float quality)
        {
            Vector3 v = shots.Set(ball.pos, shots.AttackPoint(Partner(p)), quality, controllers[p.Index].PassError);
            if (!RegisterTouch(p, false)) return;
            ball.Launch(ball.pos, v);
            p.Body.Play(HAction.Set, 0.5f);
            AudioManager.Play(Sfx.Bump, 0.6f, 1.25f);
            lastWasSpike = false;
        }

        void Attack(VolleyballPlayer p, bool jumping)
        {
            var c = controllers[p.Index];
            Vector2 aim = c.AttackAim(p, out bool feint);
            Vector3 start = ball.pos;
            Vector3 v;
            bool spike = jumping && !feint;
            if (spike)
            {
                float quality = 1f - Mathf.Clamp01(Vector3.Distance(start, p.HandPoint) / config.spikeReach);
                Vector3 target = VolleyballShots.CourtTarget(p.Team, aim, c.SpikeError * (1.2f - quality));
                bool intoNet = Random.value < (quality < 0.25f ? 0.4f : 0.04f);
                v = shots.Spike(start, target, c.SpikePower(quality), intoNet);
            }
            else
            {
                // フェイント(ネット際へ軽く)またはつなぎ(深く山なり)
                Vector3 target = VolleyballShots.CourtTarget(p.Team, new Vector2(aim.x, jumping ? -1f : 0.4f), 0.6f);
                v = shots.Lob(start, target, jumping ? VolleyballDims.NetTop + 1.2f : config.freeBallApex);
            }
            if (!RegisterTouch(p, false)) return;
            ball.Launch(start, v);
            lastWasSpike = spike;
            lastAttacker = p.Index;
            if (spike)
            {
                if (p.Index == HumanIndex) stats.humanSpikes++;
                p.Body.Play(HAction.Spike, 0.45f);
                AudioManager.Play(Sfx.Spike, 1f);
                Rig.Shake(0.12f, 0.2f);
                if (p.Index == HumanIndex) SlowMotion(0.4f, 0.25f);
                ballView.SetTrail(true);
            }
            else
            {
                p.Body.Play(jumping ? HAction.Spike : HAction.Bump, 0.45f);
                AudioManager.Play(Sfx.Bump, 0.7f, 1.1f);
            }
        }

        void Block(VolleyballPlayer p)
        {
            bool kill = Random.value < 0.6f;
            Vector3 v = VolleyballShots.BlockDeflection(ball.vel, p.Team, kill);
            if (!RegisterTouch(p, true)) return;
            ball.Launch(ball.pos, v);
            lastWasSpike = false;
            ballView.SetTrail(false);
            AudioManager.Play(Sfx.Spike, 0.7f, 0.8f);
            Rig.Shake(0.1f, 0.2f);
            p.Body.Play(HAction.Block, 0.6f);
        }

        void OnBallContact(BallContact c)
        {
            switch (c.kind)
            {
                case ContactKind.Ground:
                    AudioManager.Play(Sfx.SandHit, Mathf.Clamp01(c.speed / 8f) * 0.7f + 0.3f);
                    if (phase == Phase.Rally) EndRally(referee.RegisterGround(c.point));
                    break;
                case ContactKind.Net:
                case ContactKind.NetTop:
                    AudioManager.Play(Sfx.NetHit, 0.7f);
                    break;
            }
        }

        void EndRally(VolleyballDecision d)
        {
            if (!d.point || phase != Phase.Rally) return;
            SetPhase(Phase.Dead);
            landingMarker.Hide();
            targetMarker.Hide();
            ballView.SetTrail(false);
            Hud.Hint(null);
            stats.RecordPoint(d.winner, d.reason);
            if (d.reason == RallyEnd.Kill && d.winner == 0 && lastAttacker == HumanIndex) stats.humanKills++;
            GameLog.Info("Volleyball", $"point -> {VolleyballHudPresenter.Name(d.winner)} ({d.reason}) touches={rallyTouches} score={score.Summary()}");
            AudioManager.Play(Sfx.Whistle, 0.6f);
            Hud.Banner.Show(VolleyballHudPresenter.ReasonText(d.reason), VolleyballHudPresenter.PointColor(d.winner), 1.1f, VolleyballHudPresenter.Name(d.winner) + " のポイント");
            PointFeedback(d.winner == 0, d.reason == RallyEnd.Kill || d.reason == RallyEnd.Ace || d.reason == RallyEnd.Blocked);
            for (int i = d.winner * 2; i < d.winner * 2 + 2; i++) players[i].Body.Play(HAction.Celebrate, 1.2f);
            After(1.6f, () => ApplyScore(d.winner));
        }

        void ApplyScore(int winner)
        {
            var e = score.AwardRally(winner);
            Hud.Scoreboard.Punch(winner);
            if (e == VolleyballScoreEvent.Match)
            {
                EndMatch();
                return;
            }
            if (e == VolleyballScoreEvent.Set)
            {
                Hud.Banner.Show("SET " + VolleyballHudPresenter.Name(winner), UITheme.Gold, 1.4f, score.Summary());
                AudioManager.Play(Sfx.Point, 0.7f);
                After(2f, BeginRally);
                return;
            }
            After(0.5f, BeginRally);
        }

        void EndMatch()
        {
            bool win = score.Winner == 0;
            for (int i = 0; i < players.Length; i++)
                players[i].Body.Play(players[i].Team == score.Winner ? HAction.Celebrate : HAction.Dejected, 1.5f, true);
            FinishMatch(win, "MATCH", players[HumanIndex].Pos + Vector3.forward * 2f, () => new MatchResult
            {
                humanWon = win,
                scoreLine = "YOU  " + score.Summary() + "  CPU",
                stats = stats.Lines(),
            });
        }

        // ================= 表示 =================

        void UpdateMarkers(float dt)
        {
            markerTimer -= dt;
            if (markerTimer > 0f) return;
            markerTimer = MarkerInterval;
            var mine = plans[0];
            if (mine.valid && referee.TouchIndexFor(0) == 0) landingMarker.Show(mine.landing);
            else landingMarker.Hide();
            if (mine.valid && mine.toucher == HumanIndex && controllers[HumanIndex].IsHuman) targetMarker.Show(mine.spot);
            else targetMarker.Hide();
        }

        void UpdateRallyHint()
        {
            var mine = plans[0];
            if (!controllers[HumanIndex].IsHuman || !mine.valid || mine.toucher != HumanIndex)
            {
                Hud.Hint(null);
                return;
            }
            if (mine.touch == VbTouch.Attack)
            {
                float lead = mine.contactTime - VolleyballPlayer.RiseTime(config.jumpSpeed);
                Hud.Hint(Mathf.Abs(lead) < 0.12f ? "今だ！ B でジャンプ！" : "トスに合わせて B でジャンプ(A でフェイント)");
            }
            else Hud.Hint(humanTouches < HintTouches ? "緑のリングへ移動して A ボタン！" : null);
        }

        void UpdateCamera()
        {
            Rig.SetTargets(new Vector3(ball.pos.x * 0.3f, 7.8f, -17.5f), new Vector3(ball.pos.x * 0.35f, 1.4f, 0.8f));
        }

        void UpdateHud()
        {
            presenter.ShowScore(score, !MatchOver);
            if (HumanServing && (phase == Phase.PreServe || phase == Phase.Toss)) presenter.ShowServeButtons(phase == Phase.Toss);
            else presenter.ShowRallyButtons(plans[0], plans[1], HumanIndex);
        }
    }
}
