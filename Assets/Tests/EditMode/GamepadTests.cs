// NIGHT13 U3-26.7: the web's tests/gamepad.test.js (OVERNIGHT3 P3: 20 cases) ported against Core/Gamepad.cs on fake
// PadStates: dead zones, the response curve, the button -> action table, press edges, the hot-swap's activity test and
// the menu navigation's repeat, the keyboard path untouched (no pad, or a pad at rest: every field of InputFrame and
// CombatInput compared bit for bit through DoubleToInt64Bits, the web's JSON.stringify compare), the pad remap and raw
// presses. Plus one line for PORT-RULES section 6: the per-frame pad path allocates nothing.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints; // AllocatingGCMemory() is an extension method here
using AllocIs = UnityEngine.TestTools.Constraints.Is;
using Is = NUnit.Framework.Is;

namespace Skygrind.Core.Tests
{
    public class GamepadTests
    {
        /// <summary>A fake standard pad: `down` = button names held, `axes` = [lx, ly, rx, ry], `values` = analog-only buttons.</summary>
        static PadState FakePad(string[] down = null, double[] axes = null, (string name, double v)[] values = null)
        {
            var p = new PadState();
            if (axes != null)
            {
                for (int i = 0; i < axes.Length; i++) p.axes[i] = axes[i];
            }
            if (down != null)
            {
                foreach (string name in down) p.buttons[GamepadMath.ButtonFromName(name)] = new PadButton { pressed = true, value = 1 };
            }
            if (values != null)
            {
                foreach (var (name, v) in values) p.buttons[GamepadMath.ButtonFromName(name)] = new PadButton { pressed = false, value = v };
            }
            return p;
        }

        static readonly StickSettings Sticks = new StickSettings { inner = 0.15, outer = 0.95, precise = false, sensX = 1, sensY = 1, invertY = false, yawRate = 3, pitchRate = 2 };

        /// <summary>A mask's action names, sorted as JS sorts strings (UTF-16 code units).</summary>
        static string[] Sorted(uint mask)
        {
            var names = new List<string>();
            for (int a = 0; a < GamepadMath.ActionCount; a++)
            {
                if ((mask & (1u << a)) != 0) names.Add(GamepadMath.ActionNames[a]);
            }
            names.Sort(string.CompareOrdinal);
            return names.ToArray();
        }

        static string[] InOrder(uint mask, PadState pad, PadLayout layout = null)
        {
            var order = new PadAction[GamepadMath.ActionCount];
            GamepadMath.HeldActions(pad, layout, order, out int n);
            return order.Take(n).Select(a => GamepadMath.ActionNames[(int)a]).ToArray();
        }

        static string[] ButtonNamesOf(int mask)
        {
            var names = new List<string>();
            for (int i = 0; i < GamepadMath.ButtonNames.Length; i++)
            {
                if ((mask & (1 << i)) != 0) names.Add(GamepadMath.ButtonNames[i]);
            }
            names.Sort(string.CompareOrdinal);
            return names.ToArray();
        }

        static uint Union(PadLayout layout)
        {
            uint all = 0;
            for (int i = 0; i < layout.Count; i++) all |= layout.MaskOf(layout[i]);
            return all;
        }

        // --- dead zone + curve -------------------------------------------------------------------------------------------

        [Test]
        public void ReadsZeroInsideTheInnerDeadZone_AndFullPastTheOuterOne()
        {
            var (zx, zy) = GamepadMath.RadialDeadzone(0.1, 0.05, 0.15, 0.95);
            Expect.Exactly(zx, 0);
            Expect.Exactly(zy, 0);
            var (x, y) = GamepadMath.RadialDeadzone(1, 0, 0.15, 0.95);
            Expect.Exactly(x, 1);
            Expect.Exactly(y, 0);
            var (dx, dy) = GamepadMath.RadialDeadzone(0.7, 0.7, 0.15, 0.95); // past outer along a diagonal: length 1
            Expect.CloseTo(Math.Sqrt(dx * dx + dy * dy), 1, 9);
        }

        [Test]
        public void RescalesBetweenTheTwo_SoTheFirstMoveStartsAtZero()
        {
            var (x, _) = GamepadMath.RadialDeadzone(0.16, 0, 0.15, 0.95);
            Assert.That(x, Is.GreaterThan(0));
            Assert.That(x, Is.LessThan(0.02));
            Expect.CloseTo(GamepadMath.RadialDeadzone(0.55, 0, 0.15, 0.95).x, 0.5, 9);
        }

        [Test]
        public void KeepsTheDirection()
        {
            var (x, y) = GamepadMath.RadialDeadzone(-0.3, 0.4, 0.1, 0.9);
            Expect.CloseTo(Math.Atan2(y, x), Math.Atan2(0.4, -0.3), 12);
        }

        [Test]
        public void ThePreciseCurveIsSquaredAndKeepsTheSign_LinearIsIdentity()
        {
            Expect.Exactly(GamepadMath.ResponseCurve(0.5, "linear"), 0.5);
            Expect.Exactly(GamepadMath.ResponseCurve(0.5, "precise"), 0.25);
            Expect.Exactly(GamepadMath.ResponseCurve(-0.5, "precise"), -0.25);
            Expect.Exactly(GamepadMath.ResponseCurve(1, "precise"), 1);
            Expect.Exactly(GamepadMath.ResponseCurve(-1, "precise"), -1);
        }

        // --- sticks ------------------------------------------------------------------------------------------------------

        [Test]
        public void LeftStickMoves_UpIsForward_RightStickTurns_RightIsYawDown_UpIsPitchUp()
        {
            Sticks r = GamepadMath.ReadSticks(FakePad(axes: new double[] { 1, -1, 1, -1 }), Sticks);
            Expect.CloseTo(r.moveX, Math.Sqrt(0.5), 9);
            Expect.CloseTo(r.moveZ, Math.Sqrt(0.5), 9);
            Assert.That(r.yawRate, Is.LessThan(0));
            Assert.That(r.pitchRate, Is.GreaterThan(0));
        }

        [Test]
        public void SeparateHorizontalAndVerticalSensitivity_InvertY()
        {
            StickSettings s = Sticks;
            s.sensX = 2;
            Expect.CloseTo(GamepadMath.ReadSticks(FakePad(axes: new double[] { 0, 0, 1, 0 }), s).yawRate, -6, 9);
            s = Sticks;
            s.sensY = 0.5;
            Expect.CloseTo(GamepadMath.ReadSticks(FakePad(axes: new double[] { 0, 0, 0, -1 }), s).pitchRate, 1, 9);
            s = Sticks;
            s.invertY = true;
            Expect.CloseTo(GamepadMath.ReadSticks(FakePad(axes: new double[] { 0, 0, 0, -1 }), s).pitchRate, -2, 9);
        }

        [Test]
        public void ACentredPadReadsExactlyZero_ARestingStickNeverDriftsTheView()
        {
            Sticks r = GamepadMath.ReadSticks(FakePad(axes: new double[] { 0.04, -0.06, 0.08, 0.02 }), Sticks);
            foreach (double v in new[] { r.moveX, r.moveZ, r.yawRate, r.pitchRate }) Assert.That(v == 0, $"{v.ToString("R", CultureInfo.InvariantCulture)} is +0 or -0");
        }

        // --- buttons -> actions ------------------------------------------------------------------------------------------

        [Test]
        public void TheDefaultLayoutCoversEveryInputFrameButtonAndPause()
        {
            uint mapped = Union(GamepadMath.DefaultLayout);
            for (int a = 0; a < 10; a++) Assert.That((mapped & (1u << a)) != 0, GamepadMath.ActionNames[a]);
            Assert.That((GamepadMath.FrameButtonsWeb & ~mapped) == 0, "every frame button"); // NIGHT19: the web's ten (Sprint is bound by the v2 layout, PadLayoutV2Tests)
            Assert.That((mapped & GamepadMath.Bit(PadAction.Pause)) != 0, "pause");
            for (int i = 0; i < GamepadMath.DefaultLayout.Count; i++)
            {
                PadButtonName b = GamepadMath.DefaultLayout[i];
                Assert.That(GamepadMath.ButtonFromName(GamepadMath.ButtonNames[(int)b]), Is.EqualTo((int)b), b.ToString());
            }
        }

        [Test]
        public void MapsHeldButtonsThroughTheLayout_AJumps_RTFires_XBuysAndReels()
        {
            uint held = GamepadMath.HeldActions(FakePad(new[] { "a", "x" }, values: new[] { ("rt", 0.9) }));
            Assert.That(Sorted(held), Is.EqualTo(new[] { "fire", "interact", "jump", "reel" }));
        }

        [Test]
        public void ATriggerOnlyCountsPastItsThreshold()
        {
            Assert.That(GamepadMath.ButtonDown(FakePad(values: new[] { ("lt", 0.2) }), (int)PadButtonName.LT), Is.False);
            Assert.That(GamepadMath.ButtonDown(FakePad(values: new[] { ("lt", 0.5) }), (int)PadButtonName.LT), Is.True);
        }

        [Test]
        public void PressEdgesFireOnce()
        {
            PadState pad = FakePad(new[] { "a" });
            var order = new PadAction[GamepadMath.ActionCount];
            var pressed = new PadAction[GamepadMath.ActionCount];
            uint a = GamepadMath.HeldActions(pad, null, order, out int n);
            int count = GamepadMath.PressedSince(order, n, 0, pressed);
            Assert.That(pressed.Take(count).Select(x => GamepadMath.ActionNames[(int)x]), Is.EqualTo(new[] { "jump" }));
            Assert.That(GamepadMath.PressedSince(order, n, a, pressed), Is.EqualTo(0));
            Assert.That(GamepadMath.PressedSince(a, a), Is.EqualTo(0u));
        }

        [Test]
        public void ACustomLayoutIsJustATable()
        {
            PadLayout layout = GamepadMath.DefaultLayout.With(PadButtonName.A, PadAction.Slide).With(PadButtonName.B, PadAction.Jump);
            PadState pad = FakePad(new[] { "a" });
            Assert.That(InOrder(GamepadMath.HeldActions(pad, layout), pad, layout), Is.EqualTo(new[] { "slide" }));
            Assert.That(GamepadMath.ButtonFor(PadAction.Jump, layout), Is.EqualTo(PadButtonName.B));
            Assert.That(GamepadMath.ButtonFor(PadAction.Jump), Is.EqualTo(PadButtonName.A));
        }

        // --- hot-swap + menus --------------------------------------------------------------------------------------------

        [Test]
        public void Activity_APressOrARealStickPushCounts_ARestingStickDoesNot()
        {
            Assert.That(GamepadMath.PadActivity(FakePad()), Is.False);
            Assert.That(GamepadMath.PadActivity(FakePad(axes: new[] { 0.1, 0.1, 0, 0 })), Is.False);
            Assert.That(GamepadMath.PadActivity(FakePad(axes: new[] { 0, 0, 0.8, 0 })), Is.True);
            Assert.That(GamepadMath.PadActivity(FakePad(new[] { "start" })), Is.True);
            Assert.That(GamepadMath.PadActivity(null), Is.False);
        }

        [Test]
        public void Menu_OneStepPerDPadPress_ThenARepeatWhileHeld()
        {
            var st = new MenuNavState();
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "down" }), st, 0.016).move, Is.EqualTo(1));
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "down" }), st, 0.1).move, Is.EqualTo(0)); // held, before the repeat delay
            int repeats = 0;
            for (int i = 0; i < 30; i++) repeats += Math.Abs(GamepadMath.MenuNav(FakePad(new[] { "down" }), st, 0.02).move);
            Assert.That(repeats, Is.GreaterThan(1));
            Assert.That(GamepadMath.MenuNav(FakePad(), st, 0.02).move, Is.EqualTo(0));
            Assert.That(GamepadMath.MenuNav(FakePad(axes: new[] { 0, -0.9, 0, 0 }), st, 0.02).move, Is.EqualTo(-1)); // the stick steps too
        }

        [Test]
        public void Menu_AConfirmsAndBBacksOut_OnThePressOnly()
        {
            var st = new MenuNavState();
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "a" }), st, 0.016).confirm, Is.True);
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "a" }), st, 0.016).confirm, Is.False);
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "b" }), st, 0.016).back, Is.True);
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "start" }), st, 0.016).start, Is.True);
        }

        /// <summary>NIGHT19 U3-33.3 (builder P): LB / RB's press edges are the previous / next tab (once a press; both at once: neither).</summary>
        [Test]
        public void Menu_TheBumpersStepTheTab_OnThePressOnly()
        {
            var st = new MenuNavState();
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "rb" }), st, 0.016).tab, Is.EqualTo(1), "RB: the next tab");
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "rb" }), st, 0.5).tab, Is.EqualTo(0), "held: no repeat");
            Assert.That(GamepadMath.MenuNav(FakePad(), st, 0.016).tab, Is.EqualTo(0));
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "lb" }), st, 0.016).tab, Is.EqualTo(-1), "LB: the previous tab");
            Assert.That(GamepadMath.MenuNav(FakePad(), st, 0.016).tab, Is.EqualTo(0));
            Assert.That(GamepadMath.MenuNav(FakePad(new[] { "lb", "rb" }), st, 0.016).tab, Is.EqualTo(0), "both at once: neither");
            PadMenu d = GamepadMath.MenuNav(FakePad(new[] { "down" }), new MenuNavState(), 0.016);
            Assert.That((d.move, d.tab), Is.EqualTo((1, 0)), "the d-pad still moves the focus (the Night13 smoke's tab step)");
        }

        // --- the keyboard path is untouched (OVERNIGHT3 P3) --------------------------------------------------------------

        /// <summary>The keyboard's frame (gamepad.test.js kbFrame): moveX -1, moveZ 1, yaw 0.123456789, pitch -0.3, weapon 2, jump held, fire pressed.</summary>
        static (InputFrame frame, CombatInput combat) KbFrame()
        {
            var f = new InputFrame { moveX = -1, moveZ = 1, yaw = 0.123456789, pitch = -0.3 };
            f.jump.held = true;
            var c = new CombatInput { weapon = 2 };
            c.fire.pressed = true;
            return (f, c);
        }

        static string B(Button b) => (b.held ? "1" : "0") + (b.pressed ? "1" : "0");

        static string D(double d) => BitConverter.DoubleToInt64Bits(d).ToString("x16");

        /// <summary>Every field of both structs, doubles by their bits (JSON.stringify of the web's one frame).</summary>
        static string Bits(InputFrame f, CombatInput c)
        {
            var sb = new StringBuilder();
            sb.Append(D(f.moveX)).Append(D(f.moveZ)).Append(D(f.yaw)).Append(D(f.pitch));
            sb.Append(B(f.jump)).Append(B(f.dash)).Append(B(f.slide)).Append(B(f.grapple)).Append(B(f.reel));
            sb.Append(f.grappleTarget.HasValue ? $"{D(f.grappleTarget.Value.point.x)}{D(f.grappleTarget.Value.point.y)}{D(f.grappleTarget.Value.point.z)}{f.grappleTarget.Value.node}" : "none");
            sb.Append(B(c.fire)).Append(B(c.altFire)).Append(B(c.special)).Append(B(c.punch)).Append(B(c.interact)).Append(c.weapon);
            return sb.ToString();
        }

        [Test]
        public void NoPad_TheFrameIsBitIdentical()
        {
            var (f, c) = KbFrame();
            string before = Bits(f, c);
            GamepadMath.MergePad(ref f, ref c, null);
            Assert.That(Bits(f, c), Is.EqualTo(before));
        }

        [Test]
        public void APadAtRest_BitIdenticalToo()
        {
            var (f, c) = KbFrame();
            string before = Bits(f, c);
            GamepadMath.MergePad(ref f, ref c, new PadFrame { moveX = 0, moveZ = 0, held = 0, latched = 0 });
            Assert.That(Bits(f, c), Is.EqualTo(before));
        }

        [Test]
        public void APadAddsOnTop_SticksClamp_ButtonsOr_PressesLatch()
        {
            var (f, c) = KbFrame();
            GamepadMath.MergePad(ref f, ref c, new PadFrame { moveX = 0.8, moveZ = 0.6, held = GamepadMath.Bit(PadAction.Dash), latched = GamepadMath.Bit(PadAction.Punch) });
            Expect.CloseTo(f.moveX, -0.2, 12);
            Expect.Exactly(f.moveZ, 1);
            Assert.That(f.dash.held && f.jump.held && c.punch.pressed && c.fire.pressed, Is.True);
            Expect.Exactly(f.yaw, 0.123456789); // look is the camera's, never the merge's
        }

        // --- pad remap (OVERNIGHT3 P3) -----------------------------------------------------------------------------------

        [Test]
        public void ARebindSwapsTheTwoButtons_NeverLeavingAnActionUnbound()
        {
            PadLayout def = GamepadMath.DefaultLayout;
            PadRebind r = GamepadMath.RebindPad(def, PadAction.Jump, "b");
            Assert.That(r.ok, Is.True);
            Assert.That(r.layout.ActionsOf(PadButtonName.B), Is.EqualTo(new[] { PadAction.Jump }));
            Assert.That(r.layout.ActionsOf(PadButtonName.A), Is.EqualTo(new[] { PadAction.Slide }));
            Assert.That(r.swapped, Is.EqualTo(PadButtonName.A));
            Assert.That(def.ActionsOf(PadButtonName.A), Is.EqualTo(new[] { PadAction.Jump })); // the input is untouched
            Assert.That(GamepadMath.RebindPad(def, PadAction.Jump, "nope").ok, Is.False);
            Assert.That(Sorted(Union(r.layout)), Is.EqualTo(Sorted(Union(def))));
            // a stored layout is checked: garbage or a layout that drops a needed action falls back to the default
            Assert.That(GamepadMath.ReadPadLayout(r.layout.ToJson()).SameAs(r.layout), Is.True);
            Assert.That(GamepadMath.ReadPadLayout("{bad"), Is.SameAs(def));
            Assert.That(GamepadMath.ReadPadLayout("{\"a\":[\"jump\"]}"), Is.SameAs(def));
            string withZz = def.ToJson();
            withZz = withZz.Substring(0, withZz.Length - 1) + ",\"zz\":[\"jump\"]}";
            Assert.That(GamepadMath.ReadPadLayout(withZz), Is.SameAs(def));
            // the table lists both sticks and every button
            Assert.That(GamepadMath.PadRows().Count, Is.EqualTo(2 + def.Count));
        }

        [Test]
        public void RawPressesReportButtonNamesOnThePressOnly()
        {
            PadState pad = FakePad(new[] { "a", "lb" });
            int pressed = GamepadMath.RawPresses(pad, 0, out int now);
            Assert.That(ButtonNamesOf(pressed), Is.EqualTo(new[] { "a", "lb" }));
            Assert.That(ButtonNamesOf(GamepadMath.RawPresses(pad, now, out _)), Is.Empty);
        }

        // --- PORT-RULES section 6 ----------------------------------------------------------------------------------------

        [Test]
        public void ThePerFramePadPath_AllocatesNothing()
        {
            PadState pad = FakePad(new[] { "a", "x", "down" }, new[] { 0.5, -0.7, 0.3, 0.9 }, new[] { ("rt", 0.9) });
            var nav = new MenuNavState();
            var order = new PadAction[GamepadMath.ActionCount];
            var pressed = new PadAction[GamepadMath.ActionCount];
            var padFrame = new PadFrame { moveX = 0.5, held = GamepadMath.Bit(PadAction.Jump), latched = GamepadMath.Bit(PadAction.Fire) };
            InputFrame f = default;
            CombatInput c = default;
            uint prev = 0;
            int raw = 0;
            double yaw = 0, pitch = 0;
            TestDelegate poll = () =>
            {
                Sticks s = GamepadMath.ReadSticks(pad, Sticks);
                GamepadMath.MenuNav(pad, nav, 0.016, 0.35, 0.12, 0.6);
                GamepadMath.RawPresses(pad, raw, out raw);
                uint held = GamepadMath.HeldActions(pad, GamepadMath.DefaultLayout, order, out int n);
                GamepadMath.PressedSince(order, n, prev, pressed);
                prev = held;
                GamepadMath.PadActivity(pad, 0.5);
                GamepadMath.ApplyLook(ref yaw, ref pitch, s, 1, 0.016, 1.5);
                GamepadMath.MergePad(ref f, ref c, padFrame);
            };
            poll(); // warm up (the JIT)
            Assert.That(poll, AllocIs.Not.AllocatingGCMemory());
        }
    }
}
