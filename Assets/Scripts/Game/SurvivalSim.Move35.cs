// NIGHT35 builder M (NIGHT35.md M1; the hooks are the lead's SurvivalSim.N35.cs): the movement rows in the sim.
// - wallride35 (design.n35.wallride35, DECIDE35 D-6): the run's Lucio ride rules handed to the Mover at the run's start (Core/Ride35.cs; the
//   Mover's "NIGHT35 wallride35" branches read them), as flow34's are. The row off (or --off35 / -skygrindOff35, or a tape of format 17 or
//   older): Ride35.Params.Neutral, so every branch is skipped and the ride is NIGHT34's bit for bit.
// - grapple35 (design.n35.grapple35, D-2, D-9; audit.md item 3): the grapple pulls you straight to whatever it hit (a node, a bare wall, a big
//   enemy's zip) at the NIGHT31 pull's constant speed and stops there (ULTRAKILL's whiplash, rea-ultrakill.md HookArm :1545: a constant pull,
//   no swing). From outside the Mover (it already has the zip: straight at the anchor at a constant speed, released at the anchor):
//   - GrappleTap31.pullOnFire (the `swing` sub-switch false): every hook that goes out is latched as a pull at once, held or not; a bare
//     surface is never refused (tapNodesOnly cleared). A second press, a jump in the air or a dash still let go (NIGHT31 / 32's cancels).
//   - The arrival: the horizontal speed kept at the anchor is clamped to grapple35.arriveKeepMax (NIGHT31's 12 m/s: a slide past the point)
//     and arrivePop is added up: the run's GrapplePull31Rules is swapped for grapple35's numbers (restored at the next run start).
//   - The unlock (D-2): grapple35Here is false on a campaign level whose place in the campaign order (1-based) is below fromLevel (4), so
//     the lead's auto-own (SurvivalSim.Systems30.cs, Grapple35AutoOwn) leaves the grapple unowned there, and with it the bend (a latch).
//   - The rope view is taut (Arcline21View.Rope33.cs reads Grapple35Pull).
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switches for wallride35 / grapple35 (null: the run's n35 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force35Ride, Force35Grapple;

        /// <summary>The harness's per-rule probe of the ride's numbers (null: the config's). Process-wide, read at each run's start.</summary>
        public static System.Func<Ride35.Params, Ride35.Params> Probe35Ride;

        /// <summary>NIGHT35 grapple35: is this run's grapple the straight pull (the row on, its swing sub-switch off)? The rope view reads it.</summary>
        public bool Grapple35Pull => n35.grapple35 && grappleTap31.pullOnFire;

        /// <summary>NIGHT35 grapple35: this level's place in the campaign order (1-based; 0: not in it) as N35MoveStartRun found it (the harness reads it).</summary>
        public int grapple35LevelPlace;

        GrapplePull31Rules g35Rules, g35Saved;
        bool g35Swapped;

        partial void N35MoveStartRun()
        {
            if (Force35Ride.HasValue) n35.wallride35 = Force35Ride.Value;
            if (Force35Grapple.HasValue) n35.grapple35 = Force35Grapple.Value;
            Ride35.Params rp = Ride35.Params.From(cfg, n35.wallride35);
            if (Probe35Ride != null && rp.on) rp = Probe35Ride(rp);
            mover.SetRide35(rp);
            G35StartRun();
        }

        void G35StartRun()
        {
            if (g35Swapped)
            {
                grapplePull31 = g35Saved; // the run before had grapple35's arrival: NIGHT31's numbers back
                g35Swapped = false;
            }
            bool on = n35.grapple35;
            grappleTap31.pullOnFire = on && n31.grapplePull && !cfg.Design("n35.grapple35.swing");
            if (on) grappleTap31.tapNodesOnly = false; // a bare wall is a target (NIGHT32's D-3 refusal is off under grapple35)
            if (on && n31.grapplePull && grapplePull31 != null && cfg.Has("design.n35.grapple35.arriveKeepMax"))
            {
                g35Rules ??= new GrapplePull31Rules
                {
                    speed = grapplePull31.speed,
                    tapWindow = grapplePull31.tapWindow,
                    arrivePop = cfg.Num("design.n35.grapple35.arrivePop"),
                    arriveKeepMax = cfg.Num("design.n35.grapple35.arriveKeepMax"),
                };
                g35Saved = grapplePull31;
                grapplePull31 = g35Rules;
                g35Swapped = true;
            }
            grapple35LevelPlace = 0;
            if (!on || map.json == null || !map.json.ContainsKey("campaign")) return;
            string id = LevelDef.From(map.json).id;
            N25Blocks b = n25Override ?? (N25Blocks.ForceOff ? N25Blocks.Off : n25Design);
            grapple35LevelPlace = Curriculum25.Order(cfg, b).IndexOf(id) + 1;
            int from = cfg.Has("design.n35.grapple35.fromLevel") ? cfg.Int("design.n35.grapple35.fromLevel") : 1;
            if (grapple35LevelPlace > 0 && grapple35LevelPlace < from) grapple35Here = false;
        }
    }
}
