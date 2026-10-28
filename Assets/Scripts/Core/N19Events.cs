// NIGHT19 (the shared commit, the lead): the bus payloads NIGHT19's sim emits, fixed before the builders start so the
// emitters (W: the gust, the ported guns, the combos; E: the new kinds) and the views (V: the gust's air, the arcs, the
// grenades; H: the combo callouts) build in parallel. Every name is also an event for GG's sounds (CLAUDE.md rule 7:
// PORT-STATUS.md's event list). The web's names where the web has the event (guns.js: `zap`, `discharge`); port-only
// otherwise. A payload is made once and reused by its emitter (no allocation per step).
namespace Skygrind.Core
{
    public static class N19Events
    {
        /// <summary>Port-only: the KESTREL's wind gust fired (payload GustEvent).</summary>
        public const string Gust = "gust";
        /// <summary>guns.js :161: the ZAPPER's arc (payload ZapEvent: the chain's points, the hits).</summary>
        public const string Zap = "zap";
        /// <summary>guns.js :421: the ZAPPER's alt, the discharge (payload ZapEvent: the stunned enemies' points).</summary>
        public const string Discharge = "discharge";
        /// <summary>Port-only: a named combo landed (payload ComboEvent: POP, CONDUCTOR, LAUNCH + SNIPE, SWAP-CANCEL, ...).</summary>
        public const string Combo = "combo";
        /// <summary>Port-only: a SHIELDED front plate broke (payload: the enemy id, an int).</summary>
        public const string PlateBreak = "plateBreak";
    }

    /// <summary>The gust: where it left from, the aim it pushed along (unit), the player's push (m/s, opposite the aim),
    /// how many enemies it pushed, and whether it fired in the air (its one air charge).</summary>
    public sealed class GustEvent
    {
        public string owner = "player";
        public Vec3 origin, dir;
        public double selfPush;
        public int pushed;
        public bool airborne;
    }

    /// <summary>The ZAPPER's arc (or the discharge's points): points[0 .. count) from the muzzle along the chain.</summary>
    public sealed class ZapEvent
    {
        public string owner = "player";
        public readonly Vec3[] points = new Vec3[16];
        public int count;
        public int hits;
    }

    /// <summary>A combo: its name (the callout's text, upper case), the payoff gun, where it landed, and the bonus it paid
    /// (a multiplier on the payoff's damage or points; 1 when it pays in kind).</summary>
    public sealed class ComboEvent
    {
        public string owner = "player";
        public string name;
        public string weapon;
        public Vec3 at;
        public double bonus = 1;
    }
}
