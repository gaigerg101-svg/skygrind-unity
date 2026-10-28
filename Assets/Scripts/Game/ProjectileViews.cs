// The projectiles' drawing (skygrind/src/combat/projectiles.js render(), NIGHT10 U3-13.3): nails in flight and stuck
// nails (a BoxGeometry(0.035, 0.035, 0.34) in 0xd6fbff x 1.6), rockets (a CylinderGeometry(0.09, 0.12, 0.55, 8) turned
// onto its flight axis, 0xffb070 x 2.2) with a grey smoke puff every 0.025 s of frame time, each interpolated between
// its last two steps and turned onto its direction (q.setFromUnitVectors(FORWARD, dir)). The web's instanced meshes are
// pooled GameObjects here, made at load, one per pool slot. The grenade / disc / vortex / dart meshes come with N11.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class ProjectileViews
    {
        readonly SurvivalGame game;
        readonly Transform[] nails, stuck, rockets;
        readonly double[] rocketTrail;

        // NIGHT36 F muzzle36 (view only; REVIEW.md root cause 2: "the crosshair fires but the gun doesn't"): a new player nail / rocket is DRAWN from the held
        // gun's muzzle (Viewmodel.MuzzleWorld, the muzzle flash's point) and blends onto its sim path over UnityTuning.Muzzle36BlendS (a smoothstep), the
        // offset fixed at its first drawn frame; the sim path (the eye's line) is untouched. Off (the row off / -skygrindOff36): drawn at the sim position.
        readonly bool m36;
        readonly Vector3[] nailOff36, rocketOff36;
        readonly float[] nailAge36, rocketAge36;
        readonly bool[] nailSeen36, rocketSeen36;
        readonly int[] nailLastAge36, rocketLastAge36;

        /// <summary>NIGHT36 (the reels): player projectiles drawn off their sim path this frame (blending from the muzzle), and the largest offset (m).</summary>
        public static int Blending36;
        public static float BlendMax36;

        public ProjectileViews(SurvivalGame game)
        {
            this.game = game;
            Projectiles p = game.projectiles;
            m36 = N36Fixes.ViewRowOn(game.cfg, "muzzle36");
            nailOff36 = new Vector3[p.nails.Length];
            rocketOff36 = new Vector3[p.rockets.Length];
            nailAge36 = new float[p.nails.Length];
            rocketAge36 = new float[p.rockets.Length];
            nailSeen36 = new bool[p.nails.Length];
            rocketSeen36 = new bool[p.rockets.Length];
            nailLastAge36 = new int[p.nails.Length];
            rocketLastAge36 = new int[p.rockets.Length];
            var root = new GameObject("Projectiles").transform;
            var nailKit = new MeshKit();
            Color nailCol = Look.Hex(0xd6fbff, 1.6f); // projectiles.js:65
            nailKit.Cube(Vector3.zero, new Vector3(0.035f, 0.035f, 0.34f), nailCol);
            Mesh nailMesh = nailKit.Build("nail");
            var rocketKit = new MeshKit();
            Frustum(rocketKit, 0.09f, 0.12f, 0.55f, 8, Look.Hex(0xffb070, 2.2f)); // projectiles.js:66
            Mesh rocketMesh = rocketKit.Build("rocket");
            Material mat = game.mats.neon;
            nails = Make(root, "nail", nailMesh, mat, p.nails.Length);
            stuck = Make(root, "stuck nail", nailMesh, mat, p.stuck.Length);
            rockets = Make(root, "rocket", rocketMesh, mat, p.rockets.Length);
            rocketTrail = new double[p.rockets.Length];
        }

        static Transform[] Make(Transform root, string name, Mesh mesh, Material mat, int n)
        {
            var list = new Transform[n];
            for (int i = 0; i < n; i++)
            {
                GameObject go = MeshKit.Object($"{name} {i}", root, mesh, mat, shadows: false);
                go.SetActive(false);
                list[i] = go.transform;
            }
            return list;
        }

        /// <summary>A CylinderGeometry(top, bottom, height, sides) laid along +z (rotateX(PI / 2): the top at the nose, +z).</summary>
        static void Frustum(MeshKit kit, float top, float bottom, float height, int sides, Color col)
        {
            float h = height * 0.5f;
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                var n0 = new Vector3(Mathf.Sin(a0), Mathf.Cos(a0), 0);
                var n1 = new Vector3(Mathf.Sin(a1), Mathf.Cos(a1), 0);
                Vector3 tb0 = n0 * bottom + new Vector3(0, 0, -h), tb1 = n1 * bottom + new Vector3(0, 0, -h);
                Vector3 tt0 = n0 * top + new Vector3(0, 0, h), tt1 = n1 * top + new Vector3(0, 0, h);
                kit.Quad(tb0, tb1, tt1, tt0, (n0 + n1).normalized, col);
                kit.Triangle(new Vector3(0, 0, h), tt1, tt0, Vector3.forward, col);
                kit.Triangle(new Vector3(0, 0, -h), tb0, tb1, Vector3.back, col);
            }
        }

        /// <summary>render(alpha, frameDt): interpolated between the last two steps, turned onto the flight direction.</summary>
        public void Render(double alpha, double frameDt)
        {
            Projectiles p = game.projectiles;
            Blending36 = 0;
            BlendMax36 = 0;
            for (int i = 0; i < p.nails.Length; i++)
            {
                Projectiles.Projectile n = p.nails[i];
                Place(nails[i], n.alive, n.previous, n.position, n.dir, alpha);
                if (m36)
                {
                    FromMuzzle36(nails[i], n, ref nailSeen36[i], ref nailLastAge36[i], ref nailOff36[i], ref nailAge36[i], frameDt);
                    if (n.alive)
                    {
                        // n36-r1 (the lead: no readable nails at the barrel in the volley frame): a flying nail is drawn thicker and stretched into a
                        // streak behind its tip (a nail covers 1-1.5 m a frame; a 3.5 cm x 34 cm box read as nothing)
                        Transform t = nails[i];
                        t.localScale = new Vector3(UnityTuning.Nail36Thick, UnityTuning.Nail36Thick, UnityTuning.Nail36Long);
                        t.position -= t.forward * (UnityTuning.Nail36Length * (UnityTuning.Nail36Long - 1) * 0.5f);
                    }
                }
            }
            for (int i = 0; i < p.stuck.Length; i++)
            {
                Projectiles.Stuck s = p.stuck[i];
                if (!s.alive)
                {
                    if (stuck[i].gameObject.activeSelf) stuck[i].gameObject.SetActive(false);
                    continue;
                }
                Vec3 at = s.enemy.AimPoint + s.offset; // enemies.aimPoint(s.enemy, tmp).add(s.offset)
                Place(stuck[i], true, at, at, s.dir, 1);
            }
            for (int i = 0; i < p.rockets.Length; i++)
            {
                Projectiles.Projectile r = p.rockets[i];
                Place(rockets[i], r.alive, r.previous, r.position, r.dir, alpha);
                if (m36) FromMuzzle36(rockets[i], r, ref rocketSeen36[i], ref rocketLastAge36[i], ref rocketOff36[i], ref rocketAge36[i], frameDt);
                if (!r.alive) continue;
                rocketTrail[i] += frameDt;
                if (rocketTrail[i] > 0.025)
                {
                    rocketTrail[i] = 0;
                    p.burst?.Invoke(Lerp(r.previous, r.position, alpha), 0x8f8a80, 1, 0.6, 0.35, 1.3); // projectiles.js:490
                }
            }
        }

        /// <summary>NIGHT36 muzzle36: after Place (the sim position), a player shot new in its slot (alive again, or its age went back) takes the offset from
        /// its drawn position to the muzzle; the offset fades with a smoothstep over Muzzle36BlendS of world time (frozen in a hitstop).</summary>
        void FromMuzzle36(Transform t, Projectiles.Projectile pr, ref bool seen, ref int lastAge, ref Vector3 off, ref float age, double frameDt)
        {
            if (!pr.alive)
            {
                seen = false;
                return;
            }
            if (!seen || pr.ageSteps < lastAge)
            {
                seen = true;
                age = 0;
                off = Vector3.zero;
                if (ReferenceEquals(pr.loadout, game.loadout) && game.viewmodel != null && game.viewmodel.MuzzleWorld(out Vector3 mw))
                {
                    off = mw - t.position;
                    if (off.magnitude > UnityTuning.Muzzle36MaxOffset) off = Vector3.zero; // a shot far from the hands (a rebound, a script): drawn where it is
                }
            }
            lastAge = pr.ageSteps;
            if (off == Vector3.zero) return;
            float u = Mathf.Clamp01(age / UnityTuning.Muzzle36BlendS);
            float k = 1f - u * u * (3f - 2f * u);
            age += (float)frameDt;
            if (k <= 0f)
            {
                off = Vector3.zero;
                return;
            }
            t.position += off * k;
            Blending36++;
            float m = off.magnitude * k;
            if (m > BlendMax36) BlendMax36 = m;
        }

        // NIGHT36 muzzle36: the BOUNCER's grenades (GunFx.Bouncer.cs draws them; it calls Grenade36 after placing slot i)
        Vector3[] grenOff36;
        float[] grenAge36;
        bool[] grenSeen36;
        int[] grenLastAge36;

        /// <summary>NIGHT36 muzzle36: grenade slot `i` (of `n`) drawn from the muzzle and blended onto its sim path, as the nails and rockets.</summary>
        public void Grenade36(int i, int n, Transform t, Projectiles.Projectile pr, double frameDt)
        {
            if (!m36 || i < 0 || i >= n) return;
            if (grenOff36 == null || grenOff36.Length != n)
            {
                grenOff36 = new Vector3[n];
                grenAge36 = new float[n];
                grenSeen36 = new bool[n];
                grenLastAge36 = new int[n];
            }
            FromMuzzle36(t, pr, ref grenSeen36[i], ref grenLastAge36[i], ref grenOff36[i], ref grenAge36[i], frameDt);
        }

        static Vec3 Lerp(Vec3 a, Vec3 b, double t) => new Vec3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);

        static void Place(Transform t, bool alive, Vec3 previous, Vec3 position, Vec3 dir, double alpha)
        {
            if (!alive)
            {
                if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
                return;
            }
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            t.position = WebSpace.ToUnity(Lerp(previous, position, alpha));
            Vector3 d = WebSpace.ToUnity(dir);
            if (d.sqrMagnitude > 1e-12f) t.rotation = Quaternion.LookRotation(d);
        }
    }
}
