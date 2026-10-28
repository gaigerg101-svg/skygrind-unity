// NIGHT74 builder B (slate B3, design.n74.kill74, a VIEW row: NightRows.ViewOn(cfg, "kill74") EVERY frame, never `enabled` written here; off: -skygrindRows72 kill74:0, Rows72.cs).
// finish55 (Finish55View.cs: the 6 deg kick, the 8 deg FOV punch, the letterbox) went SHORT three times at NIGHT73: the blind spotter read NOTHING OBVIOUS on the framed bosskill72 strips (a boss kill
// "should land": GG NIGHT16 "kills not crunchy", OWNER:37; CORE: "kills aren't crunchy"). scout-bosses.md K1-K4: the kill is the one beat to give the full stack; a freeze of 0.25-0.5 s at the top of the observed
// range, a 1-3 frame white flash first, the camera kept on the impact, the punch last. What kill74 does at a campaign boss's death (all five, BossAttackClock63's ids; B3 built it on THE KILN WYRM, the bosskill72 strip's boss, leftover 6 added THE WICK, THE CROWN, THE LOOM and THE ICEBREAKER):
//   1. THE FLASH (this file, Kill74View): the render frames N74KillFlashStartFrame .. +N74KillFlashFrames after the frame the death event (bossDying) was seen in, an additive white overlay on
//      each of the wyrm's boxes (the sim's own hit boxes, drawn as WyrmView draws them, x N74KillFlashScale), or, for the other four, a white copy (Graphics.DrawMesh, the same scale about each pivot) of every
//      solid mesh under the roots their views register (Kill74.Register: the crown's ring, the wick's cage, the loom's body and legs and hull, the icebreaker's body): the silhouette goes white for two frames and
//      nothing else on the screen does;
//   2. THE HOLD (SurvivalGame.Kill74.cs, the lead's one line in SurvivalGame.Impact.cs): for N74KillHoldS real seconds from the event the world's time share is N74KillHoldShare (0.1): the stepping slows,
//      no step is skipped or changed (the Debut / Arena36 pattern: "visual time only: a tape and the fingerprint never see it"), so the dead pose is held about a third of a second;
//   3. THE PUNCH-IN (Finish55View.cs): finish55's kick, FOV punch and letterbox wait the same N74KillHoldS (kill74 on, any of the five), so they land after the hold, not on the flash.
// VIEW ONLY: no sim read for writing, no step paused or changed, no tape bit, no fingerprint, no new collider; the sim's step sequence is the same with the row on or off (the hold only changes how many
// steps run per frame). Reduced motion / SLOW-MO off (impactFrames.rules.slowMoOn) keeps the hold off; the flash is one shape for two frames and stays.
// THE PROOF (L-014 / L-115): `[kill74] boss kill (wyrm) at frame <f>: flash on frames +<a>..+<b> (<n> boxes x<gain>), hold <s> s at <share>x, punch-in after <s> s | row on|off ...` at the event (the row off says
// nothing is drawn), and `[kill74] flash frame +<k> (frame <f>): <n> boxes drawn, head at x,y,z | row on` on each flashed frame (none with the row off). Judged on the framed bosskill72 strip
// (FeelReel.N73.Frame.cs; n74-kill.py reads the frames' luma inside the head's box).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>The pure maths of the beat's clock (no Unity object). EditMode: N74BKillTests.</summary>
    public static class Kill74Math
    {
        /// <summary>Is the flash drawn on the `idx`-th rendered frame after the event's own (0): from `start` for `frames` frames.</summary>
        public static bool FlashOn(int idx, int start, int frames) => idx >= start && idx < start + frames;

        /// <summary>True when the beat is over after the `idx`-th frame (nothing more to draw).</summary>
        public static bool Done(int idx, int start, int frames) => idx >= start + frames;

        /// <summary>The world's time share `t` real seconds after the event: `share` through the hold, 1 after it (the caller takes the smaller of this and every other layer's).</summary>
        public static double HoldShare(double t, double holdS, double share) => t >= 0 && t < holdS ? share : 1.0;

        /// <summary>finish55's delay (s): the hold when kill74 dresses this boss, else 0.</summary>
        public static double PunchDelayS(bool kill74, bool supported, double holdS) => kill74 && supported ? holdS : 0.0;
    }

    public static class Kill74
    {
        public const string Row = "kill74";

        /// <summary>THE KILN WYRM's id: its silhouette is the sim's boxes (the other four's the view-built meshes).</summary>
        public const string WyrmId = "wyrm";

        static int onFrame = -1;
        static bool onNow;
        static GameConfig onCfg;

        /// <summary>The row, uncached (tests).</summary>
        public static bool Decide(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);

        /// <summary>Is the row on this frame? (Read every frame; once a frame per config.)</summary>
        public static bool On(GameConfig cfg)
        {
            int f = Time.frameCount;
            if (f != onFrame || !ReferenceEquals(cfg, onCfg)) { onFrame = f; onCfg = cfg; onNow = Decide(cfg); }
            return onNow;
        }

        /// <summary>The bosses kill74 dresses (UnityTuning.N74KillBosses: the five campaign bosses, BossAttackClock63's ids).</summary>
        public static bool Supports(string boss) => !string.IsNullOrEmpty(boss) && Array.IndexOf(UnityTuning.N74KillBosses, boss) >= 0;

        /// <summary>For the reel: beats begun, flash frames drawn, and the frame the last event was seen in.</summary>
        public static int Beats, FlashFrames, EventFrame = -1;

        // The silhouettes of the four bosses that are not THE KILN WYRM (whose is the sim's boxes): the transforms their views hang their solid parts under, handed here by each view every frame
        // it draws (a rebuilt view replaces them). The flash draws white copies of the solid meshes (MeshRenderers that cast shadows: the glows, decals and lights do not) under them.
        static readonly System.Collections.Generic.Dictionary<string, Transform[]> roots = new System.Collections.Generic.Dictionary<string, Transform[]>(StringComparer.Ordinal);

        /// <summary>A boss view's body roots (its solid parts), a cached array: called every frame the view draws the boss.</summary>
        public static void Register(string boss, Transform[] under)
        {
            if (!string.IsNullOrEmpty(boss)) roots[boss] = under;
        }

        /// <summary>A boss view's one body root: the boss's single-element array is reused (no allocation after the first frame).</summary>
        public static void Register(string boss, Transform under)
        {
            if (string.IsNullOrEmpty(boss)) return;
            if (roots.TryGetValue(boss, out Transform[] r) && r != null && r.Length == 1) r[0] = under;
            else roots[boss] = new[] { under };
        }

        /// <summary>The boss's registered roots (null: none).</summary>
        public static Transform[] RootsOf(string boss) => boss != null && roots.TryGetValue(boss, out Transform[] r) ? r : null;
    }

    /// <summary>The flash: installs itself after the first scene load, hooks the game's bus and render hook once the game exists (Finish55View's pattern).</summary>
    public sealed class Kill74View : MonoBehaviour
    {
        SurvivalGame game;
        Mesh boxMesh;
        Material add;
        Transform root;
        readonly List<Transform> boxT = new List<Transform>();
        readonly List<Renderer> boxR = new List<Renderer>();
        MaterialPropertyBlock block;
        int baseColorId;
        int idx = -1; // rendered frames since the event's frame (0 = that frame); -1: no beat
        string bossNow; // the dying boss's id
        readonly List<MeshRenderer> bodyMeshes = new List<MeshRenderer>();
        readonly List<MeshFilter> bodyFilters = new List<MeshFilter>();
        bool bodyCollected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Kill74View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Kill74View>();
        }

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            Clear();
            g.OnRender += Render;
            g.OnNewRun += Clear;
            g.events.On<BossEvent>("bossDying", e => Begin(e != null ? e.boss : null));
        }

        void Clear()
        {
            idx = -1;
            Hide();
        }

        /// <summary>The death event (inside a sim step of this frame's Update): the beat starts, frame 0 is this one.</summary>
        void Begin(string boss)
        {
            if (!Kill74.Supports(boss) || game == null) return;
            idx = 0;
            bossNow = boss;
            bodyCollected = false;
            Kill74.Beats++;
            Kill74.EventFrame = Time.frameCount;
            bool on = Kill74.On(game.cfg);
            CultureInfo ci = CultureInfo.InvariantCulture;
            Debug.Log("[kill74] boss kill (" + boss + ") at frame " + Time.frameCount + ": flash on frames +" + UnityTuning.N74KillFlashStartFrame + "..+" + (UnityTuning.N74KillFlashStartFrame + UnityTuning.N74KillFlashFrames - 1)
                + " (" + (boss == "wyrm" ? "the wyrm's boxes" : "the solid meshes under the boss view's roots") + " x" + UnityTuning.N74KillFlashGain.ToString("0.0", ci) + " white, " + UnityTuning.N74KillFlashScale.ToString("0.00", ci) + " scale), hold " + UnityTuning.N74KillHoldS.ToString("0.00", ci) + " s at "
                + UnityTuning.N74KillHoldShare.ToString("0.0", ci) + "x, punch-in after " + Kill74Math.PunchDelayS(on, true, UnityTuning.N74KillHoldS).ToString("0.00", ci) + " s | row kill74 " + (on ? "on" : "off (nothing drawn, no hold, finish55 at once)")
                + " | t " + FeelClock.Now.ToString("0.00", ci) + " s (FeelClock)");
        }

        void Render(double frameDt, double alpha)
        {
            SurvivalGame g = game;
            if (g == null || idx < 0) { Hide(); return; }
            bool on = Kill74.On(g.cfg);
            if (FeelClock.Holding) { if (!on) Hide(); return; } // a same-run hold frame is not a rendered frame of the run: the counter stands, and the held OFF half shows no overlay
            int here = idx++;
            if (Kill74Math.Done(here, UnityTuning.N74KillFlashStartFrame, UnityTuning.N74KillFlashFrames)) { idx = -1; Hide(); return; }
            if (on && Kill74Math.FlashOn(here, UnityTuning.N74KillFlashStartFrame, UnityTuning.N74KillFlashFrames)) Draw(g, here, alpha);
            else Hide();
        }

        void Build(SurvivalGame g)
        {
            if (root != null) return;
            block = new MaterialPropertyBlock();
            baseColorId = Shader.PropertyToID("_BaseColor");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Destroy(cube);
            add = FxDraw.Additive(g.mats.neon, "Kill74 flash");
            add.SetColor(baseColorId, new Color(UnityTuning.N74KillFlashGain, UnityTuning.N74KillFlashGain, UnityTuning.N74KillFlashGain, 1f)); // Graphics.DrawMesh copies carry no property block: the material's own white
            root = new GameObject("Kill74 flash").transform;
            root.SetParent(transform, false);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // WyrmView.U

        /// <summary>The four view-built bosses: a white copy of each solid mesh under the boss view's registered roots (the opaque ones: the glows, decals and lights are additive), drawn this frame at
        /// N74KillFlashScale about its own pivot with Graphics.DrawMesh and the additive white material: the silhouette the player sees, in the shapes the view drew.</summary>
        void DrawMeshes(SurvivalGame g, int frameIdx)
        {
            Build(g);
            if (!bodyCollected)
            {
                bodyCollected = true;
                bodyMeshes.Clear();
                bodyFilters.Clear();
                Transform[] under = Kill74.RootsOf(bossNow);
                var found = new List<MeshRenderer>();
                if (under != null)
                    foreach (Transform t in under)
                    {
                        if (t == null) continue;
                        t.GetComponentsInChildren<MeshRenderer>(false, found);
                        foreach (MeshRenderer mr in found)
                        {
                            Material m = mr.sharedMaterial;
                            MeshFilter mf = mr.GetComponent<MeshFilter>();
                            if (m == null || mf == null || mf.sharedMesh == null || m.renderQueue >= (int)RenderQueue.Transparent) continue; // additive glows are not the silhouette
                            bodyMeshes.Add(mr);
                            bodyFilters.Add(mf);
                        }
                    }
            }
            int drawn = 0;
            Matrix4x4 grow = Matrix4x4.Scale(Vector3.one * UnityTuning.N74KillFlashScale);
            for (int i = 0; i < bodyMeshes.Count; i++)
            {
                MeshRenderer mr = bodyMeshes[i];
                if (mr == null || !mr.enabled || !mr.gameObject.activeInHierarchy) continue;
                Graphics.DrawMesh(bodyFilters[i].sharedMesh, mr.transform.localToWorldMatrix * grow, add, mr.gameObject.layer, null, 0, null, ShadowCastingMode.Off, false);
                drawn++;
            }
            if (drawn > 0) Kill74.FlashFrames++;
            Debug.Log("[kill74] flash frame +" + frameIdx + " (frame " + Time.frameCount + "): " + drawn + " meshes drawn (" + bossNow + "; " + bodyMeshes.Count + " solid meshes under " + (Kill74.RootsOf(bossNow) != null ? Kill74.RootsOf(bossNow).Length : 0)
                + " roots) | row kill74 on");
        }

        void Draw(SurvivalGame g, int frameIdx, double alpha)
        {
            if (bossNow != Kill74.WyrmId) { DrawMeshes(g, frameIdx); return; }
            Wyrm w = g.sim != null ? g.sim.wyrm24 : null;
            if (w == null || g.mats == null) { Hide(); return; }
            Build(g);
            root.gameObject.SetActive(true);
            int n = w.PieceCount, drawn = 0;
            float a = Mathf.Clamp01((float)alpha);
            while (boxT.Count < n)
            {
                var go = new GameObject("flash box");
                go.transform.SetParent(root, false);
                go.AddComponent<MeshFilter>().sharedMesh = boxMesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = add;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
                boxT.Add(go.transform);
                boxR.Add(mr);
            }
            Color white = new Color(UnityTuning.N74KillFlashGain, UnityTuning.N74KillFlashGain, UnityTuning.N74KillFlashGain, 1f);
            Vector3 head = Vector3.zero;
            for (int i = 0; i < boxT.Count; i++)
            {
                bool live = i < n && !w.buried[i];
                if (boxT[i].gameObject.activeSelf != live) boxT[i].gameObject.SetActive(live);
                if (!live) continue;
                WyrmBox b = w.boxes[i];
                Vector3 c = Vector3.Lerp(U(w.prevCenters[i]), U(b.c), a);
                Quaternion q = Quaternion.LookRotation(U(b.f), U(b.u));
                boxT[i].SetPositionAndRotation(c, q);
                boxT[i].localScale = new Vector3((float)b.hr * 2f, (float)b.hu * 2f, (float)b.hf * 2f) * UnityTuning.N74KillFlashScale;
                block.Clear();
                block.SetColor(baseColorId, white);
                boxR[i].SetPropertyBlock(block);
                if (i == 0) head = c;
                drawn++;
            }
            Kill74.FlashFrames++;
            CultureInfo ci = CultureInfo.InvariantCulture;
            Debug.Log("[kill74] flash frame +" + frameIdx + " (frame " + Time.frameCount + "): " + drawn + " boxes drawn, head at " + head.x.ToString("0.0", ci) + "," + head.y.ToString("0.0", ci) + "," + head.z.ToString("0.0", ci) + " | row kill74 on");
        }

        void Hide()
        {
            if (root != null && root.gameObject.activeSelf) root.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (game != null) game.OnRender -= Render;
            if (add != null) Destroy(add);
        }
    }
}
