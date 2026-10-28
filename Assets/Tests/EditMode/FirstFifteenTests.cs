// NIGHT20 E1 (builder P; Research/2026-10-06-additions-backlog.md E1, QUEUE-first-fifteen U3-28.6): the first fifteen minutes
// v2 (design.adds.firstFifteen; Hodent: teach a verb at its moment of need, gone on use). Core/FirstFifteen.cs is the rule:
// - a moment's need shows its one line (its id from design.adds.firstFifteen.moments) unless the profile learned it;
// - one at a time: a need while another shows waits, and shows when that one goes;
// - its first use learns it for the profile (a flag "learn.<id>": never again), and the next waiting one shows;
// - a moment whose need passes without a use (the break ended, the draft closed) goes unlearned, as does one shown
//   showSeconds without a use: the next need shows it again;
// - its show clock runs only while the view can show it (a lesson card on screen holds it);
// - a new run drops what shows and waits; what was learned stays. Disabled: nothing ever shows.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class FirstFifteenTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static FirstFifteen Make(Profile p = null) => new FirstFifteen(Cfg, p ?? new Profile(), true);

        [Test]
        public void TheBlock_IsInTheDesignConfig_WithItsMoments_EachLineAtMostThreeWords()
        {
            Assert.That(Cfg.Has("design.adds.firstFifteen._why"), Is.True);
            Assert.That(Cfg.Design("adds.firstFifteen.enabled"), Is.True, "the row's own switch (the adds bit is the lead's)");
            FirstFifteen f = Make();
            string[] want = { "parry", "melee", "box", "hold", "mods", "exfil" };
            foreach (string id in want) Assert.That(f.Has(id), Is.True, id);
            foreach (FirstFifteen.Moment m in f.moments)
            {
                string words = System.Text.RegularExpressions.Regex.Replace(m.line, "\\{[a-zA-Z]+\\}", "").Trim();
                Assert.That(words.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length, Is.LessThanOrEqualTo(3), $"{m.id}: '{m.line}' (the HUD's 3 words, NIGHT17)");
            }
            Assert.That(f.showSeconds, Is.GreaterThan(0));
        }

        [Test]
        public void ANeed_ShowsItsLine_TheFirstUse_LearnsItForTheProfile()
        {
            var p = new Profile();
            FirstFifteen f = Make(p);
            Assert.That(f.Active, Is.Null);
            Assert.That(f.Need("parry"), Is.True);
            Assert.That(f.Active?.id, Is.EqualTo("parry"));
            Assert.That(f.Used("parry"), Is.True, "newly learned: the caller saves the profile");
            Assert.That(f.Active, Is.Null, "gone on use");
            Assert.That(p.Flag(FirstFifteen.FlagPrefix + "parry"), Is.True, "the profile remembers it");
            Assert.That(f.Need("parry"), Is.False, "learned: never again");
            Assert.That(Make(p).Need("parry"), Is.False, "nor in a later session");
            Assert.That(f.Used("parry"), Is.False, "already learned");
        }

        [Test]
        public void OneAtATime_ANeedWaits_AndShowsWhenTheFirstGoes()
        {
            FirstFifteen f = Make();
            f.Need("hold");
            Assert.That(f.Need("parry"), Is.True, "waiting");
            Assert.That(f.Active.id, Is.EqualTo("hold"), "one at a time");
            int v = f.Version;
            f.Used("hold");
            Assert.That(f.Active?.id, Is.EqualTo("parry"), "the waiting one shows");
            Assert.That(f.Version, Is.Not.EqualTo(v), "the view rebuilds on a change");
            f.Need("parry");
            Assert.That(f.Active.id, Is.EqualTo("parry"), "a repeated need changes nothing");
        }

        [Test]
        public void UsingAWaitingMoment_LearnsIt_AndItNeverShows()
        {
            FirstFifteen f = Make();
            f.Need("hold");
            f.Need("parry");
            Assert.That(f.Used("parry"), Is.True);
            f.Used("hold");
            Assert.That(f.Active, Is.Null, "parry was learned while it waited");
        }

        [Test]
        public void ANeedThatPasses_GoesUnlearned_AndShowsAgainOnTheNextNeed()
        {
            var p = new Profile();
            FirstFifteen f = Make(p);
            f.Need("exfil");
            f.Gone("exfil");
            Assert.That(f.Active, Is.Null);
            Assert.That(p.Flag(FirstFifteen.FlagPrefix + "exfil"), Is.False);
            Assert.That(f.Need("exfil"), Is.True, "the next radio shows it again");
            f.Need("mods");
            f.Gone("mods"); // a waiting one whose moment passed: dropped
            f.Used("exfil");
            Assert.That(f.Active, Is.Null, "mods' moment had passed");
        }

        [Test]
        public void ShownShowSecondsWithoutAUse_ItGoes_TheClockRunsOnlyWhileItCanShow()
        {
            FirstFifteen f = Make();
            f.Need("box");
            f.Need("melee");
            double dt = 0.5;
            int n = (int)System.Math.Ceiling(f.showSeconds / dt) + 4;
            for (int i = 0; i < n; i++) f.Tick(dt, false);
            Assert.That(f.Active?.id, Is.EqualTo("box"), "held while the view can't show it (a lesson card)");
            for (int i = 0; i < n; i++) f.Tick(dt, true);
            Assert.That(f.Active?.id, Is.EqualTo("melee"), "box timed out unlearned; the waiting one shows");
            Assert.That(f.Need("box"), Is.True, "and box can come back");
        }

        [Test]
        public void ANewRun_DropsWhatShowsAndWaits_KeepsWhatWasLearned()
        {
            var p = new Profile();
            FirstFifteen f = Make(p);
            f.Need("melee");
            f.Used("melee");
            f.Need("hold");
            f.Need("box");
            f.Reset();
            Assert.That(f.Active, Is.Null);
            f.Need("mods");
            Assert.That(f.Active.id, Is.EqualTo("mods"), "nothing waited from the last run");
            Assert.That(f.Need("melee"), Is.False, "learned stays");
        }

        [Test]
        public void Disabled_NothingShows_AnUnknownIdIsIgnored()
        {
            var off = new FirstFifteen(Cfg, new Profile(), false);
            Assert.That(off.Need("parry"), Is.False);
            Assert.That(off.Active, Is.Null);
            FirstFifteen f = Make();
            Assert.That(f.Need("nope"), Is.False);
            Assert.That(f.Used("nope"), Is.False);
            Assert.That(f.Active, Is.Null);
        }

        // NIGHT20 E1 (builder P2): FirstFifteenVerbsTests (a new profile's rounds 1-7) found the moments' lines held for whole
        // rounds: behind the first run's lesson cards (round 6's power lesson held the card through the HOLD round) and behind
        // a standing moment (the box's line kept the draft's MODS line waiting until the pick).

        [Test]
        public void AnUrgentMoment_TakesTheLineFromAStandingOne_WhichWaitsAndComesBack()
        {
            FirstFifteen f = Make();
            foreach (string id in new[] { "parry", "hold", "mods", "exfil" }) Assert.That(f.Urgent(id), Is.True, id + ": its window is short");
            foreach (string id in new[] { "melee", "box" }) Assert.That(f.Urgent(id), Is.False, id + ": its moment stands");
            f.Need("box");
            Assert.That(f.Need("mods"), Is.True);
            Assert.That(f.Active?.id, Is.EqualTo("mods"), "the draft's line at once");
            f.Used("mods");
            Assert.That(f.Active?.id, Is.EqualTo("box"), "the box's line back");
            f.Need("hold");
            Assert.That(f.Need("parry"), Is.True);
            Assert.That(f.Active?.id, Is.EqualTo("hold"), "an urgent one never takes the line from another urgent one");
        }

        [Test]
        public void AnUrgentLine_ReadReadSeconds_GivesTheLineToTheNextUrgentOne()
        {
            FirstFifteen f = Make();
            Assert.That(f.readSeconds, Is.GreaterThan(0).And.LessThan(f.showSeconds), "design.adds.firstFifteen.readSeconds");
            f.Need("hold");
            f.Tick(f.readSeconds * 0.5, false);
            f.Need("parry");
            Assert.That(f.Active?.id, Is.EqualTo("hold"), "not read yet (its clock runs only while it can show)");
            f.Tick(f.readSeconds, true);
            Assert.That(f.Active?.id, Is.EqualTo("parry"), "read: the urgent one waiting takes the line");
            f.Used("parry");
            Assert.That(f.Active?.id, Is.EqualTo("hold"), "the ring's line back while its round runs");
            f.Need("mods");
            Assert.That(f.Active?.id, Is.EqualTo("mods"), "the ring's line came back already read: a new urgent need takes it at once");
            f.Used("mods");
            f.Tick(f.showSeconds - f.readSeconds, true);
            Assert.That(f.Active, Is.Null, "the ring's line goes after showSeconds in all, unlearned");
        }

        [Test]
        public void ALessonCard_YieldsToAMoment_AfterLessonYieldSeconds()
        {
            FirstFifteen f = Make();
            Assert.That(f.lessonYield, Is.GreaterThan(0).And.LessThan(10), "design.adds.firstFifteen.lessonYield (s)");
            Assert.That(f.MayShow(false, 0), Is.True, "no lesson: the card is the moment's");
            Assert.That(f.MayShow(true, f.lessonYield * 0.5), Is.False, "a fresh lesson keeps the card");
            Assert.That(f.MayShow(true, f.lessonYield), Is.True, "a lesson up lessonYield s yields it");
        }
    }
}
