// NIGHT73 builder K (design.n73.kiln73, a VIEW row: NightRows.ViewOn(cfg, "kiln73") EVERY frame, never `enabled` written here; off: -skygrindRows72 kiln73:0, Rows72.cs).
// CLAUDE.md rule 8 (red-orange is enemies and danger only); GG 2026-10-22 "effect colores everything doesnt feel alligned", 2026-10-26 "a lot of the room especially the
// boss rooms are very dark" (so the fix is hue, never darkness). The lead's meter (Tools/night/n73-hue.py: hue 0-40 / 345-360, S >= 0.35, V >= 0.20) read THE KILN WYRM's
// arena 92.5 / 94.8 / 88.1 / 44.2 / 95.8 % danger-hue pixels in GG's play build (n72-play, boss63 on n57-kiln).
// THE CAUSE (Research/2026-10-27-wave73/K/ledger.md, section 1; read and measured on older pair frames, not on the play build): the arena's stone is cool (pillarTop 0x877D92,
// pillarSide 0x524A7A; the level's own k5 stone wears palette72's kiln plum / sand), and THE WYRM's two boss grades turn it red-orange:
//   - BossGrade54's wyrm palette pushes its split-toning colours 1.8x away from grey (common.intensity): the shadows' blue goes below 0 and clamps, URP's SoftLight with a
//     blend of 0 squares the channel, and a wall (36, 25, 57) becomes (5, 4, 1) (n54 a254 pair); its colour filter cuts linear blue to 0.6, its saturation / contrast deltas harden it;
//   - BossGrade55's wyrm palette is amber-umber on purpose ("the yellow-red-orange theme kept through every phase": 7.3 % -> 51.9 % danger on the same frame, n56 a155 pair);
//   - lit72 then scales that dark saturated pixel up in its own hue (kmax 9): the amplifier, +10-15 points (lit72 OFF still reads 77-87 %).
// WHAT THIS DOES, row ON (view only; the sim, the director, the tells and the wyrm's body are untouched, so the fingerprints stay EQUAL):
//   K1 THE GRADES GO COOL: the wyrm's Grade54Boss and Grade55Boss phases (the public objects BossGrade54.PaletteOf / BossGrade55.PaletteOf hand out) take UnityTuning.Kiln73*
//      in place: a near-white filter, a plum split with no clamped channel, neutral highlights, a pale turn wash, a cool pale ambient / key (THE CROWN's), a dark plum fog, no
//      saturation delta, a small contrast delta, `turnSat` 0, and grade54's rim colour (`rimHdr`, pre-divided by P1's filter at BossGrade54.ReadBoss) recomputed against the
//      new filter. A deep copy is taken first and put back the frame the row reads OFF (D1's contract: the next ON frame is the frame it would have drawn). The grades, their
//      turns, THE WICK / THE CROWN / the other bosses' palettes and every other fight are not touched. The wyrm keeps a quiet plum identity; its red-orange is its body and tells.
//   K2 RETIRED (NIGHT73 K3; the night's review S1): the first build disabled the HUD's "Exposed edge" Image (UiFeelEdges.cs: 0xff280a, alpha 0.35 + 0.35 sin(7 t), shown while the
//      director's camp meter calls the player EXPOSED) while the arena was drawn, because that thick frame was the red border in the wyrm's frames. calm72 (Calm72.cs) owns the
//      edge now: ON it draws the thin, calmer EXPOSED frame in every room, the arena included; OFF the HUD's own edge is the NIGHT72 game's. With calm72's off switch K2 had
//      taken the anti-camp cue away in the arena only (PRINCIPLES rule 1). kiln73 NEVER touches the EXPOSED Image, the hurt flash or the damage vignette: it only READS them for
//      the edge probe below.
//   K3 NO RETINT OF THE ROOM MESH: WyrmRoomView's boxes are pillarTop / pillarSide (cool); a retint would add a writer with no measured job. The build line prints their colours.
// THE PROOF (L-014 / L-115): `[kiln73] wyrm arena: row ON|off ...` at the arena's build (WyrmRoomView.Build; it says the EXPOSED edge is calm72's), `[kiln73] wyrm grades: ...` at each
// flip of the palettes, `[kiln73] wyrm arena shown ...` each time the arena is drawn (the light and the grades' live state) and `[kiln73] edge probe ...` (read only) at each change of
// the director's EXPOSED state while it is drawn.
using System.Globalization;
using System.Text;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>kiln73's pure maths and palette copies (EditMode: Kiln73Tests).</summary>
    public static class Kiln73Math
    {
        public static Color C(float[] v) => new Color(v[0], v[1], v[2], 1f);
        public static Color C(float[] v, int i) => new Color(v[i * 3], v[i * 3 + 1], v[i * 3 + 2], 1f);

        /// <summary>BossGrade54.Push: `c` pushed away from `neutral` by `k`, each channel kept in 0..1.</summary>
        public static Color Push(Color c, Color neutral, float k) =>
            new Color(Mathf.Clamp01(neutral.r + (c.r - neutral.r) * k), Mathf.Clamp01(neutral.g + (c.g - neutral.g) * k), Mathf.Clamp01(neutral.b + (c.b - neutral.b) * k), 1);

        /// <summary>True when the pushed value hit a channel's end (an end clamps the soft-light blend: the cause in the ledger).</summary>
        public static bool PushClamps(Color c, Color neutral, float k)
        {
            float r = neutral.r + (c.r - neutral.r) * k, g = neutral.g + (c.g - neutral.g) * k, b = neutral.b + (c.b - neutral.b) * k;
            return r < 0f || r > 1f || g < 0f || g > 1f || b < 0f || b > 1f;
        }

        /// <summary>The lead's meter band (Tools/night/n73-hue.py): hue 0-40 or 345-360 degrees, saturation at least 0.35, value at least 0.20.</summary>
        public static bool InDanger(Color c)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            float deg = h * 360f;
            return (deg <= 40f || deg >= 345f) && s >= 0.35f && v >= 0.20f;
        }

        /// <summary>BossGrade54.ReadBoss's rim: the boss's rim colour divided by P1's filter after the push (linear, floored at `minFilter`), x the rim gain (alpha too, as the grade).</summary>
        public static Color RimHdr(Color rim, Color filter, float intensity, float minFilter, float gain)
        {
            Color f = Push(filter, Color.white, intensity).linear;
            return new Color(rim.r / Mathf.Max(minFilter, f.r), rim.g / Mathf.Max(minFilter, f.g), rim.b / Mathf.Max(minFilter, f.b), 1) * gain;
        }

        /// <summary>How many colours PushedInDanger looks at: the filter, the wash, the highlights, three shadows, grade55's split, the ambient, the key, the fog.</summary>
        public const int PushedColours = 10;

        /// <summary>The colours kiln73 writes (grade54's after the grade's push at `intensity`, grade55's and the lights' as they stand): how many sit in the danger band (want 0 of PushedColours).</summary>
        public static int PushedInDanger(float intensity)
        {
            int n = 0;
            Color flt = Push(C(UnityTuning.Kiln73Filter), Color.white, intensity), wash = Push(C(UnityTuning.Kiln73Wash), Color.white, intensity), hi = Push(C(UnityTuning.Kiln73Highlights), Color.grey, intensity);
            if (InDanger(flt)) n++;
            if (InDanger(wash)) n++;
            if (InDanger(hi)) n++;
            for (int i = 0; i < 3; i++) if (InDanger(Push(C(UnityTuning.Kiln73Shadows, i), Color.grey, intensity))) n++;
            if (InDanger(C(UnityTuning.Kiln73Split55))) n++;
            if (InDanger(C(UnityTuning.Kiln73Ambient))) n++;
            if (InDanger(C(UnityTuning.Kiln73Key))) n++;
            if (InDanger(C(UnityTuning.Kiln73Fog) * 4f)) n++; // the fog colour is dark: its hue is read at a readable value
            return n;
        }

        public static void Copy54(Grade54Phase d, Grade54Phase s)
        {
            d.filter = s.filter; d.shadows = s.shadows; d.highlights = s.highlights; d.fog = s.fog; d.ambient = s.ambient; d.key = s.key; d.wash = s.wash;
            d.balance = s.balance; d.sat = s.sat; d.contrast = s.contrast; d.ev = s.ev; d.fogMix = s.fogMix;
        }

        public static void Copy55(Grade55Phase d, Grade55Phase s)
        {
            d.split = s.split; d.turn = s.turn; d.fog = s.fog; d.ambient = s.ambient; d.key = s.key; d.lights = s.lights;
            d.balance = s.balance; d.ev = s.ev; d.contrast = s.contrast; d.fogMix = s.fogMix; d.lightsMix = s.lightsMix; d.sat = s.sat; d.turnBalance = s.turnBalance;
        }

        /// <summary>K1: grade54's phase `i` in kiln73's numbers (`ev` is left as the config has it: Bright71 zeroes it in the game).</summary>
        public static void Cool54(Grade54Phase p, int i)
        {
            p.filter = C(UnityTuning.Kiln73Filter);
            p.shadows = C(UnityTuning.Kiln73Shadows, i);
            p.highlights = C(UnityTuning.Kiln73Highlights);
            p.wash = C(UnityTuning.Kiln73Wash);
            p.fog = C(UnityTuning.Kiln73Fog);
            p.ambient = C(UnityTuning.Kiln73Ambient);
            p.key = C(UnityTuning.Kiln73Key);
            p.balance = 0f;
            p.sat = UnityTuning.Kiln73Sat;
            p.contrast = UnityTuning.Kiln73Contrast;
            p.fogMix = UnityTuning.Kiln73FogMix;
        }

        /// <summary>K1: grade55's phase in kiln73's numbers (`ev` and `turnBalance` as the config has them).</summary>
        public static void Cool55(Grade55Phase p)
        {
            p.split = C(UnityTuning.Kiln73Split55);
            p.turn = C(UnityTuning.Kiln73Turn55);
            p.fog = C(UnityTuning.Kiln73Fog);
            p.ambient = C(UnityTuning.Kiln73Ambient);
            p.key = C(UnityTuning.Kiln73Key);
            p.balance = UnityTuning.Kiln73Balance55;
            p.contrast = UnityTuning.Kiln73Contrast55;
            p.sat = UnityTuning.Kiln73Sat55;
            p.fogMix = UnityTuning.Kiln73FogMix;
        }
    }

    /// <summary>kiln73's live state: ticked once a frame by WyrmRoomView.Render.</summary>
    public static class Kiln73
    {
        public const string Row = "kiln73";

        /// <summary>The last [kiln73] line (the reels print it).</summary>
        public static string Line = "kiln73: not attached";
        /// <summary>The wyrm's grade54 / grade55 palettes are in kiln73's numbers now.</summary>
        public static bool Cool54 { get; private set; }
        public static bool Cool55 { get; private set; }
        public static int Flips;

        static Grade54Boss b54;
        static Grade55Boss b55;
        static readonly Grade54Phase[] orig54 = { new Grade54Phase(), new Grade54Phase(), new Grade54Phase() };
        static readonly Grade55Phase[] orig55 = { new Grade55Phase(), new Grade55Phase(), new Grade55Phase() };
        static float origTurnSat;
        static Color origRimHdr;
        static Image exposed, hurtImg, damageImg;
        static bool lastShown, lastExposed;
        static int edgeProbes;

        public static bool On(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);

        static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);

        static float Num(GameConfig cfg, string path, float missing)
        {
            if (!cfg.Has(path)) return missing;
            try { return (float)cfg.Num(path); }
            catch (System.Exception) { return missing; }
        }

        /// <summary>WyrmRoomView.Build's hook: one line per arena build (L-089), with the room mesh's colours and the row's state.</summary>
        public static void OnBuild(GameConfig cfg, string id, Color top, Color side, int boxes)
        {
            if (id != "wyrm") return;
            bool on = On(cfg);
            Color.RGBToHSV(top, out float th, out float ts, out _);
            Color.RGBToHSV(side, out float sh, out float ss, out _);
            Line = $"[kiln73] wyrm arena: row {(on ? "ON" : "off")}, surfaces 0 retinted (the room mesh's {boxes} boxes keep pillarTop #{Hex(top)} hue {th * 360f:0} S {ts:0.00} and pillarSide #{Hex(side)} hue {sh * 360f:0} S {ss:0.00}: cool; the level's k5 stone is palette72's), the EXPOSED edge is calm72's (kiln73 never touches it), " +
                   $"the cool grade numbers' pushed colours in the danger band: {Kiln73Math.PushedInDanger(Num(cfg, "design.n54.grade54.numbers.common.intensity", 1f))} of {Kiln73Math.PushedColours}";
            Debug.Log(Line);
        }

        /// <summary>Every render frame (WyrmRoomView.Render): the row read now; the wyrm's grade palettes in kiln73's numbers or the config's. The HUD's edges are not touched (calm72's).</summary>
        public static void Tick(SurvivalGame g, bool wyrmShown)
        {
            if (g == null || g.cfg == null) return;
            bool want = On(g.cfg);
            Palettes(g.cfg, want);
            if (wyrmShown) EdgeProbe(g);
            else lastExposed = false;
            if (wyrmShown != lastShown)
            {
                lastShown = wyrmShown;
                if (wyrmShown) Probe(g, want);
            }
        }

        static void Palettes(GameConfig cfg, bool want)
        {
            Grade54Boss n54 = BossGrade54.PaletteOf("wyrm");
            Grade55Boss n55 = BossGrade55.PaletteOf("wyrm");
            if (!ReferenceEquals(n54, b54)) { b54 = n54; Cool54 = false; } // a palette read again from the config: its numbers are the config's
            if (!ReferenceEquals(n55, b55)) { b55 = n55; Cool55 = false; }
            bool flipped = false;
            if (b54 != null && Cool54 != want)
            {
                if (want)
                {
                    for (int i = 0; i < 3; i++) Kiln73Math.Copy54(orig54[i], b54.p[i]);
                    origTurnSat = b54.turnSat;
                    origRimHdr = b54.rimHdr;
                    for (int i = 0; i < 3; i++) Kiln73Math.Cool54(b54.p[i], i);
                    b54.turnSat = UnityTuning.Kiln73TurnSat;
                    b54.rimHdr = Kiln73Math.RimHdr(b54.rim, b54.p[0].filter, Num(cfg, "design.n54.grade54.numbers.common.intensity", 1f), Mathf.Clamp(Num(cfg, "design.n54.grade54.numbers.common.rimMinFilter", 0.3f), 0.05f, 1f), b54.rimGain);
                }
                else
                {
                    for (int i = 0; i < 3; i++) Kiln73Math.Copy54(b54.p[i], orig54[i]);
                    b54.turnSat = origTurnSat;
                    b54.rimHdr = origRimHdr;
                }
                Cool54 = want;
                flipped = true;
            }
            if (b55 != null && Cool55 != want)
            {
                if (want)
                {
                    for (int i = 0; i < 3; i++) Kiln73Math.Copy55(orig55[i], b55.p[i]);
                    for (int i = 0; i < 3; i++) Kiln73Math.Cool55(b55.p[i]);
                }
                else for (int i = 0; i < 3; i++) Kiln73Math.Copy55(b55.p[i], orig55[i]);
                Cool55 = want;
                flipped = true;
            }
            if (!flipped) return;
            Flips++;
            float k = Num(cfg, "design.n54.grade54.numbers.common.intensity", 1f);
            Line = $"[kiln73] wyrm grades: row {(want ? "ON" : "off")} (flip {Flips}): grade54 {(b54 != null ? (Cool54 ? "cool" : "config") : "absent")}" +
                   (b54 != null ? $" (P1 shadows pushed #{Hex(Kiln73Math.Push(b54.p[0].shadows, Color.grey, k))}, filter pushed #{Hex(Kiln73Math.Push(b54.p[0].filter, Color.white, k))}, sat {b54.p[0].sat:0.#}, contrast {b54.p[0].contrast:0.#}, turnSat {b54.turnSat:0.#}, rimHdr ({b54.rimHdr.r:0.00}, {b54.rimHdr.g:0.00}, {b54.rimHdr.b:0.00}))" : "") +
                   $", grade55 {(b55 != null ? (Cool55 ? "cool" : "config") : "absent")}" +
                   (b55 != null ? $" (P1 split #{Hex(b55.p[0].split)}, balance {b55.p[0].balance:0.#}, ambient #{Hex(b55.p[0].ambient)}, key #{Hex(b55.p[0].key)})" : "") +
                   $", the cool numbers' pushed colours in the danger band {Kiln73Math.PushedInDanger(k)} of {Kiln73Math.PushedColours}";
            Debug.Log(Line);
        }

        static void Find()
        {
            foreach (Image im in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (exposed == null && im.name == "Exposed edge") exposed = im;
                else if (hurtImg == null && im.name == "Hurt vignette") hurtImg = im;
                else if (damageImg == null && im.name == "Damage vignette") damageImg = im;
            }
        }

        static string Alpha(Image im) => im == null ? "not found" : (im.enabled ? "" : "disabled, ") + "alpha " + im.color.a.ToString("0.00", CultureInfo.InvariantCulture);

        /// <summary>The edge speckle's source, read on both sides (L-014): each time the director's EXPOSED state changes while the arena is drawn, the three HUD edge Images' state
        /// (the exposed edge, the hurt flash, the damage vignette) and the player's health fraction, so the plain side's line names which overlay the border is.</summary>
        static void EdgeProbe(SurvivalGame g)
        {
            bool ex = g.director != null && g.director.Exposed;
            if (ex == lastExposed) return;
            lastExposed = ex;
            if (edgeProbes >= UnityTuning.Kiln73EdgeProbeMax) return;
            edgeProbes++;
            if (exposed == null || hurtImg == null || damageImg == null) Find();
            Debug.Log($"[kiln73] edge probe: director exposed {(ex ? "yes" : "no")} at {Time.unscaledTime:0.0} s, the HUD's exposed edge {Alpha(exposed)}, hurt flash {Alpha(hurtImg)}, damage vignette {Alpha(damageImg)}, calm72 {(Calm72.Active ? "ON" : "off")} (read only: kiln73 touches none of the three)");
        }

        static void Probe(SurvivalGame g, bool want)
        {
            Light sun = RenderSettings.sun;
            var sb = new StringBuilder("[kiln73] wyrm arena shown: row ").Append(want ? "ON" : "off");
            sb.Append(", grade54 live ").Append(BossGrade54.Live ? "yes" : "no").Append(" w ").Append(BossGrade54.Weight.ToString("0.00", CultureInfo.InvariantCulture));
            sb.Append(", grade55 live ").Append(BossGrade55.Live ? "yes" : "no").Append(" w ").Append(BossGrade55.Weight.ToString("0.00", CultureInfo.InvariantCulture));
            sb.Append(", palettes cool ").Append(Cool54 ? "54" : "-").Append(Cool55 ? "+55" : "");
            sb.Append(", sun #").Append(sun != null ? Hex(sun.color) : "none").Append(", ambient equator #").Append(Hex(RenderSettings.ambientEquatorColor)).Append(", fog #").Append(Hex(RenderSettings.fogColor));
            sb.Append(", lit72 ").Append(Lit72.Live ? "live" : "off").Append(" w ").Append(Lit72.Weight.ToString("0.00", CultureInfo.InvariantCulture));
            sb.Append(", exposed edge ").Append(exposed == null ? "not found yet" : exposed.enabled ? "drawn (calm72's: kiln73 never touches it)" : "disabled (not by kiln73)");
            sb.Append(", director exposed ").Append(g.director != null && g.director.Exposed ? "yes" : "no");
            Line = sb.ToString();
            Debug.Log(Line);
        }
    }
}
