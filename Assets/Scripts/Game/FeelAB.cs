// NIGHT18 (NIGHT18.md rule 2): the same-build A / B switch. Every NIGHT18 change to an effect's behaviour or number reads
// FeelAB.On; with it off, the code takes NIGHT17's path and values through named numbers (UnityTuning.Pick(FeelAB.On,
// UnityTuning.X, UnityTuning.XN17)), or the two freeze tables (design.impact / design.impact.n17, picked in ImpactFrames).
// - On by default. `-skygrindFeelAB off` turns it off for a whole run (the reel's A side); the command line wins over the
//   LAB row for that run.
// - In a developer build (-skygrindDev, or the Editor's DevKeys default), F6 and the LAB row FEEL N18 flip it live, and a
//   small corner chip reads FEEL N17 / FEEL N18, so GG can feel the difference in one session (FEEL-PLAYTEST.md).
// - The switch is never saved: every boot starts on (or off with the flag), so a toggle in one session can't leak into
//   the next one's play.
// - Version moves on every change, so a view that caches a FeelAB-dependent value can rebuild it.
// Pure moves (the EnemyViews split) and the harness (FeelProbe, FeelClock, FeelStrip) are exempt.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static class FeelAB
    {
        /// <summary>NIGHT18's look (true) or NIGHT17's (false). Read it where the effect's number or path is chosen.</summary>
        public static bool On { get; private set; } = true;

        /// <summary>Moves on every change (a cache keyed on it rebuilds).</summary>
        public static int Version { get; private set; }

        /// <summary>The command line set the side (-skygrindFeelAB on / off): the LAB row and F6 don't move it.</summary>
        public static bool Pinned { get; private set; }

        /// <summary>Raised after every change (main thread; the views' caches, ImpactFrames' freeze table).</summary>
        public static event Action Changed;

        /// <summary>Is `-skygrindFeelAB off` (or `0` / `n17`) on the command line? `on` pins the switch on.</summary>
        public static bool? FromArgs(string[] args)
        {
            int i = Array.IndexOf(args, UnityTuning.FeelABFlag);
            if (i < 0 || i + 1 >= args.Length) return null;
            string v = args[i + 1].ToLowerInvariant();
            if (v == "off" || v == "0" || v == "n17") return false;
            if (v == "on" || v == "1" || v == "n18") return true;
            return null;
        }

        /// <summary>Set the side (a no-op when unchanged).</summary>
        public static void Set(bool on)
        {
            if (on == On) return;
            On = on;
            Version++;
            Changed?.Invoke();
        }

        /// <summary>Boot: the command line's side (pinned) or on; the LAB row shows it.</summary>
        public static void Boot(SurvivalGame g)
        {
            bool? arg = FromArgs(Environment.GetCommandLineArgs());
            Pinned = arg.HasValue;
            Set(arg ?? true);
            if (g.settings != null && g.settings.store.Item(UnityTuning.FeelABSetting) != null) g.settings.Set(UnityTuning.FeelABSetting, On);
            settingsSeen = g.settings != null ? g.settings.version : -1;
        }

        static int settingsSeen = -1;
        static GameObject chipRoot;
        static TextMeshProUGUI chipText;
        static bool chipShownOn;
        static int chipShownVersion = -1;

        /// <summary>
        /// Once a frame (SurvivalGame.HotKeys): F6 in a developer build flips the switch (and the LAB row); a LAB row change
        /// flips it; the corner chip shows the side in a developer build.
        /// </summary>
        public static void Poll(SurvivalGame g)
        {
            bool dev = g.devMode;
            if (g.settings != null && g.settings.version != settingsSeen)
            {
                settingsSeen = g.settings.version;
                if (!Pinned && g.settings.store.Item(UnityTuning.FeelABSetting) != null) Set(g.settings.Bool(UnityTuning.FeelABSetting));
            }
            Keyboard kb = Keyboard.current;
            if (dev && !Pinned && kb != null && kb[UnityTuning.FeelABKey].wasPressedThisFrame)
            {
                Set(!On);
                if (g.settings != null && g.settings.store.Item(UnityTuning.FeelABSetting) != null)
                {
                    g.settings.Set(UnityTuning.FeelABSetting, On);
                    settingsSeen = g.settings.version;
                }
                Debug.Log($"[feelab] F6: {(On ? UnityTuning.FeelABChipN18 : UnityTuning.FeelABChipN17)}");
            }
            Chip(dev);
        }

        static void Chip(bool show)
        {
            if (!show)
            {
                if (chipRoot != null && chipShownOn) chipRoot.SetActive(chipShownOn = false);
                return;
            }
            if (chipRoot == null)
            {
                chipRoot = new GameObject("FeelABChip");
                UnityEngine.Object.DontDestroyOnLoad(chipRoot);
                var canvas = chipRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = UnityTuning.FeelABChipSort;
                var scaler = chipRoot.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 1;
                var go = new GameObject("Text", typeof(RectTransform));
                go.transform.SetParent(chipRoot.transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
                rt.anchoredPosition = new Vector2(UnityTuning.FeelABChipInset, UnityTuning.FeelABChipInset);
                rt.sizeDelta = new Vector2(UnityTuning.FeelABChipWidth, UnityTuning.FeelABChipHeight);
                chipText = go.AddComponent<TextMeshProUGUI>();
                chipText.raycastTarget = false;
                chipText.alignment = TextAlignmentOptions.BottomLeft;
                if (UiTheme.Ready) UiTheme.Style(chipText, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold);
                else chipText.fontSize = UiTheme.Px(UiTheme.Size.S18);
            }
            if (!chipShownOn) chipRoot.SetActive(chipShownOn = true);
            if (chipShownVersion != Version)
            {
                chipShownVersion = Version;
                chipText.text = On ? UnityTuning.FeelABChipN18 : UnityTuning.FeelABChipN17;
            }
        }
    }
}
