// NIGHT33 builder E: the spawn tell's look (SpawnTell33View; the tell's length is gameplay: design.n33.spawns33.tellSeconds).
using UnityEngine;
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // columns drawn at once (a wave is at most about 8; the oldest gives way past this)
        public const int SpawnTell33Pool = 16;
        // the column's full radius in m (about a husk's body: the enemy appears inside it)
        public const float SpawnTell33Radius = 0.55f;
        // the column's height in m (seen over cover across a room)
        public const float SpawnTell33Height = 6f;
        // the column's width at the tell's start, as a share of its full width (it widens to full as the enemy comes in)
        public const float SpawnTell33StartWidth = 0.25f;
        // the red-orange's HDR glow (colors.enemy x this; under L1's <= 4 for emissives)
        public const float SpawnTell33Glow = 1.6f;

        // ---- STILL ARMOR's sign (StillArmor33View; the rule is design.n33.bosses33.stillArmor) ----
        // the deflect grey (neither red-orange nor cyan: a blocked hit reads as no damage)
        public static readonly Color StillArmor33Grey = new Color(0.8f, 0.82f, 0.85f);
        // grey cubes thrown per blocked hit, their speed (m/s), life (s) and size (x the particle cube)
        public const int StillArmor33SparkCount = 10;
        public const float StillArmor33SparkSpeed = 6f;
        public const float StillArmor33SparkLife = 0.3f;
        public const float StillArmor33SparkScale = 0.9f;
        // the shell's strength (x the grey: a veil that dims the boss's glow, not a wall)
        public const float StillArmor33ShellAlpha = 0.55f;
        // the shell round the body: its radius / height x this, at least these m (a boss body's hit cylinder can be small: the Wyrm's head)
        public const float StillArmor33Grow = 1.25f;
        public const float StillArmor33MinRadius = 1.5f;
        public const float StillArmor33MinHeight = 3f;
        // the shell's slow pulse: depth (share of its strength) and rate (Hz)
        public const float StillArmor33Pulse = 0.35f;
        public const float StillArmor33PulseRate = 1.2f;
        // the message slot's danger line while the armor is up (builder U's slot, priority danger)
        public const string StillArmor33Line = "MOVE";
    }
}
