// NIGHT64 builder G (slate U1 "One order at a time", design.n64.order64; the rules and the counter: Order64.N64G.cs). The HUD's half, one hook:
// UiHud47.Boss.cs Fight47Late calls Order64Late() right after Fight47Tick(frameDt), so this is the frame's LAST write on bossbar43's hint
// holder: after Bar43Draw's Text33.Set (RenderCard38 -> RenderBar41, earlier in RenderHud33), after UiBossRule61's every-frame hold
// (BossRule61Sub) and its release (BossRule61HintRelease), and after the fight line's queue drew this frame (Fight47Tick: its alpha and text).
// Row on: the holder is set to Order64.HintShown every frame (hidden while the fight line draws the bar's own order; else exactly as without the
// row). Row off: nothing is written, except once on the first frame off after a frame on (D1's held off frame), when the holder is put back as
// the game without the row draws it. The counter runs on every frame the bar is up, the row on or off (the red is its row-off half).
// Proof lines (Player.log; L-089: they print that the input fired; t on FeelClock, the reel's fixed clock):
//   [order64] <boss> order "<text>" drawn by the fight line <n> frames: would-duplicate <w>, duplicate shown <s> | the bar's hint after it:
//             drawn | row order64 on|off | t <s> s (FeelClock)                         when the fight line stops drawing the bar's order
//   [order64] <boss> | row order64 on <n> of <m> frames | would-duplicate <W> frames ... | duplicate shown <S> frames ... | held <h> frames:
//             would-duplicate <hw>, duplicate shown <hs> ... | orders <o>: shown <k> (...) | <why>, t <s> s (FeelClock)
//                                       the totals: after each order line, at the bar's going down, a new boss, a new run and the quit
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        readonly Order64.Count o64 = new Order64.Count();
        bool o64Wrote, o64Wired;

        /// <summary>From Fight47Late after Fight47Tick (hud47 built): the bar's hint for this frame and the counter.</summary>
        void Order64Late()
        {
            Order64Wire();
            bool barUp = b43Root != null && b43Root.gameObject.activeSelf;
            if (!barUp || b43Hint == null)
            {
                if (o64.Dirty) Order64Say("the boss bar went down");
                return;
            }
            string boss = string.IsNullOrEmpty(b43ShownName) ? "?" : b43ShownName;
            if (!string.Equals(boss, o64.Boss, System.StringComparison.Ordinal))
            {
                if (o64.Dirty) Order64Say("a new boss");
                o64.Reset(boss);
            }
            bool rowOn = Order64.On(cfg); // every frame (D1's same-run toggle)
            bool ruleHolds = br61.Armed || br61.Showing; // bossRule61's line holds the place this frame (BossRule61Sub hid the hint)
            string hint = b43Hint.Shown;
            string line = fight47Shown;
            bool lineDrawn = fight47 != null && fight47.rt.gameObject.activeInHierarchy && fight47.Alpha > 0f;
            if (rowOn || o64Wrote)
            {
                bool want = Order64.HintShown(rowOn, ruleHolds, lineDrawn, line, hint);
                if (b43Hint.rt.gameObject.activeSelf != want) b43Hint.rt.gameObject.SetActive(want);
                o64Wrote = rowOn;
            }
            bool hintDrawn = hint.Length > 0 && b43Hint.rt.gameObject.activeInHierarchy;
            if (o64.Frame(FeelClock.Holding, rowOn, lineDrawn, line, hint, Order64.OffShown(ruleHolds, hint), hintDrawn))
            {
                Debug.Log(o64.OrderLine(hintDrawn, rowOn, FeelClock.Now));
                Order64Say("an order's end");
            }
        }

        /// <summary>Once a session: a new run prints the totals and starts clean; the quit prints what is left.</summary>
        void Order64Wire()
        {
            if (o64Wired || game == null) return;
            o64Wired = true;
            game.OnNewRun += () => { if (o64.Dirty) Order64Say("a new run"); o64.Reset(""); };
            Application.quitting += () => { if (o64.Dirty) Order64Say("the quit"); };
        }

        void Order64Say(string why)
        {
            Debug.Log(o64.Line(why, Order64.On(cfg), FeelClock.Now));
            o64.Dirty = false;
        }
    }
}
