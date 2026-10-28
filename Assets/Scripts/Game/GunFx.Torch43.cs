// NIGHT43 builder F1 (design.n43.feel43, a view row: no sim bit; G20 GG: the flamethrower "needs a better visual"). Round 1 drew only per-tick
// puffs, and the lead's n43-r1 frames showed "almost no visible flame" after 1 s: the flame runs straight down the aim, and round 1 dimmed every
// sprite near the crosshair (banned 16's spray rule), so the whole jet was cleared; additive cyan on level 2's pale ice washed out the rest.
// Round 2: the TORCH looks like a flamethrower, drawn ON TOP of NIGHT20's spray (GunFx.Spray20.cs, unchanged):
//   - THE JET: every frame while it fires (a flame tick within Torch43HoldGap), a continuous cone of soft flame billboards from the muzzle to
//     the reach (the wall it licks, or its 7 m), white-hot at the muzzle -> gold -> orange -> a dark smoke tail, the puffs flowing outward
//     (Torch43FlowHz) and rolling, wobbling off the line, growing with distance (frame time; nothing pooled per frame: it is redrawn);
//   - THE LICK: each tick, flame puffs burst at the reach and spread sideways over the target / wall, with embers;
//   - SMOKE: dark multiply puffs (Fx43Shade) along the jet's far third and rising off the target: value contrast on any floor;
//   - A LIGHT at the reach (LightPools.Flash, warm, flickering 0.8-1.2x), so the target and the walls glow;
//   - the ramp: the first Torch43RampTime of a hold grows from 50 % to full.
// Colour (Torch43Warm): the lead asked for a white-hot -> orange -> smoke flame (n43-r1). CLAUDE.md rule 8 gives red-orange to enemies and danger;
// a flame is danger, the player's own: the jet keeps a white-gold heart and only its far end runs orange. Torch43Warm false: NIGHT20's cyan ladder.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class TorchFeel43
    {
        readonly SurvivalGame game;
        readonly Fx43Pool pool;
        Mulberry32 rng;
        bool built;
        Color white, gold, orange, deep, cyanHot, cyan, cyanPale;
        FxSprites jet;
        Fx43Shade smoke;
        double holdStart = double.NegativeInfinity, lastTick = double.NegativeInfinity;
        double flow; // the jet's flow phase (0..1)
        float reach = 7; // m: the last tick's reach
        Vector3 reachAt;

        public TorchFeel43(SurvivalGame game)
        {
            this.game = game;
            pool = new Fx43Pool(game, UnityTuning.Torch43Pool);
            rng = new Mulberry32(UnityTuning.Fusion43SeedSalt + 2);
        }

        public void NewRun()
        {
            rng = new Mulberry32(game.runSeed ^ (UnityTuning.Fusion43SeedSalt + 2));
            pool.Clear();
            holdStart = lastTick = double.NegativeInfinity;
        }

        /// <summary>The current hold's age (s; 0 when not firing): the reel's line.</summary>
        public double HeldSeconds => Firing ? lastTick - holdStart : 0;

        bool Firing => SurvivalGame.RealNow - lastTick <= UnityTuning.Torch43HoldGap;

        /// <summary>The jet's sprites drawn last frame (the reel).</summary>
        public static int JetDrawn;

        void Build()
        {
            built = true;
            Color usable = Look.C(game.cfg, "colors.usable");
            white = Look.Hex(UnityTuning.WhiteHotHex33) * 2.4f;
            gold = Look.Hex(UnityTuning.Torch43GoldHex) * 2.0f;
            orange = Look.Hex(UnityTuning.Torch43OrangeHex) * 1.7f;
            deep = Look.Hex(UnityTuning.Torch43DeepHex) * 1.2f;
            cyanHot = (UiTheme.Ready ? UiTheme.CyanHot : usable) * 1.8f;
            cyan = (UiTheme.Ready ? UiTheme.Cyan : usable) * 1.4f;
            cyanPale = (UiTheme.Ready ? UiTheme.CyanPale : usable) * 1.1f;
            jet = new FxSprites(game.mats.neon, UnityTuning.KenneyFlare, UnityTuning.Torch43JetSegments * (3 + UnityTuning.Vis44TorchRing)); // NIGHT44 V2: room for vis44's cone ring
            // NIGHT47 F fx47 (G43; F-cards 1f): the player's cyan-blue plasma: a white-hot core -> blue -> violet tips (rule 8: the warm orange was
            // the enemies' colour on a player weapon; the violet stays in the overheal's band, h <= 295, never RIVAL's magenta)
            plasmaBlue = Look.Hex(UnityTuning.Torch47BlueHex) * 2.0f;
            plasmaMid = Look.Hex(UnityTuning.Torch47MidHex) * 1.7f;
            plasmaTip = Look.Hex(UnityTuning.Torch47TipHex) * 1.3f;
            // NIGHT49 X fx49 (Fx49.cs): the TORCH in the player's kit tones (Look49's usable cyan: core white-cyan -> cyan -> the rim -> a dim tip),
            // one hue with every other player effect (the plasma's blue 231 deg / violet 289 deg tips were two more hues of the player's)
            Fx49.Tones(Fx49Owner.Player, out k49Core, out k49Body, out k49Rim);
            float lk = Fx49.GlowK(Fx49Owner.Player, Fx49Tier.Light);
            k49Core *= lk;
            k49Body *= lk;
            k49Rim *= lk;
            k49Tip = k49Rim * UnityTuning.Fx49TorchTipShare;
            foreach (Color c in new[] { white, gold, orange, deep, cyanHot, cyan, cyanPale, plasmaBlue, plasmaMid, plasmaTip, k49Core, k49Body, k49Rim, k49Tip })
            {
                jet.Warm(c);
                pool.Warm(c);
            }
            smoke = new Fx43Shade(game.mats.neon, "Torch43 smoke", UnityTuning.Torch43SmokePool, Look.Hex(UnityTuning.Torch43SmokeHex), UnityTuning.Torch43SmokeDark);
        }

        // the jet's colour at share s of its length (0 muzzle .. 1 reach)
        Color plasmaBlue, plasmaMid, plasmaTip; // NIGHT47 F fx47
        Color k49Core, k49Body, k49Rim, k49Tip; // NIGHT49 X fx49: the kit's player tones (light tier)
        bool wasFiring47;
        float lickAge47 = -1, lickReach47 = 7;
        Vector3 lickAim47 = Vector3.forward, lickFrom47;

        Color JetColour(float s)
        {
            if (Fx49.On(game.cfg)) return s < 0.12f ? k49Core : s < 0.45f ? k49Body : s < 0.8f ? k49Rim : k49Tip; // NIGHT49 X fx49: the kit's player tones
            if (Fx47View.RowOn(game.cfg)) return s < 0.12f ? white : s < 0.45f ? plasmaBlue : s < 0.8f ? plasmaMid : plasmaTip; // NIGHT47 F fx47: the plasma
            if (!UnityTuning.Torch43Warm) return s < 0.2f ? cyanHot : s < 0.6f ? cyan : cyanPale;
            return s < 0.12f ? white : s < 0.4f ? gold : s < 0.75f ? orange : deep;
        }

        public void OnFlame(FlameEvent e)
        {
            if (e == null || e.owner != "player" || game.cam == null || game.loadout == null || game.loadout.Weapon != "torch") return;
            if (!built) Build();
            double now = SurvivalGame.RealNow;
            bool fx47 = Fx47View.RowOn(game.cfg);
            if (Fx49.On(game.cfg)) Fx49.Note("torch", Fx49Tier.Light, Fx49Owner.Player); // NIGHT49 X fx49: the kit's probe (the a249 line)
            bool calm = game.cfg.movement.settings.reducedMotion;
            if (now - lastTick > UnityTuning.Torch43HoldGap)
            {
                holdStart = now;
                if (fx47 && !calm) game.trauma.Add(UnityTuning.Torch47StartTrauma); // NIGHT47 F fx47: the pressure kick on the start
            }
            else if (fx47 && !calm) game.trauma.Add(UnityTuning.Torch47HoldTrauma); // the low continuous recoil while held (C-3: held weapons feel held)
            lastTick = now;
            float ramp = (float)Feel43Math.TorchRamp(now - holdStart);
            Transform c = game.cam.transform;
            Vector3 aim = c.forward;
            double range = e.range > 0 ? e.range : 7;
            double r = range;
            if (game.sim != null && game.sim.world != null && game.sim.world.RayIntersect(WebSpace.FromUnity(c.position), WebSpace.FromUnity(aim), out RayHit hit) && hit.distance < range) r = Math.Max(0.6, hit.distance);
            reach = (float)r * ramp;
            reachAt = c.position + aim * reach;
            // the lick: flame spreading over what the jet hits, embers, smoke rising off it
            Vector3 side = c.right, up = Vector3.up;
            for (int i = 0; i < UnityTuning.Torch43Puffs; i++)
            {
                float a = (float)(rng.Next() * Math.PI * 2);
                Vector3 v = (side * Mathf.Cos(a) + up * (0.6f + 0.6f * Mathf.Abs(Mathf.Sin(a)))) * UnityTuning.Torch43LickSpeed * (0.6f + 0.4f * (float)rng.Next()) - aim * 1.5f;
                pool.Add(Fx43Pool.Shape.Flare, reachAt - aim * 0.3f, v, (i & 1) == 0 ? JetColour(0.5f) : JetColour(0.8f), (float)UnityTuning.Torch43PuffLife * (0.7f + 0.3f * (float)rng.Next()), UnityTuning.Torch43PuffFrom * ramp, UnityTuning.Torch43PuffTo * ramp, drag: 2.2f, roll: (float)(rng.Next() * Math.PI * 2), fadePow: 1.4f);
            }
            for (int i = 0; i < UnityTuning.Torch43Embers; i++)
            {
                Vector3 v = (SprayMath.UnitSphere(rng.Next(), rng.Next()) + up * 0.8f).normalized * 5f * (0.5f + 0.5f * (float)rng.Next());
                pool.Add(Fx43Pool.Shape.Streak, reachAt - aim * 0.2f, v, JetColour(0.3f), 0.35f + 0.25f * (float)rng.Next(), 0, 0, 0.035f, gravity: -1.5f, drag: 1.5f);
            }
            for (int i = 0; i < UnityTuning.Torch43Haze; i++)
                SmokePuff(reachAt - aim * 0.5f + side * (float)((rng.Next() - 0.5) * 1.2), up * UnityTuning.Torch43Rise + side * (float)((rng.Next() - 0.5) * 1.5), ramp);
            Feel43Probe.TorchTicks++;
            Feel43Probe.TorchSprites += UnityTuning.Torch43Puffs + UnityTuning.Torch43Embers + UnityTuning.Torch43Haze;
            LightPools lights = game.lightAndLens?.lights;
            if (lights != null)
            {
                double flick = 1 + (rng.Next() * 2 - 1) * UnityTuning.Torch43LightFlicker;
                Vector3 lightAt = c.position + aim * (float)(reach * UnityTuning.Torch43LightAt);
                Color lc = fx47 ? Look.Hex(UnityTuning.Torch47BlueHex) : UnityTuning.Torch43Warm ? Look.Hex(UnityTuning.Torch43OrangeHex) : cyanHot / 1.8f; // NIGHT47 F fx47: a blue light on the walls
                if (Fx49.On(game.cfg)) lc = Fx49.Hue(Fx49Owner.Player); // NIGHT49 X fx49: the player's cyan on the walls (the kit's hue)
                double li = UnityTuning.Torch43LightIntensity * flick * ramp * (Fx49.On(game.cfg) ? UnityTuning.Fx49TorchLightShare : 1); // round 3: the wall light lit the aim's middle
                lights.Flash(lightAt, lc, li, UnityTuning.Torch43LightRange, UnityTuning.Torch43LightLife);
                Feel43Probe.TorchLights++;
            }
        }

        // the smoke puffs: their own small pool (position, velocity, age) drawn with Fx43Shade
        struct Smoke
        {
            public Vector3 pos, vel;
            public float age, life, size, roll;
            public bool live;
        }

        readonly Smoke[] smokes = new Smoke[UnityTuning.Torch43SmokePool];
        int nextSmoke;

        void SmokePuff(Vector3 at, Vector3 vel, float ramp)
        {
            ref Smoke s = ref smokes[nextSmoke];
            nextSmoke = (nextSmoke + 1) % smokes.Length;
            s.pos = at;
            s.vel = vel;
            s.age = 0;
            s.life = (float)UnityTuning.Torch43HazeLife * (0.8f + 0.4f * (float)rng.Next());
            s.size = UnityTuning.Torch43HazeFrom * ramp;
            s.roll = (float)(rng.Next() * Math.PI * 2);
            s.live = true;
        }

        public void Render(double frameDt)
        {
            pool.Render(frameDt);
            JetDrawn = 0;
            if (!built || game.cam == null) return;
            Transform ct = game.cam.transform;
            float dt = (float)frameDt;
            smoke.Begin();
            jet.Begin();
            bool firing = Firing && game.loadout != null && game.loadout.Weapon == "torch";
            if (firing)
            {
                float ramp = (float)Feel43Math.TorchRamp(SurvivalGame.RealNow - holdStart);
                flow = (flow + frameDt * UnityTuning.Torch43FlowHz) % 1.0;
                Vector3 aim = ct.forward;
                Vector3 muzzle = game.viewmodel != null && game.viewmodel.MuzzleWorld(out Vector3 m) ? m : ct.position + aim * 0.5f;
                Vector3 end = ct.position + aim * reach; // the jet bends from the muzzle onto the aim line by its far end
                int n = UnityTuning.Torch43JetSegments;
                double t = SurvivalGame.RealNow;
                bool v44 = Vis44.On(game.cfg);
                bool k49 = Fx49.On(game.cfg); // NIGHT49 X fx49 round 3
                for (int i = 0; i < n; i++)
                {
                    float s = (float)((i + flow) / n); // flowing outward
                    Vector3 p = Vector3.LerpUnclamped(muzzle, end, s);
                    // wobble off the line, growing with distance (two slow sines per puff: rolling, not jitter)
                    float w = 0.12f + 0.45f * s;
                    p += ct.right * (w * Mathf.Sin((float)(t * 7.1 + i * 1.7))) * 0.5f + ct.up * (w * Mathf.Sin((float)(t * 5.3 + i * 2.3))) * 0.4f + Vector3.up * (s * s * 0.5f);
                    float size = Mathf.Lerp(UnityTuning.Torch43JetFrom, UnityTuning.Torch43JetTo, s) * ramp;
                    float k = Mathf.Clamp01(s * 6) * (1 - s * 0.45f); // fades in off the muzzle, keeps most of its heat to the end
                    float roll = (float)(t * 2.4 + i * 1.3);
                    // NIGHT49 X fx49 round 3 (D-17; n49-r2 a249: the TORCH +0.30 ON in the wash box): a puff inside the middle 20 % draws at the light
                    // tier's wash share (the flame converges on the aim there); outside it, as it was
                    if (k49 && Fx49.Place(game.cam, p, out float _, out bool inBox) && inBox) { size *= Fx49.WashSize(Fx49Tier.Light); k *= Fx49.WashK(Fx49Tier.Light); }
                    jet.Billboard(p, size, roll, ct, JetColour(s), k);
                    jet.Billboard(p + aim * 0.1f, size * 0.55f, -roll * 1.3f, ct, JetColour(Mathf.Max(0, s - 0.25f)), Mathf.Min(1, k * 1.2f)); // the hotter heart
                    JetDrawn += 2;
                    if (v44)
                    {
                        // NIGHT44 V2 (vis44): a dense cone, not a thin stream: a ring of flame puffs round the axis at each segment, the ring's
                        // radius the cone's (Vis44TorchConeDeg from the muzzle), turning with the flow so the cone's skin rolls
                        float dist = (p - muzzle).magnitude;
                        float rad = dist * Mathf.Tan(UnityTuning.Vis44TorchConeDeg * Mathf.Deg2Rad);
                        int nr = UnityTuning.Vis44TorchRing;
                        for (int q = 0; q < nr; q++)
                        {
                            float ang = (float)((q + 0.5 * (i & 1)) / nr * Math.PI * 2 + t * 1.9);
                            Vector3 rp = p + (ct.right * Mathf.Cos(ang) + ct.up * Mathf.Sin(ang)) * rad;
                            jet.Billboard(rp, size * UnityTuning.Vis44TorchRingSize, roll + q, ct, JetColour(Mathf.Min(1, s + 0.1f)), k * 0.8f);
                        }
                        JetDrawn += nr;
                    }
                    if (s > 0.62f) smoke.Billboard(p + Vector3.up * (0.25f + s * 0.4f), size * 1.15f, roll * 0.5f, ct, (s - 0.62f) / 0.38f * 0.7f);
                }
                Feel43Probe.TorchSprites = Math.Max(Feel43Probe.TorchSprites, JetDrawn);
                if (Fx47View.RowOn(game.cfg)) // NIGHT47 F fx47 (G43): the white-hot core at the nozzle (a hot heart before the blue)
                {
                    jet.Billboard(muzzle + aim * 0.08f, UnityTuning.Torch47CoreSize * ramp, (float)(t * 9.0), ct, Fx49.On(game.cfg) ? k49Core : white, 1f); // NIGHT49 X fx49: the kit's core
                    JetDrawn++;
                    lickAim47 = aim;
                    lickFrom47 = muzzle;
                    lickReach47 = reach;
                }
            }
            // NIGHT47 F fx47 (G43): the LICK on release: the flame's last tongue stretches past its reach and fades (NIGHT47.md section 3)
            if (wasFiring47 && !firing && Fx47View.RowOn(game.cfg)) lickAge47 = 0;
            wasFiring47 = firing;
            if (lickAge47 >= 0)
            {
                float lk = lickAge47 / UnityTuning.Torch47LickS;
                if (lk >= 1f) lickAge47 = -1;
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float s = (i + 1) / 6f;
                        Vector3 p = lickFrom47 + lickAim47 * (lickReach47 * UnityTuning.Torch47LickStretch * s * (0.6f + 0.4f * lk));
                        jet.Billboard(p, Mathf.Lerp(UnityTuning.Torch43JetFrom, UnityTuning.Torch43JetTo, s) * (1f - lk), lk * 3f + i, ct, JetColour(s), (1f - lk) * (1f - s * 0.4f));
                    }
                    JetDrawn += 6;
                    lickAge47 += dt;
                }
            }
            for (int i = 0; i < smokes.Length; i++)
            {
                ref Smoke sp = ref smokes[i];
                if (!sp.live) continue;
                sp.age += dt;
                if (sp.age >= sp.life) { sp.live = false; continue; }
                sp.vel *= Mathf.Max(0, 1 - 0.8f * dt);
                sp.pos += sp.vel * dt;
                float u = sp.age / sp.life;
                float size = Mathf.Lerp(sp.size, UnityTuning.Torch43HazeTo, 1 - (1 - u) * (1 - u));
                smoke.Billboard(sp.pos, size, sp.roll + u, ct, (1 - u) * (u < 0.15f ? u / 0.15f : 1));
            }
            jet.Draw();
            smoke.Draw();
        }
    }
}
