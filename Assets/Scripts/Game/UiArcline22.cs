// NIGHT22 U3-36.3 (builder M): THE ARCLINE's HUD chip with three looks (RESEARCH R7: cooldown and out-of-range are different looks;
// DECIDE22 D-3: none of them red). UNCOMPILED when written (no Editor in the builder's worktree); the rule it draws is
// Core/Arcline21Chip.cs (Arcline22Chip.Resolve), tested headless.
//   READY      a whole CYAN ring and the word READY: the gadget is bought, off cooldown, and the crosshair has something to hook
//   COOLDOWN   a dim CYAN-DEEP arc that empties clockwise as the 15 s run out, and the seconds left (rounded up) beside it
//   NO TARGET  a broken, dim ring (4 gaps, 40 % alpha) and the words NO TARGET in the muted pale; the crosshair's four arms dim to 40 %
//              alpha (the CanvasRenderer alpha, so the crosshair colour logic in Ui.cs is untouched). Never red.
// Shown only while design.n21.arcline is live and the gadget is owned, in play. The chip enters with the HUD's entrance (HudEntrance18:
// no pop-in) and the looks ease over Arcline22EaseTime. Text is the theme's S18 (18 px at 1080p). Written on change only.
// Wiring (the lead): Ui.BuildN22Hud: `BuildArcline22Chip();` and Ui.RenderN22Hud: `RenderArcline22Chip(frameDt);`.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform arc22Chip, arc22Body;
        CanvasGroup arc22Group;
        UiRing20 arc22Disc, arc22Track, arc22Fill;
        TextMeshProUGUI arc22Label;
        Arcline22Look arc22Look = Arcline22Look.Hidden;
        double arc22Enter = -1, arc22Ease = 1;
        float arc22Alpha = -1, arc22ReticleAlpha = 1, arc22ReticleShown = -1;
        int arc22Seconds = -2;
        float arc22FillShown = -1;

        /// <summary>What the chip draws now, for the reel's scenario (the Ui instance is private to SurvivalGame): the chip was built, its look, the
        /// crosshair arms' alpha (1 whole; 0.4 on NO TARGET).</summary>
        public static bool Arcline22Built;
        public static Arcline22Look Arcline22LookNow = Arcline22Look.Hidden;
        public static float Arcline22ReticleAlphaNow = 1f;
        public static int Arcline22SecondsNow;

        void BuildArcline22Chip()
        {
            Arcline22Built = true;
            float d = UnityTuning.Arcline22ChipD, rOut = d * 0.5f, rIn = rOut - UnityTuning.Arcline22RingW;
            float w = d + UnityTuning.Arcline22LabelGap + UnityTuning.Arcline22LabelW;
            // the root's left edge is the ring's left edge; it sits right and below the crosshair's centre
            arc22Chip = Hud17Rect(sight, "Arcline chip", new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(UnityTuning.Arcline22ChipX, UnityTuning.Arcline22ChipY), new Vector2(w, d));
            arc22Group = arc22Chip.gameObject.AddComponent<CanvasGroup>();
            arc22Group.blocksRaycasts = arc22Group.interactable = false;
            arc22Group.alpha = 0;
            arc22Body = Hud17Rect(arc22Chip, "Ring", new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(d, d));
            arc22Disc = Ring20(arc22Body, "Disc", UiTheme.InkSolid, 0, rIn, UnityTuning.Arcline22ArcSteps);
            arc22Track = Ring20(arc22Body, "Track", UiTheme.InkSoft, rIn, rOut, UnityTuning.Arcline22ArcSteps);
            arc22Fill = Ring20(arc22Body, "Fill", UiTheme.Cyan, rIn, rOut, UnityTuning.Arcline22ArcSteps);
            arc22Label = Hud17Text(arc22Chip, "Label", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.Cyan, UiTheme.BodyBold, TextAlignmentOptions.Left, new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(d + UnityTuning.Arcline22LabelGap, 0), new Vector2(UnityTuning.Arcline22LabelW, UnityTuning.Arcline22LabelH), false);
            arc22Chip.gameObject.SetActive(false);
            game.OnNewRun += () =>
            {
                arc22Look = Arcline22Look.Hidden;
                arc22Seconds = -2;
            };
        }

        void RenderArcline22Chip(double frameDt)
        {
            if (arc22Chip == null) return;
            SurvivalSim sim = game.sim;
            Arcline21 a = sim?.arcline21;
            bool live = a != null && sim.n21.arcline && a.Owned && game.flow == Flow.Playing;
            Arcline21Kind reticle = Arcline21Kind.None;
            if (live)
            {
                var look = new InputFrame { yaw = sim.input.Yaw, pitch = sim.input.Pitch };
                reticle = sim.mover.rope.phase == RopePhase.None ? a.Reticle(look) : Arcline21Kind.Surface; // roped: the hook is out, not "no target"
            }
            Arcline22Look now = live ? Arcline22Chip.Resolve(true, a.YankSteps, reticle) : Arcline22Look.Hidden;

            // the crosshair's arms: 40 % alpha on NO TARGET (the reticle only; eased)
            float wantReticle = live && Arcline22Chip.ReticleDims(true, reticle) ? UnityTuning.Arcline22NoTargetAlpha : 1f;
            arc22ReticleAlpha = Mathf.MoveTowards(arc22ReticleAlpha, wantReticle, (float)(frameDt / UnityTuning.Arcline22EaseTime) * (1f - UnityTuning.Arcline22NoTargetAlpha));
            if (arc22ReticleAlpha != arc22ReticleShown)
            {
                arc22ReticleShown = arc22ReticleAlpha;
                for (int i = 0; i < crossImg.Length; i++) crossImg[i].canvasRenderer.SetAlpha(arc22ReticleAlpha);
            }

            bool show = now != Arcline22Look.Hidden && !Arc24Moved; // NIGHT24 (builder F): with the reticle arc on, the chip's three looks live in the arc (UiReticleArc24.cs); the crosshair dim above stays
            if (arc22Chip.gameObject.activeSelf != show)
            {
                arc22Chip.gameObject.SetActive(show);
                if (show) arc22Enter = 0; // M9: it enters (HudEntrance18), never whole on its first frame
            }
            Arcline22ReticleAlphaNow = arc22ReticleAlpha;
            if (!show)
            {
                arc22Look = Arcline22Look.Hidden;
                Arcline22LookNow = Arcline22Look.Hidden;
                return;
            }
            if (now != arc22Look)
            {
                arc22Look = now;
                arc22Ease = 0;
                arc22Seconds = -2;
                FeelProbe.Event(now == Arcline22Look.Ready ? UnityTuning.Arcline22ProbeReady : now == Arcline22Look.Cooldown ? UnityTuning.Arcline22ProbeCooldown : UnityTuning.Arcline22ProbeNoTarget);
                ApplyArcline22Look(now);
            }
            Arcline22LookNow = now;
            Arcline22SecondsNow = now == Arcline22Look.Cooldown ? Arcline22Chip.Seconds(a.YankSteps, sim.step) : 0;
            if (arc22Ease < 1) arc22Ease = Math.Min(1, arc22Ease + frameDt / UnityTuning.Arcline22EaseTime);

            // the looks' live parts
            float fill;
            if (now == Arcline22Look.Cooldown)
            {
                fill = (float)Arcline22Chip.ArcFill(a.YankShare); // the remaining share, emptying
                int secs = Arcline22Chip.Seconds(a.YankSteps, sim.step);
                if (secs != arc22Seconds)
                {
                    arc22Seconds = secs;
                    arc22Label.SetText("{0:0} s", secs);
                }
            }
            else fill = 1f;
            if (Mathf.Abs(fill - arc22FillShown) > 0.0005f)
            {
                arc22FillShown = fill;
                arc22Fill.Fill = fill;
            }

            // the entrance and the look's ease (the whole chip's alpha)
            float alpha = 1f;
            if (arc22Enter >= 0)
            {
                arc22Enter = HudEntrance18.Step(arc22Enter, frameDt);
                bool done = HudEntrance18.Done(arc22Enter);
                float es = done || game.settings.View.reducedMotion ? 1f : HudEntrance18.Scale(arc22Enter);
                arc22Chip.localScale = new Vector3(es, es, 1);
                alpha = done ? 1f : HudEntrance18.Alpha(arc22Enter);
                if (done) arc22Enter = -1;
            }
            float ease = 0.55f + 0.45f * UiTween.OutCubic((float)arc22Ease); // the new look settles in from 55 % (no pop, no flash)
            alpha *= ease;
            if (alpha != arc22Alpha)
            {
                arc22Alpha = alpha;
                arc22Group.alpha = alpha;
            }
        }

        /// <summary>The look's colours, ring shape and word (the seconds are RenderArcline22Chip's: they change every second).</summary>
        void ApplyArcline22Look(Arcline22Look look)
        {
            switch (look)
            {
                case Arcline22Look.Ready:
                    arc22Fill.color = UiTheme.Cyan;
                    arc22Fill.SetGaps(0, 0, 0);
                    arc22Label.color = UiTheme.Cyan;
                    arc22Label.SetText("READY");
                    break;
                case Arcline22Look.Cooldown:
                    arc22Fill.color = UiTheme.A(UiTheme.CyanDeep, UnityTuning.Arcline22CooldownArcAlpha);
                    arc22Fill.SetGaps(0, 0, 0);
                    arc22Label.color = UiTheme.A(UiTheme.White, UnityTuning.Arcline22CooldownLabelAlpha);
                    break;
                default: // NO TARGET: a broken, dim ring in the muted pale (the boundary role), never red
                    arc22Fill.color = UiTheme.A(UiTheme.Muted, UnityTuning.Arcline22NoTargetRingAlpha);
                    arc22Fill.SetGaps(UnityTuning.Arcline22NoTargetGaps, UnityTuning.Arcline22NoTargetGapDeg, 0);
                    arc22Label.color = UiTheme.A(UiTheme.Muted, UnityTuning.Arcline22NoTargetRingAlpha + 0.2f);
                    arc22Label.SetText("NO TARGET");
                    break;
            }
            arc22FillShown = -1;
        }
    }
}
