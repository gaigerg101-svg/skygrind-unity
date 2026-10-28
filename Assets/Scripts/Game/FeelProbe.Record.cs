// NIGHT18 (NIGHT18.md section 1.1, the lead's): FeelProbe's recorder. Begin (FeelReel, before a scenario's pre-roll) clears
// the ring; every rendered frame's end (EndFrame -> RecordFrame) copies the frame into fixed arrays: the sim step, HELD, the
// camera, the player's velocity, the husks the view reported, the effect groups, the chunk points, the events, and every
// visible UI Graphic under the game's canvases (the scanner: root canvases found at Begin and every ProbeRescanFrames; each
// root's active Graphics listed with GetComponentsInChildren into a reused list). End writes telemetry.jsonl
// (Tools/feel-schema.md) between scenarios. Reel-only: outside Begin..End nothing here runs.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static partial class FeelProbe
    {
        /// <summary>The scenario header (FeelReel fills it before Begin).</summary>
        public sealed class Header
        {
            public string name, kind;
            public bool feelAB;
            public uint seed;
            public double distance = double.NaN;
            public int target = -1;
            public double maxSpeed;
        }

        struct FrameRec
        {
            public int step, steps, heldSteps;
            public bool held;
            public float slowmo;
            public Flow flow;
            public float fov, roll, pitch, height, cx, cy, cz;
            public float vFwd, vRight, vUp, speed;
            public int streakCount; // NIGHT20 (M8 v2): -1 when nothing reported
            public float streakLenPx, streakAngleDeg, streakMinRho, streakInnerAlpha;
            public int streakCross;
            public int enemyStart, enemyCount, uiStart, uiCount, chunkStart, chunkCount, chunkTotal, eventStart, eventCount;
            public string fired; // NIGHT41 T2 (FeelProbe.Weapons41.cs): the weapon fields, written next to vel
            public float charge, pump, spin;
            public int spinKind;
            public bool pad;
        }

        struct FxRec
        {
            public float x0, y0, x1, y1, alpha;
            public int count;
        }

        struct EnemyRec
        {
            public int id;
            public Rect rect;
            public float flash;
            public Vector2 anchor;
        }

        struct UiRec
        {
            public int id;
            public Rect rect;
            public float scale, alpha, px;
            public Color32 color;
            public byte role; // 0 none, 1 GOLD, 2 DANGER, 3 RIVAL
            public string text;
        }

        static Header header;
        static string outDir;
        static int frames, inputFrame = -1;
        static FrameRec[] frameRecs;

        static int streakCount = -1;
        static float streakLen, streakAngle, streakMinRho, streakInnerAlpha;
        static int streakCross;

        /// <summary>NIGHT20 (M8 v2): the speed lines report this frame's drawn streaks (count, the median screen length in px at
        /// the capture's size, the median screen angle in deg: 0 = horizontal right, 90 = up), once per frame before EndFrame;
        /// minRho: the drawn streaks' nearest pixel to the crosshair in the ellipse's units (x / (w / 2), y / (h / 2): 1 = the
        /// screen's edge midpoints; L1: >= 0.33), innerAlpha: the strongest drawn alpha inside rho 0.55 (L1: < 0.30), cross:
        /// the streaks whose screen segment crosses a live enemy's screen rect (L5: 0).</summary>
        public static void Streaks(int count, float medianLenPx, float medianAngleDeg, float minRho = -1, float innerAlpha = -1, int cross = -1)
        {
            if (!Recording) return;
            streakCount = count;
            streakLen = medianLenPx;
            streakAngle = medianAngleDeg;
            streakMinRho = minRho;
            streakInnerAlpha = innerAlpha;
            streakCross = cross;
        }
        static FxRec[] fxRecs; // frames x FeelFx.Count
        static EnemyRec[] enemyRecs;
        static UiRec[] uiRecs;
        static Vector2[] chunkRecs;
        static string[] eventRecs;
        static int enemyUsed, uiUsed, chunkUsed, eventUsed;
        static readonly List<Canvas> roots = new List<Canvas>();
        static readonly List<Graphic> graphics = new List<Graphic>(512);
        static readonly Dictionary<int, int> uiIds = new Dictionary<int, int>(1024);
        static readonly List<string> uiPaths = new List<string>(1024);
        static int sinceRescan;
        static Action<object> onHit, onFire;
        static EventBus bus;

        /// <summary>The frames recorded so far in this scenario (the next frame's index; the JPG of frame f is f%04d.jpg).</summary>
        public static int FrameCount => frames;

        /// <summary>Start a scenario's recording into `dir` (the scenario's folder). FeelClock should be running.</summary>
        public static void Begin(SurvivalGame g, Header h, string dir)
        {
            header = h;
            outDir = dir;
            Directory.CreateDirectory(dir);
            int max = UnityTuning.ProbeMaxFrames;
            frameRecs ??= new FrameRec[max];
            fxRecs ??= new FxRec[max * (int)FeelFx.Count];
            enemyRecs ??= new EnemyRec[max * UnityTuning.ProbeMaxEnemies];
            uiRecs ??= new UiRec[max * UnityTuning.ProbeMaxUi];
            chunkRecs ??= new Vector2[max * UnityTuning.ProbeMaxChunkPoints];
            eventRecs ??= new string[max * 16];
            frames = 0;
            inputFrame = -1;
            enemyUsed = uiUsed = chunkUsed = eventUsed = 0;
            uiIds.Clear();
            uiPaths.Clear();
            for (int i = 0; i < fxCount.Length; i++)
            {
                fxCount[i] = 0;
                fxAlpha[i] = 0;
            }
            chunkPtCount = chunkPtTotal = enemyCount = eventCount = 0;
            Rescan();
            bus = g.events;
            onHit ??= o =>
            {
                if (o is HitEvent e && e.owner == "player")
                {
                    Event(e.killed ? "kill" : "hit");
                    if (e.headshot) Event("headshot");
                }
            };
            onFire ??= _ => Event("shot");
            bus.On("hit", onHit);
            bus.On("fire", onFire);
            bus.On("punch", onFire);
            Weapons41Begin(bus); // NIGHT41 T2: the fired weapon's id per frame
            Recording = true;
        }

        /// <summary>Mark this frame as the scenario's input frame (the shot, the key press, the menu open).</summary>
        public static void MarkInput()
        {
            if (!Recording) return;
            if (inputFrame < 0) inputFrame = frames;
            Event("input");
        }

        /// <summary>Stop recording and write telemetry.jsonl; returns the frames written.</summary>
        public static int End()
        {
            if (!Recording) return 0;
            Recording = false;
            if (bus != null)
            {
                bus.Off("hit", onHit);
                bus.Off("fire", onFire);
                bus.Off("punch", onFire);
                Weapons41End(bus); // NIGHT41 T2
            }
            Write(Path.Combine(outDir, "telemetry.jsonl"));
            return frames;
        }

        static void Rescan()
        {
            roots.Clear();
            Canvas[] all = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                Canvas c = all[i];
                if (!c.isRootCanvas) continue;
                string n = c.gameObject.name;
                if (n == "FeelABChip" || n.StartsWith("FeelStrip", StringComparison.Ordinal) || n.StartsWith("FeelReel", StringComparison.Ordinal)) continue; // the harness's own
                roots.Add(c);
            }
            sinceRescan = 0;
        }

        static partial void RecordFrame(SurvivalGame g)
        {
            if (frames >= frameRecs.Length) return;
            int f = frames;
            ref FrameRec r = ref frameRecs[f];
            r = default;
            r.step = g.simStep;
            r.streakCount = streakCount; // NIGHT20: this frame's report (Streaks), -1 when none
            r.streakLenPx = streakLen;
            r.streakAngleDeg = streakAngle;
            r.streakMinRho = streakMinRho;
            r.streakInnerAlpha = streakInnerAlpha;
            r.streakCross = streakCross;
            streakCount = -1;
            r.steps = g.StepsThisFrame;
            r.held = g.HeldThisFrame;
            r.heldSteps = g.HeldStepsThisFrame;
            r.slowmo = (float)g.WorldScale;
            r.flow = g.flow;
            Weapons41Frame(g, out r.fired, out r.charge, out r.pump, out r.spin, out r.spinKind, out r.pad); // NIGHT41 T2
            Camera cam = g.cam;
            if (cam != null)
            {
                Transform t = cam.transform;
                Vector3 e = t.rotation.eulerAngles;
                r.fov = cam.fieldOfView;
                r.roll = Signed(e.z);
                r.pitch = -Signed(e.x);
                Vector3 p = t.position;
                r.cx = p.x;
                r.cy = p.y;
                r.cz = p.z;
                if (g.sim != null)
                {
                    Vec3 feet = g.sim.previous + (g.sim.current - g.sim.previous) * g.Alpha;
                    r.height = p.y - WebSpace.ToUnity(feet).y;
                    Vector3 v = WebSpace.ToUnity(g.mover.velocity) - WebSpace.ToUnity(new Vec3(0, 0, 0));
                    Vector3 fwd = t.forward;
                    fwd.y = 0;
                    fwd = fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward;
                    Vector3 right = new Vector3(fwd.z, 0, -fwd.x);
                    r.vFwd = Vector3.Dot(v, fwd);
                    r.vRight = Vector3.Dot(v, right);
                    r.vUp = v.y;
                    r.speed = new Vector2(v.x, v.z).magnitude;
                }
            }
            for (int i = 0; i < (int)FeelFx.Count; i++)
            {
                ref FxRec x = ref fxRecs[f * (int)FeelFx.Count + i];
                x.count = fxCount[i];
                if (fxCount[i] <= 0) continue;
                x.x0 = fxMinX[i];
                x.y0 = fxMinY[i];
                x.x1 = fxMaxX[i];
                x.y1 = fxMaxY[i];
                x.alpha = fxAlpha[i];
            }
            r.enemyStart = enemyUsed;
            for (int i = 0; i < enemyCount && enemyUsed < enemyRecs.Length; i++)
                enemyRecs[enemyUsed++] = new EnemyRec { id = enemyId[i], rect = enemyRect[i], flash = enemyFlash[i], anchor = enemyAnchor[i] };
            r.enemyCount = enemyUsed - r.enemyStart;
            r.chunkStart = chunkUsed;
            for (int i = 0; i < chunkPtCount && chunkUsed < chunkRecs.Length; i++) chunkRecs[chunkUsed++] = chunkPts[i];
            r.chunkCount = chunkUsed - r.chunkStart;
            r.chunkTotal = chunkPtTotal;
            r.eventStart = eventUsed;
            for (int i = 0; i < eventCount && eventUsed < eventRecs.Length; i++) eventRecs[eventUsed++] = eventNames[i];
            r.eventCount = eventUsed - r.eventStart;
            r.uiStart = uiUsed;
            ScanUi();
            r.uiCount = uiUsed - r.uiStart;
            frames++;
        }

        static float Signed(float deg) => deg > 180 ? deg - 360 : deg;

        static void ScanUi()
        {
            if (++sinceRescan >= UnityTuning.ProbeRescanFrames) Rescan();
            float sw = Screen.width, sh = Screen.height;
            int perFrame = 0;
            for (int c = 0; c < roots.Count; c++)
            {
                Canvas root = roots[c];
                if (root == null || !root.isActiveAndEnabled) continue;
                Camera cam = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;
                float rootScale = root.transform.lossyScale.x;
                if (rootScale <= 0) rootScale = 1;
                graphics.Clear();
                root.GetComponentsInChildren(false, graphics);
                for (int i = 0; i < graphics.Count; i++)
                {
                    if (perFrame >= UnityTuning.ProbeMaxUi || uiUsed >= uiRecs.Length) return;
                    Graphic gr = graphics[i];
                    if (!gr.enabled || gr is TMP_SubMeshUI) continue;
                    CanvasRenderer cr = gr.canvasRenderer;
                    if (cr == null || cr.cull) continue;
                    Color col = gr.color;
                    float a = col.a * cr.GetInheritedAlpha() * cr.GetAlpha();
                    if (a <= 0.01f) continue;
                    RectTransform rt = gr.rectTransform;
                    rt.GetWorldCorners(uiCorners);
                    float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
                    for (int k = 0; k < 4; k++)
                    {
                        Vector3 s = cam != null ? cam.WorldToScreenPoint(uiCorners[k]) : uiCorners[k];
                        if (s.x < x0) x0 = s.x;
                        if (s.y < y0) y0 = s.y;
                        if (s.x > x1) x1 = s.x;
                        if (s.y > y1) y1 = s.y;
                    }
                    if (x1 - x0 < 1 || y1 - y0 < 1 || x1 < 0 || y1 < 0 || x0 > sw || y0 > sh) continue;
                    var tmp = gr as TMP_Text;
                    if (tmp != null && (tmp.textInfo == null || tmp.textInfo.characterCount == 0)) continue; // NIGHT18: SetText leaves .text empty
                    int id = UiId(gr);
                    ref UiRec u = ref uiRecs[uiUsed++];
                    perFrame++;
                    u.id = id;
                    u.rect = Rect.MinMaxRect(x0, y0, x1, y1);
                    u.scale = rt.lossyScale.x / rootScale;
                    u.alpha = a;
                    u.color = col;
                    u.role = Role(col);
                    u.px = tmp != null ? tmp.fontSize * root.scaleFactor * 1080f / Math.Max(1, sh) : -1;
                    u.text = tmp != null ? tmp.GetParsedText() : null;
                }
            }
        }

        static int UiId(Graphic gr)
        {
            int key = gr.GetInstanceID();
            if (uiIds.TryGetValue(key, out int id)) return id;
            id = uiPaths.Count;
            uiIds[key] = id;
            // the path under the root canvas, built once per element
            var sb = new StringBuilder(gr.gameObject.name);
            Transform t = gr.transform.parent;
            while (t != null)
            {
                sb.Insert(0, t.gameObject.name + "/");
                Canvas cv = t.GetComponent<Canvas>();
                if (cv != null && cv.isRootCanvas) break;
                t = t.parent;
            }
            uiPaths.Add(sb.ToString());
            return id;
        }

        static byte Role(Color c)
        {
            if (!UiTheme.Ready) return 0;
            if (Near(c, UiTheme.Gold)) return 1;
            if (Near(c, UiTheme.Danger)) return 2;
            return 0;
        }

        static bool Near(Color a, Color b) => Mathf.Abs(a.r - b.r) < 0.02f && Mathf.Abs(a.g - b.g) < 0.02f && Mathf.Abs(a.b - b.b) < 0.02f;

        // --- the writer (between scenarios: allocation here is fine) ---------------------------------------------------------

        static readonly string[] FxNames = { "chunks", "burst", "ring", "killMarker", "hitMarker", "speedLines", "hitFlash", "sparks", "shed", "speedLinesClear", "gun", "gust", "climb", "shockwave" };
        static readonly string[] RoleNames = { null, "GOLD", "DANGER", "RIVAL" };

        static void Write(string path)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            float sh = Screen.height;
            var sb = new StringBuilder(1 << 20);
            using (var w = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                sb.Append("{\"type\":\"scenario\",\"name\":").Append(Q(header.name)).Append(",\"kind\":").Append(Q(header.kind))
                    .Append(",\"feelAB\":").Append(header.feelAB ? "true" : "false").Append(",\"seed\":").Append(header.seed)
                    .Append(",\"distance\":").Append(double.IsNaN(header.distance) ? "null" : header.distance.ToString("0.###", ci))
                    .Append(",\"target\":").Append(header.target >= 0 ? header.target.ToString(ci) : "null")
                    .Append(",\"inputFrame\":").Append(inputFrame)
                    .Append(",\"w\":").Append(Screen.width).Append(",\"h\":").Append(Screen.height)
                    .Append(",\"fps\":").Append(UnityTuning.ReelFps).Append(",\"simHz\":120")
                    .Append(",\"maxSpeed\":").Append(header.maxSpeed.ToString("0.###", ci)).Append(",\"frames\":").Append(frames).Append('}');
                w.WriteLine(sb.ToString());
                for (int f = 0; f < frames; f++)
                {
                    sb.Clear();
                    FrameRec r = frameRecs[f];
                    sb.Append("{\"type\":\"frame\",\"f\":").Append(f).Append(",\"t\":").Append((f / (double)UnityTuning.ReelFps).ToString("0.#####", ci))
                        .Append(",\"step\":").Append(r.step).Append(",\"steps\":").Append(r.steps)
                        .Append(",\"held\":").Append(r.held ? "true" : "false").Append(",\"heldSteps\":").Append(r.heldSteps)
                        .Append(",\"slowmo\":").Append(r.slowmo.ToString("0.###", ci)).Append(",\"flow\":").Append(Q(r.flow.ToString()))
                        .Append(",\"cam\":{\"fov\":").Append(r.fov.ToString("0.###", ci)).Append(",\"roll\":").Append(r.roll.ToString("0.###", ci))
                        .Append(",\"pitch\":").Append(r.pitch.ToString("0.###", ci)).Append(",\"height\":").Append(r.height.ToString("0.####", ci))
                        .Append(",\"x\":").Append(r.cx.ToString("0.###", ci)).Append(",\"y\":").Append(r.cy.ToString("0.###", ci)).Append(",\"z\":").Append(r.cz.ToString("0.###", ci))
                        .Append("},\"vel\":{\"fwd\":").Append(r.vFwd.ToString("0.###", ci)).Append(",\"right\":").Append(r.vRight.ToString("0.###", ci))
                        .Append(",\"up\":").Append(r.vUp.ToString("0.###", ci)).Append(",\"speed\":").Append(r.speed.ToString("0.###", ci)).Append('}');
                    sb.Append(",\"fired\":").Append(r.fired == null ? "null" : Q(r.fired)) // NIGHT41 T2 (FeelProbe.Weapons41.cs): the weapon fields
                        .Append(",\"charge\":").Append(r.charge.ToString("0.###", ci)).Append(",\"pump\":").Append(r.pump.ToString("0.###", ci))
                        .Append(",\"spin\":").Append(r.spin.ToString("0.###", ci)).Append(",\"spinKind\":").Append(r.spinKind)
                        .Append(",\"device\":").Append(r.pad ? "\"pad\"" : "\"kbm\"");
                    if (r.streakCount >= 0) // NIGHT20 (M8 v2): the speed lines' own report
                        sb.Append(",\"streaks\":{\"count\":").Append(r.streakCount).Append(",\"lenPx\":").Append(r.streakLenPx.ToString("0.#", ci)).Append(",\"angle\":").Append(r.streakAngleDeg.ToString("0.#", ci))
                            .Append(",\"minRho\":").Append(r.streakMinRho.ToString("0.###", ci)).Append(",\"innerAlpha\":").Append(r.streakInnerAlpha.ToString("0.###", ci)).Append(",\"cross\":").Append(r.streakCross).Append('}');
                    sb.Append(",\"enemies\":[");
                    for (int i = 0; i < r.enemyCount; i++)
                    {
                        EnemyRec e = enemyRecs[r.enemyStart + i];
                        if (i > 0) sb.Append(',');
                        sb.Append("{\"id\":").Append(e.id).Append(",\"rect\":");
                        RectJson(sb, e.rect, sh, ci);
                        sb.Append(",\"flash\":").Append(e.flash.ToString("0.###", ci));
                        if (!float.IsNaN(e.anchor.x)) sb.Append(",\"anchor\":[").Append(e.anchor.x.ToString("0.#", ci)).Append(',').Append((sh - e.anchor.y).ToString("0.#", ci)).Append(']');
                        sb.Append('}');
                    }
                    sb.Append("],\"fx\":{");
                    bool first = true;
                    for (int i = 0; i < (int)FeelFx.Count; i++)
                    {
                        FxRec x = fxRecs[f * (int)FeelFx.Count + i];
                        if (x.count <= 0) continue;
                        if (!first) sb.Append(',');
                        first = false;
                        sb.Append(Q(FxNames[i])).Append(":{\"rect\":");
                        RectJson(sb, Rect.MinMaxRect(x.x0, x.y0, x.x1, x.y1), sh, ci);
                        sb.Append(",\"n\":").Append(x.count).Append(",\"a\":").Append(x.alpha.ToString("0.###", ci)).Append('}');
                    }
                    sb.Append("},\"chunkPts\":[");
                    for (int i = 0; i < r.chunkCount; i++)
                    {
                        Vector2 p = chunkRecs[r.chunkStart + i];
                        if (i > 0) sb.Append(',');
                        sb.Append('[').Append(p.x.ToString("0.#", ci)).Append(',').Append((sh - p.y).ToString("0.#", ci)).Append(']');
                    }
                    sb.Append("],\"chunkTotal\":").Append(r.chunkTotal).Append(",\"ui\":[");
                    for (int i = 0; i < r.uiCount; i++)
                    {
                        UiRec u = uiRecs[r.uiStart + i];
                        if (i > 0) sb.Append(',');
                        sb.Append("{\"id\":").Append(u.id).Append(",\"path\":").Append(Q(u.id < uiPaths.Count ? uiPaths[u.id] : "?")).Append(",\"rect\":");
                        RectJson(sb, u.rect, sh, ci);
                        sb.Append(",\"scale\":").Append(u.scale.ToString("0.####", ci)).Append(",\"alpha\":").Append(u.alpha.ToString("0.###", ci))
                            .Append(",\"color\":\"#").Append(u.color.r.ToString("X2")).Append(u.color.g.ToString("X2")).Append(u.color.b.ToString("X2")).Append('"')
                            .Append(",\"role\":").Append(u.role == 0 ? "null" : Q(RoleNames[u.role]))
                            .Append(",\"px\":").Append(u.px < 0 ? "null" : u.px.ToString("0.##", ci))
                            .Append(",\"text\":").Append(u.text == null ? "null" : Q(u.text.Length > 64 ? u.text.Substring(0, 64) : u.text)).Append('}');
                    }
                    sb.Append("],\"events\":[");
                    for (int i = 0; i < r.eventCount; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append(Q(eventRecs[r.eventStart + i]));
                    }
                    sb.Append("]}");
                    w.WriteLine(sb.ToString());
                }
            }
        }

        /// <summary>A Unity screen rect (origin bottom-left) as an image rect [x, y, w, h] (origin top-left), rounded to 0.1 px.</summary>
        static void RectJson(StringBuilder sb, Rect r, float sh, CultureInfo ci)
        {
            sb.Append('[').Append(r.xMin.ToString("0.#", ci)).Append(',').Append((sh - r.yMax).ToString("0.#", ci)).Append(',')
                .Append(r.width.ToString("0.#", ci)).Append(',').Append(r.height.ToString("0.#", ci)).Append(']');
        }

        static string Q(string s)
        {
            if (s == null) return "null";
            var sb = new StringBuilder(s.Length + 2);
            sb.Append('"');
            foreach (char ch in s)
            {
                if (ch == '"' || ch == '\\') sb.Append('\\').Append(ch);
                else if (ch < 0x20) sb.Append("\\u").Append(((int)ch).ToString("x4"));
                else sb.Append(ch);
            }
            return sb.Append('"').ToString();
        }
    }
}
