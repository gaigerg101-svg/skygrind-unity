// NIGHT49 builder X (design.n49.fx49, G5): the impact kit's tables (UnityTuning.Fx49.cs) and rules (Fx49.cs). Pinned:
//   1. more damage = bigger, brighter, sharper: per owner, up the tiers the size, the glow, the white core and the spark count grow, the
//      sparks get longer and thinner; the timing table is whole steps, a bigger tier holds and fades longer
//   2. the envelope: lit on its first frame (a snap, never a fade-in), 1 through the hold, 0 after the tier's life
//   3. the tier rules: a KESTREL tap (25) and a pellet (8) light, a headshot (50) and a charged shot (100) heavy, the slug (192) critical;
//      the owners' colours are Look49's role tokens (the one palette)
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N49FxTests
    {
        [Test]
        public void TheLadder_BiggerBrighterSharper_UpTheTiers()
        {
            Assert.That(UnityTuning.Fx49Cells.Length, Is.EqualTo(12), "4 owners x 3 tiers");
            for (int o = 0; o < 4; o++)
                for (int t = 1; t < 3; t++)
                {
                    Fx49Cell lo = Fx49.Cell((Fx49Owner)o, (Fx49Tier)(t - 1)), hi = Fx49.Cell((Fx49Owner)o, (Fx49Tier)t);
                    string at = (Fx49Owner)o + " " + (Fx49Tier)t;
                    Assert.That(hi.size, Is.GreaterThan(lo.size), at + ": bigger");
                    Assert.That(hi.glow, Is.GreaterThan(lo.glow), at + ": brighter");
                    Assert.That(hi.core, Is.GreaterThan(lo.core), at + ": a whiter core");
                    Assert.That(hi.sparks, Is.GreaterThanOrEqualTo(lo.sparks), at + ": as many sparks or more");
                    Assert.That(hi.sparkLen, Is.GreaterThan(lo.sparkLen), at + ": longer sparks");
                    Assert.That(hi.sparkWidth, Is.LessThan(lo.sparkWidth), at + ": thinner (sharper) sparks");
                    Assert.That(hi.sparks, Is.LessThanOrEqualTo(10), "the kit keeps 10 directions a burst");
                }
            for (int t = 1; t < 3; t++)
            {
                Assert.That(UnityTuning.Fx49HoldSteps[t], Is.GreaterThan(UnityTuning.Fx49HoldSteps[t - 1]), "a bigger tier holds longer");
                Assert.That(UnityTuning.Fx49FadeSteps[t], Is.GreaterThan(UnityTuning.Fx49FadeSteps[t - 1]), "a bigger tier fades longer");
                Assert.That(Fx49.LifeS((Fx49Tier)t), Is.GreaterThan(Fx49.LifeS((Fx49Tier)(t - 1))));
            }
            for (int t = 0; t < 3; t++) Assert.That(UnityTuning.Fx49RiseSteps[t], Is.InRange(1, 2), "the snap-in: 1-2 steps (DOOM: tell me fast)");
        }

        [Test]
        public void TheEnvelope_SnapsHoldsFades()
        {
            foreach (Fx49Tier t in new[] { Fx49Tier.Light, Fx49Tier.Heavy, Fx49Tier.Critical })
            {
                Assert.That(Fx49.Env(t, 0f), Is.GreaterThanOrEqualTo(0.35f), t + ": lit on its first frame");
                Assert.That(Fx49.Env(t, Fx49.RiseS(t) + 0.5f / 120f), Is.EqualTo(1f).Within(1e-6), t + ": held at the peak");
                Assert.That(Fx49.Env(t, Fx49.LifeS(t) + 0.01f), Is.EqualTo(0f), t + ": gone after its life");
                float mid = Fx49.Env(t, (Fx49.PeakEndS(t) + Fx49.LifeS(t)) * 0.5f);
                Assert.That(mid, Is.InRange(0.1f, 0.5f), t + ": a quadratic ease-out (a quarter half way)");
                Assert.That(Fx49.Grow(t, Fx49.LifeS(t)), Is.EqualTo(1f).Within(1e-6), t + ": a ring has grown out by its end");
            }
            Assert.That(Fx49.Env(Fx49Tier.Light, -1f), Is.EqualTo(0f));
        }

        [Test]
        public void TheTiersAndOwners_FromTheDamageAndThePalette()
        {
            Assert.That(Fx49.HitTier(25), Is.EqualTo(Fx49Tier.Light), "a KESTREL tap");
            Assert.That(Fx49.HitTier(8), Is.EqualTo(Fx49Tier.Light), "a THUNDERCLAP pellet");
            Assert.That(Fx49.HitTier(50), Is.EqualTo(Fx49Tier.Heavy), "a KESTREL headshot");
            Assert.That(Fx49.HitTier(100), Is.EqualTo(Fx49Tier.Heavy), "a charged KESTREL shot (4 x 25)");
            Assert.That(Fx49.HitTier(192), Is.EqualTo(Fx49Tier.Critical), "the slug (8 x 8 x 3)");
            Assert.That(Fx49.HurtTier(5), Is.EqualTo(Fx49Tier.Light));
            Assert.That(Fx49.HurtTier(20), Is.EqualTo(Fx49Tier.Heavy));
            Assert.That(Fx49.HurtTier(50), Is.EqualTo(Fx49Tier.Critical));
            Assert.That(Fx49.OwnerOf("gauntlet"), Is.EqualTo(Fx49Owner.Gauntlet));
            Assert.That(Fx49.OwnerOf("revolver"), Is.EqualTo(Fx49Owner.Player));
            Assert.That(Fx49.OwnerOf("punch"), Is.EqualTo(Fx49Owner.Player), "the fist is the player's cyan, not a fourth blue");
            Assert.That(Fx49.Up(Fx49Tier.Critical), Is.EqualTo(Fx49Tier.Critical));
            Assert.That(Fx49.Hue(Fx49Owner.Player), Is.EqualTo(Look.Hex(UnityTuning.Look49UsableHex)), "player = Look49's usable cyan");
            Assert.That(Fx49.Hue(Fx49Owner.Enemy), Is.EqualTo(Look.Hex(UnityTuning.Look49DangerHex)), "enemy = Look49's danger");
            Assert.That(Fx49.Hue(Fx49Owner.Parry), Is.EqualTo(Look.Hex(UnityTuning.Look49ParryHex)), "parry = Look49's yellow");
            Assert.That(Fx49.ShapeOf(Fx49Owner.Enemy), Is.EqualTo(Fx49Shape.Shard), "the enemy's sharp shards");
            Assert.That(Fx49.ShapeOf(Fx49Owner.Parry), Is.EqualTo(Fx49Shape.Star), "the parry's four-point star");
            Fx49.Tones(Fx49Owner.Player, out Color core, out Color body, out Color rim);
            Assert.That(core.r, Is.GreaterThan(body.r), "the core is whiter than the body (red rises toward white on cyan)");
            Assert.That(rim.g, Is.LessThan(body.g), "the rim is the body's darker tone");
        }
    }
}
