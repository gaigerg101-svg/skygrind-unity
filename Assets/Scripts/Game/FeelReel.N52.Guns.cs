// NIGHT52 builder U: the a352 phase (FeelReel.N52.cs routes it here): guns52's proof (design.n52.guns52, BlockyKit.Guns52.cs, Viewmodel.Guns52.cs).
// Any campaign level:
//   bash Tools/night/n47-reels.sh <tag> u3on,n48-foundry,a352                 (the build copy's config with design.n52.guns52 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> u3off,n48-foundry,a352,-skygrindOff52 (the row off: RED, NIGHT51's guns)
// The CAMPAIGN on the booted map (L-062), DOORFREE, God, enemies frozen, the player's input back and still. For the KESTREL, then the THUNDERCLAP
// (equipped through the combat input's slot): with the row built each moment is a PAIR in the same Player and pose, Guns52Probe.ForceOff then
// Viewmodel.Guns52Rebuild (NIGHT51's plain meshes, the "before"), then guns52's machined meshes (the "after"). Frames:
//   shot-N30-G52-<gun>-idle-<off|on>.png (+ -crop.png: the gun's screen box nearest-neighbour to 640 px)   the gun at rest
//   shot-N30-G52-<gun>-fire-<off|on>.png (+ -crop.png)                                                     3 frames after a shot (recoil, flash)
// (the row not built: -off52, the one side there is). Printed per gun: the meshes' vertices off / on, their local bounds off / on (the silhouette's
// extent), the strong-edge share ON THE GUN'S PIXELS off / on (round 3: the gun's pixels are those that change when the viewmodel camera is off for
// one frame; n52-r5's whole-box share was mostly the room behind the THUNDERCLAP; an edge is a luma step over Guns52ReelEdgeLuma to the next pixel
// right or up: chamfers, panels and stripes add edges), the whole box's share for reference, the shots registered; the build counts.
// GREEN when guns52 is built and, for both guns: the vertices grow (the machined build ran), the bounds match within 0.1 mm (no new silhouette),
// the idle edge share on the gun grows by Guns52ReelEdgeGain; a shot registered in each fire pair; no frame shows magenta. The fallback
// (design.n52.guns52.numbers.thunderclap 0, the KESTREL only): the THUNDERCLAP must stay plain both ways (what ships), the KESTREL as above.
// Round 4 (leftover 2): each shot's muzzle LIGHT (LightPools.Guns52.cs): ms from the shot to its first dark frame and the frames it was lit, off
// (NIGHT51: fx.flashFrames render frames) and on; GREEN also needs each ON shot's light to last N51StarS of real time (to within the first dark
// frame: <= N51StarS + 2.5 frames + 5 ms).
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
        static partial void N52Guns(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Guns52Scenario(g, dir, res, offs, pre);

        /// <summary>The share of `box`'s pixels whose luma steps by more than Guns52ReelEdgeLuma to the pixel right of it or above it.</summary>
        static float Edges52(Color32[] px, int w, int h, Rect box)
        {
            if (px == null || box.width < 3 || box.height < 3) return -1f;
            int x0 = Mathf.Clamp((int)box.xMin, 0, w - 2), x1 = Mathf.Clamp((int)box.xMax, 0, w - 2), y0 = Mathf.Clamp((int)box.yMin, 0, h - 2), y1 = Mathf.Clamp((int)box.yMax, 0, h - 2);
            float cut = UnityTuning.Guns52ReelEdgeLuma * 255f;
            float L(Color32 c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            int n = 0, e = 0;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float l = L(px[y * w + x]);
                    n++;
                    if (Mathf.Abs(L(px[y * w + x + 1]) - l) > cut || Mathf.Abs(L(px[(y + 1) * w + x]) - l) > cut) e++;
                }
            return n > 0 ? e / (float)n : -1f;
        }

        /// <summary>Round 3: the strong-edge share on the GUN's pixels only: inside `box`, a pixel is the gun's when it differs from the same frame
        /// drawn without the viewmodel (`noGun`) by more than Guns52ReelMaskStep in any channel; an edge as Edges52 (to the next pixel right or up).</summary>
        static float EdgesMasked52(Color32[] px, Color32[] noGun, int w, int h, Rect box, out int gunPx)
        {
            gunPx = 0;
            if (px == null || noGun == null || px.Length != noGun.Length || box.width < 3 || box.height < 3) return -1f;
            int x0 = Mathf.Clamp((int)box.xMin, 0, w - 2), x1 = Mathf.Clamp((int)box.xMax, 0, w - 2), y0 = Mathf.Clamp((int)box.yMin, 0, h - 2), y1 = Mathf.Clamp((int)box.yMax, 0, h - 2);
            float cut = UnityTuning.Guns52ReelEdgeLuma * 255f;
            int step = UnityTuning.Guns52ReelMaskStep;
            float L(Color32 c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            bool Gun(int i) => Mathf.Abs(px[i].r - noGun[i].r) > step || Mathf.Abs(px[i].g - noGun[i].g) > step || Mathf.Abs(px[i].b - noGun[i].b) > step;
            int e = 0;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    int i = y * w + x;
                    if (!Gun(i)) continue;
                    gunPx++;
                    float l = L(px[i]);
                    if (Mathf.Abs(L(px[i + 1]) - l) > cut || Mathf.Abs(L(px[i + w]) - l) > cut) e++;
                }
            return gunPx >= 500 ? e / (float)gunPx : -1f;
        }

        static float BoundsGap52(Bounds a, Bounds b) => Mathf.Max((a.min - b.min).magnitude, (a.max - b.max).magnitude);

        static IEnumerator Guns52Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            offs.Add(() =>
            {
                if (Guns52Probe.ForceOff && g.viewmodel != null) { Guns52Probe.ForceOff = false; g.viewmodel.Guns52Rebuild(); }
                Guns52Probe.ForceOff = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a352: the run did not start as a CAMPAIGN"; yield break; }
            if (g.viewmodel == null) { res[0] = "RED a352: no viewmodel in this Player"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 360;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 340) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput); // the player's own input back (no bot); nothing pressed but the reel's
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            int fires = 0;
            offs.Add(g.events.On("fire", o => fires++));
            string pc = Preconditions(g, g.Feet);
            Start(g, "a352-guns52", "camera", dir);
            yield return Frames(pre);
            bool built = Guns52Probe.Built;
            Viewmodel vm = g.viewmodel;
            var lines = new List<string> { $"guns52 built {(built ? "yes (pairs: ForceOff + rebuild = NIGHT51's guns, then guns52)" : "NO (the row is off in this build or -skygrindOff52: NIGHT51's guns, the RED side)")}; frame {Screen.width}x{Screen.height}" };
            string[] sides = built ? new[] { "off", "on" } : new[] { "off52" };
            Color32[][] frame = new Color32[1][];
            int[] size = new int[2], mag = new int[1];
            int magentaFrames = 0, gunsOk = 0, gunsRun = 0, lightOk = 0, lightChecks = 0;
            var lightNotes = new List<string>();
            float k1080 = Screen.height / 1080f;
            IEnumerator Settle(int n) { for (int i = 0; i < n; i++) { God(g); yield return null; } }

            foreach ((string id, string label) in new[] { ("revolver", "kestrel"), ("shotgun", "thunderclap") })
            {
                if (Time.realtimeSinceStartup > wallEnd) { lines.Add($"{label}: the wall clock ran out"); break; }
                yield return Equip27(g, id);
                g.forcedCombat = IdleCombat;
                yield return Settle(40);
                if (g.loadout.Weapon != id) { lines.Add($"{label}: not in hand (held {g.loadout.Weapon})"); continue; }
                gunsRun++;
                var sb = new System.Text.StringBuilder($"{label}:");
                int vOff = -1, vOn = -1, shotsOff = 0, shotsOn = 0;
                Bounds bOff = default, bOn = default;
                float eOff = -1f, eOn = -1f;
                foreach (string side in sides)
                {
                    if (built)
                    {
                        Guns52Probe.ForceOff = side == "off";
                        vm.Guns52Rebuild();
                    }
                    yield return Settle(12);
                    int verts = vm.Guns52Verts(id);
                    Bounds bb = vm.Guns52Bounds(id);
                    string counts = $"parts {Guns52Probe.ChamferedParts}, profiles {Guns52Probe.ChamferedProfiles}, panels {Guns52Probe.Panels}, plain {Guns52Probe.PlainParts}, folds {Guns52Probe.Folds}";
                    // idle
                    string name = $"G52-{label}-idle-{side}";
                    yield return Grab48(dir, frame, size, name);
                    yield return Capture47(dir, null, mag);
                    if (mag[0] > 0) magentaFrames++;
                    Rect box = vm.Guns52Box(id);
                    float edgesBox = Edges52(frame[0], size[0], size[1], box);
                    if (frame[0] != null && box.width > 2) CropUp48(dir, name + "-crop", frame[0], size[0], size[1], box.center.x, box.center.y, Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(box.width, box.height)), 32, size[1]), 640);
                    // round 3: the gun's own pixels (n52-r5: the THUNDERCLAP's 835 x 480 box was mostly the room behind it, its edges diluted the gun's):
                    // the same frame with the viewmodel camera off for one frame; a pixel that changes is the gun's
                    Color32[] withGun = frame[0];
                    Camera vmc = vm.Camera;
                    float edges = -1f;
                    int gunPx = 0;
                    if (vmc != null && withGun != null)
                    {
                        vmc.enabled = false;
                        yield return Grab48(dir, frame, size, null);
                        vmc.enabled = true;
                        edges = EdgesMasked52(withGun, frame[0], size[0], size[1], box, out gunPx);
                        frame[0] = withGun;
                    }
                    // firing: a press of one frame and its release, then the frame 3 frames on (the recoil and the flash)
                    int f0 = fires, le0 = Guns52Probe.LightEnds;
                    g.forcedCombat = new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } };
                    yield return null;
                    g.forcedCombat = IdleCombat;
                    yield return Settle(3);
                    string fname = $"G52-{label}-fire-{side}";
                    yield return Grab48(dir, frame, size, fname);
                    Rect fbox = vm.Guns52Box(id);
                    if (frame[0] != null && fbox.width > 2) CropUp48(dir, fname + "-crop", frame[0], size[0], size[1], fbox.center.x, fbox.center.y, Mathf.Clamp(Mathf.RoundToInt(Mathf.Max(fbox.width, fbox.height)), 32, size[1]), 640);
                    int shots = fires - f0;
                    float tFrames = Time.unscaledTime;
                    yield return Settle(60); // the cooldown and the recoil settle before the next pose
                    double frameMs = (Time.unscaledTime - tFrames) * 1000.0 / 60.0;
                    // round 4: the muzzle LIGHT's lifetime (LightPools.Guns52.cs): ms from the shot to its first dark frame, the frames it was lit
                    bool lightEnded = Guns52Probe.LightEnds > le0;
                    double lightMs = lightEnded ? Guns52Probe.LightMs : -1;
                    int lightFrames = lightEnded ? Guns52Probe.LightFrames : -1;
                    if (side == "on" && built)
                    {
                        // guns52: lit for N51StarS of real time (the first dark frame lands within about a frame after it)
                        double want = UnityTuning.N51StarS * 1000.0;
                        bool rt = lightEnded && lightMs >= want - 1.0 && lightMs <= want + 2.5 * frameMs + 5.0;
                        lightChecks++;
                        if (rt) lightOk++;
                        lightNotes.Add($"{label} on: light {lightMs.ToString("0.0", ci)} ms over {lightFrames} frames (want {want.ToString("0", ci)} ms of real time; frame {frameMs.ToString("0.0", ci)} ms): {rt}");
                    }
                    else lightNotes.Add($"{label} {side}: light {lightMs.ToString("0.0", ci)} ms over {lightFrames} frames (frame {frameMs.ToString("0.0", ci)} ms; NIGHT51's {UnityTuning.N51StarS * 1000:0} ms rule off: fx.flashFrames render frames)");
                    sb.Append($" {side}: verts {verts}, bounds {bb.size.x.ToString("0.0000", ci)} x {bb.size.y.ToString("0.0000", ci)} x {bb.size.z.ToString("0.0000", ci)} m, edge share on the gun {edges.ToString("0.000", ci)} ({gunPx} gun px; the whole box {edgesBox.ToString("0.000", ci)}, {Mathf.RoundToInt(box.width / k1080)} x {Mathf.RoundToInt(box.height / k1080)} px at 1080p), shots {shots}, build {counts}, stripes {Guns52Probe.Stripes};");
                    if (side == "on") { vOn = verts; bOn = bb; eOn = edges; shotsOn = shots; }
                    else { vOff = verts; bOff = bb; eOff = edges; shotsOff = shots; }
                }
                lines.Add(sb.ToString());
                if (built && id == "shotgun" && !Guns52Probe.Thunderclap)
                {
                    // the fallback (design.n52.guns52.numbers.thunderclap 0): the THUNDERCLAP ships plain; what ships is checked: plain both ways
                    bool plain = vOn == vOff && vOff > 0 && BoundsGap52(bOff, bOn) <= 1e-4f;
                    bool shot = shotsOff > 0 && shotsOn > 0;
                    if (plain && shot) gunsOk++;
                    lines.Add($"{label}: the fallback (thunderclap 0 in this build's config): plain as NIGHT51 {plain} (verts {vOff} -> {vOn}), shots {shot} ({shotsOff} / {shotsOn})");
                }
                else if (built)
                {
                    bool grew = vOn > vOff && vOff > 0;
                    float gap = BoundsGap52(bOff, bOn);
                    bool same = gap <= 1e-4f;
                    bool edgesUp = eOff > 0 && eOn >= UnityTuning.Guns52ReelEdgeGain * eOff;
                    bool shot = shotsOff > 0 && shotsOn > 0;
                    if (grew && same && edgesUp && shot) gunsOk++;
                    lines.Add($"{label}: machined {grew} (verts {vOff} -> {vOn}), silhouette kept {same} (bounds gap {(gap * 1000).ToString("0.000", ci)} mm), edges on the gun {edgesUp} ({eOff.ToString("0.000", ci)} -> {eOn.ToString("0.000", ci)}, want x{UnityTuning.Guns52ReelEdgeGain.ToString("0.00", ci)}), shots {shot} ({shotsOff} / {shotsOn})");
                }
            }
            if (built && Guns52Probe.ForceOff) { Guns52Probe.ForceOff = false; vm.Guns52Rebuild(); }
            lines.Add($"muzzle light: {string.Join("; ", lightNotes)}");
            lines.Add($"magenta frames {magentaFrames}");
            yield return Frames(5);
            yield return Stop();
            bool lightsOk = !built || (lightChecks == gunsRun && lightOk == lightChecks);
            bool ok = built && gunsRun == 2 && gunsOk == 2 && lightsOk && magentaFrames == 0;
            res[0] = $"{(ok ? "GREEN" : "RED")} a352 guns52 | built {built}, guns {gunsOk}/{gunsRun} (machined, silhouette kept, edges up, shots), muzzle light in real time {lightOk}/{lightChecks}, magenta {magentaFrames} || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
