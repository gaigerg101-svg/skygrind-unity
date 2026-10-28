// NIGHT41 builder T2 (INVENTORY A1 / A2 / A14): the Player scenario `weapons41`: the KESTREL's rhythm and charge, the THUNDERCLAP's volley and
// pump, and its ground slug, each framed at fixed GAME times after the shot (the telemetry's `fired` / `charge` / `pump` / `spin` / `device`
// fields: FeelProbe.Weapons41.cs, every frame in telemetry.jsonl). The dispatch: FeelReel.N41.cs (PhaseN41T, N41ProofBody); the lead's hook in
// FeelReel.N27.cs calls it. Not run by its author (no Player in the worktree).
// Boot: a campaign level that owns both guns (reel.sh + -skygrindMap <level> + -skygrindReelOnly weapons41, L-062); a gun the level does not own
// yet is added by the reel (the line says so). The player stands on SafeSpot's open floor, God; a 1e9-hp husk at 6 m, frozen, is the target; the
// trigger is a virtual pad's RT (the real input path: PadReader), so `device` reads "pad" once the pad is read.
// Frames (shot-N30-<name>.png), t = GAME time after the frame the shot's "fire" event was seen (that frame's own capture is +0):
//   W41-kestrel-tapN-000 / -150 / -300 / -450   three KESTREL taps (a 2-frame press; with p39 the tap fires on the release), 0.5 s apart:
//                                              the crosshair's quarter turn and snap, the hammer, the kick
//   W41-kestrel-charge-full                     the KESTREL held to FULL (FullSteps + 0.1 s), before the release: the full-charge look
//   W41-kestrel-charged-000 / -150 / -300 / -450 the charged shot after the release: the full turn, the beam
//   W41-thunder-volley-000 / -150 / -300 / -450 a THUNDERCLAP click on the ground (charge40: fires on the release): the volley, the pump (pump40 /
//                                              pump41On when live: the line prints Viewmodel.Pump40Live and the pump phase seen at each frame)
//   W41-slug-release / W41-slug-150              the THUNDERCLAP held to FULL on the ground, released: the slug at release and +0.15 s
// Line: GREEN when every shot fired (taps 3, charged 1, volley 1, slug 1) and every frame was saved; each frame's pump / charge / spin printed.
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
        static IEnumerator Weapons41Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)")
                + $" | map {g.MapId} | charge35 {sim.charge35 != null}, p39 {sim.charge35 != null && sim.charge35.p39}, c40 {sim.charge35 != null && sim.charge35.c40}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            bool hadK = g.loadout.owned.IndexOf("revolver") >= 0, hadT = g.loadout.owned.IndexOf("shotgun") >= 0;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "weapons41", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 120;
            Husk bag = PlaceAt(g, 6, 0, out double _);
            if (bag != null) bag.hp = bag.maxHp = 1e9;
            Action aim = () => { God(g); if (bag != null && bag.alive) { bag.hp = 1e9; Look(g, bag.AimPoint); } };
            string lastFire = null;
            int fires = 0, charged = 0;
            offs.Add(g.events.On<string>("fire", w => { fires++; lastFire = w; }));
            offs.Add(g.events.On<ChargeShot27Event>("chargeShot27Fire", e => charged++));
            var lines = new List<string>();
            int framesSaved = 0;

            // the frames after a shot: waits for the fire event (up to 1.5 s), then +0 and each offset in GAME time
            IEnumerator After(string label, int f0, double[] offsets)
            {
                double w0 = sim.SimTime;
                while (fires == f0 && sim.SimTime - w0 < 1.5 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                if (fires == f0) { lines.Add($"{label}: NO SHOT in 1.5 s"); yield break; }
                double t0 = sim.SimTime;
                var sb = new System.Text.StringBuilder();
                sb.Append(label).Append(" (").Append(lastFire).Append("):");
                for (int k = 0; k < offsets.Length; k++)
                {
                    double off = offsets[k];
                    while (sim.SimTime - t0 < off && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                    string tag = off <= 0 ? (label.StartsWith("W41-slug", StringComparison.Ordinal) ? "release" : "000") : ((int)Math.Round(off * 1000)).ToString("000", ci);
                    double pump = g.viewmodel != null ? g.viewmodel.Pump41Phase : -1;
                    double charge = g.loadout.Charge35Level;
                    sb.Append(' ').Append(tag).Append(" pump ").Append(pump.ToString("0.00", ci)).Append(" charge ").Append(charge.ToString("0.00", ci))
                        .Append(" spin ").Append((Hud38.Live ? Hud38.SpinDeg / UnityTuning.Hud38SpinDeg : -1f).ToString("0.00", ci));
                    yield return Shot30(dir, label + "-" + tag);
                    framesSaved++;
                }
                lines.Add(sb.ToString());
            }

            // ---- the KESTREL: three taps 0.5 s apart ----
            yield return Equip27(g, "revolver");
            g.forcedCombat = null;
            yield return Wait27(g, 0.6, wallEnd, aim);
            int kTaps0 = fires;
            for (int i = 1; i <= 3; i++)
            {
                double ts = sim.SimTime;
                int f0 = fires;
                pad.Send(0f, 1f);
                yield return Frames(2);
                pad.Send(0f, 0f);
                yield return After("W41-kestrel-tap" + i, f0, new[] { 0.0, 0.15, 0.30, 0.45 });
                while (sim.SimTime - ts < 0.5 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            }
            int kTaps = fires - kTaps0;

            // ---- the KESTREL: a full charge ----
            ChargeShots27 c = g.loadout.charge27;
            if (c != null) yield return Wait27(g, c.cooldown[0] * sim.step + 0.1, wallEnd, aim);
            yield return Wait27(g, 0.6, wallEnd, aim);
            double kFull = c != null ? c.FullSteps(0) * sim.step : 0.9;
            int ch0 = charged, fk = fires;
            pad.Send(0f, 1f);
            yield return Wait27(g, kFull + 0.1, wallEnd, aim);
            string kLevel = g.loadout.Charge35Level.ToString("0.00", ci);
            yield return Shot30(dir, "W41-kestrel-charge-full");
            framesSaved++;
            pad.Send(0f, 0f);
            yield return After("W41-kestrel-charged", fk, new[] { 0.0, 0.15, 0.30, 0.45 });
            int kCharged = charged - ch0;

            // ---- the THUNDERCLAP: a volley (a click on the ground) ----
            yield return Equip27(g, "shotgun");
            g.forcedCombat = null;
            yield return Wait27(g, 0.6, wallEnd, aim);
            bool grounded = g.mover.grounded;
            int fv = fires;
            pad.Send(0f, 1f);
            yield return Frames(2);
            pad.Send(0f, 0f);
            yield return After("W41-thunder-volley", fv, new[] { 0.0, 0.15, 0.30, 0.45 });
            int volley = fires - fv;
            bool pumpLive = Viewmodel.Pump40Live;

            // ---- the THUNDERCLAP: the ground slug ----
            if (c != null) yield return Wait27(g, c.cooldown[1] * sim.step + 0.1, wallEnd, aim);
            yield return Wait27(g, 1.0, wallEnd, aim);
            double tFull = c != null ? c.FullSteps(1) * sim.step : 1.2;
            int ch1 = charged, fs = fires;
            pad.Send(0f, 1f);
            yield return Wait27(g, tFull + 0.1, wallEnd, aim);
            string tLevel = g.loadout.Charge35Level.ToString("0.00", ci);
            pad.Send(0f, 0f);
            yield return After("W41-slug", fs, new[] { 0.0, 0.15 });
            int slug = charged - ch1;
            yield return Wait27(g, 0.4, wallEnd, aim);
            yield return Stop();

            bool ok = kTaps >= 3 && kCharged >= 1 && volley >= 1 && slug >= 1 && framesSaved == 23; // 3 x 4 taps, 1 full, 4 charged, 4 volley, 2 slug
            res[0] = $"{(ok ? "GREEN" : "RED")} weapons41{(hadK ? "" : " (KESTREL added by the reel)")}{(hadT ? "" : " (THUNDERCLAP added by the reel)")}"
                + $" | KESTREL taps {kTaps}/3, charged {kCharged}/1 (held {kFull.ToString("0.00", ci)} s + 0.1, level at release {kLevel})"
                + $" | THUNDERCLAP volley {volley} (grounded {grounded}), slug {slug}/1 (held {tFull.ToString("0.00", ci)} s + 0.1, level {tLevel}); pump40 drawing {pumpLive}"
                + $" | frames {framesSaved}/23 || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
