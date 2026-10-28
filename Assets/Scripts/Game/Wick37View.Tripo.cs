// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment; the view row design.n74.tripo74; TripoModels.cs has the rule): THE WICK's Tripo model (Resources/Tripo/wick.fbx). Render calls
// TripoWick once a frame, after every part is placed. With the row on and the file there the model is made once under bodyRoot (the cage's centre, the sim's: it follows the breath, wick43's
// flinch and bossMotion63's springs), as tall as the cage (design.n37.wick37.boss.cageHeight x UnityTuning.TripoWickHeightScale), its middle on the cage's centre, and (UnityTuning.
// TripoWickFacesPlayer) turned to face the player on the spot. The cage's solid parts (the plates, the rings and crowns, the spire, the keel: every opaque mesh under cageT) stop drawing.
// KEPT, so the fight still reads: the flame core (the weak point: warm, the tell's yellow / red, the window's cyan, the weak74 identity colour) and its light, the plate seams (they glow
// cyan on a cracked plate and in the window, and turn with the cage), the danger column, the flare, the embers, the slam's decal, the stack. A model that closes the core in hides it behind
// its mesh (the core draws on the glow queue, depth tested): check it in a Player. A rebuilt stack destroys the model with the old body (BuildBody); the next frame makes it again. View only.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Wick37View
    {
        TripoSlot tripoSlot;

        void TripoWick()
        {
            tripoSlot ??= new TripoSlot("wick");
            if (tripoSlot.Missing) return;
            GameConfig cfg = game.cfg;
            if (tripoSlot.NeedsAttach(cfg) && cageT != null)
            {
                float h = (float)cfg.Num("design.n37.wick37.boss.cageHeight") * UnityTuning.TripoWickHeightScale;
                if (tripoSlot.Attach(bodyRoot, TripoModels.Fit.Height, h, Quaternion.Euler(0, UnityTuning.TripoYawDeg("wick"), 0), false, true, -1, true) != null)
                    tripoSlot.HideSolids(cageT);
            }
            if (tripoSlot.Model == null) return;
            if (UnityTuning.TripoWickFacesPlayer && game.cam != null)
            {
                Vector3 toward = game.cam.transform.position - bodyRoot.position;
                toward.y = 0;
                if (toward.sqrMagnitude > 1e-4f) tripoSlot.Model.transform.rotation = Quaternion.LookRotation(toward);
            }
            tripoSlot.Show(cfg);
        }
    }
}
