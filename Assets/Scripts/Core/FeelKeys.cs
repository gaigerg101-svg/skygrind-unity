// NIGHT16 U3-29.3 (the shared commit): the feel night's names in one place. Every builder reads these, nobody edits them
// but the lead:
// - the events each new moment emits (PORT-STATUS.md's event list): the web's `weaponReady` (viewmodel.js :850, the
//   cooldown-ready glint), and the port-only `impactFreeze`, `slowMo`, `slideCharged`, `weaponDraw`;
// - their payloads (fields named for GG's audio map; reused instances: read them inside the handler, don't keep them);
// - the settings keys: HIT FREEZE / SLOW-MO (design.impact.settings) and the LAB tab's three movement rows
//   (design.move.*.enabled; unlike the LAB toggles they are on by default and never stamp a run LAB).
// No UnityEngine use: Core.
namespace Skygrind.Core
{
    public static class FeelKeys
    {
        // --- events -------------------------------------------------------------------------------------------------
        /// <summary>Port-only: a freeze the impact layer started (ImpactFreezeEvent).</summary>
        public const string ImpactFreeze = "impactFreeze";

        /// <summary>Port-only: a slow-mo started (SlowMoEvent).</summary>
        public const string SlowMo = "slowMo";

        /// <summary>Port-only: the slide boost's charge came back (design.move.slideCharge; payload: the owner id).</summary>
        public const string SlideCharged = "slideCharged";

        /// <summary>Port-only: a gun's draw started on the viewmodel (payload: the weapon id).</summary>
        public const string WeaponDraw = "weaponDraw";

        /// <summary>The web's (viewmodel.js :850): a slow gun's cycle ended, the glint (payload: the weapon id).</summary>
        public const string WeaponReady = "weaponReady";

        // --- settings ------------------------------------------------------------------------------------------------
        /// <summary>HIT FREEZE: "off" | "light" | "full" (design.impact.settings.hitFreezeDefault).</summary>
        public const string HitFreezeSetting = "hitFreeze";

        public const string HitFreezeOff = "off", HitFreezeLight = "light", HitFreezeFull = "full";

        /// <summary>SLOW-MO: on / off (design.impact.settings.slowMoDefault; REDUCED MOTION turns it off too).</summary>
        public const string SlowMoSetting = "slowMo";

        /// <summary>The LAB tab's movement rows (design.move.slideCharge / slideDecay / hopFalloff .enabled).</summary>
        public const string MoveSlideChargeSetting = "move.slideCharge", MoveSlideDecaySetting = "move.slideDecay", MoveHopFalloffSetting = "move.hopFalloff";
    }

    /// <summary>Port-only `impactFreeze`: a freeze the impact layer started (or skipped, over its budget or cooldown).</summary>
    public sealed class ImpactFreezeEvent
    {
        public string owner; // "player"
        public string cause; // kill | heavyHit | headshot | headshotKill | punchHit | punchKill | parry | perfectParry
        public double seconds; // the freeze granted (0 when skipped)
        public bool skipped; // over the budget or inside the tier's cooldown
    }

    /// <summary>Port-only `slowMo`: a slow-mo started.</summary>
    public sealed class SlowMoEvent
    {
        public string owner; // "player"
        public string cause; // lastKill | perfectParry | multiKill
        public double scale; // the time scale held (0.3 = three tenths of real time)
        public double seconds; // the hold, in real seconds (the ease back comes after)
    }
}
