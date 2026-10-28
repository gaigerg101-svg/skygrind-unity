// NIGHT56 builder V colour56 (design.n56.colour56, a VIEW row: NightRows.ViewOn(cfg, "colour56"); -skygrindOff56 (or any older off switch) turns it
// off, read from the command line here too as Decor54 / Decor55 do: the map is built before PlayerChecks sets NightRows.ForceOffFrom). Slate V1:
// GG "way too bland" three times (w16, 2026-10-19, 2026-10-20); critic-ggproxy #5 (decor on vs off cannot be told apart). Each level of 1-5 gets
// ITS OWN colour direction (numbers.levels.<key>, keyed as decor54's: the level id after its "nNN-" prefix), two saturated colours that are never
// a role hue (cyan 170-200, red-orange 5-35, yellow 45-70, magenta 285-330; the meter Tools/night/n56-colour.py checks them on the frames):
//   THE KEY (keyHex): the level's building colour, a FRIEZE: one unlit band along the room faces of every map box of numbers.kinds, from
//     UnityTuning.Colour56FriezeY to +Colour56FriezeH over the floor beside the wall (above the accent's string course at 2.7 m, above the eye and
//     the waist-high cyan ride strip), at the key's own value (an unlit copy of the neon: the hue on screen is the number's, whatever the level's
//     light tint; colors.md method 8, Valorant / Hyper Light Drifter: the landmark colour carries the level). The wall below and above keeps its
//     stone and its value structure (the trap GG named, T-112: a whole-frame tint is a FAIL, and nothing here touches the grade).
//   THE ACCENT (accentHex): colour56 OWNS the level accent: Decor55.Accent (Decor54.ScriptFor's existing hook) returns colour56's accent before
//     decor55's, so every decor54 accent piece (wall-top rims, floor trims, door frames, roof strips, prop collars, the moved lenses) and the
//     string course take it at decor54's / decor55's own luminance rules; the string course is built while colour56 is on even with decor55 off
//     (decor55's off switch still turns decor55's own accent off).
//   THE WALL TONE (optional wallBaseHex / wallUpperHex): Decor54.ScriptFor's one hook line (Colour56.Walls) swaps decor54's wall ramp for the
//     level's (the foundry's orange-brown ramp, hue 22-29, sat in the danger band on every wall: oxide-green iron instead).
// Row off: nothing here builds or changes; decor54 / decor55 exactly as NIGHT55. THE PROOF: one Player log line per map, prefix "[colour56]": the key
// and the accent (hue, S, V, the least gap to a role band), the frieze (faces, segments, metres, skipped), the wall tone; the a356 reel's frames
// (FeelReel.N56.Colour.cs) read by Tools/night/n56-colour.py.
// NIGHT57 builder V (slate V1-colour56: "cap the world first"): THE WORLD SATURATION CAP rides this row (Render/Cap57Pass.cs, numbers.cap): each
// level's world held under its cap outside the role hues, so the tells stay the most saturated thing and the key / accent sit under them. Set on
// each new map from Update (Cap57.ForMap); -skygrindOff57 keeps NIGHT56's colour56 (no cap).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Colour56 : MonoBehaviour
    {
        const string P = "design.n56.colour56.numbers.";

        public static bool On(GameConfig cfg) => cfg != null && !ArgsOff() && NightRows.ViewOn(cfg, "colour56");

        static int argsOff = -1;
        static bool ArgsOff()
        {
            if (argsOff < 0)
            {
                try { argsOff = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 56 ? 1 : 0; }
                catch (System.Exception) { argsOff = 0; }
            }
            return argsOff == 1;
        }

        /// <summary>The level's colour56 hex at numbers.levels.&lt;key&gt;.&lt;name&gt; (false: none, or the row off).</summary>
        public static bool Hex(GameConfig cfg, string key, string name, out uint hex)
        {
            hex = 0;
            if (!On(cfg) || string.IsNullOrEmpty(key) || !cfg.Has(P + "levels." + key + "." + name)) return false;
            hex = (uint)cfg.Num(P + "levels." + key + "." + name);
            return true;
        }

        /// <summary>Decor55.Accent's first check: with the row on and the level's accentHex present, the level's accent is colour56's.</summary>
        public static bool Accent(GameConfig cfg, string key, ref uint accentHex, ref Color accent)
        {
            if (!Hex(cfg, key, "accentHex", out uint h)) return false;
            accentHex = h;
            accent = Look.Hex(h);
            return true;
        }

        /// <summary>Decor54.Recolour's hook (every moved decor piece, rim and trim): while the row is on, the colour's largest channel is held at
        /// UnityTuning.Colour56AccentMaxChannel (its hue and saturation kept, its value lower). The r1 critic saw decor55's green-brass 72 render hue
        /// ~60 on the lit floor-edge strips: an HDR accent at the cyan's luminance runs its big channels into the tonemapper's shoulder, which
        /// squeezes them more than the small one and turns the hue toward its neighbour (violet 268 toward magenta, cobalt 222 toward cyan).</summary>
        public static Color Clamp(Color n)
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || !On(g.cfg)) return n;
            float m = Mathf.Max(n.r, Mathf.Max(n.g, n.b));
            // NIGHT57 V (the r1 greyscale guard: colour56 lifted the world's luminance p75 over the enemies and hazards, v1none 25/35 -> v1on 18/36):
            // the accent's largest channel lower (UnityTuning.Colour57AccentMaxChannel), the hue and saturation kept; -skygrindOff57: NIGHT56's
            float mc = Rim58.AccentMax(g.cfg, Cap57.Off57() ? UnityTuning.Colour56AccentMaxChannel : UnityTuning.Colour57AccentMaxChannel); // NIGHT58 V1: rim56's accent value
            if (m <= mc) return n;
            float a = n.a;
            n *= mc / m;
            n.a = a;
            return n;
        }

        /// <summary>Decor54.ScriptFor's hook: the level's wall ramp (base / upper) when colour56 has one for it.</summary>
        public static void Walls(GameConfig cfg, string key, ref Color wallBase, ref Color wallUpper)
        {
            // NIGHT57 V (the r1 greyscale guard): wallBaseHex57 / wallUpperHex57, the neutral walls at the LUMINANCE of decor54's own ramp (NIGHT56's
            // neutrals kept its value: a neutral grey at a violet's V is ~1.65x its luminance: span's walls lifted the world's p75); -skygrindOff57: NIGHT56's
            if (!Cap57.Off57() && Hex(cfg, key, "wallBaseHex57", out uint b57) && Hex(cfg, key, "wallUpperHex57", out uint u57))
            {
                wallBase = Look.Hex(b57);
                wallUpper = Look.Hex(u57);
                return;
            }
            if (Hex(cfg, key, "wallBaseHex", out uint b) && Hex(cfg, key, "wallUpperHex", out uint u))
            {
                wallBase = Look.Hex(b);
                wallUpper = Look.Hex(u);
            }
        }

        // ==================== the frieze: self-installed after the first scene load (as Decor55), built once per map ====================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Colour56") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Colour56>();
        }

        SurvivalGame game;
        SurvivalMap map;
        int frames;
        bool built;

        static GameObject frieze, friezeNoJob; // NIGHT72 C: the job segments' object and the no-job segments' (hidden while decor72 reads on)

        /// <summary>The a356 reel's A/B (FeelReel.N56.Colour.cs): the frieze hidden for one shot, so the meter reads its exact pixels by the difference.</summary>
        public static void SetFriezeVisible(bool on)
        {
            if (frieze != null && frieze.activeSelf != on) frieze.SetActive(on);
            SurvivalGame g = SurvivalGame.Instance;
            bool noJobOn = on && !(g != null && Decor72.On(g.cfg)); // the no-job segments follow the reel's switch only while decor72 draws them
            if (friezeNoJob != null && friezeNoJob.activeSelf != noJobOn) friezeNoJob.SetActive(noJobOn);
        }

        /// <summary>For the proof (the a356 reel): the last map's frieze and palette.</summary>
        public static int Segments, Faces, NoFloor, Buried, Short;
        public static double Metres;
        public static string Line = "", Level = "";
        public static bool Built;
        /// <summary>NIGHT57 V: the frieze colour's scale this map (1: NIGHT56's).</summary>
        public static float FriezeScale = 1f;

        static bool look49Done;
        public static string Look49Line = "";

        /// <summary>Round 3 (the r2 critic: "bring the world's p99.5 saturation under the tells' on every frame"; n56-r2: the foundry's and the spire's
        /// world at S 1.00 on the OFF sides too, look49's saturation +23 / +28 on a jade / emerald light): while the row is on, numbers.look49.&lt;theme&gt;
        /// (saturation, rimHex) replaces that level's entries of look49's tables (UnityTuning.V49Saturation / V50Saturation, V49RimHex) once, before
        /// any campaign map is themed (PostFx.ApplyTheme33 reads them per map). The same grade, the same light; two of its numbers per level.</summary>
        static void Look49(GameConfig cfg)
        {
            look49Done = true;
            if (!On(cfg)) return;
            var said = new List<string>();
            string[] themes = LightPlan33.Themes;
            for (int i = 0; i < themes.Length && i < UnityTuning.V49Saturation.Length; i++)
            {
                string k = P + "look49." + themes[i] + ".";
                if (cfg.Has(k + "saturation"))
                {
                    double was = UnityTuning.V49Saturation[i], now = cfg.Num(k + "saturation");
                    UnityTuning.V49Saturation[i] = now;
                    int s = PostFx.V50Slot(i);
                    if (s >= 0 && s < UnityTuning.V50Saturation.Length) UnityTuning.V50Saturation[s] = now;
                    said.Add($"{themes[i]} saturation {was:0} -> {now:0}");
                }
                if (cfg.Has(k + "rimHex") && i < UnityTuning.V49RimHex.Length)
                {
                    uint was = UnityTuning.V49RimHex[i], now = (uint)cfg.Num(k + "rimHex");
                    UnityTuning.V49RimHex[i] = now;
                    said.Add($"{themes[i]} rim #{was:x6} -> #{now:x6}");
                }
            }
            Look49Line = "[colour56] look49: " + (said.Count > 0 ? string.Join(", ", said) : "nothing replaced");
            Debug.Log(Look49Line);
        }

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (!look49Done && g != null && g.cfg != null) Look49(g.cfg);
            if (g == null || g.map == null) { game = null; map = null; Cap57.Value = 1f; return; }
            if (g != game || g.map != map)
            {
                game = g; map = g.map; frames = 0; built = false; Built = false;
                Cap57.ForMap(g.cfg, map.json != null ? PostFx.LevelIdOf(map.json) : ""); // NIGHT57 V: the world saturation cap for this level (Render/Cap57Pass.cs)
            }
            if (built || ++frames < UnityTuning.Colour56SettleFrames) return;
            built = true;
            Build();
        }

        void Build()
        {
            GameConfig cfg = game.cfg;
            string level = map.json != null ? PostFx.LevelIdOf(map.json) : "";
            string key = Decor54.KeyOf(level);
            Level = level;
            frieze = null;
            Segments = Faces = NoFloor = Buried = Short = 0;
            Metres = 0;
            if (!On(cfg)) { Line = $"[colour56] {level} off: nothing built (NIGHT55's look)"; Debug.Log(Line); return; }
            if (!Hex(cfg, key, "keyHex", out uint keyHex) || !Hex(cfg, key, "accentHex", out uint accHex))
            { Line = $"[colour56] {level} on: no palette for '{key}' (nothing built)"; Debug.Log(Line); return; }
            GameObject rg = GameObject.Find("Survival map");
            if (rg == null || game.mats == null || game.mats.neon == null) { Line = $"[colour56] {level}: no 'Survival map' root or neon material"; Debug.Log(Line); return; }
            var kinds = new HashSet<string>();
            if (cfg.Has(P + "kinds")) foreach (string k in cfg.Strs(P + "kinds")) kinds.Add(k);
            else kinds.Add("wall");
            var kit = new MeshKit();
            var kitNo = new MeshKit(); // NIGHT72 C decor72 (Decor72.cs): the segments with no job, a second object hidden while the row reads on
            Decor72Marks marks = Decor72.MarksFor(map);
            var tally = new Decor72.Tally();
            foreach (MapBox b in map.boxes)
            {
                if (b.kind == null || !kinds.Contains(b.kind)) continue;
                for (int f = 0; f < 4; f++) Face(kit, kitNo, marks, tally, b, f, map.boxes);
            }
            Color key56 = Look.Hex(keyHex);
            // NIGHT57 V (the r1 greyscale guard; n57-r1 a357 A/B: the frieze rendered at luma 0.23-0.42, over the world's p75 0.17-0.29, on 2-19 % of
            // the frame): the key's colour scaled (sRGB, hue and saturation kept) so its Rec. 709 luma is at most numbers.friezeLumaMax; -skygrindOff57:
            // NIGHT56's frieze
            FriezeScale = 1f;
            if (!Cap57.Off57() && cfg.Has(P + "friezeLumaMax"))
            {
                float lm = (float)cfg.Num(P + "friezeLumaMax"), l = 0.2126f * key56.r + 0.7152f * key56.g + 0.0722f * key56.b;
                if (l > lm && l > 1e-4f) { FriezeScale = lm / l; key56 = new Color(key56.r * FriezeScale, key56.g * FriezeScale, key56.b * FriezeScale, key56.a); }
            }
            if (Segments > 0)
            {
                var m = new Material(game.mats.neon) { name = "Colour56 frieze" };
                m.SetColor("_BaseColor", key56);
                frieze = Decor72.Objects(map, level, "frieze", "Colour56 frieze", rg.transform, kit, kitNo, m, tally, marks);
                friezeNoJob = Decor72.LastNoJob;
                Built = true;
                Palette72.Frieze(m, key56); // NIGHT72 C palette72: with the row on the frieze (the level's second colour) takes the one accent's hue
            }
            bool walls = Hex(cfg, key, "wallBaseHex", out uint wb) && Hex(cfg, key, "wallUpperHex", out uint wu);
            bool walls57 = !Cap57.Off57() && Hex(cfg, key, "wallBaseHex57", out uint wb57) && Hex(cfg, key, "wallUpperHex57", out uint wu57);
            Line = $"[colour56] {level} on: key {Word(keyHex)}, accent {Word(accHex)} (decor54's pieces and the string course take it); frieze at {UnityTuning.Colour56FriezeY:0.00}-{UnityTuning.Colour56FriezeY + UnityTuning.Colour56FriezeH:0.00} m over the floor on [{string.Join(",", kinds)}]: faces {Faces}, segments {Segments} ({Metres:0} m), skipped: no floor {NoFloor}, buried {Buried}, wall too short {Short}; wall tone {(walls57 ? "colour56's at decor54's luminance (NIGHT57)" : walls ? "colour56's" : "decor54's")}; NIGHT57: frieze luma scale {FriezeScale:0.00}, accent max channel {(Cap57.Off57() ? UnityTuning.Colour56AccentMaxChannel : UnityTuning.Colour57AccentMaxChannel):0.00}";
            Debug.Log(Line);
        }

        static string Word(uint hex)
        {
            Color.RGBToHSV(Look.Hex(hex), out float h, out float s, out float v);
            float hd = h * 360f;
            return $"#{hex:x6} (hue {hd:0}, s {s:0.00}, v {v:0.00}; least gap to a role band {RoleGap(hd):0} deg)";
        }

        /// <summary>Degrees from a hue to the nearest role band (danger 5-35, yellow 45-70, cyan 170-200, magenta 285-330); 0 inside one.</summary>
        public static float RoleGap(float hue)
        {
            float[,] bands = { { 5, 35 }, { 45, 70 }, { 170, 200 }, { 285, 330 } };
            float best = 360;
            for (int i = 0; i < 4; i++)
            {
                float lo = bands[i, 0], hi = bands[i, 1];
                if (hue >= lo && hue <= hi) return 0;
                best = Mathf.Min(best, Mathf.Min(Gap(hue, lo), Gap(hue, hi)));
            }
            return best;
        }

        static float Gap(float a, float b)
        {
            float d = Mathf.Abs(a - b) % 360f;
            return d > 180f ? 360f - d : d;
        }

        /// <summary>One vertical face of a box (0 +x, 1 -x, 2 +z, 3 -z; web space), cut into segments, each with the frieze over its own floor
        /// (Decor55.Face's walk at the frieze's height).</summary>
        void Face(MeshKit kitJob, MeshKit kitNo, Decor72Marks marks, Decor72.Tally tally, MapBox b, int f, List<MapBox> boxes)
        {
            Faces++;
            bool alongZ = f < 2;
            double nx = f == 0 ? 1 : f == 1 ? -1 : 0, nz = f == 2 ? 1 : f == 3 ? -1 : 0;
            double fixedC = f == 0 ? b.max.x : f == 1 ? b.min.x : f == 2 ? b.max.z : b.min.z;
            double a0 = alongZ ? b.min.z : b.min.x, a1 = alongZ ? b.max.z : b.max.x;
            double len = a1 - a0;
            if (len < UnityTuning.Colour56FriezeMinLen) return;
            int n = Mathf.Max(1, Mathf.CeilToInt((float)(len / UnityTuning.Colour56FriezeSegment)));
            double seg = len / n, probe = UnityTuning.Colour56FriezeProbe, out_ = UnityTuning.Colour56FriezeOut;
            for (int i = 0; i < n; i++)
            {
                double s0 = a0 + seg * i, s1 = s0 + seg, mid = (s0 + s1) / 2;
                double px = alongZ ? fixedC + nx * probe : mid, pz = alongZ ? mid : fixedC + nz * probe;
                double top = b.max.y - 0.05;
                if (InsideAny(boxes, px, top - 0.5, pz)) { Buried++; continue; }
                if (!game.world.RayIntersect(new Vec3(px, top, pz), new Vec3(0, -1, 0), out RayHit hit) || hit.distance > (b.max.y - b.min.y) + UnityTuning.Colour56FriezeFloorBelow)
                { NoFloor++; continue; }
                double floorY = top - hit.distance;
                double y0 = floorY + UnityTuning.Colour56FriezeY, y1 = y0 + UnityTuning.Colour56FriezeH;
                if (y1 > b.max.y - UnityTuning.Colour56FriezeTopClear || y0 < b.min.y) { Short++; continue; }
                if (InsideAny(boxes, px, (y0 + y1) / 2, pz)) { Buried++; continue; }
                double e0 = s0 - (i == 0 ? out_ : 0), e1 = s1 + (i == n - 1 ? out_ : 0);
                double c0 = fixedC, c1 = fixedC + (alongZ ? nx : nz) * out_;
                Vec3 lo = alongZ ? new Vec3(System.Math.Min(c0, c1), y0, e0) : new Vec3(e0, y0, System.Math.Min(c0, c1));
                Vec3 hi = alongZ ? new Vec3(System.Math.Max(c0, c1), y1, e1) : new Vec3(e1, y1, System.Math.Max(c0, c1));
                Vector3 ul = WebSpace.ToUnity(lo), uh = WebSpace.ToUnity(hi);
                MeshKit kit = Decor72.KitFor(marks, b, px, pz, kitJob, kitNo, tally); // NIGHT72 C decor72: the segment's job decides which object it is in
                kit.BoxUnity(Vector3.Min(ul, uh), Vector3.Max(ul, uh), Color.white, Color.white, Color.white, skipBottom: false);
                Segments++;
                Metres += seg;
            }
        }

        static bool InsideAny(List<MapBox> boxes, double x, double y, double z)
        {
            foreach (MapBox o in boxes)
                if (x > o.min.x && x < o.max.x && y > o.min.y && y < o.max.y && z > o.min.z && z < o.max.z) return true;
            return false;
        }
    }
}
