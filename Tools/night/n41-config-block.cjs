// NIGHT41 shared commit (the lead): adds design.n41 to Design/unity-config.json (every row off), registers the four NIGHT41 level ids in
// design.maps.list (campaignOnly, after n40-spire: the campaign and -skygrindMap load a level id only through the list; NIGHT40's G0 missed
// it), writes a placeholder file for each new id (a byte copy of its levelFrom with only the "name" changed: n41-foundry from n38-foundry,
// n41-kiln from n40-kiln, n41-spire from n40-spire, n41-finale from n35-finale) so the list never points at a missing file before a builder's
// generator writes the real one, then copies the config to Assets/StreamingAssets/Design/ (byte-identical before this run). Refuses to run
// twice. Builders patch ONLY their own row's text afterwards (L-049), with a .cjs script.
// Run: node Tools/night/n41-config-block.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the block");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
if (d.design.n41) throw new Error("design.n41 already exists");

// 1. the maps.list rows
const levels = [
  ["n41-foundry", "LEVEL 1 THE FOUNDRY", "n38-foundry"],
  ["n41-kiln", "LEVEL 3 THE KILN", "n40-kiln"],
  ["n41-spire", "LEVEL 5 THE SPIRE", "n40-spire"],
  ["n41-finale", "LEVEL 6 GREYBOX", "n35-finale"],
];
const anchorRe = /(\{\s*"id":\s*"n40-spire",[^}]*\})/;
const m = raw.match(anchorRe);
if (!m) throw new Error("the n40-spire row was not found");
if (raw.includes('"id": "n41-')) throw new Error("n41 rows already present");
const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
const add = levels.map(([id, label]) => "," + eol + indent + JSON.stringify({ id, label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
raw = raw.replace(anchorRe, m[1] + add);

// 2. design.n41, every row off
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const off = "Off until its check passes and the lead has looked at its Player frames once and iterated once (NIGHT41.md method 2; closing_reminder).";
const n41 = {
  _why: "NIGHT41, the finish night (GG 2026-10-14 ~08:40, verbatim: 'ok lets make a prompt related to things that were not done or still partial, expand upon that much more and think of other things to imporve and make better'; Research/2026-10-14-wave41/GG-REQUEST.md, INVENTORY.md; NIGHT41.md; DECIDE41.md). Every row is off in the shared commit and turned on only when its check passes and its frames were looked at. Sim rows (sweep41, tokens41, heavy41, aim41, foundry41, kiln41, spire41, finale41) are behind N41Blocks; the view rows (juice41, juice41On, doors41On, pump41On, bars41, entrance41, select41, look41, brawlLook41, models41) are N41View rows (no bit). Off (--off41 / -skygrindOff41, and every older off switch): NIGHT40's game bit for bit. Old rows are turned on through n41 rows, never in place: juice38 = design.n38.juice38 || juice41On, doors38 = design.n38.doors38 || doors41On, pump40 = design.n40.pump40 || pump41On. Level rows swap levelFrom for levelTo (Curriculum25.N41Levels); an n41 level is its levelFrom to every older system (SurvivalSim.N41BaseId).",
  sweep41: { _why: "Builder B8 (NIGHT41.md B8; INVENTORY A4; GG w40 asked for floor cues): THE LOOM sweeps its canyon floor (a red-orange decal tell of at least 0.9 s, boss_rules 1); the answer is height (a ride wall, a ledge, a hook); in phase 2 it is the reason to hook. B8 adds its numbers with their _why.", enabled: false, enabled_why: off },
  tokens41: { _why: "Builder E6 (NIGHT41.md E6; INVENTORY A5; DECIDE41 D-5): at most two enemies winding up at once in the campaign, across the token pools (survival keeps its pools). E3 measured up to 5 committed, more than 2 for 3.5 % of fight time. E6 adds its numbers with their _why.", enabled: false, enabled_why: off },
  heavy41: { _why: "Builder E6 (NIGHT41.md E6; INVENTORY C7; GG w40: 'the gauntlet excels against large enemies'): the gauntlet on a big enemy (tank, WARDEN, CHARGER, THE ICEBREAKER) answers harder: a longer hitstop and a knock-back (Loadout.Heavy41, Core/Heavy41.cs). 'Big' is a per-kind flag in this row. E6 adds its numbers with their _why.", enabled: false, enabled_why: off },
  aim41: { _why: "Builder P3 (NIGHT41.md P3; INVENTORY C2; DECIDE41 D-6: the pad's 'assist' mode only): aim assist finds a boss's weak point (Enemy.weakPoint41: the CYAN core in a window, THE LOOM's lit knee, THE CROWN's ring while passable, THE ICEBREAKER's stuck fist), centre mass otherwise. P3 adds its numbers with their _why.", enabled: false, enabled_why: off },
  foundry41: { _why: "Builder R5 (NIGHT41.md R5; INVENTORY A12; DECIDE41 D-2 = DECIDE40 D-14): level 1 as n41-foundry (from n38-foundry; n38-foundry untouched): a no-ride band on THE GANTRY's long b2-g1 wall so the chain of walls is needed again under the endless ride. If GG keeps the single ride, this row stays off.", enabled: false, enabled_why: off, levelFrom: "n38-foundry", levelTo: "n41-foundry" },
  kiln41: { _why: "Builder E6 (NIGHT41.md E6; INVENTORY C5; GG w40: 'make weapon-switching meaningful'): level 3 as n41-kiln (from n40-kiln; n40-kiln untouched): one CHARGER room on the kiln's open floor. kiln40's start set, awards and boost chain read n41-kiln as n40-kiln (SurvivalSim.N41BaseId).", enabled: false, enabled_why: off, levelFrom: "n40-kiln", levelTo: "n41-kiln" },
  spire41: { _why: "Builder R5 (NIGHT41.md R5; INVENTORY A10, C5): level 5 as n41-spire (from n40-spire; n40-spire untouched): grapple nodes where they serve the descent (one per descent room at least) and E6's DRONE room, so VERBS rows 17 / 19 / 22 / 23 pass on level 5.", enabled: false, enabled_why: off, levelFrom: "n40-spire", levelTo: "n41-spire" },
  finale41: { _why: "Builder L6 (NIGHT41.md L6; INVENTORY A13; DECIDE41 D-3): the level 6 greybox (n41-finale) from its beat sheet, put back as the campaign's last level while on. OFF tonight: GG reacts to the plan first, and level 6's slot stays locked.", enabled: false, enabled_why: "OFF by DECIDE41 D-3 (GG reacts to the plan first).", levelTo: "n41-finale" },
  juice41: { _why: "Builder J2 (NIGHT41.md J2; INVENTORY B1, C4): the juice's iteration after its frames (the entrance beat's order and lengths, the static); a view row; numbers in UnityTuning.*41.cs.", enabled: false, enabled_why: off },
  juice41On: { _why: "Builder J2 (NIGHT41.md J2; INVENTORY B1; GG w37: the shake 'when he first enters', the static 'like a TV'): turns juice38 on (design.n38.juice38 stays as it is) after T2's entrance frames and one iteration.", enabled: false, enabled_why: off },
  doors41On: { _why: "Builder J2 (NIGHT41.md J2; INVENTORY B2; GG w37: the doors 'a holographic barrier or a different colour'): turns doors38 on (design.n38.doors38 stays as it is) after T2's door frames and one iteration; locked red-orange, open cyan, the boss door pale violet-white.", enabled: false, enabled_why: off },
  pump41On: { _why: "Builder T2 (NIGHT41.md T2; INVENTORY A1; GG w40: 'The shotgun's firing animation should also be sped up'): turns pump40 on (design.n40.pump40 stays as it is) after the weapons41 frames and one iteration.", enabled: false, enabled_why: off },
  bars41: { _why: "Builder J2 (NIGHT41.md J2; INVENTORY C3; GG w35 'hard to understand what to do', w40 THE CROWN 'no idea'): every boss's bar shows a tick at each phase threshold and the stagger meter under it (UiBossBar41 replaces the HUD33 bar while on).", enabled: false, enabled_why: off },
  entrance41: { _why: "Builder J2 (NIGHT41.md J2; INVENTORY C4): the boss entrance beat for all five bosses: the name card, juice38's shake + flicker and the boss's BossLine40 rule line in one 1.5 s beat, the same order every time.", enabled: false, enabled_why: off },
  select41: { _why: "Builder J2 (NIGHT41.md J2; INVENTORY C9; DECIDE41 D-7): each level-select slot shows the level's name, its new weapon and its boss's silhouette; slot 6 'REBUILDING', locked.", enabled: false, enabled_why: off },
  look41: { _why: "Builder K2 (NIGHT41.md K2; INVENTORY A3; DECIDE41 D-4): room light colour by theme (iceline cool, foundry warm, kiln molten amber, span dusk violet, spire pale gold; never cyan or red-orange) and level 2's canal look (ice sheen, frost) as view props; numbers in UnityTuning.Look41.cs.", enabled: false, enabled_why: off },
  brawlLook41: { _why: "Builder B8 (NIGHT41.md B8; INVENTORY C6; GG w40: 'the model lacks polish'): THE ICEBREAKER in ice armour (pale blue-white plates, a dark stone core), the fists glowing through their tells, a death that shatters into blocks, more poses.", enabled: false, enabled_why: off },
  models41: { _why: "Builder E6 (NIGHT41.md E6; INVENTORY A7, A8): the HUSK's own feature (the blind reader's weakest pair husk / spitter) and a death break-apart for every kind's parts.", enabled: false, enabled_why: off },
};
d.design.n41 = n41;
const block = JSON.stringify(n41, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
const out = head + eol + "    }," + eol + "    \"n41\": " + block + eol + "  }" + eol + "}";
const chk = JSON.parse(out);
const ids = chk.design.maps.list.map((x) => x.id);
for (const [id] of levels) if (!ids.includes(id)) throw new Error("missing " + id);

// 3. the placeholder level files (both copies; a builder's generator replaces them)
for (const [id, , from] of levels) {
  for (const dir of ["Design/levels/", "Assets/StreamingAssets/Design/levels/"]) {
    const dst = root + dir + id + ".json";
    if (fs.existsSync(dst)) throw new Error(dst + " exists");
    let lv = fs.readFileSync(root + "Design/levels/" + from + ".json", "utf8");
    const key = '"name":"' + from + '"';
    const n = lv.split(key).length - 1;
    if (n < 1 || n > 2) throw new Error(from + ": the name key found " + n + " times");
    lv = lv.split(key).join('"name":"' + id + '"'); // the level's name and, when present, its data block's name (n38-foundry has both)
    const p = JSON.parse(lv);
    if (p.name !== id || (p.data && p.data.name !== undefined && p.data.name !== id)) throw new Error(dst + ": the names did not change");
    fs.writeFileSync(dst, lv);
  }
}

fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n41 added: " + Object.keys(n41).filter((k) => k !== "_why").join(", "));
console.log("design.maps.list now " + ids.length + " rows; placeholders: " + levels.map((l) => l[0] + " (from " + l[2] + ")").join(", "));
