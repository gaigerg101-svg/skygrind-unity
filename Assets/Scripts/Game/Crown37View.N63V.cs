// NIGHT63 builder V (slate B1, bossMotion63; BossMotion63.Layer.cs): THE CROWN's hook. One line at the end of Crown37View.DrawCrown (after
// crownLook47's rock / rear / flinch and Crown47Draw: the rigid pose with NIGHT47's own springs) calls Motion63: the layer's motion on crownT, the
// ring's pivot at its centre (the sim's Crown37.centre, interpolated): the follow spring's lag, the wind-up's lean back and crouch, the strike's
// snap, the flinch's squash and recoil, the slump in the window (the ring sinks, droops toward the player, squashes, sways), the breath. The view
// never sets crownT's scale, so the hook writes it every frame (one when the row is off). The guard is tight here (bosses.crown37.limitShare
// 0.25 of the 3 m ring: 0.75 m, inside its 2.4 m pass radius) because the ring is the pass's target. The cut band and the shards never move.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        BossMotionRig63 rig63;
        BossMotionParams63 p63;
        GameConfig p63For;

        /// <summary>For the reels: the motion drawn on the last fight frame.</summary>
        public static Motion63 Drawn63;

        void DrawMotion63(Crown37 c, SurvivalSim sim)
        {
            if (crownT == null) return;
            rig63 ??= new BossMotionRig63(BossAttackClock63.Crown);
            Vector3 rigid = crownT.position;
            Vector3 player = sim.mover != null ? U(sim.mover.position) : rigid + Vector3.forward;
            Motion63 m = rig63.Frame(game, ref p63, ref p63For, rigid, player, (float)c.p.ringRadius, out BossAttack63 _);
            Drawn63 = m;
            crownT.position = rigid + m.offset;
            crownT.rotation = m.tilt * crownT.rotation;
            crownT.localScale = m.squash;
        }
    }
}
