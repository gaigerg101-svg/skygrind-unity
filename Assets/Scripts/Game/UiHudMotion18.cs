// NIGHT18 U3-31.6 (builder H): HUD motion and the kill marker, measured (NIGHT18.md M9, M2's marker half).
// - HudEntrance18: the one entrance every HUD element that can appear takes while FeelAB is on (banners, pops, callouts,
//   chips, the markers, the round's CLEAR): it grows from Hud18EnterFrom with an overshoot (UiTween.PopIn), fades in over
//   Hud18EnterAlphaTime and, for the big ones, comes along the 12 deg slant; it is whole in Hud18EnterTime. Its first frame
//   is drawn at FirstAge (one 60 Hz frame at most), so that frame is never a pop-in at any frame rate. Exits fade only.
// - KillMarker18: the kill confirm at the crosshair (GG's KILL MARKER switch ON): white, four thick wedges (the hit marker is
//   four thin arms), drawn on the kill's own frame, a pop past its size, gone in Hud18KillMarkerTime.
// FeelAB off: NIGHT17's path everywhere (the callers branch; nothing here runs).
using System;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        // --- the hit marker's grow (Ui.cs draws it) -------------------------------------------------------------------------
        double markerGrowAge = double.MaxValue;
        float markerGrowFrom = UnityTuning.Hud18HitMarkerFrom, markerShownGrow = 1;

        // Round 1's picks ("the hit X barely visible"): with FeelAB on each arm is Hud18HitMarkerWidth wide and
        // Hud18HitMarkerLength x long over an INK underlay Hud18HitMarkerOutline px round it (drawn under it, its twin in place,
        // scale and alpha); off: hidden.
        readonly UnityEngine.UI.Image[] markerUnder18 = new UnityEngine.UI.Image[4];
        bool markerUnderShown18;

        void BuildHitMarker18()
        {
            for (int i = 0; i < 4; i++)
            {
                var u = Img(sight, $"Hit marker {i} (underlay)", UiTheme.InkSolid, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                u.transform.SetSiblingIndex(markerImg[i].transform.GetSiblingIndex()); // just under its arm
                u.enabled = false;
                markerUnder18[i] = u;
            }
        }

        /// <summary>Arm `i`'s underlay this frame (on: drawn under the arm at its place, size + 2 x outline, its rotation, scale and alpha).</summary>
        void HitUnder18(int i, bool on, Vector2 at, Vector2 size, Quaternion rot, Vector3 scale, float alpha)
        {
            UnityEngine.UI.Image u = markerUnder18[i];
            if (u.enabled != on) u.enabled = on;
            if (!on) return;
            RectTransform rt = u.rectTransform;
            rt.anchoredPosition = at;
            rt.sizeDelta = size + new Vector2(2 * UnityTuning.Hud18HitMarkerOutline, 2 * UnityTuning.Hud18HitMarkerOutline);
            rt.localRotation = rot;
            rt.localScale = scale;
            u.color = UiTheme.A(UiTheme.InkSolid, alpha);
        }

        // --- the banner, EXPOSED, the buy prompt, the lock brackets, the screen flashes (Ui.cs calls these) ----------------
        double bannerEnterAge = double.MaxValue, exposedEnterAge = double.MaxValue, promptEnterAge = double.MaxValue, lockEnterAge = double.MaxValue;
        double hurtFlashAge = double.MaxValue;
        Vector2 bannerHome18, exposedHome18;
        bool hudHomes18;
        float bannerShownScale18 = 1, exposedShownScale18 = 1, promptShownScale18 = 1, exposedShownAlpha18 = 1, promptShownAlpha18 = 1;
        Vector2 bannerShownOff18, exposedShownOff18;
        bool exposedWasOn18, promptWasOn18;
        CanvasGroup promptGroup18;

        void HudHomes18()
        {
            if (hudHomes18) return;
            hudHomes18 = true;
            bannerHome18 = banner.rectTransform.anchoredPosition;
            exposedHome18 = exposed.rectTransform.anchoredPosition;
            promptGroup18 = prompt.gameObject.AddComponent<CanvasGroup>(); // its text, its hold bar and fill fade together
            promptGroup18.interactable = promptGroup18.blocksRaycasts = false;
        }

        /// <summary>
        /// A banner (the round's objectives, a zone's name, a gate) enters along the slant: it grows from Hud18EnterFrom,
        /// fades in and slides ThemeEnterDistance px (FeelAB on; each new banner enters again). Returns `ba` x its fade-in.
        /// </summary>
        float Banner18(double frameDt, float ba)
        {
            HudHomes18();
            float s = 1;
            Vector2 off = default;
            if (FeelAB.On && bannerLeft > 0 && !HudEntrance18.Done(bannerEnterAge))
            {
                bannerEnterAge = HudEntrance18.Step(bannerEnterAge, frameDt);
                s = HudEntrance18.Scale(bannerEnterAge);
                off = HudEntrance18.Offset(bannerEnterAge, UnityTuning.ThemeEnterDistance);
                ba *= HudEntrance18.Alpha(bannerEnterAge);
            }
            if (s != bannerShownScale18)
            {
                bannerShownScale18 = s;
                banner.rectTransform.localScale = new Vector3(s, s, 1);
            }
            if (off != bannerShownOff18)
            {
                bannerShownOff18 = off;
                banner.rectTransform.anchoredPosition = bannerHome18 + off;
            }
            return ba;
        }

        /// <summary>EXPOSED enters (grows and fades in) when the director calls it (FeelAB on); it leaves at once, as before.</summary>
        void Exposed18(double frameDt, bool on)
        {
            HudHomes18();
            if (on && !exposedWasOn18) exposedEnterAge = 0;
            exposedWasOn18 = on;
            float s = 1, a = 1;
            Vector2 off = default;
            if (FeelAB.On && on && !HudEntrance18.Done(exposedEnterAge))
            {
                exposedEnterAge = HudEntrance18.Step(exposedEnterAge, frameDt);
                s = HudEntrance18.Scale(exposedEnterAge);
                a = HudEntrance18.Alpha(exposedEnterAge);
                off = HudEntrance18.Offset(exposedEnterAge, UnityTuning.Hud18SmallSlide);
            }
            if (s != exposedShownScale18)
            {
                exposedShownScale18 = s;
                exposed.rectTransform.localScale = new Vector3(s, s, 1);
            }
            if (a != exposedShownAlpha18)
            {
                exposedShownAlpha18 = a;
                exposed.alpha = a;
            }
            if (off != exposedShownOff18)
            {
                exposedShownOff18 = off;
                exposed.rectTransform.anchoredPosition = exposedHome18 + off;
            }
        }

        /// <summary>The buy prompt enters (grows and fades in, its bar with it) when a buy spot comes in reach (FeelAB on).</summary>
        void Prompt18(double frameDt, bool on)
        {
            HudHomes18();
            if (on && !promptWasOn18) promptEnterAge = 0;
            promptWasOn18 = on;
            float s = 1, a = 1;
            if (FeelAB.On && on && !HudEntrance18.Done(promptEnterAge))
            {
                promptEnterAge = HudEntrance18.Step(promptEnterAge, frameDt);
                s = HudEntrance18.Scale(promptEnterAge);
                a = HudEntrance18.Alpha(promptEnterAge);
            }
            if (s != promptShownScale18)
            {
                promptShownScale18 = s;
                prompt.rectTransform.localScale = new Vector3(s, s, 1);
            }
            if (a != promptShownAlpha18)
            {
                promptShownAlpha18 = a;
                promptGroup18.alpha = a;
            }
        }

        /// <summary>The lock brackets fade in over their first frames (their size is the target's; FeelAB on). x the pulse's alpha.</summary>
        float Lock18(double frameDt, bool started)
        {
            if (started) lockEnterAge = 0;
            if (!FeelAB.On || HudEntrance18.Done(lockEnterAge)) return 1f;
            lockEnterAge = HudEntrance18.Step(lockEnterAge, frameDt);
            return HudEntrance18.Alpha(lockEnterAge);
        }

        /// <summary>A screen flash's attack (FeelAB on): it swells over Hud18FlashAttack (ease-in) instead of appearing whole.</summary>
        static float FlashAttack18(double age) => FeelAB.On ? UiTween.InCubic(age / UnityTuning.Hud18FlashAttack) : 1f;
    }

    /// <summary>NIGHT18 U3-31.6: a HUD element's entrance (pure; HudMotionTests).</summary>
    public static class HudEntrance18
    {
        /// <summary>The age an entrance that starts this frame is drawn at: one 60 Hz frame at most.</summary>
        public static double FirstAge(double frameDt) => Math.Min(Math.Max(0, frameDt), UnityTuning.Hud18EnterFirstStep);

        /// <summary>The age to draw this frame: an entrance that started this frame (age 0) is drawn at FirstAge, else it steps.</summary>
        public static double Step(double age, double frameDt) => age <= 0 ? FirstAge(frameDt) : age + frameDt;

        /// <summary>x its rest scale `age` s in: Hud18EnterFrom -> Hud18EnterPeak -> 1 (UiTween.PopIn).</summary>
        public static float Scale(double age) =>
            UiTween.PopIn(age / UnityTuning.Hud18EnterTime, UnityTuning.Hud18EnterFrom, UnityTuning.Hud18EnterPeak, UnityTuning.Hud18EnterPeakAt);

        /// <summary>x its rest alpha `age` s in (ease-out cubic over Hud18EnterAlphaTime).</summary>
        public static float Alpha(double age) => UiTween.OutCubic(age / UnityTuning.Hud18EnterAlphaTime);

        /// <summary>A fade-only entrance's alpha (a band or a streak the reel sees whole: its fill sweep is invisible to it).</summary>
        public static float Fade(double age) => UiTween.OutCubic(age / UnityTuning.Hud18FadeInTime);

        /// <summary>The share of its slide still to go (1 -> 0, ease-out cubic over Hud18EnterTime).</summary>
        public static float Slide(double age) => 1f - UiTween.OutCubic(age / UnityTuning.Hud18EnterTime);

        /// <summary>The theme's slant direction x `px` (an entrance comes from -Along: down-left, moving up-right).</summary>
        public static Vector2 Along(float px)
        {
            float rad = UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * px;
        }

        /// <summary>The offset from its home `age` s in, for a slide of `px` along the slant.</summary>
        public static Vector2 Offset(double age, float px) => -Along(px * Slide(age));

        /// <summary>An exit's alpha `t` s into it (1 -> 0, ease-in over Hud18ExitTime).</summary>
        public static float ExitAlpha(double t) => 1f - UiTween.InCubic(t / UnityTuning.Hud18ExitTime);

        public static bool Done(double age) => age >= UnityTuning.Hud18EnterTime;

        /// <summary>M9's rule (Tools/feel-schema.md): a first frame at >= 0.95 of rest scale and alpha and under 40 px from rest.</summary>
        public static bool PopIn(float scale, float alpha, float px) => scale >= 0.95f && scale <= 1.05f && alpha >= 0.95f && px < 40f; // round 3: above 1.05x is an entrance (a slam)
    }

    /// <summary>
    /// NIGHT18 U3-31.9 backlog 5 (builder H): HUD STYLE ATTACHED (hud.js :819-826; settings.hudStyle, the web's default): the
    /// weapon block rides the viewmodel's sway + bob at viewmodel.hudAttach x hud.attachPxPerMeter px per m, and rolls with
    /// its sway roll x hudAttach. The viewmodel writes its offset each frame (Set; viewmodel.js :930-932 hudOffset).
    /// </summary>
    public static class HudAttach18
    {
        /// <summary>The viewmodel's sway + bob (m, camera space, y up) and its sway roll (deg), this frame.</summary>
        public static double X, Y, RollDeg;

        /// <summary>viewmodel.js :930-932: the viewmodel's hook (Viewmodel.cs, once a frame).</summary>
        public static void Set(double x, double y, double rollDeg)
        {
            X = x;
            Y = y;
            RollDeg = rollDeg;
        }

        public static bool Attached(string hudStyle) => hudStyle == "attached";

        /// <summary>The block's offset in uGUI px (y up), at 0.1 px.</summary>
        public static Vector2 Offset(double x, double y, double hudAttach, double pxPerMeter)
        {
            double k = hudAttach * pxPerMeter;
            return new Vector2((float)(Math.Round(x * k * 10) / 10), (float)(Math.Round(y * k * 10) / 10));
        }

        /// <summary>The block's z rotation in uGUI deg (counter-clockwise +), at 0.01 deg.</summary>
        public static float Roll(double rollDeg, double hudAttach) => (float)(-Math.Round(rollDeg * hudAttach * 100) / 100);
    }

    /// <summary>
    /// NIGHT18 round 2's picks: the big banners' entrance (ROUND N CLEAR, the power-up banner): a SLAM from Hud18SlamFrom down
    /// to 1 (ease-out quad, no bounce) over Hud18SlamTime, fading in from Hud18SlamAlphaFrom (so its first frame is an M9
    /// entrance by its alpha). Drawn at its age, then stepped (its first frame at age 0). Pure; HudMotionTests.
    /// </summary>
    public static class HudSlam18
    {
        public static float Scale(double age) => 1f + (UnityTuning.Hud18SlamFrom - 1f) * (1f - UiTween.OutQuad(age / UnityTuning.Hud18SlamTime));

        public static float Alpha(double age) => UnityTuning.Hud18SlamAlphaFrom + (1f - UnityTuning.Hud18SlamAlphaFrom) * UiTween.OutCubic(age / UnityTuning.Hud18SlamAlphaTime);

        /// <summary>Round 3: white-hot for its first Hud18SlamWhite s (the caller: FLASH INTENSITY 0, its own colour).</summary>
        public static bool White(double age) => age >= 0 && age < UnityTuning.Hud18SlamWhite;
    }

    /// <summary>NIGHT18 U3-31.6: the kill marker's envelope (pure; HudMotionTests). Drawn at its age before the frame's step.</summary>
    public static class KillMarker18
    {
        public static bool Shown(double age) => age >= 0 && age < UnityTuning.Hud18KillMarkerTime;

        /// <summary>Hud18KillMarkerFrom -> Hud18KillMarkerPeak at Hud18KillMarkerPeakAt -> 1 by Hud18KillMarkerIn; then 1.</summary>
        public static float Scale(double age) =>
            UiTween.PopIn(age / UnityTuning.Hud18KillMarkerIn, UnityTuning.Hud18KillMarkerFrom, UnityTuning.Hud18KillMarkerPeak, UnityTuning.Hud18KillMarkerPeakAt / UnityTuning.Hud18KillMarkerIn);

        /// <summary>Whole, then its last Hud18KillMarkerOut s fade out (ease-in) at its rest size.</summary>
        public static float Alpha(double age)
        {
            double outAt = UnityTuning.Hud18KillMarkerTime - UnityTuning.Hud18KillMarkerOut;
            return age <= outAt ? 1f : 1f - UiTween.InCubic((age - outAt) / UnityTuning.Hud18KillMarkerOut);
        }

        /// <summary>deg: Hud18KillMarkerTurn -> 0 (ease-out) over Hud18KillMarkerIn, the X settling square.</summary>
        public static float Turn(double age) => UnityTuning.Hud18KillMarkerTurn * (1f - UiTween.OutCubic(age / UnityTuning.Hud18KillMarkerIn));
    }
}
