// NIGHT14 U3-27 (the shared types): the frame loop's hooks for tonight's systems, one owner per hook (NIGHT14.md section
// 0 rule 11). Each agent implements its partial methods in its own partial file; a hook nobody implements compiles away,
// so with none of them the frame is u3-n13's.
//   K  SurvivalGame.Kiosk.cs    BootKiosk, FrameKiosk (the idle clock, the attract loop), CameraKiosk (the flythrough)
//   V  SurvivalGame.Recap.cs    BootRecap, RecapGate (holds the sim while WHAT GOT YOU plays), RecapRender
//   V2 SurvivalGame.Route.cs    BootRoute, RenderGhost (the ghost runner)
//   T  SurvivalGame.Tape.cs     BootTape (the recorder on the sim; COPY REPORT's tape path)
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>U3-27.7: the attract loop is playing behind TITLE (the sim runs a Playing run on the AUTOPILOT; the UI
        /// shows TITLE). Set only by the kiosk's code.</summary>
        public bool attractOn;

        /// <summary>The flow the UI shows: TITLE over the attract loop, else the sim's flow.</summary>
        public Flow UiFlow => attractOn ? Flow.Title : sim.flow;

        /// <summary>StartGame's end: every night-14 system's boot (after the UI, the run log and TITLE exist).</summary>
        void BootNight14()
        {
            BootTape();
            BootRecap();
            BootRoute();
            BootKiosk();
            // U3-27.4 / U3-27.5: the bot's nav graph (navGraph.js on the map data: seconds to build) cached on disk and built
            // once on a worker at boot, so the attract, the bench's autopilot and the farm find it ready (BotNav is
            // thread-safe and reads only the loaded data).
            BotNav.CacheDir = System.IO.Path.Combine(Application.persistentDataPath, UnityTuning.BotNavCacheFolder);
#if !UNITY_WEBGL || UNITY_EDITOR
            // Only when something will drive the bot here (the review's A-2: with the flags off nothing is built or written):
            // design.autopilot on and the attract possible (the kiosk, or an idle TITLE).
            bool attractPossible = Kiosk.Enabled || cfg.Num("design.attract.idleSeconds") > 0;
            if (!AutopilotInput.Enabled(cfg) || !attractPossible) return;
            // The worker loads its own GameConfig (GameConfig's lookup cache isn't thread-safe: the review's B-6); BotNav keys
            // its cache by content, so the main thread's later call finds this build.
            string navConfig = System.IO.Path.Combine(dataRoot, "config.json");
            SurvivalMap navMap = map;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    BotNav.Survival(GameConfig.Load(navConfig), navMap);
                }
                catch (System.Exception)
                {
                    // the first caller builds it instead (a few seconds then)
                }
            });
#endif
        }

        partial void BootKiosk();

        /// <summary>Each frame after the devices' poll (before the sim's steps): the kiosk's idle clock and the attract.</summary>
        partial void FrameKiosk(double frameDt);

        /// <summary>After the camera feel placed the camera: the attract's flythrough may move it (handled: it did).</summary>
        partial void CameraKiosk(double frameDt, ref bool handled);

        partial void BootRecap();

        /// <summary>WHAT GOT YOU holds the sim (DOWN's clock waits) while it plays.</summary>
        partial void RecapGate(ref bool simulating);

        /// <summary>WHAT GOT YOU draws this frame instead of the live views (drawn: it did; the UI still renders).</summary>
        partial void RecapRender(double frameDt, double alpha, ref bool drawn);

        partial void BootRoute();

        /// <summary>The ghost runner (U3-27.11), in the props' pass.</summary>
        partial void RenderGhost(double frameDt, double alpha);

        partial void BootTape();
    }
}
