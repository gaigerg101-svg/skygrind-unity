// NIGHT30 (builder S; design.n30.rules, item 1, DESIGN30 section A): THE WEAPON RULES, enforced in one place.
//   1. One thing at a time: a shot, a swing or a blast is an ACTION; its length is the weapon's attack time, capped at
//      design.n30.rules.actionSeconds (the punch's own punch.cycleTime 0.35 is the model): min(the cooldown it set, actionSteps).
//   2. A switch is instant unless an action is running; then it runs the moment that action ends (queued, the latest ask wins; never cancelled).
//   3. A cooldown is the wait after an action before that weapon can act again; it keeps running holstered (Combat.Step counts every gun's
//      cooldown down whatever is in hand; the punch's in StepPunch; the PULSE blast's in PulseCharge27.Step): nothing here pauses or restarts one.
//   4. The PULSE gauntlet's heavy swing (2.5 s, the gauntlet's cooldown) and its LT blast (15 s, PulseCharge27) follow rule 3; the blast needs the pips.
//   5. Fire and melee never overlap (NIGHT29's lockout, kept): a melee press during a gun's action, or with the gauntlet in hand, is dropped; the
//      trigger during a punch's action is dropped; nothing is buffered.
// The diagnosis (S's log line): NIGHT29's Lockout29.FilterSwitch was given `CooldownOf(Weapon) > 0 || punchCooldown > 0` (Combat.cs:697), so a
// switch waited for the WHOLE cooldown of the weapon in hand: the gauntlet's 2.5 s heavy swing (it IS in the cooldown table: Combat.cs:874,
// pulse27.SwingSteps), the shotgun's slowed pump, a rocket's interval; the melee lockout used the same whole cooldown. That is the "switch and wait the
// full cooldown". Here the busy window is the ACTION only. Off (rules30 null): NIGHT29's game bit for bit.
// The HUD's read (builder V): Loadout.Slots30 / Slot30 / Gauntlet30Now / Switching30 below (READY / COOLING / CHARGING, the share and the seconds left).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Rules30
    {
        /// <summary>design.n30.rules.actionSeconds in steps: the longest any action holds a switch or the other hand.</summary>
        public readonly int actionSteps;

        /// <summary>Steps left of the gun-hand action in progress (a shot, the gauntlet's heavy swing, the blast) and of the punch's.</summary>
        public int gunAction, meleeAction;

        /// <summary>A switch asked during an action, waiting for it to end (-1: none).</summary>
        public int queuedSwitch = -1;

        /// <summary>Counters (harness, reel): switches that waited, melee presses dropped, trigger steps dropped, the longest wait (steps) a switch had.</summary>
        public int waitedSwitches, droppedMelee, droppedFireSteps, longestWait;

        int waitSteps;
        readonly Dictionary<string, int> full = new Dictionary<string, int>();

        public Rules30(int actionSteps) => this.actionSteps = Math.Max(1, actionSteps);

        public static Rules30 From(GameConfig cfg) => new Rules30(MovementMath.StepsFor(cfg.Num("design.n30.rules.actionSeconds"), cfg.Dt));

        public void Reset()
        {
            gunAction = meleeAction = 0;
            queuedSwitch = -1;
            waitedSwitches = droppedMelee = droppedFireSteps = longestWait = waitSteps = 0;
            full.Clear();
        }

        /// <summary>Steps of the action in progress (either hand).</summary>
        public int ActionLeft => Math.Max(gunAction, meleeAction);

        public bool Busy => gunAction > 0 || meleeAction > 0;

        /// <summary>The switch for this step (before the cooldowns tick): during an action an ask is queued and -1 comes back; the queued ask runs when it ends.</summary>
        public int FilterSwitch(int want)
        {
            if (Busy)
            {
                if (want >= 0)
                {
                    if (queuedSwitch < 0) waitSteps = 0;
                    queuedSwitch = want;
                    waitedSwitches++;
                }
                if (queuedSwitch >= 0) waitSteps++;
                return -1;
            }
            if (want < 0 && queuedSwitch >= 0)
            {
                want = queuedSwitch;
                if (waitSteps > longestWait) longestWait = waitSteps;
            }
            queuedSwitch = -1;
            waitSteps = 0;
            return want;
        }

        /// <summary>One step (beside the cooldowns' tick): the actions run out.</summary>
        public void Step()
        {
            if (gunAction > 0) gunAction--;
            if (meleeAction > 0) meleeAction--;
        }

        /// <summary>A melee press is dropped now: the gauntlet is in hand (its swing is the melee), a gun-hand action runs, or a switch is queued (rule 2: the
        /// switch happens the moment the action ends, so no new action may start in between and starve it).</summary>
        public bool MeleeBlocked(bool gauntletInHand) => gauntletInHand || gunAction > 0 || queuedSwitch >= 0;

        /// <summary>The trigger is dropped now: a punch's action runs, or a switch is queued (rule 2).</summary>
        public bool FireBlocked => meleeAction > 0 || queuedSwitch >= 0;

        /// <summary>A shot or a heavy swing set weapon `w`'s cooldown to `steps`: the action is min(steps, actionSteps); the full length is kept for the HUD's share.</summary>
        public void OnGunAction(string w, int steps)
        {
            full[w] = steps;
            gunAction = Math.Min(steps, actionSteps);
        }

        /// <summary>A punch went out with its cooldown `steps`.</summary>
        public void OnPunch(int steps) => meleeAction = Math.Min(steps, actionSteps);

        /// <summary>The PULSE's LT blast went out: an action of actionSteps.</summary>
        public void OnBlast() => gunAction = actionSteps;

        /// <summary>The last full cooldown weapon `w` set (0: none yet).</summary>
        public int FullOf(string w) => w != null && full.TryGetValue(w, out int n) ? n : 0;
    }

    /// <summary>NIGHT30: a weapon slot's state for the HUD (shape + colour + word: DESIGN30 F).</summary>
    public enum WeaponState30
    {
        Ready,
        Cooling,
        Charging,
    }

    /// <summary>One owned weapon's read for the weapon-state widget.</summary>
    public struct SlotRead30
    {
        public string weapon;
        public WeaponState30 state;
        /// <summary>COOLING: the share of the cooldown left (1 at the action, 0 ready); CHARGING: the charge's share built.</summary>
        public double share;
        /// <summary>COOLING: seconds left.</summary>
        public double seconds;
        public bool inHand;
        /// <summary>A switch to this slot is queued behind the action in progress, or the swap to it is running ("SWITCHING").</summary>
        public bool switching;
    }

    /// <summary>The gauntlet's two slots (the reviewer's fix: PUNCH and BLAST, DESIGN30 I-2).</summary>
    public struct Gauntlet30
    {
        public bool owned;
        /// <summary>PUNCH: the heavy swing's cooldown (the gauntlet's cooldown table entry).</summary>
        public WeaponState30 punch;
        public double punchShare, punchSeconds;
        /// <summary>BLAST: CHARGING while the pips fill (share = pips / pipsMax), COOLING after a blast, READY when full and cool.</summary>
        public WeaponState30 blast;
        public int pips, pipsMax;
        public double blastShare, blastSeconds;
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT30 rules (null with design.n30.rules off: NIGHT29's game). The sim hands it over at a run's start.</summary>
        public Rules30 rules30;

        /// <summary>Weapon `w`'s cooldown steps left (the harness's and the HUD's read; it runs holstered).</summary>
        public int CooldownStepsOf(string w) => CooldownOf(w);

        /// <summary>The slot index a switch is going to (a queued ask, else the swap in progress), or -1.</summary>
        public int Switching30 => rules30 != null && rules30.queuedSwitch >= 0 ? rules30.queuedSwitch : swapActive ? targetSlot : -1;

        /// <summary>Owned weapon `i`'s state.</summary>
        public SlotRead30 Slot30(int i)
        {
            var s = new SlotRead30();
            if (i < 0 || i >= owned.Count) return s;
            string w = owned[i];
            double dt = cfg.Dt;
            s.weapon = w;
            s.inHand = i == slot;
            s.switching = i == Switching30 && i != slot;
            int c = CooldownOf(w);
            int ci = ChargeShots27.Index(w);
            if (s.inHand && charge27 != null && charge27.active && charge27.charging && ci >= 0)
            {
                s.state = WeaponState30.Charging;
                s.share = charge27.Level(ci);
            }
            else if (s.inHand && w == "rail" && RailCharge > 0)
            {
                s.state = WeaponState30.Charging;
                s.share = RailCharge;
            }
            else if (c > 0)
            {
                int f = rules30 != null ? Math.Max(rules30.FullOf(w), c) : c;
                s.state = WeaponState30.Cooling;
                s.share = Math.Min(1.0, c / (double)Math.Max(1, f));
                s.seconds = c * dt;
            }
            else s.state = WeaponState30.Ready;
            return s;
        }

        /// <summary>Every owned weapon's state into `into` (no allocation); the count written.</summary>
        public int Slots30(SlotRead30[] into)
        {
            int n = Math.Min(into.Length, owned.Count);
            for (int i = 0; i < n; i++) into[i] = Slot30(i);
            return n;
        }

        /// <summary>The gauntlet's PUNCH (heavy swing) and BLAST (pips, cooldown) reads.</summary>
        public Gauntlet30 Gauntlet30Now()
        {
            var g = new Gauntlet30 { owned = owned.Contains("gauntlet") };
            double dt = cfg.Dt;
            int c = CooldownOf("gauntlet");
            if (c > 0)
            {
                int f = rules30 != null ? Math.Max(rules30.FullOf("gauntlet"), c) : c;
                g.punch = WeaponState30.Cooling;
                g.punchShare = Math.Min(1.0, c / (double)Math.Max(1, f));
                g.punchSeconds = c * dt;
            }
            else g.punch = WeaponState30.Ready;
            PulseCharge27 pc = pulse27;
            if (pc != null && pc.active)
            {
                g.pips = pc.pips;
                g.pipsMax = pc.p.pips;
                if (pc.cooldownSteps > 0)
                {
                    g.blast = WeaponState30.Cooling;
                    g.blastShare = pc.CooldownShare;
                    g.blastSeconds = pc.cooldownSteps * dt;
                }
                else if (!pc.Full)
                {
                    g.blast = WeaponState30.Charging;
                    g.blastShare = g.pipsMax > 0 ? g.pips / (double)g.pipsMax : 0;
                }
                else g.blast = WeaponState30.Ready;
            }
            return g;
        }
    }
}
