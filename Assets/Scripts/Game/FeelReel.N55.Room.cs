// NIGHT55 builder R (P2-clear; design.n55.clear55, a view row): the a455 phase (FeelReel.N55.cs routes it here): the room-clear beat's Player
// proof. Not run by its author (builders never run a Player). A campaign level boots only via reel.sh (L-062), e.g.:
//   bash Tools/night/n47-reels.sh <tag> c55on,n52-iceline,a455                      (the build copy's config with design.n55.clear55 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> c55off,n52-iceline,a455,-skygrindOff55      (the row off, NIGHT54's game: RED)
// (n52-foundry, n52-kiln, n52-span, n52-spire the same.) A campaign run, DOORFREE (as a452), the player kept alive, nothing pressed. For the
// first UnityTuning.Clear55ReelRooms combat rooms (kind room) with a frames52 entry spot, in room order: the camera at the entry spot, the
// campaign in that room, then a KillAll sweep every Clear55ReelKillEvery s of sim time (the waves, their spawn tells, a lesson's top-ups come
// in and die) until the bus's `roomClear` for that room (the input: the room's last enemy dead; at most Clear55ReelRoomMaxS). Then, every
// frame from the clear: the loop's held frames (SurvivalGame.HeldThisFrame) until the world runs again, the sim time across them (it must not
// move), the hold served on the loop's own clock (Clear55.lastServed), the holds started (exactly one per clear); a frame mid-hold
// (Clear55ReelShotAtFrame held frames in) and one of the card Clear55ReelCardShotS after the hold; the card's two lines (Ui.Clear55TextNow),
// its on-screen alpha and its smallest font px. The KillAll sweep has no kill of the player's, so no kill freeze shares the hold: it is the
// beat's alone. Each room prints that its input fired (the roomClear: frame, the room's time and par, the par's source), then its numbers.
// GREEN: the row on, a room cleared, and every cleared room: one hold, served in [Clear55ReelHoldMin, Clear55ReelHoldMax] s, no sim time
// during it, the card's text carrying the room's time and its par, its alpha >= Clear55ReelMinAlpha, its text >= 16 px. RED otherwise (the
// row off: no hold, no card: NIGHT54's game). One line "done: GREEN a455 CLEAR | ..." (or RED); each room's full line in the log as "[a455] ...".
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
        static partial void N55Room(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Clear55Scenario(g, dir, res, offs, pre);

        static IEnumerator Clear55Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with reel.sh + -skygrindMap <id> -skygrindReelOnly a455, design.n55.clear55 on in the build copy (L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED a455: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            bool rowOn = Clear55.On(g.cfg);
            Clear55 beat = g.Clear55Beat; // null with the row off
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var rows = Levels35Rows(g);
            RoomEvent seen = null;
            int seenFrame = -1, clearEvents = 0;
            offs.Add(g.events.On<RoomEvent>("roomClear", e =>
            {
                clearEvents++;
                if (Clear55.Counts(e)) { seen = e; seenFrame = Time.frameCount; }
            }));
            Debug.Log($"[a455] start: map {g.MapId}, level {id}, row clear55 {(rowOn ? "ON" : "OFF")}, hold {(beat != null ? beat.holdS.ToString("0.00", ci) : "-")} s (design.n55.clear55.numbers.holdS)");
            var st = new double[4];
            var lines = new List<string>();
            var shots = new List<string>();
            int tried = 0, cleared = 0, good = 0;
            string ended = null;
            foreach (RoomDef r in lv.rooms)
            {
                if (tried >= UnityTuning.Clear55ReelRooms) break;
                if (r.kind != "room") continue;
                double[] fe = K52Frame(rows, r.id, "entry");
                if (fe == null) continue;
                tried++;
                g.husks.KillAll();
                yield return Frames(4);
                K52Place(g, fe);
                float end = Time.realtimeSinceStartup + 6;
                while (K52Room(g) != r.id && Time.realtimeSinceStartup < end) { God(g); yield return null; }
                g.UiView?.ClearBanners41();
                seen = null;
                double t0 = sim.SimTime, lastSweep = double.NegativeInfinity;
                float realEnd = Time.realtimeSinceStartup + (float)UnityTuning.Clear55ReelRoomMaxS * 2f;
                while (!(seen != null && seen.id == r.id) && sim.SimTime - t0 < UnityTuning.Clear55ReelRoomMaxS && Time.realtimeSinceStartup < realEnd && (ended = Ended30(g)) == null)
                {
                    God(g);
                    if (sim.SimTime - lastSweep >= UnityTuning.Clear55ReelKillEvery) { g.husks.KillAll(); lastSweep = sim.SimTime; }
                    yield return null;
                }
                if (ended != null) break;
                if (seen == null || seen.id != r.id)
                {
                    lines.Add($"{r.id}: NO roomClear in {UnityTuning.Clear55ReelRoomMaxS.ToString("0", ci)} s of sim time (campaign room {K52Room(g)}): the input never fired");
                    continue;
                }
                cleared++;
                RoomEvent ev = seen;
                int clearFrame = seenFrame;
                int holdsBefore = beat != null ? beat.holds : 0; // the hold starts on the next frame's loop line (Clear55Frame)
                // the hold: every frame from the clear's until the world runs again
                int held = 0, framesAfter = 0;
                double heldReal = 0, simAtHold = double.NaN;
                bool simMoved = false;
                string holdShot = null;
                float holdEnd = Time.realtimeSinceStartup + 3f;
                while (Time.realtimeSinceStartup < holdEnd)
                {
                    yield return null;
                    God(g);
                    framesAfter++;
                    if (g.HeldThisFrame)
                    {
                        if (held == 0) simAtHold = sim.SimTime;
                        else if (sim.SimTime != simAtHold) simMoved = true;
                        held++;
                        heldReal += Time.unscaledDeltaTime;
                        if (held == UnityTuning.Clear55ReelShotAtFrame && holdShot == null)
                        {
                            string hn = $"C55-{id}-{r.id}-hold";
                            yield return Shot29(dir, hn, st);
                            holdShot = "shot-N29-" + hn + ".png";
                            Debug.Log($"[reel] a455: {r.id} hold {holdShot}");
                            shots.Add($"{ShotLine29(hn, st, ci)}, card '{Ui.Clear55TextNow}', card alpha {Ui.Clear55Alpha.ToString("0.00", ci)}");
                        }
                    }
                    else if (held > 0) break; // the world runs again
                    else if (framesAfter > 4) break; // no hold began (the row off)
                }
                int holdsNow = beat != null ? beat.holds : 0;
                double served = beat != null ? beat.lastServed : 0;
                // the card, after the hold (still up)
                float cardAt = Time.realtimeSinceStartup + (float)UnityTuning.Clear55ReelCardShotS;
                while (Time.realtimeSinceStartup < cardAt) { God(g); yield return null; }
                string cardText = Ui.Clear55TextNow ?? "";
                float cardAlpha = Ui.Clear55Alpha, cardPx = Ui.Clear55MinPx;
                string cn = $"C55-{id}-{r.id}-card";
                yield return Shot29(dir, cn, st);
                string cardShot = "shot-N29-" + cn + ".png";
                Debug.Log($"[reel] a455: {r.id} card {cardShot}");
                shots.Add($"{ShotLine29(cn, st, ci)}, card '{cardText}', card alpha {cardAlpha.ToString("0.00", ci)}");
                bool textOk = cardText.Contains(Clear55.Seconds(ev.time)) && (ev.par <= 0 || cardText.Contains("PAR " + Clear55.Par(ev.par)));
                bool roomGood = rowOn && holdsNow - holdsBefore == 1 && held > 0 && !simMoved
                    && served >= UnityTuning.Clear55ReelHoldMin && served <= UnityTuning.Clear55ReelHoldMax
                    && textOk && cardAlpha >= UnityTuning.Clear55ReelMinAlpha && cardPx >= 16f;
                if (roomGood) good++;
                string line = $"{r.id} {(roomGood ? "GREEN" : "RED")}: input roomClear fired (the room's last enemy dead) at frame {clearFrame}, room time {ev.time.ToString("0.00", ci)} s, par {ev.par.ToString("0.##", ci)} s"
                    + $" (par source: {id}.json campaign.rooms[{r.id}].par, the plan's room row, via RoomEvent.par), within par {Clear55.Within(ev.time, ev.par)}"
                    + $" | hold served {served.ToString("0.000", ci)} s on the loop's clock (band {UnityTuning.Clear55ReelHoldMin.ToString("0.0", ci)}-{UnityTuning.Clear55ReelHoldMax.ToString("0.0", ci)}),"
                    + $" {held} held frames ({heldReal.ToString("0.000", ci)} s real, the mid-hold shot's frame included), holds started {holdsNow - holdsBefore} (one per clear), sim time moved during the hold {simMoved}"
                    + $" | card '{cardText}' (text has the time and par {textOk}), alpha {cardAlpha.ToString("0.00", ci)}, min font {cardPx.ToString("0", ci)} px | frames {holdShot ?? "(none: no hold)"}, {cardShot}";
                Debug.Log("[a455] " + line);
                lines.Add(line);
            }
            g.husks.KillAll();
            g.forcedMove = null;
            bool green = rowOn && cleared > 0 && good == cleared && ended == null;
            res[0] = $"done: {(green ? "GREEN" : "RED")} a455 CLEAR | map {g.MapId}, level {id} | row clear55 {(rowOn ? "ON" : "OFF (NIGHT54: no clear beat)")}"
                + $" | combat rooms tried {tried}, cleared {cleared}, GREEN {good}; roomClear events {clearEvents}; holds {(beat != null ? beat.holds : 0)}, cards {Ui.Clear55Cards}"
                + $" | screen {Screen.width}x{Screen.height}{(ended != null ? " | ended: " + ended : "")}"
                + (tried == 0 ? " | no combat room with a frames52 entry (nothing to clear)" : "")
                + " || " + string.Join(" || ", lines) + " || " + string.Join(" || ", shots);
        }
    }
}
