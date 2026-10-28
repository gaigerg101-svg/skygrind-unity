// NIGHT18 (NIGHT18.md section 1.2, the lead's): FeelStrip v2, the measured reel. `SKYGRIND.exe -skygrindReel <abs dir>
// [-skygrindReelOnly a,b] [-skygrindFeelAB off]` (PlayerChecks) or the Editor's guarded eval runs each scenario:
// - set up off the record (a fresh run, the same seed; the target on open floor at exactly 6 / 12 / 20 m, found by casting
//   the collider along the clearest headings; held still with husks.frozen; the same placement on both FeelAB sides);
// - then record: FeelClock fixes every frame at 1/60 s of game time (the 120 Hz sim steps 2 per frame however slow the
//   frame), FeelProbe logs the frame's telemetry, and every rendered frame is captured at its end and written as a JPG
//   (q90; encoded on worker threads, the main thread waits when 6 are queued, so no frame is ever skipped);
// - 300 ms (18 frames) of pre-roll, the input (FeelProbe.MarkInput), the event, a post-roll (1.2 s after a kill);
// - then telemetry.jsonl (FeelProbe.End) and the next scenario.
// The quality preset is pinned (AUTO held: Quality.cs) and the attract can't start (Kiosk) while a scenario records.
// Phase A (build 1): hit12, kill12, kill20, headshot12, horde5, boost, sprint, slide, rankup, roundend, ui-title,
// ui-settings. Phase B (build 2): kill6, punchkill, boostside, land, strafe, swapall, pickup, hud-fight, ui-paused,
// ui-results, ui-mods. Tools/feel-schema.md is the output format; Tools/feel-metrics.py reads it.
// NIGHT19 (the lead): phase B adds gust (the KESTREL's LT from a standstill, aimed level, then down), vitals (a sprint that
// empties the bar, then a kill within 6 m that refills it) and ui-weapons (WEAPONS entering, the focus moved twice); phase C
// (both passes) the prompt scenarios: prompt-buy (a gate's buy prompt), prompt-box, prompt-forge, ui-down (DOWN). The pad
// pass (`-skygrindReelPad`, PadPass) holds PlayerDevices.Last on the pad for the whole reel (real pads stay disabled: an
// override, not a device), so every prompt and screen shows what a pad player sees (S12).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Experimental.Rendering;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseA = { "hit12", "kill12", "kill20", "headshot12", "horde5", "boost", "sprint", "slide", "rankup", "roundend", "ui-title", "ui-settings" };
        public static readonly string[] PhaseB = { "kill6", "punchkill", "boostside", "land", "strafe", "swapall", "pickup", "hud-fight", "ui-paused", "ui-results", "ui-mods", "gust", "vitals", "ui-weapons" };

        /// <summary>NIGHT19: the prompt scenarios (both passes: S12 reads the keyboard pass for pad words and the pad pass for keys).</summary>
        public static readonly string[] PhaseC = { "prompt-buy", "prompt-box", "prompt-forge", "ui-down" };

        /// <summary>NIGHT19 (-skygrindReelPad): the pad pass: PlayerDevices.Last held on the pad for the whole reel.</summary>
        public static bool PadPass;

        /// <summary>The scenarios to run (-skygrindReelOnly; null: phase A then phase B).</summary>
        public static string[] Only;

        /// <summary>The reel is running (any scenario, recorded or being set up).</summary>
        public static bool Running { get; private set; }

        const uint Seed = 160716;
        static int pending; // JPGs queued on worker threads
        static int written, failed;

        static void Log(List<string> log, string s)
        {
            log?.Add(s);
            Debug.Log("[reel] " + s);
        }

        /// <summary>The whole reel into `dir` (absolute): reel.json, then one folder per scenario.</summary>
        public static IEnumerator Run(SurvivalGame g, string dir, List<string> log)
        {
            Running = true;
            Directory.CreateDirectory(dir);
            var list = new List<string>();
            foreach (string s in Only ?? PhaseA) list.Add(s);
            if (Only == null)
            {
                list.AddRange(PhaseB);
                list.AddRange(PhaseC);
                list.AddRange(PhaseD); // NIGHT20 (FeelReel.N20.cs)
                list.AddRange(PhaseE); // NIGHT21 (FeelReel.N21.cs)
                list.AddRange(PhaseF); // NIGHT22 (FeelReel.N22.cs)
            }
            if (PadPass) PlayerDevices.Last = InputDevice.Pad; // NIGHT19: the pad pass (re-held before every scenario)
            bool attract = g.attractSuppressed;
            g.attractSuppressed = true; // no attract on TITLE between scenarios either
            CultureInfo ci = CultureInfo.InvariantCulture;
            File.WriteAllText(Path.Combine(dir, "reel.json"),
                "{\"build\":\"" + BuildStamp.Line.Replace("\"", "'") + "\",\"feelAB\":" + (FeelAB.On ? "true" : "false") + ",\"pad\":" + (PadPass ? "true" : "false") + ",\"map\":\"" + g.MapId + "\",\"w\":" + Screen.width + ",\"h\":" + Screen.height +
                ",\"fps\":" + UnityTuning.ReelFps + ",\"simHz\":120,\"scenarios\":[\"" + string.Join("\",\"", list) + "\"],\"started\":\"" + DateTime.Now.ToString("s", ci) + "\"}\n");
            Log(log, $"reel on '{g.MapId}', {Screen.width}x{Screen.height}, FeelAB {(FeelAB.On ? "on" : "off")}, {(PadPass ? "PAD pass" : "keyboard pass")}, {list.Count} scenario(s) into {dir}");
            float t0 = Time.realtimeSinceStartup;
            // NIGHT18: no device's look, fire or move reaches a scenario (a pad's drift and trigger did, builds 1 and 4)
            var disabled = new List<UnityEngine.InputSystem.InputDevice>();
            var names = new List<string>();
            foreach (UnityEngine.InputSystem.InputDevice d in InputSystem.devices)
            {
                names.Add(d.displayName ?? d.name);
                if (!(d is Gamepad || d is Joystick) || !d.enabled) continue; // NIGHT18: not the mouse (disabling it froze its last delta)
                InputSystem.DisableDevice(d);
                disabled.Add(d);
            }
            Log(log, $"devices: {string.Join(", ", names)}; disabled for the reel: {disabled.Count}");
            g.playerInput.lookHeld = true; // only the reel's SetLook moves the look
            try
            {
                foreach (string s in list)
                {
                    float s0 = Time.realtimeSinceStartup;
                    if (PadPass) PlayerDevices.Last = InputDevice.Pad;
                    var res = new string[1];
                    yield return One(g, s, Path.Combine(dir, s), res);
                    Quiet(g);
                    while (pending > 0) yield return null;
                    Log(log, $"{s}: {res[0]} ({Time.realtimeSinceStartup - s0:0.0} s wall)");
                    yield return null;
                }
            }
            finally
            {
                if (FeelProbe.Recording) FeelProbe.End();
                FeelClock.End();
                Quiet(g);
                g.forcedCombat = null; // the player's devices again
                g.forcedMove = null;
                foreach (UnityEngine.InputSystem.InputDevice d in disabled) if (d.added) InputSystem.EnableDevice(d);
                g.playerInput.lookHeld = false;
                g.noRecords = g.noLab = false;
                g.attractSuppressed = attract;
                Running = false;
            }
            g.ToTitle();
            Log(log, $"done: {written} frames written, {failed} failed, {Time.realtimeSinceStartup - t0:0} s wall");
        }

        // --- helpers (the strip's, kept here so the old strip tool stays as it was) -----------------------------------------

        /// <summary>No fire, no swap, no move: the reel's input whenever it isn't firing or moving (never the devices': build 1's
        /// reel fired every 0.4 s from a real device's fire).</summary>
        static CombatInput IdleCombat => new CombatInput { weapon = -1 };

        static void Quiet(SurvivalGame g)
        {
            g.forcedCombat = IdleCombat;
            g.forcedMove = new InputFrame();
            if (g.husks != null) g.husks.frozen = false;
        }

        static void God(SurvivalGame g) => g.health.max = g.health.hp = 1e6;

        static void Fresh(SurvivalGame g)
        {
            Quiet(g);
            g.noRecords = true;
            g.noLab = true;
            g.StartRun(Seed);
            God(g);
        }

        static void HoldBreak(SurvivalGame g)
        {
            if (g.run.state == SurvivalRun.State.Break) g.run.phaseSteps = UnityTuning.ReelHoldBreakSteps;
        }

        static void Look(SurvivalGame g, Vec3 at, double rightDeg = 0)
        {
            Vec3 to = at - g.Eye;
            g.input.SetLook(FeelStrip.TurnRight(Math.Atan2(-to.x, -to.z), rightDeg), Math.Atan2(to.y, Math.Sqrt(to.x * to.x + to.z * to.z)));
        }

        static double ClearAhead(SurvivalGame g, double yaw)
        {
            var dirv = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
            return g.world.RayIntersect(g.Eye, dirv, out RayHit hit) ? hit.distance : 60;
        }

        /// <summary>The 24 headings, clearest first.</summary>
        static List<double> Yaws(SurvivalGame g)
        {
            var ys = new List<(double yaw, double d)>();
            for (int i = 0; i < 24; i++)
            {
                double yaw = i * Math.PI * 2 / 24;
                ys.Add((yaw, ClearAhead(g, yaw)));
            }
            ys.Sort((a, b) => b.d.CompareTo(a.d));
            var r = new List<double>();
            foreach (var y in ys) r.Add(y.yaw);
            return r;
        }

        static bool FloorAt(SurvivalGame g, double x, double z, double floorY) =>
            g.world.RayIntersect(new Vec3(x, floorY + 3, z), new Vec3(0, -1, 0), out RayHit down) && Math.Abs(floorY + 3 - down.distance - floorY) <= 0.05;

        /// <summary>A husk on the player's floor exactly `d` m ahead along the clearest heading that has floor there and a clear line.</summary>
        static Husk PlaceAt(SurvivalGame g, double d, double lat, out double yawUsed)
        {
            Vec3 eye = g.Eye;
            double floorY = g.Feet.y;
            foreach (double yaw in Yaws(g))
            {
                double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw), rx = Math.Cos(yaw), rz = -Math.Sin(yaw);
                double x = eye.x + fx * d + rx * lat, z = eye.z + fz * d + rz * lat;
                if (!FloorAt(g, x, z, floorY)) continue;
                var aim = new Vec3(x, floorY + 1, z);
                Vec3 to = aim - eye;
                double len = to.Length;
                if (g.world.RayIntersect(eye, to / len, out RayHit wall) && wall.distance < len) continue;
                Husk h = g.husks.Spawn(new Vec3(x, floorY, z), eye);
                if (h == null) continue;
                yawUsed = yaw;
                return h;
            }
            yawUsed = 0;
            return null;
        }

        static bool DropSpot(SurvivalGame g, double yaw, double height, out Vec3 at)
        {
            double floorY = g.Feet.y;
            double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
            foreach (double d in new[] { 6.0, 4.0, 8.0, 3.0, 10.0 })
            {
                double x = g.Feet.x + fx * d, z = g.Feet.z + fz * d;
                if (!FloorAt(g, x, z, floorY)) continue;
                if (g.world.RayIntersect(new Vec3(x, floorY + 0.5, z), new Vec3(0, 1, 0), out RayHit up) && up.distance < height + 1.5) continue;
                at = new Vec3(x, floorY + height, z);
                return true;
            }
            at = default;
            return false;
        }

        static CombatInput Fire => new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } };
        static CombatInput Punch => new CombatInput { weapon = -1, punch = new Skygrind.Core.Button { held = true, pressed = true } };

        static IEnumerator Frames(int n)
        {
            for (int i = 0; i < n; i++) yield return null;
        }

        static int Ms(int ms) => (int)Math.Round(ms * UnityTuning.ReelFps / 1000.0);

        // --- the recording ----------------------------------------------------------------------------------------------------

        static bool capturing;

        static void Start(SurvivalGame g, string name, string kind, string dir, double distance = double.NaN, int target = -1)
        {
            if (Directory.Exists(dir))
                foreach (string f in Directory.GetFiles(dir)) File.Delete(f); // a re-run replaces the scenario
            FeelClock.Begin();
            FeelProbe.Begin(g, new FeelProbe.Header
            {
                name = name, kind = kind, feelAB = FeelAB.On, seed = Seed, distance = distance, target = target, maxSpeed = g.cfg.movement.slide.floorSpeed,
            }, dir);
            capturing = true;
            g.StartCoroutine(Capture(dir));
        }

        static bool stopAsked;

        /// <summary>End the recording at this frame's end (the capture loop takes the frame, then ends the probe), then the clock.</summary>
        static IEnumerator Stop()
        {
            stopAsked = true;
            while (capturing) yield return null;
            stopAsked = false;
            FeelClock.End();
        }

        /// <summary>Every rendered frame while capturing: read back at the frame's end, encoded and written off the main thread.</summary>
        static IEnumerator Capture(string dir)
        {
            var end = new WaitForEndOfFrame();
            int lastCaptured = -1; // NIGHT49 shared (the lead's Capture fix)
            while (capturing)
            {
                yield return end;
                if (!capturing || !FeelProbe.Recording) break;
                int f = FeelProbe.FrameCount - 1; // EndFrame recorded this frame in its Update
                if (f < 0) continue;
                // NIGHT49 shared (the lead's Capture fix; NIGHT48's f1799.jpg was overwritten 20-110 times a long reel): a frame whose index did not advance is
                // skipped, so no f{index}.jpg is written twice; a stop still ends the recording
                if (f <= lastCaptured)
                {
                    if (stopAsked) { capturing = false; FeelProbe.End(); }
                    continue;
                }
                lastCaptured = f;
                Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
                if (stopAsked)
                {
                    capturing = false;
                    FeelProbe.End(); // after this frame's capture: every recorded frame has its JPG
                }
                byte[] raw = tex.GetRawTextureData();
                GraphicsFormat fmt = tex.graphicsFormat;
                uint w = (uint)tex.width, h = (uint)tex.height;
                UnityEngine.Object.Destroy(tex);
                while (Volatile.Read(ref pending) >= 6) Thread.Sleep(2); // the clock is fixed: waiting costs wall time only
                Interlocked.Increment(ref pending);
                string path = Path.Combine(dir, $"f{f:0000}.jpg");
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        byte[] jpg = ImageConversion.EncodeArrayToJPG(raw, fmt, w, h, 0, UnityTuning.ReelJpgQuality);
                        File.WriteAllBytes(path, jpg);
                        Interlocked.Increment(ref written);
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref failed);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref pending);
                    }
                });
            }
        }

        // --- the scenarios ------------------------------------------------------------------------------------------------------

        static IEnumerator One(SurvivalGame g, string s, string dir, string[] res)
        {
            var offs = new List<Action>();
            res[0] = "SKIPPED (no setup)";
            FeelClock.Begin(); // NIGHT18: the setup too runs on the fixed clock (the same steps on both FeelAB sides)
            Fresh(g);
            bool round = s == "roundend" || s == "hud-fight";
            if (!round)
            {
                HoldBreak(g);
                g.husks.frozen = true;
            }
            yield return Frames(30); // the run's first frames settle
            double yaw = Yaws(g)[0];
            g.input.SetLook(yaw, 0);
            int pre = Ms(UnityTuning.ReelPreRollMs);
            int post = Ms(UnityTuning.ReelPostRollKillMs);
            try
            {
                switch (s)
                {
                    case "hit12":
                    case "kill6":
                    case "kill12":
                    case "kill20":
                    case "headshot12":
                    {
                        double d = s == "kill6" ? 6 : s == "kill20" ? 20 : 12;
                        bool kill = s != "hit12";
                        bool head = s == "headshot12";
                        Husk mark = PlaceAt(g, d, 0, out double y);
                        if (mark == null)
                        {
                            // no floor at exactly d m from the spawn: try the player on other open floor nearby (off the record)
                            Vec3 home = g.Feet;
                            foreach (double r in UnityTuning.ReelMoveTries)
                            {
                                for (int k = 0; k < 8 && mark == null; k++)
                                {
                                    double a = k * Math.PI / 4;
                                    double x = home.x - Math.Sin(a) * r, z = home.z - Math.Cos(a) * r;
                                    if (!FloorAt(g, x, z, home.y)) continue;
                                    if (g.world.RayIntersect(new Vec3(x, home.y + 0.5, z), new Vec3(0, 1, 0), out RayHit up) && up.distance < 2.5) continue;
                                    g.Respawn(new Vec3(x, home.y, z), 0);
                                    yield return Frames(4);
                                    God(g);
                                    mark = PlaceAt(g, d, 0, out y);
                                }
                                if (mark != null) break;
                            }
                        }
                        if (mark == null)
                        {
                            res[0] = $"SKIPPED: no open floor at {d} m (the collider cast)";
                            yield break;
                        }
                        mark.hp = kill ? 1 : Math.Max(mark.hp, UnityTuning.ReelHitHp);
                        Vec3 aim = head && mark.HasHead ? mark.HeadCenter : mark.AimPoint;
                        Look(g, aim);
                        yield return Frames(20); // the view settles on the target
                        bool done = false;
                        offs.Add(g.events.On("hit", o =>
                        {
                            if (o is HitEvent h && h.owner == "player") done = true;
                        }));
                        Start(g, s, kill ? "kill" : "hit", dir, d, mark.id);
                        for (int i = 0; i < pre; i++)
                        {
                            God(g);
                            Look(g, aim);
                            yield return null;
                        }
                        FeelProbe.MarkInput();
                        int n = 0;
                        while (!done && n++ < 60)
                        {
                            God(g);
                            Look(g, aim);
                            g.forcedCombat = Fire;
                            yield return null;
                        }
                        g.forcedCombat = IdleCombat;
                        yield return Frames(kill ? post : Ms(600));
                        yield return Stop();
                        res[0] = done ? $"{FeelProbe.FrameCount} frames (target at {d} m, {(head ? "head" : "body")})" : "NO HIT in 60 frames";
                        yield break;
                    }
                    case "horde5":
                    {
                        var pack = new List<Husk>();
                        foreach (double d in UnityTuning.ReelHordeDistances)
                            foreach (double lat in UnityTuning.ReelHordeLaterals)
                            {
                                if (pack.Count >= UnityTuning.ReelHordeCount) break;
                                Husk h = PlaceAt(g, d, lat, out _);
                                if (h == null) continue;
                                h.hp = 1;
                                pack.Add(h);
                            }
                        if (pack.Count < 2)
                        {
                            res[0] = $"SKIPPED: only {pack.Count} husk(s) placed";
                            yield break;
                        }
                        string gun = UnityTuning.ReelHordeBlastGun;
                        if (g.cfg.Has($"weapons.catalog.{gun}") && !g.loadout.owned.Contains(gun)) g.loadout.AddWeapon(gun);
                        yield return Frames(60);
                        int slot = g.loadout.owned.IndexOf(gun);
                        for (int i = 0; i < 90 && slot >= 0 && g.loadout.targetSlot != slot; i++)
                        {
                            g.forcedCombat = new CombatInput { weapon = slot };
                            yield return null;
                        }
                        g.forcedCombat = IdleCombat;
                        yield return Frames(45); // the draw ends
                        Vec3 mid = default;
                        foreach (Husk h in pack) mid += h.AimPoint;
                        mid /= pack.Count;
                        int kills = 0;
                        offs.Add(g.events.On("kill", o =>
                        {
                            if (o is KillEvent k && k.owner == "player") kills++;
                        }));
                        Look(g, mid);
                        Start(g, s, "kill", dir, UnityTuning.ReelHordeDistances[0], pack[0].id);
                        for (int i = 0; i < pre; i++)
                        {
                            God(g);
                            Look(g, mid);
                            yield return null;
                        }
                        FeelProbe.MarkInput();
                        int n = 0;
                        while (kills == 0 && n++ < 60)
                        {
                            God(g);
                            Look(g, mid);
                            g.forcedCombat = Fire;
                            yield return null;
                        }
                        g.forcedCombat = IdleCombat;
                        yield return Frames(post);
                        yield return Stop();
                        res[0] = $"{FeelProbe.FrameCount} frames ({kills} of {pack.Count} killed in one blast)";
                        yield break;
                    }
                    case "punchkill":
                    {
                        Husk mark = PlaceAt(g, 1.4, 0, out _);
                        if (mark == null)
                        {
                            res[0] = "SKIPPED: no floor 1.4 m ahead";
                            yield break;
                        }
                        mark.hp = 1;
                        bool done = false;
                        offs.Add(g.events.On("kill", o =>
                        {
                            if (o is KillEvent k && k.owner == "player") done = true;
                        }));
                        Look(g, mark.AimPoint, UnityTuning.StripMeleeYawOffsetDeg);
                        yield return Frames(20);
                        Start(g, s, "kill", dir, 1.4, mark.id);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        int n = 0;
                        while (!done && n++ < 60)
                        {
                            God(g);
                            Look(g, mark.AimPoint, UnityTuning.StripMeleeYawOffsetDeg);
                            g.forcedCombat = Punch;
                            yield return null;
                        }
                        g.forcedCombat = IdleCombat;
                        yield return Frames(post);
                        yield return Stop();
                        res[0] = done ? $"{FeelProbe.FrameCount} frames" : "NO KILL in 60 frames";
                        yield break;
                    }
                    case "boost":
                    case "boostside":
                    case "sprint":
                    case "strafe":
                    case "slide":
                    {
                        bool side = s == "boostside" || s == "strafe";
                        int mz = side ? 0 : 1, mx = side ? 1 : 0;
                        if (side)
                        {
                            yaw = Yaws(g)[0] + Math.PI / 2; // strafing right runs along the clearest line
                            g.input.SetLook(yaw, 0);
                        }
                        if (s == "slide")
                        {
                            // from a sprint: the boost key held (the boost, then the sprint) for 0.6 s off the record
                            for (int i = 0; i < 40; i++)
                            {
                                God(g);
                                g.input.SetLook(yaw, 0);
                                g.forcedMove = new InputFrame { moveZ = 1, dash = new Skygrind.Core.Button { held = true, pressed = i == 0 } };
                                yield return null;
                            }
                        }
                        Start(g, s, "camera", dir);
                        for (int i = 0; i < pre; i++)
                        {
                            God(g);
                            g.input.SetLook(yaw, 0);
                            g.forcedMove = s == "slide" ? new InputFrame { moveZ = 1, dash = new Skygrind.Core.Button { held = true } } : new InputFrame();
                            yield return null;
                        }
                        FeelProbe.MarkInput();
                        int len = s == "sprint" || s == "strafe" ? Ms(1200) : Ms(900);
                        for (int i = 0; i < len; i++)
                        {
                            God(g);
                            g.input.SetLook(yaw, 0);
                            bool first = i == 0;
                            switch (s)
                            {
                                case "boost":
                                case "boostside":
                                    g.forcedMove = new InputFrame { moveZ = mz, moveX = mx, dash = new Skygrind.Core.Button { held = first, pressed = first } };
                                    break;
                                case "sprint":
                                    g.forcedMove = new InputFrame { moveZ = 1, dash = new Skygrind.Core.Button { held = true, pressed = first } };
                                    break;
                                case "strafe":
                                    g.forcedMove = new InputFrame { moveX = 1 };
                                    break;
                                default: // slide: pressed and held
                                    g.forcedMove = new InputFrame { moveZ = 1, slide = new Skygrind.Core.Button { held = true, pressed = first } };
                                    break;
                            }
                            yield return null;
                        }
                        g.forcedMove = new InputFrame();
                        yield return Stop();
                        res[0] = $"{FeelProbe.FrameCount} frames";
                        yield break;
                    }
                    case "land":
                    {
                        if (!DropSpot(g, yaw, UnityTuning.ReelLandHeight, out Vec3 drop))
                        {
                            res[0] = "SKIPPED: no open floor with clearance";
                            yield break;
                        }
                        bool landed = false;
                        Action<MoverEvent> onMove = e =>
                        {
                            if (e.type == MoverEventType.Land) landed = true; // CameraFeel logs the "land" event
                        };
                        Start(g, s, "camera", dir);
                        double landPitch = UnityTuning.ReelLandPitchDeg * Math.PI / 180;
                        g.Respawn(drop, yaw * 180 / Math.PI);
                        g.input.SetLook(yaw, landPitch);
                        g.mover.Events += onMove;
                        offs.Add(() => g.mover.Events -= onMove);
                        int n = 0;
                        while (!landed && n++ < 180)
                        {
                            God(g);
                            g.input.SetLook(yaw, landPitch);
                            yield return null;
                        }
                        FeelProbe.MarkInput(); // the landing (the fall is the pre-roll)
                        yield return Frames(Ms(500));
                        yield return Stop();
                        res[0] = landed ? $"{FeelProbe.FrameCount} frames (a {UnityTuning.ReelLandHeight} m drop)" : "NO LAND in 3 s";
                        yield break;
                    }
                    case "swapall":
                    {
                        foreach (string id in UnityTuning.StripSwapGuns)
                            if (g.cfg.Has($"weapons.catalog.{id}") && !g.loadout.owned.Contains(id)) g.loadout.AddWeapon(id);
                        yield return Frames(60);
                        Start(g, s, "camera", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        for (int k = 0; k < g.loadout.owned.Count; k++)
                        {
                            int slot = (g.loadout.targetSlot + 1) % g.loadout.owned.Count;
                            g.forcedCombat = new CombatInput { weapon = slot };
                            yield return null;
                            g.forcedCombat = IdleCombat;
                            yield return Frames(Ms(450));
                        }
                        yield return Stop();
                        res[0] = $"{FeelProbe.FrameCount} frames ({g.loadout.owned.Count} guns)";
                        yield break;
                    }
                    case "rankup":
                    {
                        int r0 = g.style.rank;
                        for (int i = 0; i < UnityTuning.ReelRankUpHusks; i++)
                        {
                            double off = (i % 2 == 0 ? 1 : -1) * (i / 2) * UnityTuning.ReelRankUpSpreadDeg * Math.PI / 180;
                            g.input.SetLook(yaw + off, 0);
                            Husk h = PlaceAt(g, 7, 0, out _);
                            if (h != null) h.hp = 1;
                        }
                        g.input.SetLook(yaw, 0);
                        bool up = false;
                        offs.Add(g.events.On("rankChange", _ =>
                        {
                            if (g.style.rank > r0 && !up)
                            {
                                up = true;
                                FeelProbe.MarkInput();
                            }
                        }));
                        Start(g, s, "ui", dir);
                        int n = 0;
                        while (!up && n++ < 600)
                        {
                            God(g);
                            FeelStrip.AimBot(g);
                            yield return null;
                        }
                        g.forcedCombat = IdleCombat;
                        yield return Frames(Ms(1500));
                        yield return Stop();
                        res[0] = up ? $"{FeelProbe.FrameCount} frames (rank {r0} -> {g.style.rank})" : "NO RANK-UP in 10 s";
                        yield break;
                    }
                    case "roundend":
                    case "hud-fight":
                    {
                        g.run.phaseSteps = 1; // the first break ends at once
                        bool cleared = false;
                        offs.Add(g.events.On("roundClear", _ =>
                        {
                            if (!cleared)
                            {
                                cleared = true;
                                if (s == "roundend") FeelProbe.MarkInput();
                            }
                        }));
                        if (s == "hud-fight")
                        {
                            // a 3 s fight once the round's enemies are on the field
                            int w = 0;
                            while (g.enemies.all.Count < 3 && w++ < 900)
                            {
                                God(g);
                                yield return null;
                            }
                            Start(g, s, "ui", dir);
                            FeelProbe.MarkInput();
                            for (int i = 0; i < Ms(3000); i++)
                            {
                                God(g);
                                FeelStrip.AimBot(g);
                                yield return null;
                            }
                            g.forcedCombat = IdleCombat;
                            yield return Stop();
                            res[0] = $"{FeelProbe.FrameCount} frames";
                            yield break;
                        }
                        // roundend: the aim bot clears round 1 off the record until one enemy is left, then records the end
                        int guard = 0;
                        while (!cleared && guard++ < 7200)
                        {
                            God(g);
                            FeelStrip.AimBot(g);
                            int alive = 0;
                            foreach (Enemy e in g.enemies.all) if (e.alive) alive++;
                            if (alive == 1 && g.run.state != SurvivalRun.State.Break && g.run.stats.kills >= UnityTuning.ReelRoundEndKillsBeforeRecord) break;
                            yield return null;
                        }
                        Start(g, s, "ui", dir);
                        int n = 0;
                        while (!cleared && n++ < 1200)
                        {
                            God(g);
                            FeelStrip.AimBot(g);
                            yield return null;
                        }
                        g.forcedCombat = IdleCombat;
                        yield return Frames(Ms(UnityTuning.ReelPostRollFlourishMs));
                        yield return Stop();
                        res[0] = cleared ? $"{FeelProbe.FrameCount} frames" : "NO ROUND CLEAR";
                        yield break;
                    }
                    case "pickup":
                    {
                        Vec3 feet = g.Feet;
                        var at = new Vec3(feet.x - Math.Sin(yaw) * UnityTuning.ReelPickupAhead, feet.y, feet.z - Math.Cos(yaw) * UnityTuning.ReelPickupAhead);
                        PowerUps.Drop d = g.powerUps.Spawn(at, UnityTuning.ReelPickupType);
                        yield return Frames(20);
                        Start(g, s, "ui", dir);
                        yield return Frames(pre);
                        int n = 0;
                        while (d.active && n++ < 300)
                        {
                            God(g);
                            g.input.SetLook(yaw, UnityTuning.ReelPickupPitchDeg * Math.PI / 180);
                            g.forcedMove = new InputFrame { moveZ = UnityTuning.ReelPickupWalk };
                            yield return null;
                        }
                        FeelProbe.MarkInput(); // the take (the walk in is the pre-roll)
                        g.forcedMove = new InputFrame();
                        yield return Frames(Ms(1200));
                        yield return Stop();
                        res[0] = $"{FeelProbe.FrameCount} frames (taken {!d.active})";
                        yield break;
                    }
                    case "ui-title":
                    {
                        Start(g, s, "ui", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        g.ToTitle();
                        yield return Frames(Ms(1200));
                        yield return Stop();
                        res[0] = $"{FeelProbe.FrameCount} frames";
                        yield break;
                    }
                    case "ui-settings":
                    {
                        g.ToTitle();
                        yield return Frames(60);
                        Start(g, s, "ui", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        g.UiView.OpenSettingsTab(g.UiView.ReelSettingsTabs()[0]);
                        yield return Frames(Ms(500));
                        for (int k = 0; k < 2; k++)
                        {
                            FeelProbe.Event("focus");
                            MoveFocus(MoveDirection.Down);
                            yield return Frames(Ms(400));
                        }
                        yield return Frames(Ms(300));
                        yield return Stop();
                        g.UiView.CloseSettings();
                        res[0] = $"{FeelProbe.FrameCount} frames";
                        yield break;
                    }
                    case "ui-paused":
                    {
                        Start(g, s, "ui", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        g.Pause();
                        yield return Frames(Ms(900));
                        yield return Stop();
                        g.Resume();
                        res[0] = $"{FeelProbe.FrameCount} frames";
                        yield break;
                    }
                    case "ui-mods":
                    {
                        Start(g, s, "ui", dir);
                        yield return Frames(pre);
                        g.mods.Queue(1);
                        int n = 0;
                        while (!g.DraftOpen && n++ < 180) yield return null;
                        FeelProbe.MarkInput();
                        yield return Frames(Ms(1000));
                        yield return Stop();
                        if (g.DraftOpen) g.PickMod(0);
                        res[0] = n < 180 ? $"{FeelProbe.FrameCount} frames" : "NO DRAFT in 3 s";
                        yield break;
                    }
                    case "ui-results":
                    {
                        try
                        {
                            if (g.settings.Bool(UnityTuning.RecapSetting))
                            {
                                g.settings.Set(UnityTuning.RecapSetting, false);
                                offs.Add(() => g.settings.Set(UnityTuning.RecapSetting, true));
                            }
                        }
                        catch (KeyNotFoundException) { }
                        Vec3 f0 = g.Feet;
                        g.health.max = g.health.hp = 100;
                        g.health.Damage(g.health.max * 2, "husk", new Vec3(f0.x, f0.y, f0.z - 3));
                        // DOWN plays off the record; the recording starts 300 ms before RESULTS can come (the down time's end)
                        int n = 0;
                        while (g.flow != Flow.Results && n++ < 1800)
                        {
                            if (!FeelProbe.Recording && g.flow == Flow.Down && n >= UnityTuning.ReelResultsRecordAfterFrames) Start(g, s, "ui", dir);
                            yield return null;
                        }
                        if (!FeelProbe.Recording) Start(g, s, "ui", dir);
                        FeelProbe.MarkInput();
                        yield return Frames(Ms(1500));
                        yield return Stop();
                        res[0] = g.flow == Flow.Results ? $"{FeelProbe.FrameCount} frames" : $"NO RESULTS (flow {g.flow})";
                        yield break;
                    }
                    case "gust":
                    {
                        // the KESTREL (slot 0 at a run's start) from a standstill: its LT aimed level, then aimed down
                        g.forcedCombat = new CombatInput { weapon = 0 };
                        yield return Frames(30);
                        g.forcedCombat = IdleCombat;
                        Start(g, s, "camera", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        g.input.SetLook(yaw, 0);
                        g.forcedCombat = new CombatInput { weapon = -1, altFire = new Skygrind.Core.Button { held = true, pressed = true } };
                        yield return null;
                        g.forcedCombat = IdleCombat;
                        yield return Frames(Ms(1200));
                        FeelProbe.Event("gustDown");
                        g.input.SetLook(yaw, -UnityTuning.ReelGustDownDeg * Math.PI / 180);
                        g.forcedCombat = new CombatInput { weapon = -1, altFire = new Skygrind.Core.Button { held = true, pressed = true } };
                        yield return null;
                        g.forcedCombat = IdleCombat;
                        yield return Frames(Ms(1200));
                        yield return Stop();
                        res[0] = $"{FeelProbe.FrameCount} frames (level, then down {UnityTuning.ReelGustDownDeg:0} deg)";
                        yield break;
                    }
                    case "vitals":
                    {
                        // a sprint (the boost key held after its press) along the clearest heading until the bar empties, then a
                        // husk 5 m ahead killed with the KESTREL: the kill's refill (design.stamina's rewards)
                        Start(g, s, "move", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        int n = 0;
                        double circleYaw = yaw; // NIGHT20: a sprint around a ~6 m circle (the yaw turning ReelVitalsTurnDegPerS), so the start area stays open for the kill
                        while (n++ < Ms(6000) && !(g.mover.stamina.rules.Any && g.mover.stamina.bar <= 0))
                        {
                            circleYaw += UnityTuning.ReelVitalsTurnDegPerS * Math.PI / 180 / UnityTuning.ReelFps;
                            g.input.SetLook(circleYaw, 0);
                            g.forcedMove = new InputFrame { moveZ = 1, dash = new Skygrind.Core.Button { held = true, pressed = n == 1 }, sprint = new Skygrind.Core.Button { held = true, pressed = n == 1 } };
                            yield return null;
                        }
                        g.forcedMove = new InputFrame();
                        FeelProbe.Event("barEmpty");
                        yield return Frames(Ms(300));
                        Husk h = PlaceAt(g, UnityTuning.ReelVitalsKillAt, 0, out double ky);
                        if (h == null)
                        {
                            yield return Stop();
                            res[0] = "NO SPOT for the kill";
                            yield break;
                        }
                        h.hp = 1;
                        g.husks.frozen = true;
                        int k = 0;
                        while (h.alive && k++ < 120)
                        {
                            Look(g, new Vec3(h.position.x, h.position.y + 1, h.position.z));
                            g.forcedCombat = Fire;
                            yield return null;
                            g.forcedCombat = IdleCombat;
                            yield return null;
                        }
                        FeelProbe.Event("kill");
                        yield return Frames(Ms(1200));
                        yield return Stop();
                        res[0] = $"{FeelProbe.FrameCount} frames (sprint {n} frames, bar {g.mover.stamina.bar:0.00}, killed {!h.alive})";
                        yield break;
                    }
                    case "ui-weapons":
                    {
                        g.ToTitle();
                        yield return Frames(60);
                        Start(g, s, "ui", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        g.UiView.ReelOpenWeapons();
                        yield return Frames(Ms(500));
                        for (int k = 0; k < 2; k++)
                        {
                            FeelProbe.Event("focus");
                            MoveFocus(MoveDirection.Down);
                            yield return Frames(Ms(400));
                        }
                        yield return Frames(Ms(300));
                        yield return Stop();
                        g.UiView.ReelCloseMeta();
                        res[0] = $"{FeelProbe.FrameCount} frames";
                        yield break;
                    }
                    case "prompt-buy":
                    case "prompt-box":
                    case "prompt-forge":
                    {
                        // stand at the buy facing it with the points to pay (the prompt in its affordable form), no press
                        g.zones.OpenAll();
                        Vec3 at;
                        double faceDeg;
                        if (s == "prompt-buy")
                        {
                            BuySpot bs = g.zones.spots.Find(z => z.zoneId == "armory");
                            g.zones.Reset(); // the gate shut again: its prompt
                            g.economy.total = Math.Max(g.economy.total, bs.price + 100);
                            at = bs.spot.pos;
                            faceDeg = -90; // the ARMORY spot faces +x (SliceSmoke)
                        }
                        else if (s == "prompt-box")
                        {
                            if (g.box == null)
                            {
                                res[0] = "SKIPPED (no box)";
                                yield break;
                            }
                            BoxSpot b = g.box.Current;
                            g.economy.total = Math.Max(g.economy.total, g.box.price + 100);
                            at = new Vec3(b.pos.x + Math.Sin(b.yaw) * 1.6, b.pos.y, b.pos.z + Math.Cos(b.yaw) * 1.6);
                            faceDeg = b.yaw * 180 / Math.PI;
                        }
                        else
                        {
                            if (!g.map.forgeSpot.HasValue)
                            {
                                res[0] = "SKIPPED (no forge on this map)";
                                yield break;
                            }
                            var f = g.map.forgeSpot.Value;
                            g.economy.total = Math.Max(g.economy.total, 100000);
                            at = new Vec3(f.pos.x + Math.Sin(f.yaw) * 1.5, f.pos.y, f.pos.z + Math.Cos(f.yaw) * 1.5);
                            faceDeg = f.yaw * 180 / Math.PI;
                        }
                        g.Respawn(at, faceDeg);
                        yield return Frames(30);
                        Start(g, s, "ui", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        yield return Frames(Ms(1000));
                        yield return Stop();
                        res[0] = $"{FeelProbe.FrameCount} frames";
                        yield break;
                    }
                    case "ui-down":
                    {
                        Vec3 f0 = g.Feet;
                        Start(g, s, "ui", dir);
                        yield return Frames(pre);
                        FeelProbe.MarkInput();
                        g.health.max = g.health.hp = 100;
                        g.health.Damage(g.health.max * 2, "husk", new Vec3(f0.x, f0.y, f0.z - 3));
                        yield return Frames(Ms(1200));
                        yield return Stop();
                        res[0] = g.flow == Flow.Down || g.flow == Flow.Results ? $"{FeelProbe.FrameCount} frames (flow {g.flow})" : $"NO DOWN (flow {g.flow})";
                        yield break;
                    }
                    default:
                        if (IsPhaseD(s))
                        {
                            yield return N20Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT20 (FeelReel.N20.cs)
                            yield break;
                        }
                        if (IsPhaseE(s))
                        {
                            yield return N21Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT21 (FeelReel.N21.cs)
                            yield break;
                        }
                        if (IsPhaseF(s))
                        {
                            yield return N22Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT22 (FeelReel.N22.cs)
                            yield break;
                        }
                        if (IsPhaseJ(s))
                        {
                            yield return N27Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT27 (FeelReel.N27.cs)
                            yield break;
                        }
                        if (IsPhaseI(s))
                        {
                            yield return N26Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT26 (FeelReel.N26.cs)
                            yield break;
                        }
                        if (IsPhaseH(s))
                        {
                            yield return N25Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT25 (FeelReel.N25.cs)
                            yield break;
                        }
                        if (IsPhaseG(s))
                        {
                            yield return N24Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT24 (FeelReel.N24.cs)
                            yield break;
                        }
                        res[0] = "SKIPPED: unknown scenario";
                        yield break;
                }
            }
            finally
            {
                foreach (Action o in offs) o();
                Quiet(g);
                if (FeelProbe.Recording)
                {
                    capturing = false;
                    FeelProbe.End();
                }
                FeelClock.End();
            }
        }

        /// <summary>The menus' focus moves one step (as a d-pad press would: the EventSystem's move event on the selected object).</summary>
        static void MoveFocus(MoveDirection dirn)
        {
            EventSystem es = EventSystem.current;
            if (es == null || es.currentSelectedGameObject == null) return;
            var data = new AxisEventData(es) { moveDir = dirn, moveVector = dirn == MoveDirection.Down ? Vector2.down : Vector2.up };
            ExecuteEvents.Execute(es.currentSelectedGameObject, data, ExecuteEvents.moveHandler);
        }
    }
}
