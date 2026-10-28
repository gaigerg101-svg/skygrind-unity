// NIGHT25 builder W: RIVAL PRIME as the view draws it (Core/Enemies/RivalPrime.cs): a blocky mirror of the player (CLAUDE.md rule 7): two legs, a torso, a head, a gun arm, in
// the enemy hull's dark steel, with a MAGENTA visor and chest stripe (rule 8: magenta is the RIVAL's only colour; nothing else in the game uses it), a white flash on a hit
// and its orbs as small additive red-orange boxes (danger). Interpolated on the sim's render alpha; shown only while it fights. Numbers: UnityTuning.Rival25.cs.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class RivalView
    {
        readonly SurvivalGame game;
        readonly Transform root, legL, legR, torso, head, visor, stripe, arm;
        readonly Transform[] orbT;
        readonly Renderer[] all;
        readonly Renderer visorR, stripeR, bodyTorsoR, headR, legLR, legRR, armR;
        readonly Renderer[] orbR;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color body, magenta, orb, white;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public bool Shown => root.gameObject.activeSelf;

        public RivalView(SurvivalGame game)
        {
            this.game = game;
            body = Look.Hex(UnityTuning.RivalBodyHex);
            magenta = Look.Hex(UnityTuning.RivalMagentaHex, UnityTuning.RivalMagentaGain);
            orb = Look.Hex(UnityTuning.RivalOrbHex, UnityTuning.RivalOrbGain);
            white = Look.Hex(0xffffff);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            Material solid = FxDraw.Opaque(game.mats.neon, "Rival body", Color.white);
            Material add = FxDraw.Additive(game.mats.neon, "Rival glow");
            root = new GameObject("RIVAL PRIME").transform;
            legL = Part(root, "legL", box, solid, out legLR);
            legR = Part(root, "legR", box, solid, out legRR);
            torso = Part(root, "torso", box, solid, out bodyTorsoR);
            head = Part(root, "head", box, solid, out headR);
            visor = Part(root, "visor", box, add, out visorR);
            stripe = Part(root, "stripe", box, add, out stripeR);
            arm = Part(root, "arm", box, solid, out armR);
            orbT = new Transform[UnityTuning.RivalOrbPool];
            orbR = new Renderer[UnityTuning.RivalOrbPool];
            for (int i = 0; i < orbT.Length; i++) orbT[i] = Part(root, "orb " + i, box, add, out orbR[i]);
            root.gameObject.SetActive(false);
        }

        static Transform Part(Transform parent, string name, Mesh mesh, Material mat, out Renderer r)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            return g.transform;
        }

        void Paint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z);

        public void Render(double alpha)
        {
            RivalPrime r = game.sim?.rival25;
            bool on = r != null && r.Alive;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            if (!on) return;
            float a = Mathf.Clamp01((float)alpha);
            Vector3 feet = Vector3.Lerp(U(r.body.previous), U(r.body.position), a);
            // web yaw theta looks along (-sin, -cos) in web x,z = (-sin, +cos) in Unity: a rotation of -theta about Unity y
            Quaternion q = Quaternion.Euler(0, (float)(-r.body.yaw * Mathf.Rad2Deg), 0);
            float h = (float)r.p.height;
            Place(legL, feet, q, new Vector3(-0.16f, UnityTuning.RivalLeg[1] * 0.5f, 0), UnityTuning.RivalLeg);
            Place(legR, feet, q, new Vector3(0.16f, UnityTuning.RivalLeg[1] * 0.5f, 0), UnityTuning.RivalLeg);
            Place(torso, feet, q, new Vector3(0, UnityTuning.RivalLeg[1] + UnityTuning.RivalTorso[1] * 0.5f, 0), UnityTuning.RivalTorso);
            Place(head, feet, q, new Vector3(0, h - UnityTuning.RivalHead[1] * 0.5f, 0), UnityTuning.RivalHead);
            Place(visor, feet, q, new Vector3(0, h - UnityTuning.RivalHead[1] * 0.5f, UnityTuning.RivalHead[2] * 0.5f), new[] { UnityTuning.RivalHead[0] * 0.9f, 0.08f, UnityTuning.RivalVisorThick });
            Place(stripe, feet, q, new Vector3(0, UnityTuning.RivalLeg[1] + UnityTuning.RivalTorso[1] * 0.5f, UnityTuning.RivalTorso[2] * 0.5f), new[] { UnityTuning.RivalTorso[0] * 0.8f, 0.1f, UnityTuning.RivalVisorThick });
            Place(arm, feet, q, new Vector3(0.38f, UnityTuning.RivalLeg[1] + UnityTuning.RivalTorso[1] * 0.75f, UnityTuning.RivalArm[2] * 0.4f), UnityTuning.RivalArm);
            bool flash = r.body.flash > 0;
            Color c = flash ? white : body;
            Paint(legLR, c); Paint(legRR, c); Paint(bodyTorsoR, c); Paint(headR, c); Paint(armR, c);
            Paint(visorR, magenta);
            Paint(stripeR, magenta);
            int n = Mathf.Min(r.orbs.Count, orbT.Length);
            for (int i = 0; i < orbT.Length; i++)
            {
                bool live = i < n;
                if (orbT[i].gameObject.activeSelf != live) orbT[i].gameObject.SetActive(live);
                if (!live) continue;
                orbT[i].position = U(r.orbs[i].pos);
                orbT[i].rotation = Quaternion.identity;
                orbT[i].localScale = Vector3.one * UnityTuning.RivalOrbSize;
                Paint(orbR[i], orb);
            }
        }

        static void Place(Transform t, Vector3 feet, Quaternion q, Vector3 local, float[] size)
        {
            t.SetPositionAndRotation(feet + q * local, q);
            t.localScale = new Vector3(size[0], size[1], size[2]);
        }
    }
}
