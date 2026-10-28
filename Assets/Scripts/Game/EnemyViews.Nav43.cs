// NIGHT43 builder A1 (round 2, the lead's ask; E1's G22 on-screen relocation: HuskBrain.Watchdog43 and SurvivalSim.N43.Rooms' unstick emit
// "navTell43" (the enemy's id) 0.35 s before they step a stuck body to a free node, then "navRelocate43" (its id) on the step): the step must not
// look like a pop. On the tell the body GATHERS (it crouches: y down, wider, eased in over the tell), shivers, its visor strobes (models43's
// VISOR role reads EnemyLook.NavFlicker43) and a little dust kicks at its feet; on the relocation it ARRIVES (it springs up out of the same
// crouch over UnityTuning.Nav43ArriveTime, a dust puff at the new spot): a hop, not a teleport. Added to EnemyLook.React's answer (every kind).
// Render only; with rooms43 off nothing emits them and React is unchanged. Pooled slots, no allocation per frame.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class EnemyLook
    {
        struct Nav43
        {
            public int id;
            public float tellAt, arriveAt; // Time.unscaledTime of the beats (-1 none)
        }

        readonly Nav43[] nav43 = new Nav43[UnityTuning.Nav43Slots];
        int nav43Next;
        Color nav43Dust;

        /// <summary>The visor strobe of the enemy React was last asked about (0 none; models43's VISOR role multiplies by 1 + this).</summary>
        public float NavFlicker43 { get; private set; }

        void Nav43Init()
        {
            for (int i = 0; i < nav43.Length; i++) nav43[i] = new Nav43 { id = NoEnemy, tellAt = -1, arriveAt = -1 };
            nav43Dust = Look.C(game.cfg, "colors.slamDust");
            game.events.On("navTell43", o => Nav43Beat(o, true));
            game.events.On("navRelocate43", o => Nav43Beat(o, false));
            game.OnNewRun += () => { for (int i = 0; i < nav43.Length; i++) nav43[i].id = NoEnemy; };
        }

        void Nav43Beat(object o, bool tell)
        {
            if (!(o is int id)) return;
            int slot = -1;
            for (int i = 0; i < nav43.Length; i++) if (nav43[i].id == id) { slot = i; break; }
            if (slot < 0)
            {
                slot = nav43Next;
                nav43Next = (nav43Next + 1) % nav43.Length;
                nav43[slot] = new Nav43 { id = id, tellAt = -1, arriveAt = -1 };
            }
            if (tell) nav43[slot].tellAt = FeelClock.UnscaledTime; // NIGHT63 D1: the beats on FeelClock's held clock
            else
            {
                nav43[slot].arriveAt = FeelClock.UnscaledTime;
                nav43[slot].tellAt = -1;
            }
            foreach (Skygrind.Core.Enemy e in game.enemies.all)
            {
                if (e.id != id || !e.alive) continue;
                game.Particles43?.Burst(e.position, nav43Dust, UnityTuning.Nav43DustCount, 2.2f, 0.4f, 0.8f);
                break;
            }
        }

        void Nav43React(int id, ref Vector3 offset, ref Vector3 squash)
        {
            NavFlicker43 = 0;
            for (int i = 0; i < nav43.Length; i++)
            {
                if (nav43[i].id != id) continue;
                float now = FeelClock.UnscaledTime;
                float crouch = 0;
                if (nav43[i].tellAt >= 0)
                {
                    float t = now - nav43[i].tellAt;
                    if (t < UnityTuning.Nav43TellMax)
                    {
                        crouch = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / UnityTuning.Nav43TellTime));
                        NavFlicker43 = UnityTuning.Nav43Flicker * (Mathf.Sin(t * UnityTuning.Nav43FlickerHz * 2 * Mathf.PI) > 0 ? 1 : -0.6f);
                        offset += screenRight * (Mathf.Sin(t * 47) * UnityTuning.Nav43Shiver * crouch);
                    }
                }
                if (nav43[i].arriveAt >= 0)
                {
                    float t = (now - nav43[i].arriveAt) / UnityTuning.Nav43ArriveTime;
                    if (t < 1)
                    {
                        // springs up out of the crouch: below 1 at once, a small stretch past it, settled at 1
                        float k = 1 - t;
                        crouch = Mathf.Max(crouch, k * k);
                        if (t > 0.35f && t < 0.8f) squash = Vector3.Scale(squash, new Vector3(0.96f, 1.06f, 0.96f));
                    }
                }
                if (crouch > 0)
                {
                    float d = UnityTuning.Nav43Crouch * crouch;
                    squash = Vector3.Scale(squash, new Vector3(1 + d * 0.5f, 1 - d, 1 + d * 0.5f));
                }
                return;
            }
        }
    }

    public static partial class UnityTuning
    {
        public const int Nav43Slots = 8; // enemies told / relocated at once (the oldest slot reused)
        public const float Nav43TellTime = 0.3f; // s the crouch takes to gather (E1's tell is 0.35 s: rooms43.onScreenTellS)
        public const float Nav43TellMax = 0.6f; // a tell with no relocation after it lets go after this (s)
        public const float Nav43Crouch = 0.3f; // the crouch: y x (1 - this), x / z x (1 + half of it)
        public const float Nav43Shiver = 0.035f; // m of shiver across the screen while it gathers
        public const float Nav43Flicker = 1.6f; // the visor's strobe: x (1 + this) on, x (1 - 0.6 this) off (models43's VISOR)
        public const float Nav43FlickerHz = 14f;
        public const float Nav43ArriveTime = 0.22f; // s it takes to spring up at the new spot
        public const int Nav43DustCount = 5; // cubes of dust at the old spot (the tell) and the new one (the step)
    }
}
