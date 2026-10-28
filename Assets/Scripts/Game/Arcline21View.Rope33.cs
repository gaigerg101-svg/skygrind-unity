// NIGHT33 builder W (row view33, DESIGN33 W "the rope"; A4 c: the rope was a 2-point straight line from the chest, the web's ropeSegments 14 and sag 0.35
// unused, no hook head). With view33 on (N33Fixes.ViewRowOn; off with -skygrindOff33) the rope is the web's fx/rope.js, frame time, draw only:
// - it leaves from the held gun's muzzle (WeaponFx.HeldMuzzle; the web's left hand point when there is no WeaponFx);
// - a quadratic curve of arcline.ropeSegments segments hand -> mid -> end; the middle sags by the rope it does not need (rope.js :84-86: flying
//   slack = 0.08 x the distance, attached slack = L - d (+0.3 when not taut), mid.y -= min(6, slack x arcline.sag x 2)): a slight sag while it flies,
//   straight (taut) once it bites; brighter when taut (rope.js :87);
// - a hook head (rope.js :33, :105-108: a small glowing octahedron spinning at the hook in flight; here a cube turned on its corner) that stays on the
//   anchor while the rope is attached;
// - a snap on release (Unity-only): the rope whips back to the muzzle over Rope33SnapTime with a sideways wave dying out.
// Off: NIGHT32's straight chest line (Arcline21View.Render).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Arcline21View
    {
        bool view33;
        int segments33;
        float sag33;
        Renderer hook33;
        Vector3 lastEnd33;
        bool wasRoped33;
        float snapAge33 = float.PositiveInfinity, hookSpin33;

        void Rope33Build(Transform root, GameConfig cfg)
        {
            view33 = N33Fixes.ViewRowOn(cfg, "view33");
            if (!view33) return;
            segments33 = Mathf.Max(2, cfg.Int("arcline.ropeSegments"));
            sag33 = (float)cfg.Num("arcline.sag");
            rope.positionCount = segments33 + 1;
            var kit = new MeshKit();
            float s = UnityTuning.Rope33HookEdge;
            kit.Cube(Vector3.zero, new Vector3(s, s, s), Color.white);
            hook33 = DesignKit.Neon("arcline hook head", root, kit, game);
            DesignKit.Tint(hook33, block, ropeColor * UnityTuning.Rope33HookGlow);
            hook33.gameObject.SetActive(false);
        }

        /// <summary>The web's hand point (rope.js :80): 0.22 m left, 0.2 m down, 0.45 m ahead of the eye.</summary>
        Vector3 Hand33()
        {
            if (WeaponFx.Current != null) return WeaponFx.Current.HeldMuzzle(game.loadout.Weapon);
            Transform c = game.cam.transform;
            return c.position - c.right * UnityTuning.Rope33HandLeft - c.up * UnityTuning.Rope33HandDown + c.forward * UnityTuning.Rope33HandAhead;
        }

        static Vector3 Curve33(Vector3 a, Vector3 m, Vector3 b, float t)
        {
            float u = 1 - t;
            return u * u * a + 2 * u * t * m + t * t * b;
        }

        void Rope33Render(SurvivalSim sim, Rope r, bool roped, double alpha, double worldDt)
        {
            float dt = (float)worldDt;
            Vector3 hand = Hand33();
            if (roped)
            {
                Vector3 end = WebSpace.ToUnity(r.phase == RopePhase.Flying ? r.hook : r.anchor);
                lastEnd33 = end;
                float d = Vector3.Distance(hand, end);
                bool taut35 = sim.Grapple35Pull; // NIGHT35 M grapple35: the pull's rope is a straight line from the fire to the arrival (no sag, the taut glow)
                float slack = taut35 ? 0 : r.phase == RopePhase.Flying ? d * UnityTuning.Rope33FlySlack : Mathf.Max(0, (float)r.L - d + (r.taut ? 0 : UnityTuning.Rope33LooseSlack)); // rope.js :84
                Vector3 mid = (hand + end) * 0.5f;
                mid.y -= Mathf.Min(UnityTuning.Rope33SagMax, slack * sag33 * UnityTuning.Rope33SagScale); // rope.js :86
                for (int i = 0; i <= segments33; i++) rope.SetPosition(i, Curve33(hand, mid, end, i / (float)segments33));
                ropeMat.SetColor(DesignKit.BaseColor, ropeColor * (r.taut || taut35 ? UnityTuning.Rope33TautGlow : UnityTuning.Rope33SlackGlow));
                rope.enabled = true;
                hookSpin33 += dt * UnityTuning.Rope33HookSpin;
                hook33.transform.SetPositionAndRotation(end, Quaternion.Euler(45, hookSpin33 * Mathf.Rad2Deg, 45));
                hook33.gameObject.SetActive(true);
                wasRoped33 = true;
                snapAge33 = float.PositiveInfinity;
                return;
            }
            if (wasRoped33)
            {
                wasRoped33 = false;
                snapAge33 = 0; // the release: the snap starts from the last end
            }
            if (snapAge33 < UnityTuning.Rope33SnapTime)
            {
                float k = snapAge33 / UnityTuning.Rope33SnapTime;
                Vector3 tip = Vector3.Lerp(lastEnd33, hand, k * k); // it whips back, faster as it comes in
                Vector3 side = game.cam.transform.right;
                float amp = UnityTuning.Rope33SnapWave * (1 - k);
                for (int i = 0; i <= segments33; i++)
                {
                    float t = i / (float)segments33;
                    rope.SetPosition(i, Vector3.Lerp(hand, tip, t) + side * (amp * Mathf.Sin(t * Mathf.PI * 3 + k * 10) * t));
                }
                ropeMat.SetColor(DesignKit.BaseColor, ropeColor * UnityTuning.Rope33SlackGlow * (1 - k));
                rope.enabled = true;
                hook33.transform.position = tip;
                hook33.gameObject.SetActive(true);
                snapAge33 += dt;
                return;
            }
            rope.enabled = false;
            hook33.gameObject.SetActive(false);
        }
    }
}
