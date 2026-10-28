// NIGHT20 U3-34.9 rows B2 and D1 (builder G; Research/2026-10-06-additions-backlog.md rows 10 and 14), each behind
// design.adds (the lead's bit) and its own sub-block:
// - B2, named combos on screen: every named combo (ComboEmit: POP, LAUNCH + SNIPE, CONDUCTOR, NAILBOMB, SWAP-CANCEL, SENT)
//   also lands a style award, a styleBonus with the combo's name and design.adds.b2.points.<NAME> (StyleMeter pays it, as
//   the web's IMPACT or TRAPPED; the callout stays H's: UiStyleMeter reads N19Events.Combo);
// - D1, trap combos: an enemy this loadout pushed (the shockwave's cone or blast, the THUNDERCLAP variant's cone, a punch,
//   a gauntlet swing, a COMET launch) that a live trap kills (Traps.cs deals "trap" damage through Loadout.Damage) within
//   design.adds.d1.window s is SENT: a named combo credited to you. ULTRAKILL's environment kills; CoD's traps.
// The rows are read at a run's start (Reset). Allocation-free per step (reused lists; the awards' payload reused).
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class Combos20
    {
        /// <summary>D1: the trap combo's name (the callout, the style award).</summary>
        public const string Sent = "SENT";
    }

    public sealed partial class Loadout
    {
        bool b2On, d1On; // design.adds.b2 / .d1 this run
        double d1Window;
        readonly Dictionary<string, double> comboPoints = new Dictionary<string, double>(); // design.adds.b2.points
        readonly List<Enemy> pushed20 = new List<Enemy>(); // D1: enemies pushed by this loadout ...
        readonly List<double> pushedAt20 = new List<double>(); // ... and when (the loadout's clock, s)
        readonly StyleBonusEvent comboBonus = new StyleBonusEvent();

        void Combos20Init()
        {
            if (cfg.Has("design.adds.b2.points"))
                foreach (KeyValuePair<string, object> kv in cfg.Obj("design.adds.b2.points")) comboPoints[kv.Key] = PortData.Num(kv.Value);
            d1Window = cfg.Has("design.adds.d1.window") ? cfg.Num("design.adds.d1.window") : 0;
        }

        /// <summary>A run's start (Reset): the rows' bits, no pushes.</summary>
        void Combos20Reset()
        {
            b2On = n20.Add(cfg, "b2");
            d1On = n20.Add(cfg, "d1") && d1Window > 0;
            pushed20.Clear();
            pushedAt20.Clear();
        }

        /// <summary>B2: a named combo's style award (after its callout).</summary>
        void ComboAward20(string name, string weapon)
        {
            if (!b2On || !comboPoints.TryGetValue(name, out double points)) return;
            comboBonus.owner = owner;
            comboBonus.name = name;
            comboBonus.points = points;
            comboBonus.weapon = weapon;
            comboBonus.assistPay = 1;
            events.Emit("styleBonus", comboBonus);
        }

        /// <summary>D1: this loadout pushed `e` now (the window starts again).</summary>
        void Pushed20(Enemy e)
        {
            if (!d1On || e == null) return;
            for (int i = pushed20.Count - 1; i >= 0; i--)
                if (pushed20[i] == e || !pushed20[i].alive || clock - pushedAt20[i] > d1Window)
                {
                    pushed20.RemoveAt(i);
                    pushedAt20.RemoveAt(i);
                }
            pushed20.Add(e);
            pushedAt20.Add(clock);
        }

        /// <summary>D1: a trap killed `e` at `at`: SENT if this loadout pushed it within the window.</summary>
        void TrapKill20(Enemy e, Vec3 at)
        {
            for (int i = 0; i < pushed20.Count; i++)
            {
                if (pushed20[i] != e) continue;
                bool inWindow = clock - pushedAt20[i] <= d1Window;
                pushed20.RemoveAt(i);
                pushedAt20.RemoveAt(i);
                if (inWindow) ComboEmit(Combos20.Sent, "trap", at, 1);
                return;
            }
        }
    }
}
