// NIGHT24 (builder L): a CAMPAIGN run's tape (format 8) replays EQUAL. A bot plays the first 25 s of layer 1 (room 1's fight) on the written level
// (Design/levels/layer1.json), the tape goes through its bytes, a fresh composition replays it: every checkpoint hash, the last step's light hash and
// the campaign's state (level, room, checkpoint, limits, per-room clear steps) are EQUAL; with the campaign bit cleared the same tape DIFFERS.
// The headless counterpart over rooms 1-4 is `campaign --tape` (Tools/SimHarness).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Campaign24TapeTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Level() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "levels", "layer1.json")))));

        [Test]
        public void ACampaignTape_ReplaysEqual_WithItsCampaignState()
        {
            GameConfig cfg = Cfg();
            SurvivalMap map = Level();
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.n24Override = new N24Blocks { campaign = true, wyrm = true };
            sim.setup.mode = Campaign24.Mode;
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            var ap = new AutopilotInput(sim, "normal", 1);
            ap.GoalProvider = () => sim.campaignLevel.rooms[0].spawn;
            sim.UseInput(ap);
            RunTape tape = TapeScript.Record(sim, 1, MovementMath.StepsFor(25, sim.step), "tests", null, null, scripted: false);
            ap.Detach();
            RunTape back = RunTape.FromBytes(tape.ToBytes());
            Assert.That(back.mode, Is.EqualTo(Campaign24.Mode));
            Assert.That(back.campaign.levelId, Is.EqualTo("layer1"), "the tape names the level");
            Assert.That(N24Blocks.FromBits(back.n24).campaign, Is.True);
            TapeResult r = TapeReplay.Replay(cfg, map, back);
            Assert.That(r.Equal, Is.True, r.Line);
            Assert.That(r.Sim.campaign24.Same(sim.campaign24), Is.True, "room, checkpoint, limits");
            Assert.That(r.Sim.campaign.clearSteps.Count, Is.EqualTo(sim.campaign.clearSteps.Count));
            Assert.That(TapeReplay.Light(r.Sim), Is.EqualTo(TapeReplay.Light(sim)), "the last step's state");
            back.n24 &= ~(1 << 5);
            Assert.That(TapeReplay.Replay(cfg, map, back).Equal, Is.False, "without the campaign bit it is another game");
        }
    }
}
