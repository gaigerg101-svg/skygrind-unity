// GG 2026-10-26 (quick fixes from play): the Unity game's movement over the web's. The web's movement stays exact in PortExport/config.json
// (the golden vectors and the trajectory replays read it); these are applied once, in the Survival boot, before the sim reads the config.
// The Core tests and the harness bots load their own configs and keep the web's numbers.
// - design.ultra71 (on): "the jump height gravity and movement ... almost a one to one copy" of ULTRAKILL: `set` is a map of
//   MovementConfig dotted paths ("jump.height", "dash.speed", ...) to absolute values (Research/2026-10-26-ultrakill/movement.md). Off:
//   -skygrindNoUltra71 (a player's switch) or enabled false.
// - design.movementTuning.jumpHeightMult (1.5): the earlier "jump 50% higher", used only when ultra71 is off.
// The web has no such numbers (its movement is fixed), so they live in Design/unity-config.json, not in code.
using System;
using System.Collections.Generic;
using System.Reflection;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class MovementTuning71
    {
        public const string Path = "design.movementTuning.jumpHeightMult";
        public const string UltraOffFlag = "-skygrindNoUltra71";

        /// <summary>The ULTRAKILL profile applied at the last boot (false: the web's movement, or the jump multiplier).</summary>
        public static bool UltraOn { get; private set; }

        /// <summary>The jump height multiplier from the design config (1 when the key is missing: the web's jump).</summary>
        public static double JumpHeightMult(GameConfig config) => config.Has(Path) ? config.Num(Path) : 1.0;

        /// <summary>Applies the ULTRAKILL profile (when on) or the jump multiplier. Returns the jump height after it (m).</summary>
        public static double Apply(GameConfig config) => Apply(config, Environment.GetCommandLineArgs());

        public static double Apply(GameConfig config, string[] args)
        {
            UltraOn = UltraWanted(config, args);
            FingerprintBaseline.Ultra71 = UltraOn; // the smoke's clean-slate / attract checks judge the run by the profile's own baseline
            if (UltraOn)
            {
                int n = 0;
                if (config.Get("design.ultra71.set") is Dictionary<string, object> set)
                    foreach (KeyValuePair<string, object> kv in set) if (SetField(config.movement, kv.Key, PortData.Num(kv.Value))) n++;
                UnityEngine.Debug.Log("[ultra71] on: " + n + " movement values set (jump " + config.movement.jump.height + " m in " + config.movement.jump.apexTime + " s)");
            }
            else
            {
                double mult = JumpHeightMult(config);
                if (mult != 1.0) config.movement.jump.height = config.movement.jump.height * mult;
            }
            return config.movement.jump.height;
        }

        static bool UltraWanted(GameConfig config, string[] args)
        {
            if (!(config.Has("design.ultra71.enabled") && config.Get("design.ultra71.enabled") is bool on && on)) return false;
            foreach (string a in args ?? new string[0]) if (string.Equals(a, UltraOffFlag, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        /// <summary>Sets one numeric field of the movement config by its dotted path ("jump.height"). False when the path is not a field.</summary>
        public static bool SetField(MovementConfig mc, string dotted, double value)
        {
            string[] p = dotted.Split('.');
            if (p.Length != 2) return false;
            FieldInfo section = typeof(MovementConfig).GetField(p[0], BindingFlags.Public | BindingFlags.Instance);
            object sec = section?.GetValue(mc);
            FieldInfo f = sec?.GetType().GetField(p[1], BindingFlags.Public | BindingFlags.Instance);
            if (f == null) return false;
            if (f.FieldType == typeof(double)) f.SetValue(sec, value);
            else if (f.FieldType == typeof(int)) f.SetValue(sec, (int)Math.Round(value));
            else return false;
            return true;
        }
    }
}
