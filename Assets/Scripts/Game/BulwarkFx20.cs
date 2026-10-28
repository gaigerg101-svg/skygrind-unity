// NIGHT20 A1 (builder HV; NIGHT20.md row A1: "V (the ram line, the stomp rings)"): the BULWARK's attacks as bulwark.js
// render (:69-79, :621-635) draws them, on builder EN's port (Core/Enemies/Bulwark.cs: RingLive / RingX / RingZ /
// RingRadius, BeamLive / BeamAngle / BeamCharging, Center, the body). Its telegraphs (the RAM's red-orange line filling over
// ram.tell, the STOMP's circle, the hatches' circles, the beat's flash, the off-screen threat) are telegraph.js's shapes,
// drawn by Telegraph20 through the kit (Core's ITelegraph): nothing of them here. What the web draws itself, "attacks, not
// telegraphs":
// - the STOMP's shock ring: an open cylinder (56 sides) at the ring's centre, radius ring.r, stomp.height tall on the pen's
//   floor, 0xff7a30 x 2 additive, both faces; the port's ring wears a vertical ramp (brightest at the floor, gone at its
//   top: a wave running along the floor, the band a jump clears) instead of the web's flat fill;
// - the SWEEP BEAM: a 1 x 0.3 x 0.3 box from the boss at beam.height along its angle, beam.range long; while it charges 0.4
//   as thick and 0xff3a10 x 1.2, while it sweeps whole and x 4.
// Both red-orange (enemies and danger, rule 8). The boss is found by its registry body (HudReads20.BulwarkNow); frame time
// of the world (frozen in a hitstop as the sim is); no allocation per frame.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>bulwark.js render's ring and beam placements, pure (BossLook20Tests).</summary>
    public static class BulwarkFx20Math
    {
        /// <summary>:623 ringMesh: (ring.x, centre.y + stomp.height x 0.5, ring.z), scale (ring.r, stomp.height, ring.r), on the
        /// unit open cylinder (radius 1, y -0.5..0.5); web space in, Unity out.</summary>
        public static Matrix4x4 Ring(double x, double z, double floorY, double r, double height)
        {
            Vector3 p = WebSpace.ToUnity(new Vec3(x, floorY + height * 0.5, z));
            float rr = (float)r, h = (float)height;
            return new Matrix4x4(new Vector4(rr, 0, 0, 0), new Vector4(0, h, 0, 0), new Vector4(0, 0, rr, 0), new Vector4(p.x, p.y, p.z, 1));
        }

        /// <summary>The ring's brightness up its height (v 0 at the floor, 1 at its top): (1 - v)^BulwarkRingRampPow.</summary>
        public static float RingRamp(float v) => Mathf.Pow(Mathf.Clamp01(1f - v), UnityTuning.BulwarkRingRampPow);

        /// <summary>:631 beamMesh.rotation (0, -angle, 0) on a box along +x: web (cos a, 0, sin a), Unity's z flipped.</summary>
        public static Vector3 BeamDir(double angle) => new Vector3((float)Math.Cos(angle), 0, (float)-Math.Sin(angle));

        /// <summary>:633 the box's 0.3 m, x 0.4 while it charges.</summary>
        public static float BeamThick(bool charging) => UnityTuning.BulwarkBeamThick * (charging ? 0.4f : 1f);

        /// <summary>:634 0xff3a10 x 1.2 while it charges, x 4 while it sweeps.</summary>
        public static float BeamGain(bool charging) => charging ? 1.2f : 4f;
    }

    /// <summary>The BULWARK's shock ring and sweep beam (SurvivalGame.BulwarkFx).</summary>
    public sealed class BulwarkFx20
    {
        readonly SurvivalGame game;
        readonly Mesh cylinder, box;
        readonly RenderParams ringRp, beamRp;
        readonly MaterialPropertyBlock ringBlock = new MaterialPropertyBlock(), beamBlock = new MaterialPropertyBlock();
        readonly Color ringColor, beamColor;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public BulwarkFx20(SurvivalGame game)
        {
            this.game = game;
            ringColor = Look.Hex(UnityTuning.BulwarkRingHex) * UnityTuning.BulwarkRingGain;
            beamColor = Look.Hex(UnityTuning.BulwarkBeamHex);
            Material ring = FxDraw.Additive(game.mats.neon, "BULWARK shock ring");
            ring.SetFloat("_Cull", 0);
            ring.SetTexture("_BaseMap", RampTexture());
            ringRp = FxDraw.Params(ring);
            Material beam = FxDraw.Additive(game.mats.neon, "BULWARK sweep beam");
            beam.SetFloat("_Cull", 0);
            beamRp = FxDraw.Params(beam);
            cylinder = OpenCylinder(UnityTuning.BulwarkRingSides);
            box = UnitBox();
        }

        /// <summary>Frame: the live BULWARK's ring and beam (nothing while none fights).</summary>
        public void Render()
        {
            Bulwark b = HudReads20.BulwarkNow(game);
            if (b == null) return;
            BulwarkParams k = b.b;
            if (b.RingLive && b.RingRadius > 0)
            {
                ringBlock.SetColor(BaseColor, ringColor);
                RenderParams p = ringRp;
                p.matProps = ringBlock;
                Graphics.RenderMesh(p, cylinder, 0, BulwarkFx20Math.Ring(b.RingX, b.RingZ, b.Center.y, b.RingRadius, k.stompHeight));
            }
            if (b.BeamLive)
            {
                bool charging = b.BeamCharging;
                Vec3 at = b.body.position;
                Vector3 o = WebSpace.ToUnity(new Vec3(at.x, b.Center.y + k.beamHeight, at.z));
                Vector3 d = BulwarkFx20Math.BeamDir(b.BeamAngle), side = Vector3.Cross(d, Vector3.up);
                float th = BulwarkFx20Math.BeamThick(charging), len = (float)k.beamRange;
                var m = new Matrix4x4(
                    new Vector4(d.x * len, d.y * len, d.z * len, 0),
                    new Vector4(0, th, 0, 0),
                    new Vector4(side.x * th, side.y * th, side.z * th, 0),
                    new Vector4(o.x, o.y, o.z, 1));
                beamBlock.SetColor(BaseColor, beamColor * BulwarkFx20Math.BeamGain(charging));
                RenderParams p = beamRp;
                p.matProps = beamBlock;
                Graphics.RenderMesh(p, box, 0, m);
            }
        }

        /// <summary>The ring's vertical ramp (white x RingRamp up v), made once: additive, so black adds nothing.</summary>
        static Texture2D RampTexture()
        {
            const int h = 64;
            var tex = new Texture2D(4, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "BULWARK ring ramp" };
            var px = new Color32[4 * h];
            for (int y = 0; y < h; y++)
            {
                byte c = (byte)Mathf.RoundToInt(255 * BulwarkFx20Math.RingRamp((y + 0.5f) / h));
                for (int x = 0; x < 4; x++) px[y * 4 + x] = new Color32(c, c, c, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        /// <summary>CylinderGeometry(1, 1, 1, sides, 1, true): radius 1, y -0.5..0.5, open; v 0 at the floor, 1 at the top.</summary>
        static Mesh OpenCylinder(int sides)
        {
            var v = new Vector3[(sides + 1) * 2];
            var uv = new Vector2[v.Length];
            var t = new int[sides * 6];
            for (int i = 0; i <= sides; i++)
            {
                float a = i * Mathf.PI * 2 / sides, u = i / (float)sides;
                v[i * 2] = new Vector3(Mathf.Cos(a), -0.5f, Mathf.Sin(a));
                v[i * 2 + 1] = new Vector3(Mathf.Cos(a), 0.5f, Mathf.Sin(a));
                uv[i * 2] = new Vector2(u, 0);
                uv[i * 2 + 1] = new Vector2(u, 1);
            }
            for (int i = 0; i < sides; i++)
            {
                int o = i * 6, a0 = i * 2, b0 = (i + 1) * 2;
                t[o] = a0; t[o + 1] = a0 + 1; t[o + 2] = b0;
                t[o + 3] = b0; t[o + 4] = a0 + 1; t[o + 5] = b0 + 1;
            }
            var m = new Mesh { name = "BULWARK shock ring", vertices = v, uv = uv, triangles = t };
            m.RecalculateBounds();
            return m;
        }

        /// <summary>BoxGeometry(1, 0.3, 0.3).translate(0.5, 0, 0) as a unit: x 0..1, y and z -0.5..0.5 (scaled by the matrix).</summary>
        static Mesh UnitBox()
        {
            var v = new[]
            {
                new Vector3(0, -0.5f, -0.5f), new Vector3(1, -0.5f, -0.5f), new Vector3(1, 0.5f, -0.5f), new Vector3(0, 0.5f, -0.5f),
                new Vector3(0, -0.5f, 0.5f), new Vector3(1, -0.5f, 0.5f), new Vector3(1, 0.5f, 0.5f), new Vector3(0, 0.5f, 0.5f),
            };
            var t = new[]
            {
                0, 2, 1, 0, 3, 2, // z-
                4, 5, 6, 4, 6, 7, // z+
                0, 1, 5, 0, 5, 4, // y-
                3, 7, 6, 3, 6, 2, // y+
                0, 4, 7, 0, 7, 3, // x 0
                1, 2, 6, 1, 6, 5, // x 1
            };
            var m = new Mesh { name = "BULWARK sweep beam", vertices = v, triangles = t };
            m.RecalculateBounds();
            return m;
        }
    }
}
