// NIGHT33 L round 3 (design.n33.look33): Vfx33's numbers (L1 section 3 "Cheap stylized VFX"). View only. Unity types, so not in the harness (UnityTuning.Look33 is).
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // embers over lava (L1: 20-40 quads rising 0.5-1.2 m/s with sway, 3-5 s, orange HDR 2-3 fading to dark red, up 4-6 m)
        public const float Look33EmberHdr = 1.6f; // the embers' additive colour: colors.enemy x this (danger: lava only; n33-r5: 2.5 -> 1.6 with the lava's orange share over the cap)
        public const double Look33EmbersPerM2 = 0.6; // embers alive per m2 of lava, clamped to the min / max below
        public const int Look33EmberMin = 20, Look33EmberMax = 40; // per lava pit (L1: 20-40)
        public static readonly Vector2 Look33EmberLife = new Vector2(3f, 5f); // s (L1); at 1.2 m/s a 5 s ember climbs 6 m at most
        public static readonly Vector2 Look33EmberSize = new Vector2(0.04f, 0.08f); // m
        public static readonly Vector2 Look33EmberRise = new Vector2(0.5f, 1.2f); // m/s up (L1)
        public const float Look33EmberSway = 0.35f; // the noise module's strength: the sideways sway
        // dust motes in each room's key pool (L1: 40-80 tiny soft quads, 0.02-0.04 m, 0.05-0.15 m/s, alpha low, in light pools only)
        public const float Look33MoteGain = 0.18f; // the motes' additive colour: the boundary's pale violet-white x this (very faint)
        public const float Look33MotePool = 8f; // m: the emitter box's widest side (the key's pool under its panel)
        public const float Look33MotesPerM2 = 1.2f; // motes alive per m2 of that box, clamped below
        public const int Look33MoteMin = 40, Look33MoteMax = 80; // per room (L1)
        public static readonly Vector2 Look33MoteLife = new Vector2(8f, 12f); // s
        public static readonly Vector2 Look33MoteSize = new Vector2(0.02f, 0.04f); // m (L1)
        public const float Look33MoteDrift = 0.1f; // the noise strength: about 0.05-0.15 m/s of drift (L1)
        // sparks at the braziers (L1: 8-12 stretched billboards, 0.3 s, no light)
        public const uint Look33SparkGoldHex = 0xffc35a; // the sparks lean half-way to this gold from colors.enemy (a hot spark, still the danger family)
        public const float Look33SparkHdr = 3f; // their additive brightness
        public static readonly Vector2Int Look33SparkBurst = new Vector2Int(8, 12); // sparks per burst (L1)
        public const float Look33SparkEvery = 2.2f; // s between bursts (L1: 1.5-3)
        public static readonly Vector2 Look33SparkLife = new Vector2(0.25f, 0.35f); // s (L1: 0.3)
        public static readonly Vector2 Look33SparkSize = new Vector2(0.015f, 0.03f); // m
        public static readonly Vector2 Look33SparkSpeed = new Vector2(1.5f, 3f); // m/s out of the cage
        // the danger flicker (L1: perlin(time x 8-14), +-15 % on danger fixtures only; cyan steady)
        public const float Look33FlickerHz = 11f; // the perlin's time scale (L1: 8-14)
        public const float Look33FlickerDepth = 0.15f; // +-15 % (L1)
    }
}
