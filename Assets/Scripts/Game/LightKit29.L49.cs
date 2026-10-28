// NIGHT49 builder L (design.n49.lights49, a VIEW row; G2-G4, the light side): LightKit29's half of the lights in their roles. LightPlan33.Roles49 names
// every fixture's role ("cyan" usable, "white" the boundary, "orange" danger, "level49" the level accent) and adds the landmark's light; this file
//   - Named49: the role names as the ONE PALETTE's tokens (UnityTuning.Look49.cs) and "level49" as the level's theme key (Look41.KeyHex), guarded:
//     a key more saturated than L49PaleSat and within Look49AccentMinHueGapDeg of any role token is desaturated to L49PaleSat (hue and value kept),
//     so no accent fixture reads as usable, danger, parry, RIVAL or the boundary;
//   - Build49 / Step49: the landmark's own point light (the WICK's furnace, the BRAWL's floor, the KILN's ring, the LOOM's warp, the SPIRE's column),
//     re-set every LightKitRepick so its illuminance AT THE LANDMARK is L49LandmarkOverRoom x the largest any other light in its room puts there
//     (I / d^2 inside the light's range), between L49LandmarkMinIntensity and L49LandmarkMaxIntensity (x LightScale).
// With the row off nothing here runs (Named49 is not asked; the plan has no landmark light). View only: nothing here touches the sim.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class LightKit29
    {
        static readonly uint[] Roles49 = { UnityTuning.Look49UsableHex, UnityTuning.Look49DangerHex, UnityTuning.Look49ParryHex, UnityTuning.Look49RivalHex, UnityTuning.Look49BoundaryHex };

        /// <summary>lights49's colours by role name; false for any other name (the caller's NIGHT48 switch answers it).</summary>
        static bool Named49(string name, int theme41, out Color c)
        {
            switch (name)
            {
                case "cyan": c = Look.Hex(UnityTuning.Look49UsableHex); return true;
                case "white": c = Look.Hex(UnityTuning.Look49BoundaryHex); return true;
                case "orange": c = Look.Hex(UnityTuning.Look49DangerHex); return true;
                case "level49": c = Look.Hex(Accent49Hex(Look41.KeyHex(theme41))); return true;
                default: c = default; return false;
            }
        }

        /// <summary>The level accent from a theme key: the key when it is paler than L49PaleSat or at least Look49AccentMinHueGapDeg from every role
        /// token's hue; else the key desaturated to L49PaleSat (its hue and value kept). Unity-free (Look41.HueSat).</summary>
        public static uint Accent49Hex(uint key)
        {
            Look41.HueSat(key, out double h, out double s);
            if (s < UnityTuning.L49PaleSat) return key;
            bool clear = true;
            foreach (uint r in Roles49)
            {
                Look41.HueSat(r, out double rh, out _);
                if (Look41.HueGap(h, rh) < UnityTuning.Look49AccentMinHueGapDeg) { clear = false; break; }
            }
            if (clear) return key;
            double v = System.Math.Max(((key >> 16) & 255) / 255.0, System.Math.Max(((key >> 8) & 255) / 255.0, (key & 255) / 255.0));
            Color c = Color.HSVToRGB((float)(h / 360.0), (float)UnityTuning.L49PaleSat, (float)v);
            uint R = (uint)Mathf.Clamp(Mathf.RoundToInt(c.r * 255f), 0, 255), G = (uint)Mathf.Clamp(Mathf.RoundToInt(c.g * 255f), 0, 255), B = (uint)Mathf.Clamp(Mathf.RoundToInt(c.b * 255f), 0, 255);
            return (R << 16) | (G << 8) | B;
        }

        Light landmark49;
        Vector3 landmarkTarget49, landmarkDir49 = Vector3.up; // round 2: the light sits on the line target + dir x distance
        float landmarkDist49;
        RoomDef landmarkRoom49;
        /// <summary>For the reel a449: the landmark light's last set (its intensity, the illuminance at the landmark from it and from the room's next strongest
        /// light, that light's name); empty with the row off or no landmark.</summary>
        public static string Landmark49Now = "";
        /// <summary>For the reel a449: the landmark's own illuminance over the room's next strongest at the landmark (>= L49LandmarkOverRoom when it is capped by nothing).</summary>
        public static double Landmark49Ratio;

        void Build49(LightPlan33.Plan plan)
        {
            Landmark49Now = "";
            Landmark49Ratio = 0;
            LightPlan33.Lit lm = null;
            foreach (LightPlan33.Lit l in plan.lights) if (l.kind == "landmark") { lm = l; break; }
            if (lm == null || level == null) return;
            foreach (RoomDef r in level.rooms) if (r.id == lm.room) { landmarkRoom49 = r; break; }
            if (landmarkRoom49 == null) return;
            LightPlan33.Roles49Report rep = LightPlan33.Cached49;
            landmarkTarget49 = rep != null ? U33(rep.tx, rep.ty, rep.tz) : U33(lm.x, lm.y - UnityTuning.L49LandmarkLiftM, lm.z);
            var go = new GameObject("Landmark light 49");
            go.transform.SetParent(transform, false);
            go.transform.position = U33(lm.x, lm.y, lm.z);
            Vector3 off = go.transform.position - landmarkTarget49;
            landmarkDist49 = off.magnitude;
            landmarkDir49 = landmarkDist49 > 1e-3f ? off / landmarkDist49 : Vector3.up;
            landmark49 = go.AddComponent<Light>();
            landmark49.type = LightType.Point;
            landmark49.shadows = LightShadows.None;
            landmark49.range = (float)lm.range;
            landmark49.color = Named(game.cfg, "level49", theme41);
            landmark49.intensity = (float)(UnityTuning.L49LandmarkMinIntensity * lightScale);
        }

        void Step49()
        {
            if (landmark49 == null || landmarkRoom49 == null) return;
            RoomDef r = landmarkRoom49;
            Vector3 t = landmarkTarget49;
            double best = 0;
            string bestName = "none";
            foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l == landmark49 || l.type == LightType.Directional || !l.isActiveAndEnabled || l.intensity <= 0) continue;
                if (l54 && Light54.Transient.Contains(l)) continue; // NIGHT54 light54: a shot's flash or pop never moves the landmark
                Vector3 p = l.transform.position;
                double wz = -p.z; // Unity -> web (z flips)
                if (p.x < r.x0 || p.x > r.x1 || wz < r.z0 || wz > r.z1) continue;
                float d = Vector3.Distance(p, t);
                if (d > l.range) continue;
                double e = l.intensity / System.Math.Max(0.25, (double)d * d);
                if (e > best) { best = e; bestName = l.name; }
            }
            // round 2: the plan's distance (L49LandmarkLiftM) when the cap reaches OverRoom from there; else in along the same line toward the target, to
            // the distance the capped light reaches OverRoom from (never under L49LandmarkMinLiftM): the landmark is lit by its own light, no white-out
            double lo = UnityTuning.L49LandmarkMinIntensity * lightScale, hi = UnityTuning.L49LandmarkMaxIntensity * lightScale;
            double far = System.Math.Max(UnityTuning.L49LandmarkMinLiftM, landmarkDist49);
            double reach = best > 0 ? System.Math.Sqrt(hi / (best * UnityTuning.L49LandmarkOverRoom)) : far;
            float dl = (float)System.Math.Max(UnityTuning.L49LandmarkMinLiftM, System.Math.Min(far, reach));
            double want = best * UnityTuning.L49LandmarkOverRoom * dl * dl;
            double set = System.Math.Min(hi, System.Math.Max(lo, want));
            if (!Land54(t + landmarkDir49 * dl, (float)set)) // NIGHT54 light54 (LightKit29.Light54.cs): a target eased every frame; off: set now, as NIGHT49
            {
                landmark49.transform.position = t + landmarkDir49 * dl;
                landmark49.intensity = (float)set;
            }
            double own = set / (dl * dl);
            Landmark49Ratio = best > 0 ? own / best : double.PositiveInfinity;
            Landmark49Now = $"landmark light {set:0.00} (range {landmark49.range:0} m, {dl:0.0} m from its target at web {t.x:0.0},{t.y:0.0},{-t.z:0.0} in {r.id}):{own:0.000} at the landmark vs the room's next {best:0.000} ({bestName}), x{(best > 0 ? (own / best).ToString("0.00") : "inf")}";
        }
    }
}
