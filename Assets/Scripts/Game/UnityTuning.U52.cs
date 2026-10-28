// NIGHT52 builder U (the polish lane; NIGHT52.md section 4): the render and layout numbers of the three view rows design.n52.hud52, skin52 and
// guns52 (NightRows.ViewOn; off = NIGHT51's look exactly; -skygrindOff52 turns them off). Unity-only numbers: the sim never reads them, so golden
// vectors, replays and fingerprints are untouched. Colours are the one palette's (UnityTuning.Look49.cs / UiStyle33: bone, cyan, red-orange, ink).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- hud52: the health / charge block and the weapon block as hard-edged ink panels (UiHud52.cs) -------------------------------------------
        public const float Hud52LowFrac = 0.30f; // below this health share the fill, the number and the panel's edge turn red-orange (the brief: "red-orange only below 30 %"); above it the fill is bone
        public const float Hud52PlateAlpha = 0.66f; // the panel's ink plate (UiTheme.InkSolid at this alpha): dark enough that bone and cyan read on any wall, light enough that the world shows through (w33's "filled HUD box" was an empty teal panel; this one is ink, tight to its contents)
        public const float Hud52ShadowPx = 5f; // the hard drop shadow: a solid ink copy of the plate this many px right and down (no blur: the ULTRAKILL / DOOM hard edge)
        public const float Hud52ShadowAlpha = 0.85f; // ... its alpha
        public const float Hud52EdgeH = 3f; // px: the lit rule along the panel's top edge (the state colour: bone at rest, red-orange on a hit or low, cyan on a heal)
        public const float Hud52EdgeRestAlpha = 0.85f; // the rule's alpha at rest
        public const float Hud52AccentW = 6f; // px: the accent blade on the panel's outer edge (left for health, right for the weapon), the counter43 accent's width family
        public const float Hud52PadL = 20f, Hud52PadR = 24f, Hud52PadB = 12f, Hud52PadT = 12f; // px: the health panel's padding round its contents (the 12 deg lean moves the edges ~8 px at the top and bottom: the accent stays clear of the digits)
        public const float Hud52NumCapShare = 0.86f; // the number's glyph top over its box bottom, as a share of its px (the condensed italic's cap height + baseline gap): the panel's top hugs the digits
        public const string Hud52NumFit = "000"; // the panel's left edge fits this many digits (3: 100, 130 with overheal) so the panel does not jump as the count changes
        public const float Hud52GhostAlpha = 0.9f; // the lost-health after-image (hud47's ghost) in red-orange at this alpha: damage is danger (hud43 round 2: a white after-image vanished against the bone fill)
        public const float Hud52OverWhite = 0.2f; // the overheal layer: the usable cyan this far toward white (it reads over the bone fill)
        // the hit shake (game-feel: decaying trauma, shake = trauma^2, smooth sines, never a random jitter; the panel moves, never the sim)
        public const float Hud52TraumaBase = 0.30f; // trauma added by any hit (a chip still twitches the panel)
        public const float Hud52TraumaPerShare = 2.5f; // ... plus this x the share of the max lost in that frame (a 20 % hit adds 0.5: 0.8 in all)
        public const float Hud52TraumaDecay = 2.6f; // trauma lost per s (a full 1.0 settles in 0.38 s: juice returns to rest)
        public const float Hud52ShakePx = 10f; // px at trauma 1 (x), 0.7 of it on y; x the CAMERA SHAKE setting (accessibility)
        public const float Hud52ShakeRollDeg = 1.2f; // deg of roll at trauma 1, about the panel's centre
        public const float Hud52ShakeRate = 34f; // the shake's phase rate (rad / s scale of the three incommensurate sines 1.7, 2.3, 1.1)
        public const double Hud52HitTime = 0.25; // s: the edge's red-orange flash on a hit decays over this
        // the heal pulse (an eased pop with overshoot, a cyan flash that settles)
        public const double Hud52HealTime = 0.40; // s: the cyan flash on the healed segment, the fill and the edge fades over this (ease-out)
        public const double Hud52PopTime = 0.22; // s: the panel's pop on a heal (1 -> Hud52PopScale -> 1, the punch curve)
        public const float Hud52PopScale = 1.06f; // the pop's peak scale, about the panel's centre
        public const float Hud52HealFillMix = 0.55f; // at the heal's peak the bone fill is this far toward cyan
        public const float Hud52BreatheHz = 1.2f; // low health: the edge and the accent breathe red-orange (Ui43BreatheHz, the one idle loop rate)
        public const float Hud52LowDim = 0.45f; // ... between red-orange at this alpha and full
        // the weapon block (bottom-right): the same panel, mirrored (the accent on the right edge)
        public const float Hud52WPadL = 24f, Hud52WPadR = 28f, Hud52WPadB = 12f, Hud52WPadT = 10f; // px round the rail, the LT label and the name (right: the mirrored lean pulls the top-right edge ~12 px inward over the name)
        public const float Hud52WTopGap = 10f; // px the panel stays under the grapple row (hud47 moved it over the weapon name at Hud47GrappleY)
        public const float Hud52KickPx = 12f; // px: a weapon swap kicks the panel outward (right) and it settles back (OutBack)
        public const double Hud52SwapTime = 0.24; // s: the swap's kick and the edge's cyan flash
        // round 11 (GG's "looks so AI generated", the rest of the HUD; UiHud52.Top.cs): the counter, the score and the room title in the same family
        public const float Hud52CPadL = 16f, Hud52CPadR = 20f, Hud52CPadT = 10f, Hud52CPadB = 10f; // px round the counter's count, word and drain line (top-left)
        public const float Hud52CLineH = 6f; // px: the counter's drain line under hud52 (hud47's 3 px underline becomes a bar inside the plate, the health bar's family)
        public const float Hud52PPadL = 18f, Hud52PPadR = 14f, Hud52PPadT = 4f, Hud52PPadB = 8f; // px round the score (top-right; its accent on the outer, right edge)
        public const float Hud52PCapTop = 0.10f; // the score's glyph top under its box's top, as a share of its px (top-aligned digits): the plate's top hugs it
        public const float Hud52SPadX = 22f, Hud52SPadT = 6f, Hud52SPadB = 8f; // px round the room title (the message slot's line, top-centre)
        public const float Hud52SCapTop = 0.14f, Hud52SBase = 0.92f; // the title's glyph top and baseline under its box's top, as shares of its px (top-aligned caps)
        public const float Hud52SShadowPx = 3f; // px: the title plate's hard shadow (the same family, smaller)
        public const float Hud52SEdgeH = 2f; // px: its rule
        public const float Hud52SAccentW = 4f; // px: its accent (the line's role: bone a title, red-orange a danger line, cyan a combo)
        public const string Hud52ReelTitle = "THE NAVE"; // the a152 reel's forced room title (spire's room name: a short real title)
        public const int Hud52ReelCount = 7; // the a152 reel's forced counter (Counter43Probe.Force: "7 LEFT")
        // round 3, the room's lesson card as a chip (GG via the persona review, OWNER.md: "way too much instruction at the start of the level"; the
        // teach43 card under the room title stayed up the whole room): with hud52 on, after Teach52ChipAfterS on the card or at the first kill since
        // it opened (whichever is first) it shrinks to its verb line alone; the lesson's text and logic are untouched (view only)
        public const double Teach52ChipAfterS = 6.0; // s the full card stands before it becomes a chip
        public const float Teach52ChipScale = 0.55f; // the chip's scale of the card (the verb line 34 px -> ~19 px)
        public const float Teach52ChipMinPx = 17f; // ... never under this on screen (px at 1080p; CLAUDE.md rule 8: >= 16): a shrunk head line's scale is raised to keep it
        public const double Teach52ChipEaseS = 0.25; // s: the shrink's ease (out-cubic: everything moves a little, U-5)
        public const string Teach52ReelLine = "Q: KESTREL far, GAUNTLET close. Swap to the gun that fits the range."; // the a152 reel's lesson (a room card's shape: a verb line and a rule)
        public const string Teach52ReelLineKill = "Ride the long wall to the high ledge. Jump at its end."; // ... its second line (the kill path)
        public const float Teach52ReelChipShare = 0.6f; // the a152 reel: the chip is at most this x the card's full height on screen
        // round 14 (NIGHT52.md leftover 5, UiTeach52.Verb.cs): a teach35 line whose verb the view can hear stays until the player does it (hud52 on)
        /// <summary>A teach35 line id -> the verb that ends it (a Mover event: dash, slide, wallride, walljump, grapple, jumpdash; a sim event:
        /// fire, fire:&lt;gun&gt;, charge, whip, punch). A line not listed (l1.zoom, l4.bend, w.torch: no event to hear) keeps NIGHT51's timer.</summary>
        public static readonly System.Collections.Generic.Dictionary<string, string> Teach52Verbs = new System.Collections.Generic.Dictionary<string, string>
        {
            { "l1.dash", "dash" }, { "l1.slide", "slide" }, { "l1.fire", "fire" }, { "l1.charge", "charge" }, { "l2.wallride", "wallride" },
            { "l2.walljump", "walljump" }, { "l3.charge", "charge" }, { "l4.grapple", "grapple" }, { "l5.jumpdash", "jumpdash" }, { "l5.whip", "whip" },
            { "w.shotgun", "fire:shotgun" }, { "w.nailgun", "fire:nailgun" }, { "w.gauntlet", "punch" },
        };
        public const double Teach52VerbSafetyS = 20.0; // s: a line waiting for its verb ends after this long anyway (a player who never does it is not stuck with it)
        public const double Teach52VerbMinS = 1.0; // s: the line stands at least this long even when the verb comes at once (no one-frame flash)
        public const string Teach52ReelVerbId = "l1.fire", Teach52ReelVerbLine = "PRESS {fire} to FIRE the KESTREL."; // the a152 reel's forced line (teach35's own l1.fire)
        public const double Teach52ReelVerbWaitS = 8.0; // s: the a152 reel holds the verb back this long (past NIGHT51's 5 s timer) before it fires
        public const int Teach52ReelVerbClearFrames = 10; // the a152 reel: the line must be gone within this many frames of the verb (counted from the first "fire" heard)
        // round 16 (n52-r20: 6 taps of 3 frames with the THUNDERCLAP, no "fire" heard): the KESTREL with a252's kill-wash press (1 frame, every 30 frames)
        public const int Teach52ReelTapFrames = 1; // the a152 reel's press holds the trigger this many frames, then releases (round 15: 3)
        public const int Teach52ReelTapEvery = 30; // ... and presses again this many frames later until a "fire" is heard (round 15: 20)

        // ---- skin52: the enemy surface (Skin52.cs, Assets/Shaders/Enemy.shader's _Skin52* globals) ---------------------------------------------------
        public const float Skin52RimNearM = 3f; // m: at this distance and nearer the rim is cut to Skin52RimNearShare (the close-up jelly read: fx.enemyRimColor x the crunch boost glowed over the whole body at arm's length)
        public const float Skin52RimFarM = 14f; // m: from here on the rim is the material's (NIGHT51's), and its far boost from EnemyRimBoostNear (15 m) is unchanged: the far read is kept
        public const float Skin52RimNearShare = 0.3f; // the rim's strength at the near distance (a share of NIGHT51's)
        public const float Skin52RimNearPower = 5f; // the rim's power at the near distance (fx.enemyRimPower 2.2 from the far one): a thin edge light, not a halo over the body
        // the ramp's key light (weights of a direction). Round 2 (n52-r2 a252: the round-1 key, mostly toward the camera, put the terminator at 57
        // deg round a pill's side: only the outer 16 % of its width in shadow, left - right 0.003 -> 0.009): now mostly from the camera's left, so the
        // terminator runs ~25 deg right of the pill's middle (the right ~30 % in shadow) and the underside is shaded: a two-tone volume
        public const float Skin52KeyUp = 0.55f; // the weight of up (the sky over the arena: tops lit, undersides in shadow)
        public const float Skin52KeyToward = 0.30f; // ... of toward the camera (flat) (round 3: 0.35)
        public const float Skin52KeySide = -0.75f; // ... of the camera's right (negative: the key from the left; the shadow on the right and below)
        // round 3 (n52-r5 a252: the terminator read on the frames but the shadow was 0.117 / 0.104 of the body, under the 0.12 floor): the
        // terminator moves to N.L 0.15 (and toward 0.30), ~11 deg right of a pill's middle: the right ~40 % of its width and its underside in shadow
        public const float Skin52RampMid = 0.15f; // the terminator: N.L at the ramp's middle (NiloCat's _CelShadeMidPoint; round 2: 0.05)
        public const float Skin52RampSoft = 0.03f; // the terminator's half-width (NiloCat's _CelShadeSoftness): a hard cel step, anti-aliased (round 2: 0.06 read as a soft blend)
        public const float Skin52LitGain = 1.0f; // the lit side x this (1: the body's own colour, the saturation budget the enemies carry: V-1)
        public const float Skin52ShadeR = 0.20f, Skin52ShadeG = 0.15f, Skin52ShadeB = 0.30f; // the shadow side's multiplier (linear; round 2 from 0.42 / 0.34 / 0.55, whose gamma ratio was only 0.68): red-orange's shadow is #7C1A0C-ish (OKLab L 0.38 against 0.67, hue 31.5 deg against 34.3: the hue kept, darker and cooler)
        public const float Skin52RampFullM = 8f; // m: the ramp is full to here (up close and mid-range a lit volume; round 3: 10, so the bigger shadow eases sooner and the far read keeps its margin: n52-r5 charger 0.87 x at 12.7 m)
        public const float Skin52RampFlatM = 20f; // m: from here the body is flat, NIGHT51's bright colour (the far read: colour, rim and ink carry the shape; round 3: 24)
        // round 4 (leftover 1): the scorch mark's hot core under skin52 (EnemyViews.Scorch52.cs)
        // round 9: the glow 4 (rounds 4-6: 6 over a near-black red): the core is now the danger token's colour pulled hot (linear 1 / 0.26 / 0.08),
        // at 0.5 s (heat 0.56) 2.25 / 0.59 / 0.19 HDR over the blotch: the floor's red stays clipped, green and blue rise (a yellow-orange, not white)
        public const float Skin52ScorchGlow = 4f; // the core's glow gain (Scorch51.shader _Glow 3 before)
        public const float Skin52ScorchTintMin = 0.3f; // round 9: a mark whose colour (linear) has no channel at or above this takes the danger token's for its core (a husk's dying view gives 0.10 / 0.01 / 0.00)
        public const float Skin52ScorchHeatS = 2.0f; // s the core stays hot, cooling as (1 - t)^2 (N51ScorchHeatS 1.2 before): at 0.3 s 0.72 of full, at 1 s 0.25
        public const float Skin52ScorchHotMix = 0.35f; // round 6: the mark's tint this far toward the hot colour below (the core reads as hot metal, not a saturated red whose luma sits under a lit warm floor)
        public const float Skin52ScorchHotR = 1f, Skin52ScorchHotG = 0.62f, Skin52ScorchHotB = 0.22f; // ... the hot colour (linear): a yellow-orange, the danger family's hot end (V-2), never white or cyan
        public const float Skin52ReelScorchCoreM = 0.2f; // the a252 reel (round 6): the core box's half-width on the floor (m; the core's hot disc is ~0.29 m across a husk's mark)
        public const float Skin52ReelScorchMin = 0.08f; // the a252 reel: the core's mean luma (0..1) over the same floor before the kill must rise by at least this
        public const float Skin52ReelScorchAgeS = 0.5f; // round 9: the frame when the youngest mark is this old (game s): past the kill's pop light (fx.popTime 0.12 s); the dying body stood over the mark at 0.3 s in n52-r12's OFF frames (whether it is gone by then: not checked, the frame shows it)
        public const float Skin52ReelScorchWaitS = 0.9f; // ... or this long after the kill (unscaled s) at most (the row off has no mark age to read)
        public const double Skin52ReelWashM = 3.3; // round 12: the a252 reel's kill-wash husk distance (m; past KillFarWhiteFrom 3 m: the far-white kill T-111 measured)
        public const int Skin52ReelWashFrames = 12; // ... render frames recorded after the kill's first frame
        public const double Skin52ReelWashTurnDeg = 10; // ... the OFF and ON kills on two headings this far either side of the line (fresh floor and air each)
        public const int Skin52ReelWashShotK = 6; // ... the kill's frame saved per side (NIGHT50 G4's kill wash: +0.25 on f6, +0.27 on f8, as Fx49KillWhiteFrames' comment reads)
        public const float Skin52ReelScorchTurnDeg = 14f; // the a252 reel: the OFF and ON kills on two headings this far either side of the line (each core on fresh floor)
        public const float Skin52GapDot = 0.999f; // a vertex whose own normal and its position's smoothed normal differ past this dot is a gap site (counted for the reel)
        public const float Skin52ReelBright = 0.7f; // the a252 reel: a pixel brighter than this luma (0..1) in the enemy's box counts as glow (NIGHT51's near rim tops ~0.8)
        public const float Skin52ReelShadeRatio = 0.65f; // the a252 reel: a body pixel whose luma on is under this x its luma off is in the ramp's shadow
        public const float Skin52ReelBodyRG = 1.6f, Skin52ReelBodyRB = 2f; // the a252 reel: a body pixel is red-orange in the OFF frame (R over this x G and x B; colors.enemy is 3.4 / 9; kiln's brown floor ~1.3)
        public const float Skin52ReelShadeMin = 0.12f, Skin52ReelShadeMax = 0.7f; // ... the two tones read when the shadow covers this share of the box (some, never all)
        public const float Skin52ReelFarMinM = 12f; // the a252 reel: the far pose is judged only at this distance or more (the heading's clear length permitting)
        public const float Skin52ReelFarKeep = 0.8f; // ... the far read is kept when the box's mean luma on is at least this x off

        // ---- look52 (round 5): the health pickup's silhouette, the grapple's rope and rings (Look52.cs, Pickup35View.Look52.cs) ----------------------
        public const float Look52CrossArm = 0.84f; // m: the health plus's height and width (the vial was 0.62 x 0.34: at 15 m the plus stands ~40 px at 1080p)
        public const float Look52CrossBar = 0.30f; // m: each bar's width (chunky: a plus, not a thin cross)
        public const float Look52CrossDepth = 0.22f; // m: its thickness (a solid object that catches the light, not a sign)
        public const float Look52InlayArm = 0.6f, Look52InlayBar = 0.13f; // m: the cyan plus inlaid on each face (the usable glow; never red: a protected emblem and the danger hue)
        public const float Look52InlayProud = 0.012f; // m: the inlay stands this far off each face (no z-fight)
        public const float Look52RockDeg = 28f; // deg: the plus rocks this far either side of facing the camera (a slow turn that never shows its edge)
        public const float Look52RockHz = 0.35f; // Hz: the rock's rate
        public const float Look52RopePale = 0.55f; // the ARCLINE's rope and hook: the usable cyan this far toward white (pale cyan; rule 8: cyan family, never saturated as a beam)
        public const float Look52ReelPickupMinPx = 28f; // the a252 reel: the health pickup's on-screen height at 15 m must be at least this (px at 1080p)
        public const float Look52ReelPickupAspect = 0.8f; // ... and its width at least this x its height (a plus, not a tube)
        public const float Look52ReelPickupM = 15f; // m: the reel's distance to the health pickup
        /// <summary>Round 6 (leftover 3, T-116; Look52Lift): the walls' light in the level hue (the stone's rim and the SH probe's equator) x this on
        /// the V50 levels while look52 and look49 draw there; order = UnityTuning.V50Themes (iceline, span). 1.5 linear ~ x1.2 in a wall's sRGB value
        /// (the offline pass over n50-r1's frames: combat 7 -> 16 % iceline, 4 -> 14 % span; the floor frames under 10 %); an estimate of the
        /// engine's mapping, measured by a349.
        /// Round 9: n52-r12's a349 combat ON frames at 1.5 overshot (iceline dark 29 / mid 66 / saturated 20 / bins 4, span 19 / 79 / 27 / 3: 2/4
        /// each; at 1, the grade alone: 45 / 50 / 8.2 / 6 and 49 / 48 / 5.1 / 6, 2/4). Each pixel interpolated in linear light between the two
        /// frames (scratch, not a Player): iceline 4/4 from 1.08 to 1.12 (1.12: 42 / 53 / 11.8 / 4; 1.15 fails mid at 56); span 4/4 at no lift
        /// value at all (mid <= 55 holds to k 1.10, where saturated is 7.3 %; saturated >= 10 % needs k >= 1.25, mid 67 %; hue bins 6 to k 1.3): span 1,
        /// nothing done (Look52Lift.Walls leaves a lift of 1 alone).</summary>
        public static readonly float[] Look52WallLift = { 1.12f, 1f };
        // round 8 (Look52Tier: three values by a render box's kind on spire / foundry; GG-persona review, T-112). Linear albedo scales: the floor
        // x0.65 (~x0.82 in sRGB: about -0.05 of luma on spire's ~0.3 stone), the ledges' faces x1.5 and tops x1.45 (~x1.2: +0.06), the walls x1
        // round 11 (the lead, n52-r15 a349: foundry's ledge face over same-facing walls +0.045 -> +0.125 GREEN; spire -0.079 -> -0.007 RED): foundry only
        public static readonly string[] Look52TierLevels = { "n48-foundry", "n52-foundry", "n65-foundry" }; // the levels the tiers apply on (map ids, lower case); NIGHT65 A1: n65-foundry (level 1 rebuilt behind design.n65.foundry65) as n52-foundry
        // round 9 (n52-r13 a349: x0.65 left the floor 0.085 / 0.156 OVER the walls on spire / foundry's room frames, the floor being the sunlit top
        // and a wall its side; under the walls by 0.04 needs ~x0.24 (spire room) to ~x0.07 (spire's fogged arena) albedo, extrapolated: a near-black
        // floor whose 2 m seams fall to ~0.02 of luma, estimated): the floor back to NIGHT51's, the ledge faces lifted further (x1.5 gave face - wall
        // +0.007 spire room, +0.036 foundry room: luma rose x1.36 / x1.15 for x1.5 albedo, so x2 ~ x1.70 / x1.27: ~+0.10 / ~+0.05, extrapolated)
        public const float Look52TierFloor = 1f; // the floor boxes' colour x this (round 8: 0.65; round 9: NIGHT51's, see above)
        public const float Look52TierDeckTop = 1.45f; // a ledge's (deck, island, step) top x this (lifted: the tier reads as a separate plane)
        public const float Look52TierLedgeFace = 2.0f; // a ledge's faces x this (lifted: v6's 2.4 m island face reads against the wall behind it; round 8: 1.5)
        public const float Look52ReelTierGap = 0.04f; // the a349 reel: the ledge face's mean luma (0..1) over walls facing the same way, pooled over the room / floor / arena ON frames, at least this

        // ---- guns52: the KESTREL and THUNDERCLAP viewmodels machined (BlockyKit.Guns52.cs, Viewmodel.Guns52.cs); metres at viewmodel scale ---------
        public const float Guns52Chamfer = 0.0025f; // m: a part's edge chamfer (the KESTREL's frame is 0.03 wide: an 8 % cut, a visible machined edge at arm's length; stone51's face bevel only lit the edge, this cuts it)
        public const float Guns52ChamferShare = 0.2f; // ... at most this share of the part's smallest side (a 9 mm rib gets 1.8 mm)
        public const float Guns52ChamferMin = 0.0008f; // m: a part whose chamfer would be smaller stays a plain box (a 4 mm pin, a sight post)
        public const float Guns52EdgeLight = 0.35f; // the chamfer faces' colour: the part's lifted this far toward white (the lit edge; tilted 45 deg, it also catches the key)
        public const float Guns52PanelBorder = 0.006f; // m: a cap's frame width round its recessed panel (the cap's outline inset by chamfer + this)
        public const float Guns52PanelMinArea = 0.0006f; // m^2: a cap gets a panel only if the panel is at least this big (about 2.5 x 2.5 cm: the frame and grip sides, not a sight)
        public const float Guns52PanelDark = 0.68f; // the recessed panel's colour: the part's x this (the second tone; a gunmetal side reads as a frame round a darker inlay)
        // round 3 (n52-r5 a352: the THUNDERCLAP's edge share 0.080 -> 0.083; on its frames the panels read, its lit edges barely): the big gun's build
        public const float Guns52ChamferBig = 0.005f; // m: the THUNDERCLAP's chamfer (0.72 m long, 2.4 x the KESTREL's 0.31 m: twice the cut, ~8 px at its screen size)
        public const float Guns52ChamferShareBig = 0.3f; // ... at most this share of the part's smallest side (its 8 mm top rib gets 2.4 mm)
        public const float Guns52EdgeLightBig = 0.55f; // ... its chamfer faces lifted this far toward white (the KESTREL's 0.35 read; the THUNDERCLAP's did not)
        public const float Guns52StripeMinW = 0.035f; // m: a THUNDERCLAP wall at least this wide between its chamfers (the stock's 0.05, the receiver's 0.07, the pump's 0.06) carries the two-tone stripe
        public const float Guns52StripeShare = 0.4f; // ... the stripe's width as a share of that wall (centred, in the part's darker tone, Guns52PanelDark)
        public const float Guns52MiterMax = 3f; // an outline corner sharper than this mitre (1 / cos(half its turn)) is not inset (the part stays plain: no spike)
        public const float Guns52ReelEdgeGain = 1.15f; // the a352 reel: the gun's box must show at least this x NIGHT51's strong-edge share (the chamfers and panels read)
        public const float Guns52ReelEdgeLuma = 0.06f; // the a352 reel: a strong edge is a luma step over this (0..1) to the next pixel right or up
        public const int Guns52ReelMaskStep = 12; // the a352 reel (round 3): a pixel is the gun's when the frame without the viewmodel differs from it by more than this (0..255) in a channel (over the CRT's grain)
    }
}
