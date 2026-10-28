// NIGHT22 U3-36.3 (builder M): `arcline2 [--off] [--tag t]`, the contract `arcline`'s Core half on the harness: the same story the
// Player reel `arcline2` plays (Game/FeelReel.N22.Arcline.cs), scripted on SURVIVAL, every grapple press coming through the PAD path
// (the layout's GRAPPLE button -> HeldActions -> PadFrame -> GamepadMath.MergeMove/MergeCombat; v3 binds it to Y, which also carries
// SPECIAL, so the combat half of the same press is fed too: nothing is forced).
//   1 buy     the terminal bought with the interact key held (5500, ISLANDS opened: the zone is the scenario's, as the run's would be)
//   2 line    three husks stand in a line ahead (10, 12.5, 15 m); the chip reads READY (the crosshair has an enemy)
//   3 yank    one pad press at the nearest: arclinePull 1, the husk moves toward the player; the chip reads COOLDOWN with the seconds
//   4 look    on cooldown with the crosshair on the sky: the chip still reads COOLDOWN, the reticle dims (40 %); on a fresh owned run
//             off cooldown the same sky reads NO TARGET (three different looks, none red: the roles are the view's)
//   5 swing   a second pad press 3 s later, still inside the 15 s cooldown, at a node: ropeAttach 1, arclinePull still 1, and the
//             peak speed after the attach is >= 20 m/s
//   --off     the same story with design.n21.arcline off (--off21's game): nothing to buy, nothing yanks: FAIL (RED), exit 1
// Prints one line per part and the contract's line "arcline2 PASS|FAIL: ...". The sim is untouched (Core/Arcline21*.cs unchanged).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static uint arc2PadHeld;

    /// <summary>One step with the pad's GRAPPLE button down or up: the layout's button -> PadState -> held/latched -> the merged frames.</summary>
    static void Arcline2PadTick(SurvivalSim sim, double yaw, double pitch, bool down, double moveZ = 0, bool dash = false)
    {
        PadLayout layout = GamepadMath.DefaultLayoutV3;
        PadButtonName? b = GamepadMath.ButtonFor(PadAction.Grapple, layout);
        var st = new PadState();
        if (b.HasValue) st.Set(b.Value, down, down ? 1 : 0);
        uint held = GamepadMath.HeldActions(st, layout);
        var pf = new PadFrame { held = held, latched = GamepadMath.PressedSince(held, arc2PadHeld) & GamepadMath.FrameButtons };
        arc2PadHeld = held;
        var fr = new InputFrame { moveZ = moveZ };
        if (dash) fr.dash = new Button { held = true, pressed = true };
        var co = new CombatInput { weapon = -1 };
        GamepadMath.MergeMove(ref fr, pf);
        GamepadMath.MergeCombat(ref co, pf);
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, pitch);
        inp.Set(fr, co);
        sim.hitstopLeft = 0;
        sim.Step();
    }

    static partial void Arcline2Command(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0 && Array.IndexOf(args, "--directed") < 0 && Array.IndexOf(args, "--trick") < 0) // Arcline22Tests' own assertions (the chip's rule, the haptics patterns), outside Unity
        {
            int failed = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Arcline22Tests), Console.WriteLine) + NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Haptics22Tests), Console.WriteLine);
            Console.WriteLine($"arcline2 --selftest {(failed == 0 ? "PASS" : "FAIL")}");
            code = failed == 0 ? 0 : 1;
            return;
        }
        if (Array.IndexOf(args, "--directed") >= 0) { DirectedCommand(args, ref code); return; }
        if (Array.IndexOf(args, "--trick") >= 0) { TrickCommand(args, ref code); return; }
        bool off = Array.IndexOf(args, "--off") >= 0;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        PadButtonName? grapple = GamepadMath.ButtonFor(PadAction.Grapple, GamepadMath.DefaultLayoutV3);
        PadButtonName? special = GamepadMath.ButtonFor(PadAction.Special, GamepadMath.DefaultLayoutV3);
        Console.WriteLine($"  pad: v3 layout binds GRAPPLE to {grapple}, SPECIAL to {special} (one button carries both: {grapple == special})");
        Need(grapple.HasValue, "the layout binds no button to GRAPPLE");

        SurvivalSim sim = ArclineSim(cfg, map, !off, 2601);
        // --- 1 buy ------------------------------------------------------------------------------------------------------
        Buyables.Item item = sim.buyables.list.Find(i => i.id == "gadget:arcline");
        bool owned = false;
        double spent = 0;
        if (item != null)
        {
            sim.zones.Open("islands", true);
            sim.economy.total = item.price() + 100;
            PerkSpot spot = sim.Arcline21Spot().Value;
            double fx = Math.Sin(spot.yaw), fz = Math.Cos(spot.yaw);
            sim.Respawn(new Vec3(spot.pos.x + fx * 1.2, spot.pos.y, spot.pos.z + fz * 1.2), 0);
            (double byaw, _) = ArclineLook(sim.Eye, new Vec3(spot.pos.x, sim.Eye.y, spot.pos.z));
            double s0 = sim.economy.spent;
            for (int s = 0; s < 240 && !sim.arcline21.Owned; s++) ArclineTick(sim, byaw, 0, false, false, 0, true);
            owned = sim.arcline21.Owned;
            spent = sim.economy.spent - s0;
        }
        Console.WriteLine($"  1 buy: terminal in the list {item != null}, owned {owned}, spent {spent:0} (price {(item != null ? item.price() : 0):0})");
        Need(item != null && owned && Math.Abs(spent - item.price()) < 0.5, "the terminal buy was not played");

        // --- 2 line + 3 yank --------------------------------------------------------------------------------------------
        double dt = sim.step;
        int pulls = 0, attaches = 0;
        sim.events.On("arclinePull", _ => pulls++);
        sim.mover.Events += e => { if (e.type == MoverEventType.RopeAttach) attaches++; };
        Vec3 player = map.playerSpawn;
        sim.Respawn(player, 0);
        sim.husks.KillAll();
        (double lookYaw, double ux, double uz) = ArclineOpenDir(sim, player, 15);
        var husks = new List<Husk>();
        foreach (double d in new[] { 10.0, 12.5, 15.0 })
        {
            Husk h = sim.husks.Spawn("husk", new Vec3(player.x + ux * d, 0, player.z + uz * d), player);
            if (h != null) husks.Add(h);
        }
        for (int s = 0; s < 4; s++) Arcline2PadTick(sim, 0, 0, false);
        Husk nearest = null;
        double nd = double.MaxValue;
        foreach (Husk h in husks)
        {
            double d = Vec3.DistanceTo(h.position, sim.mover.position);
            if (d < nd) { nd = d; nearest = h; }
        }
        Need(husks.Count >= 3 && nearest != null, "fewer than 3 husks placed in a line");
        double moved = 0;
        Arcline22Look readyLook = Arcline22Look.Hidden, coolLook = Arcline22Look.Hidden;
        int coolSecs = 0;
        if (nearest != null)
        {
            (double yaw, double pitch) = ArclineLook(sim.Eye, nearest.AimPoint);
            var lk = new InputFrame { yaw = yaw, pitch = pitch };
            readyLook = Arcline22Chip.Resolve(sim.arcline21 != null && sim.arcline21.Owned, sim.arcline21?.YankSteps ?? 0, sim.arcline21 != null ? sim.arcline21.Reticle(lk) : Arcline21Kind.None);
            double d0 = Vec3.DistanceTo(nearest.position, sim.mover.position);
            Arcline2PadTick(sim, yaw, pitch, true); // the press
            Arcline2PadTick(sim, yaw, pitch, false);
            if (sim.arcline21 != null)
            {
                coolLook = Arcline22Chip.Resolve(sim.arcline21.Owned, sim.arcline21.YankSteps, sim.arcline21.Reticle(lk));
                coolSecs = Arcline22Chip.Seconds(sim.arcline21.YankSteps, dt);
            }
            for (int s = 0; s < 58; s++) Arcline2PadTick(sim, yaw, pitch, false);
            moved = d0 - Vec3.DistanceTo(nearest.position, sim.mover.position);
        }
        Console.WriteLine($"  2 line: {husks.Count} husks at 10 / 12.5 / 15 m, chip before the press: {readyLook}");
        Console.WriteLine($"  3 yank: pad press -> arclinePull {pulls}, the nearest moved {moved:0.0} m toward the player in 1 s; chip after: {coolLook} {coolSecs} s");
        Need(readyLook == Arcline22Look.Ready, $"the chip before the press read {readyLook}, not Ready");
        Need(pulls == 1, $"the pad press yanked {pulls} times, not 1");
        Need(coolLook == Arcline22Look.Cooldown && coolSecs >= 14 && coolSecs <= 15, $"the chip after the press read {coolLook} {coolSecs} s, not Cooldown ~15 s");

        // --- 4 look: sky on cooldown (still Cooldown, reticle dims), sky off cooldown (NoTarget) -------------------------------------
        Arcline22Look skyCool = Arcline22Look.Hidden, skyFree = Arcline22Look.Hidden;
        bool dimCool = false, dimFree = false, foundEmpty = false;
        Vec3 emptyEye = default;
        double emptyYaw = 0, emptyPitch = 0;
        if (sim.arcline21 != null)
        {
            SurvivalSim fresh = ArclineSim(cfg, map, true, 2602);
            fresh.arcline21.Own(true);
            double eyeH = cfg.movement.body.eyeHeight;
            double below = cfg.Num("ai.grid.lookBelow"), above = cfg.Num("ai.grid.lookAbove");
            List<Vec3> eyes = Arcline22Chip.EyeGrid(map.floor, eyeH, 6, (x, z) => fresh.nav.grid.NodeAt(x, map.floor.max.y, z, below, above) >= 0);
            foundEmpty = Arcline22Chip.FindEmptyLook(fresh.arcline21, eyes, out emptyEye, out emptyYaw, out emptyPitch);
            Arcline21Kind k = foundEmpty ? Arcline21Kind.None : Arcline21Kind.Surface;
            skyCool = Arcline22Chip.Resolve(sim.arcline21.Owned, sim.arcline21.YankSteps, k);
            dimCool = Arcline22Chip.ReticleDims(sim.arcline21.Owned, k);
            skyFree = Arcline22Chip.Resolve(fresh.arcline21.Owned, 0, k);
            dimFree = Arcline22Chip.ReticleDims(fresh.arcline21.Owned, k);
        }
        Console.WriteLine($"  4 look: {(foundEmpty ? $"an empty look from ({emptyEye.x:0.0}, {emptyEye.z:0.0}) yaw {emptyYaw:0.00} pitch {emptyPitch:0.00}" : "NO empty look on this map from any eye point")}; on cooldown -> {skyCool}, reticle dims {dimCool}; off cooldown -> {skyFree}, reticle dims {dimFree} (the dim is {UnityDimPercent}% alpha, a neutral dim, not a colour)");
        Need(skyCool == Arcline22Look.Cooldown && dimCool, "on cooldown with nothing to hook the chip must stay Cooldown and the reticle dim");
        Need(skyFree == Arcline22Look.NoTarget && dimFree, "off cooldown with nothing to hook the chip must read NoTarget");

        // --- 5 swing inside the cooldown ----------------------------------------------------------------------------------------
        double peak = 0;
        int steps0 = sim.arcline21 != null ? sim.arcline21.YankSteps : 0;
        int cdAtPress = 0;
        string note = "";
        if (sim.arcline21 != null)
        {
            for (int s = 0; s < 120; s++) Arcline2PadTick(sim, 0, 0, false); // 1 s more: the press below is ~3 s after the yank
            sim.husks.KillAll();
            var rng = new Mulberry32(2135);
            Vec3 at = default;
            for (int tries = 0; tries < 2000; tries++)
            {
                double ang = rng.Next() * Math.PI * 2, rad = 12 + rng.Next() * 14;
                double sx = Math.Cos(ang) * rad, sz = Math.Sin(ang) * rad;
                if (!sim.world.RayIntersect(new Vec3(sx, 26, sz), new Vec3(0, -1, 0), out RayHit hit) || 26 - hit.distance > 0.2) continue;
                if (ArclineSwingNode(map, sim.world, new Vec3(sx, 1.6, sz)) == null) continue;
                at = new Vec3(sx, 0, sz);
                break;
            }
            sim.Respawn(at, 0);
            for (int s = 0; s < 40; s++) Arcline2PadTick(sim, 0, 0, false); // land
            Vec3 eye = sim.Eye;
            Vec3 node = ArclineSwingNode(map, sim.world, eye) ?? map.grappleNodes[0];
            (double yaw, double pitch) = ArclineLook(eye, node);
            const int pre = 40, hl = 150, dashBefore = 15;
            for (int s = 0; s < 300 + pre; s++)
            {
                bool hold = s >= pre && s < pre + hl;
                if (s == pre) cdAtPress = sim.arcline21.YankSteps;
                Arcline2PadTick(sim, yaw, pitch, hold, 1, s == pre - dashBefore);
                if (attaches > 0) peak = Math.Max(peak, sim.mover.velocity.Length);
            }
            note = $"(node {node.x:0},{node.y:0},{node.z:0}; cooldown left at the press {cdAtPress * dt:0.0} s of {sim.arcline21.rules.yankCooldown:0})";
        }
        Console.WriteLine($"  5 swing: second pad press inside the cooldown -> ropeAttach {attaches}, arclinePull still {pulls}, peak speed after the attach {peak:0.0} m/s {note}");
        Need(cdAtPress > 0, "the second press was not inside the cooldown");
        Need(attaches == 1, $"the second press attached {attaches} times, not 1");
        Need(pulls == 1, $"arclinePull ended at {pulls}, not 1");
        Need(peak >= 20, $"the speed after the attach peaked at {peak:0.0} m/s, under 20");

        bool ok = fails.Count == 0;
        Console.WriteLine($"arcline2 {(ok ? "PASS" : "FAIL")}{(off ? " (row OFF: the RED run)" : "")}: buy {(owned ? "played" : "NOT played")}, {husks.Count} husks in a line, one pad press ({grapple}) yanked the nearest (arclinePull {pulls}), a second inside the cooldown attached (ropeAttach {attaches}, peak {peak:0.0} m/s), chip looks Ready / Cooldown {coolSecs} s / NoTarget = {readyLook} / {coolLook} / {skyFree}{(ok ? "" : "; " + string.Join("; ", fails))}");
        code = ok ? 0 : 1;
    }

    const int UnityDimPercent = 40;
}
