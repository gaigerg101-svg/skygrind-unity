// NIGHT56 builder G (G3 tokens56, sim row design.n56.tokens56; Core/HuskBrain.N56.cs): the sim side. At a run's start the row's bit and numbers;
// every step (Night56Step, after the brain stepped) the brain gets the sim's eye and look (SurvivalSim.Aim(): the tape's look, never the camera's
// FOV setting) for the next step's thinks, and at a campaign room's change that room's loop nodes (RoomDef.nodes, copied into a fixed array:
// no allocation per step). A CAMPAIGN run only, as tokens41 (survival keeps the web's brain); set every step (campaignOn is known only after
// N24StartRun, SurvivalSim.N41.Foes.cs's rule). Off (the bit false, --off56 / -skygrindOff56): brain.on56 false, NIGHT55's brain bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        bool tokens56On;
        Tokens56Params tokens56P;
        readonly Vec3[] tokens56Nodes = new Vec3[64];
        int tokens56Room = -2;

        /// <summary>tokens56 on for this run (the harness and the reels read it).</summary>
        public bool Tokens56On => tokens56On;

        partial void N56TokensStartRun()
        {
            tokens56On = rows.On(idTokens56);
            tokens56Room = -2;
            if (brain == null) return;
            brain.on56 = false;
            brain.nodeCount56 = 0;
            brain.coneWaits56 = brain.advances56 = 0;
            if (!tokens56On) return;
            tokens56P ??= Tokens56Params.From(cfg);
            brain.p56 = tokens56P;
            brain.nodes56 = tokens56Nodes;
        }

        partial void N56TokensStep(double dt)
        {
            if (!tokens56On || brain == null) return;
            brain.on56 = campaignOn;
            if (!campaignOn) return;
            (Vec3 eye, Vec3 look) = Aim();
            brain.eye56 = eye;
            brain.look56 = look;
            int room = campaign != null ? campaign.room : -1;
            if (room == tokens56Room) return;
            tokens56Room = room;
            int n = 0;
            if (campaignLevel != null && room >= 0 && room < campaignLevel.rooms.Count)
            {
                RoomDef r = campaignLevel.rooms[room];
                for (int i = 0; i < r.nodes.Count && n < tokens56Nodes.Length; i++) tokens56Nodes[n++] = r.nodes[i];
            }
            brain.nodeCount56 = n;
        }
    }
}
