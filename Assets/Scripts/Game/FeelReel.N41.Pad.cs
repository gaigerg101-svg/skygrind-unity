// NIGHT41 builder T2 (INVENTORY C1 / A14; NIGHT41.md card T2): the Player scenario `pad41`: every NIGHT40 line with a {token} framed while the PAD
// is the last device, so C1 can see which lines show pad glyphs and which show keyboard names ("L-SHIFT", "LEFT CLICK", "G" in NIGHT40's frames:
// the reels never set PlayerDevices.Last to Pad). Run it with -skygrindReelPad (FeelReel.PadPass); the reel also holds PlayerDevices.Last = Pad
// every frame. Boot one Player per level (reel.sh + -skygrindMap <level> + -skygrindReelOnly pad41 + -skygrindReelPad). Not run by its author.
// The run: a CAMPAIGN run, DOORFREE on and its banners cleared (A9); every room of the level in order: the husks killed, the player put at the room's
// spawn (the campaign enters it: its lesson line; a boss room begins its fight: its BossLine40 line), watched 3.5 s (a boss room 6 s); while new lines
// keep coming the room is watched on, up to 40 s (level 3's BOOST 1/5 .. 5/5 chain moves on by itself after each line's own seconds).
// Each NEW line (Ui.Lesson25TextNow, Ui.Teach35TextNow, Crown37View.Crown40Call) gets a frame 0.4 s after it appeared (its fade-in done):
//   P41-<room index>-<room id>-<n>   (shot-N30-*.png), e.g. P41-0-i1-0 = level 2's first room's lesson "{fire}: ONE SWING FELLS A TANK".
// The line lists every text seen and FLAGS any that still reads as a keyboard ("CLICK", "SHIFT", "SPACE", "MOUSE", "CTRL", "LMB", "RMB", a lone
// E / F / G / Q / R / C / V / X / Z key letter, or an unresolved {token}). The THUNDERCLAP / KESTREL crosshair spin on the pad (A14) is in every frame's
// telemetry (`spin`, `device`).
// Line: GREEN when at least one line was framed, every frame recorded `device` pad, and no text was flagged.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static readonly Regex Kbm41 = new Regex(@"CLICK|SHIFT|SPACE|MOUSE|CTRL|\bLMB\b|\bRMB\b|\bTAB\b|(?<![A-Z<=/])\b[EFGQRCVXZ]\b(?![A-Za-z=>])|\{[a-zA-Z]+\}", RegexOptions.Compiled);

        static IEnumerator Pad41Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            PlayerDevices.Last = InputDevice.Pad;
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED pad41: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            SurvivalSim sim = g.sim;
            LevelDef lv = sim.campaignLevel;
            g.forcedCombat = IdleCombat;
            God(g);
            double wallEnd = Time.realtimeSinceStartup + 600;
            var seen = new List<string>();
            var flagged = new List<string>();
            int saved = 0, notPad = 0;
            string last = "";
            Start(g, "pad41", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            string Now()
            {
                // the lines a pad player reads now: the lesson line, the teaching slot, THE CROWN's call (rich-text tags stripped for the flag check only)
                string a = Ui.Lesson25Visible ? Ui.Lesson25TextNow : "", b = Ui.Teach35TextNow ?? "", c = Crown37View.Crown40Call ?? "";
                var parts = new List<string>(3);
                if (!string.IsNullOrEmpty(a)) parts.Add(a);
                if (!string.IsNullOrEmpty(b) && b != a) parts.Add(b);
                if (!string.IsNullOrEmpty(c)) parts.Add(c);
                return string.Join(" | ", parts);
            }
            for (int ri = 0; ri < lv.rooms.Count && Time.realtimeSinceStartup < wallEnd && saved < 60; ri++)
            {
                RoomDef room = lv.rooms[ri];
                g.husks.KillAll();
                g.Respawn(room.spawn + new Vec3(0, 0.05, 0), room.spawnYawDeg);
                double yaw = room.spawnYawDeg * Math.PI / 180;
                g.input.SetLook(yaw, 0.05);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = 0.05 };
                double watch = room.kind == "boss" ? 6.0 : 3.5;
                double t0 = Time.realtimeSinceStartup, lastNew = t0;
                int n = 0;
                while (Time.realtimeSinceStartup < wallEnd && saved < 60)
                {
                    double now = Time.realtimeSinceStartup;
                    if (now - t0 > 40 || (now - t0 > watch && now - lastNew > 6.0)) break;
                    PlayerDevices.Last = InputDevice.Pad;
                    God(g);
                    if (room.kind != "boss") g.husks.KillAll(); // a lesson room's wave never covers the line; a boss is left to begin
                    string s = Now();
                    if (s.Length > 0 && s != last)
                    {
                        last = s;
                        lastNew = now;
                        double tIn = now;
                        while (Time.realtimeSinceStartup - tIn < 0.4) { PlayerDevices.Last = InputDevice.Pad; God(g); yield return null; }
                        string shown = Now();
                        if (shown.Length == 0) shown = s;
                        if (PlayerDevices.Last != InputDevice.Pad) notPad++;
                        string name = $"P41-{ri}-{room.id}-{n++}";
                        yield return Shot30(dir, name);
                        saved++;
                        string plain = Regex.Replace(shown, "<[^>]*>", " ");
                        seen.Add($"{name}: '{shown}'");
                        if (Kbm41.IsMatch(plain)) flagged.Add($"{name}: '{plain.Trim()}'");
                        continue;
                    }
                    yield return null;
                }
            }
            yield return Stop();
            bool green = saved > 0 && notPad == 0 && flagged.Count == 0;
            res[0] = $"{(green ? "GREEN" : "RED")} pad41 | map {g.MapId}, level {lv.id}, {lv.rooms.Count} rooms, PadPass {PadPass}, device pad at every frame {notPad == 0} | lines framed {saved}"
                + $" | FLAGGED (keyboard names / unresolved tokens) {flagged.Count}: {string.Join(" ; ", flagged)} || seen: {string.Join(" || ", seen)}";
        }
    }
}
