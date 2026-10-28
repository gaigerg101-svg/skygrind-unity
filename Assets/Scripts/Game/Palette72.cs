// NIGHT72 builder C slate C2 (design.n72.palette72, a VIEW row: NightRows.ViewOn(cfg, "palette72") EVERY frame; off: -skygrindNoPalette72 or
// -skygrindRows72 palette72:0, Rows72.cs). GG 2026-10-26: "I asked previously about stuff related colors and animations and consistency and I just
// haven't seen it"; gg-proxy #2 (Research/2026-10-27-forge72/gg-proxy.md): each level is one tint (the spire olive green everywhere: floor, walls,
// ramps and pillars the same green), its walls carry dark-red stripes ("red is supposed to mean danger"), the kiln's floor is salmon (hue 17: the
// danger band), iceline and span are one blue / violet from floor to wall. With the row on:
//   - THE FLOOR: every level's walkable surface (the floor's tops, a ledge's top, a ramp, a pit's floor) is ONE warm-neutral language, a sand
//     (hue 36-42, S 0.25-0.27) at the level's own brightness: out of every role band, so a floor never wears the danger or the parry hue
//     (UnityTuning.Palette72FloorMul). A ramp is lifted against the flat floor beside it, a deck a little, a pit's floor is darkened.
//   - THE WALLS: each level's walls carry ITS hue at their own brightness (foundry oxide green, iceline navy, kiln plum, span indigo, spire bottle
//     green: UnityTuning.Palette72WallMul): floor and wall are two families, never one tint.
//   - THE ONE ACCENT: the level's accent pieces (decor54's moved trims, rims, door frames, roof strips, prop collars, the string course) wear ONE
//     accent per level, never a role hue: the spire's rose (hue 345, in the danger band) becomes a bone ivory (UnityTuning.Palette72Accent); colour56's
//     frieze (its second colour, the level's "key") takes the accent's hue at its own luminance.
// HOW (view only; no collider, level file or sim change): the stone's vertex colours are linear albedo (Stone.shader: albedo = vertex colour x light),
// so the floor and the wall are a per-vertex multiplier (UnityTuning.Palette72*Mul, calibrated on the baseline frames by Tools/night/n72-c-calibrate.py).
// Decor54.Walls hands this class the stone's colours after its wall tone (and yellow56's floor rule, folded in here so one writer owns the mesh:
// Yellow56Floors stands down on a held mesh); the row is read every frame (Decor54.Update -> Tick): the mesh is multiplied by the vertex's multiplier
// when the row reads on and divided by it when it reads off, so the held OFF frame of a same-run pair (SameRun63) is the plain stone with every other
// writer's edit kept, and the next ON frame is the frame it would have drawn. The accent pieces are found by their colour (a scan after decor54's
// second pass, as Colours63's) and flipped the same way; a colour someone else changed since the scan is left alone (Colours63's "lost").
// THE PROOF (L-089): one Player log line per map, "[palette72] <level> ...", and one per flip: the row's state, the multipliers, the vertex classes,
// the accent pieces found; the frames read by Tools/night/n72-c-values.py (floor / wall gap, top hue window, cyan first).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Palette72
    {
        const string Row = "palette72";
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public static bool On(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);

        /// <summary>For the reels and the tests: the last stone line, the flips since the map, the accent pieces found and moved.</summary>
        public static string Line = "palette72: not attached";
        public static int Flips, AccentFound, AccentLost;

        // ==================== the stone ====================
        static Mesh held, handled;
        // the stone's colours as decor54 (and, folded in, yellow56's floors) left them (offCols), as decor54 alone left them (rawCols: the palette's baseline), the
        // per-vertex palette multiplier (null: no palette for the level) and the per-vertex decor72 value structure (1: none); a flip composes the colours of the
        // two rows' states and writes them: every combination is exact, whatever order the rows flip in
        static Color[] offCols, rawCols;
        static Vector3[] mulPal;
        static float[] mulDec;
        static bool palShown, decShown;

        /// <summary>The stone mesh Offer handled (Yellow56Floors stands down on it: Offer folded its floor rule in, at decor54's pass).</summary>
        public static bool Holds(Mesh m) => handled != null && ReferenceEquals(handled, m);

        /// <summary>The level's row in the tables (-1: no palette: the stone keeps its colours).</summary>
        public static int LevelIndex(string key) => string.IsNullOrEmpty(key) ? -1 : System.Array.IndexOf(UnityTuning.Palette72Levels, key);

        /// <summary>The multiplier of a vertex by its class: a wall (a side or an underside) the wall's; a top the floor's, lifted on a ramp or a deck and
        /// darkened in a pit. `ny` is the vertex normal's world y, `y` its world height. Pure (EditMode: Palette72Tests).</summary>
        public static Vector3 Multiplier(int level, float ny, float y, out string cls)
        {
            int k = level * 3;
            if (level < 0 || k + 2 >= UnityTuning.Palette72FloorMul.Length) { cls = "none"; return Vector3.one; }
            if (ny < UnityTuning.Palette72FlatMinNy)
            {
                cls = "wall";
                return new Vector3(UnityTuning.Palette72WallMul[k], UnityTuning.Palette72WallMul[k + 1], UnityTuning.Palette72WallMul[k + 2]);
            }
            var f = new Vector3(UnityTuning.Palette72FloorMul[k], UnityTuning.Palette72FloorMul[k + 1], UnityTuning.Palette72FloorMul[k + 2]);
            if (y < UnityTuning.Palette72PitMaxY) { cls = "pit"; return f * UnityTuning.Palette72PitDark; }
            if (ny < UnityTuning.Palette72SlopeMaxNy) { cls = "slope"; return f * UnityTuning.Palette72SlopeLift; }
            if (y > UnityTuning.Palette72DeckMinY) { cls = "deck"; return f * UnityTuning.Palette72DeckLift; }
            cls = "floor";
            return f;
        }

        /// <summary>A vertex colour times its multiplier. No clamp: Mesh.SetColors(Color[]) stores Float32 (the n72-h0 Player logs show no decor54 re-tone: its
        /// probe compares the float it wrote with the one it reads back), and the shader's albedo may exceed 1. A flip writes the stored array, so it is exact.</summary>
        public static Color Scale(Color c, Vector3 m) => new Color(c.r * m.x, c.g * m.y, c.b * m.z, c.a);

        /// <summary>Decor54.Walls' hook (the stone, after its wall tone): classifies every vertex, folds yellow56's floor rule into `off` (in place), and
        /// returns the colours to write: `off` itself while the row reads off (or the level has no palette), else `off` x the multipliers.</summary>
        public static Color[] Offer(GameConfig cfg, Mesh mesh, Transform t, string key, Color[] off, Vector3[] v, Vector3[] nrm)
        {
            held = null;
            offCols = rawCols = null;
            mulPal = null;
            mulDec = null;
            palShown = decShown = false;
            Flips = 0;
            handled = mesh;
            // the palette's own colours are made from the stone as decor54 left it (the baseline the multipliers were calibrated on: yellow56 off); yellow56's floor
            // rule (S cap in the yellow arc) is folded into the OFF colours only: palette72's floors are sand at S 0.25-0.27, never the parry yellow it guards against
            Color[] raw = off.Length == nrm.Length ? (Color[])off.Clone() : off;
            YellowFold(cfg, off, v, nrm, t);
            int li = LevelIndex(key);
            if (off.Length != v.Length || nrm.Length != v.Length)
            {
                Line = $"[palette72] {key}: the stone's arrays do not match (stone kept)";
                Debug.Log(Line);
                return off;
            }
            // NIGHT72 C5 (decor72): a wall face's own value structure, from the face coordinates MeshKit put in uv3 (u, v up from the face's foot, width, height)
            var face = new List<Vector4>();
            mesh.GetUVs(3, face);
            bool hasFace = face.Count == v.Length;
            mulPal = li >= 0 ? new Vector3[v.Length] : null;
            mulDec = new float[v.Length];
            CrownRoom74.Begin(cfg); // NIGHT74 B crownroom74 (CrownRoom74.cs): THE CROWN room's wall multiplier, read once for this stone mesh
            int floor = 0, slope = 0, deck = 0, pit = 0, wall = 0, over = 0, structured = 0;
            for (int i = 0; i < v.Length; i++)
            {
                float ny = t.TransformDirection(nrm[i]).y, y = t.TransformPoint(v[i]).y;
                string cls = "wall";
                if (li >= 0)
                {
                    Vector3 m = Multiplier(li, ny, y, out cls);
                    if (cls == "wall") m = CrownRoom74.Wall(m, t.TransformPoint(v[i])); // NIGHT74 B crownroom74: inside THE CROWN's room the slate's own multiplier, not the spire's blue boost
                    mulPal[i] = m;
                    Color on = Scale(raw[i], m);
                    if (on.r > 1f || on.g > 1f || on.b > 1f) over++;
                }
                else cls = ny < UnityTuning.Palette72FlatMinNy ? "wall" : "floor";
                mulDec[i] = hasFace && ny < UnityTuning.Palette72FlatMinNy ? Decor72.FaceMul(face[i].w, face[i].y) : 1f;
                if (mulDec[i] != 1f) structured++;
                switch (cls) { case "floor": floor++; break; case "slope": slope++; break; case "deck": deck++; break; case "pit": pit++; break; default: wall++; break; }
            }
            CrownRoom74.End(key); // NIGHT74 B crownroom74: the proof line for this stone mesh
            held = mesh;
            offCols = (Color[])off.Clone();
            rawCols = raw;
            palShown = li >= 0 && On(cfg);
            decShown = Decor72.On(cfg);
            if (li >= 0)
            {
                int k = li * 3;
                Line = $"[palette72] {key} {(palShown ? "ON" : "off")}: floor x({UnityTuning.Palette72FloorMul[k]:0.00}, {UnityTuning.Palette72FloorMul[k + 1]:0.00}, {UnityTuning.Palette72FloorMul[k + 2]:0.00}), " +
                       $"wall x({UnityTuning.Palette72WallMul[k]:0.00}, {UnityTuning.Palette72WallMul[k + 1]:0.00}, {UnityTuning.Palette72WallMul[k + 2]:0.00}); stone vertices: floor {floor}, slope {slope}, deck {deck}, pit {pit}, wall {wall} ({over} over 1.0: kept, Float32)";
            }
            else Line = $"[palette72] {key}: no palette for this level (stone kept; vertices: floor {floor}, wall {wall})";
            Debug.Log(Line);
            Debug.Log($"[decor72] {key} faces: {structured} of {wall} wall vertices on a face of {UnityTuning.Decor72FaceMinH:0.0} m or more take a value structure (the row reads {(decShown ? "ON" : "off")})");
            return Compose(palShown, decShown);
        }

        /// <summary>The stone's colours for the two rows' states: the folded colours (both off), or the palette's x its multiplier (the palette on) x the face's value
        /// structure (decor72 on). Both off returns offCols itself.</summary>
        static Color[] Compose(bool pal, bool dec)
        {
            if (!pal && !dec) return offCols;
            var c = new Color[offCols.Length];
            for (int i = 0; i < c.Length; i++)
            {
                Color b = pal && mulPal != null ? Scale(rawCols[i], mulPal[i]) : offCols[i];
                if (dec && mulDec != null) { float d = mulDec[i]; b = new Color(b.r * d, b.g * d, b.b * d, b.a); }
                c[i] = b;
            }
            return c;
        }

        /// <summary>yellow56's floor rule (Yellow56.Floor: a floor vertex in the yellow arc over S floorS goes to floorS) on the colours Offer is handed,
        /// the same function Yellow56Floors would run at its frame 8; with the row on and this map's stone held, Yellow56Floors stands down. Row off: nothing.</summary>
        static void YellowFold(GameConfig cfg, Color[] col, Vector3[] v, Vector3[] nrm, Transform t)
        {
            if (!Yellow56.On(cfg) || col == null || col.Length != nrm.Length) return;
            int seen = 0, moved = 0;
            for (int i = 0; i < col.Length; i++)
            {
                if (t.TransformDirection(nrm[i]).y <= UnityTuning.Yellow56FloorMinNy) continue;
                seen++;
                Color c = col[i];
                if (Yellow56.Floor(cfg, ref c)) { col[i] = c; moved++; }
            }
            Yellow56Floors.Seen = seen;
            Yellow56Floors.Moved += moved;
            Yellow56Floors.Passes++;
            Debug.Log($"[yellow56] floors pass {Yellow56Floors.Passes}: floor vertices {seen}, moved off the yellow arc {moved} (folded into palette72's stone pass)");
        }

        // ==================== the accent ====================
        sealed class Piece
        {
            public Material m;
            public Color off, on;
            public bool lost;
        }

        static readonly List<Piece> pieces = new List<Piece>();
        static readonly HashSet<int> pieceIds = new HashSet<int>();
        static bool accentShown;
        static GameObject accentRoot;
        static string accentKey = "";
        static Color accentOff, accentOn;
        static float accentSince;
        static int scans;
        static Material frieze;
        static Color friezeOff;

        /// <summary>Colour56.Build's hook: the frieze's material and its own (key) colour. While the row reads on the frieze takes the accent's hue at its
        /// own luminance (one accent per level); the next accent scan adds it.</summary>
        public static void Frieze(Material m, Color keyColour)
        {
            frieze = m;
            friezeOff = keyColour;
        }

        /// <summary>Decor54.Pass' hook at its second pass: this map's accent pieces are scanned `AccentScanS` after it (and again at `AccentRescanS`).</summary>
        public static void BeginAccent(GameObject root, string key, Color accent)
        {
            pieces.Clear();
            pieceIds.Clear();
            accentShown = false;
            accentRoot = root;
            accentKey = key;
            accentOff = accent;
            int li = LevelIndex(key);
            uint hex = li >= 0 && li < UnityTuning.Palette72Accent.Length ? UnityTuning.Palette72Accent[li] : 0;
            accentOn = hex != 0 ? Look.Hex(hex) : accent;
            accentSince = 0;
            scans = 0;
            AccentFound = AccentLost = 0;
        }

        static void Scan()
        {
            scans++;
            if (accentRoot == null) return;
            Color.RGBToHSV(accentOff, out float ah, out float asat, out _);
            bool change = accentOn != accentOff; // an unchanged accent: its pieces keep their colours (only the frieze takes the accent's hue)
            int found = 0, friezes = 0;
            foreach (MeshRenderer r in accentRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material m = r.sharedMaterial;
                if (m == null || !m.HasProperty(BaseColorId) || !pieceIds.Add(m.GetInstanceID())) continue;
                Color c = m.GetColor(BaseColorId);
                bool isFrieze = ReferenceEquals(m, frieze);
                if (!isFrieze && (!change || !IsAccent(c, ah, asat))) continue;
                var p = new Piece { m = m, off = c, on = Decor54.Recolour(accentOn, Decor54.Lum(c)) };
                p.on.a = c.a;
                if (isFrieze) friezes++;
                pieces.Add(p);
                found++;
            }
            AccentFound = pieces.Count;
            Debug.Log($"[palette72] {accentKey} accent scan {scans}: {found} new pieces ({friezes} frieze) of {pieces.Count}; accent #{ColorUtility.ToHtmlStringRGB(accentOff)} -> #{ColorUtility.ToHtmlStringRGB(accentOn)}");
        }

        static bool IsAccent(Color c, float ah, float asat)
        {
            float mx = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            if (mx <= 1e-4f) return false;
            Color.RGBToHSV(new Color(c.r / mx, c.g / mx, c.b / mx), out float h, out float s, out _);
            float d = Mathf.Abs(h * 360f - ah * 360f) % 360f;
            if (d > 180f) d = 360f - d;
            return d <= UnityTuning.Palette72AccentHueDeg && s >= asat * UnityTuning.Palette72AccentSatShare;
        }

        static void ApplyAccent(bool on)
        {
            accentShown = on;
            foreach (Piece p in pieces)
            {
                if (p.lost || p.m == null) continue;
                Color cur = p.m.GetColor(BaseColorId), from = on ? p.off : p.on;
                if (cur != from) { p.lost = true; AccentLost++; continue; }
                p.m.SetColor(BaseColorId, on ? p.on : p.off);
            }
        }

        // ==================== every frame ====================
        /// <summary>Decor54.Update's hook (after its two passes), every frame: reads the row, flips the stone's multipliers and the accent pieces when it
        /// changed, runs the accent scans on decor54's unscaled clock. Returns true when it wrote the stone (`now`: its colours, for decor54's probe).</summary>
        public static bool Tick(GameConfig cfg, Mesh mesh, float dt, out Color[] now)
        {
            now = null;
            bool want = On(cfg);
            if (accentRoot != null)
            {
                accentSince += dt;
                if ((scans == 0 && accentSince >= UnityTuning.Palette72AccentScanS) || (scans == 1 && accentSince >= UnityTuning.Palette72AccentRescanS)) Scan();
                if (want != accentShown) ApplyAccent(want);
            }
            bool wantPal = want && mulPal != null, wantDec = Decor72.On(cfg);
            if (held == null || !ReferenceEquals(held, mesh) || offCols == null || (wantPal == palShown && wantDec == decShown)) return false;
            Color[] c = Compose(wantPal, wantDec);
            mesh.colors = c;
            palShown = wantPal;
            decShown = wantDec;
            Flips++;
            Debug.Log($"[palette72] {accentKey} stone flipped: palette72 {(wantPal ? "ON" : "off")}, decor72 faces {(wantDec ? "ON" : "off")} (flip {Flips}); accent pieces {pieces.Count} ({AccentLost} lost)");
            now = c;
            return true;
        }
    }
}
