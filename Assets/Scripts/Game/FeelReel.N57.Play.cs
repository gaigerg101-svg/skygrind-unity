// NIGHT57 builder G: the a257 phase (FeelReel.N57.cs routes it here): G2 press57's Player proof (the tell on a frame, and that it fired). Not run
// by its author (builders never run a Player). A campaign level (L-062):
//   bash Tools/night/n47-reels.sh <tag> g2on,n52-foundry,a257                    (the build copy's config with design.n57.press57 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> g2off,n52-foundry,a257,-skygrindOff57    (the row off, NIGHT56's spitter: RED, no circle)
// The CAMPAIGN on the booted map, DOORFREE, God (hurts still fire: the health is only made large), the level's enemies killed, standing at a
// non-boss room's spawn facing its clearest line (a256's spot finder). ONE spitter is spawned Press57ReelAhead m ahead on that line (inside its
// 6-24 m band), its brain on (it takes the ranged token and throws on its own).
//   1. STILL: the player stands, looking Press57ReelPitchDeg down the line (the floor round the feet and the spitter both in the frame). On the
//      spitter's commit press57 marks the spot under the feet (the telegraph's red-orange circle, filling): shot-N57-G-tell.jpg
//      Press57ReelShotS after the mark, shot-N57-G-strike.jpg on the frame its globs land. The spitter's globs ("spitter") and acid ("acid")
//      that hurt the player from the mark to Press57ReelAfterS after the landing are counted: a still player is hit.
//   2. MOVING: on the next mark the player backs away from the spitter (the stick held back, Press57ReelMoveFrames frames) out of the circle,
//      so the circle stays ahead in the frame: shot-N57-G-moved.jpg on the landing frame (the globs into the empty circle); its glob hits
//      counted the same way (a moving player is not hit by the globs).
// Printed first (L-089): the marks laid (press57's counter), the volleys thrown at a mark, each mark's circle life (the tell + the flight, s:
// E-4 wants >= 0.8 s), the hurts per phase. GREEN: the row on, 2 marks, both circles >= 0.8 s, the still phase hit, the moving phase's globs
// 0. RED otherwise (the row off: no mark, "marked 0"). One line "GREEN a257 PLAY | ..." (or RED), the full one in the log as "[a257] ...".
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
        static partial void N57Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Press57Scenario(g, dir, res, offs, pre);

        /// <summary>Saves the screen to dir/shot-N57-G-name.jpg. Yields to the frame's end.</summary>
        static IEnumerator G57Shot(string dir, string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = ScreenCapture.CaptureScreenshotAsTexture();
            if (t == null) yield break;
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "shot-N57-G-" + name + ".jpg"), t.EncodeToJPG(85));
            UnityEngine.Object.Destroy(t);
        }

        static IEnumerator Press57Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string F2(double v) => v.ToString("0.00", ci);
            res[0] = "RED a257 PLAY: began, the spitter not reached";
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var spawned = new List<Husk>();
            offs.Add(() =>
            {
                foreach (Husk h in spawned) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a257 PLAY: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 240;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 220) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            sim.husks.KillAll();
            bool on = sim.Press57On;
            string map = PostFx.LevelIdOf(g.map.json);

            // ---- the spot: a non-boss room's spawn with the clearest heading (a256's) ----
            LevelDef lv = sim.campaignLevel;
            Vec3 f0 = g.Feet, u = new Vec3(0, 0, -1);
            double bestLen = -1, need = UnityTuning.Press57ReelAhead + 2;
            if (lv != null)
                foreach (RoomDef r in lv.rooms)
                {
                    if (r.kind == "boss") continue;
                    Vec3 s0 = r.spawn;
                    if (!Move54Floor(g.world, s0.x, s0.z, s0.y + 1.5, out double sy, out _)) continue;
                    s0 = new Vec3(s0.x, sy, s0.z);
                    for (int k = 0; k < UnityTuning.Move54ReelHeadings; k++)
                    {
                        double a = 2 * Math.PI * k / UnityTuning.Move54ReelHeadings;
                        var uk = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                        double len = Move54Line(g.world, s0, uk, out _, out _);
                        if (len > bestLen) { bestLen = len; f0 = s0; u = uk; }
                    }
                    if (bestLen >= need) break;
                }
            if (bestLen < need) { res[0] = $"RED a257 PLAY: no clear line of {need.ToString("0", ci)} m from any room's spawn on {map}"; yield break; }
            double yawU = Math.Atan2(-u.x, -u.z), pitch = UnityTuning.Press57ReelPitchDeg * Math.PI / 180;
            string pc = Preconditions(g, f0);
            g.Respawn(new Vec3(f0.x, f0.y + 0.05, f0.z), yawU * 180 / Math.PI);
            for (int i = 0; i < 40; i++) { God(g); g.forcedMove = new InputFrame(); g.input.SetLook(yawU, pitch); yield return null; }

            // the spitter ahead, its brain on
            double sx = f0.x + u.x * UnityTuning.Press57ReelAhead, sz = f0.z + u.z * UnityTuning.Press57ReelAhead;
            double syy = Move54Floor(g.world, sx, sz, f0.y + 2, out double fy, out _) ? fy : f0.y;
            Husk sp = null;
            try { sp = sim.husks.Spawn("spitter", new Vec3(sx, syy, sz), f0); } catch (Exception) { }
            if (sp == null) { res[0] = $"RED a257 PLAY: no spitter spawned on {map}"; yield break; }
            spawned.Add(sp);
            g.husks.frozen = false;

            // the hurts by phase
            int phase = 0, still = 0, stillAcid = 0, moving = 0, movingAcid = 0;
            offs.Add(g.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner != "player" || phase == 0) return;
                bool glob = e.source == "spitter", acid = e.source == "acid";
                if (phase == 1) { if (glob) still++; if (acid) stillAcid++; }
                if (phase == 2) { if (glob) moving++; if (acid) movingAcid++; }
            }));
            SpitterAcid sa = sim.husks.acid;
            int marked0 = sa.marked57, struck0 = sa.struck57;
            Start(g, "a257-press", "tell", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // waits for the next mark (a new alive, unthrown mark), the player held by `move`; returns it through m[0]
            var lives = new List<double>();
            var shots = new List<string>();
            SpitterAcid.Mark57 Newest()
            {
                if (sa.marks57 == null) return null;
                SpitterAcid.Mark57 best = null;
                foreach (SpitterAcid.Mark57 m in sa.marks57) if (m.alive && !m.thrown && m.owner == sp && (best == null || m.age < best.age)) best = m;
                return best;
            }
            IEnumerator Phase(int ph, string shotTell, string shotLand, SpitterAcid.Mark57[] got)
            {
                int marks = sa.marked57;
                for (int i = 0; i < UnityTuning.Press57ReelWaitFrames && sa.marked57 == marks && sp.alive; i++)
                {
                    God(g);
                    g.forcedMove = new InputFrame();
                    g.input.SetLook(yawU, pitch);
                    yield return null;
                }
                SpitterAcid.Mark57 m = Newest();
                if (m == null) yield break;
                got[0] = m;
                phase = ph;
                double life = m.land;
                lives.Add(life);
                bool shotT = false;
                int moveLeft = ph == 2 ? UnityTuning.Press57ReelMoveFrames : 0;
                for (int i = 0; i < UnityTuning.Press57ReelWaitFrames && m.alive; i++)
                {
                    God(g);
                    var mv = new InputFrame();
                    if (moveLeft > 0) { mv.moveZ = -1; moveLeft--; } // back away along the line: the circle stays ahead, in the frame
                    g.forcedMove = mv;
                    g.input.SetLook(yawU, pitch);
                    if (!m.thrown) life = m.land; // the throw re-times the landing (the flight to the spot)
                    if (!shotT && shotTell != null && m.age >= UnityTuning.Press57ReelShotS) { shotT = true; yield return G57Shot(dir, shotTell); shots.Add(shotTell); }
                    else yield return null;
                }
                lives[lives.Count - 1] = life;
                yield return G57Shot(dir, shotLand); // the landing frame (the mark ended this step: its globs are down)
                shots.Add(shotLand);
                for (int i = 0; i < UnityTuning.Press57ReelAfterFrames; i++) { God(g); g.forcedMove = new InputFrame(); g.input.SetLook(yawU, pitch); yield return null; }
                phase = 0;
            }

            var m1 = new SpitterAcid.Mark57[1];
            var m2 = new SpitterAcid.Mark57[1];
            yield return Phase(1, "tell", "strike", m1);
            if (m1[0] != null) yield return Phase(2, null, "moved", m2);
            g.forcedMove = new InputFrame();
            God(g);
            yield return Frames(5);
            yield return Stop();

            // ---- the numbers ----
            int marked = sa.marked57 - marked0, struck = sa.struck57 - struck0;
            bool tellOk = lives.Count >= 2 && lives.TrueForAll(l => l >= 0.8);
            bool stillHit = still + stillAcid > 0, movedClear = m2[0] != null && moving == 0;
            string input = $"input: a spitter {UnityTuning.Press57ReelAhead.ToString("0", ci)} m ahead (brain on), the player still, then backing away; marks laid {marked}, volleys thrown at a mark {struck}";
            string nums = $"circle life (the tell + the flight) {(lives.Count > 0 ? string.Join(" / ", lives.ConvertAll(l => F2(l) + " s")) : "none")} (E-4 >= 0.80 s); still: globs {still}, acid ticks {stillAcid}; moving: globs {moving}, acid ticks {movingAcid}; frames {(shots.Count > 0 ? string.Join(", ", shots.ConvertAll(s => "shot-N57-G-" + s + ".jpg")) : "none")}";
            bool ok = on && marked >= 2 && tellOk && stillHit && movedClear;
            string verdict = $"marked {(marked >= 2 ? "ok" : "NO")}, tell >= 0.8 s {(tellOk ? "ok" : "NO")}, still hit {(stillHit ? "ok" : "NO")}, moving clear of the globs {(movedClear ? "ok" : "NO")}";
            Debug.Log($"[a257] press57 on {on}; map {map}; {input}; {nums}; {verdict}; {pc}");
            res[0] = $"{(ok ? "GREEN" : "RED")} a257 PLAY | press57 on {on} (map {map}) | {input} | {nums} | {verdict} | {pc}";
        }
    }
}
