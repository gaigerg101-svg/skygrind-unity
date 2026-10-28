// NIGHT22 U3-36.6 smoke step (builder W; UNCOMPILED, written without the Editor): THE GRIND from TITLE with the pad only.
// The lead calls `yield return Grind22(g);` LAST in the night's step list (after Night15), because it changes the run setup's
// mode; it puts the mode back to SURVIVAL at its end.
// A virtual pad (as SliceSmoke.Night15Pad.cs): TITLE -> the d-pad reaches the MODE row -> A on it cycles MODE until THE GRIND is
// picked -> the d-pad reaches PLAY -> A starts the run -> wave 1's break (the WAVE banner event), the telegraph's marks seen in
// its last seconds, the reshape, the fight; the kill aura clears the wave (god mode) -> wave 2 is reached. The pad is removed
// and TITLE is back at the end. Every wait is in real time. Lines start "grind22:".
// RED (before the feature, or with design.n21.grind off): the step prints FAIL on its first Check.
using System.Collections;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public sealed partial class SliceSmoke
    {
        IEnumerator Grind22(SurvivalGame g)
        {
            bool grindOn = !N21Blocks.ForceOff && N21Blocks.From(g.cfg).grind;
            if (!grindOn)
            {
                Check(false, "grind22: design.n21.grind is off (--off21): THE GRIND is not on the MODE row");
                yield break;
            }
            DisableRealPads(); // NIGHT24: see SliceSmoke.DisableRealPads
            Gamepad pad = InputSystem.AddDevice<Gamepad>("Smoke grind pad");
            var state = new GamepadState();
            IEnumerator Press(GamepadButton b)
            {
                state = state.WithButton(b, true);
                InputSystem.QueueStateEvent(pad, state);
                yield return PadOnlyWait(PadOnlyPressHold);
                state = state.WithButton(b, false);
                InputSystem.QueueStateEvent(pad, state);
                yield return PadOnlyWait(PadOnlyPressHold);
            }
            // The d-pad toward `name` (down first, then up) in at most `max` presses; true: it is selected.
            IEnumerator Reach(string name, System.Action<bool> done, int max = 8)
            {
                for (int k = 0; k < max && PadOnlySelected() != name; k++) yield return Press(GamepadButton.DpadDown);
                for (int k = 0; k < 2 * max && PadOnlySelected() != name; k++) yield return Press(GamepadButton.DpadUp);
                done(PadOnlySelected() == name);
            }

            g.setup.mode = RunSetup.Survival;
            g.ToTitle();
            ClearMenuSelection(); // NIGHT24
            yield return PadOnlyWait(0.4f); // TITLE settles; the nav selects PLAY and the run setup card shows
            bool onMode = false;
            yield return Reach("MODE", r => onMode = r);
            int presses = 0;
            for (; onMode && presses < 5 && g.setup.mode != RunSetup.Grind; presses++) yield return Press(GamepadButton.South); // A on the MODE row cycles it
            bool picked = g.setup.mode == RunSetup.Grind;
            Check(onMode && picked, $"grind22: the d-pad reached the MODE row {onMode}; A on it {presses}x picked '{g.setup.mode}' (want '{RunSetup.Grind}')");
            if (!picked)
            {
                yield return Grind22End(g, pad);
                yield break;
            }
            // NIGHT24: the listeners go on BEFORE A starts the run (wave 1's waveBreak and banner fire inside StartRun; the step attached them after and always read 0x)
            int waveBreaks = 0, banners = 0, roundBegins = 0;
            System.Action off1 = g.events.On("waveBreak", _ => waveBreaks++);
            System.Action off2 = g.events.On<BannerEvent>("banner", b => { if (b.text != null && b.text.StartsWith("WAVE")) banners++; });
            System.Action off3 = g.events.On<RoundEvent>("roundBegin", _ => roundBegins++);
            bool onPlay = false;
            yield return Reach("PLAY", r => onPlay = r);
            if (onPlay) yield return Press(GamepadButton.South);
            float t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Playing && Time.realtimeSinceStartup - t0 < 3) yield return null;
            Check(onPlay && g.flow == Flow.Playing && g.sim.grindOn, $"grind22: the d-pad reached PLAY {onPlay}; A started a run: {g.flow}, GRIND on {g.sim.grindOn}");
            if (g.flow != Flow.Playing || !g.sim.grindOn)
            {
                yield return Grind22End(g, pad);
                yield break;
            }
            g.health.max = g.health.hp = 1e6; // the smoke's god mode (not an input)

            // Wave 1: the WAVE banner (waveBreak), the telegraph in the break's last seconds, the reshape, the fight.
            bool telegraphSeen = false, markedAny = false, fightSeen = false;
            string preset = "-";
            int rising = 0;
            t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 30)
            {
                g.health.hp = g.health.max;
                var ph = g.sim.grind != null ? g.sim.grind.Phase : Endless21.State.Idle;
                if (g.sim.GrindTelegraphing && !telegraphSeen)
                {
                    telegraphSeen = true;
                    preset = g.sim.GrindNextPreset;
                    if (g.sim.GrindRising != null) foreach (bool b in g.sim.GrindRising) if (b) rising++;
                    markedAny = rising > 0;
                }
                if (ph == Endless21.State.Fight) { fightSeen = true; break; }
                yield return null;
            }
            Check(waveBreaks >= 1 && banners >= 1 && telegraphSeen && markedAny && fightSeen,
                $"grind22: wave 1: waveBreak {waveBreaks}x, WAVE banner {banners}x; the telegraph came on for the '{preset}' shape ({rising} pillars marked rising); the fight began {fightSeen} ({Time.realtimeSinceStartup - t0:0.0} s)");

            // Clear wave 1 (the kill aura, god mode): wave 2 is reached.
            t0 = Time.realtimeSinceStartup;
            while (g.sim.grind != null && g.sim.grind.CurrentWave < 2 && Time.realtimeSinceStartup - t0 < 60)
            {
                g.health.hp = g.health.max;
                KillAura(g);
                yield return null;
            }
            int wave = g.sim.grind != null ? g.sim.grind.CurrentWave : 0;
            Check(wave >= 2 && g.flow == Flow.Playing, $"grind22: wave {wave} reached ({g.sim.grind?.Cleared} cleared, score {g.sim.GrindScore:0}) in {Time.realtimeSinceStartup - t0:0.0} s; roundBegin events from SurvivalRun {roundBegins} (the Ui draws no ROUND banner in the GRIND: read on a frame)");
            off1(); off2(); off3();
            yield return Grind22End(g, pad);
        }

        /// <summary>The pad let go and removed, the mode back to SURVIVAL, back to TITLE.</summary>
        IEnumerator Grind22End(SurvivalGame g, Gamepad pad)
        {
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return PadOnlyWait(0.05f);
            InputSystem.RemoveDevice(pad);
            RestoreRealPads(); // NIGHT24
            g.setup.mode = RunSetup.Survival;
            g.ToTitle();
            yield return null;
        }
    }
}
