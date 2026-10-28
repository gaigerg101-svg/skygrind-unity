// NIGHT13 U3-26.12 (S13, D-15): the debug keys (F3 the overlay, F8 every gate open, F9 the next round now) are a
// developer's: on in the Editor, in a Player only with -skygrindDev (UnityTuning.DevFlag) on the command line. The web
// kept its debug keys behind ?debug=1; a shipped Player must not open every gate on a stray F8.
// The lead gates SurvivalGame.HotKeys' F3 / F8 / F9 on DevKeys.Enabled (read once; the command line doesn't change).
using System;
using UnityEngine;

namespace Skygrind.Game
{
    public static class DevKeys
    {
        static int enabled = -1; // -1: not read yet

        /// <summary>
        /// True in the Editor, or when `args` (the command line) holds the dev flag exactly (any case: it's typed by hand
        /// into a shortcut). False by default: no args, other flags, or a near miss ("-skygrindDevX", "--skygrindDev").
        /// </summary>
        public static bool Allowed(string[] args, bool isEditor)
        {
            if (isEditor) return true;
            if (args == null) return false;
            foreach (string a in args)
                if (a != null && string.Equals(a, UnityTuning.DevFlag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>This process's answer (Application.isEditor, Environment.GetCommandLineArgs), read once and cached.</summary>
        public static bool Enabled
        {
            get
            {
                if (enabled < 0) enabled = Allowed(Environment.GetCommandLineArgs(), Application.isEditor) ? 1 : 0;
                return enabled == 1;
            }
        }
    }
}
