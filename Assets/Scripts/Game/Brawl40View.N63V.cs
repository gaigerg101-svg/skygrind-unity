// NIGHT63 builder V (slate B1, bossMotion63; BossMotion63.Layer.cs): THE ICEBREAKER's hook. One line in Brawl40View.Render, after the pose per
// state is chosen (Render41 / Render42 / Render47) and before it is applied: Motion63 turns the view's own pose numbers (the torso's lean, the
// head's pitch, the knee drop, which NIGHT40 snaps from state to state) into springs (the layer's lean spring: they ease in, overshoot and settle),
// adds the layer's motion to them (the wind-up's lean back, the strike's lunge, the flinch's tip back, the slump's droop go into the torso's lean;
// the slump's sink into the knee drop, so the feet stay planted) and moves the whole body by the rest (the follow's lag, the pull back and the
// recoil, the breath's lift, the sway, the volume-keeping squash). The arms are re-spanned after it from the moved shoulders to the sim's fists, so
// the fists stay on the sim's fist spheres (the hitboxes). Every pose spring's deviation from the view's own number is held inside the guard's
// limitDeg (and the knee drop's inside limitShare x radius). The size: the sim's body radius (design.n40.brawl40.body.radius, the torso's hit sphere).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Brawl40View
    {
        BossMotionRig63 rig63;
        BossMotionParams63 p63;
        GameConfig p63For;
        readonly Spring63 leanS63 = new Spring63(), headS63 = new Spring63(), dropS63 = new Spring63();
        BossMotionParams63 springsFor63;
        bool springsLive63;

        /// <summary>For the reels: the motion drawn on the last fight frame.</summary>
        public Motion63 Drawn63;

        /// <summary>NIGHT63 review S1 (Logs/n63/review-n63.md): the body's drawn position, the view's own placement this frame (`rigid`: the feet,
        /// Render42's leap height, Punch42's recoil) plus the layer's offset without the slump's sink (the knee drop takes it). A layer at rest
        /// leaves the placement exactly.</summary>
        public static Vector3 Placed63(Vector3 rigid, Motion63 m) => rigid + m.offset + Vector3.up * m.sinkM;

        // NIGHT63 V (the lead's real-play bug; nearFade63, NearFade63.N63V.cs): the body's parts hidden while the eye is at them
        readonly System.Collections.Generic.List<Renderer> nearParts63 = new System.Collections.Generic.List<Renderer>();
        readonly System.Collections.Generic.List<Renderer> nearScratch63 = new System.Collections.Generic.List<Renderer>();
        readonly System.Collections.Generic.HashSet<Renderer> nearHidden63 = new System.Collections.Generic.HashSet<Renderer>();
        NearFade63.Params nearP63;
        GameConfig nearFor63;
        Transform[] nearRoots63;

        /// <summary>For the reels: the body's parts hidden on the last frame (the eye at them).</summary>
        public int NearHidden63 => nearHidden63.Count;

        /// <summary>NIGHT63 V (the lead's real-play bug: THE ICEBREAKER covered the screen when the player was close; nearFade63's rule): every live
        /// frame, at the end of Render (every part placed), the body's parts (bodyRoot's: the legs, the leaning torso and hump, the head, the rims,
        /// the frost, the cracks; the two arms and the two fists with the knuckles and the held block) whose world bounds the eye is within
        /// numbers.bossHideM of stop drawing, and draw again once the eye is clear; the arena and the fight's effects (fxRoot's floes, decals, waves,
        /// the thrown block, the rush line) never. The row read every frame; off: every part it hid drawn again.</summary>
        void NearHide63()
        {
            bool on = game.cam != null && NearFade63.On(game.cfg);
            if (on && !ReferenceEquals(nearFor63, game.cfg)) { nearP63 = NearFade63.Params.From(game.cfg); nearFor63 = game.cfg; }
            nearParts63.Clear();
            if (on)
            {
                nearRoots63 ??= new[] { bodyRoot, armR, armL, fistR, fistL };
                foreach (Transform t in nearRoots63)
                {
                    if (t == null) continue;
                    t.GetComponentsInChildren(true, nearScratch63);
                    nearParts63.AddRange(nearScratch63);
                }
            }
            NearFade63.HideNear(nearParts63, nearHidden63, on, nearP63, on ? game.cam.transform.position : Vector3.zero);
        }

        void DrawMotion63(Brawl40 g, ref float leanDeg, ref float headDeg, ref float drop, Vector3 fu, Quaternion face)
        {
            rig63 ??= new BossMotionRig63(BossAttackClock63.Brawl);
            // NIGHT63 review S1: the transform as the view placed it this frame (Render42 ran first: the leap's height, Punch42's recoil), as the
            // Wick's and the Crown's hooks take theirs; the layer only adds to it (never rebuilt from the feet: the drawn body stays on its hit spheres)
            Vector3 rigid = bodyRoot.position;
            Quaternion rigidRot = bodyRoot.rotation;
            SurvivalSim sim = game.sim;
            float size = (float)g.p.radius;
            Vector3 player = sim != null && sim.mover != null ? U(sim.mover.position) : fu + face * Vector3.forward;
            Motion63 m = rig63.Frame(game, ref p63, ref p63For, fu, player, size, out BossAttack63 read);
            Drawn63 = m;
            if (!read.Live) springsLive63 = false; // no fight on the clock (a retry's reveal, the dying): the next fight's first frame starts the springs fresh
            // the pose springs follow the view's own pose every frame, on or off (NIGHT63 fix round 2, D1's contract: a row read every frame keeps
            // its state while off and only skips drawing, so the frame after a same-run toggle equals a run without it)
            float dt = (float)game.WorldDt;
            float leanSprung = leanDeg, headSprung = headDeg, dropSprung = drop;
            if (p63 != null && p63.Valid)
            {
                if (!ReferenceEquals(springsFor63, p63)) { leanS63.Set(p63.leanF, p63.leanZ, p63.leanR); headS63.Set(p63.leanF, p63.leanZ, p63.leanR); dropS63.Set(p63.leanF, p63.leanZ, p63.leanR); springsFor63 = p63; }
                if (!springsLive63) { leanS63.Reset(leanDeg); headS63.Reset(headDeg); dropS63.Reset(drop); springsLive63 = true; } // once, at the first fight frame
                leanSprung = leanS63.Step(leanDeg, dt);
                headSprung = headS63.Step(headDeg, dt);
                dropSprung = dropS63.Step(drop, dt);
            }
            if (!m.on) { bodyRoot.localScale = Vector3.one; return; } // drawn rigid: the view never sets the scale (its one)
            float lim = p63.limitDeg;
            float leanRigid = leanDeg, headRigid = headDeg, dropRigid = drop;
            float leanDev = Mathf.Clamp(leanSprung - leanRigid + m.fwdDeg, -lim, lim);
            float headDev = Mathf.Clamp(headSprung - headRigid, -lim, lim);
            float dropDev = Mathf.Clamp(dropSprung - dropRigid + m.sinkM, -p63.limitShare * size, p63.limitShare * size);
            leanDeg = leanRigid + leanDev;
            headDeg = headRigid + headDev;
            drop = dropRigid + dropDev;
            // the body: the offset without the sink (the knees take it), the sway about the line to the player, the squash about the feet
            Vector3 toward = player - fu;
            toward.y = 0;
            toward = toward.sqrMagnitude > 1e-6f ? toward.normalized : face * Vector3.forward;
            bodyRoot.position = Placed63(rigid, m);
            bodyRoot.rotation = Quaternion.AngleAxis(m.sideDeg, toward) * rigidRot;
            bodyRoot.localScale = m.squash;
            // NIGHT63 B1e (a measurement): the torso's centre as this view draws it (its knees take the slump's sink: the layer's generic point
            // would count it), against the sim's torso sphere (its centre, the body radius)
            Vector3 simWeak = U(g.Centre);
            Vector3 drawnWeak = bodyRoot.TransformPoint(Quaternion.Inverse(rigidRot) * (simWeak - rigid));
            BossMotion63Probe.Weak((drawnWeak - simWeak).magnitude, size);
            // the torso and the head as their own parts (reach: the torso's 2 m, the head's 0.5 m), their share of the guard
            BossMotion63Probe.Part("torso", Mathf.Abs(leanDev) * Mathf.Deg2Rad * 2f + Mathf.Abs(dropDev), Mathf.Abs(dropDev), Mathf.Max(Mathf.Abs(leanDev) / lim, Mathf.Abs(dropDev) / (p63.limitShare * size)));
            BossMotion63Probe.Part("head", Mathf.Abs(headDev) * Mathf.Deg2Rad * 0.5f, 0, Mathf.Abs(headDev) / lim);
        }
    }
}
