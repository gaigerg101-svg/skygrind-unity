// NIGHT33 builder W (DESIGN33 W, D-3 / D-4): the whip's and the piercer's headless judges. ONE entry method, N33Weapons(cmd, args, ref code) (declared
// and called in Program.cs).
//   pierce33 [--off]   D-4 (design.n33.pierce33): the SURVIVAL map, invulnerable, the KESTREL in hand, five husks pinned in a line 5, 7, 9, 11, 13 m ahead
//                      in an open direction, aimed at the first one's aim point (the line runs through all five); one press held for the hold + the
//                      full charge + 0.05 s, then released: ONE charged shot. Run A: the husks' real hit points (kills). Run B: bag husks (1e9 hp; the
//                      damage each body took). Also: the kestrelPierce33 event (its hits, how far its end is from the wall the eye's ray meets) and
//                      the sim's hitstop request ("kestrelPierce33"). GREEN (row on): 5 / 5 bodies hit, each for damageMult x weapons.revolver.damage,
//                      the beam's end at the wall (within 0.01 m), one hitstop request. `--off`: the RED side (n31.charge: 3 bodies at 4x, no beam).
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N33Weapons(string cmd, string[] args, ref int code)
    {
        if (cmd == "pierce33") Pierce33Command(args, ref code);
        else if (cmd == "whip33") Whip33Command(args, ref code); // Harness.N33.Whip.cs
        else if (cmd == "whipboss33") WhipBoss33Command(args, ref code); // Harness.N33.Whip.cs
    }

    sealed class Pierce33Result
    {
        public int charged, chargedHits, kills, beams, beamHits, hitstops;
        public double beamToWall = double.NaN, lineTotal;
        public readonly List<double> perBody = new List<double>();
    }

    static Pierce33Result Pierce33Run(GameConfig cfg, SurvivalMap map, bool on, bool bags)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n31Override = new N31Blocks { charge = true };
        sim.n33Override = new N33Blocks { pierce33 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3301, null, null);
        sim.holdRunClock = true; // no director spawns: the five bodies are the only husks (husk.maxCount)
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        int slot = sim.loadout.owned.IndexOf("revolver");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 15);
        Vec3 sp = map.playerSpawn;
        double[] at = { 5, 7, 9, 11, 13 };
        var line = new Husk[at.Length];
        var pin = new Vec3[at.Length];
        for (int i = 0; i < at.Length; i++)
        {
            for (int k = 0; k < 6 && line[i] == null; k++) // a step or a kerb under the spot: lift the body until its capsule is clear
                line[i] = sim.husks.Spawn("husk", new Vec3(sp.x + ux * at[i], sp.y + 0.25 * k, sp.z + uz * at[i]), sp);
            if (line[i] == null) continue;
            pin[i] = line[i].position;
            if (bags) line[i].hp = line[i].maxHp = 1e9;
        }
        Console.WriteLine($"  ({(bags ? "B" : "A")}) bodies placed {line.Count(h => h != null)} / 5 [{string.Join(" ", line.Select(h => h == null ? "-" : "o"))}]");
        var r = new Pierce33Result();
        var dmg = new Dictionary<int, double>(); // the charged shot's damage per body (hits from the release step on; the press's tap shot is not counted)
        double holdS = cfg.Num("design.n31.charge.holdSeconds"), fullS = cfg.Num("design.n31.charge.revolver.fullSeconds");
        int lead = Steps(0.5), end = lead + Steps(holdS) + Steps(fullS) + Steps(0.05), stop = end + Steps(0.3);
        int s = 0;
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (e.gun == "revolver") { r.charged++; r.chargedHits += e.hits; } });
        sim.events.On<HitEvent>("hit", e => { if (s >= end && e.owner == "player" && e.weapon == "revolver") dmg[e.enemy] = (dmg.TryGetValue(e.enemy, out double d) ? d : 0) + e.damage; });
        sim.events.On<string>("hitstop", c => { if (c == Loadout.KestrelPierce33) r.hitstops++; });
        sim.events.On<RailFireEvent>(Loadout.KestrelPierce33, e =>
        {
            r.beams++;
            r.beamHits += e.hits;
            Vec3 d = e.to - e.from;
            double len = d.Length;
            if (len > 0 && sim.world.RayIntersect(e.from, d / len, out RayHit wall)) r.beamToWall = Math.Abs(wall.distance - len);
        });
        for (s = 0; s < stop; s++)
        {
            for (int i = 0; i < at.Length; i++)
                if (line[i] != null && line[i].alive) line[i].position = pin[i];
            double py = 0;
            Husk aim = line[line.Length - 1]; // the far body: the ray from the eye then crosses every body in between
            if (aim != null && s < end) (yaw, py) = ArclineLook(sim.Eye, aim.AimPoint);
            bool down = s >= lead && s < end, prev = s - 1 >= lead && s - 1 < end;
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = down && !prev } });
        }
        foreach (Husk h in line)
        {
            if (h == null) continue;
            if (!h.alive) r.kills++;
            double d = dmg.TryGetValue(h.id, out double v) ? v : 0;
            r.perBody.Add(d);
            r.lineTotal += d;
        }
        return r;
    }

    static void Pierce33Command(string[] args, ref int code)
    {
        bool on = !Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        double baseDmg = cfg.Num("weapons.revolver.damage");
        double mult = on ? cfg.Num("design.n33.pierce33.damageMult") : cfg.Num("design.n31.charge.revolver.damageMult");
        int pierce = on ? cfg.Int("design.n33.pierce33.pierce") : cfg.Int("design.n31.charge.revolver.pierce");
        Console.WriteLine($"pierce33: {(on ? "design.n33.pierce33 ON" : "RED side: design.n31.charge's charged KESTREL (pierce33 off)")}: pierce {pierce}, damageMult {mult}");
        Pierce33Result a = Pierce33Run(cfg, map, on, false), b = Pierce33Run(cfg, map, on, true);
        Console.WriteLine($"  A (real hp, 5 husks in a line 5-13 m): charged shots {a.charged}, bodies hit {a.chargedHits} / 5, kills {a.kills} / 5, damage on the line {a.lineTotal:0}");
        Console.WriteLine($"  B (bags): bodies hit {b.chargedHits} / 5, damage per body [{string.Join(", ", b.perBody.Select(d => d.ToString("0")))}] (want {baseDmg * mult:0}, {baseDmg * mult * cfg.Num("weapons.revolver.headshotMult"):0} on a headshot, to {Math.Min(5, pierce)} bodies), total {b.lineTotal:0}");
        Console.WriteLine($"  beam event {Loadout.KestrelPierce33}: {b.beams} (hits {b.beamHits}), its end {(double.IsNaN(b.beamToWall) ? "n/a" : b.beamToWall.ToString("0.000") + " m")} from the wall; hitstop requests {b.hitstops}");
        if (on)
        {
            Need(a.charged == 1 && b.charged == 1, "one charged shot each run");
            Need(b.chargedHits == 5 && b.perBody.All(d => Math.Abs(d - baseDmg * mult) < 1e-6 || Math.Abs(d - baseDmg * mult * cfg.Num("weapons.revolver.headshotMult")) < 1e-6), "5 bodies at damageMult x");
            Need(a.kills == 5, $"kills {a.kills} / 5");
            Need(b.beams == 1 && !double.IsNaN(b.beamToWall) && b.beamToWall < 0.01, "beam ends at the wall");
            Need(b.hitstops == 1, "one hitstop request");
        }
        else Need(b.beams == 0 && b.chargedHits <= 3, "RED side: n31's 3-body charge, no beam");
        Console.WriteLine(fails.Count == 0 ? (on ? "pierce33 GREEN" : "pierce33 RED side as expected (n31's charge)") : $"pierce33 FAIL: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }
}
