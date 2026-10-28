// NIGHT54 builder D (design.n54.decor54, a VIEW row: NightRows.ViewOn(cfg, "decor54"); -skygrindOff54 turns it off): THE LEVELS' OWN COLOUR.
// GG 2026-10-20 (NIGHT54.md): "a lot of the decorative elements in several arenas are poorly executed and lack strong color direction". Every level
// drew its pure decor in the usable cyan (CLAUDE.md rule 8: cyan = player-usable) or in the boundary's pale violet-white, over walls of one flat
// tone. With the row on, each level reads its colour script (numbers.levels.<the level id after its "nNN-" prefix>: accentHex, wallBaseHex,
// wallUpperHex) and:
//   - THE DECOR takes the level's accent at numbers.accentScale of its own luminance, capped at numbers.usableCap of the brightest usable cyan
//     renderer under the map (the usable cyan stays the brightest accent in the room); only a cyan or boundary-white piece moves:
//       LightPlan33's floor-perimeter trims (fix kind "trim" in cyan / level49 / accentN: Fixtures33 groups them under FixKey, coloured here),
//       RoofView27's ceiling strips ("Strips boundary": the boundary's violet-white on a ceiling that bounds nothing; "Strips cyan": with lights49
//       on, a ceiling strip is cyan by its 1.5 m reach to a ride panel / pad / node, not as a route; without lights49 they are the route pair: kept),
//       DoorFrames29's static jamb / lintel / sill lines ("Door frame neon 29": on every door, behind you too; Look43's exit frame is the way on),
//       PropsView29's prop collars, console panels and floor strips ("Props pillar collar", "Props console panel", "Props flat floorStrip..."; NIGHT74 L: not a
//       usable floor mark, a floorStrip row with a job and no `decor: true`, which PropsView29Math.FlatKey names "flat floorMark" and DecorKind leaves cyan);
//   - THE WALLS: the map stone's side faces ("Stone" under "Survival map") mix numbers.toneMix of the level's wall tone into their vertex colour,
//     ramped by height above each face's own base (MeshKit's uv3.y; wallBase at the foot to wallUpper at numbers.bandM, the curve t^rampGamma):
//     a dark plinth under a lighter upper band, on a wall standing on a high floor too;
//   - KEPT CYAN (usable): SurvivalBuilder's ride strips, ledge rims and node cubes, the pads, Look43's ride strips / dashes and exit frames, RideTrim36,
//     the guide lamps under the exit lintels, the lenses lights49 put by a usable surface, the pickups, the grapple rings.
// Not moved (builder L's): every Light's colour (RoofView27's strip lights keep their cyan / pale light), the lenses, the beams.
// No geometry, collider, level file or sim change: the same objects and lights; colours and emission only (light colours stay builder L's).
// Self-installed after the first scene load (as LavaView30); one pass UnityTuning.Decor54SettleFrames frames after each map is seen and a last one at
// Decor54SecondPassS (both idempotent). THE PROOF: one Player log line per pass, prefix "[decor54]", printed with the row on or off (off: the counts
// only, nothing changed): the cyan decor pieces before and after (RED off: > 0; GREEN on: 0), the boundary-white decor, the usable cyan kept, the
// brightest decor vs the brightest usable cyan, the accent's least hue gap from the role tokens, and the walls' upper / base value ratio.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Decor54 : MonoBehaviour
    {
        const string P = "design.n54.decor54.numbers.";
        /// <summary>Fixtures33's group key for a decor trim (its colour is FixAccent).</summary>
        public const string FixKey = "decor54";

        /// <summary>A level's colour script (sRGB).</summary>
        public struct Script { public string key; public uint accentHex; public Color accent, wallBase, wallUpper; }

        /// <summary>The row's switch. -skygrindOffNN (NN <= 54) is read from the command line here too: the map and Fixtures33 are built before
        /// PlayerChecks sets NightRows.ForceOffFrom (the lead's n54-r2 off run moved 48 trims).</summary>
        public static bool On(GameConfig cfg) => cfg != null && !ArgsOff() && NightRows.ViewOn(cfg, "decor54");

        static int argsOff = -1;
        static bool ArgsOff()
        {
            if (argsOff < 0)
            {
                try { argsOff = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 54 ? 1 : 0; }
                catch (System.Exception) { argsOff = 0; }
            }
            return argsOff == 1;
        }

        // ==================== SurvivalBuilder's hook: the rims on the tops of the decor kinds (numbers.rimDecorKinds: room walls meet the ceiling there) ====
        static SurvivalMap rimMap;
        static bool rimOn;
        static Script rimScript;
        static readonly HashSet<string> rimKinds = new HashSet<string>();
        /// <summary>For the proof line: the rim boxes of a decor kind SurvivalBuilder saw on this map, and how many went to "Neon rims 54" (0 off).</summary>
        public static int RimsSeen, RimsMoved;

        /// <summary>Whether a rim box of `kind` goes to the accent mesh (row on, a script, the kind listed); counts either way.</summary>
        public static bool RimDecor(GameConfig cfg, SurvivalMap map, string kind)
        {
            if (map != rimMap)
            {
                rimMap = map;
                RimsSeen = RimsMoved = 0;
                rimKinds.Clear();
                if (cfg != null && cfg.Has(P + "rimDecorKinds")) foreach (string k in cfg.Strs(P + "rimDecorKinds")) rimKinds.Add(k);
                rimOn = On(cfg) && ScriptFor(cfg, map, out rimScript);
            }
            if (kind == null || !rimKinds.Contains(kind)) return false;
            RimsSeen++;
            if (!rimOn) return false;
            RimsMoved++;
            return true;
        }

        /// <summary>The accent material of the moved rims: a copy of the neon at accentScale x the neon's own luminance (under the usable rims).</summary>
        public static Material RimMaterial(Material neon, GameConfig cfg)
        {
            var m = new Material(neon) { name = "Neon rims 54" };
            Color c = neon.HasProperty("_BaseColor") ? neon.GetColor("_BaseColor") : Color.white;
            Color n = Recolour(rimScript.accent, Lum(c) * Num(cfg, "accentScale", 0.8f));
            n.a = c.a;
            m.SetColor("_BaseColor", n);
            return m;
        }

        /// <summary>The script key of a level id: the id after its "nNN-" prefix (n52-foundry -> foundry), else the id itself.</summary>
        public static string KeyOf(string levelId)
        {
            if (string.IsNullOrEmpty(levelId)) return "";
            int d = levelId.IndexOf('-');
            if (d > 1 && levelId[0] == 'n' && char.IsDigit(levelId[1])) return levelId.Substring(d + 1);
            return levelId;
        }

        /// <summary>The map's script; false when the level has none (a survival map, a boss arena: nothing changes there).</summary>
        public static bool ScriptFor(GameConfig cfg, SurvivalMap map, out Script s)
        {
            s = default;
            if (cfg == null || map == null || map.json == null) return false;
            string key = KeyOf(PostFx.LevelIdOf(map.json));
            string p = P + "levels." + key + ".";
            if (key.Length == 0 || !cfg.Has(p + "accentHex") || !cfg.Has(p + "wallBaseHex") || !cfg.Has(p + "wallUpperHex")) return false;
            s.key = key;
            s.accentHex = (uint)cfg.Num(p + "accentHex");
            s.accent = Look.Hex(s.accentHex);
            Decor55.Accent(cfg, key, ref s.accentHex, ref s.accent); // NIGHT55 C decor55 (Decor55.cs): the accent moved off the walls' own hue and the roles (row off: unchanged)
            s.wallBase = Look.Hex((uint)cfg.Num(p + "wallBaseHex"));
            s.wallUpper = Look.Hex((uint)cfg.Num(p + "wallUpperHex"));
            Colour56.Walls(cfg, key, ref s.wallBase, ref s.wallUpper); // NIGHT56 V colour56 (Colour56.cs): the level's wall ramp while on (row off: unchanged)
            return true;
        }

        static float Num(GameConfig cfg, string name, float dflt) => cfg != null && cfg.Has(P + name) ? (float)cfg.Num(P + name) : dflt;

        // ==================== Fixtures33's hook (the trims are grouped by colour into one mesh: only the builder can split them) ====================
        static bool fixOn;
        static Script fixScript;
        /// <summary>For the proof line: the trims Fixtures33 saw in cyan on this map, and how many moved to the accent (0 with the row off).</summary>
        public static int TrimsCyan, TrimsMoved, TrimsCyanMoved;
        static int trimsMap = -1;

        /// <summary>Called once at the top of Fixtures33.Build: resets the counts and reads the row and this map's script.</summary>
        public static bool BeginFixtures(GameConfig cfg)
        {
            SurvivalGame g = SurvivalGame.Instance;
            TrimsCyan = TrimsMoved = TrimsCyanMoved = 0;
            LensCyan = LensMoved = LensCyanMoved = 0;
            onSurfaceM = Num(cfg, "onSurfaceM", 0.3f);
            trimsMap = g != null && g.map != null ? g.map.GetHashCode() : -1;
            fixOn = On(cfg) && g != null && ScriptFor(cfg, g.map, out fixScript);
            return fixOn;
        }

        /// <summary>A fix's group colour: a floor-perimeter trim in cyan / level49 / accentN -> FixKey while the row is on; anything else (and every fix
        /// with the row off) as it came. Orange (danger) and white (L49's fall-edge boundary role) are never moved.</summary>
        public static string FixColour(LightPlan33.Fix f, string col, bool on)
        {
            if (f != null && f.kind == "lens" && col != null && (col == "cyan" || col == "level49" || col.StartsWith("accent", System.StringComparison.Ordinal)))
            {
                // the lit lenses (sconces, recessed strips, poles, floods): lights49 gave a lens cyan when it was within 1.5 m of a usable surface;
                // it keeps cyan only when it sits ON one (its box within numbers.onSurfaceM of a ride panel / climb step / pad / node)
                if (col == "cyan") LensCyan++;
                if (!on || OnUsable(f)) return col;
                LensMoved++;
                if (col == "cyan") LensCyanMoved++;
                return FixKey;
            }
            if (f == null || f.kind != "trim" || col == "orange" || col == "white") return col;
            if (col == "cyan") TrimsCyan++;
            if (!on || !(col == "cyan" || col == "level49" || col.StartsWith("accent", System.StringComparison.Ordinal))) return col;
            TrimsMoved++;
            if (col == "cyan") TrimsCyanMoved++;
            return FixKey;
        }

        /// <summary>For the proof line: the cyan lenses Fixtures33 saw on this map, the lenses moved, the cyan ones moved.</summary>
        public static int LensCyan, LensMoved, LensCyanMoved;
        static float onSurfaceM = 0.3f;

        /// <summary>A lens sits on a usable surface: its box within onSurfaceM of a usable box or point of lights49's read (LightPlan33.Cached49).
        /// No read (lights49 off): kept (cyan then is the plan's own, not a proximity role).</summary>
        static bool OnUsable(LightPlan33.Fix f)
        {
            LightPlan33.Surfaces49 s = LightPlan33.Cached49 != null ? LightPlan33.Cached49.surf : null;
            if (s == null) return true;
            foreach (LightPlan33.Box u in s.usable) if (u.Gap(f.box) <= onSurfaceM + 1e-9) return true;
            return false;
        }

        /// <summary>FixKey's colour (before Fixtures33's HDR, the trim's own): the accent at accentScale x the cyan token's luminance, so a moved trim
        /// sits under a usable lens of the same HDR tier.</summary>
        public static Color FixAccent(GameConfig cfg) => Recolour(fixScript.accent, Lum(Look.Hex(UnityTuning.Look49UsableHex)) * Num(cfg, "accentScale", 0.8f) * Decor55.TrimLum(cfg)); // NIGHT56 V: x decor55's / colour56's trimLum (1 with both off)

        // ==================== the pass ====================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Decor54") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Decor54>();
        }

        SurvivalGame game;
        SurvivalMap map;
        int frames, passes;
        float since;
        readonly HashSet<int> doneMats = new HashSet<int>();
        readonly HashSet<int> doneMeshes = new HashSet<int>();
        readonly HashSet<int> movedMats = new HashSet<int>();
        // the counts of this map (kept across its two passes)
        int cyanDecorBefore, whiteDecorBefore, moved, movedDoors, movedRoof, movedProps, wallMeshes, wallVerts;
        float wallRatioBefore = -1, wallRatioAfter = -1;

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.map == null) { game = null; map = null; return; }
            if (g != game || g.map != map)
            {
                game = g; map = g.map; frames = 0; passes = 0; since = 0;
                doneMats.Clear(); doneMeshes.Clear(); movedMats.Clear();
                cyanDecorBefore = whiteDecorBefore = moved = movedDoors = movedRoof = movedProps = wallMeshes = wallVerts = 0;
                wallRatioBefore = wallRatioAfter = -1;
                wallMesh = null; wallFilter = null; recheck = 0; retones = 0;
            }
            if (passes >= 2)
            {
                Recheck();
                // NIGHT72 C palette72: the row read every frame (D1's contract); a flip rewrites the stone, so the recheck's probe follows what is in it now
                if (Palette72.Tick(game.cfg, wallMesh, FeelClock.UnscaledDelta, out Color[] palNow) && probeIdx < palNow.Length) probeCol = palNow[probeIdx];
                return;
            }
            frames++;
            since += FeelClock.UnscaledDelta; // NIGHT63 D1: 0 in a same-run held frame
            // look43 drains the stone (35 % toward grey) on its own first Update; the wall tone goes on after it, so a level's tone is never drained
            // on one load and full on another (its draining copies the colours it finds: ours stay under its drain only if we were first)
            if (passes == 0 && Look43View.Instance != null && Look43Probe.On && Look43Probe.StoneMeshes == 0 && frames < UnityTuning.Decor54MaxWaitFrames) return;
            if ((passes == 0 && frames >= UnityTuning.Decor54SettleFrames) || (passes == 1 && since >= UnityTuning.Decor54SecondPassS)) Pass();
        }

        // the wall tone's guard: look43's SetOn(true) re-applies its drained copy of the stone (taken before the tone): seen by one vertex, re-toned
        Mesh wallMesh;
        MeshFilter wallFilter;
        Script wallScript;
        int probeIdx, recheck, retones;
        Color probeCol;
        readonly List<Color> probeList = new List<Color>();

        void Recheck()
        {
            if (wallMesh == null || wallFilter == null || retones >= UnityTuning.Decor54MaxRetones || ++recheck < UnityTuning.Decor54RecheckFrames) return;
            recheck = 0;
            wallMesh.GetColors(probeList);
            if (probeIdx < probeList.Count && probeList[probeIdx] == probeCol) return;
            retones++;
            Walls(game.cfg, wallFilter, wallScript, true);
            Debug.Log($"[decor54] {(map != null && map.json != null ? PostFx.LevelIdOf(map.json) : "")} walls re-toned after a stone reset ({retones}): upper / base value {wallRatioBefore:0.00} -> {wallRatioAfter:0.00}");
        }

        /// <summary>The decor kind of a renderer by its object's name (null: not decor this pass recolours).</summary>
        /// <summary>NIGHT74 L (R's finding, Decor54.cs:251 as it was): public for the test. "Props flat floorStrip..." is decor and moves to the level's accent; a usable floor mark (a floorStrip row with a job and no
        /// `decor: true`: THE MARK, THE RIDE'S END) is named "Props flat floorMark..." by PropsView29Math.FlatKey and is NOT matched here: it stays the player's cyan.</summary>
        public static string DecorKind(string name)
        {
            if (name.Contains("flat floorMark")) return null; // a usable mark: cyan, player-usable (rule 8), never recoloured
            if (name == "Strips boundary" || name == "Strips cyan") return "roof"; // lights49 colours a ceiling strip cyan by its 1.5 m reach to a ride panel, not as a route
            if (name == "Door frame neon 29") return "door";
            if (name.StartsWith("Props ", System.StringComparison.Ordinal) && (name.Contains("pillar collar") || name.Contains("console panel") || name.Contains("flat floorStrip"))) return "prop";
            return null;
        }

        void Pass()
        {
            passes++;
            GameConfig cfg = game.cfg;
            bool on = On(cfg);
            bool has = ScriptFor(cfg, map, out Script s);
            bool apply = on && has;
            string level = map.json != null ? PostFx.LevelIdOf(map.json) : "";
            GameObject rg = GameObject.Find("Survival map");
            if (rg == null) { Debug.Log($"[decor54] {level} pass {passes}: no 'Survival map' root"); return; }
            float scale = Num(cfg, "accentScale", 0.8f), cap = Num(cfg, "usableCap", 0.9f);
            bool roles49 = LightPlan33.On49(cfg); // without lights49 a cyan roof strip is RoofView27's route pair (the two nearest the exit): kept
            int usableCyan = 0, cyanDecorAfter = 0, whiteDecorAfter = 0;
            float maxUsable = 0, maxDecor = 0;
            Shader neon = game.mats != null && game.mats.neon != null ? game.mats.neon.shader : null;
            MeshRenderer[] all = rg.GetComponentsInChildren<MeshRenderer>(true);
            var decor = new List<MeshRenderer>();
            var kinds = new List<string>();
            var fixMats = new List<Material>();
            foreach (MeshRenderer r in all) // first the usable cyan (its brightest caps the moved decor), and the decor list
            {
                Material m = r.sharedMaterial;
                if (m == null || !m.HasProperty("_BaseColor")) continue;
                string name = r.gameObject.name;
                string kind = DecorKind(name);
                if (kind == "roof" && name == "Strips cyan" && !roles49) kind = null;
                Color c = m.GetColor("_BaseColor");
                if (name.StartsWith("Fixtures 33 " + FixKey, System.StringComparison.Ordinal)) { fixMats.Add(m); continue; } // coloured at build (FixAccent); capped below
                if (kind != null) { decor.Add(r); kinds.Add(kind); continue; }
                if (IsCyan(c) && (neon == null || m.shader == neon) && r.enabled && r.gameObject.activeInHierarchy) { usableCyan++; maxUsable = Mathf.Max(maxUsable, Lum(c)); }
            }
            foreach (Material m in fixMats) // the moved trims: Fixtures33 coloured them at their own HDR tier; the same cap as every moved piece
            {
                Color c = m.GetColor("_BaseColor");
                if (apply && maxUsable > 0 && doneMats.Add(m.GetInstanceID()) && Lum(c) > maxUsable * cap)
                {
                    c *= maxUsable * cap / Lum(c);
                    c.a = 1f;
                    m.SetColor("_BaseColor", c);
                }
                maxDecor = Mathf.Max(maxDecor, Lum(c));
            }
            var seen = new HashSet<int>(); // one count per material (a material shared by two pieces is one piece of colour)
            for (int i = 0; i < decor.Count; i++)
            {
                MeshRenderer r = decor[i];
                string kind = kinds[i];
                Material m = r.sharedMaterial;
                if (!seen.Add(m.GetInstanceID())) continue;
                Color c = m.GetColor("_BaseColor");
                bool mine = false; // moved by this row (this pass or the last)
                if (doneMats.Add(m.GetInstanceID()))
                {
                    bool cy = IsCyan(c), wh = IsBoundary(c);
                    if (cy) cyanDecorBefore++;
                    if (wh) whiteDecorBefore++;
                    if (apply && (cy || wh)) // only a cyan or boundary-white piece moves (a danger-orange floor strip stays)
                    {
                        float lum = Lum(c) * scale;
                        if (maxUsable > 0) lum = Mathf.Min(lum, maxUsable * cap);
                        Color n = Recolour(s.accent, lum);
                        n.a = c.a;
                        m.SetColor("_BaseColor", n);
                        c = n;
                        movedMats.Add(m.GetInstanceID());
                        moved++;
                        if (kind == "door") movedDoors++; else if (kind == "roof") movedRoof++; else movedProps++;
                    }
                }
                mine = movedMats.Contains(m.GetInstanceID());
                bool cyNow = IsCyan(c), whNow = IsBoundary(c);
                if (cyNow) cyanDecorAfter++;
                if (whNow) whiteDecorAfter++;
                if (mine || cyNow || whNow) maxDecor = Mathf.Max(maxDecor, Lum(c));
            }
            Transform stone = rg.transform.Find("Stone");
            if (stone != null && stone.TryGetComponent(out MeshFilter mf) && mf.sharedMesh != null && doneMeshes.Add(mf.sharedMesh.GetInstanceID()))
                Walls(cfg, mf, s, apply);
            if (apply && passes == 2) Palette72.BeginAccent(rg, s.key, s.accent); // NIGHT72 C palette72: the accent pieces are scanned after this pass (Palette72.Tick)
            bool trimsHere = map != null && trimsMap == map.GetHashCode();
            int trimCyanBefore = trimsHere ? TrimsCyan : -1, trimCyanAfter = trimsHere ? TrimsCyan - TrimsCyanMoved : -1;
            Color.RGBToHSV(s.accent, out float ah, out float asat, out _);
            string gap = has ? $"accent #{s.accentHex:x6} (hue {ah * 360f:0}, s {asat:0.00}, least role gap {LeastGap(ah * 360f):0} deg{(asat < UnityTuning.Decor54PaleSat ? ", pale" : "")})" : "no script for this level";
            string verdict = !apply ? (on ? "n/a (no script)" : "RED (row off: NIGHT53's look)") : (cyanDecorAfter == 0 && trimCyanAfter <= 0 && maxDecor < maxUsable ? "GREEN" : "RED");
            Debug.Log($"[decor54] {level} pass {passes} {(on ? "on" : "off")}: {gap}; cyan decor pieces {cyanDecorBefore} -> {cyanDecorAfter} (roof strips / door frames / props), " +
                      $"cyan floor trims {trimCyanBefore} -> {trimCyanAfter} (Fixtures33, {TrimsMoved} trims moved), cyan lenses {(trimsHere ? LensCyan : -1)} -> {(trimsHere ? LensCyan - LensCyanMoved : -1)} ({LensMoved} lenses moved), wall-top rims {RimsSeen} -> {RimsSeen - RimsMoved} cyan ({RimsMoved} moved), boundary-white decor {whiteDecorBefore} -> {whiteDecorAfter}; " +
                      $"moved {moved} (doors {movedDoors}, roof {movedRoof}, props {movedProps}); usable cyan renderers kept {usableCyan}; brightest decor {maxDecor:0.00} vs usable cyan {maxUsable:0.00}; " +
                      $"walls {wallMeshes} mesh, {wallVerts} side vertices, upper / base value {wallRatioBefore:0.00} -> {wallRatioAfter:0.00}; {verdict}");
        }

        /// <summary>The stone's side faces: numbers.toneMix of the height-ramped wall tone into each vertex colour (row on with a script); the value ratio
        /// (mean luminance of the side vertices in the top quarter of bandM over the bottom quarter) is measured either way.</summary>
        void Walls(GameConfig cfg, MeshFilter mf, Script s, bool apply)
        {
            Mesh mesh = mf.sharedMesh;
            if (!mesh.isReadable) return;
            Vector3[] v = mesh.vertices;
            Vector3[] nrm = mesh.normals;
            Color[] col = mesh.colors;
            if (col == null || col.Length != v.Length || nrm == null || nrm.Length != v.Length) return;
            float band = Mathf.Max(0.5f, Num(cfg, "bandM", 9f)), gamma = Num(cfg, "rampGamma", 1.6f), mix = Mathf.Clamp01(Num(cfg, "toneMix", 0.6f));
            Transform t = mf.transform;
            var face = new List<Vector4>();
            mesh.GetUVs(3, face); // MeshKit.Face: uv3.y = a vertex's height above its own face's bottom (a side face's v runs up)
            bool local = face.Count == v.Length;
            wallRatioBefore = Ratio(t, v, nrm, col, band, local ? face : null);
            wallMeshes++;
            int n = 0;
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 wn = t.TransformDirection(nrm[i]);
                if (Mathf.Abs(wn.y) >= UnityTuning.Decor54SideMaxNy) continue;
                n++;
                if (!apply) continue;
                float y = local ? face[i].y : t.TransformPoint(v[i]).y; // each face ramps from its own base (a wall on a 15 m spire floor too)
                float k = Mathf.Pow(Mathf.Clamp01(y / band), gamma);
                Color tone = Color.Lerp(s.wallBase, s.wallUpper, k);
                Color c = Color.Lerp(col[i], tone, mix);
                c.a = col[i].a;
                col[i] = c;
            }
            wallVerts += n;
            if (apply && n > 0)
            {
                // NIGHT72 C palette72 (Palette72.cs): the stone's floor and wall multipliers, written while the row reads on (the same array while it reads off)
                Color[] shownCols = Palette72.Offer(cfg, mesh, t, s.key, col, v, nrm);
                mesh.colors = shownCols;
                // the recheck's probe: the last side vertex (a stone reset, look43's SetOn re-applying its drained copy, rewrites it)
                for (int i = v.Length - 1; i >= 0; i--)
                    if (Mathf.Abs(t.TransformDirection(nrm[i]).y) < UnityTuning.Decor54SideMaxNy) { probeIdx = i; probeCol = shownCols[i]; break; }
                wallMesh = mesh; wallFilter = mf; wallScript = s;
            }
            wallRatioAfter = Ratio(t, v, nrm, col, band, local ? face : null);
        }

        static float Ratio(Transform t, Vector3[] v, Vector3[] nrm, Color[] col, float band, List<Vector4> face)
        {
            double hi = 0, lo = 0;
            int nh = 0, nl = 0;
            for (int i = 0; i < v.Length; i++)
            {
                if (Mathf.Abs(t.TransformDirection(nrm[i]).y) >= UnityTuning.Decor54SideMaxNy) continue;
                float y = (face != null ? face[i].y : t.TransformPoint(v[i]).y) / band;
                if (y >= 0.75f) { hi += Lum(col[i]); nh++; }
                else if (y <= 0.25f) { lo += Lum(col[i]); nl++; }
            }
            return nh > 0 && nl > 0 && lo > 1e-6 ? (float)((hi / nh) / (lo / nl)) : -1f;
        }

        static float Max3(Color c) => Mathf.Max(c.r, Mathf.Max(c.g, c.b));
        public static float Lum(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b; // public: Palette72's accent scan (and its test)

        /// <summary>The accent's hue and saturation at the luminance `lum` (accents brighter than cyan at equal channels, ivory 0.95 vs cyan 0.75,
        /// are scaled by luminance, not by their largest channel).</summary>
        public static Color Recolour(Color accent, float lum)
        {
            Color n = accent * (Mathf.Max(0f, lum) / Mathf.Max(1e-4f, Lum(accent)));
            n.a = 1f;
            return Colour56.Clamp(n); // NIGHT56 V colour56: the largest channel held at Colour56AccentMaxChannel while on (the hue survives the tonemap); off: n
        }

        static bool IsCyan(Color c)
        {
            float m = Max3(c);
            if (m <= 1e-4f) return false;
            Color.RGBToHSV(new Color(c.r / m, c.g / m, c.b / m), out float h, out float sat, out _);
            Color.RGBToHSV(Look.Hex(UnityTuning.Look49UsableHex), out float uh, out _, out _);
            return sat >= UnityTuning.Decor54CyanMinSat && Gap(h * 360f, uh * 360f) <= UnityTuning.Decor54CyanBandDeg;
        }

        static bool IsBoundary(Color c)
        {
            float m = Max3(c);
            if (m <= 1e-4f) return false;
            Color.RGBToHSV(new Color(c.r / m, c.g / m, c.b / m), out float h, out float sat, out _);
            Color.RGBToHSV(Look.Hex(UnityTuning.Look49BoundaryHex), out float bh, out _, out _);
            return sat < UnityTuning.Decor54BoundaryMaxSat && m > UnityTuning.Decor54BoundaryMinV && Gap(h * 360f, bh * 360f) <= UnityTuning.Decor54CyanBandDeg;
        }

        static float Gap(float a, float b)
        {
            float d = Mathf.Abs(a - b) % 360f;
            return d > 180f ? 360f - d : d;
        }

        /// <summary>The accent's least hue gap (deg) from the five role tokens (usable, danger, parry, RIVAL, boundary).</summary>
        static float LeastGap(float hueDeg)
        {
            float best = 360f;
            foreach (uint r in new[] { UnityTuning.Look49UsableHex, UnityTuning.Look49DangerHex, UnityTuning.Look49ParryHex, UnityTuning.Look49RivalHex, UnityTuning.Look49BoundaryHex })
            {
                Color.RGBToHSV(Look.Hex(r), out float h, out _, out _);
                best = Mathf.Min(best, Gap(hueDeg, h * 360f));
            }
            return best;
        }
    }
}
