// NIGHT17 U3-30.7 (builder H): the rebuilt HUD's numbers (display only, none is a gameplay number). Px at 1080p, s of frame
// time. The type sizes are the theme's scale (UiTheme.Size); the tokens that every screen shares (slant, cut, safe inset,
// HUD SCALE range, motion) stay in UnityTuning.Theme.cs. Research/2026-10-05-overhaul.md section 2 ("energetic score")
// gives most of these; the rest are the builder's picks, each with its reason.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the three tiers: the glance block's sizes --------------------------------------------------------------------
        public const float Hud17VitalsWidth = 380; // the vitals plate (HP number, HP bar, the speed row), bottom left
        // Round 1's reviewer: POINTS is the one hero number; HP is secondary: an S42 numeral over a fat segmented bar.
        public const float Hud17VitalsHeight = 104; // its height before UiMoveRow grows it by MoveRowPlateGrow (the S42 numeral, its bar, the speed row)
        public const float Hud17VitalsPad = 20; // px inside the plate's left edge (past the accent bar)
        public const float Hud17HpBarWidth = 320; // the HP bar (slanted with the theme)
        public const float Hud17HpBarHeight = 18; // fat: the HP's glance is the bar (round 1's reviewer)
        public const int Hud17HpSegments = 4; // the bar's segments (INK slits at each quarter: a quarter of HP reads at a glance)
        public const float Hud17HpSlit = 3; // px: a slit's width
        public const string Hud17Tabular = "<mspace=0.56em>"; // TMP tabular digits for numbers that change (the theme's monospaced digits)

        // --- points: the odometer ---------------------------------------------------------------------------------------------
        public const float Hud17PointsBox = 92; // the S75 numeral's line box (round 1's reviewer: POINTS is the one hero number)
        public const float Hud17PointsPlatePad = 10; // round 3's reviewer: the score on an opaque INK plate (cut), this far round the number and its bar
        public const float Hud17PopMergePunch = 1.35f; // round 3: a merged gain re-punches the pop 1.0 -> 1.35 -> 1.0 ...
        public const double Hud17PopMergePunchTime = 0.12; // ... over 120 ms
        public const double Hud17PointsWhite = 0.034; // s of the white frame on a gain (two frames at 60 Hz; one at the floor of 30)
        public const float Hud17PointsBarHeight = 4; // the gold accent bar under the points (the theme's accent bar)
        public const float Hud17PointsBarGap = 2; // px between the numeral's box and its bar

        // --- the pops at the crosshair (round 1's reviewer: 36-48 px, in at 1.4x -> 1.0 ease-out-back over 150 ms, one running
        // sum per burst, then a flight into the counter, which punches 1.0 -> 1.25 -> 1.0 over 150 ms on the arrival) ---------
        public const double Hud17PopIn = 0.12; // round 2's reviewer: in at 1.8x over 120 ms
        public const double Hud17PopHold = 0.35; // s held after the last gain (a merge restarts it)
        public const double Hud17PopFly = 0.25; // s into the counter (ease-in)
        public const float Hud17PopOver = 1.3f; // round 5: the entrance's overshoot (at half of Hud17PopIn)
        public const float Hud17PopFrom = 0.7f; // round 5: a pop enters at 0.7x, overshoots to Hud17PopOver and settles at 1.0x within Hud17PopIn (it jumped from big white to small gold)
        public const float Hud17PopFlyScale = 0.6f; // its scale as it lands in the counter
        public const float Hud17PopArrivePunch = 1.2f; // round 5: the counter scales to 1.2x on each arrival (and on each gain) ...
        public const double Hud17PopArriveTime = 0.12; // ... and settles over 120 ms
        public const double Hud17PopMerge = 0.15; // s: round 5: pops landing within 150 ms become one summed number (two kills ~17 ms apart overprinted +12025)
        // Round 2's reviewer: the pops sit up-right of the crosshair (never on the body under it), with a 4-5 px INK stroke.
        public const float Hud17PopX = 90; // px right of the crosshair: the newest pop's centre
        public const float Hud17PopY = 90; // px above it
        public const float Hud17PopJitter = 16; // px of x jitter (the run's Mulberry32 stream; the web's popSpread was 40)
        public const float Hud17PopStack = 84; // round 6: a new burst's pop sits this far (a line of the S75 tier at its 1.3x overshoot) above every older pop still near the crosshair, so two live pops never overlap
        public const float Hud17PopLiftMax = 84; // px at most (one line: the pops lane stays under the round bonus line)
        public const float Hud17PopOutline = 0.4f; // TMP outline width (SDF units): round 5's 3 px INK outline, so gold survives on orange bodies and on world signs

        // --- the style meter, right side (ULTRAKILL's letter, word, decay bar, callouts) ---------------------------------------
        // The tile's layout (IDENTITY.md, Logs/shots/n17-b1/n17-tile.png): the letter on the left, then a column with the word
        // (DISPLAY, cyan) over the slanted decay bar over the callouts; the whole block right-anchored at the safe inset.
        public const float Hud17StyleRight = 0; // px in from the safe inset
        // Round 2's reviewer: out of the play space: the block in the top third (its bottom edge >= 300 px over the centre:
        // the letter's middle at 410, the third callout's bottom ~307), no plate, a dark stroke on its text instead.
        public const float Hud17StyleTop = 130; // px under the top edge: the letter's centre (410 over the middle at 1080; anchored to the top so HUD SCALE keeps it on screen)
        public const float Hud17StyleLetterPx = 120; // the letter (UiTheme.Rank; the reviewer's 120 px, between S100 and S133)
        public const float Hud17StyleLetterBox = 124; // the letter's box, its height
        public const float Hud17StyleLetterCap = 86; // px: the letter's capital height at rest (round 5: the plate fits it, not the box)
        public const float Hud17StyleLetterWidth = 170; // the letter's box width ("SS" fits); the letter is right-aligned in it
        public const float Hud17StyleColumn = 186; // px from the block's left: the word / bar / callouts column
        public const float Hud17StyleWidth = 456; // the block: the letter's box + the column (the bar and the longest callout)
        public const float Hud17StyleWordY = 34; // px above the letter's centre: the word's centre
        public const float Hud17StyleBarY = 6; // px above the letter's centre: the decay bar's top
        public const float Hud17StyleBarWidth = 240, Hud17StyleBarHeight = 8; // the decay bar (slanted, on an INK-SOFT track; round 1's reviewer: thicker, its fill obvious)
        public const float Hud17StylePlatePad = 14; // px the column's INK backing plate reaches past the word, the bar and the callouts
        public const double Hud17StyleWordTime = 0.6; // s the rank's word shows after a rank-up (research: 600 ms)
        public const double Hud17StyleWordIn = 0.12; // s its entrance along the slant
        public const float Hud17StyleWordSlide = 40; // px it slides in from the right
        public const double Hud17StyleKnockTime = 0.28; // s a rank drop's knock-down settles
        public const float Hud17StyleKnockPx = 22; // px the letter is knocked down on a drop
        public const float Hud17StyleKnockDeg = -9; // deg it tips on a drop
        public const double Hud17StyleDangerTime = 0.16; // s the decay bar shows DANGER after the player is hurt (the bar only)
        public const float Hud17StyleShakePx = 6; // px the HUD (frame + values layers) shakes on a rank-up (round 3: 6 px; rank-ups only, never with REDUCED MOTION)
        public const double Hud17StyleShakeTime = 0.1;
        // Round 1's fix list: the rank-up letter is the screen's event for its 100 ms: it slams from 3x (the theme's slam is
        // 2x: too small at 133 px), and the word slams in at 1.8x beside it.
        public const float Hud17StyleSlamFrom = 2.2f; // round 3: the rank-up letter slams from 2.2x, white-hot, held Hud17StyleSlamHold
        public const double Hud17StyleSlamHold = 0.1; // round 3: s the slam holds at 2.2x, white-hot (it was at rest by ~70 ms)
        public const double Hud17StyleSlamTime = 0.16; // round 3: then ease-out-back to 1.0 over this many s
        public const float Hud17StyleWordSlamFrom = 1.8f;
        public const double Hud17StyleWordSlamTime = 0.16;
        // U3-30.10 (a): the rank-up's STREAK: a full-width band on the 12 deg slant behind the letter, in the new rank's ladder
        // colour, sweeping in from the left over 150 ms, out (thinning and fading) over 250 ms.
        public const double Hud17StreakIn = 0.15, Hud17StreakOut = 0.25;
        // Round 2's fix list: behind the style column only (it crossed the crosshair edge to edge): it starts right of the
        // screen's central 40 % (Hud17StreakLeft of the width from the left) and runs past the right edge; thinner and fainter.
        public const float Hud17StreakHeight = 80; // px (the letter's box is 150)
        public const float Hud17StreakAlpha = 0.35f; // its colour's alpha at full (the letter reads over it)
        public const float Hud17StreakLeft = 0.72f; // share of the canvas width: the band's left end (the centre's 40 % ends at 0.70)
        public const float Hud17StreakOver = 60; // px it runs past the right edge (turned 12 deg, no end shows)
        public const int Hud17StreakKills = 32; // U3-30.10 (b): the kill-streak buffer (kills inside one multikill window; MASSACRE at 5)
        public const float Hud17ModChipHeight = 28; // px: a mod chip (its whole word at S18 bold; round 1: "BOU" was cut and tiny)
        public const float Hud17ModChipPad = 8; // px each side of its word
        public const float Hud17StyleGlowScale = 1.1f; // the top rungs' glow copy, x the letter
        public const float Hud17StyleGlowAlpha = 0.45f;
        public const float Hud17StyleOffsetAlpha = 1f; // the hero offset copy (CYAN-DEEP, ThemeHeroOffset px down-right)
        public const float Hud17CalloutLine = 30; // px per callout line (the newest S24 WHITE, older ones S18 CYAN: the tile)
        public const float Hud17CalloutTop = 22; // px under the decay bar: the newest callout's centre
        public const float Hud17CalloutWidth = 360;
        public const double Hud17CalloutFadeAt = 0.6; // s: a line starts fading (round 2's reviewer: the oldest fading at 600 ms)
        public const double Hud17CalloutLife = 0.9; // s: gone (was hud.feedFadeTime's 1.6)
        public const float Hud17CalloutSlide = 48; // px a callout slides in from the right (round 3's reviewer: over 120 ms)
        public const double Hud17CalloutSlideTime = 0.12;
        public const int Hud17CalloutMax = 3; // lines at most (round 2's reviewer; hud.feedSize is 5)

        // --- the multiplier chip ("x1.2 AIR"; research: slams from 1.6x with a 2-frame white flash and a thin drain bar) -------
        public const float Hud17ChipFrom = 1.6f;
        public const double Hud17ChipSlam = 0.12;
        public const double Hud17ChipWhite = 0.034; // s white (two frames at 60 Hz)
        public const double Hud17ChipLinger = 0.45; // s the chip stays after the state ends, its bar draining (it reads "ending")
        public const float Hud17ChipWidth = 150, Hud17ChipHeight = 40, Hud17ChipBar = 3;
        public const float Hud17ChipGap = 10; // px between the chip and the letter's box (the chip sits left of the block: round 2, the block's top third)

        // --- the stamina bar under the crosshair --------------------------------------------------------------------------------
        public const int Hud17StaminaMaxSegments = 4; // segments made once (the web's 3 pips + VIGOR)

        // --- the weapon card (Neon White: a silhouette, not a name plus key hints) -----------------------------------------------
        public const float Hud17CardSilW = 120, Hud17CardSilH = 46; // the silhouette's box
        public const int Hud17CardSilTex = 128; // px of the silhouette's texture (wide); its height is half
        // Round 2's reviewer: the weapon readout is a strip of at most 360 x 64 px in the corner (the 610 px card drew over the
        // viewmodels): one slot glyph per owned gun (its silhouette), the gun in hand CYAN-filled, a freshness pip; no names.
        public const int Hud17StripCells = 6; // slots shown at most (the window scrolls with the gun in hand; "+N" past it)
        public const float Hud17StripCellW = 48, Hud17StripCellH = 40, Hud17StripGap = 3; // a slot, and between slots
        public const float Hud17StripSilW = 46, Hud17StripSilH = 24; // a slot's silhouette box (2:1, preserveAspect)
        public const float Hud17StripPad = 8; // px inside the strip's plate (each side)
        public const float Hud17StripMeterH = 4; // the heat / charge / reload meter under the slots
        public const float Hud17StripMoreW = 32; // px for "+N" at the row's left
        public const float Hud17StripPip = 6, Hud17StripPipInset = 4; // the freshness pip (FRESH CYAN-HOT / USED CYAN / STALE MUTED), top-right
        // The gun's NAME only on a swap, for 600 ms, under the crosshair (S32 DISPLAY with the INK stroke).
        public const double Hud17SwapNameTime = 0.6, Hud17SwapNameIn = 0.12, Hud17SwapNameOut = 0.1;
        // Each gun's silhouette: rects (x, y, w, h) on a 32 x 16 grid, x to the muzzle, y up, drawn white along the theme's
        // slant (UiHotbar.cs WeaponSilhouette). Blocky like the guns' models (BlockyKit): barrels, bodies, grips, scopes.
        public static readonly System.Collections.Generic.Dictionary<string, float[]> Hud17Silhouettes = new System.Collections.Generic.Dictionary<string, float[]>
        {
            ["revolver"] = new float[] { 10, 9, 17, 3, 7, 7, 8, 5, 4, 7, 6, 4, 4, 1, 5, 7, 3, 10, 3, 2 },
            ["gauntlet"] = new float[] { 8, 3, 14, 10, 22, 4, 5, 8, 2, 5, 7, 6 },
            ["shotgun"] = new float[] { 6, 9, 25, 3, 10, 6, 17, 3, 0, 4, 9, 5, 7, 6, 6, 6, 9, 2, 4, 5 },
            ["nailgun"] = new float[] { 6, 6, 16, 7, 22, 9, 9, 3, 11, 1, 7, 6, 6, 1, 4, 6 },
            ["rocket"] = new float[] { 2, 6, 26, 7, 26, 5, 5, 9, 10, 1, 4, 6, 14, 13, 4, 2 },
            ["rail"] = new float[] { 8, 9, 24, 2, 4, 7, 14, 5, 9, 12, 9, 3, 0, 5, 6, 5, 12, 2, 3, 6 },
            ["rattler"] = new float[] { 6, 6, 14, 6, 20, 8, 9, 2, 12, 0, 4, 7, 7, 1, 4, 6, 0, 7, 6, 3 },
            ["torch"] = new float[] { 2, 4, 8, 9, 10, 6, 14, 5, 24, 7, 7, 3, 12, 2, 3, 5 },
            ["bouncer"] = new float[] { 12, 5, 16, 9, 4, 6, 10, 6, 6, 1, 4, 6 },
            ["zapper"] = new float[] { 14, 4, 3, 9, 18, 4, 3, 9, 22, 4, 3, 9, 4, 6, 10, 6, 25, 8, 6, 2, 6, 1, 4, 6 },
            ["discus"] = new float[] { 6, 5, 20, 6, 10, 2, 12, 12 },
            ["vortex"] = new float[] { 16, 3, 12, 10, 4, 6, 12, 5, 6, 1, 4, 6 },
            ["sunder"] = new float[] { 9, 6, 23, 4, 6, 3, 3, 10, 0, 7, 6, 2 },
            ["flicker"] = new float[] { 6, 9, 20, 2, 10, 4, 20, 2, 2, 9, 4, 2, 6, 4, 4, 2 },
            ["thumper"] = new float[] { 20, 2, 10, 12, 0, 7, 20, 2 },
            ["tether"] = new float[] { 0, 6, 8, 3, 8, 7, 8, 2, 16, 8, 6, 2, 22, 10, 5, 2, 27, 12, 4, 2 },
            ["choir"] = new float[] { 10, 10, 20, 2, 10, 7, 22, 2, 10, 4, 20, 2, 4, 4, 8, 9, 5, 0, 4, 5 },
            ["prism"] = new float[] { 16, 2, 10, 12, 4, 6, 12, 5, 6, 1, 4, 6 },
        };

        // --- the round: a hero number, then its corner (research: 133-160 px at the centre for 1.5 s, then 400 ms) -------------
        public const float Hud17RoundHeroPx = 150; // the hero numeral (the RANK face): between S133 and 160
        // Round 2's reviewer ("ROUND 01" by the HP card read like ammo): the hero number flies into the top line (ROUND N ·
        // N LEFT) and is gone; no corner counter at rest.
        public const float Hud17RoundCornerPx = 24; // its size as it lands in the top line (S24, the line's size)
        public const float Hud17RoundTopDrop = 16; // px under the safe inset: the top line's middle (its 32 px box)
        public const float Hud17SpeedShowAt = 1.05f; // the speed readout shows above walkSpeed x this (round 2: hidden at rest)
        public const bool Hud17ModChipsOnHud = false; // round 5: the mod chips ("BOUNTY HUNTER") come off the HUD at rest (PAUSED lists the mods)
        public const bool Hud17SpeedOnHud = false; // round 5: the speed readout comes off the default HUD (it read ~10 px; speed shows as the lines and the FOV)
        public const float Hud17ModChipBar = 4; // px: the mod chip's bar in its tag's colour (the words stay WHITE on INK)
        public const double Hud17RoundSlam = 0.12; // s the hero slams from Hud17RoundSlamFrom to 1
        public const float Hud17RoundSlamFrom = 1.4f; // round 3: 1.7x dipped its ROUND into the crosshair's box for the slam's first frames
        public const double Hud17RoundHold = 1.5; // s at the centre
        public const double Hud17RoundFly = 0.4; // s to the corner (ease-in-out cubic: nothing linear)
        public const float Hud17RoundHeroY = 250; // px above the screen's middle (round 3: off the crosshair and the spawning enemies; its ROUND under it stays above the crosshair's 200 px box even at the slam's 1.4x; left of the style block, under the top line)
        public const float Hud17PerkGap = 12; // px between the vitals plate (grown by UiMoveRow) and the perks row over it

        // --- the round's CLEAR (round 1's reviewer: "ROUND 1 CLEAR" slams 1.6 -> 1.0, ease-out-back 160 ms, the round bonus
        // tallies into the counter, everything cleared within 1.2 s) ------------------------------------------------------------
        public const float Hud17ClearFrom = 1.6f; // round 5: ROUND N CLEAR slams 1.6x -> 1.0x (ease-out cubic: a slam, no overshoot) ...
        public const double Hud17ClearIn = 0.14; // ... in 140 ms, from its final place
        public const double Hud17ClearFadeFrom = 1.34; // s: held 1.2 s after its entrance, then out (ease-in)
        public const double Hud17ClearEnd = 1.54; // s: gone (200 ms out)
        // Round 3's reviewer (a whisper: plate-less white text over a pale sky): a full-width INK band at ~37 % of the screen's
        // height from the top (its middle 135 px over the centre), 220 px tall, behind the S133 CLEAR; it sweeps in with it.
        public const float Hud17ClearBandY = 220, Hud17ClearBandH = 220; // round 5: 110-310 px over the centre (the upper third), opaque INK-SOLID
        public const double Hud17ClearBandIn = 0.15; // s its sweep from the left (ease-out)
        public const float Hud17ClearBandWidth = 1.25f; // x the canvas width (slanted, its ends off screen)
        public const float Hud17PopRoundPunch = 1.25f; // the score's punch when the round's bonus lands in it (the ceremony's roll)
        public const float Hud17ClearY = 270; // px above the screen's middle: the CLEAR's centre (round 6: lifted with its bonus line, clear of the kill pops' lane)
        // Round 2's fix list (the bonus pop sat on the tally and the CLEAR's edge): one slot ~40 px under the CLEAR, used in
        // turn: the round's bonus pops there first and flies into the counter (in 150 ms + hold 200 ms + flight 250 ms), and
        // the tally slides into the same slot as the bonus leaves.
        public const float Hud17ClearBonusDrop = 90; // px under the CLEAR's centre: the bonus pop's centre (180 over the centre: above the kill pops' lane, up to ~126 at their overshoot)
        // U3-30.10 (c): the round's tally under the CLEAR ("14 KILLS · BEST +480", S24 body bold), sliding in along the slant
        public const float Hud17ClearTallyDrop = 90; // px under the CLEAR's centre: the tally's (the same slot, after the bonus has flown)
        public const double Hud17ClearTallyDelay = 0.75; // s after the CLEAR starts (the bonus is most of the way to the score)
        public const double Hud17ClearTallyIn = 0.15; // s, ease-out-back, from ThemeEnterDistance px down-left along the slant
        public const double Hud17ClearBonusHold = 0.5; // s the bonus holds under the CLEAR before it flies (it lands by ~0.87 s, and the score rolls then)

        // --- the power-up pickup (round 3's reviewer: DOUBLE POINTS showed as an ~11 px line) --------------------------------------
        public const double Hud17PowerEdgeTime = 0.1; // s of the edge flash in the power-up's colour (through the flash gate)
        public const float Hud17PowerEdgeAlpha = 0.55f; // its alpha at full FLASH INTENSITY
        public const float Hud17PowerBannerY = 250; // px over the centre: the banner (S75 DISPLAY, the INK stroke; the hero's lane)
        public const double Hud17PowerBannerIn = 0.14, Hud17PowerBannerHold = 1.0, Hud17PowerBannerFly = 0.25; // s: in (round 5: a slam from 2x, ease-out cubic), shown, shrinking into its chip
        public const float Hud17PowerBannerFrom = 2.0f; // round 5: the banner slams in from 2x (it appeared full size)
        public const float Hud17PowerChip = 44; // px: a chip beside the score (INK, cut), its ring timer inside
        public const float Hud17PowerRing = 17, Hud17PowerRingWidth = 4; // px: the ring's radius and width (the time left as a clockwise fill)
        public const float Hud17PowerChipGap = 8; // px between chips, and from the score's plate
        // Each power-up's chip glyph (no word on the HUD; at most 2 characters).
        public static readonly System.Collections.Generic.Dictionary<string, string> Hud17PowerGlyphs = new System.Collections.Generic.Dictionary<string, string>
        {
            ["double"] = "2×", ["overclock"] = "OC", ["stasis"] = "ST", ["refund"] = "½", ["shockwave"] = "SW",
        };

        // --- the zone's banner (round 1's reviewer: LANDING sat on screen through fights) -----------------------------------------
        public const double Hud17ZoneBannerTime = 2.0; // s a zone's name shows on entering it (zones.banner.time is 2.4)
        public const double Hud17ZoneRepeat = 20; // s before the same zone's name shows again (crossing a border back and forth)

        // --- the kill marker (distinct from the hit marker: bigger, another shape, a scale punch) --------------------------------
        public const float Hud17KillMarkerPx = 58; // the 4-spike burst's box
        public const double Hud17KillMarkerTime = 0.32; // s it shows
        public const float Hud17KillMarkerPunch = 1.45f; // its scale at the kill, settling to 1 (ease-out cubic)
        public const double Hud17KillMarkerPunchTime = 0.12;
        public const float Hud17KillMarkerTurn = 20; // deg it turns as it settles

        // --- UiFeel (round 1's reviewer, traced by builder X) ------------------------------------------------------------------
        public const float Hud17ParryFlashMax = 0.25f; // the parry's white flash at most this alpha (punch.parryFlashOpacity is 0.45: it whited out the HUD)
        public const float Hud17LinesShift = 0.35f; // the speed lines' centre moves this share of half the canvas toward the sideways travel
        public const float Hud17LinesEase = 10f; // per s: how fast it follows (about 0.1 s)

        // --- low HP: the vignette breathes (no new hue: DANGER, as before) -----------------------------------------------------
        public const double Hud17LowHpPeriod = 1.2; // s per breath
        public const float Hud17LowHpBase = 0.18f, Hud17LowHpAmp = 0.16f; // the vignette's alpha: base + amp x the breath

        // ======================================================================================================================
        // NIGHT18 U3-31.6 (builder H): HUD motion and the kill marker, behind FeelAB (off: the Hud17 numbers above, untouched).
        // M9: no HUD element's first drawn frame is at >= 0.95 of its rest scale AND alpha AND < 40 px from rest; entrances
        // 90-300 ms. IDENTITY.md's motion: in 150 ms ease-out-back (~10 % overshoot) along the 12 deg slant, out 100 ms ease-in.
        // Every entrance grows from under 0.95 (a slam DOWN from 1.6x is a pop-in by the reel's rule) and fades in; its exit
        // fades only, so its rest scale stays 1.
        // ======================================================================================================================

        public const double Hud18EnterTime = 0.20; // s: an entrance is whole. NIGHT22 T-12: 0.15 -> 0.20 (IDENTITY.md's 150 ms settled in 83 ms by M9's rule, under its 90 ms floor: Round mark, EXPOSED; 0.20 settles in about 110-130 ms, inside 90-300)
        public const float Hud18EnterFrom = 0.6f; // its first scale (under M9's 0.95 on its first drawn frame at any fps)
        public const float Hud18EnterPeak = 1.1f; // the overshoot (IDENTITY.md's ~10 %) ...
        public const double Hud18EnterPeakAt = 0.55; // ... at this share of Hud18EnterTime, then it settles to 1 (UiTween.PopIn)
        public const double Hud18EnterAlphaTime = 0.06; // s: its fade-in (ease-out cubic): visible on its first frame, whole by ~4 frames
        public const double Hud18EnterFirstStep = 1.0 / 60; // s: the age its first frame is drawn at, at most (the same first frame at any fps)
        public const double Hud18ExitTime = 0.10; // s: an exit's fade (ease-in; IDENTITY.md's out 100 ms)
        public const double Hud18FadeInTime = 0.2; // s: a fade-only entrance (the CLEAR's band, the rank-up streak): 95 % at ~126 ms (round 1: 33 ms read as a pop)

        // The kill marker (GG's KILL MARKER switch, ON): WHITE (the hit marker is white too: the kill's is another SHAPE, four
        // thick wedges, and bigger), drawn on the kill's own frame, an overshoot pop, 300-400 ms (M2's marker half).
        public const float Hud18KillMarkerPx = 120; // its box at rest (round 2: the wedges outside a 40 px clear centre: Inner / Outer below): 96 px on its first frame (x Hud18KillMarkerFrom)
        public const float Hud18KillMarkerFrom = 0.8f; // its first frame's scale (drawn at age 0: under 0.95, so not a pop-in)
        public const float Hud18KillMarkerPeak = 1.18f; // the pop's peak ...
        public const double Hud18KillMarkerPeakAt = 0.04; // ... this many s in (between frames 2 and 3 at 60 fps) ...
        public const double Hud18KillMarkerIn = 0.20; // ... settled to 1 by this many s (within 5 % of rest at ~96 ms; round 1: 0.12 read as an 83 ms entrance), then held crisp
        public const double Hud18KillMarkerTime = 0.36; // s it shows (M2: 300-400 ms)
        public const double Hud18KillMarkerOut = 0.10; // s: its fade at the end (ease-in), at its rest size
        public const float Hud18KillMarkerTurn = 0f; // deg it turns as it settles (round 1's picks: none, it is held crisp; was the 12 deg slant)
        public const float Hud18KillMarkerHead = 1.15f; // x on a headshot kill
        public const float Hud18KillMarkerOutline = 5f; // px: its INK stroke all round (round 1's picks: white wedges vanished over the white burst)
        // Round 2's picks ("at 20 m the marker hides the whole target"): px along each diagonal from the crosshair, at rest:
        // the white wedge from Inner (its stroke from Inner - Outline: a 40 px clear centre) to Outer.
        public const float Hud18KillMarkerInner = 45f, Hud18KillMarkerOuter = 75f;
        public const float Hud18KillMarkerWedge = 7f; // px: a wedge's half-width at its outer end

        // Round 2's picks (DOUBLE POINTS and ROUND N CLEAR: "slams down from about 1.4x [NIGHT17]; [tonight] grows from a faded
        // 0.8x"): the big banners SLAM: from Hud18SlamFrom to 1 over Hud18SlamTime (ease-out quad: within 5 % by ~97 ms),
        // fading in from Hud18SlamAlphaFrom over Hud18SlamAlphaTime (its first frame's alpha: M9 sees an entrance).
        public const float Hud18SlamFrom = 1.6f; // round 3's picks: from about 1.6x at FULL alpha (was 1.4x at 0.6: "A1 nearly invisible")
        public const double Hud18SlamTime = 0.14; // within 5 % at ~100 ms
        public const float Hud18SlamAlphaFrom = 1f; // whole from its first frame (the tool counts a first frame above 1.05x as an entrance)
        public const double Hud18SlamAlphaTime = 0.1;
        public const double Hud18SlamWhite = 0.05; // s: round 3's picks ("~1.7x and white, settles to cyan"): white-hot for its first three 60 Hz frames

        // The hit marker: its arms grow out from the crosshair (scale and distance) instead of appearing whole.
        public const float Hud18HitMarkerFrom = 0.5f; // its first scale (a hit's alpha fades over its life, so the scale carries M9)
        public const float Hud18HitMarkerPeak = 1.15f; // its overshoot ...
        public const double Hud18HitMarkerIn = 0.09; // ... and whole by this many s (the marker lives hud.hitMarkerTime: 150 ms)
        public const float Hud18HitMarkerRepeatFrom = 0.8f; // a hit while it shows re-grows from here (a small kick, not a new entrance)
        // Round 1's picks ("the hit X barely visible"): bolder arms over an INK underlay, still inside the kill marker's hollow.
        public const float Hud18HitMarkerWidth = 3f; // px an arm is wide (NIGHT17: 2)
        public const float Hud18HitMarkerLength = 1.5f; // x hud.hitMarkerSize (8 px: 12 px arms)
        public const float Hud18HitMarkerOutline = 2f; // px of INK round each arm
        // Round 1's picks ("+96 overlaps +100 ROUND"): the round's bonus pops left of the CLEAR's middle, clear of the kill pops' lane (up-right of the crosshair).
        public const float Hud18RoundPopX = 220f; // px left of the centre

        // Flashes that cover the screen (the power-up's edge, the hurt vignette) swell in over a few frames instead of
        // appearing at full: an ease-in attack, then their own fade.
        public const double Hud18FlashAttack = 0.04; // s (ease-in: ~7 % on its first 60 Hz frame, ~58 % on its second, whole on its third)
        public const double Hud18PowerEdgeRise = 0.12; // s: the power-up's edge wash rises (ease-in-out) before NIGHT17's 100 ms fade (round 1: a 40 ms attack still read as a pop)
        public const float Hud18SmallSlide = 24f; // px a small text (EXPOSED) slides along the slant as it enters
        // The style meter: a rank-up pops the letter from small past its size (NIGHT17 held it 100 ms at 2.2x white-hot: the
        // reviewers' "grey ghost"), white for its first frames; the callouts ease down a slot as a new line enters.
        public const float Hud18RankUpFrom = 0.6f; // the letter's scale on the rank-up's frame ...
        public const float Hud18RankUpPeak = 1.25f; // ... its peak ...
        public const double Hud18RankUpPeakAt = 0.45; // ... at this share of Hud18RankUpIn ...
        public const double Hud18RankUpIn = 0.15; // ... settled by this many s
        public const double Hud18RankUpWhite = 0.05; // s white-hot (three frames at 60 Hz; FLASH INTENSITY 0: CYAN-HOT)
        public const float Hud18CalloutFollow = 25f; // per s: an older callout's ease down to its new slot, its size with it (~0.1 s)
        public const float Hud18CalloutPxSnap = 0.25f; // px: its eased size snaps to the slot's within this

        // The round's CLEAR: a living hold (it creeps along the slant instead of sitting still over the kill).
        public const float Hud18ClearDrift = 14f; // px along the slant over its hold
        public const float Hud18ClearGrow = 0.04f; // scale it gains over its hold (1 -> 1.04)
    }
}
