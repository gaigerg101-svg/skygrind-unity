// NIGHT74 builder B (bosses): the boss63 reel's lines for weak74 (B1) and target74 (B2). boss63 (FeelReel.N63V.cs / N63G.cs) takes five frames per boss (idle, windup, tell, flinch, stagger,
// shot-N30-B63-<boss>-<tag>.png); Mark starts a coroutine just before each shot's own, so both wait for the same end of frame: the line is written when the frame the PNG captures has drawn
// (the Crown draws in LateUpdate, after the reel's coroutine step, so a line written at the step would read the frame before).
//   [weak74] <file> | boss <id> | row on|off | open yes|no|not drawn | weak box <x0,y0,x1,y1|none|off|behind|partial:..> | body box <...> | frame WxH | parts cyan <n> of <m> | identity #RRGGBB | totals
// The boxes are image pixels (rows from the top) of the captured frame: the open weak point's (only while open) and the boss's whole drawn body. n74-cyan.py reads them to split the cyan the
// frame holds into inside / outside the weak box; the row OFF reads the parts the row would take (cyan now), ON the parts still cyan (0 while shut).
// Reel commands (the lead's; one Player per reel): ON `-skygrindRows72,weak74:1`, OFF plain; read Logs/feel/<tag>/<side>/boss63/boss63-<boss>.txt for the [weak74] lines.
using System;
using System.Collections;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Weak74Reel
    {
        /// <summary>Starts the line for the shot `tag` of `boss` (a BossAttackClock63 id): written through `say` at the end of this frame (before the shot's own PNG is encoded).</summary>
        public static void Mark(SurvivalGame g, string boss, string tag, Action<string> say)
        {
            if (g == null || say == null || string.IsNullOrEmpty(boss)) return;
            g.StartCoroutine(MarkAtEnd(g, boss, tag, say));
        }

        static IEnumerator MarkAtEnd(SurvivalGame g, string boss, string tag, Action<string> say)
        {
            yield return new WaitForEndOfFrame();
            say(Weak74.ShotLine(g, "shot-N30-B63-" + boss + "-" + tag + ".png", boss)); // the caller's Say logs it and keeps it for boss63-<boss>.txt
        }
    }
}
