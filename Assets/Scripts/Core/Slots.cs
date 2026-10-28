// Port of skygrind/src/ai/slots.js (NIGHT9 U3-07, line for line; checked by PortExport/vectors/slots.json).
// Engagement slots (SURVIVAL.md §5, S4). Pure: no three.js, unit tested. Rings of slots around the player
// (melee 2-3 m, mid 8-12 m, far 18-30 m); each enemy claims the nearest free slot on its preferred ring,
// so a group spreads out instead of stacking on one point. Slots are fixed compass directions around the
// player (they move with the player, not with the camera); a claim is kept until released, and an enemy
// may trade up to a nearer free slot.
// The Survival brain doesn't claim ring slots yet (HuskBrain waits on a ring at its own angle); it uses flankPoint.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Slots
    {
        public sealed class Ring
        {
            public double radius;
            public int count;
            public int?[] owners; // null: free
        }

        public readonly Ring[] rings;

        /// <summary>The web's byOwner Map, owner -> {ring, index}, in its iteration order (a claim releases first, then goes last).</summary>
        public readonly List<(int owner, int ring, int index)> byOwner = new List<(int, int, int)>();

        /// <summary>createSlots(rings)</summary>
        public Slots(IList<(double radius, int count)> rings)
        {
            this.rings = new Ring[rings.Count];
            for (int i = 0; i < rings.Count; i++) this.rings[i] = new Ring { radius = rings[i].radius, count = rings[i].count, owners = new int?[rings[i].count] };
        }

        /// <summary>Slot position (x, z) on ring `ring` around `center`.</summary>
        public static void SlotPoint(Slots slots, int ring, int index, double cx, double cz, out double x, out double z)
        {
            Ring r = slots.rings[ring];
            double a = (double)index / r.count * Math.PI * 2;
            x = cx + JsMath.Cos(a) * r.radius;
            z = cz + JsMath.Sin(a) * r.radius;
        }

        /// <summary>
        /// Claim the nearest free slot on `ring` for `owner` at `from` (x, z), releasing any slot it had. `accept`
        /// (optional) filters slot points (e.g. only reachable ones). A claim on the same ring that is already the
        /// nearest free one is kept. Returns the slot index, or -1 when the ring is full.
        /// </summary>
        public static int ClaimSlot(Slots slots, int ring, int owner, double fromX, double fromZ, double cx, double cz, Func<double, double, bool> accept = null)
        {
            Ring r = slots.rings[ring];
            int best = -1;
            double bestD = double.PositiveInfinity;
            for (int i = 0; i < r.count; i++)
            {
                if (r.owners[i] != null && r.owners[i] != owner) continue;
                SlotPoint(slots, ring, i, cx, cz, out double px, out double pz);
                if (accept != null && !accept(px, pz)) continue;
                double d = JsMath.Hypot(px - fromX, pz - fromZ);
                if (d < bestD)
                {
                    best = i;
                    bestD = d;
                }
            }
            if (best < 0) return -1;
            ReleaseSlot(slots, owner);
            r.owners[best] = owner;
            slots.byOwner.Add((owner, ring, best));
            return best;
        }

        static int Held(Slots slots, int owner)
        {
            for (int k = 0; k < slots.byOwner.Count; k++) if (slots.byOwner[k].owner == owner) return k;
            return -1;
        }

        public static bool ReleaseSlot(Slots slots, int owner)
        {
            int k = Held(slots, owner);
            if (k < 0) return false;
            var held = slots.byOwner[k];
            slots.rings[held.ring].owners[held.index] = null;
            slots.byOwner.RemoveAt(k);
            return true;
        }

        /// <summary>The slot an owner holds ({ring, index}), or false (the web's null).</summary>
        public static bool SlotOf(Slots slots, int owner, out int ring, out int index)
        {
            int k = Held(slots, owner);
            ring = k < 0 ? -1 : slots.byOwner[k].ring;
            index = k < 0 ? -1 : slots.byOwner[k].index;
            return k >= 0;
        }

        /// <summary>Every claimed slot is held by exactly one owner (a test invariant).</summary>
        public static bool SlotsUnique(Slots slots)
        {
            var seen = new HashSet<(int, int)>();
            foreach (var (owner, ring, index) in slots.byOwner)
            {
                if (seen.Contains((ring, index)) || slots.rings[ring].owners[index] != owner) return false;
                seen.Add((ring, index));
            }
            return true;
        }

        /// <summary>
        /// Flank slots: points on `ring` behind or beside the player's facing (yaw 0 looks toward -z). `side`:
        /// 0 = straight behind, +1 / -1 = behind-right / behind-left (60 deg off).
        /// </summary>
        public static void FlankPoint(double cx, double cz, double yaw, double radius, double side, out double x, out double z)
        {
            double back = Math.PI + side * (Math.PI / 3); // relative to the facing
            double a = yaw + back;
            x = cx - JsMath.Sin(a) * radius;
            z = cz - JsMath.Cos(a) * radius;
        }
    }
}
