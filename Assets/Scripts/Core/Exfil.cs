// NIGHT11 U3-24.7: EXFIL (design.exfil). New design, not a web port: the web game ends only in death. CoD Zombies' exfil
// (NIGHT11-RESEARCH.md A.3 and C.2): Cold War from round 10, BO6 from round 11, every 5, at a radio at the spawn; about 90 s
// to reach the site, then 35 kills in about 85 s at round 11, the demand growing with the round; failure ends the run.
// Here it ends the run either way (Cold War's frequent, safe exfils felt too easy).
// - The radio (a Buyables item: hold E, free, perks.reach, at design.exfil.radio) is offered in the break before round R
//   when R >= from and (R - from) % every == 0 (SurvivalRun counts `round` up when the break starts, so that break's
//   roundBreak carries R) and a site's zone is open. Calling it: `exfilCall {round}` and a banner.
// - Round R is the exfil. At its `roundBegin`: the site (sites[((R - from) / every) % count], else the next one whose zone
//   is open), `exfilStart {round, site, zone, position}` and a banner. Holding is true while it runs (the Game ORs it into
//   SurvivalRun.holdRound, so the round can't clear).
//   REACH: stand within siteRadius m (horizontal) and siteHeight m (vertical) of the site within reachTime s:
//   `exfilReach {round, needed}`, a banner. Else `exfilFail {round, reason "time"}`.
//   HORDE: `needed` player kills (kills + killsPerExfil x ((R - from) / every)) within killTime s. FullPace is true (the
//   Game sets director.fullPace); whenever fewer enemies remain than kills are still needed, the queue is topped up from
//   the round's own squad plan (DirectorMath.PlanSquads for R on the exfil's Mulberry32, runSeed ^ seedSalt) through
//   `addSquads`, at most once per director.squadInterval. Success: `exfilDone {round, kills}`, Outcome "EXFIL". Time:
//   `exfilFail {round, reason "time"}`. The player's death while it runs: `exfilFail {reason "death"}`. Either failure:
//   Outcome "EXFIL FAILED".
// - The Game ends the run on Outcome (EXFIL as a win, EXFIL FAILED as a death) and copies it to RunStats.exfil; Records.Run
//   keeps an exfil flag.
// Flag off (or no design config): no radio (Radio is null), no handler acts, Holding is false: the web's game.
// Step allocates nothing unless it tops the queue up (PlanSquads' lists, at most once per squad interval); payloads reused.
using System;
using System.Collections.Generic;
using System.IO;

namespace Skygrind.Core
{
    public enum ExfilPhase
    {
        Off, // nothing called
        Called, // the radio was used in this break; the next round is the exfil
        Reach, // get to the site
        Horde, // the kill demand
        Done, // success (Outcome EXFIL)
        Failed, // failure (Outcome EXFIL FAILED)
    }

    /// <summary>exfilCall / exfilStart / exfilReach / exfilDone / exfilFail (port-only). Reused: read it inside the handler.</summary>
    public sealed class ExfilEvent
    {
        public string owner = "player";
        public int round;
        public int site = -1; // the site index (design.exfil.sites)
        public string zone; // the site's zone id
        public Vec3 position; // the site's feet
        public int kills, needed;
        public string reason; // exfilFail: "time" or "death"
    }

    public static class ExfilMath
    {
        /// <summary>Is round R an exfil round (offered in the break before it)?</summary>
        public static bool IsExfilRound(int round, int from, int every) => every > 0 && round >= from && (round - from) % every == 0;

        /// <summary>Which exfil this is (0 for the first, at `from`).</summary>
        public static int Index(int round, int from, int every) => (round - from) / every;

        /// <summary>The kill demand of exfil round R: kills + killsPerExfil x its index.</summary>
        public static int KillsFor(int round, int from, int every, int kills, int killsPerExfil) => kills + killsPerExfil * Index(round, from, every);

        /// <summary>The site of exfil round R: index % count, else the next open one (cycling); -1 when none is open.</summary>
        public static int SiteFor(int round, int from, int every, int count, Func<int, bool> open)
        {
            if (count <= 0) return -1;
            int k = Index(round, from, every) % count;
            for (int i = 0; i < count; i++)
            {
                int s = (k + i) % count;
                if (open == null || open(s)) return s;
            }
            return -1;
        }
    }

    public sealed class Exfil : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the stream (the HORDE's top-up squads), the phase and its clocks, the break
        // it was called in, the site, the kills, the outcome and the top-up counters. (BO6's rule keeps a save out of an
        // exfil; a called exfil in its break is saved like any phase.)
        public void Save(StateWriter w)
        {
            w.Rng("rng", rng);
            w.I("phaseSteps", phaseSteps);
            w.I("topUpWait", topUpWait);
            w.I("breakRound", breakRound);
            w.B("inBreak", inBreak);
            w.D("dt", dt);
            w.I("phase", (int)phase);
            w.I("round", round);
            w.I("site", site);
            w.I("kills", kills);
            w.I("needed", needed);
            w.S("outcome", Outcome);
            w.S("failReason", failReason);
            w.I("topUpSquads", topUpSquads);
            w.I("topUpEnemies", topUpEnemies);
        }

        public void Load(StateReader r)
        {
            int ph = r.Int("phase"), s = r.Int("site");
            if (!Enum.IsDefined(typeof(ExfilPhase), ph) || s < -1 || s >= sites.Count) throw new StateException("snapshot: exfil: the phase or site is out of range");
            rng = r.Rng("rng");
            rngNext = rng.Next;
            phaseSteps = r.Int("phaseSteps");
            topUpWait = r.Int("topUpWait");
            breakRound = r.Int("breakRound");
            inBreak = r.B("inBreak");
            dt = r.D("dt");
            phase = (ExfilPhase)ph;
            round = r.Int("round");
            site = s;
            kills = r.Int("kills");
            needed = r.Int("needed");
            Outcome = r.S("outcome");
            failReason = r.S("failReason");
            topUpSquads = r.Int("topUpSquads");
            topUpEnemies = r.Int("topUpEnemies");
        }

        /// <summary>The radio's Buyables item id.</summary>
        public const string ItemId = "exfil:radio";

        /// <summary>Outcome values (RunStats.exfil).</summary>
        public const string Win = "EXFIL", Lost = "EXFIL FAILED";

        /// <summary>design.exfil.enabled: false (or no design config) means no exfil at all, the web's game.</summary>
        public static bool Enabled(GameConfig cfg) => cfg.Design("exfil.enabled");

        readonly EventBus events;
        readonly Func<Vec3> playerPos;
        readonly Func<int> remaining;
        readonly Action<List<List<string>>> addSquads;
        readonly DirectorMath.D d;
        readonly List<BoxSpot> sites = new List<BoxSpot>();
        readonly List<List<string>> topUp = new List<List<string>>();
        readonly ExfilEvent payload = new ExfilEvent();
        readonly BannerEvent banner = new BannerEvent();
        readonly Func<int, bool> siteOpen;
        readonly uint seedSalt;
        readonly int topUpSteps;
        Mulberry32 rng = new Mulberry32(1);
        Func<double> rngNext;
        int phaseSteps, topUpWait, breakRound;
        bool inBreak;
        double dt;

        public readonly bool enabled;
        public readonly int from, every, killsBase, killsPerExfil;
        public readonly double reachTime, killTime, siteRadius, siteHeight;

        /// <summary>The radio's spot (design.exfil.radio).</summary>
        public readonly BoxSpot radio;

        /// <summary>The radio as a Buyables item (hold E, free, perks.reach); null with the flag off.</summary>
        public readonly Buyables.Item Radio;

        /// <summary>Optional: is zone `id` open? (a site only in an open zone). Null: every zone.</summary>
        public Func<string, bool> zoneOpen;

        public ExfilPhase phase;
        /// <summary>The exfil round (called or running); 0 when none.</summary>
        public int round;
        /// <summary>The running exfil's site index (-1: none).</summary>
        public int site = -1;
        /// <summary>HORDE: player kills so far, and the demand.</summary>
        public int kills, needed;
        /// <summary>null, "EXFIL" (a win) or "EXFIL FAILED"; the Game ends the run when it is set.</summary>
        public string Outcome;
        /// <summary>exfilFail's reason ("time", "death"), else null.</summary>
        public string failReason;
        /// <summary>Squads and enemies the HORDE added this run (top-ups).</summary>
        public int topUpSquads, topUpEnemies;

        /// <param name="mapSpots">NIGHT15: a Unity-authored map's spots (SurvivalMap.spots): its "exfil" {radio, sites} replaces design.exfil.radio / sites.</param>
        public Exfil(GameConfig cfg, EventBus events, Func<Vec3> playerPos, Func<int> remaining = null, Action<List<List<string>>> addSquads = null, Dictionary<string, object> mapSpots = null)
        {
            this.events = events;
            this.playerPos = playerPos;
            this.remaining = remaining;
            this.addSquads = addSquads;
            dt = cfg.Dt;
            d = DirectorMath.From(cfg);
            siteOpen = SiteOpen;
            rngNext = rng.Next;
            enabled = Enabled(cfg);
            topUpSteps = MovementMath.StepsFor(cfg.Num("director.squadInterval"), dt);
            events.On<RoundEvent>("roundBreak", OnBreak);
            events.On<RoundEvent>("roundBegin", OnBegin);
            events.On<KillEvent>("kill", OnKill);
            events.On<DeathEvent>("death", OnDeath);
            if (!enabled) return;
            from = cfg.Int("design.exfil.from");
            every = cfg.Int("design.exfil.every");
            reachTime = cfg.Num("design.exfil.reachTime");
            killTime = cfg.Num("design.exfil.killTime");
            killsBase = cfg.Int("design.exfil.kills");
            killsPerExfil = cfg.Int("design.exfil.killsPerExfil");
            siteRadius = cfg.Num("design.exfil.siteRadius");
            siteHeight = cfg.Num("design.exfil.siteHeight");
            seedSalt = (uint)cfg.Num("design.exfil.seedSalt");
            Dictionary<string, object> mapExfil = mapSpots != null && mapSpots.TryGetValue("exfil", out object me) && me != null ? PortData.Obj(me) : null;
            radio = Spot(mapExfil != null ? PortData.Obj(mapExfil["radio"]) : cfg.Obj("design.exfil.radio"));
            foreach (object o in mapExfil != null ? PortData.Arr(mapExfil["sites"]) : cfg.Arr("design.exfil.sites")) sites.Add(Spot(PortData.Obj(o)));
            if (sites.Count == 0) throw new InvalidDataException("design.exfil.sites is empty");
            Radio = new Buyables.Item
            {
                id = ItemId, name = "CALL EXFIL", kind = "radio", position = radio.pos, reach = cfg.Num("perks.reach"),
                price = Free, available = CanCall, buy = CallFromBuy,
            };
        }

        static BoxSpot Spot(Dictionary<string, object> s) => new BoxSpot
        {
            pos = new Vec3(PortData.Num(s["x"]), PortData.Num(s["y"]), PortData.Num(s["z"])),
            yaw = s.TryGetValue("yaw", out object y) ? PortData.Num(y) : 0,
            zone = (string)s["zone"],
        };

        static double Free() => 0;

        void CallFromBuy() => Call();

        bool SiteOpen(int i) => zoneOpen == null || zoneOpen(sites[i].zone);

        public int SiteCount => sites.Count;

        public BoxSpot Site(int i) => sites[i];

        /// <summary>The running exfil's site (feet); default when none.</summary>
        public Vec3 SitePos => site >= 0 ? sites[site].pos : default;

        /// <summary>The running exfil's site zone id; null when none.</summary>
        public string SiteZone => site >= 0 ? sites[site].zone : null;

        /// <summary>Is round R an exfil round?</summary>
        public bool IsExfilRound(int r) => enabled && ExfilMath.IsExfilRound(r, from, every);

        /// <summary>Exfil round R's kill demand.</summary>
        public int KillsFor(int r) => ExfilMath.KillsFor(r, from, every, killsBase, killsPerExfil);

        /// <summary>Exfil round R's site (-1: no site's zone is open).</summary>
        public int SiteFor(int r) => ExfilMath.SiteFor(r, from, every, sites.Count, siteOpen);

        /// <summary>REACH or HORDE: the round can't clear (OR it into SurvivalRun.holdRound).</summary>
        public bool Holding => phase == ExfilPhase.Reach || phase == ExfilPhase.Horde;

        /// <summary>HORDE: the director runs at full pace (the Game sets director.fullPace).</summary>
        public bool FullPace => phase == ExfilPhase.Horde;

        /// <summary>Seconds left in REACH or HORDE (0 otherwise), for the HUD.</summary>
        public double SecondsLeft => Holding ? phaseSteps * dt : 0;

        /// <summary>The radio can be called now (its Buyables `available`): the flag on, nothing called or decided, in the break before an exfil round with a site's zone open.</summary>
        public bool CanCall() => enabled && phase == ExfilPhase.Off && Outcome == null && inBreak && IsExfilRound(breakRound) && SiteFor(breakRound) >= 0;

        /// <summary>A new run: nothing called, no outcome; the top-up stream from the run seed.</summary>
        public void Reset(uint runSeed)
        {
            rng = new Mulberry32(runSeed ^ seedSalt);
            rngNext = rng.Next;
            phase = ExfilPhase.Off;
            round = breakRound = 0;
            inBreak = false;
            site = -1;
            kills = needed = 0;
            phaseSteps = topUpWait = 0;
            topUpSquads = topUpEnemies = 0;
            Outcome = null;
            failReason = null;
        }

        /// <summary>Use the radio (the Buyables item's buy): the next round becomes the exfil. False when it can't be called now.</summary>
        public bool Call(string owner = "player")
        {
            if (!CanCall()) return false;
            phase = ExfilPhase.Called;
            round = breakRound;
            Fill(owner);
            events.Emit("exfilCall", payload);
            Banner($"EXFIL CALLED · ROUND {round} IS THE WAY OUT"); // once per call
            return true;
        }

        void OnBreak(RoundEvent e)
        {
            inBreak = true;
            breakRound = e.number;
            if (phase == ExfilPhase.Called && round != e.number) phase = ExfilPhase.Off; // a debug skip past the called round
            if (Holding) // only a debug skip ends a running exfil's round (it can't clear)
            {
                phase = ExfilPhase.Off;
                site = -1;
            }
        }

        void OnBegin(RoundEvent e)
        {
            inBreak = false;
            if (!enabled || phase != ExfilPhase.Called || e.number != round) return;
            site = SiteFor(round);
            if (site < 0) site = ExfilMath.Index(round, from, every) % sites.Count; // zones never close: only a test gets here
            needed = KillsFor(round);
            kills = 0;
            phase = ExfilPhase.Reach;
            phaseSteps = MovementMath.StepsFor(reachTime, dt);
            Fill("player");
            events.Emit("exfilStart", payload);
            Banner("EXFIL · GET TO THE BEACON");
        }

        void OnKill(KillEvent e)
        {
            if (phase != ExfilPhase.Horde || e.owner != "player") return;
            kills++;
            if (kills >= needed) Finish();
        }

        void OnDeath(DeathEvent e)
        {
            if (Holding && e.owner == "player") Fail("death");
        }

        /// <summary>One fixed step of the exfil round (the Game calls it from SurvivalRun.stepHook, i.e. during the fight).</summary>
        public void Step(double dt)
        {
            this.dt = dt;
            if (phase == ExfilPhase.Reach)
            {
                Vec3 p = playerPos(), s = sites[site].pos;
                if (JsMath.Hypot(p.x - s.x, p.z - s.z) <= siteRadius && Math.Abs(p.y - s.y) <= siteHeight)
                {
                    phase = ExfilPhase.Horde;
                    phaseSteps = MovementMath.StepsFor(killTime, dt);
                    topUpWait = 0;
                    Fill("player");
                    events.Emit("exfilReach", payload);
                    Banner($"EXFIL · {needed} KILLS TO GET OUT"); // once per exfil (not "HOLD": that is objectives.js's round)
                    TopUp();
                    return;
                }
                if (--phaseSteps <= 0) Fail("time");
                return;
            }
            if (phase != ExfilPhase.Horde) return;
            TopUp();
            if (--phaseSteps <= 0) Fail("time");
        }

        /// <summary>HORDE: when fewer enemies remain than kills are still needed, add squads from the round's own plan (at most once per squad interval).</summary>
        void TopUp()
        {
            if (topUpWait > 0)
            {
                topUpWait--;
                return;
            }
            if (remaining == null || addSquads == null) return;
            int left = needed - kills, have = remaining();
            if (have >= left) return;
            topUp.Clear();
            int add = 0;
            foreach (List<string> squad in DirectorMath.PlanSquads(round, rngNext, d, Director.Available))
            {
                if (have + add >= left) break;
                topUp.Add(squad);
                add += squad.Count;
            }
            topUpWait = topUpSteps;
            if (topUp.Count == 0) return;
            topUpSquads += topUp.Count;
            topUpEnemies += add;
            addSquads(topUp);
        }

        void Finish()
        {
            phase = ExfilPhase.Done;
            Outcome = Win;
            Fill("player");
            events.Emit("exfilDone", payload);
            Banner("EXFIL · YOU MADE IT OUT");
        }

        void Fail(string reason)
        {
            phase = ExfilPhase.Failed;
            Outcome = Lost;
            failReason = reason;
            Fill("player");
            payload.reason = reason;
            events.Emit("exfilFail", payload);
            Banner("EXFIL FAILED");
        }

        void Fill(string owner)
        {
            payload.owner = owner;
            payload.round = round;
            payload.site = site;
            payload.zone = site >= 0 ? sites[site].zone : null;
            payload.position = site >= 0 ? sites[site].pos : default;
            payload.kills = kills;
            payload.needed = needed;
            payload.reason = null;
        }

        void Banner(string text)
        {
            banner.text = text;
            events.Emit("banner", banner);
        }
    }
}
