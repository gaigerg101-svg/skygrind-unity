// NIGHT33 builder R (design.n33.devices33, DESIGN33 card R): the arena devices in the sim (Core/Devices33.cs: updraft fan, hazard vent, moving
// platform). The lead's hooks (SurvivalSim.N33.cs): N33DevicesStartRun at the run's start (the map's devices, the platforms' collider layer),
// N33DevicesMove right after mover.Step and the jump pads (SurvivalSim.Run.cs Step: where a device moves the player, through the Mover's own velocity,
// position and Launch, the jump pad's path; never a Mover.cs edit). Off (the row off, --off33 / -skygrindOff33, a tape without the bit): no devices
// object, no layer, nothing runs: NIGHT32's game.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT33 R: the map's devices and their state (null with devices33 off).</summary>
        public Devices33 devices33;

        Func<double, bool> deviceHurt33;
        Action<double> deviceKnock33;

        partial void N33DevicesStartRun()
        {
            if (!n33.devices33)
            {
                if (devices33 != null) world?.SetLayer("devices33", null);
                devices33 = null;
                return;
            }
            devices33 ??= new Devices33(Devices33Params.From(cfg), step);
            devices33.Load(map.json);
            devices33.Reset();
            world?.SetLayer("devices33", devices33.Platforms > 0 ? devices33.layer : null, true);
            deviceHurt33 ??= amount => health.Damage(amount, "vent");
            deviceKnock33 ??= vy => mover.Launch(vy); // the jump pad's path: ends a dash / slide / ride, airborne; no Mover edit
        }

        /// <summary>NIGHT33 R (the harness's arenas33 and the arenas33 reel only): turn the devices on (or off) for THIS run whatever the design row says,
        /// and restart them (their clock from 0). Never called by the game's own flow.</summary>
        public void Devices33Force(bool on)
        {
            n33.devices33 = on;
            N33DevicesStartRun();
        }

        partial void N33DevicesMove(double dt)
        {
            if (devices33 == null || devices33.Count == 0) return;
            devices33.Move(mover, flow == Flow.Playing, health.max, deviceHurt33, deviceKnock33, events);
        }
    }
}
