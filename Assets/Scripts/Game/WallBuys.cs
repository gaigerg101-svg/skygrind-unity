// ARMORY wall buys (skygrind/src/survival/wallBuys.js), the guns ported so far: each hangs on the hall's long east
// wall (map.armoryWall, in config.weapons.prices order: the THUNDERCLAP is first), a dark plate (wallBuy.plateWidth x
// plateHeight) with a cyan hologram of its kit model turning (holoSpin) and bobbing (holoBob) holoOut m out, and a sign
// (name, price or OWNED, the family in its colour) labelHeight above. Hold E at buyOut m from the wall: the weapon takes
// the next slot and goes in hand.
// NIGHT12 U3-25.10 (port-only: the web's wall shows no finish): a gun wearing a finish (the finish source: weapon id ->
// finish id, the lead passes mastery.FinishFor) hangs as its kit model in that finish's body tones (BlockyKit.Finish,
// Finishes.PaletteOf), lit, with its family trim and gem, in the hologram's place and size; FACTORY keeps the web's cyan
// hologram. The mesh is rebuilt in place only when a gun's finish changes.
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class WallBuys
    {
        // NIGHT11 U3-24.2: everything the frame reads is made once (the Development profile: 36 allocations a frame here,
        // config keys built and the sign's text rebuilt for every gun every frame; NIGHT10's three guns made it 4 guns).
        sealed class Slot
        {
            public string id;
            public Transform holo;
            public TextMeshPro sign;
            public float baseY;
            public string signForSale, signOwned;
            public int shownOwned = -1;
            public Mesh mesh; // the hologram's (or the finished gun's) mesh, rebuilt in place
            public MeshRenderer renderer;
            public Color family;
            public string finishShown = Factory;
            public double price; // NIGHT35 pickup35: the locked silhouette while the price is out of reach
            public GameObject rim; // NIGHT35 pickup35: the silhouette's dim outline (an inverted hull)
            public int shownLocked = -1;
        }

        const string Factory = "factory"; // config.weapons.finishes.order[0], Mastery.FinishFor's default
        readonly SurvivalGame game;
        readonly List<Slot> items = new List<Slot>();
        readonly float holoSpin, holoBob;
        readonly Material holoMat, kitMat;
        // NIGHT35 W (design.n35.pickup35, N35Fixes.ViewRowOn; off or -skygrindOff35: NIGHT34's wall): a FACTORY gun the player cannot afford hangs as a
        // near-black silhouette with a dim outline (CoD Zombies' chalk outline: the shape before it is earned); affordable or owned, the cyan hologram.
        readonly bool pickup35;
        readonly Material lockedMat, lockedRimMat;
        /// <summary>For the reels (pickup35): wall guns drawn as a locked silhouette this frame.</summary>
        public static int Pickup35Locked;
        /// <summary>For the reels: the wall built last (SurvivalGame keeps its own reference private).</summary>
        public static WallBuys Current;
        System.Func<string, string> finishFor;
        float time;

        /// <summary>The finish source (weapon id -> finish id); null shows every gun as the FACTORY hologram.</summary>
        public void SetFinishSource(System.Func<string, string> finishFor) => this.finishFor = finishFor;

        public WallBuys(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            SurvivalMap map = game.map;
            var root = new GameObject("Wall buys").transform;
            var plateMat = new Material(game.mats.lit) { color = new Color(0.06f, 0.06f, 0.09f) };
            Current = this;
            pickup35 = N35Fixes.ViewRowOn(cfg, "pickup35");
            if (pickup35)
            {
                lockedMat = new Material(game.mats.neon) { name = "Wall gun silhouette" };
                lockedMat.SetColor("_BaseColor", new Color(UnityTuning.Pickup35LockedBody, UnityTuning.Pickup35LockedBody, UnityTuning.Pickup35LockedBody, 1));
                lockedRimMat = FxDraw.Additive(game.mats.neon, "Wall gun outline");
                lockedRimMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Front);
                lockedRimMat.SetColor("_BaseColor", Color.Lerp(Look.C(cfg, "colors.usable"), Color.white, 0.6f) * UnityTuning.Pickup35LockedRimGain);
            }
            holoMat = new Material(game.mats.neon);
            holoMat.SetColor("_BaseColor", Look.C(cfg, "colors.usable", 1.6f));
            // a finished gun: the viewmodel's kit material (the stone shader's vertex colours, no grid)
            kitMat = new Material(game.mats.stone) { name = "Wall gun" };
            kitMat.SetFloat("_GridMinor", 0);
            kitMat.SetFloat("_GridMajor", 0);
            double plateW = cfg.Num("wallBuy.plateWidth"), plateH = cfg.Num("wallBuy.plateHeight"), holoOut = cfg.Num("wallBuy.holoOut"), labelH = cfg.Num("wallBuy.labelHeight");
            holoSpin = (float)cfg.Num("wallBuy.holoSpin") * Mathf.Rad2Deg;
            holoBob = (float)cfg.Num("wallBuy.holoBob");
            // NIGHT13 U3-26.6: the guns and their buyables are the sim's (SurvivalSim.AddWallBuys, in wallBuys.js's order); the look here.
            foreach (SurvivalSim.WallGun gun in game.sim.wallGuns)
            {
                string id = gun.id;
                // NIGHT15: each gun's own wall point and facing (the ARMORY wall's guns face along x; a map spot's either way).
                double x = gun.at.x, y = gun.at.y, z = gun.at.z, fx = gun.fx, fz = gun.fz;
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.Destroy(plate.GetComponent<Collider>());
                plate.transform.SetParent(root, false);
                plate.transform.position = WebSpace.ToUnity(new Vec3(x + fx * 0.02, y, z + fz * 0.02));
                plate.transform.localScale = fz == 0 ? new Vector3(0.04f, (float)plateH, (float)plateW) : new Vector3((float)plateW, (float)plateH, 0.04f);
                plate.GetComponent<Renderer>().sharedMaterial = plateMat;
                // The hologram: the kit model, one cyan mesh, scaled up to holoSize
                var kit = new MeshKit();
                BlockyKit.ForWeapon(id, kit, Color.white);
                GameObject holoGo = MeshKit.Object($"{id} hologram", root, kit.Build($"{id} holo"), holoMat, shadows: false);
                var holo = holoGo.transform;
                holo.position = WebSpace.ToUnity(new Vec3(x + fx * holoOut, y, z + fz * holoOut));
                holo.localScale = Vector3.one * (float)(cfg.Num("wallBuy.holoSize") / ModelLength(id)); // the model fit to holoSize
                // The sign
                var go = new GameObject($"{id} sign", typeof(TextMeshPro));
                go.transform.SetParent(root, false);
                go.transform.position = WebSpace.ToUnity(new Vec3(x + fx * holoOut, y + labelH, z + fz * holoOut));
                go.transform.rotation = Quaternion.LookRotation(new Vector3((float)-fx, 0, (float)fz)); // facing out of the wall (web (x, z) = Unity (x, -z): the text reads from the buyer's side)
                var t = go.GetComponent<TextMeshPro>();
                t.alignment = TextAlignmentOptions.Center;
                t.fontSize = 3.2f;
                t.enableWordWrapping = false;
                t.rectTransform.sizeDelta = new Vector2((float)cfg.Num("wallBuy.signWidth"), 0.8f);
                string family = cfg.Str($"weapons.catalog.{id}.family");
                string name = gun.name;
                double price = gun.price;
                uint familyHex = WeaponLook.FamilyHex(cfg, id) ?? 0; // NIGHT13: the one family colour lookup (every wall gun has a family)
                var slot = new Slot
                {
                    id = id, holo = holo, sign = t, baseY = holo.position.y, signForSale = Sign(name, price, false, family, familyHex), signOwned = Sign(name, price, true, family, familyHex),
                    mesh = holoGo.GetComponent<MeshFilter>().sharedMesh, renderer = holoGo.GetComponent<MeshRenderer>(), family = Look.Hex(familyHex),
                };
                slot.price = price;
                if (pickup35)
                {
                    // the outline: the hologram's mesh, back faces only, a little bigger about its centre (Viewmodel.Feel's aura shell)
                    var rim = new GameObject($"{id} outline");
                    rim.transform.SetParent(holo, false);
                    rim.AddComponent<MeshFilter>().sharedMesh = slot.mesh;
                    var rr = rim.AddComponent<MeshRenderer>();
                    rr.sharedMaterial = lockedRimMat;
                    rr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    Bounds bb = slot.mesh.bounds;
                    float rimM = UnityTuning.Pickup35RimM / Mathf.Max(holo.localScale.x, 1e-3f);
                    var sc = new Vector3(1 + 2 * rimM / Mathf.Max(bb.size.x, 1e-3f), 1 + 2 * rimM / Mathf.Max(bb.size.y, 1e-3f), 1 + 2 * rimM / Mathf.Max(bb.size.z, 1e-3f));
                    rim.transform.localScale = sc;
                    rim.transform.localPosition = bb.center - Vector3.Scale(bb.center, sc);
                    rim.SetActive(false);
                    slot.rim = rim;
                }
                items.Add(slot);
                t.text = slot.signForSale;
                slot.shownOwned = 0;
            }
        }

        /// <summary>A kit model's length along its barrel (m), measured on the built mesh, to fit the hologram to holoSize.</summary>
        public static double ModelLength(string id) => id switch
        {
            "nailgun" => 0.483, // STITCHER (NIGHT10)
            "rocket" => 0.770, // COMET
            "rail" => 1.030, // LONGWAVE
            "revolver" => 0.312, // KESTREL
            _ => 0.714, // the THUNDERCLAP (the coach gun; NIGHT7's fit, kept)
        };

        static string Sign(string name, double price, bool owned, string family, uint familyHex) =>
            owned
                ? $"<i><b><color=#8f8a80>{name}</color></b></i>\n<color=#8f8a80>OWNED</color>"
                : $"<i><b>{name}</b></i>\n<color=#7ff0ff>{price:0}</color>\n<size=60%><color=#{familyHex:x6}>{family.ToUpperInvariant()}</color></size>";

        public void Render(double frameDt)
        {
            time += (float)frameDt;
            int locked35 = 0;
            for (int i = 0; i < items.Count; i++)
            {
                Slot s = items[i];
                s.holo.rotation = Quaternion.Euler(0, time * holoSpin, 0);
                Vector3 p = s.holo.position;
                p.y = s.baseY + Mathf.Sin(time * 2) * holoBob;
                s.holo.position = p;
                int owned = game.loadout.owned.Contains(s.id) ? 1 : 0;
                if (owned != s.shownOwned)
                {
                    s.shownOwned = owned;
                    s.sign.text = owned == 1 ? s.signOwned : s.signForSale; // only when it changes (hud.js write)
                }
                if (finishFor != null)
                {
                    string finish = finishFor(s.id) ?? Factory;
                    if (finish != s.finishShown) { Refinish(s, finish); s.shownLocked = -1; } // NIGHT12 U3-25.10: only when it changes
                }
                if (pickup35 && s.finishShown == Factory)
                {
                    int locked = owned == 0 && game.sim != null && game.sim.economy != null && !game.sim.economy.CanAfford(s.price) ? 1 : 0;
                    locked35 += locked;
                    if (locked != s.shownLocked)
                    {
                        s.shownLocked = locked;
                        s.renderer.sharedMaterial = locked == 1 ? lockedMat : holoMat;
                        if (s.rim != null) s.rim.SetActive(locked == 1);
                    }
                }
            }
            Pickup35Locked = locked35;
        }

        /// <summary>NIGHT35 pickup35 (the reels): the wall guns this frame as "id LOCKED|LIT@(x, y, z)" (Unity space). Allocates: reels only.</summary>
        public string Describe35()
        {
            var sb = new System.Text.StringBuilder();
            foreach (Slot s in items)
            {
                if (sb.Length > 0) sb.Append("; ");
                Vector3 p = s.holo.position;
                sb.Append(s.id).Append(s.shownLocked == 1 ? " LOCKED" : " LIT").Append(System.FormattableString.Invariant($"@({p.x:0.0}, {p.y:0.0}, {p.z:0.0})"));
            }
            return sb.ToString();
        }

        /// <summary>For the reels: the first wall gun's hologram position (Unity), false when the map has none.</summary>
        public bool First35(out Vector3 at, out string id)
        {
            at = default;
            id = null;
            if (items.Count == 0) return false;
            at = items[0].holo.position;
            id = items[0].id;
            return true;
        }

        /// <summary>A gun's wall model in its finish: the FACTORY hologram (cyan), or the kit in the finish's body tones (lit).</summary>
        void Refinish(Slot s, string finish)
        {
            s.finishShown = finish;
            Finishes.Palette palette = Finishes.PaletteOf(game.cfg, finish);
            var kit = new MeshKit();
            if (palette == null) BlockyKit.ForWeapon(s.id, kit, Color.white);
            else BlockyKit.ForWeapon(s.id, kit, s.family, palette);
            kit.Build(s.mesh.name, s.mesh);
            s.renderer.sharedMaterial = palette == null ? holoMat : kitMat;
        }
    }
}
