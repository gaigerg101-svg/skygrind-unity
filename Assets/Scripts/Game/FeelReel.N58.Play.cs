// NIGHT58 builder G (G1, the reel phase a258; FeelReel.N58.cs's N58Play hook): kill55's marks AFTER the white hit flash.
// a258 runs a355's scenario (FeelReel.N55.Kill.cs: the chain ladder, the debris, the spaced kills, the damage victim) with one step added in the
// damage part: after the hit's next frame it grabs every frame and measures, in the victim's body rect, the share of the before frame's warm
// pixels that are the flash's white (S < N58GFlashWhiteS, V > N58GFlashWhiteV); the first frame where that share falls under N58GFlashEndShare
// is the flash's end (measured on the pixels, L-093). There it saves shot-N55-K-flash-end.jpg and prints:
//   input:  kill55 on, the hit (hp before -> after, the victim alive), the marks laid on it (Kill55Damage.MarkAt: its newest mark, its stage)
//   flash:  the white share on the hit's next frame, the end frame (frames after the hit, + s of real time), the white share there
//   MAIN:   MARKS AT THE FLASH END = the before frame's warm body pixels that changed and are dark (V < Kill55ReelMarkV), share of the body
//           (want >= Kill55ReelMarkNextShare 5 %), the changed pixels' median H / S / V; and the probe: the mark's drawn centre projected to the
//           screen, the mean V of a (2 N58GProbeHalf + 1) px square there before -> at the flash end (a mark that is drawn and seen goes dark)
// GREEN: kill55 on AND the ladder ok (a355's) AND the MAIN >= 5 %. Off side (the same build): -skygrindNoKill55 (Kill55.On; this row alone).
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
        static bool k58Mode, k58Ladder;
        static double k58Marks = -1;
        static string k58Line = "the flash-end step was not reached (no victim hit)";

        static partial void N58Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = N58PlayScenario(g, dir, res, offs, pre);

        static IEnumerator N58PlayScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            k58Mode = true;
            k58Ladder = false;
            k58Marks = -1;
            k58Line = "the flash-end step was not reached (no victim hit)";
            IEnumerator inner = Kill55Scenario(g, dir, res, offs, pre);
            while (inner.MoveNext()) yield return inner.Current;
            k58Mode = false;
            bool on = Kill55.Live(g.cfg);
            bool ok = on && k58Ladder && k58Marks >= UnityTuning.Kill55ReelMarkNextShare;
            string line = $"{(ok ? "GREEN" : "RED")} a258 KILL55 AFTER THE FLASH | kill55 on {on} | {k58Line} | ladder {(k58Ladder ? "ok" : "NO")} | a355's line: {res[0]}";
            Debug.Log("[a258] " + line);
            res[0] = line;
        }

        /// <summary>The frame's screen as a texture (the caller destroys it). Yields to the frame's end.</summary>
        static IEnumerator K58Grab(Texture2D[] keep)
        {
            yield return new WaitForEndOfFrame();
            keep[0] = ScreenCapture.CaptureScreenshotAsTexture();
        }

        /// <summary>Of the before frame's warm body pixels in `r`, the share that is the flash's white in `now`.</summary>
        static double K58White(Texture2D before, Texture2D now, RectInt r)
        {
            int n = 0, white = 0;
            for (int y = r.yMin; y < r.yMax; y += 2)
                for (int x = r.xMin; x < r.xMax; x += 2)
                {
                    Color.RGBToHSV(before.GetPixel(x, y), out float hh, out float s, out float v);
                    if (!(s > 0.35f && v > 0.15f && (hh < 0.12f || hh > 0.94f))) continue;
                    n++;
                    Color.RGBToHSV(now.GetPixel(x, y), out _, out float s2, out float v2);
                    if (s2 < UnityTuning.N58GFlashWhiteS && v2 > UnityTuning.N58GFlashWhiteV) white++;
                }
            return n > 0 ? white / (double)n : 0;
        }

        /// <summary>The mean V of the probe square at screen point (x, y) (-1 off the screen).</summary>
        static double K58PatchV(Texture2D t, float x, float y)
        {
            int cx = (int)x, cy = (int)y, k = UnityTuning.N58GProbeHalf, n = 0;
            double sum = 0;
            for (int yy = cy - k; yy <= cy + k; yy++)
                for (int xx = cx - k; xx <= cx + k; xx++)
                {
                    if (xx < 0 || yy < 0 || xx >= t.width || yy >= t.height) continue;
                    Color.RGBToHSV(t.GetPixel(xx, yy), out _, out _, out float v);
                    sum += v;
                    n++;
                }
            return n > 0 ? sum / n : -1;
        }

        /// <summary>a258's step inside a355's damage part, after the hit's next frame: frame by frame to the flash's end, the marks there.</summary>
        static IEnumerator K58FlashEnd(SurvivalGame g, Husk h, bool hit, Texture2D before, string dir, Action hold)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            if (!hit || before == null) { k58Line = $"input: hit {(hit ? "yes" : "NO")} (no before frame or no hit: nothing measured)"; yield break; }
            RectInt rect = K55BodyRect(g, h, before.width, before.height);
            Kill55Damage dm = g.N55KDamage;
            float t0 = Time.realtimeSinceStartup;
            double whiteNext = -1, whiteEnd = -1;
            int endFrame = -1;
            Texture2D end = null;
            for (int k = 1; k <= UnityTuning.N58GFlashMaxFrames && h.alive; k++)
            {
                hold();
                yield return null;
                var keep = new Texture2D[1];
                yield return K58Grab(keep);
                if (keep[0] == null) continue;
                double w = K58White(before, keep[0], rect);
                if (k == 1) whiteNext = w;
                if (w < UnityTuning.N58GFlashEndShare) { endFrame = k; whiteEnd = w; end = keep[0]; break; }
                UnityEngine.Object.Destroy(keep[0]);
            }
            float endS = Time.realtimeSinceStartup - t0;
            int laid = dm != null ? dm.Laid : 0, drawn = dm != null ? dm.DrawnLastFrame : 0;
            bool has = dm != null && dm.MarkAt(h.id, out _, out _);
            int stage = 0;
            Vector3 mw = default;
            if (dm != null) dm.MarkAt(h.id, out mw, out stage);
            string input = $"input: hit yes (alive {h.alive}, hp {h.hp.ToString("0.0", ci)} of {h.maxHp.ToString("0", ci)}), kill55 marks laid {laid}, on the victim {(has ? (stage == 2 ? "a hole" : "a crack") : "NONE")}, drawn last frame {drawn}, by {(dm != null && dm.Mark59 ? "Kill59Mark (NIGHT59: pulled, no fog)" : "the body copies (NIGHT58)")}" +
                           $", mark queue {(dm != null ? dm.MarkQueue : -1)} vs the enemy x-ray's {(int)UnityEngine.Rendering.RenderQueue.Transparent + UnityTuning.EnemyXrayQueueOffset} (x-ray setting {(g.settings.Bool("xray") ? "on" : "off")}; NIGHT60 B2)";
            if (end == null)
            {
                k58Line = $"{input} | flash: white share next frame {(whiteNext * 100).ToString("0.0", ci)} %, NOT ENDED in {UnityTuning.N58GFlashMaxFrames} frames";
                yield break;
            }
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "shot-N55-K-flash-end.jpg"), end.EncodeToJPG(85));
            k58Marks = K55Marks(before, end, rect, out float mh, out float ms, out float mv);
            string probe = "no mark to probe";
            if (has)
            {
                Vector3 sp = g.cam.WorldToScreenPoint(mw);
                probe = sp.z <= 0 ? "the mark is behind the camera"
                    : $"the mark's centre at px ({sp.x.ToString("0", ci)}, {sp.y.ToString("0", ci)}) {(rect.Contains(new Vector2Int((int)sp.x, (int)sp.y)) ? "inside" : "OUTSIDE")} the body rect: mean V {K58PatchV(before, sp.x, sp.y).ToString("0.00", ci)} before -> {K58PatchV(end, sp.x, sp.y).ToString("0.00", ci)} at the flash end";
            }
            k58Line = $"{input} | flash: white share next frame {(whiteNext * 100).ToString("0.0", ci)} %, ENDS at frame {endFrame} after the hit (+{endS.ToString("0.000", ci)} s real; white {(whiteEnd * 100).ToString("0.0", ci)} % < {(UnityTuning.N58GFlashEndShare * 100).ToString("0", ci)} %) | " +
                      $"MARKS AT THE FLASH END (changed, V < {UnityTuning.Kill55ReelMarkV.ToString("0.00", ci)}) {(k58Marks * 100).ToString("0.00", ci)} % of the body (want >= {(UnityTuning.Kill55ReelMarkNextShare * 100).ToString("0", ci)} %; the changed pixels' median H {mh.ToString("0", ci)} S {ms.ToString("0.00", ci)} V {mv.ToString("0.00", ci)}), body rect {rect.width}x{rect.height} px | probe: {probe} | frame shot-N55-K-flash-end.jpg";
            UnityEngine.Object.Destroy(end);
        }
    }
}
