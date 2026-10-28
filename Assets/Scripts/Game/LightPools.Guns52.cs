// NIGHT52 builder U, guns52 (design.n52.guns52, view only; NIGHT52.md leftover 2): the muzzle LIGHT in real time, as star51 made the muzzle star.
// fx.flashFrames counts RENDER frames (2: 14 ms at 144 Hz, 33 ms at 60 Hz; GG plays at 104-172 FPS), so the shot's light on the room was a flicker
// that changed with the frame rate. With guns52 drawing (Guns52Probe.LiveNow) the light is lit by real time since the shot: full for
// N51StarFullS, its second share (fx.flashSecondFrame) to N51StarS (40 ms in all), and never less than the shot's frame; then it ends and a kill's
// pop may borrow the light again (flashTime 0). Off (the row off, -skygrindOff52, the a352 reel's ForceOff): the render-frame rule, as NIGHT51.
// The a352 reel reads Guns52Probe.LightMs / LightFrames: from the shot to the first frame the light is off, and the frames it was lit (either mode).
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class LightPools
    {
        double light52T0;
        bool light52Pending, light52First;
        int Light52Frames;

        /// <summary>guns52 draws now (the muzzle light by real time).</summary>
        static bool Light52On => Guns52Probe.LiveNow;

        /// <summary>From OnFire: the shot's real time (both modes: the reel's lifetime is measured from it).</summary>
        void Light52Fire()
        {
            light52T0 = FeelClock.UnscaledNow;
            light52Pending = true;
            light52First = true;
            Light52Frames = 0;
        }

        /// <summary>guns52's rule for this frame: lit (and its strength share `k`) while the shot's real-time window runs; at its end the flash is
        /// over (flashTime / flashFrames 0: a pop may take the light).</summary>
        bool Light52Lit(out double k)
        {
            k = 0;
            if (!light52Pending) return false;
            double age = FeelClock.UnscaledNow - light52T0;
            if (light52First || age < UnityTuning.N51StarS)
            {
                k = light52First || age < UnityTuning.N51StarFullS ? 1 : flashSecond;
                light52First = false;
                return true;
            }
            light52Pending = false;
            flashTime = 0;
            flashFrames = 0;
            return false;
        }

        /// <summary>The frame the flash went dark: its lifetime for the reel (ms from the shot) and the frames it was lit.</summary>
        void Light52Ended()
        {
            Guns52Probe.LightMs = (FeelClock.UnscaledNow - light52T0) * 1000.0;
            Guns52Probe.LightFrames = Light52Frames;
            Guns52Probe.LightEnds++;
        }
    }
}
