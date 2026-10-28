// NIGHT43 builder B1 (wick43; NIGHT43.md card B1: G13-G16, G10; DECIDE43 D-7; research/bosses-web.md). The B1 hooks of SurvivalSim.N43.cs, with the
// wick43 bit on (N43Blocks; --off43 / -skygrindOff43 and every older off switch: nothing here runs, THE WICK is NIGHT42's):
//   N43BossStateFill  (G14) what the boss bar draws (U1's bossbar43 reads SurvivalSim.N43BossState): VALID for the whole fight, from the walk-in's
//                     reveal to the end of the dying (NIGHT42's bar read Wick37.Bar(), whose valid is the body's alive: gone before the rise and at
//                     the finisher), hp01, the leg's lock (lockAt; locked = the HP sits on it, so the bar says why it does not move: climb), OPEN
//                     (the CYAN window or a cracked plate: full damage) with openLeft, the stagger meter, the leg of 3 and a hint (what hurts it
//                     now, design.n43.wick43.hints).
//   the reveal        (G15, DECIDE43 D-7: you keep control) the walk-in's first reveal eases the LOOK toward the rising WICK for
//                     reveal.firstSeconds (a retry: reveal.retrySeconds): a share of the angle left per step on an ease-in / ease-out curve; the
//                     player's own look (a turn of more than reveal.yieldDeg in a step) ends the ease at once. The view draws the letterbox over
//                     the same window (Wick43RevealView reads Wick43Reveal01 / Wick43RevealFirst).
//   the legs          (G16) Core/Enemies/Wick37.N43.cs (the beat, the gift crack, leg 2's second volley, leg 3's cap flare, bossOpen43).
// Off: no hook body runs; the Wick37 object's n43 is false (its Step43 is skipped). No UnityEngine (SimHarness compiles this file).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Wick43Params wick43Legs;
        string w43Reveal, w43LegChange, w43Window, w43Crack, w43Locked, w43Ember, w43Stall, w43Cap, w43Idle;
        double w43FirstS, w43RetryS, w43LookRate, w43YieldDeg, w43PitchMaxDeg, w43AirEase;
        /// <summary>Counts for the harness: steps the shaft air left a floor run alone, steps it eased an airborne speed.</summary>
        public int wick43AirFloor, wick43AirEased;
        bool w43Read;

        /// <summary>The reveal: the step it began (-1: none this run), its length in steps, first (the walk-in) or a retry; the look ease
        /// yielded to the player's own look.</summary>
        public int wick43RevealAt = -1, wick43RevealSteps;
        public bool wick43RevealFirst, wick43LookYielded;
        /// <summary>Counts for the harness and the reels: reveals (first + retries), steps the look was eased, fights begun this run.</summary>
        public int wick43Reveals, wick43EasedSteps, wick43Fights;
        double w43LastYaw, w43LastPitch;

        /// <summary>wick43 runs here (the row on, THE STACK on this level and its rules on).</summary>
        public bool Wick43On => n43.wick43 && Wick37On;

        /// <summary>The reveal's progress 0..1 (-1 when no reveal is playing).</summary>
        public double Wick43Reveal01 => wick43RevealAt < 0 || wick43RevealSteps <= 0 || simStep - wick43RevealAt > wick43RevealSteps ? -1 : (simStep - wick43RevealAt) / (double)wick43RevealSteps;

        void Wick43Read()
        {
            if (w43Read) return;
            w43Read = true;
            const string r = "design.n43.wick43.";
            wick43Legs = Wick43Params.From(cfg);
            w43Reveal = cfg.Str(r + "hints.reveal");
            w43LegChange = cfg.Str(r + "hints.legChange");
            w43Window = cfg.Str(r + "hints.window");
            w43Crack = cfg.Str(r + "hints.crack");
            w43Locked = cfg.Str(r + "hints.locked");
            w43Ember = cfg.Str(r + "hints.ember");
            w43Stall = cfg.Str(r + "hints.stall");
            w43Cap = cfg.Str(r + "hints.cap");
            w43Idle = cfg.Str(r + "hints.idle");
            w43FirstS = cfg.Num(r + "reveal.firstSeconds");
            w43RetryS = cfg.Num(r + "reveal.retrySeconds");
            w43LookRate = cfg.Num(r + "reveal.lookRate");
            w43YieldDeg = cfg.Num(r + "reveal.yieldDeg");
            w43PitchMaxDeg = cfg.Num(r + "reveal.pitchMaxDeg");
            w43AirEase = cfg.Num(r + "air.easeRate");
        }

        partial void N43WickStartRun()
        {
            wick43RevealAt = -1;
            wick43RevealSteps = 0;
            wick43RevealFirst = wick43LookYielded = false;
            wick43Reveals = wick43EasedSteps = wick43Fights = wick43AirFloor = wick43AirEased = 0;
            b43Boss = null;
            boss43Opens = boss43Legs = 0;
            if (!n43.wick43) return;
            Wick43Read();
        }

        /// <summary>The reel's switch (FeelReel.N43.Wick.cs: the before / after frames in one run): the run's wick43 bit set now, the reveal's
        /// "first" counter cleared (the next Wick37Begin is a first reveal). Never called by the game.</summary>
        public void Wick43ReelArm(bool on)
        {
            n43.wick43 = on;
            wick43Fights = 0;
            wick43RevealAt = -1;
            wick43LookYielded = false;
            if (on) Wick43Read();
        }

        /// <summary>Wick37Begin's call (SurvivalSim.Wick37.cs) before a fight starts: the Wick37 object takes the row's bit and numbers.</summary>
        void Wick43Arm()
        {
            if (wick37 == null) return;
            wick37.n43 = n43.wick43 && Wick37On;
            if (!wick37.n43) return;
            Wick43Read();
            wick37.p43 = wick43Legs;
        }

        /// <summary>Wick37Begin's call once a fight began (retry: the leg restarted from its checkpoint): the reveal's window.</summary>
        void Wick43Began(bool retry)
        {
            if (!Wick43On) return;
            wick43Fights++;
            bool first = !retry && wick43Fights == 1;
            wick43RevealFirst = first;
            wick43RevealAt = simStep;
            wick43RevealSteps = MovementMath.StepsFor(first ? w43FirstS : w43RetryS, cfg.Dt);
            wick43LookYielded = false;
            w43LastYaw = input != null ? input.Yaw : 0;
            w43LastPitch = input != null ? input.Pitch : 0;
            wick43Reveals++;
        }

        partial void N43WickStep(double dt)
        {
            if (n43.wick43) Bosses43Step(); // round 2: bossOpen43 / bossLeg43 for the other bosses (SurvivalSim.N43.Bosses.cs)
            if (!Wick43On || wick37 == null || !wick37Owns || !wick37.Alive) return;
            Wick43Look(dt);
        }

        /// <summary>The reveal's look ease (D-7: the player keeps control; his own turn ends it).</summary>
        void Wick43Look(double dt)
        {
            double t = Wick43Reveal01;
            if (t < 0 || wick43LookYielded || input == null || !Active) return;
            double y = input.Yaw, pt = input.Pitch;
            double yieldRad = w43YieldDeg * Math.PI / 180;
            if (Math.Abs(WrapPi43(y - w43LastYaw)) > yieldRad || Math.Abs(pt - w43LastPitch) > yieldRad) { wick43LookYielded = true; return; }
            Vec3 eye = Eye, d = wick37.Centre - eye;
            double h = JsMath.Hypot(d.x, d.z);
            if (h < 0.5) return;
            double wantYaw = JsMath.Atan2(-d.x, -d.z), pmax = w43PitchMaxDeg * Math.PI / 180;
            double wantPitch = Math.Max(-pmax, Math.Min(pmax, JsMath.Atan2(d.y, h)));
            // ease in over the first quarter, out over the last quarter (smoothstep), full between
            double a = Math.Min(1, Math.Min(t / 0.25, (1 - t) / 0.25));
            double w = a <= 0 ? 0 : a * a * (3 - 2 * a);
            double k = Math.Min(1, w43LookRate * dt * w);
            double ny = y + WrapPi43(wantYaw - y) * k, np = pt + (wantPitch - pt) * k;
            input.SetLook(ny, np);
            w43LastYaw = ny;
            w43LastPitch = np;
            if (k > 0) wick43EasedSteps++;
        }

        /// <summary>G10 / G13 (the "brick wall"): THE STACK's thick air (design.n37.wick37.air.maxSpeed 12.4 m/s: every leap of the climb flies the
        /// same arc) with wick43 on: never on a floor (a run in from the tunnel at sprint speed keeps it), and in the air the speed over the cap
        /// bleeds at air.easeRate m/s per second instead of being cut in one step. False (NIGHT42's clamp runs) with the row off.</summary>
        bool Wick43AirClamp(double airMax, double dt)
        {
            if (!n43.wick43) return false;
            if (mover.grounded) { wick43AirFloor++; return true; }
            Vec3 v = mover.velocity;
            double hs = JsMath.Hypot(v.x, v.z);
            if (hs <= airMax) return true;
            double to = Math.Max(airMax, hs - w43AirEase * dt);
            mover.velocity = new Vec3(v.x * to / hs, v.y, v.z * to / hs);
            wick43AirEased++;
            return true;
        }

        static double WrapPi43(double a)
        {
            while (a > Math.PI) a -= 2 * Math.PI;
            while (a < -Math.PI) a += 2 * Math.PI;
            return a;
        }

        partial void N43BossStateFill(ref Boss43State s)
        {
            if (!n43.wick43) return;
            if (!Wick43Fill(ref s)) Bosses43Fill(ref s); // round 2 (the lead: DESIGN-RULES B-1, every boss): THE WICK, else the live boss (SurvivalSim.N43.Bosses.cs)
        }

        /// <summary>THE WICK's bar state; false when THE WICK is not the live fight.</summary>
        bool Wick43Fill(ref Boss43State s)
        {
            if (!Wick43On || wick37 == null || !wick37Owns || wick37.brain == null) return false;
            Boss37Brain b = wick37.brain;
            if (b.state == Boss37State.Off || b.state == Boss37State.Dead) return false;
            if (!wick37.Alive && b.state != Boss37State.Dying) return false;
            bool dying = b.state == Boss37State.Dying;
            s.valid = true;
            s.name = wick37.p.name;
            s.leg = b.leg;
            s.legs = 3;
            s.hp01 = dying ? 0 : Math.Max(0, Math.Min(1, wick37.HpFrac));
            s.lockAt = dying ? 0 : wick37.hpLock;
            s.locked = !dying && wick37.hpLock > 0 && wick37.body.hp <= wick37.body.maxHp * wick37.hpLock + 0.5;
            s.open = !dying && wick37.Open43;
            s.openLeft = !s.open ? 0 : b.Open ? b.stateLeft * cfg.Dt : wick37.CrackLeftMax * cfg.Dt;
            s.stagger01 = dying ? 0 : b.Open ? Math.Max(0, 1 - b.Progress) : b.StaggerFrac;
            s.hint = wick37.w47 ? null : Wick43Hint(b, s.locked, dying); // NIGHT47 W (wick47): the leg hints give way to wick47's own lines (one at a time: SurvivalSim.N47.Wick.cs Wick47Say)
            return true;
        }

        string Wick43Hint(Boss37Brain b, bool locked, bool dying)
        {
            if (dying) return null;
            if (b.state == Boss37State.Reveal) return w43Reveal;
            if (b.state == Boss37State.LegChange) return w43LegChange;
            if (wick37.stalled) return w43Stall;
            if (b.Open) return w43Window;
            if (wick37.CrackedCount > 0) return w43Crack;
            if (locked && b.leg < 3) return w43Locked;
            if ((wick37.AttackId == Wick37.Ember && (b.state == Boss37State.Tell || b.state == Boss37State.Active)) || wick37.EmbersIncoming) return w43Ember;
            if (b.leg >= 3) return w43Cap;
            return w43Idle;
        }
    }
}
