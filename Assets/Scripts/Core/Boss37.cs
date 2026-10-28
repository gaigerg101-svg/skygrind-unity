// NIGHT37 builder K (boss37; NIGHT37.md card K1, Research/2026-10-12-wave37/BOSSES.md "Shared rules" 1-8, DECIDE37 D-5 / D-6): the shared
// BRAIN every one of tonight's four bosses (THE WICK, THE STAG, THE LOOM, THE CROWN) runs on. An evolution of Core/Boss36.cs (copied
// shape, not edited: Boss36 stays as it is). Pure (no scene, no UnityEngine), fixed 120 Hz, every timer in whole steps, every number in
// design.n37.boss37 (a boss's row may override any of them under design.n37.<row>.kit), randomness from the boss's own Mulberry32.
//
// THE STATES (one at a time, never two tells at once: rule 7):
//   REVEAL     the walk-in (walkin36): no damage, no attacks, revealSteps long.
//   REST       the breather between bursts (restSeconds), or the forced rest at the budget cap (budgetRestSeconds).
//   TELL       one attack's wind-up (its own windupSteps): raised through Tells36 (YELLOW = parry it, RED = dodge it).
//   ACTIVE     the attack runs (its boss decides what that means: read `entered` to fire it once).
//   RECOVER    the attack's tail (recoverSteps); then the next attack of the burst, or a rest.
//   WINDOW     the CYAN window: the stagger meter emptied, the boss's damage rule applies full damage for windowSeconds (D-5).
//   LEGCHANGE  between legs: the boss changes the arena (legChangeSeconds), no attacks, no damage.
//   DYING      the finisher; then DEAD.
//
// THE RULES (each printed in isolation by the harness `boss37 kit`):
//   STAGGER  (Minos Prime's stamina, uk-bosses.md: 6 in phase 1, 12 in phase 2): a boss's "right move" calls Stagger(amount); at 0 the
//            CYAN window opens; when it closes the meter refills to the leg's full value (stagger[leg-1]).
//   BUDGET   (MinosPrime.cs:342-353): each attack adds its cost (1-3); when the total reaches the leg's cap it resets to 0 and the boss rests
//            budgetRestSeconds (2 s on Standard). A parry refunds parryRefund points (MinosPrime.cs:1125; clamped at 0 here, a delta).
//   BURST    (Gabriel.cs:132-142): `burst` attacks chain (2 on Standard), then a rest.
//   PICK     (Gabriel.cs:74-128): every allowed attack rolls rng 0..1 x weight x habit + its pity; the highest wins; the picked one's pity
//            resets to 0, every other attack's pity rises by pityStep (+0.25); never the previous attack (unless it is the only one
//            allowed). Allowed = its legs, its distance band [minDist, maxDist], and the camp tag only when the camp clock is due.
//   HABIT    (Minotaur.cs:294-296): airBias drifts toward 1 while the player is airborne or riding a wall and toward 0 while grounded,
//            across habitSeconds (Minotaur: 20 s; ours ~8 s, BOSSES.md rule 5). Ground-tagged attacks (they deny the ground) weigh
//            1 + habitGain x (1 - airBias); air-tagged attacks 1 + habitGain x airBias. A habit attack snaps the bias to the other extreme
//            (Minotaur.cs:399-410), so the boss doesn't deny the same space twice running.
//   CAMP     (Boss36's camp clock): a grounded player within campRadius of one spot for campSeconds makes the next pick a camp-tagged
//            attack (if the boss has one allowed). Riding a wall or leaving the circle restarts the clock.
//   HIDE     (GabrielBase.cs:355-363): out of sight, farther than hideReach, or more than hideBelow under the boss: a timer fills 1/s and
//            drains 2/s; at hideSeconds the "boss37Hide" event and RepositionDue (the boss moves onto the player; ConsumeReposition()).
//   LEGS     (rule 1, rule 8, D-6): three legs; with autoLegs the leg changes when hpFrac falls to legAt[leg-1]; a boss whose legs are places
//            calls NextLeg(). RestartLeg() (a fall / a death): the leg's checkpoint, the boss's HP and stagger meter kept, the budget,
//            burst, clocks and tells cleared, a short rest; the health cost is the boss's sim's (it owns the player).
//
// EVENTS (names for GG's sounds; payload a pooled Boss37Event: read it in the handler, don't keep it):
//   boss37Tell (stage "tell"), boss37Attack ("active"), boss37Rest ("burst" | "budget" | "restart" | "window"), boss37Stagger ("drain"
//   | "parry"), boss37Window ("open" | "close"), boss37Leg ("fight" | "change" | "start" | "restart"), boss37Hide ("reposition").
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Boss37State : byte { Off, Reveal, Rest, Tell, Active, Recover, Window, LegChange, Dying, Dead }

    /// <summary>What an attack denies (the habit reader weighs it) or answers (camp: only when the camp clock is due).</summary>
    public enum Boss37Habit : byte { Any = 0, Ground = 1, Air = 2, Camp = 3 }

    /// <summary>One attack of a Boss37 boss: its stable id (the tell's and GG's sound's name), its colour, its timing and its pick rules.</summary>
    public sealed class Boss37Attack
    {
        public string id;
        public Tell36Kind kind = Tell36Kind.Red;
        /// <summary>Whole sim steps: the wind-up (the tell), the attack, the tail.</summary>
        public int windupSteps = 1, activeSteps = 1, recoverSteps = 1;
        /// <summary>Budget points (1-3: MinosPrime.cs:520-740).</summary>
        public int cost = 1;
        /// <summary>The pick's weight (1 = an even roll).</summary>
        public double weight = 1;
        /// <summary>The distance band the attack is allowed in (metres, boss to player).</summary>
        public double minDist = 0, maxDist = 1e9;
        /// <summary>The legs it is allowed in: bit 0 = leg 1, bit 1 = leg 2, bit 2 = leg 3 (7 = every leg).</summary>
        public int legMask = 7;
        public Boss37Habit habit = Boss37Habit.Any;

        /// <summary>The pity bonus (Gabriel.cs:74-128): +pityStep each turn it is not picked, 0 when picked.</summary>
        public double pity;
        /// <summary>Times picked this fight (the harness).</summary>
        public int picks;

        public bool InLeg(int leg) => leg >= 1 && leg <= 3 && (legMask & (1 << (leg - 1))) != 0;

        /// <summary>An attack from seconds (whole steps at dt).</summary>
        public static Boss37Attack Of(string id, Tell36Kind kind, double windup, double active, double recover, double dt, int cost = 1,
            double weight = 1, double minDist = 0, double maxDist = 1e9, int legMask = 7, Boss37Habit habit = Boss37Habit.Any) =>
            new Boss37Attack
            {
                id = id, kind = kind, windupSteps = MovementMath.StepsFor(windup, dt), activeSteps = MovementMath.StepsFor(active, dt),
                recoverSteps = MovementMath.StepsFor(recover, dt), cost = cost, weight = weight, minDist = minDist, maxDist = maxDist,
                legMask = legMask, habit = habit,
            };

        /// <summary>
        /// An attack from a config object at `path` (a boss's row): id (default: the last path key), kind ("yellow" | "red"), windup, active,
        /// recover (seconds), cost, weight, minDist, maxDist, legs ([1, 2, 3]), habit ("any" | "ground" | "air" | "camp"). Missing = the default.
        /// </summary>
        public static Boss37Attack From(GameConfig c, string path, double dt)
        {
            string key = path.Substring(path.LastIndexOf('.') + 1);
            var a = new Boss37Attack { id = c.Has(path + ".id") ? c.Str(path + ".id") : key };
            if (c.Has(path + ".kind")) a.kind = c.Str(path + ".kind") == "yellow" ? Tell36Kind.Yellow : Tell36Kind.Red;
            if (c.Has(path + ".windup")) a.windupSteps = MovementMath.StepsFor(c.Num(path + ".windup"), dt);
            if (c.Has(path + ".active")) a.activeSteps = MovementMath.StepsFor(c.Num(path + ".active"), dt);
            if (c.Has(path + ".recover")) a.recoverSteps = MovementMath.StepsFor(c.Num(path + ".recover"), dt);
            if (c.Has(path + ".cost")) a.cost = c.Int(path + ".cost");
            if (c.Has(path + ".weight")) a.weight = c.Num(path + ".weight");
            if (c.Has(path + ".minDist")) a.minDist = c.Num(path + ".minDist");
            if (c.Has(path + ".maxDist")) a.maxDist = c.Num(path + ".maxDist");
            if (c.Has(path + ".legs"))
            {
                a.legMask = 0;
                foreach (double l in c.Nums(path + ".legs")) if (l >= 1 && l <= 3) a.legMask |= 1 << ((int)l - 1);
            }
            if (c.Has(path + ".habit"))
            {
                string h = c.Str(path + ".habit");
                a.habit = h == "ground" ? Boss37Habit.Ground : h == "air" ? Boss37Habit.Air : h == "camp" ? Boss37Habit.Camp : Boss37Habit.Any;
            }
            return a;
        }
    }

    /// <summary>design.n37.boss37's shared numbers (each with its _why in the config). A boss's row overrides any of them under `kit`.</summary>
    public sealed class Boss37Params
    {
        public double restSeconds = 1.5, budgetRestSeconds = 2, parryRefund = 5, pityStep = 0.25, windowSeconds = 2.5, legChangeSeconds = 1.5,
            restartRestSeconds = 1.5, habitSeconds = 8, habitGain = 2, campSeconds = 3, campRadius = 4, hideFill = 1, hideDrain = 2,
            hideSeconds = 3, hideReach = 20, hideBelow = 15, chipScale = 0.1;
        public int burst = 2;
        public bool autoLegs = true, habitSnap = true;
        /// <summary>Per leg (index 0 = leg 1): the stagger meter's full value, the budget cap.</summary>
        public double[] stagger = { 6, 9, 12 }, budgetCap = { 6, 6, 10 };
        /// <summary>The hpFrac at which leg 1 ends and leg 2 ends (autoLegs).</summary>
        public double[] legAt = { 2 / 3.0, 1 / 3.0 };

        public const string Row = "design.n37.boss37";

        /// <summary>The shared numbers, each overridden by design.n37.&lt;bossRow&gt;.kit.&lt;key&gt; when the boss's row has it (bossRow null: none).</summary>
        public static Boss37Params From(GameConfig c, string bossRow = null)
        {
            var p = new Boss37Params();
            string over = bossRow == null ? null : "design.n37." + bossRow + ".kit.";
            string Pick(string k) => over != null && c.Has(over + k) ? over + k : c.Has(Row + "." + k) ? Row + "." + k : null;
            double N(string k, double d) { string q = Pick(k); return q == null ? d : c.Num(q); }
            double[] A(string k, double[] d) { string q = Pick(k); return q == null ? d : c.Nums(q); }
            bool B(string k, bool d) { string q = Pick(k); return q == null ? d : c.Bool(q); }
            p.restSeconds = N("restSeconds", p.restSeconds);
            p.budgetRestSeconds = N("budgetRestSeconds", p.budgetRestSeconds);
            p.parryRefund = N("parryRefund", p.parryRefund);
            p.pityStep = N("pityStep", p.pityStep);
            p.windowSeconds = N("windowSeconds", p.windowSeconds);
            p.legChangeSeconds = N("legChangeSeconds", p.legChangeSeconds);
            p.restartRestSeconds = N("restartRestSeconds", p.restartRestSeconds);
            p.habitSeconds = N("habitSeconds", p.habitSeconds);
            p.habitGain = N("habitGain", p.habitGain);
            p.campSeconds = N("campSeconds", p.campSeconds);
            p.campRadius = N("campRadius", p.campRadius);
            p.hideFill = N("hideFill", p.hideFill);
            p.hideDrain = N("hideDrain", p.hideDrain);
            p.hideSeconds = N("hideSeconds", p.hideSeconds);
            p.hideReach = N("hideReach", p.hideReach);
            p.hideBelow = N("hideBelow", p.hideBelow);
            p.chipScale = N("chipScale", p.chipScale);
            p.burst = (int)N("burst", p.burst);
            p.autoLegs = B("autoLegs", p.autoLegs);
            p.habitSnap = B("habitSnap", p.habitSnap);
            p.stagger = A("stagger", p.stagger);
            p.budgetCap = A("budgetCap", p.budgetCap);
            p.legAt = A("legAt", p.legAt);
            return p;
        }

        public double StaggerOf(int leg) => stagger.Length == 0 ? 6 : stagger[Math.Min(stagger.Length, Math.Max(1, leg)) - 1];
        public double CapOf(int leg) => budgetCap.Length == 0 ? 6 : budgetCap[Math.Min(budgetCap.Length, Math.Max(1, leg)) - 1];
    }

    /// <summary>What the boss's sim tells the brain each step about the player and itself.</summary>
    public struct Boss37Context
    {
        /// <summary>The player's feet and velocity; the boss's centre (the distance, the hide rule's "below").</summary>
        public Vec3 player, playerVel, boss;
        /// <summary>Standing on a floor / in the air / on a wall (a rider is not "grounded"; airborne = !grounded &amp;&amp; !riding).</summary>
        public bool grounded, airborne, riding;
        /// <summary>The player is moving the level's way (riding, skating, pulled, mid jump-dash: the boss decides; rule 2).</summary>
        public bool movingTheWay;
        /// <summary>The boss sees the player (its own ray).</summary>
        public bool lineOfSight;
        /// <summary>The boss's own "out of reach" reason (a tower it can't reach, a ledge it's cut off from): fills the hide timer too.</summary>
        public bool outOfReach;
        /// <summary>The boss's bar, 1..0 (autoLegs).</summary>
        public double hpFrac;
        /// <summary>Where a new tell's flash sits (a limb, the core, the aim point).</summary>
        public Vec3 aim;
    }

    /// <summary>The payload of the boss37 events (pooled per brain).</summary>
    public sealed class Boss37Event
    {
        public string boss, attack, stage;
        public Tell36Kind kind;
        public int leg;
        public double seconds, stagger, budget;
        public Vec3 at;
    }

    public sealed class Boss37Brain
    {
        public const string EvTell = "boss37Tell", EvAttack = "boss37Attack", EvRest = "boss37Rest", EvStagger = "boss37Stagger",
            EvWindow = "boss37Window", EvLeg = "boss37Leg", EvHide = "boss37Hide";

        public readonly Boss37Params p;
        public readonly string boss;
        readonly double dt;
        readonly EventBus events;
        readonly Mulberry32 rng;
        readonly Boss37Event said = new Boss37Event();
        /// <summary>The boss's attacks (it fills them before Begin; pity and picks reset at Begin).</summary>
        public readonly List<Boss37Attack> attacks = new List<Boss37Attack>();

        public Boss37State state = Boss37State.Off;
        /// <summary>Steps spent in the current state, steps left in it, its full length.</summary>
        public int stateSteps, stateLeft, stateTotal;
        /// <summary>The state started this step (a boss reads it to fire its attack's start once).</summary>
        public bool entered;
        /// <summary>The attack in TELL / ACTIVE / RECOVER (-1: none), and the one before it (never picked twice running).</summary>
        public int attack = -1, previous = -1;
        /// <summary>The leg (1..3).</summary>
        public int leg = 1;
        /// <summary>The stagger meter (StaggerFull at a leg's start and after a window).</summary>
        public double stagger;
        /// <summary>The attack budget spent since the last forced rest.</summary>
        public double budget;
        /// <summary>Attacks chained in the current burst.</summary>
        public int burstAt;
        /// <summary>The habit reader: 0 = the player lives on the ground, 1 = in the air / on the walls.</summary>
        public double airBias = 0.5;
        /// <summary>The camp clock (steps) and its anchor; due = the next pick is a camp attack.</summary>
        public Vec3 campAnchor;
        public int campSteps;
        public bool campDue;
        /// <summary>The hide timer in steps x hideFill (fills 1 per step hidden, drains hideDrain/hideFill per step seen); at hideSeconds: reposition.</summary>
        public double hideSteps;
        /// <summary>The anti-hide fired: the boss moves onto the player, then ConsumeReposition().</summary>
        public bool RepositionDue;
        /// <summary>Each leg's checkpoint (index 1..3; the boss's sim sets them: SetCheckpoint).</summary>
        public readonly Vec3[] checkpoints = new Vec3[4];
        public readonly double[] checkpointYaw = new double[4];
        /// <summary>The tells registry (null in a bare harness) and the source id the tells carry (the boss body's Enemy.id).</summary>
        public Tells36 tells;
        public int source;
        /// <summary>A queued attack (Force) taken at the next pick instead of the roll (-1: none).</summary>
        int forced = -1;
        Boss37Context last;

        /// <summary>Counters for the harness and the reels.</summary>
        public int yellowTells, redTells, windows, budgetRests, burstRests, parries, hides, legChanges, restarts, campPicks, repeats;
        public readonly List<(string attack, int steps)> tellLog = new List<(string, int)>();

        public Boss37Brain(Boss37Params p, string boss, double dt, EventBus events, uint seed)
        {
            this.p = p;
            this.boss = boss;
            this.dt = dt;
            this.events = events;
            rng = new Mulberry32(seed);
        }

        public int S(double sec) => MovementMath.StepsFor(sec, dt);

        public Boss37Attack Current => attack >= 0 && attack < attacks.Count ? attacks[attack] : null;
        public double StaggerFull => p.StaggerOf(leg);
        public double StaggerFrac => StaggerFull > 0 ? Math.Max(0, Math.Min(1, stagger / StaggerFull)) : 0;
        /// <summary>0 at the state's start, 1 at its end (0 for an open-ended state).</summary>
        public double Progress => stateTotal > 0 ? Math.Min(1, stateSteps / (double)stateTotal) : 0;
        /// <summary>The CYAN window is open: the boss's damage rule applies full damage.</summary>
        public bool Open => state == Boss37State.Window;
        /// <summary>The boss can be hurt and drained (not in the reveal, a leg change, the dying).</summary>
        public bool Fighting => state == Boss37State.Rest || state == Boss37State.Tell || state == Boss37State.Active || state == Boss37State.Recover || state == Boss37State.Window;
        /// <summary>The damage multiplier for a hit: 1 in the window or for the boss's right move (rightMove), chipScale otherwise; 0 outside a fight.</summary>
        public double Scale(bool rightMove) => !Fighting ? 0 : Open || rightMove ? 1 : p.chipScale;

        /// <summary>Clear everything (Off).</summary>
        public void Reset()
        {
            state = Boss37State.Off;
            stateSteps = stateLeft = stateTotal = 0;
            entered = false;
            attack = previous = forced = -1;
            leg = 1;
            stagger = p.StaggerOf(1);
            budget = 0;
            burstAt = 0;
            airBias = 0.5;
            campSteps = 0;
            campDue = false;
            hideSteps = 0;
            RepositionDue = false;
            yellowTells = redTells = windows = budgetRests = burstRests = parries = hides = legChanges = restarts = campPicks = repeats = 0;
            tellLog.Clear();
            foreach (Boss37Attack a in attacks) { a.pity = 0; a.picks = 0; }
        }

        void Go(Boss37State s, int steps)
        {
            if (state == Boss37State.Tell && Current != null) tellLog.Add((Current.id, stateSteps));
            state = s;
            stateSteps = 0;
            stateLeft = stateTotal = steps;
            entered = true;
        }

        Boss37Event Say(string stage)
        {
            said.boss = boss;
            said.stage = stage;
            said.attack = Current?.id;
            said.kind = Current?.kind ?? Tell36Kind.None;
            said.leg = leg;
            said.seconds = stateTotal * dt;
            said.stagger = stagger;
            said.budget = budget;
            said.at = last.aim;
            return said;
        }

        /// <summary>Start the walk-in reveal (revealSteps without damage or attacks), then leg 1. The leg-1 checkpoint defaults to `player`.</summary>
        public void Begin(int revealSteps, Vec3 player)
        {
            Reset();
            campAnchor = player;
            if (checkpoints[1].Equals(Vec3.Zero)) checkpoints[1] = player;
            Go(Boss37State.Reveal, Math.Max(1, revealSteps));
        }

        /// <summary>A leg's checkpoint (where RestartLeg puts the player back).</summary>
        public void SetCheckpoint(int forLeg, Vec3 at, double yawDeg = 0)
        {
            if (forLeg < 1 || forLeg > 3) return;
            checkpoints[forLeg] = at;
            checkpointYaw[forLeg] = yawDeg;
        }

        /// <summary>Queue an attack (by id) for the next pick instead of the roll (a boss's forced staple). False if unknown.</summary>
        public bool Force(string id)
        {
            for (int i = 0; i < attacks.Count; i++) if (attacks[i].id == id) { forced = i; return true; }
            return false;
        }

        /// <summary>The boss's death: the brain stops (the boss plays its own finisher).</summary>
        public void Die(int dyingSteps)
        {
            tells?.End(source);
            attack = -1;
            Go(Boss37State.Dying, Math.Max(1, dyingSteps));
        }

        /// <summary>The boss's right move landed: drain the meter; at 0 the CYAN window opens. Returns true if this call opened it.</summary>
        public bool Stagger(double amount, string why = "drain")
        {
            if (!Fighting || Open || amount <= 0) return false;
            stagger = Math.Max(0, stagger - amount);
            events?.Emit(EvStagger, Say(why));
            if (stagger > 0) return false;
            windows++;
            tells?.End(source);
            attack = -1;
            burstAt = 0;
            Go(Boss37State.Window, Math.Max(1, S(p.windowSeconds)));
            events?.Emit(EvWindow, Say("open"));
            return true;
        }

        /// <summary>The player parried one of the boss's attacks: the budget refunds parryRefund (MinosPrime.cs:1125) and the meter drains by
        /// `stagger`. A parried attack in TELL / ACTIVE is broken (to RECOVER). Returns true if the window opened.</summary>
        public bool Parried(double staggerAmount)
        {
            if (!Fighting || Open) return false;
            parries++;
            budget = Math.Max(0, budget - p.parryRefund);
            if ((state == Boss37State.Tell || state == Boss37State.Active) && Current != null)
            {
                tells?.End(source);
                Go(Boss37State.Recover, Math.Max(1, Current.recoverSteps));
            }
            return Stagger(staggerAmount, "parry");
        }

        /// <summary>The next leg now (a boss whose legs are places: a lap, a tower pair). No-op in leg 3 or outside a fight.</summary>
        public void NextLeg()
        {
            if (leg >= 3 || !(Fighting || state == Boss37State.Reveal)) return;
            tells?.End(source);
            attack = -1;
            leg++;
            legChanges++;
            Go(Boss37State.LegChange, Math.Max(1, S(p.legChangeSeconds)));
            events?.Emit(EvLeg, Say("change"));
        }

        /// <summary>A fall or a death inside the fight (D-6): the leg starts again from its checkpoint. The boss's HP (its own) and the stagger
        /// meter are kept; the budget, the burst, the camp / hide clocks and the live tell are cleared; a restartRestSeconds rest. Returns the
        /// checkpoint (the boss's sim puts the player there and takes the health cost).</summary>
        public Vec3 RestartLeg()
        {
            Vec3 at = checkpoints[Math.Max(1, Math.Min(3, leg))];
            if (state == Boss37State.Off || state == Boss37State.Dying || state == Boss37State.Dead) return at;
            restarts++;
            tells?.End(source);
            attack = -1;
            budget = 0;
            burstAt = 0;
            campSteps = 0;
            campDue = false;
            hideSteps = 0;
            RepositionDue = false;
            campAnchor = at;
            if (state != Boss37State.Reveal && state != Boss37State.LegChange) Go(Boss37State.Rest, Math.Max(1, S(p.restartRestSeconds)));
            events?.Emit(EvLeg, Say("restart"));
            return at;
        }

        /// <summary>NIGHT40 B4 (loom40): keeps a LEGCHANGE open one more step (a boss whose body still glides to its new place: the change ends
        /// when it arrives, the 1-2 s phase transition of boss_rules 6). No-op in any other state; the caller bounds how long it holds.</summary>
        public void HoldLegChange()
        {
            if (state == Boss37State.LegChange && stateLeft < 2) stateLeft = 2;
        }

        /// <summary>The boss has repositioned onto the player (clears RepositionDue). Returns whether one was due.</summary>
        public bool ConsumeReposition()
        {
            bool was = RepositionDue;
            RepositionDue = false;
            return was;
        }

        /// <summary>One fixed step.</summary>
        public void Step(in Boss37Context ctx)
        {
            entered = false;
            last = ctx;
            if (state == Boss37State.Off || state == Boss37State.Dead) return;
            stateSteps++;
            if (Fighting)
            {
                Habit(ctx);
                Camp(ctx);
                Hide(ctx);
                // autoLegs: the bar crossing legAt changes the leg (ends a window early: the arena changes now)
                // (hpFrac 0 = not given / dead: never a leg change)
                if (p.autoLegs && leg < 3 && leg - 1 < p.legAt.Length && ctx.hpFrac > 0 && ctx.hpFrac <= p.legAt[leg - 1])
                {
                    if (Open) events?.Emit(EvWindow, Say("close"));
                    NextLeg();
                    return;
                }
            }
            if (stateLeft > 0 && --stateLeft > 0) return;
            switch (state)
            {
                case Boss37State.Reveal:
                    Go(Boss37State.Rest, Math.Max(1, S(p.restSeconds)));
                    events?.Emit(EvLeg, Say("fight"));
                    break;
                case Boss37State.Rest:
                    StartAttack(ctx);
                    break;
                case Boss37State.Tell:
                    Go(Boss37State.Active, Math.Max(1, Current.activeSteps));
                    events?.Emit(EvAttack, Say("active"));
                    break;
                case Boss37State.Active:
                    Go(Boss37State.Recover, Math.Max(1, Current.recoverSteps));
                    break;
                case Boss37State.Recover:
                    AfterAttack(ctx);
                    break;
                case Boss37State.Window:
                    stagger = StaggerFull;
                    events?.Emit(EvWindow, Say("close"));
                    ToRest(p.restSeconds, "window");
                    break;
                case Boss37State.LegChange:
                    stagger = StaggerFull;
                    budget = 0;
                    burstAt = 0;
                    campAnchor = ctx.player;
                    campSteps = 0;
                    events?.Emit(EvLeg, Say("start"));
                    ToRest(p.restSeconds, "leg");
                    break;
                case Boss37State.Dying:
                    state = Boss37State.Dead;
                    break;
            }
        }

        void ToRest(double seconds, string why)
        {
            attack = -1;
            burstAt = 0;
            Go(Boss37State.Rest, Math.Max(1, S(seconds)));
            events?.Emit(EvRest, Say(why));
        }

        void AfterAttack(in Boss37Context ctx)
        {
            burstAt++;
            if (budget >= p.CapOf(leg))
            {
                budget = 0;
                budgetRests++;
                ToRest(p.budgetRestSeconds, "budget");
                return;
            }
            if (burstAt >= Math.Max(1, p.burst))
            {
                burstRests++;
                ToRest(p.restSeconds, "burst");
                return;
            }
            StartAttack(ctx);
        }

        // the habit reader: drift across habitSeconds toward 1 (air / wall) or 0 (ground)
        void Habit(in Boss37Context ctx)
        {
            double step = 1.0 / Math.Max(1, S(p.habitSeconds));
            if (ctx.grounded) airBias = Math.Max(0, airBias - step);
            else airBias = Math.Min(1, airBias + step);
        }

        void Camp(in Boss37Context ctx)
        {
            double dx = ctx.player.x - campAnchor.x, dz = ctx.player.z - campAnchor.z;
            // riding a wall or leaving the circle restarts the clock; a hop inside the circle pauses it (it counts only grounded steps)
            if (ctx.riding || dx * dx + dz * dz > p.campRadius * p.campRadius) { campAnchor = ctx.player; campSteps = 0; }
            else if (ctx.grounded && ++campSteps >= S(p.campSeconds)) campDue = true;
        }

        void Hide(in Boss37Context ctx)
        {
            double dist = Vec3.DistanceTo(ctx.player, ctx.boss);
            bool hidden = ctx.outOfReach || !ctx.lineOfSight || dist > p.hideReach || ctx.boss.y - ctx.player.y > p.hideBelow;
            if (hidden) hideSteps += p.hideFill;
            else hideSteps = Math.Max(0, hideSteps - p.hideDrain);
            if (hideSteps >= S(p.hideSeconds) * p.hideFill)
            {
                hideSteps = 0;
                hides++;
                RepositionDue = true;
                events?.Emit(EvHide, Say("reposition"));
            }
        }

        /// <summary>Allowed now: its leg, its distance band, camp attacks only when the camp clock is due.</summary>
        public bool Allowed(int i, double dist)
        {
            Boss37Attack a = attacks[i];
            if (!a.InLeg(leg)) return false;
            if (dist < a.minDist || dist > a.maxDist) return false;
            if (a.habit == Boss37Habit.Camp && !campDue) return false;
            return true;
        }

        /// <summary>The habit multiplier of an attack (ground attacks weigh more for a grounded player, air attacks for an airborne one).</summary>
        public double HabitMul(Boss37Attack a) =>
            a.habit == Boss37Habit.Ground ? 1 + p.habitGain * (1 - airBias) : a.habit == Boss37Habit.Air ? 1 + p.habitGain * airBias : 1;

        /// <summary>The weighted pick with pity (Gabriel.cs:74-128). -1 if nothing is allowed. Public for the harness.</summary>
        public int Pick(double dist)
        {
            if (forced >= 0 && forced < attacks.Count) { int f = forced; forced = -1; return Took(f, dist); }
            // camp due and a camp attack allowed: it wins outright (the anti-camp answer)
            if (campDue)
            {
                for (int i = 0; i < attacks.Count; i++)
                    if (attacks[i].habit == Boss37Habit.Camp && Allowed(i, dist)) { campPicks++; campDue = false; campSteps = 0; return Took(i, dist); }
            }
            int best = -1, only = -1, allowed = 0;
            double bestScore = double.NegativeInfinity;
            for (int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i].habit == Boss37Habit.Camp || !Allowed(i, dist)) continue;
                allowed++;
                only = i;
                if (i == previous) continue;
                Boss37Attack a = attacks[i];
                double score = rng.Next() * a.weight * HabitMul(a) + a.pity;
                if (score > bestScore) { bestScore = score; best = i; }
            }
            if (best < 0 && allowed == 1) { repeats++; best = only; } // the only allowed attack: a repeat beats a stall
            return best < 0 ? -1 : Took(best, dist);
        }

        // the pity: the picked attack's resets, every other attack that could have been picked now gains pityStep (an attack out of its
        // leg or distance band, or a camp attack, gains none: its pity would pile up while gated and then win every roll once allowed)
        int Took(int i, double dist)
        {
            for (int j = 0; j < attacks.Count; j++)
            {
                if (j == i) attacks[j].pity = 0;
                else if (attacks[j].habit != Boss37Habit.Camp && Allowed(j, dist)) attacks[j].pity += p.pityStep;
            }
            Boss37Attack a = attacks[i];
            a.picks++;
            if (p.habitSnap && a.habit == Boss37Habit.Ground) airBias = 1; // denied the ground: expect the air next (Minotaur.cs:399-410)
            else if (p.habitSnap && a.habit == Boss37Habit.Air) airBias = 0;
            return i;
        }

        void StartAttack(in Boss37Context ctx)
        {
            int pick = Pick(Vec3.DistanceTo(ctx.player, ctx.boss));
            if (pick < 0) { ToRest(p.restSeconds, "none"); return; }
            previous = pick;
            attack = pick;
            Boss37Attack a = attacks[pick];
            budget += a.cost;
            int w = Math.Max(1, a.windupSteps);
            Go(Boss37State.Tell, w);
            if (a.kind == Tell36Kind.Yellow) yellowTells++;
            else redTells++;
            tells?.Raise(source, a.id, a.kind, w, ctx.aim);
            events?.Emit(EvTell, Say("tell"));
        }
    }
}
