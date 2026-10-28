// NIGHT33 builder W (DESIGN33 W, D-3): the whip's headless judge (the entry is N33Weapons in Harness.N33.Weapons.cs).
//   whip33 [--off]   the SURVIVAL map, the round clock held (no director spawns), invulnerable, the ARCLINE owned, NIGHT31's pull and NIGHT32's charges
//                    as the config has them, design.n33.whip33 on (GREEN) or off (`--off`: NIGHT32's grapple, the RED side). Taps are a press held 2
//                    steps (inside the 0.2 s tap window). Enemies are pinned in place (bags, 1e9 hp, unless said).
//     A HEAVY: a charger (radius 0.7) 6 / 10 / 14 m ahead in an open direction, one tap at its aim point, 2.5 s. Per pull: the gap at the stop
//       (SurvivalSim.Whip33Gap: chest to aim point less both radii), the least gap over the run, steps INSIDE (the player's axis within the enemy's
//       radius + the capsule radius, the bodies overlapping in height), the whip's damage, the hitstop request, the speed after the stop.
//     B LIGHT: a husk 10 m ahead, a tap: the yank (the husk's distance 0.6 s later). Then a second husk 10 m ahead with 10 hp while the yank cools,
//       a tap: GREEN: it is a whip zip (stop short, the hit kills it), and the kill gives NO grapple charge back.
//     C CANCEL: a charger 14 m ahead, a tap, a JUMP 0.2 s after the hook bites: the speed kept, no arrival hit.
//     D SPAM: four chargers 8-14 m around, 20 s; whenever the rope is idle, aim at the nearest and tap: hooks fired, stops, the cap
//       2 + floor(20 / rechargeSeconds) (bags: no kill refills).
//   (round 2 item 3, the bosses with pierce33 on vs off: Tools/night/n33w-bosspierce.sh runs boss32 / crown32 from a scratch root whose design config has the row on)
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Whip33Run
    {
        public SurvivalSim sim;
        public ScriptedInput si;
        public Vec3 sp;
        public double ux, uz, yaw;
        public int hitstops, arrivals, whipHits, kills, ropeFires;
        public double whipDamage;
        public readonly List<double> arriveGaps = new List<double>();
    }

    static Whip33Run Whip33Setup(GameConfig cfg, SurvivalMap map, bool on, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n33Override = new N33Blocks { whip33 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.holdRunClock = true;
        if (sim.arcline21 == null) return null;
        sim.arcline21.Own(true);
        sim.Respawn(map.playerSpawn, 0);
        var r = new Whip33Run { sim = sim, si = (ScriptedInput)sim.input, sp = map.playerSpawn };
        (r.yaw, r.ux, r.uz) = ArclineOpenDir(sim, map.playerSpawn, 15);
        sim.events.On<string>("hitstop", c => { if (c == "whip33") r.hitstops++; });
        sim.events.On<Whip33Event>("whip33Arrive", e => { r.arrivals++; r.arriveGaps.Add(e.gap); });
        sim.events.On<HitEvent>("hit", e => { if (e.weapon == SurvivalSim.Whip33Weapon) { r.whipHits++; r.whipDamage += e.damage; } });
        sim.events.On<KillEvent>("kill", e => { if (e.weapon == SurvivalSim.Whip33Weapon) r.kills++; });
        sim.mover.Events += e => { if (e.type == MoverEventType.RopeFire) r.ropeFires++; };
        for (int s = 0; s < 30; s++) G32Tick(sim, r.si, null, false, false, false);
        return r;
    }

    static Husk Whip33Place(Whip33Run r, string kind, double d, double side, double hp)
    {
        double rx = -r.uz, rz = r.ux; // to the right of the open direction
        Husk h = null;
        for (int k = 0; k < 8 && h == null; k++)
            h = r.sim.husks.Spawn(kind, new Vec3(r.sp.x + r.ux * d + rx * side, r.sp.y + 0.25 * k, r.sp.z + r.uz * d + rz * side), r.sp);
        if (h != null) h.hp = h.maxHp = hp;
        return h;
    }

    /// <summary>Is the player's body inside the enemy's hit volume (axis within both radii, heights overlapping)?</summary>
    static bool Whip33Inside(SurvivalSim sim, Enemy e)
    {
        Vec3 p = sim.mover.position;
        double h = JsMath.Hypot(p.x - e.position.x, p.z - e.position.z);
        double top = p.y + sim.cfg.movement.body.capsuleHeight;
        return h < e.Radius + sim.cfg.movement.body.capsuleRadius && top > e.position.y && p.y < e.position.y + e.Height;
    }

    static void Whip33Command(string[] args, ref int code)
    {
        bool on = !Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        double stopGap = cfg.Num("design.n33.whip33.stopGap");
        Console.WriteLine($"whip33: {(on ? "design.n33.whip33 ON" : "RED side: NIGHT32's grapple (whip33 off)")}; stopGap {stopGap} m, damage {cfg.Num("design.n33.whip33.damage")}, hitstop {cfg.Int("design.n33.whip33.hitstopSteps")} steps");

        // A HEAVY
        foreach (double d in new[] { 6.0, 10.0, 14.0 })
        {
            Whip33Run r = Whip33Setup(cfg, map, on, 3310);
            if (r == null) { fails.Add("no ARCLINE"); break; }
            Husk e = Whip33Place(r, "charger", d, 0, 1e9);
            if (e == null) { fails.Add($"A {d} m: no charger placed"); continue; }
            Vec3 pin = e.position;
            double minGap = double.MaxValue, maxSpeed = 0, speedAfter = -1;
            int inside = 0, arrivedAt = -1;
            for (int s = 0; s < Steps(2.5); s++)
            {
                if (e.alive) { e.position = pin; e.velocity = default; }
                G32Tick(r.sim, r.si, e.AimPoint, s < 2, s == 0, false);
                if (!e.alive) continue;
                minGap = Math.Min(minGap, r.sim.Whip33Gap(e));
                if (Whip33Inside(r.sim, e)) inside++;
                maxSpeed = Math.Max(maxSpeed, r.sim.mover.velocity.Length);
                if (arrivedAt < 0 && r.arrivals > 0) { arrivedAt = s; speedAfter = JsMath.Hypot(r.sim.mover.velocity.x, r.sim.mover.velocity.z); }
            }
            string at = r.arriveGaps.Count > 0 ? r.arriveGaps[0].ToString("0.00") : "-";
            Console.WriteLine($"  A charger {d,2} m: hooks {r.ropeFires}, stops {r.arrivals} (gap at the stop {at} m, at {(arrivedAt >= 0 ? (arrivedAt * dt).ToString("0.00") + " s" : "-")}), least gap {minGap:0.00} m, steps inside {inside}, top speed {maxSpeed:0.0} m/s, flat speed after the stop {(speedAfter >= 0 ? speedAfter.ToString("0.0") : "-")}, whip hits {r.whipHits} ({r.whipDamage:0} dmg), hitstops {r.hitstops}");
            if (on)
            {
                Need(r.arrivals == 1 && r.whipHits == 1 && r.hitstops == 1, $"A {d} m: one stop, one hit, one hitstop");
                Need(minGap >= stopGap - 1e-6, $"A {d} m: least gap {minGap:0.000} < {stopGap}");
                Need(inside == 0, $"A {d} m: {inside} steps inside");
            }
        }

        // B LIGHT + the kill refill
        {
            Whip33Run r = Whip33Setup(cfg, map, on, 3311);
            Husk a = Whip33Place(r, "husk", 10, 0, 1e9);
            Vec3 pin0 = a != null ? a.position : default;
            double before = a != null ? JsMath.Hypot(a.position.x - r.sim.mover.position.x, a.position.z - r.sim.mover.position.z) : 0;
            int yanks0 = r.sim.arcline21.pulls;
            for (int s = 0; s < Steps(0.6); s++) G32Tick(r.sim, r.si, a != null && a.alive ? a.AimPoint : (Vec3?)null, s < 2, s == 0, false);
            double after = a != null ? JsMath.Hypot(a.position.x - r.sim.mover.position.x, a.position.z - r.sim.mover.position.z) : 0;
            int yanks = r.sim.arcline21.pulls - yanks0;
            if (a != null) { a.alive = false; } // out of the way (a bag: it would stand between)
            // the second husk while the yank cools (15 s): 10 hp
            for (int s = 0; s < Steps(1.0); s++) G32Tick(r.sim, r.si, null, false, false, false);
            Husk b = Whip33Place(r, "husk", 10, 0, 10);
            int refills0 = r.sim.g32KillRefills, charges0 = r.sim.grappleCharges32, zips0 = r.sim.arcline21.whip33Zips;
            Vec3 pinB = b != null ? b.position : default;
            int insideB = 0;
            double minGapB = double.MaxValue;
            for (int s = 0; s < Steps(2.0); s++)
            {
                if (b != null && b.alive) { b.position = pinB; b.velocity = default; }
                G32Tick(r.sim, r.si, b != null && b.alive ? b.AimPoint : (Vec3?)null, s < 2, s == 0, false);
                if (b != null && b.alive) { minGapB = Math.Min(minGapB, r.sim.Whip33Gap(b)); if (Whip33Inside(r.sim, b)) insideB++; }
            }
            int refills = r.sim.g32KillRefills - refills0;
            Console.WriteLine($"  B husk 10 m (yank ready): yanks {yanks}, its distance {before:0.0} -> {after:0.0} m in 0.6 s");
            Console.WriteLine($"  B husk 10 m, 10 hp, the yank cooling: whip zips {r.sim.arcline21.whip33Zips - zips0}, stops {r.arrivals}, least gap {(minGapB == double.MaxValue ? "-" : minGapB.ToString("0.00"))} m, steps inside {insideB}, whip kills {r.kills}; kill refills {refills}, no-refill whip kills {r.sim.whip33NoRefill}; charges {charges0} -> {r.sim.grappleCharges32}");
            Need(yanks == 1 && after < before - 3, "B: the light husk is yanked to you");
            if (on)
            {
                Need(r.sim.arcline21.whip33Zips - zips0 == 1 && r.arrivals == 1 && r.kills == 1, "B: the cooling husk is whipped (stop + kill)");
                Need(refills == 0 && r.sim.whip33NoRefill == 1, "B: a whip kill gives no charge back");
                Need(insideB == 0 && minGapB >= stopGap - 1e-6, "B: stop short");
            }
        }

        // C CANCEL
        {
            Whip33Run r = Whip33Setup(cfg, map, on, 3312);
            Husk e = Whip33Place(r, "charger", 14, 0, 1e9);
            Vec3 pin = e != null ? e.position : default;
            int bitAt = -1, releasedAt = -1;
            double speedAtRelease = 0;
            r.sim.mover.Events += ev =>
            {
                if (ev.type == MoverEventType.RopeAttach && bitAt < 0) bitAt = r.sim.simStep;
                if (ev.type == MoverEventType.RopeRelease && releasedAt < 0) { releasedAt = r.sim.simStep; speedAtRelease = ev.speed; }
            };
            for (int s = 0; s < Steps(2.0); s++)
            {
                if (e != null && e.alive) { e.position = pin; e.velocity = default; }
                bool jump = bitAt >= 0 && r.sim.simStep == bitAt + Steps(0.2);
                G32Tick(r.sim, r.si, e != null && e.alive ? e.AimPoint : (Vec3?)null, s < 2, s == 0, jump);
            }
            Console.WriteLine($"  C charger 14 m, a jump 0.2 s after the bite: released {(releasedAt >= 0 && bitAt >= 0 ? ((releasedAt - bitAt) * dt).ToString("0.00") + " s after the bite" : "-")} at {speedAtRelease:0.0} m/s (NIGHT32's cancel clamp {cfg.Num("design.n32.grapple32.cancelKeepMax")}), stops {r.arrivals}, whip hits {r.whipHits}");
            if (on) Need(r.arrivals == 0 && r.whipHits == 0 && speedAtRelease > 10, "C: a jump cancels with momentum, no hit");
        }

        // D SPAM
        {
            Whip33Run r = Whip33Setup(cfg, map, on, 3313);
            // one charger 12 m out on the open line; after each stop (or a whip that is over) the bot is put back at the spawn 0.3 s later and taps again
            Husk bag = Whip33Place(r, "charger", 12, 0, 1e9);
            Vec3 pinD = bag != null ? bag.position : default;
            int fires0 = r.ropeFires, latches0 = r.sim.whip33Latches, presses = 0, insideD = 0, settle = -1;
            double minGapD = double.MaxValue;
            for (int s = 0; s < Steps(20) && bag != null; s++)
            {
                if (bag.alive) { bag.position = pinD; bag.velocity = default; }
                bool idle = r.sim.mover.rope.phase == RopePhase.None && r.sim.mover.rope.cooldownSteps == 0;
                double away = JsMath.Hypot(r.sim.mover.position.x - r.sp.x, r.sim.mover.position.z - r.sp.z);
                if (idle && away > 1) { if (settle < 0) settle = Steps(0.3); else if (--settle == 0) { r.sim.Respawn(r.sp, 0); settle = -1; } }
                bool press = idle && away <= 1 && s % 6 == 0;
                if (press) presses++;
                G32Tick(r.sim, r.si, bag.AimPoint, press || (s % 6 == 1 && !idle), press, false);
                minGapD = Math.Min(minGapD, r.sim.Whip33Gap(bag));
                if (Whip33Inside(r.sim, bag)) insideD++;
            }
            int hooks = r.ropeFires - fires0, latches = r.sim.whip33Latches - latches0;
            int cap = 2 + (int)Math.Floor(20 / cfg.Num("design.n32.grapple32.rechargeSeconds"));
            Console.WriteLine($"  D spam 20 s, a charger 12 m out (back to the spawn after each pull): presses {presses}, hooks fired {hooks} (cap {cap}; charges spent {r.sim.g32Spent}, refunds {r.sim.g32Refunds}, presses cleared {r.sim.g32Empty}), whip latches {latches}, stops {r.arrivals}, whip hits {r.whipHits}, least gap {minGapD:0.00} m, steps inside {insideD}");
            if (on) Need(hooks - r.sim.g32Refunds <= cap && r.arrivals >= 3 && r.arrivals <= cap && insideD == 0, "D: spam within the charges, no step inside");
        }
        Console.WriteLine(fails.Count == 0 ? (on ? "whip33 GREEN" : "whip33 RED side printed (NIGHT32's grapple)") : $"whip33 FAIL: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }

    // ---- whipboss33 ----------------------------------------------------------------------------------------------------------------------------
    //   whipboss33 [--off]   round 3: the whip on every boss. The camp rigs (Harness.N31.Boss.cs CampRig31For: lantern31, stag, wyrm31, loom31; THE CROWN's
    //                        CrownSim32), the fight begun, then whip33 armed on the run (SurvivalSim.Whip33Arm; --off: NIGHT32's grapple). From each camp spot:
    //                        the ARCLINE owned, charges full, aim at the boss's centre and HOLD the grapple 2.5 s (a held zip: some rigs run without the
    //                        NIGHT31 tap latch). Per spot: what the hook took (an enemy kind and its Radius, a node, nothing), the stops, the gap at the stop
    //                        (SurvivalSim.Whip33Gap: the enemy's own hit shapes), the least TRUE gap to the boss over the run (the rig's own ray to the boss's
    //                        hit shapes from the chest toward its centre, less the capsule radius; a ray that starts inside counts 0) and the steps inside
    //                        (true gap <= 0). GREEN: no spot ever inside; every enemy stop at a true gap >= stopGap - 0.05.
    static void WhipBoss33Command(string[] args, ref int code)
    {
        bool on = !Flag(args, "--off");
        var fails = new List<string>();
        Console.WriteLine($"whipboss33: {(on ? "whip33 armed" : "RED side: NIGHT32's grapple")}");
        SurvivalSim.Wyrm24Dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Root, ".."));
        foreach (string boss in new[] { "lantern31", "stag", "wyrm31", "loom31", "crown" })
        {
            SurvivalSim sim;
            Func<Vec3> centre;
            Func<Vec3, Vec3, double> ray;
            Func<bool> alive;
            List<(string name, Vec3 at)> spots;
            if (boss == "crown")
            {
                GameConfig ccfg = CrownCfg32();
                sim = CrownSim32(ccfg, 1);
                if (sim == null) { Console.WriteLine("  crown: NOT BUILT"); continue; }
                Crown32 cr = sim.crown32;
                centre = () => cr.Centre;
                ray = cr.Raycast;
                alive = () => cr.Alive;
                spots = CrownSpots32(cr.arena, cr.origin);
            }
            else
            {
                CampRig31 rig = CampRig31For(boss, 1);
                if (rig == null) { Console.WriteLine($"  {boss}: NOT BUILT"); continue; }
                sim = rig.sim;
                centre = rig.centre;
                ray = rig.ray;
                alive = rig.alive;
                spots = rig.spots;
            }
            if (sim.arcline21 == null) { Console.WriteLine($"  {boss}: no ARCLINE in this rig's run (design.n21.arcline off)"); fails.Add(boss + ": no ARCLINE"); continue; }
            sim.Whip33Arm(on);
            sim.arcline21.Own(true);
            Func<Vec3> aimFor = centre;
            int seen = 0, lastPress = 0; // steps a wyrm piece was in sight and in range (aimFor found one); the step of the last press
            bool seenNow = true;
            if (boss == "wyrm31")
            {
                // THE KILN WYRM: the nearest piece that is not buried and that the eye sees (the head box is often under the floor or behind cover)
                Wyrm wy = sim.wyrm24;
                SurvivalSim sw = sim;
                aimFor = () =>
                {
                    Vec3 eye = sw.Eye, best = wy.boxes[0].c;
                    double bd = double.MaxValue, range = sw.cfg.movement.arcline.range;
                    for (int k = 0; k < wy.PieceCount; k++)
                    {
                        if (wy.buried[k]) continue;
                        Vec3 c = wy.boxes[k].c, dd = c - eye;
                        double l = dd.Length;
                        if (l < 4 || l > range) continue; // a piece closer than 4 m is already inside the stop gap: nothing to whip
                        if (sw.world.RayIntersect(eye, dd / l, out RayHit wh) && wh.distance < l - 0.5) continue;
                        if (l < bd) { bd = l; best = c; }
                    }
                    seenNow = bd < double.MaxValue;
                    if (seenNow) seen++;
                    return best;
                };
            }
            var si = (ScriptedInput)sim.input;
            double dt = sim.cfg.Dt, cr0 = sim.cfg.movement.body.capsuleRadius, stopGap = sim.cfg.Num("design.n33.whip33.stopGap");
            string took = "";
            sim.mover.Events += e => { if (e.type == MoverEventType.RopeFire) took = e.zip ? "enemy" : e.node ? "node" : "surface"; };
            for (int i = 0; i < spots.Count; i++)
            {
                var (name, at) = spots[i];
                sim.Respawn(at, 0);
                for (int s = 0; s < 30; s++) G32Tick(sim, si, centre(), false, false, false);
                if (sim.Grapple32On) sim.grappleCharges32 = sim.grapple32Rules.charges;
                took = "nothing";
                int arr0 = sim.whip33Arrivals, inside = 0, window = 0;
                seen = 0;
                lastPress = 0;
                seenNow = true;
                double latchGap = double.NaN;
                Action<Whip33Event> onLatch = e => { if (double.IsNaN(latchGap)) latchGap = e.gap; };
                sim.events.On<Whip33Event>("whip33Latch", onLatch);
                double least = double.MaxValue, stopTrue = double.NaN, stopGapE = double.NaN;
                string kind = "-";
                double radius = double.NaN;
                for (int s = 0; s < MovementMath.StepsFor(boss == "wyrm31" ? 20.0 : 8.0, dt) && alive() && !(sim.whip33Arrivals > arr0 && window == 0 && sim.mover.rope.phase == RopePhase.None); s++)
                {
                    if (s == 0 && boss == "wyrm31" && Flag(args, "--dbg"))
                    {
                        Wyrm w = sim.wyrm24;
                        Vec3 eye = sim.Eye, ap = w.body.AimPoint, h0 = w.boxes[0].c;
                        Vec3 look = GrappleAim.AimDirection(si.Yaw, si.Pitch);
                        double dA = Vec3.DistanceTo(ap, eye), cA = Vec3.Dot(ap - eye, look) / dA;
                        bool seesA = !sim.world.RayIntersect(eye, (ap - eye) / dA, out RayHit hA) || hA.distance >= dA - sim.cfg.movement.arcline.losSlack;
                        var peek = sim.arcline21.Peek(new InputFrame { yaw = si.Yaw, pitch = si.Pitch }, out Vec3 pp);
                        double dH = Vec3.DistanceTo(h0, eye); bool hitH = sim.world.RayIntersect(eye, (h0 - eye) / dH, out RayHit hH);
                        bool backHit = sim.world.RayIntersect(ap, (eye - ap) / dA, out RayHit hB); bool downHit = sim.world.RayIntersect(new Vec3(eye.x, eye.y + 3, eye.z), new Vec3(0, -1, 0), out RayHit hD);
                        Console.WriteLine($"    dbg back from the aim point: {(backHit ? hB.distance.ToString("0.0") + " of " + dA.ToString("0.0") : "clear")}; down from 3 m above the eye: {(downHit ? hD.distance.ToString("0.0") : "clear")}");
                        Console.WriteLine($"    dbg eye {eye.x:0.0},{eye.y:0.0},{eye.z:0.0}, feet {sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}, grounded {sim.mover.grounded}, spot {at.x:0.0},{at.y:0.0},{at.z:0.0}");
                        Console.WriteLine($"    dbg wall hit toward the aim point at {(seesA ? -1 : hA.distance):0.0} m (normal {hA.normal.x:0.0},{hA.normal.y:0.0},{hA.normal.z:0.0}); toward the head box ({dH:0.0} m): {(hitH ? hH.distance.ToString("0.0") : "clear")}");
                        Console.WriteLine($"    dbg wyrm phase {w.phase}, buried head {w.buried[0]}, head box {h0.x:0.0},{h0.y:0.0},{h0.z:0.0}; body.AimPoint {ap.x:0.0},{ap.y:0.0},{ap.z:0.0} at {dA:0.0} m, cos to the look {cA:0.000} (cone cos {Math.Cos(sim.cfg.movement.arcline.coneDeg * Math.PI / 360):0.000}), sees {seesA}; range {sim.cfg.movement.arcline.range}; Peek {peek}");
                    }
                    bool idle = sim.mover.rope.phase == RopePhase.None && sim.mover.rope.cooldownSteps == 0;
                    Vec3 aimNow = aimFor(); // (the wyrm: sets seenNow)
                    bool press = s == 0 || (idle && sim.whip33Arrivals == arr0 && (took == "surface" || took == "nothing") && seenNow && s - lastPress >= 100); // again (0.83 s apart: grapple32's gap) until a stop, the wyrm only with a piece in sight
                    if (press) lastPress = s;
                    if (press && s > 0 && boss == "wyrm31" && Flag(args, "--dbg")) { var pk = sim.arcline21.Peek(new InputFrame { yaw = si.Yaw, pitch = si.Pitch }, out Vec3 _); Console.WriteLine($"    dbg press at {s * dt:0.00} s: Peek {pk} (look before this tick), rope {sim.mover.rope.phase}, cooldown {sim.mover.rope.cooldownSteps}, charges {sim.grappleCharges32}, aim {Vec3.DistanceTo(aimNow, sim.Eye):0.0} m"); }
                    if (press && s > 0 && sim.Grapple32On) sim.grappleCharges32 = sim.grapple32Rules.charges;
                    // held while the press fires and while an enemy zip reels; a hook on a surface or a node is let go so the next press can try again
                    Rope rq = sim.mover.rope;
                    bool held = press || (rq.phase != RopePhase.None && took != "surface");
                    G32Tick(sim, si, aimNow, held, press, false);
                    Rope rp = sim.mover.rope;
                    bool pulling = rp.phase == RopePhase.Attached && rp.zip && sim.mover.zipAnchor != null; // a rope reeling the player to an enemy
                    window = pulling ? MovementMath.StepsFor(0.3, dt) : Math.Max(0, window - 1); // the pull and 0.3 s after it
                    Enemy t = sim.Whip33Target;
                    if (t != null) { kind = t.kind; radius = t.Radius; }
                    Vec3 chest = sim.mover.RopePoint, c = centre(), d = c - chest;
                    double len = d.Length;
                    double hit = len > 1e-9 ? ray(chest, d / len) : -1;
                    double trueGap = hit >= 0 ? hit - cr0 : (len < 0.5 ? -1 : double.NaN); // a miss right at the centre: inside
                    if (rp.phase != RopePhase.None && rp.zip && sim.mover.zipAnchor != null)
                    {
                        // the rope's anchor too (a zip to a piece that is not the centre's)
                        Vec3 d2 = rp.anchor - chest;
                        double l2 = d2.Length;
                        double h2 = l2 > 1e-9 ? ray(chest, d2 / l2) : -1;
                        double g2 = h2 >= 0 ? h2 - cr0 : (l2 < 0.3 ? -1 : double.NaN);
                        if (!double.IsNaN(g2) && (double.IsNaN(trueGap) || g2 < trueGap)) trueGap = g2;
                    }
                    if (!double.IsNaN(trueGap))
                    {
                        least = Math.Min(least, trueGap);
                        if (trueGap <= 0 && window > 0) inside++;
                    }
                    if (sim.whip33Arrivals > arr0 && double.IsNaN(stopTrue)) { stopTrue = trueGap; stopGapE = sim.whip33LastGap; }
                }
                // let the rope go before the next spot
                for (int s = 0; s < 30; s++) G32Tick(sim, si, null, false, false, false);
                int stops = sim.whip33Arrivals - arr0;
                Console.WriteLine($"  {boss,-9} {name,-44} took {took,-7} {(kind != "-" ? $"{kind} (Radius {radius:0.00})" : ""),-24} stops {stops}, gap at the stop {(double.IsNaN(stopGapE) ? "-" : stopGapE.ToString("0.00"))} (true {(double.IsNaN(stopTrue) ? "-" : stopTrue.ToString("0.00"))}) m, gap at the latch {(double.IsNaN(latchGap) ? "-" : latchGap.ToString("0.00"))} m, least true gap {(least == double.MaxValue ? "-" : least.ToString("0.00"))} m, steps inside during a pull (+0.3 s) {inside}{(boss == "wyrm31" ? $", steps with a piece in sight {seen}" : "")}");
                if (on && inside > 0) fails.Add($"{boss} {name}: {inside} steps inside");
                if (on && stops > 0 && !double.IsNaN(stopTrue) && stopTrue < Math.Min(stopGap, double.IsNaN(latchGap) ? stopGap : latchGap) - 0.05) fails.Add($"{boss} {name}: stop at true gap {stopTrue:0.00}");
            }
        }
        Console.WriteLine(fails.Count == 0 ? (on ? "whipboss33 GREEN" : "whipboss33 RED side printed") : $"whipboss33 {(on ? "FAIL" : "RED")}: {string.Join("; ", fails)}");
        code = on && fails.Count > 0 ? 1 : 0;
    }

}
