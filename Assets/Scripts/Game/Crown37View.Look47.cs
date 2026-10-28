// NIGHT47 builder L (crownLook47, a VIEW row: NightRows.ViewOn(cfg, "crownLook47"); NIGHT47.md G41 "THE CROWN rebuilt"; L-cards.md section 2
// Silhouette / Palette / Animation): THE CROWN's new look on Crown37View. View only: the sim's ring (its centre, plane, pass radius, gems) is the
// truth; nothing here moves what the pass or a shot tests.
//  - THE CROWN: eight tall bronze points standing out of the ring at its gems (a crown's points: triangles = danger), gold-edged, red-gold lit;
//    a bell's CLAPPER hanging from the ring's centre on a short chain (the flame's nozzle).
//  - THE PALETTE (level 5's stone, bronze bells, amber lamps): dark bronze points with a gold edge glow; phase 2 hot red-gold, phase 3 the points
//    glowing red-orange from inside (Gabriel's recolour).
//  - MOTION (frame-time springs): the ring ROCKS like a struck bell after every pass (a damped swing, Look47RockDeg), REARS BACK through a tell's
//    first 40 % (anticipation) and snaps forward on the strike, FLINCHES on every hit; the clapper swings with its own lag (follow-through).
//    REDUCED MOTION stills the springs (the colours kept).
// Numbers: UnityTuning.Crown47.cs (CLook47*). Row off (or -skygrindOff47): nothing is built or drawn.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        Transform clook47Root, clapperT, chainT;
        Renderer clapperR;
        readonly List<Transform> pointT = new List<Transform>(), edgeT = new List<Transform>();
        readonly List<Renderer> pointR = new List<Renderer>(), edgeR = new List<Renderer>();
        Material bronzeMat;
        float rock, rockVel, rear, rearVel, swing, swingVel;
        int lastPass47 = -100000;
        bool clookHooked;
        Boss37State lastState47;

        /// <summary>For the reels / frames (crownLook47): the new look drawn, the rock's angle now (deg).</summary>
        public static bool CrownLook47Shown;
        public static float CrownRock47;

        void CrownLook47Build(Crown37 c)
        {
            pointT.Clear(); pointR.Clear(); edgeT.Clear(); edgeR.Clear();
            clook47Root = new GameObject("crownLook47").transform;
            clook47Root.SetParent(spinT, false); // the points turn with the ring
            bronzeMat = FxDraw.Opaque(game.mats.neon, "crown47 bronze", Look.Hex(UnityTuning.CLook47BronzeHex));
            float r = (float)c.p.ringRadius;
            int n = UnityTuning.CLook47Points;
            for (int k = 0; k < n; k++)
            {
                float a = k * 2 * Mathf.PI / n;
                Vector3 dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
                float len = UnityTuning.CLook47PointLen;
                // a point: a box turned 45 deg about its long axis's normal, long along the radius: a diamond whose outer half reads as a spike
                var pt = Box(clook47Root, "crown point", bronzeMat, dir * (r + len * 0.45f), new Vector3(len, UnityTuning.CLook47PointW, UnityTuning.CLook47PointW), out Renderer pr);
                pt.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg) * Quaternion.Euler(45, 0, 0);
                pointT.Add(pt);
                pointR.Add(pr);
                var eg = Box(clook47Root, "crown point edge", addMat, dir * (r + len * 0.45f), new Vector3(len * 1.02f, UnityTuning.CLook47EdgeW, UnityTuning.CLook47EdgeW), out Renderer er);
                eg.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg);
                edgeT.Add(eg);
                edgeR.Add(er);
            }
            chainT = Box(crownT, "crown clapper chain", bronzeMat, Vector3.zero, new Vector3(0.12f, UnityTuning.CLook47ChainLen, 0.12f), out _);
            var cl = Box(crownT, "crown clapper", addMat, Vector3.zero, Vector3.one * UnityTuning.CLook47ClapperM, out clapperR);
            clapperT = cl;
            if (!clookHooked && game.events != null)
            {
                clookHooked = true;
                game.events.On<BossEvent>("bossHit", e => { if (e != null && e.boss == "crown37") rockVel += UnityTuning.CLook47FlinchKick; });
            }
        }

        static void Spring47(ref float x, ref float v, float target, float hz, float zeta, float dt)
        {
            float w = 2f * Mathf.PI * hz;
            v += (-2f * zeta * w * v - w * w * (x - target)) * dt;
            x += v * dt;
        }

        /// <summary>Every frame of a live Crown (the top of Crown47Draw).</summary>
        void CrownLook47Draw(Crown37 c, SurvivalSim sim, float now)
        {
            bool on = NightRows.ViewOn(game.cfg, "crownLook47");
            CrownLook47Shown = on;
            if (!on) { if (clook47Root != null && clook47Root.gameObject.activeSelf) { clook47Root.gameObject.SetActive(false); chainT.gameObject.SetActive(false); clapperT.gameObject.SetActive(false); } return; }
            if (clook47Root == null) CrownLook47Build(c);
            if (!clook47Root.gameObject.activeSelf) { clook47Root.gameObject.SetActive(true); chainT.gameObject.SetActive(true); clapperT.gameObject.SetActive(true); }
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            bool calm = game.cfg.movement.settings.reducedMotion;
            Boss37Brain b = c.brain;
            float prog = (float)b.Progress;
            int leg = Mathf.Clamp(c.Leg, 1, 3);
            // the springs: a pass strikes the bell (a rock), a tell rears it back, the strike snaps it
            if (c.lastPassStep != lastPass47) { if (lastPass47 > -100000) rockVel += UnityTuning.CLook47RockKick; lastPass47 = c.lastPassStep; }
            float rearTarget = b.state == Boss37State.Tell ? -UnityTuning.CLook47RearDeg * Mathf.Clamp01(prog / 0.4f) : 0f;
            if (b.state == Boss37State.Active && lastState47 == Boss37State.Tell) rearVel += UnityTuning.CLook47StrikeKick;
            lastState47 = b.state;
            if (calm) { rock = rockVel = rear = rearVel = swing = swingVel = 0; }
            else
            {
                Spring47(ref rock, ref rockVel, 0f, UnityTuning.CLook47RockHz, UnityTuning.CLook47RockZeta, dt);
                Spring47(ref rear, ref rearVel, rearTarget, UnityTuning.CLook47RearHz, UnityTuning.CLook47RockZeta, dt);
                // the clapper lags the ring's rock and rear (follow-through)
                Spring47(ref swing, ref swingVel, -(rock + rear) * 1.4f, UnityTuning.CLook47ClapperHz, 0.25f, dt);
            }
            CrownRock47 = rock;
            crownT.rotation = crownT.rotation * Quaternion.Euler(UnityTuning.CLook47TiltDeg + rear, 0, rock);
            // the clapper on its chain under the centre
            Quaternion hang = Quaternion.Euler(0, 0, swing);
            Vector3 down = crownT.InverseTransformDirection(Vector3.down);
            chainT.localPosition = hang * (down * (UnityTuning.CLook47ChainLen * 0.5f));
            chainT.localRotation = Quaternion.FromToRotation(Vector3.up, -(hang * down));
            clapperT.localPosition = hang * (down * (UnityTuning.CLook47ChainLen + UnityTuning.CLook47ClapperM * 0.5f));
            bool fire = c.FireTelling || c.FireLive;
            // NIGHT63 B3: danger with bossColours63; B3c: after the clapper's gain, clip-safe (the white fire flash untouched). Row off: the gold x the gain, as before
            if (fire) Tint(clapperR, Look.Hex(UnityTuning.Crown47WhiteHex), 2.4f);
            else Tint(clapperR, BossColours63.Hot(game.cfg, "crown.clap", Look.Hex(leg >= 2 ? UnityTuning.CLook47RedGoldHex : UnityTuning.CLook47GoldHex), 1.2f), 1f);
            // the points' colours: bronze with a gold edge; hot red-gold from phase 2; red-orange from inside in phase 3; cyan in the window
            // NIGHT63 B3: danger with bossColours63; B3c: after the edge's tell gain (up to x4.2), clip-safe, phase 3's red-orange too (the GATE's amber
            // ring blocks); the window's cyan untouched. Row off: each edge's colour x its gain, as before
            Color edgeSrc = leg >= 3 ? danger : leg >= 2 ? Look.Hex(UnityTuning.CLook47RedGoldHex) : Look.Hex(UnityTuning.CLook47GoldHex);
            float eg = (b.state == Boss37State.Tell ? 1.2f + prog : 1f) * UnityTuning.CLook47EdgeGain;
            int gemsLeft = c.GemsLeft;
            for (int k = 0; k < pointR.Count; k++)
            {
                bool alive = k < Mathf.CeilToInt(gemsLeft * pointR.Count / (float)Mathf.Max(1, c.p.gems)); // the points dim as the gems go (the bar on the crown itself)
                Tint(pointR[k], leg >= 3 ? Color.Lerp(Look.Hex(UnityTuning.CLook47BronzeHex), danger, 0.35f) : Look.Hex(UnityTuning.CLook47BronzeHex), 1f);
                if (c.Open) Tint(edgeR[k], cyan, alive ? eg : eg * 0.25f);
                else Tint(edgeR[k], BossColours63.Hot(game.cfg, "crown.edge", edgeSrc, alive ? eg : eg * 0.25f), 1f);
            }
        }
    }
}
