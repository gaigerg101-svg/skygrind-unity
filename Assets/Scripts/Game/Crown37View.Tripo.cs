// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment; the view row design.n74.tripo74; TripoModels.cs has the rule): THE CROWN's Tripo model (Resources/Tripo/crown.fbx). DrawCrown calls
// TripoCrown at its end (after the frame's last tint). With the row on and the file there the model is made once under spinT (the ring's spin: it turns with the ring and stops with it in the
// window; the ring's facing is the sim's `normal`), its up axis along the ring's axis (the model's crown stands the way the ring faces), its widest side the ring's diameter (2 x the sim's
// ringRadius x UnityTuning.TripoCrownScale), its middle on the ring's centre. The ring's solid parts (the 48 segments and the 8 blades: every opaque mesh under crownT) stop drawing. KEPT:
// the gems and every glow of the look passes (the tells' yellow / red, the window's cyan, the point light, the cut band, the shards): the ring's weak-point colour is carried by them, not by
// the model. The row off (any frame): the model hides and the ring draws again. View only.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        TripoSlot tripoSlot;

        void TripoCrown(Crown37 c)
        {
            tripoSlot ??= new TripoSlot("crown");
            if (tripoSlot.Missing || spinT == null) return;
            GameConfig cfg = game.cfg;
            if (tripoSlot.NeedsAttach(cfg))
            {
                float diameter = 2f * (float)c.p.ringRadius * UnityTuning.TripoCrownScale;
                // yaw about the model's own up first, then up -> the ring's axis (+z of the crown's transform)
                Quaternion orient = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, UnityTuning.TripoYawDeg("crown"), 0);
                if (tripoSlot.Attach(spinT, TripoModels.Fit.Widest, diameter, orient, false, true, -1, true) != null) tripoSlot.HideSolids(crownT);
            }
            tripoSlot.Show(cfg);
        }
    }
}
