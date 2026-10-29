// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment; the view row design.n74.tripo74; TripoModels.cs has the rule): the KESTREL's and the THUNDERCLAP's Tripo models in the hands
// (Resources/Tripo/kestrel.fbx, thunderclap.fbx). Viewmodel.Render calls TripoFrame once a frame after the rigs' visibility is set. With the row on and the file there, a rig gets its model
// once: a child of the rig's transform (rig.t, so it follows the hold pose, the ADS pose, the recoil, the sway, the draw, the gun's visibility), on the viewmodel layer (every child: the overlay
// camera draws only that layer), no shadow (the overlay casts none), its long side laid along the barrel and its length and muzzle end set from the kit's own body mesh (the pill gun's z extent
// and front end; UnityTuning.TripoGunLengthScale), its middle at the kit body's height. Every solid mesh of the rig (the body, the KESTREL's cylinder and hammer, the THUNDERCLAP's pump) stops
// drawing; the gem (the gun's ready / fresh / parry light), the charge tip and sleeve, the glints and the muzzle (rig.muzzle: the shots' origin, unchanged) stay. The model is one static mesh;
// the cylinder and the pump have their own models (kestrel-cylinder.fbx, thunderclap-pump.fbx) on the moving parts' transforms, so the step and the slide still show. The gauntlet and the
// torch take the same path as the two guns. The row off (any frame): the models hide and the kit draws again. View only.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Viewmodel
    {
        TripoSlot kestrelTripo, thunderTripo, gauntletTripo, torchTripo, cylinderTripo, pumpTripo, stitcherTripo, cometTripo, longwaveTripo;

        /// <summary>The frame's Tripo models for the guns that have one (see the file's header), and the moving parts' own models.</summary>
        void TripoFrame()
        {
            kestrelTripo ??= new TripoSlot("kestrel");
            thunderTripo ??= new TripoSlot("thunderclap");
            gauntletTripo ??= new TripoSlot("gauntlet");
            torchTripo ??= new TripoSlot("torch");
            cylinderTripo ??= new TripoSlot("kestrel-cylinder");
            pumpTripo ??= new TripoSlot("thunderclap-pump");
            TripoGun(kestrelTripo, "revolver");
            TripoGun(thunderTripo, "shotgun");
            TripoGun(gauntletTripo, "gauntlet");
            TripoGun(torchTripo, "torch");
            stitcherTripo ??= new TripoSlot("stitcher");
            cometTripo ??= new TripoSlot("comet");
            longwaveTripo ??= new TripoSlot("longwave");
            TripoGun(stitcherTripo, "nailgun"); // the rigs' names are the weapon ids (STITCHER nailgun, COMET rocket, LONGWAVE rail)
            TripoGun(cometTripo, "rocket");
            TripoGun(longwaveTripo, "rail");
            TripoArm();
            // the cylinder file's axis is already its z (its diameter is its longest side, so it is not re-laid); the pump's axis is its long side
            if (rigs.TryGetValue("revolver", out Rig kestrel)) TripoPart(kestrelTripo, cylinderTripo, kestrel.cylinder, false);
            if (rigs.TryGetValue("shotgun", out Rig thunder)) TripoPart(thunderTripo, pumpTripo, thunder.part, true);
        }

        TripoSlot armTripo;

        /// <summary>PG3: the quick punch's fist (the left arm, ArmBuild's "arm" mesh) wears the gauntlet model too: fitted to the arm's length along z and its middle, the blocky arm hidden while
        /// it shows; the knuckles' parry flash plate stays.</summary>
        void TripoArm()
        {
            if (armRoot == null) return;
            armTripo ??= new TripoSlot("gauntlet");
            if (armTripo.Missing) return;
            if (armTripo.NeedsAttach(cfg))
            {
                Transform arm = armRoot.Find("arm");
                MeshFilter mf = arm != null ? arm.GetComponent<MeshFilter>() : null;
                if (mf == null || mf.sharedMesh == null) return;
                Bounds bb = mf.sharedMesh.bounds;
                GameObject m = armTripo.Attach(armRoot, TripoModels.Fit.Length, bb.size.z, Quaternion.Euler(0, UnityTuning.TripoYawDeg("gauntlet"), 0), true, true, Layer, false);
                if (m != null)
                {
                    m.transform.localPosition = arm.localPosition + bb.center;
                    armTripo.Hide(arm.GetComponent<Renderer>());
                }
            }
            armTripo.Show(cfg);
        }

        TripoSlot TripoSlotOf(string rigName) => rigName == "revolver" ? kestrelTripo : rigName == "shotgun" ? thunderTripo : rigName == "gauntlet" ? gauntletTripo : rigName == "torch" ? torchTripo
            : rigName == "nailgun" ? stitcherTripo : rigName == "rocket" ? cometTripo : rigName == "rail" ? longwaveTripo : null;

        /// <summary>PG3 (GG 2026-10-28: "gun model charge needs to actually match gun"): from Glow50Frame each frame. While a gun's model shows, gunglow50's steps (cubes laid on the pill barrel)
        /// stop drawing for it and the model's own accents fill instead: TripoGlowRest + TripoGlowPerStep per lit quarter of the gun in hand, TripoGlowFlash at the full-charge flash.</summary>
        void TripoCharge(string gun, int step, bool flash)
        {
            for (int k = 0; k < g50Looks.Count; k++)
            {
                TripoSlot s = TripoSlotOf(g50Looks[k].gun);
                bool model = s != null && s.Shown;
                for (int i = 0; i < g50Looks[k].segs.Length; i++)
                {
                    Renderer r = g50Looks[k].segs[i].GetComponent<Renderer>();
                    if (r != null && r.forceRenderingOff != model) r.forceRenderingOff = model;
                }
            }
            foreach (string rigName in new[] { "revolver", "shotgun", "gauntlet", "torch", "nailgun", "rocket", "rail" })
            {
                TripoSlot s = TripoSlotOf(rigName);
                if (s == null || !s.Shown) continue;
                bool mine = rigName == gun;
                s.Glow(!mine ? UnityTuning.TripoGlowRest : flash ? UnityTuning.TripoGlowFlash : UnityTuning.TripoGlowRest + UnityTuning.TripoGlowPerStep * step);
            }
        }

        /// <summary>PG3 (GG 2026-10-28: "the same animations that we have should already work"): a moving part's own model (the KESTREL's cylinder, the THUNDERCLAP's pump) as a child of the
        /// part's transform, so the cylinder's step and the pump's slide move it. It waits for its gun's model (which hid the pill part), takes the pill part's length along z and its middle.</summary>
        void TripoPart(TripoSlot gun, TripoSlot slot, Transform part, bool alongZ)
        {
            if (slot.Missing || part == null || gun.Model == null) return;
            if (slot.NeedsAttach(cfg))
            {
                MeshFilter mf = part.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) return;
                Bounds pb = mf.sharedMesh.bounds; // the pill part, in its own space (the part's transform carries the mesh)
                GameObject m = slot.Attach(part, TripoModels.Fit.Length, pb.size.z, Quaternion.Euler(0, UnityTuning.TripoYawDeg(slot.id), 0), alongZ, true, Layer, false);
                if (m != null) m.transform.localPosition = pb.center;
            }
            slot.Show(cfg);
        }

        void TripoGun(TripoSlot slot, string rigName)
        {
            if (slot.Missing || !rigs.TryGetValue(rigName, out Rig rig)) return;
            if (slot.NeedsAttach(cfg) && rig.meshes.Count > 0 && rig.meshes[0] != null)
            {
                Bounds bb = rig.meshes[0].bounds; // the kit body, in rig space (+z: the muzzle end; BlockyKit builds the web's -z forward as Unity +z)
                float len = bb.size.z * UnityTuning.TripoGunLengthScale;
                bool fwd = rig.muzzle.z >= bb.center.z;
                float yaw = UnityTuning.TripoYawDeg(slot.id) + (fwd ? 0f : 180f);
                GameObject m = slot.Attach(rig.t, TripoModels.Fit.Length, len, Quaternion.Euler(0, yaw, 0), true, true, Layer, false);
                if (m != null)
                {
                    m.transform.localPosition = new Vector3(0, bb.center.y, fwd ? bb.max.z - len * 0.5f : bb.min.z + len * 0.5f); // the muzzle end on the kit's, the middle at the kit's height
                    slot.HideSolids(rig.t, null, rig.gem);
                    // the STITCHER's drum and the COMET's / LONGWAVE's ring are see-through glow parts on the pill gun's shape: they would float over the model
                    foreach (Transform glow in new[] { rig.drum, rig.ring })
                        if (glow != null) foreach (Renderer r in glow.GetComponentsInChildren<Renderer>(true)) slot.Hide(r);
                }
            }
            slot.Show(cfg);
        }
    }
}
