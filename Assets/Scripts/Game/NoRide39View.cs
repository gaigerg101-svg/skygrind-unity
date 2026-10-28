// NIGHT39 builder R1 (NIGHT39.md R1, ask 9; GG 2026-10-13: "a new wall material with a clear visual texture that indicates you cannot wall ride on
// it, not just any wall"): THE NO-RIDE SURFACE. View only: installs itself (no edit in the lead's SurvivalGame files; Doors38View's pattern), reads
// the level's campaign boxes tagged `noride: true` (Ride38.NoRideFrom: the same boxes the Mover refuses), and covers every vertical face of each with
// a dark backing plate and VERTICAL fins (corrugation): dull, dark, ridged; never cyan, never red-orange (UnityTuning.NoRide39.cs has the why).
// A ride panel stays smooth and lit with its horizontal cyan trim, so the two read as opposites at a glance.
// On when design.n39.ride39.noRideLook is true and NIGHT39 is not forced off (-skygrindOff39 and older); the Mover's refusal is NIGHT38's noride
// (on while any NIGHT38 block is on), so this look never claims a wall the Mover would let you ride. While on, Doors38View's NIGHT38 ribs (V's file,
// "No-ride ribs 38") are hidden: one pattern, not two. Unity objects are made on first use (never in field initialisers: L-035).
// Events: none (a view). Numbers: UnityTuning.NoRide39.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class NoRide39View : MonoBehaviour
    {
        /// <summary>For the reels / the lead's frames: the boxes and fins drawn on the current map (0: none or the look off).</summary>
        public static int Boxes, Fins;

        /// <summary>Is the look on in this session?</summary>
        public static bool On;

        SurvivalGame game;
        SurvivalMap builtFor;
        Transform root;
        int hideTick;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("NoRide39View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<NoRide39View>();
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.cfg == null || game.sim == null) { game = null; return false; }
            On = !N39Blocks.ForceOff && !N38Blocks.ForceOff && game.cfg.Design("n39.ride39.noRideLook");
            builtFor = null;
            return true;
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (builtFor != game.map) Build();
            Loom40Phases(); // NIGHT40 B4: THE LOOM's no-ride phases (drawn whatever noRideLook says)
            if (!On || Boxes == 0) return;
            if (--hideTick > 0) return;
            hideTick = UnityTuning.NoRide39RibsHideFrames;
            GameObject ribs = GameObject.Find("No-ride ribs 38");
            if (ribs != null && ribs.activeSelf) ribs.SetActive(false);
        }

        void Build()
        {
            builtFor = game.map;
            if (root != null) Destroy(root.gameObject);
            root = null;
            Boxes = Fins = 0;
            hideTick = 0;
            if (!On || game.map == null) return;
            Ride38.Box[] boxes = Ride38.NoRideFrom(game.map.json);
            if (boxes == null || boxes.Length == 0) return;
            root = BuildRoot(boxes, "No-ride 39", out int nb, out int nf);
            Boxes = nb;
            Fins = nf;
        }

        // NIGHT40 B4 (loom40, D-12; NIGHT40.md B4 "the NoRide39View look ON for these walls"): THE LOOM's no-ride phases. The sim switches the
        // Mover's noride set at the fight's begin and its leg change (SurvivalSim.N40.Loom.cs); this draws the phase's walls with this same look,
        // whatever design.n39.ride39.noRideLook says (they are the rule of the fight), blinking through the noRideWarnS warning first.
        Transform phaseRoot;
        int phaseVersion = -1;

        /// <summary>For the reels / the lead's frames: THE LOOM's phase walls drawn now and whether they blink (the warning).</summary>
        public static int PhaseBoxes;
        public static bool PhaseBlinking;

        void Loom40Phases()
        {
            SurvivalSim sim = game.sim;
            int v = sim != null ? sim.loom40NoRideVersion : -1;
            if (v != phaseVersion)
            {
                phaseVersion = v;
                if (phaseRoot != null) Destroy(phaseRoot.gameObject);
                phaseRoot = null;
                PhaseBoxes = 0;
                int ph = sim == null ? 0 : sim.loom40NoRideWarnLeft > 0 ? sim.loom40NoRideNext : sim.loom40NoRidePhase;
                Ride38.Box[] boxes = sim?.Loom40NoRideBoxes(ph);
                if (boxes != null && boxes.Length > 0) phaseRoot = BuildRoot(boxes, "No-ride 40 (THE LOOM)", out PhaseBoxes, out _);
            }
            PhaseBlinking = phaseRoot != null && sim != null && sim.loom40NoRideWarnLeft > 0;
            if (phaseRoot == null) return;
            bool show = !PhaseBlinking || Mathf.Repeat(Time.time * UnityTuning.Loom40NoRideBlinkHz, 1f) < 0.5f;
            if (phaseRoot.gameObject.activeSelf != show) phaseRoot.gameObject.SetActive(show);
        }

        /// <summary>The look over `boxes` (plates and vertical fins) under a new root.</summary>
        Transform BuildRoot(Ride38.Box[] boxes, string name, out int nBoxes, out int nFins)
        {
            nBoxes = nFins = 0;
            Transform root = new GameObject(name).transform;
            DontDestroyOnLoad(root.gameObject);
            int Fins = 0, Boxes = 0;
            Color side = Look.C(game.cfg, "colors.pillarSide");
            Color plate = side * UnityTuning.NoRide39PlateShade, fin = side * UnityTuning.NoRide39FinShade;
            plate.a = fin.a = 1f;
            var kit = new MeshKit();
            float pp = UnityTuning.NoRide39PlateProud, fp = UnityTuning.NoRide39FinProud, fw = UnityTuning.NoRide39FinWidth, pitch = UnityTuning.NoRide39FinPitch;
            foreach (Ride38.Box b in boxes)
            {
                Vector3 lo = WebSpace.ToUnity(b.min), hi = WebSpace.ToUnity(b.max);
                float x0 = Mathf.Min(lo.x, hi.x), x1 = Mathf.Max(lo.x, hi.x), z0 = Mathf.Min(lo.z, hi.z), z1 = Mathf.Max(lo.z, hi.z);
                float y0 = Mathf.Min(lo.y, hi.y), y1 = Mathf.Max(lo.y, hi.y), h = y1 - y0, ym = (y0 + y1) / 2;
                if (h <= 0.01f) continue;
                // the plates: one thin slab per side face, just proud of it
                kit.Cube(new Vector3(x0 - pp / 2, ym, (z0 + z1) / 2), new Vector3(pp, h, z1 - z0 + pp * 2), plate);
                kit.Cube(new Vector3(x1 + pp / 2, ym, (z0 + z1) / 2), new Vector3(pp, h, z1 - z0 + pp * 2), plate);
                kit.Cube(new Vector3((x0 + x1) / 2, ym, z0 - pp / 2), new Vector3(x1 - x0 + pp * 2, h, pp), plate);
                kit.Cube(new Vector3((x0 + x1) / 2, ym, z1 + pp / 2), new Vector3(x1 - x0 + pp * 2, h, pp), plate);
                // the fins: vertical, full height, along each face wide enough
                if (z1 - z0 >= UnityTuning.NoRide39MinFace)
                    for (float z = z0 + pitch / 2; z < z1 - fw / 2; z += pitch)
                    {
                        kit.Cube(new Vector3(x0 - fp / 2, ym, z), new Vector3(fp, h, fw), fin);
                        kit.Cube(new Vector3(x1 + fp / 2, ym, z), new Vector3(fp, h, fw), fin);
                        Fins += 2;
                    }
                if (x1 - x0 >= UnityTuning.NoRide39MinFace)
                    for (float x = x0 + pitch / 2; x < x1 - fw / 2; x += pitch)
                    {
                        kit.Cube(new Vector3(x, ym, z0 - fp / 2), new Vector3(fw, h, fp), fin);
                        kit.Cube(new Vector3(x, ym, z1 + fp / 2), new Vector3(fw, h, fp), fin);
                        Fins += 2;
                    }
                Boxes++;
            }
            MeshKit.Object("No-ride surface 39", root, kit.Build("noride39"), game.mats.stone, shadows: false);
            nBoxes = Boxes;
            nFins = Fins;
            return root;
        }

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
            if (phaseRoot != null) Destroy(phaseRoot.gameObject);
        }
    }
}
