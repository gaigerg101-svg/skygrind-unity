// NIGHT31 builder W (weapons): the W rows' headless judges. ONE entry method, N31Weapons(cmd, args, ref code) (declared and called in Program.cs).
//   unlocks31 [--off]   item 3 (design.n31.unlocks): each NIGHT30 level (n30-foundry, n30-iceline, n30-kiln, n30-span) as a CAMPAIGN run: the
//                       loadout at room 1 (after the game's render-side CampaignUnlocks26.Apply work, done here before the first step as the
//                       harness's level runs do), then after the level's award (level 2: on entering i1a; levels 3-4: the player put on the plan's
//                       weapon pickup). GREEN (on): L1 [revolver]; L2 [revolver] then + gauntlet; L3 [revolver, gauntlet] then + shotgun;
//                       L4 [revolver, gauntlet, shotgun] then + nailgun; each weapon earned once. `--off`: NIGHT30's start (KESTREL + gauntlet): RED.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N31Weapons(string cmd, string[] args, ref int code)
    {
        if (cmd == "unlocks31") Unlocks31Command(args, ref code);
        else if (cmd == "charge31") Charge31Command(args, ref code);
        else if (cmd == "ltalways31") LtAlways31Command(args, ref code);
        else if (cmd == "boss31w") Boss31WCommand(args, ref code);
        else if (cmd == "autoparry31") AutoParry31Command(args, ref code);
        else if (cmd == "rows31") Rows31Command(args, ref code);
    }

    // ---- rows31 ----------------------------------------------------------------------------------------------------------------------------
    //   rows31 [--off]   round 2 (4): builder L's level rows under design.n31.unlocks: each NIGHT30 level as a CAMPAIGN run; after the start, every room with a
    //                    lessons31 / standIns31 / waves31 row uses it (f9's first line is no longer "FIVE KILLS ..."; f8 3 stand-ins; k1's last wave "heavy");
    //                    iceline's pickup31 (i1a-P31) is a pickup, and walking onto it takes it with ONE gauntlet weaponEarned. `--off`: the NIGHT30 rows.

    static void Rows31Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        var fails = new List<string>();
        foreach (string lvName in new[] { "foundry", "iceline", "kiln", "span" })
        {
            string path = $"Design/levels/n30-{lvName}.json";
            SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
            SurvivalSim sim = CampaignSim30(cfg, map, N30Blocks.From(cfg));
            sim.n29Override = new N29Blocks { pickups = true, roomRules = true, doorFree = true };
            sim.n31Override = new N31Blocks { unlocks = !off, charge = !off };
            var earned = new List<string>();
            int taken = 0;
            sim.events.On<WeaponEarnedEvent>("weaponEarned", e => earned.Add(e.weapon));
            sim.events.On<PickupEvent29>("pickup29", e => { if (e.id != null && e.id.EndsWith("P31")) taken++; });
            sim.StartRun(3106, null, null);
            for (int s = 0; s < 3; s++) { sim.health.hp = sim.health.max; sim.Step(); }
            LevelDef lv = sim.campaignLevel;
            var notes = new List<string>();
            foreach (RoomDef r in lv.rooms)
            {
                if (r.lessons31 == null && r.standIns31 == null && r.waves31 == null) continue;
                string first = r.lessons.Count > 0 ? r.lessons[0].prompt : "-";
                string lastWave = r.waves.Count > 0 ? string.Join("+", r.waves[r.waves.Count - 1]) : "-";
                notes.Add($"{r.id} '{r.name}': first line '{first}', stand-ins {r.standIns.Count}, last wave {lastWave}");
                if (!off)
                {
                    if (r.lessons31 != null && (r.lessons.Count != r.lessons31.Count || r.lessons[0].prompt != r.lessons31[0].prompt)) fails.Add($"{r.id} lessons");
                    if (r.standIns31 != null && r.standIns.Count != r.standIns31.Count) fails.Add($"{r.id} standIns");
                    if (r.waves31 != null && r.waves.Count != r.waves31.Count) fails.Add($"{r.id} waves");
                }
            }
            string pk = "";
            PickupDef p31 = lv.pickups.Find(p => p.id != null && p.id.EndsWith("P31"));
            if (lv.pickups31.Count > 0)
            {
                if (p31 != null)
                {
                    sim.Respawn(p31.at, 0);
                    for (int s = 0; s < 60 && taken == 0; s++) { sim.health.hp = sim.health.max; sim.Step(); }
                }
                int g = earned.FindAll(w => w == "gauntlet").Count;
                pk = $"; pickup31 {lv.pickups31[0].id} in the pickups {p31 != null}, taken {taken}, gauntlet weaponEarned {g}, owned [{string.Join(", ", sim.loadout.owned)}]";
                if (!off && (p31 == null || taken != 1 || g != 1)) fails.Add($"{lvName} pickup31");
            }
            string pierce = "";
            RoomDef f8 = lv.rooms.Find(r => r.standIns31 != null);
            if (f8 != null && f8.standIns31.Count > 0)
            {
                // into f8 (DOORFREE), stand on the floor 6 m behind the line's first stand-in, look along it, charge the KESTREL and release
                sim.campaign?.SetDoorFree29(true);
                sim.Respawn(f8.spawn, f8.spawnYawDeg);
                int ri = lv.rooms.IndexOf(f8);
                for (int s = 0; s < 240 && sim.campaign.room != ri; s++) { sim.doorFreeHeld = true; sim.health.hp = sim.health.max; sim.Step(); }
                sim.doorFreeHeld = false;
                for (int s = 0; s < 30; s++) { sim.health.hp = sim.health.max; sim.Step(); }
                Vec3 a = f8.standIns31[0].at, b = f8.standIns31[f8.standIns31.Count - 1].at;
                Vec3 dirL = Vec3.Normalize(b - a);
                Vec3 stand = new Vec3(a.x - dirL.x * 6, a.y, a.z - dirL.z * 6);
                sim.Respawn(stand, 0);
                var bodies = new List<Enemy>();
                foreach (Enemy e in sim.enemies.all) if (e.alive && e.standIn29) bodies.Add(e);
                List<int> ids0 = bodies.ConvertAll(e => e.id);
                if (Flag(args, "--dbg")) foreach (Enemy e in bodies) Console.WriteLine($"    before the shot: stand-in {e.id} at ({e.position.x:0.00}, {e.position.y:0.00}, {e.position.z:0.00}); live enemies {sim.enemies.AliveCount}");
                int slot = sim.loadout.owned.IndexOf("revolver");
                ChargeShots27 c = sim.loadout.charge27;
                int hold = c != null ? MovementMath.StepsFor(c.p.holdSeconds, sim.step) + MovementMath.StepsFor(c.p.fullSeconds[0], sim.step) + 6 : 120;
                for (int s = 0; s < hold + 40; s++)
                {
                    Vec3 mid = new Vec3((a.x + b.x) / 2, a.y + 1.2, (a.z + b.z) / 2);
                    var (yaw, pitch) = ArclineLook(sim.Eye, mid);
                    bool down = s >= 20 && s < 20 + hold;
                    sim.health.hp = sim.health.max;
                    Tick25(sim, yaw, pitch, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = s == 20 } });
                }
                // a killed stand-in's husk body goes back to the pool and the room's next wave can reuse it at once (new id, full hp): judge by the id taken before the shot
                int dead = 0;
                for (int k = 0; k < bodies.Count; k++) if (!bodies[k].alive || bodies[k].id != ids0[k] || bodies[k].hp < bodies[k].maxHp) dead++;
                if (Flag(args, "--dbg"))
                    foreach (Enemy e in bodies) Console.WriteLine($"    stand-in at ({e.position.x:0.00}, {e.position.y:0.00}, {e.position.z:0.00}) alive {e.alive} hp {e.hp:0.0}/{e.maxHp:0.0}; eye ({sim.Eye.x:0.00}, {sim.Eye.y:0.00}, {sim.Eye.z:0.00}); charged hits {(c != null ? c.firedHits[0] : -1)}");
                pierce = $"; f8 pierce: {bodies.Count} stand-ins up (roomRules), ONE charged KESTREL shot from the floor hurt or killed {dead} (charged hits {(c != null ? c.firedHits[0] : -1)}), room {lv.rooms[Math.Max(0, sim.campaign.room)].id}, charge on {c != null}";
                if (!off && (bodies.Count == 0 || dead < bodies.Count)) fails.Add("f8 pierce");
            }
            Console.WriteLine($"  n30-{lvName}: {string.Join(" | ", notes)}{pk}{pierce}");
        }
        Console.WriteLine(off ? "rows31 RED side (n31.unlocks off: the NIGHT30 rows)" : fails.Count == 0 ? "rows31 GREEN" : "rows31 RED: " + string.Join("; ", fails));
        code = !off && fails.Count == 0 ? 0 : 1;
    }

    // ---- autoparry31 -----------------------------------------------------------------------------------------------------------------------
    //   autoparry31 [--off] [--seconds S]   round 2 (3): the AUTOPILOT's punch parry through AltUse.ParryWanted(Loadout, ...), design.n31.ltAlways on: a SURVIVAL
    //                     autopilot run (seed 4242, normal) counts its punches, the punches while a body in parry range winds up, and the punches pressed with the
    //                     gauntlet in hand (want 0, R8). `--off`: ltAlways off: the autopilot never punches (NIGHT30's).

    static void AutoParry31Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        double seconds = 150;
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--seconds") seconds = double.Parse(args[i + 1], System.Globalization.CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n31Override = new N31Blocks { ltAlways = !off, charge = !off };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        var ap = new AutopilotInput(sim, "normal", 4242);
        sim.UseInput(ap);
        sim.StartRun(4242, null, null);
        int punches = 0, withGauntlet = 0, onWindup = 0;
        AltUseParams p = sim.loadout.AltParams;
        sim.events.On<MeleeEvent>("punch", e =>
        {
            punches++;
            if (sim.loadout.Weapon == "gauntlet") withGauntlet++;
            if (AltUse.ParryWanted(sim.loadout.Enemies, sim.mover.position, p)) onWindup++;
        });
        int max = MovementMath.StepsFor(seconds, sim.step), s = 0;
        for (; s < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); s++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.health.hp = sim.health.max;
            sim.Step();
        }
        ap.Detach();
        Console.WriteLine($"autoparry31 {(off ? "OFF" : "ON")}: {s * sim.step:0.0} s, round {sim.run.round}, kills {sim.run.stats.kills}; punches {punches} (a body winding up in parry range at {onWindup}), with the gauntlet in hand {withGauntlet}");
        bool ok = off ? punches == 0 : punches > 0 && withGauntlet == 0;
        Console.WriteLine(off ? (punches == 0 ? "autoparry31 RED side: no punch (NIGHT30's autopilot)" : "autoparry31 OFF punched") : ok ? "autoparry31 GREEN" : "autoparry31 RED");
        code = ok ? 0 : 1;
    }

    // ---- boss31w ---------------------------------------------------------------------------------------------------------------------------
    //   boss31w [--off]   round 2: the THUNDERCLAP's charged SLUG on a BOSS (the Lantern's body, NIGHT27's boss1 rig) and on a husk: the boss takes the hit with
    //                     no knock (R7: knockSteps stays 0), the husk is knocked; then the gauntlet's LT PARRY with a Lantern orb 1 m in front: the orb is gone and
    //                     the Lantern took a gauntlet hit (orb damage x orb.reflectDamageMult). `--off`: charge and ltAlways off (NIGHT27 volley, no parry): RED.

    /// <summary>The gauntlet in hand, a boss projectile put 1 m in front of the eye (toward the boss) by `add` (returns "gone?"), one LT press: the line and ok.</summary>
    static string ParryCase31(SurvivalSim sim, GameConfig cfg, Func<Vec3> bossAim, Func<double> bossHp, Func<Vec3, Vec3, Func<bool>> add, double projDamage, out bool ok)
    {
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        for (int k = 0; k < MovementMath.StepsFor(1.5, sim.step) && !(sim.loadout.Weapon == "gauntlet" && sim.loadout.ready); k++)
        {
            var (y0, p0) = ArclineLook(sim.Eye, bossAim());
            Tick27(sim, y0, p0, new CombatInput { weapon = k == 0 ? slot : -1 });
        }
        for (int k = 0; k < MovementMath.StepsFor(1.2, sim.step); k++) { var (y1, p1) = ArclineLook(sim.Eye, bossAim()); Tick27(sim, y1, p1, new CombatInput { weapon = -1 }); }
        var (yy, pp) = ArclineLook(sim.Eye, bossAim());
        Vec3 eye = sim.Eye, aim = Vec3.Normalize(bossAim() - eye);
        Func<bool> gone = add(eye + aim * 1.0, aim * -0.5);
        double hp0 = bossHp(), back0 = sim.bossParryDamage31;
        int ret0 = sim.loadout.lt31?.bossReturns ?? 0;
        Tick27(sim, yy, pp, new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = true } });
        for (int k = 0; k < 3; k++) Tick27(sim, yy, pp, new CombatInput { weapon = -1 });
        int ret = (sim.loadout.lt31?.bossReturns ?? 0) - ret0;
        double mult = cfg.Has("orb.reflectDamageMult") ? cfg.Num("orb.reflectDamageMult") : 2;
        ok = ret == 1 && gone() && bossHp() < hp0; // round 4 (the lead 12:18): a return always hurts its boss (weapon parry31)
        return $"returned {ret}, gone {gone()}, sent back {sim.bossParryDamage31 - back0:0.0} damage (projectile {projDamage} x {mult}), boss hp {hp0:0.0} -> {bossHp():0.0}, weapon {sim.loadout.Weapon}";
    }

    static void Boss31WCommand(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        double dt = cfg.Dt;
        int Steps(double s) => MovementMath.StepsFor(s, dt);
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        var fails = new List<string>();
        try
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n27Override = new N27Blocks { boss1 = true, chargeShots27 = off };
            sim.n26Override = N26Blocks.Off;
            sim.n31Override = new N31Blocks { charge = !off, ltAlways = !off };
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3105, null, null);
            sim.husks.frozen = true;
            sim.mover.invulnerable = true;
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg)) { Console.WriteLine("boss31w: the Lantern did not start (boss1 row / arena file)"); code = 1; return; }
            sim.Respawn(spawn, yawDeg);
            Lantern27 l = sim.lantern27;
            foreach (string w in new[] { "shotgun", "gauntlet" }) if (!sim.loadout.owned.Contains(w)) sim.loadout.AddWeapon(w);
            (double, double) LookAt(Vec3 p) => ArclineLook(sim.Eye, p);
            void Equip(string w)
            {
                int slot = sim.loadout.owned.IndexOf(w);
                for (int k = 0; k < Steps(1.5) && !(sim.loadout.Weapon == w && sim.loadout.ready); k++)
                {
                    var (y, p) = LookAt(l.body.AimPoint);
                    Tick27(sim, y, p, new CombatInput { weapon = k == 0 ? slot : -1 });
                }
            }
            // let the intro run out (the body hurtable)
            for (int k = 0; k < Steps(8) && !l.Hurtable; k++) { var (y, p) = LookAt(l.body.AimPoint); Tick27(sim, y, p, new CombatInput { weapon = -1 }); }
            Equip("shotgun");
            // 1. the slug on the boss
            double hp0 = l.body.hp;
            int bossHits0 = sim.loadout.slugBossHits31, knockMax = 0;
            ChargeShots27 c = sim.loadout.charge27;
            double holdS = c != null ? c.p.holdSeconds : 0.4, fullS = c != null ? c.p.fullSeconds[1] : 0.7;
            int hold = Steps(holdS) + Steps(fullS) + Steps(0.05);
            int fired0 = c != null ? c.fired[1] : 0;
            for (int k = 0; k < hold + Steps(0.3); k++)
            {
                var (y, p) = LookAt(l.body.AimPoint);
                bool down = k < hold;
                Tick27(sim, y, p, new CombatInput { weapon = -1, fire = new Button { held = down, pressed = k == 0 } });
                knockMax = Math.Max(knockMax, l.body.knockSteps);
            }
            int charged = c != null ? c.fired[1] - fired0 : 0, bossHits = sim.loadout.slugBossHits31 - bossHits0;
            Console.WriteLine($"  slug on THE LANTERN (hurtable {l.Hurtable}): charged shots {charged}, slug hits on the boss {bossHits}, boss hp {hp0:0.0} -> {l.body.hp:0.0}, the boss's knockSteps max {knockMax} (want 0)");
            if (!off) { if (charged != 1 || bossHits != 1) fails.Add("the slug did not hit the boss"); if (knockMax != 0) fails.Add("the slug knocked the boss"); }
            // 2. the gauntlet's parry with a Lantern orb 1 m in front
            Equip("gauntlet");
            for (int k = 0; k < Steps(1.2); k++) { var (y, p) = LookAt(l.body.AimPoint); Tick27(sim, y, p, new CombatInput { weapon = -1 }); }
            var (yy, pp) = LookAt(l.body.AimPoint);
            Vec3 eye = sim.Eye, aim = Vec3.Normalize(l.body.AimPoint - eye);
            var orb = new Lantern27Orb { pos = eye + aim * 1.0, prev = eye + aim * 1.0, vel = aim * -0.5, radius = l.p.orbRadius, life = 600, total = 600, kindId = 0 };
            l.orbs.Add(orb);
            double hpP = l.body.hp, back0 = sim.bossParryDamage31;
            int ret0 = sim.loadout.lt31?.bossReturns ?? 0;
            Tick27(sim, yy, pp, new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = true } });
            for (int k = 0; k < 3; k++) Tick27(sim, yy, pp, new CombatInput { weapon = -1 });
            bool gone = !l.orbs.Contains(orb);
            int ret = (sim.loadout.lt31?.bossReturns ?? 0) - ret0;
            Console.WriteLine($"  parry: a Lantern orb 1 m in front, LT pressed with the gauntlet: returned {ret}, the orb gone {gone}, sent back {sim.bossParryDamage31 - back0:0.0} damage (orb {l.p.orbDamage} x {(cfg.Has("orb.reflectDamageMult") ? cfg.Num("orb.reflectDamageMult") : 2)}), boss hp {hpP:0.0} -> {l.body.hp:0.0}");
            if (!off && (ret != 1 || !gone)) fails.Add("the parry did not return the orb");
            if (!off && !(l.body.hp < hpP)) fails.Add("the Lantern lost no hp to the return");
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
        // round 4: the parry on THE STAG's shard and THE KILN WYRM's spit (the rigs of Harness.N31.Boss.cs, with ltAlways on too)
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        try
        {
            {
                SurvivalSim sim = SurvivalSim.Headless(Cfg30("glacier30", "glide"), Map());
                sim.noLab = sim.noRecords = true;
                sim.n27Override = new N27Blocks { pulse27 = true };
                sim.n29Override = new N29Blocks { boss2 = true };
                sim.n30Override = new N30Blocks { glide = true, lava = true };
                sim.n26Override = new N26Blocks { surfaces = true, level2 = true };
                sim.n31Override = new N31Blocks { stag31 = true, ltAlways = !off };
                sim.bosses25Override = true;
                sim.setup.mode = RunSetup.Survival;
                sim.StartRun(3107, null, null);
                sim.husks.frozen = true;
                sim.mover.invulnerable = true;
                if (!sim.Stag31Begin(out Vec3 sp, out double yw)) fails.Add("the Stag did not start");
                else
                {
                    sim.Respawn(sp, yw);
                    if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
                    Stag31 st = sim.stag31;
                    string line = ParryCase31(sim, cfg, () => st.body.AimPoint, () => st.body.hp, (at, v) => { var sh = new Stag31Shard { pos = at, vel = v, life = 600, alive = true }; st.shards.Add(sh); return () => !sh.alive || !st.shards.Contains(sh); }, st.p.shardDamage, out bool okS);
                    Console.WriteLine("  parry: THE STAG's shard 1 m in front: " + line);
                    if (!off && !okS) fails.Add("the Stag's shard: not returned, or the Stag lost no hp (parry31 not taken by its filter yet?)");
                }
            }
            {
                SurvivalSim sim = SurvivalSim.Headless(Cfg30("wyrm30"), Map());
                sim.noLab = sim.noRecords = true;
                sim.n24Override = new N24Blocks { wyrm = true };
                sim.n26Override = new N26Blocks { level3 = true };
                sim.n29Override = new N29Blocks { boss1 = true };
                sim.n30Override = new N30Blocks { wyrm30 = true };
                sim.n31Override = new N31Blocks { wyrm31 = true, ltAlways = !off };
                sim.setup.mode = RunSetup.Survival;
                sim.StartRun(3108, null, null);
                sim.husks.frozen = true;
                sim.mover.invulnerable = true;
                if (!sim.Wyrm24Begin()) fails.Add("the Wyrm did not start");
                else
                {
                    sim.Respawn(sim.Wyrm24Spawn, 0);
                    if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
                    for (int k = 0; k < 5; k++) Tick27(sim, 0, 0, new CombatInput { weapon = -1 }); // wyrm31 is made at its first step
                    if (sim.wyrm31 == null) fails.Add("no wyrm31 object");
                    else
                    {
                        Wyrm31 w = sim.wyrm31;
                        string line = ParryCase31(sim, cfg, () => sim.wyrm24.body.AimPoint, () => sim.wyrm24.body.hp, (at, v) => { var sh = new Stag31Shard { pos = at, vel = v, life = 600, alive = true }; w.spits.Add(sh); return () => !sh.alive || !w.spits.Contains(sh); }, w.p.spitDamage, out bool okW);
                        Console.WriteLine("  parry: THE KILN WYRM's spit 1 m in front: " + line);
                        if (!off && !okW) fails.Add("the Wyrm's spit: not returned, or the Wyrm lost no hp (parry31 not taken by its filter yet?)");
                    }
                }
            }
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
        bool ok = fails.Count == 0;
        Console.WriteLine(off ? $"boss31w RED side (rows off)" : ok ? "boss31w GREEN" : "boss31w RED: " + string.Join("; ", fails));
        code = ok && !off ? 0 : 1;
    }

    // ---- ltalways31 ------------------------------------------------------------------------------------------------------------------------
    //   ltalways31 [--off] [--steps N]   item 2b (design.n31.ltAlways, with design.n31.charge on too): a SURVIVAL run owning the KESTREL, the
    //                       THUNDERCLAP, the NAILGUN and the gauntlet, random input from a Mulberry32 (fire holds, punch presses, LT presses held a
    //                       random time, slot switches, the look on the nearest body) for N steps (10,000). Each LT press is judged on the weapon in hand
    //                       after its step: an EFFECT (zoom on: adsActive / adsStart within the hold; detonate: nails went off; blast; parry window opened)
    //                       or a REASON (an ltDeny31 word on the slot; the gun switching (ready false); a dash / swing suspending the zoom; the
    //                       nailgun's detonate cooldown). NOTHING = a press that did neither. Fire / melee overlap = a gun shot and a punch in one step.
    //                       GREEN: 0 NOTHING, 0 overlaps, every weapon pressed. `--off`: ltAlways and charge off (NIGHT30's LT): RED expected (the gauntlet
    //                       and the NAILGUN presses without nails).

    static void LtAlways31Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        int steps = 10000;
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--steps") steps = int.Parse(args[i + 1]);
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n31Override = new N31Blocks { ltAlways = !off, charge = !off };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3103, null, null);
        sim.mover.invulnerable = true;
        foreach (string w in new[] { "revolver", "shotgun", "nailgun", "gauntlet" }) if (!sim.loadout.owned.Contains(w)) sim.loadout.AddWeapon(w);
        var rng = new Mulberry32(31031);
        string[] names = { "revolver", "shotgun", "nailgun", "gauntlet" };
        var presses = new Dictionary<string, int>();
        var effects = new Dictionary<string, Dictionary<string, int>>();
        foreach (string w in names) { presses[w] = 0; effects[w] = new Dictionary<string, int>(); }
        void Count(string w, string what) { effects[w].TryGetValue(what, out int n); effects[w][what] = n + 1; }
        int s = 0, firedStep = -1, punchedStep = -1, overlaps = 0, adsStartStep = -1, denyStep = -1, parryStep = -1, blastStep = -1;
        string denyWord = null;
        sim.events.On<string>("fire", w => { if (w != "gauntlet") { firedStep = s; if (punchedStep == s) overlaps++; } });
        sim.events.On<MeleeEvent>("punch", e => { punchedStep = s; if (firedStep == s) overlaps++; });
        sim.events.On<string>("adsStart", w => adsStartStep = s);
        sim.events.On<LtAlways31Event>("ltDeny31", e => { denyStep = s; denyWord = e.reason; });
        sim.events.On<LtAlways31Event>("ltParry31Open", e => parryStep = s);
        sim.events.On<object>("pulse27Blast", e => blastStep = s);
        bool fire = false, lt = false, prevFire = false, prevLt = false, prevPunch = false;
        int ltLeft = 0;
        var pending = new List<(int step, string weapon, bool nails, int detonated)>();
        double yaw = 0, pitch = 0;
        for (s = 0; s < steps; s++)
        {
            if (sim.flow != Skygrind.Game.Flow.Playing && sim.flow != Skygrind.Game.Flow.Down) break;
            sim.health.hp = sim.health.max;
            Enemy near = null;
            double nd = double.PositiveInfinity;
            foreach (Enemy e in sim.enemies.all) if (e.alive) { double d = Vec3.DistanceTo(e.position, sim.mover.position); if (d < nd) { nd = d; near = e; } }
            if (near != null) (yaw, pitch) = ArclineLook(sim.Eye, near.AimPoint);
            if (rng.Next() < 0.03) fire = !fire;
            bool punch = rng.Next() < 0.015;
            if (ltLeft > 0) ltLeft--;
            else if (lt) lt = false;
            else if (rng.Next() < 0.02) { lt = true; ltLeft = 1 + (int)(rng.Next() * 90); }
            int slot = rng.Next() < 0.006 ? (int)(rng.Next() * sim.loadout.owned.Count) : -1;
            bool ltPress = lt && !prevLt;
            string before = sim.loadout.Weapon;
            bool nailsBefore = sim.loadout.NailsStuck31;
            int detBefore = sim.loadout.detonated;
            Tick25(sim, yaw, pitch, new CombatInput { weapon = slot, fire = new Button { held = fire, pressed = fire && !prevFire }, punch = new Button { held = punch, pressed = punch && !prevPunch }, altFire = new Button { held = lt, pressed = ltPress } });
            prevFire = fire; prevLt = lt; prevPunch = punch;
            if (ltPress && Array.IndexOf(names, before) >= 0 && sim.loadout.Weapon == before) pending.Add((s, before, nailsBefore, detBefore));
            // judge presses once their hold has had a chance (zoom: up to the end of the hold or 30 steps; the rest: this step)
            for (int k = pending.Count - 1; k >= 0; k--)
            {
                var pr = pending[k];
                string w = pr.weapon;
                bool zoomGun = w == "revolver" || w == "shotgun" || (w == "nailgun" && !pr.nails);
                string verdict = null;
                if (w == "gauntlet")
                {
                    if (blastStep == pr.step) verdict = "effect: BLAST";
                    else if (parryStep == pr.step) verdict = "effect: PARRY";
                    else if (denyStep == pr.step) verdict = "reason: " + denyWord;
                    else verdict = sim.loadout.ready ? "NOTHING" : "reason: switching";
                }
                else if (!zoomGun)
                {
                    if (sim.loadout.detonated > pr.detonated) verdict = "effect: DETONATE";
                    else if (denyStep == pr.step) verdict = "reason: " + denyWord;
                    else verdict = !sim.loadout.ready ? "reason: switching" : "NOTHING (detonate)";
                }
                else if (adsStartStep >= pr.step || sim.loadout.adsActive) verdict = "effect: ZOOM";
                else if (s - pr.step >= 30 || !lt)
                    verdict = !sim.loadout.ready ? "reason: switching" : sim.loadout.ZoomGun(w) <= 1 ? "NOTHING (no zoom)" : "reason: suspended (dash / swing)";
                if (verdict == null) continue;
                presses[w]++;
                Count(w, verdict);
                pending.RemoveAt(k);
            }
        }
        int nothing = 0, pressed = 0;
        foreach (string w in names)
        {
            pressed += presses[w] > 0 ? 1 : 0;
            var parts = new List<string>();
            foreach (var kv in effects[w]) { parts.Add($"{kv.Key} {kv.Value}"); if (kv.Key.StartsWith("NOTHING")) nothing += kv.Value; }
            Console.WriteLine($"  {w}: {presses[w]} LT presses: {string.Join(", ", parts)}");
        }
        LtAlways31 l = sim.loadout.lt31;
        Console.WriteLine($"ltalways31 {(off ? "OFF (NIGHT30's LT)" : "ON")}: {s} steps, round {sim.run.round}; parries opened {l?.parriesOpened ?? 0}, returned {l?.parriesReturned ?? 0}, denies {l?.denies ?? 0}; fire / melee overlaps {overlaps}; presses that did NOTHING {nothing}");
        bool ok = nothing == 0 && overlaps == 0 && pressed == names.Length;
        Console.WriteLine(ok ? "ltalways31 GREEN" : "ltalways31 RED");
        code = ok ? 0 : 1;
    }

    // ---- charge31 --------------------------------------------------------------------------------------------------------------------------
    //   charge31 [--off]   item 2a + P0 a (design.n31.charge). Per gun (KESTREL, THUNDERCLAP), scripted fire / punch buttons at a pinned husk:
    //                      HELD (the smoke's virtual pad / the attract bot: one press, RT held 6 s at husks that die and come back): kills and shots
    //                      after the first 1.5 s; TAP latency; EARLY release (hold + 0.2 s of charge): cancelled, nothing spent; FULL release: one
    //                      charged shot, the n31 cooldown; MELEE press while charging: cancelled, nothing spent. GREEN = design.n31.charge on.
    //                      `--off`: the RED side = NIGHT27's rule (design.n27.chargeShots27 on, no auto release): the held trigger stalls.

    sealed class Charge31Result
    {
        public int fires, kills, firesLate, killsLate, charged, cancels, autoReleased, meleeCancelled, cooldownAtEnd, chargedHits;
        public int firstFire = -1;
    }

    static Charge31Result Charge31Run(GameConfig cfg, SurvivalMap map, string gun, bool n31, Func<int, bool> held, Func<int, bool> punch, int steps, bool realHp, int lateFrom)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n27Override = new N27Blocks { chargeShots27 = !n31 };
        sim.n31Override = new N31Blocks { charge = n31 };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3102, null, null);
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        var r = new Charge31Result();
        int s = 0;
        sim.events.On<string>("fire", w => { if (w == gun) { r.fires++; if (r.firstFire < 0) r.firstFire = s; if (s >= lateFrom) r.firesLate++; } });
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { r.charged++; r.chargedHits += e.hits; });
        sim.events.On<ChargeShot27Event>("chargeShot27Cancel", e => r.cancels++);
        Husk body = null;
        for (s = 0; s < steps; s++)
        {
            double py = 0;
            if (realHp)
            {
                if (body != null && !body.alive) { r.kills++; if (s >= lateFrom) r.killsLate++; body = null; }
                if (body == null) body = sim.husks.Spawn("husk", new Vec3(sp.x + ux * 4, sp.y, sp.z + uz * 4), sp);
                if (body != null) body.position = new Vec3(sp.x + ux * 4, sp.y, sp.z + uz * 4);
            }
            else body = Pin27(sim, body, sp, ux, uz, 4.0, 1e9);
            if (body != null && body.alive) (yaw, py) = ArclineLook(sim.Eye, body.AimPoint);
            bool down = held(s), prev = s > 0 && held(s - 1);
            bool pu = punch(s), puPrev = s > 0 && punch(s - 1);
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = down, pressed = down && !prev }, punch = new Button { held = pu, pressed = pu && !puPrev } });
        }
        ChargeShots27 c = sim.loadout.charge27;
        if (c != null)
        {
            r.autoReleased = c.autoReleased;
            r.meleeCancelled = c.meleeCancelled;
            r.cooldownAtEnd = c.cooldown[ChargeShots27.Index(gun)];
        }
        return r;
    }

    static void Charge31Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        bool n31 = !off;
        string P = n31 ? "design.n31.charge." : "design.n27.chargeShots27.";
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        Console.WriteLine($"charge31: {(n31 ? "design.n31.charge ON" : "RED side: design.n27.chargeShots27's rule (no auto release)")}");
        foreach (string gun in new[] { "revolver", "shotgun" })
        {
            double holdS = cfg.Num(P + "holdSeconds"), fullS = cfg.Num(P + gun + ".fullSeconds"), cdS = cfg.Num(P + gun + ".cooldownSeconds");
            int lead = Steps(0.5), late = lead + Steps(1.5);
            // 1. the smoke's held trigger (P0 a): one press, held 6 s, husks with their real hit points
            var hold = Charge31Run(cfg, map, gun, n31, s => s >= lead && s < lead + Steps(6), s => false, lead + Steps(6), true, late);
            // 2. a tap
            var tap = Charge31Run(cfg, map, gun, n31, s => s == lead, s => false, lead + Steps(1), false, int.MaxValue);
            // 3. early release: the hold + 0.2 s of charge
            int earlyEnd = lead + Steps(holdS) + Steps(0.2);
            var early = Charge31Run(cfg, map, gun, n31, s => s >= lead && s < earlyEnd, s => false, earlyEnd + Steps(0.5), false, int.MaxValue);
            // 4. full release
            int fullEnd = lead + Steps(holdS) + Steps(fullS) + Steps(0.05);
            var full = Charge31Run(cfg, map, gun, n31, s => s >= lead && s < fullEnd, s => false, fullEnd + Steps(0.2), false, int.MaxValue);
            // 5. a melee press while charging (fire still held)
            int mel = lead + Steps(holdS) + Steps(0.2);
            var melee = Charge31Run(cfg, map, gun, n31, s => s >= lead && s < mel + Steps(0.1), s => s >= mel && s < mel + 3, mel + Steps(0.6), false, int.MaxValue);
            Console.WriteLine($"  {gun}: HELD 6 s (one press): {hold.fires} shots, {hold.kills} kills; after 1.5 s: {hold.firesLate} shots, {hold.killsLate} kills; charged {hold.charged} (auto released {hold.autoReleased})");
            Console.WriteLine($"  {gun}: TAP fires {(tap.firstFire >= 0 ? tap.firstFire - lead : -1)} steps after the press; EARLY release ({holdS} + 0.2 s): charged {early.charged}, cancels {early.cancels}, cooldown left {early.cooldownAtEnd}; FULL release ({holdS} + {fullS} s): charged {full.charged} ({full.chargedHits} hit), cooldown left {full.cooldownAtEnd} steps (want {Steps(cdS) - Steps(0.2)} = {cdS} s less 0.2 s)");
            Console.WriteLine($"  {gun}: MELEE press while charging: charged {melee.charged}, cancels {melee.cancels} (by melee {melee.meleeCancelled}), cooldown left {melee.cooldownAtEnd}");
            Need(hold.firesLate > 0 && hold.killsLate > 0, $"{gun}: a held trigger stalls (after 1.5 s: {hold.firesLate} shots, {hold.killsLate} kills)");
            Need(tap.firstFire == lead, $"{gun}: tap latency {tap.firstFire - lead}");
            Need(early.charged == 0 && early.cancels == 1 && early.cooldownAtEnd == 0, $"{gun}: early release");
            Need(full.charged == 1 && full.cooldownAtEnd == Steps(cdS) - Steps(0.2), $"{gun}: full release / cooldown");
            Need(melee.charged == 0 && melee.meleeCancelled == 1 && melee.cooldownAtEnd == 0, $"{gun}: melee cancel");
        }
        // 6. item 2c (design.n31.charge.comboNails): nails stuck in a bag husk by 1 s of NAILGUN fire, then a full KESTREL charge through it
        // the baseline: the same nails set off by the NAILGUN's own detonate (SPECIAL), so the ratio cancels the hit's own scaling (falloff, armour)
        double baseMean = 0;
        {
            var r = ComboRun31(cfg, map, n31, P, false);
            baseMean = r.hits.Count > 0 ? r.hits.Sum() / r.hits.Count : 0;
        }
        {
            var r = ComboRun31(cfg, map, n31, P, true);
            double mean = r.hits.Count > 0 ? r.hits.Sum() / r.hits.Count : 0;
            Console.WriteLine($"  combo (2c): {r.stuck} nails stuck before the charge; nails the charged shot set off {r.set}; their hits {r.hits.Count}, mean {mean:0.00} vs the NAILGUN's own detonate {baseMean:0.00} = {(baseMean > 0 ? mean / baseMean : 0):0.00}x (want {(n31 ? cfg.Num(P + "comboNails.mult") : 0)}x)");
            if (n31) Need(r.stuck > 0 && r.set > 0 && baseMean > 0 && Math.Abs(mean / baseMean - cfg.Num(P + "comboNails.mult")) < 0.15, "combo 2c"); // the two runs' nails sit at slightly different points (hit scaling differs a little)
        }
        Console.WriteLine("  not checked here: the slug's knock on a boss (R7: Combat.Damage skips Knockback for a non-Husk while design.n31.charge.knockHusksOnly)");
        Console.WriteLine(fails.Count == 0 ? "charge31 GREEN" : $"charge31 RED: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }

    /// <summary>Item 2c's run: 1 s of NAILGUN fire into a bag husk, then either a full KESTREL charge through it (viaCharge) or the NAILGUN's SPECIAL.</summary>
    static (int stuck, int set, List<double> hits) ComboRun31(GameConfig cfg, SurvivalMap map, bool n31, string P, bool viaCharge)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.n27Override = new N27Blocks { chargeShots27 = !n31 };
            sim.n31Override = new N31Blocks { charge = n31 };
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(3104, null, null);
            sim.mover.invulnerable = true;
            foreach (string w in new[] { "revolver", "nailgun" }) if (!sim.loadout.owned.Contains(w)) sim.loadout.AddWeapon(w);
            sim.Respawn(map.playerSpawn, 0);
            (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
            Vec3 sp = map.playerSpawn;
            Husk bag = null;
            int s = 0, chargedAt = -1, comboFrom = Steps(1.0) + Steps(0.8) + (viaCharge ? Steps(cfg.Num(P + "holdSeconds")) : 0); // the hold's charge part: no nail is fired then
            var nailHits = new List<double>();
            sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "nailgun" && s >= comboFrom) nailHits.Add(e.damage); }); // the detonations come before the charged shot's own event
            sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (chargedAt < 0) chargedAt = s; });
            int nailSlot = sim.loadout.owned.IndexOf("nailgun"), revSlot = sim.loadout.owned.IndexOf("revolver");
            double holdS = cfg.Num(P + "holdSeconds"), fullS = cfg.Num(P + "revolver.fullSeconds");
            int nailEnd = Steps(1.0), swapEnd = nailEnd + Steps(0.8), holdEnd = swapEnd + Steps(holdS) + Steps(fullS) + Steps(0.05), end = holdEnd + Steps(0.3);
            int stuckBefore = 0;
            for (s = 0; s < end; s++)
            {
                double py = 0;
                bag = Pin27(sim, bag, sp, ux, uz, 4.0, 1e9);
                if (bag != null && bag.alive) (yaw, py) = ArclineLook(sim.Eye, bag.AimPoint);
                bool nail = s < nailEnd, hold = s >= swapEnd && s < holdEnd;
                if (s == swapEnd - 1 && sim.projectiles != null && bag != null) stuckBefore = sim.projectiles.StuckOn(bag, sim.loadout);
                if (!viaCharge)
                {
                    bool sp2 = s == swapEnd;
                    Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? nailSlot : -1, fire = new Button { held = nail, pressed = s == 0 }, special = new Button { held = sp2, pressed = sp2 } });
                    continue;
                }
                int slot = s == 0 ? nailSlot : s == nailEnd ? revSlot : -1;
                bool down = nail || hold, prev = s > 0 && ((s - 1) < nailEnd || ((s - 1) >= swapEnd && (s - 1) < holdEnd));
                Tick27(sim, yaw, py, new CombatInput { weapon = slot, fire = new Button { held = down, pressed = down && (!prev || s == swapEnd) } });
            }
            return (stuckBefore, sim.loadout.comboNails31, nailHits);
        }
    }

    // ---- unlocks31 -------------------------------------------------------------------------------------------------------------------------

    static void Unlocks31Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        var levels = new[]
        {
            ("Design/levels/n30-foundry.json", new[] { "revolver" }, (string)null),
            ("Design/levels/n30-iceline.json", new[] { "revolver" }, "gauntlet"),
            ("Design/levels/n30-kiln.json", new[] { "revolver", "gauntlet" }, "shotgun"),
            ("Design/levels/n30-span.json", new[] { "revolver", "gauntlet", "shotgun" }, "nailgun"),
            ("Design/levels/n32-foundry.json", new[] { "revolver" }, (string)null), // NIGHT32: the swapped levels and LEVEL 5
            ("Design/levels/n32-iceline.json", new[] { "revolver" }, "gauntlet"),
            ("Design/levels/n32-kiln.json", new[] { "revolver", "gauntlet" }, "shotgun"),
            ("Design/levels/n32-span.json", new[] { "revolver", "gauntlet", "shotgun" }, "nailgun"),
            ("Design/levels/n32-spire.json", new[] { "revolver", "gauntlet", "shotgun", "nailgun" }, (string)null) /* the TORCH comes with the level clear, not in room 1 */,
        };
        Console.WriteLine($"unlocks31: design.n31.unlocks {(off ? "OFF (NIGHT30's start)" : "ON")}; the n26 unlock table applied before step 1 as the game's CampaignUnlocks26.Apply does");
        int fails = 0;
        foreach (var (path, wantStart, award) in levels)
        {
            if (!File.Exists(path)) { Console.WriteLine($"  {path}: missing"); fails++; continue; }
            SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
            SurvivalSim sim = CampaignSim30(cfg, map, N30Blocks.From(cfg));
            sim.n26Override = new N26Blocks { weaponIdentity = true };
            sim.n29Override = new N29Blocks { pickups = true };
            sim.n31Override = new N31Blocks { unlocks = !off };
            var earned = new List<string>();
            sim.events.On<WeaponEarnedEvent>("weaponEarned", e => earned.Add(e.weapon));
            sim.StartRun(3101, null, null);
            bool applied = false;
            int s = 0;
            for (; s < 3; s++)
            {
                if (!applied && sim.campaign != null && File.Exists("Design/levels/unlocks.json"))
                {
                    applied = true; // the game's render-side Apply (it can land before or after the sim's first campaign step)
                    Unlocks26 u = Unlocks26.Parse(File.ReadAllText("Design/levels/unlocks.json"));
                    sim.campaign.unlocks = u;
                    if (!sim.campaign.unlocks31)
                    {
                        List<string> owned = u.OwnedAfter(null);
                        foreach (string w in owned) if (!sim.loadout.owned.Contains(w)) sim.loadout.AddWeapon(w);
                        sim.loadout.FilterOwned26(owned);
                    }
                }
                sim.health.hp = sim.health.max;
                sim.Step();
            }
            string room1 = sim.campaign != null && sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "-";
            var atRoom1 = new List<string>(sim.loadout.owned);
            List<string> startSet = sim.unlocks31Start.Count > 0 ? new List<string>(sim.unlocks31Start) : atRoom1;
            string how = "-";
            if (award != null && !sim.loadout.owned.Contains(award))
            {
                PickupDef pk = sim.campaignLevel.pickups.Find(p => p.kind == "weaponPickup");
                if (pk != null)
                {
                    how = $"pickup {pk.id} in {pk.room}";
                    sim.Respawn(pk.at, 0);
                    for (int k = 0; k < 30 && !sim.loadout.owned.Contains(award); k++) { sim.health.hp = sim.health.max; sim.Step(); }
                }
                else how = "no weapon pickup in the plan";
            }
            else if (award != null) how = $"on entering {room1}";
            var after = new List<string>(sim.loadout.owned);
            var wantAfter = new List<string>(wantStart);
            if (award != null) wantAfter.Add(award);
            bool startOk = Same(startSet, wantStart) && (award == "gauntlet" || Same(atRoom1, wantStart));
            bool afterOk = Same(after, wantAfter.ToArray());
            int dup = 0;
            foreach (string w in earned) if (earned.FindAll(x => x == w).Count > 1) dup++;
            bool ok = startOk && afterOk && dup == 0;
            if (!ok) fails++;
            Console.WriteLine($"  {Path.GetFileNameWithoutExtension(path)}: start set [{string.Join(", ", startSet)}] (want [{string.Join(", ", wantStart)}]); room 1 {room1} owned [{string.Join(", ", atRoom1)}]; " +
                $"award {(award ?? "none")} ({how}): owned [{string.Join(", ", after)}] (want [{string.Join(", ", wantAfter)}]); weaponEarned [{string.Join(", ", earned)}]{(dup > 0 ? " DUPLICATE" : "")} -> {(ok ? "ok" : "MISS")}");
        }
        Console.WriteLine(fails == 0 ? "unlocks31 GREEN" : $"unlocks31 RED ({fails} of {levels.Length} levels miss the NIGHT31 order)");
        code = fails == 0 ? 0 : 1;
    }

    static bool Same(List<string> have, string[] want)
    {
        if (have.Count != want.Length) return false;
        foreach (string w in want) if (!have.Contains(w)) return false;
        return true;
    }
}
