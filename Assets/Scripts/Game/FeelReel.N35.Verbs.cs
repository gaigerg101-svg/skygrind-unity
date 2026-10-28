// NIGHT35 builder T (NIGHT35.md method 1, PRINCIPLES 1: nothing the player had is lost): the Player scenario `verbs35` (PhaseN35T; N35TeachBody in
// FeelReel.N35.Teach.cs sends it here). It performs every verb of docs/VERBS.md's tables (rows 1-37; 38-39 are their own reels) once, scripted through the
// game's forced move / combat frames (the reel's input path; no device), in a CAMPAIGN run on the booted level (DOORFREE, god, the husks killed and frozen,
// an enemy spawned only for the whip), and prints per row
//   [reel] verbs35: <#> <verb> PASS|RED|SKIP <measured>
// then `[reel] verbs35: total <pass>/<performed>` (SKIP rows are not performed). One Player per map (L-046): the lead boots it as
//   -skygrindMap <L1 | L3 | L4 id> -skygrindReelOnly verbs35
// and the rows a map cannot hold SKIP with the reason (a weapon the unlock rows do not give on that level; a jump pad where none is placed). Verbs changed on
// purpose by DECIDE35 check the NEW behaviour: the KESTREL / THUNDERCLAP hold (D-8, charge35: no normal shot during the hold), the ride (D-6, wallride35:
// longer than 1.2 s; the jump-off faster than the ride), the grapple (D-2, grapple35: NOT owned before level 4 (foundry, iceline, kiln); on level 4 a tap at a
// bare wall pulls there) and the hold (D-9: a held grapple pulls straight, no swing). While such a row is off in this run the line is RED "not landed".
// The verb table here is VERBS.md's (#, verb); a row added to the doc needs its performance here (the doc says so: "updates this table and the reel together").
// Not run by its author (no Player in the worktree).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>docs/VERBS.md rows 1-37 (#, verb), in the doc's order.</summary>
        static readonly (int n, string verb)[] Verbs35Rows =
        {
            (1, "walk / strafe"), (2, "sprint"), (3, "jump"), (4, "double jump"), (5, "dash"), (6, "slide"), (7, "slam"), (8, "jump dash"), (9, "wall ride"),
            (10, "wall jump"), (11, "wall-ride jump-off boost"), (12, "bunny hop"), (13, "slide bounce"), (14, "zip carry / wall coyote / wall buffer / lurch"),
            (15, "jump pad"), (16, "glide rail / ice glide"), (17, "grapple pull to a node"), (18, "grapple pull to a bare wall"), (19, "grapple hold-swing"),
            (20, "the whip"), (21, "the whip readies the punch"), (22, "the BEND"), (23, "instant swap inside a bend"), (24, "punch"), (25, "KESTREL fire"),
            (26, "KESTREL zoom"), (27, "KESTREL charge"), (28, "PULSE gauntlet punch / LT blast"), (29, "THUNDERCLAP fire"), (30, "THUNDERCLAP charge"),
            (31, "THUNDERCLAP shockwave"), (32, "NAILGUN fire / LT"), (33, "TORCH fire / LT"), (34, "weapon swap"), (35, "weapon wheel"), (36, "ARSENAL / QUICKDRAW"),
            (37, "interact / buy"),
        };

        sealed class Verbs35State
        {
            public readonly Dictionary<MoverEventType, int> mv = new Dictionary<MoverEventType, int>();
            public readonly Dictionary<string, int> ev = new Dictionary<string, int>();
            public readonly Dictionary<string, int> fires = new Dictionary<string, int>();
            public readonly Dictionary<int, string> result = new Dictionary<int, string>();
            public double rideStartT = double.NaN, rideEndT = double.NaN, rideSpeed, wallJumpSpeed, slideSpeed;
            /// <summary>NIGHT36 M (wallgrip36, VERBS.md row 9): the current ride's attach height and the most any ride rose above its attach point.</summary>
            public double rideAttachY = double.NaN, rideRise;
            /// <summary>NIGHT36 M (tap36, VERBS.md row 25): the sim step of the last "fire" event.</summary>
            public int lastFireStep = -1;
            /// <summary>Every ride of the row-9 window: its WallRideStart and WallRideEnd sim times (end NaN: still riding).</summary>
            public readonly List<(double start, double end)> rides = new List<(double, double)>();
            /// <summary>Each WallRideEnd's reason (MoverEvent.reason), in order.</summary>
            public readonly List<string> rideEndReasons = new List<string>();
            public int pass, performed;
            public int M(MoverEventType t) => mv.TryGetValue(t, out int n) ? n : 0;
            public int E(string name) => ev.TryGetValue(name, out int n) ? n : 0;
            public int F(string gun) => fires.TryGetValue(gun, out int n) ? n : 0;
        }

        /// <summary>M (r3): ReaderInput applies forcedMove on EVERY sim step of a rendered frame, so a reel "press" repeated on 2-3 steps (a ground jump, then
        /// a double jump). Wrapped around the player's input for the jump rows (9-13): a button's `pressed` reaches the sim on the FIRST step of a run of
        /// pressed steps only (edge by sim step); held is untouched.</summary>
        sealed class Edge35Input : IPlayerInput
        {
            readonly IPlayerInput inner;
            bool jump, dash, slide, grapple, sprint;
            public Edge35Input(IPlayerInput inner) => this.inner = inner;
            public IPlayerInput Inner => inner;
            public double Yaw => inner.Yaw;
            public double Pitch => inner.Pitch;
            public void SetLook(double yaw, double pitch) => inner.SetLook(yaw, pitch);
            public bool Captured => inner.Captured;
            public CombatInput SampleCombat(bool active) => inner.SampleCombat(active);

            static void Edge(ref Btn b, ref bool was)
            {
                bool p = b.pressed;
                if (p && was) b.pressed = false;
                was = p;
            }

            public InputFrame SampleMove(bool active)
            {
                InputFrame f = inner.SampleMove(active);
                Edge(ref f.jump, ref jump);
                Edge(ref f.dash, ref dash);
                Edge(ref f.slide, ref slide);
                Edge(ref f.grapple, ref grapple);
                Edge(ref f.sprint, ref sprint);
                return f;
            }
        }

        /// <summary>NIGHT40 (DECIDE40 D-2): the reel stands on an n40 level whose start gives the gauntlet alone (n40-iceline): the F punch acts
        /// only with a gun in hand (Combat.cs), and the KESTREL returns in a later zone, so rows 24 / 40 / 41 / 42 cannot be performed at the start.</summary>
        static bool verbs40GauntletOnly;

        static void Verb35(Verbs35State v, int n, string status, string measured)
        {
            if (verbs40GauntletOnly && status == "RED" && (n == 24 || n == 40 || n == 41 || n == 42))
            {
                status = "SKIP"; // by design, not a lost verb: the gauntlet's own swing parries with foes40 (L2's walk g4 5/5 parries, E3's n40foes drone)
                measured = "the gauntlet is the only weapon at this n40 level's start (DECIDE40 D-2; the punch needs a gun in hand, the KESTREL returns in a later zone) | " + measured;
            }
            string verb = "?";
            foreach (var r in Verbs35Rows) if (r.n == n) verb = r.verb;
            foreach (var r in Verbs36Table) if (r.n == n) verb = r.verb; // NIGHT36 M (FeelReel.N36.Move.cs)
            foreach (var r in Verbs37Table) if (r.n == n) verb = r.verb; // NIGHT37 K (FeelReel.N37.Kit.cs)
            if (status == "PASS") { v.pass++; v.performed++; }
            else if (status == "RED") v.performed++;
            string line = $"{n} {verb} {status} {measured}";
            v.result[n] = line;
            Debug.Log("[reel] " + verbsTag + ": " + line);
        }

        /// <summary>Game seconds since `s0` (the sim's steps).</summary>
        static double T35(SurvivalGame g, int s0) => (g.sim.simStep - s0) * g.sim.step;

        /// <summary>Run `each(t)` every frame for `seconds` of game time (t: game seconds since the start), god mode on.</summary>
        static IEnumerator Do35(SurvivalGame g, double seconds, Action<double> each, double wallEnd)
        {
            int s0 = g.sim.simStep;
            while (T35(g, s0) < seconds && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                each(T35(g, s0));
                yield return null;
            }
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
        }

        static Btn Press(bool on) => new Btn { pressed = on, held = on };
        static Btn Hold(bool on) => new Btn { held = on };

        static Vec3 Fwd35(double yaw) => new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
        static double YawOf35(Vec3 d) => Math.Atan2(-d.x, -d.z);
        static double Flat35(Vec3 a, Vec3 b) => Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));

        /// <summary>The open floor every ground verb uses: the CLEAR LANE (r2: on n33-span the roomiest room's quarter point had a wall 2 m ahead: the dash
        /// read 2.05 m, the whip's husk sat behind it). From each non-boss room's spawn, 16 headings: knee and chest rays clear and the floor under the lane
        /// every 2 m (no pit, no lava drop); the longest lane wins (14 m enough). Falls back to the roomiest room's quarter point.</summary>
        static (Vec3 at, double yaw, RoomDef room) Open35(SurvivalSim sim)
        {
            Vec3 bestAt = default;
            double bestYaw = 0, bestLen = 0;
            RoomDef bestRoom = null;
            // NIGHT57 (the lead, D-16): never stage before the level's first award room (n57-kiln's new kb2 has a 21 m lane before kb2b's
            // KESTREL award, so the reel punched with the gauntlet in hand: rows 40 / 41 RED, 25-27 SKIP). With no award room: as before.
            string firstAward = sim.FirstAwardRoom57;
            bool reached = firstAward == null || sim.campaignLevel.rooms.FindIndex(x => x.id == firstAward) < 0;
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                if (!reached && r.id == firstAward) reached = true;
                if (!reached) continue;
                if (r.kind == "boss") continue;
                Vec3 o = r.spawn;
                for (int k = 0; k < 16; k++)
                {
                    double yaw = k * Math.PI / 8;
                    Vec3 f = Fwd35(yaw);
                    double clear = 30;
                    foreach (double h in new[] { 0.5, 1.5 })
                        if (sim.world.RayIntersect(new Vec3(o.x, o.y + h, o.z), f, out RayHit hit)) clear = Math.Min(clear, hit.distance);
                    double len = 0;
                    for (double d = 2; d <= clear - 1.5; d += 2)
                    {
                        var p = new Vec3(o.x + f.x * d, o.y + 1.0, o.z + f.z * d);
                        if (!sim.world.RayIntersect(p, new Vec3(0, -1, 0), out RayHit dh) || dh.distance > 1.6) break;
                        len = d;
                    }
                    if (len > bestLen) { bestLen = len; bestAt = new Vec3(o.x, o.y + 0.2, o.z); bestYaw = yaw; bestRoom = r; }
                }
                if (bestLen >= 14) break;
            }
            if (bestRoom != null && bestLen >= 8) return (bestAt, bestYaw, bestRoom);
            return Roomiest35(sim);
        }

        static (Vec3 at, double yaw, RoomDef room) Roomiest35(SurvivalSim sim)
        {
            RoomDef best = null;
            double area = -1;
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                if (r.kind == "boss") continue;
                double a = (r.x1 - r.x0) * (r.z1 - r.z0);
                if (a > area) { area = a; best = r; }
            }
            if (best == null) best = sim.campaignLevel.rooms[0];
            bool alongX = best.x1 - best.x0 >= best.z1 - best.z0;
            // start a quarter in from one end, facing the far end
            Vec3 at = alongX ? new Vec3(best.x0 + (best.x1 - best.x0) * 0.25, best.spawn.y + 0.2, (best.z0 + best.z1) / 2) : new Vec3((best.x0 + best.x1) / 2, best.spawn.y + 0.2, best.z0 + (best.z1 - best.z0) * 0.25);
            double yaw = alongX ? YawOf35(new Vec3(1, 0, 0)) : YawOf35(new Vec3(0, 0, 1));
            return (at, yaw, best);
        }

        /// <summary>The longest straight wall found by rays from each room's centre: a start point 0.7 m off the wall, the direction along it, its normal and
        /// the clear length ahead (rays back to the wall every metre).</summary>
        /// <summary>NIGHT37 K (verbs37 n37-r5: rows 13 / 18 RED on n37-span and n37-spire): a floor within 1.6 m under (x, y + 1, z), the clear lane's own floor
        /// test (Open35). The longest wall there was in a NIGHT37 rehearsal room (o1, v9) whose spot 3.3 m / 9 m out is over a drop: the player fell before the
        /// slide met the wall and before the grapple tap (harness `verbs37probe`: feet 3.1 m under the spot, lowest y -14).</summary>
        static bool Floor35(SurvivalSim sim, double x, double y, double z) =>
            sim.world.RayIntersect(new Vec3(x, y + 1.0, z), new Vec3(0, -1, 0), out RayHit dh) && dh.distance <= 1.6 && dh.normal.y > 0.7;

        static bool Wall35(SurvivalSim sim, out Vec3 start, out Vec3 along, out Vec3 normal, out double length) => Wall35(sim, false, out start, out along, out normal, out length);

        /// <summary>floors: only a wall whose rows' spots all have a floor under them (rows 13 and 18). Without floors (rows 9-11, the rides): NIGHT39 V,
        /// the run stops at a noride face or a lane with no floor (n38-span's noride liner, n38-foundry's GANTRY pit: R1's measurement). The search lives
        /// in Bot/VerbsWall39.cs so the harness runs the same pick (`n39views wall`).</summary>
        static bool Wall35(SurvivalSim sim, bool floors, out Vec3 start, out Vec3 along, out Vec3 normal, out double length) =>
            VerbsWall39.Pick(sim, floors, !floors, out start, out along, out normal, out length, out string _);

        /// <summary>A grapple node in clear sight of a floor spot 8-18 m from it (the node, the spot, the yaw / pitch that aim at it).</summary>
        static bool Node35(SurvivalSim sim, out Vec3 node, out Vec3 from, out double yaw, out double pitch) => Node35(sim, false, out node, out from, out yaw, out pitch);

        /// <summary>floors (NIGHT37 K, row 19): only a spot with a floor under it (on n37-spire / n33-spire the first spot was in the air: the held rope let go
        /// 0.08 s in and the pull never reached the node; harness `verbs37probe`).</summary>
        static bool Node35(SurvivalSim sim, bool floors, out Vec3 node, out Vec3 from, out double yaw, out double pitch)
        {
            node = from = default;
            yaw = pitch = 0;
            foreach (Vec3 nd in sim.map.grappleNodes)
            {
                foreach (RoomDef r in sim.campaignLevel.rooms)
                {
                    if (nd.x < r.x0 - 2 || nd.x > r.x1 + 2 || nd.z < r.z0 - 2 || nd.z > r.z1 + 2) continue;
                    for (int k = 0; k < 8; k++)
                    {
                        double a = k * Math.PI / 4;
                        var f = new Vec3(nd.x + Math.Sin(a) * 10, r.spawn.y + 0.2, nd.z + Math.Cos(a) * 10);
                        if (f.x < r.x0 + 1 || f.x > r.x1 - 1 || f.z < r.z0 + 1 || f.z > r.z1 - 1) continue;
                        var eye = new Vec3(f.x, f.y + 1.6, f.z);
                        Vec3 to = nd - eye;
                        double len = to.Length;
                        if (sim.world.RayIntersect(eye, to / len, out RayHit h) && h.distance < len - 0.5) continue;
                        if (floors && !Floor35(sim, f.x, r.spawn.y, f.z)) continue;
                        node = nd;
                        from = f;
                        yaw = YawOf35(to);
                        pitch = Math.Atan2(to.y, Math.Sqrt(to.x * to.x + to.z * to.z));
                        return true;
                    }
                }
            }
            return false;
        }

        static bool Owns35(SurvivalGame g, string gun) => g.loadout.owned.IndexOf(gun) >= 0;

        /// <summary>Put an OWNED gun in hand (never adds one: the unlock rows decide what the level gives).</summary>
        static IEnumerator Hand35(SurvivalGame g, string gun, double wallEnd)
        {
            int slot = g.loadout.owned.IndexOf(gun);
            if (slot < 0) yield break;
            int s0 = g.sim.simStep;
            while ((g.loadout.Weapon != gun || T35(g, s0) < 0.3) && T35(g, s0) < 3 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                g.forcedCombat = new CombatInput { weapon = g.loadout.Weapon == gun ? -1 : slot };
                yield return null;
            }
            g.forcedCombat = IdleCombat;
        }

        /// <summary>The charge check (rows 27, 30): hold fire `hold` s, release, count the gun's normal shots during the hold and the charge events.</summary>
        static IEnumerator Charge35(SurvivalGame g, Verbs35State v, int row, string gun, double wallEnd)
        {
            yield return Hand35(g, gun, wallEnd);
            // the gun settles: its own interval, and the charge's cooldown after an earlier charged shot (design.n35.charge35.<gun>.cooldownS: KESTREL 2.5 s,
            // THUNDERCLAP 3.0 s; a press inside it is normal fire by design)
            string cdKey = "design.n35.charge35." + gun + ".cooldownS";
            double settle = Math.Max(0.6, (g.cfg.Has(cdKey) ? g.cfg.Num(cdKey) : 0) + 0.1);
            yield return Do35(g, settle, t => { }, wallEnd);
            // W (b34d9163): with charge35 on every charge emits BOTH chargeShot27Start and charge35Start (and both Fire events): count charge35's only (off:
            // chargeShot27's), else one charge reads "starts 2 / charged shots 2". No earlier row fires a charged shot, so the 2.5 / 3.0 s cooldown after one
            // is not in the way; the 0.6 s settle covers the gun's own interval.
            bool c35 = g.sim.n35.charge35;
            string startEv = c35 ? "charge35Start" : "chargeShot27Start", fireEv = c35 ? "charge35Fire" : "chargeShot27Fire";
            int f0 = v.F(gun), c0 = v.E(startEv), cf0 = v.E(fireEv);
            // VERBS.md rows 27 / 30 (D-8, charge35): 0 normal shots BEFORE the first charged shot, and one charged shot. The hold ends before the charge
            // fires itself (full + design.n35.charge35.holdMaxS, about 1.5 s): hold 0.9 s, release. After a charged shot a still-held trigger is normal fire
            // in the cooldown by design ("a held trigger never stalls"), so only the shots before the first charged one count. A frame whose steps hold
            // the charged shot is not counted (its own "fire", if it emits one, is the charged shot).
            double hold = 0.9;
            // NIGHT40 W1 (VERBS.md rows 27 / 30, changed by charge40): FULL takes 0.9 s / 1.2 s, so the hold is FULL + 0.25 s (still under FULL +
            // holdMaxS 1.0); a THUNDERCLAP hold that starts on the ground fires no volley first (p39's rule), one in the air (tap36)
            bool c40 = g.sim.charge35 != null && g.sim.charge35.c40;
            ChargeShots27 cs27 = g.sim.loadout != null ? g.sim.loadout.charge27 : null;
            if (c40 && cs27 != null && ChargeShots27.Index(gun) >= 0) hold = cs27.FullSteps(ChargeShots27.Index(gun)) / 120.0 + 0.25;
            bool groundedAtPress = g.sim.mover != null && g.sim.mover.grounded;
            int before = 0, lastF = f0, lastCf = cf0;
            bool chargedSeen = false;
            yield return Do35(g, hold + 0.8, t =>
            {
                int f = v.F(gun), cf = v.E(fireEv);
                if (!chargedSeen && cf == lastCf) before += f - lastF;
                if (cf > cf0) chargedSeen = true;
                lastF = f;
                lastCf = cf;
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = t < 0.02, held = t < hold } };
            }, wallEnd);
            int f1 = v.F(gun), cf1 = v.E(fireEv);
            if (!chargedSeen && cf1 == lastCf) before += f1 - lastF;
            int starts = v.E(startEv) - c0, fired = cf1 - cf0;
            string m = $"held {hold:0.0} s, released: charge starts {starts}, charged shots {fired}, normal shots before the first charged shot {before} (charge35 {g.sim.n35.charge35})";
            if (!g.sim.n35.charge35) Verb35(v, row, "RED", "not landed (charge35 off, D-8): " + m);
            else
            {
                // NIGHT39 P1 (VERBS.md row 27, changed by p39): with design.n39.p39 live the KESTREL's hold fires NO tap first (the THUNDERCLAP keeps tap36)
                bool p39 = gun == "revolver" && g.sim.charge35 != null && g.sim.charge35.p39;
                int wantBefore = !p39 && g.sim.charge35 != null && g.sim.charge35.tap36 ? 1 : 0; // NIGHT36 M (VERBS.md row 27, changed by F-1): tap36's press fires one normal shot first
                if (gun == "shotgun" && c40 && groundedAtPress) wantBefore = 0; // NIGHT40 W1 (row 30, changed by charge40): a ground hold charges with no volley first
                m += $" (charge40 {c40}, grounded at the press {groundedAtPress})";
                Verb35(v, row, fired == 1 && starts == 1 && before == wantBefore ? "PASS" : "RED", m + $" (want {wantBefore} normal before: tap36 {g.sim.charge35 != null && g.sim.charge35.tap36}, p39 {p39})");
            }
        }

        static IEnumerator Verbs35Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot -skygrindMap <L1 | L3 | L4 id> -skygrindReelOnly verbs35";
            var why = new string[1];
            bool roWas = g.profileReadOnly;
            offs.Add(() => g.profileReadOnly = roWas);
            g.profileReadOnly = true;
            yield return Campaign35(g, offs, why);
            if (why[0] != null) { res[0] = why[0] + " (" + boot + ")"; yield break; }
            SurvivalSim sim = g.sim;
            Mover m = sim.mover;
            var v = new Verbs35State();
            string key = Teach35Table.LevelKey(sim.campaignLevel.id);
            // before the grapple's level: M's gate when grapple35 is on (the level's place in the campaign order < fromLevel: SurvivalSim.Move35.cs), else the
            // level names (L1-L3) so the off run still prints "not landed"
            bool beforeGrapple = sim.n35.grapple35 ? !sim.grapple35Here : key == "foundry" || key == "iceline" || key == "kiln" || key == "tutorial";
            Action<MoverEvent> onMove = e =>
            {
                v.mv[e.type] = v.M(e.type) + 1;
                double now = sim.simStep * sim.step;
                if (e.type == MoverEventType.WallRideStart) { v.rideAttachY = m.position.y; if (double.IsNaN(v.rideStartT)) v.rideSpeed = e.speed; if (double.IsNaN(v.rideStartT)) v.rideStartT = now; v.rideEndT = double.NaN; v.rides.Add((now, double.NaN)); }
                if (e.type == MoverEventType.WallRideEnd)
                {
                    v.rideEndT = now;
                    v.rideEndReasons.Add(e.reason ?? "?");
                    int last = v.rides.Count - 1;
                    if (last >= 0 && double.IsNaN(v.rides[last].end)) v.rides[last] = (v.rides[last].start, now);
                }
                if (e.type == MoverEventType.WallJump) v.wallJumpSpeed = e.speed;
                if (e.type == MoverEventType.SlideStart) v.slideSpeed = e.speed;
            };
            m.Events += onMove;
            var names = new[] { "punch", "chargeShot27Start", "chargeShot27Fire", "charge35Start", "charge35Fire", "whip33Latch", "bend34Start", "weaponSwap", N20Events.Shockwave, "pulse27Blast", "parry", "shotHit", "meleeSwing", "arclinePull" };
            var handlers = new List<(string, Action<object>)>();
            foreach (string nm in names)
            {
                string k = nm;
                Action<object> h = _ => v.ev[k] = v.E(k) + 1;
                sim.events.On(k, h);
                handlers.Add((k, h));
            }
            Action<object> onFire = o => { string gun = o as string ?? "?"; v.fires[gun] = v.F(gun) + 1; v.lastFireStep = sim.simStep; };
            sim.events.On("fire", onFire);
            offs.Add(() =>
            {
                m.Events -= onMove;
                foreach (var (k, h) in handlers) sim.events.Off(k, h);
                sim.events.Off("fire", onFire);
                g.husks.frozen = false;
            });
            g.husks.KillAll();
            g.husks.frozen = true;
            var (open, openYaw, openRoom) = Open35(sim);
            bool wallOk = Wall35(sim, out Vec3 wStart, out Vec3 wAlong, out Vec3 wNormal, out double wLen);
            string pc = $"level {sim.campaignLevel.id} ({key}), owned [{string.Join(", ", g.loadout.owned)}], grapple owned {(sim.arcline21 != null && sim.arcline21.Owned)}, n35 {sim.n35}, open floor {openRoom.id}, wall {(wallOk ? wLen.ToString("0", ci) + " m" : "none >= 8 m")}";
            Debug.Log("[reel] " + verbsTag + ": start " + pc);
            Start(g, "verbs35", "camera", dir);
            yield return Frames(pre);
            double wallEnd = Time.realtimeSinceStartup + 900;

            IEnumerator Reset35(Vec3 at, double yaw, double pitch = 0)
            {
                g.forcedMove = new InputFrame();
                g.forcedCombat = IdleCombat;
                g.Respawn(at, yaw * 180 / Math.PI);
                g.input.SetLook(yaw, pitch);
                yield return Do35(g, 0.35, t => { }, wallEnd);
            }

            // ---- 1 walk ----
            yield return Reset35(open, openYaw);
            Vec3 p0 = m.position;
            double walkMax = 0;
            yield return Do35(g, 1.0, t => { g.forcedMove = new InputFrame { moveZ = 1 }; if (t > 0.5) walkMax = Math.Max(walkMax, m.Speed); }, wallEnd);
            double walked = Flat35(p0, m.position);
            Verb35(v, 1, walked > 2 ? "PASS" : "RED", $"{walked.ToString("0.00", ci)} m in 1 s of forward (top speed {walkMax.ToString("0.00", ci)} m/s)");
            // ---- 2 sprint ----
            yield return Reset35(open, openYaw);
            double sprintMax = 0;
            yield return Do35(g, 1.5, t => { g.forcedMove = new InputFrame { moveZ = 1, sprint = new Btn { pressed = t < 0.02, held = true } }; if (t > 0.8) sprintMax = Math.Max(sprintMax, m.Speed); }, wallEnd);
            Verb35(v, 2, sprintMax > walkMax * 1.03 ? "PASS" : "RED", $"top speed {sprintMax.ToString("0.00", ci)} m/s sprinting vs {walkMax.ToString("0.00", ci)} walking");
            // ---- 3 jump ----
            yield return Reset35(open, openYaw);
            int j0 = v.M(MoverEventType.Jump);
            double y0 = m.position.y, yTop = y0;
            yield return Do35(g, 0.6, t => { g.forcedMove = new InputFrame { jump = new Btn { pressed = t < 0.02, held = t < 0.2 } }; yTop = Math.Max(yTop, m.position.y); }, wallEnd);
            Verb35(v, 3, v.M(MoverEventType.Jump) > j0 && yTop - y0 > 0.5 ? "PASS" : "RED", $"Jump events {v.M(MoverEventType.Jump) - j0}, rise {(yTop - y0).ToString("0.00", ci)} m");
            // ---- 4 double jump ----
            yield return Reset35(open, openYaw);
            int dj0 = v.M(MoverEventType.DoubleJump);
            yield return Do35(g, 0.9, t => g.forcedMove = new InputFrame { jump = new Btn { pressed = t < 0.02 || (t >= 0.35 && t < 0.37), held = t < 0.15 || (t >= 0.35 && t < 0.5) } }, wallEnd);
            Verb35(v, 4, v.M(MoverEventType.DoubleJump) > dj0 ? "PASS" : "RED", $"DoubleJump events {v.M(MoverEventType.DoubleJump) - dj0}");
            // ---- 5 dash ----
            yield return Reset35(open, openYaw);
            int d0 = v.M(MoverEventType.Dash);
            Vec3 dashFrom = default;
            double dashDist = 0;
            bool dashSeen = false;
            yield return Do35(g, 0.8, t =>
            {
                g.forcedMove = new InputFrame { moveZ = 1, dash = new Btn { pressed = t >= 0.1 && t < 0.12, held = t >= 0.1 && t < 0.2 } };
                if (!dashSeen && v.M(MoverEventType.Dash) > d0) { dashSeen = true; dashFrom = m.position; }
                if (dashSeen && t < 0.5) dashDist = Flat35(dashFrom, m.position);
            }, wallEnd);
            Verb35(v, 5, dashSeen && dashDist > 4 ? "PASS" : "RED", $"Dash events {v.M(MoverEventType.Dash) - d0}, {dashDist.ToString("0.00", ci)} m in about 0.3 s after it");
            // ---- 6 slide ----
            yield return Reset35(open, openYaw);
            int ss0 = v.M(MoverEventType.SlideStart), se0 = v.M(MoverEventType.SlideEnd);
            double slideTop = 0;
            yield return Do35(g, 1.6, t =>
            {
                bool sl = t >= 0.5 && t < 1.2;
                g.forcedMove = new InputFrame { moveZ = 1, slide = new Btn { pressed = t >= 0.5 && t < 0.52, held = sl } };
                if (sl) slideTop = Math.Max(slideTop, m.Speed);
            }, wallEnd);
            bool slideOk = v.M(MoverEventType.SlideStart) > ss0 && v.M(MoverEventType.SlideEnd) > se0 && slideTop > walkMax;
            Verb35(v, 6, slideOk ? "PASS" : "RED", $"SlideStart {v.M(MoverEventType.SlideStart) - ss0}, SlideEnd {v.M(MoverEventType.SlideEnd) - se0}, top slide speed {slideTop.ToString("0.00", ci)} m/s (walk {walkMax.ToString("0.00", ci)})");
            // ---- 7 slam ----
            yield return Reset35(open, openYaw);
            int sm0 = v.M(MoverEventType.Slam);
            yield return Do35(g, 1.2, t => g.forcedMove = new InputFrame { jump = new Btn { pressed = t < 0.02, held = t < 0.2 }, slide = new Btn { pressed = t >= 0.4 && t < 0.42, held = t >= 0.4 && t < 0.8 } }, wallEnd);
            Verb35(v, 7, v.M(MoverEventType.Slam) > sm0 ? "PASS" : "RED", $"Slam events {v.M(MoverEventType.Slam) - sm0}");
            // ---- 8 jump dash ----
            yield return Reset35(open, openYaw);
            int jd0 = v.M(MoverEventType.DashJump) + v.M(MoverEventType.BoostJump);
            yield return Do35(g, 1.0, t => g.forcedMove = new InputFrame { moveZ = 1, dash = new Btn { pressed = t >= 0.1 && t < 0.12, held = t >= 0.1 && t < 0.2 }, jump = new Btn { pressed = t >= 0.18 && t < 0.2, held = t >= 0.18 && t < 0.4 } }, wallEnd);
            int jd = v.M(MoverEventType.DashJump) + v.M(MoverEventType.BoostJump) - jd0;
            Verb35(v, 8, jd > 0 ? "PASS" : "RED", $"DashJump + BoostJump events {jd} (dash at 0.10 s, jump at 0.18 s)");

            // rows 9-13: presses are edges by SIM STEP (Edge35Input around the player's input; put back after row 13 and in the offs)
            IPlayerInput plainInput = sim.input;
            sim.UseInput(new Edge35Input(plainInput));
            offs.Add(() => { if (sim.input is Edge35Input ei) sim.UseInput(ei.Inner); });
            // ---- 9 wall ride, 11 jump-off boost ----
            if (!wallOk)
            {
                Verb35(v, 9, "RED", "no straight wall of 8 m or more found by the rays (the level's rooms)");
                Verb35(v, 11, "RED", "no wall found");
            }
            else
            {
                // V's look35 approach (r2: it rides): 0.7 m off the wall, already running along it at 11 m/s with a 0.5 m/s lean into it (set only in the
                // first 0.05 s and the vertical speed KEPT: r2 zeroed vy there and killed the jump: 0 rides on foundry), jump pressed at once and HELD,
                // the stick forward and half into the wall (moveX toward it), the look along the wall.
                double ry = YawOf35(wAlong);
                Vec3 rightV = MovementMath.WishDirection(1, 0, ry);
                double into = rightV.x * wNormal.x + rightV.z * wNormal.z > 0 ? -1 : 1; // strafe toward the wall
                yield return Reset35(wStart, ry);
                int r0 = v.M(MoverEventType.WallRideStart);
                v.rideStartT = v.rideEndT = double.NaN;
                v.rides.Clear();
                v.rideEndReasons.Clear();
                double t0Sim = sim.simStep * sim.step;
                // the wall's height at the start point: rays into the wall every 0.5 m up from the floor, the highest that still hits within 1.4 m
                double wallH = 0;
                for (double hy = 0.5; hy <= 20; hy += 0.5)
                {
                    if (!sim.world.RayIntersect(new Vec3(wStart.x, wStart.y + hy, wStart.z), new Vec3(-wNormal.x, 0, -wNormal.z), out RayHit hh) || hh.distance > 1.4) break;
                    wallH = hy;
                }
                // M (r2 note): a hold ride runs 10.4-11.7 m/s; let go (wallLen - 3) / 11.7 s after WallRideStart (at least 1.3 s, so row 9 can pass), before
                // the ride reaches the wall's end (a ride already ended has no jump-off). rideHold counts from WallRideStart (or from the start if none).
                double offSpeed = 0, preSpeed = 0, rideHold = Math.Max(1.3, (wLen - 3) / 11.7);
                bool released = false;
                double releasedAt = -1, releaseSimT = double.NaN;
                yield return Do35(g, rideHold + 1.2, t =>
                {
                    if (t < 0.05) m.velocity = new Vec3(wAlong.x * 11 - wNormal.x * 0.5, m.velocity.y, wAlong.z * 11 - wNormal.z * 0.5);
                    if (m.state == MoveState.WallRide && !double.IsNaN(v.rideAttachY)) v.rideRise = Math.Max(v.rideRise, m.position.y - v.rideAttachY); // NIGHT36 M (wallgrip36)
                    double onWall = double.IsNaN(v.rideStartT) ? t : sim.simStep * sim.step - v.rideStartT;
                    // let go at the planned time since the FIRST start, or once no ride has been on for 0.15 s (r3: a ride that ended and restarted at once
                    // released the jump on the first end)
                    bool off = !double.IsNaN(v.rideEndT) && sim.simStep * sim.step - v.rideEndT > 0.15;
                    bool hold = !released && onWall < rideHold && !off;
                    if (!hold && !released) { released = true; releasedAt = t; releaseSimT = sim.simStep * sim.step; preSpeed = Math.Sqrt(m.velocity.x * m.velocity.x + m.velocity.y * m.velocity.y + m.velocity.z * m.velocity.z); }
                    g.input.SetLook(ry, 0);
                    g.forcedMove = new InputFrame { moveZ = 1, moveX = released ? 0 : into * 0.5, jump = new Btn { pressed = t < 0.02, held = hold } };
                    if (released && t - releasedAt > 0.02 && t - releasedAt < 0.25) offSpeed = Math.Max(offSpeed, Math.Sqrt(m.velocity.x * m.velocity.x + m.velocity.y * m.velocity.y + m.velocity.z * m.velocity.z));
                }, wallEnd);
                bool rode = v.M(MoverEventType.WallRideStart) > r0;
                // the LONGEST SINGLE ride: a WallRideStart to its own WallRideEnd (one still on at the window's end: to the release, else the window's end)
                double nowEnd = sim.simStep * sim.step, rideS = 0;
                var spans = new List<string>();
                foreach (var (st, en) in v.rides)
                {
                    double e = !double.IsNaN(en) ? en : !double.IsNaN(releaseSimT) && releaseSimT > st ? releaseSimT : nowEnd;
                    rideS = Math.Max(rideS, e - st);
                    spans.Add($"{(st - t0Sim).ToString("0.00", ci)}-{(double.IsNaN(en) ? "on" : (en - t0Sim).ToString("0.00", ci))}");
                }
                string rm = $"WallRideStart {v.M(MoverEventType.WallRideStart) - r0}, longest single ride {rideS.ToString("0.00", ci)} s on a {wLen.ToString("0", ci)} m wall {wallH.ToString("0.0", ci)} m high at the start; rides start-end s [{string.Join(", ", spans)}], end reasons [{string.Join(", ", v.rideEndReasons)}], released at {(double.IsNaN(releaseSimT) ? "-" : (releaseSimT - t0Sim).ToString("0.00", ci))} (planned {rideHold.ToString("0.0", ci)} s after the first start; wallride35 {sim.n35.wallride35})";
                bool grip36 = sim.n36.wallgrip36; // NIGHT36 M (VERBS.md row 9, changed by F-5): a jump into the wall rises at most 1.5 m above the attach point
                rm += $"; the most a ride rose above its attach point {v.rideRise.ToString("0.00", ci)} m (wallgrip36 {grip36}{(grip36 ? ": at most 1.5" : "")})";
                if (!sim.n35.wallride35) Verb35(v, 9, "RED", "not landed (wallride35 off, D-6): " + rm);
                else Verb35(v, 9, rode && rideS > 1.2 && (!grip36 || v.rideRise <= 1.5) ? "PASS" : "RED", rm);
                string om = $"speed in the 0.25 s after letting go {offSpeed.ToString("0.00", ci)} m/s vs {preSpeed.ToString("0.00", ci)} m/s on the wall the frame before (entry {v.rideSpeed.ToString("0.00", ci)})";
                if (!sim.n35.wallride35) Verb35(v, 11, "RED", "not landed (wallride35 off): " + om);
                else Verb35(v, 11, rode && offSpeed > preSpeed ? "PASS" : "RED", om);
            }
            // ---- 10 wall jump: face the wall from 1.2 m, jump, jump again at it ----
            if (wallOk)
            {
                var face = new Vec3(-wNormal.x, 0, -wNormal.z);
                var near = new Vec3(wStart.x + wAlong.x * 2 + wNormal.x * 0.5, wStart.y, wStart.z + wAlong.z * 2 + wNormal.z * 0.5);
                yield return Reset35(near, YawOf35(face));
                int wj0 = v.M(MoverEventType.WallJump);
                yield return Do35(g, 1.0, t => g.forcedMove = new InputFrame { moveZ = 1, jump = new Btn { pressed = t < 0.02 || (t >= 0.3 && t < 0.32), held = t < 0.15 || (t >= 0.3 && t < 0.45) } }, wallEnd);
                Verb35(v, 10, v.M(MoverEventType.WallJump) > wj0 ? "PASS" : "RED", $"WallJump events {v.M(MoverEventType.WallJump) - wj0} (jump, then jump again against the wall)");
            }
            else Verb35(v, 10, "RED", "no wall found");
            // ---- 12 bunny hop: a ground jump, then each landing's jump pressed inside the buffer ----
            // NIGHT72 N1 (HopSite72.cs): the BAR is the same (3 hops, each keeping >= 99 % of the speed). The window grows to three hops of the profile's air time (the
            // ULTRAKILL profile's 1.4 s flat jump: the old 4 s could not hold them), and a run that misses the bar is tried again on up to Hop72Retries other places
            // (the longest clear lanes of the level's rooms; Open35's lane can end in a wall inside three hops of 12 m); every attempt is printed in the line.
            double hopWindow = HopSite72.Window(m.Config);
            var hopSites = new List<(Vec3 at, double yaw, string room, double lane)> { (open, openYaw, openRoom.id, -1) };
            var attempts = new List<string>();
            string hopMeasured = null, hopLastSite = "";
            bool hopPass = false;
            // one run of the row at hopSites[ha] (the old body, as a local iterator: Open35's place now, the retries after the last row)
            IEnumerator HopAttempt(int ha)
            {
                    yield return Reset35(hopSites[ha].at, hopSites[ha].yaw);
                    var hops = new List<double>();
                    var hopSteps = new List<int>(); // the sim step of each counted hop: a retry place must show real hops (a pogo under a low ceiling is not one)
                    Action<MoverEvent> hop = e => { if (e.type == MoverEventType.Jump && e.speedBefore > 0.5 && hops.Count < 3) { hops.Add(100 * e.speedAfter / e.speedBefore); hopSteps.Add(sim.simStep); } };
                    m.Events += hop;
                    bool first = true, pressedAir = false;
                    double floorY = m.position.y, heldUntil = -1;
                    int s0 = sim.simStep;
                    yield return Do35(g, hopWindow, t =>
                    {
                        bool press = false;
                        if (hops.Count >= 3) { g.forcedMove = new InputFrame { moveZ = 1 }; return; }
                        if (first && m.grounded && t > 0.3) { press = true; first = false; }
                        else if (!first && !m.grounded && m.velocity.y < 0 && !pressedAir && m.position.y - floorY <= -m.velocity.y * 0.05) { press = true; pressedAir = true; }
                        else if (!first && m.grounded && !pressedAir && t >= heldUntil) press = true;
                        if (m.grounded && !press) pressedAir = false;
                        if (press) heldUntil = t + 0.15;
                        g.forcedMove = new InputFrame { moveZ = 1, jump = new Btn { pressed = press, held = press || t < heldUntil } };
                    }, wallEnd);
                    m.Events -= hop;
                    double min = hops.Count > 0 ? 1e9 : 0;
                    foreach (double h in hops) min = Math.Min(min, h);
                    hopPass = hops.Count >= 3 && min >= 99.0;
                    // a retry place counts only with real hops: each gap between two hops at least half the profile's flat air time (Open35's own place keeps today's rule)
                    if (hopPass && ha > 0)
                        for (int hi = 1; hi < hopSteps.Count; hi++)
                            if ((hopSteps[hi] - hopSteps[hi - 1]) * sim.step < UnityTuning.Hop72RealGapOfAir * HopSite72.AirTime(m.Config)) { hopPass = false; attempts.Add($"room {hopSites[ha].room}: hops {((hopSteps[hi] - hopSteps[hi - 1]) * sim.step).ToString("0.00", ci)} s apart (a low ceiling, not a hop)"); break; }
                    string site = $"room {hopSites[ha].room}{(ha == 0 ? " (Open35)" : " lane " + hopSites[ha].lane.ToString("0", ci) + " m")}";
                    hopMeasured = $"{hops.Count} hops, speed kept per hop {string.Join(" / ", hops.ConvertAll(h => h.ToString("0.0", ci)))} % (flow34 {sim.n34.flow34})";
                    attempts.Add($"{site}: {hops.Count} hops");
                    hopLastSite = site;
            }
            string HopLine() => hopMeasured + $"; window {hopWindow.ToString("0.0", ci)} s at {hopLastSite}" + (attempts.Count > 1 ? "; attempts: " + string.Join(" | ", attempts) : "");
            // NIGHT72 N1: Open35's place runs here, as the row always did. A retry's Respawn into another room starts that room's fight and moves the campaign on, which put rows 40-42 RED on iceline and kiln
            // (the two levels whose first run missed the bar: nothing was reachable for the parry, launch and heal rows): so a miss is held, the other rows run, and the retries come after the last of them.
            yield return HopAttempt(0);
            bool hopDeferred = !hopPass;
            if (!hopDeferred) Verb35(v, 12, "PASS", HopLine());
            // ---- 13 slide bounce: M's flow34 harness shape (Harness.N34.FlowChecks.cs Flow34SlideBounce): a slide on the floor heading 45 deg into the wall
            // at about 9.9 m/s, the jump pressed on the FIRST wall contact (Mover.touchingWall); the horizontal speed just after vs just before the press ----
            // NIGHT37 K: rows 13 and 18 stand on a wall whose spots have a floor (Wall35 floors: n37-r5's RED on n37-span / n37-spire was the player falling
            // off a rehearsal room's ledge, not the verb)
            bool wallOkF = Wall35(sim, true, out Vec3 fStart, out Vec3 fAlong, out Vec3 fNormal, out double fLen);
            if (wallOkF)
            {
                Vec3 inDir = new Vec3((fAlong.x - fNormal.x) / Math.Sqrt(2), 0, (fAlong.z - fNormal.z) / Math.Sqrt(2)); // 45 deg: along the wall and into it
                var back = new Vec3(fStart.x + fAlong.x * 2 + fNormal.x * 3.3, fStart.y, fStart.z + fAlong.z * 2 + fNormal.z * 3.3); // 4 m off the wall face, 45 deg out
                // NIGHT41 T2 (A11; harness `n41proof slidewall --level n40-kiln --doorfree`): with DOORFREE the wall above is kb2's east wall, whose slide line
                // crosses THE EAST POUR (lava) from its first step: Air, never a slide ("slid False, 7.07 -> 10.70" since NIGHT39). A wall whose whole slide
                // line is floored (Bot/VerbsWall41.PickSlide) is used when one exists; the line says which.
                // n41-r6: Pick's wall and the old start are kept whenever that line is floored and the wall has no noride face (VerbsWall41.Row13Start)
                VerbsWall41.Row13Start(sim, fStart, fAlong, fNormal, ref back, ref inDir, out string slideWall);
                yield return Reset35(back, YawOf35(inDir));
                int wj0 = v.M(MoverEventType.WallJump), gj0 = v.M(MoverEventType.Jump);
                bool pressed = false, slid = false;
                double before = 0, after = 0, pressT = -1;
                // NIGHT41 T2 (A11, L-039): the slide's press was `t < 0.02`: under one frame on the low preset (~4 sim steps a frame), so it fell on the
                // frame the reset player was still settling ("slid False" on n38 / n40-kiln since NIGHT39). Now the press starts on the first GROUNDED
                // frame and stays pressed for at least two sim steps (Edge35Input keeps one edge); if no slide began 0.2 s later, one more press (a
                // released frame between: a new edge). The 9.9 m/s push is kept until the slide begins (at most 0.3 s). The jump's press: two sim steps too.
                int slidePressStep = -1, slidePresses = 0, slideOffFrames = 0, jumpPressStep = -1;
                yield return Do35(g, 1.6, t =>
                {
                    if (t < 0.05 || (!slid && t < 0.3)) m.velocity = new Vec3(inDir.x * 9.9, m.velocity.y, inDir.z * 9.9);
                    if (m.state == MoveState.Slide) slid = true;
                    int stepNow = g.sim.simStep;
                    if (!slid && !pressed && m.grounded && slidePresses < 2 && (slidePressStep < 0 || (slideOffFrames > 0 && (stepNow - slidePressStep) * g.sim.step > 0.2)))
                    {
                        slidePressStep = stepNow;
                        slidePresses++;
                        slideOffFrames = 0;
                    }
                    bool slidePressing = slidePressStep >= 0 && stepNow - slidePressStep < 2;
                    if (slidePressStep >= 0 && !slidePressing) slideOffFrames++;
                    bool press = !pressed && t > 0.05 && m.touchingWall;
                    if (press) { pressed = true; pressT = t; before = m.Speed; jumpPressStep = stepNow; }
                    bool jumpPressing = jumpPressStep >= 0 && stepNow - jumpPressStep < 2;
                    if (pressed && t - pressT > 0 && t - pressT < 0.15) after = Math.Max(after, m.Speed);
                    g.input.SetLook(YawOf35(inDir), 0);
                    bool slideHeld = !pressed && (slidePressing || slid); // held through the slide until the jump; released between a failed press and its retry
                    g.forcedMove = new InputFrame { moveZ = 1, slide = new Btn { pressed = slidePressing, held = slidePressing || slideHeld }, jump = Press(jumpPressing) };
                }, wallEnd);
                int wj = v.M(MoverEventType.WallJump) - wj0, gj = v.M(MoverEventType.Jump) - gj0;
                string bm = $"slid {slid}, wall contact {pressed}: WallJump {wj}, ground Jump {gj}; horizontal speed {before.ToString("0.00", ci)} -> {after.ToString("0.00", ci)} m/s on the press (slideBounce34, flow34 {sim.n34.flow34})";
                Verb35(v, 13, slid && pressed && wj > 0 && after >= 0.9 * before && before > 0 ? "PASS" : "RED", bm + $"; wall {fLen.ToString("0", ci)} m with floors; slide start: {slideWall}"); // Mover.cs:867: the slide's jump at a wall IS a wall jump
            }
            else Verb35(v, 13, "RED", "no wall with a floor at its spots found");
            if (sim.input is Edge35Input edgeIn) sim.UseInput(edgeIn.Inner); // the plain input back after the jump rows
            Verb35(v, 14, "SKIP", "flow34's own line (rides on 12-13; VERBS.md: not re-performed)");
            // ---- 15 jump pad ----
            if (sim.map.pads.Count == 0) Verb35(v, 15, "SKIP", "no jump pad on this level");
            else
            {
                Vec3 pad = sim.map.pads[0].pos;
                // NIGHT37 K (n37-r5 RED on n37-spire; the same on n33-spire with -skygrindOff37, harness `verbs37probe n33-spire --off37`): a pad sleeps while
                // its room's zone is locked (Zones.ApplyLocks: padEnabled, and a locked zone is solid), and the reel stands on it without walking the doors
                // open. Open the pad's zone and its target's as the campaign does when the player reaches that room (Campaign24 OpenZone: zones.Open free)
                PadSpec ps0 = sim.map.pads[0];
                bool awake0 = sim.zones.padEnabled[0];
                foreach (Vec3 q in new[] { ps0.pos, ps0.aimed ? ps0.target : ps0.pos })
                {
                    int zq = ZoneMath.ZoneIndexAt(sim.map.zoneParts, q.x, q.y + 1, q.z);
                    if (zq >= 0) sim.zones.Open(sim.map.zones[zq].id, free: true);
                }
                // NIGHT52 (builder K's finding): the baseline is read BEFORE the reset: Reset35 settles 0.35 s and the player lands on the pad at
                // ~0.13 s, so the launch happens inside the settle; an aimed pad then lands on its target and never launches again in the counted 1 s
                int jp0 = v.M(MoverEventType.JumpPad);
                yield return Reset35(new Vec3(pad.x, pad.y + 0.3, pad.z), openYaw);
                yield return Do35(g, 1.0, t => g.forcedMove = new InputFrame { moveZ = t < 0.3 ? 0.3 : 0 }, wallEnd);
                Verb35(v, 15, v.M(MoverEventType.JumpPad) > jp0 ? "PASS" : "RED", $"JumpPad events {v.M(MoverEventType.JumpPad) - jp0} (pad 0 awake before its room's zone was opened {awake0}, after {sim.zones.padEnabled[0]})");
            }
            Verb35(v, 16, "SKIP", "map feature, not performed by verbs35 (the walk bot crosses it; VERBS.md)");

            // ---- 17-23 the grapple, the whip, the bend ----
            bool owned = sim.arcline21 != null && sim.arcline21.Owned;
            if (beforeGrapple)
            {
                string gm = $"level {key}: grapple owned {owned} (grapple35 {sim.n35.grapple35}, campaign place {sim.grapple35LevelPlace}, D-2: owned from level 4 only)";
                string st = !sim.n35.grapple35 ? "RED" : owned ? "RED" : "PASS";
                string pre35 = !sim.n35.grapple35 ? "not landed (grapple35 off): " : "";
                foreach (int row in new[] { 17, 18, 19, 20, 22, 23 }) Verb35(v, row, st, pre35 + gm);
                Verb35(v, 21, "SKIP", "rides on 20 + 24 (VERBS.md)");
            }
            else if (!owned)
            {
                foreach (int row in new[] { 17, 18, 19, 20, 22, 23 }) Verb35(v, row, "RED", $"level {key}: the grapple is NOT owned (grapple35 {sim.n35.grapple35})");
                Verb35(v, 21, "SKIP", "rides on 20 + 24 (VERBS.md)");
            }
            else
            {
                int bend0 = v.E("bend34Start");
                // 17: a tap at a node
                if (Node35(sim, out Vec3 node, out Vec3 from, out double ny, out double np))
                {
                    yield return Reset35(from, ny, np);
                    yield return Do35(g, 0.4, t => { }, wallEnd); // the charges back
                    int ra0 = v.M(MoverEventType.RopeAttach);
                    double closest = 1e9;
                    int swapSteps = -1, swapAsked = -1;
                    string swapFrom = null;
                    yield return Do35(g, 2.5, t =>
                    {
                        g.forcedMove = new InputFrame { grapple = new Btn { pressed = t < 0.02, held = t < 0.1 } };
                        closest = Math.Min(closest, Math.Min((m.position - node).Length, (m.position + new Vec3(0, 1.0, 0) - node).Length)); // the feet or the chest
                        // 23: inside the bend, ask for the other gun; count the steps until it is in hand
                        if (swapAsked < 0 && v.E("bend34Start") > bend0 && sim.Bend34Depth01 > 0 && g.loadout.owned.Count >= 2)
                        {
                            swapAsked = sim.simStep;
                            swapFrom = g.loadout.Weapon;
                        }
                        if (swapAsked >= 0 && swapSteps < 0)
                        {
                            if (g.loadout.Weapon != swapFrom) swapSteps = sim.simStep - swapAsked;
                            else g.forcedCombat = new CombatInput { weapon = (g.loadout.owned.IndexOf(swapFrom) + 1) % g.loadout.owned.Count };
                        }
                    }, wallEnd);
                    bool att = v.M(MoverEventType.RopeAttach) > ra0;
                    Verb35(v, 17, att && closest <= 2 ? "PASS" : "RED", $"RopeAttach {v.M(MoverEventType.RopeAttach) - ra0}, the chest came within {closest.ToString("0.00", ci)} m of the node");
                    Verb35(v, 22, v.E("bend34Start") > bend0 ? "PASS" : "RED", $"bend34Start events after the latch {v.E("bend34Start") - bend0} (bend34 {sim.n34.bend34})");
                    if (g.loadout.owned.Count < 2) Verb35(v, 23, "SKIP", "one gun owned: no swap");
                    else Verb35(v, 23, swapSteps >= 0 && swapSteps <= 3 ? "PASS" : "RED", swapAsked < 0 ? "no bend to swap in" : $"the hand changed {swapSteps} sim steps after the frame that asked (the next frame's steps, at most 3, = instant; a lowered / raised swap is 18+)");
                    // 19: held at the node, the path stays on the straight line (no pendulum)
                    // NIGHT37 K (n37-r5 RED on n37-span, n37-spire and n35-finale; the same headless on n33-span / n33-spire / n35-finale with --off37,
                    // harness `verbs37probe`): the pull reaches the node and the rope lets go (RopeRelease ~0.3 s in), then the player FALLS for the rest
                    // of the 1.6 s and the fall was counted as "off the line" (7-13 m). The path is now measured while the rope is attached (attach to
                    // release), and the pull must reach the node (the chest within 2 m, row 17's bar): a swing still shows as a path off the line while
                    // attached. Its spot has a floor (Node35 floors: on the spires the first spot was in the air and the rope let go 0.08 s in).
                    Vec3 node19 = node, from19 = from;
                    double ny19 = ny, np19 = np;
                    if (Node35(sim, true, out Vec3 nF, out Vec3 fF, out double yF, out double pF)) { node19 = nF; from19 = fF; ny19 = yF; np19 = pF; }
                    yield return Reset35(from19, ny19, np19);
                    yield return Do35(g, 1.0, t => { }, wallEnd);
                    Vec3 a = m.position + new Vec3(0, 1.0, 0);
                    double dev = 0, devAfter = 0, near19 = 1e9;
                    int ra1 = v.M(MoverEventType.RopeAttach), rr1 = v.M(MoverEventType.RopeRelease);
                    double rel19 = -1;
                    yield return Do35(g, 1.6, t =>
                    {
                        g.forcedMove = new InputFrame { grapple = new Btn { pressed = t < 0.02, held = true } };
                        Vec3 p = m.position + new Vec3(0, 1.0, 0), ab = node19 - a, ap = p - a;
                        double l2 = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z, u = l2 > 1e-9 ? Math.Max(0, Math.Min(1, (ap.x * ab.x + ap.y * ab.y + ap.z * ab.z) / l2)) : 0;
                        var c = new Vec3(a.x + ab.x * u, a.y + ab.y * u, a.z + ab.z * u);
                        bool attached = v.M(MoverEventType.RopeAttach) > ra1, released = v.M(MoverEventType.RopeRelease) > rr1;
                        near19 = Math.Min(near19, (p - node19).Length); // the whole window, as row 17 (the pull carries the chest on after the release)
                        if (attached && !released) dev = Math.Max(dev, (p - c).Length);
                        else if (attached) { devAfter = Math.Max(devAfter, (p - c).Length); if (rel19 < 0) rel19 = t; }
                    }, wallEnd);
                    string sm = $"held 1.6 s: RopeAttach {v.M(MoverEventType.RopeAttach) - ra1}, released at {(rel19 < 0 ? "-" : rel19.ToString("0.00", ci))} s; while attached the farthest off the straight line {dev.ToString("0.00", ci)} m; the chest came within {near19.ToString("0.00", ci)} m of the node; after the release {devAfter.ToString("0.0", ci)} m (the fall, not counted)";
                    if (!sim.n35.grapple35) Verb35(v, 19, "RED", "not landed (grapple35 off, D-9): " + sm);
                    else Verb35(v, 19, v.M(MoverEventType.RopeAttach) > ra1 && dev < 1.0 && near19 <= 2 ? "PASS" : "RED", sm);
                }
                else foreach (int row in new[] { 17, 19, 22, 23 }) Verb35(v, row, "RED", "no grapple node in clear sight of a floor spot on this level");
                // 18: a tap at a bare wall 6-15 m ahead
                if (wallOkF) // NIGHT37 K: the wall with floors (row 13's): on n37-span the spot 9 m out was over o1's drop (n37-r5: the wall point 12 m away, no attach)
                {
                    var face = new Vec3(-fNormal.x, 0, -fNormal.z);
                    var spot = new Vec3(fStart.x + fAlong.x * 4 + fNormal.x * 9, fStart.y, fStart.z + fAlong.z * 4 + fNormal.z * 9);
                    yield return Reset35(spot, YawOf35(face), 0.12);
                    yield return Do35(g, 0.4, t => { }, wallEnd);
                    Vec3 eye = sim.Eye;
                    Vec3 aim = Fwd35(YawOf35(face));
                    var dir3 = new Vec3(aim.x * Math.Cos(0.12), Math.Sin(0.12), aim.z * Math.Cos(0.12));
                    Vec3 hitP = sim.world.RayIntersect(eye, dir3, out RayHit wh) ? new Vec3(eye.x + dir3.x * wh.distance, eye.y + dir3.y * wh.distance, eye.z + dir3.z * wh.distance) : eye;
                    int ra2 = v.M(MoverEventType.RopeAttach);
                    double close = 1e9;
                    yield return Do35(g, 2.0, t =>
                    {
                        g.forcedMove = new InputFrame { grapple = new Btn { pressed = t < 0.02, held = t < 0.1 } };
                        close = Math.Min(close, (m.position + new Vec3(0, 1.0, 0) - hitP).Length);
                    }, wallEnd);
                    string wm = $"RopeAttach {v.M(MoverEventType.RopeAttach) - ra2}, the chest came within {close.ToString("0.00", ci)} m of the wall point {Flat35(eye, hitP).ToString("0.0", ci)} m away";
                    if (!sim.n35.grapple35) Verb35(v, 18, "RED", "not landed (grapple35 off: NIGHT32 tapNodesOnly refuses a bare wall): " + wm);
                    else Verb35(v, 18, close <= 2 ? "PASS" : "RED", wm);
                }
                else Verb35(v, 18, "RED", "no wall with a floor at its spots found");
                // 20: the whip on a husk 9 m ahead
                {
                    yield return Reset35(open, openYaw);
                    yield return Do35(g, 0.4, t => { }, wallEnd);
                    Vec3 f = Fwd35(openYaw);
                    var at = new Vec3(open.x + f.x * 9, open.y, open.z + f.z * 9);
                    Husk hk = sim.husks.Spawn("husk", at, m.position);
                    yield return Do35(g, 0.3, t => { }, wallEnd);
                    if (hk != null)
                    {
                        Vec3 eye = sim.Eye, to = hk.position + new Vec3(0, 1.0, 0) - eye;
                        g.input.SetLook(YawOf35(to), Math.Atan2(to.y, Math.Sqrt(to.x * to.x + to.z * to.z)));
                    }
                    // Arcline21 / Whip33: a TAP on a small enemy (arcline.pullKinds: husk) first YANKS it to you ("arclinePull", the web's yank, 15 s crowd-control
                    // cooldown); while that cooldown runs a tap latches it as a zip ("whip33Latch"). So: tap, then re-aim and tap again 0.9 s later. Either
                    // event is the whip working (r2 counted the latch only and read 0 after the yank).
                    int wl0 = v.E("whip33Latch"), ap0 = v.E("arclinePull");
                    yield return Do35(g, 0.9, t => g.forcedMove = new InputFrame { grapple = new Btn { pressed = t < 0.02, held = t < 0.1 } }, wallEnd);
                    if (hk != null && hk.alive)
                    {
                        Vec3 eye = sim.Eye, to = hk.position + new Vec3(0, 1.0, 0) - eye;
                        g.input.SetLook(YawOf35(to), Math.Atan2(to.y, Math.Sqrt(to.x * to.x + to.z * to.z)));
                    }
                    yield return Do35(g, 1.2, t => g.forcedMove = new InputFrame { grapple = new Btn { pressed = t < 0.02, held = t < 0.1 } }, wallEnd);
                    int wl = v.E("whip33Latch") - wl0, ap = v.E("arclinePull") - ap0;
                    double hd = hk != null ? Flat35(open, hk.position) : -1;
                    Verb35(v, 20, wl + ap > 0 ? "PASS" : "RED", hk == null ? "no husk could be spawned" : $"two taps at a husk {hd.ToString("0.0", ci)} m ahead in a clear lane: arclinePull (yank) {ap}, whip33Latch {wl} (whip33 {sim.n33.whip33}, grapple owned {sim.arcline21.Owned})");
                    g.husks.KillAll();
                }
                Verb35(v, 21, "SKIP", "rides on 20 + 24 (VERBS.md)");
            }

            // ---- 24 punch ----
            yield return Reset35(open, openYaw);
            { // NIGHT40 (D-2): an n40 level that starts with no gun (n40-iceline): rows 24 / 40-42 SKIP with the reason (Verb35)
                bool gun40 = false;
                foreach (string w in g.loadout.owned) if (w != "gauntlet" && w != "pulse") gun40 = true;
                verbs40GauntletOnly = !gun40 && sim.campaignLevel != null && (sim.N41BaseId(sim.campaignLevel.id) ?? "").StartsWith("n40-"); // NIGHT41: an n41 level as its levelFrom
            }
            // NIGHT37 K (n37-r5 RED on n37-iceline: the hand was the PULSE gauntlet after the reset; harness `verbs37probe n35-iceline --off37`: the
            // same on NIGHT36's iceline): with the gauntlet in hand the F press is dropped by design (Combat.cs StepPunch: rules30 / lockout29
            // MeleeBlocked, "its swing is the melee", row 28). Row 24 is the fist with a GUN in hand: an owned gun is put in hand first (Hand35).
            string hand24 = g.loadout.Weapon;
            string gun24 = null;
            foreach (string w in g.loadout.owned) if (w != "gauntlet" && w != "pulse") { gun24 = w; break; }
            if (hand24 == "gauntlet" && gun24 != null) yield return Hand35(g, gun24, wallEnd);
            int pu0 = v.E("punch");
            string held24 = g.loadout.Weapon;
            yield return Do35(g, 0.6, t => g.forcedCombat = new CombatInput { weapon = -1, punch = new Btn { pressed = t < 0.02, held = t < 0.1 } }, wallEnd);
            Verb35(v, 24, v.E("punch") > pu0 ? "PASS" : "RED", $"punch events {v.E("punch") - pu0} with {held24} in hand (after the reset {hand24})");
            // ---- 25 KESTREL fire, 26 zoom, 27 charge ----
            // NIGHT40 (DECIDE40 D-2, GG: levels 2 and 3 start with their new weapon alone, the KESTREL unlocked by a zone later in the level): on an n40
            // level a KESTREL not owned at the reel's start is by design (SKIP with the reason); anywhere else it is still RED
            bool n40Start = sim.campaignLevel != null && (sim.N41BaseId(sim.campaignLevel.id) ?? "").StartsWith("n40-"); // NIGHT41: an n41 level as its levelFrom
            if (!Owns35(g, "revolver")) foreach (int row in new[] { 25, 26, 27 }) Verb35(v, row, n40Start ? "SKIP" : "RED", n40Start ? $"not owned at {key}'s start (DECIDE40 D-2: the KESTREL returns in a zone later in the level)" : "the KESTREL is not owned");
            else
            {
                yield return Hand35(g, "revolver", wallEnd);
                yield return Do35(g, 0.5, t => { }, wallEnd);
                int k0 = v.F("revolver");
                // NIGHT36 M (VERBS.md row 25, changed by F-1): a 0.1 s click (a real click: 50-100 ms); with tap36 the shot comes on the press step (the first
                // step of the frame that pressed), charge35 alone fires it on the release (12+ steps later)
                int pressStep = -1, fireStep0 = v.lastFireStep;
                yield return Do35(g, 0.4, t => { if (pressStep < 0) pressStep = sim.simStep; g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = t < 0.02, held = t < 0.1 } }; }, wallEnd);
                // NIGHT39 P1 (VERBS.md row 25, changed by p39): with design.n39.p39 live the KESTREL's click fires on its RELEASE (the 0.1 s click: about
                // 12 steps after the press; any step past the press's own frame and before FULL, as a slow frame can stretch the click), aimed from the press
                bool p39Live = sim.charge35 != null && sim.charge35.p39;
                bool tapLive = !p39Live && sim.charge35 != null && sim.charge35.tap36;
                int lag = v.lastFireStep != fireStep0 && pressStep >= 0 ? v.lastFireStep - pressStep : -1;
                int fullSteps = p39Live && g.loadout.charge27 != null ? g.loadout.charge27.FullSteps(0) : 0;
                string tm = $"KESTREL shots on one 0.1 s click {v.F("revolver") - k0}, the first {(lag < 0 ? "-" : lag.ToString(ci))} sim steps after the press frame (tap36 {tapLive}{(tapLive ? ": on the press, at most 2" : "")}; p39 {p39Live}{(p39Live ? $": on the release, 3-{fullSteps - 1} (tapS {sim.charge35.P39TapSteps})" : "")})";
                bool ok25 = v.F("revolver") > k0 && (p39Live ? lag >= 3 && lag < fullSteps : !tapLive || (lag >= 0 && lag <= 2));
                Verb35(v, 25, ok25 ? "PASS" : "RED", tm);
                bool zoomed = false;
                yield return Do35(g, 0.7, t => { g.forcedCombat = new CombatInput { weapon = -1, altFire = new Btn { pressed = t < 0.02, held = true } }; if (t > 0.4) zoomed |= g.loadout.adsActive; }, wallEnd);
                Verb35(v, 26, zoomed ? "PASS" : "RED", $"zoom (adsActive) while LT held: {zoomed} (zoom on {g.loadout.ZoomOn})");
                yield return Charge35(g, v, 27, "revolver", wallEnd);
            }
            // ---- 28 PULSE gauntlet ----
            if (!Owns35(g, "gauntlet")) Verb35(v, 28, "SKIP", $"not owned on {key} (the unlock rows)");
            else
            {
                // the gauntlet in hand emits "meleeSwing" (weapon gauntlet; Combat.cs SwingGauntlet), not "fire" / "punch": r2 counted those and read 0.
                // The swap is waited for (Hand35: the gauntlet in hand 0.3 s), then 0.4 s idle so the n30 swap rules / the raise are done.
                yield return Hand35(g, "gauntlet", wallEnd);
                yield return Do35(g, 0.4, t => { }, wallEnd);
                string inHand = g.loadout.Weapon;
                int g0 = v.E("meleeSwing");
                yield return Do35(g, 0.8, t => g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = t < 0.02, held = t < 0.15 } }, wallEnd);
                int gh = v.E("meleeSwing") - g0;
                Verb35(v, 28, gh > 0 ? "PASS" : "RED", $"in hand {inHand}: meleeSwing events on one RT press {gh}");
            }
            // ---- 29-31 THUNDERCLAP ----
            if (!Owns35(g, "shotgun")) foreach (int row in new[] { 29, 30, 31 }) Verb35(v, row, "SKIP", $"not owned on {key} (the unlock rows)");
            else
            {
                yield return Hand35(g, "shotgun", wallEnd);
                yield return Do35(g, 0.8, t => { }, wallEnd);
                int s0 = v.F("shotgun");
                yield return Do35(g, 0.5, t => g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = t < 0.02, held = t < 0.03 } }, wallEnd);
                Verb35(v, 29, v.F("shotgun") > s0 ? "PASS" : "RED", $"THUNDERCLAP shots on one tap {v.F("shotgun") - s0}");
                yield return Charge35(g, v, 30, "shotgun", wallEnd);
                yield return Do35(g, 0.6, t => { }, wallEnd);
                if (!g.loadout.ShockwaveOn)
                    Verb35(v, 31, "SKIP", $"not in this game: GG took the shockwave off the THUNDERCLAP (NIGHT25 shockwaveOff {g.loadout.shockwaveOff25}: its LT zooms); VERBS.md row 31 to correct");
                else
                {
                    int sw0 = v.E(N20Events.Shockwave);
                    yield return Do35(g, 0.8, t => g.forcedCombat = new CombatInput { weapon = -1, altFire = new Btn { pressed = t < 0.02, held = t < 0.3 }, special = new Btn { pressed = t >= 0.4 && t < 0.42, held = t >= 0.4 && t < 0.5 } }, wallEnd);
                    Verb35(v, 31, v.E(N20Events.Shockwave) > sw0 ? "PASS" : "RED", $"shockwave events {v.E(N20Events.Shockwave) - sw0} (LT, then SPECIAL)");
                }
            }
            Verb35(v, 32, "SKIP", $"not performed (VERBS.md: owned from level 5){(Owns35(g, "nailgun") ? "; owned here" : "")}");
            Verb35(v, 33, "SKIP", $"not performed (VERBS.md: owned after level 5){(Owns35(g, "torch") ? "; owned here" : "")}");
            // ---- 34 swap ----
            if (g.loadout.owned.Count < 2) Verb35(v, 34, "SKIP", $"one gun owned on {key}");
            else
            {
                string w0 = g.loadout.Weapon;
                int ws0 = v.E("weaponSwap");
                int other = (g.loadout.owned.IndexOf(w0) + 1) % g.loadout.owned.Count;
                yield return Do35(g, 0.8, t => g.forcedCombat = new CombatInput { weapon = t < 0.05 ? other : -1 }, wallEnd);
                Verb35(v, 34, v.E("weaponSwap") > ws0 && g.loadout.Weapon != w0 ? "PASS" : "RED", $"weaponSwap events {v.E("weaponSwap") - ws0}, hand {w0} -> {g.loadout.Weapon}");
            }
            Verb35(v, 35, "SKIP", "pad-only UI (PadRbCycle tests; VERBS.md: not performed)");
            Verb35(v, 36, "SKIP", "style names (NIGHT34's rea34 line; VERBS.md: not performed)");
            Verb35(v, 37, "SKIP", "SURVIVAL wall buys / the shop's own reel (VERBS.md: not performed)");
            if (verbsTag == "verbs36" || verbsTag == "verbs37") yield return Verbs36Rows(g, v, open, openYaw, wallEnd); // NIGHT36 M: VERBS.md rows 40-43 (FeelReel.N36.Move.cs)
            if (verbsTag == "verbs37") yield return Verbs37Rows(g, v, open, openYaw, wallEnd); // NIGHT37 K: VERBS.md rows 44-47 (FeelReel.N37.Kit.cs)

            if (hopDeferred)
            {
                // row 12's retry places (HopSite72.Candidates: the longest clear lanes of the level's rooms), tried after every other row; the line keeps its old text and adds the window and every attempt
                string fa = sim.FirstAwardRoom57; // Open35's room walk: from the level's first award room on, no boss room
                bool seen = fa == null || sim.campaignLevel.rooms.FindIndex(x => x.id == fa) < 0;
                var spawnRooms = new List<RoomDef>();
                foreach (RoomDef rr in sim.campaignLevel.rooms)
                {
                    if (!seen && rr.id == fa) seen = true;
                    if (seen && rr.kind != "boss") spawnRooms.Add(rr);
                }
                foreach (var c in HopSite72.Candidates(sim.world, spawnRooms.ConvertAll(x => x.spawn), open, openYaw)) hopSites.Add((c.at, c.yaw, spawnRooms[c.room].id, c.lane));
                for (int ha = 1; ha < hopSites.Count && !hopPass; ha++) yield return HopAttempt(ha);
                Verb35(v, 12, hopPass ? "PASS" : "RED", HopLine());
            }
            yield return Stop();
            Debug.Log($"[reel] {verbsTag}: total {v.pass}/{v.performed}");
            var red = new List<string>();
            foreach (var kv in v.result) if (kv.Value.Contains(" RED ")) red.Add(kv.Key.ToString(ci));
            res[0] = $"{(v.pass == v.performed ? "PASS" : "RED")}: total {v.pass}/{v.performed} on {sim.campaignLevel.id}; RED rows [{string.Join(", ", red)}] | {pc}";
        }
    }
}
