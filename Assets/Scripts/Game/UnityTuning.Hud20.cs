// NIGHT20 U3-34.10 (builder H): the HUD's display numbers for tonight's things (none is a gameplay number: the shockwave's
// cooldown, the zoom's magnifications and the regen's segment come from the design blocks through HudReads20). Px at 1080p,
// s of frame time. GG (2026-10-06 evening): the shockwave on the THUNDERCLAP with a long cooldown, zoom on most guns, regen
// at the round's end; NIGHT20.md U3-34.3 / .5 / .10: the cooldown ring around the LT glyph on the THUNDERCLAP's card
// (visible while holstered), its seconds above 3 s, a 0.2 s cyan flash on ready; the zoom's reticle and the LONGWAVE's
// scope; the segment ticks and the round-end refill. S14: nothing persistent in the central 40 % but the crosshair class.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the shockwave's chip: a ring around the alt-fire glyph, on a tab standing on the weapon card over the THUNDERCLAP's
        // slot. Apex / Overwatch's ability icon (a radial wipe with the seconds inside) on the card the gun lives on, so it reads
        // while the THUNDERCLAP is holstered. The ring fills clockwise from 12 o'clock with the next charge's progress. Above the
        // card (not in its header row) so it never meets the role text and the card keeps its width (Hud20Tests: S14).
        public const float Hud20ShockChipD = 40; // px: the chip's diameter ("RMB" at S18 fits its 34 px inner disc)
        public const float Hud20ShockRingW = 3; // px: the ring's thickness (the stamina pips' hard edge, not a hairline)
        public const float Hud20ShockTabPad = 4; // px of the tab's INK round the chip (the tab stands on the card's top edge over the THUNDERCLAP's slot)
        public const double Hud20ShockSecondsAbove = 3; // s: the seconds show while more than this is left (under it: the glyph, get ready)
        public const double Hud20ShockFlash = 0.2; // s: the ready flash (the disc CYAN-HOT -> INK, ease-out; x FLASH INTENSITY)
        public const float Hud20ShockPunch = 1.3f; // the chip's scale on a charge's return ...
        public const double Hud20ShockPunchTime = 0.22; // ... back to 1 over this (ease-out; REDUCED MOTION: none)
        public const float Hud20ShockHolsterAlpha = 0.75f; // the glyph / seconds while the THUNDERCLAP is holstered (in hand: 1)
        public const int Hud20ShockArcSteps = 48; // the ring mesh's steps round a whole turn (smooth at 40 px)
        public const string Hud20ShockReadyEvent = "shockChipReady"; // FeelProbe.Event on the frame the chip's ready flash starts
        public const float Hud20ShockInnerFrom = 1.3f; // fix H1: a new inside (the seconds, the glyph) enters from this scale (+ a fade up over Hud18FadeInTime)
        public const string Hud20ShockGun = "shotgun"; // the gun the chip rides (the THUNDERCLAP: design.shockwave's LT)
        public const float Hud20ShockPipW = 7, Hud20ShockPipH = 3, Hud20ShockPipGap = 3; // the charge pips under the glyph (the FORGE's second charge)
        public const float Hud20ShockPipY = -11; // px from the chip's centre: the pips' row
        public const float Hud20ShockTextY = 1; // px: the glyph / seconds sit a hair over the centre (the pips under them)

        // --- the zoom's reticle (1.25 / 1.5) and the LONGWAVE's scope (3.5) --------------------------------------------------
        // A ring gun: a segmented ring in the crosshair's colour with an INK edge under it, its radius R0 / magnification (the
        // tighter the zoom, the tighter the ring), gaps where the crosshair's arms are; entering from Hud20ReticleFrom x with the
        // zoom's eased amount. A scope gun: a lens: a mask over everything outside its radius, a CYAN-DEEP rim, a shaded inner
        // edge (the tube's depth), a duplex reticle (thick posts outside, fine lines in) with stadia ticks, a centre dot.
        public const double Hud20ReticleR0 = 36; // px: the ring's radius x the magnification (1.25: 28.8 px, 1.5: 24 px)
        public const float Hud20ReticleW = 2; // px: the ring's line
        public const float Hud20ReticleEdge = 1; // px of INK each side of the line (legible on a bright sky)
        public const float Hud20ReticleEdgeAlpha = 0.7f; // the INK edge's alpha
        public const float Hud20ReticleGapDeg = 34; // deg: each gap, centred on an arm (4 gaps)
        public const float Hud20ReticleDot = 4; // px: the centre dot (1.5 only: the precise gun)
        public const double Hud20ReticleDotFromMag = 1.4; // the dot shows from this magnification (the KESTREL's 1.5, not the STITCHER's 1.25)
        public const float Hud20ReticleFrom = 1.4f; // the ring's scale at the zoom's start (it closes to 1 as the zoom eases in)
        public const double Hud20ScopeFromMag = 3; // a zoom of this magnification or more is a scope (the LONGWAVE's 3.5)
        public const float Hud20ScopeRadiusShare = 0.42f; // the lens' radius as a share of the canvas height (454 px at 1080)
        public const float Hud20ScopeMaskAlpha = 0.94f; // the mask outside the lens (INK-SOLID)
        public const float Hud20ScopeRimW = 3; // px: the lens' CYAN-DEEP rim
        public const float Hud20ScopeShadeW = 34; // px: the shaded inner edge inside the rim (INK, alpha 0 -> Hud20ScopeShadeAlpha outward)
        public const float Hud20ScopeShadeAlpha = 0.6f;
        public const float Hud20ScopeFineW = 2; // px: the fine cross lines (inside the posts)
        public const float Hud20ScopePostW = 6; // px: the duplex posts (the outer part of each line)
        public const float Hud20ScopePostShare = 0.55f; // the posts start at this share of the radius from the centre
        public const float Hud20ScopeGap = 10; // px: the fine lines stop this far from the centre (the dot sits in the gap)
        public const int Hud20ScopeTicks = 4; // stadia ticks per half line (the horizontal and the lower vertical: holdover / lead)
        public const float Hud20ScopeTickStep = 34; // px between ticks
        public const float Hud20ScopeTickLen = 10; // px: a tick's length
        public const float Hud20ScopeFrom = 1.18f; // the lens' scale at the scope's start (it settles to 1 with the eased amount)
        public const int Hud20ScopeArcSteps = 128; // the lens' mesh steps (smooth at 454 px)

        // --- the regen's segments on the vitals plate, and the round-end refill -------------------------------------------------
        // design.regen.n20: in a round the passive regen climbs only to the top of the current segment; at a round's clear the
        // bar refills over the break's first seconds. The HP bar's quarters become a tick every segment (25 HP: 4 at 100, 6 with
        // the PLATE's 150); the part the regen will still give back (HP to the segment's top) shows CYAN-DEEP behind the WHITE
        // fill (Bloodborne's rally, Halo's segmented health: what you can still win back reads at a glance); the refill shows its
        // target as a CYAN-PALE promise ahead of the fill, each tick it crosses flashing CYAN-HOT.
        public const int Hud20MaxTicks = 15; // ticks made at most (a 400 HP bar at 25)
        public const double Hud20TickFlash = 0.14; // s a crossed tick shows CYAN-HOT on the refill
        public const double Hud20RefillLinger = 0.35; // s the promise stays after the bar reached it (then it fades ...)
        public const double Hud20RefillFade = 0.25; // ... over this (ease-in)
        public const double Hud20RefillIn = 0.12; // s: the promise's entrance (alpha, ease-out)
        public const string Hud20RefillEvent = "refillStart"; // FeelProbe.Event on the frame the refill's promise appears
        public const string Hud20TickEvent = "refillTick"; // ... and on the frame a tick lights

        // --- F1 the announcer (design.adds.announcer): one slab per run beat ----------------------------------------------------
        // No audio from us (CLAUDE.md rule 7), so a text slab carries what CoD's announcer carries (Research/2026-10-06-
        // additions-backlog.md F1: UT's announcer, CoD's voiced beats). One slab at a time at the banner's place (the banner
        // waits under it), <= 3 words, a priority queue, at most one start per Hud20SlabGap s (a boss beat cuts in).
        public const double Hud20SlabLife = 1.5; // s a slab stays (the backlog's "at most one slab per 1.5 s")
        public const double Hud20SlabFadeAt = 1.15; // s: its fade starts (ease-in)
        public const double Hud20SlabGap = 1.5; // s between two slabs' starts at least (a boss beat excepted)
        public const double Hud20SlabMaxWait = 2.5; // s a queued beat may wait; older, it's stale and dropped (a boss beat excepted)
        public const int Hud20SlabQueue = 4; // beats queued at most
        public const double Hud20SlabWhite = 0.05; // s its text is WHITE-hot over the accent's tint (three 60 Hz frames; FLASH INTENSITY 0: none)
        public const float Hud20SlabPadX = 28; // px of plate each side of the words
        public const float Hud20SlabH = 76; // px: the plate's height (S56 words)
        public const float Hud20SlabTop = 160; // px under the top edge: the plate's top (the banner's line: Ui.cs puts the banner at 170)
        public const int Hud20LastOneFrom = 4; // LAST ONE only in a round that had at least this many enemies left at once
        public const string Hud20SlabEvent = "announcerSlab"; // FeelProbe.Event on the frame a slab starts

        // --- A8 the NEXT line (design.adds.next): the break's forecast under the round line ------------------------------------
        // Hades shows what's behind each door; CoD's round number is the clock (the backlog's A8): in the break, under the top
        // line, the nearest special round in words (NEXT: HOLD / SWARM IN 3 / EXFIL OPEN) and five pips for rounds +1 .. +5.
        public const float Hud20NextY = 98; // px under the top edge: the line's centre (the round line spans 56 .. 88)
        public const float Hud20NextPipW = 22, Hud20NextPipH = 10, Hud20NextPipGap = 5; // a pip, slanted (UiShape)
        public const float Hud20NextPipBossH = 16; // a boss round's pip stands taller
        public const float Hud20NextGap = 14; // px between the words and the pips
        public const double Hud20NextIn = 0.18; // s: the line's entrance at the break's start (alpha + a slide along the slant)

        // --- C1 the debut cards (design.adds.debut): a kind's first appearance in the profile --------------------------------------
        // DOOM Eternal stops on a new demon to name its weak point; Hodent's one-at-a-time onboarding (the backlog's C1): the
        // kind's name and one counter (<= 4 words), once per profile, only when nothing is within 8 m, never in a boss intro.
        public const double Hud20DebutClear = 8; // m: no enemy nearer than this while a card shows (the backlog's 8 m)
        public const double Hud20DebutLife = 1.2; // s: the card (the backlog's 1.2 s; the world slowed 1.5 s at most)
        public const double Hud20DebutFadeAt = 0.9; // s: its fade starts (ease-in)
        public const float Hud20DebutTop = 252; // px under the top edge: the card's top (under the slab / banner line)
        public const float Hud20DebutH = 92; // px: the card's plate (an S42 name over an S24 counter)
        public const float Hud20DebutPadX = 28; // px each side of the widest line
        public const string Hud20DebutEvent = "debutCard"; // the bus event a card emits (port-only, for GG's sounds; payload DebutCardEvent)

        // --- the climb's off-screen arrow (design.routes; N's `climb` events) -------------------------------------------------
        // A DANGER chevron pinned to the screen's edge toward a climber you can't see (above, below or behind you), from its
        // tell ("start") to its mantle ("top") or its fall ("knocked"); the on-screen climber has N's own tell instead.
        public const float Hud20ClimbArrowInset = 72; // px from the screen's edge to the chevron's centre (inside the safe inset)
        public const float Hud20ClimbArrowW = 30, Hud20ClimbArrowH = 22; // px: the chevron
        public const float Hud20ClimbArrowNotch = 0.45f; // the notch's depth as a share of the height (a chevron, not a triangle)
        public const int Hud20ClimbArrows = 4; // arrows at most (the climbers followed at once)
        public const double Hud20ClimbArrowMax = 6; // s: a climb never reported done stops being followed
        public const double Hud20ClimbArrowPulse = 0.5; // s: one pulse of the chevron's alpha (0.65 .. 1; REDUCED MOTION: steady)
        public const float Hud20ClimbArrowPulseLow = 0.65f;

        // --- the earned heal's "+N" (design.regen.n20's earned heals: a close kill, a punch kill) ---------------------------------
        public const double Hud20HealMerge = 0.4; // s: heals this close sum into one pop
        public const double Hud20HealLife = 0.9; // s the pop stays (fading out over its last Hud20HealFade)
        public const double Hud20HealFade = 0.3;
        public const float Hud20HealRise = 14; // px it drifts up over its life
        public const float Hud20HealX = 112; // px right of the HP numeral's left edge (past a three-digit S42 "100")
        public const float Hud20HealPunch = 1.4f; // its scale on a heal (back to 1 over Hud20HealPunchTime, ease-out)
        public const double Hud20HealPunchTime = 0.15;
        public const string Hud20HealEvent = "earnedHealPop"; // FeelProbe.Event on the frame an earned heal's pop starts

        // --- the boss bar and the elite's plate: one threat bar at the bottom centre (Souls' boss bar; the backlog's A1 / A4) ----
        // The boss (A1 BULWARK, E) and the named elite (A4, E: a CHARGER with two affixes) share one place: its name (and the
        // elite's affixes) over a slanted bar, DANGER fill (an enemy's), a WHITE chunk where a hit took HP that holds, then drains
        // to the fill; a phase tick (BULWARK's 50 %). Bottom centre, between the vitals plate and the card, out of S14's box; its
        // width shrinks on narrow screens and at a large HUD SCALE so it never meets the vitals (Hud20Layout.ThreatBar).
        public const float Hud20ThreatBossW = 640, Hud20ThreatEliteW = 440; // px: the bar's width at most
        public const float Hud20ThreatBossH = 16, Hud20ThreatEliteH = 10; // px: the bar's height
        public const float Hud20ThreatBottom = 64; // px over the screen's bottom edge: the bar's foot
        public const float Hud20ThreatNameH = 30; // px: the name's line over the bar
        public const float Hud20ThreatNameGap = 4; // px between the name and the bar
        public const float Hud20ThreatSideGap = 20; // px kept between the bar and the vitals plate at any HUD SCALE
        public const double Hud20ThreatTrailHold = 0.35; // s the chunk stays WHITE and whole after a hit
        public const double Hud20ThreatTrailRate = 0.6; // bar shares per s the chunk drains at, after its hold
        public const double Hud20ThreatSweep = 0.5; // s: the bar fills in as it enters (ease-out)

        // --- F3 the reward reveal (design.adds.reveal): after a boss, an elite or a hunt, in the break: pick 1 of 3 ----------------
        // VS's chest (a slot-machine reveal), Hades' 1 of 3, CoD's box (the backlog's F3): three slots turn over in sequence
        // (0.8 s in all), then 1 / 2 / 3, a click or the pad takes one; B / Esc skips. The gameplay numbers (the points, the seed's
        // salt) are design.adds.reveal's.
        public const double Hud20RevealTime = 0.8; // s: every slot whole (the backlog's 0.8 s three-slot reveal)
        public const double Hud20RevealStagger = 0.2; // s between the slots' starts
        public const double Hud20RevealSlotIn = 0.4; // s: one slot's turn-over (ease-out)
        public const float Hud20RevealCardW = 340, Hud20RevealCardH = 300, Hud20RevealCardGap = 40; // px
        public const float Hud20RevealFlip = 0.35f; // a slot's starting x scale (it turns over into place)
        public const float Hud20RevealKeyX = 18, Hud20RevealKeyY = 42; // px from the card's top-left: its key chip's left edge and middle (under the bar)
        public const string Hud20RevealEvent = "rewardReveal"; // the bus event when the reveal opens (port-only, for GG's sounds)
        public const string Hud20RevealPickEvent = "rewardPick"; // ... and when a slot is taken (or skipped: kind -1)

        // --- the LANCER's off-screen tell (design.adds.lancer, builder E's port of lancer.js) -----------------------------------
        // CoD's sniper glint / Halo's threat marker: while a LANCER you can't see aims at you (its 1.2 s laser), a DANGER
        // chevron at the screen's edge points at it, from Hud20LancerFrom alpha to whole at the shot, swelling in the aim's
        // last Hud20LancerSwellFrom share; on screen its own laser (E's view) is the tell.
        public const float Hud20LancerFrom = 0.45f; // the tell's alpha on the aim's first frame (an entrance: <= 0.6 of its peak)
        public const float Hud20LancerSwell = 1.3f; // its scale at the shot
        public const double Hud20LancerSwellFrom = 0.7; // the aim's share from which it swells
        public const float Hud20LancerW = 40, Hud20LancerH = 30; // px: the chevron (larger than the climb's: a shot is coming)
        public const int Hud20LancerArrows = 3; // tells at most (design.adds.lancer.max is 6; three at once already reads)

        // --- E3 the CODEX tab inside WEAPONS (design.adds.codex) ----------------------------------------------------------------
        public const float Hud20CodexTabW = 140, Hud20CodexTabH = 44, Hud20CodexTabGap = 12; // px: the GUNS / CODEX tabs by the head
        public const float Hud20CodexRowH = 96; // px: an entry (an S32 name over an S18 line)
        public const float Hud20CodexHeadH = 44; // px: a section's head row (its count)
        public const float Hud20CodexSilW = 220, Hud20CodexSilH = 26; // px: a locked entry's name as a dark slanted bar
        public const double Hud20CodexScan = 0.5; // s between the in-run scans for kinds met
        public const string Hud20CodexLockedEnemy = "NOT MET YET"; // a locked enemy's line


        // --- A1 the BULWARK's plates on its bar (UiThreat20.cs: BossBar.pips / pipsOn) ------------------------------------------
        public const double Hud20PipFlash = 0.3; // s a lost plate's pip stays WHITE and punched before it goes dark
        public const float Hud20PipPunch = 1.45f; // its scale on the loss (ease-out back to 1; REDUCED MOTION: none)
        public const float Hud20PipW = 18, Hud20PipH = 12, Hud20PipGap = 5; // px: a plate's pip (a slanted cell, the alt meter's)
        public const int Hud20PipMax = 6; // pips at most (the BULWARK has 5 plates)

        // --- F4's player half: WHAT GOT YOU's tell (UiDownTell20.cs, design.adds.downtell) ---------------------------------------
        public const float Hud20TellPlateH = 44; // px: the tell's INK plate (the S24 words with room; its CYAN foot inside)
        public const float Hud20TellPadX = 18; // px each side of the words
        public const float Hud20TellGap = 12; // px: the tell plate under the replay's block / under DOWN's KILLED BY row
        public const float Hud20TellKillGap = 14; // px: KILLED BY to the kind's DANGER plate on DOWN
        public const float Hud20TellLift = 18; // px the replay's tell rises into place over its fade (REDUCED MOTION: none)
        public const double Hud20TellRecapDelay = 0.5; // s into the replay before its tell enters (the heading, the killer, then the lesson)

        // --- row 5: the package effects (HudFx20; Skygrind.HudFx20: UIEffect + LitMotion), used in three places only -------------
        public const bool Hud20FxOn = true; // the one switch (false: the HUD as before the packages)
        public const float Hud20FxCharStagger = 0.028f; // s between two characters' reveals (a 13-letter slab whole in ~0.45 s)
        public const float Hud20FxCharIn = 0.12f; // s: one character's fade and rise (ease-out cubic)
        public const float Hud20FxCharRise = 14f; // px a character rises into place
        public const float Hud20FxEdgeWidth = 2f; // px: the plate's edge (UIEffect's 8-way outline in CYAN, additive)
        public const float Hud20FxEdgeAlpha = 0.85f;
        public const float Hud20FxShineTime = 0.5f; // s: the rank letter's shine sweep (UIEffect's Shiny transition 0 -> 1)
        public const float Hud20FxShineWidth = 0.35f; // the shine band's width (UIEffect's transitionWidth, a share of the letter)
    }
}
