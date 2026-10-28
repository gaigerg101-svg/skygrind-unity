// NIGHT30 builder V: the Player scenarios of the view rows (design.n30.hud30 / look30). The lead's FeelReel.N27.cs dispatches PhaseN30V here.
//   menus30      P0 c: the run's rows (Hud29's GAP bar + DOORS OPEN tag, the lesson line) are put up while playing, then the REAL pause (a virtual pad's
//                START, PadReader's pause command), TITLE (g.ToTitle, the title's own path) and RESULTS (the player dies; DOWN plays out). Under each
//                menu it counts the run rows DRAWN (read from the scene: the Lesson25 object's scale / alpha / active, Hud29's canvas), wants 0, and
//                saves a frame. With the row off (-skygrindOff30) the rows stay drawn: the RED frame.
//   layout30     P0 e / f: Hud30Layout.Report (every text x text, text x icon, chip x plate collision of the live HUD) in four HUD states: at rest with the
//                pistol, the gauntlet in hand with a full glove (NIGHT29's "PULSE READY" state), airborne (the "x1.2 AIR" chip), and the REFUND chip's
//                state (a refund power-up granted). Frames saved per state.
//   weaponhud30  item 2: the widget's state per slot through the REAL inputs (a virtual pad: RB cycles the weapon, RT fires): every slot READY at rest;
//                the THUNDERCLAP fired -> its slot COOLING (a draining ring, seconds) then READY (the word, a READY turn counted); RB inside the shot's
//                cycle -> SWITCHING on the queued slot (NIGHT29's lockout); RT held on the pistol -> CHARGING; the gauntlet's BLAST slot "0/5", then
//                pips set by the reel (2, then 5: the view's read, the kill path is pulse27's own reel) -> "2/5" -> READY, then its cooldown set ->
//                COOLING with seconds. Frames saved per state.
//   look30       item 8: three rooms of the loaded level (the first, the middle and the last fight room: respawned at each room's spawn, enemies frozen)
//                each saved as a frame with its mean luminance and the share of near-white pixels on the lower third (the floor panels that light
//                nothing, GG frame 170047); then three HUD states in the first room: at rest, a hit taken (the vitals glow + flicker), under 25 %
//                health (the buzz). Reads Ui.Fx30* and Crt29.QuantStrength. Run once with -skygrindOff30 (the BEFORE frames: the line is RED "look30
//                off") and once with the row on (AFTER).
// Every loop is bounded by frames and a wall clock. Results start with PASS or RED and the reason, then the numbers.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N30ViewBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "menus30") body = Menus30Scenario(g, dir, res, offs, pre);
            else if (s == "layout30") body = Layout30Scenario(g, dir, res, offs, pre);
            else if (s == "weaponhud30") body = WeaponHud30Scenario(g, dir, res, offs, pre);
            else if (s == "look30") body = Look30Scenario(g, dir, res, offs, pre);
        }

        /// <summary>A virtual pad for the NIGHT30 view scenarios: buttons and triggers as a player's pad sends them (PadReader reads it).</summary>
        sealed class Pad30
        {
            public Gamepad pad;
            public GamepadState st;

            public void Send() => InputSystem.QueueStateEvent(pad, st);

            public void Button(GamepadButton b, bool down)
            {
                st = st.WithButton(b, down);
                Send();
            }

            public void Rt(float v)
            {
                st.rightTrigger = v;
                Send();
            }

            public void Lt(float v)
            {
                st.leftTrigger = v;
                Send();
            }
        }

        static Pad30 NewPad30(List<Action> offs)
        {
            var p = new Pad30 { pad = InputSystem.AddDevice<Gamepad>("ReelPadN30") };
            offs.Add(() =>
            {
                try
                {
                    InputSystem.QueueStateEvent(p.pad, new GamepadState());
                    InputSystem.RemoveDevice(p.pad);
                }
                catch (Exception) { /* the Player is closing */ }
            });
            return p;
        }

        static IEnumerator Tap30(Pad30 pad, GamepadButton b)
        {
            pad.Button(b, true);
            yield return Frames(3);
            pad.Button(b, false);
            yield return Frames(3);
        }

        /// <summary>Saves the screen to dir/shot-N30-name.png (the end of this frame).</summary>
        static IEnumerator Shot30(string dir, string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N30-" + name + ".png"), t.EncodeToPNG());
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
            yield return SameRun63(dir, "shot-N30-" + name + ".png"); // NIGHT63 D1: with -skygrindSameRun63, the row-off half into <side>-off/ (FeelReel.N63Same.cs); nothing without it
        }

        // ======================================================= menus30 =======================================================

        /// <summary>Run rows drawn now, read from the scene (not from Hud30's statics: the off path never writes them).</summary>
        static int RunRowsDrawn30(Transform lesson, out string which)
        {
            int n = 0;
            which = "";
            if (lesson != null && lesson.gameObject.activeInHierarchy && lesson.lossyScale.x > 1e-4f)
            {
                var cg = lesson.GetComponent<CanvasGroup>();
                if (cg == null || cg.alpha > 0.01f) { n++; which += "lesson "; }
            }
            if (Hud29View.DrawnNow) { n++; which += "gap/doors "; }
            return n;
        }

        static void PutRunRows30(SurvivalGame g)
        {
            Hud29.ReelForce = true; // the sim's own writers reset DoorsOpenTag / the gap each frame (round 1: only the lesson was up): forced through Hud29's state
            Hud29.DoorsOpenTag = true;
            Hud29.BossGapMetres = 20;
            g.UiView?.ShowLesson25("MENUS30: A RUN ROW");
        }

        static IEnumerator Menus30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string pc = Preconditions(g, g.Feet);
            offs.Add(() => { Hud29.ReelForce = false; Hud29.Reset(); g.UiView?.HideLesson25(); Quiet(g); });
            Pad30 pad = NewPad30(offs);
            var fails = new List<string>();
            var lines = new List<string>();
            double wallEnd = Time.realtimeSinceStartup + 120;
            God(g);
            PutRunRows30(g);
            yield return Frames(40); // the lesson fades in
            Transform lesson = null;
            var lgo = GameObject.Find("Lesson25");
            if (lgo != null) lesson = lgo.transform;
            Start(g, "menus30", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int playing = RunRowsDrawn30(lesson, out string wPlay);
            lines.Add($"PLAYING: {playing} run row(s) drawn ({wPlay.Trim()})");
            if (playing < 2) fails.Add($"only {playing} run row(s) drawn while playing (the scenario wants the lesson and Hud29's rows up first)");
            yield return Shot30(dir, "menus30-playing");

            // the REAL pause: the pad's START (PadReader's pause command)
            yield return Tap30(pad, GamepadButton.Start);
            int n = 0;
            while (g.flow != Flow.Paused && n++ < 60) yield return null;
            yield return Frames(10);
            int paused = RunRowsDrawn30(lesson, out string wPause);
            lines.Add($"PAUSE (pad START, flow {g.flow}): {paused} drawn ({wPause.Trim()})");
            if (g.flow != Flow.Paused) fails.Add($"the pad's START did not pause (flow {g.flow})");
            else if (paused != 0) fails.Add($"PAUSE: {paused} run row(s) drawn over the menu ({wPause.Trim()})");
            yield return Shot30(dir, "menus30-pause");
            g.Resume();
            yield return Frames(20);
            int back = RunRowsDrawn30(lesson, out string wBack);
            lines.Add($"RESUMED: {back} drawn ({wBack.Trim()})");
            if (Hud30.On && back < 2) fails.Add($"after the pause only {back} run row(s) came back ({wBack.Trim()})");

            // RESULTS: the player dies, DOWN plays out (the recap off)
            try
            {
                if (g.settings.Bool(UnityTuning.RecapSetting))
                {
                    g.settings.Set(UnityTuning.RecapSetting, false);
                    offs.Add(() => g.settings.Set(UnityTuning.RecapSetting, true));
                }
            }
            catch (KeyNotFoundException) { }
            PutRunRows30(g);
            Vec3 f0 = g.Feet;
            g.health.max = g.health.hp = 100;
            g.health.Damage(g.health.max * 2, "husk", new Vec3(f0.x, f0.y, f0.z - 3));
            n = 0;
            while (g.flow != Flow.Results && n++ < 1800 && Time.realtimeSinceStartup < wallEnd)
            {
                if (g.flow == Flow.Down) PutRunRows30(g); // the rows stay asked for all the way in
                yield return null;
            }
            PutRunRows30(g);
            yield return Frames(10);
            int results = RunRowsDrawn30(lesson, out string wRes);
            lines.Add($"RESULTS (flow {g.flow}): {results} drawn ({wRes.Trim()})");
            if (g.flow != Flow.Results) fails.Add($"no RESULTS (flow {g.flow})");
            else if (results != 0) fails.Add($"RESULTS: {results} run row(s) drawn over the menu ({wRes.Trim()})");
            yield return Shot30(dir, "menus30-results");

            // TITLE (the title's own path)
            g.ToTitle();
            yield return Frames(20);
            PutRunRows30(g);
            yield return Frames(10);
            int title = RunRowsDrawn30(lesson, out string wTitle);
            lines.Add($"TITLE (flow {g.UiFlow}): {title} drawn ({wTitle.Trim()})");
            if (title != 0) fails.Add($"TITLE: {title} run row(s) drawn over the menu ({wTitle.Trim()})");
            yield return Shot30(dir, "menus30-title");
            yield return Stop();
            Hud29.ReelForce = false; Hud29.Reset();
            g.UiView?.HideLesson25();
            g.StartRun(Seed); // a live run for the next scenario
            God(g);
            yield return Frames(30);

            string row = Hud30.On ? "hud30 ON" : "hud30 OFF (-skygrindOff30 or the row off: NIGHT29's HUD, the RED half)";
            if (!Hud30.On) fails.Insert(0, "design.n30.hud30 is not live");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + " | " + row + " | " + string.Join(" || ", lines);
        }

        // ======================================================= layout30 =======================================================

        static IEnumerator Layout30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot);
            offs.Add(() => { Quiet(g); g.husks.frozen = false; });
            var fails = new List<string>();
            var lines = new List<string>();
            g.forcedCombat = null;
            foreach (string w in new[] { "revolver", "shotgun", "gauntlet" }) if (g.loadout.owned.IndexOf(w) < 0) g.loadout.AddWeapon(w);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            God(g);
            g.husks.frozen = true;
            yield return Equip27(g, "revolver");
            Start(g, "layout30", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // 1: at rest, the pistol in hand
            yield return Frames(20);
            string r1 = Hud30Layout.Report(out bool ok1, out int c1);
            lines.Add("REST (pistol): " + r1);
            if (!ok1) fails.Add($"rest: {c1} collision(s)");
            yield return Shot30(dir, "layout30-rest");

            // 2: the gauntlet in hand with a full glove (NIGHT29: "PULSE READY" over the icons and "TRIGGER STALE")
            PulseCharge27 p = g.sim.pulse27;
            yield return Equip27(g, "gauntlet");
            if (p != null && p.active)
            {
                p.pips = p.p.pips; // the view's state only (pulse27's own reel walks the kill path)
                p.cooldownSteps = 0;
            }
            yield return Frames(20);
            string r2 = Hud30Layout.Report(out bool ok2, out int c2);
            lines.Add($"GLOVE FULL (pulse27 {(p != null && p.active ? "live" : "off")}): " + r2);
            if (!ok2) fails.Add($"glove: {c2} collision(s)");
            yield return Shot30(dir, "layout30-glove");

            // 3: airborne (the style meter's "x1.2 AIR" chip): dropped from 6 m over the spot
            g.Respawn(new Vec3(spot.x, spot.y + 6, spot.z), spotYaw * 180 / Math.PI);
            string r3 = "not reached";
            bool ok3 = true;
            int c3 = 0;
            for (int i = 0; i < 30; i++)
            {
                yield return null;
                if (i == 18) { r3 = Hud30Layout.Report(out ok3, out c3); }
            }
            lines.Add("AIRBORNE (chip): " + r3);
            if (!ok3) fails.Add($"airborne: {c3} collision(s)");
            yield return Shot30(dir, "layout30-air");
            yield return Frames(60);

            // 4: a REFUND power-up running (P0 f: its "½" chip by the score is kept off a campaign run)
            string r4;
            bool ok4;
            int c4;
            g.powerUps.Spawn(g.Feet, "refund"); // dropped at the player's feet: the real grab path picks it up
            yield return Frames(Ms(2500)); // the banner flies into its chip
            r4 = Hud30Layout.Report(out ok4, out c4);
            lines.Add($"REFUND (mode {g.setup.mode}, chip hidden {Hud30.RefundHidden}): " + r4);
            if (!ok4) fails.Add($"refund: {c4} collision(s)");
            yield return Shot30(dir, "layout30-refund");
            yield return Stop();

            string row = Hud30.On ? "hud30 ON" : "hud30 OFF (NIGHT29's HUD, the RED half)";
            if (!Hud30.On) fails.Insert(0, "design.n30.hud30 is not live");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + (gapOk ? "" : " (SafeSpot: the sandbox spot)") + " | " + row + " | " + string.Join(" || ", lines);
        }

        // ======================================================= weaponhud30 =======================================================

        static int Slot30Index(string id)
        {
            for (int i = 0; i < Ui.Weapon30Ids.Count; i++) if (Ui.Weapon30Ids[i] == id) return i;
            return -1;
        }

        static string Slot30State(string id) { int i = Slot30Index(id); return i >= 0 && i < Ui.Weapon30States.Count ? Ui.Weapon30States[i] : "none"; }
        static string Slot30Word(string id) { int i = Slot30Index(id); return i >= 0 && i < Ui.Weapon30Words.Count ? Ui.Weapon30Words[i] : "none"; }

        /// <summary>The pad's live NEXT WEAPON button (PadReader.NextWeaponInputButton: RB on v3, Y on older layouts) until `gun` is in hand, at most `taps`
        /// presses (round 1 pressed RB by name and the layout's button was another: nothing switched).</summary>
        static IEnumerator CycleTo30(SurvivalGame g, Pad30 pad, string gun, int taps, bool[] ok)
        {
            ok[0] = g.loadout.Weapon == gun;
            GamepadButton next = g.padReader != null ? g.padReader.NextWeaponInputButton : GamepadButton.North;
            for (int i = 0; i < taps && !ok[0]; i++)
            {
                yield return Tap30(pad, next);
                yield return Frames(Ms(500)); // the swap's lower and raise
                ok[0] = g.loadout.Weapon == gun;
            }
        }

        static string BlastRead30() => Ui.Weapon30BlastState + " '" + Ui.Weapon30BlastWord + "'";

        static IEnumerator WeaponHud30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot);
            offs.Add(() => { Quiet(g); g.husks.frozen = false; });
            var fails = new List<string>();
            var lines = new List<string>();
            double wallEnd = Time.realtimeSinceStartup + 150;
            g.forcedCombat = null;
            // the reel's own grant first (round 1: the run owned two guns): the shotgun and the gauntlet beside the pistol
            foreach (string w in new[] { "revolver", "shotgun", "gauntlet" }) if (g.loadout.owned.IndexOf(w) < 0) g.loadout.AddWeapon(w);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            God(g);
            g.husks.frozen = true;
            Pad30 pad = NewPad30(offs);
            yield return Frames(30);
            yield return Equip27(g, "revolver");
            Start(g, "weaponhud30", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var ok = new bool[1];
            PulseCharge27 p = g.sim.pulse27;
            bool pulse = p != null && p.active && g.sim.n27.pulse27;
            if (pulse) { p.pips = 0; p.cooldownSteps = 0; }

            // a: at rest, every slot READY (the BLAST slot fills its pips: CHARGING); a READY slot shows no word at rest (the word READY shows for 1 s on a turn)
            yield return Frames(Ms(1500));
            string rest = Ui.Weapon30Describe();
            lines.Add("REST (a READY slot has no word at rest; READY shows 1 s on a turn): " + rest);
            for (int i = 0; i < Ui.Weapon30Ids.Count && i < Ui.Weapon30States.Count; i++)
                if (Ui.Weapon30Ids[i] != "blast" && Ui.Weapon30States[i] != "READY") { fails.Add("rest: a slot not READY (" + rest + ")"); break; }
            if (Ui.Weapon30Ids.Count < 3) fails.Add($"rest: {Ui.Weapon30Ids.Count} slots for 3 guns ({rest})");
            yield return Shot30(dir, "weaponhud30-rest");

            // b: the real switch to the gauntlet; its heavy swing (RT): PUNCH COOLING with seconds (2.5 s: the guns' shot cycles are under the 0.6 s
            //    COOLING floor, the shotgun's 0.33 s included, so a gun never flickers COOLING per shot)
            yield return CycleTo30(g, pad, "gauntlet", 6, ok);
            if (!ok[0]) fails.Add($"the pad's NEXT WEAPON never put the gauntlet in hand ({g.loadout.Weapon})");
            yield return Frames(Ms(700));
            int turns0 = Ui.Weapon30ReadyTurns;
            string idCool = pulse ? "punch" : "gauntlet";
            pad.Rt(1f);
            yield return Frames(3);
            pad.Rt(0f);
            yield return Frames(3);
            string cool = Slot30State(idCool), coolWord = Slot30Word(idCool);
            lines.Add($"GAUNTLET SWING: {idCool} {cool} '{coolWord}' (cooldown {g.loadout.CooldownStepsOf("gauntlet") * g.sim.step:0.00} s)");
            if (cool != "COOLING" || string.IsNullOrEmpty(coolWord)) fails.Add($"the swung gauntlet's {idCool} slot read {cool} '{coolWord}', not COOLING with its seconds (cooldown {g.loadout.CooldownStepsOf("gauntlet") * g.sim.step:0.00} s)");
            yield return Shot30(dir, "weaponhud30-cooling");

            // c: SWITCHING: the next-weapon press right after the swing (inside its action, or the swap that follows): the target slot says SWITCHING
            string sw = "not seen";
            GamepadButton next = g.padReader != null ? g.padReader.NextWeaponInputButton : GamepadButton.North;
            pad.Button(next, true);
            for (int i = 0; i < 40 && sw == "not seen"; i++)
            {
                if (i == 3) pad.Button(next, false); // the tap's release (a held NEXT may open the wheel)
                int k = Ui.Weapon30Words.IndexOf(UnityTuning.Hud30WordSwitching);
                if (k >= 0) sw = Ui.Weapon30Ids[k] + " SWITCHING (to slot " + Hud30Reads.SwitchingSlot(g.loadout) + (g.loadout.rules30 != null ? ", rules30 queue / swap" : g.loadout.lockout29 != null ? ", lockout29 queue / swap" : ", swap") + ")";
                yield return null;
            }
            pad.Button(next, false);
            yield return Shot30(dir, "weaponhud30-switching");
            if (sw == "not seen") fails.Add("no SWITCHING word on the switch's target");
            lines.Add("SWITCH AFTER THE SWING: " + sw + $", now in hand {g.loadout.Weapon}");

            // d: the gauntlet's cooldown runs in the holster and the slot turns READY: the word READY and a READY turn (flash / event)
            string readyWord = "";
            int n = 0;
            while (n++ < 600 && Time.realtimeSinceStartup < wallEnd)
            {
                yield return null;
                if (Slot30State(idCool) == "READY") { readyWord = Slot30Word(idCool); break; }
            }
            int turns = Ui.Weapon30ReadyTurns - turns0;
            lines.Add($"{idCool} READY after {n} frames (holstered): word '{readyWord}', READY turns +{turns}");
            if (readyWord != UnityTuning.Hud30WordReady) fails.Add($"the {idCool} slot turned ready with the word '{readyWord}', not READY");
            if (turns < 1) fails.Add("no READY turn counted (flash / event)");
            yield return Shot30(dir, "weaponhud30-ready");

            // e: CHARGING: RT held on the pistol (design.n27.chargeShots27)
            ChargeShots27 cs = g.sim.charge27;
            yield return CycleTo30(g, pad, "revolver", 6, ok);
            if (cs != null && cs.active && g.sim.n27.chargeShots27 && ok[0])
            {
                yield return Frames(Ms(600));
                pad.Rt(1f);
                string ch = "not seen";
                for (int i = 0; i < Ms(1600); i++)
                {
                    yield return null;
                    if (Slot30State("revolver") == "CHARGING") { ch = "CHARGING '" + Slot30Word("revolver") + "' level " + cs.Level(0).ToString("0.00"); if (cs.Level(0) > 0.5) break; }
                }
                yield return Shot30(dir, "weaponhud30-charging");
                pad.Rt(0f);
                lines.Add("PISTOL HELD: " + ch);
                if (ch == "not seen") fails.Add("RT held on the pistol: no CHARGING");
                yield return Frames(Ms(800));
            }
            else lines.Add($"PISTOL HELD: skipped (chargeShots27 {(cs != null && cs.active ? "live" : "off")}, pistol in hand {ok[0]})");

            // f: the BLAST slot: 0/5, 2/5 (pips set by the reel: the kill path is pulse27's own reel), full = READY, then a REAL blast (the pad's LT) ->
            //    COOLING with its seconds
            yield return CycleTo30(g, pad, "gauntlet", 6, ok);
            if (pulse && ok[0])
            {
                yield return Frames(Ms(600));
                p.pips = 0;
                p.cooldownSteps = 0;
                yield return Frames(6);
                string b0 = BlastRead30();
                p.pips = 2;
                yield return Frames(6);
                string b2 = BlastRead30();
                yield return Shot30(dir, "weaponhud30-pips");
                int t0 = Ui.Weapon30ReadyTurns;
                p.pips = p.p.pips;
                yield return Frames(6);
                string b5 = BlastRead30();
                yield return Shot30(dir, "weaponhud30-blastready");
                int blasts0 = p.blasts;
                pad.Lt(1f);
                yield return Frames(6);
                pad.Lt(0f);
                yield return Frames(6);
                string bc = BlastRead30();
                yield return Shot30(dir, "weaponhud30-blastcooling");
                lines.Add($"BLAST: 0 pips {b0}; 2 pips {b2}; full {b5} (turns +{Ui.Weapon30ReadyTurns - t0}); after the pad's LT ({p.blasts - blasts0} blast, cooldown {p.cooldownSteps * g.sim.step:0.0} s) {bc}; PUNCH {Ui.Weapon30PunchState}");
                if (Ui.Weapon30BlastState == "") fails.Add("no BLAST slot with the glove live");
                if (!b0.StartsWith("CHARGING '0/", StringComparison.Ordinal)) fails.Add($"0 pips read {b0}");
                if (!b2.StartsWith("CHARGING '2/", StringComparison.Ordinal)) fails.Add($"2 pips read {b2}");
                if (!b5.StartsWith("READY 'READY'", StringComparison.Ordinal)) fails.Add($"a full glove read {b5}");
                if (p.blasts - blasts0 != 1) fails.Add($"the pad's LT made {p.blasts - blasts0} blasts with a full glove (refused: {p.deniedWhy})");
                else if (!bc.StartsWith("COOLING '", StringComparison.Ordinal) || bc.EndsWith("''", StringComparison.Ordinal)) fails.Add($"after the blast the slot read {bc} (want COOLING with its seconds)");
                p.cooldownSteps = 0;
            }
            else lines.Add($"BLAST: skipped (pulse27 {(pulse ? "live" : "off")}, gauntlet in hand {ok[0]})");
            yield return Stop();

            string row = Hud30.On ? "hud30 ON" : "hud30 OFF (NIGHT29's HUD, the RED half: no widget)";
            if (!Hud30.On) fails.Insert(0, "design.n30.hud30 is not live (no widget)");
            WeaponHud30Result(res, fails, pc, gapOk, row, lines);
        }

        static void WeaponHud30Result(string[] res, List<string> fails, string pc, bool gapOk, string row, List<string> lines)
        {
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + (gapOk ? "" : " (SafeSpot: the sandbox spot)") + " | " + row + $" | {FeelProbe.FrameCount} frames, virtual pad (RB, RT) | " + string.Join(" || ", lines);
        }

        // ======================================================= look30 =======================================================

        /// <summary>Saves dir/shot-N30-name.png and measures it: [0] the mean luminance, [1] the share of near-white pixels (luminance over 0.85) on the
        /// lower third, outside the gun's corner and the HUD's bottom band (the floor panels that light nothing).</summary>
        static IEnumerator LookShot30(string dir, string name, double[] stats)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N30-" + name + ".png"), t.EncodeToPNG());
                int w = t.width, h = t.height, n = 0, nl = 0, white = 0;
                double sum = 0;
                for (int y = 0; y < h; y += 6)
                    for (int x = 0; x < w; x += 6)
                    {
                        Color c = t.GetPixel(x, y); // y from the bottom
                        double l = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                        sum += l;
                        n++;
                        if (y > h * 0.14 && y < h * 0.36 && x > w * 0.2 && x < w * 0.6) { nl++; if (l > 0.85) white++; }
                    }
                stats[0] = n > 0 ? sum / n : -1;
                stats[1] = nl > 0 ? (double)white / nl : -1;
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        static IEnumerator Look30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string pc = Preconditions(g, g.Feet);
            offs.Add(() => { Quiet(g); g.husks.frozen = false; });
            var fails = new List<string>();
            var lines = new List<string>();
            var st = new double[2];
            bool look = N30Fixes.ViewRowOn(g.cfg, "look30");
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            God(g);
            Start(g, "look30", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // three rooms: the first, the middle and the last fight room of the loaded level (else three headings here)
            // the run's campaign level, else the loaded map's own campaign block (r2: on -skygrindMap n30-foundry the reel's run is not a campaign run, so
            // sim.campaignLevel was null and the views fell back to "no campaign level")
            LevelDef lv = g.sim.campaignLevel;
            if (lv == null && g.map != null && g.map.json != null && g.map.json.ContainsKey("campaign")) lv = LevelDef.From(g.map.json);
            var picks = new List<RoomDef>();
            if (lv != null)
            {
                var fights = new List<RoomDef>();
                foreach (RoomDef r in lv.rooms) if (r.kind != "boss") fights.Add(r);
                if (fights.Count > 0) { picks.Add(fights[0]); if (fights.Count > 2) picks.Add(fights[fights.Count / 2]); if (fights.Count > 1) picks.Add(fights[fights.Count - 1]); }
            }
            if (picks.Count == 0)
            {
                double yaw0 = g.input.yaw;
                for (int k = 0; k < 3; k++)
                {
                    g.input.SetLook(yaw0 + k * 2.1, -0.15);
                    yield return Frames(20);
                    yield return LookShot30(dir, "look30-view" + k, st);
                    lines.Add($"VIEW {k} (no campaign level): luminance {st[0]:0.000}, floor white {100 * st[1]:0.0} %");
                }
            }
            foreach (RoomDef r in picks)
            {
                g.Respawn(r.spawn, r.spawnYawDeg);
                g.input.SetLook(r.spawnYawDeg * Math.PI / 180, -0.12);
                God(g);
                yield return Frames(45);
                g.husks.KillAll();
                yield return Frames(15);
                yield return LookShot30(dir, "look30-room-" + r.id, st);
                lines.Add($"ROOM {r.id}: luminance {st[0]:0.000}, floor white {100 * st[1]:0.0} %");
            }

            // the HUD states, back in the first room
            if (picks.Count > 0) { g.Respawn(picks[0].spawn, picks[0].spawnYawDeg); g.input.SetLook(picks[0].spawnYawDeg * Math.PI / 180, -0.12); }
            g.health.max = g.health.hp = 100;
            yield return Frames(40);
            yield return LookShot30(dir, "look30-hud-rest", st);
            int fl0 = Ui.Fx30Flickers, gl0 = Ui.Fx30Glows;
            Vec3 f0 = g.Feet;
            g.health.Damage(12, "husk", new Vec3(f0.x, f0.y, f0.z - 3));
            yield return Frames(2);
            yield return LookShot30(dir, "look30-hud-hit", st);
            yield return Frames(20);
            int flick = Ui.Fx30Flickers - fl0, glows = Ui.Fx30Glows - gl0;
            g.health.hp = g.health.max * 0.18;
            yield return Frames(30);
            bool buzz = Ui.Fx30Buzzing;
            yield return LookShot30(dir, "look30-hud-low", st);
            yield return Stop();
            God(g);
            lines.Add($"HUD: fx live {Ui.Fx30Live}, a hit taken -> {flick} flicker(s), {glows} glow(s); at 18 % health buzzing {buzz}; quantise strength {Crt29.QuantStrength:0.00} (levels {Crt29.QuantLevels:0}, dither cell {Crt29.QuantDitherScale:0} px), grain-in-the-dark {Crt29.GrainDark:0}, CRT strength {Crt29.Strength:0.00}, pixel block {Crt29.PixelStep:0} px");
            if (!look) fails.Add("look30 off (-skygrindOff30 or the row off): these are the BEFORE frames");
            else
            {
                if (!Ui.Fx30Live) fails.Add("the HUD fx are not live (HUD FLICKER off?)");
                if (flick < 1) fails.Add("a hit taken flickered no panel");
                if (glows < 1) fails.Add("a hit taken glowed no panel");
                if (!buzz) fails.Add("no buzz under 25 % health");
                if (!(Crt29.QuantStrength > 0)) fails.Add("the quantise pass is not fed (Crt29.QuantStrength 0)");
            }
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + " | " + string.Join(" || ", lines);
        }
    }
}
