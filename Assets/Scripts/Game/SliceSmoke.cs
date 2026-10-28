// NIGHT7's one new tool (the brief, section 4): the slice smoke. Run in Play mode with
//   unity command eval --code 'Skygrind.Game.SliceSmoke.Begin(); return "started";'
// (a Game-assembly component: Unity won't add an Editor-assembly MonoBehaviour to a scene object) then read SliceSmoke.Report (eval 'return SliceSmoke.Report;') or the console's [smoke] lines. A scripted run: an aim
// bot (the look turned onto the nearest husk's chest, the trigger held through SurvivalGame.forcedCombat) clears round 1,
// waits out the break, sees round 2 start, buys ARMORY at its gate (hold E), dies, sees DOWN then RESULTS, restarts.
// NIGHT8 U2-05: the death is a husk's blow (the recap must name it), RESULTS has a personal-best line, and the restart
// goes through SurvivalGame.Restart (R) from RESULTS, then again from DOWN (skipping the wait), each timed under 1 s.
// Each step has its own timeout; the whole run stops itself after 10 minutes. God mode: HP 1e6 until the death step.
using System.Collections;
using System.Text;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
public sealed partial class SliceSmoke : MonoBehaviour
{
    public static string Report = "not run";
    public static bool Done;
    readonly StringBuilder log = new StringBuilder();
    float started;
    int failures;

    static readonly System.Collections.Generic.List<UnityEngine.InputSystem.InputDevice> realPadsOff = new System.Collections.Generic.List<UnityEngine.InputSystem.InputDevice>();

    /// <summary>NIGHT24 (the NIGHT22-23 smoke failures `virtual pad`, `pad only`, `grind22`: a real "Wireless Gamepad" was plugged into the
    /// PC and its reports reached the pad readers beside the smoke's virtual pads; they fail the same with -skygrindOff24): the three
    /// virtual-pad steps call this before they add their pad and RestoreRealPads after they remove it, so the other steps run with the
    /// real pad as the player had it. The reel has disabled real pads for its whole run since NIGHT18.</summary>
    public static void DisableRealPads()
    {
        realPadsOff.Clear();
        foreach (UnityEngine.InputSystem.InputDevice d in UnityEngine.InputSystem.InputSystem.devices)
        {
            if (!(d is UnityEngine.InputSystem.Gamepad || d is UnityEngine.InputSystem.Joystick) || !d.enabled) continue;
            UnityEngine.InputSystem.InputSystem.DisableDevice(d);
            realPadsOff.Add(d);
        }
        Debug.Log($"[smoke] real pads disabled for a virtual-pad step: {realPadsOff.Count}");
    }

    /// <summary>NIGHT24: g.ToTitle() on a TITLE that is already showing keeps the focus where an earlier step left it (the CODE row after
    /// the NIGHT22 CODE step: `pad only` read TITLE ('CODE' selected) and A pressed CODE, not PLAY); the pad steps clear it so UiNav
    /// selects PLAY, as it does for a player coming back from a run (a new screen).</summary>
    public static void ClearMenuSelection()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    public static void RestoreRealPads()
    {
        foreach (UnityEngine.InputSystem.InputDevice d in realPadsOff)
            if (d != null && d.added) UnityEngine.InputSystem.InputSystem.EnableDevice(d);
        realPadsOff.Clear();
    }

    public static void Begin()
    {
        // NIGHT11: one smoke at a time. The CLI helper retries an eval whose answer timed out (unity-loop.sh ucr), and a
        // retried Begin started a second smoke beside the first in Play check 3 (its StartRun reset the first one's run).
        if (FindFirstObjectByType<SliceSmoke>() != null)
        {
            Debug.Log("[smoke] already running: this Begin() ignored");
            return;
        }
        Report = "running";
        Done = false;
        var go = new GameObject("Slice smoke");
        go.AddComponent<SliceSmoke>();
    }

    void Line(string s)
    {
        string l = $"[smoke] {Time.realtimeSinceStartup - started:0.0}s {s}";
        Debug.Log(l);
        log.AppendLine(l);
        Report = log.ToString();
    }

    void Check(bool ok, string what)
    {
        if (!ok) failures++;
        Line($"{(ok ? "PASS" : "FAIL")} {what}");
        N55Checked(ok); // NIGHT55 S: a failing step's "n55 why:" line (focus / device / flow state, what paused the run)
    }

    IEnumerator Start()
    {
        started = Time.realtimeSinceStartup;
        SurvivalGame g = SurvivalGame.Instance;
        if (g == null)
        {
            Line("FAIL no SurvivalGame (open Survival.unity and press Play)");
            Finish();
            yield break;
        }
        // NIGHT15 U3-28.5: the smoke plays the old arena (its steps know its spots); the Player's automation boots it always,
        // the Editor boots the saved MAP pick.
        if (g.MapId != RunSetup.DefaultMap)
        {
            Line($"FAIL the smoke plays the arena: this composition plays '{g.MapId}' (pick ARENA on TITLE, or run the Player's -skygrindSmoke)");
            Finish();
            yield break;
        }
        N55Begin(g); // NIGHT55 S (SliceSmoke.N55.cs): IgnoreFocus, the focus / device log, the guard that resolves a pause the smoke did not make
        g.noRecords = true; // the smoke's runs never become the player's personal bests
        g.noLab = true; // and play the web's rules whatever the LAB tab says (NIGHT8 U2-16)
        // NIGHT12: the profile is snapshotted and never written while the smoke runs (its meta steps record into it in
        // memory); Finish restores it in place.
        profileSnap = g.profile?.ToJson();
        g.profileReadOnly = true;
        g.setup.mode = RunSetup.Survival;
        g.setup.heat = 0;
        g.StartRun(424242);
        g.health.max = g.health.hp = 1e6;
        Check(g.flow == Flow.Playing && g.run.state == SurvivalRun.State.Break, $"a run starts: {g.flow}, {g.run.state}, round {g.run.round}");

        // 1. Clear round 1 with the aim bot.
        float t0 = Time.realtimeSinceStartup;
        int kills0 = g.run.stats.kills;
        while (!(g.run.round == 2 && g.run.state == SurvivalRun.State.Break) && Time.realtimeSinceStartup - t0 < 150)
        {
            Aim(g);
            yield return null;
        }
        g.forcedCombat = null;
        Check(g.run.round == 2 && g.run.state == SurvivalRun.State.Break, $"round 1 cleared in {Time.realtimeSinceStartup - t0:0} s: {g.run.stats.kills - kills0} kills, {g.run.stats.headshots} headshots, points {g.economy.total:0}");

        // 2. The break, then round 2.
        t0 = Time.realtimeSinceStartup;
        while (g.run.state != SurvivalRun.State.Fight && Time.realtimeSinceStartup - t0 < 30) yield return null;
        Check(g.run.state == SurvivalRun.State.Fight && g.run.round == 2, $"round 2 begins after the break ({Time.realtimeSinceStartup - t0:0.0} s)");

        // 3. Buy ARMORY at its gate: stand on the spot facing it, hold E.
        BuySpot spot = g.zones.spots.Find(s => s.zoneId == "armory");
        g.economy.total = System.Math.Max(g.economy.total, spot.price + 100);
        double before = g.economy.total;
        g.Respawn(spot.spot.pos, -90); // the ARMORY spot faces +x
        t0 = Time.realtimeSinceStartup;
        while (!g.zones.IsOpen("armory") && Time.realtimeSinceStartup - t0 < 10)
        {
            g.forcedCombat = new CombatInput { weapon = -1, interact = new Skygrind.Core.Button { held = true } };
            yield return null;
        }
        g.forcedCombat = null;
        Check(g.zones.IsOpen("armory") && g.economy.total == before - spot.price, $"ARMORY bought: open {g.zones.IsOpen("armory")}, points {before:0} -> {g.economy.total:0}");

        // 3b. NIGHT8 U2-11: a perk buy: stand at VIGOR's machine facing it (its front is the web model's +z turned by its
        // yaw), hold E: VIGOR owned at its taxed price, one more stamina pip.
        PerkSpot vs = g.map.perkSpots.Find(s => s.perk == "vigor");
        int vzone = ZoneMath.ZoneIndexAt(g.map.zoneParts, vs.pos.x, vs.pos.y + 1, vs.pos.z);
        if (vzone >= 0 && g.zones.LockedTag(vzone)) g.zones.Open(g.map.zones[vzone].id, free: true);
        double vigorPrice = g.perks.Price("vigor");
        g.economy.total = System.Math.Max(g.economy.total, vigorPrice + 100);
        double beforePerk = g.economy.total;
        g.Respawn(new Vec3(vs.pos.x + System.Math.Sin(vs.yaw) * 1.5, vs.pos.y, vs.pos.z + System.Math.Cos(vs.yaw) * 1.5), vs.yaw * 180 / System.Math.PI);
        // Let go of E first: a buy latches until E is released (one buy per press).
        t0 = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - t0 < 0.15) yield return null;
        t0 = Time.realtimeSinceStartup;
        while (!g.perks.Owns("vigor") && Time.realtimeSinceStartup - t0 < 10)
        {
            g.forcedCombat = new CombatInput { weapon = -1, interact = new Skygrind.Core.Button { held = true } };
            g.input.SetLook(vs.yaw, 0); // keep facing the machine (SNAP would turn the look to the round's husks)
            yield return null;
        }
        g.forcedCombat = null;
        // NIGHT17 U3-30.4: with design.stamina's bar VIGOR adds bonusBars to the bar (1 -> 2); with the rules off, one pip (the web's)
        bool staminaBar = g.mover.stamina.rules.Any;
        bool vigorStamina = staminaBar ? g.mover.StaminaMax == g.cfg.Num("design.stamina.bar.max") + g.cfg.Num("design.stamina.bar.bonusBars") : g.mover.MaxPips == g.cfg.Num("dash.maxPips") + 1;
        Check(g.perks.Owns("vigor") && g.economy.total == beforePerk - vigorPrice && vigorStamina, $"VIGOR bought: owned {g.perks.Owns("vigor")}, points {beforePerk:0} -> {g.economy.total:0} (price {vigorPrice:0}), stamina {(staminaBar ? $"bar {g.mover.StaminaMax}" : $"pips {g.mover.MaxPips}")}");

        // 3c. NIGHT10 U3-13.8: the Mystery Box at its first spot (the starting zone): hold E for its price, the reveal, then
        // hold E again to take the gun (it joins the loadout as a wall buy does). The first pull never gives the teddy.
        // God mode again (VIGOR's perks.js apply resets health.max) and the round's husks frozen: these steps take ~30 s of round 2.
        g.health.max = g.health.hp = 1e6;
        g.husks.frozen = true;
        if (g.box != null)
        {
            var boxEvents = new System.Collections.Generic.List<string>();
            System.Action<object> onOpen = _ => boxEvents.Add("boxOpen"), onRoll = o => boxEvents.Add("boxRoll " + ((BoxRollEvent)o).weapon), onTake = _ => boxEvents.Add("boxTake");
            g.events.On("boxOpen", onOpen);
            g.events.On("boxRoll", onRoll);
            g.events.On("boxTake", onTake);
            BoxSpot bs = g.box.Current;
            g.economy.total = System.Math.Max(g.economy.total, g.box.price + 100);
            double beforeBox = g.economy.total;
            int ownedBefore = g.loadout.owned.Count;
            g.Respawn(new Vec3(bs.pos.x + System.Math.Sin(bs.yaw) * 1.6, bs.pos.y, bs.pos.z + System.Math.Cos(bs.yaw) * 1.6), bs.yaw * 180 / System.Math.PI);
            t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 0.15) yield return null; // E released first
            t0 = Time.realtimeSinceStartup;
            while (g.box.state == BoxState.Idle && Time.realtimeSinceStartup - t0 < 10)
            {
                g.forcedCombat = new CombatInput { weapon = -1, interact = new Skygrind.Core.Button { held = true } };
                g.input.SetLook(bs.yaw, 0);
                yield return null;
            }
            g.forcedCombat = null;
            double paid = beforeBox - g.economy.total;
            t0 = Time.realtimeSinceStartup;
            while (g.box.state == BoxState.Revealing && Time.realtimeSinceStartup - t0 < 10) yield return null;
            string offered = g.box.Offered;
            t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 0.15) yield return null; // E released between the buy and the take
            t0 = Time.realtimeSinceStartup;
            while (g.box.Offered != null && Time.realtimeSinceStartup - t0 < 10)
            {
                g.forcedCombat = new CombatInput { weapon = -1, interact = new Skygrind.Core.Button { held = true } };
                g.input.SetLook(bs.yaw, 0);
                yield return null;
            }
            g.forcedCombat = null;
            g.events.Off("boxOpen", onOpen);
            g.events.Off("boxRoll", onRoll);
            g.events.Off("boxTake", onTake);
            Check(paid == g.box.price && offered != null && g.loadout.owned.Contains(offered) && g.loadout.owned.Count == ownedBefore + 1 && boxEvents.Contains("boxTake"),
                $"Mystery Box rolled at spot {g.box.spot} ({bs.zone}): paid {paid:0}, offered {offered}, taken: owned [{string.Join(", ", g.loadout.owned)}]; events [{string.Join(", ", boxEvents)}]");
        }
        else Line("Mystery Box off (design.box.enabled false): no box step");

        // 3d. NIGHT10 U3-13.4: the ARMORY wall's STITCHER, COMET and LONGWAVE: buy each at its slot (hold E at the buy point,
        // facing the wall; skipped if the box already gave it), turn into the hall and kill a husk placed on open floor in
        // clear sight (found by casting the collider), the trigger held until it dies.
        var kills = new System.Collections.Generic.List<string>();
        System.Action<object> onKill = o => kills.Add(((KillEvent)o).weapon);
        g.events.On("kill", onKill);
        foreach (string gun in new[] { "nailgun", "rocket", "rail" })
        {
            string gunName = g.cfg.Str($"weapons.catalog.{gun}.name");
            var names = new System.Collections.Generic.List<string>();
            foreach (var kv in g.cfg.Obj("weapons.prices")) if (g.cfg.Str($"weapons.catalog.{kv.Key}.kind") != "melee") names.Add(kv.Key);
            double wz = g.map.armoryWallZs[names.IndexOf(gun)], buyX = g.map.armoryWallX + g.map.armoryWallFacing * g.cfg.Num("wallBuy.buyOut");
            bool bought = g.loadout.owned.Contains(gun);
            string how = bought ? "from the box" : "";
            if (!bought)
            {
                double price = g.cfg.Num($"weapons.prices.{gun}");
                g.economy.total = System.Math.Max(g.economy.total, price + 100);
                double beforeGun = g.economy.total;
                double wallYaw = g.map.armoryWallFacing < 0 ? -System.Math.PI / 2 : System.Math.PI / 2; // facing the wall
                g.Respawn(new Vec3(buyX, 0, wz), wallYaw * 180 / System.Math.PI);
                t0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t0 < 0.15) yield return null;
                t0 = Time.realtimeSinceStartup;
                while (!g.loadout.owned.Contains(gun) && Time.realtimeSinceStartup - t0 < 10)
                {
                    g.forcedCombat = new CombatInput { weapon = -1, interact = new Skygrind.Core.Button { held = true } };
                    g.input.SetLook(wallYaw, 0);
                    yield return null;
                }
                g.forcedCombat = null;
                how = $"bought {beforeGun:0} -> {g.economy.total:0}";
            }
            Vec3 eyeAt = new Vec3(buyX, g.cfg.Num("body.eyeHeight"), wz);
            Husk target = PlaceTarget(g, eyeAt, g.map.armoryWallFacing); // into the hall: the plates face it
            if (target == null)
            {
                Check(false, $"{gunName}: no open floor in sight for a target");
                continue;
            }
            g.Respawn(new Vec3(buyX, 0, wz), 0);
            int slot = g.loadout.owned.IndexOf(gun);
            kills.Clear();
            // NIGHT29 D-1: a weapon switch waits for the current attack's cycle (the lockout): idle until the last gun's cycle (the rocket's is about 1.5 s) is over
            g.forcedCombat = null;
            t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 2.0) yield return null;
            t0 = Time.realtimeSinceStartup;
            while (target.alive && Time.realtimeSinceStartup - t0 < 12)
            {
                Vec3 to = target.AimPoint - g.Eye;
                g.input.SetLook(System.Math.Atan2(-to.x, -to.z), System.Math.Atan2(to.y, System.Math.Sqrt(to.x * to.x + to.z * to.z)));
                g.forcedCombat = new CombatInput { weapon = slot, fire = new Skygrind.Core.Button { held = true, pressed = true } };
                yield return null;
            }
            g.forcedCombat = null;
            Check(!target.alive && kills.Contains(gun), $"{gunName} ({how}): a husk {Vec3.DistanceTo(target.position, new Vec3(buyX, 0, wz)):0.0} m away killed in {Time.realtimeSinceStartup - t0:0.0} s; kill weapons [{string.Join(", ", kills)}]");
        }
        g.events.Off("kill", onKill);
        g.husks.frozen = false;

        // 4. Death (a husk's blow, from 3 m ahead) -> DOWN with the recap -> RESULTS with the personal-best line.
        Vec3 f = g.Feet;
        g.health.Damage(g.health.max * 2, "husk", new Vec3(f.x, f.y, f.z - 3));
        Check(g.flow == Flow.Down && g.lastDeath?.killer == "HUSK", $"death: {g.flow}, the recap names the killer: {g.lastDeath?.killer} ({g.lastDeath?.parts.Count} source(s) in the last {g.cfg.Num("ui.recap.window")} s)");
        t0 = Time.realtimeSinceStartup;
        while (g.flow != Flow.Results && Time.realtimeSinceStartup - t0 < 10) yield return null;
        Check(g.flow == Flow.Results && g.lastStats != null && g.lastStats.round == 2, $"results: {g.flow}, round {g.lastStats?.round}, kills {g.lastStats?.kills}, zones {string.Join(",", g.lastStats?.zones ?? new System.Collections.Generic.List<string>())}, killed by {g.lastStats?.killedBy}");
        Check(!string.IsNullOrEmpty(g.lastPb.text), $"the personal-best line: '{g.lastPb.text}' (new best {g.lastPb.newBest})");

        // 5. Restart through R (SurvivalGame.Restart, what the restart binding calls) from RESULTS: a fresh run at once.
        var sw = System.Diagnostics.Stopwatch.StartNew();
        bool restarted = g.Restart();
        sw.Stop();
        Check(restarted && g.flow == Flow.Playing && g.run.round == 1 && g.health.hp == g.health.max && g.economy.total == g.cfg.Num("points.start") && !g.zones.IsOpen("armory") && sw.Elapsed.TotalSeconds < 1, $"restart (R) from RESULTS in {sw.Elapsed.TotalMilliseconds:0} ms: {g.flow}, round {g.run.round}, hp {g.health.hp}, points {g.economy.total:0}, ARMORY locked again {!g.zones.IsOpen("armory")}");

        // 6. And from DOWN: R skips the wait (the run is still judged, then a new one starts).
        g.health.Damage(g.health.max * 2, "swarmer", new Vec3(f.x + 2, f.y, f.z));
        Check(g.flow == Flow.Down, $"second death: {g.flow}, killed by {g.lastDeath?.killer}");
        sw.Restart();
        restarted = g.Restart();
        sw.Stop();
        Check(restarted && g.flow == Flow.Playing && g.run.round == 1 && g.lastStats != null && g.lastStats.round == 1 && sw.Elapsed.TotalSeconds < 1, $"restart (R) from DOWN in {sw.Elapsed.TotalMilliseconds:0} ms: {g.flow}, round {g.run.round}; the ended run recorded (round {g.lastStats?.round}, '{g.lastPb.text}')");

        // 7. NIGHT8 U2-10: the spitter round (director.rounds.introduce.spitter) through the debug round skip: spitters
        // spawn and lob at the (god-mode, still) player.
        // The round's squads are drawn from the run seed (Director.StartWave), and R restarts on the clock's seed, which can
        // plan a round 6 with no spitter at all (2 of its 6 squad templates have one). So this step starts a run on the
        // first seed whose opening squads hold a spitter (NIGHT8 U2-16: the step failed on such a draw at 19:10).
        int spitRound = (int)g.cfg.Num("director.rounds.introduce.spitter");
        DirectorMath.D plan = DirectorMath.From(g.cfg);
        double stride = g.cfg.Num("director.rounds.seedStride");
        int opening = g.cfg.Int("director.openingSquads");
        uint spitSeed = 1;
        for (uint s = 1; s < 1000; s++)
        {
            var squads = DirectorMath.PlanSquads(spitRound, new Mulberry32(s + spitRound * stride).Next, plan, Director.Available);
            if (squads.GetRange(0, System.Math.Min(opening, squads.Count)).Exists(q => q.Contains("spitter")))
            {
                spitSeed = s;
                break;
            }
        }
        g.StartRun(spitSeed);
        g.health.max = g.health.hp = 1e6;
        g.run.DebugSkipTo(spitRound);
        t0 = Time.realtimeSinceStartup;
        while (!(g.run.round == spitRound && g.run.state == SurvivalRun.State.Fight) && Time.realtimeSinceStartup - t0 < 15) yield return null;
        int volleys0 = g.husks.acid.volleys, seen = 0;
        while (Time.realtimeSinceStartup - t0 < 45 && (seen == 0 || g.husks.acid.volleys == volleys0))
        {
            seen = System.Math.Max(seen, g.husks.CountOf("spitter"));
            yield return null;
        }
        Check(g.run.round == spitRound && seen > 0 && g.husks.acid.volleys > volleys0, $"round {g.run.round}: {seen} spitter(s) alive at once, {g.husks.acid.volleys - volleys0} volley(s) in {Time.realtimeSinceStartup - t0:0} s");

        // 8-10. NIGHT11: the run's shape (its own coroutine).
        yield return RunShape(g);
        // 11. NIGHT12: a contract and its unlock (its own coroutine; NIGHT24: the DAILY step 12 is gone and the later steps renumbered).
        yield return Meta(g);
        // 12-. NIGHT13: the night's steps (SliceSmoke.Night13.cs).
        yield return Night13(g);
        // 17-. NIGHT14: the night's steps (SliceSmoke.Night14.cs).
        yield return Night14(g);
        // 22-. NIGHT15: the pad only, the new map's AUTOPILOT (SliceSmoke.Night15.cs).
        yield return Night15(g);
        yield return Grind22(g); // NIGHT22 (builder W; SliceSmoke.N22.Grind.cs): THE GRIND from the MODE row, last because it changes the run setup
        g.ToTitle();
        Finish();
    }

    /// <summary>
    /// NIGHT12 smoke steps (the live profile, recorded in memory only: profileReadOnly, restored by Finish). 11: a contract
    /// completes in a run (the first-unlock contract, from a cleared record) and its unlock lands in the profile, with
    /// contractDone. (NIGHT24: the DAILY run on HEAT 1 that was step 12 is removed with the DAILY.)
    /// </summary>
    IEnumerator Meta(SurvivalGame g)
    {
        // 11. A contract and its unlock.
        string id = g.contracts.FirstUnlockId;
        Contracts.Row row = g.contracts.rows.Find(r => r.id == id);
        if (row == null)
        {
            Check(false, $"contract: no first-unlock contract ({id})");
        }
        else
        {
            g.profile.contracts.Remove(id); // a fresh record for this check (restored at the end)
            if (row.unlock != null) g.profile.flags.Remove(row.unlock);
            g.contracts.Reload();
            var done = new System.Collections.Generic.List<string>();
            System.Action<object> onDone = o => done.Add(((ContractEvent)o).id);
            g.events.On(MetaEvents.ContractDone, onDone);
            g.noRecords = false; // this run records (into the profile in memory: profileReadOnly)
            g.StartRun(515151);
            g.health.max = g.health.hp = 1e6;
            float t0 = Time.realtimeSinceStartup;
            while (!done.Contains(id) && Time.realtimeSinceStartup - t0 < 120)
            {
                g.health.hp = g.health.max;
                // The breaks run out on their own (a DebugSkipTo each frame restarted the break forever: Play check 2's FAIL).
                KillAura(g);
                yield return null;
            }
            g.events.Off(MetaEvents.ContractDone, onDone);
            row = g.contracts.rows.Find(r => r.id == id);
            bool flagged = row.unlock == null || g.profile.Flag(row.unlock);
            Check(done.Contains(id) && row.done && g.profile.Contract(id).done && flagged && g.contracts.doneThisRun.Contains(id),
                $"contract {row.name} ({id}, target {row.target}) completed in round {g.run.round}: contractDone {done.Contains(id)}, profile done {g.profile.Contract(id).done}, unlock '{row.unlock}' in the profile {flagged} ({row.unlockName})");
            g.noRecords = true;
        }

    }

    /// <summary>NIGHT11: an Editor Play check sets this (a folder under the project) to take the run shape's captures.</summary>
    public static string CaptureDir;

    /// <summary>NIGHT11: when set, only the capture of that name is taken (the night's capture cap).</summary>
    public static string CaptureOnly;

    void Capture(string name)
    {
        if (string.IsNullOrEmpty(CaptureDir)) return;
        if (!string.IsNullOrEmpty(CaptureOnly) && name != CaptureOnly) return;
        string path = System.IO.Path.Combine(CaptureDir, name);
        ScreenCapture.CaptureScreenshot(path);
        Line($"capture {path}");
    }

    readonly System.Collections.Generic.List<Enemy> doomed = new System.Collections.Generic.List<Enemy>();

    /// <summary>Kill every live enemy as the player's (the aura the run-shape steps clear rounds with; god mode on).</summary>
    void KillAura(SurvivalGame g)
    {
        doomed.Clear();
        foreach (Enemy e in g.enemies.all) if (e.alive) doomed.Add(e);
        foreach (Enemy e in doomed) g.loadout.Damage(e, 1e6, false, "revolver", g.Eye);
    }

    /// <summary>Stand at a buy point facing yaw (rad) and hold E until done() or the timeout; E released first.</summary>
    IEnumerator HoldE(SurvivalGame g, Vec3 at, double yaw, System.Func<bool> done, float timeout = 10)
    {
        g.Respawn(at, yaw * 180 / System.Math.PI);
        float t0 = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - t0 < 0.15) yield return null; // a buy latches until E is released
        t0 = Time.realtimeSinceStartup;
        while (!done() && Time.realtimeSinceStartup - t0 < timeout)
        {
            g.forcedCombat = new CombatInput { weapon = -1, interact = new Skygrind.Core.Button { held = true } };
            g.input.SetLook(yaw, 0);
            g.health.hp = g.health.max;
            yield return null;
        }
        g.forcedCombat = null;
    }

    /// <summary>
    /// NIGHT11 smoke steps. 8: the power switch (design.power), then a trap kill (the SHOCK PLATE at LANDING, bought after the
    /// power). 9: round 6 (debug skip): the HOLD ring held; the PROTOCOL's four relays lit (the zones opened free); the SURGE
    /// and the round cleared by the kill aura: THE PROTOCOL complete (every gun forged, +2500). 10: exfil at round 11 (debug
    /// skip): the radio in the break, the site reached, the horde killed: RESULTS says EXFIL with a grade. God mode
    /// throughout (re-asserted every frame).
    /// </summary>
    IEnumerator RunShape(SurvivalGame g)
    {
        var seen = new System.Collections.Generic.List<string>();
        System.Action<object> onPower = _ => seen.Add("powerOn"), onTrapKill = o => seen.Add("trapKill " + ((TrapKillEvent)o).kind),
            onHeld = o => seen.Add("objectiveDone " + ((ObjectiveDoneEvent)o).bonus), onLit = o => seen.Add("relayLit " + ((RelayLitEvent)o).id),
            onSurge = _ => seen.Add("protocolSurge"), onComplete = _ => seen.Add("protocolComplete"), onCall = _ => seen.Add("exfilCall"),
            onStart = _ => seen.Add("exfilStart"), onReach = _ => seen.Add("exfilReach"), onDone = _ => seen.Add("exfilDone"), onFail = _ => seen.Add("exfilFail");
        string[] names = { "powerOn", "trapKill", "objectiveDone", "relayLit", "protocolSurge", "protocolComplete", "exfilCall", "exfilStart", "exfilReach", "exfilDone", "exfilFail" };
        System.Action<object>[] handlers = { onPower, onTrapKill, onHeld, onLit, onSurge, onComplete, onCall, onStart, onReach, onDone, onFail };
        for (int i = 0; i < names.Length; i++) g.events.On(names[i], handlers[i]);
        g.StartRun(1111);
        g.health.max = g.health.hp = 1e6;

        // 8. The power switch, then the SHOCK PLATE.
        Traps.Trap plate = g.traps.Get("plate");
        double points0 = g.economy.total;
        if (g.power.enabled)
        {
            g.zones.Open(g.power.spot.zone, free: true);
            bool trapBefore = plate.buyable.available();
            BoxSpot ps = g.power.spot;
            yield return HoldE(g, new Vec3(ps.pos.x + System.Math.Sin(ps.yaw) * 1.4, ps.pos.y, ps.pos.z + System.Math.Cos(ps.yaw) * 1.4), ps.yaw, () => g.power.On);
            bool trapAfter = plate.buyable.available();
            Check(g.power.On && seen.Contains("powerOn") && !trapBefore && trapAfter && g.economy.total == points0,
                $"the power switch thrown at ({ps.pos.x}, {ps.pos.y}, {ps.pos.z}) in {ps.zone}: on {g.power.On}, free (points {points0:0} -> {g.economy.total:0}); the plate for sale before {trapBefore}, after {trapAfter}");
            Capture("n11-power-on.png");
        }
        else Line("the power switch off (design.power.enabled false): the traps need no power (the web's rule)");
        g.economy.total = System.Math.Max(g.economy.total, plate.spec.price + 100);
        double beforeTrap = g.economy.total;
        g.husks.frozen = true;
        Husk victim = g.husks.Spawn(new Vec3(plate.spot.pos.x, plate.spot.pos.y, plate.spot.pos.z), g.mover.position);
        yield return HoldE(g, new Vec3(plate.spot.sx, plate.spot.pos.y, plate.spot.sz), System.Math.Atan2(-(plate.spot.pos.x - plate.spot.sx), -(plate.spot.pos.z - plate.spot.sz)), () => plate.active > 0);
        float tk = Time.realtimeSinceStartup;
        while (victim != null && victim.alive && Time.realtimeSinceStartup - tk < 10)
        {
            g.health.hp = g.health.max;
            yield return null;
        }
        Check(victim != null && !victim.alive && seen.Contains("trapKill husk") && beforeTrap - g.economy.total >= plate.spec.price - 1e-9 - 1000,
            $"SHOCK PLATE bought ({beforeTrap:0} -> {g.economy.total:0} incl. the kill's points), the husk on it killed in {Time.realtimeSinceStartup - tk:0.0} s: events [{string.Join(", ", seen)}]");
        g.husks.frozen = false;

        // 9. Round 6: HOLD and THE PROTOCOL.
        g.zones.OpenAll();
        g.run.DebugSkipTo(6);
        float t0 = Time.realtimeSinceStartup;
        while (!(g.run.round == 6 && g.run.state == SurvivalRun.State.Fight) && Time.realtimeSinceStartup - t0 < 15)
        {
            g.health.hp = g.health.max;
            yield return null;
        }
        Check(g.run.round == 6 && g.objectives.Type == Objectives.Hold && g.protocol.Awake, $"round 6: the objective {g.objectives.Type}, the PROTOCOL awake {g.protocol.Awake} (power on {g.power.On})");
        g.husks.frozen = true;
        foreach (Protocol.Relay r in g.protocol.relays)
        {
            if (r.secret) continue;
            Protocol.Relay relay = r;
            yield return HoldE(g, relay.spot, 0, () => relay.lit, 6);
        }
        int lit = g.protocol.HudLit;
        Check(lit == 4 && seen.Contains("protocolSurge"), $"the four relays lit ({lit} / {g.protocol.HudTotal}): events [{string.Join(", ", seen.FindAll(s => s.StartsWith("relay") || s.StartsWith("protocol")))}], the surge {g.protocol.Surge}, queued squads {g.director.Queue.Count}");
        Vec3 ring = g.objectives.Spot ?? g.map.holdSpots[0];
        g.Respawn(ring, 0);
        double grantsBefore = g.economy.total;
        bool shotRing = false, shotRelay = false;
        t0 = Time.realtimeSinceStartup;
        while (!(g.objectives.Done && g.protocol.Complete) && Time.realtimeSinceStartup - t0 < 150)
        {
            g.health.hp = g.health.max;
            // Stand in the ring (respawned only if moved out: Respawn zeroes the loop's accumulator, so a respawn every
            // frame starves the sim of steps at a high frame rate; NIGHT11 Play check 2 held 338 of 3600 steps that way).
            Vec3 feet = g.Feet;
            if (JsMath.Hypot(feet.x - ring.x, feet.z - ring.z) > 2) g.Respawn(ring, 0);
            if (!shotRing && g.objectives.Held > 120)
            {
                g.input.SetLook(0, -0.55);
                shotRing = true;
                yield return null;
                Capture("n11-hold-ring.png");
            }
            else if (shotRing && !shotRelay && g.objectives.Held > 360)
            {
                // the nearest lit relay's beam from the ring
                Vec3 to = g.protocol.relays[0].spot - g.Eye;
                g.input.SetLook(System.Math.Atan2(-to.x, -to.z), System.Math.Atan2(to.y + 6, System.Math.Sqrt(to.x * to.x + to.z * to.z)));
                shotRelay = true;
                yield return null;
                Capture("n11-relay-lit.png");
            }
            KillAura(g);
            yield return null;
        }
        bool forged = true;
        foreach (string id in g.loadout.owned) if (g.forge.Known(id) && !g.forge.IsForged(id)) forged = false;
        Check(g.objectives.Done && seen.Exists(s => s.StartsWith("objectiveDone")), $"the HOLD ring held at ({ring.x}, {ring.z}): {g.objectives.Held} / {g.objectives.Need} steps; {seen.Find(s => s.StartsWith("objectiveDone"))}");
        Check(g.protocol.Complete && seen.Contains("protocolComplete") && forged, $"THE PROTOCOL complete in {Time.realtimeSinceStartup - t0:0} s: every owned gun forged {forged} ([{string.Join(", ", g.forge.order)}]), points {grantsBefore:0} -> {g.economy.total:0}; round {g.run.round} {g.run.state}");
        g.husks.frozen = false;

        // 10. Exfil at round 11.
        if (g.exfil.enabled)
        {
            g.run.DebugSkipTo(11);
            t0 = Time.realtimeSinceStartup;
            while (!(g.run.round == 11 && g.run.state == SurvivalRun.State.Break) && Time.realtimeSinceStartup - t0 < 5) yield return null;
            BoxSpot rs = g.exfil.radio;
            bool canCall = g.exfil.CanCall();
            yield return HoldE(g, new Vec3(rs.pos.x + System.Math.Sin(rs.yaw) * 1.2, rs.pos.y, rs.pos.z + System.Math.Cos(rs.yaw) * 1.2), rs.yaw, () => g.exfil.phase == ExfilPhase.Called);
            Check(canCall && g.exfil.phase == ExfilPhase.Called && seen.Contains("exfilCall"), $"the exfil radio at the spawn called in round {g.run.round}'s break (could {canCall}): {g.exfil.phase}");
            t0 = Time.realtimeSinceStartup;
            while (g.exfil.phase != ExfilPhase.Reach && Time.realtimeSinceStartup - t0 < 20)
            {
                g.health.hp = g.health.max;
                yield return null;
            }
            Vec3 site = g.exfil.SitePos;
            g.husks.frozen = true;
            t0 = Time.realtimeSinceStartup;
            bool placed = false;
            while (g.flow == Flow.Playing && g.exfil.Outcome == null && Time.realtimeSinceStartup - t0 < 120)
            {
                g.health.hp = g.health.max;
                if (g.exfil.phase == ExfilPhase.Reach)
                {
                    if (!placed) g.Respawn(site, 0); // once (Respawn zeroes the loop's accumulator)
                    placed = true;
                }
                else KillAura(g);
                yield return null;
            }
            g.husks.frozen = false;
            t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Results && Time.realtimeSinceStartup - t0 < 5) yield return null;
            yield return null;
            Capture("n11-results-exfil.png");
            RunStats st = g.lastStats;
            Check(g.flow == Flow.Results && st != null && st.exfil == Exfil.Win && seen.Contains("exfilDone") && st.grade != null && st.line != null,
                $"EXFIL at round {st?.round} (site {g.exfil.site} {g.exfil.SiteZone}, {g.exfil.kills} / {g.exfil.needed} kills, {g.exfil.topUpSquads} squads topped up): {g.flow}, '{st?.exfil}', grade {st?.grade}, the PROTOCOL {st?.protocolLit} / {st?.protocolTotal} complete {st?.protocolComplete}; line '{st?.line}'");
        }
        else Line("exfil off (design.exfil.enabled false): no radio");
        for (int i = 0; i < names.Length; i++) g.events.Off(names[i], handlers[i]);
    }

    /// <summary>
    /// A husk on open floor in clear sight of `eye`, `dirX` (+1 / -1) along x from it, 5-12 m out: each candidate's floor is
    /// found by a ray down (y within 0.05 of the eye's floor) and its line from the eye must hit no wall first (the memory
    /// note play-check-eval-recipe: place actors by casting the collider).
    /// </summary>
    static Husk PlaceTarget(SurvivalGame g, Vec3 eye, double dirX)
    {
        double floorY = eye.y - g.cfg.Num("body.eyeHeight");
        foreach (double d in new[] { 8.0, 6.0, 10.0, 5.0, 12.0 })
        {
            foreach (double dz in new[] { 0.0, 1.5, -1.5, 3.0, -3.0 })
            {
                double x = eye.x + dirX * d, z = eye.z + dz;
                if (!g.world.RayIntersect(new Vec3(x, floorY + 3, z), new Vec3(0, -1, 0), out RayHit down) || System.Math.Abs(floorY + 3 - down.distance - floorY) > 0.05) continue;
                Vec3 aim = new Vec3(x, floorY + 1, z);
                Vec3 to = aim - eye;
                double len = to.Length;
                if (g.world.RayIntersect(eye, to / len, out RayHit wall) && wall.distance < len) continue;
                Husk h = g.husks.Spawn(new Vec3(x, floorY, z), eye);
                if (h != null) return h;
            }
        }
        return null;
    }

    void Aim(SurvivalGame g)
    {
        Enemy best = null;
        double bestD = double.PositiveInfinity;
        Vec3 eye = g.Eye;
        foreach (Enemy e in g.enemies.all)
        {
            if (!e.alive) continue;
            double d = Vec3.DistanceTo(e.AimPoint, eye);
            if (d < bestD)
            {
                bestD = d;
                best = e;
            }
        }
        if (best == null)
        {
            g.forcedCombat = new CombatInput { weapon = -1 };
            return;
        }
        Vec3 to = (best is Husk h && h.HasHead ? h.HeadCenter : best.AimPoint) - eye;
        double yaw = System.Math.Atan2(-to.x, -to.z);
        double pitch = System.Math.Atan2(to.y, System.Math.Sqrt(to.x * to.x + to.z * to.z));
        g.input.SetLook(yaw, pitch);
        g.forcedCombat = new CombatInput { weapon = 0, fire = new Skygrind.Core.Button { held = true, pressed = true } };
    }

    string profileSnap;

    void Finish()
    {
        SurvivalGame sg = SurvivalGame.Instance;
        if (sg != null)
        {
            sg.noRecords = sg.noLab = false;
            if (profileSnap != null) sg.profile.CopyFrom(Profile.Read(profileSnap)); // NIGHT12: the profile as it was
            sg.sim.records = sg.profile.records; // NIGHT13: CopyFrom replaces the records object; the sim's field follows it (it went stale before)
            sg.profileReadOnly = false;
            sg.setup.mode = RunSetup.Survival;
            sg.setup.heat = 0;
        }
        N55End(); // NIGHT55 S: the input summary, backgroundBehavior restored (before DONE: DONE stays the last line)
        Line(failures == 0 ? "DONE: all steps passed" : $"DONE: {failures} step(s) failed");
        Done = true;
        Destroy(gameObject);
    }

    void Update()
    {
        if (!Done && Time.realtimeSinceStartup - started > 600)
        {
            Line("FAIL the 10 minute cap");
            failures++;
            StopAllCoroutines();
            Finish();
        }
    }
}
}
