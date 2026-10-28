// NIGHT14 U3-27.7 (agent K): the kiosk and a living TITLE, SurvivalGame's half (the shared types' hooks: BootKiosk,
// FrameKiosk, CameraKiosk). The logic is Kiosk.cs (kiosk.js's port), the hints and the lock UiKiosk.cs.
// - BootKiosk: config.kiosk (idleReset, the orbit) and design.attract (idleSeconds, seed, difficulty) read once; the
//   kiosk on with -skygrindKiosk; the idle attract off under automation (PlayerChecks' flags: the smoke and the bench
//   keep a still TITLE; a test calls AttractStart itself).
// - FrameKiosk (each frame after the devices' poll, before the steps): any input read straight from the Input System
//   (a key, a mouse button, a mouse move past keyboardMouse.js's desk-bump rule, the wheel, a pad's button, trigger or
//   stick past gamepad.activity, a touch); Kiosk.Update says what the frame does: the kiosk's idle reset ('kioskReset',
//   the open screens closed, TITLE), the attract started or stopped; while it plays, the show goes on (below).
// - The attract (main.js :701-740, ported to SURVIVAL: no Duel arena yet): the AUTOPILOT plays a SURVIVAL run behind
//   TITLE (UiFlow shows TITLE; the HUD stays off): AutopilotInput on design.attract.difficulty swapped in with
//   sim.UseInput, the run started on design.attract.seed at HEAT 0 (the player's TITLE choices kept), metaSilent and
//   noLab on (NIGHT14 rule 7: the meta records nothing), god mode (UnityTuning.AttractGodHp, re-pinned when a perk
//   resets it), the gun's camera off, the cursor free and hidden, TITLE's controls locked (UiKiosk). A waiting MODS
//   draft is the autopilot's (AutopilotInput.TakeDraft, before Update would open it and pause); a run that ends (a
//   fall's DOWN, an exfil) starts a fresh one.
// - Any input stops it at once (AttractStop): the player's input back, the run left without judging (ToTitle:
//   sim.ToTitle ends a live run with run.End, never EndRun), the world reset, the camera back on TITLE's path, the cursor
//   free, the flags back; TITLE takes clicks again from the next frame, so the stopping input never presses a button.
// - CameraKiosk: the flythrough (main.js :2337-2343): `angle += frameDt * orbitSpeed`; the target the midpoint of the
//   autopilot and its target (the autopilot alone without one) at lookHeight; the camera orbitRadius from it at
//   orbitHeight. The port's only change: the midpoint follows smoothly (UnityTuning.AttractCamFollowRate), since the
//   target changes as enemies die.
// Per frame: device reads and compares; nothing allocates (the logs are on start / stop only).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>The kiosk's logic (kiosk.js: its idle clock, resets, the attract's start; null before BootKiosk).</summary>
        public Kiosk kiosk;

        /// <summary>Automation (a Play check, the smoke): no attract on a normal TITLE (the kiosk's still runs). On under PlayerChecks' flags.</summary>
        public bool attractSuppressed;

        /// <summary>The attract's autopilot (null while it isn't playing).</summary>
        public AutopilotInput AttractPilot => attractPilot;

        AutopilotInput attractPilot;
        bool attractBroken; // the autopilot can't play (design.autopilot off, design.attract missing, a throw): no attract
        bool attractStarting; // inside AttractRun's StartRun (RunStarted is the attract's own)
        bool attractMetaSilentWas, attractNoLabWas;
        bool attractVmCamOff; // the attract turned the viewmodel's camera off (and turns it back on)
        uint attractSeed;
        string attractDifficulty;
        double attractOrbitSpeed, attractOrbitRadius, attractOrbitHeight, attractLookHeight, attractPadActivity;
        double attractAngle, attractMidX, attractMidZ, attractRunAt;
        bool attractMidSet;
        int attractUnlockFrame = -1;
        static readonly object KioskResetPayload = new object(); // kiosk.js emits 'kioskReset' with {}

        partial void BootKiosk()
        {
            // main.js :2337's numbers and the kiosk's idle reset (config.kiosk), the pad's activity rule (gamepad.activity).
            attractOrbitSpeed = cfg.Num("kiosk.orbitSpeed");
            attractOrbitRadius = cfg.Num("kiosk.orbitRadius");
            attractOrbitHeight = cfg.Num("kiosk.orbitHeight");
            attractLookHeight = cfg.Num("kiosk.lookHeight");
            attractPadActivity = cfg.Num("gamepad.activity");
            double idleSeconds = 0;
            if (cfg.Has("design.attract.idleSeconds") && cfg.Has("design.attract.seed") && cfg.Has("design.attract.difficulty"))
            {
                idleSeconds = cfg.Num("design.attract.idleSeconds");
                attractSeed = (uint)cfg.Num("design.attract.seed");
                attractDifficulty = cfg.Str("design.attract.difficulty");
            }
            else AttractCant("design.attract is missing");
            if (RunLogMath.IsAutomation(Environment.GetCommandLineArgs())) attractSuppressed = true;
            kiosk = new Kiosk(Kiosk.Enabled, cfg.Num("kiosk.idleReset"), idleSeconds);
            sim.RunStarted += AttractRunStarted;
            if (kiosk.enabled) Debug.Log($"[kiosk] on ({Kiosk.Flag}): back to TITLE after {kiosk.idleReset} s without input; the attract on TITLE; no QUIT");
        }

        partial void FrameKiosk(double frameDt)
        {
            if (kiosk == null) return;
            if (attractUnlockFrame >= 0 && Time.frameCount >= attractUnlockFrame)
            {
                attractUnlockFrame = -1;
                ui?.KioskTitleLock(false);
            }
            bool input = KioskAnyInput();
            bool can = !attractBroken && (kiosk.enabled || !attractSuppressed) && !FeelClock.Active && sim.flow == Flow.Title && ui != null && ui.KioskTitleBare; // NIGHT18: never inside a reel scenario
            // NIGHT14 review B-5: in the kiosk a screen left open over TITLE (SETTINGS, CONTROLS, CONTRACTS, WEAPONS) counts
            // as away from TITLE, so the 60 s idle reset closes it (KioskReset) and the attract can start again.
            bool kioskTitle = UiFlow == Flow.Title && (attractOn || !kiosk.enabled || ui == null || ui.KioskTitleBare);
            switch (kiosk.Update(frameDt, input, kioskTitle, attractOn, can))
            {
                case Kiosk.Step.StopAttract:
                    AttractStop();
                    break;
                case Kiosk.Step.Reset:
                    KioskReset();
                    break;
                case Kiosk.Step.StartAttract:
                    AttractStart();
                    break;
            }
            if (attractOn) AttractKeep();
        }

        partial void CameraKiosk(double frameDt, ref bool handled)
        {
            if (!attractOn || attractPilot == null || cam == null) return;
            // main.js :2337-2343: "The flythrough: a slow orbit around the two fighters, a little above them."
            attractAngle += frameDt * attractOrbitSpeed;
            Vec3 me = previous + (current - previous) * Alpha; // the autopilot as drawn
            double mx = me.x, mz = me.z;
            if (attractPilot.HasTarget)
            {
                Vec3 t = attractPilot.TargetPosition;
                mx = (me.x + t.x) / 2;
                mz = (me.z + t.z) / 2;
            }
            if (!attractMidSet)
            {
                attractMidSet = true;
                attractMidX = mx;
                attractMidZ = mz;
            }
            else
            {
                double k = 1 - Math.Exp(-UnityTuning.AttractCamFollowRate * frameDt);
                attractMidX += (mx - attractMidX) * k;
                attractMidZ += (mz - attractMidZ) * k;
            }
            Vector3 at = WebSpace.ToUnity(new Vec3(attractMidX + Math.Cos(attractAngle) * attractOrbitRadius, attractOrbitHeight, attractMidZ + Math.Sin(attractAngle) * attractOrbitRadius));
            Vector3 look = WebSpace.ToUnity(new Vec3(attractMidX, attractLookHeight, attractMidZ));
            cam.transform.SetPositionAndRotation(at, Quaternion.LookRotation(look - at, Vector3.up));
            handled = true;
        }

        /// <summary>kiosk.js debugIdle: pretend nobody touched anything for `s` seconds (automation).</summary>
        public void KioskDebugIdle(double s) => kiosk?.DebugIdle(s);

        /// <summary>
        /// Start the attract loop now (the kiosk's TITLE, the idle clock, or automation): only from TITLE, once the
        /// autopilot can play. False if it didn't start.
        /// </summary>
        public bool AttractStart()
        {
            if (attractOn || attractBroken || sim.flow != Flow.Title) return false;
            if (!AttractRun()) return false;
            ui?.KioskTitleLock(true); // the input that stops it must not press a TITLE button
            attractUnlockFrame = -1;
            Camera gun = viewmodel?.Camera;
            if (gun != null && gun.enabled)
            {
                gun.enabled = false; // the viewmodel stays off over the flythrough
                attractVmCamOff = true;
            }
            attractMidSet = false;
            Debug.Log($"[kiosk] attract on: the AUTOPILOT ({attractDifficulty}) on seed {attractSeed}, metaSilent");
            return true;
        }

        /// <summary>
        /// Any input: the attract stops and TITLE stays (the run left without judging, the player's input and the camera
        /// back; TITLE takes clicks from the next frame). Nothing while it isn't playing.
        /// </summary>
        public void AttractStop()
        {
            if (!attractOn) return;
            sim.UseInput(playerInput); // first: ToTitle's respawn sets the player's look
            ToTitle(); // sim.ToTitle: a live run ends with run.End (no EndRun: never judged), the world reset; the cursor free
            AttractRelease();
            Debug.Log("[kiosk] attract off: an input");
        }

        /// <summary>For the lead's StartRun / ResumeSave (a run started by anything else): stop the attract first.</summary>
        public void AttractYield()
        {
            if (attractOn && !attractStarting) AttractStop();
        }

        /// <summary>The attract's state let go (its run left as it is): the input, the flags, the gun's camera, TITLE's lock.</summary>
        void AttractRelease()
        {
            attractOn = false;
            AttractDropPilot(null);
            if (sim.input != playerInput) sim.UseInput(playerInput);
            sim.metaSilent = attractMetaSilentWas;
            sim.noLab = attractNoLabWas;
            if (attractVmCamOff)
            {
                attractVmCamOff = false;
                Camera gun = viewmodel?.Camera;
                if (gun != null) gun.enabled = true;
            }
            if (!Cursor.visible && !input.Locked) Cursor.visible = true;
            attractUnlockFrame = Time.frameCount + 1;
        }

        /// <summary>
        /// One attract run (a start, or a fresh one after the last ended): a new autopilot swapped in, the run started on
        /// design.attract.seed at HEAT 0 (the TITLE's mode / HEAT / seed choices put back after), the meta silent, no LAB,
        /// god mode, the cursor free and hidden. False (and the attract off for the session) if the autopilot can't play.
        /// </summary>
        bool AttractRun()
        {
            AutopilotInput pilot;
            // NIGHT14 integration (the review's B-1 / B-2): the player's run rules are saved before any pilot touches them
            // (AutopilotInput's constructor sets metaSilent, noLab and the aim assist off: saved after it, they were the
            // bot's, and every later run of the session recorded nothing), then the last pilot lets go (Detach puts back
            // what it found) before the next is built (a pilot built over an attached one saves the bot's rules).
            if (!attractOn)
            {
                attractMetaSilentWas = sim.metaSilent;
                attractNoLabWas = sim.noLab;
            }
            attractPilot?.Detach();
            try
            {
                if (!AutopilotInput.Enabled(cfg)) return AttractCant("design.autopilot.enabled is false");
                pilot = new AutopilotInput(sim, attractDifficulty, attractSeed);
            }
            catch (Exception e)
            {
                return AttractCant($"the AUTOPILOT can't play ({e.GetType().Name}: {e.Message})");
            }
            sim.metaSilent = true; // NIGHT14 rule 7: no profile, records, contracts, mastery, DAILY or run log
            sim.noLab = true; // the web's rules, whatever the LAB tab says
            attractOn = true;
            AttractDropPilot(pilot);
            RunSetup su = setup;
            string mode = su.mode;
            int heat = su.heat;
            uint? seed = su.seed;
            su.mode = RunSetup.Survival;
            su.heat = 0;
            su.seed = null;
            attractStarting = true;
            try
            {
                sim.UseInput(pilot); // the look carried over
                StartRun(attractSeed);
            }
            catch (Exception e)
            {
                attractStarting = false;
                su.mode = mode;
                su.heat = heat;
                su.seed = seed;
                AttractCant($"the attract run didn't start ({e.GetType().Name}: {e.Message})");
                AttractStop();
                return false;
            }
            attractStarting = false;
            su.mode = mode;
            su.heat = heat;
            su.seed = seed;
            FreeCursor(); // StartRun locked it; TITLE's buttons answer after the stopping input
            Cursor.visible = false; // no arrow parked over the show (any move stops it and shows the cursor)
            health.max = health.hp = UnityTuning.AttractGodHp; // main.js :729 player.setGodMode(true)
            attractRunAt = FeelClock.Now; // NIGHT18: FeelClock
            return true;
        }

        /// <summary>The autopilot in use becomes `next`; the one dropped is detached (its run rules back, its RunStarted handler off).</summary>
        void AttractDropPilot(AutopilotInput next)
        {
            AutopilotInput old = attractPilot;
            attractPilot = next;
            if (old != null && !ReferenceEquals(old, next)) old.Detach(); // NIGHT14 integration: AutopilotInput has Detach, not IDisposable
        }

        bool AttractCant(string why)
        {
            attractBroken = true;
            Debug.LogWarning($"[kiosk] no attract loop this session: {why}");
            return false;
        }

        /// <summary>While the attract plays (after this frame's input): the show goes on.</summary>
        void AttractKeep()
        {
            Flow f = sim.flow;
            if (f == Flow.Title)
            {
                AttractRelease(); // something took the game to TITLE under it
                return;
            }
            if (f != Flow.Playing)
            {
                // DOWN (a fall: god mode can't stop the kill plane's full-HP blow), RESULTS (an exfil), PAUSED (a draft the
                // autopilot didn't take): a fresh attract run, unless the last one ended at once (then it stops).
                if (FeelClock.Now - attractRunAt < UnityTuning.AttractMinRunSeconds) // NIGHT18: FeelClock
                {
                    AttractCant($"an attract run ended within {UnityTuning.AttractMinRunSeconds} s ({f})");
                    AttractStop();
                    return;
                }
                if (AttractRun()) attractMidSet = false;
                else if (attractOn) AttractStop(); // the autopilot can't play any more: TITLE, once
                return;
            }
            // A waiting MODS draft is the autopilot's, before Update opens it and pauses (mods.js: a break's draft).
            if (run.state == SurvivalRun.State.Break && mods.Pending)
            {
                try
                {
                    AutopilotInput.TakeDraft(sim);
                }
                catch (Exception e)
                {
                    AttractCant($"the AUTOPILOT can't take the MODS draft ({e.GetType().Name}: {e.Message})");
                    AttractStop();
                    return;
                }
            }
            // God mode: a perk's apply (PLATE, VIGOR) resets health.max; the show's HP goes back up.
            Health h = health;
            if (h.max != UnityTuning.AttractGodHp || h.hp < UnityTuning.AttractGodHp) h.max = h.hp = UnityTuning.AttractGodHp;
        }

        /// <summary>A run started that isn't the attract's (automation calling StartRun under it): the attract lets go.</summary>
        void AttractRunStarted()
        {
            if (!attractOn || attractStarting) return;
            Debug.LogWarning("[kiosk] a run started under the attract loop: the attract let go (that run started with metaSilent and noLab on)");
            AttractRelease();
        }

        /// <summary>kiosk.js update's reset: 'kioskReset', then the title (enterTitle shows the title menu alone).</summary>
        void KioskReset()
        {
            events.Emit("kioskReset", KioskResetPayload);
            if (ui != null)
            {
                if (ui.SettingsOpen) ui.CloseSettings();
                if (ui.ControlsOpen) ui.CloseControls();
                if (ui.MetaOpen) ui.CloseMeta();
            }
            ToTitle();
            Debug.Log($"[kiosk] {kiosk.idleReset} s without input: back to TITLE (reset {kiosk.resets})");
        }

        /// <summary>
        /// Any input this frame, read from the Input System (kiosk.js pokes on keydown, mousedown, mousemove, wheel,
        /// touchstart): a key or mouse button held, a mouse move past keyboardMouse.js's desk-bump rule
        /// (GamepadMath.MouseDeviceMove), the wheel, any pad's button, trigger (GamepadMath.TriggerDown) or stick
        /// (gamepad.activity, gamepad.js padActivity), a touch.
        /// </summary>
        bool KioskAnyInput()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.anyKey.isPressed) return true;
            Mouse m = Mouse.current;
            if (m != null)
            {
                if (m.leftButton.isPressed || m.rightButton.isPressed || m.middleButton.isPressed || m.forwardButton.isPressed || m.backButton.isPressed) return true;
                Vector2 d = m.delta.ReadValue();
                if (Math.Abs(d.x) + Math.Abs(d.y) > GamepadMath.MouseDeviceMove) return true;
                Vector2 s = m.scroll.ReadValue();
                if (s.x != 0 || s.y != 0) return true;
            }
            var pads = Gamepad.all;
            for (int i = 0; i < pads.Count; i++)
                if (KioskPadUsed(pads[i])) return true;
            Touchscreen ts = Touchscreen.current;
            return ts != null && ts.primaryTouch.press.isPressed;
        }

        bool KioskPadUsed(Gamepad g)
        {
            if (g.buttonSouth.isPressed || g.buttonEast.isPressed || g.buttonWest.isPressed || g.buttonNorth.isPressed
                || g.leftShoulder.isPressed || g.rightShoulder.isPressed || g.startButton.isPressed || g.selectButton.isPressed
                || g.leftStickButton.isPressed || g.rightStickButton.isPressed
                || g.dpad.up.isPressed || g.dpad.down.isPressed || g.dpad.left.isPressed || g.dpad.right.isPressed) return true;
            if (g.leftTrigger.ReadValue() > GamepadMath.TriggerDown || g.rightTrigger.ReadValue() > GamepadMath.TriggerDown) return true;
            Vector2 l = g.leftStick.ReadValue(), r = g.rightStick.ReadValue();
            double a = attractPadActivity;
            return Math.Abs(l.x) > a || Math.Abs(l.y) > a || Math.Abs(r.x) > a || Math.Abs(r.y) > a;
        }
    }
}
