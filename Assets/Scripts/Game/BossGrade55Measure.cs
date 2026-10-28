// NIGHT55 builder B: a155's colour measure (FeelReel.N55.Boss.cs), pure and tested (N55BossGradeTests). A captured frame is sampled on a grid; each
// sample's CLASS comes from the render's own geometry (a ray from the camera into the C# world collider and the enemy registry: wall / floor /
// ceiling / slope / body / sky; the HUD's graphics' rects and the viewmodel's renderers' rects mask their samples out), never from its colour. Only
// the danger-tell class is by colour, as the slate's contract defines it ("the danger-tell pixels (red-orange stripes / tells)"): a sample whose
// HSV hue is within tellBand of the danger hue, saturation >= tellSat and value >= tellVal. Per class: the count, the circular mean hue weighted by
// saturation (HSV, as a254), the mean saturation / value / linear colour, the Oklab hue and chroma, and the share of samples near the danger hue.
using System;
using UnityEngine;

namespace Skygrind.Game
{
    public enum G55Class : byte { Sky = 0, Wall = 1, Floor = 2, Ceiling = 3, Slope = 4, Body = 5, Ui = 6, Viewmodel = 7, Count = 8 }

    /// <summary>One class's colour sums.</summary>
    public struct G55Stats
    {
        public int n, near;
        public double hx, hy, s, v, lr, lg, lb, ox, oy, oc;

        /// <summary>Adds one sample (an sRGB colour from the captured frame); `nearDeg`: within this many degrees of `dangerHue` with S >= 0.2
        /// counts as near the danger hue.</summary>
        public void Add(Color c, double dangerHue, double nearDeg)
        {
            Color.RGBToHSV(c, out float h, out float sat, out float val);
            double a = h * 2 * Math.PI;
            hx += sat * Math.Cos(a);
            hy += sat * Math.Sin(a);
            s += sat;
            v += val;
            Color l = c.linear;
            lr += l.r; lg += l.g; lb += l.b;
            G55Measure.Oklab(l, out double L, out double oa, out double ob);
            ox += oa; oy += ob;
            oc += Math.Sqrt(oa * oa + ob * ob);
            if (sat >= 0.2f && G55Measure.HueDist(h * 360, dangerHue) < nearDeg) near++;
            n++;
        }

        public void Add(G55Stats o)
        {
            n += o.n; near += o.near; hx += o.hx; hy += o.hy; s += o.s; v += o.v; lr += o.lr; lg += o.lg; lb += o.lb; ox += o.ox; oy += o.oy; oc += o.oc;
        }

        public bool Ok => n > 0;
        /// <summary>The circular mean hue (deg, 0..360) weighted by saturation (a254's measure); 0 with no samples.</summary>
        public double Hue { get { if (n == 0) return 0; double d = Math.Atan2(hy, hx) * 180 / Math.PI; return d < 0 ? d + 360 : d; } }
        public double S => n > 0 ? s / n : 0;
        public double V => n > 0 ? v / n : 0;
        public Color Lin => n > 0 ? new Color((float)(lr / n), (float)(lg / n), (float)(lb / n), 1) : Color.black;
        /// <summary>The Oklab hue (deg) of the mean a / b, and the mean chroma.</summary>
        public double OkHue { get { if (n == 0) return 0; double d = Math.Atan2(oy, ox) * 180 / Math.PI; return d < 0 ? d + 360 : d; } }
        public double OkChroma => n > 0 ? oc / n : 0;
        /// <summary>The share of samples within the near band of the danger hue (S >= 0.2).</summary>
        public double NearShare => n > 0 ? (double)near / n : 0;
    }

    public static class G55Measure
    {
        /// <summary>The hue distance in degrees (0..180).</summary>
        public static double HueDist(double h1, double h2)
        {
            double d = Math.Abs(h1 - h2) % 360;
            return d > 180 ? 360 - d : d;
        }

        /// <summary>Linear sRGB to Oklab (Bjorn Ottosson, public domain / MIT: https://bottosson.github.io/posts/oklab/).</summary>
        public static void Oklab(Color lin, out double L, out double a, out double b)
        {
            double l = 0.4122214708 * lin.r + 0.5363325363 * lin.g + 0.0514459929 * lin.b;
            double m = 0.2119034982 * lin.r + 0.6806995451 * lin.g + 0.1073969566 * lin.b;
            double s = 0.0883024619 * lin.r + 0.2817188376 * lin.g + 0.6299787005 * lin.b;
            double l_ = Math.Cbrt(l), m_ = Math.Cbrt(m), s_ = Math.Cbrt(s);
            L = 0.2104542553 * l_ + 0.7936177850 * m_ - 0.0040720468 * s_;
            a = 1.9779984951 * l_ - 2.4285922050 * m_ + 0.4505937099 * s_;
            b = 0.0259040371 * l_ + 0.7827717662 * m_ - 0.8086757660 * s_;
        }

        /// <summary>Is this sRGB sample a danger tell (by colour: the contract's red-orange class)?</summary>
        public static bool IsTell(Color c, double dangerHue, double bandDeg, double minS, double minV)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            return s >= minS && v >= minV && HueDist(h * 360, dangerHue) <= bandDeg;
        }

        /// <summary>The geometric class of a world ray hit's normal (web axes: +y up).</summary>
        public static G55Class OfNormal(double ny)
        {
            if (ny > 0.7) return G55Class.Floor;
            if (ny < -0.7) return G55Class.Ceiling;
            if (Math.Abs(ny) < 0.35) return G55Class.Wall;
            return G55Class.Slope;
        }

        /// <summary>WCAG relative luminance of a linear colour.</summary>
        public static double Luminance(Color lin) => 0.2126 * lin.r + 0.7152 * lin.g + 0.0722 * lin.b;

        /// <summary>WCAG contrast ratio of two linear colours.</summary>
        public static double ContrastRatio(Color a, Color b)
        {
            double la = Luminance(a), lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        /// <summary>The class colour drawn in the mask image (sky dark, walls grey-blue, floor brown, ceiling teal, slope olive, body white, UI
        /// black, viewmodel dark grey; a tell is drawn red over its class).</summary>
        public static Color32 MaskColour(G55Class c, bool tell, bool hot)
        {
            if (hot) return new Color32(255, 230, 0, 255);
            if (tell) return new Color32(255, 40, 20, 255);
            switch (c)
            {
                case G55Class.Wall: return new Color32(110, 130, 200, 255);
                case G55Class.Floor: return new Color32(150, 110, 70, 255);
                case G55Class.Ceiling: return new Color32(70, 160, 160, 255);
                case G55Class.Slope: return new Color32(140, 150, 60, 255);
                case G55Class.Body: return new Color32(255, 255, 255, 255);
                case G55Class.Ui: return new Color32(0, 0, 0, 255);
                case G55Class.Viewmodel: return new Color32(50, 50, 50, 255);
                default: return new Color32(20, 20, 35, 255);
            }
        }
    }
}
