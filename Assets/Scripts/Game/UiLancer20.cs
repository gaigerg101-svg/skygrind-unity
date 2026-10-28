// NIGHT20 U3-34.9 C2 HUD (builder H): the LANCER's off-screen tell (design.adds.lancer: builder E's port of lancer.js, a
// sniper that perches and fires a hitscan beam after a 1.2 s lagging laser). CoD's sniper glint, Halo's threat marker: while
// a LANCER you can't see (off screen or behind you) aims at you, a DANGER chevron (UiChevron20) is pinned Hud20ClimbArrowInset
// px in from the screen's edge toward it (EdgeArrow20), from Hud20LancerFrom alpha on the aim's first frame to whole at the
// shot, swelling in the aim's last 30 % (LancerTell20; REDUCED MOTION: no swell); it goes with the shot or the lancer's
// blink. On screen its own laser (E's view) is the tell. Reads Lancer.State / AimProgress / Muzzle (E's view fields); up to
// Hud20LancerArrows; in the sight layer at the edge (outside S14's box). No allocation per frame.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UiChevron20[] lancerArrows20;
        bool lancerCfg20;

        void BuildLancer20()
        {
            lancerCfg20 = cfg.Design("adds.lancer.enabled");
            lancerArrows20 = new UiChevron20[UnityTuning.Hud20LancerArrows];
            for (int i = 0; i < lancerArrows20.Length; i++)
            {
                var go = new GameObject($"Lancer arrow {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiChevron20));
                var rt = (RectTransform)go.transform;
                rt.SetParent(sight, false);
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(UnityTuning.Hud20LancerW, UnityTuning.Hud20LancerH);
                UiChevron20 c = go.GetComponent<UiChevron20>();
                c.color = UiTheme.Danger;
                c.raycastTarget = false;
                go.SetActive(false);
                lancerArrows20[i] = c;
            }
        }

        void RenderLancer20()
        {
            if (lancerArrows20 == null) return;
            int shown = 0;
            bool live = lancerCfg20 && game.sim != null && game.sim.n20.adds && game.UiFlow == Flow.Playing && game.cam != null && game.enemies != null;
            if (live)
            {
                Camera cam = game.cam;
                float scale = canvas.scaleFactor > 0 ? canvas.scaleFactor : 1f;
                Vector2 half = sight.rect.size * 0.5f;
                var mid = new Vector2(UnityEngine.Screen.width * 0.5f, UnityEngine.Screen.height * 0.5f);
                bool still = game.settings.View.reducedMotion;
                List<Enemy> all = game.enemies.all;
                for (int i = 0; i < all.Count && shown < lancerArrows20.Length; i++)
                {
                    if (!(all[i] is Lancer l) || !l.alive || l.state != Lancer.State.Aim) continue;
                    Vector3 s = cam.WorldToScreenPoint(WebSpace.ToUnity(l.Muzzle));
                    var p = new Vector2((s.x - mid.x) / scale, (s.y - mid.y) / scale);
                    if (!EdgeArrow20.Place(p, s.z < 0, half, UnityTuning.Hud20ClimbArrowInset, out Vector2 at, out float angle)) continue;
                    UiChevron20 c = lancerArrows20[shown++];
                    if (!c.gameObject.activeSelf) c.gameObject.SetActive(true);
                    double progress = l.AimProgress;
                    RectTransform rt = c.rectTransform;
                    rt.anchoredPosition = at;
                    rt.localRotation = Quaternion.Euler(0, 0, angle);
                    float k = LancerTell20.Scale(progress, still);
                    rt.localScale = new Vector3(k, k, 1);
                    c.color = UiTheme.A(UiTheme.Danger, LancerTell20.Alpha(progress));
                }
            }
            for (int i = shown; i < lancerArrows20.Length; i++)
                if (lancerArrows20[i].gameObject.activeSelf) lancerArrows20[i].gameObject.SetActive(false);
        }
    }
}
