// NIGHT43 builder A1 (burrow43; NIGHT43.md card A1, G28: "its look: a segmented orange / yellow body, a ringed mouth, readable from above"): what
// the player sees of a BURROWER (Core/Enemies/Burrower43.cs), interpolated on frame time, pooled (no allocation per frame):
// - BURIED (hunt / rest / tell): a raised TRAIL on the floor: the head lump (a dark dome with the enemy rim) where its head is, five smaller
//   lumps behind it along the way it came (a history of head positions every TrailGap m), dust bursts as it travels; in the TELL the head
//   lump shakes hard and swells, and two RED-ORANGE rings mark the spot: the outer at the emergence's reach (emergeRadius: step out of it),
//   the inner closing from it to the centre over the tell (time to impact). Its sound is GG's: burrowTell43 (the rumble's cue).
// - OUT (dig / emerge / dive): the body rears out of the floor: five segments (dark and bright orange bands, curling toward the player), a
//   ringed MOUTH glowing amber (the visor's role: the brightest thing on it), bone fangs; it rises in 0.12 s at the emergence, sinks over the
//   dig and the dive. In the COUNTER window the mouth burns hotter and a CYAN ring lies at its base (hit it NOW: the ICEBREAKER's stuck-fist
//   language); COUNTERED it reels back, the mouth gutters and the fangs pulse (the stagger). Hit flash, flinch and FUSION shake via EnemyLook;
//   a kill breaks every part into blocks (EnemyLook.Die). Hooked from the charger view's init / render (EnemyViews.Charger.cs, A1's file).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        BurrowerView burrowerView;

        /// <summary>NIGHT43 A1: the cube particles (the burrower's dust).</summary>
        internal Particles Particles43 => particles;
    }

    public sealed class BurrowerView
    {
        sealed class Body
        {
            public Transform root, body;
            public Renderer[] rends = new Renderer[1];
            public Color[] colors = new Color[1];
            public Enemy40Parts parts;
            public Transform[] lumps = new Transform[UnityTuning.Burrow43Lumps];
            public Renderer[] lumpRends = new Renderer[UnityTuning.Burrow43Lumps];
            public Transform ringOuter, ringInner, ringWindow;
            public Renderer ringOuterR, ringInnerR, ringWindowR;
            public readonly Vector3[] hist = new Vector3[UnityTuning.Burrow43Lumps * 2];
            public int histCount;
            public Vector3 lastHist;
            public float dust;
            public Burrower43 drawn;
            public int drawnId = int.MinValue;
        }

        static readonly float[] Rims = { 1 };
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly List<Body> pool = new List<Body>();
        int drawnCount;
        readonly Mesh[] meshes;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Transform parent;
        readonly Color enemy, dirt, dust, danger, usable, parry, bone;

        public BurrowerView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game);
            parent = new GameObject("Burrowers").transform;
            enemy = Look.C(cfg, "colors.enemy");
            dirt = Look.Hex(UnityTuning.Burrow43DirtHex);
            dust = Look.C(cfg, "colors.slamDust");
            danger = Look.Hex(UnityTuning.Tell36RedHex);
            usable = Look.C(cfg, "colors.usable");
            parry = Look.Hex(UnityTuning.Tell36YellowHex);
            bone = Look.Hex(UnityTuning.Models40WeakHex);
            meshes = new[] { Mesh43.Get(Mesh43.Segment) };
            game.events.On<KillEvent>("kill", OnKill);
        }

        void OnKill(KillEvent e)
        {
            for (int i = 0; i < drawnCount; i++)
            {
                Body b = pool[i];
                if (b.drawn == null || b.drawn.id != e.enemy) continue;
                look.Die(b.drawn, e, b.rends, meshes, b.colors, Rims, 1, -1, b.parts);
                return;
            }
        }

        Renderer Part(Transform at, string name, Mesh mesh, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(at, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            look.Attach(r, false, false);
            t = g.transform;
            return r;
        }

        Body Make()
        {
            var b = new Body();
            b.root = new GameObject("burrower").transform;
            b.root.SetParent(parent, false);
            b.body = new GameObject("body").transform;
            b.body.SetParent(b.root, false);
            float h = (float)game.husks.burrowBody43.height;
            b.rends[0] = Part(b.body, "segment 0", Mesh43.Get(Mesh43.Segment), out Transform s0);
            s0.localPosition = new Vector3(0, h * 0.11f, 0);
            s0.localScale = new Vector3(1.15f, 0.55f, 1.15f);
            look.Attach(b.rends[0], true, false); // the x-ray on the body
            b.parts = Enemy40Parts.Make43("burrower", UnityTuning.Models43Burrower, b.body, new[] { s0 }, look, h);
            for (int i = 0; i < b.lumps.Length; i++)
            {
                b.lumpRends[i] = Part(parent, "burrow trail " + i, Mesh43.Get(Mesh43.Pauldron), out b.lumps[i]);
                b.lumps[i].gameObject.SetActive(false);
            }
            b.ringOuterR = Part(parent, "burrow ring", Mesh43.Get(Mesh43.Ring), out b.ringOuter);
            b.ringInnerR = Part(parent, "burrow ring inner", Mesh43.Get(Mesh43.Ring), out b.ringInner);
            b.ringWindowR = Part(parent, "burrow window", Mesh43.Get(Mesh43.Ring), out b.ringWindow);
            b.ringOuter.gameObject.SetActive(false);
            b.ringInner.gameObject.SetActive(false);
            b.ringWindow.gameObject.SetActive(false);
            return b;
        }

        static void Show(Transform t, bool on)
        {
            if (t.gameObject.activeSelf != on) t.gameObject.SetActive(on);
        }

        public void Render(double alpha, double frameDt)
        {
            if (game.husks == null) return;
            look.Frame();
            int n = 0;
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Burrower43 w) || !w.alive) continue;
                if (n >= pool.Count) pool.Add(Make());
                Body b = pool[n++];
                if (b.drawnId != w.id)
                {
                    // a new burrower in this slot: its trail starts where it is
                    b.drawnId = w.id;
                    b.histCount = 0;
                    b.lastHist = WebSpace.ToUnity(w.trail);
                    b.dust = 0;
                }
                b.drawn = w;
                w.flash = System.Math.Max(0, w.flash - frameDt);
                w.drawAge += frameDt;
                float age = (float)w.drawAge;
                Burrow43Params rules = w.Rules;
                Vector3 head = WebSpace.ToUnity(w.trailPrevious + (w.trail - w.trailPrevious) * alpha);
                bool buried = w.Buried, telling = w.state == Burrower43.Tell;
                // the trail: a history of the head's way
                if ((head - b.lastHist).sqrMagnitude >= UnityTuning.Burrow43TrailGap * UnityTuning.Burrow43TrailGap)
                {
                    for (int k = b.hist.Length - 1; k > 0; k--) b.hist[k] = b.hist[k - 1];
                    b.hist[0] = b.lastHist;
                    b.histCount = Mathf.Min(b.hist.Length, b.histCount + 1);
                    b.lastHist = head;
                    if (buried)
                    {
                        b.dust += 1;
                        if (b.dust >= UnityTuning.Burrow43DustEvery)
                        {
                            b.dust = 0;
                            game.Particles43?.Burst(new Vec3(w.trail.x, w.trail.y + 0.1, w.trail.z), dust, UnityTuning.Burrow43DustCount, 2.5f, 0.45f, 0.8f);
                        }
                    }
                }
                for (int k = 0; k < b.lumps.Length; k++)
                {
                    bool on = buried && (k == 0 || k <= b.histCount);
                    Show(b.lumps[k], on);
                    if (!on) continue;
                    Vector3 at = k == 0 ? head : b.hist[System.Math.Min(k - 1, b.histCount - 1)];
                    float size = k == 0 ? 1 : 1 - k * UnityTuning.Burrow43LumpShrink;
                    float bob = Mathf.Sin(age * 9 + k * 1.3f) * 0.04f;
                    Vector3 jit = Vector3.zero;
                    if (k == 0 && telling)
                    {
                        size *= UnityTuning.Burrow43TellSwell;
                        jit = new Vector3(Mathf.Sin(age * 61), 0, Mathf.Cos(age * 53)) * UnityTuning.Burrow43TellShake;
                    }
                    b.lumps[k].position = at + jit + Vector3.up * (0.02f + bob);
                    b.lumps[k].localScale = new Vector3(UnityTuning.Burrow43LumpSize * size, UnityTuning.Burrow43LumpHeight * size, UnityTuning.Burrow43LumpSize * size);
                    Color lc = Color.Lerp(dirt, danger, k == 0 && telling ? 0.35f + 0.25f * Mathf.Sin(age * 30) : 0);
                    look.Tint(block, b.lumpRends[k], lc, 0, 1);
                }
                // the tell's rings at the locked spot
                Show(b.ringOuter, telling);
                Show(b.ringInner, telling);
                if (telling)
                {
                    float p = (float)w.StateProgress;
                    Vector3 spot = WebSpace.ToUnity(w.tellAt) + Vector3.up * 0.03f;
                    float r = (float)rules.emergeRadius * 2; // Mesh43.Ring's outer radius is 0.5
                    b.ringOuter.position = spot;
                    b.ringOuter.localScale = new Vector3(r, UnityTuning.Burrow43RingFlat, r);
                    float ri = r * Mathf.Lerp(1, 0.12f, p);
                    b.ringInner.position = spot;
                    b.ringInner.localScale = new Vector3(ri, UnityTuning.Burrow43RingFlat, ri);
                    look.Tint(block, b.ringOuterR, danger * (UnityTuning.Burrow43RingGlow + Mathf.Sin(age * 24) * 0.5f), 0, 0);
                    look.Tint(block, b.ringInnerR, Color.Lerp(danger, Color.white, p * 0.5f) * UnityTuning.Burrow43RingGlow, 0, 0);
                }
                // the body out of the ground
                bool out_ = !buried;
                Show(b.root, out_);
                bool window = w.state == Burrower43.Emerge && w.windowSteps > 0;
                Show(b.ringWindow, out_ && window);
                if (!out_) continue;
                float h = (float)w.Height;
                float rise;
                double prog = w.StateProgress;
                if (w.state == Burrower43.Emerge) rise = Mathf.Clamp01((float)(prog * w.stateTotal / 120.0) / UnityTuning.Burrow43RiseTime);
                else rise = 1 - (float)prog; // dig and dive sink
                if (w.state == Burrower43.Emerge && w.countered) rise = 1;
                Vec3 pp = w.previous + (w.position - w.previous) * alpha;
                Quaternion yawQ = Quaternion.Euler(0, -(float)(w.yaw * Mathf.Rad2Deg), 0);
                float flash = look.React(w.id, yawQ, w.flash > 0, out Vector3 off, out Vector3 squash, out Quaternion tilt);
                bool reeling = w.staggerSteps > 0;
                float sway = Mathf.Sin(age * 3.1f) * UnityTuning.Burrow43SwayDeg;
                Quaternion lean = Quaternion.Euler(reeling ? -UnityTuning.Burrow43ReelDeg : 0, 0, sway);
                b.root.position = WebSpace.ToUnity(pp) + off;
                b.root.rotation = tilt * yawQ;
                b.root.localScale = squash;
                b.body.localPosition = new Vector3(0, -(1 - rise) * h, 0);
                b.body.localRotation = lean;
                Color bc = enemy * UnityTuning.Models43BodyValue;
                bc.a = 1;
                b.colors[0] = bc;
                look.Tint(block, b.rends[0], bc, flash, 1);
                b.parts.Pose(look, block, bc, flash, window ? 1 : 0, reeling, danger, parry, age);
                look.Notch(w.id, b.root);
                FeelProbe.Enemy(w.id, b.rends[0].bounds, b.parts.rends[b.parts.rends.Length - 2].bounds, flash);
                if (window)
                {
                    b.ringWindow.position = WebSpace.ToUnity(pp) + Vector3.up * 0.04f;
                    float rw = (float)w.Radius * 2 * UnityTuning.Burrow43WindowRing;
                    b.ringWindow.localScale = new Vector3(rw, UnityTuning.Burrow43RingFlat, rw);
                    look.Tint(block, b.ringWindowR, usable * (UnityTuning.Burrow43RingGlow + Mathf.Sin(age * 30) * 0.6f), 0, 0);
                }
            }
            drawnCount = n;
            for (int i = n; i < pool.Count; i++)
            {
                Body b = pool[i];
                Show(b.root, false);
                for (int k = 0; k < b.lumps.Length; k++) Show(b.lumps[k], false);
                Show(b.ringOuter, false);
                Show(b.ringInner, false);
                Show(b.ringWindow, false);
                b.drawn = null;
                b.drawnId = int.MinValue;
            }
        }
    }

    public static partial class UnityTuning
    {
        // NIGHT43 A1 burrow43's view (render only: every gameplay number is design.n43.burrow43's).
        public const int Burrow43Lumps = 6; // the trail: the head lump + five behind it
        public const float Burrow43TrailGap = 0.45f; // m of head travel between two trail lumps
        public const float Burrow43LumpSize = 0.95f; // the head lump's width (m; the Pauldron dome's cube space)
        public const float Burrow43LumpHeight = 0.42f; // ... and height (a mound, readable from above and at a low angle)
        public const float Burrow43LumpShrink = 0.13f; // each lump behind is this much smaller (the trail tapers)
        public const uint Burrow43DirtHex = 0x4A2416; // the trail's mounds: a dark scorched red-brown (the enemy rim picks out its edge: an enemy, not floor)
        public const int Burrow43DustEvery = 2; // a dust burst every this many trail lumps laid
        public const int Burrow43DustCount = 4; // cubes per burst (colors.slamDust)
        public const float Burrow43TellSwell = 1.35f; // the head lump in the tell: x this
        public const float Burrow43TellShake = 0.07f; // ... and it shakes this far (m)
        public const float Burrow43RingFlat = 0.06f; // the floor rings' height scale (flat: Mesh43.Ring's tube is 0.1 of its size)
        public const float Burrow43RingGlow = 2.2f; // the rings' HDR glow (x their colour)
        public const float Burrow43RiseTime = 0.12f; // s the body takes to burst out at the emergence
        public const float Burrow43SwayDeg = 5f; // the body's idle sway (deg)
        public const float Burrow43ReelDeg = 28f; // countered: it reels back this far (deg)
        public const float Burrow43WindowRing = 1.7f; // the counter window's cyan ring: x the body's diameter

        // its body (the root at its feet on the floor; y x its height 2.2 m): four segments curling toward the player over the base segment (the
        // view's own renderer), dark and bright bands (the card's orange / yellow: TRIM is the enemy orange brightened), the ringed MOUTH (VISOR:
        // the brightest thing on it, hotter in the counter window), bone FANGS (WEAK: they pulse while it reels)
        public static readonly float[] Models43Burrower =
        {
            Mesh43.Segment, 0, 0, 0.28f, 0.05f, 1.1f, 0.52f, 1.1f, 6, 0, 0, 6, 0, 0, 0,
            Mesh43.Segment, 0, 0, 0.445f, 0.14f, 1.04f, 0.5f, 1.04f, 12, 0, 0, 0, 0, 0, 0,
            Mesh43.Segment, 0, 0, 0.605f, 0.28f, 0.98f, 0.48f, 0.98f, 20, 0, 0, 6, 0, 0, 0,
            Mesh43.Segment, 0, 0, 0.75f, 0.46f, 0.92f, 0.46f, 0.92f, 30, 0, 0, 0, 0, 0, 0,
            Mesh43.Mouth, 0, 0, 0.865f, 0.64f, 0.95f, 0.8f, 0.95f, 40, 0, 0, 5, 0, 0, 0.12f,
            Mesh43.Teeth, 0, 0, 0.865f, 0.64f, 0.95f, 0.8f, 0.95f, 40, 0, 0, 4, 0, 0, 0,
        };
    }
}
