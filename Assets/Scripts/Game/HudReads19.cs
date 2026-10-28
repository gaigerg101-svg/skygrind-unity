// NIGHT19 U3-33.8 (builder H): what the HUD's weapon card reads from builder W's sim, declared here as partial hooks so W
// fills them in W's own partial file (e.g. Game/HudReads19.Arsenal.cs) without editing this one (the shared-hook pattern
// of NIGHT19's shared commit). A hook W hasn't written compiles away: the card then shows nothing for it.
// - AltMeter: the gun in hand's alt meter. Tonight: the KESTREL's gust (design.gust): its charges ready, how many it holds
//   (one per airtime plus one on the ground), and the next charge's refill 0..1. The card draws it in the meter row under
//   the slots (UiHotbar.cs) when the gun has no heat / charge / reload meter of its own (Loadout.Meter).
// - FreshTier: the tier of a gun with design.freshness on (ULTRAKILL's table): 0 FRESH, 1 USED, 2 STALE, 3 DULL; -1 none
//   (the block off, or the gun outside the table). The card prints the word by the role tag, in the cyan value ladder.
// Both are read every frame: no allocation (a struct by ref, an int).
namespace Skygrind.Game
{
    public static partial class HudReads19
    {
        /// <summary>An alt meter: `charges` of `max` ready, the next one `next` (0..1) refilled.</summary>
        public struct Alt
        {
            public bool valid;
            public int charges, max;
            public double next;
        }

        /// <summary>The gun in hand's alt meter (invalid when W's read isn't there or the gun has none).</summary>
        public static void AltMeter(SurvivalGame g, string weapon, ref Alt a)
        {
            a.valid = false;
            a.charges = a.max = 0;
            a.next = 0;
            if (g == null || weapon == null) return;
            ReadAlt(g, weapon, ref a);
            if (a.max <= 0) a.valid = false;
        }

        /// <summary>The freshness tier (0 FRESH .. 3 DULL), or -1.</summary>
        public static int FreshTier(SurvivalGame g, string weapon)
        {
            int tier = -1;
            if (g != null && weapon != null) ReadFreshTier(g, weapon, ref tier);
            return tier < 0 || tier > 3 ? -1 : tier;
        }

        /// <summary>The tier's word.</summary>
        public static string TierWord(int tier) => tier == 0 ? "FRESH" : tier == 1 ? "USED" : tier == 2 ? "STALE" : tier == 3 ? "DULL" : null;

        // W: set a.valid = true, a.charges, a.max and a.next for the gun's alt (the gust's charges on the KESTREL).
        static partial void ReadAlt(SurvivalGame g, string weapon, ref Alt a);

        // W: set tier (0 FRESH, 1 USED, 2 STALE, 3 DULL) for `weapon` while design.freshness is on.
        static partial void ReadFreshTier(SurvivalGame g, string weapon, ref int tier);
    }
}
