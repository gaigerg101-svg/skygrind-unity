// NIGHT13 U3-26.3: automation's input (the fingerprint, the bench's play scenario, the headless tests): the next step's
// movement frame and combat input are set before the step (Set), and the look is its own. Sampling inactive returns the
// look only (as the readers do); nothing is latched beyond one step. No UnityEngine use.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class ScriptedInput : IPlayerInput
    {
        double yaw, pitch;
        InputFrame move;
        CombatInput combat = new CombatInput { weapon = -1 };

        public double Yaw => yaw;
        public double Pitch => pitch;
        public bool Captured => true;

        public void SetLook(double newYaw, double newPitch)
        {
            yaw = newYaw;
            pitch = newPitch;
        }

        /// <summary>The next step's input (its yaw / pitch fields are ignored: the look is SetLook's).</summary>
        public void Set(InputFrame nextMove, CombatInput nextCombat)
        {
            move = nextMove;
            combat = nextCombat;
        }

        public InputFrame SampleMove(bool active)
        {
            InputFrame f = active ? move : default;
            f.yaw = yaw;
            f.pitch = pitch;
            move = default;
            return f;
        }

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = active ? combat : new CombatInput { weapon = -1 };
            combat = new CombatInput { weapon = -1 };
            return c;
        }
    }
}
