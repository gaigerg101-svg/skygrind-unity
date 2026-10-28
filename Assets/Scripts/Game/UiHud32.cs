// NIGHT32 builder G (design.n32.hud32, a view row: no sim bit): the three HUD errors in the NIGHT31 Player frames (Research/2026-10-10-wave32/REVIEW-frames.md).
// 1. "ROUND 1 IN 83" during every boss fight: the survival break line is blank while a boss fight is live (any mode; NIGHT30's hud30 blanked it only in a
//    campaign; the boss reels begin their fights from a SURVIVAL run's break, so the countdown ran over them). Ui.cs's top line asks HideBreakLine32.
// 2. The LT slot's word cut ("CHARGIN"): HudWeapons30's word box (86 px, auto-size 18 -> 16, TRUNCATE) is narrower than "CHARGING" at 16 px bold; with the
//    row on the box is the slot's pitch and the word overflows instead of truncating (never under 16 px: rule 8). BuildSlot30 asks FitWord32.
// 3. A boss's teaching line left up in another fight (the Lantern's "RIDE THE WALL. SHOOT THE CORE." in the Stag and Wyrm reels): the line is cleared only by
//    the Lantern's own line timer (SurvivalSim.BossLines30Step) and by a room change (NIGHT31's DropLesson25Now on roomEnter). The reels start each boss with
//    BossRoomBegin and no roomEnter, so nothing cleared it: a REEL path, not seen in a campaign run (each level is its own run; the Lantern keeps stepping
//    after its death, so its timer clears the line). Not checked by a run. With the row on, the widget also drops a boss line (lessonBegin id "boss") the
//    moment the fight that said it is no longer the live one (dead, or another boss live) or after UnityTuning.Hud32BossLineMax seconds.
// Off: the row off, or -skygrindOff32 (N32Fixes.ViewRowOn): NIGHT31's HUD.
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool hud32On, hud32Read;

        /// <summary>For the reels: the break line blanked by a live boss fight this frame; boss lines dropped by the hud32 rule; the boss whose line is up.</summary>
        public static bool Hud32BreakHidden;
        public static int Hud32BossLineDrops;
        public static string Hud32LineBoss;
        public static string Hud32TopLineNow = "";

        bool Hud32On
        {
            get
            {
                if (!hud32Read && cfg != null)
                {
                    hud32On = N32Fixes.ViewRowOn(cfg, "hud32");
                    hud32Read = true;
                }
                return hud32On;
            }
        }

        /// <summary>The live boss fights as bits (0: none): the campaign bosses and the NIGHT27-31 fights a reel or a boss room starts, and THE CROWN.</summary>
        static int BossMask32(SurvivalSim s)
        {
            if (s == null) return 0;
            int m = 0;
            if (s.lantern27 != null && s.lantern27.Alive) m |= 1;
            if (s.stag31 != null && s.stag31.Alive) m |= 2;
            if (s.wyrm24 != null && s.wyrm24.Alive) m |= 4;
            if (s.loom30 != null && s.loom30.Alive) m |= 8;
            if (s.glacier28 != null && s.glacier28.Alive) m |= 16;
            if (s.glacier27 != null && s.glacier27.Alive) m |= 32;
            if (s.crown32 != null && s.crown32.Alive) m |= 64; // NIGHT32 C: THE CROWN, level 5's boss (SurvivalSim.Crown32.cs)
            return m;
        }

        /// <summary>Fix 1: blank the survival break line ("ROUND 1 IN 83") while a boss fight is live.</summary>
        bool HideBreakLine32(SurvivalRun run)
        {
            bool hide = Hud32On && run.state == SurvivalRun.State.Break && BossMask32(game.sim) != 0;
            Hud32BreakHidden = hide;
            return hide;
        }

        /// <summary>Fix 2: the slot's word box as wide as the slot's pitch, and a word that does not fit overflows instead of being cut.</summary>
        void FitWord32(TextMeshProUGUI word)
        {
            if (!Hud32On || word == null) return;
            word.overflowMode = TextOverflowModes.Overflow;
            word.textWrappingMode = TextWrappingModes.NoWrap;
            word.rectTransform.sizeDelta = new Vector2(UnityTuning.Hud30SlotPitch, word.rectTransform.sizeDelta.y);
        }

        bool hud32LineUp;
        int hud32LineMask;
        double hud32LineAge;

        /// <summary>Fix 3's listeners (from BuildLesson25).</summary>
        void BuildHud32Lesson()
        {
            if (!Hud32On) return;
            game.events.On<LessonEvent>("lessonBegin", e =>
            {
                hud32LineUp = e.id == "boss" && !string.IsNullOrEmpty(e.prompt);
                hud32LineMask = hud32LineUp ? BossMask32(game.sim) : 0;
                hud32LineAge = 0;
                Hud32LineBoss = hud32LineUp ? hud32LineMask.ToString() : null;
            });
            game.events.On<LessonEvent>("lessonClear", _ => { hud32LineUp = false; Hud32LineBoss = null; });
        }

        /// <summary>Fix 3 (from RenderLesson25): a boss line goes the moment its fight is over (no boss live), another boss begins (a fight live now
        /// that was not when the line came up), or after Hud32BossLineMax.</summary>
        void RenderHud32Lesson(double frameDt)
        {
            Hud32TopLineNow = roundText != null ? roundText.text : ""; // the reels' mirror of the top line (any row state)
            if (!hud32LineUp || !Hud32On) return;
            hud32LineAge += frameDt;
            int now = BossMask32(game.sim);
            if (now != 0 && (now & ~hud32LineMask) == 0 && hud32LineAge <= UnityTuning.Hud32BossLineMax) return;
            hud32LineUp = false;
            Hud32LineBoss = null;
            Hud32BossLineDrops++;
            DropLesson25Now();
        }
    }
}
