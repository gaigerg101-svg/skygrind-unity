// NIGHT8 U2-04: the settings store. Ported from the web's tests/settings.test.js (the migrations, rangeText) and
// tests/menuFocus.test.js (RESET THIS TAB), plus the save / load round trip the SETTINGS screen relies on.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class SettingsTests
    {
        static GameConfig cfg;
        static List<object> AimModes => new List<object> { "off", "assist", "snap", "lock" };

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void TheNewDefaultSensitivity_IsAbout36PercentLower()
        {
            Assert.That(cfg.Num("mouse.sensitivity"), Is.EqualTo(0.0014));
            Assert.That(cfg.Num("settings.sensitivity"), Is.EqualTo(0.0014));
            Expect.CloseTo(1 - 0.0014 / 0.0022, 0.36, 2);
        }

        [Test]
        public void ASavedOldDefault_MigratesOnce_AnyOtherValueStays()
        {
            double oldS = cfg.Num("mouse.oldDefault"), s = cfg.Num("mouse.sensitivity");
            var values = new Dictionary<string, object> { ["sensitivity"] = 0.0022, ["aimAssist"] = "soft" };
            var done = SettingsMath.MigrateSettings(new Dictionary<string, object> { ["sensitivity"] = 0.0022, ["aimAssist"] = "soft" }, values, oldS, s, AimModes);
            Assert.That(values["sensitivity"], Is.EqualTo(0.0014));
            Assert.That(values["aimAssist"], Is.EqualTo("snap"));
            Assert.That(done, Is.EqualTo(new List<string> { "sens14", "snap1" }));
            // Once migrated, a later 0.0022 (the player chose it) stays.
            var again = new Dictionary<string, object> { ["sensitivity"] = 0.0022, ["aimAssist"] = "lock" };
            SettingsMath.MigrateSettings(new Dictionary<string, object> { ["sensitivity"] = 0.0022, ["aimAssist"] = "lock", ["_migrated"] = new List<object> { "sens14", "snap1" } }, again, oldS, s, AimModes);
            Assert.That(again["sensitivity"], Is.EqualTo(0.0022));
            Assert.That(again["aimAssist"], Is.EqualTo("lock"));
            var own = new Dictionary<string, object> { ["sensitivity"] = 0.003, ["aimAssist"] = "off" };
            SettingsMath.MigrateSettings(new Dictionary<string, object> { ["sensitivity"] = 0.003, ["aimAssist"] = "off" }, own, oldS, s, AimModes);
            Assert.That(own["sensitivity"], Is.EqualTo(0.003));
            Assert.That(own["aimAssist"], Is.EqualTo("off"));
        }

        [Test]
        public void ANewProfile_StartsOnTheNewDefaults_WithTheMigrationsDone()
        {
            var values = new Dictionary<string, object> { ["sensitivity"] = 0.0014, ["aimAssist"] = "snap" };
            Assert.That(SettingsMath.MigrateSettings(null, values, cfg.Num("mouse.oldDefault"), cfg.Num("mouse.sensitivity"), AimModes), Is.EqualTo(new List<string> { "sens14", "snap1" }));
            Assert.That(values["sensitivity"], Is.EqualTo(0.0014));
        }

        [Test]
        public void TheSensitivityRow_ReadsInDegreesPer1000Counts()
        {
            var store = new SettingsStore(cfg);
            SettingItem item = store.Item("sensitivity");
            Assert.That(item.min, Is.EqualTo(0.0004));
            Assert.That(item.max, Is.EqualTo(0.006));
            Assert.That(item.fine, Is.EqualTo(0.0001));
            Assert.That(SettingsMath.RangeText(item, 0.0014), Is.EqualTo("0.0014 · 80.2°/1K"));
            Assert.That(SettingsMath.Nudge(item, 0.0014, 1), Is.EqualTo(0.0015), "0.0001 steps are exact");
            Assert.That(SettingsMath.Nudge(item, 0.006, 1), Is.EqualTo(0.006), "clamped at max");
        }

        [Test]
        public void ResetThisTab_ResetsExactlyTheOpenTab()
        {
            var store = new SettingsStore(cfg);
            var access = SettingsMath.TabDefaults(store.items, "access", store.defaults, "aim");
            var keys = access.ConvertAll(p => p.key);
            Assert.That(keys, Does.Contain("reducedMotion"));
            Assert.That(keys, Does.Not.Contain("sensitivity"));
            foreach (var (key, value) in access) Assert.That(value, Is.EqualTo(store.defaults[key]));
            int all = 0;
            foreach (var (id, _) in store.tabs) all += SettingsMath.TabDefaults(store.items, id, store.defaults, "aim").Count;
            Assert.That(all, Is.EqualTo(store.items.Count), "every setting is on exactly one tab");

            store.Set("reducedMotion", true);
            store.Set("sensitivity", 0.003);
            store.ResetTab("access");
            Assert.That(store.Bool("reducedMotion"), Is.EqualTo(store.defaults["reducedMotion"]));
            Assert.That(store.Num("sensitivity"), Is.EqualTo(0.003), "another tab keeps its value");
        }

        [Test]
        public void SaveThenLoad_RoundTripsEveryType_AndKeepsUnityExtras()
        {
            var vsync = new SettingItem { key = "vsync", tab = "video", label = "VSYNC", type = "toggle" };
            var a = new SettingsStore(cfg, new List<(SettingItem, object)> { (vsync, false) });
            a.Load(null);
            a.Set("sensitivity", 0.0021);
            a.Set("fovDeg", 95.0);
            a.Set("invertY", true);
            a.Set("crosshairColor", "magenta");
            a.Set("aimAssist", "lock");
            a.Set("snapStrength", 35.0);
            a.Set("fpsCap", 144.0);
            a.Set("vsync", true);
            a.Set("fovDeg", 400.0); // a range clamps to its max
            string json = a.Save();
            StringAssert.Contains("\"_version\":1", json);
            StringAssert.Contains("\"_migrated\":[\"sens14\",\"snap1\"]", json);

            var b = new SettingsStore(cfg, new List<(SettingItem, object)> { (vsync, false) });
            Assert.That(b.Load(json), Is.False, "nothing new to migrate");
            foreach (var kv in a.values) Assert.That(b.values[kv.Key], Is.EqualTo(kv.Value), kv.Key);
            Assert.That(b.Num("fovDeg"), Is.EqualTo(100));
            Assert.That(b.Bool("vsync"), Is.True);

            // A value of the wrong type or corrupt JSON keeps the defaults (createSettings' typeof check, its catch).
            var c = new SettingsStore(cfg);
            c.Load("{\"fovDeg\":\"wide\",\"invertY\":1}");
            Assert.That(c.values["fovDeg"], Is.EqualTo(c.defaults["fovDeg"]));
            Assert.That(c.values["invertY"], Is.EqualTo(c.defaults["invertY"]));
            c.Load("{not json");
            Assert.That(c.Num("sensitivity"), Is.EqualTo(0.0014));
        }

        // NIGHT13 U3-26.8: the store counts its changes, so the Game's typed view (SettingsView) rebuilds once per change and
        // never reads a key per frame; a load straight into the store (the fingerprint's defaults) counts too.
        [Test]
        public void EveryChange_BumpsTheStoreVersion_ReadsAndSavesDoNot()
        {
            var store = new SettingsStore(cfg);
            int v0 = store.version;
            store.Load(null);
            Assert.That(store.version, Is.EqualTo(v0 + 1), "a load");
            store.Set("fovDeg", 90.0);
            Assert.That(store.version, Is.EqualTo(v0 + 2), "a set");
            store.ResetTab("video");
            Assert.That(store.version, Is.EqualTo(v0 + 3), "a tab reset");
            store.Num("fovDeg");
            store.Bool("invertY");
            store.Str("aimAssist");
            store.Save();
            Assert.That(store.version, Is.EqualTo(v0 + 3), "reads and saves change nothing");
        }

        // NIGHT13 U3-26.8: the settings file beside the profile wins; PlayerPrefs' copy (the web's localStorage value, the
        // port's store until NIGHT13) is read only while there is no file: the migration happens once.
        [Test]
        public void TheSettingsFile_Wins_PlayerPrefsMigrateOnlyWithoutOne()
        {
            Assert.That(SettingsMath.PickSaved("{\"fovDeg\":90}", "{\"fovDeg\":70}", out string json), Is.EqualTo(SettingsSource.File));
            Assert.That(json, Is.EqualTo("{\"fovDeg\":90}"));
            Assert.That(SettingsMath.PickSaved(null, "{\"fovDeg\":70}", out json), Is.EqualTo(SettingsSource.Legacy));
            Assert.That(json, Is.EqualTo("{\"fovDeg\":70}"));
            Assert.That(SettingsMath.PickSaved(" \n", "{\"fovDeg\":70}", out json), Is.EqualTo(SettingsSource.Legacy), "an empty file is no file");
            Assert.That(SettingsMath.PickSaved("", "", out json), Is.EqualTo(SettingsSource.None));
            Assert.That(json, Is.Null);
            Assert.That(SettingsMath.PickSaved(null, null, out json), Is.EqualTo(SettingsSource.None));
            // A corrupt file is still the file: the store keeps the defaults (createSettings' catch); old prefs never return.
            Assert.That(SettingsMath.PickSaved("{not json", "{\"fovDeg\":70}", out json), Is.EqualTo(SettingsSource.File));
        }

        [Test]
        public void APlayerPrefsProfile_MovesToTheFile_WithEveryValue_AndTheFileRoundTrips()
        {
            var old = new SettingsStore(cfg);
            old.Load(null);
            old.Set("fovDeg", 95.0);
            old.Set("crosshairColor", "magenta");
            old.Set("invertY", true);
            old.Set("padSensX", 1.5);
            string legacy = old.Save();

            var boot = new SettingsStore(cfg);
            Assert.That(SettingsMath.PickSaved(null, legacy, out string json), Is.EqualTo(SettingsSource.Legacy));
            Assert.That(boot.Load(json), Is.False, "nothing new to migrate");
            string file = boot.Save(); // GameSettings writes this at once
            StringAssert.Contains("\"_version\":1", file);

            var next = new SettingsStore(cfg);
            Assert.That(SettingsMath.PickSaved(file, "{\"fovDeg\":61}", out json), Is.EqualTo(SettingsSource.File));
            next.Load(json);
            foreach (var kv in old.values) Assert.That(next.values[kv.Key], Is.EqualTo(kv.Value), kv.Key);
        }

        // NIGHT13 U3-26.8: SETTINGS shows WINDOW MODE (Unity-only) in place of the web's FULLSCREEN toggle; a profile saved with
        // FULLSCREEN on keeps a fullscreen window, once (an id in _migrated, which the web's loader keeps as it is).
        [Test]
        public void ASavedFullscreen_BecomesTheFullscreenWindowMode_Once()
        {
            var values = new Dictionary<string, object> { ["fullscreen"] = true, ["windowMode"] = "windowed" };
            var migrated = new List<string> { "sens14", "snap1" };
            Assert.That(SettingsMath.MigrateWindowMode(values, migrated, "windowMode", "borderless"), Is.True);
            Assert.That(values["windowMode"], Is.EqualTo("borderless"));
            Assert.That(migrated, Is.EqualTo(new List<string> { "sens14", "snap1", SettingsMath.WindowMigration }));
            values["windowMode"] = "windowed"; // the player picks a window later: it stays
            Assert.That(SettingsMath.MigrateWindowMode(values, migrated, "windowMode", "borderless"), Is.False);
            Assert.That(values["windowMode"], Is.EqualTo("windowed"));

            var off = new Dictionary<string, object> { ["fullscreen"] = false, ["windowMode"] = "windowed" };
            var fresh = new List<string>();
            Assert.That(SettingsMath.MigrateWindowMode(off, fresh, "windowMode", "borderless"), Is.True, "the id is remembered with nothing to move");
            Assert.That(off["windowMode"], Is.EqualTo("windowed"));
            Assert.That(fresh, Is.EqualTo(new List<string> { SettingsMath.WindowMigration }));
        }
    }
}
