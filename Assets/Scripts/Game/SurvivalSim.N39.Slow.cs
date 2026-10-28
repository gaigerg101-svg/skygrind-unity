// NIGHT39 builder F1 (design.n39.slow39; NIGHT39.md card F1; GG 2026-10-13: "when you're wall riding and zooming in with a weapon the game needs
// to slow down dramatically ... it needs to be extremely slow every time"; "if you grapple onto an enemy the game should slow down with the same
// effect"; the gauntlet's parry "emphasized more and slowed down"). The slow motion's two sim READS and its pacing math, Unity-free so the harness
// runs the same code the loop runs (SurvivalGame.SlowAim27.cs calls it; `n39slow` in Tools/SimHarness/Harness.N39.Slow.cs measures it).
// LOOP PACING ONLY, exactly as slowAim27: the share it returns scales the frame's dt into the accumulator, so the sim still steps at 120 Hz (fewer
// steps per real second), the tape and the fingerprint never see it, Time.timeScale is never touched, and look input keeps the real frame delta
// (ReaderInput.Poll(frameDt): aim runs in real time, Max Payne's rule, research/feel-web.md section 1). Nothing here writes the sim: the two
// properties below only read the mover, the loadout and the ARCLINE. So no N39Blocks bit; --off39 / -skygrindOff39 (N39Blocks.ForceOff) and the
// row off give slowAim27's NIGHT38 behaviour bit for bit.
// - The ride: riding a wall AND zoomed (slowAim27's own test, every ride row: wallride35 / wallgrip36 / ride38 all ride in MoveState.WallRide).
// - The latch: the hook bitten into an enemy (rope Attached on an enemy zip; the whip33 pull and the plain ARCLINE zip alike). A yank (no rope) is not
//   a latch. While latched the world slows the same way as the ride.
// - The depth eases linearly in over easeInS and out over easeOutS (real time), and the share follows a smoothstep of the depth (ease in AND out:
//   no snap at either end): share = 1 - (1 - scale) x smooth(depth).
// - The parry window: a player parry ("parry" or "parry36") arms it; it starts on the first frame the world runs again (after the impact layer's
//   freeze, design.ultra.parryHitstop 0.25 s, so the freeze is unchanged and the slow comes after it), holds parryScale for parryHoldS, then eases
//   back to real time over parryOutS (smoothstep). With HIT FREEZE off it starts at once.
// The slower of every slow-down wins (Math.Min with the share it is given); a freeze (share 0) stays 0.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>design.n39.slow39's numbers (Unity-only pacing: the web had no slow motion of this kind).</summary>
    public sealed class Slow39Rules
    {
        public bool on, latch, parry;
        public double scale, easeInS, easeOutS, parryScale, parryHoldS, parryOutS;

        /// <summary>The row's numbers; `on` false when the row is off or NIGHT39 is forced off (then nothing else is read).</summary>
        public static Slow39Rules From(GameConfig cfg)
        {
            var r = new Slow39Rules { on = !N39Blocks.ForceOff && cfg.Design("n39.slow39.enabled") };
            if (!r.on) return r;
            r.scale = cfg.Num("design.n39.slow39.scale");
            r.easeInS = cfg.Num("design.n39.slow39.easeInS");
            r.easeOutS = cfg.Num("design.n39.slow39.easeOutS");
            r.latch = cfg.Bool("design.n39.slow39.latch");
            r.parry = cfg.Bool("design.n39.slow39.parry");
            r.parryScale = cfg.Num("design.n39.slow39.parryScale");
            r.parryHoldS = cfg.Num("design.n39.slow39.parryHoldS");
            r.parryOutS = cfg.Num("design.n39.slow39.parryOutS");
            return r;
        }
    }

    /// <summary>One game's slow39 state (the loop's): the ride / latch depth and the parry window.</summary>
    public sealed class Slow39
    {
        public readonly Slow39Rules rules;

        /// <summary>The ride / latch depth, 0 .. 1, eased linearly on real time.</summary>
        public double depth;

        /// <summary>A parry happened and its window waits for the world to run again (the freeze first).</summary>
        public bool parryArmed;

        /// <summary>Real seconds into the parry window; negative: no window.</summary>
        public double parryT = -1;

        /// <summary>Parry windows started (the reels' and the harness's count).</summary>
        public int parryWindows;

        /// <summary>This row's OWN share on the last frame (1 = none), before the slower of the others is taken: the reel's measure, so THE
        /// LANTERN's floor (design.n29.boss1.slowScale), the bend or the impact layer's slow-mo are not counted as this row's.</summary>
        public double own = 1;

        /// <summary>The parry window is running (real time inside it).</summary>
        public bool ParryWindow => parryT >= 0;

        public Slow39(Slow39Rules r) => rules = r;

        public static double Smooth(double x)
        {
            x = x < 0 ? 0 : x > 1 ? 1 : x;
            return x * x * (3 - 2 * x);
        }

        /// <summary>The parry window's share at real time t into it (1 outside).</summary>
        public double ParryShare(double t)
        {
            if (t < 0) return 1;
            if (t < rules.parryHoldS) return rules.parryScale;
            double u = rules.parryOutS <= 0 ? 1 : (t - rules.parryHoldS) / rules.parryOutS;
            if (u >= 1) return 1;
            return rules.parryScale + (1 - rules.parryScale) * Smooth(u);
        }

        /// <summary>A player parry: arm the window (it starts when the world runs again).</summary>
        public void OnParry()
        {
            if (rules.on && rules.parry) parryArmed = true;
        }

        /// <summary>One frame: `target` = allowed and (riding + zoomed, or latched); `share` = the world's share so far this frame (0 frozen).
        /// Returns the slower of `share` and this row's; `cue01` is the screen cue's depth (0 .. 1).</summary>
        public double Frame(double frameDt, double share, bool target, bool allowed, out double cue01)
        {
            depth = target ? Math.Min(1, depth + frameDt / Math.Max(1e-4, rules.easeInS)) : Math.Max(0, depth - frameDt / Math.Max(1e-4, rules.easeOutS));
            double ride = 1 - (1 - rules.scale) * Smooth(depth);
            if (!allowed) { parryArmed = false; parryT = -1; }
            if (parryArmed && share > 0)
            {
                parryArmed = false;
                parryT = 0;
                parryWindows++;
            }
            double parry = 1;
            if (parryT >= 0)
            {
                parry = ParryShare(parryT);
                if (share > 0) parryT += frameDt; // the window runs on real time while the world runs (a later freeze holds it)
                if (parryT >= rules.parryHoldS + rules.parryOutS) parryT = -1;
            }
            double mine = Math.Min(ride, parry);
            own = mine;
            double rideCue = rules.scale < 1 ? (1 - ride) / (1 - rules.scale) : 0;
            double parryCue = rules.parryScale < 1 ? (1 - parry) / (1 - rules.parryScale) : 0;
            cue01 = Math.Max(rideCue, parryCue);
            if (share <= 0) return share;
            return Math.Min(share, mine);
        }

        /// <summary>The way to TITLE: nothing carried into the next run.</summary>
        public void Halt()
        {
            depth = 0;
            parryArmed = false;
            parryT = -1;
            own = 1;
        }
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT39 F1 (read only): riding a wall AND zoomed (slowAim27's test).</summary>
        public bool Slow39RideAim => mover.state == MoveState.WallRide && loadout.adsActive;

        /// <summary>NIGHT39 F1 (read only): the hook bitten into an enemy (the rope Attached on an enemy zip; whip33 or the plain ARCLINE zip).</summary>
        public bool Slow39Latched => arcline21 != null && mover.rope.phase == RopePhase.Attached && mover.rope.zip && mover.zipAnchor != null && arcline21.ZipEnemy != null;
    }
}
