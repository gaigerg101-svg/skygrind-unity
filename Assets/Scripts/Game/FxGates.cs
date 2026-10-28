// NIGHT16 polish #41 / #43 / #44 (builder V): the pure gates the effects read (FxGatesTests):
// - #41 particles.js :135: a cube's scale is the pool's cube size (fx.size) x the burst's scale x the life left;
// - #43 the port-only motion flourishes (the enemy flinch and vibration, the THUNDERCLAP's shells) run as CameraFeel's do:
//   not with REDUCED MOTION, not at EFFECTS INTENSITY 0;
// - #44 a THUNDERCLAP shotResult whose pellets hit nothing (Combat.cs fireShotgun leaves surface "world" and the end at
//   origin + aim x weapons.shotgun.range then): no sparks and no light pop in mid-air. Read-only facts of the event; Core
//   is untouched.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class FxGates
    {
        /// <summary>particles.js :135 a cube's scale: (life / maxLife) x p.scale, on the pool's cube size (fx.size).</summary>
        public static float ParticleScale(float size, float scale, float age, float life) => life > 0 ? size * scale * Math.Max(0, 1 - age / life) : 0;

        /// <summary>A motion flourish runs (CameraFeel's reading): not with REDUCED MOTION, not at effects intensity 0.</summary>
        public static bool MotionOn(bool reducedMotion, double effectsIntensity) => !reducedMotion && effectsIntensity > 0;

        /// <summary>#50 particles.js setCap(n): max(1, min(poolSize, floor(n))).</summary>
        public static int ParticleCap(int poolSize, double presetCap) => Math.Max(1, Math.Min(poolSize, (int)Math.Floor(presetCap)));

        /// <summary>#50 particles.js take(): the ring over [0, cap), the oldest recycled first; returns the slot, advances `next`.</summary>
        public static int TakeSlot(ref int next, int cap)
        {
            if (next >= cap) next = 0; // if (next >= cap) next = 0;
            int slot = next; // const p = parts[next];
            next = (next + 1) % cap; // next = (next + 1) % cap; // oldest particles are recycled first
            return slot;
        }

        /// <summary>A quality preset's number: config.quality.presets.{low, medium, high} (anything else: high, as LightAndLens).</summary>
        public static double PresetValue(string level, double low, double medium, double high) => level == "low" ? low : level == "medium" ? medium : high;

        /// <summary>#51 impactFx.js :63 Math.round(18 x detail) cubes (detail: the preset's `impact`).</summary>
        public static int ImpactBurstCount(double detail) => (int)JsMath.Round(UnityTuning.ImpactRingBurstCount * detail);

        /// <summary>#52 gunFx.js :120-122: a melee hit of the player's (with a position) sparks.</summary>
        public static bool MeleeSparks(HitEvent e) => e.owner == "player" && e.melee; // mine(); if (!e.melee || !e.position) return (a Vec3 is always set)

        /// <summary>#53 the death burst's height: husk.js :107 at aimHeight (the KillEvent's AimPoint), enemyKind.js :67 at height x 0.5.</summary>
        public static double DeathBurstY(string kind, double positionY, double height, double aimPointY) => kind == "husk" ? aimPointY : positionY + height * 0.5;

        /// <summary>#58 the OVERLOAD stand-in burst: for the player's (gunFx.js :96 mine()).</summary>
        public static bool OverloadBurst(ArsenalEvent e) => e.owner == "player";

        /// <summary>A THUNDERCLAP shot whose pellets hit nothing at all (its end is the aim at the shotgun's range).</summary>
        public static bool ShotgunMissedAll(ShotEvent e, double shotgunRange) =>
            e.weapon == "shotgun" && e.surface == "world" && Vec3.DistanceTo(e.origin, e.end) >= shotgunRange - UnityTuning.ShotgunMissTolerance;
    }
}
