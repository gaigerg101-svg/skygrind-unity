// NIGHT8 U2-04: the player's settings in the Player (settings.js createSettings / save): the store (Core SettingsStore:
// config.settings defaults, the web's keys, one versioned JSON), loaded at boot, saved on every change, and applied to
// the systems that read them: sensitivity / invertY / fovDeg / cameraRoll / effectsIntensity / reducedMotion into the
// typed movement settings (InputReader, CameraFeel), the frame cap / vsync / the window, and the aim assist's mode and
// strength. The HUD reads crosshairColor, showPrices, hurtVignette, damageDirection and flashIntensity live. Unity-only:
// VSYNC (video tab), FIRST-RUN GUIDE (game tab, NIGHT12: the first-run onboarding script runs only while it's on).
// Stage 1 kept only the aim-assist toggle in PlayerPrefs ("skygrind.aimAssist"); a profile without saved settings
// takes it over once (0 -> aimAssist off).
// NIGHT13 U3-26.8 (agent S):
// - The JSON lives in a file beside the profile (UnityTuning.SettingsFileName under Application.persistentDataPath),
//   written through a temporary file (SettingsFile); PlayerPrefs' copy (SettingsStore.StorageKey, the store until now)
//   is read only while there is no file, then written to the file at once (the old key is left as it was, as the
//   records' migration did).
// - View: the values as typed fields (SettingsView), rebuilt once per change of the store: what the frame, step and hurt
//   paths read (PORT-RULES section 6: no key string there).
// - Unity-only display rows: WINDOW MODE (replaces the web's FULLSCREEN row; fullscreen is kept in step with it, and a
//   saved FULLSCREEN on migrates once) and RESOLUTION (AUTO, or one of the display's sizes), applied in the Player only
//   and only when they change (a SetResolution on every settings change would flicker the window).
// - Descriptions the port changes: REDUCED MOTION (what it covers here), UI SCALE (the HUD only).
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class GameSettings
    {
        public readonly SettingsStore store;
        readonly SurvivalGame game;
        public int version; // + 1 on every change (the HUD re-reads what it caches)
        readonly SettingsView view = new SettingsView();
        readonly string path;
        bool writeFailed; // the warning once, not on every change
        string appliedWindow, appliedResolution; // the window last applied (null: not yet)

        /// <summary>The values as typed fields, rebuilt when the store changed (one int compare per read otherwise).</summary>
        public SettingsView View
        {
            get
            {
                view.Sync(store);
                return view;
            }
        }

        /// <summary>The Unity-only settings rows (key, tab, default), shown with the web's: VSYNC, FIRST-RUN GUIDE (NIGHT12
        /// U3-25.11: design.onboarding is skippable here; the GAME tab; on by default; StartRun reads it), WINDOW MODE and
        /// RESOLUTION (NIGHT13 U3-26.8, the VIDEO tab), and the LAB tab's toggles (design.lab.toggles, NIGHT8 U2-16; their
        /// defaults are off).</summary>
        /// <summary>config.ui.settings' GAMEPAD tab id (the pad rows' tab).</summary>
        const string PadTabId = "pad";

        static List<(SettingItem, object)> Extras(Lab lab, bool recapRow, double? rumbleDefault, GameConfig cfg)
        {
            var list = new List<(SettingItem, object)>
            {
                (new SettingItem { key = SettingsView.VsyncKey, tab = UnityTuning.VideoTabId, label = "VSYNC", type = "toggle", desc = "Present on the display's refresh (the FPS CAP is ignored while on)" }, UnityTuning.VsyncDefault),
                (new SettingItem { key = UnityTuning.OnboardingSetting, tab = UnityTuning.GameTabId, label = "FIRST-RUN GUIDE", type = "toggle", desc = "The first run teaches one system per round" }, UnityTuning.OnboardingDefault),
                // NIGHT13 U3-26.12: RUN LOG (RunLog: one JSONL file per run in the save folder, the last 20 kept; local only).
                (new SettingItem { key = UnityTuning.RunLogSetting, tab = UnityTuning.GameTabId, label = "RUN LOG", type = "toggle", desc = "Keep a local log of each run (the last 20) for bug reports" }, UnityTuning.RunLogDefault),
                (WindowModeItem(), UnityTuning.WindowModeDefault),
                (ResolutionItem(), UnityTuning.ResolutionDefault),
                // NIGHT17 U3-30.8 (NIGHT17.md rule 8): HUD SCALE (0.8-1.25; builder H applies it to the HUD), in place of UI SCALE's row.
                (new SettingItem { key = UnityTuning.HudScaleSetting, tab = UnityTuning.GameTabId, label = "HUD SCALE", type = "range", min = UnityTuning.ThemeHudScaleMin, max = UnityTuning.ThemeHudScaleMax, step = UnityTuning.HudScaleStep, digits = 2, hasDigits = true, desc = "THE HUD'S SIZE" }, UnityTuning.HudScaleDefault),
            };
            // NIGHT14 U3-27.9: RECAP (WHAT GOT YOU: the last seconds before DOWN replayed from an orbit camera), after RUN LOG;
            // only with design.recap.enabled (the review's A-5: with the flag off the GAME tab is u3-n13's).
            if (recapRow) list.Insert(3, (new SettingItem { key = UnityTuning.RecapSetting, tab = UnityTuning.GameTabId, label = "RECAP", type = "toggle", desc = "On DOWN, replay the last seconds from above with what got you marked" }, UnityTuning.RecapDefault));
            // NIGHT15 U3-28.8: RUMBLE (the GAMEPAD tab, after VIBRATION): the motors' strength, design.pad.rumbleDefault.
            if (rumbleDefault.HasValue) list.Add((new SettingItem { key = UnityTuning.PadRumbleSetting, tab = PadTabId, label = UnityTuning.PadRumbleLabel, type = "range", min = 0, max = 1, step = UnityTuning.PadRumbleStep, digits = 1, hasDigits = true, desc = UnityTuning.PadRumbleDesc }, rumbleDefault.Value));
            foreach (Lab.Toggle t in lab.toggles) list.Add((new SettingItem { key = t.key, tab = UnityTuning.LabTabId, label = t.name, type = "toggle", desc = t.desc }, t.on));
            FeelRows(list, cfg);
            // NIGHT18 (NIGHT18.md rule 6, GG's CAMERA FX switch): FOV KICK and CAMERA SHAKE (ACCESS, 0-100 %; builder M applies
            // them in CameraFeel, builder N shows the rows), and the LAB's FEEL N18 (FeelAB; never saved across boots).
            list.Add((new SettingItem { key = UnityTuning.FovKickSetting, tab = UnityTuning.AccessTabId, label = UnityTuning.FovKickLabel, type = "range", min = 0, max = 1, step = UnityTuning.CameraFxStep, digits = 1, hasDigits = true, desc = UnityTuning.FovKickDesc }, UnityTuning.FovKickDefault));
            list.Add((new SettingItem { key = UnityTuning.CameraShakeSetting, tab = UnityTuning.AccessTabId, label = UnityTuning.CameraShakeLabel, type = "range", min = 0, max = 1, step = UnityTuning.CameraFxStep, digits = 1, hasDigits = true, desc = UnityTuning.CameraShakeDesc }, UnityTuning.CameraShakeDefault));
            // NIGHT20 U3-34.7 (builder V's travel blur; the lead places the row): MOTION BLUR (ACCESS, on by default; REDUCED MOTION also turns it off)
            list.Add((new SettingItem { key = UnityTuning.MotionBlurSetting, tab = UnityTuning.AccessTabId, label = UnityTuning.MotionBlurLabel, type = "toggle", desc = UnityTuning.MotionBlurDesc }, UnityTuning.MotionBlurDefault));
            // NIGHT47 F (design.n47.hud47; XAG 101: text the player can scale to 200 %): TEXT SIZE (ACCESS); only with the row on
            if (cfg != null && NightRows.ViewOn(cfg, "hud47"))
                list.Add((new SettingItem { key = UnityTuning.TextSize47Setting, tab = UnityTuning.AccessTabId, label = UnityTuning.TextSize47Label, type = "range", min = UnityTuning.TextSize47Min, max = UnityTuning.TextSize47Max, step = UnityTuning.TextSize47Step, digits = 2, hasDigits = true, desc = UnityTuning.TextSize47Desc }, UnityTuning.TextSize47Default));
            // NIGHT29 item 4 (builder V): CRT (VIDEO, 0-100 %, default design.n29.crt.defaultStrength) and CRT OVERLAY (ACCESS, the off switch); only with the row on.
            if (N29Fixes.CrtOn(cfg))
            {
                list.Add((new SettingItem { key = UnityTuning.CrtSetting, tab = UnityTuning.VideoTabId, label = UnityTuning.CrtLabel, type = "range", min = 0, max = 1, step = UnityTuning.CrtStep, digits = 2, hasDigits = true, desc = UnityTuning.CrtDesc }, cfg.Num("design.n29.crt.defaultStrength")));
                list.Add((new SettingItem { key = UnityTuning.CrtOverlaySetting, tab = UnityTuning.AccessTabId, label = UnityTuning.CrtOverlayLabel, type = "toggle", desc = UnityTuning.CrtOverlayDesc }, UnityTuning.CrtOverlayDefault));
            }
            // NIGHT30 item 8 (builder V, design.n30.look30): PIXEL SIZE (VIDEO, 1-4 px, 1 = off: D-4) and HUD FLICKER (ACCESS, the HUD fx' accessibility switch); only with the row on.
            if (N30Fixes.ViewRowOn(cfg, "look30"))
            {
                list.Add((new SettingItem { key = UnityTuning.Pixel30Setting, tab = UnityTuning.VideoTabId, label = UnityTuning.Pixel30Label, type = "range", min = UnityTuning.Pixel30Min, max = UnityTuning.Pixel30Max, step = UnityTuning.Pixel30Step, digits = 0, hasDigits = true, desc = UnityTuning.Pixel30Desc }, UnityTuning.Pixel30Default));
                list.Add((new SettingItem { key = UnityTuning.HudFx30Setting, tab = UnityTuning.AccessTabId, label = UnityTuning.HudFx30Label, type = "toggle", desc = UnityTuning.HudFx30Desc }, UnityTuning.HudFx30Default));
            }
            list.Add((new SettingItem { key = UnityTuning.FeelABSetting, tab = UnityTuning.LabTabId, label = UnityTuning.FeelABLabel, type = "toggle", desc = UnityTuning.FeelABDesc }, true));
            // NIGHT20 row 6 (builder P): L3 SPRINT TOGGLE / HOLD (the GAMEPAD tab; GG's switch the default; ReaderInput reads it), with design.pad.n19.
            if (cfg.Has("design.pad.n19")) list.Add((ControlsPad20.SprintModeItem(), ControlsPad20.SprintModeDefault(cfg)));
            // NIGHT20 E2 (builder P): ASSIST (ACCESS; Hades' God Mode, design.adds.assist; Assist20Run applies it), only with the block.
            if (cfg.Has("design.adds.assist")) list.Add((new SettingItem { key = UnityTuning.AssistSetting, tab = UnityTuning.AccessTabId, label = UnityTuning.AssistLabel, type = "toggle", desc = UnityTuning.AssistDesc }, UnityTuning.AssistDefault));
            return list;
        }

        /// <summary>
        /// NIGHT16 U3-29.3: HIT FREEZE (off / light / full) and SLOW-MO (the ACCESS tab, after FLASH INTENSITY; defaults from
        /// design.impact.settings) and the LAB tab's three movement rows (design.move.*.enabled, on by default; a run reads
        /// them at its start and they never stamp it LAB). Each only with its design block.
        /// </summary>
        static void FeelRows(List<(SettingItem, object)> list, GameConfig cfg)
        {
            if (cfg.Has("design.impact.settings"))
            {
                var freeze = new SettingItem { key = FeelKeys.HitFreezeSetting, tab = UnityTuning.AccessTabId, label = UnityTuning.HitFreezeLabel, type = "select", desc = UnityTuning.HitFreezeDesc };
                foreach (var (mode, label) in UnityTuning.HitFreezeModes)
                {
                    freeze.options.Add(mode);
                    freeze.labels[mode] = label;
                }
                list.Add((freeze, cfg.Str("design.impact.settings.hitFreezeDefault")));
                list.Add((new SettingItem { key = FeelKeys.SlowMoSetting, tab = UnityTuning.AccessTabId, label = UnityTuning.SlowMoLabel, type = "toggle", desc = UnityTuning.SlowMoDesc }, cfg.Bool("design.impact.settings.slowMoDefault")));
            }
            if (cfg.Has("design.move"))
            {
                string recharge = cfg.Has("design.move.slideCharge.rechargeTime") ? cfg.Num("design.move.slideCharge.rechargeTime").ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) : "?";
                foreach (var (key, block, label, desc) in UnityTuning.MoveRows)
                    list.Add((new SettingItem { key = key, tab = UnityTuning.LabTabId, label = label, type = "toggle", desc = string.Format(System.Globalization.CultureInfo.InvariantCulture, desc, recharge) }, cfg.Bool($"design.move.{block}.enabled")));
            }
        }

        static SettingItem WindowModeItem()
        {
            var item = new SettingItem { key = SettingsView.WindowModeKey, tab = UnityTuning.VideoTabId, label = UnityTuning.WindowModeLabel, type = "select", desc = UnityTuning.WindowModeDesc };
            foreach (var (key, label) in UnityTuning.WindowModes)
            {
                item.options.Add(key);
                item.labels[key] = label;
            }
            return item;
        }

        /// <summary>RESOLUTION's options: AUTO, then each size the display offers (Screen.resolutions, one per size).</summary>
        static SettingItem ResolutionItem()
        {
            var item = new SettingItem { key = SettingsView.ResolutionKey, tab = UnityTuning.VideoTabId, label = UnityTuning.ResolutionLabel, type = "select", desc = UnityTuning.ResolutionDesc };
            item.options.Add(UnityTuning.ResolutionAuto);
            item.labels[UnityTuning.ResolutionAuto] = UnityTuning.ResolutionAutoLabel;
            Resolution[] modes = Screen.resolutions;
            for (int i = 0; i < modes.Length; i++)
            {
                string key = $"{modes[i].width}x{modes[i].height}";
                if (item.labels.ContainsKey(key)) continue;
                item.options.Add(key);
                item.labels[key] = $"{modes[i].width} x {modes[i].height}";
            }
            return item;
        }

        public GameSettings(SurvivalGame game)
        {
            this.game = game;
            double? rumbleDefault = game.cfg.Has("design.pad.rumbleDefault") ? game.cfg.Num("design.pad.rumbleDefault") : (double?)null;
            store = new SettingsStore(game.cfg, Extras(game.lab, game.cfg.Design("recap.enabled"), rumbleDefault, game.cfg), new List<(string, string)> { (UnityTuning.LabTabId, UnityTuning.LabTabLabel) });
            SettingItem reduced = store.Item("reducedMotion");
            if (reduced != null) reduced.desc = UnityTuning.ReducedMotionDesc;
            SettingItem uiScale = store.Item("uiScale");
            if (uiScale != null) uiScale.desc = UnityTuning.UiScaleDesc;
            // NIGHT15 U3-28.7: SNAP MODE's options named for a player (OFF / LIGHT / STRONG / LOCK; the saved values stay the
            // web's mode names), on the mouse's and the pad's rows.
            foreach (string aimKey in new[] { "aimAssist", "padAimAssist" })
            {
                SettingItem aim = store.Item(aimKey);
                if (aim == null) continue;
                foreach (var (mode, label) in UnityTuning.AimAssistLabels) aim.labels[mode] = label;
                aim.label = aimKey == "aimAssist" ? UnityTuning.AimAssistRowLabel : UnityTuning.PadAimAssistRowLabel; // capture 4: "SNAP MODE" read as jargon
            }
            SettingsMath.ApplyDesignDefaults(store, game.cfg); // NIGHT15: design.aimAssist / design.pad over the web's defaults
            path = Path.Combine(Application.persistentDataPath, UnityTuning.SettingsFileName);
            SettingsSource from = SettingsMath.PickSaved(SettingsFile.Read(path), PlayerPrefs.GetString(SettingsStore.StorageKey, ""), out string json);
            bool resave = store.Load(json);
            // NIGHT15: once per profile, a saved value still at the web's old default moves to the new one (a choice stays).
            if (SettingsMath.MigrateDesignDefaults(store, game.cfg, from != SettingsSource.None))
            {
                if (from != SettingsSource.None) Debug.Log($"[settings] the NIGHT15 defaults: aimAssist {store.Str("aimAssist")}, padAimAssist {store.Str("padAimAssist")}, padVibration {store.Bool("padVibration")}");
                resave = true;
            }
            if (from == SettingsSource.None)
            {
                if (PlayerPrefs.HasKey(UnityTuning.AimAssistPref) && PlayerPrefs.GetInt(UnityTuning.AimAssistPref) == 0) store.Set("aimAssist", "off");
                resave = true;
            }
            else if (from == SettingsSource.Legacy)
            {
                Debug.Log($"[settings] moved from PlayerPrefs ({SettingsStore.StorageKey}) to {path}");
                resave = true;
            }
            if (SettingsMath.MigrateWindowMode(store.values, store.migrated, SettingsView.WindowModeKey, UnityTuning.WindowFullscreen))
            {
                store.version++; // a value may have changed outside Set: the view rebuilds
                resave = true;
            }
            if (resave) Save();
        }

        public double Num(string key) => store.Num(key);

        /// <summary>NIGHT47 F: is `key` a setting this build has (TEXT SIZE exists only with design.n47.hud47 on)?</summary>
        public bool Has(string key) => store.values.ContainsKey(key);

        public bool Bool(string key) => store.Bool(key);

        public string Str(string key) => store.Str(key);

        /// <summary>Set, save and apply one value. WINDOW MODE keeps the web's fullscreen key in step.</summary>
        public void Set(string key, object value)
        {
            store.Set(key, value);
            if (key == SettingsView.WindowModeKey && value is string mode && store.defaults.ContainsKey("fullscreen")) store.Set("fullscreen", mode != UnityTuning.WindowWindowed);
            Save();
            Apply();
        }

        public void ResetTab(string tab)
        {
            store.ResetTab(tab);
            Save();
            Apply();
        }

        /// <summary>The store's JSON to the settings file (through a temporary file); a failure is logged once.</summary>
        public void Save()
        {
            if (SettingsFile.Write(path, store.Save())) writeFailed = false;
            else if (!writeFailed)
            {
                writeFailed = true;
                Debug.LogWarning($"[settings] could not write {path}: the settings last for this session");
            }
        }

        /// <summary>Push the values into the systems that read them.</summary>
        public void Apply()
        {
            version++;
            SettingsView v = View;
            SettingsConfig s = game.cfg.movement.settings;
            s.sensitivity = v.sensitivity;
            s.invertY = v.invertY;
            s.fovDeg = v.fovDeg;
            s.cameraRoll = v.cameraRoll;
            s.effectsIntensity = v.effectsIntensity;
            s.reducedMotion = v.reducedMotion;
            SurvivalGame.ApplyFrameCap(v.FpsCap, v.vsync);
            if (!Application.isEditor) ApplyWindow(v.windowMode, v.resolution);
        }

        /// <summary>
        /// WINDOW MODE and RESOLUTION, when either changed since the last apply (the boot's apply sets them once): a size
        /// sets the mode with it; AUTO sets the mode only. An unknown size (a saved one the display doesn't offer) is AUTO.
        /// </summary>
        void ApplyWindow(string window, string resolution)
        {
            if (window == appliedWindow && resolution == appliedResolution) return;
            appliedWindow = window;
            appliedResolution = resolution;
            FullScreenMode mode = window == UnityTuning.WindowExclusive ? FullScreenMode.ExclusiveFullScreen : window == UnityTuning.WindowFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            if (TrySize(resolution, out int w, out int h)) Screen.SetResolution(w, h, mode);
            else if (Screen.fullScreenMode != mode) Screen.fullScreenMode = mode;
        }

        static bool TrySize(string key, out int w, out int h)
        {
            w = h = 0;
            if (string.IsNullOrEmpty(key) || key == UnityTuning.ResolutionAuto) return false;
            int x = key.IndexOf('x');
            return x > 0 && int.TryParse(key.Substring(0, x), out w) && int.TryParse(key.Substring(x + 1), out h) && w > 0 && h > 0;
        }
    }
}
