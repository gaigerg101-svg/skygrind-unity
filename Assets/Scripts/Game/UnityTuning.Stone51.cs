// NIGHT51 (stone51, a view change: no sim bit, no config row; off with -skygrindOff51 or any older switch). GG 2026-10-19: "how can we
// seriously make this game better"; the lead's read of every n50 frame: the world is flat slabs (one vertex colour per face, a faint 1 m
// world grid that ignores the box's edges), so nothing reads as built and no ledge reads as an edge. SKYGRIND/Stone now draws, from each
// face's own coordinates (MeshKit uv3): panels sized to divide the face evenly, dark seams between them, a lit bevel on the face's edges
// (a wall's foot excepted: it meets the floor), grime at a wall's foot, a slow light gradient up tall walls, and a small tone change per
// panel. Unity-only numbers (render): the sim never reads them.
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Stone51WallPanelW = 3.0f; // m: a wall panel's target width (the face divides evenly: 20 m -> 7 panels of 2.86 m)
        public const float Stone51WallPanelH = 1.5f; // m: a wall panel's target height (a 2 m player sees about one and a half rows at eye level)
        public const float Stone51FloorTile = 2.0f; // m: a floor / top tile's target size (a run at 12 m/s crosses six a second: a speed cue)
        public const float Stone51Bevel = 0.06f; // m: the lit bevel's width along a face's edges (reads as a chamfer at 10 m)
        public const float Stone51Seam = 0.05f; // m: a panel seam's width
        public const float Stone51SeamDark = 0.45f; // a seam's darkening (x (1 - this))
        public const float Stone51BevelLight = 0.45f; // the bevel's brightening (x (1 + this)): ledges and corners catch the light
        public const float Stone51PanelTone = 0.10f; // +- per panel value change (a hash per panel and face): panels, not a sheet
        public const float Stone51GrimeH = 1.4f; // m: the grime band at a wall's foot
        public const float Stone51GrimeDark = 0.40f; // its darkening at the foot (fades to none at GrimeH)
        public const float Stone51RiseLight = 0.18f; // a wall's slow brightening from its foot to its top (x (1 + this) at the top)
        // round 2 (n51-r1 frames: the seams and bevels vanished on the dark violet walls, a multiply of a dark colour): light ADDED in albedo
        public const float Stone51BevelAdd = 0.07f; // the bevel's added light (a dark wall's edges catch it too)
        public const float Stone51LipAdd = 0.05f; // a lit lip just under each horizontal wall seam (the panel's top edge facing the sky)
    }

    /// <summary>NIGHT51 stone51: sets the stone's globals once per Look.Apply (on unless -skygrindOff51 or an older switch is given: Night51.On).</summary>
    public static class Stone51
    {
        static readonly int A = Shader.PropertyToID("_Stone51A"), B = Shader.PropertyToID("_Stone51B"), C = Shader.PropertyToID("_Stone51C"), D = Shader.PropertyToID("_Stone51D");

        public static void Apply()
        {
            Shader.SetGlobalVector(A, new Vector4(UnityTuning.Stone51WallPanelW, UnityTuning.Stone51WallPanelH, UnityTuning.Stone51FloorTile, UnityTuning.Stone51Bevel));
            Shader.SetGlobalVector(B, new Vector4(UnityTuning.Stone51Seam, UnityTuning.Stone51SeamDark, UnityTuning.Stone51BevelLight, UnityTuning.Stone51PanelTone));
            Shader.SetGlobalVector(C, new Vector4(UnityTuning.Stone51GrimeH, UnityTuning.Stone51GrimeDark, UnityTuning.Stone51RiseLight, Night51.On ? 1 : 0));
            Shader.SetGlobalVector(D, new Vector4(UnityTuning.Stone51BevelAdd, UnityTuning.Stone51LipAdd, 0, 0));
        }
    }
}
