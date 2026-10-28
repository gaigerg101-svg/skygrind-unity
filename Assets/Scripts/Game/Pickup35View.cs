// NIGHT35 builder W (design.n35.pickup35, a VIEW row; PRINCIPLES 3 "a pickup shows what it is"; audit.md items 2 and 4; DECIDE35 D-5): what a campaign
// pickup looks like. PropsView29 builds one of these per weaponPickup / healthPickup row while N35Fixes.ViewRowOn(cfg, "pickup35"); off (the row off or
// -skygrindOff35) it draws NIGHT34's spinning cube. View only: the sim's Campaign24.StepPickups takes the pickup as before.
//   - WEAPON: the weapon's own kit model (BlockyKit.ForWeapon, the mesh SURVIVAL's wall buys and the viewmodel use) in its body tones, fit to
//     Pickup35GunLength, turning and bobbing, with a cyan rim (an inverted hull, additive) and its name ("THUNDERCLAP") over it within
//     Pickup35NameNear m (fading in over the last Pickup35NameFade m, always facing the camera).
//   - LOCKED (the pickup's room is ahead of the room the player is in: Campaign24.room < the room's index): the same model as a near-black silhouette with
//     a dim outline (CoD Zombies' chalk outline: the shape is shown before it is earned); it lights up when the player reaches its room. No name.
//   - HEALTH: a cyan vial (a glass tube, a bright core that pulses at Pickup35VialPulseHz, bone caps, a small cyan plus on its faces): never a red cross
//     (a protected emblem: research/teach.md section 2), and "MEND +N" over it on approach (N = design.n29.pickups.healFraction x the player's max health).
// Numbers: UnityTuning.Weapons35.cs. Unity objects are made in Build (from PropsView29.Build), never in a field initialiser (L-035).
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Pickup35View : MonoBehaviour // NIGHT52 look52: partial (Pickup35View.Look52.cs, the health plus)
    {
        SurvivalGame game;
        Transform model, label;
        TextMeshPro text;
        Material bodyMat, rimMat, lockedMat, lockedRimMat, coreMat;
        Renderer bodyR, rimR;
        Color cyan, coreColor;
        float baseY, spin;
        bool health;
        int roomIndex = -1;
        string nameText, shownText;
        bool? shownLocked;
        float shownAlpha = -1;

        /// <summary>For the reels (pickup35): every pickup35 view built this map, and the last one's state.</summary>
        public static readonly System.Collections.Generic.List<Pickup35View> All = new System.Collections.Generic.List<Pickup35View>();
        public string Kind => health ? "healthPickup" : "weaponPickup";
        public string WeaponId { get; private set; }
        public bool LockedNow => shownLocked == true;
        public float LabelAlpha => shownAlpha;
        public string LabelText => shownText ?? "";
        public Vector3 Centre => model != null ? model.position : transform.position;

        /// <summary>For the reels: the label's drawn height on screen (px; 0 hidden): its text bounds' top and bottom through the camera.</summary>
        public float LabelPixelHeight(Camera cam)
        {
            if (cam == null || text == null || !label.gameObject.activeInHierarchy) return 0;
            text.ForceMeshUpdate();
            Bounds b = text.textBounds;
            Vector3 top = label.TransformPoint(new Vector3(b.center.x, b.max.y, 0)), bot = label.TransformPoint(new Vector3(b.center.x, b.min.y, 0));
            return Mathf.Abs(cam.WorldToScreenPoint(top).y - cam.WorldToScreenPoint(bot).y);
        }

        /// <summary>A plan's weapon word (a loadout id or the weapon's name: "thunderclap") as the loadout id (Campaign24.WeaponId's rule); null when unknown.</summary>
        public static string WeaponIdOf(GameConfig cfg, string word)
        {
            if (string.IsNullOrEmpty(word)) return null;
            if (cfg.Has("weapons.names." + word)) return word;
            foreach (var kv in cfg.Obj("weapons.names"))
                if (kv.Value is string nm && string.Equals(nm, word, StringComparison.OrdinalIgnoreCase)) return kv.Key;
            return null;
        }

        /// <summary>A pickup's view at `at` (Unity space, the plan's centre). `weaponWord` null for health; `room` the row's room id (null: never locked).</summary>
        public static Pickup35View Build(SurvivalGame g, Transform parent, SurvivalMaterials mats, string kind, string id, string weaponWord, string room, LevelDef level, Vector3 at)
        {
            var go = new GameObject("pickup35 " + kind + " " + id);
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            var v = go.AddComponent<Pickup35View>();
            v.Make(g, mats, kind, weaponWord, room, level, at);
            All.Add(v);
            return v;
        }

        void OnDestroy() => All.Remove(this);

        void Make(SurvivalGame g, SurvivalMaterials mats, string kind, string weaponWord, string room, LevelDef level, Vector3 at)
        {
            game = g;
            GameConfig cfg = g.cfg;
            cyan = Look.C(cfg, "colors.usable");
            health = kind == "healthPickup";
            baseY = at.y;
            spin = UnityTuning.PickupSpinDegPerS;
            if (level != null && !string.IsNullOrEmpty(room))
                for (int i = 0; i < level.rooms.Count; i++)
                    if (level.rooms[i].id == room) { roomIndex = i; break; }
            var holder = new GameObject("model").transform;
            holder.SetParent(transform, false);
            model = holder;
            if (health && Look52.On(cfg)) MakeCross52(mats, cfg); // NIGHT52 U look52 (Pickup35View.Look52.cs): the health plus that reads at 15 m
            else if (health) MakeVial(mats, cfg);
            else MakeGun(mats, cfg, weaponWord);
            // the name over it (TextMeshPro in the world, as WallBuys' signs)
            var lg = new GameObject("pickup35 name", typeof(TextMeshPro));
            lg.transform.SetParent(transform, false);
            lg.transform.localPosition = new Vector3(0, UnityTuning.Pickup35NameH, 0);
            text = lg.GetComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = UnityTuning.Pickup35NameSize;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.fontStyle = FontStyles.Bold; // upright (n35-r2: the italic label read as a slanted banner)
            text.rectTransform.sizeDelta = new Vector2(4f, 0.6f);
            text.color = new Color(cyan.r, cyan.g, cyan.b, 0);
            label = lg.transform;
            lg.SetActive(false);
            // n35-r2: NIGHT29's glow pillar crossed the model and the label: under pickup35 a cyan ring on the floor marks the spot instead
            var ring = new MeshKit();
            float ry = -UnityTuning.PickupFloat + 0.02f, r0 = UnityTuning.Pickup35RingR - UnityTuning.Pickup35RingW, r1 = UnityTuning.Pickup35RingR;
            const int seg = 32;
            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2 / seg, a1 = (i + 1) * Mathf.PI * 2 / seg;
                Vector3 i0 = new Vector3(Mathf.Cos(a0) * r0, ry, Mathf.Sin(a0) * r0), i1 = new Vector3(Mathf.Cos(a1) * r0, ry, Mathf.Sin(a1) * r0);
                Vector3 o0 = new Vector3(Mathf.Cos(a0) * r1, ry, Mathf.Sin(a0) * r1), o1 = new Vector3(Mathf.Cos(a1) * r1, ry, Mathf.Sin(a1) * r1);
                ring.Quad(i0, i1, o1, o0, Vector3.up, Color.white);
                ring.Quad(o0, o1, i1, i0, Vector3.down, Color.white); // both faces (the winding is not checked here)
            }
            var ringMat = new Material(mats.neon) { name = "pickup35 ring" };
            ringMat.SetColor("_BaseColor", cyan * UnityTuning.Pickup35RingGain);
            Part(transform, "ring", ring.Build("pickup35 ring"), ringMat);
            // its own small light, so the gun's body tones read in a dark room (n35-r2: the gun was dark, only its rim showed)
            var lightGo = new GameObject("pickup35 light");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0, UnityTuning.Pickup35LightUp, 0);
            var li = lightGo.AddComponent<Light>();
            li.type = LightType.Point;
            li.range = UnityTuning.Pickup35LightRange;
            li.intensity = UnityTuning.Pickup35LightIntensity;
            li.color = Color.Lerp(Color.white, cyan, 0.35f);
            li.shadows = LightShadows.None;
        }

        void MakeGun(SurvivalMaterials mats, GameConfig cfg, string weaponWord)
        {
            string wid = WeaponIdOf(cfg, weaponWord) ?? "revolver";
            WeaponId = wid;
            nameText = cfg.Has("weapons.names." + wid) ? ((string)cfg.Get("weapons.names." + wid)).ToUpperInvariant() : wid.ToUpperInvariant();
            uint familyHex = WeaponLook.FamilyHex(cfg, wid) ?? (uint)cfg.Num("colors.usable");
            Color family = Look.Hex(familyHex);
            var kit = new MeshKit();
            if (!BlockyKit.ForWeapon(wid, kit, family))
            {
                kit.Clear();
                if (wid == "gauntlet") BlockyKit.Gauntlet(kit, kit, family); // the PULSE GAUNTLET's body + its strip (ForWeapon has no gauntlet case)
                else BlockyKit.ForWeapon("revolver", kit, family);
            }
            Mesh mesh = kit.Build("pickup35 " + wid);
            Bounds b = mesh.bounds;
            float longest = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            float scale = longest > 1e-4f ? UnityTuning.Pickup35GunLength / longest : 1f;
            var body = new GameObject("gun");
            body.transform.SetParent(model, false);
            body.transform.localScale = Vector3.one * scale;
            body.transform.localPosition = -b.center * scale; // the model turns about its own centre
            body.AddComponent<MeshFilter>().sharedMesh = mesh;
            bodyR = body.AddComponent<MeshRenderer>();
            // the gun's body tones: the viewmodel / wall buy kit material (the stone shader's vertex colours, no grid)
            bodyMat = new Material(mats.stone) { name = "pickup35 gun" };
            bodyMat.SetFloat("_GridMinor", 0);
            bodyMat.SetFloat("_GridMajor", 0);
            lockedMat = new Material(mats.neon) { name = "pickup35 silhouette" };
            lockedMat.SetColor("_BaseColor", new Color(UnityTuning.Pickup35LockedBody, UnityTuning.Pickup35LockedBody, UnityTuning.Pickup35LockedBody, 1));
            bodyR.sharedMaterial = bodyMat;
            bodyR.shadowCastingMode = ShadowCastingMode.Off;
            // the rim: the same mesh, back faces only, additive, a little bigger (Viewmodel.Feel's style aura shell)
            var rim = new GameObject("rim");
            rim.transform.SetParent(body.transform, false);
            rim.AddComponent<MeshFilter>().sharedMesh = mesh;
            rimR = rim.AddComponent<MeshRenderer>();
            rimMat = FxDraw.Additive(mats.neon, "pickup35 rim");
            rimMat.SetFloat("_Cull", (float)CullMode.Front);
            rimMat.SetColor("_BaseColor", cyan * UnityTuning.Pickup35RimGain);
            lockedRimMat = FxDraw.Additive(mats.neon, "pickup35 locked rim");
            lockedRimMat.SetFloat("_Cull", (float)CullMode.Front);
            Color pale = Color.Lerp(cyan, Color.white, 0.6f);
            lockedRimMat.SetColor("_BaseColor", pale * UnityTuning.Pickup35LockedRimGain);
            rimR.sharedMaterial = rimMat;
            rimR.shadowCastingMode = ShadowCastingMode.Off;
            float rimM = UnityTuning.Pickup35RimM / scale; // in the mesh's own units
            var s = new Vector3(1 + 2 * rimM / Mathf.Max(b.size.x, 1e-3f), 1 + 2 * rimM / Mathf.Max(b.size.y, 1e-3f), 1 + 2 * rimM / Mathf.Max(b.size.z, 1e-3f));
            rim.transform.localScale = s;
            rim.transform.localPosition = b.center - Vector3.Scale(b.center, s);
        }

        void MakeVial(SurvivalMaterials mats, GameConfig cfg)
        {
            WeaponId = null;
            float r = UnityTuning.Pickup35VialR, h = UnityTuning.Pickup35VialH;
            // the parts are built along z, then the holder's child is stood up (z -> y)
            var stand = new GameObject("vial").transform;
            stand.SetParent(model, false);
            stand.localRotation = Quaternion.Euler(-90, 0, 0);
            var glass = new MeshKit();
            BlockyKit.TubeZ(glass, r, h, 0, 0, 0, 12, Color.white);
            var core = new MeshKit();
            BlockyKit.TubeZ(core, r * 0.62f, h * 0.8f, 0, 0, 0, 12, Color.white);
            // a plus on two faces (a non-red plus: never the protected red cross)
            float pw = r * 0.95f, pt = r * 0.28f, off = r * 1.02f;
            foreach (float side in new[] { 1f, -1f })
            {
                core.Cube(new Vector3(0, side * off, 0), new Vector3(pw, 0.01f, pt), Color.white);
                core.Cube(new Vector3(0, side * off, 0), new Vector3(pt, 0.01f, pw), Color.white);
            }
            var caps = new MeshKit();
            BlockyKit.TubeZ(caps, r * 1.12f, h * 0.12f, 0, 0, h * 0.5f, 12, BlockyKit.Bone);
            BlockyKit.TubeZ(caps, r * 1.12f, h * 0.12f, 0, 0, -h * 0.5f, 12, BlockyKit.Bone);
            var glassMat = FxDraw.Additive(mats.neon, "pickup35 vial glass");
            glassMat.SetColor("_BaseColor", cyan * 0.35f);
            coreMat = new Material(mats.neon) { name = "pickup35 vial core" };
            coreColor = cyan * UnityTuning.Pickup35VialGain;
            coreMat.SetColor("_BaseColor", coreColor);
            var capMat = new Material(mats.stone) { name = "pickup35 vial caps" };
            capMat.SetFloat("_GridMinor", 0);
            capMat.SetFloat("_GridMajor", 0);
            Part(stand, "core", core.Build("pickup35 vial core"), coreMat);
            Part(stand, "glass", glass.Build("pickup35 vial glass"), glassMat);
            Part(stand, "caps", caps.Build("pickup35 vial caps"), capMat);
            // the pickup's own heal at the config's max health (healFraction x health.max: 0.4 x 100 = 40); never the live max (n35-r2: the reel's god
            // mode read "MEND +400000")
            double heal = cfg.Has("design.n29.pickups.healFraction") ? cfg.Num("design.n29.pickups.healFraction") : 0;
            nameText = "MEND +" + Math.Round(heal * cfg.Num("health.max")).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        static void Part(Transform parent, string name, Mesh mesh, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        void Update()
        {
            if (game == null || model == null) return;
            float t = FeelClock.UnscaledTime; // NIGHT63 D1: the shot frame's time in a same-run held frame
            model.rotation = cross52 ? Cross52Turn(t) : Quaternion.Euler(0, t * spin, 0); // NIGHT52 look52: the plus faces the camera, rocking
            Vector3 p = model.position;
            p.y = baseY + Mathf.Sin(t * UnityTuning.PickupBobHz * 2f * Mathf.PI) * UnityTuning.PickupBob;
            model.position = p;
            // locked: the pickup's room is ahead of the player's
            Campaign24 c = game.sim != null ? game.sim.campaign : null;
            bool locked = !health && roomIndex >= 0 && c != null && c.room >= 0 && c.room < roomIndex;
            if (shownLocked != locked && bodyR != null)
            {
                shownLocked = locked;
                bodyR.sharedMaterial = locked ? lockedMat : bodyMat;
                rimR.sharedMaterial = locked ? lockedRimMat : rimMat;
            }
            if (health)
            {
                shownLocked = false;
                float pulse = 1f - UnityTuning.Pickup35VialPulse * (0.5f + 0.5f * Mathf.Cos(t * UnityTuning.Pickup35VialPulseHz * 2f * Mathf.PI));
                coreMat.SetColor("_BaseColor", coreColor * pulse);
            }
            // the name on approach (never on a locked silhouette)
            float alpha = 0f, d = 0f;
            Camera cam = game.cam;
            if (!locked && cam != null && !cross52) // NIGHT52 look52: the plus has no label (its shape says it)
            {
                d = Vector3.Distance(cam.transform.position, model.position);
                alpha = Mathf.Clamp01((UnityTuning.Pickup35NameNear - d) / UnityTuning.Pickup35NameFade);
                alpha = Mathf.Round(alpha * 20f) / 20f;
            }
            bool on = alpha > 0f;
            if (label.gameObject.activeSelf != on) label.gameObject.SetActive(on);
            if (on)
            {
                if (shownText != nameText) { shownText = nameText; text.text = nameText; }
                label.rotation = Quaternion.LookRotation(label.position - cam.transform.position);
                float k = Mathf.Max(1f, d / UnityTuning.Pickup35NameScaleFrom); // about 17 px tall from 5 m out to 12 m (the lead's 16 px floor)
                k = Mathf.Round(k * 20f) / 20f;
                if (label.localScale.x != k) label.localScale = Vector3.one * k;
            }
            if (alpha != shownAlpha)
            {
                shownAlpha = alpha;
                text.color = new Color(cyan.r, cyan.g, cyan.b, alpha);
            }
        }
    }
}
