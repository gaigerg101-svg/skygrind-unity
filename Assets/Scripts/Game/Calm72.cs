// NIGHT72 builder L (L4; design.n72.calm72, a view row: NightRows.ViewOn(cfg, "calm72") every frame, never `enabled` written here): no THICK red border at full
// health (the first build: none at all; reworked in NIGHT73 K2, below). gg-proxy.md #10: "a red speckled noise frame 150-250 px deep eats all four edges at 9999 health"
// (the wyrm's stagger and windup frames, n71-forge72), and GG's own screenshots at 45 and 31 health show a thick red frame; GG 2026-10-06: "not just that
// weird border that only appears".
// THE SOURCES (read, not guessed; the HUD is Ui.cs's and none of it is edited here):
//   1. EXPOSED's pulsing edge, `exposedEdge` ("Exposed edge", UiFeelEdges.cs:50-86; UnityTuning ExposedEdge*: blur 90, spread 30, so about 120-150 px deep):
//      while the director calls the player EXPOSED (standing still), alpha 0.35 + 0.35 sin(7 t), at any health. The reel's boss frames stand still.
//   2. the hurt flash, `vignette` ("Hurt vignette", Ui.cs:1166-1168): alpha hud.hurtVignetteOpacity 0.6 falling over 0.35 s after EVERY hit, at any health;
//      and (Ui.cs:1169-1184) while frac <= health.lowHpFraction its breath, base 0.18 + 0.16 x the beat.
//   3. the damage vignette, `damageVignette` ("Damage vignette", Ui.cs:881-887): alpha (1 - frac) x regen.vignetteMax 0.5, eased; 0.28 at 45 %, 0.35 at 31 %.
//   Both vignettes' sprites are UiEdge31.cs's frames (clear 0.86 / 0.88 of the half-size: the outer 7 % of the width, 135 px a side, 76 px top and bottom).
//   The speckle in the frames is the CRT grain (Crt29.shader) and Juice38's damage static riding bright red: the border is what shows it.
// NIGHT73 K2 (the lead: "calm72, reworked so nothing the player had is lost", PRINCIPLES rule 1): the first build DISABLED the hurt flash, the damage vignette and the
// EXPOSED edge at or above 30 % health: the hit cue and the anti-camp cue were gone. GG's complaint is the THICK speckled frame, not the cues. NOW, with the row ON,
// at ANY health, from LateUpdate (Crt29Driver ticks it after Ui has written its alphas this frame, so nothing draws one frame late):
//   - the hurt flash, the damage vignette and the EXPOSED edge ALL still draw, at the alphas Ui wrote, on a THIN frame (UnityTuning.L72CalmEdgeClear of the half-size:
//     58 px deep at 1920 wide, 32 px at the top and bottom at 1080p; the three Images' sprites are swapped for thin ones and put back with the row);
//   - EXPOSED's pulse (0.35 + 0.35 sin(7 t): full alpha to nothing, the flicker GG named) keeps its mean and loses more than half its amplitude: Calm72Math.ExposedAlpha
//     (UnityTuning.Calm73ExposedSwingScale 0.45: 0.35 +- 0.16 at the edge of the frame). Ui writes the Image's alpha only when its own value changes, so what the HUD wrote
//     is kept apart from what is drawn (the colour's alpha is the drawn one; the HUD's last write is remembered and put back with the row);
//   - under UnityTuning.L72CalmEdgeBelow (30 %) of health nothing else changes: the thin vignettes were all calm72 showed there; PostFx's own low-HP vignette starts at
//     the same 30 % (PostFx.cs, the `hurt` line);
//   - kiln73's K2 (the EXPOSED edge hidden in THE KILN WYRM's arena) is RETIRED (NIGHT73 K3, the night's review S1): calm72 owns the three edges in every room, the arena
//     included (ON: thin; OFF: the HUD's own, the NIGHT72 game's); kiln73 only reads them.
// NIGHT73 K3 (the night's review): the HUD is rebuilt with every scene load, so the search allowance (UnityTuning.Calm73FindMax) and what is kept per HUD (the sprites each Image had,
// the EXPOSED type) are per HUD: an Image found once that is gone now drops all of it (Calm72Math.HudLost), the allowance is whole again (Calm72Search.Reset) and the sprites are
// kept again from the new Images (S2, S3). EXPOSED's swell age follows the loop's own clock, clamped as SurvivalGame clamps it, every frame whatever the row reads (S4).
// The sim is not touched: the exposed state, the hits and the health are as they were (fingerprints EQUAL). Off (the row OFF, or -skygrindNoCalm72): the
// Images are put back (sprite, type, enabled, alpha) at once. The three Images are enabled by the HUD and stay enabled (calm72 never holds an `enabled` it did not set).
// The Player line (L-089): `[calm72] ...` on every change of what the HUD wrote and what is drawn (frame depth in px, the drawn alpha), and `[calm72] hit ...` once per hit
// at the flash's peak (read OFF, then ON with -skygrindRows72 calm72:1). Tools/night/n73-edge.py measures the red border's depth in px on the frames.
using System;
using System.Globalization;
using System.Text;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>calm72's pure maths.</summary>
    public static class Calm72Math
    {
        /// <summary>The edge may show: health strictly under `below` of its maximum.</summary>
        public static bool EdgeOn(float frac, float below) => frac < below;

        /// <summary>How deep (px) the thin frame's alpha ramp runs on a screen of `size` px across the axis: (1 - clear) of the half-size.</summary>
        public static double DepthPx(double clear, double sizePx) => (1 - clear) * sizePx / 2;

        /// <summary>The age-based swell Ui gives the EXPOSED edge when it starts (UiHudMotion18.FlashAttack18: InCubic over `attackS`; 1 with FeelAB off).</summary>
        public static float ExposedAttack(double age, bool feelAb, double attackS)
        {
            if (!feelAb) return 1f;
            double x = attackS <= 0 ? 1 : Math.Max(0, Math.Min(1, age / attackS));
            return (float)(x * x * x);
        }

        /// <summary>The EXPOSED edge's drawn alpha from the alpha Ui wrote (`wrote` = f x (base + swing sin t), f the swell): the pulse keeps its mean and keeps `scale` of its
        /// amplitude, so a trough rises and a peak falls: scale x wrote + (1 - scale) x base x f. scale 1 is Ui's; 0.5 halves the peak-to-trough range.</summary>
        public static float ExposedAlpha(float wrote, float f, float baseAlpha, float scale) => scale * wrote + (1f - scale) * baseAlpha * f;

        /// <summary>A hit's flash starts rising: the HUD's hurt alpha went up by at least `jump` since the last frame (a hit is 0 -> 0.6 in three frames; the low-HP breath moves
        /// by hundredths a frame).</summary>
        public static bool HitStart(float prev, float now, float jump) => now - prev >= jump;

        /// <summary>NIGHT73 K3 (the night's review S2 / S3): the HUD is rebuilt with every scene load (SurvivalGame.Init: `ui = new Ui(this)`), which destroys the three Images. The HUD is
        /// LOST when an Image that was found once is gone now: everything kept from it (the refs, the sprites, the search count) is dropped and the new HUD is searched for afresh.</summary>
        public static bool HudLost(bool hurtSeen, bool hurtAlive, bool damageSeen, bool damageAlive, bool exposedSeen, bool exposedAlive) =>
            (hurtSeen && !hurtAlive) || (damageSeen && !damageAlive) || (exposedSeen && !exposedAlive);
    }

    /// <summary>calm72's HUD search (pure; Calm72Tests): at most `Max` searches, `Every` seconds apart, while an Image is missing; `Reset` (a lost HUD) gives the new one a full allowance.
    /// (The first build's count was static for the session and never reset: after about 40 level loads calm72 silently stopped finding the HUD: the night's review S2.)</summary>
    public sealed class Calm72Search
    {
        public readonly int Max;
        public readonly double Every;
        public int Tries { get; private set; }
        double next;

        public Calm72Search(int max, double every)
        {
            Max = max;
            Every = every;
        }

        /// <summary>A search is due: an Image is missing, the allowance is not spent and the pause has passed.</summary>
        public bool Due(bool anyMissing, double now) => anyMissing && Tries < Max && now >= next;

        public void Searched(double now)
        {
            Tries++;
            next = now + Every;
        }

        public void Reset()
        {
            Tries = 0;
            next = 0;
        }
    }

    /// <summary>calm72's live state, ticked once a frame by Crt29Driver.LateUpdate.</summary>
    public static class Calm72
    {
        public const string Row = "calm72";

        public static bool Active { get; private set; }
        /// <summary>Health is under the low-health bar (UnityTuning.L72CalmEdgeBelow) this frame, the row ON: PostFx's low-HP vignette is live. The three edges draw at any health.</summary>
        public static bool EdgeLive { get; private set; }
        /// <summary>The last [calm72] line (the reel prints it).</summary>
        public static string LastProbe { get; private set; } = "";
        /// <summary>The last [calm72] hit line (one per hit, at the flash's peak).</summary>
        public static string LastHit { get; private set; } = "";
        /// <summary>Hits seen with the row ON (the flash's rises).</summary>
        public static int Hits { get; private set; }

        static Image hurt, damage, exposed;
        // an Image found once (so its going is a lost HUD, Calm72Math.HudLost) and the Image each kept sprite was taken from (a kept sprite belongs to ONE HUD: it is taken again for another)
        static bool hurtSeen, damageSeen, exposedSeen;
        static Image hurtFrom, damageFrom, exposedFrom;
        static Sprite hurtSpr0, damageSpr0, exposedSpr0, thinHurt, thinDamage, thinExposed;
        static Image.Type exposedType0;
        static bool applied;
        static float nextProbe;
        static readonly Calm72Search search = new Calm72Search(UnityTuning.Calm73FindMax, UnityTuning.Calm73FindEveryS);
        static int lastKey = -1;
        static int probeLines, hitLines;
        // EXPOSED: what the HUD last wrote, what calm72 last drew, how long the director has called the player exposed (Ui's own exposedT)
        static float exposedWrote, exposedOut;
        static bool exposedHasOut;
        static double exposedAge;
        // the hurt flash: last frame's alpha, and whether it is rising (a hit: the peak is logged when the rise ends)
        static float prevHurt;
        static bool hurtRising;

        static void Find()
        {
            foreach (Image im in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (hurt == null && im.name == "Hurt vignette") { hurt = im; hurtSeen = true; }
                else if (damage == null && im.name == "Damage vignette") { damage = im; damageSeen = true; }
                else if (exposed == null && im.name == "Exposed edge") { exposed = im; exposedSeen = true; }
            }
        }

        /// <summary>The HUD that was found is gone (a scene load rebuilt it): what was kept from it is dropped (nothing is restored onto destroyed Images, and no sprite of the old HUD is
        /// ever put on the new one), the search allowance is whole again, and the hurt / EXPOSED trackers start from the new HUD.</summary>
        static void HudLost()
        {
            hurt = damage = exposed = null;
            hurtSeen = damageSeen = exposedSeen = false;
            hurtFrom = damageFrom = exposedFrom = null;
            hurtSpr0 = damageSpr0 = exposedSpr0 = null;
            applied = false;
            exposedHasOut = false;
            exposedWrote = exposedOut = 0;
            hurtRising = false;
            prevHurt = 0;
            search.Reset();
            Hud++;
            Debug.Log($"[calm72] the HUD was rebuilt (a scene load; {Hud} so far): the sprites kept from the old one are dropped, the Image search starts again ({UnityTuning.Calm73FindMax} allowed)");
        }

        /// <summary>HUDs lost so far (the reel's scene loads, counted: the search starts again for each).</summary>
        public static int Hud { get; private set; }

        /// <summary>A frame: alpha 0 inside the box |x|, |y| below `clear` of the half-size, a smooth ramp to `edgeAlpha` at the screen's edge (UiEdge31's frame, thinner).</summary>
        static Sprite Thin(float clear, float edgeAlpha, string name)
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = name };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Abs((x + 0.5f) / n * 2 - 1), dy = Mathf.Abs((y + 0.5f) / n * 2 - 1);
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Mathf.Max(dx, dy) - clear) / (1 - clear))) * edgeAlpha;
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        static float A(Image im) => im != null ? im.color.a : 0f;
        static string F(float v) => v.ToString("0.00", CultureInfo.InvariantCulture);

        /// <summary>This frame: the row read now; the three HUD Images held or put back; one probe line when what shows changes.</summary>
        public static void Tick(SurvivalGame g)
        {
            if (g == null || g.cfg == null || g.health == null) return;
            // NIGHT73 K3 (review S2 / S3): the HUD is rebuilt with every scene load; an Image found once that is gone now is a lost HUD: everything kept from it is dropped and the search is whole again
            if (Calm72Math.HudLost(hurtSeen, hurt != null, damageSeen, damage != null, exposedSeen, exposed != null)) HudLost();
            // the three Images are built with the HUD: found within a few tries; a missing one (a HUD without it) is searched for UnityTuning.Calm73FindMax times (20 s), then left (no scene scan
            // forever); a new HUD gets the whole allowance again
            if (search.Due(hurt == null || damage == null || exposed == null, Time.unscaledTime)) { Find(); search.Searched(Time.unscaledTime); }
            // NIGHT73 K3 (review S4): how long the director has called the player exposed, on Ui's own clock (the loop's frame delta, clamped as SurvivalGame clamps it: the reel's fixed clock,
            // 0 in a held frame), advanced every frame whatever the row reads, so the first ON frame after a hold is the frame it would have drawn
            bool exNow = g.director != null && g.director.Exposed;
            exposedAge = exNow ? exposedAge + Math.Min(FeelClock.FrameDelta, g.cfg.movement.loop.maxFrameDelta) : 0;
            bool on = NightRows.ViewOn(g.cfg, Row);
            double frac = g.health.max > 0 ? g.health.hp / g.health.max : 1;
            if (!on)
            {
                if (applied) Restore();
                Active = false;
                EdgeLive = false;
                hurtRising = false;
                prevHurt = A(hurt);
                Probe(false, frac, false);
                return;
            }
            // each Image's own sprite (and the exposed edge's type) is kept the first time THAT Image is seen with the row ON, and again for any other Image (review S3: a kept sprite
            // belongs to one HUD; Restore never puts another HUD's sprite on this one)
            if (hurt != null && !ReferenceEquals(hurtFrom, hurt)) { hurtFrom = hurt; hurtSpr0 = hurt.sprite; }
            if (damage != null && !ReferenceEquals(damageFrom, damage)) { damageFrom = damage; damageSpr0 = damage.sprite; }
            if (exposed != null && !ReferenceEquals(exposedFrom, exposed)) { exposedFrom = exposed; exposedSpr0 = exposed.sprite; exposedType0 = exposed.type; exposedHasOut = false; }
            Active = true;
            bool low = Calm72Math.EdgeOn((float)frac, UnityTuning.L72CalmEdgeBelow);
            EdgeLive = low;
            if (thinHurt == null)
            {
                thinHurt = Thin(UnityTuning.L72CalmEdgeClear, UnityTuning.Edge31HurtEdgeAlpha, "Hurt vignette calm72");
                thinDamage = Thin(UnityTuning.L72CalmEdgeClear, UnityTuning.Edge31DamageEdgeAlpha, "Damage vignette calm72");
                thinExposed = Thin(UnityTuning.L72CalmEdgeClear, UnityTuning.ExposedEdgeAlpha, "Exposed edge calm72");
            }
            // all three draw at any health, on the thin frame; the HUD builds them enabled and never toggles `enabled`, so they are made enabled (never held at a value read from someone else's write)
            if (hurt != null) { if (hurt.sprite != thinHurt) hurt.sprite = thinHurt; hurt.enabled = true; }
            if (damage != null) { if (damage.sprite != thinDamage) damage.sprite = thinDamage; damage.enabled = true; }
            if (exposed != null)
            {
                if (exposed.sprite != thinExposed) { exposed.sprite = thinExposed; exposed.type = Image.Type.Simple; } // the HUD's is a 9-slice inset shadow: the thin frame is one stretched sprite
                exposed.enabled = true;
                ExposedPulse(exNow);
            }
            HurtWatch(frac);
            applied = true;
            Probe(true, frac, low);
        }

        /// <summary>The EXPOSED edge's alpha: Ui writes the Image's colour only when its own (rounded) value changes, so a value that is not the one calm72 last drew is Ui's new write;
        /// the drawn alpha is Calm72Math.ExposedAlpha of it while the director calls the player exposed (not exposed: Ui's, which is 0).</summary>
        static void ExposedPulse(bool exNow)
        {
            Color c = exposed.color;
            if (!exposedHasOut || Mathf.Abs(c.a - exposedOut) > 1e-4f) exposedWrote = c.a;
            float f = Calm72Math.ExposedAttack(exposedAge, FeelAB.On, UnityTuning.Hud18FlashAttack);
            float o = exNow ? Calm72Math.ExposedAlpha(exposedWrote, f, (float)UnityTuning.ExposedEdgeBase, UnityTuning.Calm73ExposedSwingScale) : exposedWrote;
            if (Mathf.Abs(o - c.a) > 1e-4f) { c.a = o; exposed.color = c; }
            exposedOut = o;
            exposedHasOut = true;
        }

        /// <summary>The hurt flash, once per hit: a rise of at least UnityTuning.Calm73HitJump in a frame starts one, the line is written when the rise ends (the flash's peak).</summary>
        static void HurtWatch(double frac)
        {
            float wh = A(hurt);
            if (hurt != null)
            {
                if (Calm72Math.HitStart(prevHurt, wh, UnityTuning.Calm73HitJump)) hurtRising = true;
                else if (hurtRising && wh <= prevHurt)
                {
                    hurtRising = false;
                    Hits++;
                    if (hitLines < UnityTuning.L72CalmProbeMax)
                    {
                        hitLines++;
                        LastHit = new StringBuilder("[calm72] hit ").Append(Hits).Append(" at ").Append(Time.unscaledTime.ToString("0.0", CultureInfo.InvariantCulture)).Append(" s | health ").Append((frac * 100).ToString("0", CultureInfo.InvariantCulture))
                            .Append(" % | the HUD wrote hurt ").Append(F(prevHurt)).Append(" (peak) | ").Append(Drawn("hurt", hurt, prevHurt)).ToString();
                        Debug.Log(LastHit);
                    }
                }
            }
            prevHurt = wh;
        }

        static void Restore()
        {
            if (hurt != null) { hurt.sprite = hurtSpr0; hurt.enabled = true; }
            if (damage != null) { damage.sprite = damageSpr0; damage.enabled = true; }
            if (exposed != null)
            {
                exposed.sprite = exposedSpr0;
                exposed.type = exposedType0;
                exposed.enabled = true;
                if (exposedHasOut)
                {
                    Color c = exposed.color;
                    c.a = Mathf.Abs(c.a - exposedOut) > 1e-4f ? c.a : exposedWrote; // the HUD's own last write (or one it made since): its next compare (its shown value) holds
                    exposed.color = c;
                }
            }
            exposedHasOut = false;
            hurtRising = false;
            applied = false; // exposedAge is NOT reset: it follows the director's state whatever the row reads (review S4)
        }

        /// <summary>What an Image draws now, in words: its drawn alpha with the frame it is on ("thin 58 x 32 px" is calm72's sprite; any other sprite is named: the HUD's), or 0.</summary>
        static string Drawn(string what, Image im, float alpha)
        {
            if (im == null || !im.enabled || alpha <= 0.005f) return what + " 0";
            bool thin = im.sprite != null && (ReferenceEquals(im.sprite, thinHurt) || ReferenceEquals(im.sprite, thinDamage) || ReferenceEquals(im.sprite, thinExposed));
            string frame = thin
                ? "thin " + Calm72Math.DepthPx(UnityTuning.L72CalmEdgeClear, Screen.width).ToString("0", CultureInfo.InvariantCulture) + " x " + Calm72Math.DepthPx(UnityTuning.L72CalmEdgeClear, Screen.height).ToString("0", CultureInfo.InvariantCulture) + " px"
                : "sprite \"" + (im.sprite != null ? im.sprite.name : "none") + "\" (the HUD's)";
            return what + " " + F(alpha) + " " + frame;
        }

        /// <summary>One line when what the HUD wrote or what draws changes (a bitmask, no garbage); at most one in 0.5 s, 200 a run. The drawn alphas: the hurt flash's and the damage
        /// vignette's are the HUD's (the sprite is the frame); the EXPOSED edge's is calm72's (Calm72Math.ExposedAlpha of the HUD's) with the row ON.</summary>
        static void Probe(bool on, double frac, bool low)
        {
            if (probeLines >= UnityTuning.L72CalmProbeMax || Time.unscaledTime < nextProbe) return;
            float wh = A(hurt), wd = A(damage);
            float we = on && exposedHasOut ? exposedWrote : A(exposed), de = A(exposed);
            bool dh = hurt != null && hurt.enabled && wh > 0.005f, dd = damage != null && damage.enabled && wd > 0.005f, dx = exposed != null && exposed.enabled && de > 0.005f;
            // a bitmask, not a string: this runs every frame (no garbage)
            int key = (on ? 1 : 0) | (wh > 0.005f ? 2 : 0) | (wd > 0.005f ? 4 : 0) | (we > 0.005f ? 8 : 0) | (dh ? 16 : 0) | (dd ? 32 : 0) | (dx ? 64 : 0) | (low ? 128 : 0);
            if (key == lastKey) return;
            lastKey = key;
            nextProbe = Time.unscaledTime + 0.5f;
            probeLines++;
            var sb = new StringBuilder("[calm72] ").Append(on ? "ON" : "off").Append(" | health ").Append((frac * 100).ToString("0", CultureInfo.InvariantCulture)).Append(" % | under the low-health bar ").Append(low ? "yes" : "no")
                .Append(" | the HUD wrote hurt ").Append(F(wh)).Append(" damage ").Append(F(wd)).Append(" exposed ").Append(F(we))
                .Append(" | drawn ").Append(Drawn("hurt", hurt, wh)).Append(", ").Append(Drawn("damage", damage, wd)).Append(", ").Append(Drawn("exposed", exposed, de));
            LastProbe = sb.ToString();
            Debug.Log(LastProbe);
        }
    }
}
