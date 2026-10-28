// NIGHT57 builder G (G2 press57, sim row design.n57.press57; NIGHT57.md 5 G2; GG 2026-10-21 "2. yes": attack behaviour on EXISTING kinds may
// change, looks and new kinds stay his): STANDING STILL LOSES. research/design.md lesson 2, ULTRAKILL's Virtue: its insignia "follows and surrounds
// the player for about 3 seconds", then fires at that spot, so "constant horizontal movement is encouraged" (https://ultrakill.wiki.gg/wiki/Virtue).
// Given to the SPITTER's existing attack (its tell, then three lobbed globs and their acid; count, speed, damage and acid unchanged):
//   MARK    at the spitter's commit (its tell's first step, the "spitterTell" event: the sim calls Mark57At) the spot under its target's feet is
//           marked, and the telegraph module draws its red-orange danger circle there (E-4: "avoid this area, it hits when full"), numbers.markR
//           m across the spot, filling until the globs land: the tell (spitter.tellTime; tells36's 1 s) + the globs' flight to the spot
//           (spitter.globTimeMin..Max by the distance) = 1.5-2.3 s.
//   STRIKE  at the throw (ThrowVolley's one call-site line: Press57Aim) the volley flies to the marked spot: no lead and no deliberate miss (the
//           miss roll's stream, acid.rng, is not drawn for a marked volley). A player who stood still is hit and stands in the acid; a player
//           who moved is not: the spot is where they were.
// (b) ranged aim worse with the player's speed ALREADY ships (AimMath.HitChance, ai.aim; NIGHT56 G.md), and a marked volley never leads, so a
//     moving player is never at its spot: nothing added.
// The marks are a fixed pool (spitter.globPool slots: one per volley in flight at most, allocated once at the first run with the row on), stepped
// on the sim's step (Step57, after the enemies): an unthrown mark ends when its spitter leaves its tell without throwing (killed, staggered);
// a thrown one when its globs land. No allocation per step; no randomness. Off (the bit false, --off57 / -skygrindOff57): on57 false and
// Press57Aim returns false at its first test: NIGHT56's spitter bit for bit. The marks are not in the run snapshot (row on only; they last <= 2.3 s).
using System;

namespace Skygrind.Core
{
    /// <summary>design.n57.press57.numbers (each number's _why there).</summary>
    public sealed class Press57Params
    {
        public const string Row = "design.n57.press57.numbers.";
        public double markR;

        public static Press57Params From(GameConfig c) => new Press57Params { markR = c.Num(Row + "markR") };
    }

    public sealed partial class SpitterAcid
    {
        /// <summary>One marked spot: its spitter, the spot (the target's feet at the commit), its age and the age it lands at (s).</summary>
        public sealed class Mark57
        {
            public bool alive, thrown;
            public Vec3 at;
            public Spitter owner;
            public double age, land;
            /// <summary>The telegraph circle's id (0: none drawn).</summary>
            public int tele;
        }

        /// <summary>press57 on for this step (the sim sets it every step: campaign runs only, as tokens56).</summary>
        public bool on57;
        public Press57Params p57;
        /// <summary>The pool (null until the first run with the row on).</summary>
        public Mark57[] marks57;
        /// <summary>Counters (the harness): spots marked, volleys thrown at a mark.</summary>
        public int marked57, struck57;

        /// <summary>The pool, once (StartRun with the row on).</summary>
        public void Alloc57()
        {
            if (marks57 != null) return;
            marks57 = new Mark57[Math.Max(1, p.globPool)];
            for (int i = 0; i < marks57.Length; i++) marks57[i] = new Mark57();
        }

        /// <summary>Every mark gone (a run's start; a cancelled circle on the view).</summary>
        public void Clear57(ITelegraph view)
        {
            marked57 = struck57 = 0;
            if (marks57 == null) return;
            foreach (Mark57 m in marks57)
            {
                if (m.alive && m.tele != 0) view?.Cancel(m.tele);
                m.alive = m.thrown = false;
                m.owner = null;
                m.tele = 0;
            }
        }

        /// <summary>The globs' flight time to `to` from the spitter's mouth (ThrowVolley's own rule).</summary>
        double Flight57(Spitter e, Vec3 to)
        {
            double d = JsMath.Hypot(to.x - e.position.x, to.z - e.position.z);
            return Math.Max(p.globTimeMin, Math.Min(p.globTimeMax, d / p.globSpeed));
        }

        /// <summary>MARK (the sim, at a spitter's commit): the spot under its target's feet, landing after the tell (tellS) and the flight; the
        /// telegraph's danger circle on it. A spitter with an unthrown mark keeps it. Returns the mark (null: off, or the pool is full).</summary>
        public Mark57 Mark57At(Spitter e, Vec3 feet, double tellS, ITelegraph view)
        {
            if (!on57 || marks57 == null || e == null) return null;
            Mark57 slot = null;
            foreach (Mark57 m in marks57)
            {
                if (m.alive && m.owner == e && !m.thrown) return m;
                if (slot == null && !m.alive) slot = m;
            }
            if (slot == null) return null; // full: this volley keeps the old aim
            slot.alive = true;
            slot.thrown = false;
            slot.owner = e;
            slot.at = feet;
            slot.age = 0;
            slot.land = tellS + Flight57(e, feet);
            slot.tele = view != null ? view.Circle(feet.x, feet.y, feet.z, p57.markR, slot.land) : 0;
            marked57++;
            return slot;
        }

        /// <summary>STRIKE (ThrowVolley's call-site line): a marked spitter's volley goes to its spot, landing in the flight time to it, with no
        /// miss; false (off, or no mark): ThrowVolley's own aim, untouched.</summary>
        bool Press57Aim(Spitter e, ref Vec3 at, ref double time)
        {
            if (!on57 || marks57 == null) return false;
            foreach (Mark57 m in marks57)
            {
                if (!m.alive || m.thrown || m.owner != e) continue;
                time = Flight57(e, m.at);
                at = new Vec3(m.at.x, m.at.y + 0.4, m.at.z); // ThrowVolley's own height over the feet (tp.y + 0.4)
                m.thrown = true;
                m.land = m.age + time;
                struck57++;
                return true;
            }
            return false;
        }

        /// <summary>One sim step (after the enemies): marks age; an unthrown mark whose spitter left its tell ends (its circle cancelled); a
        /// thrown one ends when its globs land.</summary>
        public void Step57(double dt, ITelegraph view)
        {
            if (marks57 == null) return;
            foreach (Mark57 m in marks57)
            {
                if (!m.alive) continue;
                m.age += dt;
                bool gone = m.thrown ? m.age >= m.land : m.owner == null || !m.owner.alive || m.owner.state != "tell";
                if (!gone) continue;
                if (!m.thrown && m.tele != 0) view?.Cancel(m.tele);
                m.alive = m.thrown = false;
                m.owner = null;
                m.tele = 0;
            }
        }
    }
}
