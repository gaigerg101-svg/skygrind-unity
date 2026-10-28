// NIGHT47 builder F (design.n47.fx47; NIGHT47.md G24; GG 2026-10-18: "all weapons need better effects, animations, and impact so they feel heavy and
// satisfying"). The layers today's guns lack (F-cards 1a, read in code), drawn by Fx47View on frame time with its pooled sprites:
//   - every player HIT of a gun: impact sparks (Fx47ImpactSparks streaks out of the hit, the gun's family colour) and a short light (LightPools'
//     flash slot: never a new Light);
//   - every player KILL: a world-space flare in the player's family (cyan; the gauntlet's pink) (NIGHT47.md section 3);
//   - the THUNDERCLAP's shot: a 2-frame muzzle CONE (three flares along the aim from the muzzle), not only a star;
//   - the THUNDERCLAP's air boost (a shot fired looking down in the air: boost43's): a ground ring and dust under the player, and an upward
//     camera kick (the boost had no look of its own);
//   - the swap: the new gun lands with a small camera dip (it had no camera read);
//   - the camera kicks above are one queue (Kick47), x EFFECTS INTENSITY, off under REDUCED MOTION. The guns' existing kicks are not changed in
//     this slice: they are measured first (a747's sheets, L-001), then tuned toward the ladder in F-cards 1.
// Off: nothing. Numbers: UnityTuning.F47.cs (Fx47Impact* / Fx47Kill* / Fx47Cone* / Fx47Boost* / Fx47Swap*).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Fx47View
    {
        struct Puff
        {
            public float age, life, size;
            public Vector3 at, vel;
            public byte kind; // 0 a flare, 1 a streak along its velocity, 2 a floor ring, 3 a falling chunk (a burst's)
            public byte colour; // 0 cyan core, 1 cyan body, 2 pink body, 3 dust
        }

        readonly Puff[] puffs = new Puff[UnityTuning.Fx47Puffs];
        int nextPuff, livePuffs;
        Color wCyanCore, wCyan, wPink, wDust;

        struct Kick
        {
            public float age, deg, roll, rise, settle;
        }

        readonly Kick[] kicks = new Kick[4];

        void BuildWeapons47()
        {
            WarmWeapons47(); // EXT53 B: already warmed by MakeSheets53 (FxSprites keys a colour once: no new material here)
            for (int i = 0; i < puffs.Length; i++) puffs[i].age = -1;
            for (int i = 0; i < kicks.Length; i++) kicks[i].age = -1;
        }

        /// <summary>The weapons' colours and their materials on the three sheets (EXT53 B: split from BuildWeapons47 so Prewarm53 makes
        /// them without the puffs' and kicks' reset).</summary>
        void WarmWeapons47()
        {
            wCyanCore = Look.Hex(UnityTuning.Fx47FistCoreHex) * UnityTuning.Fx47Glow;
            wCyan = Look.C(game.cfg, "colors.usable") * UnityTuning.Fx47Glow;
            wPink = Look.Hex(UnityTuning.Fx47BodyHex) * UnityTuning.Fx47Glow;
            wDust = Look.Hex(UnityTuning.Fx47DustHex) * 0.8f;
            foreach (FxSprites s in new[] { flare, ring, trace })
                foreach (Color c in new[] { wCyanCore, wCyan, wPink, wDust }) s.Warm(c);
        }

        void HookWeapons47()
        {
            game.events.On<HitEvent>("hit", e => { if (On && e.owner == "player") OnGunHit(e); });
            game.events.On<KillEvent>("kill", e => { if (On && e.owner == "player") OnKill47(e); });
            game.events.On("fire", o => { if (On) OnFire47(o as string); });
            game.events.On("weaponRaise", o => { if (On) AddKick(-UnityTuning.Fx47SwapDipDeg, 0, 0.05f, UnityTuning.Fx47SwapSettleS); });
            game.events.On<Launch36BurstEvent>(Launch36.BurstEvent, e => { if (On) OnBurst47(e); });
            game.events.On<BossEvent>("bossDown", e => { if (Finale47.On(game.cfg)) OnFinale47(e); });
            game.events.On<BossEvent>(Finale47.TestEvent, e => { if (Finale47.On(game.cfg)) OnFinale47(e); }); // the a747 reel's beat
        }

        /// <summary>R4 finale47 (UiHud47.Finale.cs's header): the arena answers a boss's kill: a white-cyan light flare from the boss (the
        /// LightPools flash slot), two floor rings out from it, the camera's heavy-tier hit (up, a roll), x EFFECTS INTENSITY, off under REDUCED MOTION.</summary>
        void OnFinale47(BossEvent e)
        {
            if (e == null) return;
            if (!built) Build();
            Vector3 at = WebSpace.ToUnity(e.position);
            LightPools lights = game.lightAndLens?.lights;
            lights?.Flash(at + Vector3.up * 2f, Color.Lerp(Look.C(game.cfg, "colors.usable"), Color.white, 0.5f), UnityTuning.Finale47Light, UnityTuning.Finale47LightRange, UnityTuning.Finale47LightLife);
            Vector3 floor = at;
            if (game.sim != null && game.sim.world != null && game.sim.world.RayIntersect(new Vec3(e.position.x, e.position.y + 1, e.position.z), new Vec3(0, -1, 0), out RayHit hit) && hit.distance < 20)
                floor = WebSpace.ToUnity(new Vec3(e.position.x, e.position.y + 1 - hit.distance, e.position.z));
            AddPuff(2, 0, floor + Vector3.up * 0.06f, Vector3.zero, UnityTuning.Finale47RingM, UnityTuning.Finale47RingS);
            AddPuff(2, 1, floor + Vector3.up * 0.07f, Vector3.zero, UnityTuning.Finale47RingM * 1.3f, UnityTuning.Finale47RingS * 1.4f);
            AddKick(UnityTuning.Finale47KickDeg, UnityTuning.Finale47RollDeg, 0.05f, UnityTuning.Finale47SettleS);
            if (!Calm()) game.trauma.Add(UnityTuning.Finale47Trauma);
        }

        /// <summary>n47-r1b: a launch36 burst under fx47 throws small chunks (Tell36View's 40 cube bits at 11 m/s, which flew into the camera as
        /// slabs, are not drawn with the row on): Fx47BurstChunks of 0.1-0.3 m off the burst's floor, outward and AWAY from the eye.</summary>
        void OnBurst47(Launch36BurstEvent e)
        {
            Camera cam = game.cam;
            if (cam == null || e == null) return;
            if (!built) Build();
            Vector3 at = WebSpace.ToUnity(e.position) + Vector3.up * 0.1f;
            Vector3 away = at - cam.transform.position;
            away.y = 0;
            away = away.sqrMagnitude > 1e-6f ? away.normalized : cam.transform.forward;
            for (int i = 0; i < UnityTuning.Fx47BurstChunks; i++)
            {
                Vector3 dir = Quaternion.Euler(0, ((float)rng.Next() - 0.5f) * 140f, 0) * away; // within 70 deg of straight away
                Vector3 v = dir * (2f + 3f * (float)rng.Next()) + Vector3.up * (3f + 3f * (float)rng.Next());
                AddPuff(3, (byte)(i % 3 == 0 ? 2 : 3), at, v, Mathf.Lerp(UnityTuning.Fx47ChunkMin, UnityTuning.Fx47ChunkMax, (float)rng.Next()), UnityTuning.Fx47BurstChunkLife);
            }
        }

        /// <summary>A chunk: a short shard along its flight, `size` m long; not drawn within Fx47NearEye of the eye (n47-r1b). True when drawn.</summary>
        bool Chunk47(Vector3 at, Vector3 vel, float size, Vector3 eye, Color c, float k)
        {
            if ((at - eye).sqrMagnitude < UnityTuning.Fx47NearEye * UnityTuning.Fx47NearEye) return false;
            Vector3 d = vel.sqrMagnitude > 1e-6f ? vel.normalized : Vector3.up;
            trace.Streak(at - d * (size * 0.5f), at + d * (size * 0.5f), size * 0.6f, eye, c, k);
            return true;
        }

        /// <summary>G24 (round 3): the camera punch's multiplier per gun under fx47 (Viewmodel.OnFire's feel.punch peaks), the weight ladder's
        /// targets against the n47-r1b sheets (UnityTuning.F47.cs Fx47Punch*); 1 for a gun the ladder leaves as it is.</summary>
        public static double PunchMul(string weapon) =>
            weapon == "revolver" ? UnityTuning.Fx47PunchRevolver : weapon == "torch" ? UnityTuning.Fx47PunchTorch : weapon == "shotgun" ? UnityTuning.Fx47PunchShotgun : 1;

        /// <summary>A puff was added since the last draw (livePuffs counts the last frame's).</summary>
        bool PuffQueued47()
        {
            for (int i = 0; i < puffs.Length; i++) if (puffs[i].age == 0) return true;
            return false;
        }

        Color Col(byte c) => c == 0 ? wCyanCore : c == 1 ? wCyan : c == 2 ? wPink : wDust;

        void AddPuff(byte kind, byte colour, Vector3 at, Vector3 vel, float size, float life)
        {
            puffs[nextPuff] = new Puff { age = 0, life = life, size = size, at = at, vel = vel, kind = kind, colour = colour };
            nextPuff = (nextPuff + 1) % puffs.Length;
        }

        public void AddKick(float deg, float roll, float rise, float settle)
        {
            if (Calm()) return;
            for (int i = 0; i < kicks.Length; i++)
                if (kicks[i].age < 0) { kicks[i] = new Kick { age = 0, deg = deg, roll = roll, rise = rise, settle = settle }; return; }
        }

        void OnGunHit(HitEvent e)
        {
            if (e.weapon == "punch" || e.weapon == "gauntlet" || e.weapon == "torch") return; // the melee's are the aura's; the TORCH's are its flame's
            if (Fx49.On(game.cfg)) return; // NIGHT49 X fx49: a gun's hit is ONE burst, the kit's (Fx49Kit.OnHit: its sparks, its light)
            if (!built) Build();
            Vector3 at = WebSpace.ToUnity(e.position);
            Camera cam = game.cam;
            if (cam != null)
            {
                Vector3 to = cam.transform.position - at;
                float d = to.magnitude;
                if (d > 1e-3f) at += to / d * Mathf.Min(0.35f, d * 0.3f);
            }
            int n = e.weapon == "shotgun" ? UnityTuning.Fx47ImpactSparks / 2 : UnityTuning.Fx47ImpactSparks;
            for (int i = 0; i < n; i++)
            {
                Vector3 v = SprayMath.UnitSphere(rng.Next(), rng.Next()) * UnityTuning.Fx47ImpactSpeed * (0.5f + 0.5f * (float)rng.Next());
                AddPuff(1, (byte)(i % 3 == 0 ? 0 : 1), at, v, 0.025f, UnityTuning.Fx47ImpactLife);
            }
            LightPools lights = game.lightAndLens?.lights;
            lights?.Flash(at, Look.C(game.cfg, "colors.usable"), UnityTuning.Fx47ImpactLight, UnityTuning.Fx47ImpactLightRange, UnityTuning.Fx47ImpactLightLife);
        }

        void OnKill47(KillEvent e)
        {
            if (Fx49.On(game.cfg)) return; // NIGHT49 X fx49: the kill's flare is the kit's (heavy; critical for a headshot / melee / boss kill)
            if (!built) Build();
            bool pink = e.weapon == "gauntlet";
            Vector3 at = WebSpace.ToUnity(e.position) + Vector3.up * 0.2f;
            AddPuff(0, (byte)(pink ? 2 : 1), at, Vector3.zero, UnityTuning.Fx47KillFlare, UnityTuning.Fx47KillLife);
            AddPuff(0, 0, at, Vector3.zero, UnityTuning.Fx47KillFlare * 0.5f, UnityTuning.Fx47KillLife);
        }

        void OnFire47(string weapon)
        {
            if (weapon != "shotgun" || game.cam == null) return;
            if (!built) Build();
            Transform ct = game.cam.transform;
            Vector3 muzzle = WeaponFx.Current != null ? WeaponFx.Current.HeldMuzzle("shotgun") : ct.position + ct.forward * 0.6f;
            Vector3 aim = ct.forward;
            Fx49Kit kit = Fx49.On(game.cfg) ? Fx49Kit.Of(game) : null; // NIGHT49 X fx49: the cone and the boost ring from the kit (player, heavy)
            for (int i = 0; i < 3; i++)
            {
                if (kit != null) kit.Spawn(i == 0 ? "thunder-shot" : null, muzzle + aim * (0.15f + 0.25f * i), Fx49Owner.Player, Fx49Tier.Heavy, -1, 1, default, UnityTuning.Fx49ConeScale * (0.6f + 0.4f * i));
                else AddPuff(0, (byte)(i == 0 ? 0 : 1), muzzle + aim * (0.15f + 0.25f * i), Vector3.zero, UnityTuning.Fx47ConeSize * (0.6f + 0.4f * i), UnityTuning.Fx47ConeLife);
            }
            // the air boost: fired looking down while airborne (boost43's shot)
            if (game.mover != null && !game.mover.grounded && aim.y < -UnityTuning.Fx47BoostLookDown)
            {
                Vector3 feet = WebSpace.ToUnity(game.Feet);
                Vector3 floor = feet;
                if (game.sim != null && game.sim.world != null && game.sim.world.RayIntersect(game.Feet, new Vec3(0, -1, 0), out RayHit hit) && hit.distance < 12) floor = feet + Vector3.down * (float)hit.distance;
                if (kit != null) kit.Spawn("thunder-boost", floor + Vector3.up * 0.06f, Fx49Owner.Player, Fx49Tier.Heavy, -1, 2, Vector3.up);
                else AddPuff(2, 1, floor + Vector3.up * 0.06f, Vector3.zero, UnityTuning.Fx47BoostRing, UnityTuning.Fx47BoostLife);
                for (int i = 0; i < 10; i++)
                {
                    Vector3 v = Quaternion.Euler(0, i * 36f, 0) * Vector3.forward * UnityTuning.Fx47BoostDust + Vector3.up * 0.8f;
                    AddPuff(0, 3, floor + Vector3.up * 0.15f, v, 0.5f, UnityTuning.Fx47BoostLife * 1.4f);
                }
                AddKick(UnityTuning.Fx47BoostKickDeg, 0, 0.04f, UnityTuning.Fx47BoostSettleS);
            }
        }

        /// <summary>The puffs drawn this frame (from Render's sprite pass). Returns the sprites added; sets livePuffs.</summary>
        int Puffs47(Transform ct, float dt)
        {
            int n = 0, live = 0;
            Vector3 eye = ct.position;
            for (int i = 0; i < puffs.Length; i++)
            {
                if (puffs[i].age < 0) continue;
                float a = puffs[i].age;
                puffs[i].age += dt;
                if (a > puffs[i].life) { puffs[i].age = -1; continue; }
                live++;
                float k = 1f - a / puffs[i].life;
                Vector3 at = puffs[i].at + puffs[i].vel * a;
                Color c = Col(puffs[i].colour);
                if (puffs[i].kind == 3) // a chunk (n47-r1b): it falls
                {
                    at += Vector3.down * (9f * a * a);
                    if (Chunk47(at, puffs[i].vel + Vector3.down * (18f * a), puffs[i].size, eye, c, k)) n++;
                    continue;
                }
                if (puffs[i].kind == 0) flare.Billboard(at, puffs[i].size * (1f + 0.5f * (1f - k)), 0f, ct, c, k);
                else if (puffs[i].kind == 1) trace.Streak(at, at + puffs[i].vel * 0.035f, puffs[i].size, eye, c, k);
                else ring.Disc(at, Vector3.up, puffs[i].size * (0.3f + 0.7f * (1f - k * k)) / UnityTuning.KenneyRingMid, 0f, c, k);
                n++;
            }
            livePuffs = live;
            return n;
        }

        /// <summary>The kick queue applied to the camera (after CameraFeel, as the blast's kick); returns the summed pitch (deg).</summary>
        float Kicks47(Camera cam, float dt)
        {
            if (cam == null) return 0;
            float pitch = 0, roll = 0;
            for (int i = 0; i < kicks.Length; i++)
            {
                if (kicks[i].age < 0) continue;
                float t = kicks[i].age;
                kicks[i].age += dt;
                if (t > kicks[i].rise + kicks[i].settle) { kicks[i].age = -1; continue; }
                float up = Mathf.Clamp01(t / Mathf.Max(1e-4f, kicks[i].rise)), down = Mathf.Clamp01((t - kicks[i].rise) / kicks[i].settle);
                float k = up * (1f - down) * (1f - down);
                pitch += kicks[i].deg * k;
                roll += kicks[i].roll * k;
            }
            if (pitch == 0 && roll == 0) return 0;
            float ei = (float)System.Math.Max(0, System.Math.Min(1, game.cfg.movement.settings.effectsIntensity));
            cam.transform.rotation = cam.transform.rotation * Quaternion.Euler(-pitch * ei, 0, roll * ei);
            return pitch * ei;
        }
    }
}
