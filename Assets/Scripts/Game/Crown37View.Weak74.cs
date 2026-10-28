// NIGHT74 builder B (slate B1, design.n74.weak74, a VIEW row: Weak74.On(game.cfg) every frame; Weak74.cs has the rule and the reasons). THE CROWN's ring is the one boss part bossColours63's
// table does not route: crown40 draws its blades, rim and halo CYAN "while it may be passed", crown72 keeps them cyan through a red tell (Crown37View.Crown40.cs:84-86, Crown37View.Crown72.cs:104-106),
// and the point light follows (Crown37View.Crown40.cs:94). gg-proxy NIGHT73 close #2: that ring is a second cyan beside the player's TORCH (crown37-idle 560-1520 x 345-1080).
// With the row on and the Crown SHUT (no window, no pass flash), this pass runs after the frame's last tint (after Crown72Draw): each part of the ring still reading cyan (its _BaseColor read back:
// Weak74.RecolourIfCyan) is taken to the ivory identity colour at its own luminance, and the light likewise. While the window is OPEN the whole ring is the weak point ("OPEN: SHOOT IT") and keeps
// its cyan; a pass's flash is the hit's confirmation and is not touched; yellow and red tells, a gem's white break flash and the broken stubs are not cyan, so they are not touched.
// With the row off nothing is drawn differently; the probe still counts the parts that read cyan (the OFF half of the proof line) and boxes the ring (Weak74.Probe) for the boss63 reel's [weak74] line.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        partial void Weak74Crown(Crown37 c, bool window, bool flash)
        {
            Kill74.Register("crown37", crownT); // NIGHT74 B kill74 (Kill74.cs): the ring's solid parts are the silhouette the death flash covers
            bool on = Weak74.On(game.cfg);
            bool shut = !window && !flash;
            int taken = 0;
            if (on && shut)
            {
                taken += RecolourList74(blades) + RecolourList74(rim38) + RecolourList74(halo38) + RecolourList74(gemCross38) + RecolourList74(gemHalo38) + RecolourList74(gems);
                if (Weak74.RecolourLight(crownLight, Weak74.Crown)) taken++;
            }
            Weak74.NoteFrame(Weak74.Crown, window, taken);
            if (!FeelReel.Running) return; // the probe is the boss63 reel's instrument (boxes and cyan counts for its [weak74] line): not run in a player's game
            // the probe: the ring and its gems are the weak point while the window is open; the halos join the body box
            Weak74.Probe.Begin(Weak74.Crown, window);
            foreach (Renderer r in blades) Weak74.Probe.Add(r, true);
            foreach (Renderer r in rim38) Weak74.Probe.Add(r, true);
            foreach (Renderer r in gems) Weak74.Probe.Add(r, true);
            foreach (Renderer r in gemCross38) Weak74.Probe.Add(r, true);
            foreach (Renderer r in halo38) Weak74.Probe.Add(r, false);
            foreach (Renderer r in gemHalo38) Weak74.Probe.Add(r, false);
        }

        static int RecolourList74(System.Collections.Generic.List<Renderer> list)
        {
            int n = 0;
            for (int k = 0; k < list.Count; k++)
                if (Weak74.RecolourIfCyan(list[k], Weak74.Crown)) n++;
            return n;
        }
    }
}
