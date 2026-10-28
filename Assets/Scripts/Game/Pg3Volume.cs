// PG3 A1 (GG 2026-10-28, the class assignment's sound): the player's three volumes, MASTER / MUSIC / SFX, each a whole step
// 0..UnityTuning.Pg3VolumeMaxStep (the level is step / max). One small static class that Pg3Audio (every one-shot, loop and 3D voice:
// SFX) and MusicDirector (the tracks and the stings: MUSIC) multiply in; MASTER is AudioListener.volume.
// Saved across sessions in PlayerPrefs (UnityTuning.Pg3MasterPref ...). The SETTINGS screen's AUDIO rows are ordinary store rows
// (StoreItems; a range 0-10, step 1, so the slider, the pad's left / right and the mouse work as for every row); GameSettings
// pushes a changed row here (PullFrom) and starts the store from here (SeedStore): PlayerPrefs is the truth, the store's copy is the
// row's mirror. View only: nothing here touches the sim.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Pg3Volume
    {
        public enum Channel { Master, Music, Sfx }

        /// <summary>The settings store keys of the three rows (the player's settings file keeps the mirror under them).</summary>
        public const string MasterKey = "audioMaster", MusicKey = "audioMusic", SfxKey = "audioSfx";

        /// <summary>Where the steps are read and written (PlayerPrefs; the tests swap both for a dictionary).</summary>
        public static Func<string, int, int> GetInt = (key, fallback) => PlayerPrefs.GetInt(key, fallback);
        public static Action<string, int> SetInt = (key, value) => { PlayerPrefs.SetInt(key, value); PlayerPrefs.Save(); };

        /// <summary>Where MASTER lands (AudioListener.volume; the tests swap it so the Editor's own volume never moves).</summary>
        public static Action<float> ApplyListener = level => AudioListener.volume = level;

        static readonly Channel[] All = { Channel.Master, Channel.Music, Channel.Sfx };
        static readonly int[] steps = { -1, -1, -1 }; // -1: not read from storage yet

        /// <summary>A volume as a level, 0..1: what Pg3Audio and MusicDirector multiply in.</summary>
        public static float Master => Level(Step(Channel.Master));
        public static float Music => Level(Step(Channel.Music));
        public static float Sfx => Level(Step(Channel.Sfx));

        /// <summary>The channel's step now (read from storage once, then cached).</summary>
        public static int Step(Channel c)
        {
            int i = (int)c;
            if (steps[i] < 0) steps[i] = ClampStep(GetInt(PrefOf(c), DefaultOf(c)));
            return steps[i];
        }

        /// <summary>Set a channel's step (clamped to 0..max), saved at once; MASTER is applied to the listener. Unchanged: no write.</summary>
        public static void SetStep(Channel c, int step)
        {
            step = ClampStep(step);
            if (Step(c) != step)
            {
                steps[(int)c] = step;
                SetInt(PrefOf(c), step);
            }
            if (c == Channel.Master) ApplyMaster();
        }

        /// <summary>MASTER onto the listener (Pg3Audio's Awake and every change of the row call it).</summary>
        public static void ApplyMaster() => ApplyListener(Muted ? 0f : Master);

        /// <summary>`-skygrindMute` on the command line: no sound at all for this run (the lead's checks while GG needs quiet,
        /// 2026-10-28); never saved, so the player's own levels are untouched.</summary>
        public static readonly bool Muted = Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindMute") >= 0;

        /// <summary>A whole step in 0..max (a stored or typed number out of range, or not a number, is brought in).</summary>
        public static int ClampStep(double v)
        {
            if (double.IsNaN(v)) return 0;
            return (int)Math.Max(0, Math.Min(UnityTuning.Pg3VolumeMaxStep, Math.Round(v, MidpointRounding.AwayFromZero)));
        }

        /// <summary>A step as a level, 0..1.</summary>
        public static float Level(int step) => ClampStep(step) / (float)UnityTuning.Pg3VolumeMaxStep;

        /// <summary>A step moved by `dir` (left / right, a whole step), clamped: no wrap.</summary>
        public static int Nudge(int step, int dir) => ClampStep(step + dir);

        public static string KeyOf(Channel c) => c == Channel.Master ? MasterKey : c == Channel.Music ? MusicKey : SfxKey;

        public static string PrefOf(Channel c) => c == Channel.Master ? UnityTuning.Pg3MasterPref : c == Channel.Music ? UnityTuning.Pg3MusicPref : UnityTuning.Pg3SfxPref;

        public static int DefaultOf(Channel c) => c == Channel.Master ? UnityTuning.Pg3MasterDefault : c == Channel.Music ? UnityTuning.Pg3MusicDefault : UnityTuning.Pg3SfxDefault;

        /// <summary>Is `key` one of the three rows' settings keys?</summary>
        public static bool IsKey(string key) => key == MasterKey || key == MusicKey || key == SfxKey;

        /// <summary>The three rows as store items with their defaults (GameSettings.Extras adds them; MenuSettingsRows places them).</summary>
        public static List<(SettingItem, object)> StoreItems()
        {
            var list = new List<(SettingItem, object)>(3);
            foreach (Channel c in All)
            {
                string label = c == Channel.Master ? UnityTuning.Pg3MasterLabel : c == Channel.Music ? UnityTuning.Pg3MusicLabel : UnityTuning.Pg3SfxLabel;
                list.Add((new SettingItem { key = KeyOf(c), tab = UnityTuning.Pg3AudioTabId, label = label, type = "range", min = 0, max = UnityTuning.Pg3VolumeMaxStep, step = 1, digits = 0, hasDigits = true, desc = UnityTuning.Pg3VolumeDesc }, (double)DefaultOf(c)));
            }
            return list;
        }

        /// <summary>A changed row (or a reset tab) onto the volumes: each channel takes its store value.</summary>
        public static void PullFrom(SettingsStore store)
        {
            foreach (Channel c in All)
                if (store.values.TryGetValue(KeyOf(c), out object v) && v is double d) SetStep(c, ClampStep(d));
        }

        /// <summary>The store's rows set to the saved steps (the boot: PlayerPrefs is the truth) and MASTER applied.</summary>
        public static void SeedStore(SettingsStore store)
        {
            foreach (Channel c in All)
                if (store.values.ContainsKey(KeyOf(c))) store.values[KeyOf(c)] = (double)Step(c);
            store.version++;
            ApplyMaster();
        }

        /// <summary>For the tests: forget the cached steps (the next read goes to storage again).</summary>
        public static void ForgetSteps()
        {
            for (int i = 0; i < steps.Length; i++) steps[i] = -1;
        }
    }
}
