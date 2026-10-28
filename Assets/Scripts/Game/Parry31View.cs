// NIGHT31 builder W (round 4, design.n31.ltAlways's view; gated on N31Fixes.On, no sim bit): a boss projectile the gauntlet's LT PARRY sent back
// (SurvivalSim.BossParry31: the "parry" event with kind lanternOrb / stagShard / wyrmSpit) is drawn flying back: a cyan streak (rule 8: cyan is
// player-usable) from where the projectile was to the boss's aim point, UnityTuning.Parry31StreakSeconds long, its head running ahead of a short tail.
// The return itself is instant in the sim (the damage lands on the parry's step); this is the picture of it. View only; installs itself (no edit in the
// lead's SurvivalGame files); Unity objects are made on first use (never in field initialisers). -skygrindOff31: nothing drawn.
// NIGHT39 F1 (design.n39.slow39 on): the streak ages on the world's time (game.WorldDt: 0 through the parry's 0.25 s freeze, slowed in slow39's
// parry window), starts Parry39StreakStartU of the way out so the frozen frame shows the turn, and is wider, brighter and longer (UnityTuning.Slow39.cs).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Parry31View : MonoBehaviour
    {
        /// <summary>For the reels: streaks drawn this frame and the count of returns seen.</summary>
        public static int StreaksShown, ReturnsSeen;

        sealed class Streak
        {
            public Vector3 from, to;
            public float age;
            public LineRenderer line;
        }

        SurvivalGame game;
        Transform root;
        Material mat;
        System.Action off;
        readonly List<Streak> live = new List<Streak>();
        readonly List<LineRenderer> pool = new List<LineRenderer>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying || !N31Fixes.On) return;
            var go = new GameObject("Parry31View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Parry31View>();
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.cfg == null || game.events == null) { game = null; return false; }
            off?.Invoke();
            if (root != null) Destroy(root.gameObject);
            live.Clear();
            pool.Clear();
            root = new GameObject("Parry 31").transform;
            DontDestroyOnLoad(root.gameObject);
            if (mat != null) Destroy(mat);
            mat = FxDraw.Additive(game.mats.neon, "Parry streak 31");
            off = game.events.On<HitEvent>("parry", OnParry);
            return true;
        }

        void OnParry(HitEvent e)
        {
            if (e == null || (e.kind != "lanternOrb" && e.kind != "stagShard" && e.kind != "wyrmSpit") || game == null || game.sim == null) return;
            Enemy boss = null;
            foreach (Enemy x in game.sim.enemies.all) if (x.id == e.enemy) { boss = x; break; }
            if (boss == null) return;
            ReturnsSeen++;
            LineRenderer l = pool.Count > 0 ? pool[pool.Count - 1] : NewLine();
            if (pool.Count > 0) pool.RemoveAt(pool.Count - 1);
            bool s39 = Slow39On;
            l.widthMultiplier = s39 ? UnityTuning.Parry39StreakWidth : UnityTuning.Parry31StreakWidth;
            // NIGHT39 F1: with slow39 the streak starts a little way out, so the frozen parry frame already shows the turn
            live.Add(new Streak { from = WebSpace.ToUnity(e.position), to = WebSpace.ToUnity(boss.AimPoint), age = s39 ? UnityTuning.Parry39StreakStartU * UnityTuning.Parry39StreakSeconds : 0, line = l });
        }

        /// <summary>NIGHT39 F1: design.n39.slow39 is on for this game (its loop state exists once the first frame ran).</summary>
        bool Slow39On => game != null && game.Slow39State != null;

        LineRenderer NewLine()
        {
            var l = new GameObject("streak").AddComponent<LineRenderer>();
            l.transform.SetParent(root, false);
            l.useWorldSpace = true;
            l.alignment = LineAlignment.View;
            l.positionCount = 2;
            l.widthMultiplier = UnityTuning.Parry31StreakWidth;
            l.sharedMaterial = mat;
            l.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            l.receiveShadows = false;
            l.enabled = false;
            return l;
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            // NIGHT39 F1 (design.n39.slow39 on): the streak ages on WORLD time (held through the parry's freeze, slowed in its window), longer and brighter
            bool s39 = Slow39On;
            float dt = s39 ? (float)game.WorldDt : Time.deltaTime;
            float seconds = s39 ? UnityTuning.Parry39StreakSeconds : UnityTuning.Parry31StreakSeconds;
            Color c = Look.C(game.cfg, "colors.usable") * (s39 ? UnityTuning.Parry39StreakBrightness : UnityTuning.Parry31StreakBrightness);
            StreaksShown = 0;
            for (int i = live.Count - 1; i >= 0; i--)
            {
                Streak s = live[i];
                s.age += dt;
                float u = s.age / seconds;
                if (u >= 1f)
                {
                    s.line.enabled = false;
                    pool.Add(s.line);
                    live.RemoveAt(i);
                    continue;
                }
                Vector3 head = Vector3.Lerp(s.from, s.to, u), tail = Vector3.Lerp(s.from, s.to, Mathf.Max(0f, u - UnityTuning.Parry31StreakTail));
                s.line.SetPosition(0, tail);
                s.line.SetPosition(1, head);
                s.line.startColor = c * 0.4f;
                s.line.endColor = c;
                s.line.enabled = true;
                StreaksShown++;
            }
        }

        void OnDestroy() => off?.Invoke();
    }
}
