// NIGHT16 U3-29.3 (the shared commit; the lead's): the feel night's settings rows. The web has none of them: it has no
// impact tiers or slow-mo to switch (its kill / parry hitstop is always on) and no slide limits (design.move is Unity-only),
// so their labels and descriptions live here (CLAUDE.md rule 5); their defaults are design.impact.settings and
// design.move.*.enabled. The builders' own render numbers are in UnityTuning.Impact.cs (I) and UnityTuning.Hands.cs (V).
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // config.ui.settings' ACCESS tab id (EFFECTS INTENSITY, REDUCED MOTION, FLASH INTENSITY live there).
        public const string AccessTabId = "access";

        // HIT FREEZE: OFF freezes nothing (the web's kill / parry hitstop included), LIGHT scales every freeze by
        // design.impact.settings.lightScale, FULL is design.impact's tiers.
        public const string HitFreezeLabel = "HIT FREEZE";
        public const string HitFreezeDesc = "The world stops for a moment on a kill, a headshot, a punch or a parry (LIGHT: half as long)";
        public static readonly (string mode, string label)[] HitFreezeModes =
        {
            (FeelKeys.HitFreezeOff, "OFF"), (FeelKeys.HitFreezeLight, "LIGHT"), (FeelKeys.HitFreezeFull, "FULL"),
        };

        // SLOW-MO: the round's last kill, a perfect parry and a multi-kill slow time for a moment (REDUCED MOTION turns it off).
        public const string SlowMoLabel = "SLOW-MO";
        public const string SlowMoDesc = "Time slows for a moment on the round's last kill, a perfect parry and a multi-kill (off with REDUCED MOTION)";

        // The LAB tab's movement rows: (the settings key, the design.move block, the label, the description). On by default,
        // unlike the LAB's experiments; switching one off plays the web's slide for that rule from the next run.
        public static readonly (string key, string block, string label, string desc)[] MoveRows =
        {
            // {0}: design.move.slideCharge.rechargeTime (GameSettings fills it once: the review's finding 11, it said 1 s)
            (FeelKeys.MoveSlideChargeSetting, "slideCharge", "SLIDE CHARGE", "The slide's speed burst needs a charge, back {0} s after a slide ends (off: every slide bursts)"),
            (FeelKeys.MoveSlideDecaySetting, "slideDecay", "SLIDE DECAY", "A slide on flat ground slows to a run after a moment; downhill still pays (off: it never slows)"),
            (FeelKeys.MoveHopFalloffSetting, "hopFalloff", "HOP FALLOFF", "Chained slide-jumps gain less each time (off: every hop gains x1.2)"),
        };

        // The LAB tab's note, now that its last three rows (the movement rules) are on by default (NIGHT8's said "ALL OFF"):
        // one line under the rows (the review's finding 11: the old three lines sat on the selected row's description).
    }
}
