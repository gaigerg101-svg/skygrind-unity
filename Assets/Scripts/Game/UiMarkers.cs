// NIGHT8 U2-09: enemy markers (main.js updateMarkers + hud.js setMarkers; "couldn't find him"). Every live enemy off
// screen (or behind) gets a red-orange teardrop on the screen edge, pointing outward; once director.remaining <=
// director.markersAt (3) every remaining one on screen gets an outline diamond too. Up to hud.markerCount, kept
// hud.edgeMargin in from the edges. Survival uses markersAt only (spawner.js's markersAfter is the other modes').
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UnityEngine.UI.Image[] markers;
        bool[] markerEdge;
        Sprite diamondSprite, dropSprite;

        void BuildMarkers()
        {
            int n = cfg.Int("hud.markerCount");
            markers = new UnityEngine.UI.Image[n];
            markerEdge = new bool[n];
            diamondSprite = MarkerSprite(false);
            dropSprite = MarkerSprite(true);
            for (int i = 0; i < n; i++)
            {
                var img = Img(sight, $"Marker {i}", danger, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(16, 16));
                img.sprite = diamondSprite;
                img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                img.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
                img.enabled = false;
                markers[i] = img;
            }
        }

        /// <summary>hud.js .hud-markers i: a 12 px square with a 2 px danger border (a diamond once turned 45 deg), or the edge's filled teardrop (the point up).</summary>
        static Sprite MarkerSprite(bool drop)
        {
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2 - 1, v = (y + 0.5f) / n * 2 - 1;
                    float a;
                    if (!drop)
                    {
                        float d = Mathf.Max(Mathf.Abs(u), Mathf.Abs(v));
                        a = d <= 1 && d >= 0.66f ? 1 : 0; // the border
                    }
                    else
                    {
                        // A disc below, narrowing to a point at the top (the CSS's one sharp corner).
                        float disc = Mathf.Sqrt(u * u + (v + 0.25f) * (v + 0.25f));
                        bool body = disc <= 0.7f;
                        bool tip = v > -0.25f && Mathf.Abs(u) <= 0.7f * (1 - (v + 0.25f) / 1.25f);
                        a = body || tip ? 1 : 0;
                    }
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        // --- NIGHT17 U3-30.7: the kill marker, distinct from the hit marker -------------------------------------------------
        // The hit marker is four thin white arms (the headshot adds its diamond); a kill is four thick spikes pointing in at
        // the diagonals (another shape), Hud17KillMarkerPx wide (bigger), in DANGER (the enemy's colour: the web's kill
        // marker), punched from 1.45x and turned 20 deg as it settles over 120 ms, its first frames white (FLASH INTENSITY
        // 0: none), fading over its last 40 %. A headshot kill is 1.2x. Frame time; nothing allocates after the build.
        UnityEngine.UI.Image killMarker17;
        double killMarker17Age = double.MaxValue;
        float killMarker17Size = 1;
        bool killMarker17White;

        void BuildKillMarker17()
        {
            killMarker17 = Img(sight, "Kill marker", UiTheme.Danger, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud17KillMarkerPx, UnityTuning.Hud17KillMarkerPx));
            killMarker17.sprite = KillSpikesSprite();
            killMarker17.enabled = false;
            BuildKillMarker18(killMarker17.sprite);
        }

        void KillMarker17Fire(bool headshot)
        {
            killMarker17Age = 0;
            killMarker17Size = headshot ? 1.2f : 1f;
            killMarker17White = game.settings.Num("flashIntensity") > 0;
            // NIGHT18 U3-31.6: tonight's marker starts too (FeelAB picks which one draws)
            killMarker18Age = 0;
            killMarker18Size = headshot ? UnityTuning.Hud18KillMarkerHead : 1f;
            killMarker18Frame = Time.frameCount;
            KillPop53.Fire(cfg, headshot, game.settings.View.reducedMotion, Time.frameCount); // NIGHT53 U polish53 (killPop; KillPop53.cs; off: nothing)
        }

        // --- NIGHT18 U3-31.6: the kill marker (GG's KILL MARKER switch ON; M2's marker half) --------------------------------
        // WHITE (no DANGER: the kill confirm is the player's read, white-hot like the kill), the same four thick wedges on the
        // diagonals (the hit marker is four thin arms inside their hollow; on the kill's own frame the hit marker stands down,
        // Ui.cs), Hud18KillMarkerPx at rest over a hard INK underlay. Drawn on the kill's own frame at Hud18KillMarkerFrom
        // (52 px: >= 48, and growing, so never an M9 pop-in), popping to Hud18KillMarkerPeak by ~40 ms and settling square from
        // the theme's 12 deg turn by 120 ms (REDUCED MOTION: no turn), fading over its last 100 ms, gone at 360 ms. Frame
        // time (it moves through the kill's freeze). Reported to the reel each frame it draws (FeelFx.KillMarker).
        RectTransform killMarker18Root;
        UnityEngine.UI.Image killMarker18, killMarker18Shadow;
        double killMarker18Age = double.MaxValue;
        float killMarker18Size = 1, killMarker18ShownAlpha = -1;
        int killMarker18Frame = -1;
        bool killMarker18On;

        /// <summary>The kill confirm fired this frame (the hit marker stands down for the kill's own hits).</summary>
        bool KillMarker18Fresh => killMarker18Frame == Time.frameCount;

        void BuildKillMarker18(Sprite spikes)
        {
            float px = UnityTuning.Hud18KillMarkerPx;
            killMarker18Root = Hud17Rect(sight, "Kill marker (N18)", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(px, px));
            // round 1's picks (the white wedges vanished over the white burst): an INK stroke all round, Hud18KillMarkerOutline px
            // (the wedges dilated; its texture is 1:1 with the marker at rest)
            killMarker18Shadow = Img(killMarker18Root, "Underlay", UiTheme.InkSolid, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(px, px));
            killMarker18Shadow.sprite = KillSpikesOutlineSprite18((int)px, UnityTuning.Hud18KillMarkerOutline);
            // round 2's picks ("at 20 m the marker hides the whole target"): tonight's own wedges, from Hud18KillMarkerInner to
            // Outer px along each diagonal (a 40 px clear centre inside their stroke), 1:1 texels at rest
            spikes = KillSpikesOutlineSprite18((int)px, 0);
            killMarker18Shadow.raycastTarget = false;
            killMarker18 = Img(killMarker18Root, "Marker", UiTheme.White, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(px, px));
            killMarker18.sprite = spikes;
            killMarker18.raycastTarget = false;
            killMarker18Root.gameObject.SetActive(false);
        }

        void RenderKillMarker18(double frameDt, bool playing)
        {
            bool on = playing && KillMarker18.Shown(killMarker18Age);
            if (on != killMarker18On)
            {
                killMarker18On = on;
                killMarker18Root.gameObject.SetActive(on);
            }
            if (!on) return;
            double age = killMarker18Age; // drawn at its age, then stepped: its first frame is the kill's own, at age 0
            float s = killMarker18Size * KillMarker18.Scale(age) * (KillPop53.Built ? KillPop53.ScaleK : 1f); // NIGHT53 U polish53: the pop's punch (1 when idle or off)
            killMarker18Root.localScale = new Vector3(s, s, 1);
            float turn = game.settings.View.reducedMotion ? 0f : KillMarker18.Turn(age);
            killMarker18Root.localRotation = Quaternion.Euler(0, 0, turn);
            float a = KillMarker18.Alpha(age) * Mathf.Clamp01((float)((age + frameDt) / UnityTuning.KillMarkerFadeIn25)); // NIGHT25 (T-12, M9): two frames of fade-in, not a full-alpha first frame
            if (a != killMarker18ShownAlpha)
            {
                killMarker18ShownAlpha = a;
                killMarker18.color = UiTheme.A(UiTheme.White, a);
                killMarker18Shadow.color = UiTheme.A(UiTheme.InkSolid, a);
            }
            FeelProbe.EffectUi(FeelFx.KillMarker, killMarker18.rectTransform, a);
            killMarker18Age += frameDt;
        }

        void RenderKillMarker17(double frameDt, bool playing)
        {
            if (killMarker17 == null) return;
            if (FeelAB.On)
            {
                if (killMarker17.enabled) killMarker17.enabled = false;
                killMarker17Age = double.MaxValue; // a live F6 flip never replays a stale kill
                RenderKillMarker18(frameDt, playing);
                return;
            }
            killMarker18Age = double.MaxValue;
            if (killMarker18On)
            {
                killMarker18On = false;
                killMarker18Root.gameObject.SetActive(false);
            }
            bool on = playing && killMarker17Age < UnityTuning.Hud17KillMarkerTime;
            if (killMarker17.enabled != on) killMarker17.enabled = on;
            if (!on) return;
            killMarker17Age += frameDt;
            double k = killMarker17Age / UnityTuning.Hud17KillMarkerPunchTime;
            float e = UiTween.OutCubic(k);
            float s = killMarker17Size * Mathf.LerpUnclamped(UnityTuning.Hud17KillMarkerPunch, 1f, e);
            RectTransform rt = killMarker17.rectTransform;
            rt.localScale = new Vector3(s, s, 1);
            rt.localRotation = Quaternion.Euler(0, 0, UnityTuning.Hud17KillMarkerTurn * (1 - e));
            double life = killMarker17Age / UnityTuning.Hud17KillMarkerTime;
            float a = life < 0.6 ? 1f : 1f - UiTween.InCubic((life - 0.6) / 0.4);
            Color c = killMarker17White && killMarker17Age < UnityTuning.Hud17PointsWhite ? UiTheme.White : UiTheme.Danger;
            c.a = a;
            killMarker17.color = c;
            FeelProbe.EffectUi(FeelFx.KillMarker, rt, a); // the reel measures NIGHT17's marker too (the A side)
        }

        /// <summary>
        /// NIGHT18 rounds 1-2: tonight's wedges (Hud18KillMarkerInner .. Outer px along each diagonal, Hud18KillMarkerWedge px
        /// half-wide at the outer end) on an n-texel square, dilated by `r` px (the max of the wedge over a disc; 0: the
        /// wedges themselves), white (tinted): r = the outline, the INK stroke behind the white marker. Built once each.
        /// </summary>
        static Sprite KillSpikesOutlineSprite18(int n, float r)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = r > 0 ? "Kill spikes outline" : "Kill spikes (N18)" };
            var px = new Color32[n * n];
            int ri = Mathf.CeilToInt(r);
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float a = 0;
                    for (int oy = -ri; oy <= ri && a < 1; oy++)
                        for (int ox = -ri; ox <= ri; ox++)
                        {
                            if (ox * ox + oy * oy > r * r) continue;
                            a = Mathf.Max(a, KillWedgeAlpha18(x + ox, y + oy, n));
                            if (a >= 1) break;
                        }
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        /// <summary>Tonight's wedge alpha at texel (x, y) of an n-texel square (KillSpikesSprite's formula with Hud18KillMarker's radii).</summary>
        static float KillWedgeAlpha18(float x, float y, int n)
        {
            float c = n * 0.5f, rIn = UnityTuning.Hud18KillMarkerInner, rOut = UnityTuning.Hud18KillMarkerOuter, wOut = UnityTuning.Hud18KillMarkerWedge;
            float dx = x + 0.5f - c, dy = y + 0.5f - c;
            float u = (Mathf.Abs(dx) + Mathf.Abs(dy)) * 0.70710678f;
            float v = Mathf.Abs(Mathf.Abs(dx) - Mathf.Abs(dy)) * 0.70710678f;
            float t = (u - rIn) / (rOut - rIn);
            return t < 0 || t > 1 ? 0 : Mathf.Clamp01(t * wOut - v + 0.5f) * Mathf.Clamp01((rOut - u) + 0.5f);
        }

        /// <summary>Four thick spikes on the diagonals, their points toward the centre (a hollow middle), white (tinted).</summary>
        static Sprite KillSpikesSprite()
        {
            const int n = 96;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Kill spikes" };
            var px = new Color32[n * n];
            float c = n * 0.5f, rIn = n * 0.17f, rOut = n * 0.49f, wOut = n * 0.075f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c;
                    // along / across the nearer diagonal
                    float u = (Mathf.Abs(dx) + Mathf.Abs(dy)) * 0.70710678f;
                    float v = Mathf.Abs(Mathf.Abs(dx) - Mathf.Abs(dy)) * 0.70710678f;
                    float t = (u - rIn) / (rOut - rIn); // 0 at the point, 1 at the base
                    float half = t * wOut;
                    float a = t < 0 || t > 1 ? 0 : Mathf.Clamp01(half - v + 0.5f) * Mathf.Clamp01((rOut - u) + 0.5f);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        // NIGHT18 U3-31.6 round 1 (build 3: roundend's Marker 0-2 were whole on their first frame, f17): a marker that turns
        // on enters (grows from 0.6x and fades in: HudEntrance18, 150 ms; FeelAB on). A marker already on that moves to
        // another enemy (the list's order changed) is not an entrance. Off: NIGHT17's markers.
        double[] markerEnterAge18;
        float[] markerShownScale18;

        void MarkerEnter18(int i, double frameDt)
        {
            float s = 1, a = 1;
            if (FeelAB.On && !HudEntrance18.Done(markerEnterAge18[i]))
            {
                markerEnterAge18[i] = HudEntrance18.Step(markerEnterAge18[i], frameDt);
                s = HudEntrance18.Scale(markerEnterAge18[i]);
                a = HudEntrance18.Alpha(markerEnterAge18[i]);
            }
            if (s == markerShownScale18[i]) return;
            markerShownScale18[i] = s;
            markers[i].rectTransform.localScale = new Vector3(s, s, 1);
            markers[i].color = UiTheme.A(danger, a);
        }

        void RenderMarkers(double frameDt)
        {
            if (markers == null) BuildMarkers();
            if (markerEnterAge18 == null)
            {
                markerEnterAge18 = new double[markers.Length];
                markerShownScale18 = new float[markers.Length];
                for (int i = 0; i < markers.Length; i++)
                {
                    markerEnterAge18[i] = double.MaxValue;
                    markerShownScale18[i] = 1;
                }
            }
            int count = 0;
            Camera cam = game.cam;
            if (game.Active && cam != null)
            {
                float w = root.rect.width, h = root.rect.height, pad = margin;
                bool marked = game.run.state == SurvivalRun.State.Fight && game.director.Remaining <= cfg.Num("director.markersAt");
                foreach (Enemy e in game.enemies.all)
                {
                    if (count >= markers.Length) break;
                    if (!e.alive) continue;
                    Vector3 vp = cam.WorldToViewportPoint(WebSpace.ToUnity(e.AimPoint));
                    bool behind = vp.z < 0;
                    float nx = vp.x * 2 - 1, ny = vp.y * 2 - 1;
                    if (behind)
                    {
                        nx = -nx;
                        ny = -ny;
                    }
                    bool edge = behind || Mathf.Abs(nx) > 1 || Mathf.Abs(ny) > 1;
                    if (!edge && !marked) continue;
                    if (edge)
                    {
                        float k = 1 / Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny), 1e-6f);
                        nx *= k;
                        ny *= k;
                    }
                    UnityEngine.UI.Image m = markers[count++];
                    if (!m.enabled)
                    {
                        m.enabled = true;
                        markerEnterAge18[count - 1] = 0;
                    }
                    MarkerEnter18(count - 1, frameDt);
                    m.rectTransform.anchoredPosition = new Vector2(Mathf.Clamp((nx * 0.5f + 0.5f) * w, pad, w - pad), Mathf.Clamp((ny * 0.5f + 0.5f) * h, pad, h - pad));
                    if (edge != markerEdge[count - 1])
                    {
                        markerEdge[count - 1] = edge;
                        m.sprite = edge ? dropSprite : diamondSprite;
                    }
                    m.rectTransform.localRotation = Quaternion.Euler(0, 0, edge ? Mathf.Atan2(ny, nx) * Mathf.Rad2Deg - 90 : 45);
                }
            }
            for (int i = count; i < markers.Length; i++) if (markers[i].enabled) markers[i].enabled = false;
        }
    }
}
