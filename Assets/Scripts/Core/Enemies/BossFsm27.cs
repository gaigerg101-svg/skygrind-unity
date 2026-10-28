// NIGHT27 (builder B): the boss state machine, the UnityHFSM pattern rewritten in pure C# for Skygrind.Core (no UnityEngine).
// Pattern source (read, not copied line for line): Minoteaur / UnityHFSM, https://github.com/Inspiaaa/UnityHFSM, MIT licence: the ideas of
// hierarchical states (a state may itself be a machine, so a boss phase holds its own attack machine), transitions evaluated on the active
// state only, and `needsExitTime` (a state with it may not be left until it says it can: an attack finishes its telegraph and its recovery).
// What differs, on purpose: ONE transition per step (a chain A -> B -> C takes two steps, so a boss never skips a state inside one fixed
// 120 Hz step); every timer is a whole number of steps (the caller makes them with MovementMath.StepsFor / BossFsm27.Steps); a roll
// is the machine's own Mulberry32 (Roll), never System.Random / UnityEngine.Random; no delegates are kept across a run start (Reset()).
// Order inside Step(): 1. a pending exit (a transition that wanted to leave a needsExitTime state) is taken the step the state can exit;
// 2. otherwise the machine's own transitions out of the active state are tried in the order added, the first true wins; 3. the active state's
// OnLogic runs (a nested machine steps itself here). The state's `steps` counts the whole steps since it was entered (OnEnter sees 0).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>One state: enter / logic / exit, an optional exit gate, and the whole steps spent in it.</summary>
    public class FsmState27
    {
        public readonly string name;
        public Action<FsmState27> onEnter, onLogic, onExit;
        /// <summary>true: the state may not be left until canExit says so (default: always may exit once its minSteps passed).</summary>
        public bool needsExitTime;
        public Func<FsmState27, bool> canExit;
        public int minSteps;
        /// <summary>Whole steps since entry (0 in OnEnter, 1 in the first OnLogic).</summary>
        public int steps;
        /// <summary>The machine this state lives in (null for the root).</summary>
        public Fsm27 parent;

        public FsmState27(string name) { this.name = name; }

        public virtual void Enter() { steps = 0; onEnter?.Invoke(this); }
        public virtual void Logic() { steps++; onLogic?.Invoke(this); }
        public virtual void Exit() { onExit?.Invoke(this); }

        /// <summary>May the state be left now? (always true when it does not need exit time)</summary>
        public virtual bool CanExit()
        {
            if (!needsExitTime) return true;
            if (steps < minSteps) return false;
            return canExit == null || canExit(this);
        }
    }

    /// <summary>A machine of states, itself a state (hierarchy). Transitions are (from, to, condition).</summary>
    public sealed class Fsm27 : FsmState27
    {
        sealed class Transition
        {
            public string from, to;
            public Func<bool> when;
            public bool forceInstant;
        }

        readonly Dictionary<string, FsmState27> states = new Dictionary<string, FsmState27>();
        readonly Dictionary<string, List<Transition>> from = new Dictionary<string, List<Transition>>();
        readonly List<Transition> any = new List<Transition>();
        Transition pending;
        string start;
        FsmState27 active;
        public Mulberry32 rng = new Mulberry32(1);
        /// <summary>The steps of one second, for the timers' callers (set by the owner from the config's dt).</summary>
        public double dt = 1.0 / 120;

        public Fsm27(string name) : base(name) { }

        public FsmState27 Active => active;
        public string ActiveName => active?.name;
        /// <summary>The names from this machine down to the deepest active state (a/b/c).</summary>
        public string Path => active == null ? "" : active is Fsm27 f ? active.name + "/" + f.Path : active.name;

        /// <summary>Whole steps for a duration in seconds (movementMath.stepsFor).</summary>
        public int Steps(double seconds) => MovementMath.StepsFor(seconds, dt);

        /// <summary>A roll in [0, 1) from this machine's Mulberry32 (a child machine shares its parent's stream through Share).</summary>
        public double Roll() => rng.Next();

        public int RollInt(int n)
        {
            int i = (int)(rng.Next() * n);
            return i >= n ? n - 1 : i;
        }

        public Fsm27 Add(FsmState27 s)
        {
            s.parent = this;
            states[s.name] = s;
            if (start == null) start = s.name;
            return this;
        }

        public FsmState27 Add(string name, Action<FsmState27> enter = null, Action<FsmState27> logic = null, Action<FsmState27> exit = null, bool needsExitTime = false, int minSteps = 0, Func<FsmState27, bool> canExit = null)
        {
            var s = new FsmState27(name) { onEnter = enter, onLogic = logic, onExit = exit, needsExitTime = needsExitTime, minSteps = minSteps, canExit = canExit };
            Add(s);
            return s;
        }

        public void SetStart(string name) => start = name;

        public Fsm27 Transit(string fromState, string to, Func<bool> when, bool forceInstant = false)
        {
            var t = new Transition { from = fromState, to = to, when = when, forceInstant = forceInstant };
            if (fromState == null) any.Add(t);
            else
            {
                if (!from.TryGetValue(fromState, out var l)) from[fromState] = l = new List<Transition>();
                l.Add(t);
            }
            return this;
        }

        public FsmState27 State(string name) => states.TryGetValue(name, out var s) ? s : null;

        /// <summary>Enter the start state (a fresh fight).</summary>
        public override void Enter()
        {
            base.Enter();
            pending = null;
            active = null;
            if (start != null) Go(start);
        }

        public override void Exit()
        {
            active?.Exit();
            active = null;
            pending = null;
            base.Exit();
        }

        /// <summary>Back to nothing (a run start): no state active, nothing pending.</summary>
        public void Reset()
        {
            active = null;
            pending = null;
            steps = 0;
        }

        /// <summary>Force a state now (the owner's own rule: a phase change).</summary>
        public void Go(string name)
        {
            if (!states.TryGetValue(name, out var next)) throw new KeyNotFoundException("Fsm27 " + this.name + " has no state " + name);
            active?.Exit();
            active = next;
            pending = null;
            active.Enter();
        }

        public override bool CanExit() => active == null || active.CanExit();

        public override void Logic()
        {
            steps++;
            onLogic?.Invoke(this);
            Step();
        }

        /// <summary>One fixed step of this machine: at most one transition, then the active state's logic.</summary>
        public void Step()
        {
            if (active == null) { if (start != null) Go(start); return; }
            if (pending != null)
            {
                if (active.CanExit()) { string to = pending.to; pending = null; Go(to); }
            }
            else
            {
                Transition t = Find();
                if (t != null)
                {
                    if (t.forceInstant || active.CanExit()) Go(t.to);
                    else pending = t;
                }
            }
            active.Logic();
        }

        Transition Find()
        {
            foreach (Transition t in any)
                if (t.to != active.name && t.when()) return t;
            if (from.TryGetValue(active.name, out var l))
                foreach (Transition t in l)
                    if (t.when()) return t;
            return null;
        }
    }
}
