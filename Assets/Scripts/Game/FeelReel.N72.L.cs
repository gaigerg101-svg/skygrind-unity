// NIGHT72 builder L (L1; NIGHT72.md section 6; slate L1): the Player phase `still72`, the FLICKER METER's camera. No meter has ever measured flicker
// (gg-proxy.md: "a still cannot"); GG 2026-10-26 "there's flickers", 2026-10-20 "flickering bugs that happen constantly". This phase tours a campaign
// level as tour65 does (the same rooms, the same card poses: Tour65.Poses, the fight rooms and any room with frames65), and at EACH pose holds the
// camera still, the enemies frozen (husks.frozen), for UnityTuning.L72StillFrames rendered frames on the fixed 60 fps clock (FeelClock), and writes the
// frames' luma into one file per pose (Still72.cs documents the layout). It records no JPG strip (no Start()): the stack files are the output.
// Tools/night/n72-flicker.py reads them: per pose the share of pixels whose luma reverses direction 3+ times by more than 6 levels, the p99.9 of
// |dY|, and a heatmap PNG; its pose verdict is GREEN under the bar written there, else RED.
//   boot one Player per level (L-062):  bash Tools/night/n72-reels.sh <tag> <side>,<map>,still72[,<extra args>]
//   e.g.  bash Tools/night/n72-reels.sh n72-t1 s1,n52-spire,still72   and the planted case on the same pose set:
//         bash Tools/night/n72-reels.sh n72-t1 s1p,n52-spire,still72,-skygrindFlickerPlant72
//   a row ON for the OFF / ON pair: ...,still72,-skygrindRows72,fix72:1  (n72-reels.sh splits extra args on commas)
// -skygrindFlickerPlant72 adds an overlay block (UnityTuning.L72PlantSize px square, white / black swapped EVERY rendered frame) to the screen the
// capture reads: the planted case that proves the chain (capture -> stack file -> meter) sees a flicker and calls it RED. It is a reel-only object, made
// here and destroyed when the phase ends; the game never has it.
// The phase's own line: `done: GREEN still72 | ...` when every toured room was entered in its own room, every pose kept all its frames, no pose is
// inside a solid and the screen is 1920x1080; RED otherwise (a room with no wave is counted in the line, not RED). That is the capture's verdict, not the flicker's. A reel phase only: it changes nothing in the game.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>The planted case's switch (the slate's L1).</summary>
        public const string FlickerPlantSwitch72 = "-skygrindFlickerPlant72";

        static partial void N72StillBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Still72Scenario(g, dir, res, offs, pre);

        /// <summary>One pose's stack file as it is written: the open file, the luma planes, and what went wrong.</summary>
        sealed class Still72Writer
        {
            public string path, format, error;
            public int w, h, frames;
            public long bytes;
            FileStream fs;
            GZipStream gz;
            byte[] raw, cur, prev, delta;
            int r, g, b, stride;
            readonly int planned;

            public Still72Writer(string path, int planned) { this.path = path; this.planned = planned; }

            /// <summary>Adds the texture as the next frame; false (error set) when it cannot be read.</summary>
            public bool Add(Texture2D t)
            {
                try
                {
                    if (t == null) { error = "no capture"; return false; }
                    if (fs == null)
                    {
                        w = t.width;
                        h = t.height;
                        format = t.format.ToString();
                        if (!Still72.Layout(format, out r, out g, out b, out stride)) { error = "texture format " + format + " is not one the meter reads"; return false; }
                        cur = new byte[w * h];
                        prev = new byte[w * h];
                        delta = new byte[w * h];
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                        gz = Still72.BeginWrite(fs, w, h, planned);
                    }
                    else if (t.width != w || t.height != h) { error = "the screen changed size to " + t.width + "x" + t.height; return false; }
                    Unity.Collections.NativeArray<byte> nat = t.GetRawTextureData<byte>();
                    if (raw == null || raw.Length != nat.Length) raw = new byte[nat.Length];
                    nat.CopyTo(raw);
                    Still72.ToLuma(raw, w, h, r, g, b, stride, cur);
                    if (frames == 0) gz.Write(cur, 0, cur.Length);
                    else
                    {
                        Still72.Delta(cur, prev, delta, cur.Length);
                        gz.Write(delta, 0, delta.Length);
                    }
                    byte[] swap = prev; prev = cur; cur = swap;
                    frames++;
                    return true;
                }
                catch (Exception e)
                {
                    error = e.GetType().Name + ": " + e.Message;
                    return false;
                }
            }

            public void Close()
            {
                try
                {
                    if (gz != null) { gz.Dispose(); gz = null; }
                    if (fs != null)
                    {
                        Still72.PatchFrames(fs, frames);
                        bytes = fs.Length;
                        fs.Dispose();
                        fs = null;
                    }
                }
                catch (Exception e) { error = error ?? (e.GetType().Name + ": " + e.Message); }
            }
        }

        /// <summary>The planted block: an overlay canvas with a UnityTuning.L72PlantSize square. Null when the switch is off.</summary>
        static Image Still72Plant(List<Action> offs)
        {
            if (!NightRows.HasSwitch(Environment.GetCommandLineArgs(), FlickerPlantSwitch72)) return null;
            var go = new GameObject("FlickerPlant72");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var block = new GameObject("Block");
            block.transform.SetParent(go.transform, false);
            var img = block.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = Color.black;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(UnityTuning.L72PlantX, -UnityTuning.L72PlantY);
            rt.sizeDelta = new Vector2(UnityTuning.L72PlantSize, UnityTuning.L72PlantSize);
            offs.Add(() => { if (go != null) UnityEngine.Object.Destroy(go); });
            return img;
        }

        /// <summary>Keeps `frames` rendered frames at the current pose into one stack file; the plant, when on, swaps colour on every one.</summary>
        static IEnumerator Still72Capture(Still72Writer wr, Image plant, string pngPath, Action hold)
        {
            var end = new WaitForEndOfFrame();
            for (int k = 0; k < UnityTuning.L72StillFrames; k++)
            {
                yield return null; // this rendered frame begins
                hold?.Invoke(); // the health stays where the run put it
                if (plant != null) plant.color = (k & 1) == 0 ? Color.white : Color.black;
                yield return end;
                Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
                bool good = wr.Add(tex);
                if (good && k == 0 && pngPath != null)
                {
                    try { File.WriteAllBytes(pngPath, tex.EncodeToPNG()); } catch (Exception e) { wr.error = e.GetType().Name + ": " + e.Message; }
                }
                if (tex != null) UnityEngine.Object.Destroy(tex);
                if (!good) break;
            }
            wr.Close();
        }

        /// <summary>The planted health (L4, calm72's frames): `-skygrindStill72Health=0.45` (or the number as the next argument) holds the player at that share
        /// of 100 health at every pose, so the HUD's low-health edge and damage vignette draw as they would in play; NaN with no switch (God, as the reels do).</summary>
        static double Still72Health()
        {
            const string flag = "-skygrindStill72Health";
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string v = null;
                if (args[i] != null && args[i].StartsWith(flag + "=", StringComparison.OrdinalIgnoreCase)) v = args[i].Substring(flag.Length + 1);
                else if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) v = args[i + 1];
                if (v != null && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double f)) return Math.Max(0.001, Math.Min(1, f));
            }
            return double.NaN;
        }

        static void Hold72(SurvivalGame g, double frac)
        {
            if (double.IsNaN(frac)) { God(g); return; }
            g.health.max = 100;
            g.health.hp = 100 * frac;
        }

        static IEnumerator Still72Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with reel.sh + -skygrindMap <id> (bash Tools/night/n72-reels.sh <tag> s1,n52-spire,still72; L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            FeelClock.Begin(); // the fixed 60 fps clock (One() began it; a no-op then): every kept frame is 1 / 60 s of game time
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED still72: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            yield return Frames(pre);
            Image plant = Still72Plant(offs);
            double hf = Still72Health();
            Debug.Log("[reel] still72: input: " + UnityTuning.L72StillFrames + " frames per pose, settle " + UnityTuning.L72StillSettleFrames + ", enemies frozen, plant " + (plant != null ? "ON (" + UnityTuning.L72PlantSize + " px block at " + UnityTuning.L72PlantX + "," + UnityTuning.L72PlantY + ", swapped every frame)" : "off")
                + ", health " + (double.IsNaN(hf) ? "God (1e6)" : (hf * 100).ToString("0", ci) + " % of 100 (-skygrindStill72Health)") + ", calm72 " + (NightRows.ViewOn(g.cfg, Calm72.Row) ? "ON" : "off") + ", lit72 " + (NightRows.ViewOn(g.cfg, Lit72.Row) ? "ON" : "off")
                + ", fix72 " + (NightRows.ViewOn(g.cfg, Fix72.Row) ? "ON" : "off") + ", flicker sources off (" + Fix72.DiagSwitch + "): " + (Fix72.OffList.Length > 0 ? string.Join("+", Fix72.OffList) : "none"));
            var rows = Levels35Rows(g);
            List<double[]> solids = Tour65.Solids(g.map?.json);
            var lines = new List<string>();
            var unframed = new List<string>();
            int rooms = 0, poses = 0, kept = 0, short60 = 0, warns = 0, wrongRoom = 0, noWave = 0;
            string ended = null;
            Func<double, string> f1 = v => v.ToString("0.0", ci);
            foreach (RoomDef r in lv.rooms)
            {
                rows.TryGetValue(r.id, out var row);
                List<Tour65.Pose> list = Tour65.Poses(row, out string source);
                if (!Tour65.Toured(r.kind, source, false)) continue; // the fight rooms, and any room whose card gave it poses
                if (list.Count == 0) { unframed.Add(r.id); continue; }
                rooms++;
                g.husks.KillAll();
                yield return Frames(4);
                K52Place(g, list[0].Spot);
                float end = Time.realtimeSinceStartup + UnityTuning.L56RoomWaitS;
                while (K52Room(g) != r.id && Time.realtimeSinceStartup < end) { God(g); yield return null; }
                double t0 = sim.SimTime;
                bool waves = r.WaveCount > 0;
                while (waves && sim.enemies.AliveCount == 0 && sim.SimTime - t0 < UnityTuning.L56WaveWaitS && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (waves && sim.enemies.AliveCount == 0) noWave++;
                double t1 = sim.SimTime;
                while (sim.SimTime - t1 < UnityTuning.L56SettleS && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (ended != null) break;
                foreach (Tour65.Pose p in list)
                {
                    poses++;
                    g.husks.frozen = false;
                    K52Place(g, p.Spot);
                    yield return null; // the respawn's camera cut lands
                    yield return null;
                    g.husks.frozen = true; // the enemies stand where the wave put them: only the screen can move
                    for (int s = 0; s < UnityTuning.L72StillSettleFrames; s++) { Hold72(g, hf); g.UiView?.ClearBanners41(); yield return null; }
                    string room = K52Room(g);
                    if (room != r.id) wrongRoom++;
                    L56Count(g, out int alive, out int seen, out _);
                    string name = $"T72-{id}-{r.id}-{p.name}";
                    var wr = new Still72Writer(Path.Combine(dir, "still72-" + name + ".s72"), UnityTuning.L72StillFrames);
                    double wall0 = Time.realtimeSinceStartupAsDouble;
                    yield return Still72Capture(wr, plant, Path.Combine(dir, "shot-N72-" + name + ".png"), () => Hold72(g, hf));
                    double wall = Time.realtimeSinceStartupAsDouble - wall0;
                    God(g); // back to the reel's own health: the next room's entry is not at the planted one
                    g.husks.frozen = false;
                    bool full = wr.frames == UnityTuning.L72StillFrames && wr.error == null;
                    if (full) kept++; else short60++;
                    Vec3 cam = g.cam != null ? WebSpace.FromUnity(g.cam.transform.position) : new Vec3(double.NaN, double.NaN, double.NaN);
                    string inside = Tour65.InsideSolid(solids, p);
                    if (inside != null) warns++;
                    string line = $"{r.id} pose {p.name} ({p.source}) still72-{name}.s72 | frames {wr.frames}/{UnityTuning.L72StillFrames} | {wr.w}x{wr.h} {wr.format} | {wr.bytes} bytes | wall {wall.ToString("0.0", ci)} s"
                        + $" | feet ({f1(p.x)}, {f1(p.y)}, {f1(p.z)}) yaw {p.yawDeg.ToString("0", ci)} pitch {p.pitch.ToString("0.00", ci)} | camera ({f1(cam.x)}, {f1(cam.y)}, {f1(cam.z)})"
                        + $" | alive {alive} seen {seen} | campaign {room} | plant {(plant != null ? "on" : "off")}"
                        + (wr.error != null ? " | ERROR " + wr.error : "") + (inside != null ? $" | WARN inside a solid ({inside}): the camera in a wall" : "");
                    Debug.Log("[reel] still72: " + line);
                    lines.Add(line);
                }
            }
            g.husks.KillAll();
            g.forcedMove = null;
            yield return Stop();
            bool screen = Screen.width == 1920 && Screen.height == 1080;
            // a room whose wave never came up (noWave) is counted, not RED: the meter measures the screen, and an empty room still has one (tour65's rule is stricter)
            bool green = rooms > 0 && kept > 0 && short60 == 0 && unframed.Count == 0 && warns == 0 && wrongRoom == 0 && screen && ended == null;
            res[0] = $"done: {(green ? "GREEN" : "RED")} still72 | map {g.MapId}, level {id} | rooms {rooms}, poses {poses}, kept in full {kept}, short {short60} | WARN (a pose inside a solid) {warns}"
                + $" | in the wrong room {wrongRoom}, with no wave {noWave} | screen {Screen.width}x{Screen.height}{(screen ? "" : " (NOT 1920x1080)")}{(ended != null ? " | ended: " + ended : "")}"
                + $" | rooms with no pose (not framed): {(unframed.Count > 0 ? string.Join(", ", unframed) : "none")} | plant {(plant != null ? "on" : "off")} || " + string.Join(" || ", lines);
        }
    }
}
