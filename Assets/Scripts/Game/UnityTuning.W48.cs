// NIGHT48 builder W (G9 round 3; the lead's n48-r2 frame W48-g9-rideparry: "whole-screen rainbow colour rings across the full view"): Unity-only
// numbers. The sim never reads these. Traced from the frame strip (Logs/feel/n48-r2/w-on/a148 f0947 -> f0951, telemetry "hit" at f949): the rings
// are the shaft's red-orange hot seams split into red / green / blue copies by URP's chromatic aberration at full intensity. The forced ride-parry
// ember hit THE WICK while a plate was cracked (OPEN), and feel43's open-boss read (GunFx.Hits43.cs OnBossHit) kicked PostFx with
// UnityTuning.BossOpen43Chroma 0.6. PostFx's pulse is in post.js units (post.chromaHurt 0.006 is the player's hurt), x ChromaScale 36: 0.6 gives
// 21.6, clamped to 1.0 (the whole screen at full split), and at post.chromaDecay 0.03 a second it stayed at full for about 19 s, on every
// OPEN hit of every boss. The open hit's own read stays: the white-hot star, the flare and the two rings at the hit point (local, world space),
// the trauma and the rumble.
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The open-boss hit's chroma kick as a share of post.chromaHurt (the player's own hurt kick: 0.006 x ChromaScale 36 = 0.22 URP
        // intensity, edge-weighted so the reticle's centre box stays clean, gone in 0.2 s): as strong as a hurt, never a 19 s full-screen wash.
        public const double Open48ChromaOfHurt = 1.0;
        // a148's check (the reel only): the URP chromatic aberration intensity at the ride-parry hit and 0.6 s on stays at or under this
        // (a hurt's 0.22 + margin; the G20 wash meter's +0.25 step); n48-r2 measured 1.00 (the frame's rainbow split)
        public const float Wick48ChromaMax = 0.25f;

        /// <summary>The open-boss hit's chroma kick in PostFx's pulse units (post.js): post.chromaHurt x Open48ChromaOfHurt.</summary>
        public static double Open48Chroma(GameConfig cfg) => cfg.Num("post.chromaHurt") * Open48ChromaOfHurt;
    }
}
