// NIGHT11 U3-24.3 / U3-24.4: the run shape's props, drawn on frame time from Core's state (draw only: the sim is Core's
// Objectives and Protocol).
// - ObjectiveRingView: objectives.js's ring (:19-25, :122-128): a flat RingGeometry(radius - 0.25, radius, 48) at
//   ring.position (Objectives.RingPosition: the spot + 0.04 m), objectives.color, additive (transparent, no depth write,
//   not tone-mapped), opacity (inside ? 0.95 : 0.55) x (0.8 + 0.2 sin(4t)), scale 1 + 0.02 sin(2t), t the ring's own
//   frame time (it runs only while the ring shows).
// - RelayViews: protocol.js's relays (:21-29, :111-122, :158-175): per relay a dark pylon (CylinderGeometry(0.12, 0.2,
//   protocol.pylonHeight, 8), Lambert 0x232838), an orb (SphereGeometry(0.24, 12, 8), unlit, at pylonHeight + 0.2) whose
//   colour is protocol.litColor x 2.2 when lit, else protocol.color x (awake ? 0.6 + 0.5 sin(3t + spot.x) : 0.15), a beam
//   (open CylinderGeometry(0.14, 0.14, protocol.beamHeight, 8), additive, opacity 0.35, protocol.color) up from the orb
//   while lit; the tall guide column (open CylinderGeometry(0.45, 0.45, protocol.guideHeight, 10), additive, opacity
//   0.12) standing on the next relay while the quest runs; the secret relay's orb barely there until lit
//   (0.18 + 0.08 sin(1.3t)), no guide. Rebuilt when Protocol.Version changes (a new run's Load).
// Every size, opacity and pulse above is the web's own literal (cited by line), not a Unity-only number.
// The materials as WeaponFx's: copies of the neon material (URP Unlit), the additive ones Blend One One, no depth write,
// the transparent queue, with the opacity folded into the colour (three's additive is src x alpha + dst). The pylon body
// is a copy of the lit material. Colours are scaled in linear space, as three's color management does
// (setHex(...).multiplyScalar(k) on a linear colour): (Look.Hex(hex).linear x k).gamma, so SetColor's sRGB-to-linear
// conversion gives back linear x k.
// UnityTuning: none. Every number here is the web's (objectives.js, protocol.js) or config's (objectives.radius,
// objectives.color, protocol.pylonHeight / beamHeight / guideHeight / color / litColor).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>Mesh and material helpers shared by the run shape's views.</summary>
    static class RunShapeKit
    {
        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>(hex as a linear colour) x k, as an sRGB Color for SetColor (alpha 1).</summary>
        public static Color Scaled(uint hex, float k)
        {
            Color lin = Look.Hex(hex).linear;
            Color c = new Color(lin.r * k, lin.g * k, lin.b * k, 1).gamma;
            c.a = 1;
            return c;
        }

        /// <summary>A copy of the neon (URP Unlit) material drawn additively: Blend One One, no depth write, transparent queue.</summary>
        public static Material Additive(Material neon, string name)
        {
            var m = new Material(neon) { name = name, renderQueue = (int)RenderQueue.Transparent };
            m.SetFloat("_SrcBlend", (float)BlendMode.One);
            m.SetFloat("_DstBlend", (float)BlendMode.One);
            m.SetFloat("_ZWrite", 0);
            return m;
        }

        /// <summary>three's RingGeometry(inner, outer, segments).rotateX(-PI / 2): a flat annulus facing up, centred at the origin.</summary>
        public static Mesh Ring(float inner, float outer, int segments, string name)
        {
            var kit = new MeshKit();
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2 / segments, a1 = (i + 1) * Mathf.PI * 2 / segments;
                var d0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                kit.Quad(d0 * inner, d0 * outer, d1 * outer, d1 * inner, Vector3.up, Color.white);
            }
            return kit.Build(name);
        }

        /// <summary>
        /// three's CylinderGeometry(rTop, rBottom, height, sides, 1, openEnded).translate(0, height / 2, 0): from y 0 to
        /// `height`, the sides facing out (three's FrontSide), capped unless open.
        /// </summary>
        public static Mesh Cylinder(float rTop, float rBottom, float height, int sides, bool openEnded, string name)
        {
            var kit = new MeshKit();
            Vector3 top = Vector3.up * height;
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                var d0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                Vector3 b0 = d0 * rBottom, b1 = d1 * rBottom, t0 = d0 * rTop + top, t1 = d1 * rTop + top;
                kit.Quad(b0, b1, t1, t0, (d0 + d1).normalized, Color.white);
                if (!openEnded)
                {
                    kit.Triangle(top, t0, t1, Vector3.up, Color.white);
                    kit.Triangle(Vector3.zero, b0, b1, Vector3.down, Color.white);
                }
            }
            return kit.Build(name);
        }

        /// <summary>three's SphereGeometry(radius, width, height): flat faces (the material is unlit).</summary>
        public static Mesh Sphere(float radius, int width, int height, string name)
        {
            var kit = new MeshKit();
            Vector3 P(int i, int j)
            {
                float th = i * Mathf.PI / height, ph = j * Mathf.PI * 2 / width;
                return new Vector3(Mathf.Sin(th) * Mathf.Cos(ph), Mathf.Cos(th), Mathf.Sin(th) * Mathf.Sin(ph)) * radius;
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Vector3 a = P(i, j), b = P(i, j + 1), c = P(i + 1, j + 1), d = P(i + 1, j);
                    Vector3 n = (a + b + c + d).normalized;
                    if (i == 0) kit.Triangle(a, c, d, n, Color.white); // a == b: the top pole
                    else if (i == height - 1) kit.Triangle(a, b, c, n, Color.white); // c == d: the bottom pole
                    else kit.Quad(a, b, c, d, n, Color.white);
                }
            }
            return kit.Build(name);
        }

        public static Renderer Part(string name, Transform parent, Mesh mesh, Material mat, bool shadows, float y = 0)
        {
            GameObject go = MeshKit.Object(name, parent, mesh, mat, shadows);
            go.transform.localPosition = new Vector3(0, y, 0);
            return go.GetComponent<Renderer>();
        }
    }

    /// <summary>objectives.js's HOLD ring (the ring mesh and update()).</summary>
    public sealed class ObjectiveRingView
    {
        readonly Objectives o;
        readonly Transform ring;
        readonly Renderer renderer;
        readonly Material mat;
        readonly uint color;
        double ringTime;

        public ObjectiveRingView(SurvivalGame game, Objectives o)
        {
            this.o = o;
            color = o.o.color; // objectives.color
            float radius = (float)o.Radius;
            mat = RunShapeKit.Additive(game.mats.neon, "Objective ring");
            mat.SetColor(RunShapeKit.BaseColor, RunShapeKit.Scaled(color, 0.7f)); // :21 opacity 0.7 until update() runs
            // :20 RingGeometry(o.radius - 0.25, o.radius, 48).rotateX(-Math.PI / 2); :24 noShadow
            renderer = RunShapeKit.Part("Objective ring", null, RunShapeKit.Ring(radius - 0.25f, radius, 48, "objective ring"), mat, shadows: false);
            ring = renderer.transform;
            renderer.enabled = false;
        }

        /// <summary>update(frameDt): the ring breathes, brighter while you stand in it.</summary>
        public void Render(double frameDt)
        {
            bool on = o.RingVisible;
            renderer.enabled = on;
            if (!on) return; // :124
            ringTime += frameDt;
            float t = (float)ringTime;
            float opacity = (o.Inside ? 0.95f : 0.55f) * (0.8f + 0.2f * Mathf.Sin(t * 4)); // :126
            mat.SetColor(RunShapeKit.BaseColor, RunShapeKit.Scaled(color, opacity));
            ring.position = WebSpace.ToUnity(o.RingPosition); // :77
            ring.localScale = Vector3.one * (1 + 0.02f * Mathf.Sin(t * 2)); // :127
        }
    }

    /// <summary>protocol.js's relays, beams and guide (the meshes of load() and update()).</summary>
    public sealed class RelayViews
    {
        sealed class View
        {
            public Protocol.Relay relay;
            public Transform root;
            public Renderer orb, beam;
        }

        readonly Protocol p;
        readonly Transform group;
        readonly Material bodyMat, orbMat, beamMat, guideMat;
        readonly Mesh bodyGeo, orbGeo, beamGeo;
        readonly Renderer guide;
        readonly List<View> views = new List<View>();
        View secret;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly uint color, litColor;
        readonly float pylonHeight;
        int built = -1;
        double time;

        public RelayViews(SurvivalGame game, Protocol p)
        {
            this.p = p;
            Protocol.P c = p.c;
            color = c.color;
            litColor = c.litColor;
            pylonHeight = (float)c.pylonHeight;
            group = new GameObject("Protocol relays").transform; // :19
            bodyMat = new Material(game.mats.lit) { name = "Relay body", color = Look.Hex(0x232838) }; // :21 MeshLambertMaterial 0x232838
            orbMat = new Material(game.mats.neon) { name = "Relay orb" }; // :22 MeshBasicMaterial (its colour per orb: a property block)
            beamMat = RunShapeKit.Additive(game.mats.neon, "Relay beam"); // :23 opacity 0.35
            beamMat.SetColor(RunShapeKit.BaseColor, RunShapeKit.Scaled(color, 0.35f));
            guideMat = RunShapeKit.Additive(game.mats.neon, "Relay guide"); // :24 opacity 0.12
            guideMat.SetColor(RunShapeKit.BaseColor, RunShapeKit.Scaled(color, 0.12f));
            bodyGeo = RunShapeKit.Cylinder(0.12f, 0.2f, pylonHeight, 8, false, "relay body"); // :25
            orbGeo = RunShapeKit.Sphere(0.24f, 12, 8, "relay orb"); // :26
            beamGeo = RunShapeKit.Cylinder(0.14f, 0.14f, (float)c.beamHeight, 8, true, "relay beam"); // :27
            Mesh guideGeo = RunShapeKit.Cylinder(0.45f, 0.45f, (float)c.guideHeight, 10, true, "relay guide"); // :28
            guide = RunShapeKit.Part("Relay guide", group, guideGeo, guideMat, shadows: false); // :29-32
            guide.enabled = false;
        }

        /// <summary>load()'s build(spot): the root at the spot, the body, the orb and the (hidden) beam.</summary>
        View Build(Protocol.Relay r)
        {
            var root = new GameObject(r.secret ? $"Secret relay {r.id}" : $"Relay {r.id}").transform;
            root.SetParent(group, false);
            root.position = WebSpace.ToUnity(r.spot); // :113
            RunShapeKit.Part("Body", root, bodyGeo, bodyMat, shadows: true);
            var v = new View
            {
                relay = r,
                root = root,
                orb = RunShapeKit.Part("Orb", root, orbGeo, orbMat, shadows: false, y: pylonHeight + 0.2f), // :116
                beam = RunShapeKit.Part("Beam", root, beamGeo, beamMat, shadows: false, y: pylonHeight + 0.2f), // :118
            };
            v.beam.enabled = false; // :119
            return v;
        }

        void Rebuild()
        {
            built = p.Version;
            foreach (View v in views) Object.Destroy(v.root.gameObject);
            views.Clear();
            if (secret != null) Object.Destroy(secret.root.gameObject);
            secret = null;
            guide.enabled = false; // clear(): guide.visible = false
            for (int i = 0; i < p.relays.Count; i++) views.Add(Build(p.relays[i]));
            if (p.Secret != null) secret = Build(p.Secret);
        }

        void SetOrb(Renderer orb, uint hex, float k)
        {
            block.SetColor(RunShapeKit.BaseColor, RunShapeKit.Scaled(hex, k)); // color.setHex(hex).multiplyScalar(k)
            orb.SetPropertyBlock(block);
        }

        /// <summary>update(frameDt): unlit relays pulse while awake, lit ones beam, the next one carries the tall guide.</summary>
        public void Render(double frameDt)
        {
            if (p.Version != built) Rebuild();
            if (!p.Active) return; // :160
            time += frameDt;
            float t = (float)time;
            Protocol.Relay n = p.Awake && p.Surge != Protocol.SurgeDone ? p.NextRelay : null; // :162
            for (int i = 0; i < views.Count; i++)
            {
                View v = views[i];
                Protocol.Relay r = v.relay;
                float k = r.lit ? 2.2f : p.Awake ? 0.6f + 0.5f * Mathf.Sin(t * 3 + (float)r.spot.x) : 0.15f; // :164
                SetOrb(v.orb, r.lit ? litColor : color, k); // :165
                v.beam.enabled = r.lit; // :166
            }
            guide.enabled = n != null; // :168
            if (n != null) guide.transform.position = WebSpace.ToUnity(n.spot); // :169
            if (secret != null)
            {
                // Barely there until lit: a slow, faint pulse (no guide, no HUD line). :170-174
                bool lit = secret.relay.lit;
                SetOrb(secret.orb, lit ? litColor : color, lit ? 2.2f : 0.18f + 0.08f * Mathf.Sin(t * 1.3f));
                secret.beam.enabled = lit;
            }
        }
    }
}
