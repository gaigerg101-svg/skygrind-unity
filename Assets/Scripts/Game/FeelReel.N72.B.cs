// NIGHT72 builder B (slate B5; the phase FeelReel.N72.cs pre-placed and FeelReel.N57.cs's N57ProofBody dispatches): bosskill72, the kill feel on a boss. Not run by its author (builders never
// run a Player). One Player per boss (L-046): the booted campaign level picks it (n57-kiln THE KILN WYRM, n52-spire THE CROWN; the other three: not built, said). Boss63's front (the campaign run,
// the walk-in to the rise), then the fight BossKill72LeadS from the rise (the camera on the weak point, taps), then the reel's action: the boss's health set to BossKill72HpShare of its pool
// (the sim's own public field, as wyrmdies sets it), its stagger forced (so every hit lands: ForceStagger63), and the taps go on until it dies. Frames (Shot30, shot-N30-BK72-<boss>-<name>.png):
//   pre     the last frame before the killing blow (the boss alive, hp at or under the share)
//   dying   two frames after bossDying (the finisher begins: the bars slide in with finish55; the kick and the FOV punch peak)
//   down    the frame of bossDown
//   after   BossKill72AfterS after bossDown (the bars out, or going)
// With -skygrindSameRun63,n71.kicks71 each frame also writes the kicks71-OFF half of the same instant (FeelReel.N63Same.cs: an ALIGNED pair, L-156); finish55 is not in FrameRows, so its pair is two
// sides (-skygrindRows72 finish55:1 against the plain config: the reel is deterministic).
// THE LINES (Player.log and dir/bosskill72-<boss>.txt):
//   [bosskill72] <boss> hp <a> -> <b> at <ms> ms (forced), stagger forced <bool>
//   [bosskill72] <boss> kill: bossDying at <ms> ms, kill event (kind boss) at <ms> ms, bossDown at <ms> ms (ms from the rise, the reel's clock)
//   [bosskill72] <boss> feel: kicks71 <on|off>, kill kicks <n>, fov punches <n>, last kick <deg> deg | finish55 <on|off>: beats <n>, kick peak <deg> deg, fov peak <deg> deg, bars peak <share>
//   GREEN|RED|OFF bosskill72 <boss> | ... (GREEN: the boss died, bossDying or the kill seen, and with finish55 on: a beat, a kick peak over half Finish55KickDeg, the bars at their full share;
//   OFF: finish55 off and no beat drawn (kick peak 0, bars 0); RED: else, with the reasons)
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
        /// <summary>The switch that turns bosskill72 into the ORDINARY kill's aligned frames (below): `-skygrindKill72 <metres>`, the husk's distance (3 to 30). A HUSK kill: kicks71's kick only; the BOSS
        /// kill (finish55's 6 deg kick, the FOV punch, the bars) is bosskill72 WITHOUT this switch (the t3 B5 SHORT: the kk sides had it and killed a husk).</summary>
        public const string Kill72Switch = "-skygrindKill72";

        static partial void N72BossKillBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            string[] args;
            try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
            catch (Exception) { args = new string[0]; }
            string v = SameRun63Math.ArgAfter(args, Kill72Switch);
            if (v != null && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) && d >= 3 && d <= 30) body = Kill72Scenario(g, dir, res, offs, pre, d);
            else body = BossKill72Scenario(g, dir, res, offs, pre);
        }

        /// <summary>L-156 (a SPOT pair must be aligned in time): kill12's kill, but with the frames AT the kick. kill12 writes a jpg every frame and n72-prune keeps every 50th: its f0100 is 22
        /// frames after the kill, where kicks71's 13-frame kick (n72-h0 telemetry: frames 78-90, pitch +2.19 deg at 79, FOV -1.17 deg) is over. Here the kill's frames are SHOTS (kept):
        /// K72-kill+0 (the frame the hit was seen), +1, +2 (the kick's peak), +5, +15, each with the row-off half of the same instant under -skygrindSameRun63,n71.kicks71 (Shot30 does it). The line:
        /// `[kill72] ... kill kicks <n>, fov punches <n>, last kick <deg>, last fov <deg> | kicks71 on|off | hitzone47 on|off | models41 on|off`. GREEN when the husk died and the five shots were taken.</summary>
        static IEnumerator Kill72Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, double d)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            offs.Add(() => { g.forcedCombat = IdleCombat; });
            Husk mark = PlaceAt(g, d, 0, out double _);
            if (mark == null)
            {
                // kill12's fallback, copied (t2: "no open floor at 12 m" on the kiln, where kill12 itself found a spot): the player on other open floor nearby, off the record
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
            if (mark == null) { res[0] = $"SKIPPED kill72: no open floor at {d.ToString("0", ci)} m (the collider cast, and the nearby spots kill12 tries)"; yield break; }
            mark.hp = 1;
            Vec3 aim = mark.AimPoint;
            Look(g, aim);
            yield return Frames(20); // the view settles on the target
            bool done = false;
            int kills = 0;
            offs.Add(g.events.On("hit", o => { if (o is HitEvent h && h.owner == "player") done = true; }));
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e != null && e.owner == "player") kills++; }));
            Start(g, "kill72", "kill", dir, d, mark.id);
            for (int i = 0; i < pre; i++) { God(g); Look(g, aim); yield return null; }
            FeelProbe.MarkInput();
            Feel46Probe.Clear();
            int n = 0;
            while (!done && n++ < 60) { God(g); Look(g, aim); g.forcedCombat = Fire; yield return null; }
            g.forcedCombat = IdleCombat;
            int shots = 0;
            foreach (var (name, gap) in new[] { ("kill+0", 0), ("kill+1", 1), ("kill+2", 1), ("kill+5", 3), ("kill+15", 10) })
            {
                if (gap > 0) yield return Frames(gap);
                yield return Shot30(dir, "K72-" + name);
                shots++;
            }
            yield return Frames(Ms(UnityTuning.ReelPostRollKillMs));
            yield return Stop();
            bool died = kills > 0;
            // the t3 verdict (B5 SHORT): this is a HUSK's kill (kicks71's kick); finish55's boss kick and FOV punch never fire on it: the BOSS kill is bosskill72 WITHOUT -skygrindKill72
            string line = $"{(died && shots == 5 ? "GREEN" : "RED")} kill72 (a HUSK kill, NOT a boss: finish55 is not measured here, run bosskill72 without -skygrindKill72) | husk at {d.ToString("0", ci)} m, hit {(done ? "yes" : "NO")}, kills {kills}, shots {shots} of 5 (K72-kill+0, +1, +2 the peak, +5, +15) | kill kicks {Feel46Probe.KillKicks}, fov punches {Feel46Probe.FovPunches}, last kick {Feel46Probe.LastKickDeg.ToString("0.00", ci)} deg, last fov {Feel46Probe.LastFovDeg.ToString("0.00", ci)} deg"
                + $" | kicks71 {(Kicks71.On(g.cfg) ? "on" : "off")} | hitzone47 {(NightRows.ViewOn(g.cfg, "hitzone47") ? "on" : "off")} | models41 {(NightRows.ViewOn(g.cfg, "models41") ? "on" : "off")} | map {g.MapId}";
            Debug.Log("[kill72] " + line);
            res[0] = line;
        }

        /// <summary>The boss's health body (hp, maxHp) of the five campaign bosses: the sim's own fields, null when it is not the live boss.</summary>
        static bool BossHp72(SurvivalSim sim, string boss, out double hp, out double maxHp, out Action<double> set)
        {
            hp = maxHp = 0;
            set = null;
            switch (boss)
            {
                case BossAttackClock63.Wyrm:
                    if (sim.wyrm24 == null || !sim.wyrm24.Alive) return false;
                    { Wyrm w = sim.wyrm24; hp = w.body.hp; maxHp = w.body.maxHp; set = v => { w.body.hp = v; }; return true; }
                case BossAttackClock63.Crown:
                    if (sim.crown37 == null || !sim.crown37.Alive) return false;
                    { Crown37 c = sim.crown37; hp = c.body.hp; maxHp = c.body.maxHp; set = v => { c.body.hp = v; }; return true; }
                case BossAttackClock63.Wick:
                    if (sim.wick37 == null || !sim.wick37.Alive) return false;
                    { Wick37 w = sim.wick37; hp = w.body.hp; maxHp = w.body.maxHp; set = v => { w.body.hp = v; }; return true; }
                case BossAttackClock63.Loom:
                    if (sim.loom37 == null || !sim.loom37.Alive) return false;
                    { Loom37 l = sim.loom37; hp = l.body.hp; maxHp = l.body.maxHp; set = v => { l.body.hp = v; }; return true; }
            }
            return false;
        }

        /// <summary>The t3 round (B5 SHORT: the kill-timed sides killed a husk, and a wyrm's health set to the kill share in phase 1 would come back at 66 %: Wyrm.Apply resets it to the phase
        /// threshold): steps THE KILN WYRM's phases down the way FeelReel.N24.Wyrm's wyrmdies does. Each call in p1 / p2 (or p3 where phase4At exists): hp to BossKill72StageBelow of the next
        /// threshold and Apply(0) (the sim's own beat). True once the wyrm is in its LAST phase (the kill share can be set); false while it is stepping down (intro, beat and dying wait).</summary>
        static bool Wyrm72Descend(Wyrm w, double ms, Action<string> say)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string last = w.p.phase4At > 0 ? "p4" : "p3";
            if (w.phase == last) return true;
            double at = w.phase == "p1" ? w.p.phase2At : w.phase == "p2" ? w.p.phase3At : w.phase == "p3" ? w.p.phase4At : 0;
            if (at <= 0) return false;
            string from = w.phase;
            w.body.hp = w.body.maxHp * at * UnityTuning.BossKill72StageBelow;
            w.Apply(0);
            if (w.phase != from) say($"[bosskill72] wyrm stage: {from} -> {w.phase} (hp set to {(at * UnityTuning.BossKill72StageBelow * 100).ToString("0", ci)} % of the pool, Apply(0)) at {ms.ToString("0", ci)} ms");
            return false;
        }

        /// <summary>The boss's own public damage call for the last points (the same call wyrmdies makes): true when a live boss took it. Used only when the taps did not kill it in BossKill72ApplyAfterS.</summary>
        static bool Kill72Apply(SurvivalSim sim, string boss)
        {
            switch (boss)
            {
                case BossAttackClock63.Wyrm:
                    if (sim.wyrm24 != null && sim.wyrm24.Alive) { sim.wyrm24.Apply(sim.wyrm24.body.hp + 1); return true; }
                    break;
                case BossAttackClock63.Crown:
                    if (sim.crown37 != null && sim.crown37.Alive) { sim.crown37.Apply(sim.crown37.body.hp + 1, "window"); return true; }
                    break;
                case BossAttackClock63.Wick:
                    if (sim.wick37 != null && sim.wick37.Alive) { sim.wick37.Apply(sim.wick37.body.hp + 1); return true; }
                    break;
                case BossAttackClock63.Loom:
                    if (sim.loom37 != null && sim.loom37.Alive) { sim.loom37.Apply(sim.loom37.body.hp + 1); return true; }
                    break;
            }
            return false;
        }

        static IEnumerator BossKill72Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; });
            yield return Wick48Res(1920, 1080);
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED bosskill72: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "); boot reel.sh + -skygrindMap <level> + -skygrindReelOnly bosskill72"; yield break; }
            SurvivalSim sim = g.sim;
            LevelDef lv = sim.campaignLevel;
            RoomDef room = lv != null ? lv.rooms.Find(r => r.kind == "boss") : null;
            if (room == null) { res[0] = $"RED bosskill72: level {lv?.id ?? "?"} has no boss room"; yield break; }
            string boss = room.boss ?? "?";
            // the boss is not alive until the walk-in below starts it: the reel knows the health body of four of the five (wick, loom, wyrm, crown)
            if (boss != BossAttackClock63.Wyrm && boss != BossAttackClock63.Crown && boss != BossAttackClock63.Wick && boss != BossAttackClock63.Loom)
            { res[0] = $"SHORT bosskill72 {boss}: not built (the reel kills wick, loom, wyrm and crown; THE ICEBREAKER's death is Brawl40's own state machine)"; yield break; }
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            God(g);
            double wallEnd = Time.realtimeSinceStartup + 300;
            Action hold = () => { God(g); };
            yield return Wait27(g, 3.0, wallEnd, hold);
            g.UiView?.ClearBanners41();
            int rises = 0;
            double riseAt = -1, dyingAt = -1, killAt = -1, downAt = -1;
            int dyingF = -1, downF = -1; // NIGHT73: FeelProbe.FrameCount inside the sim step of the event's Update = the telemetry's / the JPG's frame f (EndFrame bumps it at the end of that frame's RenderFrame)
            offs.Add(g.events.On<BossEvent>("bossRise", e => { rises++; if (riseAt < 0) riseAt = FeelClock.Now; }));
            offs.Add(g.events.On<BossEvent>("bossDying", e => { if (dyingAt < 0) { dyingAt = FeelClock.Now; dyingF = FeelProbe.FrameCount; } }));
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e != null && e.kind == "boss" && killAt < 0) killAt = FeelClock.Now; }));
            offs.Add(g.events.On<BossEvent>("bossDown", e => { if (downAt < 0) { downAt = FeelClock.Now; downF = FeelProbe.FrameCount; } }));
            double yaw = room.spawnYawDeg * Math.PI / 180;
            Start(g, "bosskill72-" + boss, "camera", dir);
            yield return Frames(pre);
            g.Respawn(room.spawn + new Vec3(0, 0.05, 0), room.spawnYawDeg);
            g.input.SetLook(yaw, 0.05);
            g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
            FeelProbe.MarkInput();
            double t0 = FeelClock.Now;
            while (riseAt < 0 && FeelClock.Now - t0 < 3.0 && Time.realtimeSinceStartup < wallEnd) { God(g); yield return null; }
            if (riseAt < 0)
            {
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05, moveZ = 1 };
                t0 = FeelClock.Now;
                while (riseAt < 0 && FeelClock.Now - t0 < 4.0 && Time.realtimeSinceStartup < wallEnd) { God(g); yield return null; }
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
            }
            if (riseAt < 0) { sim.BossRoomBegin(boss, out Vec3 _, out double _); for (int k = 0; k < 30 && riseAt < 0; k++) yield return null; }
            if (riseAt < 0) { yield return Stop(); res[0] = $"RED bosskill72 {boss}: no bossRise | map {g.MapId}"; yield break; }

            var lines = new List<string>();
            void Say(string line) { lines.Add(line); Debug.Log(line); }
            bool f55On0 = NightRows.ViewOn(g.cfg, Finish55View.Row), k71On0 = Kicks71.On(g.cfg);
            bool forcedHp = false, forcedStagger = false, preShot = false, dyingShot = false, downShot = false, afterShot = false, applied = false;
            double hpBefore = -1, hpAfter = -1, forcedAtMs = -1, barPeak = 0;
            int frame = 0, dyingFrame = -1;
            Feel46Probe.Clear();
            Finish55View.KickPeak = Finish55View.FovPeak = 0;
            int beats0 = Finish55View.Beats;
            // NIGHT73 (the lead: "frame the boss kill so finish55 can be judged"; FeelReel.N73.Frame.cs): THE KILN WYRM only. From the forced health on, the camera is on the body (the first body piece behind the
            // head), the player stands where the body can be a third of the frame, and the killing taps wait for that (the gate); the same decision on ON and OFF (sim positions and the config's field of view)
            Wyrm fw = sim.wyrm24;
            Fin73 fin = boss == BossAttackClock63.Wyrm && fw != null ? new Fin73() : null;
            bool downBoxDone = false;
            while (FeelClock.Now - riseAt < UnityTuning.BossKill72MaxS && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                if (FeelClock.Holding) { yield return null; continue; }
                frame++;
                double ms = (FeelClock.Now - riseAt) * 1000;
                BossAttack63 a = BossAttackClock63.Read(sim, boss);
                bool fighting = a.Live && a.phase != BossPhase63.Reveal;
                Vec3 lookAt = fighting ? a.weak : a.centre;
                bool gateTap = true;
                if (fin != null && forcedHp && fighting)
                {
                    if (dyingAt < 0 && killAt < 0) lookAt = Fin73Step(g, sim, fin, fw, room, ms, forcedAtMs, frame, out gateTap);
                    else lookAt = Fin73Aim(fw);
                }
                if (a.Live) Look(g, lookAt);
                bool tap = fighting && gateTap && frame % Math.Max(1, UnityTuning.Boss63TapEvery) == 0;
                g.forcedCombat = new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = tap, pressed = tap } };
                // THE REEL'S ACTION: BossKill72LeadS into the fight the boss's health goes to its kill share and its stagger is forced (every hit lands)
                // THE WYRM has phases that put its health back at a threshold: step them down first (Wyrm72Descend), the kill share goes in its last phase
                bool lastPhase = true;
                if (!forcedHp && fighting && ms >= UnityTuning.BossKill72LeadS * 1000 && boss == BossAttackClock63.Wyrm && sim.wyrm24 != null && sim.wyrm24.Alive)
                    lastPhase = Wyrm72Descend(sim.wyrm24, ms, Say);
                if (!forcedHp && lastPhase && fighting && ms >= UnityTuning.BossKill72LeadS * 1000 && BossHp72(sim, boss, out double hp, out double max, out Action<double> set))
                {
                    forcedHp = true;
                    hpBefore = hp;
                    set(max * UnityTuning.BossKill72HpShare);
                    hpAfter = max * UnityTuning.BossKill72HpShare;
                    forcedAtMs = ms;
                    forcedStagger = ForceStagger63(sim, boss);
                    Say($"[bosskill72] {boss} hp {hpBefore.ToString("0.0", ci)} -> {hpAfter.ToString("0.0", ci)} at {ms.ToString("0", ci)} ms (forced), stagger forced {forcedStagger}");
                }
                else if (forcedHp && dyingAt < 0 && killAt < 0 && downAt < 0 && fighting && a.phase != BossPhase63.Stagger && frame % 120 == 0) forcedStagger |= ForceStagger63(sim, boss); // the Crown's window closes: open it again
                // the taps did not kill it (a part, an armour or a closed window refused them): the boss's own damage call for the last points, said
                // NIGHT73: with the wyrm's framing the fallback counts from the gate's arming (the taps were held until then), not from the forced health
                if (forcedHp && !applied && dyingAt < 0 && killAt < 0 && downAt < 0 && (fin == null || fin.armed) && ms - (fin != null && fin.armedMs >= 0 ? fin.armedMs : forcedAtMs) >= UnityTuning.BossKill72ApplyAfterS * 1000)
                {
                    applied = Kill72Apply(sim, boss);
                    Say($"[bosskill72] {boss} the taps did not kill it in {UnityTuning.BossKill72ApplyAfterS.ToString("0", ci)} s: the reel calls the boss's own Apply for the last points ({(applied ? "done" : "NOT DONE: no live boss body")}) at {ms.ToString("0", ci)} ms");
                }
                if (forcedHp && !preShot && dyingAt < 0 && killAt < 0 && downAt < 0 && a.Live && a.hp <= a.maxHp * (UnityTuning.BossKill72HpShare + 0.01)) { preShot = true; yield return Shot30(dir, "BK72-" + boss + "-pre"); }
                if (dyingAt >= 0 && dyingFrame < 0) dyingFrame = frame;
                if (!dyingShot && (dyingAt >= 0 || killAt >= 0) && frame >= (dyingFrame >= 0 ? dyingFrame : frame) + 2) { dyingShot = true; yield return Shot30(dir, "BK72-" + boss + "-dying"); }
                if (!downShot && downAt >= 0) { downShot = true; yield return Shot30(dir, "BK72-" + boss + "-down"); }
                barPeak = Math.Max(barPeak, Finish55View.BarK);
                if (fin != null && fin.begun && !fin.noCentre)
                {
                    // the body on screen at the dying beat, half a second in, and at bossDown (the real camera: what the strip will show)
                    if (dyingAt >= 0 && fin.sinceDying < 0) { fin.sinceDying = 0; fin.atDying = Fin73Measure(g, fw, dyingF); }
                    else if (fin.sinceDying >= 0 && ++fin.sinceDying == UnityTuning.Fin73HalfFrames) fin.atHalf = Fin73Measure(g, fw, dyingF + fin.sinceDying);
                    if (downAt >= 0 && !downBoxDone) { downBoxDone = true; fin.atDown = Fin73Measure(g, fw, downF); }
                }
                if (downAt >= 0 && !afterShot && FeelClock.Now - downAt >= UnityTuning.BossKill72AfterS) { afterShot = true; yield return Shot30(dir, "BK72-" + boss + "-after"); break; }
                yield return null;
            }
            g.forcedCombat = IdleCombat;
            yield return Frames(10);
            yield return Stop();
            string ms0(double t) => t < 0 ? "never" : ((t - riseAt) * 1000).ToString("0", ci);
            Say($"[bosskill72] {boss} kill: bossDying at {ms0(dyingAt)} ms, kill event (kind boss) at {ms0(killAt)} ms, bossDown at {ms0(downAt)} ms (ms from the rise, the reel's clock)");
            bool f55On = NightRows.ViewOn(g.cfg, Finish55View.Row), k71On = Kicks71.On(g.cfg);
            int beats = Finish55View.Beats - beats0;
            Say($"[bosskill72] {boss} feel: kicks71 {(k71On ? "on" : "off")}, kill kicks {Feel46Probe.KillKicks}, fov punches {Feel46Probe.FovPunches}, last kick {Feel46Probe.LastKickDeg.ToString("0.0", ci)} deg | finish55 {(f55On ? "on" : "off")}: beats {beats}, kick peak {Finish55View.KickPeak.ToString("0.00", ci)} deg, fov peak {Finish55View.FovPeak.ToString("0.00", ci)} deg, bars peak {barPeak.ToString("0.00", ci)}");
            // NIGHT73: what was framed (THE KILN WYRM only), and the frames to cut. FRAMED when the gate arrived by itself (not its timeout) and the body's box at the dying beat fills at least Fin73FramedMinShare of
            // the frame's width or height; the kill's own verdict below is unchanged.
            string framed = "n/a (" + boss + " is not framed)";
            if (fin != null)
            {
                Camera fcam = g.cam;
                int pw = fcam != null ? fcam.pixelWidth : 1920, ph = fcam != null ? fcam.pixelHeight : 1080;
                double share = fin.atDying.have ? Math.Max((double)fin.atDying.W / pw, (double)fin.atDying.H / ph) : 0;
                bool isFramed = fin.begun && !fin.noCentre && !fin.timedOut && fin.atDying.have && share >= UnityTuning.Fin73FramedMinShare;
                framed = (isFramed ? "FRAMED" : "NOT FRAMED") + $" (the body's larger side is {(share * 100).ToString("0", ci)} % of the frame at the dying beat; the bar is {(UnityTuning.Fin73FramedMinShare * 100).ToString("0", ci)} %)";
                string cut = dyingF >= 0
                    ? $"CUT the dying beat (frame f{dyingF}, bossDown at f{downF}): f{Math.Max(0, dyingF - UnityTuning.Fin73CutBefore)} through f{dyingF + UnityTuning.Fin73CutAfter}: python -I Tools/night/n73-strip.py strip <out>.png {dir} {Math.Max(0, dyingF - UnityTuning.Fin73CutBefore)} {dyingF + UnityTuning.Fin73CutAfter} 6 (the same numbers for the other side)"
                    : "CUT: no dying beat";
                string gate = !fin.begun ? "never begun (the forced health never came)"
                    : fin.armed ? $"armed at {fin.armedMs.ToString("0", ci)} ms (frame {fin.armedFrame}) after {fin.gateFrames} gate frames, {fin.bandFrames} of them in the band{(fin.why.Length > 0 ? "; " + fin.why : "")}"
                    : $"NEVER ARMED in {fin.gateFrames} gate frames (last distance {fin.lastD.ToString("0.0", ci)} m, miss {fin.lastMiss.ToString("0.0", ci)} m)";
                Say($"[bosskill72] {boss} framing: aim = the centre of the first body piece (the head and its next {UnityTuning.Fin73Pieces - 1} pieces, bounding radius {fin.rho.ToString("0.0", ci)} m), the player placed {fin.teleports}x at ({fin.vantage.x.ToString("0.0", ci)}, {fin.vantage.y.ToString("0.0", ci)}, {fin.vantage.z.ToString("0.0", ci)}) = {UnityTuning.Fin73VantageM.ToString("0", ci)} m from the arena centre toward the room's entry"
                    + $", target distance {fin.targetD.ToString("0.0", ci)} m for {(UnityTuning.Fin73FillShare * 100).ToString("0", ci)} % of the {fin.fovDeg.ToString("0", ci)} deg vertical field of view, band {(UnityTuning.Fin73BandLo * fin.targetD).ToString("0.0", ci)}..{(UnityTuning.Fin73BandHi * fin.targetD).ToString("0.0", ci)} m held {UnityTuning.Fin73HoldFrames} frames | gate: {gate}"
                    + $" | at arming: {Fin73BoxText(g, fin.atArm)} | AT THE DYING BEAT (f{dyingF}): {Fin73BoxText(g, fin.atDying)} | half a second on (f{(dyingF >= 0 ? dyingF + UnityTuning.Fin73HalfFrames : -1)}): {Fin73BoxText(g, fin.atHalf)} | at bossDown (f{downF}): {Fin73BoxText(g, fin.atDown)} | {framed}");
                Say($"[bosskill72] {boss} {cut}");
            }
            var bad = new List<string>();
            if (!forcedHp) bad.Add("the boss's health was never set (no live boss body)");
            if (downAt < 0 && dyingAt < 0 && killAt < 0) bad.Add("the boss did not die in " + UnityTuning.BossKill72MaxS.ToString("0", ci) + " s");
            if (f55On)
            {
                if (beats < 1) bad.Add("finish55 on but no beat began");
                if (Finish55View.KickPeak < UnityTuning.Finish55KickDeg * 0.5) bad.Add($"finish55's kick peak {Finish55View.KickPeak.ToString("0.00", ci)} deg under half of {UnityTuning.Finish55KickDeg.ToString("0.0", ci)}");
                if (barPeak < 0.99) bad.Add($"the bars peaked at {barPeak.ToString("0.00", ci)} of full");
            }
            else if (Finish55View.KickPeak > 0 || barPeak > 0) bad.Add("finish55 off but a kick or the bars were drawn");
            string verdict = bad.Count > 0 ? "RED" : f55On ? "GREEN" : "OFF";
            string summary = $"{verdict} bosskill72 {boss} | rise at 0, hp {(forcedHp ? hpBefore.ToString("0", ci) + " -> " + hpAfter.ToString("0", ci) : "not set")} at {forcedAtMs.ToString("0", ci)} ms, dying {ms0(dyingAt)} / kill {ms0(killAt)} / down {ms0(downAt)} ms"
                + $" | death by {(applied ? "the reel's Apply (the taps did not kill it)" : "the taps")}"
                + $" | finish55 {(f55On ? "on" : "OFF (the pair's other half)")} (at the start {(f55On0 ? "on" : "off")}), kicks71 {(k71On ? "on" : "off")} (at the start {(k71On0 ? "on" : "off")}) | frames pre {(preShot ? "yes" : "no")}, dying {(dyingShot ? "yes" : "no")}, down {(downShot ? "yes" : "no")}, after {(afterShot ? "yes" : "no")}"
                + $" | framing {framed}"
                + $" | map {g.MapId}, room {room.id}{(bad.Count > 0 ? " | BAD: " + string.Join("; ", bad) : "")}";
            lines.Add(summary);
            try
            {
                Directory.CreateDirectory(dir);
                File.WriteAllLines(Path.Combine(dir, "bosskill72-" + boss + ".txt"), lines);
            }
            catch (Exception) { }
            res[0] = summary;
        }
    }
}
