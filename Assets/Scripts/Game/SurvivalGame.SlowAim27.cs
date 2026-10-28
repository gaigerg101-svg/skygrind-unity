// NIGHT27 slowAim27 (the lead's loop line; builder V ships the screen cue): while the player is WALL-RIDING and AIMING (the sim's
// zoom, loadout.adsActive) the world's time share eases down to design.n27.slowAim27.scale (0.75), in over easeInS and out over
// easeOutS (GG 2026-10-09: "if you aim a weapon while riding [a wall] the game kind of temporarily slows down a little bit").
// Visual pacing only, exactly as the debut slow-down and the impact layer: a smaller dt into the accumulator, so the sim still
// steps at 120 Hz (fewer steps per real second), the tape and the fingerprint never see it, and Time.timeScale is never touched.
// Look input reads the real frame delta (it is not scaled here). Hitstop wins (a share of 0 stays 0); the slower of this and the
// other slow-downs wins. SlowAim01 (0..1) is what the cue reads. Off with SLOW-MO off or REDUCED MOTION (the impact layer's slowMoOn).
// NIGHT39 F1 (design.n39.slow39; builder F1 owns this file tonight): with the row on, the same line runs Slow39 (SurvivalSim.N39.Slow.cs)
// instead: "extremely slow" (scale 0.3) with a smoothstep ease, the whip latch on an enemy as a second condition on the same target, and
// the parry's slow window after its freeze (a player "parry" / "parry36" arms it). Still loop pacing only (no sim write, no tape bit).
// The row off (or --off39 / -skygrindOff39): slowAim27 exactly as NIGHT38.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        double slowAim01;
        int slowAimOn = -1; // -1 unread, 0 off, 1 on (design.n27.slowAim27.enabled; --off27 turns it off)
        double slowAimScale, slowAimIn, slowAimOut;
        Slow39 slow39; // NIGHT39 F1: null with design.n39.slow39 off (or NIGHT39 forced off)

        /// <summary>The slow-mo's depth this frame, 0 (real time) .. 1 (the full slow): the cue's one input.</summary>
        public double SlowAim01 => slowAim01;

        /// <summary>NIGHT39 F1: this line's OWN world share on the last frame (1 = none): slowAim27's or slow39's, before the slower of the others
        /// (THE LANTERN's floor, the bend, the impact layer) is taken. The slowworld reel judges the row on it, not on WorldScale.</summary>
        public double SlowAimShare { get; private set; } = 1;

        /// <summary>NIGHT39 F1: the slow39 state (null with the row off); the reels and the parry view read it.</summary>
        public Slow39 Slow39State => slow39;

        /// <summary>The loop's line (ImpactBeginFrame): the world's share with the wall-ride aim slow-down; the slower wins.</summary>
        double SlowAimFrame(double frameDt, double share)
        {
            if (slowAimOn < 0)
            {
                slowAimOn = !N27Blocks.ForceOff && cfg.Design("n27.slowAim27.enabled") ? 1 : 0;
                if (slowAimOn == 1)
                {
                    slowAimScale = cfg.Num("design.n27.slowAim27.scale");
                    slowAimIn = cfg.Num("design.n27.slowAim27.easeInS");
                    slowAimOut = cfg.Num("design.n27.slowAim27.easeOutS");
                }
                Slow39Rules r39 = Slow39Rules.From(cfg);
                if (r39.on)
                {
                    slow39 = new Slow39(r39);
                    if (r39.parry)
                    {
                        events.On<HitEvent>("parry", Slow39Parry);
                        events.On<HitEvent>(Parry36.Event, Slow39Parry);
                    }
                }
            }
            // NIGHT29 item 12 (D-7): while THE LANTERN's floor is live the world slows to design.n29.boss1.slowScale (the sim's own eased value); the slower wins
            if (flow == Flow.Playing && impactFrames != null && impactFrames.rules.slowMoOn) share = Math.Min(share, sim.Lantern29SlowScale);
            // NIGHT34 B (design.n34.bend34, the global path): the grapple BEND's world share (the sim's own eased value, 1 outside a bend); the slower wins
            if (flow == Flow.Playing && impactFrames != null && impactFrames.rules.slowMoOn) share = Math.Min(share, sim.Bend34Share);
            if (slow39 != null)
            {
                // NIGHT39 F1: riding + zoomed, or latched on an enemy (design.n39.slow39.latch); the parry window; the cue's depth from the same values
                bool allowed39 = flow == Flow.Playing && impactFrames != null && impactFrames.rules.slowMoOn;
                bool target39 = allowed39 && (sim.Slow39RideAim || (slow39.rules.latch && sim.Slow39Latched));
                share = slow39.Frame(frameDt, share, target39, allowed39, out double cue39);
                slowAim01 = cue39;
                SlowAimShare = slow39.own;
                return share;
            }
            SlowAimShare = 1;
            if (slowAimOn == 0) return share;
            bool allowed = flow == Flow.Playing && impactFrames != null && impactFrames.rules.slowMoOn;
            bool target = allowed && sim.mover.state == MoveState.WallRide && sim.loadout.adsActive;
            slowAim01 = target ? Math.Min(1, slowAim01 + frameDt / slowAimIn) : Math.Max(0, slowAim01 - frameDt / slowAimOut);
            if (slowAim01 > 0) SlowAimShare = 1 - (1 - slowAimScale) * slowAim01;
            if (slowAim01 <= 0 || share <= 0) return share;
            return Math.Min(share, 1 - (1 - slowAimScale) * slowAim01);
        }

        /// <summary>NIGHT39 F1: a player parry arms the slow39 window (it opens once the parry's freeze is over).</summary>
        void Slow39Parry(HitEvent e)
        {
            if (e != null && e.owner == "player") slow39?.OnParry();
        }

        /// <summary>The way to TITLE: no slow-down carried into the next run.</summary>
        void SlowAimHalt()
        {
            slowAim01 = 0;
            SlowAimShare = 1;
            slow39?.Halt();
        }
    }
}
