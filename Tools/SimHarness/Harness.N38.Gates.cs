// NIGHT38 builder E (NIGHT38.md E1; PLAN.md section 2; DECIDE38 D-6): the envelope table and the gate checker, declared and called by the
// lead's Program.cs (N38Gates). Every number is MEASURED on the real sim: a SurvivalSim (Headless, a Survival run started with the config's
// rows, so the Mover carries the game's flow34 / wallride35 / wallgrip36 / stamina / (ride38 with --ride38) rules and the THUNDERCLAP's boost
// is the real Loadout's shot: `velocity -= aim x weapons.shotgun.jumpSpeed` when airborne, at the gun's own fire interval) whose Mover's
// World is swapped for a test world (TriangleWorld: the web's collider) 2 km east of the survival map (no pad, zone or core cell of it there).
// Nothing is hand-typed: the distances come out of sim.Step.
//
//   n38 envelopes [--write] [--ride38]
//       every verb and combo from standing ground at a lip (a 40 m ledge over a void: the metres reached at the takeoff height, at 2 m under
//       it, the apex gain) and from a wall ride (a held ride released at look pitch -45 / 0 / +45, then each air verb: the metres out from the
//       wall plane at the release height, along the wall, the apex gain), the facing-wall ride chain (plane gaps 3..20 m that chain), the
//       shaft climb (taps, walls 3 m and 5 m apart: metres climbed in 4 s); the grapple and the shockwave are READ (their live state and
//       numbers: the hook aims through the sim's own GameWorld, so the gate checker tests the grapple by line of sight + range).
//       --write: Design/levels/envelopes38.json (the table; rerun after any movement tuning).
//   n38 gates <plan.json> [--verbose] [--only <gateId>]
//       every solid (box or collider prop) with `gate: {verb, from, to, runUp?, id?}` in a level PLAN: the real Mover (this rig) is dropped
//       `runUp` m (default 6) behind `from` on the line from -> to, runs at it and tries EVERY verb / combo the level owns (`owns38` on the
//       plan; default every verb but the grapple). PASS (D-6): some try that uses the gate's verb lands on the gate solid (grounded at its top,
//       past its near edge) with a margin >= 10 % (the same try with the gate solid taken out of the world: how far past the near edge it
//       still is at the top's height, / the gap; and for a rise over 0.5 m the apex over the top / the rise: the smaller), and NO try without
//       the gate's verb lands there. Prints each gate's margins, the best other try, every crossing cheat. Exit 1 on a RED gate.
//       --ride40 (NIGHT40 R2): ride40 forced on in the rig (every ride holds; the attach asks for intent) and THE CLIMB added to the ride
//       tries (the look pitched up +25 / +45 while riding drifts the endless ride up to the wall's top, then the release at the top, 1 m or 5 m
//       before the near edge, or at the wall's end): an endless ride may cross what no 1.2 s ride could (L-042 / L-061).
//   n38 gates --selftest
//       a synthetic room of three gap gates sized from the measured envelope: a clean gate (must PASS), a planted cheesable gate (a gap the
//       best other verb clears: must FAIL), a planted thin gate (the verb's margin under 10 %: must FAIL). GREEN only if all three behave.
//   NIGHT39 E2: the plan's ICE (its `surfaces`) is in the test world and the real sim runs it (Surfaces26 + Glide30); a takeoff on ice adds the
//   pumped SKATE run-up to every ground try; `verb: skate` is judged against a DRY control (the ice out); `--gate` / `--room-limit` (what-ifs).
//   A `grapple` gate is FLOWN: the rig's own Arcline21 (the game's pick) over the test world and the plan's nodes (rooms' grappleNodes too), the
//   sim's latch / pull / arrival / charges / gap; one or two hooks; margin = the farthest hook's range slack (and the carry when the last bite is not
//   the gate solid). The line-of-sight hook stays the cheat test on every other gate.
// The level-format notes for gate / ceiling / noride / flight / ice: Design/levels/n38/GATES38.md.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N38Gates(string cmd, string[] args, ref int code)
    {
        if (cmd != "n38" || args.Length < 2) return;
        int ti = Array.IndexOf(args, "--trace");
        if (ti >= 0 && ti + 1 < args.Length) n38Trace = args[ti + 1];
        if (args[1] == "envelopes") code = N38EnvelopesCommand(args);
        else if (args[1] == "limits") code = N38LimitsCommand(args);
        else if (args[1] == "gates") code = Array.IndexOf(args, "--selftest") >= 0 ? N38GatesSelftest(args) : N38GatesCommand(args);
    }

    // ================================================================================================ the rig (the real sim)

    /// <summary>The test worlds sit this far east of the survival map (its pads, zones, core cells and lava rects are all near the origin).</summary>
    static readonly Vec3 N38Off = new Vec3(2000, 0, 0);

    sealed class N38Rig
    {
        public SurvivalSim sim;
        public ScriptedInput inp;
        public readonly List<MoverEvent> log = new List<MoverEvent>();
        public int shots, shocks, pulses;
        public double dt;
        public bool ride38;
        public int steps;
        // NIGHT39 E2: the FLOWN grapple. The sim's own Arcline21 aims through the sim's world and map nodes, so the rig has its own hook (the real
        // Arcline21 pick: nodes, then surfaces under the crosshair) over a proxy world swapped to each test world, and a node list refilled per arena;
        // the pull, its arrival, the tap latch, the charges and the gap stay the sim's (grapple31 / grapple35 / grapple32)
        public N38WorldProxy proxy;
        public List<Vec3> hookNodes;
        public Arcline21 hook;
    }

    /// <summary>NIGHT39 E2: an ICollisionWorld that forwards to `inner` (the test world of the try).</summary>
    sealed class N38WorldProxy : ICollisionWorld
    {
        public ICollisionWorld inner;
        public int GatherCandidates(in Capsule capsule) => inner.GatherCandidates(capsule);
        public bool CapsuleIntersect(in Capsule capsule, int candidate, out CapsuleContact contact) => inner.CapsuleIntersect(capsule, candidate, out contact);
        public bool RayIntersect(Vec3 origin, Vec3 direction, out RayHit hit) => inner.RayIntersect(origin, direction, out hit);
    }

    static N38Rig N38NewRig(GameConfig cfg, bool ride38)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        if (ride38)
        {
            N38Blocks b = N38Blocks.ForceOff ? N38Blocks.Off : N38Blocks.From(cfg);
            b.ride38 = true;
            sim.n38Override = b;
        }
        if (N38GatesRide40) // NIGHT40 R2: ride40 forced on (n40ride gates / n38 gates --ride40): every ride holds, the attach asks for intent
        {
            N40Blocks b40 = N40Blocks.ForceOff ? N40Blocks.Off : N40Blocks.From(cfg);
            b40.ride40 = true;
            sim.n40Override = b40;
        }
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3801, null, null);
        sim.husks.frozen = true;
        sim.map.pads.Clear(); // no survival pad can fire under the test worlds
        var rig = new N38Rig { sim = sim, inp = (ScriptedInput)sim.input, dt = cfg.Dt, ride38 = ride38 };
        sim.mover.Events += e => rig.log.Add(e);
        sim.events.On<string>("fire", w => { if (w == "shotgun") rig.shots++; });
        sim.events.On<Pulse27Event>("pulse27Blast", _ => rig.pulses++);
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet"); // the PULSE gauntlet (its LT blast: the pulse tries)
        if (!sim.loadout.owned.Contains("shotgun")) sim.loadout.AddWeapon("shotgun");
        int slot = sim.loadout.owned.IndexOf("shotgun");
        for (int k = 0; k < 360 && !(sim.loadout.Weapon == "shotgun" && sim.loadout.ready); k++)
            N38Tick(rig, 0, 0, new InputFrame(), new CombatInput { weapon = k == 0 ? slot : -1 });
        Mover mv = sim.mover;
        double eyeH = cfg.movement.body.eyeHeight, drop = cfg.movement.slide.cameraDrop;
        rig.proxy = new N38WorldProxy { inner = mv.World };
        rig.hookNodes = new List<Vec3>();
        rig.hook = new Arcline21(cfg, sim.events, mv, sim.enemies, new GrappleAim(cfg.movement, rig.hookNodes), rig.proxy, () => new Vec3(mv.position.x, mv.position.y + eyeH - (mv.crouched ? drop : 0), mv.position.z));
        rig.hook.Reset(true); // the Mover's aimHook is the rig's pick from here; ropeEnabled stays false outside a grapple try
        return rig;
    }

    static void N38Tick(N38Rig r, double yaw, double pitch, InputFrame move, CombatInput combat)
    {
        r.inp.SetLook(yaw, pitch);
        r.inp.Set(move, combat);
        r.sim.hitstopLeft = 0;
        r.sim.health.hp = r.sim.health.max;
        r.sim.mover.invulnerable = true;
        r.sim.Step();
        r.steps++;
    }

    // ================================================================================================ the tries (verbs and combos)

    /// <summary>The verbs a level can own (`owns38`). jump and double are the jump's two presses; slide is the slide-hop's slide; boost the
    /// THUNDERCLAP's mid-air shot; ride the held wall ride (its jump-off and chains); walljump the tap wall jump; grapple the hook (tested by line
    /// of sight + range); shockwave the THUNDERCLAP's LT self push (only when Loadout.ShockwaveOn).</summary>
    static readonly string[] N38Atoms = { "jump", "double", "dash", "slide", "boost", "ride", "walljump", "grapple", "shockwave", "pulse" };

    static string N38Atom(string verb)
    {
        switch (verb)
        {
            case "jumpdash": case "dashjump": case "dash-jump": return "dashjump"; // the Mover's dash jump (JumpKind.Dash: the jump inside a dash)
            case "slidehop": return "slide";
            case "doublejump": return "double";
            case "shotgun": case "shotgunboost": return "boost";
            case "wallride": case "ridechain": return "ride";
            case "ice": case "glide": return "skate"; // NIGHT39 E2: the ice glide (a dash begun on ice adds speed: Core/Glide30.cs)
            default: return verb;
        }
    }

    sealed class N38Try
    {
        public string name;
        public HashSet<string> atoms;
        public List<(string act, double t)> seq = new List<(string, double)>(); // after the takeoff: jump / double / dash / boost / shock at t s
        public double slideLead; // > 0: the slide pressed this many metres before the lip (the slide-hop)
        public double dashLead; // > 0: the DASH-JUMP (the Mover's order: dash, then jump inside the dash): the dash pressed this many metres before the lip
        public int dashJumpSteps; // the jump this many steps after the dash press
        public string weapon; // the gun in hand for the try (null: the THUNDERCLAP); "gauntlet" for the PULSE blast
        public double pulsePitchDeg; // the PULSE blast's look pitch (the blast's self-boost goes ALONG the aim)
        public bool apexDouble; // the double jump pressed at the apex after the first shot (K3's ledge sequence)
        public bool shotAfterDouble; // the next shots only after that double jump (K3's order: shot, apex double, shot)
        public bool tapJumps; // every jump press released at once (no hold: no ride can start; the jump cut applies)
        public bool noSprint; // the run-up at walk speed (the sprint drains the bar a dash jump needs: design.stamina.boost.dashJumpExtra)
        // NIGHT39 E2, the flown grapple: the hook points (plan coordinates) fired in order, the first `hookT0` s after the jump at the lip, each next
        // one as soon as the rope is idle and the gap (design.n32.grapple32.minGapSeconds) has run; then `hookFollow` ("", "double", "dash")
        public List<Vec3> hooks;
        public List<string> hookNames;
        public double hookT0;
        public string hookFollow = "";
        public int faceTaps; // NIGHT39 E2 (K4's blastwall): after the PULSE blast, a jump press on each new wall contact in the air (a wall jump off the cliff face), at most this many
        public bool faceDouble; // then the double jump at the apex
        public bool pump; // NIGHT39 E2: the SKATE run-up: a dash on every free pip while grounded, no sprint, stopping N38PumpStop m before the lip (on ice each grounded dash ADDS design.n30.glide.dashAdd: Core/Glide30.cs)
        public int boostShots;
        public double boostPitchDeg;
        // ride / walljump tries
        public int rideSide; // -1 left, +1 right (the stick toward the wall), 0: no ride
        public double releaseLeft = double.NaN; // release the ride when this many metres remain to the target's near edge (NaN: after releaseAfter s)
        public double releaseAfter = 0.5;
        public double releasePitchDeg;
        public bool taps; // the walljump try
        public string follow; // after a ride's release: "", "double", "dash", "boost1", "boost2"
        public double chainLook = double.NaN; // the chain release: let go when no wall is `chainLook` m ahead along the travel (the wall's end)
        public bool chainSame; // the chain's next wall on the same side (else the other side)
        public double ridePitchDeg; // NIGHT40 R2: the look pitch WHILE riding (ride38 / ride40: + drifts the ride up to the wall's top)
        public bool releaseAtTop; // NIGHT40 R2: let go once the climb has stalled (the wall's top: the vertical speed 0 after 0.3 s on the wall)

        public override string ToString() => name;
    }

    static N38Try N38Seq(string name, string atoms, params (string, double)[] seq)
    {
        var t = new N38Try { name = name, atoms = new HashSet<string>(atoms.Split(' ', StringSplitOptions.RemoveEmptyEntries)) };
        t.seq.AddRange(seq);
        return t;
    }

    /// <summary>Every ground try (from standing ground at a lip) whose verbs are all owned.</summary>
    static List<N38Try> N38GroundTries(HashSet<string> owns)
    {
        var all = new List<N38Try>();
        all.Add(N38Seq("walk", ""));
        all.Add(N38Seq("jump", "jump", ("jump", 0)));
        foreach (double d in new[] { 0.2, 0.3, 0.4, 0.5, 0.6 }) all.Add(N38Seq($"double@{d:0.0#}", "jump double", ("jump", 0), ("double", d)));
        all.Add(N38Seq("dash", "dash", ("dash", 0)));
        foreach (double d in new[] { 0.02, 0.1, 0.2, 0.3, 0.45 }) all.Add(N38Seq($"jump+airdash@{d:0.0#}", "jump dash", ("jump", 0), ("dash", d)));
        // the dash jump in the Mover's order: the dash pressed `lead` m before the lip, the jump `j` steps into it (a jump inside the dash is
        // JumpKind.Dash, the DashJump event; dash.duration 0.25 s = 30 steps); the grid is C2's spire38 grid; then the double jump on top
        foreach (double lead in new[] { 0.2, 0.6, 1.0, 1.6, 2.4, 3.4 })
            foreach (int j in new[] { 1, 3, 6, 10, 15, 20, 25 })
            {
                var dj = N38Seq($"dashjump<{lead:0.0}m j{j}", "dash jump dashjump");
                dj.dashLead = lead;
                dj.dashJumpSteps = j;
                dj.noSprint = true; // a sprinted run-up can leave the bar short of the dash jump's extra
                all.Add(dj);
                if ((lead == 0.6 || lead == 1.6) && (j == 6 || j == 15))
                {
                    var ds = N38Seq($"dashjump<{lead:0.0}m j{j} sprinted", "dash jump dashjump");
                    ds.dashLead = lead;
                    ds.dashJumpSteps = j;
                    all.Add(ds);
                }
            }
        foreach (double lead in new[] { 1.0, 2.4 })
            foreach (int j in new[] { 6, 15 })
                foreach (double d in new[] { 0.45, 0.7 })
                {
                    var dj = N38Seq($"dashjump<{lead:0.0}m j{j}+double@{d:0.0#}", "dash jump dashjump double", ("double", d));
                    dj.dashLead = lead;
                    dj.dashJumpSteps = j;
                    dj.noSprint = true;
                    all.Add(dj);
                    // the same with tapped jumps: no hold, so no wall ride can start on the way (a ride-free line past ride walls)
                    var dt2 = N38Seq($"dashjump<{lead:0.0}m j{j}+double@{d:0.0#} tapped", "dash jump dashjump double", ("double", d));
                    dt2.dashLead = lead;
                    dt2.dashJumpSteps = j;
                    dt2.noSprint = true;
                    dt2.tapJumps = true;
                    all.Add(dt2);
                }
        foreach (double d in new[] { 0.3, 0.5 }) // the double jump tapped too (no ride on the way)
        {
            var tj = N38Seq($"double@{d:0.0#} tapped", "jump double", ("jump", 0), ("double", d));
            tj.tapJumps = true;
            all.Add(tj);
        }
        foreach (double d in new[] { 0.35, 0.5, 0.65 }) all.Add(N38Seq($"jump+airdash+double@{d:0.0#}", "jump dash double", ("jump", 0), ("dash", 0.1), ("double", d)));
        foreach (double d in new[] { 0.45, 0.6, 0.8 }) all.Add(N38Seq($"double+dash@{d:0.0#}", "jump double dash", ("jump", 0), ("double", 0.35), ("dash", d)));
        foreach (double lead in new[] { 1.5, 3.0, 5.0 })
        {
            var s = N38Seq($"slidehop<{lead:0.#}m", "slide jump", ("jump", 0));
            s.slideLead = lead;
            all.Add(s);
            var s2 = N38Seq($"slidehop<{lead:0.#}m+double", "slide jump double", ("jump", 0), ("double", 0.4));
            s2.slideLead = lead;
            all.Add(s2);
            var s3 = N38Seq($"slidehop<{lead:0.#}m+dash", "slide jump dash", ("jump", 0), ("dash", 0.15));
            s3.slideLead = lead;
            all.Add(s3);
        }
        foreach (int n in new[] { 1, 2, 3 })
            foreach (double p in new[] { -89.0, -65, -45, -25, -10 })
                foreach (double t0 in new[] { 0.05, 0.3 })
                {
                    var b = N38Seq($"boost x{n} p{p:0} @{t0:0.0#}", "jump boost", ("jump", 0), ("boost", t0));
                    b.boostShots = n;
                    b.boostPitchDeg = p;
                    all.Add(b);
                }
        // K3's ledge sequence (a 9-10 m ledge): jump, shoot straight down at 0.05 s, the double jump at the apex, shoot down again at the gun's
        // own fire interval (0.9 s), (a third shot at the next interval)
        foreach (int n in new[] { 2, 3 })
            foreach (double p in new[] { -89.0, -80, -70 })
            {
                var k3 = N38Seq($"boost x{n} p{p:0} @0.05+double@apex", "jump boost double", ("jump", 0), ("boost", 0.05));
                k3.boostShots = n;
                k3.boostPitchDeg = p;
                k3.apexDouble = true;
                all.Add(k3);
                var k4 = N38Seq($"boost x{n} p{p:0} @0.05+double@apex then shot", "jump boost double", ("jump", 0), ("boost", 0.05));
                k4.boostShots = n;
                k4.boostPitchDeg = p;
                k4.apexDouble = true;
                k4.shotAfterDouble = true;
                all.Add(k4);
            }
        foreach (int n in new[] { 1, 3 })
            foreach (double p in new[] { -89.0, -45, -10 })
            {
                var b = N38Seq($"double+boost x{n} p{p:0}", "jump double boost", ("jump", 0), ("double", 0.35), ("boost", 0.45));
                b.boostShots = n;
                b.boostPitchDeg = p;
                all.Add(b);
                var c = N38Seq($"jump+airdash+boost x{n} p{p:0}", "jump dash boost", ("jump", 0), ("dash", 0.1), ("boost", 0.35));
                c.boostShots = n;
                c.boostPitchDeg = p;
                all.Add(c);
                var e = N38Seq($"boost x{n} p{p:0}+double", "jump boost double", ("jump", 0), ("boost", 0.05), ("double", 0.6));
                e.boostShots = n;
                e.boostPitchDeg = p;
                all.Add(e);
            }
        // the PULSE gauntlet's LT blast (a full glove: once per charge), the gauntlet in hand: from the jump, with the double, after a dash jump, and
        // from the ground at the lip; its self-boost (boostSpeed over boostSeconds through Mover.Impulse) goes along the look
        foreach (double p in new[] { 0.0, 15, 30, 45, 70, 80, 89 })
            foreach (double at in new[] { 0.05, 0.3 })
            {
                var b = N38Seq($"pulse p{p:0} @{at:0.0#}", "jump pulse", ("jump", 0), ("pulse", at));
                b.weapon = "gauntlet";
                b.pulsePitchDeg = p;
                all.Add(b);
                var b2 = N38Seq($"pulse p{p:0} @{at:0.0#}+double", "jump pulse double", ("jump", 0), ("pulse", at), ("double", at + 0.45));
                b2.weapon = "gauntlet";
                b2.pulsePitchDeg = p;
                all.Add(b2);
            }
        foreach (double p in new[] { 15.0, 30, 45 })
        {
            var g0 = N38Seq($"pulse ground p{p:0}", "pulse", ("pulse", 0));
            g0.weapon = "gauntlet";
            g0.pulsePitchDeg = p;
            all.Add(g0);
            var dp = N38Seq($"dashjump+pulse p{p:0}", "dash jump dashjump pulse", ("pulse", 0.3));
            dp.dashLead = 1.0;
            dp.dashJumpSteps = 15;
            dp.noSprint = true;
            dp.weapon = "gauntlet";
            dp.pulsePitchDeg = p;
            all.Add(dp);
            var dd = N38Seq($"double+pulse p{p:0}", "jump double pulse", ("jump", 0), ("double", 0.35), ("pulse", 0.6));
            dd.weapon = "gauntlet";
            dd.pulsePitchDeg = p;
            all.Add(dd);
        }
        // NIGHT39 E2 (K4's `n38kiln blastwall`): the PULSE blast, then wall jumps off the cliff face (the Mover allows a wall jump off a noride wall).
        // Reactive: a press on each new wall contact after the blast (1 or 2), then the double at the apex or not. Timed (K4's grid, thinned): the
        // press d1 s after the blast, a second d2 s later (none / +0.3 s); near the face a press is a wall jump, in the open the double
        foreach (double p in new[] { 30.0, 50, 70, 89 })
            foreach (double at in new[] { 0.05, 0.15, 0.3 })
                foreach (int n in new[] { 1, 2 })
                    foreach (bool dbl in new[] { false, true })
                    {
                        var bw = N38Seq($"pulse p{p:0} @{at:0.0#} + face x{n}{(dbl ? " + apex double" : "")}", dbl ? "jump pulse walljump double" : "jump pulse walljump", ("jump", 0), ("pulse", at));
                        bw.weapon = "gauntlet";
                        bw.pulsePitchDeg = p;
                        bw.faceTaps = n;
                        bw.faceDouble = dbl;
                        all.Add(bw);
                    }
        foreach (double p in new[] { 30.0, 50, 70, 89 })
            foreach (double at in new[] { 0.05, 0.3 })
                foreach (double d1 in new[] { 0.2, 0.4, 0.6, 0.8, 1.0, 1.2 })
                    foreach (double d2 in new[] { 0.0, 0.3 })
                    {
                        var seq = new List<(string, double)> { ("jump", 0), ("pulse", at), ("double", at + d1) };
                        if (d2 > 0) seq.Add(("double", at + d1 + d2));
                        var bt = N38Seq($"pulse p{p:0} @{at:0.0#} + press +{d1:0.0#}{(d2 > 0 ? $" + press +{d2:0.0#}" : "")}", "jump pulse double walljump", seq.ToArray());
                        bt.weapon = "gauntlet";
                        bt.pulsePitchDeg = p;
                        all.Add(bt);
                    }
        foreach (double p in new[] { -89.0, -45, -10 }) // the shockwave's self push (live only with design.n25.shockwaveOff off)
        {
            var s = N38Seq($"shock p{p:0}", "jump shockwave", ("jump", 0), ("shock", 0.1));
            s.boostPitchDeg = p;
            all.Add(s);
        }
        // every sprinted try that dashes, again with a walked run-up: the sprint drains the stamina bar the dash (and the dash jump's extra) spend
        int n0 = all.Count;
        for (int i = 0; i < n0; i++)
        {
            N38Try t = all[i];
            if (!t.atoms.Contains("dash") || t.noSprint || t.dashLead > 0) continue;
            var w = N38Seq(t.name + " walked", string.Join(" ", t.atoms));
            w.seq.AddRange(t.seq);
            w.slideLead = t.slideLead;
            w.boostShots = t.boostShots;
            w.boostPitchDeg = t.boostPitchDeg;
            w.noSprint = true;
            w.weapon = t.weapon;
            w.pulsePitchDeg = t.pulsePitchDeg;
            all.Add(w);
        }
        return all.FindAll(t => t.atoms.All(a => owns.Contains(a) || a == "dashjump" && owns.Contains("dash") && owns.Contains("jump")));
    }

    /// <summary>Ride tries toward a target `gap` m ahead (the release `releaseLeft` m before the near edge), both sides, pitch -45 / 0 / +45,
    /// each release followed 0.25 s later by the jump pressed again (the next wall's ride, or the double jump); plus the tap wall jumps.</summary>
    /// <summary>NIGHT40 R2: the rig runs ride40 (`--ride40`, or n40ride gates): every ride holds and the checker adds THE CLIMB (below).</summary>
    static bool N38GatesRide40;

    static List<N38Try> N38RideTries(HashSet<string> owns)
    {
        var all = new List<N38Try>();
        // NIGHT40 R2, the endless ride's own move (L-042 / L-061): a ride that never ends on its own can first CLIMB (the look pitched up drifts it up,
        // ride38's rule, to the wall's top) and only then let go: the release at the top (the rise stalled) or at the usual distances before the near
        // edge, looking up +45 (the jump-off's most up speed) or level, with or without the re-press; and a ride held to the wall's end looking up.
        if (N38GatesRide40 && owns.Contains("ride"))
            foreach (int side in new[] { -1, 1 })
            {
                foreach (double rp in new[] { 45.0, 25 })
                    foreach (double p in new[] { 45.0, 0 })
                        foreach (bool re in new[] { true, false })
                        {
                            if (re && !owns.Contains("double")) continue;
                            all.Add(new N38Try { name = $"climb {(side < 0 ? "L" : "R")} p{rp:0} to the top, release p{p:0}{(re ? "" : " (no re-press)")}", atoms = re ? new HashSet<string> { "jump", "ride", "double" } : new HashSet<string> { "jump", "ride" }, rideSide = side, ridePitchDeg = rp, releaseAtTop = true, releasePitchDeg = p, follow = re ? "" : "none" });
                            foreach (double left in new[] { 1.0, 5 })
                                all.Add(new N38Try { name = $"climb {(side < 0 ? "L" : "R")} p{rp:0} release {left:0}m p{p:0}{(re ? "" : " (no re-press)")}", atoms = re ? new HashSet<string> { "jump", "ride", "double" } : new HashSet<string> { "jump", "ride" }, rideSide = side, ridePitchDeg = rp, releaseLeft = left, releasePitchDeg = p, follow = re ? "" : "none" });
                        }
                all.Add(new N38Try { name = $"climb {(side < 0 ? "L" : "R")} p45 to the wall's end", atoms = new HashSet<string> { "jump", "ride", "double" }, rideSide = side, ridePitchDeg = 45, releaseLeft = double.NaN, chainLook = 0.5, releasePitchDeg = 45 });
            }
        if (owns.Contains("ride"))
            foreach (int side in new[] { -1, 1 })
                foreach (double left in new[] { 1.0, 3, 5, 8, 12 })
                    foreach (double p in new[] { -45.0, 0, 45 })
                    {
                        var t = new N38Try { name = $"ride {(side < 0 ? "L" : "R")} release {left:0}m p{p:0}", atoms = new HashSet<string> { "jump", "ride", "double" }, rideSide = side, releaseLeft = left, releasePitchDeg = p };
                        if (owns.Contains("double")) all.Add(t);
                        var u = new N38Try { name = t.name + " (no re-press)", atoms = new HashSet<string> { "jump", "ride" }, rideSide = side, releaseLeft = left, releasePitchDeg = p, follow = "none" };
                        all.Add(u);
                    }
        // the CHAIN: every ride let go where its wall ends (a probe `look` m ahead along the travel finds no wall), the jump pressed again 0.25 s
        // later to catch the next wall (the stick toward the other side, or the same side), each wall the same way, to the landing
        if (owns.Contains("ride"))
            foreach (int side in new[] { -1, 1 })
                foreach (double look in new[] { 0.5, 1.5, 3.0 })
                    foreach (double p in new[] { -20.0, 0, 30 })
                        foreach (bool same in new[] { false, true })
                            all.Add(new N38Try { name = $"ride chain {(side < 0 ? "L" : "R")}{(same ? " same-side" : "")} wall-end {look:0.0}m p{p:0}", atoms = new HashSet<string> { "jump", "ride", "double" }, rideSide = side, releaseLeft = double.NaN, chainLook = look, chainSame = same, releasePitchDeg = p });
        if (owns.Contains("walljump"))
            foreach (int side in new[] { -1, 1 })
                all.Add(new N38Try { name = $"walljump taps {(side < 0 ? "L" : "R")}", atoms = new HashSet<string> { "jump", "walljump" }, rideSide = side, taps = true });
        return all;
    }

    // ================================================================================================ one attempt

    sealed class N38Trace
    {
        public bool started, landed, dropped, rode, aborted;
        public string note = "";
        public double reachAtTop = double.NegativeInfinity; // the farthest progress along the line while the feet are at or above topY
        public double reachAtTop2 = double.NegativeInfinity; // the same at topY - 2
        public double apex = double.NegativeInfinity; // the most the feet rose over the takeoff point (or the release point for ride envelopes)
        public double landProg, landY;
        public Vec3 landPos;
        public int rides, shots;
        public int wallJumps, pulses;
        public bool dashJumped; // a DashJump event (the Mover's JumpKind.Dash) happened in this try
        public int ropeFires, ropeReleases; // NIGHT39 E2: hooks the Mover fired (RopeFire) and let go
        public double hookFarthest; // the longest eye-to-hook distance at a fire
        public bool hookOnGate; // a hook landed on the gate solid itself
        public int iceDashAdds; // NIGHT39 E2: dashes begun on ice that ADDED speed (Glide30.dashAdds over the try, the run-up included): the skate
        public double lipSpeed; // the horizontal speed at the takeoff
        // ride envelopes: measured from the release point
        public Vec3 release;
        public bool released;
        public double outAtRelease = double.NegativeInfinity, alongAtRelease = double.NegativeInfinity, apexOverRelease = double.NegativeInfinity;
    }

    /// <summary>The world the attempt runs in plus the solids that may be `noride` (a ride or a wall jump on one drops the attempt: until builder
    /// M's Mover flag lands, the checker treats a noride wall as unrideable by dropping any try that rides it).</summary>
    sealed class N38Arena
    {
        public TriangleWorld world;
        public List<N38Solid> noride = new List<N38Solid>();
        /// <summary>NIGHT39 E2: the level's ICE / MUD / BOOST surfaces near the gate, moved to the test world (null: none, or the dry control). The
        /// rig's sim runs them through its own Surfaces26 (friction) and Glide30 (design.n30.glide: the glide decay, the dash that adds speed).</summary>
        public Surfaces26 surf;
        /// <summary>NIGHT39 E2: the plan's grapple nodes near the gate (test-world coordinates): the rig's hook sees them.</summary>
        public List<Vec3> nodes = new List<Vec3>();
        /// <summary>NIGHT39 E2: the gate solid in this world (test-world box; null in the free world): a hook on it is "on the gate".</summary>
        public Vec3? gateMin, gateMax;

        public bool IceAt(Vec3 feetTestWorld)
        {
            if (surf == null) return false;
            foreach (SurfaceZone z in surf.zones)
                if (z.kind == SurfaceKind.Ice && z.Contains(feetTestWorld.x, feetTestWorld.y, feetTestWorld.z)) return true;
            return false;
        }
    }

    /// <summary>The skate run-up stops pumping this many metres before the lip (a pip refills for the air; S2's iceline38 skater uses the same 12 m).</summary>
    const double N38PumpStop = 12, N38PumpStopDash = 30;

    /// <summary>One attempt: the Mover placed at `start` (feet, world coordinates), settled, then running along `dir` (unit, horizontal) to the
    /// takeoff `O`; the try's inputs from there. Ends at the first landing after the takeoff, a drop under topY - 8, a noride ride, or maxAfter s.
    /// `wallOut`: for ride envelopes, the unit vector out of the ridden wall (the release's out distance is measured along it).</summary>
    static N38Trace N38Run(N38Rig rig, N38Arena arena, Vec3 start, Vec3 O, Vec3 dir, N38Try t, double topY, double edge, double maxAfter, Vec3? wallOut = null)
    {
        var tr = new N38Trace();
        SurvivalSim sim = rig.sim;
        Mover m = sim.mover;
        double dt = rig.dt;
        int S(double s) => MovementMath.StepsFor(s, dt);
        double yawF = Math.Atan2(-dir.x, -dir.z);
        m.World = arena.world;
        sim.surfaces26 = arena.surf; // NIGHT39 E2: the gate's ice (null: dry ground, friction x1)
        if (arena.surf != null) arena.surf.Reset(m);
        else m.groundFrictionMult = 1;
        // NIGHT39 E2: the rig's hook sees this world and its nodes; owned only in a grapple try (no other try can fire it)
        rig.proxy.inner = arena.world;
        rig.hookNodes.Clear();
        rig.hookNodes.AddRange(arena.nodes);
        m.ropeEnabled = false;
        m.SetNoRide38(arena.noride.Count > 0 ? arena.noride.ConvertAll(s => new Ride38.Box { min = s.min + N38Off, max = s.max + N38Off }).ToArray() : null);
        m.Reset(start.x, start.y, start.z);
        m.slideCharge = 1;
        // the gun the try needs in hand (the gauntlet for the PULSE blast, the THUNDERCLAP otherwise), ready
        string want = t.weapon ?? "shotgun";
        int wantSlot = sim.loadout.owned.IndexOf(want);
        for (int k = 0; k < 120 || !(sim.loadout.ready && m.grounded && (wantSlot < 0 || sim.loadout.Weapon == want)); k++)
        {
            N38Tick(rig, yawF, 0, new InputFrame(), new CombatInput { weapon = sim.loadout.Weapon != want && wantSlot >= 0 && k % 30 == 0 ? wantSlot : -1 });
            if (k > 600) { tr.aborted = true; tr.note = "never settled on the ground at the run-up start"; return tr; }
        }
        PulseCharge27 glove = sim.loadout.pulse27;
        if (glove != null)
        {
            glove.pips = glove.p.pips; // a full glove and no cooldown: one blast per try (once per charge)
            glove.cooldownSteps = 0;
            glove.boostSteps = 0;
        }
        // placed again after the gun's wait (the wait can drift the body), then 0.1 s to stand
        m.Reset(start.x, start.y, start.z);
        m.slideCharge = 1;
        for (int k = 0; k < 12; k++) N38Tick(rig, yawF, 0, new InputFrame(), new CombatInput { weapon = -1 });
        Vec3 settled = m.position - start;
        if (Math.Abs(settled.y + 0.05) > 0.3 || Math.Sqrt(settled.x * settled.x + settled.z * settled.z) > 0.5)
        {
            tr.aborted = true;
            tr.note = $"the run-up start is not standing ground at its height (settled {F35(settled.x)}, {F35(settled.y)}, {F35(settled.z)} m off)";
            return tr;
        }
        rig.log.Clear();
        sim.glide30?.Reset(); // the glide's last-step memory from the previous try (teleported) must not carry
        int glide0 = sim.glide30?.dashAdds ?? 0, pumpCool = 0;
        // NIGHT39 E2: a grapple try starts with the hook owned, the charges full, the tap latch clear
        int hookIdx = 0, lastFire = -100000, hookDone = -1, lastPress = -100000;
        int faceTapsDone = 0, lastFaceTap = -100000;
        bool faceDoubleDone = false;
        int gapSteps = sim.grapple32Rules != null ? S(sim.grapple32Rules.minGapSeconds) : 0;
        if (t.hooks != null)
        {
            m.ropeEnabled = true;
            if (sim.Grapple32On) sim.grappleCharges32 = sim.grapple32Rules.charges;
            if (sim.grapplePull31 != null) sim.grappleTap31.Reset(S(sim.grapplePull31.tapWindow));
        }
        int shots0 = rig.shots, logAt = 0, pulses0 = rig.pulses;
        int t0 = -1, lastJump = -1, released = -1, rideAge = 0, tapUntil = -1, lastTap = -1000;
        bool slid = false, boostOn = false, rePressed = false, wasAir = false;
        int dashAt = -1;
        bool apexDone = false;
        int At(double s) => s <= 0 ? 0 : S(s); // a verb at t = 0 fires on the takeoff step itself (StepsFor never returns 0)
        double side = t.rideSide, y0 = O.y;
        int maxRun = S(12), maxAll = maxRun + S(maxAfter);
        for (int i = 0; i < maxAll; i++)
        {
            Vec3 rel = m.position - O;
            double prog = rel.x * dir.x + rel.z * dir.z;
            if (t0 < 0 && prog >= -0.1) { t0 = i; tr.started = true; y0 = m.position.y; shots0 = rig.shots; pulses0 = rig.pulses; tr.lipSpeed = Math.Sqrt(m.velocity.x * m.velocity.x + m.velocity.z * m.velocity.z); } // only the shots and blasts from the takeoff on count (a stale press can fire in the run-up)
            if (t0 < 0 && i > maxRun) { tr.aborted = true; tr.note = $"never reached the takeoff (stopped at progress {F35(prog)} m)"; return tr; }
            int k = t0 < 0 ? -1 : i - t0;
            var f = new InputFrame { moveZ = 1 };
            var c = new CombatInput { weapon = -1 };
            double yaw = yawF, pitch = 0;
            if (k < 0)
            {
                f.sprint = new Button { held = !t.noSprint };
                if (t.slideLead > 0 && prog >= -t.slideLead)
                {
                    f.slide = new Button { held = true, pressed = !slid };
                    slid = true;
                    f.sprint = default;
                }
                if (t.rideSide != 0) f.moveX = 0;
                if (t.pump)
                {
                    // the skate: no sprint (it drains the bar the dash spends), a FULL boost whenever the bar holds one (Mover.BoostReady: design.stamina
                    // boost.cost 0.5 of a 1.0 bar) while grounded, until N38PumpStop m before the lip; a try that dashes itself (a dash jump: cost +
                    // dashJumpExtra) stops N38PumpStopDash m before it so the bar refills for its own dash (refill 0.5 / s after 0.4 s)
                    f.sprint = default;
                    if (pumpCool > 0) pumpCool--;
                    double stop = t.dashLead > 0 || t.seq.Exists(s => s.act == "dash") ? N38PumpStopDash : N38PumpStop;
                    if (prog < -stop && pumpCool == 0 && m.grounded && m.BoostReady(1, false) && !f.slide.held) { f.dash = new Button { held = true, pressed = true }; pumpCool = S(0.35); }
                }
            }
            else if (t.rideSide != 0 && !t.taps)
            {
                // the held ride: jump pressed at the lip and held, the stick toward the wall; released at the rule, re-pressed 0.25 s later
                bool riding = m.state == MoveState.WallRide;
                rideAge = riding ? rideAge + 1 : 0;
                if (riding && released < 0)
                {
                    // the wall's side from its normal (the normal points away from the wall): the stick keeps pushing into it
                    double nr = m.wallNormal.x * -dir.z + m.wallNormal.z * dir.x;
                    if (Math.Abs(nr) > 0.2) side = nr > 0 ? -1 : 1;
                }
                f.moveX = side * 0.6;
                bool held = true;
                if (released < 0) pitch = t.ridePitchDeg * Math.PI / 180; // NIGHT40 R2: the look while riding (0 for every older try)
                if (k == 0) f.jump = new Button { held = true, pressed = true };
                if (released < 0 && riding)
                {
                    bool rel1;
                    if (t.releaseAtTop) rel1 = rideAge >= S(0.3) && Math.Abs(m.velocity.y) < 1e-9 || rideAge >= S(8); // NIGHT40 R2: the climb stalled at the wall's top
                    else if (!double.IsNaN(t.chainLook)) rel1 = N38WallEnds(m, arena.world, t.chainLook, rig.sim.cfg.movement.body.capsuleRadius) || prog >= edge + 1 || rideAge >= S(8);
                    else rel1 = double.IsNaN(t.releaseLeft) ? rideAge >= S(t.releaseAfter) : edge - prog <= t.releaseLeft || rideAge >= S(8);
                    if (rel1)
                    {
                        released = i;
                        if (!tr.released) { tr.released = true; tr.release = m.position; }
                    }
                }
                if (released >= 0)
                {
                    int since = i - released;
                    pitch = t.releasePitchDeg * Math.PI / 180;
                    f.moveX = (t.chainSame ? side : -side) * 0.7;
                    held = false;
                    if (since >= S(0.25) && t.follow != "none")
                    {
                        if (string.IsNullOrEmpty(t.follow) || t.follow == "double")
                        {
                            held = true;
                            if (!rePressed) { f.jump = new Button { held = true, pressed = true }; rePressed = true; }
                        }
                    }
                    if (t.follow == "dash" && since == S(0.15)) f.dash = new Button { held = true, pressed = true };
                    if ((t.follow == "boost1" || t.follow == "boost2") && since >= S(0.1) && rig.shots - shots0 < (t.follow == "boost1" ? 1 : 2))
                    {
                        // the shot looks back along the travel and down 45 deg: the push goes out and up
                        Vec3 travel = new Vec3(m.velocity.x, 0, m.velocity.z);
                        double yawT = travel.Length > 0.1 ? Math.Atan2(-travel.x, -travel.z) : yawF;
                        yaw = yawT + Math.PI;
                        pitch = -45 * Math.PI / 180;
                        c.fire = new Button { held = true, pressed = true };
                    }
                    if (riding && since > S(0.3)) { released = -1; rePressed = false; rideAge = 1; } // a new wall caught (the chain): ride it the same way (its side from its normal, above)
                }
                if (!f.jump.pressed) f.jump = new Button { held = held };
            }
            else if (t.taps)
            {
                // the web's wall jumps: a tap (pressed, held 0.1 s) on every new wall contact in the air, the stick toward the wall
                f.moveX = side * 0.7;
                bool near = m.touchingWall || m.state == MoveState.WallRide;
                if (k == 0) { f.jump = new Button { held = true, pressed = true }; tapUntil = i + S(0.3); lastTap = i; }
                else if (near && !m.grounded && i - lastTap > S(0.15)) { f.jump = new Button { held = true, pressed = true }; tapUntil = i + S(0.1); lastTap = i; side = -side; }
                else f.jump = new Button { held = i < tapUntil };
            }
            else if (t.hooks != null)
            {
                // NIGHT39 E2, the flown grapple: the jump at the lip (held while rising), then each hook point in turn: look at it from the eye and press
                // the grapple (a fresh press every other step until the Mover fires: RopeFire), the next one once the rope is idle and the gap has run
                if (k == 0) { f.jump = new Button { held = true, pressed = true }; lastJump = i; }
                else f.jump = new Button { held = m.velocity.y > 0 && hookIdx == 0 };
                if (hookIdx < t.hooks.Count && k >= At(t.hookT0) && m.rope.phase == RopePhase.None && i - lastFire > gapSteps + 1)
                {
                    Vec3 eye = m.position + new Vec3(0, rig.sim.cfg.movement.body.eyeHeight, 0);
                    Vec3 d = t.hooks[hookIdx] + N38Off - eye;
                    double hz = Math.Sqrt(d.x * d.x + d.z * d.z);
                    yaw = Math.Atan2(-d.x, -d.z);
                    pitch = Math.Atan2(d.y, hz);
                    if (i - lastPress >= 2) { f.grapple = new Button { held = true, pressed = true }; lastPress = i; }
                }
                if (hookIdx >= t.hooks.Count && hookDone < 0 && m.rope.phase == RopePhase.None) hookDone = i;
                if (hookDone >= 0 && i - hookDone == S(0.1))
                {
                    if (t.hookFollow == "double") { f.jump = new Button { held = true, pressed = true }; lastJump = i; }
                    if (t.hookFollow == "dash") f.dash = new Button { held = true, pressed = true };
                }
                else if (hookDone >= 0 && t.hookFollow == "double" && lastJump >= 0 && i - lastJump < S(0.3)) f.jump = new Button { held = true };
            }
            else
            {
                foreach (var (act, at) in t.seq)
                {
                    if (k != At(at)) continue;
                    if (act == "jump" || act == "double") { f.jump = new Button { held = true, pressed = true }; lastJump = i; }
                    else if (act == "dash") f.dash = new Button { held = true, pressed = true };
                    else if (act == "boost") boostOn = true;
                    else if (act == "shock") c.special = new Button { held = true, pressed = true };
                    else if (act == "pulse")
                    {
                        // the PULSE gauntlet's LT blast (design.n27.pulse27): its self-boost goes along the aim, so look ahead and up
                        c.altFire = new Button { held = true, pressed = true };
                        pitch = t.pulsePitchDeg * Math.PI / 180;
                    }
                }
                if (t.faceTaps > 0 && rig.pulses - pulses0 >= 1 && !m.grounded)
                {
                    // NIGHT39 E2 (K4's blastwall): the blast went; a press on each new contact with a wall (a wall jump off the noride cliff face: the Mover
                    // allows it), then the double at the apex
                    if (faceTapsDone < t.faceTaps && (m.touchingWall || m.state == MoveState.WallRide) && i - lastFaceTap > S(0.15))
                    {
                        f.jump = new Button { held = true, pressed = true };
                        lastJump = lastFaceTap = i;
                        faceTapsDone++;
                    }
                    else if (t.faceDouble && !faceDoubleDone && faceTapsDone > 0 && i - lastFaceTap > S(0.15) && m.velocity.y <= 0)
                    {
                        f.jump = new Button { held = true, pressed = true };
                        lastJump = i;
                        faceDoubleDone = true;
                    }
                }
                if (t.apexDouble && !apexDone && rig.shots - shots0 >= 1 && m.velocity.y <= 0 && !m.grounded)
                {
                    f.jump = new Button { held = true, pressed = true };
                    lastJump = i;
                    apexDone = true;
                }
                if (!f.jump.pressed && lastJump >= 0) f.jump = new Button { held = !t.tapJumps && (t.apexDouble || i - lastJump < S(0.05) || m.velocity.y > 0) };
                bool shockStep = t.seq.Exists(s => s.act == "shock" && At(s.t) == k);
                if (boostOn && rig.shots - shots0 < t.boostShots && !(t.shotAfterDouble && rig.shots - shots0 >= 1 && !apexDone))
                {
                    yaw = yawF + Math.PI;
                    pitch = t.boostPitchDeg * Math.PI / 180;
                    f.moveZ = -1;
                    c.fire = new Button { held = (i & 1) == 0, pressed = (i & 1) == 0 }; // a fresh pull every other step (a held trigger does not re-fire the THUNDERCLAP)
                }
                else if (shockStep)
                {
                    yaw = yawF + Math.PI;
                    pitch = t.boostPitchDeg * Math.PI / 180;
                    f.moveZ = -1;
                }
            }
            if (t.dashLead > 0)
            {
                // the dash jump (the Mover's order): the dash pressed dashLead m before the lip, the jump dashJumpSteps later (before or after the lip)
                if (dashAt < 0 && prog >= -t.dashLead)
                {
                    f.dash = new Button { held = true, pressed = true };
                    f.sprint = default;
                    dashAt = i;
                }
                if (dashAt >= 0 && i == dashAt + t.dashJumpSteps) { f.jump = new Button { held = true, pressed = true }; lastJump = i; }
                else if (!f.jump.pressed && lastJump >= 0) f.jump = new Button { held = !t.tapJumps && (t.apexDouble || i - lastJump < S(0.05) || m.velocity.y > 0) };
            }
            N38Tick(rig, yaw, pitch, f, c);
            if (n38Trace != null && t.name == n38Trace && k >= 0 && k < 300 && k % 6 == 0)
                Console.WriteLine($"      k {k} y {F35(m.position.y - y0)} vy {F35(m.velocity.y)} jump {(f.jump.pressed ? "PRESS" : f.jump.held ? "held" : "-")} fire {(c.fire.pressed ? "x" : "-")} shots {rig.shots - shots0} hand {sim.loadout.Weapon} ready {sim.loadout.ready} {m.state} z {F35(m.position.z - O.z)} rope {m.rope.phase} fires {tr.ropeFires} charges {sim.grappleCharges32}{(f.grapple.pressed ? " GRAPPLE" : "")} x {F35((m.position - O).x * -dir.z + (m.position - O).z * dir.x)} touch {m.touchingWall} r40 refused low {m.flow34.ride35.r40Low} slow {m.flow34.ride35.r40Slow} stick {m.flow34.ride35.r40NoIntent} short {m.flow34.ride35.r40Short} lock {m.flow34.ride35.r40Locked}");
            // events: rides (and a noride surface under them)
            for (; logAt < rig.log.Count; logAt++)
            {
                MoverEvent e = rig.log[logAt];
                if (e.type == MoverEventType.DashJump) tr.dashJumped = true;
                if (e.type == MoverEventType.WallJump) tr.wallJumps++;
                if (e.type == MoverEventType.RopeFire)
                {
                    // NIGHT39 E2: a hook went out (the charge is spent): its distance from the eye, and whether it bit the gate solid
                    tr.ropeFires++;
                    hookIdx++;
                    lastFire = i;
                    Vec3 eye = m.position + new Vec3(0, rig.sim.cfg.movement.body.eyeHeight, 0);
                    tr.hookFarthest = Math.Max(tr.hookFarthest, (e.point - eye).Length);
                    if (arena.gateMin.HasValue)
                    {
                        Vec3 lo = arena.gateMin.Value, hi = arena.gateMax.Value, q = e.point;
                        if (q.x >= lo.x - 0.1 && q.x <= hi.x + 0.1 && q.y >= lo.y - 0.1 && q.y <= hi.y + 0.1 && q.z >= lo.z - 0.1 && q.z <= hi.z + 0.1) tr.hookOnGate = true;
                    }
                }
                if (e.type == MoverEventType.RopeRelease)
                {
                    tr.ropeReleases++;
                    if (n38Trace != null && t.name == n38Trace) Console.WriteLine($"      k {k} RopeRelease '{e.reason}' at z {F35(m.position.z - O.z)} y {F35(m.position.y - y0)} speed {F35(e.speed)}");
                }
                if (e.type != MoverEventType.WallRideStart && e.type != MoverEventType.WallJump) continue;
                if (e.type == MoverEventType.WallRideStart) { tr.rides++; tr.rode = true; }
                // noride: the real Mover's flag (Mover.SetNoRide38, builder M) decides; a ride that still starts on a noride solid is reported
                N38Solid nr = e.type == MoverEventType.WallRideStart ? N38WallSolid(arena, m, rig.sim.cfg.movement.body.capsuleRadius) : null;
                if (nr != null) tr.note = $"a ride started on the noride solid {nr.Label} (the Mover's noride flag did not stop it)";
            }
            if (tr.dropped) break;
            Vec3 p = m.position;
            Vec3 rp = p - O;
            prog = rp.x * dir.x + rp.z * dir.z;
            if (t0 >= 0)
            {
                tr.apex = Math.Max(tr.apex, p.y - y0);
                if (p.y >= topY - 0.05) tr.reachAtTop = Math.Max(tr.reachAtTop, prog);
                if (p.y >= topY - 2.05) tr.reachAtTop2 = Math.Max(tr.reachAtTop2, prog);
                if (tr.released && wallOut.HasValue)
                {
                    Vec3 d = p - tr.release;
                    tr.apexOverRelease = Math.Max(tr.apexOverRelease, d.y);
                    if (p.y >= tr.release.y - 0.05)
                    {
                        tr.outAtRelease = Math.Max(tr.outAtRelease, d.x * wallOut.Value.x + d.z * wallOut.Value.z);
                        tr.alongAtRelease = Math.Max(tr.alongAtRelease, d.x * dir.x + d.z * dir.z);
                    }
                }
                if (!m.grounded) wasAir = true;
                if (!wasAir && k > S(1.5)) break; // never left the ground (a walk or a dash along a floor): no flight
                if (wasAir && m.grounded)
                {
                    tr.landed = true;
                    tr.landProg = prog;
                    tr.landY = p.y;
                    tr.landPos = p;
                    break;
                }
                if (p.y < topY - 8 && p.y < y0 - 8) break;
            }
        }
        tr.shots = rig.shots - shots0;
        tr.pulses = rig.pulses - pulses0;
        tr.iceDashAdds = (sim.glide30?.dashAdds ?? 0) - glide0;
        m.ropeEnabled = false;
        return tr;
    }

    /// <summary>The chain release: does the wall the Mover rides END within `look` m ahead along its travel? (A ray from the capsule's middle,
    /// `look` m ahead, into the wall finds nothing within the radius + 0.6 m.)</summary>
    static bool N38WallEnds(Mover m, ICollisionWorld w, double look, double radius)
    {
        Vec3 v = new Vec3(m.velocity.x, 0, m.velocity.z);
        if (v.Length < 0.5) return false;
        Vec3 n = new Vec3(m.wallNormal.x, 0, m.wallNormal.z);
        if (n.Length < 0.5) return false;
        v = v / v.Length;
        n = n / n.Length;
        Vec3 o = m.position + new Vec3(0, 0.9, 0) + v * look;
        return !(w.RayIntersect(o, n * -1, out RayHit hit) && hit.distance <= radius + 0.6);
    }

    /// <summary>The solid of the noride list behind the Mover's current wall contact (null: none, or no noride solid near).</summary>
    static N38Solid N38WallSolid(N38Arena a, Mover m, double radius)
    {
        if (a.noride.Count == 0) return null;
        Vec3 n = m.wallNormal;
        Vec3 q = m.position + new Vec3(0, 0.9, 0) - new Vec3(n.x, 0, n.z) * (radius + 0.2);
        foreach (N38Solid s in a.noride)
        {
            Vec3 lo = s.min + N38Off, hi = s.max + N38Off;
            if (q.x >= lo.x - 0.1 && q.x <= hi.x + 0.1 && q.y >= lo.y - 0.1 && q.y <= hi.y + 0.1 && q.z >= lo.z - 0.1 && q.z <= hi.z + 0.1) return s;
        }
        return null;
    }

    // ================================================================================================ the envelope table

    sealed class N38EnvRow
    {
        public string verb, best, from;
        public double reach = double.NegativeInfinity, reach2 = double.NegativeInfinity, apex = double.NegativeInfinity, apexBestReach;
        public string apexBest;
    }

    /// <summary>A 40 m high ledge (top y 40) ending at z = 0 (the lip), the void north of it; a floor 40 m below catches the falls.</summary>
    static N38Arena N38LedgeArena(bool rideWall, double wallX)
    {
        var w = new TriangleWorld();
        Vec3 o = N38Off;
        w.AddBox(new Vec3(-30, 38, 0) + o, new Vec3(30, 40, 40) + o); // the ledge: top 40, the lip at z = 0
        w.AddBox(new Vec3(-120, -1, -160) + o, new Vec3(120, 0, 60) + o); // the floor far below
        if (rideWall) w.AddBox(new Vec3(wallX - 1, 0, -120) + o, new Vec3(wallX, 100, 30) + o); // a long wall on the left (its face x = wallX, normal +x)
        return new N38Arena { world = w };
    }

    static int N38EnvelopesCommand(string[] args)
    {
        GameConfig cfg = Config();
        bool ride38 = Array.IndexOf(args, "--ride38") >= 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        N38Rig rig = N38NewRig(cfg, ride38);
        Mover m0 = rig.sim.mover;
        bool shockLive = rig.sim.loadout.ShockwaveOn;
        double r = cfg.movement.body.capsuleRadius;
        Console.WriteLine($"n38 envelopes: the real sim (Survival run, the config's rows: wallride35 {m0.flow34.ride35.rules.on}, wallgrip36 {m0.Grip36On}, ride38 {rig.sim.n38.ride38}{(ride38 ? " (forced)" : "")}, stamina {m0.stamina.rules.Any}, flow34 {m0.flow34.rules.Any}); THUNDERCLAP jumpSpeed {cfg.Num("weapons.shotgun.jumpSpeed")} m/s, fire interval {cfg.Num("weapons.shotgun.fireInterval")} s; shockwave live {shockLive}");
        var owns = new HashSet<string>(N38Atoms);
        if (!shockLive) owns.Remove("shockwave");
        owns.Remove("grapple");
        Vec3 dir = new Vec3(0, 0, -1);
        Vec3 O = new Vec3(0, 40, 0) + N38Off;
        Vec3 start = new Vec3(0, 40.05, 12) + N38Off;
        N38Arena ledge = N38LedgeArena(false, 0);

        // 1. ground tries at the lip
        var groundRows = new List<(N38Try t, N38Trace tr)>();
        foreach (N38Try t in N38GroundTries(owns))
        {
            N38Trace tr = N38Run(rig, ledge, start, O, dir, t, O.y, 0, 6);
            groundRows.Add((t, tr));
            if (Array.IndexOf(args, "--verbose") >= 0) Console.WriteLine($"    {t.name,-28} reach {F35(tr.reachAtTop)} m (at -2 m {F35(tr.reachAtTop2)}), apex +{F35(tr.apex)} m, shots {tr.shots}{(tr.dashJumped ? " DashJump" : "")}{(tr.aborted ? " ABORTED " + tr.note : "")}");
        }
        // the family rows: the best try per verb family (the family = the try's last verb in the order of N38Families)
        var fam = new Dictionary<string, N38EnvRow>();
        foreach (var (t, tr) in groundRows)
        {
            string key = N38Family(t);
            if (!fam.TryGetValue(key, out N38EnvRow row)) fam[key] = row = new N38EnvRow { verb = key, from = "ground" };
            if (tr.reachAtTop > row.reach) { row.reach = tr.reachAtTop; row.best = t.name; }
            row.reach2 = Math.Max(row.reach2, tr.reachAtTop2);
            if (tr.apex > row.apex) { row.apex = tr.apex; row.apexBest = t.name; row.apexBestReach = tr.reachAtTop; }
        }
        Console.WriteLine("  from standing ground at a lip (metres along the run at the takeoff height / 2 m under it; the apex over the takeoff):");
        foreach (N38EnvRow row in fam.Values) Console.WriteLine($"    {row.verb,-22} reach {F35(row.reach),6} m  (-2 m: {F35(row.reach2),6} m)  apex +{F35(row.apex)} m   best: {row.best}; highest: {row.apexBest}");

        // 2. from a wall ride: a held ride on a long wall (face x = -1.5, normal +x), released 0.5 s in at pitch -45 / 0 / +45, then the air verb
        double wallX = -1.5;
        N38Arena wallA = N38LedgeArena(true, wallX);
        Vec3 wStart = new Vec3(wallX + r + 0.08, 40.05, 12) + N38Off;
        Vec3 wO = new Vec3(wallX + r + 0.08, 40, 0) + N38Off;
        var rideRows = new List<(string name, N38Trace tr)>();
        Console.WriteLine("  from a wall ride (a held ride released after 0.5 s; metres OUT from the wall and ALONG it at the release height; the apex over the release):");
        foreach (double p in new[] { -45.0, 0, 45 })
            foreach (string follow in new[] { "none", "double", "dash", "boost1", "boost2" })
            {
                var t = new N38Try { name = $"rideoff p{p:0}" + (follow == "none" ? "" : "+" + follow), atoms = new HashSet<string> { "ride" }, rideSide = -1, releaseAfter = 0.5, releasePitchDeg = p, follow = follow };
                N38Trace tr = N38Run(rig, wallA, wStart, wO, dir, t, wO.y, 0, 6, new Vec3(1, 0, 0));
                rideRows.Add((t.name, tr));
                Console.WriteLine($"    {t.name,-22} out {F35(tr.outAtRelease),6} m  along {F35(tr.alongAtRelease),6} m  apex +{F35(tr.apexOverRelease)} m{(tr.released ? $" (released at y {F35(tr.release.y - N38Off.y)}, rides {tr.rides}, shots {tr.shots})" : " NO RIDE " + tr.note)}");
            }

        // 3. the facing-wall ride chain: two walls g m apart (planes), the second starting where the first ends
        var chainGaps = new List<double>();
        var sbc = new StringBuilder();
        for (double g = 3; g <= 20; g += 1)
        {
            var w = new TriangleWorld();
            Vec3 o = N38Off;
            w.AddBox(new Vec3(-60, -1, -120) + o, new Vec3(60, 0, 40) + o);
            w.AddBox(new Vec3(-g / 2 - 1, 0, -10) + o, new Vec3(-g / 2, 30, 30) + o); // A: face x = -g/2 (normal +x), z 30 .. -10
            w.AddBox(new Vec3(g / 2, 0, -60) + o, new Vec3(g / 2 + 1, 30, -10) + o); // B: face x = +g/2 (normal -x), z -10 .. -60
            var arena = new N38Arena { world = w };
            var t = new N38Try { name = $"chain {g}", atoms = new HashSet<string> { "ride" }, rideSide = -1, releaseLeft = double.NaN, releaseAfter = 0, releasePitchDeg = 0 };
            // release when 3 m of wall A are left: the run's progress past z = -7
            t.releaseLeft = 3;
            N38Trace tr = N38Run(rig, arena, new Vec3(-g / 2 + r + 0.08, 0.05, 22) + o, new Vec3(-g / 2 + r + 0.08, 0, 10) + o, dir, t, 0, 20, 6); // edge 20 m on: the release 3 m before it (z -7), 3 m of wall A left
            bool ok = tr.rides >= 2;
            if (ok) chainGaps.Add(g);
            sbc.Append($" {g:0}:{(ok ? "chain" : "miss")}");
        }
        Console.WriteLine($"  facing-wall chain (plane gaps, m):{sbc}");

        // 4. the shaft climb (tap wall jumps), walls 3 m and 5 m apart: metres climbed in 4 s
        var shaft = new Dictionary<double, double>();
        foreach (double g in new[] { 3.0, 5.0 })
        {
            var w = new TriangleWorld();
            Vec3 o = N38Off;
            w.AddBox(new Vec3(-40, -1, -40) + o, new Vec3(40, 0, 40) + o);
            w.AddBox(new Vec3(-g / 2 - 1, 0, -10) + o, new Vec3(-g / 2, 60, 10) + o);
            w.AddBox(new Vec3(g / 2, 0, -10) + o, new Vec3(g / 2 + 1, 60, 10) + o);
            var arena = new N38Arena { world = w };
            double top = N38Shaft(rig, arena, g, r);
            shaft[g] = top;
            Console.WriteLine($"  shaft {g:0} m (tap wall jumps): {F35(top)} m climbed in 4 s");
        }

        // 5. read, not flown: the grapple and the shockwave
        double gRange = cfg.Has("design.n32.grapple32.range") ? cfg.Num("design.n32.grapple32.range") : double.NaN;
        double gCharges = cfg.Has("design.n32.grapple32.charges") ? cfg.Num("design.n32.grapple32.charges") : double.NaN;
        double gRecharge = cfg.Has("design.n32.grapple32.rechargeSeconds") ? cfg.Num("design.n32.grapple32.rechargeSeconds") : double.NaN;
        double gGap = cfg.Has("design.n32.grapple32.minGapSeconds") ? cfg.Num("design.n32.grapple32.minGapSeconds") : double.NaN;
        double gFrom = cfg.Has("design.n35.grapple35.fromLevel") ? cfg.Num("design.n35.grapple35.fromLevel") : double.NaN;
        double sPush = cfg.Has("design.shockwave.selfPush") ? cfg.Num("design.shockwave.selfPush") : double.NaN;
        Console.WriteLine($"  grapple (read): range {F35(gRange)} m, charges {gCharges}, recharge {gRecharge} s, min gap {gGap} s, owned from campaign level {gFrom} (grapple35: it pulls to ANY surface it hits, so a gate's landing visible within range is a grapple crossing)");
        Console.WriteLine($"  shockwave (read): live {shockLive} (design.n25.shockwaveOff {cfg.Design("n25.shockwaveOff.enabled")}), selfPush {F35(sPush)} m/s");
        Console.WriteLine($"n38 envelopes: {groundRows.Count + rideRows.Count + 18 + 2} attempts, {rig.steps} sim steps in {sw.Elapsed.TotalSeconds:0.0} s");

        if (Array.IndexOf(args, "--write") >= 0)
        {
            string N(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "null" : v.ToString("0.00", CultureInfo.InvariantCulture);
            string Q(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"_why\": \"NIGHT38 builder E (NIGHT38.md E1, PLAN.md 2): every verb and combo's reach, MEASURED on the real sim by `dotnet run --project Tools/SimHarness -c Release -- n38 envelopes --write` (a Survival run's SurvivalSim, the config's movement rows, the real Loadout's THUNDERCLAP shot; the Mover's World a box world). Not a level file: level builders space gates from it, and `n38 gates <plan>` proves each gate in its own geometry. Rerun after any movement or weapon tuning.\",\n");
            sb.Append($"  \"rows\": {{ \"wallride35\": {(m0.flow34.ride35.rules.on ? "true" : "false")}, \"wallgrip36\": {(m0.Grip36On ? "true" : "false")}, \"ride38\": {(rig.sim.n38.ride38 ? "true" : "false")}, \"stamina\": {(m0.stamina.rules.Any ? "true" : "false")}, \"flow34\": {(m0.flow34.rules.Any ? "true" : "false")}, \"shockwaveLive\": {(shockLive ? "true" : "false")} }},\n");
            sb.Append($"  \"shotgunJumpSpeed\": {N(cfg.Num("weapons.shotgun.jumpSpeed"))},\n");
            sb.Append($"  \"shotgunFireInterval\": {N(cfg.Num("weapons.shotgun.fireInterval"))},\n");
            sb.Append("  \"ground_why\": \"From standing ground: the run-up sprint held (12 m), the verb at the lip of a 40 m ledge over a void. reach = metres along the run past the lip while the feet are still at or above the takeoff height (the gap a same-height landing can be); reach2m = the same 2 m under it (a landing 2 m lower); apex = the most the feet rose over the takeoff. best = the try (timing, pitch) that reached farthest; apexBest = the highest. Boost tries look back along the run and down (pitch deg) so the shot's push (-aim x jumpSpeed) goes forward and up; xN = N shots chained at the gun's own fire interval.\",\n");
            sb.Append("  \"ground\": {\n");
            int i = 0;
            foreach (N38EnvRow row in fam.Values)
                sb.Append($"    {Q(row.verb)}: {{ \"reach\": {N(row.reach)}, \"reach2m\": {N(row.reach2)}, \"apex\": {N(row.apex)}, \"best\": {Q(row.best)}, \"apexBest\": {Q(row.apexBest)}, \"apexBestReach\": {N(row.apexBestReach)} }}{(++i < fam.Count ? "," : "")}\n");
            sb.Append("  },\n");
            sb.Append("  \"ride_why\": \"From a wall ride: a held ride on a long wall, released 0.5 s in at look pitch -45 / 0 / +45 deg (the jump-off), then nothing / the double jump 0.25 s later / a dash 0.15 s later / one or two THUNDERCLAP shots (looking back down 45 deg) from 0.1 s later. out = metres out from the release point perpendicular to the wall while the feet are at or above the release height; along = metres along the wall; apex = over the release point.\",\n");
            sb.Append("  \"ride\": {\n");
            i = 0;
            foreach (var (name, tr) in rideRows)
                sb.Append($"    {Q(name)}: {{ \"out\": {N(tr.outAtRelease)}, \"along\": {N(tr.alongAtRelease)}, \"apex\": {N(tr.apexOverRelease)}, \"rode\": {(tr.released ? "true" : "false")} }}{(++i < rideRows.Count ? "," : "")}\n");
            sb.Append("  },\n");
            sb.Append($"  \"facingGapsThatChain\": [{string.Join(", ", chainGaps.ConvertAll(x => x.ToString("0", CultureInfo.InvariantCulture)))}],\n");
            sb.Append("  \"facingGaps_why\": \"Plane gaps (m, 3..20) between two facing walls where a held ride on the first, released with 3 m of it left (pitch 0, the stick away) and the jump pressed again 0.25 s later, starts a ride on the second (the flight room's ride chain). A gap past the last one listed needs more than a ride chain.\",\n");
            sb.Append($"  \"shaftClimb4s\": {{ \"gap3\": {N(shaft[3])}, \"gap5\": {N(shaft[5])} }},\n");
            sb.Append("  \"shaft_why\": \"Metres climbed in 4 s between two parallel walls 3 m / 5 m apart with tap wall jumps (pressed and held 0.1 s on each new wall contact).\",\n");
            sb.Append($"  \"grapple\": {{ \"range\": {N(gRange)}, \"charges\": {N(gCharges)}, \"rechargeSeconds\": {N(gRecharge)}, \"minGapSeconds\": {N(gGap)}, \"fromLevel\": {N(gFrom)} }},\n");
            sb.Append("  \"grapple_why\": \"READ from design.n32.grapple32 / design.n35.grapple35, not flown (the hook aims through the sim's own GameWorld). Under grapple35 the hook pulls you to whatever surface it hits within range: the gate checker counts a gate as grappled when a point of its landing solid within 1 m of the top is in line of sight within range from the takeoff eye (or a grapple node is).\",\n");
            sb.Append($"  \"shockwave\": {{ \"live\": {(shockLive ? "true" : "false")}, \"selfPush\": {N(sPush)} }},\n");
            sb.Append("  \"shockwave_why\": \"READ: the THUNDERCLAP's LT self push (design.shockwave.selfPush via Mover.Impulse) is live only while design.n25.shockwaveOff is off (Loadout.ShockwaveOn); off, no level owns it.\"\n");
            sb.Append("}\n");
            string path = Path.Combine(Root, "..", "Design", "levels", "envelopes38.json");
            File.WriteAllText(path, sb.ToString());
            Console.WriteLine($"  wrote {Path.GetFullPath(path)}");
        }
        return 0;
    }

    /// <summary>The family a ground try reports under: its verbs joined (walk, jump, jump+double, dash, jump+dash, slide+jump, ...).</summary>
    static string N38Family(N38Try t)
    {
        if (t.atoms.Count == 0) return "walk";
        string[] order = { "dashjump", "slide", "jump", "dash", "double", "boost", "pulse", "shockwave" };
        var parts = new List<string>();
        bool dj = t.atoms.Contains("dashjump");
        foreach (string a in order) if (t.atoms.Contains(a) && !(dj && (a == "jump" || a == "dash"))) parts.Add(a);
        string s = string.Join("+", parts);
        if (t.atoms.Contains("boost")) s += $" x{t.boostShots}";
        return s;
    }

    /// <summary>The shaft climb: tap wall jumps between two walls g m apart, from a standing jump; the height reached in 4 s.</summary>
    static double N38Shaft(N38Rig rig, N38Arena arena, double g, double r)
    {
        Mover m = rig.sim.mover;
        m.World = arena.world;
        Vec3 o = N38Off;
        m.Reset(o.x, 0.05, o.z);
        for (int k = 0; k < 24; k++) N38Tick(rig, 0, 0, new InputFrame(), new CombatInput { weapon = -1 });
        double top = 0, side = 1;
        int lastPress = -1000, heldUntil = 0;
        for (int s = 0; s < 480; s++)
        {
            bool near = m.touchingWall || m.state == MoveState.WallRide;
            bool press = s == 0;
            if (s == 0) { heldUntil = 30; lastPress = 0; }
            if (near && !m.grounded && s - lastPress > 18) { press = true; lastPress = s; heldUntil = s + 12; side = -side; }
            if (m.grounded && s > 30 && s - lastPress > 18) { press = true; lastPress = s; heldUntil = s + 30; }
            N38Tick(rig, 0, 0, new InputFrame { moveX = side, jump = new Button { pressed = press, held = press || s < heldUntil } }, new CombatInput { weapon = -1 });
            top = Math.Max(top, m.position.y);
        }
        return top;
    }

    // ================================================================================================ the plan (gates)

    sealed class N38Solid
    {
        public int idx;
        public string zone = "", kind = "", src = "box", id;
        public Vec3 min, max;
        public bool noride, ceiling, ride, ramp;
        public double[] rampFrom, rampTo;
        public double rampWidth, rampRise;
        public N38Gate gate;
        public string Label => id ?? $"{src} {idx} ({zone}, {kind})";
    }

    sealed class N38Gate
    {
        public string id, verb, atom;
        public Vec3 from, to;
        public double runUp = 6;
        public N38Solid solid;
    }

    sealed class N38Room
    {
        public string id;
        public double[] rect;
        public bool flight; // flight38: true (or kind "flight"): a room whose job is its gates
        public List<string> limits = new List<string>(); // the room's LIMIT flags (Campaign24: pistolOnly, meleeOnly, noDash, noSlide, noJump)
        public HashSet<string> owns; // the room's own owns38 (null: the plan's)
        public double height;
        public bool roof;
    }

    sealed class N38Plan
    {
        public string id = "?";
        public double wallH = 14;
        public readonly List<N38Room> rooms = new List<N38Room>();
        public readonly List<N38Solid> solids = new List<N38Solid>();
        public readonly List<(double[] rect, double floorY, string zone)> pits = new List<(double[], double, string)>();
        public readonly List<(double[] rect, double ceiling)> tunnels = new List<(double[], double)>();
        public readonly List<Vec3> nodes = new List<Vec3>();
        public HashSet<string> owns;
        public bool ownsDefault, pulseAdded;
        /// <summary>NIGHT39 E2: the plan's `surfaces` (ice / mud / boost rects, plan coordinates; Core/Surface26.cs), read with design.n26.surfaces' numbers.</summary>
        public Surfaces26 surfaces;
    }

    static double N38N(object o) => PortData.Num(o);
    static object N38Get(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out object v) ? v : null;
    static Vec3 N38V(object o) { List<object> a = PortData.Arr(o); return new Vec3(N38N(a[0]), N38N(a[1]), N38N(a[2])); }
    static double[] N38R(object o) { List<object> a = PortData.Arr(o); return new[] { N38N(a[0]), N38N(a[1]), N38N(a[2]), N38N(a[3]) }; }
    static bool N38B(Dictionary<string, object> d, string k) => N38Get(d, k) is bool b && b;

    static N38Plan N38LoadPlan(string path)
    {
        var plan = new N38Plan();
        Dictionary<string, object> p = PortData.Obj(PortData.LoadJson(path));
        plan.id = N38Get(p, "id") as string ?? Path.GetFileNameWithoutExtension(path);
        if (N38Get(p, "wallHeight") != null) plan.wallH = N38N(p["wallHeight"]);
        foreach (object ro in N38Arr(p, "rooms"))
        {
            var r = PortData.Obj(ro);
            var room = new N38Room { id = N38Get(r, "id") as string, rect = N38R(r["rect"]), height = N38Get(r, "height") != null ? N38N(r["height"]) : plan.wallH, roof = N38B(r, "roof"), flight = N38B(r, "flight38") || N38Get(r, "kind") as string == "flight" };
            if (N38Get(r, "limit") is List<object> lim) room.limits = lim.ConvertAll(o => o as string);
            if (N38Get(r, "owns38") is List<object> rown) room.owns = new HashSet<string>(rown.ConvertAll(o => N38Atom(o as string)));
            plan.rooms.Add(room);
        }
        int idx = 0;
        foreach (object bo in N38Arr(p, "boxes"))
        {
            var b = PortData.Obj(bo);
            var s = new N38Solid { idx = idx++, src = "box", zone = N38Get(b, "zone") as string ?? "", kind = N38Get(b, "kind") as string ?? "", min = N38V(b["min"]), max = N38V(b["max"]), noride = N38B(b, "noride"), ceiling = N38B(b, "ceiling"), ride = N38B(b, "ride"), id = N38Get(b, "id") as string };
            N38ReadGate(s, b);
            plan.solids.Add(s);
        }
        idx = 0;
        foreach (object po in N38Arr(p, "props"))
        {
            var b = PortData.Obj(po);
            idx++;
            if (!N38B(b, "collider")) continue;
            Vec3 at = N38V(b["at"]), sz = N38V(b["size"]);
            var s = new N38Solid { idx = idx - 1, src = "prop", zone = N38Get(b, "room") as string ?? "", kind = N38Get(b, "kind") as string ?? "", id = N38Get(b, "id") as string, min = at - sz * 0.5, max = at + sz * 0.5, noride = N38B(b, "noride"), ceiling = N38B(b, "ceiling") };
            N38ReadGate(s, b);
            plan.solids.Add(s);
        }
        idx = 0;
        foreach (object ro in N38Arr(p, "ramps"))
        {
            var b = PortData.Obj(ro);
            List<object> fa = PortData.Arr(b["from"]), ta = PortData.Arr(b["to"]);
            double[] fr = { N38N(fa[0]), N38N(fa[1]) }, to = { N38N(ta[0]), N38N(ta[1]) };
            double w = N38N(b["width"]), rise = N38N(b["rise"]);
            double x0 = Math.Min(fr[0], to[0]) - w, x1 = Math.Max(fr[0], to[0]) + w, z0 = Math.Min(fr[1], to[1]) - w, z1 = Math.Max(fr[1], to[1]) + w;
            plan.solids.Add(new N38Solid { idx = idx++, src = "ramp", zone = N38Get(b, "zone") as string ?? "", ramp = true, rampFrom = fr, rampTo = to, rampWidth = w, rampRise = rise, min = new Vec3(x0, 0, z0), max = new Vec3(x1, rise, z1) });
        }
        foreach (object po in N38Arr(p, "pits"))
        {
            var b = PortData.Obj(po);
            plan.pits.Add((N38R(b["rect"]), N38N(b["floorY"]), N38Get(b, "zone") as string ?? ""));
        }
        foreach (object to in N38Arr(p, "tunnels"))
        {
            var b = PortData.Obj(to);
            plan.tunnels.Add((N38R(b["rect"]), N38N(b["ceiling"])));
        }
        foreach (object go in N38Arr(p, "grapple"))
        {
            var b = PortData.Obj(go);
            if (N38Get(b, "at") != null) plan.nodes.Add(N38V(b["at"]));
        }
        // NIGHT39 E2: a room's own grapple nodes (`grappleNodes` on the room row: n38-finale's x6 rings)
        foreach (object ro in N38Arr(p, "rooms"))
            foreach (object no in N38Arr(PortData.Obj(ro), "grappleNodes"))
                plan.nodes.Add(N38V(no));
        plan.surfaces = Surfaces26.From(p, SurfaceRules.From(Config()));
        if (N38Get(p, "owns38") is List<object> ow) plan.owns = new HashSet<string>(ow.ConvertAll(o => N38Atom(o as string)));
        else
        {
            plan.owns = new HashSet<string> { "jump", "double", "dash", "slide", "boost", "ride", "walljump", "pulse" };
            plan.ownsDefault = true;
        }
        // the PULSE gauntlet: owned from level 2 on (n38-iceline's j1 gives it, and it carries on), so its LT blast is an owned combo on every
        // level after the foundry whether or not the plan's owns38 lists it ("gauntlet" in owns38 also means it)
        if (plan.owns.Contains("gauntlet")) plan.owns.Add("pulse");
        if (!plan.owns.Contains("pulse") && !plan.id.Contains("foundry"))
        {
            plan.owns.Add("pulse");
            plan.pulseAdded = true;
        }
        return plan;
    }

    static List<object> N38Arr(Dictionary<string, object> p, string k) => N38Get(p, k) is List<object> l ? l : new List<object>();

    static void N38ReadGate(N38Solid s, Dictionary<string, object> b)
    {
        if (!(N38Get(b, "gate") is Dictionary<string, object> g)) return;
        s.gate = new N38Gate
        {
            id = N38Get(g, "id") as string ?? s.id ?? $"{s.src}{s.idx}",
            verb = N38Get(g, "verb") as string ?? "?",
            from = N38V(g["from"]),
            to = N38V(g["to"]),
            runUp = N38Get(g, "runUp") != null ? N38N(g["runUp"]) : 6,
            solid = s,
        };
        s.gate.atom = N38Atom(s.gate.verb);
    }

    /// <summary>The world around a gate, built from the plan the way levelgen builds the level (LEVEL-FORMAT.md): each room's floor (a slab cut
    /// by its pits, each pit's own floor), its 2 m wall ring, its roof, the tunnels' ceiling slabs, every box / collider prop, the ramps' wedges;
    /// only what lies within `reach` m of the gate's line. `without`: one solid left out (the margin's free flight).</summary>
    static N38Arena N38BuildArena(N38Plan plan, Vec3 a, Vec3 b, double reach, N38Solid without, bool dry = false)
    {
        var w = new TriangleWorld();
        var arena = new N38Arena { world = w };
        double bx0 = Math.Min(a.x, b.x) - reach, bx1 = Math.Max(a.x, b.x) + reach, bz0 = Math.Min(a.z, b.z) - reach, bz1 = Math.Max(a.z, b.z) + reach;
        bool Near(double x0, double z0, double x1, double z1) => x1 >= bx0 && x0 <= bx1 && z1 >= bz0 && z0 <= bz1;
        Vec3 o = N38Off;
        // NIGHT39 E2: the level's surfaces near the line, moved with the world (dry: the control with the ice taken out)
        if (!dry && plan.surfaces != null && plan.surfaces.Any)
        {
            var s = new Surfaces26(plan.surfaces.rules);
            foreach (SurfaceZone z in plan.surfaces.zones)
                if (Near(z.x0, z.z0, z.x1, z.z1))
                    s.zones.Add(new SurfaceZone { kind = z.kind, x0 = z.x0 + o.x, z0 = z.z0 + o.z, x1 = z.x1 + o.x, z1 = z.z1 + o.z, minY = z.minY + o.y, maxY = z.maxY + o.y, dirX = z.dirX, dirZ = z.dirZ });
            if (s.Any) arena.surf = s;
        }
        foreach (Vec3 n in plan.nodes) // NIGHT39 E2: the grapple nodes the rig's hook can pick
            if (Near(n.x, n.z, n.x, n.z)) arena.nodes.Add(n + o);
        void Box(double x0, double y0, double z0, double x1, double y1, double z1)
        {
            if (x1 - x0 < 1e-6 || y1 - y0 < 1e-6 || z1 - z0 < 1e-6) return;
            w.AddBox(new Vec3(x0, y0, z0) + o, new Vec3(x1, y1, z1) + o);
        }
        foreach (N38Room rm in plan.rooms)
        {
            double[] rc = rm.rect;
            if (!Near(rc[0] - 2, rc[1] - 2, rc[2] + 2, rc[3] + 2)) continue;
            var pits = plan.pits.FindAll(q => q.rect[0] < rc[2] && q.rect[2] > rc[0] && q.rect[1] < rc[3] && q.rect[3] > rc[1]);
            double bottom = -1;
            foreach (var q in pits) bottom = Math.Min(bottom, q.floorY - 1);
            var pieces = new List<double[]> { (double[])rc.Clone() };
            foreach (var q in pits) pieces = N38Cut(pieces, q.rect);
            foreach (double[] pc in pieces) Box(pc[0], bottom, pc[1], pc[2], 0, pc[3]);
            foreach (var q in pits) Box(Math.Max(q.rect[0], rc[0]), q.floorY - 1, Math.Max(q.rect[1], rc[1]), Math.Min(q.rect[2], rc[2]), q.floorY, Math.Min(q.rect[3], rc[3]));
            double wallTop = Math.Max(rm.height, plan.wallH) + 1;
            Box(rc[0] - 2, bottom, rc[1] - 2, rc[0], wallTop, rc[3] + 2);
            Box(rc[2], bottom, rc[1] - 2, rc[2] + 2, wallTop, rc[3] + 2);
            Box(rc[0], bottom, rc[1] - 2, rc[2], wallTop, rc[1]);
            Box(rc[0], bottom, rc[3], rc[2], wallTop, rc[3] + 2);
            if (rm.roof) Box(rc[0] - 2, rm.height, rc[1] - 2, rc[2] + 2, rm.height + 1, rc[3] + 2);
        }
        foreach (var tn in plan.tunnels)
            if (Near(tn.rect[0], tn.rect[1], tn.rect[2], tn.rect[3])) Box(tn.rect[0], tn.ceiling, tn.rect[1], tn.rect[2], tn.ceiling + 2, tn.rect[3]);
        foreach (N38Solid s in plan.solids)
        {
            if (s == without || !Near(s.min.x, s.min.z, s.max.x, s.max.z)) continue;
            if (s.ramp)
            {
                N38Ramp(w, s);
                continue;
            }
            Box(s.min.x, s.min.y, s.min.z, s.max.x, s.max.y, s.max.z);
            if (s.noride) arena.noride.Add(s);
        }
        return arena;
    }

    /// <summary>A ramp row as a wedge: low end at `from` (y 0), high end at `to` (y rise), `width` m wide.</summary>
    static void N38Ramp(TriangleWorld w, N38Solid s)
    {
        Vec3 o = N38Off;
        double dx = s.rampTo[0] - s.rampFrom[0], dz = s.rampTo[1] - s.rampFrom[1], len = Math.Sqrt(dx * dx + dz * dz);
        if (len < 1e-6) return;
        double px = -dz / len * s.rampWidth / 2, pz = dx / len * s.rampWidth / 2;
        Vec3 lowL = new Vec3(s.rampFrom[0] + px, 0, s.rampFrom[1] + pz) + o, lowR = new Vec3(s.rampFrom[0] - px, 0, s.rampFrom[1] - pz) + o;
        Vec3 highL = new Vec3(s.rampTo[0] + px, 0, s.rampTo[1] + pz) + o, highR = new Vec3(s.rampTo[0] - px, 0, s.rampTo[1] - pz) + o;
        Vec3 topL = highL + new Vec3(0, s.rampRise, 0), topR = highR + new Vec3(0, s.rampRise, 0);
        Vec3 c = (lowL + lowR + highL + highR + topL + topR) / 6;
        w.AddOutward(lowL, lowR, topR, c);
        w.AddOutward(lowL, topR, topL, c);
        w.AddOutward(highL, topL, topR, c);
        w.AddOutward(highL, topR, highR, c);
        w.AddOutward(lowL, topL, highL, c);
        w.AddOutward(lowR, highR, topR, c);
    }

    /// <summary>Rects minus a hole (each rect [x0, z0, x1, z1] split into at most four pieces around it).</summary>
    static List<double[]> N38Cut(List<double[]> rects, double[] h)
    {
        var res = new List<double[]>();
        foreach (double[] r in rects)
        {
            if (h[0] >= r[2] || h[2] <= r[0] || h[1] >= r[3] || h[3] <= r[1]) { res.Add(r); continue; }
            if (h[1] > r[1]) res.Add(new[] { r[0], r[1], r[2], h[1] });
            if (h[3] < r[3]) res.Add(new[] { r[0], h[3], r[2], r[3] });
            double z0 = Math.Max(r[1], h[1]), z1 = Math.Min(r[3], h[3]);
            if (h[0] > r[0]) res.Add(new[] { r[0], z0, h[0], z1 });
            if (h[2] < r[2]) res.Add(new[] { h[2], z0, r[2], z1 });
        }
        return res;
    }

    // ================================================================================================ one gate

    sealed class N38GateResult
    {
        public N38Gate gate;
        public bool pass, intendedCrossed;
        public double gap, rise, margin = double.NegativeInfinity, hMargin, vMargin;
        public string bestIntended, bestOther, aborted;
        public double bestOtherMargin = double.NegativeInfinity;
        public readonly List<string> cheats = new List<string>();
        public readonly Dictionary<string, int> cheatFamilies = new Dictionary<string, int>();
        public string grapple;
        public int tries, dropped, nChain, nDashJump, wallJumpTries, noRideRides, pulseTries;
        public readonly List<double> linesSkipped = new List<double>();
        public string roomOwns = "";
        public double msAll, msChain, msDashJump;
        // NIGHT39 E2: ice
        public bool iceAtTakeoff;
        public int iceZones;
        public double skateReach = double.NegativeInfinity, noSkateReach = double.NegativeInfinity, skateLip;
        public string skateBest, noSkateBest;
        // NIGHT39 E2: the flown grapple
        public int grappleTries, grappleCrossings;
        public string bestGrappleHooks;
    }

    /// <summary>NIGHT39 E2: the flown grapple tries on one line. Hook points (plan coordinates): the grapple nodes along the gap, the gate solid (its top
    /// 1 m past the near edge, its top's centre, its near face 0.5 m under the top), whatever stands past it (a point 60 m on, 2 m over the top: the
    /// far wall), and the room's roof (over the gap's middle, its near edge, 3 m past it). One hook each, or two in order (design.n32.grapple32 has two
    /// charges; a second never goes back along the line unless it bites the gate), fired 0.05 / 0.3 s after the jump at the lip; then nothing, the double
    /// jump or a dash.</summary>
    static List<N38Try> N38GrappleTries(N38Plan plan, N38Room room, Vec3 fromL, Vec3 dir, N38Solid sol, double edgeL, double top, HashSet<string> owns)
    {
        var pts = new List<(string name, Vec3 at, double prog, bool gate)>();
        foreach (Vec3 n in plan.nodes)
        {
            Vec3 rel = n - fromL;
            double prog = rel.x * dir.x + rel.z * dir.z, lat = Math.Abs(rel.x * -dir.z + rel.z * dir.x);
            if (prog > -2 && prog < edgeL + 20 && lat < 12) pts.Add(($"node({F35(n.x)},{F35(n.y)},{F35(n.z)})", n, prog, false));
        }
        Vec3 c = (sol.min + sol.max) * 0.5;
        Vec3 nearTop = fromL + dir * (edgeL + 1);
        pts.Add(("gate top +1m", new Vec3(nearTop.x, top - 0.05, nearTop.z), edgeL + 1, true));
        pts.Add(("gate top centre", new Vec3(c.x, top - 0.05, c.z), edgeL + 2, true));
        Vec3 face = fromL + dir * (edgeL + 0.05);
        pts.Add(("gate near face", new Vec3(face.x, top - 0.5, face.z), edgeL, true));
        Vec3 far = fromL + dir * (edgeL + 60);
        pts.Add(("past the gate +2m", new Vec3(far.x, top + 2, far.z), edgeL + 60, false));
        if (room != null && room.roof)
            foreach (double f in new[] { 0.5, 1.0 })
            {
                Vec3 q = fromL + dir * (edgeL * f + (f >= 1 ? 3 : 0));
                pts.Add(($"roof @{(f >= 1 ? "edge+3m" : "mid")}", new Vec3(q.x, room.height + 0.5, q.z), edgeL * f + (f >= 1 ? 3 : 0), false));
            }
        var seqs = new List<List<int>>();
        for (int a = 0; a < pts.Count; a++)
        {
            seqs.Add(new List<int> { a });
            for (int b = 0; b < pts.Count; b++)
                if (b != a && (pts[b].prog > pts[a].prog || pts[b].gate) && !pts[a].gate) seqs.Add(new List<int> { a, b });
        }
        var all = new List<N38Try>();
        string[] follows = owns.Contains("dash") ? new[] { "", "double", "dash" } : new[] { "", "double" };
        foreach (List<int> s in seqs)
            foreach (double t0 in new[] { 0.05, 0.3 })
                foreach (string fo in follows)
                {
                    if (fo == "double" && !owns.Contains("double")) continue;
                    string nm = $"grapple {string.Join(" > ", s.ConvertAll(i => pts[i].name))} @{t0:0.0#}{(fo == "" ? "" : "+" + fo)}";
                    var atoms = new HashSet<string> { "jump", "grapple" };
                    if (fo != "") atoms.Add(fo);
                    all.Add(new N38Try { name = nm, atoms = atoms, hooks = s.ConvertAll(i => pts[i].at), hookNames = s.ConvertAll(i => pts[i].name), hookT0 = t0, hookFollow = fo });
                }
        return all;
    }

    /// <summary>NIGHT39 E2: the same try with the SKATE run-up (pumped dashes on the ice; no sprint).</summary>
    static N38Try N38Pumped(N38Try t) => new N38Try
    {
        name = t.name + " pumped", atoms = new HashSet<string>(t.atoms) { "dash" }, seq = new List<(string act, double t)>(t.seq), slideLead = t.slideLead, dashLead = t.dashLead,
        dashJumpSteps = t.dashJumpSteps, weapon = t.weapon, pulsePitchDeg = t.pulsePitchDeg, apexDouble = t.apexDouble, shotAfterDouble = t.shotAfterDouble, tapJumps = t.tapJumps,
        noSprint = true, pump = true, boostShots = t.boostShots, boostPitchDeg = t.boostPitchDeg,
    };

    /// <summary>The verbs a try USED, as the Mover performed them (its events), not as the script meant them: `ride` when a wall ride started
    /// (a jump held into a ride wall rides it, scripted or not), `dashjump` when a DashJump happened (a jump inside a dash), `walljump` when a
    /// wall jump happened with no ride (a ride's jump-off is a WallJump too); the other verbs are the script's presses.</summary>
    static HashSet<string> N38Did(N38Try t, N38Trace tr)
    {
        var s = new HashSet<string>(t.atoms);
        s.Remove("ride");
        s.Remove("dashjump");
        s.Remove("walljump");
        s.Remove("pulse");
        if (tr.pulses > 0) s.Add("pulse"); // the PULSE blast went (pulse27Blast)
        if (tr.rides > 0) s.Add("ride");
        if (tr.dashJumped) s.Add("dashjump");
        if (tr.wallJumps > 0 && tr.rides == 0) s.Add("walljump");
        if (tr.iceDashAdds > 0) s.Add("skate");
        s.Remove("grapple");
        if (tr.ropeFires > 0) s.Add("grapple"); // NIGHT39 E2: a hook the Mover fired // NIGHT39 E2: a dash on ice added speed (the pump, or the dash of a dash jump begun on ice)
        return s;
    }

    /// <summary>Can the capsule stand at these feet: inside a room's rect (with its radius), its body clear of every solid? (A start inside a solid
    /// sees only back faces and runs through it: a shifted line that starts there is not a line.)</summary>
    static bool N38Standable(N38Plan plan, Vec3 feet, double r)
    {
        bool inRoom = plan.rooms.Exists(q => feet.x >= q.rect[0] + r && feet.x <= q.rect[2] - r && feet.z >= q.rect[1] + r && feet.z <= q.rect[3] - r);
        if (!inRoom) return false;
        foreach (N38Solid s in plan.solids)
        {
            if (s.ramp) continue;
            if (feet.x > s.min.x - r && feet.x < s.max.x + r && feet.z > s.min.z - r && feet.z < s.max.z + r && feet.y + 1.8 > s.min.y && feet.y + 0.05 < s.max.y) return false;
        }
        return true;
    }

    /// <summary>The distance along `dir` from `O` to the box's footprint (0 when O stands in it; +inf when the line misses it).</summary>
    static double N38Edge(Vec3 O, Vec3 dir, Vec3 lo, Vec3 hi)
    {
        double tmin = 0, tmax = double.PositiveInfinity;
        double[] o = { O.x, O.z }, d = { dir.x, dir.z }, l = { lo.x, lo.z }, h = { hi.x, hi.z };
        for (int i = 0; i < 2; i++)
        {
            if (Math.Abs(d[i]) < 1e-9)
            {
                if (o[i] < l[i] || o[i] > h[i]) return double.PositiveInfinity;
                continue;
            }
            double t1 = (l[i] - o[i]) / d[i], t2 = (h[i] - o[i]) / d[i];
            if (t1 > t2) (t1, t2) = (t2, t1);
            tmin = Math.Max(tmin, t1);
            tmax = Math.Min(tmax, t2);
        }
        return tmin <= tmax ? tmin : double.PositiveInfinity;
    }

    static N38GateResult N38CheckGate(N38Rig rig, GameConfig cfg, N38Plan plan, N38Gate g, bool verbose)
    {
        var res = new N38GateResult { gate = g };
        double r = cfg.movement.body.capsuleRadius;
        Vec3 from = g.from, to = g.to;
        Vec3 flat = new Vec3(to.x - from.x, 0, to.z - from.z);
        if (flat.Length < 0.5) { res.aborted = "from and to are less than 0.5 m apart on the floor plan"; return res; }
        Vec3 dir = flat / flat.Length;
        N38Solid sol = g.solid;
        double top = sol.max.y;
        double edge = N38Edge(from, dir, sol.min, sol.max);
        if (double.IsInfinity(edge)) { res.aborted = "the line from -> to misses the gate solid's footprint"; return res; }
        res.gap = edge;
        res.rise = top - from.y;
        N38Arena full = N38BuildArena(plan, from - dir * g.runUp, to, 45, null);
        N38Arena free = N38BuildArena(plan, from - dir * g.runUp, to, 45, sol);
        Vec3 O = from + N38Off, start = from - dir * g.runUp + new Vec3(0, 0.05, 0) + N38Off;
        // NIGHT39 E2: the worlds a gate is flown in: the level as it is (its ice included); for a SKATE gate also the DRY control (the same
        // geometry with the ice taken out): a try that crosses off dry ground crosses without the skate, so it is a cheat (D-6)
        bool skateGate = g.atom == "skate";
        var worlds = new List<(N38Arena full, N38Arena free, string tag)> { (full, free, "") };
        if (skateGate) worlds.Add((N38BuildArena(plan, from - dir * g.runUp, to, 45, null, true), N38BuildArena(plan, from - dir * g.runUp, to, 45, sol, true), " [dry]"));
        res.iceAtTakeoff = full.IceAt(O);
        res.iceZones = full.surf?.zones.Count ?? 0;
        foreach (var w in worlds) { w.full.gateMin = sol.min + N38Off; w.full.gateMax = sol.max + N38Off; }
        bool shockLive = rig.sim.loadout.ShockwaveOn;
        // the room's own verbs: its owns38 (else the plan's), less what its LIMIT takes away (measured by `n38 limits`: PISTOL ONLY empties the
        // owned list to the KESTREL, so no THUNDERCLAP shot and no PULSE blast fire in the room)
        N38Room gateRoom = plan.rooms.Find(q => q.id == sol.zone);
        var owns = N38LimitOwns(gateRoom?.owns ?? plan.owns, gateRoom?.limits);
        if (gateRoom?.owns != null && plan.owns.Contains("pulse") && !gateRoom.owns.Contains("pulse") && gateRoom.owns.Contains("gauntlet")) owns.Add("pulse");
        string pulseAt = owns.Contains("pulse") ? N42PulseLockedUntil(plan, sol.zone) : null; // NIGHT42 M1: per-room ownership (the gauntlet's award room)
        if (pulseAt != null) owns.Remove("pulse");
        res.roomOwns = $"{string.Join(" ", owns)}{(gateRoom != null && gateRoom.limits.Count > 0 ? $" (room {gateRoom.id} LIMIT {string.Join(" ", gateRoom.limits)})" : "")}{(gateRoom?.owns != null ? $" (room {gateRoom.id} owns38)" : "")}{(pulseAt != null ? $" (no pulse: the gauntlet is awarded at {pulseAt})" : "")}";
        if (!shockLive) owns.Remove("shockwave");
        var tries = new List<N38Try>(N38GroundTries(owns));
        // NIGHT39 E2: a takeoff on ice adds the SKATE run-up (pumped dashes on the ice) to every ground try
        if (res.iceAtTakeoff && owns.Contains("dash"))
        {
            int n0 = tries.Count;
            for (int i = 0; i < n0; i++) tries.Add(N38Pumped(tries[i]));
        }
        tries.AddRange(N38RideTries(owns));
        // the LINES: the gate's own line, and the same line shifted sideways 2 and 4 m each way (a player picks his line: a wall-hugging gate line
        // must not hide a ride-free line down the middle). A shifted line whose run-up start is not standing ground, or that misses the solid, is skipped.
        Vec3 perp = new Vec3(-dir.z, 0, dir.x);
        foreach (var (fullW, freeW, worldTag) in worlds)
        foreach (double off in N38Lines)
        {
            if (worldTag != "" && res.linesSkipped.Contains(off)) continue;
            Vec3 fromL = from + perp * off, toL = to + perp * off;
            double edgeL = N38Edge(fromL, dir, sol.min, sol.max);
            if (double.IsInfinity(edgeL)) { res.linesSkipped.Add(off); continue; }
            if (off != 0 && !N38Standable(plan, fromL - dir * g.runUp, r) ) { res.linesSkipped.Add(off); continue; }
            if (off != 0 && !N38Standable(plan, fromL, r)) { res.linesSkipped.Add(off); continue; }
            Vec3 OL = fromL + N38Off, startL = fromL - dir * g.runUp + new Vec3(0, 0.05, 0) + N38Off;
            string lineTag = (off == 0 ? "" : $" [line {off:+0;-0} m]") + worldTag;
            bool Landed(N38Trace tr) => tr.landed && !tr.dropped && tr.landY >= top - 0.1 && tr.landProg >= edgeL - r;
            var lineTries = tries;
            if (g.atom == "grapple" && owns.Contains("grapple"))
            {
                // NIGHT39 E2: the grapple gate is FLOWN (the real hook, pull, arrival, charges and gap) on this line's hook points
                lineTries = new List<N38Try>(tries);
                List<N38Try> gt = N38GrappleTries(plan, gateRoom, fromL, dir, sol, edgeL, top, owns);
                lineTries.AddRange(gt);
                res.grappleTries += gt.Count;
            }
            foreach (N38Try t in lineTries)
            {
                res.tries++;
                var swTry = System.Diagnostics.Stopwatch.StartNew();
                double maxAfter = t.hooks != null ? 7 : t.rideSide != 0 ? 9 : 5;
                N38Trace tr = N38Run(rig, fullW, startL, OL, dir, t, top, edgeL, maxAfter);
                double msTry = swTry.Elapsed.TotalMilliseconds;
                res.msAll += msTry;
                if (!double.IsNaN(t.chainLook)) { res.msChain += msTry; res.nChain++; }
                if (t.dashLead > 0) { res.msDashJump += msTry; res.nDashJump++; }
                if (tr.aborted)
                {
                    if (off == 0) { res.aborted = tr.note; return res; }
                    res.linesSkipped.Add(off);
                    break; // this shifted line has no run-up: skip it
                }
                if (tr.dropped) res.dropped++;
                if (tr.wallJumps > 0 && tr.rides == 0) res.wallJumpTries++;
                if (tr.note.Contains("noride")) res.noRideRides++;
                if (tr.pulses > 0) res.pulseTries++;
                // the try's verbs as the Mover performed them (N38Did: a ride that started, a DashJump that happened), whatever the script meant
                bool uses = N38Did(t, tr).Contains(g.atom);
                bool crossed = Landed(tr);
                if (verbose) Console.WriteLine($"      {t.name + lineTag,-44} {(crossed ? "CROSSES" : "no     ")} land prog {F35(tr.landProg)} y {F35(tr.landY)} reach@top {F35(tr.reachAtTop)} apex +{F35(tr.apex)} rides {tr.rides} walljumps {tr.wallJumps} blasts {tr.pulses} shots {tr.shots}{(tr.dashJumped ? " DashJump" : "")}{(tr.iceDashAdds > 0 ? $" skate x{tr.iceDashAdds} lip {F35(tr.lipSpeed)} m/s" : "")} {tr.note}");
                if (skateGate)
                {
                    // the skate's sizing (INFO): every try's reach at the top's height past the near edge, flown again without the gate solid when it
                    // landed on it (a landing cuts the reach short): the best skate try vs the best try without it
                    double reachT = crossed ? N38Run(rig, freeW, startL, OL, dir, t, top, edgeL, maxAfter).reachAtTop : tr.reachAtTop;
                    if (uses && reachT > res.skateReach) { res.skateReach = reachT; res.skateBest = t.name + lineTag; res.skateLip = tr.lipSpeed; }
                    if (!uses && reachT > res.noSkateReach) { res.noSkateReach = reachT; res.noSkateBest = t.name + lineTag; }
                }
                if (crossed && !uses)
                {
                    string famC = string.Join("+", N38Did(t, tr).OrderBy(x => x, StringComparer.Ordinal));
                    res.cheats.Add($"{t.name}{lineTag} (lands at {F35(tr.landProg)} m, y {F35(tr.landY)})");
                    res.cheatFamilies.TryGetValue(famC, out int nf);
                    res.cheatFamilies[famC] = nf + 1;
                    continue;
                }
                if (!uses && !crossed)
                {
                    double om = tr.reachAtTop > double.NegativeInfinity ? (tr.reachAtTop - edgeL) / Math.Max(edgeL, 0.5) : double.NegativeInfinity;
                    if (om > res.bestOtherMargin) { res.bestOtherMargin = om; res.bestOther = t.name + lineTag; }
                    continue;
                }
                if (!crossed) continue;
                // the margin: the same try on the same line with the gate solid out of the world (how far past the edge at the top's height; the apex over the rise)
                res.intendedCrossed = true;
                double h, v, mg;
                if (t.hooks != null)
                {
                    // NIGHT39 E2, the flown grapple's margin: the range slack of its farthest hook ((range - distance) / range: a hook at 36 m of 40 is
                    // +10 %); a crossing whose last bite was not the gate solid also needs its carry (the same flight with the solid out of the world)
                    res.grappleCrossings++;
                    double range = rig.sim.cfg.movement.arcline.range;
                    double slack = (range - tr.hookFarthest) / range;
                    h = slack;
                    v = double.PositiveInfinity;
                    if (!tr.hookOnGate)
                    {
                        N38Trace fr = N38Run(rig, freeW, startL, OL, dir, t, top, edgeL, maxAfter);
                        h = Math.Min(slack, (fr.reachAtTop - edgeL) / Math.Max(edgeL, 0.5));
                        v = res.rise > 0.5 ? (fr.apex - res.rise) / res.rise : double.PositiveInfinity;
                    }
                    mg = Math.Min(h, v);
                    if (mg > res.margin) res.bestGrappleHooks = $"{tr.ropeFires} hook(s), farthest {F35(tr.hookFarthest)} m of {F35(range)}, {(tr.hookOnGate ? "the last on the gate solid" : "carried onto it")}";
                }
                else
                {
                    N38Trace fr = N38Run(rig, freeW, startL, OL, dir, t, top, edgeL, maxAfter);
                    h = (fr.reachAtTop - edgeL) / Math.Max(edgeL, 0.5);
                    v = res.rise > 0.5 ? (fr.apex - res.rise) / res.rise : double.PositiveInfinity;
                    mg = Math.Min(h, v);
                }
                if (mg > res.margin) { res.margin = mg; res.hMargin = h; res.vMargin = v; res.bestIntended = t.name + lineTag; }
            }
        }
        // the grapple (owned): a hook point on the gate solid within 1 m of its top, in sight within range from the takeoff eye; or a node
        if (owns.Contains("grapple"))
        {
            double range = cfg.Has("design.n32.grapple32.range") ? cfg.Num("design.n32.grapple32.range") : 40;
            Vec3 eye = O + new Vec3(0, cfg.movement.body.eyeHeight, 0);
            string hook = N38GrappleHook(full.world, eye, sol, range, plan.nodes);
            res.grapple = hook ?? $"no hook point within {F35(range)} m in sight";
            bool usesG = g.atom == "grapple";
            if (hook != null && !usesG)
            {
                res.cheats.Add($"grapple ({hook})");
                res.cheatFamilies["grapple"] = 1;
            }
            // NIGHT39 E2: a grapple gate is judged by its FLOWN tries (above: the real hook, pull and charges, one or two pulls); the line-of-sight
            // hook (one pull to the landing) is printed beside them. NIGHT38's rule (a hook in sight = crossed, margin +100 %) is gone.
        }
        res.pass = res.aborted == null && res.intendedCrossed && res.margin >= 0.10 && res.cheats.Count == 0;
        return res;
    }

    /// <summary>A hook point on the solid (each face sampled 5 x 5, those within 1 m under its top) the eye sees within `range`, or a grapple node
    /// within range in sight that stands within 6 m of the solid's footprint and no more than 1 m under its top. Null: none.</summary>
    static string N38GrappleHook(TriangleWorld w, Vec3 eye, N38Solid s, double range, List<Vec3> nodes)
    {
        Vec3 lo = s.min + N38Off, hi = s.max + N38Off;
        bool Sees(Vec3 p, out double dist)
        {
            Vec3 d = p - eye;
            dist = d.Length;
            if (dist > range || dist < 0.1) return false;
            if (!w.RayIntersect(eye, d / dist, out RayHit hit)) return false;
            Vec3 q = eye + d / dist * hit.distance;
            return q.x >= lo.x - 0.06 && q.x <= hi.x + 0.06 && q.y >= lo.y - 0.06 && q.y <= hi.y + 0.06 && q.z >= lo.z - 0.06 && q.z <= hi.z + 0.06;
        }
        for (int face = 0; face < 6; face++)
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    double u = 0.1 + 0.8 * i / 4, v = 0.1 + 0.8 * j / 4;
                    Vec3 p;
                    switch (face)
                    {
                        case 0: p = new Vec3(lo.x + (hi.x - lo.x) * u, hi.y - 0.01, lo.z + (hi.z - lo.z) * v); break; // top
                        case 1: p = new Vec3(lo.x + 0.01, hi.y - (hi.y - lo.y) * u * 0.999, lo.z + (hi.z - lo.z) * v); break;
                        case 2: p = new Vec3(hi.x - 0.01, hi.y - (hi.y - lo.y) * u * 0.999, lo.z + (hi.z - lo.z) * v); break;
                        case 3: p = new Vec3(lo.x + (hi.x - lo.x) * v, hi.y - (hi.y - lo.y) * u * 0.999, lo.z + 0.01); break;
                        case 4: p = new Vec3(lo.x + (hi.x - lo.x) * v, hi.y - (hi.y - lo.y) * u * 0.999, hi.z - 0.01); break;
                        default: continue;
                    }
                    if (p.y < hi.y - 1.0) continue;
                    if (Sees(p, out double dist)) return $"the solid's {(face == 0 ? "top" : "side")} at ({F35(p.x - N38Off.x)}, {F35(p.y)}, {F35(p.z)}), {F35(dist)} m";
                }
        foreach (Vec3 n0 in nodes)
        {
            Vec3 n = n0 + N38Off;
            double dx = Math.Max(lo.x - n.x, Math.Max(0, n.x - hi.x)), dz = Math.Max(lo.z - n.z, Math.Max(0, n.z - hi.z));
            if (Math.Sqrt(dx * dx + dz * dz) > 6 || n.y < hi.y - 1) continue;
            Vec3 d = n - eye;
            double dist = d.Length;
            if (dist > range) continue;
            if (!w.RayIntersect(eye, d / dist, out RayHit hit) || hit.distance >= dist - 0.3) return $"the grapple node at ({F35(n0.x)}, {F35(n0.y)}, {F35(n0.z)}), {F35(dist)} m";
        }
        return null;
    }

    static bool n38VerboseGates;

    /// <summary>`--trace "<try name>"`: that try's state every 6 steps (a debugging aid).</summary>
    static string n38Trace;

    /// <summary>The lines a gate is flown on: its own, then shifted sideways (m). `--lines 0` flies its own line only.</summary>
    static double[] N38Lines = { 0, -2, 2, -4, 4 };

    static string N38Pct(double v) => double.IsNaN(v) || double.IsInfinity(v) ? (v > 0 ? "inf" : "n/a") : (v * 100).ToString("+0;-0", CultureInfo.InvariantCulture) + " %";

    static void N38PrintGate(N38GateResult g)
    {
        N38Gate q = g.gate;
        Console.WriteLine($"  gate {q.id} [{q.verb}] {q.solid.Label}: from ({F35(q.from.x)}, {F35(q.from.y)}, {F35(q.from.z)}) to ({F35(q.to.x)}, {F35(q.to.y)}, {F35(q.to.z)}), gap {F35(g.gap)} m to the near edge, rise {F35(g.rise)} m, {g.tries} tries{(g.dropped > 0 ? $" ({g.dropped} dropped on noride walls)" : "")}");
        if (g.aborted != null) { Console.WriteLine($"    RED: {g.aborted}"); return; }
        Console.WriteLine($"    verbs in the room: {g.roomOwns}");
        if (g.iceZones > 0 || q.atom == "skate")
            Console.WriteLine($"    ice: {g.iceZones} surface zone(s) near the line; the takeoff {(g.iceAtTakeoff ? "ON ice (every ground try also flown with the skate run-up: pumped dashes)" : "on dry ground")}{(q.atom == "skate" ? "; flown twice: the level (ice) and the DRY control (the ice taken out: a crossing there is a crossing without the skate)" : "")}");
        if (q.atom == "skate")
        {
            double lo = g.noSkateReach, hi = g.skateReach / 1.1;
            Console.WriteLine($"    skate sizing: the best skate try reaches {F35(g.skateReach)} m past the near edge at the top's height ({g.skateBest ?? "none"}, lip {F35(g.skateLip)} m/s); the best try without the skate {F35(g.noSkateReach)} m ({g.noSkateBest ?? "none"})");
            Console.WriteLine($"    a skate-only gap on this line: {(hi > lo ? $"over {F35(lo)} m and at most {F35(hi)} m (today {F35(g.gap)} m)" : $"NONE: the skate's reach / 1.1 ({F35(hi)} m) is not past the best try without it ({F35(lo)} m)")}");
        }
        Console.WriteLine($"    lines flown:{string.Join(" ", N38Lines.Where(l => !g.linesSkipped.Contains(l)).Select(l => l.ToString("+0;-0;0", CultureInfo.InvariantCulture)))} m{(g.linesSkipped.Count > 0 ? $" (skipped: no run-up or it misses the solid: {string.Join(" ", g.linesSkipped.Select(l => l.ToString("+0;-0", CultureInfo.InvariantCulture)))} m)" : "")}");
        Console.WriteLine($"    {q.verb}: {(g.intendedCrossed ? $"crosses, margin {N38Pct(g.margin)} (along {N38Pct(g.hMargin)}, rise {N38Pct(g.vMargin)}) by {g.bestIntended}" : "does NOT cross")}{(g.intendedCrossed && g.margin < 0.10 ? " (under 10 %)" : "")}");
        Console.WriteLine($"    the best other try: {(g.bestOther == null ? "none" : $"{g.bestOther}, {N38Pct(g.bestOtherMargin)} at the top's height")}{(g.bestOtherMargin > -0.05 && g.bestOtherMargin < 0 ? " (WARN: within 5 % of crossing)" : "")}");
        if (g.grapple != null) Console.WriteLine($"    grapple (line of sight from the lip, one pull): {g.grapple}");
        if (g.grappleTries > 0) Console.WriteLine($"    grapple FLOWN (the real hook, pull, arrival, 2 charges, the gap): {g.grappleTries} tries, {g.grappleCrossings} land on the gate{(g.bestGrappleHooks != null ? $"; the best: {g.bestGrappleHooks} (margin = the farthest hook's range slack; a carry also needs its reach with the solid out)" : "")}");
        if (g.cheats.Count > 0)
        {
            Console.WriteLine($"    CHEATS: {g.cheats.Count} tries cross without the {q.verb}: {string.Join(", ", g.cheatFamilies.Select(kv => $"{kv.Key} x{kv.Value}"))}");
            int shown = 0;
            foreach (string c in g.cheats)
                if (n38VerboseGates || shown++ < 4) Console.WriteLine($"      e.g. {c}");
        }
        Console.WriteLine($"    {(g.pass ? "PASS" : "RED")}   (tries {g.msAll / 1000:0.00} s: {g.nChain} ride chains {g.msChain / 1000:0.00} s, {g.nDashJump} dash jumps {g.msDashJump / 1000:0.00} s)");
    }

    static int N38GatesCommand(string[] args)
    {
        string path = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : null;
        if (path == null)
        {
            Console.WriteLine("n38 gates <plan.json> [--verbose] [--only <gateId>] | n38 gates --selftest");
            return 2;
        }
        GameConfig cfg = Config();
        bool verbose = Array.IndexOf(args, "--verbose") >= 0;
        n38VerboseGates = verbose;
        int oi = Array.IndexOf(args, "--only");
        string only = oi >= 0 && oi + 1 < args.Length ? args[oi + 1] : null;
        N38Plan plan = N38LoadPlan(path);
        int li = Array.IndexOf(args, "--lines");
        if (li >= 0 && li + 1 < args.Length && args[li + 1] == "0") N38Lines = new[] { 0.0 };
        // NIGHT39 E2: `--gate <solid>:<verb>:<fx,fy,fz>:<tx,ty,tz>[:<runUp>[:<id>]]` tags a solid the plan does not tag (solid: box<i> / prop<i> /
        // its id), e.g. n38-iceline's G1 on D1: --gate box24:skate:14,0,-134:14,0,-164:53:j3-g1 (a gate a level builder has not written yet)
        for (int a = 0; a + 1 < args.Length; a++)
        {
            if (args[a] == "--room-limit") // `--room-limit <room>:<flag>[,<flag>]`: a what-if LIMIT on a room (Campaign24's flags: pistolOnly, meleeOnly, noDash, noSlide, noJump)
            {
                string[] rl = args[a + 1].Split(':');
                N38Room rm = plan.rooms.Find(q => q.id == rl[0]);
                if (rm == null || rl.Length < 2) { Console.WriteLine($"n38 gates: --room-limit {args[a + 1]}: no such room"); return 2; }
                rm.limits.AddRange(rl[1].Split(','));
                Console.WriteLine($"n38 gates: WHAT-IF room {rm.id} LIMIT {string.Join(" ", rm.limits)} (not in the plan)");
                continue;
            }
            if (args[a] != "--gate") continue;
            string[] f = args[a + 1].Split(':');
            Vec3 P(string v) { string[] c = v.Split(','); return new Vec3(double.Parse(c[0], CultureInfo.InvariantCulture), double.Parse(c[1], CultureInfo.InvariantCulture), double.Parse(c[2], CultureInfo.InvariantCulture)); }
            N38Solid s = f.Length >= 4 ? plan.solids.Find(q => q.src + q.idx == f[0] || q.id == f[0]) : null;
            if (s == null) { Console.WriteLine($"n38 gates: --gate {args[a + 1]}: no such solid (box<i>, prop<i> or an id) or fewer than 4 fields"); return 2; }
            s.gate = new N38Gate { id = f.Length > 5 ? f[5] : $"{f[0]}-{f[1]}", verb = f[1], atom = N38Atom(f[1]), from = P(f[2]), to = P(f[3]), runUp = f.Length > 4 ? double.Parse(f[4], CultureInfo.InvariantCulture) : 6, solid = s };
        }
        var gates =plan.solids.Where(s => s.gate != null).Select(s => s.gate).Where(g => only == null || g.id == only).ToList();
        if (Array.IndexOf(args, "--ride40") >= 0) N38GatesRide40 = true; // NIGHT40 R2
        N38Rig rig = N38NewRig(cfg, Array.IndexOf(args, "--ride38") >= 0);
        Console.WriteLine($"n38 gates {plan.id}: {gates.Count} gate(s); owns {string.Join(" ", plan.owns)}{(plan.ownsDefault ? " (no owns38 on the plan: every verb but the grapple)" : "")}{(plan.pulseAdded ? " (pulse added: the PULSE gauntlet is owned from level 2 on)" : "")}; ride38 {rig.sim.n38.ride38}; ride40 {rig.sim.n40.ride40}{(N38GatesRide40 ? " (forced; + the climb tries)" : "")}; shockwave live {rig.sim.loadout.ShockwaveOn}");
        int red = 0;
        foreach (N38Gate g in gates)
        {
            N38GateResult res = N38CheckGate(rig, cfg, plan, g, verbose);
            N38PrintGate(res);
            if (!res.pass) red++;
        }
        if (gates.Count == 0) Console.WriteLine("  no solid carries a gate");
        // a flight room (flight38: true) is its gates: one with no gate solid in its zone is RED
        if (only == null)
            foreach (N38Room rm in plan.rooms)
            {
                if (!rm.flight) continue;
                int n = plan.solids.Count(s => s.gate != null && s.zone == rm.id);
                Console.WriteLine($"  flight room {rm.id}: {n} gate(s){(n == 0 ? " RED: a flight room with no gate" : "")}");
                if (n == 0) red++;
            }
        Console.WriteLine($"n38 gates {plan.id}: {(red == 0 && gates.Count > 0 ? "GREEN" : "RED")} ({gates.Count - red} / {gates.Count} pass)");
        return red == 0 && gates.Count > 0 ? 0 : 1;
    }

    // ================================================================================================ room limits in the real sim

    /// <summary>`n38 limits [level-id] [roomId]`: does a LIMIT room (default: the level's first `pistolOnly` room) let the PULSE blast and the
    /// THUNDERCLAP boost fire? A Campaign24 run of the level (the real SurvivalSim, Loadout and Combat), the gauntlet and the THUNDERCLAP owned;
    /// each verb tried once BEFORE the room (the control) and once after the campaign's own room entry (Campaign24.Enter, which sets the room's
    /// limits through SurvivalSim.CampaignSetLimits): select the gun (its slot, then every slot), fill the glove, press LT (the blast) / jump and
    /// fire in the air (the boost). Counts pulse27Blast / pulse27Denied and THUNDERCLAP "fire" events. Exit 0 always (a measurement).</summary>
    static int N38LimitsCommand(string[] args)
    {
        GameConfig cfg = Config();
        string levelId = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : "n38-spire";
        string roomId = args.Length > 3 && !args[3].StartsWith("--") ? args[3] : null;
        string path = Path.GetFullPath(Path.Combine(Root, "..", "Design", "levels", levelId + ".json"));
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(3801, null, null);
        sim.husks.frozen = true;
        var inp = (ScriptedInput)sim.input;
        int blasts = 0, denied = 0, sgFires = 0;
        sim.events.On<Pulse27Event>("pulse27Blast", _ => blasts++);
        sim.events.On<Pulse27Event>("pulse27Denied", _ => denied++);
        sim.events.On<string>("fire", w => { if (w == "shotgun") sgFires++; });
        void Tick(InputFrame f, CombatInput c)
        {
            inp.SetLook(0, 0);
            inp.Set(f, c);
            sim.hitstopLeft = 0;
            sim.health.hp = sim.health.max;
            sim.mover.invulnerable = true;
            sim.Step();
        }
        void Idle(int n) { for (int k = 0; k < n; k++) Tick(new InputFrame(), new CombatInput { weapon = -1 }); }
        bool Select(string gun)
        {
            for (int pass = 0; pass < 2 && sim.loadout.Weapon != gun; pass++)
            {
                int n = Math.Max(1, sim.loadout.owned.Count);
                for (int s = 0; s < (pass == 0 ? 1 : Math.Max(n, 6)) && sim.loadout.Weapon != gun; s++)
                {
                    int slot = pass == 0 ? sim.loadout.owned.IndexOf(gun) : s;
                    if (slot < 0) continue;
                    Tick(new InputFrame(), new CombatInput { weapon = slot });
                    for (int k = 0; k < 90 && !(sim.loadout.ready && sim.loadout.Weapon == gun); k++) Idle(1);
                }
            }
            return sim.loadout.Weapon == gun;
        }
        string TryBlast()
        {
            int b0 = blasts, d0 = denied;
            bool inHand = Select("gauntlet");
            if (sim.loadout.pulse27 != null) { sim.loadout.pulse27.pips = sim.loadout.pulse27.p.pips; sim.loadout.pulse27.cooldownSteps = 0; }
            double vy0 = sim.mover.velocity.y;
            Tick(new InputFrame(), new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = true } });
            Tick(new InputFrame(), new CombatInput { weapon = -1 });
            Idle(60);
            return $"gauntlet in hand {(inHand ? "yes" : "NO")} (hand {sim.loadout.Weapon}), pulse27Blast {blasts - b0}, pulse27Denied {denied - d0}";
        }
        string TryBoost()
        {
            int f0 = sgFires;
            bool inHand = Select("shotgun");
            Tick(new InputFrame { jump = new Button { held = true, pressed = true } }, new CombatInput { weapon = -1 });
            for (int k = 0; k < 10; k++) Tick(new InputFrame { jump = new Button { held = true } }, new CombatInput { weapon = -1 });
            double vy0 = sim.mover.velocity.y;
            Tick(new InputFrame(), new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } });
            double dvy = sim.mover.velocity.y - vy0;
            Idle(120);
            return $"THUNDERCLAP in hand {(inHand ? "yes" : "NO")} (hand {sim.loadout.Weapon}), its fire events {sgFires - f0}";
        }
        // NIGHT39 E2: the grapple too (a LIMIT must not take it from a grapple gate's room): a press looking ahead and up, counted by the Mover's RopeFire
        int ropeFires = 0;
        sim.mover.Events += e => { if (e.type == MoverEventType.RopeFire) ropeFires++; };
        string TryGrapple()
        {
            int r0 = ropeFires;
            if (sim.Grapple32On) sim.grappleCharges32 = sim.grapple32Rules.charges;
            Idle(120); // the gap and the rope's cooldown run out
            bool owned = sim.mover.ropeEnabled;
            for (int k = 0; k < 6 && ropeFires == r0; k++)
            {
                inp.SetLook(0, 0.35);
                inp.Set(new InputFrame { yaw = 0, pitch = 0.35, grapple = new Button { held = true, pressed = true } }, new CombatInput { weapon = -1 });
                sim.hitstopLeft = 0;
                sim.Step();
                inp.Set(new InputFrame { yaw = 0, pitch = 0.35 }, new CombatInput { weapon = -1 });
                sim.Step();
            }
            Idle(240);
            return $"owned (ropeEnabled) {(owned ? "yes" : "NO")}, RopeFire {ropeFires - r0}";
        }
        LevelDef lv = sim.campaignLevel;
        int ri = roomId != null ? lv.rooms.FindIndex(q => q.id == roomId) : lv.rooms.FindIndex(q => (q.limits & Limit24.PistolOnly) != 0);
        if (ri < 0) { Console.WriteLine($"n38 limits {levelId}: no {(roomId ?? "pistolOnly")} room"); return 1; }
        RoomDef rd = lv.rooms[ri];
        foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        Idle(30);
        Console.WriteLine($"n38 limits {levelId}: room {rd.id} limits {Limit24.Banner(rd.limits)} (flags {rd.limits}); the campaign at room {sim.campaign.room}, flags in force {sim.campaign.limits}");
        Console.WriteLine($"  before (control): owned {string.Join(",", sim.loadout.owned)}");
        Console.WriteLine($"    PULSE blast: {TryBlast()}");
        Console.WriteLine($"    THUNDERCLAP boost: {TryBoost()}");
        Console.WriteLine($"    grapple: {TryGrapple()}");
        typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { ri });
        Idle(2);
        Console.WriteLine($"  in {rd.id} (Campaign24.Enter: the campaign's room entry): flags in force {sim.campaign.limits}; owned {string.Join(",", sim.loadout.owned)}; hand {sim.loadout.Weapon}");
        Console.WriteLine($"    PULSE blast: {TryBlast()}");
        Console.WriteLine($"    THUNDERCLAP boost: {TryBoost()}");
        Console.WriteLine($"    grapple: {TryGrapple()}");
        SurvivalSim.Wyrm24Dir = null;
        return 0;
    }

    /// <summary>The verbs a LIMIT room takes away (SurvivalSim.CampaignSetLimits / Limit24.Strip, measured by `n38 limits`): PISTOL ONLY leaves the
    /// KESTREL alone (no THUNDERCLAP shot, no PULSE blast), MELEE ONLY the gauntlet alone (no THUNDERCLAP shot), NO DASH / NO SLIDE / NO JUMP drop
    /// that key (and every combo that uses it).</summary>
    static HashSet<string> N38LimitOwns(HashSet<string> owns, List<string> limits)
    {
        var o = new HashSet<string>(owns);
        if (limits == null || limits.Count == 0) return o;
        bool melee = limits.Contains("meleeOnly");
        if (limits.Contains("pistolOnly") || melee) { o.Remove("boost"); o.Remove("shockwave"); }
        if (limits.Contains("pistolOnly") && !melee) o.Remove("pulse");
        if (limits.Contains("noDash")) { o.Remove("dash"); o.Remove("dashjump"); }
        if (limits.Contains("noSlide")) o.Remove("slide");
        if (limits.Contains("noJump")) { o.Remove("jump"); o.Remove("double"); o.Remove("ride"); o.Remove("walljump"); }
        return o;
    }

    // ================================================================================================ the selftest

    /// <summary>One room (60 x 120 m, roof 30 m) with three gap gates, each a takeoff deck and a landing deck (tops 0) over a 20 m deep pit;
    /// the gaps sized from the envelope measured here: intended = the verb whose best try outreaches the best try without it by the most.</summary>
    static int N38GatesSelftest(string[] args)
    {
        GameConfig cfg = Config();
        bool verbose = Array.IndexOf(args, "--verbose") >= 0;
        n38VerboseGates = verbose;
        N38Rig rig = N38NewRig(cfg, false);
        var owns = new HashSet<string> { "jump", "double", "dash", "slide" };
        // the envelope on a void lip (the same rig): the best reach per try
        N38Arena ledge = N38LedgeArena(false, 0);
        Vec3 dir = new Vec3(0, 0, -1), O = new Vec3(0, 40, 0) + N38Off, start = new Vec3(0, 40.05, 12) + N38Off;
        var reach = new List<(N38Try t, double r)>();
        foreach (N38Try t in N38GroundTries(owns)) reach.Add((t, N38Run(rig, ledge, start, O, dir, t, O.y, 0, 6).reachAtTop));
        string bestAtom = null;
        double r1 = 0, r2 = 0, bestRatio = 0;
        foreach (string a in owns)
        {
            if (a == "jump") continue; // every air combo starts with the jump: a jump gate says nothing
            double with = reach.Where(x => x.t.atoms.Contains(a)).Select(x => x.r).DefaultIfEmpty(double.NegativeInfinity).Max();
            double without = reach.Where(x => !x.t.atoms.Contains(a)).Select(x => x.r).DefaultIfEmpty(0).Max();
            if (without > 0 && with / without > bestRatio) { bestRatio = with / without; bestAtom = a; r1 = with; r2 = without; }
        }
        Console.WriteLine($"n38 gates --selftest: owns {string.Join(" ", owns)}; the widest split: {bestAtom} reaches {F35(r1)} m, the best try without it {F35(r2)} m (x{bestRatio:0.00})");
        if (bestAtom == null || bestRatio < 1.25)
        {
            Console.WriteLine("n38 gates --selftest: RED (no verb outreaches the others by 25 %: no clean gate can be sized)");
            return 1;
        }
        double clean = Math.Sqrt(r1 * r2) * 0.98; // between the two: r1 / clean >= 1.1, r2 < clean
        double cheesy = r2 * 0.85; // the best other try clears it
        double thin = r1 / 1.04; // the verb clears it by about 4 %
        var plan = new N38Plan { id = "selftest38", wallH = 30 };
        plan.owns = owns;
        plan.rooms.Add(new N38Room { id = "t1", rect = new[] { -30.0, -200, 30, 0 }, height = 30, roof = true });
        plan.pits.Add((new[] { -28.0, -198, 28, -2 }, -20.0, "t1"));
        var gates = new List<(string label, double gap, bool expect)> { ("clean", clean, true), ("cheesable", cheesy, false), ("thin", thin, false) };
        double x = -18;
        var gateList = new List<N38Gate>();
        foreach (var (label, gap, _) in gates)
        {
            // takeoff deck: z -4 .. -30 (the lip at -30); landing deck from z -30 - gap, 20 m deep
            var take = new N38Solid { idx = plan.solids.Count, kind = "deck", zone = "t1", id = label + "-takeoff", min = new Vec3(x - 4, -20, -30), max = new Vec3(x + 4, 0, -4) };
            var land = new N38Solid { idx = plan.solids.Count + 1, kind = "deck", zone = "t1", id = label + "-landing", min = new Vec3(x - 4, -20, -30 - gap - 20), max = new Vec3(x + 4, 0, -30 - gap) };
            land.gate = new N38Gate { id = label, verb = bestAtom, atom = bestAtom, from = new Vec3(x, 0, -30), to = new Vec3(x, 0, -30 - gap - 2), runUp = 12, solid = land };
            plan.solids.Add(take);
            plan.solids.Add(land);
            gateList.Add(land.gate);
            x += 18;
        }
        bool all = true;
        // the classifier (F's round-2 case): a try is judged by what the Mover DID. A slide-hop + double that rode two walls used the ride; a scripted
        // dash jump with no DashJump event did not dash-jump; a scripted ride that never started a ride did not ride.
        {
            var slideHop = N38Seq("slidehop+double", "slide jump double", ("jump", 0), ("double", 0.4));
            var dj = N38Seq("dashjump", "dash jump dashjump");
            var ride = new N38Try { name = "ride", atoms = new HashSet<string> { "jump", "ride", "double" }, rideSide = -1 };
            bool c1 = N38Did(slideHop, new N38Trace { rides = 2 }).Contains("ride");
            bool c2 = !N38Did(slideHop, new N38Trace { rides = 0 }).Contains("ride");
            bool c3 = !N38Did(dj, new N38Trace { dashJumped = false }).Contains("dashjump") && N38Did(dj, new N38Trace { dashJumped = true }).Contains("dashjump");
            bool c4 = !N38Did(ride, new N38Trace { rides = 0 }).Contains("ride");
            bool c5 = N38Did(new N38Try { name = "taps", atoms = new HashSet<string> { "jump", "walljump" }, taps = true }, new N38Trace { wallJumps = 2 }).Contains("walljump");
            bool cls = c1 && c2 && c3 && c4 && c5;
            Console.WriteLine($"    selftest classifier: slide-hop+double with 2 rides counts as ride {(c1 ? "yes" : "NO")}, with none not {(c2 ? "yes" : "NO")}; dash jump only with its event {(c3 ? "yes" : "NO")}; a ride that never started is no ride {(c4 ? "yes" : "NO")}; tap wall jumps count {(c5 ? "yes" : "NO")}: {(cls ? "ok" : "WRONG")}");
            all &= cls;
        }
        for (int i = 0; i < gates.Count; i++)
        {
            N38GateResult res = N38CheckGate(rig, cfg, plan, gateList[i], verbose);
            N38PrintGate(res);
            bool ok = res.pass == gates[i].expect;
            if (!gates[i].expect && gates[i].label == "cheesable") ok &= res.cheats.Count > 0; // it fails for the right reason: a cheat
            if (!gates[i].expect && gates[i].label == "thin") ok &= res.intendedCrossed && res.margin < 0.10; // the right reason: the margin
            Console.WriteLine($"    selftest {gates[i].label} (gap {F35(gates[i].gap)} m): expected {(gates[i].expect ? "PASS" : "FAIL")}, got {(res.pass ? "PASS" : "FAIL")}: {(ok ? "ok" : "WRONG")}");
            all &= ok;
        }
        // a boost RISE gate (the THUNDERCLAP owned): a landing deck 4.5 m over the takeoff, 4 m on: only the mid-air shot lifts that high
        var plan2 = new N38Plan { id = "selftest38-boost", wallH = 30 };
        plan2.owns = new HashSet<string> { "jump", "double", "dash", "slide", "boost" };
        plan2.rooms.Add(new N38Room { id = "t2", rect = new[] { -30.0, -120, 30, 0 }, height = 30, roof = true });
        var take2 = new N38Solid { idx = 0, kind = "deck", zone = "t2", id = "rise-takeoff", min = new Vec3(-4, 0, -30), max = new Vec3(4, 0.5, -4) };
        var land2 = new N38Solid { idx = 1, kind = "deck", zone = "t2", id = "rise-landing", min = new Vec3(-4, 0, -54), max = new Vec3(4, 5.0, -34) };
        land2.gate = new N38Gate { id = "rise", verb = "boost", atom = "boost", from = new Vec3(0, 0.5, -30), to = new Vec3(0, 5.0, -40), runUp = 12, solid = land2 };
        plan2.solids.Add(take2);
        plan2.solids.Add(land2);
        N38GateResult r2res = N38CheckGate(rig, cfg, plan2, land2.gate, verbose);
        N38PrintGate(r2res);
        bool ok2 = r2res.pass && r2res.vMargin >= 0.10;
        Console.WriteLine($"    selftest rise (a 4.5 m rise, boost owned): expected PASS, got {(r2res.pass ? "PASS" : "FAIL")}: {(ok2 ? "ok" : "WRONG")}");
        all &= ok2;
        // the PULSE blast (K3's gap 1): a 4.5 m rise with the gauntlet owned and no THUNDERCLAP: the blast crosses (PASS as the verb); the same
        // gate meant for the double jump with the gauntlet owned: the blast is a cheat (RED, a pulse cheat)
        var plan3 = new N38Plan { id = "selftest38-pulse", wallH = 30 };
        plan3.owns = new HashSet<string> { "jump", "double", "dash", "slide", "pulse" };
        plan3.rooms.Add(new N38Room { id = "t3", rect = new[] { -30.0, -120, 30, 0 }, height = 30, roof = true });
        var take3 = new N38Solid { idx = 0, kind = "deck", zone = "t3", id = "pulse-takeoff", min = new Vec3(-4, 0, -30), max = new Vec3(4, 0.5, -4) };
        var land3 = new N38Solid { idx = 1, kind = "deck", zone = "t3", id = "pulse-landing", min = new Vec3(-4, 0, -54), max = new Vec3(4, 5.0, -34) };
        plan3.solids.Add(take3);
        plan3.solids.Add(land3);
        land3.gate = new N38Gate { id = "pulse-rise", verb = "pulse", atom = "pulse", from = new Vec3(0, 0.5, -30), to = new Vec3(0, 5.0, -40), runUp = 12, solid = land3 };
        N38GateResult p3 = N38CheckGate(rig, cfg, plan3, land3.gate, verbose);
        N38PrintGate(p3);
        land3.gate = new N38Gate { id = "pulse-cheat", verb = "double", atom = "double", from = new Vec3(0, 0.5, -30), to = new Vec3(0, 5.0, -40), runUp = 12, solid = land3 };
        N38GateResult p4 = N38CheckGate(rig, cfg, plan3, land3.gate, verbose);
        N38PrintGate(p4);
        bool okP = p3.pass && p3.pulseTries > 0 && !p4.pass && p4.cheatFamilies.Keys.Any(k => k.Contains("pulse"));
        Console.WriteLine($"    selftest pulse: the blast as the verb {(p3.pass ? "PASS" : "FAIL")} ({p3.pulseTries} tries blasted), the blast as a cheat on a double gate {(p4.cheatFamilies.Keys.Any(k => k.Contains("pulse")) ? "caught" : "MISSED")}: {(okP ? "ok" : "WRONG")}");
        all &= okP;
        // noride walls (K3's gap 2): a 4.5 m rise beside a 3 m shaft of noride walls, the THUNDERCLAP owned: the real Mover starts no ride on them but
        // wall-jumps off them (the tries are flown, not dropped); the boost still clears the rise
        var plan4 = new N38Plan { id = "selftest38-noride", wallH = 30 };
        plan4.owns = new HashSet<string> { "jump", "double", "dash", "slide", "boost", "ride", "walljump" };
        plan4.rooms.Add(new N38Room { id = "t4", rect = new[] { -30.0, -120, 30, 0 }, height = 30, roof = true });
        plan4.solids.Add(new N38Solid { idx = 0, kind = "hull", zone = "t4", id = "noride-W", min = new Vec3(-2.5, 0, -33.5), max = new Vec3(-1.5, 8, -4), noride = true });
        plan4.solids.Add(new N38Solid { idx = 1, kind = "hull", zone = "t4", id = "noride-E", min = new Vec3(1.5, 0, -33.5), max = new Vec3(2.5, 8, -4), noride = true });
        var land4 = new N38Solid { idx = 2, kind = "deck", zone = "t4", id = "noride-landing", min = new Vec3(-4, 0, -54), max = new Vec3(4, 4.5, -34) };
        land4.gate = new N38Gate { id = "noride-rise", verb = "boost", atom = "boost", from = new Vec3(0, 0, -30), to = new Vec3(0, 4.5, -40), runUp = 12, solid = land4 };
        plan4.solids.Add(land4);
        N38Lines = new[] { 0.0 };
        N38GateResult p5 = N38CheckGate(rig, cfg, plan4, land4.gate, verbose);
        N38Lines = new[] { 0.0, -2, 2, -4, 4 };
        N38PrintGate(p5);
        bool okN = p5.wallJumpTries > 0 && p5.noRideRides == 0 && p5.dropped == 0;
        Console.WriteLine($"    selftest noride: {p5.wallJumpTries} tries wall-jumped off the noride walls (flown, not dropped), rides started on them {p5.noRideRides}; the gate {(p5.pass ? "PASS" : "RED")}: {(okN ? "ok" : "WRONG")}");
        all &= okN;
        // NIGHT39 E2, ice (n38-iceline's G1 in small): a 40 m ice lane to a lip, a 24 m dry pit, a landing deck; the dash owned. The judge must
        // put the ice under the run-up (pumped tries whose dashes on the ice ADD speed: Glide30.dashAdds), let a skate try cross, and fly the DRY
        // control, where the Mover's dash jump crosses without the skate (24 m/s, the dash's own speed): RED for the right reason (a [dry] cheat)
        var plan5 = new N38Plan { id = "selftest38-ice", wallH = 30 };
        plan5.owns = new HashSet<string> { "jump", "double", "dash", "slide" };
        plan5.rooms.Add(new N38Room { id = "t5", rect = new[] { -12.0, -120, 12, 0 }, height = 30, roof = true });
        plan5.pits.Add((new[] { -12.0, -74, 12, -50 }, -4.0, "t5"));
        plan5.surfaces = new Surfaces26(SurfaceRules.From(cfg));
        plan5.surfaces.zones.Add(new SurfaceZone { kind = SurfaceKind.Ice, x0 = -12, z0 = -50, x1 = 12, z1 = 0, minY = -0.5, maxY = 0.5 });
        var land5 = new N38Solid { idx = 0, kind = "deck", zone = "t5", id = "ice-landing", min = new Vec3(-12, -4, -86), max = new Vec3(12, 0, -74) };
        land5.gate = new N38Gate { id = "skate", verb = "skate", atom = "skate", from = new Vec3(0, 0, -50), to = new Vec3(0, 0, -80), runUp = 44, solid = land5 };
        plan5.solids.Add(land5);
        N38Lines = new[] { 0.0 };
        N38GateResult p6 = N38CheckGate(rig, cfg, plan5, land5.gate, verbose);
        N38Lines = new[] { 0.0, -2, 2, -4, 4 };
        N38PrintGate(p6);
        bool dryCheat = p6.cheats.Exists(c => c.Contains("[dry]"));
        bool okI = p6.iceAtTakeoff && p6.intendedCrossed && p6.skateBest != null && dryCheat && !p6.pass;
        Console.WriteLine($"    selftest ice: the takeoff on ice {(p6.iceAtTakeoff ? "yes" : "NO")}, a skate try crossed {(p6.intendedCrossed ? "yes" : "NO")} (dashes on the ice added speed: {(p6.skateBest != null ? "yes" : "NO")}), the dry control's crossing caught as a cheat {(dryCheat ? "yes" : "NO")}; the gate {(p6.pass ? "PASS" : "RED")}: {(okI ? "ok" : "WRONG")}");
        all &= okI;
        // NIGHT39 E2, the two-pull grapple (n38-finale's x6 in small): a 40 m void (x6 is 44.5 m), two grapple nodes 8 m up at 13.5 m and 29.5 m, a roofed room;
        // jump, double and the grapple owned. The flown grapple must cross with two hooks (PASS); the same void with no node and no roof must NOT be
        // crossed with a 10 % margin (the hook's 40 m barely reaches the far side: RED for the grapple itself, not for a cheat)
        bool okG = true;
        foreach (bool planted in new[] { false, true })
        {
            var plan6 = new N38Plan { id = planted ? "selftest38-grapple-bare" : "selftest38-grapple", wallH = 16 };
            plan6.owns = new HashSet<string> { "jump", "double", "grapple" };
            plan6.rooms.Add(new N38Room { id = "t6", rect = new[] { -10.0, -77.5, 10, 0 }, height = 16, roof = !planted }); // x6's shape: the end wall right behind the deck
            plan6.pits.Add((new[] { -10.0, -70, 10, -30 }, -6.0, "t6"));
            if (!planted)
            {
                plan6.nodes.Add(new Vec3(0, 8, -43.5));
                plan6.nodes.Add(new Vec3(0, 8, -59.5));
            }
            var land6 = new N38Solid { idx = 0, kind = "deck", zone = "t6", id = "grapple-landing", min = new Vec3(-10, -6, -77.5), max = new Vec3(10, 0, -70) };
            land6.gate = new N38Gate { id = "grapple2", verb = "grapple", atom = "grapple", from = new Vec3(0, 0, -30), to = new Vec3(0, 0, -74), runUp = 12, solid = land6 };
            plan6.solids.Add(land6);
            N38Lines = new[] { 0.0 };
            N38GateResult p7 = N38CheckGate(rig, cfg, plan6, land6.gate, verbose);
            N38Lines = new[] { 0.0, -2, 2, -4, 4 };
            N38PrintGate(p7);
            bool ok = planted ? !p7.pass && p7.cheats.Count == 0 && (p7.grappleCrossings == 0 || p7.margin < 0.10) : p7.pass && p7.bestGrappleHooks != null && p7.bestGrappleHooks.StartsWith("2 hook");
            Console.WriteLine($"    selftest grapple {(planted ? "bare (no node, no roof)" : "two nodes")}: expected {(planted ? "RED for the grapple itself (no crossing with a 10 % margin)" : "PASS with two hooks")}, got {(p7.pass ? "PASS" : "RED")}, {p7.grappleCrossings} grapple crossing(s), best {p7.bestGrappleHooks ?? "none"}: {(ok ? "ok" : "WRONG")}");
            okG &= ok;
        }
        all &= okG;
        // NIGHT39 E2 (K4's blastwall): n38-kiln's G1 cliff in small: a 9.5 m noride cliff 1.5 m from the take-off, wall to wall in a 16 m roofed room;
        // the PULSE gauntlet and the wall jump owned, the THUNDERCLAP not (the gate's verb `boost` cannot cross here). The blast alone tops out under
        // the ledge (8.46 m on kiln); the blast plus a wall jump off the cliff face lands: the checker must catch it as a cheat of that family
        {
            var plan8 = new N38Plan { id = "selftest38-blastwall", wallH = 16 };
            plan8.owns = new HashSet<string> { "jump", "double", "walljump", "pulse" };
            plan8.rooms.Add(new N38Room { id = "t8", rect = new[] { -8.0, -56, 8, -30 }, height = 16, roof = true });
            var cliff = new N38Solid { idx = 0, kind = "deck", zone = "t8", id = "blastwall-cliff", min = new Vec3(-8, -1, -46), max = new Vec3(8, 9.5, -41.5), noride = true };
            cliff.gate = new N38Gate { id = "blastwall", verb = "boost", atom = "boost", from = new Vec3(0, 0, -40), to = new Vec3(0, 9.5, -43.75), runUp = 6, solid = cliff };
            plan8.solids.Add(cliff);
            plan8.solids.Add(new N38Solid { idx = 1, kind = "hull", zone = "t8", id = "blastwall-W", min = new Vec3(-8, -1, -56), max = new Vec3(-7.7, 16, -30), noride = true }); // kiln's noride liners
            plan8.solids.Add(new N38Solid { idx = 2, kind = "hull", zone = "t8", id = "blastwall-E", min = new Vec3(7.7, -1, -56), max = new Vec3(8, 16, -30), noride = true });
            N38Lines = new[] { 0.0 };
            N38GateResult p8 = N38CheckGate(rig, cfg, plan8, cliff.gate, verbose);
            N38Lines = new[] { 0.0, -2, 2, -4, 4 };
            N38PrintGate(p8);
            bool caught = p8.cheatFamilies.Keys.Any(k => k.Contains("pulse") && k.Contains("walljump"));
            bool blastAlone = p8.cheatFamilies.Keys.Any(k => k.Contains("pulse") && !k.Contains("walljump") && !k.Contains("ride"));
            bool okB = !p8.pass && caught && !blastAlone;
            Console.WriteLine($"    selftest blastwall: the blast + a wall jump off the noride cliff caught as a cheat {(caught ? "yes" : "NO")}, the blast alone crossing {(blastAlone ? "YES (the cliff is too low to isolate the combo)" : "no")}: {(okB ? "ok" : "WRONG")}");
            all &= okB;
        }
        Console.WriteLine($"n38 gates --selftest: {(all ? "GREEN" : "RED")}");
        return all ? 0 : 1;
    }
}
