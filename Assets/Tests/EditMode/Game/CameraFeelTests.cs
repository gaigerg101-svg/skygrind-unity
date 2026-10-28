// NIGHT16 polish (a web port): CameraFeel's rank-up kick (player.js :207-210: on `rankChange` with up, kicks.add(style.kickDeg,
// kickAttack, kickRecover)). The port's `rankChange` carries the rank only, so "up" is a rank above the last one seen; a
// respawn (Cut) starts from the lowest rank again.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class CameraFeelTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static (CameraFeel feel, EventBus bus) Make()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            var mover = new Mover(cfg.movement, new TriangleWorld());
            var feel = new CameraFeel(cfg.movement, mover);
            var bus = new EventBus();
            feel.Wire(bus, cfg);
            return (feel, bus);
        }

        [Test]
        public void ARankUpKicks_AFallOrTheSameRankDoesNot()
        {
            var (feel, bus) = Make();
            bus.Emit("rankChange", 1); // up from the lowest
            bus.Emit("rankChange", 2); // up
            bus.Emit("rankChange", 2); // the same
            bus.Emit("rankChange", 1); // down
            bus.Emit("rankChange", 2); // up again
            Assert.That(feel.RankUpKicks, Is.EqualTo(3));
        }

        [Test]
        public void ARespawnStartsFromTheLowestRank()
        {
            var (feel, bus) = Make();
            bus.Emit("rankChange", 3);
            feel.Cut();
            bus.Emit("rankChange", 1); // a new run's first rank-up
            Assert.That(feel.RankUpKicks, Is.EqualTo(2));
        }
    }
}
