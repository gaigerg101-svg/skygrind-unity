// NIGHT68 builder T, slice I3 ("the walk bot reads lava by height"; Logs/n68/slate.json I3; Research/2026-10-24-forge68/code-scout.md (c)):
// the walk bot's lava rule (F1Input, Harness.N46.Flow.cs) as static functions over the two lists (the level's top-level `lava` rects and its
// collision boxes), so a planted case can call the rule without a sim, and `n46flow --lava-cases`, the planted pair:
//   (A) the pure rule on a planted level (no sim): the feet's height and the point 1.5 m ahead -> burning (hold) or not, against what I3 expects;
//   (B) two scripted walks under the real F1Input on n52-foundry's a3 lava pit with a planted deck (in memory; the level file is never written):
//       B1 on a deck 2 m up across the pit (I3 expects a crossing, no hold), B2 on the floor beside the pit, off the deck (I3 expects the hold at
//       the rim and the feet never in the lava).
// Each line says what I3 expects and what the rule did; the last line counts the lines that match. Run on the t0 rule it is the RED.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static bool F1LavaDetail;

    /// <summary>The index of the lava rect over (x, z) in plan, or -1.</summary>
    static int F1LavaAt(List<double[]> lava, double x, double z)
    {
        for (int i = 0; i < lava.Count; i++) { double[] r = lava[i]; if (x >= r[0] && x <= r[2] && z >= r[1] && z <= r[3]) return i; }
        return -1;
    }

    /// <summary>A solid over (x, z) whose top a body with its feet at feetY can stand on: its top at -0.2 m or higher and no more than
    /// max(1 m, feetY + 1 m) (NIGHT68 I3: a deck or a column top over lava is walked from on it; the t0 rule read no height and took only tops
    /// up to 1 m, so a deck 2 m up read as lava from anywhere, the deck itself included: v2's stall). With the feet at or under the floor
    /// (feetY <= 0) the bound is 1 m, the t0 rule exactly; with the feet on a step under 1 m (0 < feetY <= 1) it rises to feetY + 1.</summary>
    static bool F1Bridged(List<double[]> boxes, double x, double z, double feetY)
    {
        double top = Math.Max(1.0, feetY + 1.0);
        foreach (double[] b in boxes) if (x >= b[0] && x <= b[3] && z >= b[2] && z <= b[5] && b[4] >= -0.2 && b[4] <= top) return true;
        return false;
    }

    /// <summary>(x, z) is lava with nothing to stand on over it, for a body whose feet are at feetY.</summary>
    static bool F1Burning(List<double[]> lava, List<double[]> boxes, double x, double z, double feetY) => F1LavaAt(lava, x, z) >= 0 && !F1Bridged(boxes, x, z, feetY);

    static int F1LavaCases(string[] args)
    {
        int match = 0, total = 0;
        F1Trace = F1Flag(args, "--trace"); // the scripted walks' feet every 0.25 s
        Console.WriteLine("n46flow --lava-cases: the walk bot's lava rule (F1Input) on planted cases; 'I3 expects' is the slice's rule (a deck over lava is walked, lava at foot height is refused)");
        // (A) the pure rule. Planted level: lava [0,0,10,10] with its bed (top -0.8, as n52-foundry a3), a deck (2,3,2)..(8,3.5,8), a floor
        // bridge (0,0.3,9.5)..(10,0.8,10); a second lava [20,0,30,10] with a deck 2 m up (22,1,2)..(28,2,8)
        var lava = new List<double[]> { new double[] { 0, 0, 10, 10 }, new double[] { 20, 0, 30, 10 } };
        var boxes = new List<double[]>
        {
            new double[] { 0, -1.8, 0, 10, -0.8, 10 },
            new double[] { 2, 3, 2, 8, 3.5, 8 },
            new double[] { 0, 0.3, 9.5, 10, 0.8, 10 },
            new double[] { 20, -1.8, 0, 30, -0.8, 10 },
            new double[] { 22, 1, 2, 28, 2, 8 },
        };
        void Pure(string name, double feetY, double fx, double fz, double ax, double az, bool expect)
        {
            bool got = F1Burning(lava, boxes, ax, az, feetY);
            total++; if (got == expect) match++;
            Console.WriteLine($"  A {name}: feet ({fx:0.0}, {feetY:0.0}, {fz:0.0}) -> 1.5 m ahead ({ax:0.0}, {az:0.0}): I3 expects {(expect ? "HOLD (lava)" : "walk")}, the rule: {(got ? "HOLD (lava)" : "walk")}: {(got == expect ? "OK" : "WRONG")}");
        }
        Pure("A1 on the 3.5 m deck, ahead on the deck", 3.5, 5, 5, 5, 6.5, false);
        Pure("A2 on the 2 m deck, ahead on the deck", 2.0, 25, 5, 25, 6.5, false);
        Pure("A3 on the 3.5 m deck, ahead past its edge (lava below)", 3.5, 5, 7.5, 5, 9, true);
        Pure("A4 on the floor at the rim, ahead into the lava", 0, -1, 5, 0.5, 5, true);
        Pure("A5 on the floor, ahead under the 3.5 m deck", 0, 3.5, 5, 5, 5, true);
        Pure("A6 on the floor, ahead onto the 0.8 m bridge", 0, 5, 11.25, 5, 9.75, false);
        Pure("A7 in the pit's bed (feet -0.8), ahead in the lava", -0.8, 5, 3.5, 5, 5, true);

        // (B) the scripted walks on n52-foundry a3 (lava [-5,-80,5,-71], its bed top -0.8, a wall x -2.6..-2 up to 10 m). Planted in memory: a
        // deck x -2..9, z -78..-73, y 1..2 (its top 2 m up, 2.8 m over the lava's surface). The walk pilot's own move is replaced by a straight
        // walk to a target (moveOverride); everything else is F1Input's (the hold, the chase after 2 s). 4 s of steps after the placement.
        string path = Path.GetFullPath(Path.Combine("Design", "levels", "n52-foundry.json"));
        Dictionary<string, object> Planted()
        {
            var j = PortData.Obj(PortData.LoadJson(path));
            PortData.Arr(PortData.Obj(j["collision"])["boxes"]).Add(new Dictionary<string, object>
            {
                ["min"] = new List<object> { -2.0, 1.0, -78.0 },
                ["max"] = new List<object> { 9.0, 2.0, -73.0 },
                ["kind"] = "ledge",
            });
            return j;
        }
        (int holds, int deckHolds, int slowOnDeck, int inLava, int cross, double minZ, Vec3 end, double hp, int alive, int steps) Walk(Vec3 start, double tx, double tz, uint seed)
        {
            GameConfig cfg = Config();
            var json = Planted();
            LevelDef level = LevelDef.From(json);
            int a = level.rooms.FindIndex(r => r.id == "a3");
            SurvivalMap map = SurvivalMap.FromExport(Planted());
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () => level.rooms[a].spawn;
            var pilot = new F1Input(sim, new Walk38Pilot(sim, ap), json);
            bool walking = false;
            pilot.moveOverride = active =>
            {
                Vec3 p = sim.mover.position;
                double yaw = walking ? Math.Atan2(-(tx - p.x), -(tz - p.z)) : 0;
                if (walking) pilot.SetLook(yaw, 0); // the camera's look is the move's frame (as F1Input's own exits and detours do)
                return new InputFrame { moveZ = walking ? 1 : 0, yaw = yaw, pitch = 0 };
            };
            sim.UseInput(pilot);
            sim.StartRun(seed, null, null);
            int stp = 0;
            bool placed = false;
            for (; stp < 120 * 60; stp++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Campaign24 c = sim.campaign;
                if (c == null || c.room < 0) continue;
                if (!placed) { c.SetDoorFree29(true); sim.Respawn(level.rooms[a].spawn, level.rooms[a].spawnYawDeg); placed = true; continue; }
                if (c.room == a) { c.SetDoorFree29(false); break; }
            }
            sim.Respawn(start, Math.Atan2(-(tx - start.x), -(tz - start.z)) * 180 / Math.PI);
            walking = true;
            int h0 = pilot.edgeHolds, d0 = pilot.deckHolds, slow = 0, inLava = 0, cross = -1, n = 0;
            double minZ = double.MaxValue, stallV = 0.25 * cfg.Num("ground.walkSpeed");
            for (int k = 0; k < 480 && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); k++, n++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Vec3 p = sim.mover.position;
                double hv = Math.Sqrt(sim.mover.velocity.x * sim.mover.velocity.x + sim.mover.velocity.z * sim.mover.velocity.z);
                bool onDeck = p.x >= -2 && p.x <= 9 && p.z >= -78 && p.z <= -73 && p.y > 1.5;
                if (onDeck && k >= 30 && hv < stallV) slow++;
                if (p.x >= -5 && p.x <= 5 && p.z >= -80 && p.z <= -71 && p.y < -0.2) inLava++;
                if (cross < 0 && onDeck && p.x > 5.5) cross = k;
                if (p.z < minZ) minZ = p.z;
                if (F1Trace && k % 30 == 0) Console.WriteLine($"      step {k}: feet ({p.x:0.00}, {p.y:0.00}, {p.z:0.00}) hv {hv:0.0} grounded {sim.mover.grounded} room {sim.campaign?.room} {sim.campaign?.phase} state {sim.mover.state} holds {pilot.edgeHolds - h0}");
            }
            ap.Detach();
            return (pilot.edgeHolds - h0, pilot.deckHolds - d0, slow, inLava, cross, minZ, sim.mover.position, sim.health.hp, sim.enemies.AliveCount, n);
        }
        var b1 = Walk(new Vec3(-1.2, 2.0, -75.5), 20, -75.5, 1);
        bool b1ok = b1.cross >= 0 && b1.deckHolds == 0 && b1.slowOnDeck == 0 && b1.inLava == 0;
        total++; if (b1ok) match++;
        Console.WriteLine($"  B1 a3 + a deck 2 m up across the lava, a straight walk east from (-1.2, 2.0, -75.5) over the lava: I3 expects a crossing with 0 holds; the rule: crossed the far rim {(b1.cross >= 0 ? $"yes at {b1.cross / 120.0:0.00} s" : "NO")}, edge holds {b1.holds} ({b1.deckHolds} with the feet above 1 m), slow steps on the deck (under 2 m/s, the first 0.25 s aside) {b1.slowOnDeck}, steps in the lava {b1.inLava}; end ({b1.end.x:0.0}, {b1.end.y:0.0}, {b1.end.z:0.0}) after {b1.steps / 120.0:0.00} s, hp {b1.hp:0}, alive {b1.alive}: {(b1ok ? "OK" : "WRONG")}");
        // B2: the pit's west part (x -5..-2.6, west of its wall), walked into from the north floor; a3's jump pad (2.5, 0, -70) is 6 m east
        var b2 = Walk(new Vec3(-3.8, 0.0, -64), -3.8, -95, 1);
        bool b2ok = b2.holds > 0 && b2.inLava == 0 && b2.minZ > -71;
        total++; if (b2ok) match++;
        Console.WriteLine($"  B2 a3, the floor north of the lava (no deck over this part), a straight walk south from (-3.8, 0.0, -64.0) into the lava: I3 expects the hold at the rim, the feet never in the lava; the rule: edge holds {b2.holds} ({b2.deckHolds} with the feet above 1 m), steps in the lava {b2.inLava}, the farthest south z {b2.minZ:0.00} (the rim z -71.0); end ({b2.end.x:0.0}, {b2.end.y:0.0}, {b2.end.z:0.0}), hp {b2.hp:0}, alive {b2.alive}: {(b2ok ? "OK" : "WRONG")}");
        Console.WriteLine($"lava-cases: {match} / {total} as I3 expects{(match == total ? " (GREEN)" : " (RED)")}");
        return match == total ? 0 : 1;
    }
}
