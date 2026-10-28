// NIGHT24 remove-replay (builder R): the reel scenario `remove`. UNCOMPILED by the Editor and UNRUN when written (dn-check 0 errors). On the Player:
// a run is driven to DOWN and on to RESULTS (a lethal blow between frames, then real time until the flow reaches RESULTS, 25 s at most),
// counting rendered frames; then every UI object of the scene is scanned by NAME for anything called Replay* or VIEW, and TITLE's and RESULTS'
// texts for REPLAY. The ring and the view are types that no longer exist (the grep check: `SimHarness remove --grep`), so "ring absent" is a
// compile-time fact the line states. Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static readonly Regex RemovedUiName = new Regex("replay", RegexOptions.IgnoreCase);

        static partial void N24RemoveBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = RemoveScenario(g, dir, res, offs, pre);
        }

        /// <summary>The names of the scene's UI objects that look like the removed replay (by name: Replay*, or exactly VIEW).</summary>
        static List<string> RemovedUiObjects()
        {
            var hits = new List<string>();
            foreach (RectTransform t in UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (RemovedUiName.IsMatch(t.name) || t.name == "VIEW") hits.Add(t.name);
            return hits;
        }

        /// <summary>The texts under the TITLE and RESULTS screens (found by name) that match `bad`.</summary>
        static List<string> ScreenTexts(Regex bad, List<string> all = null)
        {
            var hits = new List<string>();
            foreach (RectTransform t in UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name != "Title" && t.name != "Results") continue;
                foreach (TMP_Text tx in t.GetComponentsInChildren<TMP_Text>(true))
                {
                    string s = tx.text ?? "";
                    all?.Add(s);
                    if (bad != null && bad.IsMatch(s)) hits.Add(t.name + ": " + s);
                }
            }
            return hits;
        }

        static IEnumerator RemoveScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Frames(20);
            Start(g, "remove", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            g.health.max = g.health.hp = 100;
            g.health.Damage(g.health.max * 2, "husk", new Vec3(spot.x, spot.y, spot.z - 3));
            int frames = 0;
            float t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Results && Time.realtimeSinceStartup - t0 < 25)
            {
                frames++;
                yield return null;
            }
            bool results = g.flow == Flow.Results;
            yield return Frames(30);
            yield return Stop();
            List<string> names = RemovedUiObjects();
            List<string> texts = ScreenTexts(new Regex("REPLAY|\\bVIEW\\b", RegexOptions.IgnoreCase));
            bool ok = results && names.Count == 0 && texts.Count == 0;
            res[0] = pc + (ok
                ? $" | remove PASS: no replay prompt / view object found, DOWN -> RESULTS in {frames} frames, ring absent (its type is gone)"
                : $" | remove RED: flow {g.flow} after {frames} frames; replay-like UI objects [{string.Join(", ", names)}]; TITLE / RESULTS texts [{string.Join("; ", texts)}]");
        }
    }
}
