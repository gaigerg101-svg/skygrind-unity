// NIGHT43 builder A1 (burrow43; NIGHT43.md card A1, G28): the BURROWER's rules in the pure sim (Core/Enemies/Burrower43.cs through Husks, a box
// world): off, "burrower" is a plain husk (NIGHT42's game); on, a player standing still is hurt by the emergence 0.8 s after the tell; a buried
// one takes nothing; a hit in the window is a counter (x2, one event); a raised plate is safe. The harness `n43burrow` measures the rest.
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class Burrower43Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));

        static Husks Make(bool on, bool plate, out EventBus events, out Health hp)
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            if (plate) w.AddBox(new Vec3(-2, 0, -2), new Vec3(2, 0.45, 2));
            events = new EventBus();
            var h = new Husks(Cfg, events, new EnemyRegistry(), w, -30);
            if (on)
            {
                h.burrow43 = Burrow43Params.From(Cfg);
                h.burrowBody43 = h.burrow43.Body(Cfg, -30);
            }
            hp = new Health(100000, events, "player");
            return h;
        }

        [Test]
        public void OffItIsAPlainHusk()
        {
            Husks h = Make(false, false, out _, out _);
            Husk b = h.Spawn("burrower", new Vec3(6, 0, 0), new Vec3(0, 0, 0));
            Assert.IsNotNull(b);
            Assert.IsFalse(b is Burrower43, "burrow43 off: an unknown kind is a husk, as before");
            Assert.AreEqual("husk", b.kind);
        }

        [Test]
        public void AStandingPlayerIsHurtByTheEmergenceAfterTheTell()
        {
            Husks h = Make(true, false, out EventBus ev, out Health hp);
            int tellStep = -1, emergeStep = -1, step = 0;
            ev.On<Burrow43Event>(N43Events.BurrowTell, e => { if (tellStep < 0) tellStep = step; });
            ev.On<Burrow43Event>(N43Events.BurrowEmerge, e => { if (emergeStep < 0) emergeStep = step; });
            var b = h.Spawn("burrower", new Vec3(6, 0, 0), new Vec3(0, 0, 0)) as Burrower43;
            Assert.IsNotNull(b, "burrow43 on: a BURROWER");
            Assert.AreEqual(0, h.CountOf("husk"), "it is not in the husks' pool");
            for (step = 0; step < 600 && emergeStep < 0; step++) h.Step(new Vec3(0, 0, 0), hp, 1.0 / 120);
            Assert.GreaterOrEqual(tellStep, 0, "no tell");
            Assert.AreEqual(MovementMath.StepsFor(b.Rules.tellSeconds, 1.0 / 120), emergeStep - tellStep, "tell -> emergence");
            Assert.AreEqual(100000 - b.Rules.damage, hp.hp, 1e-9, "the emergence's damage");
        }

        [Test]
        public void BuriedItTakesNothingAndTheWindowCounters()
        {
            Husks h = Make(true, false, out EventBus ev, out Health hp);
            int counters = 0;
            ev.On<Burrow43Event>(N43Events.BurrowCounter, e => counters++);
            var b = h.Spawn("burrower", new Vec3(6, 0, 0), new Vec3(0, 0, 0)) as Burrower43;
            for (int s = 0; s < 600 && b.state != Burrower43.Hunt; s++) h.Step(new Vec3(0, 0, 0), hp, 1.0 / 120);
            Assert.IsTrue(b.Buried);
            Assert.IsFalse(b.Damage(64, 0.08), "buried: no damage");
            Assert.AreEqual(b.maxHp, b.hp);
            Assert.Less(b.RayHit(new Vec3(6, 1, 5), new Vec3(0, 0, -1), out _), 0, "buried: no ray hits it");
            for (int s = 0; s < 600 && b.state != Burrower43.Emerge; s++) h.Step(new Vec3(0, 0, 0), hp, 1.0 / 120);
            Assert.Greater(b.windowSteps, 0);
            double before = b.hp;
            b.Damage(64, 0.08);
            b.Damage(64, 0.08);
            Assert.AreEqual(256, before - b.hp, 1e-9, "x2 for every hit in the window");
            Assert.AreEqual(1, counters, "one counter per emergence");
        }

        [Test]
        public void ARaisedPlateIsSafeGround()
        {
            Husks h = Make(true, true, out EventBus ev, out Health hp);
            int tells = 0;
            ev.On<Burrow43Event>(N43Events.BurrowTell, e => tells++);
            h.Spawn("burrower", new Vec3(6, 0, 0), new Vec3(0, 0.45, 0));
            for (int s = 0; s < 960; s++) h.Step(new Vec3(0, 0.45, 0), hp, 1.0 / 120);
            Assert.AreEqual(0, tells, "the player on a 0.45 m plate: never its ground");
            Assert.AreEqual(100000, hp.hp, 1e-9);
        }
    }
}
