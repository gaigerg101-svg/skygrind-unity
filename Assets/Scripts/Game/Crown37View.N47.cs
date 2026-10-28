// NIGHT47 builder L (crown47's attacks on screen; NIGHT47.md G41, G43 boss part "the flame ... has weight"; L-cards.md section 5, the boss flame spec;
// DESIGN-RULES E-4): THE CROWN's CROWN FIRE and BELL TOLL on Crown37View (view only; the rules are Core/Enemies/Crown37.N47.cs). Drawn only while the
// fight runs with crown47 (c.crown47: the sim row's flag for this fight; off: nothing here shows, Crown37View is NIGHT46's).
//  - THE TELL (CROWN FIRE, 1.3 s): a white-hot glow grows at the ring's centre (the nozzle: a 0 -> full ramp), and a red-orange line burns along the
//    floor across the arc the cone will sweep, at the flame's reach, as wide as the cone is there (Riot: never more room than the attack gives),
//    drawn from its start edge to its end edge as the wind-up runs (it says where and which way).
//  - THE FLAME (its sweep): a cone of FireSegs stacked flame layers along the cone's direction, each as wide as the cone at its distance, coloured
//    from white-hot at the nozzle through yellow and orange to red-orange at the tips (the enemy family), additive, each layer flickering on its own
//    slow curve (a cosmetic hash of time: never the sim's stream); embers (FireEmbers small sparks thrown along the cone, falling); a flickering
//    point light at the cone's middle (a slow, low-amplitude curve: G11's lava rule); the ring pressure-kicks back FireKickM on the sweep's first
//    frame (a 0.15 s spring) and shudders while it pours.
//  - THE VENT (its recover, the punish): the nozzle's glow dims to an ember pulse; the scorch marks it left on the floor fade over ScorchSeconds.
//  - BELL TOLL: through its tell the ring rocks (the nozzle glow red-orange); its wave is a ring of TollSegs red-orange segments at the wave's radius on
//    the floor (the height to hop).
// EFFECTS INTENSITY scales every gain; REDUCED MOTION stills the flicker and the shudder (a flat flame). Numbers: UnityTuning.Crown47.cs.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        Transform c47Root, nozzleT;
        Renderer nozzleR;
        readonly List<Transform> flameT = new List<Transform>(), lineT = new List<Transform>(), tollT = new List<Transform>(), emberT = new List<Transform>(), scorchT = new List<Transform>();
        readonly List<Renderer> flameR = new List<Renderer>(), lineR = new List<Renderer>(), tollR = new List<Renderer>(), emberR = new List<Renderer>();
        readonly float[] scorchAge = new float[UnityTuning.Crown47ScorchMax];
        int scorchNext;
        float scorchClock;
        Light fireLight;
        Material scorchMat;
        float kickT = 99f;
        // round 2 (the lead's n47-r0d frame: "a FLAT YELLOW BOX BEAM, hard edges, one colour"): the flame is soft textured sprites now
        FxSprites fire47, spark47, haze47;
        Fx43Shade smoke47;
        bool wasLive;

        /// <summary>For the reels / frames (crown47): the flame's layers drawn, the floor line's segments, the toll's segments, the scorches.</summary>
        public static int FlameShown47, LineShown47, TollShown47, ScorchShown47;
        /// <summary>The blind pair's two looks (NIGHT47 G43: "a blind pair between two looks"): 0 = A, the body (the dense cone + its continuous
        /// core + the floor lick: round 2b); 1 = B, a particle cone (the puffs alone, sparser and larger, no core streaks, no floor lick). The game
        /// draws A; the a547 reel takes the sweep frame in both.</summary>
        public static int FlameVariant47;

        void Crown47Build()
        {
            flameT.Clear(); flameR.Clear(); lineT.Clear(); lineR.Clear(); tollT.Clear(); tollR.Clear(); emberT.Clear(); emberR.Clear(); scorchT.Clear(); // a re-attach: the old root went with its objects
            c47Root = new GameObject("crown47").transform;
            c47Root.SetParent(root, false);
            nozzleT = Box(c47Root, "crown47 nozzle", addMat, Vector3.zero, Vector3.one, out nozzleR);
            for (int i = 0; i < UnityTuning.Crown47LineSegs; i++) { lineT.Add(Box(c47Root, "crown47 line " + i, addMat, Vector3.zero, Vector3.one, out Renderer r)); lineR.Add(r); }
            for (int i = 0; i < UnityTuning.Crown47TollSegs; i++) { tollT.Add(Box(c47Root, "crown47 toll " + i, addMat, Vector3.zero, Vector3.one, out Renderer r)); tollR.Add(r); }
            int cap = UnityTuning.Crown47FirePuffs * (2 + UnityTuning.Crown47FireRing) + UnityTuning.Crown47PoolPuffs + 8;
            fire47 = new FxSprites(game.mats.neon, UnityTuning.KenneyFlare, cap);
            spark47 = new FxSprites(game.mats.neon, UnityTuning.KenneyTrace, UnityTuning.Crown47FireEmbers + UnityTuning.Crown47CoreStreaks + 4);
            haze47 = new FxSprites(game.mats.neon, UnityTuning.KenneySmoke, UnityTuning.Crown47HazePuffs + 4);
            smoke47 = new Fx43Shade(game.mats.neon, "crown47 smoke", UnityTuning.Crown47SmokePuffs + 4, Look.Hex(UnityTuning.Torch43SmokeHex), UnityTuning.Crown47SmokeDark);
            for (int i = 0; i <= 8; i++) { fire47.Warm(FlameColor(i / 8f)); spark47.Warm(FlameColor(i / 8f)); }
            haze47.Warm(Look.Hex(UnityTuning.Crown47HazeHex));
            scorchMat = FxDraw.Opaque(game.mats.neon, "crown47 scorch", Look.Hex(UnityTuning.Crown47ScorchHex));
            for (int i = 0; i < UnityTuning.Crown47ScorchMax; i++) { scorchT.Add(Box(c47Root, "crown47 scorch " + i, scorchMat, Vector3.zero, Vector3.one, out _)); scorchAge[i] = 99f; }
            var lg = new GameObject("crown47 fire light");
            lg.transform.SetParent(c47Root, false);
            fireLight = lg.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.shadows = LightShadows.None;
            fireLight.range = UnityTuning.Crown47LightRange;
            fireLight.color = Look.Hex(UnityTuning.Crown47OrangeHex);
            fireLight.enabled = false;
            Crown47Hide();
        }

        void Crown47Hide()
        {
            if (c47Root == null) return;
            nozzleT.gameObject.SetActive(false);
            foreach (Transform t in lineT) t.gameObject.SetActive(false);
            foreach (Transform t in tollT) t.gameObject.SetActive(false);
            foreach (Transform t in scorchT) t.gameObject.SetActive(false);
            for (int i = 0; i < scorchAge.Length; i++) scorchAge[i] = 99f;
            fireLight.enabled = false;
            FlameShown47 = LineShown47 = TollShown47 = ScorchShown47 = 0;
        }

        static float Hash47(float x) { float s = Mathf.Sin(x * 12.9898f) * 43758.5453f; return s - Mathf.Floor(s); }

        static Color FlameColor(float f)
        {
            // 0 at the nozzle (white-hot) -> yellow -> orange -> red-orange at the tips (the enemy family; no cyan, no magenta)
            Color w = Look.Hex(UnityTuning.Crown47WhiteHex), y = BossColours63.Part(SurvivalGame.Instance != null ? SurvivalGame.Instance.cfg : null, "crown.flameYellow", Look.Hex(UnityTuning.Crown47YellowHex)), o = Look.Hex(UnityTuning.Crown47OrangeHex), r = Look.Hex(UnityTuning.Crown37DangerHex); // NIGHT63 B3: the flame's yellow to danger with bossColours63
            if (f < 0.2f) return Color.Lerp(w, y, f / 0.2f);
            if (f < 0.55f) return Color.Lerp(y, o, (f - 0.2f) / 0.35f);
            return Color.Lerp(o, r, (f - 0.55f) / 0.45f);
        }

        /// <summary>Every frame of a live Crown, after Crown40Draw (DrawCrown).</summary>
        void Crown47Draw(Crown37 c, SurvivalSim sim, float now)
        {
            CrownLook47Draw(c, sim, now); // NIGHT47 L (crownLook47, a view row: Crown37View.Look47.cs): the crown's points, clapper and springs
            if (!c.crown47 || c.p47 == null) { Crown47Hide(); return; }
            if (c47Root == null) Crown47Build();
            float fx = Mathf.Max(0f, FxIntensity47()), dt = Time.deltaTime;
            bool still = ReducedMotion47();
            Crown47Params p = c.p47;
            Boss37Brain br = c.brain;
            Boss37Attack a = br.Current;
            bool fireTell = c.FireTelling, live = c.FireLive, vent = c.Venting;
            float prog = (float)br.Progress;
            Vector3 centre = crownT.position;
            float floorY = (float)sim.mover.position.y + 0.04f;

            // the nozzle: white-hot ramp through the tell, full while it pours, an ember pulse through the vent, red-orange in the toll's tell
            bool tollTell = a != null && a.id == Crown37.BellToll && br.state == Boss37State.Tell;
            bool nozzle = fireTell || live || vent || tollTell;
            if (nozzleT.gameObject.activeSelf != nozzle) nozzleT.gameObject.SetActive(nozzle);
            if (nozzle)
            {
                float g = fireTell ? prog * prog : live ? 1f : vent ? 0.25f + 0.15f * Mathf.Sin(now * 5f) : 0.6f * prog;
                nozzleT.position = centre + Vector3.down * 0.4f;
                nozzleT.localScale = Vector3.one * (UnityTuning.Crown47NozzleM * (0.4f + 0.6f * g));
                Tint(nozzleR, tollTell ? danger : Look.Hex(UnityTuning.Crown47WhiteHex), UnityTuning.Crown47NozzleGain * g * fx);
            }

            // the floor line through the tell: the arc at the reach, as wide as the cone there, drawn start -> end with the wind-up
            int lines = 0;
            for (int i = 0; i < lineT.Count; i++)
            {
                float f = (i + 0.5f) / lineT.Count;
                bool show = fireTell && f <= prog + 0.05f;
                if (lineT[i].gameObject.activeSelf != show) lineT[i].gameObject.SetActive(show);
                if (!show) continue;
                lines++;
                Vector3 d = Quaternion.AngleAxis((f - 0.5f) * (float)c.fireArc47, Vector3.up) * Dir(c.fireMid47); // the arc it will sweep (Rot47's web +angle is Unity's +angle about up: worked through the z flip)
                float reach = (float)p.reachM * UnityTuning.Crown47LineAt, wide = 2f * reach * Mathf.Tan((float)p.halfDeg * Mathf.Deg2Rad);
                Vector3 at = new Vector3(centre.x, floorY, centre.z) + d * reach;
                lineT[i].position = at;
                lineT[i].rotation = Quaternion.LookRotation(d);
                lineT[i].localScale = new Vector3(wide / lineT.Count * 3f, 0.03f, UnityTuning.Crown47LineThick);
                float blink = prog > 0.75f && !still ? (Mathf.Sin(now * 12f) > 0 ? 1f : 0.4f) : 1f;
                Tint(lineR[i], danger, (UnityTuning.Crown47LineGain0 + (UnityTuning.Crown47LineGain1 - UnityTuning.Crown47LineGain0) * prog) * blink * fx);
            }
            LineShown47 = lines;

            // the flame
            if (live && !wasLive) kickT = 0f; // the pressure kick on the sweep's first frame
            wasLive = live;
            kickT += dt;
            Vector3 dir = Dir(c.fireDir);
            float kick = kickT < 0.15f ? UnityTuning.Crown47FireKickM * (1f - kickT / 0.15f) : 0f;
            if (kick > 0f && !still) crownT.position -= dir * kick;
            if (live && !still) crownT.position += new Vector3(Hash47(now * 31f) - 0.5f, Hash47(now * 17f) - 0.5f, 0f) * UnityTuning.Crown47ShudderM;
            int shown = 0;
            Transform cam = game.cam != null ? game.cam.transform : null;
            fire47?.Begin(); spark47?.Begin(); haze47?.Begin(); smoke47?.Begin();
            // round 2b (the lead's n47-r1 frame: "a thin cloud of orange embers drifting left of the ring ... it does not read as a FLAME with weight"):
            // the cone's axis runs from the clapper DOWN to the floor line (the floor point at Crown47FloorAt of the reach), so the flame pours onto
            // the floor it scorches; its body is dense overlapping soft puffs plus a continuous core of long bright streaks; where it meets the
            // floor it spreads in a pool of flame (the lick on the ground)
            Vector3 noz = centre + Vector3.down * 0.6f;
            Vector3 floorAt = new Vector3(centre.x, floorY + 0.3f, centre.z) + dir * ((float)p.reachM * UnityTuning.Crown47FloorAt);
            Vector3 axis = floorAt - noz;
            float axisL = Mathf.Max(1f, axis.magnitude);
            Vector3 ax = axis / axisL;
            if (live && cam != null && fire47 != null && fire47.Ready)
            {
                float tanH = Mathf.Tan((float)p.halfDeg * Mathf.Deg2Rad);
                Vector3 across = Vector3.Cross(ax, Vector3.up).normalized, upish = Vector3.Cross(across, ax).normalized;
                float flow = (now * UnityTuning.Crown47FlowHz) % 1f;
                int n = FlameVariant47 == 1 ? UnityTuning.Crown47FirePuffs / 2 : UnityTuning.Crown47FirePuffs;
                for (int i = 0; i < n; i++)
                {
                    float fr = (i + flow) / n;
                    float dist = Mathf.Max(0.2f, fr * axisL);
                    float w = still ? 0f : (0.1f + 0.35f * fr);
                    Vector3 at = noz + ax * dist + across * (w * Mathf.Sin(now * 7.1f + i * 1.7f) * 0.6f) + upish * (w * Mathf.Sin(now * 5.3f + i * 2.3f) * 0.4f);
                    float rad = Mathf.Max(0.25f, dist * tanH);
                    float size = (Mathf.Lerp(UnityTuning.Crown47PuffFrom, UnityTuning.Crown47PuffTo, fr) + rad * 1.6f) * (FlameVariant47 == 1 ? 1.4f : 1f);
                    float k = Mathf.Clamp01(0.35f + fr * 4f) * (1f - fr * fr * 0.6f) * fx; // bright from the nozzle, thinning toward the tips
                    float roll = now * 2.4f + i * 1.3f;
                    fire47.Billboard(at, size, roll, cam, FlameColor(fr), k);
                    fire47.Billboard(at - ax * 0.15f, size * 0.55f, -roll * 1.3f, cam, FlameColor(Mathf.Max(0f, fr - 0.35f)), Mathf.Min(1f, k * 1.4f)); // the hotter heart
                    shown += 2;
                    for (int q = 0; q < UnityTuning.Crown47FireRing; q++)
                    {
                        float ang = (q + 0.5f * (i & 1)) / UnityTuning.Crown47FireRing * Mathf.PI * 2f + now * 1.9f;
                        Vector3 rp = at + (across * Mathf.Cos(ang) + upish * Mathf.Sin(ang)) * rad * 0.8f;
                        fire47.Billboard(rp, size * 0.75f, roll + q, cam, FlameColor(Mathf.Min(1f, fr + 0.1f)), k * 0.8f);
                        shown++;
                    }
                    if (fr > 0.6f && smoke47 != null) smoke47.Billboard(at + Vector3.up * (0.5f + fr * 0.8f), size * 1.4f, roll * 0.5f, cam, (fr - 0.6f) / 0.4f * 0.55f * fx);
                }
                // the core: long bright streaks along the axis, overlapping (a continuous body from the clapper down, not puffs alone)
                if (FlameVariant47 == 0 && spark47 != null && spark47.Ready)
                    for (int j = 0; j < UnityTuning.Crown47CoreStreaks; j++)
                    {
                        float u0 = j / (float)UnityTuning.Crown47CoreStreaks, u1 = Mathf.Min(1f, u0 + UnityTuning.Crown47CoreLen);
                        float wob = still ? 0f : Mathf.Sin(now * 9f + j * 2.1f) * 0.15f;
                        Vector3 p0 = noz + ax * (u0 * axisL) + across * wob, p1 = noz + ax * (u1 * axisL) - across * wob;
                        float wd = Mathf.Max(0.5f, (u0 + u1) * 0.5f * axisL * tanH * 1.4f);
                        spark47.Streak(p0, p1, wd, cam.position, FlameColor(u0 * 0.7f), (1f - u0 * 0.6f) * fx);
                        shown++;
                    }
                // the lick on the floor: the flame spreads where it lands
                for (int q = 0; q < (FlameVariant47 == 0 ? UnityTuning.Crown47PoolPuffs : 0); q++)
                {
                    float ang = q * Mathf.PI * 2f / UnityTuning.Crown47PoolPuffs + now * 0.8f;
                    float rr = UnityTuning.Crown47PoolR * (0.6f + 0.4f * Mathf.Sin(now * 4f + q));
                    Vector3 pp = floorAt + new Vector3(Mathf.Cos(ang) * rr, 0.2f, Mathf.Sin(ang) * rr);
                    fire47.Billboard(pp, UnityTuning.Crown47PoolPuffM, now * 1.7f + q, cam, FlameColor(0.55f), 0.7f * fx);
                    shown++;
                }
                // the heat haze band: faint warm smoke-texture sprites along the cone's edge, shimmering (no screen-wide effect, never on the reticle)
                if (haze47 != null && haze47.Ready && !still)
                    for (int h = 0; h < UnityTuning.Crown47HazePuffs; h++)
                    {
                        float fr = (h + 0.5f) / UnityTuning.Crown47HazePuffs;
                        float dist = fr * axisL, rad = dist * tanH * 1.3f;
                        float side = (h & 1) == 0 ? 1f : -1f;
                        Vector3 at = noz + ax * dist + across * side * rad + Vector3.up * (0.3f * Mathf.Sin(now * 9f + h));
                        haze47.Billboard(at, 1.2f + rad, now * 0.7f + h, cam, Look.Hex(UnityTuning.Crown47HazeHex), UnityTuning.Crown47HazeK * fx);
                    }
            }
            // sparks: thrown along the cone, stretched along their speed, falling (Company of Heroes' rule: they collide with the floor and stop)
            if ((live || vent) && cam != null && spark47 != null && spark47.Ready)
            {
                Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
                for (int i = 0; i < UnityTuning.Crown47FireEmbers; i++)
                {
                    float life = (now * UnityTuning.Crown47EmberRate + Hash47(i * 3.1f)) % 1f;
                    float side = (Hash47(i * 7.3f) - 0.5f) * 2f;
                    float dist = life * (float)p.reachM * 1.15f;
                    float drop = life * life * UnityTuning.Crown47EmberFall;
                    Vector3 at = centre + dir * dist + perp * side * dist * Mathf.Tan((float)p.halfDeg * Mathf.Deg2Rad) + Vector3.down * drop;
                    if (at.y < floorY) at.y = floorY; // on the floor: it stops there
                    Vector3 vel = dir * ((float)p.reachM * 1.15f * UnityTuning.Crown47EmberRate) + Vector3.down * (2f * life * UnityTuning.Crown47EmberFall * UnityTuning.Crown47EmberRate);
                    Vector3 tail = at - vel * UnityTuning.Crown47SparkStretch;
                    spark47.Streak(tail, at, UnityTuning.Crown47EmberM, cam.position, FlameColor(0.25f + 0.6f * life), (1f - life) * fx);
                }
            }
            fire47?.Draw(); spark47?.Draw(); haze47?.Draw(); smoke47?.Draw();
            FlameShown47 = shown;
            fireLight.enabled = live || vent;
            if (fireLight.enabled)
            {
                fireLight.transform.position = centre + dir * ((float)p.reachM * 0.5f);
                float curve = still ? 1f : 0.85f + 0.15f * Mathf.Sin(now * UnityTuning.Crown47LightHz * 2f * Mathf.PI);
                fireLight.intensity = (live ? UnityTuning.Crown47LightIntensity : UnityTuning.Crown47LightIntensity * 0.25f) * curve * UnityTuning.LightScale * fx;
            }
            // scorch marks where the cone passed (a mark every ScorchEvery s while it pours, at the floor under the cone's middle), fading
            if (live)
            {
                scorchClock += dt;
                if (scorchClock >= UnityTuning.Crown47ScorchEvery)
                {
                    scorchClock = 0f;
                    int k = scorchNext++ % scorchT.Count;
                    scorchAge[k] = 0f;
                    Vector3 at = new Vector3(floorAt.x, floorY - 0.02f, floorAt.z); // under the flame's landing (round 2b)
                    scorchT[k].position = at;
                    scorchT[k].rotation = Quaternion.LookRotation(dir);
                }
            }
            int scorches = 0;
            for (int i = 0; i < scorchT.Count; i++)
            {
                scorchAge[i] += dt;
                bool show = scorchAge[i] < UnityTuning.Crown47ScorchSeconds;
                if (scorchT[i].gameObject.activeSelf != show) scorchT[i].gameObject.SetActive(show);
                if (!show) continue;
                scorches++;
                float s = UnityTuning.Crown47ScorchM * (1f - scorchAge[i] / UnityTuning.Crown47ScorchSeconds);
                scorchT[i].localScale = new Vector3(s, 0.02f, s * 1.4f);
            }
            ScorchShown47 = scorches;

            // BELL TOLL's wave
            bool toll = c.TollLive && c.tollR > 0.1;
            int ts = 0;
            for (int i = 0; i < tollT.Count; i++)
            {
                if (tollT[i].gameObject.activeSelf != toll) tollT[i].gameObject.SetActive(toll);
                if (!toll) continue;
                ts++;
                float ang = i * 2f * Mathf.PI / tollT.Count;
                float r = (float)c.tollR;
                Vector3 at = U(c.centre.x + Mathf.Cos(ang) * r, c.tollY + UnityTuning.Crown47TollH * 0.5f, c.centre.z + Mathf.Sin(ang) * r);
                tollT[i].position = at;
                tollT[i].rotation = Quaternion.Euler(0, -ang * Mathf.Rad2Deg, 0);
                tollT[i].localScale = new Vector3(UnityTuning.Crown47TollThick, UnityTuning.Crown47TollH, 2f * Mathf.PI * r / tollT.Count * 1.05f);
                Tint(tollR[i], danger, UnityTuning.Crown47TollGain * fx);
            }
            TollShown47 = ts;
        }

        static Vector3 Dir(Vec3 flat)
        {
            Vector3 v = U(flat) - U(0, 0, 0);
            v.y = 0;
            return v.sqrMagnitude > 1e-6f ? v.normalized : Vector3.forward;
        }

        /// <summary>EFFECTS INTENSITY (the game's setting, clamped to its max: as Feel46View reads it).</summary>
        float FxIntensity47()
        {
            MovementConfig mc = game.cfg.movement;
            return (float)System.Math.Max(0, System.Math.Min(mc.feel.effectsIntensityMax, mc.settings.effectsIntensity));
        }

        /// <summary>REDUCED MOTION (the game's setting).</summary>
        bool ReducedMotion47() => game.cfg.movement.settings.reducedMotion;
    }
}
