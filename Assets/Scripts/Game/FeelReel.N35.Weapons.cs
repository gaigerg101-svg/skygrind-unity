// NIGHT35 builder W: the Player scenarios of the W rows (FeelReel.N27.cs PhaseN35W -> N35WeaponsBody). On the player's own input path (a virtual pad's RT,
// PadReader reads it as a player's; L-031), whole Player frames (L-039). One Player per scenario and map (L-046). Not run by its author (no Player in the
// worktree).
//   charge35  boot on the campaign's LEVEL 1 (the KESTREL) or LEVEL 3 (THE THUNDERCLAP): the level ids are Curriculum25.Order's [0] and [2] (the line
//             prints the order it read, so the lead boots `-skygrindMap <that id>`; A may swap the files). Any other map: the KESTREL, and the line says so.
//             A safe spot, husks killed and frozen, god mode; one bag husk (1e9 hp) 6 m ahead, aimed at.
//     TAP: RT down 0.05 s, up: the normal shots and charge35Start in the next 0.4 s (want 1 shot, 0 starts).
//     HOLD: RT down until the full charge (n31's fullSeconds from the press) + 0.25 s, then up. Measured: the gun's "fire" events while held (want 0:
//       no shots first), charge35Start / Full / Fire (1 / 1 / 1), the hands' drawn level and tip radius (Viewmodel.Charge35DrawnLevel / Charge35TipNow) and the
//       rail's text and fill (Ui.Charge35RailText / Charge35RailFill) at half and at full, then the rail's charge cooldown drain after the shot.
//     Shots: shot-N30-charge35-<gun>-half / -full / -fire / -cool.png (the gun's glow, the rail's CHARGING / CHARGED, the beam or slug, the drain).
//     GREEN (charge35 on): 0 shots while held, 1 / 1 / 1 events, the tip drawn at full, the rail read CHARGING then CHARGED, a tap = 1 shot.
//     RED (-skygrindOff35): NIGHT31's rule: normal shots while held (0.4 s of auto-fire), no charge35 events, no glow, no rail state.
//   pickup35  (round 2, below)
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
        static partial void N35WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "charge35") body = Charge35Scenario(g, dir, res, offs, pre);
            else if (s == "pickup35") body = Pickup35Scenario(g, dir, res, offs, pre);
        }

        static IEnumerator Charge35Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            List<string> order = Curriculum25.Order(g.cfg, sim.n25);
            int li = order.IndexOf(g.MapId);
            string gun = li == 2 ? "shotgun" : "revolver";
            string name = gun == "revolver" ? "KESTREL" : "THUNDERCLAP";
            string levelNote = li == 0 ? "campaign level 1" : li == 2 ? "campaign level 3" : $"NOT campaign level 1 / 3 (index {li})";
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId} = {levelNote}; order [{string.Join(", ", order)}] (boot level 1 = {(order.Count > 0 ? order[0] : "?")}, level 3 = {(order.Count > 2 ? order[2] : "?")}) | n35.charge35 {sim.n35.charge35}, n31.charge {sim.n31.charge}, N35Fixes.On {N35Fixes.On}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            bool hadGun = g.loadout.owned.IndexOf(gun) >= 0;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "charge35", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 90;
            yield return Equip27(g, gun);
            g.forcedCombat = null;
            Husk bag = PlaceAt(g, 6, 0, out double _);
            if (bag != null) bag.hp = bag.maxHp = 1e9;
            Action aim = () => { God(g); if (bag != null && bag.alive) Look(g, bag.AimPoint); };
            int fires = 0, starts = 0, fulls = 0, fires35 = 0;
            offs.Add(g.events.On<string>("fire", w => { if (w == gun) fires++; }));
            offs.Add(g.events.On<ChargeShot27Event>("charge35Start", e => starts++));
            offs.Add(g.events.On<ChargeShot27Event>("charge35Full", e => fulls++));
            offs.Add(g.events.On<ChargeShot27Event>("charge35Fire", e => fires35++));
            ChargeShots27 c = g.loadout.charge27;
            int gi = ChargeShots27.Index(gun);
            yield return Wait27(g, 0.6, wallEnd, aim);

            // ---- TAP ----
            int f0 = fires, st0 = starts;
            pad.Send(0f, 1f);
            yield return Wait27(g, 0.05, wallEnd, aim);
            pad.Send(0f, 0f);
            yield return Wait27(g, 0.4, wallEnd, aim);
            int tapShots = fires - f0, tapStarts = starts - st0;
            bool tapOk = tapShots == 1 && tapStarts == 0;
            string tapLine = $"TAP {(tapOk ? "PASS" : "RED")}: {tapShots} shot(s), charge35Start {tapStarts}";

            // ---- HOLD ----
            if (c != null && gi >= 0) yield return Wait27(g, c.cooldown[gi] * sim.step + 0.1, wallEnd, aim); // a charge that is cooling cannot start
            double fullS = c != null && gi >= 0 ? (c.c35 != null && c.c35.c40 ? c.FullSteps(gi) * g.cfg.Dt : c.p.fullSeconds[gi]) : 0.5; // NIGHT40 W1: charge40's FULL while live
            f0 = fires;
            st0 = starts;
            int fu0 = fulls, fi0 = fires35;
            pad.Send(0f, 1f);
            yield return Wait27(g, fullS * 0.5, wallEnd, aim);
            double halfLevel = Viewmodel.Charge35DrawnLevel;
            float halfTip = Viewmodel.Charge35TipNow;
            string halfRail = Ui.Charge35RailText;
            float halfFill = Ui.Charge35RailFill;
            int fHalf = FeelProbe.FrameCount;
            yield return Shot30(dir, "charge35-" + gun + "-half");
            yield return Wait27(g, fullS * 0.5 + 0.12, wallEnd, aim);
            double fullLevel = Viewmodel.Charge35DrawnLevel;
            float fullTip = Viewmodel.Charge35TipNow;
            string fullRail = Ui.Charge35RailText;
            float fullFill = Ui.Charge35RailFill;
            int fFull = FeelProbe.FrameCount;
            yield return Shot30(dir, "charge35-" + gun + "-full");
            yield return Wait27(g, 0.13, wallEnd, aim);
            int heldShots = fires - f0;
            pad.Send(0f, 0f);
            double tRel = sim.SimTime;
            while (fires35 == fi0 && sim.SimTime - tRel < 0.3 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
            int fFire = FeelProbe.FrameCount;
            yield return Frames(2);
            yield return Shot30(dir, "charge35-" + gun + "-fire");
            yield return Wait27(g, 0.5, wallEnd, aim);
            float cool = Ui.Charge35RailCool;
            yield return Shot30(dir, "charge35-" + gun + "-cool");
            bool holdOk = heldShots == 0 && starts - st0 == 1 && fulls - fu0 == 1 && fires35 - fi0 == 1;
            bool readOk = fullLevel >= 0.99 && fullTip > 0 && halfRail == UnityTuning.Charge35RailCharging && fullRail == UnityTuning.Charge35RailFull && cool > 0;
            string holdLine = $"HOLD {(holdOk ? "PASS" : "RED")}: shots while held {heldShots} (want 0), charge35 Start / Full / Fire {starts - st0} / {fulls - fu0} / {fires35 - fi0} (want 1 / 1 / 1)";
            string readLine = $"READ {(readOk ? "PASS" : "RED")}: hands level {halfLevel.ToString("0.00", ci)} -> {fullLevel.ToString("0.00", ci)}, tip {(halfTip * 1000).ToString("0.0", ci)} -> {(fullTip * 1000).ToString("0.0", ci)} mm; rail '{halfRail}' fill {halfFill.ToString("0.00", ci)} -> '{fullRail}' fill {fullFill.ToString("0.00", ci)}; the charge cooldown drain {cool.ToString("0.00", ci)} 0.5 s after | frames f{fHalf:0000} half, f{fFull:0000} full, f{fFire:0000} fire; shots shot-N30-charge35-{gun}-half / -full / -fire / -cool.png";
            g.husks.KillAll();
            yield return Frames(Ms(300));
            yield return Stop();
            bool ok = tapOk && holdOk && readOk && (li == 0 || li == 2);
            res[0] = $"{(ok ? "PASS" : "RED")} {name}{(hadGun ? "" : " (added by the reel: not owned at boot)")} || {tapLine} || {holdLine} || {readLine} | {pc}";
        }

        /// <summary>A floor spot `dist` m from `centre` (web space) with a clear eye line to it, on the pickup's own floor; false when none of 16 headings has one.</summary>
        static bool Spot35(SurvivalGame g, Vec3 centre, double dist, out Vec3 feet)
        {
            feet = default;
            if (!g.world.RayIntersect(new Vec3(centre.x, centre.y + 0.3, centre.z), new Vec3(0, -1, 0), out RayHit down) || down.distance > 3) return false;
            double floorY = centre.y + 0.3 - down.distance;
            foreach (double d in new[] { dist, dist * 0.75, dist * 1.4 })
                for (int i = 0; i < 16; i++)
                {
                    double a = i * Math.PI * 2 / 16, x = centre.x + Math.Sin(a) * d, z = centre.z + Math.Cos(a) * d;
                    if (!FloorAt(g, x, z, floorY)) continue;
                    var eye = new Vec3(x, floorY + 1.6, z);
                    Vec3 to = centre - eye;
                    double len = to.Length;
                    if (g.world.RayIntersect(eye, to / len, out RayHit wall) && wall.distance < len - 0.4) continue;
                    feet = new Vec3(x, floorY, z);
                    return true;
                }
            return false;
        }

        //   pickup35  boot on the campaign's LEVEL 3 (THE THUNDERCLAP's pickup k1-P2 and the MEND vial k3-P2 in NIGHT34's kiln; Curriculum25.Order[2]); god mode,
        //             husks killed and frozen. Per pickup (weapon first, then health): the player stood 3.2 m from it on its floor, looking at it, 0.6 s; then 12 m
        //             away (the name must be gone). Lines: kind, weapon, locked at boot (its room ahead of the player's) and when stood by it, the name label's text and
        //             opacity near / far. Shots: shot-N30-pickup35-<id>-near / -far.png. A wall buy (SURVIVAL maps only: campaign levels have none): the first wall
        //             gun from 3 m, at the run's points (want LOCKED: the silhouette) and with its price added (want LIT); shots pickup35-wall-locked / -lit.
        //             GREEN (pickup35 on): every pickup drawn by Pickup35View, near: the label shows (alpha > 0.5, "THUNDERCLAP" / "MEND +N"), far: hidden.
        //             RED (-skygrindOff35): no Pickup35View (NIGHT34's cubes): the frames are the before.
        //             Run it twice: -skygrindMap <level 3 id> (pickups) and the default survival map (the wall buy; its line says "no pickups" and is not RED for that).
        static IEnumerator Pickup35Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            List<string> order = Curriculum25.Order(g.cfg, sim.n25);
            bool row = N35Fixes.ViewRowOn(g.cfg, "pickup35");
            string pc = $"map {g.MapId} (order index {order.IndexOf(g.MapId)}; level 3 = {(order.Count > 2 ? order[2] : "?")}) | pickup35 {row}, N35Fixes.On {N35Fixes.On}, pickup views {Pickup35View.All.Count}, PropsView29 props {(PropsView29.Last != null ? PropsView29.Last.propCount : -1)}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            var lockedAtBoot = new Dictionary<Pickup35View, bool>();
            foreach (Pickup35View v in Pickup35View.All) lockedAtBoot[v] = v.LockedNow;
            Start(g, "pickup35", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 120;
            var lines = new List<string>();
            bool ok = true;
            var views = new List<Pickup35View>(Pickup35View.All);
            views.Sort((a, b) => a.Kind == b.Kind ? 0 : a.Kind == "weaponPickup" ? -1 : 1);
            foreach (Pickup35View v in views)
            {
                if (v == null) continue;
                Vec3 c = WebSpace.FromUnity(v.Centre);
                string id = v.gameObject.name.Replace("pickup35 ", "").Replace(' ', '-');
                if (!Spot35(g, c, 3.2, out Vec3 near)) { lines.Add($"{id} RED: no floor spot with a clear line within 4.5 m"); ok = false; continue; }
                g.Respawn(near, 0);
                Action look = () => { God(g); Look(g, c); };
                yield return Wait27(g, 0.6, wallEnd, look);
                float aNear = v.LabelAlpha;
                string tNear = v.LabelText;
                bool lockNear = v.LockedNow;
                int fNear = FeelProbe.FrameCount;
                yield return Shot30(dir, "pickup35-" + id + "-near");
                float pxNear = v.LabelPixelHeight(g.cam), px12 = -1, a12 = -1;
                if (Spot35(g, c, 12, out Vec3 at12))
                {
                    g.Respawn(at12, 0);
                    yield return Wait27(g, 0.6, wallEnd, look);
                    a12 = v.LabelAlpha;
                    px12 = v.LabelPixelHeight(g.cam);
                    yield return Shot30(dir, "pickup35-" + id + "-12m");
                }
                float aFar = -1;
                int fFar = -1;
                if (Spot35(g, c, 20, out Vec3 far))
                {
                    g.Respawn(far, 0);
                    yield return Wait27(g, 0.6, wallEnd, look);
                    aFar = v.LabelAlpha;
                    fFar = FeelProbe.FrameCount;
                    yield return Shot30(dir, "pickup35-" + id + "-far");
                }
                bool want = v.Kind == "healthPickup" ? tNear.StartsWith("MEND +") : tNear.Length > 0;
                bool pOk = (lockNear || (aNear > 0.5f && want && (px12 < 0 || px12 >= 16f))) && aFar <= 0.01f;
                ok &= pOk;
                lockedAtBoot.TryGetValue(v, out bool lb);
                lines.Add($"{id} {(pOk ? "PASS" : "RED")}: {v.Kind} {v.WeaponId ?? "-"}; locked at boot {lb}, by it {lockNear}; label near '{tNear}' alpha {aNear.ToString("0.00", ci)} {pxNear.ToString("0", ci)} px, at about 12 m alpha {a12.ToString("0.00", ci)} {px12.ToString("0", ci)} px (want >= 16), far (past 14 m) alpha {aFar.ToString("0.00", ci)} | frames f{fNear:0000} near, f{fFar:0000} far");
            }
            if (views.Count == 0) { lines.Add(row ? "no pickups on this map (boot the level-3 id for them)" : "RED: no Pickup35View (the row off / -skygrindOff35: NIGHT34's cubes)"); if (!row) ok = false; }

            // ---- a wall buy ----
            WallBuys wb = WallBuys.Current;
            if (wb != null && wb.First35(out Vector3 wAt, out string wid))
            {
                Vec3 c = WebSpace.FromUnity(wAt);
                if (Spot35(g, c, 3.0, out Vec3 ws))
                {
                    g.Respawn(ws, 0);
                    Action look = () => { God(g); Look(g, c); };
                    double before = sim.economy.total;
                    sim.economy.total = 0;
                    yield return Wait27(g, 0.5, wallEnd, look);
                    int lockedN = WallBuys.Pickup35Locked;
                    yield return Shot30(dir, "pickup35-wall-locked");
                    sim.economy.total = 1e6;
                    yield return Wait27(g, 0.5, wallEnd, look);
                    int litLocked = WallBuys.Pickup35Locked;
                    yield return Shot30(dir, "pickup35-wall-lit");
                    sim.economy.total = before;
                    bool wOk = !row || (lockedN > 0 && litLocked == 0);
                    ok &= wOk;
                    lines.Add($"WALL {(wOk ? "PASS" : "RED")}: {wid} from 3 m: at 0 points {lockedN} locked silhouette(s), at 1e6 points {litLocked} | {wb.Describe35()}");
                }
                else lines.Add($"WALL RED: no spot 3 m from {wid}");
            }
            else lines.Add("WALL: no wall buy on this map (run on the default survival map for it)");
            yield return Frames(Ms(300));
            yield return Stop();
            res[0] = $"{(ok && row ? "PASS" : "RED")} || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
