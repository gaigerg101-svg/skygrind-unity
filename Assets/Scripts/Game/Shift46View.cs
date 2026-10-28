// NIGHT46 builder V1, the leftover row `shift46` (design.n46.shift46, a VIEW row: no sim bit; DECIDE46 N-2; DESIGN-RULES A-9 "State change",
// "Light / colour story"; A-6): a room's state change shows in its named fixture's light. Read only: the campaign's own state (Campaign24.room /
// phase / wave: what roomEnter, the waves and roomClear set) and the worm's crumble (SurvivalSim.wyrm31CrumbleState: what the "wyrm31" event
// says), through Shift46Rule (UnityTuning.Shift46.cs, pure). Owned and driven by Feel46View (one self-hook; nothing in a shared file):
//   the room's LAST WAVE is out   its named fixture (UnityTuning.Shift46Fixtures) turns hot red-orange (danger) and pulses;
//   the room is CLEARED            the same fixture turns steady cyan (the room is yours: A-6 "lit when the room clears");
//   THE KILN'S HEART crumbles      the KILN MOUTH's and the core's own lights (WyrmRing43View's) burn x Shift46CrumbleMult, pulsing through the tell.
// How a fixture "turns": an added point light at the fixture (its row's position, range x Shift46RangeMul) and, for a wall / ceiling slab, an
// emissive slab over it in the new colour (LightKit29 batches the slabs by colour, so the fixture's own mesh cannot be recoloured alone); both
// ease in and out (U-5). A level without the room or the fixture is skipped. Row off (or -skygrindOff46): nothing drawn, the borrowed lights
// put back. The reel's override (Force / ForceRoom + ForceState) frames a state at a chosen moment.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Shift46View
    {
        /// <summary>The reel's override of the row (null: NightRows.ViewOn(cfg, "shift46")).</summary>
        public static bool? Force;
        /// <summary>The reel's override of a room's state (ForceRoom null: the campaign's own).</summary>
        public static string ForceRoom;
        public static int ForceState;

        /// <summary>The a546 reel runs the shift46 scenario instead of feel46's when the Player is started with -skygrindShift46.</summary>
        public static bool ReelFlag => Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindShift46") >= 0;

        /// <summary>The reel's read: the room and state shown now, and how many fixtures this level has.</summary>
        public static string RoomNow = "", Line = "";
        public static int StateNow, Fixtures;
        public static float BlendNow;
        /// <summary>Each room's fixture (Unity space) in the level built last (the reel looks at it).</summary>
        public static readonly Dictionary<string, Vector3> FixtureAt = new Dictionary<string, Vector3>();

        sealed class Fix
        {
            public string room, id;
            public Light light;
            public Material mat;
            public GameObject slab;
            public float baseRange;
            public float blend; // 0 (its own look) .. 1 (the new colour)
            public int state;
        }

        readonly SurvivalGame game;
        readonly List<Fix> fixes = new List<Fix>();
        Transform root;
        bool built;
        float clock;
        Color hot, cyan;
        // k5: the KILN MOUTH's and the core's lights, found once the crumble starts (WyrmRing43View's own), and their multiplier now
        readonly List<Light> crumbleLights = new List<Light>();
        float crumbleBlend;

        public Shift46View(SurvivalGame g) => game = g;

        public static bool On(SurvivalGame g) => Force ?? (g != null && g.cfg != null && NightRows.ViewOn(g.cfg, "shift46"));

        /// <summary>Once a frame from Feel46View.Render (after the camera and the views: WyrmRing43View has set its lights' breathing this frame).</summary>
        public void Render(double frameDt)
        {
            SurvivalGame g = game;
            SurvivalSim sim = g != null ? g.sim : null;
            bool on = On(g) && sim != null && sim.campaignOn && sim.campaignLevel != null;
            float dt = (float)frameDt;
            clock += dt;
            if (on && !built) Build(g);
            string room = ForceRoom ?? (on && sim.campaign != null && sim.campaign.room >= 0 && sim.campaign.room < sim.campaignLevel.rooms.Count ? sim.campaignLevel.rooms[sim.campaign.room].id : "");
            int shownState = 0;
            float shownBlend = 0;
            foreach (Fix f in fixes)
            {
                int want = !on ? Shift46Rule.None : ForceRoom != null ? (f.room == ForceRoom ? ForceState : Shift46Rule.None) : Shift46Rule.RoomState(sim.campaign, sim.campaignLevel, f.room);
                if (want != Shift46Rule.None) f.state = want;
                float target = want != Shift46Rule.None ? 1f : 0f;
                f.blend = target > f.blend ? Mathf.Min(1f, f.blend + dt / UnityTuning.Shift46EaseIn) : Mathf.Max(0f, f.blend - dt / UnityTuning.Shift46EaseOut);
                bool show = f.blend > 0.001f;
                Color c = f.state == Shift46Rule.Cleared ? cyan : hot;
                float pulse = f.state == Shift46Rule.LastWave
                    ? 1f - UnityTuning.Shift46PulseDepth * (0.5f + 0.5f * Mathf.Sin(clock * UnityTuning.Shift46PulseHz * 2 * Mathf.PI)) : 1f;
                if (f.light != null)
                {
                    if (f.light.enabled != show) f.light.enabled = show;
                    if (show)
                    {
                        f.light.color = c;
                        f.light.intensity = UnityTuning.Shift46LightIntensity * UnityTuning.LightScale * f.blend * pulse;
                    }
                }
                if (f.slab != null)
                {
                    if (f.slab.activeSelf != show) f.slab.SetActive(show);
                    if (show) f.mat.SetColor("_BaseColor", c * (UnityTuning.Shift46Glow * f.blend * pulse));
                }
                if (f.room == room) { shownState = want; shownBlend = f.blend; }
            }
            Crumble(on ? (ForceRoom == "k5" ? ForceState : Shift46Rule.CrumbleState(sim.wyrm31CrumbleState)) : Shift46Rule.None, dt);
            if (room == "k5") { shownState = ForceRoom == "k5" ? ForceState : (on ? Shift46Rule.CrumbleState(sim.wyrm31CrumbleState) : 0); shownBlend = crumbleBlend; }
            RoomNow = room;
            StateNow = shownState;
            BlendNow = shownBlend;
        }

        void Crumble(int state, float dt)
        {
            float target = state == Shift46Rule.Crumble ? 1f : 0f;
            crumbleBlend = target > crumbleBlend ? Mathf.Min(1f, crumbleBlend + dt / UnityTuning.Shift46EaseIn) : Mathf.Max(0f, crumbleBlend - dt / UnityTuning.Shift46EaseOut);
            if (crumbleBlend <= 0f) { crumbleLights.Clear(); return; } // WyrmRing43View sets their own intensity every frame: nothing to put back
            if (crumbleLights.Count == 0 || crumbleLights[0] == null)
            {
                crumbleLights.Clear();
                foreach (Light l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) // once, at the crumble's onset
                    if (Array.IndexOf(UnityTuning.Shift46CrumbleLights, l.gameObject.name) >= 0) crumbleLights.Add(l);
            }
            // the tell pulses, the fall holds; on top of the breathing WyrmRing43View set this frame
            bool tell = game.sim != null && game.sim.wyrm31CrumbleState == 1 || (ForceRoom == "k5" && ForceState == Shift46Rule.Crumble);
            float pulse = tell ? 0.75f + 0.25f * Mathf.Sin(clock * UnityTuning.Shift46CrumbleHz * 2 * Mathf.PI) : 1f;
            if (tell && Light54.On(game.cfg))
            {
                // NIGHT54 light54 (fix 10): the crumble's 3 Hz pulse on the KILN MOUTH's lights (x3 at its peak) -> Light54.Calm's swell
                double hz = UnityTuning.Shift46CrumbleHz, d = 0.25;
                Light54.Calm(game.cfg, ref hz, ref d);
                pulse = 1f - (float)d + (float)d * Mathf.Sin(clock * (float)hz * 2 * Mathf.PI);
            }
            float mult = 1f + (UnityTuning.Shift46CrumbleMult - 1f) * crumbleBlend * pulse;
            foreach (Light l in crumbleLights) if (l != null && l.isActiveAndEnabled) l.intensity *= mult; // only a light its view drew this frame (an inactive one is not reset each frame)
        }

        /// <summary>The named fixtures of this level (campaign.lights rows), each with its added light and, for a slab, its overlay.</summary>
        void Build(SurvivalGame g)
        {
            built = true;
            hot = Look.Hex(UnityTuning.Shift46HotHex);
            cyan = LightKit29.ColorNamed(g.cfg, UnityTuning.Shift46ClearName);
            root = new GameObject("Shift46").transform;
            FixtureAt.Clear();
            var rows = new Dictionary<string, Dictionary<string, object>>();
            if (g.map != null && g.map.json != null && g.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp
                && camp.TryGetValue("lights", out object lo) && lo is List<object> lights)
                foreach (object o in lights)
                    if (o is Dictionary<string, object> r && r.TryGetValue("id", out object io) && io is string id) rows[id] = r;
            var names = new List<string>();
            for (int i = 0; i < UnityTuning.Shift46Fixtures.GetLength(0); i++)
            {
                string room = UnityTuning.Shift46Fixtures[i, 0], id = UnityTuning.Shift46Fixtures[i, 1];
                if (!rows.TryGetValue(id, out Dictionary<string, object> r)) continue; // the level lacks it: skipped
                Vector3 at = U(r, "at", Vector3.zero);
                Vector3 n = U(r, "normal", Vector3.up);
                if (n.sqrMagnitude < 1e-6f) n = Vector3.up;
                n.Normalize();
                string kind = r.TryGetValue("kind", out object ko) ? ko as string : "";
                float range = r.TryGetValue("range", out object rg) ? (float)PortData.Num(rg) : 8f;
                var f = new Fix { room = room, id = id, baseRange = range };
                var lg = new GameObject("shift46 light " + id);
                lg.transform.SetParent(root, false);
                lg.transform.position = at + n * (kind == "pool" ? 1.5f : 0.6f); // where LightKit29 stands the fixture's own light
                f.light = lg.AddComponent<Light>();
                f.light.type = LightType.Point;
                f.light.shadows = LightShadows.None;
                f.light.range = range * UnityTuning.Shift46RangeMul;
                f.light.intensity = 0;
                f.light.enabled = false;
                if (kind != "pool" && g.mats != null && g.mats.neon != null)
                {
                    float w = 1, h = 1;
                    if (r.TryGetValue("size", out object so) && so is List<object> sl && sl.Count >= 2) { w = (float)PortData.Num(sl[0]); h = (float)PortData.Num(sl[1]); }
                    Vector3 up = kind == "ceiling" || Mathf.Abs(n.y) > 0.9f ? Vector3.forward : Vector3.up;
                    GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    UnityEngine.Object.Destroy(slab.GetComponent<Collider>()); // view only: the game's collision is its own (CLAUDE.md rule 4)
                    slab.name = "shift46 slab " + id;
                    slab.transform.SetParent(root, false);
                    slab.transform.position = at + n * (UnityTuning.Shift46OverlayDepth * 0.5f);
                    slab.transform.rotation = Quaternion.LookRotation(n, up);
                    slab.transform.localScale = new Vector3(w * UnityTuning.Shift46OverlayScale, h * UnityTuning.Shift46OverlayScale, UnityTuning.Shift46OverlayDepth);
                    f.mat = FxDraw.Opaque(g.mats.neon, "shift46 " + id, hot);
                    var mr = slab.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = f.mat;
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    slab.SetActive(false);
                    f.slab = slab;
                }
                fixes.Add(f);
                FixtureAt[room] = at;
                names.Add(room + ":" + id + (f.slab != null ? "+slab" : ""));
            }
            Fixtures = fixes.Count;
            Line = $"shift46 fixtures {fixes.Count} [{string.Join(", ", names)}]";
        }

        static Vector3 U(Dictionary<string, object> r, string key, Vector3 fallback) =>
            r.TryGetValue(key, out object o) && o is List<object> l && l.Count >= 3
                ? new Vector3((float)PortData.Num(l[0]), (float)PortData.Num(l[1]), (float)-PortData.Num(l[2])) : fallback; // web -> Unity (z flips)

        /// <summary>A new game (a scene reload): its objects go with the old scene's; nothing to put back (the crumble's lights are set fresh each frame).</summary>
        public void Dispose()
        {
            if (root != null) UnityEngine.Object.Destroy(root.gameObject);
        }
    }
}
