// NIGHT12 U3-25.11: tips.js (a tip shows once a run, for its first `times` runs; the {action} keys filled; automation
// shows but doesn't count) and the first-run script (design.onboarding): the round table (rounds 1, 2, 3, 5, 6 have
// lessons, 4 doesn't), its placeholders are real bindings, Active / Finish in the profile, the order of tips.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public sealed class TipsTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static string Key(string action) => action == "interact" ? "E" : action.ToUpperInvariant();

        [Test]
        public void ATip_ShowsOnceARun_ForItsFirstTimesRuns()
        {
            var profile = new Profile();
            var tips = new Tips(cfg, profile, Key) { recording = true };
            int times = cfg.Int("tips.times");
            for (int run = 0; run < times + 2; run++)
            {
                tips.NewRun();
                string text = tips.Moment("break");
                if (run < times) Assert.That(text, Is.Not.Null, "run " + run);
                else Assert.That(text, Is.Null, "run " + run);
                Assert.That(tips.Moment("break"), Is.Null, "once a run");
            }
            Assert.That(profile.TipCount("break"), Is.EqualTo(times));
            Assert.That(profile.TipCount("wallBuy"), Is.EqualTo(0));
            Assert.That(tips.Moment("nope"), Is.Null);
        }

        [Test]
        public void TheKeysAreFilledIn_AndATipShownEmitsTip()
        {
            var events = new EventBus();
            var shown = new List<string>();
            events.On<TipEvent>(MetaEvents.Tip, e => shown.Add(e.id + ": " + e.text));
            var tips = new Tips(cfg, new Profile(), Key, events) { recording = true };
            string expect = cfg.Str("tips.list.wallBuy").Replace("{interact}", "E");
            Assert.That(tips.Moment("wallBuy"), Is.EqualTo(expect));
            Assert.That(shown, Is.EqualTo(new[] { "wallBuy: " + expect }));
            Assert.That(Tips.Fill("X {interact} {nope!} {} {{jump}", Key), Is.EqualTo("X E {nope!} {} {JUMP"));
            Assert.That(Tips.Fill("{fire}", _ => null), Is.EqualTo("null"), "JS's replace prints a null label as null");
            Assert.That(tips.Ids.Count, Is.EqualTo(cfg.Obj("tips.list").Count));
        }

        [Test]
        public void AnAutomationRun_ShowsTips_ButDoesntCountThem()
        {
            var profile = new Profile();
            var tips = new Tips(cfg, profile, Key) { recording = false };
            for (int run = 0; run < cfg.Int("tips.times") + 2; run++)
            {
                tips.NewRun();
                Assert.That(tips.Moment("perk"), Is.Not.Null);
            }
            Assert.That(profile.tips, Is.Empty);
        }

        [Test]
        public void TheFirstRunScript_HasOneLessonForRounds12356()
        {
            var ob = new Onboarding(cfg, new Profile());
            List<Onboarding.Lesson> lessons = ob.Lessons();
            var rounds = new List<int>();
            foreach (Onboarding.Lesson l in lessons) rounds.Add(l.round);
            Assert.That(rounds, Is.EqualTo(new[] { 1, 2, 3, 5, 6 }));
            Assert.That(ob.For(4), Is.Null);
            Assert.That(ob.For(7), Is.Null);
            Assert.That(ob.For(3).guide, Is.EqualTo("wallBuy"));
            Assert.That(ob.For(5).guide, Is.EqualTo("perk"));
            Assert.That(ob.For(6).guide == "power" && ob.For(6).guideThen == "relay", Is.True);
            var guides = new HashSet<string> { "wallBuy", "perk", "power", "relay", "gate" };
            var ids = new HashSet<string>();
            Dictionary<string, object> bindings = cfg.Obj("input.bindings");
            foreach (Onboarding.Lesson l in lessons)
            {
                Assert.That(ids.Add(l.id), Is.True, "unique id " + l.id);
                Assert.That(!string.IsNullOrEmpty(l.title) && !string.IsNullOrEmpty(l.text), Is.True, l.id);
                Assert.That(l.guide == null || guides.Contains(l.guide), Is.True, l.id);
                Assert.That(l.guideThen == null || guides.Contains(l.guideThen), Is.True, l.id);
                foreach (string tip in l.tips) Assert.That(cfg.Has("tips.list." + tip), Is.True, l.id + " teaches tip " + tip);
                // Every {action} is a bound action, so the keyboard's label is never "null".
                string filled = Tips.Fill(l.text, a => bindings.ContainsKey(a) ? "KEY" : null);
                Assert.That(filled, Does.Not.Contain("null"), l.id);
                Assert.That(filled, Does.Not.Contain("{"), l.id);
            }
        }

        [Test]
        public void TheScript_RunsOnTheFirstRunOnly_AndHoldsBackTheTipsItTeaches()
        {
            var profile = new Profile();
            var events = new EventBus();
            var steps = new List<string>();
            events.On<OnboardingEvent>(MetaEvents.OnboardingStep, e => steps.Add(e.round + " " + e.id));
            var ob = new Onboarding(cfg, profile, events);
            Assert.That(ob.Active, Is.False, "before a run");
            ob.StartRun(false);
            Assert.That(ob.Active, Is.False, "the SETTINGS toggle off");
            Assert.That(ob.RoundStart(1), Is.Null);
            Assert.That(ob.TipAllowed("wallBuy", 1), Is.True, "outside the script every tip may show");

            ob.StartRun(true);
            Assert.That(ob.Active, Is.True);
            Assert.That(ob.RoundStart(1).id, Is.EqualTo(ob.For(1).id));
            Assert.That(ob.RoundStart(1), Is.Null, "once a run");
            Assert.That(ob.RoundStart(4), Is.Null);
            Assert.That(ob.TipAllowed("wallBuy", 1), Is.False);
            Assert.That(ob.TipAllowed("wallBuy", 3), Is.True);
            Assert.That(ob.TipAllowed("relay", 5), Is.False);
            Assert.That(ob.TipAllowed("break", 1), Is.True, "a tip no lesson teaches");
            Assert.That(steps, Is.EqualTo(new[] { "1 " + ob.For(1).id }));

            ob.Finish(false);
            Assert.That(profile.onboarded, Is.False, "not recording: nothing written");
            ob.StartRun(true);
            Assert.That(ob.Active, Is.True);
            ob.Finish(true);
            Assert.That(profile.onboarded, Is.True);
            Assert.That(ob.Active, Is.False);
            ob.StartRun(true);
            Assert.That(ob.Active, Is.False, "onboarded: the script doesn't run again");
        }
    }
}
