// NIGHT34 builder H (design.n34.bendLook, a view row: no sim bit): THE BEND's look. GG (2026-10-11): the grapple triggers "a dramatic slow-motion
// effect with a clear visual overlay" ... "as the world bends around you". Driven by builder B's sim.Bend34Depth01 (0 .. 1, eased in 0.08 s and out
// 0.3 s by the sim), so the exit restores the look over the bend's own ease out. With the row on:
//   - THE GRADE (Bend34Grade, run from PostFx.Render so the Volume's scripted update sees it): a ColorCurves hueVsSat curve on the post Volume.
//     Every hue loses Bend34Desat (65 %) of its saturation at full depth except two bands, red-orange (enemies, danger) and cyan (player-usable),
//     which keep theirs: only enemies, danger and cyan pop (CLAUDE.md rule 8's palette made the read). The world leans to a neutral COOL grey
//     (ColorAdjustments' colour filter, never violet: pale violet-white is the boundary's). Each kill inside the bend (the sim's `bend34Kill`)
//     takes Bend34KillStep more, up to Bend34DesatMax; the steps clear when the bend has eased out.
//   - THE SKY: the hue curve alone would keep the orange dusk horizon saturated, so the sky's own tint dims toward its grey (the Sky material's
//     colours, the stone's sky-fog globals, RenderSettings' fog colour). Other systems write those colours (DuskClock22, HuntSky20, Boss29View,
//     PostFx's look33 globals): each channel keeps what WE wrote and takes the current value as its rest whenever someone else changed it, so the
//     bend dims whatever the world shows and puts it back exactly at depth 0.
//   - THE ENTRY THUMP (Bend34View, LateUpdate after the camera pass): the world FOV narrows Bend34EntryFovDeg (-4 deg) on `bend34Start` and comes
//     back over Bend34EntryFovS (0.15 s); x FOV KICK %, none with REDUCED MOTION. The viewmodel's camera is not touched (the gun never moves).
//   No screen-border lines, no vignette bars (retired w20): the effect lives in the post grade only.
// Off: the row off or -skygrindOff34 (N34Fixes.ViewRowOn): no curve is added to the Volume and nothing here runs. Numbers: UnityTuning.Bend34.cs.
// Unity objects are made in Attach / the PostFx constructor, never in a field initialiser (L-035).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    /// <summary>The bend's post grade and sky dim (owned by PostFx; built only with design.n34.bendLook on).</summary>
    public sealed class Bend34Grade
    {
        /// <summary>For the reels: the grade's strength now (0 .. Bend34DesatMax: the share of saturation taken outside the kept hues), the sky's
        /// dim share now (0 .. 1), the kills counted inside this bend.</summary>
        public static float GradeNow, SkyNow;
        public static int KillsNow;

        readonly SurvivalGame game;
        readonly ColorCurves curves;
        readonly TextureCurve hueVsSat;
        readonly ColorAdjustments color;
        readonly float[] keyT;
        readonly bool[] keyKept;
        int kills, shownStep = -1, shownSky = -1;

        struct Chan
        {
            public Color rest, written;
            public bool on;
        }

        Chan zenith, mid, horizon, sun, fogZ, fogM, fogH, fog;
        static readonly int ZenithId = Shader.PropertyToID("_Zenith"), MidId = Shader.PropertyToID("_Mid"), HorizonId = Shader.PropertyToID("_Horizon"), SunId = Shader.PropertyToID("_Sun");
        static readonly int FogZId = Shader.PropertyToID("_SkyFogZenith"), FogMId = Shader.PropertyToID("_SkyFogMid"), FogHId = Shader.PropertyToID("_SkyFogHorizon");

        public Bend34Grade(SurvivalGame game, PostStack post)
        {
            this.game = game;
            color = post.color;
            // the curve's keys: (hue, kept); the curve loops over 0 .. 1 (Keyframe tangents 0: an S between keys, flat at each)
            float f = UnityTuning.Bend34Fall;
            keyT = new[]
            {
                UnityTuning.Bend34RedHi, UnityTuning.Bend34RedHi + f, UnityTuning.Bend34CyanLo - f, UnityTuning.Bend34CyanLo,
                UnityTuning.Bend34CyanHi, UnityTuning.Bend34CyanHi + f, UnityTuning.Bend34RedLo - f, UnityTuning.Bend34RedLo,
            };
            keyKept = new[] { true, false, false, true, true, false, false, true };
            var keys = new Keyframe[keyT.Length];
            for (int i = 0; i < keys.Length; i++) keys[i] = new Keyframe(keyT[i], 0.5f); // 0.5 = the hue's saturation x 1: identity at rest
            hueVsSat = new TextureCurve(keys, 0.5f, true, new Vector2(0f, 1f));
            curves = post.profile.Add<ColorCurves>(false); // only hueVsSat overrides (the other curves stay at their identity defaults)
            curves.hueVsSat.Override(hueVsSat);
            if (!color.colorFilter.overrideState) color.colorFilter.Override(Color.white);
            game.events.On("bend34Kill", _ => kills++);
        }

        /// <summary>Every frame from PostFx.Render (after look33's globals): true when the Volume's values changed (PostFx marks its stack dirty).</summary>
        public bool Render(double frameDt)
        {
            SurvivalSim sim = game.sim;
            float depth = sim != null ? Mathf.Clamp01((float)sim.Bend34Depth01) : 0f;
            if (depth <= 0f) kills = 0;
            KillsNow = kills;
            float desat = Mathf.Min(UnityTuning.Bend34DesatMax, UnityTuning.Bend34Desat + kills * UnityTuning.Bend34KillStep) * depth;
            int step = Mathf.RoundToInt(desat * UnityTuning.Bend34GradeSteps);
            bool dirty = false;
            if (step != shownStep)
            {
                shownStep = step;
                float d = step / (float)UnityTuning.Bend34GradeSteps;
                GradeNow = d;
                float off = 0.5f * (1f - d);
                for (int i = 0; i < keyT.Length; i++) hueVsSat.MoveKey(i, new Keyframe(keyT[i], keyKept[i] ? 0.5f : off));
                // the cool grey leans in with the same share of the depth
                float g = Mathf.Clamp01(d / Mathf.Max(1e-3f, UnityTuning.Bend34Desat));
                color.colorFilter.value = new Color(Mathf.Lerp(1f, UnityTuning.Bend34GreyR, g), Mathf.Lerp(1f, UnityTuning.Bend34GreyG, g), Mathf.Lerp(1f, UnityTuning.Bend34GreyB, g), 1f);
                dirty = true;
            }
            Sky(depth);
            return dirty;
        }

        /// <summary>The sky's tint dims with the depth; whatever another system wrote since is the new rest (exactly restored at depth 0).</summary>
        void Sky(float depth)
        {
            int s = Mathf.RoundToInt(depth * UnityTuning.Bend34GradeSteps);
            bool any = zenith.on || mid.on || horizon.on || sun.on || fogZ.on || fogM.on || fogH.on || fog.on;
            if (s == 0 && !any) { SkyNow = 0; shownSky = 0; return; }
            float k = s / (float)UnityTuning.Bend34GradeSteps;
            SkyNow = k;
            bool force = s != shownSky;
            shownSky = s;
            Material m = RenderSettings.skybox;
            if (m != null)
            {
                if (m.HasProperty(ZenithId)) Mat(m, ZenithId, ref zenith, k, force);
                if (m.HasProperty(MidId)) Mat(m, MidId, ref mid, k, force);
                if (m.HasProperty(HorizonId)) Mat(m, HorizonId, ref horizon, k, force);
                if (m.HasProperty(SunId)) Mat(m, SunId, ref sun, k, force);
            }
            Global(FogZId, ref fogZ, k, force);
            Global(FogMId, ref fogM, k, force);
            Global(FogHId, ref fogH, k, force);
            Color fc = RenderSettings.fogColor;
            if (Step(ref fog, fc, k, force, out Color fw)) RenderSettings.fogColor = fw;
        }

        static void Mat(Material m, int id, ref Chan c, float k, bool force)
        {
            if (Step(ref c, m.GetColor(id), k, force, out Color w)) m.SetColor(id, w);
        }

        static void Global(int id, ref Chan c, float k, bool force)
        {
            if (Step(ref c, Shader.GetGlobalColor(id), k, force, out Color w)) Shader.SetGlobalColor(id, w);
        }

        /// <summary>One colour channel: `now` is what the world shows; true (with `write`) when it must be rewritten.</summary>
        static bool Step(ref Chan c, Color now, float k, bool force, out Color write)
        {
            bool other = !c.on || now != c.written; // someone else wrote it (or we never did): that is the rest now
            if (other) c.rest = now;
            if (k <= 0f)
            {
                write = c.rest;
                bool was = c.on;
                c.on = false;
                return was && !other; // put our dim back to the rest (another system's newer value is already there)
            }
            if (!other && !force) { write = now; return false; }
            Color r = c.rest;
            float luma = 0.2126f * r.r + 0.7152f * r.g + 0.0722f * r.b;
            var grey = new Color(luma * UnityTuning.Bend34GreyR, luma * UnityTuning.Bend34GreyG, luma * UnityTuning.Bend34GreyB, r.a) * UnityTuning.Bend34SkyDim;
            grey.a = r.a;
            write = Color.Lerp(r, grey, k * UnityTuning.Bend34SkyDesat);
            write.a = r.a;
            c.written = write;
            c.on = true;
            return true;
        }
    }

    /// <summary>The bend's entry thump on the world FOV (self-installing; view only).</summary>
    public sealed class Bend34View : MonoBehaviour
    {
        /// <summary>For the reels: the FOV added this frame (deg, negative in a thump), the bends entered (bend34Start) since the run began.</summary>
        public static float FovNow;
        public static int Entries;

        SurvivalGame game;
        bool rowOn;
        double thumpAge = double.MaxValue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Bend34View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Bend34View>();
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.cfg == null) { game = null; return false; }
            rowOn = N34Fixes.ViewRowOn(game.cfg, "bendLook");
            thumpAge = double.MaxValue;
            if (rowOn)
            {
                game.events.On("bend34Start", _ => { thumpAge = 0; Entries++; });
                game.OnNewRun += () => { Entries = 0; thumpAge = double.MaxValue; };
            }
            return true;
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (!rowOn) { FovNow = 0; return; }
            float add = 0f;
            if (thumpAge < double.MaxValue)
            {
                double u = thumpAge / UnityTuning.Bend34EntryFovS;
                thumpAge += Time.deltaTime; // frame time, as Grapple31View's FOV kick (the reels' FeelClock fixes it per recorded frame)
                if (u >= 1) thumpAge = double.MaxValue;
                else
                {
                    bool calm = game.cfg.movement.settings.reducedMotion;
                    add = calm ? 0f : UnityTuning.Bend34EntryFovDeg * (1f - UiTween.OutCubic(u)) * (float)CameraFeel.FovKickScale(game.settings);
                }
            }
            if (game.cam != null && add != 0f) game.cam.fieldOfView += add;
            FovNow = add;
        }
    }
}
