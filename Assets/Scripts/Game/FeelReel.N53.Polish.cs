// NIGHT53 builder U: the a153 phase (FeelReel.N53.cs routes it here): design.n53.polish53's proof (Polish53.cs; NIGHT53.md section 4).
// Any campaign level:
//   bash Tools/night/n47-reels.sh <tag> u1on,n52-foundry,a153                   (the build copy's config with design.n53.polish53 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> u1off,n52-foundry,a153,-skygrindOff53   (the row off: RED, NIGHT52's look)
// The CAMPAIGN on the booted map (L-062), DOORFREE, God, enemies frozen, the player's input back and still. Each item prints that its input
// happened (L-089) before its number; res[0] holds every item's short line; the full lines go to the Player log as "[a153] <item>: ...".
// Item 1 (T-122, the first shot; FirstShot53): run BEFORE the reel's recorder starts (its every-frame screen grab allocates 8 MB a frame and
// queues JPG encodes: the a352 long frames came while it ran, the off side as long as the on side), with no grab in the window: the run's first
// KESTREL shot and then the first THUNDERCLAP shot, each after Polish53ReelMedianFrames still frames (their median is the run's frame); the
// shot's frame and the Polish53ReelShotFrames after it timed by the wall clock at each frame; the worst against max(K x median, median + floor);
// the muzzle light's lifetime (LightPools.Guns52.cs) judged as a352 does (guns52 live: 40 ms of real time to within 2.5 frames + 5 ms).
// GREEN needs FirstShot53 to have run at boot (RED off by construction: -skygrindOff53 / the item's on 0 leave it unrun), both shots fired,
// both worst frames under the limit, both lights in their window. Frames: shot-N30-P53-first-<gun>-before.png / -after.png (grabbed outside
// the timed window: before the still frames and after the timed ones, written once the recorder starts).
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
        static partial void N53Polish(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Polish53Scenario(g, dir, res, offs, pre);

        static readonly CultureInfo Ci53 = CultureInfo.InvariantCulture;

        /// <summary>One item's result: its short line for res[0], its full line for the log, GREEN or not.</summary>
        sealed class Item53
        {
            public string id, shortLine, line;
            public bool green, na; // na: not judged on this map (iceline's grade on another level)
        }

        /// <summary>A frame grabbed in memory (written later: the recorder's Start empties the folder).</summary>
        sealed class Held53
        {
            public string name;
            public Color32[] px;
            public int w, h;
        }

        static void Write53(string dir, Held53 f)
        {
            if (f == null || f.px == null) return;
            var t = new Texture2D(f.w, f.h, TextureFormat.RGBA32, false);
            try
            {
                t.SetPixels32(f.px);
                t.Apply(false);
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, "shot-N30-" + f.name + ".png"), t.EncodeToPNG());
            }
            finally { UnityEngine.Object.Destroy(t); }
        }

        static double Median53(List<double> xs)
        {
            if (xs.Count == 0) return double.NaN;
            var s = new List<double>(xs);
            s.Sort();
            return s.Count % 2 == 1 ? s[s.Count / 2] : 0.5 * (s[s.Count / 2 - 1] + s[s.Count / 2]);
        }

        static IEnumerator Polish53Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var spawned = new List<Husk>();
            offs.Add(() =>
            {
                Polish53.Force = null;
                KillPop53.Suppress = false;
                foreach (Husk h in spawned) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a153: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 480;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 460) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput); // the player's own input back (no bot); nothing pressed but the reel's
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            sim.husks.KillAll(); // the room's own wave out of the frame (silently: no kill events)
            bool on = Polish53.On(g.cfg);
            var items = new List<Item53>();
            var held = new List<Held53>();
            int[] fires = new int[1];
            int[] fireFrame = { -1 };
            string[] fireGun = { null };
            offs.Add(g.events.On("fire", o => { fires[0]++; fireFrame[0] = Time.frameCount; fireGun[0] = o as string; }));

            // ---- item 1 (T-122): the first shots, before the recorder ----
            var it1 = new Item53 { id = "T-122" };
            yield return FirstShot53Item(g, it1, held, fires, fireFrame, fireGun, wallEnd);
            items.Add(it1);

            string pc = Preconditions(g, g.Feet);
            Start(g, "a153-polish53", "camera", dir);
            foreach (Held53 f in held) Write53(dir, f);
            held.Clear();
            yield return Frames(pre);

            // ---- item 2 (T-111): the kill's wash, OFF (Polish53.Force false: NIGHT52's chunks) then ON (the config) in this Player ----
            var it2 = new Item53 { id = "T-111" };
            KillPop53.Suppress = true; // the pop's HUD shards out of the wash box's measure (item 5 judges the pop)
            yield return KillWash53Item(g, dir, it2, spawned, wallEnd);
            KillPop53.Suppress = false;
            items.Add(it2);

            // ---- item 3 (T-120): the scorch core, a pair on one mark ----
            var it3 = new Item53 { id = "T-120" };
            if (Time.realtimeSinceStartup < wallEnd) yield return Scorch53Item(g, dir, it3, spawned, wallEnd);
            else { it3.line = "the scorch core: the wall clock ran out"; it3.shortLine = "not run"; }
            items.Add(it3);

            // ---- item 4 (T-116): iceline's grade (n/a on another map) ----
            var it4 = new Item53 { id = "T-116" };
            yield return IcelineGrade53Item(g, dir, it4);
            items.Add(it4);

            // ---- item 5: the kill pop (OFF then ON in this Player) ----
            var it5 = new Item53 { id = "POP" };
            if (Time.realtimeSinceStartup < wallEnd) yield return KillPop53Item(g, dir, it5, spawned, wallEnd);
            else { it5.line = "the kill pop: the wall clock ran out"; it5.shortLine = "not run"; }
            items.Add(it5);

            yield return Frames(5);
            yield return Stop();
            int greens = 0, judged = 0;
            var shorts = new List<string>();
            foreach (Item53 it in items)
            {
                string v = it.na ? "n/a" : it.green ? "GREEN" : "RED";
                if (!it.na) judged++;
                if (!it.na && it.green) greens++;
                shorts.Add($"{it.id} {v}: {it.shortLine}");
                Debug.Log($"[a153] {it.id}: {v} {it.line}");
            }
            bool ok = on && greens == judged && judged > 0;
            res[0] = $"{(ok ? "GREEN" : "RED")} a153 polish53 | on {on}, items {greens}/{judged} judged (map {PostFx.LevelIdOf(g.map.json)}) || {string.Join(" || ", shorts)} | {pc}";
        }

        /// <summary>Item 2 (T-111): per side (OFF: Polish53.Force false, NIGHT52's chunks with K's cap; ON: the config) a husk at Skin52ReelWashM
        /// on the longest clear line, killed by one KESTREL shot; every frame grabbed from before the kill to Skin52ReelWashFrames after it;
        /// NIGHT50's WASH box (Wash49) per frame, its rise over f-2; frames S53-wash-&lt;side&gt;-f0..f8-box (a252's crops) and S53-wash-&lt;side&gt;-f4.
        /// GREEN: both kills made, ON's peak within Wash49Rise (+0.25) and ON's f3-f4 under OFF's.</summary>
        static IEnumerator KillWash53Item(SurvivalGame g, string dir, Item53 it, List<Husk> spawned, float wallEnd)
        {
            SurvivalSim sim = g.sim;
            var full = new List<string>();
            var brief = new List<string>();
            // the longest clear line through the feet (8 headings, at 0.5 m and the eye)
            Vec3 feet = g.Feet;
            double best = -1, bestYaw = 0;
            for (int i = 0; i < 8; i++)
            {
                double yaw = i * Math.PI / 4;
                var dv = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
                double ahead = double.MaxValue;
                foreach (double y in new[] { 0.5, 1.6 })
                {
                    var o = new Vec3(feet.x, feet.y + y, feet.z);
                    ahead = Math.Min(ahead, g.world.RayIntersect(o, dv, out RayHit h) ? h.distance : 60);
                }
                if (ahead > best) { best = ahead; bestYaw = yaw; }
            }
            g.Respawn(feet, bestYaw * 180 / Math.PI);
            for (int i = 0; i < 30; i++) { God(g); yield return null; }
            feet = g.Feet;
            Vector3 fwdU = g.cam.transform.forward;
            fwdU.y = 0;
            fwdU = fwdU.sqrMagnitude > 1e-6f ? fwdU.normalized : Vector3.forward;
            Vec3 fwd = WebSpace.FromUnity(fwdU);
            yield return Equip27(g, "revolver"); // the KESTREL
            g.forcedCombat = IdleCombat;
            var peaks = new Dictionary<string, float>();
            var mids = new Dictionary<string, float>();
            var grid = new float[64 * 36];
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            int wi = 0;
            bool liveOn = false;
            foreach (string side in new[] { "off", "on" })
            {
                if (Time.realtimeSinceStartup > wallEnd) { full.Add($"{side}: the wall clock ran out"); break; }
                Polish53.Force = side == "off" ? false : (bool?)null;
                bool live = Polish53.Item(g.cfg, "killWash");
                if (side == "on") liveOn = live;
                double ang = (wi++ == 0 ? -1 : 1) * UnityTuning.Skin52ReelWashTurnDeg * Math.PI / 180;
                var hd = new Vec3(fwd.x * Math.Cos(ang) + fwd.z * Math.Sin(ang), 0, -fwd.x * Math.Sin(ang) + fwd.z * Math.Cos(ang));
                g.Respawn(feet, Math.Atan2(-hd.x, -hd.z) * 180 / Math.PI);
                sim.husks.KillAll();
                for (int i = 0; i < 30; i++) { God(g); yield return null; }
                double dW = Math.Min(UnityTuning.Skin52ReelWashM, best - 1.0);
                var spot = new Vec3(feet.x + hd.x * dW, feet.y, feet.z + hd.z * dW);
                Husk hs = null;
                try { hs = sim.husks.Spawn("husk", spot, feet); } catch (Exception) { }
                if (hs == null) { full.Add($"{side}: a kill happened: NO (the husk did not spawn)"); brief.Add($"{side} no husk"); continue; }
                spawned.Add(hs);
                double face = Math.Atan2(hd.x, hd.z);
                for (int i = 0; i < 45; i++) { hs.position = hs.previous = spot; hs.yaw = face; hs.hp = hs.maxHp = 1; hs.alive = true; God(g); yield return null; }
                int capped0 = KillWash53.Capped, rings0 = KillWash53.RingsSkipped; // EXT53 B: the kill ring's frames left out of the wash box
                int pops71 = Wash71.KillPopsClocked; // NIGHT71 L4: kill pops wash71 clocked on frame time
                string wash71 = Wash71.Live(g.cfg) ? "live" : "off (" + (Polish53.On(g.cfg) ? Wash71.OffReason(g.cfg, Environment.GetCommandLineArgs()) : "polish53 off") + ")";
                var wash = new List<float>();
                int f0 = -1, killFrame = -1;
                for (int i = 0; i < 150; i++)
                {
                    int k = f0 >= 0 ? i - f0 : -1;
                    if (f0 >= 0 && k > UnityTuning.Skin52ReelWashFrames) break;
                    // one press two frames into the capture (again every 30 frames if it missed)
                    g.forcedCombat = f0 < 0 && i % 30 == 2 ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                    if (hs.alive) { hs.position = hs.previous = spot; hs.yaw = face; }
                    God(g);
                    string shot = k == 4 ? $"S53-wash-{side}-f4" : null; // the f3-f4 peak's frame, whole
                    yield return Grab48(dir, frame, size, shot);
                    wash.Add(frame[0] != null ? Wash49(frame[0], size[0], size[1], grid) : float.NaN);
                    if (f0 >= 0 && k >= 0 && k <= 8 && frame[0] != null)
                        CropUp48(dir, $"S53-wash-{side}-f{k}-box", frame[0], size[0], size[1], size[0] * 0.5f, size[1] * 0.5f, (int)(size[1] * 0.4f), 432);
                    if (f0 < 0 && !hs.alive) { f0 = i; killFrame = Time.frameCount; } // the first frame drawn with the husk dead (its kill's frame 0)
                }
                g.forcedCombat = IdleCombat;
                if (f0 < 2) { full.Add($"{side}: a kill happened: NO (the shot missed in 150 frames)"); brief.Add($"{side} no kill"); hs.alive = false; continue; }
                float w0 = wash[f0 - 2];
                float peak = float.MinValue, mid = float.MinValue;
                int peakK = -1;
                var curve = new System.Text.StringBuilder();
                for (int k = 0; f0 + k < wash.Count; k++)
                {
                    float r = wash[f0 + k] - w0;
                    curve.Append($"{(k == 0 ? "" : " ")}f{k} {r.ToString("+0.000;-0.000;0.000", Ci53)}");
                    if (r > peak) { peak = r; peakK = k; }
                    if (k == 3 || k == 4) mid = Mathf.Max(mid, r);
                }
                peaks[side] = peak;
                mids[side] = mid;
                full.Add($"{side}: a kill happened: yes (a husk at {dW.ToString("0.0", Ci53)} m, one KESTREL shot, its frame 0 = frame {killFrame}; killWash live {live}, white frames set on {KillWash53.Capped - capped0} parts, kill-ring frames left out of the wash box {KillWash53.RingsSkipped - rings0}; wash71 {wash71}, kill pops on frame time {Wash71.KillPopsClocked - pops71}); "
                    + $"f-2 {w0.ToString("0.000", Ci53)}, peak {peak.ToString("+0.000;-0.000;0.000", Ci53)} at f{peakK}, f3-f4 max {mid.ToString("+0.000;-0.000;0.000", Ci53)} [{curve}]");
                brief.Add($"{side} kill yes, peak {peak.ToString("+0.000;-0.000;0.000", Ci53)} f{peakK}, f3-f4 {mid.ToString("+0.000;-0.000;0.000", Ci53)}");
            }
            Polish53.Force = null;
            bool both = peaks.TryGetValue("on", out float pOn) & peaks.TryGetValue("off", out float pOff);
            bool midsBoth = mids.TryGetValue("on", out float mOn) & mids.TryGetValue("off", out float mOff);
            bool under = both && pOn <= Wash49Rise;
            bool lower = midsBoth && mOn < mOff;
            it.green = liveOn && both && midsBoth && under && lower;
            it.line = $"the kill wash (the WASH box's rise over f-2; bar +{Wash49Rise.ToString("0.00", Ci53)}; killWash.whiteFrames {Polish53.Num(g.cfg, "killWash", "whiteFrames", -1)}, silhouetteFrames {Polish53.Num(g.cfg, "killWash", "silhouetteFrames", -1)}, burstK {Polish53.Num(g.cfg, "killWash", "burstK", -1)}, ringInWash {Polish53.Num(g.cfg, "killWash", "ringInWash", -1)}):{string.Join("; ", full)}; "
                + $"ON peak within the bar {under}, ON f3-f4 under OFF {lower}";
            it.shortLine = string.Join(", ", brief) + $" (ON peak <= +{Wash49Rise.ToString("0.00", Ci53)} {under}, f3-f4 lower {lower})";
        }

        /// <summary>The mean luma (Rec. 709 on the sRGB values, 0..1) of a px box (centre, half side) in a grabbed frame (bottom-up rows).</summary>
        static float BoxLuma53(Color32[] px, int w, int h, Vector2 c, float half)
        {
            if (px == null) return float.NaN;
            int x0 = Mathf.Clamp((int)(c.x - half), 0, w - 1), x1 = Mathf.Clamp((int)(c.x + half), 0, w - 1);
            int y0 = Mathf.Clamp((int)(c.y - half), 0, h - 1), y1 = Mathf.Clamp((int)(c.y + half), 0, h - 1);
            double s = 0;
            int n = 0;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    Color32 p = px[y * w + x];
                    s += (0.2126 * p.r + 0.7152 * p.g + 0.0722 * p.b) / 255.0;
                    n++;
                }
            return n > 0 ? (float)(s / n) : float.NaN;
        }

        /// <summary>Item 3 (T-120): one husk killed on fresh floor (Skin52ReelScorchTurnDeg x 2 off the line); the bare floor's core box before the
        /// husk stands there; when the mark is Skin52ReelScorchAgeS old a PAIR in one pose a frame apart: polish53 forced off (NIGHT52's mark: no
        /// core, scorchGlow 0) then on (scorchCore.glow). Frames shot-N30-P53-scorch-&lt;floor|off|on&gt;.png (+ -crop). GREEN: scorchCore live, a
        /// kill, the core box ON over OFF by Skin52ReelScorchMin and over the bare floor.</summary>
        static IEnumerator Scorch53Item(SurvivalGame g, string dir, Item53 it, List<Husk> spawned, float wallEnd)
        {
            SurvivalSim sim = g.sim;
            Polish53.Force = null;
            float glow = Scorch53.Glow(g.cfg);
            bool live = glow > 0 && Skin52.Live;
            Vec3 feet = g.Feet;
            Vector3 fwdU = g.cam.transform.forward;
            fwdU.y = 0;
            fwdU = fwdU.sqrMagnitude > 1e-6f ? fwdU.normalized : Vector3.forward;
            Vec3 fwd = WebSpace.FromUnity(fwdU);
            // fresh floor: the clearer of the two headings 2 x Skin52ReelScorchTurnDeg either side of the wash's line
            Vec3 hd = fwd;
            double ahead = -1;
            foreach (double sgn in new[] { 1.0, -1.0 })
            {
                double ang = sgn * 2 * UnityTuning.Skin52ReelScorchTurnDeg * Math.PI / 180;
                var d = new Vec3(fwd.x * Math.Cos(ang) + fwd.z * Math.Sin(ang), 0, -fwd.x * Math.Sin(ang) + fwd.z * Math.Cos(ang));
                double a = g.world.RayIntersect(new Vec3(feet.x, feet.y + 0.5, feet.z), d, out RayHit h) ? h.distance : 60;
                if (a > ahead) { ahead = a; hd = d; }
            }
            g.Respawn(feet, Math.Atan2(-hd.x, -hd.z) * 180 / Math.PI);
            sim.husks.KillAll();
            for (int i = 0; i < 40; i++) { God(g); yield return null; }
            double dS = Math.Min(UnityTuning.Skin52ReelWashM, ahead - 1.0);
            var spot = new Vec3(feet.x + hd.x * dS, feet.y, feet.z + hd.z * dS);
            Camera cam = g.cam;
            Vector3 su = WebSpace.ToUnity(spot);
            Vector3 sc = cam.WorldToScreenPoint(su);
            float half = Vector2.Distance(sc, cam.WorldToScreenPoint(su + cam.transform.right * UnityTuning.Skin52ReelScorchCoreM));
            var c2 = new Vector2(sc.x, sc.y);
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            yield return Grab48(dir, frame, size, "P53-scorch-floor");
            float floor = BoxLuma53(frame[0], size[0], size[1], c2, half);
            if (frame[0] != null) CropUp48(dir, "P53-scorch-floor-crop", frame[0], size[0], size[1], c2.x, c2.y, Mathf.Clamp(Mathf.RoundToInt(half * 8), 32, size[1]), 640);
            Husk hs = null;
            try { hs = sim.husks.Spawn("husk", spot, feet); } catch (Exception) { }
            if (hs == null) { it.line = "the scorch core: a kill happened: NO (the husk did not spawn)"; it.shortLine = "no husk"; yield break; }
            spawned.Add(hs);
            double face = Math.Atan2(hd.x, hd.z);
            for (int i = 0; i < 45; i++) { hs.position = hs.previous = spot; hs.yaw = face; hs.hp = hs.maxHp = 1; hs.alive = true; God(g); yield return null; }
            int killFrame = -1;
            for (int i = 0; i < 150 && killFrame < 0; i++)
            {
                g.forcedCombat = i % 30 == 2 ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                if (hs.alive) { hs.position = hs.previous = spot; hs.yaw = face; }
                God(g);
                yield return null;
                if (!hs.alive) killFrame = Time.frameCount;
            }
            g.forcedCombat = IdleCombat;
            if (killFrame < 0) { hs.alive = false; it.line = "the scorch core: a kill happened: NO (the shot missed in 150 frames)"; it.shortLine = "no kill"; yield break; }
            float tKill = Time.unscaledTime;
            while (Time.unscaledTime - tKill < UnityTuning.Skin52ReelScorchWaitS && !(EnemyLook.Scorch52Age >= UnityTuning.Skin52ReelScorchAgeS && EnemyLook.Scorch52Age < 10f)) { God(g); yield return null; }
            float age = EnemyLook.Scorch52Age;
            // the pair, one pose, a frame apart
            int drawn0 = Scorch53.Drawn;
            Polish53.Force = false;
            for (int i = 0; i < 2; i++) { God(g); yield return null; }
            yield return Grab48(dir, frame, size, "P53-scorch-off");
            float lumOff = BoxLuma53(frame[0], size[0], size[1], c2, half);
            if (frame[0] != null) CropUp48(dir, "P53-scorch-off-crop", frame[0], size[0], size[1], c2.x, c2.y, Mathf.Clamp(Mathf.RoundToInt(half * 8), 32, size[1]), 640);
            Polish53.Force = null;
            for (int i = 0; i < 2; i++) { God(g); yield return null; }
            yield return Grab48(dir, frame, size, "P53-scorch-on");
            float lumOn = BoxLuma53(frame[0], size[0], size[1], c2, half);
            if (frame[0] != null) CropUp48(dir, "P53-scorch-on-crop", frame[0], size[0], size[1], c2.x, c2.y, Mathf.Clamp(Mathf.RoundToInt(half * 8), 32, size[1]), 640);
            Color core = EnemyLook.Scorch52Core;
            bool drewCore = Scorch53.Drawn > drawn0;
            float rise = lumOn - lumOff, overFloor = lumOn - floor;
            bool hot = rise >= UnityTuning.Skin52ReelScorchMin && overFloor > 0;
            it.green = live && drewCore && hot;
            string F(float v) => v.ToString("0.000", Ci53);
            it.line = $"the scorch core: a kill happened: yes (a husk at {dS.ToString("0.0", Ci53)} m, frame {killFrame}); the mark {age.ToString("0.00", Ci53)} s old; scorchCore live {live} (glow {glow.ToString("0.0", Ci53)}, skin52 live {Skin52.Live}), "
                + $"drawn with it {(drewCore ? "yes" : "NO")} (core linear {core.r.ToString("0.00", Ci53)} / {core.g.ToString("0.00", Ci53)} / {core.b.ToString("0.00", Ci53)}); the core box ({Mathf.RoundToInt(2 * half * 1080f / Screen.height)} px at 1080p) "
                + $"bare floor {F(floor)}, OFF {F(lumOff)}, ON {F(lumOn)}: ON - OFF {rise.ToString("+0.000;-0.000", Ci53)} (want >= +{UnityTuning.Skin52ReelScorchMin.ToString("0.00", Ci53)}), ON - floor {overFloor.ToString("+0.000;-0.000", Ci53)} (want > 0)";
            it.shortLine = $"kill yes, mark {age.ToString("0.00", Ci53)} s, live {live}, ON-OFF {rise.ToString("+0.000;-0.000", Ci53)}, ON-floor {overFloor.ToString("+0.000;-0.000", Ci53)}";
        }

        /// <summary>Item 4 (T-116): on an iceline map, the grade's gate (look52's list with iceline's ids) and the lift applied on this map
        /// (Look52Lift.Lift, set once per map by PostFx.Globals49) against icelineGrade.lift; a frame where the reel stands with NIGHT49's meter
        /// (MeterV49). GREEN: the lift applied at icelineGrade's value and the frame inside the two bands the lift trades (saturated 10-30 %,
        /// mid <= 55 %; dark and the hue bins printed). The 4/4 is a349's combat frame (the lead's run on n52-iceline). Not iceline: n/a.</summary>
        static IEnumerator IcelineGrade53Item(SurvivalGame g, string dir, Item53 it)
        {
            string level = PostFx.LevelIdOf(g.map.json);
            if (Array.IndexOf(UnityTuning.Polish53IcelineLevels, level) < 0)
            {
                it.na = true;
                it.line = $"iceline's grade: n/a on {level} (run a153 on n52-iceline; a349 there is the 4/4 meter)";
                it.shortLine = "n/a (not iceline)";
                yield break;
            }
            int slot = PostFx.V50Slot(LightPlan33.Theme(g.map.json));
            bool listed = Look52Lift.GradeListed(g.cfg, level);
            float want = IcelineGrade53.Lift(g.cfg, slot), lift = Look52Lift.Lift;
            bool applied = want > 0 && listed && Mathf.Abs(lift - want) < 1e-4f;
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            for (int i = 0; i < 10; i++) { God(g); yield return null; }
            yield return Grab48(dir, frame, size, "P53-iceline-grade");
            double[] m = frame[0] != null ? MeterV49(frame[0], size[0], size[1]) : null;
            bool st = m != null && m[4] >= 10 && m[4] <= 30, md = m != null && m[2] <= 55, dk = m != null && m[1] >= 35, hb = m != null && m[6] >= 3 && m[6] <= 5;
            int met = (st ? 1 : 0) + (md ? 1 : 0) + (dk ? 1 : 0) + (hb ? 1 : 0);
            // n53-r1: this frame is where the reel stands after item 3 (floor-heavy), not a349's combat frame (three husks at 8 / 12 / 16 m, firing),
            // where the look meter's 4/4 is defined (a349 at x1.12: dark 42 / mid 54 / saturated 10- / bins 4 = 3/4; n52-r15). GREEN here: the
            // lift applied (the input) and this frame inside the saturated band the lift exists for; the 4/4 verdict stays a349's.
            it.green = applied && st;
            string meter = m != null ? MeterV49Text(m, Ci53) : "no frame";
            it.line = $"iceline's grade: the map is iceline: yes ({level}, V50 slot {slot}); look52's grade list holds it {(listed ? "yes" : "NO")}; the wall lift applied x{lift.ToString("0.000", Ci53)} "
                + $"(icelineGrade.lift {(want > 0 ? want.ToString("0.000", Ci53) : "off")}): {(applied ? "yes" : "NO")}; the reel's frame (shot-N30-P53-iceline-grade): {meter}; "
                + $"the look meter's 4/4 thresholds (a349's combat frame): dark >= 35 % {(dk ? "yes" : "no")}, mid <= 55 % {(md ? "yes" : "no")}, saturated 10-30 % {(st ? "yes" : "no")}, hue bins 3-5 {(hb ? "yes" : "no")}: {met}/4 on THIS frame "
                + "(not the combat frame: the 4/4 verdict is a349 on n52-iceline; the lift is design.n53.polish53.numbers.icelineGrade.lift, a config number)";
            it.shortLine = $"iceline yes, lift x{lift.ToString("0.000", Ci53)} applied {(applied ? "yes" : "NO")}, this frame {met}/4 (dark {(m != null ? m[1].ToString("0", Ci53) : "?")} >= 35, mid {(m != null ? m[2].ToString("0", Ci53) : "?")} <= 55, sat {(m != null ? m[4].ToString("0.0", Ci53) : "?")} in 10-30, bins {(m != null ? m[6].ToString("0", Ci53) : "?")} in 3-5; 4/4 = a349 combat)";
        }

        /// <summary>Item 5 (the kill pop): per side (OFF: Polish53.Force false, NIGHT52's marker; ON: the config) a husk killed by one KESTREL shot
        /// on the line the reel faces; from the kill's frame KillPop53's values sampled each frame for 30 frames (the fixed reel clock: 1 / 60 s a
        /// frame); frames shot-N30-P53-pop-&lt;side&gt;-k1.png (+ -crop: the reticle's 240 px box at 640 px). GREEN: the pop built and fired on the ON
        /// kill, its punch >= x1.15 within 3 frames and back to x1 (within 0.02) by +20, the shards' alpha >= 0.5 at +1 and gone (<= 0.05) by +30,
        /// no magenta in the box (UIEffect's material in this build); OFF: no pop.</summary>
        static IEnumerator KillPop53Item(SurvivalGame g, string dir, Item53 it, List<Husk> spawned, float wallEnd)
        {
            SurvivalSim sim = g.sim;
            var full = new List<string> { $"built {KillPop53.Built} (UIEffect on the shards {KillPop53.FxOn})" };
            var brief = new List<string>();
            bool onOk = false, offQuiet = false;
            Vec3 feet = g.Feet;
            Vector3 fwdU = g.cam.transform.forward;
            fwdU.y = 0;
            fwdU = fwdU.sqrMagnitude > 1e-6f ? fwdU.normalized : Vector3.forward;
            Vec3 fwd = WebSpace.FromUnity(fwdU);
            double ahead = g.world.RayIntersect(new Vec3(feet.x, feet.y + 0.5, feet.z), fwd, out RayHit hit0) ? hit0.distance : 60;
            double dK = Math.Min(UnityTuning.Skin52ReelWashM + 2.0, ahead - 1.0);
            yield return Equip27(g, "revolver");
            g.forcedCombat = IdleCombat;
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            foreach (string side in new[] { "off", "on" })
            {
                if (Time.realtimeSinceStartup > wallEnd) { full.Add($"{side}: the wall clock ran out"); break; }
                Polish53.Force = side == "off" ? false : (bool?)null;
                sim.husks.KillAll();
                for (int i = 0; i < 20; i++) { God(g); yield return null; }
                var spot = new Vec3(feet.x + fwd.x * dK, feet.y, feet.z + fwd.z * dK);
                Husk hs = null;
                try { hs = sim.husks.Spawn("husk", spot, feet); } catch (Exception) { }
                if (hs == null) { full.Add($"{side}: a kill happened: NO (the husk did not spawn)"); brief.Add($"{side} no husk"); continue; }
                spawned.Add(hs);
                double face = Math.Atan2(fwd.x, fwd.z);
                for (int i = 0; i < 45; i++) { hs.position = hs.previous = spot; hs.yaw = face; hs.hp = hs.maxHp = 1; hs.alive = true; God(g); yield return null; }
                int fired0 = KillPop53.Fired, killFrame = -1;
                for (int i = 0; i < 150 && killFrame < 0; i++)
                {
                    g.forcedCombat = i % 30 == 2 ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                    if (hs.alive) { hs.position = hs.previous = spot; hs.yaw = face; }
                    God(g);
                    yield return null;
                    if (!hs.alive) killFrame = Time.frameCount;
                }
                g.forcedCombat = IdleCombat;
                if (killFrame < 0) { hs.alive = false; full.Add($"{side}: a kill happened: NO (the shot missed in 150 frames)"); brief.Add($"{side} no kill"); continue; }
                bool popped = KillPop53.Fired > fired0;
                var sc = new float[31];
                var al = new float[31];
                var ht = new float[31];
                var ou = new float[31];
                int magenta = 0;
                for (int k = 0; k <= 30; k++)
                {
                    sc[k] = KillPop53.ScaleK;
                    al[k] = KillPop53.ShardAlpha;
                    ht[k] = KillPop53.Heat;
                    ou[k] = KillPop53.ShardOut;
                    if (k == 1)
                    {
                        yield return Grab48(dir, frame, size, $"P53-pop-{side}-k1");
                        if (frame[0] != null)
                        {
                            float cx = size[0] * 0.5f, cy = size[1] * 0.5f, half = 120f * size[1] / 1080f;
                            for (int y = (int)(cy - half); y < (int)(cy + half); y++)
                                for (int x = (int)(cx - half); x < (int)(cx + half); x++)
                                {
                                    Color32 p = frame[0][y * size[0] + x];
                                    if (p.r >= 200 && p.b >= 200 && p.g <= 60) magenta++;
                                }
                            CropUp48(dir, $"P53-pop-{side}-k1-crop", frame[0], size[0], size[1], cx, cy, Mathf.RoundToInt(2 * half), 640);
                        }
                    }
                    else { God(g); yield return null; }
                }
                float peak = Mathf.Max(Mathf.Max(sc[0], sc[1]), Mathf.Max(sc[2], sc[3]));
                string F(float v) => v.ToString("0.00", Ci53);
                string curve = $"scale +0 {F(sc[0])} +1 {F(sc[1])} +2 {F(sc[2])} +4 {F(sc[4])} +8 {F(sc[8])} +20 {F(sc[20])}; shard alpha +1 {F(al[1])} +8 {F(al[8])} +30 {F(al[30])}; out +1 {F(ou[1])} +8 {F(ou[8])}; heat +0 {F(ht[0])} +2 {F(ht[2])} +6 {F(ht[6])}";
                full.Add($"{side}: a kill happened: yes (a husk at {dK.ToString("0.0", Ci53)} m, one KESTREL shot, frame {killFrame}); the pop fired: {(popped ? $"yes (frame {KillPop53.LastFrame})" : "no")}; {curve}; magenta in the reticle box {magenta}");
                if (side == "on")
                {
                    onOk = KillPop53.Built && popped && peak >= 1.15f && Mathf.Abs(sc[20] - 1f) <= 0.02f && al[1] >= 0.5f && al[30] <= 0.05f && magenta == 0;
                    brief.Add($"on kill yes, pop {(popped ? "yes" : "NO")}, peak x{F(peak)}, +20 x{F(sc[20])}, alpha +1 {F(al[1])} +30 {F(al[30])}, magenta {magenta}");
                }
                else
                {
                    offQuiet = !popped;
                    brief.Add($"off kill yes, pop {(popped ? "yes" : "no")}");
                }
            }
            Polish53.Force = null;
            it.green = onOk && offQuiet;
            it.line = "the kill pop: " + string.Join("; ", full);
            it.shortLine = string.Join(", ", brief);
        }

        /// <summary>Item 1 (T-122): the run's first KESTREL shot, then the first THUNDERCLAP shot, timed by the wall clock (see the header).</summary>
        static IEnumerator FirstShot53Item(SurvivalGame g, Item53 it, List<Held53> held, int[] fires, int[] fireFrame, string[] fireGun, float wallEnd)
        {
            bool ran = FirstShot53.Ran;
            string warm = ran ? $"prewarm ran: yes ({FirstShot53.Methods} methods of {FirstShot53.Types} types at boot in {FirstShot53.Ms.ToString("0.0", Ci53)} ms, {FirstShot53.Skipped} skipped{(FirstShot53.OverBudget ? ", budget spent" : "")}; the fx49 kit's sprites built at boot {(FirstShot53.KitPrewarmed ? "yes" : "no")} in {FirstShot53.KitMs.ToString("0.0", Ci53)} ms; fx47's sheets built at the run's first frame {(FirstShot53.Fx47Sheets ? "yes" : "no")} in {FirstShot53.Fx47Ms.ToString("0.0", Ci53)} ms)"
                : "prewarm ran: NO (polish53 off, -skygrindOff53 or firstShot.on 0: NIGHT52's first shot)";
            var full = new List<string> { warm };
            var brief = new List<string> { ran ? $"prewarm yes (fx47 sheets {(FirstShot53.Fx47Sheets ? "yes" : "no")})" : "prewarm NO" }; // EXT53 B: the THUNDERCLAP's own lazy build
            int ok = 0, run = 0;
            bool live52 = Guns52Probe.LiveNow;
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2];
            foreach ((string id, string label) in new[] { ("revolver", "KESTREL"), ("shotgun", "THUNDERCLAP") })
            {
                if (Time.realtimeSinceStartup > wallEnd) { full.Add($"{label}: the wall clock ran out"); break; }
                int firesBefore = fires[0];
                yield return Equip27(g, id);
                g.forcedCombat = IdleCombat;
                for (int i = 0; i < 40; i++) { God(g); yield return null; }
                if (g.loadout.Weapon != id) { full.Add($"{label}: not in hand (held {g.loadout.Weapon})"); brief.Add($"{label} not in hand"); continue; }
                run++;
                yield return Grab48(null, frame, size, null); // the "before" frame, outside the timed window
                held.Add(new Held53 { name = $"P53-first-{label.ToLowerInvariant()}-before", px = frame[0], w = size[0], h = size[1] });
                frame[0] = null;
                GC.Collect(); // the grab's garbage collected now, not inside the window
                for (int i = 0; i < 20; i++) { God(g); yield return null; }
                // the still frames: each frame's wall time from one resume to the next
                var still = new List<double>();
                double t = Time.realtimeSinceStartupAsDouble;
                for (int i = 0; i < UnityTuning.Polish53ReelMedianFrames; i++)
                {
                    God(g);
                    yield return null;
                    double now = Time.realtimeSinceStartupAsDouble;
                    still.Add((now - t) * 1000.0);
                    t = now;
                }
                double median = Median53(still);
                // the shot: a press of one frame and its release (a352's tap)
                int f0 = fires[0], le0 = Guns52Probe.LightEnds;
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } };
                yield return null;
                g.forcedCombat = IdleCombat;
                double tShot = Time.realtimeSinceStartupAsDouble; // the end of the shot's frame's Update
                var shotMs = new List<double>();
                t = tShot;
                for (int i = 0; i <= UnityTuning.Polish53ReelShotFrames; i++)
                {
                    God(g);
                    yield return null;
                    double now = Time.realtimeSinceStartupAsDouble;
                    shotMs.Add((now - t) * 1000.0);
                    t = now;
                }
                for (int i = 0; i < 10; i++) { God(g); yield return null; } // the light's end lands before the "after" grab
                yield return Grab48(null, frame, size, null);
                held.Add(new Held53 { name = $"P53-first-{label.ToLowerInvariant()}-after", px = frame[0], w = size[0], h = size[1] });
                frame[0] = null;
                bool fired = fires[0] > f0 && fireGun[0] == id;
                bool first = firesBefore == 0 || id == "shotgun"; // the KESTREL's is the reel's first shot; the THUNDERCLAP's its first shot
                double worst = 0;
                foreach (double ms in shotMs) worst = Math.Max(worst, ms);
                double limit = Math.Max(UnityTuning.Polish53ReelHitchK * median, median + UnityTuning.Polish53ReelHitchFloorMs);
                bool smooth = worst <= limit;
                bool lightEnded = Guns52Probe.LightEnds > le0;
                double lightMs = lightEnded ? Guns52Probe.LightMs : -1;
                int lightFrames = lightEnded ? Guns52Probe.LightFrames : -1;
                double want = UnityTuning.N51StarS * 1000.0;
                bool lightOk = !live52 || (lightEnded && lightMs >= want - 1.0 && lightMs <= want + 2.5 * median + 5.0);
                string curve = string.Join(" ", shotMs.ConvertAll(ms => ms.ToString("0.0", Ci53)));
                full.Add($"{label}: a shot fired: {(fired ? "yes" : "NO")} ({label}, frame {fireFrame[0]}; fires before this gun {firesBefore}, the gun's first shot {(first ? "yes" : "no")}); "
                    + $"the shot's frames [{curve}] ms, worst {worst.ToString("0.0", Ci53)} ms vs the median {median.ToString("0.0", Ci53)} ms of {still.Count} still frames (limit {limit.ToString("0.0", Ci53)} ms): {(smooth ? "smooth" : "HITCH")}; "
                    + $"muzzle light {lightMs.ToString("0.0", Ci53)} ms over {lightFrames} frames ({(live52 ? $"guns52 live: want {want.ToString("0", Ci53)} ms to {(want + 2.5 * median + 5.0).ToString("0", Ci53)} ms" : "guns52 not live: NIGHT51's frames rule, not judged")}): {(lightOk ? "ok" : "LONG")}");
                brief.Add($"{label} fired {(fired ? "yes" : "NO")} f{fireFrame[0]}, worst {worst.ToString("0.0", Ci53)} / median {median.ToString("0.0", Ci53)} ms ({(smooth ? "smooth" : "HITCH")}), light {lightMs.ToString("0", Ci53)} ms ({(lightOk ? "ok" : "LONG")})");
                if (fired && smooth && lightOk) ok++;
            }
            it.green = ran && run == 2 && ok == 2;
            it.line = "the first shot: " + string.Join("; ", full);
            it.shortLine = string.Join(", ", brief) + $" ({ok}/{run})";
        }
    }
}
