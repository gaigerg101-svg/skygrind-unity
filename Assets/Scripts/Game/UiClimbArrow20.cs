// NIGHT20 U3-34.10 (builder H): the climb's off-screen arrow (NIGHT20.md U3-34.10: "a DANGER chevron at the screen's edge
// toward a climbing enemy above or below you"; GG: "there always really needs to be a route that enemies can climb up").
// N's `climb` events (ClimbEvent: "start" the tell, "up" the climb, "top" the mantle, "knocked" a punch knocked it off)
// feed ClimbTrack20; each frame every followed climber (the enemy by its id, its chest; its face's lip when it's gone) is
// projected through the camera, and one off screen or behind you gets a DANGER chevron pinned Hud20ClimbArrowInset px in
// from the screen's edge, turned toward it (EdgeArrow20), pulsing 0.65 .. 1 (REDUCED MOTION: steady), fading in when it
// appears (M9). A climber on screen has N's own tell, no arrow. Shown only while design.routes is live; in the sight layer
// (the screen's edge: outside S14's box). Up to Hud20ClimbArrows; no allocation per frame.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UiChevron20[] climbArrows20;
        readonly int[] climbArrowId20 = { -1, -1, -1, -1 };
        readonly double[] climbArrowAge20 = new double[UnityTuning.Hud20ClimbArrows];
        readonly ClimbTrack20 climbTrack20 = new ClimbTrack20();
        double climbClock20;

        void BuildClimbArrow20()
        {
            climbArrows20 = new UiChevron20[UnityTuning.Hud20ClimbArrows];
            for (int i = 0; i < climbArrows20.Length; i++)
            {
                var go = new GameObject($"Climb arrow {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiChevron20));
                var rt = (RectTransform)go.transform;
                rt.SetParent(sight, false);
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(UnityTuning.Hud20ClimbArrowW, UnityTuning.Hud20ClimbArrowH);
                UiChevron20 c = go.GetComponent<UiChevron20>();
                c.color = UiTheme.Danger;
                c.raycastTarget = false;
                go.SetActive(false);
                climbArrows20[i] = c;
            }
            game.events.On<ClimbEvent>(N20Events.Climb, e => climbTrack20.On(e, climbClock20));
            game.OnNewRun += () => climbTrack20.Clear();
        }

        void RenderClimbArrow20(double frameDt)
        {
            if (climbArrows20 == null) return;
            bool live = game.sim != null && game.sim.n20.routes && game.UiFlow == Flow.Playing && game.cam != null;
            if (live) climbClock20 += frameDt;
            climbTrack20.Step(climbClock20);
            int shown = 0;
            if (live)
            {
                Camera cam = game.cam;
                float scale = canvas.scaleFactor > 0 ? canvas.scaleFactor : 1f;
                Vector2 half = sight.rect.size * 0.5f;
                Vector2 mid = new Vector2(UnityEngine.Screen.width * 0.5f, UnityEngine.Screen.height * 0.5f);
                bool still = game.settings.View.reducedMotion;
                float pulse = still ? 1f : UnityTuning.Hud20ClimbArrowPulseLow + (1f - UnityTuning.Hud20ClimbArrowPulseLow) * (0.5f + 0.5f * Mathf.Cos((float)(climbClock20 / UnityTuning.Hud20ClimbArrowPulse) * Mathf.PI * 2f));
                for (int i = 0; i < climbTrack20.Count && shown < climbArrows20.Length; i++)
                {
                    Vec3 target = ClimbTarget20(climbTrack20.Id(i), climbTrack20.Lip(i));
                    Vector3 s = cam.WorldToScreenPoint(WebSpace.ToUnity(target));
                    bool behind = s.z < 0;
                    var p = new Vector2((s.x - mid.x) / scale, (s.y - mid.y) / scale);
                    if (!EdgeArrow20.Place(p, behind, half, UnityTuning.Hud20ClimbArrowInset, out Vector2 at, out float angle)) continue;
                    UiChevron20 c = climbArrows20[shown];
                    int id = climbTrack20.Id(i);
                    if (!c.gameObject.activeSelf || climbArrowId20[shown] != id)
                    {
                        c.gameObject.SetActive(true);
                        climbArrowId20[shown] = id;
                        climbArrowAge20[shown] = 0; // M9: it fades in
                    }
                    climbArrowAge20[shown] = HudEntrance18.Step(climbArrowAge20[shown], frameDt);
                    float a = pulse * HudEntrance18.Fade(climbArrowAge20[shown]);
                    RectTransform rt = c.rectTransform;
                    rt.anchoredPosition = at;
                    rt.localRotation = Quaternion.Euler(0, 0, angle);
                    c.color = UiTheme.A(UiTheme.Danger, a);
                    shown++;
                }
            }
            for (int i = shown; i < climbArrows20.Length; i++)
            {
                if (!climbArrows20[i].gameObject.activeSelf) continue;
                climbArrows20[i].gameObject.SetActive(false);
                climbArrowId20[i] = -1;
            }
        }

        /// <summary>The climber's chest (interpolated), or its face's lip when it's no longer on the field.</summary>
        Vec3 ClimbTarget20(int id, Vec3 lip)
        {
            List<Enemy> all = game.enemies != null ? game.enemies.all : null;
            if (all != null)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    Enemy e = all[i];
                    if (e == null || e.id != id || !e.alive) continue;
                    Vec3 p = e.previous + (e.position - e.previous) * game.Alpha;
                    return new Vec3(p.x, p.y + e.AimHeight, p.z);
                }
            }
            return lip;
        }
    }
}
