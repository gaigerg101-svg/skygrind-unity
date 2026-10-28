// NIGHT48 builder F: the a848 phase (F's look reel, as a847 was), round 3: G14's IN-GAME read at 20 m (the lead's n48-r2 ask: "one in-game 20 m
// frame of a swarmer: a real arena, the player's normal camera, not the lineup; say its size in pixels"). Run:
//   reel.sh + -skygrindMap <a campaign level, e.g. n47-iceline> + -skygrindReelOnly a848
// The CAMPAIGN on the booted map, DOORFREE, God, every enemy frozen, the player's own input, standing still: the HUD and the viewmodel ON,
// the game's own camera and field of view. A SWARMER stood on the player's floor 20 m and then 10 m ahead (the clearest heading with floor
// there and a clear line), facing the player, the camera looking at its middle; OFF (visor47 / foes47 / foes48 forced off) then ON; and a
// HUSK at 20 m on the ON pass for scale. Per stand: the frame with the body (shot-N30-F48-<kind>-<off|on>-<d>m.png, full 1080p frame) and the
// same frame with the body taken away (alive false: the view hides it), and their difference: the pixels that changed by more than
// Lineup48DiffL in Rec. 709 luminance (the body, its visor's glow and its bloom), their bounding box (w x h px, at 1080p) and their count.
// A 1:1 crop round it (shot-N30-F48-<kind>-<off|on>-<d>m-crop.png, 160 px at 1080p: the true pixels, not magnified).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N48Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Look48Scenario(g, dir, res, offs, pre);

        /// <summary>A body of `kind` on the player's floor `d` m ahead along the clearest heading with floor there and a clear line (PlaceAt's way).</summary>
        static Husk PlaceKind48(SurvivalGame g, string kind, double d, out double yawUsed)
        {
            Vec3 eye = g.Eye;
            double floorY = g.Feet.y;
            foreach (double yaw in Yaws(g))
            {
                double fx = -Math.Sin(yaw), fz = -Math.Cos(yaw);
                double x = eye.x + fx * d, z = eye.z + fz * d;
                if (!FloorAt(g, x, z, floorY)) continue;
                var aim = new Vec3(x, floorY + 0.4, z);
                Vec3 to = aim - eye;
                double len = to.Length;
                if (g.world.RayIntersect(eye, to / len, out RayHit wall) && wall.distance < len) continue;
                Husk h = null;
                try { h = g.husks.Spawn(kind, new Vec3(x, floorY, z), eye); } catch (Exception) { }
                if (h == null) continue;
                yawUsed = yaw;
                return h;
            }
            yawUsed = 0;
            return null;
        }

        /// <summary>URP's chromatic aberration intensity now (the camera's Volume stack, else the global one; W's Wick48Chroma's read); -1 when none.</summary>
        static float Chroma48(SurvivalGame g)
        {
            UnityEngine.Rendering.VolumeStack vs = null;
            if (g.cam != null) vs = UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(g.cam).volumeStack;
            vs ??= UnityEngine.Rendering.VolumeManager.instance.stack;
            var ca = vs != null ? vs.GetComponent<UnityEngine.Rendering.Universal.ChromaticAberration>() : null;
            return ca != null ? ca.intensity.value : -1f;
        }

        static IEnumerator Look48Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            bool? vWas = Visor47.Force, f47Was = Foes47.Force, f48Was = Foes48.Force;
            var placed = new List<Husk>();
            offs.Add(() =>
            {
                Visor47.Force = vWas;
                Foes47.Force = f47Was;
                Foes48.Force = f48Was;
                foreach (Husk h in placed) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
            });
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED a848: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 200;
            while ((sim.campaign == null || sim.campaign.room < 0 || sim.campaign.phase == Campaign24.Phase.None) && Time.realtimeSinceStartup < wallEnd - 180)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            for (int i = 0; i < 20; i++) { God(g); yield return null; }
            string pc = Preconditions(g, g.Feet);
            Start(g, "a848-look48", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string> { $"level {sim.campaignLevel.id}, frame {Screen.width}x{Screen.height}, fov {g.cam.fieldOfView.ToString("0.0", ci)}, the HUD and the viewmodel as in play" };
            Color32[][] withF = new Color32[1][], withoutF = new Color32[1][];
            int[] size = new int[2];
            float k1080 = 1080f / Mathf.Max(1, Screen.height);
            var stands = new List<(string kind, bool on, double d)> { ("swarmer", false, 20), ("swarmer", true, 20), ("swarmer", false, 10), ("swarmer", true, 10), ("husk", true, 20) };
            foreach (var (kind, on, d) in stands)
            {
                if (Time.realtimeSinceStartup > wallEnd) break;
                Visor47.Force = Foes47.Force = Foes48.Force = on;
                Husk h = PlaceKind48(g, kind, d, out double yaw);
                if (h == null) { lines.Add($"{kind} {(on ? "ON" : "off")} {d.ToString("0", ci)} m: no floor / clear line found"); continue; }
                placed.Add(h);
                h.hp = h.maxHp = 1e9;
                h.yaw = Math.Atan2(h.position.x - g.Feet.x, h.position.z - g.Feet.z); // facing the player (yaw 0 looks toward -z)
                h.previous = h.position;
                var mid = new Vec3(h.position.x, h.position.y + Math.Max(0.35, h.Height * 0.5), h.position.z);
                for (int i = 0; i < UnityTuning.Lineup48SettleFrames + 6; i++) { God(g); Look(g, mid); yield return null; }
                string tag = $"F48-{kind}-{(on ? "on" : "off")}-{d.ToString("0", ci)}m";
                // round 4 (the lead's n48-r3 read: "drawn 1918 x 1051 px" was every changed pixel of the screen, 47212 of them): the difference is
                // taken only inside the body's projected box (Sil48: its centre, radius and height through the camera) x Lineup48DiffBoxPad
                Vector3 sbox = Sil48(g.cam, h, kind);
                yield return Grab48(dir, withF, size, tag);
                h.alive = false; // the same view without the body
                for (int i = 0; i < 4; i++) { God(g); Look(g, mid); yield return null; }
                yield return Grab48(dir, withoutF, size, null);
                if (withF[0] == null || withoutF[0] == null) { lines.Add($"{tag}: no frame"); continue; }
                int w = size[0], hgt = size[1], n = 0, x0 = w, y0 = hgt, x1 = -1, y1 = -1;
                Color32[] a = withF[0], b = withoutF[0];
                float halfBox = sbox.z / UnityTuning.Lineup48ZoomPad * 0.5f * UnityTuning.Lineup48DiffBoxPad + 4f;
                int bx0 = Mathf.Max(0, (int)(sbox.x - halfBox)), bx1 = Mathf.Min(w - 1, (int)(sbox.x + halfBox));
                int by0 = Mathf.Max(0, (int)(sbox.y - halfBox)), by1 = Mathf.Min(hgt - 1, (int)(sbox.y + halfBox));
                for (int y = by0; y <= by1; y++)
                    for (int x = bx0; x <= bx1; x++)
                    {
                        Color32 p = a[y * w + x], q = b[y * w + x];
                        float dl = Mathf.Abs((0.2126f * (p.r - q.r) + 0.7152f * (p.g - q.g) + 0.0722f * (p.b - q.b)) / 255f);
                        if (dl <= UnityTuning.Lineup48DiffL) continue;
                        n++;
                        if (x < x0) x0 = x;
                        if (x > x1) x1 = x;
                        if (y < y0) y0 = y;
                        if (y > y1) y1 = y;
                    }
                if (n == 0) { lines.Add($"{tag}: the body changed no pixel (hidden by the view or off screen)"); continue; }
                float bw = (x1 - x0 + 1) * k1080, bh = (y1 - y0 + 1) * k1080;
                Crop48(dir, tag + "-crop", a, w, hgt, (x0 + x1) * 0.5f, (y0 + y1) * 0.5f, Mathf.RoundToInt(160 / k1080));
                lines.Add($"{tag}: drawn {bw.ToString("0", ci)} x {bh.ToString("0", ci)} px (w x h at 1080p), {(n * k1080 * k1080).ToString("0", ci)} px changed inside its projected box ({(bx1 - bx0 + 1)} x {(by1 - by0 + 1)} px searched), at ({((x0 + x1) * 0.5f).ToString("0", ci)}, {((y0 + y1) * 0.5f).ToString("0", ci)})");
            }
            // round 3 (W's 1413372e found the post.js unit mix-up; mine is the FUSION pop's Fusion43Chroma 1.4, ~46 s at a full split): a FUSION at a
            // husk 3 m ahead (a synthetic Fusion43Event, the a843 / a844 reels' way: the screen's pop and its chroma kick are the view's), URP's
            // chromatic aberration read every frame: before, its peak, 3 frames after the pop, 0.6 s on; frames shot-N30-F48-fusion-hit.png (3 frames
            // on) and shot-N30-F48-fusion-0p6s.png (0.6 s on)
            float chromaMax = -1, chroma0 = -1, chroma3 = -1, chroma06 = -1;
            if (Time.realtimeSinceStartup < wallEnd)
            {
                Visor47.Force = Foes47.Force = Foes48.Force = null;
                Husk bag = PlaceKind48(g, "husk", 3, out _);
                if (bag != null)
                {
                    placed.Add(bag);
                    bag.hp = bag.maxHp = 1e9;
                    for (int i = 0; i < 150; i++) { God(g); Look(g, bag.AimPoint); yield return null; } // 2.5 s: any earlier kick decays
                    chroma0 = Chroma48(g);
                    int fusions = 0;
                    offs.Add(g.events.On<Fusion43Event>(N43Events.Fusion, e => { if (e.owner == "player") fusions++; }));
                    g.events.Emit(N43Events.Fusion, new Fusion43Event { owner = "player", position = bag.AimPoint, dir = new Vec3(0, 0, -1), speed = 16, weapon = "gauntlet", enemy = bag.id, kind = "husk", killed = false, damage = 0 });
                    float t0 = Time.realtimeSinceStartup;
                    int frameN = 0;
                    bool shotHit = false;
                    while (Time.realtimeSinceStartup - t0 < 0.6f + 0.02f)
                    {
                        God(g);
                        Look(g, bag.AimPoint);
                        yield return null;
                        frameN++;
                        float c = Chroma48(g);
                        if (c > chromaMax) chromaMax = c;
                        if (!shotHit && Time.realtimeSinceStartup - t0 >= (float)UnityTuning.Fusion43ConvergeTime + 0.05f) { shotHit = true; chroma3 = c; yield return Grab48(dir, withF, size, "F48-fusion-hit"); } // ~3 frames after the pop (it comes at the streams' meet, 0.09 s)
                    }
                    chroma06 = Chroma48(g);
                    yield return Grab48(dir, withF, size, "F48-fusion-0p6s");
                    bool ok48 = chromaMax >= 0 && chromaMax <= UnityTuning.Fusion48ChromaMax + 1e-4f && chroma06 <= UnityTuning.Fusion48ChromaMax + 1e-4f;
                    lines.Add($"FUSION (synthetic event at a husk 3 m ahead; heard {fusions}): URP chromatic aberration before {chroma0.ToString("0.00", ci)}, peak {chromaMax.ToString("0.00", ci)}, 3 frames on {chroma3.ToString("0.00", ci)}, 0.6 s on {chroma06.ToString("0.00", ci)} (wants <= {UnityTuning.Fusion48ChromaMax.ToString("0.00", ci)}: {(ok48 ? "GREEN" : "RED")}; Fusion43Chroma 1.4 held 1.00 for ~46 s); frames F48-fusion-hit / F48-fusion-0p6s");
                    bag.alive = false;
                }
                else lines.Add("FUSION: no husk placed 3 m ahead");
            }
            yield return Stop();
            res[0] = $"GREEN a848 look48 (G14 in game) | {string.Join(" || ", lines)} | {pc}";
        }
    }
}
