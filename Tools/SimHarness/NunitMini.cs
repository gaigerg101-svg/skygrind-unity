// NIGHT21 (builder W): a tiny stand-in for the slice of NUnit that Assets/Tests/EditMode/Endless21Tests.cs uses, so `grind --selftest`
// runs those very assertions outside Unity (the Editor was unavailable). Unity compiles the real NUnit; this is the harness's only.
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestAttribute : Attribute { }

    public sealed class AssertionException : Exception
    {
        public AssertionException(string m) : base(m) { }
    }

    public abstract class Constraint
    {
        public abstract bool Matches(object actual);
        public abstract string Describe();
    }

    sealed class EqualConstraint : Constraint
    {
        readonly object expected;
        readonly bool negate;
        public EqualConstraint(object expected, bool negate) { this.expected = expected; this.negate = negate; }
        public override bool Matches(object actual) => Eq(actual, expected) != negate;
        public override string Describe() => (negate ? "not " : "") + Show(expected);

        static string Show(object o) => o is IEnumerable e && !(o is string) ? "[" + string.Join(", ", e.Cast<object>().Select(Show)) + "]" : o == null ? "null" : o.ToString();
        public static string ShowValue(object o) => Show(o);

        static bool Eq(object a, object b)
        {
            if (a == null || b == null) return a == b;
            if (a is string || b is string) return a.Equals(b);
            if (a is IEnumerable ea && b is IEnumerable eb)
            {
                var la = ea.Cast<object>().ToList();
                var lb = eb.Cast<object>().ToList();
                return la.Count == lb.Count && la.Zip(lb, Eq).All(x => x);
            }
            return a.Equals(b);
        }
    }

    sealed class BoolConstraint : Constraint
    {
        readonly bool want;
        public BoolConstraint(bool want) { this.want = want; }
        public override bool Matches(object actual) => actual is bool b && b == want;
        public override string Describe() => want.ToString();
    }

    sealed class NullConstraint : Constraint
    {
        public override bool Matches(object actual) => actual == null;
        public override string Describe() => "null";
    }

    public static class Is
    {
        public static Constraint EqualTo(object expected) => new EqualConstraint(expected, false);
        public static Constraint True => new BoolConstraint(true);
        public static Constraint False => new BoolConstraint(false);
        public static Constraint Null => new NullConstraint();
        public static NotIs Not => new NotIs();
    }

    public sealed class NotIs
    {
        public Constraint EqualTo(object expected) => new EqualConstraint(expected, true);
    }

    public static class Assert
    {
        public static void That(object actual, Constraint c, string message = null)
        {
            if (!c.Matches(actual)) throw new AssertionException($"expected {c.Describe()} but was {EqualConstraintShow(actual)}" + (message != null ? " (" + message + ")" : ""));
        }

        static string EqualConstraintShow(object o) => o == null ? "null" : o.ToString();
        static string Shown(object o) => o is IEnumerable e && !(o is string) ? "[" + string.Join(", ", e.Cast<object>().Select(Shown)) + "]" : o?.ToString();
    }

    // Runs every [Test] method of a class on a fresh instance (the harness's selftest).
    public static class MiniRunner
    {
        public static int Run(Type type, Action<string> log)
        {
            int failed = 0, total = 0;
            foreach (MethodInfo m in type.GetMethods().Where(x => x.GetCustomAttribute<TestAttribute>() != null))
            {
                total++;
                try
                {
                    m.Invoke(Activator.CreateInstance(type), null);
                    log($"  PASS {m.Name}");
                }
                catch (TargetInvocationException ex)
                {
                    failed++;
                    log($"  FAIL {m.Name}: {ex.InnerException?.Message}");
                }
            }
            log($"{type.Name}: {total - failed} of {total} pass");
            return failed;
        }
    }
}

namespace Skygrind.Core.Tests
{
    /// <summary>The harness's Port.Root (Unity's is in GoldenVectorTests.cs).</summary>
    public static class Port
    {
        public static string Root => System.IO.Path.GetFullPath("PortExport");
    }
}
