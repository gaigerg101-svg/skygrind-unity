// NIGHT74 builder B (slate B2, design.n74.target74, a VIEW row: NightRows.ViewOn(cfg, "target74") EVERY frame, never `enabled` written here; off: -skygrindRows72 target74:0, Rows72.cs).
// gg-proxy NIGHT73 close #3 (Logs/n73/gg-proxy-close.md): "crown37-stagger: the HUD says OPEN 2.9 (640-775, 125-155) and STAGGERED: EVERY HIT COUNTS, but no target is visible in the frame". GG 2026-10-12:
// bosses "you just teleport into these random rooms ... hard to understand what to do". While a boss bar reads OPEN (the window: a shot takes full damage) and the open weak point is NOT on
// the screen, the bar's tag names the way ("OPEN 2.9 - ABOVE", the bar's own face: 26 px with hud47) and a cyan edge chevron points at it. The weak point is cyan, so the chevron is: it points
// at the one cyan thing the boss has (rule W). Nothing is drawn, and the tag is unchanged, while the weak point is on screen, and while the row is off.
//   - the weak point: BossAttackClock63's `weak` of the live boss in its window (a.phase == Stagger): THE WICK's core, THE ICEBREAKER's torso, THE KILN WYRM's head or exposed core, THE LOOM's
//     spinneret, THE CROWN's ring (its top blade). One truth for the reel, the bars and this row.
//   - off screen: the point's viewport position outside 0..1, or behind the camera (Weak74Math.Direction: BEHIND, else the larger overshoot ABOVE / BELOW / LEFT / RIGHT).
//   - the chevron: two bars meeting at a tip on the screen edge (UnityTuning.N74Target*), pointing along the camera-local bearing of the point, pulsing under 2 Hz.
// View only: no sim step, no tape bit, no fingerprint. THE PROOF (L-014): `[target74] dir=<ABOVE|BELOW|LEFT|RIGHT|BEHIND> boss <id> | tag "<text>" at <px> px | chevron at <x>,<y> pointing <deg> deg | row on`
// the first frame each direction shows (nothing with the row off); the boss63 reel's [weak74] line adds `target74 row on|off, weak point <on screen|off screen DIR|no open weak point>` at every shot
// (so the OFF frame of a pair says what the ON frame drew); the reel's `-skygrindTarget74Away=<yawDeg>` turns the camera away from the weak point for the stagger frames (0: not planted).
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The pure maths (no Unity object): where the chevron sits on the screen's edge, the composed tag, the word count. EditMode: N74BTargetTests.</summary>
    public static class Target74Math
    {
        /// <summary>The point on the rectangle (half sizes hw, hh, less `inset` on each side) where the ray from the centre along (dx, dy) leaves it (UI px, y up), and the angle of that
        /// direction in degrees (0 = right, 90 = up). A zero direction points left (the shorter turn is arbitrary behind you).</summary>
        public static void Edge(double dx, double dy, double hw, double hh, double inset, out double ex, out double ey, out double angleDeg)
        {
            if (Math.Abs(dx) < 1e-9 && Math.Abs(dy) < 1e-9) { dx = -1; dy = 0; }
            double len = Math.Sqrt(dx * dx + dy * dy);
            double ux = dx / len, uy = dy / len;
            double bx = Math.Max(1, hw - inset), by = Math.Max(1, hh - inset);
            double tx = Math.Abs(ux) > 1e-9 ? bx / Math.Abs(ux) : double.PositiveInfinity;
            double ty = Math.Abs(uy) > 1e-9 ? by / Math.Abs(uy) : double.PositiveInfinity;
            double t = Math.Min(tx, ty);
            ex = ux * t;
            ey = uy * t;
            angleDeg = Math.Atan2(uy, ux) * 180.0 / Math.PI;
        }

        /// <summary>The bar's tag with the direction: "OPEN 2.9" + " - " + "ABOVE".</summary>
        public static string Tag(string baseTag, string dir, string sep) => string.IsNullOrEmpty(dir) ? baseTag : (baseTag ?? "") + sep + dir;

        public static int Words(string s) => string.IsNullOrWhiteSpace(s) ? 0 : s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public static class Target74
    {
        public const string Row = "target74";

        static int onFrame = -1;
        static bool onNow;
        static GameConfig onCfg;

        /// <summary>The row, uncached (tests).</summary>
        public static bool Decide(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);

        /// <summary>Is the row on this frame? (Read every frame; once a frame per config.)</summary>
        public static bool On(GameConfig cfg)
        {
            int f = Time.frameCount;
            if (f != onFrame || !ReferenceEquals(cfg, onCfg)) { onFrame = f; onCfg = cfg; onNow = Decide(cfg); }
            return onNow;
        }

        /// <summary>For the reel and the tests: what the last Ui frame did. Dir null: nothing drawn.</summary>
        public static string Dir, Boss, Tag;
        public static float TagPx, ChevronX, ChevronY, ChevronDeg;
        public static int Drawn;

        /// <summary>The live boss's open weak point (web space): the first live boss in its window (BossAttackClock63: phase Stagger). False: no boss in a window.</summary>
        public static bool WeakPoint(SurvivalSim sim, out Vec3 weak, out string boss)
        {
            weak = default;
            boss = null;
            BossAttack63 a = BossAttackClock63.Read(sim);
            if (!a.Live || a.phase != BossPhase63.Stagger) return false;
            weak = a.weak;
            boss = a.boss;
            return true;
        }

        /// <summary>The weak point's direction word for the camera, null when it is on screen. `local` is the point in the camera's own axes (right, up, forward): the chevron's bearing.</summary>
        public static string Where(Camera cam, Vec3 weak, out Vector3 local)
        {
            Vector3 world = WebSpace.ToUnity(weak);
            local = cam.transform.InverseTransformPoint(world);
            Vector3 vp = cam.WorldToViewportPoint(world);
            return Weak74Math.Direction(vp.x, vp.y, vp.z);
        }

        /// <summary>The reel's status text for a shot: "on screen", "off screen DIR" or "no open weak point" (read whatever the row says, so an OFF frame names what an ON frame drew).</summary>
        public static string Status(SurvivalGame g)
        {
            if (g == null || g.sim == null || g.cam == null) return "no game";
            if (!WeakPoint(g.sim, out Vec3 weak, out string boss)) return "no open weak point";
            string dir = Where(g.cam, weak, out _);
            return dir == null ? "on screen (" + boss + ")" : "off screen " + dir + " (" + boss + ")";
        }

        static string turnArg;
        static bool turnRead;

        /// <summary>The reel's planted turn (-skygrindTarget74Away=<yawDeg>): degrees to the right of the weak point the stagger frames look; 0 when not given. Command line only.</summary>
        public static double TurnAwayDeg()
        {
            if (!turnRead)
            {
                turnRead = true;
                try
                {
                    foreach (string a in Environment.GetCommandLineArgs())
                        if (a != null && a.StartsWith("-skygrindTarget74Away=", StringComparison.OrdinalIgnoreCase)) turnArg = a.Substring("-skygrindTarget74Away=".Length);
                }
                catch (Exception) { turnArg = null; }
            }
            return turnArg != null && double.TryParse(turnArg, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? d : 0.0;
        }
    }
}
