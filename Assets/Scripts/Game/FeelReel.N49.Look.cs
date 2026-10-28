// NIGHT49 builder V: the a349 phase (design.n49.look49, G2-G4 environment: the one grade and the colour script). Not run by its author (no Player in
// a worktree): the lead runs it, one Player per level (L-046, L-062):
//   reel.sh + -skygrindMap <a campaign level, e.g. n47-foundry / n47-iceline / ... > + -skygrindReelOnly a349
// The CAMPAIGN on the booted map, DOORFREE, God, the window at 1920 x 1080, the HUD and the viewmodel as in play, the game's own camera. The same
// five frames, each taken twice at the same pose: look49 forced OFF (PostFx.Look49Force = false: look33's grade, NIGHT48's look) then ON (true):
//   room    the level's first fight room from its spawn, facing in (pitch -0.04 rad)
//   floor   the same stand, looking down 0.45 rad (floors against walls)
//   arena   the largest non-boss, non-link room from its spawn, along its clearest heading
//   boss    the boss room from its spawn, facing in (none: the last room)
//   combat  the first fight room: three husks frozen at 8 / 12 / 16 m, the player FIRING (the reel's own fire, sim.UseInput(g.playerInput);
//           fires counted from the "fire" event: 0 = NO READING), the frame 10 frames into the burst
// Each frame: shot-N49-V-<level>-<spot>-<off|on>.png (full frame) and its look meter numbers (Tools/night/n49-lookmeter.py's definitions, HUD bands
// top 12 % / bottom 18 % cut, every 2nd pixel): V, dark / mid / bright %, saturated %, cyan %, hue bins; plus the frame's magenta count (r, b >= 200,
// g <= 60: a missing material) and, in the HUD bands only, the pixels within 3 of the usable cyan 0x19D3E6 (the HUD outside the grade: the count
// should not fall with look49 on). The canvases' render modes are listed (Screen Space - Overlay = drawn after URP's post: outside the grade).
// The combat frame ON is checked against the targets (hypotheses): dark >= 35 %, mid <= 55 %, saturated 10-30 %, hue bins 3-5; the line says
// MEASURED (the lead writes GREEN after looking at the frames and the blind sheet), RED when the run, a frame, the fire or look49's apply failed.
// NIGHT50 builder V (G3, look49 per level; the a349 body only):
// - THE GATE: the line names the level id, the row, the list (design.n49.look49.numbers.levels) and the config's verdict; after the five spots
//   the force is cleared (PostFx.Look49Force = null) and the line prints whether the grade applied: RED when that differs from the config's
//   verdict (the per-level gate does not work). The off / on passes still force (a force wins over the list: frames on an unlisted level).
// - THE GRAYSCALE AUDIT (NIGHT50.md G3: "most readability failures are value failures"): on the combat frame, off and on (the same pose), the
//   Rec. 709 luma (Y' = 0.2126 R' + 0.7152 G' + 0.0722 B' of the sRGB frame, every 6th pixel each way, the HUD bands and the viewmodel's
//   screen box cut) averaged in four masks: ENEMY (a placed husk's own ray hit nearer than the world's), USABLE (the meter's cyan test above),
//   WALL (the world collider's hit with |normal.y| <= 0.35) and FLOOR (normal.y >= 0.7, below the eye); the gaps enemy - wall,
//   (enemy + usable) - wall, usable - wall and enemy - floor. Logged, not gated (round 1). shot-N50-V-<level>-combat-<off|on>-gray.png (the luma
//   frame) and -mask.png (the masks at 1/6 size: enemy in the danger token, usable in the usable token, wall white, floor grey) beside the frames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N49Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = LookV49Scenario(g, dir, res, offs, pre);

        /// <summary>One frame's meter (n49-lookmeter.py's definitions). [0] V, [1] dark %, [2] mid %, [3] bright %, [4] saturated %, [5] cyan %,
        /// [6] hue bins, [7] magenta pixels (whole frame), [8] HUD-band pixels at the usable cyan (+-3).</summary>
        static double[] MeterV49(Color32[] px, int w, int h)
        {
            var m = new double[9];
            int y0 = (int)(h * 0.18), y1 = (int)(h * 0.88); // GetPixels32 rows are bottom-up: the PNG's top 12 % is y >= 0.88 h, its bottom 18 % y < 0.18 h
            long n = 0, dark = 0, bright = 0, sat = 0, cyan = 0, magenta = 0, hudCyan = 0;
            double vSum = 0;
            var bins = new long[12];
            const int ur = 0x19, ug = 0xD3, ub = 0xE6; // UnityTuning.Look49UsableHex's channels
            for (int y = 0; y < h; y += 2)
            {
                bool world = y >= y0 && y < y1;
                for (int x = 0; x < w; x += 2)
                {
                    Color32 p = px[y * w + x];
                    if (p.r >= 200 && p.b >= 200 && p.g <= 60) magenta++;
                    if (!world)
                    {
                        if (Math.Abs(p.r - ur) <= 3 && Math.Abs(p.g - ug) <= 3 && Math.Abs(p.b - ub) <= 3) hudCyan++;
                        continue;
                    }
                    float r = p.r / 255f, gg = p.g / 255f, b = p.b / 255f;
                    float mx = Math.Max(r, Math.Max(gg, b)), mn = Math.Min(r, Math.Min(gg, b));
                    float v = mx, s = mx > 0 ? (mx - mn) / mx : 0, d = mx - mn + 1e-6f;
                    float hue = mx == r ? ((gg - b) / d % 6 + 6) % 6 : mx == gg ? (b - r) / d + 2 : (r - gg) / d + 4;
                    hue *= 60;
                    n++;
                    vSum += v;
                    if (v < 0.25f) dark++;
                    else if (v > 0.75f) bright++;
                    if (s > 0.5f && v > 0.3f)
                    {
                        sat++;
                        bins[Math.Min(11, (int)(hue / 30))]++;
                    }
                    if (hue >= 170 && hue <= 200 && s > 0.35f && v > 0.35f) cyan++;
                }
            }
            double k = n > 0 ? 100.0 / n : 0;
            m[0] = n > 0 ? vSum / n : -1;
            m[1] = dark * k;
            m[3] = bright * k;
            m[2] = n > 0 ? 100 - m[1] - m[3] : -1;
            m[4] = sat * k;
            m[5] = cyan * k;
            int spread = 0;
            foreach (long c in bins) if (c >= 0.03 * Math.Max(1, sat)) spread++;
            m[6] = spread;
            m[7] = magenta * 4; // every 2nd pixel each way: x 4 for the frame's count
            m[8] = hudCyan * 4;
            return m;
        }

        static string MeterV49Text(double[] m, CultureInfo ci) =>
            $"V {m[0].ToString("0.00", ci)} | dark {m[1].ToString("0", ci)} % mid {m[2].ToString("0", ci)} % bright {m[3].ToString("0", ci)} % | saturated {m[4].ToString("0", ci)} % | cyan {m[5].ToString("0.0", ci)} % | hue bins {m[6].ToString("0", ci)} | magenta {m[7].ToString("0", ci)} px | HUD cyan {m[8].ToString("0", ci)} px";

        static IEnumerator LookV49Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why + " (boot reel.sh + -skygrindMap <a campaign level> + -skygrindReelOnly a349)"; yield break; }
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            bool? forceWas = PostFx.Look49Force;
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            for (int i = 0; i < 60 && Screen.height != 1080; i++) yield return null;
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var placed = new List<Husk>();
            offs.Add(() =>
            {
                PostFx.Look49Force = forceWas;
                foreach (Husk h in placed) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedMove = null;
                g.forcedCombat = IdleCombat;
            });
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED a349: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 240;
            while ((sim.campaign == null || sim.campaign.room < 0 || sim.campaign.phase == Campaign24.Phase.None) && Time.realtimeSinceStartup < wallEnd - 200)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            int fires = 0;
            offs.Add(g.events.On("fire", o => fires++));
            LevelDef lv = sim.campaignLevel;
            string lvId = lv.id ?? "level";
            PostFx fx = g.lightAndLens != null ? g.lightAndLens.post : null;
            Start(g, "a349-look49", "camera", dir);
            yield return Frames(pre);

            // the canvases: Screen Space - Overlay is drawn after URP's post (outside the grade)
            int ov = 0, camSpace = 0, worldSpace = 0;
            foreach (Canvas c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (!c.isRootCanvas || !c.isActiveAndEnabled) continue;
                if (c.renderMode == RenderMode.ScreenSpaceOverlay) ov++;
                else if (c.renderMode == RenderMode.ScreenSpaceCamera) camSpace++;
                else worldSpace++;
            }
            var lines = new List<string> { $"level {lvId} (theme {LightPlan33.Theme(g.map.json)}), frame {Screen.width}x{Screen.height}, the HUD and the viewmodel as in play; root canvases: overlay {ov}, screen-space camera {camSpace} (graded: wants 0), world {worldSpace}" };
            // NIGHT50 V: the per-level gate as the config states it (the force aside)
            int theme50 = LightPlan33.Theme(g.map.json);
            string mapLevel50 = PostFx.LevelIdOf(g.map.json);
            string[] levels50 = PostFx.Look49LevelsOf(g.cfg);
            string[] levels52 = Look52Lift.GradeLevelsOf(g.cfg); // NIGHT52 U look52: its own grade list, read only while look52 is on (Look52.cs)
            bool row50 = NightRows.ViewOn(g.cfg, "look49"), listed50 = PostFx.Look49Listed(levels50, mapLevel50) || PostFx.Look49Listed(levels52, mapLevel50);
            bool gateWant50 = row50 && listed50 && theme50 >= 0;
            string gateHead50 = $"gate: map level {mapLevel50}, row {(row50 ? "on" : "off")}, levels [{string.Join(", ", levels50)}], look52 grade levels [{string.Join(", ", levels52)}] (wall lift x{(PostFx.V50Slot(theme50) >= 0 && levels52.Length > 0 ? Look52Lift.LiftFor(g.cfg, PostFx.V50Slot(theme50)).ToString("0.00", CultureInfo.InvariantCulture) : "1")}; applied x{Look52Lift.Lift.ToString("0.00", CultureInfo.InvariantCulture)}), listed {(listed50 ? "yes" : "no")}, V50 values {(PostFx.V50Slot(theme50) >= 0 ? "yes" : "no (V49's row)")}: config says {(gateWant50 ? "ON" : "off")}";
            lines.Add(gateHead50);
            double[] grayOff = null, grayOn = null;

            RoomDef first = null, arena = null, boss = null;
            double best = -1;
            foreach (RoomDef r in lv.rooms)
            {
                if (r.kind == "boss") { boss ??= r; continue; }
                if (r.kind == "link") continue;
                if (first == null && r.waves.Count > 0) first = r;
                double area = Math.Abs((r.x1 - r.x0) * (r.z1 - r.z0));
                if (area > best) { best = area; arena = r; }
            }
            first ??= lv.rooms[0];
            arena ??= first;
            boss ??= lv.rooms[lv.rooms.Count - 1];

            bool failed = false, applyFailed = false;
            double[] combatOn = null;
            var tier52Lines = new List<string>(); // NIGHT52 U look52 round 8: the tier audit's lines
            double[] tier52Pool = null; // round 9: the same-facing buckets pooled over the frames (FeelReel.N52.Tier.cs)
            var frame = new Color32[1][];
            int[] size = new int[2];
            var spots = new List<(string name, RoomDef room, double pitch, bool clearest, bool combat)>
            {
                ("room", first, -0.04, false, false),
                ("floor", first, -0.45, false, false),
                ("arena", arena, -0.04, true, false),
                ("boss", boss, -0.04, false, false),
                ("combat", first, 0, false, true),
            };
            foreach (var (name, room, pitch, clearest, combat) in spots)
            {
                if (Time.realtimeSinceStartup > wallEnd) { lines.Add(name + ": out of time"); failed = true; break; }
                g.husks.frozen = false;
                g.husks.KillAll();
                foreach (Husk h in placed) if (h != null) h.alive = false;
                placed.Clear();
                g.Respawn(room.spawn, room.spawnYawDeg);
                yield return Frames(4);
                double yaw = room.spawnYawDeg * Math.PI / 180;
                if (clearest) { List<double> ys = Yaws(g); if (ys.Count > 0) yaw = ys[0]; }
                g.input.SetLook(yaw, pitch);
                g.husks.frozen = true;
                Vec3 aim = default;
                if (combat)
                {
                    foreach (var (d, lat) in new[] { (12.0, 0.0), (8.0, -2.0), (16.0, 2.0) })
                    {
                        Husk h = PlaceAt(g, d, lat, out double used);
                        if (h == null) continue;
                        h.hp = h.maxHp = 1e9; // never dies: the same three in both passes
                        h.previous = h.position;
                        placed.Add(h);
                        if (placed.Count == 1) aim = h.AimPoint;
                    }
                    if (placed.Count == 0) { lines.Add("combat: no husk placed (no floor / clear line)"); failed = true; continue; }
                }
                for (int i = 0; i < 40; i++) { God(g); if (combat) Look(g, aim); yield return null; }
                foreach (bool on in new[] { false, true })
                {
                    PostFx.Look49Force = on;
                    for (int i = 0; i < 12; i++) { God(g); if (combat) Look(g, aim); yield return null; } // the theme re-applied, the Volume refreshed (Via Scripting)
                    int fires0 = fires;
                    if (combat)
                    {
                        g.forcedCombat = Fire;
                        for (int i = 0; i < 10; i++) { God(g); Look(g, aim); yield return null; }
                    }
                    string tag = $"N49-V-{lvId}-{name}-{(on ? "on" : "off")}";
                    double[][] gray = combat ? new double[1][] : null; // NIGHT50 V: the grayscale audit, taken at the frame's end with the capture
                    yield return GrabV49(dir, frame, size, tag, g, placed, gray, $"N50-V-{lvId}-combat-{(on ? "on" : "off")}");
                    if (combat && gray[0] != null)
                    {
                        if (on) grayOn = gray[0]; else grayOff = gray[0];
                        lines.Add($"combat {(on ? "ON" : "off")} gray: {GrayV50Text(gray[0], ci)}");
                    }
                    else if (combat) lines.Add($"combat {(on ? "ON" : "off")} gray: no audit (no frame)");
                    if (combat)
                    {
                        g.forcedCombat = IdleCombat;
                        for (int i = 0; i < 30; i++) { God(g); Look(g, aim); yield return null; } // the burst's effects fade before the next pass
                    }
                    if (frame[0] == null) { lines.Add(tag + ": no frame"); failed = true; continue; }
                    double[] m = MeterV49(frame[0], size[0], size[1]);
                    bool applied = fx != null && fx.Look49Applied;
                    if (on && !applied) applyFailed = true;
                    if (m[7] > 0) failed = true;
                    string fireNote = combat ? $", fires {fires - fires0}{(fires - fires0 == 0 ? " (NO READING)" : "")}" : "";
                    if (combat && fires - fires0 == 0) failed = true;
                    lines.Add($"{name} {(on ? "ON" : "off")} (look49 applied {applied}{fireNote}): {MeterV49Text(m, ci)}");
                    if (on && !combat && name != "boss") // NIGHT52 U look52 round 8 (FeelReel.N52.Tier.cs): the tiers' value separation on the room / floor / arena frames
                    {
                        double[] ta = Tier52Audit(g, frame[0], size[0], size[1]);
                        tier52Lines.Add($"{name} ON: {Tier52Text(ta, ci)}");
                        tier52Pool = Tier52Pool(tier52Pool, ta);
                    }
                    if (combat && on) combatOn = m;
                    frame[0] = null;
                }
            }
            // NIGHT50 V: the gate itself: no force, the config decides; applied must equal the config's verdict for this level
            bool gateFailed = false;
            {
                PostFx.Look49Force = null;
                for (int i = 0; i < 12; i++) { God(g); yield return null; } // the theme re-applied with the force cleared
                bool appliedNoForce = fx != null && fx.Look49Applied;
                gateFailed = fx == null || appliedNoForce != gateWant50 || fx.Look49Gate != (row50 && listed50) || fx.Look49Level != mapLevel50;
                lines.Add($"gate (no force): PostFx level {(fx != null ? fx.Look49Level : "?")}, PostFx gate {(fx != null && fx.Look49Gate)}, applied {appliedNoForce}; config says {(gateWant50 ? "ON" : "off")}{(gateFailed ? " (MISMATCH)" : "")}");
            }
            PostFx.Look49Force = forceWas;
            yield return Stop();
            string grayHead = "gray (combat): no audit";
            if (grayOff != null && grayOn != null)
                grayHead = $"gray (combat) off -> ON: enemy-wall {GapV50(grayOff, 0, 4, ci)} -> {GapV50(grayOn, 0, 4, ci)}, (enemy+usable)-wall {GapV50(grayOff, 8, 4, ci)} -> {GapV50(grayOn, 8, 4, ci)}, usable-wall {GapV50(grayOff, 2, 4, ci)} -> {GapV50(grayOn, 2, 4, ci)}, enemy-floor {GapV50(grayOff, 0, 6, ci)} -> {GapV50(grayOn, 0, 6, ci)} (logged, not gated)";
            string targets = "combat ON: no frame";
            if (combatOn != null)
            {
                bool dk = combatOn[1] >= 35, md = combatOn[2] <= 55, st = combatOn[4] >= 10 && combatOn[4] <= 30, hb = combatOn[6] >= 3 && combatOn[6] <= 5;
                int met = (dk ? 1 : 0) + (md ? 1 : 0) + (st ? 1 : 0) + (hb ? 1 : 0);
                targets = $"combat ON vs the targets (hypotheses): dark >= 35 {(dk ? "yes" : "no")}, mid <= 55 {(md ? "yes" : "no")}, saturated 10-30 {(st ? "yes" : "no")}, hue bins 3-5 {(hb ? "yes" : "no")}: {met}/4";
            }
            string head = failed || applyFailed || gateFailed ? "RED a349" : "MEASURED a349";
            string why2 = applyFailed ? " (look49 did not apply on an ON pass: look33 off, or the level's name matches no theme)" : "";
            if (gateFailed) why2 += " (the per-level gate: applied with no force differs from the config's verdict)";
            // NIGHT52 U look52 round 8: the tiers (Look52Tier on spire / foundry); round 9: GREEN when applied and the ledge face clears walls facing the
            // same way by Look52ReelTierGap, pooled over the frames (the floor is NIGHT51's: its gaps printed, not judged)
            string tier52Pooled = Tier52Pooled(tier52Pool, ci, out bool tier52Ok);
            // round 11 (the lead, n52-r15: spire left UnityTuning.Look52TierLevels): a level look52 does not tier is "n/a", not RED; look52 off is the RED side
            string tier52Verdict = Look52Tier.Applied ? (tier52Ok ? "GREEN" : "RED") : Look52.On(g.cfg) ? "n/a (not a tier level: NIGHT51's stone)" : "RED (look52 off: NIGHT51's stone)";
            string tier52Head = $"tier52 (look52 tiers applied {Look52Tier.Applied}; want the ledge face >= {UnityTuning.Look52ReelTierGap.ToString("0.00", ci)} over walls facing the same way): {tier52Verdict} {tier52Pooled} [{string.Join("; ", tier52Lines)}]";
            res[0] = $"{head} look49 (G2-G4 environment; NIGHT50 G3 per level){why2} | {gateHead50} | {targets} | {tier52Head} | {grayHead} || {string.Join(" || ", lines)}";
        }

        /// <summary>NIGHT50 V: one gap of the audit (mean a - mean b, indices into GrayAuditV50's array), "n/a" when either mask is empty.</summary>
        static string GapV50(double[] a, int i, int j, CultureInfo ci) =>
            a[i + 1] > 0 && a[j + 1] > 0 ? (a[i] - a[j]).ToString("+0.000;-0.000;0.000", ci) : "n/a";

        static string GrayV50Text(double[] a, CultureInfo ci)
        {
            string M(int i) => a[i + 1] > 0 ? $"{a[i].ToString("0.000", ci)} (n {a[i + 1].ToString("0", ci)})" : "none (n 0)";
            return $"Y' enemy {M(0)} | usable {M(2)} | wall {M(4)} | floor {M(6)} | enemy+usable {M(8)} | gap enemy-wall {GapV50(a, 0, 4, ci)}, (enemy+usable)-wall {GapV50(a, 8, 4, ci)}, usable-wall {GapV50(a, 2, 4, ci)}, enemy-floor {GapV50(a, 0, 6, ci)}";
        }

        /// <summary>NIGHT50 V: the grayscale audit of one frame (pixels bottom-up, as GetPixels32; the camera as it drew this frame: call it at the
        /// frame's end). Returns [0] enemy mean Y', [1] its sample count, [2] / [3] usable, [4] / [5] wall, [6] / [7] floor, [8] / [9] enemy + usable.
        /// Writes the luma frame and the mask beside the shots.</summary>
        static double[] GrayAuditV50(SurvivalGame g, Color32[] px, int w, int h, List<Husk> husks, string dir, string name)
        {
            const int step = 6; // every 6th pixel each way (about 40 k rays on a 1920 x 1080 frame with the HUD bands cut)
            const double floorNy = 0.7, wallNy = 0.35; // |normal.y| at or over 0.7: a floor (below the eye); at or under 0.35: a wall
            var sum = new double[10];
            Camera cam = g.cam;
            if (cam == null || px == null) return null;
            // the viewmodel's screen boxes (its renderers' bounds through the overlay camera): cut, the collider rays cannot see the gun
            var vm = new List<Rect>();
            Camera vc = g.viewmodel != null ? g.viewmodel.Camera : null;
            if (vc != null)
                foreach (Renderer r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    if (!r.enabled || !r.gameObject.activeInHierarchy || r.gameObject.layer != Viewmodel.Layer) continue;
                    Bounds b = r.bounds;
                    float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
                    for (int c = 0; c < 8; c++)
                    {
                        Vector3 corner = new Vector3((c & 1) == 0 ? b.min.x : b.max.x, (c & 2) == 0 ? b.min.y : b.max.y, (c & 4) == 0 ? b.min.z : b.max.z);
                        Vector3 sp = vc.WorldToScreenPoint(corner);
                        if (sp.z <= 0) continue;
                        x0 = Math.Min(x0, sp.x); y0 = Math.Min(y0, sp.y); x1 = Math.Max(x1, sp.x); y1 = Math.Max(y1, sp.y);
                    }
                    if (x1 > x0 && y1 > y0) vm.Add(Rect.MinMaxRect(x0, y0, x1, y1));
                }
            int hudLo = (int)(h * 0.18), hudHi = (int)(h * 0.88); // MeterV49's HUD bands (bottom-up rows)
            int mw = w / step, mh = h / step;
            var mask = new Color32[mw * mh];
            Color32 enemyC = Skygrind.Game.Look.Hex(UnityTuning.Look49DangerHex), usableC = Skygrind.Game.Look.Hex(UnityTuning.Look49UsableHex); // FeelReel has its own Look(): the class by its full name
            Color32 wallC = new Color32(230, 230, 230, 255), floorC = new Color32(120, 120, 120, 255), otherC = new Color32(40, 40, 40, 255), cutC = new Color32(0, 0, 0, 255);
            for (int my = 0; my < mh; my++)
            for (int mx = 0; mx < mw; mx++)
            {
                int x = mx * step + step / 2, y = my * step + step / 2;
                Color32 p = px[y * w + x];
                Color32 mc = otherC;
                if (y < hudLo || y >= hudHi) mc = cutC;
                else
                {
                    bool inVm = false;
                    foreach (Rect rc in vm) if (rc.Contains(new Vector2(x + 0.5f, y + 0.5f))) { inVm = true; break; }
                    if (inVm) mc = cutC;
                    else
                    {
                        float r = p.r / 255f, gg = p.g / 255f, bb = p.b / 255f;
                        double luma = 0.2126 * r + 0.7152 * gg + 0.0722 * bb;
                        Ray ray = cam.ScreenPointToRay(new Vector3(x + 0.5f, y + 0.5f, 0));
                        Vec3 o = WebSpace.FromUnity(ray.origin), d = WebSpace.FromUnity(ray.direction);
                        bool hitWorld = g.world.RayIntersect(o, d, out RayHit hit);
                        double dw = hitWorld ? hit.distance : double.PositiveInfinity, de = double.PositiveInfinity;
                        foreach (Husk hk in husks)
                        {
                            if (hk == null || !hk.alive) continue;
                            double t = hk.RayHit(o, d, out _);
                            if (t >= 0 && t < de) de = t;
                        }
                        // the meter's cyan test (MeterV49: hue 170-200, s > 0.35, v > 0.35)
                        float mxc = Math.Max(r, Math.Max(gg, bb)), mnc = Math.Min(r, Math.Min(gg, bb)), dd = mxc - mnc + 1e-6f;
                        float s = mxc > 0 ? (mxc - mnc) / mxc : 0;
                        float hue = (mxc == r ? ((gg - bb) / dd % 6 + 6) % 6 : mxc == gg ? (bb - r) / dd + 2 : (r - gg) / dd + 4) * 60;
                        bool cyan = hue >= 170 && hue <= 200 && s > 0.35f && mxc > 0.35f;
                        int k = -1;
                        if (de < dw) k = 0;
                        else if (cyan) k = 2;
                        else if (hitWorld)
                        {
                            double ny = Math.Abs(hit.normal.y);
                            if (ny <= wallNy) k = 4;
                            else if (ny >= floorNy && d.y < 0) k = 6;
                        }
                        if (k >= 0)
                        {
                            sum[k] += luma;
                            sum[k + 1]++;
                            if (k <= 2) { sum[8] += luma; sum[9]++; }
                            mc = k == 0 ? enemyC : k == 2 ? usableC : k == 4 ? wallC : floorC;
                        }
                    }
                }
                mask[my * mw + mx] = mc;
            }
            for (int k = 0; k < 10; k += 2) if (sum[k + 1] > 0) sum[k] /= sum[k + 1];
            // the luma frame and the mask, beside the shots
            Texture2D gt = null, mt = null;
            try
            {
                var gp = new Color32[w * h];
                for (int i = 0; i < gp.Length; i++)
                {
                    Color32 p = px[i];
                    byte yv = (byte)Math.Round(Math.Min(255.0, 0.2126 * p.r + 0.7152 * p.g + 0.0722 * p.b));
                    gp[i] = new Color32(yv, yv, yv, 255);
                }
                gt = new Texture2D(w, h, TextureFormat.RGBA32, false);
                gt.SetPixels32(gp);
                gt.Apply(false);
                mt = new Texture2D(mw, mh, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                mt.SetPixels32(mask);
                mt.Apply(false);
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-" + name + "-gray.png"), gt.EncodeToPNG());
                File.WriteAllBytes(Path.Combine(dir, "shot-" + name + "-mask.png"), mt.EncodeToPNG());
            }
            finally
            {
                if (gt != null) UnityEngine.Object.Destroy(gt);
                if (mt != null) UnityEngine.Object.Destroy(mt);
            }
            return sum;
        }

        /// <summary>The frame at its end, with the HUD (ScreenCapture after the overlay canvases): pixels into into[0] (bottom-up), size w / h; saved as
        /// shot-&lt;name&gt;.png in the reel dir. NIGHT50 V: with gray (one slot), the grayscale audit of the same frame at the same end of frame (the
        /// camera and the husks as drawn) into gray[0], its images named after grayName.</summary>
        static IEnumerator GrabV49(string dir, Color32[][] into, int[] size, string name, SurvivalGame g = null, List<Husk> husks = null, double[][] gray = null, string grayName = null)
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
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-" + name + ".png"), t.EncodeToPNG());
                if (gray != null && g != null)
                {
                    try { gray[0] = GrayAuditV50(g, into[0], size[0], size[1], husks ?? new List<Husk>(), dir, grayName ?? name); }
                    catch (Exception e) { Debug.LogWarning("a349 grayscale audit failed: " + e.Message); gray[0] = null; } // the frame and its meter stand without it
                }
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }
    }
}
