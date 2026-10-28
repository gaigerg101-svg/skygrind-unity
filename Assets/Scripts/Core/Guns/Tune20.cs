// NIGHT20 U3-34.5 / U3-34.8 (builder G, design.arsenal.n20.tune): tonight's gun retune, one knob per gun per iteration
// (Research/2026-10-06-balance-zoom-regen.md 7.5; the ledger in G's report and NIGHT20-LOG.md). Every knob is a multiplier
// on the web's own number, applied where the loadout reads that number, only in a run with design.arsenal.n20 on (the
// web's weapons.* in PortExport/config.json stay the web's):
// - damage: x every hit the gun deals (Loadout.Damage: pellets, nails, rockets' blasts, beams, arcs, grenades);
// - interval: x the gun's fire interval (the LONGWAVE's: its cooldown; the STITCHER's: 1 / rate);
// - radius: x a blast's splash radius (the COMET's rockets, the BOUNCER's grenades);
// - charge: x the LONGWAVE's charge time.
// Absent: x1. With the block off nothing here is read (bit for bit NIGHT19's numbers). Allocation-free (dictionaries read
// once at construction; a lookup per shot).
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed partial class Loadout
    {
        readonly Dictionary<string, double> tuneDamage = new Dictionary<string, double>();
        readonly Dictionary<string, double> tuneInterval = new Dictionary<string, double>();
        readonly Dictionary<string, double> tuneRadius = new Dictionary<string, double>();
        readonly Dictionary<string, double> tuneCharge = new Dictionary<string, double>();

        /// <summary>design.arsenal.n20.tune is live this run (design.arsenal.n20 on).</summary>
        bool TuneOn => n20.arsenal;

        void Tune20Init()
        {
            const string root = "design.arsenal.n20.tune";
            if (!cfg.Has(root)) return;
            foreach (KeyValuePair<string, object> gun in cfg.Obj(root))
            {
                if (gun.Key.StartsWith("_")) continue;
                foreach (KeyValuePair<string, object> knob in PortData.Obj(gun.Value))
                {
                    if (knob.Key.StartsWith("_")) continue;
                    if (!SetKnob(gun.Key, knob.Key, PortData.Num(knob.Value)))
                        throw new System.Exception($"{root}.{gun.Key}.{knob.Key}: an unknown knob (damage, interval, radius, charge)");
                }
            }
        }

        /// <summary>Set one knob (the config's table at construction; the tests). False: no such knob.</summary>
        public bool SetKnob(string gun, string knob, double value)
        {
            Dictionary<string, double> t = knob == "damage" ? tuneDamage : knob == "interval" ? tuneInterval : knob == "radius" ? tuneRadius : knob == "charge" ? tuneCharge : null;
            if (t == null) return false;
            t[gun] = value;
            return true;
        }

        static double Knob(Dictionary<string, double> table, string weapon) => weapon != null && table.TryGetValue(weapon, out double m) ? m : 1;

        /// <summary>x the gun's damage this run (1 with the block off or no knob).</summary>
        public double TuneDamage(string weapon) => TuneOn ? Knob(tuneDamage, weapon) : 1;

        /// <summary>x the gun's fire interval this run.</summary>
        public double TuneInterval(string weapon) => TuneOn ? Knob(tuneInterval, weapon) : 1;

        /// <summary>x the gun's splash radius this run.</summary>
        public double TuneRadius(string weapon) => TuneOn ? Knob(tuneRadius, weapon) : 1;

        /// <summary>x the gun's charge time this run.</summary>
        public double TuneCharge(string weapon) => TuneOn ? Knob(tuneCharge, weapon) : 1;
    }
}
