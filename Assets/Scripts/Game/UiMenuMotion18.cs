// NIGHT18 U3-31.7 (builder N): the menus' motion rules, pure (MenuMotion18Tests), read the way the reel measures them
// (Tools/feel-schema.md M9: an element's first visible frame is a pop-in at 95-105 % of its rest scale, >= 95 % of its rest
// alpha and < 40 px from rest; an entrance is whole in 90-300 ms). The views (UiMenus, UiSettings, UiResults, UiOnboarding)
// call these with FeelAB on; off, they keep NIGHT17's path.
// - A screen's parts (head, run line, tabs, footer, BACK) enter like its items: ease-out-back over ThemeInTime from
//   ThemeEnterDistance px down-left along the 12 deg slant, fading in, MenuEnterStagger apart.
// - A screen's INK sheet fades from the sheet shown before it to its own alpha (MenuSheetFade18, ease-out-cubic).
// - The focused item or settings row slides along the slant (right, 12 deg up), ease-out-back over MenuFocusTime18.
// - RESULTS' grade flash: a white-hot copy of the grade's plate, shrinking from TallyFlashFrom18 to TallyFlashTo18 as it fades.
using System;
using UnityEngine;

namespace Skygrind.Game
{
    public static class MenuMotion18
    {
        /// <summary>tan(12 deg): the slant's rise per px of run.</summary>
        public static readonly float SlantK = (float)Math.Tan(UnityTuning.ThemeSlantDeg * Math.PI / 180.0);

        /// <summary>A part's entrance at `age` s: the eased value (ease-out-back, overshooting past 1).</summary>
        public static float Enter(double age) => age <= 0 ? 0f : UiTween.OutBack(age / UnityTuning.ThemeInTime);

        /// <summary>The wait before item / part `index` enters (s): N18 15 ms apart capped at 50 ms (whole by ~200 ms); NIGHT17
        /// MenuEnterStagger apart, unbounded.</summary>
        public static double Wait(int index, bool n18) =>
            n18 ? Math.Min(index * UnityTuning.MenuEnterStagger18, UnityTuning.MenuEnterSpread18) : index * UnityTuning.MenuEnterStagger;

        /// <summary>Round 3: a SETTINGS row's wait in its tab's cascade (s).</summary>
        public static double RowWait(int index) => index * UnityTuning.SettingsRowStagger18;

        /// <summary>Round 3: a SETTINGS row's entrance at `age` (ease-out-back over SettingsRowIn18).</summary>
        public static float RowEnter(double age) => age <= 0 ? 0f : UiTween.OutBack(age / UnityTuning.SettingsRowIn18);

        /// <summary>Round 3: on a screen with a slab (PAUSED, RESULTS), item / part `index`'s wait: after the slab's lead.</summary>
        public static double SlabWait(int index) => UnityTuning.MenuSlabLead18 + index * UnityTuning.MenuCascade18;

        /// <summary>Round 3: the slab's offset from rest at `age`: MenuSlabEnter18 px down-left along the slant, ease-out-back.</summary>
        public static Vector2 SlabOffset(double age)
        {
            float d = (1f - (age <= 0 ? 0f : UiTween.OutBack(age / UnityTuning.MenuSlabIn18))) * UnityTuning.MenuSlabEnter18;
            return new Vector2(-d, -d * SlantK);
        }

        /// <summary>A part's offset at `age` from `distance` px (MODS' cards come from farther).</summary>
        public static Vector2 Offset(double age, float distance)
        {
            float d = (1f - Enter(age)) * distance;
            return new Vector2(-d, -d * SlantK);
        }

        /// <summary>A part's alpha at `age` (0 .. 1).</summary>
        public static float Alpha(double age) => Mathf.Clamp01(Enter(age));

        /// <summary>A part's offset from its rest at `age`: down-left along the slant, 0 at rest.</summary>
        public static Vector2 Offset(double age)
        {
            float d = (1f - Enter(age)) * UnityTuning.ThemeEnterDistance;
            return new Vector2(-d, -d * SlantK);
        }

        /// <summary>M9's pop-in test on a first frame: scale and alpha as ratios of rest, the distance from rest in px.</summary>
        public static bool PopIn(float scaleRatio, float alphaRatio, float px) =>
            scaleRatio >= 0.95f && scaleRatio <= 1.05f && alphaRatio >= 0.95f && px < 40f;

        /// <summary>A sheet's alpha `age` s after its screen showed, fading from `from` to `rest`.</summary>
        public static float SheetAlpha(double age, float from, float rest) =>
            from + (rest - from) * UiTween.OutCubic(age / UnityTuning.MenuSheetFade18);

        /// <summary>Where a sheet's fade starts: the sheet shown before it (capped at its own rest), or 0.</summary>
        public static float SheetFrom(float shownBefore, float rest) => Mathf.Clamp(shownBefore, 0f, rest);

        /// <summary>The focus nudge at `lit` (0 .. 1, linear in time over MenuFocusTime18): along the slant, eased out-back.</summary>
        public static Vector2 FocusNudge(float lit, float nudge)
        {
            float e = lit <= 0 ? 0f : UiTween.OutBack(lit) * nudge;
            return new Vector2(e, e * SlantK);
        }

        /// <summary>RESULTS' grade flash `age` s after the grade lands: its scale (x the plate) and alpha (x TallyFlashAlpha18).</summary>
        public static float GradeFlashScale(double age) =>
            Mathf.LerpUnclamped(UnityTuning.TallyFlashFrom18, UnityTuning.TallyFlashTo18, UiTween.OutCubic(age / UnityTuning.TallyFlashTime18));

        public static float GradeFlashAlpha(double age) => age >= UnityTuning.TallyFlashTime18 ? 0f : 1f - UiTween.InCubic(age / UnityTuning.TallyFlashTime18);
    }
}
