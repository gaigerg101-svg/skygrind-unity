// NIGHT40 builder E3 (models40; NIGHT40.md card E3: "the renderer count per model at most 2x today's (an EditMode test the lead runs)"): each kind's
// silhouette parts (Game/Enemy40View.cs, UnityTuning.Models40.cs) are built on a bare root, as a view does with the row on, and counted against the
// renderers its view draws today (read from the views 2026-10-14: the husk 2 = body + head, also the tank's body; the swarmer 3 boxes; the spitter capsule +
// sac + box; the charger 3 boxes (its charge line is a separate floor decal, not the model); the warden body + shield + arm; the drone shell + core).
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Models40Tests
    {
        static readonly Dictionary<string, int> Today = new Dictionary<string, int>
        {
            { "husk", 2 }, { "swarmer", 3 }, { "spitter", 3 }, { "charger", 3 }, { "warden", 3 }, { "drone", 2 },
        };

        static readonly Dictionary<string, float> Height = new Dictionary<string, float>
        {
            { "husk", 1.8f }, { "swarmer", 0.75f }, { "spitter", 2f }, { "charger", 2.2f }, { "warden", 2.3f }, { "drone", 1f },
        };

        [Test]
        public void EveryModelStaysWithinTwiceTodaysRenderers()
        {
            foreach (KeyValuePair<string, int> k in Today)
            {
                var root = new GameObject("models40 test " + k.Key);
                try
                {
                    Enemy40Parts m = Enemy40Parts.Make(k.Key, root.transform, null, Height[k.Key]);
                    Assert.IsNotNull(m, k.Key + ": no parts");
                    int added = root.GetComponentsInChildren<Renderer>(true).Length;
                    Assert.AreEqual(m.rends.Length, added, k.Key + ": the parts' renderers");
                    Assert.Greater(added, 0, k.Key + ": no part added");
                    Assert.LessOrEqual(k.Value + added, 2 * k.Value, $"{k.Key}: {k.Value} today + {added} parts is over 2x");
                    Assert.AreEqual(0, root.GetComponentsInChildren<Collider>(true).Length, k.Key + ": a part kept a collider (the hit shapes are the sim's)");
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        [Test]
        public void EverySpecIsWholeParts()
        {
            foreach (string kind in Today.Keys)
            {
                float[] s = Enemy40Parts.Spec(kind);
                Assert.IsNotNull(s, kind);
                Assert.AreEqual(0, s.Length % UnityTuning.Models40Stride, kind + ": the spec is not whole parts");
                for (int o = 0; o < s.Length; o += UnityTuning.Models40Stride)
                {
                    Assert.That(s[o + 10], Is.InRange(0f, 4f), kind + ": a role");
                    Assert.That(s[o + 11], Is.InRange(0f, 2f), kind + ": a group");
                }
            }
        }

        [Test]
        public void TheHuskShowsItsOwnPartsAndTheTankItsOwn()
        {
            var root = new GameObject("models40 test husk groups");
            try
            {
                Enemy40Parts m = Enemy40Parts.Make("husk", root.transform, null, 1.8f);
                m.ShowGroup(1);
                int husk = 0;
                foreach (Transform t in m.parts) if (t.gameObject.activeSelf) husk++;
                m.ShowGroup(2);
                int tank = 0;
                foreach (Transform t in m.parts) if (t.gameObject.activeSelf) tank++;
                Assert.AreEqual(1, husk, "a husk shows its yoke only");
                Assert.AreEqual(1, tank, "a tank shows its slab only");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
