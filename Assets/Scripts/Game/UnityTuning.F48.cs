// NIGHT48 builder F: Unity-only view numbers of F's NIGHT48 rows (lineup48, foes48, blast48, guns48; CLAUDE.md rule 5: render / reel
// numbers live here, each with a comment). No gameplay number: every row here is a view row or a reel. No UnityEngine type in this file (the
// harness reads it). The specs: NIGHT48.md section 1 (G12-G14, G20, G24, G43) and Research/2026-10-19-wave48/F.md.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- lineup48: the a948 lineup reel (G12-G14; NIGHT48.md: "hides the viewmodel and the HUD and stands each kind alone on a neutral plate") ----
        public const float Lineup48StageAboveM = 18f; // m: the stage stands this far over whatever is above the player's feet (a roof or open sky): nothing of the level in the frame
        public const float Lineup48StageMaxM = 80f; // m: the stage's height over the feet at most (a level with no roof caps here)
        public const float Lineup48EyeM = 1.6f; // m over the plate: the reel camera's eye (a standing player's eye)
        public static readonly float[] Lineup48Distances = { 3f, 10f, 20f, 30f }; // m: the camera's distances (NIGHT48.md: 3 close-up, 10, 20, 30)
        public const float Lineup48PlateGrey = 0.34f; // the plate's floor value (neutral, unlit): dark bodies read against it, the amber visor is the brightest thing
        public const float Lineup48WallGrey = 0.52f; // the backdrop's value (lighter than the floor: the silhouette reads against it)
        public const float Lineup48WallBehindM = 7f; // m: the backdrop stands this far behind the body
        public const float Lineup48WallW = 220f, Lineup48WallH = 130f, Lineup48FloorW = 240f; // m: the plates (at 30 m the frame spans ~52 x 92 m at the backdrop)
        public const int Lineup48Cell = 320; // px (at 1080p): one contact-sheet cell; 10 / 20 / 30 m cells are a 1:1 crop (the true read), the 3 m cell a crop scaled down
        public const int Lineup48CloseCrop = 960; // px (at 1080p): the 3 m cell's crop before it is scaled to the cell (the whole body in view)
        public const int Lineup48SettleFrames = 14; // render frames a pose settles before its capture (the shield's SmoothDamp 0.07 s settles in ~0.22 s)
        public const float Lineup48GaitMps = 3.2f; // m/s: the gait strip's walk across the plate (side-on, 4 m away)
        public const float Lineup48GaitCamM = 4f; // m: the gait strip's camera distance
        public const float Lineup48GaitS = 2f; // s: the gait strip's length (NIGHT48.md: "a 2 s gait strip")
        public const int Lineup48GaitFrames = 12; // cells in the strip (one every 1/6 s)
        public const float Lineup48DiffBoxPad = 1.5f; // round 4 (a848): the difference is searched only inside the body's projected box x this (+4 px): the rest of the screen moves too (motes, the HUD)
        public const float Lineup48DiffL = 0.04f; // round 3 (a848): a pixel is the body's when the frame with it and without it differ by more than this (Rec. 709 luminance, 0..1)
        public const float Lineup48ZoomPad = 1.3f; // round 2: the zoom sheet's crop = the silhouette's larger side x this (the whole body in the cell, a margin round it)
        public const float Lineup48TeleAsM = 5f; // round 2: the 20 m TELE row's field of view draws the body as big as at this distance
        public const float Lineup48DroneLiftM = 1.7f; // m over the plate: the drone's centre on the stage (it hovers; its saucer at the eye's height)

        // ---- foes48: the WARDEN's shield (G12; EnemyViews.Foes48.cs). The arc is the sim's +-60 deg block cone round the body's axis (Mesh43.ShieldArc48);
        // the body's drawn ellipse is 0.75 (x) by 0.66 (z) m (warden.radius 0.6 x WardenBodyX 1.25 / WardenBodyZ 1.1), its pauldrons reach x 1.05 at y >= 1.59 ----
        public const float Shield48ArcR = 0.84f; // m: the arc's inner radius round the body's axis (0.09 m outside the body's 0.75 half-width at the side, 0.18 in front)
        public const float Shield48UpY = 0.565f; // x height: the shield's centre while up (1.30 m: the chest, as foes47)
        public const float Shield48ShoulderX = 0.62f, Shield48ShoulderY = 0.74f, Shield48ShoulderZ = 0.02f; // the arm's shoulder (x m, y x height, z m): under the pauldron
        public const float Shield48GripDeg = 18f; // deg off the facing: the hand's grip on the shield's inner face (to the arm's side)
        public const float Shield48GripIn = 0.16f; // m inside the inner face: the hand (the arm's end face stays behind the shield: it never pokes out the front)
        public const float Shield48GripY = 0.0f; // m over the shield's centre: the grip's height
        public const float Shield48ArmThick = 0.28f; // m: the forearm's width (NIGHT46 0.35 m by 1.3 m: it ran through the shield's front)
        public const float Shield48DownSwingDeg = 90f; // deg: down, the arc swings round the body's axis to its right side (the same arc: it never cuts the body)
        public const float Shield48DownTipDeg = 20f; // deg: and tips its top out about its bottom edge (opened; the top clears the pauldron, the bottom stays outside the body)
        public const float Shield48DownBottomY = 0.06f; // m: the shield's bottom edge when down (just off the floor)
        public const float Shield48DownGripIn = 0.10f; // m: the hand inside the inner face when down
        public const float Shield48SwingS = 0.09f; // s: SmoothDamp's time for the swing (settles in ~0.3 s: a heavy shield, eased, never a snap)
        // round 5 (the blind pair, DECIDE48 D-33: "the new plate reads as a pale slab fused into the torso"): the plate in the shield's own orange
        // at a body's value with a dark rim (the models43 frame child, re-shaped to the arc)
        public const float Shield48Value = 0.85f; // x WardenShieldHex 0xffb05a (NIGHT20 drew it x1.3 +- 0.4 shimmer: HDR, read pale / near white)
        public const float Shield48Shimmer = 0.12f; // +- share: its slow shimmer (sin 3t)
        public const float Shield48BlockLift = 0.8f; // + share while a block flashes (the block still lights it)
        public const float Shield48FrameW = 0.07f; // m: the rim tubes' width round the plate
        public const float Shield48FrameProud = 0.03f; // m: the rim stands proud of the plate's outer face (an edge that catches the light)
        public const int Shield48QueueAfterXray = 1; // render queue steps after the enemy x-ray's (Transparent + EnemyXrayQueueOffset): foes48's shield draws over the warden's own x-ray body
        public const float Shield48BashK = 0.12f; // x the NIGHT20 tell arm angle (-1.2 rad): the shield and the arm pitched top-back about the shoulder (~8 deg: a wind-up; at 0.18 its top reached 2.36 m, over the visor)
        public const float Shield48ShoveM = 0.22f; // m: the swing (arm 1.1 rad) shoves the shield forward this far (a bash; a forward pitch swung its bottom into the body: n48feel shield)
        public const float Shield48DropShare = 0.6f; // of the eased down share: the shield is lowered over its first 60 % ...
        // ---- round 3: the FUSION pop's chroma (Fusion43Screen; W's 1413372e found the same unit mix-up in the open-boss hit) ----
        // PostFx's pulse is in post.js units (post.chromaHurt 0.006 = the player's hurt), x ChromaScale 36 into URP's chromatic aberration (0..1),
        // decaying at post.chromaDecay 0.03 a second. Fusion43Chroma 1.4 gave 50.4, clamped to 1.0: the whole screen at a full split for
        // (1.4 - 1 / 36) / 0.03 = 45.7 s after every FUSION. Now the hurt's kick (0.22 intensity, gone in 0.2 s): the FUSION's own picture (the
        // pink ring, the word, the freeze, the kick) is its read, not a screen-wide rainbow.
        public const double Fusion48ChromaOfHurt = 1.0;
        /// <summary>The FUSION pop's chroma kick in PostFx's pulse units: post.chromaHurt x Fusion48ChromaOfHurt.</summary>
        public static double Fusion48Chroma(Skygrind.Core.GameConfig cfg) => cfg.Num("post.chromaHurt") * Fusion48ChromaOfHurt;
        public const float Fusion48ChromaMax = 0.25f; // a848's check (the reel only): URP's chromatic aberration at the FUSION and 0.6 s on stays at or under this

        // ---- a748: the weapons reel's staging (G24 sheets) ----
        public const double Fx48ChargeHoldKestrelS = 1.25; // s of game time RT is held before the KESTREL's release (its charge: design charge40 0.9 s, + margin)
        public const double Fx48ChargeHoldThunderS = 1.55; // s held before the THUNDERCLAP's slug (its charge: charge40 1.2 s, + margin)
        public const double Fx48PrePassS = 1.4; // s of game time before each pass's f-2 (round 2: 0.6 s left the last pass's shake / FOV in the baseline)
        public const float Fx48QuietDeg = 0.02f; // deg a frame: the pitch and the field of view count as still under this (round 3's quiet baseline)
        public const int Fx48QuietFrames = 12; // frames in a row still before a pass's f-2
        public const float Fx48QuietMaxS = 4f; // s of wall time at most waiting for it (the line says when it never came)
        public const float Fx48LighterTol = 0.15f; // deg: "lighter than N47" past this (the every-frame peak's spread between two passes of the same kick)
        public const double Fx48BoostUpM = 3.0; // m over the floor the boost's sheet starts (in the air: a press fires at once and boosts)

        // ---- guns48: G24 from the n48-r1 a748 ladder (Fx48View.Guns.cs) ----
        public const float Guns48FistNodDeg = -1.2f, Guns48FistRiseS = 0.04f, Guns48FistSettleS = 0.25f; // the fist's nod (- down; F-cards 1a light-medium 1.2 deg): it read 0.00 deg on every frame
        public const float Guns48ShotFovDeg = 2.0f; // deg: the THUNDERCLAP's plain shot widens the view (the heavy tier's FOV punch +2-3; it read 0.0)
        public const float Guns48TapFovDeg = 0.6f; // deg: the KESTREL's tap (light-medium; the medium tier's +0.8 is the charged shot's, which already punches +4-7)
        public const float Guns48FovOutS = 0.06f, Guns48FovBackS = 0.30f; // s: out, then back (the blast's FOV punch's curve)
        public const float Blast48NoAuraS = 0.15f; // s after a blast: its own gauntlet hits draw no punch aura (round 2: the f4 wash was the aura on the 3 m body)

        // ---- blast48: the LT blast's wash fixes (G20; Fx48View.Blast.cs) ----
        public const float Blast48AimRingClear = 0.17f; // x the distance: the aim ring's radius / the lance's offsets at least this (the ring's inner edge 0.857 x 0.17 = 0.146 > the centre box's 0.134 half-width at 74 deg, 16:9; fx47's 0.12 put the band across the box)
        public const float Blast48BoxHalfTan = 0.134f; // tan of the centre box's half-width (10 % of a 16:9 frame's width at a 74 deg vertical field of view): the shell test's box
        public const float Blast48ShellCentreAlpha = 0.35f; // a burst shell whose silhouette covers the screen's centre draws at this share (n47-r3's f18 / f30 wash: +0.34 / +0.35)

        // ---- foes48: G13's band on the lens visors (Mesh43.Visor48.cs; in the head's cube units, Patch47's rules) and G14's gait ----
        public const float Visor48SwarmerLat = 0.095f; // rad on the swarmer's head: the band through its two upper lenses (head y 0.116 = Eyes3's upper pair)
        public const float Visor48SwarmerGapIn = 0.10f, Visor48SwarmerGapOut = 0.17f; // of the band's half-width: the two slits between the bridge and the side segments
        public const float Visor48SpitterLat = 0f; // rad on the spitter's front box: the band through the round lens's middle (Ball at y 0.02)
        public const bool Visor48DroneBrows = false; // round 5: the drone's brows off (the blind pair: "pale overlapping patches"; OLD won 3-0): its lenses as before
        public const float Visor48BrowSpanRad = 0.45f, Visor48BrowR = 0.52f, Visor48BrowHalfH = 0.13f; // the drone: each brow's half arc, radius, half height (Lenses3 space: lenses at r 0.5, 0.1 tall)
        public const int Legs48Phases = 8; // poses of the gait's cycle (one mesh each, made once)
        public const float Legs48SwingDeg = 24f; // deg: a leg's swing forward / back about its root
        public const float Legs48Lift = 0.25f; // the forward-swinging leg's lift in the legs' unit box (x 0.20 m: 5 cm)
        public const float Legs48StrideM = 0.5f; // m walked per gait cycle (the phase follows the drawn distance: no motion, no gait)
        public const float Shield48SwingLag = 0.15f; // ... and swings to the side after its first 15 % (lowered and swung together, mid-swing it met the pauldron: n48feel shield)
    }
}
