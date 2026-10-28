// NIGHT8 U2-13: the MODS draft screen (main.js's draft screen over mods.js view()): PICK A MOD and the round, three cards,
// keys 1-3 or a click take one; REROLL for mods.rerollCost while one is left. The game stays paused until a pick.
// NIGHT17 U3-30.8 (builder N): the cards in the identity: a slanted opaque INK-SOFT card, its rarity as a thick bar on top
// (the cyan value ladder: CYAN common, CYAN-HOT rare; RARE in CYAN-HOT on a rare card); REROLL shows its cost in GOLD and,
// NIGHT16 polish #57 (u-small2-n16, 91caa6b), is disabled (skipped by the focus) with no reroll left or when it can't be
// paid; the cards stay focusable, so the draft always has something to select.
// Round 5: the line under the number is one finished thought (UnityTuning.ModLines17: "+25%" over "DAMAGE BEYOND 20 M");
// gold only for a mod that pays points (SCAVENGER has its own crate glyph); the card's texts white (readable on the focus).
// Round 3 (the reviewer: "three identical text-only cards, the top 40 % empty, no icons"): each card leads with a 96 px
// glyph drawn in code from simple shapes per tag (UiModGlyph: ECONOMY a coin, pierce an arrow, AIR a double chevron, ...)
// in the tag's colour (gold only for ECONOMY, which pays points; STYLE's gold is the cyan ladder's CYAN-HOT), the mod's
// number as the card's hero (MenuRules.ModHero: "+25%", S56; GOLD when it is points), the rest of the line under it; card 1
// shows focused when the draft opens (UiNav: the frame parked on it; nothing is selected until a move, so a jump's A never
// takes a card).
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        GameObject draftScreen;
        TextMeshProUGUI draftHead, rerollLabel, bigTimer, bigHint;

        sealed class ModCardView
        {
            public GameObject go;
            public TextMeshProUGUI name, tag, hero, desc, stack;
            public UiShape bar;
            public UiModGlyph glyph;
        }

        readonly List<ModCardView> cards = new List<ModCardView>(3);
        int shownDraft = -1;

        void BuildDraft()
        {
            draftScreen = Screen("Mods draft", UiTheme.A(UiTheme.InkSolid, UnityTuning.MenuSheetAlpha));
            float h1 = UiTheme.Px(UiTheme.Size.S56);
            draftHead = Text(draftScreen.transform, "Head", "PICK A MOD", h1, UiTheme.White, TextAlignmentOptions.Center, new Vector2(0, 300), new Vector2(1200, 80), new Vector2(0.5f, 0.5f));
            UiTheme.Style(draftHead, h1, UiTheme.White, UiTheme.Display, shadow: true);
            draftHead.fontStyle = FontStyles.Normal;
            float w = UnityTuning.ModCardWidth, h = UnityTuning.ModCardHeight;
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var go = new GameObject($"Card {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
                var rt = (RectTransform)go.transform;
                rt.SetParent(draftScreen.transform, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2((i - 1) * (w + 40), 20);
                rt.sizeDelta = new Vector2(w, h);
                var plate = go.GetComponent<UiShape>();
                plate.color = UiTheme.A(UiTheme.InkSoft, 1f); // opaque (round 2)
                plate.raycastTarget = true;
                plate.Slant = UnityTuning.ThemeSlantDeg * 0.5f; // a tall card leans less (the same direction)
                var b = go.AddComponent<UnityEngine.UI.Button>();
                b.transition = UnityEngine.UI.Selectable.Transition.None; // the focus frame shows it
                b.onClick.AddListener(() => game.PickMod(idx));
                var card = new ModCardView { go = go };
                card.bar = ModShape(rt, "Rarity", UiTheme.Cyan, new Vector2(0, h * 0.5f - 6), new Vector2(w - 8, 12));
                var gGo = new GameObject("Glyph", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiModGlyph));
                var grt = (RectTransform)gGo.transform;
                grt.SetParent(rt, false);
                grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.5f);
                grt.anchoredPosition = new Vector2(0, UnityTuning.ModGlyphY);
                grt.sizeDelta = new Vector2(UnityTuning.ModGlyphSize, UnityTuning.ModGlyphSize);
                card.glyph = gGo.GetComponent<UiModGlyph>();
                card.glyph.raycastTarget = false;
                ModText(rt, "Key", (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), UiTheme.Size.S24, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.TopLeft, new Vector2(-w * 0.5f + 34, h * 0.5f - 34), new Vector2(40, 30));
                card.tag = ModText(rt, "Tag", "", UiTheme.Size.S18, UiTheme.CyanHot, UiTheme.BodyBold, TextAlignmentOptions.TopRight, new Vector2(w * 0.5f - 70, h * 0.5f - 34), new Vector2(110, 30));
                card.tag.characterSpacing = UnityTuning.ThemeLabelTracking;
                card.name = ModText(rt, "Name", "", UiTheme.Size.S32, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0, UnityTuning.ModNameY), new Vector2(w - 30, 44));
                card.hero = ModText(rt, "Hero", "", UiTheme.Size.S56, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0, UnityTuning.ModHeroY), new Vector2(w - 30, 70));
                card.desc = ModText(rt, "Desc", "", UiTheme.Size.S24, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Top, new Vector2(0, UnityTuning.ModDescY), new Vector2(w - 50, 56));
                card.desc.textWrappingMode = TextWrappingModes.Normal;
                card.stack = ModText(rt, "Stack", "", UiTheme.Size.S18, UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0, -h * 0.5f + 24), new Vector2(w - 30, 26));
                card.stack.characterSpacing = UnityTuning.ThemeLabelTracking;
                cards.Add(card);
            }
            rerollLabel = Button(draftScreen.transform, "REROLL", new Vector2(0, -260), () =>
            {
                game.mods.Reroll();
            });
            rerollLabel.richText = true;
            // NIGHT24 (builder F, UiBigPick24.cs): the BIG PICK's line under the cards (the default pick's countdown); hidden for the old draft
            float tPx = UiTheme.Px(UiTheme.Size.S24);
            bigTimer = Text(draftScreen.transform, "Big pick timer", "", tPx, UiTheme.White, TextAlignmentOptions.Center, new Vector2(0, -260), new Vector2(900, 40), new Vector2(0.5f, 0.5f));
            UiTheme.Style(bigTimer, tPx, UiTheme.White, UiTheme.BodyBold);
            bigTimer.fontStyle = FontStyles.Normal;
            bigTimer.gameObject.SetActive(false);
            bigHint = Text(draftScreen.transform, "Big pick hint", "", tPx, UiTheme.Cyan, TextAlignmentOptions.Center, new Vector2(0, -306), new Vector2(900, 40), new Vector2(0.5f, 0.5f));
            UiTheme.Style(bigHint, tPx, UiTheme.Cyan, UiTheme.BodyBold);
            bigHint.fontStyle = FontStyles.Normal;
            bigHint.gameObject.SetActive(false);
            // NIGHT18: the head, the three cards and REROLL enter along the slant one after another (FeelAB on), inside the
            // draft's own rise (UiFeelHud's TransitionWatch moves the screen; these move within it)
            MenuPart18Add(draftScreen, draftHead);
            for (int i = 0; i < cards.Count; i++) MenuPart18Add(draftScreen, cards[i].go.transform);
            MenuPart18Add(draftScreen, rerollLabel.transform.parent);
            // round 3 ("nearly pixel-identical" to NIGHT17): the cards rise from farther along the slant, one by one
            MenuScreen18 ds = MenuScreenOf18(draftScreen);
            ds.partDistance = UnityTuning.DraftCardEnter18;
            ds.partCascade = UnityTuning.DraftCascade18;
        }

        UiShape ModShape(Transform parent, string name, Color c, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var s = go.GetComponent<UiShape>();
            s.color = c;
            s.raycastTarget = false;
            return s;
        }

        TextMeshProUGUI ModText(Transform parent, string name, string s, UiTheme.Size size, Color c, TMP_FontAsset face, TextAlignmentOptions align, Vector2 pos, Vector2 box)
        {
            float px = UiTheme.Px(size);
            TextMeshProUGUI t = Text(parent, name, s, px, c, align, pos, box, new Vector2(0.5f, 0.5f));
            UiTheme.Style(t, px, c, face);
            t.fontStyle = FontStyles.Normal;
            return t;
        }

        /// <summary>A glyph's colour: gold only for a mod that pays points (round 5: MenuRules.ModPaysPoints); the tag's colour
        /// otherwise (config), but never gold: ECONOMY's or STYLE's gold becomes CYAN-HOT (the cyan ladder).</summary>
        static Color ModGlyphColor(Mods mods, ModDef d)
        {
            if (MenuRules.ModPaysPoints(d.id)) return UiTheme.Gold;
            if (d.tag == "ECONOMY" || d.tag == "STYLE") return UiTheme.CyanHot;
            return Look.Hex(mods.TagColor(d.tag));
        }

        /// <summary>The draft screen's cards from the open offer (rebuilt when the mods change).</summary>
        void RenderDraft()
        {
            if (draftScreen == null) BuildDraft();
            Mods mods = game.mods;
            if (mods.BigOffer)
            {
                RenderBigPick24(mods); // NIGHT24 (builder F): the BIG PICK's cards, its countdown and its pad A (UiBigPick24.cs)
                return;
            }
            if (mods.version == shownDraft || mods.offer == null) return;
            shownDraft = mods.version;
            if (bigStyled) EndBigPick24Style(); // the last draft was a BIG PICK: the old cards' parts and REROLL come back
            draftHead.SetText("PICK A MOD  ·  ROUND {0:0}", mods.offerRound);
            for (int i = 0; i < cards.Count; i++)
            {
                bool on = i < mods.offer.Count;
                cards[i].go.SetActive(on);
                if (!on) continue;
                ModDef d = mods.defs[mods.offer[i]];
                int k = mods.N(d.id);
                cards[i].name.text = d.name;
                cards[i].tag.text = d.tier == "rare" ? "RARE" : "";
                cards[i].bar.color = d.tier == "rare" ? UiTheme.CyanHot : UiTheme.Cyan;
                cards[i].glyph.Set(d.id == "scavenger" ? "drop" : d.tag, ModGlyphColor(mods, d)); // round 5: SCAVENGER drops power-ups (not a coin)
                string hero = MenuRules.ModHero(ModMath.ModLine(d, k + 1).ToUpperInvariant(), out string rest);
                cards[i].hero.text = hero;
                cards[i].hero.color = MenuRules.ModPaysPoints(d.id) ? UiTheme.Gold : UiTheme.White; // gold: points only
                cards[i].desc.text = MenuRules.ModLine17(d.id, rest); // round 5: one finished line (the number's noun)
                cards[i].stack.text = $"STACK {k + 1} / {d.max}";
            }
            bool can = NavLook.RerollEnabled(mods.rerollsLeft, game.economy.CanAfford(mods.m.rerollCost));
            rerollLabel.text = mods.rerollsLeft > 0 ? $"REROLL  <color=#{UiTheme.GoldHex}>{mods.m.rerollCost:0}</color>" : "NO REROLLS";
            NavSetEnabled(rerollLabel.transform.parent.GetComponent<UnityEngine.UI.Selectable>(), can);
        }
    }

    /// <summary>
    /// Round 3: a mod's glyph drawn in code from simple shapes, one per tag, in one colour (the Graphic's): ECONOMY a coin (a
    /// ring and a slot), pierce an arrow, AIR a double chevron, WALL a wall and a rider, SLIDE speed lines, HEALTH a cross,
    /// MELEE a fist block, STYLE a star, volt a bolt, fire a flame, edge a blade, gravity a ring round a core, blast a burst;
    /// anything else a hexagon. Unit space -1..1 mapped onto the rect; rebuilt only when Set changes the tag.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiModGlyph : UnityEngine.UI.MaskableGraphic
    {
        string kind;

        public void Set(string tag, Color c)
        {
            color = c;
            if (tag == kind) return;
            kind = tag;
            SetVerticesDirty();
        }

        Rect box;

        Vector2 P(float x, float y) => new Vector2(box.center.x + x * box.width * 0.5f, box.center.y + y * box.height * 0.5f);

        void Tri(UnityEngine.UI.VertexHelper vh, Vector2 a, Vector2 b, Vector2 c)
        {
            int i = vh.currentVertCount;
            Color32 col = color;
            vh.AddVert(a, col, Vector2.zero);
            vh.AddVert(b, col, Vector2.zero);
            vh.AddVert(c, col, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
        }

        void Quad(UnityEngine.UI.VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Tri(vh, a, b, c);
            Tri(vh, a, c, d);
        }

        void Rect4(UnityEngine.UI.VertexHelper vh, float x0, float y0, float x1, float y1) => Quad(vh, P(x0, y0), P(x0, y1), P(x1, y1), P(x1, y0));

        void Star(UnityEngine.UI.VertexHelper vh, float cx, float cy, float r0, float r1, int n, float rot)
        {
            for (int i = 0; i < n * 2; i++)
            {
                float a0 = rot + Mathf.PI * i / n, a1 = rot + Mathf.PI * (i + 1) / n;
                float ra = i % 2 == 0 ? r1 : r0, rb = i % 2 == 0 ? r0 : r1;
                Tri(vh, P(cx, cy), P(cx + Mathf.Sin(a0) * ra, cy + Mathf.Cos(a0) * ra), P(cx + Mathf.Sin(a1) * rb, cy + Mathf.Cos(a1) * rb));
            }
        }

        void Disc(UnityEngine.UI.VertexHelper vh, float cx, float cy, float r, int n) => Star(vh, cx, cy, r, r, n, 0);

        void Ring(UnityEngine.UI.VertexHelper vh, float cx, float cy, float r0, float r1, int n)
        {
            for (int i = 0; i < n; i++)
            {
                float a0 = 2 * Mathf.PI * i / n, a1 = 2 * Mathf.PI * (i + 1) / n;
                float s0 = Mathf.Sin(a0), c0 = Mathf.Cos(a0), s1 = Mathf.Sin(a1), c1 = Mathf.Cos(a1);
                Quad(vh, P(cx + s0 * r0, cy + c0 * r0), P(cx + s0 * r1, cy + c0 * r1), P(cx + s1 * r1, cy + c1 * r1), P(cx + s1 * r0, cy + c1 * r0));
            }
        }

        void Chevron(UnityEngine.UI.VertexHelper vh, float y, float t)
        {
            Quad(vh, P(-0.85f, y - 0.45f), P(0, y + 0.4f), P(0, y + 0.4f - t), P(-0.85f, y - 0.45f - t));
            Quad(vh, P(0, y + 0.4f), P(0.85f, y - 0.45f), P(0.85f, y - 0.45f - t), P(0, y + 0.4f - t));
        }

        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear();
            box = GetPixelAdjustedRect();
            switch (kind)
            {
                case "ECONOMY":
                    Ring(vh, 0, 0, 0.62f, 0.92f, 28);
                    Rect4(vh, -0.12f, -0.42f, 0.12f, 0.42f);
                    break;
                case "pierce":
                    Quad(vh, P(-0.85f, -0.65f), P(-0.65f, -0.85f), P(0.35f, 0.15f), P(0.15f, 0.35f));
                    Tri(vh, P(0.9f, 0.9f), P(-0.05f, 0.55f), P(0.55f, -0.05f));
                    break;
                case "AIR":
                    Chevron(vh, 0.35f, 0.32f);
                    Chevron(vh, -0.3f, 0.32f);
                    break;
                case "WALL":
                    Rect4(vh, -0.9f, -0.9f, -0.55f, 0.9f);
                    Quad(vh, P(-0.4f, -0.2f), P(-0.3f, 0.2f), P(0.9f, 0.2f), P(0.8f, -0.2f));
                    break;
                case "SLIDE":
                    Quad(vh, P(-0.9f, 0.35f), P(-0.8f, 0.6f), P(0.5f, 0.6f), P(0.4f, 0.35f));
                    Quad(vh, P(-0.6f, -0.12f), P(-0.5f, 0.12f), P(0.9f, 0.12f), P(0.8f, -0.12f));
                    Quad(vh, P(-0.9f, -0.6f), P(-0.8f, -0.35f), P(0.3f, -0.35f), P(0.2f, -0.6f));
                    break;
                case "HEALTH":
                    Rect4(vh, -0.25f, -0.85f, 0.25f, 0.85f);
                    Rect4(vh, -0.85f, -0.25f, 0.85f, 0.25f);
                    break;
                case "MELEE":
                    Rect4(vh, -0.7f, -0.55f, 0.6f, 0.45f);
                    Rect4(vh, -0.7f, 0.55f, 0.6f, 0.85f);
                    Rect4(vh, 0.7f, -0.35f, 0.9f, 0.3f);
                    break;
                case "STYLE":
                    Star(vh, 0, 0, 0.4f, 0.95f, 5, 0);
                    break;
                case "volt":
                    Quad(vh, P(-0.05f, 0.95f), P(0.6f, 0.95f), P(0.15f, 0.05f), P(-0.45f, 0.05f));
                    Tri(vh, P(-0.3f, 0.2f), P(0.55f, 0.2f), P(-0.35f, -0.95f));
                    break;
                case "fire":
                    Disc(vh, 0, -0.3f, 0.6f, 24);
                    Tri(vh, P(-0.57f, -0.12f), P(0.57f, -0.12f), P(0.1f, 0.95f));
                    break;
                case "edge":
                    Quad(vh, P(-0.85f, -0.85f), P(-0.05f, 0.25f), P(0.85f, 0.85f), P(0.25f, -0.05f));
                    break;
                case "gravity":
                    Ring(vh, 0, 0, 0.72f, 0.95f, 28);
                    Disc(vh, 0, 0, 0.35f, 18);
                    break;
                case "drop":
                    // round 5 (SCAVENGER): a power-up crate with a falling chevron over it
                    Rect4(vh, -0.6f, -0.9f, 0.6f, -0.2f);
                    Rect4(vh, -0.7f, -0.2f, 0.7f, -0.05f);
                    Chevron(vh, 0.45f, 0.3f);
                    break;
                case "blast":
                    Star(vh, 0, 0, 0.45f, 0.95f, 8, 0);
                    break;
                default:
                    Disc(vh, 0, 0, 0.85f, 6);
                    break;
            }
        }
    }
}
