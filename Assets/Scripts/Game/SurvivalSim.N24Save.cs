// NIGHT24 audit (f, builder B): the run snapshot's NIGHT24 sections. A run restored at a break must equal the uninterrupted run (RunRestoreTests):
// the charged fist's charge and lockout (Fist24), the BIG PICK's stream (BigPick24), the worm's stream (Wyrm; a fight never spans a break, the
// stream does, as the BULWARK's). A section exists only while its object does (the block was on this run); the campaign's room / checkpoint /
// limits are the tape's (campaign24) and its rooms are restarted from their start, so they are not here.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N24Sections(List<(string name, IRunState state)> list)
        {
            if (n24.chargeFist && fist24 != null) list.Add(("fist24", new CustomState(w => fist24.SaveState(w), r => fist24.LoadState(r))));
            if (n24.bigPick && bigPick24 != null) list.Add(("bigPick24", new CustomState(w => bigPick24.SaveState(w), r => bigPick24.LoadState(r))));
            if (n24.wyrm && wyrm24 != null) list.Add(("wyrm24", new CustomState(w => w.I("rng", wyrm24.RngState), r => wyrm24.RngState = r.U("rng"))));
        }
    }
}
