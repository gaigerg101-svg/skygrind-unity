// NIGHT52 builder U, skin52 rounds 4, 6 and 9 (NIGHT52.md leftover 1: "the scorch mark's hot core ... never seen glowing in a frame"): scorch51's
// mark (EnemyViews.Scorch51.cs, Shaders/Resources/Scorch51.shader) under skin52's switch.
// Round 9, the cause (read from n52-r12's a252 frames, kiln and spire): NIGHT51's block DOES reach the draw. The OFF star's middle is the floor
// pushed to a saturated red (spire: floor 254 / 156-180 / 0-9 sRGB, the star's middle 255 / 46-85 / 0-16: the floor alone under the blotch would
// drop red to ~150): that is the core, drawn in the mark's colour, which Scorch51Kill takes from the dying view's first part, a near-black red
// (linear 0.10 / 0.01 / 0.00). A dark red core over a warm lit floor has no luma to add: rounds 4 (glow 6 on the block) and 6 read no rise for
// that reason. Round 6's own material per mark (dropped here) drew NO core at all (spire ON's middle 147 / 47 / 0: the floor x 0.3, the blotch
// alone); why the material's values did not reach the draw was not checked. So under skin52 the block (the path that draws) gets skin52's core:
// the tint the danger token's when the mark's own colour is darker than Skin52ScorchTintMin, pulled toward the hot yellow-orange by
// Skin52ScorchHotMix (the danger family, V-2: never white or cyan), the heat over Skin52ScorchHeatS, the glow Skin52.ScorchGlow (the config's
// optional design.n52.skin52.numbers.scorchGlow; 0 ships the fallback: NIGHT51's mark, the a252 scorch check not judged).
// With the row built but the a252 reel's ForceOff (or scorchGlow 0): NIGHT51's values, _Glow set back to the material's own (a block keeps a value
// once set). Row off / -skygrindOff52: nothing here runs (NIGHT51's draw exactly). The a252 reel reads the youngest mark's age, heat and colours.
// Numbers: UnityTuning.U52.cs.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class EnemyLook
    {
        static readonly int S52Glow = Shader.PropertyToID("_Glow");
        int scorch52Frame = -1;

        /// <summary>The youngest live mark this frame: its age (s) and the heat it was drawn with (-1: none).</summary>
        public static float Scorch52Age = -1f, Scorch52Heat = -1f;
        /// <summary>The youngest mark's colour as Scorch51Kill gave it, and the core's colour it was drawn with (linear; the reel prints both).</summary>
        public static Color Scorch52Tint, Scorch52Core;
        /// <summary>Mark frames given skin52's core on their block since boot (the reel reads the count per side).</summary>
        public static int Scorch52Drawn;

        /// <summary>From Scorch51Frame after it set the mark's block to NIGHT51's values: under skin52 (and scorchGlow above 0) the block's tint,
        /// heat and glow become skin52's hot core; built but not live: NIGHT51's, the glow the material's own.</summary>
        void Scorch52Block(Scorch51Mark m)
        {
            if (!Skin52.Built || scorch51Mat == null) return;
            float heat;
            Color core;
            float glow53 = Scorch53.Glow(Cfg); // NIGHT53 U polish53 (T-120, Polish53.cs): the core's glow when scorchCore is on (0: NIGHT52's rule)
            if (Skin52.Live && (Skin52.ScorchOn || glow53 > 0))
            {
                float t = 1 - Mathf.Clamp01(m.age / UnityTuning.Skin52ScorchHeatS);
                heat = t * t;
                Color hot = new Color(UnityTuning.Skin52ScorchHotR, UnityTuning.Skin52ScorchHotG, UnityTuning.Skin52ScorchHotB, 1f);
                // the dying view's first part is a near-black red on a husk: the core is the enemy's heat, so the danger token's colour
                Color baseT = m.tint.maxColorComponent < UnityTuning.Skin52ScorchTintMin ? Look.Hex(UnityTuning.Look49DangerHex).linear : m.tint;
                core = Color.Lerp(baseT, hot, UnityTuning.Skin52ScorchHotMix);
                core.a = 1f;
                m.block.SetColor(S51Tint, core);
                m.block.SetFloat(S51Heat, heat);
                m.block.SetFloat(S52Glow, glow53 > 0 ? glow53 : Skin52.ScorchGlow);
                Scorch52Drawn++;
                if (glow53 > 0) Scorch53.Drawn++;
            }
            else
            {
                float t = 1 - Mathf.Clamp01(m.age / UnityTuning.N51ScorchHeatS);
                heat = t * t; // as Scorch51Frame set it
                core = m.tint;
                m.block.SetFloat(S52Glow, scorch51Mat.GetFloat(S52Glow)); // the material's own glow: NIGHT51's draw (it never set _Glow)
            }
            if (scorch52Frame != Time.frameCount) { scorch52Frame = Time.frameCount; Scorch52Age = float.MaxValue; }
            if (m.age < Scorch52Age) { Scorch52Age = m.age; Scorch52Heat = heat; Scorch52Tint = m.tint; Scorch52Core = core; }
        }
    }
}
