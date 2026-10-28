// NIGHT63 builder V (slate B1, bossMotion63; BossMotion63.Layer.cs): THE LOOM's hook. One line in Loom37View.Render, once the fight is live and
// before the body is drawn, calls Motion63: the layer's motion for this frame (m63) from the sim's centre (Loom37.centre). loomLook47 and loomHull48
// (both ON since NIGHT47 / NIGHT48) draw the body from it: Look47Render adds m63.offset to the centre its whole shuttle is built round (the hull, the
// bands, the strips, the eyes, the hips: the upper legs re-span from the moved hips to the sim's knees, so the knees, the sim's grapple nodes and
// weak points, never move), and Hull48Render turns the hull by m63.tilt and sizes it by m63.squash. With loomLook47 off nothing moves (NIGHT37's
// box body: not drawn in any build since NIGHT47). The size the guard scales by: the sim's body half height (design.n37.loom37.bodyHalfH).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        BossMotionRig63 rig63;
        BossMotionParams63 p63;
        GameConfig p63For;

        /// <summary>This frame's motion for the shuttle (Rest when the row is off): Look47Render and Hull48Render read it.</summary>
        public Motion63 m63 = Motion63.Rest;

        void DrawMotion63(SurvivalSim sim, Loom37 L)
        {
            rig63 ??= new BossMotionRig63(BossAttackClock63.Loom);
            Vector3 rigid = U(L.centre);
            Vector3 player = sim.mover != null ? U(sim.mover.position) : rigid + Vector3.forward;
            m63 = rig63.Frame(game, ref p63, ref p63For, rigid, player, (float)L.p.bodyHalfH, out BossAttack63 _);
        }
    }
}
