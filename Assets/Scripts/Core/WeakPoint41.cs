// NIGHT41 (the shared commit, the lead; NIGHT41.md G0's pre-placed hooks): a boss body's weak point, the spot aim41 steers the pad's aim
// assist to (P3, Core/AimAssist.cs) instead of the body's centre (Enemy.AimPoint). Every Enemy carries one (Enemy.weakPoint41), inactive by
// default, so nothing reads it until a boss builder fills it: B8 for THE LOOM (its lit knee) and THE ICEBREAKER (its stuck fist), P3 for THE
// CROWN (its ring while passable), each from its own sim step. A plain value: no allocation, nothing on a tape (the boss's state is).
namespace Skygrind.Core
{
    public struct WeakPoint41
    {
        /// <summary>Where the weak point is now (world metres).</summary>
        public Vec3 at;

        /// <summary>Is it worth aiming at now (the CYAN core in a window, the lit knee, the passable ring, the stuck fist)? False: the body's centre.</summary>
        public bool active;

        public void Set(Vec3 p) { at = p; active = true; }

        public void Clear() { active = false; }
    }
}
