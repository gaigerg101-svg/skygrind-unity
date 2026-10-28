// NIGHT12 (the shared types): the event names and payloads tonight's systems emit. The web's names are kept where the web
// has the event (challengeDone, mutatorUnlock, fx:killConfirm); the rest are port-only (listed in PORT-STATUS.md's event
// list so GG can bind sounds: CLAUDE.md rule 7). Every ceremony and FX fires on one of these; none plays audio.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class MetaEvents
    {
        /// <summary>challenges.js: a web badge reached its target. ChallengeEvent.</summary>
        public const string ChallengeDone = "challengeDone";
        /// <summary>Port-only: any contract (a web badge or a port-only one) completed. ContractEvent.</summary>
        public const string ContractDone = "contractDone";
        /// <summary>Port-only: an unlock flag earned for the first time (unlocks.js set returned true). UnlockEvent.</summary>
        public const string UnlockEarned = "unlockEarned";
        /// <summary>mutators.js: a mutator unlocked by a new best round. MutatorEvent.</summary>
        public const string MutatorUnlock = "mutatorUnlock";
        /// <summary>Port-only: a gun's MASTERY tier reached (its finish unlocked). MasteryEvent.</summary>
        public const string MasteryTier = "masteryTier";
        /// <summary>Port-only: an EXFIL on HEAT rung N opened rung N + 1. HeatEvent.</summary>
        public const string HeatUnlock = "heatUnlock";
        /// <summary>Port-only: the round ceremony's beats (the number flashes at the clear; the new one burns in). RoundCeremonyEvent.</summary>
        public const string RoundCeremony = "roundCeremony";
        /// <summary>Port-only: RESULTS' staged tally reached a beat (outcome, round, card, grade, best, line). ResultsStageEvent.</summary>
        public const string ResultsStage = "resultsStage";
        /// <summary>killConfirm.js: a kill ring spawned (frame code, an audio hook). KillConfirmEvent.</summary>
        public const string KillConfirm = "fx:killConfirm";
        /// <summary>Port-only: a tip shown (tips.js moment returned true). TipEvent.</summary>
        public const string Tip = "tip";
        /// <summary>Port-only: the first-run script moved to a round's lesson. OnboardingEvent.</summary>
        public const string OnboardingStep = "onboardingStep";
    }

    public sealed class ChallengeEvent
    {
        public string id, name;
    }

    public sealed class ContractEvent
    {
        public string id, name;
        public string unlock; // the flag it grants (null: none)
        public string unlockName; // what the flag opens, for the toast ("FINISH: GRAPHITE"), or null
        public bool web; // one of challenges.js's 24
    }

    public sealed class UnlockEvent
    {
        public string flag, name;
    }

    public sealed class MutatorEvent
    {
        public string id;
    }

    public sealed class MasteryEvent
    {
        public string weapon; // the catalog id
        public int tier; // 1..3
        public string finish; // the finish it unlocked (null: none)
    }

    public sealed class HeatEvent
    {
        public int rung; // the rung now open
    }

    public sealed class RoundCeremonyEvent
    {
        public int number;
        public string phase; // "clear" (the old number flashes) or "burn" (the new one burns in)
    }

    public sealed class ResultsStageEvent
    {
        public string stage; // "outcome", "round", "card", "grade", "best", "line", "done"
        public int index; // the card's index for "card"
    }

    public sealed class KillConfirmEvent
    {
        public string owner = "player";
        public int tier; // killConfirm.js tier (0 plain, 1 head / melee, 2 multikill)
        public int chain;
        public Vec3 position;
    }

    public sealed class TipEvent
    {
        public string id, text;
    }

    public sealed class OnboardingEvent
    {
        public int round;
        public string id; // the lesson's id
    }

    /// <summary>NIGHT12: the run setup chosen on TITLE (agent F's screen sets it; SurvivalGame.StartRun reads it).</summary>
    public sealed class RunSetup
    {
        public const string Survival = "survival", Grind = "grind", Campaign = "campaign"; // NIGHT21: THE GRIND (Endless21); NIGHT24: the CAMPAIGN (Campaign24.Mode)
        /// <summary>NIGHT15 U3-28.5: the old arena's map id (PortExport/maps/survival.json), the default.</summary>
        public const string DefaultMap = "survival";
        public string mode = Survival;
        public string mapId = DefaultMap; // NIGHT15: the map the run plays (maps/<id>.json from PortExport or Design/maps)
        public int heat; // 0: no HEAT rung
        public uint? seed; // RETRY SAME SEED / a typed seed code (null: a fresh seed)
        public readonly List<string> mutators = new List<string>(); // the fun toggles picked (LOW GRAVITY...), not the rung's
    }
}
