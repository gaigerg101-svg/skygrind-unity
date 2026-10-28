// NIGHT35 builder W (design.n35.charge35, N35Blocks.charge35; DECIDE35 D-8): the KESTREL's and THE THUNDERCLAP's charge as its own trigger shape.
// GG 2026-10-11 (after n34): "the pistol lost the ability to charge and fire same with the shotgun". The audit (Research/2026-10-11-wave35/research/audit.md
// item 1): NIGHT31's rule (ChargeShots27.cs StepCharge27 with design.n31.charge) auto-fired normal shots for the first 0.4 s of a hold, so a held trigger read
// as plain auto-fire and the charge as gone. ULTRAKILL's revolver charges on its own input with no shots first (rea-ultrakill.md).
// With the row on (the sim sets ChargeShots27.c35 at a run's start; null = NIGHT31's rule, bit-exact):
// - A press on the KESTREL / THUNDERCLAP starts a hold at once: NO normal shot fires while it is held (the charge's level counts from the press).
// - The hold ends on a release, or on a new press edge with no release seen (a bot or a script pressing while it holds):
//     * a FULL charge (n31's fullSeconds, from the press) fires the charged shot (pierce33's beam / the slug, as today: FireCharged27);
//     * anything shorter (a tap inside tapS, or a partial charge) fires ONE normal shot, at the release (queued until the gun's own fire
//       interval is over: a tap is never lost and never doubled).
// - A full charge held holdMaxS more fires itself (a held trigger never stalls the gun: the smoke's virtual pad, the attract and walk bots).
// - After a charged shot a short fixed cooldown (cooldownS per gun) gates the NEXT charge only: a press or a hold while it runs is the normal
//   fire as today (auto-fire while held); a new charge needs a new press after the cooldown.
// - A melee press while holding cancels the hold (n31's R8), nothing spent; a swap drops it (Charge27WeaponGuard).
// Events (GG's sound hooks): charge35Start {gun} (the hold passed tapS: a charge, not a tap), charge35Full {gun}, charge35Fire {gun, hits}.
// The old names still fire (chargeShot27Start at charge35Start, chargeShot27Fire / chargeShot27Cancel) so HUD33's arcs and the old reels keep reading.
// Whole steps everywhere (MovementMath.StepsFor). Per run: Reset at a run's start (ChargeShots27.Reset).
// NIGHT36 builder F tap36 (N36Blocks.tap36, design.n36.tap36; the sim sets Charge35.tap36 at a run's start): the tap fires ON the press, always, and a hold
// past holdStartS charges a SECOND, charged shot fired on the release (Loadout.StepTap36 below). Off: the rule above, bit-exact.
// NIGHT39 builder P1 p39 (N39Blocks.p39, design.n39.p39; the sim sets Charge35.p39 at a run's start, SurvivalSim.N39.Pistol.cs): the KESTREL only (the
// THUNDERCLAP keeps tap36). GG 2026-10-13: "if I'm holding it down it should intelligently prioritize the charge shot instead of firing the fast shot first".
// A press released inside tapS fires the tap ON THE RELEASE; a press held past tapS fires NO tap and charges (Loadout.StepP39 below). The release's added
// latency (the press's own length, at most tapS) is hidden as INVERSUS hides its own (research/feel-web.md section 3): the tap's ray leaves from the
// PRESS's eye along the PRESS's aim, in full up to correctFullS after the press, fading to the release's own aim by correctFadeS. A press while the
// charge cools cannot become a charge, so it fires ON the press (tap36's instant answer). Off: tap36 / charge35 as before, bit-exact.
// NIGHT40 builder W1 charge40 (N40Blocks.charge40, design.n40.charge40; the sim sets Charge35.c40 at a run's start, SurvivalSim.N40.Charge.cs). GG
// 2026-10-13: "charge shots for both weapons should take longer to load"; "The shotgun also needs a charge shot" (it had one: tap36's hold, unfound).
// - FULL comes later: the KESTREL 0.9 s, the THUNDERCLAP 1.2 s from the press (ChargeShots27.FullSteps reads c40FullS; a shop33 ratio still scales it).
// - The THUNDERCLAP takes p39's hold rule ON THE GROUND (StepP39 with gun 1: a hold fires nothing and charges the slug; a click fires ONE volley on
//   its release, aimed from the press; the press emits charge40Press). IN THE AIR a press fires at once (tap36, DECIDE40 D-6), so the boost keeps
//   its timing. The rule is picked on the press and kept for that hold (c40Ground); with no hold, by mover.grounded on the step.
// Off: p39 / tap36 / charge35 as NIGHT39 left them, bit-exact.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n35.charge35's numbers and the hold's state (gun 0 = revolver, 1 = shotgun).</summary>
    public sealed class Charge35
    {
        public double tapS, holdMaxS;
        public readonly double[] cooldownS = new double[2];
        readonly double dt;

        /// <summary>A hold in progress: the gun it began on, steps since the press, past the tap window (a charge), full.</summary>
        public bool holding, started, full;
        public int gun = -1, steps;

        /// <summary>A tap / partial hold's normal shot waiting for the gun's fire interval (the gun it is for).</summary>
        public bool tapQueued;
        public int tapGun = -1;

        /// <summary>The run's counters (the harness and the reels read them).</summary>
        public int starts, fulls, fires, autoFires, taps, partials, tapShots, meleeCancels;

        /// <summary>The last step's fire button was `pressed` (n35-r3: an automation input applied to every step of a rendered frame repeats `pressed`; a
        /// pressed run with no step between is ONE press, never a release + press).</summary>
        public bool pressedLast;

        /// <summary>NIGHT36 F tap36 (N36Blocks.tap36, design.n36.tap36; set by the sim at a run's start, SurvivalSim.Guns36.cs): the tap fires ON the
        /// press, always; a hold past holdStartS starts the charge, and a FULL charge's release fires the charged shot as a second shot. false: charge35 as
        /// NIGHT35 left it (bit-exact).</summary>
        public bool tap36;
        public double holdStartS;
        public int HoldStartSteps => MovementMath.StepsFor(holdStartS, dt);

        /// <summary>NIGHT36 tap36's counters (the harness reads them): normal shots fired on their press step, charged shots fired on a release, partial
        /// charges released (drained: nothing fires, the press already answered).</summary>
        public int pressShots36, releaseShots36, drained36;

        /// <summary>NIGHT39 P1 p39 (N39Blocks.p39, design.n39.p39; set by the sim at a run's start, SurvivalSim.N39.Pistol.cs): the KESTREL's hold rule
        /// (StepP39). false: tap36 / charge35 as NIGHT38 left them (bit-exact).</summary>
        public bool p39;
        /// <summary>design.n39.p39's numbers: the tap / hold split, whether a partial charge's release fires the tap, the aim correction's window.</summary>
        public double p39TapS, p39CorrectFullS, p39CorrectFadeS;
        public bool p39PartialFires;
        /// <summary>design.n39.p39.intervalMult: x the KESTREL's normal fire interval while p39 is live (1: as before).</summary>
        public double p39IntervalMult = 1;
        public int P39TapSteps => MovementMath.StepsFor(p39TapS, dt);

        /// <summary>p39: the press's aim (the sim's eye and look on the press step) and the queued tap's age in steps since that press (-1: no
        /// press pose for it, e.g. a click inside one step, which fires on its press step anyway).</summary>
        public Vec3 pressOrigin39, pressDir39;
        public int tapAge39 = -1;

        /// <summary>p39's counters (the harness reads them): taps fired on a quick release, taps fired on the press (the charge cooling), holds that
        /// passed tapS (no tap), partial charges released (fired or drained), taps whose aim was corrected, the last correction's weight 0..1 and the
        /// last tap's release-to-press age in steps.</summary>
        public int releaseTaps39, pressTaps39, holds39, partials39, corrected39, autoShots39, lastTapAge39 = -1;
        public double lastCorrW39;

        /// <summary>NIGHT40 W1 charge40 (N40Blocks.charge40, design.n40.charge40; set by the sim at a run's start, SurvivalSim.N40.Charge.cs): the
        /// longer FULL charges and the THUNDERCLAP's ground hold rule. false: NIGHT39's rules (bit-exact).</summary>
        public bool c40;
        /// <summary>design.n40.charge40's numbers: FULL per gun (seconds from the press), the base it replaces (n31's, so a shop33 ratio still
        /// scales it), the THUNDERCLAP's ground tap / hold split, partial-release rule and aim correction window.</summary>
        public readonly double[] c40FullS = new double[2], c40BaseS = new double[2];
        public double c40TapS, c40CorrectFullS, c40CorrectFadeS;
        public bool c40PartialFires;
        public int C40TapSteps => MovementMath.StepsFor(c40TapS, dt);
        /// <summary>The THUNDERCLAP's rule for its current hold / queued tap: true = the ground's release rule (StepP39), false = the air's tap36.</summary>
        public bool c40Ground;
        /// <summary>charge40's counters (the harness reads them): THUNDERCLAP presses under the ground rule / the air rule.</summary>
        public int c40GroundPresses, c40AirPresses;

        /// <summary>NIGHT42 W1 charge42 (N42Blocks.charge42, design.n42.charge42; set by the sim at a run's start, SurvivalSim.N42.Weapons.cs). GG
        /// 2026-10-14: "if you are hgolding down fire button and charge attack is reday t go again it should start automaticaly and not keep firing
        /// faster attack". A trigger held through the charge's cooldown fires NOTHING (no auto-fired taps; a press still fires its one tap), and the
        /// step the cooldown ends with the trigger still held starts the next charge, as a press would (Loadout.C42Held). G8: after a charged shot the
        /// gun's own fire waits turnSteps42 (the reticle's cannon turn, design.n42.charge42.cannonTurnS). false: NIGHT41's rules (bit-exact).</summary>
        public bool c42;
        public int turnSteps42;
        /// <summary>charge42's counters (the harness reads them): steps held through a cooldown with no shot, charges started by a held trigger.</summary>
        public int c42Held, c42Starts;

        /// <summary>The tap / hold split of the release rule for gun i (0: p39's KESTREL, 1: charge40's ground THUNDERCLAP).</summary>
        public int ReleaseTapSteps(int i) => i == 0 ? P39TapSteps : C40TapSteps;

        /// <summary>charge40: the ground THUNDERCLAP's correction weight for a volley fired `age` steps after its press (as P39CorrectionWeight).</summary>
        public double C40CorrectionWeight(int age)
        {
            if (age < 0) return 0;
            double t = age * dt;
            if (t <= c40CorrectFullS) return 1;
            if (t >= c40CorrectFadeS || c40CorrectFadeS <= c40CorrectFullS) return 0;
            return (c40CorrectFadeS - t) / (c40CorrectFadeS - c40CorrectFullS);
        }

        public Charge35(GameConfig cfg) : this(cfg, "design.n35.charge35.") { }

        public Charge35(GameConfig cfg, string p)
        {
            dt = cfg.Dt;
            tapS = cfg.Num(p + "tapS");
            holdMaxS = cfg.Num(p + "holdMaxS");
            cooldownS[0] = cfg.Num(p + "revolver.cooldownS");
            cooldownS[1] = cfg.Num(p + "shotgun.cooldownS");
        }

        public int TapSteps => MovementMath.StepsFor(tapS, dt);
        public int HoldMaxSteps => MovementMath.StepsFor(holdMaxS, dt);
        public int CooldownSteps(int i) => MovementMath.StepsFor(cooldownS[i], dt);

        public void Clear()
        {
            holding = started = full = false;
            gun = -1;
            steps = 0;
        }

        public void Reset()
        {
            Clear();
            tapQueued = false;
            tapGun = -1;
            pressedLast = false;
            starts = fulls = fires = autoFires = taps = partials = tapShots = meleeCancels = 0;
            pressShots36 = releaseShots36 = drained36 = 0;
            tapAge39 = lastTapAge39 = -1;
            releaseTaps39 = pressTaps39 = holds39 = partials39 = corrected39 = autoShots39 = 0;
            lastCorrW39 = 0;
            c40Ground = false;
            c40GroundPresses = c40AirPresses = 0;
            c42Held = c42Starts = 0;
        }

        /// <summary>p39: the aim correction's weight for a tap fired `age` steps after its press: 1 up to correctFullS, 0 from correctFadeS, linear
        /// between (INVERSUS: full to 0.11 s, none by 0.19 s; research/feel-web.md section 3).</summary>
        public double P39CorrectionWeight(int age)
        {
            if (age < 0) return 0;
            double t = age * dt;
            if (t <= p39CorrectFullS) return 1;
            if (t >= p39CorrectFadeS || p39CorrectFadeS <= p39CorrectFullS) return 0;
            return (p39CorrectFadeS - t) / (p39CorrectFadeS - p39CorrectFullS);
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT35 charge35: the hold's level 0..1 from the press (the viewmodel's glow and the rail's fill read it; 0 with the row off or no hold).</summary>
        public double Charge35Level
        {
            get
            {
                Charge35 q = charge27 != null && charge27.active ? charge27.c35 : null;
                if (q == null || !q.holding || q.gun < 0) return 0;
                bool p39 = q.p39 && q.gun == 0; // NIGHT39 P1: the KESTREL's hold glows from tapS, as tap36's from holdStartS
                bool c40g = q.c40 && q.gun == 1 && q.c40Ground; // NIGHT40 W1 charge40: the ground THUNDERCLAP's hold glows from its tapS
                if (q.tap36 || p39 || c40g)
                {
                    // NIGHT36 tap36: no level until a real hold has started (a tap never glows); then 0..1 from the hold's start to FULL
                    if (!q.started) return 0;
                    int hs = p39 ? q.P39TapSteps : c40g ? q.C40TapSteps : q.HoldStartSteps, full = charge27.FullSteps(q.gun);
                    if (q.steps >= full) return 1;
                    return full > hs ? Math.Max(0.0, Math.Min(1.0, (q.steps - hs) / (double)(full - hs))) : 1;
                }
                return Math.Min(1.0, q.steps / (double)charge27.FullSteps(q.gun));
            }
        }

        /// <summary>NIGHT35 charge35: the fire gate while the row is on (StepCharge27 hands over here). True: this step is spent (a hold, a tap's shot, a
        /// charged shot); false: the normal path runs (a press or a hold while the charge cools: today's fire).</summary>
        bool StepCharge35(string weapon, int i, CombatInput input, double dt)
        {
            ChargeShots27 c = charge27;
            Charge35 q = c.c35;
            if (q.c42 && C42Held(weapon, i, input)) return true; // NIGHT42 W1 charge42: a held trigger through the cooldown fires nothing, then charges
            if (q.p39 && i == 0) return StepP39(weapon, i, input, dt); // NIGHT39 P1 p39: the KESTREL's hold charges with no tap first
            if (q.c40 && i == 1 && C40Ground(q, i, input)) return StepP39(weapon, i, input, dt); // NIGHT40 W1 charge40: the THUNDERCLAP's ground hold rule
            if (q.tap36) return StepTap36(weapon, i, input, dt); // NIGHT36 tap36: the tap on the press, the charge as a second shot
            if (q.holding && q.gun != i) { q.Clear(); c.ClearHold(); } // the gun in hand changed under the hold: dropped, nothing spent
            if (q.tapQueued && q.tapGun != i) q.tapQueued = false;
            if (c.p.meleeCancels && input.punch.pressed && q.holding)
            {
                // n31's R8: a melee press while a hold builds cancels it, nothing spent (fire and melee never overlap)
                q.Clear();
                c.ClearHold();
                c.cancelled++;
                c.meleeCancelled++;
                q.meleeCancels++;
                chargeEv27.gun = weapon;
                events.Emit("chargeShot27Cancel", chargeEv27);
                return true; // this step is the punch's
            }
            bool held = input.fire.held, spent = false, ended = false;
            // n35-r3 (verbs35 rows 27 / 30: 2 normal shots before the charge): a forced input repeats `pressed` on every step of its frame; only the
            // first step of a pressed run is the edge (a human trigger's latch is consumed after one step, so this changes nothing for a player)
            bool pressed = input.fire.pressed && !q.pressedLast;
            q.pressedLast = input.fire.pressed;
            if (q.holding && (!held || pressed))
            {
                // the hold ends: a release, or a new press edge with no release seen (a bot / script pressing while it holds)
                bool wasFull = q.steps >= c.FullSteps(i), wasCharge = q.started;
                q.Clear();
                c.ClearHold();
                ended = true;
                if (wasFull) spent = FireCharged35(weapon, i, dt);
                else
                {
                    q.tapQueued = true; // a tap or a partial charge: one normal shot (the trigger always answers)
                    q.tapGun = i;
                    if (wasCharge) q.partials++;
                    else q.taps++;
                }
            }
            if (pressed)
            {
                if (!held)
                {
                    // a click inside one step (the mouse latch): a tap
                    if (!ended) { q.tapQueued = true; q.tapGun = i; q.taps++; }
                }
                else if (c.cooldown[i] <= 0 && !spent)
                {
                    q.holding = true;
                    q.gun = i;
                    q.steps = 0;
                }
                // a press while the charge cools: the normal path below fires it (today's tap at the press)
            }
            if (q.holding)
            {
                q.steps++;
                if (!q.started && q.steps > q.TapSteps)
                {
                    q.started = true;
                    q.starts++;
                    c.charging = true;
                    chargeEv27.gun = weapon;
                    events.Emit("charge35Start", chargeEv27);
                    events.Emit("chargeShot27Start", chargeEv27);
                }
                if (q.started) c.holdSteps = c.chargeSteps = q.steps; // ChargeShots27.Level (HUD33's arcs, the rail's state) reads the hold from the press
                if (!q.full && q.steps >= c.FullSteps(i))
                {
                    q.full = true;
                    q.fulls++;
                    chargeEv27.gun = weapon;
                    events.Emit("charge35Full", chargeEv27);
                }
                if (q.full && q.steps >= c.FullSteps(i) + q.HoldMaxSteps)
                {
                    // a full charge held holdMaxS more fires itself; the hold then runs the normal fire through the cooldown
                    q.Clear();
                    c.ClearHold();
                    c.autoReleased++;
                    q.autoFires++;
                    FireCharged35(weapon, i, dt);
                    return true;
                }
                if (!q.tapQueued) return true; // holding: the trigger is a charge, no shot
            }
            if (spent) return true;
            if (q.tapQueued)
            {
                if (CooldownOf(weapon) > 0) return true; // the tap's shot waits for the gun (the held auto-fire never doubles it)
                q.tapQueued = false;
                q.tapShots++;
                FireNormal35(weapon, dt);
                return true;
            }
            return false;
        }

        /// <summary>NIGHT40 W1 charge40: does the THUNDERCLAP take the ground's release rule this step? A hold keeps the rule its press chose; with no
        /// hold, mover.grounded on this step picks it (a tap queued under the other rule loses its press pose: no correction).</summary>
        bool C40Ground(Charge35 q, int i, CombatInput input)
        {
            if (boost43.on && Boost43TakeoffFire(q, i)) return true; // NIGHT43 boost43 (a): a ground hold at the takeoff fires now, in the air (Core/Boost43.cs)
            bool ground = q.holding && q.gun == i ? q.c40Ground : mover.grounded;
            if (ground != q.c40Ground)
            {
                if (q.tapQueued && q.tapGun == i) q.tapAge39 = -1;
                q.c40Ground = ground;
            }
            if (input.fire.pressed && !q.pressedLast && !(q.holding && q.gun == i))
            {
                if (ground) q.c40GroundPresses++;
                else q.c40AirPresses++;
            }
            return ground;
        }

        /// <summary>
        /// NIGHT42 W1 charge42 (G10): the held trigger with no hold of its own on gun i. While the charge cools it fires NOTHING (the step is spent:
        /// no auto-fired taps); the first step the charge is ready again with the trigger still held starts a hold exactly as a press would (the press
        /// pose kept for p39's correction, charge40's ground / air rule picked now), and the gun's own rule then runs it (charge35Start past its tap
        /// split, FULL, the release fires it). A press edge, a hold already running, a tap still owed or a melee press: the old rules (false).
        /// Event: charge42Rearm {gun} on the held start (GG's sound hook).
        /// </summary>
        bool C42Held(string weapon, int i, CombatInput input)
        {
            ChargeShots27 c = charge27;
            Charge35 q = c.c35;
            if (!input.fire.held || (input.fire.pressed && !q.pressedLast)) return false;
            if (q.holding && q.gun == i) return false;
            if (q.tapQueued && q.tapGun == i) return false;
            if (c.p.meleeCancels && input.punch.pressed) return false;
            if (q.holding) { q.Clear(); c.ClearHold(); } // a hold on the other gun: dropped (as the rules' own swap check)
            if (c.cooldown[i] > 0)
            {
                q.pressedLast = input.fire.pressed;
                q.c42Held++;
                return true; // held through the cooldown: no shot
            }
            q.holding = true;
            q.gun = i;
            q.steps = 0;
            if (i == 1 && q.c40) q.c40Ground = mover.grounded;
            var (o, d) = getAim();
            q.pressOrigin39 = o;
            q.pressDir39 = d;
            q.c42Starts++;
            chargeEv27.gun = weapon;
            events.Emit("charge42Rearm", chargeEv27);
            return false; // the gun's own rule steps the new hold this step
        }

        /// <summary>
        /// NIGHT36 F tap36 (REVIEW.md root cause 1: charge35 fired every tap on its RELEASE, 50-100 ms after the finger). ULTRAKILL's tap is instant on the
        /// press and its charge never delays it (Research/2026-10-12-wave36/research/rea-ultrakill-combat.md: Revolver.cs:365). The rule:
        /// - every press edge fires ONE normal shot on that step when the gun's own interval allows it, else the shot waits for the interval (never lost);
        /// - a press that is still held starts a hold (unless the charge cools: then a held trigger is today's auto-fire); past holdStartS the hold is a
        ///   CHARGE (charge35Start / chargeShot27Start, the HUD's CHARGING), and a FULL charge (n31's fullSeconds, from the press) fires the charged shot
        ///   on the release as a second shot; a partial charge released drains (nothing fires: the press already answered);
        /// - a full charge held holdMaxS more fires itself; a melee press cancels the hold; a swap drops it (as charge35).
        /// While a hold runs the trigger never auto-fires (a hold is a charge, not a stream).
        /// </summary>
        bool StepTap36(string weapon, int i, CombatInput input, double dt)
        {
            ChargeShots27 c = charge27;
            Charge35 q = c.c35;
            if (q.holding && q.gun != i) { q.Clear(); c.ClearHold(); }
            if (q.tapQueued && q.tapGun != i) q.tapQueued = false;
            if (c.p.meleeCancels && input.punch.pressed && q.holding)
            {
                q.Clear();
                c.ClearHold();
                c.cancelled++;
                c.meleeCancelled++;
                q.meleeCancels++;
                chargeEv27.gun = weapon;
                events.Emit("chargeShot27Cancel", chargeEv27);
                return true;
            }
            bool held = input.fire.held, spent = false;
            bool pressed = input.fire.pressed && !q.pressedLast; // n35-r3: a forced input's pressed run is one press
            q.pressedLast = input.fire.pressed;
            if (q.holding && (!held || pressed))
            {
                bool wasFull = q.steps >= c.FullSteps(i), wasCharge = q.started;
                q.Clear();
                c.ClearHold();
                if (wasFull)
                {
                    spent = FireCharged35(weapon, i, dt);
                    q.releaseShots36++;
                }
                else if (wasCharge)
                {
                    q.partials++;
                    q.drained36++;
                }
            }
            if (pressed)
            {
                q.taps++;
                if (!spent && CooldownOf(weapon) <= 0)
                {
                    q.tapQueued = false;
                    q.tapShots++;
                    q.pressShots36++;
                    FireNormal35(weapon, dt); // THE TAP: on the press step
                    spent = true;
                }
                else
                {
                    q.tapQueued = true; // the gun's interval (or this step's charged shot) first: the tap waits, never lost
                    q.tapGun = i;
                }
                if (held && c.cooldown[i] <= 0)
                {
                    q.holding = true;
                    q.gun = i;
                    q.steps = 0;
                }
            }
            if (q.holding)
            {
                q.steps++;
                if (!q.started && q.steps > q.HoldStartSteps)
                {
                    q.started = true;
                    q.starts++;
                    c.charging = true;
                    chargeEv27.gun = weapon;
                    events.Emit("charge35Start", chargeEv27);
                    events.Emit("chargeShot27Start", chargeEv27);
                }
                if (q.started) c.holdSteps = c.chargeSteps = q.steps;
                if (!q.full && q.steps >= c.FullSteps(i))
                {
                    q.full = true;
                    q.fulls++;
                    chargeEv27.gun = weapon;
                    events.Emit("charge35Full", chargeEv27);
                }
                if (q.full && q.steps >= c.FullSteps(i) + q.HoldMaxSteps)
                {
                    q.Clear();
                    c.ClearHold();
                    c.autoReleased++;
                    q.autoFires++;
                    FireCharged35(weapon, i, dt);
                    return true;
                }
            }
            if (q.tapQueued && !spent && CooldownOf(weapon) <= 0)
            {
                q.tapQueued = false;
                q.tapShots++;
                FireNormal35(weapon, dt);
                spent = true;
            }
            // spent, holding, a press, or a tap waiting: this step is the charge rule's; otherwise the normal path (a held trigger while the charge cools)
            return spent || q.holding || pressed || q.tapQueued;
        }

        /// <summary>
        /// NIGHT39 P1 p39 (the KESTREL only; GG 2026-10-13: a held trigger "should intelligently prioritize the charge shot instead of firing the fast shot
        /// first"). Research/2026-10-12-wave36/REVIEW.md root cause 1 is why tap36 fired on the press: charge35's release-fired tap read late. p39 takes the
        /// release rule back for the KESTREL only, with the two fixes INVERSUS shipped for the same delay (research/feel-web.md section 3):
        /// - a press edge that can charge (the charge not cooling) starts a hold and fires nothing; the press emits p39Press (the view answers at once:
        ///   the hammer cocks, GG's sound hook), and the press's eye and aim are kept;
        /// - released inside tapS: ONE normal shot on the release step (or when the gun's interval allows), its ray from the press's eye along the
        ///   press's aim (P39CorrectionWeight: in full up to correctFullS after the press, fading to the release's own aim by correctFadeS);
        /// - held past tapS: a CHARGE (charge35Start / chargeShot27Start), no tap ever; a FULL charge (n31's fullSeconds from the press) fires the charged
        ///   shot on the release; a partial charge's release fires the tap (partialFires: the trigger always answers) or drains;
        /// - a press while the charge cools (no hold can charge) fires ON the press step, as tap36 (a held trigger then auto-fires, as today);
        /// - a full charge held holdMaxS more fires itself; a melee press cancels the hold; a swap drops it (as charge35 / tap36).
        /// </summary>
        bool StepP39(string weapon, int i, CombatInput input, double dt)
        {
            ChargeShots27 c = charge27;
            Charge35 q = c.c35;
            if (q.holding && q.gun != i) { q.Clear(); c.ClearHold(); }
            if (q.tapQueued && q.tapGun != i) { q.tapQueued = false; q.tapAge39 = -1; }
            if (c.p.meleeCancels && input.punch.pressed && q.holding)
            {
                q.Clear();
                c.ClearHold();
                c.cancelled++;
                c.meleeCancelled++;
                q.meleeCancels++;
                chargeEv27.gun = weapon;
                events.Emit("chargeShot27Cancel", chargeEv27);
                return true;
            }
            bool held = input.fire.held, spent = false, ended = false;
            bool pressed = input.fire.pressed && !q.pressedLast; // n35-r3: a forced input's pressed run is one press
            q.pressedLast = input.fire.pressed;
            if (q.tapQueued && q.tapAge39 >= 0) q.tapAge39++; // a tap waiting for the gun's interval ages from its press
            if (q.holding && (!held || pressed))
            {
                // the hold ends: a release, or a new press edge with no release seen (a bot / script pressing while it holds)
                bool wasFull = q.steps >= c.FullSteps(i), wasCharge = q.started;
                int age = q.steps;
                q.Clear();
                c.ClearHold();
                ended = true;
                if (wasFull)
                {
                    spent = FireCharged35(weapon, i, dt);
                    q.releaseShots36++;
                }
                else if (!wasCharge || (i == 0 ? q.p39PartialFires : q.c40PartialFires)) // NIGHT40 W1: the ground THUNDERCLAP reads charge40's
                {
                    q.tapQueued = true; // a quick release (or a partial charge): the tap, on the release, aimed from the press
                    q.tapGun = i;
                    q.tapAge39 = age;
                    q.releaseTaps39++;
                    if (wasCharge) { q.partials++; q.partials39++; }
                    else q.taps++;
                }
                else
                {
                    q.partials++;
                    q.partials39++;
                    q.drained36++;
                }
            }
            if (pressed)
            {
                if (held && c.cooldown[i] <= 0 && !spent)
                {
                    // a press that can charge: a hold, nothing fires yet (the press's pose is kept for the tap's correction)
                    q.holding = true;
                    q.gun = i;
                    q.steps = 0;
                    var (o, d) = getAim();
                    q.pressOrigin39 = o;
                    q.pressDir39 = d;
                    chargeEv27.gun = weapon;
                    events.Emit(i == 0 ? "p39Press" : "charge40Press", chargeEv27); // NIGHT40 W1: the ground THUNDERCLAP's press has its own name
                }
                else if (!ended)
                {
                    // a click inside one step (the mouse latch), or a press while the charge cools: no hold can charge, so the tap fires ON the press
                    q.tapQueued = true;
                    q.tapGun = i;
                    q.tapAge39 = -1;
                    q.taps++;
                    q.pressTaps39++;
                }
            }
            if (q.holding)
            {
                q.steps++;
                if (!q.started && q.steps > q.ReleaseTapSteps(i)) // NIGHT40 W1: gun 1 = charge40's shotgunTapS
                {
                    q.started = true;
                    q.starts++;
                    q.holds39++;
                    c.charging = true;
                    chargeEv27.gun = weapon;
                    events.Emit("charge35Start", chargeEv27);
                    events.Emit("chargeShot27Start", chargeEv27);
                }
                if (q.started) c.holdSteps = c.chargeSteps = q.steps; // ChargeShots27.Level (HUD33's arcs, the rail) reads the hold from the press
                if (!q.full && q.steps >= c.FullSteps(i))
                {
                    q.full = true;
                    q.fulls++;
                    chargeEv27.gun = weapon;
                    events.Emit("charge35Full", chargeEv27);
                }
                if (q.full && q.steps >= c.FullSteps(i) + q.HoldMaxSteps)
                {
                    q.Clear();
                    c.ClearHold();
                    c.autoReleased++;
                    q.autoFires++;
                    FireCharged35(weapon, i, dt);
                    return true;
                }
                if (!q.tapQueued) return true; // holding: the trigger is a charge, no shot
            }
            if (spent) return true;
            if (q.tapQueued)
            {
                if (CooldownOf(weapon) > 0) return true; // the tap waits for the gun's interval (never lost, never doubled)
                q.tapQueued = false;
                q.tapShots++;
                FireTap39(weapon, q.tapAge39, dt);
                q.tapAge39 = -1;
                return true;
            }
            if (i == 0 && held && q.p39IntervalMult != 1) // NIGHT40 W1: the KESTREL's slower interval only (gun 1 here is charge40's ground THUNDERCLAP)
            {
                // a held trigger while the charge cools: the auto-fire, here so it keeps p39's slower interval (Combat.Step's own branch would not)
                if (CooldownOf(weapon) <= 0)
                {
                    q.autoShots39++;
                    FireTap39(weapon, -1, dt);
                }
                return true;
            }
            return false; // a held trigger while the charge cools: the normal path's auto-fire (as tap36 / charge35)
        }

        /// <summary>p39's tap: the normal shot with the press's aim blended in (aim39W, read by ShotAim39 inside FireRevolver).</summary>
        void FireTap39(string weapon, int age, double dt)
        {
            Charge35 q = charge27.c35;
            double w = weapon == "revolver" ? q.P39CorrectionWeight(age) : q.C40CorrectionWeight(age); // NIGHT40 W1: the ground THUNDERCLAP's window
            q.lastTapAge39 = age;
            q.lastCorrW39 = w;
            if (w > 0)
            {
                aim39W = w;
                q.corrected39++;
            }
            FireNormal35(weapon, dt);
            aim39W = 0;
        }

        /// <summary>p39: the correction weight live only while a corrected tap fires (0 otherwise: ShotAim39 is getAim()).</summary>
        double aim39W;

        /// <summary>NIGHT39 P1: the KESTREL shot's aim (FireRevolver's one call). getAim() exactly, unless a p39 tap fires with a correction: then the
        /// press's eye and aim, blended toward the release's by the weight (1: the press's exactly).</summary>
        (Vec3, Vec3) ShotAim39()
        {
            var a = getAim();
            if (aim39W <= 0) return a;
            Charge35 q = charge27.c35;
            if (aim39W >= 1) return (q.pressOrigin39, q.pressDir39);
            Vec3 o = a.Item1 + (q.pressOrigin39 - a.Item1) * aim39W;
            Vec3 d = Vec3.Normalize(a.Item2 + (q.pressDir39 - a.Item2) * aim39W);
            return (o, d);
        }

        /// <summary>The charged shot (FireCharged27: pierce33's beam / the slug; the cooldown is charge35's through ChargeShots27.CooldownSteps), then charge35Fire.</summary>
        bool FireCharged35(string weapon, int i, double dt)
        {
            bool r = FireCharged27(weapon, i, dt); // sets chargeEv27.gun / hits and emits chargeShot27Fire
            charge27.c35.fires++;
            Charge35 q42 = charge27.c35;
            if (q42.c42 && q42.turnSteps42 > CooldownOf(weapon)) SetCooldown(weapon, q42.turnSteps42); // NIGHT42 W1 charge42 (G8): the next shot waits for the reticle's turn
            events.Emit("charge35Fire", chargeEv27);
            return r;
        }

        /// <summary>The KESTREL's / THUNDERCLAP's normal shot, exactly Combat.Step's own branch for them (their interval, then the shot).</summary>
        void FireNormal35(string weapon, double dt)
        {
            if (weapon == "revolver")
            {
                Charge35 q = charge27 != null ? charge27.c35 : null;
                double slow = q != null && q.p39 ? q.p39IntervalMult : 1; // NIGHT39 P1 p39: the KESTREL's heavier, slower tap (design.n39.p39.intervalMult)
                SetCooldown("revolver", MovementMath.StepsFor(revInterval * TuneInterval("revolver") * slow / fireRateMult, dt));
                FireRevolver();
            }
            else
            {
                SetCooldown("shotgun", MovementMath.StepsFor(sgInterval * TuneInterval("shotgun") * sgIntervalMult24 * sgIntervalMult29 / fireRateMult, dt));
                if (Arsenal19On && a19 != null) FireShotgun19(dt); // NIGHT19 U3-33.6 (Sandbox19.cs): Combat.Step's local arsenal19
                else FireShotgun();
            }
        }
    }
}
