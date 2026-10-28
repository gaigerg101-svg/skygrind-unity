// NIGHT40 builder B5 (crown40; NIGHT40.md B5, <boss_rules> 4; Research/2026-10-14-wave40/B5.md D3 / D4 / D5 / D7 / D8): THE CROWN's NIGHT40
// rules, read from design.n40.crown40 when the crown40 bit is on (the sim hands them to Crown37.n40; null = NIGHT39's Crown exactly):
//   hoverDistance  the ring hovers this far (flat) from the player (NIGHT37: crown.hoverDistance 5 m: the ring and its blades filled the
//                  view, frames f0700 / f1000).
//   THE CORE       the ring's rim never enters the spire's core column: its centre is kept coreHalf + ringRadius + coreMargin (flat) off the
//                  axis or ringRadius + coreMargin over the summit, whichever is the shorter move (D3: 9.9 % of fight steps in the core).
//   THE REVEAL     it rises from just over the summit (NIGHT37: from 8 m inside the core, still inside when the fight began).
//   THE CLAMP      the centre stays inside the boss room's rect less ringRadius + roomMargin (boss_rules 4).
//   THE ANTI-HIDE  no teleport (Crown37.cs:407): a reposition moves at repositionSpeed (eased in by MoveTo's straight line), so it is seen.
//   fallDamage     a fall's cost (NIGHT37: 20; falls are part of the descent: the skilled pilot fell 4.7 times a fight and died of it).
// Pure (no UnityEngine), JsMath-free arithmetic only (no transcendentals), no randomness.
using System;

namespace Skygrind.Core
{
    public sealed class Crown40Params
    {
        public double hoverDistance, coreMargin, roomMargin, repositionSpeed, fallDamage;
        /// <summary>The boss room's rect (x0, z0, x1, z1; web coordinates), set by the sim from the level (NaN: no clamp).</summary>
        public double x0 = double.NaN, z0, x1, z1;

        /// <summary>The harness's tuning probe (n40crown --set40 key=v): replaces a ring.* / fallDamage number; null in the game.</summary>
        public static System.Collections.Generic.Dictionary<string, double> Probe;

        public static Crown40Params From(GameConfig c)
        {
            const string g = "design.n40.crown40.ring.";
            double N(string key, string path) => Probe != null && Probe.TryGetValue(key, out double v) ? v : c.Num(path);
            return new Crown40Params
            {
                hoverDistance = N("hoverDistance", g + "hoverDistance"), coreMargin = N("coreMargin", g + "coreMargin"), roomMargin = N("roomMargin", g + "roomMargin"),
                repositionSpeed = N("repositionSpeed", g + "repositionSpeed"), fallDamage = N("fallDamage", "design.n40.crown40.fallDamage"),
            };
        }
    }

    public sealed partial class Crown37
    {
        /// <summary>NIGHT40 B5: crown40's rules (null: the row off, NIGHT39's Crown).</summary>
        public Crown40Params n40;
        /// <summary>NIGHT40 B5: core pushes and clamps this fight (the harness), the last blocked weapon hit (step, weapon: the view's BLOCKED).</summary>
        public int corePushes40, clamps40, lastBlockStep = -100000, lastHitStep = -100000;
        public string lastBlockWeapon;
        /// <summary>Steps left of a crown40 reposition (the anti-hide's fast, seen flight in place of the teleport).</summary>
        int fast40;

        double Hover40 => n40 != null ? n40.hoverDistance : p.hoverDistance;
        /// <summary>The flat distance the ring hovers from the player (the pilots line up on it).</summary>
        public double HoverDistance => Hover40;

        /// <summary>The ring's move this step with crown40 on: `next` (MoveTo's) when it keeps out of the core; else round the core: the angle
        /// about the axis turned toward `want`'s by the step's length, outward as needed (no jump: at most the step's length).</summary>
        Vec3 Round40(Vec3 prev, Vec3 next, Vec3 want, double stepL)
        {
            double R = p.ringRadius, half = orbitCoreHalf + R + n40.coreMargin, top = axis.y + R + n40.coreMargin;
            if (!InCore40(next, half, top)) return next;
            if (InCore40(prev, half, top)) return Keep40(next); // already inside (a fight's first step): out by the nearest face
            if (prev.y >= top - 1e-9) return new Vec3(next.x, top, next.z); // over the summit: it crosses at the summit's height, then comes down outside
            double px = prev.x - axis.x, pz = prev.z - axis.z, r = JsMath.Hypot(px, pz);
            double ap = JsMath.Atan2(pz, px), aw = JsMath.Atan2(want.z - axis.z, want.x - axis.x), d = aw - ap;
            while (d > Math.PI) d -= 2 * Math.PI;
            while (d < -Math.PI) d += 2 * Math.PI;
            double da = Math.Sign(d) * Math.Min(Math.Abs(d), stepL / Math.Max(1, r));
            double y = prev.y + Math.Max(-stepL, Math.Min(stepL, want.y - prev.y)) * 0.5;
            for (int k = 0; k < 8; k++, r += stepL * 0.5) // outward as needed (at most 4 steps' length; a 10 x 0.1 search held the ring stuck: 5/10 kills)
            {
                Vec3 t = new Vec3(axis.x + r * JsMath.Cos(ap + da), y, axis.z + r * JsMath.Sin(ap + da));
                if (!InCore40(t, half, top)) { rounds40++; return t; }
            }
            return prev; // held this step
        }

        /// <summary>Steps the ring went round the core (the harness).</summary>
        public int rounds40;

        bool InCore40(Vec3 c, double half, double top) => Math.Abs(c.x - axis.x) < half && Math.Abs(c.z - axis.z) < half && c.y < top;

        /// <summary>A centre the ring may sit at: out of the spire's core column, inside the boss room (crown40 on); `c` unchanged with it off.
        /// With `feet` (the hover point): first further out along the flat line from his feet (the line to the landing it offers stays the line);
        /// else (and for the move itself) out through the nearest face of the widened column. Never lifted over him (a ring over his head
        /// cannot be passed from the summit).</summary>
        Vec3 Keep40(Vec3 c, bool alongLine = false, Vec3 feet = default)
        {
            if (n40 == null) return c;
            double R = p.ringRadius, half = orbitCoreHalf + R + n40.coreMargin, top = axis.y + R + n40.coreMargin;
            if (InCore40(c, half, top))
            {
                bool done = false;
                if (alongLine)
                {
                    double fx = c.x - feet.x, fz = c.z - feet.z, fl = JsMath.Hypot(fx, fz);
                    if (fl > 1e-6)
                        for (double extra = 0.25; extra <= 12 && !done; extra += 0.25)
                        {
                            Vec3 t = new Vec3(c.x + fx / fl * extra, c.y, c.z + fz / fl * extra);
                            if (!InCore40(t, half, top)) { c = t; done = true; }
                        }
                }
                if (!done)
                {
                    double dx = c.x - axis.x, dz = c.z - axis.z, ox = half - Math.Abs(dx), oz = half - Math.Abs(dz);
                    if (ox <= oz) c = new Vec3(axis.x + (dx >= 0 ? half : -half), c.y, c.z);
                    else c = new Vec3(c.x, c.y, axis.z + (dz >= 0 ? half : -half));
                }
                corePushes40++;
            }
            if (!double.IsNaN(n40.x0))
            {
                double m = R + n40.roomMargin;
                double x = Math.Max(n40.x0 + m, Math.Min(n40.x1 - m, c.x)), z = Math.Max(n40.z0 + m, Math.Min(n40.z1 - m, c.z));
                if (x != c.x || z != c.z) { clamps40++; c = new Vec3(x, c.y, z); }
            }
            return c;
        }
    }
}
