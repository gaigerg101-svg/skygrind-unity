// NIGHT21 U3-35.4 (builder M): `slidebite [--tag t]`, SLIDE BITES on SURVIVAL (PortExport/maps/survival.json), scripted: on open
// floor the player runs, then slides along a line with three enemies standing in it (two husks, then a swarmer: 40 hp, 40 hp,
// 10 hp). With design.n21.slideBites on, the slide damages at least two of the three, bites each at most once in one pass (the
// cooldown), the swarmer dies to one bite and that kill pays a CLOSE CALL (a slide kill is a close kill), a SLIDE BITE style
// bonus is paid per bite, and a survivor is staggered. The same script with the block off bites nothing. Exit 0 only when all hold.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void SlideBiteCommand(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        SlideBiteRun on = SlideBitePass(cfg, map, true);
        SlideBiteRun off = SlideBitePass(cfg, map, false);
        int damaged = 0;
        for (int i = 0; i < on.hp.Length; i++) if (on.hp[i] < on.hp0[i]) damaged++;
        int damagedOff = 0;
        for (int i = 0; i < off.hp.Length; i++) if (off.hp[i] < off.hp0[i]) damagedOff++;
        Console.WriteLine($"  on : slide {on.slideSteps} steps, {on.bites} bites ({on.kills} kills), enemies damaged {damaged}/3 (hp now {string.Join(", ", on.hp)} of {string.Join(", ", on.hp0)}), CLOSE CALL awards {on.closeCalls}, SLIDE BITE awards {on.biteAwards}, the survivor's stagger steps {on.maxStagger}");
        Console.WriteLine($"  off: slide {off.slideSteps} steps, {off.bites} bites, enemies damaged {damagedOff}/3");
        bool ok = damaged >= 2 && on.bites >= 2 && on.bites <= 3 && on.biteAwards == on.bites && on.kills >= 1 && on.closeCalls >= 1 && on.maxStagger > 0
            && off.bites == 0 && damagedOff == 0 && on.slideSteps > 0;
        Console.WriteLine($"slidebite {(ok ? "PASS" : "FAIL")}: a scripted slide through 3 enemies damaged {damaged} (want >= 2), {on.bites} bites (one per enemy per pass), the swarmer kill paid CLOSE CALL x{on.closeCalls}, block off bit {off.bites}");
        code = ok ? 0 : 1;
    }

    sealed class SlideBiteRun
    {
        public int bites, kills, closeCalls, biteAwards, slideSteps, maxStagger;
        public double[] hp = new double[3], hp0 = new double[3];
    }

    static SlideBiteRun SlideBitePass(GameConfig cfg, SurvivalMap map, bool on)
    {
        var r = new SlideBiteRun();
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n21Override = on ? new N21Blocks { slideBites = true } : N21Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(2601, null, null);
        sim.events.On("slideBite", o => { var e = (SlideBite21Event)o; r.bites++; if (e.killed) r.kills++; });
        sim.events.On("styleAward", o => { var e = (StyleAwardEvent)o; if (e.name == "CLOSE CALL") r.closeCalls++; if (e.name == "SLIDE BITE") r.biteAwards++; });
        // an open line of 16 m from a floor spot (the lines from the eye to the bodies clear)
        Vec3 from = default;
        double yaw = 0, ux = 0, uz = -1;
        var rng = new Mulberry32(2610);
        for (int t = 0; t < 400; t++)
        {
            double a = rng.Next() * Math.PI * 2, rad = 12 + rng.Next() * 16;
            Vec3 p = new Vec3(Math.Cos(a) * rad, 0, Math.Sin(a) * rad);
            if (!sim.world.RayIntersect(new Vec3(p.x, 26, p.z), new Vec3(0, -1, 0), out RayHit down) || 26 - down.distance > 0.2) continue;
            (double y, double dx, double dz) = ArclineOpenDir(sim, p, 16);
            if (dx == 0 && dz == -1 && y == 0) continue; // none found
            from = p; yaw = y; ux = dx; uz = dz;
            break;
        }
        sim.Respawn(from, yaw * 180 / Math.PI);
        string[] kinds = { "husk", "husk", "swarmer" };
        double[] at = { 7, 9.5, 12 };
        var bodies = new Husk[3];
        for (int i = 0; i < 3; i++)
        {
            bodies[i] = sim.husks.Spawn(kinds[i], new Vec3(from.x + ux * at[i], 0, from.z + uz * at[i]), from);
            r.hp0[i] = bodies[i].hp;
        }
        for (int s = 0; s < 40; s++) SlideBiteTick(sim, yaw, false, 0); // land
        for (int s = 0; s < 200; s++)
        {
            // run for 0.2 s, then slide (held) while pushing on
            bool slide = s >= 24;
            SlideBiteTick(sim, yaw, slide, 1);
            if (sim.mover.state == MoveState.Slide) r.slideSteps++;
            for (int i = 0; i < 3; i++) if (bodies[i].alive) r.maxStagger = Math.Max(r.maxStagger, bodies[i].staggerSteps);
        }
        for (int i = 0; i < 3; i++) r.hp[i] = bodies[i].alive ? bodies[i].hp : 0;
        return r;
    }

    static void SlideBiteTick(SurvivalSim sim, double yaw, bool slide, double moveZ)
    {
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, 0);
        inp.Set(new InputFrame { moveZ = moveZ, slide = new Button { held = slide, pressed = slide } }, new CombatInput { weapon = -1 });
        sim.hitstopLeft = 0;
        sim.Step();
    }
}
