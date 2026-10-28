// NIGHT56 builder V yellow56 (design.n56.yellow56, a VIEW row: NightRows.ViewOn(cfg, "yellow56"); -skygrindOff56 or older turns it off, read from the
// command line here too). Slate V3: yellow means parry only (CLAUDE.md rule 8, DESIGN-RULES C-5 / E-4; critic-firsttime #4: "level 2 teaches me that
// yellow means 'punch it back'. Then the biggest yellow thing on screen is my own shotgun"). Measured first on NIGHT55's verbs37 x5 (n55-freeze) by
// Tools/night/n56-colour.py: yellow (45-70 deg, S > 0.5, V >= 0.15) outside the enemy rects and the HUD over 0.5 % of the frame on 66 of 180 frames
// (foundry 24 / 36: the floor-edge trims in decor54's brass accent, hue 41, clipped to ~55 on the frame; spire 30 / 36: the TORCH's plate 2-6 % and
// the walls' dark olive under the level's gold key; span 8, iceline 3, kiln 1). With the row on:
//   - THE GUNS: a viewmodel gun whose family colour (weapons.families, the web's) sits in numbers.gunHue (the amber-to-yellow arc: edge #ffb000 41,
//     blast #ffe14a 50, fire #ff8c1a 30 on the TORCH, whose plate renders ~45 under the key) at S > numbers.gunMinS takes that hue at numbers.gunS:
//     a pale brushed brass / champagne, the gun's warm metal kept, the saturated yellow left to the parry tells (Viewmodel.FamilyOf's one hook line;
//     the THUNDERCLAP's build reads FamilyOf too). Muzzle flashes, sparks, pickups and the radial keep the family colour (they are not the gun's plate).
//   - THE FLOOR TRIMS: a level whose decor accent (decor54's, when neither colour56 nor decor55 sets one) sits in the yellow arc takes
//     numbers.levels.<key>.accentHex (Decor55.Accent's chain: colour56, then decor55, then yellow56).
// Enemy and boss parts in yellow (THE CROWN's gems, the CROWN FIRE's yellow core, enemy visors) are GG's design: not changed (Research/2026-10-20-wave56/V.md).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Yellow56
    {
        const string P = "design.n56.yellow56.numbers.";

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

        public static bool On(GameConfig cfg) => cfg != null && !ArgsOff() && !Rim58.No("-skygrindNoYellow56") && NightRows.ViewOn(cfg, "yellow56"); // NIGHT58 V: the one-row off switch (Rim58.N58V.cs)

        static float N(GameConfig cfg, string k, float missing) => cfg != null && cfg.Has(P + k) ? (float)cfg.Num(P + k) : missing;

        /// <summary>For the a356 line / the proof: the gun families moved off the yellow arc since the row read (count) and the last one's words.</summary>
        public static int GunsMoved;
        public static string LastGun = "";

        /// <summary>Viewmodel.FamilyOf's hook: a family colour in the amber-yellow arc, saturated, comes back at numbers.gunS (its hue and value kept).</summary>
        public static Color Family(GameConfig cfg, string weapon, Color c)
        {
            if (!On(cfg)) return c;
            Color.RGBToHSV(c, out float h, out float s, out float v);
            float hd = h * 360f, lo = N(cfg, "gunHueLo", 28f), hi = N(cfg, "gunHueHi", 75f);
            if (hd < lo || hd > hi || s <= N(cfg, "gunMinS", 0.5f)) return c;
            Color o = Color.HSVToRGB(h, Mathf.Clamp01(N(cfg, "gunS", 0.32f)), v);
            o.a = c.a;
            GunsMoved++;
            LastGun = $"{weapon} hue {hd:0} s {s:0.00} -> s {N(cfg, "gunS", 0.32f):0.00}";
            return o;
        }

        /// <summary>A floor vertex colour (Stone.shader reads vertex colours) in numbers.floorHueLo..Hi above numbers.floorS: its S down to floorS.</summary>
        public static bool Floor(GameConfig cfg, ref Color c)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            float hd = h * 360f, fs = Mathf.Clamp01(N(cfg, "floorS", 1f));
            if (hd < N(cfg, "floorHueLo", 30f) || hd > N(cfg, "floorHueHi", 75f) || s <= fs) return false;
            Color o = Color.HSVToRGB(h, fs, v);
            o.a = c.a;
            c = o;
            return true;
        }

        /// <summary>Decor55.Accent's last link: a level accent in the yellow arc (numbers.accentHueLo..Hi) takes numbers.levels.&lt;key&gt;.accentHex.</summary>
        public static void Accent(GameConfig cfg, string key, ref uint accentHex, ref Color accent)
        {
            if (!On(cfg) || string.IsNullOrEmpty(key) || !cfg.Has(P + "levels." + key + ".accentHex")) return;
            Color.RGBToHSV(accent, out float h, out float s, out _);
            float hd = h * 360f;
            if (hd < N(cfg, "accentHueLo", 35f) || hd > N(cfg, "accentHueHi", 80f) || s < 0.3f) return;
            accentHex = (uint)cfg.Num(P + "levels." + key + ".accentHex");
            accent = Look.Hex(accentHex);
        }
    }

    /// <summary>NIGHT56 V yellow56, round 2 (n56-r2 a356 foundry: 6 / 6 entry and mid frames over 0.5 % yellow, 0.9-2.4 %, mostly V 0.25-0.4: the
    /// tan floors (hue ~41) the foundry's grade turns ochre-yellow): with the row on, the map stone's FLOOR vertices (world normal y > 0.7) whose
    /// colour sits in the yellow arc go to numbers.floorS (hue and value kept: a warm grey). Once per map after decor54's pass, and again when a
    /// probe vertex shows the stone was rewritten (decor54's recheck rule: look43's SetOn re-applies its copy). Row off: nothing runs.</summary>
    public sealed class Yellow56Floors : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Yellow56Floors") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Yellow56Floors>();
        }

        /// <summary>For the proof: the last map's floor vertices seen and moved.</summary>
        public static int Seen, Moved, Passes;
        SurvivalMap map;
        int frames, probe = -1;
        Mesh mesh;
        Color probeCol;

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.map == null || !Yellow56.On(g.cfg)) return;
            if (g.map != map) { map = g.map; frames = 0; mesh = null; probe = -1; Seen = Moved = Passes = 0; }
            frames++;
            if (frames < UnityTuning.Yellow56FloorFrames) return;
            if (mesh != null && Palette72.Holds(mesh)) return; // NIGHT72 C: palette72's stone pass owns the mesh (the floors' rule folded in)
            if (mesh != null && (frames % UnityTuning.Yellow56FloorRecheck) != 0) return;
            if (mesh != null && probe >= 0)
            {
                Color[] now = mesh.colors;
                if (probe < now.Length && now[probe] == probeCol) return; // still ours
            }
            Apply(g);
        }

        void Apply(SurvivalGame g)
        {
            GameObject rg = GameObject.Find("Survival map");
            Transform stone = rg != null ? rg.transform.Find("Stone") : null;
            if (stone == null || !stone.TryGetComponent(out MeshFilter mf) || mf.sharedMesh == null || !mf.sharedMesh.isReadable) return;
            Mesh m = mf.sharedMesh;
            if (Palette72.Holds(m)) { mesh = m; probe = -1; return; } // NIGHT72 C palette72 (Palette72.cs): the floors' rule was folded into decor54's stone pass (one writer owns the mesh)
            Vector3[] nrm = m.normals;
            Color[] col = m.colors;
            if (col == null || nrm == null || col.Length != nrm.Length) return;
            int seen = 0, moved = 0, last = -1;
            for (int i = 0; i < col.Length; i++)
            {
                if (mf.transform.TransformDirection(nrm[i]).y <= UnityTuning.Yellow56FloorMinNy) continue;
                seen++;
                Color c = col[i];
                if (Yellow56.Floor(g.cfg, ref c)) { col[i] = c; moved++; last = i; }
            }
            if (moved > 0) m.colors = col;
            mesh = m;
            probe = last;
            probeCol = last >= 0 ? col[last] : default;
            Seen = seen;
            Moved += moved;
            Passes++;
            Debug.Log($"[yellow56] floors pass {Passes}: floor vertices {seen}, moved off the yellow arc {moved}");
        }
    }
}
