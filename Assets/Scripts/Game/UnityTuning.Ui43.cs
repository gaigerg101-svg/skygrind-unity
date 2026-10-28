// NIGHT43 builder U1 (view rows design.n43.hud43 / counter43 / teach43 / bossbar43 / screens43; NIGHT43.md U1; GG's G5 / G6 / G7 / G14 / G24):
// the HUD's visual language ("the dusk instrument", Research/2026-10-16-wave43/U1.md section 1) and the rows' numbers. CLAUDE.md rule 5:
// Unity-only numbers (render / UI layout / motion), each with a comment; px at 1080p (the HUD canvas scales with the height). No gameplay number
// lives here: the health, the room's count and the boss's state are the sim's. F1 / A1 read the language tokens (section 1.6 of U1.md).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the language (U1.md section 1) -----------------------------------------------------------------------------------------------------------
        public const float Ui43SlantDeg = 12f; // the blade's lean: the display face's own italic (ThemeSlantDeg), one angle for every NIGHT43 element
        public const uint Ui43SteelHex = 0x8E97B5; // STEEL: a boss that cannot be hurt now (LOCKED); low saturation so it never reads as the player's cyan
        public const float Ui43ScanAlpha = 0.18f; // the scanline overlay: one row in Ui43ScanPitch darker by this share (the PS2 / CRT treatment, PRINCIPLES 7)
        public const int Ui43ScanPitch = 3; // px between scanlines (at 1080p)
        public const float Ui43BracketArm = 10f; // px: a HUD bracket's arm (an L at the top-left and the bottom-right of a framed element)
        public const float Ui43BracketW = 2f; // px: the bracket's line width
        public const float Ui43MutedMix = 0.42f; // MUTED = BONE toward INK by this share (the theme's ThemeMutedMix: one value)
        public const float Ui43SplitPx = 3f; // px: the chroma split's offset (cyan left, red-orange right) on a hero number when the player is hit
        public const double Ui43SplitTime = 0.18; // s: the split fades over this
        public const double Ui43OpenX = 0.08; // s: the CRT pop's first half (X grows from Ui43OpenFrom; ULTRAKILL HudOpenEffect ~0.078 s per axis)
        public const double Ui43OpenY = 0.08; // s: its second half (Y grows)
        public const float Ui43OpenFrom = 0.05f; // the pop's starting scale on each axis (HudOpenEffect's 0.05)
        public const double Ui43CloseY = 0.05; // s: the close: Y collapses first
        public const double Ui43CloseX = 0.05; // s: then X
        public const double Ui43TickTime = 0.15; // s: a number's tick punch (1 -> Ui43TickScale -> 1)
        public const float Ui43TickScale = 1.25f; // the tick punch's peak scale
        public const double Ui43EnterTime = 0.15; // s: one part's staggered entrance on a full screen (ease-out-back)
        public const double Ui43EnterGap = 0.05; // s: between two parts' entrances
        public const float Ui43EnterFrom = 60f; // px: a part enters along the slant from this far down-left
        public const double Ui43BreatheHz = 1.2; // Hz: the only idle loops (low health's bracket, an OPEN boss timer)
        public const double Ui43AfterWait = 0.15; // s: an after-image waits this long after a drop (bosses-web A2: ULTRAKILL's waitForDamage)
        public const double Ui43AfterRate = 5.0; // the after-image drains at (gap x Ui43AfterRate + Ui43AfterBase) per s (ULTRAKILL's numbers)
        public const double Ui43AfterBase = 0.5; // see Ui43AfterRate (a share of the bar per s)

        // --- hud43: the health bar (UiHud33.Hud43.cs), bottom-left ------------------------------------------------------------------------------------
        public const int Hud43Cells = 10; // ten blade cells, one per 10 % of the max (hud33's ten ticks, now a bar)
        public const float Hud43CellW = 15f; // px: a cell's width (round 2: 21 x 18 cells read as plain rectangles in n43-r1; narrow + tall shows the 12 deg lean: 5 px over 15)
        public const float Hud43CellH = 24f; // px: a cell's height
        public const float Hud43CellPitch = 22f; // px between cells' left edges (7 px gaps: each cell its own blade; ten cells = 213 px, the stamina's 214)
        public const float Hud43NumPx = 56f; // the number: the HERO face at the type scale's 56 (hud33 had 64 over the ticks; beside the bar it needs less)
        public const float Hud43NumW = 128f; // px: the number's box (4 digits of the condensed face at 56 px), its text right-aligned to the box's right edge
        public const float Hud43NumGap = 14f; // px from the number's box to the bar
        public const int Hud43NumCap = 9999; // the most the number shows (4 digits: the box; the reels' god mode sets 1e6 health)
        public const float Hud43StaminaGap = 6f; // px from the stamina bars (under the bar) to the bar
        public const float Hud43TrackAlpha = 0.35f; // an empty cell's track (MUTED at this alpha): the lost health is still a shape
        public const float Hud43NotchGap = 3f; // px from the bar's top to a 25 % notch
        public const float Hud43NotchH = 5f; // px: a notch's height
        public const float Hud43BracketPad = 6f; // px: the bracket stands this far outside the bar
        public const double Hud43HitTime = 0.25; // s: the bracket's red-orange pulse on a hit decays over this
        public const double Hud43HealTime = 0.45; // s: the heal's cyan glow on the cells it filled fades over this
        public const float Hud43LowDim = 0.45f; // low health's breathe: the fill and the bracket swing between red-orange at this alpha and full

        // --- style43: style42's meter line as blade cells (UiHud33.Style43.cs, with hud43; round 3) ----------------------------------------------------
        public const int Style43Cells = 8; // cells in the style meter (the band's share; fewer and smaller than health's ten: it is the second read)
        public const float Style43CellW = 16f; // px: a cell's width
        public const float Style43CellH = 10f; // px: a cell's height (style42's line was 3 px; the name row sits under it)
        public const float Style43CellPitch = 20f; // px between cells (8 cells = 156 px: inside style42's 160 px line)
        public const float Style43BracketPad = 5f; // px: its bracket past the cells
        public const float Style43BracketTint = 0.5f; // the bracket takes this share of the cells' colour (cyan / a gain's white / a hit's red-orange)

        // --- the fallReturn43 blink (UiHud33.Fall43.cs, with hud43) ------------------------------------------------------------------------------------
        public const double Fall43Out = 0.06; // s: to dusk (near a cut: the player is already back on the lip when the event fires)
        public const double Fall43Hold = 0.19; // s: held dark (with Fall43Out, the lead's ~0.25 s "out")
        public const double Fall43In = 0.25; // s: back to the game (the lead's ~0.25 s "in")
        public const float Fall43Peak = 0.9f; // the sheet's darkest alpha (DUSK, colors.skyZenith: the HUD reads over it)

        // --- counter43: the locked room's enemies left (UiHud33.Counter43.cs), top-left ---------------------------------------------------------------
        public const float Counter43W = 300f; // px: the counter's box (under hud33's 360 rule)
        public const float Counter43H = 56f; // px: its height (the number + the pips)
        public const float Counter43NumPx = 36f; // the count in the HERO face: small (GG: "small, unobtrusive"), still a number you read at speed
        public const float Counter43NumW = 44f; // px: the count's box (two digits), right-aligned to the label
        public const float Counter43AccentW = 4f; // px: the red-orange blade accent left of the count
        public const float Counter43Gap = 8f; // px between the accent, the count and the label
        public const float Counter43LabelPx = 16f; // "LEFT" / "CLEAR": the label face at the HUD floor
        public const float Counter43LabelW = 120f; // px: the label's box
        public const float Counter43LabelDrop = 14f; // px: the label sits this far down so its baseline meets the count's
        public const int Counter43MaxPips = 12; // pips drawn at most (the number carries the rest)
        public const float Counter43PipW = 5f; // px: a pip's width (a small blade)
        public const float Counter43PipH = 9f; // px: a pip's height
        public const float Counter43PipPitch = 9f; // px between pips
        public const float Counter43PipGap = 5f; // px from the count's bottom to the pip row
        public const double Counter43FlashTime = 0.05; // s: a kill flashes the count white (about 3 frames at 60 fps)
        public const string Counter43LeftWord = "LEFT"; // the label while enemies remain
        public const string Counter43ClearWord = "CLEAR"; // the label at 0 (cyan: the door is the player's)

        // --- teach43: the centre card (UiTeach43.cs) --------------------------------------------------------------------------------------------------
        public const float Teach43Y = 170f; // px: the card's centre above the canvas centre (the crosshair); a two-line card's bottom stays ~116 px up, clear of the crosshair's 160 x 160 box
        public const float Teach43MaxW = 640f; // px: the widest card (the top-centre column's width: hud33's one wide line)
        public const float Teach43MinW = 280f; // px: the narrowest (a two-word verb still reads as a card, not a tag)
        public const float Teach43MinH = 64f; // px: the shortest (one line)
        public const float Teach43PlateAlpha = 0.82f; // the INK plate's opacity (text36's plate was 0.68 and read as a grey box over the pale viewmodel; this one sits over the sky / walls)
        public const float Teach43AccentW = 4f; // px: the cyan blade accent on the plate's left
        public const float Teach43AccentInset = 8f; // px: the accent's inset from the plate's left / top / bottom
        public const float Teach43HeadPx = 40f; // the verb line: the HERO face at 40 px (text36's 32 px body line was the n42-r4 frame)
        public const float Teach43SubPx = 24f; // the rule line: the body face at the type scale's 24
        public const float Teach43HeadMinPx = 28f; // the verb line shrinks (4 px steps) to this at most when it would take more than two lines (round 2: never an ellipsis)
        public const float Teach43SubMinPx = 18f; // the rule line's floor (>= the HUD's 16)
        public const float Teach43PadX = 32f; // px: the plate's side padding
        public const float Teach43PadY = 14f; // px: its top / bottom padding
        public const float Teach43LineGap = 4f; // px between the verb and the rule
        public const float Teach43HeadTracking = 2f; // TMP character spacing on the verb line (the condensed face in caps needs a little air)

        // --- bossbar43 (UiBossBar43.cs) ---------------------------------------------------------------------------------------------------------------
        public const string Bar43LegWord = "LEG"; // "LEG 1 / 3"
        public const float Bar43W = 640f; // px: the bar's width (hud33's boss bar column)
        public const float Bar43H = 96f; // px: the whole block (name 36 + 6 + bar 14 + 6 + meter 4 + 6 + the state row 24)
        public const float Bar43NamePx = 32f; // the boss's name: the HERO face at 32 px (hud33's was 16 px: the boss is the room's subject)
        public const float Bar43LabelPx = 16f; // LEG 1 / 3, LOCKED, OPEN, STAGGER: the label face at the HUD floor
        public const float Bar43HintPx = 18f; // the hint (what hurts it now): the body face
        public const float Bar43LegDrop = 6f; // px: the leg label sits this far down so its baseline meets the name's
        public const float Bar43Gap = 6f; // px between the name, the bar, the meter and the state row
        public const float Bar43BarH = 14f; // px: the bar's height (hud33's was 6: an after-image needs room to read)
        public const float Bar43NotchW = 2f; // px: the lock notch's width
        public const float Bar43NotchOver = 4f; // px it stands past the bar above and below
        public const float Bar43LineH = 4f; // px: the stagger meter / the OPEN timer under the bar
        public const float Bar43TagW = 200f; // px: the state tag's box (left), the hint takes the rest (right)
        public const float Bar43OpenHot = 0.35f; // OPEN: the fill red-orange toward white by this share ...
        public const float Bar43OpenSwing = 0.25f; // ... plus this much more at the breathe's peak (Ui43BreatheHz)
        public const float Bar43HintRestAlpha = 0.7f; // the hint's alpha in the plain fight (full when LOCKED / OPEN: then it is the instruction)
        public const float Bar43SlotY = 164f; // px from the top: hud33's message slot while this bar draws (54 + 96 + 14)
        public const string Bar43LockedWord = "LOCKED"; // the tag while the boss cannot be hurt
        public const string Bar43OpenWord = "OPEN"; // the tag while it takes full damage (+ the seconds left)
        public const string Bar43StaggerWord = "STAGGER"; // the meter's name in the plain fight

        // --- screens43: the backdrop (UiBlur43.cs) and the shop / results panels (UiShop33.Screens43.cs, UiCampaignResults25.Screens43.cs) -------------
        public const int Blur43Levels = 4; // the blur's halvings (from the half-size capture: 1/2 .. 1/16), then the same back up: a strong, smooth blur
        public const float Blur43Offset = 1.5f; // the Kawase taps' spread in texels of each level (1 = the paper's; 1.5 wider, still no ringing)
        public const uint Blur43DuskHex = 0x4A2168; // the backdrop's multiply colour: colors.skyMid, the dusk violet (U1.md 1.2 DUSK)
        public const float Blur43DuskMix = 0.5f; // the multiply: white toward the dusk violet by this share ...
        public const float Blur43Dim = 0.8f; // ... then darkened by this (the game reads as a place behind glass, the panels read over it)
        public const uint Blur43FlatHex = 0x100A2C; // no capture (the shader missing, the readback failed, or its first frame): colors.skyZenith ...
        public const float Blur43FlatAlpha = 0.92f; // ... at this alpha (a flat dusk sheet, never the game unblurred)
        public const float Blur43ScanAlpha = 0.14f; // the scanlines over the whole backdrop (lighter than a panel's)
        public const float Screens43ScrimAlpha = 0.25f; // the shop's NIGHT33 scrim over the backdrop (was 0.86 alone): a little more contrast for the text
        public const float Screens43PanelAlpha = 0.9f; // a panel's INK alpha (U1.md 1.3: opaque or none; 0.9 lets the blur's colour breathe at the cut)
        public const float Screens43AccentW = 4f; // px: a panel's left accent bar
        public const float Screens43Pad = 24f; // px: a panel's padding around its texts
        public const float Screens43FocusPlateAlpha = 0.95f; // the focused row's raised plate (INK-SOFT)
        public const float Screens43HeroPx = 75f; // a full screen's title / hero number: the HERO face at the type scale's 75
        public const float Screens43ContinueH = 52f; // px: the CONTINUE blade's height
        // the menus (UiMenus43.cs, round 3)
        public const float Menu43FocusScale = 1.08f; // the focused item grows to this (ui-art-web B6, DESIGN-RULES U-3)
        public const float Menu43PlateAlpha = 0.95f; // its raised plate (INK-SOFT) at full focus
        public const float Menu43EdgeW = 5f; // px: the cyan edge bar left of the focused item
        public const float Menu43EdgeGap = 8f; // px between that bar and the item's left edge
        public const float Menu43LockedAlpha = 0.5f; // a locked level row's muted label alpha (never lit)
        public const float Menu43Pad = 20f; // px: a menu panel past its items
        public const float Menu43EdgeRoom = 30f; // px more on the panel's left: the cyan edge (and NIGHT33's focus ticks' place)
        public const float Menu43GrowRoom = 48f; // px more on its right: the focused item's 1.08 grow and its focus slide along the slant
        public const float Menu43HeaderRoom = 50f; // px more above the level select's rows: its column headers (RANK / TIME / ...)
        public const float Shop43AccentH = 30f; // px: a shop panel's accent bar (straight, in the left margin, beside the header only: round 2)
        // the results' components (round 2; px from the canvas centre, y up; the head stays at ResultsHeadY 360, the menu at ResultsFirstRowY -132 down)
        public const float Results43TopY = 290f; // the hero panel's and the tiles' top (under the 75 px head)
        public const float Results43HeroX = -660f; // the hero panel's left (the menu column's left side)
        public const float Results43HeroSize = 300f; // the hero panel: a 300 px square (its bottom at -10, clear of the menu panel's top at about -86)
        public const float Results43HeroPx = 180f; // the OVERALL grade: the rank face at 180 px (3x the tiles' grades at 60: the lead's "3x")
        public const float Results43HeroLift = 14f; // px the grade sits above the panel's centre (the OVERALL label under it)
        public const float Results43LabelFromBottom = 26f; // px from the panel's bottom to the OVERALL label's centre
        public const float Results43RightX = -330f; // the tiles' and the banked line's left
        public const float Results43RightW = 1190f; // their width (to 860 px right of the centre: inside the 960 px half-width with a 100 px margin)
        public const float Results43Gap = 16f; // px between tiles
        public const float Results43TileH = 140f; // a tile's height (its label, its value at 42 px, its grade at 60 px)
        public const float Results43GradeW = 80f; // px: a tile's grade box (right)
        public const float Results43GradePx = 60f; // a tile's grade: the rank face at 60 px
        public const float Results43BankGap = 16f; // px from the tiles' bottom to the banked line
        public const float Results43RoomsX = -80f; // the rooms panel's left (clear of the menu panel's right edge at about -114)
        public const float Results43RoomsTopY = 80f; // its top (under the banked line)
        public const float Results43RoomsW = 940f; // its width (to 860)
        public const float Results43RowPitch = 36f; // px per room row (24 px text; 8 rows = 288 px)
        public const float Results43RoomNameW = 520f; // px: a room's number + name
        public const float Results43RoomGradeW = 70f; // px: its grade (the rank face at 32)
        public const float Results43AccentInset = 10f; // px: a panel's accent bar from its top-left corner (a short straight bar: never across the text)
        public const float Results43AccentH = 28f; // px: that bar's height (the header's height)
        public const float Results43MenuPad = 16f; // px: the menu panel past the items
        public const float Results43MenuExtraW = 40f; // px: more width for "CONTINUE: <A LONG LEVEL NAME>"
        public const double Results43EnterAt = 0.15; // s after the open: the components start their entrance (the head first)
    }
}
