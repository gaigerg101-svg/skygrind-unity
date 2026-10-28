// NIGHT24 (builder L): the room lighting kit, minimal (design.n24.lighting; Design/levels/LEVEL-FORMAT.md rooms). View only: nothing here
// touches the sim. In a CAMPAIGN level it adds, from the shipped neon material (a copy per accent) and two ordinary URP point lights:
// - emissive trim: four thin strips along each room's floor edge in the room's accent colour (cool and player-side: cyan, mint, blue; never
//   red-orange, which stays on enemies and danger, and never magenta, which is RIVAL's);
// - a room light: one soft point light at the middle of the room you are in, in its accent, so a closed room reads as lit;
// - a guide light: one cyan point light that stands at the exit of the room you are in (the door's side once it is cleared and open) and pulses.
// The two lights move between rooms; no light is created per frame. Created by SurvivalGame.Init for a map with a "campaign" block.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class CampaignLighting24 : MonoBehaviour
    {
        SurvivalGame game;
        LevelDef level;
        Light roomLight, guide;
        /// <summary>NIGHT27 roof27's view (null when the row is off): the lead's reel and ms A/B read it.</summary>
        public RoofView27 roof;
        Color[] accents;
        float roomIntensity, guideIntensity, guideHeight, roomHeight, pulseHz;

        static Color Hex(double v)
        {
            int h = (int)v;
            return new Color(((h >> 16) & 255) / 255f, ((h >> 8) & 255) / 255f, (h & 255) / 255f);
        }

        public static CampaignLighting24 Create(SurvivalGame game, Transform parent, SurvivalMaterials mats)
        {
            if (game.map.json == null || !game.map.json.ContainsKey("campaign")) return null;
            var go = new GameObject("Campaign lighting");
            go.transform.SetParent(parent, false);
            var k = go.AddComponent<CampaignLighting24>();
            k.Build(game, mats);
            return k;
        }

        void Build(SurvivalGame g, SurvivalMaterials mats)
        {
            game = g;
            GameConfig cfg = g.cfg;
            level = LevelDef.From(g.map.json);
            var list = cfg.Arr("design.n24.lighting.accents");
            accents = new Color[list.Count];
            for (int i = 0; i < accents.Length; i++) accents[i] = Hex(PortData.Num(list[i]));
            float tw = (float)cfg.Num("design.n24.lighting.trim.width"), th = (float)cfg.Num("design.n24.lighting.trim.height");
            float inset = (float)cfg.Num("design.n24.lighting.trim.inset"), bright = (float)cfg.Num("design.n24.lighting.trim.brightness");
            var trimMats = new Material[accents.Length];
            for (int i = 0; i < accents.Length; i++)
            {
                trimMats[i] = new Material(mats.neon);
                trimMats[i].SetColor("_BaseColor", accents[i] * bright);
            }
            look33 = N33Fixes.ViewRowOn(cfg, "look33");
            theme41 = Look41.ThemeFor(cfg, g.map.json); // NIGHT41 K2 (look41): the room key / fill colours by theme; before Build33 (Colour33 reads it)
            Sun41(cfg);
            lights35 = N35Fixes.ViewRowOn(cfg, "lights35"); // NIGHT35 V: the key pool + the cool fill (Key35)
            if (look33) plan33 = LightPlan33.GreenFor(g.map.json, cfg, N30Fixes.ViewRowOn(cfg, "look30"), out _);
            if (plan33 != null) Build33(g, mats, trimMats);
            for (int r = 0; r < level.rooms.Count && plan33 == null; r++)
            {
                RoomDef room = level.rooms[r];
                if (room.x1 <= room.x0 || room.z1 <= room.z0) continue;
                Material m = trimMats[r % accents.Length];
                float x0 = (float)room.x0 + inset, x1 = (float)room.x1 - inset, z0 = (float)room.z0 + inset, z1 = (float)room.z1 - inset;
                Strip(m, (x0 + x1) / 2, (z0), x1 - x0, tw, th);
                Strip(m, (x0 + x1) / 2, (z1), x1 - x0, tw, th);
                Strip(m, x0, (z0 + z1) / 2, tw, z1 - z0, th);
                Strip(m, x1, (z0 + z1) / 2, tw, z1 - z0, th);
            }
            WallRideStrips26.Build(g, transform, mats); // NIGHT26 (builder M): cyan strips along the long rideable walls (design.n26.wallRide)
            RideTrim36.Build(g, transform, mats); // NIGHT36 (builder M): the cyan trim on the level's wall-ride panels (with design.n36.wallgrip36)
            Look43View.Create(g, transform, mats); // NIGHT43 F1 (design.n43.look43): doors, lava underglow, ride strips, landmarks, the stone's saturation (null with the row off)
            roof = RoofView27.Create(g, transform, mats); // NIGHT27 (builder V): the ceiling, AO, strips, gradient ambient and fog (design.n27.roof27; null when off)
            LightKit29 kit29 = LightKit29.Create(g, transform, mats); // NIGHT29 (builder V): the plan's light sockets and door frames (design.n29.lights / doors; null when both are off)
            SkateFx27.Create(g, transform); // NIGHT27 (builder V): the ice skate feel (design.n27.skate27; reads the level's ice zones)
            Ice41View.Create(g, transform, mats); // NIGHT41 K2 (look41): level 2's frost and ice sheen (view props; nothing unless look41 is on and the theme is iceline)
            roomIntensity = (float)cfg.Num("design.n24.lighting.roomLight.intensity") * UnityTuning.LightScale;
            roomHeight = (float)cfg.Num("design.n24.lighting.roomLight.height");
            guideIntensity = (float)cfg.Num("design.n24.lighting.guide.intensity") * UnityTuning.LightScale;
            guideHeight = (float)cfg.Num("design.n24.lighting.guide.height");
            pulseHz = (float)cfg.Num("design.n24.lighting.guide.pulseHz");
            roomLight = MakeLight("Room light", (float)cfg.Num("design.n24.lighting.roomLight.range"), accents[0]);
            guide = MakeLight("Guide light", (float)cfg.Num("design.n24.lighting.guide.range"), Look.C(cfg, "colors.usable"));
            if (plan33 != null) vfx33 = Vfx33.Create(g, transform, plan33, kit29, Danger33); // NIGHT33 L round 3: embers, dust, sparks, the danger flicker
        }

        Light MakeLight(string name, float range, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.shadows = LightShadows.None;
            l.range = range;
            l.color = color;
            l.intensity = 0;
            return l;
        }

        void Strip(Material m, float cx, float cz, float sx, float sz, float sy)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
            s.name = "Trim";
            Destroy(s.GetComponent<Collider>());
            s.transform.SetParent(transform, false);
            s.transform.position = new Vector3(cx, sy * 0.5f, cz); // the map builder uses the web coordinates as they are
            s.transform.localScale = new Vector3(sx, sy, sz);
            s.GetComponent<MeshRenderer>().sharedMaterial = m;
        }

        // ==================== NIGHT33 L (design.n33.look33): every light on a fixture (LightPlan33.Green; DESIGN33 L round 1) ====================
        bool look33;
        int theme41 = -1; // NIGHT41 K2 (design.n41.look41): the level's theme for the key / fill colours (UnityTuning.Look41.cs); -1 = NIGHT40's colours

        // NIGHT41 K2 round 2b (the lead's n41-r2 frames: the floor did not move with the room keys): the near floor is lit by the dusk sun (Look.Apply's
        // directional key, colors.lightSun), so look41 tints the sun by theme while this campaign level stands (UnityTuning.Look41SunHex; 0 = unchanged).
        // Round 2c (the lead's leak check): this view is built BEFORE Look.Apply makes the sun (SurvivalGame.Init: CampaignLighting24.Create, then every
        // directional light destroyed, then Look.Apply), so the tint is written in Update (Sun41Tick), never at build. A boss room (RoomDef.kind "boss")
        // keeps the config's warm sun: its red-orange tells are additive over the floor (Brawl40's slam decal at 0.9 of 0xff3a14) and were tuned on a
        // warm floor; over a blue-grey floor they would drift toward pink. Unload paths: every map change, the menu and survival reload the scene
        // (SceneManager.LoadScene; Look.Apply makes a fresh config sun), a respawn keeps the level (the tint stays right); OnDestroy puts the config
        // colour back if ours is still on the sun.
        Light sun41;
        Color sunSet41, sunConfig41;
        uint sunHex41;

        void Sun41(GameConfig cfg)
        {
            sunHex41 = Look41.SunHex(theme41);
            if (sunHex41 == 0) return;
            sunSet41 = Look.Hex(sunHex41);
            sunConfig41 = Look.C(cfg, "colors.lightSun");
        }

        void Sun41Tick(RoomDef room)
        {
            if (sunHex41 == 0) return;
            Light s = RenderSettings.sun;
            if (s == null) return;
            Color want = room != null && room.kind == "boss" ? sunConfig41 : sunSet41;
            sun41 = s;
            if (s.color != want) s.color = want;
        }

        void OnDestroy()
        {
            if (sunHex41 != 0 && sun41 != null && sun41.color == sunSet41) sun41.color = sunConfig41;
        }
        LightPlan33.Plan plan33;
        readonly System.Collections.Generic.Dictionary<string, Vector3> roomAt33 = new System.Collections.Generic.Dictionary<string, Vector3>(), guideAt33 = new System.Collections.Generic.Dictionary<string, Vector3>();
        /// <summary>For the reel line: fixtures look33 built here (trims, panels, frames, guide lamps).</summary>
        public int trims33, panels33, frames33, lamps33, lenses33, bodies33;
        /// <summary>NIGHT33 L round 3: the effects (null with look33 off).</summary>
        public Vfx33 vfx33;
        /// <summary>The danger (orange) fixture materials: Vfx33 flickers these (danger fixtures only; L1).</summary>
        public readonly System.Collections.Generic.List<Material> Danger33 = new System.Collections.Generic.List<Material>();

        static Vector3 U33(double x, double y, double z) => new Vector3((float)x, (float)y, (float)-z); // web -> Unity (z flips)

        /// <summary>look33: the trims cut at pits and lava, the flush framed ceiling panels (each room's key and the pools' downlights), the guide lamps under the
        /// exit lintels, from the shared plan; and where the room and guide lights stand in each room (Update moves them there, never to mid-air).</summary>
        void Build33(SurvivalGame g, SurvivalMaterials mats, Material[] trimMats)
        {
            GameConfig cfg = g.cfg;
            // round 2: every fixture mesh (trims, panels, frames, guide lamps, and the six L1 types: sconce / strip / pole / brazier bodies and lenses) in one pass
            foreach (LightPlan33.Fix f in plan33.fixes)
            {
                if (f.kind == "trim") trims33++;
                else if (f.kind == "panel" || f.kind == "poolPanel" || (f.kind == "lens" && f.type == "panel")) panels33++;
                else if (f.kind == "frame") frames33++;
                else if (f.kind == "guideLamp") lamps33++;
                else if (f.kind == "lens") lenses33++;
                else if (f.kind == "body") bodies33++;
            }
            Fixtures33.Build(transform, mats, cfg, plan33, f => !(f.kind == "lens" && f.type == "panel" && LightKit29.OverNode33(g, f)), name => Colour33(cfg, name), Danger33); // NIGHT31 G: no bright panel straight over a grapple node
            foreach (LightPlan33.Lit l in plan33.lights)
            {
                if (l.kind == "room") roomAt33[l.room] = U33(l.x, l.y, l.z);
                else if (l.kind == "guide") guideAt33[l.room] = U33(l.x, l.y, l.z);
            }
        }

        /// <summary>A plan colour name as a colour: accentN = the room accents, else LightKit29's names (cyan / green / amber / white / orange).</summary>
        Color Colour33(GameConfig cfg, string name)
        {
            if (name != null && name.StartsWith("accent") && name.Length > 6 && accents.Length > 0) return accents[(name[6] - '0') % accents.Length];
            return LightKit29.ColorNamed(cfg, name ?? "cyan", theme41);
        }

        // ==================== NIGHT35 V (design.n35.lights35; the lead's n35-r1 "lit" frame: mid luminance 0.188 vs 0.193, no pool to see) ====================
        // One warm key + one cool fill per room (look.md section 2): the key a narrower, stronger spot down from its panel with hard shadows (a pool a still frame
        // shows on the floor), the fill a cool blue-violet point light low at the room's far end (from its spawn: the wall you look at). Numbers UnityTuning.Look35.cs.
        bool lights35;
        Light fill35;
        /// <summary>The look35 reel's read of the key (intensity, height, cone, shadows); empty with lights35 off.</summary>
        public static string Key35Now = "";
        string fill35Room;

        void Key35(RoomDef room)
        {
            if (roomLight.spotAngle != UnityTuning.Look35KeySpotAngle)
            {
                roomLight.spotAngle = UnityTuning.Look35KeySpotAngle;
                roomLight.innerSpotAngle = UnityTuning.Look35KeySpotAngle * UnityTuning.Look35KeyInner;
                roomLight.shadows = LightShadows.Hard;
            }
            roomLight.range = roomLight.transform.position.y + UnityTuning.Look35KeyRangeExtra;
            float keyH = Mathf.Max(2f, roomLight.transform.position.y - (float)room.spawn.y); // r4: URP lights fall off as 1 / d^2: at a 9 m ceiling x2.4 of NIGHT24 was ~0.06 on the floor (n35-r3 lit: no pool)
            roomLight.intensity = UnityTuning.Look35KeyFloor * keyH * keyH; // the floor under the key gets the same light whatever the ceiling height
            Key35Now = $"{roomLight.intensity:0.0} at {keyH:0.0} m, cone {roomLight.spotAngle:0} deg, shadows {roomLight.shadows}"; // for the look35 reel
            if (fill35 == null)
            {
                fill35 = MakeLight("Fill light 35", 10f, Look.Hex(Look41.FillHex(theme41))); // NIGHT41 K2: the theme's fill with look41 (else NIGHT35's Look35FillHex)
                fill35.shadows = LightShadows.None;
            }
            if (fill35Room != room.id)
            {
                fill35Room = room.id;
                // the far end from the spawn along its facing, inset; web coordinates as the map builder uses them (z as is: the room rect is web space too)
                double yaw = room.spawnYawDeg * Mathf.Deg2Rad, fx = -System.Math.Sin(yaw), fz = -System.Math.Cos(yaw);
                double cx = (room.x0 + room.x1) * 0.5, cz = (room.z0 + room.z1) * 0.5, hx = (room.x1 - room.x0) * 0.5 - UnityTuning.Look35FillInset, hz = (room.z1 - room.z0) * 0.5 - UnityTuning.Look35FillInset;
                double s = System.Math.Min(System.Math.Abs(fx) > 1e-3 ? hx / System.Math.Abs(fx) : 1e9, System.Math.Abs(fz) > 1e-3 ? hz / System.Math.Abs(fz) : 1e9);
                Vector3 at = WebSpace.ToUnity(new Vec3(cx + fx * s, room.spawn.y + UnityTuning.Look35FillY, cz + fz * s));
                fill35.transform.position = at;
                fill35.range = (float)System.Math.Max(room.x1 - room.x0, room.z1 - room.z0) * UnityTuning.Look35FillRangeShare;
            }
            fill35.intensity = UnityTuning.Look35FillIntensity * UnityTuning.LightScale;
        }

        void Update()
        {
            // NIGHT33 L: the null guard (an Editor NRE every frame after a reload: the view outlived its build, or a level with no rooms)
            if (game == null || game.sim == null || level == null || level.rooms == null || level.rooms.Count == 0 || roomLight == null || guide == null || accents == null || accents.Length == 0) return;
            Campaign24 c = game.sim.campaign;
            int r = c != null && c.room >= 0 ? c.room : 0;
            bool lava47 = Lava47.On(game.cfg);
            if (lava47) r = Room47(r); // NIGHT47 F lava47: the room's lights cross-fade on a room change (out, move, in), never a cut
            RoomDef room = level.rooms[Mathf.Clamp(r, 0, level.rooms.Count - 1)];
            Sun41Tick(room); // NIGHT41 K2 (look41): the themed dusk sun (the config's in a boss room)
            // the room light: the room's middle, in its accent (look33: under the room's flush ceiling panel, never at a fixed height in mid-air)
            roomLight.transform.position = plan33 != null && roomAt33.TryGetValue(room.id, out Vector3 ra) ? ra : new Vector3((float)(room.x0 + room.x1) * 0.5f, roomHeight, (float)(room.z0 + room.z1) * 0.5f);
            roomLight.color = plan33 != null ? Look.Hex(Look41.KeyHex(theme41)) : accents[Mathf.Clamp(r, 0, level.rooms.Count - 1) % accents.Length]; // NIGHT33 L: the key under its warm panel
            roomLight.intensity = roomIntensity;
            if (plan33 != null)
            {
                // NIGHT33 L round 2 (the lead's n33-r1 frames: brighter, not more contrasty): the room's key is a SPOT straight down from its panel, its range the room's
                // height plus a little (a pool of light on the floor, the ceiling round the panel dark), at Look33KeyScale of NIGHT24's intensity
                if (roomLight.type != LightType.Spot) { roomLight.type = LightType.Spot; roomLight.transform.rotation = Quaternion.Euler(90f, 0f, 0f); roomLight.spotAngle = UnityTuning.Look33KeySpotAngle; roomLight.innerSpotAngle = UnityTuning.Look33KeySpotAngle * 0.4f; }
                roomLight.range = roomLight.transform.position.y + UnityTuning.Look33KeyRangeExtra;
                roomLight.intensity = roomIntensity * UnityTuning.Look33KeyScale;
                guide.range = UnityTuning.Look33GuideRange;
                if (lights35) Key35(room);
            }
            // the guide: the room's exit, brighter and pulsing once the room is cleared (the way on is open) (look33: under its lamp on the exit door's lintel)
            bool open = c != null && c.phase == Campaign24.Phase.Cleared;
            guide.transform.position = plan33 != null && guideAt33.TryGetValue(room.id, out Vector3 ga) ? ga : new Vector3((float)room.exit.x, (float)room.exit.y + guideHeight, (float)room.exit.z);
            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * pulseHz * Mathf.PI * 2f);
            guide.intensity = guideIntensity * (open ? 1.6f : 0.6f) * pulse;
            if (lava47)
            {
                roomLight.intensity *= k47;
                guide.intensity *= k47;
                if (fill35 != null) fill35.intensity = UnityTuning.Look35FillIntensity * UnityTuning.LightScale * k47;
            }
        }

        int room47 = -1;
        float k47 = 1f;

        /// <summary>NIGHT47 F lava47: the room the lights are shown for: a new room fades them out (Lava47SlotFadeS), then they move and fade in.</summary>
        int Room47(int r)
        {
            float rate = FeelClock.UnscaledDelta / UnityTuning.Lava47SlotFadeS; // NIGHT63 D1: 0 in a same-run held frame
            if (room47 < 0) { room47 = r; k47 = 1f; }
            if (r != room47)
            {
                k47 -= rate;
                if (k47 <= 0f) { k47 = 0f; room47 = r; }
            }
            else k47 = Mathf.Min(1f, k47 + rate);
            return room47;
        }
    }
}
