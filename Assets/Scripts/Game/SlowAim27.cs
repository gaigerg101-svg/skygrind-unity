// NIGHT27 slowAim27 (builder V): the SCREEN CUE of the wall-ride aim slow-down. The loop hook is the lead's (SurvivalGame.SlowAim27.cs:
// SlowAim01); this ships one global URP Volume driven by a single value, slow01 (0 real time .. 1 full slow): vignette 0..cueVignette,
// chromatic aberration 0..cueChroma, saturation 0..cueSaturation (-15) and a FOV add (+4, applied by the caller after the camera feel).
// NO border speed lines (GG banned them). Numbers: design.n27.slowAim27.cue* (each with a _why). View only: nothing here touches the sim.
// The lead's call, once per frame after the camera feel: `slowAim27?.Set((float)SlowAim01);` and `cam.fieldOfView += slowAim27.FovAdd;`.
// NIGHT35 V (design.n35.ridelook35; GG: "a bluer overlay or some ... gamey glitchy effect" when wall riding + zooming): a second Volume, the FOCUS
// look, driven by its own focus01 (wall-riding AND zoomed, read from the sim's state each frame, eased on real time; it shows with SLOW-MO off too):
// a cold-blue colour filter, cooler white balance, less saturation, more contrast, strong edge chroma (URP's aberration is edge-weighted) and a
// deep-blue vignette; plus, through the CRT pass (Crt29.SetRide35), a faint band tear and a bright scanline sweeping down (both held still with
// REDUCED MOTION). Blue = focus; the bend's grade stays grey (Bend34View). Numbers: UnityTuning.Look35.cs (Ride35*). -skygrindOff35: none of it.
// NIGHT39 F1 (design.n39.slow39 on): the cue is built for slow39 too and takes UnityTuning.Slow39Cue* (deeper); SlowAim01 is then slow39's depth (the
// ride, the whip latch and the parry window). Off (or -skygrindOff39): NIGHT38's cue.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    public sealed class SlowAim27
    {
        /// <summary>The world's time share at slow01 (1 = real time, `min` at full): the lerp the loop driver uses.</summary>
        public static float Scale(float slow01, float min) => 1f - (1f - min) * Mathf.Clamp01(slow01);

        /// <summary>One eased step of slow01 toward `target` (0 or 1): in over `inS`, out over `outS` seconds (linear, as the loop hook).</summary>
        public static float Ease(float current, bool target, float dt, float inS, float outS)
        {
            return target ? Mathf.Min(1f, current + dt / Mathf.Max(1e-4f, inS)) : Mathf.Max(0f, current - dt / Mathf.Max(1e-4f, outS));
        }

        Camera cam; // NIGHT35 V: bound lazily (Bind): Create runs before SurvivalGame.Init sets cam
        UniversalAdditionalCameraData camData;
        readonly Volume volume;
        readonly Vignette vignette;
        readonly ChromaticAberration chroma;
        readonly ColorAdjustments color;
        readonly float fovAdd;
        float shown = -1;
        readonly float cueVig, cueChroma, cueSat;

        /// <summary>The FOV to add this frame (degrees; 0 when off).</summary>
        public float FovAdd { get; private set; }

        // NIGHT35 ridelook35: the focus look (null with the row off)
        readonly bool slowOn;
        readonly Volume focusVolume;
        readonly ColorAdjustments focusColor;
        readonly WhiteBalance focusWb;
        readonly ChromaticAberration focusChroma;
        readonly Vignette focusVignette;
        float focusShown = -1;
        /// <summary>The focus look's depth this frame (0 .. 1; the reel reads it).</summary>
        public static float Focus35Now;
        /// <summary>Times the focus look reached full depth (the reel's count).</summary>
        public static int Focus35Entries;
        /// <summary>The reel's override: the focus look at full depth whatever the sim's state (false in play).</summary>
        public static bool Force35;

        /// <summary>Built when design.n27.slowAim27.enabled is on (and not --off27); null otherwise.</summary>
        public static SlowAim27 Create(SurvivalGame g, Transform parent)
        {
            bool slow = (!N27Blocks.ForceOff && g.cfg.Design("n27.slowAim27.enabled")) || Slow39Rules.From(g.cfg).on; // NIGHT39 F1: slow39 drives the same cue
            bool ride = N35Fixes.ViewRowOn(g.cfg, "ridelook35"); // NIGHT35: the focus look stands on its own (no slow-mo needed)
            Focus35Now = 0;
            if (!slow && !ride) return null; // NIGHT35 V: no g.cam test here (Init sets cam after this call: the cue was never built)
            var cue = new SlowAim27(g, parent, slow, ride);
            var d = new GameObject("Slow aim cue driver");
            d.transform.SetParent(parent, false);
            d.AddComponent<SlowAim27Driver>().Init(g, cue);
            return cue;
        }

        SlowAim27(SurvivalGame g, Transform parent, bool slow, bool ride)
        {
            GameConfig cfg = g.cfg;
            Bind(g.cam);
            slowOn = slow;
            if (ride)
            {
                var fgo = new GameObject("Ride focus look 35 (Volume)");
                fgo.transform.SetParent(parent, false);
                focusVolume = fgo.AddComponent<Volume>();
                focusVolume.isGlobal = true;
                focusVolume.priority = 51; // over the slow-aim cue's Volume (50) and the post Volume
                focusVolume.weight = 0;
                var fp = ScriptableObject.CreateInstance<VolumeProfile>();
                focusColor = fp.Add<ColorAdjustments>(true);
                focusColor.colorFilter.Override(Look.Hex(UnityTuning.Ride35FilterHex));
                focusColor.saturation.Override(UnityTuning.Ride35Saturation);
                focusColor.contrast.Override(UnityTuning.Ride35Contrast);
                focusWb = fp.Add<WhiteBalance>(true);
                focusWb.temperature.Override(UnityTuning.Ride35Temperature);
                focusChroma = fp.Add<ChromaticAberration>(true);
                focusChroma.intensity.Override(UnityTuning.Ride35Chroma);
                focusVignette = fp.Add<Vignette>(true);
                focusVignette.color.Override(Look.Hex(UnityTuning.Ride35VignetteHex));
                focusVignette.intensity.Override(UnityTuning.Ride35Vignette);
                focusVignette.smoothness.Override(UnityTuning.Ride35VignetteSmooth);
                focusVolume.sharedProfile = fp;
            }
            var go = new GameObject("Slow aim cue (Volume)");
            go.transform.SetParent(parent, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50; // over the post Volume (Look.Apply's)
            volume.weight = 0;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override((float)cfg.Num("design.n27.slowAim27.cueVignette"));
            chroma = profile.Add<ChromaticAberration>(true);
            chroma.intensity.Override((float)cfg.Num("design.n27.slowAim27.cueChroma"));
            color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override((float)cfg.Num("design.n27.slowAim27.cueSaturation"));
            volume.sharedProfile = profile;
            fovAdd = (float)cfg.Num("design.n27.slowAim27.cueFovDeg");
            cueVig = (float)cfg.Num("design.n27.slowAim27.cueVignette"); cueChroma = (float)cfg.Num("design.n27.slowAim27.cueChroma"); cueSat = (float)cfg.Num("design.n27.slowAim27.cueSaturation");
            if (Slow39Rules.From(cfg).on)
            {
                // NIGHT39 F1 (design.n39.slow39): the deeper slow's cue (UnityTuning.Slow39.cs); SlowAim01 is slow39's depth (ride, latch, parry window)
                fovAdd = UnityTuning.Slow39CueFovDeg;
                cueVig = UnityTuning.Slow39CueVignette;
                cueChroma = UnityTuning.Slow39CueChroma;
                cueSat = UnityTuning.Slow39CueSaturation;
            }
        }

        /// <summary>The cue's one input. The Volume framework runs Via Scripting (PostFx), so the main camera's stack is refreshed when the value changes.</summary>
        public void Set(float slow01)
        {
            if (!slowOn) { FovAdd = 0; return; } // NIGHT35: built for the focus look alone
            slow01 = Mathf.Clamp01(slow01);
            FovAdd = fovAdd * slow01;
            if (slow01 <= 0 && shown <= 0) { shown = 0; return; } // idle: nothing to write
            shown = slow01;
            // ADD on top of the post Volume's live values (its vignette is 0.45 at rest and moves with hurt and beats): the Volume blends lerp(base, mine, weight),
            // so mine = base + cue gives base + cue x slow01. The values are re-read every active frame.
            PostStack post = Look.Post;
            vignette.intensity.value = Mathf.Min(1f, (post != null ? post.vignette.intensity.value : 0f) + cueVig);
            chroma.intensity.value = Mathf.Min(1f, (post != null ? post.chroma.intensity.value : 0f) + cueChroma);
            color.saturation.value = Mathf.Clamp((post != null ? post.color.saturation.value : 0f) + cueSat, -100f, 100f);
            volume.weight = slow01;
            RefreshStack();
        }

        /// <summary>NIGHT35 V: the camera whose Volume stack the cue refreshes (the driver passes game.cam every frame).</summary>
        public void Bind(Camera c)
        {
            if (c == null || c == cam) return;
            cam = c;
            camData = c.GetUniversalAdditionalCameraData();
        }

        void RefreshStack()
        {
            if (VolumeManager.instance.isInitialized && camData != null && cam != null)
            {
                if (camData.volumeStack != null && !camData.volumeStack.isValid) cam.DestroyVolumeStack(camData);
                cam.UpdateVolumeStack(camData);
            }
        }

        /// <summary>NIGHT35 ridelook35: one eased step of the focus look toward riding-and-zoomed (real time), the Volume and the CRT pass's tear / sweep.</summary>
        public void Focus(bool target, float dt, bool reducedMotion, double now)
        {
            if (focusVolume == null) { Focus35Now = 0; return; }
            float was = Focus35Now;
            Focus35Now = Ride35Math.Ease(Focus35Now, target || Force35, dt);
            if (Focus35Now >= 1f && was < 1f) Focus35Entries++;
            float k = Focus35Now;
            if (k <= 0 && focusShown <= 0) { focusShown = 0; Crt29.SetRide35(0, 0, 0, 0, 0); return; }
            focusShown = k;
            // ADD on top of the post Volume's live values (as Set does): the Volume blends lerp(base, mine, weight)
            PostStack post = Look.Post;
            focusChroma.intensity.value = Mathf.Min(1f, (post != null ? post.chroma.intensity.value : 0f) + UnityTuning.Ride35Chroma);
            focusVignette.intensity.value = Mathf.Min(1f, (post != null ? post.vignette.intensity.value : 0f) + UnityTuning.Ride35Vignette);
            focusColor.saturation.value = Mathf.Clamp((post != null ? post.color.saturation.value : 0f) + UnityTuning.Ride35Saturation, -100f, 100f);
            focusColor.contrast.value = Mathf.Clamp((post != null ? post.color.contrast.value : 0f) + UnityTuning.Ride35Contrast, -100f, 100f);
            focusVolume.weight = k;
            RefreshStack();
            Ride35Math.Glitch(k, now, reducedMotion, out float tear, out float step, out float sweep, out float sweepY);
            Crt29.SetRide35(tear, step, sweep, sweepY, UnityTuning.Ride35SweepHalf);
        }
    }

    /// <summary>Runs the cue after the frame's camera work: reads SurvivalGame.SlowAim01, sets the Volume and adds the cue's FOV on top of the camera feel
    /// (the feel writes the FOV absolutely every frame, so the add never accumulates). So the lead's only line is SlowAim27.Create(this, parent) in Init.</summary>
    public sealed class SlowAim27Driver : MonoBehaviour
    {
        SurvivalGame game;
        SlowAim27 cue;

        public SlowAim27 Cue => cue;

        public void Init(SurvivalGame g, SlowAim27 c) { game = g; cue = c; }

        void LateUpdate()
        {
            if (game == null || cue == null || game.cam == null) return;
            cue.Bind(game.cam);
            cue.Set((float)game.SlowAim01);
            game.cam.fieldOfView += cue.FovAdd;
            // NIGHT35 ridelook35: riding AND zoomed, read from the sim's state (view only), eased on the real frame time
            SurvivalSim sim = game.sim;
            bool target = sim != null && game.flow == Flow.Playing && sim.mover != null && sim.mover.state == MoveState.WallRide && sim.loadout != null && sim.loadout.adsActive;
            cue.Focus(target, FeelClock.UnscaledDelta, game.cfg.movement.settings.reducedMotion, FeelClock.UnscaledNow); // NIGHT63 D1: held in a same-run held frame
        }

        void OnDestroy()
        {
            SlowAim27.Focus35Now = 0;
            Crt29.SetRide35(0, 0, 0, 0, 0);
        }
    }
}
