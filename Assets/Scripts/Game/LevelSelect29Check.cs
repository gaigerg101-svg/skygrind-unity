// NIGHT29 item 1 (GG: the level select "brings me back to the home menu and not start the level"): the Player check
//   SKYGRIND.exe -skygrindLevelSelect29 -logFile <f>
// presses each playable level's slot through the REAL button onClick path (TITLE's PLAY, then the slot's Button.onClick) 20 times with the real
// scene reload (nothing is skipped), a double press on every fifth press, then reads the booted game: flow Playing on the pressed level.
// A press that reloads the scene destroys this object; its statics carry the count and the expectation to the next boot, where it checks the
// run first and then presses again. Prints `[levelselect29] press n -> id: PASS|FAIL ...` and `[levelselect29] DONE`, then quits.
// RED: run it with -skygrindOff29 too (the old Awake-time StartRun, which Start's ToTitle undid): the presses that reload land on TITLE.
using System;
using System.Collections;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class LevelSelect29Check : MonoBehaviour
    {
        const int Presses = 20;
        static int presses, passes, fails;
        static string expectId;
        SurvivalGame game;

        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindLevelSelect29") >= 0;

        public static void Attach(SurvivalGame g) => g.gameObject.AddComponent<LevelSelect29Check>().game = g;

        IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(1.5f);
            while (true)
            {
                if (expectId != null)
                {
                    float t = 0;
                    while ((game.flow != Flow.Playing || game.MapId != expectId) && t < 4f) { t += Time.unscaledDeltaTime; yield return null; }
                    bool ok = game.flow == Flow.Playing && game.MapId == expectId;
                    if (ok) passes++; else fails++;
                    Debug.Log($"[levelselect29] press {presses} -> '{expectId}': {(ok ? "PASS" : "FAIL")} (flow {game.flow}, map '{game.MapId}', waited {t:0.0} s, N29Fixes.LevelSelectDeferStart {N29Fixes.LevelSelectDeferStart})");
                    expectId = null;
                }
                if (presses >= Presses)
                {
                    Debug.Log($"[levelselect29] DONE {presses} presses, {passes} PASS, {fails} FAIL");
                    Application.Quit();
                    yield break;
                }
                if (game.flow != Flow.Title) game.ToTitle();
                yield return new WaitForSecondsRealtime(0.5f);
                game.setup.mode = RunSetup.Campaign;
                if (game.UiView == null || !game.UiView.ReelPressPlay26())
                {
                    Debug.Log("[levelselect29] FAIL: TITLE's PLAY button could not be pressed");
                    fails++;
                    presses++;
                    continue;
                }
                yield return new WaitForSecondsRealtime(0.7f);
                int slot = presses % Math.Max(1, LevelSel26.Params(game.cfg).playable); // NIGHT30 round 4: LEVEL 4 THE SPAN is pressed too while design.n30.span is on (4 playable)
                bool dbl = presses % 5 == 4;
                string id = game.UiView.ReelClickSlot29(slot);
                if (id == "")
                {
                    Debug.Log($"[levelselect29] FAIL: slot {slot + 1} is not a playable level");
                    fails++;
                    presses++;
                    continue;
                }
                if (dbl) { yield return null; game.UiView.ReelClickSlot29(slot); }
                presses++;
                expectId = id;
                yield return new WaitForSecondsRealtime(0.25f); // a reload destroys this object here; the next boot's Start reads the result
            }
        }
    }
}
