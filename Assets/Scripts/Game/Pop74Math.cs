// PG3 V3 (KILL POP, row pop74; the pure half of Pop74.cs, pinned by Pop74Tests). No engine objects: sizes by kind, the tier's scale, the counts a pop throws and how a
// crowded frame thins them, the flash's life in frames, the shard's flight and bounce, and the palette (enemy red-orange and white-hot, never cyan). Numbers: UnityTuning.PG3V3.cs.
using UnityEngine;

namespace Skygrind.Game
{
    public static class Pop74Math
    {
        /// <summary>The dying body's size in metres (its height) for the regular kinds the table lists; 0 for everything else: a boss (kind "boss", or a boss body's own kind: wyrm,
        /// loom, stag, crown ...), the RIVAL (magenta, its own death), a tower. A kind not in the table is never popped.</summary>
        public static float BodyM(string kind)
        {
            switch (kind)
            {
                case "husk": return UnityTuning.PG3V3BodyHuskM;
                case "swarmer": return UnityTuning.PG3V3BodySwarmerM;
                case "spitter": return UnityTuning.PG3V3BodySpitterM;
                case "charger": return UnityTuning.PG3V3BodyChargerM;
                case "warden": return UnityTuning.PG3V3BodyWardenM;
                case "drone": return UnityTuning.PG3V3BodyDroneM;
                case "burrower": return UnityTuning.PG3V3BodyBurrowerM;
                case "lancer": return UnityTuning.PG3V3BodyLancerM;
            }
            return 0f;
        }

        /// <summary>True for the kinds the table lists.</summary>
        public static bool Listed(string kind) => BodyM(kind) > 0f;

        /// <summary>The pop's tier: the kill confirm's (0 plain, 1 head or melee, 2 multikill) when one came (confirmed >= 0), else the kill's own: a head or melee kill is 1.</summary>
        public static int Tier(int confirmed, bool head, bool melee)
        {
            if (confirmed >= 0) return confirmed > 2 ? 2 : confirmed;
            return head || melee ? 1 : 0;
        }

        /// <summary>x the unit for a tier: 1, the head / melee scale, the multikill scale.</summary>
        public static float TierK(int tier) => tier >= 2 ? UnityTuning.PG3V3Tier2K : tier == 1 ? UnityTuning.PG3V3Tier1K : 1f;

        /// <summary>A far pop grows with distance (1 inside the reference distance, up to the cap) so it reads at 25 m.</summary>
        public static float FarK(float distM) => Mathf.Clamp(distM / UnityTuning.PG3V3FarRefM, 1f, UnityTuning.PG3V3FarMaxK);

        /// <summary>The pop's unit (m): the body's size x its tier x the far growth. Every size of the pop is a multiple of it.</summary>
        public static float Unit(float bodyM, int tier, float distM) => bodyM * TierK(tier) * FarK(distM);

        /// <summary>The ring's final radius (m) for a unit (a husk's plain pop: about 2 m).</summary>
        public static float RingRadiusM(float unit) => unit * UnityTuning.PG3V3RingReachK;

        /// <summary>The ring quad's width (m) whose texture band lands at `radius`: the band sits at PG3V3RingBand of the quad's half width.</summary>
        public static float RingQuadM(float radius) => 2f * radius / UnityTuning.PG3V3RingBand;

        /// <summary>A crowded frame's share of the shards and embers for the pop at `index` of the frame's kills: the first few whole, the next half, the rest a quarter.</summary>
        public static float LodK(int index) => index < UnityTuning.PG3V3LodFullKills ? 1f : index < UnityTuning.PG3V3LodHalfKills ? UnityTuning.PG3V3LodHalfK : UnityTuning.PG3V3LodFarK;

        /// <summary>A crowded frame's share of each flash's brightness for the pop at `index` (the flashes overlap on one screen spot, so the later ones are dimmer; no full-screen flash).</summary>
        public static float FlashK(int index) => index < UnityTuning.PG3V3LodFullKills ? 1f : index < UnityTuning.PG3V3LodHalfKills ? UnityTuning.PG3V3CrowdFlashHalfK : UnityTuning.PG3V3CrowdFlashFarK;

        public static int ShardCount(int tier, int index)
        {
            int n = tier >= 2 ? UnityTuning.PG3V3Shards2 : tier == 1 ? UnityTuning.PG3V3Shards1 : UnityTuning.PG3V3Shards0;
            return Mathf.Max(UnityTuning.PG3V3MinShards, Mathf.RoundToInt(n * LodK(index)));
        }

        public static int EmberCount(int tier, int index)
        {
            int n = tier >= 2 ? UnityTuning.PG3V3Embers2 : tier == 1 ? UnityTuning.PG3V3Embers1 : UnityTuning.PG3V3Embers0;
            return Mathf.Max(UnityTuning.PG3V3MinEmbers, Mathf.RoundToInt(n * LodK(index)));
        }

        /// <summary>The flash's life in seconds: the base, but never fewer than `minFrames` rendered frames at this frame time, never more than `maxS`.</summary>
        public static float FlashLife(float baseS, int minFrames, float frameDt, float maxS) => Mathf.Min(maxS, Mathf.Max(baseS, minFrames * Mathf.Max(0f, frameDt)));

        /// <summary>The shard speed's scale for a body's size: the square root of its share of a husk's, kept inside the limits.</summary>
        public static float ShardSpeedK(float bodyM) => Mathf.Clamp(Mathf.Sqrt(Mathf.Max(0f, bodyM) / UnityTuning.PG3V3ShardSpeedRefM), UnityTuning.PG3V3ShardSpeedKMin, UnityTuning.PG3V3ShardSpeedKMax);

        /// <summary>A shard's width (m) from a 0..1 die roll: the unit's share, never under the minimum.</summary>
        public static float ShardWidthM(float unit, float r01) => Mathf.Max(UnityTuning.PG3V3ShardMinM, unit * Mathf.Lerp(UnityTuning.PG3V3ShardSizeMinK, UnityTuning.PG3V3ShardSizeMaxK, Mathf.Clamp01(r01)));

        /// <summary>One shard step of `dt`: gravity, drag, motion; a floor under it (floorY, -infinity for none) bounces it (a share of its fall speed, less sideways) or, slow, rests it.
        /// A resting shard no longer moves. Returns true when it bounced or came to rest this step.</summary>
        public static bool ShardStep(ref Vector3 pos, ref Vector3 vel, ref bool rest, float floorY, float gravity, float drag, float dt)
        {
            if (rest || dt <= 0f) return false;
            vel.y -= gravity * dt;
            vel *= Mathf.Exp(-drag * dt);
            pos += vel * dt;
            float lo = floorY + UnityTuning.PG3V3ShardFloorLiftM;
            if (pos.y > lo) return false;
            pos.y = lo;
            if (vel.y < -UnityTuning.PG3V3ShardBounceMinSpeed)
            {
                vel.y = -vel.y * UnityTuning.PG3V3ShardBounce;
                vel.x *= UnityTuning.PG3V3ShardFriction;
                vel.z *= UnityTuning.PG3V3ShardFriction;
            }
            else
            {
                vel = Vector3.zero;
                rest = true;
            }
            return true;
        }

        /// <summary>The palette: the enemy's own red-orange lerped toward white by `mix` (0: the enemy colour, 1: white). It stays on the red-orange side of every colour: red >= green >= blue.</summary>
        public static Color Hot(Color enemy, float mix) => Color.Lerp(enemy, Color.white, Mathf.Clamp01(mix));

        /// <summary>The light a tier throws (LightPools.Flash intensity).</summary>
        public static float LightFor(int tier) => tier >= 2 ? UnityTuning.PG3V3Light2 : tier == 1 ? UnityTuning.PG3V3Light1 : UnityTuning.PG3V3Light0;

        /// <summary>True when a kill's confirm position names the kill's own (the confirm lifts y by fx.killConfirm.lift; x and z are the kill's exactly).</summary>
        public static bool SameSpot(double ax, double az, double bx, double bz) => System.Math.Abs(ax - bx) < 1e-4 && System.Math.Abs(az - bz) < 1e-4;
    }
}
