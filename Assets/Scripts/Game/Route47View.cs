// NIGHT47 builder L (crownLook47, a VIEW row; NIGHT47.md G42 "the goal visible from the start and from each room"; L-cards.md section 4; the lead's
// round-2 note 6: "a column only above the roof line seen through the slot windows"): THE CROWN'S LIGHT seen through the way-on windows. The 300 m
// column over the shaft cannot be seen through level 5's closed roofs (n47spire view 0 / 10: LevelValidate29 closedRoom); so each lit window
// (gen-n47-spire.cjs's campaign.lights rows <room>-crown47*, LightKit29's white boundary slabs) gets, in front of it, a narrow vertical red-gold shaft
// of light (the column, as if seen through the glass) over a faint red-gold wash: the same light in every room, brighter the nearer the room is
// to the shaft, pulsing slowly (the CROWN's toll). Self-hooked (Feel46View's way: a hidden object made at scene load finds the live SurvivalGame;
// no shared file changes). Row off (or -skygrindOff47), or a level without the windows: nothing is built. View only: no collider, no sim read.
// Round 4 (the lead's item 3: "a view-only object drawn after the opaque roofs with a depth test against the sky"): THE COLUMN itself, a 300 m
// red-gold cylinder over the shaft drawn by SKYGRIND/Column47 (Assets/Shaders/Resources/Column47.shader) in the transparent queue after the roofs:
// full where nothing is in front of it (open sky), Route47ColumnThrough of it where a roof or wall is (ZTest Greater, Enemy.shader's x-ray rule).
// When the shader is missing or unsupported, no column (the windows stay).
// Numbers: UnityTuning.Crown47.cs (Route47*).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Route47View : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Route47View>() != null) return;
            var go = new GameObject("Route47");
            DontDestroyOnLoad(go);
            go.AddComponent<Route47View>();
        }

        /// <summary>For the reels / frames: windows given a column now.</summary>
        public static int Columns;
        /// <summary>For the reels / frames: THE COLUMN is built (its shader found and supported).</summary>
        public static bool ColumnBuilt;

        SurvivalGame game;
        SurvivalMap builtFor;
        Transform root;
        readonly List<Renderer> shafts = new List<Renderer>(), washes = new List<Renderer>();
        readonly List<float> nearness = new List<float>();
        Material mat, columnMat;
        Renderer column;
        MaterialPropertyBlock block;
        int baseColor = -1; // Shader.PropertyToID at first use, never in a field initializer (L-035)

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.cfg == null || g.mats == null) return;
            if (g != game) { game = g; builtFor = null; }
            bool on = NightRows.ViewOn(g.cfg, "crownLook47");
            if (!on) { if (root != null) root.gameObject.SetActive(false); return; }
            if (builtFor != g.map) Build(g);
            if (root == null) return;
            if (!root.gameObject.activeSelf) root.gameObject.SetActive(true);
            float t = Time.time;
            float pulse = 0.85f + 0.15f * Mathf.Sin(t * UnityTuning.Route47PulseHz * 2f * Mathf.PI);
            float fx = (float)System.Math.Max(0, System.Math.Min(g.cfg.movement.feel.effectsIntensityMax, g.cfg.movement.settings.effectsIntensity));
            Color own = Look.Hex(UnityTuning.CLook47RedGoldHex);
            Color c = Colours63.Route("goal47", own); // NIGHT63 C1 (colours63, read every frame): the boundary's pale violet-white, not danger's red-gold, with the row on; NIGHT64 C4: the shafts' colour
            Color cc = Goal64Column(g.cfg, own, c); // NIGHT64 C4 (goal64, read every frame): the column alone in the colour table's goal role
            if (column != null)
            {
                block.Clear(); block.SetColor(baseColor, cc * (UnityTuning.Route47ColumnGain * pulse * Mathf.Max(0.3f, fx))); column.SetPropertyBlock(block);
            }
            for (int i = 0; i < shafts.Count; i++)
            {
                float k = Mathf.Lerp(UnityTuning.Route47GainFar, UnityTuning.Route47GainNear, nearness[i]) * pulse * Mathf.Max(0.3f, fx);
                block.Clear(); block.SetColor(baseColor, c * k); shafts[i].SetPropertyBlock(block);
                block.Clear(); block.SetColor(baseColor, c * (k * UnityTuning.Route47WashShare)); washes[i].SetPropertyBlock(block);
            }
        }

        // --- NIGHT64 builder D (slate C4, goal64; Goal64Math.N64D.cs): THE SPLIT. Line 58's one colour stays the shafts'; the column reads the row ---

        /// <summary>For a reel: the last goal64 proof line, and the times its live value changed.</summary>
        public static string Goal64Line = "goal64: not read";
        public static int Goal64Toggles;

        static string[] goalArgs;
        GameConfig goalCfg;
        bool goalKnown, goalSeen, goalWas;
        string goalRole;
        Color goalColour;

        static string[] GoalArgs()
        {
            if (goalArgs == null)
            {
                try { goalArgs = System.Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (System.Exception) { goalArgs = new string[0]; }
            }
            return goalArgs;
        }

        /// <summary>The column's colour this frame: the colour table's goal role (Colours63Math.From: read once a config, whatever colours63's
        /// enabled, never Colours63.Route) while goal64 is live, else the shafts' colour, as before. Allocation-free after a config's first
        /// frame, except the proof line when the live value changes.</summary>
        Color Goal64Column(GameConfig cfg, Color own, Color shafts)
        {
            if (!ReferenceEquals(cfg, goalCfg))
            {
                goalCfg = cfg;
                goalSeen = false;
                goalKnown = Goal64Math.GoalColour(Colours63Math.From(cfg), own.r, own.g, own.b, out double r, out double gg, out double b, out goalRole);
                goalColour = new Color((float)r, (float)gg, (float)b, own.a);
            }
            bool live = Goal64Math.Live(cfg, GoalArgs());
            Color col = Goal64Math.Column(live, goalKnown, goalColour, shafts);
            if (!goalSeen || live != goalWas)
            {
                if (goalSeen) Goal64Toggles++;
                goalSeen = true;
                goalWas = live;
                Goal64Line = Goal64Math.LiveLine(live, goalRole, own, col, shafts, ColumnBuilt, FeelClock.FrameCount, FeelClock.Now);
                Debug.Log(Goal64Line);
            }
            return col;
        }

        void Build(SurvivalGame g)
        {
            builtFor = g.map;
            if (root != null) Destroy(root.gameObject);
            root = null;
            shafts.Clear(); washes.Clear(); nearness.Clear();
            Columns = 0;
            column = null;
            ColumnBuilt = false;
            if (g.map?.json == null || !g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)
                || !camp.TryGetValue("lights", out object lo) || !(lo is List<object> lights)) return;
            if (block == null) block = new MaterialPropertyBlock();
            if (baseColor < 0) baseColor = Shader.PropertyToID("_BaseColor");
            if (mat == null) mat = FxDraw.Additive(g.mats.neon, "Route47 crown light");
            root = new GameObject("Route47 windows").transform;
            // the shaft's place (the v11 core's axis) for the nearness; the farthest window sets the scale
            var rows = new List<(Vector3 at, Vector3 n, float w, float h, float d)>();
            foreach (object o in lights)
            {
                if (!(o is Dictionary<string, object> r) || !(r.TryGetValue("id", out object io) && io is string id) || id.IndexOf("-crown47", System.StringComparison.Ordinal) < 0) continue;
                var at = PortData.Arr(r["at"]);
                var sz = PortData.Arr(r["size"]);
                Vector3 p = new Vector3((float)PortData.Num(at[0]), (float)PortData.Num(at[1]), (float)-PortData.Num(at[2]));
                float d = Mathf.Abs((float)PortData.Num(at[2]) - (float)UnityTuning.Route47ShaftZ);
                rows.Add((p, Vector3.back, (float)PortData.Num(sz[0]), (float)PortData.Num(sz[1]), d)); // web +z (into the room) is Unity -z
            }
            float far = 1f;
            foreach (var row in rows) far = Mathf.Max(far, row.d);
            foreach (var row in rows)
            {
                Vector3 front = row.at + row.n * UnityTuning.Route47Lift;
                Renderer wash = Slab("Route47 wash", front, new Vector3(row.w * 0.96f, row.h * 0.96f, 0.02f));
                Renderer shaft = Slab("Route47 shaft", front + row.n * 0.01f, new Vector3(Mathf.Min(row.w * 0.3f, UnityTuning.Route47ShaftW), row.h * 0.98f, 0.02f));
                washes.Add(wash);
                shafts.Add(shaft);
                nearness.Add(1f - row.d / far);
                Columns++;
            }
            if (rows.Count > 0) BuildColumn();
        }

        /// <summary>THE COLUMN: Unity's cylinder (y -1..1, radius 0.5) stretched from Route47ColumnBottom to Route47ColumnTop over the shaft's axis.</summary>
        void BuildColumn()
        {
            if (columnMat == null)
            {
                Shader sh = Shader.Find("SKYGRIND/Column47");
                if (sh == null || !sh.isSupported) return;
                columnMat = new Material(sh) { name = "Route47 column" };
                columnMat.SetFloat("_Through", UnityTuning.Route47ColumnThrough);
            }
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(s.GetComponent<Collider>()); // view only (CLAUDE.md rule 4)
            s.name = "Route47 column";
            s.transform.SetParent(root, false);
            float h = UnityTuning.Route47ColumnTop - UnityTuning.Route47ColumnBottom;
            s.transform.position = new Vector3(UnityTuning.Route47ColumnX, UnityTuning.Route47ColumnBottom + h * 0.5f, (float)-UnityTuning.Route47ShaftZ); // web +z is Unity -z
            s.transform.localScale = new Vector3(UnityTuning.Route47ColumnW, h * 0.5f, UnityTuning.Route47ColumnW);
            var mr = s.GetComponent<MeshRenderer>();
            mr.sharedMaterial = columnMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            column = mr;
            ColumnBuilt = true;
        }

        Renderer Slab(string name, Vector3 at, Vector3 size)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(s.GetComponent<Collider>()); // view only: the game's collision is its own (CLAUDE.md rule 4)
            s.name = name;
            s.transform.SetParent(root, false);
            s.transform.position = at;
            s.transform.localScale = size;
            var mr = s.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return mr;
        }
    }
}
