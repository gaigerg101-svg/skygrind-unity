// NIGHT74 builder L (L1; NIGHT74.md section 6; slate L1): the Player phase `pan74`, the MOVING-CAMERA flicker meter's camera. NIGHT73 measured flicker only with the
// camera standing still (still72); a moving-camera flicker (SSAO crawl, bloom on thin strips) was never measured (NIGHT73-REPORT section 5; GG 2026-10-26 "there's
// flickers"). This phase tours a campaign level as still72 does (the same rooms, the card poses: Tour65.Poses, the fight rooms and any room with frames65) and, at
// each room's FIRST pose, turns the camera through a fixed slow yaw sweep (UnityTuning.N74PanFrames rendered frames at N74PanRateDeg degrees a frame, centred on the
// pose's yaw; Pan74.YawDeg: the same path every run, the pitch the pose's, the feet fixed, the enemies frozen) on the fixed 60 fps clock (FeelClock), and writes, for every
// rendered frame, the sum of the luma of every 8 x 8 pixel block and the sum of its motion-compensated residual (this frame minus the previous one warped to this camera:
// a yaw sweep is a pure rotation, so the warp is exact and depth-free; Pan74Writer self-calibrates its sign conventions on the first pair and prints how well it works)
// into one file per room (Pan74.cs documents the layout and why the residual). It records no JPG strip (no Start()): the stack files are the output, plus two PNGs per
// room (the first and the middle frame: the heat map's background and a look at the room).
// Tools/night/n74-flick.py reads them: per sweep the share of block-frames that flip (a block's residual changes sign between two steps by more than a threshold,
// 2 flips in 6 frames), the regions that flip most, a heat map; `compare` pairs a candidate run with a baseline run; `floor` pairs two baselines.
//   boot one Player per level (L-062):  bash Tools/night/n74-reels.sh <tag> <side>,<map>,pan74[,<extra args>]
//   e.g.  bash Tools/night/n74-reels.sh n74-t1 p0,n52-spire,pan74      the planted case on the same path:   ...,pan74,-skygrindFlickerPlant74
//   a row ON for the OFF / ON pair: ...,pan74,-skygrindRows72,flick74:1      one source off to rank it: ...,pan74,-skygrindFlickerOff72=bloom
//   only some rooms (a quick check; the file names keep the room id): ...,pan74,-skygrindPan74Rooms=a2+a5
// -skygrindFlickerPlant74 adds an overlay block (UnityTuning.N74PlantSize px square on the 8 px block grid, display luma 0.5 +- 0.03 swapped EVERY rendered frame) to the
// screen the capture reads: the planted case that proves the chain (capture -> stack file -> meter) sees a +-3 % alternation and flags it. It is a reel-only object, made
// here and destroyed when the phase ends; the game never has it.
// The phase's own line: `done: GREEN pan74 | ...` when every swept room was entered in its own room, every sweep kept all its frames, no pose is inside a solid and the
// screen is 1920x1080; RED otherwise. That is the capture's verdict, not the flicker's. A reel phase only: it changes nothing in the game.
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
        /// <summary>The planted case's switch (the slate's L1: "a planted +-3 % alternating-luma control that must fire").</summary>
        public const string FlickerPlantSwitch74 = "-skygrindFlickerPlant74";

        /// <summary>The room filter's switch: `-skygrindPan74Rooms=a2+a5` (one token: n74-reels.sh splits a side's extra args on commas) sweeps only those rooms.</summary>
        public const string PanRoomsSwitch74 = "-skygrindPan74Rooms";

        static partial void N74PanBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Pan74Scenario(g, dir, res, offs, pre);

        /// <summary>The planted block: an overlay canvas with a UnityTuning.N74PlantSize square on the block grid. Null when the switch is off.</summary>
        static Image Pan74Plant(List<Action> offs)
        {
            if (!NightRows.HasSwitch(Environment.GetCommandLineArgs(), FlickerPlantSwitch74)) return null;
            var go = new GameObject("FlickerPlant74");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var block = new GameObject("Block");
            block.transform.SetParent(go.transform, false);
            var img = block.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = new Color(UnityTuning.N74PlantMid, UnityTuning.N74PlantMid, UnityTuning.N74PlantMid, 1f);
            RectTransform rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(UnityTuning.N74PlantX, -UnityTuning.N74PlantY);
            rt.sizeDelta = new Vector2(UnityTuning.N74PlantSize, UnityTuning.N74PlantSize);
            offs.Add(() => { if (go != null) UnityEngine.Object.Destroy(go); });
            return img;
        }

        /// <summary>The rooms named on the command line (`-skygrindPan74Rooms=a2+a5`, or the list as the next argument): empty when none, meaning every toured room up to the cap.</summary>
        static List<string> Pan74RoomFilter()
        {
            var list = new List<string>();
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string v = null;
                if (args[i] != null && args[i].StartsWith(PanRoomsSwitch74 + "=", StringComparison.OrdinalIgnoreCase)) v = args[i].Substring(PanRoomsSwitch74.Length + 1);
                else if (string.Equals(args[i], PanRoomsSwitch74, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) v = args[i + 1];
                if (v == null) continue;
                foreach (string s in v.Split('+', ',', '/'))
                {
                    string n = s.Trim();
                    if (n.Length > 0 && !list.Contains(n)) list.Add(n);
                }
            }
            return list;
        }

        static IEnumerator Pan74Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with reel.sh + -skygrindMap <id> (bash Tools/night/n74-reels.sh <tag> p0,n52-spire,pan74; L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            FeelClock.Begin(); // the fixed 60 fps clock (One() began it; a no-op then): every kept frame is 1 / 60 s of game time
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED pan74: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            yield return Frames(pre);
            Image plant = Pan74Plant(offs);
            List<string> only = Pan74RoomFilter();
            int frames = UnityTuning.N74PanFrames;
            Debug.Log("[reel] pan74: input: " + frames + " frames per room at " + UnityTuning.N74PanRateDeg.ToString("0.00", ci) + " deg/frame (" + (UnityTuning.N74PanRateDeg * UnityTuning.ReelFps).ToString("0.0", ci) + " deg/s at "
                + UnityTuning.ReelFps + " fps; +-" + (UnityTuning.N74PanRateDeg * (frames - 1) / 2).ToString("0.0", ci) + " deg about the pose's yaw), settle " + UnityTuning.N74PanSettleFrames + ", enemies frozen, " + UnityTuning.N74PanBlock + " px blocks, at most "
                + UnityTuning.N74PanMaxRooms + " rooms, rooms " + (only.Count > 0 ? string.Join("+", only) : "every toured one") + ", plant " + (plant != null ? "ON (" + UnityTuning.N74PlantSize + " px block at " + UnityTuning.N74PlantX + "," + UnityTuning.N74PlantY
                + ", luma " + UnityTuning.N74PlantMid.ToString("0.00", ci) + " +- " + UnityTuning.N74PlantAmp.ToString("0.00", ci) + " swapped every frame)" : "off")
                + ", health God (1e6), calm72 " + (NightRows.ViewOn(g.cfg, Calm72.Row) ? "ON" : "off") + ", lit72 " + (NightRows.ViewOn(g.cfg, Lit72.Row) ? "ON" : "off") + ", fix72 " + (NightRows.ViewOn(g.cfg, Fix72.Row) ? "ON" : "off")
                + ", flick74 " + (NightRows.ViewOn(g.cfg, "flick74") ? "ON" : "off") + ", flicker sources off (" + Fix72.DiagSwitch + "): " + (Fix72.OffList.Length > 0 ? string.Join("+", Fix72.OffList) : "none"));
            var rows = Levels35Rows(g);
            List<double[]> solids = Tour65.Solids(g.map?.json);
            var lines = new List<string>();
            var unframed = new List<string>();
            var skipped = new List<string>();
            int rooms = 0, kept = 0, short240 = 0, warns = 0, wrongRoom = 0, noWave = 0;
            double lastRoomWall = 0, roomT0 = 0; // wall seconds of the last whole room (its wave wait, the settle and the sweep): the time budget's estimate of the next
            string ended = null;
            bool gridOk = true;
            Func<double, string> f1 = v => v.ToString("0.0", ci);
            foreach (RoomDef r in lv.rooms)
            {
                rows.TryGetValue(r.id, out var row);
                List<Tour65.Pose> list = Tour65.Poses(row, out string source);
                if (!Tour65.Toured(r.kind, source, false)) continue; // the fight rooms, and any room whose card gave it poses
                if (only.Count > 0 && !only.Contains(r.id)) { skipped.Add(r.id); continue; }
                if (list.Count == 0) { unframed.Add(r.id); continue; }
                if (only.Count == 0 && rooms >= UnityTuning.N74PanMaxRooms) { skipped.Add(r.id); continue; }
                if (lastRoomWall > 0 && Time.realtimeSinceStartupAsDouble + lastRoomWall * 1.2 + 15 > UnityTuning.N74PanBudgetS) { skipped.Add(r.id + " (time)"); continue; } // another sweep would run into reel.sh's 420 s (counted from the Player's start: realtimeSinceStartup)
                rooms++;
                roomT0 = Time.realtimeSinceStartupAsDouble;
                Tour65.Pose p = list[0]; // the room's first card pose is the sweep's spot (the same for every run: it comes from the level file)
                double yaw0 = Pan74.YawDeg(p.yawDeg, 0, frames);
                g.husks.KillAll();
                yield return Frames(4);
                K52Place(g, p.Spot);
                float end = Time.realtimeSinceStartup + UnityTuning.L56RoomWaitS;
                while (K52Room(g) != r.id && Time.realtimeSinceStartup < end) { God(g); yield return null; }
                double t0 = sim.SimTime;
                bool waves = r.WaveCount > 0;
                while (waves && sim.enemies.AliveCount == 0 && sim.SimTime - t0 < UnityTuning.L56WaveWaitS && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (waves && sim.enemies.AliveCount == 0) noWave++;
                double t1 = sim.SimTime;
                while (sim.SimTime - t1 < UnityTuning.L56SettleS && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (ended != null) break;
                g.husks.frozen = false;
                K52Place(g, new[] { p.x, p.y, p.z, yaw0, p.pitch }); // back at the pose, turned to the sweep's first yaw
                yield return null; // the respawn's camera cut lands
                yield return null;
                g.husks.frozen = true; // the enemies stand where the wave put them: only the camera moves
                for (int s = 0; s < UnityTuning.N74PanSettleFrames; s++) { God(g); g.UiView?.ClearBanners41(); yield return null; }
                string room = K52Room(g);
                if (room != r.id) wrongRoom++;
                L56Count(g, out int alive, out int seen, out _);
                string name = $"{id}-{r.id}";
                var wr = new Pan74Writer(Path.Combine(dir, "pan74-" + name + ".p74"), frames, g.cam != null ? g.cam.fieldOfView : 75.0, p.pitch);
                double wall0 = Time.realtimeSinceStartupAsDouble;
                var endOfFrame = new WaitForEndOfFrame();
                var padRegions = new Pan74Pads(g, solids); // NIGHT74 pads74: where each jump pad is on the screen, per frame (a file beside the stack: n74-flick.py pads); reads the scene, writes nothing to it
                for (int k = 0; k < frames; k++)
                {
                    double yawDeg = Pan74.YawDeg(p.yawDeg, k, frames), yaw = yawDeg * Math.PI / 180;
                    g.input.SetLook(yaw, p.pitch); // this frame's Update renders the camera at yaw k (the look is read there)
                    g.forcedMove = new InputFrame { yaw = yaw, pitch = p.pitch };
                    g.forcedCombat = IdleCombat;
                    God(g);
                    g.UiView?.ClearBanners41();
                    if (plant != null)
                    {
                        float grey = UnityTuning.N74PlantMid + ((k & 1) == 0 ? UnityTuning.N74PlantAmp : -UnityTuning.N74PlantAmp); // 0.53, 0.47, 0.53 ...: +-3 % of the range every frame
                        plant.color = new Color(grey, grey, grey, 1f);
                    }
                    yield return null; // this rendered frame begins: Update has turned the camera
                    yield return endOfFrame;
                    Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
                    bool good = wr.Add(tex);
                    if (good) padRegions.Add(k); // the camera is where the captured frame drew it (after the end of the frame)
                    if (good && (k == 0 || k == frames / 2))
                    {
                        try { File.WriteAllBytes(Path.Combine(dir, "shot-N74-pan74-" + name + (k == 0 ? "-first" : "-mid") + ".png"), tex.EncodeToPNG()); } catch (Exception e) { wr.error = e.GetType().Name + ": " + e.Message; }
                    }
                    if (tex != null) UnityEngine.Object.Destroy(tex);
                    if (!good) break;
                }
                wr.Close();
                if (padRegions.padCount > 0 && !padRegions.Save(Path.Combine(dir, "pan74-" + name + ".pads"))) Debug.Log("[reel] pan74: WARN pad regions not written for " + name + ": " + padRegions.error); // never the sweep's verdict
                double wall = Time.realtimeSinceStartupAsDouble - wall0;
                lastRoomWall = Time.realtimeSinceStartupAsDouble - roomT0;
                God(g);
                g.husks.frozen = false;
                if (wr.error != null && wr.error.IndexOf("blocks", StringComparison.Ordinal) >= 0) gridOk = false;
                bool full = wr.frames == frames && wr.error == null;
                if (full) kept++; else short240++;
                Vec3 cam = g.cam != null ? WebSpace.FromUnity(g.cam.transform.position) : new Vec3(double.NaN, double.NaN, double.NaN);
                string inside = Tour65.InsideSolid(solids, p);
                if (inside != null) warns++;
                string line = $"{r.id} sweep pan74-{name}.p74 | frames {wr.frames}/{frames} | blocks {wr.bw}x{wr.bh} of {UnityTuning.N74PanBlock} px | {wr.w}x{wr.h} {wr.format} | {wr.bytes} bytes | wall {wall.ToString("0.0", ci)} s"
                    + $" | pose {p.name} ({p.source}) feet ({f1(p.x)}, {f1(p.y)}, {f1(p.z)}) yaw {p.yawDeg.ToString("0", ci)} -> {Pan74.YawDeg(p.yawDeg, 0, frames).ToString("0.0", ci)} .. {Pan74.YawDeg(p.yawDeg, frames - 1, frames).ToString("0.0", ci)} pitch {p.pitch.ToString("0.00", ci)}"
                    + $" | camera ({f1(cam.x)}, {f1(cam.y)}, {f1(cam.z)}) | alive {alive} seen {seen} | campaign {room} | plant {(plant != null ? "on" : "off")} | pads {padRegions.padCount} built, in view in {padRegions.framesWithPad} of {padRegions.frames} frames, pads74 {(NightRows.ViewOn(g.cfg, Pad74Math.Row) ? "ON" : "off")} | compensation: {wr.calib}"
                    + (wr.error != null ? " | ERROR " + wr.error : "") + (inside != null ? $" | WARN inside a solid ({inside}): the camera in a wall" : "");
                Debug.Log("[reel] pan74: " + line);
                lines.Add(line);
            }
            g.husks.KillAll();
            g.forcedMove = null;
            yield return Stop();
            bool screen = Screen.width == 1920 && Screen.height == 1080;
            // a room whose wave never came up (noWave) is counted, not RED: the meter measures the screen, and an empty room still has one
            bool green = rooms > 0 && kept > 0 && short240 == 0 && unframed.Count == 0 && warns == 0 && wrongRoom == 0 && screen && gridOk && ended == null;
            res[0] = $"done: {(green ? "GREEN" : "RED")} pan74 | map {g.MapId}, level {id} | rooms {rooms}, sweeps kept in full {kept}, short {short240} | WARN (a pose inside a solid) {warns}"
                + $" | in the wrong room {wrongRoom}, with no wave {noWave} | screen {Screen.width}x{Screen.height}{(screen ? "" : " (NOT 1920x1080)")}{(ended != null ? " | ended: " + ended : "")}"
                + $" | rooms with no pose (not framed): {(unframed.Count > 0 ? string.Join(", ", unframed) : "none")} | rooms not swept (cap, filter or time budget):{(skipped.Count > 0 ? string.Join(", ", skipped) : "none")} | plant {(plant != null ? "on" : "off")} || " + string.Join(" || ", lines);
        }
    }
}
