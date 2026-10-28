// NIGHT48 builder W (SurvivalSim.N48.cs's W hooks; NIGHT48.md G1; the level row design.n48.climbTop48: n47-foundry -> n48-foundry):
//   climbTop48  THE STACK's top third with a LIP (a catch shelf) at the far end of each top-third panel: the NIGHT47 sloppy pilot's every
//               top-third miss was a release 150 ms late that ran past a panel's end (Research/2026-10-18-wave47/W.md round 4); the late rider
//               now lands on the lip and a short hop rides the next panel. A level row has no sim bit: the stack gets its lips when the loaded
//               level's campaign room row carries `climbTop48` (only n48-foundry does: Tools/night/gen-n48-foundry.cjs), or when a harness /
//               reel sets climbTop48Force. The lips' numbers: design.n48.climbTop48.numbers. With wick47 off there is no NIGHT47 stack, so no
//               lips. Off (n47-foundry loaded, --off48, the row off): wick48Stack stays null and nothing here runs (NIGHT47 bit for bit).
//   The hot wall leaves a rider over a lip alone (Wick48OverLip, one line in SurvivalSim.Wick37.cs), as NIGHT47's steps.
// Per-run fields reset in N48WStartRun (rule 6); no allocation per step (the stack is built once, at a run start).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT48 climbTop48: the harness's and the reel's switch (a map without the level row's room): the lips on at the next StartRun.</summary>
        public bool climbTop48Force;
        Wick37Stack wick48Stack;
        /// <summary>NIGHT48 climbTop48: the lips are in this run's stack.</summary>
        public bool Climb48On => wick48Stack != null && wick37Stack == wick48Stack;
        /// <summary>NIGHT48 climbTop48 (the harness): landings on a lip this run; the lip last stood on (-1 none).</summary>
        public int climb48LipLands;
        int climb48OnLip = -1;

        partial void N48WStartRun()
        {
            climb48LipLands = 0;
            climb48OnLip = -1;
            bool want = (climbTop48Force || Climb48InLevel()) && rows.On(idWick47) && wick37Centre != null && wick47Stack != null && wick37Stack == wick47Stack;
            if (!want) return;
            if (wick48Stack == null)
            {
                Wick37StackParams sp = wick47Stack.p; // NIGHT47's stack (a struct copy; the constructor lays the spiral out again)
                const string g = "design.n48.climbTop48.numbers.";
                sp.lipAfter = cfg.Nums(g + "lipAfter");
                sp.lipOver = cfg.Num(g + "lipOver");
                sp.lipArcDeg = cfg.Num(g + "lipArcDeg");
                sp.lipStartDeg = cfg.Num(g + "lipStartDeg");
                sp.lipDepth = cfg.Num(g + "lipDepth");
                if (cfg.Has(g + "extraTopPanels")) sp.panelCount += cfg.Int(g + "extraTopPanels"); // round 2: more panels in the top section (the climb 25-45 s); the cap rises a rise each
                wick48Stack = new Wick37Stack(sp);
            }
            wick37Stack = wick48Stack;
            Wick37Build();
        }

        partial void N48WStep(double dt)
        {
            if (!Climb48On || wick37Centre == null) return;
            if (!mover.grounded) { climb48OnLip = -1; return; }
            int li = wick48Stack.LipAt(mover.position - wick37Centre.Value, 0.3);
            if (li >= 0 && li != climb48OnLip) climb48LipLands++;
            climb48OnLip = li;
        }

        partial void N48WCampaign() { }

        /// <summary>NIGHT48 climbTop48: arena-local feet l over one of the lips (its arc, its depth, up to 3 m over its top): the hot wall does
        /// not throw a player off a lip (NIGHT47's steps' rule). False with no lips (NIGHT47).</summary>
        bool Wick48OverLip(Vec3 l) => Climb48On && wick48Stack.LipAt(l, 3) >= 0;

        /// <summary>A campaign room row of the loaded level carries `climbTop48` (n48-foundry's a9).</summary>
        bool Climb48InLevel()
        {
            if (map?.json == null || !map.json.ContainsKey("campaign")) return false;
            var camp = PortData.Obj(map.json["campaign"]);
            if (!camp.ContainsKey("rooms")) return false;
            foreach (object ro in PortData.Arr(camp["rooms"]))
                if (PortData.Obj(ro).ContainsKey("climbTop48")) return true;
            return false;
        }
    }
}
