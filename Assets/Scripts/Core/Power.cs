// NIGHT11 U3-24.6: THE POWER SWITCH (design.power). New design, not a web port: the web game never had one. The
// reference is Call of Duty Zombies' power switch (NIGHT11-RESEARCH.md A.1 and C.3): one switch a few doors deep turns
// several systems on at once, the run's first goal. Here it wakes the web's own "power" systems (traps.js's traps and
// protocol.js's relays); THE FORGE stays open from round 1 (the web's rule, as BO6's Liberty Falls keeps its Pack-a-Punch).
// - Flag off (design.power.enabled false, or no design config): no switch, no event, and Powered is always true (the
//   web's rules: traps always buyable, the PROTOCOL awake at protocol.wakeRound).
// - Flag on: the switch is a Buyables item (hold E, free, perks.reach) at design.power.spot while the power is off;
//   throwing it sets On, emits `powerOn {owner}` (port-only) and the web's `banner {text}`. A new run starts with it off.
// The Game wires the effects: the traps' and the PROTOCOL's `powered` hooks read Powered. Power builds its own Buyables
// item (Item, null with the flag off); the Game adds it to its Buyables list. Nothing here runs per step.
using System;

namespace Skygrind.Core
{
    /// <summary>powerOn (port-only): who threw the switch.</summary>
    public sealed class PowerEvent
    {
        public string owner = "player";
    }

    public sealed class Power : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the switch thrown or not (no powerOn event on load).
        public void Save(StateWriter w) => w.B("on", On);

        public void Load(StateReader r) => On = r.B("on");

        /// <summary>The Buyables item id of the switch.</summary>
        public const string ItemId = "power";

        /// <summary>The banner the throw shows (the web's `banner {text}`).</summary>
        public const string BannerText = "POWER ON · THE TRAPS AND THE RELAYS WAKE";

        /// <summary>design.power.enabled: false (or no design config) means no switch at all, the web's game.</summary>
        public static bool Enabled(GameConfig cfg) => cfg.Design("power.enabled");

        readonly EventBus events;
        readonly PowerEvent powerEvent = new PowerEvent();
        readonly BannerEvent banner = new BannerEvent { text = BannerText };

        /// <summary>design.power.enabled, read once.</summary>
        public readonly bool enabled;

        /// <summary>The switch's spot (design.power.spot: feet, the yaw its front faces as (sin yaw, 0, cos yaw), its zone id).</summary>
        public readonly BoxSpot spot;

        /// <summary>The switch as a Buyables item (hold E, free, perks.reach, at the spot); null with the flag off.</summary>
        public readonly Buyables.Item Item;

        /// <summary>The switch has been thrown this run.</summary>
        public bool On { get; private set; }

        /// <summary>Optional: is zone `id` open (the switch is buyable only in an open zone)? Null: always.</summary>
        public Func<string, bool> zoneOpen;

        /// <param name="mapSpots">NIGHT15: a Unity-authored map's spots (SurvivalMap.spots): its "power" {x, y, z, yaw, zone} replaces design.power.spot.</param>
        public Power(GameConfig cfg, EventBus events, System.Collections.Generic.Dictionary<string, object> mapSpots = null)
        {
            this.events = events;
            enabled = Enabled(cfg);
            if (!enabled) return;
            if (mapSpots != null && mapSpots.TryGetValue("power", out object mp) && mp != null)
            {
                var s = PortData.Obj(mp);
                spot = new BoxSpot { pos = new Vec3(PortData.Num(s["x"]), PortData.Num(s["y"]), PortData.Num(s["z"])), yaw = PortData.Num(s["yaw"]), zone = (string)s["zone"] };
            }
            else spot = new BoxSpot
            {
                pos = new Vec3(cfg.Num("design.power.spot.x"), cfg.Num("design.power.spot.y"), cfg.Num("design.power.spot.z")),
                yaw = cfg.Num("design.power.spot.yaw"),
                zone = cfg.Str("design.power.spot.zone"),
            };
            Item = new Buyables.Item
            {
                id = ItemId, name = "POWER", kind = "power", position = spot.pos, reach = cfg.Num("perks.reach"),
                price = Free, available = CanThrow, buy = ThrowFromBuy,
            };
        }

        static double Free() => 0;

        void ThrowFromBuy() => Throw();

        /// <summary>The traps and the relays have power: always with the flag off (the web's rules), else once thrown.</summary>
        public bool Powered => !enabled || On;

        /// <summary>The switch can be thrown now (the Buyables item's `available`).</summary>
        public bool CanThrow() => enabled && !On && (zoneOpen == null || zoneOpen(spot.zone));

        /// <summary>Throw the switch: On, `powerOn` and the banner. False (nothing happens) with the flag off or when already on.</summary>
        public bool Throw(string owner = "player")
        {
            if (!enabled || On) return false;
            On = true;
            powerEvent.owner = owner;
            events.Emit("powerOn", powerEvent);
            events.Emit("banner", banner);
            return true;
        }

        /// <summary>A new run: the power is off.</summary>
        public void Reset() => On = false;
    }
}
