// NIGHT30 builder B (lantern30; DESIGN30 C1 + I; GG 2026-10-09: "ideally the first boss arena would feel better if it were circular so you could continuously circle around it",
// "explaining how the boss and its mechanics work in the first level, including guidance like 'get above the lava'"). THE LANTERN on the RING (Lantern30Arena), a wrapper on
// Lantern29 (which keeps the floor cycle: rest -> tell -> live) and Lantern27 (the orbs and phases), both untouched with the row off:
//   THE HIT RULE   full damage only from the WALL while the core is open (Lantern29's rule), light damage elsewhere, and NONE from a platform (DESIGN30 I-3: a refuge, not a firing spot).
//   LEAD           the fan orbs turn, the step they fire, to the player's position leadSeconds ahead (position + velocity x leadSeconds): circling at a steady speed is not safe.
//   SWEEP          every sweepEvery-th live phase one panel sweepAheadPanels ahead of the rider is marked (sweepTellSeconds) and then burns (sweepBurnSeconds): the rider jumps the seam
//                  or drops to a platform. Event "lanternSweep" (stage "tell" / "burn" / "end", the panel) for the view's red-orange panel.
//   TEACHING       the first floor tell shows lineTell ("GET ABOVE THE LAVA"), the first core opening lineCore ("RIDE THE WALL. SHOOT THE CORE."), each at most lineMaxShows times per
//                  fight: event "bossLine30" (BossLine30) and LineNow / LineKey for the Lesson / boss-line code.
// The orbs die at the ring's wall (the old square courtyard's limit sits outside the ring near the diagonals). Fixed 120 Hz, timers in whole steps, no randomness, no audio, no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The payload of "bossLine30": a boss's teaching line (the Lesson / boss-line view shows `text` for `seconds`).</summary>
    public sealed class BossLine30
    {
        public string boss, key, text;
        public double seconds;
        public int show; // 1 for the first time this fight, 2 for the second
    }

    /// <summary>The payload of "lanternSweep": the marked / burning panel (arena-local index, Lantern30Arena.PanelAt).</summary>
    public sealed class LanternSweep30
    {
        public string stage; // "tell", "burn", "end"
        public int panel;
        public double seconds;
    }

    /// <summary>The payload of "lanternLob": the arcing orb at a platform camper ("tell": its landing spot is marked; "fire": it is thrown).</summary>
    public sealed class LanternLob30
    {
        public string stage;
        public Vec3 target;
        public double seconds;
    }

    /// <summary>design.n30.lantern30 (every number Unity-only; each has its _why).</summary>
    public sealed class Lantern30Params
    {
        public double tellSeconds, leadSeconds, sweepTellSeconds, sweepBurnSeconds, sweepDamagePerSecond, sweepReach, platformScale, lineSeconds, floorScale;
        public double platformOrbSeconds, platformOrbTellSeconds, platformOrbFlightSeconds, platformOrbGravity;
        public int sweepEvery, sweepAheadPanels, lineMaxShows;
        public string lineTell, lineCore;

        public static Lantern30Params From(GameConfig c)
        {
            const string g = "design.n30.lantern30.";
            return new Lantern30Params
            {
                tellSeconds = c.Num(g + "tellSeconds"), leadSeconds = c.Num(g + "leadSeconds"), sweepEvery = c.Int(g + "sweepEvery"), sweepAheadPanels = c.Int(g + "sweepAheadPanels"),
                sweepTellSeconds = c.Num(g + "sweepTellSeconds"), sweepBurnSeconds = c.Num(g + "sweepBurnSeconds"), sweepDamagePerSecond = c.Num(g + "sweepDamagePerSecond"), sweepReach = c.Num(g + "sweepReach"),
                platformScale = c.Num(g + "platformScale"), lineSeconds = c.Num(g + "lineSeconds"), lineMaxShows = c.Int(g + "lineMaxShows"),
                lineTell = c.Str(g + "lineTell"), lineCore = c.Str(g + "lineCore"),
                platformOrbSeconds = c.Num(g + "platformOrbSeconds"), platformOrbTellSeconds = c.Num(g + "platformOrbTellSeconds"),
                platformOrbFlightSeconds = c.Num(g + "platformOrbFlightSeconds"), platformOrbGravity = c.Num(g + "platformOrbGravity"), floorScale = c.Num(g + "floorScale"),
            };
        }
    }

    public sealed class Lantern30 : ILanternRule29
    {
        public readonly Lantern30Params p;
        public readonly Lantern30Arena arena;
        public readonly Lantern27 inner;
        public readonly Lantern29 rule;
        readonly EventBus events;
        readonly BossTarget target;
        readonly double dt0;
        readonly int tickSteps, sweepTellSteps, sweepBurnSteps, lineSteps;
        readonly LanternSweep30 sweepSaid = new LanternSweep30();
        readonly BossLine30 lineSaid = new BossLine30();

        /// <summary>The sim hands these in: is the player wall riding, and its velocity (the lead and the sweep's direction).</summary>
        public Func<bool> playerRiding;
        public Func<Vec3> playerVelocity;

        Lantern29Stage lastStage = Lantern29Stage.Rest;
        int liveCount, tellShows, coreShows, sweepTell, sweepBurn, sweepTick, lineLeft, dirSign = 1;
        int lobWait, lobTell;
        readonly int lobWaitSteps, lobTellSteps;
        readonly LanternLob30 lobSaid = new LanternLob30();
        /// <summary>The arcing orb's landing spot is marked (the view draws it red-orange) and where (world).</summary>
        public bool LobTelling => lobTell > 0;
        public Vec3 LobTarget { get; private set; }
        /// <summary>0..1 through the lob's tell.</summary>
        public double LobTellProgress => lobTell > 0 ? 1.0 - (double)lobTell / lobTellSteps : 0;
        public int lobs;

        /// <summary>The panel the sweep marks or burns (-1: none).</summary>
        public int SweepPanel { get; private set; } = -1;
        /// <summary>The sweep's mark is on (the tell): the view paints the panel red-orange.</summary>
        public bool SweepTelling => sweepTell > 0;
        /// <summary>The marked panel burns now.</summary>
        public bool SweepBurning => sweepBurn > 0;
        /// <summary>0..1 through the sweep's tell (the view's fill).</summary>
        public double SweepTellProgress => sweepTell > 0 ? 1.0 - (double)sweepTell / sweepTellSteps : 0;
        /// <summary>The teaching line up now (null: none) and its key ("tell" / "core").</summary>
        public string LineNow { get; private set; }
        public string LineKey { get; private set; }
        /// <summary>The floor's tell is on: the view glows the vents (Lantern30Arena.Vent).</summary>
        public bool VentsGlow => rule.Stage == Lantern29Stage.Tell;

        // the harness's counters (this fight)
        public int platformHits, sweeps, sweepTicks, ledFans, culled;
        public double sweepDamage;

        public Lantern30(GameConfig cfg, Lantern27 inner, Lantern29 rule, Lantern30Arena arena, EventBus events, BossTarget target)
        {
            p = Lantern30Params.From(cfg);
            this.inner = inner;
            this.rule = rule;
            this.arena = arena;
            this.events = events;
            this.target = target;
            dt0 = cfg.Dt;
            tickSteps = Math.Max(1, S(cfg.Num("design.n27.hazardFloor27.tickSeconds")));
            sweepTellSteps = S(p.sweepTellSeconds);
            sweepBurnSteps = S(p.sweepBurnSeconds);
            lineSteps = S(p.lineSeconds);
            lobWaitSteps = S(p.platformOrbSeconds);
            lobTellSteps = S(p.platformOrbTellSeconds);
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        Vec3 Local(Vec3 w) => w - inner.OriginWorld;

        /// <summary>Is the player standing on a platform now (its feet on a platform's top)?</summary>
        public bool PlayerOnPlatform() => !(playerRiding != null && playerRiding()) && arena.OnPlatform(Local(target.position()), 1.2); // a hop on the platform still counts; a rider over it does not

        /// <summary>The share of a hit that lands: none from a platform; Lantern29's light share x floorScale from the floor (DESIGN30 C1: light damage from the floor; round 1's
        /// Player reel projected a floor camper at 143 s against a 66.6 s rider, under 3x); otherwise Lantern29's (full from the wall with the core open, light elsewhere).</summary>
        public double HitScale()
        {
            if (PlayerOnPlatform()) { platformHits++; return p.platformScale; }
            Vec3 me = Local(target.position());
            bool riding = playerRiding != null && playerRiding();
            if (!riding && me.y <= rule.p.standHeight) { floorHits++; return rule.HitScale() * p.floorScale; }
            return rule.HitScale();
        }

        public int floorHits;

        /// <summary>The sim's boss-line gate (builder S's SurvivalSim.BossLine30 with design.n30.teach on: its own per-fight budget and the lesson widget); null: this class's budget.</summary>
        public Func<string, string, bool> lineGate;

        /// <summary>A new fight (a new body id): the counts and the lines start over.</summary>
        public void Reset()
        {
            lastStage = Lantern29Stage.Rest;
            liveCount = tellShows = coreShows = 0;
            sweepTell = sweepBurn = sweepTick = 0;
            SweepPanel = -1;
            lineLeft = 0;
            LineNow = LineKey = null;
            platformHits = sweeps = sweepTicks = ledFans = culled = floorHits = 0;
            sweepDamage = 0;
            dirSign = 1;
            lobWait = lobTell = lobs = 0;
        }

        /// <summary>NIGHT30 (builder S, item 12): the ONE boss-line budget (SurvivalSim.BossLine30 via Teach30, design.n30.teach.maxShows per fight): asked before a
        /// line is said; false = over the budget, nothing said. Null (teach off): the Lantern's own lineMaxShows count alone. (The lead's merge: B's lineGate and
        /// S's sayLine30 are the same gate; only one is asked, lineGate first.)</summary>
        public Func<string, string, bool> sayLine30;

        /// <summary>One place for the budget: the sim's gate when it has one (design.n30.teach: Teach30's per-fight count), else lineMaxShows per fight here.</summary>
        void TryLine(string key, string text, ref int shows)
        {
            Func<string, string, bool> gate = lineGate ?? sayLine30;
            if (gate != null) { if (!gate(key, text)) return; shows++; }
            else { if (shows >= p.lineMaxShows) return; shows++; }
            Line(key, text, shows);
        }

        void Line(string key, string text, int show)
        {
            LineNow = text; // (the budget is asked once, in TryLine: the lead's merge of B's lineGate and S's sayLine30)
            LineKey = key;
            lineLeft = lineSteps;
            lineSaid.boss = "lantern";
            lineSaid.key = key;
            lineSaid.text = text;
            lineSaid.seconds = p.lineSeconds;
            lineSaid.show = show;
            events.Emit("bossLine30", lineSaid);
        }

        /// <summary>One fixed step, after Lantern27 and Lantern29 have stepped.</summary>
        public void Step(double dt)
        {
            rule.Tell30(p.tellSeconds);
            Vec3 me = Local(target.position());
            Vec3 v = playerVelocity != null ? playerVelocity() : default;
            // the player's direction round the ring (counter-clockwise = +1): the sign of the angular velocity, kept while it stands still
            double cross = me.x * v.z - me.z * v.x;
            if (cross > 0.5) dirSign = 1; else if (cross < -0.5) dirSign = -1;
            if (lineLeft > 0 && --lineLeft == 0) LineNow = LineKey = null;
            if (!inner.Alive)
            {
                sweepTell = sweepBurn = 0;
                SweepPanel = -1;
                lobWait = lobTell = 0;
                return;
            }
            StepLob();
            Lantern29Stage st = rule.Stage;
            if (st != lastStage)
            {
                if (st == Lantern29Stage.Tell) TryLine("tell", p.lineTell, ref tellShows);
                if (st == Lantern29Stage.Live)
                {
                    liveCount++;
                    TryLine("core", p.lineCore, ref coreShows);
                    if (p.sweepEvery > 0 && liveCount % p.sweepEvery == 0) BeginSweep(me);
                }
                lastStage = st;
            }
            StepSweep(me);
            LeadAndCull(me, v, dt);
        }

        /// <summary>The platform camper: after platformOrbSeconds on a platform its landing spot is marked for platformOrbTellSeconds (at the player's feet then), and a slow
        /// orb is lobbed from the lantern onto it, falling under platformOrbGravity, landing platformOrbFlightSeconds later (it hurts as any orb: Lantern27's orb damage).
        /// Stepping off resets the wait; a tell already shown still fires. Only while the lantern fights (p1-p3).</summary>
        void StepLob()
        {
            bool fighting = inner.phase == "p1" || inner.phase == "p2" || inner.phase == "p3";
            if (lobTell > 0)
            {
                if (--lobTell == 0 && fighting) ThrowLob();
                return;
            }
            if (!fighting || !PlayerOnPlatform()) { lobWait = 0; return; }
            if (++lobWait < lobWaitSteps) return;
            lobWait = 0;
            lobTell = Math.Max(1, lobTellSteps);
            LobTarget = target.position();
            lobSaid.stage = "tell";
            lobSaid.target = LobTarget;
            lobSaid.seconds = p.platformOrbTellSeconds;
            events.Emit("lanternLob", lobSaid);
        }

        void ThrowLob()
        {
            Vec3 from = inner.Centre, to = new Vec3(LobTarget.x, LobTarget.y + 0.9, LobTarget.z);
            double T = Math.Max(0.2, p.platformOrbFlightSeconds), g = p.platformOrbGravity;
            var v = new Vec3((to.x - from.x) / T, (to.y - from.y + 0.5 * g * T * T) / T, (to.z - from.z) / T);
            int life = S(T + 1.0);
            inner.orbs.Add(new Lantern27Orb { pos = from, prev = from, vel = v, radius = inner.p.orbRadius, life = life, total = life, rain = false, kindId = 4 });
            lobs++;
            lobSaid.stage = "fire";
            lobSaid.target = LobTarget;
            lobSaid.seconds = T;
            events.Emit("lanternLob", lobSaid);
        }

        void BeginSweep(Vec3 me)
        {
            int n = arena.p.panels;
            int at = arena.PanelAt(me.x, me.z);
            SweepPanel = ((at + dirSign * p.sweepAheadPanels) % n + n) % n;
            sweepTell = sweepTellSteps;
            sweepBurn = 0;
            sweeps++;
            Say("tell", p.sweepTellSeconds);
        }

        void Say(string stage, double sec)
        {
            sweepSaid.stage = stage;
            sweepSaid.panel = SweepPanel;
            sweepSaid.seconds = sec;
            events.Emit("lanternSweep", sweepSaid);
        }

        void StepSweep(Vec3 me)
        {
            if (sweepTell > 0)
            {
                if (--sweepTell == 0) { sweepBurn = sweepBurnSteps; sweepTick = 0; Say("burn", p.sweepBurnSeconds); }
                return;
            }
            if (sweepBurn <= 0) return;
            bool on = arena.PanelAt(me.x, me.z) == SweepPanel && arena.WallDistance(me.x, me.z) <= p.sweepReach;
            if (on && ++sweepTick >= tickSteps)
            {
                sweepTick = 0;
                double amount = p.sweepDamagePerSecond * tickSteps * dt0;
                Health h = target.health;
                if (h != null && !h.Dead && h.Damage(amount, "hazard", target.position())) { sweepDamage += amount; sweepTicks++; }
            }
            else if (!on) sweepTick = 0;
            if (--sweepBurn == 0) { Say("end", 0); SweepPanel = -1; }
        }

        /// <summary>The fan orbs that fired this step turn to the lead point; every orb past the ring's wall is gone.</summary>
        void LeadAndCull(Vec3 me, Vec3 v, double dt)
        {
            List<Lantern27Orb> orbs = inner.orbs;
            Vec3 c = Local(inner.Centre);
            bool turned = false;
            double turn = 0;
            for (int i = orbs.Count - 1; i >= 0; i--)
            {
                Lantern27Orb o = orbs[i];
                Vec3 lo = Local(o.pos);
                if (arena.WallDistance(lo.x, lo.z) < 0) { orbs.RemoveAt(i); culled++; continue; }
                if (o.kindId == 4) { o.vel = new Vec3(o.vel.x, o.vel.y - p.platformOrbGravity * dt, o.vel.z); continue; } // the lob falls (Lantern27 moves it in a straight line each step)
                if (o.kindId != 0 || o.life != o.total - 1 || p.leadSeconds <= 0) continue;
                if (!turned)
                {
                    // one rotation for the whole fan (its spread stays): from the fan's centre line (the tell's aim) to the lead point's direction
                    Vec3 lead = me + v * p.leadSeconds;
                    double want = JsMath.Atan2(lead.z - c.z, lead.x - c.x), have = JsMath.Atan2(me.z - c.z, me.x - c.x);
                    have = FanCentre(c, have);
                    turn = want - have;
                    while (turn > Math.PI) turn -= 2 * Math.PI;
                    while (turn < -Math.PI) turn += 2 * Math.PI;
                    turned = true;
                    ledFans++;
                }
                double ct = JsMath.Cos(turn), stn = JsMath.Sin(turn);
                Vec3 nv = new Vec3(o.vel.x * ct - o.vel.z * stn, o.vel.y, o.vel.x * stn + o.vel.z * ct);
                o.vel = nv;
                o.pos = o.prev + nv * dt;
            }
        }

        /// <summary>The fan's centre direction now (the mean heading of the orbs that fired this step); `fallback` when none.</summary>
        double FanCentre(Vec3 c, double fallback)
        {
            double sx = 0, sz = 0;
            foreach (Lantern27Orb o in inner.orbs)
            {
                if (o.kindId != 0 || o.life != o.total - 1) continue;
                double l = JsMath.Hypot(o.vel.x, o.vel.z);
                if (l < 1e-9) continue;
                sx += o.vel.x / l;
                sz += o.vel.z / l;
            }
            return sx * sx + sz * sz < 1e-12 ? fallback : JsMath.Atan2(sz, sx);
        }
    }
}
