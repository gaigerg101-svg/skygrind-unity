// NIGHT8 U2-16: the LAB (DESIGN-RESEARCH section 4, decision D-03): research proposals that change an earlier GG
// decision, as opt-in A/B toggles for Demo Day, all off by default. The toggles are settings (lab.* booleans in the
// settings JSON, the LAB tab, listed in design.lab.toggles); their numbers are Design/unity-config.json's design.lab. A
// run reads the toggles once, at its start, and keeps them; a run with any toggle on is stamped LAB on the results and
// never stored as a personal best. With every toggle off, each hook below gives the web's value.
//   CLOSE-KILL HEAL     a punch / gauntlet kill, or any kill within closeKill.range m, heals closeKill.heal; regen climbs
//                       at closeKill.regenRate HP/s instead of regen.rate
//   FRESHNESS TEETH     FRESH pays freshness.freshMult instead of style.freshness.freshMult; a DULL tier under
//                       freshness.dullAt pays freshness.dullMult
//   STYLE RAMP          the state multiplier climbs from style.stateMult to styleRamp.rampMax over styleRamp.rampTime s of
//                       unbroken air / dash / wall ride / slam / slide / swing (the web's state-multiplier states)
//   READY FROM ROUND 1  READY from round ready.readyFrom instead of survival.readyFrom
//   SQUADS TIGHTEN      the squad interval x squads.perRound^(round - 1), never under squads.floor s
//   ULTRA HEALTH        NIGHT11 U3-24.9: ULTRAKILL's hard damage and blood heal (Core/UltraHealth.cs, design.lab.ultraHealth);
//                       Apply sets UltraHealth.enabled from it
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class LabMath
    {
        public const string CloseKillHeal = "lab.closeKillHeal", Freshness = "lab.freshness", StyleRamp = "lab.styleRamp", ReadyFromOne = "lab.readyFromOne", SquadTighten = "lab.squadTighten";
        public const string UltraHealthKey = "lab.ultraHealth"; // NIGHT11 U3-24.9 (the class UltraHealth holds its rules)

        public sealed class P
        {
            public double heal, range, regenRate, freshMult, dullAt, dullMult, rampMax, rampTime, readyFrom, perRound, floor;
            public List<string> healWeapons = new List<string>();

            public static P From(GameConfig c)
            {
                var p = new P
                {
                    heal = c.Num("design.lab.closeKill.heal"), range = c.Num("design.lab.closeKill.range"), regenRate = c.Num("design.lab.closeKill.regenRate"),
                    freshMult = c.Num("design.lab.freshness.freshMult"), dullAt = c.Num("design.lab.freshness.dullAt"), dullMult = c.Num("design.lab.freshness.dullMult"),
                    rampMax = c.Num("design.lab.styleRamp.rampMax"), rampTime = c.Num("design.lab.styleRamp.rampTime"),
                    readyFrom = c.Num("design.lab.ready.readyFrom"), perRound = c.Num("design.lab.squads.perRound"), floor = c.Num("design.lab.squads.floor"),
                };
                p.healWeapons.AddRange(c.Strs("design.lab.closeKill.weapons"));
                return p;
            }
        }

        /// <summary>CLOSE-KILL HEAL: a kill with a listed melee weapon, or any kill within range (the shot's length).</summary>
        public static bool IsCloseKill(string weapon, double distance, P p) => p.healWeapons.Contains(weapon) || distance <= p.range;

        /// <summary>FRESHNESS TEETH: the multiplier for a freshness value (the web's USED and STALE stay).</summary>
        public static double FreshnessMult(double value, StyleMath.S s, P p) => value < p.dullAt ? p.dullMult : value >= s.freshAt ? p.freshMult : StyleMath.FreshnessMult(value, s);

        /// <summary>FRESHNESS TEETH: the tier name, with DULL under dullAt.</summary>
        public static string FreshnessTierName(double value, StyleMath.S s, P p) => value < p.dullAt ? "DULL" : StyleMath.FreshnessTierName(value, s);

        /// <summary>STYLE RAMP: after `steps` fixed steps in a state-multiplier state, of `rampSteps` to the top; 1 elsewhere.</summary>
        public static double RampMult(bool inState, int steps, int rampSteps, StyleMath.S s, P p)
        {
            if (!inState) return 1;
            double t = rampSteps <= 0 ? 1 : Math.Min(1, (double)steps / rampSteps);
            return s.stateMult + (p.rampMax - s.stateMult) * t;
        }

        /// <summary>SQUADS TIGHTEN: round n's squad interval.</summary>
        public static double SquadInterval(double interval, int round, P p) => Math.Max(p.floor, interval * JsMath.Pow(p.perRound, Math.Max(0, round - 1)));
    }

    /// <summary>The LAB toggles (design.lab.toggles: key, name, default, desc) and the set a run started with.</summary>
    public sealed class Lab : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the toggles the run started with (a resumed run keeps them, whatever the LAB
        // tab says now). What Apply set elsewhere is those systems' (regen's rate, the style meter's switches, READY's
        // round, ULTRA HEALTH's switch; the director's SQUADS TIGHTEN hook comes back from its own flag).
        public void Save(StateWriter w) => w.Bools("toggles", new[] { closeKillHeal, freshness, styleRamp, readyFromOne, squadTighten, ultraHealth });

        public void Load(StateReader r)
        {
            List<bool> t = r.Bools("toggles");
            if (t.Count != 6) throw new StateException("snapshot: lab.toggles");
            closeKillHeal = t[0];
            freshness = t[1];
            styleRamp = t[2];
            readyFromOne = t[3];
            squadTighten = t[4];
            ultraHealth = t[5];
        }

        public sealed class Toggle
        {
            public string key, name, desc;
            public bool on;
        }

        public readonly LabMath.P p;
        public readonly List<Toggle> toggles = new List<Toggle>();
        public bool closeKillHeal, freshness, styleRamp, readyFromOne, squadTighten, ultraHealth;

        public Lab(GameConfig cfg)
        {
            p = LabMath.P.From(cfg);
            foreach (object o in cfg.Arr("design.lab.toggles"))
            {
                var d = PortData.Obj(o);
                toggles.Add(new Toggle { key = (string)d["key"], name = (string)d["name"], desc = (string)d["desc"], on = (bool)d["default"] });
            }
        }

        public bool Any => closeKillHeal || freshness || styleRamp || readyFromOne || squadTighten || ultraHealth;

        /// <summary>Take the toggles for a run (`isOn` reads a key; null: every toggle off, the web's game).</summary>
        public void Read(Func<string, bool> isOn)
        {
            closeKillHeal = isOn != null && isOn(LabMath.CloseKillHeal);
            freshness = isOn != null && isOn(LabMath.Freshness);
            styleRamp = isOn != null && isOn(LabMath.StyleRamp);
            readyFromOne = isOn != null && isOn(LabMath.ReadyFromOne);
            squadTighten = isOn != null && isOn(LabMath.SquadTighten);
            ultraHealth = isOn != null && isOn(LabMath.UltraHealthKey);
        }

        /// <summary>The names of the toggles on ("CLOSE-KILL HEAL, STYLE RAMP"), for the results' LAB line.</summary>
        public string Names()
        {
            var on = new List<string>();
            foreach (Toggle t in toggles)
            {
                bool v = t.key == LabMath.CloseKillHeal ? closeKillHeal : t.key == LabMath.Freshness ? freshness : t.key == LabMath.StyleRamp ? styleRamp
                    : t.key == LabMath.ReadyFromOne ? readyFromOne : t.key == LabMath.SquadTighten ? squadTighten : t.key == LabMath.UltraHealthKey && ultraHealth;
                if (v) on.Add(t.name);
            }
            return string.Join(", ", on);
        }

        /// <summary>
        /// Put the run's toggles into the systems (after Read, at a run's start): regen's rate, the style meter's freshness
        /// and ramp, READY's first round, the squad interval, ULTRA HEALTH's switch (NIGHT11: `ultra`, optional). CLOSE-KILL
        /// HEAL's heal is the kill handler from Wire.
        /// </summary>
        public void Apply(GameConfig cfg, Regen regen, StyleMeter style, SurvivalRun run, Director director, UltraHealth ultra = null)
        {
            if (ultra != null)
            {
                ultra.enabled = ultraHealth;
                ultra.Reset(); // no hard damage carried from the last run
            }
            if (regen != null) regen.rateOverride = closeKillHeal ? p.regenRate : 0;
            if (style != null)
            {
                style.lab = p;
                style.labFreshness = freshness;
                style.labRamp = styleRamp;
            }
            if (run != null) run.readyFrom = readyFromOne ? p.readyFrom : cfg.Num("survival.readyFrom");
            if (director != null) director.squadIntervalOf = squadTighten ? SquadIntervalFor : null;
        }

        double SquadIntervalFor(int round, double interval) => LabMath.SquadInterval(interval, round, p);

        /// <summary>CLOSE-KILL HEAL: a close player kill heals (while the run's toggle is on).</summary>
        public void Wire(EventBus events, Health health)
        {
            events.On<KillEvent>("kill", e =>
            {
                if (closeKillHeal && e.owner == "player" && LabMath.IsCloseKill(e.weapon, e.distance, p)) health.Heal(p.heal);
            });
        }
    }
}
