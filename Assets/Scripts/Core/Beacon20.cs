// NIGHT20 A3 (builder P; Research/2026-10-06-additions-backlog.md row 16: Risk of Rain 2's Shrine of the Mountain, an
// opt-in harder teleporter event for more loot; Deep Rock Galactic's Machine Events, an optional fight you can ignore):
// THE BEACON's rule (design.adds.beacon). Port-only: the web has no opt-in challenge.
// - It stands in the break before round `round` when the row is on, from round `from`, before a normal or gold round only
//   (never before a boss, hunt (the web's swarm round) or HOLD round: those are rituals of their own), with probability
//   `chance`: a draw on a stream of its own from the run seed ^ salt and the round (the run's streams are never drawn, so an
//   ignored beacon changes nothing).
// - Where: a seeded pick of the spots offered (the run side offers the map's HOLD ring spots in open zones).
// The run side (Game/SurvivalSim.Beacon20.cs: Beacon20Run) makes it a free buy; taken, the next round adds `escortSquads`
// squads and `elites` elites of `kind` (the web's CHARGER through builder E's ELITE: Director.SpawnElite).
using System;

namespace Skygrind.Core
{
    /// <summary>THE BEACON's bus payload (`beaconStand`, `beaconTake`, `beaconEscort`: GG's sound hooks): the round it is for
    /// and where it stands. Reused (read it inside the handler).</summary>
    public sealed class BeaconEvent
    {
        public int round;
        public Vec3 position;
        /// <summary>`beaconEscort`: the squads added and the elites called.</summary>
        public int squads, elites;
    }

    public static class Beacon20
    {
        /// <summary>The bus events (port-only; GG's sounds): it stands in a break, it is taken, its round begins with the escort.</summary>
        public const string StandEvent = "beaconStand", TakeEvent = "beaconTake", EscortEvent = "beaconEscort";

        /// <summary>design.adds.beacon.</summary>
        public struct P
        {
            public bool enabled;
            public int from, elites, escortSquads;
            public double chance;
            public string kind;
            public uint salt;

            /// <summary>The row's numbers; a config without the row: off (default).</summary>
            public static P From(GameConfig cfg)
            {
                const string k = "design.adds.beacon.";
                if (!cfg.Has(k + "salt")) return default;
                return new P
                {
                    enabled = cfg.Design("adds.beacon.enabled"), from = cfg.Int(k + "from"), chance = cfg.Num(k + "chance"),
                    elites = cfg.Int(k + "elites"), escortSquads = cfg.Int(k + "escortSquads"), kind = cfg.Str(k + "kind"),
                    salt = (uint)cfg.Num(k + "salt"),
                };
            }
        }

        /// <summary>The beacon stands in the break before `round` of the run seeded `runSeed` (a pure function of both).</summary>
        public static bool Stands(uint runSeed, int round, in P p, DirectorMath.D d)
        {
            if (!p.enabled || round < p.from) return false;
            string type = DirectorMath.RoundType(round, d);
            if (type != "normal" && type != "gold") return false;
            return Draw(runSeed, round, p.salt, 0) < p.chance;
        }

        /// <summary>Which of `count` offered spots it stands at (-1: none offered, no beacon).</summary>
        public static int SpotIndex(uint runSeed, int round, int count, in P p)
        {
            if (count <= 0) return -1;
            return Math.Min(count - 1, (int)Math.Floor(Draw(runSeed, round, p.salt, 1) * count));
        }

        /// <summary>One draw on the beacon's own stream for (seed, round, lane): never the run's streams.</summary>
        static double Draw(uint runSeed, int round, uint salt, uint lane) =>
            new Mulberry32(unchecked(runSeed ^ salt ^ (uint)round * 2654435761u ^ lane * 0x85ebca6bu)).Next();
    }
}
