// NIGHT43 builder F1 (design.n43.feel43, a view row: no sim bit; G20 GG: gunshots and hits need more impact): the night's gun reads, drawn
// from the sim's events (nothing here is read by the sim):
// - every gun hit of the player's (`hit`, not melee / a wave's share): a white-hot star at the hit point for ~3 frames and streak sparks
//   thrown back toward the shooter, sized by weapon (Feel43Math.GunHit: nailgun < KESTREL < THUNDERCLAP pellet < LONGWAVE), at most
//   Gun43SparksMax stars a frame a weapon (a volley adds sparks, not a sheet of stars);
// - the THUNDERCLAP's charged SLUG (shotResult "shotgun" then chargeShot27Fire "shotgun" with the slug on, ChargeShots27.FireSlug31): a thick
//   white-hot tracer in a cyan sheath from the muzzle to the impact, an explosion there (a flare, a ring, sparks, a light, trauma +0.3), and
//   every body the slug killed thrown along the shot (its chunks in the enemy colour, flying down the line);
// - a boss hit while it is OPEN (sim.N43BossState.open, or the WICK's core hit): the strongest gun read: a big white-hot star, a double cyan
//   ring, trauma, a chroma kick, the CRACK rumble (at most one every BossOpen43Gap s).
// The rumble per shot (light / THUD / release) is Feel43's. Colours: the player's cyan ladder and white-hot (rule 8); the enemy colour only
// on the enemy's own chunks. REDUCED MOTION drops the trauma and the chroma; the sprites stay.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class GunFeel43
    {
        readonly SurvivalGame game;
        readonly Feel43 feel;
        readonly Fx43Pool pool;
        Mulberry32 rng;
        bool colours;
        Color white, cyan, cyanHot, enemy, grit;

        // the step's stars per weapon (reset each frame)
        int starsRevolver, starsShotgun, starsOther;

        // the last player hit (the WICK's bossHit has no position)
        Vector3 lastHitAt;
        double lastHitTime = double.NegativeInfinity;
        double lastBossRead = double.NegativeInfinity;

        // the slug: the last shotgun shotResult, the kills since its `fire`
        bool shotgunShot;
        Vec3 shotOrigin, shotEnd, shotDir;
        string shotSurface;
        readonly Vector3[] slugKills = new Vector3[4];
        int slugKillCount;

        public GunFeel43(SurvivalGame game, Feel43 feel)
        {
            this.game = game;
            this.feel = feel;
            pool = new Fx43Pool(game, UnityTuning.Gun43Pool);
            rng = new Mulberry32(UnityTuning.Fusion43SeedSalt + 1);
        }

        public void NewRun()
        {
            rng = new Mulberry32(game.runSeed ^ (UnityTuning.Fusion43SeedSalt + 1));
            pool.Clear();
            shotgunShot = false;
            slugKillCount = 0;
            lastHitTime = lastBossRead = double.NegativeInfinity;
        }

        void Colours()
        {
            if (colours) return;
            colours = true;
            float g = UnityTuning.Gun43Glow;
            white = Look.Hex(UnityTuning.WhiteHotHex33) * g;
            cyan = (UiTheme.Ready ? UiTheme.Cyan : Look.C(game.cfg, "colors.usable")) * g;
            cyanHot = (UiTheme.Ready ? UiTheme.CyanHot : Look.C(game.cfg, "colors.usable")) * g;
            enemy = Look.C(game.cfg, "colors.enemy") * g;
            grit = Look.Hex(UnityTuning.Slug43GritHex) * 1.2f; // the blast's stone grit (pale, neutral: not the player's cyan, not danger)
            pool.Warm(white);
            pool.Warm(cyan);
            pool.Warm(cyanHot);
            pool.Warm(enemy);
            pool.Warm(grit);
            pool.Warm(Look.Hex(UnityTuning.Torch43OrangeHex) * 2.2f);
            pool.Warm(Look.Hex(UnityTuning.Torch43GoldHex) * 2.4f);
        }

        bool Calm() => game.cfg.movement.settings.reducedMotion;

        Vector3 Eye() => game.cam != null ? game.cam.transform.position : Vector3.zero;

        // --- the events ------------------------------------------------------------------------------------------------------------------------

        public void OnHit(HitEvent e)
        {
            if (!Feel43Math.IsGunHit(e)) return;
            Colours();
            Vector3 at = WebSpace.ToUnity(e.position);
            lastHitAt = at;
            lastHitTime = SurvivalGame.RealNow;
            if (Fx49.On(game.cfg)) return; // NIGHT49 X fx49: the hit's picture is the kit's one burst (the boss read below still finds this hit's point)
            float[] w = Feel43Math.GunHit(e.weapon);
            int sparks = (int)w[0];
            float star = w[1], width = w[2];
            ref int stars = ref (e.weapon == "revolver" ? ref starsRevolver : ref (e.weapon == "shotgun" ? ref starsShotgun : ref starsOther));
            if (star > 0 && stars < UnityTuning.Gun43SparksMax)
            {
                stars++;
                pool.Add(Fx43Pool.Shape.Star, at, Vector3.zero, white, (float)UnityTuning.Gun43StarLife, star * 1.25f, star, roll: (float)(rng.Next() * Math.PI), fadePow: 1);
                pool.Add(Fx43Pool.Shape.Flare, at, Vector3.zero, cyanHot, (float)UnityTuning.Gun43StarLife * 2, star * 0.8f, star * 1.4f);
                Feel43Probe.GunStars++;
            }
            Vector3 back = Eye() - at;
            back = back.sqrMagnitude > 1e-6f ? back.normalized : Vector3.up;
            for (int i = 0; i < sparks; i++)
            {
                Vector3 r = SprayMath.UnitSphere(rng.Next(), rng.Next());
                Vector3 v = (back * 0.5f + r).normalized * UnityTuning.Gun43SparkSpeed * (0.6f + 0.4f * (float)rng.Next());
                pool.Add(Fx43Pool.Shape.Streak, at, v, (i & 1) == 0 ? white : cyanHot, (float)UnityTuning.Gun43SparkLife * (0.7f + 0.3f * (float)rng.Next()), 0, 0, width, gravity: 9, drag: 3);
                Feel43Probe.GunSparks++;
            }
        }

        public void OnFire(string weapon)
        {
            if (weapon != "shotgun") return;
            shotgunShot = false;
            slugKillCount = 0;
        }

        public void OnKill(KillEvent e)
        {
            if (e == null || e.owner != "player" || e.weapon != "shotgun") return;
            if (slugKillCount < slugKills.Length) slugKills[slugKillCount++] = WebSpace.ToUnity(e.position);
        }

        public void OnShot(ShotEvent e)
        {
            if (e == null || e.owner != "player" || e.weapon != "shotgun") return;
            shotgunShot = true;
            shotOrigin = e.origin;
            shotEnd = e.end;
            shotDir = e.dir;
            shotSurface = e.surface;
        }

        /// <summary>chargeShot27Fire: the charged release (Feel43 plays the rumble); a THUNDERCLAP slug draws its round here.</summary>
        public void OnChargeFire(ChargeShot27Event e)
        {
            if (e == null || e.gun != "shotgun" || !shotgunShot) return;
            ChargeShots27 cs = game.loadout != null ? game.loadout.charge27 : null;
            if (cs == null || !cs.p.slug) return; // a charged pellet volley keeps the pellets' read
            shotgunShot = false;
            Colours();
            Vector3 end = WebSpace.ToUnity(shotEnd), dir = WebSpace.ToUnity(shotDir).normalized;
            Vector3 from = game.viewmodel != null && game.viewmodel.MuzzleWorld(out Vector3 m) ? m : WebSpace.ToUnity(shotOrigin) + dir * 0.5f;
            float life = (float)UnityTuning.Slug43Life;
            // n43-r1: on the slug's own full-screen cyan wash the white round and the explosion lost their edge: an ink shadow down the line
            // and a dark halo behind the impact (multiply) give them value contrast; the round is a brighter, thicker white core
            float span = Vector3.Distance(from, end);
            for (float s = 1.2f; s < span - 0.5f; s += UnityTuning.Slug43InkStep)
                pool.Add(Fx43Pool.Shape.Shade, Vector3.Lerp(from, end, s / span), Vector3.zero, Color.black, life * 1.2f, UnityTuning.Slug43SheathWidth * 1.6f, UnityTuning.Slug43SheathWidth * 1.8f, fadePow: 1);
            pool.Line(from, end, BossGrade54.Rim(cyan), life * 1.3f, UnityTuning.Slug43SheathWidth); // NIGHT54 G grade54: the sheath in the boss's rim
            pool.Line(from, end, white, life, UnityTuning.Slug43Width);
            pool.Line(from, end, white, life * 0.6f, UnityTuning.Slug43Width * 0.5f); // a hotter heart down the middle
            // the explosion at the impact (W1's slug43 event draws its own blast at the slug's end this frame when it is on)
            if (blastFrame != Time.frameCount) Blast(end, dir, UnityTuning.Slug43RingRadius, false);
            // the bodies thrown along the shot
            for (int k = 0; k < slugKillCount; k++)
            {
                for (int i = 0; i < UnityTuning.Slug43Chunks; i++)
                {
                    Vector3 r = SprayMath.UnitSphere(rng.Next(), rng.Next());
                    Vector3 v = (dir * 1.2f + r * 0.55f + Vector3.up * 0.25f).normalized * UnityTuning.Slug43ChunkSpeed * (0.6f + 0.4f * (float)rng.Next());
                    pool.Add(Fx43Pool.Shape.Streak, slugKills[k] + Vector3.up * 0.9f, v, enemy, (float)UnityTuning.Slug43ChunkLife * (0.7f + 0.3f * (float)rng.Next()), 0, 0, 0.16f, gravity: 14, drag: 1.2f, fadePow: 1.5f);
                    Feel43Probe.SlugChunks++;
                }
            }
            slugKillCount = 0;
            if (!Calm()) game.trauma.Add(UnityTuning.Slug43Trauma);
            Feel43Probe.Slugs++;
        }

        int blastFrame = -1;

        /// <summary>The slug's blast at `at`: the dark halo behind it (value contrast), a white / cyan flare, a shock ring growing to `radius`
        /// (and a second, flat on the floor), sparks, debris (the enemy colour when the blast killed, pale stone grit otherwise), a white light.</summary>
        void Blast(Vector3 at, Vector3 dir, float radius, bool killed)
        {
            float life = (float)UnityTuning.Slug43Life;
            pool.Add(Fx43Pool.Shape.Shade, at, Vector3.zero, Color.black, (float)UnityTuning.Slug43HaloLife, UnityTuning.Slug43FlareSize * 1.1f, Mathf.Max(UnityTuning.Slug43FlareSize * 2.2f, radius * 2.4f), fadePow: 1.2f);
            pool.Add(Fx43Pool.Shape.Flare, at, Vector3.zero, white, life * 1.4f, UnityTuning.Slug43FlareSize * 0.4f, UnityTuning.Slug43FlareSize);
            pool.Add(Fx43Pool.Shape.Flare, at, Vector3.zero, cyanHot, life * 2f, UnityTuning.Slug43FlareSize * 0.6f, UnityTuning.Slug43FlareSize * 1.5f);
            pool.Add(Fx43Pool.Shape.Ring, at, Vector3.zero, cyanHot, (float)UnityTuning.Slug43RingTime, radius * 0.15f, radius, fadePow: 2);
            pool.Add(Fx43Pool.Shape.Ring, at, Vector3.zero, white, (float)UnityTuning.Slug43RingTime * 1.2f, radius * 0.1f, radius * 0.75f, roll: 0.7f, fadePow: 2);
            for (int i = 0; i < UnityTuning.Slug43Sparks; i++)
            {
                Vector3 r = SprayMath.UnitSphere(rng.Next(), rng.Next());
                Vector3 v = (r - dir * 0.6f).normalized * UnityTuning.Slug43SparkSpeed * (0.5f + 0.5f * (float)rng.Next());
                pool.Add(Fx43Pool.Shape.Streak, at, v, (i % 3) == 0 ? white : cyanHot, 0.3f + 0.15f * (float)rng.Next(), 0, 0, 0.06f, gravity: 12, drag: 2.5f);
            }
            for (int i = 0; i < UnityTuning.Slug43Debris; i++)
            {
                Vector3 r = SprayMath.UnitSphere(rng.Next(), rng.Next());
                r.y = Mathf.Abs(r.y) + 0.35f; // thrown up and out
                Vector3 v = r.normalized * UnityTuning.Slug43DebrisSpeed * (0.5f + 0.5f * (float)rng.Next());
                pool.Add(Fx43Pool.Shape.Streak, at, v, killed && (i & 1) == 0 ? enemy : grit, 0.45f + 0.3f * (float)rng.Next(), 0, 0, 0.13f, gravity: 16, drag: 0.8f, fadePow: 1.3f);
            }
            game.lightAndLens?.lights?.Flash(at, white / UnityTuning.Gun43Glow, UnityTuning.Slug43LightIntensity, UnityTuning.Slug43LightRange, UnityTuning.Slug43LightLife);
        }

        /// <summary>W1's `slug43` (Core/Slug43.cs, payload Slug43Event: position, dir, surface, killed, blastHits, blastKills, radius): the blast
        /// at the slug's end. Read by field name (Ev43) so this builds with or without W1's types.</summary>
        public void OnSlug43(object e)
        {
            if (e == null || Ev43.Get(e, "owner", "player") != "player") return;
            Colours();
            Vector3 at = WebSpace.ToUnity(Ev43.Get(e, "position", default(Vec3)));
            Vector3 dir = WebSpace.ToUnity(Ev43.Get(e, "dir", new Vec3(0, 0, -1)));
            dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.forward;
            float radius = Mathf.Max(UnityTuning.Slug43BlastMinR, (float)Ev43.Get(e, "radius", 0.0));
            bool killed = Ev43.Get(e, "killed", false) || Ev43.Get(e, "blastKills", 0) > 0;
            Blast(at - dir * 0.3f, dir, radius, killed); // a hair toward the shooter: in front of the struck body, not inside it
            blastFrame = Time.frameCount;
            Feel43Probe.SlugBlasts++;
        }

        /// <summary>W1's `burn43` (Core/Torch43.cs, payload Burn43Event: position, seconds): a short flare as the body catches fire.</summary>
        public void OnBurn43(object e)
        {
            if (e == null || Ev43.Get(e, "owner", "player") != "player") return;
            Colours();
            Vector3 at = WebSpace.ToUnity(Ev43.Get(e, "position", default(Vec3)));
            Color hot = UnityTuning.Torch43Warm ? Look.Hex(UnityTuning.Torch43OrangeHex) * 2.2f : cyanHot;
            Color gold = UnityTuning.Torch43Warm ? Look.Hex(UnityTuning.Torch43GoldHex) * 2.4f : white;
            pool.Add(Fx43Pool.Shape.Flare, at, Vector3.zero, gold, UnityTuning.Burn43FlareLife, UnityTuning.Burn43FlareSize * 0.4f, UnityTuning.Burn43FlareSize, fadePow: 1.5f);
            pool.Add(Fx43Pool.Shape.Ring, at, Vector3.zero, hot, UnityTuning.Burn43FlareLife, 0.2f, UnityTuning.Burn43FlareSize * 0.7f);
            for (int i = 0; i < UnityTuning.Burn43Embers; i++)
            {
                Vector3 r = SprayMath.UnitSphere(rng.Next(), rng.Next());
                r.y = Mathf.Abs(r.y) + 0.6f;
                pool.Add(Fx43Pool.Shape.Streak, at, r.normalized * 4.5f * (0.5f + 0.5f * (float)rng.Next()), (i & 1) == 0 ? hot : gold, 0.4f + 0.2f * (float)rng.Next(), 0, 0, 0.05f, gravity: -2f, drag: 1.5f);
            }
            Feel43Probe.Burns++;
        }

        /// <summary>bossHit: an OPEN boss's hit gets the strongest read.</summary>
        public void OnBossHit(BossEvent e)
        {
            if (e == null || game.sim == null) return;
            Boss43State s = game.sim.N43BossState;
            if (!Feel43Math.BossOpenHit(s.valid, s.open, e.core, e.damage)) return;
            double now = SurvivalGame.RealNow;
            if (now - lastBossRead < UnityTuning.BossOpen43Gap) return;
            lastBossRead = now;
            Colours();
            Vector3 at = now - lastHitTime <= UnityTuning.BossOpen43HitAge ? lastHitAt
                : e.position.x != 0 || e.position.y != 0 || e.position.z != 0 ? WebSpace.ToUnity(e.position)
                : game.cam != null ? game.cam.transform.position + game.cam.transform.forward * 8 : Vector3.zero;
            float star = UnityTuning.BossOpen43StarSize;
            if (Fx49.On(game.cfg)) Fx49Kit.Of(game)?.Spawn("boss-weak", at, Fx49Owner.Player, Fx49Tier.Critical, -1, 0, default, UnityTuning.Fx49BossWeakScale); // NIGHT49 X fx49: the kit's critical (the boss's own colours untouched: rule 7)
            else
            {
                pool.Add(Fx43Pool.Shape.Star, at, Vector3.zero, white, (float)UnityTuning.Gun43StarLife * 2, star * 1.3f, star, roll: (float)(rng.Next() * Math.PI), fadePow: 1);
                pool.Add(Fx43Pool.Shape.Flare, at, Vector3.zero, cyanHot, 0.22f, star * 0.6f, star * 1.6f);
                pool.Add(Fx43Pool.Shape.Ring, at, Vector3.zero, cyanHot, 0.32f, 0.3f, UnityTuning.BossOpen43RingRadius[0]);
                pool.Add(Fx43Pool.Shape.Ring, at, Vector3.zero, white, 0.36f, 0.2f, UnityTuning.BossOpen43RingRadius[1], roll: 0.6f);
            }
            if (!Calm())
            {
                game.trauma.Add(UnityTuning.BossOpen43Trauma);
                feel.Post?.Kick43(UnityTuning.Open48Chroma(game.cfg)); // NIGHT48 W (UnityTuning.W48.cs): was BossOpen43Chroma 0.6 (in post.js pulse units: a full-screen split for ~19 s)
            }
            feel.Haptics.Fire(Feel43Haptics.Crack);
            Feel43Probe.BossOpenReads++;
        }

        public void Render(double frameDt)
        {
            starsRevolver = starsShotgun = starsOther = 0;
            pool.Render(frameDt);
        }
    }
}
