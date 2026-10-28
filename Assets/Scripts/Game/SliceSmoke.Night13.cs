// NIGHT13 smoke steps (after NIGHT12's): each item's proof in the built Player and the Editor smoke.
// 12. U3-26.12: the local run log: a run writes its JSONL file (a header first, the run's events, a footer last).
// 13. U3-26.10: SAVE & QUIT at a break after round 1, TITLE, RESUME: the round, the points and the owned guns equal
//     (through a temp file: the player's slot is never touched).
// 14. U3-26.9: the radial opened by scripted input (WeaponSelect, as the Tab key drives it): the HUD shows it, the pointer
//     highlights a slot, the release asks for it and the loadout takes it.
// 15. U3-26.8: a rebind of jump read back after reloading the store (restored after).
// 16. U3-26.7 / U3-26.8: a virtual pad (InputSystem.AddDevice<Gamepad>, state events): TITLE -> PLAY with A, the stick
//     moves and turns, the trigger fires and kills, START pauses, B resumes; then on TITLE the d-pad reaches SETTINGS, A
//     opens it, right moves a slider, B backs out (the value restored after). The keyboard steps before are unchanged.
using System.Collections;
using System.IO;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public sealed partial class SliceSmoke
    {
        IEnumerator Night13(SurvivalGame g)
        {
            // 12. The run log (RunLog: automation writes only into automationFolder, so the player's runs folder is untouched).
            RunLog rl = RunLog.Instance;
            if (rl == null) Check(false, "run log: no RunLog attached");
            else
            {
                string folder = Path.Combine(Application.temporaryCachePath, "runlog-smoke");
                rl.automationFolder = folder;
                g.StartRun(616161);
                g.health.max = g.health.hp = 1e6;
                float t0 = Time.realtimeSinceStartup;
                while (g.run.round < 1 && Time.realtimeSinceStartup - t0 < 30) yield return null; // the first break, then round 1 begins
                yield return null;
                g.ToTitle();
                yield return null;
                rl.FlushNow();
                string path = rl.LastPath;
                string[] lines = path != null && File.Exists(path) ? File.ReadAllLines(path) : new string[0];
                bool ok = lines.Length >= 3 && lines[0].Contains("\"e\":\"header\"") && lines[lines.Length - 1].Contains("\"e\":\"footer\"");
                Check(ok, $"run log: {(path == null ? "no file" : Path.GetFileName(path))}, {lines.Length} lines (header first: {lines.Length > 0 && lines[0].Contains("\"e\":\"header\"")}, footer last: {lines.Length > 0 && lines[lines.Length - 1].Contains("\"e\":\"footer\"")})");
                rl.automationFolder = null;
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (System.Exception)
                {
                    // a leftover temp folder is harmless
                }
            }
            yield return SaveResume(g);
            yield return Radial(g);
            Rebind(g);
            yield return VirtualPad(g);
        }

        /// <summary>14. SAVE & QUIT at round 1's clear, TITLE, RESUME (a temp save file).</summary>
        IEnumerator SaveResume(SurvivalGame g)
        {
            string file = Path.Combine(Application.temporaryCachePath, "smoke-run.json");
            if (File.Exists(file)) File.Delete(file);
            g.UseSavePath(file);
            g.StartRun(727272);
            g.health.max = g.health.hp = 1e6;
            float t0 = Time.realtimeSinceStartup;
            while (!(g.run.state == SurvivalRun.State.Break && g.run.cleared >= 1) && Time.realtimeSinceStartup - t0 < 120)
            {
                if (g.run.state == SurvivalRun.State.Fight) KillAura(g);
                g.health.hp = g.health.max;
                yield return null;
            }
            g.Pause();
            yield return null;
            bool allowed = g.SaveAllowed;
            int round = g.run.round;
            double points = g.economy.total;
            string guns = string.Join(",", g.loadout.owned);
            g.SaveAndQuit();
            yield return null;
            bool saved = g.flow == Flow.Title && g.HasSave && File.Exists(file);
            g.ResumeSave();
            yield return null;
            bool equal = g.run.round == round && g.economy.total == points && string.Join(",", g.loadout.owned) == guns;
            Check(allowed && saved && equal && !File.Exists(file), $"save and resume: allowed at round {round}'s break {allowed}, saved to TITLE {saved}, resumed round {g.run.round} / points {g.economy.total} / guns {string.Join(",", g.loadout.owned)} (equal {equal}), file consumed {!File.Exists(file)}");
            g.Resume();
            g.ToTitle();
            g.UseSavePath(null);
            yield return null;
        }

        /// <summary>15. The radial, opened and released by scripted input (WeaponSelect: the Tab key's own path).</summary>
        IEnumerator Radial(SurvivalGame g)
        {
            g.StartRun(737373);
            g.health.max = g.health.hp = 1e6;
            yield return null;
            int n = g.loadout.owned.Count;
            CombatReader cr = g.combatReader;
            bool shown = false;
            int hi = -1;
            // Hold the radial open for a few frames, pointing down (slot 1 of 2 sits opposite slot 0, which is up).
            for (int f = 0; f < 6; f++)
            {
                if (!cr.select.radial.open) cr.select.OpenRadial(n);
                cr.select.MoveRadial(0, 40, n);
                cr.radial = cr.select.radial;
                yield return null;
                shown |= g.UiView.RadialShown;
                if (g.UiView.RadialHighlight >= 0) hi = g.UiView.RadialHighlight;
            }
            cr.select.CloseRadial(true, n);
            cr.radial = cr.select.radial;
            int want = cr.select.radial.highlighted >= 0 ? cr.select.radial.highlighted : hi;
            float t0 = Time.realtimeSinceStartup;
            while (g.loadout.targetSlot != 1 && Time.realtimeSinceStartup - t0 < 3) yield return null;
            Check(n >= 2 && shown && hi == 1 && g.loadout.targetSlot == 1, $"radial: {n} guns, shown on the HUD {shown}, highlighted {hi}, the loadout's slot {g.loadout.targetSlot} (cursor captured {g.input.Locked})");
            g.ToTitle();
            yield return null;
        }

        /// <summary>16. Rebind jump, reload the store, read it back (then the old key again).</summary>
        void Rebind(SurvivalGame g)
        {
            string old = g.bindings.Code("jump");
            g.bindings.Set("jump", "KeyJ");
            Bindings reloaded = InputPrefs.LoadBindings(g.cfg);
            string back = reloaded.Code("jump");
            g.bindings.Set("jump", old);
            string restored = InputPrefs.LoadBindings(g.cfg).Code("jump");
            Check(back == "KeyJ" && restored == old, $"rebind: jump -> {back} after reloading the store; restored to {restored}");
        }

        /// <summary>17. A virtual pad: PLAY with A, move, look, fire and kill, START pauses, B resumes; SETTINGS by the d-pad.</summary>
        IEnumerator VirtualPad(SurvivalGame g)
        {
            DisableRealPads(); // NIGHT24: see SliceSmoke.DisableRealPads
            Gamepad pad = InputSystem.AddDevice<Gamepad>("Smoke pad");
            var state = new GamepadState();
            void Send() => InputSystem.QueueStateEvent(pad, state);
            IEnumerator Press(GamepadButton b)
            {
                state = state.WithButton(b, true);
                Send();
                yield return null;
                yield return null;
                state = state.WithButton(b, false);
                Send();
                yield return null;
                yield return null;
            }
            g.ToTitle();
            ClearMenuSelection(); // NIGHT24
            for (int f = 0; f < 10; f++) yield return null; // TITLE settles; the nav selects PLAY
            GameObject sel = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            string selName = sel != null ? sel.name : "none";
            yield return Press(GamepadButton.South);
            for (int f = 0; f < 5 && g.flow != Flow.Playing; f++) yield return null;
            bool played = g.flow == Flow.Playing;
            if (!played) g.StartRun(747474); // the rest of the step still runs
            g.health.max = g.health.hp = 1e6;
            yield return null;
            Vec3 at = g.mover.position;
            double yaw0 = g.input.yaw;
            state.leftStick = new Vector2(0, 1);
            state.rightStick = new Vector2(1, 0);
            Send();
            float tw = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - tw < 0.6f) yield return null; // 0.6 s of stick (frames are ~2.5 ms in a Player)
            state.leftStick = Vector2.zero;
            state.rightStick = Vector2.zero;
            Send();
            yield return null;
            Vec3 moved = g.mover.position;
            bool walked = Vec3.DistanceTo(at, moved) > 0.5;
            bool turned = g.input.yaw != yaw0;
            // Fire on the trigger at the round's husks (the look aimed for it: the stick's turn is checked above).
            g.run.DebugSkipTo(1);
            int kills0 = g.run.stats.kills;
            float t0 = Time.realtimeSinceStartup;
            state.rightTrigger = 1;
            Send();
            while (g.run.stats.kills == kills0 && Time.realtimeSinceStartup - t0 < 30)
            {
                AimAtNearest(g);
                g.health.hp = g.health.max;
                yield return null;
            }
            state.rightTrigger = 0;
            Send();
            yield return null;
            bool killed = g.run.stats.kills > kills0;
            yield return Press(GamepadButton.Start);
            bool paused = g.flow == Flow.Paused;
            for (int f = 0; f < 3; f++) yield return null;
            yield return Press(GamepadButton.East);
            for (int f = 0; f < 3 && g.flow != Flow.Playing; f++) yield return null;
            bool resumed = g.flow == Flow.Playing;
            Check(played && walked && turned && killed && paused && resumed, $"virtual pad: TITLE selected '{selName}', A played {played}, the stick moved {Vec3.DistanceTo(at, moved):0.0} m and turned {turned}, the trigger killed {killed}, START paused {paused}, B resumed {resumed}");
            // On TITLE: the d-pad down to SETTINGS, A opens it, right moves the selected slider, B backs out.
            g.ToTitle();
            for (int f = 0; f < 10; f++) yield return null;
            string settingsSnap = g.settings.store.Save();
            for (int k = 0; k < 8 && !(EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.name == "SETTINGS"); k++)
                yield return Press(GamepadButton.DpadDown);
            string at2 = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : "none";
            yield return Press(GamepadButton.South);
            for (int f = 0; f < 3; f++) yield return null;
            bool opened = g.UiView.SettingsOpen;
            yield return Press(GamepadButton.DpadDown); // from the selected tab onto the first row (right on a tab switches the tab)
            string row = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.name : "none";
            int v0 = g.settings.version;
            string before = g.settings.store.Save();
            yield return Press(GamepadButton.DpadRight);
            for (int f = 0; f < 3; f++) yield return null;
            bool changed = g.settings.store.Save() != before;
            yield return Press(GamepadButton.East);
            for (int f = 0; f < 3; f++) yield return null;
            bool closed = !g.UiView.SettingsOpen;
            Check(opened && changed && closed, $"virtual pad menus: the d-pad reached '{at2}', A opened SETTINGS {opened}, down reached '{row}', right moved a value {changed} (settings version {v0} -> {g.settings.version}), B backed out {closed}");
            g.settings.store.Load(settingsSnap); // the player's settings as they were (saved below)
            g.settings.Save();
            g.settings.Apply();
            InputSystem.RemoveDevice(pad);
            RestoreRealPads(); // NIGHT24
            yield return null;
        }

        /// <summary>The look onto the nearest live enemy's aim point (the smoke's aim bot).</summary>
        static void AimAtNearest(SurvivalGame g)
        {
            Vec3 eye = g.Eye;
            Enemy near = null;
            double best = double.PositiveInfinity;
            foreach (Enemy e in g.enemies.all)
            {
                if (!e.alive) continue;
                double d = Vec3.DistanceTo(e.AimPoint, eye);
                if (d < best)
                {
                    best = d;
                    near = e;
                }
            }
            if (near == null) return;
            Vec3 to = near.AimPoint - eye;
            g.input.SetLook(System.Math.Atan2(-to.x, -to.z), System.Math.Atan2(to.y, System.Math.Sqrt(to.x * to.x + to.z * to.z)));
        }
    }
}
