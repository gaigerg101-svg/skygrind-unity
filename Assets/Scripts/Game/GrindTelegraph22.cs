// NIGHT22 U3-36.6 THE GRIND's telegraph (builder W; UNCOMPILED and UNSEEN: written without the Editor). ULTRAKILL's Cyber Grind
// announces what comes before it comes (RESEARCH.md R6); here, for the last design.n21.grind.telegraphTime s of the break and
// through the reshape, every pillar the next shape will LIFT wears a hollow frame at the height it is going to, in the enemy
// red-orange (CLAUDE.md rule 8: red-orange is danger: the floor is about to move under you). The pillars that stay or sink are
// unmarked, so the safe ground reads as the absence of a mark. Draw only: no new shader (the game's neon material), no audio.
// The rule is SurvivalSim.GrindTelegraphing / GrindRising / GrindNextHeights (Game/SurvivalSim.Grind21.cs).
//
// The lead wires it: SurvivalGame.N22InitViews:   grindTelegraph22 = new GrindTelegraph22(this);
//                    SurvivalGame.N22RenderViews: grindTelegraph22?.Render(worldDt, alpha);
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class GrindTelegraph22
    {
        readonly SurvivalGame game;
        readonly Renderer marks;
        readonly MeshFilter filter;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly MeshKit kit = new MeshKit();
        readonly Color danger;
        double[] builtFor; // the heights array the mesh was built from (the sim keeps one per break)
        float time;

        /// <summary>Marks are drawn now (the frame's count of frames the Player reel reads).</summary>
        public bool Showing { get; private set; }

        public GrindTelegraph22(SurvivalGame game)
        {
            this.game = game;
            danger = Look.C(game.cfg, "colors.enemy", UnityTuning.GrindMarkGlow);
            var root = new GameObject("Grind telegraph").transform;
            marks = DesignKit.Neon("grind marks", root, kit, game);
            filter = marks.GetComponent<MeshFilter>();
            marks.shadowCastingMode = ShadowCastingMode.Off;
            marks.enabled = false;
        }

        /// <summary>Once a frame, on the world's time (0 in hitstop).</summary>
        public void Render(double worldDt, double alpha)
        {
            time += (float)worldDt;
            SurvivalSim sim = game.sim;
            bool on = sim.GrindTelegraphing && sim.core != null;
            if (on && !ReferenceEquals(builtFor, sim.GrindNextHeights)) Rebuild(sim);
            if (marks.enabled != on) marks.enabled = on;
            Showing = on;
            if (!on) return;
            float pulse = UnityTuning.GrindMarkFloor + (1 - UnityTuning.GrindMarkFloor) * (0.5f + 0.5f * Mathf.Sin(time * Mathf.PI * 2 * UnityTuning.GrindMarkRate));
            DesignKit.Tint(marks, block, danger * pulse);
        }

        // One mesh of four thin bars per rising pillar, rebuilt once per break (the marks do not move: the target heights are fixed).
        void Rebuild(SurvivalSim sim)
        {
            builtFor = sim.GrindNextHeights;
            kit.Clear();
            bool[] rising = sim.GrindRising;
            CoreSpec s = sim.core.spec;
            if (rising != null && builtFor != null)
            {
                float edge = (float)s.cellSize * UnityTuning.GrindMarkInset, bar = UnityTuning.GrindMarkBar, half = edge * 0.5f - bar * 0.5f;
                for (int i = 0; i < rising.Length && i < builtFor.Length; i++)
                {
                    if (!rising[i]) continue;
                    int k = i % s.cells, r = i / s.cells;
                    Vector3 c = WebSpace.ToUnity(new Vec3(s.x0 + (k + 0.5) * s.cellSize, 0, s.z0 + (r + 0.5) * s.cellSize));
                    float y = (float)builtFor[i] + UnityTuning.GrindMarkLift;
                    kit.Cube(new Vector3(c.x - half, y, c.z), new Vector3(bar, bar, edge), Color.white);
                    kit.Cube(new Vector3(c.x + half, y, c.z), new Vector3(bar, bar, edge), Color.white);
                    kit.Cube(new Vector3(c.x, y, c.z - half), new Vector3(edge - 2 * bar, bar, bar), Color.white);
                    kit.Cube(new Vector3(c.x, y, c.z + half), new Vector3(edge - 2 * bar, bar, bar), Color.white);
                }
            }
            filter.sharedMesh = kit.Build("grind marks", filter.sharedMesh);
        }
    }
}
