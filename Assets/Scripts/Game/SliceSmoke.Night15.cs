// NIGHT15 smoke steps (after NIGHT14's): each item's proof in the built Player and the Editor smoke.
// 22. U3-28.8: "pad only": TITLE to a run, the wheel, PAUSED, SETTINGS and back with a virtual pad, never the mouse
//     (builder K's PadOnly, SliceSmoke.Night15Pad.cs).
// 23. U3-28.6: the new map: the AUTOPILOT (BotBrain, normal, real HP) plays design.maps.list's first Unity-authored map
//     for NewMapSeconds of sim time on a fresh headless composition, twice: at least three rounds cleared and the same
//     hash every 600 steps both times (no stall, deterministic on the new map's data).
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SliceSmoke
    {
        /// <summary>Step 24's run length in sim seconds (rounds 1-3 and their breaks fit with room on the old map's pace).</summary>
        const double NewMapSeconds = 300;

        IEnumerator Night15(SurvivalGame g)
        {
            yield return PadOnly(g);
            yield return NewMapAutopilot(g);
        }

        IEnumerator NewMapAutopilot(SurvivalGame g)
        {
            SurvivalGame.MapEntry? entry = null;
            foreach (SurvivalGame.MapEntry m in g.Maps)
            {
                if (m.id == RunSetup.DefaultMap) continue;
                entry = m;
                break;
            }
            if (entry == null)
            {
                Check(false, "new map: design.maps.list has no Unity-authored map with a file");
                yield break;
            }
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(entry.Value.file)));
            var a = new List<string>();
            var b = new List<string>();
            float t0 = Time.realtimeSinceStartup;
            Farm.Record ra = Farm.RunOne(g.cfg, map, 4242, "normal", NewMapSeconds, a);
            yield return null;
            Farm.Record rb = Farm.RunOne(g.cfg, map, 4242, "normal", NewMapSeconds, b);
            int same = 0;
            for (int i = 0; i < a.Count && i < b.Count; i++)
                if (a[i] == b[i]) same++;
            Check(ra.brain == "botBrain" && ra.cleared >= 3 && a.Count > 0 && same == a.Count && a.Count == b.Count && ra.round == rb.round,
                $"autopilot on {map.name}: seed 4242 normal ({ra.brain}) twice for {NewMapSeconds:0} s: round {ra.round} / {rb.round}, cleared {ra.cleared}, {ra.kills} kills, cause {ra.cause}; {same} of {a.Count} hashes equal ({Time.realtimeSinceStartup - t0:0.0} s); last {(a.Count > 0 ? a[a.Count - 1] : "-")}");
            yield return null;
        }
    }
}
