// The checker for vectors/jsMath.json (NIGHT9 U3-04): every recorded V8 call of Math.atan2, exp, tan, acos, asin,
// round, sin, cos, hypot (2 and 3 arguments), min, max,
// atan, tanh and pow (port-export-p18/scripts/jsMath.mjs, Node 24.19.0 / V8 13.6) replayed on JsMath (JsMath.cs,
// JsMath.Fdlibm.cs) and compared bit for bit.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddJsMath(Dictionary<string, GoldenCheck> d)
        {
            d["jsMath"] = CheckJsMath;
        }

        static void CheckJsMath(GoldenData data, GoldenResult r)
        {
            var cases = data.Cases("jsMath");
            for (int i = 0; i < cases.Count; i++)
            {
                var c = cases[i];
                string fn = (string)c["fn"];
                var args = PortData.Arr(c["args"]);
                double a = PortData.Num(args[0]);
                double b = args.Count > 1 ? PortData.Num(args[1]) : double.NaN;
                r.where = $"{fn}#{i}";
                double got;
                switch (fn)
                {
                    case "atan2": got = JsMath.Atan2(a, b); break;
                    case "exp": got = JsMath.Exp(a); break;
                    case "tan": got = JsMath.Tan(a); break;
                    case "acos": got = JsMath.Acos(a); break;
                    case "asin": got = JsMath.Asin(a); break;
                    case "atan": got = JsMath.Atan(a); break;
                    case "tanh": got = JsMath.Tanh(a); break;
                    case "pow": got = JsMath.Pow(a, b); break;
                    case "round": got = JsMath.Round(a); break;
                    case "sin": got = JsMath.Sin(a); break;
                    case "cos": got = JsMath.Cos(a); break;
                    case "hypot": got = args.Count > 2 ? JsMath.Hypot(a, b, PortData.Num(args[2])) : JsMath.Hypot(a, b); break;
                    case "min": got = JsMath.Min(a, b); break;
                    case "max": got = JsMath.Max(a, b); break;
                    case "log": got = JsMath.Log(a); break;
                    default:
                        r.Fail($"unknown function {fn}");
                        continue;
                }
                string call = args.Count > 1
                    ? $"{fn}({GoldenResult.Fmt(a)}, {GoldenResult.Fmt(b)})"
                    : $"{fn}({GoldenResult.Fmt(a)})";
                r.Same(c["result"], got, call);
                r.cases++;
            }
            r.where = "";
        }
    }
}
