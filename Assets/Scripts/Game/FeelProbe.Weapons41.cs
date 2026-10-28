// NIGHT41 builder T2 (INVENTORY A1 / A2 / A14): per-frame weapon telemetry in the reel's frame record, written next to `vel` in telemetry.jsonl
// (FeelProbe.Record.cs). Additive: old readers ignore the new fields.
//   fired   the weapon id when a shot / volley / swing left in this frame ("revolver", "shotgun", "gauntlet", "punch", ...; several joined with
//           '+'), null otherwise (the sim's "fire" / "punch" / "meleeSwing" events)
//   charge  0..1: the held KESTREL / THUNDERCLAP's charge level (Loadout.Charge35Level, else ChargeShots27.Level), 0 with no hold
//   pump    the THUNDERCLAP's pump phase 0..1 from its slide's start to home (Viewmodel.Pump41Phase, pump40's duration when it is live), -1 at rest
//   spin    the crosshair meter's turn 0..1 (Hud38.SpinDeg / Hud38SpinDeg; -1 with hud38 off); spinKind: Hud40.SpinKind (0 none, 1 tap, 2 charge, 3 cannon)
//   device  "pad" or "kbm" (PlayerDevices.Last)
// Reel-only: nothing here runs outside FeelProbe.Begin..End. No allocation per frame (the payloads are the sim's own strings).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelProbe
    {
        static Action<object> onFire41;
        static string fired41;

        /// <summary>Begin's hook: the fire / swing listener (one cached delegate).</summary>
        static void Weapons41Begin(EventBus b)
        {
            fired41 = null;
            onFire41 ??= o =>
            {
                string w = o as string ?? (o is MeleeEvent m ? m.weapon : null);
                if (w == null) return;
                fired41 = fired41 == null ? w : fired41 + "+" + w; // two in one frame: rare (a reel's frame is 1/30 s)
            };
            if (b == null) return;
            b.On("fire", onFire41);
            b.On("punch", onFire41);
            b.On("meleeSwing", onFire41);
        }

        /// <summary>End's hook.</summary>
        static void Weapons41End(EventBus b)
        {
            if (b == null || onFire41 == null) return;
            b.Off("fire", onFire41);
            b.Off("punch", onFire41);
            b.Off("meleeSwing", onFire41);
        }

        /// <summary>RecordFrame's hook: this frame's weapon fields.</summary>
        static void Weapons41Frame(SurvivalGame g, out string fired, out float charge, out float pump, out float spin, out int spinKind, out bool pad)
        {
            fired = fired41;
            fired41 = null;
            charge = 0f;
            Loadout lo = g != null ? g.loadout : null;
            if (lo != null)
            {
                double level = lo.Charge35Level;
                ChargeShots27 cs = lo.charge27 != null && lo.charge27.active ? lo.charge27 : null;
                if (level <= 0 && cs != null && cs.charging)
                {
                    int gi = ChargeShots27.Index(lo.Weapon);
                    if (gi >= 0) level = cs.Level(gi);
                }
                charge = (float)Math.Max(0, Math.Min(1, level));
            }
            pump = g != null && g.viewmodel != null ? (float)g.viewmodel.Pump41Phase : -1f;
            spin = Hud38.Live ? Hud38.SpinDeg / UnityTuning.Hud38SpinDeg : -1f;
            spinKind = Hud40.SpinKind;
            pad = PlayerDevices.Last == InputDevice.Pad;
        }
    }

    public sealed partial class Viewmodel
    {
        /// <summary>NIGHT41 T2 (the reels' telemetry): the THUNDERCLAP's pump phase, 0..1 from the slide's start to home, -1 at rest or with no
        /// THUNDERCLAP rig. The duration is the one the frame draws (pump40's while it is live).</summary>
        public double Pump41Phase
        {
            get
            {
                if (!rigs.TryGetValue("shotgun", out Rig sg) || sg.part == null) return -1;
                double dur = Pump40Duration(v.pumpDuration);
                if (!(dur > 0) || !(sg.pumpAge < dur)) return -1;
                return Math.Max(0, sg.pumpAge / dur);
            }
        }
    }
}
