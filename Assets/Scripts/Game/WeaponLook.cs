// NIGHT13 (the shared types; audit-perf section 4 "duplication"): a gun's family colour, written once. main.js
// familyHex / viewmodel.js createRig / effects.js: config.weapons.families[config.weapons.catalog[gun].family], else the
// caller's fallback colour. Cached per gun (no config lookup on the frame path: PORT-RULES section 6).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class WeaponLook
    {
        static readonly Dictionary<string, uint?> hexOf = new Dictionary<string, uint?>();
        static GameConfig cachedFor;

        /// <summary>The gun's family hex (weapons.families), or null when the gun or its family has none.</summary>
        public static uint? FamilyHex(GameConfig cfg, string weapon)
        {
            if (weapon == null) return null;
            if (!ReferenceEquals(cfg, cachedFor))
            {
                hexOf.Clear();
                cachedFor = cfg;
            }
            if (hexOf.TryGetValue(weapon, out uint? hex)) return hex;
            string fam = cfg.Has($"weapons.catalog.{weapon}.family") ? cfg.Str($"weapons.catalog.{weapon}.family") : null;
            hex = fam != null && cfg.Has($"weapons.families.{fam}") ? (uint)cfg.Num($"weapons.families.{fam}") : (uint?)null;
            hexOf[weapon] = hex;
            return hex;
        }

        /// <summary>The gun's family colour, else `fallback`.</summary>
        public static Color FamilyColor(GameConfig cfg, string weapon, Color fallback)
        {
            uint? hex = FamilyHex(cfg, weapon);
            return hex.HasValue ? Look.Hex(hex.Value) : fallback;
        }
    }
}
