// NIGHT43 builder F1 (design.n43.feel43): the pure half of the night's feel (Game/Feel43Math.cs): a fake FUSION event on a bare bus starts one
// burst (W1's sim emits the real one), the burst's timeline returns to rest, the trauma waits for the freeze, the rumble words keep their
// priority and their log, the gun reads grow with the gun. No engine objects (no SurvivalGame): the views' Unity half is the reel's (feel43).
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class Feel43Tests
    {
        sealed class Recorder : IHapticsSink
        {
            public bool can = true;
            public readonly List<double[]> played = new List<double[]>();
            public bool CanPlay => can;
            public double Strength => 1;
            public void Play(double strong, double weak, double ms) => played.Add(new[] { strong, weak, ms });
        }

        [Test]
        public void AFakeFusionEvent_OnTheBus_StartsOneBurst_ThePlayersOnly()
        {
            var bus = new EventBus();
            var bursts = new List<Fusion43Burst>();
            bus.On<Fusion43Event>(N43Events.Fusion, e =>
            {
                if (Fusion43Burst.For(e, out Fusion43Burst b)) bursts.Add(b);
            });
            var ev = new Fusion43Event { owner = "player", position = new Vec3(1, 1, 2), speed = 18, weapon = "gauntlet", kind = "husk", damage = 64 };
            bus.Emit(N43Events.Fusion, ev);
            Assert.AreEqual(1, bursts.Count, "one burst for the player's FUSION");
            Assert.IsTrue(bursts[0].live && !bursts[0].big && bursts[0].traumaDue);
            bus.Emit(N43Events.Fusion, new Fusion43Event { owner = "rival", kind = "husk" });
            Assert.AreEqual(1, bursts.Count, "another owner's FUSION draws nothing");
            bus.Emit(N43Events.Fusion, new Fusion43Event { owner = "player", kind = "boss" });
            Assert.AreEqual(2, bursts.Count);
            Assert.IsTrue(bursts[1].big, "a boss FUSION is the big burst");
            Assert.AreEqual("fusion43", N43Events.Fusion, "GG's sound hook keeps its name");
        }

        [Test]
        public void TheBurst_Converges_Flashes_BlowsOut_AndReturnsToRest()
        {
            double meet = UnityTuning.Fusion43ConvergeTime;
            Assert.AreEqual(0, Fusion43Burst.Converge(0));
            Assert.Less(Fusion43Burst.Converge(meet * 0.5), 0.5, "ease-in: pulled, slow first");
            Assert.AreEqual(1, Fusion43Burst.Converge(meet));
            Assert.AreEqual(0, Fusion43Burst.Core(0), "no core before the forces meet");
            Assert.AreEqual(1, Fusion43Burst.Core(meet), 1e-9, "the core peaks at the meeting");
            Assert.AreEqual(0, Fusion43Burst.Core(meet + UnityTuning.Fusion43CoreTime + 0.01));
            Assert.AreEqual(0, Fusion43Burst.Ring(meet - 0.01, 0).k, "no ring before the meeting");
            var mid = Fusion43Burst.Ring(meet + UnityTuning.Fusion43RingTime * 0.5, 0);
            Assert.Greater(mid.radius, 0.5, "ease-out: the ring is past half its size halfway");
            Assert.Greater(mid.k, 0);
            Assert.AreEqual(0, Fusion43Burst.Ring(meet + UnityTuning.Fusion43RingTime + 0.01, 0).k);
            Assert.AreEqual(1, Fusion43Burst.Kick(0), 1e-9);
            Assert.AreEqual(0, Fusion43Burst.Kick(UnityTuning.Fusion43KickTime));
            var w0 = Fusion43Burst.Word(0);
            Assert.AreEqual(UnityTuning.Fusion43WordFrom, w0.scale, 1e-9, "the word is punched in big");
            Assert.AreEqual(1, w0.alpha);
            Assert.AreEqual(1, Fusion43Burst.Word(UnityTuning.Fusion43WordPunch).scale, 1e-6, "settled at 1x after the punch");
            Assert.AreEqual(0, Fusion43Burst.Word(UnityTuning.Fusion43WordPunch + UnityTuning.Fusion43WordHold + UnityTuning.Fusion43WordFade + 0.01).alpha);
            Assert.LessOrEqual(UnityTuning.Fusion43WordPunch + UnityTuning.Fusion43WordHold + UnityTuning.Fusion43WordFade, UnityTuning.Fusion43Rest);
            Assert.GreaterOrEqual(UnityTuning.Fusion43Rest, meet + System.Math.Max(UnityTuning.Fusion43RingTime, UnityTuning.Fusion43SparkLife));
            Assert.GreaterOrEqual(UnityTuning.Fusion43WordPx, 16f, "UI text >= 16 px (rule 8)");
        }

        [Test]
        public void TheTrauma_WaitsForTheFreeze_ThenLetsGoOnce()
        {
            Fusion43Burst b = Fusion43Burst.Start(false);
            int releases = 0, frame = 0;
            double t = 0, dt = 1.0 / 60, freeze = 0.22;
            while (b.live && frame < 600)
            {
                double left = freeze - t; // W1's 0.22 s freeze, counting down on real time
                if (b.Advance(dt, left))
                {
                    releases++;
                    Assert.GreaterOrEqual(t + dt, freeze - 1e-9, "the trauma lets go only when the freeze is over");
                }
                t += dt;
                frame++;
            }
            Assert.AreEqual(1, releases);
            Assert.IsFalse(b.live, "the burst ends");
            Assert.LessOrEqual(t, UnityTuning.Fusion43Rest + 2 * dt);
            // a freeze that never ends (refused budget reads 0, a stuck one is capped): at most Fusion43TraumaWaitMax
            Fusion43Burst c = Fusion43Burst.Start(true);
            double waited = 0;
            while (!c.Advance(dt, 99)) waited += dt;
            Assert.LessOrEqual(waited, UnityTuning.Fusion43TraumaWaitMax + 1e-9);
        }

        [Test]
        public void RumbleWords_ACrackIsNotCutByALightShot_EveryAskIsLogged()
        {
            var rec = new Recorder();
            var h = new Feel43Haptics(rec);
            Assert.IsTrue(h.Fire(Feel43Haptics.Crack));
            Assert.AreEqual(1.0, rec.played[0][0]);
            Assert.AreEqual(1.0, rec.played[0][1]);
            Assert.AreEqual(120, rec.played[0][2]);
            Assert.IsFalse(h.Fire(Feel43Haptics.Light), "a light shot does not cut the crack");
            Assert.AreEqual(Feel43Haptics.Crack, h.RunningName);
            h.Tick(0.125); // the crack's 120 ms is over: the thud's next step
            Assert.AreEqual(2, rec.played.Count);
            Assert.AreEqual(0.7, rec.played[1][0]);
            for (int i = 0; i < 20; i++) h.Tick(0.05);
            Assert.IsNull(h.RunningName, "the crack ends");
            Assert.AreEqual(4, rec.played.Count, "four steps: the crack and its fading thud");
            Assert.IsTrue(h.Fire(Feel43Haptics.Buzz));
            Assert.IsTrue(h.Fire(Feel43Haptics.Buzz), "the TORCH's buzz re-asked each tick holds");
            Assert.IsTrue(h.Fire(Feel43Haptics.Release), "a charged release cuts the buzz");
            Assert.AreEqual(5, h.LogCount, "every word asked is logged");
            StringAssert.Contains("crack 1.00/1.00 350ms;", h.LogLine());
            StringAssert.Contains("light 0.12/0.32 45ms (not played);", h.LogLine());
            rec.can = false; // no pad in use: nothing plays, the ask is still logged (the reel reads it on a machine with no pad)
            Assert.IsFalse(h.Fire(Feel43Haptics.Crack));
            Assert.AreEqual(6, h.LogCount);
            StringAssert.EndsWith("crack 1.00/1.00 350ms (not played);", h.LogLine());
        }

        [Test]
        public void TheWords_ForEachShot_AndTheirPatterns()
        {
            Assert.AreEqual(Feel43Haptics.Thud, Feel43Math.FireWord("shotgun"));
            Assert.AreEqual(Feel43Haptics.Buzz, Feel43Math.FireWord("torch"));
            Assert.AreEqual(Feel43Haptics.Rail, Feel43Math.FireWord("rail"));
            Assert.AreEqual(Feel43Haptics.Light, Feel43Math.FireWord("revolver"));
            Assert.IsNull(Feel43Math.FireWord(null));
            foreach (string w in Feel43Haptics.Names)
            {
                double[] p = Feel43Haptics.Pattern(w);
                Assert.IsNotNull(p, w);
                Assert.AreEqual(0, p.Length % 3, w + ": [low, high, ms] triples");
                for (int i = 0; i < p.Length; i += 3)
                {
                    Assert.That(p[i], Is.InRange(0.0, 1.0), w);
                    Assert.That(p[i + 1], Is.InRange(0.0, 1.0), w);
                    Assert.Greater(p[i + 2], 0, w);
                }
            }
            Assert.Greater(Feel43Haptics.Priority(Feel43Haptics.Crack), Feel43Haptics.Priority(Feel43Haptics.Release));
            Assert.Greater(Feel43Haptics.Priority(Feel43Haptics.Release), Feel43Haptics.Priority(Feel43Haptics.Thud));
            Assert.Greater(Feel43Haptics.Priority(Feel43Haptics.Thud), Feel43Haptics.Priority(Feel43Haptics.Light));
        }

        [Test]
        public void GunReads_GrowWithTheGun_TheTorchRamps_TheOpenBossReads()
        {
            float[] nail = Feel43Math.GunHit("nailgun"), kes = Feel43Math.GunHit("revolver"), rail = Feel43Math.GunHit("rail");
            Assert.Less(nail[0], kes[0]);
            Assert.Less(kes[0], rail[0]);
            Assert.Less(nail[1], kes[1]);
            Assert.Less(kes[1], rail[1]);
            Assert.AreSame(UnityTuning.Gun43Default, Feel43Math.GunHit("zapper"));
            Assert.IsTrue(Feel43Math.IsGunHit(new HitEvent { owner = "player", weapon = "revolver" }));
            Assert.IsFalse(Feel43Math.IsGunHit(new HitEvent { owner = "player", weapon = "gauntlet", melee = true }), "the gauntlet is KillFeel42's / the FUSION's");
            Assert.IsFalse(Feel43Math.IsGunHit(new HitEvent { owner = "player", weapon = "shotgun", shock = true }), "a wave's share is not a hit read");
            Assert.IsFalse(Feel43Math.IsGunHit(new HitEvent { owner = "rival", weapon = "revolver" }));
            Assert.AreEqual(UnityTuning.Torch43RampFrom, Feel43Math.TorchRamp(0), 1e-9);
            Assert.AreEqual(1, Feel43Math.TorchRamp(UnityTuning.Torch43RampTime), 1e-9);
            Assert.AreEqual(1, Feel43Math.TorchRamp(5), 1e-9);
            Assert.IsTrue(Feel43Math.BossOpenHit(true, true, false, 10), "the boss bar says OPEN");
            Assert.IsTrue(Feel43Math.BossOpenHit(false, false, true, 10), "the WICK's own core hit");
            Assert.IsFalse(Feel43Math.BossOpenHit(true, false, false, 10), "a locked / closed boss: the plain read");
            Assert.IsFalse(Feel43Math.BossOpenHit(true, true, false, 0), "no damage, no read");
        }

        sealed class FakeSlug43
        {
            public string owner = "player";
            public Vec3 position = new Vec3(1, 2, 3);
            public double radius = 2.5;
            public bool killed = true;
            public int blastKills = 2;
        }

        [Test]
        public void Ev43_ReadsW1sPayloadByFieldName_ElseTheFallback()
        {
            var e = new FakeSlug43();
            Assert.AreEqual(2.5, Ev43.Get(e, "radius", 0.0), 1e-9);
            Assert.AreEqual(3, Ev43.Get(e, "position", default(Vec3)).z, 1e-9);
            Assert.IsTrue(Ev43.Get(e, "killed", false));
            Assert.AreEqual(2, Ev43.Get(e, "blastKills", 0));
            Assert.AreEqual("player", Ev43.Get(e, "owner", "x"));
            Assert.AreEqual(7, Ev43.Get(e, "nope", 7), "a missing field: the fallback");
            Assert.AreEqual(0.0, Ev43.Get(e, "owner", 0.0), "a field of another type: the fallback");
            Assert.AreEqual(5, Ev43.Get<int>(null, "radius", 5));
        }

        [Test]
        public void TheFusionPink_IsNotRivalsMagenta()
        {
            Assert.AreNotEqual(UnityTuning.RivalMagentaHex, UnityTuning.Fusion43PinkHex, "D-3: the FUSION's pink is its own");
            uint p = UnityTuning.Fusion43PinkHex, m = UnityTuning.RivalMagentaHex;
            Assert.Greater((p >> 8) & 0xff, (m >> 8) & 0xff, "hotter / whiter: more green than RIVAL's");
        }
    }
}
