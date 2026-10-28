// NIGHT32 (builder F, DESIGN32 card F, row design.n32.level5): LEVEL 5 THE SPIRE's registration.
//   1. the campaign order with the row on has 5 levels, THE SPIRE fifth (right after n30-span); off or forced off (--off32): 4
//   2. the design config names it (id, after), the map list carries its campaign-only row, the unlock rows give all four weapons from the start and the TORCH at the end
//   3. the level file loads: 9 rooms, every combat room at most 24 m across, the last the boss room v9 with boss "crown"
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Level5Tests
    {
        static string Repo => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        static GameConfig Real() => GameConfig.Load(Path.Combine(Repo, "PortExport", "config.json"));

        static GameConfig Synthetic(bool level5) => new GameConfig(new Dictionary<string, object>
        {
            ["design"] = new Dictionary<string, object>
            {
                ["n30"] = new Dictionary<string, object> { ["levels"] = new Dictionary<string, object> { ["enabled"] = true, ["order"] = new List<object> { "n30-foundry", "n30-iceline", "n30-kiln", "n30-span" } } },
                ["n32"] = new Dictionary<string, object> { ["level5"] = new Dictionary<string, object> { ["enabled"] = level5, ["id"] = "n32-spire", ["after"] = "n30-span" } },
            },
        });

        [Test]
        public void TheOrder_WithLevel5On_HasFiveLevels_TheSpireFifth()
        {
            var all = new N25Blocks { tutorial = true, curriculum = true };
            bool was = N32Blocks.ForceOff, was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = false;
            try
            {
                N32Blocks.ForceOff = false;
                List<string> on = Curriculum25.Order(Synthetic(true), all);
                Assert.That(on.Count, Is.EqualTo(5), string.Join(",", on));
                Assert.That(on[4], Is.EqualTo("n32-spire"), "level 5 right after level 4");
                Assert.That(Curriculum25.Next(Synthetic(true), all, "n30-span"), Is.EqualTo("n32-spire"), "RESULTS after level 4 continues to THE SPIRE");
                Assert.That(Curriculum25.Order(Synthetic(false), all).Count, Is.EqualTo(4), "the row off: NIGHT31's four");
                N32Blocks.ForceOff = true;
                Assert.That(Curriculum25.Order(Synthetic(true), all).Count, Is.EqualTo(4), "--off32: NIGHT31's four whatever the row says");
            }
            finally { N32Blocks.ForceOff = was; N30Blocks.ForceOff = was30; }
        }

        [Test]
        public void TheDesignConfig_RegistersTheSpire_WithAllFourWeapons()
        {
            GameConfig cfg = Real();
            Assert.That(cfg.Str("design.n32.level5.id"), Is.EqualTo("n32-spire"));
            Assert.That(cfg.Str("design.n32.level5.after"), Is.EqualTo("n30-span"));
            bool mapRow = false;
            foreach (object o in cfg.Arr("design.maps.list"))
            {
                var d = PortData.Obj(o);
                if ((string)d["id"] != "n32-spire") continue;
                mapRow = true;
                Assert.That(d["file"], Is.EqualTo("Design/levels/n32-spire.json"));
                Assert.That(d.TryGetValue("campaignOnly", out object co) && co is bool b && b, Is.True, "never on the MAP row");
                Assert.That(File.Exists(Path.Combine(Repo, "Design", "levels", "n32-spire.json")), Is.True);
            }
            Assert.That(mapRow, Is.True, "design.maps.list has the LEVEL 5 row");
            List<string> start = Unlocks31.From(cfg).StartFor("n32-spire");
            Assert.That(start, Is.EquivalentTo(new[] { "revolver", "gauntlet", "shotgun", "nailgun" }), "KESTREL, PULSE gauntlet, THUNDERCLAP, NAILGUN from the start");
            Unlocks26 table = Unlocks26.Parse(File.ReadAllText(Path.Combine(Repo, "Design", "levels", "unlocks.json")));
            Assert.That(table.Earned("n32-spire"), Is.EqualTo("torch"), "the old table's level-5 weapon, earned with the last room");
        }

        [Test]
        public void TheSpire_LoadsNineRooms_SmallCombatRooms_TheCrownLast()
        {
            LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(Path.Combine(Repo, "Design", "levels", "n32-spire.json"))));
            Assert.That(level.id, Is.EqualTo("n32-spire"));
            Assert.That(level.rooms.Count, Is.EqualTo(9));
            foreach (RoomDef r in level.rooms)
                if (r.kind == "room") Assert.That(System.Math.Max(r.x1 - r.x0, r.z1 - r.z0), Is.LessThanOrEqualTo(24), r.id + " is at most 24 m across");
            RoomDef last = level.rooms[level.rooms.Count - 1];
            Assert.That(last.id, Is.EqualTo("v9"));
            Assert.That(last.kind, Is.EqualTo("boss"));
            Assert.That(last.boss, Is.EqualTo("crown"), "builder C's THE CROWN");
        }
    }
}
