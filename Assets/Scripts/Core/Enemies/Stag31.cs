// NIGHT31 builder B (design.n31.stag31; DESIGN31 C2 + the reviewer's R1 / R7; GG 2026-10-10 on the level-2 boss: "boring, you can camp"). THE STAG, LEVEL 2's boss in place of the Glacier:
// a blocky ice ram (boxes, the web's style, as THE LANTERN and THE LOOM) on the glide-ice rink (Rink28Arena, the NIGHT30 Glacier rink with the banks lowered to low ramps).
//   STILL / FAR   a player still for stillSeconds, or farther than farMetres (or chargeEverySeconds anyway): the CHARGE: a tellSeconds tell (head down, the red-orange line on the ice,
//                 locked at the tell's start), then a straight run at chargeSpeed that does NOT stop at walls (R1): a wall SLAM stuns it wallStunSeconds with its back open; a run that
//                 meets no wall in chargeMaxMetres skids (a MISS) and turns its OPEN BACK to the player for openSeconds. Where it stops: a stompRadius STOMP (wall or not).
//   DAMAGE        a gauntlet punch from behind on an open back (open / stunned) lands in full; every other hit lands gunScale. Two cyan ANTLER CRYSTALS: a hit of crystalMinDamage or
//                 more (a charged KESTREL shot) shatters one: a crystalStunSeconds stun (back open); it regrows after crystalRegrowSeconds (R7: 15 s).
//   SHARDS        a far, still or off-floor player meets a led shard volley every shardEverySeconds (so no spot is safe: C2 "every spot ice or in shard range").
//   HALF          at halfAt the banks melt to open water (design.n30.lava's rules: damageFraction, knockUp, graceSeconds) and charges come in pairs.
// Fixed 120 Hz, whole steps, no randomness (none needed), JsMath, no audio, no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.n31.stag31 (every number Unity-only; each has its _why).</summary>
    public sealed class Stag31Params
    {
        public string name = "THE STAG";
        public double pool, introHold, dieTime, halfW, halfL, halfH, walkSpeed, turnRate, stillSpeed, stillSeconds, farMetres, chargeFirstSeconds, chargeEverySeconds, chargeGapSeconds;
        public double tellSeconds, chargeSpeed, chargeMaxMetres, chargeHalfWidth, chargeDamageFrac, chargeKnock, chargeKnockUp, skidSeconds, openSeconds, wallStunSeconds;
        public double stompRadius, stompDamageFrac, stompKnock, stompKnockUp, gunScale, crystalRadius, crystalUp, crystalSide, crystalMinDamage, crystalStunSeconds, crystalRegrowSeconds;
        public double shardFarMetres, shardEverySeconds, shardSpread, shardSpeed, shardDamage, shardRadius, shardLife, halfAt, pairGapSeconds;
        public int shardCount;
        public double lavaFraction, lavaKnockUp, lavaGrace;
        public double headForward, headHeight, headHalf;

        public static Stag31Params From(GameConfig c)
        {
            const string g = "design.n31.stag31.";
            return new Stag31Params
            {
                name = c.Str(g + "name"), pool = c.Num(g + "pool"), introHold = c.Num(g + "introHold"), dieTime = c.Num(g + "dieTime"),
                halfW = c.Num(g + "halfW"), halfL = c.Num(g + "halfL"), halfH = c.Num(g + "halfH"), walkSpeed = c.Num(g + "walkSpeed"), turnRate = c.Num(g + "turnRate"),
                stillSpeed = c.Num(g + "stillSpeed"), stillSeconds = c.Num(g + "stillSeconds"), farMetres = c.Num(g + "farMetres"),
                chargeFirstSeconds = c.Num(g + "chargeFirstSeconds"), chargeEverySeconds = c.Num(g + "chargeEverySeconds"), chargeGapSeconds = c.Num(g + "chargeGapSeconds"),
                tellSeconds = c.Num(g + "tellSeconds"), chargeSpeed = c.Num(g + "chargeSpeed"), chargeMaxMetres = c.Num(g + "chargeMaxMetres"), chargeHalfWidth = c.Num(g + "chargeHalfWidth"),
                chargeDamageFrac = c.Num(g + "chargeDamageFrac"), chargeKnock = c.Num(g + "chargeKnock"), chargeKnockUp = c.Num(g + "chargeKnockUp"),
                skidSeconds = c.Num(g + "skidSeconds"), openSeconds = c.Num(g + "openSeconds"), wallStunSeconds = c.Num(g + "wallStunSeconds"),
                stompRadius = c.Num(g + "stompRadius"), stompDamageFrac = c.Num(g + "stompDamageFrac"), stompKnock = c.Num(g + "stompKnock"), stompKnockUp = c.Num(g + "stompKnockUp"),
                gunScale = c.Num(g + "gunScale"), crystalRadius = c.Num(g + "crystalRadius"), crystalUp = c.Num(g + "crystalUp"), crystalSide = c.Num(g + "crystalSide"),
                crystalMinDamage = c.Num(g + "crystalMinDamage"), crystalStunSeconds = c.Num(g + "crystalStunSeconds"), crystalRegrowSeconds = c.Num(g + "crystalRegrowSeconds"),
                shardFarMetres = c.Num(g + "shardFarMetres"), shardEverySeconds = c.Num(g + "shardEverySeconds"), shardCount = (int)c.Num(g + "shardCount"), shardSpread = c.Num(g + "shardSpread"),
                shardSpeed = c.Num(g + "shardSpeed"), shardDamage = c.Num(g + "shardDamage"), shardRadius = c.Num(g + "shardRadius"), shardLife = c.Num(g + "shardLife"),
                halfAt = c.Num(g + "halfAt"), pairGapSeconds = c.Num(g + "pairGapSeconds"),
                headForward = c.Num(g + "headForward"), headHeight = c.Num(g + "headHeight"), headHalf = c.Num(g + "headHalf"),
                lavaFraction = c.Num("design.n30.lava.damageFraction"), lavaKnockUp = c.Num("design.n30.lava.knockUp"), lavaGrace = c.Num("design.n30.lava.graceSeconds"),
            };
        }

        /// <summary>NIGHT32 B: a copy (NIGHT31's values kept while design.n32.bosses32 overrides some).</summary>
        public Stag31Params Clone32() => (Stag31Params)MemberwiseClone();
    }

    /// <summary>The payload of "stag31": stage tell / charge / slam / miss / stomp / open / stun / crystal / regrow / melt / hit (the charge hit the player) / back (a full back punch).</summary>
    public sealed class Stag31Event
    {
        public string stage;
        public Vec3 at, dir;
        public double seconds;
        public int crystal = -1;
    }

    public sealed class Stag31Shard
    {
        public Vec3 pos, vel;
        public int life;
        public bool alive;
    }

    public sealed class Stag31Body : Enemy, IBossBody27
    {
        public readonly Stag31 system;
        public Stag31Body(Stag31 system) { this.system = system; kind = "stag"; }
        public override double Height => system.p.halfH * 2;
        public override double Radius => system.p.halfL;
        public override double AimHeight => system.p.halfH;
        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            double t = system.Raycast(o, d);
            headshot = t >= 0 && system.hitCrystal >= 0;
            return t;
        }
        public override bool Damage(double amount, double flashTime) { system.Apply(amount); return false; }
        public double Filter(double amount, string weapon, Vec3 from, out bool blocked) => system.Filter(amount, weapon, from, out blocked);
        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    public sealed class Stag31 : IBossSystem
    {
        public readonly Stag31Params p;
        public readonly Stag31Body body;
        public readonly Rink28Arena rink;
        readonly EventBus events;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        readonly Stag31Event ev = new Stag31Event();
        BossKit kit;
        BossScale scale = BossScale.One;

        /// <summary>The sim hands these in: the player's velocity and whether he stands on the ground (the stomp and the water).</summary>
        public Func<Vec3> playerVelocity;
        public Func<bool> playerGrounded;
        /// <summary>A full back punch landed (the sim refills one gauntlet pip: C2.4).</summary>
        public Action backPunched;

        public string phase = "dead";
        /// <summary>walk / tell / charge / skid / open / stun (inside p1).</summary>
        public string act = "walk";
        public int stepNo, phaseSteps, actSteps;
        int introSteps, dieSteps, chargeTimer, everyTimer, shardTimer, lavaGrace, pairLeft, actLen;
        double stillTime, run;
        bool chargeHit, slammed;
        public WyrmBox box;
        public Vec3 pos, facing = new Vec3(0, 0, -1), chargeDir;
        public readonly bool[] crystalUp = { true, true };
        readonly int[] regrow = new int[2];
        public int hitCrystal = -1;
        string lastPart;
        public bool melted;
        public readonly List<Stag31Shard> shards = new List<Stag31Shard>();
        // the harness's counters (this fight)
        public int charges, chargeHits, slams, misses, stomps, stompHits, backPunches, crystalBreaks, shardVolleys, shardHits, lavaHits, pairs, parryHits, gauntletHits, punchBackShut, punchNotBehind;
        /// <summary>Builder W's weapon id for a parried projectile sent back at its shooter: it always lands in full on every NIGHT31 boss (the lead's decision, 12:18).</summary>
        public const string ParryWeapon = "parry31";

        /// <summary>NIGHT32 B (design.n32.bosses32.stag; GG 2026-10-10 "you can barely hit them"): every hit lands chip32 of itself; any hit from behind on an open back, and a
        /// charged KESTREL shot on a crystal, lands open32 (x2.5 over the chip); the windows (open, wall stun, crystal stun) last the row's seconds (at least the PULSE's cooldown
        /// + 0.75 s) and the crystals regrow sooner. full32 / chip32Hits: the hits by share since the system was made (the harness's; never reset).</summary>
        public bool on32;
        public double chip32 = 0.4, open32 = 1, hitScale32 = 1;
        public int full32, chip32Hits;
        Stag31Params orig32;

        /// <summary>NIGHT32 B: the row on (its numbers) or off (NIGHT31's).</summary>
        public void Set32(GameConfig c, bool on)
        {
            orig32 ??= p.Clone32();
            on32 = on;
            chip32 = Boss32Cfg.Num(c, "stag", "chip", 0.4);
            open32 = Boss32Cfg.Num(c, "stag", "open", 1);
            hitScale32 = Boss32Cfg.Num(c, "stag", "hitScale", 1);
            p.openSeconds = on ? Boss32Cfg.Num(c, "stag", "openSeconds", orig32.openSeconds) : orig32.openSeconds;
            p.wallStunSeconds = on ? Boss32Cfg.Num(c, "stag", "wallStunSeconds", orig32.wallStunSeconds) : orig32.wallStunSeconds;
            p.crystalStunSeconds = on ? Boss32Cfg.Num(c, "stag", "crystalStunSeconds", orig32.crystalStunSeconds) : orig32.crystalStunSeconds;
            p.crystalRegrowSeconds = on ? Boss32Cfg.Num(c, "stag", "crystalRegrowSeconds", orig32.crystalRegrowSeconds) : orig32.crystalRegrowSeconds;
            // the anti-camp (the lead 20:13: punish stillness, not tankier bosses): a still player meets the charge and the shards sooner, and the shards hit harder
            p.stillSeconds = on ? Boss32Cfg.Num(c, "stag", "stillSeconds", orig32.stillSeconds) : orig32.stillSeconds;
            p.stillSpeed = on ? Boss32Cfg.Num(c, "stag", "stillSpeed", orig32.stillSpeed) : orig32.stillSpeed;
            p.shardEverySeconds = on ? Boss32Cfg.Num(c, "stag", "shardEverySeconds", orig32.shardEverySeconds) : orig32.shardEverySeconds;
            p.shardDamage = on ? Boss32Cfg.Num(c, "stag", "shardDamage", orig32.shardDamage) : orig32.shardDamage;
        }

        double Filter32(double amount, string weapon, Vec3 from, int k)
        {
            if (weapon == ParryWeapon) { parryHits++; full32++; lastPart = "body"; return amount * open32; }
            if (k >= 0 && crystalUp[k])
            {
                bool charged = (weapon != null && weapon.IndexOf("charge", StringComparison.Ordinal) >= 0) || (weapon == "revolver" && amount >= p.crystalMinDamage);
                lastPart = "crystal";
                if (!charged) { chip32Hits++; return amount * chip32; }
                crystalUp[k] = false;
                regrow[k] = S(p.crystalRegrowSeconds);
                crystalBreaks++;
                pairLeft = 0;
                SetAct("stun", S(p.crystalStunSeconds));
                Say("crystal", p.crystalStunSeconds, k);
                full32++;
                return amount * open32; // a charged crystal hit lands as the bonus (A1: it was the 5 % chip)
            }
            if (weapon == "gauntlet") { gauntletHits++; if (!BackOpen) punchBackShut++; else if (!Behind(from)) punchNotBehind++; }
            if (BackOpen && Behind(from))
            {
                if (weapon == "gauntlet") { backPunches++; backPunched?.Invoke(); Say("back", 0); }
                lastPart = "back";
                full32++;
                return amount * open32;
            }
            lastPart = "body";
            chip32Hits++;
            return amount * chip32;
        }

        public Stag31(GameConfig cfg, Stag31Params p, Rink28Arena rink, EventBus events, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.rink = rink;
            this.events = events;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new Stag31Body(this);
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public bool Hurtable => body.alive && phase == "p1";
        /// <summary>The back takes a full gauntlet punch now (the view glows it cyan).</summary>
        public bool BackOpen => Hurtable && (act == "open" || act == "stun");
        public bool Telling => phase == "p1" && act == "tell";
        public double TellProgress => Telling ? Math.Min(1, (double)actSteps / Math.Max(1, actLen)) : 0;
        public bool Charging => phase == "p1" && act == "charge";
        public bool Half => HpFrac <= p.halfAt;
        /// <summary>The charge line's end (where a run from here along chargeDir meets a wall, or chargeMaxMetres): the view's red-orange line.</summary>
        public Vec3 LineEnd { get; private set; }
        public Vec3 Centre => box.c;

        /// <summary>Where the crystal k sits (world).</summary>
        public Vec3 CrystalAt(int k)
        {
            Vec3 right = new Vec3(-facing.z, 0, facing.x), h = HeadAt;
            double side = k == 0 ? -p.crystalSide : p.crystalSide;
            return new Vec3(h.x + right.x * side, h.y + p.headHalf + p.crystalUp, h.z + right.z * side);
        }

        /// <summary>The head's centre (world): headForward in front of the centre, headHeight over the ice (lower than the shoulders); the tell drops it by headHalf.</summary>
        public Vec3 HeadAt => new Vec3(pos.x + facing.x * p.headForward, FloorY(pos) + p.headHeight - (Telling || Charging ? p.headHalf : 0), pos.z + facing.z * p.headForward);

        double FloorY(Vec3 at) => rink.SurfaceYAt(at.x, at.z);

        /// <summary>A floor point the Stag can stand on: the lane or a bank (not the island, the lip, the rim or the door).</summary>
        public bool Walkable(double x, double z)
        {
            Rink28Region r = rink.RegionAt(x, z);
            return r == Rink28Region.Lane || r == Rink28Region.Bank;
        }

        void Pose()
        {
            var b = default(WyrmBox);
            b.Orient(facing);
            b.c = new Vec3(pos.x, FloorY(pos) + p.halfH, pos.z);
            b.hr = p.halfW;
            b.hf = p.halfL;
            b.hu = p.halfH;
            box = b;
            body.position = b.c;
        }

        /// <summary>Put it on the rink at `at` facing `dir` (the fight's start).</summary>
        public void Place(Vec3 at, Vec3 dir)
        {
            pos = at;
            facing = Flat(dir);
            Pose();
        }

        static Vec3 Flat(Vec3 v)
        {
            double l = JsMath.Hypot(v.x, v.z);
            return l < 1e-9 ? new Vec3(0, 0, -1) : new Vec3(v.x / l, 0, v.z / l);
        }

        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp);
            body.hp = body.maxHp;
            stepNo = 0;
            charges = chargeHits = slams = misses = stomps = stompHits = backPunches = crystalBreaks = shardVolleys = shardHits = lavaHits = pairs = parryHits = gauntletHits = punchBackShut = punchNotBehind = 0;
            crystalUp[0] = crystalUp[1] = true;
            regrow[0] = regrow[1] = 0;
            melted = false;
            shards.Clear();
            stillTime = 0;
            pairLeft = 0;
            lavaGrace = 0;
            chargeTimer = S(p.chargeFirstSeconds);
            everyTimer = S(p.chargeEverySeconds);
            shardTimer = S(p.shardEverySeconds);
            introSteps = Math.Max(kit != null ? kit.IntroSteps() : 0, S(p.introHold));
            SetAct("walk", 0);
            Pose();
            body.previous = body.position;
            SetPhase("intro");
            said.Reset("stag");
            said.name = "STAG";
            said.hp = body.maxHp;
            events.Emit("bossRise", said);
        }

        /// <summary>A retry (the player died in the room): the fight goes on (its HP kept), the charge clock starts over.</summary>
        public void Retry()
        {
            SetAct("walk", 0);
            chargeTimer = S(p.chargeFirstSeconds);
            everyTimer = S(p.chargeEverySeconds);
            shards.Clear();
            stillTime = 0;
            pairLeft = 0;
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            shards.Clear();
        }

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            said.Reset("stag");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        void SetAct(string a, int len)
        {
            act = a;
            actSteps = 0;
            actLen = len;
        }

        void Say(string stage, double sec, int crystal = -1)
        {
            ev.stage = stage;
            ev.at = box.c;
            ev.dir = act == "charge" || act == "tell" ? chargeDir : facing;
            ev.seconds = sec;
            ev.crystal = crystal;
            events.Emit("stag31", ev);
        }

        public BossBar Bar() => new BossBar { valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = 0 };

        /// <summary>The nearest intact crystal or the body along a unit ray (-1: a miss); hitCrystal names the crystal (-1: the body).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            // NIGHT32 B (design.n32.bosses32.stag.hitScale; DESIGN32 B "hurtboxes at least 1.3x the model"): the shot's box, head and crystals scaled (the drawn torso is the box's
            // own width and length, Boss31View.cs:375); the body's walking and charging box is unchanged
            double hs = on32 && hitScale32 > 0 ? hitScale32 : 1;
            WyrmBox hb = box;
            if (hs != 1) { hb.hr *= hs; hb.hu *= hs; hb.hf *= hs; }
            double best = hb.Ray(o, d);
            double th = WyrmMath.RaySphere(o, d, HeadAt, p.headHalf * 1.3 * hs); // the head (drawn in front of the body box) takes hits as the body
            if (th >= 0 && (best < 0 || th < best)) best = th;
            int k = -1;
            for (int i = 0; i < 2; i++)
            {
                if (!crystalUp[i]) continue;
                double t = WyrmMath.RaySphere(o, d, CrystalAt(i), p.crystalRadius * hs);
                if (t >= 0 && (best < 0 || t < best)) { best = t; k = i; }
            }
            hitCrystal = best >= 0 ? k : -1;
            return best;
        }

        /// <summary>Is `from` behind it (the attacker in the rear half, past the back's plane)?</summary>
        public bool Behind(Vec3 from)
        {
            double dx = from.x - box.c.x, dz = from.z - box.c.z;
            return dx * facing.x + dz * facing.z < -p.halfL * 0.5;
        }

        public double Filter(double amount, string weapon, Vec3 from, out bool blocked)
        {
            blocked = false;
            int k = hitCrystal;
            hitCrystal = -1;
            if (!Hurtable) { blocked = true; return 0; }
            if (on32) return Filter32(amount, weapon, from, k); // NIGHT32 B (design.n32.bosses32)
            if (weapon == ParryWeapon) { parryHits++; lastPart = "body"; return amount; } // the lead (12:18): a parried projectile ALWAYS hurts its boss, in full, outside every window (W's weapon id)
            if (k >= 0 && crystalUp[k])
            {
                bool charged = (weapon != null && weapon.IndexOf("charge", StringComparison.Ordinal) >= 0) || (weapon == "revolver" && amount >= p.crystalMinDamage); // the KESTREL only (a PULSE punch is 192: it must not shatter one)
                if (charged)
                {
                    crystalUp[k] = false;
                    regrow[k] = S(p.crystalRegrowSeconds);
                    crystalBreaks++;
                    pairLeft = 0;
                    SetAct("stun", S(p.crystalStunSeconds));
                    Say("crystal", p.crystalStunSeconds, k);
                }
                lastPart = "crystal";
                return amount * p.gunScale;
            }
            if (weapon == "gauntlet") { gauntletHits++; if (!BackOpen) punchBackShut++; else if (!Behind(from)) punchNotBehind++; } // the back test's reasons (the lead's 12:40 ask)
            if (weapon == "gauntlet" && BackOpen && Behind(from))
            {
                backPunches++;
                lastPart = "back";
                backPunched?.Invoke();
                Say("back", 0);
                return amount;
            }
            lastPart = "body";
            return amount * p.gunScale;
        }

        public void Apply(double dmg)
        {
            if (!body.alive) return;
            body.hp -= dmg;
            body.flash = 0.06;
            said.Reset("stag");
            said.damage = JsMath.Round(dmg);
            said.part = lastPart ?? "body";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (!melted && HpFrac <= p.halfAt && body.hp > 0) { melted = true; Say("melt", 0); }
            if (body.hp <= 0 && phase != "dying")
            {
                body.hp = 0;
                SetPhase("dying");
                dieSteps = S(p.dieTime);
                shards.Clear();
                kit?.Dying("stag", body.position);
            }
        }

        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            stepNo++;
            phaseSteps++;
            if (phase == "intro" && phaseSteps >= introSteps) SetPhase("p1");
            if (phase == "p1") StepFight(dt);
            StepShards(dt);
            Pose();
            if (phase == "dying" && --dieSteps <= 0)
            {
                body.alive = false;
                phase = "dead";
                kit?.Down("stag", body.id, body.position);
            }
        }

        void StepFight(double dt)
        {
            actSteps++;
            Vec3 me = target.position();
            Vec3 v = playerVelocity != null ? playerVelocity() : default;
            double flatSpeed = JsMath.Hypot(v.x, v.z);
            stillTime = flatSpeed < p.stillSpeed ? stillTime + dt : 0;
            double dist = JsMath.Hypot(me.x - pos.x, me.z - pos.z);
            for (int k = 0; k < 2; k++) if (!crystalUp[k] && regrow[k] > 0 && --regrow[k] == 0) { crystalUp[k] = true; Say("regrow", 0, k); }
            if (lavaGrace > 0) lavaGrace--;
            // the melted banks: open water (design.n30.lava's rules) on every bank once the bar is under halfAt
            if (melted && lavaGrace == 0 && rink.RegionAt(me.x, me.z) == Rink28Region.Bank && me.y <= rink.SurfaceYAt(me.x, me.z) + 0.15)
            {
                Health h = target.health;
                if (h != null && !h.Dead && h.Damage(h.max * p.lavaFraction, "boss")) lavaHits++;
                target.impulse?.Invoke(0, p.lavaKnockUp, 0);
                lavaGrace = S(p.lavaGrace);
            }
            switch (act)
            {
                case "walk":
                {
                    Vec3 to = Flat(new Vec3(me.x - pos.x, 0, me.z - pos.z));
                    Turn(to, p.turnRate * dt);
                    if (dist > p.halfL + 1.5) Move(facing, p.walkSpeed * dt);
                    if (chargeTimer > 0) chargeTimer--;
                    if (everyTimer > 0) everyTimer--;
                    bool want = pairLeft > 0 || stillTime >= p.stillSeconds || dist > p.farMetres || everyTimer == 0;
                    if (chargeTimer == 0 && want) StartTell(me);
                    break;
                }
                case "tell":
                    if (actSteps >= actLen)
                    {
                        SetAct("charge", 0);
                        run = 0;
                        chargeHit = slammed = false;
                        charges++;
                        Say("charge", 0);
                    }
                    break;
                case "charge":
                    StepCharge(dt, me);
                    break;
                case "skid":
                {
                    double f = 1.0 - (double)actSteps / Math.Max(1, actLen);
                    Move(chargeDir, p.chargeSpeed * 0.35 * f * dt);
                    if (actSteps >= actLen) Stop(false, me);
                    break;
                }
                case "open":
                case "stun":
                    if (actSteps >= actLen) Recover();
                    break;
            }
            // shards: a far, still or off-floor player (C2: no safe spot), never during the charge itself
            if (shardTimer > 0) shardTimer--;
            bool offFloor = !Walkable(me.x, me.z) || me.y > FloorY(me) + 1.2;
            if (shardTimer == 0 && act != "charge" && (dist > p.shardFarMetres || offFloor || stillTime >= p.stillSeconds))
            {
                Volley(me, v);
                shardTimer = S(p.shardEverySeconds);
            }
        }

        void StartTell(Vec3 me)
        {
            chargeDir = Flat(new Vec3(me.x - pos.x, 0, me.z - pos.z));
            facing = chargeDir;
            // the line: from here along the run to the first wall (or the max run)
            double len = 0, stepL = 0.25;
            while (len < p.chargeMaxMetres && Walkable(pos.x + chargeDir.x * (len + stepL + p.halfL), pos.z + chargeDir.z * (len + stepL + p.halfL))) len += stepL;
            LineEnd = new Vec3(pos.x + chargeDir.x * len, FloorY(pos), pos.z + chargeDir.z * len);
            SetAct("tell", Math.Max(1, S(pairLeft > 0 ? Math.Min(p.tellSeconds, p.tellSeconds) : p.tellSeconds)));
            if (pairLeft > 0) { pairLeft--; pairs++; }
            else if (Half) pairLeft = 1;
            stillTime = 0;
            everyTimer = S(p.chargeEverySeconds);
            Say("tell", p.tellSeconds);
            said.Reset("stag");
            said.name = "charge";
            said.seconds = p.tellSeconds;
            events.Emit("bossTell", said);
        }

        void StepCharge(double dt, Vec3 me)
        {
            double stepL = p.chargeSpeed * dt;
            Vec3 nose = new Vec3(pos.x + chargeDir.x * (p.halfL + stepL), 0, pos.z + chargeDir.z * (p.halfL + stepL));
            if (!Walkable(nose.x, nose.z)) { slammed = true; Stop(true, me); return; }
            pos = new Vec3(pos.x + chargeDir.x * stepL, pos.y, pos.z + chargeDir.z * stepL);
            run += stepL;
            // the hit: the player inside the ram's width ahead of its centre, his feet under its back
            if (!chargeHit)
            {
                double dx = me.x - pos.x, dz = me.z - pos.z;
                double along = dx * chargeDir.x + dz * chargeDir.z, lat = Math.Abs(-dx * chargeDir.z + dz * chargeDir.x);
                if (along > -p.halfL && along < p.halfL + 0.5 && lat <= p.chargeHalfWidth && me.y <= FloorY(pos) + p.halfH * 2)
                {
                    chargeHit = true;
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(h.max * p.chargeDamageFrac * scale.damage, "boss")) chargeHits++;
                    target.impulse?.Invoke(chargeDir.x * p.chargeKnock, p.chargeKnockUp, chargeDir.z * p.chargeKnock);
                    Say("hit", 0);
                }
            }
            if (run >= p.chargeMaxMetres) { SetAct("skid", Math.Max(1, S(p.skidSeconds))); }
        }

        /// <summary>The run is over: a wall SLAM (stunned, back open) or the end of a skid (a hit: walk on; a miss: the open back turned to the player). The stomp either way.</summary>
        void Stop(bool wall, Vec3 me)
        {
            stomps++;
            double dx = me.x - pos.x, dz = me.z - pos.z, d = JsMath.Hypot(dx, dz);
            Say("stomp", 0);
            if (d <= p.stompRadius && me.y <= FloorY(pos) + 1.5)
            {
                Health h = target.health;
                if (h != null && !h.Dead && h.Damage(h.max * p.stompDamageFrac * scale.damage, "boss")) stompHits++;
                if (d < 1e-6) { dx = -chargeDir.x; dz = -chargeDir.z; d = 1; }
                target.impulse?.Invoke(dx / d * p.stompKnock, p.stompKnockUp, dz / d * p.stompKnock);
            }
            if (wall)
            {
                slams++;
                pairLeft = 0;
                SetAct("stun", Math.Max(1, S(p.wallStunSeconds)));
                Say("slam", p.wallStunSeconds);
                return;
            }
            if (chargeHit) { Recover(); return; }
            misses++;
            if (pairLeft > 0) { SetAct("walk", 0); chargeTimer = S(p.pairGapSeconds); return; }
            // the miss: it turns its back to the player it charged (it faces away from him)
            facing = Flat(new Vec3(pos.x - me.x, 0, pos.z - me.z));
            SetAct("open", Math.Max(1, S(p.openSeconds)));
            Say("open", p.openSeconds);
        }

        void Recover()
        {
            SetAct("walk", 0);
            chargeTimer = pairLeft > 0 ? S(p.pairGapSeconds) : S(p.chargeGapSeconds);
        }

        void Turn(Vec3 to, double maxRad)
        {
            double a0 = JsMath.Atan2(facing.z, facing.x), a1 = JsMath.Atan2(to.z, to.x), da = a1 - a0;
            while (da > Math.PI) da -= 2 * Math.PI;
            while (da < -Math.PI) da += 2 * Math.PI;
            if (da > maxRad) da = maxRad;
            if (da < -maxRad) da = -maxRad;
            double a = a0 + da;
            facing = new Vec3(JsMath.Cos(a), 0, JsMath.Sin(a));
        }

        void Move(Vec3 dir, double len)
        {
            double nx = pos.x + dir.x * len, nz = pos.z + dir.z * len;
            if (Walkable(nx + dir.x * p.halfL, nz + dir.z * p.halfL)) pos = new Vec3(nx, pos.y, nz);
        }

        void Volley(Vec3 me, Vec3 v)
        {
            shardVolleys++;
            Vec3 from = HeadAt; // the shards leave the head (its mouth), so the red-orange shard reads as its attack
            Vec3 chest = new Vec3(me.x, me.y + 1.0, me.z);
            double tFly = Vec3.DistanceTo(from, chest) / Math.Max(1e-6, p.shardSpeed);
            Vec3 led = new Vec3(chest.x + v.x * tFly, chest.y, chest.z + v.z * tFly);
            Vec3 aim = Vec3.Normalize(led - from);
            Vec3 side = Flat(new Vec3(-aim.z, 0, aim.x));
            for (int i = 0; i < p.shardCount; i++)
            {
                double off = i == 0 ? 0 : (i % 2 == 1 ? 1 : -1) * p.shardSpread * ((i + 1) / 2);
                Vec3 to = led + side * off;
                Vec3 d = Vec3.Normalize(to - from);
                shards.Add(new Stag31Shard { pos = from, vel = d * p.shardSpeed, life = S(p.shardLife), alive = true });
            }
            Say("shards", 0);
        }

        void StepShards(double dt)
        {
            if (shards.Count == 0) return;
            Vec3 me = target.position();
            Vec3 chest = new Vec3(me.x, me.y + 1.0, me.z);
            for (int i = shards.Count - 1; i >= 0; i--)
            {
                Stag31Shard s = shards[i];
                s.pos = s.pos + s.vel * dt;
                if (--s.life <= 0 || !s.alive) { shards.RemoveAt(i); continue; }
                if (phase == "p1" && Vec3.DistanceTo(s.pos, chest) <= p.shardRadius)
                {
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(p.shardDamage * scale.damage, "boss")) shardHits++;
                    shards.RemoveAt(i);
                }
            }
        }
    }
}
