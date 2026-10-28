// NIGHT19 U3-33.3 (builder P; the shared commit placed the empty fixture): the v2 pad layout (design.pad.n19, GG's PAD
// LAYOUT switch "shooter") and its store:
// - the v2 default: L3 SPRINT, R3 PUNCH / PARRY, LB the boost (DASH, as NIGHT18), RB SPECIAL + ARCLINE (the grapple must stay
//   bound: it's in REQUIRED; the rope is off in Survival), d-pad up LOCK-ON; every other button as NIGHT18's (the web's);
// - the v1 migration: a saved skygrind.padLayout.v1 equal to the v1 default becomes the v2 default; a customised v1 keeps
//   its buttons and gains SPRINT on L3 / PUNCH on R3 only where those buttons still hold the v1 default's ARCLINE / LOCK-ON
//   (PUNCH and LOCK-ON trade places; the ARCLINE joins the button that carries SPECIAL, as the v2 default's RB), else SPRINT
//   stays unbound (CONTROLS: UNBOUND) and PUNCH stays where it is; the result is written as v2 ({"version":2,"layout":..}),
//   the v1 string kept for a rollback;
// - L3 SPRINT TOGGLE (CoD): a click latches the sprint key on; it lets go when the move stick is back at the centre, when
//   the bar is empty, or on another click (GamepadMath.SprintLatch; the HOLD choice is the button itself).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class PadLayoutV2Tests
    {
        static PadAction[] Acts(PadLayout l, PadButtonName b)
        {
            var list = l.ActionsOf(b);
            var a = new PadAction[list.Count];
            for (int i = 0; i < a.Length; i++) a[i] = list[i];
            return a;
        }

        [Test]
        public void TheV2Default_IsTheShooterLayout_EveryOtherButtonAsNight18()
        {
            PadLayout v1 = GamepadMath.DefaultLayout, v2 = GamepadMath.DefaultLayoutV2;
            Assert.That(Acts(v2, PadButtonName.LS), Is.EqualTo(new[] { PadAction.Sprint }), "L3 sprints");
            Assert.That(Acts(v2, PadButtonName.RS), Is.EqualTo(new[] { PadAction.Punch }), "R3 is the punch and parry");
            Assert.That(Acts(v2, PadButtonName.LB), Is.EqualTo(new[] { PadAction.Dash }), "LB boosts, as now");
            Assert.That(Acts(v2, PadButtonName.RB), Is.EqualTo(new[] { PadAction.Special, PadAction.Grapple }), "RB: the special, the grapple kept bound");
            Assert.That(Acts(v2, PadButtonName.Up), Is.EqualTo(new[] { PadAction.LockOn }), "d-pad up locks on");
            PadButtonName[] same =
            {
                PadButtonName.RT, PadButtonName.LT, PadButtonName.A, PadButtonName.B, PadButtonName.X, PadButtonName.Y,
                PadButtonName.Down, PadButtonName.Left, PadButtonName.Right, PadButtonName.Select, PadButtonName.Start,
            };
            foreach (PadButtonName b in same) Assert.That(Acts(v2, b), Is.EqualTo(Acts(v1, b)), $"{b} as NIGHT18");
            Assert.That(v2.Count, Is.EqualTo(v1.Count), "the same 16 buttons");
            for (int i = 0; i < v1.Count; i++) Assert.That(v2[i], Is.EqualTo(v1[i]), "the v1 key order (CONTROLS' and padRows' rows)");
        }

        [Test]
        public void TheV2Default_KeepsEveryRequiredActionBound_AndReadsBackThroughTheV2Store()
        {
            PadLayout v2 = GamepadMath.DefaultLayoutV2;
            Assert.That(GamepadMath.ReadPadLayout(v2.ToJson()).SameAs(v2), Is.True, "every REQUIRED action is bound (else the read gives the default)");
            string json = GamepadMath.PadLayoutV2Json(v2);
            StringAssert.StartsWith("{\"version\":2,\"layout\":{", json);
            Assert.That(GamepadMath.LoadPadLayoutV2(json, null, out bool migrated).SameAs(v2), Is.True);
            Assert.That(migrated, Is.False, "a v2 store is read as it is");
            Assert.That(GamepadMath.ButtonFor(PadAction.Grapple, v2), Is.EqualTo(PadButtonName.RB));
            Assert.That(GamepadMath.DefaultFor(true), Is.SameAs(v2));
            Assert.That(GamepadMath.DefaultFor(false), Is.SameAs(GamepadMath.DefaultLayout), "off: NIGHT18's layout");
        }

        [Test]
        public void NoStore_IsTheV2Default_NothingMigrated()
        {
            Assert.That(GamepadMath.LoadPadLayoutV2(null, null, out bool migrated).SameAs(GamepadMath.DefaultLayoutV2), Is.True);
            Assert.That(migrated, Is.False, "nothing to write");
        }

        [Test]
        public void AV1EqualToTheV1Default_BecomesTheV2Default()
        {
            PadLayout v1 = GamepadMath.DefaultLayout;
            Assert.That(GamepadMath.LoadPadLayoutV2(null, v1.ToJson(), out bool migrated).SameAs(GamepadMath.DefaultLayoutV2), Is.True);
            Assert.That(migrated, Is.True, "written as v2 (the caller keeps the v1 string)");
            // The same buttons in another key order is still the default (toEqual).
            var entries = v1.Entries();
            entries.Reverse();
            Assert.That(GamepadMath.MigrateV1(new PadLayout(entries)).SameAs(GamepadMath.DefaultLayoutV2), Is.True);
            // A corrupt v1 reads as the v1 default (ReadPadLayout's rule): the v2 default.
            Assert.That(GamepadMath.LoadPadLayoutV2(null, "{not json", out _).SameAs(GamepadMath.DefaultLayoutV2), Is.True);
        }

        [Test]
        public void ACustomisedV1_KeepsItsButtons_GainsSprintAndPunch_WhereL3AndR3StillHoldTheirDefaults()
        {
            // JUMP to B (A and B swap: A slides, B jumps); L3 and R3 untouched.
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayout, PadAction.Jump, PadButtonName.B).layout;
            Assert.That(custom.SameAs(GamepadMath.DefaultLayout), Is.False);
            PadLayout m = GamepadMath.LoadPadLayoutV2(null, custom.ToJson(), out bool migrated);
            Assert.That(migrated, Is.True);
            Assert.That(Acts(m, PadButtonName.A), Is.EqualTo(new[] { PadAction.Slide }), "the player's swap kept");
            Assert.That(Acts(m, PadButtonName.B), Is.EqualTo(new[] { PadAction.Jump }), "the player's swap kept");
            Assert.That(Acts(m, PadButtonName.LS), Is.EqualTo(new[] { PadAction.Sprint }), "L3 held the default ARCLINE: it sprints now");
            Assert.That(Acts(m, PadButtonName.RS), Is.EqualTo(new[] { PadAction.Punch }), "R3 held the default LOCK-ON: it punches now");
            Assert.That(Acts(m, PadButtonName.RB), Is.EqualTo(new[] { PadAction.LockOn }), "PUNCH and LOCK-ON traded places");
            Assert.That(Acts(m, PadButtonName.Up), Is.EqualTo(new[] { PadAction.Special, PadAction.Grapple }), "the ARCLINE joins SPECIAL's button (bound: REQUIRED)");
            Assert.That(GamepadMath.ReadPadLayout(m.ToJson()).SameAs(m), Is.True, "every REQUIRED action still bound");
            for (int i = 0; i < custom.Count; i++) Assert.That(m[i], Is.EqualTo(custom[i]), "the player's key order");
        }

        [Test]
        public void ACustomisedV1_WhoseL3AndR3WereRemapped_LeavesSprintUnbound_AndPunchWhereItIs()
        {
            PadLayout custom = GamepadMath.DefaultLayout;
            custom = GamepadMath.RebindPad(custom, PadAction.Jump, PadButtonName.LS).layout; // L3 jumps, A carries the ARCLINE
            custom = GamepadMath.RebindPad(custom, PadAction.LockOn, PadButtonName.Y).layout; // Y locks on, R3 is NEXT WEAPON
            PadLayout m = GamepadMath.MigrateV1(custom);
            Assert.That(m.SameAs(custom), Is.True, "nothing moves: L3 and R3 hold the player's own picks");
            Assert.That(GamepadMath.ButtonFor(PadAction.Sprint, m), Is.Null, "SPRINT unbound (CONTROLS: UNBOUND)");
            Assert.That(GamepadMath.ButtonFor(PadAction.Punch, m), Is.EqualTo(PadButtonName.RB), "PUNCH where the player left it");
        }

        [Test]
        public void ACustomisedV1_WithOnlyR3StillLockOn_GainsPunchThere_SprintStaysUnbound()
        {
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayout, PadAction.Grapple, PadButtonName.Down).layout; // L3 is X-RAY now
            PadLayout m = GamepadMath.MigrateV1(custom);
            Assert.That(Acts(m, PadButtonName.LS), Is.EqualTo(new[] { PadAction.Xray }), "L3 kept the player's pick");
            Assert.That(GamepadMath.ButtonFor(PadAction.Sprint, m), Is.Null, "SPRINT unbound");
            Assert.That(Acts(m, PadButtonName.RS), Is.EqualTo(new[] { PadAction.Punch }));
            Assert.That(Acts(m, PadButtonName.RB), Is.EqualTo(new[] { PadAction.LockOn }));
            Assert.That(Acts(m, PadButtonName.Down), Is.EqualTo(new[] { PadAction.Grapple }), "the ARCLINE stays where the player put it");
        }

        [Test]
        public void TheV2Store_NeedsItsVersion_ACorruptOrOtherVersionReadsAsMissing()
        {
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV2, PadAction.Jump, PadButtonName.B).layout;
            string json = GamepadMath.PadLayoutV2Json(custom);
            Assert.That(GamepadMath.LoadPadLayoutV2(json, GamepadMath.DefaultLayout.ToJson(), out bool m0).SameAs(custom), Is.True, "v2 wins over v1");
            Assert.That(m0, Is.False);
            string v3 = json.Replace("\"version\":2", "\"version\":3");
            Assert.That(GamepadMath.LoadPadLayoutV2(v3, null, out bool m1).SameAs(GamepadMath.DefaultLayoutV2), Is.True, "another version: as missing");
            Assert.That(m1, Is.False);
            Assert.That(GamepadMath.LoadPadLayoutV2("[1,2]", null, out _).SameAs(GamepadMath.DefaultLayoutV2), Is.True, "not an object");
            Assert.That(GamepadMath.LoadPadLayoutV2(custom.ToJson(), null, out _).SameAs(GamepadMath.DefaultLayoutV2), Is.True, "a bare layout has no version");
            // A v2 store that leaves a REQUIRED action unbound reads as the v2 default (ReadPadLayout's rule).
            string noPause = "{\"version\":2,\"layout\":{\"rt\":[\"fire\"]}}";
            Assert.That(GamepadMath.LoadPadLayoutV2(noPause, null, out _).SameAs(GamepadMath.DefaultLayoutV2), Is.True);
        }

        [Test]
        public void TheSprintLatch_AClickWhileMoving_HoldsUntilTheStickCentres()
        {
            bool on = GamepadMath.SprintLatch(false, true, true, false);
            Assert.That(on, Is.True, "a click while moving latches");
            for (int i = 0; i < 5; i++)
            {
                on = GamepadMath.SprintLatch(on, false, true, false);
                Assert.That(on, Is.True, "held without the button");
            }
            on = GamepadMath.SprintLatch(on, false, false, false);
            Assert.That(on, Is.False, "the stick back at the centre lets go");
            on = GamepadMath.SprintLatch(on, false, true, false);
            Assert.That(on, Is.False, "and moving again doesn't restart it (a click does)");
        }

        [Test]
        public void TheSprintLatch_AnotherClickLetsGo_AClickAtRestDoesNothing()
        {
            bool on = GamepadMath.SprintLatch(false, true, true, false);
            on = GamepadMath.SprintLatch(on, true, true, false);
            Assert.That(on, Is.False, "another click lets go");
            on = GamepadMath.SprintLatch(on, true, true, false);
            Assert.That(on, Is.True, "and a third latches again");
            Assert.That(GamepadMath.SprintLatch(false, true, false, false), Is.False, "a click with the stick at rest doesn't latch (CoD)");
        }

        [Test]
        public void TheSprintLatch_AnEmptyBarLetsGo_TheRefillDoesntRestartIt()
        {
            bool on = GamepadMath.SprintLatch(false, true, true, false);
            on = GamepadMath.SprintLatch(on, false, true, true);
            Assert.That(on, Is.False, "the bar ran out");
            on = GamepadMath.SprintLatch(on, false, true, false);
            Assert.That(on, Is.False, "the bar refilled: still off until a click");
            Assert.That(GamepadMath.SprintLatch(false, true, true, true), Is.False, "a click on an empty bar doesn't latch");
        }

        // --- the latch on a Mover with the stamina rules on (design.stamina + design.stamina.n19's own key) -------------

        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));

        /// <summary>Port.Root in the Editor; the working folder's PortExport under a plain .NET runner (no Application.dataPath).</summary>
        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static readonly StickSettings Sticks = new StickSettings { inner = 0.15, outer = 0.95, sensX = 1, sensY = 1, yawRate = 3, pitchRate = 2 };

        /// <summary>
        /// A pad on the v2 layout feeding a Mover the way ReaderInput does each step: the held actions and the press edges
        /// (PadFrame), MergeMove, the move latches consumed, then L3's TOGGLE (ApplySprintToggle, reading the bar), then the
        /// Mover's step. The Mover stands on a plane with every stamina rule on and the sprint's own key (ownKey).
        /// </summary>
        sealed class PadRig
        {
            public readonly Mover m;
            readonly PadState pad = new PadState();
            readonly PadFrame f = new PadFrame();
            readonly double dt;
            uint prev;
            public bool latch;
            public InputFrame last;

            public PadRig(bool toggle)
            {
                this.toggle = toggle;
                MovementConfig mc = Cfg.movement;
                dt = 1 / mc.loop.simHz;
                var w = new TriangleWorld();
                w.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
                m = new Mover(mc, w);
                Stamina.Params rules = Stamina.Params.From(Cfg).With(true, true, true, true, true);
                rules.ownKey = true;
                m.SetStamina(rules);
                m.Reset(0, 0, 1000);
                for (int i = 0; i < 30; i++) m.Step(new InputFrame(), dt);
            }

            readonly bool toggle;

            public void Step(bool l3, bool forward)
            {
                pad.Clear();
                if (l3) pad.Set(PadButtonName.LS, true, 1);
                if (forward) pad.axes[1] = -1; // stick up (the web's -1)
                uint held = GamepadMath.HeldActions(pad, GamepadMath.DefaultLayoutV2);
                f.held = held;
                f.latched |= GamepadMath.PressedSince(held, prev) & GamepadMath.FrameButtons;
                prev = held;
                Sticks s = GamepadMath.ReadSticks(pad, PadLayoutV2Tests.Sticks);
                f.moveX = s.moveX;
                f.moveZ = s.moveZ;
                var frame = new InputFrame();
                GamepadMath.MergeMove(ref frame, f);
                f.latched &= ~GamepadMath.MoveButtons;
                if (toggle) GamepadMath.ApplySprintToggle(ref frame, ref latch, GamepadMath.SprintBarEmpty(m.stamina));
                last = frame;
                m.Step(frame, dt);
            }

            public void Run(int steps, bool forward)
            {
                for (int i = 0; i < steps; i++) Step(false, forward);
            }
        }

        [Test]
        public void TheL3Toggle_OnTheV2Layout_SprintsTheMover_UntilTheStickCentres_OrAnotherClick()
        {
            var r = new PadRig(true);
            r.Run(120, true);
            Assert.That(r.m.stamina.sprinting, Is.False, "the stick alone walks");
            double full = r.m.stamina.bar;
            r.Step(true, true); // the click (L3 down one step, then up)
            r.Run(30, true);
            Assert.That(r.last.sprint.held, Is.True, "the latch holds the sprint key without the button");
            Assert.That(r.m.stamina.sprinting, Is.True, "the own key sprints (no boost first)");
            Assert.That(r.m.stamina.bar, Is.LessThan(full), "and drains");
            Assert.That(r.m.state, Is.Not.EqualTo(MoveState.Dash), "L3 never boosts");
            r.Run(1, false); // the stick back at the centre
            Assert.That(r.latch, Is.False);
            r.Run(30, true);
            Assert.That(r.m.stamina.sprinting, Is.False, "pushing again walks until the next click");
            r.Step(true, true);
            r.Run(10, true);
            Assert.That(r.m.stamina.sprinting, Is.True);
            r.Step(true, true); // another click
            r.Run(10, true);
            Assert.That(r.latch || r.m.stamina.sprinting, Is.False, "another click lets go");
        }

        [Test]
        public void TheL3Toggle_LetsGoWhenTheBarEmpties_TheRefillDoesntRestartTheSprint()
        {
            var r = new PadRig(true);
            r.Run(60, true);
            r.Step(true, true);
            int n = 0;
            while (r.m.stamina.bar > 0 && n++ < 5000) r.Step(false, true);
            Assert.That(r.m.stamina.bar, Is.EqualTo(0), "sprinted the bar empty");
            r.Run(1, true);
            Assert.That(r.latch, Is.False, "the empty bar let go of the latch");
            n = 0;
            while (r.m.stamina.bar <= 0 && n++ < 5000) r.Step(false, true);
            Assert.That(r.m.stamina.bar, Is.GreaterThan(0), "the bar refills (the stick still forward)");
            r.Run(60, true);
            Assert.That(r.m.stamina.sprinting, Is.False, "no sprint until the next click");
        }

        [Test]
        public void TheL3Hold_SprintsWhileHeld_AndStopsOnTheRelease()
        {
            var r = new PadRig(false);
            r.Run(60, true);
            for (int i = 0; i < 30; i++) r.Step(true, true);
            Assert.That(r.m.stamina.sprinting, Is.True, "HOLD: the held button is the key");
            r.Run(2, true);
            Assert.That(r.m.stamina.sprinting, Is.False, "the release ends it");
        }
    }
}
