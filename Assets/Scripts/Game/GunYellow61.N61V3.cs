// NIGHT61 builder V (slate C3, gg-proxy's candidate; view row design.n61.gunYellow61, OFF in the repo; Tools/night/n61-v3-rows.cjs): NO YELLOW ON THE
// GUNS. Yellow means a parry tell (CLAUDE.md rule 8, DESIGN-RULES C-5; the NIGHT55 first-time critic: "the biggest yellow thing on screen is my own
// shotgun"); gg-proxy at NIGHT61's start: the THUNDERCLAP's flat yellow slab and yellow triangle (kb2c-entry, span's verbs37 f0600), the TORCH's
// orange-yellow slab and yellow pip (spire's verbs37 f0300 / f0900). yellow56 also moved the guns (to a pale brass, not a palette colour) and is OFF
// (it needs a tells mask). View only: nothing here reaches the sim.
// THE RULE: a viewmodel gun whose family colour (the web's weapons.families) is in the yellow arc (hue UnityTuning.GunYellow61HueLo..HueHi, saturation
// over GunYellow61MinS: edge #ffb000 the THUNDERCLAP, fire #ff8c1a the TORCH, blast #ffe14a the BOUNCER) takes colors.usable, the dusk palette's
// player-usable cyan (alpha kept). The family colour is what the viewmodel builds a gun's accent plate (HandsLook.Accent), its gem and the TORCH's
// pilot glow from, so those move together; the shapes are untouched. Two hook lines in Viewmodel.cs (FamilyOf and the THUNDERCLAP's build) and nothing
// else: wall buys, pickups, the mystery box, muzzle flashes and every enemy and tell colour keep theirs (N61GunYellowTests scans for callers).
// The viewmodel is built before PlayerChecks applies -skygrindOffNN, so the command line is read here as well (as yellow56 and guns52 do).
// With yellow56 also on, this rule runs first: the cyan is outside yellow56's arc, so yellow56 leaves it.
// Off: the row off, -skygrindOff61 (or older) or -skygrindNoGunYellow61 (this row alone). The proof (L-089): one Player log line per gun moved,
// "[gunyellow61] <weapon> #<from> -> #<to> (colors.usable)"; GunYellow61.GunsMoved / Moved for a reel. Numbers: UnityTuning.N61V3.cs.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class GunYellow61
    {
        /// <summary>The view row's name (design.n61.gunYellow61).</summary>
        public const string Row = "gunYellow61";
        /// <summary>The one-row off switch: this row alone off.</summary>
        public const string NoSwitch = "-skygrindNoGunYellow61";
        /// <summary>The night of the row (its -skygrindOffNN switch covers it from this night on).</summary>
        public const int Night = 61;

        /// <summary>For a reel: the guns moved off the arc in this process, and their weapon ids.</summary>
        public static int GunsMoved;
        public static string Moved = "";

        static readonly HashSet<string> logged = new HashSet<string>();
        static string[] args;

        /// <summary>This process's command line, read once (none readable: no switch).</summary>
        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Is the row off for this config and command line? Off when the row is off (NightRows.ViewOn: its design `enabled`, the process's
        /// -skygrindOffNN), under -skygrindOff61 or an older night's switch, or -skygrindNoGunYellow61 (this row alone).</summary>
        public static bool OffFor(GameConfig cfg, string[] args) => OffReason(cfg, args) != null;

        /// <summary>Why it is off, null when on.</summary>
        public static string OffReason(GameConfig cfg, string[] args)
        {
            if (!NightRows.ViewOn(cfg, Row)) return "design.n61." + Row + " off";
            if (NightRows.OffFrom(args, "-skygrindOff") <= Night) return "-skygrindOff61 (or older)";
            if (NightRows.HasSwitch(args, NoSwitch)) return NoSwitch; // NIGHT63 R1 N5: the one rule (case ignored, as before)
            return null;
        }

        /// <summary>Is a family colour in the yellow arc (hue GunYellow61HueLo..HueHi, saturation over GunYellow61MinS)?</summary>
        public static bool InArc(Color c)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            float hd = h * 360f;
            return hd >= UnityTuning.GunYellow61HueLo && hd <= UnityTuning.GunYellow61HueHi && s > UnityTuning.GunYellow61MinS;
        }

        /// <summary>A viewmodel gun's family colour with the rule applied for this config and command line: colors.usable (alpha kept) when the
        /// row is on and the colour is in the arc, else the colour as it came.</summary>
        public static Color FamilyFor(GameConfig cfg, string[] args, string weapon, Color c)
        {
            if (cfg == null || !InArc(c) || OffFor(cfg, args)) return c;
            Color o = Look.C(cfg, "colors.usable");
            o.a = c.a;
            return o;
        }

        /// <summary>The viewmodel's hook (Viewmodel.FamilyOf and the THUNDERCLAP's build): FamilyFor with this process's command line; the proof
        /// line once per gun moved.</summary>
        public static Color Family(GameConfig cfg, string weapon, Color c)
        {
            Color o = FamilyFor(cfg, Args(), weapon, c);
            if (o != c && weapon != null && logged.Add(weapon))
            {
                GunsMoved++;
                Moved = Moved.Length == 0 ? weapon : Moved + ", " + weapon;
                Debug.Log(LogLine(weapon, c, o));
            }
            return o;
        }

        /// <summary>The Player's proof line when a gun's colour moves off the arc (L-089: the reel greps "[gunyellow61]").</summary>
        public static string LogLine(string weapon, Color from, Color to) =>
            "[gunyellow61] " + weapon + " #" + ColorUtility.ToHtmlStringRGB(from) + " -> #" + ColorUtility.ToHtmlStringRGB(to) + " (colors.usable)";
    }
}
