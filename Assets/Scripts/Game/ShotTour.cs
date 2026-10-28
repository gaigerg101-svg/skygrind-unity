// NIGHT15 (rule 7: verify by looking): the Player's capture tour, `SKYGRIND.exe -skygrindShots <dir> [-skygrindMap <id>]`.
// The Editor's Play checks need its focus (it stalls unfocused while GG uses the machine), so the night's captures come
// from a Player: TITLE (the MAP row), SETTINGS' GAMEPAD tab, and on a Unity-authored map its views (the plan from above,
// the hub, each built zone's signature place, the landmark from far away). Each capture is written at the end of a
// frame (ScreenCapture), never in the frame a screen changes (the memory note), and logged as `[shots] <file>`. The run's
// player is in god mode; the views are fixed poses (a respawn and a look), so the shots are the same every time.
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class ShotTour
    {
        /// <summary>One pose: where the eye stands (feet, web coordinates), where it looks (yaw / pitch, degrees), the file.</summary>
        struct Pose
        {
            public string file;
            public Vec3 at;
            public double yawDeg, pitchDeg;
        }

        /// <summary>DRYDOCK's views (MAP-DESIGN.md section 4; the plan's coordinates).</summary>
        static readonly Pose[] Drydock =
        {
            new Pose { file = "n15-drydock-above.png", at = new Vec3(0, 78, 6), yawDeg = 0, pitchDeg = -89 }, // the built heart from above
            // Build 2's tour: at the gangway door the locked gate's sign filled the view; the zones are opened first now and
            // the hub is seen from inside THE RING's south-west corner, facing the hull and the mast.
            new Pose { file = "n15-drydock-hub.png", at = new Vec3(-17, 0, 33), yawDeg = -27, pitchDeg = 24 },
            new Pose { file = "n15-drydock-ribs.png", at = new Vec3(-43.5, 3, 30), yawDeg = 0, pitchDeg = 4 }, // up a rib alley, mid-air as on a ride
            // Build 2's tour saw the deck's surface: now the lip of lane A (z -22), looking down the 40 m ramp.
            new Pose { file = "n15-drydock-slipway.png", at = new Vec3(30, 10, -22.6), yawDeg = 180, pitchDeg = -24 },
            // Build 2's tour stood beside a rib slab, which hid the mast (a finding: inside the rib alleys it is hidden); now
            // the slipway's far corner, 66 m from the mast across the open ramps.
            new Pose { file = "n15-drydock-landmark.png", at = new Vec3(58, 0, 32), yawDeg = 61, pitchDeg = 22 },
        };

        static void Log(List<string> log, string s)
        {
            string l = "[shots] " + s;
            Debug.Log(l);
            log.Add(l);
        }

        static IEnumerator Wait(float seconds)
        {
            float t0 = Time.realtimeSinceStartup;
            int frames = 0;
            while (frames < 3 || Time.realtimeSinceStartup - t0 < seconds)
            {
                frames++;
                yield return null;
            }
        }

        static IEnumerator Shot(string dir, string file, List<string> log)
        {
            yield return null; // never in the frame the screen changed
            string path = Path.Combine(dir, file);
            ScreenCapture.CaptureScreenshot(path);
            yield return null;
            yield return null;
            Log(log, path);
        }

        /// <summary>NIGHT16: `-skygrindShotsUi` (with -skygrindShots): the screens a run passes through instead of the title and
        /// the map views: SETTINGS' ACCESS and LAB tabs (tonight's rows), PAUSED, DOWN (the recap may be playing) and RESULTS.</summary>
        public static bool UiPass;

        static IEnumerator RunUi(SurvivalGame g, string dir, List<string> log)
        {
            g.ToTitle();
            yield return Wait(1.5f);
            g.UiView.OpenSettingsTab(UnityTuning.AccessTabId);
            yield return Wait(1f);
            yield return Shot(dir, "n16-access-tab.png", log);
            g.UiView.OpenSettingsTab(UnityTuning.LabTabId);
            yield return Wait(1f);
            yield return Shot(dir, "n16-lab-tab.png", log);
            g.UiView.CloseSettings();
            yield return Wait(0.5f);
            g.noRecords = true;
            g.StartRun(151515);
            yield return Wait(2f);
            g.Pause();
            yield return Wait(1f);
            yield return Shot(dir, "n16-paused.png", log);
            g.Resume();
            yield return Wait(0.5f);
            Vec3 f = g.Feet;
            g.health.Damage(g.health.max * 2, "husk", new Vec3(f.x, f.y, f.z - 3));
            yield return Wait(0.8f);
            yield return Shot(dir, "n16-down.png", log);
            float t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Results && Time.realtimeSinceStartup - t0 < 15) yield return null;
            yield return Wait(3f); // RESULTS' staged tally lands
            yield return Shot(dir, "n16-results.png", log);
            g.ToTitle();
            Log(log, "done (the screens)");
        }

        /// <summary>
        /// NIGHT17 U3-30.3: the feel reel's screens (after its strips): TITLE, every SETTINGS tab shown, the HUD at rest, PAUSED,
        /// MODS (a draft queued), the HUD in a fight (round 1, the aim bot), DOWN, RESULTS. Files n17-*.png. The reel's process
        /// isn't automation (devMode off), so these are the player's screens.
        /// </summary>
        public static IEnumerator RunReel(SurvivalGame g, string dir, List<string> log)
        {
            Directory.CreateDirectory(dir);
            float settle = UnityTuning.ReelScreenSettle;
            g.ToTitle();
            yield return Wait(2.5f);
            if (FeelStrip.ReelWants("title")) yield return Shot(dir, "n17-title.png", log);
            string[] tabs = FeelStrip.ReelWants("settings") ? g.UiView.ReelSettingsTabs() : FeelStrip.ReelWants("settings1") ? new[] { g.UiView.ReelSettingsTabs()[0] } : new string[0];
            foreach (string tab in tabs)
            {
                if (tab == UnityTuning.LabTabId && !g.devMode) continue; // the LAB tab is devMode's (it would open GAME again)
                g.UiView.OpenSettingsTab(tab);
                yield return Wait(settle);
                yield return Shot(dir, "n17-settings-" + tab + ".png", log);
            }
            g.UiView.CloseSettings();
            yield return Wait(0.5f);
            g.noRecords = true;
            g.StartRun(151515);
            g.health.max = g.health.hp = 1e6;
            yield return Wait(2f);
            if (FeelStrip.ReelWants("rest")) yield return Shot(dir, "n17-hud-rest.png", log);
            g.Pause();
            yield return Wait(settle);
            if (FeelStrip.ReelWants("paused")) yield return Shot(dir, "n17-paused.png", log);
            g.Resume();
            yield return Wait(0.5f);
            g.mods.Queue(1);
            float m0 = Time.realtimeSinceStartup;
            while (!g.DraftOpen && Time.realtimeSinceStartup - m0 < 3) yield return null;
            if (g.DraftOpen)
            {
                yield return Wait(settle);
                if (FeelStrip.ReelWants("mods")) yield return Shot(dir, "n17-mods.png", log);
                g.PickMod(0); // the live pick (it resumes the run); AutopilotInput.TakeDraft refuses while the draft has it paused
                yield return Wait(0.5f);
            }
            else Log(log, "n17-mods.png: SKIPPED (no draft opened in 3 s)");
            // The fight: the break ends now; the aim bot shoots round 1's husks for ReelFightTime s.
            if (g.run.state == SurvivalRun.State.Break) g.run.phaseSteps = 1;
            float f0 = Time.realtimeSinceStartup;
            bool shot = false;
            while (Time.realtimeSinceStartup - f0 < UnityTuning.ReelFightTime + 4f)
            {
                g.health.max = g.health.hp = 1e6;
                FeelStrip.AimBot(g);
                if (!shot && Time.realtimeSinceStartup - f0 >= UnityTuning.ReelFightTime && g.run.state != SurvivalRun.State.Break)
                {
                    shot = true;
                    string path = Path.Combine(dir, "n17-hud-fight.png");
                    if (FeelStrip.ReelWants("fight")) ScreenCapture.CaptureScreenshot(path);
                    Log(log, path);
                }
                yield return null;
                if (shot && Time.realtimeSinceStartup - f0 >= UnityTuning.ReelFightTime + 0.2f) break;
            }
            g.forcedCombat = null;
            if (!shot) Log(log, "n17-hud-fight.png: SKIPPED (no round running)");
            Vec3 f = g.Feet;
            g.health.max = g.health.hp = 100;
            g.health.Damage(g.health.max * 2, "husk", new Vec3(f.x, f.y, f.z - 3));
            yield return Wait(0.8f);
            if (FeelStrip.ReelWants("down")) yield return Shot(dir, "n17-down.png", log);
            float t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Results && Time.realtimeSinceStartup - t0 < 15) yield return null;
            yield return Wait(3f); // RESULTS' tally lands
            if (FeelStrip.ReelWants("results")) yield return Shot(dir, "n17-results.png", log);
            g.ToTitle();
            Log(log, "done (the reel's screens)");
        }

        public static IEnumerator Run(SurvivalGame g, string dir, List<string> log)
        {
            Directory.CreateDirectory(dir);
            Log(log, $"tour on '{g.MapId}' ({g.Maps.Count} map(s) listed), {Screen.width}x{Screen.height}");
            if (UiPass)
            {
                yield return RunUi(g, dir, log);
                yield break;
            }
            g.ToTitle();
            yield return Wait(2.5f);
            yield return Shot(dir, "n15-title.png", log);
            g.UiView.OpenSettingsTab("pad");
            yield return Wait(1f);
            yield return Shot(dir, "n15-gamepad-tab.png", log);
            g.UiView.CloseSettings();
            yield return Wait(0.5f);
            if (g.MapId == RunSetup.DefaultMap)
            {
                Log(log, "done (the arena: no map views)");
                yield break;
            }
            g.noRecords = true;
            g.StartRun(151515);
            // Every zone open (free), so no gate field stands between a pose and what it looks at.
            foreach (ZoneDef z in g.map.zones) g.zones.Open(z.id, free: true);
            yield return Wait(1f);
            foreach (Pose p in Drydock)
            {
                g.health.max = g.health.hp = 1e6;
                g.Respawn(p.at, p.yawDeg);
                g.input.SetLook(p.yawDeg * System.Math.PI / 180, p.pitchDeg * System.Math.PI / 180);
                yield return null;
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, p.file)); // the first frame after the pose: no fall yet
                yield return null;
                yield return null;
                Log(log, Path.Combine(dir, p.file));
                yield return Wait(0.6f);
            }
            g.ToTitle();
            Log(log, "done");
        }
    }
}
