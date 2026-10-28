// NIGHT38 builder S2 (stag38's READ PASS; PLAN.md section 5: "a read pass (where it is, what's coming)"; GG 2026-10-13: "it's just hard to really
// know what's happening"). View only, while the run's stag38 bit is on (sim.n38.stag38; off with --off38 / -skygrindOff38):
//  - WHERE IT IS: a red-orange chevron (rule 8: an enemy) at the screen's edge pointing to THE STAG while it is off screen or behind you (the climb
//    arrow's placement, EdgeArrow20), pulsing; steady and full while it winds up an attack (a tell is coming from there).
//  - WHAT'S COMING: every RED lane warning brighter (Warn38: a floe's telegraph, the shard kick's lane strip, the lane cut's stripe and barrier,
//    x UnityTuning.Stag38WarnGain), and a told floe gets a BEACON, a thin red column standing on it, seen over the 5 m lane walls and the island.
// Numbers: UnityTuning.Stag38.cs. The arrow is a UiChevron20 in the HUD's sight layer ("HUD sight"), found by name once (as Boss37StaggerBar
// finds the boss bar); no HUD (a reel without it): no arrow.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Stag37View
    {
        RectTransform sight38;
        Canvas canvas38;
        UiChevron20 arrow38;
        float arrowFind38;
        readonly System.Collections.Generic.List<Renderer> beaconRs38 = new System.Collections.Generic.List<Renderer>();

        /// <summary>Frames the reel reads: the arrow drawn now, the beacons up now.</summary>
        public bool Arrow38Shown => arrow38 != null && arrow38.gameObject.activeSelf;
        public int Beacons38;

        /// <summary>The RED warnings' gain factor: UnityTuning.Stag38WarnGain with the run's stag38 bit on, else 1 (NIGHT37's look).</summary>
        static float Warn38(SurvivalSim sim) => sim != null && sim.n38.stag38 ? UnityTuning.Stag38WarnGain : 1f;

        /// <summary>A told floe's beacon (made with the floe, hidden until it is told).</summary>
        void Beacon38(GameObject floe, Vec3 centreTop)
        {
            Renderer b = Glow(floe.transform, "beacon38", boxMesh, out Transform bt);
            bt.position = U(centreTop) + new Vector3(0, UnityTuning.Stag38BeaconHeight * 0.5f, 0);
            bt.localScale = new Vector3(UnityTuning.Stag38BeaconWidth, UnityTuning.Stag38BeaconHeight, UnityTuning.Stag38BeaconWidth);
            bt.gameObject.SetActive(false);
            beaconRs38.Add(b);
        }

        /// <summary>Every frame of the canal, after the floes: the beacons of the told floes.</summary>
        void RenderBeacons38(SurvivalSim sim, int i, bool told, float prog)
        {
            if (i >= beaconRs38.Count) return;
            Renderer b = beaconRs38[i];
            bool on = told && sim.n38.stag38;
            if (b.gameObject.activeSelf != on) b.gameObject.SetActive(on);
            if (!on) return;
            Beacons38++;
            Tint(b, Look.Hex(UnityTuning.Canal37DangerHex, UnityTuning.Stag38BeaconGain * Mathf.Lerp(0.4f, 1f, prog) * (0.8f + 0.2f * Mathf.Sin(t * 18f))));
        }

        bool FindSight38()
        {
            if (sight38 != null) return true;
            if (Time.unscaledTime < arrowFind38) return false;
            arrowFind38 = Time.unscaledTime + 1f;
            foreach (RectTransform rt in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (rt.name == "HUD sight") { sight38 = rt; break; }
            if (sight38 == null) return false;
            canvas38 = sight38.GetComponentInParent<Canvas>();
            var go = new GameObject("Stag38 arrow", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiChevron20));
            var r = (RectTransform)go.transform;
            r.SetParent(sight38, false);
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(UnityTuning.Stag38ArrowW, UnityTuning.Stag38ArrowH);
            arrow38 = go.GetComponent<UiChevron20>();
            arrow38.color = UiTheme.Danger;
            arrow38.raycastTarget = false;
            go.SetActive(false);
            return true;
        }

        void HideArrow38()
        {
            if (arrow38 != null && arrow38.gameObject.activeSelf) arrow38.gameObject.SetActive(false);
        }

        /// <summary>Every frame of the fight view: THE STAG's off-screen arrow.</summary>
        void RenderArrow38(SurvivalSim sim)
        {
            Stag37 g = sim.stag37;
            bool live = sim.n38.stag38 && g != null && g.Alive && game.cam != null && game.UiFlow == Flow.Playing;
            if (!live || !FindSight38()) { HideArrow38(); return; }
            Camera cam = game.cam;
            Vec3 aim = g.body.position + new Vec3(0, UnityTuning.Stag38ArrowAimY, 0);
            Vector3 s = cam.WorldToScreenPoint(WebSpace.ToUnity(aim));
            float scale = canvas38 != null && canvas38.rootCanvas != null && canvas38.rootCanvas.scaleFactor > 0 ? canvas38.rootCanvas.scaleFactor : 1f;
            Vector2 half = sight38.rect.size * 0.5f;
            var p = new Vector2((s.x - Screen.width * 0.5f) / scale, (s.y - Screen.height * 0.5f) / scale);
            if (!EdgeArrow20.Place(p, s.z < 0, half, UnityTuning.Stag38ArrowInset, out Vector2 at, out float angle)) { HideArrow38(); return; }
            if (!arrow38.gameObject.activeSelf) arrow38.gameObject.SetActive(true);
            RectTransform rt = arrow38.rectTransform;
            rt.anchoredPosition = at;
            rt.localRotation = Quaternion.Euler(0, 0, angle);
            bool winding = g.brain.state == Boss37State.Tell || g.brain.state == Boss37State.Active;
            bool still = game.settings.View.reducedMotion;
            float a = winding || still ? 1f : UnityTuning.Stag38ArrowPulseLow + (1f - UnityTuning.Stag38ArrowPulseLow) * (0.5f + 0.5f * Mathf.Cos(t / UnityTuning.Stag38ArrowPulse * Mathf.PI * 2f));
            arrow38.color = UiTheme.A(UiTheme.Danger, a);
        }
    }
}
