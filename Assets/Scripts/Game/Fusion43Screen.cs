// NIGHT43 builder F1 round 2 (design.n43.feel43; the lead's n43-r1 frames: "the hot-pink core / burst barely registers ... on level 2's bright
// pale ice everything washes out; the burst is small and centred behind the enemy body"). The FUSION's SCREEN layer, over the world's burst
// (Fusion43View): uGUI images on their own overlay canvas, alpha-blended, every bright shape on a dark ink twin a few px wider, so the read
// holds on a dark foundry and on white ice alike (value contrast, not only additive light):
//   t 0 .. meet   two streams converge on the hit's screen point: cyan streaks from the player's side (low, either side of the gun) and
//                 red-orange streaks from the enemy's side (high, behind it), visibly travelling in BEFORE the pop;
//   t meet        the pop: a hot-pink / white core disc, a ring blowing out, twenty radial shards flying out from the hit (inner and outer ends both
//                 moving out: an explosion, not a static star), a pink-white edge flash round the screen border for ~3 frames (FLASH INTENSITY,
//                 the flash gate), a chroma kick;
//   the word      "FUSION" punched in above and to the side of the hit (never over the enemy), clamped on screen.
// Timeline: Fusion43Burst (real time, through the freeze). Pooled images (built once); nothing allocated per frame.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed class Fusion43Screen
    {
        readonly SurvivalGame game;
        readonly Feel43 feel;
        Canvas canvas;
        RectTransform root;
        Image[] shard, shardInk, stream, streamInk, edge;
        Image ring, ringInk, coreDisc, coreInk;
        TextMeshProUGUI word;
        RectTransform wordRt;
        float[] shardAngle, shardLen, streamAngle, streamFrom;
        Color pink, core, cyan, ember, ink;
        bool built;
        double age = double.MaxValue;
        Vector3 hitWorld;
        bool popped, flashOk;
        // NIGHT44 V2 (vis44): the streaks drawn as tapered pieces along a curve, and a pink / white glow at the hit before the pop
        Image[] seg44, segInk44;
        Image preGlowInk44, preGlow44, preCore44;
        bool v44;
        Mulberry32 rng = new Mulberry32(UnityTuning.Fusion43SeedSalt + 7);

        public Fusion43Screen(SurvivalGame game, Feel43 feel)
        {
            this.game = game;
            this.feel = feel;
        }

        public bool Active => age < UnityTuning.Fusion43Rest;

        static Sprite ringSprite, discSprite, edgeSprite;

        static Sprite MakeSprite(int n, Func<float, float, float> alpha, string name)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = name };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2 - 1, dy = (y + 0.5f) / n * 2 - 1;
                    px[y * n + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha(dx, dy)) * 255));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100);
        }

        Image Img(string name, Sprite s, Color c, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(root, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = Vector2.zero; // canvas units from the bottom left
            rt.pivot = pivot;
            var im = go.GetComponent<Image>();
            im.sprite = s;
            im.color = c;
            im.raycastTarget = false;
            im.enabled = false;
            return im;
        }

        void Build()
        {
            built = true;
            if (!UiTheme.Ready) { built = false; return; }
            pink = Look.Hex(UnityTuning.Fusion43PinkHex);
            core = Look.Hex(UnityTuning.Fusion43CoreHex);
            cyan = UiTheme.Cyan;
            ember = UiTheme.Danger;
            ink = UiTheme.A(UiTheme.InkSolid, UnityTuning.Fusion43InkAlpha);
            ringSprite ??= MakeSprite(128, (dx, dy) => { float r = Mathf.Sqrt(dx * dx + dy * dy); return 1 - Mathf.Abs(r - 0.86f) / 0.12f; }, "Fusion43 ring");
            discSprite ??= MakeSprite(64, (dx, dy) => { float r = Mathf.Sqrt(dx * dx + dy * dy); float f = Mathf.Clamp01((1 - r) / 0.35f); return f * f * (3 - 2 * f); }, "Fusion43 disc");
            edgeSprite ??= MakeSprite(64, (dx, dy) => { float f = Mathf.Clamp01((dy + 1) * 0.5f); return f * f; }, "Fusion43 edge"); // opaque at the top, gone at the bottom
            var go = new GameObject("Fusion43 screen", typeof(Canvas), typeof(CanvasScaler));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.Fusion43WordSortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1; // canvas units: 1080 tall
            root = (RectTransform)go.transform;
            int ns = UnityTuning.Fusion43ScreenShards, nm = UnityTuning.Fusion43ScreenStreams;
            shard = new Image[ns]; shardInk = new Image[ns]; shardAngle = new float[ns]; shardLen = new float[ns];
            stream = new Image[nm]; streamInk = new Image[nm]; streamAngle = new float[nm]; streamFrom = new float[nm];
            // the edge flash first (under everything), then the inks, then the brights
            edge = new Image[4];
            for (int i = 0; i < 4; i++) edge[i] = Img("Fusion43 edge " + i, edgeSprite, pink, new Vector2(0.5f, 1f));
            preGlowInk44 = Img("Fusion44 glow ink", discSprite, ink, new Vector2(0.5f, 0.5f));
            preGlow44 = Img("Fusion44 glow", discSprite, pink, new Vector2(0.5f, 0.5f));
            preCore44 = Img("Fusion44 glow core", discSprite, core, new Vector2(0.5f, 0.5f));
            int nseg = nm * UnityTuning.Vis44StreakSegments;
            seg44 = new Image[nseg]; segInk44 = new Image[nseg];
            for (int i = 0; i < nseg; i++) segInk44[i] = Img("Fusion44 streak ink", discSprite, ink, new Vector2(0.5f, 0.5f));
            for (int i = 0; i < nseg; i++) seg44[i] = Img("Fusion44 streak", discSprite, (i / UnityTuning.Vis44StreakSegments) < nm / 2 ? cyan : ember, new Vector2(0.5f, 0.5f));
            ringInk = Img("Fusion43 ring ink", ringSprite, ink, new Vector2(0.5f, 0.5f));
            coreInk = Img("Fusion43 core ink", discSprite, ink, new Vector2(0.5f, 0.5f));
            for (int i = 0; i < nm; i++) streamInk[i] = Img("Fusion43 stream ink", null, ink, new Vector2(1f, 0.5f));
            for (int i = 0; i < ns; i++) shardInk[i] = Img("Fusion43 shard ink", null, ink, new Vector2(0f, 0.5f));
            for (int i = 0; i < nm; i++) stream[i] = Img("Fusion43 stream", null, i < nm / 2 ? cyan : ember, new Vector2(1f, 0.5f));
            for (int i = 0; i < ns; i++) shard[i] = Img("Fusion43 shard", null, (i % 3) == 0 ? core : pink, new Vector2(0f, 0.5f));
            ring = Img("Fusion43 ring", ringSprite, pink, new Vector2(0.5f, 0.5f));
            coreDisc = Img("Fusion43 core", discSprite, core, new Vector2(0.5f, 0.5f));
            var t = new GameObject("FUSION", typeof(RectTransform), typeof(TextMeshProUGUI));
            t.transform.SetParent(root, false);
            wordRt = (RectTransform)t.transform;
            wordRt.anchorMin = wordRt.anchorMax = Vector2.zero;
            wordRt.pivot = new Vector2(0f, 0.5f);
            wordRt.sizeDelta = new Vector2(700, 140);
            word = t.GetComponent<TextMeshProUGUI>();
            word.raycastTarget = false;
            word.alignment = TextAlignmentOptions.Left;
            word.textWrappingMode = TextWrappingModes.NoWrap;
            word.text = UnityTuning.Fusion43Word;
            UiTheme.Style(word, UnityTuning.Fusion43WordPx, pink, UiTheme.Display, shadow: true);
            word.outlineWidth = UnityTuning.Fusion43WordOutline; // a dark rim on the bright pink (reads on ice)
            word.outlineColor = (Color32)UiTheme.InkSolid;
            word.enabled = false;
            canvas.enabled = false;
        }

        /// <summary>A FUSION at `hit` (Unity space): the streams, the pop, the word.</summary>
        public void Start(Vector3 hit)
        {
            if (!built) Build();
            if (!built) return;
            hitWorld = hit;
            v44 = Vis44.On(game.cfg);
            age = 0;
            popped = false;
            for (int i = 0; i < shard.Length; i++)
            {
                shardAngle[i] = (float)(i * 360.0 / shard.Length + (rng.Next() - 0.5) * 12);
                shardLen[i] = Mathf.Lerp(UnityTuning.Fusion43ShardLenMin, UnityTuning.Fusion43ShardLenMax, (float)rng.Next());
            }
            int half = stream.Length / 2;
            for (int i = 0; i < stream.Length; i++)
            {
                // the player's (cyan) come from below, fanned either side; the enemy's (red-orange) from above, fanned behind it
                float baseAngle = i < half ? -90 : 90;
                float spread = i < half ? 70 : 60;
                int k = i < half ? i : i - half;
                streamAngle[i] = baseAngle + (k - (half - 1) * 0.5f) / Mathf.Max(1, half - 1) * spread * 2;
                streamFrom[i] = Mathf.Lerp(UnityTuning.Fusion43StreamFromMin, UnityTuning.Fusion43StreamFromMax, (float)rng.Next());
            }
            canvas.enabled = true;
            Feel43Probe.ScreenBursts++;
        }

        public void Clear()
        {
            age = double.MaxValue;
            if (canvas != null) canvas.enabled = false;
        }

        static void Bar(Image im, Vector2 at, float angleDeg, float len, float w, float a)
        {
            if (!(a > 0.01f) || !(len > 1)) { im.enabled = false; return; }
            var rt = (RectTransform)im.transform;
            rt.anchoredPosition = at;
            rt.sizeDelta = new Vector2(len, w);
            rt.localRotation = Quaternion.Euler(0, 0, angleDeg);
            Color c = im.color;
            c.a = a;
            im.color = c;
            im.enabled = true;
        }

        static void Disc(Image im, Vector2 at, float size, float a)
        {
            if (!(a > 0.01f) || !(size > 1)) { im.enabled = false; return; }
            var rt = (RectTransform)im.transform;
            rt.anchoredPosition = at;
            rt.sizeDelta = new Vector2(size, size);
            Color c = im.color;
            c.a = a;
            im.color = c;
            im.enabled = true;
        }

        public void Render(double frameDt)
        {
            if (!built)
            {
                if (UiTheme.Ready) Build(); // made at load (the first FUSION's frame does not build ~70 images)
                return;
            }
            if (canvas == null) return;
            if (age >= UnityTuning.Fusion43Rest)
            {
                if (canvas.enabled) canvas.enabled = false;
                return;
            }
            age += frameDt;
            Camera cam = game.cam;
            bool show = cam != null && game.flow == Flow.Playing;
            canvas.enabled = show;
            if (!show) return;
            float k = 1080f / Mathf.Max(1, Screen.height);
            Vector3 sp = cam.WorldToScreenPoint(hitWorld);
            bool front = sp.z > 0.05f;
            Vector2 H = new Vector2(sp.x, sp.y) * k;
            float W = Screen.width * k, Ht = 1080f;
            double meet = UnityTuning.Fusion43ConvergeTime, t = age;
            float inkW = UnityTuning.Fusion43InkPx;

            // the streams, converging before the pop
            double c = Fusion43Burst.Converge(t);
            Streaks44(H, front && v44 && t < meet, c, t / meet, inkW);
            for (int i = 0; i < stream.Length; i++)
            {
                if (!front || t >= meet || v44) { stream[i].enabled = streamInk[i].enabled = false; continue; }
                float ang = streamAngle[i] * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)); // from the hit out to where the stream starts
                float headR = streamFrom[i] * (1 - (float)(0.15 + 0.85 * c)); // the head's distance from the hit
                float len = Mathf.Min(streamFrom[i] * 0.45f, streamFrom[i] - headR + 40);
                Vector2 head = H + dir * headR;
                float deg = streamAngle[i]; // pivot at the head (1, 0.5): the bar extends away from the hit
                float w = UnityTuning.Fusion43StreamW;
                Bar(streamInk[i], head + dir * -2, deg + 180, len + inkW, w + inkW, 0.9f);
                Bar(stream[i], head, deg + 180, len, w, 1);
            }

            // the pop
            if (!popped && t >= meet)
            {
                popped = true;
                if (!game.cfg.movement.settings.reducedMotion) feel.Post?.Kick43(UnityTuning.Fusion48Chroma(game.cfg)); // NIGHT48 F: in post.js units (Fusion43Chroma 1.4 held the screen at a full split ~46 s)
                double fi = game.settings.View.flashIntensity;
                flashOk = fi > 0 && game.flashGate.TryFlash(SurvivalGame.RealNow);
            }
            double pt = t - meet;
            if (front && pt >= 0)
            {
                float u = (float)Math.Min(1, pt / UnityTuning.Fusion43ScreenPopTime);
                float w1 = 1 - u, grow = 1 - w1 * w1 * w1;
                // core disc: big at the pop, shrinking and fading
                float coreA = (float)Fusion43Burst.Core(t);
                float coreSize = Mathf.Lerp(UnityTuning.Fusion43CorePx, UnityTuning.Fusion43CorePx * 0.45f, u);
                Disc(coreInk, H, coreSize * 1.25f, coreA * 0.8f);
                Disc(coreDisc, H, coreSize, coreA);
                // ring
                var (rs, rk) = Fusion43Burst.Ring(t, 0);
                float rpx = UnityTuning.Fusion43RingPx * (float)rs;
                Disc(ringInk, H, rpx * 2 + inkW * 2, (float)rk * 0.85f);
                Disc(ring, H, rpx * 2, (float)rk);
                // shards: inner and outer ends both moving out
                for (int i = 0; i < shard.Length; i++)
                {
                    float inner = UnityTuning.Fusion43CorePx * 0.25f + grow * UnityTuning.Fusion43ShardTravel;
                    float len = shardLen[i] * (0.35f + 0.65f * grow) * (1 - 0.5f * u);
                    float sw = Mathf.Lerp(UnityTuning.Fusion43ShardW, UnityTuning.Fusion43ShardW * 0.3f, u);
                    float a = w1 * w1 > 0.02f ? Mathf.Min(1, 1.6f * w1) : 0;
                    float ang = shardAngle[i] * Mathf.Deg2Rad;
                    Vector2 at = H + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * inner;
                    Bar(shardInk[i], at - new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * (inkW * 0.5f), shardAngle[i], len + inkW, sw + inkW, a * 0.85f);
                    Bar(shard[i], at, shardAngle[i], len, sw, a);
                }
            }
            else
            {
                coreDisc.enabled = coreInk.enabled = ring.enabled = ringInk.enabled = false;
                for (int i = 0; i < shard.Length; i++) shard[i].enabled = shardInk[i].enabled = false;
            }

            // the edge flash: the four borders, pink to white, ~3 frames from the pop
            float ea = flashOk && pt >= 0 && pt < UnityTuning.Fusion43EdgeTime
                ? UnityTuning.Fusion43EdgeAlpha * (float)Math.Max(0, Math.Min(1, game.settings.View.flashIntensity)) * (float)(1 - pt / UnityTuning.Fusion43EdgeTime) : 0;
            float ep = UnityTuning.Fusion43EdgePx;
            EdgeAt(edge[0], new Vector2(W * 0.5f, Ht), W, ep, 0, ea); // top (the sprite's opaque side is its top)
            EdgeAt(edge[1], new Vector2(W * 0.5f, 0), W, ep, 180, ea); // bottom
            EdgeAt(edge[2], new Vector2(0, Ht * 0.5f), Ht, ep, 90, ea); // left
            EdgeAt(edge[3], new Vector2(W, Ht * 0.5f), Ht, ep, -90, ea); // right

            // the word: above and to the side of the hit, clamped on screen
            var (ws, wa) = Fusion43Burst.Word(t);
            Feel43Probe.WordScale = (float)ws;
            Feel43Probe.WordAlpha = (float)wa;
            if (wa > 0.01)
            {
                Vector2 off = new Vector2(UnityTuning.Fusion43WordDx, UnityTuning.Fusion43WordDy);
                Vector2 p = front ? H + off : new Vector2(W * 0.5f + off.x, Ht * 0.5f + off.y);
                p.x = Mathf.Clamp(p.x, 40, W - 40 - 460 * (float)ws);
                p.y = Mathf.Clamp(p.y, 80, Ht - 120);
                wordRt.anchoredPosition = p;
                wordRt.localScale = new Vector3((float)ws, (float)ws, 1);
                word.alpha = (float)wa;
                word.enabled = true;
            }
            else word.enabled = false;
        }

        // NIGHT44 V2 (vis44): each streak swirls in along a curve (its angle turns with its distance from the hit), drawn as short soft pieces
        // that taper from a wide head (nearest the hit) to a thin tail, each on a dark ink twin; a hot-pink glow with a white heart grows at the
        // hit from t 0 to the meet, so the +0.05 s frame already shows where the two forces meet.
        void Streaks44(Vector2 H, bool show, double c, double u01, float inkW)
        {
            int S = UnityTuning.Vis44StreakSegments, half = stream.Length / 2;
            for (int i = 0; i < stream.Length; i++)
            {
                if (!show)
                {
                    for (int j = 0; j < S; j++) seg44[i * S + j].enabled = segInk44[i * S + j].enabled = false;
                    continue;
                }
                float from = streamFrom[i];
                float headR = from * (1 - (float)(0.15 + 0.85 * c));
                float len = Mathf.Min(from * UnityTuning.Vis44StreakLenShare, from - headR + 60);
                float curl = UnityTuning.Vis44StreakCurlDeg * (i < half ? 1 : -1);
                for (int j = 0; j < S; j++)
                {
                    float r0 = headR + len * j / S, r1 = headR + len * (j + 1) / S;
                    Vector2 p0 = H + Dir44(streamAngle[i] + curl * r0 / from) * r0;
                    Vector2 p1 = H + Dir44(streamAngle[i] + curl * r1 / from) * r1;
                    float s = (j + 0.5f) / S;
                    float w = Mathf.Lerp(UnityTuning.Vis44StreakHeadPx, UnityTuning.Vis44StreakTailPx, s);
                    float a = Mathf.Lerp(1f, 0.5f, s);
                    Seg44(segInk44[i * S + j], p0, p1, w + inkW, inkW, a * 0.85f);
                    Seg44(seg44[i * S + j], p0, p1, w, 0, a);
                }
            }
            if (show)
            {
                float u = (float)Math.Max(0, Math.Min(1, u01));
                float size = UnityTuning.Vis44PreCorePx * (UnityTuning.Vis44PreCoreFrom + (1 - UnityTuning.Vis44PreCoreFrom) * u);
                Disc(preGlowInk44, H, size * 1.25f, 0.8f);
                Disc(preGlow44, H, size, 0.85f + 0.15f * u);
                Disc(preCore44, H, size * UnityTuning.Vis44PreCoreWhite, 1);
            }
            else preGlowInk44.enabled = preGlow44.enabled = preCore44.enabled = false;
        }

        static Vector2 Dir44(float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        }

        // a soft piece from a to b, `w` across, overlapping its neighbours by half its width (the joins hidden in the soft ends)
        static void Seg44(Image im, Vector2 a, Vector2 b, float w, float extra, float alpha)
        {
            Vector2 d = b - a;
            float l = d.magnitude;
            if (!(alpha > 0.01f) || !(l > 0.5f)) { im.enabled = false; return; }
            var rt = (RectTransform)im.transform;
            rt.anchoredPosition = (a + b) * 0.5f;
            rt.sizeDelta = new Vector2(l + w * 0.9f + extra, w);
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            Color c = im.color;
            c.a = alpha;
            im.color = c;
            im.enabled = true;
        }

        static void EdgeAt(Image im,Vector2 at, float len, float depth, float rot, float a)
        {
            if (!(a > 0.01f)) { im.enabled = false; return; }
            var rt = (RectTransform)im.transform;
            rt.anchoredPosition = at;
            rt.sizeDelta = new Vector2(len, depth);
            rt.localRotation = Quaternion.Euler(0, 0, rot);
            Color c = im.color;
            c.a = a;
            im.color = c;
            im.enabled = true;
        }
    }
}
