// NIGHT55 builder C decor55 (design.n55.decor55, a VIEW row: NightRows.ViewOn(cfg, "decor55"); -skygrindOff55 (or any older off switch) turns it
// off, read from the command line here too as Decor54 does: the map is built before PlayerChecks sets NightRows.ForceOffFrom). Slate P0-decor,
// OPEN O-07 / O-08: decor54's accents vanish on screen. Measured first on NIGHT54's own a452 entry frames (Tools/night/n55-decor-share.py):
// the accent hue IS the environment's hue on two levels: the foundry's pale brass (hue 41, s 0.39) classifies 24-34 % of each entry frame (the
// tan floors and umber walls are the same hue), the ice line's cobalt (222) 46-56 % (the slate-blue walls and floors); the kiln's patina (137)
// is its own hue but covers 0.16-0.25 %. An accent the walls already wear cannot read, and thin trims cannot cover 1 % of a frame. With the row on:
//   1. THE ACCENT moves off the environment and off the roles (numbers.levels.<key>.accentHex, read in place of decor54's by Decor54.ScriptFor's
//      one call-site line): the foundry an antique green-brass (hue 72: 28 deg off the parry yellow, O-08; ~40 deg off the tan), the ice line an
//      aurora green (150: off the slate blue by ~70, 36 off cyan), the kiln's patina kept at its hue and saturated (0.40 -> 0.58). Every decor54
//      piece (wall-top rims, floor trims, door frames, roof strips, prop collars, the moved lenses) takes it at decor54's own luminance rules.
//   2. THE STRING COURSE: one band (UnityTuning.Decor55Band*) along the room faces of every map box of a numbers.bandKinds kind, at a fixed
//      height over the floor beside it (a lintel-height course, above the head: the waist-high cyan ride strip keeps its height and its colour),
//      in the accent, unlit (a copy of the neon material, vertex colours white), its luminance a share of the usable cyan token's (usable cyan
//      stays the brightest class). A wall's face is cut into segments of Decor55BandSegment m, each with its own floor (a down ray beside it):
//      a face with no floor beside it (the level's outside) or a segment whose probe is inside another box (a buried face) gets none; a wall
//      too short for the band gets none. DESIGN-RULES V-4: decoration tells the world: a string course is the building's own line (the foundry's
//      brass course, the ice line's aurora line, the kiln's patina pipe), the same height through every room of a level.
// No collider, no level file, no sim change: one mesh per map under "Survival map" (destroyed with it). Off = NIGHT54's decor exactly.
// THE PROOF: one Player log line per map, prefix "[decor55]": the accent (hue, s, its least gap to the role tokens, the old one), the band
// (faces seen, segments built, metres, skipped: no floor / buried / short), the band's luminance vs the usable token's; and the a452 frames
// read by Tools/night/n55-decor-share.py.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Decor55 : MonoBehaviour
    {
        const string P = "design.n55.decor55.numbers.";

        public static bool On(GameConfig cfg) => cfg != null && !ArgsOff() && NightRows.ViewOn(cfg, "decor55");

        static int argsOff = -1;
        static bool ArgsOff()
        {
            if (argsOff < 0)
            {
                try { argsOff = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 55 ? 1 : 0; }
                catch (System.Exception) { argsOff = 0; }
            }
            return argsOff == 1;
        }

        /// <summary>Decor54.ScriptFor's hook: with the row on and numbers.levels.&lt;key&gt;.accentHex present, the level's accent is decor55's.</summary>
        public static void Accent(GameConfig cfg, string key, ref uint accentHex, ref Color accent)
        {
            if (Colour56.Accent(cfg, key, ref accentHex, ref accent)) return; // NIGHT56 V colour56 (Colour56.cs): colour56 owns the level accent while on
            if (!On(cfg) || string.IsNullOrEmpty(key) || !cfg.Has(P + "levels." + key + ".accentHex"))
            {
                Yellow56.Accent(cfg, key, ref accentHex, ref accent); // NIGHT56 V yellow56 (Yellow56.cs): decor54's accent off the yellow arc while on
                return;
            }
            accentHex = (uint)cfg.Num(P + "levels." + key + ".accentHex");
            accent = Look.Hex(accentHex);
        }

        /// <summary>NIGHT56 V (the r1 critic, P0-decor: the foundry's lit floor-edge strips rendered hue ~60, S 0.87, 16 deg from parry, and the
        /// accent's p95 luminance over usable cyan's on 3 of 12 entry frames): Decor54.FixAccent's luminance share for the moved floor trims, the
        /// pieces Fixtures33 draws at its HDR tier (a bright accent clips per channel toward its neighbour hue: green-brass 72 -> yellow 60).
        /// colour56's numbers.trimLum while colour56 is on, else decor55's while decor55 is on, else 1 (NIGHT54 / 55's trims).</summary>
        public static float TrimLum(GameConfig cfg)
        {
            const string C = "design.n56.colour56.numbers.trimLum";
            if (Colour56.On(cfg) && cfg.Has(C)) return Mathf.Clamp01((float)cfg.Num(C));
            if (On(cfg) && cfg.Has(P + "trimLum")) return Mathf.Clamp01((float)cfg.Num(P + "trimLum"));
            return 1f;
        }

        // ==================== the band: self-installed after the first scene load (as Decor54), built once per map ====================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Decor55") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Decor55>();
        }

        SurvivalGame game;
        SurvivalMap map;
        int frames;
        bool built;

        /// <summary>For the proof (the reel): the last map's band: segments built, metres, faces seen; skipped segments by cause.</summary>
        public static int Segments, Faces, NoFloor, Buried, Short;
        public static double Metres;
        public static string Line = "";

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.map == null) { game = null; map = null; return; }
            if (g != game || g.map != map) { game = g; map = g.map; frames = 0; built = false; }
            if (built || ++frames < UnityTuning.Decor55SettleFrames) return;
            built = true;
            Build();
        }

        void Build()
        {
            GameConfig cfg = game.cfg;
            string level = map.json != null ? PostFx.LevelIdOf(map.json) : "";
            string key = Decor54.KeyOf(level);
            bool c56 = Colour56.On(cfg); // NIGHT56 V: colour56 carries the string course in its accent even with decor55 off (-skygrindOff55 turns both off)
            bool on = On(cfg) || c56;
            Segments = Faces = NoFloor = Buried = Short = 0;
            Metres = 0;
            if (!Decor54.ScriptFor(cfg, map, out Decor54.Script s)) { Line = $"[decor55] {level}: no colour script for this level (nothing changes)"; Debug.Log(Line); return; }
            uint oldHex = cfg.Has("design.n54.decor54.numbers.levels." + key + ".accentHex") ? (uint)cfg.Num("design.n54.decor54.numbers.levels." + key + ".accentHex") : 0;
            Color.RGBToHSV(s.accent, out float ah, out float asat, out _);
            Color.RGBToHSV(Look.Hex(oldHex), out float oh, out float osat, out _);
            string accentWord = $"accent #{s.accentHex:x6} (hue {ah * 360f:0}, s {asat:0.00}; gaps: parry {Gap(ah * 360f, Hue(UnityTuning.Look49ParryHex)):0}, danger {Gap(ah * 360f, Hue(UnityTuning.Look49DangerHex)):0}, cyan {Gap(ah * 360f, Hue(UnityTuning.Look49UsableHex)):0}, magenta {Gap(ah * 360f, Hue(UnityTuning.Look49RivalHex)):0} deg) was decor54's #{oldHex:x6} (hue {oh * 360f:0}, s {osat:0.00}, parry gap {Gap(oh * 360f, Hue(UnityTuning.Look49ParryHex)):0})";
            if (!on) { Line = $"[decor55] {level} off: {accentWord}; no band (row off: NIGHT54's decor)"; Debug.Log(Line); return; }
            GameObject rg = GameObject.Find("Survival map");
            if (rg == null || game.mats == null || game.mats.neon == null) { Line = $"[decor55] {level}: no 'Survival map' root or neon material"; Debug.Log(Line); return; }
            var kinds = new HashSet<string>();
            if (cfg.Has(P + "bandKinds")) foreach (string k in cfg.Strs(P + "bandKinds")) kinds.Add(k);
            else kinds.Add("wall");
            var kit = new MeshKit();
            var kitNo = new MeshKit(); // NIGHT72 C decor72 (Decor72.cs): the segments with no job, a second object hidden while the row reads on
            Decor72Marks marks = Decor72.MarksFor(map);
            var tally = new Decor72.Tally();
            List<MapBox> boxes = map.boxes;
            foreach (MapBox b in boxes)
            {
                if (b.kind == null || !kinds.Contains(b.kind)) continue;
                for (int f = 0; f < 4; f++) Face(kit, kitNo, marks, tally, b, f, boxes);
            }
            float lum = Lum(Look.Hex(UnityTuning.Look49UsableHex)) * UnityTuning.Decor55BandLum;
            if (Segments > 0)
            {
                var m = new Material(game.mats.neon) { name = "Decor55 band" };
                Rim60.BandScaled = Rim60.On(cfg); // NIGHT60 V (B7): rim56's accent dim on the band too (Rim60.N60V.cs)
                Color c = Rim60.Band(Recolour(s.accent, lum), Rim60.BandScaled);
                m.SetColor("_BaseColor", c);
                Decor72.Objects(map, level, "string course", "Decor55 band", rg.transform, kit, kitNo, m, tally, marks);
            }
            Line = $"[decor55] {level} on{(c56 && !On(cfg) ? " (for colour56; decor55 itself off)" : "")}: {accentWord}; band at {UnityTuning.Decor55BandY:0.00}-{UnityTuning.Decor55BandY + UnityTuning.Decor55BandH:0.00} m over the floor on [{string.Join(",", kinds)}]: faces {Faces}, segments {Segments} ({Metres:0} m), skipped: no floor {NoFloor}, buried {Buried}, wall too short {Short}; band luminance {lum:0.00} vs the usable token's {Lum(Look.Hex(UnityTuning.Look49UsableHex)):0.00} (x{UnityTuning.Decor55BandLum:0.00})" +
                   $"; rim56's accent dim on the band {(Segments > 0 && Rim60.BandScaled ? $"x{UnityTuning.Rim60BandScale:0.00}" : "no")} (NIGHT60 B7)";
            Debug.Log(Line);
        }

        /// <summary>One vertical face of a box (0 +x, 1 -x, 2 +z, 3 -z; web space), cut into segments, each banded over its own floor.</summary>
        void Face(MeshKit kitJob, MeshKit kitNo, Decor72Marks marks, Decor72.Tally tally, MapBox b, int f, List<MapBox> boxes)
        {
            Faces++;
            bool alongZ = f < 2; // a +-x face runs along z
            double nx = f == 0 ? 1 : f == 1 ? -1 : 0, nz = f == 2 ? 1 : f == 3 ? -1 : 0;
            double fixedC = f == 0 ? b.max.x : f == 1 ? b.min.x : f == 2 ? b.max.z : b.min.z;
            double a0 = alongZ ? b.min.z : b.min.x, a1 = alongZ ? b.max.z : b.max.x;
            double len = a1 - a0;
            if (len < UnityTuning.Decor55BandMinLen) return;
            int n = Mathf.Max(1, Mathf.CeilToInt((float)(len / UnityTuning.Decor55BandSegment)));
            double seg = len / n, probe = UnityTuning.Decor55BandProbe, out_ = UnityTuning.Decor55BandOut;
            for (int i = 0; i < n; i++)
            {
                double s0 = a0 + seg * i, s1 = s0 + seg, mid = (s0 + s1) / 2;
                double px = alongZ ? fixedC + nx * probe : mid, pz = alongZ ? mid : fixedC + nz * probe;
                // the floor beside the face: a down ray from just under the box's top, outside the face
                double top = b.max.y - 0.05;
                if (InsideAny(boxes, px, top - 0.5, pz)) { Buried++; continue; }
                if (!game.world.RayIntersect(new Vec3(px, top, pz), new Vec3(0, -1, 0), out RayHit hit) || hit.distance > (b.max.y - b.min.y) + UnityTuning.Decor55BandFloorBelow)
                { NoFloor++; continue; }
                double floorY = top - hit.distance;
                double y0 = floorY + UnityTuning.Decor55BandY, y1 = y0 + UnityTuning.Decor55BandH;
                if (y1 > b.max.y - UnityTuning.Decor55BandTopClear || y0 < b.min.y) { Short++; continue; }
                if (InsideAny(boxes, px, (y0 + y1) / 2, pz)) { Buried++; continue; }
                // a thin slab proud of the face, its ends a slab-depth past the segment (closes the box's outer corners)
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

        static float Lum(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

        static Color Recolour(Color accent, float lum)
        {
            Color n = accent * (Mathf.Max(0f, lum) / Mathf.Max(1e-4f, Lum(accent)));
            n.a = 1f;
            return n;
        }

        static float Hue(uint hex)
        {
            Color.RGBToHSV(Look.Hex(hex), out float h, out _, out _);
            return h * 360f;
        }

        static float Gap(float a, float b)
        {
            float d = Mathf.Abs(a - b) % 360f;
            return d > 180f ? 360f - d : d;
        }
    }
}
