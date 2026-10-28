// NIGHT29 round 2 (builder V): the authored props (`campaign.props`: crate, pillar, console, press, floorStrip, cable, padMarker, frost,
// healthPickup, weaponPickup) as simple blocky shapes in the dusk palette, and the hazard strips' pulse. View only: NO collider here (a
// prop that blocks has `collider: true` and its box is the plan's own `boxes`). `at` is the prop's CENTRE, `size` its [w, h, d], `yawDeg`
// its turn about the vertical (the validator's LevelPlanBox). Dark stone with a lighter lid; cyan neon (player-usable) on collars and
// panels; flat props (floorStrip, padMarker, frost) are lit quads on the floor; a weapon pickup is a cyan glow pillar with a spinning,
// bobbing box; a health pickup the same in green and half as tall. Red-orange is never used here (danger only: the hazard strips).
// The hazard strips (`hazard.rect` of a room with a hazard) show the sim's state: rest = a faint mark, telegraph = a pulse that quickens,
// live = bright and steady. Built by LightKit29.Build (props under design.n29.props.enabled; hazards whenever the kit exists).
// NIGHT35 W (design.n35.pickup35, view row; off with -skygrindOff35): a weapon pickup shows its own model with a cyan rim and its name on approach,
// a pickup in a room ahead of the player its dark silhouette, the health pickup a cyan MEND vial (Pickup35View.cs); the glow pillar stays (cyan for both).
// NOTE the pickups are SHOWN only: nothing in the sim consumes a prop row (Core and Game have no pickup code for them).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class PropsView29Math
    {
        /// <summary>NIGHT74 L (R's finding, the proper fix): the neon key of a flat prop's quad. Decor54.DecorKind moves every "Props flat floorStrip..." renderer from cyan to the level's accent, but a
        /// floorStrip row that names a JOB and is not `decor: true` is not decor: it is a usable floor mark (THE MARK a2-P1, THE RIDE'S END a3-RE, THE SLIDE LINE a5-P2: cyan, player-usable, job "line"), and the
        /// recolour drew it dark purple in the Player. Such a row gets its own key "flat floorMark" (a renderer of its own: the quads of one key are merged into one object), which DecorKind does not match; the
        /// decor floorStrips (no job: the 79 rows of the 22 old plan files, n29 - n37, in cyan, white, amber, green and orange) keep "flat floorStrip" and move as before. Brightness: a floorMark keeps a floorStrip's (PropFlatBrightness x Look30FloorStripScale).</summary>
        public static string FlatKey(string kind, Dictionary<string, object> row, string colourName) => "flat " + (IsUsableMark(kind, row) ? "floorMark" : kind) + (colourName ?? "");

        /// <summary>A floorStrip row with a non-empty `job` and no `decor: true`: a usable floor mark, not decor (decor rows: dress33's `decor: true`, and the old plans' strips, which carry no job).</summary>
        public static bool IsUsableMark(string kind, Dictionary<string, object> row)
        {
            if (kind != "floorStrip" || row == null) return false;
            if (row.TryGetValue("decor", out object d) && d is bool db && db) return false;
            return row.TryGetValue("job", out object j) && j is string js && js.Length > 0;
        }

        /// <summary>The hazard strip's brightness (x the danger colour) from the sim's state (0 rest, 1 telegraph, 2 live), the seconds into the telegraph and its length.</summary>
        public static float HazardBrightness(int state, double teleAge, double teleLength, bool reducedMotion)
        {
            if (state == 2) return UnityTuning.HazardLive;
            if (state != 1) return UnityTuning.HazardOff;
            if (reducedMotion) return UnityTuning.HazardReduced;
            double u = teleLength > 0 ? System.Math.Max(0, System.Math.Min(1, teleAge / teleLength)) : 1;
            double hz = UnityTuning.HazardPulseHzFrom + (UnityTuning.HazardPulseHzTo - UnityTuning.HazardPulseHzFrom) * u;
            double wave = 0.5 + 0.5 * System.Math.Sin(2 * System.Math.PI * hz * teleAge);
            return (float)(UnityTuning.HazardTeleLow + (UnityTuning.HazardTeleHigh - UnityTuning.HazardTeleLow) * wave);
        }
    }

    public sealed class PropsView29 : MonoBehaviour
    {
        SurvivalGame game;
        LevelDef level;
        readonly List<Transform> spinners = new List<Transform>();
        readonly List<float> spinY = new List<float>();
        // NIGHT31 W (N31Fixes.On): the pickups by id, their spinning box and their own glow (hidden while Campaign24.IsPickupTaken); the reels read PickupShown31
        readonly List<string> pickupIds = new List<string>();
        readonly List<GameObject> pickupBoxes = new List<GameObject>(), pickupGlows = new List<GameObject>();
        public static readonly Dictionary<string, bool> PickupShown31 = new Dictionary<string, bool>();
        /// <summary>NIGHT40 S1 (rods40): the prop rows this level's build skipped (the reels read it; 0 with the row off).</summary>
        public static int Rods40PropsHidden;
        readonly List<(Material mat, Color color)> hazardMats = new List<(Material, Color)>();
        readonly List<int> hazardRoom = new List<int>();
        int lastState;
        float teleStart;
        public int propCount, hazardCount;
        public int kit33Drawn; // NIGHT33 K: rows the arena kit drew (the kit33 reel reads it through Last)
        public int kit33Objects, kit33Vertices, kit33Boxes; // NIGHT33 K round 2: the kit's mesh objects (= its draws before culling), vertices and boxes
        public static PropsView29 Last; // NIGHT33 K: the view built for the current map (the kit33 reel's count)

        public static PropsView29 Create(SurvivalGame g, Transform parent, SurvivalMaterials mats, bool propsOn)
        {
            var go = new GameObject("Props 29");
            go.transform.SetParent(parent, false);
            var v = go.AddComponent<PropsView29>();
            Last = v;
            v.Build(g, mats, propsOn);
            return v;
        }

        static Vector3 U(List<object> a) => new Vector3((float)PortData.Num(a[0]), (float)PortData.Num(a[1]), (float)-PortData.Num(a[2])); // web -> Unity (z flips)

        void Build(SurvivalGame g, SurvivalMaterials mats, bool propsOn)
        {
            game = g;
            GameConfig cfg = g.cfg;
            level = LevelDef.From(g.map.json);
            Dictionary<string, object> campaign = PortData.Obj(g.map.json["campaign"]);
            float bright = (float)cfg.Num("design.n27.roof27.stripBrightness");
            Color cyan = Look.C(cfg, "colors.usable");
            BuildHazards(g, mats, cfg);
            if (!propsOn || !campaign.TryGetValue("props", out object po) || !(po is List<object> rows)) return;
            // NIGHT31 W (design.n31.unlocks): a room's pickup31 row (i1a's gauntlet) is drawn as a weapon pickup prop (the sim joins it to the pickups at the
            // run's start: LevelDef.UseUnlocks31); the k1 THUNDERCLAP prop's size. Off (the row off or -skygrindOff31): not drawn.
            if (!N31Blocks.ForceOff && N31Blocks.From(cfg).unlocks && level.pickups31.Count > 0)
            {
                rows = new List<object>(rows);
                foreach (PickupDef p31 in level.pickups31)
                    rows.Add(new Dictionary<string, object>
                    {
                        ["id"] = p31.id, ["room"] = p31.room, ["kind"] = p31.kind,
                        ["at"] = new List<object> { p31.at.x, p31.at.y, p31.at.z },
                        ["weapon"] = p31.weapon, // NIGHT35 W pickup35: the model shown
                        ["size"] = new List<object> { (double)UnityTuning.Pickup31Size, (double)UnityTuning.Pickup31Size, (double)UnityTuning.Pickup31Size }, // PortData.Num reads doubles only (a boxed float threw: the lead's n31-r6 LevelSelect29, 470888 NREs)
                    });
            }
            Color top = Look.C(cfg, "colors.pillarTop"), side = Look.C(cfg, "colors.pillarSide");
            Color lid = Color.Lerp(top, Color.white, UnityTuning.PropTopLift);
            var stone = new MeshKit();
            var neon = new Dictionary<string, MeshKit>(); // by colour name
            var neonColor = new Dictionary<string, Color>();
            MeshKit Neon(string name, Color c)
            {
                if (!neon.TryGetValue(name, out MeshKit k)) { neon[name] = k = new MeshKit(); neonColor[name] = c; }
                return k;
            }
            // NIGHT33 K (design.n33.kit33): the arena kit draws its own kinds (dress33's decor and cover rows: only the n33 files have them) and,
            // with the kit33 view row on, NIGHT32's heatVent / checkpoint / plinth / slot as themselves (A3: they fell to the crate default).
            // -skygrindOff33 or the row off: those four stay crates (NIGHT32's view).
            bool kit33On = N33Fixes.ViewRowOn(cfg, "kit33");
            bool pickup35 = N35Fixes.ViewRowOn(cfg, "pickup35"); // NIGHT35 W: the pickups show what they are (Pickup35View.cs); off: NIGHT34's cubes
            var kit33 = new Kit33Props(cfg, level != null ? level.id : null);
            // NIGHT40 S1 (rods40, a view row): a collider-less rod in the play volume (a truss across the room, its chain and lamp) is not drawn (Rods40.cs)
            LightPlan33.Level rods40Lv = Rods40.On(cfg) ? LightPlan33.Read(g.map.json) : null;
            Rods40PropsHidden = 0;
            foreach (object ro in rows)
            {
                if (!(ro is Dictionary<string, object> r) || !r.ContainsKey("at") || !r.ContainsKey("size")) continue;
                if (rods40Lv != null && Rods40.HideProp(rods40Lv, r, out _)) { Rods40PropsHidden++; continue; }
                string kind33 = r.TryGetValue("kind", out object k33) && k33 is string ks33 ? ks33 : "crate";
                if (Kit33Props.Kinds.Contains(kind33) || (kit33On && Kit33Props.Legacy.Contains(kind33)))
                {
                    List<object> s33 = PortData.Arr(r["size"]);
                    float yaw33 = r.TryGetValue("yawDeg", out object y33) ? -(float)PortData.Num(y33) : 0;
                    string id33 = r.TryGetValue("id", out object i33) ? i33 as string : null;
                    string room33 = r.TryGetValue("room", out object rm33) ? rm33 as string : null;
                    float[] bands33 = null; // round 2: the row's clear bands (the wall-ride strip's height, L's sockets), dress33's `gap` [lo, hi, lo, hi, ...]
                    if (r.TryGetValue("gap", out object gp33) && gp33 is List<object> gl33 && gl33.Count >= 2)
                    {
                        bands33 = new float[gl33.Count & ~1];
                        for (int bi = 0; bi < bands33.Length; bi++) bands33[bi] = (float)PortData.Num(gl33[bi]);
                    }
                    if (kit33.Draw(id33, kind33, U(PortData.Arr(r["at"])), Quaternion.Euler(0, yaw33, 0), (float)PortData.Num(s33[0]), (float)PortData.Num(s33[1]), (float)PortData.Num(s33[2]), room33, bands33))
                    {
                        propCount++;
                        continue;
                    }
                }
                string kind = r.TryGetValue("kind", out object ko) && ko is string ks ? ks : "crate";
                Vector3 at = U(PortData.Arr(r["at"]));
                List<object> sz = PortData.Arr(r["size"]);
                float w = (float)PortData.Num(sz[0]), h = (float)PortData.Num(sz[1]), d = (float)PortData.Num(sz[2]);
                // the plan's yaw is about +y in web space; web z flips to Unity, so the turn flips sign
                float yaw = r.TryGetValue("yawDeg", out object yo) ? -(float)PortData.Num(yo) : 0;
                Quaternion q = Quaternion.Euler(0, yaw, 0);
                Vector3 right = q * Vector3.right, fwd = q * Vector3.forward;
                string cname = r.TryGetValue("color", out object co) && co is string cs ? cs : null;
                Color c = cname != null ? LightKit29.ColorNamed(cfg, cname) : cyan;
                switch (kind)
                {
                    case "floorStrip":
                    case "padMarker":
                    case "frost":
                    {
                        Color fc = kind == "frost" ? LightKit29.ColorNamed(cfg, "white") : kind == "padMarker" ? cyan : c;
                        Flat(Neon(PropsView29Math.FlatKey(kind, r, cname), fc), at, right, fwd, w, d); // NIGHT74 L: a floorStrip with a job is a usable mark, named apart so Decor54 does not recolour it (PropsView29Math.FlatKey)
                        break;
                    }
                    case "cable":
                        LightKit29.OrientedBox(stone, at, right, Vector3.up, fwd, w * 0.5f, h * 0.5f, d * 0.5f, side);
                        break;
                    case "weaponPickup":
                    case "healthPickup":
                    {
                        bool weapon = kind == "weaponPickup";
                        Color pc = weapon || pickup35 ? cyan : Look.Hex(UnityTuning.FixtureGreenHex); // NIGHT35 pickup35: the health vial's glow is cyan too (D-5)
                        string pid = r.TryGetValue("id", out object pio) ? pio as string : null;
                        // NIGHT31 W (N31Fixes.On): each pickup's glow is its own object (keyed by the pickup's id) so a TAKEN pickup hides (Update reads
                        // Campaign24.IsPickupTaken). -skygrindOff31: NIGHT30's view (one merged glow, never hidden).
                        bool own = N31Fixes.On && pid != null;
                        string key = (weapon ? "pickup w" : "pickup h") + (own ? " #" + pid : "");
                        float gh = UnityTuning.PickupGlowH * (weapon ? 1f : UnityTuning.PickupGlowHealthShare);
                        float floorY = at.y - UnityTuning.PickupFloat; // the plans float a pickup about PickupFloat m over its floor
                        float gw = UnityTuning.PickupGlowW;
                        if (!pickup35) // NIGHT35 pickup35 (n35-r2): the pillar crossed the model and its name: Pickup35View draws a floor ring instead
                            LightKit29.OrientedBox(Neon(key + " glow", pc), new Vector3(at.x, floorY + gh * 0.5f, at.z), Vector3.right, Vector3.up, Vector3.forward, gw * 0.5f, gh * 0.5f, gw * 0.5f, Color.white);
                        if (pickup35)
                        {
                            // NIGHT35 W (design.n35.pickup35, Pickup35View.cs): the weapon's own model / the locked silhouette / the MEND vial, with its name on approach
                            string word = r.TryGetValue("weapon", out object wo) ? wo as string : null;
                            string room = r.TryGetValue("room", out object rmo) ? rmo as string : null;
                            Pickup35View pv = Pickup35View.Build(g, transform, mats, kind, pid ?? "", word, room, level, at);
                            if (own) { pickupIds.Add(pid); pickupBoxes.Add(pv.gameObject); pickupGlows.Add(null); }
                            break;
                        }
                        // the spinning box: its own object (turned each frame), neon, brighter than the glow
                        var box = new GameObject(kind + " " + (r.TryGetValue("id", out object io) ? io : ""));
                        box.transform.SetParent(transform, false);
                        box.transform.position = at;
                        var mk = new MeshKit();
                        mk.Cube(Vector3.zero, new Vector3(w, h, d), Color.white);
                        var mat = new Material(mats.neon);
                        // NIGHT30 round 5 (look30): at NIGHT29's brightness the health pickup bloomed to a white human-shaped figure (r5 f6): dimmed so it reads green / cyan
                        mat.SetColor("_BaseColor", pc * bright * UnityTuning.PickupBoxBrightness * (N30Fixes.ViewRowOn(cfg, "look30") ? UnityTuning.Look30PickupScale : 1f));
                        box.AddComponent<MeshFilter>().sharedMesh = mk.Build("pickup box29");
                        var mr = box.AddComponent<MeshRenderer>();
                        mr.sharedMaterial = mat;
                        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        mr.receiveShadows = false;
                        spinners.Add(box.transform);
                        spinY.Add(at.y);
                        if (own) { pickupIds.Add(pid); pickupBoxes.Add(box); pickupGlows.Add(null); }
                        break;
                    }
                    case "console":
                        LightKit29.OrientedBox(stone, at, right, Vector3.up, fwd, w * 0.5f, h * 0.5f, d * 0.5f, side);
                        LightKit29.OrientedBox(Neon("console panel", cyan), at + Vector3.up * (h * 0.5f), right, Vector3.up, fwd, w * 0.5f * UnityTuning.PropPanelShare, UnityTuning.PropPanelH * 0.5f, d * 0.5f * UnityTuning.PropPanelShare, Color.white);
                        break;
                    case "pillar":
                    case "press":
                    case "crate":
                    default:
                        LightKit29.OrientedBox(stone, at, right, Vector3.up, fwd, w * 0.5f, h * 0.5f, d * 0.5f, side);
                        Flat(stone, at + Vector3.up * (h * 0.5f + 0.001f), right, fwd, w, d, lid); // the lighter lid
                        if (kind == "pillar" && h > 1f)
                        {
                            float cw = w + 2 * UnityTuning.PropCollar, cd = d + 2 * UnityTuning.PropCollar;
                            LightKit29.OrientedBox(Neon("pillar collar", cyan), at + Vector3.up * (h * 0.5f - UnityTuning.PropCollarDrop), right, Vector3.up, fwd, cw * 0.5f, UnityTuning.PropCollarH * 0.5f, cd * 0.5f, Color.white);
                        }
                        break;
                }
                propCount++;
            }
            if (stone.Vertices > 0) MeshKit.Object("Props stone", transform, stone.Build("props29 stone"), mats.stone, shadows: false);
            kit33.Build(transform, mats, bright);
            kit33Drawn = kit33.drawn;
            kit33Objects = kit33.objects; kit33Vertices = kit33.vertices; kit33Boxes = kit33.boxes;
            foreach (var kv in neon)
            {
                var m = new Material(mats.neon);
                float s = kv.Key.StartsWith("flat") ? UnityTuning.PropFlatBrightness : kv.Key.StartsWith("pickup") ? 1f : UnityTuning.PropNeonScale;
                // NIGHT30 item 8 (builder V, design.n30.look30): a floor strip lights nothing and was the brightest thing in GG's frame 170047: dimmed to a trim
                if ((kv.Key.StartsWith("flat floorStrip") || kv.Key.StartsWith("flat floorMark")) && N30Fixes.ViewRowOn(cfg, "look30")) s *= UnityTuning.Look30FloorStripScale; // NIGHT74 L: a usable floorMark keeps the floorStrip's brightness
                if (kv.Key.StartsWith("pickup") && N30Fixes.ViewRowOn(cfg, "look30")) s *= UnityTuning.Look30PickupScale; // round 5: the pickup's glow column too
                m.SetColor("_BaseColor", neonColor[kv.Key] * bright * s);
                GameObject neonGo = MeshKit.Object("Props " + kv.Key, transform, kv.Value.Build("props29 " + kv.Key), m, shadows: false);
                int hash = kv.Key.IndexOf(" #", System.StringComparison.Ordinal);
                if (hash >= 0)
                {
                    int pi = pickupIds.IndexOf(kv.Key.Substring(hash + 2)); // NIGHT31 W: this pickup's own glow
                    if (pi >= 0) pickupGlows[pi] = neonGo;
                }
            }
        }

        static void Flat(MeshKit k, Vector3 centre, Vector3 right, Vector3 fwd, float w, float d) => Flat(k, centre, right, fwd, w, d, Color.white);

        static void Flat(MeshKit k, Vector3 centre, Vector3 right, Vector3 fwd, float w, float d, Color col)
        {
            Vector3 a = right * (w * 0.5f), b = fwd * (d * 0.5f);
            k.Quad(centre - a - b, centre + a - b, centre + a + b, centre - a + b, Vector3.up, col);
        }

        void BuildHazards(SurvivalGame g, SurvivalMaterials mats, GameConfig cfg)
        {
            Color danger = Look.C(cfg, "colors.enemy");
            for (int i = 0; i < level.rooms.Count; i++)
            {
                RoomDef r = level.rooms[i];
                if (!r.hasHazard || r.hx1 <= r.hx0 || r.hz1 <= r.hz0) continue;
                var k = new MeshKit();
                float y = UnityTuning.HazardLift;
                k.Quad(new Vector3((float)r.hx0, y, (float)-r.hz0), new Vector3((float)r.hx1, y, (float)-r.hz0), new Vector3((float)r.hx1, y, (float)-r.hz1), new Vector3((float)r.hx0, y, (float)-r.hz1), Vector3.up, Color.white);
                var m = new Material(mats.neon);
                m.SetColor("_BaseColor", danger * UnityTuning.HazardOff);
                MeshKit.Object("Hazard strip " + r.id, transform, k.Build("hazard29 " + r.id), m, shadows: false);
                hazardMats.Add((m, danger));
                hazardRoom.Add(i);
                hazardCount++;
            }
        }

        void Update()
        {
            if (game == null) return;
            float t = FeelClock.UnscaledTime; // NIGHT63 D1: the shot frame's time in a same-run held frame
            if (pickupIds.Count > 0)
            {
                Campaign24 c = game.sim != null ? game.sim.campaign : null;
                for (int i = 0; i < pickupIds.Count; i++)
                {
                    bool show = c == null || !c.IsPickupTaken(pickupIds[i]); // a new run (a fresh campaign) shows it again
                    if (pickupBoxes[i] != null && pickupBoxes[i].activeSelf != show) pickupBoxes[i].SetActive(show);
                    if (pickupGlows[i] != null && pickupGlows[i].activeSelf != show) pickupGlows[i].SetActive(show);
                    PickupShown31[pickupIds[i]] = show;
                }
            }
            for (int i = 0; i < spinners.Count; i++)
            {
                if (spinners[i] == null) continue;
                spinners[i].rotation = Quaternion.Euler(0, t * UnityTuning.PickupSpinDegPerS, 0);
                Vector3 p = spinners[i].position;
                p.y = spinY[i] + Mathf.Sin(t * UnityTuning.PickupBobHz * 2f * Mathf.PI) * UnityTuning.PickupBob;
                spinners[i].position = p;
            }
            if (hazardMats.Count == 0 || game.sim == null) return;
            int state = game.sim.hazard27State, room = game.sim.hazard27Room;
            if (state != lastState) { lastState = state; if (state == 1) teleStart = t; }
            double tele = game.cfg.Num("design.n27.hazardFloor27.telegraphSeconds");
            bool reduced = game.cfg.movement.settings.reducedMotion;
            for (int i = 0; i < hazardMats.Count; i++)
            {
                bool mine = hazardRoom[i] == room;
                float b = PropsView29Math.HazardBrightness(mine ? state : 0, t - teleStart, tele, reduced);
                hazardMats[i].mat.SetColor("_BaseColor", hazardMats[i].color * b);
            }
        }
    }
}
