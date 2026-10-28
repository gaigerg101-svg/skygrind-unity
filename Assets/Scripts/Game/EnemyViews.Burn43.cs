// NIGHT43 builder A1 (round 2, the lead's ask: W1's torch43 sets an enemy hit by the TORCH burning for ~3 s; W1 names the accessor): what a
// BURNING enemy looks like. Wired (round 2) to W1's Loadout.BurnLeft43 (0..1 of the burn left: 1 freshly lit, fading to 0; 0 with torch43 off:
// nothing drawn); EnemyLook.Burning43 overrides it (a reel / a test). `burn43` (the ignition) flares fire cubes at the body's aim point. Each frame (EnemyLook.Frame, once) every live burning enemy gets flame TONGUES licking
// up its body (Mesh43.Spike, additive, hot orange to a yellow-white core, flickering in height and sway, more and taller the fresher the burn)
// drawn with Graphics.RenderMesh (no GameObject, no allocation per frame), and React's NavFlicker43 slot carries a visor flicker for it
// (models43's VISOR gutters as it burns). Render only. The colour is the fire's, not a team's: hot orange is the enemies' and danger's (rule 8).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class EnemyLook
    {
        /// <summary>An override of the burn of an enemy, 0..1 (a reel's or a test's; null: W1's Loadout.BurnLeft43, burnDefault).</summary>
        public static System.Func<Enemy, float> Burning43;
        System.Func<Enemy, float> burnDefault;

        Material burnMat;
        RenderParams burnRp;
        readonly MaterialPropertyBlock burnBlock = new MaterialPropertyBlock();
        readonly int[] burnIds = new int[UnityTuning.Burn43Max];
        readonly float[] burnShare = new float[UnityTuning.Burn43Max];
        int burnCount;

        /// <summary>Round 2 (W1's torch43 on port/stage3): the burn is W1's Loadout.BurnLeft43 (0..1 of the burn left; 0 not burning or the row
        /// off), drawn whenever it burns (a weapon's read, with models43 on or off); `burn43` (Torch43.Event, at ignition) flares fire at its aim point.</summary>
        void Burn43Init()
        {
            burnDefault = e =>
            {
                Loadout l = game.sim != null ? game.sim.loadout : null;
                return l != null ? (float)l.BurnLeft43(e) : 0;
            };
            Color flare = Look.Hex(UnityTuning.Burn43OuterHex), core = Look.Hex(UnityTuning.Burn43CoreHex);
            game.events.On<Burn43Event>(Torch43.Event, e =>
            {
                bool p47 = Fx47View.RowOn(game.cfg); // NIGHT47 F fx47: the TORCH's burn in its own plasma (blue tongues, a white core)
                game.Particles43?.Burst(e.position, p47 ? Look.Hex(UnityTuning.Torch47BlueHex) : flare, UnityTuning.Burn43IgniteCount, 3.2f, 0.35f, 0.9f);
                game.Particles43?.Burst(e.position, p47 ? Look.Hex(UnityTuning.Torch47CoreHex) : core, UnityTuning.Burn43IgniteCount / 2, 2.0f, 0.25f, 0.7f);
            });
        }

        /// <summary>Once a frame (from Frame): the tongues on every burning enemy; the ids and shares React reads.</summary>
        void Burn43Frame()
        {
            burnCount = 0;
            System.Func<Enemy, float> of = Burning43 ?? burnDefault; // a test's / reel's override, else W1's BurnLeft43
            if (of == null || game.flow == Flow.Title) return;
            if (burnMat == null)
            {
                burnMat = new Material(game.mats.neon) { name = "Burn43", renderQueue = (int)RenderQueue.Transparent };
                burnMat.SetFloat("_SrcBlend", (float)BlendMode.One); // additive (the glob trail's recipe)
                burnMat.SetFloat("_DstBlend", (float)BlendMode.One);
                burnMat.SetFloat("_ZWrite", 0);
                burnMat.SetFloat("_Cull", 0);
                burnRp = new RenderParams(burnMat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false, matProps = burnBlock };
            }
            Mesh tongue = Mesh43.Get(Mesh43.Spike);
            float t = Time.time;
            foreach (Enemy e in game.enemies.all)
            {
                if (!e.alive) continue;
                float s = Mathf.Clamp01(of(e));
                if (s <= 0) continue;
                if (burnCount < burnIds.Length)
                {
                    burnIds[burnCount] = e.id;
                    burnShare[burnCount] = s;
                    burnCount++;
                }
                Vector3 feet = WebSpace.ToUnity(e.position);
                float h = (float)e.Height, r = (float)e.Radius;
                int n = Mathf.Max(2, Mathf.RoundToInt(UnityTuning.Burn43Tongues * (0.5f + 0.5f * s)));
                for (int k = 0; k < n; k++)
                {
                    float a = k * 2.399963f + e.id; // the golden angle round the body
                    float fl = 0.65f + 0.35f * Mathf.Sin(t * (11 + k * 1.7f) + k * 2.1f);
                    float y = h * (0.15f + 0.6f * ((k * 0.37f) % 1));
                    Vector3 at = feet + new Vector3(Mathf.Sin(a) * r * 0.85f, y, Mathf.Cos(a) * r * 0.85f);
                    float len = UnityTuning.Burn43TongueLen * (0.5f + 0.5f * s) * fl * Mathf.Max(0.6f, h / 1.8f);
                    Quaternion q = Quaternion.Euler(Mathf.Sin(t * 7 + k) * 14, a * Mathf.Rad2Deg, Mathf.Cos(t * 6 + k) * 14);
                    var m = Matrix4x4.TRS(at + Vector3.up * len * 0.5f, q, new Vector3(UnityTuning.Burn43TongueWidth * fl, len, UnityTuning.Burn43TongueWidth * fl));
                    bool p47 = Fx47View.RowOn(game.cfg); // NIGHT47 F fx47: blue tongues (the player's plasma: a burning enemy reads as the TORCH's)
                    Color c = Color.Lerp(Look.Hex(p47 ? UnityTuning.Torch47BlueHex : UnityTuning.Burn43OuterHex), Look.Hex(p47 ? UnityTuning.Torch47CoreHex : UnityTuning.Burn43CoreHex), 0.5f + 0.5f * Mathf.Sin(t * 17 + k * 3)) * (UnityTuning.Burn43Glow * (0.4f + 0.6f * s));
                    c.a = 1;
                    burnBlock.SetColor(BaseColor, c);
                    burnRp.worldBounds = new Bounds(at, Vector3.one * (len + 1));
                    Graphics.RenderMesh(burnRp, tongue, 0, m);
                }
            }
        }

        /// <summary>The burn's visor flicker for enemy `id` (0 none): the VISOR gutters while it burns.</summary>
        float Burn43Flicker(int id)
        {
            for (int i = 0; i < burnCount; i++)
                if (burnIds[i] == id) return -0.5f * burnShare[i] * (0.5f + 0.5f * Mathf.Sin(Time.time * 23 + id));
            return 0;
        }
    }

    public static partial class UnityTuning
    {
        public const int Burn43Max = 32; // burning enemies tracked a frame (the visor flicker's table)
        public const float Burn43Tongues = 7; // flame tongues on a fresh burn (half as many at its end)
        public const float Burn43TongueLen = 0.55f; // m, a fresh tongue on a husk-sized body (x height / 1.8, min x 0.6)
        public const float Burn43TongueWidth = 0.16f; // m
        public const uint Burn43OuterHex = 0xFF5A14; // the flame's edge: hot orange
        public const uint Burn43CoreHex = 0xFFD890; // its core: a yellow-white heat (not the parry yellow 0xFFD23C)
        public const float Burn43Glow = 2.4f; // HDR x the colour (additive: the bloom picks it up)
        public const int Burn43IgniteCount = 10; // cubes of fire at the ignition (burn43), half as many again in the yellow-white core colour
    }
}
