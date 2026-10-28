// NIGHT52 builder U, skin52 (design.n52.skin52, a view row: no sim bit; NIGHT52.md section 4: "the enemies read as glowing jelly up close (the
// rim)"). GG owns enemy design: this changes the enemy SURFACE only (no model's shape, part or colour's meaning). With the row on, SKYGRIND/Enemy
// (Assets/Shaders/Enemy.shader) draws:
//   - the RIM CUT CLOSE UP: at Skin52RimNearM and nearer the rim is Skin52RimNearShare of its strength at Skin52RimNearPower (a thin edge light,
//     not a glowing gel); the material's rim from Skin52RimFarM on, the far boost unchanged (the enemy still pops at range);
//   - a RAMP-LIT BODY (NiloCat's URP toon lit, MIT: a smoothstep cel ramp on N.L) under a key light fixed to the view, the shadow side the body's
//     hue darker and cooler (Skin52Shade): a pill reads as a volume, not a flat sticker; the hit flash on top, unchanged;
//   - INK51'S OUTLINE GAPS CLOSED: the outline hull pushes along SMOOTHED normals (QuickOutline's technique, MIT: the normals of the vertices
//     that share a position averaged) baked into UV channel 3 of a copy of each enemy mesh (one copy per source mesh, made once), so the split
//     vertices of a flat-shaded corner (the cube heads, plates, horns) move together and the ink has no gap there.
// The shader's switch is a global (_Skin52.x): unset or 0 (the row off, -skygrindOff52, the a252 reel's ForceOff) draws exactly as NIGHT51; the
// baked copies carry the same vertices, normals and UVs as their sources (only UV3 added), so with the switch at 0 nothing on screen changes.
// Numbers: UnityTuning.U52.cs.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>skin52's statics for the a252 reel.</summary>
    public static class Skin52Probe
    {
        /// <summary>The row was read on this session (the meshes are baked as the enemies appear).</summary>
        public static bool Built;
        /// <summary>The reel's "before" frame: the shader's switch at 0 (NIGHT51's surface) with the row still built.</summary>
        public static bool ForceOff;
        /// <summary>The reel only: forces the row (null: the config's design.n52.skin52). Read when the enemies' look is made.</summary>
        public static bool? Force;
        /// <summary>The a252 reel's "flat" frame: skin52 with the ramp off (the rim cut and the ink only: the fallback's look), to measure the ramp alone.</summary>
        public static bool NoRamp;
        /// <summary>Meshes baked, renderers swapped to a baked copy, meshes skipped (not readable: their ink as NIGHT51), and across the baked
        /// meshes the vertices whose own normal differs from their position's smoothed one (the gap sites the bake closed).</summary>
        public static int Meshes, Renderers, Skipped, GapVerts;
        public static readonly List<string> Notes = new List<string>(); // pure .NET: safe in an initialiser
    }

    /// <summary>skin52's switch, its globals and the smoothed-normal bake.</summary>
    public static class Skin52
    {
        static bool built;
        static readonly HashSet<int> seen = new HashSet<int>(); // renderer instance ids already looked at (pure .NET)
        static readonly Dictionary<int, Mesh> baked = new Dictionary<int, Mesh>(); // source mesh instance id -> its baked copy (or itself when skipped)
        static readonly int Id = Shader.PropertyToID("_Skin52"), RimId = Shader.PropertyToID("_Skin52Rim"), KeyId = Shader.PropertyToID("_Skin52Key"),
            RampId = Shader.PropertyToID("_Skin52Ramp"), ShadeId = Shader.PropertyToID("_Skin52Shade");

        /// <summary>The row was read on (the bake runs).</summary>
        public static bool Built => built;

        /// <summary>The shader draws skin52 now (built, and not the reel's NIGHT51 frame).</summary>
        public static bool Live => built && !Skin52Probe.ForceOff;

        /// <summary>From EnemyLook's constructor (one per game): reads the row, clears the per-game sets, sets the globals.</summary>
        static bool ramp = true;

        /// <summary>The ramp draws (round 2): the config's optional design.n52.skin52.numbers.ramp (0 = the fallback, the rim's near cut only; absent
        /// or 1 = the ramp) and not the a252 reel's NoRamp frame.</summary>
        public static bool RampOn => ramp && !Skin52Probe.NoRamp;

        /// <summary>The config's ramp switch alone (the a252 reel judges the two tones only when it is on).</summary>
        public static bool RampConfigured => ramp;

        /// <summary>Round 4: the scorch mark's hot core glow under skin52 (EnemyViews.Scorch52.cs): the config's optional
        /// design.n52.skin52.numbers.scorchGlow, else UnityTuning.Skin52ScorchGlow.</summary>
        public static float ScorchGlow { get; private set; } = UnityTuning.Skin52ScorchGlow;

        /// <summary>Round 9: skin52 gives the scorch mark its hot core (scorchGlow 0 ships the fallback: NIGHT51's mark, the a252 check not judged).</summary>
        public static bool ScorchOn => ScorchGlow > 0;

        public static void Init(GameConfig cfg)
        {
            ramp = cfg == null || !cfg.Has("design.n52.skin52.numbers.ramp") || cfg.Num("design.n52.skin52.numbers.ramp") > 0.5;
            ScorchGlow = cfg != null && cfg.Has("design.n52.skin52.numbers.scorchGlow") ? (float)cfg.Num("design.n52.skin52.numbers.scorchGlow") : UnityTuning.Skin52ScorchGlow;
            // n52-r2's u2off: EnemyLook is made before PlayerChecks applies -skygrindOffNN to NightRows.ForceOffFrom (StartGame), so the switch is
            // also read from the command line here (Night51.On's way); the Editor and the harness have no switch: ViewOn alone decides there
            bool offSwitch = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 52;
            built = Skin52Probe.Force ?? (NightRows.ViewOn(cfg, "skin52") && !offSwitch);
            Skin52Probe.Built = built;
            seen.Clear();
            Apply();
        }

        /// <summary>The shader's globals for this state (the reel calls it after flipping ForceOff).</summary>
        public static void Apply()
        {
            bool on = Live;
            Shader.SetGlobalVector(Id, new Vector4(on ? 1f : 0f, UnityTuning.Skin52RimNearM, UnityTuning.Skin52RimFarM, UnityTuning.Skin52RimNearShare));
            Shader.SetGlobalVector(RimId, new Vector4(UnityTuning.Skin52RimNearPower, UnityTuning.Skin52RampFullM, UnityTuning.Skin52RampFlatM, 0));
            Shader.SetGlobalVector(KeyId, new Vector4(UnityTuning.Skin52KeyUp, UnityTuning.Skin52KeyToward, UnityTuning.Skin52KeySide, 0));
            Shader.SetGlobalVector(RampId, new Vector4(UnityTuning.Skin52RampMid, UnityTuning.Skin52RampSoft, UnityTuning.Skin52LitGain, 0));
            // the ramp off (the fallback, or the reel's NoRamp frame): the shadow side's colour = the lit side's (a flat body; the rim cut and the ink stay)
            float lit = UnityTuning.Skin52LitGain;
            Shader.SetGlobalVector(ShadeId, RampOn ? new Vector4(UnityTuning.Skin52ShadeR, UnityTuning.Skin52ShadeG, UnityTuning.Skin52ShadeB, 0) : new Vector4(lit, lit, lit, 0));
        }

        /// <summary>From EnemyLook.Tint (every drawn enemy part, before its block is set): the first time a renderer is seen with the row built, its
        /// MeshFilter's mesh is swapped for the baked copy (made once per source mesh).</summary>
        public static void Bake(Renderer r)
        {
            if (!built || r == null || !seen.Add(r.GetInstanceID())) return;
            var mf = r.GetComponent<MeshFilter>();
            if (mf == null) return;
            Mesh src = mf.sharedMesh;
            if (src == null) return;
            Mesh m = Smoothed(src);
            if (m != src)
            {
                mf.sharedMesh = m;
                Skin52Probe.Renderers++;
            }
        }

        /// <summary>The baked copy of `src`: its vertices, normals, UVs and triangles as they are, plus UV3 = each vertex's smoothed normal
        /// (QuickOutline's SmoothNormals: group the vertices by position, average their normals, normalise). `src` itself when it cannot be read.</summary>
        static Mesh Smoothed(Mesh src)
        {
            int key = src.GetInstanceID();
            if (baked.TryGetValue(key, out Mesh done) && done != null) return done;
            if (!src.isReadable)
            {
                baked[key] = src;
                Skin52Probe.Skipped++;
                if (Skin52Probe.Notes.Count < 12) Skin52Probe.Notes.Add(src.name + " (not readable: ink as NIGHT51)");
                return src;
            }
            Vector3[] pos = src.vertices, nrm = src.normals;
            if (nrm == null || nrm.Length != pos.Length)
            {
                baked[key] = src;
                Skin52Probe.Skipped++;
                return src;
            }
            var groups = new Dictionary<Vector3, Vector3>(pos.Length);
            for (int i = 0; i < pos.Length; i++)
            {
                groups.TryGetValue(pos[i], out Vector3 sum);
                groups[pos[i]] = sum + nrm[i];
            }
            var smooth = new List<Vector4>(pos.Length);
            int gaps = 0;
            for (int i = 0; i < pos.Length; i++)
            {
                Vector3 s = groups[pos[i]];
                s = s.sqrMagnitude > 1e-12f ? s.normalized : nrm[i];
                if (Vector3.Dot(s, nrm[i]) < UnityTuning.Skin52GapDot) gaps++;
                smooth.Add(new Vector4(s.x, s.y, s.z, 0f));
            }
            Mesh m = Object.Instantiate(src);
            m.name = src.name + " skin52";
            m.SetUVs(3, smooth);
            baked[key] = m;
            Skin52Probe.Meshes++;
            Skin52Probe.GapVerts += gaps;
            if (Skin52Probe.Notes.Count < 12) Skin52Probe.Notes.Add($"{src.name}: {pos.Length} verts, {groups.Count} positions, {gaps} gap verts");
            return m;
        }
    }
}
