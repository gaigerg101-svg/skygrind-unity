// NIGHT33 P (DESIGN33 card P, D-6, row design.n33.shop33): the SHOP between campaign levels, opened by CONTINUE on the RESULTS
// (SurvivalGame.N33ShopBeforeContinue) once per cleared level. GG 2026-10-11: points in the campaign must buy something.
// In builder U's language (UiStyle33, U1 spec 6): no panel fills (one full-screen scrim, not a box), the 20-degree slash as the only ornament (the
// focus marker, the rack's slot ticks), text with a 2 px shadow, 64 / 24 / 16 px type, the wallet in bone white, cyan only for what the player can
// use now (the focus, an equipped upgrade, CONTINUE when focused), never red-orange (nothing here is danger).
//   top-left     SHOP (64) and one 16 px line on the rule (the rack: 2 weapon mods, 1 movement, 1 wildcard)
//   top-right    WALLET (16), the wallet (64, bone), "+N BANKED" from the level just cleared (24)
//   three columns, one per slot kind: the head (24) with the rack's slot ticks (filled cyan = equipped there), then each upgrade: its name (24), its
//                effect (16) and its state (24, right): the price as "2,650" (faded when the wallet is short), OWNED, or EQUIPPED (cyan)
//   under it     nothing: the RESULTS screen is hidden while the shop is open (Ui.Shop33Cover), the world dimmed by the scrim
//   bottom       the message slot (24: BOUGHT / EQUIPPED / NEED n MORE), the hint line (16), CONTINUE: <NEXT LEVEL> (24) bottom-right
// Input: pad (d-pad / left stick to move, A buy / equip / take off, B jumps to CONTINUE, START continues), keyboard (arrows / WASD, Enter / Space,
// Esc) and mouse (hover focuses, a click is A). The RESULTS under it get no input while it is open (the EventSystem's navigation is off and its
// selection cleared; the scrim takes the clicks). Built in code on first open; no Unity API in field initialisers (L-035).
// The reel `shop33` drives it through Move / Confirm / Back and reads the mirrors (FocusNow, MessageNow, WalletNow).
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class UiShop33 : MonoBehaviour // NIGHT43 U1: partial (screens43's look: UiShop33.Screens43.cs)
    {
        sealed class Row
        {
            public Shop33Item item;
            public RectTransform rt;
            public UiStyle33.Text33 name, line, state;
            public Image focus;
        }

        sealed class Col
        {
            public string slot;
            public UiStyle33.Text33 head;
            public readonly List<Image> ticks = new List<Image>();
            public readonly List<Row> rows = new List<Row>();
        }

        SurvivalGame game;
        Canvas canvas;
        CanvasGroup group;
        RectTransform root;
        UiStyle33.Text33 head, sub, walletLabel, wallet, banked, message, hint, cont;
        Image contFocus;
        RectTransform contRt;
        readonly List<Col> cols = new List<Col>();
        bool built;

        // focus: col >= 0 and row inside it, or col == -1 for CONTINUE
        int fc, fr;
        string next = "";
        float openedAt, messageUntil, repeatAt;
        int heldDir; // 0 none, 1 up, 2 down, 3 left, 4 right
        Vector2 mouseWas;
        EventSystem esWas;
        bool navWas;

        public bool IsOpen { get; private set; }

        /// <summary>The focused thing's id ("continue" or an upgrade's id); the reel reads it.</summary>
        public string FocusNow => fc < 0 ? "continue" : cols[fc].rows[fr].item.id;

        /// <summary>The message slot's text now ("" when empty).</summary>
        public string MessageNow => message != null ? message.Shown : "";

        /// <summary>The wallet as the screen shows it.</summary>
        public int WalletNow => game != null ? game.Shop33Wallet : 0;

        /// <summary>The next level's id CONTINUE goes to.</summary>
        public string NextNow => next;

        public void Init(SurvivalGame g)
        {
            game = g; // a scene object: CONTINUE's scene reload destroys it with its game, and the next game makes its own
        }

        void Build()
        {
            if (built) return;
            built = true;
            Shop33Catalogue cat = game.Shop33Cat;
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.Shop33SortOrder;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            gameObject.AddComponent<GraphicRaycaster>();
            group = gameObject.AddComponent<CanvasGroup>();
            root = (RectTransform)transform;

            // the scrim: one full-screen dim (it also takes the clicks meant for the RESULTS under it)
            Image scrim = UiStyle33.Line(root, "Scrim", new Color(UiStyle33.Shadow.r, UiStyle33.Shadow.g, UiStyle33.Shadow.b, UnityTuning.Shop33ScrimAlpha), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            scrim.rectTransform.anchorMin = Vector2.zero;
            scrim.rectTransform.anchorMax = Vector2.one;
            scrim.rectTransform.offsetMin = scrim.rectTransform.offsetMax = Vector2.zero;
            scrim.raycastTarget = true;

            float side = UnityTuning.Shop33Side, top = UnityTuning.Shop33Top;
            Vector2 tl = new Vector2(0, 1), tr = new Vector2(1, 1), bl = new Vector2(0, 0), br = new Vector2(1, 0);
            head = UiStyle33.Label(root, "Head", UnityTuning.Hud33Big, UiStyle33.Bone, UiStyle33.FaceNumber, TextAlignmentOptions.TopLeft, tl, tl, new Vector2(side, -top), new Vector2(UnityTuning.Shop33HeadW, UnityTuning.Hud33Big + 8));
            sub = UiStyle33.Label(root, "Sub", UnityTuning.Hud33Small, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopLeft, tl, tl, new Vector2(side, -UnityTuning.Shop33SubY), new Vector2(UnityTuning.Shop33HeadW, 22));
            walletLabel = UiStyle33.Label(root, "WalletLabel", UnityTuning.Hud33Small, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopRight, tr, tr, new Vector2(-side, -top), new Vector2(UnityTuning.Shop33WalletW, UnityTuning.Shop33WalletLabelH));
            wallet = UiStyle33.Label(root, "Wallet", UnityTuning.Hud33Big, UiStyle33.Bone, UiStyle33.FaceNumber, TextAlignmentOptions.TopRight, tr, tr, new Vector2(-side, -UnityTuning.Shop33WalletNumY), new Vector2(UnityTuning.Shop33WalletW, UnityTuning.Shop33WalletNumH), false);
            banked = UiStyle33.Label(root, "Banked", UnityTuning.Hud33Name, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopRight, tr, tr, new Vector2(-side, -UnityTuning.Shop33BankedY), new Vector2(UnityTuning.Shop33WalletW, 30));

            string[] heads = { "WEAPON MODS", "MOVEMENT", "WILDCARD" };
            for (int c = 0; c < Shop33Catalogue.Slots.Length; c++)
            {
                var col = new Col { slot = Shop33Catalogue.Slots[c] };
                float x = side + c * UnityTuning.Shop33ColPitch;
                col.head = UiStyle33.Label(root, "ColHead" + c, UnityTuning.Hud33Name, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopLeft, tl, tl, new Vector2(x, -UnityTuning.Shop33ColTop), new Vector2(UnityTuning.Shop33ColW - 80, 30));
                col.head.Set(heads[c]);
                int size = cat.SlotSize(col.slot);
                for (int k = 0; k < size; k++)
                {
                    float tx = x + UnityTuning.Shop33ColW - (size - k) * UnityTuning.Shop33RackTickPitch;
                    col.ticks.Add(UiStyle33.Slash(root, $"Rack{c}_{k}", UiStyle33.Bone, tl, tl, new Vector2(tx, -UnityTuning.Shop33ColTop - 3), new Vector2(UnityTuning.Shop33RackTickW, UnityTuning.Shop33RackTickH)));
                }
                foreach (Shop33Item it in cat.items)
                {
                    if (it.slot != col.slot) continue;
                    float y = UnityTuning.Shop33RowTop + col.rows.Count * UnityTuning.Shop33RowPitch;
                    var r = new Row { item = it };
                    r.rt = UiStyle33.Rect(root, "Row_" + it.id, tl, tl, new Vector2(x, -y), new Vector2(UnityTuning.Shop33ColW, UnityTuning.Shop33RowPitch - 16));
                    r.name = UiStyle33.Label(root, "Name_" + it.id, UnityTuning.Hud33Name, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopLeft, tl, tl, new Vector2(x, -y), new Vector2(UnityTuning.Shop33ColW - UnityTuning.Shop33StateW, 30));
                    r.line = UiStyle33.Label(root, "Line_" + it.id, UnityTuning.Hud33Small, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopLeft, tl, tl, new Vector2(x, -y - UnityTuning.Shop33LineGap), new Vector2(UnityTuning.Shop33ColW, 22));
                    r.state = UiStyle33.Label(root, "State_" + it.id, UnityTuning.Shop33StatePx, UiStyle33.Bone, UiStyle33.FaceNumber, TextAlignmentOptions.TopRight, tl, tl, new Vector2(x + UnityTuning.Shop33ColW - UnityTuning.Shop33StateW, -y), new Vector2(UnityTuning.Shop33StateW, 30));
                    r.focus = UiStyle33.Slash(root, "Focus_" + it.id, UiStyle33.Cyan, tl, tl, new Vector2(x - UnityTuning.Shop33FocusTickX, -y), new Vector2(UnityTuning.Shop33FocusTickW, UnityTuning.Shop33FocusTickH));
                    r.name.Set(it.name);
                    r.line.Set(it.line);
                    col.rows.Add(r);
                }
                cols.Add(col);
            }

            message = UiStyle33.Label(root, "Message", UnityTuning.Hud33Name, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.BottomLeft, bl, bl, new Vector2(side, UnityTuning.Shop33MessageY), new Vector2(UnityTuning.Shop33HeadW, 30));
            hint = UiStyle33.Label(root, "Hint", UnityTuning.Hud33Small, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.BottomLeft, bl, bl, new Vector2(side, UnityTuning.Shop33BottomRowY), new Vector2(UnityTuning.Shop33HeadW, 22));
            cont = UiStyle33.Label(root, "Continue", UnityTuning.Hud33Name, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.BottomRight, br, br, new Vector2(-side, UnityTuning.Shop33BottomRowY), new Vector2(UnityTuning.Shop33ContinueW, UnityTuning.Shop33ContinueH));
            contRt = cont.rt;
            contFocus = UiStyle33.Slash(root, "ContinueFocus", UiStyle33.Cyan, br, br, new Vector2(-side - UnityTuning.Shop33ContinueW - 12, UnityTuning.Shop33BottomRowY + 4), new Vector2(UnityTuning.Shop33FocusTickW, UnityTuning.Shop33ContinueH - 8));
            head.Set("SHOP");
            sub.Set("THE RACK HOLDS 2 WEAPON MODS, 1 MOVEMENT, 1 WILDCARD");
            walletLabel.Set("WALLET");
            hint.Set("A  BUY / EQUIP      B  TO CONTINUE      MOVE  D-PAD");
            Build43(); // NIGHT43 U1 screens43 (UiShop33.Screens43.cs): the backdrop, the panels, the entrance; nothing with the row off
            gameObject.SetActive(false);
        }

        public void Open(string nextLevel)
        {
            Build();
            next = nextLevel ?? "";
            IsOpen = true;
            gameObject.SetActive(true);
            openedAt = Time.unscaledTime;
            group.alpha = 0;
            messageUntil = 0;
            message.Set("");
            heldDir = 0;
            // the first thing the wallet can buy, else CONTINUE
            fc = -1;
            fr = 0;
            int wal = game.Shop33Wallet, code = game.Shop33Code;
            for (int c = 0; c < cols.Count && fc < 0; c++)
                for (int r = 0; r < cols[c].rows.Count; r++)
                    if (game.Shop33Cat.CanBuy(code, wal, cols[c].rows[r].item.index)) { fc = c; fr = r; break; }
            string name = NextName(next);
            cont.Set(name.Length > 0 ? "CONTINUE: " + name : "CONTINUE");
            banked.Set(Shop33.LastBanked > 0 ? "+" + Pts(Shop33.LastBanked) + " BANKED" : Shop33.LastBanked == 0 ? "NOTHING NEW BANKED" : "");
            // the RESULTS under it: no navigation, nothing selected (the scrim takes the clicks)
            game.UiView?.Shop33Cover(true); // the RESULTS hidden under the shop (the lead's round 3: they showed through)
            esWas = EventSystem.current;
            if (esWas != null)
            {
                navWas = esWas.sendNavigationEvents;
                esWas.sendNavigationEvents = false;
                esWas.SetSelectedGameObject(null);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mouseWas = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Refresh();
            Open43(); // NIGHT43 U1 screens43: the capture of this frame and the entrance
            Debug.Log($"[skygrind] shop33: open (wallet {wal}, code {code}, next {next}, focus {FocusNow})");
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (esWas != null) esWas.sendNavigationEvents = navWas;
            esWas = null;
            gameObject.SetActive(false);
            if (game != null) game.UiView?.Shop33Cover(false);
        }

        /// <summary>Points as a price reads: "2,650" (invariant thousands separator).</summary>
        static string Pts(int n) => n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>The reel's layout check: pairs of visible shop texts whose drawn glyph boxes overlap (screen px, more than 1 px each way; the shadow
        /// twins skipped), the worst pair named; and the smallest visible font size in px at 1080p.</summary>
        public int OverlapCount(out string worst, out float minPx)
        {
            worst = "";
            minPx = float.MaxValue;
            var boxes = new List<(string name, Rect r)>();
            float k = Screen.height > 0 ? 1080f / Screen.height : 1f;
            foreach (TextMeshProUGUI t in GetComponentsInChildren<TextMeshProUGUI>(false))
            {
                if (t.name == "Shadow" || string.IsNullOrEmpty(t.text) || t.color.a < 0.05f) continue;
                minPx = Mathf.Min(minPx, t.fontSize);
                t.ForceMeshUpdate();
                Bounds b = t.textBounds;
                if (b.size.x <= 0 || b.size.y <= 0) continue;
                Vector3 a = t.rectTransform.TransformPoint(b.min), c = t.rectTransform.TransformPoint(b.max);
                var r = Rect.MinMaxRect(Mathf.Min(a.x, c.x) * k, Mathf.Min(a.y, c.y) * k, Mathf.Max(a.x, c.x) * k, Mathf.Max(a.y, c.y) * k);
                boxes.Add((t.transform.parent != null ? t.transform.parent.name : t.name, r));
            }
            int n = 0;
            float worstArea = 0;
            for (int i = 0; i < boxes.Count; i++)
                for (int j = i + 1; j < boxes.Count; j++)
                {
                    Rect p = boxes[i].r, q = boxes[j].r;
                    float w = Mathf.Min(p.xMax, q.xMax) - Mathf.Max(p.xMin, q.xMin), h = Mathf.Min(p.yMax, q.yMax) - Mathf.Max(p.yMin, q.yMin);
                    if (w <= 1f || h <= 1f) continue;
                    n++;
                    if (w * h > worstArea) { worstArea = w * h; worst = $"{boxes[i].name} x {boxes[j].name} ({w:0}x{h:0} px)"; }
                }
            if (minPx == float.MaxValue) minPx = 0;
            return n;
        }

        string NextName(string id)
        {
            if (string.IsNullOrEmpty(id)) return "";
            foreach (SurvivalGame.MapEntry m in game.Maps)
                if (m.id == id && !string.IsNullOrEmpty(m.label)) return Ui.LevelName30(m.label);
            return id.ToUpperInvariant();
        }

        void Refresh()
        {
            Shop33Catalogue cat = game.Shop33Cat;
            int code = cat.Sanitize(game.Shop33Code), wal = game.Shop33Wallet;
            wallet.Set(Pts(wal));
            for (int c = 0; c < cols.Count; c++)
            {
                Col col = cols[c];
                int eq = cat.EquippedIn(code, col.slot);
                for (int k = 0; k < col.ticks.Count; k++)
                {
                    Color t = k < eq ? UiStyle33.Cyan : UiStyle33.Bone;
                    if (k >= eq) t.a = UnityTuning.Shop33EmptyTickAlpha;
                    col.ticks[k].color = t;
                }
                for (int r = 0; r < col.rows.Count; r++)
                {
                    Row row = col.rows[r];
                    int i = row.item.index;
                    bool owned = Shop33Catalogue.Owns(code, i), on = Shop33Catalogue.Active(code, i), can = cat.CanBuy(code, wal, i), focused = fc == c && fr == r;
                    row.state.Set(on ? "EQUIPPED" : owned ? "OWNED" : Pts(row.item.price));
                    row.state.Color = on ? UiStyle33.Cyan : UiStyle33.Bone;
                    row.name.Color = focused || on ? UiStyle33.Cyan : UiStyle33.Bone;
                    float a = owned || can ? 1f : UnityTuning.Shop33DimAlpha;
                    row.name.Alpha = a;
                    row.line.Alpha = a;
                    row.state.Alpha = a;
                    row.focus.enabled = focused;
                }
            }
            contFocus.enabled = fc < 0;
            cont.Color = fc < 0 ? UiStyle33.Cyan : UiStyle33.Bone;
            Refresh43(); // NIGHT43 U1 screens43: colours by meaning, the focused row's plate
        }

        void Say(string s)
        {
            message.Set(s);
            messageUntil = Time.unscaledTime + UnityTuning.Shop33MessageSeconds;
        }

        // --- the verbs (the input below and the reel call these) -------------------------------------------------------------------------

        /// <summary>Move the focus: dy +1 down / -1 up inside a column (down past the last row: CONTINUE), dx across the columns (the row kept when it exists).</summary>
        public void Move(int dx, int dy)
        {
            if (!IsOpen) return;
            if (fc < 0)
            {
                if (dy < 0 || dx != 0)
                {
                    fc = dx < 0 ? 0 : cols.Count - 1;
                    fr = cols[fc].rows.Count - 1;
                }
            }
            else if (dy != 0)
            {
                int r = fr + dy;
                if (r >= cols[fc].rows.Count) fc = -1;
                else fr = Mathf.Max(0, r);
            }
            else if (dx != 0)
            {
                fc = Mathf.Clamp(fc + dx, 0, cols.Count - 1);
                fr = Mathf.Min(fr, cols[fc].rows.Count - 1);
            }
            Refresh();
        }

        /// <summary>A on the focus: buy it, equip / take it off when owned, or CONTINUE.</summary>
        public void Confirm()
        {
            if (!IsOpen) return;
            if (fc < 0)
            {
                Debug.Log("[skygrind] shop33: CONTINUE");
                game.Shop33Continue();
                return;
            }
            Shop33Item it = cols[fc].rows[fr].item;
            int code = game.Shop33Code;
            if (!Shop33Catalogue.Owns(code, it.index))
            {
                if (game.Shop33Buy(it.index)) Say($"BOUGHT {it.name}: EQUIPPED");
                else Say("NEED " + Pts(it.price - game.Shop33Wallet) + " MORE");
            }
            else if (game.Shop33Toggle(it.index)) Say(Shop33Catalogue.IsEquipped(game.Shop33Code, it.index) ? $"{it.name} EQUIPPED" : $"{it.name} OFF THE RACK");
            Refresh();
        }

        /// <summary>B: the focus jumps to CONTINUE (a second B, or A, goes on).</summary>
        public void Back()
        {
            if (!IsOpen) return;
            if (fc < 0) { Confirm(); return; }
            fc = -1;
            Refresh();
        }

        // --- input ---------------------------------------------------------------------------------------------------------------------------

        void Update()
        {
            if (!IsOpen) return;
            float now = Time.unscaledTime;
            group.alpha = Mathf.Clamp01((now - openedAt) / UnityTuning.Shop33Fade);
            Tick43(now); // NIGHT43 U1 screens43: the staggered entrance
            if (messageUntil > 0 && now > messageUntil) { message.Set(""); messageUntil = 0; }
            if (now - openedAt < UnityTuning.Shop33OpenGuard) return;
            if (esWas != null && esWas.currentSelectedGameObject != null) esWas.SetSelectedGameObject(null); // nothing under the shop takes A / Enter
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;

            // directions: an edge moves, a hold repeats (every pad counts: a second pad or the reel's virtual one is never shut out)
            int dir = 0;
            bool padA = false, padB = false, padStart = false;
            var pads = Gamepad.all;
            for (int p = 0; p < pads.Count; p++)
            {
                Gamepad pad = pads[p];
                Vector2 st = pad.leftStick.ReadValue();
                if (dir == 0)
                {
                    if (pad.dpad.up.isPressed || st.y > UnityTuning.Shop33StickDead) dir = 1;
                    else if (pad.dpad.down.isPressed || st.y < -UnityTuning.Shop33StickDead) dir = 2;
                    else if (pad.dpad.left.isPressed || st.x < -UnityTuning.Shop33StickDead) dir = 3;
                    else if (pad.dpad.right.isPressed || st.x > UnityTuning.Shop33StickDead) dir = 4;
                }
                padA |= pad.buttonSouth.wasPressedThisFrame;
                padB |= pad.buttonEast.wasPressedThisFrame;
                padStart |= pad.startButton.wasPressedThisFrame;
            }
            if (dir == 0 && kb != null)
            {
                if (kb.upArrowKey.isPressed || kb.wKey.isPressed) dir = 1;
                else if (kb.downArrowKey.isPressed || kb.sKey.isPressed) dir = 2;
                else if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) dir = 3;
                else if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) dir = 4;
            }
            if (dir != heldDir)
            {
                heldDir = dir;
                repeatAt = now + UnityTuning.Shop33RepeatDelay;
                if (dir != 0) Step(dir);
            }
            else if (dir != 0 && now >= repeatAt)
            {
                repeatAt = now + UnityTuning.Shop33RepeatEvery;
                Step(dir);
            }

            // the mouse: hover focuses (only when it moved), a click is A on what it is over
            if (mouse != null)
            {
                Vector2 mp = mouse.position.ReadValue();
                bool moved = (mp - mouseWas).sqrMagnitude > 1f;
                mouseWas = mp;
                int hc = -2, hr = -1;
                if (RectTransformUtility.RectangleContainsScreenPoint(contRt, mp, null)) hc = -1;
                for (int c = 0; c < cols.Count && hc == -2; c++)
                    for (int r = 0; r < cols[c].rows.Count; r++)
                        if (RectTransformUtility.RectangleContainsScreenPoint(cols[c].rows[r].rt, mp, null)) { hc = c; hr = r; break; }
                if (hc != -2 && moved && (hc != fc || (hc >= 0 && hr != fr)))
                {
                    fc = hc;
                    if (hc >= 0) fr = hr;
                    Refresh();
                }
                if (hc != -2 && mouse.leftButton.wasPressedThisFrame)
                {
                    fc = hc;
                    if (hc >= 0) fr = hr;
                    Confirm();
                    return;
                }
            }

            bool a = padA || (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame));
            bool b = padB || (kb != null && kb.escapeKey.wasPressedThisFrame);
            bool start = padStart;
            if (start) { fc = -1; Confirm(); }
            else if (a) Confirm();
            else if (b) Back();
        }

        void Step(int dir)
        {
            if (dir == 1) Move(0, -1);
            else if (dir == 2) Move(0, 1);
            else if (dir == 3) Move(-1, 0);
            else if (dir == 4) Move(1, 0);
        }
    }
}
