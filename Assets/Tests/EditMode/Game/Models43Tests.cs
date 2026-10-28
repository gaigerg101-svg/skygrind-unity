// NIGHT43 builder A1 (models43; NIGHT43.md card A1): the rebuilt enemies' pure checks (no Player): every Mesh43 mesh is a sound smooth mesh (unit
// normals, front faces outward, inside its unit space, a triangle budget), every spec is whole parts with valid roles / groups / anchors, a visor
// sits ON the helmet it is anchored to, each kind stays inside its renderer budget, and the swap is reversible (OFF puts NIGHT42's picture back).
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Models43Tests
    {
        // the view's renderers per kind (read from the views 2026-10-16) and the budget with models43 drawing (UnityTuning.Models43.cs's header)
        static readonly Dictionary<string, int> Base = new Dictionary<string, int>
        {
            { "husk", 2 }, { "swarmer", 3 }, { "spitter", 3 }, { "charger", 3 }, { "warden", 3 }, { "drone", 2 },
        };

        static readonly Dictionary<string, int> Budget = new Dictionary<string, int>
        {
            { "husk", 6 }, { "swarmer", 5 }, { "spitter", 6 }, { "charger", 5 }, { "warden", 7 }, { "drone", 6 },
        };

        static int[] BaseIds(string kind) => kind == "husk" ? UnityTuning.Models43HuskBase : kind == "swarmer" ? UnityTuning.Models43SwarmerBase
            : kind == "spitter" ? UnityTuning.Models43SpitterBase : kind == "charger" ? UnityTuning.Models43ChargerBase : kind == "warden" ? UnityTuning.Models43WardenBase : null;

        [Test]
        public void EveryMeshIsSmoothSoundAndInsideItsSpace()
        {
            for (int id = 0; id < Mesh43.Count; id++)
            {
                Mesh m = Mesh43.Get(id);
                Assert.IsNotNull(m, "mesh " + id);
                Vector3[] v = m.vertices, n = m.normals;
                int[] t = m.triangles;
                Assert.Greater(v.Length, 0, "mesh " + id + ": no vertex");
                Assert.AreEqual(v.Length, n.Length, "mesh " + id + ": a normal per vertex");
                Assert.AreEqual(0, t.Length % 3, "mesh " + id);
                Assert.LessOrEqual(t.Length / 3, 2400, "mesh " + id + ": over the triangle budget");
                foreach (Vector3 nn in n) Assert.AreEqual(1, nn.magnitude, 1e-3, "mesh " + id + ": a normal not unit");
                int back = 0;
                for (int i = 0; i < t.Length; i += 3)
                {
                    Vector3 cr = Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]);
                    if (Vector3.Dot(cr, n[t[i]] + n[t[i + 1]] + n[t[i + 2]]) < 0) back++;
                }
                Assert.AreEqual(0, back, "mesh " + id + ": triangles facing inward");
                Bounds b = m.bounds;
                // the pill is the capsule's space (y -1..1), the saucer r 1, the shield and its frame bowed 0.625 on z (scaled thin by the shield)
                float lim = id == Mesh43.Pill ? 1.02f : id == Mesh43.Saucer ? 1.05f : id == Mesh43.ShieldSlab || id == Mesh43.ShieldFrame ? 1.3f : 0.75f;
                Assert.LessOrEqual(Mathf.Max(Mathf.Abs(b.min.x), Mathf.Abs(b.max.x), Mathf.Abs(b.min.y), Mathf.Abs(b.max.y), Mathf.Abs(b.min.z), Mathf.Abs(b.max.z)), lim, "mesh " + id + ": outside its unit space");
            }
        }

        [Test]
        public void ThePillStaysAPill()
        {
            // GG: "keep the pill silhouette": the capsule's height and width, a waist no deeper than 15 %
            Bounds b = Mesh43.Get(Mesh43.Pill).bounds;
            Assert.AreEqual(2, b.size.y, 0.02, "the pill's height");
            Assert.AreEqual(1, b.size.x, 0.03, "the pill's width");
            float waist = float.MaxValue;
            foreach (Vector3 p in Mesh43.Get(Mesh43.Pill).vertices)
                if (Mathf.Abs(p.y + 0.05f) < 0.06f) waist = Mathf.Min(waist, new Vector2(p.x, p.z).magnitude);
            Assert.That(waist, Is.InRange(0.42f, 0.49f), "a waist, not a straight capsule, not a wasp");
        }

        static float Inside(Vector3 p, float e1, float e2)
        {
            float xz = Mathf.Pow(Mathf.Abs(2 * p.x), 2 / e2) + Mathf.Pow(Mathf.Abs(2 * p.z), 2 / e2);
            return Mathf.Pow(xz, e2 / e1) + Mathf.Pow(Mathf.Abs(2 * p.y), 2 / e1);
        }

        [Test]
        public void EveryVisorSitsOnItsHelmet()
        {
            (int id, float e1, float e2)[] visors =
            {
                (Mesh43.VisorSlit, Mesh43.HelmetE1, Mesh43.HelmetE2), (Mesh43.VisorSlit2, Mesh43.HelmetE1, Mesh43.HelmetE2),
                (Mesh43.VisorPlate, Mesh43.HelmetE1, Mesh43.HelmetE2), (Mesh43.VisorChevron, Mesh43.BrowE1, Mesh43.BrowE2),
            };
            foreach ((int id, float e1, float e2) in visors)
            {
                // F is homogeneous of degree 2 / e1, so F^(e1 / 2) is the radial scale off the helmet's surface (1 = on it)
                float min = float.MaxValue, max = 0, front = 0;
                foreach (Vector3 p in Mesh43.Get(id).vertices)
                {
                    float s = Mathf.Pow(Inside(p, e1, e2), e1 / 2);
                    min = Mathf.Min(min, s);
                    max = Mathf.Max(max, s);
                    front += p.z;
                }
                Assert.GreaterOrEqual(min, 1.0f, "visor " + id + ": a vertex inside its helmet");
                Assert.LessOrEqual(max, 1.25f, "visor " + id + ": floating off its helmet (over 25 % of its radius)");
                Assert.Greater(front, 0, "visor " + id + ": not on the front (+z)");
            }
        }

        [Test]
        public void EverySpecIsWholePartsWithValidAnchors()
        {
            foreach (string kind in Base.Keys)
            {
                float[] s = Models43.Spec(kind);
                Assert.IsNotNull(s, kind);
                Assert.AreEqual(0, s.Length % UnityTuning.Models43Stride, kind + ": the spec is not whole parts");
                for (int o = 0, k = 0; o < s.Length; o += UnityTuning.Models43Stride, k++)
                {
                    Assert.That((int)s[o], Is.InRange(0, Mesh43.Count - 1), kind + ": a mesh id");
                    int a = (int)s[o + 1];
                    Assert.IsTrue(a == 0 || (a >= 1 && a <= Base[kind]) || (a >= 100 && a - 100 < k), $"{kind}: part {k}'s anchor {a}");
                    Assert.That(s[o + 11], Is.InRange(0f, 6f), kind + ": a role");
                    Assert.That(s[o + 12], Is.InRange(0f, 2f), kind + ": a group");
                }
            }
        }

        /// <summary>A bare body as the kind's view makes it: its renderers (a primitive mesh each) under a root.</summary>
        static Renderer[] FakeBody(string kind, Transform root)
        {
            var rends = new Renderer[Base[kind]];
            var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh cube = probe.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(probe);
            for (int i = 0; i < rends.Length; i++)
            {
                var go = new GameObject(kind + " base " + i);
                go.transform.SetParent(root, false);
                go.AddComponent<MeshFilter>().sharedMesh = cube;
                rends[i] = go.AddComponent<MeshRenderer>();
            }
            return rends;
        }

        static int ActiveRenderers(Transform root)
        {
            int n = 0;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(false)) n++;
            return n;
        }

        [Test]
        public void EveryKindStaysInsideItsBudgetAndTheSwapIsReversible()
        {
            foreach (string kind in Base.Keys)
            {
                var root = new GameObject("models43 test " + kind);
                try
                {
                    Renderer[] rends = FakeBody(kind, root.transform);
                    Mesh before = rends[0].GetComponent<MeshFilter>().sharedMesh;
                    Mesh[] own = kind == "drone" ? new[] { Mesh43.Scaled(Mesh43.Saucer, 0.6f), Mesh43.Scaled(Mesh43.Ball, 0.6f) } : null;
                    var l = new Look43(kind, root.transform, rends, BaseIds(kind), own, null, kind == "drone" ? 1f : 2f);
                    l.Apply(true, null);
                    Assert.IsTrue(l.on, kind);
                    Assert.IsNotNull(l.extras, kind + ": no extra parts");
                    if (kind == "husk") l.extras.ShowGroup(1);
                    Assert.LessOrEqual(ActiveRenderers(root.transform), Budget[kind], kind + ": over its renderer budget");
                    Assert.AreNotSame(before, rends[0].GetComponent<MeshFilter>().sharedMesh, kind + ": the body kept its primitive");
                    Assert.AreEqual(0, root.GetComponentsInChildren<Collider>(true).Length, kind + ": a part with a collider (the hit shapes are the sim's)");
                    l.Apply(false, null);
                    Assert.AreSame(before, rends[0].GetComponent<MeshFilter>().sharedMesh, kind + ": OFF did not put the primitive back");
                    Assert.AreEqual(Base[kind], ActiveRenderers(root.transform), kind + ": OFF left extra parts showing");
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        [Test]
        public void TheHuskAndTheTankEachShowTheirOwn()
        {
            var root = new GameObject("models43 test husk groups");
            try
            {
                Renderer[] rends = FakeBody("husk", root.transform);
                var l = new Look43("husk", root.transform, rends, UnityTuning.Models43HuskBase, null, null, 1.8f);
                l.Apply(true, null);
                l.extras.ShowGroup(1);
                int husk = ActiveRenderers(root.transform);
                l.extras.ShowGroup(2);
                int tank = ActiveRenderers(root.transform);
                Assert.AreEqual(6, husk, "a husk: body, helmet, slit visor, gorget, chest plate, back spines");
                Assert.AreEqual(6, tank, "a tank: body, helmet, double slit, gorget, pauldrons, exhaust");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void NightFortyPartsHideWhileModels43DrawsAndComeBack()
        {
            var root = new GameObject("models43 test m40");
            try
            {
                Renderer[] rends = FakeBody("charger", root.transform);
                Enemy40Parts m40 = Enemy40Parts.Make("charger", root.transform, null, 2.2f);
                int withM40 = ActiveRenderers(root.transform);
                var l = new Look43("charger", root.transform, rends, UnityTuning.Models43ChargerBase, null, null, 2.2f);
                l.Apply(true, m40);
                Assert.IsFalse(m40.Shown, "NIGHT40's parts still shown under models43");
                Assert.AreSame(l.extras, l.DyingParts(m40), "a death breaks models43's parts while it draws");
                l.Apply(false, m40);
                Assert.IsTrue(m40.Shown);
                Assert.AreEqual(withM40, ActiveRenderers(root.transform), "OFF: NIGHT40's picture back");
                Assert.AreSame(m40, l.DyingParts(m40));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
