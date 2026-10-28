// NIGHT36 builder B (boss36; NIGHT36.md card B1, PLAN.md section 4, design-web.md rules 4-7): the shared boss SHAPE every rebuilt boss runs on.
// One brain, pure (no scene, no UnityEngine), fixed 120 Hz, timers in whole steps:
//   REVEAL   the walk-in: the boss arrives in view and takes no damage until the fight starts (walkin36's timeline, the boss's own numbers).
//   REST     between attacks (restP1 / restP2 seconds; shorter after the 50 % change).
//   TELL     one attack's wind-up: its own pose plus ONE colour (YELLOW = parry it, RED = dodge it), 1-1.5 s (Birkhead: "beyond 2 seconds I
//            start counting"; design-web.md rule 5). Raised through builder P's Tells36 registry (Core/Tell36.cs) so every view reads one grammar.
//   ACTIVE   the attack runs (its boss decides what that means).
//   PUNISH   after each BIG attack: the boss is open long enough to reach it and land one full combo (Birkhead; design-web.md rule 5).
//   STUN     at stunAt of the bar (50 %): a free-hit window (Gabriel, design-web.md section 3), then ONE change (phase 2).
//   CAMP     the anti-camp attack: a player who stays inside campRadius of one spot, off the walls, for campSeconds (V2: distancePatience
//            reaches 12 at 2 per second = 6 s, V2.cs:708-731; Mindflayer 10-15 s, rea-ultrakill-combat.md section 3) gets the next attack
//            aimed at that spot, with its own RED tell.
// Never two tells at once: an attack's tell starts only from REST. No added health anywhere: the bar is the boss's own pool.
// Numbers: design.n36.boss36 (each with its _why). No audio: events keep names for GG's sounds (boss36Tell, boss36Punish, boss36Stun,
// boss36Phase, boss36Camp).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public enum Boss36State : byte { Off, Reveal, Rest, Tell, Active, Punish, Stun, Dying, Dead }

    /// <summary>One attack of a boss: its id (a stable string for the tell and GG's sounds), its colour and its timing.</summary>
    public sealed class Boss36Attack
    {
        public string id;
        public Tell36Kind kind;
        public double tellSeconds, activeSeconds, punishSeconds;
        /// <summary>The anti-camp attack (picked only when the camp clock fires).</summary>
        public bool camp;
    }

    /// <summary>design.n36.boss36's shared numbers.</summary>
    public sealed class Boss36Params
    {
        public double restP1, restP2, stunAt, stunSeconds, campSeconds, campRadius;

        public static Boss36Params From(GameConfig c)
        {
            const string g = "design.n36.boss36.";
            return new Boss36Params
            {
                restP1 = c.Num(g + "restP1"), restP2 = c.Num(g + "restP2"), stunAt = c.Num(g + "stunAt"), stunSeconds = c.Num(g + "stunSeconds"),
                campSeconds = c.Num(g + "campSeconds"), campRadius = c.Num(g + "campRadius"),
            };
        }
    }

    /// <summary>The payload of the boss36 events (pooled per brain: read it in the handler, don't keep it).</summary>
    public sealed class Boss36Event
    {
        public string boss, attack, stage;
        public Tell36Kind kind;
        public double seconds;
        public int phase;
        public Vec3 at;
    }

    public sealed class Boss36Brain
    {
        public readonly Boss36Params p;
        public readonly string boss;
        readonly double dt;
        readonly EventBus events;
        readonly Boss36Event said = new Boss36Event();
        public readonly List<Boss36Attack> attacks = new List<Boss36Attack>();
        /// <summary>The order the ordinary attacks come in, per phase (indexes into attacks); the camp attack is outside it.</summary>
        public int[] orderP1 = new int[0], orderP2 = new int[0];

        public Boss36State state = Boss36State.Off;
        /// <summary>Steps spent in the current state, and steps left in it (0: open-ended).</summary>
        public int stateSteps, stateLeft, stateTotal;
        /// <summary>The state started this step (a boss reads it to fire its attack's start once).</summary>
        public bool entered;
        /// <summary>The attack in TELL / ACTIVE / PUNISH (-1: none).</summary>
        public int attack = -1;
        public int phase = 1, orderAt;
        public bool stunDone;
        /// <summary>The camp clock: the anchor the player stays near, and how long he has stayed (steps).</summary>
        public Vec3 campAnchor, campAt;
        public int campSteps;
        bool campDue;
        /// <summary>The tells registry (P's; null in a bare harness) and the source id the tells carry (the boss body's Enemy.id).</summary>
        public Tells36 tells;
        public int source;
        /// <summary>Counters for the harness: tells per kind, punishes, stuns, camp attacks, and every tell's measured length in steps.</summary>
        public int yellowTells, redTells, punishes, stuns, camps;
        public readonly List<(string attack, int steps)> tellLog = new List<(string, int)>();

        public Boss36Brain(Boss36Params p, string boss, double dt, EventBus events)
        {
            this.p = p;
            this.boss = boss;
            this.dt = dt;
            this.events = events;
        }

        public int S(double sec) => MovementMath.StepsFor(sec, dt);

        public Boss36Attack Current => attack >= 0 && attack < attacks.Count ? attacks[attack] : null;
        /// <summary>0 at the state's start, 1 at its end (0 for an open-ended state).</summary>
        public double Progress => stateTotal > 0 ? Math.Min(1, stateSteps / (double)stateTotal) : 0;
        /// <summary>The boss is open to full hits now (a punish window or the stun).</summary>
        public bool Open => state == Boss36State.Punish || state == Boss36State.Stun;
        public bool Fighting => state == Boss36State.Rest || state == Boss36State.Tell || state == Boss36State.Active || state == Boss36State.Punish || state == Boss36State.Stun;

        public void Reset()
        {
            state = Boss36State.Off;
            stateSteps = stateLeft = stateTotal = 0;
            entered = false;
            attack = -1;
            phase = 1;
            orderAt = 0;
            stunDone = false;
            campSteps = 0;
            campDue = false;
            yellowTells = redTells = punishes = stuns = camps = 0;
            tellLog.Clear();
        }

        void Go(Boss36State s, int steps)
        {
            if (state == Boss36State.Tell && Current != null) tellLog.Add((Current.id, stateSteps));
            state = s;
            stateSteps = 0;
            stateLeft = stateTotal = steps;
            entered = true;
        }

        Boss36Event Say(string stage)
        {
            said.boss = boss;
            said.stage = stage;
            said.attack = Current?.id;
            said.kind = Current?.kind ?? Tell36Kind.None;
            said.seconds = stateTotal * dt;
            said.phase = phase;
            said.at = campAt;
            return said;
        }

        /// <summary>Start the walk-in reveal (revealSteps of no damage), then the fight.</summary>
        public void Begin(int revealSteps, Vec3 player)
        {
            Reset();
            campAnchor = player;
            Go(Boss36State.Reveal, Math.Max(1, revealSteps));
        }

        /// <summary>The boss's death: the brain stops (the boss plays its own finisher).</summary>
        public void Die(int dyingSteps)
        {
            tells?.End(source);
            attack = -1;
            Go(Boss36State.Dying, Math.Max(1, dyingSteps));
        }

        /// <summary>One fixed step. player = the player's feet, riding = on a wall (never camping), hpFrac = the bar. `aim` = where a tell's flash sits.</summary>
        public void Step(Vec3 player, bool riding, double hpFrac, Vec3 aim)
        {
            entered = false;
            if (state == Boss36State.Off || state == Boss36State.Dead) return;
            stateSteps++;
            // the camp clock (only while fighting): leaving the circle, or riding a wall, restarts it at the player's feet
            if (Fighting)
            {
                double dx = player.x - campAnchor.x, dz = player.z - campAnchor.z;
                if (riding || dx * dx + dz * dz > p.campRadius * p.campRadius) { campAnchor = player; campSteps = 0; }
                else if (++campSteps >= S(p.campSeconds) && !campDue) { campDue = true; campAt = campAnchor; }
            }
            // the 50 % stun interrupts anything but the reveal and the dying
            if (!stunDone && Fighting && state != Boss36State.Stun && hpFrac <= p.stunAt)
            {
                stunDone = true;
                stuns++;
                tells?.End(source);
                attack = -1;
                Go(Boss36State.Stun, S(p.stunSeconds));
                events?.Emit("boss36Stun", Say("start"));
                return;
            }
            if (stateLeft > 0 && --stateLeft > 0) return;
            switch (state)
            {
                case Boss36State.Reveal:
                    Go(Boss36State.Rest, S(p.restP1));
                    events?.Emit("boss36Phase", Say("fight"));
                    break;
                case Boss36State.Rest:
                    StartAttack(aim);
                    break;
                case Boss36State.Tell:
                    Go(Boss36State.Active, Math.Max(1, S(Current.activeSeconds)));
                    events?.Emit("boss36Attack", Say("active"));
                    break;
                case Boss36State.Active:
                    if (Current.punishSeconds > 0)
                    {
                        punishes++;
                        Go(Boss36State.Punish, S(Current.punishSeconds));
                        events?.Emit("boss36Punish", Say("start"));
                    }
                    else ToRest();
                    break;
                case Boss36State.Punish:
                    events?.Emit("boss36Punish", Say("end"));
                    ToRest();
                    break;
                case Boss36State.Stun:
                    phase = 2;
                    orderAt = 0; // phase 2's order from its start
                    ToRest();
                    events?.Emit("boss36Phase", Say("change"));
                    break;
                case Boss36State.Dying:
                    state = Boss36State.Dead;
                    break;
            }
        }

        void ToRest()
        {
            attack = -1;
            Go(Boss36State.Rest, S(phase >= 2 ? p.restP2 : p.restP1));
        }

        void StartAttack(Vec3 aim)
        {
            int pick = -1;
            if (campDue)
            {
                for (int i = 0; i < attacks.Count; i++) if (attacks[i].camp) { pick = i; break; }
                campDue = false;
                campSteps = 0;
                if (pick >= 0) camps++;
            }
            if (pick < 0)
            {
                int[] order = phase >= 2 && orderP2.Length > 0 ? orderP2 : orderP1;
                if (order.Length == 0) { ToRest(); return; }
                pick = order[orderAt % order.Length];
                orderAt++;
            }
            attack = pick;
            Boss36Attack a = attacks[pick];
            int w = Math.Max(1, S(a.tellSeconds));
            Go(Boss36State.Tell, w);
            if (a.kind == Tell36Kind.Yellow) yellowTells++;
            else redTells++;
            tells?.Raise(source, a.id, a.kind, w, a.camp ? campAt : aim);
            events?.Emit("boss36Tell", Say("tell"));
        }
    }
}
