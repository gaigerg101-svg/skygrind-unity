// NIGHT24 (builder F): `fist [--off] [--tag t] [--seeds n]`, the contract charge-fist's headless judge (design.n24.chargeFist; rule Core/Fist24.cs).
//   1 J       the horizontal reach of a jump from a standstill with forward held, measured on the Mover over flat floor; the config's
//             jumpDistance must match it (it is the unit of the flick)
//   2 idle    60 s of standing about (invulnerable, no hit, no kill): the charge stays 0 (it never rises with time)
//   3 rates   one husk killed by fist: the charge rose by hits and the kill at their rates; one killed by the revolver: otherKill
//   4 flick   three flicks (charge staged at each notch, a punch): the distances measured on the Mover against 0.5 / 1.0 / 2.25 J,
//             all the charge spent, the lockout holds 3 s (a second punch inside it does not flick), the three measured distances printed
//   5 air     one flick in the air until the player is grounded again
//   6 wall    aim into a wall 2 m ahead: the flick is cut to the room there is (it never ends inside geometry) and nothing launches
//             when there is less than minRoom
//   7 hurt    a hit on the player takes a third of the charge
//   8 bots    kills-to-FULL for a fist-only bot and a revolver-only bot over the same husk waves; the fist-only number is 25-35
//             (the first rates of R3 C2 and the config's final rates are both printed)
//   --off     the block off (--off24's game): no charge, no flick: FAIL (RED), exit 1
// Prints one line per part and the contract line "fist PASS|FAIL: ...". Mover.cs is untouched: the flick is Mover.Impulse.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static SurvivalSim FistSim(GameConfig cfg, SurvivalMap map, bool on, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = on ? new N24Blocks { chargeFist = true } : N24Blocks.Off;
        sim.n25Override = N25Blocks.Off; // NIGHT25: the fist harness judges NIGHT24's rules (the pulse row replaces them; harness `pulse` judges that)
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.mover.invulnerable = true;
        return sim;
    }

    /// <summary>One step with the given buttons (the look is set first).</summary>
    static void FistTick(SurvivalSim sim, double yaw, double pitch, double moveZ = 0, bool jump = false, bool jumpHeld = false, bool punch = false, bool fire = false, int weapon = -1)
    {
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, pitch);
        inp.Set(new InputFrame { moveZ = moveZ, jump = new Button { held = jump || jumpHeld, pressed = jump } },
            new CombatInput { weapon = weapon, punch = new Button { held = punch, pressed = punch }, fire = new Button { held = fire, pressed = fire } });
        sim.hitstopLeft = 0;
        sim.Step();
    }

    static void FistSettle(SurvivalSim sim, double yaw, int steps = 200)
    {
        for (int s = 0; s < steps; s++) FistTick(sim, yaw, 0);
    }

    /// <summary>A flat stretch: a start, a yaw and how far a wall is ahead (>= `need` m), floor level within 5 cm along the way, 3 m of head room.</summary>
    static bool FistFlat(SurvivalSim sim, double need, out Vec3 start, out double yaw, out double wallDist)
    {
        var rng = new Mulberry32(2424);
        start = default;
        yaw = 0;
        wallDist = 0;
        for (int tries = 0; tries < 6000; tries++)
        {
            double ang = rng.Next() * Math.PI * 2, rad = 4 + rng.Next() * 26;
            double sx = Math.Cos(ang) * rad, sz = Math.Sin(ang) * rad;
            if (!sim.world.RayIntersect(new Vec3(sx, 26, sz), new Vec3(0, -1, 0), out RayHit f0)) continue;
            double fy = 26 - f0.distance;
            if (Math.Abs(fy) > 0.2) continue;
            for (int k = 0; k < 8; k++)
            {
                double a = rng.Next() * Math.PI * 2;
                double dx = Math.Sin(a), dz = -Math.Cos(a);
                var from = new Vec3(sx, fy + 0.9, sz);
                double wd = sim.world.RayIntersect(from, new Vec3(dx, 0, dz), out RayHit wh) ? wh.distance : 1e9;
                if (wd < need + 1) continue;
                bool flat = true;
                for (double m = 0; m <= need && flat; m += 1)
                {
                    double px = sx + dx * m, pz = sz + dz * m;
                    if (!sim.world.RayIntersect(new Vec3(px, fy + 6, pz), new Vec3(0, -1, 0), out RayHit fh) || Math.Abs(fy + 6 - fh.distance - fy) > 0.05) flat = false;
                    else if (sim.world.RayIntersect(new Vec3(px, fy + 0.2, pz), new Vec3(0, 1, 0), out RayHit ch) && ch.distance < 4) flat = false;
                }
                if (!flat) continue;
                start = new Vec3(sx, fy, sz);
                yaw = Math.Atan2(-dx, -dz);
                wallDist = wd;
                return true;
            }
        }
        return false;
    }

    static double FistHoriz(Vec3 a, Vec3 b) => Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));

    /// <summary>J: a standstill, forward held, jump held 0.25 s: the horizontal distance from the take-off to the landing.</summary>
    static double FistMeasureJump(SurvivalSim sim, Vec3 start, double yaw)
    {
        sim.Respawn(start, 0);
        FistSettle(sim, yaw);
        Vec3 takeoff = sim.mover.position;
        bool left = false;
        for (int s = 0; s < 600; s++)
        {
            FistTick(sim, yaw, 0, 1, s == 0, s < 30);
            if (!left && !sim.mover.grounded)
            {
                left = true;
                takeoff = sim.mover.position;
            }
            if (left && sim.mover.grounded) return FistHoriz(takeoff, sim.mover.position);
        }
        return -1;
    }

    /// <summary>A flick at a staged charge: the horizontal distance from the stand to where the player comes to rest.</summary>
    static double FistMeasureFlick(SurvivalSim sim, Vec3 start, double yaw, double charge, out bool flicked, out double spent)
    {
        sim.Respawn(start, 0);
        FistSettle(sim, yaw, 120);
        Vec3 from = sim.mover.position;
        int f0 = sim.fist24.flicks;
        sim.fist24.charge = charge;
        for (int s = 0; s < 720; s++)
        {
            FistTick(sim, yaw, 0, 0, false, false, s == 0);
            if (s > 30 && sim.mover.Speed < 0.05 && sim.mover.grounded) break;
        }
        flicked = sim.fist24.flicks > f0;
        spent = charge - sim.fist24.charge;
        return FistHoriz(from, sim.mover.position);
    }

    /// <summary>A bot that banks the charge: kills husks one by one, fist (the gauntlet, at 6 m the husk walks in) or revolver (at 12 m), until FULL.
    /// Returns the kills; `hitsPerKill` the mean fist hits a husk took.</summary>
    static int FistKillsToFull(GameConfig cfg, SurvivalMap map, Vec3 start, double lookYaw, bool fist, double[] rates, out double hitsPerKill, out int guesses)
    {
        SurvivalSim sim = FistSim(cfg, map, true, 2455);
        sim.fist24.noFlick = true; // the bot banks the charge: the punches must not spend it
        if (rates != null)
        {
            sim.fist24.rules.hit = rates[0];
            sim.fist24.rules.fistKill = rates[1];
            sim.fist24.rules.otherKill = rates[2];
        }
        int slot = sim.loadout.owned.IndexOf(fist ? "gauntlet" : "revolver");
        Vec3 p = start;
        sim.Respawn(p, 0);
        int kills = 0, hits = 0, killsFist = 0;
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.melee) hits++; });
        guesses = 0;
        for (int i = 0; i < 400 && sim.fist24.Tier < 3; i++)
        {
            sim.husks.KillAll();
            double ux = -Math.Sin(lookYaw), uz = -Math.Cos(lookYaw);
            double d = fist ? 6 : 12;
            Husk h = sim.husks.Spawn("husk", new Vec3(p.x + ux * d, p.y, p.z + uz * d), p);
            if (h == null) continue;
            int k0 = sim.fist24.fistKillsFed + sim.fist24.otherKillsFed;
            for (int s = 0; s < 1500 && h.alive; s++)
            {
                (double yaw, double pitch) = ArclineLook(sim.Eye, h.AimPoint);
                FistTick(sim, yaw, pitch, 0, false, false, false, true, s == 0 ? slot : -1);
            }
            if (sim.fist24.fistKillsFed + sim.fist24.otherKillsFed > k0) kills++;
            if (fist) killsFist++;
            sim.Respawn(p, 0);
        }
        Console.WriteLine($"    bot {(fist ? "fist" : "gun")}: owned [{string.Join(",", sim.loadout.owned)}] slot {slot} weapon {sim.loadout.Weapon}, kills {kills}, charge {sim.fist24.charge:0.00}, hits {hits}, fistKillsFed {sim.fist24.fistKillsFed}, other {sim.fist24.otherKillsFed}");
        hitsPerKill = killsFist > 0 ? (double)hits / killsFist : 0;
        return sim.fist24.Tier >= 3 ? kills : -1;
    }

    /// <summary>`fist --bigpick [--off]`: the BIG PICK's rule on the sim (builder F, contract big-pick's Core half; the screen is the Player's `reel bigpick`).</summary>
    static void BigPickHeadless(string[] args, ref int code)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = off ? N24Blocks.Off : new N24Blocks { bigPick = true, chargeFist = true }; sim.n25Override = N25Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(2424, null, null);
        sim.mover.invulnerable = true;
        BigPick24 bp = sim.bigPick24;
        Need(bp != null && bp.Active, "the BIG PICK is not wired (off)");
        var offers = new List<string>();
        int firstDraft = -1;
        for (int r = 1; r <= 22; r++)
        {
            sim.events.Emit("roundClear", new RoundEvent { number = r });
            if (sim.mods.Pending && firstDraft < 0) firstDraft = r;
            if (sim.mods.Pending)
            {
                List<string> o = sim.mods.Open();
                offers.Add($"round {r}: [{string.Join(", ", o)}]");
                if (bp != null && bp.Active)
                {
                    Need(o.Count == 3, $"round {r} offered {o.Count} cards");
                    Need(sim.mods.BigOffer && !sim.mods.Reroll(), "a reroll was possible or the offer is not a BIG PICK");
                    foreach (string id in o) Need(Array.IndexOf(BigPick24.Kinds, BigPick24.KindOf(id)) >= 0, $"card {id} is not a BIG PICK kind");
                }
                sim.mods.Pick(0);
            }
        }
        foreach (string s in offers) Console.WriteLine("  " + s);
        Console.WriteLine($"  first draft after the clear of round {firstDraft}; {offers.Count} drafts in rounds 1-22");
        if (bp != null && bp.Active)
        {
            Need(firstDraft == 5 && offers.Count == 4, $"drafts at rounds other than 5, 10, 15, 20 ({offers.Count} drafts, first {firstDraft})");
            // each reward on a fresh run
            double pts5 = Reward(cfg, map, "points", 5), pts10 = Reward(cfg, map, "points", 10), pts20 = Reward(cfg, map, "points", 20);
            Console.WriteLine($"  +POINTS: round 5 {pts5:0}, round 10 {pts10:0}, round 20 {pts20:0}");
            Need(pts5 == 1000 && pts10 == 2000 && pts20 == 3000, "the points card did not pay 1000 / 2000 / 3000");
            string heal = RewardLine(cfg, map, "heal", 5), fist = RewardLine(cfg, map, "fist", 5), perk = RewardLine(cfg, map, "perk", 5), pack = RewardLine(cfg, map, "pack", 5), box = RewardLine(cfg, map, "box", 5);
            Console.WriteLine($"  heal: {heal}\n  fist: {fist}\n  perk: {perk}\n  pack: {pack}\n  box: {box}");
            Need(heal.Contains("OK") && fist.Contains("OK") && perk.Contains("OK") && pack.Contains("OK") && box.Contains("OK"), "a reward did not apply");
            // the campaign's layer-end pick (D-7): one pick, no round trigger, the run waits for it
            SurvivalSim ls = BigPickSim(cfg, map);
            bool holdBefore = ls.CampaignBigPickHold(); // queues it
            bool pendingLayer = ls.mods.LayerPick;
            List<string> lo = ls.mods.Open();
            bool holdOpen = ls.CampaignBigPickHold();
            double pts0 = ls.economy.total;
            bool picked = ls.mods.Pick(0);
            bool holdAfter = ls.CampaignBigPickHold();
            bool again = ls.mods.Pending;
            Console.WriteLine($"  layer end: hold {holdBefore} (queued, LayerPick {pendingLayer}), offer [{(lo == null ? "-" : string.Join(", ", lo))}], hold while open {holdOpen}, picked {picked}, hold after {holdAfter}, another pick waiting {again}");
            Need(holdBefore && pendingLayer && lo != null && lo.Count == 3 && holdOpen && picked && !holdAfter && !again, "the layer-end pick did not hold, open, close and release once");
            // a run saved before round 5 and loaded draws the same three cards (the snapshot holds bigPick24's stream); and once more after a pick
            SurvivalSim sa = BigPickSim(cfg, map);
            string snap0 = RunSnapshot.Save(sa.SnapshotSections());
            SurvivalSim sb = BigPickSim(cfg, map);
            RunSnapshot.Load(snap0, sb.SnapshotSections());
            sa.events.Emit("roundClear", new RoundEvent { number = 5 });
            sb.events.Emit("roundClear", new RoundEvent { number = 5 });
            string offA = string.Join(",", sa.mods.Open()), offB = string.Join(",", sb.mods.Open());
            sa.mods.Pick(0);
            sb.mods.Pick(0);
            string snap1 = RunSnapshot.Save(sa.SnapshotSections());
            SurvivalSim sc = BigPickSim(cfg, map);
            RunSnapshot.Load(snap1, sc.SnapshotSections());
            sa.events.Emit("roundClear", new RoundEvent { number = 10 });
            sc.events.Emit("roundClear", new RoundEvent { number = 10 });
            string offA10 = string.Join(",", sa.mods.Open()), offC10 = string.Join(",", sc.mods.Open());
            Console.WriteLine($"  save/load: round 5 [{offA}] vs loaded [{offB}]; after a pick, round 10 [{offA10}] vs loaded [{offC10}]");
            Need(offA == offB && offA10 == offC10, "a loaded run drew different BIG PICK cards");
            // no round trigger in a campaign-style run: rounds 1-4 clears still draft nothing, and the layer pick is not a second draft at round 5
            Need(ls.mods.stacks.Count == 0, "the layer pick stacked a mod");
        }
        else Need(false, "no BIG PICK to test");
        bool ok = fails.Count == 0;
        Console.WriteLine($"bigpick {(ok ? "PASS" : "FAIL")}{(off ? " (row OFF: the RED run, NIGHT21's draft: first draft after round " + firstDraft + ")" : "")}: no draft before round 5, drafts at 5 / 10 / 15 / 20, three big cards, no reroll, rewards apply{(ok ? "" : "; " + string.Join("; ", fails))}");
        code = ok ? 0 : 1;
    }

    static SurvivalSim BigPickSim(GameConfig cfg, SurvivalMap map)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { bigPick = true, chargeFist = true }; sim.n25Override = N25Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(2425, null, null);
        sim.mover.invulnerable = true;
        return sim;
    }

    static double Reward(GameConfig cfg, SurvivalMap map, string id, int round)
    {
        SurvivalSim sim = BigPickSim(cfg, map);
        double before = sim.economy.total;
        sim.bigPick24.Take(id, round);
        return sim.economy.total - before;
    }

    static string RewardLine(GameConfig cfg, SurvivalMap map, string kind, int round)
    {
        SurvivalSim sim = BigPickSim(cfg, map);
        BigPick24 bp = sim.bigPick24;
        string id = kind;
        if (kind == "pack") { string t = bp.packTarget(); if (t == null) return "NO TARGET"; id = "pack:" + t; }
        if (kind == "perk") { string t = bp.perkTarget(); if (t == null) return "NO TARGET"; id = "perk:" + t; }
        if (kind == "box" && !bp.boxAvailable()) return "BOX NOT AVAILABLE (design.box off or busy)";
        if (kind == "heal") { sim.mover.invulnerable = false; sim.health.Damage(60, "test"); sim.mover.invulnerable = true; }
        int guns0 = sim.loadout.owned.Count, perks0 = sim.perks.owned.Count;
        int rolls = 0;
        sim.events.On("boxRoll", _ => rolls++);
        int teddies = 0;
        sim.events.On("boxTeddy", _ => teddies++);
        bp.Take(id, round);
        switch (kind)
        {
            case "heal":
            {
                bool full = sim.health.hp >= sim.health.max;
                int shield0 = 0;
                sim.events.Emit("roundBegin", new RoundEvent { number = round + 1 });
                return $"{(full && sim.health.Shielded ? "OK" : "FAIL")}: hp {sim.health.hp:0}/{sim.health.max:0}, shield after the next roundBegin {sim.health.Shielded}";
            }
            case "fist":
                return $"{(sim.fist24.charge == 1.0 ? "OK" : "FAIL")}: charge {sim.fist24.charge:0.00}";
            case "perk":
                return $"{(sim.perks.owned.Count == perks0 + 1 ? "OK" : "FAIL")}: owned {string.Join(",", sim.perks.owned)} ({id})";
            case "pack":
                return $"{(sim.forge.IsForged(BigPick24.PayloadOf(id)) ? "OK" : "FAIL")}: {id} forged";
            case "box":
            {
                int steps = 0;
                while (steps++ < 3000 && sim.loadout.owned.Count == guns0) { sim.hitstopLeft = 0; ((ScriptedInput)sim.input).Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
                return $"{(sim.loadout.owned.Count == guns0 + 1 && rolls == 1 && teddies == 0 ? "OK" : "FAIL")}: boxRoll {rolls}, teddies {teddies}, guns {guns0} -> {sim.loadout.owned.Count} ({string.Join(",", sim.loadout.owned)}) after {steps} steps";
            }
        }
        return "?";
    }

    static partial void FistCommand(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--bigpick") >= 0) { BigPickHeadless(args, ref code); return; }
        bool off = Array.IndexOf(args, "--off") >= 0;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        SurvivalSim sim = FistSim(cfg, map, !off, 2401);
        Fist24Rules rules = sim.fist24?.rules ?? Fist24Rules.From(cfg);
        Console.WriteLine($"  fist block {(off ? "OFF" : "on")}: gain hit {rules.hit:0.###} / fist kill {rules.fistKill:0.###} / other kill {rules.otherKill:0.###}, tiers {rules.tiers[0]:0.##} / {rules.tiers[1]:0.##} / {rules.tiers[2]:0.##}, reach x J {rules.jumpMult[0]:0.##} / {rules.jumpMult[1]:0.##} / {rules.jumpMult[2]:0.##}, lockout {rules.lockout:0.#} s");
        Need(sim.fist24 != null && sim.fist24.Active, "the fist block is not wired (off)");
        bool found = FistFlat(sim, 18, out Vec3 start, out double yaw, out double wallDist);
        Console.WriteLine($"  flat run: {(found ? $"({start.x:0.0}, {start.y:0.0}, {start.z:0.0}) yaw {yaw:0.00}, wall {wallDist:0.0} m ahead" : "NONE FOUND")}");
        Need(found, "no flat 18 m stretch on the map");
        if (!found || sim.fist24 == null)
        {
            Console.WriteLine($"fist FAIL{(off ? " (row OFF: the RED run)" : "")}: {string.Join("; ", fails)}");
            code = 1;
            return;
        }

        // --- 1 J ------------------------------------------------------------------------------------------------------------
        double J = FistMeasureJump(sim, start, yaw);
        Console.WriteLine($"  1 J = {J:0.000} m (the horizontal reach of a standing jump, forward held, on the Mover); config jumpDistance {rules.jumpDistance:0.000}");
        Need(J > 0 && Math.Abs(J - rules.jumpDistance) <= 0.05, $"J {J:0.000} differs from the config's jumpDistance {rules.jumpDistance:0.000}");

        if (Array.IndexOf(args, "--scan") >= 0) // tuning aid: the ground reach of each notch's impulse for a range of speeds
        {
            for (int tier = 1; tier <= 3; tier++)
                for (double v = 4; v <= 44; v += 4)
                {
                    SurvivalSim sc = FistSim(cfg, map, true, 2499);
                    sc.fist24.rules.speed[tier - 1] = v;
                    double d = FistMeasureFlick(sc, start, yaw, rules.tiers[tier - 1], out _, out _);
                    Console.WriteLine($"  scan tier {tier} speed {v:0}: reach {d:0.00} m (target {rules.jumpMult[tier - 1] * J:0.00})");
                }
            code = 0;
            return;
        }

        // --- 2 idle ---------------------------------------------------------------------------------------------------------
        SurvivalSim idle = FistSim(cfg, map, !off, 2402);
        idle.Respawn(start, 0);
        double maxCharge = 0;
        for (int s = 0; s < 7200; s++)
        {
            FistTick(idle, yaw, 0);
            if (idle.fist24 != null) maxCharge = Math.Max(maxCharge, idle.fist24.charge);
        }
        Console.WriteLine($"  2 idle: 60 s standing about, charge at the end {(idle.fist24?.charge ?? 0):0.000}, highest {maxCharge:0.000}");
        Need(maxCharge == 0, "the charge rose with time or without a hit");

        // --- 3 rates --------------------------------------------------------------------------------------------------------
        SurvivalSim rt = FistSim(cfg, map, !off, 2403);
        rt.Respawn(map.playerSpawn, 0);
        (double ryaw, double rux, double ruz) = ArclineOpenDir(rt, map.playerSpawn, 6);
        rt.fist24.noFlick = true;
        int gSlot = rt.loadout.owned.IndexOf("gauntlet"), rSlot = rt.loadout.owned.IndexOf("revolver");
        double hitGain = 0, killGain = 0, otherGain = 0;
        rt.events.On<Fist24ChargeEvent>("fistCharge", e =>
        {
            if (e.why == "hit") hitGain += e.gain;
            else if (e.why == "fistKill") killGain += e.gain;
            else if (e.why == "otherKill") otherGain += e.gain;
        });
        Husk h1 = rt.husks.Spawn("husk", new Vec3(map.playerSpawn.x + rux * 5, 0, map.playerSpawn.z + ruz * 5), map.playerSpawn);
        for (int s = 0; s < 1500 && h1 != null && h1.alive; s++)
        {
            (double yy, double pp) = ArclineLook(rt.Eye, h1.AimPoint);
            FistTick(rt, yy, pp, 0, false, false, false, true, s == 0 ? gSlot : -1);
        }
        rt.Respawn(map.playerSpawn, 0);
        for (int s = 0; s < 120; s++) FistTick(rt, ryaw, 0, 0, false, false, false, false, s == 0 ? rSlot : -1);
        Husk h2 = rt.husks.Spawn("husk", new Vec3(map.playerSpawn.x + rux * 9, 0, map.playerSpawn.z + ruz * 9), map.playerSpawn);
        for (int s = 0; s < 2000 && h2 != null && h2.alive; s++)
        {
            (double yy, double pp) = ArclineLook(rt.Eye, h2.AimPoint);
            FistTick(rt, yy, pp, 0, false, false, false, true);
        }
        Console.WriteLine($"  3 rates: a husk killed by the fist paid {hitGain:0.000} in hits + {killGain:0.000} for the kill; one killed by the revolver paid {otherGain:0.000} (rates hit {rules.hit:0.###}, kill {rules.fistKill:0.###}, other {rules.otherKill:0.###})");
        Need(Math.Abs(killGain - rules.fistKill) < 1e-9 && Math.Abs(otherGain - rules.otherKill) < 1e-9 && hitGain >= 0 && Math.Abs(hitGain / rules.hit - Math.Round(hitGain / rules.hit)) < 1e-6, "the charge did not rise by the hit / kill rates");

        // --- 4 flick ---------------------------------------------------------------------------------------------------------
        var flickCases = new[] { (rules.tiers[0], rules.jumpMult[0]), (rules.tiers[1], rules.jumpMult[1]), (rules.tiers[2], rules.jumpMult[2]) };
        var measured = new double[3];
        SurvivalSim fk = FistSim(cfg, map, !off, 2404);
        for (int i = 0; i < 3; i++)
        {
            double d = FistMeasureFlick(FistSim(cfg, map, !off, (uint)(2410 + i)), start, yaw, flickCases[i].Item1, out bool flicked, out double spent);
            measured[i] = d;
            double want = flickCases[i].Item2 * J;
            Console.WriteLine($"  4 flick tier {i + 1}: charge {flickCases[i].Item1:0.00} -> {(flicked ? "flicked" : "NO flick")}, spent {spent:0.00}, reached {d:0.00} m (wanted {flickCases[i].Item2:0.##} J = {want:0.00} m, {(want > 0 ? d / want * 100 : 0):0} %)");
            Need(flicked && Math.Abs(spent - flickCases[i].Item1) < 1e-9, $"tier {i + 1} did not flick or did not spend all the charge");
            Need(Math.Abs(d - want) <= 0.15 * want, $"tier {i + 1} reached {d:0.00} m, not within 15 % of {want:0.00}");
        }
        // lockout: a flick, then a staged charge and a punch 1 s later does not flick; 3.2 s later it does
        fk.Respawn(start, 0);
        FistSettle(fk, yaw, 120);
        fk.fist24.charge = 0.4;
        FistTick(fk, yaw, 0, 0, false, false, true);
        int flicks1 = fk.fist24.flicks;
        for (int s = 0; s < 240; s++) FistTick(fk, yaw, 0);
        FistSettle(fk, yaw, 0);
        fk.fist24.charge = 0.4;
        FistTick(fk, yaw, 0, 0, false, false, true);
        int flicksIn = fk.fist24.flicks;
        for (int s = 0; s < 3 * 120; s++) FistTick(fk, yaw, 0);
        fk.fist24.charge = 0.4;
        for (int s = 0; s < 80; s++) FistTick(fk, yaw, 0);
        FistTick(fk, yaw, 0, 0, false, false, true);
        int flicksAfter = fk.fist24.flicks;
        Console.WriteLine($"  4 lockout: first punch flicks {flicks1}; 2 s later (inside {rules.lockout:0.#} s) {flicksIn - flicks1} more; after it {flicksAfter - flicksIn} more");
        Need(flicks1 == 1 && flicksIn == 1 && flicksAfter == 2, "the lockout did not hold 3 s or did not release");

        // --- 5 air ----------------------------------------------------------------------------------------------------------
        SurvivalSim ar = FistSim(cfg, map, !off, 2405);
        ar.Respawn(start, 0);
        FistSettle(ar, yaw, 120);
        ar.fist24.rules.lockout = 0; // only the air rule is on trial here
        for (int s = 0; s < 10; s++) FistTick(ar, yaw, 0, 0, s == 0, true);
        bool airborne = !ar.mover.grounded;
        ar.fist24.charge = 0.4;
        FistTick(ar, yaw, 0, 0, false, true, true);
        int a1 = ar.fist24.flicks;
        ar.fist24.charge = 0.4;
        for (int s = 0; s < 45 && !ar.mover.grounded; s++) FistTick(ar, yaw, 0, 0, false, true); // the punch's own 0.35 s cycle passes
        bool stillAir = !ar.mover.grounded;
        FistTick(ar, yaw, 0, 0, false, false, true);
        int a2 = ar.fist24.flicks;
        for (int s = 0; s < 1200 && !ar.mover.grounded; s++) FistTick(ar, yaw, 0);
        for (int s = 0; s < 120; s++) FistTick(ar, yaw, 0);
        ar.fist24.charge = 0.4;
        FistTick(ar, yaw, 0, 0, false, false, true);
        int a3 = ar.fist24.flicks;
        Console.WriteLine($"  5 air: airborne {airborne} (and {stillAir} 0.35 s on); the first air punch flicks {a1}, a second in the same airtime {a2 - a1} more, after landing {a3 - a2} more");
        Need(airborne && stillAir && a1 == 1 && a2 == a1 && a3 == a2 + 1, "one air flick until grounded did not hold");

        // --- 6 wall ---------------------------------------------------------------------------------------------------------
        double gap = 2.0;
        Vec3 nearWall = new Vec3(start.x + (-Math.Sin(yaw)) * (wallDist - gap), start.y, start.z + (-Math.Cos(yaw)) * (wallDist - gap));
        SurvivalSim wl = FistSim(cfg, map, !off, 2406);
        wl.Respawn(nearWall, 0);
        FistSettle(wl, yaw, 120);
        var eyeP = new Vec3(wl.mover.position.x, wl.mover.position.y + rules.bodyHeight, wl.mover.position.z);
        wl.world.RayIntersect(eyeP, new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw)), out RayHit wh);
        Vec3 before = wl.mover.position;
        wl.fist24.charge = 1.0;
        double minGap = 1e9;
        FistTick(wl, yaw, 0, 0, false, false, true);
        bool blockedOrCut = true;
        for (int s = 0; s < 360; s++)
        {
            FistTick(wl, yaw, 0);
            var pos = wl.mover.position;
            wl.world.RayIntersect(new Vec3(pos.x, pos.y + rules.bodyHeight, pos.z), new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw)), out RayHit wh2);
            minGap = Math.Min(minGap, wh2.distance);
        }
        double wallMoved = FistHoriz(before, wl.mover.position);
        Console.WriteLine($"  6 wall: wall {wh.distance:0.00} m ahead; a FULL flick (wanted {wl.fist24.lastWanted:0.00} m) was cut to {wl.fist24.lastAllowed:0.00} m; blocked {wl.fist24.blocked}, flicks {wl.fist24.flicks}; the player moved {wallMoved:0.00} m and never came nearer than {minGap:0.00} m to the wall (body radius {wl.mover.Config.body.capsuleRadius:0.##})");
        Need(wl.fist24.lastAllowed < wl.fist24.lastWanted, "the flick was not cut by the wall");
        Need(minGap >= wl.mover.Config.body.capsuleRadius - 0.01, "the player ended inside the wall");
        SurvivalSim wl2 = FistSim(cfg, map, !off, 2407);
        Vec3 touching = new Vec3(start.x + (-Math.Sin(yaw)) * (wallDist - 0.8), start.y, start.z + (-Math.Cos(yaw)) * (wallDist - 0.8));
        wl2.Respawn(touching, 0);
        FistSettle(wl2, yaw, 120);
        wl2.fist24.charge = 1.0;
        FistTick(wl2, yaw, 0, 0, false, false, true);
        Console.WriteLine($"  6 wall (against it): a FULL flick with {wl2.fist24.lastAllowed:0.00} m of room: flicks {wl2.fist24.flicks}, blocked {wl2.fist24.blocked}, charge kept {wl2.fist24.charge:0.00}");
        Need(wl2.fist24.flicks == 0 && wl2.fist24.blocked == 1 && wl2.fist24.charge == 1.0, "a flick into a wall at arm's length launched or spent the charge");

        // --- 7 hurt ---------------------------------------------------------------------------------------------------------
        SurvivalSim hu = FistSim(cfg, map, !off, 2408);
        hu.mover.invulnerable = false;
        hu.fist24.charge = 0.6;
        hu.health.Damage(5, "test");
        Console.WriteLine($"  7 hurt: charge 0.60 -> {hu.fist24.charge:0.000} after a hit on the player (a third lost = 0.400)");
        Need(Math.Abs(hu.fist24.charge - 0.4) < 1e-6, "a hit did not take a third of the charge");

        // --- 8 bots ---------------------------------------------------------------------------------------------------------
        int firstFist = FistKillsToFull(cfg, map, start, yaw, true, new[] { 0.02, 0.08, 0.03 }, out double hpk0, out _);
        int firstGun = FistKillsToFull(cfg, map, start, yaw, false, new[] { 0.02, 0.08, 0.03 }, out _, out _);
        int finalFist = FistKillsToFull(cfg, map, start, yaw, true, null, out double hpk, out _);
        int finalGun = FistKillsToFull(cfg, map, start, yaw, false, null, out _, out _);
        Console.WriteLine($"  8 bots: first rates (0.02 / 0.08 / 0.03): fist-only {firstFist} kills to FULL ({hpk0:0.0} hits per husk), revolver-only {firstGun}; final rates ({rules.hit:0.###} / {rules.fistKill:0.###} / {rules.otherKill:0.###}): fist-only {finalFist} kills to FULL ({hpk:0.0} hits per husk), revolver-only {finalGun}");
        Need(finalFist >= 25 && finalFist <= 35, $"the fist-only bot needs {finalFist} kills to FULL, not 25-35");
        Need(finalGun > finalFist, $"the revolver-only bot ({finalGun}) is not slower than the fist-only bot ({finalFist})");

        bool ok = fails.Count == 0;
        Console.WriteLine($"fist {(ok ? "PASS" : "FAIL")}{(off ? " (row OFF: the RED run)" : "")}: J {J:0.00} m; idle 60 s +{maxCharge:0.###}; flicks reach {measured[0]:0.0} / {measured[1]:0.0} / {measured[2]:0.0} m (0.5 / 1.0 / {rules.jumpMult[2]:0.##} J); lockout {rules.lockout:0.#} s; one air flick; wall-cut; a third lost when hit; kills to FULL fist-only {finalFist} (first rates {firstFist}), revolver-only {finalGun}{(ok ? "" : "; " + string.Join("; ", fails))}");
        code = ok ? 0 : 1;
    }
}
