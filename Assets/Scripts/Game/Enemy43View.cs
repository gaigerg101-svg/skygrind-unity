// NIGHT43 builder A1 (models43, a view row: design.n43.models43; NIGHT43.md card A1; GG's G21 / G23; DECIDE43 D-4): the rebuilt enemies. A view
// gives each pooled body a Look43 (its renderers, their primitive meshes and the kind's Mesh43 swaps); every frame the view asks Models43.On and
// the body swaps when the answer changed (no work on a frame it did not): ON, its own renderers draw the smooth Mesh43 meshes (the same unit
// space as the primitives: the views' sizes, poses, flashes, notches and FeelProbe rects are untouched), the kind's extra parts (visors, armour,
// attachments: UnityTuning.Models43.cs, built on the first ON frame, then kept) show, NIGHT40's silhouette parts hide, the body's colour is darkened
// (UnityTuning.Models43BodyValue: dark body, bright visor) and SKYGRIND/Enemy's form shade is on (EnemyLook.Form); OFF, everything is put back
// (the primitives, NIGHT40's parts): NIGHT42's picture. The reel flips Models43.Force for its on / off line-up in one Player; the game reads the
// row (read once). A death breaks the extra parts with the body (EnemyLook.Die, Enemy40Parts.kit43). No collider, no shadow, no allocation per
// frame; no Unity API in a field initializer (L-035).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Models43
    {
        /// <summary>The reel's switch (null: the row decides).</summary>
        public static bool? Force;

        static GameConfig rowCfg;
        static bool row;

        /// <summary>Is models43 drawing (the reel's Force, else design.n43.models43 on and NIGHT43 not off in this session; the row read once per config).</summary>
        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (!ReferenceEquals(cfg, rowCfg))
            {
                rowCfg = cfg;
                row = N43View.RowOn(cfg, "models43");
            }
            return row;
        }

        /// <summary>The kind's extra parts (UnityTuning.Models43*), or null.</summary>
        public static float[] Spec(string kind) => kind == "husk" ? UnityTuning.Models43Husk : kind == "swarmer" ? UnityTuning.Models43Swarmer
            : kind == "spitter" ? UnityTuning.Models43Spitter : kind == "charger" ? UnityTuning.Models43Charger : kind == "drone" ? UnityTuning.Models43Drone
            : kind == "warden" ? UnityTuning.Models43Warden : null;

        /// <summary>The body's colour while models43 draws (darkened; alpha 1).</summary>
        public static Color Body(Color c)
        {
            c *= UnityTuning.Models43BodyValue;
            c.a = 1;
            return c;
        }
    }

    /// <summary>NIGHT43 A1: one pooled body's models43 state (see the file's header).</summary>
    public sealed partial class Look43 // PG3 tripo74: partial (the Tripo model over the body: Enemy43View.Tripo.cs)
    {
        readonly string kind;
        readonly Transform root;
        readonly Transform[] anchors;
        readonly MeshFilter[] filters;
        readonly Mesh[] prim, swap;
        readonly EnemyLook look;
        readonly float height;
        /// <summary>The extra parts (null until the first ON frame).</summary>
        public Enemy40Parts extras;
        /// <summary>Drawing models43 now.</summary>
        public bool on;

        /// <summary>`rends`: the view's renderers in its order; `swapIds`: a Mesh43 id per renderer (-1 keeps it), or `swapMeshes` (a view's own
        /// made-once meshes, e.g. the drone's metre-sized saucer), whichever is given.</summary>
        public Look43(string kind, Transform root, Renderer[] rends, int[] swapIds, Mesh[] swapMeshes, EnemyLook look, float height)
        {
            this.kind = kind;
            this.root = root;
            this.look = look;
            this.height = height;
            int n = rends.Length;
            anchors = new Transform[n];
            filters = new MeshFilter[n];
            prim = new Mesh[n];
            swap = new Mesh[n];
            for (int i = 0; i < n; i++)
            {
                anchors[i] = rends[i].transform;
                filters[i] = rends[i].GetComponent<MeshFilter>();
                prim[i] = filters[i] != null ? filters[i].sharedMesh : null;
                swap[i] = swapMeshes != null ? swapMeshes[i] : swapIds != null && i < swapIds.Length && swapIds[i] >= 0 ? Mesh43.Get(swapIds[i]) : null;
            }
        }

        /// <summary>The frame's answer (`want`): swap on a change. `m40`: NIGHT40's parts (hidden while models43 draws; may be null).</summary>
        public void Apply(bool want, Enemy40Parts m40)
        {
            if (want == on) { TripoSync(m40); return; } // PG3 tripo74: the Tripo model over the pills, every frame (Enemy43View.Tripo.cs; after the swap below, so a part set made this frame is handed over before it draws; nothing when its file is missing or the row is off)
            on = want;
            for (int i = 0; i < filters.Length; i++)
                if (filters[i] != null && swap[i] != null) filters[i].sharedMesh = want ? swap[i] : prim[i];
            if (want && extras == null) extras = Enemy40Parts.Make43(kind, Models43.Spec(kind), root, anchors, look, height);
            extras?.Show(want);
            m40?.Show(!want);
            TripoSync(m40); // PG3 tripo74: as above
        }

        /// <summary>The parts EnemyLook.Die breaks with the body: models43's while it draws, else NIGHT40's.</summary>
        public Enemy40Parts DyingParts(Enemy40Parts m40) => on ? extras : m40;
    }
}
