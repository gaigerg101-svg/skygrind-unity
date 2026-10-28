// Contact shadows, after skygrind/src/fx/blobShadows.js (POLISH V4; NIGHT12 U3-25.2): a soft dark blob on the floor
// under every live enemy (main.js forEachBody: the web skips bosses, drones and RIVAL, none of which the port has), so
// bodies sit on the floor instead of hovering over it. One instanced draw (Graphics.RenderMeshInstanced) of a flat 1 m
// quad (BlobShadow.shader), blobShadows.lift over the surface, sized radius x blobShadows.scale, at most
// blobShadows.max; an airborne body's blob fades to blobShadows.airAlpha (carried in the instance matrix's y scale) and,
// unlike the web's (at the body's own feet), lies on the floor under it (one ray down, the C# collider: Game.world).
// Every quality preset draws them (the web's: Low has no shadow map, so they are its only shadows). Frame-rate only
// (no clock); the matrix array is reused, nothing is allocated per frame.
// NIGHT17 round 1 (builder X, who owns this file from then): build 2's Player drew nothing through
// Graphics.RenderMeshInstanced, so each blob is its own Graphics.RenderMesh (at most blobShadows.max a frame) with its
// matrix as before: BlobShadow.shader reads the airborne alpha from the matrix's y scale and the blob from the quad's uv,
// which one batched mesh (CubeBatch) would lose. The same material, quad and look; nothing allocated per frame.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class BlobShadows
    {
        readonly SurvivalGame game;
        readonly Mesh quad;
        readonly Material material;
        readonly Matrix4x4[] matrices;
        readonly double scale, lift, airAlpha;
        RenderParams rp;
        static readonly Vec3 Down = new Vec3(0, -1, 0);

        public int Count { get; private set; }

        public BlobShadows(SurvivalGame game, Shader shader)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            matrices = new Matrix4x4[cfg.Int("blobShadows.max")];
            scale = cfg.Num("blobShadows.scale");
            lift = cfg.Num("blobShadows.lift");
            airAlpha = cfg.Num("blobShadows.airAlpha");
            if (shader == null) return;
            // PlaneGeometry(1, 1).rotateX(-PI / 2): a 1 m quad in the XZ plane facing up, uv 0..1.
            quad = new Mesh { name = "Blob shadow (fx/blobShadows.js)" };
            quad.SetVertices(new[] { new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(0.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f) });
            quad.SetUVs(0, new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            quad.SetTriangles(new[] { 0, 2, 1, 0, 3, 2 }, 0);
            quad.RecalculateBounds();
            material = new Material(shader) { name = "Blob shadows (fx/blobShadows.js)", enableInstancing = true };
            material.SetColor("_Color", Look.Hex((uint)cfg.Num("blobShadows.color")));
            material.SetFloat("_Opacity", (float)cfg.Num("blobShadows.opacity"));
            rp = new RenderParams(material)
            {
                layer = 0,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
            };
        }

        /// <summary>blobShadows.js update: one blob per live body (visible false hides them all).</summary>
        public void Render(bool visible)
        {
            Count = 0;
            if (material == null || !visible) return;
            var all = game.enemies.all;
            double alpha = game.Alpha; // drawn where the enemy views draw the body (interpolated as they are)
            for (int i = 0; i < all.Count && Count < matrices.Length; i++)
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                Vec3 p = e.previous + (e.position - e.previous) * alpha;
                bool airborne = e is Husk h && !h.grounded;
                double floor = p.y;
                if (airborne)
                {
                    // The port's own: the blob on the floor under it (one ray down from just over its feet).
                    var from = new Vec3(p.x, p.y + UnityTuning.BlobRayLift, p.z);
                    if (!game.world.RayIntersect(from, Down, out RayHit hit) || hit.distance > UnityTuning.BlobMaxDrop + UnityTuning.BlobRayLift) continue;
                    floor = from.y - hit.distance;
                }
                float d = (float)(e.Radius * scale);
                Vector3 at = WebSpace.ToUnity(new Vec3(p.x, floor + lift, p.z));
                matrices[Count++] = Matrix4x4.TRS(at, Quaternion.identity, new Vector3(d, (float)(airborne ? airAlpha : 1), d));
            }
            // NIGHT17 round 1: one RenderMesh per blob (no instancing), each culled by its own small box
            for (int i = 0; i < Count; i++)
            {
                Matrix4x4 m = matrices[i];
                rp.worldBounds = new Bounds(new Vector3(m.m03, m.m13, m.m23), new Vector3(m.m00 + 0.1f, 0.2f, m.m22 + 0.1f));
                Graphics.RenderMesh(rp, quad, 0, m);
            }
        }
    }
}
