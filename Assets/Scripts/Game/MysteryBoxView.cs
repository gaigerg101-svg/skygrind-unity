// The Mystery Box's look (NIGHT10 U3-13.8; new design, D-20: CoD Zombies' box, the web game never had one). A plain
// blocky crate in the dusk palette: a dark body with the player-usable cyan trim (CLAUDE.md rule 8), a lid that swings
// open while a roll runs, and over it the roll's gun as a cyan hologram (the wall buys' kit models, BlockyKit.ForWeapon)
// that rises and cycles through the ported guns (MysteryBox.DisplayWeapon), then waits to be taken and sinks back when
// the offer runs out. A teddy is a simple blocky bear in a muted grey (not red-orange: it isn't an enemy). On a move the
// crate sinks into the floor at the old spot and rises at the new one. Draw only: the sim is Core's MysteryBox.
// Sizes and timings of the drawing are UnityTuning numbers (render, not gameplay).
// NIGHT12 U3-25.6 (Research/2026-10-03-polish.md F3: CoD's box, Vampire Survivors' slot-machine chest): the reveal cycles
// like a slot reel slowing down: the hologram samples the roll's cycle (MysteryBox.DisplayWeapon, every ported gun) on
// its own schedule, one gun per design.box.cycleTime at first, slowing to UnityTuning.BoxCycleSlowest x it by the end
// (eased in), each new gun with a small pop; then it settles on the result as the offer opens, with a bigger pop and a
// brightness flash of the cyan hologram. The outcome is still never shown before the offer (the samples are the cycle's).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class MysteryBoxView
    {
        readonly SurvivalGame game;
        readonly MysteryBox box;
        readonly Transform root, lid, holoRoot, teddy;
        readonly Dictionary<string, GameObject> holos = new Dictionary<string, GameObject>();
        readonly Dictionary<string, Material> holoMats = new Dictionary<string, Material>();
        readonly Color holoColor;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        string shownHolo;
        float lidOpen, time;
        // the reveal's reel: the gun sampled, the next sample's time (s into the reveal), the pops
        string reelGun;
        double reelClock, reelNext;
        float popLeft, settleLeft, shownFlash = -1;
        BoxState lastState = BoxState.Idle;

        public MysteryBoxView(SurvivalGame game, MysteryBox box)
        {
            this.game = game;
            this.box = box;
            GameConfig cfg = game.cfg;
            holoColor = Look.C(cfg, "colors.usable", 1.6f);
            root = new GameObject("Mystery box").transform;
            float w = UnityTuning.BoxWidth, d = UnityTuning.BoxDepth, h = UnityTuning.BoxHeight;
            Color body = new Color(0.07f, 0.06f, 0.1f), trim = Look.C(cfg, "colors.usable", 1.4f);
            var kit = new MeshKit();
            kit.Cube(new Vector3(0, h * 0.5f, 0), new Vector3(w, h, d), body);
            // the cyan trim: a band round the top edge and the two front corners
            kit.Cube(new Vector3(0, h - 0.04f, d * 0.5f + 0.005f), new Vector3(w, 0.05f, 0.02f), trim);
            kit.Cube(new Vector3(0, h - 0.04f, -d * 0.5f - 0.005f), new Vector3(w, 0.05f, 0.02f), trim);
            kit.Cube(new Vector3(-w * 0.5f + 0.03f, h * 0.5f, d * 0.5f + 0.005f), new Vector3(0.05f, h, 0.02f), trim);
            kit.Cube(new Vector3(w * 0.5f - 0.03f, h * 0.5f, d * 0.5f + 0.005f), new Vector3(0.05f, h, 0.02f), trim);
            kit.Cube(new Vector3(0, h * 0.55f, d * 0.5f + 0.01f), new Vector3(0.22f, 0.22f, 0.02f), trim); // the "?" plate
            MeshKit.Object("crate", root, kit.Build("mystery box"), game.mats.stone, shadows: true);
            // The lid hinges on the back edge.
            var hinge = new GameObject("lid hinge").transform;
            hinge.SetParent(root, false);
            hinge.localPosition = new Vector3(0, h, -d * 0.5f);
            var lidKit = new MeshKit();
            lidKit.Cube(new Vector3(0, 0.04f, d * 0.5f), new Vector3(w + 0.04f, 0.08f, d + 0.04f), body);
            lidKit.Cube(new Vector3(0, 0.085f, d * 0.5f), new Vector3(w * 0.6f, 0.01f, d * 0.4f), trim);
            MeshKit.Object("lid", hinge, lidKit.Build("mystery box lid"), game.mats.stone, shadows: true);
            lid = hinge;
            holoRoot = new GameObject("box hologram").transform;
            holoRoot.SetParent(root, false);
            var bearKit = new MeshKit();
            Color fur = new Color(0.55f, 0.53f, 0.5f), dark = new Color(0.12f, 0.11f, 0.12f);
            bearKit.Cube(new Vector3(0, 0.18f, 0), new Vector3(0.28f, 0.32f, 0.2f), fur); // body
            bearKit.Cube(new Vector3(0, 0.44f, 0), new Vector3(0.24f, 0.22f, 0.2f), fur); // head
            bearKit.Cube(new Vector3(-0.1f, 0.57f, 0), new Vector3(0.07f, 0.07f, 0.05f), fur); // ears
            bearKit.Cube(new Vector3(0.1f, 0.57f, 0), new Vector3(0.07f, 0.07f, 0.05f), fur);
            bearKit.Cube(new Vector3(-0.19f, 0.22f, 0), new Vector3(0.08f, 0.2f, 0.08f), fur); // arms
            bearKit.Cube(new Vector3(0.19f, 0.22f, 0), new Vector3(0.08f, 0.2f, 0.08f), fur);
            bearKit.Cube(new Vector3(-0.06f, 0.47f, 0.101f), new Vector3(0.03f, 0.03f, 0.01f), dark); // eyes
            bearKit.Cube(new Vector3(0.06f, 0.47f, 0.101f), new Vector3(0.03f, 0.03f, 0.01f), dark);
            teddy = MeshKit.Object("teddy", root, bearKit.Build("teddy"), game.mats.stone, shadows: false).transform;
            teddy.gameObject.SetActive(false);
        }

        GameObject Holo(string id)
        {
            if (holos.TryGetValue(id, out GameObject go)) return go;
            var kit = new MeshKit();
            if (!BlockyKit.ForWeapon(id, kit, Color.white)) return holos[id] = null;
            var mat = new Material(game.mats.neon);
            mat.SetColor(BaseColorId, holoColor);
            go = MeshKit.Object($"box holo {id}", holoRoot, kit.Build($"box {id}"), mat, shadows: false);
            go.transform.localScale = Vector3.one * (float)(UnityTuning.BoxHoloSize / WallBuys.ModelLength(id));
            go.SetActive(false);
            holoMats[id] = mat;
            return holos[id] = go;
        }

        public void Render(double frameDt)
        {
            time += (float)frameDt;
            BoxState s = box.state;
            // Where it stands: the current spot (while Moving, sinking at the old one, then rising at the new one).
            BoxSpot at = box.Current;
            float sink = 0;
            if (s == BoxState.Moving)
            {
                float p = (float)box.PhaseProgress;
                if (p < 0.5f && box.fromSpot >= 0) at = box.Spot(box.fromSpot);
                sink = p < 0.5f ? p * 2 : (1 - p) * 2; // down at the old spot, up at the new one
            }
            root.position = WebSpace.ToUnity(at.pos) + Vector3.down * (sink * (UnityTuning.BoxHeight + 0.1f));
            root.rotation = Quaternion.LookRotation(WebSpace.ToUnity(new Vec3(JsMath.Sin(at.yaw), 0, JsMath.Cos(at.yaw))));
            // The lid: open while a roll shows (Revealing, Offering, Teddy), shut otherwise.
            bool open = s == BoxState.Revealing || s == BoxState.Offering || s == BoxState.Teddy;
            lidOpen = Mathf.MoveTowards(lidOpen, open ? 1 : 0, (float)frameDt * UnityTuning.BoxLidSpeed);
            lid.localRotation = Quaternion.Euler(-lidOpen * 100f, 0, 0);
            // The gun: rises over the reveal, cycles like a reel slowing down, settles on the offer, waits, sinks as the
            // offer runs out.
            string show = null;
            if (s == BoxState.Revealing)
            {
                if (lastState != BoxState.Revealing)
                {
                    reelClock = 0;
                    reelNext = 0;
                }
                reelClock += frameDt;
                if (reelClock >= reelNext)
                {
                    reelGun = box.DisplayWeapon;
                    double p = box.RevealProgress;
                    reelNext = reelClock + box.cycleTime * (1 + (UnityTuning.BoxCycleSlowest - 1) * p * p);
                    popLeft = UnityTuning.BoxCyclePopTime;
                }
                show = reelGun;
            }
            else if (s == BoxState.Offering)
            {
                show = box.DisplayWeapon;
                if (lastState == BoxState.Revealing) settleLeft = UnityTuning.BoxSettleTime; // the result lands
            }
            lastState = s;
            if (show != shownHolo)
            {
                if (shownHolo != null && holos.TryGetValue(shownHolo, out GameObject old) && old != null) old.SetActive(false);
                if (shownHolo != null && holoMats.TryGetValue(shownHolo, out Material oldMat) && oldMat != null) oldMat.SetColor(BaseColorId, holoColor);
                shownFlash = -1; // the new one gets the flash written
                shownHolo = show;
                GameObject now = show != null ? Holo(show) : null;
                if (now != null) now.SetActive(true);
            }
            popLeft = Mathf.Max(0, popLeft - (float)frameDt);
            settleLeft = Mathf.Max(0, settleLeft - (float)frameDt);
            float pop = 1 + (UnityTuning.BoxCyclePop - 1) * (popLeft / UnityTuning.BoxCyclePopTime) + (UnityTuning.BoxSettlePop - 1) * (settleLeft / UnityTuning.BoxSettleTime);
            holoRoot.localScale = new Vector3(pop, pop, pop);
            float flash = 1 + UnityTuning.BoxSettleFlash * (settleLeft / UnityTuning.BoxSettleTime);
            if (shownHolo != null && holoMats.TryGetValue(shownHolo, out Material hm) && hm != null && flash != shownFlash)
            {
                shownFlash = flash;
                Color hc = holoColor * flash;
                hc.a = holoColor.a;
                hm.SetColor(BaseColorId, hc);
            }
            float rise = s == BoxState.Revealing ? (float)box.RevealProgress : s == BoxState.Offering ? Mathf.Clamp01((float)(box.OfferLeft / UnityTuning.BoxSinkTime)) : 0;
            holoRoot.localPosition = new Vector3(0, UnityTuning.BoxHeight + rise * UnityTuning.BoxRise, 0);
            holoRoot.localRotation = Quaternion.Euler(0, time * 40f, 0);
            teddy.gameObject.SetActive(s == BoxState.Teddy);
            if (s == BoxState.Teddy)
            {
                teddy.localPosition = new Vector3(0, UnityTuning.BoxHeight + (float)box.PhaseProgress * UnityTuning.BoxRise, 0);
                teddy.localRotation = Quaternion.Euler(0, 180 + Mathf.Sin(time * 6) * 15, 0);
            }
        }
    }
}
