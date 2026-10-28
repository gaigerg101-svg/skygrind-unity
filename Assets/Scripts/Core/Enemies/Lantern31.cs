// NIGHT31 builder B (design.n31.lantern31; DESIGN31 C1): THE LANTERN's shield. Keeps NIGHT30's ring, lava floor, sweep, lead and platform lob (design.n30.lantern30). Adds:
//   SHIELD   the core takes a light share (Lantern29's lightDamage) of every hit until a CHARGED KESTREL shot (weapon revolver, at least chargedMinDamage: the charge is 4 x 25 = 100,
//            a plain shot 25 or 50 on a head part) hits it while the shooter WALL RIDES: the shield breaks (event lantern31 "break") and the core is OPEN for coreOpenSeconds:
//            every hit lands in full (none from a platform: NIGHT30's refuge rule). A plain KESTREL shot from the wall sparks (event "spark"): the tell that it needs the charge.
//   SINK     at sinkAt of the bar the three platforms (design.n30.lantern30.arena.platformPanels; builder L's f12.sink31) tell for sinkTellSeconds (a red-orange edge), then sink
//            from the platform top to topTo over sinkSeconds (the sim rebuilds the ring's layer at each SinkTop step); a new fight puts them back.
// Fixed 120 Hz, whole steps, no randomness, no audio, no UnityEngine.
using System;

namespace Skygrind.Core
{
    public sealed class Lantern31Params
    {
        public double rideGraceSeconds, lightMult, coreOpenSeconds, chargedMinDamage, sinkAt, sinkTellSeconds, sinkSeconds, sinkTo, sinkStepSeconds;

        public static Lantern31Params From(GameConfig c)
        {
            const string g = "design.n31.lantern31.";
            return new Lantern31Params
            {
                rideGraceSeconds = c.Num(g + "rideGraceSeconds"), lightMult = c.Num(g + "lightMult"), coreOpenSeconds = c.Num(g + "coreOpenSeconds"), chargedMinDamage = c.Num(g + "chargedMinDamage"), sinkAt = c.Num(g + "sinkAt"), sinkTellSeconds = c.Num(g + "sinkTellSeconds"),
                sinkSeconds = c.Num(g + "sinkSeconds"), sinkTo = c.Num(g + "sinkTo"), sinkStepSeconds = c.Num(g + "sinkStepSeconds"),
            };
        }

        Lantern31Params orig32;

        /// <summary>NIGHT32 B (design.n32.bosses32.lantern): the row's overrides on (the core open longer, a longer wall-ride grace), or NIGHT31's values back.</summary>
        public void Set32(GameConfig c, bool on)
        {
            orig32 ??= (Lantern31Params)MemberwiseClone();
            rideGraceSeconds = on ? Boss32Cfg.Num(c, "lantern", "rideGraceSeconds", orig32.rideGraceSeconds) : orig32.rideGraceSeconds;
            coreOpenSeconds = on ? Boss32Cfg.Num(c, "lantern", "coreOpenSeconds", orig32.coreOpenSeconds) : orig32.coreOpenSeconds;
        }
    }

    /// <summary>NIGHT32 B (design.n32.bosses32; DESIGN32 B): a boss override `design.n32.bosses32.&lt;boss&gt;.&lt;key&gt;` when present, else NIGHT31's value. Read only while
    /// the row is on (the sim's SurvivalSim.Boss32), so --off32 stays NIGHT31's game.</summary>
    public static class Boss32Cfg
    {
        public const string Root = "design.n32.bosses32.";

        public static double Num(GameConfig c, string boss, string key, double night31)
        {
            string k = Root + boss + "." + key;
            return c.Has(k) ? c.Num(k) : night31;
        }
    }

    /// <summary>The payload of "lantern31": stage break / spark / close / sinkTell / sink / sunk.</summary>
    public sealed class Lantern31Event
    {
        public string stage;
        public double seconds;
    }

    public sealed class Lantern31
    {
        public readonly Lantern31Params p;
        readonly EventBus events;
        readonly double dt0;
        readonly Lantern31Event ev = new Lantern31Event();
        int openLeft, sinkLeft, sparkGap;
        /// <summary>0 up, 1 the sink tell, 2 sinking, 3 sunk.</summary>
        public int sinkState;
        public double platformTop0;
        /// <summary>The platforms' top now (arena-local y; the sim rebuilds the layer when it changes by a step).</summary>
        public double SinkTop { get; private set; }
        public double SinkTellProgress => sinkState == 1 ? 1.0 - (double)sinkLeft / Math.Max(1, S(p.sinkTellSeconds)) : 0;
        public bool CoreOpen => openLeft > 0;
        public double CoreOpenProgress => openLeft > 0 ? 1.0 - (double)openLeft / Math.Max(1, S(p.coreOpenSeconds)) : 0;
        // the harness's counters (this fight)
        public int breaks, sparks, openHits, lightHits, chargedSeen, rejectNotWall, rejectPlatform;

        /// <summary>NIGHT32 B (design.n32.bosses32.lantern; GG 2026-10-10 "you can barely hit them"): every hit lands at least chip32 of itself (floor, air, ride), platform32 from a
        /// platform (NIGHT31: 0), open32 while the core is open (the charged break stays the bonus: open32 / chip32 = x2.5). finisherComp32 (the sim sets it each step) undoes
        /// Lantern27's finisher gun cut (level 1 has no PULSE) up to the row's finisherGun share.</summary>
        public bool on32;
        public double chip32 = 0.4, platform32 = 0.4, open32 = 1, finisherComp32 = 1;

        /// <summary>NIGHT32 B: the row on (its numbers) or off (NIGHT31's rule).</summary>
        public void Set32(GameConfig c, bool on)
        {
            p.Set32(c, on);
            on32 = on;
            chip32 = Boss32Cfg.Num(c, "lantern", "chip", 0.4);
            platform32 = Boss32Cfg.Num(c, "lantern", "platformChip", 0.4);
            open32 = Boss32Cfg.Num(c, "lantern", "open", 1);
            campEvery32 = on ? Boss32Cfg.Num(c, "lantern", "campEverySeconds", 0) : 0;
            campStill32 = Boss32Cfg.Num(c, "lantern", "campStillSeconds", 2);
            campSpeed32 = Boss32Cfg.Num(c, "lantern", "campStillSpeed", 1.5);
            campTell32 = Boss32Cfg.Num(c, "lantern", "campTellSeconds", 0.8);
            campRadius32 = Boss32Cfg.Num(c, "lantern", "campRadius", 2.5);
            campDamage32 = Boss32Cfg.Num(c, "lantern", "campDamage", 20);
            still32 = 0;
            campTellLeft = campCool = 0;
        }

        /// <summary>NIGHT32 B (design.n32.bosses32.lantern; the lead 20:13: punish stillness, not tankier bosses): a player slower than campSpeed for campStill seconds in the fight
        /// gets a CAMP STRIKE at his spot: a campTell tell (event lantern31 "campTell", where), then within campRadius campDamage (event "camp"); campEvery before the next.
        /// Off (campEvery 0) with the row off. The sim calls Camp32 each step.</summary>
        public double campStill32, campSpeed32, campTell32, campRadius32, campDamage32, campEvery32;
        /// <summary>NIGHT33 E (design.n33.bosses33.lantern.campThroughBreaks): the strike keeps running while THE LANTERN lives but cannot be hurt (its 66 % / 33 % breaks:
        /// with the cut pool a camper reached them sooner and the paused strike let him live 13-14 s instead of 9). The sim sets it each step; false = NIGHT32's rule.</summary>
        public bool breaks33;
        double still32;
        int campTellLeft, campCool;
        public Vec3 campAt32;
        public int campStrikes32, campHits32;
        public bool CampTelling32 => campTellLeft > 0;
        /// <summary>0..1 through the camp strike's tell (the view's ring).</summary>
        public double CampTellProgress32 => campTellLeft > 0 ? 1.0 - (double)campTellLeft / Math.Max(1, S(campTell32)) : 0;

        public void Camp32(Vec3 me, Vec3 vel, Health h, double dt, bool fighting)
        {
            if (campEvery32 <= 0 || !fighting && !breaks33) { still32 = 0; campTellLeft = 0; return; }
            if (campCool > 0) campCool--;
            if (campTellLeft > 0)
            {
                if (--campTellLeft == 0)
                {
                    campStrikes32++;
                    Say("camp", 0);
                    if (JsMath.Hypot(me.x - campAt32.x, me.z - campAt32.z) <= campRadius32 && Math.Abs(me.y - campAt32.y) <= 2.0 && h != null && !h.Dead && h.Damage(campDamage32, "boss")) campHits32++;
                    campCool = S(campEvery32);
                }
                return;
            }
            still32 = JsMath.Hypot(vel.x, vel.z) < campSpeed32 ? still32 + dt : 0;
            if (campCool == 0 && still32 >= campStill32)
            {
                campAt32 = me;
                campTellLeft = Math.Max(1, S(campTell32));
                still32 = 0;
                Say("campTell", campTell32);
            }
        }

        double Scale32(double amount, string weapon, bool riding, bool onPlatform)
        {
            double comp = weapon == "gauntlet" ? 1 : finisherComp32;
            if (weapon == Stag31.ParryWeapon) { openHits++; return open32 * comp; }
            bool chargedHit = weapon == "revolver" && amount >= p.chargedMinDamage;
            if (chargedHit) chargedSeen++;
            if (openLeft > 0) { openHits++; return open32 * comp; }
            if (riding && chargedHit)
            {
                breaks++;
                openLeft = S(p.coreOpenSeconds);
                Say("break", p.coreOpenSeconds);
                openHits++;
                return open32 * comp; // the breaking shot lands as the bonus
            }
            if (chargedHit) rejectNotWall++;
            if (riding && weapon == "revolver" && sparkGap == 0) { sparks++; Say("spark", 0); sparkGap = S(0.25); }
            lightHits++;
            return (onPlatform && sinkState < 2 ? platform32 : chip32) * comp;
        }

        public Lantern31(GameConfig cfg, Lantern31Params p, EventBus events, double platformTop)
        {
            this.p = p;
            this.events = events;
            dt0 = cfg.Dt;
            platformTop0 = platformTop;
            SinkTop = platformTop;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public void Reset()
        {
            openLeft = sinkLeft = sparkGap = 0;
            sinkState = 0;
            SinkTop = platformTop0;
            breaks = sparks = openHits = lightHits = chargedSeen = rejectNotWall = rejectPlatform = 0;
            still32 = 0; // NIGHT32 B: the anti-camp strike's state (a new fight)
            campTellLeft = campCool = 0;
        }

        /// <summary>Lantern27.scale31: the share of a hit of `amount` by `weapon` that lands. `riding`: the shooter wall rides; `onPlatform`: he stands on a platform;
        /// `light`: Lantern29's light share; `platformScale`: NIGHT30's platform share.</summary>
        public double Scale(double amount, string weapon, bool riding, bool onPlatform, double light, double platformScale)
        {
            if (on32) return Scale32(amount, weapon, riding, onPlatform); // NIGHT32 B (design.n32.bosses32)
            if (weapon == Stag31.ParryWeapon) { openHits++; return 1.0; } // the lead (12:18): a parried projectile always lands in full (platform, shield up or not)
            bool chargedHit = weapon == "revolver" && amount >= p.chargedMinDamage;
            if (chargedHit) chargedSeen++;
            if (onPlatform && sinkState < 2) { if (chargedHit && openLeft == 0) rejectPlatform++; lightHits++; return platformScale; }
            if (openLeft > 0) { openHits++; return 1.0; }
            if (chargedHit && !riding) rejectNotWall++; // the reason a charged KESTREL hit did not break the shield (the lead's 12:40 ask): not from the wall
            if (riding && weapon == "revolver")
            {
                if (amount >= p.chargedMinDamage)
                {
                    breaks++;
                    openLeft = S(p.coreOpenSeconds);
                    Say("break", p.coreOpenSeconds);
                    openHits++;
                    return 1.0; // the breaking shot lands in full
                }
                if (sparkGap == 0) { sparks++; Say("spark", 0); sparkGap = S(0.25); }
            }
            lightHits++;
            return light * p.lightMult;
        }

        /// <summary>One step; hpFrac the bar. True when SinkTop changed (the sim refills the ring's layer).</summary>
        public bool Step(double hpFrac, bool fighting)
        {
            if (sparkGap > 0) sparkGap--;
            if (openLeft > 0 && --openLeft == 0) Say("close", 0);
            if (!fighting) return false;
            if (sinkState == 0 && hpFrac <= p.sinkAt)
            {
                sinkState = 1;
                sinkLeft = Math.Max(1, S(p.sinkTellSeconds));
                Say("sinkTell", p.sinkTellSeconds);
                return false;
            }
            if (sinkState == 1 && --sinkLeft == 0)
            {
                sinkState = 2;
                sinkLeft = Math.Max(1, S(p.sinkSeconds));
                Say("sink", p.sinkSeconds);
                return false;
            }
            if (sinkState == 2)
            {
                sinkLeft--;
                int total = Math.Max(1, S(p.sinkSeconds)), stepN = Math.Max(1, S(p.sinkStepSeconds));
                int done = total - sinkLeft;
                if (sinkLeft <= 0)
                {
                    sinkState = 3;
                    SinkTop = p.sinkTo;
                    Say("sunk", 0);
                    return true;
                }
                if (done % stepN == 0)
                {
                    SinkTop = platformTop0 + (p.sinkTo - platformTop0) * done / total;
                    return true;
                }
            }
            return false;
        }

        void Say(string stage, double sec)
        {
            ev.stage = stage;
            ev.seconds = sec;
            events.Emit("lantern31", ev);
        }
    }
}
