// NIGHT13 U3-26.3 (the shared types for save-and-quit, U3-26.10): IRunState, the interface every stateful run system
// implements for the run snapshot, and its writer / reader: one JSON object per system, written in a fixed order, read
// back by key. Doubles round-trip exactly: printed as JS String(number) does (JsNumber: V8's shortest round-trip),
// checked by parsing it back (JsJson.ParseNumber), and written as their 64 bits in hex ("0x...") in the rare case the
// text would not round-trip. A missing key or a value of the wrong type throws StateException: a corrupt snapshot fails
// loudly, never loads half a run. No per-step use: written on save, read on load.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Skygrind.Core
{
    /// <summary>A system whose run state goes in the run snapshot (save-and-quit).</summary>
    public interface IRunState
    {
        void Save(StateWriter w);
        void Load(StateReader r);
    }

    public sealed class StateException : Exception
    {
        public StateException(string message) : base(message) { }
    }

    public sealed class StateWriter
    {
        readonly StringBuilder sb = new StringBuilder(4096);
        readonly Stack<bool> first = new Stack<bool>();

        public StateWriter()
        {
            sb.Append('{');
            first.Push(true);
        }

        void Key(string key)
        {
            if (!first.Pop()) sb.Append(',');
            first.Push(false);
            Str(key);
            sb.Append(':');
        }

        void Str(string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                else sb.Append(c);
            }
            sb.Append('"');
        }

        void Num(double v)
        {
            if (!double.IsNaN(v) && !double.IsInfinity(v))
            {
                string t = JsNumber.ToString(v);
                if (BitConverter.DoubleToInt64Bits(JsJson.ParseNumber(t)) == BitConverter.DoubleToInt64Bits(v))
                {
                    sb.Append(t);
                    return;
                }
            }
            Str("0x" + BitConverter.DoubleToInt64Bits(v).ToString("x16", CultureInfo.InvariantCulture));
        }

        /// <summary>Open a nested object (a system's section).</summary>
        public void Begin(string key)
        {
            Key(key);
            sb.Append('{');
            first.Push(true);
        }

        public void End()
        {
            first.Pop();
            sb.Append('}');
        }

        public void D(string key, double v)
        {
            Key(key);
            Num(v);
        }

        public void I(string key, long v)
        {
            Key(key);
            sb.Append(v.ToString(CultureInfo.InvariantCulture));
        }

        public void B(string key, bool v)
        {
            Key(key);
            sb.Append(v ? "true" : "false");
        }

        public void S(string key, string v)
        {
            Key(key);
            if (v == null) sb.Append("null");
            else Str(v);
        }

        public void Doubles(string key, IList<double> v)
        {
            Key(key);
            sb.Append('[');
            for (int i = 0; i < v.Count; i++)
            {
                if (i > 0) sb.Append(',');
                Num(v[i]);
            }
            sb.Append(']');
        }

        public void Ints(string key, IList<int> v)
        {
            Key(key);
            sb.Append('[');
            for (int i = 0; i < v.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(v[i].ToString(CultureInfo.InvariantCulture));
            }
            sb.Append(']');
        }

        public void Bools(string key, IList<bool> v)
        {
            Key(key);
            sb.Append('[');
            for (int i = 0; i < v.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(v[i] ? "true" : "false");
            }
            sb.Append(']');
        }

        public void Strings(string key, IList<string> v)
        {
            Key(key);
            sb.Append('[');
            for (int i = 0; i < v.Count; i++)
            {
                if (i > 0) sb.Append(',');
                if (v[i] == null) sb.Append("null");
                else Str(v[i]);
            }
            sb.Append(']');
        }

        /// <summary>The snapshot's text (call once, after the last End of a section).</summary>
        public string ToJson()
        {
            if (first.Count != 1) throw new StateException("StateWriter: a section was left open");
            return sb.ToString() + "}";
        }
    }

    public sealed class StateReader
    {
        readonly Dictionary<string, object> o;
        readonly string path;

        public StateReader(string json)
        {
            object v;
            try
            {
                v = JsJson.Parse(json);
            }
            catch (Exception e)
            {
                throw new StateException("snapshot: not JSON: " + e.Message);
            }
            o = v as Dictionary<string, object> ?? throw new StateException("snapshot: not an object");
            path = "";
        }

        StateReader(Dictionary<string, object> o, string path)
        {
            this.o = o;
            this.path = path;
        }

        public bool Has(string key) => o.ContainsKey(key);

        object Get(string key)
        {
            if (!o.TryGetValue(key, out object v)) throw new StateException($"snapshot: missing {path}{key}");
            return v;
        }

        StateException Wrong(string key, string type) => new StateException($"snapshot: {path}{key} is not {type}");

        static bool TryNum(object v, out double d)
        {
            if (v is double x)
            {
                d = x;
                return true;
            }
            if (v is string s && s.StartsWith("0x", StringComparison.Ordinal) && s.Length == 18 && long.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long bits))
            {
                d = BitConverter.Int64BitsToDouble(bits);
                return true;
            }
            d = 0;
            return false;
        }

        public StateReader Section(string key) => Get(key) is Dictionary<string, object> d ? new StateReader(d, path + key + ".") : throw Wrong(key, "an object");

        public double D(string key) => TryNum(Get(key), out double d) ? d : throw Wrong(key, "a number");

        public long I(string key)
        {
            double d = D(key);
            if (d != Math.Floor(d) || Math.Abs(d) > 9007199254740992.0) throw Wrong(key, "an integer");
            return (long)d;
        }

        public int Int(string key)
        {
            long v = I(key);
            if (v < int.MinValue || v > int.MaxValue) throw Wrong(key, "an int");
            return (int)v;
        }

        public uint U(string key)
        {
            long v = I(key);
            if (v < 0 || v > uint.MaxValue) throw Wrong(key, "a uint");
            return (uint)v;
        }

        public bool B(string key) => Get(key) is bool b ? b : throw Wrong(key, "a boolean");

        public string S(string key)
        {
            object v = Get(key);
            if (v == null) return null;
            return v as string ?? throw Wrong(key, "a string");
        }

        List<object> Arr(string key) => Get(key) as List<object> ?? throw Wrong(key, "an array");

        public List<double> Doubles(string key)
        {
            List<object> a = Arr(key);
            var r = new List<double>(a.Count);
            foreach (object v in a) r.Add(TryNum(v, out double d) ? d : throw Wrong(key, "an array of numbers"));
            return r;
        }

        public List<int> Ints(string key)
        {
            List<object> a = Arr(key);
            var r = new List<int>(a.Count);
            foreach (object v in a)
            {
                if (!(v is double d) || d != Math.Floor(d) || d < int.MinValue || d > int.MaxValue) throw Wrong(key, "an array of ints");
                r.Add((int)d);
            }
            return r;
        }

        public List<bool> Bools(string key)
        {
            List<object> a = Arr(key);
            var r = new List<bool>(a.Count);
            foreach (object v in a) r.Add(v is bool b ? b : throw Wrong(key, "an array of booleans"));
            return r;
        }

        public List<string> Strings(string key)
        {
            List<object> a = Arr(key);
            var r = new List<string>(a.Count);
            foreach (object v in a) r.Add(v == null ? null : v as string ?? throw Wrong(key, "an array of strings"));
            return r;
        }
    }
}
