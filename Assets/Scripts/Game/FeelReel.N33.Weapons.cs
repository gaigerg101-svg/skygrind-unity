// NIGHT33 builder W: the Player scenario of the W rows (the name is FeelReel.N27.cs PhaseN33W; the body is the partial N33WeaponsBody). On the player's own
// input path (a virtual pad's LT / RT, PadReader reads them as a player's; L-031), triggers held for whole Player frames (L-039). Not run by its author (no
// Player in the worktree).
//   feel33  any map, SURVIVAL, a safe spot, husks frozen, invulnerable.
//     ZOOM (view33): the KESTREL in hand; 1 s at rest, then LT held 1 s. Lines: the gun's root (Viewmodel camera/Viewmodel) at rest vs zoomed: its local
//       x / y / z (m), its roll (deg), its on-screen x through the viewmodel camera (px from the centre), the world FOV. Shots: feel33-zoom-rest, feel33-zoom-on.
//       PASS (view33 on): |dx| < 1 mm, roll change < 0.5 deg, the gun lowered 4-6 cm, the world FOV narrower. Off (-skygrindOff33 or the row off): the web's
//       ADS pose (the gun slides ~330 px toward the centre and rolls 4-5 deg): RED with the numbers (the before).
//     PIERCER (pierce33): five husks in a line 5-13 m ahead, aimed at the far one; RT held for the hold + the full charge + 0.15 s, then released (ONE
//       charged shot). Lines: the kestrelPierce33 events and their hits, bodies hurt, kills. Shot: feel33-pierce-beam (two frames after the beam event: the
//       LONGWAVE beam from the KESTREL's muzzle to the wall). PASS (pierce33 on): one beam, 5 bodies hit. Off: n31's charged shot (3 bodies, no beam): RED.
//     WHIP (whip33 + the view33 rope): see Whip33Part below (a tap at a frozen charger 10 m out: the stop, the gap, the rope frames).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N33WeaponsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "feel33") body = Feel33Scenario(g, dir, res, offs, pre);
        }

        struct Gun33Pose
        {
            public Vector3 local;
            public float roll, screenX, fov;
            public bool found;
        }

        static Gun33Pose GunPose33(SurvivalGame g)
        {
            var p = new Gun33Pose { fov = g.cam != null ? g.cam.fieldOfView : 0 };
            Transform vm = g.cam != null ? g.cam.transform.Find("Viewmodel camera") : null;
            Transform root = vm != null ? vm.Find("Viewmodel") : null;
            Camera vmCam = vm != null ? vm.GetComponent<Camera>() : null;
            if (root == null || vmCam == null) return p;
            p.found = true;
            p.local = root.localPosition;
            float r = root.localEulerAngles.z;
            p.roll = r > 180 ? r - 360 : r;
            p.screenX = vmCam.WorldToScreenPoint(root.position).x - Screen.width * 0.5f;
            return p;
        }

        static IEnumerator Feel33Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            bool view33 = N33Fixes.ViewRowOn(g.cfg, "view33");
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | view33 {view33}, n33.pierce33 {sim.n33.pierce33}, n33.whip33 {sim.n33.whip33}, n31.charge {sim.n31.charge}";
            offs.Add(() => { g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            g.forcedCombat = null;
            if (g.loadout.owned.IndexOf("revolver") < 0) g.loadout.AddWeapon("revolver");
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "feel33", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 120;
            yield return Equip27(g, "revolver");

            // ---- ZOOM ----
            yield return Wait27(g, 1.0, wallEnd, () => God(g));
            Gun33Pose rest = GunPose33(g);
            int fRest = FeelProbe.FrameCount;
            yield return Shot30(dir, "feel33-zoom-rest");
            pad.Send(1f, 0f);
            yield return Wait27(g, 1.0, wallEnd, () => God(g));
            Gun33Pose zoom = GunPose33(g);
            int fZoom = FeelProbe.FrameCount;
            yield return Shot30(dir, "feel33-zoom-on");
            pad.Send(0f, 0f);
            yield return Wait27(g, 0.5, wallEnd, () => God(g));
            float dx = zoom.local.x - rest.local.x, dy = zoom.local.y - rest.local.y, dz = zoom.local.z - rest.local.z, droll = zoom.roll - rest.roll;
            bool zoomOk = view33 && rest.found && zoom.found && Mathf.Abs(dx) < 0.001f && Mathf.Abs(droll) < 0.5f && -dy >= 0.04f && -dy <= 0.06f && zoom.fov < rest.fov;
            string zoomLine = $"ZOOM {(zoomOk ? "PASS" : "RED")}: gun root rest ({rest.local.x.ToString("0.000", ci)}, {rest.local.y.ToString("0.000", ci)}, {rest.local.z.ToString("0.000", ci)}) roll {rest.roll.ToString("0.0", ci)} deg, screen x {rest.screenX.ToString("0", ci)} px, world FOV {rest.fov.ToString("0.0", ci)}; zoomed: d = ({dx.ToString("0.000", ci)}, {dy.ToString("0.000", ci)}, {dz.ToString("0.000", ci)}) m, roll {droll.ToString("+0.0;-0.0", ci)} deg, screen x {zoom.screenX.ToString("0", ci)} px ({(zoom.screenX - rest.screenX).ToString("+0;-0", ci)} px), world FOV {zoom.fov.ToString("0.0", ci)} | frames f{fRest:0000} rest, f{fZoom:0000} zoomed; shots shot-N30-feel33-zoom-rest.png / -zoom-on.png";

            // ---- PIERCER ----
            g.husks.KillAll();
            yield return Frames(2);
            var line = new List<Husk>();
            Husk first = PlaceAt(g, 5, 0, out double yawUsed);
            if (first != null)
            {
                line.Add(first);
                Vec3 eye = g.Eye;
                double fx = -Math.Sin(yawUsed), fz = -Math.Cos(yawUsed), floorY = g.Feet.y;
                foreach (double d in new[] { 7.0, 9.0, 11.0, 13.0 })
                {
                    double x = eye.x + fx * d, z = eye.z + fz * d;
                    if (!FloorAt(g, x, z, floorY)) continue;
                    Husk h = g.husks.Spawn(new Vec3(x, floorY, z), eye);
                    if (h != null) line.Add(h);
                }
            }
            foreach (Husk h in line) h.hp = h.maxHp = 1e9; // bags: every body on the line stays to be hit
            int beams = 0, beamHits = 0, fBeam = -1;
            var hurt = new HashSet<int>();
            bool afterRelease = false;
            offs.Add(g.events.On<RailFireEvent>(Loadout.KestrelPierce33, e => { if (e.owner == "player") { beams++; beamHits += e.hits; if (fBeam < 0) fBeam = FeelProbe.FrameCount; } }));
            offs.Add(g.events.On<HitEvent>("hit", e => { if (afterRelease && e.owner == "player" && e.weapon == "revolver") hurt.Add(e.enemy); }));
            string pierceLine;
            ChargeShots27 c = g.loadout.charge27;
            if (line.Count == 0) pierceLine = "PIERCE RED: no husk line placed";
            else if (c == null || !c.active) pierceLine = "PIERCE RED: no charged shot in this run (design.n31.charge off)";
            else
            {
                Husk far = line[line.Count - 1];
                Action aim = () => { God(g); if (far.alive) Look(g, far.AimPoint); };
                yield return Wait27(g, Math.Max(0.1, c.CooldownShare(0) * c.p.cooldownSeconds[0] + 0.1), wallEnd, aim); // a charge that is cooling cannot start
                pad.Send(0f, 1f);
                yield return Wait27(g, c.p.holdSeconds + (c.c35 != null && c.c35.c40 ? c.FullSteps(0) * g.cfg.Dt : c.p.fullSeconds[0]) + 0.15, wallEnd, aim); // NIGHT40 W1: charge40's FULL while live
                afterRelease = true;
                pad.Send(0f, 0f);
                double tRel = sim.SimTime;
                while (fBeam < 0 && sim.SimTime - tRel < 0.3 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                yield return Frames(2);
                yield return Shot30(dir, "feel33-pierce-beam");
                yield return Wait27(g, 0.5, wallEnd, aim);
                bool pierceOk = sim.n33.pierce33 && beams == 1 && beamHits == line.Count && line.Count == 5;
                pierceLine = $"PIERCE {(pierceOk ? "PASS" : "RED")}: {line.Count} husks in a line; kestrelPierce33 beams {beams}, its hits {beamHits}, bodies hurt after the release {hurt.Count}, charged shots fired {c.fired[0]}, player hit sparks since boot {(WeaponFx.Current != null ? WeaponFx.Current.HitSparks33 : -1)} (view33: white-hot + the gun's cool colour on every gun hit; 0 off), muzzle stars since boot {(WeaponFx.Current != null ? WeaponFx.Current.Stars : -1)} | frame f{fBeam:0000} (the beam), shot shot-N30-feel33-pierce-beam.png";
            }
            g.husks.KillAll();
            yield return Frames(2);

            // ---- GUN FEEL ----
            string feelLine = null;
            yield return GunFeel33Part(g, spot, spotYaw, pad, wallEnd, s2 => feelLine = s2);

            // ---- WHIP ----
            string whipLine = null;
            IEnumerator w = Whip33Part(g, dir, wallEnd, s2 => whipLine = s2);
            while (w.MoveNext()) yield return w.Current;
            yield return Frames(Ms(300));
            yield return Stop();
            res[0] = $"{zoomLine} || {pierceLine} || {feelLine} || {whipLine} | {pc}";
        }

        //   GUN FEEL (view33, round 3): the gun root's pose (Viewmodel.RootPos33 / RootEuler33, Unity local m and deg) on four moments, one number each; run
        //   it with -skygrindOff33 for the RED line (the web's layers) and without for GREEN.
        //     idle: 3 s standing still, the root's x range / 2 (mm) and the idle sway's own x (Viewmodel.IdleSwayNow33; 0 off);
        //     bob: 2 s running forward (moveZ 1), the root's y range / 2 over the last 1 s (mm) and the speed (m/s);
        //     dip: dropped from 6 m onto the spot: the deepest landing dip (Viewmodel.LandDipNow33, mm) and the root's largest z change in the 0.4 s after (mm);
        //     recoil: per gun owned of KESTREL / THUNDERCLAP / NAILGUN, one tap: the root's peak change (position mm, pitch deg) and the settle time (the last
        //       frame the change is over 10 % of its peak, ms after the shot).
        //   GREEN (view33): idle sway > 0, the recoil settle 80-150 ms per gun; the bob and dip lines are numbers for the table (their change shows above
        //   12 m/s and in z).
        static IEnumerator GunFeel33Part(SurvivalGame g, Vec3 spot, double spotYaw, Pad27 pad, double wallEnd, Action<string> done)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            Viewmodel vm = g.viewmodel;
            if (vm == null) { done("GUNFEEL RED: no viewmodel"); yield break; }
            bool on = vm.View33On;
            g.husks.KillAll();
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            yield return Wait27(g, 0.5, wallEnd, () => God(g));
            // idle
            float minX = float.MaxValue, maxX = float.MinValue;
            double idleMax = 0;
            int s0 = g.sim.simStep;
            while ((g.sim.simStep - s0) * g.sim.step < 3.0 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                yield return null;
                float x = vm.RootPos33.x;
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                idleMax = Math.Max(idleMax, Math.Abs(vm.IdleSwayNow33));
            }
            double idleMm = (maxX - minX) / 2 * 1000;
            // bob
            float minY = float.MaxValue, maxY = float.MinValue;
            double speedSum = 0;
            int speedN = 0;
            s0 = g.sim.simStep;
            while ((g.sim.simStep - s0) * g.sim.step < 2.0 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                g.input.SetLook(spotYaw, 0);
                g.forcedMove = new InputFrame { moveZ = 1, yaw = spotYaw };
                yield return null;
                if ((g.sim.simStep - s0) * g.sim.step < 1.0) continue;
                float y = vm.RootPos33.y;
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
                speedSum += g.sim.mover.Speed;
                speedN++;
            }
            g.forcedMove = new InputFrame();
            double bobMm = minY < float.MaxValue ? (maxY - minY) / 2 * 1000 : double.NaN, runSpeed = speedN > 0 ? speedSum / speedN : 0;
            // dip
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            yield return Wait27(g, 0.5, wallEnd, () => God(g));
            float z0 = vm.RootPos33.z;
            g.Respawn(new Vec3(spot.x, spot.y + 6, spot.z), spotYaw * 180 / Math.PI);
            double dipMax = 0, dipZ = 0;
            bool landed = false;
            int landStep = -1;
            s0 = g.sim.simStep;
            while ((g.sim.simStep - s0) * g.sim.step < 2.5 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                yield return null;
                if (!landed && g.sim.mover.grounded) { landed = true; landStep = g.sim.simStep; }
                if (landed && (g.sim.simStep - landStep) * g.sim.step < 0.4)
                {
                    dipMax = Math.Max(dipMax, vm.LandDipNow33);
                    dipZ = Math.Max(dipZ, Math.Abs(vm.RootPos33.z - z0));
                }
            }
            // recoil per gun
            var recoil = new List<string>();
            bool settleOk = true;
            foreach (string gun in new[] { "revolver", "shotgun", "nailgun" })
            {
                if (g.loadout.owned.IndexOf(gun) < 0) continue;
                yield return Equip27(g, gun);
                yield return Wait27(g, 1.0, wallEnd, () => God(g));
                Vector3 p0 = vm.RootPos33;
                float pitch0 = vm.RootEuler33.x;
                pad.Send(0f, 1f);
                yield return null;
                yield return null; // whole Player frames (L-039)
                pad.Send(0f, 0f);
                double tShot = g.sim.SimTime, peak = 0, peakPitch = 0;
                var samples = new List<(double t, double d)>();
                while (g.sim.SimTime - tShot < 0.6 && Time.realtimeSinceStartup < wallEnd)
                {
                    God(g);
                    yield return null;
                    double d = (vm.RootPos33 - p0).magnitude;
                    double dp = Mathf.Abs(Mathf.DeltaAngle(pitch0, vm.RootEuler33.x));
                    samples.Add((g.sim.SimTime - tShot, d));
                    peak = Math.Max(peak, d);
                    peakPitch = Math.Max(peakPitch, dp);
                }
                double settle = 0;
                foreach (var (t, d) in samples) if (d > peak * 0.1) settle = t;
                if (on && (settle < 0.06 || settle > 0.17)) settleOk = false;
                recoil.Add($"{gun} peak {(peak * 1000).ToString("0", ci)} mm / {peakPitch.ToString("0.0", ci)} deg, settle {(settle * 1000).ToString("0", ci)} ms");
            }
            bool ok = on && idleMax > 0 && settleOk && recoil.Count > 0;
            done($"GUNFEEL {(ok ? "PASS" : "RED")}: view33 {on}, viewmodel FOV {(g.cam != null && g.cam.transform.Find("Viewmodel camera") != null ? g.cam.transform.Find("Viewmodel camera").GetComponent<Camera>().fieldOfView.ToString("0", ci) : "-")}; idle: root x +-{idleMm.ToString("0.0", ci)} mm (idle sway {(idleMax * 1000).ToString("0.0", ci)} mm); bob at {runSpeed.ToString("0.0", ci)} m/s: +-{bobMm.ToString("0.0", ci)} mm; dip from 6 m: {(dipMax * 1000).ToString("0", ci)} mm down, z {(dipZ * 1000).ToString("0", ci)} mm; recoil [{string.Join("; ", recoil)}] (table: bob scale at 16 m/s {GunFeel33.BobScale(on, 16, g.cfg.movement.ground.walkSpeed, g.cfg.Num("viewmodel.bobMaxScale")).ToString("0.00", ci)}; class settle light / medium / heavy {(GunFeel33.SettleSeconds(8, 0.5, GunFeel33.RecoilRate(on, "light")) * 1000).ToString("0", ci)} / {(GunFeel33.SettleSeconds(6, 0.45, GunFeel33.RecoilRate(on, "medium")) * 1000).ToString("0", ci)} / {(GunFeel33.SettleSeconds(4.5, 0.42, GunFeel33.RecoilRate(on, "heavy")) * 1000).ToString("0", ci)} ms)");
        }

        /// <summary>A charger (a bag, frozen) 10 m out on a clear heading with floor; null when none.</summary>
        static Husk PlaceKind33(SurvivalGame g, string kind, double d)
        {
            Vec3 eye = g.Eye;
            double floorY = g.Feet.y;
            foreach (double yaw in Yaws(g))
            {
                double x = eye.x - Math.Sin(yaw) * d, z = eye.z - Math.Cos(yaw) * d;
                if (!FloorAt(g, x, z, floorY)) continue;
                Vec3 to = new Vec3(x, floorY + 1, z) - eye;
                double len = to.Length;
                if (g.world.RayIntersect(eye, to / len, out RayHit wall) && wall.distance < len + 1) continue;
                Husk h = g.husks.Spawn(kind, new Vec3(x, floorY, z), eye);
                if (h != null) return h;
            }
            return null;
        }

        //   WHIP (whip33 + view33's rope): the ARCLINE owned, a frozen bag CHARGER 10 m out; a TAP on the virtual input path (pressed for whole frames until
        //   the hook is out, then up: the NIGHT31 latch keeps the pull). Lines: the hook's flight, the bite, the stop (whip33Arrive: its gap), the least
        //   gap and the frames inside the charger's hit volume, the whip's hit. Shots: feel33-whip-fly (the hook head in flight, if a frame catches it),
        //   feel33-whip-latch (the taut rope from the muzzle), feel33-whip-stop (the stop short of the charger), feel33-whip-snap (the rope snapping back).
        //   PASS (whip33 on): one stop, the least gap >= stopGap, 0 frames inside, one hit. Off: NIGHT32's zip (through the body, no hit): RED.
        static IEnumerator Whip33Part(SurvivalGame g, string dir, double wallEnd, Action<string> done)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            if (sim.arcline21 == null) { done("WHIP RED: no ARCLINE in this run (design.n21.arcline off)"); yield break; }
            if (!sim.arcline21.Owned) sim.arcline21.Own(true);
            Husk c = PlaceKind33(g, "charger", 10);
            if (c == null) { done("WHIP RED: no charger placed 10 m out"); yield break; }
            c.hp = c.maxHp = 1e9;
            g.husks.frozen = true;
            yield return Wait27(g, 0.5, wallEnd, () => { God(g); Look(g, c.AimPoint); });
            int stops = 0, hits = 0, fFly = -1, fLatch = -1, fStop = -1, fSnap = -1, inside = 0;
            double stopGap = double.NaN, least = double.MaxValue;
            var offs2 = new List<Action>();
            offs2.Add(g.events.On<Whip33Event>("whip33Arrive", e => { stops++; stopGap = e.gap; if (fStop < 0) fStop = FeelProbe.FrameCount; }));
            offs2.Add(g.events.On<HitEvent>("hit", e => { if (e.weapon == SurvivalSim.Whip33Weapon) hits++; }));
            bool shotFly = false, shotLatch = false, shotStop = false, shotSnap = false, wasRoped = false;
            double t0 = sim.SimTime;
            bool pressing = true;
            while (sim.SimTime - t0 < 2.5 && Time.realtimeSinceStartup < wallEnd)
            {
                God(g);
                Look(g, c.AimPoint);
                Rope r = sim.mover.rope;
                if (pressing && (r.phase != RopePhase.None || sim.SimTime - t0 > 0.25)) pressing = false;
                g.forcedMove = new InputFrame { grapple = new Btn { held = pressing, pressed = pressing } };
                yield return null;
                r = sim.mover.rope;
                if (c.alive)
                {
                    least = Math.Min(least, sim.Whip33Gap(c));
                    Vec3 p = sim.mover.position;
                    if (Math.Sqrt((p.x - c.position.x) * (p.x - c.position.x) + (p.z - c.position.z) * (p.z - c.position.z)) < c.Radius + g.cfg.movement.body.capsuleRadius && p.y < c.position.y + c.Height && p.y + g.cfg.movement.body.capsuleHeight > c.position.y) inside++;
                }
                if (r.phase == RopePhase.Flying && fFly < 0) { fFly = FeelProbe.FrameCount; if (!shotFly) { shotFly = true; yield return Shot30(dir, "feel33-whip-fly"); } }
                if (r.phase == RopePhase.Attached && fLatch < 0) { fLatch = FeelProbe.FrameCount; if (!shotLatch) { shotLatch = true; yield return Shot30(dir, "feel33-whip-latch"); } }
                if (r.phase != RopePhase.None) wasRoped = true;
                else if (wasRoped && fSnap < 0) { fSnap = FeelProbe.FrameCount; if (!shotSnap) { shotSnap = true; yield return Shot30(dir, "feel33-whip-snap"); } }
                if (stops > 0 && !shotStop) { shotStop = true; yield return Frames(3); yield return Shot30(dir, "feel33-whip-stop"); }
            }
            g.forcedMove = new InputFrame();
            foreach (Action off in offs2) off();
            bool on = sim.n33.whip33;
            bool ok = on && stops == 1 && hits == 1 && inside == 0 && least >= g.cfg.Num("design.n33.whip33.stopGap") - 1e-6;
            done($"WHIP {(ok ? "PASS" : "RED")}: whip33 {on}, view33 rope {N33Fixes.ViewRowOn(g.cfg, "view33")}; a charger 10 m out: stops {stops} (gap at the stop {(double.IsNaN(stopGap) ? "-" : stopGap.ToString("0.00", ci))} m), least gap {(least == double.MaxValue ? "-" : least.ToString("0.00", ci))} m, frames inside {inside}, whip hits {hits} | frames: fly f{fFly:0000}, latch f{fLatch:0000}, stop f{fStop:0000}, snap f{fSnap:0000}; shots shot-N30-feel33-whip-fly / -latch / -stop / -snap.png");
        }
    }
}
