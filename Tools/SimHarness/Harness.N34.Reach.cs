// NIGHT34 builder M (design.n34.reach34; NIGHT34.md M1): `reach34 [--seeds N] [--nobosses] [--bossesoff]`, called from Harness.N34.Flow.cs (N34Flow).
// 1. THE KILN WYRM fight (Boss32Run's rig: design.n30.wyrm30 + n31.wyrm31 on), the player invulnerable with the PULSE GAUNTLET in hand, put in the air at the
//    head's height so the eye is at the worm body's surface + gRange + bossMargin - 0.05 m, aiming at the head, once with the row OFF and once ON:
//    a) the swing's target math as the patched Combat.cs runs it (CombatMath.SweepTargets over Loadout.GauntletTargets34, gRange, the cone);
//    b) the real click swing through sim.Step (the HitEvent "hit" with weapon gauntlet on the worm's id); its rule is Combat.cs (B's) after the lead's
//       Tools/night/n34m-reach-patch.cjs: before the patch the real swing uses PunchTargets, so (b) says "patch pending" instead of judging;
//    c) the real LT BLAST (NIGHT27 pulse27, PulseCharge27.cs: no patch needed) with a full glove, the worm's aim point blastRadius + R + bossMargin / 2 m
//       off the aim line, 3 m along it.
//    PASS: each of a / b / c hits ON and misses OFF.
// 2. The bot boss times with the row forced ON (SurvivalSim.Force34Reach) through NIGHT33's bosses33 pilots (Bosses33Command --nocamp): band p50 15-22 s.
//    --flow forces flow34 ON for that run too (the boss pilots with the new mover branches).
//    --bossesoff runs them with the row forced OFF too (the before line).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Reach34Row
    {
        public bool ok, mathHit, swingHit, blastHit, blastLive, swingDamage;
        public double swingSurface, blastOff;
        public string note = "";
    }

    static double? reach34At;
    static readonly List<string> reach34Seen = new List<string>();

    static int Reach34Command(string[] args)
    {
        bool patched = File.ReadAllText(Path.Combine(Root, "..", "Assets", "Scripts", "Core", "Combat.cs")).Contains("GauntletTargets34()");
        int seeds = int.Parse(Opt33(args, "--seeds", "5"), CultureInfo.InvariantCulture);
        string at = Opt33(args, "--at", null); // a probe: the eye this far (m) past the body's surface instead
        reach34At = at == null ? (double?)null : double.Parse(at, CultureInfo.InvariantCulture);
        GameConfig cfg0 = Cfg30("wyrm30");
        double gRange = cfg0.Num("weapons.gauntlet.range"), margin = cfg0.Num("design.n34.reach34.bossMargin");
        Console.WriteLine($"reach34: gauntlet range {gRange} m, bossMargin {margin} m; the eye at surface + {gRange + margin - 0.05:0.00} m (range + margin - 0.05); Combat.cs patch {(patched ? "APPLIED" : "PENDING (n34m-reach-patch.cjs)")}");
        int bad = 0;
        var rows = new Dictionary<bool, Reach34Row>();
        foreach (bool on in new[] { false, true })
        {
            Reach34Row r = Reach34Wyrm(on, 1, gRange, margin);
            rows[on] = r;
            Console.WriteLine($"  row {(on ? "ON " : "OFF")}: {(r.ok ? "" : "RIG FAILED " + r.note + "  ")}swing math {(r.mathHit ? "HIT " : "miss")}  real swing {(r.swingHit ? "HIT " : "miss")} (surface {r.swingSurface:0.000} m)  LT blast {(r.blastLive ? r.blastHit ? "HIT " : "miss" : "not live")} (off the line {r.blastOff:0.000} m)  {r.note}");
            if (!r.ok) bad++;
        }
        Reach34Row f = rows[false], t = rows[true];
        bool mathOk = t.mathHit && !f.mathHit, blastOk = t.blastLive && t.blastHit && !f.blastHit, swingOk = t.swingHit && !f.swingHit;
        Console.WriteLine($"  swing math {(mathOk ? "PASS" : "RED")}; LT blast {(blastOk ? "PASS" : "RED")}; real swing {(patched ? swingOk ? "PASS" : "RED" : "patch pending (off miss " + (!f.swingHit ? "yes" : "NO") + ")")}");
        if (!mathOk || !blastOk || patched && !swingOk) bad++;
        // the hit lands: the swing at the margin's edge DAMAGES the worm (a gauntlet HitEvent on its id) with the row on, nothing off
        bool dmgOk = t.swingDamage && !f.swingDamage;
        Console.WriteLine($"  the swing's hit event (damage on the worm) at the margin: OFF {(f.swingDamage ? "HIT" : "none")}, ON {(t.swingDamage ? "HIT" : "none")}  {(patched ? dmgOk ? "PASS" : "RED" : "patch pending")}");
        if (patched && !dmgOk) bad++;
        // the stagger: a swing while the head is punchable (the charge's window), the eye at the head box's surface + range + headReachMargin + bossMargin - 0.05
        var st = new Dictionary<bool, string>();
        var sg = new Dictionary<bool, int>();
        foreach (bool on in new[] { false, true }) { st[on] = Reach34Stagger(on, gRange, margin, out int n); sg[on] = n; }
        bool stagOk = sg[true] == 1 && sg[false] == 0;
        Console.WriteLine($"  stagger at the margin: OFF {st[false]}; ON {st[true]}  {(patched ? stagOk ? "PASS" : "RED" : "patch pending")}");
        if (patched && !stagOk) bad++;
        if (Array.IndexOf(args, "--nobosses") < 0)
        {
            try
            {
                if (Array.IndexOf(args, "--bossesoff") >= 0)
                {
                    SurvivalSim.Force34Reach = false;
                    Console.WriteLine("  bosses with reach34 forced OFF (the before line):");
                    Bosses33Command(new[] { "--nocamp", "--seeds", seeds.ToString(CultureInfo.InvariantCulture) });
                }
                SurvivalSim.Force34Reach = true;
                if (Array.IndexOf(args, "--flow") >= 0) { SurvivalSim.Force34Flow = true; Console.WriteLine("  (--flow: flow34 forced ON for the boss pilots too)"); }
                Console.WriteLine($"  bosses with reach34 forced ON{(patched ? "" : " (the swing's margin needs the Combat.cs patch: only the blast's is live here)")}:");
                if (Bosses33Command(new[] { "--nocamp", "--seeds", seeds.ToString(CultureInfo.InvariantCulture) }) != 0) bad++;
            }
            finally { SurvivalSim.Force34Reach = null; SurvivalSim.Force34Flow = null; }
        }
        Console.WriteLine($"reach34 {(bad == 0 ? "PASS" : "RED")}");
        return bad == 0 ? 0 : 1;
    }

    /// <summary>Boss32Run's worm rig with reach34 on or off; null when the fight did not start.</summary>
    static SurvivalSim Reach34WyrmSim(bool on, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(Cfg30("wyrm30"), Map());
        sim.noLab = sim.noRecords = true;
        sim.n24Override = new N24Blocks { wyrm = true };
        sim.n26Override = new N26Blocks { level3 = true };
        sim.n29Override = new N29Blocks { boss1 = true };
        sim.n30Override = new N30Blocks { wyrm30 = true };
        sim.n31Override = new N31Blocks { wyrm31 = true };
        sim.n34Override = new N34Blocks { reach34 = on };
        sim.setup.mode = RunSetup.Survival;
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.Wyrm24Begin()) return null;
        sim.Respawn(sim.Wyrm24Spawn, 0);
        sim.mover.invulnerable = true;
        foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        return sim;
    }

    /// <summary>Idle steps (looking at the worm) until the head is high enough to float the player beside it (feet above the floor), up to 30 s.</summary>
    static bool Reach34WaitHead(SurvivalSim sim, Wyrm w, double floorY, int minSteps)
    {
        for (int k = 0; k < 3600; k++)
        {
            // the head high enough, and a phase whose hits land (Wyrm.Filter blocks the intro, the beat and the finisher)
            if (k >= minSteps && w.body.position.y - sim.cfg.movement.body.eyeHeight > floorY + 0.5 && w.phase != "intro" && w.phase != "beat" && w.phase != "dying") return true;
            var (y, p) = ArclineLook(sim.Eye, w.body.position);
            Tick27(sim, y, p, new CombatInput { weapon = -1 });
        }
        return false;
    }

    /// <summary>The sphere-to-box distance (WyrmBox.Touches' measure): how far `p` is from the oriented box.</summary>
    static double Reach34BoxDist(WyrmBox b, Vec3 p)
    {
        Vec3 q = p - b.c;
        double x = Vec3.Dot(q, b.r), y = Vec3.Dot(q, b.u), z = Vec3.Dot(q, b.f);
        double dx = x - Math.Max(-b.hr, Math.Min(b.hr, x)), dy = y - Math.Max(-b.hu, Math.Min(b.hu, y)), dz = z - Math.Max(-b.hf, Math.Min(b.hf, z));
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>The stagger at the margin: the fight runs (the player standing at the spawn, invulnerable) until the head is punchable (the charge's
    /// window, sim.Wyrm30Punchable); then the eye goes on the line from the head box's centre toward the player, where its distance to the box is
    /// gRange + headReachMargin + bossMargin - 0.05 (inside the ON reach, outside the OFF one), and one swing (the lunge off). `staggers`: this swing's.</summary>
    static string Reach34Stagger(bool on, double gRange, double margin, out int staggers)
    {
        staggers = -1;
        SurvivalSim sim = Reach34WyrmSim(on, 1);
        if (sim == null) return "no fight";
        Wyrm w = sim.wyrm24;
        double eyeH = sim.cfg.movement.body.eyeHeight, headReach = sim.cfg.Num("design.n30.wyrm30.headReachMargin");
        int swingHits = 0, hits = 0;
        sim.events.On<MeleeEvent>("meleeSwing", e => { if (e.owner == "player") swingHits += e.hits; });
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet" && e.enemy == w.body.id) hits++; });
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        Vec3 home = sim.Wyrm24Spawn;
        for (int k = 0; k < 180 && !(sim.loadout.Weapon == "gauntlet" && sim.loadout.ready); k++)
        {
            var (y, p) = ArclineLook(sim.Eye, w.body.position);
            Tick27(sim, y, p, new CombatInput { weapon = k == 0 ? slot : -1 });
        }
        bool found = false;
        for (int k = 0; k < 120 * 90 && !found; k++)
        {
            if (sim.Wyrm30Punchable && w.Alive) { found = true; break; }
            var (y, p) = ArclineLook(sim.Eye, w.body.position);
            Tick27(sim, y, p, new CombatInput { weapon = -1 });
            if (Vec3.DistanceTo(sim.mover.position, home) > 6) sim.Respawn(home, 0); // stay where the worm charges
        }
        if (!found) return "the head never became punchable in 90 s";
        WyrmBox b = w.boxes[0];
        Vec3 d = Vec3.Normalize(sim.Eye - b.c);
        double want = gRange + headReach + margin - 0.05, lo = 0, hi = 30;
        for (int i = 0; i < 60; i++) { double mid = (lo + hi) / 2; if (Reach34BoxDist(b, b.c + d * mid) < want) lo = mid; else hi = mid; }
        Vec3 eye = b.c + d * hi;
        sim.Respawn(new Vec3(eye.x, eye.y - eyeH, eye.z), 0);
        double boxDist = Reach34BoxDist(b, sim.Eye), capSurface = Vec3.DistanceTo(sim.Eye, w.body.position) - w.body.Radius;
        var (sy, sp) = ArclineLook(sim.Eye, w.body.position);
        int s0 = sim.wyrm30Staggers, h0 = hits, p0 = swingHits;
        AimAssist keep = sim.loadout.assist;
        sim.loadout.assist = null;
        Tick27(sim, sy, sp, new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } });
        sim.loadout.assist = keep;
        for (int k = 0; k < 2; k++) Tick27(sim, sy, sp, new CombatInput { weapon = -1 });
        staggers = sim.wyrm30Staggers - s0;
        SurvivalSim.Wyrm24Dir = null;
        return $"box distance {boxDist:0.000} m (want {want:0.00}), capsule surface {capSurface:0.000} m, pick {swingHits - p0}, hit events {hits - h0}, staggers {staggers}";
    }

    static Reach34Row Reach34Wyrm(bool on, uint seed, double gRange, double margin)
    {
        var r = new Reach34Row();
        SurvivalSim sim = Reach34WyrmSim(on, seed);
        if (sim == null) { r.note = "no fight"; return r; }
        Wyrm w = sim.wyrm24;
        double floorY = sim.Wyrm24Spawn.y, eyeH = sim.cfg.movement.body.eyeHeight;
        int hits = 0, swings = 0, swingHits = 0;
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet" && e.enemy == w.body.id) hits++; });
        sim.events.On<MeleeEvent>("meleeSwing", e => { if (e.owner == "player") { swings++; swingHits += e.hits; } });
        int blasts = 0, blastHits = 0, denied = 0;
        double atFire = double.NaN;
        sim.events.On<Pulse27Event>("pulse27Blast", e =>
        {
            blasts++;
            blastHits += e.hits;
            // the geometry the blast saw: the worm's aim point now (the worm moves after the loadout) against the line from the eye along +x
            Vec3 eyeNow = new Vec3(sim.mover.position.x, sim.mover.position.y + sim.cfg.movement.body.eyeHeight, sim.mover.position.z);
            Vec3 rel0 = sim.wyrm24.body.AimPoint - eyeNow;
            atFire = (rel0 - new Vec3(1, 0, 0) * rel0.x).Length - sim.wyrm24.body.Radius - sim.loadout.pulse27.p.blastRadius;
        });
        sim.events.On<Pulse27Event>("pulse27Denied", _ => denied++);
        reach34Seen.Clear();
        sim.events.On<HitEvent>("hit", e => reach34Seen.Add($"{e.weapon}/{e.kind}/{e.enemy}/{e.damage:0.#}"));
        // the gauntlet in hand, ready
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        for (int k = 0; k < 180 && !(sim.loadout.Weapon == "gauntlet" && sim.loadout.ready); k++)
        {
            var (y, p) = ArclineLook(sim.Eye, w.body.position);
            Tick27(sim, y, p, new CombatInput { weapon = k == 0 ? slot : -1 });
        }
        if (sim.loadout.Weapon != "gauntlet") { r.note = "no gauntlet"; return r; }
        double cone = sim.cfg.Num("weapons.gauntlet.coneDeg") * Math.PI / 180;

        // a + b: the swing at surface + range + margin - 0.05
        if (!Reach34WaitHead(sim, w, floorY, 0)) { r.note = "the head never rose"; return r; }
        Vec3 head = w.body.position;
        Vec3 u = new Vec3(1, 0, 0);
        Vec3 eye = head + u * (w.body.Radius + (reach34At ?? gRange + margin - 0.05));
        sim.Respawn(new Vec3(eye.x, eye.y - eyeH, eye.z), 0);
        Vec3 dir = Vec3.Normalize(head - sim.Eye);
        foreach (TargetPick pk in CombatMath.SweepTargets(sim.Eye, dir, sim.loadout.GauntletTargets34(), gRange, cone))
            if (pk.target.enemy == w.body) r.mathHit = true;
        var (sy, sp) = ArclineLook(sim.Eye, head);
        int h0 = hits;
        // the swing's own reach: the assist's lunge (aim.lunge 1.5 m, POWER P2) is taken off for this swing (it would close any gap up to 1.5 m, on or off)
        AimAssist keep = sim.loadout.assist;
        sim.loadout.assist = null;
        Tick27(sim, sy, sp, new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } });
        sim.loadout.assist = keep;
        r.swingSurface = Vec3.DistanceTo(sim.Eye, head) - w.body.Radius;
        for (int k = 0; k < 2; k++) Tick27(sim, sy, sp, new CombatInput { weapon = -1 });
        // a pick is the hit (meleeSwing.hits); the worm's damage filter (armor, a buried phase) decides separately whether a HitEvent follows
        r.swingHit = swingHits > 0;
        r.swingDamage = hits > h0;
        r.note = $"[swing picks {swingHits} of {swings}, gauntlet HitEvents on the worm {hits - h0}, stagger {sim.wyrm30Staggers}] ";

        // c: the LT blast with a full glove, the aim point blastRadius + R + margin - 0.05 m off the line
        PulseCharge27 pc = sim.loadout.pulse27;
        r.blastLive = pc != null && pc.active;
        if (r.blastLive)
        {
            if (!Reach34WaitHead(sim, w, floorY, 360)) { r.note = "the head never rose (blast)"; return r; }
            pc.pips = pc.p.pips;
            pc.cooldownSteps = 0;
            Vec3 a = w.body.AimPoint, perp = new Vec3(0, 0, 1);
            // half the margin past the body: the live worm (phase p1) moves about 0.14 m between the placement and the blast's own test (measured at the
            // fire: `offset at the fire`), so the - 0.05 placement the swing uses would land outside the ON reach; margin / 2 keeps the case inside ON, outside OFF
            double s = pc.p.blastRadius + w.body.Radius + margin * 0.5;
            Vec3 be = a - u * 3 + perp * s;
            sim.Respawn(new Vec3(be.x, be.y - eyeH, be.z), 0);
            Vec3 e0 = sim.Eye;
            var (by, bp) = ArclineLook(e0, e0 + u * 10);
            h0 = hits;
            Tick27(sim, by, bp, new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = true } });
            Vec3 rel = a - sim.Eye;
            r.blastOff = (rel - u * Vec3.Dot(rel, u)).Length - w.body.Radius - pc.p.blastRadius;
            for (int k = 0; k < 2; k++) Tick27(sim, by, bp, new CombatInput { weapon = -1 });
            r.blastHit = hits > h0;
            r.note += $"[blast events {blasts}, its hits {blastHits}, offset at the fire {atFire:0.000} m, denied {denied}, worm phase {w.phase}, gun busy {sim.loadout.rules30?.gunAction}] ";
            r.note += $"(blast offset past radius + R: {r.blastOff:0.000} m, glove {pc.pips} pips after)";
        }
        r.ok = true;
        SurvivalSim.Wyrm24Dir = null;
        return r;
    }
}
