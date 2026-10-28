// NIGHT41 builder B8 (NIGHT41.md B8; INVENTORY C6): brawlLook41, THE ICEBREAKER in ice armour (view only, behind design.n41.brawlLook41;
// off or -skygrindOff41: Brawl40View draws as NIGHT40).
// The root cause (read in the code, seen on the lead's n41-r1 frames: the body almost the wall colour): SKYGRIND/Stone takes its albedo from
// vertex colours only (Assets/Shaders/Stone.shader: albedo = i.color.rgb); Brawl40View.Opaque set _BaseColor / _Color, which it never reads,
// and the cube primitive has no vertex colours, so every body part drew white and the warm room light made it tan.
//  - THE ARMOUR: with the row on, the body's parts draw coloured cube meshes: ice plates (torso, hump, arms) a strong blue that lands pale
//    blue under the warm light, the stone core (legs, skull, fists) near-black; faint additive pale-blue frost edges on the plates.
//  - THE FISTS: an additive shell over each fist glows red-orange through its tell (haymaker: the right; slam: both) and at the strike, cyan
//    while stuck after the haymaker (the punish).
//  - THE POSES: the slam wind-up crouches and arches back, the rush wind-up drops lower.
//  - THE DEATH: on the first dying frame the body shatters into blocks (crunch's Chunks.Break per part, its own colours) and the body hides.
// Numbers: UnityTuning.Brawl41.cs.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Brawl40View
    {
        Mesh plateMesh41, coreMesh41;
        readonly List<MeshFilter> plates41 = new List<MeshFilter>(), cores41 = new List<MeshFilter>();
        readonly List<Renderer> frost41 = new List<Renderer>();
        Renderer shellR41, shellL41;
        bool look41, shattered41;

        /// <summary>For the reels / frames: the armour drawn, the fist shells lit now, the death's blocks made.</summary>
        public bool Look41On, Shell41Lit;
        public int Shatter41Blocks;

        static Mesh Coloured(Mesh src, Color c, string name)
        {
            Mesh m = Object.Instantiate(src);
            m.name = name;
            var cols = new Color[m.vertexCount];
            for (int i = 0; i < cols.Length; i++) cols[i] = c;
            m.colors = cols;
            return m;
        }

        void Init41()
        {
            plateMesh41 = Coloured(boxMesh, Look.Hex(UnityTuning.Brawl41PlateHex), "brawl41 plate");
            coreMesh41 = Coloured(boxMesh, Look.Hex(UnityTuning.Brawl41CoreHex), "brawl41 core");
            foreach (MeshFilter mf in fxRoot.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = mf.GetComponent<MeshRenderer>();
                if (r == null) continue;
                if (r.sharedMaterial == bodyMat) plates41.Add(mf);
                else if (r.sharedMaterial == bodyDark) cores41.Add(mf);
            }
            // frost: thin pale-blue edges on the torso's and hump's front and top (local to the leaning torso)
            frost41.Add(GlowPart(lean, "frost torso front", new Vector3(0, 1.72f, 0.91f), new Vector3(2.22f, 0.06f, 0.04f)));
            frost41.Add(GlowPart(lean, "frost hump top", new Vector3(0, 2.16f, -0.15f), new Vector3(2.62f, 0.04f, 1.42f)));
            frost41.Add(GlowPart(lean, "frost torso L", new Vector3(-1.11f, 0.9f, 0.15f), new Vector3(0.04f, 1.7f, 0.06f)));
            frost41.Add(GlowPart(lean, "frost torso R", new Vector3(1.11f, 0.9f, 0.15f), new Vector3(0.04f, 1.7f, 0.06f)));
            shellR41 = Glow(fistR, "brawl41 shell R", out Transform sr);
            shellL41 = Glow(fistL, "brawl41 shell L", out Transform sl);
            sr.localScale = sl.localScale = Vector3.one * UnityTuning.Brawl41ShellScale;
            foreach (Renderer f in frost41) f.gameObject.SetActive(false);
        }

        void Look41(bool on)
        {
            if (on == look41) return;
            look41 = on;
            foreach (MeshFilter mf in plates41) mf.sharedMesh = on ? plateMesh41 : boxMesh;
            foreach (MeshFilter mf in cores41) mf.sharedMesh = on ? coreMesh41 : boxMesh;
            foreach (Renderer f in frost41) f.gameObject.SetActive(on);
            if (!on) { shellR41.gameObject.SetActive(false); shellL41.gameObject.SetActive(false); }
        }

        /// <summary>After Brawl40's pose switch, before the lean is applied: the row's armour, extra poses, fist shells, the shatter.</summary>
        void Render41(SurvivalSim sim, Brawl40 g, Brawl40State st, string atk, float prog, ref float leanDeg, ref float drop, Color dangerC, Color cyanC)
        {
            bool on = N41View.RowOn(sim.cfg, "brawlLook41");
            Look41(on);
            Look41On = on;
            Shell41Lit = false;
            if (!on) { shattered41 = false; return; }
            if (st == Brawl40State.Tell && atk == Brawl40.Slam) { drop += UnityTuning.Brawl41SlamCrouch * prog; leanDeg += UnityTuning.Brawl41SlamArch * prog; }
            if (st == Brawl40State.Tell && atk == Brawl40.Rush) drop += UnityTuning.Brawl41RushCrouch * prog;
            bool litR = (st == Brawl40State.Tell || st == Brawl40State.Active) && (atk == Brawl40.Haymaker || atk == Brawl40.Slam);
            bool litL = (st == Brawl40State.Tell || st == Brawl40State.Active) && atk == Brawl40.Slam;
            bool stuck = st == Brawl40State.Stuck && atk == Brawl40.Haymaker;
            float gain = st == Brawl40State.Active ? UnityTuning.Brawl41ShellGain : Mathf.Lerp(UnityTuning.Brawl40TellMin, UnityTuning.Brawl41ShellGain, prog);
            Color shell = Look.Hex(UnityTuning.Brawl40DangerHex, gain);
            bool showR = litR || stuck;
            if (shellR41.gameObject.activeSelf != showR) shellR41.gameObject.SetActive(showR);
            if (shellL41.gameObject.activeSelf != litL) shellL41.gameObject.SetActive(litL);
            if (showR) Tint(shellR41, stuck ? Look.Hex(UnityTuning.Brawl40CyanHex, UnityTuning.Brawl41ShellStuckGain) : shell);
            if (litL) Tint(shellL41, shell);
            Shell41Lit = litR || litL;
            Color frost = Look.Hex(UnityTuning.Brawl41FrostHex, UnityTuning.Brawl41FrostGain);
            foreach (Renderer f in frost41) Tint(f, frost);
            // the death: shatter once into crunch's blocks (the parts' own colours), the body hidden while it dies
            if (st != Brawl40State.Dying) shattered41 = false;
            if (st == Brawl40State.Dying && !shattered41)
            {
                shattered41 = true;
                Shatter41(g);
            }
            bool hide = st == Brawl40State.Dying && shattered41;
            if (bodyRoot.gameObject.activeSelf == hide)
            {
                bodyRoot.gameObject.SetActive(!hide);
                armR.gameObject.SetActive(!hide); armL.gameObject.SetActive(!hide);
                fistR.gameObject.SetActive(!hide); fistL.gameObject.SetActive(!hide);
            }
        }

        void Shatter41(Brawl40 g)
        {
            Chunks chunks = EnemyLook.For(game).chunks;
            if (chunks == null) return;
            Vector3 feet = U(g.body.position);
            Vector3 center = U(g.Centre);
            Vector3 cam = game.cam != null ? game.cam.transform.position : center - Vector3.forward;
            Vector3 shot = center - cam;
            shot = shot.sqrMagnitude > 1e-4f ? shot.normalized : Vector3.forward;
            float dist = Vector3.Distance(cam, center);
            SigParams sp = CrunchMath.ParamsOf(DeathSig.Clean);
            Color plate = Look.Hex(UnityTuning.Brawl41PlateHex), core = Look.Hex(UnityTuning.Brawl41CoreHex);
            int made = 0;
            foreach (MeshFilter mf in plates41) made += chunks.Break(mf.transform.localToWorldMatrix, boxMesh.bounds, plate, shot, center, UnityTuning.Brawl41ShatterThrow, sp, false, feet.y, dist);
            foreach (MeshFilter mf in cores41) made += chunks.Break(mf.transform.localToWorldMatrix, boxMesh.bounds, core, shot, center, UnityTuning.Brawl41ShatterThrow, sp, false, feet.y, dist);
            Shatter41Blocks = made;
        }
    }
}
