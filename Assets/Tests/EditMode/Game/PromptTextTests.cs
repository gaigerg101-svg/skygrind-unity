// NIGHT19 U3-33.4 (builder P; the shared commit placed the empty fixture): PromptText.For, the one API every prompt calls.
// With design.pad.n19 live and the pad used last: every action the v1 and v2 layouts bind is a drawn sprite in the Xbox,
// PlayStation and Switch sets (an unknown pad draws Xbox's), its HOLD the ringed glyph; remaps followed; the WHEEL prompt
// ("radial") is the button that holds the wheel open; a keyboard-only action ("restart") reads as nothing, never a key;
// the hotbar's cycle line in glyphs; the words when the sprites aren't built. The keyboard: each site's own text, except the
// HOLD buy's live key. The block off: NIGHT18's text. Answers are cached (the same string back: no allocation per call).
// PlayerDevices is set by hand (ReaderInput publishes it in the game) and restored after each test; PromptGlyphs is never
// built here (ForceSprites stands in for it): no Unity runtime is needed.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class PromptTextTests
    {
        static string Root()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (System.Exception) { return Path.GetFullPath("."); } // a plain .NET runner: the working folder is the project
        }

        InputDevice last;
        GlyphSet glyphs;
        PadLayout layout;
        bool n19;
        Bindings bindings;

        [SetUp]
        public void Save()
        {
            last = PlayerDevices.Last;
            glyphs = PlayerDevices.Glyphs;
            layout = PlayerDevices.Layout;
            n19 = PlayerDevices.N19;
            bindings = PlayerDevices.Bindings;
        }

        [TearDown]
        public void Restore()
        {
            PlayerDevices.Last = last;
            PlayerDevices.Glyphs = glyphs;
            PlayerDevices.Layout = layout;
            PlayerDevices.N19 = n19;
            PlayerDevices.Bindings = bindings;
            PromptText.ForceSprites = false;
        }

        static void Pad(GlyphSet set, PadLayout l = null, bool sprites = true)
        {
            PlayerDevices.N19 = true;
            PlayerDevices.Last = InputDevice.Pad;
            PlayerDevices.Glyphs = set;
            PlayerDevices.Layout = l ?? GamepadMath.DefaultLayoutV2;
            PromptText.ForceSprites = sprites;
        }

        static string Tag(string name) => "<sprite=\"" + UnityTuning.PromptSpriteAsset + "\" name=\"" + name + "\">";

        static readonly GlyphSet[] Sets = { GlyphSet.Xbox, GlyphSet.PlayStation, GlyphSet.Nintendo, GlyphSet.Neutral };

        [Test]
        public void EveryBoundAction_IsADrawnSprite_InAllThreeSets_PlainAndHold()
        {
            foreach (PadLayout l in new[] { GamepadMath.DefaultLayoutV2, GamepadMath.DefaultLayout })
            {
                foreach (GlyphSet set in Sets)
                {
                    Pad(set, l);
                    int s = set == GlyphSet.PlayStation ? 1 : set == GlyphSet.Nintendo ? 2 : 0;
                    foreach (PadAction a in GamepadMath.LabelOrderV2)
                    {
                        PadButtonName? b = GamepadMath.ButtonFor(a, l);
                        if (!b.HasValue) continue; // SPRINT on the v1 layout
                        string name = GamepadMath.ActionNames[(int)a];
                        Assert.That(PromptText.For(name, false, "E"), Is.EqualTo(Tag(PromptText.SpriteName(s, (int)b.Value, false))), $"{set} {name}");
                        Assert.That(PromptText.For(name, true, "HOLD E"), Is.EqualTo(Tag(PromptText.SpriteName(s, (int)b.Value, true))), $"{set} {name} held: the ring");
                    }
                }
            }
        }

        [Test]
        public void TheSpriteNames_AreUnique_AndTheAtlasHoldsEveryOne()
        {
            var seen = new System.Collections.Generic.HashSet<string>();
            for (int h = 0; h < 2; h++)
                for (int s = 0; s < PromptText.SetCount; s++)
                    for (int g = 0; g < PromptText.GlyphCount; g++)
                    {
                        string n = PromptText.SpriteName(s, g, h == 1);
                        Assert.That(seen.Add(n), Is.True, n);
                        Assert.That(n, Does.Match("^[a-z0-9]+$"), "one lower-case word: no token of it reads as a key");
                    }
            int cells = 2 * PromptText.SetCount * PromptText.GlyphCount;
            int rows = (cells + UnityTuning.PromptAtlasColumns - 1) / UnityTuning.PromptAtlasColumns;
            Assert.That(rows * UnityTuning.PromptAtlasCell, Is.LessThanOrEqualTo(UnityTuning.PromptAtlasSize));
            // The packed atlas (pack_atlas.py) is the size the grid assumes (the PNG's IHDR: width and height, big-endian).
            string png = Path.Combine(Root(), "Assets", "ThirdParty", "KenneyInputPrompts", "Resources", UnityTuning.PromptAtlasResource + ".png");
            byte[] head = new byte[24];
            using (FileStream f = File.OpenRead(png)) f.Read(head, 0, 24);
            int w = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19], hgt = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
            Assert.That((w, hgt), Is.EqualTo((UnityTuning.PromptAtlasSize, UnityTuning.PromptAtlasSize)));
            Assert.That(File.Exists(Path.Combine(Root(), "Assets", "ThirdParty", "KenneyInputPrompts", "License.txt")), Is.True, "the CC0 licence beside the art");
        }

        [Test]
        public void ARemap_IsFollowed_AndSwitchIsDrawnByPosition()
        {
            PadLayout l = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV2, PadAction.Jump, PadButtonName.B).layout;
            Pad(GlyphSet.Xbox, l);
            Assert.That(PromptText.For("jump", false, "SPACE"), Is.EqualTo(Tag("xboxb")), "JUMP moved to B");
            Pad(GlyphSet.Nintendo, l);
            Assert.That(PromptText.For("jump", false, "SPACE"), Is.EqualTo(Tag("switchb")), "the east button (Nintendo prints A there): by position");
            Pad(GlyphSet.Xbox);
            Assert.That(PromptText.For("special", false, "D-PAD UP"), Is.EqualTo(Tag("xboxrb")), "v2: SPECIAL is on RB, whatever the site typed");
            Assert.That(PromptText.For("punch", false, "RB"), Is.EqualTo(Tag("xboxrs")), "v2: R3 punches");
        }

        [Test]
        public void TheWheelPrompt_IsTheButtonThatHoldsItOpen_NotTab()
        {
            Pad(GlyphSet.PlayStation);
            Assert.That(PromptText.For("radial", false, "TAB"), Is.EqualTo(Tag("psyhold")), "the keyboard's wheel key on a pad: TRIANGLE held");
            Assert.That(PromptText.For("weaponWheel", true, "Y"), Is.EqualTo(Tag("psyhold")));
            Pad(GlyphSet.Xbox, GamepadMath.DefaultLayoutV2.With(PadButtonName.Down, PadAction.Wheel));
            Assert.That(PromptText.For("radial", false, "TAB"), Is.EqualTo(Tag("xboxdownhold")), "a button bound to WEAPON WHEEL");
        }

        [Test]
        public void AKeyboardOnlyAction_ReadsAsNothingOnThePad_NeverAKey()
        {
            Pad(GlyphSet.Xbox);
            Assert.That(PromptText.For("restart", false, "R"), Is.EqualTo(""));
            Assert.That(PromptText.For("restart", false, null), Is.Null, "a site that passed null hides its chip");
            Assert.That(PromptText.For("forward", false, "W"), Is.EqualTo(Tag("xboxlstick")), "PAD_MOVE: the left stick");
            Assert.That(PromptText.For("back", false, "S"), Is.EqualTo(""));
        }

        [Test]
        public void TheHotbarsCycleLine_IsDrawn()
        {
            Pad(GlyphSet.Xbox);
            Assert.That(PromptText.For("nextWeapon", false, UnityTuning.HudPadCycleNeutral), Is.EqualTo(Tag("xboxy") + " NEXT · " + Tag("xboxleft") + " BACK"));
            Pad(GlyphSet.Nintendo);
            Assert.That(PromptText.For("nextWeapon", false, UnityTuning.HudPadCycleNintendo), Is.EqualTo(Tag("switchy") + " NEXT · " + Tag("switchleft") + " BACK"));
        }

        [Test]
        public void WithoutTheSprites_TheWordSetsStand()
        {
            Pad(GlyphSet.PlayStation, null, false);
            Assert.That(PromptText.For("interact", true, "HOLD E"), Is.EqualTo("HOLD SQUARE"));
            Assert.That(PromptText.For("jump", false, "SPACE"), Is.EqualTo("CROSS"));
            Pad(GlyphSet.Xbox, null, false);
            Assert.That(PromptText.For("altFire", false, "RMB"), Is.EqualTo("LT"));
        }

        [Test]
        public void TheKeyboard_KeepsEachSitesText_TheHoldBuyFollowsTheLiveKey()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root(), "PortExport", "config.json"));
            var keys = new Bindings(cfg);
            keys.Load(null);
            Pad(GlyphSet.Xbox);
            PlayerDevices.Last = InputDevice.KeyboardMouse;
            PlayerDevices.Bindings = keys;
            Assert.That(PromptText.For("interact", true, "HOLD E"), Is.EqualTo("HOLD E"));
            keys.Set("interact", "KeyF");
            Assert.That(PromptText.For("interact", true, "HOLD E"), Is.EqualTo("HOLD F"), "the site typed E; the live key is F");
            Assert.That(PromptText.For("restart", false, "R"), Is.EqualTo("R"));
            Assert.That(PromptText.For("altFire", false, "RMB"), Is.EqualTo("RMB"));
        }

        [Test]
        public void TheBlockOff_IsNight18sText()
        {
            Pad(GlyphSet.Xbox);
            PlayerDevices.N19 = false;
            Assert.That(PromptText.For("interact", true, "HOLD E"), Is.EqualTo("HOLD E"));
            Assert.That(PromptText.For("radial", false, "TAB"), Is.EqualTo("TAB"));
        }

        [Test]
        public void AnAnswerIsCached_TheSameStringBack_UntilTheDeviceChanges()
        {
            Pad(GlyphSet.Xbox);
            string a = PromptText.For("nextWeapon", false, UnityTuning.HudPadCycleNeutral);
            Assert.That(PromptText.For("nextWeapon", false, UnityTuning.HudPadCycleNeutral), Is.SameAs(a), "no string built per call");
            string b = PromptText.For("interact", true, "HOLD E");
            Assert.That(PromptText.For("interact", true, "HOLD E"), Is.SameAs(b));
            int v = PromptText.Version;
            PlayerDevices.Glyphs = GlyphSet.PlayStation;
            Assert.That(PromptText.Version, Is.Not.EqualTo(v), "a change moves the version (a site rebuilds its text once)");
            Assert.That(PromptText.For("interact", true, "HOLD E"), Is.EqualTo(Tag("psxhold")));
        }
    }
}
