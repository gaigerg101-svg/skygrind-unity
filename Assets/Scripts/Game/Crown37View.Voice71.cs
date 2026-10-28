// NIGHT71 builder B (slate B1, design.n71.voice71; the rule and the counter: Voice71.cs; the HUD's half: UiVoice71.cs). THE CROWN's half, one
// hook: Crown40Draw calls Voice71Late(window) right after it set the call's text (THE WORDS), so this is the frame's last write on call40
// (THE CROWN draws in LateUpdate, after the HUD's frame in SurvivalGame.Update). Row on: call40's holder is set to Voice71.CallShown every
// frame (not drawn while the window call stands beside the HUD's window order; else exactly as without the row). Row off: the holder is
// drawn (crown40's look). Crown40Hide calls Voice71Hide (the totals once the crown's words go). The counter runs on every frame the crown's
// words are up, the row on or off (the red is its row-off half).
// Proof lines (Player.log; L-089: they print that the input fired; t on FeelClock, the reel's fixed clock):
//   [voice71] crown window <k>: both drawn <n> of <m> frames, would both <w>, the call drawn alone <a> | row voice71 on|off | t <s> s (FeelClock)
//                                                                                                  when a window call ends
//   [voice71] crown both drawn <B> frames (...) | would both <W> frames (...) | the window call set <c> frames, drawn alone <a> | the HUD's
//             window order <h> frames (the fight line <l>, the bar <b>) | held <h> frames: would both <hw>, both drawn <hb> (...) | windows <k>
//             | row voice71 on <n> of <m> frames | <why>, t <s> s (FeelClock)        the totals: the crown's words gone, a new run, the quit
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        readonly Voice71.Count v71 = new Voice71.Count();
        bool v71Wired;

        /// <summary>From Crown40Draw after THE WORDS (`window`: the crown's window open now; the call is the window's when it says anything then).</summary>
        void Voice71Late(bool window)
        {
            Voice71Wire();
            if (call40 == null || canvas40 == null) return;
            bool rowOn = Voice71.On(game.cfg); // every frame (D1's same-run toggle)
            bool line = false, bar = false;
            Ui ui = game.UiView;
            if (ui != null) ui.Voice71HudOrder(out line, out bar);
            bool windowCall = window && canvas40.gameObject.activeSelf && Crown40Call.Length > 0;
            bool want = Voice71.CallShown(rowOn, windowCall, line || bar);
            if (call40.gameObject.activeSelf != want) call40.gameObject.SetActive(want);
            bool callDrawn = windowCall && call40.gameObject.activeInHierarchy;
            if (v71.Frame(FeelClock.Holding, rowOn, windowCall, callDrawn, line, bar))
            {
                Debug.Log(v71.WindowLine(rowOn, FeelClock.Now));
                Voice71Say("a window's end");
            }
        }

        /// <summary>From Crown40Hide (the crown's words gone: every frame without a live crown, so cheap).</summary>
        void Voice71Hide()
        {
            if (v71.Dirty) Voice71Say("the crown's words went");
        }

        /// <summary>Once a session: a new run prints the totals and starts clean; the quit prints what is left.</summary>
        void Voice71Wire()
        {
            if (v71Wired || game == null) return;
            v71Wired = true;
            game.OnNewRun += () => { if (v71.Dirty) Voice71Say("a new run"); v71.Reset(); };
            Application.quitting += () => { if (v71.Dirty) Voice71Say("the quit"); };
        }

        void Voice71Say(string why)
        {
            Debug.Log(v71.Line(why, game != null && Voice71.On(game.cfg), FeelClock.Now));
            v71.Dirty = false;
        }
    }
}
