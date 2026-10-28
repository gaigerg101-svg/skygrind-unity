// NIGHT13 U3-26.8 (agent S): the typed settings view (one rebuild per change of the store, every web key a typed field)
// and the settings file (written through a temporary file, read back; a failed write reports false, never throws).
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class SettingsViewTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport", "config.json")));

        [Test]
        public void TheView_RebuildsOncePerChange_AndFollowsADirectLoad()
        {
            var store = new SettingsStore(cfg);
            store.Load(null);
            var view = new SettingsView();
            Assert.That(view.Sync(store), Is.True, "the first read builds it");
            Assert.That(view.adsSensitivity, Is.EqualTo(store.Num("adsSensitivity")));
            Assert.That(view.Sync(store), Is.False, "no change: no rebuild");
            Assert.That(view.Sync(store), Is.False);
            Assert.That(view.rebuilds, Is.EqualTo(1));

            store.Set("adsSensitivity", 1.25);
            store.Set("damageDirection", false);
            store.Set("aimAssist", "lock");
            store.Set("slideMode", "toggle");
            store.Set("padCurve", "precise");
            store.Set("fpsCap", 144.0);
            Assert.That(view.Sync(store), Is.True);
            Assert.That(view.rebuilds, Is.EqualTo(2), "six changes, one rebuild");
            Assert.That(view.adsSensitivity, Is.EqualTo(1.25));
            Assert.That(view.damageDirection, Is.False);
            Assert.That(view.aimAssist, Is.EqualTo("lock"));
            Assert.That(view.slideToggle, Is.True);
            Assert.That(view.arclineToggle, Is.False);
            Assert.That(view.padCurvePrecise, Is.True);
            Assert.That(view.FpsCap, Is.EqualTo(144));

            // The fingerprint loads the defaults straight into the store (no GameSettings.Set): the view follows.
            store.Load("");
            Assert.That(view.Sync(store), Is.True);
            Assert.That(view.adsSensitivity, Is.EqualTo(store.defaults["adsSensitivity"]));
            Assert.That(view.aimAssist, Is.EqualTo(store.defaults["aimAssist"]));
        }

        [Test]
        public void EveryWebSetting_HasATypedFieldOfItsName_WithTheStoresValue()
        {
            var store = new SettingsStore(cfg);
            store.Load(null);
            var view = new SettingsView();
            view.Sync(store);
            foreach (var kv in store.defaults)
            {
                FieldInfo f = typeof(SettingsView).GetField(kv.Key, BindingFlags.Public | BindingFlags.Instance);
                Assert.That(f, Is.Not.Null, $"SettingsView has no field {kv.Key}");
                Assert.That(f.GetValue(view), Is.EqualTo(kv.Value), kv.Key);
            }
        }

        [Test]
        public void TheSettingsFile_IsReplacedThroughATempFile_AndReadsBack()
        {
            string dir = Path.Combine(Path.GetTempPath(), "skygrind-settings-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                string path = Path.Combine(dir, "skygrind-settings.json");
                Assert.That(SettingsFile.Read(path), Is.Null, "no file yet");
                Assert.That(SettingsFile.Write(path, "{\"fovDeg\":90}"), Is.True);
                Assert.That(SettingsFile.Read(path), Is.EqualTo("{\"fovDeg\":90}"));
                Assert.That(File.Exists(path + ".tmp"), Is.False, "the temporary file is gone");

                File.WriteAllText(path + ".tmp", "{half a write"); // a crash mid-write left this behind
                Assert.That(SettingsFile.Write(path, "{\"fovDeg\":75}"), Is.True, "a new write replaces the old file");
                Assert.That(SettingsFile.Read(path), Is.EqualTo("{\"fovDeg\":75}"));
                Assert.That(File.Exists(path + ".tmp"), Is.False);

                Assert.That(SettingsFile.Write(dir, "{}"), Is.False, "a path that can't be written: false, no throw");
                Assert.That(Directory.Exists(dir), Is.True);
                Assert.That(File.Exists(dir + ".tmp"), Is.False, "a failed write leaves no temporary file");
            }
            finally
            {
                if (File.Exists(dir + ".tmp")) File.Delete(dir + ".tmp");
                Directory.Delete(dir, true);
            }
        }
    }
}
