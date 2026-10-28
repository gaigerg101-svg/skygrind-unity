// NIGHT33 builder E (design.n33.spawns33; DESIGN33 E "a 0.6 s red-orange spawn tell"): draws each `enemySpawnTell33` {kind, x, y, z, seconds} as a red-orange
// column (CLAUDE.md rule 8: red-orange is enemies and danger) at the spot an enemy is about to appear: it rises from a thin line to its full width over the tell and
// is gone when the enemy is in. A plain class (no MonoBehaviour, no Unity API in field initializers: L-035): made once by SurvivalGame (the lead's line), drawn by
// Render(worldDt) once a frame (0 in hitstop: the tell holds). A pool of SpawnTell33Pool columns, each with its own field material (made once): nothing allocates
// per frame or per tell. Numbers: UnityTuning.Spawns33.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class SpawnTell33View
    {
        readonly Transform root;
        readonly Transform[] cols;
        readonly Material[] mats;
        readonly float[] left, full;
        int next;

        public SpawnTell33View(SurvivalGame game)
        {
            int n = UnityTuning.SpawnTell33Pool;
            cols = new Transform[n];
            mats = new Material[n];
            left = new float[n];
            full = new float[n];
            root = new GameObject("SpawnTell33").transform;
            Color danger = Look.C(game.cfg, "colors.enemy", UnityTuning.SpawnTell33Glow);
            for (int i = 0; i < n; i++)
            {
                mats[i] = DesignKit.Field(game, danger, 0, UnityTuning.SpawnTell33Height);
                cols[i] = DesignKit.Beam("spawn tell " + i, root, UnityTuning.SpawnTell33Radius, UnityTuning.SpawnTell33Height, mats[i]);
                cols[i].gameObject.SetActive(false);
            }
            game.events.On<SpawnTell33Event>("enemySpawnTell33", e =>
            {
                int i = next;
                next = (next + 1) % cols.Length; // the oldest gives way when more than the pool tell at once
                Vector3 at = WebSpace.ToUnity(new Vec3(e.x, e.y, e.z));
                cols[i].position = at + new Vector3(0, UnityTuning.SpawnTell33Height / 2, 0);
                mats[i].SetFloat("_FadeTop", at.y + UnityTuning.SpawnTell33Height);
                full[i] = left[i] = (float)e.seconds;
                cols[i].gameObject.SetActive(true);
            });
        }

        public void Render(double worldDt)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                if (left[i] <= 0) continue;
                left[i] -= (float)worldDt;
                if (left[i] <= 0)
                {
                    cols[i].gameObject.SetActive(false);
                    continue;
                }
                float k = full[i] > 0 ? 1 - left[i] / full[i] : 1; // 0 at the tell, 1 as the enemy appears
                float r = UnityTuning.SpawnTell33Radius * (UnityTuning.SpawnTell33StartWidth + (1 - UnityTuning.SpawnTell33StartWidth) * k);
                cols[i].localScale = new Vector3(r * 2, UnityTuning.SpawnTell33Height / 2, r * 2);
            }
        }
    }
}
