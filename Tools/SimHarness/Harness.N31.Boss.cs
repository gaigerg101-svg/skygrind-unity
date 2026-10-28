// NIGHT31 builder B: `camp31 [lantern|glacier|wyrm|loom|stag|all] [--first n] [--pilot s] [--dbg]` (DESIGN31 E + R2, the CAMP TEST).
// Declared and called by the lead's Program.cs (N31Boss). Four MORTAL campers per boss (centre, edge / corner, the highest reachable spot, the farthest
// reachable point; the Stag adds the wall corner): each walks back to its spot when pushed off it, aims at the boss's centre and fires its best owned
// weapon by the boss's level (DESIGN31 A: L1 KESTREL; L2 + gauntlet; L3 + THUNDERCLAP; L4 + NAILGUN; the gauntlet only inside its reach, the
// THUNDERCLAP only inside 12 m). The 30 s clock starts when the boss can be hurt. A camper LOSES (the boss wins, GOOD) only when it DIES or loses at
// least 50 % of max health within 30 s AND its damage rate is under a third of the moving pilot's (R2: both). Otherwise it CAMPS (BAD).
// The pilot's kill time is the NIGHT30 pilots' (boss30): the Lantern rider, the Glacier glide chaser, the Wyrm's human-lag WyrmBot, the Loom's lag bot.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N31Boss(string cmd, string[] args, ref int code)
    {
        if (cmd == "stagglide31") { code = StagGlide31(); return; }
        if (cmd == "parry31b") { code = Parry31B(); return; }
        if (cmd == "loompit31") { code = LoomPit31(); return; }
        if (cmd == "loomsight31") { code = LoomSight31(Array.IndexOf(args, "--off") >= 0); return; }
        if (cmd != "camp31") return;
        code = 0;
        string which = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int first = 1;
        double pilotOver = -1;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--first") int.TryParse(args[i + 1], out first);
            if (args[i] == "--pilot") double.TryParse(args[i + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pilotOver);
        }
        bool dbg = Array.IndexOf(args, "--dbg") >= 0;
        designRows31 = Array.IndexOf(args, "--design") >= 0;
        framesEmu31 = Array.IndexOf(args, "--frames") >= 0;
        sets30.Clear();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        try
        {
            foreach (string b in new[] { "lantern", "lantern31", "glacier", "stag", "wyrm", "wyrm31", "loom", "loom31" })
                if (which == "all" || which == b) code |= Camp31Boss(b, (uint)first, pilotOver, dbg);
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
    }

    // --- the boss adapter ------------------------------------------------------------------------------------------------------------------

    /// <summary>One boss fight set up for a camper: the sim, the boss's alive / hurtable / bar / centre / ray, the level's weapons, and the camp spots.</summary>
    sealed class CampRig31
    {
        public SurvivalSim sim;
        public Func<bool> alive, hurtable;
        public Func<double> hpFrac;
        public Func<Vec3> centre;
        public Func<Vec3, Vec3, double> ray;
        public int level;
        public readonly List<(string name, Vec3 at)> spots = new List<(string, Vec3)>();
        public string note = "";
    }

    sealed class CampRow31
    {
        public string name;
        public double alive, lost, rate, dist;
        public bool died, camps;
        public int shots;
        public string weapons = "";
    }

    /// <summary>A fresh fight for `boss` (the NIGHT30 boss rows, or a NIGHT31 row with `n31` on). Each camper gets its own fight.</summary>
    static CampRig31 CampRig31For(string boss, uint seed)
    {
        var rig = new CampRig31();
        if (boss == "lantern" || boss == "lantern31")
        {
            GameConfig cfg = Cfg30("lantern30");
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n27Override = new N27Blocks { boss1 = true };
            sim.n29Override = new N29Blocks { boss1 = true };
            sim.n30Override = new N30Blocks { lantern30 = true };
            if (boss == "lantern31") sim.n31Override = new N31Blocks { lantern31 = true, charge = true };
            sim.n26Override = N26Blocks.Off;
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(seed, null, null);
            sim.husks.frozen = true;
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out _)) return null;
            Lantern27 l = sim.lantern27;
            Lantern30Arena a = sim.lantern30Arena ?? Lantern30Arena.Build(cfg);
            Vec3 o = l.OriginWorld;
            rig.sim = sim; rig.level = 1;
            rig.alive = () => l.Alive; rig.hurtable = () => l.Hurtable; rig.hpFrac = () => l.HpFrac; rig.centre = () => l.Centre; rig.ray = l.Raycast;
            int k = a.p.platformPanels[0];
            int far = (a.p.panels / 2 + k) % a.p.panels;
            Vec3 Ring(int panel, double r, double y) { Vec3 v = -a.InwardNormal(panel) * r; return new Vec3(o.x + v.x, o.y + y, o.z + v.z); }
            rig.spots.Add(("centre (floor)", new Vec3(o.x + 1.5, o.y + 0.05, o.z + 1.5)));
            rig.spots.Add(("edge (floor at the wall)", Ring((k + 4) % a.p.panels, a.p.apothem - 1.2, 0.05)));
            rig.spots.Add(("highest (platform)", Ring(k, a.p.apothem - a.p.platformDepth / 2, a.p.platformTop + 0.05)));
            rig.spots.Add(("farthest (wall, across)", Ring(far, a.p.apothem - 1.2, 0.05)));
            rig.note = $"ring apothem {a.p.apothem} m, {a.p.panels} panels, platform top {a.p.platformTop} m";
            return rig;
        }
        if (boss == "glacier")
        {
            GameConfig cfg = Cfg30("glacier30", "glide");
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n27Override = new N27Blocks { pulse27 = true };
            sim.n29Override = new N29Blocks { boss2 = true };
            sim.n30Override = new N30Blocks { glacier30 = true, glide = true };
            sim.n26Override = new N26Blocks { surfaces = true, level2 = true };
            sim.bosses25Override = true;
            sim.glacier28Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(seed, null, null);
            sim.husks.frozen = true;
            if (!sim.Glacier28Begin()) return null;
            Glacier28 g = sim.glacier28;
            Rink28Arena a = sim.glacier28Rink;
            if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
            rig.sim = sim; rig.level = 2;
            rig.alive = () => g.Alive; rig.hurtable = () => g.IsPlaying; rig.hpFrac = () => g.HpFrac; rig.centre = () => g.box.c; rig.ray = g.Raycast;
            double s0 = a.p.playerStartS;
            Vec3 F(double s, double lat) { Vec3 v = a.FloorPoint(s, lat); return new Vec3(v.x, v.y + 0.1, v.z); }
            rig.spots.Add(("centre (lane middle)", F(s0 + 20, (a.latIsland + a.latLaneOut) / 2)));
            rig.spots.Add(("edge (bank top)", F(s0 + 20, a.latBankTop - 0.6)));
            rig.spots.Add(("highest (the lip)", F(s0 + 20, a.latLip - 0.3)));
            rig.spots.Add(("farthest (bank top, half a lap)", F(s0 + 20 + a.Lap / 2, a.latBankTop - 0.6)));
            rig.note = $"rink lap {a.Lap:0} m, bank top y {a.bankTopY - a.p.cy:0.0} m, lip lat {a.latLip:0.0}";
            return rig;
        }
        if (boss == "wyrm" || boss == "wyrm31")
        {
            GameConfig cfg = Cfg30("wyrm30");
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n24Override = new N24Blocks { wyrm = true };
            sim.n26Override = new N26Blocks { level3 = true };
            sim.n29Override = new N29Blocks { boss1 = true };
            sim.n30Override = new N30Blocks { wyrm30 = true };
            if (boss == "wyrm31") sim.n31Override = new N31Blocks { wyrm31 = true };
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(seed, null, null);
            sim.husks.frozen = true;
            if (!sim.Wyrm24Begin()) return null;
            Wyrm w = sim.wyrm24;
            WyrmArena a = sim.wyrm24Arena;
            Vec3 o = sim.Wyrm24Spawn - a.spawn;
            foreach (string gun in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
            rig.sim = sim; rig.level = 3;
            rig.alive = () => w.Alive; rig.hurtable = () => w.Alive && w.phase != "intro" && w.phase != "beat" && w.phase != "dying"; rig.hpFrac = () => w.HpFrac; rig.centre = () => w.boxes[0].c; rig.ray = w.Raycast;
            // the highest platform: the top of the tallest box that is not a wall (kind != "wall") and is at least 2 x 2 m
            Vec3 best = default;
            double bestY = -1;
            foreach (var (mn, mx, kind) in a.boxes)
            {
                if (kind == "wall" || mx.x - mn.x < 2 || mx.z - mn.z < 2 || mx.y >= a.height - 0.5 || Math.Abs(mn.x) > a.half - 1 && Math.Abs(mx.x) > a.half - 1) continue;
                if (mx.y > bestY && mx.y < 20) { bestY = mx.y; best = new Vec3((mn.x + mx.x) / 2, mx.y + 0.05, (mn.z + mx.z) / 2); }
            }
            double fy = a.spawn.y + 0.05, sx = a.spawn.x >= 0 ? 1 : -1, sz = a.spawn.z >= 0 ? 1 : -1;
            rig.spots.Add(("centre (floor)", o + new Vec3(1, fy, 1)));
            rig.spots.Add(("corner (floor, spawn side)", o + new Vec3(sx * (a.half - 2), fy, sz * (a.half - 2))));
            if (bestY > 0) rig.spots.Add(($"highest (platform top y {bestY:0.0})", o + best));
            rig.spots.Add(("farthest (floor, far corner)", o + new Vec3(-sx * (a.half - 2), fy, -sz * (a.half - 2))));
            rig.note = $"arena half {a.half} m, {a.boxes.Count} boxes";
            return rig;
        }
        if (boss == "loom" || boss == "loom31")
        {
            GameConfig cfg = Cfg30("span");
            SurvivalMap map = CampaignMapFrom("Design/levels/n30/span.json", cfg, out var json);
            var level = LevelDef.From(json);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.n24Override = new N24Blocks { campaign = true };
            sim.n25Override = new N25Blocks { tutorial = true };
            sim.n29Override = new N29Blocks { doorFree = true };
            if (boss == "loom31") sim.n31Override = new N31Blocks { loom31 = true, grapplePull = true };
            sim.setup.mode = Campaign24.Mode;
            sim.StartRun(seed, null, null);
            RoomDef room = level.rooms.Find(r => r.kind == "boss" && r.boss == "loom");
            if (room == null) return null;
            for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.Step(); }
            sim.campaign?.SetDoorFree29(true); // DOORFREE (the hold's effect): every room open, the boss room entered from where the player stands
            sim.Respawn(new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z - 2), room.spawnYawDeg);
            for (int s = 0; s < 600 && (sim.loom30 == null || !sim.loom30.Alive); s++) { sim.health.hp = sim.health.max; sim.Step(); }
            if (sim.loom30 == null || !sim.loom30.Alive) { Console.WriteLine($"  loom rig: span {sim.n30.span}, campaign {(sim.campaign != null ? "room " + sim.campaign.room : "none")}, boss room {room.id}, at ({sim.mover.position.x:0},{sim.mover.position.y:0},{sim.mover.position.z:0}), flow {sim.flow}"); return null; }
            Loom30 l = sim.loom30;
            foreach (string gun in new[] { "gauntlet", "shotgun", "nailgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
            rig.sim = sim; rig.level = 4;
            rig.alive = () => l.Alive; rig.hurtable = () => l.Hurtable; rig.hpFrac = () => l.HpFrac; rig.ray = l.Raycast;
            // the body hangs under the deck (no line of sight from it): the camper aims at the joint nearest its eye instead (the harder test)
            rig.centre = () =>
            {
                Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
                Vec3 best = l.joints[NearestJoint(l, eye)];
                double bd = 1e9;
                foreach (Vec3 j in l.joints)
                {
                    Vec3 d = j - eye;
                    double len = d.Length;
                    if (len < 1e-6 || len >= bd) continue;
                    if (sim.world.RayIntersect(eye, d / len, out RayHit wh) && wh.distance < len - 0.5) continue;
                    bd = len; best = j;
                }
                return best;
            };
            Vec3 c = new Vec3((l.grips[0].x + l.grips[2].x) / 2, l.grips[0].y, (l.grips[0].z + l.grips[2].z) / 2);
            Vec3 In(Vec3 g, double by) { Vec3 d = Vec3.Normalize(new Vec3(c.x - g.x, 0, c.z - g.z)); return new Vec3(g.x + d.x * by, g.y + 0.05, g.z + d.z * by); }
            rig.spots.Add(("centre (deck)", new Vec3(c.x + 1, c.y + 0.05, c.z + 1)));
            rig.spots.Add(("corner (deck grip)", In(l.grips[0], 1.0)));
            rig.spots.Add(("highest (none higher than the deck: far deck corner)", In(l.grips[2], 1.0)));
            rig.spots.Add(("farthest (the entry, room spawn)", new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z)));
            rig.note = $"room {room.id}, deck centre ({c.x:0},{c.y:0},{c.z:0})";
            return rig;
        }
        return CampRig31Stag(boss, seed);
    }

    /// <summary>A fresh STAG fight (design.n31.stag31 on through the override; level 2's weapons: the KESTREL and the PULSE gauntlet; glide on, as in the build).</summary>
    /// <summary>--design: the build's own rows (Design/unity-config.json as it is, every block from the file: what the Player runs), no overrides.</summary>
    static bool designRows31, framesEmu31;

    static SurvivalSim StagSim31(uint seed, bool glide = true)
    {
        GameConfig cfg = designRows31 ? GameConfig.FromText(File.ReadAllText(Path.Combine(Root, "config.json")), File.ReadAllText(Path.Combine(Root, "..", "Design", "unity-config.json"))) : glide ? Cfg30("glacier30", "glide") : Cfg30("glacier30");
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        if (!designRows31)
        {
            sim.n27Override = new N27Blocks { pulse27 = true };
            sim.n29Override = new N29Blocks { boss2 = true };
            sim.n30Override = new N30Blocks { glide = glide, lava = true };
            sim.n26Override = new N26Blocks { surfaces = true, level2 = true };
            sim.n31Override = new N31Blocks { stag31 = true };
        }
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.Stag31Begin(out Vec3 spawn, out double yaw)) return null;
        sim.Respawn(spawn, yaw);
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        return sim;
    }

    /// <summary>`stagglide31`: does the glide (design.n30.glide) see the Stag's ice? On the Stag rink (glide on, then glide off) the player stands on the lane, dashes forward along it
    /// and coasts 1.5 s: prints OnIce30 at the feet, the glide's steps and dash adds, the peak and the 1.5 s speed. PASS: on ice with glide on, the dash adds speed, and it coasts faster than off.</summary>
    static int StagGlide31()
    {
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        var res = new double[2, 2];
        var ice = new bool[2];
        var adds = new int[2];
        for (int g = 0; g < 2; g++)
        {
            SurvivalSim sim = StagSim31(1, g == 0);
            sim.stag31.Frozen = true; // the Stag stands still: only the ice is measured
            Rink28Arena a = sim.stag31Rink;
            double s0 = a.p.playerStartS - 12;
            Vec3 at = a.FloorPoint(s0, (a.latIsland + a.latLaneOut) / 2);
            a.math.PointAt(s0, 0, out _, out _, out double tx, out double tz);
            double yaw = Math.Atan2(-tx, -tz);
            sim.Respawn(new Vec3(at.x, at.y + 0.05, at.z), yaw * 180 / Math.PI);
            var si = (ScriptedInput)sim.input;
            double peak = 0, end = 0;
            for (int i = 0; i < 60 + 180; i++)
            {
                bool dash = i == 60;
                si.SetLook(yaw, 0);
                si.Set(new InputFrame { yaw = yaw, moveZ = i < 60 ? 1 : 0, dash = new Skygrind.Core.Button { held = dash, pressed = dash } }, new CombatInput { weapon = -1 });
                sim.Step();
                sim.health.hp = sim.health.max;
                double sp = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z);
                if (i >= 60) peak = Math.Max(peak, sp);
                end = sp;
                if (i == 30) ice[g] = sim.OnIce30(sim.mover.position);
            }
            res[g, 0] = peak; res[g, 1] = end;
            adds[g] = sim.glide30 != null ? sim.glide30.dashAdds : 0;
            Console.WriteLine($"stagglide31 glide {(g == 0 ? "ON " : "OFF")}: on ice at the feet {ice[g]}, glide steps {(sim.glide30 != null ? sim.glide30.glideSteps : 0)}, dash adds {adds[g]}, dash peak {peak:0.0} m/s, speed 1.5 s after the dash {end:0.0} m/s");
        }
        SurvivalSim.Wyrm24Dir = null;
        bool ok = ice[0] && res[0, 1] > res[1, 1] + 0.5; // the dash add is information (it needs a dash entry speed the glide rule scales)
        Console.WriteLine($"stagglide31 {(ok ? "PASS" : "FAIL")}: the glide {(ok ? "sees" : "does NOT see")} the Stag's ice (coast {res[0, 1]:0.0} vs {res[1, 1]:0.0} m/s with glide off)");
        return ok ? 0 : 1;
    }

    /// <summary>`loompit31`: the deck break as a PIT (the lead, 12:18). The Loom in s8 (loom31 on); a player stands still in the middle of the break rect and another time on the
    /// deck frame; the bar is put just under breakAt by Loom30.Apply (a harness shortcut: the break is the subject, not the fight); after the tell: the middle player falls to the
    /// pit floor and design.n30.lava burns and knocks him (Lava30 touches), the frame player stays on the deck; then a new run: the deck is whole (a player in the middle stays up).</summary>
    static int LoomPit31()
    {
        var fails = new List<string>();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        (double minY, int touches, bool open, double hp) Run(bool middle)
        {
            CampRig31 rig = CampRig31For("loom31", 1);
            SurvivalSim sim = rig.sim;
            for (int i = 0; i < 1200 && !sim.loom30.Hurtable; i++) { sim.health.hp = sim.health.max; sim.Step(); }
            Loom30 l = sim.loom30;
            double[] r = sim.loom31.rect;
            Vec3 at = middle ? new Vec3((r[0] + r[2]) / 2, l.centre.y + 0.05, (r[1] + r[3]) / 2) : new Vec3(Math.Min(r[0], r[2]) - 1.5, l.centre.y + 0.05, (r[1] + r[3]) / 2);
            sim.Respawn(at, 0);
            l.Apply(l.body.hp - l.body.maxHp * (sim.loom31.p.breakAt - 0.01));
            int t0 = sim.loom31.lavaHits;
            sim.events.On<Loom31Event>("loom31", e => Console.WriteLine($"      loom31 {e.stage} at step {sim.simStep}, bar {l.HpFrac:0.00}, pit {sim.loom31.pitOpen}, feet {sim.mover.position.y - l.centre.y:0.00}"));
            double minY = 1e9, hp0 = sim.health.hp;
            bool pitWas = false;
            var si = (ScriptedInput)sim.input;
            for (int i = 0; i < 4 * 120; i++)
            {
                si.Set(new InputFrame(), new CombatInput { weapon = -1 });
                sim.Step();
                minY = Math.Min(minY, sim.mover.position.y - l.centre.y);
                if (sim.loom31.pitOpen != pitWas) { pitWas = sim.loom31.pitOpen; Console.WriteLine($"      pit {(pitWas ? "OPEN" : "closed")} at step {sim.simStep}, floor {sim.Loom31PitFloorY:0.00}, alive {l.Alive}, break {sim.loom31.breakState}, flow {sim.flow}"); }
            }
            return (minY, sim.loom31.lavaHits - t0, sim.loom31.pitOpen, hp0 - sim.health.hp);
        }
        var mid = Run(true);
        bool okMid = mid.open && mid.minY < -1.0 && mid.touches > 0;
        Console.WriteLine($"  {(okMid ? "PASS" : "FAIL")} middle: pit open {mid.open}, lowest feet {mid.minY:0.00} m from the deck, lava hits {mid.touches} (design.n30.lava numbers; a player who stands still in it dies), health lost {mid.hp:0}");
        if (!okMid) fails.Add("middle");
        var frame = Run(false);
        bool okFrame = frame.open && frame.minY > -0.2;
        Console.WriteLine($"  {(okFrame ? "PASS" : "FAIL")} frame: pit open {frame.open}, lowest feet {frame.minY:0.00} m, lava touches {frame.touches}");
        if (!okFrame) fails.Add("frame");
        // a new run on the same sim: the octree's deck triangles are back
        {
            CampRig31 rig = CampRig31For("loom31", 1);
            SurvivalSim sim = rig.sim;
            for (int i = 0; i < 1200 && !sim.loom30.Hurtable; i++) { sim.health.hp = sim.health.max; sim.Step(); }
            Loom30 l = sim.loom30;
            l.Apply(l.body.hp - l.body.maxHp * (sim.loom31.p.breakAt - 0.01));
            for (int i = 0; i < 2 * 120; i++) { sim.health.hp = sim.health.max; sim.Step(); }
            bool wasOpen = sim.loom31.pitOpen;
            sim.StartRun(2, null, null);
            double[] r = sim.loom31.rect;
            sim.Respawn(new Vec3((r[0] + r[2]) / 2, l.centre.y + 0.05, (r[1] + r[3]) / 2), 0);
            double minY = 1e9;
            for (int i = 0; i < 2 * 120; i++) { sim.health.hp = sim.health.max; sim.Step(); minY = Math.Min(minY, sim.mover.position.y - l.centre.y); }
            bool ok = wasOpen && !sim.loom31.pitOpen && minY > -0.2;
            Console.WriteLine($"  {(ok ? "PASS" : "FAIL")} new run: open before {wasOpen}, after {sim.loom31.pitOpen}, a player in the middle's lowest feet {minY:0.00} m");
            if (!ok) fails.Add("restore");
        }
        SurvivalSim.Wyrm24Dir = null;
        Console.WriteLine($"loompit31 {(fails.Count == 0 ? "PASS" : "FAIL")}");
        return fails.Count == 0 ? 0 : 1;
    }

    /// <summary>`parry31b`: the lead's 12:18 rule on each NIGHT31 boss filter: a 100 hit by W's weapon id parry31 lands 100 (outside every window: the Stag's back shut, the Wyrm
    /// not staggered, the Lantern's shield up and the shooter on a platform, the Loom joint from the deck); the same hit as a KESTREL lands the chip (the RED half).</summary>
    static int Parry31B()
    {
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        var fails = new List<string>();
        void Row(string name, double parry, double plain) { bool ok = Math.Abs(parry - 100) < 1e-9 && plain < 100; Console.WriteLine($"  {(ok ? "PASS" : "FAIL")} {name}: parry31 100 -> {parry:0.0}, revolver 100 -> {plain:0.0}"); if (!ok) fails.Add(name); }
        // the Stag: in its fight, back shut
        SurvivalSim ss = StagSim31(1);
        for (int i = 0; i < 400 && !ss.stag31.Hurtable; i++) ss.Step();
        Stag31 st = ss.stag31;
        Vec3 front = st.box.c + st.facing * 8;
        Row($"stag (act {st.act}, back open {st.BackOpen})", st.Filter(100, Stag31.ParryWeapon, front, out _), st.Filter(100, "revolver", front, out _));
        // the Wyrm's rule, not staggered
        GameConfig cfg = Cfg30("wyrm30");
        var w31 = new Wyrm31(cfg, Wyrm31Params.From(cfg), new EventBus(), new BossTarget { position = () => default });
        Row("wyrm31 (not staggered)", w31.Filter(100, Stag31.ParryWeapon, "head"), w31.Filter(100, "revolver", "head"));
        // the Lantern's rule: shield up, on a platform
        var l31 = new Lantern31(cfg, Lantern31Params.From(cfg), new EventBus(), 2.4);
        Row("lantern31 (shield up, on a platform)", 100 * l31.Scale(100, Stag31.ParryWeapon, false, true, 0.2, 0), 100 * l31.Scale(100, "revolver", false, true, 0.2, 0));
        // the Loom: s8, a joint from the deck
        CampRig31 rig = CampRig31For("loom31", 1);
        if (rig == null) fails.Add("no Loom");
        else
        {
            for (int i = 0; i < 1200 && !rig.sim.loom30.Hurtable; i++) { rig.sim.health.hp = rig.sim.health.max; rig.sim.Step(); }
            Loom30 l = rig.sim.loom30;
            Vec3 eye = new Vec3(l.centre.x, l.centre.y + 1.6, l.centre.z);
            l.Raycast(eye, Vec3.Normalize(l.joints[0] - eye));
            double p = l.Filter(100, Stag31.ParryWeapon, out _);
            l.Raycast(eye, Vec3.Normalize(l.joints[0] - eye));
            double n = l.Filter(100, "nailgun", out _);
            Row($"loom31 (joint 0 from the deck middle, {Vec3.DistanceTo(eye, l.joints[0]):0.0} m)", p, n);
        }
        SurvivalSim.Wyrm24Dir = null;
        Console.WriteLine($"parry31b {(fails.Count == 0 ? "PASS" : "FAIL")}: {4 - fails.Count} of 4 NIGHT31 bosses take a parried projectile in full");
        return fails.Count == 0 ? 0 : 1;
    }

    /// <summary>`loomsight31 [--off]` (G's pick31 note: under the deck the hook took a surface): the Loom begun in s8 (loom31 on, or NIGHT30's with --off), then from each deck
    /// edge's middle (1 m in, eye height) every joint's line of sight through the level collider: clear or the distance it is blocked at. PASS: every hip and knee is clear from
    /// at least one edge.</summary>
    static int LoomSight31(bool off)
    {
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        CampRig31 rig = CampRig31For(off ? "loom" : "loom31", 1);
        SurvivalSim.Wyrm24Dir = null;
        if (rig == null) { Console.WriteLine("loomsight31: no Loom"); return 1; }
        SurvivalSim sim = rig.sim;
        for (int i = 0; i < 3; i++) sim.Step(); // the hooks set, the joints posed
        Loom30 l = sim.loom30;
        var seen = new bool[l.joints.Length];
        for (int e = 0; e < 4; e++)
        {
            Vec3 g0 = l.grips[e], g1 = l.grips[(e + 1) % 4];
            Vec3 mid = new Vec3((g0.x + g1.x) / 2, g0.y, (g0.z + g1.z) / 2);
            Vec3 inward = Vec3.Normalize(new Vec3(l.centre.x - mid.x, 0, l.centre.z - mid.z));
            Vec3 eye = new Vec3(mid.x + inward.x, mid.y + 1.6, mid.z + inward.z);
            var parts = new List<string>();
            for (int j = 0; j < l.joints.Length; j++)
            {
                Vec3 d = l.joints[j] - eye;
                double len = d.Length;
                bool blocked = sim.world.RayIntersect(eye, d / len, out RayHit h) && h.distance < len - 0.3;
                if (!blocked) seen[j] = true;
                parts.Add($"{(j % 2 == 0 ? "knee" : "hip")}{j / 2} {(blocked ? "blocked at " + h.distance.ToString("0.0") : "clear")} ({len:0.0} m)");
            }
            Console.WriteLine($"loomsight31 {(off ? "OFF" : "ON")} edge {e}: {string.Join(", ", parts)}");
        }
        int n = 0;
        foreach (bool s in seen) if (s) n++;
        Console.WriteLine($"loomsight31 {(off ? "OFF" : "ON")} {(n == seen.Length ? "PASS" : "FAIL")}: {n} of {seen.Length} joints in sight from a deck edge (hips at y {l.joints[1].y - l.centre.y:0.0} m from the deck)");
        return n == seen.Length ? 0 : 1;
    }

    /// <summary>THE STAG's rig: the four campers plus the WALL CORNER (DESIGN31 C2's extra camper: the lane against the rim where a straight meets a curve).</summary>
    static CampRig31 CampRig31Stag(string boss, uint seed)
    {
        if (boss != "stag") return null;
        SurvivalSim sim = StagSim31(seed);
        if (sim == null) return null;
        Stag31 st = sim.stag31;
        Rink28Arena a = sim.stag31Rink;
        var rig = new CampRig31 { sim = sim, level = 2 };
        rig.alive = () => st.Alive; rig.hurtable = () => st.Hurtable; rig.hpFrac = () => st.HpFrac; rig.centre = () => st.box.c; rig.ray = st.Raycast;
        double s0 = a.p.playerStartS;
        Vec3 F(double s, double lat) { Vec3 v = a.FloorPoint(s, lat); return new Vec3(v.x, v.y + 0.1, v.z); }
        rig.spots.Add(("centre (lane middle)", F(s0 + 10, (a.latIsland + a.latLaneOut) / 2)));
        rig.spots.Add(("edge (bank top)", F(s0 + 10, a.latBankTop - 0.6)));
        rig.spots.Add(("highest (the island top)", new Vec3(a.p.cx + 0.5, a.islandTopY + 0.1, a.p.cz)));
        rig.spots.Add(("farthest (bank top, half a lap)", F(s0 + 10 + a.Lap / 2, a.latBankTop - 0.6)));
        rig.spots.Add(("wall corner (lip, straight meets curve)", F(a.p.straight / 2, a.latLip - 0.4)));
        rig.note = $"rink lap {a.Lap:0} m, bank top y {a.bankTopY - a.p.cy:0.0} m, island top {a.islandTopY - a.p.cy:0.0} m";
        return rig;
    }


    sealed class StagRow31
    {
        public bool killed;
        public double seconds, hpEnd;
        public int deaths, dodges, punches;
        public string counters = "";
    }

    static StagRow31 StagPilotRun31(uint seed, double capSeconds, bool dbg)
    {
        SurvivalSim sim = StagSim31(seed);
        var row = new StagRow31();
        if (sim == null) return row;
        Stag31 st = sim.stag31;
        double dt = sim.cfg.Dt;
        var si = (ScriptedInput)sim.input;
        var pilot = new StagPilot31();
        int step = framesEmu31 ? 1 : 0, cap = (int)(capSeconds / dt) + step; // --frames starts on an ODD step (the Player reel's bad case)
        InputFrame fm = default;
        CombatInput fc = default;
        while (st.Alive && step < cap)
        {
            if (!framesEmu31)
            {
                pilot.Step(sim, step);
                fm = pilot.Move; fc = pilot.Combat;
            }
            else if (step % 2 == 1)
            {
                // the Player reel's PilotDrive27 at ReelFps 60: two pilot steps on one sim state, the LAST step's held, the presses ORed, applied to both sim steps
                pilot.Step(sim, step);
                InputFrame m0 = pilot.Move; CombatInput c0 = pilot.Combat;
                pilot.Step(sim, step + 1);
                fm = pilot.Move; fc = pilot.Combat;
                fm.jump.pressed |= m0.jump.pressed; fm.dash.pressed |= m0.dash.pressed; fm.sprint.pressed |= m0.sprint.pressed;
                fc.fire.pressed |= c0.fire.pressed;
                if (fc.weapon < 0) fc.weapon = c0.weapon;
            }
            si.SetLook(fm.yaw, fm.pitch);
            si.Set(fm, fc);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (dbg && step % 1200 == 0) Console.WriteLine($"      t={step * dt:0}s {st.phase}/{st.act} hp {st.HpFrac:0.00} php {sim.health.hp:0} charges {st.charges} hits {st.chargeHits} slams {st.slams} misses {st.misses} back {st.backPunches} dist {JsMath.Hypot(st.box.c.x - sim.mover.position.x, st.box.c.z - sim.mover.position.z):0.0}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Stag31Begin(out Vec3 sp, out double yw);
                sim.Respawn(sp, yw);
            }
        }
        row.killed = !st.Alive;
        row.seconds = step * dt;
        row.hpEnd = st.HpFrac;
        row.dodges = pilot.dodges;
        row.punches = pilot.punches;
        row.counters = $"charges {st.charges} (hit the pilot {st.chargeHits}, wall slams {st.slams}, misses {st.misses}, pairs {st.pairs}), stomps {st.stomps} ({st.stompHits} hit), back punches {st.backPunches}, crystals broken {st.crystalBreaks}, shard volleys {st.shardVolleys} ({st.shardHits} hits), water hits {st.lavaHits}, melted {st.melted}";
        return row;
    }

    // --- the camper ------------------------------------------------------------------------------------------------------------------------

    static CampRow31 CampRun31(string boss, uint seed, int spot, bool dbg)
    {
        CampRig31 rig = CampRig31For(boss, seed);
        if (rig == null || spot >= rig.spots.Count) return null;
        SurvivalSim sim = rig.sim;
        double dt = sim.cfg.Dt;
        var (name, at) = rig.spots[spot];
        var row = new CampRow31 { name = name };
        sim.Respawn(at, 0);
        sim.mover.invulnerable = false;
        var si = (ScriptedInput)sim.input;
        double reach = sim.cfg.Num("weapons.gauntlet.range");
        var used = new HashSet<string>();
        var byPart = new Dictionary<string, double>();
        if (dbg) sim.events.On<BossEvent>("bossHit", e => { string k = e.boss + "/" + (e.part ?? "?"); byPart[k] = (byPart.TryGetValue(k, out double v0) ? v0 : 0) + e.damage; });
        if (dbg) sim.events.On<HitEvent>("hit", e => { string k = "hit " + e.weapon; byPart[k] = (byPart.TryGetValue(k, out double v0) ? v0 : 0) + e.damage; });
        int clock = -1, step = 0, cap = 30 * 120;
        double hpFrac0 = 0, prevHp = sim.health.hp, maxHp = sim.health.max, distSum = 0;
        int distN = 0;
        // before the boss is hurtable: hold the spot, full health (the intro is not the test)
        while (step < 60 * 120 && rig.alive())
        {
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = rig.centre() - eye;
            double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            if (clock < 0 && rig.hurtable()) { clock = 0; hpFrac0 = rig.hpFrac(); sim.health.hp = sim.health.max; prevHp = sim.health.hp; }
            var mv = new InputFrame { yaw = wy, pitch = wp };
            double cx = at.x - me.x, cz = at.z - me.z, cl = JsMath.Hypot(cx, cz);
            if (cl > 1.0)
            {
                Vec3 fwd = MovementMath.WishDirection(0, 1, wy), right = MovementMath.WishDirection(1, 0, wy);
                mv.moveZ = (cx * fwd.x + cz * fwd.z) / cl;
                mv.moveX = (cx * right.x + cz * right.z) / cl;
            }
            double dist = d.Length;
            Vec3 dn = Vec3.Normalize(d);
            double tHit = rig.ray(eye, dn);
            bool see = tHit >= 0 && !(sim.world.RayIntersect(eye, dn, out RayHit wh) && wh.distance < tHit);
            string gun = "revolver";
            if (rig.level >= 4) gun = "nailgun";
            if (rig.level >= 3 && dist < 12) gun = "shotgun";
            if (rig.level >= 2 && tHit >= 0 && tHit <= reach) gun = "gauntlet";
            if (!sim.loadout.owned.Contains(gun)) gun = "revolver";
            int gi = sim.loadout.owned.IndexOf(gun);
            bool fire = clock >= 0 && see;
            si.SetLook(wy, wp);
            si.Set(mv, new CombatInput { weapon = sim.loadout.Weapon != gun ? gi : -1, fire = new Skygrind.Core.Button { held = fire, pressed = fire && step % 2 == 0 } });
            sim.Step();
            step++;
            sim.hitstopLeft = 0;
            if (clock < 0) { sim.health.hp = sim.health.max; prevHp = sim.health.hp; continue; }
            if (fire) { row.shots++; used.Add(gun); }
            distSum += dist; distN++;
            row.lost += Math.Max(0, prevHp - sim.health.hp);
            prevHp = sim.health.hp;
            clock++;
            if (dbg && clock % 600 == 0) Console.WriteLine($"      t={clock * dt:0}s hp {sim.health.hp:0} boss {rig.hpFrac():0.000} at ({me.x:0.0},{me.y:0.0},{me.z:0.0}) see {see} gun {gun}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead) { row.died = true; break; }
            if (clock >= cap) break;
        }
        row.alive = Math.Max(0, clock) * dt;
        row.rate = row.alive > 0 ? (hpFrac0 - rig.hpFrac()) / row.alive : 0;
        row.lost = row.lost / maxHp;
        row.dist = distN > 0 ? distSum / distN : 0;
        row.weapons = string.Join("+", used);
        if (clock < 0) row.name += " (the boss never became hurtable in 60 s)";
        if (dbg && sim.wyrm31 != null) Console.WriteLine($"      wyrm31: full hits {sim.wyrm31.fullHits}, 20 % hits {sim.wyrm31.gunHits}, spits {sim.wyrm31.spitVolleys} ({sim.wyrm31.spitHits} hit), sweeps {sim.wyrm31.sweeps}, staggers {sim.wyrm30Staggers}, worm stats head {sim.wyrm24.stats["headHits"]} core {sim.wyrm24.stats["coreHits"]}");
        if (dbg) foreach (var kv in byPart) Console.WriteLine($"      damage {kv.Key}: {kv.Value:0}");
        if (dbg && sim.loom30 != null) Console.WriteLine($"      loom: full hits {sim.loom30.fullHits}, light hits {sim.loom30.lightHits}, max hp {sim.loom30.body.maxHp}, loom31 {(sim.loom31 != null ? $"shakes {sim.loom31.shakeTells}, drops {sim.loom31.drops} ({sim.loom31.dropHits} hit), break {sim.loom31.breakState}" : "off")}");
        return row;
    }

    // --- the pilots (NIGHT30's boss30, one seed) -------------------------------------------------------------------------------------------

    static double campPilotRate31;
    static string campLantern31Line = "", campLoom31Line = "";

    static double CampPilot31(string boss, uint seed, bool dbg)
    {
        double dt = 1.0 / 120;
        if (boss == "lantern") { LanternRow30 r = LanternRun30(Cfg30("lantern30"), seed, 240 * 120, 0, true, dbg); return r.killed ? r.steps * dt : -r.steps * dt; }
        if (boss == "lantern31")
        {
            LanternRow30 r = LanternRun30(Cfg30("lantern30"), seed, 240 * 120, 0, true, dbg, new N31Blocks { lantern31 = true, charge = true });
            Console.WriteLine($"  lantern31 pilot seed {seed}: {(r.killed ? "KILLED at " + (r.steps * dt).ToString("0.0") + " s" : "NOT killed in " + (r.steps * dt).ToString("0") + " s (bar " + r.hpEnd.ToString("0.00") + ")")}; deaths {r.deaths}, rides {r.rides}, platform hits {r.platformHits}, lobs {r.lobs}; {campLantern31Line}");
            return r.killed ? r.steps * dt : -r.steps * dt;
        }
        if (boss == "glacier") { GlacierRow30 r = GlacierRun30(Cfg30("glacier30", "glide"), seed, 240 * 120, 0, true, dbg, true); return r.killed ? r.steps * dt : -r.steps * dt; }
        if (boss == "wyrm")
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg30("wyrm30"), Map());
            sim.noLab = sim.noRecords = true;
            sim.n24Override = new N24Blocks { wyrm = true };
            sim.n26Override = new N26Blocks { level3 = true };
            sim.n29Override = new N29Blocks { boss1 = true };
            sim.n30Override = new N30Blocks { wyrm30 = true };
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(seed + 2, null, null);
            sim.husks.frozen = true;
            if (!sim.Wyrm24Begin()) return 0;
            sim.Respawn(sim.Wyrm24Spawn, 0);
            var rig = new WyrmRig(sim);
            var bot = new WyrmBot(true);
            while (sim.wyrm24.Alive && rig.step < 12 * 60 * 120) { InputFrame f = bot.Next(rig, out bool fire); rig.Step(f, fire); sim.hitstopLeft = 0; }
            return !sim.wyrm24.Alive ? rig.step * dt : -rig.step * dt;
        }
        if (boss == "loom")
        {
            GameConfig cfg = Cfg30("span");
            var plan = PortData.Obj(PortData.LoadJson(Path.GetFullPath(Path.Combine(Root, "..", "Design", "levels", "n30", "span.plan.json"))));
            Vec3[] legs = null;
            foreach (object ro in PortData.Arr(plan["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (r.ContainsKey("loomLegs")) { var a = PortData.Arr(r["loomLegs"]); legs = new Vec3[4]; for (int i = 0; i < 4; i++) legs[i] = PortData.V3(a[i]); }
            }
            Vec3 deck = legs != null ? new Vec3((legs[0].x + legs[2].x) * 0.5, legs[0].y, (legs[0].z + legs[2].z) * 0.5) : new Vec3(0, 0, -218);
            LoomBotRow lr = LoomBotRun(cfg, legs, deck, seed);
            return lr.killed ? lr.seconds : -lr.seconds;
        }
        if (boss == "loom31")
        {
            GameConfig cfg = Cfg30("span");
            var plan = PortData.Obj(PortData.LoadJson(Path.GetFullPath(Path.Combine(Root, "..", "Design", "levels", "n30", "span.plan.json"))));
            Vec3[] legs = null;
            foreach (object ro in PortData.Arr(plan["rooms"]))
            {
                var r = PortData.Obj(ro);
                if (r.ContainsKey("loomLegs")) { var a = PortData.Arr(r["loomLegs"]); legs = new Vec3[4]; for (int i = 0; i < 4; i++) legs[i] = PortData.V3(a[i]); }
            }
            Vec3 deck = legs != null ? new Vec3((legs[0].x + legs[2].x) * 0.5, legs[0].y, (legs[0].z + legs[2].z) * 0.5) : new Vec3(0, 0, -218);
            LoomBotRow lr = LoomBotRun(cfg, legs, deck, seed, true);
            Console.WriteLine($"  loom31 lag bot seed {seed}: {(lr.killed ? "KILLED at " + lr.seconds.ToString("0.0") + " s" : "NOT killed in " + lr.seconds.ToString("0") + " s (hp " + lr.hpEnd.ToString("0.00") + ")")}; hangs {lr.hangs}, cut tells {lr.tells} (let go {lr.dodged}, cut {lr.cuts}), nails {lr.nails} (full {lr.full}, light {lr.light}), hanging {lr.hangShare * 100:0} %; {campLoom31Line}");
            return lr.killed ? lr.seconds : -lr.seconds;
        }
        if (boss == "wyrm31")
        {
            // the PUNCH-AND-SLUG pilot (mortal, respawns): NIGHT30's puncher (walks up inside the aggro radius, faces the head, swings the PULSE when the charging head's box is in
            // reach), the THUNDERCLAP at the head while the worm is staggered, the KESTREL otherwise; it strafes so it is never still for the spit
            SurvivalSim sim = SurvivalSim.Headless(Cfg30("wyrm30"), Map());
            sim.noLab = sim.noRecords = true;
            sim.n24Override = new N24Blocks { wyrm = true };
            sim.n26Override = new N26Blocks { level3 = true };
            sim.n29Override = new N29Blocks { boss1 = true };
            sim.n30Override = new N30Blocks { wyrm30 = true };
            sim.n31Override = new N31Blocks { wyrm31 = true };
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(seed + 2, null, null);
            sim.husks.frozen = true;
            if (!sim.Wyrm24Begin()) return 0;
            sim.Respawn(sim.Wyrm24Spawn, 0);
            Wyrm w = sim.wyrm24;
            foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
            var si = (ScriptedInput)sim.input;
            double pilotDealt = 0;
            sim.events.On<BossEvent>("bossHit", e => { if (e.boss == "wyrm") pilotDealt += e.damage; });
            string crumbleSeen = "";
            sim.events.On<Wyrm31Event>("wyrm31", e => { if (e.stage.StartsWith("crumble")) crumbleSeen += e.stage + "@" + (sim.wyrm24.stepNo * dt).ToString("0") + "s "; });
            var wpilot = new WyrmPilot31(); // Game/Bot/WyrmPilot31.cs: the Player reel wyrm31 drives the same pilot
            int step = 0, deaths = 0, cap = 8 * 60 * 120;
            while (w.Alive && step < cap)
            {
                wpilot.Step(sim, step);
                si.SetLook(wpilot.Move.yaw, wpilot.Move.pitch);
                si.Set(wpilot.Move, wpilot.Combat);
                sim.Step();
                sim.hitstopLeft = 0;
                step++;
                if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
                {
                    deaths++;
                    sim.downSteps = 0;
                    sim.flow = Skygrind.Game.Flow.Playing;
                    sim.health.Reset();
                    sim.Wyrm24Begin();
                    sim.Respawn(sim.Wyrm24Spawn, 0);
                }
            }
            Wyrm31 r = sim.wyrm31;
            campPilotRate31 = pilotDealt / Math.Max(1, w.body.maxHp) / Math.Max(1e-6, step * dt);
            Console.WriteLine($"  wyrm31 pilot damage dealt {pilotDealt:0} = {pilotDealt / Math.Max(1, w.body.maxHp) * 100:0} % of the bar in {step * dt:0} s = {campPilotRate31 * 100:0.00} %/s (its deaths put the bar back to the phase start: the rate is damage dealt, not the kill time)");
            Console.WriteLine($"  wyrm31 pilot seed {seed + 2}: {(!w.Alive ? "KILLED at " + (step * dt).ToString("0.0") + " s" : "NOT killed in " + (step * dt).ToString("0") + " s (bar " + w.HpFrac.ToString("0.00") + ")")}; deaths {deaths}, staggers {sim.wyrm30Staggers}, feints {r?.feints}, sweeps {r?.sweeps} ({r?.sweepHits} hit), spit volleys {r?.spitVolleys} ({r?.spitHits} hits), full hits {r?.fullHits}, 20 % hits {r?.gunHits}, crumble events {crumbleSeen} (boxes {string.Join(",", sim.wyrm31CrumbleBoxes)})");
            return !w.Alive ? step * dt : -step * dt;
        }
        if (boss == "stag")
        {
            StagRow31 r = StagPilotRun31(seed, 240, dbg);
            Console.WriteLine($"  stag pilot seed {seed}: {(r.killed ? "KILLED at " + r.seconds.ToString("0.0") + " s" : "NOT killed in " + r.seconds.ToString("0") + " s (bar " + r.hpEnd.ToString("0.00") + ")")}; deaths {r.deaths}, dodges {r.dodges}, punches {r.punches}; {r.counters}");
            return r.killed ? r.seconds : -r.seconds;
        }
        return 0;
    }

    // --- the table -------------------------------------------------------------------------------------------------------------------------

    /// <summary>The camp table lines for one boss (also printed by the reel's camp31 scenario through the same rule).</summary>
    static int Camp31Boss(string boss, uint seed, double pilotOver, bool dbg)
    {
        CampRig31 probe = CampRig31For(boss, seed);
        if (probe == null) { Console.WriteLine($"camp31 {boss}: NOT BUILT (no fight could be begun)"); return boss == "stag" ? 0 : 1; }
        double pilot = pilotOver > 0 ? pilotOver : CampPilot31(boss, seed, dbg);
        bool pilotKilled = pilot > 0;
        pilot = Math.Abs(pilot);
        double pilotRate = campPilotRate31 > 0 ? campPilotRate31 : pilot > 0 ? 1.0 / pilot : 0; // the bar per second (a kill drains 1.0; a pilot whose deaths reset the bar: its damage dealt per second)
        campPilotRate31 = 0;
        Console.WriteLine($"camp31 {boss} (level {probe.level}; {probe.note}): pilot {(pilotKilled ? "KILLED in " + pilot.ToString("0.0") + " s" : "NOT killed in " + pilot.ToString("0") + " s")} = {pilotRate * 100:0.00} % of the bar / s; a third = {pilotRate / 3 * 100:0.00} % / s");
        Console.WriteLine($"  {"camper",-52} {"alive",7} {"hp lost",8} {"dmg %/s",8} {"dist",6} {"weapons",-20} verdict");
        int camps = 0;
        for (int i = 0; i < probe.spots.Count; i++)
        {
            CampRow31 r = CampRun31(boss, seed, i, dbg);
            if (r == null) { Console.WriteLine($"  spot {i}: rig failed"); camps++; continue; }
            bool hurt = r.died || r.lost >= 0.5;
            bool slow = r.rate < pilotRate / 3;
            r.camps = !(hurt && slow);
            if (r.camps) camps++;
            string why = r.camps ? (hurt ? "CAMPS (dmg rate too high)" : slow ? "CAMPS (safe: not hurt enough)" : "CAMPS (safe and fast)") : "LOSES (boss wins)";
            Console.WriteLine($"  {r.name,-52} {(r.died ? "DIED " : "") + r.alive.ToString("0.0") + " s",7} {r.lost * 100,7:0} % {r.rate * 100,8:0.00} {r.dist,5:0} m {r.weapons,-20} {why}");
        }
        Console.WriteLine($"camp31 {boss} {(camps == 0 ? "PASS" : "RED")}: {probe.spots.Count - camps} of {probe.spots.Count} campers lose (PASS = every camper dies or loses 50 % in 30 s AND deals under a third of the pilot's rate)");
        return camps == 0 ? 0 : 1;
    }
}
