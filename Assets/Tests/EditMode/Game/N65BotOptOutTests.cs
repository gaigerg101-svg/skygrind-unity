// NIGHT65 builder T (T1; redteam.md 5): the walk bot's opt-out. A room rebuilt in place under its old id (n65-foundry's six slots keep a2 .. a8)
// would still be driven by the old room's id-keyed scripts (Walk36's a3 / a5, Walk38Foundry's b2, Walk43Foundry's b2 / a7 / a8: Pierce43's lip at
// lz 18.2, its perch, P2), which steer by the OLD geometry. A row with "bot43": false skips them: the generic autopilot plays that room. The old
// level's rows carry no such key, so every pilot runs as before (n46flow on n52-foundry equal to the flow base: Research/2026-10-23-wave65/T/).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N65BotOptOutTests
    {
        static readonly string Repo = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo, "PortExport", "config.json"));

        static Dictionary<string, object> Level(string id) => PortData.Obj(PortData.LoadJson(Path.Combine(Repo, "Design", "levels", id + ".json")));

        static void OptOut(Dictionary<string, object> json, string room)
        {
            foreach (object o in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
            {
                var r = PortData.Obj(o);
                if ((string)r["id"] == room) r["bot43"] = false;
            }
        }

        [Test]
        public void TheOldLevels_HaveNoOptOut()
        {
            foreach (string id in new[] { "n52-foundry", "n52-iceline", "n57-kiln", "n52-span", "n52-spire" })
                Assert.That(BotRows65.OffRooms(Level(id)), Is.Empty, id + ": no row says bot43 false, so every id script runs as before");
        }

        [Test]
        public void Bot43False_OptsOutThatRoomOnly()
        {
            var json = Level("n52-foundry");
            OptOut(json, "a8");
            Assert.That(BotRows65.OffRooms(json), Is.EquivalentTo(new[] { "a8" }));
            var keep = Level("n52-foundry");
            PortData.Obj(PortData.Arr(PortData.Obj(keep["campaign"])["rooms"])[1])["bot43"] = true; // true (or absent) keeps the scripts
            Assert.That(BotRows65.OffRooms(keep), Is.Empty);
        }

        /// <summary>The walk pilot (Walk38Pilot, n46flow's) put at a8 PIERCE's spawn on n52-foundry for 12 s: the steps on which Walk43Foundry's
        /// id script drove the move, and whether the campaign took the room.</summary>
        static (int scripted, bool entered) RunA8(bool optOut)
        {
            var json = Level("n52-foundry");
            if (optOut) OptOut(json, "a8");
            SurvivalMap map = SurvivalMap.FromExport(json);
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            var ap = new AutopilotInput(sim, "normal", 1);
            int a8 = -1;
            ap.GoalProvider = () => sim.campaignLevel.rooms[a8 < 0 ? 0 : a8].spawn;
            var pilot = new Walk38Pilot(sim, ap);
            sim.UseInput(pilot);
            sim.StartRun(1, null, null);
            Assert.That(sim.campaignLevel, Is.Not.Null);
            Assert.That(sim.campaignLevel.id, Is.EqualTo("n52-foundry"));
            a8 = sim.campaignLevel.rooms.FindIndex(r => r.id == "a8");
            Assert.That(a8, Is.GreaterThan(0));
            RoomDef rd = sim.campaignLevel.rooms[a8];
            bool placed = false, entered = false;
            int steps = MovementMath.StepsFor(12, sim.step);
            for (int s = 0; s < steps; s++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Campaign24 c = sim.campaign;
                if (c == null || c.room < 0) continue;
                if (!placed) { c.SetDoorFree29(true); sim.Respawn(rd.spawn, rd.spawnYawDeg); placed = true; continue; }
                if (c.room == a8 && !entered) { entered = true; c.SetDoorFree29(false); }
            }
            ap.Detach();
            return (pilot.idScriptSteps65, entered);
        }

        [Test]
        public void InA8_TheOldIdScriptDrives_UnlessTheRowSaysBot43False()
        {
            var (on, enteredOn) = RunA8(false);
            var (off, enteredOff) = RunA8(true);
            Assert.That(enteredOn && enteredOff, Is.True, "the campaign took a8 in both runs (so a 0 below is not vacuous)");
            Assert.That(on, Is.GreaterThan(0), "n52-foundry's a8 (BaseId n43-foundry): Walk43Foundry's Pierce43 drives the move");
            Assert.That(off, Is.EqualTo(0), "bot43 false: no id script step, the generic autopilot plays a8");
        }
    }
}
