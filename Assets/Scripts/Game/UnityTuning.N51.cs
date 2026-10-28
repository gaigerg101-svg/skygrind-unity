// NIGHT51 (GG 2026-10-19: "how can we seriously make this game better"; the lead's honest read of the n50 frames): view-only changes, on by
// default with no config row, off with -skygrindOff51 (or any older off switch: NightRows.OffFrom). Unity-only numbers (render, UI layout,
// camera feel); the sim never reads them, so golden vectors, replays and fingerprints are untouched.
// - crunch51: a plain gun kill had ~0.4 deg of shake, no FOV punch and no kick; the hit flash was LDR white (bloom threshold 1: it never
//   glowed) held 40 ms, most of it inside the body-hit freeze; gun kills' chunk speed was capped at +15 % over the base whatever the overkill.
// - declutter51: the teach card sat 170 px over the crosshair (in the aim zone) at 44 / 30 px; it moves up under the room title, smaller.
// - gem51: the guns' core gem was a smooth Unity sphere on a blocky gun (it read as a dome); it becomes the web's faceted octahedron
//   (blockyKit.js socketGem: OctahedronGeometry(r x 0.82) scaled (1, 0.8, 1)), seated in its socket.
using System;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // crunch51
        public const double N51KillPunchInDeg = 1.0; // a plain kill's FOV punch-in (a headshot kill keeps ImpactPunchInDeg 1.5)
        public const double N51KillPunchHoldS = 0.085; // s held before its release: the kill freeze's length (design.n42.impact42 killS)
        public const double N51ShakeKill = 0.62; // a plain kill's trauma (0.42 before: ~0.35 deg; this ~0.8 deg)
        public const float N51HitFlashHoldS = 0.07f; // the enemy's white hold on a hit (Crunch36FlashHoldS 0.04 before)
        public const float N51FlashHdr = 2.4f; // the hit flash's white x this (over the bloom threshold 1: the body glows on a hit)
        public const float N51ChunkOverkillSpeedMax = 1.5f; // a gun kill's chunk speed may grow to x this with the overkill (1.15 before)
        public const float N51KillThrowNearShare = 0.5f; // a near kill's slowest throw share (0.3 before: point-blank pieces barely moved)
        // declutter51
        public const float N51TeachY = 368f; // px over the canvas centre: the card's centre under the room title (Teach43Y 170 before: the aim zone)
        public const float N51TeachHeadPx = 34f, N51TeachHeadMinPx = 28f; // the card's head line (hud47's 44 / 36 before)
        public const float N51TeachSubPx = 24f, N51TeachSubMinPx = 20f; // its rule line (hud47's 30 / 26 before); >= 16 px (CLAUDE.md rule 8)
        public const float N51TeachPlateAlpha = 0.62f; // the plate (0.82 before): the wall reads through it
        // star51: the muzzle star in real time (fx.flashFrames = 2 render frames: 33 ms at 60 Hz, 14 ms at 144 Hz)
        public const double N51StarFullS = 0.022; // s at full strength
        public const double N51StarS = 0.04; // s in all (the rest at fx.flashSecondFrame's share)
        // ink51 (round 3: n51-r2 frames: the orange pills melt into kiln's and foundry's warm floors; ULTRAKILL's enemies read by silhouette)
        public const float N51InkPx = 2.6f; // the enemy outline's width, px at 1080p (constant on screen: a far enemy keeps its edge)
        public const uint N51InkHex = 0x140A22; // its colour: near-black violet (the dusk's darkest; not a palette hue)
        // ceil51 (round 5: the ceilings were one flat dark quad over every room, a quarter of most frames: a void, no scale)
        public const float N51CeilPanel = 4f; // m: a coffer's target size (the face divides evenly)
        public const float N51CeilBeam = 0.22f; // m: half a beam's width (the beam is twice this)
        public const float N51CeilBeamLight = 2.6f; // the beam's colour x this (the warm grey lifted: it reads as structure; n51-r5's 1.9 read faintly on span)
        public const float N51CeilRecess = 0.7f; // a coffer's middle x this (deeper)
        // scorch51 (round 6: a kill left nothing behind once its chunks were gone; the arena kept no record of the fight)
        public const int N51ScorchMax = 32; // marks at once (the oldest replaced)
        public const float N51ScorchLifeS = 20f; // s a mark stays
        public const float N51ScorchFadeS = 4f; // s it fades over at the end of its life
        public const float N51ScorchHeatS = 1.2f; // s its core glows hot (cooling as (1 - t)^2)
        public const float N51ScorchRadii = 1.8f; // its half-width x the enemy's radius (a husk's ~1.8 m splat)
        public const float N51ScorchMin = 0.6f; // m: the smallest half-width (a swarmer's mark still reads)
        public const float N51ScorchLift = 0.03f; // m over the feet (and a depth offset in the shader): no fight with the floor
        // rank51 (GG w41: the style meter should "feel aweosme to combo"; style42 animates it, but its letter was 50 px, the corner's small print)
        public const float N51StyleLetterScale = 2.0f; // the rank letter over hud33's 40 px (Style42LetterScale 1.25: 50 px -> 80 px); its punch-in scales with it
        // gem51
        public const float N51GemScale = 0.8f; // the octahedron's radius x the web's r x 0.82 (a hair smaller: a set stone, not a lamp)
    }

    /// <summary>NIGHT51's switch and its globals (Look.Apply calls Apply once per run's look).</summary>
    public static class Night51
    {
        static int on = -1;
        public static bool On
        {
            get
            {
                if (on < 0) on = 51 < Skygrind.Core.NightRows.OffFrom(Environment.GetCommandLineArgs(), "-skygrindOff") ? 1 : 0;
                return on == 1;
            }
        }

        static readonly int FlashHdr = Shader.PropertyToID("_Flash51Hdr"), Ink = Shader.PropertyToID("_Ink51"), Ceil = Shader.PropertyToID("_Ceil51");

        public static void Apply()
        {
            Stone51.Apply();
            Shader.SetGlobalFloat(FlashHdr, On ? UnityTuning.N51FlashHdr : 1f);
            Color ink = Look.Hex(UnityTuning.N51InkHex).linear;
            Shader.SetGlobalVector(Ink, new Vector4(On ? UnityTuning.N51InkPx : 0f, ink.r, ink.g, ink.b));
            Shader.SetGlobalVector(Ceil, new Vector4(UnityTuning.N51CeilPanel, UnityTuning.N51CeilBeam, UnityTuning.N51CeilBeamLight, UnityTuning.N51CeilRecess));
        }
    }
}
