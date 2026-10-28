// NIGHT48 builder F (design.n48.foes48, a view row; NIGHT48.md G14; F-cards 5c "legs on a 2-phase gait cycle tied to its speed (so it
// scuttles instead of sliding)"). NIGHT47's foes47 only breathed the four leg spikes' width at 6 Hz by the clock (Swarmer47ScuttleHz): the
// same flutter standing still or running, no step. With foes48 on: the legs' one mesh is one of Mesh43.Legs48's poses (diagonal pairs: one
// pair swings forward and lifts while the other swings back), picked by the distance the body was DRAWN to move (Legs48StrideM a cycle), so a
// walking swarmer steps and a still one stands (it eases to a rest pose); the leap (pounce) holds the pose it left the ground in. Frame time,
// render only; the per-body state is a reused dictionary entry (no allocation after a body's first frame).
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SwarmerView
    {
        readonly Dictionary<Transform, Vector4> gait48 = new Dictionary<Transform, Vector4>(); // per pooled body: the last drawn position (xyz), the gait's phase in cycles (w)

        /// <summary>For the a948 gait strip's line: the last body's gait pose index and its phase.</summary>
        public static int Gait48Pose;
        public static float Gait48Phase;

        void Gait48(Body b, float dt)
        {
            if (b.legsF47 == null) return;
            Vector3 p = b.root.position;
            if (!gait48.TryGetValue(b.root, out Vector4 st)) st = new Vector4(p.x, p.y, p.z, 0);
            float dx = p.x - st.x, dz = p.z - st.z;
            float step = Mathf.Sqrt(dx * dx + dz * dz);
            if (step > 2f || b.state47 == "pounce") step = 0f; // a teleport, a reused body or the leap: no stride
            float ph = st.w + step / UnityTuning.Legs48StrideM;
            if (step < 1e-4f && b.state47 != "pounce")
            {
                // still: ease to the nearest rest pose (half a cycle apart: both are the four legs at rest)
                float whole = Mathf.Floor(ph), frac = ph - whole;
                float target = frac < 0.25f ? 0f : frac < 0.75f ? 0.5f : 1f;
                ph = whole + Mathf.MoveTowards(frac, target, dt * 2f);
            }
            if (ph > 1000f) ph -= Mathf.Floor(ph);
            gait48[b.root] = new Vector4(p.x, p.y, p.z, ph);
            int n = UnityTuning.Legs48Phases;
            int idx = Mathf.FloorToInt((ph - Mathf.Floor(ph)) * n + 0.5f) % n;
            b.legsF47.sharedMesh = Mesh43.Legs48(idx);
            Gait48Pose = idx;
            Gait48Phase = ph;
        }
    }
}
