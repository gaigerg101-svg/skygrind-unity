// NIGHT74 builder B (slate B1, design.n74.weak74; Weak74.cs has the rule). THE LOOM's idle knees, nails, pips and seams are routed to its identity colour by BossColours63's parts (loom.knees, loom.nails,
// loom.seams: Loom37View.cs:221-243, Loom37View.N47.cs:166; the role "id74" while the boss is closed), so this view changes no colour. The leg's live pair of knees (the grapple node and the weak point:
// Loom37View.cs:221 `live`) keeps the player's cyan, and the spinneret's window is Boss37Glow's cyan. Its part here is the probe for the boss63 reel's [weak74] line: the live knees are the weak
// point's box (and the spinneret while the window is open), every standing knee joins the body box, and the knees still reading cyan are counted.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        Transform[] kill74Roots;

        partial void Weak74Loom(Loom37 L)
        {
            // NIGHT74 B kill74 (Kill74.cs): the body, the twelve leg spans and the hull's root are the silhouette the death flash covers (one array, its last slot the hull root once it is built)
            if (kill74Roots == null)
            {
                kill74Roots = new Transform[14];
                kill74Roots[0] = bodyT;
                for (int i = 0; i < 6; i++) { kill74Roots[1 + i] = upper[i]; kill74Roots[7 + i] = lower[i]; }
            }
            kill74Roots[13] = hull48Root;
            Kill74.Register("loom37", kill74Roots);
            bool open = L.Open;
            Weak74.NoteFrame(Weak74.Loom, open, 0);
            if (!FeelReel.Running) return; // the probe is the boss63 reel's instrument: not run in a player's game
            bool live = false;
            for (int i = 0; i < 6; i++) live |= L.ActiveKnee(i);
            Weak74.Probe.Begin(Weak74.Loom, open || live); // the weak point exists while a knee of the leg's pair stands (live) or the window is open
            for (int i = 0; i < 6; i++)
            {
                if (L.knees[i].torn) continue;
                Weak74.Probe.Add(kneeR[i], L.ActiveKnee(i) && !open);
            }
            if (open) Weak74.Probe.AddBox(new Bounds(spinT.position, Vector3.one * (2f * UnityTuning.Loom37GlowR)), true);
        }
    }
}
