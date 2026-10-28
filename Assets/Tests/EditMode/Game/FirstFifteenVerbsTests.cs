// NIGHT20 E1 (builder P2; Research/2026-10-06-additions-backlog.md E1's done-when: "each verb prompted at its moment of need
// and gone on first use"): the first fifteen minutes v2 (Game/FirstFifteenWatch.cs, Core/FirstFifteen.cs) over a NEW
// PROFILE's first run, rounds 1-7, played by the AUTOPILOT (a recording run: metaSilent cleared, as FirstUnlockTests). The
// moments of need come from the run itself (the first spitter volley, the first enemy in punching reach, the bank at the
// box's price, the HOLD ring, the first MODS draft; the exfil is offered from round 11, outside this window). What the
// player sees is modelled as Ui's onboarding draws it (UiOnboarding.RenderFirstFifteen): the line shows only while the
// first run's lesson card (design.onboarding.rounds: rounds 1, 2, 3, 5, 6) doesn't hold the card: from its break until
// its thing is done (UnityTuning.LessonDoneOn: a kill, a dash or slide, a gun bought, a perk, the power on) or the next
// break, plus UnityTuning.CardFade for the card to fade; a lesson up FirstFifteen.lessonYield s yields the card to a
// moment (FirstFifteen.MayShow, the rule UiOnboarding draws by).
// The uses are a player's, 1 s (React) after the line shows: a real punch (the input pressed; the game's own "punch"), the
// draft's first card (the draft opened at its break and picked then; a human reads it), the ring stood in (the bot walks
// there: the game's own Objectives.Inside); the parry and the box roll are given as the game reports them ("parry",
// "boxOpen": the bot neither parries a glob nor walks to the box).
// Asserted per verb whose moment came: its line showed within 10 s of the first need (or the verb was used before its
// line was due: nothing left to teach); on its first use the line went that step and the verb is learned for the profile;
// it never showed again that run.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class FirstFifteenVerbsTests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig cfg;
        static SurvivalMap map;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));
        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(PortRoot(), "maps", "survival.json"))));

        const double React = 1.0, MaxLatency = 10.0;
        static readonly string[] Verbs = { FirstFifteenWatch.Parry, FirstFifteenWatch.Melee, FirstFifteenWatch.Box, FirstFifteenWatch.Hold, FirstFifteenWatch.Mods };

        /// <summary>The AUTOPILOT's input with a player's punch pressed on demand (the test's real punch).</summary>
        sealed class PunchingInput : IPlayerInput
        {
            readonly IPlayerInput inner;
            public int punchSteps; // > 0: hold punch this many steps (pressed on the first)
            bool first;

            public PunchingInput(IPlayerInput inner) => this.inner = inner;
            public double Yaw => inner.Yaw;
            public double Pitch => inner.Pitch;
            public void SetLook(double yaw, double pitch) => inner.SetLook(yaw, pitch);
            public bool Captured => inner.Captured;
            public InputFrame SampleMove(bool active) => inner.SampleMove(active);

            public CombatInput SampleCombat(bool active)
            {
                CombatInput c = inner.SampleCombat(active);
                if (punchSteps > 0 && active)
                {
                    c.punch = new Button { held = true, pressed = !first };
                    first = true;
                    if (--punchSteps == 0) first = false;
                }
                return c;
            }

            public void Punch()
            {
                punchSteps = 3;
                first = false;
            }
        }

        /// <summary>One verb's timeline in the run (sim seconds; NaN: never).</summary>
        sealed class Verb
        {
            public string id;
            public double need = double.NaN, shown = double.NaN, used = double.NaN;
            public string atNeed; // the line the rule held when the need came (the report's)
            public bool clearedAtUse, learnedAtUse, shownAfterUse;
        }

        sealed class Run
        {
            public uint seed;
            public int round;
            public readonly Dictionary<string, Verb> verbs = new Dictionary<string, Verb>();
            public string Line()
            {
                var parts = new List<string>();
                foreach (string id in Verbs)
                {
                    Verb v = verbs[id];
                    parts.Add(double.IsNaN(v.need) ? $"{id} -" : $"{id} need {v.need:0.0} (over {v.atNeed}) shown {(double.IsNaN(v.shown) ? "-" : (v.shown - v.need).ToString("+0.0;-0.0"))} used {(double.IsNaN(v.used) ? "-" : (v.used - v.need).ToString("+0.0;-0.0"))}");
                }
                return $"seed {seed} (to round {round}): " + string.Join("; ", parts);
            }
        }

        static Run Play(uint seed)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.TellTheTruth();
            var input = new PunchingInput(ap);
            sim.UseInput(input);
            sim.metaSilent = false; // a new player's recording run (the watch is never on in a bot's run)
            sim.n24Override = N24Blocks.Off; // NIGHT24 premise: the first-fifteen "mods" verb is NIGHT21's PICK A MOD draft (round 2, opens in a break); the BIG PICK (round 5 and every 5) has no first-use prompt of its own yet, so this contract is judged with the blocks off
            var watch = new FirstFifteenWatch(sim, sim.profile, null) { Force = true };
            FirstFifteen rule = watch.rule;
            var run = new Run { seed = seed };
            foreach (string id in Verbs) run.verbs[id] = new Verb { id = id };
            double t = 0;
            double punchRange = Cfg().Num("punch.range");
            double boxPrice = sim.box != null ? sim.box.price : double.PositiveInfinity;
            // The moments of need, from the run (the watch's own triggers, observed independently).
            void Need(string id)
            {
                Verb v = run.verbs[id];
                if (!double.IsNaN(v.need)) return;
                v.need = t;
                v.atNeed = rule.Active?.id ?? "-";
            }
            sim.events.On("spitterVolley", _ => Need(FirstFifteenWatch.Parry));
            sim.events.On<ObjectiveStartEvent>("objectiveStart", e =>
            {
                if (e.type == Objectives.Hold) Need(FirstFifteenWatch.Hold);
            });
            sim.events.On<ModEvent>("modDraft", _ => Need(FirstFifteenWatch.Mods));
            // The lesson card (UiOnboarding: shown at its round's break, done by its thing, faded over CardFade).
            Onboarding.Lesson lessonNow = null;
            string lessonDoneOn = null;
            double cardFreeAt = 0, lessonFor = 0;
            string shownId = null; // the moment's line on the card (null: none)
            void LessonEnd()
            {
                if (lessonNow == null) return;
                lessonNow = null;
                lessonDoneOn = null;
                if (shownId == null) cardFreeAt = t + UnityTuning.CardFade; // a moment's line on the card stays (UiOnboarding)
            }
            void LessonDone(string kind)
            {
                if (lessonNow != null && lessonDoneOn == kind) LessonEnd();
            }
            sim.events.On<RoundEvent>("roundBreak", e =>
            {
                LessonEnd();
                if (sim.onboarding == null || !sim.onboarding.Active) return;
                Onboarding.Lesson l = sim.onboarding.For(e.number);
                if (l == null) return;
                lessonNow = l;
                lessonFor = 0;
                UnityTuning.LessonDoneOn.TryGetValue(l.id ?? string.Empty, out lessonDoneOn);
            });
            sim.events.On<KillEvent>("kill", e =>
            {
                if (e.owner == "player") LessonDone(UnityTuning.DoneOnKill);
            });
            sim.events.On("weaponBought", _ => LessonDone(UnityTuning.DoneOnGun));
            sim.events.On("perk", _ => LessonDone(UnityTuning.DoneOnPerk));
            sim.events.On("powerOn", _ => LessonDone(UnityTuning.DoneOnPower));
            // The uses (each verb's first: the step it happened, the line and the flag right after).
            string usedNow = null;
            sim.events.On<MeleeEvent>("punch", e =>
            {
                if (e.owner == "player") usedNow ??= FirstFifteenWatch.Melee;
            });
            sim.events.On<ModEvent>("modPick", e =>
            {
                if (!e.free) usedNow ??= FirstFifteenWatch.Mods;
            });
            sim.events.On<HitEvent>("parry", e => // the bot's own punch can reflect a drone's orb before any spitter volley
            {
                if (e.owner == "player") usedNow ??= FirstFifteenWatch.Parry;
            });
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            Assert.That(sim.Recording && watch.On, Is.True, "a new profile's recording run, the moments on");
            Assert.That(sim.onboarding.Active, Is.True, "the first run's lessons on (a new profile)");
            double shownSince = 0, draftSince = double.NaN;
            int shownSteps = 0, reactSteps = MovementMath.StepsFor(React, sim.step); // whole steps (a float window can miss)
            int maxSteps = MovementMath.StepsFor(40 * 60, sim.step);
            for (int i = 0; i < maxSteps && sim.flow == Flow.Playing && sim.run.round <= 7; i++)
            {
                usedNow = null; // this step's first use (the pick below, then the step's events)
                // the draft: opened at its break (modDraft: the need), picked a human's moment later
                if (sim.run.state == SurvivalRun.State.Break && (sim.mods.Pending || sim.mods.offer != null)) // (Pending is false once the draft is open)
                {
                    if (sim.mods.offer == null)
                    {
                        sim.mods.Open();
                        draftSince = t;
                    }
                    else if ((shownId == FirstFifteenWatch.Mods && shownSteps >= reactSteps) || t - draftSince >= MaxLatency) sim.mods.Pick(0);
                }
                sim.Step();
                t += sim.step;
                if (lessonNow != null && lessonDoneOn == UnityTuning.DoneOnDashSlide)
                {
                    MoveState ms = sim.mover.state;
                    if (ms == MoveState.Dash || ms == MoveState.Slide || ms == MoveState.Slam) LessonDone(UnityTuning.DoneOnDashSlide);
                }
                // state moments: the bank at the box's price, an enemy in punching reach, the ring stood in
                if (sim.economy.total >= boxPrice) Need(FirstFifteenWatch.Box);
                Vec3 p = sim.mover.position;
                foreach (Enemy e in sim.enemies.all)
                {
                    if (e.alive && JsMath.Hypot(e.position.x - p.x, e.position.z - p.z) <= punchRange)
                    {
                        Need(FirstFifteenWatch.Melee);
                        break;
                    }
                }
                if (sim.objectives.Holding && sim.objectives.Inside) usedNow ??= FirstFifteenWatch.Hold;
                watch.Step();
                if (lessonNow != null) lessonFor += sim.step;
                bool can = lessonNow != null ? rule.MayShow(true, lessonFor) : t >= cardFreeAt;
                rule.Tick(sim.step, can);
                string active = can ? rule.Active?.id : null;
                shownSteps++;
                if (active != shownId)
                {
                    shownId = active;
                    shownSince = t;
                    shownSteps = 0;
                    if (active != null && run.verbs.TryGetValue(active, out Verb sv))
                    {
                        if (double.IsNaN(sv.shown)) sv.shown = t;
                        if (!double.IsNaN(sv.used)) sv.shownAfterUse = true;
                    }
                }
                if (usedNow != null && run.verbs.TryGetValue(usedNow, out Verb u) && double.IsNaN(u.used))
                {
                    u.used = t;
                    u.clearedAtUse = rule.Active?.id != usedNow;
                    u.learnedAtUse = rule.Learned(usedNow);
                }
                // a player's use, React s after the line shows
                if (shownId != null && shownSteps == reactSteps)
                {
                    if (shownId == FirstFifteenWatch.Melee) input.Punch();
                    else if (shownId == FirstFifteenWatch.Parry || shownId == FirstFifteenWatch.Box)
                    {
                        string id = shownId;
                        if (id == FirstFifteenWatch.Parry) sim.events.Emit("parry", new HitEvent { owner = "player", weapon = "punch", kind = "spitter" });
                        else sim.events.Emit("boxOpen", new BoxOpenEvent { owner = "player", price = boxPrice });
                        Verb v = run.verbs[id];
                        if (double.IsNaN(v.used))
                        {
                            v.used = t;
                            v.clearedAtUse = rule.Active?.id != id;
                            v.learnedAtUse = rule.Learned(id);
                        }
                    }
                }
            }
            run.round = sim.run.round;
            return run;
        }

        [Test]
        public void ANewProfile_RoundsOneToSeven_EachVerbPromptedWithin10s_GoneOnFirstUse()
        {
            var lines = new List<string>();
            var fails = new List<string>();
            var seen = new HashSet<string>();
            foreach (uint seed in new uint[] { 1, 4, 5, 7, 8 })
            {
                Run r = Play(seed);
                lines.Add(r.Line());
                foreach (string id in Verbs)
                {
                    Verb v = r.verbs[id];
                    if (double.IsNaN(v.need)) continue;
                    seen.Add(id);
                    double due = double.IsNaN(v.shown) ? double.PositiveInfinity : v.shown - v.need;
                    double usedFirst = double.IsNaN(v.used) ? double.PositiveInfinity : v.used - v.need;
                    if (!(due <= MaxLatency) && !(usedFirst <= MaxLatency && (double.IsNaN(v.shown) || v.used <= v.shown)))
                        fails.Add($"seed {seed} {id}: shown {(double.IsPositiveInfinity(due) ? "never" : $"{due:0.0} s")} after its need at {v.need:0.0} s");
                    if (!double.IsNaN(v.used) && (!v.clearedAtUse || !v.learnedAtUse)) fails.Add($"seed {seed} {id}: used at {v.used:0.0} s but cleared {v.clearedAtUse}, learned {v.learnedAtUse}");
                    if (v.shownAfterUse) fails.Add($"seed {seed} {id}: shown again after its use");
                }
            }
            string report = string.Join("\n", lines);
            Assert.That(fails, Is.Empty, report);
            foreach (string id in Verbs) Assert.That(seen.Contains(id), Is.True, $"{id}'s moment came in rounds 1-7 of some seed\n{report}");
            Assert.Pass(report);
        }
    }
}
