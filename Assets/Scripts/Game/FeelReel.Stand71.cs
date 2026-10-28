// NIGHT71 builder B (slate B3 wyrm71): the boss63 reel's `-skygrindStand71 <m>` (Stand71Rig.cs has the rule). One hook in FeelReel.N63V.cs
// Boss63Scenario: Stand71Begin after the rise (before the hold), Stand71End after the hold (and in the scenario's offs, so an early end
// removes the rig too). Without the option: nothing (the reel as NIGHT63's).
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public const string Stand71Switch = "-skygrindStand71";
        static Stand71Rig stand71;

        static void Stand71Begin(SurvivalGame g, string boss, List<Action> offs)
        {
            string[] args;
            try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
            catch (Exception) { args = new string[0]; }
            if (!NightRowsHas(args, Stand71Switch)) return;
            string v = SameRun63Math.ArgAfter(args, Stand71Switch);
            CultureInfo ci = CultureInfo.InvariantCulture;
            if (v == null || !double.TryParse(v, NumberStyles.Float, ci, out double m) || m < UnityTuning.Stand71MinM || m > UnityTuning.Stand71MaxM)
            {
                Debug.Log("[stand71] option refused: " + Stand71Switch + " wants metres in " + UnityTuning.Stand71MinM.ToString("0", ci) + "-" + UnityTuning.Stand71MaxM.ToString("0", ci)
                    + ", got " + (v ?? "nothing") + " (the reel frames from the player's eye)");
                return;
            }
            Stand71End();
            var go = new GameObject("Stand71Rig") { hideFlags = HideFlags.HideAndDontSave };
            UnityEngine.Object.DontDestroyOnLoad(go);
            stand71 = go.AddComponent<Stand71Rig>();
            stand71.Begin(g, boss, (float)m);
            offs.Add(Stand71End);
            Debug.Log("[stand71] boss63 " + boss + ": the camera stands " + m.ToString("0.0", ci) + " m from the boss's centre for the hold (render only)");
        }

        static bool NightRowsHas(string[] args, string flag) => Skygrind.Core.NightRows.HasSwitch(args, flag);

        static void Stand71End()
        {
            if (stand71 == null) return;
            Debug.Log(stand71.Line());
            UnityEngine.Object.Destroy(stand71.gameObject);
            stand71 = null;
        }
    }
}
