// NIGHT36 builder P (the core loop's picture; tells36 / parry36 / launch36 / heal36): view only, installs itself (no edit in the lead's
// SurvivalGame files or the shared enemy views); Unity objects are made on first use (never in field initialisers). Each part draws only
// while its row is on in the current run (game.sim.n36):
// - tells36: every live tell on an ordinary enemy (Tells36.Live; a boss's tells are Boss36View's) gets an additive halo around the body
//   in the tell's colour (YELLOW punch it / RED dodge), rising from Tell36FlashStart to the peak over the wind-up and pulsing in its
//   last 30 %; parryable projectiles (spitter globs not yet returned) glow yellow.
// - parry36: a yellow shell and sparks at the parry point (the freeze itself is the sim's hitstop).
// - launch36 / parry36 bursts: an orange-white shell the size of the burst radius and sparks.
// - heal36: a cyan "+10" rising from the kill.
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Tell36View : MonoBehaviour
    {
        /// <summary>For the reels: halos / glows drawn this frame, shells live, heal numbers live, and counts of what was seen.</summary>
        public static int HalosShown, YellowHalos, RedHalos, GlobGlows, ShellsShown, HealNumbersShown, ParriesSeen, BurstsSeen, HealsSeen;

        sealed class Shell
        {
            public Vector3 at;
            public float age, life, radius, alpha;
            public Color color;
            public Transform t;
            public Renderer r;
        }

        sealed class Number
        {
            public Vector3 at;
            public float age;
            public TextMeshPro text;
        }

        SurvivalGame game;
        Transform root;
        Material mat;
        Mesh sphere;
        MaterialPropertyBlock mpb;
        readonly List<System.Action> offs = new List<System.Action>();
        readonly List<Renderer> halos = new List<Renderer>();
        readonly List<Renderer> glows = new List<Renderer>();
        readonly List<Shell> shells = new List<Shell>();
        readonly List<Shell> shellPool = new List<Shell>();
        readonly List<Number> numbers = new List<Number>();
        readonly List<Number> numberPool = new List<Number>();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Tell36View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Tell36View>();
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.cfg == null || game.events == null || game.sim == null) { game = null; return false; }
            foreach (System.Action o in offs) o();
            offs.Clear();
            if (root != null) Destroy(root.gameObject);
            halos.Clear();
            glows.Clear();
            shells.Clear();
            shellPool.Clear();
            numbers.Clear();
            numberPool.Clear();
            root = new GameObject("Tell 36").transform;
            DontDestroyOnLoad(root.gameObject);
            if (mat != null) Destroy(mat);
            mat = FxDraw.Additive(game.mats.neon, "Tell 36");
            mpb ??= new MaterialPropertyBlock();
            if (sphere == null)
            {
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere = ball.GetComponent<MeshFilter>().sharedMesh;
                Destroy(ball);
            }
            offs.Add(game.events.On<HitEvent>(Parry36.Event, OnParry));
            offs.Add(game.events.On<Launch36BurstEvent>(Launch36.BurstEvent, e => OnBurst(e, game.sim.launch36Core != null ? game.sim.launch36Core.burstRadius : 3)));
            offs.Add(game.events.On<Launch36BurstEvent>("parry36Burst", e => OnBurst(e, game.sim.parry36Core != null ? game.sim.parry36Core.returnBurstRadius : 3, true)));
            offs.Add(game.events.On<Heal36Event>(Heal36.Event, OnHeal));
            return true;
        }

        Renderer NewSphere(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = sphere;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.enabled = false;
            return r;
        }

        void Paint(Renderer r, Color c)
        {
            mpb.SetColor(BaseColor, c);
            r.SetPropertyBlock(mpb);
        }

        void OnParry(HitEvent e)
        {
            if (game == null || e == null || e.owner != "player") return;
            ParriesSeen++;
            if (Fx49.On(game.cfg)) return; // NIGHT49 X fx49: the parry is the kit's critical four-point star in Look49's parry yellow (Fx49Kit.OnParry)
            Color y = Look.Hex(UnityTuning.Tell36YellowHex);
            game.ParticleBurst(e.position, y, UnityTuning.Tell36ParryBits, UnityTuning.Tell36ParryBitSpeed, UnityTuning.Tell36ParryBitLife, UnityTuning.Tell36ParryBitSize);
            AddShell(WebSpace.ToUnity(e.position), UnityTuning.Tell36ParryRingR, UnityTuning.Tell36ParryFlashS * 2f, y * UnityTuning.Tell36FlashPeak, UnityTuning.Tell36BurstAlpha);
        }

        void OnBurst(Launch36BurstEvent e, double radius, bool parryBurst = false) // NIGHT49 X: parryBurst (the kit's owner)
        {
            if (game == null || e == null) return;
            BurstsSeen++;
            Color c = Look.Hex(UnityTuning.Tell36BurstHex);
            if (Fx49.On(game.cfg)) c = Fx49.Hue(parryBurst ? Fx49Owner.Parry : Fx49Owner.Player) * (Fx49.Cell(Fx49Owner.Parry, Fx49Tier.Heavy).glow / UnityTuning.Tell36FlashPeak); // NIGHT49 X fx49: a returned parry's burst in the parry yellow, a launch's in the player's cyan (the orange-white was danger's family); at the kit's heavy glow
            // NIGHT47 F fx47 (n47-r1b): with the row on the cube bits (40 at 11 m/s, some into the camera: "huge flat slabs") are Fx47View's small
            // chunks thrown away from the eye (Fx47View.Weapons.cs OnBurst47); the shell is capped in LateUpdate
            if (!Fx47View.RowOn(game.cfg)) game.ParticleBurst(e.position, c, UnityTuning.Tell36BurstBits, UnityTuning.Tell36BurstBitSpeed, UnityTuning.Tell36BurstBitLife, UnityTuning.Tell36BurstBitSize);
            AddShell(WebSpace.ToUnity(e.position), (float)radius, UnityTuning.Tell36BurstS, c * UnityTuning.Tell36FlashPeak, UnityTuning.Tell36BurstAlpha);
        }

        void AddShell(Vector3 at, float radius, float life, Color c, float alpha)
        {
            Shell s;
            if (shellPool.Count > 0)
            {
                s = shellPool[shellPool.Count - 1];
                shellPool.RemoveAt(shellPool.Count - 1);
            }
            else
            {
                s = new Shell();
                s.r = NewSphere("shell");
                s.t = s.r.transform;
            }
            s.at = at;
            s.radius = radius;
            s.life = life;
            s.age = 0;
            s.color = c;
            s.alpha = alpha;
            shells.Add(s);
        }

        void OnHeal(Heal36Event e)
        {
            if (game == null || e == null) return;
            HealsSeen++;
            Number n;
            if (numberPool.Count > 0)
            {
                n = numberPool[numberPool.Count - 1];
                numberPool.RemoveAt(numberPool.Count - 1);
            }
            else
            {
                n = new Number();
                var go = new GameObject("heal36");
                go.transform.SetParent(root, false);
                n.text = go.AddComponent<TextMeshPro>();
                n.text.alignment = TextAlignmentOptions.Center;
                n.text.fontSize = UnityTuning.Tell36HealNumberSize;
                n.text.fontStyle = FontStyles.Bold;
            }
            n.text.text = "+" + Mathf.RoundToInt((float)e.amount);
            n.at = WebSpace.ToUnity(e.position);
            n.age = 0;
            n.text.gameObject.SetActive(true);
            numbers.Add(n);
            game.ParticleBurst(e.position, Look.Hex(UnityTuning.Tell36HealHex), UnityTuning.Tell36ParryBits / 2, UnityTuning.Tell36ParryBitSpeed * 0.5f, UnityTuning.Tell36ParryBitLife, UnityTuning.Tell36ParryBitSize);
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (game.sim == null) return;
            float dt = Time.deltaTime;
            SurvivalSim sim = game.sim;
            PoseBodies(sim, dt); // the tell pose, on top of the enemy views' transforms
            bool fx47 = Fx47View.RowOn(game.cfg);
            bool eyeOk = game.cam != null;
            Vector3 eyeAt = eyeOk ? game.cam.transform.position : Vector3.zero;
            HalosShown = YellowHalos = RedHalos = GlobGlows = 0;
            int h = 0, g = 0;
            if (sim.n36.tells36 && sim.tells36 != null)
            {
                float time = Time.time;
                foreach (Tell36 t in sim.tells36.Live)
                {
                    if (!t.live || !IsScan(t.attack)) continue;
                    Enemy e = Find(sim, t.source);
                    if (e == null) continue;
                    if (h >= halos.Count) halos.Add(NewSphere("halo"));
                    Renderer r = halos[h++];
                    float p = (float)t.Progress;
                    float k = Mathf.Lerp(UnityTuning.Tell36FlashStart, 1f, p);
                    if (p > 0.7f) k *= Light54.TellOn ? Light54.TellRamp(p) : 0.75f + 0.25f * Mathf.Sin(time * UnityTuning.Tell36PulseHz * Mathf.PI * 2f); // NIGHT54 light54: one brighten, not the 7 Hz pulse
                    if (fx47 && eyeOk)
                    {
                        // NIGHT47 F fx47 (n47-r1b: the +8 / +30 yellow wash, a body's halo round the eye): the halo fades out as the eye nears its
                        // surface (gone inside it); the body's own tell pose and colour still read
                        float hr = (float)System.Math.Max(e.Radius * UnityTuning.Tell36HaloScale, e.Height * 0.5 * UnityTuning.Tell36HaloScale);
                        Vector3 hc = WebSpace.ToUnity(new Vec3(e.position.x, e.position.y + e.Height * 0.5, e.position.z));
                        float gap = (hc - eyeAt).magnitude - hr * Mathf.Max(1f, UnityTuning.Foes47RimScale);
                        k *= Mathf.Clamp01(gap / UnityTuning.Fx47HaloNearFade);
                        if (k <= 0.001f) { r.enabled = false; continue; }
                    }
                    Color c = Look.Hex(t.kind == Tell36Kind.Yellow ? UnityTuning.Tell36YellowHex : UnityTuning.Tell36RedHex) * (UnityTuning.Tell36FlashPeak * UnityTuning.Tell36HaloAlpha * k);
                    Paint(r, c);
                    float d = (float)(e.Radius * 2 * UnityTuning.Tell36HaloScale);
                    r.transform.position = WebSpace.ToUnity(new Vec3(e.position.x, e.position.y + e.Height * 0.5, e.position.z));
                    r.transform.localScale = new Vector3(d, (float)e.Height * UnityTuning.Tell36HaloScale, d);
                    r.enabled = true;
                    if (Fx47View.RowOn(game.cfg)) // NIGHT47 F fx47 (G21, the enemies' side; F-cards 5d): the halo in three tones of its family (a light core inside, a darker rim outside)
                    {
                        bool yel = t.kind == Tell36Kind.Yellow;
                        float kk = UnityTuning.Tell36FlashPeak * UnityTuning.Tell36HaloAlpha * k;
                        for (int layer = 0; layer < 2; layer++)
                        {
                            if (h >= halos.Count) halos.Add(NewSphere("halo"));
                            Renderer x = halos[h++];
                            uint hex = layer == 0 ? (yel ? UnityTuning.Foes47YellowCoreHex : UnityTuning.Foes47RedCoreHex) : (yel ? UnityTuning.Foes47YellowRimHex : UnityTuning.Foes47RedRimHex);
                            float sc = layer == 0 ? UnityTuning.Foes47CoreScale : UnityTuning.Foes47RimScale;
                            Paint(x, Look.Hex(hex) * (kk * (layer == 0 ? 1.2f : 0.6f)));
                            x.transform.position = r.transform.position;
                            x.transform.localScale = r.transform.localScale * sc;
                            x.enabled = true;
                        }
                    }
                    HalosShown++;
                    if (t.kind == Tell36Kind.Yellow) YellowHalos++;
                    else RedHalos++;
                }
                if (sim.husks?.acid != null)
                {
                    Color y = Look.Hex(UnityTuning.Tell36YellowHex) * (UnityTuning.Tell36FlashPeak * UnityTuning.Tell36HaloAlpha);
                    float gd = (float)(sim.husks.acid.p.globRadius * 2 * UnityTuning.Tell36GlobGlowScale);
                    foreach (SpitterAcid.Glob gl in sim.husks.acid.globs)
                    {
                        if (!gl.alive || gl.reflected) continue;
                        if (g >= glows.Count) glows.Add(NewSphere("glob glow"));
                        Renderer r = glows[g++];
                        Paint(r, y);
                        r.transform.position = WebSpace.ToUnity(gl.position);
                        r.transform.localScale = Vector3.one * gd;
                        r.enabled = true;
                        GlobGlows++;
                    }
                    if (sim.husks.orbs != null)
                    {
                        // the drones' orbs: parryable too (parry36 sends them back at their drone)
                        float od = (float)(sim.husks.orbs.Radius * 2 * UnityTuning.Tell36GlobGlowScale);
                        foreach (DroneOrbs.Orb o in sim.husks.orbs.pool)
                        {
                            if (!o.alive || o.reflected) continue;
                            if (g >= glows.Count) glows.Add(NewSphere("glob glow"));
                            Renderer r = glows[g++];
                            Paint(r, y);
                            r.transform.position = WebSpace.ToUnity(o.position);
                            r.transform.localScale = Vector3.one * od;
                            r.enabled = true;
                            GlobGlows++;
                        }
                    }
                }
            }
            for (int i = h; i < halos.Count; i++) halos[i].enabled = false;
            for (int i = g; i < glows.Count; i++) glows[i].enabled = false;

            ShellsShown = 0;
            for (int i = shells.Count - 1; i >= 0; i--)
            {
                Shell s = shells[i];
                s.age += dt;
                float u = s.age / s.life;
                if (u >= 1f)
                {
                    s.r.enabled = false;
                    shellPool.Add(s);
                    shells.RemoveAt(i);
                    continue;
                }
                float grow = 1f - (1f - u) * (1f - u); // ease out: fast at first
                s.t.position = s.at;
                float sr = s.radius * Mathf.Lerp(0.2f, 1f, grow);
                if (fx47 && eyeOk)
                {
                    // NIGHT47 F fx47 (n47-r1b: "at +8 the screen is a full yellow wash"): a shell never holds the eye nor fills the screen: its drawn
                    // radius at most Fx47ShellMaxShare x its distance (about 20 deg) and Fx47NearEye short of the eye
                    float dist = (s.at - eyeAt).magnitude;
                    sr = Mathf.Min(sr, Mathf.Min(UnityTuning.Fx47ShellMaxShare * dist, dist - UnityTuning.Fx47NearEye));
                }
                if (sr <= 0.02f) { s.r.enabled = false; continue; }
                s.t.localScale = Vector3.one * (sr * 2f);
                float c48 = fx47 && eyeOk && Blast48.On(game.cfg) ? Blast48.ShellCentreK(game.cam, s.at, sr) : 1f; // NIGHT48 F blast48 (G20's wash): a shell over the reticle's box is drawn faint
                Paint(s.r, s.color * (s.alpha * (1f - u) * c48));
                s.r.enabled = true;
                ShellsShown++;
            }

            HealNumbersShown = 0;
            Camera cam = game.cam;
            for (int i = numbers.Count - 1; i >= 0; i--)
            {
                Number n = numbers[i];
                n.age += dt;
                float u = n.age / UnityTuning.Tell36HealNumberS;
                if (u >= 1f)
                {
                    n.text.gameObject.SetActive(false);
                    numberPool.Add(n);
                    numbers.RemoveAt(i);
                    continue;
                }
                n.text.transform.position = n.at + Vector3.up * (UnityTuning.Tell36HealNumberRise * u);
                if (cam != null) n.text.transform.rotation = cam.transform.rotation;
                Color c = Look.Hex(UnityTuning.Tell36HealHex);
                c.a = 1f - u * u;
                n.text.color = c;
                HealNumbersShown++;
            }
        }

        // ---- the tell POSE: the enemy views (EnemyViews.cs HuskView / SwarmerView / SpitterView) draw body n of a kind under their parent's
        // child n, in enemies.all order (alive, that kind); their Render runs in SurvivalGame.Update, so this LateUpdate poses on top of it.
        sealed class Pose
        {
            public float back, strike;
            public int seenFrame;
        }

        readonly Dictionary<int, Pose> poses = new Dictionary<int, Pose>();
        readonly List<int> poseDrop = new List<int>();
        Transform huskParent, swarmerParent, spitterParent;
        public static int PosedNow;
        public static float PoseMax;

        static Transform FindParent(ref Transform t, string name)
        {
            if (t == null)
            {
                GameObject go = GameObject.Find(name);
                if (go != null) t = go.transform;
            }
            return t;
        }

        void PoseBodies(SurvivalSim sim, float dt)
        {
            PosedNow = 0;
            PoseMax = 0;
            if (!sim.n36.tells36 || sim.tells36 == null) return;
            int frame = Time.frameCount;
            int nh = 0, ns = 0, np = 0;
            Transform hp = FindParent(ref huskParent, "Husks"), sp = FindParent(ref swarmerParent, "Swarmers"), pp = FindParent(ref spitterParent, "Spitters");
            float k = 1f - Mathf.Exp(-UnityTuning.Tell36PoseEase * dt), kIn = 1f - Mathf.Exp(-UnityTuning.Tell36PoseEaseIn * dt);
            var all = sim.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                Transform parent;
                int index;
                int kind; // 0 husk, 1 swarmer, 2 spitter
                if (e is Swarmer) { parent = sp; index = ns++; kind = 1; }
                else if (e is Spitter) { parent = pp; index = np++; kind = 2; }
                else if (e is Husk h && h.kind == "husk") { parent = hp; index = nh++; kind = 0; }
                else continue;
                Tell36 t = sim.tells36.Of(e.id);
                bool live = t != null && t.live && IsScan(t.attack);
                if (!poses.TryGetValue(e.id, out Pose ps))
                {
                    if (!live) continue;
                    ps = new Pose();
                    poses[e.id] = ps;
                }
                ps.seenFrame = frame;
                float p = live ? (float)t.Progress : 0f;
                float backT = live ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(p / UnityTuning.Tell36PoseRiseAt)) : 0f;
                float strikeT = live && p >= UnityTuning.Tell36PoseStrikeAt ? 1f : 0f;
                if (live && strikeT > 0) backT = 0f; // the strike replaces the rear-back
                ps.back += (backT - ps.back) * (live && backT > ps.back ? kIn : k); // in fast (Tell36PoseEaseIn), out at Tell36PoseEase
                ps.strike += (strikeT - ps.strike) * (live ? 1f - Mathf.Exp(-UnityTuning.Tell36PoseEase * 3f * dt) : k);
                if (parent == null || index >= parent.childCount) continue;
                Transform root = parent.GetChild(index);
                if (!root.gameObject.activeSelf) continue;
                float amount = Mathf.Max(ps.back, ps.strike);
                if (amount < 0.001f) continue;
                PosedNow++;
                PoseMax = Mathf.Max(PoseMax, ps.back);
                Vector3 s = root.localScale;
                if (kind == 0)
                {
                    float rad = ps.back * UnityTuning.Tell36PoseHuskBack - ps.strike * UnityTuning.Tell36PoseStrike;
                    root.rotation = root.rotation * Quaternion.Euler(-rad * Mathf.Rad2Deg, 0, 0);
                    float grow = ps.back * UnityTuning.Tell36PoseHuskGrow;
                    root.localScale = new Vector3(s.x * (1f - grow * 0.4f), s.y * (1f + grow), s.z * (1f - grow * 0.4f));
                }
                else if (kind == 1)
                {
                    float c = ps.back * UnityTuning.Tell36PoseSwarmerCrouch;
                    root.localScale = new Vector3(s.x * (1f + c * 0.6f), s.y * (1f - c), s.z * (1f + c * 0.6f));
                    root.rotation = root.rotation * Quaternion.Euler(ps.back * 0.15f * Mathf.Rad2Deg, 0, 0); // nose down, ready to spring
                }
                else
                {
                    float rad = ps.back * UnityTuning.Tell36PoseSpitterBack - ps.strike * UnityTuning.Tell36PoseStrike * 0.5f;
                    root.rotation = root.rotation * Quaternion.Euler(-rad * Mathf.Rad2Deg, 0, 0);
                    if (root.childCount > 1)
                    {
                        Transform sac = root.GetChild(1); // the back sac (SpitterView.Make: capsule, sphere, cube)
                        sac.localScale = sac.localScale * (1f + ps.back * UnityTuning.Tell36PoseSpitterSac); // on top of the view's own swell (it sets the sac every frame)
                    }
                }
            }
            poseDrop.Clear();
            foreach (var kv in poses) if (kv.Value.seenFrame != frame || (kv.Value.back < 0.001f && kv.Value.strike < 0.001f && sim.tells36.Of(kv.Key) == null)) poseDrop.Add(kv.Key);
            foreach (int id in poseDrop) poses.Remove(id);
        }

        static bool IsScan(string a) => SurvivalSim.IsScanTell(a);

        static Enemy Find(SurvivalSim sim, int id)
        {
            var all = sim.enemies.all;
            for (int i = 0; i < all.Count; i++) if (all[i].id == id && all[i].alive) return all[i];
            return null;
        }

        void OnDestroy()
        {
            foreach (System.Action o in offs) o();
            offs.Clear();
        }
    }
}
