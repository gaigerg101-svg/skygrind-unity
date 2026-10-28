// NIGHT8 U2-11 / NIGHT9 U3-08, U3-11: the perk machines in Survival (survival/perks.js load / update / apply: Perks.Ported, ten of 14; NIGHT8's starter five: VIGOR, MEND,
// PLATE, QUICK, GRIP at their map spots, data.perkSpots). Each is a buyable (hold E, perks.reach, the stack-taxed price,
// once a run, only while its zone is open); a buy applies the effects (perks.js apply): mover.bonusPips /
// bonusRideTime / bonusWallJumps, regen.perk, the loadout's fire rate and swap, health.max (PLATE fills its new HP at
// once). MEND's machine stands on the Grinder core and rides its pillar (a core spot).
// The look (assets/machineModels.js, rebuilt simply): a gunmetal cabinet (perks.machineWidth / Height / Depth) on a
// plinth with a bone cap, neon trim in the perk's colour (x perks.glow), a screen at 0.64 of the height on the front
// (+z in the web model) with the perk's 8x8 pixel icon, its name and price, and a floor ring (perks.ringRadius) pulsing
// at perks.pulseRate; dimmed when owned, darker when its zone is locked.
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class PerkMachines
    {
        sealed class Machine
        {
            public PerkSpot spot;
            public int zone;
            public Transform root;
            public Renderer trim, ring;
            public TextMeshPro label;
            public Buyables.Item item;
            public SurvivalSim.Machine entry; // NIGHT13: the sim's machine (its buyable)
            public int shownState = -1; // 0 for sale, 1 owned, 2 locked
            public double shownPrice = -1;
            public Color color;
            public string iconKey; // perks.<id>.icon, or forge.icon
            public bool forge; // NIGHT8 U2-17: THE FORGE's cabinet (forge.js load: the same machine model, named FORGE)
            public string shownWeapon;
        }

        readonly SurvivalGame game;
        readonly GameConfig cfg;
        readonly List<Machine> machines = new List<Machine>();
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly float glow, pulseRate, height;
        double time;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public PerkMachines(SurvivalGame game)
        {
            this.game = game;
            cfg = game.cfg;
            glow = (float)cfg.Num("perks.glow");
            pulseRate = (float)cfg.Num("perks.pulseRate");
            height = (float)cfg.Num("perks.machineHeight");
            var parent = new GameObject("Perk machines").transform;
            // NIGHT13 U3-26.6: the machines, their buyables and perks.apply are the sim's (SurvivalSim.AddMachines, in its order);
            // the look here: a perk machine in its perk's colour, THE FORGE's cabinet (forge.js load: the same model, named FORGE).
            foreach (SurvivalSim.Machine e in game.sim.machines)
            {
                var m = e.forge
                    ? new Machine { entry = e, spot = e.spot, zone = e.zone, item = e.item, color = Look.C(cfg, "forge.color"), iconKey = "forge.icon", forge = true }
                    : new Machine { entry = e, spot = e.spot, zone = e.zone, item = e.item, color = Look.C(cfg, $"perks.{e.spot.perk}.color"), iconKey = $"perks.{e.spot.perk}.icon" };
                Build(m, parent);
                machines.Add(m);
            }
        }

        static Transform Part(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go.transform;
        }

        void Build(Machine m, Transform parent)
        {
            float w = (float)cfg.Num("perks.machineWidth"), h = height, d = (float)cfg.Num("perks.machineDepth");
            var root = new GameObject($"Perk {m.spot.perk}").transform;
            root.SetParent(parent, false);
            root.position = WebSpace.ToUnity(m.spot.pos);
            root.rotation = Quaternion.Euler(0, -(float)(m.spot.yaw * Mathf.Rad2Deg), 0);
            m.root = root;
            var body = new Material(game.mats.lit) { color = new Color(0.16f, 0.17f, 0.21f) }; // the kit's gunmetal
            var bone = new Material(game.mats.lit) { color = new Color(0.86f, 0.83f, 0.76f) }; // the bone cap
            Part(PrimitiveType.Cube, root, new Vector3(0, 0.08f, 0), new Vector3(w + 0.16f, 0.16f, d + 0.16f), body); // plinth
            Part(PrimitiveType.Cube, root, new Vector3(0, h * 0.5f, 0), new Vector3(w, h, d), body);
            Part(PrimitiveType.Cube, root, new Vector3(0, h + 0.05f, 0), new Vector3(w + 0.08f, 0.1f, d + 0.08f), bone);
            // Neon trim: the front's two vertical edges and a top bar, in the perk's colour x glow (one renderer each).
            var neon = new Material(game.mats.neon);
            Color c = m.color * glow;
            c.a = 1;
            neon.SetColor(BaseColor, c);
            Part(PrimitiveType.Cube, root, new Vector3(-w * 0.5f, h * 0.5f, -d * 0.5f), new Vector3(0.05f, h * 0.92f, 0.05f), neon);
            Part(PrimitiveType.Cube, root, new Vector3(w * 0.5f, h * 0.5f, -d * 0.5f), new Vector3(0.05f, h * 0.92f, 0.05f), neon);
            m.trim = Part(PrimitiveType.Cube, root, new Vector3(0, h * 0.95f, -d * 0.5f), new Vector3(w, 0.05f, 0.05f), neon).GetComponent<Renderer>();
            // The screen: the 8x8 pixel icon on a dark panel (web front +z -> Unity -z).
            var screenMat = new Material(game.mats.neon) { mainTexture = IconTexture(cfg.Strs(m.iconKey), m.color) };
            screenMat.SetColor(BaseColor, Color.white);
            screenMat.SetTexture("_BaseMap", screenMat.mainTexture);
            var screen = Part(PrimitiveType.Quad, root, new Vector3(0, h * 0.72f, -d * 0.5f - 0.066f), new Vector3(w * 0.5f, w * 0.5f, 1), screenMat);
            screen.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off; // a Quad's face looks toward -z: the front
            var labelGo = new GameObject("Label", typeof(TextMeshPro));
            labelGo.transform.SetParent(root, false);
            labelGo.transform.localPosition = new Vector3(0, h * 0.44f, -d * 0.5f - 0.07f);
            labelGo.transform.localRotation = Quaternion.Euler(0, 0, 0);
            m.label = labelGo.GetComponent<TextMeshPro>();
            m.label.alignment = TextAlignmentOptions.Center;
            m.label.fontSize = 2.2f;
            m.label.enableWordWrapping = false;
            m.label.rectTransform.sizeDelta = new Vector2(w, 0.6f);
            // The floor ring: a flat disc edge of perks.ringRadius.
            var ringMat = new Material(game.mats.neon);
            m.ring = Part(PrimitiveType.Cylinder, root, new Vector3(0, 0.01f, 0), new Vector3((float)cfg.Num("perks.ringRadius") * 2, 0.005f, (float)cfg.Num("perks.ringRadius") * 2), ringMat).GetComponent<Renderer>();
            m.ring.shadowCastingMode = ShadowCastingMode.Off;
        }

        /// <summary>blockyKit.js drawPixelIcon: '#' pixels in the perk's colour on the screen's dark ground, point-filtered.</summary>
        static Texture2D IconTexture(List<string> rows, Color color)
        {
            int n = rows.Count;
            var tex = new Texture2D(n + 2, n + 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var dark = new Color(0.05f, 0.05f, 0.09f, 1);
            Color lit = color * 1.6f;
            lit.a = 1;
            for (int y = 0; y < n + 2; y++)
            {
                for (int x = 0; x < n + 2; x++)
                {
                    int row = n - y, col = x - 1; // texture rows go up; the icon's first string is the top row
                    bool on = row >= 0 && row < n && col >= 0 && col < rows[row].Length && rows[row][col] == '#';
                    tex.SetPixel(x, y, on ? lit : dark);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>Frame time: the screens' text, the trim and ring by state, MEND riding its pillar.</summary>
        public void Render(double frameDt)
        {
            time += frameDt;
            foreach (Machine m in machines)
            {
                string id = m.spot.perk;
                string weapon = game.loadout.Weapon;
                // THE FORGE shows the gun in hand's state (forge.js update: "<NAME>+" once forged); a perk its own.
                bool owned = m.forge ? game.forge.IsForged(weapon) : game.perks.Owns(id);
                bool locked = m.zone >= 0 && game.zones.LockedTag(m.zone);
                double price = m.forge ? game.forge.p.cost : game.perks.Price(id);
                int state = owned ? 1 : locked ? 2 : 0;
                if (state != m.shownState || price != m.shownPrice || (m.forge && !ReferenceEquals(weapon, m.shownWeapon)))
                {
                    m.shownState = state;
                    m.shownPrice = price;
                    m.shownWeapon = weapon;
                    if (m.forge)
                    {
                        string gun = cfg.Has($"weapons.catalog.{weapon}.name") ? cfg.Str($"weapons.catalog.{weapon}.name") : weapon.ToUpperInvariant();
                        m.label.text = owned ? $"<b>FORGE</b>\n{gun}+" : $"<b>FORGE</b>\n{price:0}";
                        m.item.name = $"FORGE {gun}"; // the buy prompt's name follows the gun in hand (forge.js: get name())
                    }
                    else m.label.text = owned ? $"<b>{id.ToUpperInvariant()}</b>\nOWNED" : $"<b>{id.ToUpperInvariant()}</b>\n{price:0}";
                    m.label.color = locked ? new Color(0.5f, 0.5f, 0.55f) : Color.white;
                }
                float pulse = 0.75f + 0.25f * Mathf.Sin((float)time * pulseRate + (float)m.spot.pos.x);
                Color rc = m.color * (locked ? 0.3f : glow * pulse * (owned && !m.forge ? 0.4f : 1)); // the forge never dims (forge.js)
                rc.a = 1;
                block.SetColor(BaseColor, rc);
                m.ring.SetPropertyBlock(block);
                if (m.spot.core && game.core != null)
                {
                    double? top = game.core.TopAt(m.spot.pos.x, m.spot.pos.z);
                    if (top.HasValue)
                    {
                        m.root.position = WebSpace.ToUnity(new Vec3(m.spot.pos.x, top.Value, m.spot.pos.z));
                        game.sim.RideCore(m.entry); // the buy point rides too (the sim's)
                    }
                }
            }
        }
    }
}
