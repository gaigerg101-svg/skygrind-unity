// NIGHT73 builder B (design.n73.core73, a VIEW row: NightRows.ViewOn(cfg, "core73") EVERY frame, never `enabled` written here; off: -skygrindRows72 core73:0, Rows72.cs). gg-proxy #4 at NIGHT73: THE KILN WYRM's
// open weak point (while STAGGERED / OPEN) is a flat translucent cyan BOX over its body. What draws it (read, not run; the n73-f frame f0548 and the finish55c strip):
//   BossMarks32.cs (NIGHT32, design.n32.bossMarks; Boss31View.Render's hook): `if (open) Shell(w.boxes[0], OpenSwell, BossColours63.ShellRim(game.cfg, "wyrm.mark", Cyan, Pulse()))`: an ADDITIVE cyan-white
//     shell 1.45 x the head's hurtbox (3 x 3 x 4 m: 4.35 x 4.35 x 5.8 m), flat colour 1.36-1.6, pulsing at 3 Hz (OpenPulseHz) - the big translucent square; plus the head's 12 edge beams (OutlineGlow 0.35);
//   WyrmView.cs:171 (wyrmLook47 on) and WyrmView.Body72.cs (special72): the head's own box lit in the weak role (BossColours63 "wyrm.open", NIGHT63 B3b) while OPEN - the saturated opaque cyan box inside it.
// With the row ON the open weak point is a CORE: a soft cyan glow (concentric additive spheres, the outermost 1.5 x the sim's core radius, brightest at its middle, falling off to nothing at its edge) set where the
// sim's core is (the head's back: Wyrm.coreUp / coreBack / coreR), half in the head's box so the box's depth cuts it into a dome on the back; the 1.45 x translucent box is not drawn; the head keeps its own
// body colour (the cyan "shoot here" is the core's); the pulse is 1.2 Hz and 12 % deep (under 2 Hz), the glow fades in over 0.25 s on opening; the head's edge beams stay. The colour is the same cyan role colour
// (BossColours63.ShellRim, "wyrm.mark": the weak role through the fight's grade rim, as today's shell). The row OFF draws exactly today's box. The pure maths (the layers, the pulse, the fade) is Core73Math,
// pinned by Core73Tests. The proof (L-014): one Player log line on each open, row on or off:
//   [core73] wyrm weak point: row ON|off, drawn as core|box | core <R> m (<n> layers, steps <max> of the peak) = <d> px across, today's box (1.45 x the head) = <w> x <h> px on screen | pulse <hz> Hz | head lit cyan|body
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>The row's name and its read (every frame).</summary>
    public static class Core73
    {
        public const string Row = "core73";

        public static bool On(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);
    }

    /// <summary>The pure maths of the core's glow (no engine call): a radial falloff built from concentric additive discs, a slow pulse, a fade-in.</summary>
    public static class Core73Math
    {
        /// <summary>The falloff profile: peak at the middle (rho 0), 0 at the edge (rho 1): peak x (1 - rho^2)^power (power 1.5-2 reads as a lit core with a soft edge, power 1 as a dome).</summary>
        public static float Profile(float rho, float power, float peak)
        {
            if (rho >= 1f) return 0f;
            if (rho < 0f) rho = 0f;
            return peak * (float)Math.Pow(1.0 - (double)rho * rho, power);
        }

        /// <summary>`n` concentric discs, the outermost first: radius[i] is the disc's share of the outer radius ((n - i) / n, so radius[0] is 1), glow[i] what it adds. A point in the ring between disc i + 1 and
        /// disc i is covered by discs 0..i, so the brightness there is glow[0] + .. + glow[i], set to Profile at the ring's middle: glow[0] = Profile(mid 0), glow[i] = Profile(mid i) - Profile(mid i - 1).
        /// The sum of the glows is the centre's brightness (the peak, less the last ring's small drop).</summary>
        public static void Layers(int n, float power, float peak, float[] radius, float[] glow)
        {
            if (n < 1) return;
            float prev = 0f;
            for (int i = 0; i < n; i++)
            {
                float outer = (n - i) / (float)n, inner = (n - i - 1) / (float)n;
                radius[i] = outer;
                float at = Profile((outer + inner) * 0.5f, power, peak);
                glow[i] = at - prev;
                prev = at;
            }
        }

        /// <summary>ROUND 2 (the lead's look: the core above the head was a faint haze and the "shoot here" mark was gone): where the glow sits. The point where the line from an oriented box's centre toward `eye` leaves
        /// the box: on the face the camera sees, at the box's own centre on screen (the head's middle as the camera has it). `axis` 0, 1, 2 = the box's r, u, f face it leaves by. An eye inside the box gives the eye's own
        /// side (t capped at 1: never past the eye). A degenerate eye (on the centre) gives the centre.</summary>
        public static Vec3 FrontPoint(Vec3 c, Vec3 r, Vec3 u, Vec3 f, double hr, double hu, double hf, Vec3 eye, out int axis)
        {
            Vec3 d = eye - c;
            double dr = Math.Abs(Vec3.Dot(d, r)), du = Math.Abs(Vec3.Dot(d, u)), df = Math.Abs(Vec3.Dot(d, f));
            double tr = dr > 1e-12 ? hr / dr : double.PositiveInfinity, tu = du > 1e-12 ? hu / du : double.PositiveInfinity, tf = df > 1e-12 ? hf / df : double.PositiveInfinity;
            double t = Math.Min(tr, Math.Min(tu, tf));
            axis = tr <= tu && tr <= tf ? 0 : tu <= tf ? 1 : 2;
            if (double.IsInfinity(t)) return c;
            return c + d * Math.Min(t, 1.0);
        }

        /// <summary>The glow's outer radius on the face: `share` of the box's smallest half size, so the dome stays inside the face's outline from the front (the head 3 x 3 x 4 m: 1.5 m x share).</summary>
        public static double FaceRadius(double hr, double hu, double hf, double share) => share * Math.Min(hr, Math.Min(hu, hf));

        /// <summary>The pulse: 1 at its top, 1 - depth at its bottom, `hz` cycles a second (starts at the top).</summary>
        public static float Pulse(double t, float hz, float depth) => 1f - depth * (0.5f - 0.5f * (float)Math.Cos(2.0 * Math.PI * hz * t));

        /// <summary>The fade-in: 0 at the open, 1 after `seconds` (a smooth step: no pop, no flash).</summary>
        public static float Fade(double since, float seconds)
        {
            if (seconds <= 0f || since >= seconds) return 1f;
            if (since <= 0.0) return 0f;
            float x = (float)(since / seconds);
            return x * x * (3f - 2f * x);
        }
    }
}
