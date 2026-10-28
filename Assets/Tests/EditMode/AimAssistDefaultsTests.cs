// NIGHT15 U3-28.7 (GG, 2026-10-05: "auto aim is too strong"): the Unity-side defaults (design.aimAssist: the mouse off,
// the pad on the web's mildest mode 'assist'; design.pad: VIBRATION on) over the web's ('snap' for both, VIBRATION off),
// and the one-time migration: a saved value still equal to the old default moves to the new one, once; a player's own
// choice ('lock', 'assist', VIBRATION off set again after the migration) stays. The web's store alone keeps the web's
// defaults (SettingsTests).
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class AimAssistDefaultsTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        /// <summary>The Game's boot order: the store, the design defaults, Load, the migration.</summary>
        static SettingsStore Boot(string json, out bool resave)
        {
            var store = new SettingsStore(cfg);
            SettingsMath.ApplyDesignDefaults(store, cfg);
            bool loaded = store.Load(json);
            resave = SettingsMath.MigrateDesignDefaults(store, cfg, !string.IsNullOrEmpty(json)) || loaded;
            return store;
        }

        [Test]
        public void TheDesignBlock_SaysMouseOffPadAssistVibrationOn()
        {
            Assert.That(cfg.Str("design.aimAssist.mouse"), Is.EqualTo("off"));
            Assert.That(cfg.Str("design.aimAssist.pad"), Is.EqualTo("assist"));
            Assert.That(cfg.Str("design.aimAssist.migrateFrom"), Is.EqualTo("snap"));
            Assert.That(cfg.Bool("design.pad.vibrationDefault"), Is.True);
            Assert.That(cfg.Str("settings.aimAssist"), Is.EqualTo("snap"), "the web's default stays in config.json");
        }

        [Test]
        public void AFreshProfile_HasTheMouseOff_ThePadOnAssist_VibrationOn()
        {
            SettingsStore s = Boot(null, out _);
            Assert.That(s.Str("aimAssist"), Is.EqualTo("off"));
            Assert.That(s.Str("padAimAssist"), Is.EqualTo("assist"));
            Assert.That(s.Bool("padVibration"), Is.True);
            Assert.That(s.migrated, Does.Contain(SettingsMath.DesignDefaultsMigration), "a new profile starts migrated");
            Assert.That(s.defaults["aimAssist"], Is.EqualTo("off"), "RESET THIS TAB goes to the new default");
        }

        [Test]
        public void ASavedSnap_MigratesOnce_ThenThePlayersSnapStays()
        {
            const string old = "{\"aimAssist\":\"snap\",\"padAimAssist\":\"snap\",\"padVibration\":false,\"_migrated\":[\"sens14\",\"snap1\"],\"_version\":1}";
            SettingsStore s = Boot(old, out bool resave);
            Assert.That(resave, Is.True, "the migration saves at once");
            Assert.That(s.Str("aimAssist"), Is.EqualTo("off"));
            Assert.That(s.Str("padAimAssist"), Is.EqualTo("assist"));
            Assert.That(s.Bool("padVibration"), Is.True);
            Assert.That(s.migrated, Does.Contain(SettingsMath.DesignDefaultsMigration));
            // The player picks STRONG (snap) and VIBRATION off again: the next boot keeps them.
            s.Set("aimAssist", "snap");
            s.Set("padAimAssist", "snap");
            s.Set("padVibration", false);
            SettingsStore again = Boot(s.Save(), out bool resave2);
            Assert.That(again.Str("aimAssist"), Is.EqualTo("snap"));
            Assert.That(again.Str("padAimAssist"), Is.EqualTo("snap"));
            Assert.That(again.Bool("padVibration"), Is.False);
            Assert.That(resave2, Is.False, "nothing to migrate the second time");
        }

        [Test]
        public void ASavedLockOrAssist_IsKept()
        {
            SettingsStore s = Boot("{\"aimAssist\":\"lock\",\"padAimAssist\":\"lock\",\"_migrated\":[\"sens14\",\"snap1\"],\"_version\":1}", out _);
            Assert.That(s.Str("aimAssist"), Is.EqualTo("lock"));
            Assert.That(s.Str("padAimAssist"), Is.EqualTo("lock"));
            SettingsStore t = Boot("{\"aimAssist\":\"assist\",\"padAimAssist\":\"off\",\"_migrated\":[\"sens14\",\"snap1\"],\"_version\":1}", out _);
            Assert.That(t.Str("aimAssist"), Is.EqualTo("assist"));
            Assert.That(t.Str("padAimAssist"), Is.EqualTo("off"));
        }

        [Test]
        public void TheWebsStoreAlone_KeepsTheWebsDefaults()
        {
            var s = new SettingsStore(cfg);
            s.Load(null);
            Assert.That(s.Str("aimAssist"), Is.EqualTo("snap"));
            Assert.That(s.Str("padAimAssist"), Is.EqualTo("snap"));
            Assert.That(s.Bool("padVibration"), Is.False);
        }
    }
}
