// NIGHT43 builder A1 (F1's hand-off through the lead: "during a FUSION's freeze, shake / squash the hit enemy's mesh in your views"): a FUSION
// (W1's N43Events.Fusion "fusion43", Fusion43Event.enemy) makes its victim's body shake across the screen and squash-pulse for
// UnityTuning.Fusion43ShakeTime s of UNSCALED time (it plays through the hitstop's freeze, which holds world time), fading out. Added to
// EnemyLook.React's answer, so every view that reacts (husk, swarmer, spitter, charger, drone, warden, burrower) shows it. Render only; with no
// FUSION (fusion43 off) nothing is emitted and React is unchanged. REDUCED MOTION / EFFECTS 0: none (FxGates.MotionOn).
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class EnemyLook
    {
        int fusionId = NoEnemy;
        float fusionAge = Idle;
        int fusionFrame = -1;

        void Fusion43Init()
        {
            game.events.On<Skygrind.Core.Fusion43Event>(Skygrind.Core.N43Events.Fusion, e =>
            {
                fusionId = e.enemy;
                fusionAge = 0;
                fusionFrame = Time.frameCount;
            });
            game.OnNewRun += () => { fusionId = NoEnemy; fusionAge = Idle; };
        }

        void Fusion43React(int id, ref Vector3 offset, ref Vector3 squash)
        {
            if (id != fusionId) return;
            if (fusionFrame != Time.frameCount)
            {
                fusionFrame = Time.frameCount;
                fusionAge += FeelClock.UnscaledDelta; // through the freeze (NIGHT63 D1: 0 in a same-run held frame)
            }
            float T = UnityTuning.Fusion43ShakeTime;
            if (fusionAge >= T) return;
            Skygrind.Core.MovementConfig mc = game.cfg.movement;
            if (!FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity)) return;
            float fade = 1 - fusionAge / T;
            float w = fusionAge * UnityTuning.Fusion43ShakeHz * 2 * Mathf.PI;
            offset += (screenRight * Mathf.Sin(w) + screenUp * Mathf.Sin(w * 1.37f + 1.1f) * 0.6f) * (UnityTuning.Fusion43ShakeAmp * fade);
            float s = UnityTuning.Fusion43Squash * fade * Mathf.Abs(Mathf.Cos(w * 0.5f));
            squash = Vector3.Scale(squash, new Vector3(1 + s * 0.6f, 1 - s, 1 + s * 0.6f));
        }
    }

    public static partial class UnityTuning
    {
        public const float Fusion43ShakeTime = 0.45f; // s (unscaled) the FUSION victim shakes: past the 0.22 s freeze (0.28 s on a boss) and into the knockback
        public const float Fusion43ShakeHz = 26f; // the shake's rate: a hard buzz, faster than the hit vibration
        public const float Fusion43ShakeAmp = 0.09f; // m across the screen at its start (fades to 0)
        public const float Fusion43Squash = 0.22f; // the squash-pulse's share at its start (y down, x / z out x 0.6)
    }
}
