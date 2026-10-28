// NIGHT37 builder C (crown37): THE CROWN itself in Crown37View. View only. A ring of 8 steel blades round an empty centre (the ring's facing is
// the sim's `normal`; the blades spin with `spin`), a red-orange gem on each blade (enemy colour; a broken gem is gone), the whole ring CYAN
// in the window (D-5: "the ring stops spinning CYAN"), a cyan flash through the opening on a pass; its tells raise Tells36 (the shared tell
// view draws the YELLOW / RED flash at its centre); the blade ring's cut is a red-orange band round the spire at the cut's height, its front
// at cutR; the shards are small gems (yellow-lit = parry it, red = dodge); a point light rides the Crown.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        /// <summary>For the reel: the Crown drawn now, the window's cyan on, a pass flash on, the cut band on, shards drawn.</summary>
        public static bool CrownShown, WindowShown, PassFlashShown, CutShown;
        public static int ShardsShown, GemsShown;
        /// <summary>For the reel: the ring's tell colour drawn now ("yellow", "red" or "").</summary>
        public static string TellShownC = "";

        Transform crownT, spinT, cutT;
        readonly List<Renderer> blades = new List<Renderer>(), gems = new List<Renderer>(), cutSegs = new List<Renderer>();
        readonly List<Transform> shardT = new List<Transform>();
        readonly List<Renderer> shardR = new List<Renderer>();
        Light crownLight;
        Color steel, gemColor, yellow, red;

        partial void CrownReset()
        {
            crownT = spinT = cutT = null;
            blades.Clear();
            gems.Clear();
            cutSegs.Clear();
            shardT.Clear();
            shardR.Clear();
            crownLight = null;
            Crown38Reset(); // NIGHT38 C2: the look pass's parts (Crown37View.Crown38.cs)
            Crown72Reset(); // NIGHT72 B2 bossfx72: the base scales are read again (Crown37View.Crown72.cs)
        }

        partial void Crown40Draw(Crown37 c, SurvivalSim sim, float now, bool window, bool telling, bool flash);
        partial void Crown40Hide();
        partial void Weak74Crown(Crown37 c, bool window, bool flash); // NIGHT74 B weak74 (Crown37View.Weak74.cs): the shut ring's cyan taken to the identity colour, the probe's boxes

        void BuildCrown(Crown37 c)
        {
            steel = Look.Hex(UnityTuning.Crown37RingHex);
            gemColor = Look.Hex(UnityTuning.Crown37GemHex);
            yellow = Look.Hex(UnityTuning.Tell36YellowHex); // the parryable glow (the tell grammar's yellow)
            red = Look.Hex(UnityTuning.Tell36RedHex);
            crownT = new GameObject("THE CROWN").transform;
            crownT.SetParent(root, false);
            spinT = new GameObject("spin").transform;
            spinT.SetParent(crownT, false);
            float r = (float)c.p.ringRadius;
            RingSegments(spinT, r, blades);
            int n = c.p.gems;
            for (int k = 0; k < n; k++)
            {
                float a = k * 2 * Mathf.PI / n;
                // a blade pointing out from the ring, a gem set at its root
                var bl = Box(spinT, "blade", ringMat, new Vector3(Mathf.Cos(a) * (r + UnityTuning.Crown37BladeLength / 2), Mathf.Sin(a) * (r + UnityTuning.Crown37BladeLength / 2), 0),
                    new Vector3(UnityTuning.Crown37BladeLength, 0.22f, 0.12f), out Renderer br);
                bl.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg);
                blades.Add(br);
                float g = UnityTuning.Crown37GemSize;
                var gm = Box(spinT, "gem", addMat, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0), new Vector3(g, g, g), out Renderer gr);
                gm.localRotation = Quaternion.Euler(0, 0, 45 + a * Mathf.Rad2Deg);
                gems.Add(gr);
            }
            var lg = new GameObject("crown light");
            lg.transform.SetParent(crownT, false);
            crownLight = lg.AddComponent<Light>();
            crownLight.type = LightType.Point;
            crownLight.range = UnityTuning.Crown37LightRange;
            crownLight.intensity = UnityTuning.Crown37LightIntensity;
            crownLight.shadows = LightShadows.None;
            cutT = new GameObject("blade ring cut").transform;
            cutT.SetParent(root, false);
            int segs = 48;
            for (int k = 0; k < segs; k++)
            {
                var s = Box(cutT, "cut", addMat, Vector3.zero, Vector3.one, out Renderer sr);
                cutSegs.Add(sr);
            }
        }

        partial void DrawCrown(SurvivalSim sim, float alpha, float now)
        {
            Crown37 c = sim.crown37;
            bool on = c != null && c.Alive;
            if (!on)
            {
                if (crownT != null) { crownT.gameObject.SetActive(false); cutT.gameObject.SetActive(false); }
                Crown40Hide(); // NIGHT40 B5: its words go with it
                Crown47Hide(); // NIGHT47 L (crown47): the flame and the toll go with it (Crown37View.N47.cs)
                foreach (Transform t in shardT) t.gameObject.SetActive(false);
                CrownShown = WindowShown = PassFlashShown = CutShown = false;
                ShardsShown = GemsShown = 0;
                return;
            }
            if (crownT == null)
            {
                BuildCrown(c);
                Crown38Build(c, sim); // NIGHT38 C2: the look pass (design.n38.crown38; nothing with the row off)
            }
            crownT.gameObject.SetActive(true);
            Vec3 a = c.prevCentre, b = c.centre;
            crownT.position = U(a.x + (b.x - a.x) * alpha, a.y + (b.y - a.y) * alpha, a.z + (b.z - a.z) * alpha);
            Vector3 n = U(c.normal) - U(0, 0, 0);
            crownT.rotation = Quaternion.LookRotation(n.sqrMagnitude > 1e-6f ? n.normalized : Vector3.forward);
            spinT.localRotation = Quaternion.Euler(0, 0, (float)c.spin * Mathf.Rad2Deg);
            bool window = c.Open;
            float since = (c.stepNo - c.lastPassStep) * (float)sim.step;
            bool flash = since >= 0 && since < UnityTuning.Crown37PassFlashSeconds;
            foreach (Renderer r in blades)
            {
                if (window) Tint(r, cyan, 1.3f + 0.3f * Mathf.Sin(now * 6f));
                else if (flash) Tint(r, cyan, UnityTuning.Crown37PassFlashGain * (1f - since / UnityTuning.Crown37PassFlashSeconds) + 0.6f);
                else Tint(r, steel, 1f);
            }
            // the tell grammar on the ring itself: a YELLOW wind-up (the gem shard: parry it) lights every gem and blade yellow, a RED one red, rising
            Boss37Brain br = c.brain;
            bool telling = br.state == Boss37State.Tell && br.Current != null;
            Color tellC = telling && br.Current.kind == Tell36Kind.Yellow ? yellow : red;
            // NIGHT63 B3c (bossColours63, builder D; B3's GATE: the tell ring read amber): a RED (dodge-only) tell is danger after its HDR gain,
            // clip-safe (BossColours63.Hot); the yellow (parryable) tell is never routed. Row off: Hot returns tellC x the gain, as Tint did.
            bool redTell = telling && br.Current.kind != Tell36Kind.Yellow;
            float tellGain = telling ? 0.8f + 1.2f * (float)br.Progress : 0f;
            if (telling && !window)
                foreach (Renderer r in blades)
                {
                    if (redTell) Tint(r, BossColours63.Hot(game.cfg, "crown.tell", tellC, 0.6f + 0.6f * (float)br.Progress), 1f);
                    else Tint(r, tellC, 0.6f + 0.6f * (float)br.Progress);
                }
            int left = c.GemsLeft, shown = 0;
            for (int k = 0; k < gems.Count; k++)
            {
                bool alive = k < left;
                gems[k].enabled = alive;
                if (!alive) continue;
                shown++;
                if (redTell && !window) Tint(gems[k], BossColours63.Hot(game.cfg, "crown.tell", tellC, UnityTuning.Crown37GemGain * tellGain), 1f); // NIGHT63 B3c
                else Tint(gems[k], window ? cyan : telling ? tellC : gemColor, telling && !window ? UnityTuning.Crown37GemGain * tellGain : UnityTuning.Crown37GemGain);
            }
            crownLight.color = window || flash ? cyan : telling ? tellC : gemColor;
            TellShownC = telling ? (br.Current.kind == Tell36Kind.Yellow ? "yellow" : "red") : "";
            // the blade ring's cut: a band round the spire's axis at cutY, from the core out to the front cutR
            bool cut = c.cutR > 0;
            cutT.gameObject.SetActive(cut);
            if (cut)
            {
                int m = cutSegs.Count;
                float rr = (float)c.cutR, len = 2 * Mathf.PI * rr / m * 1.08f;
                for (int k = 0; k < m; k++)
                {
                    float an = k * 2 * Mathf.PI / m;
                    Transform t = cutSegs[k].transform;
                    t.position = U(c.axis.x + Mathf.Cos(an) * rr, c.cutY, c.axis.z + Mathf.Sin(an) * rr);
                    Vector3 tan = U(-Mathf.Sin(an), 0, Mathf.Cos(an)) - U(0, 0, 0); // the ring's tangent (web space to Unity)
                    t.rotation = Quaternion.LookRotation(tan.normalized);
                    t.localScale = new Vector3(0.12f, UnityTuning.Crown37CutHeight, len);
                    Tint(cutSegs[k], danger, UnityTuning.Crown37CutGain);
                }
            }
            // the shards
            while (shardT.Count < c.shards.Count)
            {
                float s = UnityTuning.Crown37ShardSize;
                shardT.Add(Box(root, "shard", addMat, Vector3.zero, new Vector3(s, s, s), out Renderer sr));
                shardR.Add(sr);
            }
            for (int k = 0; k < shardT.Count; k++)
            {
                bool live = k < c.shards.Count;
                shardT[k].gameObject.SetActive(live);
                if (!live) continue;
                Crown37Shard sh = c.shards[k];
                Vec3 p0 = sh.prev, p1 = sh.pos;
                shardT[k].position = U(p0.x + (p1.x - p0.x) * alpha, p0.y + (p1.y - p0.y) * alpha, p0.z + (p1.z - p0.z) * alpha);
                shardT[k].rotation = Quaternion.Euler(now * 400f, now * 260f, 0);
                Tint(shardR[k], sh.back ? cyan : sh.red ? danger : yellow, 1.6f);
            }
            Crown38Draw(c, now, window, telling, tellC, flash, Time.deltaTime); // NIGHT38 C2: the look pass (nothing with the row off)
            Crown40Draw(c, sim, now, window, telling, flash); // NIGHT40 B5: the ring reads as the player's (cyan) + its words (nothing with crown40 off)
            Flame48Select(); // NIGHT48 L (flame48, a view row: Crown37View.Flame48.cs): the particle-cone flame (off: NIGHT47's)
            Crown47Draw(c, sim, now); // NIGHT47 L (crown47): CROWN FIRE and BELL TOLL (nothing with the row off)
            DrawMotion63(c, sim); // NIGHT63 V bossMotion63 (Crown37View.N63V.cs): the springs, the wind-up, the flinch, the slump, the breath on the ring (off: the rigid pose)
            Crown72Draw(c, sim, now, window, telling, redTell, flash); // NIGHT72 B2 bossfx72 (Crown37View.Crown72.cs): the gems small and clear of the crosshair, the ring cyan through a red tell (NIGHT71's frame with the row off); after the motion: the gems' screen positions are final
            Weak74Crown(c, window, flash); // NIGHT74 B weak74 (Crown37View.Weak74.cs): after the last tint of the frame (crown72's), the shut ring's cyan taken to the ivory identity colour (nothing with the row off but the probe)
            TripoCrown(c); // PG3 tripo74: THE CROWN's Tripo model over the ring, the gems' and glows' colours kept (Crown37View.Tripo.cs; nothing when its file is missing or the row is off)
            CrownShown = true;
            WindowShown = window;
            PassFlashShown = flash;
            CutShown = cut;
            ShardsShown = c.shards.Count;
            GemsShown = shown;
        }
    }
}
