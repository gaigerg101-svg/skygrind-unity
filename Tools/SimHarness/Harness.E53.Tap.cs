// EXT53 builder B (T-125: "a152's THUNDERCLAP never fires": six 3-frame taps in a152's context gave no `fire`, n52-r20 / n52-freeze; at the freeze
// a152's Equip27(revolver) before them left the THUNDERCLAP in hand too). `tap53`: the a152 reel's sim-side steps, headless, on the level it ran on:
// the CAMPAIGN on the level file (Design/levels/<id>.json, n46flow's / hold52's boot), DOORFREE, the husks frozen, the health held at 100 every
// frame, 2 sim steps a rendered frame (the reels' 60 fps at simHz 120), the THUNDERCLAP added (Loadout.AddWeapon) and asked for until in hand
// (Equip27), then the taps as a152 sent them (the trigger held `--frames` frames, `pressed` on every step of the first, again every `--every`
// frames, `--tries` times), then the KESTREL asked for (round 16's Equip27). Prints each tap's fire and the gun in hand: if the sim fires here,
// the game is right and the Player reel's context is what stopped the trigger.
// Hooked without a Program.cs line (hold52's module initializer pattern):
//   dotnet run --project Tools/SimHarness -c Release -- tap53 [--level n48-foundry] [--frames 3] [--every 20] [--tries 6] [--wait 30] [--off53]
// Probe numbers only: not gameplay.
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void E53TapInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "tap53") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = E53Tap(args);
        }
        catch (Exception e) { Console.WriteLine("tap53: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    /// <summary>EXT53 B (NIGHT52 D-31: "kiln's line ended before 8 s: the queue rule likely, not checked"): `teach53 --level <id> [--seconds 60]` boots the
    /// level's CAMPAIGN as a252 does (DOORFREE, husks frozen, the player still) and prints every teach35 line the SIM emits (id, sim time) in that
    /// time: the lines UiTeach52.Verb's queue rule ("another line is waiting: it ends at NIGHT51's time") would see behind a reel's line.</summary>
    [ModuleInitializer]
    internal static void E53TeachInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "teach53") return;
        string[] args = argv.Skip(1).ToArray();
        int code = 0;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string Arg(string k, string d) { int i = Array.IndexOf(args, k); return i >= 0 && i + 1 < args.Length ? args[i + 1] : d; }
            string id = Arg("--level", "n47-kiln");
            double seconds = double.Parse(Arg("--seconds", "60"), System.Globalization.CultureInfo.InvariantCulture);
            GameConfig cfg = Config();
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.UseInput(new E53Script());
            sim.StartRun(160716, null, null);
            var shown = new System.Collections.Generic.List<string>();
            sim.events.On<Teach35Event>("teach35", e => shown.Add($"{e?.id} @ {sim.simStep * sim.step:0.00} s ({e?.trigger}, {e?.seconds:0.0} s)"));
            int n = MovementMath.StepsFor(seconds, sim.step);
            bool freed = false;
            for (int s = 0; s < n; s++)
            {
                if (!freed && sim.campaign != null && sim.campaign.room >= 0) { sim.campaign.SetDoorFree29(true); sim.husks.frozen = true; freed = true; }
                sim.health.hp = sim.health.max;
                sim.Step();
            }
            Console.WriteLine($"teach53 {id}: {shown.Count} teach35 lines emitted by the sim in {seconds:0} s (campaign {sim.campaignLevel?.id}, room {sim.campaign?.room})");
            foreach (string l in shown) Console.WriteLine("  " + l);
        }
        catch (Exception e) { Console.WriteLine("teach53: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    sealed class E53Script : IPlayerInput
    {
        public CombatInput combat = new CombatInput { weapon = -1 };
        double yaw, pitch;
        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; }
        public bool Captured => true;
        public InputFrame SampleMove(bool active) => new InputFrame { yaw = yaw, pitch = pitch };
        public CombatInput SampleCombat(bool active) => combat;
    }

    static int E53Tap(string[] args)
    {
        string Arg(string k, string d) { int i = Array.IndexOf(args, k); return i >= 0 && i + 1 < args.Length ? args[i + 1] : d; }
        string id = Arg("--level", "n48-foundry");
        int tapFrames = int.Parse(Arg("--frames", "3")), every = int.Parse(Arg("--every", "20")), tries = int.Parse(Arg("--tries", "6"));
        GameConfig cfg = Config();
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var si = new E53Script();
        sim.UseInput(si);
        sim.StartRun(160716, null, null);
        int fires = 0, frame = 0;
        string lastGun = "";
        sim.events.On("fire", o => { fires++; lastGun = o as string ?? "?"; });
        void Frame()
        {
            sim.health.max = 100; // a152's Hold()
            sim.health.hp = 100;
            sim.health.over = 0;
            sim.Step();
            sim.Step();
            frame++;
        }
        for (int i = 0; i < 600 && (sim.campaign == null || sim.campaign.room < 0); i++) Frame();
        sim.campaign?.SetDoorFree29(true);
        sim.husks.frozen = true;
        Console.WriteLine($"tap53 on {id} ({path}): campaign {sim.campaignLevel?.id ?? "none"} room {sim.campaign?.room}, flow {sim.flow}, active {sim.Active}, grounded {sim.mover.grounded}, owned [{string.Join(", ", sim.loadout.owned)}], in hand {sim.loadout.Weapon}");
        bool Equip(string gun)
        {
            if (sim.loadout.owned.IndexOf(gun) < 0) sim.loadout.AddWeapon(gun);
            int slot = sim.loadout.owned.IndexOf(gun);
            int s0 = sim.simStep;
            while ((sim.loadout.Weapon != gun || (sim.simStep - s0) * sim.step < 0.2) && (sim.simStep - s0) * sim.step < 3.0)
            {
                si.combat = new CombatInput { weapon = slot };
                Frame();
            }
            si.combat = new CombatInput { weapon = -1 };
            for (int i = 0; i < 60; i++) Frame(); // Wait27's 1 s
            return sim.loadout.Weapon == gun;
        }
        bool sg = Equip("shotgun");
        Console.WriteLine($"  Equip27(shotgun): in hand {sim.loadout.Weapon} ({(sg ? "yes" : "NO")}), ready {sim.loadout.ready}, grounded {sim.mover.grounded}");
        int wait = int.Parse(Arg("--wait", "30")); // frames held still before the taps (a152's sections 5-6 take ~1000 frames after its Equip27)
        for (int i = 0; i < wait; i++) Frame();
        Console.WriteLine($"  after {wait} still frames: campaign room {sim.campaign?.room} phase {sim.campaign?.phase}, flow {sim.flow}, active {sim.Active}, in hand {sim.loadout.Weapon}, ready {sim.loadout.ready}");
        int heard = 0;
        bool firstFired = false;
        for (int t = 0; t < tries; t++)
        {
            int f0 = fires;
            Charge35 q = sim.charge35;
            int taps0 = q != null ? q.taps : 0, shots0 = q != null ? q.tapShots : 0, c42h0 = q != null ? q.c42Held : 0;
            for (int ph = 0; ph < every; ph++)
            {
                si.combat = ph < tapFrames ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = ph == 0 } } : new CombatInput { weapon = -1 };
                Frame();
            }
            si.combat = new CombatInput { weapon = -1 };
            bool fired = fires > f0;
            if (fired) heard++;
            if (t == 0) firstFired = fired;
            Console.WriteLine($"  tap {t + 1} ({tapFrames} frames held): fire {(fired ? "yes (" + lastGun + ")" : "NO")}, in hand {sim.loadout.Weapon}, grounded {sim.mover.grounded}, active {sim.Active}, flow {sim.flow}"
                + (q != null ? $"; charge35 taps +{q.taps - taps0}, tap shots +{q.tapShots - shots0}, c42 held +{q.c42Held - c42h0}, holding {q.holding}, tap queued {q.tapQueued}" : "; charge35 null"));
        }
        bool kes = Equip("revolver");
        Console.WriteLine($"  Equip27(revolver) after the taps (round 16): in hand {sim.loadout.Weapon} ({(kes ? "yes" : "NO")})");
        // a tap inside the gun's fire interval is queued, never lost (charge35 / p39: "queued until the gun's own fire interval is over"): with
        // `every` frames shorter than the THUNDERCLAP's interval every other tap fires at the next window, so the check is the first tap's shot
        bool ok = firstFired && heard >= 1 && sg && kes;
        Console.WriteLine($"tap53 {(ok ? "FIRES" : "DOES NOT FIRE")}: the first tap fired {(firstFired ? "yes" : "NO")}, {heard}/{tries} tap windows heard a THUNDERCLAP shot headless (a tap inside the gun's interval waits for it); the KESTREL equipped {(kes ? "yes" : "no")}");
        return ok ? 0 : 1;
    }
}
