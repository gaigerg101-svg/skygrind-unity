// NIGHT52 builder U, hud52 round 11 (the lead, from GG's "looks so AI generated" for the rest of the HUD): with hud52 on, the top of the HUD takes the
// health and weapon blocks' language (UiHud52.cs: a hard-edged ink plate leaning with the display face, a hard offset shadow, a lit rule on its top
// edge, an accent blade on its outer edge; bone and cyan, red-orange only for danger):
//   - THE COUNTER (top-left, counter43 + hud47's "N LEFT"): on a plate; its accent the plate's outer (left) blade in counter43's own colour
//     (red-orange: enemies; cyan at CLEAR), counter43's small accent hidden; hud47's 3 px drain underline thickened to a Hud52CLineH bar inside the
//     plate (the health bar's family).
//   - THE SCORE (top-right): BONE, not style43's gold (yellow is the parry's token, UnityTuning.Look49ParryHex: the score must not read as
//     "punch it"), on a mirrored plate (accent on the right edge, the weapon panel's lean) fitted to the digits.
//   - THE ROOM TITLE (top-centre, hud33's message slot: room names, DOOR OPEN, a bend's name, a danger line): on a smaller plate of the family
//     (Hud52SShadowPx / SEdgeH / SAccentW) in place of the soft scrim, fitted to the line, fading with it; the accent in the line's role colour.
// Every text keeps its size (the counter 60 / 36 px, the score 32 px, the slot 36 px at 1080p: all >= 16); every plate sits in the top band, far
// from the reticle's 18 px box. Off (the row off, -skygrindOff52) nothing here is built; the a152 reel's Hud52Probe.ForceOff restores NIGHT51's
// look in the same session (the gold, the thin line, the scrim, counter43's accent). Numbers: UnityTuning.U52.cs.
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>hud52 round 11's statics for the a152 reel.</summary>
    public static class Top52Probe
    {
        public static bool CounterBuilt, PointsBuilt, SlotBuilt;
        /// <summary>This frame's plates (the reel reads their screen rects; null: not built).</summary>
        public static Image CounterPlate, PointsPlate, SlotPlate;
        public static Color CounterAccent, CounterLine, PointsColor, SlotAccent, SlotText;
        /// <summary>The texts' sizes on screen (px at 1080p: font size x their root's scale).</summary>
        public static float CounterNumPx, CounterWordPx, PointsPx, SlotPx, SlotAlpha;
    }

    public sealed partial class Ui
    {
        Image t52CShadow, t52CPlate, t52CEdge, t52CAccent;
        Image t52PShadow, t52PPlate, t52PEdge, t52PAccent;
        Image t52SShadow, t52SPlate, t52SEdge, t52SAccent;
        float t52CLineH0 = -1f, t52PFitPx = -1f, t52SFitPx = -1f, t52SAlpha = -1f;
        string t52PFit, t52SFit;
        Color t52PGold, t52SRole;
        bool t52Shown = true;

        /// <summary>From BuildHud52 (hud52 on, the health panel built): each top element that exists gets its plate.</summary>
        void BuildTop52()
        {
            if (c43Root != null && c47Built && c47Line != null && c47LineTrack != null)
            {
                Panel52(c43Root, new Vector2(0, 1), 0, 1, -1, 0, false, out t52CShadow, out t52CPlate, out t52CEdge, out t52CAccent);
                t52CLineH0 = c47Line.rectTransform.sizeDelta.y;
                Counter52Fit(true);
                Top52Probe.CounterBuilt = true;
                Top52Probe.CounterPlate = t52CPlate;
            }
            if (h33Points != null)
            {
                t52PGold = h33Points.Color; // style43's gold (bone without style43): NIGHT51's, restored on the reel's OFF frame
                Panel52(h33Points.rt, new Vector2(1, 1), -1, 0, -1, 0, true, out t52PShadow, out t52PPlate, out t52PEdge, out t52PAccent);
                Top52Probe.PointsBuilt = true;
                Top52Probe.PointsPlate = t52PPlate;
            }
            if (h33Slot != null && h33SlotScrim != null)
            {
                Panel52(h33Slot.rt, new Vector2(0.5f, 1), -1, 1, -1, 0, false, UnityTuning.Hud52SShadowPx, UnityTuning.Hud52SEdgeH, UnityTuning.Hud52SAccentW,
                    out t52SShadow, out t52SPlate, out t52SEdge, out t52SAccent);
                Top52Probe.SlotBuilt = true;
                Top52Probe.SlotPlate = t52SPlate;
            }
            t52Shown = !H52Live; // the first Top52 applies the state
        }

        /// <summary>The counter's plate round its count, its widest word and its drain line (thickened when `thick`, hud47's height back otherwise).</summary>
        void Counter52Fit(bool thick)
        {
            RectTransform ln = c47Line.rectTransform, tr = c47LineTrack.rectTransform;
            float lineH = thick ? UnityTuning.Hud52CLineH : t52CLineH0;
            ln.sizeDelta = new Vector2(ln.sizeDelta.x, lineH);
            tr.sizeDelta = new Vector2(tr.sizeDelta.x, lineH);
            if (!thick) return;
            float wordW = 0f;
            for (int n = 0; n <= 2; n++) wordW = Mathf.Max(wordW, c43Label.main.GetPreferredValues(Hud47Math.CounterWord(n)).x); // CLEAR, LAST ONE, LEFT
            float right = Mathf.Max(ln.anchoredPosition.x + ln.sizeDelta.x, c43Label.rt.anchoredPosition.x + wordW);
            float numPx = c43Num.main.fontSize;
            float x0 = -UnityTuning.Hud52CPadL, x1 = right + UnityTuning.Hud52CPadR;
            float y1 = -numPx * (1f - UnityTuning.Hud52NumCapShare) + UnityTuning.Hud52CPadT; // the count's glyph top (bottom-aligned digits) + the pad
            float y0 = ln.anchoredPosition.y - lineH - UnityTuning.Hud52CPadB;
            Place52(t52CShadow, t52CPlate, t52CEdge, t52CAccent, x0, x1, y0, y1, false, UnityTuning.Hud52ShadowPx, UnityTuning.Hud52EdgeH, UnityTuning.Hud52AccentW);
        }

        /// <summary>Every frame the HUD draws, after the slot (RenderHud33): the plates' state, fit and colours.</summary>
        void Top52()
        {
            bool live = H52Live;
            if (live != t52Shown)
            {
                t52Shown = live;
                foreach (Image im in new[] { t52CShadow, t52CPlate, t52CEdge, t52CAccent, t52PShadow, t52PPlate, t52PEdge, t52PAccent, t52SShadow, t52SPlate, t52SEdge, t52SAccent })
                    if (im != null) im.gameObject.SetActive(live);
                if (t52CPlate != null)
                {
                    c43Accent.gameObject.SetActive(!live); // the plate's blade is the counter's accent now
                    Counter52Fit(live);
                }
                if (t52PPlate != null)
                {
                    h33Points.Color = live ? UiStyle33.Bone : t52PGold;
                    t52PFit = null;
                }
                if (t52SPlate != null)
                {
                    h33SlotScrim.enabled = !live; // the plate replaces the soft wash
                    t52SFit = null;
                    t52SAlpha = -1f;
                }
            }
            if (!live) return;
            Color bone = UiStyle33.Bone, rest = UiTheme.A(bone, UnityTuning.Hud52EdgeRestAlpha);

            // the counter: its accent in counter43's colour (red-orange; cyan at CLEAR), the rule bone
            if (t52CPlate != null)
            {
                Color ac = c43Accent.color;
                if (t52CAccent.color != ac) t52CAccent.color = ac;
                if (t52CEdge.color != rest) t52CEdge.color = rest;
                Top52Probe.CounterAccent = ac;
                Top52Probe.CounterLine = c47Line.color;
                float sc = c43Root.localScale.y;
                Top52Probe.CounterNumPx = c43Num.main.fontSize * sc;
                Top52Probe.CounterWordPx = c43Label.main.fontSize * sc;
            }

            // the score: bone on a mirrored plate fitted to its digits (measured when they change)
            if (t52PPlate != null)
            {
                string s = h33Points.Shown;
                float px = h33Points.main.fontSize;
                if (!string.Equals(s, t52PFit, System.StringComparison.Ordinal) || px != t52PFitPx)
                {
                    t52PFit = s;
                    t52PFitPx = px;
                    float tw = s.Length > 0 ? h33Points.main.GetPreferredValues(s).x : 0f;
                    float x0 = -(tw + UnityTuning.Hud52PPadL), x1 = UnityTuning.Hud52PPadR;
                    float y1 = -px * UnityTuning.Hud52PCapTop + UnityTuning.Hud52PPadT, y0 = -px - UnityTuning.Hud52PPadB;
                    Place52(t52PShadow, t52PPlate, t52PEdge, t52PAccent, x0, x1, y0, y1, true, UnityTuning.Hud52ShadowPx, UnityTuning.Hud52EdgeH, UnityTuning.Hud52AccentW);
                }
                if (t52PEdge.color != rest) t52PEdge.color = rest;
                if (t52PAccent.color != bone) t52PAccent.color = bone;
                Top52Probe.PointsColor = h33Points.Color;
                Top52Probe.PointsPx = px;
            }

            // the room title: the plate fitted to the line, fading with it; the accent in the line's role
            if (t52SPlate != null)
            {
                string s = h33Slot.Shown;
                float px = h33Slot.main.fontSize, a = h33Slot.Alpha;
                if (s.Length > 0 && (!string.Equals(s, t52SFit, System.StringComparison.Ordinal) || px != t52SFitPx))
                {
                    t52SFit = s;
                    t52SFitPx = px;
                    float tw = Mathf.Min(h33Slot.main.GetPreferredValues(s).x, h33Slot.rt.sizeDelta.x);
                    float x0 = -tw * 0.5f - UnityTuning.Hud52SPadX, x1 = tw * 0.5f + UnityTuning.Hud52SPadX;
                    float y1 = -px * UnityTuning.Hud52SCapTop + UnityTuning.Hud52SPadT, y0 = -px * UnityTuning.Hud52SBase - UnityTuning.Hud52SPadB;
                    Place52(t52SShadow, t52SPlate, t52SEdge, t52SAccent, x0, x1, y0, y1, false, UnityTuning.Hud52SShadowPx, UnityTuning.Hud52SEdgeH, UnityTuning.Hud52SAccentW);
                }
                Color role = h33Slot.Color;
                if (s.Length == 0) a = 0f;
                if (a != t52SAlpha || role != t52SRole)
                {
                    t52SAlpha = a;
                    t52SRole = role;
                    Color ink = Ink52;
                    t52SShadow.color = UiTheme.A(ink, UnityTuning.Hud52ShadowAlpha * a);
                    t52SPlate.color = UiTheme.A(ink, UnityTuning.Hud52PlateAlpha * a);
                    t52SEdge.color = UiTheme.A(bone, UnityTuning.Hud52EdgeRestAlpha * a);
                    t52SAccent.color = UiTheme.A(role, a);
                }
                Top52Probe.SlotAccent = t52SAccent.color;
                Top52Probe.SlotText = role;
                Top52Probe.SlotPx = px;
                Top52Probe.SlotAlpha = a;
            }
        }
    }
}
