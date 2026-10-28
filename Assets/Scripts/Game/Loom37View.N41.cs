// NIGHT41 builder B8 (NIGHT41.md B8; NIGHT40.md boss_rules 1 "a red-orange decal tell of at least 0.9 s"): THE LOOM's floor sweep on
// Loom37View (view only; the rules are Core/Enemies/Loom37.N41.cs). Drawn only while the sim's sweep is telling or live (sweep41 on this run;
// off: the sweep never tells and nothing here shows).
//  - THE DECAL: a red-orange slab on every ledge (the start / end ledges, the tower tops) inside the sweep's band and under its top: "this
//    floor is about to be swept". It brightens through the wind-up (1.2 s) and blinks in its last quarter.
//  - THE FILL: a brighter strip on each decal growing from the start wall's side with the wind-up: the line's direction.
//  - THE TOP: a thin red-orange bar along the start wall at the sweep's height over the band (leg 1 under the tower tops, legs 2-3 over them).
//  - THE LINE: while it passes, a red-orange sheet across the band from the void up to the top, at the sweep's x.
// Numbers: UnityTuning.Loom41.cs.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        readonly List<Transform> decalT = new List<Transform>(), fillT = new List<Transform>();
        readonly List<Renderer> decalR = new List<Renderer>(), fillR = new List<Renderer>();
        Transform sweepTopT, sweepLineT;
        Renderer sweepTopR, sweepLineR;

        /// <summary>For the reels / frames (sweep41): the ledge decals drawn now, the top bar shown, the line shown.</summary>
        public int SweepDecals41;
        public bool SweepTop41, SweepLine41;

        void Init41()
        {
            sweepTopR = Glow(root, "sweep41 top", boxMesh, out sweepTopT);
            sweepLineR = Glow(root, "sweep41 line", boxMesh, out sweepLineT);
            sweepTopT.gameObject.SetActive(false);
            sweepLineT.gameObject.SetActive(false);
            sweepSheetR = Glow(root, "sweep41 sheet", boxMesh, out sweepSheetT);
            sweepSheetT.gameObject.SetActive(false);
            for (int i = 0; i < 3; i++)
            {
                sweepEdgeR[i] = Glow(root, "sweep41 edge " + i, boxMesh, out sweepEdgeT[i]);
                sweepEdgeT[i].gameObject.SetActive(false);
            }
        }

        Transform sweepSheetT;
        Renderer sweepSheetR;
        readonly Transform[] sweepEdgeT = new Transform[3];
        readonly Renderer[] sweepEdgeR = new Renderer[3];

        static void Slab(Transform tr, float x0, float x1, float y, float z0, float z1, float thick)
        {
            // sim rect (x, z) -> Unity (x, -z)
            tr.position = new Vector3((x0 + x1) * 0.5f, y, -(z0 + z1) * 0.5f);
            tr.localScale = new Vector3(Mathf.Max(0.01f, x1 - x0), thick, Mathf.Max(0.01f, z1 - z0));
            tr.rotation = Quaternion.identity;
        }

        /// <summary>Every fight frame, after Render40.</summary>
        void Render41(SurvivalSim sim, Loom37 L)
        {
            bool telling = L.SweepTelling, live = L.SweepLive;
            int n = 0;
            if (telling || live)
            {
                float prog = telling ? (float)L.SweepTell01 : 1f;
                float gain = Mathf.Lerp(UnityTuning.Loom41DecalGain0, UnityTuning.Loom41DecalGain1, prog);
                if (telling && prog > 0.75f) gain *= 1f + UnityTuning.Loom41DecalBlinkAmp * Mathf.Sign(Mathf.Sin(t * UnityTuning.Loom41DecalBlinkHz * 2f * Mathf.PI));
                Color danger = Look.Hex(UnityTuning.Loom37DangerHex, gain);
                Color fill = Look.Hex(UnityTuning.Loom37DangerHex, gain + UnityTuning.Loom41FillGain);
                float bz0 = (float)L.sweepZ0, bz1 = (float)L.sweepZ1, top = (float)L.sweepTopY;
                float fromX = (float)L.sweepFromX, toX = (float)L.sweepToX;
                foreach (SurvivalSim.Loom37Ledge l in sim.loom37Ledges)
                {
                    if (l.y >= top) continue;
                    float z0 = Mathf.Max((float)l.z0, bz0), z1 = Mathf.Min((float)l.z1, bz1);
                    if (z1 <= z0) continue;
                    Pool(decalT, decalR, n + 1, "sweep41 decal", boxMesh);
                    Pool(fillT, fillR, n + 1, "sweep41 fill", boxMesh);
                    float x0 = (float)l.x0, x1 = (float)l.x1, y = (float)l.y + UnityTuning.Loom41DecalLift;
                    if (!decalT[n].gameObject.activeSelf) decalT[n].gameObject.SetActive(true);
                    Slab(decalT[n], x0, x1, y, z0, z1, UnityTuning.Loom41DecalThick);
                    Tint(decalR[n], danger);
                    // the fill: from the start wall's side, the wind-up's share of the canyon's width (live: the line's place)
                    float reach = telling ? Mathf.Lerp(fromX, toX, prog) : (float)L.sweepLineX;
                    float fx0 = Mathf.Max(x0, Mathf.Min(fromX, reach)), fx1 = Mathf.Min(x1, Mathf.Max(fromX, reach));
                    bool anyFill = fx1 > fx0;
                    if (fillT[n].gameObject.activeSelf != anyFill) fillT[n].gameObject.SetActive(anyFill);
                    if (anyFill)
                    {
                        Slab(fillT[n], fx0, fx1, y + UnityTuning.Loom41DecalThick, z0, z1, UnityTuning.Loom41DecalThick);
                        Tint(fillR[n], fill);
                    }
                    n++;
                }
                Slab(sweepTopT, fromX - UnityTuning.Loom41TopThick, fromX + UnityTuning.Loom41TopThick, top, bz0, bz1, UnityTuning.Loom41TopThick);
                Tint(sweepTopR, Look.Hex(UnityTuning.Loom37DangerHex, UnityTuning.Loom41TopGain));
                // the height sheet (MEASURED n41-r3: leg 1's band held no ledge under its top, so no slab was drawn and the frames showed nothing):
                // a faint red-orange plane over the whole band at the sweep's height, its border bright: under it is swept, over it is safe
                float wx0 = Mathf.Min(fromX, toX), wx1 = Mathf.Max(fromX, toX), et = UnityTuning.Loom41TopThick;
                if (!sweepSheetT.gameObject.activeSelf) sweepSheetT.gameObject.SetActive(true);
                Slab(sweepSheetT, wx0, wx1, top, bz0, bz1, UnityTuning.Loom41SheetThick);
                Tint(sweepSheetR, Look.Hex(UnityTuning.Loom37DangerHex, Mathf.Lerp(UnityTuning.Loom41SheetGain0, UnityTuning.Loom41SheetGain1, prog)));
                for (int i = 0; i < 3; i++) if (!sweepEdgeT[i].gameObject.activeSelf) sweepEdgeT[i].gameObject.SetActive(true);
                Slab(sweepEdgeT[0], wx0, wx1, top, bz0 - et, bz0 + et, et);
                Slab(sweepEdgeT[1], wx0, wx1, top, bz1 - et, bz1 + et, et);
                Slab(sweepEdgeT[2], toX - et, toX + et, top, bz0, bz1, et);
                for (int i = 0; i < 3; i++) Tint(sweepEdgeR[i], Look.Hex(UnityTuning.Loom37DangerHex, UnityTuning.Loom41TopGain));
                if (live)
                {
                    float lx = (float)L.sweepLineX, bottom = UnityTuning.Loom41LineBottomY;
                    sweepLineT.position = new Vector3(lx, (bottom + top) * 0.5f, -(bz0 + bz1) * 0.5f);
                    sweepLineT.localScale = new Vector3(UnityTuning.Loom41LineThick, top - bottom, bz1 - bz0);
                    sweepLineT.rotation = Quaternion.identity;
                    Tint(sweepLineR, Look.Hex(UnityTuning.Loom37DangerHex, UnityTuning.Loom41LineGain));
                }
            }
            Pool(decalT, decalR, n, "sweep41 decal", boxMesh);
            if (n == 0) Pool(fillT, fillR, 0, "sweep41 fill", boxMesh);
            else for (int i = n; i < fillT.Count; i++) if (fillT[i].gameObject.activeSelf) fillT[i].gameObject.SetActive(false);
            bool topOn = telling || live;
            if (sweepTopT.gameObject.activeSelf != topOn) sweepTopT.gameObject.SetActive(topOn);
            if (!topOn)
            {
                if (sweepSheetT.gameObject.activeSelf) sweepSheetT.gameObject.SetActive(false);
                for (int i = 0; i < 3; i++) if (sweepEdgeT[i].gameObject.activeSelf) sweepEdgeT[i].gameObject.SetActive(false);
            }
            if (sweepLineT.gameObject.activeSelf != live) sweepLineT.gameObject.SetActive(live);
            SweepDecals41 = n;
            SweepTop41 = topOn;
            SweepLine41 = live;
        }
    }
}
