// Port of skygrind/src/combat/aimAssist.js (POWER.md P2, OVERNIGHT2 N1, POLISH V1: lock before you shoot), the player's
// shots only. Modes (settings.aimAssist; numbers in aim.lock.modes, scaled by settings.snapStrength and the ADS zoom):
//   off     nothing: the look is never moved.
//   assist  6 deg / 120 ms / 90 deg/s, and friction: sensitivity x0.55 while the crosshair is on the body.
//   snap    (the web's default) 12 deg / 50 ms / 240 deg/s; the brackets pulse once the error is under 1.5 deg.
//   lock    HARD LOCK: 24 deg / 0 ms / 720 deg/s; the spring keeps tracking the target.
// Every aim.lock.scanEvery steps a scan (at most aim.lock.losRays line-of-sight rays): a candidate inside the acquire
// cone for the acquire time is PRE-LOCKED (aimLock), and a critically damped spring pulls the look onto its aim point
// (the head if it was inside the inner half of the cone, else centre mass). The player's mouse always adds on top.
// ASSIST and SNAP settle into a stick (follow + a gentle centering) under settleError; HARD LOCK stays on the spring. A
// fire press leaves on the same step when on target; 1-6 deg off and closing it waits at most fire.maxHold (the spring
// runs x fire.boost meanwhile). A flick faster than the breakaway speed away from the target releases it (blocked for
// reacquireDelay); so do its death, losLost s without sight and leaving releaseCone x the cone. A better candidate
// takes over only after scoring hysteresis.margin better for hysteresis.hold s. Assisted hits pay lockPayout.
// NIGHT9 U3-10: the lock-on key (V, input.bindings.lockOn): a sticky manual lock on the enemy nearest the crosshair
// (an automatic lock becomes manual; again: the next one, wrapping; with no other, it releases), lockOn / lockOff, 1 s
// of blindness allowed (manualLosLost), its release cone at least aim.lockCone, never taken over by a better candidate;
// and the melee lunge (lungeTarget: a body just out of reach, within aim.lunge more, in the cone).
// NIGHT10 U3-13.5: homingTarget() (the lock steers nails and rockets; Projectiles.Steer turns them).
// NIGHT13 U3-26.7 (OVERNIGHT3 P3, aimAssist.js :34-39, :434): getBreakawayCap, deg/s the breakaway flick needs at most (a
// stick turns slower than a mouse flicks: the pad caps it under its own top turn rate; +Infinity = no cap, the default,
// so a run without a pad is unchanged: Math.min(x, Infinity) is x). The pad's own mode comes in through `mode` (main.js
// getMode picks settings.padAimAssist on the pad).
// Not ported (no such things yet): projectile lead, perks (STEADY: N12).
// NIGHT41 P3 aim41 (INVENTORY C2, DECIDE41 D-6): boss weak points, the pad's ASSIST mode only. While `weakPoint41` (the sim sets it at a
// run's start from sim.n41.aim41), the mode is "assist" and the pad plays (a finite breakaway cap: PadReader's, +Infinity on the mouse),
// a body whose Enemy.weakPoint41 is active is gathered, locked and pulled at its weak point instead of its centre (Body), and scores the
// web's weak weight (aim.lock.weights.weak; Candidate.weak). Off, the mouse, or any other mode: every point is e.AimPoint, as before.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class AimLockEvent
    {
        public int id;
        public string kind, part;
        public bool manual, elite, switched;
    }

    /// <summary>lockInfo(): the lock for the HUD (brackets, the crosshair's state).</summary>
    public sealed class LockInfo
    {
        public Enemy enemy;
        public string part, mode;
        public double age, error; // s, deg
        public bool manual, elite, pulse, settled;
    }

    public sealed class AimAssist
    {
        const double DEG = Math.PI / 180;
        readonly GameConfig cfg;
        readonly EventBus events;
        readonly EnemyRegistry enemies;
        readonly ICollisionWorld world;
        readonly Func<(Vec3 eye, Vec3 dir)> getAim;
        readonly Func<string> getWeapon, mode;
        readonly Func<bool> isAds;
        readonly Func<double> strength;
        readonly Func<double> breakawayCap; // getBreakawayCap (deg/s), +Infinity unless the pad is in use
        static readonly Func<double> NoCap = () => double.PositiveInfinity; // aimAssist.js :39's default
        public readonly AssistMath.L lk;
        readonly double pitchLimit;
        Vec3 eye, dir;
        readonly List<AssistMath.Candidate> candidates = new List<AssistMath.Candidate>();
        readonly List<AssistMath.Candidate> pool = new List<AssistMath.Candidate>();
        int steps, rays, scanId = 1;
        double time;
        string modeShown;

        // The lock: its target, the part aimed at, the spring's velocity per axis (rad/s), the target's look angles last
        // step (the follow), the error after the pull (rad) this step and the one before.
        Enemy lockEnemy;
        int lockId;
        string lockPart = "chest";
        bool lockElite, settled, hasT;
        bool lockManual; // the V key made it (NIGHT9 U3-10)
        readonly double lockCone, lockRange, lunge; // aim.lockCone (deg), aim.lockRange (m), aim.lunge (m)
        readonly double homingTurn, lockHomingMult; // aim.homingTurn (deg/s), aim.lockHomingMult (x for a V-key lock)
        public int manualLocks, cycles, lunges;
        double lockAge, lockBlind, vy, vp, tyaw, tpitch, lockError, prevError = double.PositiveInfinity;
        Enemy pendingEnemy; // the candidate being acquired
        int pendingId;
        string pendingPart = "chest";
        double pendingT;
        readonly AssistMath.Rival rival = new AssistMath.Rival();
        bool shotHeld;
        double shotT;
        int blockedId;
        double blockedUntil = double.NegativeInfinity;
        bool hasLastLook;
        double lastYaw, lastPitch, lastErrorAfter, pullLog, userSpeed, friction = 1;
        readonly LockInfo info = new LockInfo();
        // U2-02: the per-step and per-scan objects, made once.
        readonly AssistMath.Params paramsBuf = new AssistMath.Params();
        readonly AssistMath.Candidate mine = new AssistMath.Candidate();
        readonly Func<AssistMath.Candidate, bool> skipLocked, skipAcquire;
        readonly Dictionary<string, double> meleeRange = new Dictionary<string, double>(); // per weapon: its lock range if melee, else -1
        readonly AimLockEvent lockEvent = new AimLockEvent(); // the aimLock payload, reused (no handler keeps it)
        public int locks, switches, breakaways, assisted, holds, heldSteps, instantShots, scans, rayCount;
        public readonly Dictionary<string, int> releases = new Dictionary<string, int>(16); // sized for every reason: no growth mid-run

        /// <summary>NIGHT41 P3 aim41: steer to a boss's weak point (the sim sets it at a run's start from sim.n41.aim41; false = NIGHT40's assist).</summary>
        public bool weakPoint41;
        bool weakNow; // this step: weakPoint41, the "assist" mode and the pad in use (a finite breakaway cap)

        /// <summary>NIGHT41 P3 aim41: the body's point the assist aims at: its active weak point while weakNow, else its centre (e.AimPoint).</summary>
        Vec3 Body(Enemy e) => weakNow && e.weakPoint41.active ? e.weakPoint41.at : e.AimPoint;

        /// <summary>NIGHT41 P3 aim41 round 3 (design.n41.aim41.reachM / acquireScale, set by the sim at a run's start; 0 / 1 = round 1's
        /// assist): a live weak point's own cone, atan(reachM / distance), at least the mode's: a close, short window (THE ICEBREAKER's stuck
        /// fist at 4-5 m) is found from where the stick already is; and its acquire time x acquireScale.</summary>
        public double weakReach41, weakAcquire41 = 1;

        double WeakCone(Enemy e, Vec3 point, double coneRad)
        {
            if (!weakNow || weakReach41 <= 0 || !e.weakPoint41.active) return coneRad;
            return JsMath.Max(coneRad, JsMath.Atan2(weakReach41, JsMath.Max(0.5, Vec3.DistanceTo(point, eye))));
        }

        /// <summary>An angle as the mode's cone sees it: a live weak point's angle scaled by coneRad / its own cone (so it scores and passes
        /// as inside the mode's cone when it is inside its own); any other angle as it is.</summary>
        double WeakAngle(Enemy e, Vec3 point, double angle, double coneRad)
        {
            double wc = WeakCone(e, point, coneRad);
            return wc > coneRad ? angle * (coneRad / wc) : angle;
        }

        public AimAssist(GameConfig cfg, EventBus events, EnemyRegistry enemies, ICollisionWorld world, Func<(Vec3, Vec3)> getAim, Func<string> getWeapon,
            Func<bool> isAds, Func<string> mode, Func<double> strength, Func<double> breakawayCap = null)
        {
            this.cfg = cfg;
            this.events = events;
            this.enemies = enemies;
            this.world = world;
            this.getAim = getAim;
            this.getWeapon = getWeapon;
            this.isAds = isAds;
            this.mode = mode;
            this.strength = strength;
            this.breakawayCap = breakawayCap ?? NoCap;
            lk = AssistMath.From(cfg);
            pitchLimit = cfg.movement.mouse.pitchLimitDeg * DEG;
            lockCone = cfg.Num("aim.lockCone");
            lockRange = cfg.Num("aim.lockRange");
            lunge = cfg.Num("aim.lunge");
            homingTurn = cfg.Num("aim.homingTurn");
            lockHomingMult = cfg.Num("aim.lockHomingMult");
            skipLocked = c => (c.id is int ci && ci == lockId) || Skip(c);
            skipAcquire = c => Skip(c) || c.rejected == scanId;
        }

        public string Mode => mode();

        static bool Alive(Enemy e, int id) => e != null && e.alive && e.id == id;

        bool Skip(AssistMath.Candidate c) => c.id is int ci && ci == blockedId && time < blockedUntil;

        /// <summary>The scan range for a weapon: a melee one's reach (weapons.(w).range, else 3) + meleeReach, else lk.range.</summary>
        double ScanRange(string w)
        {
            if (w == null) return lk.range;
            if (!meleeRange.TryGetValue(w, out double r))
            {
                bool melee = cfg.Has($"weapons.catalog.{w}.kind") && cfg.Str($"weapons.catalog.{w}.kind") == "melee";
                r = melee ? (cfg.Has($"weapons.{w}.range") ? cfg.Num($"weapons.{w}.range") : 3) + lk.meleeReach : -1;
                meleeRange[w] = r;
            }
            return r >= 0 ? r : lk.range;
        }

        double Threat(string kind) => lk.threat.TryGetValue(kind, out double t) ? t : 0.3;

        /// <summary>This step's numbers (mode, strength, zoom), or null for no lock at all. One reused object: read it, don't keep it.</summary>
        public AssistMath.Params Params() => AssistMath.LockParamsInto(paramsBuf, mode(), strength(), isAds(), lk) ? paramsBuf : null;

        AssistMath.Candidate Slot(int i)
        {
            while (pool.Count <= i) pool.Add(new AssistMath.Candidate());
            return pool[i];
        }

        static bool HeadOf(Enemy e, out Vec3 head)
        {
            if (e is Husk h && h.HasHead)
            {
                head = h.HeadCenter;
                return true;
            }
            head = default;
            return false;
        }

        /// <summary>Every live enemy within `range` m and `maxAngle` rad of the aim, into `candidates` (sorted by angle, stable).</summary>
        void Gather(double range, double maxAngle)
        {
            int count = 0;
            candidates.Clear();
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                AssistMath.Candidate c = Slot(count);
                c.chest = Body(e); // NIGHT41 aim41: the weak point while weakNow (else e.AimPoint)
                c.distance = Vec3.DistanceTo(c.chest, eye);
                if (c.distance > range) continue;
                c.head = HeadOf(e, out c.headAt);
                c.angle = Math.Min(AssistMath.AngleTo(eye, dir, c.chest), c.head ? AssistMath.AngleTo(eye, dir, c.headAt) : double.PositiveInfinity);
                c.angle = WeakAngle(e, c.chest, c.angle, maxAngle); // NIGHT41 aim41 round 3: a live weak point's own reach (unchanged unless weakNow)
                if (c.angle > maxAngle) continue;
                c.id = Boxes.Int(e.id);
                c.enemy = e;
                c.threat = Threat(e.kind);
                c.weak = weakNow && e.weakPoint41.active; // NIGHT41 aim41 (false whenever weakNow is)
                candidates.Add(c);
                count++;
            }
            for (int i = 1; i < candidates.Count; i++)
            {
                AssistMath.Candidate x = candidates[i];
                int j = i - 1;
                while (j >= 0 && candidates[j].angle > x.angle)
                {
                    candidates[j + 1] = candidates[j];
                    j--;
                }
                candidates[j + 1] = x;
            }
        }

        /// <summary>Line of sight from the eye to a point (one ray).</summary>
        bool Sees(Vec3 point)
        {
            rayCount++;
            Vec3 d = point - eye;
            double len = d.Length;
            d = Vec3.DivideScalar(d, len > 0 ? len : 1);
            return !world.RayIntersect(eye, d, out RayHit hit) || hit.distance >= len - lk.losSlack;
        }

        /// <summary>The point to aim at on an enemy: its head (when that part was chosen and it has one), else its chest.</summary>
        Vec3 AimPointOf(Enemy enemy, string part) => part == "head" && HeadOf(enemy, out Vec3 head) ? head : Body(enemy); // NIGHT41 aim41: Body

        string PartFor(AssistMath.Candidate c, double coneRad) => c.head && AssistMath.AngleTo(eye, dir, c.headAt) <= coneRad * 0.5 ? "head" : "chest";

        /// <summary>The candidate's chosen point in sight: its part, or the other one when it is hidden; null when neither shows.</summary>
        string VisiblePart(AssistMath.Candidate c, double coneRad)
        {
            string part = PartFor(c, coneRad);
            Vec3 point = part == "head" ? c.headAt : c.chest;
            if (rays <= 0) return null;
            rays--;
            if (Sees(point)) return part;
            string other = part == "chest" ? (c.head ? "head" : null) : "chest";
            if (other == null || rays <= 0) return null;
            rays--;
            return Sees(other == "head" ? c.headAt : c.chest) ? other : null;
        }

        void AimAlong(double yaw, double pitch)
        {
            eye = getAim().eye;
            dir = AssistMath.LookDir(yaw, pitch);
        }

        void StartLock(Enemy enemy, string part, bool manual = false)
        {
            bool switching = lockEnemy != null;
            if (switching && lockManual && !manual) events.Emit("lockOff", "switch");
            lockEnemy = enemy;
            lockManual = manual;
            lockId = enemy.id;
            lockPart = part;
            lockElite = lk.elite.Contains(enemy.kind);
            lockAge = 0;
            lockBlind = 0;
            settled = false;
            hasT = false;
            lockError = double.PositiveInfinity; // unknown until the first pull: that first step always counts as closing
            prevError = double.PositiveInfinity;
            if (!switching)
            {
                vy = 0;
                vp = 0;
            }
            pendingEnemy = null;
            rival.id = null;
            rival.t = 0;
            locks++;
            if (switching) switches++;
            if (manual) manualLocks++;
            lockEvent.id = enemy.id;
            lockEvent.kind = enemy.kind;
            lockEvent.part = part;
            lockEvent.manual = manual;
            lockEvent.elite = lockElite;
            lockEvent.switched = switching;
            events.Emit("aimLock", lockEvent);
            if (manual) events.Emit("lockOn", lockEvent); // {id, kind} (the aimLock payload carries both)
        }

        static void ReleaseShot(ref CombatInput frame)
        {
            frame.fire.pressed = true;
            frame.fire.held = true;
        }

        /// <summary>End the lock; true when a held press must leave now (never lost).</summary>
        bool EndLock(string reason)
        {
            bool release = shotHeld;
            shotHeld = false;
            if (lockEnemy == null) return release;
            bool manual = lockManual;
            lockEnemy = null;
            lockManual = false;
            vy = 0;
            vp = 0;
            friction = 1;
            releases[reason] = (releases.TryGetValue(reason, out int r) ? r : 0) + 1;
            events.Emit("aimRelease", reason);
            if (manual) events.Emit("lockOff", reason);
            return release;
        }

        bool RayOnTarget()
        {
            EnemyRayHit? hit = enemies.Raycast(eye, dir, lk.range);
            return hit.HasValue && hit.Value.enemy == lockEnemy;
        }

        /// <summary>One step of the pull on the locked target: follow its motion (capped), then the spring or the stick's centering.</summary>
        double Pull(ref double yaw, ref double pitch, double dt, AssistMath.Params p)
        {
            Vec3 tmp = AimPointOf(lockEnemy, lockPart);
            var aim = AssistMath.YawPitchTo(eye, tmp);
            double fy = 0, fp = 0;
            if (hasT)
            {
                fy = AssistMath.WrapAngle(aim.yaw - tyaw);
                fp = aim.pitch - tpitch;
                double f = JsMath.Hypot(fy, fp);
                double fMax = p.follow * DEG * dt;
                if (f > fMax)
                {
                    fy *= fMax / f;
                    fp *= fMax / f;
                }
            }
            tyaw = aim.yaw;
            tpitch = aim.pitch;
            hasT = true;
            double xy = AssistMath.WrapAngle(yaw + fy - aim.yaw);
            double xp = pitch + fp - aim.pitch;
            double dy, dp;
            if (p.track || !settled || shotHeld)
            {
                double boost = shotHeld ? lk.fire.boost : 1;
                double omega = p.omega * boost;
                var sy = AssistMath.SpringStep(xy, vy, omega, dt);
                var sp = AssistMath.SpringStep(xp, vp, omega, dt);
                dy = sy.x - xy;
                dp = sp.x - xp;
                double d = JsMath.Hypot(dy, dp);
                double dMax = p.maxPull * boost * DEG * dt;
                double k = d > dMax ? dMax / d : 1;
                dy *= k;
                dp *= k;
                vy = sy.v * k;
                vp = sp.v * k;
            }
            else
            {
                double c = Math.Min(1, lk.center * dt);
                dy = -xy * c;
                dp = -xp * c;
                vy = 0;
                vp = 0;
            }
            yaw += fy + dy;
            pitch = Math.Max(-pitchLimit, Math.Min(pitchLimit, pitch + fp + dp));
            pullLog += JsMath.Hypot(fy + dy, fp + dp) / DEG;
            double error = AssistMath.LookError(yaw, pitch, aim.yaw, aim.pitch);
            prevError = lockError;
            lockError = error;
            if (!p.track && error < lk.settleError * DEG) settled = true;
            lockAge += dt;
            // Friction (ASSIST): a slower look while the crosshair sits on the body.
            double bodyRad = JsMath.Atan2(lk.targetRadius, Math.Max(0.5, Vec3.DistanceTo(tmp, eye)));
            friction = p.friction < 1 && error < bodyRad ? p.friction : 1;
            return error;
        }

        /// <summary>A scan: the lock's upkeep (death, sight, the release cone, a better candidate) or a new candidate's acquisition.</summary>
        void Scan(double dtScan, double yaw, double pitch, AssistMath.Params p, ref CombatInput frame)
        {
            scans++;
            rays = lk.losRays;
            AimAlong(yaw, pitch);
            double range = ScanRange(getWeapon());
            double coneRad = p.cone * DEG;
            Gather(range, coneRad);
            if (lockEnemy != null)
            {
                Vec3 tmp = default;
                if (!Alive(lockEnemy, lockId))
                {
                    if (EndLock("dead")) ReleaseShot(ref frame);
                }
                else
                {
                    tmp = AimPointOf(lockEnemy, lockPart);
                    double angle = AssistMath.AngleTo(eye, dir, tmp);
                    double releaseRad = JsMath.Max(WeakCone(lockEnemy, tmp, coneRad), lockManual ? lockCone * DEG : 0) * lk.releaseCone; // NIGHT41 aim41: a live weak point's own cone (coneRad unless weakNow)
                    if (angle > releaseRad)
                    {
                        if (EndLock("cone")) ReleaseShot(ref frame);
                    }
                    else
                    {
                        rays--;
                        lockBlind = Sees(tmp) ? 0 : lockBlind + dtScan;
                        if (lockBlind > (lockManual ? lk.manualLosLost : lk.losLost) && EndLock("los")) ReleaseShot(ref frame);
                    }
                }
                // Hysteresis: a candidate scoring hysteresis.margin better for hysteresis.hold s takes over (never over a manual lock).
                if (lockEnemy != null && !lockManual)
                {
                    AssistMath.Candidate best = AssistMath.PickBest(candidates, coneRad, range, lk.weights, skipLocked);
                    string part = best != null ? VisiblePart(best, coneRad) : null;
                    mine.angle = WeakAngle(lockEnemy, tmp, AssistMath.AngleTo(eye, dir, tmp), coneRad); // NIGHT41 aim41: as Gather scores it
                    mine.distance = Vec3.DistanceTo(tmp, eye);
                    mine.threat = Threat(lockEnemy.kind);
                    mine.head = lockPart == "head";
                    mine.weak = lockPart == "weak" || weakNow && lockEnemy.weakPoint41.active; // NIGHT41 aim41: as Gather scores it
                    double current = AssistMath.LockScore(mine, coneRad, range, lk.weights);
                    double theirs = part != null ? AssistMath.LockScore(best, coneRad, range, lk.weights) : 0;
                    if (AssistMath.Challenge(rival, current, part != null ? best.id : null, theirs, dtScan, lk.hysteresisMargin, lk.hysteresisHold)) StartLock(best.enemy, part);
                }
                return;
            }
            // No lock: acquire. Never while the player's own mouse sweeps faster than acquireMaxTurn (a flick stays theirs).
            if (userSpeed > lk.acquireMaxTurn)
            {
                pendingEnemy = null;
                return;
            }
            AssistMath.Candidate pick = null;
            string pickPart = null;
            while (rays > 0)
            {
                AssistMath.Candidate best = AssistMath.PickBest(candidates, coneRad, range, lk.weights, skipAcquire);
                if (best == null) break;
                pickPart = VisiblePart(best, coneRad);
                if (pickPart != null)
                {
                    pick = best;
                    break;
                }
                best.rejected = scanId; // behind a wall: try the next
            }
            scanId++;
            if (pick == null)
            {
                pendingEnemy = null;
                return;
            }
            // First seen: it has been inside the cone for up to a scan interval already.
            if (pendingEnemy != null && pendingId == pick.enemy.id) pendingT += dtScan;
            else
            {
                pendingEnemy = pick.enemy;
                pendingId = pick.enemy.id;
                pendingT = dtScan;
            }
            pendingPart = pickPart;
            if (pendingT >= (pick.weak ? p.acquireTime * weakAcquire41 : p.acquireTime) - 1e-9) StartLock(pick.enemy, pickPart); // NIGHT41 aim41: a live weak point x acquireScale
        }

        public Enemy Target => Alive(lockEnemy, lockId) ? lockEnemy : null;

        /// <summary>The V key's manual lock target (or null).</summary>
        public Enemy ManualTarget => lockManual && Alive(lockEnemy, lockId) ? lockEnemy : null;

        /// <summary>aimAssist.js homingTarget() (NIGHT10 U3-13.5): projectiles fired now home on the lock.</summary>
        public int homing; // stats.homing

        /// <summary>
        /// The target a projectile fired now homes on: {enemy, id, turn (rad/s)}, or null. A manual lock homes twice as hard.
        /// </summary>
        public HomingTarget? HomingTarget()
        {
            if (mode() == "off" || lockEnemy == null || !Alive(lockEnemy, lockId)) return null;
            AssistMath.Params p = Params();
            double k = p != null ? p.k : 1;
            homing++;
            return new HomingTarget { enemy = lockEnemy, id = lockId, turn = homingTurn * DEG * k * (lockManual ? lockHomingMult : 1) };
        }

        /// <summary>
        /// A melee lunge: the enemy (in `targets`, punch-target shapes) nearest the aim within `range + lunge` m and the
        /// cone, or null. The caller moves the player up to `lunge` m toward it.
        /// </summary>
        public TargetPick? LungeTarget(Vec3 origin, Vec3 aimDir, IReadOnlyList<BodyTarget> targets, double range, double halfAngle)
        {
            if (mode() == "off") return null;
            TargetPick? best = null;
            for (int i = 0; i < targets.Count; i++)
            {
                BodyTarget t = targets[i];
                var tmp = new Vec3(t.basePos.x, t.basePos.y + t.height * 0.5, t.basePos.z);
                double d = Vec3.DistanceTo(tmp, origin) - t.radius;
                if (d > range + lunge || AssistMath.AngleTo(origin, aimDir, tmp) > halfAngle) continue;
                if (!best.HasValue || d < best.Value.distance) best = new TargetPick { target = t, distance = d };
            }
            if (best.HasValue) lunges++;
            return best;
        }

        /// <summary>
        /// The lock-on key: a sticky manual lock on the enemy nearest the crosshair (an automatic lock becomes manual);
        /// again: the next one (wrapping); with no other, it releases.
        /// </summary>
        public void ToggleLock()
        {
            if (Params() == null) return;
            var (e, d) = getAim();
            eye = e;
            dir = d;
            Gather(lockRange, lockCone * DEG); // x the STEADY perk's cone once perks reach the assist (N12)
            var seen = new List<AssistMath.Candidate>();
            foreach (AssistMath.Candidate c in candidates) if (Sees(c.chest)) seen.Add(c);
            AssistMath.Candidate next = AssistMath.NextLock(seen, lockEnemy != null && lockManual ? Boxes.Int(lockId) : null);
            if (next == null)
            {
                if (lockEnemy != null)
                {
                    blockedId = lockId;
                    blockedUntil = time + lk.reacquireDelay;
                    EndLock("toggle"); // no frame at a key press: a held shot is dropped, as on the web
                }
                return;
            }
            if (lockEnemy != null && lockManual) cycles++;
            StartLock(next.enemy, PartFor(next, lockCone * DEG), true);
        }

        /// <summary>x mouse sensitivity now (ASSIST's friction while the crosshair is on the locked body; else 1).</summary>
        public double LookScale => lockEnemy != null ? friction : 1;

        /// <summary>NIGHT41 P3 aim41 (the view's lock brackets, Ui.Brackets): true with the live weak point the lock pulls to while weakNow
        /// (aim41, the pad, ASSIST) and the locked body's weak point is active; false otherwise (the brackets frame the chest, as before). A read:
        /// no state changes.</summary>
        public bool LockAt41(out Vec3 at)
        {
            at = default;
            if (!weakNow || !Alive(lockEnemy, lockId) || !lockEnemy.weakPoint41.active || lockPart == "head") return false;
            at = lockEnemy.weakPoint41.at;
            return true;
        }

        public LockInfo Info()
        {
            if (!Alive(lockEnemy, lockId)) return null;
            AssistMath.Params p = Params();
            info.enemy = lockEnemy;
            info.part = lockPart;
            info.age = lockAge;
            info.error = lockError / DEG;
            info.manual = lockManual;
            info.elite = lockElite;
            info.pulse = p != null && p.pulse > 0 && lockError / DEG < p.pulse;
            info.settled = settled;
            info.mode = mode();
            return info;
        }

        /// <summary>The x points and style an assisted hit pays now (1 when OFF).</summary>
        public double Payout() => mode() == "off" ? 1 : AssistMath.LockPayout(strength(), lk);

        /// <summary>
        /// One fixed step, before the shots: the player's own turn (a breakaway flick), a press that confirms the candidate
        /// being acquired, the pull, the fire gate (it may hold this step's press, at most fire.maxHold), then a scan every
        /// scanEvery steps. Turns the look (yaw, pitch) in place; may hold or release `frame`'s fire.
        /// </summary>
        public void Step(double dt, ref CombatInput frame, ref double yaw, ref double pitch)
        {
            steps++;
            time += dt;
            pullLog *= JsMath.Exp(-dt / lk.assistedWindow);
            string m = mode();
            weakNow = weakPoint41 && m == "assist" && !double.IsPositiveInfinity(breakawayCap()); // NIGHT41 aim41: the pad's assist only (D-6)
            if (m != modeShown)
            {
                if (modeShown != null && EndLock("mode")) ReleaseShot(ref frame);
                pendingEnemy = null;
                modeShown = m;
            }
            AssistMath.Params p = Params();
            if (p == null)
            {
                if (EndLock("off")) ReleaseShot(ref frame);
                pendingEnemy = null;
                hasLastLook = false;
                return;
            }
            // The player's own turn since we last touched the look.
            double userTurn = hasLastLook ? AssistMath.LookError(yaw, pitch, lastYaw, lastPitch) : 0;
            userSpeed = userTurn / dt / DEG;
            AimAlong(yaw, pitch);
            if (lockEnemy != null && !Alive(lockEnemy, lockId) && EndLock("dead")) ReleaseShot(ref frame);
            if (lockEnemy != null)
            {
                var aim = AssistMath.YawPitchTo(eye, AimPointOf(lockEnemy, lockPart));
                double errorNow = AssistMath.LookError(yaw, pitch, aim.yaw, aim.pitch);
                if (AssistMath.BreaksAway(userTurn, dt, JsMath.Min(p.breakaway, breakawayCap()), lastErrorAfter, errorNow))
                {
                    breakaways++;
                    blockedId = lockId;
                    blockedUntil = time + lk.reacquireDelay;
                    if (EndLock("breakaway")) ReleaseShot(ref frame);
                }
            }
            // A press while a candidate is being acquired confirms it now (the intent is clear).
            if (frame.fire.pressed && lockEnemy == null && Alive(pendingEnemy, pendingId)) StartLock(pendingEnemy, pendingPart);
            if (lockEnemy != null)
            {
                if (frame.fire.pressed && !shotHeld)
                {
                    // Decide before the pull whether this press waits: it waits only while the spring is still closing in.
                    shotHeld = true;
                    shotT = 0;
                }
                lastErrorAfter = Pull(ref yaw, ref pitch, dt, p);
                AimAlong(yaw, pitch);
                if (shotHeld)
                {
                    bool onTarget = RayOnTarget();
                    if (AssistMath.FireReady(onTarget, lockError / DEG, prevError / DEG, shotT + dt, lk.fire))
                    {
                        if (shotT == 0) instantShots++;
                        shotHeld = false;
                        ReleaseShot(ref frame);
                    }
                    else
                    {
                        if (shotT == 0) holds++;
                        shotT += dt;
                        frame.fire.pressed = false;
                        frame.fire.held = false;
                        heldSteps++;
                    }
                }
            }
            else
            {
                friction = 1;
                lastErrorAfter = 0;
            }
            if (steps % lk.scanEvery == 0) Scan(dt * lk.scanEvery, yaw, pitch, p, ref frame);
            hasLastLook = true;
            lastYaw = yaw;
            lastPitch = pitch;
        }

        /// <summary>At a shot: was it assisted (the lock turned the camera recently)? Shots always leave along the look.</summary>
        public bool Aim()
        {
            if (mode() == "off" || lockEnemy == null || pullLog < lk.assistedPull) return false;
            assisted++;
            return true;
        }

        /// <summary>
        /// NIGHT16 (the NIGHT14 residual; port-only, SurvivalSim.CleanSlate at a run's start): the scan clock as a fresh assist
        /// has it. The scan runs when steps % aim.lock.scanEvery == 0, and steps counted on across runs, so a played
        /// composition's next run scanned (and locked, and turned the look) on other steps than a fresh one's: the seed-1
        /// run left the baseline at checkpoint 6. Reset (main.js aimAssist.reset, also a death or a mode switch) keeps its
        /// web behaviour. time only measures blockedUntil, which Reset clears.
        /// </summary>
        public void NewRun()
        {
            steps = 0;
            time = 0;
        }

        /// <summary>Drop the lock (a mode switch, a death, a new run).</summary>
        public void Reset()
        {
            EndLock("reset");
            pendingEnemy = null;
            rival.id = null;
            hasLastLook = false;
            blockedUntil = double.NegativeInfinity;
            pullLog = 0;
        }
    }
}
