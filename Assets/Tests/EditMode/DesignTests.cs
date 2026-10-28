// NIGHT8 U2-07: the Unity design config (Design/unity-config.json as the `design.` prefix; config.json may never have a
// `design` key) and PERFECT PARRY: a punch in the last design.parry.perfectWindow of a husk wind-up heals to max HP and
// emits a perfect parry (the style meter pays PERFECT PARRY); earlier in the wind-up it is the web's parry (+parryHeal);
// with the design flag off it is always the web's parry.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class DesignTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void TheDesignConfig_IsTheDesignPrefix_AndConfigJsonMayNotHaveOne()
        {
            Assert.That(cfg.Num("design.parry.perfectWindow"), Is.EqualTo(0.15));
            Assert.That(cfg.Num("design.parry.perfectHitstop"), Is.EqualTo(0.15));
            Assert.That(cfg.Design("parry.perfect"), Is.True);
            Assert.That(cfg.Design("director.peakFade"), Is.True);
            Assert.That(cfg.Design("no.such.flag"), Is.False);
            var raw = PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "config.json")));
            Assert.That(raw.ContainsKey("design"), Is.False, "the web config has no design key");
            string tmp = Path.Combine(Path.GetTempPath(), "skygrind-design-clash.json");
            File.WriteAllText(tmp, "{\"design\":{\"x\":1}}");
            try
            {
                Assert.Throws<InvalidDataException>(() => GameConfig.Load(tmp));
            }
            finally
            {
                File.Delete(tmp);
            }
        }

        [Test]
        public void TheGoldenAndReplayConfig_HasEveryNight12DesignFlagOff_TheGamesHasThem()
        {
            // NIGHT12: the golden vectors and replays run on config.json alone (GoldenData.Config: no design config), so
            // every new design flag reads off there: the web's game. The game's config (with Design/unity-config.json) has
            // them on, except HEAT, which is on as a ladder but opt-in per run (rung 0 = the web's game).
            string[] flags = { "contracts.enabled", "heat.enabled", "grade.p", "onboarding.enabled", "mastery.enabled", "ceremony.round", "ceremony.resultsTally", "ceremony.livingTitle", "ceremony.hudMotion", "payoff.enabled" };
            GameConfig golden = new GoldenData(Port.Root).Config;
            foreach (string f in flags)
            {
                Assert.That(golden.Design(f), Is.False, $"golden config: design.{f}");
                Assert.That(golden.Has("design." + f), Is.False, $"golden config has design.{f}");
                Assert.That(cfg.Design(f), Is.True, $"game config: design.{f}");
            }
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Mover mover;
            public readonly Health health;
            public readonly Loadout loadout;
            public readonly Husks husks;
            public readonly StyleMeter style;
            public readonly List<HitEvent> parries = new List<HitEvent>();
            public readonly List<string> awards = new List<string>();
            public Vec3 eye = new Vec3(0, 1.6, 0), dir = new Vec3(0, 0, -1);

            public Rig(GameConfig c)
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
                mover = new Mover(c.movement, world);
                mover.Reset(0, 0, 0);
                health = new Health(100, events, "player");
                loadout = new Loadout(c, events, enemies, world, mover, health, () => (eye, dir));
                husks = new Husks(c, events, enemies, world, -30);
                style = new StyleMeter(c, events, () => "GROUND", () => true);
                style.Start();
                events.On<HitEvent>("parry", e => parries.Add(e));
                events.On<StyleAwardEvent>("styleAward", a => awards.Add(a.name));
            }

            public Husk PunchAt(int windupSteps)
            {
                Husk husk = husks.Spawn(new Vec3(0, 0, -1.6), new Vec3(0, 0, 0));
                husk.windupSteps = windupSteps;
                health.hp = 50;
                dir = Vec3.Normalize(new Vec3(0, husk.AimHeight - 1.6, -1.6));
                loadout.Step(new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } }, cfg.Dt);
                return husk;
            }
        }

        [Test]
        public void APunchInTheLastWindow_IsAPerfectParry_EarlierIsTheWebsParry()
        {
            int window = MovementMath.StepsFor(cfg.Num("design.parry.perfectWindow"), cfg.Dt);
            var perfect = new Rig(cfg);
            Husk h = perfect.PunchAt(window - 2);
            Assert.That(perfect.parries.Count, Is.EqualTo(1));
            Assert.That(perfect.parries[0].perfect, Is.True);
            Assert.That(perfect.health.hp, Is.EqualTo(perfect.health.max), "healed to max HP");
            Assert.That(h.windupSteps, Is.EqualTo(0), "the wind-up is cancelled");
            Assert.That(perfect.awards, Does.Contain("PERFECT PARRY"));

            var early = new Rig(cfg);
            early.PunchAt(window + 12); // mid wind-up, before the perfect window
            Assert.That(early.parries[0].perfect, Is.False);
            Assert.That(early.health.hp, Is.EqualTo(50 + cfg.Num("punch.parryHeal")));
            Assert.That(early.awards, Does.Contain("PARRY"));
            Assert.That(early.awards, Does.Not.Contain("PERFECT PARRY"));
        }

        [Test]
        public void WithTheDesignFlagOff_EveryParryIsTheWebs()
        {
            var off = new Rig(cfg);
            off.loadout.perfectSteps = 0; // design.parry.perfect false
            off.PunchAt(3);
            Assert.That(off.parries[0].perfect, Is.False);
            Assert.That(off.health.hp, Is.EqualTo(50 + cfg.Num("punch.parryHeal")));
        }
    }
}
