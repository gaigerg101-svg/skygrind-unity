// The vitest matchers the ported tests need, with vitest's exact semantics, so a C# pass means the same as a JS pass.
using System;
using System.Globalization;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    static class Expect
    {
        // toBeCloseTo(expected, numDigits = 2): passes when |expected - received| < 10^-numDigits / 2.
        public static void CloseTo(double received, double expected, int numDigits = 2)
        {
            double tolerance = Math.Pow(10, -numDigits) / 2;
            if (!(Math.Abs(expected - received) < tolerance))
            {
                Assert.Fail(string.Format(CultureInfo.InvariantCulture,
                    "expected {0:R} to be close to {1:R} (|diff| < {2:R})", received, expected, tolerance));
            }
        }

        // toBe / toEqual on numbers: exact.
        public static void Exactly(double received, double expected) =>
            Assert.That(received, Is.EqualTo(expected), string.Format(CultureInfo.InvariantCulture,
                "expected {0:R} to be exactly {1:R}", received, expected));
    }
}
