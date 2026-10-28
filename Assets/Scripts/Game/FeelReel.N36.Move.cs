// NIGHT36 builder M: the Player scenarios of design.n36.wallgrip36 and design.n36.text36 (the names are FeelReel.N27.cs PhaseN36M; the body is the
// partial N36MoveBody). Not run by its author (no Player in the worktree). Input through the game's forced move frame only (L-046), husks frozen, the
// player kept alive. Any map, SURVIVAL, from the safe spot.
//   wallgrip36  finds a wall near the spot with 8 m of wall along it and at least 5 m of wall above the floor, stands the player on the floor 1 m off
//     it running along it at 8 m/s and 5 m/s into it, and JUMPS (a press on the first frame), then (pass 1, "tap") lets go of jump at once: the
//     web's timed ride, the one that climbed; pass 2 ("hold") keeps jump held 1.6 s (wallride35's ride). Frames (pass 1): shot-N29-N36-grip-attach
//     (the first frame on the wall), -grip-05 (attach + 0.5 s), -grip-10 (attach + 1 s). Lines: the attach height, the most risen above it, the height
//     at + 0.5 s and + 1 s, the ride's seconds and end. GREEN (row on): both passes rode and neither rose more than 1.5 m above its attach point.
//     RED (-skygrindOff36 or the row off): the tap ride climbs (several metres at + 1 s).
//     Pass 3 ("rising", the lead's n36-r1: GG's own entry): a full-speed ground jump straight into the wall (0.6 m off, 4 m/s into it), meeting it in the
//     jump's first 0.1 s at about +8 m/s; frames shot-N29-N36-grip-rising-attach / -05 / -10. RED climbs metres, GREEN stays level.
//   text36  hands the UI one teaching line (the teach35 event, the ride's own line from design.n35.teach35) and waits for it on screen. Frame:
//     shot-N29-N36-text36. Line: where it is (its rect's centre as a share of the screen height from the top), its font px at 1080p, whether
//     the top slot shows it too, the layout checker on the HUD boxes with the teaching slot. GREEN (row on): in the teaching slot, centre 0.58-0.66
//     down, >= 32 px, not in the top slot, checker 0. RED (off): in the top slot (centre ~0.07 down, 24 px).
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
        static partial void N36MoveBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "wallgrip36") body = Grip36Scenario(g, dir, res, offs, pre);
            else if (s == "text36") body = Text36Scenario(g, dir, res, offs, pre);
            else if (s == "verbs36") body = Verbs36Scenario(g, dir, res, offs, pre);
        }

        sealed class Grip36Pass
        {
            public bool rode;
            public double attachY = double.NaN, rise, at05 = double.NaN, at10 = double.NaN, seconds, sinceJump = double.NaN, vyBefore;
            public string end = "none", label = "";
        }

        static IEnumerator Grip36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            Mover m = sim.mover;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | n36.wallgrip36 {sim.n36.wallgrip36} (grip on {m.Grip36On}), n35.wallride35 {sim.n35.wallride35}, ride trim panels {RideTrim36.Panels}";
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = IdleCombat;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Vec3 chest = new Vec3(spot.x, spot.y + 1.2, spot.z);
            Vec3 wp = default, wn = default, wt = default;
            bool found = false;
            for (double reach = 12; reach <= 24 && !found; reach += 12)
            {
                if (!FindWall35(g, chest, 2, reach, 8, out wp, out wn, out wt)) continue;
                // 5 m of wall above the floor where the jump meets it
                found = g.world.RayIntersect(new Vec3(wp.x + wn.x * 0.6, spot.y + 5, wp.z + wn.z * 0.6), wn * -1, out RayHit hi) && hi.distance < 1.2;
            }
            if (!found)
            {
                res[0] = "RED: no wall with 8 m along it and 5 m up within 24 m of the safe spot (boot another map) | " + pc;
                yield break;
            }
            double yaw = Math.Atan2(-wt.x, -wt.z);
            Vec3 right = MovementMath.WishDirection(1, 0, yaw);
            double side = right.x * -wn.x + right.z * -wn.z >= 0 ? 1 : -1; // the stick's x toward the wall
            Start(g, "wallgrip36", "camera", dir);
            yield return Frames(pre);
            var passes = new List<Grip36Pass>();
            var st = new double[4];
            // the cases: "tap" (1 m off, 5 m/s into the wall: frames shot-N29-N36-grip-*), "hold" (the same, jump held 1.6 s), "rising" (the lead's n36-r1
            // note: GG's own entry: a full-speed ground jump straight into the wall, 0.6 m off at 4 m/s into it, so it meets the wall within the jump's first
            // 0.1 s at about +8 m/s: frames shot-N29-N36-grip-rising-*)
            var cases = new[] { ("tap", false, 1.0, 5.0, "N36-grip"), ("hold", true, 1.0, 5.0, null), ("rising", false, 0.4, 6.0, "N36-grip-rising") }; // the lead's n36-r3: rising met the wall at +2.9 m/s from 0.6 m: now 0.05 m of gap (capsule 0.35) at 6 m/s into it
            foreach (var (label, hold, offWall, intoSpeed, shotName) in cases)
            {
                var p = new Grip36Pass { label = label };
                double startedAt = double.NaN, jumpAt = double.NaN, lastVy = 0;
                Action<MoverEvent> onMove = e =>
                {
                    if (e.type == MoverEventType.Jump && double.IsNaN(jumpAt)) jumpAt = sim.SimTime;
                    if (e.type == MoverEventType.WallRideStart && double.IsNaN(startedAt)) { startedAt = sim.SimTime; p.rode = true; p.attachY = m.position.y; p.vyBefore = lastVy; p.sinceJump = sim.SimTime - jumpAt; }
                    if (e.type == MoverEventType.WallRideEnd && !double.IsNaN(startedAt) && p.end == "none") { p.end = e.reason; p.seconds = sim.SimTime - startedAt; }
                };
                m.Events += onMove;
                Vec3 start = new Vec3(wp.x + wn.x * offWall - wt.x * 1.5, spot.y + 0.05, wp.z + wn.z * offWall - wt.z * 1.5);
                g.Respawn(start, yaw * 180 / Math.PI);
                g.input.SetLook(yaw, 0);
                g.forcedMove = new InputFrame { yaw = yaw };
                yield return Frames(20);
                FeelProbe.MarkInput();
                m.velocity = new Vec3(wt.x * 8 - wn.x * intoSpeed, 0, wt.z * 8 - wn.z * intoSpeed);
                double t0 = sim.SimTime;
                bool shot05 = false, shot10 = false, shotA = false;
                double wallEnd = Time.realtimeSinceStartup + 20;
                int frame = 0;
                while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 3)
                {
                    God(g);
                    double now = sim.SimTime;
                    bool held = frame == 0 || (hold && now - t0 < 1.6);
                    g.input.SetLook(yaw, 0);
                    g.forcedMove = new InputFrame { moveX = side * 0.5, moveZ = 1, yaw = yaw, jump = new Btn { pressed = frame == 0, held = held } };
                    frame++;
                    if (double.IsNaN(startedAt)) lastVy = m.velocity.y;
                    if (!double.IsNaN(startedAt))
                    {
                        double dy = m.position.y - p.attachY;
                        if (m.state == MoveState.WallRide) p.rise = Math.Max(p.rise, dy);
                        if (double.IsNaN(p.at05) && now - startedAt >= 0.5) p.at05 = dy;
                        if (double.IsNaN(p.at10) && now - startedAt >= 1.0) p.at10 = dy;
                        if (shotName != null && !shotA) { shotA = true; yield return Shot29(dir, shotName + "-attach", st); continue; }
                        if (shotName != null && !shot05 && now - startedAt >= 0.5) { shot05 = true; yield return Shot29(dir, shotName + "-05", st); continue; }
                        if (shotName != null && !shot10 && now - startedAt >= 1.0) { shot10 = true; yield return Shot29(dir, shotName + "-10", st); continue; }
                        if (p.end != "none" && now - startedAt > 1.1) break;
                    }
                    yield return null;
                }
                if (p.end == "none" && p.rode) p.seconds = sim.SimTime - startedAt;
                m.Events -= onMove;
                g.forcedMove = new InputFrame { yaw = yaw };
                yield return Frames(Ms(400));
                passes.Add(p);
            }
            g.forcedMove = null;
            yield return Stop();
            bool ok = true;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < passes.Count; i++)
            {
                Grip36Pass p = passes[i];
                bool good = p.rode && p.rise <= 1.5;
                ok &= good;
                sb.Append($" | {p.label}: {(p.rode ? "rode" : "NO RIDE")}, met the wall {p.sinceJump.ToString("0.000", ci)} s after the jump at vy {p.vyBefore.ToString("+0.0;-0.0", ci)} m/s, attach y {p.attachY.ToString("0.00", ci)}, rose +{p.rise.ToString("0.00", ci)} m, +0.5 s {p.at05.ToString("+0.00;-0.00", ci)}, +1 s {p.at10.ToString("+0.00;-0.00", ci)} m, {p.seconds.ToString("0.00", ci)} s, end '{p.end}'{(good ? "" : " RED")}");
            }
            res[0] = $"{(ok ? "GREEN" : "RED")}: clamps {m.Grip36Clamps}{sb} | wall at {wp.x.ToString("0.0", ci)}, {wp.z.ToString("0.0", ci)} | {pc}";
        }

        /// <summary>The tag of the verbs reel's lines ("verbs35", or "verbs36" while verbs36 runs the same scenario with rows 40-43).</summary>
        static string verbsTag = "verbs35";

        /// <summary>NIGHT37 K (row 40 RED on every map in n37-r5 and n36-r4: "pips 3.00 at the punch"): with design.stamina's bar on (every Player
        /// run) the dash spends the BAR and the Mover holds the pips full (Mover.cs TryDash: "the pips are held full with the bar on"), so the pips
        /// could never show the dash spent or the parry's refill. The dash meter in dashes: the bar / the boost's cost, or the pips with the bar off.</summary>
        static double Dash40(Mover m) => m.stamina.rules.Any ? (m.stamina.rules.cost > 0 ? m.stamina.bar / m.stamina.rules.cost : 0) : m.pips;

        static double DashFull40(Mover m) => m.stamina.rules.Any ? (m.stamina.rules.cost > 0 ? m.StaminaMax / m.stamina.rules.cost : 0) : m.MaxPips;

        /// <summary>NIGHT37 K (row 43 RED on n35-kiln and n35-finale in n37-r5): the walk-in is a verb of the levels whose boss fights where the player
        /// walks in: a level with THE LANTERN's hall row (design.n36 level36's lantern36 room) or a NIGHT37 boss row's levelTo with that row on.
        /// THE KILN WYRM (level 3) and level 6's finale keep the Respawn into their arena (VERBS.md row 43: level 1; NIGHT36 never had them walk in).
        /// Read from the level and the rows, not from the run's walk-in flag (a missing flag on a walk-in level stays RED).</summary>
        static bool WalkInLevel43(SurvivalSim sim, out string why)
        {
            string id = sim.campaignLevel?.id ?? "";
            if (sim.lantern36Centre != null) { why = "THE LANTERN's hall row"; return true; }
            foreach (string row in new[] { "wick37", "stag37", "loom37", "crown37" })
            {
                string p = "design.n37." + row;
                if (sim.cfg.Has(p + ".levelTo") && sim.cfg.Str(p + ".levelTo") == id && RowOn37(sim, row)) { why = row + "'s level"; return true; }
            }
            why = "no walk-in boss on " + id + " (its boss keeps the Respawn into its arena)";
            return false;
        }

        /// <summary>docs/VERBS.md's NIGHT36 rows (#, verb).</summary>
        static readonly (int n, string verb)[] Verbs36Table =
        {
            (40, "parry"), (41, "launch"), (42, "heal by aggression"), (43, "the boss walk-in"),
        };

        /// <summary>verbs36 (NIGHT36.md method 4): verbs35's scenario (rows 1-37, with rows 9 / 25 / 27 / 30 as tonight changed them: wallgrip36's rise, tap36's
        /// press step and its 1 normal + 1 charged) plus rows 40-43, one line each: `[reel] verbs36: <#> <verb> PASS|RED|SKIP <measured>`, then the total.
        /// Boot as verbs35: -skygrindMap <L1 id> -skygrindReelOnly verbs36 (one Player per map, L-046).</summary>
        static IEnumerator Verbs36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            verbsTag = "verbs36";
            offs.Add(() => verbsTag = "verbs35");
            yield return Verbs35Scenario(g, dir, res, offs, pre);
            verbsTag = "verbs35";
            res[0] = "verbs36: " + res[0];
        }

        /// <summary>Rows 40-43 inside the verbs scenario (after row 37, before its close). The enemies live only for their row: every other enemy is put
        /// down each frame (P's harness shape, Harness.N36.Core.cs), the husks unfrozen while a row needs them to act, the player invulnerable (the mover's
        /// flag) with health 50 of a large max so a heal shows.</summary>
        static IEnumerator Verbs36Rows(SurvivalGame g, Verbs35State v, Vec3 open, double openYaw, double wallEnd)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            Mover m = sim.mover;
            GameConfig cfg = g.cfg;
            var keep = new HashSet<Enemy>();
            int kills = 0, parries = 0, parryBursts = 0, launchBursts = 0, heals = 0;
            Action<object> onKill = o => { if (o is KillEvent k && k.owner == "player") kills++; };
            Action<object> onParry = _ => parries++;
            Action<object> onPBurst = _ => parryBursts++;
            Action<object> onLBurst = _ => launchBursts++;
            Action<object> onHeal = _ => heals++;
            sim.events.On("kill", onKill);
            sim.events.On(Parry36.Event, onParry);
            sim.events.On("parry36Burst", onPBurst);
            sim.events.On(Launch36.BurstEvent, onLBurst);
            sim.events.On(Heal36.Event, onHeal);
            bool invWas = m.invulnerable;

            void Cull()
            {
                foreach (Enemy e in sim.enemies.all) if (e.alive && !keep.Contains(e)) e.alive = false;
                m.invulnerable = true;
            }
            void Look(Vec3 at)
            {
                Vec3 to = at - sim.Eye;
                g.input.SetLook(YawOf35(to), Math.Atan2(to.y, Math.Sqrt(to.x * to.x + to.z * to.z)));
            }
            Husk Put(string kind, double d, double hp)
            {
                Vec3 f = Fwd35(openYaw);
                for (int k = 0; k < 6; k++)
                {
                    Husk h = sim.husks.Spawn(kind, new Vec3(open.x + f.x * d, open.y + 0.25 * k, open.z + f.z * d), m.position);
                    if (h == null) continue;
                    if (hp > 0) h.hp = h.maxHp = hp;
                    keep.Add(h);
                    return h;
                }
                return null;
            }
            IEnumerator Run(double seconds, Action<double> each)
            {
                int s0 = sim.simStep;
                while (T35(g, s0) < seconds && Time.realtimeSinceStartup < wallEnd)
                {
                    Cull();
                    each(T35(g, s0));
                    yield return null;
                }
                g.forcedMove = new InputFrame();
                g.forcedCombat = IdleCombat;
            }
            IEnumerator Place(Vec3 at, double yaw)
            {
                g.forcedMove = new InputFrame();
                g.forcedCombat = IdleCombat;
                keep.Clear();
                g.Respawn(at, yaw * 180 / Math.PI);
                g.input.SetLook(yaw, 0);
                yield return Do35(g, 0.35, t => { }, wallEnd);
                g.husks.KillAll();
            }

            // ---- 40 parry: a spitter pinned 12 m down the clear lane throws one volley at the player; the punch goes in as the first glob comes within
            // orb.reflectRange (one frame early: the parry's buffer covers it) ----
            {
                yield return Place(open, openYaw);
                g.husks.frozen = false;
                Husk sh = Put("spitter", 12, 0);
                if (!(sh is Spitter s)) Verb35(v, 40, "RED", "no spitter could be placed in the clear lane");
                else
                {
                    Vec3 pin = s.position;
                    double reach = cfg.Num("orb.reflectRange"), globSpeed = cfg.Num("spitter.globSpeed");
                    g.health.hp = 50;
                    int p0 = parries, b0 = parryBursts;
                    bool thrown = false, punched = false;
                    double pipsAtPunch = -1, pipsAfter = -1, nearestSeen = double.PositiveInfinity, pipsMax = DashFull40(m); // NIGHT37 K: the dash meter (Dash40)
                    int punchFrames = 0, frame40 = 0;
                    yield return Run(3.0, t =>
                    {
                        // the lead's n36-r3: a DASH first (the pips were full at the punch, so the parry's refill could not show): one forward dash on the
                        // first frame (6 m toward the spitter), the volley thrown 0.4 s in, after it
                        g.forcedMove = new InputFrame { dash = new Btn { pressed = frame40 == 0, held = frame40 <= 2 }, moveZ = frame40 <= 2 ? 1 : 0 };
                        frame40++;
                        if (s.alive)
                        {
                            s.position = pin;
                            s.velocity = default;
                            s.cooldownSteps = 9999; // its own volleys never start: the reel throws one
                            if (s.state == "tell") s.state = "idle";
                            Look(s.AimPoint);
                        }
                        if (!thrown && t >= 0.4 && s.alive)
                        {
                            HuskAi saved = s.ai;
                            s.ai = null;
                            sim.husks.acid.ThrowVolley(s, m.position);
                            s.ai = saved;
                            thrown = true;
                        }
                        double nearest = double.PositiveInfinity;
                        foreach (SpitterAcid.Glob gl in sim.husks.acid.globs) if (gl.alive && !gl.reflected) nearest = Math.Min(nearest, Vec3.DistanceTo(gl.position, sim.Eye));
                        nearestSeen = Math.Min(nearestSeen, nearest);
                        bool press = thrown && !punched && nearest <= reach + globSpeed * Time.deltaTime;
                        if (press) { punched = true; pipsAtPunch = Dash40(m); }
                        if (punched && punchFrames < 3) punchFrames++;
                        g.forcedCombat = new CombatInput { weapon = -1, punch = new Btn { pressed = press, held = punched && punchFrames <= 2 } };
                        if (parries > p0 && pipsAfter < 0) pipsAfter = Dash40(m);
                    });
                    double hpAfter = g.health.hp;
                    bool on = sim.n36.parry36;
                    string pm = $"punched {punched} (nearest glob {(double.IsInfinity(nearestSeen) ? "-" : nearestSeen.ToString("0.0", ci))} m, reach {reach.ToString("0.0", ci)}): parry36 {parries - p0}, return bursts {parryBursts - b0}, spitter {(s.alive ? "ALIVE" : "DEAD")}, health 50 -> {hpAfter.ToString("0", ci)}, dashes in the meter {pipsAtPunch.ToString("0.00", ci)} at the punch -> {pipsAfter.ToString("0.00", ci)} at the parry (parry36 {on}, tells36 {sim.n36.tells36})";
                    pm += $", full {pipsMax.ToString("0.00", ci)} in dashes ({(m.stamina.rules.Any ? "the stamina bar / the boost cost" : "pips")}) (the dash spent one before the punch: {(pipsAtPunch >= 0 && pipsAtPunch < pipsMax - 0.5 ? "yes" : "NO")})";
                    bool ok = parries > p0 && parryBursts > b0 && hpAfter > 50 && pipsAtPunch < pipsMax - 0.5 && pipsAfter >= Math.Min(pipsMax, pipsAtPunch + 0.9) - 1e-6; // a dash back (capped at a full meter)
                    if (!on) Verb35(v, 40, "RED", "not landed (parry36 off): " + pm);
                    else Verb35(v, 40, ok ? "PASS" : "RED", pm);
                }
                g.husks.KillAll();
            }

            // ---- 41 launch: husk A 6 m down the lane, husk B 4.5 m beyond it; dash at A, punch it in reach: A flies into B, a burst, two kills ----
            {
                yield return Place(open, openYaw);
                g.husks.frozen = false;
                double hp = cfg.Num("husk.hp"), pRange = cfg.Num("punch.range");
                Husk a = Put("husk", 6, hp), b = Put("husk", 10.5, hp);
                if (a == null || b == null) Verb35(v, 41, "RED", $"husks not placed (A {a != null}, B {b != null})");
                else
                {
                    Vec3 pinA = a.position, pinB = b.position;
                    int k0 = kills, l0 = launchBursts;
                    bool punched = false;
                    string stateAt = "-";
                    int frame = 0;
                    yield return Run(2.5, t =>
                    {
                        if (!punched && a.alive) { a.position = pinA; a.velocity = default; a.cooldownSteps = 9999; }
                        if (b.alive) { b.position = pinB; b.velocity = default; b.cooldownSteps = 9999; b.knockSteps = 0; }
                        g.input.SetLook(openYaw, 0);
                        bool press = !punched && frame > 1 && a.alive && Flat35(a.position, m.position) <= pRange * 0.9;
                        if (press) { punched = true; stateAt = m.state.ToString(); }
                        g.forcedMove = new InputFrame { moveZ = punched ? 0 : 1, dash = new Btn { pressed = frame == 1, held = frame >= 1 && frame <= 3 } };
                        g.forcedCombat = new CombatInput { weapon = -1, punch = new Btn { pressed = press, held = press } };
                        frame++;
                    });
                    int kd = kills - k0, lb = launchBursts - l0;
                    string lm = $"dash-punched {punched} (in {stateAt}): launch36Burst {lb}, kills {kd} (A {(a.alive ? "alive" : "dead")}, B {(b.alive ? "alive" : "dead")}; launch36 {sim.n36.launch36})";
                    if (!sim.n36.launch36) Verb35(v, 41, "RED", "not landed (launch36 off): " + lm);
                    else Verb35(v, 41, lb > 0 && kd >= 2 ? "PASS" : "RED", lm);
                }
                g.husks.KillAll();
            }

            // ---- 42 heal: health 50; a 1 hp husk 2.5 m ahead shot dead (heal36), then one 12 m ahead (no heal) ----
            {
                string gun = Owns35(g, "revolver") ? "revolver" : g.loadout.owned.Count > 0 ? g.loadout.owned[0] : null;
                if (gun == null) Verb35(v, 42, "RED", "no gun owned");
                else
                {
                    var got = new double[2];
                    var hpAt = new double[2];
                    var dead = new bool[2];
                    double[] dist = { 2.5, 12 };
                    for (int c = 0; c < 2; c++)
                    {
                        yield return Place(open, openYaw);
                        yield return Hand35(g, gun, wallEnd);
                        yield return Do35(g, 0.5, t => { }, wallEnd);
                        g.husks.frozen = true;
                        Husk h = Put("husk", dist[c], 1);
                        if (h == null) continue;
                        Vec3 pin = h.position;
                        g.health.hp = 50;
                        int h0 = heals;
                        yield return Run(1.5, t =>
                        {
                            if (h.alive) { h.position = pin; h.velocity = default; h.cooldownSteps = 9999; Look(h.AimPoint); }
                            bool fire = h.alive && (t % 0.3) < 0.1;
                            g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { pressed = fire && (t % 0.3) < 0.02, held = fire } };
                        });
                        got[c] = heals - h0;
                        hpAt[c] = g.health.hp;
                        dead[c] = !h.alive;
                    }
                    double radius = cfg.Has("design.n36.heal36.radius") ? cfg.Num("design.n36.heal36.radius") : double.NaN;
                    string hm = $"{gun}: a kill at 2.5 m (killed {dead[0]}): heal36 {got[0]}, health 50 -> {hpAt[0].ToString("0", ci)}; a kill at 12 m (killed {dead[1]}): heal36 {got[1]}, health 50 -> {hpAt[1].ToString("0", ci)} (radius {radius.ToString("0.#", ci)} m; heal36 {sim.n36.heal36})";
                    if (!sim.n36.heal36) Verb35(v, 42, "RED", "not landed (heal36 off): " + hm);
                    else Verb35(v, 42, dead[0] && dead[1] && got[0] >= 1 && got[1] == 0 && hpAt[0] > 50 ? "PASS" : "RED", hm);
                }
                g.husks.KillAll();
            }

            // ---- 43 the boss walk-in: from the exit of the room before the boss room, walk into it; the fight starts with no Respawn (no jump of the
            // feet over 4 m in a frame) and the feet stay in the boss room ----
            {
                List<RoomDef> rooms = sim.campaignLevel.rooms;
                int bi = rooms.FindIndex(r => r.kind == "boss");
                if (bi <= 0) Verb35(v, 43, "SKIP", $"no boss room after a room on {sim.campaignLevel.id}");
                else if (!WalkInLevel43(sim, out string why43)) Verb35(v, 43, "SKIP", why43);
                else
                {
                    RoomDef prev = rooms[bi - 1], boss = rooms[bi];
                    var target = new Vec3((boss.x0 + boss.x1) / 2, boss.spawn.y, (boss.z0 + boss.z1) / 2);
                    Vec3 back = prev.exit - new Vec3((prev.x0 + prev.x1) / 2, prev.exit.y, (prev.z0 + prev.z1) / 2);
                    double bl = Math.Sqrt(back.x * back.x + back.z * back.z);
                    Vec3 from = bl > 1e-6 ? new Vec3(prev.exit.x - back.x / bl * 2, prev.exit.y + 0.2, prev.exit.z - back.z / bl * 2) : new Vec3(prev.exit.x, prev.exit.y + 0.2, prev.exit.z);
                    g.forcedMove = new InputFrame();
                    g.forcedCombat = IdleCombat;
                    g.Respawn(from, YawOf35(target - from) * 180 / Math.PI);
                    yield return Do35(g, 0.35, t => { }, wallEnd);
                    Vec3 last = m.position;
                    double jump = 0, enteredAt = -1;
                    bool entered = false;
                    int s0 = sim.simStep;
                    while (T35(g, s0) < 14 && Time.realtimeSinceStartup < wallEnd && !(entered && T35(g, s0) - enteredAt > 2))
                    {
                        God(g);
                        m.invulnerable = true;
                        double d = Flat35(last, m.position);
                        if (d > jump) jump = d;
                        last = m.position;
                        if (!entered && sim.campaign != null && sim.campaign.room == bi) { entered = true; enteredAt = T35(g, s0); }
                        double yaw = YawOf35(target - m.position);
                        g.input.SetLook(yaw, 0);
                        g.forcedMove = new InputFrame { moveZ = entered ? 0 : 1, yaw = yaw };
                        yield return null;
                    }
                    g.forcedMove = new InputFrame();
                    Vec3 p = m.position;
                    bool inside = p.x >= boss.x0 - 1 && p.x <= boss.x1 + 1 && p.z >= boss.z0 - 1 && p.z <= boss.z1 + 1;
                    string bm = $"walked from {prev.id}'s exit: entered {boss.id} {entered} at {enteredAt.ToString("0.0", ci)} s, the largest jump of the feet in one frame {jump.ToString("0.0", ci)} m (a Respawn is a jump), feet {(inside ? "in" : "OUT of")} the boss room at the end (walkin36 {sim.n36.walkin36}, boss36 {sim.n36.boss36}, walk-in here {sim.Boss36WalkIn})";
                    if (!sim.n36.walkin36) Verb35(v, 43, "RED", "not landed (walkin36 off): " + bm);
                    else Verb35(v, 43, entered && jump <= 4 && inside ? "PASS" : "RED", bm);
                }
            }

            sim.events.Off("kill", onKill);
            sim.events.Off(Parry36.Event, onParry);
            sim.events.Off("parry36Burst", onPBurst);
            sim.events.Off(Launch36.BurstEvent, onLBurst);
            sim.events.Off(Heal36.Event, onHeal);
            m.invulnerable = invWas;
            g.husks.frozen = true;
        }

        /// <summary>A UI rect's centre as a share of the screen height from the top (-1: none).</summary>
        static double ScreenCentreDown36(RectTransform rt)
        {
            if (rt == null) return -1;
            Canvas c = rt.GetComponentInParent<Canvas>();
            if (c != null) c = c.rootCanvas;
            Camera cam = c != null && c.renderMode != RenderMode.ScreenSpaceOverlay ? c.worldCamera : null;
            var w = new Vector3[4];
            rt.GetWorldCorners(w);
            Vector2 a = RectTransformUtility.WorldToScreenPoint(cam, w[0]), b = RectTransformUtility.WorldToScreenPoint(cam, w[2]);
            return 1 - (a.y + b.y) / 2 / Screen.height;
        }

        static IEnumerator Text36Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = $"text36 built {Ui.Text36Built} (row {N36Fixes.ViewRowOn(g.cfg, "text36")}), screen {Screen.width}x{Screen.height}, device {PlayerDevices.Last}";
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = IdleCombat;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "text36", "ui", dir);
            yield return Frames(pre);
            const string line = "HOLD {jump} on a WALL to WALL RIDE. Let go to leap.";
            g.events.Emit("teach35", new Teach35Event { id = "reel36", text = line, textPad = line, trigger = "reel", seconds = 5 });
            double t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 8 && Ui.Teach35IdNow != "reel36") { God(g); yield return null; }
            double up0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - up0 < 0.4) { God(g); yield return null; }
            string shown = Ui.Teach35TextNow;
            bool inTeach = Ui.Text36Built && !string.IsNullOrEmpty(Ui.Hud36TeachNow) && Ui.Hud36TeachNow == shown;
            bool inTop = !string.IsNullOrEmpty(shown) && Ui.Hud33SlotNow == shown;
            double down = inTeach ? ScreenCentreDown36(Ui.Text36Rect) : inTop ? ScreenCentreDown36(Ui.Slot33Rect) : -1;
            float px = Ui.Teach35PxNow;
            var st = new double[4];
            yield return Shot29(dir, "N36-text36", st);
            List<string> bad = Hud33Layout.CheckLayout(Screen.width, Screen.height, 5, false, false, Ui.Text36Built);
            yield return Stop();
            bool ok = inTeach && !inTop && down >= 0.58 && down <= 0.66 && px >= 32 - 0.01 && bad.Count == 0;
            res[0] = $"{(ok ? "GREEN" : "RED")}: \"{shown}\" in the {(inTeach ? "teaching slot" : inTop ? "TOP slot" : "NO slot")}, centre {down.ToString("0.000", ci)} of the screen down, {px.ToString("0", ci)} px, top slot '{Ui.Hud33SlotNow}', layout checker {bad.Count}{(bad.Count > 0 ? " (" + string.Join(" ;; ", bad) + ")" : "")} | {pc}";
        }
    }
}
