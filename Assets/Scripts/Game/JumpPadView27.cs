// NIGHT27 builder P: the jump pad's look. GG: "he also came and tell what a jump pad is just by that circle, like there needs to be a better visual and it needs to
// stand out a little more and look like a real jump pad." The web had ONE flat neon ring. A pad is now (view only, nothing here touches the sim):
//   - a raised dark body disc and a darker rim (opaque, MeshKit),
//   - a glowing cyan fill and ring on top (additive, the neon material's copy; colors.usable is the player-usable cyan), pulsing slowly,
//   - chevron arrows: an aimed pad shows three flat chevrons on the disc pointing at pad.target that sweep toward it; an unaimed pad (straight up) shows three
//     rising chevrons (two crossed vertical planes each) that climb and fade,
//   - an upward particle stream (a ParticleSystem on the additive material),
//   - a stronger burst on the sim's "jumpPad" event (payload: the pad index).
// The PadMarker component and its index stay on the root object, so SurvivalGame.Render hides the whole pad while its zone is locked. Per frame it sets colours
// through one cached MaterialPropertyBlock per renderer: no allocation. Everything is built once from MeshKit.
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class JumpPadView27 : MonoBehaviour
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        // shared materials, one set per map build (keyed by the neon material they were copied from)
        static Material cachedFor, bodyMat, rimMat, glowMat;

        public int index;
        public bool aimed;
        /// <summary>What a test reads: the count of mesh renderers (body, rim, fill, ring, one per chevron), the chevrons, the idle pulse multiplier now (0.55..1), the burst's added glow now.</summary>
        public int meshCount, chevronCount;
        public float pulse, burst;

        Color usable;
        MeshRenderer fill, ring, halo, wave;
        MeshRenderer[] beam;
        Color bright; // usable mixed toward white: the chevrons and the wave
        MeshRenderer[] chev;
        Transform[] chevT;
        ParticleSystem stream;
        ParticleSystem steady; // NIGHT74 pads74: the steadied twin of `stream` (Pad74.cs); the row picks which one renders, every frame; both simulate and feed on the burst
        ParticleSystemRenderer streamR, steadyR;
        SurvivalGame game; // NIGHT74 pads74 (the beam's near fade reads the world camera)
        /// <summary>NIGHT74 pads74: the factor the beam was painted with this frame (1 with the row off); what a test or a reel reads.</summary>
        public float beamNow = 1f;
        MaterialPropertyBlock mpb;
        float burstT = -10f;
        GameConfig cfg;
        float ringR;

        /// <summary>Builds the pad `i` under `root`: the root object carries PadMarker (index) and this view.</summary>
        public static GameObject Build(GameConfig cfg, PadSpec pad, int i, Transform root, SurvivalMaterials mats)
        {
            if (cachedFor != mats.neon)
            {
                cachedFor = mats.neon;
                bodyMat = FxDraw.Opaque(mats.neon, "Pad body", UnityTuning.Pad27BodyColor);
                rimMat = FxDraw.Opaque(mats.neon, "Pad rim", UnityTuning.Pad27RimColor);
                glowMat = FxDraw.Additive(mats.neon, "Pad glow");
            }
            float r = (float)cfg.Num("jumpPad.radius");
            Vector3 c = WebSpace.ToUnity(pad.pos);
            var go = new GameObject($"Pad {i}");
            go.transform.SetParent(root, false);
            go.transform.position = c;
            go.AddComponent<PadMarker>().index = i;
            var v = go.AddComponent<JumpPadView27>();
            v.index = i;
            v.cfg = cfg;
            v.ringR = r;
            v.usable = Look.C(cfg, "colors.usable", 1f);
            v.bright = Color.Lerp(v.usable, Color.white, UnityTuning.Pad27WhiteMix);
            v.mpb = new MaterialPropertyBlock();
            var kit = new MeshKit();
            int meshes = 0;

            // the dark body: a cylinder (top fan + side wall), and the rim: an annulus with a side wall, higher than the body
            Disc(kit, r, UnityTuning.Pad27BodyTop, 0f, 28, true);
            MeshKit.Object("Body", go.transform, kit.Build($"pad body {i}"), bodyMat, shadows: false); meshes++;
            kit.Clear();
            Annulus(kit, r, r + UnityTuning.Pad27RimWidth, UnityTuning.Pad27RimTop, 28);
            MeshKit.Object("Rim", go.transform, kit.Build($"pad rim {i}"), rimMat, shadows: false); meshes++;
            // the glow: the fill (a disc to the ring's inner edge) and the ring
            kit.Clear();
            Disc(kit, r * UnityTuning.Pad27RingInner, UnityTuning.Pad27FillY, 0f, 28, false);
            v.fill = MeshKit.Object("Glow fill", go.transform, kit.Build($"pad fill {i}"), glowMat, shadows: false).GetComponent<MeshRenderer>(); meshes++;
            kit.Clear();
            Annulus(kit, r * UnityTuning.Pad27RingInner, r * UnityTuning.Pad27RingOuter, UnityTuning.Pad27RingY, 28, false);
            v.ring = MeshKit.Object("Glow ring", go.transform, kit.Build($"pad ring {i}"), glowMat, shadows: false).GetComponent<MeshRenderer>(); meshes++;

            // the floor halo (the pad lights the floor around it) and the expanding wave (a unit ring scaled each frame)
            kit.Clear();
            Annulus(kit, r * 1.02f, r * UnityTuning.Pad27HaloOuter, UnityTuning.Pad27HaloY, 28, false);
            v.halo = MeshKit.Object("Halo", go.transform, kit.Build($"pad halo {i}"), glowMat, shadows: false).GetComponent<MeshRenderer>(); meshes++;
            kit.Clear();
            Annulus(kit, 1f, 1f + UnityTuning.Pad27WaveWidth, UnityTuning.Pad27HaloY + 0.005f, 28, false);
            v.wave = MeshKit.Object("Wave", go.transform, kit.Build($"pad wave {i}"), glowMat, shadows: false).GetComponent<MeshRenderer>(); meshes++;
            // the beacon: a tall beam in segments that fade with height; each is two crossed planes, both faces
            int bn = UnityTuning.Pad27BeamHdr.Length;
            v.beam = new MeshRenderer[bn];
            float seg = UnityTuning.Pad27BeamHeight / bn, hw = r * UnityTuning.Pad27BeamHalf;
            for (int k = 0; k < bn; k++)
            {
                kit.Clear();
                float y0 = UnityTuning.Pad27ChevronY + k * seg, y1 = y0 + seg;
                for (int pl = 0; pl < 2; pl++)
                {
                    Vector3 sd = pl == 0 ? Vector3.right : Vector3.forward, nm = pl == 0 ? Vector3.forward : Vector3.right;
                    Vector3 a = sd * -hw + Vector3.up * y0, b = sd * hw + Vector3.up * y0, c2 = sd * hw + Vector3.up * y1, d = sd * -hw + Vector3.up * y1;
                    kit.Quad(a, b, c2, d, nm, Color.white);
                    kit.Quad(a, b, c2, d, -nm, Color.white);
                }
                v.beam[k] = MeshKit.Object($"Beam {k}", go.transform, kit.Build($"pad beam {i}.{k}"), glowMat, shadows: false).GetComponent<MeshRenderer>(); meshes++;
            }

            // the chevrons: flat toward the target (aimed), or rising (unaimed)
            Vector3 flat = WebSpace.ToUnity(new Vec3(pad.target.x - pad.pos.x, 0, pad.target.z - pad.pos.z));
            bool aimed = pad.aimed && flat.sqrMagnitude > 1e-6f;
            v.aimed = aimed;
            int n = UnityTuning.Pad27Chevrons;
            v.chev = new MeshRenderer[n];
            v.chevT = new Transform[n];
            float s = r * UnityTuning.Pad27ChevronSize;
            for (int k = 0; k < n; k++)
            {
                kit.Clear();
                if (aimed)
                {
                    Vector3 f = flat.normalized, side = Vector3.Cross(Vector3.up, f);
                    float along = (k - (n - 1) * 0.5f) * r * UnityTuning.Pad27ChevronStride;
                    Chevron(kit, f * along + Vector3.up * UnityTuning.Pad27ChevronY, f, side, s, Vector3.up, false);
                }
                else
                {
                    Chevron(kit, Vector3.zero, Vector3.up, Vector3.right, s, Vector3.forward, true);
                    Chevron(kit, Vector3.zero, Vector3.up, Vector3.forward, s, Vector3.right, true);
                }
                GameObject co = MeshKit.Object($"Chevron {k}", go.transform, kit.Build($"pad chevron {i}.{k}"), glowMat, shadows: false);
                v.chev[k] = co.GetComponent<MeshRenderer>();
                v.chevT[k] = co.transform;
                meshes++;
            }
            v.chevronCount = n;
            v.meshCount = meshes;
            v.stream = MakeStream(go.transform, r);
            v.streamR = v.stream.GetComponent<ParticleSystemRenderer>();
            v.steady = Pad74View.MakeSteady(go.transform, r, i, glowMat); // NIGHT74 pads74: built with its renderer off; Update's Pad74View.Apply turns one of the two on by the row
            v.steadyR = v.steady.GetComponent<ParticleSystemRenderer>();
            return go;
        }

        // --- geometry -----------------------------------------------------------------------------------------------------------------------

        /// <summary>A disc of radius r at height y (a triangle fan facing up); `wall` adds the cylinder's side from `y0` to y, facing out.</summary>
        static void Disc(MeshKit kit, float r, float y, float y0, int seg, bool wall)
        {
            Vector3 c = new Vector3(0, y, 0);
            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2 / seg, a1 = (i + 1) * Mathf.PI * 2 / seg;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * r, y, Mathf.Sin(a0) * r), p1 = new Vector3(Mathf.Cos(a1) * r, y, Mathf.Sin(a1) * r);
                kit.Triangle(c, p1, p0, Vector3.up, Color.white);
                if (wall) Wall(kit, p0, p1, y0);
            }
        }

        /// <summary>An annulus (inner..outer) at height y; `wall` adds the outer wall down to 0.</summary>
        static void Annulus(MeshKit kit, float inner, float outer, float y, int seg, bool wall = true)
        {
            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2 / seg, a1 = (i + 1) * Mathf.PI * 2 / seg;
                Vector3 o0 = new Vector3(Mathf.Cos(a0) * outer, y, Mathf.Sin(a0) * outer), o1 = new Vector3(Mathf.Cos(a1) * outer, y, Mathf.Sin(a1) * outer);
                Vector3 i0 = new Vector3(Mathf.Cos(a0) * inner, y, Mathf.Sin(a0) * inner), i1 = new Vector3(Mathf.Cos(a1) * inner, y, Mathf.Sin(a1) * inner);
                kit.Quad(i0, o0, o1, i1, Vector3.up, Color.white);
                if (wall) Wall(kit, o0, o1, 0f);
            }
        }

        static void Wall(MeshKit kit, Vector3 top0, Vector3 top1, float y0)
        {
            Vector3 b0 = new Vector3(top0.x, y0, top0.z), b1 = new Vector3(top1.x, y0, top1.z);
            Vector3 n = new Vector3((top0.x + top1.x) * 0.5f, 0, (top0.z + top1.z) * 0.5f).normalized;
            kit.Quad(b0, b1, top1, top0, n, Color.white);
        }

        /// <summary>A V chevron pointing along f: two arm quads from the tip back to the wings (width s along `side`), thickness t along f. `normal` faces the viewer; `both` adds the back face.</summary>
        static void Chevron(MeshKit kit, Vector3 c, Vector3 f, Vector3 side, float s, Vector3 normal, bool both)
        {
            float t = s * UnityTuning.Pad27ChevronThick;
            Vector3 tip = c + f * s * 0.9f, wl = c + side * s - f * s * 0.5f, wr = c - side * s - f * s * 0.5f;
            kit.Quad(tip, wl, wl - f * t, tip - f * t * 1.5f, normal, Color.white);
            kit.Quad(tip, wr, wr - f * t, tip - f * t * 1.5f, normal, Color.white);
            if (both)
            {
                kit.Quad(tip, wl, wl - f * t, tip - f * t * 1.5f, -normal, Color.white);
                kit.Quad(tip, wr, wr - f * t, tip - f * t * 1.5f, -normal, Color.white);
            }
        }

        /// <summary>The upward stream: a circle-shaped cone of angle 0 pointing up, additive, shrinking over life. Local space so it follows the pad.</summary>
        static ParticleSystem MakeStream(Transform parent, float r)
        {
            var go = new GameObject("Stream");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, UnityTuning.Pad27ChevronY, 0);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true; main.playOnAwake = true;
            main.startLifetime = UnityTuning.Pad27StreamLife;
            main.startSpeed = UnityTuning.Pad27StreamSpeed;
            main.startSize = UnityTuning.Pad27StreamSize;
            main.maxParticles = UnityTuning.Pad27StreamMax;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startColor = Color.white;
            var em = ps.emission; em.rateOverTime = UnityTuning.Pad27StreamRate;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 0f; sh.radius = r * 0.7f; sh.rotation = new Vector3(-90f, 0f, 0f);
            var sz = ps.sizeOverLifetime; sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.2f), new Keyframe(0.2f, 1f), new Keyframe(1f, 0f)));
            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = glowMat;
            rend.shadowCastingMode = ShadowCastingMode.Off;
            rend.receiveShadows = false;
            ps.Play();
            return ps;
        }

        // --- life ---------------------------------------------------------------------------------------------------------------------------

        Action off;

        void Start()
        {
            SurvivalGame g = FindAnyObjectByType<SurvivalGame>();
            game = g; // NIGHT74 pads74: the camera's distance for the beam's near fade
            if (g != null) off = g.events.On("jumpPad", o => { if (o is int p && p == index) Burst(); });
        }

        void OnDestroy() { if (off != null) off(); }

        /// <summary>The launch burst: the glow flares and the stream throws a handful of motes.</summary>
        public void Burst()
        {
            burstT = FeelClock.UnscaledTime; // NIGHT63 D1: the shot frame's time in a same-run held frame
            if (stream != null && gameObject.activeInHierarchy) stream.Emit(UnityTuning.Pad27BurstParticles);
            if (steady != null && gameObject.activeInHierarchy) steady.Emit(UnityTuning.N74PadBurstParticles); // NIGHT74 pads74: the twin throws its own (fewer, softer) motes whether or not it renders now
        }

        void Update()
        {
            bool padsOn = NightRows.ViewOn(cfg, Pad74Math.Row); // NIGHT74 pads74: read every frame (D1)
            Pad74View.Apply(padsOn, streamR, steadyR); // off draws the shipped stream, on the steady twin
            // pads74's beam: the shipped number times 1 (exactly: x * 1f is x) with the row off; with it on, softened and faded for a camera close to the column (gg-proxy-start #5, "blinding")
            float beamK = 1f;
            if (padsOn && Pad74View.BeamPart) // -skygrindPad74NoBeam: the stream part alone (the decomposition)
            {
                Camera wc = game != null ? game.cam : null;
                double camD = 1e9;
                if (wc != null) { Vector3 cp = wc.transform.position, pp = transform.position; camD = Math.Sqrt((double)((cp.x - pp.x) * (cp.x - pp.x) + (cp.z - pp.z) * (cp.z - pp.z))); }
                beamK = Pad74Math.BeamFactor(UnityTuning.N74PadBeamScale, camD, UnityTuning.N74PadBeamNearM, UnityTuning.N74PadBeamFarM, UnityTuning.N74PadBeamNearFloor);
            }
            beamNow = beamK;
            float t = FeelClock.UnscaledTime;
            pulse =Mathf.Lerp(UnityTuning.Pad27PulseLow, UnityTuning.Pad27PulseHigh, 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f / UnityTuning.Pad27PulsePeriod));
            float since = t - burstT;
            burst = since < UnityTuning.Pad27BurstSeconds ? UnityTuning.Pad27BurstGlow * (1f - since / UnityTuning.Pad27BurstSeconds) : 0f;
            float glow = pulse + burst;
            Paint(fill, usable * (UnityTuning.Pad27FillHdr * glow));
            Paint(ring, Color.Lerp(usable, bright, 0.5f) * (UnityTuning.Pad27RingHdr * glow));
            Paint(halo, usable * (UnityTuning.Pad27HaloHdr * glow));
            float wp = Frac(t / UnityTuning.Pad27PulsePeriod), rr = ringR * Mathf.Lerp(1f, UnityTuning.Pad27WaveEnd, wp);
            wave.transform.localScale = new Vector3(rr, 1f, rr);
            Paint(wave, bright * (UnityTuning.Pad27WaveHdr * (1f - wp) * (1f - wp) * (1f + burst)));
            for (int k = 0; k < beam.Length; k++) Paint(beam[k], usable * (UnityTuning.Pad27BeamHdr[k] * (0.6f + 0.4f * glow) * beamK));
            int n = chev.Length;
            for (int k = 0; k < n; k++)
            {
                float phase = Frac(t * UnityTuning.Pad27SweepRate - (float)k / n);
                float level;
                if (aimed)
                {
                    level = (1f - phase) * (1f - phase); // the wave runs from the back chevron to the front one, brightest as it lights
                    chevT[k].localPosition = Vector3.zero;
                }
                else
                {
                    level = Mathf.Sin(phase * Mathf.PI); // climbs and fades
                    chevT[k].localPosition = new Vector3(0, UnityTuning.Pad27ChevronY + 0.25f + phase * UnityTuning.Pad27RiseHeight, 0);
                }
                Paint(chev[k], bright * (UnityTuning.Pad27ChevronHdr * (0.25f + 0.75f * level) * (0.7f + 0.3f * glow)));
            }
        }

        static float Frac(float x) => x - Mathf.Floor(x);

        void Paint(MeshRenderer r, Color c)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColor, c);
            r.SetPropertyBlock(mpb);
        }
    }
}
