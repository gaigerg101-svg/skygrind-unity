// NIGHT15 (the lead, scratch-level debug kept beside the verb check): `verbtrace <map.json> x z yaw jumpStep steps` prints
// the Mover's state, position and velocity every 6 steps of one run (forward held, a jump pressed at jumpStep).
using System;
using System.Globalization;
using System.IO;
using Skygrind.Core;

public static partial class Harness
{
    static int VerbTrace(string[] args)
    {
        double P(int i) => double.Parse(args[i], CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        MovementConfig mc = cfg.movement;
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(args[1]))));
        ICollisionWorld world = map.BuildOctree();
        double x = P(2), z = P(3), yaw = P(4);
        int jumpStep = (int)P(5), steps = (int)P(6);
        var m = new Mover(mc, world);
        m.Reset(x, 0, z);
        double dt = 1 / mc.loop.simHz;
        for (int i = 0; i < 30; i++) m.Step(new InputFrame { yaw = yaw }, dt);
        for (int i = 0; i < steps; i++)
        {
            var f = new InputFrame { yaw = yaw, moveZ = 1, jump = new Button { held = i >= jumpStep && i < jumpStep + 48, pressed = i == jumpStep } };
            m.Step(f, dt);
            if (i % 6 == 0 || m.state == MoveState.WallRide)
                Console.WriteLine($"verbtrace {i} {m.state} grounded {m.grounded} pos ({F(m.position.x)}, {F(m.position.y)}, {F(m.position.z)}) vel ({F(m.velocity.x)}, {F(m.velocity.y)}, {F(m.velocity.z)}) speed {F(m.Speed)}");
        }
        return 0;
    }
}
