// NIGHT17 U3-30.4 round 5 (builder M): the stamina bar's look (StaminaBarLook): the cyan ladder by state, the spent flash,
// the empty pulse, and "refilling" read from the Mover (design.stamina's bar and the web's pips).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class StaminaBarTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "config.json"));
        static double Dt => 1 / Cfg.movement.loop.simHz;

        static Mover Walker(bool bar)
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
            var m = new Mover(Cfg.movement, w);
            if (bar) m.SetStamina(Stamina.Params.From(Cfg));
            m.Reset(0, 0, 1000);
            for (int i = 0; i < 60; i++) m.Step(new InputFrame { moveZ = 1 }, Dt);
            return m;
        }

        static InputFrame Shift(bool held, bool pressed) => new InputFrame { moveZ = 1, dash = new Button { held = held, pressed = pressed } };

        [Test]
        public void Tint_TheCyanLadder_DeepLow_CyanFull_HotRefilling_WhiteSpent()
        {
            Assert.AreEqual(StaminaBarLook.Deep, StaminaBarLook.Tint(0.3, 0.5, false, false), "under a full boost's cost: low");
            Assert.AreEqual(StaminaBarLook.Full, StaminaBarLook.Tint(0.5, 0.5, false, false), "a full boost's cost");
            Assert.AreEqual(StaminaBarLook.Full, StaminaBarLook.Tint(1, 0.5, false, false), "full");
            Assert.AreEqual(StaminaBarLook.Refill, StaminaBarLook.Tint(0.3, 0.5, true, false), "refilling, low or not");
            Assert.AreEqual(StaminaBarLook.Refill, StaminaBarLook.Tint(0.8, 0.5, true, false));
            Assert.AreEqual(StaminaBarLook.Spent, StaminaBarLook.Tint(0.3, 0.5, true, true), "the spent flash over everything");
        }

        [Test]
        public void WasSpent_AChunkOrABarEmptied_NeverASprintFrameOrARefill()
        {
            Assert.IsTrue(StaminaBarLook.WasSpent(1, 0.5, 0.05), "a full boost");
            Assert.IsTrue(StaminaBarLook.WasSpent(0.5, 0.25, 0.05), "a dash-jump's extra");
            Assert.IsTrue(StaminaBarLook.WasSpent(0.2, 0.075, 0.05), "the shortest boost");
            Assert.IsFalse(StaminaBarLook.WasSpent(0.40, 0.39, 0.05), "a frame of the sprint's drain");
            Assert.IsTrue(StaminaBarLook.WasSpent(0.01, 0, 0.05), "the sprint emptied the bar");
            Assert.IsTrue(StaminaBarLook.WasSpent(1.01, 0.99, 0.05), "VIGOR's second bar emptied");
            Assert.IsFalse(StaminaBarLook.WasSpent(0.3, 0.31, 0.05), "a refill");
            Assert.IsFalse(StaminaBarLook.WasSpent(0, 0, 0.05), "still empty");
            Assert.IsFalse(StaminaBarLook.WasSpent(double.NaN, 0.5, 0.05), "no frame before");
        }

        [Test]
        public void FlashStep_StartsAtItsDuration_RunsOutWithTheFrames()
        {
            Assert.AreEqual(0.08, StaminaBarLook.FlashStep(0, true, 0.016, 0.08), 1e-12);
            Assert.AreEqual(0.064, StaminaBarLook.FlashStep(0.08, false, 0.016, 0.08), 1e-12);
            Assert.AreEqual(0, StaminaBarLook.FlashStep(0.01, false, 0.016, 0.08));
            Assert.AreEqual(0.08, StaminaBarLook.FlashStep(0.03, true, 0.016, 0.08), 1e-12, "a new spend starts it over");
        }

        [Test]
        public void EmptyPulse_UnderTheFloor_ASlowRaisedCosine()
        {
            Assert.IsTrue(StaminaBarLook.Empty(0.1, 0.125));
            Assert.IsFalse(StaminaBarLook.Empty(0.125, 0.125), "a boost fires at the floor");
            Assert.AreEqual(0, StaminaBarLook.Pulse(0, 1), 1e-12);
            Assert.AreEqual(1, StaminaBarLook.Pulse(0.5, 1), 1e-12);
            Assert.AreEqual(0.5, StaminaBarLook.Pulse(0.25, 1), 1e-12);
            Assert.AreEqual(0, StaminaBarLook.Pulse(1, 1), 1e-12);
            for (double t = 0; t < 3; t += 0.037)
            {
                double p = StaminaBarLook.Pulse(t, 1);
                Assert.That(p, Is.InRange(0.0, 1.0));
            }
            Assert.AreEqual(0, StaminaBarLook.PulseLevel(0, 12));
            Assert.AreEqual(12, StaminaBarLook.PulseLevel(1, 12));
            Assert.AreEqual(6, StaminaBarLook.PulseLevel(0.5, 12));
        }

        [Test]
        public void Refilling_ReadsTheSimsRefill_NotDuringTheDelayASprintOrAtFull()
        {
            Mover m = Walker(true);
            Assert.IsFalse(StaminaBarLook.Refilling(m), "full");
            m.Step(Shift(true, true), Dt);
            Assert.IsFalse(StaminaBarLook.Refilling(m), "boosting");
            for (int i = 0; i < 29; i++) m.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.AreEqual(MoveState.Ground, m.state);
            Assert.IsFalse(StaminaBarLook.Refilling(m), "inside the refill's delay");
            for (int i = 0; i < m.stamina.rules.delaySteps; i++) m.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.IsTrue(StaminaBarLook.Refilling(m), "past the delay, under the max");
            // a sprint drains, never refills
            Mover s = Walker(true);
            s.Step(Shift(true, true), Dt);
            for (int i = 0; i < 60; i++) s.Step(Shift(true, false), Dt);
            Assert.IsTrue(s.stamina.sprinting);
            Assert.IsFalse(StaminaBarLook.Refilling(s), "sprinting");
            // the web's pips: under MaxPips and not dashing
            Mover web = Walker(false);
            Assert.IsFalse(StaminaBarLook.Refilling(web));
            web.Step(Shift(true, true), Dt);
            Assert.IsFalse(StaminaBarLook.Refilling(web), "dashing");
            for (int i = 0; i < 40; i++) web.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.IsTrue(StaminaBarLook.Refilling(web));
        }
    }
}
