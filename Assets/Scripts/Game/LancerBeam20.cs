// NIGHT20 U3-34.9 C2 (builder V): the LANCER's laser sight and beam, from the web's src/enemies/lancer.js render (:156-186;
// SURVIVAL.md section 7, S6: "its laser sight tracks you WITH LAG ... then it fires a hitscan beam along the laser").
// Builder E ports the kind (Core: its state, the lagging aim point, the shot); this file only draws, through an API E's
// view calls (the kind's data stays E's; no Core type is named here):
// - Aim(id, muzzle, aimPoint, aimShare) every frame a LANCER aims: a thin open cylinder from the muzzle to the lagging aim
//   point, radius 0.012 + 0.02 x the aim's elapsed share (it thickens toward the shot), colors.enemy x 2.5 (DANGER, HDR:
//   the bloom carries it), additive, depth-tested (cover hides it, as cover blocks the shot);
// - Fire(id, muzzle, to) on its `lancerFire`: the beam to the first wall (or the range), radius 0.08 shrinking to 0 over
//   0.2 s of frame time, 0xffe0c0 x 4 (the web's white-hot DANGER).
// The web's own literals, kept (UnityTuning.Fx20.cs). Pooled (UnityTuning.LancerBeams), no allocation per frame; frame time.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The laser's and the beam's shapes, pure (LancerBeam20Tests).</summary>
    public static class LancerBeamMath
    {
        /// <summary>The laser sight's radius at the aim's elapsed share (0..1): lancer.js 0.012 + 0.02 x share.</summary>
        public static double AimRadius(double share) =>
            UnityTuning.LancerAimRadius0 + UnityTuning.LancerAimRadiusGrow * (share < 0 ? 0 : share > 1 ? 1 : share);

        /// <summary>The beam's radius `age` s after the shot: 0.08 x (1 - age / 0.2); 0 from 0.2 s.</summary>
        public static double BeamRadius(double age) =>
            !(age >= 0) || age >= UnityTuning.LancerBeamLife ? 0 : UnityTuning.LancerBeamRadius * (1 - age / UnityTuning.LancerBeamLife);

        /// <summary>The unit cylinder (y 0..1, radius 1) placed from `from` to `to` at `radius` (lancer.js m.compose(muzzle, q, (r, len, r))).</summary>
        public static Matrix4x4 Place(Vector3 from, Vector3 to, float radius)
        {
            Vector3 d = to - from; // the tube's y column: its full length along the line
            float len = d.magnitude;
            Vector3 axis = len > 1e-6f ? d / len : Vector3.up;
            Vector3 across = Vector3.Cross(axis, Mathf.Abs(axis.y) < 0.9f ? Vector3.up : Vector3.right).normalized; // any normal
            Vector3 other = Vector3.Cross(across, axis); // and the third, so the basis is right-angled
            if (len <= 1e-6f) d = Vector3.zero;
            return new Matrix4x4(
                new Vector4(across.x * radius, across.y * radius, across.z * radius, 0),
                new Vector4(d.x, d.y, d.z, 0),
                new Vector4(other.x * radius, other.y * radius, other.z * radius, 0),
                new Vector4(from.x, from.y, from.z, 1));
        }
    }

    /// <summary>The LANCERs' lasers and beams (SurvivalGame.LancerBeams; E's view calls Aim and Fire).</summary>
    public sealed class LancerBeam20
    {
        struct Beam
        {
            public Vector3 from, to;
            public float radius, age;
            public bool fire, live;
        }

        readonly Beam[] aims = new Beam[UnityTuning.LancerBeams];
        readonly Beam[] fires = new Beam[UnityTuning.LancerBeams];
        int aimCount, nextFire;
        readonly Mesh cylinder;
        readonly RenderParams rp;
        readonly MaterialPropertyBlock[] blocks = new MaterialPropertyBlock[UnityTuning.LancerBeams * 2];
        readonly Color aimColor, fireColor;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public LancerBeam20(SurvivalGame game)
        {
            aimColor = Look.C(game.cfg, "colors.enemy") * UnityTuning.LancerAimGlow;
            fireColor = Look.Hex(UnityTuning.LancerBeamHex) * UnityTuning.LancerBeamGlow;
            Material m = FxDraw.Additive(game.mats.neon, "LANCER laser");
            m.SetFloat("_Cull", 0);
            rp = FxDraw.Params(m);
            for (int i = 0; i < blocks.Length; i++) blocks[i] = new MaterialPropertyBlock();
            cylinder = Cylinder(UnityTuning.LancerBeamSides);
            game.OnNewRun += Clear;
        }

        /// <summary>An aiming LANCER this frame (Unity space): its muzzle, its lagging aim point, the aim's elapsed share (0..1).</summary>
        public void Aim(int id, Vector3 muzzle, Vector3 aimPoint, double aimShare)
        {
            if (aimCount >= aims.Length) return;
            ref Beam b = ref aims[aimCount++];
            b.from = muzzle;
            b.to = aimPoint;
            b.radius = (float)LancerBeamMath.AimRadius(aimShare);
            b.live = true;
        }

        /// <summary>The same in web space (Core's Vec3).</summary>
        public void Aim(int id, Vec3 muzzle, Vec3 aimPoint, double aimShare) => Aim(id, WebSpace.ToUnity(muzzle), WebSpace.ToUnity(aimPoint), aimShare);

        /// <summary>A LANCER fired (its `lancerFire`): the beam from its muzzle to `to` (the first wall, or the range), Unity space.</summary>
        public void Fire(int id, Vector3 muzzle, Vector3 to)
        {
            ref Beam b = ref fires[nextFire];
            nextFire = (nextFire + 1) % fires.Length;
            b.from = muzzle;
            b.to = to;
            b.age = 0;
            b.fire = b.live = true;
        }

        /// <summary>The same in web space.</summary>
        public void Fire(int id, Vec3 muzzle, Vec3 to) => Fire(id, WebSpace.ToUnity(muzzle), WebSpace.ToUnity(to));

        public void Clear()
        {
            aimCount = 0;
            seenCount = 0;
            for (int i = 0; i < fires.Length; i++) fires[i].live = false;
        }

        // NIGHT20 (the lead, 00:15): this view is the LANCER's one laser and beam: it reads builder E's kind each frame
        // (Core/Enemies/Lancer.cs: state, AimTo, AimProgress, Muzzle, BeamTo, Shots), so E's view draws only the body.
        readonly int[] seenIds = new int[UnityTuning.LancerBeams * 2], seenShots = new int[UnityTuning.LancerBeams * 2];
        int seenCount;
        EnemyRegistry registry;

        /// <summary>Read the live LANCERs: an aiming one's laser this frame; a new shot (Shots moved) fires its beam.</summary>
        void ReadLancers()
        {
            if (registry == null) return;
            var all = registry.all;
            for (int i = 0; i < all.Count; i++)
            {
                if (!(all[i] is Lancer l) || !l.alive) continue;
                int slot = -1;
                for (int s = 0; s < seenCount; s++)
                    if (seenIds[s] == l.id) { slot = s; break; }
                if (slot < 0)
                {
                    slot = seenCount < seenIds.Length ? seenCount++ : (l.id & 0x7fffffff) % seenIds.Length; // full: reuse one
                    seenIds[slot] = l.id;
                    seenShots[slot] = l.Shots; // first seen: no beam for an old shot
                }
                if (l.Shots != seenShots[slot])
                {
                    seenShots[slot] = l.Shots;
                    Fire(l.id, l.Muzzle, l.BeamTo);
                }
                if (l.state == Lancer.State.Aim) Aim(l.id, l.Muzzle, l.AimTo, l.AimProgress);
            }
        }

        /// <summary>The game's registry (SurvivalGame.Fx20.cs hands it in once).</summary>
        public void Watch(EnemyRegistry reg) => registry = reg;

        /// <summary>Frame (frame time, as lancer.js): this frame's lasers, the beams' fade; the aims are cleared for the next frame.</summary>
        public void Render(double frameDt)
        {
            ReadLancers();
            int k = 0;
            for (int i = 0; i < aimCount; i++)
            {
                ref Beam b = ref aims[i];
                MaterialPropertyBlock mp = blocks[k++];
                mp.SetColor(BaseColor, aimColor);
                RenderParams p = rp;
                p.matProps = mp;
                Graphics.RenderMesh(p, cylinder, 0, LancerBeamMath.Place(b.from, b.to, b.radius));
            }
            aimCount = 0;
            for (int i = 0; i < fires.Length; i++)
            {
                ref Beam b = ref fires[i];
                if (!b.live) continue;
                b.age += (float)frameDt;
                float r = (float)LancerBeamMath.BeamRadius(b.age);
                if (r <= 0)
                {
                    b.live = false;
                    continue;
                }
                MaterialPropertyBlock mp = blocks[k++];
                mp.SetColor(BaseColor, fireColor);
                RenderParams p = rp;
                p.matProps = mp;
                Graphics.RenderMesh(p, cylinder, 0, LancerBeamMath.Place(b.from, b.to, r));
            }
        }

        /// <summary>three's CylinderGeometry(1, 1, 1, sides, 1, true).translate(0, 0.5, 0): an open tube, y 0..1.</summary>
        static Mesh Cylinder(int sides)
        {
            var v = new Vector3[sides * 2];
            var t = new int[sides * 6];
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2 / sides;
                v[i * 2] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                v[i * 2 + 1] = new Vector3(Mathf.Cos(a), 1, Mathf.Sin(a));
                int j = (i + 1) % sides, o = i * 6;
                t[o] = i * 2; t[o + 1] = i * 2 + 1; t[o + 2] = j * 2;
                t[o + 3] = j * 2; t[o + 4] = i * 2 + 1; t[o + 5] = j * 2 + 1;
            }
            var m = new Mesh { name = "LANCER laser", vertices = v, triangles = t };
            m.RecalculateBounds();
            return m;
        }
    }
}
