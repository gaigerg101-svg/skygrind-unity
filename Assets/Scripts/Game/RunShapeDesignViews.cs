// NIGHT11 (U3-24.6, U3-24.7): the looks of the port-only run-shape designs. Draw only; the rules are Core's Power and
// Exfil. Plain blocky shapes in the dusk palette (CLAUDE.md rule 8: cyan = player-usable; red-orange is enemies only, so
// nothing here uses it), after MysteryBoxView / PerkMachines / TelegraphView:
// - PowerSwitchView: a dark cabinet with cyan neon trim, a lever on its front and a lamp on top. While the power is off
//   the trim glows cyan, the lamp pulses and a thin cyan beacon stands over it (the switch is the run's first goal and
//   sits on the west skybridge, out of sight of the spawn); thrown, the lever swings down, the beacon goes and the trim
//   and lamp dim (no longer usable).
// - ExfilRadioView: a small cabinet with an antenna at the spawn; its cyan light blinks while the radio can be called,
//   burns steady once called or while the exfil runs, and is dim otherwise.
// - ExfilSiteView: while an exfil runs, a cyan ring of design.exfil.siteRadius on the floor at the site and a tall cyan
//   beacon column over it (protocol.beamHeight, the PROTOCOL's beacon height); the ring pulses during REACH and burns
//   steady in HORDE, when the beacon thins.
// Construct each only when its Core object is enabled (or let it no-op: a disabled Power / Exfil builds nothing). Call
// Render(frameDt) once per frame (0 while hitstop / pause freezes the sim, as the box view).
// UnityTuning (NIGHT11): the sizes and rates below are render numbers with no web counterpart; they live in
// UnityTuning.cs:
//   PowerWidth 0.9, PowerHeight 1.6, PowerDepth 0.5          the cabinet (m)
//   PowerLeverLength 0.45, PowerLeverAngle 35                 the lever (m; its lean out from vertical, degrees: up
//                                                             when off, down when on)
//   PowerLeverSpeed 4                                         the lever's swing (1 / s)
//   PowerBeaconHeight 30, PowerBeaconRadius 0.25              the beacon over the switch while off (m)
//   PowerPulseRate 2.5                                        the lamp's pulse while off (Hz)
//   PowerDim 0.25                                             the trim and lamp once thrown (x the cyan)
//   RadioWidth 0.6, RadioHeight 1.0, RadioDepth 0.4           the radio cabinet (m)
//   RadioAntenna 0.9                                          the antenna's height over it (m)
//   RadioBlinkRate 2                                          the light's blinks while the radio can be called (Hz)
//   SiteRingBand 0.3                                          the site ring's band (m)
//   SiteBeaconRadius 0.7                                      the site beacon's radius (m)
//   SitePulseRate 1.5                                         the ring's pulse during REACH (Hz)
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>Shared bits: the cabinet colours, a neon part with its own property block, a field beam.</summary>
    static class DesignKit
    {
        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public static readonly int FieldColor = Shader.PropertyToID("_Color");
        public static readonly Color Body = new Color(0.07f, 0.06f, 0.1f); // the Mystery Box's dark body

        /// <summary>A spot's root: at its feet, its front (sin yaw, 0, cos yaw) along local +z (MysteryBoxView's convention).</summary>
        public static Transform Root(string name, BoxSpot s)
        {
            var root = new GameObject(name).transform;
            root.position = WebSpace.ToUnity(s.pos);
            root.rotation = Quaternion.LookRotation(WebSpace.ToUnity(new Vec3(JsMath.Sin(s.yaw), 0, JsMath.Cos(s.yaw))));
            return root;
        }

        /// <summary>A neon mesh (white vertices; the colour comes from a property block per frame).</summary>
        public static Renderer Neon(string name, Transform parent, MeshKit kit, SurvivalGame game) =>
            MeshKit.Object(name, parent, kit.Build(name), game.mats.neon, shadows: false).GetComponent<Renderer>();

        /// <summary>An upright field column (the telegraph beam's look) of radius r and height h, its foot at the parent.</summary>
        public static Transform Beam(string name, Transform parent, float r, float h, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            var rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = ShadowCastingMode.Off;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, h / 2, 0);
            go.transform.localScale = new Vector3(r * 2, h / 2, r * 2);
            return go.transform;
        }

        /// <summary>A field material in `c` from world y `foot` to `foot + h` (fading toward the top).</summary>
        public static Material Field(SurvivalGame game, Color c, float foot, float h)
        {
            var mat = new Material(game.mats.field);
            mat.SetColor(FieldColor, c);
            mat.SetFloat("_FarAlpha", 1);
            mat.SetFloat("_FadeTop", foot + h);
            mat.SetFloat("_FadeLength", h);
            mat.SetFloat("_Stripe", 1000); // a plain sheet
            return mat;
        }

        public static void Tint(Renderer r, MaterialPropertyBlock block, Color c)
        {
            c.a = 1;
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }
    }

    /// <summary>THE POWER SWITCH's cabinet (Core/Power.cs).</summary>
    public sealed class PowerSwitchView
    {
        readonly Power power;
        readonly Transform root, lever, beacon;
        readonly Renderer trim, lamp, knob;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color cyan;
        float leverDown, time;

        public PowerSwitchView(SurvivalGame game, Power power)
        {
            this.power = power;
            if (power == null || !power.enabled) return;
            GameConfig cfg = game.cfg;
            cyan = Look.C(cfg, "colors.usable", 1.4f);
            float w = UnityTuning.PowerWidth, h = UnityTuning.PowerHeight, d = UnityTuning.PowerDepth;
            root = DesignKit.Root("Power switch", power.spot);
            var body = new MeshKit();
            body.Cube(new Vector3(0, 0.06f, 0), new Vector3(w + 0.12f, 0.12f, d + 0.12f), DesignKit.Body * 1.4f); // plinth
            body.Cube(new Vector3(0, h * 0.5f, 0), new Vector3(w, h, d), DesignKit.Body);
            body.Cube(new Vector3(0, h * 0.62f, d * 0.5f + 0.01f), new Vector3(w * 0.7f, h * 0.36f, 0.02f), DesignKit.Body * 2f); // the lever's plate
            MeshKit.Object("cabinet", root, body.Build("power cabinet"), game.mats.stone, shadows: true);
            // Neon trim: the front's two edges and a top bar.
            var t = new MeshKit();
            t.Cube(new Vector3(-w * 0.5f, h * 0.5f, d * 0.5f), new Vector3(0.05f, h * 0.92f, 0.05f), Color.white);
            t.Cube(new Vector3(w * 0.5f, h * 0.5f, d * 0.5f), new Vector3(0.05f, h * 0.92f, 0.05f), Color.white);
            t.Cube(new Vector3(0, h * 0.97f, d * 0.5f), new Vector3(w, 0.05f, 0.05f), Color.white);
            trim = DesignKit.Neon("power trim", root, t, game);
            var l = new MeshKit();
            l.Cube(new Vector3(0, h + 0.09f, 0), new Vector3(0.18f, 0.18f, 0.18f), Color.white);
            lamp = DesignKit.Neon("power lamp", root, l, game);
            // The lever: hinged on the plate, a dark arm with a cyan knob.
            lever = new GameObject("lever hinge").transform;
            lever.SetParent(root, false);
            lever.localPosition = new Vector3(0, h * 0.62f, d * 0.5f + 0.05f);
            var arm = new MeshKit();
            arm.Cube(new Vector3(0, UnityTuning.PowerLeverLength * 0.5f, 0.04f), new Vector3(0.07f, UnityTuning.PowerLeverLength, 0.07f), DesignKit.Body * 3f);
            MeshKit.Object("lever arm", lever, arm.Build("power lever"), game.mats.stone, shadows: true);
            var k = new MeshKit();
            k.Cube(new Vector3(0, UnityTuning.PowerLeverLength, 0.04f), new Vector3(0.14f, 0.14f, 0.14f), Color.white);
            knob = DesignKit.Neon("lever knob", lever, k, game);
            float foot = (float)power.spot.pos.y + h;
            beacon = DesignKit.Beam("power beacon", root, UnityTuning.PowerBeaconRadius, UnityTuning.PowerBeaconHeight, DesignKit.Field(game, Look.C(cfg, "colors.usable"), foot, UnityTuning.PowerBeaconHeight));
            beacon.localPosition += new Vector3(0, h, 0);
        }

        public void Render(double frameDt)
        {
            if (root == null) return;
            time += (float)frameDt;
            bool on = power.On;
            leverDown = Mathf.MoveTowards(leverDown, on ? 1 : 0, (float)frameDt * UnityTuning.PowerLeverSpeed);
            // About local x the arm (+y) leans toward the front (+z): off it points up, thrown it swings out and down.
            lever.localRotation = Quaternion.Euler(Mathf.Lerp(UnityTuning.PowerLeverAngle, 180 - UnityTuning.PowerLeverAngle, leverDown), 0, 0);
            if (beacon.gameObject.activeSelf == on) beacon.gameObject.SetActive(!on);
            float pulse = on ? UnityTuning.PowerDim : 0.7f + 0.3f * Mathf.Sin(time * Mathf.PI * 2 * UnityTuning.PowerPulseRate);
            DesignKit.Tint(trim, block, on ? cyan * UnityTuning.PowerDim : cyan);
            DesignKit.Tint(knob, block, on ? cyan * UnityTuning.PowerDim : cyan);
            DesignKit.Tint(lamp, block, cyan * pulse);
        }
    }

    /// <summary>The exfil radio at the spawn (Core/Exfil.cs).</summary>
    public sealed class ExfilRadioView
    {
        readonly Exfil exfil;
        readonly Transform root;
        readonly Renderer light, trim;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color cyan;
        float time;

        public ExfilRadioView(SurvivalGame game, Exfil exfil)
        {
            this.exfil = exfil;
            if (exfil == null || !exfil.enabled) return;
            cyan = Look.C(game.cfg, "colors.usable", 1.4f);
            float w = UnityTuning.RadioWidth, h = UnityTuning.RadioHeight, d = UnityTuning.RadioDepth;
            root = DesignKit.Root("Exfil radio", exfil.radio);
            var body = new MeshKit();
            body.Cube(new Vector3(0, h * 0.5f, 0), new Vector3(w, h, d), DesignKit.Body);
            body.Cube(new Vector3(0, h * 0.6f, d * 0.5f + 0.01f), new Vector3(w * 0.7f, h * 0.3f, 0.02f), DesignKit.Body * 2.5f); // the dial plate
            body.Cube(new Vector3(w * 0.3f, h + UnityTuning.RadioAntenna * 0.5f, -d * 0.25f), new Vector3(0.03f, UnityTuning.RadioAntenna, 0.03f), DesignKit.Body * 3f); // the antenna
            MeshKit.Object("radio", root, body.Build("exfil radio"), game.mats.stone, shadows: true);
            var t = new MeshKit();
            t.Cube(new Vector3(0, h - 0.03f, d * 0.5f), new Vector3(w, 0.04f, 0.04f), Color.white);
            t.Cube(new Vector3(0, h * 0.6f, d * 0.5f + 0.025f), new Vector3(w * 0.5f, 0.02f, 0.01f), Color.white); // the dial's needle band
            trim = DesignKit.Neon("radio trim", root, t, game);
            var l = new MeshKit();
            l.Cube(new Vector3(w * 0.3f, h + UnityTuning.RadioAntenna + 0.05f, -d * 0.25f), new Vector3(0.12f, 0.12f, 0.12f), Color.white);
            light = DesignKit.Neon("radio light", root, l, game);
        }

        public void Render(double frameDt)
        {
            if (root == null) return;
            time += (float)frameDt;
            bool callable = exfil.CanCall();
            bool live = exfil.phase == ExfilPhase.Called || exfil.Holding;
            float lit = callable ? (Mathf.Repeat(time * UnityTuning.RadioBlinkRate, 1) < 0.5f ? 1 : 0.15f) : live ? 1 : UnityTuning.PowerDim;
            DesignKit.Tint(light, block, cyan * lit);
            DesignKit.Tint(trim, block, callable || live ? cyan : cyan * UnityTuning.PowerDim);
        }
    }

    /// <summary>The running exfil's site: a ring on the floor and a beacon column (Core/Exfil.cs).</summary>
    public sealed class ExfilSiteView
    {
        readonly Exfil exfil;
        readonly Transform root, beacon;
        readonly Renderer ring;
        readonly Material beamMat;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color cyan;
        readonly float beamHeight;
        int shownSite = -2;
        float time;

        public ExfilSiteView(SurvivalGame game, Exfil exfil)
        {
            this.exfil = exfil;
            if (exfil == null || !exfil.enabled) return;
            cyan = Look.C(game.cfg, "colors.usable", 1.4f);
            beamHeight = (float)game.cfg.Num("protocol.beamHeight"); // the PROTOCOL's beacon height (render reuse)
            root = new GameObject("Exfil site").transform;
            var kit = new MeshKit();
            SurvivalBuilder.Ring(kit, Vector3.zero, (float)exfil.siteRadius, UnityTuning.SiteRingBand, 0.03f, 48);
            ring = DesignKit.Neon("site ring", root, kit, game);
            beamMat = DesignKit.Field(game, Look.C(game.cfg, "colors.usable"), 0, beamHeight);
            beacon = DesignKit.Beam("site beacon", root, UnityTuning.SiteBeaconRadius, beamHeight, beamMat);
            root.gameObject.SetActive(false);
        }

        public void Render(double frameDt)
        {
            if (root == null) return;
            time += (float)frameDt;
            int site = exfil.Holding ? exfil.site : -1;
            if (site != shownSite)
            {
                shownSite = site;
                root.gameObject.SetActive(site >= 0);
                if (site >= 0)
                {
                    Vec3 at = exfil.Site(site).pos;
                    root.position = WebSpace.ToUnity(at);
                    beamMat.SetFloat("_FadeTop", (float)at.y + beamHeight); // the field fades by world height
                }
            }
            if (site < 0) return;
            bool horde = exfil.phase == ExfilPhase.Horde;
            float pulse = horde ? 1 : 0.6f + 0.4f * Mathf.Sin(time * Mathf.PI * 2 * UnityTuning.SitePulseRate);
            DesignKit.Tint(ring, block, cyan * pulse);
            float r = UnityTuning.SiteBeaconRadius * (horde ? 0.5f : 1);
            beacon.localScale = new Vector3(r * 2, beamHeight / 2, r * 2);
        }
    }
}
