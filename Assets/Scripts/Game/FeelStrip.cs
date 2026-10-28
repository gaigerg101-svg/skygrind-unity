// NIGHT16 U3-29.7 (builder U): the strip tool, `SKYGRIND.exe -skygrindStrip <dir> [scenario]` (PlayerChecks), then quit.
// An event-triggered contact sheet: a scripted run waits for its event, then grabs N consecutive frames, each stamped with
// the real ms since the trigger and the sim step (g.simStep), all on ONE PNG per scenario in <dir>:
// <dir>/n16-strip-<scenario>.png. A grab costs frame time (the readback), so the STEP stamp is the truth: equal steps on
// two frames (the stamp says HELD) mean no sim step ran between them (a freeze, or a slow-mo's slow frames).
// Frames and gap per scenario (UnityTuning.Strip*; -skygrindStripFrames N / -skygrindStripGap K override every scenario):
// 10 at gap 1, swap 36 at gap 2 (~0.6 s: holster, raise, fire-open, the settle), draw 40 at gap 1, punch-press 24, land 12.
// The stamp is a TMP text on its own top canvas, set every frame before the frame renders, so each grabbed frame carries
// its own stamp; its size follows the cell (FeelStrip.StampSize: StripStampSheetPx tall on the sheet). Each frame is grabbed
// at the end of its frame (WaitForEndOfFrame + ScreenCapture.CaptureScreenshotAsTexture); a group's full frames are kept
// until the group ends, then box-filtered into its rows (a CPU filter) and freed, so a grab costs only the readback. The
// tool allocates; the game's paths are untouched (it only listens: bus events by name, the mover's Events).
// Every scenario is a fresh run (god mode, no records, the web's rules). Except slowmo, the first break is held (no round
// starts, no spawner: the world stays empty) and every husk but a parry's is frozen. The player faces the longest clear
// line found by casting the collider; actors are placed by casting it too, never through a wall.
// - freeze: a husk 6-10 m ahead; the gun in hand aims at its head and fires: the trigger is the headshot kill.
// - punch: a husk 1.4 m ahead at 1 HP; the camera turned off it (StripMeleeYawOffsetDeg) so the left fist reads: the
//   trigger is the punch kill. punch-press: the same, the trigger is the punch's press (the `punch` event), 24 frames.
// - parry: a husk (with its brain) 1.5 m ahead winds up; the punch lands inside design.parry.perfectWindow (the same
//   camera offset): the trigger is the `parry` event (perfect when the window is on).
// - slowmo: the break skipped, round 1's husks cleared by the aim bot (the smoke's: the nearest head, the gun in hand):
//   the trigger is the round's last kill (`roundClear`, or `slowMo` if it comes first).
// - slide: 0.4 s of running forward, then the slide held on flat ground: the trigger is the mover's SlideStart.
// - land: the player let go StripLandHeight m above open floor, the camera StripLandPitchDeg down (at the landing point): the mover's Land.
// - swap: the kit-rigged guns (StripSwapGuns: the rest of the catalog has no viewmodel rig) given, then each in turn by
//   the forced combat input: the trigger is each `weaponSwap`; one group of rows per gun (StripSwapCols a row).
// - draw: the KESTREL and the THUNDERCLAP only, 40 frames each at gap 1, for a close look at the draw.
// - results (only when named: not in the all-scenarios run): the god-mode player killed (a blow of twice the max HP, as
//   the smoke's step 4) with RECAP off for the scenario (restored after), so DOWN leads straight to RESULTS: the trigger
//   is the first frame the flow is RESULTS, 14 frames at gap 1 (RESULTS' 0.22 s fade-in and rise, UiFeelHud.cs).
// The log: "strip <scenario>: <path> (<n> frames, steps a..b; trigger ...)" or "strip <scenario>: SKIPPED <why>".
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public static class FeelStrip
    {
        public static readonly string[] Scenarios = { "freeze", "punch", "punch-press", "parry", "slowmo", "slide", "land", "swap", "draw" };

        /// <summary>Scenarios run only when named (the full run's capture count stays as it was).</summary>
        public static readonly string[] NamedOnly = { "results" };

        /// <summary>
        /// NIGHT17 U3-30.3: the feel reel's strips (`-skygrindReel <dir>`, or each named with -skygrindStrip): kill (a body kill
        /// with the gun), headshot (= freeze), punchkill (= punch), hit (a hit that doesn't kill), horde (five husks and the
        /// THUNDERCLAP), parry, boost, sprint (the boost key held on), swapall (= swap), rankup (the aim bot's kills until the
        /// rank climbs), roundend (round 1 cleared: the round's end, the next round's number). Sheets named n17-strip-*.png.
        /// </summary>
        public static readonly string[] ReelScenarios = { "kill", "headshot", "punchkill", "hit", "horde", "parry", "boost", "boostside", "sprint", "slide", "swapall", "rankup", "roundend", "spawn", "pickup" };

        /// <summary>NIGHT17: the reel's subset (-skygrindReelOnly a,b,c; null: everything), strips and ShotTour's screens.</summary>
        public static string[] ReelOnly;

        /// <summary>Is reel item `name` wanted (no subset given, or named in it)?</summary>
        public static bool ReelWants(string name) => ReelOnly == null || Array.IndexOf(ReelOnly, name) >= 0;

        /// <summary>The sheets' file prefix (the reel writes n17-).</summary>
        public static string FilePrefix = "n16-strip-";

        /// <summary>A reel name's older scenario (headshot = freeze, punchkill = punch, swapall = swap); else the name.</summary>
        public static string Base(string s) => s == "headshot" ? "freeze" : s == "punchkill" ? "punch" : s == "swapall" ? "swap" : s;

        // --- the sheet's layout and the stamps (pure; FeelHudTests) ---------------------------------------------------------

        /// <summary>One strip (groups 1): a near-square grid; several (swap): one row per strip, one column per frame.</summary>
        public static void Grid(int frames, int groups, out int cols, out int rows)
        {
            frames = Math.Max(1, frames);
            if (groups > 1)
            {
                cols = frames;
                rows = groups;
                return;
            }
            cols = (int)Math.Ceiling(Math.Sqrt(frames));
            rows = (frames + cols - 1) / cols;
        }

        /// <summary>Groups of `perGroup` frames, at most `maxCols` a row: each group wraps into its own rows and starts on a new row.</summary>
        public static void GroupGrid(int perGroup, int groups, int maxCols, out int cols, out int rowsPerGroup, out int rows)
        {
            perGroup = Math.Max(1, perGroup);
            cols = Math.Max(1, Math.Min(perGroup, maxCols));
            rowsPerGroup = (perGroup + cols - 1) / cols;
            rows = Math.Max(1, groups) * rowsPerGroup;
        }

        /// <summary>Frame `frame` of group `group`'s cell index (row-major over the sheet).</summary>
        public static int GroupCell(int group, int frame, int cols, int rowsPerGroup) => group * rowsPerGroup * cols + frame;

        /// <summary>The overlay's text size (screen px) that reads `sheetPx` tall once the frame is scaled into a `cellW` cell.</summary>
        public static float StampSize(float sheetPx, int cellW, int frameW) => sheetPx * frameW / Math.Max(1, cellW);

        /// <summary>The yaw `deg` degrees to the right (yaw 0 looks toward -z, + yaw turns left: right is -).</summary>
        public static double TurnRight(double yaw, double deg) => yaw - deg * Math.PI / 180;

        /// <summary>A cell's height for `cellW` keeping the frame's aspect.</summary>
        public static int CellHeight(int cellW, int frameW, int frameH) => (int)Math.Round((double)cellW * frameH / Math.Max(1, frameW));

        /// <summary>Cell `index`'s pixel origin on the sheet (rows go up from the bottom in a texture): index 0 is the top row's left cell.</summary>
        public static void CellOrigin(int index, int cols, int rows, int cellW, int cellH, out int x, out int y)
        {
            int col = index % cols, row = index / cols;
            x = col * cellW;
            y = (rows - 1 - row) * cellH;
        }

        /// <summary>A frame's stamp: the strip, the frame (from 1), the real ms since the trigger, the sim step and its offset, HELD when no step ran since the frame before.</summary>
        public static string Stamp(string scenario, int frame, int frames, double ms, int step, int step0, int prevStep)
        {
            var ci = CultureInfo.InvariantCulture;
            string held = frame > 0 && step == prevStep ? "  HELD" : "";
            return string.Format(ci, "{0} {1}/{2}  +{3:0.0} MS\nSTEP {4}  +{5}{6}", (scenario ?? "").ToUpperInvariant(), frame + 1, frames, ms, step, step - step0, held);
        }

        // --- per scenario -------------------------------------------------------------------------------------------------

        struct Spec
        {
            public int frames, gap, maxCols, cellW;
            public string[] guns; // swap / draw: one group per gun
        }

        static Spec SpecFor(string s, int argFrames, int argGap)
        {
            var sp = new Spec { frames = UnityTuning.StripFrames, gap = 1, maxCols = 0, cellW = UnityTuning.StripCellWidth };
            switch (s)
            {
                case "swap":
                case "swapall":
                    sp = new Spec { frames = UnityTuning.StripSwapFrames, gap = UnityTuning.StripSwapGap, maxCols = UnityTuning.StripSwapCols, cellW = UnityTuning.StripSwapCellWidth, guns = UnityTuning.StripSwapGuns };
                    break;
                // NIGHT17 round 2: the reel's kills run past the freeze (the blocks' release)
                case "kill":
                case "headshot":
                case "punchkill":
                    sp.frames = UnityTuning.ReelKillFrames;
                    sp.gap = UnityTuning.ReelKillGap;
                    break;
                case "draw":
                    sp = new Spec { frames = UnityTuning.StripDrawFrames, gap = 1, maxCols = UnityTuning.StripDrawCols, cellW = UnityTuning.StripDrawCellWidth, guns = UnityTuning.StripDrawGuns };
                    break;
                case "punch-press":
                    sp.frames = UnityTuning.StripPunchPressFrames;
                    break;
                case "results":
                    sp.frames = UnityTuning.StripResultsFrames;
                    break;
                case "land":
                    sp.frames = UnityTuning.StripLandFrames;
                    break;
                // NIGHT17 U3-30.3: the reel's own (UnityTuning.Reel.cs)
                case "horde":
                    sp.frames = UnityTuning.ReelHordeFrames;
                    sp.gap = UnityTuning.ReelHordeGap;
                    break;
                case "boost":
                case "boostside":
                    sp.frames = UnityTuning.ReelBoostFrames;
                    sp.gap = UnityTuning.ReelBoostGap;
                    break;
                case "sprint":
                    sp.frames = UnityTuning.ReelSprintFrames;
                    sp.gap = UnityTuning.ReelSprintGap;
                    break;
                case "rankup":
                    sp.frames = UnityTuning.ReelRankUpFrames;
                    sp.gap = UnityTuning.ReelRankUpGap;
                    break;
                case "roundend":
                    sp.frames = UnityTuning.ReelRoundEndFrames;
                    sp.gap = UnityTuning.ReelRoundEndGap;
                    break;
                case "spawn":
                case "pickup":
                    sp.frames = UnityTuning.ReelBeyondFrames;
                    sp.gap = UnityTuning.ReelBeyondGap;
                    break;
            }
            if (argFrames > 0) sp.frames = argFrames;
            if (argGap > 0) sp.gap = argGap;
            sp.frames = Math.Max(1, Math.Min(UnityTuning.StripMaxFrames, sp.frames));
            return sp;
        }

        // --- the run ------------------------------------------------------------------------------------------------------

        sealed class Trigger
        {
            public bool fired;
            public double t0;
            public int step0;
            public string what;
            public SurvivalGame g;

            public void Fire(string why)
            {
                if (fired) return;
                fired = true;
                t0 = Time.realtimeSinceStartupAsDouble;
                step0 = g.simStep;
                what = why;
            }
        }

        /// <summary>The overlay stamp: a top canvas, a dark plate, a TMP text (TMP's default font).</summary>
        sealed class StampView
        {
            readonly GameObject root;
            readonly TextMeshProUGUI text;
            readonly RectTransform plate;
            float pad;

            public StampView()
            {
                root = new GameObject("Strip stamp", typeof(RectTransform), typeof(Canvas));
                var canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = short.MaxValue;
                var plateGo = new GameObject("Plate", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                plate = plateGo.GetComponent<RectTransform>();
                plate.SetParent(root.transform, false);
                plate.anchorMin = plate.anchorMax = plate.pivot = new Vector2(0, 1);
                var img = plateGo.GetComponent<UnityEngine.UI.Image>();
                img.color = new Color(0, 0, 0, UnityTuning.StripStampAlpha);
                img.raycastTarget = false;
                var textGo = new GameObject("Text", typeof(RectTransform));
                textGo.transform.SetParent(plate, false);
                text = textGo.AddComponent<TextMeshProUGUI>();
                if (text.font == null) text.font = TMP_Settings.defaultFontAsset;
                text.fontStyle = FontStyles.Bold;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.TopLeft;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.raycastTarget = false;
                RectTransform tr = text.rectTransform;
                tr.anchorMin = tr.anchorMax = tr.pivot = new Vector2(0, 1);
                Size(StampSize(UnityTuning.StripStampSheetPx, UnityTuning.StripCellWidth, Screen.width));
                Show(false);
            }

            /// <summary>The text's size in screen px (the plate's padding follows it).</summary>
            public void Size(float px)
            {
                text.fontSize = px;
                pad = px * 0.2f;
                plate.anchoredPosition = new Vector2(pad, -pad);
                text.rectTransform.anchoredPosition = new Vector2(pad, -pad);
            }

            public void Set(string s)
            {
                Show(true);
                text.SetText(s);
                Vector2 size = text.GetPreferredValues(s);
                text.rectTransform.sizeDelta = size;
                plate.sizeDelta = size + new Vector2(2 * pad, 2 * pad);
            }

            public void Show(bool on)
            {
                if (root.activeSelf != on) root.SetActive(on);
            }

            public void Destroy() => UnityEngine.Object.Destroy(root);
        }

        /// <summary>The contact sheet, filled a group at a time (each group's full frames freed once drawn).</summary>
        sealed class SheetBuilder
        {
            readonly int groups, perGroup, maxCols, cellWanted;
            int fw, fh, cellW, cellH, cols, rowsPerGroup, rows, sw, sh;
            Color32[] px;
            public int frames, firstStep = int.MinValue, lastStep;

            public SheetBuilder(int groups, int perGroup, int maxCols, int cellW)
            {
                this.groups = Math.Max(1, groups);
                this.perGroup = perGroup;
                this.maxCols = maxCols;
                cellWanted = cellW;
            }

            void Init(Texture2D first)
            {
                fw = first.width;
                fh = first.height;
                cellW = Math.Min(fw, cellWanted);
                cellH = CellHeight(cellW, fw, fh);
                if (maxCols > 0) GroupGrid(perGroup, groups, maxCols, out cols, out rowsPerGroup, out rows);
                else
                {
                    Grid(perGroup, 1, out cols, out rowsPerGroup);
                    rows = rowsPerGroup * groups;
                }
                int gut = UnityTuning.StripGutter;
                sw = cols * (cellW + gut) - gut;
                sh = rows * (cellH + gut) - gut;
                px = new Color32[sw * sh];
                var back = new Color32(12, 11, 22, 255); // the dusk panel (ui.theme.panel's rgb) between the cells
                for (int i = 0; i < px.Length; i++) px[i] = back;
            }

            /// <summary>Group `group`'s frames into their cells; the textures are destroyed.</summary>
            public void Put(int group, List<Texture2D> shots, List<int> steps)
            {
                if (shots.Count == 0) return;
                if (px == null) Init(shots[0]);
                int gut = UnityTuning.StripGutter;
                for (int i = 0; i < shots.Count && i < cols * rowsPerGroup; i++)
                {
                    Texture2D t = shots[i];
                    if (t.width == fw && t.height == fh) // a resize mid-strip: the cell stays empty
                    {
                        CellOrigin(GroupCell(group, i, cols, rowsPerGroup), cols, rows, cellW + gut, cellH + gut, out int ox, out int oy);
                        Blit(t.GetPixels32(), ox, oy);
                    }
                    frames++;
                }
                if (firstStep == int.MinValue) firstStep = steps[0];
                lastStep = steps[steps.Count - 1];
                foreach (Texture2D t in shots) UnityEngine.Object.Destroy(t);
                shots.Clear();
                steps.Clear();
            }

            void Blit(Color32[] src, int ox, int oy)
            {
                for (int y = 0; y < cellH; y++)
                {
                    int sy0 = y * fh / cellH, sy1 = Math.Max(sy0 + 1, (y + 1) * fh / cellH);
                    for (int x = 0; x < cellW; x++)
                    {
                        int sx0 = x * fw / cellW, sx1 = Math.Max(sx0 + 1, (x + 1) * fw / cellW);
                        int r = 0, gg = 0, b = 0, n = 0;
                        for (int yy = sy0; yy < sy1; yy++)
                        {
                            int row = yy * fw;
                            for (int xx = sx0; xx < sx1; xx++)
                            {
                                Color32 c = src[row + xx];
                                r += c.r;
                                gg += c.g;
                                b += c.b;
                                n++;
                            }
                        }
                        px[(oy + y) * sw + ox + x] = new Color32((byte)(r / n), (byte)(gg / n), (byte)(b / n), 255);
                    }
                }
            }

            public bool Empty => px == null;

            public void Save(string path)
            {
                var tex = new Texture2D(sw, sh, TextureFormat.RGBA32, false);
                tex.SetPixels32(px);
                tex.Apply(false);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);
            }
        }

        static void Log(List<string> log, string s)
        {
            string l = "[strip] " + s;
            Debug.Log(l);
            log?.Add(l);
        }

        static int ArgInt(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : -1;
        }

        /// <summary>The strip run for `scenario` (null: every scenario) into `dir`; each sheet's path and verdict appended to `log`.</summary>
        public static IEnumerator Run(SurvivalGame g, string dir, string scenario, List<string> log, string[] reel = null)
        {
            Directory.CreateDirectory(dir);
            int argFrames = ArgInt("-skygrindStripFrames"), argGap = ArgInt("-skygrindStripGap");
            Log(log, $"strips on '{g.MapId}', {Screen.width}x{Screen.height}, frames {(argFrames > 0 ? argFrames.ToString() : "per scenario")}, gap {(argGap > 0 ? argGap.ToString() : "per scenario")}, into {Path.GetFullPath(dir)}");
            var stamp = new StampView();
            string[] list = reel ?? (scenario == null ? Scenarios : new[] { scenario });
            try
            {
                foreach (string s in list)
                {
                    if (Array.IndexOf(Scenarios, s) < 0 && Array.IndexOf(NamedOnly, s) < 0 && Array.IndexOf(ReelScenarios, s) < 0)
                    {
                        Log(log, $"strip {s}: SKIPPED unknown scenario (one of {string.Join(", ", Scenarios)}, {string.Join(", ", NamedOnly)}, {string.Join(", ", ReelScenarios)})");
                        continue;
                    }
                    yield return One(g, s, dir, SpecFor(s, argFrames, argGap), stamp, log);
                    Quiet(g);
                    yield return Wait(0.5);
                }
            }
            finally
            {
                Quiet(g);
                stamp.Destroy();
                g.noRecords = g.noLab = false;
            }
            g.ToTitle();
            Log(log, "done");
        }

        static void Quiet(SurvivalGame g)
        {
            g.forcedCombat = null;
            g.forcedMove = null;
            if (g.husks != null) g.husks.frozen = false;
        }

        static IEnumerator Wait(double seconds)
        {
            double t0 = Time.realtimeSinceStartupAsDouble;
            int n = 0;
            while (n < 2 || Time.realtimeSinceStartupAsDouble - t0 < seconds)
            {
                n++;
                yield return null;
            }
        }

        static void Fresh(SurvivalGame g, uint seed)
        {
            Quiet(g);
            g.noRecords = true; // the strips' runs never become personal bests
            g.noLab = true; // and play the web's rules whatever the LAB tab says
            g.StartRun(seed);
            God(g);
        }

        /// <summary>The first break never ends (its step count set far out): no round begins, the spawner never runs.</summary>
        static void HoldBreak(SurvivalGame g)
        {
            if (g.run.state == SurvivalRun.State.Break) g.run.phaseSteps = UnityTuning.ReelHoldBreakSteps; // NIGHT17: a believable countdown on screen (int.MaxValue / 2 showed "ROUND 1 IN 8947848")
        }

        static void God(SurvivalGame g) => g.health.max = g.health.hp = 1e6;

        static void Look(SurvivalGame g, Vec3 at, double rightDeg = 0)
        {
            Vec3 to = at - g.Eye;
            g.input.SetLook(TurnRight(Math.Atan2(-to.x, -to.z), rightDeg), Math.Atan2(to.y, Math.Sqrt(to.x * to.x + to.z * to.z)));
        }

        /// <summary>The yaw (of 24) whose horizontal line from the eye runs longest before a wall (capped at 40 m).</summary>
        static double OpenYaw(SurvivalGame g)
        {
            Vec3 eye = g.Eye;
            double best = 0, bestD = -1;
            for (int i = 0; i < 24; i++)
            {
                double yaw = i * Math.PI * 2 / 24;
                var dir = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
                double d = g.world.RayIntersect(eye, dir, out RayHit hit) ? hit.distance : 40;
                if (d > bestD)
                {
                    bestD = d;
                    best = yaw;
                }
            }
            return best;
        }

        /// <summary>Is there floor at (x, z) on the player's level (a ray down lands within 0.05 m of it)?</summary>
        static bool FloorAt(SurvivalGame g, double x, double z, double floorY) =>
            g.world.RayIntersect(new Vec3(x, floorY + 3, z), new Vec3(0, -1, 0), out RayHit down) && Math.Abs(floorY + 3 - down.distance - floorY) <= 0.05;

        /// <summary>A husk on the player's floor `ds` m ahead along `yaw` (lateral tries), in clear sight (the collider cast).</summary>
        static Husk PlaceAhead(SurvivalGame g, double yaw, double[] ds, double[] lats, bool withBrain)
        {
            Vec3 eye = g.Eye;
            double floorY = g.Feet.y;
            double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw), rx = Math.Cos(yaw), rz = -Math.Sin(yaw);
            foreach (double d in ds)
            {
                foreach (double lat in lats)
                {
                    double x = eye.x + fx * d + rx * lat, z = eye.z + fz * d + rz * lat;
                    if (!FloorAt(g, x, z, floorY)) continue;
                    var aim = new Vec3(x, floorY + 1, z);
                    Vec3 to = aim - eye;
                    double len = to.Length;
                    if (g.world.RayIntersect(eye, to / len, out RayHit wall) && wall.distance < len) continue;
                    Husk h = g.husks.Spawn(new Vec3(x, floorY, z), eye);
                    if (h == null) continue;
                    if (withBrain) g.brain.Attach(h);
                    return h;
                }
            }
            return null;
        }

        /// <summary>A point on the player's floor 3-10 m ahead along `yaw` with StripLandHeight + 1.5 m clear above it.</summary>
        static bool DropSpot(SurvivalGame g, double yaw, out Vec3 at)
        {
            double floorY = g.Feet.y;
            double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
            foreach (double d in new[] { 6.0, 4.0, 8.0, 3.0, 10.0 })
            {
                double x = g.Feet.x + fx * d, z = g.Feet.z + fz * d;
                if (!FloorAt(g, x, z, floorY)) continue;
                double need = UnityTuning.StripLandHeight + 1.5;
                if (g.world.RayIntersect(new Vec3(x, floorY + 0.5, z), new Vec3(0, 1, 0), out RayHit up) && up.distance < need) continue;
                at = new Vec3(x, floorY + UnityTuning.StripLandHeight, z);
                return true;
            }
            at = default;
            return false;
        }

        /// <summary>Forced swaps to `slot` until a weaponSwap comes (3 s at most); the swapped gun or null.</summary>
        static IEnumerator SwapTo(SurvivalGame g, int slot, string[] swapped)
        {
            swapped[0] = null;
            double ts = Time.realtimeSinceStartupAsDouble;
            while (swapped[0] == null && Time.realtimeSinceStartupAsDouble - ts < 3)
            {
                God(g);
                g.forcedCombat = new CombatInput { weapon = slot };
                yield return null;
            }
            g.forcedCombat = null;
        }

        /// <summary>One scenario: set up, wait for the trigger (or the cap), grab, make the sheet.</summary>
        static IEnumerator One(SurvivalGame g, string label, string dir, Spec spec, StampView stamp, List<string> log)
        {
            string s = Base(label); // NIGHT17: a reel name runs its older scenario (headshot = freeze ...); the sheet keeps the name
            var shots = new List<Texture2D>();
            var steps = new List<int>();
            int groups = spec.guns != null ? spec.guns.Length : 1;
            var sheet = new SheetBuilder(groups, spec.frames, spec.maxCols, spec.cellW);
            int filled = 0;
            string skipped = null;
            var trig = new Trigger { g = g };
            var offs = new List<Action>();
            try
            {
                Fresh(g, 160716);
                if (s != "slowmo" && s != "roundend" && s != "spawn")
                {
                    HoldBreak(g); // no round, no spawns: the world stays empty
                    g.husks.frozen = s != "parry";
                }
                yield return Wait(0.6); // the run's first frames settle (the title's fade, the spawn)
                double yaw = OpenYaw(g);
                g.input.SetLook(yaw, 0);
                double cap = s == "slowmo" || s == "roundend" || s == "rankup" ? UnityTuning.StripCapRound : UnityTuning.StripCap;
                double t0 = Time.realtimeSinceStartupAsDouble;
                bool Timeout() => Time.realtimeSinceStartupAsDouble - t0 > cap;
                stamp.Size(StampSize(UnityTuning.StripStampSheetPx, Math.Min(Screen.width, spec.cellW), Screen.width));

                if (spec.guns != null)
                {
                    // swap / draw: the kit-rigged guns given as a wall buy gives them; each in turn, one group of rows each.
                    foreach (string id in spec.guns)
                        if (g.cfg.Has($"weapons.catalog.{id}") && !g.loadout.owned.Contains(id)) g.loadout.AddWeapon(id);
                    yield return Wait(1.0); // a bought gun reaches the hand first
                    var swapped = new string[1];
                    offs.Add(g.events.On("weaponSwap", o => swapped[0] = o as string));
                    for (int k = 0; k < spec.guns.Length; k++)
                    {
                        string id = spec.guns[k];
                        int slot = g.loadout.owned.IndexOf(id);
                        if (slot < 0) continue;
                        if (g.loadout.targetSlot == slot)
                        {
                            // in hand already: swap away first (not grabbed), so the grabbed swap is a whole draw
                            int other = slot == 0 ? 1 : 0;
                            if (other >= g.loadout.owned.Count) continue;
                            yield return SwapTo(g, other, swapped);
                            yield return Wait(0.6);
                        }
                        God(g);
                        g.input.SetLook(yaw, 0);
                        trig = new Trigger { g = g };
                        yield return SwapTo(g, slot, swapped);
                        if (swapped[0] == null) continue;
                        trig.Fire("weaponSwap " + swapped[0]);
                        string name = g.cfg.Has($"weapons.catalog.{id}.name") ? g.cfg.Str($"weapons.catalog.{id}.name") : id;
                        yield return Grab(g, trig, name, spec, stamp, shots, steps);
                        sheet.Put(k, shots, steps);
                        filled++;
                        yield return Wait(0.4); // the draw settles before the next
                    }
                    if (filled == 0) skipped = "no weaponSwap event (guns owned: " + string.Join(", ", g.loadout.owned) + ")";
                    yield break;
                }

                if (s == "results")
                {
                    // RECAP off while it runs (DOWN then goes straight to RESULTS), restored in the finally.
                    try
                    {
                        if (g.settings.Bool(UnityTuning.RecapSetting))
                        {
                            g.settings.Set(UnityTuning.RecapSetting, false);
                            offs.Add(() => g.settings.Set(UnityTuning.RecapSetting, true));
                        }
                    }
                    catch (KeyNotFoundException) { } // no RECAP row (design.recap off): nothing to turn off
                    Vec3 feet = g.Feet;
                    g.health.Damage(g.health.max * 2, "husk", new Vec3(feet.x, feet.y, feet.z - 3));
                    if (g.flow != Flow.Down)
                    {
                        skipped = $"the blow didn't down the player (flow {g.flow})";
                        yield break;
                    }
                    while (g.flow != Flow.Results && !Timeout()) yield return null;
                    if (g.flow != Flow.Results)
                    {
                        skipped = $"no RESULTS in {cap:0} s (flow {g.flow})";
                        yield break;
                    }
                    trig.Fire("flow RESULTS");
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    yield break;
                }

                if (s == "slide" || s == "land")
                {
                    bool land = s == "land";
                    double pitch = land ? UnityTuning.StripLandPitchDeg * Math.PI / 180 : 0;
                    Vec3 drop = default;
                    if (land && !DropSpot(g, yaw, out drop))
                    {
                        skipped = $"no open floor ahead with {UnityTuning.StripLandHeight + 1.5:0.0} m clear above it (the collider cast)";
                        yield break;
                    }
                    MoverEventType want = land ? MoverEventType.Land : MoverEventType.SlideStart;
                    if (land)
                    {
                        g.Respawn(drop, yaw * 180 / Math.PI); // let go in the air (before the handler: the spawn's own landing doesn't count)
                        g.input.SetLook(yaw, pitch);
                    }
                    Action<MoverEvent> onMove = e =>
                    {
                        if (e.type == want) trig.Fire(land ? $"land (fall speed {e.fallSpeed:0.0} m/s)" : "slideStart");
                    };
                    g.mover.Events += onMove;
                    offs.Add(() => g.mover.Events -= onMove);
                    double run0 = Time.realtimeSinceStartupAsDouble;
                    while (!trig.fired && !Timeout())
                    {
                        God(g);
                        g.input.SetLook(yaw, pitch);
                        if (!land)
                        {
                            bool slide = Time.realtimeSinceStartupAsDouble - run0 > 0.4;
                            g.forcedMove = new InputFrame { moveZ = 1, slide = new Skygrind.Core.Button { held = slide, pressed = slide } };
                        }
                        yield return null;
                    }
                    if (!trig.fired)
                    {
                        skipped = $"no {want} in the cap";
                        yield break;
                    }
                    if (!land) g.forcedMove = new InputFrame { moveZ = 1, slide = new Skygrind.Core.Button { held = true } }; // held through the strip
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    yield break;
                }

                if (s == "slowmo" || s == "roundend")
                {
                    offs.Add(g.events.On("roundClear", _ => trig.Fire("roundClear")));
                    offs.Add(g.events.On(FeelKeys.SlowMo, _ => trig.Fire("slowMo")));
                    g.run.phaseSteps = 1; // the first break ends at once (Ready's way)
                    while (!trig.fired && !Timeout())
                    {
                        God(g);
                        AimBot(g);
                        yield return null;
                    }
                    g.forcedCombat = null;
                    if (!trig.fired)
                    {
                        skipped = $"round {g.run.round} not cleared in {cap:0} s ({g.run.stats.kills} kills)";
                        yield break;
                    }
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    yield break;
                }

                // NIGHT17 U3-30.3 (the reel): a body kill or a hit that doesn't kill, with the gun in hand, one husk 6-9 m ahead.
                if (s == "kill" || s == "hit")
                {
                    bool kill = s == "kill";
                    Husk mark = PlaceAhead(g, yaw, new[] { 7.0, 6.0, 9.0 }, new[] { 0.0, 1.5, -1.5 }, false);
                    if (mark == null)
                    {
                        skipped = "no open floor ahead for a husk (the collider cast)";
                        yield break;
                    }
                    if (kill) mark.hp = Math.Min(mark.hp, 1); // the first hit kills
                    else mark.hp = Math.Max(mark.hp, UnityTuning.ReelHitHp); // the hit never kills
                    offs.Add(g.events.On("hit", o =>
                    {
                        if (o is HitEvent h && h.owner == "player" && h.killed == kill) trig.Fire((kill ? "kill" : "hit") + (h.headshot ? " (headshot)" : " (body)"));
                    }));
                    while (!trig.fired && !Timeout())
                    {
                        God(g);
                        Look(g, mark.AimPoint);
                        g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } };
                        yield return null;
                    }
                    g.forcedCombat = null;
                    if (!trig.fired)
                    {
                        skipped = $"no {s} in {cap:0} s";
                        yield break;
                    }
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    yield break;
                }

                // NIGHT17 (the reel): a knot of husks at 1 HP 3.6-4.6 m ahead (0.55 m apart: round 1 had 5-7 m, 0.9 m, and one blast
                // took only 2 of 5), the THUNDERCLAP in hand: one blast, a multikill.
                if (s == "horde")
                {
                    var pack = new List<Husk>();
                    foreach (double d in new[] { 4.0, 4.6, 3.6 })
                        foreach (double lat in new[] { 0.0, 0.55, -0.55, 1.1, -1.1 })
                        {
                            if (pack.Count >= UnityTuning.ReelHordeCount) break;
                            Husk h = PlaceAhead(g, yaw, new[] { d }, new[] { lat }, false);
                            if (h == null) continue;
                            h.hp = 1;
                            pack.Add(h);
                        }
                    if (pack.Count < 2)
                    {
                        skipped = $"only {pack.Count} husk(s) placed (the collider cast)";
                        yield break;
                    }
                    string gun = UnityTuning.ReelHordeGun;
                    if (g.cfg.Has($"weapons.catalog.{gun}") && !g.loadout.owned.Contains(gun)) g.loadout.AddWeapon(gun);
                    yield return Wait(1.0);
                    var swapped = new string[1];
                    offs.Add(g.events.On("weaponSwap", o => swapped[0] = o as string));
                    int slot = g.loadout.owned.IndexOf(gun);
                    if (slot >= 0 && g.loadout.targetSlot != slot) yield return SwapTo(g, slot, swapped);
                    yield return Wait(0.7); // the draw ends
                    int kills = 0;
                    offs.Add(g.events.On("kill", o =>
                    {
                        if (o is KillEvent k && k.owner == "player")
                        {
                            kills++;
                            trig.Fire($"kill ({k.weapon}) of a pack of {pack.Count}");
                        }
                    }));
                    Vec3 mid = default;
                    foreach (Husk h in pack) mid += h.AimPoint;
                    mid /= pack.Count;
                    while (!trig.fired && !Timeout())
                    {
                        God(g);
                        Look(g, mid);
                        g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } };
                        yield return null;
                    }
                    g.forcedCombat = null;
                    if (!trig.fired)
                    {
                        skipped = $"no kill in {cap:0} s";
                        yield break;
                    }
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    trig.what += $" ({kills} killed by the strip's end)";
                    yield break;
                }

                // NIGHT17 (the reel): run on flat ground, then the boost key: pressed until the dash starts (boost), or pressed
                // and held on (sprint: STAMINA KEY one key); the sprint's trigger is ReelSprintAfter s after the boost starts.
                if (s == "boost" || s == "boostside" || s == "sprint")
                {
                    bool sprint = s == "sprint";
                    bool side = s == "boostside"; // strafing right: moveX 1, no moveZ
                    bool dashed = false;
                    double dashAt = double.NaN;
                    Action<MoverEvent> onMove = e =>
                    {
                        if (e.type == MoverEventType.Dash && !dashed)
                        {
                            dashed = true;
                            dashAt = Time.realtimeSinceStartupAsDouble;
                            if (!sprint) trig.Fire("dash");
                        }
                    };
                    g.mover.Events += onMove;
                    offs.Add(() => g.mover.Events -= onMove);
                    double run0 = Time.realtimeSinceStartupAsDouble;
                    while (!trig.fired && !Timeout())
                    {
                        God(g);
                        g.input.SetLook(yaw, 0);
                        bool on = Time.realtimeSinceStartupAsDouble - run0 > UnityTuning.ReelBoostRunIn;
                        g.forcedMove = new InputFrame { moveZ = side ? 0 : 1, moveX = side ? 1 : 0, dash = new Skygrind.Core.Button { held = on && (sprint || !dashed), pressed = on && !dashed } };
                        if (sprint && dashed && Time.realtimeSinceStartupAsDouble - dashAt >= UnityTuning.ReelSprintAfter) trig.Fire($"the boost key held {UnityTuning.ReelSprintAfter:0.00} s after the dash");
                        yield return null;
                    }
                    if (!trig.fired)
                    {
                        skipped = "no dash in the cap";
                        yield break;
                    }
                    g.forcedMove = new InputFrame { moveZ = side ? 0 : 1, moveX = side ? 1 : 0, dash = new Skygrind.Core.Button { held = sprint } }; // held on through the strip
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    yield break;
                }

                // NIGHT17 (the reel): husks at 1 HP in an arc 6-9 m ahead; the aim bot kills until the style rank climbs.
                if (s == "rankup")
                {
                    if (g.style == null)
                    {
                        skipped = "no style meter";
                        yield break;
                    }
                    int r0 = g.style.rank;
                    for (int i = 0; i < UnityTuning.ReelRankUpHusks; i++)
                    {
                        double side = (i % 2 == 0 ? 1 : -1) * (i / 2) * UnityTuning.ReelRankUpSpreadDeg * Math.PI / 180;
                        Husk h = PlaceAhead(g, yaw + side, new[] { 7.0, 6.0, 9.0 }, new[] { 0.0, 0.8, -0.8 }, false);
                        if (h != null) h.hp = 1;
                    }
                    offs.Add(g.events.On("rankChange", _ =>
                    {
                        if (g.style.rank > r0) trig.Fire($"rankChange {r0} -> {g.style.rank}");
                    }));
                    while (!trig.fired && !Timeout())
                    {
                        God(g);
                        AimBot(g);
                        yield return null;
                    }
                    g.forcedCombat = null;
                    if (!trig.fired)
                    {
                        skipped = $"the rank didn't climb in {cap:0} s (rank {g.style.rank})";
                        yield break;
                    }
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    yield break;
                }

                // NIGHT17 U3-30.10's evidence: round 1's first enemy arriving (the break ends at once); the camera turns to the
                // spawn point on the `enemySpawn` event, in the frame it lands.
                if (s == "spawn")
                {
                    offs.Add(g.events.On("enemySpawn", o =>
                    {
                        if (trig.fired || !(o is SpawnEvent e)) return;
                        Look(g, e.position + new Vec3(0, 1, 0));
                        trig.Fire("enemySpawn " + e.kind);
                    }));
                    g.run.phaseSteps = 1; // the first break ends at once
                    while (!trig.fired && !Timeout())
                    {
                        God(g);
                        yield return null;
                    }
                    if (!trig.fired)
                    {
                        skipped = $"no enemySpawn in {cap:0} s";
                        yield break;
                    }
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    yield break;
                }

                // NIGHT17 U3-30.10's evidence: a power-up 3 m ahead on the open line, walked into: the trigger is 1.6 m from it
                // (the view's pull starts PickupPullRange outside the sim's grab radius), the strip runs through the take.
                if (s == "pickup")
                {
                    Vec3 feet = g.Feet;
                    var at = new Vec3(feet.x - Math.Sin(yaw) * UnityTuning.ReelPickupAhead, feet.y, feet.z - Math.Cos(yaw) * UnityTuning.ReelPickupAhead);
                    PowerUps.Drop d = g.powerUps.Spawn(at, UnityTuning.ReelPickupType);
                    yield return Wait(0.3);
                    while (!trig.fired && !Timeout())
                    {
                        God(g);
                        g.input.SetLook(yaw, UnityTuning.ReelPickupPitchDeg * Math.PI / 180);
                        g.forcedMove = new InputFrame { moveZ = UnityTuning.ReelPickupWalk };
                        Vec3 p = g.Feet;
                        double at0 = g.cfg.Num("powerUps.grabRadius") + UnityTuning.ReelPickupTriggerOutside; // read once a frame in a capture tool
                        if (Math.Sqrt((p.x - d.position.x) * (p.x - d.position.x) + (p.z - d.position.z) * (p.z - d.position.z)) <= at0 || !d.active)
                            trig.Fire($"{at0:0.0} m from the {UnityTuning.ReelPickupType} power-up (the grab at {g.cfg.Num("powerUps.grabRadius"):0.0} m)");
                        yield return null;
                    }
                    if (!trig.fired)
                    {
                        skipped = "never reached the power-up";
                        yield break;
                    }
                    yield return Grab(g, trig, label, spec, stamp, shots, steps);
                    sheet.Put(0, shots, steps);
                    yield break;
                }

                // freeze, punch, punch-press, parry: one husk ahead.
                bool melee = s != "freeze";
                bool punch = s == "punch" || s == "punch-press";
                Husk target = melee
                    ? PlaceAhead(g, yaw, new[] { 1.4, 1.6, 1.2 }, new[] { 0.0, 0.3, -0.3 }, s == "parry")
                    : PlaceAhead(g, yaw, new[] { 8.0, 6.0, 10.0 }, new[] { 0.0, 1.5, -1.5 }, false);
                if (target == null)
                {
                    skipped = "no open floor ahead for a husk (the collider cast)";
                    yield break;
                }
                if (punch) target.hp = Math.Min(target.hp, 1); // the first punch kills
                double off = melee ? UnityTuning.StripMeleeYawOffsetDeg : 0;
                int perfectSteps = g.cfg.Design("parry.perfect") ? (int)Math.Round(g.cfg.Num("design.parry.perfectWindow") * 120) : 0;
                if (s == "freeze")
                    offs.Add(g.events.On("kill", o =>
                    {
                        if (o is KillEvent k && k.owner == "player" && k.headshot) trig.Fire("kill (headshot, " + k.weapon + ")");
                    }));
                else if (s == "punch")
                    offs.Add(g.events.On("kill", o =>
                    {
                        if (o is KillEvent k && k.owner == "player" && (k.weapon == "punch" || k.melee)) trig.Fire("kill (" + k.weapon + ")");
                    }));
                else if (s == "punch-press")
                    offs.Add(g.events.On("punch", o =>
                    {
                        if (o is MeleeEvent m && m.owner == "player") trig.Fire(m.hits > 0 ? "punch (a hit)" : "punch (a miss)");
                    }));
                else
                    offs.Add(g.events.On("parry", o =>
                    {
                        if (o is HitEvent h && h.owner == "player") trig.Fire(h.perfect ? "parry (PERFECT)" : "parry");
                    }));
                while (!trig.fired && !Timeout())
                {
                    God(g);
                    if (!target.alive && s != "parry")
                    {
                        skipped = "the husk died without the trigger (" + s + ")";
                        yield break;
                    }
                    if (s == "freeze")
                    {
                        Look(g, target.HasHead ? target.HeadCenter : target.AimPoint);
                        g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } };
                    }
                    else if (punch)
                    {
                        Look(g, target.AimPoint, off);
                        g.forcedCombat = new CombatInput { weapon = -1, punch = new Skygrind.Core.Button { held = true, pressed = true } };
                    }
                    else
                    {
                        if (target.alive) Look(g, target.AimPoint, off);
                        int w = target.windupSteps;
                        // inside the perfect window with a step or two of slack (the input reaches the sim next frame)
                        bool now = w > 0 && (perfectSteps <= 0 || (w <= perfectSteps - 2 && w > 2));
                        g.forcedCombat = now ? new CombatInput { weapon = -1, punch = new Skygrind.Core.Button { held = true, pressed = true } } : new CombatInput { weapon = -1 };
                    }
                    yield return null;
                }
                g.forcedCombat = null;
                if (!trig.fired)
                {
                    skipped = $"no trigger in {cap:0} s";
                    yield break;
                }
                yield return Grab(g, trig, label, spec, stamp, shots, steps);
                sheet.Put(0, shots, steps);
            }
            finally
            {
                foreach (Action o in offs) o();
                Quiet(g);
                stamp.Show(false);
                foreach (Texture2D t in shots) UnityEngine.Object.Destroy(t); // a group cut short
                if (skipped != null || sheet.Empty) Log(log, $"strip {label}: SKIPPED {skipped ?? "no frames"}");
                else
                {
                    string path = Path.Combine(dir, FilePrefix + label + ".png");
                    try
                    {
                        sheet.Save(path);
                        Log(log, $"strip {label}: {path} ({sheet.frames} frames, every {spec.gap} frame(s), steps {sheet.firstStep}..{sheet.lastStep}; trigger {trig.what})");
                    }
                    catch (Exception e)
                    {
                        Log(log, $"strip {label}: SKIPPED the sheet failed: {e.GetType().Name}: {e.Message}");
                    }
                }
            }
        }

        /// <summary>The smoke's aim bot: the nearest live enemy's head (else its aim point), the gun in hand firing.</summary>
        public static void AimBot(SurvivalGame g)
        {
            Enemy best = null;
            double bestD = double.PositiveInfinity;
            Vec3 eye = g.Eye;
            foreach (Enemy e in g.enemies.all)
            {
                if (!e.alive) continue;
                double d = Vec3.DistanceTo(e.AimPoint, eye);
                if (d < bestD)
                {
                    bestD = d;
                    best = e;
                }
            }
            if (best == null)
            {
                g.forcedCombat = new CombatInput { weapon = -1 };
                return;
            }
            Look(g, best is Husk h && h.HasHead ? h.HeadCenter : best.AimPoint);
            g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } };
        }

        /// <summary>N frames from the trigger's frame (every gap-th): the stamp set before each frame renders, the frame read back at its end.</summary>
        static IEnumerator Grab(SurvivalGame g, Trigger trig, string label, Spec spec, StampView stamp, List<Texture2D> shots, List<int> steps)
        {
            int prev = trig.step0, taken = 0, frame = 0;
            var end = new WaitForEndOfFrame();
            while (taken < spec.frames)
            {
                bool take = frame % spec.gap == 0;
                int step = g.simStep;
                if (take) stamp.Set(Stamp(label, taken, spec.frames, (Time.realtimeSinceStartupAsDouble - trig.t0) * 1000, step, trig.step0, prev));
                yield return end;
                if (take)
                {
                    shots.Add(ScreenCapture.CaptureScreenshotAsTexture());
                    steps.Add(step);
                    prev = step;
                    taken++;
                }
                frame++;
                if (taken < spec.frames) yield return null;
            }
            stamp.Show(false);
        }
    }
}
