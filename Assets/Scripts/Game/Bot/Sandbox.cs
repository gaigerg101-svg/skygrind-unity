// NIGHT19 U3-33.2 (the lead owns this file; builders never edit it: NIGHT19.md "How tonight works" rule 10): the sandbox
// judges' shared parts. THE RANGE (SandboxRange.cs), THE CHASE (SandboxChase.cs) and the parity farm (Farm.cs's gun
// lock) run on the real composition (SurvivalSim.Headless on the Survival map), headless and deterministic: a judge is a
// pure function of (the build, the config, the seed). Frozen at the baseline (Logs/sandbox/b0): the cells
// (Tools/sandbox-cells.json), the aim model, the scores and the bots.
// - The cells: a situation each (kinds, counts, distances, the round's HP, the player's script); loaded from the json.
// - The open spot: the Survival map's widest open floor, found once per composition by casting the C# collider (never a
//   guess: the spawn at (0, 0, 40) is enclosed), with every gate open (the bench's rule: the whole map; a locked zone's
//   nav nodes are blocked) and a nav node under it so the kinds path to the player. The judges open every gate too.
// - The aim model, identical for every gun: the body centre (the head when the gun has a head bonus and the target is a
//   HUSK), a seeded error (sigma 0.6 deg standing, 1.5 deg moving) from the judge's own Mulberry32 stream (never the
//   run's), projectiles lead the target's current velocity, the trigger as fast as the gun allows (the LONGWAVE: held to a
//   full charge, then let go).
// No UnityEngine use (Tools/SimHarness compiles this folder).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>One group of a cell: `count` of `kind` at `distance` m ahead, spread `spread` m across (or a `line`: one
    /// every `line` m from `distance` on), with an optional affix.</summary>
    public sealed class SandboxGroup
    {
        public string kind = "husk", affix;
        public int count = 1;
        public double distance = 10, spread, line;
    }

    public sealed class SandboxCell
    {
        public string id, name, script = "stand";
        public int round = 1;
        public bool forged;
        public readonly List<SandboxGroup> groups = new List<SandboxGroup>();
    }

    /// <summary>The judges' frozen settings (Tools/sandbox-cells.json).</summary>
    public sealed class SandboxSpec
    {
        public int version;
        public double sigmaStandDeg = 0.6, sigmaMoveDeg = 1.5, capSeconds = 30, damageWeight = 0.05, parryRange = 2;
        public double strafeMeters = 4, boostEvery = 2, rotateEvery = 3;
        public int seeds = 10;
        public uint aimSalt = 0x5A4D1913;
        public readonly List<SandboxCell> cells = new List<SandboxCell>();

        public SandboxCell Cell(string id)
        {
            foreach (SandboxCell c in cells) if (c.id == id) return c;
            return null;
        }

        static double N(Dictionary<string, object> d, string k, double fallback) => d.TryGetValue(k, out object v) && v != null ? PortData.Num(v) : fallback;

        public static SandboxSpec Load(string path)
        {
            var root = PortData.Obj(PortData.LoadJson(path));
            var s = new SandboxSpec { version = (int)N(root, "version", 1) };
            var aim = PortData.Obj(root["aim"]);
            s.sigmaStandDeg = N(aim, "sigmaStandDeg", s.sigmaStandDeg);
            s.sigmaMoveDeg = N(aim, "sigmaMoveDeg", s.sigmaMoveDeg);
            s.aimSalt = (uint)N(aim, "seedSalt", s.aimSalt);
            var score = PortData.Obj(root["score"]);
            s.capSeconds = N(score, "capSeconds", s.capSeconds);
            s.damageWeight = N(score, "damageWeight", s.damageWeight);
            s.seeds = (int)N(score, "seeds", s.seeds);
            var script = PortData.Obj(root["script"]);
            s.parryRange = N(script, "parryRange", s.parryRange);
            s.strafeMeters = N(script, "strafeMeters", s.strafeMeters);
            s.boostEvery = N(script, "boostEvery", s.boostEvery);
            s.rotateEvery = N(script, "rotateEvery", s.rotateEvery);
            foreach (object o in PortData.Arr(root["cells"]))
            {
                var d = PortData.Obj(o);
                var c = new SandboxCell
                {
                    id = (string)d["id"], name = (string)d["name"], script = d.TryGetValue("script", out object sc) ? (string)sc : "stand",
                    round = (int)N(d, "round", 1), forged = d.TryGetValue("forged", out object f) && f is bool fb && fb,
                };
                foreach (object g in PortData.Arr(d["groups"]))
                {
                    var gd = PortData.Obj(g);
                    c.groups.Add(new SandboxGroup
                    {
                        kind = (string)gd["kind"], count = (int)N(gd, "count", 1), distance = N(gd, "distance", 10), spread = N(gd, "spread", 0),
                        line = N(gd, "line", 0), affix = gd.TryGetValue("affix", out object a) ? a as string : null,
                    });
                }
                s.cells.Add(c);
            }
            return s;
        }
    }

    public static class Sandbox
    {
        /// <summary>The judges' version (a report line; a change to a judge after the baseline is a new version, logged).</summary>
        public const string Version = "n19-judges-1";

        /// <summary>The guns THE RANGE scores in S1-S2 (the GAUNTLET is the melee slot: scored, outside S1-S2), in this order:
        /// the five that fire today, then the ports when the loadout fires them.</summary>
        public static readonly string[] BaseGuns = { "revolver", "shotgun", "nailgun", "rocket", "rail" };

        public static readonly string[] PortGuns = { "zapper", "bouncer", "torch" }; // NIGHT20: the TORCH ported (builder G, B5)

        /// <summary>The reachable guns on this build: the base five, plus each port the loadout fires (Loadout.Fires).</summary>
        public static List<string> Reachable(SurvivalSim sim)
        {
            var list = new List<string>(BaseGuns);
            foreach (string p in PortGuns) if (Loadout.Fires(p)) list.Add(p);
            return list;
        }

        public static string Name(GameConfig cfg, string gun) => cfg.Has($"weapons.catalog.{gun}.name") ? cfg.Str($"weapons.catalog.{gun}.name") : gun.ToUpperInvariant();

        public static double Median(List<double> v)
        {
            if (v.Count == 0) return double.NaN;
            var s = new List<double>(v);
            s.Sort();
            int n = s.Count;
            return n % 2 == 1 ? s[n / 2] : (s[n / 2 - 1] + s[n / 2]) / 2;
        }

        public static string F(double v, string fmt = "0.00") => double.IsNaN(v) ? "n/a" : v.ToString(fmt, CultureInfo.InvariantCulture);

        // --- the open spot -------------------------------------------------------------------------------------------------

        /// <summary>The widest open floor of the map and the heading with the longest clear view (found once per composition):
        /// a 4 m grid over the floor, each point with a nav node under it, scored by the least clearance of five rays across
        /// +/-30 deg of a heading (16 headings), needing strafeRoom m left and right; the best point and heading win (the
        /// first in grid order on a tie).</summary>
        public static (Vec3 pos, double yaw, double clear) OpenSpot(SurvivalSim sim, double strafeRoom = 6)
        {
            sim.zones.OpenAll(); // a locked zone's nav nodes are blocked: the whole map, as the bench plays it
            MapBox fl = sim.map.floor;
            double y = fl.max.y;
            Vec3 bestPos = sim.map.playerSpawn;
            double bestYaw = 0, best = -1;
            double below = sim.cfg.Num("ai.grid.lookBelow"), above = sim.cfg.Num("ai.grid.lookAbove");
            for (double x = fl.min.x + 4; x <= fl.max.x - 4; x += 4)
            {
                for (double z = fl.min.z + 4; z <= fl.max.z - 4; z += 4)
                {
                    if (sim.nav.grid.NodeAt(x, y, z, below, above) < 0) continue;
                    var p = new Vec3(x, y, z);
                    if (Clear(sim, p, 0) < 1 || Clear(sim, p, Math.PI / 2) < 1) continue; // inside or touching geometry
                    for (int h = 0; h < 16; h++)
                    {
                        double yaw = h * Math.PI / 8;
                        if (Clear(sim, p, yaw + Math.PI / 2) < strafeRoom || Clear(sim, p, yaw - Math.PI / 2) < strafeRoom) continue;
                        double m = double.PositiveInfinity;
                        for (int k = -2; k <= 2; k++) m = Math.Min(m, Clear(sim, p, yaw + k * Math.PI / 12));
                        if (m > best + 1e-9)
                        {
                            best = m;
                            bestPos = p;
                            bestYaw = yaw;
                        }
                    }
                }
            }
            return (bestPos, bestYaw, best);
        }

        /// <summary>The clear distance along a level ray from `p` (at 0.4 m and 1.4 m over the floor) toward `yaw` (yaw 0 looks
        /// toward -z), capped at 60 m.</summary>
        public static double Clear(SurvivalSim sim, Vec3 p, double yaw)
        {
            var d = new Vec3(-JsMath.Sin(yaw), 0, -JsMath.Cos(yaw));
            double m = 60;
            foreach (double h in new[] { 0.4, 1.4 })
                if (sim.world.RayIntersect(new Vec3(p.x, p.y + h, p.z), d, out RayHit hit) && hit.distance < m) m = hit.distance;
            return m;
        }

        /// <summary>A point `ahead` m along `yaw` and `side` m to its right, from `p`.</summary>
        public static Vec3 Along(Vec3 p, double yaw, double ahead, double side)
        {
            double fx = -JsMath.Sin(yaw), fz = -JsMath.Cos(yaw); // forward
            double rx = -fz, rz = fx; // right
            return new Vec3(p.x + fx * ahead + rx * side, p.y, p.z + fz * ahead + rz * side);
        }

        // --- the aim model ---------------------------------------------------------------------------------------------------

        /// <summary>A standard normal from the judge's stream (Box-Muller; one draw per call, the pair's second discarded so
        /// every call takes exactly two numbers).</summary>
        public static double Gauss(Mulberry32 rng)
        {
            double u1 = Math.Max(1e-12, rng.Next()), u2 = rng.Next();
            return Math.Sqrt(-2 * JsMath.Log(u1)) * JsMath.Cos(2 * Math.PI * u2); // fdlibm: the same bits on Mono and CoreCLR
        }

        /// <summary>The gun's head bonus (weapons.(gun).headshotMult over 1).</summary>
        public static bool HeadBonus(GameConfig cfg, string gun) => cfg.Has($"weapons.{gun}.headshotMult") && cfg.Num($"weapons.{gun}.headshotMult") > 1;

        /// <summary>The gun's projectile speed (0: hitscan): weapons.(gun).speed.</summary>
        public static double ProjectileSpeed(GameConfig cfg, string gun) => gun != "rail" && cfg.Has($"weapons.{gun}.speed") ? cfg.Num($"weapons.{gun}.speed") : 0;

        /// <summary>Where the model aims at `e` from `eye` with `gun`: the head (a HUSK, a gun with a head bonus) or the body
        /// centre, led by the target's velocity over the projectile's flight time.</summary>
        public static Vec3 AimPoint(GameConfig cfg, string gun, Enemy e, Vec3 eye)
        {
            bool head = HeadBonus(cfg, gun) && e.kind == "husk" && e is Husk;
            Vec3 a = head ? new Vec3(e.position.x, e.position.y + cfg.Num("husk.headHeight"), e.position.z) : e.AimPoint;
            double speed = ProjectileSpeed(cfg, gun);
            if (speed > 0)
            {
                double dx = a.x - eye.x, dy = a.y - eye.y, dz = a.z - eye.z;
                double t = Math.Sqrt(dx * dx + dy * dy + dz * dz) / speed;
                a = new Vec3(a.x + e.velocity.x * t, a.y + e.velocity.y * t, a.z + e.velocity.z * t);
            }
            return a;
        }

        /// <summary>The yaw / pitch from `eye` to `p` (yaw 0 looks toward -z: SurvivalGame.Aim).</summary>
        public static void Look(Vec3 eye, Vec3 p, out double yaw, out double pitch)
        {
            double dx = p.x - eye.x, dy = p.y - eye.y, dz = p.z - eye.z;
            yaw = JsMath.Atan2(-dx, -dz);
            pitch = JsMath.Atan2(dy, Math.Sqrt(dx * dx + dz * dz));
        }

        /// <summary>The trigger as fast as the gun allows: held and pressed every step; the LONGWAVE held to a full charge,
        /// then let go for one step (it fires on the release).</summary>
        public static Button Trigger(SurvivalSim sim, string weapon)
        {
            if (weapon == "rail" && sim.loadout.RailCharge >= 1) return default;
            return new Button { held = true, pressed = true };
        }

        // --- reports ---------------------------------------------------------------------------------------------------------

        public static string Dir(string root, string tag)
        {
            string d = Path.Combine(root, "Logs", "sandbox", tag);
            Directory.CreateDirectory(d);
            return d;
        }

        public static void Write(string path, string text) => File.WriteAllText(path, text.Replace("\r\n", "\n"), new UTF8Encoding(false));

        /// <summary>A JSON string literal.</summary>
        public static string Q(string s)
        {
            if (s == null) return "null";
            var b = new StringBuilder(s.Length + 2).Append('"');
            foreach (char c in s)
            {
                if (c == '"' || c == '\\') b.Append('\\').Append(c);
                else if (c < ' ') b.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                else b.Append(c);
            }
            return b.Append('"').ToString();
        }
    }
}
