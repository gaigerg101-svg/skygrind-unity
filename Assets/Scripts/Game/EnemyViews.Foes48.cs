// NIGHT48 builder F (design.n48.foes48, a view row; NIGHT48.md G12-G14; GG 2026-10-18: "The shield on the big shield enemy is positioned
// awkwardly and looks ugly"). NIGHT47's foes47 shield, read in code tonight (the lineup could not show it: the enemies sat small behind the
// viewmodel): the 0.35 x 1.3 m arm was turned 62 deg forward about its own centre at (0.55, 1.15, 0.38), so its far end stood at z ~0.95 m,
// out through the shield's front face (0.71 m at x 0.55) by ~0.25 m at chest height; and DOWN the shield turned flat along the body's side and
// tipped 22 deg with its BOTTOM in, so its lower third (x 0.52-0.60 m) cut into the body's lower half (0.62-0.66 m wide there). With foes48 on:
//   - UP: the same arc of the sim's +-60 deg block cone round the body's axis, centred on the chest (UnityTuning.Shield48*), its inner radius
//     0.84 m (0.09 m outside the body at the side); the forearm runs from the shoulder (under the pauldron) to a grip BEHIND the shield (its end
//     face 0.16 m inside the inner face), sized to that segment: it holds the shield and never pokes out the front;
//   - DOWN (stunned / opened): the same arc swings round the body's axis to its right side (at radius 0.84 it never cuts the body on the way),
//     its bottom edge just off the floor, its top tipped 20 deg out about that edge (it clears the pauldron, which reaches x 1.05 m at y 1.59 m);
//     the hand follows to the inner face;
//   - the swing eased (SmoothDamp, Shield48SwingS), never a snap; a block kicks the shield back Shield47KickDeg; the tell / swing pitches the
//     shield and the arm together about the shoulder (Shield48BashK x the NIGHT20 arm angle): a shield bash, not an arm leaving its shield.
// Measured in the a948 lineup (Foes48.Probe): the shield's vertices against the body's drawn capsule (the least clearance in m, how many inside),
// the arm's corners out past the shield's front face, the shield's centre (x off the axis, y as a share of the height). The sim's shield
// (Core/Warden.cs) is untouched: a view row. Off: foes47's (or NIGHT46's) shield. Numbers: UnityTuning.F48.cs (Shield48*).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>foes48's switch (the a948 lineup forces it per pass) and the lineup's shield probe.</summary>
    public static class Foes48
    {
        public static bool? Force;
        static GameConfig readFor;
        static bool on;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = NightRows.ViewOn(cfg, "foes48"); }
            return on;
        }

        /// <summary>The reel's probe (a948 sets it; off in play): the last warden drawn's shield measured every frame while on.</summary>
        public static bool Probe;
        public static float ShieldClearM, ShieldCentreX, ShieldCentreYShare, ShieldTopY, ShieldBottomY;
        public static int ShieldInside, ShieldVerts, ArmOutFront;
        public static string ShieldMode = "-";
    }

    public static partial class Mesh43
    {
        static Mesh shieldArc48;

        /// <summary>NIGHT48 F foes48: ShieldArc47's arc at UnityTuning.Shield48ArcR (the same unit space: Shield47W x Shield47H x Shield47Thick).</summary>
        public static Mesh ShieldArc48()
        {
            if (shieldArc48 != null) return shieldArc48;
            shieldArc48 = ArcIn(UnityTuning.Shield48ArcR);
            shieldArc48.name = "mesh43 shield arc 48";
            return shieldArc48;
        }

        static Mesh arcFrame48;

        public static Mesh ShieldArcFrame48IfMade() => arcFrame48;

        /// <summary>Round 5: the arc's FRAME in the arc's unit space (ShieldArc48's): rectangular tubes Shield48FrameW m across along its top and
        /// bottom edges and up its two side edges, standing Shield48FrameProud m proud of its outer face (one mesh: the frame child's one renderer).</summary>
        public static Mesh ShieldArcFrame48()
        {
            if (arcFrame48 != null) return arcFrame48;
            float r = UnityTuning.Shield48ArcR, t = UnityTuning.Shield47Thick, hh = UnityTuning.Shield47H * 0.5f;
            var scale = new Vector3(UnityTuning.Shield47W, UnityTuning.Shield47H, UnityTuning.Shield47Thick);
            float half = UnityTuning.Shield47ArcDeg * Mathf.Deg2Rad, fw = UnityTuning.Shield48FrameW;
            float rIn = r - 0.01f, rOut = r + t + UnityTuning.Shield48FrameProud;
            var b = new B();
            Vector3 P(float a, float rad, float y) => new Vector3(Mathf.Sin(a) * rad / scale.x, y / scale.y, (Mathf.Cos(a) * rad - r) / scale.z);
            Vector3 N(Vector3 n) => new Vector3(n.x * scale.x, n.y * scale.y, n.z * scale.z);
            const int segs = 18;
            // the top and bottom edges: a box tube along the arc (4 faces)
            foreach (float yc in new[] { hh - fw * 0.5f, -hh + fw * 0.5f })
                for (int face = 0; face < 4; face++)
                {
                    int baseIdx = -1;
                    for (int i = 0; i <= segs; i++)
                    {
                        float a = -half + 2 * half * i / segs;
                        var dir = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
                        Vector3 p0, p1, n;
                        if (face == 0) { p0 = P(a, rOut, yc - fw * 0.5f); p1 = P(a, rOut, yc + fw * 0.5f); n = N(dir); }
                        else if (face == 1) { p0 = P(a, rIn, yc + fw * 0.5f); p1 = P(a, rIn, yc - fw * 0.5f); n = N(-dir); }
                        else if (face == 2) { p0 = P(a, rIn, yc + fw * 0.5f); p1 = P(a, rOut, yc + fw * 0.5f); n = N(Vector3.up); }
                        else { p0 = P(a, rOut, yc - fw * 0.5f); p1 = P(a, rIn, yc - fw * 0.5f); n = N(Vector3.down); }
                        int i0 = b.Add(p0, n);
                        b.Add(p1, n);
                        if (baseIdx < 0) baseIdx = i0;
                    }
                    for (int i = 0; i < segs; i++) { int a0 = baseIdx + i * 2; b.Quad(a0, a0 + 1, a0 + 3, a0 + 2); }
                }
            // the two side edges: a straight box up each end of the arc (its width along the arc, its depth across the thickness)
            foreach (float s in new[] { -1f, 1f })
            {
                float a1 = s * half, a0 = s * (half - fw / Mathf.Max(0.1f, r));
                Vector3[] c =
                {
                    P(a0, rIn, -hh), P(a1, rIn, -hh), P(a1, rOut, -hh), P(a0, rOut, -hh),
                    P(a0, rIn, hh), P(a1, rIn, hh), P(a1, rOut, hh), P(a0, rOut, hh),
                };
                int[][] quads = { new[] { 3, 2, 6, 7 }, new[] { 1, 0, 4, 5 }, new[] { 0, 3, 7, 4 }, new[] { 2, 1, 5, 6 } };
                Vector3 boxC = Vector3.zero;
                foreach (Vector3 v in c) boxC += v / 8f;
                foreach (int[] q in quads)
                {
                    Vector3 nn = Vector3.Cross(c[q[1]] - c[q[0]], c[q[3]] - c[q[0]]);
                    Vector3 qc = (c[q[0]] + c[q[1]] + c[q[2]] + c[q[3]]) * 0.25f;
                    if (Vector3.Dot(nn, qc - boxC) < 0) nn = -nn; // outward (B.Tri winds each triangle to its normal)
                    int i0 = b.Add(c[q[0]], nn); b.Add(c[q[1]], nn); b.Add(c[q[2]], nn); b.Add(c[q[3]], nn);
                    b.Quad(i0, i0 + 1, i0 + 2, i0 + 3);
                }
            }
            arcFrame48 = b.ToMesh();
            arcFrame48.name = "mesh43 shield arc frame 48";
            return arcFrame48;
        }

        static Mesh ArcIn(float r)
        {
            float t = UnityTuning.Shield47Thick, hh = UnityTuning.Shield47H * 0.5f;
            var scale = new Vector3(UnityTuning.Shield47W, UnityTuning.Shield47H, UnityTuning.Shield47Thick);
            float half = UnityTuning.Shield47ArcDeg * Mathf.Deg2Rad;
            const int segs = 18;
            var b = new B();
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
            return b.ToMesh();
        }
    }

    public sealed partial class WardenView
    {
        // round 3 (the n48-r2 lineup pairs, read: at 3 m and 10 m the warden's own BODY is drawn over its shield as the x-ray silhouette, in both
        // the old and the new model): the warden is an x-ray elite (EnemyLook.Attach(body, xray, elite): a second material, ZTest Greater, in the
        // transparent queue Transparent + EnemyXrayQueueOffset), drawn AFTER the opaque shield, so wherever the shield stands in front of the body
        // the body's x-ray paints over the shield. With foes48 on the shield draws with a copy of the body material in the queue right after the
        // x-ray's (it is opaque, writes depth, and covers the x-ray where it is in front); with the row off, the shared body material back.
        Material shieldMat48, shieldMatWas48, frameMatWas48;

        void Shield48Queue(Body b, bool on)
        {
            Renderer r = b.rends[1];
            if (r == null) return;
            if (on)
            {
                if (shieldMat48 == null)
                {
                    if (r.sharedMaterial == null) return;
                    shieldMatWas48 = r.sharedMaterial;
                    shieldMat48 = new Material(shieldMatWas48) { name = "Warden shield 48", renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + UnityTuning.EnemyXrayQueueOffset + UnityTuning.Shield48QueueAfterXray };
                }
                if (r.sharedMaterial != shieldMat48) r.sharedMaterial = shieldMat48;
                // round 5: the frame child (the arc's frame) draws after the x-ray too, or the body would paint over the rim
                for (int ci = 0; ci < b.shield.childCount; ci++)
                {
                    Renderer cr = b.shield.GetChild(ci).GetComponent<Renderer>();
                    if (cr == null || cr.sharedMaterial == shieldMat48 || cr.sharedMaterial == null) continue;
                    if (frameMatWas48 == null) frameMatWas48 = cr.sharedMaterial;
                    cr.sharedMaterial = shieldMat48;
                }
            }
            else
            {
                if (shieldMat48 != null && r.sharedMaterial == shieldMat48 && shieldMatWas48 != null) r.sharedMaterial = shieldMatWas48;
                if (shieldMat48 != null)
                    for (int ci = 0; ci < b.shield.childCount; ci++)
                    {
                        Renderer cr = b.shield.GetChild(ci).GetComponent<Renderer>();
                        if (cr != null && cr.sharedMaterial == shieldMat48) cr.sharedMaterial = frameMatWas48 != null ? frameMatWas48 : shieldMatWas48;
                    }
                // round 5: the frame child's own mesh back with the row off (foes48 drew the arc's frame on it)
                Mesh frame48 = Mesh43.ShieldArcFrame48IfMade();
                if (frame48 != null)
                    for (int ci = 0; ci < b.shield.childCount; ci++)
                    {
                        MeshFilter cf = b.shield.GetChild(ci).GetComponent<MeshFilter>();
                        if (cf != null && cf.sharedMesh == frame48) cf.sharedMesh = Mesh43.Get(Mesh43.ShieldFrame);
                    }
            }
        }

        /// <summary>Round 5 (the blind pair: "a pale slab"): foes48's shield colour: the warden's shield orange (WardenShieldHex) at a body's value
        /// (Shield48Value, not NIGHT20's HDR x1.3 that drew it pale), its shimmer and its block flash kept as shares of it; down: NIGHT20's.</summary>
        public static Color Shield48Color(Color was, bool down, Warden w, float age)
        {
            if (down) return was;
            Color c = Look.Hex(UnityTuning.WardenShieldHex);
            float k = UnityTuning.Shield48Value * (1f + (w.blockFlash > 0 ? UnityTuning.Shield48BlockLift : UnityTuning.Shield48Shimmer * Mathf.Sin(age * 3)));
            c *= k;
            c.a = 1;
            return c;
        }

        void Shield48(Body b, Warden w, bool down, float arm, float dt)
        {
            b.shieldMoved47 = true;
            b.downK47 = Mathf.SmoothDamp(b.downK47, down ? 1f : 0f, ref b.downV47, UnityTuning.Shield48SwingS, Mathf.Infinity, Mathf.Max(1e-4f, dt));
            float k = Mathf.Clamp01(b.downK47);
            float kickTo = w.blockFlash > 0 ? UnityTuning.Shield47KickDeg : 0f;
            b.kick47 = Mathf.MoveTowards(b.kick47, kickTo, dt * UnityTuning.Shield47KickDeg / 0.05f);
            MeshFilter mf = b.shield.GetComponent<MeshFilter>();
            Mesh arc = Mesh43.ShieldArc48();
            if (mf != null && mf.sharedMesh != arc)
            {
                if (mf.sharedMesh != Mesh43.ShieldArc47()) b.mesh47 = mf.sharedMesh; // the shield's own mesh, put back with the rows off
                mf.sharedMesh = arc;
            }
            // round 5 (the blind pair, DECIDE48 D-33: OLD won 2-1, "the new plate reads as a pale slab fused into the torso"): the models43 frame
            // child stays and draws the ARC's frame (Mesh43.ShieldArcFrame48: dark tubes round its four edges, proud of its face), so the plate has
            // the old shield's dark rim and reads as a held object; any other child stays hidden
            Mesh frame48 = Mesh43.ShieldArcFrame48(), frame43 = Mesh43.Get(Mesh43.ShieldFrame);
            for (int ci = 0; ci < b.shield.childCount; ci++)
            {
                GameObject ch = b.shield.GetChild(ci).gameObject;
                MeshFilter cf = ch.GetComponent<MeshFilter>();
                bool isFrame = cf != null && (cf.sharedMesh == frame43 || cf.sharedMesh == frame48);
                if (isFrame)
                {
                    if (cf.sharedMesh != frame48) cf.sharedMesh = frame48;
                    if (!ch.activeSelf) ch.SetActive(true);
                }
                else if (ch.activeSelf) ch.SetActive(false);
            }
            float r = UnityTuning.Shield48ArcR, hh = UnityTuning.Shield47H * 0.5f;
            // the swing round the body's axis (s), the tip about the bottom edge (tau, about the horizontal axis across the swung facing d)
            // n48feel shield (round 1): lowered and swung on one share, the mid-swing shield met the pauldron: it drops first (kY), then swings (kS)
            float kY = Mathf.Clamp01(k / UnityTuning.Shield48DropShare), kS = Mathf.Clamp01((k - UnityTuning.Shield48SwingLag) / (1f - UnityTuning.Shield48SwingLag));
            kY = kY * kY * (3f - 2f * kY);
            kS = kS * kS * (3f - 2f * kS);
            float s = UnityTuning.Shield48DownSwingDeg * kS, tau = UnityTuning.Shield48DownTipDeg * kY, sRad = s * Mathf.Deg2Rad;
            var d = new Vector3(Mathf.Sin(sRad), 0, Mathf.Cos(sRad));
            Quaternion qTip = Quaternion.AngleAxis(tau, new Vector3(Mathf.Cos(sRad), 0, -Mathf.Sin(sRad)));
            Quaternion q = qTip * Quaternion.Euler(0, s, 0) * Quaternion.Euler(-b.kick47, 0, 0);
            float yb = Mathf.Lerp(height * UnityTuning.Shield48UpY - hh, UnityTuning.Shield48DownBottomY, kY);
            Vector3 origin = d * r + new Vector3(0, yb, 0) + qTip * new Vector3(0, hh, 0);
            // the grip on the inner face (the mesh's frame: its axis at local z = -r m)
            float rho = r - Mathf.Lerp(UnityTuning.Shield48GripIn, UnityTuning.Shield48DownGripIn, kS), g = Mathf.Lerp(UnityTuning.Shield48GripDeg, 0f, kS) * Mathf.Deg2Rad;
            Vector3 grip = origin + q * new Vector3(rho * Mathf.Sin(g), UnityTuning.Shield48GripY, rho * Mathf.Cos(g) - r);
            // the tell (arm < 0): the shield and the arm pitched together about the shoulder, top back (a wind-up: the bottom swings away from the
            // body); the swing (arm > 0): the shield shoved forward (a bash; pitching it forward swung its bottom into the body: n48feel shield)
            var shoulder = new Vector3(UnityTuning.Shield48ShoulderX, height * UnityTuning.Shield48ShoulderY, UnityTuning.Shield48ShoulderZ);
            Quaternion bash = Quaternion.AngleAxis(UnityTuning.Shield48BashK * Mathf.Min(0f, arm) * Mathf.Rad2Deg * (1f - k), Vector3.right);
            var shove = new Vector3(0, 0, UnityTuning.Shield48ShoveM * Mathf.Clamp01(arm / UnityTuning.WardenArmSwing) * (1f - k));
            origin = shoulder + bash * (origin - shoulder) + shove;
            grip = shoulder + bash * (grip - shoulder) + shove;
            b.shield.localPosition = origin;
            b.shield.localRotation = bash * q;
            b.shield.localScale = new Vector3(UnityTuning.Shield47W, UnityTuning.Shield47H, UnityTuning.Shield47Thick);
            Vector3 dir = grip - shoulder;
            float len = Mathf.Max(0.05f, dir.magnitude);
            b.arm.localPosition = (shoulder + grip) * 0.5f;
            b.arm.localRotation = Quaternion.FromToRotation(Vector3.up, dir / len);
            b.arm.localScale = new Vector3(UnityTuning.Shield48ArmThick, len, UnityTuning.Shield48ArmThick);
        }

        /// <summary>The a948 probe (Foes48.Probe): the drawn shield's vertices against the body's drawn capsule (the unit capsule r 0.5, y -0.5..0.5 in
        /// the body part's space; clearance in m by the part's smaller horizontal scale), the arm's 8 corners out past the shield's front face (an
        /// arc: radius > its outer face inside the +-60 deg cone and its height; a box: local z > 0.5), the shield's centre in the root's space.</summary>
        void Shield48Measure(Body b)
        {
            MeshFilter mf = b.shield.GetComponent<MeshFilter>();
            Mesh m = mf != null ? mf.sharedMesh : null;
            if (m == null) return;
            Vector3[] vs = m.vertices;
            Matrix4x4 toWorld = b.shield.localToWorldMatrix;
            Vector3 ls = b.body.lossyScale;
            float mScale = Mathf.Min(Mathf.Abs(ls.x), Mathf.Abs(ls.z));
            float clear = float.MaxValue, top = float.MinValue, bottom = float.MaxValue;
            int inside = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                Vector3 wp = toWorld.MultiplyPoint3x4(vs[i]);
                Vector3 q = b.body.InverseTransformPoint(wp);
                float dy = q.y - Mathf.Clamp(q.y, -0.5f, 0.5f);
                float dseg = Mathf.Sqrt(q.x * q.x + dy * dy + q.z * q.z);
                if (dseg < 0.5f) inside++;
                clear = Mathf.Min(clear, (dseg - 0.5f) * mScale);
                Vector3 rl = b.root.InverseTransformPoint(wp);
                top = Mathf.Max(top, rl.y);
                bottom = Mathf.Min(bottom, rl.y);
            }
            bool arc48 = m == Mesh43.ShieldArc48(), arc47 = m == Mesh43.ShieldArc47();
            float rr = arc48 ? UnityTuning.Shield48ArcR : UnityTuning.Shield47ArcR, t = UnityTuning.Shield47Thick, half = UnityTuning.Shield47ArcDeg * Mathf.Deg2Rad;
            Vector3 sc = b.shield.localScale;
            int outFront = 0;
            for (int c = 0; c < 8; c++)
            {
                var corner = new Vector3((c & 1) == 0 ? -0.5f : 0.5f, (c & 2) == 0 ? -0.5f : 0.5f, (c & 4) == 0 ? -0.5f : 0.5f);
                Vector3 l = b.shield.InverseTransformPoint(b.arm.TransformPoint(corner));
                if (arc48 || arc47)
                {
                    Vector3 mm = Vector3.Scale(l, sc);
                    float rho = Mathf.Sqrt(mm.x * mm.x + (mm.z + rr) * (mm.z + rr)), ang = Mathf.Atan2(mm.x, mm.z + rr);
                    if (rho > rr + t && Mathf.Abs(ang) < half && Mathf.Abs(mm.y) < UnityTuning.Shield47H * 0.5f) outFront++;
                }
                else if (l.z > 0.5f && Mathf.Abs(l.x) < 0.5f && Mathf.Abs(l.y) < 0.5f) outFront++;
            }
            Vector3 cen = b.root.InverseTransformPoint(b.rends[1].bounds.center);
            Foes48.ShieldClearM = clear;
            Foes48.ShieldInside = inside;
            Foes48.ShieldVerts = vs.Length;
            Foes48.ArmOutFront = outFront;
            Foes48.ShieldCentreX = cen.x;
            Foes48.ShieldCentreYShare = height > 0 ? cen.y / height : 0;
            Foes48.ShieldTopY = top;
            Foes48.ShieldBottomY = bottom;
            Foes48.ShieldMode = arc48 ? "48" : arc47 ? "47" : "off";
        }
    }
}
