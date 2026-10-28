// NIGHT47 builder F (design.n47.foes47, a view row; NIGHT47.md G12; GG 2026-10-18: "The shield on the big shield enemy is positioned awkwardly and
// looks ugly"). Today (F-cards 5a, read in code): a free slab 1.9 x 1.955 m floating 0.13 m in front of a 1.5 m body at (0, 1.196, 0.85), held by
// nothing; the helmet and visor stick out over it; its edges cover +-48 deg while the sim blocks +-60; DOWN it moves to (0.7, 0.805, 0.3) tilted
// -51.6 deg and CUTS THROUGH the body and the arm; up / down snaps in one frame. With the row on:
//   - UP: centred on the chest (0, Shield47UpY h, Shield47UpZ), Shield47W x Shield47H m (narrower than the body's drawn width, the visor peeking
//     over it like a helmet over a riot shield), Shield47Thick m; the arm turned forward and down to it (it HOLDS the shield);
//   - DOWN (stunned / opened): swung to the body's side, turned flat along it and tipped out (no overlap with the drawn body: its inner face
//     Shield47DownClear m outside the body's 0.75 m half-width), the arm down with it;
//   - the swing between them eased (a critically damped glide, Shield47EaseS), never a snap; a block kicks it back Shield47KickDeg deg for a
//     moment (the 60-100 ms flinch of NIGHT47.md section 3).
// The sim's shield (Core/Warden.cs) is untouched: a view row. Off: NIGHT46's slab and snap. Numbers: UnityTuning.F47.cs (Shield47*).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>foes47's switch (the WARDEN's shield, the small robots: a847 forces it per pass).</summary>
    public static class Foes47
    {
        public static bool? Force;
        static GameConfig readFor;
        static bool on;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = NightRows.ViewOn(cfg, "foes47"); }
            return on;
        }

        /// <summary>For the reel: the last warden drawn's shield bounds overlapping its body's drawn capsule (want false), and its eased down share.</summary>
        public static bool ShieldOverlapsBody;
        public static float ShieldDownK;
    }

    public sealed partial class WardenView
    {
        void Shield47(Body b, Warden w, bool down, float arm, float dt)
        {
            b.shieldMoved47 = true;
            float target = down ? 1f : 0f;
            b.downK47 = Mathf.SmoothDamp(b.downK47, target, ref b.downV47, UnityTuning.Shield47EaseS, Mathf.Infinity, Mathf.Max(1e-4f, dt));
            float k = Mathf.Clamp01(b.downK47);
            float kickTo = w.blockFlash > 0 ? UnityTuning.Shield47KickDeg : 0f;
            b.kick47 = Mathf.MoveTowards(b.kick47, kickTo, dt * UnityTuning.Shield47KickDeg / 0.05f);
            // round 2 (the lead's item 7): the shield is an ARC of the sim's +-60 deg block cone round the body's axis (Mesh43.ShieldArc47, its origin
            // at the arc's inner front), its frame part hidden (a flat frame does not sit on a curve)
            MeshFilter mf = b.shield.GetComponent<MeshFilter>();
            Mesh arc = Mesh43.ShieldArc47();
            if (mf != null && mf.sharedMesh != arc) { b.mesh47 = mf.sharedMesh; mf.sharedMesh = arc; }
            for (int ci = 0; ci < b.shield.childCount; ci++) { GameObject ch = b.shield.GetChild(ci).gameObject; if (ch.activeSelf) ch.SetActive(false); }
            Vector3 upPos = new Vector3(0, height * UnityTuning.Shield47UpY, UnityTuning.Shield47ArcR);
            Vector3 downPos = new Vector3(UnityTuning.Shield47BodyHalfW + UnityTuning.Shield47DownClear + UnityTuning.Shield47Thick, height * UnityTuning.Shield47DownY, UnityTuning.Shield47DownZ);
            b.shield.localPosition = Vector3.Lerp(upPos, downPos, k);
            b.shield.localRotation = Quaternion.Slerp(Quaternion.Euler(-b.kick47, 0, 0), Quaternion.Euler(-UnityTuning.Shield47DownTiltDeg, -90f, 0), k); // down: turned to the side, its bulge toward the body, its edges curving away
            b.shield.localScale = new Vector3(UnityTuning.Shield47W, UnityTuning.Shield47H, UnityTuning.Shield47Thick);
            // the arm holds it: forward and down to the grip while up, down by the side when the shield is down; the swing's raise on top
            b.arm.localPosition = Vector3.Lerp(new Vector3(UnityTuning.Shield47ArmX, height * UnityTuning.Shield47ArmUpY, UnityTuning.Shield47ArmUpZ), new Vector3(UnityTuning.Shield47BodyHalfW + 0.05f, height * 0.45f, 0.05f), k);
            b.arm.localRotation = Quaternion.Euler(Mathf.Lerp(UnityTuning.Shield47ArmForwardDeg, 0f, k) - arm * Mathf.Rad2Deg, 0, 0);
            // the reel's check: the shield's world box against the body's drawn capsule (radius x 1.25 by its height)
            Bounds sb = b.rends[1].bounds;
            Vector3 c = b.root.position + Vector3.up * (height * 0.5f);
            Vector3 q = sb.ClosestPoint(c);
            float dx = q.x - c.x, dz = q.z - c.z;
            Foes47.ShieldOverlapsBody = k > 0.99f && Mathf.Sqrt(dx * dx + dz * dz) < UnityTuning.Shield47BodyHalfW - 0.02f;
            Foes47.ShieldDownK = k;
        }
    }
}
