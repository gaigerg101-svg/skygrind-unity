// NIGHT9 U3-04: determinism. Skygrind.Core never calls the C runtime's transcendental functions (System.Math's
// Atan2, Exp, Tan, Tanh, Acos, Asin, Atan, Pow, Log, Sin, Cos, ... differ by OS and by .NET runtime: Mono's Math.Pow
// misrounded 25 of the jsMath inputs where CoreCLR misrounded 1); JsMath has V8's versions (fdlibm, checked by
// vectors/jsMath.json). Exact IEEE operations (Sqrt, Floor, Ceiling, Abs) are fine. One allowance, by name: Settings.cs
// uses Log10 to count a slider step's decimals (UI text, not the sim).
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class DeterminismTests
    {
        static readonly Regex Transcendental = new Regex(
            @"(?<![A-Za-z0-9_.])(?:System\.)?Math\.(Atan2|Atan|Acos|Asin|Tan|Tanh|Sinh|Cosh|Exp|Pow|Log|Log2|Log10|Cbrt|Sin|Cos|SinCos|ScaleB|FusedMultiplyAdd)\s*\(");

        static readonly HashSet<string> Allowed = new HashSet<string> { "Settings.cs:Log10" };

        [Test, Description("no System.Math transcendental in Skygrind.Core outside JsMath*.cs (V8's versions live in JsMath)")]
        public void Core_CallsNoSystemMathTranscendental()
        {
            string core = Path.GetFullPath(Path.Combine(Application.dataPath, "Scripts", "Core"));
            var found = new List<string>();
            int files = 0;
            foreach (string path in Directory.GetFiles(core, "*.cs", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(path);
                if (Regex.IsMatch(name, @"^JsMath(\..+)?\.cs$")) continue;
                files++;
                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    string code = lines[i];
                    int comment = code.IndexOf("//", System.StringComparison.Ordinal);
                    if (comment >= 0) code = code.Substring(0, comment);
                    foreach (Match m in Transcendental.Matches(code))
                    {
                        if (Allowed.Contains($"{name}:{m.Groups[1].Value}")) continue;
                        found.Add($"{name}:{i + 1}: {lines[i].Trim()}");
                    }
                }
            }
            TestContext.WriteLine($"{files} Core files scanned; {found.Count} System.Math transcendental calls");
            Assert.Greater(files, 40);
            Assert.IsEmpty(found, string.Join("\n", found));
        }
    }
}
