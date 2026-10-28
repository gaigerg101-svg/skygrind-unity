// NIGHT20 U3-34.6 (builder N owns this file): the climb's and the leap's look (design.routes; Core/Nav/RouteLinks.cs
// RouteJumper), a SurvivalGame partial through the shared commit's hooks (N20ClimbViewInit / N20ClimbViewRender). Port-only:
// the web's husks have no climb (POWER.md P1). Nothing new about the bodies (CLAUDE.md rule 7: the husk, swarmer and
// spitter views draw them as the web does); this view adds, to a body whose jump state is the layer's:
// - a CLIMB: two arms from its shoulders up the face, hand over hand while it climbs (the body faces the face: the jumper
//   turns it), the hands on the lip for the mantle; the hands (claws) pulse in the DANGER role (colors.enemy toward
//   husk.windupColor, the husk's own wind-up tell) from the tell's first frame to the top, and a DANGER bar on the lip's
//   edge marks where it comes over (the charger's painted line's language): the player on the perch reads it >= 0.5 s
//   before the climber's top (climbTell + rise / climbSpeed + mantle >= 1.3 s);
// - the LEAP's crouch (its DANGER tell): the arms down and forward, the claws pulsing, and a DANGER bar on the landing;
//   nothing in the flight.
// FeelProbe: FeelFx.Climb, the climber's body with its arms, claws and bar, every frame of a climb or a leap's crouch (the
// reel's `climb` scenario reads the tell's first frame against the top). No allocation per frame (pooled rigs, one
// property block, structs).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        ClimbView climbView;

        partial void N20ClimbViewInit() => climbView = new ClimbView(this);

        partial void N20ClimbViewRender(double alpha, double frameDt) => climbView?.Render(alpha, frameDt);
    }

    public sealed class ClimbView
    {
        sealed class Rig
        {
            public Transform root;
            public readonly Transform[] arms = new Transform[2], claws = new Transform[2];
            public readonly Renderer[] armRends = new Renderer[2], clawRends = new Renderer[2];
            public readonly Transform[] legs = new Transform[6]; // a swarmer's wall-crawl (row 5): three legs a side on the face
            public readonly Renderer[] legRends = new Renderer[6];
            public Transform bar;
            public Renderer barRend;
            public Husk drawn;
            public Vec3 key; // the verb's start (a new climb or leap restarts the pulse)
            public double age;
        }

        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly List<Rig> pool = new List<Rig>();
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock(), barBlock = new MaterialPropertyBlock();
        readonly Material barMat;
        readonly Transform parent;
        readonly Color enemy, tell;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public ClimbView(SurvivalGame game)
        {
            this.game = game;
            look = EnemyLook.For(game);
            parent = new GameObject("Climbs").transform;
            enemy = Look.C(game.cfg, "colors.enemy"); // the DANGER role (UiTheme.Danger's source)
            tell = Look.C(game.cfg, "husk.windupColor"); // the husk's own wind-up tell
            barMat = new Material(game.mats.neon) { name = "Climb lip bar" };
        }

        Transform Cube(Transform under, out Renderer r)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(under, false);
            r = go.GetComponent<Renderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            return go.transform;
        }

        Rig Make()
        {
            var b = new Rig { root = new GameObject("climb").transform };
            b.root.SetParent(parent, false);
            for (int i = 0; i < 2; i++)
            {
                b.arms[i] = Cube(b.root, out b.armRends[i]);
                look.Attach(b.armRends[i], false, false);
                b.claws[i] = Cube(b.root, out b.clawRends[i]);
                look.Attach(b.clawRends[i], false, false);
                b.claws[i].localScale = Vector3.one * UnityTuning.ClimbClawSize;
            }
            for (int i = 0; i < 6; i++)
            {
                b.legs[i] = Cube(b.root, out b.legRends[i]);
                look.Attach(b.legRends[i], false, false);
            }
            b.bar = Cube(b.root, out b.barRend);
            b.barRend.sharedMaterial = barMat;
            b.barRend.receiveShadows = false;
            return b;
        }

        /// <summary>
        /// Row 5 (DRG's Glyphids scale walls as their own verb): a swarmer on a face crawls, it doesn't climb hand over
        /// hand: six legs splayed on the face round its body, stepping in two tripods with the height climbed (a scuttle),
        /// their feet pulsing DANGER as the husk's claws do; the arms are hidden. Before the climb (the tell) the legs reach
        /// up the face; over the lip they reach onto its top.
        /// </summary>
        void Crawl(Rig b, Husk h, NavJumpState s, Vector3 p, Color leg, Color foot)
        {
            float height = (float)h.Height, radius = (float)h.Radius;
            Vector3 wall = -WebSpace.ToUnity(s.normal);
            Vector3 side = Vector3.Cross(Vector3.up, wall).normalized;
            Vector3 body = p + Vector3.up * height * 0.5f;
            Vector3 face = wall * (radius + 0.06f - UnityTuning.ClimbWallGap);
            float climbed = (float)(h.position.y - s.foot.y);
            for (int i = 0; i < 6; i++)
            {
                int row = i / 2 - 1; // -1, 0, +1: low, middle, high
                float sgn = i % 2 == 0 ? -1 : 1;
                bool tripodA = (i % 2 == 0) == (row != 0);
                float step = s.phase == 1 ? Mathf.Sin(climbed * UnityTuning.CrawlStrideRate + (tripodA ? 0 : Mathf.PI)) * UnityTuning.CrawlStride : 0;
                float reach = s.phase == 0 ? UnityTuning.CrawlTellReach : 0;
                Vector3 hip = body + side * sgn * radius * 0.6f + Vector3.up * row * height * 0.25f;
                Vector3 toe = body + face + side * sgn * UnityTuning.CrawlSpread + Vector3.up * (row * UnityTuning.CrawlRowGap + step + reach);
                if (s.phase == 2 && row == 1) toe = new Vector3(toe.x, (float)s.to.y + 0.05f, toe.z) + wall * 0.25f; // the front legs on the lip
                Arm(b.legs[i], hip, toe);
                b.legs[i].localScale = new Vector3(UnityTuning.CrawlLegThick, UnityTuning.CrawlLegThick, b.legs[i].localScale.z);
                look.Tint(block, b.legRends[i], row == 1 ? foot : leg, 0, 1);
            }
        }

        static void Arm(Transform arm, Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            float len = d.magnitude;
            arm.position = (from + to) * 0.5f;
            arm.rotation = len > 1e-4f ? Quaternion.LookRotation(d / len) : Quaternion.identity;
            arm.localScale = new Vector3(UnityTuning.ClimbArmThick, UnityTuning.ClimbArmThick, Mathf.Max(0.05f, len));
        }

        public void Render(double alpha, double frameDt)
        {
            int n = 0;
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Husk h) || !h.alive) continue;
                NavJumpState s = h.jump;
                bool climbing = s != null && s.verb == NavJumpState.Climb;
                bool crouching = s != null && s.verb == NavJumpState.Leap && !s.flying && s.crouchSteps >= 0;
                if (!climbing && !crouching) continue;
                if (n >= pool.Count) pool.Add(Make());
                Rig b = pool[n++];
                if (b.drawn != h || b.key.x != s.foot.x || b.key.z != s.foot.z || b.key.y != s.to.y)
                {
                    b.age = 0;
                    b.drawn = h;
                    b.key = new Vec3(s.foot.x, s.to.y, s.foot.z);
                }
                b.age += frameDt;
                if (!b.root.gameObject.activeSelf) b.root.gameObject.SetActive(true);
                float wave = 0.5f + 0.5f * Mathf.Sin((float)b.age * UnityTuning.ClimbTellPulse);
                Color claw = Color.Lerp(enemy, tell, wave) * 1.6f; // DANGER, pulsing toward the wind-up tell, glowing
                claw.a = 1;
                Color arm = enemy;
                float height = (float)h.Height, radius = (float)h.Radius;
                Vec3 at = h.previous + (h.position - h.previous) * alpha;
                Vector3 p = WebSpace.ToUnity(at);
                Vector3 hl, hr, barAt, barFwd;
                float barWidth;
                if (climbing)
                {
                    Vector3 wall = -WebSpace.ToUnity(s.normal); // into the face
                    Vector3 side = Vector3.Cross(Vector3.up, wall).normalized;
                    Vector3 shoulder = p + Vector3.up * height * UnityTuning.ClimbShoulderUp;
                    Vector3 sl = shoulder - side * radius * UnityTuning.ClimbShoulderOut, sr = shoulder + side * radius * UnityTuning.ClimbShoulderOut;
                    Vector3 face = wall * (radius + 0.06f - UnityTuning.ClimbWallGap); // the face, from the body's axis (the jumper's foot gap: 0.06 m)
                    if (s.phase < 2)
                    {
                        float climbed = (float)(at.y - s.foot.y);
                        float stride = s.phase == 1 ? Mathf.Sin(climbed * UnityTuning.ClimbStrideRate) * UnityTuning.ClimbStride : 0;
                        Vector3 reach = p + Vector3.up * (height + UnityTuning.ClimbReach) + face;
                        hl = reach - side * radius * UnityTuning.ClimbShoulderOut + Vector3.up * stride;
                        hr = reach + side * radius * UnityTuning.ClimbShoulderOut - Vector3.up * stride;
                    }
                    else
                    {
                        Vector3 lip = new Vector3(p.x, (float)s.to.y + 0.05f, p.z) + face + wall * 0.3f; // onto the lip's top
                        hl = lip - side * radius * UnityTuning.ClimbShoulderOut;
                        hr = lip + side * radius * UnityTuning.ClimbShoulderOut;
                    }
                    Arm(b.arms[0], sl, hl);
                    Arm(b.arms[1], sr, hr);
                    Vector3 foot = WebSpace.ToUnity(s.foot);
                    barAt = new Vector3(foot.x, (float)s.to.y + UnityTuning.ClimbLipBarLift, foot.z) + wall * (radius + 0.06f + UnityTuning.ClimbLipBarDepth * 0.5f);
                    barFwd = wall;
                    barWidth = UnityTuning.ClimbLipBarWidth;
                }
                else
                {
                    // The leap's crouch: the arms down and forward, toward the landing.
                    Vector3 to = WebSpace.ToUnity(s.to);
                    Vector3 fwd = new Vector3(to.x - p.x, 0, to.z - p.z);
                    fwd = fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward;
                    Vector3 side = Vector3.Cross(Vector3.up, fwd).normalized;
                    Vector3 shoulder = p + Vector3.up * height * UnityTuning.ClimbShoulderUp;
                    Vector3 sl = shoulder - side * radius * UnityTuning.ClimbShoulderOut, sr = shoulder + side * radius * UnityTuning.ClimbShoulderOut;
                    Vector3 low = p + Vector3.up * Mathf.Max(0.05f, height * 0.5f - UnityTuning.LeapCrouchDrop) + fwd * (radius + 0.1f);
                    hl = low - side * radius * UnityTuning.ClimbShoulderOut;
                    hr = low + side * radius * UnityTuning.ClimbShoulderOut;
                    Arm(b.arms[0], sl, hl);
                    Arm(b.arms[1], sr, hr);
                    barAt = new Vector3(to.x, to.y + UnityTuning.ClimbLipBarLift, to.z);
                    barFwd = fwd;
                    barWidth = UnityTuning.LeapLandBarWidth;
                }
                bool crawl = climbing && h.kind == "swarmer"; // row 5: a swarmer crawls, legs on the face, no arms
                for (int i = 0; i < 2; i++)
                {
                    if (b.arms[i].gameObject.activeSelf == crawl) b.arms[i].gameObject.SetActive(!crawl);
                    if (b.claws[i].gameObject.activeSelf == crawl) b.claws[i].gameObject.SetActive(!crawl);
                }
                for (int i = 0; i < 6; i++) if (b.legs[i].gameObject.activeSelf != crawl) b.legs[i].gameObject.SetActive(crawl);
                if (crawl) Crawl(b, h, s, p, arm, claw);
                b.claws[0].position = hl;
                b.claws[1].position = hr;
                b.claws[0].rotation = b.claws[1].rotation = Quaternion.LookRotation(barFwd);
                for (int i = 0; i < 2; i++)
                {
                    look.Tint(block, b.armRends[i], arm, 0, 1);
                    look.Tint(block, b.clawRends[i], claw, 0, 1);
                }
                b.bar.position = barAt;
                b.bar.rotation = Quaternion.LookRotation(barFwd);
                b.bar.localScale = new Vector3(barWidth, 0.02f, UnityTuning.ClimbLipBarDepth);
                Color bc = enemy * (UnityTuning.ClimbLipBarGlow + (wave - 0.5f));
                bc.a = 1;
                barBlock.SetColor(BaseColor, bc);
                b.barRend.SetPropertyBlock(barBlock);
                if (FeelProbe.Recording)
                {
                    var bounds = new Bounds(p + Vector3.up * height * 0.5f, new Vector3(radius * 2, height, radius * 2));
                    bounds.Encapsulate(hl);
                    bounds.Encapsulate(hr);
                    bounds.Encapsulate(barAt);
                    FeelProbe.EffectBounds(FeelFx.Climb, bounds, 0.5f + 0.5f * wave);
                }
            }
            for (int i = n; i < pool.Count; i++)
            {
                if (pool[i].root.gameObject.activeSelf) pool[i].root.gameObject.SetActive(false);
                pool[i].drawn = null;
            }
        }
    }
}
