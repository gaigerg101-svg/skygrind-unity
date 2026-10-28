// NIGHT13 U3-26.11 (agent F: the first-impression fixes S1-S12): the Unity-only UI numbers, each with a comment saying
// why the web has no value for it.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- S1: the lock mark (a padlock Image over a transparent placeholder glyph; UiMeta.PlaceLocks) ------------------
        // The padlock sprite's size (texture px; drawn once in code, tinted by the Image). The web drew U+25A1 with the
        // page's font, so it has no sprite.
        public const int PadlockWidth = 48;
        public const int PadlockHeight = 56;
        // The padlock's height as a share of the text's point size (about a cap height plus a little), and its centre's
        // rise over the baseline (a cap height's middle) in the same unit.
        public const float LockMarkEm = 0.78f;
        public const float LockMarkRiseEm = 0.36f;

        // --- S5: the POINTS label (bottom right; px at 1080p) --------------------------------------------------------------
        // hud.js lays the points and the weapon out with CSS flex (no numbers to port). The label's box, its gap to the
        // number (or to the points plate's left edge), and how much higher the weapon's name sits than before (it was
        // 4 px over the number's box, so the gold number read as its ammo).
        public const float PointsLabelWidth = 160f;
        public const float PointsLabelHeight = 24f;
        public const float PointsWeaponGap = 16f;

        // --- S6: the first STALE's tip (the hint toast; UiOnboarding.CheckStaleTip) ---------------------------------------
        // tips.js has no freshness tip (config.tips.list), so its id, text and count are port-only: the profile's tips key,
        // how many times a profile sees it (tips.times is 3 for the web's tips; this one says it once), and the text
        // ({action}: the live key, as tips.js fills its tips; ASCII only, inside the static atlases).
        public const string StaleTipId = "stale";
        public const double StaleTipTimes = 1;

        // --- S7: CONTRACTS (UiContracts) ------------------------------------------------------------------------------------
        // The web's CHALLENGES screen shows every badge with no reward column and has no dormant badges (its game emits every
        // event), so these texts are port-only: a row whose only reward is the badge itself; the line that counts the
        // dormant rows ({0}: how many; the reasons are Contracts.Dormant's: the Thumper, the ARCLINE, bosses, affixes, BOSS
        // RUSH); its gap under the last row (px at 1080p).
        public const float ContractFooterGap = 16f;

        // --- S9: the first-run lesson as one line (UiOnboarding) -----------------------------------------------------------
        // The web has no first-run script (design.onboarding is port-only), so none of this has a config.js value. The card's
        // widest (px at 1080p; it fits its line up to this) and its cyan tag.
        public const float LessonLineMaxWidth = 900f;
        // Each design.onboarding lesson (by id) as one line: what to press and what it pays, {action} = the live key (ASCII
        // only). design.onboarding.rounds keeps the full texts (an id missing here shows the lesson's title). These could
        // become a `line` field on those rounds; Core's Lesson doesn't read one yet.
        public static readonly System.Collections.Generic.Dictionary<string, string> LessonLines = new System.Collections.Generic.Dictionary<string, string>
        {
            ["moveShoot"] = "{forward}{left}{back}{right} MOVES, {jump} JUMPS: SHOOT THE HUSKS, EVERY HIT PAYS POINTS",
            ["dashSlide"] = "{dash} DASHES, {slide} SLIDES (SLAMS IN THE AIR): STYLISH MOVES RAISE YOUR POINTS",
            ["wallBuy"] = "HOLD {interact} AT A GUN ON THE WALL TO BUY IT WITH YOUR POINTS",
            ["perks"] = "HOLD {interact} AT A PERK MACHINE: AN UPGRADE FOR THE WHOLE RUN",
            ["powerProtocol"] = "HOLD {interact} AT THE POWER SWITCH (FREE) TO WAKE THE TRAPS AND THE RELAYS",
        };
        // What the player does that ends each lesson early (the thing it teaches): the player's first kill, a dash or a
        // slide (read from the mover), a gun bought (weaponBought), a perk, the power on. Otherwise a lesson lasts to the
        // next break.
        public const string DoneOnKill = "kill";
        public const string DoneOnDashSlide = "dashSlide";
        public const string DoneOnGun = "weaponBought";
        public const string DoneOnPerk = "perk";
        public const string DoneOnPower = "powerOn";
        public static readonly System.Collections.Generic.Dictionary<string, string> LessonDoneOn = new System.Collections.Generic.Dictionary<string, string>
        {
            ["moveShoot"] = DoneOnKill,
            ["dashSlide"] = DoneOnDashSlide,
            ["wallBuy"] = DoneOnGun,
            ["perks"] = DoneOnPerk,
            ["powerProtocol"] = DoneOnPower,
        };

        // --- S10: TITLE (Ui.cs, UiMeta's run setup) -------------------------------------------------------------------------
        // The column: PLAY's centre and the pitch between buttons (px from the screen's centre at 1080p; menus.css stacks
        // its buttons with flex gaps). 62 (54 px buttons, 8 px apart) keeps the column, the best grades under QUIT and the
        // controls line apart even with RESUME shown.
        public const float TitleButtonPitch = 62f;
        // One short line under each run-setup choice (the web's title has no DAILY mode button and no HEAT ladder, and its
        // SURVIVAL card's desc names bosses the port doesn't have), and its gap under the row's buttons (px).
        // TITLE's controls line ({action}: the live key, bindings.js fillKeys; the mouse buttons, the slot keys and Esc are
        // fixed, as keyboardMouse.js has them). menus.js shows its controls on the CONTROLS screen, not on the title, so the
        // line is port-only; it said "1-3 WEAPONS" while the hotbar has 1-0, the wheel and Q.
        // --- S12: PAUSED (Ui.cs: the button column and YOUR KEYS) ------------------------------------------------------
        // menus.js's pause panel is a CSS column with its key grid under it (no numbers to port). Px from the screen's
        // centre at 1080p: the button column's x, its first button's centre, the head's centre (the pitch is
        // TitleButtonPitch); the card's centre x, top, width, padding, head height, line height, the key column's width
        // and its gap to the label.
        // The bindings the card leaves out: the ARCLINE's (Survival's mover has ropeEnabled false; Contracts.Dormant).
        public static readonly string[] PauseKeysSkip = { "grapple", "reel" };
        // The fixed keys after the bindings (keyboardMouse.js; config.ui.mouseControls' keys, their labels shortened to
        // gamepad.js PAD_ACTION_LABELS' words where it has them), key then label, and their group's title. The aim lock (no
        // key) and photo mode (F2: the Arena only) are left out.

    }
}
