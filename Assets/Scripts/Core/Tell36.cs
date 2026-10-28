// NIGHT36 builder P (tells36; NIGHT36.md card P1, PLAN.md section 1 and 4 "one tell grammar"): every enemy attack that can hit the
// player says it is coming. A TELL is one attack's wind-up: a pose plus a colour flash on the attacker for windupSteps sim steps,
// then the attack lands (or fires). YELLOW = parryable (a punch meeting it is a parry: parry36), RED = dodge only. One tell per
// source at a time: raising a new one on a source ends the old one first (never two overlapping).
// ULTRAKILL's reference (Research/2026-10-12-wave36/research/rea-ultrakill-combat.md section 3): the parryableFlash /
// unparryableFlash spawned at the moment an attack commits (DefaultReferenceManager.cs:31-33); Filth tracks during the wind-up,
// then locks (ZombieMelee :458-467); Malicious Face's 2 s charge then a 0.5 s lock (SpiderBody :681-725). Birkhead (design-web.md
// rule 5): wind-ups of about 1-1.5 s for melee and big shots. Numbers live in design.n36.tells36 (never here).
//
// THE SHAPE (shared with builder B's Boss36: a boss raises its tells through the same registry):
// - Raise: `tells.Raise(source, attack, Tell36Kind.Yellow | Red, windupSteps, at)`; source = the attacker's Enemy.id (a boss: its
//   body's id), attack = a short stable id ("huskMelee", "spitterVolley", "lanternOrbFan", ...), windupSteps = whole sim steps until
//   the attack lands (MovementMath.StepsFor(seconds, dt)), at = the world point the flash sits on (the attacker's aim point, a
//   boss's limb or core). Emits "tell36" with the Tell36 (a pooled object: read it in the handler, don't keep it).
// - The registry counts every live tell down once per sim step (Step, from SurvivalSim.Core36's N36CoreStep). At 0 the tell ends
//   ("tell36End", the source id). The attacker may end it early (End: a parry, a stagger, a death) or move its flash point (Move).
// - A view reads `Live` (or `Of(source)`) every frame: the kind's colour (UnityTuning.Tell36's yellow / red), the Progress 0..1 for
//   the pose and the flash's rise, and `at`. Game/Tell36View.cs draws it on the ordinary enemies; Boss36View reads the same.
// - Parryable projectiles (spitter globs, drone orbs; a boss's YELLOW orbs) glow yellow in the view; they need no tell of their own.
// - Disabled (the tells36 row off), Raise returns null and nothing is kept or emitted: the off game is bit for bit tonight's base.
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>A tell's colour: YELLOW = parryable (punch it), RED = dodge only.</summary>
    public enum Tell36Kind : byte
    {
        None = 0,
        Yellow = 1,
        Red = 2,
    }

    /// <summary>One attack's wind-up (pooled: read it, don't keep it).</summary>
    public sealed class Tell36
    {
        /// <summary>The attacker: an Enemy.id (a boss: its body's id).</summary>
        public int source;

        /// <summary>A short stable id of the attack ("huskMelee", "spitterVolley", "droneOrb", "lanternOrbFan", ...).</summary>
        public string attack;

        public Tell36Kind kind;

        /// <summary>The wind-up's full length in sim steps, and the steps left until the attack lands.</summary>
        public int windupSteps, stepsLeft;

        /// <summary>Where the flash sits (web space: the attacker's aim point, a boss's limb or core).</summary>
        public Vec3 at;

        public bool live;

        /// <summary>0 at the wind-up's start, 1 when the attack lands.</summary>
        public double Progress => windupSteps > 0 ? 1 - stepsLeft / (double)windupSteps : 1;

        public bool Parryable => kind == Tell36Kind.Yellow;
    }

    /// <summary>The run's live tells (one per source), counted down per sim step.</summary>
    public sealed class Tells36
    {
        /// <summary>The event names (GG's sounds: PORT-STATUS's event list).</summary>
        public const string Event = "tell36", EndEvent = "tell36End";

        readonly EventBus events;
        readonly List<Tell36> pool = new List<Tell36>();

        /// <summary>Every live tell (a view reads it each frame; don't change it).</summary>
        public readonly List<Tell36> Live = new List<Tell36>();

        /// <summary>The tells36 row this run (SurvivalSim.Core36 sets it at the run's start). Off: Raise does nothing.</summary>
        public bool enabled;

        /// <summary>Counters (the harness, the reels): raised per kind, ended early.</summary>
        public int yellows, reds, endedEarly;

        public Tells36(EventBus events) => this.events = events;

        /// <summary>A new run: no tells, the counters at 0.</summary>
        public void Reset()
        {
            foreach (Tell36 t in Live)
            {
                t.live = false;
                pool.Add(t);
            }
            Live.Clear();
            yellows = reds = endedEarly = 0;
        }

        /// <summary>The live tell of a source, or null.</summary>
        public Tell36 Of(int source)
        {
            for (int i = 0; i < Live.Count; i++)
                if (Live[i].source == source) return Live[i];
            return null;
        }

        /// <summary>Raise a tell (ends the source's live one first). Returns it, or null with the row off or windupSteps under 1.</summary>
        public Tell36 Raise(int source, string attack, Tell36Kind kind, int windupSteps, Vec3 at)
        {
            if (!enabled || windupSteps < 1 || kind == Tell36Kind.None) return null;
            End(source, false);
            Tell36 t;
            if (pool.Count > 0)
            {
                t = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
            }
            else t = new Tell36();
            t.source = source;
            t.attack = attack;
            t.kind = kind;
            t.windupSteps = t.stepsLeft = windupSteps;
            t.at = at;
            t.live = true;
            Live.Add(t);
            if (kind == Tell36Kind.Yellow) yellows++;
            else reds++;
            events.Emit(Event, t);
            return t;
        }

        /// <summary>Move a source's flash point (an attacker that walks or turns during its wind-up).</summary>
        public void Move(int source, Vec3 at)
        {
            Tell36 t = Of(source);
            if (t != null) t.at = at;
        }

        /// <summary>Keep a source's steps left in step with the attacker's own clock (a husk's windupSteps).</summary>
        public void Sync(int source, int stepsLeft)
        {
            Tell36 t = Of(source);
            if (t != null) t.stepsLeft = stepsLeft;
        }

        /// <summary>End a source's tell before it lands (a parry, a stagger, a death). Returns true if one was live.</summary>
        public bool End(int source) => End(source, true);

        public bool End(int source, bool early)
        {
            for (int i = 0; i < Live.Count; i++)
            {
                Tell36 t = Live[i];
                if (t.source != source) continue;
                t.live = false;
                Live.RemoveAt(i);
                pool.Add(t);
                if (early) endedEarly++;
                events.Emit(EndEvent, Boxes.Int(source));
                return true;
            }
            return false;
        }

        /// <summary>One sim step: every tell counts down; at 0 it ends (its attack landed or fired).</summary>
        public void Step()
        {
            for (int i = Live.Count - 1; i >= 0; i--)
            {
                Tell36 t = Live[i];
                if (--t.stepsLeft > 0) continue;
                t.live = false;
                Live.RemoveAt(i);
                pool.Add(t);
                events.Emit(EndEvent, Boxes.Int(t.source));
            }
        }
    }
}
