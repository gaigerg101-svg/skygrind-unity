// NIGHT11 U3-24.3: port of skygrind/src/survival/objectives.js (NIGHT6 B7), line for line, minus the three.js ring.
// NIGHT6 B7: objective rounds, so not every round is "kill the count" (Zombies' special rounds; CoD's hardpoint).
// HOLD: a ring on one of the map's hold spots in an open zone; stand in it for config.objectives.holdTime s in all while
// the round runs; the round can't clear until it's held (survival's holdRound hook); combat points pay more inside it, and
// holding it pays a bonus. GOLD RUSH: every kill pays double, and a chain of kills pays a bonus per kill. The kind comes
// from the round number (ai/directorMath.js roundType), the spot from the round too: no randomness. Fixed step; the ring
// on frame time.
// The port: the ring's state is exposed for a view (RingVisible, RingPosition, Inside; Game's ObjectiveRingView breathes
// it on frame time as update() does). The deps are the calls objectives.js makes (zoneAt, isOpen, setBoost, grant, the
// player's feet): a delegate constructor for the golden session's fakes, and the game's over Zones and Economy. Step()
// allocates nothing; the kill handler filters the chain in place. Events keep the web's names and fields:
// objectiveStart {type, round}, objectiveDone {owner, type, round, bonus}, banner {text}.
// The Game's side (main.js order): Load(map.holdSpots) at run start (after the run's start), Load(null) on a reset;
// Begin(round) after the director plans the round, before roundBegin (main.js onRoundBegin); SurvivalRun.holdRound =
// () => Holding; SurvivalRun.stepHook = _ => Step().
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Skygrind.Core
{
    /// <summary>objectives.js objectiveStart {type, round}.</summary>
    public sealed class ObjectiveStartEvent
    {
        public string type;
        public int round;
    }

    /// <summary>objectives.js objectiveDone {owner, type, round, bonus}.</summary>
    public sealed class ObjectiveDoneEvent
    {
        public string owner = "player";
        public string type;
        public int round;
        public double bonus;
    }

    public sealed class Objectives : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot (explicit, so the map's Load(null) stays unambiguous): the round's objective,
        // the hold (spot, held, need, done, inside), GOLD RUSH's chain, the clock (never reset), the ring, the switch. The
        // hold spots are the map's (StartRun's Load(map.holdSpots)): their count is checked.
        void IRunState.Save(StateWriter w)
        {
            w.S("type", type);
            w.I("round", round);
            w.VN("spot", spot);
            w.I("held", held);
            w.I("need", need);
            w.B("done", done);
            w.B("inside", inside);
            w.Doubles("chain", chain);
            w.I("time", time);
            w.B("ringVisible", ringVisible);
            w.V("ringPosition", ringPosition);
            w.B("active", active);
            w.I("spots", spots.Count);
        }

        void IRunState.Load(StateReader r)
        {
            string t = r.S("type");
            if (t != Normal && t != Hold && t != Gold) throw new StateException($"snapshot: objectives.type '{t}'");
            if (r.Int("spots") != spots.Count) throw new StateException("snapshot: objectives: the hold spots differ (restore after StartRun on the same map)");
            List<double> c = r.Doubles("chain");
            type = t;
            round = r.Int("round");
            spot = r.VN("spot");
            held = r.Int("held");
            need = r.Int("need");
            done = r.B("done");
            inside = r.B("inside");
            chain.Clear();
            chain.AddRange(c);
            time = r.Int("time");
            ringVisible = r.B("ringVisible");
            ringPosition = r.V("ringPosition");
            active = r.B("active");
        }

        /// <summary>config.objectives, read once.</summary>
        public sealed class P
        {
            public bool enabled;
            public double holdTime, radius, pointsMult, bonus, goldMult, chainWindow, chainAt, chainBonus;
            public uint color;

            public static P From(GameConfig c) => new P
            {
                enabled = c.Get("objectives.enabled") is bool on && on, holdTime = c.Num("objectives.holdTime"), radius = c.Num("objectives.radius"),
                pointsMult = c.Num("objectives.pointsMult"), bonus = c.Num("objectives.bonus"), goldMult = c.Num("objectives.goldMult"),
                chainWindow = c.Num("objectives.chainWindow"), chainAt = c.Num("objectives.chainAt"), chainBonus = c.Num("objectives.chainBonus"),
                color = (uint)c.Num("objectives.color"),
            };
        }

        public const string Normal = "normal", Hold = "hold", Gold = "gold";

        readonly EventBus events;
        // The deps (objectives.js { player, economy, zones }) as the calls it makes, so a checker can drive it with fakes:
        readonly Func<Vec3, string> zoneAt; // zones.zoneAt(p)
        readonly Func<string, bool> isOpen; // zones.isOpen(id)
        readonly Action<string, double> setBoost; // economy.setBoost(name, mult)
        readonly Action<double, string> grant; // economy.grant(amount, reason)
        readonly Func<Vec3> playerPos; // player.mover.position (feet)
        readonly DirectorMath.D rounds; // config.director.rounds (roundType)
        readonly double simHz;
        public readonly P o;
        readonly string holdBanner, goldBanner; // the web's template strings, built once (their numbers are config's)
        static readonly List<Vec3> None = new List<Vec3>();
        List<Vec3> spots = None;
        readonly List<Vec3> open = new List<Vec3>(); // begin()'s open spots (reused)
        string type = Normal;
        int round;
        Vec3? spot;
        int held;
        int need;
        bool done;
        bool inside;
        readonly List<double> chain = new List<double>(16);
        int time; // fixed steps
        bool ringVisible; // the web's ring.visible
        Vec3 ringPosition; // the web's ring.position (set by a HOLD's begin, never reset)
        bool active;

        static string Js(double v) => v.ToString("R", CultureInfo.InvariantCulture); // String(number), as a template literal prints it

        /// <summary>The game's: the real zones and economy.</summary>
        public Objectives(GameConfig cfg, EventBus events, Economy economy, Zones zones, Func<Vec3> playerPos)
            : this(cfg, events, zones.ZoneAt, zones.IsOpen, economy.SetBoost, (amount, reason) => economy.Grant(amount, reason), playerPos)
        {
        }

        /// <summary>
        /// The deps as delegates (the golden session's fakes): zoneAt, isOpen, setBoost, grant, the player's feet.
        /// `objectives`: the config.objectives to use instead of cfg's (a checker's override, e.g. enabled false); null reads cfg.
        /// </summary>
        public Objectives(GameConfig cfg, EventBus events, Func<Vec3, string> zoneAt, Func<string, bool> isOpen, Action<string, double> setBoost,
            Action<double, string> grant, Func<Vec3> playerPos, P objectives = null)
        {
            this.events = events;
            this.zoneAt = zoneAt;
            this.isOpen = isOpen;
            this.setBoost = setBoost;
            this.grant = grant;
            this.playerPos = playerPos;
            o = objectives ?? P.From(cfg);
            rounds = DirectorMath.From(cfg);
            simHz = cfg.movement.loop.simHz;
            holdBanner = $"HOLD ROUND · STAND IN THE RING FOR {Js(o.holdTime)} S";
            goldBanner = $"GOLD RUSH · KILLS PAY x{Js(o.goldMult)} · CHAIN THEM";
            events.On<KillEvent>("kill", e =>
            {
                if (!active || type != Gold || e.owner != "player") return;
                double t = time / simHz;
                // chain = chain.filter((x) => t - x <= o.chainWindow), in place (the order kept)
                int keep = 0;
                for (int i = 0; i < chain.Count; i++)
                {
                    if (t - chain[i] <= o.chainWindow) chain[keep++] = chain[i];
                }
                chain.RemoveRange(keep, chain.Count - keep);
                chain.Add(t);
                if (chain.Count >= o.chainAt) grant(o.chainBonus * chain.Count, "goldChain");
            });
            events.On("roundClear", _ =>
            {
                if (active) EndRound();
            });
        }

        void EndRound()
        {
            setBoost("hold", 1);
            setBoost("gold", 1);
            ringVisible = false;
            type = Normal;
            spot = null;
        }

        /// <summary>
        /// A new run on a map (load(map)): `holdSpots` null = no map (inactive); an empty list = a map with no holdSpots
        /// (active: GOLD RUSH runs, a HOLD round is 'normal'), as the web's `spots = map?.holdSpots ?? []; active = !!map &&
        /// o.enabled`. Always ends the round first (setBoost hold 1, gold 1).
        /// </summary>
        public void Load(List<Vec3> holdSpots)
        {
            EndRound();
            spots = holdSpots ?? None;
            active = holdSpots != null && o.enabled;
        }

        /// <summary>A round begins: its objective, if any. Returns the kind.</summary>
        public string Begin(int n)
        {
            if (!active) return Normal;
            EndRound();
            round = n;
            string kind = DirectorMath.RoundType(n, rounds);
            if (kind == Hold)
            {
                // spots.filter((s) => zones.isOpen(zones.zoneAt({x: s.x, y: s.y + 1, z: s.z}))): each zoneAt, then its isOpen
                open.Clear();
                foreach (Vec3 s in spots)
                {
                    if (isOpen(zoneAt(new Vec3(s.x, s.y + 1, s.z)))) open.Add(s);
                }
                if (open.Count == 0) return Normal;
                Vec3 at = open[(n * 7) % open.Count];
                spot = at;
                held = 0;
                need = (int)JsMath.Round(o.holdTime * simHz); // S(o.holdTime): Math.round(sec * simHz), not stepsFor
                done = false;
                ringPosition = new Vec3(at.x, at.y + 0.04, at.z); // ring.position.set(spot.x, spot.y + 0.04, spot.z)
                ringVisible = true;
                type = Hold;
                events.Emit("banner", new BannerEvent { text = holdBanner });
            }
            else if (kind == Gold)
            {
                type = Gold;
                chain.Clear();
                setBoost("gold", o.goldMult);
                events.Emit("banner", new BannerEvent { text = goldBanner });
            }
            else
            {
                return kind;
            }
            events.Emit("objectiveStart", new ObjectiveStartEvent { type = type, round = n });
            return type;
        }

        /// <summary>The round may not clear yet (a HOLD still short).</summary>
        public bool Holding => active && type == Hold && !done;

        /// <summary>One fixed step: the hold's clock. `time` counts every call (inactive too), as the web's.</summary>
        public void Step()
        {
            time++;
            if (!active || type != Hold || done || !spot.HasValue) return;
            Vec3 p = playerPos();
            Vec3 s = spot.Value;
            inside = JsMath.Hypot(p.x - s.x, p.z - s.z) <= o.radius && Math.Abs(p.y - s.y) < 2.5;
            setBoost("hold", inside ? o.pointsMult : 1);
            if (inside && ++held >= need)
            {
                done = true;
                setBoost("hold", 1);
                double bonus = o.bonus * round;
                grant(bonus, "hold");
                events.Emit("objectiveDone", new ObjectiveDoneEvent { owner = "player", type = Hold, round = round, bonus = bonus });
                events.Emit("banner", new BannerEvent { text = $"HELD · +{Js(bonus)}" });
            }
        }

        // The HUD's line (hud(): {type, left (s), inside} while an objective runs, else null), read without allocating.

        /// <summary>hud() is not null.</summary>
        public bool HudOn => active && type != Normal && !(type == Hold && done);

        /// <summary>hud().type: "hold" or "gold" (read while HudOn).</summary>
        public string HudType => type;

        /// <summary>hud().left: whole seconds of holding still needed (a HOLD).</summary>
        public int HudLeft => type == Hold ? (int)Math.Ceiling((need - held) / simHz) : 0;

        /// <summary>hud().inside (a HOLD).</summary>
        public bool HudInside => inside;

        // The ring's state for the view (the web's ring mesh): shown on a HOLD round until the round ends, at the spot.

        /// <summary>ring.visible (stays on after a HOLD is done, until the round ends).</summary>
        public bool RingVisible => ringVisible;

        /// <summary>ring.position: (spot.x, spot.y + 0.04, spot.z) of the last HOLD begun (never reset; the origin before any).</summary>
        public Vec3 RingPosition => ringPosition;

        /// <summary>The ring's radius (objectives.radius).</summary>
        public double Radius => o.radius;

        /// <summary>objectives.js's private `time`: every Step() call so far (never reset; the GOLD RUSH chain's clock is time / simHz).</summary>
        public int Time => time;

        // Debug: the state (debugState(): {type, round, spot, held, need, done, inside}).
        public bool Active => active;
        public string Type => type;
        public int Round => round;
        public Vec3? Spot => spot;
        public int Held => held;
        public int Need => need;
        public bool Done => done;
        public bool Inside => inside;
        /// <summary>The GOLD RUSH chain's length (kill times within the window, as of the last kill).</summary>
        public int ChainLength => chain.Count;
    }
}
