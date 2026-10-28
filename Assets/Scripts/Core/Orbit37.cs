// NIGHT37 builder C (crown37; NIGHT37.md C1, BOSSES.md "L5 THE CROWN" engine work): the descent's MOVING and DROPPING pieces, one world layer
// ("crown37Spire"). It extends Devices33's moving platform (a solid box whose rider standing on its top is carried by the step's displacement,
// no Mover.cs edit) from a two-point path to a RING path round the spire's axis, and adds pieces that drop:
//   summit   the spire's top (four slabs and the bridge's far half): solid until the Crown shatters it (Shatter: a tell, then gone for good).
//   orbit    a ring platform: its top centre circles `centre` at `radius`, `dir` +1 / -1, at design.n37.crown37.pieces.orbitSpeed m/s along the
//            ring (the angle in whole steps from the run's start: deterministic). A grounded rider on its top moves with it (the carry). The
//            box never turns (an axis-aligned box), so a carried rider keeps his place on it exactly (the harness prints the carry error).
//   gallery  a slab of the broken spiral (leg 2): the Crown's beam cut marks one (RED), then it drops; it comes back returnSeconds later.
//   plaza    an outer sector of the plaza (leg 3): dropped one by one, back only on a leg restart.
//   fall     a rehearsal stone (room v9): it drops fallDelay s after the player first stands on it, comes back fallReturn s later.
// A piece is UP (solid), TELL (solid, the view shows it red and shaking), or DOWN (out of the collider; the view lets it fall away for
// dropSeconds, then hides it). A dropped piece is never solid: the floor goes out from under you at once (no riding a falling slab).
// Pure (no UnityEngine), 120 Hz whole steps, JsMath for the ring (no System.Math transcendentals in Core), no randomness.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Orbit37Params
    {
        public double orbitSpeed, rideMargin, rideHeight, fallDelay, fallReturn, galleryReturn, orbitReturn, dropSeconds, shatterTell;

        public static Orbit37Params From(GameConfig c)
        {
            const string g = "design.n37.crown37.pieces.";
            return new Orbit37Params
            {
                orbitSpeed = c.Num(g + "orbitSpeed"), rideMargin = c.Num(g + "rideMargin"), rideHeight = c.Num(g + "rideHeight"),
                fallDelay = c.Num(g + "fallDelay"), fallReturn = c.Num(g + "fallReturn"), galleryReturn = c.Num(g + "galleryReturn"),
                orbitReturn = c.Num(g + "orbitReturn"), dropSeconds = c.Num(g + "dropSeconds"), shatterTell = c.Num(g + "shatterTell"),
            };
        }
    }

    public enum Piece37Kind : byte { Summit, Orbit, Gallery, Plaza, Fall }

    public enum Piece37State : byte { Up, Tell, Down }

    /// <summary>One moving or dropping piece: its level row and its live state.</summary>
    public sealed class Piece37
    {
        public string id, room;
        public Piece37Kind kind;
        public int leg;
        /// <summary>The box's full size (w along x, thick along y, d along z) and its rest top centre (orbit: unused).</summary>
        public Vec3 size, at;
        /// <summary>Orbit: the ring's centre (its y = the top), radius, phase (a share of a turn) and direction (+1 / -1).</summary>
        public Vec3 centre;
        public double radius, phase, dir = 1;
        /// <summary>The top centre now and one step ago (the view interpolates; the carry is the difference).</summary>
        public Vec3 pos, prev;
        public Piece37State state;
        /// <summary>Steps in the current state and steps left in it (0: open-ended); the drop's step (the view's fall).</summary>
        public int stateSteps, stateLeft;
        /// <summary>The tell's full length (the view's shake share).</summary>
        public int tellTotal;
        /// <summary>When the TELL ends the piece drops; when DOWN ends (stateLeft > 0) it comes back.</summary>
        public int returnSteps;
        /// <summary>Why it is telling: "cut" (the beam), "habit" (stood on too long), "fall" (a rehearsal stone), "shatter", "plaza".</summary>
        public string cause;

        public bool Solid => state != Piece37State.Down;
        public double TellShare => tellTotal > 0 ? Math.Min(1, stateSteps / (double)tellTotal) : 0;
    }

    /// <summary>The payload of crown37Piece (stage tell / drop / back): pooled, read it in the handler.</summary>
    public sealed class Piece37Event
    {
        public string id, stage, kind, cause;
        public int index, leg;
        public Vec3 at;
    }

    public sealed class Orbit37
    {
        public readonly Orbit37Params p;
        public readonly List<Piece37> pieces = new List<Piece37>();
        /// <summary>The pieces' live boxes (GameWorld layer "crown37Spire").</summary>
        public readonly BoxLayer layer = new BoxLayer();
        readonly double dt;
        readonly List<(Vec3 min, Vec3 max)> boxes = new List<(Vec3, Vec3)>();
        readonly Piece37Event ev = new Piece37Event();

        /// <summary>Steps since the run started (every orbit's clock).</summary>
        public int steps;
        /// <summary>The piece the player stands on (-1: none), and how many steps he has stood on it (the habit rule).</summary>
        public int standing = -1, standSteps;
        /// <summary>Counters (harness, reel): carry steps, drops, returns; the largest carry error seen (m: the rider's offset from the piece's top
        /// centre drifting while carried).</summary>
        public int carrySteps, drops, backs;
        public double carryErrorMax;
        Vec3 carryOffset;
        bool carryLast;

        public Orbit37(Orbit37Params p, double dt)
        {
            this.p = p;
            this.dt = dt;
        }

        public int S(double sec) => MovementMath.StepsFor(sec, dt);
        public int Count => pieces.Count;

        /// <summary>The pieces of a level JSON: every campaign room's `spire37.pieces` and `fall37` rows.</summary>
        public void Load(Dictionary<string, object> mapJson)
        {
            pieces.Clear();
            if (mapJson == null || !mapJson.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rooms)) return;
            foreach (object r in rooms)
            {
                if (!(r is Dictionary<string, object> room)) continue;
                string rid = room.TryGetValue("id", out object id) ? id as string : null;
                if (room.TryGetValue("spire37", out object so) && so is Dictionary<string, object> spire && spire.TryGetValue("pieces", out object pl) && pl is List<object> list)
                    foreach (object o in list) Add(Parse(PortData.Obj(o), rid, null));
                if (room.TryGetValue("fall37", out object fo) && fo is List<object> falls)
                    foreach (object o in falls) Add(Parse(PortData.Obj(o), rid, "fall"));
            }
        }

        void Add(Piece37 q)
        {
            if (q != null) pieces.Add(q);
        }

        static Piece37 Parse(Dictionary<string, object> row, string room, string kindOverride)
        {
            string kind = kindOverride ?? (row.TryGetValue("kind", out object k) ? k as string : null);
            var q = new Piece37 { room = room, id = row.TryGetValue("id", out object id) ? id as string : kind, size = V3(row["size"]) };
            q.leg = row.TryGetValue("leg", out object lg) ? (int)PortData.Num(lg) : 0;
            switch (kind)
            {
                case "summit": q.kind = Piece37Kind.Summit; break;
                case "orbit": q.kind = Piece37Kind.Orbit; break;
                case "gallery": q.kind = Piece37Kind.Gallery; break;
                case "plaza": q.kind = Piece37Kind.Plaza; break;
                case "fall": q.kind = Piece37Kind.Fall; break;
                default: return null;
            }
            if (q.kind == Piece37Kind.Orbit)
            {
                var o = PortData.Obj(row["orbit"]);
                q.centre = V3(o["centre"]);
                q.radius = PortData.Num(o["radius"]);
                q.phase = PortData.Num(o["phase"]);
                q.dir = o.TryGetValue("dir", out object d) ? PortData.Num(d) : 1;
            }
            else q.at = V3(row["at"]);
            return q;
        }

        static Vec3 V3(object o)
        {
            var a = PortData.Arr(o);
            return new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]));
        }

        /// <summary>Everything up, every orbit at its start, the clocks at 0.</summary>
        public void Reset()
        {
            steps = 0;
            standing = -1;
            standSteps = carrySteps = drops = backs = 0;
            carryErrorMax = 0;
            carryLast = false;
            foreach (Piece37 q in pieces)
            {
                q.state = Piece37State.Up;
                q.stateSteps = q.stateLeft = q.tellTotal = q.returnSteps = 0;
                q.cause = null;
                q.pos = q.prev = At(q, 0);
            }
            Rebuild();
        }

        /// <summary>The ring's angular speed (rad/s) for a piece: orbitSpeed m/s along its ring.</summary>
        public double Rate(Piece37 q) => q.radius > 1e-6 ? p.orbitSpeed / q.radius : 0;

        /// <summary>A piece's top centre at step s (orbit: on its ring; the others: their rest place).</summary>
        public Vec3 At(Piece37 q, int s)
        {
            if (q.kind != Piece37Kind.Orbit) return q.at;
            double a = 2 * Math.PI * q.phase + q.dir * Rate(q) * s * dt;
            return new Vec3(q.centre.x + q.radius * JsMath.Cos(a), q.centre.y, q.centre.z + q.radius * JsMath.Sin(a));
        }

        /// <summary>Is a grounded player's feet standing on piece q's top at `top`?</summary>
        public bool Riding(Piece37 q, Vec3 top, Vec3 feet, bool grounded)
        {
            if (!grounded || !q.Solid) return false;
            double hx = q.size.x / 2 + p.rideMargin, hz = q.size.z / 2 + p.rideMargin;
            return Math.Abs(feet.x - top.x) <= hx && Math.Abs(feet.z - top.z) <= hz && feet.y >= top.y - p.rideHeight && feet.y <= top.y + p.rideHeight;
        }

        /// <summary>The index of the piece the player stands on now (-1: none).</summary>
        public int Under(Vec3 feet, bool grounded)
        {
            for (int i = 0; i < pieces.Count; i++) if (Riding(pieces[i], pieces[i].pos, feet, grounded)) return i;
            return -1;
        }

        /// <summary>Mark a piece: a TELL of `tellSeconds` (the view: red and shaking), then it drops; back after `returnSeconds` (0: never).</summary>
        public bool Mark(int i, double tellSeconds, double returnSeconds, string cause, EventBus events)
        {
            if (i < 0 || i >= pieces.Count) return false;
            Piece37 q = pieces[i];
            if (q.state != Piece37State.Up) return false;
            q.state = Piece37State.Tell;
            q.stateSteps = 0;
            q.stateLeft = q.tellTotal = Math.Max(1, S(tellSeconds));
            q.returnSteps = returnSeconds > 0 ? Math.Max(1, S(returnSeconds)) : 0;
            q.cause = cause;
            Emit(events, "tell", i);
            return true;
        }

        /// <summary>Drop a piece now (no tell: a leg change's collapse, a shatter's end); back after `returnSeconds` (0: never).</summary>
        public void Drop(int i, double returnSeconds, string cause, EventBus events)
        {
            Piece37 q = pieces[i];
            if (q.state == Piece37State.Down) return;
            q.cause = cause;
            q.returnSteps = returnSeconds > 0 ? Math.Max(1, S(returnSeconds)) : 0;
            Down(i, events);
        }

        void Down(int i, EventBus events)
        {
            Piece37 q = pieces[i];
            q.state = Piece37State.Down;
            q.stateSteps = 0;
            q.stateLeft = q.returnSteps;
            drops++;
            Emit(events, "drop", i);
        }

        /// <summary>Bring a piece back up now (a leg restart).</summary>
        public void Raise(int i, EventBus events)
        {
            Piece37 q = pieces[i];
            if (q.state == Piece37State.Up) return;
            q.state = Piece37State.Up;
            q.stateSteps = q.stateLeft = q.tellTotal = 0;
            q.cause = null;
            backs++;
            Emit(events, "back", i);
        }

        /// <summary>One fixed step, after the Mover (SurvivalSim N37CrownStep: Devices33's place in the step): the orbits move and carry their
        /// rider, every tell and drop counts down, the stand clock runs, the layer is rebuilt.</summary>
        public void Step(Mover mover, EventBus events)
        {
            steps++;
            int under = -1;
            bool carried = false;
            for (int i = 0; i < pieces.Count; i++)
            {
                Piece37 q = pieces[i];
                q.prev = q.pos;
                q.pos = At(q, steps);
                if (mover != null && under < 0 && Riding(q, q.prev, mover.position, mover.grounded))
                {
                    under = i;
                    Vec3 delta = new Vec3(q.pos.x - q.prev.x, q.pos.y - q.prev.y, q.pos.z - q.prev.z);
                    if (delta.x != 0 || delta.y != 0 || delta.z != 0)
                    {
                        mover.position = new Vec3(mover.position.x + delta.x, mover.position.y + delta.y, mover.position.z + delta.z);
                        carrySteps++;
                        carried = true;
                        // the carry error: a rider who keeps still on a carried piece must keep his offset from its top centre
                        Vec3 off = new Vec3(mover.position.x - q.pos.x, 0, mover.position.z - q.pos.z);
                        double still = JsMath.Hypot(mover.velocity.x, mover.velocity.z);
                        if (carryLast && still < 1e-6) carryErrorMax = Math.Max(carryErrorMax, JsMath.Hypot(off.x - carryOffset.x, off.z - carryOffset.z));
                        carryOffset = off;
                    }
                }
                if (q.state == Piece37State.Up) continue;
                q.stateSteps++;
                if (q.stateLeft > 0 && --q.stateLeft == 0)
                {
                    if (q.state == Piece37State.Tell) Down(i, events);
                    else Raise(i, events);
                }
            }
            carryLast = carried;
            if (under >= 0 && under == standing) standSteps++;
            else { standing = under; standSteps = under >= 0 ? 1 : 0; }
            // the rehearsal stones: the first step on one starts its fall
            if (standing >= 0 && pieces[standing].kind == Piece37Kind.Fall && pieces[standing].state == Piece37State.Up)
                Mark(standing, p.fallDelay, p.fallReturn, "fall", events);
            Rebuild();
        }

        void Rebuild()
        {
            boxes.Clear();
            foreach (Piece37 q in pieces)
            {
                if (!q.Solid) continue;
                Vec3 c = q.pos;
                boxes.Add((new Vec3(c.x - q.size.x / 2, c.y - q.size.y, c.z - q.size.z / 2), new Vec3(c.x + q.size.x / 2, c.y, c.z + q.size.z / 2)));
            }
            layer.SetBoxes(boxes);
        }

        void Emit(EventBus events, string stage, int i)
        {
            if (events == null) return;
            Piece37 q = pieces[i];
            ev.id = q.id;
            ev.stage = stage;
            ev.kind = q.kind.ToString().ToLowerInvariant();
            ev.cause = q.cause;
            ev.index = i;
            ev.leg = q.leg;
            ev.at = q.pos;
            events.Emit("crown37Piece", ev);
        }
    }

    /// <summary>The PASS test (the Crown's ring and room v10's ring target): did the segment a -> b (a body point's last step) cross the ring's
    /// plane (through c, unit normal n) within r of its centre?</summary>
    public static class Hoop37
    {
        public static bool Passed(Vec3 a, Vec3 b, Vec3 c, Vec3 n, double r, out Vec3 at)
        {
            at = default;
            double da = (a.x - c.x) * n.x + (a.y - c.y) * n.y + (a.z - c.z) * n.z;
            double db = (b.x - c.x) * n.x + (b.y - c.y) * n.y + (b.z - c.z) * n.z;
            if (da == db || (da > 0 && db > 0) || (da < 0 && db < 0) || (da == 0 && db == 0)) return false;
            double t = da / (da - db);
            at = new Vec3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
            double dx = at.x - c.x, dy = at.y - c.y, dz = at.z - c.z;
            return dx * dx + dy * dy + dz * dz <= r * r;
        }
    }
}
