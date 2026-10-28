// NIGHT20 U3-34.9 F2 (builder V): SKYGRIND TIME in the game (the rule: SkygrindTime20.cs).
// - The time: SkygrindTimeFrame(frameDt, share), called by the loop's ImpactBeginFrame (SurvivalGame.Impact.cs, the lead's
//   line) with the impact layer's share: returns the slower of the two (a freeze stays 0); SkygrindTimeHalt on the way to
//   TITLE. The row: design.adds.skygrindTime AND the run's design.adds bit (sim.n20.adds); on: the impact layer's slowMoOn
//   (SLOW-MO on, REDUCED MOTION off; automation plays the defaults); blocked: anything but Flow.Playing (DOWN, a menu).
// - The moments: the bus's `hit` (a shot's kills, a headshot kill) with the style's rank at the hit.
// - On its start: the port-only event `skygrindTime` (payload "blast" / "headshot") for GG's sounds and H's announcer slab,
//   and the reel's marker.
// - The look: a global Volume over Look's (priority StVolumePriority), its weight the rule's Look: the world's saturation
//   drains (StSaturation) and its colour leans toward INK (StInkLean); no hue the palette roles own moves (the filter is a
//   darkening toward the ink, not a tint). The AIR streaks freeze (SpeedLines20) and the travel blur lets go while it runs.
//   The Volume framework runs "Via Scripting" (PostFx, NIGHT13): the main camera's stack is refreshed on a frame the weight
//   changed (the ease in and out only).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        SkygrindTime20 skygrindTime;
        bool skygrindTimeRow; // design.adds.skygrindTime.enabled (read once)
        Volume stVolume;
        float stWeightShown = -1;

        /// <summary>SKYGRIND TIME's look share now (0..1): the screen toward INK, the streaks frozen.</summary>
        public double SkygrindTimeLook => skygrindTime != null ? skygrindTime.Look : 0;

        /// <summary>SKYGRIND TIME's starts this game (the telemetry's count).</summary>
        public int SkygrindTimeStarts => skygrindTime != null ? skygrindTime.Starts : 0;

        void SkygrindTimeBuild()
        {
            skygrindTime = new SkygrindTime20(SkygrindTime20.From(cfg));
            skygrindTimeRow = cfg.Design("adds.skygrindTime.enabled");
            events.On<HitEvent>("hit", e => skygrindTime.OnHit(e, style != null ? style.rank : 0));
        }

        /// <summary>The loop's line (ImpactBeginFrame): the world's share this frame with SKYGRIND TIME: the slower wins; a
        /// freeze (0) stays 0.</summary>
        double SkygrindTimeFrame(double frameDt, double share)
        {
            if (skygrindTime == null) SkygrindTimeBuild();
            skygrindTime.enabled = skygrindTimeRow && sim.n20.adds;
            skygrindTime.on = impactFrames != null && impactFrames.rules.slowMoOn;
            double s = skygrindTime.BeginFrame(frameDt, flow != Flow.Playing, share <= 0);
            if (skygrindTime.StartedThisFrame)
            {
                events.Emit(UnityTuning.StEvent, skygrindTime.Cause);
                FeelProbe.Event(UnityTuning.StEvent);
            }
            SkygrindTimeLookFrame();
            return share <= 0 ? share : Math.Min(share, s);
        }

        /// <summary>The way to TITLE (the loop's ImpactToTitle line): ended, the look gone.</summary>
        void SkygrindTimeHalt()
        {
            skygrindTime?.Halt();
            SkygrindTimeLookFrame();
        }

        /// <summary>The look's Volume at the rule's weight (made on the first frame it shows).</summary>
        void SkygrindTimeLookFrame()
        {
            float w = (float)SkygrindTimeLook;
            if (w <= 0 && stVolume == null) return;
            if (stVolume == null)
            {
                var go = new GameObject("SKYGRIND TIME (post)");
                stVolume = go.AddComponent<Volume>();
                stVolume.isGlobal = true;
                stVolume.priority = UnityTuning.StVolumePriority;
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                ColorAdjustments ca = profile.Add<ColorAdjustments>(false);
                ca.saturation.Override(UnityTuning.StSaturation);
                Color ink = UiTheme.Ready ? UiTheme.InkSolid : Color.black;
                Color filter = Color.Lerp(Color.white, ink, UnityTuning.StInkLean);
                filter.a = 1;
                ca.colorFilter.Override(filter);
                stVolume.sharedProfile = profile;
                stVolume.weight = 0;
            }
            w = Mathf.Round(w * 64) / 64; // a refresh only when the drawn weight moves a step
            if (w == stWeightShown) return;
            stWeightShown = w;
            stVolume.weight = w;
            if (cam != null && cam.GetVolumeFrameworkUpdateMode() == VolumeFrameworkUpdateMode.ViaScripting)
            {
                UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
                if (data.volumeStack != null && !data.volumeStack.isValid) cam.DestroyVolumeStack(data);
                cam.UpdateVolumeStack(data);
            }
        }
    }
}
