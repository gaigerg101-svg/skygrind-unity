// NIGHT55 builder B: the a155 phase (FeelReel.N55.cs routes it here): design.n55.grade55's proof (BossGrade55.cs, Wick37View.N55.cs). Not run by
// its author (no Player in the worktree). One campaign level a reel (L-062); the row on in the build copy's config (node Tools/night/n46-build-rows.cjs
// on n55.grade55) for GREEN, -skygrindOff55 for RED (NIGHT54's look: both bosses ungraded, the seams red-orange):
//   bash Tools/night/n47-reels.sh <tag> b1on,n52-foundry,a155     (THE WICK)    and  b1off,n52-foundry,a155,-skygrindOff55
//   bash Tools/night/n47-reels.sh <tag> b2on,n52-spire,a155       (THE CROWN)   and  b2off,n52-spire,a155,-skygrindOff55
// The CAMPAIGN on the booted map, DOORFREE, God, the husks killed and frozen; the level's boss room begun with sim.BossRoomBegin and the player held
// at its spawn, the camera on the spawn's yaw. Frames (shot-N55-<boss>-<name>.jpg, 85 % JPG) and beside each its CLASS MASK
// (shot-N55-<boss>-<name>-mask.png: every grid cell coloured by its class: walls blue-grey, floor brown, ceiling teal, slope olive, the boss white,
// sky dark, HUD black, the gun dark grey; danger-tell samples red; the hot wall's patch yellow):
//   before    the level's start, no fight
//   p1        the fight's phase 1, the grade fully in;   p1-raw  two frames later with BossGrade55.Suppress (the ungraded half of the A/B pair)
//   tell      THE WICK: a red tell on the core (the core in view), graded; tell-raw the same, suppressed (THE CROWN's ring is in p1: p1 is its pair)
//   burn      THE WICK: a real hot-wall burn (the player put in the air against the shaft wall at the far side, then back at the spawn) seen from
//             the spawn while it glows;   cold  the same view after the burn faded (no wall burning)
//   turn      a REAL phase change at the turn's peak (THE WICK's bar to P1's gate, as a254; THE CROWN's gems to legGemsLeft[0]: its own leg change)
//   p2        1 s after the turn: phase 2's palette settled (the sim's leg printed: LEG 2)
// THE CLASSES come from the render, not from colour: a ray per grid cell from the camera into the C# world collider (sim.world: wall / floor /
// ceiling / slope by the hit normal; nothing within 500 m: sky) and the enemy registry + the boss's own ray (nearer: the boss's body); the HUD's
// graphics' screen rects and the gun's renderers' rects mask their cells out. Only the danger-tell class is by colour, as the contract defines it
// (HSV hue within proof.tellBandDeg of colors.enemy, S >= proof.tellSat, V >= proof.tellVal). The walls' numbers are the wall cells that are not tells.
// Each class's numbers go to dir/a155-<boss>.txt; the line prints the verdicts and the key numbers.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N55Boss(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            string[] args = Environment.GetCommandLineArgs();
            bool fightOn = Array.IndexOf(args, "-skygrindA155Fight") >= 0, fightOff = Array.IndexOf(args, "-skygrindA155FightOff") >= 0;
            body = fightOn || fightOff ? B55Fight(g, dir, res, offs, pre, fightOff || NightRows.ForceOffFrom <= 55) : B55Scenario(g, dir, res, offs, pre);
        }

        // ---- a155 FIGHT MODE (wick55, slate P1-wickWindow): THE WICK's real fight in the Player ---------------------------------------------------
        //   bash Tools/night/n47-reels.sh <tag> b3on,n52-foundry,a155,-skygrindA155Fight       (wick54 + wick55 forced on)
        //   bash Tools/night/n47-reels.sh <tag> b3off,n52-foundry,a155,-skygrindA155FightOff   (wick54 only: NIGHT54's fight, the RED run)
        // a454's setup (FeelReel.N54.Wick.cs): the campaign on n52-foundry, God, the stack's mouth, Wick37Pilot as the sim's input one pilot step per
        // SIM step (Wick44ProofInput; the harness's seed-1 hand), the KESTREL given as the harness gives it (a454 fought with punches only). To the kill
        // or 150 s of sim time. Frames (shot-N29-<name>.png): W55-p1 (2 s in), W55-open (0.2 s into the first OPEN on the core: the CHARGE's
        // crack-open or the bare core after the RING), W55-punch (the frame of the first core punch landed while the bar read OPEN), W55-kill (0.5 s
        // after). The line: the input (pilot steps, shots, punches), the damage dealt while the bar read OPEN (n47human's measure: N43BossState.open)
        // as a share, core punches inside a window (the frame named), hot-wall burns, the hits taken by source, the kill time. GREEN: the rows on,
        // the input happened, a core punch inside a window on W55-punch, the share >= proof.windowShare, killed. RED otherwise.
        static IEnumerator B55Fight(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, bool off)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            res[0] = "RED: a155 fight began, the fight not reached";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; Quiet(g); g.setup.mode = RunSetup.Survival; });
            NightRowState overWas = sim.rowsOverride;
            NightRowState rs = sim.rowsRegistry.Defaults().Set("wick54", true);
            if (!off) rs = rs.Set("wick55", true);
            sim.rowsOverride = rs;
            offs.Add(() => { g.sim.rowsOverride = overWas; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(240);
            sim = g.sim;
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            DoorFree41(g);
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver"); // the harness's loadout (Harness.N47.Human.cs H47Wick)
            if (!sim.wick37Centre.HasValue || sim.wick37Stack == null) { res[0] = "RED: no stack on this level (boot n52-foundry)"; yield break; }
            Vec3 c = sim.wick37Centre.Value;
            Wick37Stack s = sim.wick37Stack;
            Start(g, "wick55", "camera", dir);
            yield return Frames(pre);
            var st = new double[4];
            int fires = 0, punches = 0, corePunches = 0, punchesOpen = 0, burns0 = sim.wick37HotBurns;
            bool openNow = false;
            Action offFire = sim.events.On("fire", _ => fires++);
            Action offPunch = sim.events.On("punch", _ => punches++);
            Action offCore = sim.events.On<Wick47Event>("wick47CorePunch", _ => { corePunches++; if (openNow) punchesOpen++; });
            offs.Add(() => { offFire(); offPunch(); offCore(); });
            g.Respawn(c + new Vec3(0, 0.05, s.p.radius + 1.0), 0);
            g.input.SetLook(0, 0);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0 };
            yield return Frames(20);
            sim.Wick37Reset();
            if (!sim.Wick37Begin(out _, out _)) { yield return Stop(); res[0] = "RED: Wick37Begin returned false"; yield break; }
            Wick37 w = sim.wick37;
            var pilot = new Wick37Pilot { hand = new Mulberry32(1u * 7919u + 13u) };
            IPlayerInput prevInput = sim.input;
            var pilotIn = new Wick44ProofInput(sim, pilot, prevInput);
            g.forcedMove = null;
            g.forcedCombat = null;
            sim.UseInput(new Wick56FightInput(sim, pilotIn)); // NIGHT56 G (P1-wickWindow): n47human's trigger habit round the pilot (Bot/Wick56FightInput.cs)
            offs.Add(() => { if (sim.input == pilotIn || sim.input is Wick56FightInput) sim.UseInput(prevInput); });
            double t0 = sim.SimTime, openAt = -1, killAt = -1, prevFrac = w.HpFrac, dealt = 0, dealtOpen = 0, punchAt = -1;
            bool p1Shot = false, openShot = false, punchShot = false, killShot = false;
            double wallEnd = Time.realtimeSinceStartup + 240;
            int punchesOpenSeen = 0;
            while (Time.realtimeSinceStartup < wallEnd && sim.SimTime - t0 < 150)
            {
                God(g);
                double now = sim.SimTime - t0;
                Boss43State bs = sim.N43BossState;
                bool open = bs.valid && bs.open;
                double fr = w.HpFrac;
                if (fr < prevFrac) { dealt += prevFrac - fr; if (openNow) dealtOpen += prevFrac - fr; } // the share by the bar's state before the damage (n47human's rule)
                prevFrac = fr;
                openNow = open;
                bool core = w.coreOpen47 > 0 || w.bare47 > 0;
                if (open && core && openAt < 0) openAt = now;
                if (punchesOpen > punchesOpenSeen && punchAt < 0) punchAt = now;
                punchesOpenSeen = punchesOpen;
                if (!w.Alive && killAt < 0) killAt = now;
                if (!p1Shot && now >= 2.0) { p1Shot = true; yield return Shot29(dir, "W55-p1", st); continue; }
                if (!punchShot && punchAt >= 0) { punchShot = true; yield return Shot29(dir, "W55-punch", st); continue; }
                if (!openShot && openAt >= 0 && now - openAt >= 0.2) { openShot = true; yield return Shot29(dir, "W55-open", st); continue; }
                if (!killShot && killAt >= 0 && now - killAt >= 0.5) { killShot = true; yield return Shot29(dir, "W55-kill", st); break; }
                yield return null;
            }
            if (sim.input == pilotIn) sim.UseInput(prevInput);
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();
            double share = dealt > 0 ? dealtOpen / dealt : 0, want = B55Num(g, "proof.windowShare", 0.5);
            bool input = pilotIn.piloted > 0 && fires + punches > 0;
            bool killed = killAt >= 0;
            bool ok = !off && w.w55 && input && punchShot && share >= want && killed;
            string T(double t) => t < 0 ? "never" : t.ToString("0.0", ci) + " s";
            res[0] = $"{(ok ? "GREEN" : "RED")}: FIGHT MODE, rows wick54 + {(off ? "NO wick55 (the RED run: NIGHT54's fight)" : "wick55")} forced, map {g.MapId}, w54 {w.w54}, w55 {w.w55}; input: pilot steps {pilotIn.piloted}, rides {pilot.rides}, leaps {pilot.leaps}, shots {fires}, punches {punches}; " +
                     $"damage dealt while the bar read OPEN {(share * 100).ToString("0", ci)} % (>= {(want * 100).ToString("0", ci)} %); the bar's OPEN on the core first at {T(openAt)} (W55-open); core punches {corePunches}, inside a window {punchesOpen} (the first at {T(punchAt)}: {(punchShot ? "frame shot-N29-W55-punch.png" : "no frame")}); " +
                     $"hot-wall burns {sim.wick37HotBurns - burns0}; hits taken: ember {w.emberHits}, geyser {w.geyserHits47}, ring {w.ringHits47}, charge {w.chargeHits47}, slag {sim.wick37SlagTouches}; stagger windows {w.brain.windows}, hits inside {w.windowHits55}; " +
                     $"{(w.Alive ? "alive at " + T(sim.SimTime - t0) + " (bar " + (w.HpFrac * 100).ToString("0", ci) + " %)" : "killed at " + T(killAt) + " (W55-kill)")}";
            Debug.Log("[a155] " + res[0]);
        }

        /// <summary>NIGHT57 V (slate V3-grade55 guard: THE WICK's ride panels carry the cyan ride cue): over the shots taken with the panels OUT (P2 /
        /// P3, the turn into P2), the panel cells' cyan share at least proof.panelCyanMin (pooled, and each shot with >= 20 panel cells); the P1 shots
        /// (panels folded into the wall: no ride panel there, V-3 "a wall that cannot be ridden never has it") are listed apart, not counted.</summary>
        static string B55RideCue(SurvivalGame g, B55Shot[] shots)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            double min = B55Num(g, "proof.panelCyanMin", 0.1);
            int n = 0, cyan = 0, low = 0;
            var outs = new List<string>();
            var folded = new List<string>();
            foreach (B55Shot s in shots)
            {
                if (s == null || !s.ok) continue;
                string nm = s.name != null && s.name.LastIndexOf('-') >= 0 ? s.name.Substring(s.name.LastIndexOf('-') + 1) : s.name;
                if (!s.panelsOut) { folded.Add(nm); continue; }
                outs.Add($"{nm} {s.panelN} / {(s.PanelCyanShare * 100).ToString("0", ci)} %");
                n += s.panelN;
                cyan += s.panelCyan;
                if (s.panelN >= 20 && s.PanelCyanShare < min) low++;
            }
            double share = n > 0 ? (double)cyan / n : 0;
            bool ok = n >= 20 && share >= min && low == 0;
            return $" || RIDE CUE {(ok ? "GREEN" : "RED")} (NIGHT57 V): the panels OUT on {(outs.Count > 0 ? string.Join(", ", outs) : "no shot")}: pooled cyan share {(share * 100).ToString("0", ci)} % of {n} panel cells (>= {(min * 100).ToString("0", ci)} %, each shot >= 20 cells too: {(low == 0 ? "ok" : low + " under")}); folded into the wall (P1: no ride panel, no cue by design): {(folded.Count > 0 ? string.Join(", ", folded) : "none")}";
        }

        /// <summary>One measured shot: the classes' numbers.</summary>
        sealed class B55Shot
        {
            public bool ok;
            public string name;
            public readonly G55Stats[] cls = new G55Stats[(int)G55Class.Count];
            public G55Stats wallsAll, tells, whole, world, hot, otherWalls;
            public int ui, vm, overlays, samples;
            /// <summary>NIGHT56 V (the r1 critic, P0-hotWall: "print the cyan cue's share on the ride walls"): wall cells on THE WICK's ride panels
            /// (the hit within 0.35 m of a panel's face) and those in the usable cyan band (hue 170-200, S >= 0.4, V >= 0.35).</summary>
            public int panelN, panelCyan;
            /// <summary>NIGHT57 V (V3-grade55): THE WICK's ride panels were out at the capture (sim.Wick47PanelsOut; P1 folds them into the wall and
            /// out of the world: a P1 shot's "panel cells" are the shaft wall where the panels fold, with no ride panel and so no cue, V-3).</summary>
            public bool panelsOut;
            public double PanelCyanShare => panelN > 0 ? (double)panelCyan / panelN : 0;

            public G55Stats Walls => cls[(int)G55Class.Wall];

            /// <summary>The most saturated non-tell class with at least `minN` samples (sky and the HUD / gun excluded), and its mean S.</summary>
            public string MaxClass(int minN, out double s, bool skipBody = false)
            {
                s = -1;
                string best = "none";
                for (int i = 1; i < (int)G55Class.Ui; i++)
                {
                    if (skipBody && i == (int)G55Class.Body) continue;
                    if (cls[i].n < minN) continue;
                    if (cls[i].S > s) { s = cls[i].S; best = ((G55Class)i).ToString(); }
                }
                return best;
            }

            public double TellShare => world.n > 0 ? (double)tells.n / world.n : 0;
        }

        /// <summary>The hot patch a shot classifies (THE WICK's heat: a wall cell whose hit point lies inside it is the burning region).</summary>
        struct B55Patch
        {
            public bool on, central; // central: only the screen's middle (UnityTuning.Grade55CentralShare) counts in the classes (r2: a burn's hurt vignette, a red border, sat on the edge cells)
            public Vec3 o;
            public double angle, y, spanM, halfH, radius;
        }

        static double B55Num(SurvivalGame g, string k, double missing) => BossGrade55.N(g.cfg, k, (float)missing);

        /// <summary>NIGHT56 V: the boss's leg / phase now (THE WICK's and THE CROWN's brain leg; THE WYRM's retry phase, p2 = 2, p3 / p4 = 3, else 1).</summary>
        static int B56Leg(SurvivalSim sim, string bossId)
        {
            if (bossId == "wick") return sim.wick37 != null && sim.wick37.brain != null ? sim.wick37.brain.leg : 0;
            if (bossId == "crown37") return sim.crown37 != null && sim.crown37.brain != null ? sim.crown37.brain.leg : 0;
            if (bossId == "wyrm")
            {
                if (sim.wyrm24 == null || !sim.wyrm24.Alive) return 0;
                string rp = sim.wyrm24.RetryPhase;
                return rp == "p2" ? 2 : rp == "p3" || rp == "p4" ? 3 : 1;
            }
            return 0;
        }

        static string B55S(G55Stats s, CultureInfo ci) => s.Ok
            ? $"n {s.n} H {s.Hue.ToString("0", ci)} S {s.S.ToString("0.000", ci)} V {s.V.ToString("0.000", ci)} okH {s.OkHue.ToString("0", ci)} C {s.OkChroma.ToString("0.000", ci)} nearDanger {s.NearShare.ToString("0.00", ci)}"
            : "n 0";

        static readonly List<Rect> b55Rects = new List<Rect>();

        /// <summary>The screen rects of the HUD's visible graphics (overlay and camera canvases) and of the gun's renderers; big overlays skipped.</summary>
        static void B55Masks(SurvivalGame g, int w, int h, List<Rect> ui, List<Rect> vm, out int overlays)
        {
            overlays = 0;
            ui.Clear();
            vm.Clear();
            float screen = (float)w * h;
            foreach (Canvas c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c == null || !c.isActiveAndEnabled || !c.isRootCanvas || c.renderMode == RenderMode.WorldSpace) continue;
                foreach (Graphic gr in c.GetComponentsInChildren<Graphic>(false))
                {
                    if (gr == null || !gr.isActiveAndEnabled || gr.canvasRenderer == null || gr.canvasRenderer.cull) continue;
                    float a = gr.color.a * gr.canvasRenderer.GetInheritedAlpha();
                    if (a < UnityTuning.Grade55UiMinAlpha) continue;
                    if (!FeelProbe.UiScreenRect(gr.rectTransform, out Rect r)) continue;
                    if (r.width * r.height > UnityTuning.Grade55UiOverlayShare * screen) { overlays++; continue; }
                    ui.Add(r);
                }
            }
            Camera vc = g.viewmodel != null ? g.viewmodel.Camera : null;
            if (vc == null) return;
            var corners = new Vector3[8];
            foreach (Renderer rr in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (rr == null || !rr.enabled || !rr.gameObject.activeInHierarchy || rr.gameObject.layer != Viewmodel.Layer) continue;
                Bounds b = rr.bounds;
                Vector3 mn = b.min, mx = b.max;
                corners[0] = new Vector3(mn.x, mn.y, mn.z); corners[1] = new Vector3(mx.x, mn.y, mn.z); corners[2] = new Vector3(mn.x, mx.y, mn.z); corners[3] = new Vector3(mx.x, mx.y, mn.z);
                corners[4] = new Vector3(mn.x, mn.y, mx.z); corners[5] = new Vector3(mx.x, mn.y, mx.z); corners[6] = new Vector3(mn.x, mx.y, mx.z); corners[7] = new Vector3(mx.x, mx.y, mx.z);
                float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
                bool any = false;
                foreach (Vector3 p in corners)
                {
                    Vector3 s = vc.WorldToScreenPoint(p);
                    if (s.z <= vc.nearClipPlane * 0.5f) continue;
                    any = true;
                    x0 = Mathf.Min(x0, s.x); y0 = Mathf.Min(y0, s.y); x1 = Mathf.Max(x1, s.x); y1 = Mathf.Max(y1, s.y);
                }
                if (any) vm.Add(Rect.MinMaxRect(x0, y0, x1, y1));
            }
        }

        static bool B55In(List<Rect> rects, float x, float y)
        {
            foreach (Rect r in rects) if (r.Contains(new Vector2(x, y))) return true;
            return false;
        }

        /// <summary>Saves the screen to dir/shot-N55-name.jpg, classifies and measures its grid into m, saves the class mask beside it.</summary>
        static IEnumerator B55Take(SurvivalGame g, string dir, string name, B55Shot m, B55Patch patch)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null, mask = null;
            m.ok = false;
            m.name = name;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N55-" + name + ".jpg"), t.EncodeToJPG(85));
                Camera cam = g.cam;
                SurvivalSim sim = g.sim;
                if (cam == null || sim == null) yield break;
                m.panelsOut = sim.wick37Stack != null && sim.Wick47PanelsOut; // NIGHT57 V: the ride panels out at this capture
                Color.RGBToHSV(Skygrind.Game.Look.C(g.cfg, "colors.enemy"), out float dh0, out _, out _);
                double danger = dh0 * 360, band = B55Num(g, "proof.tellBandDeg", 18), tSat = B55Num(g, "proof.tellSat", 0.5), tVal = B55Num(g, "proof.tellVal", 0.25);
                double nearDeg = B55Num(g, "proof.nearDeg", 30);
                int W = t.width, H = t.height, GW = UnityTuning.Grade55GridW, GH = UnityTuning.Grade55GridH, cell = UnityTuning.Grade55MaskCell;
                var vmRects = new List<Rect>();
                B55Masks(g, W, H, b55Rects, vmRects, out m.overlays);
                mask = new Texture2D(GW * cell, GH * cell, TextureFormat.RGB24, false);
                var px = new Color32[GW * cell * GH * cell];
                for (int j = 0; j < GH; j++)
                    for (int i = 0; i < GW; i++)
                    {
                        float x = (i + 0.5f) * W / GW, y = (j + 0.5f) * H / GH;
                        Color c = t.GetPixel((int)x, (int)y);
                        m.whole.Add(c, danger, nearDeg);
                        m.samples++;
                        G55Class k;
                        bool tell = false, hot = false;
                        float cm = (1f - UnityTuning.Grade55CentralShare) * 0.5f;
                        if (patch.central && (x < cm * W || x > (1 - cm) * W || y < cm * H || y > (1 - cm) * H)) { k = G55Class.Ui; m.ui++; } // r2: the border left out (drawn black in the mask)
                        else if (B55In(b55Rects, x, y)) { k = G55Class.Ui; m.ui++; }
                        else if (B55In(vmRects, x, y)) { k = G55Class.Viewmodel; m.vm++; }
                        else
                        {
                            Ray ray = cam.ViewportPointToRay(new Vector3(x / W, y / H, 0));
                            Vec3 o = new Vec3(ray.origin.x, ray.origin.y, -ray.origin.z), d = new Vec3(ray.direction.x, ray.direction.y, -ray.direction.z);
                            double best = 500;
                            k = G55Class.Sky;
                            Vec3 hitAt = default;
                            if (sim.world.RayIntersect(o, d, out RayHit wh) && wh.distance < best) { best = wh.distance; k = G55Measure.OfNormal(wh.normal.y); hitAt = o + d * wh.distance; }
                            EnemyRayHit? eh = sim.enemies.Raycast(o, d, best);
                            if (eh.HasValue && eh.Value.distance < best) { best = eh.Value.distance; k = G55Class.Body; }
                            if (sim.crown37 != null)
                            {
                                double ct = sim.crown37.Raycast(o, d, out _);
                                if (ct >= 0 && ct < best) { best = ct; k = G55Class.Body; }
                            }
                            if (sim.wyrm24 != null && sim.wyrm24.Alive) // NIGHT56 V: THE WYRM's oriented pieces (its own ray: BoxLayer is axis-aligned)
                            {
                                double wt = sim.wyrm24.Raycast(o, d);
                                if (wt >= 0 && wt < best) { best = wt; k = G55Class.Body; }
                            }
                            tell = G55Measure.IsTell(c, danger, band, tSat, tVal);
                            m.world.Add(c, danger, nearDeg);
                            if (tell) m.tells.Add(c, danger, nearDeg);
                            else m.cls[(int)k].Add(c, danger, nearDeg);
                            if (k == G55Class.Wall)
                            {
                                m.wallsAll.Add(c, danger, nearDeg);
                                if (sim.wick37Stack != null && sim.wick37Centre != null && sim.wick37Stack.NearestPanel(hitAt - sim.wick37Centre.Value, out double pd56) >= 0 && pd56 < 0.35)
                                {
                                    m.panelN++; // NIGHT56 V: a ride panel's cell; its cyan cue (the trim) by colour
                                    Color.RGBToHSV(c, out float ph56, out float ps56, out float pv56);
                                    if (ph56 * 360f >= 170f && ph56 * 360f <= 200f && ps56 >= 0.4f && pv56 >= 0.35f) m.panelCyan++;
                                }
                                if (patch.on)
                                {
                                    Vec3 l = hitAt - patch.o;
                                    double ang = Math.Atan2(l.z, l.x);
                                    hot = Wick37View.AlongM55(ang, patch.angle, patch.radius) <= patch.spanM && Math.Abs(l.y - patch.y) <= patch.halfH;
                                    if (hot) m.hot.Add(c, danger, nearDeg); else m.otherWalls.Add(c, danger, nearDeg);
                                }
                            }
                        }
                        Color32 mc = G55Measure.MaskColour(k, tell, hot);
                        for (int yy = 0; yy < cell; yy++)
                            for (int xx = 0; xx < cell; xx++)
                                px[(j * cell + yy) * GW * cell + i * cell + xx] = (xx == 0 || yy == 0) ? new Color32((byte)(mc.r / 2), (byte)(mc.g / 2), (byte)(mc.b / 2), 255) : mc;
                    }
                mask.SetPixels32(px);
                mask.Apply(false);
                File.WriteAllBytes(Path.Combine(dir, "shot-N55-" + name + "-mask.png"), mask.EncodeToPNG());
                m.ok = true;
            }
            finally
            {
                if (t != null) UnityEngine.Object.Destroy(t);
                if (mask != null) UnityEngine.Object.Destroy(mask);
            }
        }

        static IEnumerator B55Hold(SurvivalGame g, double s, double yaw, double pitch)
        {
            float t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < s) { God(g); g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch }; yield return null; }
        }

        static void B55Log(StringBuilder sb, B55Shot s, CultureInfo ci)
        {
            sb.Append("== ").Append(s.name).Append(s.ok ? "" : " (NOT MEASURED)").Append(": samples ").Append(s.samples).Append(", HUD-masked ").Append(s.ui)
              .Append(", gun-masked ").Append(s.vm).Append(", big overlays skipped ").Append(s.overlays).Append('\n');
            sb.Append("   whole (a254's, HUD in) ").Append(B55S(s.whole, ci)).Append('\n');
            sb.Append("   world (HUD and gun out) ").Append(B55S(s.world, ci)).Append('\n');
            sb.Append("   tells ").Append(B55S(s.tells, ci)).Append(" | share of the world ").Append(s.TellShare.ToString("0.000", ci)).Append('\n');
            for (int i = 0; i < (int)G55Class.Ui; i++) sb.Append("   ").Append(((G55Class)i).ToString().ToLowerInvariant()).Append(' ').Append(B55S(s.cls[i], ci)).Append('\n');
            sb.Append("   walls with their tells ").Append(B55S(s.wallsAll, ci)).Append('\n');
            if (s.hot.Ok || s.otherWalls.Ok) sb.Append("   hot patch ").Append(B55S(s.hot, ci)).Append(" | other walls ").Append(B55S(s.otherWalls, ci)).Append('\n');
        }

        static IEnumerator B55Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; BossGrade55.Suppress = false; Quiet(g); g.setup.mode = RunSetup.Survival; });
            string gate = N24CampaignGate(g);
            if (gate != null) { res[0] = "RED: " + gate + " (boot -skygrindMap n52-foundry or n52-spire)"; yield break; }
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(240);
            sim = g.sim;
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            DoorFree41(g);
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            bool rowOn = BossGrade55.On(g.cfg);
            string bossId = null;
            if (sim.campaign != null && sim.campaign.Level != null)
                foreach (RoomDef r in sim.campaign.Level.rooms)
                    if (r.kind == "boss") { bossId = r.boss; break; }
            string pc = $"map {g.MapId}, campaign {sim.campaign != null}, boss room {bossId ?? "none"}, row grade55 {(rowOn ? "on" : "off (the RED run)")}";
            if (bossId != "wick" && bossId != "crown37" && bossId != "wyrm") { res[0] = "RED: a155 measures THE WICK (n52-foundry), THE CROWN (n52-spire) or THE WYRM (n52-kiln, NIGHT56 V); this level's boss room is " + (bossId ?? "none") + " | " + pc; yield break; }
            bool wyrm56 = bossId == "wyrm"; // NIGHT56 V (V4): THE WYRM's turn inside 15-55 deg (GG NIGHT47: "the same yellow-red-orange color theme")
            bool wick = bossId == "wick";

            Start(g, "grade55", "camera", dir);
            yield return Frames(pre);
            var none = new B55Patch();
            var before = new B55Shot();
            g.forcedMove = new InputFrame();
            yield return Frames(20);
            yield return B55Take(g, dir, bossId + "-before", before, none);

            // the fight
            int turns0 = BossGrade55.Turns;
            bool begun = sim.BossRoomBegin(bossId, out Vec3 spawn, out double yawDeg);
            if (!begun) { yield return Stop(); res[0] = $"RED: BossRoomBegin({bossId}) returned false | " + pc; yield break; }
            g.Respawn(spawn, yawDeg);
            double yaw = yawDeg * Math.PI / 180, pitch = 0.08;
            g.input.SetLook(yaw, pitch);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
            float tBegin = Time.unscaledTime;
            for (int k = 0; k < 600 && rowOn && !BossGrade55.Live; k++) { God(g); yield return null; }
            bool live = BossGrade55.Live;
            string graded = BossGrade55.Boss ?? "none";
            double startPeak = 0;
            for (int k = 0; k < 600 && rowOn && (BossGrade55.Weight < 0.99f || BossGrade55.TurnEnvelope > 0 || BossGrade55.Turns == turns0); k++)
            {
                startPeak = Math.Max(startPeak, BossGrade55.TurnEnvelope);
                God(g); g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch }; yield return null;
            }
            int startTurns = BossGrade55.Turns - turns0;
            string startKind = BossGrade55.LastTurn + ", envelope peak " + startPeak.ToString("0.00", ci);
            double waited = Time.unscaledTime - tBegin;
            yield return B55Hold(g, Math.Max(0.8, 2.2 - waited), yaw, pitch); // the off run reaches the same moment of the fight (~2.2 s in)
            int phase1 = B56Leg(sim, bossId);
            float weight1 = BossGrade55.Weight;
            var p1 = new B55Shot();
            yield return B55Take(g, dir, bossId + "-p1", p1, none);
            BossGrade55.Suppress = true;
            yield return Frames(2);
            var raw = new B55Shot();
            yield return B55Take(g, dir, bossId + "-p1-raw", raw, none);
            BossGrade55.Suppress = false;
            yield return Frames(2);

            // THE WICK's tell: a red tell on the core (graded / suppressed); THE CROWN's ring is in p1
            B55Shot tellG = p1, tellR = raw;
            string tellInput = wick ? "no red tell in 10 s" : wyrm56 ? "THE WYRM's molten red-orange (p1 / p1-raw)" : "THE CROWN's ring (p1 / p1-raw)";
            if (wick)
            {
                Wick37 w0 = sim.wick37;
                float t0 = Time.unscaledTime;
                while (Time.unscaledTime - t0 < 10f)
                {
                    Boss37Brain b = w0 != null ? w0.brain : null;
                    if (b != null && b.state == Boss37State.Tell && b.Current != null && b.Current.kind == Tell36Kind.Red && b.Progress >= 0.5) break;
                    God(g); g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch }; yield return null;
                }
                Boss37Brain bb = w0 != null ? w0.brain : null;
                if (bb != null && bb.state == Boss37State.Tell && bb.Current != null && bb.Current.kind == Tell36Kind.Red)
                {
                    tellInput = $"red tell {w0.AttackId} at progress {bb.Progress.ToString("0.00", ci)}";
                    tellG = new B55Shot();
                    yield return B55Take(g, dir, bossId + "-tell", tellG, none);
                    BossGrade55.Suppress = true;
                    yield return Frames(2);
                    tellR = new B55Shot();
                    yield return B55Take(g, dir, bossId + "-tell-raw", tellR, none);
                    BossGrade55.Suppress = false;
                    yield return Frames(2);
                }
            }

            // THE WICK's hot wall: a real burn at the far side, seen from the spawn (burn), then the same view with no wall burning (cold)
            var burn = new B55Shot();
            var cold = new B55Shot();
            string burnInput = "THE CROWN has no hot wall";
            float heatDrawn = 0;
            int heatQuads = 0;
            if (wick && sim.wick37Centre != null && sim.wick37Stack != null)
            {
                Vec3 o = sim.wick37Centre.Value;
                Wick37Stack st = sim.wick37Stack;
                double capR = g.cfg.movement.body.capsuleRadius;
                Vec3 sl = spawn - o;
                double spawnAng = Math.Atan2(sl.z, sl.x);
                int burns0 = sim.wick37HotBurns, tries = 0;
                double burnAng = 0, burnY = 2.6;
                bool burned = false;
                // NIGHT56 V (V4; n56-r1: "burns 0 -> 0 after 8 placement(s)"): wick55 (on in the r1 copy) burns a RIDE only, never a brush in the air,
                // and r2's 8 spots at 2.6 m could sit over a step / panel. The placement now suspends wick55's ride-only rule (the brush rule, NIGHT54's:
                // the view's burn is the same whichever rule fired it; restored at once) and walks 4 heights x 6 angles x 2 sides, stopping at the first burn.
                bool w55Was = sim.wick37 != null && sim.wick37.w55;
                if (w55Was) sim.wick37.w55 = false;
                double[] ys56 = { 2.6, 4.4, 6.2, 3.5 }, offs56 = { 0.75, 1.0, 1.35, 1.7, 2.1, 2.6 };
                for (; tries < ys56.Length * offs56.Length * 2 && !burned; tries++)
                {
                    burnY = ys56[tries / (offs56.Length * 2)];
                    burnAng = spawnAng + (tries % 2 == 0 ? 1 : -1) * offs56[(tries / 2) % offs56.Length];
                    double rr = st.p.radius - capR - 0.04;
                    Vec3 at = o + new Vec3(Math.Cos(burnAng) * rr, burnY, Math.Sin(burnAng) * rr);
                    g.Respawn(at, 0);
                    for (int k = 0; k < 30 && sim.wick37HotBurns == burns0; k++) { God(g); g.forcedMove = new InputFrame(); yield return null; }
                    burned = sim.wick37HotBurns > burns0;
                }
                if (w55Was && sim.wick37 != null) sim.wick37.w55 = true;
                // NIGHT56 V: the burn seen from ACROSS the shaft (r2 looked from the spawn: the tunnel's own wall filled the frame, n56-r1 burn.jpg):
                // standing on the shaft floor at 0.55 of the radius, 50 deg off the diameter through the burn (the core at the axis out of the line)
                double va56 = burnAng + Math.PI + 0.87;
                Vec3 view56 = o + new Vec3(Math.Cos(va56) * st.p.radius * 0.55, 0.05, Math.Sin(va56) * st.p.radius * 0.55);
                g.Respawn(burned ? view56 : spawn, yawDeg);
                God(g); g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                yield return null;
                Vec3 bp = o + new Vec3(Math.Cos(burnAng) * st.p.radius, burnY, Math.Sin(burnAng) * st.p.radius);
                Vec3 eye = sim.Eye;
                double bx = bp.x - eye.x, by = bp.y + 0.6 - eye.y, bz = bp.z - eye.z;
                double byaw = Math.Atan2(-bx, -bz), bpitch = Math.Atan2(by, Math.Sqrt(bx * bx + bz * bz)); // the web's yaw (forward -z) toward the burn
                g.input.SetLook(byaw, bpitch);
                for (int k = 0; k < 3; k++) { God(g); g.forcedMove = new InputFrame { yaw = byaw, pitch = bpitch }; yield return null; }
                Wick37View v = Wick37View.Current;
                heatDrawn = v != null ? v.Heat55 : 0;
                heatQuads = v != null ? v.HeatQuads55 : 0;
                var patch = new B55Patch
                {
                    on = true, central = true, o = o, radius = st.p.radius,
                    angle = v != null && v.On55 ? v.HeatAngle55 : burnAng, y = v != null && v.On55 ? v.HeatY55 : burnY,
                    spanM = v != null && v.On55 ? v.HeatSpanM55 : B55Num(g, "hot.spanM", 3), halfH = v != null && v.On55 ? v.HeatHalfH55 : 4 * UnityTuning.Stack37SeamStep,
                };
                burnInput = $"burns {burns0} -> {sim.wick37HotBurns} after {tries} placement(s) (angle {(burnAng * 180 / Math.PI).ToString("0", ci)} deg, y {burnY.ToString("0.0", ci)}; wick55's ride-only rule {(w55Was ? "suspended for the placement" : "off")}), seen from {(burned ? "across the shaft (0.55 R, 50 deg off the diameter)" : "the spawn")}, heat drawn {heatDrawn.ToString("0.00", ci)} on {heatQuads} quads, opaque iron {(v != null && v.BurnOpaque55 ? "yes" : "no")}";
                yield return B55Take(g, dir, bossId + "-burn", burn, patch);
                yield return B55Hold(g, B55Num(g, "hot.burnS", 0.8) + 0.6, byaw, bpitch);
                yield return B55Take(g, dir, bossId + "-cold", cold, patch);
                if (burned) g.Respawn(spawn, yawDeg); // NIGHT56 V: back to the spawn for the turn and p2 (as round 2 took them)
                g.input.SetLook(yaw, pitch);
                yield return B55Hold(g, 0.3, yaw, pitch);
            }

            // THE TURN: a real phase change
            int turnsBefore = BossGrade55.Turns;
            string turnInput = "none";
            if (wick)
            {
                Wick37 w = sim.wick37;
                if (w != null && w.Alive && w.w47 && w.p47 != null && w.p47.gate.Length > 0)
                {
                    w.Apply(w.body.hp - w.body.maxHp * w.p47.gate[0] + 1);
                    turnInput = "THE WICK's bar pushed to P1's gate (a real phase change, as a254)";
                }
            }
            else if (wyrm56)
            {
                Wyrm wy = sim.wyrm24;
                if (wy != null && wy.Alive && wy.phase == "p1" && wy.p.phase2At > 0)
                {
                    wy.Apply(wy.body.hp - wy.body.maxHp * wy.p.phase2At + 1);
                    turnInput = $"THE WYRM's HP pushed under phase2At {wy.p.phase2At.ToString("0.00", ci)} (its own beat into P2: phase {wy.phase}, retry phase {wy.RetryPhase ?? "none"})";
                }
                else turnInput = $"THE WYRM not in p1 (phase {wy?.phase ?? "no wyrm"})";
            }
            else
            {
                Crown37 c = sim.crown37;
                if (c != null && c.Alive && c.p.legGemsLeft != null && c.p.legGemsLeft.Length > 0)
                {
                    double target = c.p.legGemsLeft[0] * c.p.gemHp;
                    if (c.body.hp > target) c.Apply(c.body.hp - target, "proof");
                    turnInput = $"THE CROWN's gems to legGemsLeft[0] = {c.p.legGemsLeft[0]} (its own leg change; gems left {c.GemsLeft})";
                }
            }
            for (int k = 0; k < 240 && rowOn && BossGrade55.Turns == turnsBefore; k++) { God(g); g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch }; yield return null; }
            if (!rowOn) yield return B55Hold(g, 0.15, yaw, pitch);
            bool turned = BossGrade55.Turns > turnsBefore;
            string turnKind = BossGrade55.LastTurn;
            double envPeak = 0, envPrev = 0;
            for (int k = 0; k < 120 && turned; k++)
            {
                double e = BossGrade55.TurnEnvelope;
                envPeak = Math.Max(envPeak, e);
                if (e >= 0.85 || (envPrev > 0.3 && e < envPrev)) break;
                envPrev = e;
                God(g); g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch }; yield return null;
            }
            envPeak = Math.Max(envPeak, BossGrade55.TurnEnvelope);
            var turnM = new B55Shot();
            yield return B55Take(g, dir, bossId + "-turn", turnM, none);
            yield return B55Hold(g, 1.0, yaw, pitch);
            var p2 = new B55Shot();
            yield return B55Take(g, dir, bossId + "-p2", p2, none);
            int phase2 = B56Leg(sim, bossId);
            int gradePhase2 = BossGrade55.Phase;
            g.forcedMove = null;
            g.forcedCombat = IdleCombat;
            yield return Stop();

            // ---- the numbers ----
            Color.RGBToHSV(Skygrind.Game.Look.C(g.cfg, "colors.enemy"), out float dh0, out _, out _);
            double danger = dh0 * 360;
            double wallMin = B55Num(g, "proof.wallMinDeg", 30), frameMin = B55Num(g, "proof.frameMinDeg", 20), minHue = B55Num(g, "proof.minHue", 25), minS = B55Num(g, "proof.minS", 0.10);
            double tellMinS = B55Num(g, "proof.tellMinS", 0.6), tellKeep = B55Num(g, "proof.tellKeep", 0.8), tellShift = B55Num(g, "proof.tellMaxShiftDeg", 10), cyanMin = B55Num(g, "proof.cyanMinRatio", 4.5);
            double nearMax = B55Num(g, "proof.nearMaxShare", 0.25), stepMinHue = B55Num(g, "proof.phaseMinHue", 20), stepMinV = B55Num(g, "proof.phaseMinV", 0.1);
            double coldMax = B55Num(g, "proof.coldMaxShare", 0.10), burnGap = B55Num(g, "proof.burnSatGap", 0.15), stripeMin = B55Num(g, "proof.stripeMinDeg", 30);
            int minN = (int)B55Num(g, "proof.classMinN", 20);

            double WD(B55Shot s) => s.Walls.Ok ? G55Measure.HueDist(s.Walls.Hue, danger) : 0;
            double FD(B55Shot s) => s.whole.Ok ? G55Measure.HueDist(s.whole.Hue, danger) : 0;
            double w1 = WD(p1), wT = WD(turnM), w2 = WD(p2), f1 = FD(p1), fT = FD(turnM), f2 = FD(p2);
            bool wallsOk = p1.Walls.n >= minN && w1 >= wallMin && wT >= wallMin && w2 >= wallMin;
            bool frameOk = f1 >= frameMin && fT >= frameMin && f2 >= frameMin;
            bool nearOk = p1.Walls.NearShare <= nearMax && turnM.Walls.NearShare <= nearMax && p2.Walls.NearShare <= nearMax;
            double dH = p1.Walls.Ok && raw.Walls.Ok ? G55Measure.HueDist(p1.Walls.Hue, raw.Walls.Hue) : 0, dS = p1.Walls.S - raw.Walls.S, dV = p1.Walls.V - raw.Walls.V;
            bool differs = dH >= minHue || Math.Abs(dS) >= minS;
            // the tells: kept (count), saturated, the most saturated class, their hue moved little
            bool tellsSeen = tellR.tells.n >= 5;
            double tellMax;
            string tellMaxCls = tellG.MaxClass(minN, out tellMax);
            double tShift = tellG.tells.Ok && tellR.tells.Ok ? G55Measure.HueDist(tellG.tells.Hue, tellR.tells.Hue) : 0;
            bool tellsOk = tellsSeen && tellG.tells.n >= tellKeep * tellR.tells.n && tellG.tells.S >= tellMinS && tellG.tells.S > tellMax && tShift <= tellShift;
            Color usable = Skygrind.Game.Look.C(g.cfg, "colors.usable");
            double cyanRatio = p1.whole.Ok ? G55Measure.ContrastRatio(usable.linear, p1.whole.Lin) : 0;
            bool cyanOk = cyanRatio >= cyanMin;
            double stepH = p1.Walls.Ok && p2.Walls.Ok ? G55Measure.HueDist(p1.Walls.Hue, p2.Walls.Hue) : 0, stepV = p2.Walls.V - p1.Walls.V;
            bool stepOk = phase2 > phase1 && (stepH >= stepMinHue || Math.Abs(stepV) >= stepMinV);
            bool gradeOk = rowOn && begun && live && startTurns > 0 && turned && gradePhase2 >= 2 && wallsOk && frameOk && nearOk && differs && tellsOk && cyanOk && stepOk;
            string wyrmLine = "";
            if (wyrm56)
            {
                // NIGHT56 V (V4): THE WYRM keeps the yellow-red-orange theme (GG NIGHT47), so the cool-walls rules above do not apply: its turn's wall
                // hue sits inside proof.wyrmTurnLo..Hi (15-55: amber to orange, never the acid green of n54-r3 g3k's turn), the fight live and turned
                double lo = B55Num(g, "proof.wyrmTurnLo", 15), hi = B55Num(g, "proof.wyrmTurnHi", 55);
                bool tIn = turnM.Walls.Ok && turnM.Walls.Hue >= lo && turnM.Walls.Hue <= hi;
                bool wIn = turnM.whole.Ok && turnM.whole.Hue >= lo && turnM.whole.Hue <= hi;
                gradeOk = rowOn && begun && live && turned && gradePhase2 >= 2 && tIn && cyanOk;
                wyrmLine = $" || THE WYRM'S TURN {(gradeOk ? "GREEN" : "RED")}: the turn frame's walls H {turnM.Walls.Hue.ToString("0", ci)} (n {turnM.Walls.n}; inside {lo.ToString("0", ci)}-{hi.ToString("0", ci)}: {(tIn ? "ok" : "NO")}), whole frame H {turnM.whole.Hue.ToString("0", ci)} ({(wIn ? "inside" : "outside")}); p1 walls H {p1.Walls.Hue.ToString("0", ci)}, p2 H {p2.Walls.Hue.ToString("0", ci)}; the body class H p1 {p1.cls[(int)G55Class.Body].Hue.ToString("0", ci)} / turn {turnM.cls[(int)G55Class.Body].Hue.ToString("0", ci)} (the critic's 'head within 30 deg of its molten body': read on the frames)";
            }

            string hotLine = "", hotVerdict = "";
            bool hotOk = true;
            if (wick)
            {
                Wick37View v = Wick37View.Current;
                Color seam = v != null && v.On55 ? v.Seam55 : Skygrind.Game.Look.Hex(UnityTuning.Stack37HotHex);
                Color.RGBToHSV(seam, out float sh, out float ss, out _);
                double seamD = G55Measure.HueDist(sh * 360, danger);
                bool seamOk = ss < 0.15f || seamD >= stripeMin;
                double hotMax;
                string hotMaxCls = burn.MaxClass(minN, out hotMax);
                bool burnSeen = burn.hot.n >= 8;
                bool burnOk = burnSeen && burn.hot.S >= burn.otherWalls.S + burnGap && burn.hot.S > hotMax;
                bool coldOk = cold.ok && cold.TellShare <= coldMax && p1.TellShare <= coldMax;
                hotOk = rowOn && v != null && v.On55 && heatDrawn > 0.5f && burnOk && coldOk && seamOk;
                hotVerdict = hotOk ? "GREEN" : "RED";
                hotLine = $" || HOT WALL {hotVerdict}: the burn's input: {burnInput}; burn frame: hot patch S {burn.hot.S.ToString("0.000", ci)} (n {burn.hot.n}) vs the other walls S {burn.otherWalls.S.ToString("0.000", ci)} (+{burnGap.ToString("0.00", ci)} wanted: {(burn.hot.S >= burn.otherWalls.S + burnGap ? "ok" : "NO")}), the most saturated other class {hotMaxCls} S {hotMax.ToString("0.000", ci)} ({(burn.hot.S > hotMax ? "the patch is the most saturated" : "NOT the most saturated")}); " +
                          $"no wall burning: red-orange share of the world p1 {p1.TellShare.ToString("0.000", ci)} / cold {cold.TellShare.ToString("0.000", ci)} (<= {coldMax.ToString("0.00", ci)}; the off run prints the shipped look's share: ON must be <= half of it); " +
                          $"the cold seams: hue {(sh * 360).ToString("0", ci)} S {ss.ToString("0.00", ci)} ({(ss < 0.15f ? "value-only" : seamD.ToString("0", ci) + " deg from danger")}, >= {stripeMin.ToString("0", ci)} or value-only: {(seamOk ? "ok" : "NO")}); " +
                          $"THE RIDE PANELS' CYAN CUE (NIGHT56 V): panel cells / cyan share p1 {p1.panelN} / {(p1.PanelCyanShare * 100).ToString("0", ci)} %, burn {burn.panelN} / {(burn.PanelCyanShare * 100).ToString("0", ci)} %, cold {cold.panelN} / {(cold.PanelCyanShare * 100).ToString("0", ci)} %, turn {turnM.panelN} / {(turnM.PanelCyanShare * 100).ToString("0", ci)} %, p2 {p2.panelN} / {(p2.PanelCyanShare * 100).ToString("0", ci)} %" +
                          B55RideCue(g, new[] { p1, burn, cold, turnM, p2 });
            }
            bool ok = gradeOk && hotOk;
            res[0] = $"{(ok ? "GREEN" : "RED")}: {pc}; GRADE {(gradeOk ? "GREEN" : "RED")}: BossRoomBegin({bossId}) {begun}, graded live {live} (boss {graded}, weight {weight1.ToString("0.00", ci)}), start turn fired {startTurns} ({startKind}); " +
                     $"the turn's input: {turnInput}: fired {turned} ({turnKind}, envelope peak {envPeak.ToString("0.00", ci)}), the sim's leg {phase1} -> {phase2} (LEG {phase2}), the grade's phase {gradePhase2} | " +
                     $"WALLS (the render's wall cells, tells out) p1 H {p1.Walls.Hue.ToString("0", ci)} S {p1.Walls.S.ToString("0.000", ci)} V {p1.Walls.V.ToString("0.000", ci)} (n {p1.Walls.n}) / p1-raw H {raw.Walls.Hue.ToString("0", ci)} S {raw.Walls.S.ToString("0.000", ci)} V {raw.Walls.V.ToString("0.000", ci)} / turn H {turnM.Walls.Hue.ToString("0", ci)} / p2 H {p2.Walls.Hue.ToString("0", ci)} S {p2.Walls.S.ToString("0.000", ci)} V {p2.Walls.V.ToString("0.000", ci)}; " +
                     $"danger hue {danger.ToString("0", ci)}: walls' distance p1 {w1.ToString("0", ci)} / turn {wT.ToString("0", ci)} / p2 {w2.ToString("0", ci)} (>= {wallMin.ToString("0", ci)}: {(wallsOk ? "ok" : "NO")}); walls near danger p1 {p1.Walls.NearShare.ToString("0.00", ci)} / turn {turnM.Walls.NearShare.ToString("0.00", ci)} / p2 {p2.Walls.NearShare.ToString("0.00", ci)} (<= {nearMax.ToString("0.00", ci)}: {(nearOk ? "ok" : "NO")}); " +
                     $"graded vs raw walls dH {dH.ToString("0", ci)} (>= {minHue.ToString("0", ci)}) dS {dS.ToString("+0.000;-0.000", ci)} (|.| >= {minS.ToString("0.00", ci)}) dV {dV.ToString("+0.000;-0.000", ci)}: differs {differs}; " +
                     $"WHOLE FRAME (D-16, HUD in) p1 H {p1.whole.Hue.ToString("0", ci)} / turn H {turnM.whole.Hue.ToString("0", ci)} / p2 H {p2.whole.Hue.ToString("0", ci)}: distance {f1.ToString("0", ci)} / {fT.ToString("0", ci)} / {f2.ToString("0", ci)} (>= {frameMin.ToString("0", ci)}: {(frameOk ? "ok" : "NO")}) | " +
                     $"TELLS ({tellInput}): graded n {tellG.tells.n} S {tellG.tells.S.ToString("0.000", ci)} H {tellG.tells.Hue.ToString("0", ci)} vs raw n {tellR.tells.n} S {tellR.tells.S.ToString("0.000", ci)} H {tellR.tells.Hue.ToString("0", ci)} (kept >= {tellKeep.ToString("0.0", ci)}, S >= {tellMinS.ToString("0.0", ci)}, hue moved {tShift.ToString("0", ci)} <= {tellShift.ToString("0", ci)}), the most saturated other class {tellMaxCls} S {tellMax.ToString("0.000", ci)}: {(tellsOk ? "ok" : tellsSeen ? "NO" : "NO (fewer than 5 tell cells in the raw frame)")} | " +
                     $"cyan vs the graded p1 frame {cyanRatio.ToString("0.0", ci)}:1 (>= {cyanMin.ToString("0.0", ci)}: {(cyanOk ? "ok" : "NO")}) | p1 -> p2 walls dH {stepH.ToString("0", ci)} dV {stepV.ToString("+0.000;-0.000", ci)} (>= {stepMinHue.ToString("0", ci)} deg or >= {stepMinV.ToString("0.00", ci)}: {(stepOk ? "ok" : "NO")})" + hotLine + wyrmLine +
                     $" | masks beside each frame (shot-N55-{bossId}-<name>-mask.png); every class in a155-{bossId}.txt";
            var sb = new StringBuilder();
            sb.Append("a155 ").Append(bossId).Append(": ").Append(res[0]).Append("\n\n");
            foreach (B55Shot s in new[] { before, p1, raw, tellG == p1 ? null : tellG, tellR == raw ? null : tellR, burn, cold, turnM, p2 })
                if (s != null && s.name != null) B55Log(sb, s, ci);
            try { File.WriteAllText(Path.Combine(dir, "a155-" + bossId + ".txt"), sb.ToString()); } catch (Exception) { }
            Debug.Log("[a155] " + res[0]);
        }
    }
}
