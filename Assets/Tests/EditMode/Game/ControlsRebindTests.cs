// NIGHT13 U3-26.8 (agent S): CONTROLS' rebinding (menus.js :548-558 keydown capture, :515-522 input:padButton) through the
// live stores agent I ported (Core Bindings, GamepadMath's pad layout): a captured key binds and the binding survives a
// reload of the store from what it saved; a key in use swaps with a note; an unbindable key is refused with a note; a
// pad button takes an action (the two buttons swap) and the stored layout reads back.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class ControlsRebindTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport", "config.json")));

        static string Label(string action) => action == "jump" ? "JUMP" : action == "interact" ? "BUY (HOLD)" : action;

        [Test]
        public void ARebindOfJump_IsSaved_AndReadsBackAfterReloadingTheStore()
        {
            var store = new Bindings(cfg);
            store.Load(null);
            string saved = null;
            store.persist = json => saved = json; // what InputPrefs writes under skygrind.bindings.v1
            Assert.That(store.Code("jump"), Is.EqualTo("Space"));

            Assert.That(ControlsRebind.Key(store, "jump", "KeyJ", Label), Is.EqualTo(""), "a free key: no note");
            Assert.That(store.Code("jump"), Is.EqualTo("KeyJ"));
            Assert.That(saved, Is.Not.Null, "the store saved on the rebind");

            var reloaded = new Bindings(cfg);
            reloaded.Load(saved);
            Assert.That(reloaded.Code("jump"), Is.EqualTo("KeyJ"), "the rebind sticks across a reload");
            Assert.That(reloaded.Code("interact"), Is.EqualTo("KeyE"), "the rest keep their keys");

            // menus.js: a key in use swaps ("E WAS BUY (HOLD): SWAPPED"); Ctrl is refused ("L-CTRL" is KeyName's CONTROLLEFT).
            Assert.That(ControlsRebind.Key(store, "jump", "KeyE", Label), Is.EqualTo("E WAS BUY (HOLD): SWAPPED"));
            Assert.That(store.Code("jump"), Is.EqualTo("KeyE"));
            Assert.That(store.Code("interact"), Is.EqualTo("KeyJ"), "interact took jump's old key");
            Assert.That(ControlsRebind.Key(store, "jump", "ControlLeft", Label), Is.EqualTo("CONTROLLEFT CAN NOT BE BOUND"));
            Assert.That(store.Code("jump"), Is.EqualTo("KeyE"), "a refused key changes nothing");
        }

        [Test]
        public void APadButton_TakesTheAction_TheTwoButtonsSwap_AndTheStoredLayoutReadsBack()
        {
            PadLayout layout = GamepadMath.DefaultLayout;
            string Glyph(PadButtonName b) => GamepadMath.Glyphs[(int)b];
            // NEXT WEAPON (Y) to LB (DASH): LB takes Y's actions, Y takes LB's (main.js padBinds.set: "LB SWAPPED WITH Y").
            string note = ControlsRebind.Pad(ref layout, PadAction.NextWeapon, PadButtonName.LB, Glyph, out bool changed);
            Assert.That(changed, Is.True);
            Assert.That(note, Is.EqualTo("LB SWAPPED WITH Y"));
            Assert.That(GamepadMath.ButtonFor(PadAction.NextWeapon, layout), Is.EqualTo(PadButtonName.LB));
            Assert.That(GamepadMath.ButtonFor(PadAction.Dash, layout), Is.EqualTo(PadButtonName.Y));
            PadLayout stored = GamepadMath.ReadPadLayout(layout.ToJson()); // InputPrefs.SavePadLayout / LoadPadLayout's JSON
            Assert.That(stored.SameAs(layout), Is.True, "the remap reads back");
            Assert.That(GamepadMath.DefaultLayout.SameAs(layout), Is.False, "the default layout is untouched");

            // The button it already has: nothing changes, no note.
            Assert.That(ControlsRebind.Pad(ref layout, PadAction.NextWeapon, PadButtonName.LB, Glyph, out changed), Is.EqualTo(""));
            Assert.That(changed, Is.False);
        }

        // --- NIGHT19 U3-33.3 (builder P): the v2 layout's L3 through the Game's input layer ------------------------------

        /// <summary>
        /// A run (SurvivalSim.Headless on the Survival map) driven by the Game's ReaderInput with a pad filled by hand: the v2
        /// layout, L3 TOGGLE, design.stamina.n19's own key on (n19Override: stamina.n19 and pad.n19). The latch lives in
        /// ReaderInput.SampleMove and reads the run's Mover (ReaderInput.mover) for the empty bar.
        /// </summary>
        [Test]
        public void TheL3Toggle_ThroughTheReaderInput_SprintsTheRunsMover_TheStickCentredLetsGo()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(root, "maps", "survival.json"))));
            var pad = new PadReader(cfg) { Connected = true, layout = GamepadMath.DefaultLayoutV2, n19 = true, sprintToggle = true };
            var combat = new CombatReader();
            combat.Init(cfg, null);
            var input = new ReaderInput(new InputReader(cfg.movement), combat, pad, null);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map, input);
            sim.noLab = sim.noRecords = true;
            sim.setup.mode = RunSetup.Survival;
            sim.n19Override = N19Blocks.FromBits((1 << 4) | (1 << 6)); // stamina.n19 (the own key) and pad.n19
            sim.StartRun(4242, null, null);
            input.mover = sim.mover;
            Assert.That(sim.mover.stamina.rules.ownKey, Is.True, "design.stamina.n19.ownKey with the block on");
            sim.health.max = sim.health.hp = 1e6;

            void Frame(bool l3, bool forward)
            {
                pad.state.Clear();
                if (l3) pad.state.Set(PadButtonName.LS, true, 1);
                if (forward) pad.state.axes[1] = -1;
                pad.Step(sim.step, true);
                input.ApplyPad(sim.step, 1);
                sim.Step();
            }

            for (int i = 0; i < 288 && !sim.mover.grounded; i++) Frame(false, false);
            for (int i = 0; i < 60; i++) Frame(false, true);
            Assert.That(sim.mover.stamina.sprinting, Is.False, "the stick alone walks");
            Frame(true, true); // the click
            for (int i = 0; i < 30; i++) Frame(false, true);
            Assert.That(sim.mover.stamina.sprinting, Is.True, "L3 clicked: the latch sprints without the button");
            Frame(false, false); // the stick back at the centre
            for (int i = 0; i < 30; i++) Frame(false, true);
            Assert.That(sim.mover.stamina.sprinting, Is.False, "the centre let go: walking until the next click");
        }
    }
}
