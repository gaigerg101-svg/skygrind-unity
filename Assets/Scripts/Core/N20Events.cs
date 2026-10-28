// NIGHT20 (the shared commit, the lead): the bus payloads NIGHT20's sim emits, fixed before the builders start so the
// emitters (G: the shockwave; N: the climb; E: the round heal, the special rounds) and the views (V: the shockwave's air,
// the climb's dust; H: the cooldown ring, the climb arrow, the refill, the slabs) build in parallel. Every name is also an
// event for GG's sounds (CLAUDE.md rule 7: PORT-STATUS.md's event list). The web's names where the web has the event
// (weapons.js: `doubleBlast`, `coneWave`; the `heal` of a round's end); port-only otherwise. A payload is made once and
// reused by its emitter (no allocation per step).
namespace Skygrind.Core
{
    public static class N20Events
    {
        /// <summary>Port-only: the THUNDERCLAP's shockwave fired (payload ShockwaveEvent).</summary>
        public const string Shockwave = "shockwave";
        /// <summary>Port-only: a shockwave charge came back (payload: the charges now, an int).</summary>
        public const string ShockwaveReady = "shockwaveReady";
        /// <summary>weapons.js :606-637: the THUNDERCLAP's double blast (payload ShockwaveEvent).</summary>
        public const string DoubleBlast = "doubleBlast";
        /// <summary>weapons.js: the shock cone's wave (payload ShockwaveEvent).</summary>
        public const string ConeWave = "coneWave";
        /// <summary>Port-only: a climber started up a face, or reached its top (payload ClimbEvent).</summary>
        public const string Climb = "climb";
        /// <summary>Port-only: the round-end refill started (payload RoundHealEvent).</summary>
        public const string RoundHeal = "roundHeal";
        /// <summary>Port-only: an earned heal (a close kill, a punch kill) (payload RoundHealEvent, kind set).</summary>
        public const string EarnedHeal = "earnedHeal";
        /// <summary>Port-only: a special round was announced or started (payload SpecialRoundEvent).</summary>
        public const string SpecialRound = "specialRound";
    }

    /// <summary>The shockwave: where it left from, the aim it pushed along (unit), the player's push (m/s, opposite the aim),
    /// how many enemies it pushed, the charges left and the cooldown's seconds to the next charge.</summary>
    public sealed class ShockwaveEvent
    {
        public string owner = "player";
        public Vec3 origin, dir;
        public double selfPush;
        public int pushed;
        public int chargesLeft;
        public double cooldownLeft;
    }

    /// <summary>A climb: the husk's id and kind, the face's foot and lip (world), the phase ("start": the tell began; "up":
    /// the climb began; "top": it mantled; "knocked": a punch knocked it off) and the link kind (4-7, Core/Nav/RouteLinks.cs).</summary>
    public sealed class ClimbEvent
    {
        public int enemy;
        public string kind;
        public Vec3 foot, lip;
        public string phase;
        public int linkKind;
    }

    /// <summary>A heal the NIGHT20 regen gave: the round-end refill ("round"), or an earned heal ("closeKill", "punchKill"),
    /// with the HP it will restore and over how many seconds (0: at once).</summary>
    public sealed class RoundHealEvent
    {
        public string kind = "round";
        public double amount;
        public double seconds;
        public int round;
    }

    /// <summary>A special round: its name ("hunt", "elite", "fog", ...), the round number, the phase ("announce", "start",
    /// "last", "end") and the enemies it holds (0 when not known).</summary>
    public sealed class SpecialRoundEvent
    {
        public string name;
        public int round;
        public string phase;
        public int count;
    }
}
