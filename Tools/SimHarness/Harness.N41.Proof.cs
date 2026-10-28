// NIGHT41 builder T2: the proof tooling's harness commands (Program.cs's N41Proof partial).
//   n41proof slidewall [--level id] [--doorfree]   the verbs reel's row 13 (slide bounce) as the Player runs it: a CAMPAIGN run, DOORFREE on when
//       asked (the verbs reel turns it on: Campaign35), the wall Bot/VerbsWall39.Pick gives (with floors), the start 2 m along and 3.3 m out, the reel's
//       0.35 s settle (Reset35), then 1.6 s: 9.9 m/s at 45 deg into the wall, the slide held, the jump at the first wall contact. Prints the wall, the
//       floor under the start, a trace (state, grounded, wall, y, speed) and the row's verdict. (n38kiln slidewall runs it without DOORFREE: kb1's wall.)
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N41Proof(string cmd, string[] args, ref int code)
    {
        if (cmd != "n41proof") return;
        string sub = args.Length > 1 ? args[1] : "";
        if (sub == "slidewall") { code = N41SlideWall(args); return; }
        Console.WriteLine("n41proof: slidewall [--level id] [--doorfree]");
        code = 1;
    }

    static int N41SlideWall(string[] args)
    {
        GameConfig cfg = Config();
        string levelId = "n40-kiln";
        bool doorFree = Array.IndexOf(args, "--doorfree") >= 0;
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--level") levelId = args[i + 1];
        string path = System.IO.Path.GetFullPath(System.IO.Path.Combine("Design", "levels", levelId + ".json"));
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--file") path = System.IO.Path.GetFullPath(args[i + 1]); // another builder's level (git show port/stage3:...)
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var inp = new K4Input();
        inp.next = () => new InputFrame();
        sim.UseInput(inp);
        sim.StartRun(1, null, null);
        for (int s = 0; s < 60; s++) sim.Step();
        if (doorFree) sim.campaign?.SetDoorFree29(true);
        for (int s = 0; s < 12; s++) sim.Step();
        if (!VerbsWall39.Pick(sim, true, false, out Vec3 st, out Vec3 al, out Vec3 nm, out double len, out string room)) { Console.WriteLine("n41proof slidewall: no wall with floors"); return 1; }
        Vec3 inDir = new Vec3((al.x - nm.x) / Math.Sqrt(2), 0, (al.z - nm.z) / Math.Sqrt(2));
        var back = new Vec3(st.x + al.x * 2 + nm.x * 3.3, st.y, st.z + al.z * 2 + nm.z * 3.3);
        if (Array.IndexOf(args, "--fixed") >= 0) // the reel's NIGHT41 start (Bot/VerbsWall41.cs)
        {
            VerbsWall41.Row13Start(sim, st, al, nm, ref back, ref inDir, out string why);
            Console.WriteLine($"  VerbsWall41.Row13Start: {why}");
        }
        string floor = sim.world.RayIntersect(new Vec3(back.x, back.y + 1, back.z), new Vec3(0, -1, 0), out RayHit fh) ? $"{fh.distance - 1:0.00} m under the start (normal y {fh.normal.y:0.00})" : "NONE";
        Console.WriteLine($"  noride boxes in the mover: {(sim.mover.flow34.ride35.noRide != null ? sim.mover.flow34.ride35.noRide.Length : 0)}");
        Console.WriteLine($"n41proof slidewall {levelId} (DOORFREE {doorFree}): wall in room {room}: start ({st.x:0.00}, {st.y:0.00}, {st.z:0.00}) along ({al.x:0.##}, {al.z:0.##}) normal ({nm.x:0.##}, {nm.z:0.##}) {len:0.0} m; the slide's start ({back.x:0.00}, {back.y:0.00}, {back.z:0.00}), floor {floor}");
        // the reel's Reset35: respawn, 0.35 s of no input
        sim.mover.Reset(back.x, back.y, back.z);
        inp.SetLook(Math.Atan2(-inDir.x, -inDir.z), 0);
        inp.next = () => new InputFrame();
        for (int s = 0; s < MovementMath.StepsFor(0.35, sim.step); s++) sim.Step();
        Console.WriteLine($"  after the settle: ({sim.mover.position.x:0.00}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.00}) {sim.mover.state} grounded {sim.mover.grounded}");
        bool pressed = false, slid = false, jumpNow = false;
        int t = 0;
        inp.next = () => new InputFrame { moveZ = 1, slide = new Button { pressed = t == 0, held = !pressed }, jump = new Button { pressed = jumpNow, held = jumpNow } };
        double before = 0, after = 0;
        int pressT = -1;
        for (t = 0; t < MovementMath.StepsFor(1.6, sim.step); t++)
        {
            if (t * sim.step < 0.05) sim.mover.velocity = new Vec3(inDir.x * 9.9, sim.mover.velocity.y, inDir.z * 9.9);
            jumpNow = !pressed && t * sim.step > 0.05 && sim.mover.touchingWall;
            if (jumpNow) { pressed = true; pressT = t; before = sim.mover.Speed; }
            sim.Step();
            if (sim.mover.state == MoveState.Slide) slid = true;
            if (pressT >= 0 && t - pressT > 0 && (t - pressT) * sim.step < 0.15) after = Math.Max(after, sim.mover.Speed);
            if (t < 96 && t % 6 == 0) Console.WriteLine($"  t {t * sim.step:0.00} ({sim.mover.position.x:0.00}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.00}) {sim.mover.state} grounded {sim.mover.grounded} wall {sim.mover.touchingWall} v {sim.mover.Speed:0.0}");
        }
        Console.WriteLine($"  row 13: slid {slid}, wall contact {pressed}, speed {before:0.00} -> {after:0.00}: {(slid && pressed && after >= 0.9 * before && before > 0 ? "PASS" : "RED")}");
        return 0;
    }
}
