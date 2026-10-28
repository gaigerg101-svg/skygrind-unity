// NIGHT42 (the shared commit, the lead): the nine sim-changing NIGHT42 design blocks a run reads, one bit each, so every NIGHT42 row can
// be switched off (NIGHT42.md method 3). With every block off the game is NIGHT41's bit for bit (`--off42` / `-skygrindOff42`; every older
// off switch also turns these off). Read once from Design/unity-config.json (each block's `enabled`); a run takes them at its start
// (SurvivalSim.N42), a tape records them (format 25: a VarI after the NIGHT41 value; formats 1-24 replay with every block off). The view
// rows (reticle42, meter42, style42, health42, teach42, kills42) change only what is drawn: no bit (Game/N42View.cs).
// The blocks (GG 2026-10-14 ~18:35, verbatim in Research/2026-10-14-wave42/GG-REQUEST.md; DECIDE42):
// wallfling42 (M1, G2: a jump near a wall no longer flings), boost42 (M1, G5: the THUNDERCLAP boost easier), foundry42 (L1, G1: level 1
// as n42-foundry, THE GANTRY forgiving), gauntlet42 (W1, G3: the swing 2.5 -> 1.8 s), parry42 (W1, G4: the DRONE orb's parry forgives),
// charge42 (W1, G10 / G8: a held fire starts the next charge; the attack waits for the reticle), impact42 (F1, G9 / G15: hits and kills
// land harder), brawl42 (B1, G14: THE ICEBREAKER's new attacks), arena42 (K1, G12: arena elements that change play).
namespace Skygrind.Core
{
    public struct N42Blocks
    {
        public bool wallfling42, boost42, foundry42, gauntlet42, parry42, charge42, impact42, brawl42, arena42;

        /// <summary>The number of blocks (the tape's NIGHT42 value, bit i = block i).</summary>
        public const int Count = 9;

        /// <summary>The harnesses' and the Player checks' switch (`--off42`, `-skygrindOff42`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT41's game.</summary>
        public static N42Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => Bits != 0;

        /// <summary>The names in bit order (logs, the harness's header line, the design rows `design.n42.<name>`).</summary>
        public static readonly string[] Names = { "wallfling42", "boost42", "foundry42", "gauntlet42", "parry42", "charge42", "impact42", "brawl42", "arena42" };

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT41's config).</summary>
        public static N42Blocks From(GameConfig cfg)
        {
            var b = new N42Blocks();
            for (int i = 0; i < Count; i++) b.Set(i, cfg.Design("n42." + Names[i] + ".enabled"));
            return b;
        }

        /// <summary>Block i in Names order.</summary>
        public bool Flag(int i)
        {
            switch (i)
            {
                case 0: return wallfling42;
                case 1: return boost42;
                case 2: return foundry42;
                case 3: return gauntlet42;
                case 4: return parry42;
                case 5: return charge42;
                case 6: return impact42;
                case 7: return brawl42;
                case 8: return arena42;
                default: return false;
            }
        }

        void Set(int i, bool v)
        {
            switch (i)
            {
                case 0: wallfling42 = v; break;
                case 1: boost42 = v; break;
                case 2: foundry42 = v; break;
                case 3: gauntlet42 = v; break;
                case 4: parry42 = v; break;
                case 5: charge42 = v; break;
                case 6: impact42 = v; break;
                case 7: brawl42 = v; break;
                case 8: arena42 = v; break;
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
        public static N42Blocks FromBits(int b)
        {
            var r = new N42Blocks();
            for (int i = 0; i < Count; i++) r.Set(i, (b & (1 << i)) != 0);
            return r;
        }

        public override string ToString()
        {
            if (!Any) return "every NIGHT42 block off";
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
}
