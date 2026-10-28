// NIGHT56 builder V rim56 (design.n56.rim56, a VIEW row: NightRows.ViewOn(cfg, "rim56"); -skygrindOff56 or older turns it off, read from the command
// line here too). Slate V2: the enemies the warmest and brightest thing in the frame (DESIGN-RULES E-3: "enemies are the most saturated warm objects on
// screen ... a distance-scaled rim keeps them readable at range"; colors.md method 4 / 5: the actors brightest, the shadows never black). NIGHT54's
// a452 mid frames measured by Tools/night/n56-colour.py: the enemies' warm body pixels V 0.40-0.71 against the environment's p90 V 0.58-0.95 (2 of 12
// over it on the foundry; 22 of 38 over on foundry / kiln / iceline), because skin52's shadow side is x0.20 / 0.15 / 0.30 of the body (#7C1A0C-ish).
// With the row on, skin52's own shader globals (Assets/Shaders/Enemy.shader: no shader, model or material change: rule 7) are written each frame:
//   - the SHADOW SIDE a warm, brighter multiplier (UnityTuning.Rim56Shade*: the red-orange body's shadow stays red-orange, ~0.75 of the lit side's
//     value instead of ~0.45): the body reads as one warm mass, never a dark half;
//   - the LIT SIDE x Rim56LitGain (the danger token's red channel at the clip: the hue and the saturation kept, its value at the top);
//   - the RIM up close (Rim56RimNearShare of the material's warm rim at a lower power: a warm band on the silhouette, skin52's cut eased, never the
//     whole-body "jelly" glow skin52 removed).
// skin52 off (or its probe forcing it off) leaves everything to skin52 (nothing written). Row off: skin52's globals exactly (Skin52.Apply restores
// them when the row turns off at run time). THE PROOF: a356's mid frames (FeelReel.N56.Colour.cs) read by n56-colour.py's V2 line.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Rim56 : MonoBehaviour
    {
        static readonly int Id = Shader.PropertyToID("_Skin52"), RimId = Shader.PropertyToID("_Skin52Rim"), RampId = Shader.PropertyToID("_Skin52Ramp"),
            ShadeId = Shader.PropertyToID("_Skin52Shade");

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

        public static bool On(GameConfig cfg) => cfg != null && !ArgsOff() && !Rim58.No("-skygrindNoRim56") && NightRows.ViewOn(cfg, "rim56"); // NIGHT58 V: the one-row off switch (Rim58.N58V.cs)

        static int off57 = -1;
        /// <summary>NIGHT57 V: -skygrindOff57 or lower (or the process switch) keeps NIGHT56's rim56 numbers.</summary>
        static bool Off57()
        {
            if (NightRows.ForceOffFrom <= 57) return true;
            if (off57 < 0)
            {
                try { off57 = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 57 ? 1 : 0; }
                catch (System.Exception) { off57 = 0; }
            }
            return off57 == 1;
        }

        /// <summary>For the reel: the lit side's gain written this frame (NIGHT56's Rim56LitGain, x Rim57Gain on NIGHT57).</summary>
        public static float LitNow { get; private set; }

        /// <summary>For the a356 line: the row wrote skin52's globals this frame.</summary>
        public static bool Live { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Rim56") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Rim56>();
        }

        // round 2 (n56-r3 a356: the globals were written, yet the bodies' V moved 0.559 -> 0.569 on the kiln, 0.672 -> 0.668 on span: the ramp's
        // shadow side and the near rim reach only enemies under 8-14 m, and the lit side was already at the red channel's clip): the enemy body
        // material's own far rim (EnemyLook.body: _RimColor x Rim56FarRimGain, _RimPower -> Rim56FarRimPower) is lifted too, restored when off
        Material rimMat;
        Vector4 rimColor0;
        float rimPower0;
        static readonly int RimColorId = Shader.PropertyToID("_RimColor"), RimPowerId = Shader.PropertyToID("_RimPower");

        /// <summary>For the a356 line: the enemy material's far rim was lifted this frame.</summary>
        public static bool RimLifted { get; private set; }

        void RestoreRim()
        {
            if (rimMat != null)
            {
                rimMat.SetVector(RimColorId, rimColor0);
                rimMat.SetFloat(RimPowerId, rimPower0);
            }
            rimMat = null;
            RimLifted = false;
        }

        void LateUpdate()
        {
            SurvivalGame g = SurvivalGame.Instance;
            bool on = g != null && On(g.cfg) && Skin52.Live;
            if (!on)
            {
                if (Live) Skin52.Apply(); // the row turned off (or skin52 did): skin52's own globals back
                Live = false;
                RestoreRim();
                return;
            }
            Live = true;
            if (g.sim == null || g.mats == null) return; // the enemy views (and their shared look) not up yet
            Material body = EnemyLook.For(g).body;
            if (body != rimMat)
            {
                RestoreRim();
                if (body != null && body.HasProperty(RimColorId) && body.HasProperty(RimPowerId))
                {
                    rimMat = body;
                    rimColor0 = body.GetVector(RimColorId);
                    rimPower0 = body.GetFloat(RimPowerId);
                    body.SetVector(RimColorId, rimColor0 * UnityTuning.Rim56FarRimGain);
                    body.SetFloat(RimPowerId, UnityTuning.Rim56FarRimPower);
                    RimLifted = true;
                }
            }
            Shader.SetGlobalVector(Id, new Vector4(1f, UnityTuning.Skin52RimNearM, UnityTuning.Skin52RimFarM, UnityTuning.Rim56RimNearShare));
            Shader.SetGlobalVector(RimId, new Vector4(UnityTuning.Rim56RimNearPower, UnityTuning.Skin52RampFullM, UnityTuning.Skin52RampFlatM, 0));
            // NIGHT57 V (slate V4 rim56, one attempt): the lit side and the shadow side x Rim57Gain over NIGHT56's (no -skygrindOff57): the body's value
            // at fighting range is set by the fog and the post after the shader (n56-r5 foundry bodies V 0.57-0.62 against the environment's p90
            // 0.67-0.69 once colour56's cap holds the environment's S at 0.70: the S half passes, the V half does not)
            bool n57 = !Off57();
            float lit = UnityTuning.Rim56LitGain * (n57 ? UnityTuning.Rim57Gain : 1f), sh = n57 ? UnityTuning.Rim57Gain : 1f;
            lit = Rim60.LitGain(lit, Rim60.LitOn(on)); // NIGHT60 V (B7 attempt 2): a lower lit gain (the brightest bodies kept over the environment's saturation); NIGHT63 V (T-146, the w60 review's S2): the frame's own read of the row (`on`), not a second Rim56.On
            Shader.SetGlobalVector(RampId, new Vector4(UnityTuning.Skin52RampMid, UnityTuning.Skin52RampSoft, lit, 0));
            Shader.SetGlobalVector(ShadeId, Skin52.RampOn ? new Vector4(UnityTuning.Rim56ShadeR * sh, UnityTuning.Rim56ShadeG * sh, UnityTuning.Rim56ShadeB * sh, 0) : new Vector4(lit, lit, lit, 0));
            LitNow = lit;
        }
    }
}
