// NIGHT12 U3-25.3 / U3-25.4 (agent B): the Unity-only render numbers of the enemy shader (SKYGRIND/Enemy), the death
// dissolve, the victim's flinch and vibration, the instanced particles and the kill confirm. Each has no config.js
// counterpart (a web shader literal, a pool limit, a render queue or a port-only look); the gameplay and tuned FX
// numbers stay in PortExport/config.json (fx.enemyRim*, aim.xray, aura.materializeTime / edgeGlow, fx.killConfirm,
// fx.poolSize / size / gravity, husk.hitFlashTime).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // dissolve.js createDissolveUniforms: uCell 40 (cells per metre: 2.5 cm blocks) and uBand 0.12 (the bright edge
        // band's share of the noise). Literals in the web module, not config.js; the death dissolve reuses them.
        public const float EnemyDissolveCells = 40f;
        public const float EnemyDissolveBand = 0.12f;

        // The dissolve's noise: a texture made in code at load (texels a side, a power of two; dissolve.js hashes with sin,
        // which loses precision on some GPUs) and its Mulberry32 seed (cosmetic, fixed: the same cells every run).
        public const int EnemyNoiseSize = 64;
        public const uint EnemyNoiseSeed = 0x2f6b3a91u;

        // Dying enemies drawn through their dissolve at once (a pool; a kill past it takes the oldest), and the box (m a
        // side) each corpse part is culled with (the parts are under 2 m).
        public const int EnemyCorpseMax = 32;
        public const float EnemyCorpseBounds = 4f;

        // The rim's distance boost (Research/2026-10-03-polish.md C1, Valorant's clarity rule: far enemies' fresnel grows,
        // clamped; rimGlow.js is flat): x1 inside RimBoostNear m, x RimBoostMax from RimBoostFar m, linear between.
        public const float EnemyRimBoostNear = 15f;
        public const float EnemyRimBoostFar = 60f;
        public const float EnemyRimBoostMax = 1.6f;

        // xray.js renderOrder 5: the x-ray material draws this far into the transparent queue, after every opaque wall
        // has written its depth (its pass tests ZTest Greater against it).
        public const int EnemyXrayQueueOffset = 5;

        // The victim's flinch and vibration (Research/2026-10-03-polish.md B3 after Sakurai; the web has neither; render
        // only, the hit shapes never move). Per hit: pushed back FlinchPush m and squashed FlinchSquash along the hit
        // axis over FlinchIn s, back over FlinchOut s (world time: it holds through a hitstop); a vibration of VibAmp m
        // on the hit axis at VibHz, decaying to 0 over VibTime s on frame time (it plays through the hitstop, as
        // Sakurai's victim shakes while both freeze). HitSlots: enemies tracked at once (the oldest is reused).
        public const float EnemyFlinchSquash = 0.06f;
        public const float EnemyVibAmp = 0.03f;
        public const float EnemyVibHz = 30f;
        public const float EnemyVibTime = 0.1f;
        public const int EnemyHitSlots = 32;

        // Graphics.RenderMeshInstanced's per-call instance limit (a MaterialPropertyBlock array can't hold more): the
        // particle pool (fx.poolSize, 400) is drawn in one call under it.
        public const int InstancedMax = 1023;

        // killConfirm.js renderOrder 2: the rings draw this far into the transparent queue.
        public const int KillConfirmQueueOffset = 2;
    }
}
