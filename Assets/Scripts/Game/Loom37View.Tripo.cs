// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment; the view row design.n74.tripo74; TripoModels.cs has the rule): THE LOOM's Tripo model (Resources/Tripo/loom.fbx). Render calls TripoLoom at
// its end, after the body and the legs are placed. With the row on and the file there the model is made once under the view's root and follows the pill body's place and turn every frame
// (bodyT: the sim's centre, the loomLook47 glide, the roll, loom40's pitch; not its scale), its long side along the body's z and UnityTuning.TripoLoomLengthScale x the body's length
// (Loom37BodyL), its middle on the body's. The body's solid parts stop drawing: the body box, loomLook47's shuttle hull, noses and drum, loomHull48's hub, brows and claws (every opaque mesh
// under bodyT, loomLook47 and loomHull48; the two later ones are taken the frame they exist). NOT replaced: the six legs (hip, knee, foot: their ends are the sim's knees and the canyon wall's
// feet), the knees' glows (the weak point and the grapple nodes), the spinneret and its window glow, the threads, the lash and the web wall: a model cannot stand in for the legs, so it sits
// between them. The row off (any frame): the model hides and the body draws again. View only.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        TripoSlot tripoSlot;
        bool tripoHull47, tripoHull48; // loomLook47's / loomHull48's solid parts are handed to the slot (they are built when their rows are on)

        void TripoLoom()
        {
            tripoSlot ??= new TripoSlot("loom");
            if (tripoSlot.Missing) return;
            GameConfig cfg = game.cfg;
            if (tripoSlot.NeedsAttach(cfg))
            {
                tripoHull47 = tripoHull48 = false;
                float len = UnityTuning.Loom37BodyL * UnityTuning.TripoLoomLengthScale;
                if (tripoSlot.Attach(root, TripoModels.Fit.Length, len, Quaternion.Euler(0, UnityTuning.TripoYawDeg("loom"), 0), true, true, -1, true) != null) tripoSlot.HideSolids(bodyT);
            }
            if (tripoSlot.Model == null) return;
            if (!tripoHull47 && look47Root != null) { tripoHull47 = true; tripoSlot.HideSolids(look47Root); }
            if (!tripoHull48 && hull48Root != null) { tripoHull48 = true; tripoSlot.HideSolids(hull48Root); }
            tripoSlot.Model.transform.SetPositionAndRotation(bodyT.position, bodyT.rotation);
            tripoSlot.Show(cfg);
        }
    }
}
