// NIGHT42 builder U1: the Player scenario `ui42` (NIGHT42.md U1's proof): the meter through a charge (meter42), and (style42 / health42) a combo
// climbing two ranks, a hit taken, a heal. The dispatch: FeelReel.N42.cs (N42ProofBody "ui42"). Not run by its author (no Player in the worktree).
// Boot as weapons41 (reel.sh + -skygrindMap <level> + -skygrindReelOnly ui42, L-062): a campaign level; a gun it does not own yet is added (the
// line says so). The player stands on SafeSpot's floor; a 1e9-hp husk at 6 m, frozen, is the target; the trigger is the combat input of every
// sim step (forcedCombat: n42-r1's virtual-pad presses never reached the gun on the lead's machine, a Xencelabs HID present).
// Frames (shot-N30-<name>.png). The meter (the same frames are the BEFORE with meter42 off / -skygrindOff42 and the AFTER with it on):
//   U42-k-tap          a KESTREL tap's release +0.05 s: arc40 flashed cyan here (a false FULL); meter42: the empty track
//   U42-k-half         the KESTREL held to half its charge: arc40 ~33 %, meter42 50 %
//   U42-k-full         FULL +0.05 s (the flash) and U42-k-held: +0.6 s (the breathe)
//   U42-k-cool         the charge's cooldown half run (meter42: the dim refill at half)
//   U42-k-ping         the cooldown's end +0.05 s (meter42: the cyan ping on the empty track)
//   U42-t-pump         a THUNDERCLAP click +0.2 s (meter42: the pump on line 2)
//   U42-g-swing / -late a PULSE swing +0.2 s / +1.2 s (meter42: the swing's recovery on line 2; arc40 drew nothing)
// The style meter (style42) and the health (health42), BEFORE with the rows off, AFTER on (Ui42StyleHealthRun below):
//   U42-s-first, U42-s-up1 / -up1b, U42-s-up2, U42-s-combo   a chain of named bonuses climbing two ranks (the punch, the shake, the combo count)
//   U42-h-hit, U42-h-chunk, U42-h-drain, U42-h-heal / -heal2   a 30 % hit (the chunk, the punch, the style letter red-orange) and a 25 % heal (the glow)
// The line prints meter42's check against the telemetry's `charge` (Hud42: every drawn frame, meter42 and arc40 alike): GREEN when meter42 is
// live with 0 mismatches and 0 false FULL frames and every frame was saved; with meter42 off the counters read 0 (Hud42 not live): the BEFORE
// numbers are the arc40 columns of an ON run (the same frames, the same truth).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N42Ui(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Ui42Scenario(g, dir, res, offs, pre);

        static IEnumerator Ui42Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId}";
            double maxHp0 = g.health.max; // the level's own max (God raises it for the meter's part; the health part puts it back)
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            bool hadK = g.loadout.owned.IndexOf("revolver") >= 0, hadT = g.loadout.owned.IndexOf("shotgun") >= 0, hadG = g.loadout.owned.IndexOf("gauntlet") >= 0;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            double maxLv = 0; // the highest charge level the sim reached (printed: a RED says whether a press reached the gun)
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "ui42", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 150;
            Husk bag = PlaceAt(g, 6, 0, out double _);
            if (bag != null) bag.hp = bag.maxHp = 1e9;
            bool god = true;
            Action aim = () => { if (god) God(g); if (bag != null && bag.alive) { bag.hp = 1e9; Look(g, bag.AimPoint); } maxLv = Math.Max(maxLv, g.loadout.Charge35Level); };
            int fires = 0;
            offs.Add(g.events.On<string>("fire", w => fires++));
            var lines = new List<string>();
            int saved = 0, want = 0;
            Hud42.ResetCounters();

            IEnumerator Shot(string name)
            {
                want++;
                lines.Add(name + ": " + (name.StartsWith("U42-s", StringComparison.Ordinal) || name.StartsWith("U42-h", StringComparison.Ordinal) ? Ui42StatLine(g, ci) : Meter42Line(ci)));
                yield return Shot30(dir, name);
                saved++;
            }
            IEnumerator Until(Func<bool> cond, double maxS)
            {
                double t0 = sim.SimTime;
                while (!cond() && sim.SimTime - t0 < maxS && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            }

            // The presses go through the combat input of every sim step (forcedCombat, the smoke's and the pilots' path): n42-r1's virtual-pad
            // presses never reached the gun on the lead's machine (a Xencelabs HID present). A click: pressed one frame, held one, released.
            CombatInput Trig(bool held, bool pressed) => new CombatInput { weapon = -1, fire = new Button { held = held, pressed = pressed } };
            IEnumerator Click() { g.forcedCombat = Trig(true, true); aim(); yield return null; g.forcedCombat = Trig(true, false); aim(); yield return null; g.forcedCombat = IdleCombat; }
            IEnumerator Press() { g.forcedCombat = Trig(true, true); aim(); yield return null; g.forcedCombat = Trig(true, false); }
            void Release() => g.forcedCombat = IdleCombat;

            // ---- the meter: the KESTREL ----
            yield return Equip27(g, "revolver");
            g.forcedCombat = IdleCombat; // the reel holds the combat input from here: no device can press or release for it
            ChargeShots27 c = g.loadout.charge27;
            if (c != null) yield return Wait27(g, c.cooldown[0] * sim.step + 0.1, wallEnd, aim);
            yield return Wait27(g, 0.6, wallEnd, aim);
            int f0 = fires;
            yield return Click();
            yield return Until(() => fires > f0, 1.0);
            yield return Wait27(g, 0.05, wallEnd, aim);
            yield return Shot("U42-k-tap");
            yield return Wait27(g, 0.6, wallEnd, aim);
            yield return Press();
            yield return Until(() => g.loadout.Charge35Level >= 0.5, 3.0);
            yield return Shot("U42-k-half");
            yield return Until(() => g.loadout.Charge35Level >= 1.0, 3.0);
            yield return Wait27(g, 0.05, wallEnd, aim);
            yield return Shot("U42-k-full");
            yield return Wait27(g, 0.55, wallEnd, aim);
            yield return Shot("U42-k-held");
            Release();
            double cool = c != null ? c.CooldownSteps(0) * sim.step : 2.5;
            yield return Wait27(g, cool * 0.5, wallEnd, aim);
            yield return Shot("U42-k-cool");
            yield return Until(() => c == null || c.cooldown[0] <= 0, cool + 0.5);
            yield return Wait27(g, 0.05, wallEnd, aim);
            yield return Shot("U42-k-ping");

            // ---- the THUNDERCLAP's pump on line 2 ----
            yield return Equip27(g, "shotgun");
            g.forcedCombat = IdleCombat;
            if (c != null) yield return Wait27(g, c.cooldown[1] * sim.step + 0.1, wallEnd, aim);
            yield return Wait27(g, 0.8, wallEnd, aim);
            f0 = fires;
            yield return Click();
            yield return Until(() => fires > f0, 1.0);
            yield return Wait27(g, 0.2, wallEnd, aim);
            yield return Shot("U42-t-pump");

            // ---- the PULSE's swing on line 2 ----
            bool pulse = sim.n27.pulse27 && sim.pulse27 != null && sim.pulse27.active;
            yield return Equip27(g, "gauntlet");
            g.forcedCombat = IdleCombat;
            yield return Wait27(g, 0.8, wallEnd, aim);
            int sw0 = Hud42.SwingFrames;
            yield return Click();
            yield return Wait27(g, 0.2, wallEnd, aim);
            yield return Shot("U42-g-swing");
            yield return Wait27(g, 1.0, wallEnd, aim);
            yield return Shot("U42-g-late");
            int swingFrames = Hud42.SwingFrames - sw0;
            bool meterRow = N42View.RowOn(g.cfg, "meter42");
            string meter = $"meter42 row {meterRow}, live {Hud42.Live || Hud42.Frames > 0} | frames checked {Hud42.Frames}: mismatches meter42 {Hud42.Mismatch} (worst {Hud42.WorstErr.ToString("0.000", ci)}) vs arc40 {Hud42.MismatchArc40} (worst {Hud42.WorstErrArc40.ToString("0.000", ci)}); false FULL meter42 {Hud42.FalseFull} vs arc40 {Hud42.FalseFullArc40}; flashes {Hud42.Flashes}, pings {Hud42.Pings}; swing frames {swingFrames} (pulse27 {pulse}) | presses by forcedCombat: fires {fires}, max charge {maxLv.ToString("0.00", ci)}";
            // the row off: the BEFORE frames (arc40), nothing to check; on: drawn, exact, no false FULL, the swing drawn
            bool meterOk = !meterRow || (Hud42.Frames > 0 && Hud42.Mismatch == 0 && Hud42.FalseFull == 0 && (!pulse || swingFrames > 0));

            // ---- style42 / health42 (UiHud33.Style42.cs / Health42.cs) ----
            god = false;
            bool styleOk = true;
            string style = "";
            IEnumerator part = Ui42StyleHealth(g, maxHp0, aim, wallEnd, Shot, r => { styleOk = r.ok; style = r.line; });
            while (part.MoveNext()) yield return part.Current;

            yield return Wait27(g, 0.4, wallEnd, aim);
            yield return Stop();
            bool ok = meterOk && styleOk && saved == want;
            res[0] = $"{(ok ? "GREEN" : "RED")} ui42{(hadK ? "" : " (KESTREL added)")}{(hadT ? "" : " (THUNDERCLAP added)")}{(hadG ? "" : " (gauntlet added)")}"
                + $" | {meter} | {style} | frames {saved}/{want} || {string.Join(" || ", lines)} | {pc}";
        }

        /// <summary>The meter's state for a frame's line: state, fill, lines, the truth.</summary>
        static string Meter42Line(CultureInfo ci) => Hud42.Live
            ? $"{Hud42.State} fill {Hud42.Fill.ToString("0.00", ci)} line {Hud42.Line.ToString("0.00", ci)} line2 {Hud42.Line2.ToString("0.00", ci)} charge {Hud42.Truth.ToString("0.00", ci)}"
            : $"meter42 off (arc40 {Hud40.ArcState} fill {Hud40.ArcFill.ToString("0.00", ci)})";

        /// <summary>The style meter and the health for a frame's line (the sim's and the views').</summary>
        static string Ui42StatLine(SurvivalGame g, CultureInfo ci)
        {
            StyleMeter st = g.style;
            string s = st != null ? $"style meter {st.meter.ToString("0", ci)} rank {st.rank}" : "no style";
            s += Style42.Live ? $" view '{Style42.Letter}' {Style42.Name} x{Style42.Combo} scale {Style42.Scale.ToString("0.00", ci)} shake {Style42.Shake.ToString("0.0", ci)} line {Style42.LineShown.ToString("0.00", ci)} award '{Style42.Award}'" : " (style42 off)";
            s += $" | hp {g.health.hp.ToString("0", ci)}/{g.health.max.ToString("0", ci)}";
            s += Health42.Live ? $" view {Health42.NumberShown} lit {Health42.LitTicks} chunk {Health42.ChunkTicks} glow {Health42.GlowTicks}" : " (health42 off)";
            return s;
        }

        /// <summary>The style / health part's result.</summary>
        struct Ui42Part
        {
            public bool ok;
            public string line;
        }

        static partial void Ui42StyleHealthBody(SurvivalGame g, double maxHp0, Action aim, double wallEnd, Func<string, IEnumerator> shot, Action<Ui42Part> done, ref IEnumerator body);

        static partial void Ui42StyleHealthBody(SurvivalGame g, double maxHp0, Action aim, double wallEnd, Func<string, IEnumerator> shot, Action<Ui42Part> done, ref IEnumerator body)
            => body = Ui42StyleHealthRun(g, maxHp0, aim, wallEnd, shot, done);

        /// <summary>The bonus names the reel's chain rotates through (StyleCore diminishes a repeated name: a rotation keeps each award whole).</summary>
        static readonly string[] Ui42Bonus = { "AIRSHOT", "WALLKILL", "RICOSHOT", "SLAMDOWN", "MULTIKILL", "CLOSE CALL", "HEADSHOT", "IMPACT" };

        /// <summary>
        /// style42 / health42's frames. The combo: named bonuses through the bus (`styleBonus`, the path IMPACT / TRAPPED take: StyleCore pays them with
        /// the meter's own multipliers) every 0.25 s until the rank is two above the first one shown (or 8 s). Frames:
        ///   U42-s-first   the first award +0.05 s (the letter's first punch, the award line)
        ///   U42-s-up1 / U42-s-up1b   the first rank up +0.03 / +0.15 s (the punch and shake; settled)
        ///   U42-s-up2     the second rank up +0.03 s; U42-s-combo +0.4 s (the combo count, the name, the award)
        /// The health: the level's own max back (God off), a hit of 30 % of the max (Health.Damage: the `hurt` event, the style penalty), then a heal of
        /// 25 % (Health.Heal). Frames: U42-h-hit +0.03 s (the chunk red-orange, the punch, the shake, the style letter red), U42-h-chunk +0.3 s (the
        /// chunk held), U42-h-drain +0.8 s (drained), U42-h-heal +0.05 s (the glow), U42-h-heal2 +0.35 s (fading).
        /// </summary>
        static IEnumerator Ui42StyleHealthRun(SurvivalGame g, double maxHp0, Action aim, double wallEnd, Func<string, IEnumerator> shot, Action<Ui42Part> done)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            bool styleRow = N42View.RowOn(g.cfg, "style42"), healthRow = N42View.RowOn(g.cfg, "health42");
            StyleMeter st = g.style;
            IEnumerator Wait(double s) { double t0 = sim.SimTime; while (sim.SimTime - t0 < s && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; } }

            // ---- the combo: two ranks up ----
            int ups0 = Style42.RankUps, awards0 = Style42.Awards;
            int first = -1, upsSeen = 0, k = 0, bestCombo = 0;
            double tStart = sim.SimTime;
            bool styleOn = st != null && st.enabled;
            while (styleOn && sim.SimTime - tStart < 8 && Time.realtimeSinceStartup < wallEnd)
            {
                int before = st.rank;
                bool wasShown = st.meter > 0;
                g.events.Emit("styleBonus", new StyleBonusEvent { owner = "player", name = Ui42Bonus[k % Ui42Bonus.Length], points = 40 });
                k++;
                yield return null;
                if (!wasShown && first < 0)
                {
                    first = st.rank;
                    yield return Wait(0.05);
                    yield return shot("U42-s-first");
                }
                else if (st.rank > before && first >= 0)
                {
                    upsSeen++;
                    yield return Wait(0.03);
                    if (upsSeen == 1)
                    {
                        yield return shot("U42-s-up1");
                        yield return Wait(0.12);
                        yield return shot("U42-s-up1b");
                    }
                    else
                    {
                        yield return shot("U42-s-up2");
                        bestCombo = Style42.Combo;
                        yield return Wait(0.4);
                        yield return shot("U42-s-combo");
                        break;
                    }
                }
                if (first < 0 && st.meter > 0) first = st.rank;
                yield return Wait(0.25);
            }
            if (bestCombo == 0) bestCombo = Style42.BestCombo;
            string style = $"style42 row {styleRow}, live {Style42.Live}, style on {styleOn} | bonuses {k}, rank first {first} -> {(st != null ? st.rank : -1)}, ups seen {upsSeen} (view {Style42.RankUps - ups0}), awards (view) {Style42.Awards - awards0}, combo at up2 {bestCombo}, last award '{Style42.Award}'";
            bool styleOk = upsSeen >= 2 && (!styleRow || (Style42.Live && Style42.RankUps - ups0 >= 2 && bestCombo >= Style42Min()));

            // ---- the health: a hit, the chunk, a heal ----
            double max = maxHp0 > 0 && maxHp0 < 1e5 ? maxHp0 : 100;
            g.health.max = max;
            g.health.hp = max;
            yield return Wait(0.5);
            int hits0 = Health42.Hits, heals0 = Health42.Heals;
            g.health.Damage(max * 0.3, "reel");
            yield return Wait(0.03);
            int chunkAtHit = Health42.ChunkTicks;
            yield return shot("U42-h-hit");
            yield return Wait(0.27);
            int chunkHeld = Health42.ChunkTicks;
            yield return shot("U42-h-chunk");
            yield return Wait(0.7);
            int chunkAfter = Health42.ChunkTicks;
            yield return shot("U42-h-drain");
            g.health.Heal(max * 0.25);
            yield return Wait(0.05);
            int glow = Health42.GlowTicks;
            yield return shot("U42-h-heal");
            yield return Wait(0.3);
            yield return shot("U42-h-heal2");
            string health = $"health42 row {healthRow}, live {Health42.Live} | max {max.ToString("0", ci)}, hp now {g.health.hp.ToString("0", ci)} | hits {Health42.Hits - hits0}, chunk ticks at +0.03 {chunkAtHit}, +0.3 {chunkHeld}, +1.0 {chunkAfter}; heals {Health42.Heals - heals0}, glow ticks at +0.05 {glow}";
            bool healthOk = !healthRow || (Health42.Live && Health42.Hits - hits0 >= 1 && chunkAtHit > 0 && chunkHeld > 0 && chunkAfter == 0 && Health42.Heals - heals0 >= 1 && glow > 0);
            done(new Ui42Part { ok = styleOk && healthOk, line = style + " | " + health });
        }

        /// <summary>The combo the second rank up must show at least (the awards it took: 2 ranks need more than 2 awards).</summary>
        static int Style42Min() => UnityTuning.Style42ComboMin + 1;

        static IEnumerator Ui42StyleHealth(SurvivalGame g, double maxHp0, Action aim, double wallEnd, Func<string, IEnumerator> shot, Action<Ui42Part> done)
        {
            IEnumerator body = null;
            Ui42StyleHealthBody(g, maxHp0, aim, wallEnd, shot, done, ref body);
            if (body == null) { done(new Ui42Part { ok = true, line = "style42 / health42: not in this build" }); yield break; }
            while (body.MoveNext()) yield return body.Current;
        }
    }
}
