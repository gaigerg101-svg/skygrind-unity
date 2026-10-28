// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment; the view row design.n74.tripo74; TripoModels.cs has the rule): the KESTREL's and the THUNDERCLAP's Tripo models in the hands
// (Resources/Tripo/kestrel.fbx, thunderclap.fbx). Viewmodel.Render calls TripoFrame once a frame after the rigs' visibility is set. With the row on and the file there, a rig gets its model
// once: a child of the rig's transform (rig.t, so it follows the hold pose, the ADS pose, the recoil, the sway, the draw, the gun's visibility), on the viewmodel layer (every child: the overlay
// camera draws only that layer), no shadow (the overlay casts none), its long side laid along the barrel and its length and muzzle end set from the kit's own body mesh (the pill gun's z extent
// and front end; UnityTuning.TripoGunLengthScale), its middle at the kit body's height. Every solid mesh of the rig (the body, the KESTREL's cylinder and hammer, the THUNDERCLAP's pump) stops
// drawing; the gem (the gun's ready / fresh / parry light), the charge tip and sleeve, the glints and the muzzle (rig.muzzle: the shots' origin, unchanged) stay. The model is one static mesh:
// the cylinder's step and the pump's slide, which move the pill parts, are not seen on it. The row off (any frame): the model hides and the kit draws again. View only.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Viewmodel
    {
        TripoSlot kestrelTripo, thunderTripo;

        /// <summary>The frame's Tripo models for the two guns that have one (see the file's header).</summary>
        void TripoFrame()
        {
            kestrelTripo ??= new TripoSlot("kestrel");
            thunderTripo ??= new TripoSlot("thunderclap");
            TripoGun(kestrelTripo, "revolver");
            TripoGun(thunderTripo, "shotgun");
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
                }
            }
            slot.Show(cfg);
        }
    }
}
