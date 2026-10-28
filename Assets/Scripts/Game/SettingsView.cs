// NIGHT13 U3-26.8 (agent S): the settings as typed fields, and their file. No UnityEngine use (the headless sim and the
// EditMode tests read both).
// - SettingsView: every value of the store as a plain field named after its web key (config.settings' 41, plus the
//   Unity-only VSYNC, FIRST-RUN GUIDE, WINDOW MODE, RESOLUTION) and a few derived flags, rebuilt from the store only when
//   the store's change count moved (SettingsStore.version: a Load, a Set, a ResetTab). Readers on the frame, step or hurt
//   path read GameSettings.View's fields: no string key, no dictionary, no allocation (PORT-RULES section 6). A load
//   straight into the store (the fingerprint's defaults) is followed too. The LAB toggles stay out: StartRun reads them
//   once per run through GameSettings.Bool.
// - SettingsFile: the settings JSON (the store's own, with its "_version") in a file beside the profile, written through
//   a temporary file then swapped in (File.Replace: a crash mid-write leaves the old file whole), read back as text.
using System;
using System.IO;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class SettingsView
    {
        // The Unity-only settings' keys (GameSettings adds their rows; FIRST-RUN GUIDE's is UnityTuning.OnboardingSetting).
        public const string VsyncKey = "vsync";
        public const string OnboardingKey = "onboarding";
        public const string WindowModeKey = "windowMode";
        public const string ResolutionKey = "resolution";

        /// <summary>The store's change count this view was built from (-1: never built).</summary>
        public int version = -1;

        /// <summary>How many times the view was rebuilt (a test reads it).</summary>
        public int rebuilds;

        // config.settings (the web's 41 keys, its names and types; a missing key reads 0 / false / "")
        public double fovDeg, effectsIntensity, sensitivity, uiScale, renderScale, duelRounds, snapStrength, auraIntensity, adsSensitivity, flashIntensity, fpsCap;
        public bool cameraRoll, invertY, reducedMotion, filmGrain, hurtVignette, showFps, damageNumbers, showPrices, xray, fullscreen, shapeCues, damageDirection, showGhost;
        public string hudStyle, crosshairColor, quality, aimAssist, toneMapping, adsMode, slideMode, arclineMode, gunFinish;
        public double padDeadzoneInner, padDeadzoneOuter, padSensX, padSensY;
        public bool padInvertY, padVibration;
        public string padCurve, padAimAssist;

        // Unity-only (GameSettings' extra rows; absent from a store built from the config alone)
        public bool vsync, onboarding;
        public string windowMode, resolution;
        // NIGHT16 U3-29.3: HIT FREEZE ("off" | "light" | "full"; "" without design.impact), SLOW-MO, the LAB's movement rows
        // (a row absent from the store reads off: the web's slide).
        public string hitFreeze;
        public bool slowMo, moveSlideCharge, moveSlideDecay, moveHopFalloff;

        // Derived once per rebuild: the hold / toggle modes (holdToggle.js reads 'toggle'), the zoom mode, the pad's curve.
        public bool slideToggle, arclineToggle, adsToggle, padCurvePrecise;

        /// <summary>The FPS CAP as frames per second (0: off).</summary>
        public int FpsCap => (int)fpsCap;

        /// <summary>Rebuild when the store changed since the last build; true when it did. One int compare otherwise.</summary>
        public bool Sync(SettingsStore store)
        {
            if (store.version == version) return false;
            Rebuild(store);
            return true;
        }

        /// <summary>Every field from the store (the only place a key string is read).</summary>
        public void Rebuild(SettingsStore store)
        {
            version = store.version;
            rebuilds++;
            var v = store.values;
            fovDeg = N(v, "fovDeg");
            cameraRoll = B(v, "cameraRoll");
            effectsIntensity = N(v, "effectsIntensity");
            hudStyle = S(v, "hudStyle");
            sensitivity = N(v, "sensitivity");
            invertY = B(v, "invertY");
            reducedMotion = B(v, "reducedMotion");
            filmGrain = B(v, "filmGrain");
            uiScale = N(v, "uiScale");
            hurtVignette = B(v, "hurtVignette");
            crosshairColor = S(v, "crosshairColor");
            quality = S(v, "quality");
            renderScale = N(v, "renderScale");
            showFps = B(v, "showFps");
            duelRounds = N(v, "duelRounds");
            damageNumbers = B(v, "damageNumbers");
            showPrices = B(v, "showPrices");
            aimAssist = S(v, "aimAssist");
            snapStrength = N(v, "snapStrength");
            xray = B(v, "xray");
            auraIntensity = N(v, "auraIntensity");
            toneMapping = S(v, "toneMapping");
            adsMode = S(v, "adsMode");
            adsSensitivity = N(v, "adsSensitivity");
            slideMode = S(v, "slideMode");
            arclineMode = S(v, "arclineMode");
            gunFinish = S(v, "gunFinish");
            flashIntensity = N(v, "flashIntensity");
            fpsCap = N(v, "fpsCap");
            fullscreen = B(v, "fullscreen");
            shapeCues = B(v, "shapeCues");
            damageDirection = B(v, "damageDirection");
            showGhost = B(v, "showGhost");
            padDeadzoneInner = N(v, "padDeadzoneInner");
            padDeadzoneOuter = N(v, "padDeadzoneOuter");
            padCurve = S(v, "padCurve");
            padSensX = N(v, "padSensX");
            padSensY = N(v, "padSensY");
            padInvertY = B(v, "padInvertY");
            padVibration = B(v, "padVibration");
            padAimAssist = S(v, "padAimAssist");
            vsync = B(v, VsyncKey);
            onboarding = B(v, OnboardingKey, true); // SurvivalGame.SettingOr("onboarding", true): on when the row is absent
            windowMode = S(v, WindowModeKey);
            resolution = S(v, ResolutionKey);
            hitFreeze = S(v, FeelKeys.HitFreezeSetting);
            slowMo = B(v, FeelKeys.SlowMoSetting);
            moveSlideCharge = B(v, FeelKeys.MoveSlideChargeSetting);
            moveSlideDecay = B(v, FeelKeys.MoveSlideDecaySetting);
            moveHopFalloff = B(v, FeelKeys.MoveHopFalloffSetting);
            slideToggle = slideMode == "toggle";
            arclineToggle = arclineMode == "toggle";
            adsToggle = adsMode == "toggle";
            padCurvePrecise = padCurve == "precise";
        }

        static double N(System.Collections.Generic.Dictionary<string, object> v, string key) => v.TryGetValue(key, out object o) && o is double d ? d : 0;

        static bool B(System.Collections.Generic.Dictionary<string, object> v, string key, bool missing = false) => v.TryGetValue(key, out object o) && o is bool b ? b : missing;

        static string S(System.Collections.Generic.Dictionary<string, object> v, string key) => v.TryGetValue(key, out object o) && o is string s ? s : "";
    }

    /// <summary>The settings file: read as text, written through a temporary file swapped in. No exception leaves it.</summary>
    public static class SettingsFile
    {
        /// <summary>The file's text, or null when it is missing or can't be read.</summary>
        public static string Read(string path)
        {
            try
            {
                return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Write `text` to `path`: the whole text to path + ".tmp" (flushed to the disk), then swapped in (File.Replace when the
        /// file exists, a move when it doesn't), so a crash mid-write leaves the old file whole. False on failure, with the
        /// temporary file removed.
        /// </summary>
        public static bool Write(string path, string text)
        {
            string tmp = path + ".tmp";
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                byte[] bytes = new UTF8Encoding(false).GetBytes(text);
                using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush(true);
                }
                if (File.Exists(path))
                {
                    try
                    {
                        File.Replace(tmp, path, null);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        File.Delete(path); // no atomic replace here: the old file goes, the new one moves in
                        File.Move(tmp, path);
                    }
                }
                else File.Move(tmp, path);
                return true;
            }
            catch (Exception)
            {
                try
                {
                    if (File.Exists(tmp)) File.Delete(tmp);
                }
                catch (Exception)
                {
                    // the temporary file stays; the next write truncates it
                }
                return false;
            }
        }
    }
}
