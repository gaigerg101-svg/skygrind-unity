// NIGHT15 U3-28.4 (builder P): the map pipeline's Unity-only numbers (CLAUDE.md rule 5): the authoring grid a moved
// marker snaps to, the gizmo colours, the validator's thresholds and the pipeline's paths. The web has none of these: its
// maps are hand-written JS (src/world/maps/*.js) with no editor, no exporter of its own beyond port-export, and no
// validator. Pure numbers (no UnityEngine), so the validator also runs in a plain dotnet console.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- authoring (MapHandles) ---

        // A marker whose transform was moved exports its transform's values snapped to a 1 / 20 m = 0.05 m grid: fine enough
        // for any ledge the web's maps use (their values sit on 0.05 m: 12.3, 34.98, 0.3), coarse enough that a float32
        // transform (7 digits) prints as a short, exact decimal (round(v * 20) / 20).
        public const double MapSnapPerMetre = 20;

        // A moved marker's yaw snaps to whole multiples of this (degrees): divides Unity's default rotation snap (15 deg) and
        // the web's 45 / 90 deg yaws.
        public const double MapYawSnapDegrees = 5;

        // A marker counts as unmoved (it keeps its exact web doubles) while its transform is within this of the float32
        // position / size the import gave it (m): a float holds the web's values to ~4e-6 m at 50 m, a drag moves far more.
        public const double MapUnchangedTolerance = 1e-4;

        // ... and within this of its imported rotation (degrees).
        public const double MapUnchangedYawToleranceDeg = 0.01;

        // --- gizmos (0xRRGGBB, the dusk palette of CLAUDE.md rule 8: cyan usable, red-orange danger, pale violet-white the
        // boundary; the rest neutral) ---

        public const uint MapGizmoStoneRgb = 0x877d92; // boxes, wedges, floors: config colors.pillarTop (the stone)
        public const uint MapGizmoUsableRgb = 0x19d3e6; // strips, pads, grapple nodes, spots, the player spawn: colors.usable
        public const uint MapGizmoDangerRgb = 0xff4a1c; // enemy spawns, traps: colors.enemy
        public const uint MapGizmoBoundaryRgb = 0xe6e0ff; // barriers, the floor square, the kill plane: pale violet-white
        public const uint MapGizmoZoneRgb = 0xffd6a6; // zone boxes and gate buy spots: colors.sun (neither usable nor danger)
        public const uint MapGizmoHoleRgb = 0x9aa0e6; // zone holes: colors.lightSky
        public const float MapGizmoFillAlpha = 0.18f; // a box's solid fill under its wire outline (pickable in the Scene view)
        public const float MapGizmoPointRadius = 0.35f; // a point marker's sphere (m)
        public const float MapGizmoFacingLength = 1.2f; // a facing marker's arrow (m)

        // --- the validator (MapValidate) ---

        // A zone with fewer SURVIVAL spawn nodes (Zones samples them every config zones.nodeSpacing m) than this gets a
        // FINDING: enemies spawn only on nodes in open zones, so a zone needs a choice of them. survival.json's smallest zone
        // (armory, 10 x 23 m) has 18 (Reports/MAP-survival-validate.md); 8 is a 2 x 4 patch at the 4 m spacing.
        public const int MapMinSpawnsPerZone = 8;

        // A walkable component the player can't leave (no way back to the spawn on either graph, no drop to lower ground)
        // gets a FINDING when it has at least this many grid nodes (3 x 3 m at 1 m cells) or holds a spot or a spawn node;
        // smaller empty ones are counted only.
        public const int MapIslandMinNodes = 9;

        // The designed verb gates a spot may carry in a Unity-authored map (its optional "verb": the move that reaches it
        // where RIVAL's graph has none; the graph flies NavGraph.FLIGHT_KINDS, drops and pads, and may not chain a pad
        // from every deck). The lead's ids (NIGHT15, DRYDOCK). A spot the graph can't reach that carries one of these is a
        // VERB row, not a FINDING, while its zone is reached; a verb outside this list is a FINDING (a typo hides nothing).
        public static readonly string[] MapVerbs = { "wallRide", "slideJump", "wallJump", "arcline", "pad" };

        // An aimed pad's target lands on walkable ground when a node within one cell of it sits within this height of it (m).
        public const double MapPadLandTolerance = 0.5;

        // --- paths (relative to the repo root) ---

        public const string MapDesignDir = "Design/maps"; // Design/maps/<id>.json
        public const string MapPlayerCopyDir = "Assets/StreamingAssets/Design/maps"; // its byte-identical Player copy
        public const string MapReportDir = "Reports"; // Reports/MAP-<id>-validate.md
        public const string MapRoundTripDir = "Temp/MapRoundTrip"; // the round trip's export (Unity's Temp: never committed)
    }
}
