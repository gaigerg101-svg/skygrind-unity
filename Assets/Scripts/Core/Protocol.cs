// NIGHT11 U3-24.4: port of skygrind/src/survival/protocol.js (NIGHT6 B3), line for line, minus the meshes.
// NIGHT6 B3: THE PROTOCOL, Survival's main quest, guided (Treyarch: hidden Easter eggs reach under 2% of players, so the
// steps are on screen). Four RELAYS stand on the map's movement showpieces (map.relaySpots: the skybridge past a 7 m gap,
// STATIC's chimney island, PHOENIX's wall ring, FLOW's pinnacle). They wake at config.protocol.wakeRound; hold BUY at one
// to light it (it must be in an open zone: the quest walks you through the late gates). The next relay's beacon stands
// over the map and the HUD's top line names it. With all four lit, a SURGE of elite squads joins the round; clearing that
// round completes the PROTOCOL: every gun you own is forged free (B1), bonus points, and the PROTOCOL finish for good.
// No randomness; player-side props only (cyan).
// The port:
// - The relays' state is exposed for a view (Game's RelayViews: the pylons, the orbs' pulse, the beams, the guide column,
//   the secret relay's faint pulse, as update() draws them on frame time): relays (id, name, spot, zone, lit), Secret,
//   Awake, Surge, NextRelay, GuideOn, Version (bumped on every Load).
// - The finishes are not ported: the game passes onComplete / onSecret = () => false, so the banners take their no-finish
//   text ("THE PROTOCOL IS COMPLETE · EVERY GUN FORGED", "A HIDDEN RELAY") and newFinish is false.
// - The SURGE goes through Director.AddSquads, which turns the kinds with no port yet (charger, lancer, warden) into husks
//   (Director.cs's header): the surge's 12 enemies come as husks and spitters until the roster (N13).
// - Load(relaySpots, secretRelay): null or an empty list = no quest (the web's `!!map?.relaySpots`; SurvivalMap always
//   has a list, empty when the map has none).
// - A relay's "open" test is zones.isOpen(zone) on the zone index found at load (Zones.IsLocked by index), which is the
//   same test without the per-call lookup: the buyables ask it every step.
// - NIGHT11 design.power (port-only): `powered` (null = always powered, the web's rule). The relays wake at the first
//   round begin with the power on and the round at least wakeRound.
// Events (the web's names and fields): relayLit {owner, id, name, count, total}, protocolSurge {owner},
// protocolComplete {owner, newFinish}, secretRelay {owner, id, newFinish}, banner {text}.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Skygrind.Core
{
    /// <summary>protocol.js relayLit {owner, id, name, count, total}.</summary>
    public sealed class RelayLitEvent
    {
        public string owner = "player";
        public string id;
        public string name;
        public int count;
        public int total;
    }

    /// <summary>protocol.js protocolSurge {owner}.</summary>
    public sealed class ProtocolSurgeEvent
    {
        public string owner = "player";
    }

    /// <summary>protocol.js protocolComplete {owner, newFinish}.</summary>
    public sealed class ProtocolCompleteEvent
    {
        public string owner = "player";
        public bool newFinish;
    }

    /// <summary>protocol.js secretRelay {owner, id, newFinish}.</summary>
    public sealed class SecretRelayEvent
    {
        public string owner = "player";
        public string id;
        public bool newFinish;
    }

    /// <summary>protocol.js summary(): {lit, total, complete}; `has` false where the web returns null (no quest run).</summary>
    public struct ProtocolSummary
    {
        public bool has;
        public int lit;
        public int total;
        public bool complete;
    }

    public sealed class Protocol : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: awake, the SURGE's stage, the switch, and which relays are lit (the secret one
        // too). The relays themselves (and their Buyables items) are the map's, built by StartRun's Load(map...): their
        // ids are checked. Version is bumped (the views redraw the lit beams).
        public void Save(StateWriter w)
        {
            var ids = new List<string>(relays.Count);
            var lit = new List<bool>(relays.Count);
            foreach (Relay r in relays)
            {
                ids.Add(r.id);
                lit.Add(r.lit);
            }
            w.Strings("relays", ids);
            w.Bools("lit", lit);
            w.S("secret", secret?.id);
            w.B("secretLit", secret != null && secret.lit);
            w.B("awake", awake);
            w.S("surge", surge);
            w.B("active", active);
        }

        public void Load(StateReader r)
        {
            List<string> ids = r.Strings("relays");
            List<bool> lit = r.Bools("lit");
            string secretId = r.S("secret");
            bool same = ids.Count == relays.Count && lit.Count == ids.Count && secretId == secret?.id;
            for (int i = 0; same && i < ids.Count; i++) same = ids[i] == relays[i].id;
            if (!same) throw new StateException("snapshot: protocol: the relays differ (restore after StartRun on the same map)");
            string s = r.S("surge");
            if (s != SurgeNone && s != SurgeQueued && s != SurgeLive && s != SurgeDone) throw new StateException($"snapshot: protocol.surge '{s}'");
            for (int i = 0; i < relays.Count; i++) relays[i].lit = lit[i];
            if (secret != null) secret.lit = r.B("secretLit");
            awake = r.B("awake");
            surge = s;
            active = r.B("active");
            Version++;
        }

        /// <summary>config.protocol, read once.</summary>
        public sealed class P
        {
            public bool enabled;
            public double wakeRound, bonus, reach, pylonHeight, beamHeight, guideHeight;
            public uint color, litColor;
            public readonly List<List<string>> surge = new List<List<string>>();

            public static P From(GameConfig c)
            {
                var p = new P
                {
                    enabled = c.Get("protocol.enabled") is bool on && on, wakeRound = c.Num("protocol.wakeRound"), bonus = c.Num("protocol.bonus"),
                    reach = c.Num("protocol.reach"), pylonHeight = c.Num("protocol.pylonHeight"), beamHeight = c.Num("protocol.beamHeight"),
                    guideHeight = c.Num("protocol.guideHeight"), color = (uint)c.Num("protocol.color"), litColor = (uint)c.Num("protocol.litColor"),
                };
                foreach (object s in c.Arr("protocol.surge")) p.surge.Add(PortData.Arr(s).ConvertAll(o => (string)o));
                return p;
            }
        }

        /// <summary>A relay: {id, name, spot, zone, lit} (the web's record, minus its meshes) and its buyable.</summary>
        public sealed class Relay
        {
            public string id;
            public string name;
            public Vec3 spot;
            public string zone; // zones.zoneAt(spot + 1 m up)
            public int zoneIndex; // that zone's index (-1: none)
            public bool lit;
            public bool secret; // the hidden fifth relay (map.secretRelay)
            public Buyables.Item buyable;
        }

        public const string SurgeNone = "none", SurgeQueued = "queued", SurgeLive = "live", SurgeDone = "done";

        readonly EventBus events;
        readonly Buyables buyables;
        readonly Zones zones;
        readonly Director director;
        readonly Forge forge;
        readonly IList<string> owned; // loadout.owned
        readonly Economy economy;
        readonly Func<SurvivalRun.State> getSurvivalState;
        readonly Func<bool> onComplete, onSecret;
        public readonly P c;
        static readonly Func<double> ZeroPrice = () => 0; // price: () => 0
        public readonly List<Relay> relays = new List<Relay>(); // {id, name, spot, zone, lit, buyable}
        Relay secret; // the hidden fifth relay (map.secretRelay), outside the quest's count
        bool awake;
        string surge = SurgeNone; // 'none' | 'queued' (lit in a break: it joins the next round) | 'live' (in this round) | 'done'
        bool active;

        /// <summary>NIGHT11 design.power (port-only): is the power on? Null: always (the web's rule).</summary>
        public Func<bool> powered;

        /// <summary>Bumped on every Load (a view rebuilds its relays).</summary>
        public int Version { get; private set; }

        public Protocol(GameConfig cfg, EventBus events, Buyables buyables, Zones zones, Director director, Forge forge, IList<string> owned, Economy economy,
            Func<SurvivalRun.State> survivalState, Func<bool> onComplete, Func<bool> onSecret)
        {
            this.events = events;
            this.buyables = buyables;
            this.zones = zones;
            this.director = director;
            this.forge = forge;
            this.owned = owned;
            this.economy = economy;
            getSurvivalState = survivalState;
            this.onComplete = onComplete ?? (() => false);
            this.onSecret = onSecret ?? (() => false); // onSecret = () => false
            c = P.From(cfg);
            events.On<RoundEvent>("roundBegin", e =>
            {
                if (!active || !c.enabled) return;
                if (!awake && e.number >= c.wakeRound && (powered == null || powered())) // NIGHT11: `powered` (design.power)
                {
                    awake = true;
                    events.Emit("banner", new BannerEvent { text = "THE PROTOCOL WAKES · LIGHT THE FOUR RELAYS" });
                }
                if (surge == SurgeQueued)
                {
                    director.AddSquads(SurgeSquads());
                    surge = SurgeLive;
                }
            });
            events.On("roundClear", _ =>
            {
                if (!active || surge != SurgeLive) return;
                surge = SurgeDone;
                for (int i = 0; i < owned.Count; i++) forge.Give(owned[i], 0); // for (const id of loadout.owned) forge.forge(id, 0)
                economy.Grant(c.bonus, "protocol");
                bool fresh = this.onComplete();
                events.Emit("protocolComplete", new ProtocolCompleteEvent { owner = "player", newFinish = fresh });
                events.Emit("banner", new BannerEvent { text = fresh ? "THE PROTOCOL IS COMPLETE · EVERY GUN FORGED · NEW FINISH: PROTOCOL" : "THE PROTOCOL IS COMPLETE · EVERY GUN FORGED" });
            });
        }

        int LitCount()
        {
            int n = 0;
            for (int i = 0; i < relays.Count; i++) if (relays[i].lit) n++;
            return n;
        }

        Relay Next()
        {
            for (int i = 0; i < relays.Count; i++) if (!relays[i].lit) return relays[i];
            return null;
        }

        /// <summary>c.surge.map((s) => [...s]): fresh copies for the director.</summary>
        List<List<string>> SurgeSquads()
        {
            var list = new List<List<string>>(c.surge.Count);
            foreach (List<string> s in c.surge) list.Add(new List<string>(s));
            return list;
        }

        /// <summary>zones.isOpen(r.zone), by the index found at load.</summary>
        public bool IsOpen(Relay r) => r.zoneIndex >= 0 && !zones.IsLocked(r.zoneIndex);

        void Clear()
        {
            foreach (Relay r in relays) buyables.Remove(r.buyable);
            if (secret != null) buyables.Remove(secret.buyable);
            relays.Clear();
            secret = null;
        }

        void StartSurge()
        {
            if (getSurvivalState() == SurvivalRun.State.Fight)
            {
                director.AddSquads(SurgeSquads());
                surge = SurgeLive;
            }
            else
            {
                surge = SurgeQueued;
            }
            events.Emit("protocolSurge", new ProtocolSurgeEvent { owner = "player" });
            events.Emit("banner", new BannerEvent { text = "SURGE · THE PROTOCOL ANSWERS" });
        }

        void Light(Relay r)
        {
            if (r.lit) return;
            r.lit = true;
            int count = LitCount();
            events.Emit("relayLit", new RelayLitEvent { owner = "player", id = r.id, name = r.name, count = count, total = relays.Count });
            Relay n = Next();
            events.Emit("banner", new BannerEvent
            {
                text = n != null
                    ? $"RELAY {count.ToString(CultureInfo.InvariantCulture)} / {relays.Count.ToString(CultureInfo.InvariantCulture)} LIT · NEXT: {n.name}"
                    : "ALL RELAYS LIT",
            });
            if (n == null) StartSurge();
        }

        void LightSecret(Relay r)
        {
            if (r.lit) return;
            r.lit = true;
            bool fresh = onSecret();
            events.Emit("secretRelay", new SecretRelayEvent { owner = "player", id = r.id, newFinish = fresh });
            events.Emit("banner", new BannerEvent { text = fresh ? "A HIDDEN RELAY · NEW FINISH: SKYLIGHT" : "A HIDDEN RELAY" });
        }

        Relay Build(RelaySpot spot, bool isSecret)
        {
            var r = new Relay
            {
                id = spot.id, name = spot.name, spot = spot.pos, lit = false, secret = isSecret,
                zone = zones.ZoneAt(new Vec3(spot.pos.x, spot.pos.y + 1, spot.pos.z)),
                zoneIndex = zones.TagAt(spot.pos.x, spot.pos.y, spot.pos.z), // the same point's zone index (y + 1)
            };
            r.buyable = new Buyables.Item
            {
                id = "relay:" + spot.id,
                kind = "relay",
                name = "RELAY · " + spot.name,
                position = new Vec3(spot.pos.x, spot.pos.y, spot.pos.z),
                reach = c.reach,
                price = ZeroPrice,
                available = isSecret ? (Func<bool>)(() => IsOpen(r)) : () => awake && IsOpen(r), // available() && zones.isOpen(zone)
                owned = () => r.lit,
                buy = isSecret ? (Action)(() => LightSecret(r)) : () => Light(r),
            };
            buyables.list.Add(r.buyable); // buyables.add
            return r;
        }

        /// <summary>Place the relays of a map (none clears) for a new run.</summary>
        public void Load(List<RelaySpot> relaySpots, RelaySpot? secretRelay)
        {
            Clear();
            awake = false;
            surge = SurgeNone;
            Version++;
            active = relaySpots != null && relaySpots.Count > 0 && c.enabled;
            if (!active) return;
            foreach (RelaySpot spot in relaySpots) relays.Add(Build(spot, false));
            if (secretRelay.HasValue) secret = Build(secretRelay.Value, true);
        }

        // The HUD's objective (hud(): {lit, total, next, surge} while the quest runs, else null), read without allocating.

        /// <summary>hud() is not null.</summary>
        public bool HudOn => active && awake && surge != SurgeDone;

        /// <summary>hud().lit.</summary>
        public int HudLit => LitCount();

        /// <summary>hud().total.</summary>
        public int HudTotal => relays.Count;

        /// <summary>hud().next: the next relay's name (null: all lit).</summary>
        public string HudNext => Next()?.name;

        /// <summary>hud().surge: the SURGE was called (queued or live).</summary>
        public bool HudSurge => surge != SurgeNone;

        public bool Complete => surge == SurgeDone;

        /// <summary>The results' line: summary() ({lit, total, complete}; has false outside a quest run).</summary>
        public ProtocolSummary Summary => active ? new ProtocolSummary { has = true, lit = LitCount(), total = relays.Count, complete = surge == SurgeDone } : default;

        // The view's state (update(): unlit relays pulse while awake, lit ones beam, the next one carries the tall guide).
        public bool Active => active;
        public bool Awake => awake;
        public string Surge => surge;
        public Relay Secret => secret;

        /// <summary>The next relay to light (null: all lit).</summary>
        public Relay NextRelay => Next();

        /// <summary>The guide column stands over the next relay (awake, the quest not done, one still dark).</summary>
        public bool GuideOn => active && awake && surge != SurgeDone && Next() != null;

        /// <summary>Debug: light one by id (presets); the secret relay too.</summary>
        public void DebugLight(string id)
        {
            Relay r = null;
            foreach (Relay x in relays)
            {
                if (x.id == id)
                {
                    r = x;
                    break;
                }
            }
            if (r != null) Light(r);
            else if (secret != null && secret.id == id) LightSecret(secret);
        }

        public void DebugWake() => awake = true;
    }
}
