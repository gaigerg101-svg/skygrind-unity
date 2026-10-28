// NIGHT34 builder M (NIGHT34.md M1 latency34, a measure, no row): `latency34`, called from Harness.N34.Flow.cs. The full headless sim on the
// survival map (the design config as it is, NIGHT34 rows off but flow34 forced ON / OFF by SurvivalSim.Force34Flow), the player standing at
// the spawn (60 settle steps), then ONE press on step 0 and the steps to its first sim change, read from the sim's own events:
//   jump    MoverEvent Jump                  dash   MoverEvent Dash           fire   "fire"            punch  "punch"
//   swap    "weaponSwap" (the swap starts) and "weaponRaise" (the new gun is up: the swap timeline's lower point)
//   grapple MoverEvent RopeFire (the hook leaves) and RopeAttach (it bites; a point 10 m ahead and 4 m up)
//   whip    RopeFire on a husk 7 m ahead (the ARCLINE's pick) and "whip33Latch" (a husk is a pull kind: it is YANKED first; the latch only
//           comes while the yank's 15 s cooldown runs, so "none" here is the design, not a delay)
// 0 = the press's own step. Designed: 0 for every first change (the web's verbs act on the press step); the raise and the bites follow
// their own timelines (the swap's lower point; the hook's flight time).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static int Latency34Command(string[] args)
    {
        Console.WriteLine("latency34: steps from ONE press (step 0) to its first sim change, 120 Hz (0 = the press's own step); the full headless sim, survival map");
        var verbs = new[] { "jump", "dash", "fire", "swap", "grapple", "whip", "punch" };
        var rows = new Dictionary<string, string[]>();
        int bad = 0;
        foreach (bool flow in new[] { false, true })
        {
            SurvivalSim.Force34Flow = flow;
            try
            {
                foreach (string v in verbs)
                {
                    string r = Latency34Verb(v, out bool late);
                    if (!rows.ContainsKey(v)) rows[v] = new string[2];
                    rows[v][flow ? 1 : 0] = r;
                    if (late) bad++;
                }
            }
            finally { SurvivalSim.Force34Flow = null; }
        }
        Console.WriteLine($"  {"verb",-8} {"flow34 OFF",-44} {"flow34 ON",-44}");
        foreach (string v in verbs) Console.WriteLine($"  {v,-8} {rows[v][0],-44} {rows[v][1],-44}");
        Console.WriteLine($"latency34 {(bad == 0 ? "every verb's first change on its press step" : bad + " verb run(s) later than step 0 (logged)")}");
        return 0;
    }

    static string Latency34Verb(string verb, out bool late)
    {
        late = false;
        SurvivalSim sim = SurvivalSim.Headless(Config(), Map());
        sim.noLab = sim.noRecords = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(1, null, null);
        sim.husks.frozen = true;
        sim.mover.invulnerable = true;
        sim.mover.ropeEnabled = true; // the ARCLINE (a gadget in Survival, not owned at start): on, so the grapple and the whip can be timed
        var si = (ScriptedInput)sim.input;
        if (!sim.loadout.owned.Contains("shotgun")) sim.loadout.AddWeapon("shotgun");
        double yaw = sim.input.Yaw;
        for (int k = 0; k < 60; k++) { si.SetLook(yaw, 0); si.Set(new InputFrame { yaw = yaw }, new CombatInput { weapon = -1 }); sim.Step(); }
        int step = -1;
        var first = new Dictionary<string, int>();
        void Mark(string n) { if (!first.ContainsKey(n)) first[n] = step; }
        sim.mover.Events += e =>
        {
            if (e.type == MoverEventType.Jump || e.type == MoverEventType.DoubleJump) Mark("jump");
            if (e.type == MoverEventType.Dash) Mark("dash");
            if (e.type == MoverEventType.RopeFire) Mark("ropeFire");
            if (e.type == MoverEventType.RopeAttach) Mark("ropeAttach");
        };
        sim.events.On<object>("fire", _ => Mark("fire"));
        sim.events.On<object>("weaponSwap", _ => Mark("swap"));
        sim.events.On<object>("weaponRaise", _ => Mark("raise"));
        sim.events.On<MeleeEvent>("punch", _ => Mark("punch"));
        sim.events.On<object>("whip33Latch", _ => Mark("latch"));
        Vec3 eye = sim.Eye;
        Husk h = null;
        if (verb == "whip")
        {
            Vec3 fwd = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw));
            h = sim.husks.Spawn("husk", new Vec3(sim.mover.position.x + fwd.x * 7, sim.mover.position.y, sim.mover.position.z + fwd.z * 7), sim.mover.position);
            if (h != null) h.hp = h.maxHp = 1e9;
            for (int k = 0; k < 30; k++) { var (yy, pp) = ArclineLook(sim.Eye, h.AimPoint); si.SetLook(yy, pp); si.Set(new InputFrame { yaw = yy, pitch = pp }, new CombatInput { weapon = -1 }); sim.Step(); }
        }
        for (step = 0; step < 120; step++)
        {
            bool press = step == 0, held = step < 60;
            double ly = yaw, lp = 0;
            if (h != null) (ly, lp) = ArclineLook(sim.Eye, h.AimPoint);
            var mv = new InputFrame { yaw = ly, pitch = lp, moveZ = verb == "dash" ? 1 : 0 };
            var cb = new CombatInput { weapon = -1 };
            switch (verb)
            {
                case "jump": mv.jump = new Button { pressed = press, held = held }; break;
                case "dash": mv.dash = new Button { pressed = press, held = press }; break;
                case "fire": cb.fire = new Button { pressed = press, held = press }; break;
                case "swap": cb.weapon = press ? sim.loadout.owned.IndexOf(sim.loadout.Weapon == "revolver" ? "shotgun" : "revolver") : -1; break;
                case "punch": cb.punch = new Button { pressed = press, held = press }; break;
                case "grapple":
                    mv.grapple = new Button { pressed = press, held = held };
                    if (press) mv.grappleTarget = new GrappleTarget { point = new Vec3(eye.x - Math.Sin(yaw) * 10, eye.y + 4, eye.z - Math.Cos(yaw) * 10) };
                    break;
                case "whip": mv.grapple = new Button { pressed = press, held = press }; break; // a TAP: the pull latch
            }
            si.SetLook(ly, lp);
            si.Set(mv, cb);
            sim.hitstopLeft = 0;
            sim.Step();
        }
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "--verbose") >= 0)
            Console.WriteLine($"    [{verb}] owned {string.Join(",", sim.loadout.owned)}, in hand {sim.loadout.Weapon}, rope {sim.mover.rope.phase}, ropeEnabled {sim.mover.ropeEnabled}, flow {sim.flow}, marks {string.Join(" ", first)}");
        string S(string k) => first.TryGetValue(k, out int s) ? s.ToString() : "none";
        string main = verb == "grapple" || verb == "whip" ? "ropeFire" : verb;
        late = !first.ContainsKey(main) || first[main] > 0;
        return verb switch
        {
            "swap" => $"starts {S("swap")}, the new gun up {S("raise")}",
            "grapple" => $"hook fired {S("ropeFire")}, bites {S("ropeAttach")}",
            "whip" => $"hook fired {S("ropeFire")}, latches the husk {S("latch")}",
            _ => S(main),
        };
    }
}
