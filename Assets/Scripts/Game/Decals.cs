// NIGHT10: impact decals, after skygrind/src/combat/effects.js :132-149 (addDecal), :172-180 (the shotResult wiring),
// :302 (the colour) and :325-329 (clearDecals). The player's hitscan shot (rayCount > 0: a projectile's shotResult has 0)
// that ends on the world leaves a small quad (PlaneGeometry(1, 1), a MeshBasicMaterial in colors.decal: dark, unlit,
// opaque) facing the surface normal, fx.decalLift off the surface, fx.decalSize x (0.8 + 0.4 r) across, spun r x 2 PI
// about the normal. fx.decalCount of them in a ring buffer made at load (the oldest reused). They stay until Clear (a new
// run; the web clears them when the arena reshapes).
// effects.js :179 (POLISH V3): a shot on a force field leaves no decal (barriers.shotAt: the point 0.1 m behind the
// surface, barrier.js :337, inside a map barrier box or a locked zone); the field's ripple itself is not ported here.
// Differences from the web:
// - one decal per shot: the port's shotResult carries one ray. The THUNDERCLAP records only its first pellet's end, with
//   no normal, so its normal is re-cast from the shot's origin to that end (no decal when the re-cast misses the end:
//   the first pellet hit nothing in range). The web leaves one decal per pellet.
// - the random size and spin come from a cosmetic Mulberry32 of its own, seeded with the run seed (the web draws
//   Math.random); the sim's dice are never touched.
// - no polygon offset (the web's polygonOffset -2 / -2: URP Unlit has none); the lift alone keeps the quad off the wall.
// - shown on the default layer for every camera, as the web's scene.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Decals
    {

        readonly SurvivalGame game;
        readonly Transform[] quads;
        readonly double size, lift;
        Mulberry32 rng;
        int next;

        public Decals(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            size = cfg.Num("fx.decalSize");
            lift = cfg.Num("fx.decalLift");
            // PlaneGeometry(1, 1): a unit quad facing +z (Unity space here; it is turned onto the normal below)
            var kit = new MeshKit();
            kit.Quad(new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, 0.5f, 0), Vector3.forward, Color.white);
            Mesh mesh = kit.Build("decal");
            var mat = new Material(game.mats.neon) { name = "Decal" }; // unlit, as the web's MeshBasicMaterial
            mat.SetColor("_BaseColor", Look.C(cfg, "colors.decal"));
            var root = new GameObject("Decals").transform;
            quads = new Transform[cfg.Int("fx.decalCount")];
            for (int i = 0; i < quads.Length; i++)
            {
                GameObject go = MeshKit.Object($"Decal {i}", root, mesh, mat, shadows: false);
                go.SetActive(false);
                quads[i] = go.transform;
            }
            rng = new Mulberry32(game.runSeed);
            game.events.On<ShotEvent>("shotResult", OnShot);
        }

        /// <summary>clearDecals(): every decal gone, the ring restarts; the cosmetic dice reseed from the run's seed.</summary>
        public void Clear()
        {
            foreach (Transform t in quads)
            {
                if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
            }
            next = 0;
            rng = new Mulberry32(game.runSeed);
        }

        /// <summary>effects.js :172-180: a world hit of a hitscan shot, not on a field.</summary>
        void OnShot(ShotEvent e)
        {
            if (e.owner != "player" || e.rayCount == 0 || e.surface != "world") return;
            Vec3 end = e.end, normal = e.normal; // the payload may be reused: copies
            if (normal.x == 0 && normal.y == 0 && normal.z == 0)
            {
                // The THUNDERCLAP's event has no normal: the surface under its first pellet's end, facing the shooter.
                Vec3 d = end - e.origin;
                double len = d.Length;
                if (!(len > 0) || !game.world.RayIntersect(e.origin, d / len, out RayHit hit) || System.Math.Abs(hit.distance - len) > UnityTuning.DecalOnSurface) return;
                normal = Vec3.Dot(hit.normal, d) > 0 ? -hit.normal : hit.normal;
            }
            if (OnField(end - normal * 0.1)) return; // barrier.js :337: 0.1 m behind the surface
            Add(end, normal);
        }

        /// <summary>barrier.js inField(): inside a map barrier box (barrierMath.js insideBox, strict) or a locked zone.</summary>
        bool OnField(Vec3 q)
        {
            foreach (MapBox b in game.map.barriers)
            {
                if (q.x > b.min.x && q.x < b.max.x && q.y > b.min.y && q.y < b.max.y && q.z > b.min.z && q.z < b.max.z) return true;
            }
            return game.zones != null && game.zones.LockedAt(q);
        }

        /// <summary>:140-149 addDecal(position, normal): the next slot of the ring, turned onto the normal and spun.</summary>
        void Add(Vec3 position, Vec3 normal)
        {
            double spin = rng.Next() * System.Math.PI * 2; // :142 (drawn before the size, as the web)
            double s = size * (0.8 + 0.4 * rng.Next()); // :143
            Vector3 n = WebSpace.ToUnity(normal).normalized;
            Quaternion q = Quaternion.FromToRotation(Vector3.forward, n) * Quaternion.AngleAxis((float)(spin * Mathf.Rad2Deg), Vector3.forward);
            Transform t = quads[next];
            t.SetPositionAndRotation(WebSpace.ToUnity(position + normal * lift), q);
            t.localScale = new Vector3((float)s, (float)s, 1);
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            next = (next + 1) % quads.Length; // oldest reused
        }
    }
}
