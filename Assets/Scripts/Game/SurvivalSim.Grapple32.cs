// NIGHT32 (builder G owns this file, DESIGN32 G, D-1 / D-3): THE GRAPPLE HAS CHARGES (design.n32.grapple32). GG (2026-10-10): "The grapple is
// way too broken ... you can just spam the grapple". A1 (Research/2026-10-10-wave32/A1-boss-audit.md, the grapple): the only limit was the
// web's 0.4 s rope cooldown (Mover.cs:498 / :525), any wall in 60 m was a target, nothing was spent, and a jump / dash / second press
// mid-pull kept 44 m/s past the 12 m/s arrival cap. The Mover is frozen (CLAUDE.md rule 3): every rule here works from OUTSIDE it.
// - Charges: `charges` (2). A hook that FIRES spends one (the Mover's RopeFire: a tap-pull and a hold-swing alike); a press with no target
//   fires nothing and spends nothing. A spent charge comes back after `rechargeSeconds` (one at a time); landing does not refill; a
//   non-boss kill (the "kill" event, kind not in `bossKinds`) gives one back. With 0 charges, or within `minGapSeconds` of the last fire,
//   the press is cleared on the frame before the Mover reads it (N32GrappleFrame: after the NIGHT31 latch, before mover.Step).
// - Taps (tapNodesOnly, D-3): a tap pulls only to a node (a ring, a boss joint) or a big enemy's zip (GrappleTap31.Refused on a bare
//   surface: the button comes up, the Mover lets go; the charge comes back, as for no target). A HOLD may still swing from a wall.
// - Range: `range` (40 m) on the Mover's config.arcline.range for the run (GrappleAim's nodes, rays and enemy pick read it; the way
//   SurvivalSim.Grapple31 writes the zip speed), the web's 60 back on a run with the row off.
// - Cancels: on the step a jump (in the air) or a dash releases a latched pull's attached rope, or a second press lets go of it, the
//   velocity is clamped to `cancelKeepMax` (20 m/s) BEFORE the Mover's release (which keeps it, x1.08 while rising, plus the jump's own
//   up); so a dash out of a pull is a plain dash (MovementMath.DashSpeed: max(24, entry)), not 44 m/s for its whole exit.
// Events (new, PORT-STATUS's list): grappleCharges (int: charges now) whenever the count changes; grappleEmpty when a press is cleared (no
// charge or the gap). Off (the row, --off32 and every older off switch): nothing here touches a frame, the Mover or the config: NIGHT31.
// Pure and deterministic: every clock is in whole steps and resets at run start (a tape replays it).
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>design.n32.grapple32's numbers (Unity-only: the web's grapple has only its 0.4 s cooldown).</summary>
    public sealed class Grapple32Rules
    {
        public int charges;
        public double rechargeSeconds, minGapSeconds, cancelKeepMax, range;
        public bool killRefill, tapNodesOnly;
        public string[] bossKinds;

        public static Grapple32Rules From(GameConfig cfg)
        {
            const string p = "design.n32.grapple32.";
            var r = new Grapple32Rules
            {
                charges = (int)cfg.Num(p + "charges"),
                rechargeSeconds = cfg.Num(p + "rechargeSeconds"),
                minGapSeconds = cfg.Num(p + "minGapSeconds"),
                cancelKeepMax = cfg.Num(p + "cancelKeepMax"),
                range = cfg.Num(p + "range"),
                killRefill = cfg.Design("n32.grapple32.killRefill"),
                tapNodesOnly = cfg.Design("n32.grapple32.tapNodesOnly"),
            };
            r.bossKinds = cfg.Has(p + "bossKinds") ? cfg.Strs(p + "bossKinds").ToArray() : new[] { "boss" };
            return r;
        }
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT32: design.n32.grapple32's numbers (null until a run starts with the row on).</summary>
        public Grapple32Rules grapple32Rules;

        /// <summary>NIGHT32: grapple charges now (the pips; the max with the row off).</summary>
        public int grappleCharges32;

        /// <summary>NIGHT32: steps until the next charge comes back (0: full or the row off) and the steps of one recharge (the pip's fill).</summary>
        public int grappleRechargeLeft32, grappleRechargeSteps32;

        /// <summary>NIGHT32: is the row on for this run (the pips' and the harness's switch)?</summary>
        public bool Grapple32On => n32.grapple32 && grapple32Rules != null;

        /// <summary>NIGHT32 counters this run (the harness): charges spent, given back (no target / a refused tap), refilled by kills, presses
        /// cleared (no charge / the gap), cancels clamped.</summary>
        public int g32Spent, g32Refunds, g32KillRefills, g32Empty, g32Clamps;

        double? g32WebRange;
        bool g32Wired;
        int g32GapLeft, g32SeenCancels, g32SeenRefused;

        partial void N32GrappleStartRun()
        {
            if (!g32Wired)
            {
                mover.Events += G32OnMover;
                events.On<KillEvent>("kill", G32OnKill);
                g32Wired = true;
            }
            ArclineConfig a = cfg.movement.arcline;
            g32Spent = g32Refunds = g32KillRefills = g32Empty = g32Clamps = 0;
            g32GapLeft = 0;
            g32SeenCancels = g32SeenRefused = 0;
            grappleRechargeLeft32 = 0;
            if (!n32.grapple32)
            {
                if (g32WebRange.HasValue) a.range = g32WebRange.Value;
                g32WebRange = null;
                grappleTap31.tapNodesOnly = false;
                grappleCharges32 = grapple32Rules != null ? grapple32Rules.charges : 0;
                return;
            }
            grapple32Rules ??= Grapple32Rules.From(cfg);
            g32WebRange ??= a.range;
            a.range = grapple32Rules.range;
            grappleTap31.tapNodesOnly = grapple32Rules.tapNodesOnly;
            grappleCharges32 = grapple32Rules.charges;
            grappleRechargeSteps32 = MovementMath.StepsFor(grapple32Rules.rechargeSeconds, step);
        }

        /// <summary>Before mover.Step, after the NIGHT31 latch: clear a press with no charge or inside the gap; clamp a pull about to be cancelled.</summary>
        partial void N32GrappleFrame(ref InputFrame frame)
        {
            if (!Grapple32On) return;
            Rope r = mover.rope;
            // A tap the latch refused on a bare surface: the hook it fired spent a charge for nothing: give it back (no target).
            if (grappleTap31.refused != g32SeenRefused)
            {
                g32SeenRefused = grappleTap31.refused;
                g32GapLeft = 0;
                G32Give(ref g32Refunds);
            }
            // A cancel of a pull this step: the latch's second press (letGo), a jump in the air or a dash on the attached rope. Clamp first.
            bool cancel = grappleTap31.cancels != g32SeenCancels;
            g32SeenCancels = grappleTap31.cancels;
            if (!cancel && grappleTap31.Pulling && r.phase == RopePhase.Attached && ((frame.jump.pressed && !mover.grounded) || frame.dash.pressed)) cancel = true;
            if (cancel)
            {
                double sp = mover.velocity.Length;
                double cap = grapple32Rules.cancelKeepMax;
                if (cap > 0 && sp > cap)
                {
                    double k = cap / sp;
                    mover.velocity = new Vec3(mover.velocity.x * k, mover.velocity.y * k, mover.velocity.z * k);
                    g32Clamps++;
                }
            }
            if (frame.grapple.pressed && r.phase == RopePhase.None && (grappleCharges32 <= 0 || g32GapLeft > 0))
            {
                frame.grapple.pressed = false; // the Mover fires only on a press: nothing goes out, nothing is spent
                g32Empty++;
                events.Emit("grappleEmpty");
            }
        }

        /// <summary>After mover.Step: the gap and the recharge run.</summary>
        partial void N32GrappleStep(double dt)
        {
            if (!Grapple32On) return;
            if (g32GapLeft > 0) g32GapLeft--;
            if (grappleCharges32 >= grapple32Rules.charges)
            {
                grappleRechargeLeft32 = 0;
                return;
            }
            if (grappleRechargeLeft32 <= 0) grappleRechargeLeft32 = grappleRechargeSteps32;
            if (--grappleRechargeLeft32 > 0) return;
            grappleCharges32++;
            events.Emit("grappleCharges", grappleCharges32);
            if (grappleCharges32 < grapple32Rules.charges) grappleRechargeLeft32 = grappleRechargeSteps32;
        }

        /// <summary>A hook fired: one charge spent (a tap and a hold alike), the gap starts.</summary>
        void G32OnMover(MoverEvent e)
        {
            if (!Grapple32On || e.type != MoverEventType.RopeFire) return;
            if (grappleCharges32 >= grapple32Rules.charges) grappleRechargeLeft32 = grappleRechargeSteps32; // the clock starts on the first spend
            if (grappleCharges32 > 0) grappleCharges32--;
            g32Spent++;
            g32GapLeft = MovementMath.StepsFor(grapple32Rules.minGapSeconds, step);
            events.Emit("grappleCharges", grappleCharges32);
        }

        void G32OnKill(KillEvent e)
        {
            if (!Grapple32On || !grapple32Rules.killRefill || e.owner != "player" || System.Array.IndexOf(grapple32Rules.bossKinds, e.kind) >= 0) return;
            if (n33.whip33 && e.weapon == Whip33Weapon)
            {
                whip33NoRefill++; // NIGHT33 whip33 (D-3, A4): a kill by the whip's own hit gives no charge back (else husks chain the whip forever)
                return;
            }
            G32Give(ref g32KillRefills);
        }

        void G32Give(ref int counter)
        {
            if (grappleCharges32 >= grapple32Rules.charges) return;
            grappleCharges32++;
            counter++;
            if (grappleCharges32 >= grapple32Rules.charges) grappleRechargeLeft32 = 0;
            events.Emit("grappleCharges", grappleCharges32);
        }
    }
}
