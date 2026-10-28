// NIGHT33 L (design.n33.look33; DESIGN33 L round 2): the meshes of L1's six fixture types (wall sconce, recessed strip, pole lamp, flush ceiling panel, brazier,
// and the trims / guide lamps / frames), from LightPlan33's boxes. View only: no collider, no sim read. Emissive parts are grouped by (colour, HDR) into one
// mesh each on a copy of the shipped neon material (_BaseColor = colour x HDR, HDR from the plan: <= 4); the dark parts (bodies, frames, trims of a strip)
// are one stone mesh in a darkened pillar colour. The danger parts (orange) get their own material so Vfx33 can flicker them (danger fixtures only).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Fixtures33
    {
        /// <summary>NIGHT35 V: LightPlan33 (Unity-free, also in the harness) reads design.n35.lights35 through this hook, set before the first scene loads.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Hook35() => LightPlan33.Lights35On = cfg => N35Fixes.ViewRowOn(cfg, "lights35");

        /// <summary>Counts for the reel line.</summary>
        public struct Counts { public int emissive, dark, meshes; }

        /// <summary>Builds every fix of the plan that `take` accepts (beams and roof strips have their own builders) under `parent`. `colour` names a plan colour.
        /// `danger` collects the materials of orange parts (for the flicker).</summary>
        public static Counts Build(Transform parent, SurvivalMaterials mats, GameConfig cfg, LightPlan33.Plan plan, Func<LightPlan33.Fix, bool> take, Func<string, Color> colour, List<Material> danger)
        {
            var c = new Counts();
            var lit = new Dictionary<string, MeshKit>();
            var litCol = new Dictionary<string, Color>();
            var dark = new MeshKit();
            Color shade = Look.C(cfg, "colors.pillarSide") * UnityTuning.Look33FrameShade;
            bool decor54 = Decor54.BeginFixtures(cfg); // NIGHT54 D (decor54, Decor54.cs): the floor-perimeter trims in the level's accent; off: NIGHT53's groups
            foreach (LightPlan33.Fix f in plan.fixes)
            {
                if (f.kind == "beam" || f.kind == "roofStrip" || !take(f)) continue;
                var lo = new Vec3(f.box.x0, f.box.y0, f.box.z0);
                var hi = new Vec3(f.box.x1, f.box.y1, f.box.z1);
                if (!f.emissive)
                {
                    dark.Box(lo, hi, shade, shade);
                    c.dark++;
                    continue;
                }
                double hdr = Math.Min(f.hdr > 0 ? f.hdr : 2.2, UnityTuning.Look33HdrMax); // L1 check 3: never over HDR 4
                string col = Decor54.FixColour(f, f.color ?? "cyan", decor54);
                string key = col + "|" + hdr.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                if (!lit.TryGetValue(key, out MeshKit k)) { lit[key] = k = new MeshKit(); litCol[key] = (col == Decor54.FixKey ? Decor54.FixAccent(cfg) : colour(col)) * (float)hdr; }
                k.Box(lo, hi, Color.white, Color.white);
                c.emissive++;
            }
            foreach (var kv in lit)
            {
                var m = new Material(mats.neon) { name = "Fixtures 33 " + kv.Key };
                m.SetColor("_BaseColor", litCol[kv.Key]);
                MeshKit.Object("Fixtures 33 " + kv.Key, parent, kv.Value.Build("fixtures33 " + kv.Key), m, shadows: false);
                if (danger != null && kv.Key.StartsWith("orange|")) danger.Add(m);
                c.meshes++;
            }
            if (dark.Vertices > 0) { MeshKit.Object("Fixture bodies 33", parent, dark.Build("bodies33"), mats.stone, shadows: false); c.meshes++; }
            return c;
        }
    }
}
