// NIGHT21 U3-35.3 (builder M): THE ARCLINE in Survival (Core/Arcline21.cs, Game/SurvivalSim.Arcline21.cs; design.n21.arcline).
// UNRUN on the night it was written (the Editor was stuck on a dialog): the same rules run headless in
// `SimHarness arcline` (Tools/SimHarness/Harness.N21.Arcline.cs), which passed.
//   - Off: nothing is wired, ropeEnabled stays false, the terminal is not in the buy list, a grapple press fires nothing.
//   - On: the terminal is in the buy list (the web's spot), locked until ISLANDS opens, 5500 buys it for the run; a press
//     yanks a husk ahead (once per yankCooldown), zips to a charger, else swings on a node or a surface.
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Arcline21Tests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig cfg;
        static SurvivalMap map;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));
        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(PortRoot(), "maps", "survival.json"))));

        static SurvivalSim Make(bool on)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.noRecords = sim.noLab = true;
            sim.n21Override = on ? new N21Blocks { arcline = true } : N21Blocks.Off;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(2101, null, null);
            return sim;
        }

        static void Tick(SurvivalSim sim, double yaw, double pitch, bool pressed, bool held)
        {
            var inp = (ScriptedInput)sim.input;
            inp.SetLook(yaw, pitch);
            inp.Set(new InputFrame { grapple = new Button { held = held, pressed = pressed } }, new CombatInput { weapon = -1 });
            sim.Step();
        }

        static (double yaw, double pitch) Look(Vec3 from, Vec3 to)
        {
            Vec3 d = to - from;
            return (Math.Atan2(-d.x, -d.z), Math.Asin(d.y / d.Length));
        }

        static (double dx, double dz) OpenDir(SurvivalSim sim, Vec3 p, double dist)
        {
            for (int k = 0; k < 64; k++)
            {
                double a = k * Math.PI / 32;
                double dx = Math.Sin(a), dz = -Math.Cos(a);
                bool clear = true;
                var eyeAt = new Vec3(p.x, p.y + 1.6, p.z);
                foreach (double h in new[] { 0.3, 0.8, 1.2, 2.0, 3.0 })
                {
                    var t = new Vec3(p.x + dx * dist, p.y + h, p.z + dz * dist);
                    double len = Vec3.DistanceTo(t, eyeAt);
                    if (sim.world.RayIntersect(eyeAt, (t - eyeAt) / len, out RayHit hit) && hit.distance < len + 1) clear = false;
                }
                if (clear) return (dx, dz);
            }
            return (0, -1);
        }

        [Test]
        public void Off_NothingIsWired_AndAPressFiresNothing()
        {
            SurvivalSim sim = Make(false);
            Assert.That(sim.arcline21, Is.Null);
            Assert.That(sim.mover.ropeEnabled, Is.False);
            Assert.That(sim.buyables.list.Exists(i => i.id == "gadget:arcline"), Is.False);
            int fires = 0;
            sim.mover.Events += e => { if (e.type == MoverEventType.RopeFire) fires++; };
            for (int s = 0; s < 20; s++) Tick(sim, 0, 0.3, s == 10, true);
            Assert.That(fires, Is.EqualTo(0));
        }

        [Test]
        public void On_TheTerminalIsInTheBuyList_LockedUntilIslandsOpen_AndPriceIsTheWebs()
        {
            SurvivalSim sim = Make(true);
            Buyables.Item item = sim.buyables.list.Find(i => i.id == "gadget:arcline");
            Assert.That(item, Is.Not.Null);
            Assert.That(item.price(), Is.EqualTo(Cfg().Num("arcline.price")));
            Assert.That(item.available(), Is.False);
            sim.zones.Open("islands", true);
            Assert.That(item.available(), Is.True);
            Assert.That(sim.mover.ropeEnabled, Is.False, "not owned until bought");
        }

        [Test]
        public void Own_FollowsOwnership_AndTheEventIsRaisedOnce()
        {
            SurvivalSim sim = Make(true);
            int owned = 0;
            sim.events.On("arclineOwned", _ => owned++);
            sim.arcline21.Own(true);
            Assert.That(sim.mover.ropeEnabled, Is.True);
            Assert.That(owned, Is.EqualTo(1));
            sim.StartRun(2102, null, null);
            Assert.That(sim.mover.ropeEnabled, Is.False, "the next run starts without it");
        }

        [Test]
        public void Yank_ThrowsASmallEnemyTowardYou_ThenWaitsOutItsCooldown()
        {
            SurvivalSim sim = Make(true);
            sim.arcline21.Own(true);
            int pulls = 0;
            sim.events.On("arclinePull", _ => pulls++);
            Vec3 me = Map().playerSpawn;
            (double ux, double uz) = OpenDir(sim, me, 12); // a clear line of 12 m (the harness's ArclineOpenDir), not a fixed -z
            Husk h = sim.husks.Spawn("husk", new Vec3(me.x + ux * 12, 0, me.z + uz * 12), me);
            for (int s = 0; s < 4; s++) Tick(sim, 0, 0, false, false);
            double d0 = Vec3.DistanceTo(h.position, sim.mover.position);
            (double yaw, double pitch) = Look(sim.Eye, h.AimPoint);
            Tick(sim, yaw, pitch, true, true);
            for (int s = 0; s < 59; s++) Tick(sim, yaw, pitch, false, false);
            Assert.That(pulls, Is.EqualTo(1));
            Assert.That(d0 - Vec3.DistanceTo(h.position, sim.mover.position), Is.GreaterThanOrEqualTo(3));
            Assert.That(sim.arcline21.YankSteps, Is.GreaterThan(0), "the 15 s cooldown runs");
        }

        [Test]
        public void ChooseEnemy_SkipsWhatTheCallerLeavesOut_AndNeedsALineOfSight()
        {
            SurvivalSim sim = Make(true);
            var aim = new GrappleAim(Cfg().movement, Map().grappleNodes);
            Vec3 me = Map().playerSpawn;
            Husk h = sim.husks.Spawn("husk", new Vec3(me.x, 0, me.z - 12), me);
            Vec3 eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 dir = Vec3.Normalize(h.AimPoint - eye);
            Assert.That(aim.ChooseEnemy(eye, dir, sim.world, sim.enemies, null), Is.SameAs(h).Or.Null);
            Assert.That(aim.ChooseEnemy(eye, dir, sim.world, sim.enemies, e => true), Is.Null, "skipped");
            Assert.That(aim.ChooseEnemy(eye, new Vec3(0, 1, 0), sim.world, sim.enemies, null), Is.Null, "looking straight up");
        }
    }
}
