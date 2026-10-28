// NIGHT47 builder F, R4 idea 2: hitzone47 (a view row, design.n47.hitzone47; NIGHT47.md section 6's list: "enemy hit reactions that read the hit's
// direction"). Today (NIGHT18 / NIGHT36, EnemyViews.Hit.cs ReactN18): a hit kicks the body sideways along the shooter's right, squashes it, leans
// its top toward the kick and (crunch36) tips it back along the shot: the same read wherever the hit lands. Research (Research/2026-10-18-wave47/
// F.md R13, R14): direction-based hit reactions pick a pose from the hit's place on the body (8 directions + a centred one, a 10-frame pose on an
// upper-body layer so the body keeps moving: Verified, swearsoft's Jack McLantern devlog); feedback lands with the hit, "even slight delays
// diminish the impact" (Verified, Wayline). With the row on, a player hit is read by WHERE it lands (its height on the body and its side of the
// body's middle as the shooter sees it), on top of today's reaction, for UnityTuning.Zone47 s of frame time (in, then out):
//   - HEAD (the top Zone47HeadFrom of the body): the body snaps back away from the shot, its top tipping Zone47HeadDeg about the feet;
//   - LEGS (the bottom Zone47LegsTo): the knees buckle: the body squashes Zone47LegsSquash in y (x / z out by half of it), as a stumble;
//   - SIDE (a hit further than Zone47SideFrom of the body's radius off its middle, between the two): the body twists Zone47TwistDeg away from it.
// Every view that reacts through EnemyLook.React (husk, swarmer, spitter, charger, drone, warden, burrower) shows it. Render only; no sim state.
// REDUCED MOTION / EFFECTS 0: none (FxGates.MotionOn). Off: nothing (React unchanged). Numbers: UnityTuning.F47.cs Zone47*.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class EnemyLook
    {
        struct Zone47Hit
        {
            public int id;
            public float age; // s of frame time (-1: free)
            public byte zone; // 0 head, 1 legs, 2 side
            public float side; // -1 the shooter's left of the middle, +1 its right
            public Vector3 dir; // the shot's flat direction (shooter -> body)
        }

        readonly Zone47Hit[] zone47 = new Zone47Hit[UnityTuning.Zone47Slots];
        int zone47Next, zone47Frame = -1;

        /// <summary>For the reel: the last zone read (0 head, 1 legs, 2 side, -1 none) and the reactions started.</summary>
        public static int Zone47Last = -1, Zone47Count;

        static GameConfig zone47For;
        static bool zone47On;
        public static bool? Zone47Force;

        public static bool Zone47RowOn(GameConfig cfg)
        {
            if (Zone47Force.HasValue) return Zone47Force.Value;
            if (!ReferenceEquals(cfg, zone47For)) { zone47For = cfg; zone47On = cfg != null && NightRows.ViewOn(cfg, "hitzone47"); }
            return zone47On;
        }

        void Zone47Init()
        {
            for (int i = 0; i < zone47.Length; i++) zone47[i].age = -1;
            game.events.On<HitEvent>("hit", e => { if (e.owner == "player" && Zone47RowOn(game.cfg)) Zone47OnHit(e); });
            game.OnNewRun += () => { for (int i = 0; i < zone47.Length; i++) zone47[i].age = -1; };
        }

        void Zone47OnHit(HitEvent e)
        {
            Enemy body = null;
            foreach (Enemy en in game.enemies.all) if (en.id == e.enemy && en.alive) { body = en; break; }
            if (body == null || game.cam == null || e.killed) return; // a kill breaks the body (Chunks): nothing to react
            float h = (float)Mathf.Max(0.2f, (float)body.Height);
            float rel = Mathf.Clamp01((float)(e.position.y - body.position.y) / h);
            Vector3 eye = game.cam.transform.position;
            Vector3 at = WebSpace.ToUnity(e.position), mid = WebSpace.ToUnity(new Vec3(body.position.x, e.position.y, body.position.z));
            Vector3 dir = mid - eye;
            dir.y = 0;
            if (dir.sqrMagnitude < 1e-6f) return;
            dir.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, dir); // the shooter's right across the body
            float off = Vector3.Dot(at - mid, right) / Mathf.Max(0.1f, (float)body.Radius);
            byte zone;
            if (rel >= 1f - UnityTuning.Zone47HeadFrom || e.headshot) zone = 0;
            else if (rel <= UnityTuning.Zone47LegsTo) zone = 1;
            else if (Mathf.Abs(off) >= UnityTuning.Zone47SideFrom) zone = 2;
            else return; // the middle: today's reaction says it
            zone47[zone47Next] = new Zone47Hit { id = e.enemy, age = 0, zone = zone, side = Mathf.Sign(off), dir = dir };
            zone47Next = (zone47Next + 1) % zone47.Length;
            Zone47Last = zone;
            Zone47Count++;
        }

        /// <summary>From React (after today's reaction): the zone's pose added to this enemy's offset / squash / tilt.</summary>
        void Zone47React(int id, ref Vector3 offset, ref Vector3 squash, ref Quaternion tilt)
        {
            if (!Zone47RowOn(game.cfg)) return;
            if (zone47Frame != Time.frameCount)
            {
                zone47Frame = Time.frameCount;
                float dt = FeelClock.UnscaledDelta; // through the hitstop's freeze (NIGHT63 D1: 0 in a same-run held frame) (the reaction is the hit's, as the FUSION's shake)
                for (int i = 0; i < zone47.Length; i++)
                {
                    if (zone47[i].age < 0) continue;
                    zone47[i].age += dt;
                    if (zone47[i].age > UnityTuning.Zone47InS + UnityTuning.Zone47OutS) zone47[i].age = -1;
                }
            }
            MovementConfig mc = game.cfg.movement;
            if (!FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity)) return;
            for (int i = 0; i < zone47.Length; i++)
            {
                if (zone47[i].id != id || zone47[i].age < 0) continue;
                float t = zone47[i].age;
                float k = t < UnityTuning.Zone47InS ? t / UnityTuning.Zone47InS : 1f - (t - UnityTuning.Zone47InS) / UnityTuning.Zone47OutS;
                k = Mathf.Clamp01(k);
                k = 1f - (1f - k) * (1f - k); // eased: a snap in, a settle out
                Vector3 dir = zone47[i].dir;
                switch (zone47[i].zone)
                {
                    case 0:
                        {
                            Vector3 axis = Vector3.Cross(Vector3.up, dir); // a positive turn tips the top toward dir (away from the shooter)
                            tilt = Quaternion.AngleAxis(UnityTuning.Zone47HeadDeg * k, axis) * tilt;
                            break;
                        }
                    case 1:
                        {
                            float s = UnityTuning.Zone47LegsSquash * k;
                            squash = Vector3.Scale(squash, new Vector3(1 + s * 0.5f, 1 - s, 1 + s * 0.5f));
                            offset += dir * (UnityTuning.Zone47LegsShove * k);
                            break;
                        }
                    default:
                        tilt = Quaternion.AngleAxis(zone47[i].side * UnityTuning.Zone47TwistDeg * k, Vector3.up) * tilt; // the hit side turns away
                        break;
                }
            }
        }
    }

    public static partial class UnityTuning
    {
        // ---- NIGHT47 F R4 hitzone47 (EnemyViews.Zone47.cs): render numbers only ----
        public const int Zone47Slots = 16; // reactions live at once (a THUNDERCLAP volley's pellets on a few bodies)
        public const float Zone47HeadFrom = 0.25f; // the top this share of the body's height is its head (and every headshot)
        public const float Zone47LegsTo = 0.33f; // the bottom this share is its legs
        public const float Zone47SideFrom = 0.45f; // a hit this share of the body's radius off its middle (as the shooter sees it) is a side hit
        public const float Zone47InS = 0.05f, Zone47OutS = 0.22f; // s: a snap in (3 frames at 60 fps), a settle out (the pose's 10 frames, Verified R13)
        public const float Zone47HeadDeg = 11f; // deg the top tips back on a head hit (crunch36's stagger tips 6-8: the head reads past it)
        public const float Zone47LegsSquash = 0.14f; // the knee buckle's y squash share (x / z out by half)
        public const float Zone47LegsShove = 0.05f; // m the body's feet slide along the shot with it
        public const float Zone47TwistDeg = 14f; // deg the body twists away from a side hit
    }
}
