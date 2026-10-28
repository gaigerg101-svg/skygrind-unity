// NIGHT43 (the shared commit, the lead; slug43 / torch43 added in round 1 for W1): the fourteen sim-changing NIGHT43 design blocks a run reads, one bit each, so every NIGHT43 row can
// be switched off (NIGHT43.md method 3). With every block off the game is NIGHT42's bit for bit (`--off43` / `-skygrindOff43`; every older
// off switch also turns these off). Read once from Design/unity-config.json (each block's `enabled`); a run takes them at its start
// (SurvivalSim.N43), a tape records them (format 26: a VarI after the NIGHT42 value; formats 1-25 replay with every block off). The view
// rows (hud43, counter43, teach43, screens43, bossbar43, feel43, models43, look43) change only what is drawn: no bit (Game/N43View.cs).
// The blocks (GG 2026-10-16, verbatim in Research/2026-10-16-wave43/GG-REQUEST.md; DECIDE43):
// chimney43 (M1, G13: wall jumps refill on a new wall), ride43 (M1, G3: the ride smooth and forgiving), boost43 (M1, G26: jump + shot
// combine), foundry43 (L1, G1 / G2 / G4 / G10 / G11: level 1 as n43-foundry), gauntlet43 (W1, G17: the swing 1.25 s), parry43 (W1, G18:
// the orb parry intercepts farther), fusion43 (W1, G19: a high-speed impact), rooms43 (E1, G5 / G22: doors count their own room, stuck
// enemies move), iceline43 (E1, G23: level 2 as n43-iceline), kiln43 (K1, G25-G28: level 3 as n43-kiln), burrow43 (A1, G28: the
// BURROWER), wick43 (B1, G13-G16: THE WICK readable), slug43 (W1, G20: the charged slug hits like a cannon), torch43 (W1, G20: the
// TORCH ignites).
namespace Skygrind.Core
{
    public struct N43Blocks
    {
        public bool chimney43, ride43, boost43, foundry43, gauntlet43, parry43, fusion43, rooms43, iceline43, kiln43, burrow43, wick43, slug43, torch43;

        /// <summary>The number of blocks (the tape's NIGHT43 value, bit i = block i).</summary>
        public const int Count = 14;

        /// <summary>The harnesses' and the Player checks' switch (`--off43`, `-skygrindOff43`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT42's game.</summary>
        public static N43Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => Bits != 0;

        /// <summary>The names in bit order (logs, the harness's header line, the design rows `design.n43.<name>`).</summary>
        public static readonly string[] Names = { "chimney43", "ride43", "boost43", "foundry43", "gauntlet43", "parry43", "fusion43", "rooms43", "iceline43", "kiln43", "burrow43", "wick43", "slug43", "torch43" };

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT42's config).</summary>
        public static N43Blocks From(GameConfig cfg)
        {
            var b = new N43Blocks();
            for (int i = 0; i < Count; i++) b.Set(i, cfg.Design("n43." + Names[i] + ".enabled"));
            return b;
        }

        /// <summary>Block i in Names order.</summary>
        public bool Flag(int i)
        {
            switch (i)
            {
                case 0: return chimney43;
                case 1: return ride43;
                case 2: return boost43;
                case 3: return foundry43;
                case 4: return gauntlet43;
                case 5: return parry43;
                case 6: return fusion43;
                case 7: return rooms43;
                case 8: return iceline43;
                case 9: return kiln43;
                case 10: return burrow43;
                case 11: return wick43;
                case 12: return slug43;
                case 13: return torch43;
                default: return false;
            }
        }

        void Set(int i, bool v)
        {
            switch (i)
            {
                case 0: chimney43 = v; break;
                case 1: ride43 = v; break;
                case 2: boost43 = v; break;
                case 3: foundry43 = v; break;
                case 4: gauntlet43 = v; break;
                case 5: parry43 = v; break;
                case 6: fusion43 = v; break;
                case 7: rooms43 = v; break;
                case 8: iceline43 = v; break;
                case 9: kiln43 = v; break;
                case 10: burrow43 = v; break;
                case 11: wick43 = v; break;
                case 12: slug43 = v; break;
                case 13: torch43 = v; break;
            }
        }

        /// <summary>The blocks as a value (bit i = block i): what a tape records.</summary>
        public int Bits
        {
            get
            {
                int b = 0;
                for (int i = 0; i < Count; i++)
                    if (Flag(i)) b |= 1 << i;
                return b;
            }
        }

        /// <summary>The blocks from a tape's value.</summary>
        public static N43Blocks FromBits(int b)
        {
            var r = new N43Blocks();
            for (int i = 0; i < Count; i++) r.Set(i, (b & (1 << i)) != 0);
            return r;
        }

        public override string ToString()
        {
            if (!Any) return "every NIGHT43 block off";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (!Flag(i)) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(Names[i]);
            }
            return sb.ToString();
        }
    }

    /// <summary>NIGHT43 (the shared commit, the lead): the bus payloads NIGHT43's sim emits, fixed before the builders start so the emitters
    /// and the views build in parallel (W1 emits fusion43, F1 draws it; A1 emits the burrower's beats; B1 the WICK's leg beats). Every name
    /// is also an event for GG's sounds (CLAUDE.md rule 7). A payload is made once and reused by its emitter (no allocation per step).</summary>
    public static class N43Events
    {
        /// <summary>Port-only: a FUSION, a gauntlet hit or punch landed at high speed (payload Fusion43Event).</summary>
        public const string Fusion = "fusion43";
        /// <summary>Port-only: a BURROWER's tell under the floor started (payload Burrow43Event).</summary>
        public const string BurrowTell = "burrowTell43";
        /// <summary>Port-only: a BURROWER broke the surface (payload Burrow43Event).</summary>
        public const string BurrowEmerge = "burrowEmerge43";
        /// <summary>Port-only: a BURROWER was hit inside its emergence window (payload Burrow43Event).</summary>
        public const string BurrowCounter = "burrowCounter43";
        /// <summary>Port-only: a boss changed leg / phase (payload Boss43Event).</summary>
        public const string BossLeg = "bossLeg43";
        /// <summary>Port-only: a boss became open to full damage, or closed again (payload Boss43Event).</summary>
        public const string BossOpen = "bossOpen43";
        /// <summary>Port-only: a locked room's last enemy died and its door opens (payload: the room id, a string).</summary>
        public const string RoomClear = "roomClear43";
    }

    /// <summary>A FUSION: where, the player's speed at the hit (m/s), the weapon ("gauntlet" / "punch"), the enemy and its kind, whether it
    /// killed, the damage dealt.</summary>
    public sealed class Fusion43Event
    {
        public string owner = "player";
        public Vec3 position, dir;
        public double speed;
        public string weapon;
        public int enemy;
        public string kind;
        public bool killed;
        public double damage;
    }

    /// <summary>A BURROWER beat: its position, the seconds the beat lasts (a tell's warning, an emergence's window).</summary>
    public sealed class Burrow43Event
    {
        public int enemy;
        public Vec3 position;
        public double seconds;
    }

    /// <summary>A boss beat: the boss's name, its leg (1-based) of legs, open or closed.</summary>
    public sealed class Boss43Event
    {
        public string boss;
        public int leg, legs;
        public bool open;
    }

    /// <summary>NIGHT43 (the shared commit): what the boss bar draws (SurvivalSim.N43BossState; B1 fills it for THE WICK, others may for
    /// theirs). valid false = no boss bar. hp01 the fraction left; lockAt the HP fraction it cannot pass this leg (0 = none); open = full
    /// damage now; stagger01 the stagger meter (or -1); leg / legs; hint = what hurts it now (short, the HUD's words, glyph tokens allowed).</summary>
    public struct Boss43State
    {
        public bool valid;
        public string name;
        public double hp01, lockAt, stagger01, openLeft;
        public bool open, locked;
        public int leg, legs;
        public string hint;
    }
}
