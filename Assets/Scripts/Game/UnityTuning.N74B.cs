// NIGHT74 builder B (bosses: weak74 B1, target74 B2, kill74 B3): the Unity-only numbers, each with its reason (CLAUDE.md rule 5). View only: nothing here
// touches the sim (the web has no campaign bosses, no boss colours, no boss kill beat). Rows: design.n74.weak74 / target74 / kill74, all OFF until the lead's flip.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- weak74 (rule W, FORGE74 D-4: on a boss, cyan means "shoot here" and nothing else) ----

        /// <summary>weak74: THE WICK's identity colour for a closed weak point (its idle core), in place of the player's cyan: a muted verdigris iron, hue 125, S 0.13, V 0.73.
        /// Quiet and inert beside the vivid cyan of the open core, and in none of the roles' arcs: not red-orange (danger), not yellow (the parry tell the core flashes and the embers
        /// carry), not magenta (RIVAL), not the boundary's pale violet-white (hue 252), and not the cool arc 150-215 the cyan test reads (a steel blue at hue 214 sat inside it, and
        /// the Wick's grade55 pulls its shadows to indigo 225-255, where it would drift toward the boundary). The shaft's furnace heat stays on the plates' ember edges.</summary>
        public const uint N74WickIdentityHex = 0xA3BBA5;

        /// <summary>weak74: THE CROWN's identity colour for its ring, rim, halo, blades and gems while it is shut: ivory bone, hue 43, S 0.14, the family of crown40's own gem bone
        /// (Crown40GemHex 0xf2ead8, "the thing to break"). Neutral (S under 0.15: no hue the colour meters read), so the ring is no longer a second cyan beside the TORCH
        /// (gg-proxy close #2), and it is not the parry yellow's arc (45-70) or the boundary's violet.</summary>
        public const uint N74CrownIdentityHex = 0xE8DFC8;

        /// <summary>weak74: THE LOOM's identity colour for its idle knees, nails and seams: brass-silk, hue 40, S 0.27, V 0.80 (the body is brass, Look47BrassHex 0x8a6a2e). The knee's
        /// old idle colour was the boundary's pale violet-white (Loom37KneeIdleHex 0xd8d0ff), so it could not stay; the live knee (the leg's pair) keeps its cyan. Under the danger
        /// meter's S 0.35, and 12 degrees under the parry yellow.</summary>
        public const uint N74LoomIdentityHex = 0xCDBB95;

        /// <summary>weak74: the hue arc, in degrees, and the least saturation at which a boss part's drawn colour counts as the player's cyan for the closed-ring readback pass
        /// (Weak74.RecolourIfCyan). 150-215 holds colors.usable (hue 186) and its bloom and fringe down to 160 (colours63's player band 160-205, widened by 10 each side for an HDR
        /// pulse that clips toward green or blue); 0.12 keeps a pale cyan-white pulse (the lit ring) and leaves the crown's steel (S 0.12, hue 255) and bone alone.</summary>
        public const float N74CyanHueLo = 150f, N74CyanHueHi = 215f, N74CyanSatMin = 0.12f;

        // ---- target74 (B2: an open weak point off screen is named and pointed at) ----

        /// <summary>target74: the edge chevron's distance in from the screen edge, in HUD px at 1080p: past the HUD's 54 px margin (Hud33EdgeMargin) and the boss bar's block, so the
        /// chevron never sits on a HUD box.</summary>
        public const float N74TargetInsetPx = 96f;

        /// <summary>target74: the arrow's parts, in HUD px at 1080p: each head arm's length, the shaft's length behind the tip, their thickness, and the half angle between an arm and the pointing axis.
        /// Round 1 (40 px arms, 9 px thick, 42 degrees, no shaft) read as "a small cyan corner bracket" on the lead's B2 pair (the spotter, Logs/n74/spot/B2): two thin strokes at 84 degrees, aimed
        /// diagonally, are a right angle, and a right angle on a frame's edge is a bracket. Round 2: a SHAFT behind the tip (an arrow, not an angle), a narrower head (36 degrees: 72 degrees open, acute),
        /// 64 px arms and a 76 px shaft at 16 px: about 130 px long and 100 px across, an arrow the eye finds across the frame.</summary>
        public const float N74TargetArmPx = 64f, N74TargetShaftPx = 76f, N74TargetThickPx = 16f, N74TargetHalfAngleDeg = 36f;

        /// <summary>target74: the dark outline the arrow carries (a bar behind each cyan bar, this many px longer and thicker on every side, in the HUD's shadow ink at this alpha), so it reads on the
        /// pale walls and the dark ceiling alike (the B2 frame's top left was a lattice of olive bars the thin cyan strokes sat among).</summary>
        public const float N74TargetOutlinePx = 5f, N74TargetOutlineAlpha = 0.78f;

        /// <summary>target74: the chevron's pulse, a share of its alpha swung at this rate (Hz, under 3 Hz: WCAG 2.3.1): 0.75 +- 0.25 at 1.5 Hz.</summary>
        public const float N74TargetPulseHz = 1.5f, N74TargetAlphaLo = 0.5f, N74TargetAlphaHi = 1f;

        /// <summary>target74: the least type size (px at 1080p) the direction word may be drawn at (the tag's own face: Hud47BossLabelPx is 26 with hud47 on); the log line prints the size
        /// the tag really has, so a 16 px tag (hud47 off) reads as a miss and not as a pass.</summary>
        public const float N74TargetTextMinPx = 22f;

        /// <summary>target74: the separator between the bar's tag and the direction word ("OPEN 2.9 - ABOVE").</summary>
        public const string N74TargetSeparator = " - ";

        // ---- kill74 (B3: the boss kill beat: a white flash on the silhouette, a held view, then finish55's punch-in) ----

        /// <summary>kill74: the flash starts this many RENDERED frames after the frame the death event was seen in (the frame the sim emitted bossDying in is 0), and lasts this many frames.
        /// The slate reads "the strip's kill frame +1 and +2": the strip's frame f is the frame of the event (FeelProbe.FrameCount inside the event's Update), so the flash is on f+1 and f+2.
        /// Two frames at 60 fps are 33 ms, one flash per kill: nowhere near WCAG 2.3.1's 3 flashes a second, and it is on the boss's silhouette, not the screen (finish55's header: no full-screen flash).</summary>
        public const int N74KillFlashStartFrame = 1, N74KillFlashFrames = 2;

        /// <summary>kill74: the flash's additive white, as a gain on (1, 1, 1): 2.5 saturates the lit parts to white and lets the bloom carry it past the silhouette's edge (the overlay is the
        /// sim's own hit boxes scaled N74KillFlashScale, a few cm outside the drawn box, so it does not z-fight the surface).</summary>
        public const float N74KillFlashGain = 2.5f, N74KillFlashScale = 1.06f;

        /// <summary>kill74: the hold (real seconds from the death event): the world's time share (SurvivalGame.Kill74SlowFrame; the stepping slows, no step is skipped or changed) and
        /// finish55's punch-in both wait this long. 0.35 s is the scout's top of the range for a boss kill (scout-bosses.md K1: kill freeze 0.25-0.5 s) and short of the finisher's own killTime
        /// 0.6 s slow-mo (design bossKit), which goes on after it at its own 0.3 share. 0.1 is the row's own "0.1x view time".</summary>
        public const float N74KillHoldS = 0.35f, N74KillHoldShare = 0.1f;

        /// <summary>kill74: the bosses whose death the row dresses (BossAttackClock63's ids): all five. THE KILN WYRM's silhouette is the sim's 26 oriented boxes (the bosskill72 strip's boss, B3);
        /// the other four are the solid meshes under their view's body root (leftover 6, NIGHT74.md section 7): the same overlay drawn from the renderers' own meshes.</summary>
        public static readonly string[] N74KillBosses = { "wyrm", "wick", "crown37", "loom37", "brawl40" };

        // ---- spring74 (leftover 3: an enemy's hit squash a glance sees) ----

        /// <summary>spring74: the hit squash's peak, as a share of the body's height, SET at the hit frame (a displacement, not a velocity), so the frame the hit is drawn on and the ones after it
        /// (the strip's hit + 1, + 2) are already deep in it. spring72's flinch was a velocity kick (-15.5): its squash grew over the frames after the hit and peaked under 17 % about five frames on,
        /// where crunch36's own 15 % pop (Crunch36Squash) is at its peak on the hit frame: the close crops at hit + 1 read NOTHING OBVIOUS (Logs/n73 spring72 verdict, THREADS T-159). 0.32 on top of
        /// crunch36's 0.15 is a body 0.85 x 0.68 = 58 % as tall and about a third wider (volume kept) for the first frames: seen at a glance, and still a pill (the same silhouette squashed).</summary>
        public const float Spring74Squash = 0.32f;

        /// <summary>spring74: the hold (frame time, s): the squash stays at its depth this long after the hit, a hitstop-style beat on the victim. At 60 fps 0.03 s holds the hit frame and the two after it
        /// (the strip's hit + 1 and + 2) at full depth. A free 9 Hz spring (below) from the kick is already at -0.15 on hit + 1 (simulated: -0.32, -0.15, +0.02, +0.07 on frames 0 to 3): it is the hold that
        /// lets a glance at hit + 1 or + 2 see the squash.</summary>
        public const float Spring74HoldS = 0.03f;

        /// <summary>spring74: the spring that brings it back after the hold (Spring72Math, frame time): 9 Hz, damping 0.4 (a 25 % overshoot into a stretch of about 8 %): -0.15 on the first frame, a stretch of 0.07
        /// by the third, quiet by 0.2 s: a snap, not a wobble.</summary>
        public const float Spring74Hz = 9f, Spring74Zeta = 0.4f;

        /// <summary>spring74: the squash and the stretch are held inside +-this (the sum of the spring only; crunch36's and the others' multiply on top, as spring72's do).</summary>
        public const float Spring74Max = 0.42f;

        /// <summary>spring74: what counts as a hit: the body's health fell by at least this share of its maximum since the last frame it was drawn (a burn's or a pellet's single tick does not), at
        /// least Spring74GapS seconds after the last kick (a burst of pellets is one hit). Hits under Spring74BigShare of the maximum kick Spring74SmallMul of the squash up to the full squash at it
        /// (a graze is smaller, a heavy hit is the full read).</summary>
        public const float Spring74MinShare = 0.06f, Spring74GapS = 0.08f, Spring74BigShare = 0.25f, Spring74SmallMul = 0.6f;

        /// <summary>spring74: how many kicks the Player log prints a line for (the first ones; each says its squash at the hit frame).</summary>
        public const int Spring74KickLines = 6;

        // ---- crownroom74 (leftover 2: THE CROWN's room in one palette with its identity) ----

        /// <summary>crownroom74: the wall multiplier (r, g, b on the stone's vertex colour) that REPLACES palette72's spire wall multiplier (Palette72WallMul: 1.23, 0.93, 2.92) on the stone's wall
        /// and ceiling vertices inside THE CROWN's boss room. ROUND 2 (the lead's n74-t4: round 1's uniform x1.15 turned the room's walls OLIVE, hue 77-90, S 0.5-0.7, and yellow-green jumped from
        /// 18 to 47 % of the vivid pixels): the screen is not neutral under a neutral albedo, because the spire's light is blue-starved (palette72's own note). MEASURED on the t4 idle pair
        /// (Logs/feel/n74-t4/croff and cron, boss63 crown37-idle, five wall pixels): the screen's sRGB channels scale LINEARLY with the vertex multiplier (OFF (79, 79, 118) x the multiplier ratio
        /// (1.15/1.23, 1.15/0.93, 1.15/2.92) = (74, 98, 47); the frame read (72, 96, 48)), so the multiplier that lands the room's wall on a chosen colour is palette72's times target / OFF. OFF's wall was
        /// a slate-blue, hue 240, S 0.3-0.4 (the meter's "violet" band starts at 230), (79, 79, 118) at luma 82; the target is the same luma desaturated to a cool neutral slate, (81, 81, 91), hue 240,
        /// S 0.11 (under the meter's vivid floor 0.35): palette72's x(1.23, 0.93, 2.92) x (81/79, 81/79, 91/118) = (1.26, 0.95, 2.25): the red and the green kept, the blue cut by a quarter. The lighter walls
        /// of the frame ((118, 110, 141)) land on (121, 112, 109): a warm grey, S 0.10. Predicted, not seen: the retake's Player frame is the check.</summary>
        public static readonly float[] N74CrownRoomWallMul = { 1.26f, 0.95f, 2.25f };

        /// <summary>crownroom74 round 2: the sky of THE CROWN's fight (CrownSky74.cs), three colours that replace the dusk gradient (config colors.skyZenith 0x100A2C navy, skyMid 0x4A2168 violet, skyHorizon
        /// 0xFF7A48 orange) while the boss is live. The t4 frames: of the violet + magenta that survive the wall fix (11 % of the frame), the top-right plane (85 % of its cell: hue 265, (60, 30, 99)) and the
        /// band under it (hue 305-330) are the SKY seen through the spire's open top, zenith to mid to the horizon's magenta: not stone, so no wall multiplier reaches them. THE CROWN's identity is ivory and
        /// gold at height over slate (design.n54.grade54 crown37: "slate shadows hue ~220, not violet"; pale high-value warm highlights): slate zenith 0x2C323E (hue 218, S 0.29, V 0.24), slate mid 0x5A6577
        /// (hue 215, S 0.24, V 0.47), ivory horizon 0xD9CFB4 (hue 43, S 0.17, V 0.85; the sky shader's warm glow is the horizon colour x Glow, so the sun's side is ivory too). All three sit under the
        /// meter's vivid floor (S 0.35), outside every role arc (danger 345-35, parry 45-70, cyan 160-205, RIVAL 285-335, the boundary's pale violet 230-300) and out of the olive the walls just showed.</summary>
        public const uint N74CrownSkyZenithHex = 0x2C323E, N74CrownSkyMidHex = 0x5A6577, N74CrownSkyHorizonHex = 0xD9CFB4;

        /// <summary>crownroom74 round 2: the sky blends in over this many seconds when the Crown's fight begins and out over the second when it ends (design.n54.grade54.numbers.common fadeInS 0.8 / fadeOutS 1.6:
        /// a boss's room look arrives with the fight and leaves slower than it came), on the reel's frame time (FeelClock: a same-run held frame does not move it).</summary>
        public const float N74CrownSkyInS = 0.8f, N74CrownSkyOutS = 1.6f;

        /// <summary>crownroom74: the boss whose room it is (the room's own `boss` field in the level's campaign rooms), and how far the room's floor rect is grown on every side when a stone vertex is
        /// tested against it (m): a room's wall faces stand ON its rect's edge, and a vertex can sit a few centimetres outside it.</summary>
        public const string N74CrownRoomBoss = "crown37";
        public const double N74CrownRoomMarginM = 0.75;
    }
}
