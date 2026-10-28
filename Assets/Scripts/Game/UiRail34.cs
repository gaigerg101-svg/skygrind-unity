// NIGHT34 builder H (design.n34.rail34, a view row: no sim bit): THE ARSENAL RAIL. GG (2026-10-11): "an ULTRAKILL-style weapon display that shows
// your full arsenal and highlights your current weapon". Bottom right, inside HUD33's layer (it needs design.n33.hud33 on), in place of hud33's pip
// row and its 700 ms switch list; the held name (24 px) and its LT verb (16 px, "ZL ZOOM") stay hud33's own labels, moved over the rail
// (Hud33Layout.RailLt / RailWeaponName). With the row on:
//   - Every owned weapon is a slot in one row, in slot order (a window of Rail34MaxSlots round the hand past that).
//   - The gun in hand: its silhouette filled cyan, at full size, with the cyan hand bar under it. The others: the silhouette's OUTLINE only (never a
//     filled shape), bone, dim (Rail34DimAlpha), at Rail34DimScale.
//   - A swap slides the hand bar (and eases the two icons' sizes) to the new slot over Rail34SlideS (80 ms): a read of the swap, never a delay on it.
//   - Each slot's line under it is its cooldown (Loadout.CooldownStepsOf: every gun ticks in the holster): ready = a whole bone line; cooling = the
//     line in CyanDeep draining to nothing as the cooldown runs out (never red: red-orange is danger only; never a fill box: hud33's rule). The hand
//     bar drains the same way for the gun in hand. The rule for what counts as a cooldown is HudWeapons30's (Slot30Rules.Gun: shorter than
//     Hud30MinCoolSeconds never shows, so a shot cycle does not flicker).
//   - A holstered gun turning ready (Slot30Rules.TurnedReady) pulses its slot once: the outline grows Rail34PulseScale and goes cyan over Rail34PulseS.
//   - Inside a bend (sim.Bend34Depth01 > 0, builder B's) the holstered outlines brighten one step (Rail34DimAlpha -> Rail34BendDimAlpha, with the depth).
// Off: the row off, hud33 off, or -skygrindOff34 (N34Fixes.ViewRowOn): nothing is built and hud33's pip row is as NIGHT33 left it.
// Numbers: UnityTuning.Hud34.cs. Unity objects are made in BuildRail34 (from BuildHud33Now), never in a field initialiser (L-035).
using System;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool rail34On, rail34Read;

        /// <summary>design.n34.rail34 on (and hud33 on: the rail lives in HUD33's layer).</summary>
        bool Rail34On
        {
            get
            {
                if (!rail34Read && cfg != null)
                {
                    rail34On = N34Fixes.ViewRowOn(cfg, "rail34");
                    rail34Read = true;
                }
                return rail34On && Hud33On;
            }
        }

        // ---- for the reels (hud34): what the rail draws now ----
        /// <summary>The rail is built and drawn this frame.</summary>
        public static bool Rail34Live;
        /// <summary>The rail's root (its rect is the rule checker's "Rail 34" item).</summary>
        public static RectTransform Rail34Root;
        /// <summary>Slots drawn, the highlighted slot's index among them (-1 none), the swaps seen, the READY pulses begun.</summary>
        public static int Rail34Slots, Rail34HandSlot = -1, Rail34Swaps, Rail34Pulses;
        /// <summary>The slide's progress (1: the hand bar is on its slot) and the holstered outlines' opacity now (the bend's brighten reads here).</summary>
        public static float Rail34SlideNow = 1f, Rail34DimNow;
        /// <summary>A swap the rail drew: the new gun's id (GG's sound hook "rail34Swap"; view only).</summary>
        public static event Action<string> Rail34Swapped;

        sealed class Rail34Slot
        {
            public RectTransform rt;
            public Image fill, outline, line;
            public string id;
            public float heldFrom, heldNow; // 0 holstered .. 1 in hand (eased over the slide)
            public int state = -1;
            public bool wasWaiting;
            public double pulseAge = double.MaxValue;
            public float shownScale = -1, shownFill = -1, shownOutline = -1, shownLine = -1, shownLineA = -1;
            public Color shownOutlineC, shownLineC;
        }

        static Ui rail34Ui; // the Ui that built the rail (Rail34Describe, for the reels)
        RectTransform r34Root;
        Image r34Hand;
        readonly List<Rail34Slot> r34Slots = new List<Rail34Slot>();
        readonly Dictionary<string, int> r34Full = new Dictionary<string, int>(), r34Prev = new Dictionary<string, int>();
        static readonly Dictionary<string, Sprite> Rail34Outlines = new Dictionary<string, Sprite>();
        int r34First = -1, r34Count = -1, r34HandIdx = -1;
        string r34ShownHeld;
        float r34HandFromX, r34HandToX;
        double r34SlideAge = double.MaxValue;
        float r34ShownHandFill = -1;
        Color r34ShownHandC;

        static float SlotX34(int i, int shown) => -((shown - 1 - i) * UnityTuning.Rail34SlotPitch) - UnityTuning.Rail34SlotPitch * 0.5f;

        /// <summary>From BuildHud33Now with the row on: the rail, and hud33's pip row hidden, its LT label and name moved over the rail.</summary>
        void BuildRail34()
        {
            Vector2 br = new Vector2(1, 0), bc = new Vector2(0.5f, 0);
            Hud33Layout.Box rb = Hud33Layout.Rail(UnityTuning.Rail34MaxSlots);
            r34Root = UiStyle33.Rect(hud33, Hud33Layout.RailName, br, br, P(rb), S(rb));
            Rail34Root = r34Root;
            rail34Ui = this;
            for (int i = 0; i < UnityTuning.Rail34MaxSlots; i++)
            {
                var s = new Rail34Slot();
                s.rt = UiStyle33.Rect(r34Root, "Slot " + i, br, bc, Vector2.zero, new Vector2(UnityTuning.Rail34SlotPitch, (float)Hud33Layout.RailH));
                float iconY = UnityTuning.Rail34HandBarH + UnityTuning.Rail34LineGap;
                s.line = UiStyle33.Line(s.rt, "Line", UiStyle33.Bone, bc, bc, new Vector2(0, (UnityTuning.Rail34HandBarH - UnityTuning.Rail34LineH) * 0.5f), new Vector2(UnityTuning.Rail34LineW, UnityTuning.Rail34LineH));
                s.line.sprite = UiStyle33.WhiteSprite();
                s.line.type = Image.Type.Filled;
                s.line.fillMethod = Image.FillMethod.Horizontal;
                s.line.fillOrigin = (int)Image.OriginHorizontal.Left; // it drains toward the left as the cooldown runs out
                s.outline = UiStyle33.Line(s.rt, "Outline", UiStyle33.Bone, bc, bc, new Vector2(0, iconY), new Vector2(UnityTuning.Rail34IconW, UnityTuning.Rail34IconH));
                s.outline.preserveAspect = true;
                s.fill = UiStyle33.Line(s.rt, "Icon", UiStyle33.Cyan, bc, bc, new Vector2(0, iconY), new Vector2(UnityTuning.Rail34IconW, UnityTuning.Rail34IconH));
                s.fill.preserveAspect = true;
                s.rt.gameObject.SetActive(false);
                r34Slots.Add(s);
            }
            r34Hand = UiStyle33.Line(r34Root, "Hand", UiStyle33.Cyan, br, bc, Vector2.zero, new Vector2(UnityTuning.Rail34HandBarW, UnityTuning.Rail34HandBarH));
            r34Hand.sprite = UiStyle33.WhiteSprite();
            r34Hand.type = Image.Type.Filled;
            r34Hand.fillMethod = Image.FillMethod.Horizontal;
            r34Hand.fillOrigin = (int)Image.OriginHorizontal.Left;
            r34Hand.gameObject.SetActive(false);
            // hud33's pip row hides (the rail is it); its LT label and name stand over the rail
            if (h33Pips != null && h33Pips.Length > 0 && h33Pips[0] != null) h33Pips[0].transform.parent.gameObject.SetActive(false);
            h33Lt.rt.anchoredPosition = P(Hud33Layout.RailLt);
            h33Weapon.rt.anchoredPosition = P(Hud33Layout.RailWeaponName);
            game.OnNewRun += () =>
            {
                r34ShownHeld = null;
                r34First = r34Count = r34HandIdx = -1;
                r34SlideAge = double.MaxValue;
                r34Full.Clear();
                r34Prev.Clear();
                Rail34Swaps = Rail34Pulses = 0;
            };
        }

        /// <summary>A gun's silhouette as its outline only (the same rects and slant as UiHotbar's WeaponSilhouette, the inside eroded by
        /// Rail34OutlinePx): made once per gun.</summary>
        static Sprite RailOutline34(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (Rail34Outlines.TryGetValue(id, out Sprite cached)) return cached;
            if (!UnityTuning.Hud17Silhouettes.TryGetValue(id, out float[] rects)) rects = UnityTuning.Hud17Silhouettes["revolver"];
            int w = UnityTuning.Hud17CardSilTex, h = w / 2;
            float unit = w / 32f, k = Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad);
            var inside = new bool[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float v = (y + 0.5f) / unit, u = (x + 0.5f) / unit;
                    u -= (v - 8f) * k * 0.6f; // WeaponSilhouette's slant
                    for (int r = 0; r + 3 < rects.Length; r += 4)
                        if (u >= rects[r] && u < rects[r] + rects[r + 2] && v >= rects[r + 1] && v < rects[r + 1] + rects[r + 3]) { inside[y * w + x] = true; break; }
                }
            // erode: a pixel is interior when every pixel within Rail34OutlinePx (a square) is inside; the outline is inside and not interior.
            // Separable: a horizontal run test, then a vertical one.
            int d = UnityTuning.Rail34OutlinePx;
            var row = new bool[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool all = true;
                    for (int dx = -d; dx <= d && all; dx++) { int xx = x + dx; all = xx >= 0 && xx < w && inside[y * w + xx]; }
                    row[y * w + x] = all;
                }
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool all = true;
                    for (int dy = -d; dy <= d && all; dy++) { int yy = y + dy; all = yy >= 0 && yy < h && row[yy * w + x]; }
                    bool edge = inside[y * w + x] && !all;
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(edge ? 255 : 0));
                }
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Rail34 outline " + id };
            tex.SetPixels32(px);
            tex.Apply(false, true);
            Sprite s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            Rail34Outlines[id] = s;
            return s;
        }

        /// <summary>A gun's cooldown left and the longest seen since it last reached 0 (the line's whole; HudWeapons30's Cool30 rule, own records).</summary>
        int Cool34(Loadout lo, string id, out int full)
        {
            int left = Hud30Reads.CooldownSteps(lo, id);
            r34Prev.TryGetValue(id, out int prev);
            r34Full.TryGetValue(id, out full);
            if (left > prev || left > full) full = left;
            if (left <= 0) full = 0;
            int ruled = Hud30Reads.FullSteps(lo, id);
            if (left > 0 && ruled >= left) full = ruled;
            r34Full[id] = full;
            r34Prev[id] = left;
            return left;
        }

        /// <summary>Every frame from Weapon33 with the row on (hud33's layer is up).</summary>
        void RenderRail34(double frameDt)
        {
            Loadout lo = game.loadout;
            SurvivalSim sim = game.sim;
            if (lo == null || sim == null || r34Root == null) { Rail34Live = false; return; }
            Rail34Live = true;
            // the hand is the gun the press asked for (Loadout.targetSlot, as the viewmodel's WantGun): the highlight moves on the press, not at the
            // swap's bottom step (a read of the input; hud33's name above follows the sim's gun in hand)
            int n = lo.owned.Count, hand = -1;
            string held = lo.Weapon;
            for (int i = 0; i < n; i++) if (lo.owned[i] == held) { hand = i; break; }
            if (lo.targetSlot >= 0 && lo.targetSlot < n) { hand = lo.targetSlot; held = lo.owned[hand]; }
            int shown = Math.Min(n, r34Slots.Count);
            int first = n <= shown ? 0 : Math.Max(0, Math.Min(n - shown, hand - shown / 2));
            bool layout = first != r34First || shown != r34Count;
            for (int i = 0; i < shown && !layout; i++) if (r34Slots[i].id != lo.owned[first + i]) layout = true;
            if (layout)
            {
                r34First = first;
                r34Count = shown;
                for (int i = 0; i < r34Slots.Count; i++)
                {
                    Rail34Slot s = r34Slots[i];
                    bool on = i < shown;
                    if (s.rt.gameObject.activeSelf != on) s.rt.gameObject.SetActive(on);
                    if (!on) { s.id = null; continue; }
                    string id = lo.owned[first + i];
                    s.rt.anchoredPosition = new Vector2(SlotX34(i, shown), 0);
                    if (s.id == id) continue;
                    s.id = id;
                    s.fill.sprite = WeaponSilhouette(id); // UiHotbar's, made once per gun
                    s.outline.sprite = RailOutline34(id);
                    s.state = -1;
                    s.wasWaiting = false;
                    s.pulseAge = double.MaxValue;
                    s.heldNow = s.heldFrom = id == held ? 1f : 0f;
                    s.shownScale = s.shownFill = s.shownOutline = s.shownLine = s.shownLineA = -1;
                }
                r34Root.sizeDelta = new Vector2(Math.Max(1, shown) * UnityTuning.Rail34SlotPitch, (float)Hud33Layout.RailH);
                r34HandIdx = -2; // re-place the hand bar
            }
            Rail34Slots = shown;
            int handIdx = hand >= first && hand < first + shown ? hand - first : -1;

            // a swap: the hand bar slides from where it is now to the new slot; the icon sizes ease with it
            if (held != r34ShownHeld)
            {
                if (r34ShownHeld != null && !string.IsNullOrEmpty(held))
                {
                    Rail34Swaps++;
                    FeelProbe.Event(UnityTuning.Rail34SwapEvent);
                    Rail34Swapped?.Invoke(held);
                }
                r34ShownHeld = held;
            }
            if (handIdx != r34HandIdx)
            {
                float to = handIdx >= 0 ? SlotX34(handIdx, shown) : 0f;
                bool slide = r34HandIdx >= 0 && handIdx >= 0 && !layout;
                r34HandFromX = slide ? r34Hand.rectTransform.anchoredPosition.x : to;
                r34HandToX = to;
                r34SlideAge = slide ? 0 : double.MaxValue;
                for (int i = 0; i < shown; i++) r34Slots[i].heldFrom = slide ? r34Slots[i].heldNow : i == handIdx ? 1f : 0f;
                r34HandIdx = handIdx;
                bool handOn = handIdx >= 0;
                if (r34Hand.gameObject.activeSelf != handOn) r34Hand.gameObject.SetActive(handOn);
            }
            float k = 1f;
            if (r34SlideAge < double.MaxValue)
            {
                r34SlideAge += frameDt;
                k = r34SlideAge >= UnityTuning.Rail34SlideS ? 1f : UiTween.OutCubic(r34SlideAge / UnityTuning.Rail34SlideS);
                if (k >= 1f) r34SlideAge = double.MaxValue;
            }
            Rail34SlideNow = k;
            Rail34HandSlot = handIdx;
            if (handIdx >= 0)
            {
                float x = Mathf.Round(Mathf.Lerp(r34HandFromX, r34HandToX, k));
                if (r34Hand.rectTransform.anchoredPosition.x != x) r34Hand.rectTransform.anchoredPosition = new Vector2(x, 0);
            }

            // the bend brightens the holstered outlines one step
            float bend = Mathf.Clamp01((float)sim.Bend34Depth01);
            float dim = Mathf.Lerp(UnityTuning.Rail34DimAlpha, UnityTuning.Rail34BendDimAlpha, bend);
            dim = Mathf.Round(dim * 50f) / 50f;
            Rail34DimNow = dim;

            double dt = sim.step;
            Color bone = UiStyle33.Bone, cyan = UiStyle33.Cyan, deep = UiStyle33.CyanDeep;
            for (int i = 0; i < shown; i++)
            {
                Rail34Slot s = r34Slots[i];
                float target = i == handIdx ? 1f : 0f;
                s.heldNow = Mathf.Lerp(s.heldFrom, target, k);
                int left = Cool34(lo, s.id, out int full);
                Slot30Look look = Slot30Rules.Gun(left, full, dt, false, 0, 0, 0);
                // a holstered gun turning ready: one pulse (the gun in hand fires through its own cadence: no pulse there)
                if (Slot30Rules.TurnedReady(s.wasWaiting, look.state) && s.state >= 0 && i != handIdx)
                {
                    s.pulseAge = 0;
                    Rail34Pulses++;
                    FeelProbe.Event(UnityTuning.Rail34ReadyEvent);
                }
                s.wasWaiting = look.waiting;
                s.state = (int)look.state;
                float pulse = 0f;
                if (s.pulseAge < double.MaxValue)
                {
                    pulse = s.pulseAge >= UnityTuning.Rail34PulseS ? 0f : Mathf.Sin((float)(s.pulseAge / UnityTuning.Rail34PulseS) * Mathf.PI);
                    s.pulseAge += frameDt;
                    if (s.pulseAge >= UnityTuning.Rail34PulseS) s.pulseAge = double.MaxValue;
                }
                bool cooling = look.state == Slot30.Cooling;
                float scale = Mathf.Lerp(UnityTuning.Rail34DimScale, 1f, s.heldNow) * (1f + UnityTuning.Rail34PulseScale * pulse);
                scale = Mathf.Round(scale * 100f) / 100f;
                if (scale != s.shownScale)
                {
                    s.shownScale = scale;
                    var size = new Vector2(UnityTuning.Rail34IconW * scale, UnityTuning.Rail34IconH * scale);
                    s.fill.rectTransform.sizeDelta = s.outline.rectTransform.sizeDelta = size;
                }
                float fillA = Mathf.Round(s.heldNow * (cooling ? UnityTuning.Hud30CoolIconAlpha : 1f) * UnityTuning.Rail34HeldAlpha * 50f) / 50f;
                if (fillA != s.shownFill)
                {
                    s.shownFill = fillA;
                    s.fill.color = UiTheme.A(cyan, fillA);
                    s.fill.enabled = fillA > 0f;
                }
                float outA = (1f - s.heldNow) * Mathf.Lerp(cooling ? dim * UnityTuning.Hud30CoolIconAlpha : dim, 1f, pulse);
                outA = Mathf.Round(outA * 50f) / 50f;
                Color outC = pulse > 0f ? Color.Lerp(bone, cyan, pulse) : bone;
                if (outA != s.shownOutline || outC != s.shownOutlineC)
                {
                    s.shownOutline = outA;
                    s.shownOutlineC = outC;
                    s.outline.color = UiTheme.A(outC, outA);
                    s.outline.enabled = outA > 0f;
                }
                // the line: whole and bone when ready; CyanDeep, draining, while cooling; under the hand bar it hides
                float lineFill = cooling ? Mathf.Round(look.ring * 50f) / 50f : 1f;
                float lineA = (1f - s.heldNow) * (cooling ? UnityTuning.Rail34CoolLineAlpha : UnityTuning.Rail34LineAlpha * Mathf.Lerp(1f, UnityTuning.Rail34BendDimAlpha / UnityTuning.Rail34DimAlpha, bend));
                lineA = Mathf.Round(Mathf.Clamp01(lineA) * 50f) / 50f;
                Color lineC = cooling ? deep : bone;
                if (lineFill != s.shownLine) { s.shownLine = lineFill; s.line.fillAmount = lineFill; }
                if (lineA != s.shownLineA || lineC != s.shownLineC)
                {
                    s.shownLineA = lineA;
                    s.shownLineC = lineC;
                    s.line.color = UiTheme.A(lineC, lineA);
                    s.line.enabled = lineA > 0f;
                }
                if (i == handIdx)
                {
                    // the hand bar drains the same way for the gun in hand
                    float hf = cooling ? Mathf.Round(look.ring * 50f) / 50f : 1f;
                    Color hc = cooling ? deep : cyan;
                    if (hf != r34ShownHandFill) { r34ShownHandFill = hf; r34Hand.fillAmount = hf; }
                    if (hc != r34ShownHandC) { r34ShownHandC = hc; r34Hand.color = hc; }
                }
            }
            RenderRail35Charge(handIdx, frameDt); // NIGHT35 W: charge35's CHARGING state and its cooldown (UiRail34.Charge35.cs)
        }

        // ---- THE BEND METER (design.n34.bendLook): one cyan slash beside hud33's two grapple ticks under the reticle (nothing new on screen: it
        // joins their row). Its fill is builder B's sim.Bend34Meter (0 .. 1: kills refill it, never time); whole and bright cyan when full (the next
        // latch is a FULL bend), CyanDeep while it refills. Shown while the bend row is live in the sim (sim.Bend34On) and the run plays. ----
        Image r34MeterFill, r34MeterTrack;
        float r34ShownMeter = -1;
        Color r34ShownMeterC;
        /// <summary>For the reels: the meter's slash drawn this frame and its fill.</summary>
        public static bool Bend34MeterShown;
        public static float Bend34MeterNow = -1;

        void BuildBendMeter34()
        {
            Transform ret = hud33.Find("Reticle");
            if (ret == null) return;
            var mid = new Vector2(0.5f, 0.5f);
            var at = new Vector2(0.5f * UnityTuning.Hud33GrapplePitch + UnityTuning.Bend34MeterGap + UnityTuning.Hud33TickW, UnityTuning.Hud33GrappleY);
            if (Hud38On) at = new Vector2(UnityTuning.Hud38BendX, GrappleRowY); // NIGHT38 hud38: the charge bar sits where it was; it joins the grapple's hook row (NIGHT40 arc40: the row moves under the arc, UiHud38.Arc40.cs)
            r34MeterFill = UiStyle33.SlashFill(ret, "Bend meter", UiStyle33.Cyan, mid, mid, at, new Vector2(UnityTuning.Hud33TickW, UnityTuning.Hud33TickH), out r34MeterTrack, UiTheme.A(UiStyle33.Cyan, UnityTuning.Hud33GrappleSpentAlpha));
            r34MeterFill.gameObject.SetActive(false);
            r34MeterTrack.gameObject.SetActive(false);
        }

        void RenderBendMeter34()
        {
            if (r34MeterFill == null) { Bend34MeterShown = false; return; }
            SurvivalSim sim = game.sim;
            bool show = sim != null && sim.Bend34On && sim.flow == Flow.Playing;
            // NIGHT49 H charge49: with hud47 off the slash sits under the reticle on the grapple row; while the one element shows the BEND there,
            // the slash steps aside (never two at once). With hud47 on it lives in the weapon panel and stays.
            if (show && h49El != null && !hud47Built && Charge49Probe.Shown && Charge49Probe.Source == Charge49Math.Bend) show = false;
            if (r34MeterFill.gameObject.activeSelf != show) { r34MeterFill.gameObject.SetActive(show); r34MeterTrack.gameObject.SetActive(show); }
            Bend34MeterShown = show;
            if (!show) { Bend34MeterNow = -1; return; }
            float m = Mathf.Round(Mathf.Clamp01((float)sim.Bend34Meter) * 50f) / 50f;
            Bend34MeterNow = m;
            if (m != r34ShownMeter) { r34ShownMeter = m; r34MeterFill.fillAmount = m; }
            Color c = m >= 1f ? UiStyle33.Cyan : UiStyle33.CyanDeep;
            if (c != r34ShownMeterC) { r34ShownMeterC = c; r34MeterFill.color = c; }
        }

        // ---- THE BEND COMBO's name (builder B's sim event `bend34Combo {name, guns, kills}`: a named combo inside one bend): posted into hud33's ONE
        // message slot (24 px, cyan, Bend34ComboS; no new box, no new element). Until B's event exists nothing fires and this does nothing. ----
        string h34ComboText;
        /// <summary>For the reels: combos posted, and the last name.</summary>
        public static int Combo34Posts;
        public static string Combo34Last = "";

        void WireCombo34()
        {
            // NIGHT34 addendum (the lead): ARSENAL and QUICKDRAW (builder B's `arsenal34` / `quickdraw34`, the same payload shape with a `name`)
            // post through the same slot; bend34Combo keeps its own event so GG's combo sound hook stays separate.
            Action<object> post = o =>
            {
                string name = o as string;
                if (name == null && o != null)
                {
                    // the payload's `name` (a field or a property): read by name, so this file does not depend on B's event type
                    Type t = o.GetType();
                    var f = t.GetField("name");
                    if (f != null) name = f.GetValue(o) as string;
                    else { var p = t.GetProperty("name"); if (p != null) name = p.GetValue(o) as string; }
                }
                if (string.IsNullOrEmpty(name)) return;
                h34ComboText = name;
                Combo34Posts++;
                Combo34Last = name;
                Post33(name, UnityTuning.Bend34ComboS);
            };
            game.events.On("bend34Combo", post);
            game.events.On("arsenal34", post);
            game.events.On("quickdraw34", post);
        }

        /// <summary>For the reels: the rail as one line ("revolver | [SHOTGUN] | rocket COOLING ..."; the hand in brackets). Allocates: reels only.</summary>
        public static string Rail34Describe()
        {
            Ui ui = rail34Ui;
            if (ui == null || !Rail34Live) return "rail off";
            var sb = new StringBuilder();
            for (int i = 0; i < ui.r34Slots.Count; i++)
            {
                Rail34Slot s = ui.r34Slots[i];
                if (s.id == null || !s.rt.gameObject.activeSelf) continue;
                if (sb.Length > 0) sb.Append(" | ");
                sb.Append(i == Rail34HandSlot ? "[" + s.id.ToUpperInvariant() + "]" : s.id);
                if (s.state == (int)Slot30.Cooling) sb.Append(" COOLING");
            }
            return sb.ToString();
        }
    }
}
