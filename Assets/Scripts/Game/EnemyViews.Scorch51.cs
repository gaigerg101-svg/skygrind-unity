// NIGHT51 scorch51 (view only; off with -skygrindOff51): a grounded enemy's kill leaves a burnt splat on the floor under it, in its colour: the core
// glows for N51ScorchHeatS, the mark stays N51ScorchLifeS and fades over its last N51ScorchFadeS. The arena keeps the fight's record (ULTRAKILL's
// blood, DOOM's scorch); nothing in the sim. A pool of N51ScorchMax marks, the oldest replaced; cleared on a new run and on TITLE.
// Shader: Shaders/Resources/Scorch51.shader. Numbers: UnityTuning.N51.cs.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class EnemyLook
    {
        sealed class Scorch51Mark
        {
            public bool live;
            public float age, size, seed;
            public Vector3 at;
            public Quaternion turn;
            public Color tint;
            public readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        }

        Scorch51Mark[] scorch51;
        Material scorch51Mat;
        Mesh scorch51Quad;
        int scorch51Next;
        uint scorch51Hash = 0x9e3779b9u;
        bool scorch51Tried;
        static readonly int S51Tint = Shader.PropertyToID("_Tint"), S51Heat = Shader.PropertyToID("_Heat"), S51Fade = Shader.PropertyToID("_Fade"),
            S51Seed = Shader.PropertyToID("_Seed");

        /// <summary>Marks drawn since boot (the reels' line).</summary>
        public static int Scorch51Made;
        static int Scorch51Logged; // the first kills' line in the Player log (the reels read it)

        bool Scorch51Ready()
        {
            if (scorch51Mat != null) return true;
            if (scorch51Tried || !Night51.On) return false;
            scorch51Tried = true;
            Shader s = Shader.Find("SKYGRIND/Scorch51");
            if (s == null) { Debug.LogWarning("[scorch51] SKYGRIND/Scorch51 not found: no kill marks"); return false; }
            scorch51Mat = new Material(s) { name = "Scorch51" };
            scorch51Quad = new Mesh { name = "scorch51 quad" };
            scorch51Quad.SetVertices(new[] { new Vector3(-1, 0, -1), new Vector3(1, 0, -1), new Vector3(1, 0, 1), new Vector3(-1, 0, 1) });
            scorch51Quad.SetUVs(0, new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) });
            scorch51Quad.SetTriangles(new[] { 0, 2, 1, 0, 3, 2 }, 0);
            scorch51Quad.RecalculateBounds();
            scorch51 = new Scorch51Mark[UnityTuning.N51ScorchMax];
            for (int i = 0; i < scorch51.Length; i++) scorch51[i] = new Scorch51Mark();
            return true;
        }

        /// <summary>From Die: a grounded enemy's mark at its feet (a flier or a launched body leaves none).</summary>
        void Scorch51Kill(Enemy en, Vector3 feet, Color tint)
        {
            if (Scorch51Logged < 6)
            {
                Scorch51Logged++;
                Debug.Log($"[scorch51] kill {en?.kind} airborne {en?.Airborne} ready {Scorch51Ready()} feet {feet} made {Scorch51Made}");
            }
            if (en == null || en.Airborne || !Scorch51Ready()) return;
            scorch51Hash ^= scorch51Hash << 13;
            scorch51Hash ^= scorch51Hash >> 17;
            scorch51Hash ^= scorch51Hash << 5;
            Scorch51Mark m = scorch51[scorch51Next];
            scorch51Next = (scorch51Next + 1) % scorch51.Length;
            m.live = true;
            m.age = 0;
            m.at = feet + Vector3.up * UnityTuning.N51ScorchLift;
            m.size = Mathf.Max(UnityTuning.N51ScorchMin, (float)en.Radius * UnityTuning.N51ScorchRadii);
            m.seed = (scorch51Hash & 0xffff) / 65535f * 100f;
            m.turn = Quaternion.Euler(0, (scorch51Hash >> 16) / 65535f * 360f, 0);
            m.tint = tint.linear;
            Scorch51Made++;
        }

        /// <summary>Each frame (EnemyLook's frame, after the chunks): age and draw the live marks.</summary>
        void Scorch51Frame(float dt)
        {
            Scorch51Probe();
            if (scorch51 == null) return;
            for (int i = 0; i < scorch51.Length; i++)
            {
                Scorch51Mark m = scorch51[i];
                if (!m.live) continue;
                m.age += dt;
                if (m.age >= UnityTuning.N51ScorchLifeS) { m.live = false; continue; }
                float fade = Mathf.Clamp01((UnityTuning.N51ScorchLifeS - m.age) / UnityTuning.N51ScorchFadeS);
                float heat = 1 - Mathf.Clamp01(m.age / UnityTuning.N51ScorchHeatS);
                m.block.SetColor(S51Tint, m.tint);
                m.block.SetFloat(S51Heat, heat * heat);
                m.block.SetFloat(S51Fade, fade);
                m.block.SetFloat(S51Seed, m.seed);
                Scorch52Block(m); // NIGHT52 U skin52 round 9 (EnemyViews.Scorch52.cs): skin52's hot core on the block (row off: nothing)
                var rp = new RenderParams(scorch51Mat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false, matProps = m.block,
                    worldBounds = new Bounds(m.at, Vector3.one * (m.size * 2.6f + 1)) };
                Graphics.RenderMesh(rp, scorch51Quad, 0, Matrix4x4.TRS(m.at, m.turn, Vector3.one * m.size));
            }
        }

        // The reels' probe (-skygrind51ScorchProbe): three marks on the floor 4, 6 and 9 m ahead of the camera once a run plays (the reels' kill spots
        // sit behind boxes or past the capture's end), so a Player run shows the draw itself.
        int scorch51ProbeState = -1;
        void Scorch51Probe()
        {
            if (scorch51ProbeState == 0) return;
            if (scorch51ProbeState < 0) scorch51ProbeState = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-skygrind51ScorchProbe") >= 0 ? 1 : 0;
            if (scorch51ProbeState != 1 || game.cam == null || game.flow != Flow.Playing || !Scorch51Ready()) return;
            scorch51ProbeState = 0;
            Transform ct = game.cam.transform;
            Vector3 fwd = Vector3.ProjectOnPlane(ct.forward, Vector3.up).normalized;
            float floorY = ct.position.y - 1.6f;
            float[] ahead = { 4f, 6f, 9f };
            for (int i = 0; i < ahead.Length; i++)
            {
                Scorch51Mark m = scorch51[scorch51Next];
                scorch51Next = (scorch51Next + 1) % scorch51.Length;
                m.live = true;
                m.age = i * 0.6f; // one hot, one cooling, one cold
                m.at = ct.position + fwd * ahead[i] + ct.right * (i - 1) * 1.5f;
                m.at.y = floorY + UnityTuning.N51ScorchLift;
                m.size = 0.9f;
                m.seed = 13f * (i + 1);
                m.turn = Quaternion.Euler(0, 40f * i, 0);
                m.tint = new Color(1f, 0.42f, 0.21f).linear;
            }
            Debug.Log($"[scorch51] probe: 3 marks at floor y {floorY:0.00}");
        }

        void Scorch51Clear()
        {
            if (scorch51 == null) return;
            for (int i = 0; i < scorch51.Length; i++) scorch51[i].live = false;
        }
    }
}
