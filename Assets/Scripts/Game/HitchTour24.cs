// NIGHT24 (builder F, the lead's switch-lag row): the screen-switch hitch tour, `SKYGRIND.exe -skygrindHitch`. UNCOMPILED and UNRUN when written.
// The tour, twice (a COLD pass, then the same WARM pass): TITLE -> PLAY (StartRun) -> PAUSED -> SETTINGS (open, close) -> RESUME -> three weapon
// switches (CombatInput.weapon through the real forced-input path, the one the reel uses) -> BIG PICK (g.mods.Queue(5): the break opens it; then a pick)
// -> RESULTS (sim.EndRun) -> TITLE. After each switch it samples 90 frames and prints "hitch: <tag> worst <ms> ms at frame <n> count>33ms <k>"; a final
// line "hitch SUMMARY cold worst X p99 Y n>33 Z | warm worst ...". Every hitch frame is also logged by Hitch24 as "[hitch] ...".
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class HitchTour24
    {
        const int WindowFrames = 90;

        static IEnumerator Window(string tag, List<string> log)
        {
            Hitch24.BeginWindow(tag);
            for (int i = 0; i < WindowFrames; i++)
            {
                yield return null;
                Hitch24.Sample();
                if (i < 4) Hitch24.Mark("window:" + tag + ":frame" + (i + 1)); // NIGHT25: the first frames after a switch, against the run start's marks
            }
            string line = Hitch24.EndWindow();
            log.Add(line);
            Debug.Log(line);
        }

        static IEnumerator Pass(SurvivalGame g, string pass, List<string> log)
        {
            Hitch24.ResetStats();
            Hitch24.Tag(pass + " TITLE");
            g.ToTitle();
            yield return Window(pass + " TITLE", log);

            Hitch24.Tag(pass + " PLAY");
            g.StartRun(2424);
            yield return Window(pass + " PLAY", log);

            Hitch24.Tag(pass + " PAUSED");
            g.Pause();
            yield return Window(pass + " PAUSED", log);

            Hitch24.Tag(pass + " SETTINGS");
            g.UiView.OpenSettings();
            yield return Window(pass + " SETTINGS", log);
            Hitch24.Tag(pass + " SETTINGS-CLOSE");
            g.UiView.CloseSettings();
            yield return Window(pass + " SETTINGS-CLOSE", log);

            Hitch24.Tag(pass + " RESUME");
            g.Resume();
            yield return Window(pass + " RESUME", log);

            for (int w = 0; w < 3; w++)
            {
                int slot = (w + 1) % 2; // revolver <-> gauntlet through CombatInput.weapon, as a key press does
                Hitch24.Tag(pass + " WEAPON " + (w + 1));
                g.forcedCombat = new CombatInput { weapon = slot };
                yield return null;
                g.forcedCombat = null;
                yield return Window(pass + " WEAPON " + (w + 1), log);
            }

            // BIG PICK: queue a draft (the break opens it), then take card 1
            Hitch24.Tag(pass + " BIGPICK");
            g.mods.Queue(5);
            int guard = 0;
            while (!g.DraftOpen && guard++ < 300) yield return null;
            yield return Window(pass + " BIGPICK", log);
            if (g.DraftOpen)
            {
                Hitch24.Tag(pass + " BIGPICK-PICK");
                g.PickMod(0);
                yield return Window(pass + " BIGPICK-PICK", log);
            }

            Hitch24.Tag(pass + " RESULTS");
            g.sim.downSteps = 0;
            g.sim.EndRun();
            yield return Window(pass + " RESULTS", log);

            Hitch24.Tag(pass + " TITLE2");
            g.ToTitle();
            yield return Window(pass + " TITLE2", log);
        }

        public static IEnumerator Run(SurvivalGame g, List<string> log)
        {
            Hitch24.Begin();
            yield return null; // a frame to settle
            yield return Pass(g, "cold", log);
            string cold = Hitch24.Summary();
            yield return Pass(g, "warm", log);
            string warm = Hitch24.Summary();
            Hitch24.End();
            string line = $"hitch SUMMARY cold {cold} | warm {warm}";
            log.Add(line);
            Debug.Log(line);
        }
    }
}
