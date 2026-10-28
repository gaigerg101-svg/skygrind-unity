// NIGHT24 (the shared commit, the lead): the nine NIGHT24 design blocks a run reads, one bit each, so every NIGHT24 feature can be
// switched off (NIGHT24.md "Switches"). With every block off the game is NIGHT22's bit for bit: the A side for every judge
// (`--off24` / `-skygrindOff24`); `--off22`, `--off21`, `--off20` and `--off19` also turn these off. Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.n24), a tape records them
// (format 8: two bytes, then the campaign state; formats 1-7 replay with every block off) and the harnesses force them off with ForceOff.
// The blocks: design.n24.padY (R: Y = NEXT WEAPON, pad layout v4), shotgunTune (R: the slower THUNDERCLAP), bigPick (F: the BIG PICK),
// chargeFist (F: the hit-fed fist and its flick), reticleArc (F: the cooldown arc under the reticle), campaign (L: MODE: CAMPAIGN,
// rooms and doors), wyrm (B: the worm boss), lighting (L: the room lighting kit), prewarm (the lead: the boot prewarm pass).
namespace Skygrind.Core
{
    public struct N24Blocks
    {
        public bool padY, shotgunTune, bigPick, chargeFist, reticleArc, campaign, wyrm, lighting, prewarm;

        /// <summary>The number of blocks (the tape's NIGHT24 value, bit i = block i; two bytes).</summary>
        public const int Count = 9;

        /// <summary>The harnesses' and the Player checks' switch (`--off24`, `-skygrindOff24`; also `--off22`, `--off21`, `--off20`,
        /// `--off19`): every block off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT22's game.</summary>
        public static N24Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => padY || shotgunTune || bigPick || chargeFist || reticleArc || campaign || wyrm || lighting || prewarm;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (shotgunTune, bigPick, chargeFist;
        /// the campaign and the wyrm change only their own mode's run.)</summary>
        public bool SimChanging => shotgunTune || bigPick || chargeFist;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT22's config).</summary>
        public static N24Blocks From(GameConfig cfg) => new N24Blocks
        {
            padY = cfg.Design("n24.padY.enabled"),
            shotgunTune = cfg.Design("n24.shotgunTune.enabled"),
            bigPick = cfg.Design("n24.bigPick.enabled"),
            chargeFist = cfg.Design("n24.chargeFist.enabled"),
            reticleArc = cfg.Design("n24.reticleArc.enabled"),
            campaign = cfg.Design("n24.campaign.enabled"),
            wyrm = cfg.Design("n24.wyrm.enabled"),
            lighting = cfg.Design("n24.lighting.enabled"),
            prewarm = cfg.Design("n24.prewarm.enabled"),
        };

        /// <summary>Block i (0 padY, 1 shotgunTune, 2 bigPick, 3 chargeFist, 4 reticleArc, 5 campaign, 6 wyrm, 7 lighting, 8 prewarm).</summary>
        public bool Flag(int i) => i == 0 ? padY : i == 1 ? shotgunTune : i == 2 ? bigPick : i == 3 ? chargeFist : i == 4 ? reticleArc
            : i == 5 ? campaign : i == 6 ? wyrm : i == 7 ? lighting : i == 8 && prewarm;

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
        public static N24Blocks FromBits(int b) => new N24Blocks
        {
            padY = (b & 1) != 0, shotgunTune = (b & 2) != 0, bigPick = (b & 4) != 0, chargeFist = (b & 8) != 0, reticleArc = (b & 16) != 0,
            campaign = (b & 32) != 0, wyrm = (b & 64) != 0, lighting = (b & 128) != 0, prewarm = (b & 256) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "padY", "shotgunTune", "bigPick", "chargeFist", "reticleArc", "campaign", "wyrm", "lighting", "prewarm" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT24 block off";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (!Flag(i)) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(Names[i]);
            }
            return sb.Append(" on").ToString();
        }
    }

    /// <summary>NIGHT24: the campaign's run state a tape (format 8) records and a replay restores: the level, the room the player is
    /// in, the last checkpoint, and the LIMIT flags in force (bit i = limit i; the rooms' format lists them). Empty (no level) for
    /// SURVIVAL and THE GRIND. Builder L owns what the fields mean; the lead owns the plumbing.</summary>
    public struct Campaign24State
    {
        public string levelId; // "" or null: no campaign level
        public int room;
        public int checkpoint;
        public int limits;

        public bool Any => !string.IsNullOrEmpty(levelId);
        public static Campaign24State None => new Campaign24State { levelId = "", room = 0, checkpoint = 0, limits = 0 };

        public bool Same(Campaign24State o) => (levelId ?? "") == (o.levelId ?? "") && room == o.room && checkpoint == o.checkpoint && limits == o.limits;
    }
}
