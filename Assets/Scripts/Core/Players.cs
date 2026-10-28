// NIGHT9 (the "if time" item: PORT-PLAN.md N10's first item, U4-01's code half): the players the sim holds.
// The web has one player and one fixed target (main.js: const target = {position, health, mover}; getTarget = () =>
// target); skygrind/COOP-RESEARCH.md turns that into getTarget(fromPosition): every enemy hunts the nearest living
// player, asked at the moment it needs the target (a husk's tick, the brain's think, its line-of-sight check). This is
// new design on both sides (the web never had it), so nothing here has a golden vector. With one player every answer is
// that player, which keeps the golden sets and the 22 replays as they were.
// Per player so far: the Mover, the Health (owner = id), and the slots for the Regen, Loadout, AimAssist and StyleMeter
// that SurvivalGame builds for it. Still single (N10 / U4-02): the Director's spawn picks and camp state, the wallet
// (COOP-RESEARCH: one wallet each), the buyables, power-up grabs, zones, the mods' hooks.
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>
    /// One player's systems and its owner id: the id every event it causes carries ("player" for the first, as the web's
    /// owner ids; Health and Regen already filter on it).
    /// </summary>
    public sealed class Player
    {
        public readonly string id;
        public readonly Mover mover;
        public readonly Health health;
        public Regen regen;
        public Loadout loadout;
        public AimAssist assist;
        public StyleMeter style;
        public double yaw; // the look yaw this step (the brain's front cone, flank points and relocate cone read it)

        public Player(string id, Mover mover, Health health)
        {
            this.id = id;
            this.mover = mover;
            this.health = health;
        }

        public Vec3 Feet => mover.position;
        public Vec3 Velocity => mover.velocity;
        public bool Alive => !health.Dead;
    }

    /// <summary>The players in join order, and the enemies' target picker (getTarget(from)). Holds at least one.</summary>
    public sealed class Players
    {
        public readonly List<Player> list = new List<Player>();

        public int Count => list.Count;
        public Player this[int i] => list[i];

        public Player Add(Player p)
        {
            list.Add(p);
            return p;
        }

        public int Living
        {
            get
            {
                int n = 0;
                for (int i = 0; i < list.Count; i++) if (list[i].Alive) n++;
                return n;
            }
        }

        public Player Nearest(Vec3 from)
        {
            int best = 0;
            double bestD = double.PositiveInfinity;
            for (int i = 0; i < list.Count; i++)
            {
                Player p = list[i];
                if (!p.Alive) continue;
                double d = DistanceSq(from, p.Feet);
                if (d < bestD)
                {
                    bestD = d;
                    best = i;
                }
            }
            return list[best];
        }

        /// <summary>
        /// The rule behind Nearest, on positions a system cached itself (the brain keeps the feet it saw at its step): the
        /// index of the nearest living one by squared 3D distance (no square root, so no rounding to argue about); a tie
        /// keeps the earlier player; nobody alive gives 0, so a solo run's enemies keep the one target they always had
        /// (they check target.Dead themselves).
        /// </summary>
        public static int NearestOf(Vec3 from, Vec3[] at, bool[] alive, int count)
        {
            int best = 0;
            double bestD = double.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                if (!alive[i]) continue;
                double d = DistanceSq(from, at[i]);
                if (d < bestD)
                {
                    bestD = d;
                    best = i;
                }
            }
            return best;
        }

        static double DistanceSq(Vec3 a, Vec3 b)
        {
            double dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z;
            return dx * dx + dy * dy + dz * dz;
        }
    }
}
