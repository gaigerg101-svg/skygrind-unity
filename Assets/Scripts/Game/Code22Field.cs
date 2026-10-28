// NIGHT22 U3-36.8 CHALLENGE CODE (builder W2): the TITLE's CODE field and RESULTS' code line (Core/Challenge21.cs; design.n21.challenge).
//   TITLE: a CODE item in the column (its label shows the code in force, "CODE" when none). Pressing it (A on a pad, a click) opens
//   a small sheet of twelve slots in groups of four: UP / DOWN (the d-pad, the left stick, the arrow keys) step the slot's character,
//   LEFT / RIGHT move the cursor, A or Enter reads the code (a good code is applied to game.setup so the next PLAY plays that run;
//   a bad one says so), B, Escape or a new flow closes the sheet, X or Backspace clears a slot, Ctrl+V pastes the clipboard and
//   typing on a keyboard fills the slots. CLEAR CODE in the sheet is the slot of the Y button (setup back to a plain run).
//   RESULTS: "CODE XXXX-XXXX-XXXX", the code of the run just played, in Code22ResultsText (the lead places it).
// Wiring (the lead): BuildCode22(); in UiMenus.cs right after the QUIT titleColumn.Add and before LayoutTitleColumn();
// RenderCode22(frameDt); in RenderN22Hud. The RESULTS text sits under `results` at UnityTuning.Code22ResultsY.
// UNCOMPILED when written (the builder has no Editor); every call mimics a neighbouring file.
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        MenuItemView code22Item;
        GameObject code22Sheet;
        TextMeshProUGUI code22Slots, code22Hint, code22Status;
        Challenge21.Entry code22Entry;
        bool code22Open;
        string code22Applied; // the code in force on the run setup, or null
        bool code22Typed; // a keyboard text subscription is installed
        TextMeshProUGUI code22Results;
        double code22RepeatAt;

        /// <summary>The sheet is open (the Player reel reads it).</summary>
        public bool Code22IsOpen => code22Open;
        /// <summary>The slots as drawn ("004P-PGWK-099B", '_' blank).</summary>
        public string Code22Shown => code22Entry?.Text();
        /// <summary>The CODE item and its sheet are built.</summary>
        public bool Code22Built => code22Item != null && code22Sheet != null;
        /// <summary>The RESULTS line as drawn ("" when none).</summary>
        public string Code22ResultsShown => code22Results != null ? code22Results.text : "";

        void BuildCode22()
        {
            if (!cfg.Has("design.n21.challenge.length")) return;
            code22Entry = new Challenge21.Entry(cfg);
            Vector2 itemSize = new Vector2(UnityTuning.MenuItemWidth, UnityTuning.MenuItemHeight);
            code22Item = MenuItem(title.transform, "CODE", Vector2.zero, itemSize, UiTheme.Size.S42, Code22Press);
            titleColumn.Add(code22Item.rt);

            code22Sheet = Screen("Code", UiTheme.A(UiTheme.InkSolid, UnityTuning.MenuSheetAlpha));
            var head = Text(code22Sheet.transform, "Head", "CODE", UiTheme.Px(UiTheme.Size.S75), UiTheme.White, TextAlignmentOptions.Center, new Vector2(0, 220), new Vector2(900, 100), new Vector2(0.5f, 0.5f));
            UiTheme.Style(head, UiTheme.Px(UiTheme.Size.S75), UiTheme.White, UiTheme.Display, shadow: true);
            code22Slots = Text(code22Sheet.transform, "Slots", "____-____-____", UiTheme.Px(UiTheme.Size.S75), UiTheme.CyanHot, TextAlignmentOptions.Center, new Vector2(0, 60), new Vector2(1200, 110), new Vector2(0.5f, 0.5f));
            UiTheme.Style(code22Slots, UiTheme.Px(UiTheme.Size.S75), UiTheme.CyanHot, UiTheme.Display);
            code22Slots.richText = true;
            code22Status = Text(code22Sheet.transform, "Status", "", UiTheme.Px(UiTheme.Size.S32), UiTheme.White, TextAlignmentOptions.Center, new Vector2(0, -50), new Vector2(1200, 50), new Vector2(0.5f, 0.5f));
            UiTheme.Style(code22Status, UiTheme.Px(UiTheme.Size.S32), UiTheme.White, UiTheme.BodyBold);
            code22Hint = Text(code22Sheet.transform, "Hint", "UP / DOWN CHANGE   LEFT / RIGHT MOVE   A READ   X CLEAR SLOT   Y CLEAR CODE   B BACK   CTRL+V PASTE", UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, TextAlignmentOptions.Center, new Vector2(0, -200), new Vector2(1500, 40), new Vector2(0.5f, 0.5f));
            UiTheme.Style(code22Hint, UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, UiTheme.BodyBold);

            code22Item.label.text = "CODE";
        }

        /// <summary>The RESULTS code line. Built where Ui builds `results` (UiMenus.cs, right after `results = Screen(...)`): BuildCode22 runs earlier, when `results` is still null (the n22-4 Player threw a NullReferenceException here at startup).</summary>
        void BuildCode22Results()
        {
            if (code22Entry == null) return;
            // RESULTS: the run's code, a small line under the hero tally.
            code22Results = Text(results.transform, "Code line", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, TextAlignmentOptions.Left, new Vector2(0, UnityTuning.Code22ResultsY), new Vector2(900, 36), new Vector2(0.5f, 0.5f));
            UiTheme.Style(code22Results, UiTheme.Px(UiTheme.Size.S24), UiTheme.Muted, UiTheme.BodyBold);
            MenuPinLeft(code22Results, MenuLeft(UnityTuning.Code22ResultsY) + 6, UnityTuning.Code22ResultsY);
        }

        /// <summary>The Player reel's hands: open the sheet, paste `text`, read it (the A press). True when the code was applied to the run setup.</summary>
        public bool Code22Drive(string text)
        {
            if (code22Sheet == null) return false;
            Code22Press();
            code22Entry.Paste(text);
            Code22Draw();
            Code22Read();
            return code22Applied != null && !code22Open;
        }

        void Code22Press()
        {
            if (code22Sheet == null) return;
            code22Open = true;
            code22Entry.Clear();
            code22Status.text = "";
            code22Sheet.SetActive(true);
            if (!code22Typed && Keyboard.current != null)
            {
                Keyboard.current.onTextInput += Code22OnText;
                code22Typed = true;
            }
            code22RepeatAt = 0;
            Code22Draw();
        }

        void Code22Close()
        {
            code22Open = false;
            if (code22Sheet != null) code22Sheet.SetActive(false);
            if (code22Typed && Keyboard.current != null) { Keyboard.current.onTextInput -= Code22OnText; code22Typed = false; }
        }

        void Code22OnText(char c)
        {
            if (!code22Open) return;
            if (c == '\b' || c == '\r' || c == '\n') return; // Backspace and Enter are keys, below
            code22Entry.Type(c);
            Code22Draw();
        }

        void Code22Draw()
        {
            string t = code22Entry.Text();
            var sb = new System.Text.StringBuilder(t.Length + 40);
            int slot = 0;
            foreach (char ch in t)
            {
                if (ch == '-') { sb.Append('-'); continue; }
                if (slot == code22Entry.Cursor) sb.Append("<color=#FFFFFF><u>").Append(ch).Append("</u></color>");
                else sb.Append(ch);
                slot++;
            }
            code22Slots.text = sb.ToString();
        }

        void Code22Read()
        {
            if (code22Entry.TryResult(cfg, out Challenge21.Spec spec))
            {
                Challenge21.Apply(spec, game.setup);
                code22Applied = Challenge21.Encode(spec, cfg);
                code22Item.label.text = "CODE " + code22Applied;
                code22Status.text = "READ: " + spec.mode.ToUpperInvariant() + (spec.heat > 0 ? "  HEAT " + spec.heat : "") + (spec.mutators.Count > 0 ? "  " + spec.mutators.Count + " MUTATOR" + (spec.mutators.Count > 1 ? "S" : "") : "") + "  PLAY STARTS IT";
                Code22Close();
            }
            else code22Status.text = code22Entry.Filled < code22Entry.Length ? "FILL ALL " + code22Entry.Length + " SLOTS" : "NOT A CODE: CHECK THE LAST CHARACTERS";
        }

        void Code22ClearApplied()
        {
            code22Applied = null;
            game.setup.mutators.Clear();
            game.setup.seed = null;
            code22Item.label.text = "CODE";
            code22Status.text = "CODE CLEARED";
        }

        /// <summary>Every frame: the sheet's input (pad and keyboard), the RESULTS line.</summary>
        void RenderCode22(double frameDt)
        {
            if (code22Sheet == null) return;
            // RESULTS: the code of the run just played (the seed it ran, its rung, mode and picks)
            if (game.flow == Flow.Results || game.flow == Flow.Down)
            {
                string line = Challenge21.ResultsLine(cfg, game.setup, game.runSeed, 0);
                string want = line ?? "";
                if (code22Results.text != want) code22Results.text = want;
            }
            if (game.flow != Flow.Title) { if (code22Open) Code22Close(); return; }
            if (!code22Open) return;

            Gamepad pad = Gamepad.current;
            Keyboard kb = Keyboard.current;
            int dx = 0, dy = 0;
            bool a = false, b = false, x = false, y = false, enter = false;
            if (pad != null)
            {
                Vector2 d = pad.dpad.ReadValue() + pad.leftStick.ReadValue() * 1f;
                dx = d.x > 0.5f ? 1 : d.x < -0.5f ? -1 : 0;
                dy = d.y > 0.5f ? 1 : d.y < -0.5f ? -1 : 0;
                a = pad.buttonSouth.wasPressedThisFrame;
                b = pad.buttonEast.wasPressedThisFrame;
                x = pad.buttonWest.wasPressedThisFrame;
                y = pad.buttonNorth.wasPressedThisFrame;
            }
            if (kb != null)
            {
                if (kb.rightArrowKey.isPressed) dx = 1; else if (kb.leftArrowKey.isPressed) dx = -1;
                if (kb.upArrowKey.isPressed) dy = 1; else if (kb.downArrowKey.isPressed) dy = -1;
                enter = kb.enterKey.wasPressedThisFrame;
                b |= kb.escapeKey.wasPressedThisFrame;
                x |= kb.backspaceKey.wasPressedThisFrame;
                if (kb.vKey.wasPressedThisFrame && (kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed))
                {
                    code22Entry.Clear();
                    code22Entry.Paste(GUIUtility.systemCopyBuffer);
                    Code22Draw();
                }
            }
            // a held direction repeats: the first press at once, then every Code22RepeatTime s
            double now = Time.unscaledTimeAsDouble;
            if ((dx != 0 || dy != 0) && now >= code22RepeatAt)
            {
                if (dx != 0) code22Entry.Move(dx);
                if (dy != 0) code22Entry.Cycle(dy);
                code22RepeatAt = now + (code22RepeatAt == 0 || now - code22RepeatAt > UnityTuning.Code22RepeatTime * 2 ? UnityTuning.Code22FirstRepeat : UnityTuning.Code22RepeatTime);
                Code22Draw();
            }
            else if (dx == 0 && dy == 0) code22RepeatAt = 0;
            if (x) { code22Entry.Back(); Code22Draw(); }
            if (y) { Code22ClearApplied(); Code22Close(); }
            else if (a || enter) Code22Read();
            else if (b) Code22Close();
        }
    }
}
