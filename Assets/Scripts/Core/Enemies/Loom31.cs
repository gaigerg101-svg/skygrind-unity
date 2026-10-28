// NIGHT31 builder B (design.n31.loom31; DESIGN31 C4 + the reviewer's R4): THE LOOM rebuilt. Keeps NIGHT30's joints-as-nodes, CUT and SWEEP (Loom30). Adds:
//   REACH     nails land in full on a joint only within fullReach (6 m) of it or while roped to that leg; Loom30's lightScale (20 %) from anywhere else (Loom30.fullReach31).
//   SIGHT     the hips sit hipUp above the deck and hipOut beyond the corner (Loom30.hipOut31 / hipDrop31): pull-in targets a player sees from the deck edge (G's pick31: under
//             the deck the hook took a surface), and too far from the deck for a corner camper's full damage.
//   SHAKE     a player hanging on the Loom (the rope's anchor on a joint or a leg) longer than hangSeconds: the joint SHAKES (a shakeTellSeconds tell, event loom31 "shakeTell"),
//             then throws him off (Loom30.cutLine, event "shake"). Let go in the tell: nothing.
//   DROP      a player standing on the deck deckSeconds: a FLYER DROP at his spot (a dropTellSeconds red-orange mark, event "dropTell"), then a hit within dropRadius
//             (dropDamage, a knock; event "drop"); dropEverySeconds before the next. (The flyer itself is a strike for now: real flyers are on NIGHT30's cut list.)
//   BREAK     at breakAt of the bar the deck's middle (builder L's s8.deckBreak31 rect) cracks (breakTellSeconds, event "breakTell") and opens to lava: feet in the rect at deck
//             height take design.n30.lava's rules (damageFraction, knockUp, graceSeconds). The deck's collider stays (the lava is at the deck's top: the swap is the view's).
// Fixed 120 Hz, whole steps, no randomness, no audio, no UnityEngine.
using System;

namespace Skygrind.Core
{
    public sealed class Loom31Params
    {
        public double fullReach, hipOut, hipUp, hangSeconds, shakeTellSeconds, shakeDropSpeed, deckSeconds, dropTellSeconds, dropRadius, dropDamage, dropKnock, dropUp, dropEverySeconds;
        public double otherScale, nailLightScale, breakAt, breakTellSeconds, lavaFraction, lavaKnockUp, lavaGrace;
        public double[] breakRect;

        public static Loom31Params From(GameConfig c)
        {
            const string g = "design.n31.loom31.";
            return new Loom31Params
            {
                fullReach = c.Num(g + "fullReach"), hipOut = c.Num(g + "hipOut"), hipUp = c.Num(g + "hipUp"), hangSeconds = c.Num(g + "hangSeconds"), shakeTellSeconds = c.Num(g + "shakeTellSeconds"),
                shakeDropSpeed = c.Num(g + "shakeDropSpeed"), deckSeconds = c.Num(g + "deckSeconds"), dropTellSeconds = c.Num(g + "dropTellSeconds"), dropRadius = c.Num(g + "dropRadius"),
                dropDamage = c.Num(g + "dropDamage"), dropKnock = c.Num(g + "dropKnock"), dropUp = c.Num(g + "dropUp"), dropEverySeconds = c.Num(g + "dropEverySeconds"),
                otherScale = c.Num(g + "otherScale"), nailLightScale = c.Num(g + "nailLightScale"), breakAt = c.Num(g + "breakAt"), breakTellSeconds = c.Num(g + "breakTellSeconds"), breakRect = c.Nums(g + "breakRect"),
                lavaFraction = c.Num("design.n30.lava.damageFraction"), lavaKnockUp = c.Num("design.n30.lava.knockUp"), lavaGrace = c.Num("design.n30.lava.graceSeconds"),
            };
        }

        Loom31Params orig32;

        /// <summary>NIGHT32 B (design.n32.bosses32.loom): the row's overrides on (other guns and off-joint nails chip at least 25 %, a longer full-damage reach, a longer hang
        /// before the shake), or NIGHT31's values back. The caller re-hooks the Loom (Loom31.Hook) so Loom30 reads them.</summary>
        public void Set32(GameConfig c, bool on)
        {
            orig32 ??= (Loom31Params)MemberwiseClone();
            fullReach = on ? Boss32Cfg.Num(c, "loom", "fullReach", orig32.fullReach) : orig32.fullReach;
            hangSeconds = on ? Boss32Cfg.Num(c, "loom", "hangSeconds", orig32.hangSeconds) : orig32.hangSeconds;
            otherScale = on ? Boss32Cfg.Num(c, "loom", "otherScale", orig32.otherScale) : orig32.otherScale;
            nailLightScale = on ? Boss32Cfg.Num(c, "loom", "nailLightScale", orig32.nailLightScale) : orig32.nailLightScale;
            hipOut = on ? Boss32Cfg.Num(c, "loom", "hipOut", orig32.hipOut) : orig32.hipOut;
            hipUp = on ? Boss32Cfg.Num(c, "loom", "hipUp", orig32.hipUp) : orig32.hipUp;
            // the anti-camp (the lead 20:13): the flyer drop finds a grounded player sooner, more often and harder
            deckSeconds = on ? Boss32Cfg.Num(c, "loom", "deckSeconds", orig32.deckSeconds) : orig32.deckSeconds;
            dropTellSeconds = on ? Boss32Cfg.Num(c, "loom", "dropTellSeconds", orig32.dropTellSeconds) : orig32.dropTellSeconds;
            dropEverySeconds = on ? Boss32Cfg.Num(c, "loom", "dropEverySeconds", orig32.dropEverySeconds) : orig32.dropEverySeconds;
            dropDamage = on ? Boss32Cfg.Num(c, "loom", "dropDamage", orig32.dropDamage) : orig32.dropDamage;
        }
    }

    /// <summary>The payload of "loom31": stage shakeTell / shake / shakeDodged / dropTell / drop / breakTell / break; where.</summary>
    public sealed class Loom31Event
    {
        public string stage;
        public Vec3 at;
        public double seconds;
    }

    public sealed class Loom31
    {
        public readonly Loom31Params p;
        readonly EventBus events;
        readonly BossTarget target;
        readonly double dt0;
        readonly Loom31Event ev = new Loom31Event();
        double hangTime, deckTime;
        int shakeTell, dropTell, dropCool, breakTell, lavaGrace;
        /// <summary>0 whole, 1 cracking (the tell), 2 open (lava).</summary>
        public int breakState;
        /// <summary>The deck's breakable middle (world x0, z0, x1, z1): the level row's, else design.n31.loom31.breakRect.</summary>
        public double[] rect;
        public Vec3 dropAt;
        /// <summary>The sim opened the pit (the deck middle's collider gone, lava below: design.n30.lava's own zone): this class's deck-height burn (the harness rig's fallback) stays off.</summary>
        public bool pitOpen;
        /// <summary>The lava's top in the pit (the level row's lavaY, else the deck top minus pitDepth + 1).</summary>
        public double lavaY = double.NaN;
        public bool ShakeTelling => shakeTell > 0;
        public bool DropTelling => dropTell > 0;
        public double DropTellProgress => dropTell > 0 ? 1.0 - (double)dropTell / Math.Max(1, S(p.dropTellSeconds)) : 0;
        public double BreakTellProgress => breakState == 1 ? 1.0 - (double)breakTell / Math.Max(1, S(p.breakTellSeconds)) : breakState == 2 ? 1 : 0;
        // the harness's counters (this fight)
        public int shakes, shakeTells, shakesDodged, drops, dropHits, lavaHits;

        /// <summary>NIGHT32 B (design.n32.bosses32.loom): the row is on (Set32).</summary>
        public bool on32;

        /// <summary>NIGHT32 B: the row on (its numbers) or off (NIGHT31's); re-hooks `l` when given.</summary>
        public void Set32(GameConfig c, bool on, Loom30 l)
        {
            p.Set32(c, on);
            on32 = on;
            if (l != null) Hook(l);
        }

        public Loom31(GameConfig cfg, Loom31Params p, EventBus events, BossTarget target)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            dt0 = cfg.Dt;
            rect = p.breakRect;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public void Reset()
        {
            hangTime = deckTime = 0;
            shakeTell = dropTell = dropCool = breakTell = lavaGrace = 0;
            breakState = 0;
            shakes = shakeTells = shakesDodged = drops = dropHits = lavaHits = 0;
        }

        /// <summary>Set the Loom's hooks for the row (the reach and the hips).</summary>
        public void Hook(Loom30 l)
        {
            l.fullReach31 = p.fullReach;
            l.hipOut31 = p.hipOut;
            l.hipDrop31 = -p.hipUp;
            l.nailOnly31 = true;
            l.otherScale31 = p.otherScale;
            l.lightScale31 = p.nailLightScale;
        }

        public static void Unhook(Loom30 l)
        {
            l.fullReach31 = -1;
            l.hipOut31 = double.NaN;
            l.hipDrop31 = double.NaN;
            l.nailOnly31 = false;
            l.lightScale31 = -1;
        }

        public bool InBreak(Vec3 feet) => rect != null && rect.Length == 4 && feet.x >= Math.Min(rect[0], rect[2]) && feet.x <= Math.Max(rect[0], rect[2]) && feet.z >= Math.Min(rect[1], rect[3]) && feet.z <= Math.Max(rect[1], rect[3]);

        /// <summary>One step of the fight (only in Loom30's p1). `hanging`: the player's rope is attached with its anchor on the Loom (a joint or a leg).</summary>
        public void Step(Loom30 l, bool hanging, bool grounded, double dt)
        {
            Vec3 me = target.position();
            if (l.phase != "p1") return;
            // SHAKE
            if (shakeTell > 0)
            {
                if (--shakeTell == 0)
                {
                    if (hanging) { shakes++; l.cutLine?.Invoke(p.shakeDropSpeed); Say("shake", me, 0); }
                    else { shakesDodged++; Say("shakeDodged", me, 0); }
                    hangTime = 0;
                }
            }
            else
            {
                hangTime = hanging ? hangTime + dt : 0;
                if (hangTime >= p.hangSeconds) { shakeTell = Math.Max(1, S(p.shakeTellSeconds)); shakeTells++; Say("shakeTell", me, p.shakeTellSeconds); }
            }
            // DROP
            bool onDeck = grounded && !hanging; // on the deck or anywhere on the ground of the room: the flyers find a ground camper (the entry ledge was a safe spot)
            deckTime = onDeck ? deckTime + dt : 0;
            if (dropCool > 0) dropCool--;
            if (dropTell > 0)
            {
                if (--dropTell == 0)
                {
                    drops++;
                    Say("drop", dropAt, 0);
                    double dx = me.x - dropAt.x, dz = me.z - dropAt.z, d = JsMath.Hypot(dx, dz);
                    if (d <= p.dropRadius && me.y <= dropAt.y + 1.5)
                    {
                        Health h = target.health;
                        if (h != null && !h.Dead && h.Damage(p.dropDamage, "boss")) dropHits++;
                        if (d < 1e-6) { dx = 1; d = 1; }
                        target.impulse?.Invoke(dx / d * p.dropKnock, p.dropUp, dz / d * p.dropKnock);
                    }
                    dropCool = S(p.dropEverySeconds);
                }
            }
            else if (dropCool == 0 && deckTime >= p.deckSeconds)
            {
                dropAt = me;
                dropTell = Math.Max(1, S(p.dropTellSeconds));
                deckTime = 0;
                Say("dropTell", dropAt, p.dropTellSeconds);
            }
            // BREAK
            if (breakState == 0 && l.HpFrac <= p.breakAt) { breakState = 1; breakTell = Math.Max(1, S(p.breakTellSeconds)); Say("breakTell", l.centre, p.breakTellSeconds); }
            else if (breakState == 1 && --breakTell == 0) { breakState = 2; Say("break", l.centre, 0); }
            if (lavaGrace > 0) lavaGrace--;
            if (breakState == 2 && lavaGrace == 0 && InBreak(me) && me.y <= (pitOpen && !double.IsNaN(lavaY) ? lavaY : l.centre.y) + 0.15) // the pit (the sim opened it: the lava below) or, on the harness rig, the deck itself
            {
                Health h = target.health;
                if (h != null && !h.Dead && h.Damage(h.max * p.lavaFraction, "boss")) lavaHits++;
                target.impulse?.Invoke(0, p.lavaKnockUp, 0);
                lavaGrace = S(p.lavaGrace);
            }
        }

        void Say(string stage, Vec3 at, double sec)
        {
            ev.stage = stage;
            ev.at = at;
            ev.seconds = sec;
            events.Emit("loom31", ev);
        }
    }
}
