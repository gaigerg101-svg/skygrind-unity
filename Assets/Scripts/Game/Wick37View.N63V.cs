// NIGHT63 builder V (slate B1, bossMotion63; BossMotion63.Layer.cs): THE WICK's hook. One line in Wick37View.Render (after Render43 / Render47: the
// rigid pose with wick43's own shake and flinch) calls Motion63: the layer's motion on bodyRoot, the cage's pivot at its centre (the sim's
// Wick37.Centre): the follow spring's lag, the wind-up's lean back and crouch, the strike's snap, the flinch's squash and recoil, the slump in the
// window (the cage sinks, droops toward the player, squashes, sways), the breath. The view never sets bodyRoot's rotation or scale, so the hook
// writes both every frame (the rigid identity and one when the row is off). The size the guard scales by: the sim's cage radius
// (design.n37.wick37.boss.cageR, its collider: Wick37Body.Radius). The stack, the slag and the fight's effects (fxRoot) never move.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Wick37View
    {
        BossMotionRig63 rig63;
        BossMotionParams63 p63;
        GameConfig p63For;

        /// <summary>For the reels: the motion drawn on the last fight frame.</summary>
        public Motion63 Drawn63;

        void DrawMotion63(SurvivalSim sim, Wick37 w)
        {
            rig63 ??= new BossMotionRig63(BossAttackClock63.Wick);
            Vector3 rigid = bodyRoot.position;
            Vector3 player = sim.mover != null ? U(sim.mover.position) : rigid + Vector3.forward;
            Motion63 m = rig63.Frame(game, ref p63, ref p63For, rigid, player, (float)w.p.cageR, out BossAttack63 _);
            Drawn63 = m;
            bodyRoot.position = rigid + m.offset;
            bodyRoot.localRotation = m.tilt;
            bodyRoot.localScale = m.squash;
        }
    }
}
