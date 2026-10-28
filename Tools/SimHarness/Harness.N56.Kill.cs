// NIGHT56 builder G (G2, kill55's fix; NIGHT55's view row design.n55.kill55): `n56 kestrel`, the a355 reel's CHAIN input headless (the sim side
// only: kill55 is a view row, so this measures WHEN the kills land in play time, the chain's input; the holds are the Player's). The CAMPAIGN on
// the level file (tap53's boot: Design/levels/<id>.json, DOORFREE, the husks frozen, the health held), 2 sim steps a rendered frame (the reels'
// 60 fps at 120 Hz), the KESTREL in hand, the player at a non-boss room's spawn facing its clearest 12 m line, `--chain` husks (1 hp, held in
// place) in an arc `--ahead` m ahead, 1.6 m apart (a355's Place), the look on the first one alive each frame, then one input scheme:
//   tap   a355 r1: a press (held + pressed) on a frame where the gun's cooldown reads 0 and the frame before pressed nothing, released the next
//   alt   a press / release every other frame while a husk lives (a thumb hammering the trigger): a release inside the KESTREL's interval is
//         queued and fires on the step the interval ends (charge35 / p39, tap53's note)
// Prints each kill's step and play time, the gaps, kills 1 -> 3 in play s, and how many gaps fit design.n55.kill55.numbers.chainGapS.
//   dotnet run --project Tools/SimHarness -c Release -- n56 kestrel [--level n52-foundry] [--scheme tap|alt] [--chain 6] [--ahead 7]
// Probe numbers only: not gameplay.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static int N56Kestrel(string[] args)
    {
        string Arg(string k, string d) { int i = Array.IndexOf(args, k); return i >= 0 && i + 1 < args.Length ? args[i + 1] : d; }
        CultureInfo ci = CultureInfo.InvariantCulture;
        string id = Arg("--level", "n52-foundry"), scheme = Arg("--scheme", "alt");
        int n = int.Parse(Arg("--chain", "6"), ci);
        double ahead = double.Parse(Arg("--ahead", "7"), ci);
        GameConfig cfg = Config();
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var si = new E53Script();
        sim.UseInput(si);
        sim.StartRun(160716, null, null);
        var held = new List<(Husk h, Vec3 at)>();
        void Frame()
        {
            sim.health.max = 100;
            sim.health.hp = 100;
            sim.health.over = 0;
            foreach (var p in held) if (p.h.alive) { p.h.position = p.h.previous = p.at; }
            sim.Step();
            sim.Step();
        }
        for (int i = 0; i < 600 && (sim.campaign == null || sim.campaign.room < 0); i++) Frame();
        sim.campaign?.SetDoorFree29(true);
        sim.husks.frozen = true;
        sim.husks.KillAll();
        if (sim.loadout.owned.IndexOf("revolver") < 0) sim.loadout.AddWeapon("revolver");
        int slot = sim.loadout.owned.IndexOf("revolver");
        for (int i = 0; i < 180 && sim.loadout.Weapon != "revolver"; i++) { si.combat = new CombatInput { weapon = slot }; Frame(); }
        si.combat = new CombatInput { weapon = -1 };

        // the spot: a non-boss room's spawn and its clearest heading (12 m at eye height)
        LevelDef lv = sim.campaignLevel;
        Vec3 f0 = sim.mover.position, u = new Vec3(0, 0, -1);
        double best = -1;
        foreach (RoomDef r in lv.rooms)
        {
            if (r.kind == "boss") continue;
            for (int k = 0; k < 24; k++)
            {
                double a = 2 * Math.PI * k / 24;
                var uk = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                Vec3 eye = new Vec3(r.spawn.x, r.spawn.y + 1.6, r.spawn.z);
                double len = sim.world.RayIntersect(eye, uk, out RayHit hit) ? hit.distance : 99;
                if (len > best) { best = len; f0 = r.spawn; u = uk; }
            }
            if (best >= ahead + 5) break;
        }
        double yaw0 = Math.Atan2(-u.x, -u.z);
        sim.Respawn(f0, yaw0 * 180 / Math.PI);
        for (int i = 0; i < 40; i++) { si.SetLook(yaw0, 0); Frame(); }
        Vec3 feet = sim.mover.position;
        var side = new Vec3(u.z, 0, -u.x);
        for (int k = 0; k < n; k++)
        {
            double off = (k - (n - 1) * 0.5) * 1.6;
            double x = feet.x + u.x * ahead + side.x * off, z = feet.z + u.z * ahead + side.z * off;
            double y = sim.world.RayIntersect(new Vec3(x, feet.y + 2, z), new Vec3(0, -1, 0), out RayHit fh) && fh.distance < 6 ? feet.y + 2 - fh.distance : feet.y;
            var at = new Vec3(x, y, z);
            Husk h = sim.husks.Spawn("husk", at, feet);
            if (h == null) { Console.WriteLine($"  husk {k} at ({x:0.0}, {y:0.0}, {z:0.0}) refused (feet ({feet.x:0.0}, {feet.y:0.0}, {feet.z:0.0}), line {best:0.0} m)"); continue; }
            h.hp = h.maxHp = 1;
            held.Add((h, at));
        }
        for (int i = 0; i < 60; i++) { si.SetLook(yaw0, 0); Frame(); }
        var killSteps = new List<int>();
        Action off0 = sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") killSteps.Add(sim.simStep); });
        int fires = 0;
        Action off1 = sim.events.On("fire", o => fires++);
        bool pressedLast = false;
        int frame = 0;
        for (; frame < 900 && killSteps.Count < held.Count; frame++)
        {
            Husk target = null;
            foreach (var p in held) if (p.h.alive) { target = p.h; break; }
            if (target != null)
            {
                Vec3 e = sim.Eye, t = target.AimPoint;
                double dx = t.x - e.x, dy = t.y - e.y, dz = t.z - e.z;
                si.SetLook(Math.Atan2(-dx, -dz), Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)));
            }
            bool press;
            if (scheme == "tap") press = target != null && !pressedLast && sim.loadout.CooldownFraction(sim.step) <= 0;
            else press = target != null && frame % 2 == 0;
            pressedLast = press;
            si.combat = press ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : new CombatInput { weapon = -1 };
            Frame();
        }
        off0();
        off1();
        double gap = cfg.Num("design.n55.kill55.numbers.chainGapS");
        var line = new System.Text.StringBuilder();
        int fit = 0;
        for (int k = 0; k < killSteps.Count; k++)
        {
            double t = (killSteps[k] - killSteps[0]) * sim.step;
            line.Append($"{(k > 0 ? ", " : "")}kill {k + 1} step {killSteps[k]} (+{t.ToString("0.000", ci)} s");
            if (k > 0)
            {
                double g = (killSteps[k] - killSteps[k - 1]) * sim.step;
                if (g <= gap + 1e-9) fit++;
                line.Append($", gap {(killSteps[k] - killSteps[k - 1])} steps = {g.ToString("0.000", ci)} s");
            }
            line.Append(")");
        }
        double k13 = killSteps.Count >= 3 ? (killSteps[2] - killSteps[0]) * sim.step : double.NaN;
        Console.WriteLine($"n56 kestrel {id} scheme {scheme}: {killSteps.Count}/{held.Count} kills in {frame} frames, {fires} shots; {line}");
        Console.WriteLine($"n56 kestrel {id} scheme {scheme}: kills 1 -> 3 in {k13.ToString("0.000", ci)} s of play (want <= 1 s); gaps inside chainGapS {gap.ToString("0.###", ci)} s: {fit} of {Math.Max(0, killSteps.Count - 1)}; the KESTREL's live interval {sim.loadout.CooldownFraction(sim.step).ToString("0.###", ci)} cooldown share now; p39 intervalMult {(cfg.Has("design.n39.p39.intervalMult") ? cfg.Num("design.n39.p39.intervalMult").ToString("0.###", ci) : "-")}");
        return killSteps.Count == held.Count ? 0 : 1;
    }
}
