// NIGHT71 builder B (slate B3 wyrm71: "the boss63 reel stands 12-15 m off, not 29.4 m"). The boss63 reel's stand (a reel-only option, no game
// state): with `-skygrindStand71 <m>` the reel puts the MAIN CAMERA, after every other script's LateUpdate this frame (execution order 32050:
// after SameRun63View and Lineup48Rig's 32000, before the render and the shot's end-of-frame capture), <m> metres from the boss's centre on the
// flat line from it to the player's eye, at the eye's height, looking at the boss's weak point (BossAttackClock63's centre and weak). The sim,
// the player, his aim and the reel's taps are untouched (the player still fires from his own place); only the drawn view stands closer, the
// way a player who fights the boss up close sees it. In a same-run toggle's held frames the sim is held, so both halves stand at the same pose.
// Without the option nothing here exists (the boss63 frames as NIGHT63's). The camera goes back to the game on the next frame's Update
// (CameraFeel writes it every frame). Proof line at the hold's end: "[stand71] ..." (frames placed, the distances).
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    [DefaultExecutionOrder(32050)]
    public sealed class Stand71Rig : MonoBehaviour
    {
        SurvivalGame game;
        string boss;
        float standM;
        /// <summary>Frames placed; the player's own eye distance to the boss's centre over them (m).</summary>
        public int Placed;
        public float EyeMin = float.MaxValue, EyeMax;

        public void Begin(SurvivalGame g, string b, float m)
        {
            game = g;
            boss = b;
            standM = m;
        }

        void LateUpdate()
        {
            if (game == null || game.sim == null || game.cam == null) return;
            BossAttack63 a = BossAttackClock63.Read(game.sim, boss);
            if (!a.Live) return;
            Vector3 centre = WebSpace.ToUnity(a.centre), weak = WebSpace.ToUnity(a.weak), eye = WebSpace.ToUnity(game.Eye);
            Vector3 flat = eye - centre;
            flat.y = 0f;
            if (flat.sqrMagnitude < 1e-6f) { flat = -game.cam.transform.forward; flat.y = 0f; }
            if (flat.sqrMagnitude < 1e-6f) flat = Vector3.back;
            float eyeD = (eye - centre).magnitude;
            Vector3 pos = centre + flat.normalized * standM;
            pos.y = eye.y;
            Vector3 look = weak - pos;
            if (look.sqrMagnitude < 1e-6f) return;
            game.cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(look.normalized, Vector3.up));
            Placed++;
            EyeMin = Mathf.Min(EyeMin, eyeD);
            EyeMax = Mathf.Max(EyeMax, eyeD);
        }

        public string Line()
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            return "[stand71] boss63 " + boss + ": the camera stood " + standM.ToString("0.0", ci) + " m from the boss's centre (at the player's eye height, looking at its weak point) on "
                + Placed.ToString(ci) + " frames; the player's own eye was " + (Placed > 0 ? EyeMin.ToString("0.0", ci) + "-" + EyeMax.ToString("0.0", ci) : "-") + " m from it (the sim, the aim and the taps his own)";
        }
    }
}
