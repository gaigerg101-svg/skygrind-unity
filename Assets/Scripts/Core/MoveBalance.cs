// NIGHT16 U3-29.4 (builder M owns this file): the slide made a skill, pure and tested. Three rules, each behind its own
// flag in design.move (Design/unity-config.json, on by default), read once into Params:
// 1. slideCharge: the snap up to slide.floorSpeed needs a charge (one; back rechargeTime s after the slide ends: the
//    clock holds full while any slide runs); an uncharged slide keeps its entry speed (its floor is the entry speed, never
//    above floorSpeed: no burst, never a loss); the press always slides. A slide entered at or above the floor uses none.
// 2. slideDecay: on flat ground (the slope term 0), after sustainTime s of the slide its floor eases linearly from its base
//    toward ground.walkSpeed (x the SLIPSTREAM slideSpeedMult) over decayTime s; the web's slide friction (above the floor)
//    then brings the speed down with it. Downhill untouched (the slope boost), uphill as the web.
// 3. hopFalloff: each slide-jump within chainWindow s of the last landing gets a multiplier `step` lower than the one
//    before (1.2, 1.1, 1.0, then 1.0, never below minMult); slide.jumpBoostMaxSpeed stays the cap.
// The Mover reads these through one new field (Mover.moveBalance, a State: the run's rules and their clocks), neutral by
// default: with Params.Neutral (every flag off, the golden vectors' and replays' config) it is the web's mover bit for bit.
// Whole steps (MovementMath.StepsFor at the config's simHz), JsMath for min / max; no allocation.
namespace Skygrind.Core
{
    public static class MoveBalance
    {
        /// <summary>design.move, read once (every flag off when the block is absent: the web's config).</summary>
        public struct Params
        {
            public bool slideCharge, slideDecay, hopFalloff;
            /// <summary>slideCharge's clock from the slide's ENTRY (Apex / Titanfall 2: the boost's cooldown runs from the last slide
            /// entered, sliding or not) instead of from its end (design.move.slideCharge.fromEntry; absent: false).</summary>
            public bool chargeFromEntry;
            public double rechargeTime, sustainTime, decayTime, chainWindow, hopStep, minMult;
            /// <summary>The times in whole steps at the config's simHz.</summary>
            public int rechargeSteps, sustainSteps, decaySteps, chainSteps;

            /// <summary>Every rule off: the web's mover.</summary>
            public static Params Neutral => default;

            /// <summary>Any rule on (the Mover's hooks run only then).</summary>
            public bool Any => slideCharge || slideDecay || hopFalloff;

            /// <summary>design.move (the flags are the design defaults; a run sets its own with With).</summary>
            public static Params From(GameConfig cfg)
            {
                if (!cfg.Has("design.move")) return Neutral;
                double dt = 1 / cfg.movement.loop.simHz;
                var p = new Params
                {
                    slideCharge = cfg.Design("move.slideCharge.enabled"),
                    slideDecay = cfg.Design("move.slideDecay.enabled"),
                    hopFalloff = cfg.Design("move.hopFalloff.enabled"),
                    chargeFromEntry = cfg.Design("move.slideCharge.fromEntry"),
                    rechargeTime = cfg.Num("design.move.slideCharge.rechargeTime"),
                    sustainTime = cfg.Num("design.move.slideDecay.sustainTime"),
                    decayTime = cfg.Num("design.move.slideDecay.decayTime"),
                    chainWindow = cfg.Num("design.move.hopFalloff.chainWindow"),
                    hopStep = cfg.Num("design.move.hopFalloff.step"),
                    minMult = cfg.Num("design.move.hopFalloff.minMult"),
                };
                p.rechargeSteps = MovementMath.StepsFor(p.rechargeTime, dt);
                p.sustainSteps = MovementMath.StepsFor(p.sustainTime, dt);
                p.decaySteps = MovementMath.StepsFor(p.decayTime, dt);
                p.chainSteps = MovementMath.StepsFor(p.chainWindow, dt);
                return p;
            }

            /// <summary>These numbers with a run's flags (the LAB rows, or the design defaults for automation).</summary>
            public Params With(bool charge, bool decay, bool hop)
            {
                Params p = this;
                p.slideCharge = charge;
                p.slideDecay = decay;
                p.hopFalloff = hop;
                return p;
            }
        }

        /// <summary>The Mover's balance state (Mover.moveBalance): the run's rules and their clocks, one struct so the run
        /// snapshot (MoverState) saves it whole.</summary>
        public struct State
        {
            public Params rules;
            public bool spent; // the slide charge is used (recharging)
            public int rechargeLeft; // steps until it returns (held full while a slide runs)
            public double slideBase; // this slide's floor before the decay
            public int slideSteps; // steps this slide has run
            public int sinceLand; // steps since the last landing
            public bool wasAirborne; // the mover's airborne flag at the last step's start
            public int hopChain; // the index of the last slide-jump in its chain (0: a first hop)
            public bool hopLive; // a slide-jump has happened (the chain can continue)
        }

        /// <summary>A slide's floor at its start: the floor when charged (or the rule is off), else the entry speed, never below
        /// walkFloor (the decay's target: an uncharged slide never roots the player; NIGHT16 review) nor above the floor.</summary>
        public static double EntryFloor(double floor, double entrySpeed, bool charged, double walkFloor) =>
            charged ? floor : JsMath.Min(floor, JsMath.Max(entrySpeed, walkFloor));

        /// <summary>The floor after the decay: `baseFloor` for sustainSteps, then linear to JsMath.Min(baseFloor, target) over decaySteps.</summary>
        public static double DecayedFloor(double baseFloor, double target, int slideSteps, int sustainSteps, int decaySteps)
        {
            if (slideSteps <= sustainSteps || baseFloor <= target) return baseFloor;
            if (slideSteps - sustainSteps >= decaySteps) return target;
            return baseFloor + (target - baseFloor) * ((slideSteps - sustainSteps) / (double)decaySteps);
        }

        /// <summary>The chain index of the next slide-jump: one past the last when it comes within chainSteps of the last landing, else 0.</summary>
        public static int NextHop(bool live, int lastChain, int sinceLand, int chainSteps) => live && sinceLand <= chainSteps ? (lastChain < 1000 ? lastChain + 1 : lastChain) : 0;

        /// <summary>The slide-jump multiplier at a chain index: baseMult - step x chain, never below minMult nor above baseMult.</summary>
        public static double HopMult(double baseMult, int chain, double step, double minMult) => JsMath.Min(baseMult, JsMath.Max(minMult, baseMult - step * chain));

        /// <summary>The HUD's charge (0..1, 1 = ready): the recharge's progress.</summary>
        public static double ChargeProgress(bool spent, int rechargeLeft, int rechargeSteps) =>
            !spent || rechargeSteps <= 0 ? 1 : JsMath.Max(0, JsMath.Min(1, 1 - rechargeLeft / (double)rechargeSteps));

        /// <summary>One step of the charge's clock (at a step's start): held full while sliding, else down one; true on the
        /// step the charge comes back.</summary>
        public static bool TickCharge(ref State s, bool sliding)
        {
            if (!s.spent) return false;
            if (sliding && !s.rules.chargeFromEntry)
            {
                s.rechargeLeft = s.rules.rechargeSteps; // from the slide's end: held full while it runs
                return false;
            }
            if (--s.rechargeLeft > 0) return false;
            s.rechargeLeft = 0;
            s.spent = false;
            return true;
        }
    }
}
