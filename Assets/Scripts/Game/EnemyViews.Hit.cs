// NIGHT18 (the shared commit, a pure move from EnemyViews.cs): EnemyLook's hit members (the Hit record, the flinch / tilt / knock-back, the parry's knock, the shed block and its notch, React). Owner tonight: builder R.
// NIGHT18 U3-31.4 (builder R, M5: the hit that lands): with FeelAB on, a body hit kicks the slot's HitReactBody (Game/HitSpring.cs,
// UnityTuning.HitReact.cs): the body jumps sideways along the shooter's right, away from the side the shot hit (a centred
// shot keeps its side, a fresh body takes the id's parity), its top leans with the kick about the feet, and it squashes
// (the top drops, it widens), all on one view-only spring that peaks 32 ms after the hit, overshoots once (the squash
// rebounds into a stretch) and is back by 200 ms; a stream of hits holds it out on one side; REDUCED MOTION / EFFECTS 0
// still it and keep the flash. NIGHT17's tip away from the shot (11 deg, the pop) doesn't draw on that side: on a
// symmetric capsule it barely changed the silhouette. The flash, the vibration and the heavy / melee / parry knock-back are
// NIGHT17's on both sides. FeelAB off: ReactN17, NIGHT17's React exactly.
// NIGHT18 pass 2 (the lead: a fixed size was ~60 px of head travel at 3 m and ~5 px at 20 m): the kick, lean and squash are
// sized at 12 m and drawn x HitReactBody.RangeScale(the hit's level distance), so a revolver body hit's screen size holds
// (about 13 px of centre travel at 1080 p) from 3 m to 24 m; heavy / melee / parry stay their multiple of it.
// NIGHT18 round 1 fix (build 3's reviewers: the shove read, the impact point didn't): a body hit (not a kill, not an impact
// wave's share) also throws a HitSparks impact at the bullet's hit point (Game/HitSpark.cs: a white-hot flare and 3-6
// red-orange chips off the struck side, never toward the camera), drawn and aged in FrameHits.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class EnemyLook
    {
        struct Hit
        {
            public int id;
            public Vector3 dir; // the hit axis, level, unit (Unity space)
            public Vector3 shot; // NIGHT17: the shot's axis (eye to the aim point), unit, Unity space
            public float flinchAge, vibAge;
            public float flashAge; // NIGHT17: frame s since the hit while the run simulates (the eased flash)
            public float tiltScale; // NIGHT17: x CrunchTiltMelee for a melee hit
            public float flinch51; // NIGHT51 flinch51: kills42's weapon flinch for ReactN18 (0 = none set: x 1)
            public double damage; // NIGHT17: the hit's damage (the kill's overkill)
            public float knock, knockAge; // NIGHT17 round 1: the knock-back (m) and its frame s (a heavy hit, a melee hit, a parry)
            public HitReactBody body; // NIGHT18: the body's spring reaction (kicked only while FeelAB is on; frame time)
            public Vector3 latAxis; // NIGHT18: the shooter's right at the last hit (level, unit): the kick's axis
            public float reactRange; // NIGHT18 pass 2: the body's drawn size at the last hit's distance (HitReactBody.RangeScale)
        }

        /// <summary>NIGHT18: the damage of a full reaction (weapons.revolver.damage, read once; 0 = not read yet).</summary>
        float hitReactFullDamage;

        float HitReactFullDamage()
        {
            if (hitReactFullDamage > 0) return hitReactFullDamage;
            GameConfig cfg = game.cfg;
            hitReactFullDamage = cfg.Has("weapons.revolver.damage") && cfg.Num("weapons.revolver.damage") > 0
                ? (float)cfg.Num("weapons.revolver.damage") : UnityTuning.HitReactFullDamageFallback;
            return hitReactFullDamage;
        }

        /// <summary>
        /// NIGHT18 (FeelAB on): kick slot `slot`'s body for a hit on enemy `enemy` at `at` (Unity space) along the level shot
        /// `dir`, by `scale` (HitReactBody.Scale). The side: away from where the shot landed across the body (the shooter's
        /// right), or none when it landed within HitReactSideDeadZone of the body's axis (`sided` false: a parry, no side).
        /// Pass 2: `distance` (m, level, eye to the hit) sets the slot's drawn size (HitReactBody.RangeScale). Returns the side
        /// it was pushed to (+1 the shooter's right, -1 left, 0 a centred shot).
        /// </summary>
        int HitReactKick(int slot, int enemy, Vector3 at, bool sided, Vector3 dir, float distance, float scale)
        {
            MovementConfig mc = game.cfg.movement;
            bool motion = FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity);
            Vector3 right = Vector3.Cross(Vector3.up, dir); // dir: level, unit (or zero: a shot from straight above)
            if (right.sqrMagnitude < 1e-6f)
            {
                right = screenRight;
                right.y = 0;
                if (right.sqrMagnitude < 1e-6f) right = Vector3.right;
            }
            right.Normalize();
            int want = 0;
            if (sided)
            {
                List<Enemy> all = game.enemies.all;
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].id != enemy) continue;
                    float across = Vector3.Dot(at - WebSpace.ToUnity(all[i].position), right);
                    want = across > UnityTuning.HitReactSideDeadZone ? -1 : across < -UnityTuning.HitReactSideDeadZone ? 1 : 0;
                    break;
                }
            }
            hits[slot].latAxis = right;
            hits[slot].reactRange = HitReactBody.RangeScale(distance);
            hits[slot].body.Hit(want, (enemy & 1) == 0 ? 1 : -1, scale, motion);
            return want;
        }

        /// <summary>NIGHT18 round 1 fix: the hit sparks (Game/HitSpark.cs), made on the first frame with a camera.</summary>
        HitSparks hitSparks;

        /// <summary>
        /// NIGHT18 round 1 fix (FeelAB on): the impact at the hit point `at` (Unity space) of a hit on enemy `enemy` in slot
        /// `slot` (after its kick: its axis and range are set): a white-hot flare and the chips, off the side it `struck`. None
        /// at EFFECTS 0; FLASH INTENSITY sizes the flare.
        /// </summary>
        void HitSparkAt(int enemy, int slot, Vector3 at, Vector3 shot, Vector3 dir, int struck, float scale)
        {
            if (hitSparks == null) return;
            if (!(game.cfg.movement.settings.effectsIntensity > 0)) return;
            float flash = game.settings != null ? Mathf.Clamp01((float)game.settings.Num("flashIntensity")) : 1;
            hitSparks.Spawn(enemy, at, shot, dir.sqrMagnitude > 1e-6f ? dir : shot, hits[slot].latAxis, struck, scale, hits[slot].reactRange, flash);
        }

        // NIGHT17 round 1: each gun's weight class (feel.punch.weapons.<w>.cls), looked up once per gun (the knock-back)
        readonly Dictionary<string, bool> heavyGun = new Dictionary<string, bool>();

        bool Heavy(string w)
        {
            if (w == null) return false;
            if (heavyGun.TryGetValue(w, out bool h)) return h;
            GameConfig cfg = game.cfg;
            h = cfg.Has($"feel.punch.weapons.{w}.cls") && cfg.Str($"feel.punch.weapons.{w}.cls") == "heavy";
            heavyGun[w] = h;
            return h;
        }

        /// <summary>NIGHT17: enemy `id`'s hit slot (its own, else the oldest).</summary>
        int SlotFor(int id)
        {
            int slot = 0;
            float oldest = -1;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].id == id) return i;
                if (hits[i].flinchAge > oldest)
                {
                    oldest = hits[i].flinchAge;
                    slot = i;
                }
            }
            return slot;
        }

        /// <summary>
        /// NIGHT17 round 1 (the reviewer: a parried attacker is knocked back with a 1-frame white flash): the player's parry
        /// knocks its attacker back CrunchKnockParry m (a PERFECT PARRY CrunchKnockPerfect), tips it, and flashes it white for
        /// one frame, on frame time (it plays through the parry's freeze). View only.
        /// </summary>
        void OnParry(HitEvent e)
        {
            if (e.owner != "player" || e.enemy < 0) return;
            Vector3 dir = WebSpace.ToUnity(e.position) - WebSpace.ToUnity(game.Eye);
            dir.y = 0;
            float len = dir.magnitude;
            dir = len > 1e-4f ? dir / len : Vector3.zero;
            int slot = SlotFor(e.enemy);
            bool same = hits[slot].id == e.enemy;
            hits[slot] = new Hit
            {
                id = e.enemy, dir = dir, shot = same ? hits[slot].shot : Vector3.forward, flinchAge = 0, vibAge = 0,
                flashAge = UnityTuning.CrunchHitFlashHold, // white on this frame only, then eased back
                damage = same ? hits[slot].damage : 0, tiltScale = UnityTuning.CrunchTiltMelee,
                knock = e.perfect ? UnityTuning.CrunchKnockPerfect : UnityTuning.CrunchKnockParry, knockAge = 0,
                body = same ? hits[slot].body : HitReactBody.Rest, latAxis = same ? hits[slot].latAxis : Vector3.right,
                reactRange = same ? hits[slot].reactRange : 1, // NIGHT18
            };
            // NIGHT18 (FeelAB on): the parried attacker also gets the body's kick, x HitReactParry (no side: it keeps its own)
            if (FeelAB.On) HitReactKick(slot, e.enemy, Vector3.zero, false, dir, len, UnityTuning.HitReactParry);
        }

        readonly Hit[] hits = new Hit[UnityTuning.EnemyHitSlots];

        struct ShedMark
        {
            public int id;
            public Vector3 local; // the notch's middle in the enemy root's space
        }

        struct ShedAsk
        {
            public int id;
            public Vector3 at, dir, local;
            public float feetY;
        }

        readonly ShedMark[] sheds = new ShedMark[UnityTuning.CrunchShedSlots];
        int shedNext;
        readonly ShedAsk[] shedAsks = new ShedAsk[8];
        int shedAskCount;
        Color shedColor;
        Mesh notchMesh;
        readonly MaterialPropertyBlock notchBlock = new MaterialPropertyBlock();
        RenderParams notchParams;
        Vector3 screenRight = Vector3.right, screenUp = Vector3.up;
        /// <summary>NIGHT17: the port-only event a shed block emits (on the frame it flies; payload: CrunchShedEvent, reused), for GG's sounds.</summary>
        public const string ShedEvent = "crunchShed";

        /// <summary>NIGHT17: `crunchShed`'s payload (reused: read it inside the handler).</summary>
        public sealed class CrunchShedEvent
        {
            public string owner = "player";
            public int enemy;
        }

        readonly CrunchShedEvent shedPayload = new CrunchShedEvent();

        void OnHit(HitEvent e)
        {
            Vector3 shot = WebSpace.ToUnity(e.position) - WebSpace.ToUnity(game.Eye);
            float shotLen = shot.magnitude;
            shot = shotLen > 1e-4f ? shot / shotLen : Vector3.forward;
            Vector3 dir = WebSpace.ToUnity(e.position) - WebSpace.ToUnity(game.Eye);
            dir.y = 0; // the push and the shake stay level
            float len = dir.magnitude;
            dir = len > 1e-4f ? dir / len : Vector3.zero;
            if (!e.killed) AskShed(e, dir);
            int slot = SlotFor(e.enemy);
            // A hit mid-flinch pushes on from where the push is (the ease-in's inverse), not from 0: a fast gun's stream of
            // hits holds the enemy back instead of jerking it; each hit restarts the vibration at full size.
            float age = 0;
            if (hits[slot].id == e.enemy)
            {
                float k = Flinch(hits[slot].flinchAge);
                age = UnityTuning.CrunchTiltIn * (1 - Mathf.Sqrt(1 - Mathf.Min(1, k)));
            }
            // NIGHT17 round 1 (the reviewer: the blast's survivors didn't react): a heavy gun's hit (the THUNDERCLAP's pellets, the
            // COMET, the LONGWAVE) knocks the enemy back CrunchKnockHeavy m, a melee hit CrunchKnockMelee m; a running knock
            // that is bigger keeps going (a shot's pellets land in one step)
            float knock = e.melee ? UnityTuning.CrunchKnockMelee : Heavy(e.weapon) ? UnityTuning.CrunchKnockHeavy : 0;
            float knockAge = 0;
            if (hits[slot].id == e.enemy && hits[slot].knock * CrunchMath.Tilt(hits[slot].knockAge, UnityTuning.CrunchKnockIn, UnityTuning.CrunchKnockOut) > knock)
            {
                knock = hits[slot].knock;
                knockAge = hits[slot].knockAge;
            }
            bool same = hits[slot].id == e.enemy; // NIGHT18: the body's spring carries on for the same enemy
            hits[slot] = new Hit
            {
                id = e.enemy, dir = dir, shot = shot, flinchAge = age, vibAge = 0, flashAge = 0, damage = e.damage,
                tiltScale = e.melee ? UnityTuning.CrunchTiltMelee : 1, knock = knock, knockAge = knockAge,
                body = same ? hits[slot].body : HitReactBody.Rest, latAxis = same ? hits[slot].latAxis : Vector3.right,
                reactRange = same ? hits[slot].reactRange : 1, // NIGHT18
            };
            // NIGHT18 (FeelAB on; M5): the body's kick, its lean and squash, sized by the hit's damage and weight
            int pushed36 = 0; // NIGHT36: the side the kick went (the twist's sign)
            if (FeelAB.On && !e.killed)
            {
                Vector3 at = WebSpace.ToUnity(e.position);
                float scale = HitReactBody.Scale(e.damage, HitReactFullDamage(), e.melee, !e.melee && Heavy(e.weapon));
                int pushed = HitReactKick(slot, e.enemy, at, true, dir, len, scale);
                pushed36 = pushed;
                // round 1 fix: the impact at the hit point (not an impact wave's share: it has no bullet point)
                if (!e.shock) HitSparkAt(e.enemy, slot, at, shot, dir, -pushed, scale);
            }
            // NIGHT36 F crunch36 (view; REVIEW.md root cause 3: "no stagger pose and no knockback you can see"): every body hit (not a kill, not a wave's
            // share) shoves the body back along the shot (a gun's own knock; heavy / melee keep the bigger of theirs and this) and tips its top back
            // (HitReactBody.Stagger36, a slower spring: the blow's pose, ~75 ms in, ~0.5 s back)
            if (!e.killed && !e.shock && game.sim != null && game.sim.n36.crunch36)
            {
                float k36 = e.melee ? UnityTuning.Crunch36KnockMelee : Heavy(e.weapon) ? UnityTuning.Crunch36KnockHeavy : UnityTuning.Crunch36KnockGun;
                if (k36 > hits[slot].knock * CrunchMath.Tilt(hits[slot].knockAge, UnityTuning.CrunchKnockIn, UnityTuning.CrunchKnockOut))
                {
                    hits[slot].knock = k36;
                    hits[slot].knockAge = 0;
                }
                MovementConfig mc36 = game.cfg.movement;
                bool motion36 = FxGates.MotionOn(mc36.settings.reducedMotion, mc36.settings.effectsIntensity);
                hits[slot].body.Stagger36(e.melee ? UnityTuning.Stagger36MeleeDeg : UnityTuning.Stagger36GunDeg, motion36);
                // n36-r2 (the lead: a head-on back-tip doesn't read from the front): a squash, a twist toward the struck side, a longer white-hot hold
                int side36 = pushed36 != 0 ? pushed36 : (e.enemy & 1) == 0 ? 1 : -1;
                hits[slot].body.Pop36(UnityTuning.Crunch36Squash, e.melee ? UnityTuning.Crunch36TwistMeleeDeg : UnityTuning.Crunch36TwistDeg, side36, motion36);
                hits[slot].flashAge = UnityTuning.CrunchHitFlashHold - (Night51.On ? UnityTuning.N51HitFlashHoldS : UnityTuning.Crunch36FlashHoldS); // white for Crunch36FlashHoldS (NIGHT51: N51HitFlashHoldS), then the old fade
                Staggers36++;
            }
            // NIGHT42 F1 (kills42 + impact42, view only; G9 "every weapon's hit flinch"): the flinch by weapon: x the tilt / push / squash share and
            // the knock-back (the KESTREL 1.35, the THUNDERCLAP 1.7, the STITCHER 0.8, the gauntlet 2.2), capped; 1 with either row off
            float f42 = KillFeel42.FlinchMult(e);
            hits[slot].flinch51 = f42; // NIGHT51 flinch51: ReactN18 (the live path) never read tiltScale; it reads this
            if (f42 != 1)
            {
                hits[slot].tiltScale = Mathf.Min(hits[slot].tiltScale * f42, UnityTuning.Kills42TiltScaleMax);
                if (hits[slot].knockAge <= 0) hits[slot].knock = Mathf.Min(hits[slot].knock * f42, UnityTuning.Kills42KnockMax); // a fresh knock only: a running one is not scaled twice
                KillFeel42.Flinches++;
            }
        }

        /// <summary>NIGHT36 crunch36 (the reels): body hits that staggered since boot.</summary>
        public static int Staggers36;

        /// <summary>NIGHT17: the tilt / pop's share at `t` s after a hit (CrunchMath.Tilt over CrunchTiltIn / Out: 60-100 ms).</summary>
        static float Flinch(float t) => CrunchMath.Tilt(t, UnityTuning.CrunchTiltIn, UnityTuning.CrunchTiltOut);

        /// <summary>
        /// NIGHT17 (DOOM Eternal's destructible demons): a hit that leaves enemy `e.enemy` under CrunchShedAt of its HP, the
        /// first time, asks for one shed block (inside the step: only queued; Frame throws it and emits `crunchShed`).
        /// </summary>
        void AskShed(HitEvent e, Vector3 dir)
        {
            if (shedAskCount >= shedAsks.Length) return;
            for (int i = 0; i < sheds.Length; i++) if (sheds[i].id == e.enemy) return;
            List<Enemy> all = game.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy en = all[i];
                if (en.id != e.enemy) continue;
                if (!en.alive || !(en.maxHp > 0) || en.hp / en.maxHp >= UnityTuning.CrunchShedAt) return;
                Vector3 feet = WebSpace.ToUnity(en.position);
                // the block comes off the side facing the shot, at the aim height
                Vector3 at = WebSpace.ToUnity(e.position) - dir * (float)en.Radius;
                Quaternion yaw = Quaternion.Euler(0, -(float)(en.yaw * Mathf.Rad2Deg), 0);
                sheds[shedNext] = new ShedMark { id = e.enemy, local = Quaternion.Inverse(yaw) * (at - feet) };
                shedNext = (shedNext + 1) % sheds.Length;
                shedAsks[shedAskCount++] = new ShedAsk { id = e.enemy, at = at, dir = ShedThrow(dir), local = Vector3.zero, feetY = feet.y };
                return;
            }
        }

        /// <summary>
        /// NIGHT17 round 5: the shed block's throw: off to the side away from the shot and up, never back at the player (thrown
        /// toward the camera in the enemies' red-orange, it read as an enemy projectile, n17-r4's parry). `dir`: the shot's
        /// level axis (eye to the enemy).
        /// </summary>
        Vector3 ShedThrow(Vector3 dir)
        {
            Vector3 side = Vector3.Cross(Vector3.up, dir);
            shedSide = -shedSide; // alternate sides, no randomness
            return (dir * UnityTuning.ShedAway + side * shedSide).normalized;
        }

        float shedSide = 1;

        /// <summary>
        /// NIGHT17: enemy `id`'s reaction this frame (render only; the hit shapes never move): the flash share (the eased
        /// hit flash; the web's flag without a hit record), an offset for its root (the pop away from the shot plus the
        /// vibration across the screen; m, Unity space), a squash along the hit axis in the root's own axes (`rotation`:
        /// the root's) and a tilt (the top tips away from the shot), to put before the root's rotation.
        /// NIGHT18: FeelAB on, ReactN18 (the body's spring); off, ReactN17 (NIGHT17's, exactly).
        /// </summary>
        public float React(int id, Quaternion rotation, bool webFlash, out Vector3 offset, out Vector3 squash, out Quaternion tilt)
        {
            float flash = FeelAB.On ? ReactN18(id, webFlash, out offset, out squash, out tilt) : ReactN17(id, rotation, webFlash, out offset, out squash, out tilt);
            Fusion43React(id, ref offset, ref squash); // NIGHT43 A1 (F1's ask): the FUSION victim shakes and squashes through the freeze (EnemyViews.Fusion43.cs)
            Nav43React(id, ref offset, ref squash); // NIGHT43 A1 round 2: E1's stuck tell gathers it, the relocation springs it up (EnemyViews.Nav43.cs)
            Zone47React(id, ref offset, ref squash, ref tilt); // NIGHT47 F R4 hitzone47 (EnemyViews.Zone47.cs): head snaps back, legs buckle, a side twists
            Spring72React(id, ref squash); // NIGHT72 B4 spring72 (EnemyViews.Spring72.cs): idle breath, wind-up squash, flinch follow-through, volume kept (nothing drawn with the row off)
            NavFlicker43 += Burn43Flicker(id); // NIGHT43 A1 round 2: a burning enemy's visor gutters (EnemyViews.Burn43.cs; 0 until Burning43 is wired)
            return flash;
        }

        /// <summary>
        /// NIGHT18 (FeelAB on; M5): enemy `id`'s reaction from its hit slot's HitReactBody: the offset (the sideways kick along
        /// the shooter's right, NIGHT17's knock-back and vibration), the squash in the root's own axes (y down, x and z out by
        /// HitReactBulge; the spring's overshoot stretches it) and the lean (the top toward the kick, about the feet). The
        /// flash is NIGHT17's; REDUCED MOTION / EFFECTS 0 draw the flash only.
        /// </summary>
        float ReactN18(int id, bool webFlash, out Vector3 offset, out Vector3 squash, out Quaternion tilt)
        {
            offset = Vector3.zero;
            squash = Vector3.one;
            tilt = Quaternion.identity;
            float flash = webFlash ? 1 : 0;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].id != id) continue;
                flash = CrunchMath.HitFlash(hits[i].flashAge, UnityTuning.CrunchHitFlashHold, UnityTuning.CrunchHitFlashFade);
                MovementConfig mc = game.cfg.movement;
                if (!FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity)) return flash;
                float vib = hits[i].vibAge;
                float vr = CrunchMath.Vib(vib, UnityTuning.EnemyVibTime, UnityTuning.CrunchVibAmp, UnityTuning.CrunchVibHz);
                float vu = CrunchMath.Vib(vib + 0.25f / (UnityTuning.CrunchVibHz * UnityTuning.CrunchVibUpRatio), UnityTuning.EnemyVibTime,
                    UnityTuning.CrunchVibAmp * UnityTuning.CrunchVibUpShare, UnityTuning.CrunchVibHz * UnityTuning.CrunchVibUpRatio);
                float kb = hits[i].knock > 0 ? hits[i].knock * CrunchMath.Tilt(hits[i].knockAge, UnityTuning.CrunchKnockIn, UnityTuning.CrunchKnockOut) : 0;
                ref HitReactBody body = ref hits[i].body;
                float range = hits[i].reactRange; // pass 2: the same screen size from 3 m to 24 m
                // NIGHT51 flinch51: kills42's per-weapon flinch (the KESTREL 1.35, the THUNDERCLAP 1.7, the STITCHER 0.8, the gauntlet 2.2) reached only
                // NIGHT17's path (tiltScale); on the live path it scales the kick, the squash and the lean, and the stagger by its square root (capped)
                float f51 = Night51.On && hits[i].flinch51 > 0 ? hits[i].flinch51 : 1f;
                float lat = body.Lateral * range * f51, sq = body.Squash * range * f51, lean = body.LeanDeg * range * f51;
                offset = hits[i].latAxis * lat + hits[i].dir * kb + screenRight * vr + screenUp * vu;
                if (sq != 0) squash = new Vector3(1 + sq * UnityTuning.HitReactBulge, 1 - sq, 1 + sq * UnityTuning.HitReactBulge);
                if (lean != 0)
                {
                    Vector3 axis = Vector3.Cross(Vector3.up, hits[i].latAxis); // a positive turn about it tips the top toward latAxis
                    if (axis.sqrMagnitude > 1e-6f) tilt = Quaternion.AngleAxis(lean, axis);
                }
                float s36 = body.Squash36, tw36 = body.Twist36Deg; // NIGHT36 crunch36 (n36-r2): the head-on read, not range-scaled
                if (s36 != 0) squash = Vector3.Scale(squash, new Vector3(1 + s36 * UnityTuning.Crunch36Bulge, 1 - s36, 1 + s36 * UnityTuning.Crunch36Bulge));
                if (tw36 != 0) tilt = Quaternion.AngleAxis(tw36, Vector3.up) * tilt;
                if (Mathf.Abs(tw36) > TwistMax36) TwistMax36 = Mathf.Abs(tw36);
                if (s36 > SquashMax36) SquashMax36 = s36;
                float st = body.Stagger36Deg; // NIGHT36 crunch36: the top tips back along the shot (0 with the row off)
                if (f51 != 1f && st != 0) st = Mathf.Sign(st) * Mathf.Min(Mathf.Abs(st) * Mathf.Sqrt(f51), UnityTuning.Stagger36CapDeg); // NIGHT51 flinch51
                if (st != 0)
                {
                    Vector3 back = Vector3.Cross(Vector3.up, hits[i].dir); // a positive turn about it tips the top toward dir (away from the shooter)
                    if (back.sqrMagnitude > 1e-6f) tilt = Quaternion.AngleAxis(st, back) * tilt;
                    if (st > StaggerMax36) StaggerMax36 = st;
                }
                return flash;
            }
            return flash;
        }

        /// <summary>NIGHT36 crunch36 (the reels): the largest stagger tip drawn since the reel reset it (deg).</summary>
        public static float StaggerMax36;

        /// <summary>NIGHT36 crunch36 (the reels): the largest twist (deg) and squash (share) drawn since the reel reset them.</summary>
        public static float TwistMax36, SquashMax36;

        /// <summary>NIGHT17's React, exactly (FeelAB off): the tip / pop away from the shot, the squash along it, the vibration, the knock-back.</summary>
        float ReactN17(int id, Quaternion rotation, bool webFlash, out Vector3 offset, out Vector3 squash, out Quaternion tilt)
        {
            offset = Vector3.zero;
            squash = Vector3.one;
            tilt = Quaternion.identity;
            float flash = webFlash ? 1 : 0;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].id != id) continue;
                // NIGHT17 (the b1 hit strip: the web's 80 ms binary white held 8 frames, then cut): the eased flash from the hit, white for
                // CrunchHitFlashHold, back over CrunchHitFlashFade (world time: it holds through a freeze); the web's flag only without a record
                flash = CrunchMath.HitFlash(hits[i].flashAge, UnityTuning.CrunchHitFlashHold, UnityTuning.CrunchHitFlashFade);
                // NIGHT16 polish #43: a port-only motion flourish, so REDUCED MOTION (and EFFECTS INTENSITY 0) stop it, as
                // CameraFeel's do; the hit flash and the web's stagger stay
                MovementConfig mc = game.cfg.movement;
                if (!FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity)) return flash;
                float k = Flinch(hits[i].flinchAge) * hits[i].tiltScale;
                float vib = hits[i].vibAge;
                float vr = CrunchMath.Vib(vib, UnityTuning.EnemyVibTime, UnityTuning.CrunchVibAmp, UnityTuning.CrunchVibHz);
                float vu = CrunchMath.Vib(vib + 0.25f / (UnityTuning.CrunchVibHz * UnityTuning.CrunchVibUpRatio), UnityTuning.EnemyVibTime,
                    UnityTuning.CrunchVibAmp * UnityTuning.CrunchVibUpShare, UnityTuning.CrunchVibHz * UnityTuning.CrunchVibUpRatio);
                float kb = hits[i].knock > 0 ? hits[i].knock * CrunchMath.Tilt(hits[i].knockAge, UnityTuning.CrunchKnockIn, UnityTuning.CrunchKnockOut) : 0;
                offset = hits[i].dir * (UnityTuning.CrunchPush * k + kb) + screenRight * vr + screenUp * vu;
                if (k > 0)
                {
                    Vector3 local = Quaternion.Inverse(rotation) * hits[i].dir;
                    float s = UnityTuning.EnemyFlinchSquash * k;
                    squash = new Vector3(1 - s * Mathf.Abs(local.x), 1 - s * Mathf.Abs(local.y), 1 - s * Mathf.Abs(local.z));
                    Vector3 axis = Vector3.Cross(Vector3.up, hits[i].dir);
                    if (axis.sqrMagnitude > 1e-6f) tilt = Quaternion.AngleAxis(UnityTuning.CrunchTiltDeg * k, axis);
                }
                return flash;
            }
            return flash;
        }

        bool warmed; // NIGHT17 round 2: the warm-up ran

        /// <summary>NIGHT17 round 2: one tiny draw of the notch's path (the body material, its property block, the cube).</summary>
        void WarmNotch(Camera cam)
        {
            if (!ok) return;
            notchMesh ??= chunks.Cube;
            notchBlock.SetColor(BaseColor, Color.black);
            notchBlock.SetFloat(Flash, 0);
            notchBlock.SetFloat(RimOn, 0);
            notchBlock.SetFloat(Dissolve, 1);
            notchParams.worldBounds = CrunchWarm.Bounds(cam);
            Graphics.RenderMesh(notchParams, notchMesh, 0, CrunchWarm.Matrix(cam));
        }

        /// <summary>NIGHT17: an enemy that shed a block keeps a dark notch where it was (INK), drawn with its root (`root`).</summary>
        public void Notch(int id, Transform root)
        {
            if (!ok || notchMesh == null) return;
            for (int i = 0; i < sheds.Length; i++)
            {
                if (sheds[i].id != id) continue;
                notchBlock.SetColor(BaseColor, UiTheme.Ready ? UiTheme.InkSolid : Color.black);
                notchBlock.SetFloat(Flash, 0);
                notchBlock.SetFloat(RimOn, 0);
                notchBlock.SetFloat(Dissolve, 1);
                Matrix4x4 m = root.localToWorldMatrix * Matrix4x4.TRS(sheds[i].local, Quaternion.identity,
                    new Vector3(UnityTuning.CrunchShedSize, UnityTuning.CrunchShedSize, UnityTuning.CrunchShedSize));
                notchParams.worldBounds = new Bounds(new Vector3(m.m03, m.m13, m.m23), Vector3.one);
                Graphics.RenderMesh(notchParams, notchMesh, 0, m);
                return;
            }
        }

        /// <summary>NIGHT18 (moved from Frame): the hit timers, once a frame (the flinch on world time; the vibration, the flash and
        /// the knock-back on frame time while the run simulates).</summary>
        void FrameHits(float vibDt, float worldDt)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                hits[i].flinchAge += worldDt;
                hits[i].vibAge += vibDt;
                hits[i].flashAge += vibDt; // NIGHT17 round 1: frame time (a long freeze no longer holds a static white blob)
                hits[i].knockAge += vibDt; // the knock-back plays through a freeze
                hits[i].body.Advance(vibDt); // NIGHT18: the body's spring, frame time (it plays through a hit's 0-2 held steps; paused, it holds)
            }
            // NIGHT18 round 1 fix: the hit sparks age, move and draw (frame time); made on the first frame with a camera (TITLE)
            if (game.cam != null)
            {
                hitSparks ??= new HitSparks(game);
                if (game.flow == Flow.Title) hitSparks.Clear();
                hitSparks.Frame(vibDt, game.cam);
            }
        }

        /// <summary>NIGHT18 (moved from Clear): a new run has no hit timers, sheds or queued shed blocks.</summary>
        void ClearHits()
        {
            for (int i = 0; i < hits.Length; i++) hits[i] = new Hit { id = NoEnemy, flinchAge = Idle, vibAge = Idle, flashAge = Idle, body = HitReactBody.Rest, latAxis = Vector3.right, reactRange = 1 };
            for (int i = 0; i < sheds.Length; i++) sheds[i].id = NoEnemy;
            shedAskCount = 0;
            hitSparks?.Clear(); // NIGHT18 round 1 fix
        }

        /// <summary>NIGHT17: a dead enemy's notch goes (its id may come back on a recycled enemy).</summary>
        void ForgetShed(int id)
        {
            for (int i = 0; i < sheds.Length; i++) if (sheds[i].id == id) sheds[i].id = NoEnemy;
        }
    }
}
