// NIGHT63 builder V (slate B1; view row design.n63.bossMotion63, OFF in the repo; Tools/night/n63-v-rows.cjs fills its numbers): BOSSES THAT MOVE LIKE
// THEY MEAN IT. This file: the row's switch and the probe the boss63 reel reads (BossMotion63Probe). The layer itself (the springs, the wind-up,
// the flinch, the slump, the breathing) is BossMotion63.Layer.cs. View only: nothing here reaches the sim.
// THE SWITCH is read EVERY FRAME, never cached (builder D's D1 makes NightRows.ViewOn consult a live reel override, so a reel can capture one frame
// on and off in one run): on when design.n63.bossMotion63.enabled (NightRows.ViewOn: also off under the process's -skygrindOffNN, NN <= 63), and
// neither -skygrindOff63 (or older) nor -skygrindNoBossMotion63 is on the command line.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class BossMotion63
    {
        /// <summary>The view row's name (design.n63.bossMotion63).</summary>
        public const string Row = "bossMotion63";
        /// <summary>The one-row off switch: this row alone off.</summary>
        public const string NoSwitch = "-skygrindNoBossMotion63";
        /// <summary>The row's night (its -skygrindOffNN switch covers it from this night on).</summary>
        public const int Night = 63;
        /// <summary>The row's numbers' prefix in the config.</summary>
        public const string Numbers = "design.n63.bossMotion63.numbers.";

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

        /// <summary>Why the row is off for this config and command line (null: on).</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (!NightRows.ViewOn(cfg, Row)) return "design.n63." + Row + " off";
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff63 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch; // NIGHT63 R1 N5: the one rule (case ignored, as before)
            return null;
        }

        /// <summary>Is the row off for this config and command line?</summary>
        public static bool OffFor(GameConfig cfg, string[] argv) => OffReason(cfg, argv) != null;

        /// <summary>Is the row on now (this process's command line)? Read every frame: never cache it (D1's same-run toggle flips ViewOn).</summary>
        public static bool On(GameConfig cfg) => cfg != null && !OffFor(cfg, Args());
    }

    /// <summary>What the motion layer drew on its last frame (the boss63 reel reads it; all zero while the row is off or no layer ran).</summary>
    public static class BossMotion63Probe
    {
        /// <summary>The boss the layer drew (a bossRise id) and the Time.frameCount of that frame (-1: never).</summary>
        public static string Boss;
        public static int Frame = -1;
        /// <summary>Parts moved on that frame, the sum of their squared offsets from the rigid pose (m^2), the largest pivot offset (m), its part.</summary>
        public static int Parts;
        public static double SumSq, Max;
        public static string MaxPart = "";
        /// <summary>The largest share of a part's config limit used on that frame (GUARD: never over 1).</summary>
        public static double LimitShare;
        /// <summary>The drawn wind-up pose (0..1 of its peak), the drawn flinch envelope (0..1), the drawn slump (0..1), the breath (-1..1).</summary>
        public static double Windup, Flinch, Slump, Breath;
        /// <summary>Flinches the layer started since boot (a damage event seen while on).</summary>
        public static int Flinches;
        /// <summary>NIGHT63 B1e (a measurement): on that frame, the drawn weak point's distance from the sim's hit sphere centre (m) and the sphere's
        /// radius (m; the size the guard scales by). WeakSet: recorded this frame (the layer, or a view that draws its own).</summary>
        public static bool WeakSet;
        public static double WeakDist, WeakR;

        /// <summary>NIGHT63 B1e: record the drawn weak point's distance from the sim's hit sphere centre and its radius (a later call wins: a view
        /// that draws its weak point its own way reports after the layer).</summary>
        public static void Weak(double dist, double radius)
        {
            WeakSet = true;
            WeakDist = dist;
            WeakR = radius;
        }

        /// <summary>Did the layer draw on this frame?</summary>
        public static bool DrewThisFrame => Frame == Time.frameCount;

        /// <summary>Start a frame's numbers (the layer, before its parts).</summary>
        public static void BeginFrame(string boss)
        {
            Boss = boss;
            Frame = Time.frameCount;
            Parts = 0;
            SumSq = Max = LimitShare = 0;
            MaxPart = "";
            WeakSet = false;
            WeakDist = WeakR = 0;
        }

        /// <summary>NIGHT63 V (fix round 3): the reel takes each draw once: the layer drew this frame (a view drawn in Update, before the reel's
        /// coroutine) or the frame before (THE CROWN, drawn in LateUpdate, after the coroutine), and that draw was not taken yet. A draw two or more
        /// frames old is stale. `taken`: the reel's own last taken frame.</summary>
        public static bool Take(int frameNow, ref int taken)
        {
            if (Frame < 0 || Frame <= taken || Frame < frameNow - 1) return false;
            taken = Frame;
            return true;
        }

        /// <summary>One part's offset from its rigid pose this frame (m; its pivot's offset; the share of its limit used).</summary>
        public static void Part(string name, double offset, double pivot, double limitShare)
        {
            Parts++;
            SumSq += offset * offset;
            if (pivot > Max) { Max = pivot; MaxPart = name ?? ""; }
            if (limitShare > LimitShare) LimitShare = limitShare;
        }
    }
}
