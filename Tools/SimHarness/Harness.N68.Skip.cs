// NIGHT68 builder T, slice I4 (Logs/n68/slate.json I4; the ROOM list's "M2 VERB" line): skip v2's STYLE rule and its planted cases, and the
// room file's sha256 for the GATE's fresh pre-run (`n65verb skip ... --room-file <file>`). Skip v2 itself is Harness.N65.Verb.cs V66SkipV2.
// NIGHT69 builder T, slice Y1 (Logs/n69/slate.json Y1): the STYLE rule is now verb-aware >= the faster refusal's - 5 (NaN: UNSURE); NIGHT68's
// rule printed beside it. No ratio, bar or exercised count reads it.
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

public static partial class Harness
{
    /// <summary>The ROOM list's STYLE need (NIGHT69 Y1, FORGE69): STYLE above DULL on the verb-aware run >= the faster refusal's - 5 points: the
    /// verb must not make the run duller. NIGHT68's min(refusal + 15, max(refusal, 95)) asked 91-94 % on levels 3 / 5 where the verb runs reach
    /// 79-82 % (unreachable; scout.md X6); it stays printed beside the rule (V68StyleNeedN68), never the verdict. NaN in, NaN out.</summary>
    static double V68StyleNeed(double refusal) => double.IsNaN(refusal) ? double.NaN : refusal - 5;

    /// <summary>sha256 (lowercase hex) of a file's text as the GATE pack hashes it (n68-gate-pack.cjs sha256n): the bytes decoded as UTF-8 (a
    /// BOM kept, as node's readFileSync(.., "utf8") keeps it), every CR LF made LF, the text encoded as UTF-8.</summary>
    static string V68Sha256N(byte[] bytes)
    {
        string text = new UTF8Encoding(false, false).GetString(bytes).Replace("\r\n", "\n");
        using (var h = SHA256.Create())
        {
            byte[] d = h.ComputeHash(new UTF8Encoding(false).GetBytes(text));
            var sb = new StringBuilder(64);
            foreach (byte b in d) sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            return sb.ToString();
        }
    }

    /// <summary>NIGHT68's STYLE need, min(refusal + 15, max(refusal, 95)): printed beside the rule, never the verdict. NaN in, NaN out.</summary>
    static double V68StyleNeedN68(double refusal) => double.IsNaN(refusal) ? double.NaN : Math.Min(refusal + 15, Math.Max(refusal, 95));

    /// <summary>The STYLE verdict of the M2 VERB line (verb-aware STYLE above DULL against the faster refusal's, pooled %): UNSURE when either is
    /// NaN (no fight on the exercised seeds: nothing to judge), PASS at verb-aware >= refusal - 5, else FAIL.</summary>
    static string V69StyleVerdict(double verbAware, double refusal) => double.IsNaN(verbAware) || double.IsNaN(refusal) ? "UNSURE" : verbAware >= V68StyleNeed(refusal) ? "PASS" : "FAIL";

    /// <summary>`n65verb skip --style-cases` (NIGHT69 Y1, Logs/n69/slate.json Y1): the ROOM list's STYLE rule on planted numbers (verb-aware
    /// %, the faster refusal %) through the M2 VERB line's own verdict function, NIGHT68's rule beside each.</summary>
    static int V68StyleCases()
    {
        var ci = CultureInfo.InvariantCulture;
        string P(double v) => double.IsNaN(v) ? "NaN" : v.ToString("0.0", ci);
        Console.WriteLine("n65verb skip --style-cases: the ROOM list's STYLE rule (NIGHT69 Y1), STYLE above DULL on the verb-aware run >= the faster refusal's - 5 points (NaN: UNSURE); NIGHT68's rule (>= min(refusal + 15, max(refusal, 95))) beside each");
        // (verb-aware, refusal, why the case, the ROOM list's expected verdict)
        var cases = new (double a, double r, string why, string expect)[]
        {
            (69.0, 74.0, "the slate's line: exactly refusal - 5", "PASS"),
            (68.9, 74.0, "the slate's line: 0.1 under refusal - 5", "FAIL"),
            (double.NaN, 74.0, "the slate's line: no verb-aware STYLE (no fight on an exercised seed)", "UNSURE"),
            (69.0, double.NaN, "no refusal STYLE", "UNSURE"),
            (80.0, 85.0, "a level 3 / 5 refusal: exactly refusal - 5", "PASS"),
            (79.9, 85.0, "a level 3 / 5 refusal: 0.1 under", "FAIL"),
            (84.0, 70.0, "NIGHT68's refusal + 15 branch FAILed it (need 85)", "PASS"),
            (86.3, 79.8, "r3 kb3's 2-seed skip (Logs/n68/I2): NIGHT68 need 94.8", "PASS"),
            (89.0, 88.6, "v6's 88.6 % refusal (levels-data.md item 2): NIGHT68 need 95", "PASS"),
        };
        int ok = 0;
        foreach (var c in cases)
        {
            double need = V68StyleNeed(c.r), old = V68StyleNeedN68(c.r);
            string v = V69StyleVerdict(c.a, c.r);
            string oldV = double.IsNaN(c.a) || double.IsNaN(old) ? "FAIL (NaN)" : c.a >= old ? "PASS" : "FAIL";
            if (v == c.expect) ok++;
            Console.WriteLine($"  verb-aware {P(c.a)} % vs refusal {P(c.r)} % ({c.why}): need {P(need)} % -> {v} (expected {c.expect}: {(v == c.expect ? "OK" : "WRONG")}) | NIGHT68's rule: need {P(old)} % -> {oldV}");
        }
        Console.WriteLine($"style-cases: {ok} / {cases.Length} as the ROOM list's rule expects{(ok == cases.Length ? " (GREEN)" : " (RED)")}");
        return ok == cases.Length ? 0 : 1;
    }
}
