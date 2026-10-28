// NIGHT13 U3-26.7 (agent I): the pad's light rumble (keyboardMouse.js rumble :314-329; main.js :1337-1340): a hit taken,
// a kill, a parry, each config.gamepad.vibration [strong, weak, ms], only with VIBRATION on (settings.padVibration, off by
// default as on the web) and the pad the device used last. The web's 'dual-rumble' effect runs for its duration by itself;
// the Input System's SetMotorSpeeds (low = strong, high = weak) stays on until reset, so a frame-time timer (Tick, real
// time: hitstop and pause don't stretch it) turns it off after `ms`, and Stop() turns it off at once: on pause (the lead
// calls it in Pause), on focus loss (Application.focusChanged) and on quit (Application.quitting). Haptics only.
// NIGHT15 U3-28.8 (builder K; MAP-RESEARCH section 6): VIBRATION starts on (design.pad.vibrationDefault) and each motor
// is config.gamepad.vibration's strength x RUMBLE (settings.padRumble, 0..1; default design.pad.rumbleDefault).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed class Rumble : IDisposable
    {
        readonly double[] hurt, kill, parry; // [strong, weak, ms]
        Gamepad playing; // the pad rumbling now (null: none)
        double left; // s

        /// <summary>settings.padVibration (ReaderInput.ApplySettings keeps it in step).</summary>
        public bool enabled;

        /// <summary>NIGHT15 U3-28.8: RUMBLE (settings.padRumble, 0..1): the motors' strength (ReaderInput.ApplySettings keeps it in step).</summary>
        public double strength;

        // The web rumbles at config.gamepad.vibration's strengths as they are: x1 (no RUMBLE row).
        const double WebStrength = 1;

        /// <summary>NIGHT15 U3-28.8: a motor's speed (config.gamepad.vibration's strong or weak) at a RUMBLE strength, 0..1.</summary>
        public static double Scale(double motor, double strength) => System.Math.Max(0, System.Math.Min(1, motor * strength));

        public Rumble(GameConfig cfg)
        {
            hurt = cfg.Nums("gamepad.vibration.hurt");
            kill = cfg.Nums("gamepad.vibration.kill");
            parry = cfg.Nums("gamepad.vibration.parry");
            // NIGHT15 U3-28.8: VIBRATION and RUMBLE start as design.pad says, so a rumble before the settings apply agrees
            // with them; without the block, the web's (settings.padVibration, off; full strength).
            enabled = cfg.Has("design.pad.vibrationDefault") ? cfg.Bool("design.pad.vibrationDefault") : cfg.Bool("settings.padVibration");
            strength = cfg.Has("design.pad.rumbleDefault") ? cfg.Num("design.pad.rumbleDefault") : WebStrength;
            Application.focusChanged += OnFocus;
            Application.quitting += Stop;
        }

        public void Dispose()
        {
            Stop();
            Application.focusChanged -= OnFocus;
            Application.quitting -= Stop;
        }

        void OnFocus(bool focused)
        {
            if (!focused) Stop();
        }

        public void Hurt() => Play(hurt);

        /// <summary>NIGHT22 U3-36.8 (builder M): design.n22.haptics is live, so Haptics22 plays the kill's own pattern and the web's kill pulse stays quiet.</summary>
        public bool distinctKill;

        /// <summary>NIGHT22: would a Play be felt now (VIBRATION on, RUMBLE above 0, the pad the device in use, a pad present)? Haptics22's sink asks.</summary>
        public bool CanPlay => enabled && strength > 0 && PlayerDevices.Last == InputDevice.Pad && Gamepad.current != null;

        public void Kill()
        {
            if (distinctKill) return;
            Play(kill);
        }

        public void Parry() => Play(parry);

        void Play(double[] e) => Play(e[0], e[1], e[2]);

        /// <summary>A rumble of `ms` with the strong (low-frequency) and weak (high-frequency) motors, when VIBRATION is on and the pad is in use.</summary>
        public void Play(double strong, double weak, double ms)
        {
            if (!enabled || strength <= 0 || PlayerDevices.Last != InputDevice.Pad) return;
            Gamepad gp = Gamepad.current;
            if (gp == null) return;
            if (playing != null && playing != gp) playing.SetMotorSpeeds(0, 0);
            playing = gp;
            gp.SetMotorSpeeds((float)Scale(strong, strength), (float)Scale(weak, strength)); // NIGHT15: x RUMBLE
            left = ms / 1000;
        }

        /// <summary>Once a rendered frame with the real frame time: the rumble stops when its time is up.</summary>
        public void Tick(double frameDt)
        {
            if (playing == null) return;
            left -= frameDt;
            if (left <= 0) Stop();
        }

        /// <summary>The motors off now (pause, focus loss, quit, VIBRATION turned off).</summary>
        public void Stop()
        {
            if (playing == null) return;
            if (playing.added) playing.SetMotorSpeeds(0, 0);
            playing = null;
            left = 0;
        }
    }
}
