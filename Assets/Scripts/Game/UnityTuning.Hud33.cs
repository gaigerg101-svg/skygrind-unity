// NIGHT33 builder U (design.n33.hud33, a view row: no sim bit): the HUD33 layout and look numbers (CLAUDE.md rule 5: Unity-only, each with a
// comment). Every px is at 1080p (the HUD canvas scales with the height from 1920 x 1080, so a 16:9 screen of any size has the same layout in canvas
// units). Spec: Research/2026-10-11-wave33/U1-hud.md section 6 (the slash motif, no panel fills, a 2 px shadow, bone / cyan / red-orange).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the language (UiStyle33) ---------------------------------------------------------------------------------------------------
        public const uint Hud33BoneHex = 0xE6ECEF; // U1: the neutral, bone white (no violet: pale violet-white stays the boundary's); points are bone, not gold
        public const uint Hud33ShadowHex = 0x0A0E14; // U1: the 2 px drop shadow's colour (legibility without a box)
        public const float Hud33ShadowAlpha = 0.8f; // U1: the shadow's opacity
        public const float Hud33ShadowPx = 2f; // U1: the shadow's offset (right and down), px
        public const float Hud33SlashDeg = 20f; // U1: the one ornament, a tick skewed 20 degrees (tops lean right)
        public const float Hud33TextMin = 16f; // CLAUDE.md rule 8: no HUD text under 16 px at 1080p
        public const float Hud33Big = 64f; // U1 tier 1: the health number
        public const float Hud33Mid = 32f; // U1: the points
        public const float Hud33Name = 24f; // U1 tier 2: the held weapon's name, the message slot
        public const float Hud33Small = 16f; // U1 tier 3: the LT label, the expanded weapon names, the boss name
        public const float Hud33RoundPx = 20f; // U1: round / enemies left, top-left
        public const float Hud33RankPx = 40f; // U1: the style rank's letter
        public const float Hud33Tracking = 6f; // TMP character spacing on caps labels (the weapon name, the LT label, the round line)

        // --- safe margins -----------------------------------------------------------------------------------------------------------------
        public const float Hud33SideMargin = 72f; // U1: left and right
        public const float Hud33EdgeMargin = 54f; // U1 / DESIGN33 U: top and bottom (the checker's floor on every side)
        public const float Hud33MaxWidth = 360f; // U1 rule 2: no element wider, except the boss bar
        public const float Hud33MaxHeight = 80f; // U1 rule 2: no element taller

        // --- health and stamina, bottom-left --------------------------------------------------------------------------------------------
        public const int Hud33HealthTicks = 10; // U1: ten slash ticks, one per 10 % of the max
        public const float Hud33TickW = 6f; // U1: the slash tick's width (6 x 14)
        public const float Hud33TickH = 14f; // U1: the slash tick's height
        public const float Hud33HealthTickPitch = 22f; // ten ticks over 220 px (U1)
        public const float Hud33StaminaH = 4f; // DESIGN33 U: the stamina as thin slash bars under the health ticks
        public const float Hud33StaminaGap = 6f; // px between the two stamina bars
        public const float Hud33StaminaW = 104f; // one stamina bar's width: two fill the 214 px under the ticks
        public const float Hud33RowGap = 6f; // px between the stamina row, the tick row and the number
        public const float Hud33HealthNumW = 200f; // the health number's box (3 digits at 64 px)
        public const float Hud33LowFrac = 0.25f; // U1: the ticks turn red-orange at or under 25 % (a view threshold; the web's low-HP blink keeps its own number)
        public const double Hud33HitFlash = 0.15; // U1: one 150 ms red-orange flash of the ticks per hit taken

        // --- the weapon, bottom-right ----------------------------------------------------------------------------------------------------
        public const float Hud33PipW = 5f; // DESIGN33 U: a 12 px slash pip per owned weapon
        public const float Hud33PipH = 12f; // the pip's height
        public const float Hud33PipPitch = 14f; // px between pips
        public const float Hud33PipHeldW = 7f; // the held weapon's pip is a little wider (shape, not colour alone)
        public const float Hud33WeaponGap = 6f; // px between the pip row, the LT label and the name
        public const float Hud33WeaponBoxW = 320f; // the name / LT label boxes (under the 360 px rule)
        public const double Hud33SwitchExpand = 0.7; // DESIGN33 U: the pip strip expands to names for 700 ms on a switch
        public const float Hud33ListPitch = 20f; // the expanded list's line pitch (16 px names)
        public const float Hud33SwitchSlide = 12f; // U1: the name slides 12 px in on a switch
        public const double Hud33SwitchSlideTime = 0.12; // U1: over 120 ms
        public const string Hud33LtPad = "LT"; // the LT label's prefix on a pad only when the layout gives no button for the alt fire (the live layout's button word is used: Glyphs.Label)
        public const string Hud33LtKey = "RMB"; // its prefix on mouse and keyboard: the alt fire is the right mouse button, hard-wired (CombatReader.cs :131 / :176, not rebindable)

        // --- points and rank, top-right ---------------------------------------------------------------------------------------------------
        public const float Hud33PointsBoxW = 300f; // the points' box (7 digits at 32 px)
        public const float Hud33RankLineW = 160f; // U1: the rank meter, 160 x 3
        public const float Hud33RankLineH = 3f; // U1
        public const float Hud33RankGap = 8f; // px between the points, the rank letter and its line
        public const float Hud33RankBoxEm = 2.4f; // the rank letter's box in ems of its size (room for a two-letter rank such as SS)

        // --- the round line, top-left ---------------------------------------------------------------------------------------------------
        public const int Hud33RunLines = 3; // the survival extras under the round line: the objective, the PROTOCOL's step, exfil (UiRunShape's three parts)
        public const float Hud33RunLinePitch = 22f; // px between them (16 px text)
        public const float Hud33RunLineGap = 6f; // px between the round line and the first extra (and the last and the DOORS OPEN tag)
        public const float Hud33RoundBoxW = 340f; // "ROUND 12 · 24 LEFT" at 20 px fits; under the 360 px rule

        // --- the reticle (centre) -------------------------------------------------------------------------------------------------------
        public const float Hud33Dot = 4f; // U1: the 4 px dot
        public const float Hud33ArmLen = 8f; // U1: four ticks 8 x 2
        public const float Hud33ArmW = 2f; // U1
        public const float Hud33ArmGap = 10f; // U1: the gap from the dot's centre to a tick's inner end
        public const float Hud33ArcR = 9f; // the charge arcs' radius (they close on the dot as the charge fills)
        public const float Hud33ArcW = 2f; // the charge arcs' width
        public const double Hud33HitFlashTime = 0.08; // U1: a hit flashes the ticks for 80 ms
        public const float Hud33HitGrow = 3f; // px the ticks push out on a hit's flash
        public const double Hud33KillTurnIn = 0.12; // U1: a kill rotates the ticks 45 degrees in 120 ms
        public const double Hud33KillHold = 0.18; // then holds the X this long
        public const double Hud33KillTurnOut = 0.12; // and turns back
        public const float Hud33GrappleY = -26f; // DESIGN33 U: the grapple charges as two slash ticks under the reticle (replaces GrapplePips32's diamonds)
        public const float Hud33GrapplePitch = 12f; // px between the two charge ticks
        public const float Hud33GrappleSpentAlpha = 0.25f; // a spent charge's track (its cyan fill rises in it as it comes back)

        // --- the message slot (round 1: a mirror of the banner; round 2: the priority queue) -----------------------------------------
        public const float Hud33SlotY = 54f; // U1: top-centre at the top margin
        public const float Hud33SlotBossY = 132f; // U1: under the boss bar while a boss fights
        public const float Hud33SlotW = 640f; // the slot's box: the boss bar's column (the one HUD line allowed past 360 px: one message at a time); text only, centred
        public const float Hud33BossBarW = 640f; // DESIGN33 U: the boss bar, 640 x 6 at the top centre
        public const float Hud33BossBarH = 6f; // DESIGN33 U
        public const float Hud33BossNameGap = 6f; // px between the boss's name and its bar
        public const double Hud33BossSlideIn = 0.2; // U1: the boss bar slides in over 200 ms
        public const float Hud33BossSlide = 12f; // px it slides down from
        public const float Hud33BossTrackAlpha = 0.25f; // the bar's empty track (bone at this alpha; the fill is red-orange: an enemy's)
        public const float Hud33SlotGapY = 180f; // the slot under Hud29's chase gap bar while it shows (its label at 124 px, its plate 150-164 px from the top)
        public const double Hud33SlotMinHold = 1.2; // DESIGN33 U: each message held at least 1.2 s
        public const double Hud33SlotDropAfter = 3.0; // U1: a queued message older than 3 s is dropped
        public const double Hud33SlotFadeIn = 0.05; // U1: a 3-frame flicker-on (60 fps)
        public const double Hud33SlotFadeOut = 0.12; // U1: 120 ms out
        public const float Hud33ScrimPadX = 64f; // the slot's soft dark wash reaches this far past each end of its text (the lead's n33-r1: a room title on a lit ceiling panel)
        public const float Hud33ScrimH = 76f; // its height (the 24 px line with room for the fall-off above and below; n33-r5: 60 left the glyphs' tops on the bright panel)
        public const float Hud33ScrimAlpha = 0.88f; // its peak opacity (the shadow colour); it falls to 0 at every side, so it is shade, not a box (n33-r5: 0.6 lost to a lit cyan panel)
        public const float Hud33SlotOutline = 0.28f; // the slot text's TMP outline width (SDF units, about 2 px at 24 px), in the shadow colour, opaque
        public const string Hud33DangerExposed = "EXPOSED"; // the danger line while the director marks the player EXPOSED (the NIGHT32 word)
        public const string Hud33DoorLine = "DOOR OPEN"; // the slot's door hint; the distance moves to the world marker on the door
        public const float Hud33DoorMarkUp = 2.4f; // m above the room's exit point where the door marker floats (about a door's lintel; a view number: the exit point is the floor)
        public const float Hud33DoorMarkPx = 20f; // the marker's distance text ("7 m")
        public const float Hud33DoorChevronW = 4f, Hud33DoorChevronH = 14f; // the marker's chevron: two slash ticks meeting at the bottom
        public const float Hud33DoorEdge = 80f; // px: the marker hides when its point is this close to a screen edge (it is a world marker, not an arrow)
        public const float Hud33SlotH = 32f; // one line at 24 px

        // --- the front screens (UiFront33) ----------------------------------------------------------------------------------------------
        public const float Front33TickLead = 26f; // px left of a menu item's left edge to its first focus tick (two cyan slash ticks mark the pad's focus)
        public const float Front33TickPitch = 10f; // px between the two focus ticks
        public const float Front33TickH = 28f; // the focus ticks' height (twice the HUD tick: menu items are 42-75 px type)
        public const float Front33BestGap = 24f; // px from TITLE's lowest item's right edge to the BEST GRADE line beside it (it sat inside the 54 px bottom margin under it)
        public const float Front33BankedY = 430f; // the campaign results' POINTS BANKED line, px above the centre (over the head at 360; under the 54 px top margin)

        // --- canvas ----------------------------------------------------------------------------------------------------------------------
        public const float Hud33HiddenAlpha = 0f; // the NIGHT32 HUD layers' CanvasGroup alpha while hud33 is on (they still run: the reel hooks keep their values)
    }
}
