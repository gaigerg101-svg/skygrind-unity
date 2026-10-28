// NIGHT14 U3-27.5: a STAND-IN brain for the AUTOPILOT while agent N ports botBrain.js (Core/Bot/BotBrain.cs), so the
// adapter, the shopper, determinism and the farm can be built and tested first. It is NOT the web's bot and every
// measurement made with it is labelled "stand-in": it faces the target (bot.turnRate), rerolls an aim error the way
// botBrain.js does (bot.aimErrorBase + aimErrorPerSpeed x speed, x aimErrorMult[difficulty], every bot.errorReroll, two
// rng draws), checks line of sight at bot.losInterval, keeps bot.preferredRange (+- rangeSlack) and fires when settled
// (bot.aimTolerance) in the gun's range; out of sight it walks straight at the target, hopping at walls. The nav override
// (BotBrain.SetOverride's role) walks straight to the goal. No reaction delay, no EVADE / RECOVER, no graph.
// Allocation-free per step. No UnityEngine use.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class AutopilotStandInBrain : IBotBrain
    {
        const double Deg = Math.PI / 180;

        readonly BotSelf self;
        readonly BotDeps deps;
        readonly AutopilotGuns guns;
        readonly BotLook look = new BotLook();
        readonly Dictionary<string, double> range = new Dictionary<string, double>();
        readonly Dictionary<string, double> preferred = new Dictionary<string, double>();
        readonly Dictionary<string, double> aimErrorMult = new Dictionary<string, double>();
        readonly double eyeHeight, chestHeight, turnRate, aimTolerance, losInterval, rangeSlack, aimErrorBase, aimErrorPerSpeed, errorReroll;
        readonly double approachHopSpeed, revolverRange, revolverPreferred, losSlack;
        string difficulty = "normal";
        double errMult = 1;
        bool visible;
        double losTimer, errTimer, errYaw, errPitch;
        int weaponWant;
        bool hasGoal;
        Vec3 goal;

        public AutopilotStandInBrain(GameConfig cfg, BotSelf self, BotDeps deps, AutopilotGuns guns)
        {
            this.self = self;
            this.deps = deps;
            this.guns = guns;
            eyeHeight = cfg.Num("bot.eyeHeight");
            chestHeight = cfg.Num("bot.chestHeight");
            turnRate = cfg.Num("bot.turnRate");
            aimTolerance = cfg.Num("bot.aimTolerance");
            losInterval = cfg.Num("bot.losInterval");
            rangeSlack = cfg.Num("bot.rangeSlack");
            aimErrorBase = cfg.Num("bot.aimErrorBase");
            aimErrorPerSpeed = cfg.Num("bot.aimErrorPerSpeed");
            errorReroll = cfg.Num("bot.errorReroll");
            approachHopSpeed = cfg.Num("bot.approachHopSpeed");
            losSlack = cfg.Num("weapons.losSlack");
            foreach (var kv in cfg.Obj("bot.aimErrorMult")) aimErrorMult[kv.Key] = PortData.Num(kv.Value);
            foreach (var kv in cfg.Obj("bot.preferredRange")) preferred[kv.Key] = PortData.Num(kv.Value);
            revolverRange = cfg.Num("weapons.revolver.range");
            revolverPreferred = preferred.TryGetValue("revolver", out double rp) ? rp : 0;
            foreach (var kv in cfg.Obj("weapons.catalog"))
                range[kv.Key] = cfg.Has($"weapons.{kv.Key}.range") ? cfg.Num($"weapons.{kv.Key}.range") : revolverRange; // botBrain.js: ?? config.weapons.revolver.range
        }

        public string Difficulty => difficulty;
        public string State => hasGoal ? "nav" : visible ? "engage" : "chase";
        public BotLook Look => look;
        public bool Visible => visible;

        public void Reset(string newDifficulty)
        {
            difficulty = newDifficulty;
            errMult = aimErrorMult.TryGetValue(difficulty, out double m) ? m : 1;
            visible = false;
            losTimer = errTimer = errYaw = errPitch = 0;
            weaponWant = 0;
            hasGoal = false;
        }

        public void OnPlayerFire(Vec3 origin, Vec3 dir)
        {
        }

        public void ClearPath()
        {
        }

        /// <summary>BotBrain.SetOverride's role: walk straight to `at` (null: back to the target).</summary>
        public void SetOverride(Vec3? at)
        {
            hasGoal = at.HasValue;
            if (at.HasValue) goal = at.Value;
        }

        static double Wrap(double a) => JsMath.Atan2(JsMath.Sin(a), JsMath.Cos(a));

        bool Clear(Vec3 a, Vec3 b)
        {
            double dx = b.x - a.x, dy = b.y - a.y, dz = b.z - a.z;
            double length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (length <= 0) return true;
            return !deps.ray(a, new Vec3(dx / length, dy / length, dz / length), out RayHit hit) || hit.distance >= length - losSlack;
        }

        public void Step(ref BotFrame frame, double dt)
        {
            Mover mover = self.mover;
            Vec3 p = mover.position;
            if (hasGoal)
            {
                double gx = goal.x - p.x, gz = goal.z - p.z;
                double gd = Math.Sqrt(gx * gx + gz * gz);
                if (gd > rangeSlack)
                {
                    look.yaw = JsMath.Atan2(-gx, -gz);
                    frame.move.moveZ = 1;
                    if (mover.grounded && (mover.touchingWall || mover.Speed < approachHopSpeed)) frame.move.jump.pressed = frame.move.jump.held = true;
                    else if (!mover.grounded) frame.move.jump.held = true;
                }
                look.pitch = 0;
                frame.move.yaw = look.yaw;
                frame.move.pitch = 0;
                return;
            }
            BotTarget t = deps.getPlayer();
            var eye = new Vec3(p.x, p.y + eyeHeight, p.z);
            var aim = new Vec3(t.position.x, t.position.y + chestHeight, t.position.z);
            losTimer -= dt;
            if (losTimer <= 0)
            {
                losTimer = losInterval;
                visible = !t.dead && Clear(eye, aim);
            }
            errTimer -= dt;
            if (errTimer <= 0)
            {
                errTimer = errorReroll;
                double speed = JsMath.Hypot(t.velocity.x, t.velocity.z);
                double errDeg = (aimErrorBase + aimErrorPerSpeed * speed) * errMult;
                double r = errDeg * Deg * Math.Sqrt(deps.rng.Next());
                double a = deps.rng.Next() * Math.PI * 2;
                errYaw = r * JsMath.Cos(a);
                errPitch = r * JsMath.Sin(a);
            }
            double hx = aim.x - eye.x, hz = aim.z - eye.z;
            double horiz = JsMath.Hypot(hx, hz);
            double distance = JsMath.Hypot(horiz, aim.y - eye.y);
            double wantYaw = JsMath.Atan2(-hx, -hz) + errYaw;
            double wantPitch = JsMath.Atan2(aim.y - eye.y, horiz) + errPitch;
            double turn = turnRate * Deg * dt;
            double dYaw = Wrap(wantYaw - look.yaw);
            look.yaw = Wrap(look.yaw + Math.Max(-turn, Math.Min(turn, dYaw)));
            look.pitch += Math.Max(-turn, Math.Min(turn, wantPitch - look.pitch));
            weaponWant = guns.Choose(distance, weaponWant);
            frame.weapon = weaponWant;
            string w = self.weapon();
            bool settled = Math.Abs(Wrap(wantYaw - look.yaw)) < aimTolerance * Deg && Math.Abs(wantPitch - look.pitch) < aimTolerance * Deg;
            double gunRange = w != null && range.TryGetValue(w, out double gr) ? gr : revolverRange;
            if (visible && settled && distance < gunRange && !t.dead)
            {
                frame.fire.held = true;
                frame.fire.pressed = true;
            }
            frame.move.yaw = look.yaw;
            frame.move.pitch = look.pitch;
            if (visible)
            {
                double pref = w != null && preferred.TryGetValue(w, out double pr) ? pr : revolverPreferred;
                frame.move.moveZ = distance > pref + rangeSlack ? 1 : distance < pref - rangeSlack ? -1 : 0;
                if (!mover.grounded) frame.move.jump.held = true;
            }
            else
            {
                // out of sight: straight at it, looking along the way, hopping over what is in the way
                double moveYaw = JsMath.Atan2(-(t.position.x - p.x), -(t.position.z - p.z));
                double rel = moveYaw - look.yaw;
                frame.move.moveZ = JsMath.Cos(rel);
                frame.move.moveX = -JsMath.Sin(rel);
                if (mover.grounded && (mover.touchingWall || mover.Speed < approachHopSpeed)) frame.move.jump.pressed = frame.move.jump.held = true;
                else if (!mover.grounded) frame.move.jump.held = true;
            }
        }
    }
}
