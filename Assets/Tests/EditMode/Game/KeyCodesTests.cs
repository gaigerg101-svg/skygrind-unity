// NIGHT13 U3-26.7: the web key code <-> Input System Key table (Game/Input/KeyCodes.cs; audit-input risk 2: the old
// SurvivalGame.KeyOf read "ShiftLeft", the dash default, as Key.None). Every config.input.bindings default maps to a key
// that isn't None and back to the same code; the names the two sides spell differently; the prompt labels (bindings.js
// keyName, upper-cased). This assembly has no Input System reference, so the keys are compared by index and enum name.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class KeyCodesTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport", "config.json")));

        [Test]
        public void EveryDefaultBinding_MapsToAKeyThatIsntNone_AndBack()
        {
            var bindings = cfg.Obj("input.bindings");
            Assert.That(bindings.Count, Is.EqualTo(18));
            foreach (var kv in bindings)
            {
                string code = (string)kv.Value;
                int key = KeyCodes.KeyIndex(code);
                Assert.That(key, Is.Not.EqualTo(0), $"{kv.Key}: {code} maps to Key.None");
                Assert.That(KeyCodes.CodeOfIndex(key), Is.EqualTo(code), $"{kv.Key}: {code} round trip");
            }
        }

        [Test]
        public void TheNamesTheTwoSidesSpellDifferently()
        {
            Assert.That(KeyCodes.KeyEnumName("ShiftLeft"), Is.EqualTo("LeftShift"));
            Assert.That(KeyCodes.KeyEnumName("ShiftRight"), Is.EqualTo("RightShift"));
            Assert.That(KeyCodes.KeyEnumName("ArrowUp"), Is.EqualTo("UpArrow"));
            Assert.That(KeyCodes.KeyEnumName("Equal"), Is.EqualTo("Equals"));
            Assert.That(KeyCodes.KeyEnumName("BracketLeft"), Is.EqualTo("LeftBracket"));
            Assert.That(KeyCodes.KeyEnumName("ControlLeft"), Is.EqualTo("LeftCtrl"));
            Assert.That(new[] { "LeftMeta", "LeftWindows", "LeftApple", "LeftCommand" }, Does.Contain(KeyCodes.KeyEnumName("MetaLeft"))); // one value, four names
            Assert.That(KeyCodes.KeyEnumName("NumpadAdd"), Is.EqualTo("NumpadPlus"));
            Assert.That(KeyCodes.KeyEnumName("Digit1"), Is.EqualTo("Digit1"));
            Assert.That(KeyCodes.KeyEnumName("KeyE"), Is.EqualTo("E"));
            Assert.That(KeyCodes.KeyEnumName("Space"), Is.EqualTo("Space"));
            Assert.That(KeyCodes.KeyEnumName("Tab"), Is.EqualTo("Tab"));
            Assert.That(KeyCodes.KeyEnumName("Backquote"), Is.EqualTo("Backquote"));
            Assert.That(KeyCodes.KeyEnumName("Escape"), Is.EqualTo("Escape"));
            Assert.That(KeyCodes.KeyEnumName("IntlRo"), Is.EqualTo("None"));
            Assert.That(KeyCodes.KeyIndex(null), Is.EqualTo(0));
        }

        [Test]
        public void EveryCodeInTheTable_RoundTrips_AndTheUnbindableKeysAreKnown()
        {
            Assert.That(KeyCodes.Codes.Count, Is.GreaterThan(100));
            foreach (string code in KeyCodes.Codes)
            {
                int key = KeyCodes.KeyIndex(code);
                Assert.That(key, Is.Not.EqualTo(0), code);
                Assert.That(KeyCodes.CodeOfIndex(key), Is.EqualTo(code), code);
            }
            foreach (string code in Bindings.Unbindable) Assert.That(KeyCodes.KeyIndex(code), Is.Not.EqualTo(0), code);
            Assert.That(KeyCodes.CodeOfIndex(0), Is.Null);
        }

        [Test]
        public void Labels_AreTheWebsKeyNamesUpperCased()
        {
            Assert.That(KeyCodes.Label("KeyE"), Is.EqualTo("E"));
            Assert.That(KeyCodes.Label("ShiftLeft"), Is.EqualTo("L-SHIFT"));
            Assert.That(KeyCodes.Label("Space"), Is.EqualTo("SPACE"));
            Assert.That(KeyCodes.Label("ArrowUp"), Is.EqualTo("UP"));
            Assert.That(KeyCodes.Label("Digit0"), Is.EqualTo("0"));
            Assert.That(KeyCodes.Label("Backquote"), Is.EqualTo("`"));
            Assert.That(KeyCodes.Label("Equal"), Is.EqualTo("EQUAL"));
            Assert.That(KeyCodes.Label(null), Is.EqualTo("-"));
            Assert.That(KeyCodes.Label("KeyE"), Is.SameAs(KeyCodes.Label("KeyE")), "cached: one string per code");
        }
    }
}
