// GG 2026-10-26: the rooms, the boss rooms most, are too dark. Bright71 is the brightness floor of the whole frame's ambient light: the last
// writer each frame (DefaultExecutionOrder 10000, LateUpdate), after the room themes (RoofView27 / look33), the boss grades (BossGrade54 / 55,
// whose layers re-take their base when another writer changed it) and the dusk sky. Each ambient colour, the fog colour and the fog density are
// lifted to UnityTuning.Bright71* floors. The lift keeps the hue (the colour is scaled, not replaced) and is idempotent (a colour already at
// or over its floor is left alone), so nothing compounds frame over frame. View only: the sim never reads RenderSettings. Off:
// -skygrindNoBright71 (a player's switch: N29Fixes / Room71.PlayerSwitch71), or design.bright71.enabled false.
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    [DefaultExecutionOrder(10000)]
    public sealed class Bright71 : MonoBehaviour
    {
        public const string OffFlag = "-skygrindNoBright71";

        /// <summary>On unless the switch or the design row turns it off (decided once, at boot).</summary>
        public static bool On { get; private set; } = true;

        /// <summary>Reads the switch and the row; adds the component to `host`. Returns it (null when off).</summary>
        public static Bright71 Create(GameConfig cfg, GameObject host)
        {
            On = !HasArg(OffFlag) && !(cfg != null && cfg.Has("design.bright71.enabled") && cfg.Get("design.bright71.enabled") is bool b && !b);
            if (!On) { Debug.Log("[bright71] off (" + OffFlag + " or design.bright71.enabled false)"); return null; }
            Debug.Log("[bright71] on: ambient floors " + UnityTuning.Bright71AmbientSky + " / " + UnityTuning.Bright71AmbientEquator + " / " + UnityTuning.Bright71AmbientGround + ", fog colour " + UnityTuning.Bright71FogColour + ", fog density max " + UnityTuning.Bright71FogDensityMax);
            return host.AddComponent<Bright71>();
        }

        static bool HasArg(string flag)
        {
            foreach (string a in Environment.GetCommandLineArgs()) if (string.Equals(a, flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>The colour scaled so its brightest channel is at least `floor` (hue kept; at most MaxGain; a near-black colour gets a grey of
        /// the floor's value). Idempotent: a colour at or over the floor comes back unchanged.</summary>
        public static Color Lift(Color c, float floor)
        {
            float m = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            if (m >= floor) return c;
            if (m < 1e-4f) return new Color(floor * 0.6f, floor * 0.55f, floor * 0.75f, c.a); // a black colour: a dusk-violet grey, not neutral
            float gain = Mathf.Min(floor / m, UnityTuning.Bright71MaxGain);
            Color o = new Color(c.r * gain, c.g * gain, c.b * gain, c.a);
            float m2 = Mathf.Max(o.r, Mathf.Max(o.g, o.b));
            if (m2 < floor) o = Color.Lerp(o, new Color(floor * 0.6f, floor * 0.55f, floor * 0.75f, c.a), (floor - m2) / floor); // the gain cap hit: fill the rest
            return o;
        }

        /// <summary>GG 2026-10-26 ("textures overlapping that bug out"): the shipped levels have 28-75 pairs of boxes whose faces share a plane
        /// and overlap (Logs/n72-fix/zfight-scan.cjs: foundry 75, kiln 55, spire 65, iceline 33, span 28), so the depth test flips between them
        /// as the view moves. The DRAWN box (never the collider: the sim reads the level data) grows by a few mm, a different amount per box index,
        /// so two faces on one plane separate. Decor stands 20-80 mm off its faces (UnityTuning *Lift / Look33MountGap), so it is never swallowed.</summary>
        public static void Grow(ref Vec3 min, ref Vec3 max, int index)
        {
            if (!On) return;
            double e = UnityTuning.Bright71GrowStep * (1 + index % UnityTuning.Bright71GrowSteps);
            min = new Vec3(min.x - e, min.y - e, min.z - e);
            max = new Vec3(max.x + e, max.y + e, max.z + e);
        }

        /// <summary>NIGHT72 L5: the same growth by a CLASS (GrowPlan72.Classes: chosen per map so no coplanar overlapping pair keeps one plane) instead of the index mod 3.</summary>
        public static void GrowClass(ref Vec3 min, ref Vec3 max, int cls)
        {
            if (!On) return;
            double e = GrowPlan72.Growth(cls);
            min = new Vec3(min.x - e, min.y - e, min.z - e);
            max = new Vec3(max.x + e, max.y + e, max.z + e);
        }

        /// <summary>A stone colour with its brightest channel at least UnityTuning.Bright71AlbedoFloor (hue kept), when Bright71 is on: the boss
        /// arenas' stone (Light54.StoneOrLit's colours: 0x241f2e-0x2b2536) is so dark no light makes it read. Colours already brighter are unchanged.</summary>
        public static Color Albedo(Color c) => On ? Lift(c, UnityTuning.Bright71AlbedoFloor) : c;

        static readonly int ZenithId = Shader.PropertyToID("_Zenith"), MidId = Shader.PropertyToID("_Mid"), HorizonId = Shader.PropertyToID("_Horizon");

        static void LiftSky(Material sky, int id, float floor)
        {
            if (!sky.HasProperty(id)) return;
            Color c = sky.GetColor(id), l = Lift(c, floor);
            if (l != c) sky.SetColor(id, l);
        }

        void LateUpdate()
        {
            // a boss fight (the grades' weight, 0-1) lifts the floors further: the arenas' walls facing the player still read black at the room
            // floors (n71-polish4: THE ICEBREAKER's pillars, THE LOOM's walls; mean luma 40 / 34 with Bright71 on)
            float bw = Mathf.Clamp01(Mathf.Max(BossGrade54.WeightNow, BossGrade55.WeightNow));
            float skyF = Mathf.Lerp(UnityTuning.Bright71AmbientSky, UnityTuning.Bright71BossAmbientSky, bw);
            float eq = Mathf.Lerp(UnityTuning.Bright71AmbientEquator, UnityTuning.Bright71BossAmbientEquator, bw);
            float gr = Mathf.Lerp(UnityTuning.Bright71AmbientGround, UnityTuning.Bright71BossAmbientGround, bw);
            if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor = Lift(RenderSettings.ambientSkyColor, skyF);
                RenderSettings.ambientEquatorColor = Lift(RenderSettings.ambientEquatorColor, eq);
                RenderSettings.ambientGroundColor = Lift(RenderSettings.ambientGroundColor, gr);
            }
            else if (RenderSettings.ambientMode == AmbientMode.Flat)
                RenderSettings.ambientLight = Lift(RenderSettings.ambientLight, Mathf.Lerp(UnityTuning.Bright71AmbientFlat, UnityTuning.Bright71BossAmbientEquator, bw));
            // the sky (Look.Apply's skybox; DuskClock22 / HuntSky20 animate it): the open arenas' top half read black (n71-polish1 crown: 51 % of
            // the frame under luma 30); its three bands get the same hue-keeping floor
            Material sky = RenderSettings.skybox;
            if (sky != null)
            {
                LiftSky(sky, ZenithId, UnityTuning.Bright71SkyZenith);
                LiftSky(sky, MidId, UnityTuning.Bright71SkyMid);
                LiftSky(sky, HorizonId, UnityTuning.Bright71SkyHorizon);
            }
            if (RenderSettings.fog)
            {
                RenderSettings.fogColor = Lift(RenderSettings.fogColor, UnityTuning.Bright71FogColour);
                if (RenderSettings.fogMode != FogMode.Linear && RenderSettings.fogDensity > UnityTuning.Bright71FogDensityMax)
                    RenderSettings.fogDensity = UnityTuning.Bright71FogDensityMax;
            }
        }
    }
}
