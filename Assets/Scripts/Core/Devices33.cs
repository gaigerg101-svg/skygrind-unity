// NIGHT33 builder R (design.n33.devices33, DESIGN33 card R; K1-arena-kit.md 6b): arena devices that move the player or threaten them. Pure C#,
// deterministic (every device runs on the run's step counter and its own authored phase: no randomness), timers in whole steps (MovementMath.StepsFor).
// The level data is each room row's `devices33` array, which levelgen copies into the level JSON's `campaign.rooms` (Tools/night/gen-n33-*.cjs write it;
// web coordinates: x east, y up, z south; a rect is [x0, z0, x1, z1]):
//   { "kind": "fan",      "rect": [...], "y": floorY, "height": m, "refuge": [x, y, z], "note": "..." }
//       an UPDRAFT FAN: a grate on the floor and a column `height` m tall over it. A player whose feet are in the column is lifted: from the ground
//       the jump pad's path (Mover.Launch(lift)), in the air the vertical speed rises by accel x dt up to lift. Above the column top: plain gravity.
//   { "kind": "vent",     "rect": [...], "y": floorY, "phase": 0..1, "refuge": [x, y, z], "note": "..." }
//       a HAZARD VENT: a floor grate on a fixed cycle: idle, then the TELL (warn: the grate glows red-orange, event ventTell), then the BURST (event
//       ventBurst): a player whose feet are over the grate and below y + burstHeight takes damageFraction of max health and a knock up (the lava's
//       rule: Health.Damage, Mover.Launch), once per burst (event ventBurn). `phase` offsets the cycle (a share of it).
//   { "kind": "platform", "from": [x, y, z], "to": [x, y, z], "size": [w, thick, d], "phase": 0..1, "refuge": [x, y, z], "note": "..." }
//       a MOVING PLATFORM: a solid box (top centre at `from` / `to`) on a two-point path: dwell at `from`, travel at speed, dwell at `to`, travel back.
//       A live box layer in the world collider (GameWorld.SetLayer "devices33": the player, the enemies and the rays meet it); a player standing on
//       its top is carried by its step's displacement (the feet moved with it; no Mover.cs edit).
// The numbers are design.n33.devices33's (Design/unity-config.json): the web has no devices beyond the jump pad, so every number is a Unity-side one.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Devices33Params
    {
        public double fanLift, fanAccel, ventIdleSeconds, ventWarnSeconds, ventBurstSeconds, ventDamageFraction, ventKnockUp, ventBurstHeight;
        public double platformSpeed, platformDwellSeconds, platformRideMargin, platformRideHeight;

        public static Devices33Params From(GameConfig cfg) => new Devices33Params
        {
            fanLift = cfg.Num("design.n33.devices33.fan.lift"),
            fanAccel = cfg.Num("design.n33.devices33.fan.accel"),
            ventIdleSeconds = cfg.Num("design.n33.devices33.vent.idleSeconds"),
            ventWarnSeconds = cfg.Num("design.n33.devices33.vent.warnSeconds"),
            ventBurstSeconds = cfg.Num("design.n33.devices33.vent.burstSeconds"),
            ventDamageFraction = cfg.Num("design.n33.devices33.vent.damageFraction"),
            ventKnockUp = cfg.Num("design.n33.devices33.vent.knockUp"),
            ventBurstHeight = cfg.Num("design.n33.devices33.vent.burstHeight"),
            platformSpeed = cfg.Num("design.n33.devices33.platform.speed"),
            platformDwellSeconds = cfg.Num("design.n33.devices33.platform.dwellSeconds"),
            platformRideMargin = cfg.Num("design.n33.devices33.platform.rideMargin"),
            platformRideHeight = cfg.Num("design.n33.devices33.platform.rideHeight"),
        };
    }

    public enum Device33Kind { Fan, Vent, Platform }

    /// <summary>A vent's place in its cycle.</summary>
    public enum Vent33Phase { Idle, Warn, Burst }

    /// <summary>One device: its level row and its live state.</summary>
    public sealed class Device33
    {
        public Device33Kind kind;
        public string room, note;

        /// <summary>Fan / vent: the grate's rect (world x / z, min first) and floor y; the fan's column height.</summary>
        public double x0, z0, x1, z1, y, height;

        /// <summary>The cycle's offset, a share of it (vent, platform).</summary>
        public double phase;

        /// <summary>Platform: the top centre's two ends and the box's full size (w along x, thick along y, d along z).</summary>
        public Vec3 from, to, size;

        /// <summary>The authored refuge (a standing point within 6 m: the harness checks it); null when the row has none.</summary>
        public Vec3? refuge;

        /// <summary>Platform: the top centre now and one step ago (the view interpolates; the carry is the difference).</summary>
        public Vec3 pos, prev;

        /// <summary>Vent: the phase now and the share of the current phase gone (0..1).</summary>
        public Vent33Phase vent;
        public double ventShare;

        /// <summary>Vent: this burst already hurt the player.</summary>
        public bool hitThisBurst;

        public double CenterX => kind == Device33Kind.Platform ? pos.x : (x0 + x1) / 2;
        public double CenterZ => kind == Device33Kind.Platform ? pos.z : (z0 + z1) / 2;

        public bool Over(Vec3 feet) => feet.x >= x0 && feet.x <= x1 && feet.z >= z0 && feet.z <= z1;
    }

    /// <summary>The payload of fanLift / ventTell / ventBurst / ventBurn / platformRide (one shared instance, read it in the handler).</summary>
    public sealed class Device33Event
    {
        public int index;
        public string kind, room;
        public double damage, x, y, z;
    }

    public sealed class Devices33
    {
        public readonly Devices33Params p;
        public readonly List<Device33> devices = new List<Device33>();

        /// <summary>The moving platforms' live boxes (GameWorld layer "devices33").</summary>
        public readonly BoxLayer layer = new BoxLayer();

        readonly double dt;
        readonly List<(Vec3 min, Vec3 max)> boxes = new List<(Vec3, Vec3)>();
        readonly Device33Event ev = new Device33Event();

        /// <summary>Steps since the run started (every device's clock).</summary>
        public int steps;

        /// <summary>Counters (harness, reel): fan lifts (entries into a column), vent bursts, vent hits on the player, platform carry steps.</summary>
        public int fanLifts, ventBursts, ventHits, carrySteps;

        int platforms;
        bool inFanLast;

        public Devices33(Devices33Params p, double dt)
        {
            this.p = p;
            this.dt = dt;
        }

        public int Count => devices.Count;
        public int Platforms => platforms;

        /// <summary>The devices of a level JSON: every `campaign.rooms[i].devices33` row (none: no devices, older maps).</summary>
        public void Load(Dictionary<string, object> mapJson)
        {
            devices.Clear();
            platforms = 0;
            if (mapJson == null || !mapJson.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rooms)) return;
            foreach (object r in rooms)
            {
                if (!(r is Dictionary<string, object> room) || !room.TryGetValue("devices33", out object dl) || !(dl is List<object> list)) continue;
                string rid = room.TryGetValue("id", out object id) ? id as string : null;
                foreach (object o in list)
                {
                    Device33 d = Parse(PortData.Obj(o), rid);
                    if (d == null) continue;
                    if (d.kind == Device33Kind.Platform) platforms++;
                    devices.Add(d);
                }
            }
        }

        static Device33 Parse(Dictionary<string, object> row, string room)
        {
            string kind = row.TryGetValue("kind", out object k) ? k as string : null;
            var d = new Device33 { room = room, note = row.TryGetValue("note", out object n) ? n as string : null };
            if (row.TryGetValue("phase", out object ph)) d.phase = PortData.Num(ph);
            if (row.TryGetValue("refuge", out object rf)) d.refuge = V3(rf);
            switch (kind)
            {
                case "fan":
                case "vent":
                    d.kind = kind == "fan" ? Device33Kind.Fan : Device33Kind.Vent;
                    var r = PortData.Arr(row["rect"]);
                    double ax = PortData.Num(r[0]), az = PortData.Num(r[1]), bx = PortData.Num(r[2]), bz = PortData.Num(r[3]);
                    d.x0 = Math.Min(ax, bx); d.x1 = Math.Max(ax, bx); d.z0 = Math.Min(az, bz); d.z1 = Math.Max(az, bz);
                    d.y = PortData.Num(row["y"]);
                    if (row.TryGetValue("height", out object h)) d.height = PortData.Num(h);
                    return d;
                case "platform":
                    d.kind = Device33Kind.Platform;
                    d.from = V3(row["from"]);
                    d.to = V3(row["to"]);
                    d.size = V3(row["size"]);
                    d.pos = d.prev = d.from;
                    return d;
                default:
                    return null; // an unknown kind is skipped (a newer file in an older build)
            }
        }

        static Vec3 V3(object o)
        {
            var a = PortData.Arr(o);
            return new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]));
        }

        public void Reset()
        {
            steps = 0;
            fanLifts = ventBursts = ventHits = carrySteps = 0;
            inFanLast = false;
            foreach (Device33 d in devices)
            {
                d.vent = Vent33Phase.Idle;
                d.ventShare = 0;
                d.hitThisBurst = false;
                if (d.kind == Device33Kind.Platform) d.pos = d.prev = PlatformAt(d, 0);
            }
            RebuildLayer();
        }

        // ---- the platform's path ----------------------------------------------------------------------------------------------------------------

        /// <summary>Steps of one platform leg's travel (at least 1).</summary>
        public int TravelSteps(Device33 d)
        {
            double len = Math.Sqrt((d.to.x - d.from.x) * (d.to.x - d.from.x) + (d.to.y - d.from.y) * (d.to.y - d.from.y) + (d.to.z - d.from.z) * (d.to.z - d.from.z));
            return MovementMath.StepsFor(len / Math.Max(0.1, p.platformSpeed), dt);
        }

        /// <summary>Steps of the platform's whole cycle: dwell, travel, dwell, travel back.</summary>
        public int CycleSteps(Device33 d) => 2 * (MovementMath.StepsFor(p.platformDwellSeconds, dt) + TravelSteps(d));

        /// <summary>The platform's top centre at step `s` (dwell at from, ease to `to`, dwell, ease back; a smoothstep on each leg).</summary>
        public Vec3 PlatformAt(Device33 d, int s)
        {
            int dwell = MovementMath.StepsFor(p.platformDwellSeconds, dt), travel = TravelSteps(d), cycle = 2 * (dwell + travel);
            int t = (int)((s + (long)Math.Floor(d.phase * cycle)) % cycle);
            double u;
            if (t < dwell) u = 0;
            else if (t < dwell + travel) u = Ease((t - dwell) / (double)travel);
            else if (t < 2 * dwell + travel) u = 1;
            else u = 1 - Ease((t - 2 * dwell - travel) / (double)travel);
            return new Vec3(d.from.x + (d.to.x - d.from.x) * u, d.from.y + (d.to.y - d.from.y) * u, d.from.z + (d.to.z - d.from.z) * u);
        }

        static double Ease(double x) => x * x * (3 - 2 * x);

        /// <summary>Is the player standing on platform d's top at its position `top`?</summary>
        public bool Riding(Device33 d, Vec3 top, Vec3 feet, bool grounded)
        {
            if (!grounded) return false;
            double hx = d.size.x / 2 + p.platformRideMargin, hz = d.size.z / 2 + p.platformRideMargin;
            return Math.Abs(feet.x - top.x) <= hx && Math.Abs(feet.z - top.z) <= hz && feet.y >= top.y - p.platformRideHeight && feet.y <= top.y + p.platformRideHeight;
        }

        void RebuildLayer()
        {
            if (platforms == 0) return;
            boxes.Clear();
            foreach (Device33 d in devices)
            {
                if (d.kind != Device33Kind.Platform) continue;
                Vec3 c = d.pos;
                boxes.Add((new Vec3(c.x - d.size.x / 2, c.y - d.size.y, c.z - d.size.z / 2), new Vec3(c.x + d.size.x / 2, c.y, c.z + d.size.z / 2)));
            }
            layer.SetBoxes(boxes);
        }

        // ---- the vent's cycle -------------------------------------------------------------------------------------------------------------------

        /// <summary>A vent's phase and the share of it gone at step `s`.</summary>
        public (Vent33Phase phase, double share) VentAt(Device33 d, int s)
        {
            int idle = MovementMath.StepsFor(p.ventIdleSeconds, dt), warn = MovementMath.StepsFor(p.ventWarnSeconds, dt), burst = MovementMath.StepsFor(p.ventBurstSeconds, dt);
            int cycle = idle + warn + burst;
            int t = (int)((s + (long)Math.Floor(d.phase * cycle)) % cycle);
            if (t < idle) return (Vent33Phase.Idle, t / (double)idle);
            if (t < idle + warn) return (Vent33Phase.Warn, (t - idle) / (double)warn);
            return (Vent33Phase.Burst, (t - idle - warn) / (double)burst);
        }

        // ---- one step -------------------------------------------------------------------------------------------------------------------------

        /// <summary>One fixed step, right after the Mover and the jump pads (SurvivalSim.Devices33.cs N33DevicesMove): the platforms move (and carry a
        /// rider), the fans lift, the vents cycle and burn. `hurt(amount)` lands damage; `knock(vy)` launches the player (the jump pad's path).</summary>
        public void Move(Mover mover, bool playing, double maxHp, Func<double, bool> hurt, Action<double> knock, EventBus events)
        {
            steps++;
            bool fanNow = false;
            for (int i = 0; i < devices.Count; i++)
            {
                Device33 d = devices[i];
                switch (d.kind)
                {
                    case Device33Kind.Platform:
                    {
                        d.prev = d.pos;
                        d.pos = PlatformAt(d, steps);
                        if (Riding(d, d.prev, mover.position, mover.grounded))
                        {
                            Vec3 delta = new Vec3(d.pos.x - d.prev.x, d.pos.y - d.prev.y, d.pos.z - d.prev.z);
                            if (delta.x != 0 || delta.y != 0 || delta.z != 0)
                            {
                                mover.position = new Vec3(mover.position.x + delta.x, mover.position.y + delta.y, mover.position.z + delta.z);
                                carrySteps++;
                            }
                        }
                        break;
                    }
                    case Device33Kind.Fan:
                    {
                        Vec3 f = mover.position;
                        if (!d.Over(f) || f.y < d.y - 0.25 || f.y > d.y + d.height) break;
                        fanNow = true;
                        if (mover.grounded) knock(p.fanLift); // off the ground: the jump pad's path (ends a dash / slide / ride)
                        else if (mover.velocity.y < p.fanLift)
                            mover.velocity = new Vec3(mover.velocity.x, Math.Min(p.fanLift, mover.velocity.y + p.fanAccel * dt), mover.velocity.z);
                        if (!inFanLast)
                        {
                            fanLifts++;
                            Emit(events, "fanLift", i, d, f, 0);
                        }
                        break;
                    }
                    case Device33Kind.Vent:
                    {
                        var (ph, share) = VentAt(d, steps);
                        if (ph != d.vent)
                        {
                            if (ph == Vent33Phase.Warn) Emit(events, "ventTell", i, d, new Vec3(d.CenterX, d.y, d.CenterZ), 0);
                            if (ph == Vent33Phase.Burst)
                            {
                                ventBursts++;
                                d.hitThisBurst = false;
                                Emit(events, "ventBurst", i, d, new Vec3(d.CenterX, d.y, d.CenterZ), 0);
                            }
                        }
                        d.vent = ph;
                        d.ventShare = share;
                        if (ph != Vent33Phase.Burst || d.hitThisBurst || !playing) break;
                        Vec3 f = mover.position;
                        if (!d.Over(f) || f.y < d.y - 0.25 || f.y > d.y + p.ventBurstHeight) break;
                        double amount = maxHp * p.ventDamageFraction;
                        hurt(amount); // landed or not (a shield, the god mode): the burst knocks either way, as the lava
                        knock(p.ventKnockUp);
                        d.hitThisBurst = true;
                        ventHits++;
                        Emit(events, "ventBurn", i, d, f, amount);
                        break;
                    }
                }
            }
            inFanLast = fanNow;
            if (platforms > 0) RebuildLayer();
        }

        void Emit(EventBus events, string name, int index, Device33 d, Vec3 at, double damage)
        {
            ev.index = index;
            ev.kind = d.kind == Device33Kind.Fan ? "fan" : d.kind == Device33Kind.Vent ? "vent" : "platform";
            ev.room = d.room;
            ev.damage = damage;
            ev.x = at.x;
            ev.y = at.y;
            ev.z = at.z;
            events?.Emit(name, ev);
        }
    }
}
