// NIGHT20 (builder V): the TORCH's and the VORTEX's viewmodels, from skygrind/src/assets/weaponModels.js as they are (builder
// G ports the guns' rules from guns.js tonight; the hands are ready for them, as NIGHT19's ZAPPER and BOUNCER were):
// - torch() :237-258 (TORCH, fire): "a round fuel tank slung under a stubby body, a finned barrel, a wide flared nozzle with
//   its pilot light. Signature: the tank." The pilot (a 14 mm cube) and the nozzle's mouth (a thin ring, x0.5) share the
//   gun's one glow (vcGlow(3)); viewmodel.js animateParts :461-468: lit = (0.7 + 0.3 sin(23 t) sin(7 t)) x (1 + 1.5 kick) x
//   (locked ? 0.3 : 1 + viewmodel.states.torchHeatGlow x heat), its colour the family's toward viewmodel.heatColor by the
//   heat (builder HV: from G's port, Loadout.TorchHeat / TorchLocked; TorchGlow20);
// - vortex() :340-358 (VORTEX, gravity): "an open cage of four struts around the pulsing orb, a ring plate behind it and a
//   wide emitter dish in front. Signature: the cage." The orb (an icosahedron r 0.032, detail 1) glows (vcGlow(2.4)) and
//   turns (x += 2 dt, y += 3 dt) and pulses (scale (1 - reload) x (1 + 0.12 sin(5 t)); the reload: 0 until G's cooldown
//   read lands) (:477-482).
// Built with the blocky kit's shapes in web rig space, converted with the z flip (HandsPortKit's way); turnedZ (a box turned
// about z) and the icosahedron are the two this pair needs. Not ported: the bevels (as BlockyKit).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT20 B5 (builder HV): viewmodel.js :466, the TORCH's pilot and nozzle glow.</summary>
    public static class TorchGlow20
    {
        /// <summary>(0.7 + 0.3 sin(23 t) sin(7 t)) x (1 + 1.5 kick) x (locked ? 0.3 : 1 + states.torchHeatGlow x heat).</summary>
        public static double Lit(double t, double kick, double heat, bool locked, double heatGlow) =>
            (0.7 + 0.3 * Math.Sin(t * 23) * Math.Sin(t * 7)) * (1 + 1.5 * kick) * (locked ? 0.3 : 1 + heatGlow * heat);
    }

    /// <summary>weaponModels.js torch() / vortex() in the blocky kit (web rig space in, Unity out).</summary>
    public static class HandsPortKit20
    {
        static Vector3 W(float x, float y, float z) => new Vector3(x, y, -z);

        public const float TorchGemR = 0.018f, VortexGemR = 0.018f;
        public static Vector3 TorchGemAt => W(0, 0.077f, -0.17f);
        public static Vector3 VortexGemAt => W(0, 0.088f, 0.02f);
        public static Vector3 TorchPilotAt => W(0, 0.113f, -0.425f); // :250 part(group, 'pilot', ..., [0, 0.113, -0.425])
        public static Vector3 TorchNozzleAt => W(0, 0.04f, -0.457f); // :251 part(group, 'nozzle', ..., [0, 0.04, -0.457])
        public static Vector3 VortexOrbAt => W(0, 0.05f, -0.28f); // :355 part(group, 'orb', ..., [0, 0.05, -0.28])

        /// <summary>turnedZ(box(w, h, d), a, x, y, z) (weaponModels.js :58): a box turned about z (three's rotateZ: x' = x cos a -
        /// y sin a, y' = x sin a + y cos a), then moved.</summary>
        public static void RotBoxZ(MeshKit kit, float w, float h, float d, float a, float x, float y, float z, Color c)
        {
            float cs = Mathf.Cos(a), sn = Mathf.Sin(a);
            Vector3 P(float px, float py, float pz) => W(px * cs - py * sn + x, px * sn + py * cs + y, pz + z);
            Vector3 N(float nx, float ny, float nz) => W(nx * cs - ny * sn, nx * sn + ny * cs, nz);
            float hw = w / 2, hh = h / 2, hd = d / 2;
            kit.Quad(P(-hw, hh, -hd), P(hw, hh, -hd), P(hw, hh, hd), P(-hw, hh, hd), N(0, 1, 0), c);
            kit.Quad(P(-hw, -hh, -hd), P(-hw, -hh, hd), P(hw, -hh, hd), P(hw, -hh, -hd), N(0, -1, 0), c);
            kit.Quad(P(-hw, -hh, hd), P(hw, -hh, hd), P(hw, hh, hd), P(-hw, hh, hd), N(0, 0, 1), c);
            kit.Quad(P(-hw, -hh, -hd), P(-hw, hh, -hd), P(hw, hh, -hd), P(hw, -hh, -hd), N(0, 0, -1), c);
            kit.Quad(P(-hw, -hh, -hd), P(-hw, -hh, hd), P(-hw, hh, hd), P(-hw, hh, -hd), N(-1, 0, 0), c);
            kit.Quad(P(hw, -hh, -hd), P(hw, hh, -hd), P(hw, hh, hd), P(hw, -hh, hd), N(1, 0, 0), c);
        }

        static readonly float Phi = (1 + Mathf.Sqrt(5)) / 2;
        static readonly Vector3[] IcoV =
        {
            new Vector3(-1, Phi, 0), new Vector3(1, Phi, 0), new Vector3(-1, -Phi, 0), new Vector3(1, -Phi, 0),
            new Vector3(0, -1, Phi), new Vector3(0, 1, Phi), new Vector3(0, -1, -Phi), new Vector3(0, 1, -Phi),
            new Vector3(Phi, 0, -1), new Vector3(Phi, 0, 1), new Vector3(-Phi, 0, -1), new Vector3(-Phi, 0, 1),
        };
        static readonly int[] IcoF =
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
        };

        /// <summary>three's IcosahedronGeometry(r, 1): each of the 20 faces split in 4, on the sphere; flat faces; at `at` (Unity).</summary>
        public static void Icosahedron1(MeshKit kit, float r, Color c, Vector3 at = default)
        {
            for (int f = 0; f < IcoF.Length; f += 3)
            {
                Vector3 a = IcoV[IcoF[f]].normalized, b = IcoV[IcoF[f + 1]].normalized, d = IcoV[IcoF[f + 2]].normalized;
                Vector3 ab = ((a + b) * 0.5f).normalized, bd = ((b + d) * 0.5f).normalized, da = ((d + a) * 0.5f).normalized;
                Tri(kit, a * r, ab * r, da * r, c, at);
                Tri(kit, ab * r, b * r, bd * r, c, at);
                Tri(kit, da * r, bd * r, d * r, c, at);
                Tri(kit, ab * r, bd * r, da * r, c, at);
            }
        }

        static void Tri(MeshKit kit, Vector3 a, Vector3 b, Vector3 d, Color c, Vector3 at)
        {
            Vector3 n = Vector3.Cross(b - a, d - a).normalized;
            if (Vector3.Dot(n, a + b + d) < 0) n = -n;
            kit.Triangle(a + at, b + at, d + at, n, c);
        }

        /// <summary>
        /// weaponModels.js torch() :237-258: the body on `body`; the pilot and the nozzle's mouth on `glow` built around 0 (the
        /// viewmodel puts them at TorchPilotAt / TorchNozzleAt; white: the gun's glow material colours them) or, with `glow`
        /// null, in place on the body in the family colour (the whole model). Returns the muzzle (Unity rig space).
        /// </summary>
        public static Vector3 Torch(MeshKit body, Color family, MeshKit pilot = null, MeshKit nozzle = null)
        {
            BlockyKit.Profile(body, new float[,] { { 0.05f, 0.062f }, { 0.03f, 0.07f }, { -0.19f, 0.07f }, { -0.22f, 0.05f }, { -0.22f, 0.012f }, { -0.03f, 0.008f }, { 0.0f, 0.004f }, { 0.05f, 0.004f } }, 0.06f, BlockyKit.Gunmetal);
            BlockyKit.TubeZ(body, 0.026f, 0.2f, 0, 0.04f, -0.31f, 10, BlockyKit.Gunmetal);
            BlockyKit.ConeZ(body, 0.052f, 0.028f, 0.06f, 0, 0.04f, -0.43f, 12, BlockyKit.Bone); // the flared nozzle
            foreach (float z in new[] { -0.25f, -0.28f, -0.31f, -0.34f, -0.37f }) BlockyKit.TubeZ(body, 0.04f, 0.006f, 0, 0.04f, z, 8, BlockyKit.Bone); // the fins
            BlockyKit.TubeZ(body, 0.042f, 0.2f, 0, -0.035f, -0.19f, 12, BlockyKit.Bone); // the tank
            BlockyKit.ConeZ(body, 0.026f, 0.042f, 0.02f, 0, -0.035f, -0.3f, 12, BlockyKit.Bone);
            BlockyKit.ConeZ(body, 0.042f, 0.026f, 0.02f, 0, -0.035f, -0.08f, 12, BlockyKit.Bone);
            HandsPortKit.TubeY(body, 0.008f, 0.034f, 0, 0.0f, -0.21f, 6, BlockyKit.Dark); // the feed
            BlockyKit.Box(body, 0.006f, 0.022f, 0.006f, 0, 0.098f, -0.425f, BlockyKit.Dark); // the pilot's post
            BlockyKit.TubeZ(body, 0.044f, 0.012f, 0, -0.035f, -0.13f, 12, family); // the tank's bands
            BlockyKit.TubeZ(body, 0.044f, 0.012f, 0, -0.035f, -0.25f, 12, family);
            BlockyKit.Box(body, 0.064f, 0.006f, 0.16f, 0, 0.0725f, -0.05f, family);
            BlockyKit.Box(body, 0.05f, 0.005f, 0.05f, 0, 0.072f, -0.21f, BlockyKit.Bone);
            BlockyKit.PistolGrip(body, -0.016f, 0.004f);
            // :250-251 the pilot (vcGlow(3)) and the nozzle's mouth (the same glow, x0.5)
            if (pilot != null) BlockyKit.Box(pilot, 0.014f, 0.014f, 0.014f, 0, 0, 0, Color.white);
            else BlockyKit.Box(body, 0.014f, 0.014f, 0.014f, 0, 0.113f, -0.425f, family);
            if (nozzle != null) BlockyKit.TubeZ(nozzle, 0.042f, 0.004f, 0, 0, 0, 12, Color.white * 0.5f);
            else BlockyKit.TubeZ(body, 0.042f, 0.004f, 0, 0.04f, -0.457f, 12, family * 0.5f);
            HandsPortKit.Socket(body, TorchGemR, 0, 0.077f, -0.17f, 'y');
            return W(0, 0.04f, -0.46f);
        }

        /// <summary>
        /// weaponModels.js vortex() :340-358: the body and the cage on `body`; the orb on `orb` built around 0 (the viewmodel
        /// puts it at VortexOrbAt; white: its glow material colours it) or, with `orb` null, in place in the family colour.
        /// Returns the muzzle (Unity rig space).
        /// </summary>
        public static Vector3 Vortex(MeshKit body, Color family, MeshKit orb = null)
        {
            BlockyKit.Profile(body, new float[,] { { 0.06f, 0.06f }, { 0.04f, 0.08f }, { -0.14f, 0.08f }, { -0.17f, 0.06f }, { -0.17f, 0.02f }, { -0.03f, -0.01f }, { 0.0f, 0.004f }, { 0.06f, 0.004f } }, 0.064f, BlockyKit.Gunmetal);
            foreach (int k in new[] { 1, 3, 5, 7 })
            {
                float a = k * Mathf.PI / 4; // :344-347 the four struts of the cage
                RotBoxZ(body, 0.012f, 0.012f, 0.18f, a, Mathf.Cos(a) * 0.076f, 0.05f + Mathf.Sin(a) * 0.076f, -0.28f, BlockyKit.Gunmetal);
            }
            BlockyKit.TubeZ(body, 0.08f, 0.025f, 0, 0.05f, -0.185f, 12, BlockyKit.Bone); // the ring plate
            BlockyKit.ConeZ(body, 0.09f, 0.06f, 0.035f, 0, 0.05f, -0.383f, 12, BlockyKit.Bone); // the emitter dish
            BlockyKit.TubeZ(body, 0.05f, 0.004f, 0, 0.05f, -0.399f, 12, BlockyKit.Dark);
            BlockyKit.Box(body, 0.066f, 0.006f, 0.16f, 0, 0.083f, -0.05f, family);
            BlockyKit.TubeZ(body, 0.082f, 0.008f, 0, 0.05f, -0.17f, 12, family);
            BlockyKit.PistolGrip(body, -0.016f, 0.004f);
            if (orb != null) Icosahedron1(orb, 0.032f, Color.white);
            else Icosahedron1(body, 0.032f, family, VortexOrbAt); // the whole model: the orb in place
            HandsPortKit.Socket(body, VortexGemR, 0, 0.088f, 0.02f, 'y');
            return W(0, 0.05f, -0.4f);
        }
    }

    public sealed partial class Viewmodel
    {
        double torchHeatGlow; // viewmodel.states.torchHeatGlow
        double portTime20; // the parts' clock (world time)

        /// <summary>NIGHT20: the TORCH's and the VORTEX's rigs (PortsBuild's tail).</summary>
        void Ports20Build(Material kitMat)
        {
            torchHeatGlow = cfg.Has("viewmodel.states.torchHeatGlow") ? cfg.Num("viewmodel.states.torchHeatGlow") : 0;
            Add(BuildTorch(kitMat, FamilyOf("torch")));
            Add(BuildVortex(kitMat, FamilyOf("vortex")));
        }

        /// <summary>weaponModels.js torch(): the body, the pilot and the nozzle's mouth on one glow (vcGlow(3)), the gem.</summary>
        Rig BuildTorch(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var pilot = new MeshKit();
            var nozzle = new MeshKit();
            Vector3 muzzle = HandsPortKit20.Torch(body, HandsLook.Accent(family), pilot, nozzle);
            var rig = new Rig { name = "torch", t = new GameObject("TORCH").transform, muzzle = muzzle };
            rig.t.SetParent(root, false);
            SetLayer(MeshKit.Object("body", rig.t, Keep(rig, body.Build("torch")), kitMat, shadows: false));
            rig.glow = GlowMaterial(family * 3f);
            KitPart(rig, "pilot", pilot, HandsPortKit20.TorchPilotAt, rig.glow);
            KitPart(rig, "nozzle", nozzle, HandsPortKit20.TorchNozzleAt, rig.glow);
            rig.gem = Gem(rig.t, HandsPortKit20.TorchGemAt, HandsPortKit20.TorchGemR, family);
            rig.gemColor = family;
            return rig;
        }

        /// <summary>weaponModels.js vortex(): the body and the cage, the turning orb (vcGlow(2.4)), the gem.</summary>
        Rig BuildVortex(Material kitMat, Color family)
        {
            var body = new MeshKit();
            var orb = new MeshKit();
            Vector3 muzzle = HandsPortKit20.Vortex(body, HandsLook.Accent(family), orb);
            var rig = new Rig { name = "vortex", t = new GameObject("VORTEX").transform, muzzle = muzzle };
            rig.t.SetParent(root, false);
            SetLayer(MeshKit.Object("body", rig.t, Keep(rig, body.Build("vortex")), kitMat, shadows: false));
            rig.glow = GlowMaterial(family * 2.4f);
            rig.drum = KitPart(rig, "orb", orb, HandsPortKit20.VortexOrbAt, rig.glow); // the orb rides the drum's slot (it turns)
            rig.gem = Gem(rig.t, HandsPortKit20.VortexGemAt, HandsPortKit20.VortexGemR, family);
            rig.gemColor = family;
            return rig;
        }

        /// <summary>A finish's rebuild of tonight's ports (PortRefinish's tail).</summary>
        static bool Ports20Refinish(Rig rig, MeshKit[] kits)
        {
            switch (rig.name)
            {
                case "torch": HandsPortKit20.Torch(kits[0], HandsLook.Accent(rig.gemColor), kits[1], kits[2]); return true;
                case "vortex": HandsPortKit20.Vortex(kits[0], HandsLook.Accent(rig.gemColor), kits[1]); return true;
                default: return false;
            }
        }

        /// <summary>viewmodel.js animateParts :461-482 for the TORCH and the VORTEX (world time, the gun in hand).</summary>
        bool Ports20Parts(Rig rig, double dt, double boost)
        {
            switch (rig.name)
            {
                case "torch":
                {
                    portTime20 += dt;
                    double kick = Math.Max(0, Math.Min(1, (recoil.Sample(0) + feelKick.Sample(0)) / Math.Max(1e-6, Math.Abs(GunOf("torch").kick))));
                    // loadout.guns.heat('torch') / locked('torch'): builder G's port (Core/Guns/Torch.cs TorchHeat, TorchLocked)
                    Loadout lo = game.loadout;
                    double heat = lo != null ? lo.TorchHeat : 0;
                    double lit = TorchGlow20.Lit(portTime20, kick, heat, lo != null && lo.TorchLocked, torchHeatGlow);
                    Color c = Color.Lerp(rig.gemColor, heatColor, (float)heat); // Viewmodel.cs's viewmodel.heatColor
                    rig.glow.SetColor(BaseColor, c * (float)(3 * lit * boost));
                    return true;
                }
                case "vortex":
                {
                    portTime20 += dt;
                    // loadout.cooldownFraction('vortex'): builder G2's port, read through its meter (VortexMeter: 1 = ready)
                    Loadout vlo = game.loadout;
                    double ready = vlo != null && vlo.Weapon == "vortex" && vlo.Meter(out double mv, out _, out _) ? mv : 1;
                    rig.drumAngle += dt * 2;
                    rig.spin += dt * 3;
                    float s = (float)VortexLook20.HandOrbScale(ready, portTime20);
                    rig.drum.localRotation = Quaternion.Euler((float)(-rig.drumAngle * Mathf.Rad2Deg % 360), (float)(-rig.spin * Mathf.Rad2Deg % 360), 0);
                    rig.drum.localScale = new Vector3(s, s, s);
                    rig.glow.SetColor(BaseColor, rig.gemColor * (float)(2.4 * boost));
                    return true;
                }
                default:
                    return false;
            }
        }
    }
}
