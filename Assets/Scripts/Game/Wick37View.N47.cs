// NIGHT47 builder W (wick47 / wickTop47, view row wickLook47; W-cards.md sections 1, 4): what the player SEES of THE WICK's three phases.
// Called once a frame from Wick37View.Render (Render47, one line after Render43). The sim decides, the view reads:
//   ALWAYS (the sim's state, not a look: a view that showed a panel the sim does not collide with would lie):
//     P1  the ride panels and their cyan trim hidden while the sim has them folded into the wall (SurvivalSim.Wick47PanelsOut)
//     P3  the GLASS over the shaft while it is solid (Wick47GlassOn): a faint cyan pane with a cyan rim strip, a pulse across it when it seals
//   wickLook47 (NightRows.ViewOn): the CRACK-OPEN core (yellow, 2 Hz; the plates flung wide), its RING CLOCK (a yellow shell round the cage
//     shrinking with the time left), 3 yellow CHEVRONS on the glass pointing from the player to it; the CHARGE's lane (a red-orange stripe on
//     the glass through the tell, brightening); the GEYSER vents (red-orange discs through the tell, a flash on the eruption); the PLATE RING (a
//     red-orange band sweeping out at its radius).
// No gameplay number here (UnityTuning.Wick47.cs). No Unity API in field initializers (L-035).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Wick37View
    {
        Transform panels47, trim47, steps47, stepLips47, glassT47, glassRimT47, ringClockT47, laneT47, ringBandT47;
        Renderer glassR47, glassRimR47, ringClockR47, laneR47, ringBandR47;
        readonly Transform[] chevronT47 = new Transform[UnityTuning.Wick47Chevrons];
        readonly Renderer[] chevronR47 = new Renderer[UnityTuning.Wick47Chevrons];
        readonly Transform[] geyserT47 = new Transform[4];
        readonly Renderer[] geyserR47 = new Renderer[4];
        Wick37Stack built47;
        bool glassWas47;
        float glassPulse47;
        int lookChecked47 = -1;
        bool look47;
        Mesh ring47Mesh;

        void Render47(SurvivalSim sim, Wick37 w, Vec3 o, bool fight)
        {
            if (built47 != sim.wick37Stack || panels47 == null)
            {
                built47 = sim.wick37Stack;
                panels47 = stackRoot.Find("stack panel");
                trim47 = stackRoot.Find("stack ride trim");
                steps47 = stackRoot.Find("stack step");
                stepLips47 = stackRoot.Find("stack step lips");
                glassT47 = null;
            }
            bool panelsOut = sim.Wick47PanelsOut;
            if (panels47 != null && panels47.gameObject.activeSelf != panelsOut) panels47.gameObject.SetActive(panelsOut);
            if (trim47 != null && trim47.gameObject.activeSelf != panelsOut) trim47.gameObject.SetActive(panelsOut);
            if (steps47 != null && steps47.gameObject.activeSelf != panelsOut) steps47.gameObject.SetActive(panelsOut);
            if (stepLips47 != null && stepLips47.gameObject.activeSelf != panelsOut) stepLips47.gameObject.SetActive(panelsOut);
            // the glass
            bool glass = sim.Wick47GlassOn;
            Wick37Stack s = sim.wick37Stack;
            if (glass && glassT47 == null)
            {
                glassR47 = Glow(stackRoot, "glass (wickTop47)", Disc(), out glassT47);
                glassRimR47 = Glow(stackRoot, "glass rim (wickTop47)", Ring47Mesh(), out glassRimT47);
            }
            if (glassT47 != null)
            {
                glassT47.gameObject.SetActive(glass);
                glassRimT47.gameObject.SetActive(glass);
                if (glass)
                {
                    if (!glassWas47) glassPulse47 = UnityTuning.Wick47GlassPulseS;
                    glassPulse47 = Mathf.Max(0, glassPulse47 - Time.deltaTime);
                    float rw = (float)s.CornerRadius;
                    glassT47.position = U(o) + new Vector3(0, (float)s.p.capY + 0.02f, 0);
                    glassT47.localScale = new Vector3(rw, 1, rw);
                    Tint(glassR47, Look.Hex(UnityTuning.Wick47GlassHex, UnityTuning.Wick47GlassGain + UnityTuning.Wick47GlassPulseGain * glassPulse47 / UnityTuning.Wick47GlassPulseS));
                    glassRimT47.position = U(o) + new Vector3(0, (float)s.p.capY + 0.03f, 0);
                    glassRimT47.localScale = new Vector3(rw, 1, rw);
                    Tint(glassRimR47, Look.Hex(UnityTuning.Wick47GlassHex, UnityTuning.Wick47GlassRimGain));
                }
            }
            glassWas47 = glass;
            if (lookChecked47 != Time.frameCount / 60) { lookChecked47 = Time.frameCount / 60; look47 = NightRows.ViewOn(sim.cfg, "wickLook47"); }
            bool on = fight && look47 && w != null && w.w47 && w.p47 != null;
            Look47Fx(sim, w, o, on);
            Finale47(sim, w, o);
        }

        // --- G7's slow motion on a core punch (wickLook47) and G9's finale (wickFinale47) ----------------------------------------------------
        int punchesSeen47 = -1;
        float finaleLeft47 = -1;
        bool finaleSeen47, finaleRow47;
        Vector3 finaleAt47;
        readonly Transform[] crackT47 = new Transform[UnityTuning.Wick47FinaleCracks];
        readonly Renderer[] crackR47 = new Renderer[UnityTuning.Wick47FinaleCracks];
        /// <summary>The finales drawn (the reel's line reads it).</summary>
        public int Finales47, CorePunchSlows47;

        void Finale47(SurvivalSim sim, Wick37 w, Vec3 o)
        {
            if (w == null || !w.w47 || w.p47 == null) { punchesSeen47 = -1; finaleSeen47 = false; }
            else
            {
                if (punchesSeen47 < 0 || w.corePunches47 < punchesSeen47) punchesSeen47 = w.corePunches47;
                if (w.corePunches47 > punchesSeen47)
                {
                    punchesSeen47 = w.corePunches47;
                    if (look47 && game.Slow39State != null) { game.Slow39State.OnParry(); CorePunchSlows47++; } // G7: the short slow motion (the parry window's)
                }
                if (lookChecked47 >= 0) finaleRow47 = NightRows.ViewOn(sim.cfg, "wickFinale47");
                bool dying = w.brain != null && (w.brain.state == Boss37State.Dying || w.brain.state == Boss37State.Dead);
                if (dying && !finaleSeen47)
                {
                    finaleSeen47 = true;
                    if (finaleRow47 && w.corePunches47 >= w.p47.punches)
                    {
                        Finales47++;
                        finaleLeft47 = UnityTuning.Wick47FinaleS;
                        Vec3 c = w.Centre;
                        finaleAt47 = U(new Vec3(c.x, o.y + w.stack.p.capY + 0.07, c.z));
                        game.Slow39State?.OnParry();
                    }
                }
                if (!dying && w.Alive) finaleSeen47 = false;
            }
            if (crackT47[0] == null) for (int i = 0; i < crackT47.Length; i++) { crackR47[i] = Glow(stackRoot, "glass crack (wickFinale47)", boxMesh, out crackT47[i]); crackT47[i].gameObject.SetActive(false); }
            bool show = finaleLeft47 > 0;
            for (int i = 0; i < crackT47.Length; i++) crackT47[i].gameObject.SetActive(show);
            if (!show) return;
            finaleLeft47 -= FeelClock.UnscaledDelta; // NIGHT63 D1: 0 in a same-run held frame
            float k = Mathf.Clamp01(finaleLeft47 / UnityTuning.Wick47FinaleS), grow = 1 - k;
            for (int i = 0; i < crackT47.Length; i++)
            {
                float a = i * Mathf.PI * 2 / crackT47.Length + 0.37f * (i % 2);
                float len = UnityTuning.Wick47FinaleCrackLen * Mathf.Min(1, grow * 3) * (0.6f + 0.4f * ((i * 7) % 5) / 4f);
                Vector3 d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                crackT47[i].position = finaleAt47 + d * len * 0.5f;
                crackT47[i].rotation = Quaternion.LookRotation(d, Vector3.up);
                crackT47[i].localScale = new Vector3(UnityTuning.Wick47FinaleCrackW, 0.02f, len);
                Tint(crackR47[i], Look.Hex(UnityTuning.Wick47FinaleCrackHex, UnityTuning.Wick47FinaleCrackGain * (0.4f + 0.6f * k)));
            }
            if (slagLight != null) slagLight.intensity = UnityTuning.Stack37SlagLightIntensity * UnityTuning.LightScale * (1 + UnityTuning.Wick47FinaleSlagFlare * k) * Slag54Gain(); // the slag flares under the glass (NIGHT54 light54: x wickSlagGain, 1 off)
        }

        void Look47Fx(SurvivalSim sim, Wick37 w, Vec3 o, bool on)
        {
            if (ringClockT47 == null)
            {
                ringClockR47 = Glow(fxRoot, "core ring clock (wickLook47)", Annulus47Mesh(), out ringClockT47); // round 3: a flat thick ring on the glass (the shell did not read at 6 m)
                laneR47 = Glow(fxRoot, "charge lane (wickLook47)", boxMesh, out laneT47);
                ringBandR47 = Glow(fxRoot, "plate ring (wickLook47)", Ring47Mesh(), out ringBandT47);
                for (int i = 0; i < chevronT47.Length; i++) chevronR47[i] = Glow(fxRoot, "chevron (wickLook47)", Chevron47Mesh(), out chevronT47[i]);
                for (int i = 0; i < geyserT47.Length; i++) geyserR47[i] = Glow(fxRoot, "geyser vent (wickLook47)", Disc(), out geyserT47[i]);
            }
            bool open = on && w.coreOpen47 > 0;
            ringClockT47.gameObject.SetActive(open);
            for (int i = 0; i < chevronT47.Length; i++) chevronT47[i].gameObject.SetActive(open);
            if (open)
            {
                float left = w.coreOpen47 / (float)Mathf.Max(1, w.coreOpenSteps47);
                float pulse = 0.75f + 0.25f * Mathf.Sin(t * UnityTuning.Wick47CorePulseHz * Mathf.PI * 2);
                Color y = Look.Hex(UnityTuning.Tell36YellowHex, UnityTuning.Wick47CoreOpenGain * pulse);
                CoreColour = y;
                Tint(coreR, y);
                coreLight.color = Look.Hex(UnityTuning.Tell36YellowHex);
                for (int i = 0; i < plateT.Count; i++)
                    plateT[i].localRotation = Quaternion.LookRotation(Dir((i + 0.5f) * Mathf.PI * 2 / plateT.Count), Vector3.up) * Quaternion.Euler(-UnityTuning.Wick37OpenTiltDeg, 0, 0);
                float r = (float)w.p.cageR * (1 + (UnityTuning.Wick47RingClockMax - 1) * left);
                Vec3 foot = new Vec3(w.Centre.x, o.y + w.stack.p.capY + 0.06, w.Centre.z); // on the glass under the core
                ringClockT47.position = U(foot);
                ringClockT47.rotation = Quaternion.identity;
                ringClockT47.localScale = new Vector3(r, 1, r);
                Tint(ringClockR47, Look.Hex(UnityTuning.Tell36YellowHex, UnityTuning.Wick47RingClockGain));
                // the chevrons: on the glass, from the player toward the core, pointing at it
                Vec3 me = sim.mover.position, c = w.Centre;
                double fy = o.y + w.stack.p.capY + 0.05;
                for (int i = 0; i < chevronT47.Length; i++)
                {
                    double f = (i + 1.0) / (chevronT47.Length + 1);
                    var p = new Vec3(me.x + (c.x - me.x) * f, fy, me.z + (c.z - me.z) * f);
                    chevronT47[i].position = U(p);
                    Vector3 d = U(c) - U(me);
                    d.y = 0;
                    if (d.sqrMagnitude > 1e-6f) chevronT47[i].rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
                    chevronT47[i].localScale = Vector3.one * UnityTuning.Wick47ChevronSize;
                    Tint(chevronR47[i], Look.Hex(UnityTuning.Tell36YellowHex, UnityTuning.Wick47ChevronGain * (0.6f + 0.4f * Mathf.Sin(t * 8 - i))));
                }
            }
            // the charge's lane
            Boss37Brain b = w != null ? w.brain : null;
            bool lane = on && b != null && w.AttackId == Wick37.Charge47 && (b.state == Boss37State.Tell || b.state == Boss37State.Active);
            laneT47.gameObject.SetActive(lane);
            if (lane)
            {
                Vector3 a = U(w.origin + w.chargeFrom47), z = U(w.origin + w.chargeTo47);
                a.y = z.y = (float)(w.origin.y + w.stack.p.capY + 0.04);
                laneT47.position = (a + z) * 0.5f;
                laneT47.rotation = Quaternion.LookRotation((z - a).normalized, Vector3.up);
                laneT47.localScale = new Vector3(UnityTuning.Wick47LaneW, 0.03f, (z - a).magnitude);
                float g = b.state == Boss37State.Active ? 1.6f : FlashGain(b.Progress);
                Tint(laneR47, Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Wick47LaneGain * Mathf.Max(0.25f, g)));
            }
            // the geyser vents
            bool gey = on && b != null && w.AttackId == Wick37.Geyser47 && (b.state == Boss37State.Tell || b.state == Boss37State.Active);
            for (int i = 0; i < geyserT47.Length; i++)
            {
                bool v = gey && i < w.geyserN47;
                geyserT47[i].gameObject.SetActive(v);
                if (!v) continue;
                Vec3 g = w.origin + w.geyserAt47[i];
                geyserT47[i].position = U(g) + new Vector3(0, 0.05f, 0);
                float rr = (float)w.p47.geyserRadius;
                geyserT47[i].localScale = new Vector3(rr, 1, rr);
                Tint(geyserR47[i], Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Wick47GeyserGain * (b.state == Boss37State.Active ? 2f : FlashGain(b.Progress))));
            }
            // the plate ring
            bool ring = on && w.ring47 > 0;
            ringBandT47.gameObject.SetActive(ring);
            if (ring)
            {
                ringBandT47.position = U(w.Centre);
                Vector3 p = ringBandT47.position;
                p.y = (float)(w.origin.y + w.stack.p.capY + (w.p47.ringBelow + w.p47.ringAbove) * 0.5 - UnityTuning.Wick47RingH * 0.5f);
                ringBandT47.position = p;
                float rr = (float)w.ring47;
                ringBandT47.localScale = new Vector3(rr, UnityTuning.Wick47RingH / 0.25f, rr);
                Tint(ringBandR47, Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Wick47RingGain));
            }
        }

        Mesh annulus47Mesh, chevron47Mesh;

        /// <summary>A flat annulus (outer radius 1, inner Wick47RingClockInner), both faces: the ring clock lying on the glass.</summary>
        Mesh Annulus47Mesh()
        {
            if (annulus47Mesh != null) return annulus47Mesh;
            var k = new MeshKit();
            int n = UnityTuning.Wick47Segments;
            float ri = UnityTuning.Wick47RingClockInner;
            for (int i = 0; i < n; i++)
            {
                float a0 = i * Mathf.PI * 2 / n, a1 = (i + 1) * Mathf.PI * 2 / n;
                Vector3 o0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)), o1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                k.Quad(o0 * ri, o0, o1, o1 * ri, Vector3.up, Color.white);
                k.Quad(o1 * ri, o1, o0, o0 * ri, Vector3.down, Color.white);
            }
            annulus47Mesh = k.Build("wick47 annulus");
            return annulus47Mesh;
        }

        /// <summary>A flat chevron (a V pointing +z, unit size, arms Wick47ChevronArm wide), both faces: on the glass, pointing at the open core.</summary>
        Mesh Chevron47Mesh()
        {
            if (chevron47Mesh != null) return chevron47Mesh;
            var k = new MeshKit();
            float w = UnityTuning.Wick47ChevronArm;
            Vector3 tip = new Vector3(0, 0, 0.5f), l = new Vector3(-0.5f, 0, -0.3f), r = new Vector3(0.5f, 0, -0.3f), back = new Vector3(0, 0, -w);
            foreach (Vector3 end in new[] { l, r })
            {
                Vector3 a = tip, b = tip + back, c = end + back, d = end;
                k.Quad(a, b, c, d, Vector3.up, Color.white);
                k.Quad(d, c, b, a, Vector3.down, Color.white);
            }
            chevron47Mesh = k.Build("wick47 chevron");
            return chevron47Mesh;
        }

        /// <summary>A unit ring (radius 1) strip Wick47GlassRimW wide (relative, as a share of 1 when scaled by the glass's radius) and 0.25 tall:
        /// the glass's rim and the plate ring's band (scaled per use).</summary>
        Mesh Ring47Mesh()
        {
            if (ring47Mesh != null) return ring47Mesh;
            var k = new MeshKit();
            int n = UnityTuning.Wick47Segments;
            float w = UnityTuning.Wick47GlassRimW / 9f, h = 0.25f;
            for (int i = 0; i < n; i++)
            {
                float a0 = i * Mathf.PI * 2 / n, a1 = (i + 1) * Mathf.PI * 2 / n;
                Vector3 o0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)), o1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                k.Quad(o0 * (1 - w), o1 * (1 - w), o1, o0, Vector3.up, Color.white); // the flat rim (seen from above)
                k.Quad(o0, o1, o1 + Vector3.up * h, o0 + Vector3.up * h, -(o0 + o1).normalized, Color.white); // the band (seen from inside)
                k.Quad(o1, o0, o0 + Vector3.up * h, o1 + Vector3.up * h, (o0 + o1).normalized, Color.white); // and from outside
            }
            ring47Mesh = k.Build("wick47 ring");
            return ring47Mesh;
        }
    }
}
