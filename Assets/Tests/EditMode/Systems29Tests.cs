// NIGHT29 (builder S): the systems rows' pure rules: the fire / melee lockout (Core/Guns/Lockout29.cs), its lesson window (Core/Lesson25.cs), the
// tank's vulnerability row, the wallRiddenAndFired lesson and the DOORFREE hold (Core/DoorFree29.cs). No UnityEngine: the SimHarness rows
// (`lockout29`, `shotgun29`, ...) judge the same rules inside a run.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Systems29Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static LessonDef D(string json) => LessonDef.From(PortData.Obj(JsJson.Parse(json)));

        // --- the lockout -------------------------------------------------------------------------------------------------------------------

        [Test]
        public void Lockout_MeleeIsBlockedByAGunCycleAndByTheGauntlet()
        {
            Assert.That(Lockout29.MeleeBlocked(false, false), Is.False, "nothing running: the punch lands");
            Assert.That(Lockout29.MeleeBlocked(false, true), Is.True, "a gun's cycle is running: the press is ignored");
            Assert.That(Lockout29.MeleeBlocked(true, false), Is.True, "the gauntlet in hand: its own swing is the melee, no separate one");
        }

        [Test]
        public void Lockout_TheTriggerWaitsForTheSwing()
        {
            Assert.That(Lockout29.FireBlocked(true), Is.True);
            Assert.That(Lockout29.FireBlocked(false), Is.False);
        }

        [Test]
        public void Lockout_ASwitchAskedInsideACycleWaitsAndRunsWhenItEnds()
        {
            var l = new Lockout29();
            Assert.That(l.FilterSwitch(2, true), Is.EqualTo(-1), "asked inside the cycle: not run");
            Assert.That(l.FilterSwitch(-1, true), Is.EqualTo(-1), "still inside");
            Assert.That(l.queuedSwitch, Is.EqualTo(2));
            Assert.That(l.FilterSwitch(-1, false), Is.EqualTo(2), "the cycle ended: the queued switch runs");
            Assert.That(l.queuedSwitch, Is.EqualTo(-1));
            Assert.That(l.FilterSwitch(-1, false), Is.EqualTo(-1), "and only once");
        }

        [Test]
        public void Lockout_ALaterAskReplacesTheQueuedOneAndANewAskBeatsIt()
        {
            var l = new Lockout29();
            l.FilterSwitch(1, true);
            l.FilterSwitch(3, true);
            Assert.That(l.queuedSwitch, Is.EqualTo(3), "the latest ask wins");
            Assert.That(l.FilterSwitch(4, false), Is.EqualTo(4), "an ask made when free runs at once and drops the stale one");
            Assert.That(l.queuedSwitch, Is.EqualTo(-1));
        }

        [Test]
        public void Lockout_CountsWhatItDropsAndResets()
        {
            var l = new Lockout29();
            l.DropMelee();
            l.DropFire(true);
            l.DropFire(false);
            l.FilterSwitch(1, true);
            Assert.That(l.droppedMelee == 1 && l.droppedFireSteps == 1 && l.waitedSwitches == 1, Is.True);
            l.Reset();
            Assert.That(l.droppedMelee == 0 && l.droppedFireSteps == 0 && l.waitedSwitches == 0 && l.queuedSwitch == -1, Is.True);
        }

        // --- the lesson teaches the new order ----------------------------------------------------------------------------------------------

        [Test]
        public void PunchWhileFiring_WithTheLockoutAShotThenAPunchAfterTheCycleCounts()
        {
            var defs = new List<LessonDef> { D(@"{""kind"":""punchWhileFiring""}") };
            // off: 30 steps is the window; a punch 48 steps after the shot (the cycle) is not 'together'.
            var off = new Lesson25(defs, 30);
            LessonSample s = new LessonSample { step = 0 };
            off.Begin(s);
            s.fires = 1; s.step = 10; off.Step(s);
            s.punches = 1; s.step = 58;
            Assert.That(off.Step(s), Is.False, "NIGHT28's window is shorter than the cycle");
            // on: the window grows by the longest cycle; the punch the rule allows (after the 48-step cycle) counts.
            var on = new Lesson25(new List<LessonDef> { D(@"{""kind"":""punchWhileFiring""}") }, 30);
            LessonSample t = new LessonSample { step = 0, lockoutGap = 48 };
            on.Begin(t);
            Assert.That(on.lockoutOrder, Is.True);
            t.fires = 1; t.step = 10; on.Step(t);
            t.punches = 1; t.step = 58;
            Assert.That(on.Step(t), Is.True, "a shot, the wait, then the punch");
        }

        // --- the wallRiddenAndFired lesson (the tutorial's wall-ride room: shots fired FROM the wall in the slow-mo) --------------------------

        [Test]
        public void WallRiddenAndFired_NeedsAShotFiredWhileWallRiding()
        {
            Assert.That(LessonDef.ParseKind("wallRiddenAndFired"), Is.EqualTo(LessonKind.WallRiddenAndFired), "the plan's word is a kind, not 'none' (a none lesson clears at once)");
            Assert.That(LessonDef.KindName(LessonKind.WallRiddenAndFired), Is.EqualTo("wallRiddenAndFired"));
            var l = new Lesson25(new List<LessonDef> { D(@"{""kind"":""wallRiddenAndFired""}") }, 30);
            var s = new LessonSample { step = 0 };
            l.Begin(s);
            s.step = 1; s.wallRiding = true;
            Assert.That(l.Step(s), Is.False, "riding the wall without a shot is not the lesson");
            s.step = 2; s.wallRiding = false; s.fires = 1;
            Assert.That(l.Step(s), Is.False, "a shot from the floor is not the lesson");
            s.step = 3; s.wallRiding = true; s.fires = 2;
            Assert.That(l.Step(s), Is.True, "a shot fired while wall riding");
            Assert.That(l.Done, Is.True);
        }

        [Test]
        public void WallRiddenAndFired_CountsShotsFromTheLessonsStartAndNeedsNoEnemy()
        {
            var d = D(@"{""kind"":""wallRiddenAndFired"",""n"":2}");
            Assert.That(d.NeedsEnemy, Is.False, "the shot is the lesson, not a kill: the room needs no top-up");
            var l = new Lesson25(new List<LessonDef> { d }, 30);
            var s = new LessonSample { step = 0, fires = 5, wallRiding = true };
            l.Begin(s);
            s.step = 1; s.fires = 6;
            Assert.That(l.Step(s), Is.False, "one of two");
            s.step = 2; s.fires = 7;
            Assert.That(l.Step(s), Is.True, "two");
        }

        // --- the tank's vulnerability row and the unlock table -----------------------------------------------------------------------------

        [Test]
        public void TheTank_TakesTheGauntletTenfoldAndThePistolALittle()
        {
            Vulnerability26 v = Vulnerability26.From(Cfg);
            Assert.That(v != null, Is.True);
            Assert.That(v.Mult("tank", "gauntlet", false), Is.EqualTo(10.0), "the PULSE swing: one hit");
            Assert.That(v.Mult("tank", "revolver", false), Is.EqualTo(0.15), "the pistol: a sliver");
            Assert.That(v.Mult("tank", "rail", false), Is.EqualTo(0.4), "others by class");
            Assert.That(v.Mult("tank", "punch", false), Is.EqualTo(0.4), "the plain fist by class (single)");
            Assert.That(v.Mult("husk", "revolver", false), Is.EqualTo(1.0), "a husk is untouched");
            // one gauntlet swing (the heavy swing of the PULSE) kills a tank: weapons.gauntlet.damage x pulse27 damageMult x 10 >= tank hp
            double swing = Cfg.Num("weapons.gauntlet.damage") * Cfg.Num("design.n27.pulse27.damageMult") * v.Mult("tank", "gauntlet", false);
            Assert.That(swing >= Cfg.Num("design.n29.tank.hp"), Is.True, "one hit");
            double shot = Cfg.Num("weapons.revolver.damage") * v.Mult("tank", "revolver", false);
            Assert.That(Cfg.Num("design.n29.tank.hp") / shot > 100, Is.True, "the pistol needs well over a hundred shots");
        }

        [Test]
        public void TheUnlockTable_LevelsOneAndTwoAwardNothingAndThreeTheShotgun()
        {
            string path = Path.GetFullPath(Path.Combine(Port.Root, "..", "Design", "levels", "unlocks.json"));
            Unlocks26 u = Unlocks26.Parse(File.ReadAllText(path));
            Assert.That(u.start, Is.EqualTo(new List<string> { "revolver", "gauntlet" }), "the pistol and the PULSE gauntlet from the start");
            foreach (string id in new[] { "tutorial", "level2", "n29-foundry", "n29-iceline" })
                Assert.That(u.Earned(id), Is.Null, id + " awards nothing");
            Assert.That(u.Earned("n29-kiln"), Is.EqualTo("shotgun"));
            Assert.That(u.Earned("layer1"), Is.EqualTo("shotgun"));
            string[] after = { "nailgun", "torch", "zapper", "rail", "bouncer", "rocket" };
            string[] ids = { "level3", "level4", "level5", "level06", "level07", "level08" };
            for (int i = 0; i < ids.Length; i++) Assert.That(u.Earned(ids[i]), Is.EqualTo(after[i]), "level " + (4 + i));
            var all = new List<string> { "n29-foundry", "n29-iceline", "n29-kiln" };
            Assert.That(u.OwnedAfter(all), Is.EqualTo(new List<string> { "revolver", "gauntlet", "shotgun" }), "after three levels: the pistol, the glove and the THUNDERCLAP");
        }

        // --- DOORFREE: the hold detector -----------------------------------------------------------------------------------------------------

        const double Dt = 1.0 / 120;

        static DoorFree29 Hold() => new DoorFree29(3.0, 0.4);

        [Test]
        public void DoorFree_ToggleComesAtExactlyThreeSecondsOfUnbrokenHold()
        {
            var d = Hold();
            int toggledAt = -1;
            for (int i = 1; i <= 400; i++) if (d.Update(true, Dt) && toggledAt < 0) toggledAt = i;
            Assert.That(toggledAt, Is.EqualTo(360), "3.0 s at 120 Hz");
            Assert.That(d.On, Is.True);
            Assert.That(d.Used, Is.True);
        }

        [Test]
        public void DoorFree_AShorterHoldOrABrokenOneDoesNothing()
        {
            var d = Hold();
            for (int i = 0; i < 359; i++) d.Update(true, Dt);
            Assert.That(d.On, Is.False, "one step short");
            d.Update(false, Dt); // let go
            for (int i = 0; i < 359; i++) d.Update(true, Dt); // the count starts over
            Assert.That(d.On, Is.False, "a release resets the hold");
            Assert.That(d.Update(true, Dt), Is.True, "and the full hold after it flips it");
        }

        [Test]
        public void DoorFree_AHeldButtonFlipsOnceAndTheNextHoldFlipsItBack()
        {
            var d = Hold();
            int flips = 0;
            for (int i = 0; i < 1000; i++) if (d.Update(true, Dt)) flips++;
            Assert.That(flips, Is.EqualTo(1), "holding on past 3 s is one toggle");
            d.Update(false, Dt);
            bool again = false;
            for (int i = 0; i < 360; i++) again |= d.Update(true, Dt);
            Assert.That(again, Is.True);
            Assert.That(d.On, Is.False, "the second hold turns it off");
            Assert.That(d.Used, Is.True, "but the level stays unranked");
            d.Reset();
            Assert.That(d.On || d.Used, Is.False, "a new level starts clean");
        }

        [Test]
        public void DoorFree_ANormalPressShowsNothingInsideTheFirstFourTenths()
        {
            var d = Hold();
            for (int i = 0; i < 47; i++) d.Update(true, Dt); // 0.4 s is 48 steps
            Assert.That(d.Revealing, Is.False, "a tap or a punch: no ring");
            Assert.That(d.Progress, Is.EqualTo(0.0));
            d.Update(true, Dt);
            Assert.That(d.Revealing, Is.True, "from 0.4 s the ring starts");
            Assert.That(d.Progress > 0 && d.Progress < 1, Is.True);
            d.Update(false, Dt);
            Assert.That(d.Revealing, Is.False);
        }

        [Test]
        public void TheDoorFreeRowCarriesItsNumbers()
        {
            Assert.That(Cfg.Num("design.n29.doorFree.holdSeconds"), Is.EqualTo(3.0), "GG: three seconds");
            Assert.That(Cfg.Num("design.n29.doorFree.revealSeconds"), Is.EqualTo(0.4), "a normal press is unchanged inside the first 0.4 s");
            var d = DoorFree29.From(Cfg);
            for (int i = 0; i < 360; i++) d.Update(true, Dt);
            Assert.That(d.On, Is.True);
        }

        // --- the config rows ---------------------------------------------------------------------------------------------------------------

        [Test]
        public void TheLockoutRowCarriesItsPolicyAndTheLessonPrompt()
        {
            Assert.That(Cfg.Has("design.n29.lockout.lessonPrompt"), Is.True);
            Assert.That(Cfg.Str("design.n29.lockout.lessonPrompt").Split(' ').Length <= 6, Is.True, "CURRICULUM rule 5: at most six words");
            Assert.That(Cfg.Num("design.n29.lockout.bufferSeconds"), Is.EqualTo(0.0), "D-1: the second input is dropped");
        }
    }
}
