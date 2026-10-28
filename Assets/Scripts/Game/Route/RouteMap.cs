// NIGHT14 U3-27.10 (agent V2): THE ROUTE's map, drawn on the CPU into a Texture2D (RESULTS' RawImage, the SHARE CARD).
// Top-down over the Survival map: web -z (the way the spawn looks into the arena) is up, +x right. The static layer is
// drawn once at construction (the plate; the floor; the boxes' footprints from map.boxes, fainter the lower their tops;
// the zones' borders at the ground, sampled from map.zoneParts; the Grinder core's square; the arena's outline in
// barrier.boundaryColor, the pale violet-white of the boundary). Draw(track) copies it and adds the run: the path coloured
// by speed on a cyan ramp (colors.usable x RouteRampDim at a stand-still, colors.usable at hud.speedMid, toward white at
// hud.speedHigh and up), round ticks across the path at each round's start, the kills as small gold dots, the start ring,
// the exfil site's ring (cyan), the DOWN as a red-orange X. Called once per RESULTS (and per card), never per frame.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>A Color32 buffer with anti-aliased primitives (coverage-weighted alpha blending), origin bottom-left.</summary>
    public sealed class RouteRaster
    {
        public readonly int w, h;
        public readonly Color32[] px;

        public RouteRaster(int w, int h)
        {
            this.w = w;
            this.h = h;
            px = new Color32[w * h];
        }

        public void Clear(Color c, float a)
        {
            var c32 = new Color32((byte)(Mathf.Clamp01(c.r) * 255), (byte)(Mathf.Clamp01(c.g) * 255), (byte)(Mathf.Clamp01(c.b) * 255), (byte)(Mathf.Clamp01(a) * 255));
            for (int i = 0; i < px.Length; i++) px[i] = c32;
        }

        public void CopyFrom(RouteRaster o) => Array.Copy(o.px, px, px.Length);

        /// <summary>Blend `c` at alpha `a` over pixel (x, y).</summary>
        public void Blend(int x, int y, Color c, float a)
        {
            if (x < 0 || y < 0 || x >= w || y >= h || a <= 0) return;
            if (a > 1) a = 1;
            int i = y * w + x;
            Color32 d = px[i];
            float ia = 1 - a;
            px[i] = new Color32(
                (byte)(Mathf.Clamp01(c.r) * 255 * a + d.r * ia),
                (byte)(Mathf.Clamp01(c.g) * 255 * a + d.g * ia),
                (byte)(Mathf.Clamp01(c.b) * 255 * a + d.b * ia),
                (byte)Mathf.Min(255, a * 255 + d.a * ia));
        }

        public void Rect(float x0, float y0, float x1, float y1, Color c, float a)
        {
            int ix0 = Mathf.Max(0, Mathf.RoundToInt(Mathf.Min(x0, x1))), ix1 = Mathf.Min(w - 1, Mathf.RoundToInt(Mathf.Max(x0, x1)) - 1);
            int iy0 = Mathf.Max(0, Mathf.RoundToInt(Mathf.Min(y0, y1))), iy1 = Mathf.Min(h - 1, Mathf.RoundToInt(Mathf.Max(y0, y1)) - 1);
            for (int y = iy0; y <= iy1; y++)
                for (int x = ix0; x <= ix1; x++) Blend(x, y, c, a);
        }

        /// <summary>A rectangle's outline, `width` px inside its edges.</summary>
        public void RectEdge(float x0, float y0, float x1, float y1, float width, Color c, float a)
        {
            Segment(x0, y0, x1, y0, width, c, a);
            Segment(x1, y0, x1, y1, width, c, a);
            Segment(x1, y1, x0, y1, width, c, a);
            Segment(x0, y1, x0, y0, width, c, a);
        }

        /// <summary>A segment `width` px wide with round ends, anti-aliased by its distance field.</summary>
        public void Segment(float ax, float ay, float bx, float by, float width, Color c, float a)
        {
            float r = width * 0.5f;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ax, bx) - r - 1)), x1 = Mathf.Min(w - 1, Mathf.CeilToInt(Mathf.Max(ax, bx) + r + 1));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ay, by) - r - 1)), y1 = Mathf.Min(h - 1, Mathf.CeilToInt(Mathf.Max(ay, by) + r + 1));
            float dx = bx - ax, dy = by - ay, len2 = dx * dx + dy * dy;
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float qx = x + 0.5f - ax, qy = y + 0.5f - ay;
                    float t = len2 > 0 ? Mathf.Clamp01((qx * dx + qy * dy) / len2) : 0;
                    float ex = qx - t * dx, ey = qy - t * dy;
                    float cover = Mathf.Clamp01(r + 0.5f - Mathf.Sqrt(ex * ex + ey * ey));
                    if (cover > 0) Blend(x, y, c, a * cover);
                }
            }
        }

        public void Disc(float cx, float cy, float r, Color c, float a)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1)), x1 = Mathf.Min(w - 1, Mathf.CeilToInt(cx + r + 1));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1)), y1 = Mathf.Min(h - 1, Mathf.CeilToInt(cy + r + 1));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float qx = x + 0.5f - cx, qy = y + 0.5f - cy;
                    float cover = Mathf.Clamp01(r + 0.5f - Mathf.Sqrt(qx * qx + qy * qy));
                    if (cover > 0) Blend(x, y, c, a * cover);
                }
            }
        }

        public void Ring(float cx, float cy, float r, float width, Color c, float a)
        {
            float half = width * 0.5f;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r - half - 1)), x1 = Mathf.Min(w - 1, Mathf.CeilToInt(cx + r + half + 1));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r - half - 1)), y1 = Mathf.Min(h - 1, Mathf.CeilToInt(cy + r + half + 1));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float qx = x + 0.5f - cx, qy = y + 0.5f - cy;
                    float cover = Mathf.Clamp01(half + 0.5f - Mathf.Abs(Mathf.Sqrt(qx * qx + qy * qy) - r));
                    if (cover > 0) Blend(x, y, c, a * cover);
                }
            }
        }

        /// <summary>An X of half-size `s` and stroke `width`, over a dark halo `halo` px wider.</summary>
        public void Cross(float cx, float cy, float s, float width, float halo, Color c, Color haloColor, float a)
        {
            Segment(cx - s, cy - s, cx + s, cy + s, width + 2 * halo, haloColor, a * 0.8f);
            Segment(cx - s, cy + s, cx + s, cy - s, width + 2 * halo, haloColor, a * 0.8f);
            Segment(cx - s, cy - s, cx + s, cy + s, width, c, a);
            Segment(cx - s, cy + s, cx + s, cy - s, width, c, a);
        }

        public void To(Texture2D tex)
        {
            tex.SetPixels32(px);
            tex.Apply(false);
        }
    }

    public sealed class RouteMap
    {
        public readonly int size;
        readonly RouteRaster baseLayer, layer;
        Texture2D tex;
        readonly double minX, minZ, span; // the world window: x in [minX, minX + span], z in [minZ, minZ + span]
        readonly float ppm, k; // px per metre; the raster numbers' scale (size / RouteMapTexture)
        readonly Color plate, text, usable, danger, gold, boundary, slow, fast;
        readonly double speedMid, speedHigh, siteRadius;

        /// <summary>How many times Draw ran (automation reads it).</summary>
        public int Draws { get; private set; }

        public RouteMap(GameConfig cfg, SurvivalMap map, int size)
        {
            this.size = size;
            k = size / (float)UnityTuning.RouteMapTexture;
            plate = Look.C(cfg, "ui.theme.panel");
            text = Look.C(cfg, "ui.theme.text");
            usable = Look.C(cfg, "colors.usable");
            danger = Look.C(cfg, "colors.enemy");
            gold = Look.C(cfg, "ui.theme.gold");
            boundary = Look.C(cfg, "barrier.boundaryColor");
            slow = usable * UnityTuning.RouteRampDim;
            slow.a = 1;
            fast = Color.Lerp(usable, Color.white, UnityTuning.RouteRampWhite);
            speedMid = cfg.Num("hud.speedMid");
            speedHigh = cfg.Num("hud.speedHigh");
            siteRadius = cfg.Has("design.exfil.siteRadius") ? cfg.Num("design.exfil.siteRadius") : 0;
            MapBox floor = map.floor;
            double fx0 = floor.min.x, fx1 = floor.max.x, fz0 = floor.min.z, fz1 = floor.max.z;
            if (!(fx1 > fx0) || !(fz1 > fz0))
            {
                fx0 = fz0 = -map.floorSize * 0.5;
                fx1 = fz1 = map.floorSize * 0.5;
            }
            span = Math.Max(fx1 - fx0, fz1 - fz0) + 2 * UnityTuning.RouteMapMargin;
            minX = (fx0 + fx1) * 0.5 - span * 0.5;
            minZ = (fz0 + fz1) * 0.5 - span * 0.5;
            ppm = (float)(size / span);
            baseLayer = new RouteRaster(size, size);
            layer = new RouteRaster(size, size);
            DrawBase(map, fx0, fz0, fx1, fz1);
            layer.CopyFrom(baseLayer);
        }

        /// <summary>The map's texture (made on first use with the layer drawn so far; Draw keeps it current).</summary>
        public Texture2D texture
        {
            get
            {
                if (tex != null) return tex;
                tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "The route" };
                layer.To(tex);
                return tex;
            }
        }

        /// <summary>The drawn layer (the CPU pixels behind the texture).</summary>
        public RouteRaster Raster => layer;

        /// <summary>Texture x of a world x.</summary>
        public float U(double x) => (float)((x - minX) * ppm);

        /// <summary>Texture y of a world z (web -z up).</summary>
        public float V(double z) => (float)((minZ + span - z) * ppm);

        void DrawBase(SurvivalMap map, double fx0, double fz0, double fx1, double fz1)
        {
            RouteRaster r = baseLayer;
            r.Clear(plate, UnityTuning.RoutePlateAlpha);
            r.Rect(U(fx0), V(fz1), U(fx1), V(fz0), boundary, UnityTuning.RouteFloorAlpha);
            // The boxes' footprints, the lowest tops first (the higher ones over them), alpha by top height.
            int n = map.boxes.Count;
            var order = new int[n];
            double topMin = double.PositiveInfinity, topMax = double.NegativeInfinity;
            for (int i = 0; i < n; i++)
            {
                order[i] = i;
                double top = map.boxes[i].max.y;
                if (top < topMin) topMin = top;
                if (top > topMax) topMax = top;
            }
            Array.Sort(order, (a, b) =>
            {
                int c = map.boxes[a].max.y.CompareTo(map.boxes[b].max.y);
                return c != 0 ? c : a.CompareTo(b);
            });
            double topSpan = Math.Max(1e-6, topMax - topMin);
            foreach (int i in order)
            {
                MapBox b = map.boxes[i];
                float t = (float)((b.max.y - topMin) / topSpan);
                float x0 = U(b.min.x), x1 = U(b.max.x), y0 = V(b.max.z), y1 = V(b.min.z);
                r.Rect(x0, y0, x1, y1, text, Mathf.Lerp(UnityTuning.RouteBoxAlphaLow, UnityTuning.RouteBoxAlphaHigh, t));
                r.RectEdge(x0, y0, x1, y1, Mathf.Max(1, k), text, UnityTuning.RouteBoxEdgeAlpha * Mathf.Lerp(0.5f, 1, t));
            }
            // The zones' borders at the ground (y 1, as the zone tracker reads the feet + 1), sampled on a half-size grid.
            int g = Math.Max(2, size / 2);
            var zone = new int[g * g];
            double cell = span / g;
            for (int y = 0; y < g; y++)
            {
                double z = minZ + span - (y + 0.5) * cell;
                for (int x = 0; x < g; x++) zone[y * g + x] = ZoneMath.ZoneIndexAt(map.zoneParts, minX + (x + 0.5) * cell, 1, z);
            }
            float px = size / (float)g;
            for (int y = 0; y < g; y++)
            {
                for (int x = 0; x < g; x++)
                {
                    int zi = zone[y * g + x];
                    if (zi < 0) continue;
                    bool edge = (x + 1 < g && zone[y * g + x + 1] >= 0 && zone[y * g + x + 1] != zi) || (y + 1 < g && zone[(y + 1) * g + x] >= 0 && zone[(y + 1) * g + x] != zi);
                    if (edge) r.Rect(x * px, y * px, (x + 1) * px, (y + 1) * px, boundary, UnityTuning.RouteZoneEdgeAlpha);
                }
            }
            // The Grinder core's square (its pillars rise and fall: an outline).
            if (map.core.HasValue)
            {
                CoreSpec c = map.core.Value;
                double side = c.cells * c.cellSize;
                r.RectEdge(U(c.x0), V(c.z0 + side), U(c.x0 + side), V(c.z0), Mathf.Max(1, k), boundary, UnityTuning.RouteZoneEdgeAlpha * 1.5f);
            }
            // The arena's outline: the boundary's pale violet-white.
            r.RectEdge(U(fx0), V(fz1), U(fx1), V(fz0), UnityTuning.RouteBoundaryPx * k, boundary, 0.95f);
        }

        /// <summary>The path's colour at a speed: dim cyan at rest, colors.usable at hud.speedMid, toward white at hud.speedHigh.</summary>
        public Color Ramp(double s)
        {
            if (!(s > 0)) return slow;
            if (s <= speedMid) return Color.Lerp(slow, usable, (float)(s / Math.Max(1e-6, speedMid)));
            return Color.Lerp(usable, fast, (float)Math.Min(1, (s - speedMid) / Math.Max(1e-6, speedHigh - speedMid)));
        }

        /// <summary>The run on the static layer, into the texture (once per RESULTS / per card).</summary>
        public void Draw(RouteTrack t)
        {
            DrawRaster(t);
            layer.To(texture);
        }

        /// <summary>The run on the static layer, the CPU pixels only (Raster).</summary>
        public void DrawRaster(RouteTrack t)
        {
            RouteRaster r = layer;
            r.CopyFrom(baseLayer);
            Draws++;
            if (t == null || t.count == 0) return;
            float pathW = UnityTuning.RoutePathPx * k, pa = UnityTuning.RoutePathAlpha;
            for (int i = 1; i < t.count; i++)
            {
                Color c = Ramp(0.5 * (t.speed[i - 1] + t.speed[i]));
                r.Segment(U(t.px[i - 1]), V(t.pz[i - 1]), U(t.px[i]), V(t.pz[i]), pathW, c, pa);
            }
            // Round ticks across the path where each round began.
            float tick = UnityTuning.RouteTickPx * k, tickW = UnityTuning.RouteTickWidthPx * k;
            for (int i = 0; i < t.roundCount; i++)
            {
                float cx = U(t.roundX[i]), cy = V(t.roundZ[i]);
                int at = t.everySteps > 0 ? (t.roundStep[i] - t.startStep) / t.everySteps : 0;
                int a = Mathf.Clamp(at - 1, 0, t.count - 1), b = Mathf.Clamp(at + 1, 0, t.count - 1);
                float dx = U(t.px[b]) - U(t.px[a]), dy = V(t.pz[b]) - V(t.pz[a]);
                float len = Mathf.Sqrt(dx * dx + dy * dy);
                float nx = 1, ny = 0; // standing still: a level tick
                if (len > 0.5f)
                {
                    nx = -dy / len;
                    ny = dx / len;
                }
                r.Segment(cx - nx * tick * 0.5f, cy - ny * tick * 0.5f, cx + nx * tick * 0.5f, cy + ny * tick * 0.5f, tickW, text, 0.95f);
            }
            float killR = UnityTuning.RouteKillPx * k;
            for (int i = 0; i < t.killCount; i++) r.Disc(U(t.killX[i]), V(t.killZ[i]), killR, gold, UnityTuning.RouteKillAlpha);
            float ringW = UnityTuning.RouteRingWidthPx * k;
            r.Ring(U(t.px[0]), V(t.pz[0]), UnityTuning.RouteStartPx * k, ringW, text, 0.95f);
            if (t.exfilSite)
            {
                float er = Mathf.Max(UnityTuning.RouteStartPx * k, (float)(siteRadius * ppm));
                if (t.exfilWin) r.Disc(U(t.exfilX), V(t.exfilZ), er, usable, 0.25f);
                r.Ring(U(t.exfilX), V(t.exfilZ), er, ringW, usable, 0.95f);
            }
            if (t.down) r.Cross(U(t.downX), V(t.downZ), UnityTuning.RouteDownPx * k, UnityTuning.RouteDownWidthPx * k, UnityTuning.RouteHaloPx * k, danger, plate, 1);
        }

        /// <summary>A legend icon (`px` square): a kill dot, a round tick, the DOWN's X, the start ring or the exfil ring.</summary>
        public Texture2D Icon(RouteIconKind kind, int px)
        {
            var r = new RouteRaster(px, px);
            r.Clear(plate, 0);
            float c = px * 0.5f, s = px / 18f;
            switch (kind)
            {
                case RouteIconKind.Kill: r.Disc(c, c, 3.2f * s, gold, 1); break;
                case RouteIconKind.Tick: r.Segment(c - 6 * s, c, c + 6 * s, c, 2.4f * s, text, 1); break;
                case RouteIconKind.Down: r.Cross(c, c, 5.5f * s, 2.6f * s, 1.5f * s, danger, plate, 1); break;
                case RouteIconKind.Start: r.Ring(c, c, 5.5f * s, 2.2f * s, text, 1); break;
                case RouteIconKind.Exfil: r.Ring(c, c, 6.5f * s, 2.2f * s, usable, 1); break;
            }
            var tex = new Texture2D(px, px, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Route icon " + kind };
            r.To(tex);
            return tex;
        }

        /// <summary>The speed ramp as a strip (w x 1): dim cyan to bright, hud.speedMid at its place on the way to hud.speedHigh.</summary>
        public Texture2D RampStrip(int w)
        {
            var tex = new Texture2D(w, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Route ramp" };
            var px = new Color32[w];
            for (int i = 0; i < w; i++) px[i] = Ramp(speedHigh * i / Math.Max(1, w - 1));
            tex.SetPixels32(px);
            tex.Apply(false);
            return tex;
        }
    }

    public enum RouteIconKind { Kill, Tick, Down, Start, Exfil }
}
