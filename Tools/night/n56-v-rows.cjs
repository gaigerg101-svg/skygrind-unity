// NIGHT56 builder V: THE ONE NUMBERS WRITER of V's rows (builder brief "Your numbers"). Writes into both config copies (Design/unity-config.json
// and Assets/StreamingAssets/Design/unity-config.json, byte-identical):
//   - design.n56.<row>.numbers for V's own rows (colour56, rim56, yellow56): the whole object below (NUMBERS), replacing the row's numbers;
//   - design.n55.grade55.numbers / design.n55.decor55.numbers (NIGHT55's rows, V's tonight): only the keys listed in SETS (added or replaced;
//     every other key of those objects kept as it is).
// Asserts the copies match before and after, that each written numbers object parses back to what this script built, and that nothing
// outside those numbers objects changed. The rows' `enabled` is never touched (the lead flips rows in a build's copy).
//   node Tools/night/n56-v-rows.cjs            (every row below)      node Tools/night/n56-v-rows.cjs grade55 colour56   (only those)
const fs = require("fs");
const path = require("path");

const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

// ---- design.n55 rows: keyed sets (path inside numbers -> value); a `<key>_why` beside each ----
const SETS = {
  "n55.grade55": [
    [["hot", "burnOpaqueMin"], 0.5, "share of hot.burn: while a burn's heat is at least this, its patch's iron quads are drawn OPAQUE in the danger colour (Wick37View.N55.cs, NIGHT56 V). n56-r1 a155: the additive red-orange over the graded indigo iron summed to a pink (the patch S 0.22 vs the walls' 0.66; round 1 0.48 vs 0.74): an additive glow cannot out-saturate a saturated wall. 0.5: the burn's first ~0.45 s of hot.burnS 0.8, then round 2's additive fade."],
    // THE CROWN (the r1 critic, gate-r1.md P0-wickGrade: walls 4 deg from danger at the turn, near-danger 0.26-0.31, p1 vs raw dH 15 / dS -0.079)
    [["crown37", "p1", "balance"], 50, "NIGHT56 V (attempt 2): 42 -> 50: the shadows' tint weight lower on the ring's red-orange tells, the dark walls still toned"],
    [["crown37", "p1", "sat"], 0, "NIGHT56 V attempt 2: back to 0 (n56-r2: -12 cut the marginal tell cells under proof.tellSat 0.5: kept 400 / 527 = 0.76, and moved the walls' S only -0.094); the p1 shift now comes from the split's hue (crown37.p1.split)"],
    [["crown37", "p1", "split"], [0.2, 0.42, 0.85], "NIGHT56 V attempt 2: a deeper, more saturated steel-blue shadow (~218, S 0.76; was [0.36, 0.46, 0.66], S 0.45, which moved the walls' hue 11-16 deg: the critic's dH >= 25 against raw); the balance keeps it off the bright tells"],
    [["crown37", "p2", "balance"], 44, "NIGHT56 V attempt 2: 40 -> 44"],
    [["crown37", "p2", "split"], [0.18, 0.34, 0.88], "NIGHT56 V attempt 2: a deeper blue (~222, S 0.8; was [0.4, 0.34, 0.72]): n56-r2's p2 walls 0.29 near the danger hue (<= 0.25 wanted)"],
    [["crown37", "p3", "split"], [0.2, 0.36, 0.86], "NIGHT56 V attempt 2: as p2's, a step less"],
    [["crown37", "p1", "turnBalance"], -15, "NIGHT56 V: the split's balance at the turn's peak (BossGrade55.cs): r1's turn darkened the frame and left the CROWN FIRE's red-lit walls as the walls' mean (H 8, 4 deg from danger); at -15 the turn's cool reaches the lit mid-tones"],
    [["crown37", "p1", "turn"], [0.26, 0.32, 0.84], "NIGHT56 V: the start turn's shadow colour, a saturated slate-blue (~228; was [0.34, 0.4, 0.72]): the turn swings the walls away from the fire's red"],
    [["crown37", "p2", "sat"], 0, "NIGHT56 V attempt 2: 0, as p1's"],
    [["crown37", "p2", "turnBalance"], -15, "NIGHT56 V: as p1's"],
    [["crown37", "p2", "turn"], [0.24, 0.4, 0.8], "NIGHT56 V: the p2 turn a deeper sea-steel (~215; was [0.34, 0.52, 0.64] ~190: under the -15 balance it would reach the player's cyan family)"],
    [["crown37", "p3", "sat"], 0, "NIGHT56 V attempt 2: 0, as p1's"],
    [["hot", "panelTrimScale"], 2, "NIGHT56 V attempt 2 (the r2 critic: 'give THE WICK's ride panels a cyan strip'; n56-r2 a155: the panels' cyan share 0-8 %): the ride panels' top and bottom cyan strips at 2 x Stack37TrimH (0.16 -> 0.32 m: ~32 px at 8 m) while grade55 is on (Wick37View.N55.cs PanelTrimH55; BuildStack's one line). P1 folds the panels into the wall (wick47): no strip there by design."],
    [["crown37", "p3", "turnBalance"], -15, "NIGHT56 V: as p1's"],
    // THE WYRM (V4; NIGHT55's P1-wyrmTurn; critic-ggproxy #3: n54-r3 g3k's turn "floods the arena acid green", GG NIGHT47: "the same yellow-red-orange color theme")
    [["wyrm"], {
      _why: "L3 THE KILN WYRM (BossGrade55.cs reads it while THE WYRM fights, wyrm24's retry phase as the leg; a155 on n52-kiln measures the turn). WARM shadows (amber-umber split toning, shadows only: a positive balance keeps the molten tells and the player's cyan), the turn swinging to a hotter orange / amber with the balance at 0 so it reaches the lit walls: the yellow-red-orange theme kept through every phase; over grade54's acid-green highlights (grade55's volume, priority 51, overrides the split toning at weight 1).",
      on: 1, on_why: "1: THE WYRM graded with the row; 0: grade54's look (NIGHT55's)",
      p1: { split: [0.62, 0.48, 0.34], balance: 30, ev: -0.15, contrast: 12, fog: [0.16, 0.09, 0.05], fogMix: 0.45, ambient: [1, 0.88, 0.72], key: [1, 0.9, 0.75], turn: [0.7, 0.42, 0.22], turnBalance: 0,
        _why: "P1 the kiln floor: amber-umber shadows (~30), a warm dark fog; the start turn swings to orange (~25)" },
      p2: { split: [0.64, 0.44, 0.3], balance: 28, ev: -0.22, contrast: 16, fog: [0.18, 0.08, 0.04], fogMix: 0.5, ambient: [1, 0.85, 0.68], key: [1, 0.88, 0.7], turn: [0.72, 0.52, 0.2], turnBalance: 0,
        _why: "P2: a hotter umber (~27), a step darker; its turn swings to amber (~37)" },
      p3: { split: [0.66, 0.4, 0.28], balance: 24, ev: -0.3, contrast: 22, fog: [0.2, 0.07, 0.03], fogMix: 0.55, ambient: [1, 0.82, 0.64], key: [1, 0.85, 0.66], turn: [0.7, 0.36, 0.18], turnBalance: 0,
        _why: "P3: red-umber (~22), the darkest; its turn swings to a deep orange (~21)" },
    }, "NIGHT56 V (V4): THE WYRM's palette (BossGrade55.Bosses gains 'wyrm'; no object here = grade54's look)"],
    [["proof", "wyrmTurnLo"], 15, "NIGHT56 V (V4): a155's WYRM verdict: the turn frame's wall cells' mean hue at least this (amber-orange, never the acid green 90-150 of n54-r3 g3k)"],
    [["proof", "wyrmTurnHi"], 55, "NIGHT56 V (V4): ... and at most this (GG NIGHT47: 'the same yellow-red-orange color theme'; the slate's 15-55)"],
    [["hot", "burnIron"], 1,"the opaque burning iron's colour as a share of Stack37HotHex (#ff5a1f: S 0.88, the danger family): 1 = the token's own colour, unlit and LDR, so the tonemapper keeps its saturation (the patch's edges at 45 % of it: a darker red, the same hue)."],
  ],
};

SETS["n55.decor55"] = [
  // the r1 critic (gate-r1.md P0-decor: FAIL 1, 4): foundry's lit floor-edge strips rendered hue ~60, S 0.87 (16 deg from parry 44); usable cyan not
  // the brightest class on 3 of 12 entry frames (accent p95 luminance 0.888 vs 0.786, 0.870 vs 0.837, 0.716 vs 0.691)
  [["levels", "foundry", "accentHex"], 0x6b8c73, "NIGHT56 V (round 2): 0x6b8c73 OXIDE GREEN (hue 135, s 0.24, v 0.55: the foundry's grade renders a green ~3x as saturated: n56-r2 a356 measured s 0.55 -> 0.98), colour56's foundry key (the slate: colors.md's oxide green wins over the 72 deg green-brass): on the lit floor-edge strips the green-brass clipped toward yellow (hue ~60 on the frame: 16 deg from parry); a green clips toward a lighter green, 65 deg from the yellow band. Was 0xc0db53 (hue 72)."],
  [["trimLum"], 0.6, "NIGHT56 V: the moved floor trims' luminance x this (Decor54.FixAccent via Decor55.TrimLum): Fixtures33 draws them at its HDR tier and they were the accent's brightest pixels (p95 0.716-0.888 vs usable cyan's 0.691-0.837 on 3 of 12 entry frames); x0.6 puts them under the usable cyan at the same tier and out of the per-channel clip that moved their hue."],
];

// ---- design.n56 rows (V's own): the whole numbers object ----
const NUMBERS = {
  colour56: {
    _why: "Builder V (Game/Colour56.cs; slate V1; GG 'way too bland' x3). A view row: each level of 1-5 its own colour direction, a KEY (an unlit frieze along the room walls, UnityTuning.N56V.cs Colour56Frieze*) and an ACCENT (every decor54 accent piece and decor55's string course: colour56 owns the accent while on), both saturated (S 0.5-0.62, under the tells' ~0.85-0.9) and never a role hue (danger 5-35, yellow 45-70, cyan 170-200, magenta 285-330: the hues below keep >= 18 deg from every band). The accents are pairwise >= 45 deg apart (268, 140, 222, 95, 345); THE SPIRE's key is >= 47 deg from every other key (88 vs 135, 224, 262, 345). Hexes are sRGB numbers (0x.. in each _why), made from HSV by the builder's scratch script (colorsys). The web has no such numbers (one survival map, one palette). GG's eye decides the keys: Research/2026-10-20-wave56/V.md lists each with its reason.",
    look49: {
      _why: "round 3 (the r2 critic: 'bring the world's p99.5 saturation under the tells' on every frame'): per theme (LightPlan33.Themes), the entries of look49's tables colour56 replaces while on (Colour56.Look49, once before a campaign map is themed). n56-r2 a356: the world's top-0.5 % saturation 1.00 on the foundry (jade-lit, look49 saturation +23) and the spire (emerald-lit, +28; its gold key turns the stone dark olive: 'yellow' by HSV on 30 / 36 verbs37 frames) on the OFF sides too: colour56 does not draw those pixels; these are the grade's own numbers.",
      foundry: { saturation: 0, saturation_why: "look49's +23 (UnityTuning.V49Saturation[0]) -> 0: the jade-lit world at S 1.00 under the tells' 0.86-0.95" },
      spire: { saturation: 0, saturation_why: "look49's +28 -> 0", rimHex: 0xa6ffac, rimHex_why: "0xa6ffac: look49's emerald rim 0x59ff64 (hue 124, s 0.65) at s 0.35: under the pale gold key the s 0.65 green rim made the stone dark olive on every wall (spire v6's 'one olive-green on every surface'; n56-r3 v5-mid: 5.4 % dark yellow outside the parry orbs)" },
    },
    trimLum: 0.6,
    trimLum_why: "the moved floor trims' luminance x this while colour56 is on (Decor55.TrimLum; decor55's own trimLum while only decor55 is on): the r1 critic saw the lit floor-edge strips clip toward a neighbour hue and outshine the usable cyan (gate-r1.md P0-decor); every colour56 accent on a bright HDR trim would clip the same way (violet toward magenta, cobalt toward cyan)",
    kinds: ["wall"],
    kinds_why: "map box kinds whose room faces carry the key's frieze (as decor55's bandKinds: the room walls; ledges, floors, cover and spires keep their own lines: you stand on those)",
    levels: {
      _why: "keyed as decor54's (the level id after its 'nNN-' prefix). keyHex: the frieze; accentHex: decor54's pieces and the string course; wallBaseHex / wallUpperHex (optional): decor54's wall ramp replaced (Decor54.ScriptFor's hook).",
      foundry: {
        keyHex: 0x6b8c73, keyHex_why: "0x6b8c73 OXIDE GREEN authored at hue 135, s 0.24, v 0.55 (colors.md's verdigris key, 125-140): the foundry's copper and iron gone green in the soot. Round 2: n56-r2 a356 measured the frieze authored at s 0.55 (0x39804b) RENDERING s 0.98 (median rgb 0.01 / 0.40 / 0.10: the foundry's grade, look49's jade tint x saturation +23 x contrast 25, crushes red and blue on a green), over the tells' 0.89; a power-law fit of that sample (min/max 0.45 -> 0.02) puts s 0.24 at ~0.75 rendered. 35 deg off cyan's band, 65 off the danger band.",
        accentHex: 0xa267e6, accentHex_why: "0xa267e6 TEMPER VIOLET (hue 268, s 0.55, v 0.90): the violet-blue of tempered steel; decor55's green-brass (72) sat 2 deg from the yellow band (the slate: oxide green wins over decor55's 72). 18 deg off the magenta band, 62 off cyan's.",
        wallBaseHex: 0x1b1e21, wallBaseHex_why: "0x1b1e21 SOOT IRON at the wall's foot (hue 210, s 0.18, v 0.13; colors.md: soot-iron walls H 210-220, S 0.1-0.2): decor54's foundry ramp was orange-brown (0x2a1e17 / 0x7a5e44: hue 22-29, s 0.44: the danger band on every wall, the n54 a452 frames' 'sky warm' 9-14 %).",
        wallUpperHex: 0x545c66, wallUpperHex_why: "0x545c66 the soot iron's upper band (hue 213, s 0.18, v 0.40): the same value ramp as decor54's (dark plinth, lighter band), cool and low in chroma so the green frieze and the orange hazards carry the colour.",
      },
      iceline: {
        keyHex: 0x6e5994, keyHex_why: "0x6e5994 INDIGO-VIOLET (hue 262, s 0.40, v 0.58; late NIGHT56: s 0.50 -> 0.40: n56-r4's A/B read the line's frieze, then at 224 s 0.62, rendering S 0.83 under the azure light and grade: over the tells' 0.68-0.71): round 3 (the r2 critic: 'give iceline and span a key hue that differs from their walls' own, so the key is a band, not the frame'; n56-r2 iceline TINT FAIL 12 / 12, cover 53.7 %: the ultramarine key 224 was the hue of the line's azure-lit walls, look49 216 / look52's lift): iceline and span swap keys, so the frieze sits 46 deg from the line's azure light (night ice, a violet dusk band over blue stone). Was 0x385194 (224). 23 deg off magenta's band.",
        accentHex: 0x86d9a2, accentHex_why: "0x86d9a2 PINE / AURORA GREEN (hue 140, s 0.38, v 0.85; colors.md's pine accent): the one colour the blue line does not wear (decor54's cobalt 222 vanished into the slate walls, O-07). Round 2: s 0.55 -> 0.38 (n56-r2: rendered s 0.86, p90 0.99: the line's grade crushes red on a green too). 30 deg off cyan's band.",
        wallBaseHex: 0x262729, wallBaseHex_why: "0x262729 a NEUTRAL slate at the wall's foot (hue 220, s 0.07, v 0.16). Round 2: n56-r2 a356 iceline TINT FAIL 12 / 12 (>= 60 % of the world within 25 deg of the key: decor54's ramp is itself the key's blue, 0x161e2c / 0x5c6c84 at s 0.30-0.50, under look49's azure light: one hue on every surface, the trap GG named): the walls go neutral so the frieze and the pine accent carry the colour on a grey ground (colors.md methods 3 / 11).",
        wallUpperHex: 0x7d8085, wallUpperHex_why: "0x7d8085 the neutral upper band (hue 220, s 0.06, v 0.52): decor54's value ramp kept (dark plinth, lighter band).",
      },
      kiln: {
        keyHex: 0x944a5c, keyHex_why: "0x944a5c ROSE CLAY (hue 345, s 0.50, v 0.58): THE KILN WYRM's level stays warm (GG NIGHT47: 'the same yellow-red-orange color theme') by S and value on the kiln's own fired-clay hue (decor54's walls 324-340), not by moving into the danger band: 20 deg off it, 15 off magenta's; the wyrm's and the vents' red-orange keep the frame's top saturation (0.85-0.9 vs 0.50).",
        accentHex: 0x5a82e0, accentHex_why: "0x5a82e0 COBALT GLAZE (hue 222, s 0.60, v 0.88; colors.md's kiln accent 222-230): blue glaze on the clay, the warm key's complement. 22 deg off cyan's band.",
      },
      span: {
        keyHex: 0x385194, keyHex_why: "0x385194 ULTRAMARINE (hue 224, s 0.62, v 0.58): round 3 (the r2 critic, as iceline's: n56-r2 span TINT FAIL 6 / 6, cover 33.7 %: the violet key 262 was the hue of the span's violet-lit walls, look49 272): the keys swap, so the frieze sits 48 deg from the span's violet light (a deep blue band under the violet open sky). Was 0x654a94 (262). 24 deg off cyan's band.",
        wallBaseHex: 0x262529, wallBaseHex_why: "0x262529 a NEUTRAL slate at the wall's foot (hue 255, s 0.10, v 0.16). Round 2: n56-r2 a356 span TINT FAIL 6 / 6: decor54's ramp is the key's own violet (0x161530 / 0x46447e, s 0.46-0.56) under look49's violet light: the walls go neutral, the frieze and the lime accent carry the colour.",
        wallUpperHex: 0x797780, wallUpperHex_why: "0x797780 the neutral upper band (hue 255, s 0.07, v 0.50): decor54's value ramp kept.",
        accentHex: 0x8dd957, accentHex_why: "0x8dd957 SIGNAL LIME (hue 95, s 0.60, v 0.85; colors.md's signal-green lamps, moved to 95 so the five accents stay >= 45 deg apart): 25 deg off the yellow band, 75 off cyan's.",
      },
      spire: {
        keyHex: 0x7c9460, keyHex_why: "0x7c9460 ABSINTHE (hue 88, s 0.35, v 0.58; late NIGHT56: was 0x688c3f at s 0.55, which n56-r4's A/B read rendered at hue ~100, S 0.98 under look49's +28 saturation and emerald tint: over the tells; r5 also has that saturation at 0): colors.md's achromatic spire is rejected (the slate: no neutral key); the only arc >= 40 deg from the other four keys (135, 224, 262, 345) that is not a role hue is 75-95. Saturated and bright enough to read as a colour, never the low-chroma olive GG named on spire v6 (that was one hue on every surface: here one frieze band). 18 deg off the yellow band. The alternative for GG: jade 160 (s 0.5, v 0.45), 10 deg off cyan's band.",
        accentHex: 0xe67793, accentHex_why: "0xe67793 ROSE (hue 345, s 0.48, v 0.90): the spire's lamps and trims against the green-gold key (the kiln's key hue, two levels apart). Round 2: s 0.55 -> 0.48 (n56-r2: rendered s 0.76, p90 0.83, near the tells' 0.85-0.89). 15 deg off magenta's band, 20 off the danger band; with the other four accents at 268 / 140 / 222 / 95 the 330-5 arc is the only one left >= 40 deg from all of them, and its widest margins are here.",
        wallBaseHex: 0x1d1f24, wallBaseHex_why: "0x1d1f24 SLATE at the wall's foot (hue 225, s 0.18, v 0.14): decor54's spire ramp was a warm grey (0x24221e / 0x8a8474, hue 40-44) that the level's pale gold key turns dark olive (n55-freeze verbs37 spire: 30 of 36 frames over 0.5 % 'yellow' by HSV, median 16.9 %, a wall at 1 m 58.6 %; spire v6's 'one olive-green on every surface'); a cool slate under the gold light renders a near-neutral stone.",
        wallUpperHex: 0x6e7280, wallUpperHex_why: "0x6e7280 the slate's upper band (hue 225, s 0.14, v 0.50): decor54's value ramp kept (dark plinth, lighter band), the chroma under the frieze's.",
      },
    },
  },
  rim56: {
    _why: "Builder V (Game/Rim56.cs; slate V2). Render numbers only: skin52's shader globals while the row is on (UnityTuning.N56V.cs Rim56*: the shadow side's warm multiplier, the lit gain, the near rim's share and power). No gameplay number.",
  },
  yellow56: {
    _why: "Builder V (Game/Yellow56.cs; slate V3: yellow means parry only). The viewmodel guns whose family colour (the web's weapons.families) sits in the amber-yellow arc take a pale brushed-brass saturation; a level's decor accent in the yellow arc takes a level accent here when neither colour56 nor decor55 sets one. Enemy / boss yellow (THE CROWN's gems, visors) is GG's design: untouched. The web has no such numbers (its parry has no colour rule).",
    gunHueLo: 28, gunHueLo_why: "deg: the arc's low end (weapons.families.fire #ff8c1a, hue 30, the TORCH: its plate renders ~45 under the viewmodel's warm key: n54-r3 g3s a254 crown37-p1, n55-freeze vs verbs37 f0950: 2-6 % of the frame)",
    gunHueHi: 75, gunHueHi_why: "deg: the arc's high end (edge #ffb000 41 the THUNDERCLAP, blast #ffe14a 50 the BOUNCER; volt #b8ff2c 80 the STITCHER family stays: 10 deg past the yellow band)",
    gunMinS: 0.5, gunMinS_why: "a family colour at or under this S is already not 'yellow' by the meter's rule (S > 0.5): left alone",
    gunS: 0.32, gunS_why: "the plate's S (hue and value kept): a pale brushed brass / champagne (#ffd9ad-ish for the THUNDERCLAP's #ffb000): warm metal, never the parry tells' saturated yellow (S 0.77 at the token)",
    floorS: 0.18, floorS_why: "round 2 (n56-r2 a356 foundry: 6 / 6 frames over 0.5 % yellow, 0.9-2.4 %, mostly V 0.25-0.4: the tan floors (hue ~41) the foundry's grade turns ochre): a map-stone FLOOR vertex colour (Yellow56Floors, world normal y > 0.7) in floorHueLo..floorHueHi above this S drops to it (hue and value kept: a warm grey floor, Stone.shader reads the vertex colour)",
    floorHueLo: 30, floorHueLo_why: "deg: the floors' arc low end (a tan / ochre that the grades push into 45-70)",
    floorHueHi: 75, floorHueHi_why: "deg: the floors' arc high end (the spire's olive-lit stone)",
    accentHueLo: 35, accentHueLo_why: "deg: a level decor accent from here ...",
    accentHueHi: 80, accentHueHi_why: "deg: ... to here renders in the yellow band on the lit floor trims (decor54's foundry brass 41 -> ~55 on n55-freeze vf verbs37 f1500; spire's 77 green-gold; decor55's 72 -> ~60 on n56-r1 d1on)",
    levels: {
      _why: "the replacement accent per level (keyed as decor54's), used only while neither colour56 nor decor55 gives the level one",
      foundry: { accentHex: 0x6b8c73, accentHex_why: "0x6b8c73 oxide green (hue 135, s 0.24: the foundry's grade triples a green's saturation): decor55's fixed foundry accent and colour56's foundry key: one green on the foundry whichever row sets it" },
      spire: { accentHex: 0xe67793, accentHex_why: "0xe67793 rose (hue 345, s 0.48): colour56's spire accent (decor54's 0xb4d85a, hue 77, renders yellow-green on the lit trims)" },
    },
  },
};

// ------------------------------------------------------------------------------------------------------------------------------------
const want = process.argv.slice(2);
const pick = (k) => !want.length || want.includes(k.split(".")[1]);
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const before = JSON.parse(raw);

function fmt(v, ind) {
  const pad = " ".repeat(ind);
  if (Array.isArray(v)) {
    if (!v.length) return "[]";
    if (v.every((x) => typeof x !== "object" || x === null)) return "[" + v.map((x) => JSON.stringify(x)).join(", ") + "]";
    return "[" + eol + v.map((x) => pad + "  " + fmt(x, ind + 2)).join("," + eol) + eol + pad + "]";
  }
  if (v && typeof v === "object") {
    const keys = Object.keys(v);
    if (!keys.length) return "{}";
    return "{" + eol + keys.map((k) => pad + "  " + JSON.stringify(k) + ": " + fmt(v[k], ind + 2)).join("," + eol) + eol + pad + "}";
  }
  return JSON.stringify(v);
}

function setPath(o, keys, value, why) {
  let t = o;
  for (let i = 0; i < keys.length - 1; i++) {
    if (!t[keys[i]] || typeof t[keys[i]] !== "object") throw new Error("no object at " + keys.slice(0, i + 1).join("."));
    t = t[keys[i]];
  }
  const k = keys[keys.length - 1];
  t[k] = value;
  if (why) t[k + "_why"] = why;
}

// the target numbers per row ("n55.grade55" -> object)
const targets = {};
for (const [row, sets] of Object.entries(SETS)) {
  if (!pick(row)) continue;
  const [grp, name] = row.split(".");
  if (!before.design[grp] || !before.design[grp][name]) throw new Error("no design." + row);
  const num = JSON.parse(JSON.stringify(before.design[grp][name].numbers || {}));
  for (const [keys, value, why] of sets) setPath(num, keys, value, why);
  targets[row] = num;
}
for (const [name, num] of Object.entries(NUMBERS)) {
  const row = "n56." + name;
  if (!pick(row)) continue;
  if (!before.design.n56 || !before.design.n56[name]) throw new Error("no design." + row);
  targets[row] = num;
}

let lines = raw.split(eol);
for (const [row, num] of Object.entries(targets)) {
  const [grp, name] = row.split(".");
  const g = lines.findIndex((l) => l.trim() === '"' + grp + '": {');
  if (g < 0) throw new Error("no group line for " + grp);
  const open = lines.findIndex((l, k) => k > g && l.trim() === '"' + name + '": {');
  if (open < 0) throw new Error("no row line for " + row);
  const ind = lines[open].match(/^ */)[0].length;
  const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
  const n = lines.findIndex((l, k) => k > open && k < end && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith('"numbers": '));
  if (n < 0) throw new Error(row + ": its numbers key not found");
  let last = n;
  if (!/^"numbers": \{\},?$/.test(lines[n].trim())) {
    last = lines.findIndex((l, k) => k > n && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith("}"));
    if (last < 0) throw new Error(row + ": the numbers object's end not found");
  }
  const comma = lines[last].trim().endsWith(",") ? "," : "";
  const text = (" ".repeat(ind + 2) + '"numbers": ' + fmt(num, ind + 2) + comma).split(eol);
  lines = lines.slice(0, n).concat(text, lines.slice(last + 1));
}
const out = lines.join(eol);
const after = JSON.parse(out);
for (const [row, num] of Object.entries(targets)) {
  const [grp, name] = row.split(".");
  if (JSON.stringify(after.design[grp][name].numbers) !== JSON.stringify(num)) throw new Error(row + ": written numbers differ from the script's");
  if (after.design[grp][name].enabled !== before.design[grp][name].enabled) throw new Error(row + ": enabled changed");
  after.design[grp][name].numbers = before.design[grp][name].numbers;
}
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something outside the rows' numbers changed");
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n56-v-rows: numbers written for " + Object.keys(targets).join(", ") + " (both copies; enabled untouched)");
