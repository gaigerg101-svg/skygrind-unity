// NIGHT19 U3-33.8 (builder H): the HUD's and the menus' display numbers for tonight (none is a gameplay number). Px at
// 1080p, s of frame time. GG (2026-10-06): "the stamina bar should not be just in the middle of the screen, it's really
// ugly ... don't copy ULTRAKILL's UI but it should feel similar". Research/2026-10-06-weapon-sandbox.md section 6: one
// block of information bottom left (HP over the stamina), chunky segmented bars with hard edges, one dark plate, the
// centre kept empty. NIGHT19.md S14: nothing persistent in the central 40 % x 40 % but the crosshair, the markers and pops.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- S14: the central box ----------------------------------------------------------------------------------------------
        public const float Hud19CentreBox = 0.4f; // share of each side: the box S14 keeps clear (from 30 % to 70 %)

        // --- the vitals cluster (bottom left, the HP plate of Ui.cs): HP numeral, HP bar, the stamina pips, the slide cell ------
        // Inside the plate, px under its top: the S42 HP numeral (2..46), the HP bar (Hud19HpBarY, NIGHT17's 18 px), the pips
        // row (Hud19PipsY, Hud19PipH), then the wall-jump ticks (UiMoveRow, moved down Hud19MoveRowShift so they clear it).
        public const float Hud19HpBarY = 50; // the HP bar's top (NIGHT17: 54; up 4 px so the pips fit the plate)
        public const float Hud19PipsY = 76; // the pips' top edge
        public const float Hud19PipH = 16; // chunky: as tall as the HP bar nearly (NIGHT18's crosshair bar: 14)
        public const float Hud19PipMaxW = 120; // a pip at most this wide (one bar at a run's start: 120 px, cut in two boosts)
        public const float Hud19PipGap = 8; // px between pips
        public const float Hud19PipRowW = 268; // the pips' room: the HP bar's 320 minus the slide cell and its gap
        public const float Hud19SlideW = 40; // the slide charge's cell, at the row's right end (under the HP bar's end)
        public const float Hud19SlideGap = 12; // px between the last pip's room and the slide cell
        public const float Hud19CellSlit = 3; // px: the INK slit at each boost's cost inside a pip (one bar = two boosts)
        public const int Hud19MaxSlits = 7; // slits made per pip at most (a cost of 1/8 bar or more)
        public const float Hud19MoveRowShift = 13; // px the wall-jump ticks move down (they sat where the pips are now)

        // --- the pips' moments (a drain flash, a refill tick; no fade at rest: they are always there, like HP) -----------------
        public const double Hud19DrainFlash = 0.08; // s the spent part shows WHITE (a boost, a dash-jump; FLASH INTENSITY 0: none)
        public const double Hud19TickTime = 0.14; // s of a refill tick: the cell that refilled WHITE, its pip punched
        public const double Hud19TickWhite = 0.05; // s of it the refilled cell is WHITE (three 60 Hz frames)
        public const float Hud19TickPunch = 1.12f; // the pip's scale at the tick (back to 1 over Hud19TickTime, ease-out)
        public const double Hud19EmptyPulse = 0.9; // s: one slow pulse of the tracks while a boost can't fire
        public const string Hud19PipsSpentEvent = "pipsSpent"; // FeelProbe.Event on the frame the pips show a spend (the reel's S14 timing)
        public const string Hud19PipsTickEvent = "pipsTick"; // ... and on the frame a refill tick starts

        // --- the weapon card (bottom right, over the points): a header row over the slots ---------------------------------------
        // At rest the header holds the gun in hand's ROLE (S18 BODY-BOLD, CYAN-PALE, tracked caps) and its freshness tier word
        // (design.freshness, W's read); on a swap the gun's NAME slams into the header (S24, WHITE) for Hud17SwapNameTime, then
        // the role returns. The swap name no longer sits under the crosshair (S14).
        public const float Hud19CardHeadH = 26; // px: the header row
        public const float Hud19CardHeadGap = 4; // px between the header and the slots
        public const float Hud19CardHeadMaxW = 300; // px: the header's widest text ("LINES + ARMOUR  STALE", a long name at S24): the layout tests' bound
        public const double Hud19CardTagIn = 0.15; // s: the role's entrance after the name (IDENTITY.md's 150 ms)
        public const float Hud19CardTierGap = 10; // px between the role and the tier word
        public const float Hud19AltSegGap = 3; // px between the alt meter's charges (the gust's)
        public const int Hud19AltMaxSegs = 3; // charge cells made at most (the gust: one per airtime plus one on the ground)

        // --- the combo callouts (N19Events.Combo: POP, CONDUCTOR, LAUNCH + SNIPE, SWAP-CANCEL ...) in the style meter's column ----
        // A combo is a callout of its own kind: its name in INK on a CYAN slanted plate (the player's colour, filled), slammed in
        // at the column's top slot (HudSlam18: from 1.6x at full alpha, white-hot for its first frames), held longer than an
        // award's line; it pushes the older lines down a slot (never drawn over them).
        public const double Hud19ComboLife = 1.5; // s a combo line stays (an award's: Hud17CalloutLife 0.9)
        public const double Hud19ComboFadeAt = 1.15; // s: its fade starts
        public const double Hud19ComboWhite = 0.05; // s its plate is WHITE (three 60 Hz frames; FLASH INTENSITY 0: none)
        public const float Hud19ComboPadX = 10; // px of plate each side of the name
        public const float Hud19ComboPlateInset = 3; // px the plate is shorter than the line (top and bottom: a gap to the next line)

        // --- the buy prompt's plate (section 6's banned list: flat text on nothing, a translucent grey hold bar) ----------------
        public const float Hud19BuyPlateBelow = 18; // px the plate reaches under the prompt's box (its hold bar sits 10 px under it)
        public const float Hud19BuyPlateAbove = 8; // px over the box
        public const float Hud19BuyPlatePadX = 24; // px each side of the text (or of the 240 px hold bar)

        // --- the WEAPONS screen's role cards (UiWeapons.cs): each gun's row grows a role band ------------------------------------
        // The row (UnityTuning.Meta's 124 px) gains a band at its foot: GOOD AGAINST and COMBOS WITH (S18 MUTED labels, WHITE
        // values), and the role's tag sits by the gun's name on a CYAN slanted plate (INK text). NIGHT18's leftover ("the
        // WEAPONS body appears at once"): the rows cascade in along the slant, each tag plate popping after its row.
        public const float Hud19WeaponRowHeight = 166; // px (the role band under NIGHT12's 124 px row)
        public const float Hud19RoleBandY = 128; // px under the row's top: the band's line top
        public const float Hud19RoleLineH = 26; // px: the band's line
        public const float Hud19RoleLabelGap = 10; // px between a label and its value
        public const float Hud19CombosX = 560; // px: COMBOS WITH starts where the finishes do (UnityTuning.FinishX)
        public const float Hud19RoleTagGap = 16; // px between the name and its tag plate
        public const float Hud19RoleTagPadX = 10; // px of plate each side of the tag
        public const float Hud19RoleTagH = 28; // px: the tag plate's height
        public const double Hud19RowStagger = 0.04; // s between rows entering (whole by Hud19RowStaggerMax + ThemeInTime)
        public const double Hud19RowStaggerMax = 0.2; // s: the last rows' wait at most (the list's first screen enters in ~350 ms)
        public const double Hud19TagPopAfter = 0.08; // s after its row starts, the tag plate pops (HudEntrance18: 0.6x -> 1.1x -> 1)

        // --- the guns' roles (UI words, not gameplay numbers): the card's tag and the WEAPONS screen's role cards --------------
        // NIGHT19.md U3-33.6's niches (Research/2026-10-06-weapon-sandbox.md 4-10) and the combos W builds (POP, CONDUCTOR,
        // LAUNCH + SNIPE, SWAP-CANCEL). At most 3 words a tag (IDENTITY.md: 3 on the HUD), 8 a line; the static charset only.
        public static readonly System.Collections.Generic.Dictionary<string, (string tag, string good, string combos)> Hud19Roles =
            new System.Collections.Generic.Dictionary<string, (string, string, string)>
            {
                ["revolver"] = ("TRIGGER", "HEADS AT RANGE · SPITTERS", "COMET, BOUNCER: POP"),
                ["shotgun"] = ("POINT BLANK", "RUSHES · ANYTHING IN YOUR FACE", "ANY GUN: SWAP-CANCEL"),
                ["nailgun"] = ("BATTERY", "SWARMS · KEEPING DAMAGE ON", "ZAPPER, LONGWAVE: CONDUCTOR"),
                ["rocket"] = ("CROWDS + LIFT", "PACKS · ROCKET JUMPS", "KESTREL: POP · ANY HIT: LAUNCH + SNIPE"),
                ["rail"] = ("LINES + ARMOUR", "CORRIDORS · SHIELDED FRONTS", "STITCHER: CONDUCTOR"),
                ["zapper"] = ("CHAINS", "SWARMERS · PACKS", "STITCHER: CONDUCTOR"),
                ["bouncer"] = ("BANK SHOTS", "AROUND CORNERS · GROUPS", "KESTREL: POP"),
                ["gauntlet"] = ("MELEE", "PARRIES · FINISHING", "ANY GUN: SWAP-CANCEL"),
                // NIGHT20 B5 (builder HV; G's port): the sandbox's "area denial" (Research/2026-10-06-weapon-sandbox.md 9);
                // its burn is the fire family's status, so another family's hit on it is the arsenal's CROSS (arsenal.js)
                ["torch"] = ("AREA DENIAL", "SWARMS AT YOUR FEET · CHOKE POINTS", "FIRE WALL, THEN ANY GUN: CROSS"),
                // NIGHT20 B5 (builder HV; G2's port): the pull gathers a pack (guns.js: a slow orb, a 1.5 s pull, a burst);
                // its PULL status then any blast hit is the arsenal's COLLAPSE (arsenalMath.js: 2x, in a radius)
                ["vortex"] = ("CROWD CONTROL", "SPREAD PACKS · KITERS", "PULL, THEN ANY BLAST: COLLAPSE"),
            };
    }
}
