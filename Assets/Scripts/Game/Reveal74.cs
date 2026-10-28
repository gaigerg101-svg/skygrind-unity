// NIGHT74 builder L, slice L2 (view row design.n74.reveal74; the pure half and the rule: Reveal74Math.cs; gg-proxy-close #1: foundry-b1 reads "11 LEFT" with no enemy anywhere in the frame; GG 2026-10-12 "hard
// to understand what to do"). When the room's counter reads N LEFT (N >= 1) and no live enemy has been on screen for 3 s, a red-orange double chevron rides the screen's edge toward the nearest live enemy (red-orange
// is the enemies' colour: CLAUDE.md rule 8); the moment one is on screen it goes (a 0.10 s fade). Never on the bottom band (the gun and the HP plate), no text (the counter already says N LEFT), a slow breath only
// (none with REDUCED MOTION). "On screen" is what the player can see: inside the picture (the camera's viewport) and in line of sight (a ray from the eye to the body or the head point meets no wall first, the
// reels' L56Count rule).
// Self-hooked (Goal72View's way: a hidden object made at scene load finds the live SurvivalGame and hooks its OnRender / OnNewRun; no shared file changes), its own screen-space canvas UNDER the HUD's (sorting 9).
// View only: it reads the sim's public state (the counter's number, the enemies' positions and alive flags) and writes nothing to it; no tape bit, no audio. Read EVERY frame (NightRows.ViewOn: D1's contract). The enemy
// scan (a viewport test and up to two wall rays per live enemy) and the timer run while the row is on, and while the reel's same-run toggle is flipping this row (it holds one frame off: the state runs through it as
// before); with the row off in play the scan is skipped and the timer clears (NIGHT74 review S1; Reveal74Probe.Scans / Skips count it). No allocation per frame (a string is built only when it shows or hides).
// Probe: Reveal74Probe (the Player log's `[reveal74]` lines and the reveal74 reel phase read it). Off: the row off (-skygrindRows72 reveal74:0).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT74 L2: what reveal74 does now and what it counted this run (the reel's and the log's line).</summary>
    public static class Reveal74Probe
    {
        public static bool On, Counting, Wanted, Visible;
        /// <summary>Live enemies, how many were counted on screen this frame, the seconds with nobody on screen (the timer), the chevron's alpha.</summary>
        public static int Alive, OnScreen, Shows, Hides;
        /// <summary>NIGHT74 review S1: frames that ran the enemy scan, and frames the counter was up but the scan was skipped (the row off): a Player run with the row off reads scans 0 and skipped N.</summary>
        public static int Scans, Skips;
        public static double NoneS, Alpha;
        /// <summary>The last show: the nearest enemy's bearing from the heading (0 ahead, + right), its distance, the chevron's screen angle (0 right, 90 up).</summary>
        public static double DirDeg, DistM, AngleDeg;
        /// <summary>The seconds from an enemy coming on screen to the chevron's alpha reaching 0, at the last hide (-1: none yet).</summary>
        public static double GoneAfterS = -1;
        public static int Left = -1;

        public static void Reset()
        {
            Counting = Wanted = Visible = false;
            Alive = OnScreen = Shows = Hides = 0;
            Scans = Skips = 0;
            NoneS = Alpha = 0;
            GoneAfterS = -1;
            Left = -1;
        }

        public static string Line() =>
            $"reveal74 on {On}, counting {Counting} (left {Left}), alive {Alive}, on screen {OnScreen}, nobody on screen for {NoneS:0.0} s, wanted {Wanted}, visible {Visible} (alpha {Alpha:0.00}), shows {Shows}, hides {Hides}, last dir {DirDeg:0} deg at {DistM:0.0} m, gone {GoneAfterS:0.00} s after an enemy came on screen, scans {Scans} skipped {Skips}";
    }

    public sealed class Reveal74View : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Reveal74View>() != null) return;
            var go = new GameObject("Reveal74");
            DontDestroyOnLoad(go);
            go.AddComponent<Reveal74View>();
        }

        SurvivalGame game;
        readonly Reveal74Math.State st = new Reveal74Math.State();
        Canvas canvas;
        RectTransform canvasRect, chevRt;
        Image chevron;
        float alpha, realClock, smoothDeg;
        bool drawnWanted, snap = true;
        double hideStartedAt = -1;

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            st.Reset();
            Reveal74Probe.Reset();
            drawnWanted = false;
            snap = true;
            g.OnRender += Render;
            g.OnNewRun += () =>
            {
                st.Reset();
                Reveal74Probe.Reset();
                drawnWanted = false;
                snap = true;
                alpha = 0f;
            };
        }

        void Render(double frameDt, double frameAlpha)
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null || g.sim == null || g.cfg == null) return;
            realClock += (float)frameDt;
            bool on = NightRows.ViewOn(g.cfg, Reveal74Math.Row); // read every frame (D1)
            Reveal74Probe.On = on;
            SurvivalSim sim = g.sim;

            // the counter: a campaign room's N LEFT (counter43), or a survival round's
            bool playing = g.flow == Flow.Playing && !g.attractOn && !sim.metaSilent;
            int left = -1;
            if (playing)
            {
                if (sim.campaignOn) left = sim.N43RoomLeft;
                else if (g.run.state == SurvivalRun.State.Fight && !g.WyrmFightLive) left = g.director.Remaining;
            }
            bool counting = left >= 1;
            Reveal74Probe.Left = left;
            Reveal74Probe.Counting = counting;

            // the enemies: any live one, any on screen, the nearest
            bool anyVisible = false, hasLive = false;
            Enemy nearest = null;
            double nearestD = double.MaxValue;
            int alive = 0;
            Camera cam = g.cam;
            // NIGHT74 review S1: with the row off the scan (a viewport test and up to two wall rays per live enemy) is skipped; only the reel's same-run toggle on this row keeps it running (D1)
            bool scan = counting && Reveal74Math.ShouldScan(on, NightRows.ReelOverride.Count > 0 && NightRows.ReelOverride.ContainsKey(Reveal74Math.Row));
            if (counting && !scan) Reveal74Probe.Skips++;
            if (scan)
            {
                Reveal74Probe.Scans++;
                Vec3 eye = WebSpace.FromUnity(cam.transform.position);
                foreach (Enemy e in sim.enemies.all)
                {
                    if (!e.alive || e.standIn29) continue;
                    alive++;
                    hasLive = true;
                    Vec3 bodyAt = e.position + new Vec3(0, UnityTuning.L56BodyH, 0);
                    double dist = (bodyAt - eye).Length;
                    if (dist < nearestD) { nearestD = dist; nearest = e; }
                    if (anyVisible) continue; // one is enough to say the picture has an enemy: the rest are only counted
                    for (int k = 0; k < 2; k++)
                    {
                        Vec3 t = e.position + new Vec3(0, k == 0 ? UnityTuning.L56BodyH : UnityTuning.L56HeadH, 0);
                        Vector3 vp = cam.WorldToViewportPoint(WebSpace.ToUnity(t));
                        if (vp.z <= 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) continue;
                        Vec3 d = t - eye;
                        double len = d.Length;
                        if (len < 1e-6 || !g.world.RayIntersect(eye, d / len, out RayHit hit) || hit.distance >= len - UnityTuning.L56RaySlack) { anyVisible = true; break; }
                    }
                }
            }
            int seen = anyVisible ? 1 : 0; // "at least one": the scan stops looking once the picture has an enemy
            Reveal74Probe.Alive = alive;
            Reveal74Probe.OnScreen = seen;
            bool want = st.StepOrClear(frameDt, counting, scan, hasLive, anyVisible, UnityTuning.N74RevealDelayS);
            Reveal74Probe.NoneS = st.NoneS;
            Reveal74Probe.Wanted = want;

            bool draw = on && want;
            if (draw) hideStartedAt = -1; // wanted again before the fade-out finished: that fade does not count
            // wanted no more while the chevron was drawn (an enemy came on screen, the counter went, the row went off): the clock for "gone within 0.2 s" starts
            if (drawnWanted && !draw && hideStartedAt < 0) hideStartedAt = realClock;
            if (!draw && alpha <= 0.001f && canvas == null) { Reveal74Probe.Visible = false; Reveal74Probe.Alpha = 0; drawnWanted = false; return; } // never drawn: nothing to build
            if (!UiTheme.Ready) return;
            EnsureCanvas();
            float target = draw ? 1f : 0f;
            alpha = Mathf.MoveTowards(alpha, target, (float)frameDt / (target > alpha ? UnityTuning.N74RevealFadeInS : UnityTuning.N74RevealFadeOutS));
            Reveal74Probe.Alpha = alpha;

            if (draw && !drawnWanted) // shown: the first frame it is wanted and the row is on
            {
                drawnWanted = true;
                hideStartedAt = -1;
                snap = true;
                if (nearest != null)
                {
                    Vec3 feet = g.Feet;
                    Reveal74Probe.DirDeg = Reveal74Math.BearingDeg(feet.x, feet.z, g.input.yaw, nearest.position.x, nearest.position.z);
                    Reveal74Probe.DistM = nearestD;
                }
                Reveal74Probe.Shows++;
                Debug.Log($"[reveal74] shown dir={Reveal74Probe.DirDeg:0} deg dist={Reveal74Probe.DistM:0.0} m left={left} alive={alive}: no enemy on screen for {st.NoneS:0.0} s");
            }
            if (alpha <= 0.001f)
            {
                if (chevron.enabled) chevron.enabled = false;
                if (drawnWanted && !draw) // the fade-out has run: the line says why, once
                {
                    drawnWanted = false;
                    Reveal74Probe.Hides++;
                    Reveal74Probe.GoneAfterS = hideStartedAt >= 0 ? realClock - hideStartedAt : 0;
                    Debug.Log($"[reveal74] hidden {Reveal74Probe.GoneAfterS:0.00} s after {(!on ? "the row went off" : anyVisible ? "an enemy came on screen" : counting ? "the last enemy went" : "the counter went")}; alive={alive}");
                    hideStartedAt = -1;
                }
                Reveal74Probe.Visible = false;
                return;
            }

            // where: the nearest enemy's direction in the camera's own frame, on the screen's edge rectangle
            if (nearest != null)
            {
                Vector3 local = cam.transform.InverseTransformPoint(WebSpace.ToUnity(nearest.position + new Vec3(0, UnityTuning.L56BodyH, 0)));
                double ang = Reveal74Math.Angle(local.x, local.y, local.z);
                float deg = (float)(ang * 180 / Math.PI);
                smoothDeg = snap ? deg : Mathf.LerpAngle(smoothDeg, deg, 1f - Mathf.Exp(-UnityTuning.N74RevealFollow * (float)frameDt)); // a hand-over to another enemy glides
                snap = false;
            }
            float w2 = canvasRect.rect.width, h2 = canvasRect.rect.height;
            Reveal74Math.EdgePoint(smoothDeg * Mathf.Deg2Rad, w2 * 0.5 - UnityTuning.N74RevealEdgeInsetPx, h2 * 0.5 - UnityTuning.N74RevealEdgeInsetPx, UnityTuning.N74RevealLowestPx, out double px, out double py);
            float breath = !g.cfg.movement.settings.reducedMotion ? 1f + UnityTuning.N74RevealPulseDepth * 0.5f * (1f + Mathf.Sin(realClock * UnityTuning.N74RevealPulseHz * 2f * Mathf.PI)) : 1f;
            chevron.enabled = true;
            chevRt.anchoredPosition = new Vector2((float)px, (float)py);
            chevRt.localEulerAngles = new Vector3(0, 0, smoothDeg - 90f); // the sprite points up; turned by (angle - 90) it points outward along the direction
            chevRt.localScale = new Vector3(breath, breath, 1f);
            chevron.color = new Color(1, 1, 1, alpha);
            Reveal74Probe.Visible = true;
            Reveal74Probe.AngleDeg = smoothDeg;
        }

        // --- building (once) ---------------------------------------------------------------------------------------------------------------------

        void EnsureCanvas()
        {
            if (canvas != null) return;
            var go = new GameObject("Reveal74 canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.N74RevealSortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f; // px at 1080p
            canvasRect = (RectTransform)go.transform;
            Sprite sprite = Sprite.Create(ChevronTexture(), new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
            var cg = new GameObject("Reveal74 chevron", typeof(RectTransform), typeof(Image));
            cg.transform.SetParent(go.transform, false);
            chevron = cg.GetComponent<Image>();
            chevron.sprite = sprite;
            chevron.raycastTarget = false;
            chevron.preserveAspect = true;
            chevRt = chevron.rectTransform;
            chevRt.anchorMin = chevRt.anchorMax = chevRt.pivot = new Vector2(0.5f, 0.5f);
            chevRt.sizeDelta = new Vector2(UnityTuning.N74RevealChevronPx, UnityTuning.N74RevealChevronPx);
            chevron.enabled = false;
        }

        /// <summary>Two chevrons pointing up (the second smaller and fainter, behind the first), the enemies' red-orange with a dark ink rim (it reads on level 2's white ice and on the dark rooms).</summary>
        static Texture2D ChevronTexture()
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Reveal74 chevron" };
            var px = new Color32[n * n];
            Color red = UiTheme.Danger;
            Color ink = new Color32(0x0B, 0x10, 0x18, 255);
            // chevron 1: the polyline A-B-C, stroke half-width 0.085; chevron 2: the same shifted down 0.30, scale 0.8, 55 % alpha
            Vector2 A = new Vector2(0.10f, 0.50f), B = new Vector2(0.50f, 0.92f), C = new Vector2(0.90f, 0.50f);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    var p = new Vector2((x + 0.5f) / n, (y + 0.5f) / n);
                    float d1 = Mathf.Min(Seg(p, A, B), Seg(p, B, C));
                    var q = new Vector2(0.5f + (p.x - 0.5f) / 0.8f, 0.5f + (p.y + 0.27f - 0.5f) / 0.8f); // the second chevron's own frame
                    float d2 = Mathf.Min(Seg(q, A, B), Seg(q, B, C)) * 0.8f;
                    float hw = 0.085f, rim = 0.03f;
                    float f1 = Mathf.Clamp01((hw - d1) / 0.02f), r1 = Mathf.Clamp01((hw + rim - d1) / 0.02f);
                    float f2 = Mathf.Clamp01((hw * 0.8f - d2) / 0.02f) * 0.55f, r2 = Mathf.Clamp01((hw * 0.8f + rim - d2) / 0.02f) * 0.55f;
                    float fill = Mathf.Max(f1, f2), outer = Mathf.Max(r1, r2);
                    Color col = Color.Lerp(ink, red, fill > 0.001f ? Mathf.Clamp01(fill / Mathf.Max(0.001f, outer)) : 0f);
                    px[y * n + x] = new Color32((byte)(col.r * 255), (byte)(col.g * 255), (byte)(col.b * 255), (byte)(Mathf.Clamp01(outer * 0.97f) * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        /// <summary>The distance of p from the segment a-b.</summary>
        static float Seg(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return (p - (a + ab * t)).magnitude;
        }
    }
}
