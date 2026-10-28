// NIGHT63 builder V (slate B1's proof; Logs/n63/slate.json B1): the Player scenario `boss63`, boss61's walk-in extended into the fight (registered in
// FeelReel.N41.cs's PhaseN41T beside boss61; no line in FeelReel.N27.cs). One Player per boss (L-046): the booted campaign level picks it (n52-foundry
// THE WICK, n52-iceline THE ICEBREAKER, n57-kiln THE KILN WYRM, n52-span THE LOOM, n52-spire THE CROWN). Not run by its author (no Player in the
// worktree). Input through forced frames only; the player God. Every time below is on the reel's fixed clock (FeelClock.Now), from the boss's rise.
// THE RUN: boss61's (1920x1080, a CAMPAIGN run with DOORFREE, the husks killed, 3 s of quiet, the player at the boss room's spawn: the room's own
// entry, else a 0-4 s walk, else SurvivalSim.BossRoomBegin, said FORCED), then the fight held UnityTuning.Boss63HoldS (25 s) after the rise: the
// camera on the boss's weak point (BossAttackClock63.weak) firing taps at it once the reveal is over. If no stagger came by itself by
// Boss63ForceStaggerAtS (14 s) the reel empties the boss's meter itself (a Boss37Brain's Stagger, the Icebreaker's Drain, the wyrm's OnStagger: the
// sim's own public calls, the reel's action, as boss61's BossRoomBegin) and the line says FORCED: the stagger slump's frame on every boss.
// THE LINES (Player.log and dir/boss63-<boss>.txt; BossAttackClock63's text):
//   [boss63] attack <id> windup <ms> hit <ms> lead <ms> pose <ms | off>   each strike: its wind-up's first step and the strike's step (the sim's
//                                                                         steps on the fixed clock), and how long before the hit the drawn wind-up
//                                                                         pose (BossMotion63Probe.Windup >= Boss63PoseShare) showed; off: never
//   [boss63] flinch <ms> hp <a> -> <b> drawn +<n> frames | off            each damage event (the boss's hp fell) and the frames to the drawn flinch
//   [boss63] stagger <ms> <why>[ FORCED] / stagger end <ms> after <ms>    a window, the Icebreaker stuck / dazed, the wyrm staggered
//   [boss63] rms <boss> s<n> <m> m parts <n> max <m> m limit <share>       each second: the drawn parts' rms offset from their rigid pose
// FRAMES (shot-N30-<name>.png, Shot30): B63-<boss>-idle (at rest after Boss63IdleShotS), B63-<boss>-flinch (Boss63FlinchShotFrames after the first
// damage event past Boss63FlinchAfterS: B1's SPOT pose, with B63-<boss>-stagger), B63-<boss>-stagger (Boss63StaggerShotS into the first stagger),
// B63-<boss>-windup (a wind-up 70 % through: B2's cue frame; never B1's SPOT).
// THE VERDICT (B1's MAIN / GUARD): the row on (BossMotion63.On): GREEN when every second after the reveal has rms > 0, every strike's pose shows
// >= 250 ms before its hit, every flinch is drawn within 2 frames and no part passed its limit (share <= 1); else RED with the reasons. The row
// off (the row off in the build, -skygrindOff63, -skygrindNoBossMotion63): OFF when every second's rms is 0 (the rigid pose: the SPOT pair's other
// half), else RED. Builder G (B2) adds its own per-frame measure through the partial Boss63EachG (its own file; nothing here changes for it).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>Builder G's B2 hook (its own file): every frame of boss63's hold, the clock's read, ms since the rise; lines go to the reel's file.</summary>
        static partial void Boss63EachG(SurvivalGame g, string dir, BossAttack63 a, double ms, List<string> lines);

        /// <summary>Builder G's B2 hook (its own file): once after the hold, before the verdict; lines go to the reel's file.</summary>
        static partial void Boss63EndG(SurvivalGame g, string dir, string boss, List<string> lines);

        static IEnumerator Boss63Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; });
            yield return Wick48Res(1920, 1080);
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED boss63: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "); boot reel.sh + -skygrindMap <level> + -skygrindReelOnly boss63"; yield break; }
            SurvivalSim sim = g.sim;
            LevelDef lv = sim.campaignLevel;
            RoomDef room = lv != null ? lv.rooms.Find(r => r.kind == "boss") : null;
            if (room == null) { res[0] = $"RED boss63: level {lv?.id ?? "?"} has no boss room"; yield break; }
            string boss = room.boss ?? "?";
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            God(g);
            double wallEnd = Time.realtimeSinceStartup + 240;
            Action hold = () => { God(g); };
            yield return Wait27(g, 3.0, wallEnd, hold); // any banner left runs out
            g.UiView?.ClearBanners41();
            int rises = 0;
            double riseAt = -1;
            string risen = null;
            offs.Add(g.events.On<BossEvent>("bossRise", e => { rises++; if (riseAt < 0) { riseAt = FeelClock.Now; risen = e != null ? e.boss : null; } }));
            double yaw = room.spawnYawDeg * Math.PI / 180;
            Start(g, "boss63-" + boss, "camera", dir);
            yield return Frames(pre);
            g.Respawn(room.spawn + new Vec3(0, 0.05, 0), room.spawnYawDeg);
            g.input.SetLook(yaw, 0.05);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
            FeelProbe.MarkInput();
            // boss61's walk-in: the campaign begins the fight on its own when it enters the room; else walk in; else force it (said)
            // NIGHT63 review S8: both windows on the reel's fixed clock (FeelClock.Now), so every run, slow or fast, ON or OFF, begins the fight on
            // the same frames; the wall clock only as the safety cap (wallEnd)
            double t0 = FeelClock.Now;
            while (riseAt < 0 && FeelClock.Now - t0 < 3.0 && Time.realtimeSinceStartup < wallEnd) { God(g); yield return null; }
            string how = riseAt >= 0 ? "the room's own entry" : null;
            if (riseAt < 0)
            {
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05, moveZ = 1 };
                t0 = FeelClock.Now;
                while (riseAt < 0 && FeelClock.Now - t0 < 4.0 && Time.realtimeSinceStartup < wallEnd) { God(g); yield return null; }
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
                if (riseAt >= 0) how = "a 0-4 s walk forward";
            }
            if (riseAt < 0)
            {
                bool forcedBegin = sim.BossRoomBegin(boss, out Vec3 _, out double _);
                for (int k = 0; k < 30 && riseAt < 0; k++) yield return null;
                how = forcedBegin ? "FORCED BEGIN (BossRoomBegin)" : "NO BEGIN";
            }
            bool rowOn0 = BossMotion63.On(g.cfg);
            if (riseAt < 0) { yield return Stop(); res[0] = $"RED boss63 {boss}: no bossRise ({how}) | map {g.MapId}, row bossMotion63 {(rowOn0 ? "on" : "off")}"; yield break; }
            Stand71Begin(g, boss, offs); // NIGHT71 B wyrm71 (FeelReel.Stand71.cs): with -skygrindStand71 <m> the camera stands <m> m from the boss for the hold (render only); nothing without it

            // ---- the hold: fire at the weak point, read the clock every frame, print the lines ----
            var lines = new List<string>();
            void Say(string line) { lines.Add(line); Debug.Log(line); }
            double stepMs = sim.step * 1000;
            BossAttack63 prev = default;
            double lastHp = -1, windupAt = -1, poseAt = -1, staggerAt = -1, hitPredicted = -1;
            string windupOf = null;
            int attacks = 0, attacksPosed = 0, attacksLate = 0, attacksNoWindup = 0, staggers = 0, frame = 0;
            var flinchBook = new FlinchBook63(); // NIGHT63 B1d: the damage events and their drawn flinches (BossFlinchBook63.N63V.cs)
            var reach = new ReachBook63(); // NIGHT63 B1e: the drawn weak point against the sim's hit sphere over the OPEN window (BossReachBook63.N63V.cs)
            bool forced = false, forcedNow = false, sawStagger = false, flinchShot = false, staggerShot = false, idleShot = false, windupShot = false;
            double flinchDist = -1, staggerDist = -1; // the camera's distance to the boss's centre at the SPOT-candidate frames (m)
            string staggerWhy = "";
            var leads = new List<double>();
            int probeTaken = -1; // the last layer draw this reel took (BossMotion63Probe.Take)
            int flinchShotAtFrame = -1;
            // the rms per second
            int second = 0, liveSeconds = 0, zeroSeconds = 0;
            double secSumSq = 0, secMax = 0, secShare = 0, worstShare = 0, bestRms = 0;
            int secParts = 0, secFrames = 0;
            string reveal = "never left";
            while (FeelClock.Now - riseAt < UnityTuning.Boss63HoldS && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                // NIGHT63 (the lead, b4: one Crown flinch read "drawn off" on its flinch shot's own same-run OFF frame): a D1 held frame is not the
                // run's: no step ran, its draw is the row-OFF half of a shot. It is skipped whole (no damage event, flinch frame, pose, rms, stagger or
                // G's measure read on it), so a reel with the same-run toggle reads as a run without it.
                if (FeelClock.Holding) { yield return null; continue; }
                frame++;
                double ms = (FeelClock.Now - riseAt) * 1000;
                // NIGHT63 fix round 3: the layer's draw, taken once: this frame's (the views drawn in Update, before this coroutine) or the frame
                // before's (THE CROWN, drawn in LateUpdate, after it); drewAgo dates it, so a time or a frame count below is the draw's own
                int nowFrame = Time.frameCount;
                bool drew = BossMotion63Probe.Take(nowFrame, ref probeTaken) && BossMotion63Probe.Boss == boss;
                int drewAgo = drew ? nowFrame - BossMotion63Probe.Frame : 0;
                double drewMs = ms - drewAgo * FeelClock.Fixed * 1000;
                BossAttack63 a = BossAttackClock63.Read(sim, boss);
                bool rowOn = BossMotion63.On(g.cfg);
                bool fighting = a.Live && a.phase != BossPhase63.Reveal;
                if (fighting && reveal == "never left") reveal = ms.ToString("0", ci) + " ms";
                // the camera on the weak point; taps once the reveal is over
                if (a.Live) Look(g, fighting ? a.weak : a.centre, a.phase == BossPhase63.Stagger ? Target74.TurnAwayDeg() : 0.0); // NIGHT74 B target74: with -skygrindTarget74Away=<deg> the stagger frames look that many degrees right of the weak point (0 without it)
                bool tap = fighting && frame % Math.Max(1, UnityTuning.Boss63TapEvery) == 0;
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = tap, pressed = tap } };
                // the strike's wind-up: its first step on the sim's timeline (the read may come a step or two after it began)
                if (a.phase == BossPhase63.Windup && (prev.phase != BossPhase63.Windup || prev.attack != a.attack))
                {
                    windupAt = ms - (a.windupSteps - a.stepsToHit) * stepMs;
                    hitPredicted = ms + a.stepsToHit * stepMs;
                    windupOf = a.attack;
                    poseAt = -1;
                }
                if (a.phase == BossPhase63.Windup && drew && BossMotion63Probe.Windup >= UnityTuning.Boss63PoseShare && poseAt < 0) poseAt = Math.Max(windupAt, drewMs);
                if (!windupShot && a.phase == BossPhase63.Windup && a.Windup01 >= 0.7) { windupShot = true; Weak74Reel.Mark(g, boss, "windup", Say); yield return Shot30(dir, "B63-" + boss + "-windup"); }
                if (a.phase == BossPhase63.Strike && !(prev.phase == BossPhase63.Strike && prev.attack == a.attack))
                {
                    double hitAt = ms - a.phaseSteps * stepMs;
                    attacks++;
                    if (windupOf != a.attack || windupAt < 0) { attacksNoWindup++; Say(BossAttackClock63.AttackLine(a.attack, hitAt, hitAt, -1) + " (its wind-up began before the hold)"); }
                    else
                    {
                        Say(BossAttackClock63.AttackLine(a.attack, windupAt, hitAt, poseAt));
                        leads.Add(hitAt - windupAt);
                        if (poseAt >= 0) { attacksPosed++; if (hitAt - poseAt < 250) attacksLate++; }
                    }
                    windupAt = poseAt = hitPredicted = -1;
                    windupOf = null;
                }
                // a damage event (the boss's health fell) and the drawn flinches: NIGHT63 B1d, the book judges the waiting event on this frame's
                // take before a new one replaces it (THE CROWN's draw is taken a frame late: b8's 8983 ms flinch read "drawn off")
                bool damaged = a.Live && lastHp >= 0 && a.hp < lastHp - 1e-6;
                flinchBook.Frame(frame, drew, drewAgo, BossMotion63Probe.Flinch, damaged, ms, lastHp, a.hp, Say);
                if (damaged && !flinchShot && flinchShotAtFrame < 0 && ms >= UnityTuning.Boss63FlinchAfterS * 1000) flinchShotAtFrame = frame + UnityTuning.Boss63FlinchShotFrames;
                if (!flinchShot && flinchShotAtFrame >= 0 && frame >= flinchShotAtFrame) { flinchShot = true; flinchDist = (a.centre - g.Eye).Length; Weak74Reel.Mark(g, boss, "flinch", Say); yield return Shot30(dir, "B63-" + boss + "-flinch"); }
                // a stagger: a window, the Icebreaker stuck / dazed, the wyrm staggered
                if (a.phase == BossPhase63.Stagger && prev.phase != BossPhase63.Stagger)
                {
                    staggers++;
                    sawStagger = true;
                    staggerAt = ms;
                    Say(BossAttackClock63.StaggerLine(ms, a.why, forcedNow));
                    forcedNow = false;
                }
                if (prev.phase == BossPhase63.Stagger && a.phase != BossPhase63.Stagger && staggerAt >= 0) { Say(BossAttackClock63.StaggerEndLine(ms, ms - staggerAt)); staggerAt = -1; }
                if (!staggerShot && a.phase == BossPhase63.Stagger && staggerAt >= 0 && ms - staggerAt >= UnityTuning.Boss63StaggerShotS * 1000) { staggerShot = true; staggerDist = (a.centre - g.Eye).Length; staggerWhy = a.why; Weak74Reel.Mark(g, boss, "stagger", Say); yield return Shot30(dir, "B63-" + boss + "-stagger"); }
                if (!idleShot && fighting && a.phase == BossPhase63.Idle && ms >= UnityTuning.Boss63IdleShotS * 1000) { idleShot = true; Weak74Reel.Mark(g, boss, "idle", Say); yield return Shot30(dir, "B63-" + boss + "-idle"); }
                // no stagger by itself: the reel empties the meter (said FORCED)
                if (!sawStagger && !forced && fighting && ms >= UnityTuning.Boss63ForceStaggerAtS * 1000
                    && (a.phase == BossPhase63.Idle || a.phase == BossPhase63.Windup || a.phase == BossPhase63.Strike || a.phase == BossPhase63.Recover))
                {
                    forced = ForceStagger63(sim, boss);
                    forcedNow = forced;
                    if (!forced) Say("[boss63] stagger could not be forced at " + ms.ToString("0", ci) + " (" + a.phase + ")");
                }
                // the rms of the drawn parts' offsets, per second
                int sec = (int)(ms / 1000);
                if (sec != second)
                {
                    double rms = secParts > 0 ? Math.Sqrt(secSumSq / secParts) : 0;
                    Say(BossAttackClock63.RmsLine(boss, second, rms, secFrames > 0 ? secParts / secFrames : 0, secMax, secShare));
                    if (second >= 1 && reveal != "never left") { liveSeconds++; if (rms <= 0) zeroSeconds++; }
                    bestRms = Math.Max(bestRms, rms);
                    worstShare = Math.Max(worstShare, secShare);
                    second = sec;
                    secSumSq = secMax = secShare = 0;
                    secParts = secFrames = 0;
                }
                // NIGHT63 B1e (a measurement): the OPEN window's frames the layer drew (the stagger phase: the window, stuck / dazed, staggered)
                if (drew && BossMotion63Probe.WeakSet && a.phase == BossPhase63.Stagger) reach.AddDistance(BossMotion63Probe.WeakDist, BossMotion63Probe.WeakR);
                if (drew)
                {
                    secSumSq += BossMotion63Probe.SumSq;
                    secParts += BossMotion63Probe.Parts;
                    secFrames++;
                    secMax = Math.Max(secMax, BossMotion63Probe.Max);
                    secShare = Math.Max(secShare, BossMotion63Probe.LimitShare);
                }
                Boss63EachG(g, dir, a, ms, lines);
                lastHp = a.Live ? a.hp : lastHp;
                prev = a;
                yield return null;
            }
            flinchBook.End(Say);
            Stand71End(); // NIGHT71 B wyrm71: the stand's line, the camera back to the game (nothing without -skygrindStand71)
            Say(reach.Line(boss)); // NIGHT63 B1e: the proof line (a measurement: never a RED of its own)
            int flinches = flinchBook.Damage, flinchesDrawn = flinchBook.Drawn, flinchesLate = flinchBook.Late, flinchesOff = flinchBook.Off;
            yield return Frames(10);
            yield return Stop();
            Boss63EndG(g, dir, boss, lines);
            bool rowOnEnd = BossMotion63.On(g.cfg);
            var bad = new List<string>();
            if (how == null || how.StartsWith("FORCED") || how == "NO BEGIN") bad.Add("the rise was not the room's own (" + how + ")");
            if (risen != boss) bad.Add($"the rise said '{risen}', the room '{boss}'");
            if (!BossRule61.IsBoss(boss)) bad.Add("not one of the five campaign bosses");
            if (rowOnEnd)
            {
                if (liveSeconds == 0) bad.Add("no second of the fight measured");
                if (zeroSeconds > 0) bad.Add($"rms 0 in {zeroSeconds} of {liveSeconds} seconds (the rigid copy)");
                if (attacks == 0) bad.Add("no strike seen");
                if (attacks - attacksNoWindup > attacksPosed) bad.Add($"{attacks - attacksNoWindup - attacksPosed} strikes without a drawn wind-up pose");
                if (attacksLate > 0) bad.Add($"{attacksLate} wind-up poses under 250 ms before the hit");
                if (flinches == 0) bad.Add("no damage event (no flinch to judge)");
                if (flinchesOff > 0) bad.Add($"{flinchesOff} damage events without a drawn flinch");
                if (flinchesLate > 0) bad.Add($"{flinchesLate} flinches later than 2 frames");
                if (worstShare > 1 + 1e-6) bad.Add($"a part at {worstShare.ToString("0.00", ci)} of its limit (GUARD)");
                if (!flinchShot && !staggerShot) bad.Add("no SPOT frame (no flinch or stagger framed)");
            }
            else if (bestRms > 0) bad.Add($"row off but rms {bestRms.ToString("0.0000", ci)} m (the layer drew)");
            string verdict = !rowOnEnd ? (bad.Count == 0 || (bad.Count == 1 && bad[0].StartsWith("the rise")) ? "OFF" : "RED") : bad.Count == 0 ? "GREEN" : "RED";
            leads.Sort();
            string leadTxt = leads.Count == 0 ? "none" : $"min {leads[0].ToString("0", ci)} ms, max {leads[leads.Count - 1].ToString("0", ci)} ms";
            string summary = $"{verdict} boss63 {boss} | row bossMotion63 {(rowOnEnd ? "on" : "OFF (the SPOT pair's other half)")}{(rowOn0 != rowOnEnd ? " (flipped during the run)" : "")} | begun by {how}, bossRise x{rises} ('{risen}'), the reveal left at {reveal}"
                + $" | strikes {attacks} (wind-up seen {attacks - attacksNoWindup}, posed {attacksPosed}, posed under 250 ms {attacksLate}; leads {leadTxt})"
                + $" | damage events {flinches}: drawn {flinchesDrawn} (late {flinchesLate}), not drawn {flinchesOff} | staggers {staggers}{(forced ? " (one FORCED)" : "")}"
                + $" | rms seconds {liveSeconds} (zero {zeroSeconds}), best {bestRms.ToString("0.0000", ci)} m, limit share max {worstShare.ToString("0.00", ci)}"
                + $" | reach (open frames {reach.Frames}) max {reach.Max.ToString("0.00", ci)} m, p95 {reach.P95.ToString("0.00", ci)} m, outside the sim's sphere {reach.Outside}" // NIGHT63 B1e
                + $" | frames idle {(idleShot ? "yes" : "no")}, flinch {(flinchShot ? "yes at " + flinchDist.ToString("0.0", ci) + " m" : "no")}, stagger {(staggerShot ? "yes (" + staggerWhy + ") at " + staggerDist.ToString("0.0", ci) + " m" : "no")}, windup {(windupShot ? "yes" : "no")}"
                + $" | map {g.MapId}, room {room.id} '{room.name}'{(bad.Count > 0 ? " | BAD: " + string.Join("; ", bad) : "")}";
            lines.Add(summary);
            try
            {
                Directory.CreateDirectory(dir);
                File.WriteAllLines(Path.Combine(dir, "boss63-" + boss + ".txt"), lines);
            }
            catch (Exception) { }
            res[0] = summary;
        }

        /// <summary>The reel's forced stagger (its action, said FORCED): the boss's own public call that empties its meter. False: nothing done.</summary>
        static bool ForceStagger63(SurvivalSim sim, string boss)
        {
            switch (boss)
            {
                case BossAttackClock63.Wick: return sim.wick37?.brain != null && sim.wick37.brain.Stagger(sim.wick37.brain.stagger + 1, "boss63");
                case BossAttackClock63.Loom: return sim.loom37?.brain != null && sim.loom37.brain.Stagger(sim.loom37.brain.stagger + 1, "boss63");
                case BossAttackClock63.Crown: return sim.crown37?.brain != null && sim.crown37.brain.Stagger(sim.crown37.brain.stagger + 1, "boss63");
                case BossAttackClock63.Brawl:
                {
                    Brawl40 b = sim.brawl40;
                    if (b == null || !b.Fighting || b.Open) return false;
                    b.Drain(b.stagger + 1);
                    return b.Open;
                }
                case BossAttackClock63.Wyrm:
                    if (sim.wyrm31 == null) return false;
                    sim.wyrm31.OnStagger();
                    return sim.wyrm31.Staggered;
            }
            return false;
        }

        /// <summary>NIGHT63 V C2 (nearFade63's SPOT pose; NearFade63.N63V.cs): a fresh survival run at the map's open spot, one husk at a time placed
        /// straight ahead and frozen, its body's surface `m` metres from the eye (its feet m + its radius out), the camera on its middle; after 8
        /// frames (the dissolve applied) the frame N63-near-&lt;m&gt; (shot-N30-N63-near-&lt;m&gt;.png) and the line "[near63] husk <m> m: dissolve <d>"
        /// (the rule's value for the husk's capsule bounds, 1 = whole). Same placements every run (the spawn, the open spot, no randomness in the
        /// placement): a two-run pair holds. The SPOT pose: N63-near-0.5 (the eye at the body, as n62-freeze's vf/f0400 dome); N63-near-3.0 is the
        /// far control (whole on and off).</summary>
        static IEnumerator Near63Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; if (g.husks != null) g.husks.frozen = false; });
            yield return Wick48Res(1920, 1080);
            Fresh(g);
            yield return Frames(240); // the run-start title has run out
            var (open, openYaw, _) = Sandbox.OpenSpot(g.sim);
            g.Respawn(open, openYaw * 180 / Math.PI);
            g.input.SetLook(openYaw, 0);
            g.husks.KillAll();
            yield return Frames(10);
            bool rowOn = NearFade63.On(g.cfg);
            NearFade63.Params p = NearFade63.Params.From(g.cfg);
            Start(g, "near63", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var bad = new List<string>();
            float far = -1, close = -1;
            foreach (double m in new[] { 3.0, 1.5, 1.0, 0.7, 0.5, 0.3 })
            {
                God(g);
                g.husks.KillAll();
                g.husks.frozen = false;
                g.Respawn(open, openYaw * 180 / Math.PI);
                g.input.SetLook(openYaw, 0);
                yield return Frames(2);
                double radius = UnityTuning.Near63HuskRadiusM;
                Husk h = PlaceAt(g, m + radius, 0, out double yaw);
                if (h == null) { bad.Add("no floor for a husk at " + m.ToString("0.0", ci) + " m"); continue; }
                g.husks.frozen = true;
                Vec3 mid = h.position + new Vec3(0, h.Height * 0.5, 0);
                Look(g, mid);
                yield return Frames(8);
                Vector3 c = WebSpace.ToUnity(mid);
                float r = (float)h.Radius, ht = (float)h.Height;
                var b = new Bounds(c, new Vector3(2 * r, ht, 2 * r));
                Vector3 eye = g.cam != null ? g.cam.transform.position : WebSpace.ToUnity(g.Eye);
                float d = rowOn ? NearFade63.Dissolve(p, b, eye) : 1f;
                float gap = Mathf.Sqrt(b.SqrDistance(eye));
                string line = "[near63] husk " + m.ToString("0.0", ci) + " m (eye to its capsule's bounds " + gap.ToString("0.00", ci) + " m): dissolve " + d.ToString("0.00", ci) + (rowOn ? "" : " (row off)");
                lines.Add(line);
                Debug.Log(line);
                if (Math.Abs(m - 3.0) < 1e-9) far = d;
                if (Math.Abs(m - 0.5) < 1e-9) close = d;
                yield return Shot30(dir, "N63-near-" + m.ToString("0.0", ci));
            }
            yield return Frames(4);
            yield return Stop();
            if (far < 0 || close < 0) bad.Add("the far or the near frame missing");
            if (rowOn)
            {
                if (far >= 0 && far < 1f) bad.Add("the 3 m control dissolved (" + far.ToString("0.00", ci) + ")");
                if (close >= 0 && close > 0.5f) bad.Add("the 0.5 m husk barely dissolved (" + close.ToString("0.00", ci) + ")");
            }
            else if (close >= 0 && close < 1f) bad.Add("row off but dissolved");
            string verdict = !rowOn ? (bad.Count == 0 ? "OFF" : "RED") : bad.Count == 0 ? "GREEN" : "RED";
            string summary = $"{verdict} near63 | row nearFade63 {(rowOn ? "on" : "OFF (the SPOT pair's other half)")} | numbers farM {p.farM.ToString("0.00", ci)} nearM {p.nearM.ToString("0.00", ci)} floor {p.floor.ToString("0.00", ci)} | "
                + string.Join(" | ", lines) + $" | map {g.MapId}{(bad.Count > 0 ? " | BAD: " + string.Join("; ", bad) : "")}";
            try
            {
                Directory.CreateDirectory(dir);
                lines.Add(summary);
                File.WriteAllLines(Path.Combine(dir, "near63.txt"), lines);
            }
            catch (Exception) { }
            res[0] = summary;
        }
    }
}
