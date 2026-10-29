// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment; the view row design.n74.tripo74; TripoModels.cs has the rule): THE ICEBREAKER's Tripo model (Resources/Tripo/icebreaker.fbx). Render
// calls TripoBrawl at its end, after every part is placed. With the row on and the file there the model is made once under bodyRoot (the brute's feet, turned to face where it faces: it follows
// the springs and the squash of bossMotion63), UnityTuning.TripoIcebreakerHeight tall, its feet on the ground. The brute's solid parts (the legs, the torso and hump, the skull, the arms and the
// fists: every opaque mesh under bodyRoot and the four arm / fist transforms) stop drawing. KEPT, so the fight still reads where the hit test is: the fists' knuckle glows (they stay at the
// sim's fists: the wind-up's red-orange, the stuck fist's cyan), the rims, the eyes and the belt (the window's cyan), the held ice block, the rings, the floes, the slab. The model is one static mesh:
// no walk cycle, lean or knee drop (those move the pill's parts). The row off (any frame): the model hides and the brute draws again. View only.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Brawl40View
    {
        TripoSlot tripoSlot;
        Transform[] tripoSolidRoots;

        void TripoBrawl()
        {
            tripoSlot ??= new TripoSlot("icebreaker");
            if (tripoSlot.Missing || bodyRoot == null) return;
            GameConfig cfg = game.cfg;
            if (tripoSlot.NeedsAttach(cfg))
            {
                if (tripoSlot.Attach(bodyRoot, TripoModels.Fit.Height, UnityTuning.TripoIcebreakerHeight, Quaternion.Euler(0, UnityTuning.TripoYawDeg("icebreaker"), 0), false, false, -1, true) != null)
                {
                    tripoSolidRoots ??= new[] { bodyRoot, armR, armL, fistR, fistL };
                    for (int i = 0; i < tripoSolidRoots.Length; i++) tripoSlot.HideSolids(tripoSolidRoots[i]);
                    // PG3 (GG 2026-10-28: "updated animations"): the model cut into body / legs / arms (TripoLimbs) and each piece handed to the brute's own bone, after the pill parts
                    // are hidden, so the walk cycle, the lean and the haymaker / slam / block poses move the model as they moved the pills
                    TripoLimbs limbs = TripoLimbs.Make("icebreaker", tripoSlot.Model.transform);
                    if (limbs != null)
                    {
                        tripoSlot.Rebind(); // first, while the parts are still under the model: the flash keeps them after they move to the bones
                        limbs.Ride(lean != null ? lean : torso, legL, legR, armL, armR);
                    }
                }
            }
            tripoSlot.Show(cfg, true); // nearFade63 (NearHide63, just before) flips forceRenderingOff on these parts too: set them off again every frame
        }
    }
}
