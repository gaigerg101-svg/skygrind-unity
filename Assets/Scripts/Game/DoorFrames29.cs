// NIGHT29 item 5 (builder V): a door is a FRAME built from its wall opening (DESIGN29.md section A; GG: "gaps in between the major
// doors"). The opening is the door's own rect in the map JSON (`campaign.doors[].rect` = [x0, z0, x1, z1], the gap in the wall ring
// from one room's edge to the next). The frame is built from exactly that rect, so there is nothing between it and the wall: the two
// jambs start AT the opening's edges (x0 and x1, or z0 and z1 for a door in an x wall) and stand DoorJambDepth into it; the lintel
// spans the whole opening under the wall's top (the height of the wall boxes whose end faces touch the opening); two neon lines mark
// each jamb's front faces and the lintel's; two sill lines cross the floor at the opening's two mouths. Nothing here is a collider:
// the opening stays the sim's own (the door's zone, its gate field). View only. Called by LightKit29.Build when design.n29.doors.enabled.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>One door frame's pure geometry (Lights29Tests): the boxes as Unity-space (lo, hi) pairs.</summary>
    public struct DoorFrameParts29
    {
        public Vector3 jamb0Lo, jamb0Hi, jamb1Lo, jamb1Hi, lintelLo, lintelHi;
        public bool spansX; // the opening is wide along x (the passage runs along z)
        public float height;
    }

    public static class DoorFrames29Math
    {
        /// <summary>The frame's parts for a door rect (web [x0, z0, x1, z1]) and a wall height. The rect's longer side is the opening's width, the shorter the wall's thickness; the jambs' outer faces are the opening's edges (gap 0); the lintel covers the whole rect.</summary>
        public static DoorFrameParts29 Parts(double x0, double z0, double x1, double z1, float height, float jambDepth, float lintelH)
        {
            var p = new DoorFrameParts29 { height = height, spansX = (x1 - x0) >= (z1 - z0) };
            // Unity z = -web z
            float ux0 = (float)x0, ux1 = (float)x1, uz0 = (float)-z1, uz1 = (float)-z0;
            if (p.spansX)
            {
                p.jamb0Lo = new Vector3(ux0, 0, uz0); p.jamb0Hi = new Vector3(ux0 + jambDepth, height, uz1);
                p.jamb1Lo = new Vector3(ux1 - jambDepth, 0, uz0); p.jamb1Hi = new Vector3(ux1, height, uz1);
            }
            else
            {
                p.jamb0Lo = new Vector3(ux0, 0, uz0); p.jamb0Hi = new Vector3(ux1, height, uz0 + jambDepth);
                p.jamb1Lo = new Vector3(ux0, 0, uz1 - jambDepth); p.jamb1Hi = new Vector3(ux1, height, uz1);
            }
            p.lintelLo = new Vector3(ux0, height - lintelH, uz0);
            p.lintelHi = new Vector3(ux1, height, uz1);
            return p;
        }
    }

    public static class DoorFrames29
    {
        /// <summary>Builds every door's frame under `parent`; returns the count.</summary>
        public static int Build(SurvivalGame g, Transform parent, SurvivalMaterials mats, Dictionary<string, object> campaign)
        {
            if (!campaign.TryGetValue("doors", out object dobj) || !(dobj is List<object> doors)) return 0;
            GameConfig cfg = g.cfg;
            float maxWall = (float)cfg.Num("design.n29.lights.maxWallDepth");
            float jamb = Mathf.Min(UnityTuning.DoorJambDepth, maxWall);
            Color top = Look.C(cfg, "colors.pillarTop"), side = Look.C(cfg, "colors.pillarSide");
            var stone = new MeshKit();
            var neon = new MeshKit();
            int n = 0;
            foreach (object o in doors)
            {
                if (!(o is Dictionary<string, object> d) || !d.TryGetValue("rect", out object ro) || !(ro is List<object> r) || r.Count < 4) continue;
                double x0 = PortData.Num(r[0]), z0 = PortData.Num(r[1]), x1 = PortData.Num(r[2]), z1 = PortData.Num(r[3]);
                if (x1 <= x0 || z1 <= z0) continue;
                float h = WallHeight(g, x0, z0, x1, z1);
                DoorFrameParts29 p = DoorFrames29Math.Parts(x0, z0, x1, z1, h, jamb, UnityTuning.DoorHeaderH);
                stone.BoxUnity(p.jamb0Lo, p.jamb0Hi, top, side, side, skipBottom: true);
                stone.BoxUnity(p.jamb1Lo, p.jamb1Hi, top, side, side, skipBottom: true);
                stone.BoxUnity(p.lintelLo, p.lintelHi, top, side, side);
                Neon(neon, p, h);
                n++;
            }
            if (n == 0) return 0;
            MeshKit.Object("Door frames 29", parent, stone.Build("door frames29"), mats.stone, shadows: false);
            var m = new Material(mats.neon);
            m.SetColor("_BaseColor", Look.C(cfg, "colors.usable") * (float)cfg.Num("design.n27.roof27.stripBrightness"));
            MeshKit.Object("Door frame neon 29", parent, neon.Build("door neon29"), m, shadows: false);
            return n;
        }

        /// <summary>The top of the wall boxes whose end faces touch the opening's edges (else DoorFallbackH).</summary>
        static float WallHeight(SurvivalGame g, double x0, double z0, double x1, double z1)
        {
            bool spansX = (x1 - x0) >= (z1 - z0);
            double best = 0, t = UnityTuning.DoorWallTouch;
            foreach (MapBox b in g.map.boxes)
            {
                if (b.max.y < 2) continue;
                bool touches = spansX
                    ? (System.Math.Abs(b.max.x - x0) <= t || System.Math.Abs(b.min.x - x1) <= t) && b.max.z > z0 && b.min.z < z1
                    : (System.Math.Abs(b.max.z - z0) <= t || System.Math.Abs(b.min.z - z1) <= t) && b.max.x > x0 && b.min.x < x1;
                if (touches && b.max.y > best) best = b.max.y;
            }
            return best > 0 ? (float)best : UnityTuning.DoorFallbackH;
        }

        /// <summary>The neon: a strip down each jamb's two front faces, one under the lintel on its two faces, a sill line across each mouth. 1 cm proud of the faces.</summary>
        static void Neon(MeshKit k, DoorFrameParts29 p, float h)
        {
            const float proud = 0.01f;
            float nw = UnityTuning.DoorNeonW, sw = UnityTuning.DoorSillW, lift = Light54.Lift(UnityTuning.DoorSillLift); // NIGHT54 light54: zLiftM (off: 12 mm)
            float under = h - UnityTuning.DoorHeaderH;
            Vector3 lo = p.lintelLo, hi = p.lintelHi;
            if (p.spansX)
            {
                float jw = p.jamb0Hi.x - p.jamb0Lo.x;
                for (int side = 0; side < 2; side++)
                {
                    float zf = side == 0 ? lo.z : hi.z;
                    float zo = side == 0 ? lo.z - proud : hi.z + proud;
                    float za = Mathf.Min(zf, zo), zb = Mathf.Max(zf, zo);
                    float c0 = p.jamb0Lo.x + jw * 0.5f, c1 = p.jamb1Lo.x + jw * 0.5f;
                    k.Cube(new Vector3(c0, under * 0.5f, (za + zb) * 0.5f), new Vector3(nw, under, zb - za), Color.white);
                    k.Cube(new Vector3(c1, under * 0.5f, (za + zb) * 0.5f), new Vector3(nw, under, zb - za), Color.white);
                    k.Cube(new Vector3((lo.x + hi.x) * 0.5f, under - nw * 0.5f, (za + zb) * 0.5f), new Vector3(hi.x - lo.x, nw, zb - za), Color.white);
                    float sz = side == 0 ? lo.z + sw * 0.5f : hi.z - sw * 0.5f;
                    k.Cube(new Vector3((lo.x + hi.x) * 0.5f, lift * 0.5f, sz), new Vector3(p.jamb1Hi.x - p.jamb0Lo.x - 2 * jw, lift, sw), Color.white);
                }
            }
            else
            {
                float jw = p.jamb0Hi.z - p.jamb0Lo.z;
                for (int side = 0; side < 2; side++)
                {
                    float xf = side == 0 ? lo.x : hi.x;
                    float xo = side == 0 ? lo.x - proud : hi.x + proud;
                    float xa = Mathf.Min(xf, xo), xb = Mathf.Max(xf, xo);
                    float c0 = p.jamb0Lo.z + jw * 0.5f, c1 = p.jamb1Lo.z + jw * 0.5f;
                    k.Cube(new Vector3((xa + xb) * 0.5f, under * 0.5f, c0), new Vector3(xb - xa, under, nw), Color.white);
                    k.Cube(new Vector3((xa + xb) * 0.5f, under * 0.5f, c1), new Vector3(xb - xa, under, nw), Color.white);
                    k.Cube(new Vector3((xa + xb) * 0.5f, under - nw * 0.5f, (lo.z + hi.z) * 0.5f), new Vector3(xb - xa, nw, hi.z - lo.z), Color.white);
                    float sx = side == 0 ? lo.x + sw * 0.5f : hi.x - sw * 0.5f;
                    k.Cube(new Vector3(sx, lift * 0.5f, (lo.z + hi.z) * 0.5f), new Vector3(sw, lift, p.jamb1Hi.z - p.jamb0Lo.z - 2 * jw), Color.white);
                }
            }
        }
    }
}
