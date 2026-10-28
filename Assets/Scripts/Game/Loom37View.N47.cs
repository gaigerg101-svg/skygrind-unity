// NIGHT47 builder L (loom47's tells on screen: NIGHT47.md G38, G40; L-cards.md section 1; DESIGN-RULES E-4 "every attack has a tell the player can see
// in one frame"): THE LOOM's new attacks and the seam on Loom37View (view only; the rules are Core/Enemies/Loom37.N47.cs). Drawn only while the
// fight runs with loom47 (L.loom47: the sim row's flag for this fight; off: nothing here shows, Loom37View is NIGHT46's).
//  - SHUTTLE DIVE: through its tell, a red-orange line from the body to the perch it marked, brightening with the wind-up and blinking in its last
//    quarter; a red-orange disc on the marked spot (where it lands).
//  - WARP CAGE: through its tell, eight red-orange threads standing on a ring of the cage's radius round where you stood, rising from under the
//    floor to over the head as the wind-up runs (the snap is at the top): get out of the ring.
//  - BOBBIN RAIN: the two outer bobbins red-orange (dodge), the middle one the spool's yellow (punch it back) (one line in Loom37View.cs's spool tint).
//  - THE SEAM: a knee hooked (latched) while you are inside a BEND glows YELLOW (shoot it now: the parry / punch-it language), brighter as its
//    seam fills (Loom37.Seam01).
// Numbers and colours: UnityTuning.Loom47.cs.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        Transform diveLineT, diveSpotT;
        Renderer diveLineR, diveSpotR;
        readonly Transform[] cageT = new Transform[UnityTuning.Loom47CageThreads];
        readonly Renderer[] cageR = new Renderer[UnityTuning.Loom47CageThreads];

        // THE FRAME's falling posts (sim.loom47Posts, a world layer: no level mesh draws them): stone blocks with an amber pool on top while they
        // stand, dropping into the void (and a red-orange edge flash) for Loom47PostFallS after P1 -> P2
        readonly Transform[] postT = new Transform[2], postTopT = new Transform[2];
        readonly Renderer[] postTopR = new Renderer[2];
        Material postMat;
        // THE WARP NET (round 4): THE LOOM's threads stretched under the frame at sim.Loom47NetY: a grid of pale amber threads (the loom's own colour,
        // not cyan: it is the boss's warp the player bounces on), plucked (a sag that rings down) on every catch
        readonly List<Transform> netT = new List<Transform>();
        readonly List<Renderer> netR = new List<Renderer>();
        /// <summary>For the reels / frames: net threads drawn now.</summary>
        public int NetShown47;

        /// <summary>For the reels / frames: the posts drawn (standing or falling).</summary>
        public int PostsShown47;

        /// <summary>For the reels / frames (loom47): the dive line shown, cage threads shown, knees glowing as a seam now.</summary>
        public bool DiveShown47;
        public int CageShown47, SeamsShown47;

        void Init47()
        {
            postMat = Opaque("loom47 post", UnityTuning.Loom47PostHex);
            for (int i = 0; i < 2; i++)
            {
                postT[i] = Solid(root, "loom47 post " + i, boxMesh, postMat).transform;
                postTopR[i] = Glow(root, "loom47 post top " + i, boxMesh, out postTopT[i]);
                postT[i].gameObject.SetActive(false);
                postTopT[i].gameObject.SetActive(false);
            }
            diveLineR = Glow(root, "loom47 dive line", boxMesh, out diveLineT);
            diveSpotR = Glow(root, "loom47 dive spot", boxMesh, out diveSpotT);
            diveLineT.gameObject.SetActive(false);
            diveSpotT.gameObject.SetActive(false);
            for (int i = 0; i < cageT.Length; i++)
            {
                cageR[i] = Glow(root, "loom47 cage thread " + i, boxMesh, out cageT[i]);
                cageT[i].gameObject.SetActive(false);
            }
        }

        /// <summary>A bobbin's colour (Render's spool loop): red-orange for a RED one, else the spool's own.</summary>
        static Color SpoolColor47(Loom37Spool s) => s.red47
            ? Look.Hex(UnityTuning.Loom37DangerHex, UnityTuning.Loom37SpoolGain)
            : s.back ? Look.Hex(UnityTuning.Loom37KneeLiveHex, UnityTuning.Loom37SpoolGain) : Look.Hex(UnityTuning.Loom37SpoolHex, UnityTuning.Loom37SpoolGain);

        /// <summary>Every fight frame, at the end of Render (after the knees: the seam's yellow wins over the knee's cyan).</summary>
        void Render47(SurvivalSim sim, Loom37 L)
        {
            // the warp net
            double netY = sim.Loom47NetY;
            int nx = UnityTuning.Loom47NetThreadsX, nz = UnityTuning.Loom47NetThreadsZ;
            NetShown47 = 0;
            if (!double.IsNaN(netY) && netT.Count == 0) for (int i = 0; i < nx + nz; i++) { netR.Add(Glow(root, "loom47 net " + i, boxMesh, out Transform tr)); netT.Add(tr); }
            float since = sim.loom47NetStep >= 0 ? (float)((sim.simStep - sim.loom47NetStep) * sim.step) : 99f;
            float sag = since < 1.2f ? UnityTuning.Loom47NetPluckM * Mathf.Exp(-since * 3f) * Mathf.Cos(since * 18f) : 0f;
            for (int i = 0; i < netT.Count; i++)
            {
                bool show = !double.IsNaN(netY);
                if (netT[i].gameObject.activeSelf != show) netT[i].gameObject.SetActive(show);
                if (!show) continue;
                NetShown47++;
                float x0 = (float)sim.loom37CanyonX0, x1 = (float)sim.loom37CanyonX1, z0 = (float)sim.loom37CanyonZ0, z1 = (float)sim.loom37CanyonZ1 - UnityTuning.Loom47NetLedgeM; // over the void only (not the start ledge)
                float y = (float)netY - sag;
                Vector3 na, nb;
                if (i < nx) { float x = Mathf.Lerp(x0, x1, (i + 0.5f) / nx); na = U(new Vec3(x, y, z0)); nb = U(new Vec3(x, y, z1)); }
                else { float z = Mathf.Lerp(z0, z1, (i - nx + 0.5f) / nz); na = U(new Vec3(x0, y, z)); nb = U(new Vec3(x1, y, z)); }
                Span(netT[i], na, nb, UnityTuning.Loom47NetThick);
                Tint(netR[i], Look.Hex(UnityTuning.Look47AmberHex, UnityTuning.Loom47NetGain * (since < 0.4f ? 2f : 1f)));
            }
            // the posts: standing, or falling for Loom47PostFallS after the cut
            PostsShown47 = 0;
            double fellT = sim.loom47PostsFellStep >= 0 ? (sim.simStep - sim.loom47PostsFellStep) * sim.step : 1e9;
            for (int i = 0; i < 2; i++)
            {
                bool have = i < sim.loom47Posts.Count;
                bool falling = have && !sim.loom47PostsUp && fellT < UnityTuning.Loom47PostFallS;
                bool show = have && (sim.loom47PostsUp || falling);
                if (postT[i].gameObject.activeSelf != show) postT[i].gameObject.SetActive(show);
                if (postTopT[i].gameObject.activeSelf != show) postTopT[i].gameObject.SetActive(show);
                if (!show) continue;
                PostsShown47++;
                var (mn, mx) = sim.loom47Posts[i];
                float drop = falling ? 0.5f * UnityTuning.Loom47PostFallG * (float)(fellT * fellT) : 0f;
                Vector3 pa = U(mn), pb = U(mx);
                Vector3 lo = Vector3.Min(pa, pb), hi = Vector3.Max(pa, pb);
                postT[i].position = (lo + hi) * 0.5f + Vector3.down * drop;
                postT[i].rotation = falling ? Quaternion.Euler((float)fellT * 12f * (i == 0 ? 1 : -1), 0, (float)fellT * 8f) : Quaternion.identity;
                postT[i].localScale = hi - lo;
                postTopT[i].position = new Vector3((lo.x + hi.x) * 0.5f, hi.y + 0.03f - drop, (lo.z + hi.z) * 0.5f);
                postTopT[i].rotation = postT[i].rotation;
                postTopT[i].localScale = new Vector3((hi.x - lo.x) * 0.6f, 0.05f, (hi.z - lo.z) * 0.6f);
                Tint(postTopR[i], falling ? Look.Hex(UnityTuning.Loom37DangerHex, 2.2f) : Look.Hex(UnityTuning.Look47AmberHex, UnityTuning.Loom47PostPoolGain));
            }
            bool on = L.loom47;
            Boss37Attack a = L.brain.Current;
            bool telling = on && a != null && L.brain.state == Boss37State.Tell;
            float prog = (float)L.brain.Progress;
            float blink = prog > 0.75f ? (Mathf.Sin(t * UnityTuning.Loom47BlinkHz * 2f * Mathf.PI) > 0 ? 1f : 0.35f) : 1f;
            float gain = Mathf.Lerp(UnityTuning.Loom37TellGain, UnityTuning.Loom37LiveGain, prog) * blink;
            Color danger = Look.Hex(UnityTuning.Loom37DangerHex, gain);

            // SHUTTLE DIVE
            bool dive = telling && a.id == Loom37.Dive;
            DiveShown47 = dive;
            if (diveLineT.gameObject.activeSelf != dive) diveLineT.gameObject.SetActive(dive);
            if (diveSpotT.gameObject.activeSelf != dive) diveSpotT.gameObject.SetActive(dive);
            if (dive)
            {
                Vector3 from = U(L.centre), to = U(L.diveTo) + Vector3.down * (float)(L.p.bodyHalfH + (L.p47 != null ? L.p47.diveNoseY : 3));
                Span(diveLineT, from, to, UnityTuning.Loom47DiveLineThick);
                Tint(diveLineR, danger);
                diveSpotT.position = to + Vector3.up * 0.05f;
                diveSpotT.rotation = Quaternion.identity;
                diveSpotT.localScale = new Vector3(UnityTuning.Loom47DiveSpotM, 0.05f, UnityTuning.Loom47DiveSpotM);
                Tint(diveSpotR, danger);
            }

            // WARP CAGE
            bool cage = telling && a.id == Loom37.Cage && L.p47 != null;
            CageShown47 = cage ? cageT.Length : 0;
            for (int i = 0; i < cageT.Length; i++)
            {
                if (cageT[i].gameObject.activeSelf != cage) cageT[i].gameObject.SetActive(cage);
                if (!cage) continue;
                float ang = i * 2f * Mathf.PI / cageT.Length;
                float r = (float)L.p47.cageRadius, h = (float)L.p47.cageHalfH;
                Vector3 c = U(L.cageAt) + new Vector3(Mathf.Cos(ang) * r, 0, Mathf.Sin(ang) * r);
                Vector3 lo = c + Vector3.down * h, hi = c + Vector3.up * (-h + 2f * h * prog);
                Span(cageT[i], lo, hi, UnityTuning.Loom47CageThick);
                Tint(cageR[i], danger);
            }

            // THE SEAM
            SeamsShown47 = 0;
            if (!on || !sim.Bend34Inside || L.Open) return;
            for (int i = 0; i < 6; i++)
            {
                if (L.knees[i].torn || !L.Latched40(i)) continue;
                SeamsShown47++;
                float f = (float)L.Seam01(i);
                float pulse = 0.75f + 0.25f * Mathf.Sin(t * UnityTuning.Loom47SeamHz * 2f * Mathf.PI);
                Tint(kneeR[i], BossColours63.PartRim(game.cfg, "loom.seams",Look.Hex(UnityTuning.Loom47SeamHex, (UnityTuning.Loom47SeamGain + UnityTuning.Loom47SeamGainFull * f) * pulse))); // NIGHT63 B3: the weak point's cyan, never the parry's yellow, with bossColours63
                kneeT[i].localScale = Vector3.one * (2f * (float)L.p.kneeRadius * (1f + UnityTuning.Loom47SeamGrow * f));
            }
        }
    }
}
