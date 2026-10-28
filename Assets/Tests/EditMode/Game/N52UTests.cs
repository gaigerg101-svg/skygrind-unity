// NIGHT52 builder U: hud52's pure rules (Hud52Math, UiHud52.cs) and skin52's numbers (UnityTuning.U52.cs). Pure: no scene, no sim. The rows' off
// state (design.n52.hud52 / skin52 / guns52 off by default, --off52 turning them off) is pinned by N52SharedTests.
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N52UTests
    {
        [Test]
        public void Hud52_RedOrangeOnlyBelow30()
        {
            Assert.That(Hud52Math.Low(1f), Is.False);
            Assert.That(Hud52Math.Low(0.30f), Is.False);
            Assert.That(Hud52Math.Low(0.299f), Is.True);
            Assert.That(Hud52Math.Low(0f), Is.True);
        }

        [Test]
        public void Hud52_Trauma_AddsClampsAndSettles()
        {
            float t = Hud52Math.AddTrauma(0f, 0.01f);
            Assert.That(t, Is.GreaterThan(UnityTuning.Hud52TraumaBase - 1e-4f)); // a chip still twitches
            float big = Hud52Math.AddTrauma(0f, 0.3f);
            Assert.That(big, Is.GreaterThan(t)); // a bigger hit, a bigger shake
            Assert.That(Hud52Math.AddTrauma(0.9f, 0.5f), Is.EqualTo(1f)); // hits add, clamped
            float s = 1f;
            for (int i = 0; i < 30; i++) s = Hud52Math.Decay(s, 1.0 / 60); // 0.5 s
            Assert.That(s, Is.EqualTo(0f)); // juice returns to rest
        }

        [Test]
        public void Hud52_Shake_ZeroAtRest_BoundedAtFull()
        {
            Vector2 o = Hud52Math.Shake(0f, 1.234, out float roll);
            Assert.That(o.magnitude, Is.EqualTo(0f));
            Assert.That(roll, Is.EqualTo(0f));
            float peak = 0f;
            for (int i = 0; i < 120; i++)
            {
                Vector2 f = Hud52Math.Shake(1f, i / 60.0, out float r);
                peak = Mathf.Max(peak, f.magnitude);
                Assert.That(Mathf.Abs(r), Is.LessThanOrEqualTo(UnityTuning.Hud52ShakeRollDeg + 1e-4f));
            }
            Assert.That(peak, Is.GreaterThan(UnityTuning.Hud52ShakePx * 0.5f));
            Assert.That(peak, Is.LessThanOrEqualTo(UnityTuning.Hud52ShakePx * 1.23f)); // sqrt(1 + 0.7^2)
        }

        [Test]
        public void Hud52_PopAndKick_ReturnToRest()
        {
            Assert.That(Hud52Math.Pop(double.MaxValue), Is.EqualTo(1f));
            Assert.That(Hud52Math.Pop(UnityTuning.Hud52PopTime), Is.EqualTo(1f));
            float peak = 1f;
            for (int i = 0; i <= 20; i++) peak = Mathf.Max(peak, Hud52Math.Pop(UnityTuning.Hud52PopTime * i / 20.0));
            Assert.That(peak, Is.GreaterThan(1.03f));
            Assert.That(Hud52Math.Kick(double.MaxValue), Is.EqualTo(0f));
            Assert.That(Hud52Math.Kick(0), Is.EqualTo(UnityTuning.Hud52KickPx).Within(0.01f));
            Assert.That(Hud52Math.Kick(UnityTuning.Hud52SwapTime), Is.EqualTo(0f));
            Assert.That(Hud52Math.Fade(0, 0.4), Is.EqualTo(1f));
            Assert.That(Hud52Math.Fade(0.4, 0.4), Is.EqualTo(0f));
        }

        [Test]
        public void Teach52_Chip_KeepsTheVerbLineReadable()
        {
            // the card's verb line at its smallest (declutter51's floor, hud47's floor) shrunk to the chip stays >= 16 px at 1080p (CLAUDE.md rule 8)
            foreach (float px in new[] { UnityTuning.N51TeachHeadMinPx, UnityTuning.N51TeachHeadPx, UnityTuning.Hud47TeachHeadMinPx, UnityTuning.Hud47TeachHeadPx })
            {
                float scale = Mathf.Max(UnityTuning.Teach52ChipScale, UnityTuning.Teach52ChipMinPx / px);
                Assert.That(px * scale, Is.GreaterThanOrEqualTo(16f), "head " + px);
                Assert.That(scale, Is.LessThan(1f), "head " + px + " still shrinks");
            }
        }

        [Test]
        public void Teach52Verbs_EveryLineIsTeach35s_EveryVerbIsHeard()
        {
            // round 14 (UiTeach52.Verb.cs): each listed line is one of design.n35.teach35's lines, each verb one the view listens for, the safety
            // timeout well past NIGHT51's timer and the minimum under it (a line done at once still stands a moment, never past the old time)
            string root;
            try { root = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { root = System.IO.Path.GetFullPath("PortExport"); }
            var c = Skygrind.Core.GameConfig.Load(System.IO.Path.Combine(root, "config.json"));
            var table = Skygrind.Core.Teach35Table.From(c);
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var l in table.lines) ids.Add(l.id);
            var heard = new System.Collections.Generic.HashSet<string> { "dash", "slide", "wallride", "walljump", "grapple", "jumpdash", "fire", "charge", "whip", "punch" };
            Assert.That(UnityTuning.Teach52Verbs.Count, Is.GreaterThan(0));
            foreach (var kv in UnityTuning.Teach52Verbs)
            {
                Assert.That(ids, Has.Member(kv.Key), $"{kv.Key} is not a teach35 line");
                string v = kv.Value.StartsWith("fire:") ? "fire" : kv.Value;
                Assert.That(heard, Has.Member(v), $"{kv.Key}: no event for '{kv.Value}'");
            }
            Assert.That(UnityTuning.Teach52Verbs, Does.ContainKey(UnityTuning.Teach52ReelVerbId)); // the a152 reel's line waits for a verb
            Assert.That(UnityTuning.Teach52VerbSafetyS, Is.GreaterThanOrEqualTo(2 * table.showS));
            Assert.That(UnityTuning.Teach52VerbMinS, Is.LessThan(table.showS));
            Assert.That(UnityTuning.Teach52ReelVerbWaitS, Is.GreaterThan(table.showS).And.LessThan(UnityTuning.Teach52VerbSafetyS));
        }

        static Skygrind.Core.GameConfig Look52Cfg(bool on)
        {
            string root;
            try { root = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { root = System.IO.Path.GetFullPath("PortExport"); }
            var c = Skygrind.Core.GameConfig.Load(System.IO.Path.Combine(root, "config.json"));
            var design = Skygrind.Core.PortData.Obj(c.root["design"]);
            var n52 = Skygrind.Core.PortData.Obj(design["n52"]);
            // the row as the lead's config carries it (d062cccc + round 7's gradeLevels), forced on / off here
            n52["look52"] = new System.Collections.Generic.Dictionary<string, object>
            {
                ["enabled"] = on,
                ["numbers"] = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["gradeLevels"] = new System.Collections.Generic.List<object> { "n47-iceline", "n47-span", "n52-iceline", "n52-span" },
                },
            };
            return c;
        }

        [Test]
        public void Look52_GradeLevels_ListIcelineAndSpan_OnlyWithLook52On()
        {
            int was = Skygrind.Core.NightRows.ForceOffFrom;
            bool? force = Look52.Force;
            try
            {
                Skygrind.Core.NightRows.ForceOffFrom = Skygrind.Core.NightRows.NoOff;
                Look52.Force = null;
                Skygrind.Core.GameConfig off = Look52Cfg(false), on = Look52Cfg(true);
                // look52 off: look49's gate is today's (iceline / span unlisted; look49's own list untouched)
                Assert.That(Look52Lift.GradeListed(off, "n47-iceline"), Is.False);
                Assert.That(Look52Lift.GradeListed(off, "n47-span"), Is.False);
                Assert.That(PostFx.Look49Listed(PostFx.Look49LevelsOf(off), "n47-iceline"), Is.False);
                // look52 on: listed (with the wall lift); another level is not
                Assert.That(Look52Lift.GradeListed(on, "n47-iceline"), Is.True);
                Assert.That(Look52Lift.GradeListed(on, "n52-span"), Is.True);
                Assert.That(Look52Lift.GradeListed(on, "n47-kiln"), Is.False);
                Assert.That(PostFx.Look49Listed(PostFx.Look49LevelsOf(on), "n47-iceline"), Is.False, "look49's own list never takes them");
                // the off switch (--off52 / -skygrindOff52) covers it: unlisted again
                Skygrind.Core.NightRows.ForceOffFrom = 52;
                Assert.That(Look52Lift.GradeListed(Look52Cfg(true), "n47-iceline"), Is.False);
            }
            finally
            {
                Skygrind.Core.NightRows.ForceOffFrom = was;
                Look52.Force = force;
            }
        }

        [Test]
        public void Look52Tier_LedgesLifted_SameHueAndSaturation()
        {
            Color top = new Color(0.40f, 0.44f, 0.30f), side = new Color(0.30f, 0.34f, 0.22f); // an olive pair (spire's read)
            Look52Tier.Colours("floor", top, side, out Color ft, out Color fs);
            Look52Tier.Colours("wall", top, side, out Color wt, out Color ws);
            Look52Tier.Colours("ledge", top, side, out Color lt, out Color ls);
            Assert.That(ws, Is.EqualTo(side)); // the walls keep today's value (the mid)
            Assert.That(wt, Is.EqualTo(top));
            Assert.That(ft.maxColorComponent, Is.LessThanOrEqualTo(wt.maxColorComponent)); // the floor never lifted (round 9: NIGHT51's)
            Assert.That(ls.maxColorComponent, Is.GreaterThan(ws.maxColorComponent)); // a ledge's face lifted against the wall behind it
            Assert.That(lt.maxColorComponent, Is.GreaterThan(ft.maxColorComponent)); // a deck's top over the floor
            foreach ((Color a, Color b) in new[] { (ft, top), (lt, top), (ls, side) })
            {
                Color.RGBToHSV(a, out float ha, out float sa, out _);
                Color.RGBToHSV(b, out float hb, out float sb, out _);
                Assert.That(ha, Is.EqualTo(hb).Within(1e-4f)); // the level's hue kept
                Assert.That(sa, Is.EqualTo(sb).Within(1e-4f)); // no new saturation
            }
            Assert.That(UnityTuning.Look52TierLevels, Has.Member("n52-foundry"));
            Assert.That(UnityTuning.Look52TierLevels, Has.No.Member("n52-spire")); // round 11: spire's tiers read RED in n52-r15 (foundry only ships)
        }

        static Mesh Gun52(string id, bool machined)
        {
            bool was = BlockyKit.Guns52, wasBig = BlockyKit.Guns52Big;
            BlockyKit.Guns52 = machined;
            BlockyKit.Guns52Big = machined && id == "shotgun"; // round 3: the THUNDERCLAP's big build (wider, brighter chamfers, the stripes)
            try
            {
                var body = new MeshKit();
                var part = new MeshKit();
                Color fam = new Color(0.1f, 0.83f, 0.9f);
                if (id == "revolver") BlockyKit.Kestrel(body, part, fam);
                else BlockyKit.Thunderclap(body, fam, part);
                return body.Build(id + (machined ? " machined" : " plain"));
            }
            finally { BlockyKit.Guns52 = was; BlockyKit.Guns52Big = wasBig; }
        }

        [Test]
        public void Guns52_Machined_KeepsTheSilhouette_OffIsPlain()
        {
            Assert.That(BlockyKit.Guns52, Is.False); // never left on outside the viewmodel's two builds
            foreach (string id in new[] { "revolver", "shotgun" })
            {
                Mesh plain = Gun52(id, false), plain2 = Gun52(id, false), machined = Gun52(id, true);
                try
                {
                    Assert.That(plain2.vertexCount, Is.EqualTo(plain.vertexCount), id + ": off builds NIGHT51's mesh");
                    Assert.That(machined.vertexCount, Is.GreaterThan(plain.vertexCount), id + ": the machined build ran");
                    Assert.That((machined.bounds.min - plain.bounds.min).magnitude, Is.LessThan(1e-4f), id + ": no new silhouette (min)");
                    Assert.That((machined.bounds.max - plain.bounds.max).magnitude, Is.LessThan(1e-4f), id + ": no new silhouette (max)");
                }
                finally
                {
                    Object.DestroyImmediate(plain);
                    Object.DestroyImmediate(plain2);
                    Object.DestroyImmediate(machined);
                }
            }
        }

        [Test]
        public void Skin52_Numbers_KeepTheFarReadAndTheHue()
        {
            Assert.That(UnityTuning.Skin52RimNearM, Is.LessThan(UnityTuning.Skin52RimFarM));
            Assert.That(UnityTuning.Skin52RimFarM, Is.LessThanOrEqualTo((float)UnityTuning.EnemyRimBoostNear)); // the far boost starts where the cut ends
            Assert.That(UnityTuning.Skin52RimNearShare, Is.InRange(0.05f, 1f));
            Assert.That(UnityTuning.Skin52RampSoft, Is.GreaterThan(0f));
            // the shadow keeps the body's hue: darker on every channel, cooler (blue the least cut)
            Assert.That(UnityTuning.Skin52ShadeR, Is.LessThan(1f));
            Assert.That(UnityTuning.Skin52ShadeG, Is.LessThan(1f));
            Assert.That(UnityTuning.Skin52ShadeB, Is.LessThan(1f));
            Assert.That(UnityTuning.Skin52ShadeB, Is.GreaterThan(UnityTuning.Skin52ShadeR));
            // a red-orange body (colors.enemy) in the shadow is still red-orange: OKLab hue within 25 deg
            Color body = Look.Hex(UnityTuning.Look49DangerHex).linear;
            Color shade = new Color(body.r * UnityTuning.Skin52ShadeR, body.g * UnityTuning.Skin52ShadeG, body.b * UnityTuning.Skin52ShadeB).gamma;
            float gap = Mathf.Abs(Hud47Math.HueDeg(shade) - Hud47Math.HueDeg(Look.Hex(UnityTuning.Look49DangerHex)));
            Assert.That(Mathf.Min(gap, 360f - gap), Is.LessThan(25f));
        }
    }
}
