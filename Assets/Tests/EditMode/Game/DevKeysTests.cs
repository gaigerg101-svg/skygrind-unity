// NIGHT13 U3-26.12 (S13, D-15): the debug keys (F3 overlay, F8 every gate, F9 the next round) are a developer's: on in the
// Editor, in a Player only with -skygrindDev on the command line, off by default.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class DevKeysTests
    {
        [Test]
        public void EditorAllowsWithoutTheFlag()
        {
            Assert.IsTrue(DevKeys.Allowed(new[] { "Unity.exe", "-projectPath", "x" }, true));
            Assert.IsTrue(DevKeys.Allowed(new string[0], true));
            Assert.IsTrue(DevKeys.Allowed(null, true));
        }

        [Test]
        public void PlayerAllowsWithTheFlag()
        {
            Assert.IsTrue(DevKeys.Allowed(new[] { "SKYGRIND.exe", "-skygrindDev" }, false));
            Assert.IsTrue(DevKeys.Allowed(new[] { "SKYGRIND.exe", "-logFile", "a.log", "-skygrindDev" }, false));
            Assert.IsTrue(DevKeys.Allowed(new[] { "SKYGRIND.exe", "-SKYGRINDDEV" }, false), "the flag ignores case (typed by hand into a shortcut)");
        }

        [Test]
        public void PlayerDeniesByDefault()
        {
            Assert.IsFalse(DevKeys.Allowed(new[] { "SKYGRIND.exe" }, false));
            Assert.IsFalse(DevKeys.Allowed(new string[0], false));
            Assert.IsFalse(DevKeys.Allowed(null, false));
            Assert.IsFalse(DevKeys.Allowed(new[] { "SKYGRIND.exe", "-skygrindSmoke", "-logFile", "smoke.log" }, false), "another flag is not the dev flag");
            Assert.IsFalse(DevKeys.Allowed(new[] { "SKYGRIND.exe", "-skygrindDevX", "skygrindDev", "--skygrindDev" }, false), "only the exact flag");
            Assert.IsFalse(DevKeys.Allowed(new[] { "SKYGRIND.exe", null }, false), "a null argument is skipped");
        }
    }
}
