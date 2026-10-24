using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// テニス(シングルス)の試合進行。サーブ → ラリー → 判定 → 得点の流れを管理し、
    /// ルールは TennisReferee / TennisScore、打球は TennisShotSolver、操作は ITennisController に委ねる。
    /// </summary>
    public class TennisGame : MatchBase, ITennisView
    {
        enum Phase { Intro, PreServe, Toss, Rally, Dead }

        const float IntroTime = 2.2f;
        const float PredictInterval = 0.08f;
        const float DeadBallTime = 0.8f;
        const float ServeX = 0.6f, ReceiveX = 2.3f;
        const float ServeBaselineOffset = 0.25f, ReceiveDepthOffset = 0.6f;
        const float TossSpeed = 5.2f, TossMissHeight = 1.35f;
        const int HintShots = 4;

        [SerializeField] TennisConfig config = new TennisConfig();

        readonly TennisPlayer[] players = new TennisPlayer[2];
        readonly ITennisController[] controllers = new ITennisController[2];
        readonly TennisReferee referee = new TennisReferee();
        readonly TennisStats stats = new TennisStats();
        TennisScore score;
        TennisShotSolver solver;
        TennisHudPresenter presenter;
        BallSim ball;
        BallView ballView;
        GroundMarker landingMarker;
        HudMeter serveMeter;
        Phase phase;
        float phaseTime, idleTime, predictTimer, tossApex;
        int pointServer, humanShots;

        protected override Sport Sport => Sport.Tennis;
        internal TennisScore Score => score;
        internal TennisStats Stats => stats;
        internal string PhaseName => phase.ToString();
        internal int PointServer => pointServer;

        // ---- ITennisView ----
        public BallSim Ball => ball;
        public TennisReferee Referee => referee;
        public Transform CameraTransform => Rig.transform;
        public float TossApex => tossApex;
        public TennisPlayer Opponent(TennisPlayer self) => players[1 - self.Side];

        TennisPlayer Server => players[pointServer];
        TennisPlayer Receiver => players[1 - pointServer];
        bool HumanServing => controllers[pointServer].IsHuman;

        void Start()
        {
            float skill = GameSettings.Skill;
            score = CreateScore(GameSettings.MatchLength);
            var root = new GameObject("Tennis").transform;
            var net = TennisDims.CreateNet();
            InitMatch(new MatchSetup
            {
                title = "TENNIS", homeName = "YOU", awayName = "CPU",
                buttons = new[] { new ButtonSpec("トップ\nスピン", new Color(0.95f, 0.55f, 0.15f)), new ButtonSpec("スライス", new Color(0.25f, 0.55f, 0.95f)), new ButtonSpec("ロブ", new Color(0.3f, 0.75f, 0.4f)) },
                fov = 40f, background = new Color(0.5f, 0.7f, 0.9f), skybox = true,
                lighting = new SceneLighting.Preset
                {
                    sunEuler = new Vector3(52f, -35f, 0f), sun = new Color(1f, 0.96f, 0.88f), intensity = 1.3f,
                    sky = new Color(0.6f, 0.7f, 0.85f), equator = new Color(0.45f, 0.5f, 0.5f), ground = new Color(0.25f, 0.3f, 0.25f)
                },
                crowds = TennisCourtBuilder.Build(root, net),
                ambienceVolume = 0.22f,
                warmup = new[] { Sfx.Racket, Sfx.RacketStrong, Sfx.TennisBounce, Sfx.NetHit },
            });

            ball = new BallSim { radius = TennisDims.BallRadius, restitution = 0.74f, friction = 0.78f, rollDecay = 0.8f };
            ball.colliders.Add(net);
            ball.onContact = OnBallContact;
            solver = new TennisShotSolver(config, net, ball.gravity);
            ballView = BallView.Create(root, "TennisBall", TennisDims.BallRadius, ProcTextures.Ball(Sport.Tennis), 2.4f, new Color(1f, 1f, 0.55f, 0.55f));

            var homeLook = HumanoidLook.Make(UITheme.Home, new Color(0.95f, 0.95f, 0.97f), 5);
            homeLook.accent = TeamColors.Dark(UITheme.Home);
            var awayLook = HumanoidLook.Make(UITheme.Away, new Color(0.15f, 0.15f, 0.2f), 11);
            players[0] = TennisPlayer.Create(root, 0, homeLook, "YOU", config.playerSpeed);
            players[1] = TennisPlayer.Create(root, 1, awayLook, "CPU", TennisConfig.ForSkill(config.aiSpeed, skill));
            controllers[0] = DebugOptions.AutoPlay ? (ITennisController)new TennisAIController(this, config, skill) : new TennisHumanController(this, config);
            controllers[1] = new TennisAIController(this, config, skill);

            landingMarker = GroundMarker.Create(root, "LandingMarker", 0.38f, new Color(1f, 0.92f, 0.2f, 0.9f), 0.07f);
            serveMeter = Hud.CreateMeter(new Color(1f, 0.82f, 0.2f));
            presenter = new TennisHudPresenter(Hud);

            pointServer = score.CurrentServer;
            PlacePlayers();
            HoldBallAtServer();
            Rig.Snap(new Vector3(0f, 6.5f, -24f), new Vector3(0f, 0.3f, 2f));
            phase = Phase.Intro;
        }

        static TennisScore CreateScore(int matchLength)
        {
            int first = Random.value < 0.5f ? 0 : 1;
            switch (matchLength)
            {
                case 1: return new TennisScore(6, 1, first);
                case 2: return new TennisScore(3, 2, first);
                default: return new TennisScore(3, 1, first);
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
                    if (phaseTime > IntroTime) BeginPoint();
                    break;
                case Phase.PreServe: UpdatePreServe(dt); break;
                case Phase.Toss: UpdateToss(dt); break;
                case Phase.Rally: UpdateRally(dt); break;
                case Phase.Dead:
                    ball.Step(dt);
                    Idle(dt);
                    break;
            }
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

        void BeginPoint()
        {
            referee.BeginPoint();
            pointServer = score.CurrentServer;
            PlacePlayers();
            PrepareServe();
        }

        void PrepareServe()
        {
            foreach (var p in players) p.ClearSwing();
            HoldBallAtServer();
            ballView.SetTrail(false);
            landingMarker.Hide();
            serveMeter.Hide();
            idleTime = 0f;
            SetPhase(Phase.PreServe);
        }

        /// <summary>サーバーが立つサイド(デュースサイドは本人から見て右)の x 符号。</summary>
        float ServeSideSign(TennisPlayer s) => (score.DeuceSide ? 1f : -1f) * s.Forward;

        float BaselineZ(TennisPlayer p, float offset) => -p.Forward * (TennisDims.HalfLength + offset);

        void PlacePlayers()
        {
            float sx = ServeSideSign(Server) * ServeX;
            Server.Teleport(new Vector3(sx, 0f, BaselineZ(Server, ServeBaselineOffset)));
            Receiver.Teleport(new Vector3(-Mathf.Sign(sx) * ReceiveX, 0f, BaselineZ(Receiver, ReceiveDepthOffset)));
        }

        Vector3 ServeHandPos(TennisPlayer p) => p.Pos + p.RightDir * 0.18f + p.ForwardDir * 0.32f + Vector3.up * 1.05f;

        void HoldBallAtServer() => ball.Hold(ServeHandPos(Server));

        void MoveReceiver(float dt) => Receiver.Move(controllers[Receiver.Side].PreServeMove(Receiver) * 1.2f, dt, Server.Pos);

        void UpdatePreServe(float dt)
        {
            Vector3 mv = controllers[pointServer].PreServeMove(Server);
            mv.z = 0f;
            Server.Move(mv, dt, Receiver.Pos);
            Server.ConstrainToServePosition(ServeSideSign(Server), BaselineZ(Server, ServeBaselineOffset));
            MoveReceiver(dt);
            Hud.Hint(HumanServing && humanShots < HintShots ? "ボタンでトス → ボールが一番高いときにもう一度押してサーブ！" : null);
            if (controllers[pointServer].WantsToss(Server, dt)) Toss();
            else HoldBallAtServer();
        }

        void Toss()
        {
            Vector3 start = ServeHandPos(Server) + Vector3.up * 0.35f;
            tossApex = start.y + TossSpeed * TossSpeed / (2f * ball.gravity);
            ball.Launch(start, new Vector3(0f, TossSpeed, 0f));
            Server.Body.Play(HAction.Serve, 1.1f);
            SetPhase(Phase.Toss);
        }

        void UpdateToss(float dt)
        {
            ball.Step(dt);
            Server.Move(Vector3.zero, dt, Receiver.Pos);
            MoveReceiver(dt);
            if (HumanServing) ShowServeMeter();
            if (controllers[pointServer].WantsServeHit(Server, out float quality, out float aimX))
            {
                HitServe(quality, aimX);
                return;
            }
            if (ball.vel.y < 0f && ball.pos.y < TossMissHeight)
            {
                Hud.Banner.Notice("トスやり直し", Color.white, 0.6f);
                PrepareServe();
            }
        }

        void ShowServeMeter()
        {
            float v = Mathf.InverseLerp(1.4f, tossApex, ball.pos.y);
            bool good = Mathf.Abs(v - 0.95f) < 0.07f;
            serveMeter.Show(Hud.WorldToHud(Server.Pos + Vector3.up * 1.6f + Server.RightDir, Rig.Cam), v, 0.95f, 0.14f,
                good ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.82f, 0.2f));
        }

        void HitServe(float quality, float aimX)
        {
            serveMeter.Hide();
            bool first = !referee.IsSecondServe;
            if (HumanServing && first) stats.firstServes++;
            float power = first ? Mathf.Lerp(0.35f, 1f, quality) : Mathf.Lerp(0.3f, 0.6f, quality);
            float boxSign = -Mathf.Sign(Server.Pos.x == 0f ? Server.Forward : Server.Pos.x);
            float sigma = controllers[pointServer].ServeError * (1.35f - quality) * (first ? 1.15f : 0.65f);
            Vector3 target = solver.ServeTarget(Server.Forward, boxSign, aimX, sigma);
            bool intoNet = Random.value < (first ? (1f - quality) * 0.16f : 0.03f);
            var launch = solver.Serve(ball.pos, target, first, power, intoNet);
            ball.Launch(ball.pos, launch.velocity, launch.gravityScale, launch.bounceXZ, launch.bounceY);

            referee.BeginServe(pointServer, boxSign);
            controllers[Receiver.Side].OnOpponentHit(Receiver);
            Server.Body.Play(HAction.Smash, 0.5f);
            AudioManager.Play(Sfx.RacketStrong, 1f, Random.Range(0.95f, 1.05f));
            ballView.SetTrail(first && power > 0.6f);
            if (HumanServing) humanShots++;
            idleTime = 0f;
            SetPhase(Phase.Rally);
        }

        // ================= ラリー =================

        void UpdateRally(float dt)
        {
            ball.Step(dt);
            if (phase != Phase.Rally) return;
            for (int i = 0; i < 2; i++) controllers[i].TickRally(players[i], dt);
            for (int i = 0; i < 2 && phase == Phase.Rally; i++) TryHit(players[i]);
            if (phase != Phase.Rally) return;

            UpdateLandingMarker(dt);
            idleTime = ball.OnGround ? idleTime + dt : 0f;
            bool lost = Mathf.Abs(ball.pos.x) > 26f || Mathf.Abs(ball.pos.z) > 34f;
            if (idleTime > DeadBallTime || lost) Decide(referee.JudgeDeadBall(ball.pos));

            bool hint = controllers[0].IsHuman && referee.LastHitter == 1 && humanShots < HintShots;
            Hud.Hint(hint ? "ボールが来る前にボタンを押そう！  スティックの向きでコースを狙える" : null);
        }

        void UpdateLandingMarker(float dt)
        {
            predictTimer -= dt;
            if (predictTimer > 0f) return;
            predictTimer = PredictInterval;
            if (referee.LastHitter == 1 && referee.BouncesSinceHit == 0 && BallPredictor.FirstBounce(ball, 3f, out var p, out _))
                landingMarker.Show(p);
            else landingMarker.Hide();
        }

        void TryHit(TennisPlayer p)
        {
            if (!p.Swing.active || !referee.CanHit(p.Side)) return;
            if (ball.pos.z * p.Forward > 0.25f) return;
            var g = p.GeometryTo(ball.pos);
            if (TennisPlayer.InReach(g) && TennisPlayer.IsContactMoment(g)) ExecuteHit(p, g);
        }

        void ExecuteHit(TennisPlayer p, HitGeometry g)
        {
            var req = controllers[p.Side].DecideShot(p, g);
            TShot shot = req.shot;
            if (g.height > 2.05f && shot != TShot.Lob) shot = TShot.Smash;
            else if (referee.BouncesSinceHit == 0 && (shot == TShot.Topspin || shot == TShot.Slice)) shot = TShot.Volley;

            float q = g.Quality;
            float incoming = new Vector2(ball.vel.x, ball.vel.z).magnitude;
            float runPenalty = p.Velocity.magnitude > 4f ? 1.25f : 1f;
            float sigma = req.error * (1.25f - q) * (0.8f + req.power * 0.45f) * runPenalty * (0.8f + incoming / 60f);
            Vector3 start = ball.pos;
            Vector3 target = solver.RallyTarget(p.Forward, shot, req.aim, sigma);
            var launch = solver.Rally(start, target, shot, req.power, Random.value < req.netChance);
            ball.Launch(start, launch.velocity, launch.gravityScale, launch.bounceXZ, launch.bounceY);

            referee.RegisterHit(p.Side);
            stats.longestRally = Mathf.Max(stats.longestRally, referee.RallyHits);
            p.ClearSwing();
            var opp = Opponent(p);
            controllers[opp.Side].OnOpponentHit(opp);
            idleTime = 0f;
            predictTimer = 0f;
            if (controllers[p.Side].IsHuman) humanShots++;

            bool strong = req.power > 0.7f || shot == TShot.Smash;
            if (shot == TShot.Smash)
            {
                p.Body.Play(HAction.Smash, 0.5f);
                Rig.Shake(0.12f, 0.25f);
                SlowMotion(0.4f, 0.3f);
                Fx.Burst(start, new Color(1f, 1f, 0.6f), 12, 4f, 0.06f);
            }
            else p.Body.Play(g.lateral >= 0f ? HAction.Forehand : HAction.Backhand, 0.42f);
            AudioManager.Play(strong ? Sfx.RacketStrong : Sfx.Racket, 0.9f, Random.Range(0.95f, 1.08f));
            ballView.SetTrail(strong);
        }

        // ================= 判定・得点 =================

        void OnBallContact(BallContact c)
        {
            switch (c.kind)
            {
                case ContactKind.Ground:
                    if (c.speed > 0.6f) AudioManager.Play(Sfx.TennisBounce, Mathf.Clamp01(c.speed / 8f) * 0.7f + 0.3f);
                    if (phase == Phase.Rally) Decide(referee.RegisterBounce(c.point));
                    break;
                case ContactKind.Net:
                case ContactKind.NetTop:
                    AudioManager.Play(Sfx.NetHit, c.kind == ContactKind.Net ? 0.8f : 0.5f, c.kind == ContactKind.Net ? 1f : 1.3f);
                    referee.RegisterNetTouch();
                    break;
            }
        }

        void Decide(RallyDecision d)
        {
            switch (d.call)
            {
                case RallyCall.ServeGood:
                    if (HumanServing && referee.Faults == 0) stats.firstServesIn++;
                    break;
                case RallyCall.Fault:
                    SetPhase(Phase.Dead);
                    Hud.Banner.Show("FAULT", new Color(1f, 0.6f, 0.4f), 0.9f, "セカンドサーブ");
                    AudioManager.Play(Sfx.Groan, 0.4f);
                    After(1.5f, () =>
                    {
                        PlacePlayers();
                        PrepareServe();
                    });
                    break;
                case RallyCall.Let:
                    SetPhase(Phase.Dead);
                    Hud.Banner.Show("LET", Color.white, 0.9f, "サーブやり直し");
                    After(1.4f, PrepareServe);
                    break;
                case RallyCall.Point:
                    AwardPoint(d.winner, d.reason);
                    break;
            }
        }

        void AwardPoint(int winner, PointReason reason)
        {
            SetPhase(Phase.Dead);
            landingMarker.Hide();
            ballView.SetTrail(false);
            Hud.Hint(null);
            stats.RecordPoint(winner, reason);
            GameLog.Info("Tennis", $"point -> {TennisHudPresenter.Name(winner)} ({reason}) rally={referee.RallyHits} score={score.Summary()}");
            Hud.Banner.Show(TennisHudPresenter.ReasonText(reason), TennisHudPresenter.PointColor(winner), 1.1f, TennisHudPresenter.Name(winner) + " のポイント");
            PointFeedback(winner == 0, reason == PointReason.Ace || reason == PointReason.Winner);
            players[winner].Body.Play(HAction.Celebrate, 1.2f);
            After(1.5f, () => ApplyScore(winner));
        }

        void ApplyScore(int winner)
        {
            var evt = score.AwardPoint(winner);
            if (evt == TennisScoreEvent.Match)
            {
                EndMatch();
                return;
            }
            if (evt != TennisScoreEvent.None)
            {
                Hud.Banner.Show(TennisHudPresenter.EventText(evt, winner), UITheme.Gold, 1.2f, score.Summary());
                Hud.Scoreboard.Punch(winner);
                AudioManager.Play(Sfx.Point, 0.7f);
                After(1.6f, BeginPoint);
                return;
            }
            string call = TennisHudPresenter.CallText(score);
            if (call != null) Hud.Banner.Notice(call, Color.white, 0.8f);
            After(0.6f, BeginPoint);
        }

        void EndMatch()
        {
            bool win = score.Winner == 0;
            players[score.Winner].Body.Play(HAction.Celebrate, 1.5f, true);
            players[1 - score.Winner].Body.Play(HAction.Dejected, 1.5f, true);
            FinishMatch(win, "GAME SET", players[0].Pos + Vector3.forward * 2f, () => new MatchResult
            {
                humanWon = win,
                scoreLine = "YOU  " + score.Summary() + "  CPU",
                stats = stats.Lines(),
            });
        }

        // ================= 表示 =================

        void UpdateCamera()
        {
            var h = players[0];
            Vector3 focus = Vector3.Lerp(h.Pos, ball.pos, 0.35f);
            float back = Mathf.Clamp(h.Pos.z + 12f, -4f, 6f) * 0.3f;
            Rig.SetTargets(new Vector3(focus.x * 0.35f, 6.3f, -23.5f + back), new Vector3(focus.x * 0.3f, 0.2f, 1.5f + back * 0.5f));
        }

        void UpdateHud()
        {
            presenter.ShowScore(score, !MatchOver);
            bool serving = HumanServing && (phase == Phase.PreServe || phase == Phase.Toss);
            if (serving) presenter.ShowServeButtons(phase == Phase.Toss);
            else presenter.ShowRallyButtons(phase == Phase.Rally ? players[0].Swing : default);
        }
    }
}
