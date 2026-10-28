// EXT53 builder N (row N, RIDE KILL): the a253 phase (FeelReel.N53.cs routes it here): design.n53.rideKill53's Player proof. Not run by its
// author (builders never run a Player). A campaign level with a ride wall near a room's spawn (n52-iceline; n52-foundry works too):
//   bash Tools/night/n47-reels.sh <tag> rk1,n52-iceline,a253                   (the build copy's config with design.n53.rideKill53 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> rk1off,n52-iceline,a253,-skygrindOff53 (the row off: RED)
// The CAMPAIGN on the booted map (L-062), DOORFREE, God, enemies frozen, the player's own input back (nothing pressed but the reel's), the
// KESTREL in hand. The wall: RideKill53Wall.Find around the campaign room's spawn (inside its rect first, then anywhere within
// RideKill53ReelReach), the same search the harness's `n53ridekill ride` runs headless. Two parts, each printing that its input happened first
// (L-089):
//   1. FOOT (the control): the player standing at the run line's start, a husk (1 hp, held in place) RideKill53ReelAhead m along the wall and
//      RideKill53ReelOut m off it, aimed at and shot: the kill happens, no rideKill53, no callout.
//   2. RIDE: the player put RideKill53ReelUp m up at the line's start moving RideKill53ReelSpeed m/s along the wall, jump held, the stick along
//      it and leaning in, until the Mover is in WallRide (WallRideStart counted); a husk ahead as in 1, the bar emptied, the shot fired from
//      the wall: the kill on the wall, rideKill53 once, the bar up by one boost (design.stamina.boost.cost) over the kill's own refill, the
//      RIDE KILL callout up (alpha >= 0.5) within RideKill53ReelCalloutFrames. Frames: shot-N30-N53-ridekill-foot.png, -ride.png (riding,
//      before the shot), -callout.png (2 frames after the kill).
// One line: "GREEN a253 RIDE KILL | ..." (or RED, with each check's word), the full one in the Player log as "[a253] ...".
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
        static partial void N53RideKill(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = RideKill53Scenario(g, dir, res, offs, pre);

        /// <summary>The look from the eye at a point: yaw (0 looks toward -z) and pitch (rad, up +).</summary>
        static void RideKill53Look(Vec3 eye, Vec3 at, out double yaw, out double pitch)
        {
            double dx = at.x - eye.x, dy = at.y - eye.y, dz = at.z - eye.z;
            yaw = Math.Atan2(-dx, -dz);
            pitch = Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz));
        }

        /// <summary>The stick for a world direction under a yaw (forward 1 = the look's forward, + right).</summary>
        /// <summary>Pixels of the callout's cyan (bright, blue-green over red: the theme's Cyan to CyanHot) in a screen box of a grabbed frame
        /// (bottom-up rows, as the box: screen px from the bottom left).</summary>
        static int Cyan53(Color32[] px, int w, int h, Rect r)
        {
            if (px == null || r.width < 1 || r.height < 1) return 0;
            int x0 = Mathf.Clamp((int)r.xMin, 0, w - 1), x1 = Mathf.Clamp((int)r.xMax, 0, w - 1), y0 = Mathf.Clamp((int)r.yMin, 0, h - 1), y1 = Mathf.Clamp((int)r.yMax, 0, h - 1);
            int n = 0;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    Color32 p = px[y * w + x];
                    // the theme's Cyan 0x19d3e6 (25, 211, 230) to CyanHot (Cyan 78 % to white: ~204, 242, 249): blue and green bright, red under them
                    if (p.g >= 140 && p.b >= 140 && p.r <= 215 && p.b >= p.r + 30) n++;
                }
            return n;
        }

        static void RideKill53Stick(double yaw, Vec3 w, out double moveX, out double moveZ)
        {
            double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw), rx = Math.Cos(yaw), rz = -Math.Sin(yaw);
            double len = Math.Sqrt(w.x * w.x + w.z * w.z);
            if (len < 1e-9) { moveX = moveZ = 0; return; }
            moveZ = (w.x * fx + w.z * fz) / len;
            moveX = (w.x * rx + w.z * rz) / len;
        }

        static IEnumerator RideKill53Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var spawned = new List<Husk>();
            int rideStarts = 0;
            Action<MoverEvent> onMove = e => { if (e.type == MoverEventType.WallRideStart) rideStarts++; };
            sim.mover.Events += onMove;
            var fired = new List<RideKill53Event>();
            offs.Add(g.events.On<RideKill53Event>(RideKill53.Event, e => fired.Add(new RideKill53Event { onWall = e.onWall, stepsSinceRide = e.stepsSinceRide, bar = e.bar, before = e.before, after = e.after, count = e.count, weapon = e.weapon })));
            offs.Add(() =>
            {
                sim.mover.Events -= onMove;
                foreach (Husk h in spawned) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a253 RIDE KILL: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 240;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 220) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            sim.husks.KillAll();
            bool on = sim.RideKill53On;
            LevelDef lv = sim.campaignLevel;
            // the room: the first of UnityTuning.RideKill53ReelRooms on this level (the harness's `n53ridekill ride` room), else the campaign's
            // current room, else every room in order; per room the wall inside its rect first, then anywhere within the reach of its spawn
            var tryRooms = new List<RoomDef>();
            if (lv != null)
            {
                foreach (string id in UnityTuning.RideKill53ReelRooms) { RoomDef r = lv.rooms.Find(x => x.id == id); if (r != null) tryRooms.Add(r); }
                if (sim.campaign != null && sim.campaign.room >= 0 && sim.campaign.room < lv.rooms.Count) tryRooms.Add(lv.rooms[sim.campaign.room]);
                foreach (RoomDef r in lv.rooms) if (r.kind != "boss") tryRooms.Add(r);
            }
            double reach = UnityTuning.RideKill53ReelReach, len = UnityTuning.RideKill53ReelLength, off = UnityTuning.RideKill53ReelStandOff;
            RoomDef rd = null;
            Vec3 f0 = g.Feet, u = new Vec3(0, 0, -1), n = new Vec3(1, 0, 0);
            bool inRoom = false, found = false;
            foreach (RoomDef r in tryRooms)
            {
                double[] rect = { r.x0, r.z0, r.x1, r.z1 };
                if (RideKill53Wall.Find(g.world, r.spawn, reach, len, off, rect, out f0, out u, out n)) { rd = r; inRoom = found = true; break; }
                if (RideKill53Wall.Find(g.world, r.spawn, reach, len, off, out f0, out u, out n)) { rd = r; found = true; break; }
            }
            string map = PostFx.LevelIdOf(g.map.json);
            if (!found) { res[0] = $"RED a253 RIDE KILL: no ride wall within {reach.ToString("0", ci)} m of any room's spawn on {map} (RideKill53Wall.Find)"; yield break; }
            string wallLine = $"wall {(inRoom ? "in" : "near")} room {rd?.id ?? "?"}: start ({f0.x.ToString("0.0", ci)}, {f0.y.ToString("0.0", ci)}, {f0.z.ToString("0.0", ci)}), along ({u.x.ToString("0.00", ci)}, {u.z.ToString("0.00", ci)})";
            yield return Equip27(g, "revolver"); // the KESTREL
            g.forcedCombat = IdleCombat;
            double unit = sim.mover.stamina.rules.Any ? g.cfg.Num("design.stamina.boost.cost") : g.cfg.movement.dash.pipCost;
            string pc = Preconditions(g, f0);
            Start(g, "a253-ridekill", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // a husk at `spot` (1 hp, held there each frame), aimed at and shot until it dies (a press every 20 frames, at most 120 frames)
            int[] shotFrame = { -1 };
            Color32[][] refPx = new Color32[1][], calPx = new Color32[1][];
            int[] refSize = new int[2], calSize = new int[2];
            IEnumerator ShootAt(Husk hs, Vec3 spot, Func<InputFrame> move, string grabBefore, List<bool> riding, bool needRide)
            {
                double face = Math.Atan2(-u.x, -u.z);
                int lastPress = -100;
                for (int i = 0; i < 120 && hs.alive; i++)
                {
                    foreach (Enemy e in sim.enemies.all) if (e.alive && !ReferenceEquals(e, hs)) e.alive = false; // a room's wave out of the line (silently, as KillAll)
                    hs.position = hs.previous = spot;
                    hs.yaw = face;
                    hs.hp = Math.Min(hs.hp, 1);
                    God(g);
                    sim.mover.stamina.bar = 0; // the refund reads alone (the kill's own refill and the boost back are what the bar holds after)
                    sim.mover.pips = 0;
                    RideKill53Look(sim.Eye, hs.AimPoint, out double yaw, out double pitch);
                    InputFrame mv = move();
                    mv.yaw = yaw;
                    mv.pitch = pitch;
                    if (mv.moveZ != 0 || mv.moveX != 0) RideKill53Stick(yaw, new Vec3(u.x - n.x * UnityTuning.RideKill53ReelLean, 0, u.z - n.z * UnityTuning.RideKill53ReelLean), out mv.moveX, out mv.moveZ);
                    g.input.SetLook(yaw, pitch);
                    g.forcedMove = mv;
                    // a press when the shot's line is clear (and, for the ride, the rider is on the wall), at most every 20 frames: the
                    // harness's `n53ridekill ride --shoot` rule, GREEN headless on n52-iceline g4 and n52-foundry a3
                    bool press = i - lastPress >= 20 && (!needRide || sim.mover.state == MoveState.WallRide) && RideKill53Wall.LineClear(g.world, sim.Eye, hs.AimPoint);
                    if (press && riding.Count == 0 && grabBefore != null) yield return Grab48(dir, refPx, refSize, grabBefore); // the reference frame (the callout's box before it)
                    if (press) { lastPress = i; riding.Add(sim.mover.state == MoveState.WallRide); shotFrame[0] = Time.frameCount; }
                    g.forcedCombat = press ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                    yield return null;
                }
                g.forcedCombat = IdleCombat;
            }

            // ---- 1. FOOT: the control ----
            double yawU = Math.Atan2(-u.x, -u.z);
            g.Respawn(new Vec3(f0.x, f0.y + 0.05, f0.z), yawU * 180 / Math.PI);
            for (int i = 0; i < 40; i++) { God(g); g.forcedMove = new InputFrame { yaw = yawU }; g.input.SetLook(yawU, 0); yield return null; }
            bool footStanding = sim.mover.grounded && sim.mover.state != MoveState.WallRide;
            var footSpot = new Vec3(f0.x + u.x * UnityTuning.RideKill53ReelAhead + n.x * UnityTuning.RideKill53ReelOut, f0.y, f0.z + u.z * UnityTuning.RideKill53ReelAhead + n.z * UnityTuning.RideKill53ReelOut);
            Husk hf = null;
            try { hf = sim.husks.Spawn("husk", footSpot, f0); } catch (Exception) { }
            int ev0 = fired.Count, callouts0 = Ui.RideKill53Callouts, missed0 = sim.RideKill53Missed;
            var footRiding = new List<bool>();
            if (hf != null)
            {
                spawned.Add(hf);
                for (int i = 0; i < 60; i++) { hf.position = hf.previous = footSpot; hf.hp = hf.maxHp = 1; hf.alive = true; God(g); yield return null; } // its spawn tell out (a153 waits 45)
                yield return ShootAt(hf, footSpot, () => new InputFrame(), "N53-ridekill-foot", footRiding, false);
            }
            bool footKill = hf != null && !hf.alive;
            for (int i = 0; i < UnityTuning.RideKill53ReelCalloutFrames; i++) { God(g); yield return null; }
            bool footEvent = fired.Count > ev0, footCallout = Ui.RideKill53Callouts > callouts0;
            int footMissed = sim.RideKill53Missed - missed0;
            string foot = $"foot: standing {(footStanding ? "yes" : "NO")}, kill {(hf == null ? "NO (no husk)" : footKill ? "yes" : "NO")} (shots {footRiding.Count}), rideKill53 {(footEvent ? "FIRED" : "no")}, callout {(footCallout ? "SHOWN" : "no")}, seen and not counted {footMissed}";
            for (int i = 0; i < 70; i++) { God(g); g.forcedMove = new InputFrame { yaw = yawU }; yield return null; } // past the grace of anything before

            // ---- 2. RIDE ----
            sim.husks.KillAll();
            // the target first, its spawn tell waited out (as the harness's --shoot: a shot at a husk still rising does not land), on a fixed spot
            // RideKill53ReelAhead + 2 m along the line (the attach and the first frames cover ~1 m) and RideKill53ReelOut m off the wall
            double ahead = UnityTuning.RideKill53ReelAhead + 2;
            var rideSpot = new Vec3(f0.x + u.x * ahead + n.x * UnityTuning.RideKill53ReelOut, f0.y, f0.z + u.z * ahead + n.z * UnityTuning.RideKill53ReelOut);
            Husk hr = null;
            try { hr = sim.husks.Spawn("husk", rideSpot, f0); } catch (Exception) { }
            if (hr != null)
            {
                spawned.Add(hr);
                for (int i = 0; i < 60; i++) { hr.position = hr.previous = rideSpot; hr.hp = hr.maxHp = 1; hr.alive = true; God(g); g.forcedMove = new InputFrame { yaw = yawU }; yield return null; }
            }
            int starts0 = rideStarts;
            g.Respawn(new Vec3(f0.x, f0.y + UnityTuning.RideKill53ReelUp, f0.z), yawU * 180 / Math.PI);
            double sp = UnityTuning.RideKill53ReelSpeed;
            sim.mover.velocity = new Vec3(u.x * sp - n.x, 0, u.z * sp - n.z);
            InputFrame RideMove()
            {
                var mv = new InputFrame { yaw = yawU, moveZ = 1, jump = new Button { held = true } };
                RideKill53Stick(yawU, new Vec3(u.x - n.x * UnityTuning.RideKill53ReelLean, 0, u.z - n.z * UnityTuning.RideKill53ReelLean), out mv.moveX, out mv.moveZ);
                return mv;
            }
            int waited = 0;
            for (; waited < 60 && sim.mover.state != MoveState.WallRide; waited++) { God(g); g.forcedMove = RideMove(); g.input.SetLook(yawU, 0); yield return null; }
            bool rode = sim.mover.state == MoveState.WallRide;
            int starts = rideStarts - starts0;
            int ev1 = fired.Count, callouts1 = Ui.RideKill53Callouts;
            var rideRiding = new List<bool>();
            float alphaAt = 0f, peakScale = 1f, calAlpha = 0f;
            Rect calRect = default;
            int calloutK = -1;
            if (hr != null && rode)
            {
                hr.position = hr.previous = rideSpot;
                hr.hp = hr.maxHp = 1;
                hr.alive = true;
                yield return ShootAt(hr, rideSpot, RideMove, "N53-ridekill-ride", rideRiding, true);
                for (int k = 0; k <= UnityTuning.RideKill53ReelCalloutFrames; k++)
                {
                    peakScale = Mathf.Max(peakScale, Ui.RideKill53Scale);
                    if (calloutK < 0 && Ui.RideKill53Alpha >= 0.5f) { calloutK = k; alphaAt = Ui.RideKill53Alpha; }
                    God(g);
                    g.forcedMove = RideMove();
                    if (k == 2) { yield return Grab48(dir, calPx, calSize, "N53-ridekill-callout"); calRect = Ui.RideKill53ScreenRect; calAlpha = Ui.RideKill53Alpha; }
                    else yield return null;
                }
            }
            bool rideKill = hr != null && !hr.alive;
            bool shotOnWall = rideRiding.Count > 0 && rideRiding[rideRiding.Count - 1];
            int got = fired.Count - ev1;
            RideKill53Event last = got > 0 ? fired[fired.Count - 1] : null;
            bool refund = got == 1 && last != null && Math.Abs(last.after - last.before - unit) < 1e-6;
            // on screen, not only in the component (n53-r3: alpha 1 under a hidden layer): the word's pixels in its box on the grabbed frame over
            // the same box on the frame before the shot
            int wordPx = Cyan53(calPx[0], calSize[0], calSize[1], calRect), refWordPx = Cyan53(refPx[0], refSize[0], refSize[1], calRect);
            bool onScreen = calPx[0] != null && refPx[0] != null && calRect.width > 1 && wordPx - refWordPx >= UnityTuning.RideKill53ReelMinPx;
            bool callout = Ui.RideKill53Callouts - callouts1 == 1 && calloutK >= 0 && onScreen;
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            yield return Frames(5);
            yield return Stop();
            string F(double v) => v.ToString("0.00", ci);
            string boosts = last != null ? $"{(last.bar ? "bar" : "pips")} {F(last.before)} -> {F(last.after)} (boosts {F(last.before / unit)} -> {F(last.after / unit)})" : "no event";
            string ride = $"ride {(rode ? "yes" : "NO")} (WallRideStart {starts}, attached after {waited} frames), kill {(hr == null ? "NO (no husk)" : rideKill ? "yes" : "NO")} (on the wall: {(shotOnWall && last != null && last.onWall ? "yes" : shotOnWall ? "yes at the shot" : "NO")}), "
                + $"refund {(refund ? "yes" : "NO")} ({boosts}; rideKill53 events {got}), callout {(callout ? "yes" : "NO")} (started {Ui.RideKill53Callouts - callouts1}, on-screen alpha {alphaAt.ToString("0.00", ci)} at +{calloutK}, punch x{peakScale.ToString("0.00", ci)}; "
                + $"word px {wordPx} vs {refWordPx} before in its {calRect.width.ToString("0", ci)}x{calRect.height.ToString("0", ci)} box at ({calRect.x.ToString("0", ci)}, {calRect.y.ToString("0", ci)}), want +{UnityTuning.RideKill53ReelMinPx}; alpha at the grab {calAlpha.ToString("0.00", ci)})";
            bool footOk = footKill && !footEvent && !footCallout;
            bool ok = on && rode && starts > 0 && rideKill && shotOnWall && refund && callout && footOk;
            Debug.Log($"[a253] rideKill53 on {on}; {wallLine}; {foot}; {ride}");
            res[0] = $"{(ok ? "GREEN" : "RED")} a253 RIDE KILL | rideKill53 on {on} (map {map}) | {ride} | {foot} | {wallLine} | {pc}";
        }
    }
}
