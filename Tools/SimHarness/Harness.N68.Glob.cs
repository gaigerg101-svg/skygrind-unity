// NIGHT68 builder T, g7 (the lead: "narrow the parry refusal to the parry, not every swing near a glob"; R's g7 is melee only, the gauntlet
// alone in hand): would a gauntlet press now send a spitter's glob back, by the swing itself or by the orb buffer it opens? The game's own
// catch tests, read here without changing anything (Spitter.cs SpitterAcid.TryReflect; Parry43.cs's three tests for globs; Parry42.cs's buffer):
//   the point test   the glob within orb.reflectRange of the eye (TryReflect's own test);
//   parry43 test 1   the glob's surface within reachMult x orb.reflectRange of the body's axis (the eye down to the capsule's lower sphere);
//   parry43 test 2   the glob's surface inside the catch sphere (catchRadius43, catchAhead43 ahead of the eye along the aim);
//   parry43 test 3   the glob, flying on as it flies, touching the player's capsule within windowSteps43;
//   the buffer       a swing that sent nothing back keeps catching for parry43's bufferS: tests 1-3 are run on the glob's path over those steps.
// The glob flies on in a straight line here (the game's glob falls under gravity: over 0.4 s at most that is under 1 m), and every distance
// gets a 0.5 m margin, so the refusal errs toward refusing (no leak). Probe numbers, never gameplay.
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    const double V68GlobMarginM = 0.5; // m, the catch tests' margin (a probe number: the refusal must not leak)
    static bool V68TraceLeak; // NIGHT68 g7: skip v2's --trace-leak (print only)

    /// <summary>True when a press now, aimed along (yaw, pitch), would reflect a live glob (now or within the buffer's steps). `why` names the
    /// test that holds.</summary>
    static bool V68GlobCatchable(SurvivalSim sim, double yaw, double pitch, out string why)
    {
        why = null;
        SpitterAcid acid = sim.husks.acid;
        if (acid == null) return false;
        GameConfig cfg = sim.cfg;
        DroneOrbs o = acid.orbs;
        bool p43 = o != null && o.parry43;
        double reflect = cfg.Num("orb.reflectRange"), m = V68GlobMarginM;
        double r = cfg.Num("spitter.globRadius");
        double reach42 = p43 && cfg.Has("design.n43.parry43.reachMult") ? reflect * cfg.Num("design.n43.parry43.reachMult") : 0;
        int buffer = p43 && cfg.Has("design.n43.parry43.bufferS") ? MovementMath.StepsFor(cfg.Num("design.n43.parry43.bufferS"), sim.step) : 0;
        int window = p43 ? o.windowSteps43 : 0;
        double capR = cfg.movement.body.capsuleRadius, capH = cfg.movement.body.capsuleHeight;
        double grav = MovementMath.GravityFor(cfg.movement.jump.height, cfg.movement.jump.apexTime); // the globs' gravity (SpitterAcid's own)
        Vec3 eye = sim.Eye, feet = sim.mover.position;
        double drop = capH - 2 * capR;
        double ax = -Math.Sin(yaw) * Math.Cos(pitch), ay = Math.Sin(pitch), az = -Math.Cos(yaw) * Math.Cos(pitch);
        double cx = eye.x + ax * (p43 ? o.catchAhead43 : 0), cy = eye.y + ay * (p43 ? o.catchAhead43 : 0), cz = eye.z + az * (p43 ? o.catchAhead43 : 0);
        foreach (SpitterAcid.Glob g in acid.globs)
        {
            if (!g.alive || g.reflected) continue;
            Vec3 pos = g.position, vel = g.velocity;
            for (int k = 0; k <= buffer + window; k++)
            {
                if (k <= buffer)
                {
                    if (Vec3.DistanceTo(pos, eye) <= reflect + m) { why = "the point test"; return true; }
                    if (reach42 > 0)
                    {
                        // the distance from the glob to the body's axis (eye.y down to eye.y - drop), less its radius
                        double yy = Math.Max(eye.y - drop, Math.Min(eye.y, pos.y));
                        if (JsMath.Hypot(pos.x - eye.x, pos.y - yy, pos.z - eye.z) - r <= reach42 + m) { why = "parry43 test 1 (the body's reach)"; return true; }
                    }
                    if (p43 && JsMath.Hypot(pos.x - cx, pos.y - cy, pos.z - cz) - r <= o.catchRadius43 + m) { why = "parry43 test 2 (the catch sphere ahead)"; return true; }
                }
                if (p43)
                {
                    double fy = feet.y, dy = pos.y - Math.Max(fy + 0.3, Math.Min(fy + 1.5, pos.y));
                    if (JsMath.Hypot(pos.x - eye.x, dy, pos.z - eye.z) <= r + capR + m) { why = "parry43 test 3 (it touches the body within the window)"; return true; }
                }
                vel.y -= grav * sim.step; // NIGHT68 g7 leak fix: the glob falls as the game's does (SpitterAcid: velocity.y -= gravity x dt)
                pos = pos + vel * sim.step;
            }
        }
        // NIGHT68 g7 leak fix (Logs/n68/g7leak: seeds 8 and 12 at r2, a swing passed 15 steps before parry42's buffer caught a glob): a glob
        // born INSIDE the buffer is invisible to the tests above. A spitter in its spit tell whose volley leaves within the buffer, close enough
        // that the glob (at most spitter.globSpeed) can enter a catch volume before the buffer ends or touch the body within the window after
        // it, would be caught: the press is refused. Spitters farther away, or whose tell ends after the buffer, do not stop the swing.
        if (buffer > 0)
        {
            double speed = cfg.Num("spitter.globSpeed");
            double reachMax = Math.Max(Math.Max(reach42, p43 ? o.catchAhead43 + o.catchRadius43 : 0), Math.Max(reflect, capR)) + r + m;
            foreach (Enemy e in sim.enemies.all)
            {
                if (!(e is Spitter s) || !s.alive || s.state != "tell" || s.stateSteps > buffer + 1) continue;
                double rem = Math.Max(0, buffer - s.stateSteps) * sim.step, win = window * sim.step;
                double d = Vec3.DistanceTo(s.AimPoint, eye), dMax = reachMax + speed * (rem + win);
                if (d <= dMax) { why = $"a spitter about to spit ({s.kind} #{s.id}: its tell ends in {s.stateSteps} steps, {d.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} m off, inside {dMax.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} m)"; return true; }
            }
        }
        return false;
    }
}
