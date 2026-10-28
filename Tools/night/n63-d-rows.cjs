// NIGHT63 builder D: writes ONLY design.n63.colours63.numbers (builder D's row, slate C1; the lead's A1 wrote the row with numbers {}) into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (the parsed file
// with that one object put back equals the old one, keys, values and order) and that the two copies are byte-identical (L-049, L-065).
//   node Tools/night/n63-d-rows.cjs [--dry]            first fill: refused unless the numbers are {} (A1's empty object)
//   node Tools/night/n63-d-rows.cjs --replace [--dry]  a later round: replaces builder D's own numbers object (still only that object)
// The row is read every frame (Colours63.N63D.cs: NightRows.Live); -skygrindOff63 (or older) and -skygrindNoColours63 turn it off. Its `enabled`
// stays false here: only a freeze turns it on.
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const replace = process.argv.includes("--replace");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];
const rowArg = process.argv.indexOf("--row");
const ROW = rowArg >= 0 ? process.argv[rowArg + 1] : "colours63"; // builder D's rows only: colours63 (C1), bossColours63 (B3)
if (ROW !== "colours63" && ROW !== "bossColours63") throw new Error("n63-d-rows writes builder D's rows only: colours63, bossColours63");

const BOSS_NUMBERS = {
  _why: "Builder D's boss parts for BossColours63 (Game/BossColours63Math.N63D.cs reads them, Game/BossColours63.N63D.cs applies them through one-line wrappers at the five boss views' colour sites, read every frame; NIGHT63.md section 3 B3: 'a boss's attacks are red-orange and its weak point always the same colour, on every boss'). Each named part goes to a ROLE of C1's colour table (design.n63.colours63.numbers), at the site's own luminance (a closed weak point stays dim, an open one bright): weak = the player's cyan (CLAUDE.md rule 8, DESIGN-RULES V-2: the thing the player acts on: four bosses already draw their weak point cyan when OPEN, each a different colour when closed: the call is logged for DECIDE63), danger = red-orange (an attack, an ember, a flame). No part goes to yellow (DECIDE63 D-1: the parry tell only; the parry tells (THE WICK's yellow core flash and embers, THE ICEBREAKER's block, THE LOOM's yellow spool tell) are not parts and keep their yellow), to magenta (RIVAL only) or to the boundary. G's danger column (bossTell63) is untouched. View only: the sim never reads it (no tape bit). The web has no such numbers: it has no campaign bosses.",
  parts: {
    _why: "part -> role. The site's file:line is in each part's _why.",
    "wick.core": "weak",
    "wick.core_why": "THE WICK's core at rest (Wick37View.cs, the idle core: Wick37WarmHex #FFA040, amber, off every band): its weak point, cyan when OPEN, so cyan at its idle glow too.",
    "wick.edges": "danger",
    "wick.edges_why": "THE WICK's ember-lit plate edges (Wick37View.cs, Wick37WarmHex): fire on an enemy, amber off every band: danger.",
    "crown.gems": "weak",
    "crown.gems_why": "THE CROWN's gems while closed (Crown37View.Crown38.cs gemC, Crown37GemHex #FF5A1F red-orange; Crown37View.Crown40.cs bone40 #F2EAD8): the weak points the HUD counts ('GEMS 20 / 20'), cyan in the window: one colour, dim while closed.",
    "crown.halo": "weak",
    "crown.halo_why": "THE CROWN's gem halos while closed (Crown37View.Crown38.cs haloC): the gems' own glow.",
    "crown.clap": "danger",
    "crown.clap_why": "THE CROWN's clap flash in gold (Crown37View.Look47.cs: CLook47GoldHex #FFC65A amber / CLook47RedGoldHex): an attack's flash: danger.",
    "crown.edge": "danger",
    "crown.edge_why": "THE CROWN's ring edge in gold by leg (Crown37View.Look47.cs: CLook47GoldHex amber): the enemy's own rim: danger (cyan when OPEN stays: the weak point's).",
    "crown.flameYellow": "danger",
    "crown.flameYellow_why": "THE CROWN's flame palette's yellow (Crown37View.N47.cs: Crown47YellowHex #FFC94A, the parry yellow's arc): fire: danger, never the parry's yellow.",
    "loom.knees": "weak",
    "loom.knees_why": "THE LOOM's knees at rest (Loom37View.cs: Loom37KneeIdleHex #D8D0FF, the boundary's pale violet-white): its weak points and grapple points, cyan when live: cyan at rest too (dim).",
    "loom.seams": "weak",
    "loom.seams_why": "THE LOOM's knee seams as a knee nears breaking (Loom37View.N47.cs: Loom47SeamHex #FFD23A, yellow): the weak point's state: cyan, never the parry's yellow.",
    "loom.nails": "weak",
    "loom.nails_why": "THE LOOM's nails and nail pips on a knee (Loom37View.cs: Loom37NailHex #FFB040, amber): the weak point's count: cyan.",
    "wyrm.punch": "weak",
    "wyrm.punch_why": "THE KILN WYRM's head while punchable (WyrmView.cs: Wyrm47PunchHex #FFD23A, yellow): its weak point ('punch the head'): cyan, never the parry's yellow.",
    "wyrm.open": "weak",
    "wyrm.open_why": "NIGHT63 B3b (the lead: on n63-b5's b3k the wyrm's same-run pairs changed 0 / 0 / 0 / 6 px, its punch window on no boss63 shot): THE KILN WYRM's head while it is OPEN (Wyrm31 staggered, or its core exposed: the HUD's 'OPEN', 'STAGGERED: ... SLUG IT'; the head takes x2 and a staggered wyrm full damage) is LIT as the punch window's head is (Wyrm47PunchHex x Wyrm47PunchGain) in the weak role: cyan, as bright as the punch window. Today the open head is the dark ash and jaw heat of every other moment (WyrmView.cs, the look47 head): nothing says 'shoot here' but BossMarks32's faint shell. With the row off the head is today's (BossColours63.Lit).",
    "wyrm.core": "weak",
    "wyrm.core_why": "THE KILN WYRM's exposed core (WyrmView.cs coreR while CoreOpen: WyrmCoreHex #FF7A20 x the open pulse; the core takes x3): the thing you shoot, red-orange today: the weak role at its own brightness. Its roar tell (the boundary colour) is not the weak point yet and is not routed.",
    "wyrm.seams": "danger",
    "wyrm.seams_why": "THE KILN WYRM's molten seams (WyrmView.cs trimC in the look47 branch: Wyrm47AmberHex #FF9A28 x Wyrm47SeamGain, hue 36: amber, just past danger's 35): fire on an enemy: danger. G's bossTell63 pulls them to danger during a listed wind-up (BossTell63Late); this keeps them there.",
    "crown.tell": "danger",
    "crown.tell_why": "NIGHT63 B3c (B3's GATE FAIL, Logs/n63/verdict-B3.md: 35,617 amber px, hue 35-45, on THE CROWN's ring during its tell with the row on): the crown's RED (dodge-only) wind-up (Crown37View.Crown.cs tellC = Tell36RedHex #FF3A14 on the blades, the gems, and Crown38's gem crosses, their halos and the halo ring) is danger already, but its sites multiply it by an HDR gain (up to x2.8) and the screen clips each channel: its green 0.23 becomes 0.64, amber. Routed after the gain (BossColours63.Hot), clip-safe. The YELLOW (parryable) wind-up is never routed: it is the parry tell (DECIDE63 D-1).",
    "wyrm.mark": "weak",
    "wyrm.mark_why": "NIGHT63 B3c (B3's GATE: THE KILN WYRM's weak point is never cyan on any boss63 shot, 0 cyan px among the changed px): the head the camera sees while OPEN is BossMarks32's open-window shell (1.45 x the head, additive cyan-white, pulsing 1.1-1.6) over the lit head, both under THE WYRM's grade (design.n54.grade54: a colour filter cutting blue to 0.78 and sulphur highlights), so the cyan read lime. With the row on the shell is the weak cyan through the fight's grade rim (BossGrade54.RimAt: the player's own cyan in this fight, pre-divided by the filter), at its own glow.",
    "wyrm.spit": "danger",
    "wyrm.spit_why": "THE KILN WYRM's spit and shards (WyrmView.cs spit / shard: WyrmSpitHex #FF7A30 x WyrmShotGain): its attack effects: danger's own red-orange.",
    "brawl.weak": "weak",
    "brawl.weak_why": "THE ICEBREAKER's stuck fists and dazed rims (Brawl40View.N47.cs cyanC: Brawl40YellowHex #FFD23A since N47; Brawl40CyanHex before it): its weak points ('punish stuck fists'): cyan again, never the parry's yellow (its thrown block keeps the yellow: that is its parry tell).",
  },
};

const NUMBERS_C1 = {
  _why: "Builder D's colour table (Game/Colours63Math.N63D.cs reads it, Game/Colours63.N63D.cs applies it, read every frame; NIGHT63.md section 3 C1; Research/2026-10-22-forge63/research-effect-colours.md: one hue one meaning, the world muted under the effects, one table every effect reads). View only: the sim never reads it (no tape bit; every fingerprint untouched). The bands and floors are set from our own frames first (the research: no studio publishes per-role numbers): Tools/night/n63-c1-meter.py on NIGHT62's freeze frames (Logs/feel/n62-freeze/v*/verbs37) and Tools/night/n63-c1-audit.cjs on the code. The web has no such numbers: its effects pick their colours one at a time.",
  roles: {
    _why: "The role colours (CLAUDE.md rule 8). Three are READ from config's own role colours, never typed here: the player's cyan, the enemies' and danger's red-orange, the boundary's pale violet-white. Two are typed here because config has no such colour: the parry yellow and RIVAL's magenta.",
    player: "colors.usable",
    player_why: "Cyan: player-usable (#19D3E6, hue 186).",
    danger: "colors.enemy",
    danger_why: "Red-orange: enemies and danger only (#FF4A1C, hue 11).",
    boundary: "barrier.boundaryColor",
    boundary_why: "Pale violet-white: the boundary (#C9BCFF), the boss door's tone (Doors38View).",
    parryHex: "#FFD000",
    parryHex_why: "DECIDE63 D-1: yellow is the parry tell's role only; yellow56's parry yellow (hue 49). No other effect, gun, pickup or UI element may take the yellow arc while colours63 is on.",
    rivalHex: "#FF2BD6",
    rivalHex_why: "Magenta: RIVAL only; the RIVAL's own magenta (UnityTuning.Rival25.cs RivalMagentaHex, Look49RivalHex: hue 311).",
  },
  bands: {
    _why: "Hue arcs in degrees on the sRGB colour (lo > hi wraps through 0), the same as n63-c1-meter.py's and n63-c1-audit.cjs's so the code, the table and the frames agree. Measured on NIGHT62's freeze frames: the vivid pixels inside these arcs are the tells, the usable cyan with its bloom fringe to 160, the RIVAL; what falls outside is the world's accents, lights and the lava (foundry amber 0.370 % of the frame, iceline blue 0.407 %, kiln blue 0.555 %, span lime 0.407 %, spire green 0.087 % + amber 0.077 %).",
    dangerLo: 345,
    dangerHi: 35,
    dangerLo_why: "Red-orange from 345 through 0 to 35: colors.enemy at 11, the enemy rims and pure red; 35 is where amber (the lava's gold, the fixtures' amber) starts.",
    parryLo: 45,
    parryHi: 70,
    parryLo_why: "Yellow 45-70: yellow56's band (n56-colour.py's V3).",
    playerLo: 160,
    playerHi: 205,
    playerLo_why: "Cyan 160-205: colors.usable at 186; its bloom and chromatic fringe render down to 160 (colour56's cap protects 150-200).",
    rivalLo: 285,
    rivalHi: 335,
    rivalLo_why: "Magenta 285-335: RIVAL at 311 and FUSION's hot pink (DESIGN-RULES V-2).",
    boundaryHueLo: 230,
    boundaryHueHi: 300,
    boundarySatMax: 0.4,
    boundaryValMin: 0.75,
    boundaryHueLo_why: "The boundary is pale (S under 0.40) and bright (V 0.75 and over) in the violet-blue arc 230-300: #C9BCFF is hue 252, S 0.26.",
    neutralSat: 0.15,
    neutralVal: 0.08,
    neutralSat_why: "Under S 0.15 or V 0.08 a colour has no hue that reads (white, grey, black): never a role, never routed. So is a dark weak tint under both route floors (S under routeSat and V under routeVal: the materials' gunmetal #3A3F4A, S 0.22, V 0.29).",
  },
  worldSat: 0.25,
  worldSat_why: "THE WORLD TIER: an off-palette world colour (an accent trim, a fixture, a light) keeps its hue (the level keeps its colour direction, GG 2026-10-20) at this saturation and its own luminance: under the effects (the research's saturation ladder: the world muted, the effects highest), under colour56's world cap (0.65-0.70) and under the meter's vivid floor (0.55), so it reads as the room's tint, never as a meaning. Span's lime trim #8DD957 (S 0.60) becomes a pale sage at the same brightness.",
  routeSat: 0.3,
  routeSat_why: "Only an off-palette colour at S 0.30 or over is routed: the iceline accent #86D9A2 (S 0.38) is; the materials' dark grey-blues (gunmetal #3A3F4A, S 0.22) are not.",
  routeVal: 0.35,
  routeVal_why: "Only a colour at V 0.35 or over is routed: the accents, the trims and the lights (V 0.85-1.0); the dark stone and plinth bases (V 0.10-0.30) keep their own tone.",
  routes: {
    _why: "A thing that wears a role it does not mean, moved to the role it does (one hue, one meaning).",
    goal47: "boundary",
    goal47_why: "THE CROWN's goal column and its window shafts (Route47View, crownLook47): red-gold #FF8A3A, inside the danger band, drawn through the walls as a thin line straight down spire's cyan exit door (gg-proxy's start check, 5 of 6 spire frames: 'a danger-red line through the exit'). It means 'the boss is there', not 'this hurts': the boundary's pale violet-white, the boss door's own tone (Doors38View), at the column's own brightness.",
    lavaGold: "danger",
    lavaGold_why: "The lava's gold (LavaView30 _Gold, #FFB84A, hue 36: amber, off every band; foundry's largest off-palette area, 0.370 % of its frames): lava hurts, so it wears danger's red-orange at the gold's own brightness.",
  },
};

const NUMBERS = ROW === "bossColours63" ? BOSS_NUMBERS : NUMBERS_C1;

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
const row = before.design && before.design.n63 && before.design.n63[ROW];
if (!row) throw new Error("no design.n63." + ROW + " (the lead's A1 writes it first)");
if (typeof row.numbers !== "object" || row.numbers === null) throw new Error("design.n63." + ROW + ".numbers is not an object");
if (!replace && Object.keys(row.numbers).length > 0) throw new Error("design.n63." + ROW + ".numbers is not empty (use --replace for a later round)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

const rowKey = '"' + ROW + '": {';
const rowAt = raw.indexOf(rowKey);
if (rowAt < 0 || raw.indexOf(rowKey, rowAt + 1) >= 0) throw new Error("design.n63." + ROW + " not found exactly once");
const key = '"numbers": ';
const numAt = raw.indexOf(key, rowAt);
if (numAt < 0) throw new Error(ROW + "'s numbers not found");
let i = numAt + key.length;
if (raw[i] !== "{") throw new Error("numbers does not open with {");
let depth = 0, inStr = false, end = -1;
for (let k = i; k < raw.length; k++) {
  const ch = raw[k];
  if (inStr) { if (ch === "\\") { k++; continue; } if (ch === '"') inStr = false; continue; }
  if (ch === '"') { inStr = true; continue; }
  if (ch === "{") depth++;
  else if (ch === "}") { depth--; if (depth === 0) { end = k + 1; break; } }
}
if (end < 0) throw new Error("numbers' closing brace not found");
const lineStart = raw.lastIndexOf("\n", numAt) + 1;
const indent = raw.slice(lineStart, numAt);
if (!/^ *$/.test(indent)) throw new Error("unexpected text before the numbers key on its line");
const block = JSON.stringify(NUMBERS, null, 2).split("\n").map((l, n) => (n === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, i) + block + raw.slice(end);

const after = JSON.parse(out);
if (JSON.stringify(after.design.n63[ROW].numbers) !== JSON.stringify(NUMBERS)) throw new Error("the numbers not written as the script says");
after.design.n63[ROW].numbers = row.numbers;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63." + ROW + ".numbers changed (keys, values or order)");
if (after.design.n63[ROW].enabled !== false && !replace) throw new Error(ROW + " must stay OFF here");
if (dry) {
  console.log("n63-d-rows --dry: design.n63." + ROW + ".numbers would be written (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-d-rows: design.n63." + ROW + ".numbers written (" + Object.keys(NUMBERS).filter((k) => !k.endsWith("_why")).join(", ") + "); enabled " + after.design.n63[ROW].enabled + "; both copies byte-identical; nothing else changed");
