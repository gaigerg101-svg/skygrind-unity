// NIGHT61 builder G (S3 bossRule61; view rows in design.n61): Unity-only numbers (CLAUDE.md rule 5: UI timing and the reel's floors, one
// comment each). The sim never reads them (golden vectors, replays and fingerprints untouched). The rule line's SIZE is hud47's fight line's
// (UnityTuning.Hud47FightPx: 40 px em at 1080p, 26.7 px at 720p, x the TEXT SIZE setting): the line is drawn by that one label, so it adds no
// size of its own.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- S3 bossRule61: the boss's rule line at its fight's start (BossRule61.cs, UiBossRule61.cs) ---

        /// <summary>s after the boss's rise the rule line comes in under its name card: entrance41's moment (Entrance41LineAt 0.45 s: after the
        /// name's flicker-in, while the shake fades), so the name lands first and the rule reads as its subtitle. NIGHT63 G P2: counted from the
        /// rise whether or not the boss bar is up (THE KILN WYRM's bar waits 2.0 s for its emerge), so the line stands by the reel's 1.0 s frame
        /// (BossRule61DelayS + BossRule61InS &lt;= BossRule61ShotS, N63BossRuleTimeTests).</summary>
        public const double BossRule61DelayS = 0.45;

        /// <summary>s the line fades in (hud47's fight line: 0.12 s).</summary>
        public const double BossRule61InS = 0.12;

        /// <summary>The line's scale as it begins, eased to 1 over the fade-in (hud47's fight line: from 1.08).</summary>
        public const float BossRule61ScaleFrom = 1.08f;

        /// <summary>s the line fades out at its end (hud47's fight line: 0.25 s).</summary>
        public const double BossRule61OutS = 0.25;

        /// <summary>s the line is on screen at most, its fades included (the slate's S3 doneWhen: "on screen &lt;= 5 s"): the hold (the text
        /// system's 2.0 s + 0.3 s a word, Hud47LineHoldS / Hud47LineHoldPerWord) is cut to fit.</summary>
        public const double BossRule61MaxS = 5.0;

        /// <summary>Words a rule line may have, a {button} glyph counting as one (the slate's S3: "&lt;= 8 words"); a longer line is refused,
        /// never shown.</summary>
        public const int BossRule61MaxWords = 8;

        /// <summary>The boss61 reel's floors for the line's em in screen px (the slate's S3: &gt;= 36 at 1080p and &gt;= 24 at 720p, a148's G5
        /// measure, Ui.W48FightLine).</summary>
        public const float BossRule61MinEm1080 = 36f, BossRule61MinEm720 = 24f;

        /// <summary>s into the fight the boss61 reel takes each boss's frame (the slate's S3: "a reel frame per boss 1.0 s into the fight").</summary>
        public const double BossRule61ShotS = 1.0;

        // --- C1 text61: one line of text at a time; no repeated instruction outside level 1 (Text61.cs, UiText61.cs) ---

        /// <summary>s a new instruction waits at most for the room's title to leave the message slot (outside level 1): a title stands 2-3 s
        /// (MessageSlot33's posts); past this the instruction shows anyway (and its proof line says LATE), so a long title never eats a lesson.</summary>
        public const double Text61WaitS = 3.0;
    }
}
