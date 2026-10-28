// NIGHT73 builder B (brief-B.md: spring72 that a player can see; dispatched from FeelReel.N57.cs's N57ProofBody, `spring73`). Not run by its author (builders never run a Player). The n73-a
// reel's kill12 killed its husk with one shot at 12 m, so spring72's flinch never fired ("flinch squash peak 0.000") and only the 2.2 % idle breath ran. This scenario gives the spring something
// to show, in one recording, on the booted map (`-skygrindMap n57-kiln`, as kill12 on the same side):
//   1. a husk stands still Spring73DistanceM (5 m) in front of the player, its health lifted (UnityTuning.ReelHitHp, as hit12) so a shot cannot kill it; the idle breath alone for the reel's
//      pre-roll and Spring73BreathMs;
//   2. Spring73StillHits (4) shots land on it, one every Spring73HitEveryMs (0.5 s: the flinch is quiet by 0.15 s, each hit is read alone);
//   3. Spring73LetGoAfterMs after the last, the husk is let go (husks.frozen false): it walks the 2.8 m to its melee range and WINDS UP (the sim's own: Enemy.windupSteps, the husk's
//      config windup time, the AI, nothing forced);
//   4. Spring73WindHitShare (0.3) of the way through the wind-up a fifth non-lethal shot lands (the flinch inside the gather), and at Spring73KillShare (0.65) its health goes to 1 and the
//      killing shot lands: wind-up, flinch and death all in frame; the post-roll films the death (UnityTuning.ReelPostRollKillMs).
// Nothing in the game changes: the reel sets the husk's hp and taps the gun, as hit12 / kill12 do. The player is in God mode (the husk's strike is never landed: it dies at 0.65 of its wind-up).
// A shot is a TAP (UnityTuning.Spring73HitWaitFrames: the KESTREL fires on the release of a tap; n73-d's first version held the trigger for the whole wait, fired at the release 40 frames later, and
// its counter, which looked for the hit inside the hold, read 0). The hits are recorded by the HIT EVENT's own handler (reel frame n, and FeelProbe.FrameCount at it = the telemetry's frame f), and
// the lethal hp (1) is written in that handler on the fifth hit, after its damage, so the view kicks once (a write from the coroutine kicked a second time: "flinch squash peak 0.325").
// The frame numbers n in the lines are the reel's own count; f is the telemetry's / the JPG's index: to cut a strip use Tools/night/n73-springcut.py, which reads telemetry.jsonl too.
// THE LINES (Player.log and dir/spring73.txt, which also holds one row of the springs per frame):
//   [spring73] <GREEN|OFF|RED> spring73 | husk id <n> at <d> m, hits landed <h> of 5 non-lethal (at reel frames n ...; telemetry frames f ...; gaps ...; aimed at 30) + the kill (at n, f), hit events, kills <k>, wind-up <seen|NOT SEEN> (n <a>..<b>, <w> samples) | row spring72 on|off, drawn in <a> of <b> samples
//   [spring73] springs (row on|off): flinch squash peak per hit ..., first stretch ..., wind-up squash peak ..., idle breath peak ..., the sum drawn peak ... | tuned: kick, Hz, z
// GREEN: row on, every non-lethal hit's flinch squash at least Spring73FlinchGreenMin, the breath seen, a wind-up seen, the husk killed, and the row drawn it; OFF: the same with the row off and
// nothing drawn (the springs run either way: the OFF half of a pair shows the same numbers and `drawn in 0`); RED: else, with the reasons.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        struct Spring73Row
        {
            public int n, fp, windSteps;
            public bool have, drawn;
            public float flinch, wind, breath, own;
        }

        static IEnumerator Spring73Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            double d = UnityTuning.Spring73DistanceM;
            offs.Add(() => { g.forcedCombat = IdleCombat; if (g.husks != null) g.husks.frozen = false; });
            g.husks.frozen = true;
            Husk mark = PlaceAt(g, d, 0, out double _);
            if (mark == null)
            {
                // kill12's fallback, copied (the kiln has no floor exactly d m from its spawn on every heading): the player on other open floor nearby, off the record
                Vec3 home = g.Feet;
                foreach (double r in UnityTuning.ReelMoveTries)
                {
                    for (int k = 0; k < 8 && mark == null; k++)
                    {
                        double a = k * Math.PI / 4;
                        double x = home.x - Math.Sin(a) * r, z = home.z - Math.Cos(a) * r;
                        if (!FloorAt(g, x, z, home.y)) continue;
                        if (g.world.RayIntersect(new Vec3(x, home.y + 0.5, z), new Vec3(0, 1, 0), out RayHit up) && up.distance < 2.5) continue;
                        g.Respawn(new Vec3(x, home.y, z), 0);
                        yield return Frames(4);
                        God(g);
                        mark = PlaceAt(g, d, 0, out double _);
                    }
                    if (mark != null) break;
                }
            }
            if (mark == null) { res[0] = $"SKIPPED spring73: no open floor at {d.ToString("0", ci)} m (the collider cast, and the nearby spots kill12 tries)"; yield break; }
            mark.hp = Math.Max(mark.hp, UnityTuning.ReelHitHp); // no shot can kill it until the last (hit12's way)
            Look(g, mark.AimPoint);
            yield return Frames(20); // the view settles on the target

            var rows = new List<Spring73Row>();
            var hitN = new List<int>(); // the reel's frame n of each NON-LETHAL hit event on the husk, in order (recorded by the handler itself, not by the shot loop: n73-d's loop looked for the hit inside a 40-frame hold and found none)
            var hitFp = new List<int>(); // FeelProbe.FrameCount in the handler = the telemetry's own frame f (EndFrame bumps it at the end of this frame's RenderFrame)
            int n = 0, hitEvents = 0, kills = 0, windStartN = -1, windEndN = -1, windSamples = 0, killN = -1, killFp = -1, lastLat = 0;
            bool hitFlag = false;
            int nonLethalWanted = UnityTuning.Spring73StillHits + 1;
            double hpBefore = mark.hp;
            offs.Add(g.events.On("hit", o =>
            {
                if (!(o is HitEvent h) || h.owner != "player" || h.enemy != mark.id) return;
                hitFlag = true;
                hitEvents++;
                // the event is raised inside a sim step of the Update the coroutine resumes after: this frame's reel number is n + 1 (n is bumped after the resume)
                if (h.killed) { killN = n + 1; killFp = FeelProbe.FrameCount; return; }
                hitN.Add(n + 1);
                hitFp.Add(FeelProbe.FrameCount);
                // THE LETHAL HP is written HERE, in the step of the last non-lethal hit and after its damage (the event follows Damage; EnemyViews' own handler, registered earlier, has already read the hp for its
                // shed check): the view then sees ONE drop (275 -> 1) on that hit's frame and kicks once. n73-d wrote it from the coroutine a frame or ten later: a second kick, "flinch squash peak 0.325"
                if (hitN.Count == nonLethalWanted) mark.hp = Math.Min(mark.hp, 1);
            }));
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e != null && e.owner == "player") kills++; }));

            void Sample()
            {
                int ws = mark.alive ? mark.windupSteps : 0;
                if (ws > 0)
                {
                    if (windStartN < 0) windStartN = n;
                    windEndN = n;
                    windSamples++;
                }
                var row = new Spring73Row { n = n, fp = FeelProbe.FrameCount, windSteps = ws };
                if (Spring72Probe.Last.TryGetValue(mark.id, out Spring72Probe.Sample s))
                {
                    row.have = true;
                    row.drawn = s.drawn;
                    row.flinch = s.flinch;
                    row.wind = s.wind;
                    row.breath = s.breath;
                    row.own = s.own;
                }
                rows.Add(row);
            }
            // one recorded frame with this input: the player stays alive, the look stays on the husk, the sample is taken after the frame
            IEnumerator Fr(CombatInput c)
            {
                God(g);
                if (mark.alive) Look(g, mark.AimPoint);
                g.forcedCombat = c;
                yield return null;
                n++;
                Sample();
            }
            IEnumerator Idle(int count)
            {
                for (int i = 0; i < count; i++) yield return Fr(IdleCombat);
            }
            // one shot = one TAP: the KESTREL fires on the RELEASE of a tap and charges while the trigger is held (n73-d: holding it for the whole wait made every shot come at the release, 40 frames
            // later, and the gap 69). The trigger is pressed on one frame and held T125TapFrames frames, then let go, then idle frames until the hit event (a gun still cooling down queues the tap); up
            // to Spring73TapTries taps. lastLat: frames from the tap's first frame to the hit's frame, so the next tap can start `every - 1 - lastLat` idle frames after this hit (hit to hit = every).
            IEnumerator Shoot(bool lethal)
            {
                if (lethal && mark.hp > 1) mark.hp = 1; // the fallback (no wind-up seen): after a wind-up the hit handler has written it in the step of the fifth hit
                hitFlag = false;
                int start = n + 1;
                for (int t = 0; t < UnityTuning.Spring73TapTries && !hitFlag; t++)
                {
                    for (int i = 0; i < UnityTuning.T125TapFrames && !hitFlag; i++) yield return Fr(new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = i == 0 } });
                    for (int i = 0; i < UnityTuning.Spring73HitWaitFrames && !hitFlag; i++) yield return Fr(IdleCombat);
                }
                g.forcedCombat = IdleCombat;
                lastLat = hitFlag ? Math.Max(0, n - start) : 0;
            }

            Start(g, "spring73", "kill", dir, d, mark.id);
            yield return Idle(pre + Ms(UnityTuning.Spring73BreathMs));
            FeelProbe.MarkInput();
            int every = Ms(UnityTuning.Spring73HitEveryMs);
            for (int k = 0; k < UnityTuning.Spring73StillHits; k++)
            {
                yield return Shoot(false); // returns on the frame its hit event came in (n); the next tap starts every - 1 - lastLat idle frames later, so hit to hit is `every` frames
                if (k + 1 < UnityTuning.Spring73StillHits) yield return Idle(Math.Max(0, every - 1 - lastLat));
            }
            yield return Idle(Ms(UnityTuning.Spring73LetGoAfterMs));
            g.husks.frozen = false; // let go: it walks in and winds up on the sim's own rules
            int walkMax = Ms(UnityTuning.Spring73WalkMaxMs);
            for (int i = 0; i < walkMax && windStartN < 0 && mark.alive; i++) yield return Fr(IdleCombat);
            double windTime = mark.Params.windupTime;
            bool windSeen = windStartN >= 0;
            if (windSeen)
            {
                int hitAt = (int)Math.Round(windTime * UnityTuning.Spring73WindHitShare * UnityTuning.ReelFps);
                int killAt = (int)Math.Round(windTime * UnityTuning.Spring73KillShare * UnityTuning.ReelFps);
                yield return Idle(Math.Max(0, hitAt - (n - windStartN)));
                yield return Shoot(false); // the fifth non-lethal hit, inside the gather
                yield return Idle(Math.Max(0, killAt - (n - windStartN)));
            }
            yield return Shoot(true); // the killing shot (a wind-up not seen: the husk is killed anyway and the line says so)
            yield return Idle(Ms(UnityTuning.ReelPostRollKillMs));
            g.forcedCombat = IdleCombat;
            yield return Stop();

            // ---- the numbers ----
            bool on = NightRows.ViewOn(g.cfg, EnemyLook.Spring72Row);
            int haveRows = 0, drawnRows = 0, windPeakN = -1;
            float breathPeak = 0f, windPeak = 0f, drawnPeak = 0f;
            foreach (Spring73Row r in rows)
            {
                if (!r.have) continue;
                haveRows++;
                if (r.drawn) { drawnRows++; if (-r.own > drawnPeak) drawnPeak = -r.own; }
                if (r.windSteps == 0 && Math.Abs(r.breath) > breathPeak) breathPeak = Math.Abs(r.breath);
                if (-r.wind > windPeak) { windPeak = -r.wind; windPeakN = r.n; }
            }
            int nonLethal = hitN.Count;
            var peaks = new List<string>();
            var stretches = new List<string>();
            var peakAfter = new List<string>();
            var weak = new List<string>();
            for (int k = 0; k < nonLethal; k++)
            {
                float lo = 0f, hi = 0f;
                int loN = -1;
                foreach (Spring73Row r in rows)
                    if (r.have && r.n >= hitN[k] - 1 && r.n <= hitN[k] + 18 && r.flinch < lo) { lo = r.flinch; loN = r.n; }
                foreach (Spring73Row r in rows)
                    if (r.have && loN >= 0 && r.n > loN && r.n <= hitN[k] + 18 && r.flinch > hi) hi = r.flinch;
                peaks.Add((-lo).ToString("0.000", ci));
                stretches.Add(hi.ToString("0.000", ci));
                peakAfter.Add(loN >= 0 ? (loN - hitN[k]).ToString(ci) : "-");
                if (-lo < UnityTuning.Spring73FlinchGreenMin) weak.Add((k + 1) + " (" + (-lo).ToString("0.000", ci) + ")");
            }
            var bad = new List<string>();
            if (nonLethal < nonLethalWanted) bad.Add($"only {nonLethal} of {nonLethalWanted} non-lethal hits landed");
            if (killN < 0 && kills < 1) bad.Add("the husk was not killed");
            if (!windSeen) bad.Add($"no wind-up seen in {UnityTuning.Spring73WalkMaxMs} ms of walking (the husk did not reach its melee range)");
            if (weak.Count > 0) bad.Add("a flinch squash under " + UnityTuning.Spring73FlinchGreenMin.ToString("0.00", ci) + " at hit " + string.Join(", ", weak));
            if (breathPeak < UnityTuning.Spring73BreathGreenMin) bad.Add($"the idle breath peaked at {breathPeak.ToString("0.000", ci)}");
            if (haveRows == 0) bad.Add("no spring sample: the view never drew the husk (Spring72Probe.Last empty)");
            if (on && drawnRows == 0) bad.Add("row spring72 on but nothing was drawn");
            if (!on && drawnRows > 0) bad.Add("row spring72 off but a squash was drawn");
            string verdict = bad.Count > 0 ? "RED" : on ? "GREEN" : "OFF";
            var gaps = new List<string>();
            for (int k = 1; k < hitFp.Count; k++) gaps.Add((hitFp[k] - hitFp[k - 1]).ToString(ci));
            string line1 = $"{verdict} spring73 | husk id {mark.id} at {d.ToString("0", ci)} m, hp {hpBefore.ToString("0", ci)} before the hits, hits landed {nonLethal} of {nonLethalWanted} non-lethal (at reel frames n {string.Join(",", hitN)}; telemetry frames f {string.Join(",", hitFp)}; gaps {string.Join(",", gaps)}; aimed at {every}) + the kill ({(killN >= 0 ? "at n " + killN + ", f " + killFp : "NOT landed")}), hit events {hitEvents}, kills {kills}"
                + $", wind-up {(windSeen ? "seen" : "NOT SEEN")} (n {windStartN}..{windEndN}, {windSamples} samples; the husk's windup time {windTime.ToString("0.00", ci)} s)"
                + $" | row spring72 {(on ? "on" : "off")}, drawn in {drawnRows} of {haveRows} samples | map {g.MapId}{(bad.Count > 0 ? " | BAD: " + string.Join("; ", bad) : "")}";
            string line2 = $"springs (row {(on ? "on" : "off")}): flinch squash peak per hit {string.Join(" ", peaks)} (frames after the hit {string.Join(" ", peakAfter)}), first stretch {string.Join(" ", stretches)}, wind-up squash peak {windPeak.ToString("0.000", ci)} at n {windPeakN}, idle breath peak {breathPeak.ToString("0.000", ci)}, the sum drawn peak {drawnPeak.ToString("0.000", ci)}"
                + $" | tuned: kick {UnityTuning.Spring72FlinchKick.ToString("0.0", ci)}, {UnityTuning.Spring72FlinchHz.ToString("0.0", ci)} Hz, z {UnityTuning.Spring72FlinchZeta.ToString("0.00", ci)}";
            Debug.Log("[spring73] " + line1);
            Debug.Log("[spring73] " + line2);
            try
            {
                Directory.CreateDirectory(dir);
                var sb = new StringBuilder();
                sb.AppendLine(line1).AppendLine(line2);
                sb.AppendLine("n recordedFrame windSteps have drawn flinch wind breath own");
                foreach (Spring73Row r in rows)
                    sb.AppendLine($"{r.n} {r.fp} {r.windSteps} {(r.have ? 1 : 0)} {(r.drawn ? 1 : 0)} {r.flinch.ToString("0.0000", ci)} {r.wind.ToString("0.0000", ci)} {r.breath.ToString("0.0000", ci)} {r.own.ToString("0.0000", ci)}");
                File.WriteAllText(Path.Combine(dir, "spring73.txt"), sb.ToString());
            }
            catch (Exception) { }
            res[0] = line1;
        }
    }
}
