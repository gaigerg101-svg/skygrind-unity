// NIGHT31 builder W: the Player scenarios of the W rows (the names are FeelReel.N27.cs PhaseN31W; the body is the partial N31WeaponsBody). Each writes PASS
// or RED into res with the numbers and the frame numbers to look at. Inputs: a VIRTUAL PAD (FeelReel.N27.Weapons.cs Pad27: RT / LT as analog values
// through PadReader, the path a real pad takes), never g.forcedCombat for the triggers. Not run by its author (no Player in the worktree).
//   charge31   SURVIVAL. design.n31.charge. Per gun (KESTREL, THUNDERCLAP) on a bag husk: a TAP fires; an EARLY release (hold + 0.2 s of charge) cancels with
//              nothing spent (no cooldown); a FULL hold then release fires ONE charged shot (the slot ring CHARGING, the reticle arc filling; frames printed),
//              then the cooldown (4 s / 6 s) shows COOLING; after the cooldown a HELD trigger (3 s, never released: the smoke's virtual pad, P0 a) fires the
//              charged shot by itself (auto release) and keeps shooting. The melee cancel (R8) is the harness's (`charge31`): the pad's melee is not sent here.
//   lt31       SURVIVAL, then THE LANTERN (round 3: the boss parry, LT on the pad the frame an orb is within orb.reflectRange). design.n31.ltAlways. LT on each weapon: KESTREL and THUNDERCLAP zoom (ZoomMag); the NAILGUN zooms with no nail stuck, then after 1 s of
//              nails into a bag its label reads DETONATE and LT sets them off; the gauntlet's LT (pips not full) opens a PARRY, a second press within 1 s
//              reads COOLING. Each press prints the slot word and whether something happened.
//   unlocks31  a CAMPAIGN level (boot -skygrindMap n30-foundry / n30-iceline / n30-kiln / n30-span, one Player run each). design.n31.unlocks. The owned weapons
//              at room 1 (after 1 s) and after the level's award (iceline: on entering i1a; kiln / span: the player put on the plan's weapon pickup), with the
//              expected sets; the hotbar's slots (Ui.Weapon30Ids) must list only owned weapons.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N31WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "charge31") body = Charge31Scenario(g, dir, res, offs, pre);
            else if (s == "lt31") body = Lt31Scenario(g, dir, res, offs, pre);
            else if (s == "unlocks31") body = Unlocks31Scenario(g, dir, res, offs, pre);
        }

        // ======================================================= charge31 =======================================================

        static IEnumerator Charge31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | n31.charge {g.sim.n31.charge}, n27.chargeShots27 {g.sim.n27.chargeShots27}";
            ChargeShots27 c = g.loadout.charge27;
            if (!g.sim.n31.charge || c == null || !c.active) { res[0] = "RED: design.n31.charge is off in this run (RT held never charges) | " + pc; yield break; }
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            foreach (string w in new[] { "revolver", "shotgun" }) if (g.loadout.owned.IndexOf(w) < 0) g.loadout.AddWeapon(w);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.frozen = true;
            Start(g, "charge31", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 170;
            var fails = new List<string>();
            var lines = new List<string>();
            string curGun = "";
            int fires = 0;
            offs.Add(g.events.On<string>("fire", w => { if (w == curGun) fires++; }));
            foreach (string gun in new[] { "revolver", "shotgun" })
            {
                int gi = ChargeShots27.Index(gun);
                string name = gun == "revolver" ? "KESTREL" : "THUNDERCLAP";
                curGun = gun;
                g.husks.KillAll();
                yield return Equip27(g, gun);
                Husk bag = PlaceAt(g, gun == "revolver" ? 4.0 : 3.0, 0, out _);
                if (bag == null) { fails.Add($"{name}: no bag husk placed"); continue; }
                bag.hp = bag.maxHp = 1e12;
                Action each = () => { God(g); if (bag.alive) Look(g, bag.AimPoint); };
                double holdS = c.p.holdSeconds, fullS = c.c35 != null && c.c35.c40 ? c.FullSteps(gi) * g.cfg.Dt : c.p.fullSeconds[gi] /* NIGHT40 W1: charge40's FULL while live */, cdS = c.p.cooldownSeconds[gi], autoS = c.p.autoReleaseSeconds;
                // a: a tap
                int f0 = fires;
                pad.Send(0f, 1f);
                yield return Wait27(g, 0.1, wallEnd, each);
                pad.Send(0f, 0f);
                yield return Wait27(g, 0.5, wallEnd, each);
                bool tapOk = fires > f0;
                // b: an early release: nothing spent
                int fired0 = c.fired[gi], canc0 = c.cancelled;
                pad.Send(0f, 1f);
                yield return Wait27(g, holdS + 0.2, wallEnd, each);
                pad.Send(0f, 0f);
                yield return Wait27(g, 0.3, wallEnd, each);
                bool earlyOk = c.fired[gi] == fired0 && c.cancelled == canc0 + 1 && c.cooldown[gi] == 0;
                // c: a full hold, then release: one charged shot; the ring / arc frames
                double arcPeak = 0;
                int frameCharging = -1, frameFire = -1;
                string wordCharging = "";
                pad.Send(0f, 1f);
                yield return Wait27(g, holdS + fullS + 0.1, wallEnd, () =>
                {
                    each();
                    if (Ui.Charge27ArcFillNow > arcPeak) { arcPeak = Ui.Charge27ArcFillNow; frameCharging = FeelProbe.FrameCount; wordCharging = Slot30Word(gun); }
                });
                pad.Send(0f, 0f);
                frameFire = FeelProbe.FrameCount;
                yield return Wait27(g, 0.5, wallEnd, each);
                int charged = c.fired[gi] - fired0;
                double cdLeft = c.cooldown[gi] * g.sim.step;
                string wordCooling = Slot30Word(gun);
                int frameCooling = FeelProbe.FrameCount;
                bool fullOk = charged == 1 && cdLeft > cdS - 1.0 && cdLeft <= cdS;
                // d: the cooldown out, then RT HELD 3 s and never released (P0 a): the charge fires itself and the gun keeps shooting
                yield return Wait27(g, cdLeft + 0.3, wallEnd, each);
                int auto0 = c.autoReleased, fd0 = fires, fired1 = c.fired[gi];
                pad.Send(0f, 1f);
                yield return Wait27(g, 3.0, wallEnd, each);
                pad.Send(0f, 0f);
                yield return Wait27(g, 0.3, wallEnd, each);
                int autoN = c.autoReleased - auto0, heldShots = fires - fd0;
                bool heldOk = autoN == 1 && c.fired[gi] - fired1 == 1 && heldShots >= 2;
                if (!tapOk) fails.Add($"{name}: a tap fired nothing");
                if (!earlyOk) fails.Add($"{name}: the early release spent something");
                if (!fullOk) fails.Add($"{name}: full hold + release: {charged} charged, cooldown {cdLeft.ToString("0.0", ci)} s");
                if (!heldOk) fails.Add($"{name}: a held trigger: auto released {autoN}, shots {heldShots}");
                lines.Add($"{name}: tap {(tapOk ? "fired" : "NOTHING")}; early release ({holdS}+0.2 s) charged 0 cancel +1 cooldown 0: {(earlyOk ? "ok" : "MISS")}; full ({holdS}+{fullS} s) then release: {charged} charged, arc peak {arcPeak.ToString("0.00", ci)} at frame {frameCharging} (slot word '{wordCharging}'), released at frame {frameFire}, cooldown {cdLeft.ToString("0.0", ci)} s left (config {cdS}), slot word '{wordCooling}' at frame {frameCooling}; HELD 3 s never released: {autoN} auto released after {autoS} s at full, {heldShots} shots");
                pad.Send(0f, 0f);
                yield return Frames(Ms(200));
            }
            yield return Stop();
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, virtual pad: " + string.Join(" || ", lines) + " || the melee cancel: harness charge31 only";
        }

        // ======================================================= lt31 =======================================================

        static IEnumerator Lt31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + $" | n31.ltAlways {g.sim.n31.ltAlways}, pulse27 {g.loadout.Pulse27Live}, zoom {g.loadout.ZoomOn}";
            LtAlways31 l = g.loadout.lt31;
            if (!g.sim.n31.ltAlways || l == null || !l.active) { res[0] = "RED: design.n31.ltAlways is off in this run (NIGHT30's LT) | " + pc; yield break; }
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            foreach (string w in new[] { "revolver", "shotgun", "nailgun", "gauntlet" }) if (g.loadout.owned.IndexOf(w) < 0) g.loadout.AddWeapon(w);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.frozen = true;
            Start(g, "lt31", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 120;
            var fails = new List<string>();
            var lines = new List<string>();
            foreach (string gun in new[] { "revolver", "shotgun", "nailgun", "gauntlet" })
            {
                g.husks.KillAll();
                yield return Equip27(g, gun);
                Husk bag = PlaceAt(g, 5.0, 0, out _);
                if (bag != null) bag.hp = bag.maxHp = 1e12;
                Action each = () => { God(g); if (bag != null && bag.alive) Look(g, bag.AimPoint); };
                yield return Wait27(g, 0.3, wallEnd, each);
                string label = g.loadout.LtLabel31(gun), word0 = Slot30Word(gun);
                // LT pressed and held 0.5 s
                int parry0 = l.parriesOpened, deny0 = l.denies, det0 = g.loadout.detonated;
                pad.Send(1f, 0f);
                double mag = 1;
                yield return Wait27(g, 0.5, wallEnd, () => { each(); mag = Math.Max(mag, g.loadout.ZoomMag); });
                int frameLt = FeelProbe.FrameCount;
                pad.Send(0f, 0f);
                yield return Wait27(g, 0.2, wallEnd, each);
                string line = $"{gun}: label '{label}' (slot word '{word0}'), LT 0.5 s at frame {frameLt}: zoom x{mag.ToString("0.00", ci)}";
                bool ok = gun == "gauntlet" ? (l.parriesOpened > parry0 || g.loadout.pulse27 != null && g.loadout.pulse27.cooldownSteps > 0) : mag > 1.01;
                if (gun == "gauntlet")
                {
                    line += $", parries opened +{l.parriesOpened - parry0}, word '{l.word}'";
                    pad.Send(1f, 0f); // a second press inside the 1.0 s cooldown: COOLING
                    yield return Wait27(g, 0.1, wallEnd, each);
                    pad.Send(0f, 0f);
                    yield return Wait27(g, 0.1, wallEnd, each);
                    bool cooling = l.denies > deny0;
                    line += $"; a second press within the cooldown: deny +{l.denies - deny0} word '{l.word}', slot word '{Slot30Word("gauntlet")}' at frame {FeelProbe.FrameCount}";
                    ok &= cooling;
                }
                if (gun == "nailgun")
                {
                    // nails into the bag for 1 s, then LT = DETONATE
                    pad.Send(0f, 1f);
                    yield return Wait27(g, 1.0, wallEnd, each);
                    pad.Send(0f, 0f);
                    yield return Wait27(g, 0.3, wallEnd, each);
                    string label2 = g.loadout.LtLabel31(gun), word2 = Slot30Word(gun);
                    int frameDet = FeelProbe.FrameCount;
                    pad.Send(1f, 0f);
                    yield return Wait27(g, 0.15, wallEnd, each);
                    pad.Send(0f, 0f);
                    yield return Wait27(g, 0.3, wallEnd, each);
                    int det = g.loadout.detonated - det0;
                    line += $"; after 1 s of nails: label '{label2}' (slot word '{word2}' at frame {frameDet}), LT detonated {det} nail(s)";
                    ok &= label2 == "DETONATE" && det > 0;
                }
                if (!ok) fails.Add(gun);
                lines.Add(line);
            }
            // the BOSS parry (round 3): THE LANTERN's room (BossRoomBegin "hulk", the lantern27 scenario's route), the gauntlet in hand, the look on the
            // Lantern; the pad's LT is pressed the frame a Lantern orb comes within the parry's reach (orb.reflectRange) of the eye; up to 30 s of game time
            string bossLine;
            SurvivalSim sim = g.sim;
            if (!sim.Lantern27On) bossLine = "boss parry: NOT RUN (design.n27.boss1 off: no Lantern)";
            else if (!sim.BossRoomBegin("hulk", out Vec3 bsp, out double byaw) || sim.lantern27 == null || !sim.lantern27.Alive) bossLine = "boss parry: NOT RUN (BossRoomBegin(hulk) did not start the Lantern)";
            else
            {
                Lantern27 lan = sim.lantern27;
                g.husks.KillAll();
                g.Respawn(bsp, byaw);
                yield return Equip27(g, "gauntlet");
                double reach = g.cfg.Has("orb.reflectRange") ? g.cfg.Num("orb.reflectRange") : 2;
                int ret0 = l.bossReturns, open0 = l.parriesOpened, presses = 0, frameRet = -1;
                double hp0 = lan.body.hp, closest = double.PositiveInfinity;
                double bossEnd = Time.realtimeSinceStartup + 60;
                int s0 = sim.simStep;
                while ((sim.simStep - s0) * sim.step < 30 && Time.realtimeSinceStartup < bossEnd && l.bossReturns == ret0 && lan.Alive)
                {
                    God(g);
                    Look(g, lan.body.AimPoint);
                    Vec3 eye = g.Eye;
                    double near = double.PositiveInfinity;
                    foreach (Lantern27Orb o in lan.orbs) near = Math.Min(near, Vec3.DistanceTo(o.pos, eye));
                    closest = Math.Min(closest, near);
                    bool press = near <= reach && l.parryCooldown == 0 && l.parrySteps == 0;
                    pad.Send(press ? 1f : 0f, 0f);
                    if (press) presses++;
                    yield return null;
                    if (l.bossReturns > ret0 && frameRet < 0) frameRet = FeelProbe.FrameCount;
                }
                pad.Send(0f, 0f);
                int streakMax = 0, streakFrame = -1;
                for (int f = 0; f < 30; f++)
                {
                    if (Parry31View.StreaksShown > streakMax) { streakMax = Parry31View.StreaksShown; streakFrame = FeelProbe.FrameCount; }
                    yield return null;
                }
                int ret = l.bossReturns - ret0;
                bossLine = $"boss parry on THE LANTERN: {(ret > 0 ? "RETURNED" : "NOT returned")} {ret} orb(s) at frame {frameRet}, LT pressed on {presses} frame(s) with an orb within {reach} m (parries opened +{l.parriesOpened - open0}), the closest orb {(double.IsInfinity(closest) ? "none" : closest.ToString("0.0", ci) + " m")}, boss hp {hp0.ToString("0.0", ci)} -> {lan.body.hp.ToString("0.0", ci)} (sent back {sim.bossParryDamage31.ToString("0.0", ci)} in total); the return streak (Parry31View, N31Fixes.On) drawn: {streakMax} at frame {streakFrame}";
                if (ret == 0) fails.Add("the boss parry");
            }
            lines.Add(bossLine);
            yield return Stop();
            res[0] = (fails.Count == 0 ? "PASS" : "RED: LT did nothing on " + string.Join(", ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, virtual pad LT: " + string.Join(" || ", lines);
        }

        // ======================================================= unlocks31 =======================================================

        static IEnumerator Unlocks31Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            yield return StartCampaign30(g, offs);
            SurvivalSim sim = g.sim;
            string id = sim.campaignLevel?.id ?? "-";
            string pc = Preconditions(g, g.Feet) + $" | level {id}, n31.unlocks {sim.n31.unlocks}";
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN | " + pc; yield break; }
            if (!sim.n31.unlocks) { res[0] = "RED: design.n31.unlocks is off in this run (NIGHT30's KESTREL + gauntlet start) | " + pc; yield break; }
            var want = new Dictionary<string, (string[] start, string award)>
            {
                ["n30-foundry"] = (new[] { "revolver" }, null),
                ["n30-iceline"] = (new[] { "revolver" }, "gauntlet"),
                ["n30-kiln"] = (new[] { "revolver", "gauntlet" }, "shotgun"),
                ["n30-span"] = (new[] { "revolver", "gauntlet", "shotgun" }, "nailgun"),
                ["n32-foundry"] = (new[] { "revolver" }, null), // NIGHT32: the swapped levels (design.n32.arenas32 / span32) and LEVEL 5 (level5)
                ["n32-iceline"] = (new[] { "revolver" }, "gauntlet"),
                ["n32-kiln"] = (new[] { "revolver", "gauntlet" }, "shotgun"),
                ["n32-span"] = (new[] { "revolver", "gauntlet", "shotgun" }, "nailgun"),
                ["n32-spire"] = (new[] { "revolver", "gauntlet", "shotgun", "nailgun" }, (string)null) /* the TORCH comes with the level clear, not in room 1 */,
            };
            var earned = new List<string>();
            offs.Add(g.events.On<WeaponEarnedEvent>("weaponEarned", e => earned.Add(e.weapon)));
            Start(g, "unlocks31", "ui", dir);
            yield return Frames(pre);
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 1.0) { God(g); yield return null; }
            string room1 = sim.campaign != null && sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "-";
            var atRoom1 = new List<string>(g.loadout.owned);
            var hud1 = new List<string>(Ui.Weapon30Ids);
            int frame1 = FeelProbe.FrameCount;
            var start = new List<string>(sim.unlocks31Start);
            string how = "-";
            want.TryGetValue(id, out var w);
            if (w.award != null && !g.loadout.owned.Contains(w.award))
            {
                PickupDef pk = sim.campaignLevel.pickups.Find(p => p.kind == "weaponPickup");
                if (pk != null)
                {
                    how = $"pickup {pk.id} in {pk.room}";
                    g.Respawn(pk.at, 0);
                    double t1 = sim.SimTime;
                    while (sim.SimTime - t1 < 2.0 && !g.loadout.owned.Contains(w.award)) { God(g); yield return null; }
                }
                else how = "no weapon pickup in the plan";
            }
            else if (w.award != null) how = $"on entering {room1}";
            yield return Frames(20);
            var after = new List<string>(g.loadout.owned);
            var hud2 = new List<string>(Ui.Weapon30Ids);
            int frame2 = FeelProbe.FrameCount;
            yield return Stop();
            bool known = w.start != null;
            bool startOk = known && start.Count == w.start.Length && AllIn31(w.start, start);
            var wantAfter = known ? new List<string>(w.start) : new List<string>();
            if (w.award != null) wantAfter.Add(w.award);
            bool afterOk = known && after.Count == wantAfter.Count && wantAfter.TrueForAll(after.Contains);
            bool hudOk = true;
            foreach (string h in hud2) if (h != "punch" && h != "blast" && !after.Contains(h)) hudOk = false;
            bool ok = startOk && afterOk && hudOk;
            res[0] = $"{(ok ? "PASS" : "RED")}: {id}: start set [{string.Join(", ", start)}] (want [{(known ? string.Join(", ", w.start) : "level not in the table")}]); room 1 {room1} owned [{string.Join(", ", atRoom1)}], hotbar [{string.Join(", ", hud1)}] at frame {frame1}; award {(w.award ?? "none")} ({how}): owned [{string.Join(", ", after)}] (want [{string.Join(", ", wantAfter)}]), hotbar [{string.Join(", ", hud2)}] at frame {frame2}{(hudOk ? "" : " (the hotbar lists a weapon not owned)")}; weaponEarned [{string.Join(", ", earned)}] | {pc}";
        }

        static bool AllIn31(string[] want, List<string> have)
        {
            foreach (string s in want) if (!have.Contains(s)) return false;
            return true;
        }
    }
}
