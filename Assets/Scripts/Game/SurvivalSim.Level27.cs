// NIGHT27 (builder A): hazardFloor27 - the sim side of a room's floor HAZARD strip (`hazard.rect` in the level plan, RoomDef.hasHazard).
// The lead's call sites (SurvivalSim.N27.cs) are N27LevelStartRun / N27LevelStep; this file implements them. Only with the block on
// (n27.hazardFloor27, design.n27.hazardFloor27.enabled) AND a campaign run in a room that has a strip; otherwise nothing runs (off = NIGHT26 bit for bit).
// The cycle runs in WHOLE STEPS from the step the player enters the room: quiet for firstStartSeconds, then every spacingSeconds
// a telegraph (telegraphSeconds, the strip glows red-orange, no damage) and then LIVE (liveSeconds): a player standing on the strip
// (feet inside the rect, within standHeight of the floor: a wall rider above it is safe) takes damagePerSecond, in ticks of tickSeconds.
// Mulberry32 is not needed (the cycle is fixed). All state resets at run start and at room entry.
// Events (payload Hazard27Event): "hazardTelegraph27", "hazardLive27", "hazardOff27" (the view's strip colour); "hazardHurt27" per damage tick.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class Hazard27Event
    {
        public int room;
        public double x0, z0, x1, z1;
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>0 = strip quiet (or none), 1 = telegraph, 2 = live (the view colours the strip by it).</summary>
        public int hazard27State;
        /// <summary>The strip's rect [x0, z0, x1, z1] of the room the player is in (valid while hazard27Room >= 0).</summary>
        public double hazard27X0, hazard27Z0, hazard27X1, hazard27Z1;
        public int hazard27Room = -1;
        /// <summary>Damage dealt by the strip this run (the harness line's number).</summary>
        public double hazard27Damage;
        int hazard27Steps, hazard27Tick;
        int hStart, hTele, hLive, hSpacing, hTickSteps;
        double hDps, hStand;

        partial void N27LevelStartRun()
        {
            hazard27State = 0;
            hazard27Room = -1;
            hazard27Damage = 0;
            hazard27Steps = hazard27Tick = 0;
            if (!n27.hazardFloor27) return;
            hStart = MovementMath.StepsFor(cfg.Num("design.n27.hazardFloor27.firstStartSeconds"), step);
            hTele = MovementMath.StepsFor(cfg.Num("design.n27.hazardFloor27.telegraphSeconds"), step);
            hLive = MovementMath.StepsFor(cfg.Num("design.n27.hazardFloor27.liveSeconds"), step);
            hSpacing = MovementMath.StepsFor(cfg.Num("design.n27.hazardFloor27.spacingSeconds"), step);
            hTickSteps = MovementMath.StepsFor(cfg.Num("design.n27.hazardFloor27.tickSeconds"), step);
            hDps = cfg.Num("design.n27.hazardFloor27.damagePerSecond");
            hStand = cfg.Num("design.n27.hazardFloor27.standHeight");
        }

        partial void N27LevelStep(double dt)
        {
            if (!n27.hazardFloor27 || !campaignOn || campaign == null || campaignLevel == null) return;
            int ri = campaign.room;
            if (ri < 0 || ri >= campaignLevel.rooms.Count || !campaignLevel.rooms[ri].hasHazard || campaign.phase != Campaign24.Phase.Fight || !run.Live || health.Dead)
            {
                if (hazard27State != 0) { hazard27State = 0; events.Emit("hazardOff27", new Hazard27Event { room = hazard27Room }); }
                if (ri != hazard27Room) hazard27Room = -1;
                return;
            }
            RoomDef rd = campaignLevel.rooms[ri];
            if (ri != hazard27Room)
            {
                // entering a room with a strip (a death restarts the room: the cycle starts over, like the room)
                hazard27Room = ri;
                hazard27Steps = 0;
                hazard27Tick = 0;
                hazard27X0 = rd.hx0; hazard27Z0 = rd.hz0; hazard27X1 = rd.hx1; hazard27Z1 = rd.hz1;
            }
            int t = hazard27Steps++ - hStart;
            int state = 0;
            if (t >= 0)
            {
                int c = t % hSpacing;
                state = c < hTele ? 1 : c < hTele + hLive ? 2 : 0;
            }
            if (state != hazard27State)
            {
                hazard27State = state;
                string name = state == 1 ? "hazardTelegraph27" : state == 2 ? "hazardLive27" : "hazardOff27";
                events.Emit(name, new Hazard27Event { room = ri, x0 = rd.hx0, z0 = rd.hz0, x1 = rd.hx1, z1 = rd.hz1 });
                hazard27Tick = 0;
            }
            if (state != 2 || rd.hazardSafe) return; // a safe-form strip (the tutorial's) shows its telegraph and live colour and never hurts
            Vec3 f = mover.position;
            bool on = f.x >= rd.hx0 && f.x <= rd.hx1 && f.z >= rd.hz0 && f.z <= rd.hz1 && f.y <= hStand;
            if (!on) return;
            if (++hazard27Tick < hTickSteps) return;
            hazard27Tick = 0;
            double amount = hDps * hTickSteps * step;
            if (health.Damage(amount, "hazard", f))
            {
                hazard27Damage += amount;
                events.Emit("hazardHurt27", new Hazard27Event { room = ri, x0 = rd.hx0, z0 = rd.hz0, x1 = rd.hx1, z1 = rd.hz1 });
            }
        }
    }
}
