// NIGHT13 U3-26.7: the web's tests/bindings.test.js (OVERNIGHT2 N6: 6 cases) and tests/prompts.test.js :12 (fillKeys)
// ported against Core/Bindings.cs on config.input.bindings, plus the live store (createBindings: a saved map over the
// defaults, refused when it clashes, saved on every change), which the web leaves untested.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class BindingsTests
    {
        static GameConfig cfg;
        static BindingMap d;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            d = BindingMap.From(cfg.Obj("input.bindings"));
        }

        // --- bindings.test.js ------------------------------------------------------------------------------------------

        [Test]
        public void TheDefaultsHaveNoConflicts_AndNeverUseCtrl()
        {
            Assert.That(Bindings.Conflicts(d), Is.Empty);
            for (int i = 0; i < d.Count; i++) Assert.That(Bindings.Unbindable.Contains(d[d.ActionAt(i)]), Is.False, d.ActionAt(i));
        }

        [Test]
        public void AFreeKeyJustMovesTheAction()
        {
            RebindResult r = Bindings.Rebind(d, "jump", "KeyN"); // (B is SPECIAL since POLISH V2)
            Assert.That(r.ok, Is.True);
            Assert.That(r.conflict, Is.Null);
            Assert.That(r.map["jump"], Is.EqualTo("KeyN"));
            Assert.That(d["jump"], Is.EqualTo("Space")); // the input map is not changed
        }

        [Test]
        public void AKeyAnotherActionUsesSwapsTheTwo_AndNamesTheConflict()
        {
            RebindResult r = Bindings.Rebind(d, "jump", d["dash"]);
            Assert.That(r.conflict, Is.EqualTo("dash"));
            Assert.That(r.map["jump"], Is.EqualTo(d["dash"]));
            Assert.That(r.map["dash"], Is.EqualTo(d["jump"]));
            Assert.That(Bindings.Conflicts(r.map), Is.Empty);
        }

        [Test]
        public void CtrlMetaAndEscCanNeverBeBound()
        {
            foreach (string code in new[] { "ControlLeft", "ControlRight", "Escape", "MetaLeft" }) Assert.That(Bindings.Rebind(d, "jump", code).ok, Is.False, code);
        }

        [Test]
        public void RestartIsABinding_GivingRToAnotherActionMovesRestartInsteadOfDoublingIt()
        {
            Assert.That(d["restart"], Is.EqualTo("KeyR"));
            RebindResult r = Bindings.Rebind(d, "reel", "KeyR");
            Assert.That(r.conflict, Is.EqualTo("restart"));
            Assert.That(r.map["reel"], Is.EqualTo("KeyR"));
            Assert.That(r.map["restart"], Is.EqualTo(d["reel"]));
            Assert.That(Bindings.Conflicts(r.map), Is.Empty);
            // ... and the CONTROLS page lists it, so the swap is visible there.
            bool listed = false;
            foreach (object g in cfg.Arr("ui.controlGroups"))
            {
                if (PortData.Arr(PortData.Arr(g)[1]).Contains("restart")) listed = true;
            }
            Assert.That(listed, Is.True);
            foreach (object m in cfg.Arr("ui.mouseControls")) Assert.That((string)PortData.Arr(m)[1], Is.Not.EqualTo("R"));
        }

        [Test]
        public void KeyNamesReadLikeTheKeyboard()
        {
            Assert.That(Bindings.KeyName("KeyW"), Is.EqualTo("W"));
            Assert.That(Bindings.KeyName("ShiftLeft"), Is.EqualTo("L-SHIFT"));
            Assert.That(Bindings.KeyName("Space"), Is.EqualTo("SPACE"));
            Assert.That(Bindings.KeyName("Digit4"), Is.EqualTo("4"));
        }

        // --- prompts.test.js :12 ---------------------------------------------------------------------------------------

        [Test]
        public void FillKeysFillsKnownActions_AndLeavesAnythingElseAlone()
        {
            string KeyOf(string a) => d[a] != null ? Bindings.KeyName(d[a]).ToUpperInvariant() : null;
            Assert.That(Bindings.FillKeys("HOLD {slide} TO SLIDE · {jump}", KeyOf), Is.EqualTo("HOLD C TO SLIDE · SPACE"));
            Assert.That(Bindings.FillKeys("{nope} stays", KeyOf), Is.EqualTo("{nope} stays"));
        }

        // --- the live store (createBindings) ---------------------------------------------------------------------------

        [Test]
        public void TheStore_LoadsASavedMapOverTheDefaults_RefusesAClash_SavesEveryChange()
        {
            var saved = new List<string>();
            var store = new Bindings(cfg) { persist = saved.Add };
            store.Load(null);
            Assert.That(store.Map.SameAs(d), Is.True, "nothing saved: the defaults");
            int v = store.Version;
            RebindResult r = store.Set("jump", "KeyN");
            Assert.That(r.ok && store.Code("jump") == "KeyN" && store.ActionOf("KeyN") == "jump", Is.True);
            Assert.That(store.Version, Is.GreaterThan(v), "a change moves the version");
            Assert.That(saved.Count, Is.EqualTo(1), "saved on the change");
            Assert.That(store.Set("jump", "Escape").ok, Is.False);
            Assert.That(saved.Count, Is.EqualTo(1), "a refused rebind saves nothing");

            var again = new Bindings(cfg);
            again.Load(saved[0]);
            Assert.That(again.Map.SameAs(store.Map), Is.True, "the saved map reads back");
            again.Load("{\"jump\":\"KeyW\"}");
            Assert.That(again.Map.SameAs(d), Is.True, "a saved map whose keys clash: the defaults");
            again.Load("{\"jump\":\"ControlLeft\",\"zzz\":\"KeyP\",\"dash\":7}");
            Assert.That(again.Map.SameAs(d), Is.True, "unbindable codes, unknown actions and non-strings are skipped");
            again.Load("{oops");
            Assert.That(again.Map.SameAs(d), Is.True, "corrupt: the defaults");

            store.Reset();
            Assert.That(store.Map.SameAs(d) && saved.Count == 2, Is.True, "RESET: the defaults, saved");
        }
    }
}
