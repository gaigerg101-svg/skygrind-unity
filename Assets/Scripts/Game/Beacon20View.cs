// NIGHT20 A3 (builder P2): THE BEACON's look (the rule is Core/Beacon20.cs, the run Game/SurvivalSim.Beacon20.cs). Draw
// only, after RunShapeDesignViews (the power switch's cabinet and beacon, the exfil site's column): a dark obelisk on a
// plinth at the beacon's ring spot with neon edges and a lamp, and a column of light over it.
// - Standing (a break, not lit): cyan (CLAUDE.md rule 8: player-usable), the trim and lamp pulsing, the column thin: the
//   thing to walk to, seen from across the map.
// - Lit (taken; the rest of its break): red-orange (rule 8: danger; the elite is coming), steady, the column wider.
// - Its round begins (or nothing stands): hidden.
// Banners on the Ui's line: "A BEACON STANDS ..." at `beaconStand`, "BEACON LIT · ROUND n BRINGS AN ELITE" at `beaconTake`.
// The prompt is the buy prompt's own ("HOLD E · LIGHT THE BEACON · FREE"). Render(worldDt) once a frame (0 in hitstop);
// nothing allocates per frame (the materials and the property block made once; a banner's string per beacon).
// Numbers: UnityTuning.Beacon20.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Beacon20View
    {
        readonly SurvivalGame game;
        readonly Beacon20Run beacon;
        readonly Transform root, beam;
        readonly Renderer trim, lamp, beamRenderer;
        readonly Material cyanBeam, dangerBeam;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color cyan, danger;
        readonly BannerEvent banner = new BannerEvent();
        bool shown, shownLit;
        double shownX = double.NaN, shownZ = double.NaN;
        float time;

        public Beacon20View(SurvivalGame game, Beacon20Run beacon)
        {
            this.game = game;
            this.beacon = beacon;
            if (beacon == null) return;
            GameConfig cfg = game.cfg;
            cyan = Look.C(cfg, "colors.usable", 1.4f);
            danger = Look.C(cfg, "colors.enemy", 1.4f);
            root = new GameObject("Beacon").transform;
            float w = UnityTuning.BeaconWidth, h = UnityTuning.BeaconHeight, ph = UnityTuning.BeaconPlinthHeight;
            var body = new MeshKit();
            body.Cube(new Vector3(0, ph * 0.5f, 0), new Vector3(UnityTuning.BeaconPlinth, ph, UnityTuning.BeaconPlinth), DesignKit.Body * 1.4f);
            body.Cube(new Vector3(0, ph + h * 0.35f, 0), new Vector3(w, h * 0.7f, w), DesignKit.Body);
            body.Cube(new Vector3(0, ph + h * 0.85f, 0), new Vector3(w * 0.7f, h * 0.3f, w * 0.7f), DesignKit.Body * 1.2f); // the tapered top
            MeshKit.Object("beacon obelisk", root, body.Build("beacon obelisk"), game.mats.stone, shadows: true);
            var t = new MeshKit(); // the column's four edges and a band at the taper
            for (int i = 0; i < 4; i++)
            {
                float sx = (i & 1) == 0 ? -1 : 1, sz = (i & 2) == 0 ? -1 : 1;
                t.Cube(new Vector3(sx * w * 0.5f, ph + h * 0.35f, sz * w * 0.5f), new Vector3(0.04f, h * 0.7f, 0.04f), Color.white);
            }
            t.Cube(new Vector3(0, ph + h * 0.7f, 0), new Vector3(w + 0.06f, 0.05f, w + 0.06f), Color.white);
            trim = DesignKit.Neon("beacon trim", root, t, game);
            var l = new MeshKit();
            l.Cube(new Vector3(0, ph + h + UnityTuning.BeaconLamp * 0.5f, 0), new Vector3(UnityTuning.BeaconLamp, UnityTuning.BeaconLamp, UnityTuning.BeaconLamp), Color.white);
            lamp = DesignKit.Neon("beacon lamp", root, l, game);
            float top = ph + h + UnityTuning.BeaconLamp;
            cyanBeam = DesignKit.Field(game, Look.C(cfg, "colors.usable"), 0, UnityTuning.BeaconBeamHeight);
            dangerBeam = DesignKit.Field(game, Look.C(cfg, "colors.enemy"), 0, UnityTuning.BeaconBeamHeight);
            beam = DesignKit.Beam("beacon column", root, UnityTuning.BeaconBeamRadius, UnityTuning.BeaconBeamHeight, cyanBeam);
            beam.localPosition += new Vector3(0, top, 0);
            beamRenderer = beam.GetComponent<Renderer>();
            root.gameObject.SetActive(false);
            game.events.On<BeaconEvent>(Beacon20.StandEvent, _ =>
            {
                banner.text = UnityTuning.BeaconStandBanner;
                game.events.Emit("banner", banner);
            });
            game.events.On<BeaconEvent>(Beacon20.TakeEvent, e =>
            {
                banner.text = string.Format(System.Globalization.CultureInfo.InvariantCulture, UnityTuning.BeaconLitBanner, e.round);
                game.events.Emit("banner", banner);
            });
        }

        public void Render(double worldDt)
        {
            if (root == null) return;
            time += (float)worldDt;
            bool brk = game.run.state == SurvivalRun.State.Break;
            bool lit = brk && beacon.TakenFor > 0;
            bool show = brk && (beacon.Standing || lit);
            if (show != shown)
            {
                shown = show;
                root.gameObject.SetActive(show);
            }
            if (!show) return;
            Vec3 at = beacon.Spot;
            if (at.x != shownX || at.z != shownZ)
            {
                shownX = at.x;
                shownZ = at.z;
                root.position = WebSpace.ToUnity(at);
                float top = (float)at.y + UnityTuning.BeaconPlinthHeight + UnityTuning.BeaconHeight + UnityTuning.BeaconLamp;
                cyanBeam.SetFloat("_FadeTop", top + UnityTuning.BeaconBeamHeight); // the field fades by world height
                dangerBeam.SetFloat("_FadeTop", top + UnityTuning.BeaconBeamHeight);
            }
            if (lit != shownLit)
            {
                shownLit = lit;
                beamRenderer.sharedMaterial = lit ? dangerBeam : cyanBeam;
                float r = lit ? UnityTuning.BeaconLitBeamRadius : UnityTuning.BeaconBeamRadius;
                beam.localScale = new Vector3(r * 2, UnityTuning.BeaconBeamHeight / 2, r * 2);
            }
            float pulse = lit ? 1 : UnityTuning.BeaconPulseFloor + (1 - UnityTuning.BeaconPulseFloor) * (0.5f + 0.5f * Mathf.Sin(time * Mathf.PI * 2 * UnityTuning.BeaconPulseRate));
            Color c = lit ? danger : cyan;
            DesignKit.Tint(trim, block, c * pulse);
            DesignKit.Tint(lamp, block, c * pulse);
        }
    }
}
