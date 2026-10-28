// NIGHT72 builder C slate C3 (design.n72.decor72, a VIEW row: NightRows.ViewOn(cfg, "decor72") EVERY frame; off: -skygrindNoDecor72 or
// -skygrindRows72 decor72:0, Rows72.cs). GG, asked five times: "random lava pits random pillars that serve no purpose" (2026-10-12), "cluttered with
// detail for the sake of detail" (2026-10-11), "more elements that are not just decoration" (2026-10-14), "dont just randomly plce stuff it needs
// intension and personality" (2026-10-17), "decorative elements in several arenas are poorly executed and lack strong color direction" (2026-10-20);
// gg-proxy #8: purple and dark-green horizontal bands along every foundry wall, purple panels in the iceline, red stripes in the spire, none of them
// marking a route, a ride wall or a hazard.
// THE STRIPES: two bands run along the room faces of EVERY wall box of a level: decor55's string course (the accent, 2.7 m over the floor) and
// colour56's frieze (the level's key, 3.4 m). With the row on a band segment stays only where it has a JOB (Decor72Marks, from the level's own data):
//   RIDE   the wall is a ride panel (campaign.boxes with ride: true, or a note naming a ride panel / wall: RideTrim36's rule): the band says "this is
//          the wall to ride", over the cyan trim's own two lines;
//   ROUTE  the segment is within UnityTuning.Decor72RouteReachM of a door's rect (campaign.doors): the way on is framed;
//   HAZARD the segment is within UnityTuning.Decor72HazardReachM of a pit's or the lava's rect: the drop is marked.
// Every other segment is hidden. HOW (view only; no collider, level file or sim change, so n46flow is byte-equal): Decor55.Build and Colour56.Build cut
// each band into two meshes, the segments with a job and the rest (drawn exactly as before while the row reads off); this class holds the second
// mesh's object per map and sets it inactive while the row reads on, every frame (D1's contract: the held OFF frame of a same-run pair shows the
// whole band; the next ON frame is the frame it would have drawn). THE PROOF (L-089): one Player log line per band per map, "[decor72] <level> <band>:
// segments N, with a job K (ride r, route t, hazard h), no job M: hidden while the row reads on", and Tools/night/n72-c-marks.cjs prints the same
// counts offline from the level json; the frames read by Tools/night/n72-c-values.py (blank face) and the spotter.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>A level's marks, read from its json: the ride panels, the doors and the hazards. Pure (no UnityEngine objects): Decor72Tests.</summary>
    public sealed class Decor72Marks
    {
        public struct Rect { public double x0, z0, x1, z1; }

        readonly List<BoxSpec> ride = new List<BoxSpec>();
        readonly List<Rect> doors = new List<Rect>(), hazards = new List<Rect>();
        double routeM, hazardM, eps;

        public int RidePanels => ride.Count;
        public int Doors => doors.Count;
        public int Hazards => hazards.Count;

        public static Decor72Marks From(Dictionary<string, object> json)
        {
            if (json == null || !json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return null;
            var m = new Decor72Marks { routeM = UnityTuning.Decor72RouteReachM, hazardM = UnityTuning.Decor72HazardReachM, eps = UnityTuning.Decor72RideEps };
            if (camp.TryGetValue("boxes", out object bo) && bo is List<object> boxes)
            {
                bool tagged = false; // a level that tags any box `ride: true` is read by the tag alone (RideTrim36's rule)
                foreach (object o in boxes)
                    if (o is Dictionary<string, object> d && d.TryGetValue("ride", out object rv) && rv is bool rt && rt) { tagged = true; break; }
                foreach (object o in boxes)
                {
                    var b = o as Dictionary<string, object>;
                    if (!RideTrim36.IsRidePanel(b, tagged) || !b.ContainsKey("min") || !b.ContainsKey("max")) continue;
                    m.ride.Add(new BoxSpec { min = PortData.V3(b["min"]), max = PortData.V3(b["max"]) });
                }
            }
            AddRects(camp, "doors", m.doors);
            AddRects(camp, "pits", m.hazards);
            if (json.TryGetValue("lava", out object lo) && lo is List<object> lavas)
                foreach (object o in lavas)
                    if (o is Dictionary<string, object> l) AddRect(l, m.hazards);
            return m;
        }

        static void AddRects(Dictionary<string, object> camp, string key, List<Rect> into)
        {
            if (!camp.TryGetValue(key, out object o) || !(o is List<object> list)) return;
            foreach (object e in list)
                if (e is Dictionary<string, object> d) AddRect(d, into);
        }

        static void AddRect(Dictionary<string, object> d, List<Rect> into)
        {
            if (!d.TryGetValue("rect", out object r) || !(r is List<object> a) || a.Count < 4) return;
            into.Add(new Rect { x0 = PortData.Num(a[0]), z0 = PortData.Num(a[1]), x1 = PortData.Num(a[2]), z1 = PortData.Num(a[3]) });
        }

        /// <summary>Distance (xz, m) from a point to a rect (0 inside).</summary>
        public static double Dist(Rect r, double px, double pz)
        {
            double dx = System.Math.Max(System.Math.Max(r.x0 - px, 0), px - r.x1), dz = System.Math.Max(System.Math.Max(r.z0 - pz, 0), pz - r.z1);
            return System.Math.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>The job of a band segment on wall box `b` at (px, pz) (the probe beside its face): "ride", "route", "hazard", or null (no job).</summary>
        public string Job(MapBox b, double px, double pz)
        {
            foreach (BoxSpec p in ride)
                if (b.min.x >= p.min.x - eps && b.max.x <= p.max.x + eps && b.min.y >= p.min.y - eps && b.max.y <= p.max.y + eps && b.min.z >= p.min.z - eps && b.max.z <= p.max.z + eps) return "ride";
            foreach (Rect r in doors) if (Dist(r, px, pz) <= routeM) return "route";
            foreach (Rect r in hazards) if (Dist(r, px, pz) <= hazardM) return "hazard";
            return null;
        }
    }

    public sealed class Decor72 : MonoBehaviour
    {
        public static bool On(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, "decor72");

        /// <summary>A band build's counts: the segments by job (the proof line and the tests).</summary>
        public sealed class Tally
        {
            public int ride, route, hazard, none, unmarked;
            public int Kept => ride + route + hazard + unmarked;
            public int Total => Kept + none;
            /// <summary>One segment's job ("ride", "route", "hazard" or null: none); returns whether it keeps its band.</summary>
            public bool Add(string job)
            {
                if (job == "ride") ride++;
                else if (job == "route") route++;
                else if (job == "hazard") hazard++;
                else { none++; return false; }
                return true;
            }
            /// <summary>A segment of a map with no campaign data: no marks to read, the band stays (decor72 changes nothing there).</summary>
            public void AddUnmarked() => unmarked++;
        }

        /// <summary>The one decision of a band segment: the kit it goes into (the jobs' or the no-job's), with the tally told. `marks` null: always the jobs' kit.</summary>
        public static MeshKit KitFor(Decor72Marks marks, MapBox b, double px, double pz, MeshKit kitJob, MeshKit kitNo, Tally t)
        {
            if (marks == null) { t.AddUnmarked(); return kitJob; }
            return t.Add(marks.Job(b, px, pz)) ? kitJob : kitNo;
        }

        /// <summary>For the reels and the tests: the last lines printed, the objects held, the flips.</summary>
        public static string Line = "decor72: not attached";
        public static int Flips;
        static readonly List<GameObject> noJob = new List<GameObject>();
        static bool shown = true; // the no-job objects are visible (the row reading off)
        static Dictionary<string, object> marksFor;
        static Decor72Marks marks;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Decor72") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Decor72>();
        }

        /// <summary>The level's marks (cached per level json); null for a map with no campaign data (every segment then keeps its band).</summary>
        public static Decor72Marks MarksFor(SurvivalMap map)
        {
            Dictionary<string, object> json = map != null ? map.json : null;
            if (!ReferenceEquals(json, marksFor)) { marksFor = json; marks = Decor72Marks.From(json); }
            return marks;
        }

        /// <summary>Builds the two band objects of one build (decor55's string course, colour56's frieze): the job segments as `name` (always drawn), the rest as
        /// `name (no job)` (hidden while the row reads on); prints the proof line. `kitNo` may be empty.</summary>
        public static GameObject Objects(SurvivalMap map, string level, string band, string name, Transform parent, MeshKit kitJob, MeshKit kitNo, Material mat, Tally t, Decor72Marks m)
        {
            SurvivalGame g = SurvivalGame.Instance;
            bool on = g != null && On(g.cfg);
            if (!ReferenceEquals(map, noJobMap)) { noJobMap = map; noJob.Clear(); shown = !on; Flips = 0; } // a new map: the old map's objects went with its root
            GameObject job = kitJob.Vertices > 0 ? MeshKit.Object(name, parent, kitJob.Build(name.ToLowerInvariant()), mat, shadows: false) : null;
            LastNoJob = null;
            if (kitNo.Vertices > 0)
            {
                GameObject o = MeshKit.Object(name + " (no job)", parent, kitNo.Build(name.ToLowerInvariant() + " no job"), mat, shadows: false);
                o.SetActive(shown); // follows the same state as the others (Update flips them together)
                noJob.Add(o);
                LastNoJob = o;
            }
            Line = $"[decor72] {level} {band}: segments {t.Total}, with a job {t.Kept} (ride {t.ride}, route {t.route}, hazard {t.hazard}{(t.unmarked > 0 ? ", unmarked " + t.unmarked : "")}), no job {t.none}: hidden while the row reads {(on ? "ON (now)" : "on")}" +
                   (m != null ? $"; marks: {m.RidePanels} ride panels, {m.Doors} doors, {m.Hazards} pits / lava" : "; no campaign data: every segment kept");
            Debug.Log(Line);
            return job;
        }

        /// <summary>The no-job object the last Objects call made (null: none).</summary>
        public static GameObject LastNoJob;
        static SurvivalMap noJobMap;

        // ==================== C5: a bare face's value structure ====================
        /// <summary>The value multiplier of a wall face's vertex at height `y` (m up from the face's own foot) on a face `faceH` tall: 1 under UnityTuning.Decor72FaceMinH;
        /// else the foot (relative height 0 .. PlinthT) rises from x PlinthMul to 1, and from UpperFromT to the top the colour rises (smoothstep) to x UpperMul.
        /// Pure (Decor72Tests). Applied by Palette72's stone pass while the row reads on.</summary>
        public static float FaceMul(float faceH, float y)
        {
            if (faceH < UnityTuning.Decor72FaceMinH) return 1f;
            float t = Mathf.Clamp01(y / faceH), m = 1f;
            if (t < UnityTuning.Decor72PlinthT) m *= Mathf.Lerp(UnityTuning.Decor72PlinthMul, 1f, t / UnityTuning.Decor72PlinthT);
            if (t > UnityTuning.Decor72UpperFromT)
            {
                float k = Mathf.Clamp01((t - UnityTuning.Decor72UpperFromT) / (1f - UnityTuning.Decor72UpperFromT));
                m *= Mathf.Lerp(1f, UnityTuning.Decor72UpperMul, k * k * (3f - 2f * k));
            }
            return m;
        }

        // ==================== C5: the route chevrons on ramps ====================
        /// <summary>The chevrons of one ramp: each a pair of arms (a quad each, four Unity-space corners, lifted off the slope) pointing up the slope, spaced along its
        /// centreline (one per UnityTuning.Decor72ChevronEveryM of slope, 1 to Decor72ChevronMax). `normal` is the slope's (up). Pure (Decor72Tests).</summary>
        public static List<Vector3[]> Chevrons(Vector3 lowL, Vector3 lowR, Vector3 topL, Vector3 topR, out Vector3 normal)
        {
            var quads = new List<Vector3[]>();
            Vector3 lowC = (lowL + lowR) * 0.5f, topC = (topL + topR) * 0.5f, up = topC - lowC;
            float len = up.magnitude;
            Vector3 across = lowR - lowL;
            float width = across.magnitude;
            normal = Vector3.up;
            if (len < 1e-3f || width < 1e-3f) return quads;
            up /= len;
            across /= width;
            normal = Vector3.Cross(across, up).normalized;
            if (normal.y < 0f) normal = -normal;
            int count = Mathf.Clamp(Mathf.FloorToInt((float)(len / UnityTuning.Decor72ChevronEveryM)), 1, UnityTuning.Decor72ChevronMax);
            float half = Mathf.Min(UnityTuning.Decor72ChevronWidthM * 0.5f, width * 0.3f), lean = UnityTuning.Decor72ChevronLeanM, arm = UnityTuning.Decor72ChevronArmM;
            for (int j = 0; j < count; j++)
            {
                Vector3 c = lowC + up * (len * (j + 1) / (count + 1)) + normal * UnityTuning.Decor72ChevronLiftM;
                Vector3 apex = c + up * (lean * 0.5f);
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector3 end = c - up * (lean * 0.5f) + across * (half * side);
                    Vector3 p = Vector3.Cross(normal, end - apex).normalized * (arm * 0.5f);
                    quads.Add(new[] { apex + p, apex - p, end - p, end + p });
                }
            }
            return quads;
        }

        static readonly List<GameObject> onMarks = new List<GameObject>(); // the route chevrons: drawn while the row reads on
        static bool marksShown;
        SurvivalMap seenMap;
        int seenFrames;
        bool chevronsBuilt;
        public static int Chevron72Ramps, Chevron72Count;

        void BuildChevrons(SurvivalGame g)
        {
            Chevron72Ramps = Chevron72Count = 0;
            string level = g.map.json != null ? PostFx.LevelIdOf(g.map.json) : "";
            GameObject rg = GameObject.Find("Survival map");
            if (MarksFor(g.map) == null || rg == null || g.mats == null || g.mats.neon == null || !Decor54.ScriptFor(g.cfg, g.map, out Decor54.Script s))
            {
                Debug.Log($"[decor72] {level} chevrons: none (no campaign data, no colour script or no map root)");
                return;
            }
            var kit = new MeshKit();
            foreach (WedgeSpec w in g.map.wedges)
            {
                Vec3[] c = ArenaMap.WedgeCorners(w);
                List<Vector3[]> quads = Chevrons(WebSpace.ToUnity(c[0]), WebSpace.ToUnity(c[1]), WebSpace.ToUnity(c[4]), WebSpace.ToUnity(c[5]), out Vector3 n);
                foreach (Vector3[] q in quads) { kit.Quad(q[0], q[1], q[2], q[3], n, Color.white); Chevron72Count++; }
                if (quads.Count > 0) Chevron72Ramps++;
            }
            if (kit.Vertices == 0) { Debug.Log($"[decor72] {level} chevrons: no ramps"); return; }
            var m = new Material(g.mats.neon) { name = "Decor72 chevrons" };
            Color accent = ChevronAccent(s.key, s.accent);
            m.SetColor("_BaseColor", ChevronColour(accent));
            GameObject o = MeshKit.Object("Decor72 chevrons", rg.transform, kit.Build("decor72 chevrons"), m, shadows: false);
            o.SetActive(marksShown);
            onMarks.Add(o);
            Debug.Log($"[decor72] {level} chevrons: {Chevron72Count} on {Chevron72Ramps} ramps (route marks: the way up), the level's accent #{ColorUtility.ToHtmlStringRGB(accent)} at its largest channel {UnityTuning.Decor72ChevronMaxChannel:0.00} (colour #{ColorUtility.ToHtmlStringRGB(ChevronColour(accent))}); drawn while the row reads on ({(marksShown ? "ON now" : "off now")})");
        }

        /// <summary>The accent a level's chevrons wear: the one palette72 chose for the level (UnityTuning.Palette72Accent: the spire's bone, whose colour56 accent is a rose in the
        /// danger band), else the level's own accent (Decor54.ScriptFor's). Whatever palette72 reads: the chevrons are decor72's own objects, so they are never the old look.</summary>
        public static Color ChevronAccent(string key, Color levelAccent)
        {
            int li = Palette72.LevelIndex(key);
            uint hex = li >= 0 && li < UnityTuning.Palette72Accent.Length ? UnityTuning.Palette72Accent[li] : 0;
            return hex != 0 ? Look.Hex(hex) : levelAccent;
        }

        /// <summary>The accent with its largest channel at UnityTuning.Decor72ChevronMaxChannel (hue and saturation kept). The n72-t4 pair showed the first cut near black: it took
        /// decor54's Recolour, whose Colour56.Clamp holds an accent piece's largest channel at 0.6 x 0.45 = 0.27 (rim56 / rim58: the dim accent that keeps the tells brightest), and
        /// a route mark must read: 0.8 here, the usable cyan's neon is still 2.0 (HDR).</summary>
        public static Color ChevronColour(Color accent)
        {
            float mx = Mathf.Max(accent.r, Mathf.Max(accent.g, accent.b));
            if (mx <= 1e-4f) return Color.white * UnityTuning.Decor72ChevronMaxChannel;
            Color c = accent * (UnityTuning.Decor72ChevronMaxChannel / mx);
            c.a = 1f;
            return c;
        }

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.cfg == null || g.map == null) return;
            bool on = On(g.cfg);
            if (!ReferenceEquals(g.map, seenMap)) { seenMap = g.map; seenFrames = 0; chevronsBuilt = false; onMarks.Clear(); marksShown = on; }
            if (!chevronsBuilt && ++seenFrames >= UnityTuning.Decor72ChevronFrames) { chevronsBuilt = true; BuildChevrons(g); }
            bool wantNo = !on; // the no-job band pieces are drawn while the row reads off
            if (noJob.Count > 0 && wantNo != shown)
            {
                shown = wantNo;
                Flips++;
                for (int i = noJob.Count - 1; i >= 0; i--)
                {
                    if (noJob[i] == null) { noJob.RemoveAt(i); continue; }
                    noJob[i].SetActive(wantNo);
                }
                Debug.Log($"[decor72] flipped {(wantNo ? "off: the whole band drawn" : "ON: only the segments with a job")} (flip {Flips}, {noJob.Count} no-job objects)");
            }
            if (on != marksShown)
            {
                marksShown = on;
                for (int i = onMarks.Count - 1; i >= 0; i--)
                {
                    if (onMarks[i] == null) { onMarks.RemoveAt(i); continue; }
                    onMarks[i].SetActive(on);
                }
            }
        }
    }
}
