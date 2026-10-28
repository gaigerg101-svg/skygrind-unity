// NIGHT47 builder F: Unity-only view numbers of F's view rows (hud47, lava47, copy47, fx47; CLAUDE.md rule 5: render / UI layout numbers live
// here, each with a comment). The specs and their research lines: Research/2026-10-18-wave47/F-cards.md (sections 1, 2, 6).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- hud47: text sizes at 1080p (1 fontSize unit = 1 px at 1080p: the canvas scales with height; F-cards 2a, XAG 101 = F.md R6) ----
        public const float Hud47TeachHeadPx = 44f; // the teach card's head line, which also carries the boss / fight lines (BossLine40): fight lines >= 36 (was 40)
        public const float Hud47TeachHeadMinPx = 36f; // its floor when a line would wrap past two lines (was 28: NIGHT47.md section 3's 36 for fight lines)
        public const float Hud47TeachSubPx = 30f; // the teach card's rule line (was 24): teach cards >= 30
        public const float Hud47TeachSubMinPx = 26f; // its floor (was 18): XAG's console body minimum 26 px
        public const float Hud47SlotPx = 36f; // the message slot (ROOM banners, DOOR OPEN, the countdown, boss lines posted there; was 24): a banner is a fight line
        public const float Hud47CornerPx = 22f; // corner labels read mid-fight: the LT label, the award feed, the rank name, run lines (were 16-18): >= XAG PC 18 + pad margin
        public const float Hud47NamePx = 28f; // the held weapon's name (was 24)
        public const float Hud47RoundPx = 26f; // the survival round line (was 20)
        public const float Hud47BossNamePx = 36f; // the boss bar's name (bossbar43; was 32)
        public const float Hud47BossLabelPx = 26f; // the boss bar's LEG / OPEN / STAGGER labels and its hint (were 16 / 18): XAG console 26
        // TEXT SIZE (ACCESS; XAG 101: scaling to 200 % of the minimum without loss): every hud47 size x this. The web had no such setting.
        public const string TextSize47Setting = "textSize47"; // the settings key (saved with the rest)
        public const string TextSize47Label = "TEXT SIZE";
        public const string TextSize47Desc = "MAKES THE GAME'S TEXT BIGGER";
        public const double TextSize47Default = 1.0, TextSize47Min = 1.0, TextSize47Max = 2.0, TextSize47Step = 0.25;
        // the fight line (the boss bar's hint under hud47; UiHud47.Boss.cs), in the upper third, apart from every other text's place
        public const float Hud47FightPx = 40f; // px em (fight lines >= 36 at 1080p)
        public const float Hud47FightY = 250f; // px from the top: its centre-top (the boss bar 54-130, the message slot 164-200 above it; the teach card below at +170 from the centre)
        public const float Hud47FightW = 1200f; // px: one line wide (<= 6 words at 40 px)
        public const double Hud47FightMinS = 1.0; // s: a new hint waits at least this after the last line began (no flicker of lines)
        public const float Hud47CardNamePx = 72f, Hud47CardLinePx = 36f; // the walk-in card's name and its one intro line under hud47 (were 56 / 20-24: R4 "boss names and one line of intro at the walk-in, big and readable"); the fight line's 40 px is the bar's
        public const float Hud47CardY = 280f; // px above the centre: the boss entrance card (Juice38) moved off the teach card's +170 (the n47-r0b overlap)
        public const float Hud47BossHintPx = 36f; // the boss bar's hint: the fight lines ride it (wick43.hints, W's NIGHT47 words): fight lines >= 36 (was 18)
        public const string Hud47CoreLine = "{punch} the core!"; // G7 (W's hand-off): the first crack-open's line, big, once a run (docs/COPY47.md: the glyph shown, sentence case, 3 words; was "PUNCH THE CORE!")
        public const double Hud47LineHoldS = 2.0, Hud47LineHoldPerWord = 0.3; // s: a fight line's hold (NIGHT47.md section 3: 2.0 s + 0.3 s a word)
        public const float Hud47UnderlayAlpha = 0.85f, Hud47UnderlayOffset = 0.6f, Hud47UnderlaySoftness = 0.3f; // the fight line's drop shadow (TMP underlay, SDF units): its shadow twin is off (n47-r1e)
        public const float Hud47Outline = 0.22f; // TMP outline width (about 2-3 px at these sizes) in the HUD's shadow ink: readable on any background (XAG: the Fenyx outline)

        // ---- hud47: one continuous health bar (F-cards 2b; GG: "a single continuous bar ... green to lighter green to lime to yellow to orange to red") ----
        // the six stops by health share, interpolated in OKLab (Bjorn Ottosson's matrices, F.md R1): hue and lightness measured tonight (F-cards 2b)
        public static readonly float[] Hud47HealthStops = { 1.00f, 0.80f, 0.60f, 0.40f, 0.20f, 0.00f };
        public static readonly uint[] Hud47HealthHex = { 0x35e05a, 0x86ec5c, 0xc4f03c, 0xffd83a, 0xff8a2a, 0xff3326 }; // green, light green, lime, yellow, orange, red
        public const float Hud47GlowAlpha = 0.35f; // the bar's soft glow: a copy in its own colour behind it at this alpha
        public const float Hud47GlowGrow = 1.6f; // the glow's height as a multiple of the bar's
        public const float Hud47GhostWait = 0.4f; // s the lighter ghost of lost health holds before it drains (NIGHT47.md section 3)
        public const float Hud47GhostRate = 0.6f; // of the max per s: the ghost's drain
        public const float Hud47GhostWhite = 0.5f; // the ghost is the bar's colour this far toward white (a lighter segment)
        public const float Hud47LowFrac = 0.25f; // below this share the bar pulses and the number brightens (not hue alone: red-green colour blindness)
        public const float Hud47LowPulseHz = 1.2f; // the low pulse (hud43's breath, kept)
        public const float Hud47LowNumberGain = 1.4f; // the number's brightness while low
        public const float Hud47LowPulseWhite = 0.35f; // the low pulse's peak: the bar's colour this far toward white (it only ever brightens: never darker than its share's colour)

        // ---- overheal47's look (F-cards 2c): 100 -> the cap, blue -> deeper blue -> purple, a layer over the full bar ----
        public static readonly uint[] Hud47OverHex = { 0x3fa0ff, 0x4a7dff, 0x7a3dff }; // OKLab h 251 / 265 / 289 (measured tonight)
        public const float Hud47OverHueMaxDeg = 295f; // the overheal hue never passes this (OKLab h): purple, never magenta
        public const float Hud47RivalHueDeg = 338f; // RIVAL's magenta 0xff2bd6 is OKLab h 338 (C 0.28): 43+ deg from the overheal's cap

        // ---- hud47: the enemy counter (F-cards 2d) ----
        // TMP's fontSize is the em; the condensed face's digits stand ~0.7 em tall: the lead measured the 48 em count at ~36-40 px on n47-r0. The
        // counter's spec (>= 32 px, F-cards 2d) is read on the glyphs, so the em is 60 (digits ~42 px) and the word 36 (caps ~25 px).
        public const float Hud47CounterNumPx = 60f; // the count's em (was 36; 48 in the first cut)
        public const float Hud47CounterWordPx = 36f; // "LEFT" / "LAST ONE" / "CLEAR" (was 16; 30 in the first cut)
        public const float Hud47CounterLineW = 220f; // px: the thin line under it that drains per kill
        public const float Hud47CounterLineH = 3f; // px
        public const float Hud47CounterLineEase = 0.2f; // s: the line's drain per kill (eased)
        public const float Hud47LastOnePulseHz = 1f; // "LAST ONE" pulses at this rate
        public const string Hud47LastOneWord = "LAST ONE"; // the word at 1 left (copy47's line; NIGHT47.md section 3)

        // ---- hud47: the reticle's bar's information on the weapon panel (F-cards 2e; PRINCIPLES 1: nothing lost) ----
        public const float Hud47PipW = 6f; // px: a PULSE kill pip beside the gauntlet's slot
        public const float Hud47PipH = 10f; // px
        public const float Hud47PipPitch = 9f; // px between pips
        public const float Hud47PipUp = 6f; // px above the slot's icon
        public const bool Hud47PipsUnderRail = true; // GG 2026-10-26: the pips hang under the rail (over it they sat in the LT label's band: "ZL ZOOM" over the pips)
        public const float Hud47PipUnderGap = 5f; // px from the rail's bottom (the hand bar) down to the pips' top
        public const float Hud47PipDimAlpha = 0.3f; // an unlit pip's alpha (the shape stays: 3 of 5 reads at a glance)
        public const float Hud47BlastLineH = 3f; // px: the BLAST's cooldown line over the pips (drains as it recharges)
        public const float Hud47HeatLineH = 3f; // px: the STITCHER's heat line above its slot
        public const float Hud47GrappleX = -72f; // px from the right edge: the grapple's hook + charges row moved off the reticle to the weapon panel
        public const float Hud47GrappleY = 176f; // px up from the bottom edge (over the weapon name)
        public const uint Hud47GauntletPinkHex = 0xff6fb0; // the gauntlet's pink (OKLab h 355, C 0.19: F-cards 1c), the pips' lit colour

        // ---- fx47: the LT blast's look, variant A "ring" (F-cards 1b; Fx47View.cs) ----
        public const int Fx47SeedSalt = 0x46583437; // "FX47": the view's cosmetic dice (Mulberry32 from the run seed; render side, never the sim's)
        public const uint Fx47CoreHex = 0xffe8f4; // the core tone (DECIDE43 D-3's FUSION core; OKLab h 345, L 0.95)
        public const uint Fx47BodyHex = 0xff6fb0; // the body tone (h 355, C 0.19: warmer and greyer than RIVAL's 0xff2bd6, h 338, C 0.28)
        public const uint Fx47RimHex = 0xd8487e; // the darker rim tone (h 1)
        public const float Fx47Glow = 2.4f; // HDR x on the three tones (the FUSION's 2.4: bloom picks them up)
        public const int Fx47Pool = 160; // sprites a sheet may draw in a frame
        public const int Fx47Motes = 24; // the gather's motes (NIGHT47.md section 3: inward particles, the glove brightening)
        public const float Fx47GatherS = 0.10f; // s of the gather (80-120 ms), played over the blast's first frames: the input is never delayed
        public const float Fx47GatherShell = 1.2f; // m: the shell the motes start on, round the glove
        public const float Fx47CoreS = 0.033f; // s of the core flash (1-2 render frames at 60 fps)
        public const float Fx47CoreSize = 0.12f; // m across at the glove (n47-r1b: 0.55 m with a 1.8x body covered the reticle's centre box at +2; the glove's
                                                 // point is 0.30 m off the view axis at 0.55 m, so the body (Fx47CoreBodyX) reaches 0.20 m from it: ~20 deg clear)
        public const float Fx47CoreBodyX = 1.6f; // the core's soft body x its size (was 1.8 of 0.55)
        public const float Fx47NearEye = 1.5f; // m: no debris chunk, burst chunk or burst shell drawn nearer the eye (n47-r1b: slabs in front of the camera)
        // ---- R4 finale47 (UiHud47.Finale.cs, Fx47View.Weapons.cs OnFinale47): render and layout numbers only ----
        public const float Finale47TitlePx = 72f; // px at 1080p: the boss's name DOWN (the HUD's biggest line: the boss bar's name is 36)
        public const float Finale47TitleY = 300f, Finale47TitleW = 1400f; // px from the top to its centre (upper third, clear of the reticle), its box's width
        public const string Finale47Word = "down"; // the word after the name ("THE ICEBREAKER DOWN"; uppercased with it)
        public const double Finale47InS = 0.12, Finale47HoldS = 2.3, Finale47OutS = 0.45; // s: the slam in (1.25x to 1x), the hold (2.0 s + 0.3 a word for 1-2 words), the fade
        public const double Finale47Light = 8, Finale47LightRange = 30, Finale47LightLife = 0.35; // the arena's flare (a LightPools flash: intensity, m, s)
        public const float Finale47RingM = 12f, Finale47RingS = 0.6f; // m, s: the floor ring out from the boss (the second 1.3x wider, 1.4x longer)
        public const float Finale47KickDeg = 3f, Finale47RollDeg = 2f, Finale47SettleS = 0.6f; // the camera's hit: the heavy tier's (3-5 deg, settle <= 0.7 s), up (Kicks47: + is up)
        public const float Finale47Trauma = 0.4f; // trauma added (the heavy tier's +0.5 less: the slow motion carries the rest)
        public const float Fx47AimRingClear = 0.12f; // the aim ring's radius and the lance's streak offsets at least this x their distance (about 7 deg: outside the reticle's 10 % centre box)
        public const float Fx47ChunkMaxScreen = 0.08f; // a kill's block (Chunks.cs) under fx47 stands at most this share of the screen's height (n47-r1d: a 3 m kill's blocks were slabs)
        public const float Fx47ChunkMin = 0.1f, Fx47ChunkMax = 0.3f; // m: a debris / burst chunk's size (n47-r1b: "small chunks 0.1-0.3 m")
        public const float Fx47ChunkFrom = 2f; // m: the line's debris starts this far ahead on the floor (it was 1.05 m: in the face)
        public const int Fx47BurstChunks = 10; // chunks a launch36 burst throws under fx47 (instead of Tell36View's 40 cube bits at 11 m/s, which flew into the eye)
        public const float Fx47BurstChunkLife = 0.5f; // s
        public const float Fx47ShellMaxShare = 0.34f; // a burst shell's drawn radius under fx47 is at most this x its distance from the eye (about 20 deg: never the screen)
        public const float Fx47WashCentreRise = 0.25f, Fx47WashBrightRise = 0.25f; // a747's wash check (a reel's bar, not a look): the ON blast's centre-box mean may rise at most this over the pass's f-2, its bright (> 0.85) share this many points
        public const float Fx47HaloNearFade = 1.2f; // m: under fx47 a tell halo fades out as the eye comes within this of its surface (n47-r1b: the +8 / +30 wash)
        public const float Fx47GloveFwd = 0.55f, Fx47GloveRight = 0.22f, Fx47GloveDown = 0.20f; // m: the glove's point in the view (low right)
        public const float Fx47WaveS = 0.20f; // s for the shockwave ring to reach the blast's reach (NIGHT47.md: 0 to the radius in 0.2 s)
        public const float Fx47WaveFadeS = 0.15f; // s the ring then fades
        public const float Fx47Reach = 7f; // m: the floor ring's last radius (design.n47.pulse47's range 7; the view does not read the sim's number: it is a look)
        public const float Fx47AimRing = 2.2f; // m: the ring along the aim at its widest (pulse47's line radius 2.2)
        public const float Fx47FloorLift = 0.06f; // m over the feet (no z-fight with the floor)
        public const int Fx47Debris = 12; // debris chunks thrown up from the floor along the line
        public const float Fx47DebrisFrom = 0.03f, Fx47DebrisLife = 0.5f; // s
        public const int Fx47Embers = 30; // embers drifting up
        public const float Fx47EmberLife = 0.5f; // s (NIGHT47.md: embers 0.5 s)
        public const float Fx47Life = 0.6f; // s: the whole picture
        public const float Fx47Trauma = 0.5f; // trauma added at the blast (NIGHT47.md: trauma +0.5; heavy tier)
        public const float Fx47KickDeg = 3.5f; // deg of the camera's pitch kick (heavy tier 3-5)
        public const float Fx47KickRiseS = 0.03f, Fx47KickSettleS = 0.45f; // s: the kick's rise and its settle (heavy: settle <= 0.7 s)

        // ---- grapple47: the glowing ribbon (F-cards 1e; Arcline21View.Grapple47.cs) ----
        public const float Grapple47CoreW = 0.032f; // m: the core's width at the hand (the rope was 0.07 m flat; n47-r1b: 0.018 read as a 1-2 px hairline at 15 m)
        public const float Grapple47TipShare = 0.5f; // the core's width at the hook as a share of the hand's (0.016 m): it tapers
        public const float Grapple47ShellMul = 4f; // the glow shell's width as a multiple of the core's (NIGHT47.md: 3-4x; its soft falloff makes the lit part narrower)
        public const float Grapple47CoreWhite = 0.6f; // the core's cyan this far toward white (a white-cyan core)
        public const float Grapple47CoreGlow = 3.0f; // HDR x on the core (bloom picks it up)
        public const float Grapple47ShellGlow = 2.2f; // HDR x on the cyan shell (n47-r1b: 1.2 flat read as no glow; with the Gaussian falloff its middle is this bright)
        public const int Grapple47GlowTexH = 32; // px across the made glow texture (the shell's and the pulse's falloff in v)
        public const float Grapple47GlowFalloff = 4.5f; // the Gaussian's exp(-k v^2) across the width (v -1..1): about 1 % at the edges
        public const float Grapple47WaveAmp = 0.12f; // m: the fire's wave at its start
        public const float Grapple47Waves = 2.5f; // waves along the line while it flies (NIGHT47.md: 2-3)
        public const float Grapple47WaveS = 0.15f; // s: straight by then
        public const float Grapple47PulseS = 0.25f; // s between pull pulses (hook -> hand)
        public const float Grapple47PulseLen = 1.2f; // m: a pulse's length (n47-r1b: 0.6 m was a speck at 15 m)
        public const float Grapple47PulseWidthMul = 3.0f; // the pulse's width as a multiple of the core's (soft-edged with the glow texture)
        public const float Grapple47PulseGlow = 3.5f; // HDR x on the pulse (white)
        public const float Grapple47BendGain = 1.6f; // inside the BEND the line brightens this much (its pulse slows with the bend's time)

        // ---- fx47: the punch aura (G21, the player's side; F-cards 1c: three tones of one hue + a halo; OKLab measured tonight) ----
        public const uint Fx47FistCoreHex = 0xd8f6ff, Fx47FistBodyHex = 0x46c8ff, Fx47FistRimHex = 0x1e5cff; // the fist: cyan-blue (h 218 / 231 / 264)
        public const uint Fx47FistHaloHex = 0x8fe3ff; // the fist's soft halo (h 221)
        public const uint Fx47HaloHex = 0xff9ccf; // the gauntlet's soft halo (h 348); its core / body / rim are Fx47CoreHex / BodyHex / RimHex
        public const int Fx47Auras = 8; // auras alive at once (pooled)
        public const float Fx47AuraLife = 0.15f; // s: the hit aura's fade (NIGHT47.md: fade in 0.15 s)
        public const float Fx47AuraSize = 0.35f; // m: the light core's size at the hit (the gauntlet's x1.25)
        public const float Fx47AuraFront = 0.45f; // m toward the camera from the body's aim point (the sprites depth-test: in front of the skin)
        public const float Fx47AuraRing = 1.2f; // m: the hit's ring at its widest
        public const float Fx47AuraRingS = 0.12f; // s: the ring's growth
        public const float Fx47SwingS = 0.12f; // s: the glove's glow over the swing's way out (the aura's start)

        // ---- visor47: the angular glasses (G13; F-cards 5b; Mesh43.Visor47.cs; in Mesh43's cube units on the head's superellipsoid) ----
        public const int Visor47Role = 5; // the models43 VISOR role (UnityTuning.Models43.cs: 5 VISOR): the parts swapped
        public const float Visor47HeightMul = 1.45f; // the band's height x today's (the card: 1.3-1.6x)
        public const float Visor47WrapMul = 1.09f; // the half-width x today's (husk +-1.15 -> +-1.25 rad: it wraps a little further)
        public const float Visor47ThickMul = 1.5f; // the raise off the head x today's (thicker: glasses, not a slot)
        public const float Visor47TipShare = 0.18f; // of each side: the band converges to a point over this share
        public const float Visor47TipRise = 0.6f; // x the half-height: the temple point sweeps up
        public const float Visor47DipTop = 0.30f; // x the half-height: the top edge's V dip at the bridge
        public const float Visor47DipBottom = 0.15f; // x the half-height: the bottom edge's
        public const float Visor47DipWidth = 0.22f; // of the half-width: the dip's span either side of the centre line

        // ---- foes47: the WARDEN's shield (G12; F-cards 5a; EnemyViews.Shield47.cs; local metres on the warden's root, height 2.3 m) ----
        public const float Shield47UpY = 0.565f; // x height: the shield's centre up (1.30 m: the chest; was 0.52 h = 1.196 m)
        public const float Shield47ArcR = 0.80f; // m: the arc's inner radius round the body's axis (the drawn body's ellipse 0.75 x 0.66 stays inside it at +-60 deg)
        public const float Shield47ArcDeg = 60f; // deg either side of the facing: the sim's block cone (warden.shieldArcDeg 120 / 2)
        public const float Shield47UpZ = 0.70f; // m ahead of the axis (was 0.85: it floated 0.13 m off the body; now it sits on the drawn front)
        public const float Shield47W = 1.45f; // m wide (was 1.9 on a 1.5 m drawn body)
        public const float Shield47H = 1.75f; // m tall (top 2.18 m: the visor at ~2.34 m peeks over it)
        public const float Shield47Thick = 0.10f; // m (was 0.12)
        public const float Shield47BodyHalfW = 0.75f; // m: the drawn body's half-width (warden.radius 0.6 x WardenBodyX 1.25)
        public const float Shield47DownClear = 0.10f; // m: the shield's inner face this far outside the body when down (the card: >= 0.08)
        public const float Shield47DownY = 0.40f; // x height: the shield's centre when down (0.92 m)
        public const float Shield47DownZ = 0.10f; // m: and a little forward
        public const float Shield47DownTiltDeg = 22f; // deg: tipped out (it hangs, opened)
        public const float Shield47EaseS = 0.07f; // s: SmoothDamp's time (settles in ~0.22 s: NIGHT47.md's 0.18-0.22 s raise / lower)
        public const float Shield47KickDeg = 4f; // deg: a block kicks it back (the flinch)
        public const float Shield47ArmX = 0.55f; // m: the arm's centre while it holds the shield
        public const float Shield47ArmUpY = 0.50f; // x height
        public const float Shield47ArmUpZ = 0.38f; // m ahead (reaching to the grip behind the shield)
        public const float Shield47ArmForwardDeg = 62f; // deg: the forearm turned forward to the grip

        // ---- foes47: the small robot (the swarmer; G14; F-cards 5c). Parts: 0 body, 1 head (its visor), 2 legs. Today: a body 0.46 x 0.32 x 0.8, a
        // head poking 0.42 m forward, legs a dome 0.62 wide (wider than the body: bottom-heavy), the head and legs darkened x0.8 / x0.62. ----
        public static readonly UnityEngine.Vector3[] Swarmer47Sizes = { new UnityEngine.Vector3(0.44f, 0.40f, 0.70f), new UnityEngine.Vector3(0.36f, 0.26f, 0.30f), new UnityEngine.Vector3(0.40f, 0.20f, 0.50f) }; // a pill body, a head inside its front, legs narrower than the body
        public static readonly UnityEngine.Vector3[] Swarmer47Offsets = { new UnityEngine.Vector3(0, 0.50f, 0), new UnityEngine.Vector3(0, 0.70f, 0.28f), new UnityEngine.Vector3(0, 0.16f, 0.02f) }; // y as a share of swarmer.height (x, z in m): the head in the body's front third
        public const float Swarmer47LeanDeg = 25f; // deg: the body leans into its pounce (nose down)
        public const float Swarmer47LegX = 0.38f, Swarmer47LegZ = 0.36f; // the four leg spikes' roots in the legs' unit box (x across, z along); their tips splay out past it
        public const float Swarmer47CrouchInS = 0.1f; // s the pounce's crouch eases in (F-cards 5c: anticipation 0.1 s down; NIGHT46 snapped it)
        public const float Swarmer47LeapStretch = 0.35f; // the leap's stretch as a share of the crouch squash, reversed (taller, thinner in the air)
        public const float Swarmer47LandSquash = 0.8f, Swarmer47LandPeriodS = 0.16f, Swarmer47LandS = 0.3f; // the landing's squash spring: its share, period, life (an overshoot, then still)
        public const float Swarmer47LegSplayDeg = 22f; // deg each leg's tip swings outward (splayed wide: about the body's width at the feet)
        public const float Swarmer47LegW = 0.3f; // a leg spike's width in the unit box (x 0.40 m: about 12 cm at the root, a point at the tip)
        public const float Swarmer47ScuttleHz = 6f; // the legs' scuttle (their width breathing: it moves instead of sliding)
        public const float Swarmer47ScuttleDepth = 0.12f; // +- share

        // ---- the enemies' punch aura (G21, F-cards 5d; Tell36View's tell halo in three tones; the body tone stays Tell36's yellow / red) ----
        public const uint Foes47YellowCoreHex = 0xfff6c8, Foes47YellowRimHex = 0xc89a10; // parryable: yellow (the parry language)
        public const uint Foes47RedCoreHex = 0xffe2b0, Foes47RedRimHex = 0xc23a12; // unparryable: red-orange (OKLab h 81 / 36)
        public const float Foes47SwingGlow = 2.2f; // HDR x on the WARDEN's arm in its swing's wind-up (red-orange: an unparryable blow)
        public const float Foes47DroneBandY = 1.7f; // the drone's rim ring (models43: 1.32 x 0.55 x 1.32, the lenses at 1.26 inside it) this much taller: a band frame above and below its lenses (F-cards 5c)
        public const float Foes47DroneSpikeGlow = 1.15f; // the drone's downward spike in DANGER red-orange x this (Models40's rest / tell glow on top: an ember at rest, lit through the charge)
        public const float Foes47CoreScale = 0.6f; // the light core's size as a share of the halo's
        public const float Foes47RimScale = 1.3f; // the darker rim's

        // ---- fx47: the TORCH's plasma (G43; F-cards 1f; GunFx.Torch43.cs, EnemyViews.Burn43.cs) ----
        public const uint Torch47CoreHex = 0xd8f6ff; // the white-hot core (OKLab h 218: the fist's light core)
        public const uint Torch47BlueHex = 0x46c8ff; // the body: plasma blue (h 231, inside the cyan family's reach: player-usable)
        public const uint Torch47MidHex = 0x5a7bff; // the outer flame: a deeper blue (OKLab h 269, measured)
        public const uint Torch47TipHex = 0x7a3dff; // the tips: violet (h 289: the overheal's band, <= 295; RIVAL's magenta is h 338)
        public const float Torch47CoreSize = 0.35f; // m: the white-hot core's flare at the nozzle (x the hold's ramp)
        public const float Torch47LickS = 0.15f; // s: the lick on release (NIGHT47.md: a lick on release)
        public const float Torch47LickStretch = 1.3f; // x the reach: the last tongue stretches past it
        public const float Torch47StartTrauma = 0.2f; // trauma at the start of a hold: the pressure kick (DESIGN-RULES C-3)
        public const float Torch47HoldTrauma = 0.015f; // trauma a tick (0.1 s) while held: the low continuous recoil

        // ---- zoom47: the THUNDERCLAP's shouldered zoom (F-cards 1d; Viewmodel.Zoom47.cs) ----
        public const double Zoom47FovShare = 0.80; // the zoomed world FOV as a share of the base (NIGHT47.md: about 80 %; today x1.25 = ~0.84)
        public const double Zoom47InS = 0.15; // s: the zoom's ease in (ease-out cubic; today a linear 0.12 s)
        public const double Zoom47OutS = 0.12; // s: back out
        public const double Zoom47Rise = 0.045; // m: the gun rises to the shoulder (view33's 0.05 m drop undone first)
        public const double Zoom47In = 0.02; // m: and comes in toward the centre line (never to the screen's centre: w33)
        public const double Zoom47MeetM = 25; // m: the barrel's line meets the reticle this far out
        public const double Zoom47DipM = 0.008; // m: the anticipation dip before the lift
        public const double Zoom47DipShare = 0.45; // of the ease (~70 ms of 0.15 s): the dip's span

        // ---- the a847 reel's door passes (G10's done-when: sprint, slide and dash speed) ----
        public const float Door47SprintMps = 10f; // m/s the camera crosses a curtain at in a "sprint" pass (ground walk 8 + the sprint)
        public const float Door47SlideMps = 14f; // m/s in a "slide" pass (a slide keeps and adds speed)
        public const float Door47DashMps = 24f; // m/s in a "dash" pass (a dash's burst)
        public const float Door47SlideDrop = 0.8f; // m lower in a "slide" pass (the eye drops in a slide)
        // ---- the a847 reel's lava walk (the lead's n47-r0 read: a fixed probe and a 2 m/s walk did not show the pop) ----
        public static readonly float[] Lava47WalkMps = { 2f, 8f, 12f }; // m/s: a walk, a sprint, a slide past the lava
        public const float Lava47RingProbeM = 3f; // m: the ring of 8 floor probes round the player (what the camera sees lit), + one at the feet

        // ---- fx47: the blast's depth (G20; F-cards 1b) ----
        public const float Fx47FovPunchDeg = 2.5f; // deg the world FOV widens at the blast (NIGHT47.md: an FOV punch 2-3 deg)
        public const float Fx47FovOutS = 0.06f, Fx47FovBackS = 0.3f; // s: out, then back
        public const float Fx47TumbleDegS = 540f; // deg/s a launched body turns while it flies (view only)
        public const int Fx47BlastVariant = 0; // the blast's look at boot: 0 A "ring", 1 B "lance" (the blind pair picks; a747 shoots both)
        public const int Fx47LanceStreaks = 30; // B: the streaks of the cone along the aim
        public const float Fx47LanceRingShare = 0.4f; // B: the floor ring's radius as a share of A's

        // ---- fx47: the per-weapon layers (G24; Fx47View.Weapons.cs) ----
        public const float Fx47TracerCoreW = 0.03f, Fx47TracerGlowW = 0.09f, Fx47PelletW = 0.022f; // m: the KESTREL's tracer core and glow, the THUNDERCLAP's pellets (were 0.014)
        public const float Fx47TracerGlowK = 0.45f, Fx47TracerSmokeK = 0.18f; // the glow's and the fading line's share of the tracer's colour
        public const float Fx47TracerSmokeS = 0.12f, Fx47TracerSmokeW = 0.05f; // s, m: the fading line after the shot
        public const int Fx47Puffs = 128; // pooled puffs (sparks, flares, rings, dust)
        public const int Fx47ImpactSparks = 8; // sparks a gun hit (the THUNDERCLAP's pellets half each: they come in eights)
        public const float Fx47ImpactSpeed = 7f; // m/s
        public const float Fx47ImpactLife = 0.14f; // s
        public const double Fx47ImpactLight = 2.5, Fx47ImpactLightRange = 4, Fx47ImpactLightLife = 0.06; // the hit's short light (a LightPools flash)
        public const float Fx47KillFlare = 0.5f, Fx47KillLife = 0.12f; // m, s: the kill's world flare (NIGHT47.md: a kill flare in the player's family)
        public const float Fx47ConeSize = 0.6f, Fx47ConeLife = 0.033f; // m, s: the THUNDERCLAP's muzzle cone (2 render frames)
        public const float Fx47BoostLookDown = 0.64f; // the aim's y below -this (about 40 deg down: boost43's) with the feet in the air: a boost shot
        public const float Fx47BoostRing = 1.6f, Fx47BoostLife = 0.3f, Fx47BoostDust = 3f; // m, s, m/s: the ring and dust under the boost
        public const float Fx47BoostKickDeg = 2.5f; // deg: the boost kicks the view UP (Kicks47 turns by Euler(-pitch): + is up; round 2's -2.5 turned it down, read in code round 3)
        // G24 round 3: the camera punch per gun under fx47 (Viewmodel.OnFire x these), from the n47-r1b a747 sheets (peak / settle, frames at 60 fps)
        // toward F-cards 1's ladder (light 0.5-1.0 deg, medium 1.5-2.5, heavy 3-5):
        public const double Fx47PunchRevolver = 1.0; // KESTREL tap: 1.67-2.91 deg measured; DECIDE47 D-14 ("no gun gets lighter tonight"): kept at today's kick (round 3's x0.6 undone); its weight is the tracer / impact / flare
        public const double Fx47PunchTorch = 1.8; // TORCH tick: 0.32-0.43 deg measured (under the light band's 0.5): x1.8 -> about 0.6-0.8
        public const double Fx47PunchShotgun = 1.0; // THUNDERCLAP: 4.10-4.24 deg, settled by +18 (0.3 s): in the heavy band, kept
        public const float Fx47SwingKickDeg = -2.0f, Fx47SwingKickRiseS = 0.04f, Fx47SwingKickSettleS = 0.3f; // PULSE swing (a swing at the air): n47-r2's clean sheet 0.14 deg off, 0.75 on with a 0.8 nod (r1b's 1.38 rode a hit): the medium band 1.5-2.5 wants a 2.0 deg forward nod (- is down, as the swap's dip)
        public const float Fx47BoostSettleS = 0.35f; // s
        public const float Fx47SwapDipDeg = 0.6f, Fx47SwapSettleS = 0.25f; // deg, s: the new gun lands with a small dip
        public const uint Fx47DustHex = 0x8a8496; // the dust (the dusk's grey-violet: not a colour of meaning)

        // ---- fx47: THE WICK's core punch (G7, W's hand-off: "a short slow motion and a hard camera hit"); the slow motion's numbers are in
        // design.n47.fx47.numbers.corePunchSlow (the game loop's ImpactRules reads them as it reads design.impact.slowMo) ----
        public const float Fx47CoreKickDeg = 5f; // deg: the heavy tier's top (3-5)
        public const float Fx47CoreRollDeg = 3f; // deg of roll with it (the hit lands to one side)
        public const float Fx47CoreSettleS = 0.7f; // s: the heavy tier's settle
        public const float Fx47CoreTrauma = 0.7f; // trauma added (the blast's 0.5 + more: the fight's biggest hit)

        // ---- lava47: the lava light rule (F-cards 6b; LightKit29.Lava47.cs) ----
        public const float Lava47BreathHz = 0.35f; // the one breath shared by the lava surface, its underglow and its lights (Look43's 0.35 Hz kept; the surface's 0.8 Hz retired under the row)
        public const float Lava47BreathDepth = 0.10f; // +- share: a slow, low swell (was 0.15 surface / 0.25 underglow): read as heat, never as a flicker
        public const float Lava47FadeNear = 30f; // m from the camera: a lava light is at full strength inside this
        public const float Lava47FadeFar = 45f; // m: and out past this (smooth between: never a cut)
        public const int Lava47MaxLit = 12; // the nearest this many lava lights may be lit (the rest fade out): foundry plans 21; Forward+ allows 256 per camera
        public const float Lava47SlotFadeS = 0.18f; // s: a room slot's fade out, then in, when the 0.25 s re-pick moves it to another fixture (never a cut)
        public const float Lava47EaseS = 0.4f; // s for a lava light to ease from 0 to full (or back): every change is a fade, so nothing pops
    }
}
