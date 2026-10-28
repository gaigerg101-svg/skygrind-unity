// NIGHT47 builder B, G35 (the view of SurvivalSim.N47.Kiln.cs's three foreshadow beats; drawn only while the sim runs them: wyrm47 on, n47-kiln).
// Visual only: it reads the sim's public beat state, never writes it. The worm's molten language (wyrmLook47's colours, UnityTuning.Brawl47.cs):
//   THE GLIMPSE  eight molten blocks break out of kb1's far wall, arc 5 m up and go back into the rock over glimpseS s
//   THE PASS     fourteen glowing floor plates light one after another wall to wall along the trail, then fade
//   THE CAMEO    the breach's crack circle on the floor (red-orange, a low glow brightening, white-hot once it locks) then the head and four blocks erupting 5 m
//                and falling back, with a molten burst
// Built from Unity primitives at init (no allocation per frame); hooked in SurvivalGame.Wyrm24.cs (one line each in init and render).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Wyrm47BeatsView
    {
        const int Glimpse = 8, Plates = 14, Breach = 5;
        readonly SurvivalGame game;
        readonly Transform root;
        readonly Transform[] gT = new Transform[Glimpse], pT = new Transform[Plates], bT = new Transform[Breach];
        readonly Renderer[] gR = new Renderer[Glimpse], pR = new Renderer[Plates], bR = new Renderer[Breach];
        readonly Transform discT;
        readonly Renderer discR;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        int lastBreach = int.MaxValue;

        public Wyrm47BeatsView(SurvivalGame game)
        {
            this.game = game;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            Material add = FxDraw.Additive(game.mats.neon, "Wyrm47 beats");
            root = new GameObject("WYRM47 BEATS").transform;
            for (int i = 0; i < Glimpse; i++) gR[i] = Part(box, add, out gT[i]);
            for (int i = 0; i < Plates; i++) pR[i] = Part(box, add, out pT[i]);
            for (int i = 0; i < Breach; i++) bR[i] = Part(box, add, out bT[i]);
            discR = Part(box, add, out discT);
        }

        Renderer Part(Mesh mesh, Material mat, out Transform t)
        {
            var g = new GameObject("beat part");
            g.transform.SetParent(root, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            g.SetActive(false);
            t = g.transform;
            return r;
        }

        void Show(Transform t, Renderer r, bool on, Vector3 pos, Vector3 size, Color c)
        {
            if (t.gameObject.activeSelf != on) t.gameObject.SetActive(on);
            if (!on) return;
            t.position = pos;
            t.localScale = size;
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        public void Render()
        {
            SurvivalSim sim = game.sim;
            if (sim == null) return;
            GameConfig cfg = game.cfg;
            const string R = "design.n47.wyrm47.numbers.";
            Color glow = Look.Hex(UnityTuning.Wyrm47GlowHex, UnityTuning.Wyrm47SeamGain), amber = Look.Hex(UnityTuning.Wyrm47AmberHex, UnityTuning.Wyrm47SeamGain);
            Color hot = Look.Hex(UnityTuning.Wyrm47HotHex, UnityTuning.Wyrm47PunchGain);

            // THE GLIMPSE: an arc out of the far wall (rooms run toward -z: the wall is at the beat's z, the room toward +z)
            float gs = (float)(sim.Glimpse47Steps / 120.0 / System.Math.Max(0.1, cfg.Num(R + "glimpseS")));
            for (int i = 0; i < Glimpse; i++)
            {
                float s = gs * 1.6f - i * 0.08f;
                bool on = sim.Glimpse47Steps >= 0 && s > 0 && s < 1;
                Vec3 at = sim.Glimpse47At;
                Vec3 p = new Vec3(at.x - 7 + 14 * s, at.y + 3 + 5 * Mathf.Sin(Mathf.PI * s), at.z + 6.0); // r2 note: 6 m out from the far wall (a back you can see from the top of the steps)
                Show(gT[i], gR[i], on, WebSpace.ToUnity(p), Vector3.one * (i == 0 ? 3.2f : 2.6f), i == 0 ? hot : Color.Lerp(glow, amber, (i % 2) * 0.5f));
            }

            // THE PASS: the trail lights plate by plate, then fades
            float ps = (float)(sim.Pass47Steps / 120.0 / System.Math.Max(0.1, cfg.Num(R + "passS")));
            for (int i = 0; i < Plates; i++)
            {
                float k = (i + 0.5f) / Plates;
                float age = ps - k; // passS units since this plate lit
                bool on = sim.Pass47Steps >= 0 && age > 0 && age < 0.6f;
                Vec3 p = sim.Pass47From + (sim.Pass47To - sim.Pass47From) * k;
                Show(pT[i], pR[i], on, WebSpace.ToUnity(new Vec3(p.x, p.y + UnityTuning.Wyrm47TrailLift, p.z)), new Vector3(2.6f, UnityTuning.Wyrm47TrailThick, 2.6f), glow * (1f - age / 0.6f));
            }

            // THE CAMEO: the crack circle through the tell, then the eruption
            double rad = sim.Cameo47Radius;
            bool telling = sim.Cameo47Telling;
            float prog = (float)sim.Cameo47Progress;
            bool locked = prog >= (float)cfg.Num(R + "cameoTrack");
            Color dc = Color.Lerp(Look.Hex(UnityTuning.Wyrm47GlowHex, 0.4f + 1.2f * prog), hot, locked ? 0.35f : 0f) * (locked ? 1f : 0.85f + 0.15f * Mathf.Sin(Time.time * 14f));
            Vec3 ca = sim.Cameo47At;
            Show(discT, discR, telling, WebSpace.ToUnity(new Vec3(ca.x, ca.y + 0.03, ca.z)), new Vector3((float)rad * 2, 0.04f, (float)rad * 2), dc);
            float bt = sim.Cameo47BreachSteps / 120f;
            for (int i = 0; i < Breach; i++)
            {
                float t = bt - i * 0.07f;
                bool on = sim.Cameo47BreachSteps >= 0 && t > 0 && t < 1.1f;
                float y = 5f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 1.1f));
                Show(bT[i], bR[i], on, WebSpace.ToUnity(new Vec3(ca.x, ca.y + y, ca.z)), Vector3.one * (i == 0 ? 1.4f : 1.1f), i == 0 ? hot : glow);
            }
            // a new eruption (its step count went back down): the molten burst, once
            int steps = sim.Cameo47BreachSteps;
            if (steps >= 0 && steps < lastBreach) game.ParticleBurst(new Vec3(ca.x, ca.y + 0.5, ca.z), Look.Hex(UnityTuning.Wyrm47GlowHex, 2f), 40, 10f, 0.8f, 1.2f);
            lastBreach = steps < 0 ? int.MaxValue : steps;
        }
    }
}
