// NIGHT37 builder S (stag37): THE STAG's body and its fight as Stag37View draws them (the canal: Stag37View.cs).
//  - THE BODY: a long-legged ice beast built from blocks: a dark torso (rule 8: a silhouette with no light) with red-orange rim strips (an enemy),
//    a long neck, the head and a CROWN of antlers (tines that branch), four long legs of two segments each in a gallop cycle driven by the
//    distance it runs. The HIND legs carry pale bands (the target, "its legs readable as targets"); they turn CYAN while you are close enough
//    to punch them (your window to act). In the CYAN window (stagger empty) it lies on its side, skidding, and K's Boss37Glow lights it.
//  - THE TELLS (builder P's yellow / red, one colour per answer): shard kick RED on the hind hooves + the RED lane strip behind it, then the
//    shards; ice chunk YELLOW in the antlers, then the yellow chunk (white on its way back); lane cut RED on the antlers + a RED stripe across your
//    lane behind it, then the red-orange barrier; kick back RED hooves; the antler charge YELLOW antlers, brighter through the wind-up.
//  - THE SLIPSTREAM: a cyan streak on the ice from its hind legs (or the pacer) to you while it pushes you.
//  - THE PACER (2.9): a small cyan skater.
//  - Boss37StaggerBar.Show(brain) every frame of the fight (K's helper).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Stag37View
    {
        Transform bodyRoot, tilt, torso, neck, head, crown, pacerT;
        readonly Transform[] hip = new Transform[4], knee = new Transform[4];
        readonly List<Renderer> rimRs = new List<Renderer>(), antlerRs = new List<Renderer>(), hoofRs = new List<Renderer>(), bandRs = new List<Renderer>();
        Renderer stripR, cutTellR, chunkR, streakR, pacerR;
        Transform stripT, cutTellT, chunkT, streakT;
        readonly List<Transform> shardTs = new List<Transform>();
        readonly List<Renderer> shardRs = new List<Renderer>();
        readonly List<Transform> cutTs = new List<Transform>();
        readonly List<Renderer> cutRs = new List<Renderer>();
        Material bodyMat, bodyDark;
        Boss37Glow glow;
        float gait, tiltNow, hitFlash;
        /// <summary>Frames the reel reads: the pose name, the hind bands cyan now, the streak shown, the shards / chunk / barriers drawn.</summary>
        public string Pose = "";
        public bool BandsCyan, StreakShown, ChunkShown;
        public int ShardsShown, CutsShown;
        public float GlowLevel => glow != null ? glow.Level : 0;

        partial void InitFight()
        {
            bodyMat = Opaque("stag37 body", UnityTuning.Stag37BodyHex);
            bodyDark = Opaque("stag37 body dark", UnityTuning.Stag37BodyDarkHex);
            BuildBody();
            stripR = Glow(fxRoot, "shard lane strip", boxMesh, out stripT);
            cutTellR = Glow(fxRoot, "lane cut stripe", boxMesh, out cutTellT);
            chunkR = Glow(fxRoot, "ice chunk", boxMesh, out chunkT);
            streakR = Glow(fxRoot, "slipstream streak", boxMesh, out streakT);
            for (int i = 0; i < UnityTuning.Stag37Shards; i++) { shardRs.Add(Glow(fxRoot, "shard", boxMesh, out Transform st)); shardTs.Add(st); }
            var pg = new GameObject("pacer");
            pg.transform.SetParent(root, false);
            pacerT = pg.transform;
            pacerR = Glow(pacerT, "pacer body", boxMesh, out Transform pb);
            pb.localPosition = new Vector3(0, 1.3f, 0);
            pb.localScale = new Vector3(0.7f, 0.9f, 1.8f);
            Renderer ph = Glow(pacerT, "pacer head", boxMesh, out Transform pht);
            pht.localPosition = new Vector3(0, 2.0f, 1.0f);
            pht.localScale = new Vector3(0.4f, 0.4f, 0.6f);
            Tint(ph, Look.Hex(UnityTuning.Canal37TrimHex, UnityTuning.Stag37PacerGain));
            pacerT.gameObject.SetActive(false);
            game.events.On<BossEvent>("bossHit", e => { if (e.boss == "stag37") hitFlash = 0.08f; });
        }

        Transform Part(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = size;
            g.AddComponent<MeshFilter>().sharedMesh = boxMesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.On;
            return g.transform;
        }

        Transform Pivot(Transform parent, string name, Vector3 pos)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            return g.transform;
        }

        Renderer GlowPart(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            Renderer r = Glow(parent, name, boxMesh, out Transform t);
            t.localPosition = pos;
            t.localScale = size;
            return r;
        }

        void BuildBody()
        {
            // Unity local axes: +z = the way it faces, +y up. Sizes in metres (UnityTuning.Stag37*: render only; the sim's body is design.n37.stag37.body).
            bodyRoot = new GameObject("the stag").transform;
            bodyRoot.SetParent(fxRoot, false);
            tilt = Pivot(bodyRoot, "tilt", Vector3.zero);
            float legH = UnityTuning.Stag37LegLength, torsoY = legH * 2f + 0.15f;
            torso = Part(tilt, "torso", new Vector3(0, torsoY, 0), new Vector3(1.0f, 1.1f, 2.6f), bodyMat);
            Part(tilt, "chest", new Vector3(0, torsoY + 0.15f, 0.9f), new Vector3(1.15f, 1.3f, 0.9f), bodyMat);
            rimRs.Add(GlowPart(tilt, "rim L", new Vector3(-0.52f, torsoY + 0.35f, 0), new Vector3(0.04f, 0.08f, 2.4f)));
            rimRs.Add(GlowPart(tilt, "rim R", new Vector3(0.52f, torsoY + 0.35f, 0), new Vector3(0.04f, 0.08f, 2.4f)));
            neck = Pivot(tilt, "neck", new Vector3(0, torsoY + 0.4f, 1.25f));
            Part(neck, "neck", new Vector3(0, 0.55f, 0.25f), new Vector3(0.5f, 1.3f, 0.5f), bodyMat).localRotation = Quaternion.Euler(25, 0, 0);
            head = Pivot(neck, "head", new Vector3(0, 1.2f, 0.55f));
            Part(head, "skull", new Vector3(0, 0, 0.25f), new Vector3(0.45f, 0.45f, 0.9f), bodyDark);
            rimRs.Add(GlowPart(head, "eyes", new Vector3(0, 0.08f, 0.45f), new Vector3(0.5f, 0.06f, 0.06f)));
            crown = Pivot(head, "antlers", new Vector3(0, 0.2f, 0));
            foreach (int side in new[] { -1, 1 })
            {
                Transform beam = Pivot(crown, "beam", new Vector3(side * 0.15f, 0, 0));
                beam.localRotation = Quaternion.Euler(-20, side * 25, side * -35);
                antlerRs.Add(GlowPart(beam, "beam", new Vector3(0, 0.8f, 0), new Vector3(0.09f, 1.6f, 0.09f)));
                for (int k = 0; k < 3; k++)
                {
                    Transform tine = Pivot(beam, "tine", new Vector3(0, 0.4f + k * 0.45f, 0));
                    tine.localRotation = Quaternion.Euler(-50 + k * 10, 0, side * (30 + k * 10));
                    antlerRs.Add(GlowPart(tine, "tine", new Vector3(0, 0.3f, 0), new Vector3(0.06f, 0.6f, 0.06f)));
                }
            }
            // legs: 0 front left, 1 front right, 2 hind left, 3 hind right
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -0.35f : 0.35f), z = i < 2 ? 1.0f : -1.0f;
                hip[i] = Pivot(tilt, "hip", new Vector3(x, torsoY - 0.3f, z));
                Part(hip[i], "thigh", new Vector3(0, -legH * 0.5f, 0), new Vector3(0.22f, legH, 0.22f), bodyMat);
                knee[i] = Pivot(hip[i], "knee", new Vector3(0, -legH, 0));
                Part(knee[i], "shin", new Vector3(0, -legH * 0.5f, 0), new Vector3(0.14f, legH, 0.14f), bodyDark);
                Renderer hoof = GlowPart(knee[i], "hoof", new Vector3(0, -legH + 0.05f, 0.04f), new Vector3(0.22f, 0.12f, 0.3f));
                hoofRs.Add(hoof);
                if (i >= 2) bandRs.Add(GlowPart(knee[i], "hind band", new Vector3(0, -0.1f, 0), new Vector3(0.2f, 0.18f, 0.2f)));
            }
            glow = new Boss37Glow(game, torso, UnityTuning.Stag37GlowRadius);
            bodyRoot.gameObject.SetActive(false);
        }

        partial void ClearFight()
        {
            if (bodyRoot != null) bodyRoot.gameObject.SetActive(false);
            if (pacerT != null) pacerT.gameObject.SetActive(false);
            Pose = "";
        }

        static float Gain(double progress)
        {
            float g = Mathf.Lerp(UnityTuning.Tell36FlashStart, UnityTuning.Tell36FlashPeak, (float)progress);
            if (progress > 0.7) g *= Light54.TellOn ? Light54.TellRamp(progress) : 0.75f + 0.25f * Mathf.Sin(Time.time * UnityTuning.Tell36PulseHz * Mathf.PI * 2); // NIGHT54 light54: one brighten, not the 7 Hz pulse
            return g;
        }

        void Strip(Transform t, Renderer r, Stag37Arena a, Vec3 o, double s0, double s1, double lat, double width, Color c)
        {
            Vec3 p0 = a.PointAt(s0, lat) + o, p1 = a.PointAt(s1, lat) + o;
            Vector3 u0 = U(p0), u1 = U(p1);
            t.gameObject.SetActive(true);
            t.position = (u0 + u1) * 0.5f + new Vector3(0, UnityTuning.Stag37FxLift, 0);
            Vector3 d = u1 - u0;
            t.rotation = d.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(d, Vector3.up) : Quaternion.identity;
            t.localScale = new Vector3((float)width, 0.02f, Mathf.Max(0.1f, d.magnitude));
            Tint(r, c);
        }

        partial void RenderFight(SurvivalSim sim, double alpha, float dt)
        {
            Stag37Arena a = sim.stag37Arena;
            Vec3 o = sim.stag37Centre.Value;
            // the pacer
            var pc = sim.stag37Pacer;
            pacerT.gameObject.SetActive(pc != null && pc.phase == 1);
            if (pc != null && pc.phase == 1)
            {
                pacerT.position = Vector3.Lerp(U(pc.prev), U(pc.pos), (float)alpha);
                Vector3 d = U(pc.to) - U(pc.from);
                if (d.sqrMagnitude > 1e-6f) pacerT.rotation = Quaternion.LookRotation(d, Vector3.up);
                Tint(pacerR, Look.Hex(UnityTuning.Canal37TrimHex, UnityTuning.Stag37PacerGain));
            }
            Stag37 g = sim.stag37;
            bool fight = g != null && g.Alive;
            bodyRoot.gameObject.SetActive(fight || (g != null && g.brain.state == Boss37State.Dying));
            // the slipstream streak (the Stag's or the pacer's)
            StreakShown = sim.Stag37Slipping;
            streakT.gameObject.SetActive(StreakShown);
            if (StreakShown)
            {
                Vec3 from = fight && g.InSlip ? g.Hind : pc != null ? pc.pos : sim.mover.position;
                Vector3 u0 = U(new Vec3(from.x, o.y, from.z)), u1 = U(new Vec3(sim.mover.position.x, o.y, sim.mover.position.z));
                Vector3 d = u1 - u0;
                streakT.position = (u0 + u1) * 0.5f + new Vector3(0, UnityTuning.Stag37FxLift * 2, 0);
                streakT.rotation = d.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(d, Vector3.up) : Quaternion.identity;
                streakT.localScale = new Vector3(UnityTuning.Stag37StreakWidth, 0.02f, Mathf.Max(0.1f, d.magnitude));
                Tint(streakR, Look.Hex(UnityTuning.Canal37TrimHex, UnityTuning.Stag37StreakGain * (0.8f + 0.2f * Mathf.Sin(t * 20f))));
            }
            if (g == null || !bodyRoot.gameObject.activeSelf)
            {
                stripT.gameObject.SetActive(false); cutTellT.gameObject.SetActive(false); chunkT.gameObject.SetActive(false);
                foreach (Transform st in shardTs) st.gameObject.SetActive(false);
                foreach (Transform ct in cutTs) ct.gameObject.SetActive(false);
                Pose = ""; BandsCyan = ChunkShown = false; ShardsShown = CutsShown = 0;
                return;
            }
            Boss37Brain b = g.brain;
            Boss37StaggerBar.Show(b);
            glow.Render(b, dt);
            string atk = g.AttackId;
            bool tell = b.state == Boss37State.Tell, act = b.state == Boss37State.Active;
            double prog = b.Progress;
            // the body's place and gait
            Vector3 feet = Vector3.Lerp(U(g.body.previous), U(g.body.position), (float)alpha);
            bodyRoot.position = feet;
            Vector3 face = N(g.Facing);
            if (face.sqrMagnitude > 1e-6f) bodyRoot.rotation = Quaternion.Slerp(bodyRoot.rotation, Quaternion.LookRotation(face, Vector3.up), Mathf.Min(1, dt * 14));
            gait += (float)g.speed * dt / UnityTuning.Stag37Stride;
            bool fallen = b.Open;
            tiltNow = Mathf.MoveTowards(tiltNow, fallen ? UnityTuning.Stag37FallRoll : 0, dt * 260);
            tilt.localRotation = Quaternion.Euler(0, 0, tiltNow);
            float run = Mathf.Clamp01((float)g.speed / 12f);
            for (int i = 0; i < 4; i++)
            {
                float ph = gait * Mathf.PI * 2 + (i < 2 ? 0 : Mathf.PI) + (i % 2) * 0.6f;
                float swing = Mathf.Sin(ph) * 38f * run, bend = (Mathf.Max(0, -Mathf.Cos(ph)) * 55f) * run;
                if (atk == Stag37.KickBack && act && i >= 2) { swing = -70; bend = 0; }
                if (atk == Stag37.ShardKick && act && i >= 2) { swing = -45 + Mathf.Sin(t * 40) * 20; bend = 10; }
                if (fallen) { swing = 20 + i * 8; bend = 30; }
                hip[i].localRotation = Quaternion.Euler(swing, 0, 0);
                knee[i].localRotation = Quaternion.Euler(i < 2 ? -bend : bend, 0, 0);
            }
            // the head: down for the charge, up and back for the chunk's lob
            float headPitch = atk == Stag37.AntlerCharge && (tell || act) ? 45 : atk == Stag37.IceChunk && tell ? -30 : atk == Stag37.LaneCut && tell ? 25 : 0;
            neck.localRotation = Quaternion.Slerp(neck.localRotation, Quaternion.Euler(headPitch, 0, 0), Mathf.Min(1, dt * 10));
            Pose = fallen ? "fallen" : b.state == Boss37State.Reveal ? "reveal" : atk != null && (tell || act) ? atk + (tell ? " tell" : "") : g.leg3 != Stag37Leg3.None ? "leg3 " + g.leg3 : "run";
            // colours: the rim red-orange (an enemy), flashes on a hit; antlers / hooves take the tell's colour
            hitFlash = Mathf.Max(0, hitFlash - dt);
            Color rim = hitFlash > 0 ? Color.white * 2f : Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Stag37RimGain);
            foreach (Renderer r in rimRs) Tint(r, rim);
            Color yellow = Look.Hex(UnityTuning.Tell36YellowHex, Gain(prog)), red = Look.Hex(UnityTuning.Tell36RedHex, Gain(prog));
            Color antler = Look.Hex(UnityTuning.Stag37AntlerHex, UnityTuning.Stag37AntlerGain);
            if ((atk == Stag37.AntlerCharge || atk == Stag37.IceChunk) && (tell || act)) antler = act ? Look.Hex(UnityTuning.Tell36YellowHex, UnityTuning.Tell36FlashPeak) : yellow;
            else if (atk == Stag37.LaneCut && (tell || act)) antler = red;
            foreach (Renderer r in antlerRs) Tint(r, antler);
            Color hoof = Look.Hex(UnityTuning.Stag37AntlerHex, UnityTuning.Stag37HoofGain);
            if ((atk == Stag37.ShardKick || atk == Stag37.KickBack) && (tell || act)) hoof = act ? Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Tell36FlashPeak) : red;
            foreach (Renderer r in hoofRs) Tint(r, hoof);
            // the hind bands: pale; CYAN while you are close enough to punch them
            Vec3 me = sim.mover.position;
            BandsCyan = fight && !b.Open && g.leg3 == Stag37Leg3.None && Vec3.DistanceTo(new Vec3(me.x, me.y + 1, me.z), g.Hind) <= g.p.catchRange && g.gap > -1.5;
            Color band = BandsCyan ? Look.Hex(UnityTuning.Canal37TrimHex, UnityTuning.Stag37BandCyanGain) : Look.Hex(UnityTuning.Canal37LaneHex, UnityTuning.Stag37BandGain);
            foreach (Renderer r in bandRs) Tint(r, band);
            // the shard kick: the RED lane strip behind it through the wind-up, then the shards skimming down it
            bool shardTell = atk == Stag37.ShardKick && tell, shardAct = atk == Stag37.ShardKick && act;
            float warn = Warn38(sim); // NIGHT38 S2 stag38: the RED lane warnings brighter (1 with the row off)
            if (shardTell) Strip(stripT, stripR, a, o, g.s - g.p.hindBack - g.p.shardRange, g.s - g.p.hindBack, a.LaneLat(a.LaneOf(g.lat)), a.p.laneWidth * 0.9, Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Stag37StripGain * Gain(prog) * warn));
            else if (shardAct) Strip(stripT, stripR, a, o, g.shardS0, g.shardS1, a.LaneLat(g.shardLane), a.p.laneWidth * 0.9, Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Stag37StripGain * warn));
            else stripT.gameObject.SetActive(false);
            ShardsShown = 0;
            for (int i = 0; i < shardTs.Count; i++)
            {
                bool on = shardAct;
                shardTs[i].gameObject.SetActive(on);
                if (!on) continue;
                ShardsShown++;
                double u = ((i * 0.37 + prog * 1.7) % 1.0);
                double sl = g.shardS1 - u * g.p.shardRange, la = a.LaneLat(g.shardLane) + ((i * 0.61) % 1.0 - 0.5) * a.p.laneWidth * 0.8;
                Vec3 q = a.PointAt(sl, la) + o + new Vec3(0, 0.3 + ((i * 0.43) % 1.0) * 1.0, 0);
                shardTs[i].position = U(q);
                shardTs[i].rotation = Quaternion.Euler(i * 37 + t * 300, i * 53, i * 71);
                shardTs[i].localScale = Vector3.one * UnityTuning.Stag37ShardSize;
                Tint(shardRs[i], Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Stag37ShardGain));
            }
            // the lane cut: a RED stripe across your lane behind it in the wind-up; the barriers
            if (atk == Stag37.LaneCut && tell)
            {
                double cs = g.s - g.p.hindBack - g.p.cutBehind, cl = a.LaneLat(g.playerLane);
                Strip(cutTellT, cutTellR, a, o, cs - g.p.cutThick, cs + g.p.cutThick, cl, a.p.laneWidth * 0.9, Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Stag37StripGain * 1.5f * Gain(prog) * warn));
            }
            else cutTellT.gameObject.SetActive(false);
            while (cutTs.Count < sim.stag37Cuts.Count) { cutRs.Add(Glow(fxRoot, "barrier", boxMesh, out Transform ct)); cutTs.Add(ct); }
            CutsShown = sim.stag37Cuts.Count;
            for (int i = 0; i < cutTs.Count; i++)
            {
                bool on = i < sim.stag37Cuts.Count;
                cutTs[i].gameObject.SetActive(on);
                if (!on) continue;
                var (lo, hi, _) = sim.stag37Cuts[i];
                Vector3 a0 = U(lo + o), a1 = U(hi + o);
                cutTs[i].position = (a0 + a1) * 0.5f;
                cutTs[i].rotation = Quaternion.identity;
                cutTs[i].localScale = new Vector3(Mathf.Abs(a1.x - a0.x), Mathf.Abs(a1.y - a0.y), Mathf.Abs(a1.z - a0.z));
                Tint(cutRs[i], Look.Hex(UnityTuning.Canal37DangerHex, UnityTuning.Stag37BarrierGain * warn));
            }
            // the ice chunk: yellow on its way to you, white on its way back
            ChunkShown = g.chunk.alive;
            chunkT.gameObject.SetActive(ChunkShown);
            if (ChunkShown)
            {
                chunkT.position = Vector3.Lerp(U(g.chunk.prev), U(g.chunk.pos), (float)alpha);
                chunkT.rotation = Quaternion.Euler(t * 200, t * 140, 0);
                chunkT.localScale = Vector3.one * (float)(g.p.chunkRadius * 1.6);
                Tint(chunkR, g.chunk.back ? Color.white * UnityTuning.Stag37ChunkBackGain : Look.Hex(UnityTuning.Tell36YellowHex, UnityTuning.Stag37ChunkGain));
            }
        }
    }
}
