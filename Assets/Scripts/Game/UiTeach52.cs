// NIGHT52 builder U, hud52 round 3 (design.n52.hud52, view only; the lead's order from the persona review: GG, OWNER.md, "way too much
// instruction at the start of the level"; every n52 frame showed the room's lesson card, e.g. "Q: KESTREL far, GAUNTLET close", up for the whole
// room). With hud52 on, the teach43 card (UiTeach43.cs) becomes a CHIP Teach52ChipAfterS after it opened, or at the first player kill since it
// opened, whichever is first: its rule line fades out, the plate closes up to the verb line, and the card scales to Teach52ChipScale (never under
// Teach52ChipMinPx on screen), eased. A new line opens full again. The lesson's text, its queue and its hold (text36's slot) are untouched.
// Off (the row off, -skygrindOff52, the a152 reel's ForceOff): the card as NIGHT51's. Numbers: UnityTuning.U52.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The teach chip's statics for the a152 reel.</summary>
    public static class Teach52Probe
    {
        /// <summary>The reel's forced kill (a view-only count: no sim event is sent).</summary>
        public static bool ForceKill;
        /// <summary>This frame: the chip's share (0 full .. 1 chip), why it shrank ("6 s" / "kill" / ""), the verb line's px on screen.</summary>
        public static float Chip, HeadPx, SubPx;
        /// <summary>Round 9: the chip's own clock, the HUD's time since this line opened (s). The a152 reel measures by it: the reels' HUD clock
        /// steps 1/60 s per recorded frame (FeelClock), so a Player drawing under 60 fps sees less HUD time than real time (n52-r12: no chip at
        /// 8 real s, why ''; that a152 took 42.0 s of wall against 34.6 / 36.4 s in r9 / r8; that this was the cause: not checked).</summary>
        public static float Age;
        public static string Reason = "";
        public static int Chips;
    }

    public sealed partial class Ui
    {
        double t52Age;
        int t52Kills, t52KillsAtOpen;
        float t52K, t52FullH, t52ChipH, t52Scale = 1f;
        bool t52Wired;

        /// <summary>From Teach43Draw when a new line opens the card: the chip's clock and sizes from this line.</summary>
        void Teach52Open()
        {
            if (!h52Built || t43Root == null) return;
            if (!t52Wired)
            {
                t52Wired = true;
                game.events.On<KillEvent>("kill", e => { if (e != null && e.owner == "player") t52Kills++; });
            }
            t52Age = 0;
            t52KillsAtOpen = t52Kills;
            t52K = 0f;
            t52FullH = t43Root.sizeDelta.y;
            t52ChipH = Mathf.Min(t52FullH, t43Head.rt.sizeDelta.y + 2 * UnityTuning.Teach43PadY);
            t52Scale = Mathf.Max(UnityTuning.Teach52ChipScale, UnityTuning.Teach52ChipMinPx / Mathf.Max(1f, t43Head.main.fontSize));
            t43Sub.Alpha = 1f;
            if (t43Sub.Shown.Length > 0 && !t43Sub.rt.gameObject.activeSelf) t43Sub.rt.gameObject.SetActive(true); // a chip's hidden rule line back (Set skips a repeated string)
            Teach52Probe.Reason = "";
            Teach52Probe.Chip = 0f;
            Teach52Probe.Age = 0f;
        }

        /// <summary>From Teach43Draw each frame the card is up, before its pop / close scale is applied: the chip's state, folded into that scale.</summary>
        void Teach52Chip(ref float sx, ref float sy, double frameDt)
        {
            if (!h52Built || t43Root == null || t52FullH <= 0f) return;
            if (!H52Live)
            {
                if (t52K > 0f) { t52K = 0f; Teach52Apply(0f); } // the reel's NIGHT51 frame: the full card back
                return;
            }
            if (Teach52Probe.ForceKill) { Teach52Probe.ForceKill = false; t52Kills++; }
            t52Age += frameDt;
            Teach52Probe.Age = (float)t52Age;
            bool chip = t52Age >= UnityTuning.Teach52ChipAfterS || t52Kills > t52KillsAtOpen;
            if (chip && Teach52Probe.Reason.Length == 0)
            {
                Teach52Probe.Reason = t52Kills > t52KillsAtOpen ? "kill" : "6 s";
                Teach52Probe.Chips++;
            }
            float before = t52K;
            t52K = Mathf.MoveTowards(t52K, chip ? 1f : 0f, (float)(frameDt / UnityTuning.Teach52ChipEaseS));
            float k = UiTween.OutCubic(t52K);
            if (t52K != before) Teach52Apply(k);
            float s = Mathf.Lerp(1f, t52Scale, k);
            sx *= s;
            sy *= s;
            Teach52Probe.Chip = k;
            Teach52Probe.HeadPx = t43Head.main.fontSize * s;
            bool subDrawn = t43Sub.rt.gameObject.activeInHierarchy && t43Sub.Alpha > 0.01f;
            Teach52Probe.SubPx = subDrawn ? t43Sub.main.fontSize * s : 0f; // 0: the rule line is not drawn
        }

        /// <summary>The card at chip share `k`: the rule line's alpha (and, at the chip, the rule line not drawn at all: round 4, n52-r8's 8 s frame
        /// still showed it faint at ~13 px, under rule 8's 16), the plate's height (the verb line's height at 1), the accent with it.</summary>
        void Teach52Apply(float k)
        {
            t43Sub.Alpha = 1f - k;
            bool sub = k < 0.999f && t43Sub.Shown.Length > 0;
            if (t43Sub.rt.gameObject.activeSelf != sub) t43Sub.rt.gameObject.SetActive(sub);
            float h = Mathf.Lerp(t52FullH, t52ChipH, k);
            t43Root.sizeDelta = new Vector2(t43Root.sizeDelta.x, h);
            t43Accent.rectTransform.sizeDelta = new Vector2(UnityTuning.Teach43AccentW, Mathf.Max(1f, h - 2 * UnityTuning.Teach43AccentInset));
            UiKit43.FitScan(t43Scan);
        }
    }
}
