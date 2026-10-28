// NIGHT43 builder B1 (wick43): the Player scenario `wick43` (FeelReel.N43.cs's dispatch). Not run by its author (no Player in the worktree).
// THE WICK readable: the BEFORE (the run's wick43 bit off: NIGHT42's door, entrance and reveal) and the AFTER (wick43 on) in one run, then the
// bar's states, a leg beat with its gift, leg 3's cap flare and a retry's short reveal, each forced so every frame is certain. Boot level 1
// with reel.sh + -skygrindReelOnly (L-062: a CAMPAIGN run on n43-foundry / n42-foundry, its door d10 into THE STACK); on any other map the
// stack is put at (2600, 0, 0) (no door: the tunnel's mouth instead). God; input through forcedMove / forcedCombat (L-046); the look by
// g.input.SetLook (the reveal's ease is the sim's: it turns the same look). The boss bar in the frames is U1's bossbar43 when it is on (the
// line prints SurvivalSim.N43BossState at each bar frame, which is what bossbar43 draws).
// Frames (shot-N29-<name>.png), what each should show:
//   B43-door-before / B43-door-after  8 m outside THE STACK's door looking at it: before, the violet-white curtain and sign (G10's "pink");
//                      after, a cyan curtain (open) or red-orange (locked) like every door, a heavier frame, a warm white sign, warm light beyond.
//   B43-entrance-before / -after      0.1 s after the begin, looking 34 degrees off THE WICK: before, the flicker's red / blue split (the magenta
//                      fringe) during the 0.12 s hit-stop; after, the flicker's scanlines with no colour split and no hit-stop.
//   B43-reveal-before  1.2 s into NIGHT42's reveal: no letterbox, the look where the player left it (off the rising WICK).
//   B43-reveal-1 / -2 / -3            the first reveal at 15 % / 50 % / 90 % of its 2.5 s: the letterbox sliding in (and THE STACK's lights
//                      dipped), full with the look eased onto the rising cage and the lights coming on bottom to top, sliding away on the WICK.
//   B43-bar-full       the fight in, facing THE WICK from the shaft's floor: the bar full, leg 1 / 3, its hint.
//   B43-bar-locked     the bar pushed onto leg 1's lock (hp 60 %): LOCKED, the hint "LOCKED: CLIMB TO THE NEXT LEDGE".
//   B43-bar-open       a plate cracked facing the player: OPEN (a cracked plate, full damage), "RIDE, HOLD {fire}: SHOOT THE CRACK".
//   B43-bar-stagger    the stagger meter emptied: the CYAN window, "STAGGERED: EVERY HIT COUNTS", the plates flung open.
//   B43-flinch         2 frames after a hit in that window (round 2): the cage kicked away from the camera and tilted (the flinch).
//   B43-leg-beat       on ledge 2, 0.15 s after the leg change: the cage kicked up and back (shuddering), the slag 3 m lower, the bar's leg 2 / 3,
//                      "LEG CLEARED: A PLATE CRACKS".
//   B43-leg-gift       0.2 s after the leg change ends: the plate facing the player cracked open (CYAN seam).
//   B43-cap-flare      leg 3 on the cap, the FLARE's tell at 70 %: the red-orange arc on the catwalk round the player (+-55 deg), no wall half.
//   B43-reveal-retry   a retry's reveal at 50 % of its 1 s: thinner bars, the look turning to THE WICK.
// Line: GREEN when both begins ran, the after's reveal drew the letterbox (> 0 at -2) and turned the look onto THE WICK (the yaw error at -3
// under half of the one at the begin), the after's entrance took no hit-stop, the bar state was valid with locked / open / stagger as forced,
// a bossLeg43 and a gift crack were seen, the cap flare was drawn, and the retry's bars were thinner than the first's; RED otherwise.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N43Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Wick43Scenario(g, dir, res, offs, pre);
        }

        /// <summary>THE STACK's door (the campaign's door into the room with the wick37 row): its centre (sim coords, the floor), or null.</summary>
        static Vec3? Wick43Door(SurvivalGame g)
        {
            if (g.map?.json == null || !g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return null;
            if (!camp.TryGetValue("rooms", out object ro) || !camp.TryGetValue("doors", out object dobj) || !(dobj is List<object> doors)) return null;
            string boss = null;
            foreach (object o in PortData.Arr(ro)) { var r = PortData.Obj(o); if (r.ContainsKey("wick37") && r.TryGetValue("id", out object id)) boss = id as string; }
            if (boss == null) return null;
            foreach (object o in doors)
            {
                if (!(o is Dictionary<string, object> d) || !d.TryGetValue("between", out object bo) || !(bo is List<object> between) || !between.Contains(boss)) continue;
                double[] rect = Doors38Math.Rect(d.TryGetValue("rect", out object rc) ? rc : null);
                if (rect == null) continue;
                return new Vec3((rect[0] + rect[2]) / 2, 0, (rect[1] + rect[3]) / 2);
            }
            return null;
        }

        static IEnumerator Wick43Look(SurvivalGame g, Vec3 feet, Vec3 lookAt, string dir, string name, double[] st)
        {
            Vec3 eye = new Vec3(feet.x, feet.y + 1.6, feet.z), dv = lookAt - eye;
            double yaw = Math.Atan2(-dv.x, -dv.z), pitch = Math.Atan2(dv.y, Math.Sqrt(dv.x * dv.x + dv.z * dv.z));
            g.Respawn(feet, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            yield return Frames(20);
            yield return Shot29(dir, name, st);
        }

        static string Bar43(SurvivalSim sim)
        {
            Boss43State b = sim.N43BossState;
            CultureInfo ci = CultureInfo.InvariantCulture;
            return b.valid ? $"[valid hp01 {b.hp01.ToString("0.00", ci)} lockAt {b.lockAt.ToString("0.0", ci)} locked {b.locked} open {b.open} ({b.openLeft.ToString("0.0", ci)} s) stagger {b.stagger01.ToString("0.00", ci)} leg {b.leg}/{b.legs} \"{b.hint}\"]" : "[not valid]";
        }

        static IEnumerator Wick43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            N37Blocks? over37Was = sim.n37Override;
            N37Blocks want37 = N37Blocks.ForceOff ? N37Blocks.Off : N37Blocks.From(g.cfg);
            want37.wick37 = true;
            sim.n37Override = want37;
            offs.Add(() => { sim.n37Override = over37Was; });
            bool campaign = N24CampaignGate(g) == null;
            if (campaign)
            {
                offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
                Quiet(g);
                g.noRecords = g.noLab = true;
                g.setup.mode = Campaign24.Mode;
                g.StartRun(Seed);
                yield return Frames(240);
                sim = g.sim;
                for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
                DoorFree41(g);
                campaign = sim.campaignOn && sim.campaign != null;
            }
            else sim.n37.wick37 = true; // a survival run already started: the bit for the place / the begin
            bool wasOn = sim.n43.wick43;
            offs.Add(() => sim.Wick43ReelArm(wasOn));
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            bool levelStack = sim.wick37Centre.HasValue;
            if (!levelStack) sim.Wick37Place(new Vec3(2600, 0, 0));
            if (!sim.wick37Centre.HasValue || sim.wick37Stack == null) { res[0] = "RED: no stack (the wick37 row off?)"; yield break; }
            Vec3 c = sim.wick37Centre.Value;
            Wick37Stack s = sim.wick37Stack;
            Vec3? door = levelStack ? Wick43Door(g) : null;
            Vec3 mouth = c + new Vec3(0, 0.05, s.p.radius + 1.0);
            string pc = $"{(campaign ? "CAMPAIGN" : "survival")} run, map {g.MapId}, stack {(levelStack ? "the level's" : "placed at 2600,0,0")}, door {(door.HasValue ? "found" : "none (the tunnel's mouth)")}, wick43 at start {wasOn}, view {(Wick37View.Current != null ? "on" : "MISSING")}";
            Start(g, "wick43", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            int legEv = 0, openEv = 0;
            Action offLeg = sim.events.On<Boss43Event>(N43Events.BossLeg, e => legEv++), offOpen = sim.events.On<Boss43Event>(N43Events.BossOpen, e => openEv++);
            offs.Add(() => { offLeg(); offOpen(); });
            double look0 = 0.6; // the player looks 34 degrees off the shaft's axis at each begin
            double ErrDeg() { Wick37 ww = sim.wick37; if (ww == null) return -1; Vec3 d = ww.Centre - sim.Eye; double a = Math.Atan2(-d.x, -d.z) - sim.input.Yaw; while (a > Math.PI) a -= 2 * Math.PI; while (a < -Math.PI) a += 2 * Math.PI; return Math.Abs(a) * 180 / Math.PI; }
            int hitstopsBefore = -1, hitstopsAfter = -1;
            var sb = new System.Text.StringBuilder();
            float lbFirst = 0, lbRetry = 0;
            double err0 = -1, err3 = -1;
            bool revealOk = false;
            foreach (bool on in new[] { false, true })
            {
                string tag = on ? "after" : "before";
                sim.Wick37Reset();
                sim.Wick43ReelArm(on);
                // the door
                if (door.HasValue) yield return Wick43Look(g, door.Value + new Vec3(0, 0.05, 8), door.Value + new Vec3(0, 2.2, 0), dir, "B43-door-" + tag, st);
                else yield return Wick43Look(g, c + new Vec3(0, 0.05, s.p.radius + s.p.tunnelLength + 4), mouth + new Vec3(0, 2, 0), dir, "B43-door-" + tag, st);
                // the begin from the tunnel's mouth, looking off THE WICK
                g.Respawn(mouth, look0 * 180 / Math.PI);
                g.input.SetLook(look0, 0);
                g.forcedMove = new InputFrame { yaw = look0, pitch = 0 };
                yield return Frames(20);
                int hs0 = Juice38Probe.Hitstops;
                if (!sim.Wick37Begin(out _, out _)) { res[0] = $"RED: Wick37Begin returned false ({tag}) | " + pc; yield break; }
                g.forcedMove = new InputFrame();
                double t0 = sim.SimTime;
                if (on) err0 = ErrDeg();
                while (sim.SimTime - t0 < 0.1) yield return null;
                yield return Shot29(dir, "B43-entrance-" + tag, st);
                if (on) hitstopsAfter = Juice38Probe.Hitstops - hs0; else hitstopsBefore = Juice38Probe.Hitstops - hs0;
                if (!on)
                {
                    while (sim.SimTime - t0 < 1.2) yield return null;
                    yield return Shot29(dir, "B43-reveal-before", st);
                    sb.Append($" before: hit-stops at the entrance {hitstopsBefore}, yaw error to THE WICK at 1.2 s {ErrDeg().ToString("0.0", ci)} deg;");
                    continue;
                }
                foreach (var (at, name) in new[] { (0.15, "B43-reveal-1"), (0.5, "B43-reveal-2"), (0.9, "B43-reveal-3") })
                {
                    for (int k = 0; k < 600 && sim.Wick43Reveal01 >= 0 && sim.Wick43Reveal01 < at; k++) yield return null;
                    yield return Shot29(dir, name, st);
                    if (name == "B43-reveal-2") lbFirst = Wick37View.Current != null ? Wick37View.Current.Letterbox43 : -1;
                    if (name == "B43-reveal-3") err3 = ErrDeg();
                }
                revealOk = lbFirst > 0 && err0 > 0 && err3 >= 0 && err3 < err0 * 0.5;
                sb.Append($" after: hit-stops at the entrance {hitstopsAfter} (wick43 entrances {Juice38Probe.Wick43Entrances}), the reveal {sim.wick43RevealSteps * sim.step:0.00} s first {sim.wick43RevealFirst}, letterbox at 50 % {lbFirst.ToString("0.000", ci)}, yaw error {err0.ToString("0.0", ci)} -> {err3.ToString("0.0", ci)} deg (eased steps {sim.wick43EasedSteps}, yielded {sim.wick43LookYielded});");
            }
            Wick37 w = sim.wick37;
            // wait the fight in (the reveal over)
            double tf = sim.SimTime;
            while (w.brain != null && !w.brain.Fighting && sim.SimTime - tf < 4) yield return null;
            // the bar's states, forced
            Vec3 face = c + new Vec3(0, 0.05, s.p.radius - 2.5);
            yield return Wick43Look(g, face, w.Centre, dir, "B43-bar-full", st);
            w.Apply(w.body.maxHp);
            yield return Frames(6);
            string locked = Bar43(sim);
            bool lockedOk = sim.N43BossState.locked;
            yield return Shot29(dir, "B43-bar-locked", st);
            Vec3 cc = w.Centre;
            int plate = w.PlateAt(Math.Atan2(sim.mover.position.z - cc.z, sim.mover.position.x - cc.x));
            w.crackLeft[plate] = MovementMath.StepsFor(w.p.crackSeconds, sim.step);
            yield return Frames(6);
            string open = Bar43(sim);
            bool openOk = sim.N43BossState.open;
            yield return Shot29(dir, "B43-bar-open", st);
            for (int i = 0; i < w.crackLeft.Length; i++) w.crackLeft[i] = 0;
            w.brain.Stagger(w.brain.stagger + 1, "reel");
            yield return Frames(6);
            string stag = Bar43(sim);
            bool stagOk = w.brain.Open && sim.N43BossState.open;
            yield return Shot29(dir, "B43-bar-stagger", st);
            int fl0 = Wick37View.Current != null ? Wick37View.Current.Flinches43 : -1;
            w.body.hp = Math.Min(w.body.maxHp, w.body.hp + 120); yield return null; // off the leg's lock (the locked frame put it there), one frame for the view to see it
            w.Apply(40); // round 2: a hit while OPEN (the window): the cage flinches
            yield return Frames(2);
            yield return Shot29(dir, "B43-flinch", st);
            int flinches = Wick37View.Current != null ? Wick37View.Current.Flinches43 - fl0 : -1;
            // a leg change: the player on ledge 2 (its arc), looking at THE WICK; the sim's checkpoint rule changes the leg
            double rl = s.p.radius - s.p.ledgeWidth * 0.5;
            Vec3 ledge2 = s.p.ledges.Length > 1 && s.ledgeAngle.Length > 1 ? c + new Vec3(Math.Cos(s.ledgeAngle[1]) * rl, s.p.ledges[1] + 0.05, Math.Sin(s.ledgeAngle[1]) * rl) : face;
            int legEv0 = legEv, gifts0 = w.gifts43;
            g.Respawn(ledge2, 0);
            Vec3 dvw = w.Centre - (ledge2 + new Vec3(0, 1.6, 0));
            g.input.SetLook(Math.Atan2(-dvw.x, -dvw.z), Math.Atan2(dvw.y, Math.Sqrt(dvw.x * dvw.x + dvw.z * dvw.z)));
            g.forcedMove = new InputFrame();
            double tl = sim.SimTime;
            for (int k = 0; k < 600 && w.brain.leg < 2; k++) yield return null;
            double tb = sim.SimTime;
            while (sim.SimTime - tb < 0.15) yield return null;
            string beat = Bar43(sim);
            yield return Shot29(dir, "B43-leg-beat", st);
            for (int k = 0; k < 900 && w.gifts43 == gifts0; k++) yield return null;
            double tg = sim.SimTime;
            while (sim.SimTime - tg < 0.2) yield return null;
            yield return Shot29(dir, "B43-leg-gift", st);
            bool legOk = legEv > legEv0 && w.gifts43 > gifts0;
            // leg 3: the cap, a forced FLARE
            double rc = (s.p.radius + s.p.capRadius) / 2;
            Vec3 cap = c + new Vec3(rc, s.p.capY + 0.05, 0);
            g.Respawn(cap, 0);
            for (int k = 0; k < 600 && w.brain.leg < 3; k++) yield return null;
            g.input.SetLook(Math.PI / 2, -0.35); // along the catwalk, down at it
            w.brain.Force(Wick37.Flare);
            bool capShot = false;
            double tc = sim.SimTime;
            while (sim.SimTime - tc < 12 && !capShot)
            {
                God(g);
                Boss37Brain b = w.brain;
                if (b.state == Boss37State.Tell && w.AttackId == Wick37.Flare && b.Progress >= 0.7) { capShot = true; yield return Shot29(dir, "B43-cap-flare", st); break; }
                yield return null;
            }
            bool capOk = capShot && Wick37View.Current != null && Wick37View.Current.CapFlare43;
            // a retry's reveal (the fight live: Wick37Begin restarts the leg)
            g.Respawn(mouth, look0 * 180 / Math.PI);
            g.input.SetLook(look0, 0);
            yield return Frames(10);
            sim.Wick37Begin(out _, out _);
            for (int k = 0; k < 300 && sim.Wick43Reveal01 >= 0 && sim.Wick43Reveal01 < 0.5; k++) yield return null;
            lbRetry = Wick37View.Current != null ? Wick37View.Current.Letterbox43 : -1;
            yield return Shot29(dir, "B43-reveal-retry", st);
            bool retryOk = !sim.wick43RevealFirst && lbRetry > 0 && lbRetry < lbFirst;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            bool ok = revealOk && hitstopsAfter == 0 && lockedOk && openOk && stagOk && legOk && capOk && retryOk;
            res[0] = $"{(ok ? "GREEN" : "RED")}:{sb} bar locked {lockedOk} {locked}; open {openOk} {open}; stagger {stagOk} {stag}; leg beat {legOk} (bossLeg43 {legEv}, gifts {w.gifts43}, slag {sim.wick37Slag.y.ToString("0.0", ci)} m) {beat}; cap flare {capOk} (cap flares {w.capFlares43}); retry first {sim.wick43RevealFirst}, bars {lbRetry.ToString("0.000", ci)} vs first {lbFirst.ToString("0.000", ci)} ({retryOk}); flinches at the open hit {flinches}; bossOpen43 {openEv} | {pc}";
        }
    }
}
