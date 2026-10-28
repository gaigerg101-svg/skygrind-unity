// NIGHT74 builder B (slate B1, design.n74.weak74; Weak74.cs has the rule). THE WICK's closed core is routed to its identity colour by BossColours63's "wick.core" part (Wick37View.cs:414 through
// BossColours63.Part; BossColours63.N63D.cs Routes: the role "id74" while the boss is closed), so this view changes no colour. Its part here is the probe: the core is the open weak point (the window's
// cyan, Wick37View.cs:410), the cage's plate glows join the body box, for the boss63 reel's [weak74] line (the screen box the meter splits cyan by) and the parts still reading cyan.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Wick37View
    {
        partial void Weak74Wick(bool open)
        {
            Kill74.Register("wick", bodyRoot); // NIGHT74 B kill74 (Kill74.cs): the cage's solid parts are the silhouette the death flash covers
            Weak74.NoteFrame(Weak74.Wick, open, 0);
            if (!FeelReel.Running) return; // the probe is the boss63 reel's instrument: not run in a player's game
            Weak74.Probe.Begin(Weak74.Wick, open);
            Weak74.Probe.Add(coreR, true);
            for (int i = 0; i < plateGlowR.Count; i++) Weak74.Probe.Add(plateGlowR[i], false);
        }
    }
}
