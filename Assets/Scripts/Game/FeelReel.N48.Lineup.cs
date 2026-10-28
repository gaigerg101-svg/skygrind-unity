// NIGHT48 builder F: the a948 phase, the LINEUP reel (lineup48, a reel-only switch; G12 the WARDEN's shield, G13 the visors, G14 the small
// robots; NIGHT48.md section 1: "a lineup reel that hides the viewmodel and the HUD and stands each kind alone on a neutral plate, the camera
// at 3 m (close-up), 10, 20 and 30 m, front / side / 45 deg, off then on, colour + grayscale"). NIGHT47's a847 lineup stood the kinds small
// behind the viewmodel at 10 m (Logs/n47/r3-lineup.jpg): off and on could not be told apart. Run:
//   reel.sh + -skygrindMap <any campaign level, e.g. n47-iceline> + -skygrindReelOnly a948
// The CAMPAIGN on the booted map (N24StartCampaign, L-062), God, every enemy frozen, the player's input back (no bot), standing still. Then a
// STAGE high over the player (Lineup48StageAboveM over the roof the feet see, at most Lineup48StageMaxM): a neutral unlit floor plate and a
// lighter backdrop behind the body, along the heading whose 34 m line is clear; the HUD (every root canvas) and the viewmodel's camera off
// (Lineup48Rig, after every LateUpdate), the main camera at the reel's pose and the game's own base field of view. Each kind alone on the
// plate (husk, tank, swarmer, spitter, charger, warden, drone; the lancer if it spawns), frozen, near-endless health; per pass (OFF: visor47 /
// foes47 / foes48 forced off = NIGHT46's models; ON: all three forced on) the camera at 3 / 10 / 20 / 30 m and the body turned front / side /
// 45 deg to it. Written (dir):
//   shot-N30-L48-sheet-<kind>.png / -gray.png    a 6 x 4 contact sheet: columns OFF front, side, 45 | ON front, side, 45; rows 3, 10, 20, 30 m;
//                                                 10 / 20 / 30 m cells are a 1:1 crop at 1080p (320 px: the true read), the 3 m cell a 960 px crop scaled to 320
//   shot-N30-L48-<kind>-<off|on>-<10m-45|20m-front>.png   full frames (the read at 10 and 20 m)
//   shot-N30-L48-sheet-warden-poses.png / -gray.png    rows OFF 3 m, ON 3 m, OFF 10 m, ON 10 m; columns front, side, 45, BLOCKING (a block's
//                                                 kick held), DOWN front, DOWN 45 (the shield lowered: stunned / opened)
//   shot-N30-L48-sheet-swarmer-gait.png         the 2 s gait strip: the swarmer walked side-on across the plate at Lineup48GaitMps, 4 m away, a
//                                                 cell every 1/6 s (12 a pass): rows OFF 1-6, OFF 7-12, ON 1-6, ON 7-12
//   shot-N30-L48-pair-<kind>-<close|10m>-<P|Q>.png  the blind pair (old = OFF vs new = ON), 640 px crops: "close" = 3 m 45 deg, "10m" = 10 m 45
//                                                 deg (the warden: front); P / Q by a seeded coin per kind; the key is the line's last part
// Round 2 (the lead's n48-r1 read: "the 20 m swarmer is a ~10 px dot"):
//   shot-N30-L48-zoom-<kind>.png / -gray.png    a 6 x 4 sheet: columns OFF front, side, 45 | ON front, side, 45; rows 10 m, 20 m, 30 m (the same
//                                                 frames' pixels round the kind's whole silhouette, magnified nearest-neighbour: the true read, big
//                                                 enough to judge) and 20 m TELE (the 20 m stand-off, the field of view narrowed to the 5 m size)
//   shot-N30-L48-<kind>-<off|on>-20m-tele.png   the tele frame, front
//   shot-N30-L48-pair-<kind>-20m-<P|Q>.png      the blind pair at 20 m, 45 deg (true pixels x nearest to 640)
//   shot-N30-L48-sheet-swarmer-cycle.png        one 0.5 m stride in 12 cells (one drawn frame a step: the 2 s strip aliased the 6.4 Hz gait)
// The line: per kind and pass the visor's drawn height x width (px at 1080p, Visor47Probe) at 10 / 20 / 30 m; the warden's shield probe per
// pose (Foes48.Probe: least clearance to the body's drawn capsule in m, vertices inside it, the arm's corners out past the shield's front, the
// shield's centre x off the axis and y as a share of the height); the HUD canvases / viewmodel camera drawn at the captures (want 0 / off).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N48Lineup(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Lineup48Scenario(g, dir, res, offs, pre);

        /// <summary>One contact sheet: 6 columns x 4 rows of Lineup48Cell px cells (a wider gutter between columns 3 and 4: OFF | ON).</summary>
        sealed class Sheet48
        {
            public const int Cols = 6, Rows = 4, Gut = 6, Mid = 18;
            public readonly int cell, w, h;
            public readonly Color32[] px, gray;

            public Sheet48(int cell)
            {
                this.cell = cell;
                w = Cols * cell + (Cols - 1) * Gut + (Mid - Gut);
                h = Rows * cell + (Rows - 1) * Gut;
                px = new Color32[w * h];
                gray = new Color32[w * h];
                var bg = new Color32(18, 18, 22, 255);
                for (int i = 0; i < px.Length; i++) { px[i] = bg; gray[i] = bg; }
            }

            int X(int col) => col * (cell + Gut) + (col >= 3 ? Mid - Gut : 0);
            int Y(int row) => row * (cell + Gut); // from the top

            /// <summary>The cell (col, row) from the frame `src` (sw x sh, bottom-up rows): a `crop` px square centred at (cx, cy) (screen px, y up),
            /// box-filtered down to the cell.</summary>
            public void Put(int col, int row, Color32[] src, int sw, int sh, float cx, float cy, int crop)
            {
                int x0 = X(col), y0 = Y(row);
                float step = (float)crop / cell;
                int k = Mathf.Max(1, Mathf.CeilToInt(step));
                for (int v = 0; v < cell; v++)
                    for (int u = 0; u < cell; u++)
                    {
                        float sx0 = cx - crop * 0.5f + u * step, sy0 = cy + crop * 0.5f - (v + 1) * step;
                        int r = 0, gg = 0, b = 0, n = 0;
                        for (int j = 0; j < k; j++)
                            for (int i = 0; i < k; i++)
                            {
                                int sx = (int)(sx0 + (i + 0.5f) * step / k), sy = (int)(sy0 + (j + 0.5f) * step / k);
                                if (sx < 0 || sy < 0 || sx >= sw || sy >= sh) continue;
                                Color32 c = src[sy * sw + sx];
                                r += c.r; gg += c.g; b += c.b; n++;
                            }
                        Color32 o = n > 0 ? new Color32((byte)(r / n), (byte)(gg / n), (byte)(b / n), 255) : new Color32(0, 0, 0, 255);
                        int di = (h - 1 - (y0 + v)) * w + x0 + u;
                        px[di] = o;
                        byte l = (byte)Mathf.Clamp(Mathf.RoundToInt(0.2126f * o.r + 0.7152f * o.g + 0.0722f * o.b), 0, 255);
                        gray[di] = new Color32(l, l, l, 255);
                    }
            }

            public void Write(string dir, string name, bool withGray)
            {
                var t = new Texture2D(w, h, TextureFormat.RGB24, false);
                try
                {
                    System.IO.Directory.CreateDirectory(dir);
                    t.SetPixels32(px);
                    t.Apply(false);
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + name + ".png"), t.EncodeToPNG());
                    if (withGray)
                    {
                        t.SetPixels32(gray);
                        t.Apply(false);
                        System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + name + "-gray.png"), t.EncodeToPNG());
                    }
                }
                finally { UnityEngine.Object.Destroy(t); }
            }
        }

        /// <summary>The frame at its end (the HUD / viewmodel off by the rig): its pixels into `into[0]` (bottom-up rows), `size` its width / height;
        /// `fullName` set: also saved as shot-N30-&lt;fullName&gt;.png.</summary>
        static IEnumerator Grab48(string dir, Color32[][] into, int[] size, string fullName)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                into[0] = t.GetPixels32();
                size[0] = t.width;
                size[1] = t.height;
                if (!string.IsNullOrEmpty(fullName))
                {
                    System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + fullName + ".png"), t.EncodeToPNG());
                }
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        /// <summary>A crop of the last grab as its own PNG (the blind pair's frames).</summary>
        static void Crop48(string dir, string name, Color32[] src, int sw, int sh, float cx, float cy, int crop)
        {
            var o = new Color32[crop * crop];
            for (int v = 0; v < crop; v++)
                for (int u = 0; u < crop; u++)
                {
                    int sx = (int)(cx - crop * 0.5f) + u, sy = (int)(cy - crop * 0.5f) + v;
                    o[v * crop + u] = sx < 0 || sy < 0 || sx >= sw || sy >= sh ? new Color32(0, 0, 0, 255) : src[sy * sw + sx];
                }
            var t = new Texture2D(crop, crop, TextureFormat.RGB24, false);
            try
            {
                t.SetPixels32(o);
                t.Apply(false);
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + name + ".png"), t.EncodeToPNG());
            }
            finally { UnityEngine.Object.Destroy(t); }
        }

        /// <summary>Round 2: the kind's whole silhouette on screen: (centre x, centre y, the crop's side in px = the larger of its width / height x
        /// Lineup48ZoomPad), from the body's box (its radius widened for the parts that stand out: the warden's shield, the charger's horns, the
        /// drone's saucer) projected by its 8 corners.</summary>
        static Vector3 Sil48(Camera cam, Husk h, string kind)
        {
            double r = kind == "warden" ? 1.15 : kind == "drone" ? 0.95 : kind == "charger" ? 1.3 : Math.Max(0.5, h.Radius * 1.8);
            double y0 = kind == "drone" ? h.position.y - 0.7 : h.position.y - 0.05, y1 = kind == "drone" ? h.position.y + 0.7 : h.position.y + h.Height * 1.12 + 0.05;
            float lx = float.MaxValue, hx = float.MinValue, ly = float.MaxValue, hy = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                var c = new Vec3(h.position.x + ((i & 1) == 0 ? -r : r), (i & 2) == 0 ? y0 : y1, h.position.z + ((i & 4) == 0 ? -r : r));
                Vector3 s = cam.WorldToScreenPoint(WebSpace.ToUnity(c));
                if (s.z <= 0) continue;
                lx = Mathf.Min(lx, s.x); hx = Mathf.Max(hx, s.x); ly = Mathf.Min(ly, s.y); hy = Mathf.Max(hy, s.y);
            }
            if (hx < lx) return new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 64);
            return new Vector3((lx + hx) * 0.5f, (ly + hy) * 0.5f, Mathf.Max(hx - lx, hy - ly) * UnityTuning.Lineup48ZoomPad);
        }

        /// <summary>A `crop` px square of the last grab magnified nearest-neighbour to `outPx` (the blind pair's 20 m frame: the true pixels, big).</summary>
        static void CropUp48(string dir, string name, Color32[] src, int sw, int sh, float cx, float cy, int crop, int outPx)
        {
            var o = new Color32[outPx * outPx];
            float step = (float)crop / outPx;
            for (int v = 0; v < outPx; v++)
                for (int u = 0; u < outPx; u++)
                {
                    int sx = (int)(cx - crop * 0.5f + (u + 0.5f) * step), sy = (int)(cy - crop * 0.5f + (v + 0.5f) * step);
                    o[v * outPx + u] = sx < 0 || sy < 0 || sx >= sw || sy >= sh ? new Color32(0, 0, 0, 255) : src[sy * sw + sx];
                }
            var t = new Texture2D(outPx, outPx, TextureFormat.RGB24, false);
            try
            {
                t.SetPixels32(o);
                t.Apply(false);
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + name + ".png"), t.EncodeToPNG());
            }
            finally { UnityEngine.Object.Destroy(t); }
        }

        /// <summary>The visor of `kind` drawn this frame nearest `near` (Visor47Probe's bounds): its height and width on screen (px at 1080p), or -1.</summary>
        static Vector2 VisorPx48(Camera cam, string kind, Vector3 near)
        {
            float best = float.MaxValue;
            Vector2 r = new Vector2(-1, -1);
            foreach (var (k, b) in Visor47Probe.Seen)
            {
                if (k != kind) continue;
                float d = (b.center - near).sqrMagnitude;
                if (d >= best) continue;
                float lx = float.MaxValue, hx = float.MinValue, ly = float.MaxValue, hy = float.MinValue;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 s = cam.WorldToScreenPoint(c);
                    if (s.z <= 0) continue;
                    lx = Mathf.Min(lx, s.x); hx = Mathf.Max(hx, s.x); ly = Mathf.Min(ly, s.y); hy = Mathf.Max(hy, s.y);
                }
                if (hy < ly) continue;
                best = d;
                float k1080 = 1080f / Mathf.Max(1, Screen.height);
                r = new Vector2((hy - ly) * k1080, (hx - lx) * k1080);
            }
            return r;
        }

        static IEnumerator Lineup48Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            bool? vWas = Visor47.Force, f47Was = Foes47.Force, f48Was = Foes48.Force;
            var rigGo = new GameObject("lineup48 rig");
            Lineup48Rig rig = rigGo.AddComponent<Lineup48Rig>();
            var lineup = new List<Husk>();
            offs.Add(() =>
            {
                Visor47.Force = vWas;
                Foes47.Force = f47Was;
                Foes48.Force = f48Was;
                Foes48.Probe = false;
                Visor47Probe.Recording = false;
                foreach (Husk h in lineup) if (h != null) h.alive = false;
                if (rig != null) rig.Release();
                if (rigGo != null) UnityEngine.Object.Destroy(rigGo);
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
            });
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED a948: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 420;
            while ((sim.campaign == null || sim.campaign.room < 0 || sim.campaign.phase == Campaign24.Phase.None) && Time.realtimeSinceStartup < wallEnd - 400)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput); // the player's own input back (no bot walking him); nothing is pressed
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            for (int i = 0; i < 30; i++) { God(g); yield return null; }
            string pc = Preconditions(g, g.Feet);
            float fov0 = g.cam != null ? g.cam.fieldOfView : 60f; // the game's own field of view standing still (CameraFeel's base)
            Start(g, "a948-lineup48", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();

            // ---- the stage: over the roof the feet see (or open sky), along the clearest heading ----
            Vec3 feet = g.Feet;
            double up = g.world.RayIntersect(new Vec3(feet.x, feet.y + 1, feet.z), new Vec3(0, 1, 0), out RayHit roof)
                ? Math.Min(UnityTuning.Lineup48StageMaxM, roof.distance + 1 + UnityTuning.Lineup48StageAboveM)
                : UnityTuning.Lineup48StageMaxM * 0.5;
            var S = new Vec3(feet.x, feet.y + up, feet.z);
            double bestClear = -1, camYaw = 0;
            for (int i = 0; i < 8; i++)
            {
                double yaw = i * Math.PI / 4;
                var dv = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
                double clear = double.MaxValue; // the shortest of the lines toward the camera and behind the body, at three heights
                foreach (double y in new[] { 0.3, UnityTuning.Lineup48EyeM, 2.6 })
                {
                    var o = new Vec3(S.x, S.y + y, S.z);
                    double c1 = g.world.RayIntersect(o, dv, out RayHit h1) ? h1.distance : 99;
                    double c2 = g.world.RayIntersect(o, new Vec3(-dv.x, 0, -dv.z), out RayHit h2) ? h2.distance : 99;
                    clear = Math.Min(clear, Math.Min(c1, c2));
                }
                if (clear > bestClear) { bestClear = clear; camYaw = yaw; }
            }
            var camDir = new Vec3(-Math.Sin(camYaw), 0, -Math.Cos(camYaw)); // web: from the body toward the camera
            double faceYaw = Math.Atan2(-camDir.x, -camDir.z); // a body with this yaw faces the camera
            Vector3 Su = WebSpace.ToUnity(S);
            Vector3 camDirU = WebSpace.ToUnity(new Vec3(S.x + camDir.x, S.y, S.z + camDir.z)) - Su;
            camDirU.y = 0;
            camDirU.Normalize();
            rig.cam = g.cam;
            rig.vmCam = g.viewmodel != null ? g.viewmodel.Camera : null;
            rig.fov = fov0;
            rig.camPos = Su + camDirU * 10 + Vector3.up * UnityTuning.Lineup48EyeM;
            rig.camRot = Quaternion.LookRotation(-camDirU, Vector3.up);
            rig.active = true;
            Material neon = g.mats != null ? g.mats.neon : null;
            rig.AddPlate(neon, Su + Vector3.down * 0.05f, Quaternion.LookRotation(camDirU, Vector3.up), new Vector3(UnityTuning.Lineup48FloorW, 0.1f, UnityTuning.Lineup48FloorW), UnityTuning.Lineup48PlateGrey);
            rig.AddPlate(neon, Su - camDirU * UnityTuning.Lineup48WallBehindM + Vector3.up * (UnityTuning.Lineup48WallH * 0.5f - 2f), Quaternion.LookRotation(camDirU, Vector3.up),
                new Vector3(UnityTuning.Lineup48WallW, UnityTuning.Lineup48WallH, 0.2f), UnityTuning.Lineup48WallGrey);
            lines.Add($"stage {up.ToString("0.0", ci)} m over the feet (roof {(roof.distance > 0 ? roof.distance.ToString("0.0", ci) + " m" : "none")}), heading {(camYaw * 180 / Math.PI).ToString("0", ci)} deg (clear {Math.Min(bestClear, 99).ToString("0.0", ci)} m), fov {fov0.ToString("0.0", ci)}, frame {Screen.width}x{Screen.height}");
            for (int i = 0; i < 6; i++) { rig.Apply(); yield return null; }

            Visor47Probe.Recording = true;
            float k1080 = Screen.height / 1080f;
            int crop1 = Mathf.Max(32, Mathf.RoundToInt(UnityTuning.Lineup48Cell * k1080)), cropClose = Mathf.Min(Screen.height, Mathf.RoundToInt(UnityTuning.Lineup48CloseCrop * k1080));
            int cropPair = Mathf.RoundToInt(640 * k1080);
            float teleFov = 2f * Mathf.Atan(Mathf.Tan(fov0 * 0.5f * Mathf.Deg2Rad) * UnityTuning.Lineup48TeleAsM / UnityTuning.Lineup48Distances[2]) * Mathf.Rad2Deg; // 20 m drawn as big as at 5 m
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            int hudSeen = 0, vmSeen = 0, grabs = 0;
            string[] angles = { "front", "side", "45" };
            double[] angleOff = { 0, Math.PI / 2, Math.PI / 4 };
            string[] kinds = { "husk", "tank", "swarmer", "spitter", "charger", "warden", "drone", "lancer" };
            var keys = new System.Text.StringBuilder("pairs L48-pair-<kind>-<close|10m|20m>-<P|Q> (old = OFF, new = ON; the moments: close = 3 m at 45 deg, 10m = 10 m at 45 deg (the warden: front, its shield), 20m = 20 m at 45 deg, the true pixels magnified nearest-neighbour to 640; frozen, colour; for the lead after the votes):");
            var spawnedKinds = new List<string>();
            var shieldLines = new System.Text.StringBuilder("G12 shield probe (clear m to the body's drawn capsule / verts inside / arm corners out the front / centre x m / centre y share / top y m):");
            int shieldBadOn = 0, shieldPosesOn = 0;

            // the pose: the camera `d` m from the body along the heading, at the eye's height, looking at the body's middle
            Vec3 Aim(Husk h) => new Vec3(S.x, h.position.y + Math.Max(0.35, h.Height * 0.55), S.z);
            void PoseCam(Husk h, double d)
            {
                Vector3 eye = Su + camDirU * (float)d + Vector3.up * UnityTuning.Lineup48EyeM;
                Vector3 at = WebSpace.ToUnity(Aim(h));
                rig.camPos = eye;
                rig.camRot = Quaternion.LookRotation(at - eye, Vector3.up);
                rig.Apply();
            }
            void Hold(Husk h, Vec3 at, double yaw)
            {
                h.position = at;
                h.previous = at;
                h.yaw = yaw;
                h.hp = h.maxHp = 1e9;
                h.alive = true;
            }

            foreach (string kind in kinds)
            {
                if (Time.realtimeSinceStartup > wallEnd) { lines.Add($"{kind}: the wall clock ran out"); break; }
                Husk h = null;
                try { h = sim.husks.Spawn(kind, S, new Vec3(S.x + camDir.x * 10, S.y, S.z + camDir.z * 10)); } catch (Exception) { }
                if (h == null) { lines.Add($"{kind}: did not spawn (its pool off or full)"); continue; }
                lineup.Add(h);
                spawnedKinds.Add(kind);
                Vec3 at = kind == "drone" ? new Vec3(S.x, S.y + UnityTuning.Lineup48DroneLiftM, S.z) : S;
                Hold(h, at, faceYaw);
                string probeKind = kind == "tank" ? "husk" : kind; // the tank draws the husk's view (its visor parts note as "husk")
                var sheet = new Sheet48(UnityTuning.Lineup48Cell);
                var zoom = new Sheet48(UnityTuning.Lineup48Cell); // round 2: rows 10 / 20 / 30 m (true pixels, magnified) and 20 m TELE
                var silPx20 = new System.Text.StringBuilder($"{kind} silhouette at 20 m (its larger side, px at 1080p):");
                Sheet48 poses = kind == "warden" ? new Sheet48(UnityTuning.Lineup48Cell) : null;
                var vs = new System.Text.StringBuilder($"{kind} visor h x w px (front):");
                uint kh = 0x48A9u;
                foreach (char ch in kind) kh = kh * 31u + ch; // a fixed hash (string.GetHashCode may differ per run)
                bool coin = new Mulberry32(Seed ^ kh).Next() < 0.5; // P = old (OFF) when true
                for (int pass = 0; pass < 2; pass++)
                {
                    bool on = pass == 1;
                    Visor47.Force = on;
                    Foes47.Force = on;
                    Foes48.Force = on;
                    Foes48.Probe = kind == "warden";
                    vs.Append(on ? " ON" : " off");
                    for (int di = 0; di < UnityTuning.Lineup48Distances.Length; di++)
                    {
                        double d = UnityTuning.Lineup48Distances[di];
                        for (int ai = 0; ai < 3; ai++)
                        {
                            Hold(h, at, faceYaw + angleOff[ai]);
                            PoseCam(h, d);
                            for (int f = 0; f < UnityTuning.Lineup48SettleFrames; f++) { God(g); Hold(h, at, faceYaw + angleOff[ai]); PoseCam(h, d); yield return null; }
                            string full = (di == 1 && ai == 2) ? $"L48-{kind}-{(on ? "on" : "off")}-10m-45" : (di == 2 && ai == 0) ? $"L48-{kind}-{(on ? "on" : "off")}-20m-front" : null;
                            yield return Grab48(dir, frame, size, full);
                            grabs++;
                            if (rig.canvasesOn > 0) hudSeen++;
                            if (rig.vmOn) vmSeen++;
                            if (frame[0] == null) continue;
                            Vector3 sp = g.cam.WorldToScreenPoint(WebSpace.ToUnity(Aim(h)));
                            sheet.Put(pass * 3 + ai, di, frame[0], size[0], size[1], sp.x, sp.y, di == 0 ? cropClose : crop1);
                            if (di >= 1)
                            {
                                // round 2 (the lead's n48-r1 read: "the 20 m swarmer is a ~10 px dot"): the ZOOM sheet: the same frame's pixels round the
                                // kind's whole silhouette, magnified to the cell nearest-neighbour (the true 10 / 20 / 30 m read, made big enough to judge)
                                Vector3 sc = Sil48(g.cam, h, kind);
                                zoom.Put(pass * 3 + ai, di - 1, frame[0], size[0], size[1], sc.x, sc.y, Mathf.Max(12, Mathf.RoundToInt(sc.z)));
                                if (di == 2) silPx20.Append($" {(on ? "ON" : "off")} {angles[ai]} {(sc.z / UnityTuning.Lineup48ZoomPad).ToString("0", ci)} px;");
                                if (di == 2 && ai == 2) CropUp48(dir, $"L48-pair-{kind}-20m-{((on ^ coin) ? "P" : "Q")}", frame[0], size[0], size[1], sc.x, sc.y, Mathf.Max(12, Mathf.RoundToInt(sc.z)), 640);
                            }
                            if (ai == 0 && di >= 1)
                            {
                                Vector3 head = WebSpace.ToUnity(new Vec3(h.position.x, h.position.y + h.Height * 0.8, h.position.z));
                                Vector2 vp = VisorPx48(g.cam, probeKind, head);
                                vs.Append($" {d.ToString("0", ci)} m {(vp.x < 0 ? "-" : vp.x.ToString("0.0", ci) + " x " + vp.y.ToString("0.0", ci))};");
                            }
                            // the blind pair's crops (3 m 45 deg = "close"; 10 m 45 deg = "10m", the warden 10 m front: its shield)
                            bool pairClose = di == 0 && ai == 2, pair10 = di == 1 && (kind == "warden" ? ai == 0 : ai == 2);
                            if (pairClose || pair10)
                            {
                                string tag = (on ^ coin) ? "P" : "Q"; // coin true: OFF -> P
                                Crop48(dir, $"L48-pair-{kind}-{(pairClose ? "close" : "10m")}-{tag}", frame[0], size[0], size[1], sp.x, sp.y, pairClose ? Mathf.Min(size[1], cropClose) : cropPair);
                            }
                            if (kind == "warden" && di <= 1)
                            {
                                shieldLines.Append($" {(on ? "ON" : "off")} {d.ToString("0", ci)} m {angles[ai]} [{Foes48.ShieldMode}] {Foes48.ShieldClearM.ToString("0.00", ci)} / {Foes48.ShieldInside} / {Foes48.ArmOutFront} / {Foes48.ShieldCentreX.ToString("0.00", ci)} / {Foes48.ShieldCentreYShare.ToString("0.00", ci)} / {Foes48.ShieldTopY.ToString("0.00", ci)};");
                                if (on) { shieldPosesOn++; if (Foes48.ShieldInside > 0 || Foes48.ArmOutFront > 0) shieldBadOn++; }
                                poses.Put(ai, di * 2 + pass, frame[0], size[0], size[1], sp.x, sp.y, di == 0 ? cropClose : crop1);
                            }
                            if (di == 2)
                            {
                                // and the TELE row (last: it takes a new frame): the same 20 m stand-off through a narrowed field of view (the size the 5 m
                                // view gives: the silhouette's parts in more pixels than a player at 20 m gets; labelled as such)
                                rig.fov = teleFov;
                                for (int f = 0; f < 4; f++) { God(g); Hold(h, at, faceYaw + angleOff[ai]); PoseCam(h, d); yield return null; }
                                yield return Grab48(dir, frame, size, ai == 0 ? $"L48-{kind}-{(on ? "on" : "off")}-20m-tele" : null);
                                grabs++;
                                rig.fov = fov0;
                                if (frame[0] != null)
                                {
                                    Vector3 st = Sil48(g.cam, h, kind);
                                    zoom.Put(pass * 3 + ai, 3, frame[0], size[0], size[1], st.x, st.y, Mathf.Max(12, Mathf.RoundToInt(st.z)));
                                }
                                for (int f = 0; f < 2; f++) { God(g); PoseCam(h, d); yield return null; }
                            }
                        }
                    }
                    // the warden's BLOCKING (a block's kick held: blockFlash kept while frozen) and DOWN (the shield lowered) poses at 3 and 10 m
                    if (kind == "warden" && h is Warden w)
                    {
                        string[] wp = { "block", "down-front", "down-45" };
                        for (int pi = 0; pi < 3; pi++)
                            for (int di = 0; di < 2; di++)
                            {
                                double d = UnityTuning.Lineup48Distances[di];
                                double yaw = faceYaw + (pi == 2 ? Math.PI / 4 : 0);
                                for (int f = 0; f < UnityTuning.Lineup48SettleFrames * 2; f++)
                                {
                                    God(g);
                                    Hold(h, at, yaw);
                                    w.blockFlash = pi == 0 ? 1.0 : 0;
                                    w.shieldDown = pi >= 1 ? 5.0 : 0;
                                    w.state = pi >= 1 ? "stunned" : "idle";
                                    PoseCam(h, d);
                                    yield return null;
                                }
                                yield return Grab48(dir, frame, size, null);
                                grabs++;
                                if (frame[0] == null) continue;
                                Vector3 sp = g.cam.WorldToScreenPoint(WebSpace.ToUnity(Aim(h)));
                                poses.Put(3 + pi, di * 2 + pass, frame[0], size[0], size[1], sp.x, sp.y, di == 0 ? cropClose : crop1);
                                shieldLines.Append($" {(on ? "ON" : "off")} {d.ToString("0", ci)} m {wp[pi]} [{Foes48.ShieldMode}] {Foes48.ShieldClearM.ToString("0.00", ci)} / {Foes48.ShieldInside} / {Foes48.ArmOutFront} / {Foes48.ShieldCentreX.ToString("0.00", ci)} / {Foes48.ShieldCentreYShare.ToString("0.00", ci)} / {Foes48.ShieldTopY.ToString("0.00", ci)};");
                                if (on) { shieldPosesOn++; if (Foes48.ShieldInside > 0 || Foes48.ArmOutFront > 0) shieldBadOn++; }
                            }
                        w.blockFlash = 0;
                        w.shieldDown = 0;
                        w.state = "idle";
                    }
                }
                sheet.Write(dir, $"L48-sheet-{kind}", true);
                zoom.Write(dir, $"L48-zoom-{kind}", true);
                lines.Add(silPx20.ToString());
                poses?.Write(dir, "L48-sheet-warden-poses", true);
                keys.Append($" {kind}: P = {(coin ? "OFF (old)" : "ON (new)")};");
                lines.Add(vs.ToString());

                // the swarmer's 2 s gait strip: walked side-on across the plate (a real walk: the view's gait reads its drawn motion)
                if (kind == "swarmer")
                {
                    var gait = new Sheet48(UnityTuning.Lineup48Cell);
                    var poseSeen = new System.Text.StringBuilder();
                    var walk = new Vec3(camDir.z, 0, -camDir.x); // across the camera's view
                    double walkYaw = Math.Atan2(-walk.x, -walk.z);
                    double half = UnityTuning.Lineup48GaitMps * UnityTuning.Lineup48GaitS * 0.5;
                    for (int pass = 0; pass < 2; pass++)
                    {
                        bool on = pass == 1;
                        Visor47.Force = Foes47.Force = Foes48.Force = on;
                        double t = 0;
                        int cellN = 0;
                        double nextShot = 0;
                        Vec3 P(double tt) => new Vec3(S.x + walk.x * (-half + UnityTuning.Lineup48GaitMps * tt), S.y, S.z + walk.z * (-half + UnityTuning.Lineup48GaitMps * tt));
                        // the camera stays put at 4 m, looking at the walk's middle
                        Vector3 eye = Su + camDirU * UnityTuning.Lineup48GaitCamM + Vector3.up * 1.0f;
                        rig.camPos = eye;
                        rig.camRot = Quaternion.LookRotation(Su + Vector3.up * 0.35f - eye, Vector3.up);
                        h.position = P(0);
                        h.previous = h.position;
                        for (int f = 0; f < 10; f++) { God(g); rig.Apply(); yield return null; }
                        while (cellN < UnityTuning.Lineup48GaitFrames && Time.realtimeSinceStartup < wallEnd)
                        {
                            God(g);
                            Vec3 was = h.position;
                            h.previous = was;
                            h.position = P(t);
                            h.yaw = walkYaw;
                            rig.Apply();
                            yield return null;
                            t += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f); // a capture's stall is not walked through
                            if (t >= nextShot)
                            {
                                nextShot += UnityTuning.Lineup48GaitS / UnityTuning.Lineup48GaitFrames;
                                yield return Grab48(dir, frame, size, null);
                                grabs++;
                                if (frame[0] == null) { cellN++; continue; }
                                Vector3 sp = g.cam.WorldToScreenPoint(WebSpace.ToUnity(new Vec3(h.position.x, h.position.y + 0.35, h.position.z)));
                                gait.Put(cellN % 6, pass * 2 + cellN / 6, frame[0], size[0], size[1], sp.x, sp.y, crop1);
                                if (on) poseSeen.Append(SwarmerView.Gait48Pose.ToString(ci)).Append(' ');
                                cellN++;
                            }
                        }
                    }
                    gait.Write(dir, "L48-sheet-swarmer-gait", false);
                    // round 2: the ONE-CYCLE strip (the 2 s strip samples every 1/6 s, the gait cycles 6.4 times a second at 3.2 m/s: its cells aliased to
                    // poses 0 0 0 0 1 2 3 3 3 5 5 5): the body stepped Legs48StrideM / 12 a frame, one frame per cell, so the 12 cells walk one stride
                    var cyc = new Sheet48(UnityTuning.Lineup48Cell);
                    var cycSeen = new System.Text.StringBuilder();
                    for (int pass = 0; pass < 2; pass++)
                    {
                        bool on = pass == 1;
                        Visor47.Force = Foes47.Force = Foes48.Force = on;
                        Vector3 eye = Su + camDirU * UnityTuning.Lineup48GaitCamM + Vector3.up * 1.0f;
                        rig.camPos = eye;
                        rig.camRot = Quaternion.LookRotation(Su + Vector3.up * 0.35f - eye, Vector3.up);
                        h.position = new Vec3(S.x - walk.x * UnityTuning.Legs48StrideM * 0.5, S.y, S.z - walk.z * UnityTuning.Legs48StrideM * 0.5);
                        h.previous = h.position;
                        h.yaw = walkYaw;
                        for (int f = 0; f < 40; f++) { God(g); rig.Apply(); yield return null; } // the gait eases to rest
                        for (int i = 0; i < 12 && Time.realtimeSinceStartup < wallEnd; i++)
                        {
                            God(g);
                            h.previous = h.position;
                            h.position = new Vec3(h.position.x + walk.x * UnityTuning.Legs48StrideM / 12, S.y, h.position.z + walk.z * UnityTuning.Legs48StrideM / 12);
                            h.yaw = walkYaw;
                            rig.Apply();
                            yield return null; // exactly one drawn frame per step
                            yield return Grab48(dir, frame, size, null);
                            grabs++;
                            if (frame[0] == null) continue;
                            Vector3 sp = g.cam.WorldToScreenPoint(WebSpace.ToUnity(new Vec3(h.position.x, h.position.y + 0.35, h.position.z)));
                            cyc.Put(i % 6, pass * 2 + i / 6, frame[0], size[0], size[1], sp.x, sp.y, crop1);
                            if (on) cycSeen.Append(SwarmerView.Gait48Pose.ToString(ci)).Append(' ');
                        }
                    }
                    cyc.Write(dir, "L48-sheet-swarmer-cycle", false);
                    lines.Add($"swarmer one-cycle strip (shot-N30-L48-sheet-swarmer-cycle.png: one {UnityTuning.Legs48StrideM.ToString("0.0", ci)} m stride in 12 cells, 4 m side-on; rows OFF 1-6, OFF 7-12, ON 1-6, ON 7-12): the ON cells' gait pose {cycSeen}(want 1 1 2 3 3 4 5 5 6 7 7 0 from a rest at 0, or the same + 4 from a rest at 4: every pose of the cycle in order)");
                    lines.Add($"swarmer gait strip: {UnityTuning.Lineup48GaitFrames} cells a pass over {UnityTuning.Lineup48GaitS.ToString("0.0", ci)} s at {UnityTuning.Lineup48GaitMps.ToString("0.0", ci)} m/s, 4 m side-on (rows OFF 1-6, OFF 7-12, ON 1-6, ON 7-12); the ON cells' gait pose (Mesh43.Legs48; 0 and 4 = at rest): {poseSeen}");
                }
                h.alive = false;
                for (int f = 0; f < 3; f++) yield return null;
            }
            Visor47Probe.Recording = false;
            Foes48.Probe = false;
            rig.Release();
            yield return Stop();
            lines.Add(shieldLines + $" || G12 ON poses with a vertex inside the body or the arm out the front: {shieldBadOn} of {shieldPosesOn}");
            lines.Add(keys.ToString());
            bool ok = spawnedKinds.Count >= 6 && hudSeen == 0 && vmSeen == 0 && grabs > 0;
            res[0] = $"{(ok ? "GREEN" : "RED")} a948 lineup48 | level {sim.campaignLevel.id} | kinds {string.Join(", ", spawnedKinds)} | captures {grabs}, with a HUD canvas drawn {hudSeen}, with the viewmodel camera on {vmSeen} | sim input {(sim.input != null ? sim.input.GetType().Name : "null")} || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
