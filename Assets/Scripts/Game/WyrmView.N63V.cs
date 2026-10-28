// NIGHT63 builder V (slate B1, bossMotion63; BossMotion63.Layer.cs): THE KILN WYRM's hook. WyrmView draws the sim's own oriented boxes, which ARE
// the worm's hitboxes ("a shot finds what you see"): the sim's segments stay where they are and only their drawing moves, inside the tightest guard
// (bosses.wyrm: no follow lag along its fast path, gain 0.6, the head's offset at most 0.375 m and 14 deg; each body piece's offset at most
// limitShare x its own half width). One line in WyrmView.Render, after the pieces and the core are placed (the rigid pose), calls DrawMotion63:
//  - THE HEAD takes the layer's whole motion about its centre: the wind-up rears it back from the player, the strike snaps it through, a hit
//    squashes and recoils it, a stagger droops and sinks it, the breath swells it; the core and the trim plate ride with it.
//  - THE BODY (the 24 pieces and the tail) never tilts: a hit's squash runs down it as a ripple (bosses.wyrm.rippleS a piece), the stagger's
//    sag and the breath's swell (a wave, bosses.wyrm.wave radians a piece) fade toward the tail (bosses.wyrm.tail).
// With the NIGHT26 models drawn (design.n26.wormVisual, OFF) the boxes are hidden: nothing here moves the models.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class WyrmView
    {
        BossMotionRig63 rig63;
        BossMotionParams63 p63;
        GameConfig p63For;

        /// <summary>For the reels: the head's motion drawn on the last fight frame.</summary>
        public Motion63 Drawn63;

        void DrawMotion63(SurvivalSim sim, Wyrm w)
        {
            int n = w.PieceCount;
            if (n == 0 || visual != null) return;
            Lag72Step(n); // NIGHT72 B1 body72 (WyrmView.Body72.cs): the pieces' follow-through springs follow the rigid centres, the row on or off (D1: the next frame on is the frame it would have drawn)
            rig63 ??= new BossMotionRig63(BossAttackClock63.Wyrm);
            bool headShown = baseT[0].gameObject.activeSelf;
            Vector3 rigid = headShown ? baseT[0].position : U(w.boxes[0].c);
            Vector3 player = sim.mover != null ? U(sim.mover.position) : rigid + Vector3.forward;
            float size = (float)(w.p.headW * 0.5);
            Motion63 m = Motion71(rigid, player, size); // NIGHT71 B wyrm71 (WyrmView.Wyrm71.cs): the row's parameters while it reads on; else rig63.Frame with p63 as NIGHT63
            Drawn63 = m;
            if (!m.on) return; // the loop above placed every box rigidly this frame
            if (headShown)
            {
                Vector3 c0 = baseT[0].position;
                Place(0, m.offset, m.tilt, m.squash);
                if (coreT.gameObject.activeSelf) coreT.position = c0 + m.offset + m.tilt * (coreT.position - c0);
            }
            BossMotionParams63 p = Params71; // NIGHT71 B wyrm71: the frame's own parameters (p63 with the row off)
            float g = p.gain, k = 1f / Mathf.Max(1e-3f, p.flinchPeakS), age = rig63.FlinchAge, amp = rig63.FlinchAmp, bt = rig63.BreathT;
            for (int i = 1; i < n; i++)
            {
                if (!baseT[i].gameObject.activeSelf) continue;
                float fall = Mathf.Lerp(1f, p.tail, i / (float)Mathf.Max(1, n - 1));
                float e = age >= 0f ? BossMotion63Math.ExpImpulse(age - i * p.rippleS, k) * amp * fall : 0f;
                float br = Mathf.Sin(2f * Mathf.PI * p.breathHz * bt - i * p.wave) * fall * (1f - p.breathMute * m.windup);
                float half = Mathf.Max(0.1f, (float)w.boxes[i].hr);
                Vector3 off = Vector3.down * (p.sink * half * m.slump * g * fall) + Vector3.up * (p.breathLift * half * br * g);
                if (body72Applied) off += Lag72Offset(i, baseT[i].position, p.limitShare * half); // NIGHT72 B1 body72: the follow-through, held to its share of this piece's guard; the clamp below holds the sum to the whole
                float limit = p.limitShare * half, share = off.magnitude / limit;
                if (share > 1f) { off *= 1f / share; share = 1f; }
                float sy = (1f - p.flinchSquash * e * g) * (1f + p.breathScale * br * g) * (1f - p.slumpSquash * m.slump * g * fall);
                sy = Mathf.Clamp(sy, 1f - p.limitSquash, 1f + p.limitSquash);
                Vector3 sq = BossMotion63Math.Volume(sy);
                Place(i, off, Quaternion.identity, sq);
                BossMotion63Probe.Part("piece " + i, off.magnitude + half * Mathf.Abs(sy - 1f), off.magnitude, Mathf.Max(share, Mathf.Abs(sy - 1f) / p.limitSquash));
            }
            Lag72Say(n); // NIGHT72 B1 body72: the lag's proof line (every few seconds of a drawn fight)
        }

        /// <summary>Move piece i's drawn box by `off`, turn it by `tilt` about its centre, scale it by `squash` in its own axes, and re-place its trim
        /// plate on the moved box (the view's own rule: the box's centre + its up x (half height + WyrmTrimThick / 2)).</summary>
        void Place(int i, Vector3 off, Quaternion tilt, Vector3 squash)
        {
            Transform b = baseT[i];
            Vector3 c = b.position, s = b.localScale;
            Quaternion q = tilt * b.rotation;
            b.SetPositionAndRotation(c + off, q);
            b.localScale = Vector3.Scale(s, squash);
            Transform t = trimT[i];
            if (!t.gameObject.activeSelf) return;
            t.SetPositionAndRotation(c + off + q * Vector3.up * (s.y * 0.5f * squash.y + UnityTuning.WyrmTrimThick * 0.5f), q);
            Vector3 ts = t.localScale;
            t.localScale = new Vector3(ts.x * squash.x, ts.y, ts.z * squash.z);
        }
    }
}
