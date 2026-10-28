// NIGHT19 U3-33.6 / U3-33.8 (builder V): the ported guns' viewmodels, from skygrind/src/assets/weaponModels.js as they are
// (NIGHT19.md rule 11: no grey box where a gun belongs):
// - zapper() :287-314 (ZAPPER, volt): a humped body with a capacitor canister, glowing coils along a thin barrel, a
//   vertical fork of two prongs with the arc between them; the coils and the arc share the gun's one glow (vcGlow(1.6));
// - bouncer() :261-285 (BOUNCER, blast): a fat six-chamber drum under a top strap, a short wide-bore barrel with a ladder
//   sight, a stub stock; the drum turns a sixth a shot (viewmodel.js :578 drumTarget += PI / 3, :449-451 eased at
//   viewmodel.drumTurnRate, rotation.z).
// Built with the blocky kit's shapes (BlockyKit: box, tubeZ, coneZ, profile with its hole, the pistol grip, the torus) plus
// the two this pair needs (tubeY: three's CylinderGeometry; a box turned about x: the arc's bolt segments), in web rig
// space, converted with the z flip. The parts move as viewmodel.js animateParts :449-459 has them: the BOUNCER's drum
// turns; the ZAPPER's coils glow at 0.7 (+ 1.5 x heat: the port's loadout has no ZAPPER heat yet, so 0) and the arc in its
// fork flickers 0.6 + 0.4 x a render-side hash, x (1 + 2 x the kick). Not ported: the bevels (as BlockyKit).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>weaponModels.js zapper() / bouncer() in the blocky kit (web rig space in, Unity out).</summary>
    public static class HandsPortKit
    {
        static Vector3 W(float x, float y, float z) => new Vector3(x, y, -z);

        /// <summary>tubeY(r, len, x, y, z, sides) (blockyKit.js :100-104): three's CylinderGeometry(r, r, len, sides) at (x, y, z),
        /// its sides where three puts them (a vertex at x = r sin t, z = r cos t).</summary>
        public static void TubeY(MeshKit kit, float r, float len, float x, float y, float z, int sides, Color c)
        {
            float top = y + len / 2, bot = y - len / 2;
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                float x0 = Mathf.Sin(a0) * r, z0 = Mathf.Cos(a0) * r, x1 = Mathf.Sin(a1) * r, z1 = Mathf.Cos(a1) * r;
                Vector3 n = W((x0 + x1) * 0.5f, 0, (z0 + z1) * 0.5f).normalized;
                kit.Quad(W(x + x0, bot, z + z0), W(x + x1, bot, z + z1), W(x + x1, top, z + z1), W(x + x0, top, z + z0), n, c);
                kit.Triangle(W(x, top, z), W(x + x0, top, z + z0), W(x + x1, top, z + z1), Vector3.up, c);
                kit.Triangle(W(x, bot, z), W(x + x1, bot, z + z1), W(x + x0, bot, z + z0), Vector3.down, c);
            }
        }

        /// <summary>box(w, h, d).rotateX(a).translate(x, y, z): a box turned about x (three's rotateX: y' = y cos a - z sin a, z' = y sin a + z cos a).</summary>
        public static void RotBoxX(MeshKit kit, float w, float h, float d, float a, float x, float y, float z, Color c)
        {
            float cs = Mathf.Cos(a), sn = Mathf.Sin(a);
            Vector3 P(float px, float py, float pz) => W(px + x, py * cs - pz * sn + y, py * sn + pz * cs + z);
            Vector3 N(float nx, float ny, float nz) => W(nx, ny * cs - nz * sn, ny * sn + nz * cs);
            float hw = w / 2, hh = h / 2, hd = d / 2;
            kit.Quad(P(-hw, hh, -hd), P(hw, hh, -hd), P(hw, hh, hd), P(-hw, hh, hd), N(0, 1, 0), c);
            kit.Quad(P(-hw, -hh, -hd), P(-hw, -hh, hd), P(hw, -hh, hd), P(hw, -hh, -hd), N(0, -1, 0), c);
            kit.Quad(P(-hw, -hh, hd), P(hw, -hh, hd), P(hw, hh, hd), P(-hw, hh, hd), N(0, 0, 1), c);
            kit.Quad(P(-hw, -hh, -hd), P(-hw, hh, -hd), P(hw, hh, -hd), P(hw, -hh, -hd), N(0, 0, -1), c);
            kit.Quad(P(-hw, -hh, -hd), P(-hw, -hh, hd), P(-hw, hh, hd), P(-hw, hh, -hd), N(-1, 0, 0), c);
            kit.Quad(P(hw, -hh, -hd), P(hw, hh, -hd), P(hw, hh, hd), P(hw, -hh, hd), N(1, 0, 0), c);
        }

        /// <summary>socketGem's socket (blockyKit.js :324-328): a torus (r x 1.05, tube r x 0.28, 4 x 10) in the dark tone.</summary>
        public static void Socket(MeshKit kit, float r, float x, float y, float z, char axis) =>
            BlockyKit.Torus(kit, r * 1.05f, r * 0.28f, 4, 10, x, y, z, BlockyKit.Dark, axis);

        // the gems (web rig space): socketGem(r, hex, x, y, z, {axis})
        public const float ZapperGemR = 0.016f, BouncerGemR = 0.018f;
        public static Vector3 ZapperGemAt => W(0.029f, 0.03f, -0.065f);
        public static Vector3 BouncerGemAt => W(0, 0.092f, 0);
        static readonly Vector3 BouncerDrum = new Vector3(0, -0.004f, -0.13f); // :277 part(group, 'drum', ..., [0, -0.004, -0.13])
        public static Vector3 BouncerDrumAt => W(BouncerDrum.x, BouncerDrum.y, BouncerDrum.z);

        /// <summary>
        /// weaponModels.js zapper() :287-314: the body on `body`; the coils and the arc in the fork on `glow` (white: the gun's
        /// glow material colours them; null: in place on the body in the family colour). Returns the muzzle (Unity rig space).
        /// </summary>
        public static Vector3 Zapper(MeshKit body, Color family, MeshKit glow = null)
        {
            BlockyKit.Profile(body, new float[,] { { 0.05f, 0.05f }, { 0.03f, 0.072f }, { -0.06f, 0.078f }, { -0.15f, 0.06f }, { -0.17f, 0.04f }, { -0.17f, 0.0f }, { -0.03f, -0.01f }, { 0.0f, 0.004f }, { 0.05f, 0.004f } }, 0.056f, BlockyKit.Dark);
            BlockyKit.TubeZ(body, 0.014f, 0.24f, 0, 0.04f, -0.29f, 8, BlockyKit.Gunmetal);
            BlockyKit.Profile(body, new float[,] { { -0.39f, 0.078f }, { -0.412f, 0.078f }, { -0.412f, 0.002f }, { -0.39f, 0.002f } }, 0.034f, BlockyKit.Gunmetal);
            BlockyKit.Profile(body, new float[,] { { -0.405f, 0.06f }, { -0.405f, 0.077f }, { -0.5f, 0.094f }, { -0.522f, 0.088f }, { -0.515f, 0.068f } }, 0.016f, BlockyKit.Bone);
            BlockyKit.Profile(body, new float[,] { { -0.405f, 0.02f }, { -0.405f, 0.003f }, { -0.5f, -0.014f }, { -0.522f, -0.008f }, { -0.515f, 0.012f } }, 0.016f, BlockyKit.Bone);
            TubeY(body, 0.016f, 0.036f, 0, 0.094f, -0.03f, 8, BlockyKit.Bone);
            TubeY(body, 0.018f, 0.008f, 0, 0.115f, -0.03f, 8, family);
            BlockyKit.Box(body, 0.058f, 0.006f, 0.1f, 0, 0.012f, -0.08f, family);
            BlockyKit.PistolGrip(body, -0.016f, 0.004f);
            // :305 the coils, part(group, 'coils', ..., [0, 0.04, 0], lit): three rings along the barrel
            MeshKit g = glow ?? body;
            Color gc = glow != null ? Color.white : family;
            foreach (float z in new[] { -0.2f, -0.27f, -0.34f }) BlockyKit.TubeZ(g, 0.03f, 0.01f, 0, 0.04f, z, 10, gc);
            // :306-312 the arc in the fork, part(group, 'gap', [...bolt(zig, -0.02), ...bolt(mirrored zig, 0.025)], [0, 0.04, -0.465], lit)
            float[,] zig = { { 0.021f, 0.0f }, { 0.006f, 0.014f }, { -0.006f, -0.01f }, { -0.021f, 0.006f } }; // [y, z]
            Bolt(g, zig, -0.02f, 1, gc);
            Bolt(g, zig, 0.025f, -1, gc);
            Socket(body, ZapperGemR, 0.029f, 0.03f, -0.065f, 'x');
            return W(0, 0.04f, -0.52f);
        }

        /// <summary>weaponModels.js zapper() :306-311 bolt(pts, dz): a thin box per segment of the zigzag, turned onto it.</summary>
        static void Bolt(MeshKit kit, float[,] zig, float dz, float zSign, Color c)
        {
            const float ox = 0, oy = 0.04f, oz = -0.465f; // the gap part's place
            for (int i = 1; i < zig.GetLength(0); i++)
            {
                float y0 = zig[i - 1, 0], z0 = zig[i - 1, 1] * zSign, y1 = zig[i, 0], z1 = zig[i, 1] * zSign;
                float len = Mathf.Sqrt((y1 - y0) * (y1 - y0) + (z1 - z0) * (z1 - z0));
                RotBoxX(kit, 0.005f, len + 0.003f, 0.004f, Mathf.Atan2(z1 - z0, y1 - y0), ox, oy + (y0 + y1) / 2, oz + (z0 + z1) / 2 + dz, c);
            }
        }

        /// <summary>
        /// weaponModels.js bouncer() :261-285: the body on `body`; the drum (it turns a sixth a shot) on `drum` built around 0
        /// (the viewmodel puts it at BouncerDrumAt; null: in place on the body). Returns the muzzle (Unity rig space).
        /// </summary>
        public static Vector3 Bouncer(MeshKit body, Color family, MeshKit drum = null)
        {
            BlockyKit.Profile(body, new float[,] { { 0.06f, 0.065f }, { 0.04f, 0.085f }, { -0.22f, 0.085f }, { -0.22f, 0.07f }, { -0.07f, 0.07f }, { -0.07f, -0.01f }, { -0.03f, -0.012f }, { 0.0f, 0.004f }, { 0.06f, 0.004f } }, 0.06f, BlockyKit.Gunmetal);
            BlockyKit.TubeZ(body, 0.036f, 0.18f, 0, 0.04f, -0.29f, 10, BlockyKit.Gunmetal);
            BlockyKit.ConeZ(body, 0.047f, 0.04f, 0.02f, 0, 0.04f, -0.39f, 10, BlockyKit.Bone);
            BlockyKit.Profile(body, new float[,] { { 0.06f, 0.06f }, { 0.13f, 0.054f }, { 0.136f, -0.016f }, { 0.06f, -0.0f } }, 0.05f, BlockyKit.Bone);
            BlockyKit.TubeZ(body, 0.026f, 0.004f, 0, 0.04f, -0.398f, 10, BlockyKit.Dark);
            BlockyKit.Extrude(body, new float[,] { { -0.24f, 0.074f }, { -0.27f, 0.074f }, { -0.27f, 0.12f }, { -0.24f, 0.12f } },
                new float[,] { { -0.246f, 0.083f }, { -0.264f, 0.083f }, { -0.264f, 0.112f }, { -0.246f, 0.112f } }, 0.01f, BlockyKit.Dark, 0, 0, 0, 0, 0);
            BlockyKit.TubeZ(body, 0.038f, 0.014f, 0, 0.04f, -0.33f, 10, family);
            BlockyKit.Box(body, 0.062f, 0.006f, 0.12f, 0, 0.088f, -0.13f, family);
            BlockyKit.PistolGrip(body, -0.016f, 0.004f);
            // :272-277 the drum: a fat cylinder with coned ends, six dark chamber mouths, a family band
            MeshKit d = drum ?? body;
            float x0 = drum != null ? 0 : BouncerDrum.x, y0 = drum != null ? 0 : BouncerDrum.y, z0 = drum != null ? 0 : BouncerDrum.z;
            BlockyKit.TubeZ(d, 0.07f, 0.07f, x0, y0, z0, 8, BlockyKit.Bone);
            BlockyKit.ConeZ(d, 0.07f, 0.05f, 0.02f, x0, y0, z0 - 0.045f, 8, BlockyKit.Bone);
            BlockyKit.ConeZ(d, 0.05f, 0.07f, 0.02f, x0, y0, z0 + 0.045f, 8, BlockyKit.Bone);
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2;
                BlockyKit.TubeZ(d, 0.017f, 0.112f, x0 + Mathf.Cos(a) * 0.044f, y0 + Mathf.Sin(a) * 0.044f, z0, 6, BlockyKit.Dark);
            }
            BlockyKit.TubeZ(d, 0.0715f, 0.014f, x0, y0, z0 - 0.02f, 8, family);
            Socket(body, BouncerGemR, 0, 0.092f, 0, 'y');
            return W(0, 0.04f, -0.4f);
        }
    }

    public static class HandsPortKitWhole
    {
        /// <summary>
        /// A ported gun's whole model on one mesh (the wall buys' holograms, as BlockyKit.ForWeapon does for the kit guns): the
        /// body, its parts and glows in place in the family colour, the socket and the gem (socketGem: an octahedron r x 0.82,
        /// scaled (1, 0.8, 1)). False for an id that is not a port. For BlockyKit.ForWeapon's default case (the lead's line).
        /// </summary>
        public static bool ForWeapon(string id, MeshKit kit, Color tint)
        {
            Vector3 gem;
            float r;
            switch (id)
            {
                case "zapper":
                    HandsPortKit.Zapper(kit, tint);
                    gem = HandsPortKit.ZapperGemAt;
                    r = HandsPortKit.ZapperGemR;
                    break;
                case "bouncer":
                    HandsPortKit.Bouncer(kit, tint);
                    gem = HandsPortKit.BouncerGemAt;
                    r = HandsPortKit.BouncerGemR;
                    break;
                case "torch": // NIGHT20
                    HandsPortKit20.Torch(kit, tint);
                    gem = HandsPortKit20.TorchGemAt;
                    r = HandsPortKit20.TorchGemR;
                    break;
                case "vortex": // NIGHT20
                    HandsPortKit20.Vortex(kit, tint);
                    gem = HandsPortKit20.VortexGemAt;
                    r = HandsPortKit20.VortexGemR;
                    break;
                default:
                    return false;
            }
            float g = r * 0.82f;
            for (int s = 0; s < 8; s++)
            {
                var px = new Vector3((s & 1) == 0 ? g : -g, 0, 0);
                var py = new Vector3(0, (s & 2) == 0 ? g * 0.8f : -g * 0.8f, 0);
                var pz = new Vector3(0, 0, (s & 4) == 0 ? g : -g);
                Vector3 n = Vector3.Cross(py - px, pz - px).normalized;
                if (Vector3.Dot(n, px + py + pz) < 0) n = -n;
                kit.Triangle(gem + px, gem + py, gem + pz, n, tint);
            }
            return true;
        }
    }

    public sealed partial class Viewmodel
    {
        double portDrumRate; // viewmodel.drumTurnRate (viewmodel.js :450 v.drumTurnRate)
        uint portHash = 0x7a617070; // the ZAPPER's arc flicker (a render-side stream)

        /// <summary>The ported guns' rigs (built with the others, before the draws and the feel read every rig).</summary>
        void PortsBuild(Material kitMat)
        {
            portDrumRate = cfg.Num("viewmodel.drumTurnRate");
            Add(BuildZapper(kitMat, FamilyOf("zapper")));
            Add(BuildBouncer(kitMat, FamilyOf("bouncer")));
            Ports20Build(kitMat); // NIGHT20: the TORCH and the VORTEX (Viewmodel.Ports20.cs)
        }

        /// <summary>weaponModels.js zapper(): the body, the coils and the arc on one glow (vcGlow(1.6)), the gem.</summary>
        Rig BuildZapper(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var glow = new MeshKit();
            Vector3 muzzle = HandsPortKit.Zapper(body, HandsLook.Accent(family), glow);
            var rig = new Rig { name = "zapper", t = new GameObject("ZAPPER").transform, muzzle = muzzle };
            rig.t.SetParent(root, false);
            SetLayer(MeshKit.Object("body", rig.t, Keep(rig, body.Build("zapper")), kitMat, shadows: false));
            rig.glow = GlowMaterial(family * 1.6f);
            KitPart(rig, "coils", glow, Vector3.zero, rig.glow);
            rig.gem = Gem(rig.t, HandsPortKit.ZapperGemAt, HandsPortKit.ZapperGemR, family);
            rig.gemColor = family;
            return rig;
        }

        /// <summary>weaponModels.js bouncer(): the body, the turning drum, the gem.</summary>
        Rig BuildBouncer(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var drum = new MeshKit();
            Vector3 muzzle = HandsPortKit.Bouncer(body, HandsLook.Accent(family), drum);
            var rig = new Rig { name = "bouncer", t = new GameObject("BOUNCER").transform, muzzle = muzzle };
            rig.t.SetParent(root, false);
            SetLayer(MeshKit.Object("body", rig.t, Keep(rig, body.Build("bouncer")), kitMat, shadows: false));
            rig.drum = KitPart(rig, "drum", drum, HandsPortKit.BouncerDrumAt, kitMat);
            rig.gem = Gem(rig.t, HandsPortKit.BouncerGemAt, HandsPortKit.BouncerGemR, family);
            rig.gemColor = family;
            return rig;
        }

        /// <summary>A finish's rebuild of a ported gun's meshes (Refinish's cases for the ports).</summary>
        static bool PortRefinish(Rig rig, MeshKit[] kits)
        {
            switch (rig.name)
            {
                case "zapper": HandsPortKit.Zapper(kits[0], HandsLook.Accent(rig.gemColor), kits[1]); return true;
                case "bouncer": HandsPortKit.Bouncer(kits[0], HandsLook.Accent(rig.gemColor), kits[1]); return true;
                default: return Ports20Refinish(rig, kits); // NIGHT20: the TORCH, the VORTEX
            }
        }

        /// <summary>viewmodel.js animateParts :449-459 for the ported guns (world time, the gun in hand).</summary>
        void PortParts(Rig rig, Loadout lo, double dt, double boost)
        {
            switch (rig.name)
            {
                case "bouncer":
                    // rig.spin += (rig.drumTarget - rig.spin) * easeFactor(v.drumTurnRate, dt); p.drum.rotation.z = rig.spin
                    rig.spin += (rig.drumTarget - rig.spin) * (1 - Math.Exp(-portDrumRate * dt));
                    rig.drum.localRotation = Quaternion.Euler(0, 0, (float)(rig.spin % (2 * Math.PI) * Mathf.Rad2Deg));
                    break;
                case "zapper":
                {
                    // glowTo(coils, 0.7 + 1.5 x heat); glowTo(gap, 0.6 + 0.4 random x (1 + 2 kick)): one glow material here
                    double kick = Math.Max(0, Math.Min(1, (recoil.Sample(0) + feelKick.Sample(0)) / Math.Max(1e-6, GunOf("zapper").kick)));
                    portHash ^= portHash << 13;
                    portHash ^= portHash >> 17;
                    portHash ^= portHash << 5;
                    double flicker = 0.6 + 0.4 * ((portHash & 0xffff) / 65536.0);
                    rig.glow.SetColor(BaseColor, rig.gemColor * (float)(1.6 * Math.Max(0.7, flicker) * (1 + 2 * kick) * boost));
                    break;
                }
                default:
                    Ports20Parts(rig, dt, boost); // NIGHT20: the TORCH's pilot, the VORTEX's orb (Viewmodel.Ports20.cs)
                    break;
            }
        }
    }
}
