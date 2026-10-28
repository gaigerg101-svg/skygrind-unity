// NIGHT47 builder F (foes47, G12): the WARDEN's shield as an arc of the sim's own block cone (Core/Warden.cs: +-60 deg from the facing,
// UnityTuning.Shield47ArcDeg) at Shield47ArcR m round the body's axis, so the drawn cover is the cover the sim gives. Built in the shield's UNIT
// space (each axis divided by the shield's localScale: Shield47W x Shield47H x Shield47Thick), its origin at the arc's front centre, so the
// part keeps its scale (and the models43 frame on it) and the world shape is the metre arc. Built once.
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class Mesh43
    {
        static Mesh shieldArc47;

        public static Mesh ShieldArc47()
        {
            if (shieldArc47 != null) return shieldArc47;
            float r = UnityTuning.Shield47ArcR, t = UnityTuning.Shield47Thick, hh = UnityTuning.Shield47H * 0.5f;
            var scale = new Vector3(UnityTuning.Shield47W, UnityTuning.Shield47H, UnityTuning.Shield47Thick);
            float half = UnityTuning.Shield47ArcDeg * Mathf.Deg2Rad;
            const int segs = 18;
            var b = new B();
            // a metre point (the axis at z = -r, the front centre at the origin) -> the unit space; a metre normal -> the unit space's
            Vector3 P(float a, float rad, float y) => new Vector3(Mathf.Sin(a) * rad / scale.x, y / scale.y, (Mathf.Cos(a) * rad - r) / scale.z);
            Vector3 N(Vector3 n) => new Vector3(n.x * scale.x, n.y * scale.y, n.z * scale.z);
            for (int face = 0; face < 2; face++)
            {
                float rad = face == 0 ? r + t : r;
                int baseIdx = -1;
                for (int i = 0; i <= segs; i++)
                {
                    float a = -half + 2 * half * i / segs;
                    Vector3 dir = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
                    Vector3 n = N(face == 0 ? dir : -dir);
                    int i0 = b.Add(P(a, rad, -hh), n);
                    b.Add(P(a, rad, hh), n);
                    if (baseIdx < 0) baseIdx = i0;
                }
                for (int i = 0; i < segs; i++)
                {
                    int a0 = baseIdx + i * 2;
                    b.Quad(a0, a0 + 1, a0 + 3, a0 + 2);
                }
            }
            for (int side = 0; side < 2; side++)
            {
                float y = side == 0 ? hh : -hh;
                Vector3 n = N(side == 0 ? Vector3.up : Vector3.down);
                int baseIdx = -1;
                for (int i = 0; i <= segs; i++)
                {
                    float a = -half + 2 * half * i / segs;
                    int i0 = b.Add(P(a, r, y), n);
                    b.Add(P(a, r + t, y), n);
                    if (baseIdx < 0) baseIdx = i0;
                }
                for (int i = 0; i < segs; i++)
                {
                    int a0 = baseIdx + i * 2;
                    b.Quad(a0, a0 + 1, a0 + 3, a0 + 2);
                }
            }
            for (int s = 0; s < 2; s++)
            {
                float a = s == 0 ? -half : half;
                Vector3 n = N(new Vector3(Mathf.Cos(a), 0, -Mathf.Sin(a)) * (s == 0 ? -1 : 1));
                int i0 = b.Add(P(a, r, -hh), n);
                b.Add(P(a, r + t, -hh), n);
                b.Add(P(a, r + t, hh), n);
                b.Add(P(a, r, hh), n);
                b.Quad(i0, i0 + 1, i0 + 2, i0 + 3);
            }
            shieldArc47 = b.ToMesh();
            shieldArc47.name = "mesh43 shield arc 47";
            return shieldArc47;
        }

        static Mesh legs47;

        /// <summary>NIGHT47 F foes47 (G14; F-cards 5c "four short pointed legs (triangles: danger) splayed wide under it"): the swarmer's legs as four
        /// curved spikes (Mesh43.Spike, tip up in cube space) turned tip-down and splayed Swarmer47LegSplayDeg outward from the four corners of the
        /// legs' unit box, in that unit space (the legs part's scale makes them short): one mesh for the legs' one renderer (Models43Tests' budget).</summary>
        public static Mesh Legs47()
        {
            if (legs47 != null) return legs47;
            Mesh spike = Get(Spike);
            var parts = new (Mesh, Matrix4x4)[4];
            int k = 0;
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                {
                    var corner = new Vector3(sx * UnityTuning.Swarmer47LegX, 0, sz * UnityTuning.Swarmer47LegZ);
                    // tip down (180 about x), then the tip swung outward: about z by the x side, about x by the z side
                    Quaternion r = Quaternion.Euler(-sz * UnityTuning.Swarmer47LegSplayDeg, 0, sx * UnityTuning.Swarmer47LegSplayDeg) * Quaternion.Euler(180, 0, 0);
                    parts[k++] = (spike, Matrix4x4.TRS(corner, r, new Vector3(UnityTuning.Swarmer47LegW, 1f, UnityTuning.Swarmer47LegW)));
                }
            legs47 = Combine("mesh43 swarmer legs 47", parts);
            return legs47;
        }
    }
}
