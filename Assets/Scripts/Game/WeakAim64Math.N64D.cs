// NIGHT64 builder D (slate B3, THE KILN WYRM's weak point in the player's cyan; NIGHT63's view row design.n63.bossColours63, OFF): the weak
// aim's pure parts (EditMode: N64WeakAimTests). The red (n63-b11's same-run b3k stagger pair, `n63-b3-roles.py --hue 795,376,1121,684`): the
// OPEN head's median hue 158.5, RGB (8, 238, 157). NIGHT63's RimAt draws the grade's rim pre-divided by the boss's colour filter, at the part's
// own HDR brightness (#0854FF x6.8 on the head, x3.6 on its shell); under THE WYRM's grade the tonemap clips that green before the filter cuts
// blue to about 0.33 (linear), so green stays over blue. Code-scout option 1: the weak point's OWN colour in the bossColours63 table,
// design.n63.bossColours63.numbers.weakAim.<boss id> = { aim: [r, g, b] (sRGB, brightest channel 1), scale }: in that boss's graded fight a
// weak part's routed cyan is drawn as aim x (its own brightest channel x scale), under the clip. Tuned in a Player (the slate's tuning loop).
// BossColours63.N63D.cs applies it (PartRim, ShellRim) under RimAt's own gate (BossGrade54.RimBoss); a boss with no entry keeps RimAt.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class WeakAim64Math
    {
        const string P = "design.n63.bossColours63.numbers.weakAim";

        /// <summary>One boss's aim: the colour (sRGB, brightest channel 1) and the share of the part's own brightness it is drawn at.</summary>
        public sealed class Aim
        {
            public double r, g, b, scale;
            Dictionary<string, Aim> parts;

            /// <summary>NIGHT64 B3 round 2 (Logs/n64/verdict-B3.md: one aim read ~190 on the bright cube and ~222 on the dim translucent box):
            /// the part's own aim (weakAim.&lt;boss&gt;.parts.&lt;part&gt;), else this boss's. Allocation-free.</summary>
            public Aim For(string part) => part != null && parts != null && parts.TryGetValue(part, out Aim a) ? a : this;

            /// <summary>A part's own aim (From's, and a test's).</summary>
            public void SetPart(string part, Aim a)
            {
                if (part == null || a == null) return;
                parts ??= new Dictionary<string, Aim>(StringComparer.Ordinal);
                parts[part] = a;
            }

            /// <summary>The parts with their own aim.</summary>
            public IEnumerable<string> PartNames => parts != null ? (IEnumerable<string>)parts.Keys : new string[0];
        }

        /// <summary>One aim from its config entry ({aim: [r, g, b], scale}); null when malformed (a brightest channel not in (0, 1], a negative
        /// channel, a scale not positive).</summary>
        static Aim Read(Dictionary<string, object> e)
        {
            if (!e.TryGetValue("aim", out object ao) || !e.TryGetValue("scale", out object so)) return null;
            List<object> a = PortData.Arr(ao);
            if (a.Count != 3) return null;
            var aim = new Aim { r = PortData.Num(a[0]), g = PortData.Num(a[1]), b = PortData.Num(a[2]), scale = PortData.Num(so) };
            double m = Math.Max(aim.r, Math.Max(aim.g, aim.b));
            if (!(m > 0 && m <= 1) || aim.r < 0 || aim.g < 0 || aim.b < 0 || !(aim.scale > 0)) return null;
            return aim;
        }

        static bool Key(string k) => !k.StartsWith("_", StringComparison.Ordinal) && !k.EndsWith("_why", StringComparison.Ordinal);

        /// <summary>Boss id -> aim, from the row's numbers (an entry with a malformed aim, a brightest channel not in (0, 1], or a scale not
        /// positive is left out); null when the table is missing.</summary>
        public static Dictionary<string, Aim> From(GameConfig cfg)
        {
            if (cfg == null || !cfg.Has(P) || !(cfg.Get(P) is Dictionary<string, object> d)) return null;
            var o = new Dictionary<string, Aim>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, object> kv in d)
            {
                if (!Key(kv.Key) || !(kv.Value is Dictionary<string, object> e)) continue;
                try
                {
                    Aim aim = Read(e);
                    if (aim == null) continue;
                    if (e.TryGetValue("parts", out object po) && po is Dictionary<string, object> ps) // NIGHT64 B3 round 2: a part's own aim
                        foreach (KeyValuePair<string, object> pk in ps)
                            if (Key(pk.Key) && pk.Value is Dictionary<string, object> pe) aim.SetPart(pk.Key, Read(pe));
                    o[kv.Key] = aim;
                }
                catch (Exception) { }
            }
            return o;
        }

        /// <summary>RimAt's own input gate (BossGrade54.RimAt): the player's cyan, saturated (S >= 0.3) with its hue in 160-215.</summary>
        public static bool IsCyan(double r, double g, double b)
        {
            Colours63Math.Hsv(r, g, b, out double h, out double s, out _);
            return s >= 0.3 && h >= 160 && h <= 215;
        }

        /// <summary>A weak part's routed colour through the aim: aim x (its brightest channel x scale). False (and the colour unchanged) with no
        /// aim or an input that is not the player's cyan.</summary>
        public static bool At(Aim a, double r, double g, double b, out double R, out double G, out double B)
        {
            R = r; G = g; B = b;
            if (a == null || !IsCyan(r, g, b)) return false;
            double k = Math.Max(r, Math.Max(g, b)) * a.scale;
            R = a.r * k;
            G = a.g * k;
            B = a.b * k;
            return true;
        }

        /// <summary>The proof line printed once a config (L-089): each boss's aim.</summary>
        public static string Line(Dictionary<string, Aim> aims)
        {
            if (aims == null) return "[bossColours63] weak aims: none (design.n63.bossColours63.numbers.weakAim missing): RimAt for every boss";
            var sb = new System.Text.StringBuilder("[bossColours63] weak aims:");
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;
            foreach (KeyValuePair<string, Aim> kv in aims)
            {
                sb.Append(' ').Append(kv.Key).Append(" aim ").Append(BossColours63Math.HexOf(kv.Value.r, kv.Value.g, kv.Value.b)).Append(" scale ").Append(kv.Value.scale.ToString("0.00", ci));
                foreach (string p in kv.Value.PartNames)
                {
                    Aim pa = kv.Value.For(p);
                    sb.Append(" (").Append(p).Append(" aim ").Append(BossColours63Math.HexOf(pa.r, pa.g, pa.b)).Append(" scale ").Append(pa.scale.ToString("0.00", ci)).Append(')');
                }
                sb.Append(';');
            }
            return sb.Append(" every other boss: RimAt").ToString();
        }
    }
}
