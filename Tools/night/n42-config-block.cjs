// NIGHT42 shared commit (the lead): adds design.n42 to Design/unity-config.json (every row off), registers n42-foundry in design.maps.list
// (campaignOnly, after n41-finale), writes its placeholder file (a byte copy of n41-foundry with only the "name" changed) so the list
// never points at a missing file before L1's generator writes the real one, then copies the config to Assets/StreamingAssets/Design/
// (byte-identical before this run). Refuses to run twice. Builders patch ONLY their own row's text afterwards (L-049), with a .cjs script.
// Run: node Tools/night/n42-config-block.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the block");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
if (d.design.n42) throw new Error("design.n42 already exists");

// 1. the maps.list row
const levels = [["n42-foundry", "LEVEL 1 THE FOUNDRY", "n41-foundry"]];
const anchorRe = /(\{\s*"id":\s*"n41-finale",[^}]*\})/;
const m = raw.match(anchorRe);
if (!m) throw new Error("the n41-finale row was not found");
if (raw.includes('"id": "n42-')) throw new Error("n42 rows already present");
const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
const add = levels.map(([id, label]) => "," + eol + indent + JSON.stringify({ id, label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
raw = raw.replace(anchorRe, m[1] + add);

// 2. design.n42, every row off
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const off = "Off until its check passes and the lead has looked at its before / after Player frames and iterated once (NIGHT42.md method 2; closing_reminder).";
const g = "Research/2026-10-14-wave42/GG-REQUEST.md";
const n42 = {
  _why: "NIGHT42, the feel night (GG 2026-10-14 ~18:35 after playing n41-r7, fifteen asks verbatim in " + g + " as G1-G15; NIGHT42.md; DECIDE42.md; CODE-FACTS.md). Every row is off in the shared commit and turned on only when its check passes and its before / after frames were looked at. Sim rows (wallfling42, boost42, foundry42, gauntlet42, parry42, charge42, impact42, brawl42, arena42) are behind N42Blocks; the view rows (reticle42, meter42, style42, health42, teach42, kills42) are N42View rows (no bit). Off (--off42 / -skygrindOff42, and every older off switch): NIGHT41's game bit for bit. foundry42 swaps levelFrom for levelTo (Curriculum25.N42Levels); an n42 level is its levelFrom to every older system (SurvivalSim.N42BaseId through N41BaseId).",
  wallfling42: { _why: "Builder M1 (G2, GG: 'pressing a near a wall you fly bcak so fast and it create very awkward movement'; DECIDE42 D-4): a wall jump only when the player means it (moving into the wall or riding it), a smaller back push with more lift, none in a landing's first steps; the ride's jump-off back speed capped. Mover.cs's wall-jump branch only (named tonight). M1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  boost42: { _why: "Builder M1 (G5, GG: 'shoutgun jumping is too difficult and needs to be eaiser to boost'): the THUNDERCLAP air boost easier: more lift, a looser pitch, lift scaled by pitch below level (CODE-FACTS: today velocity -= aim x 10, no pitch rule). M1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  foundry42: { _why: "Builder L1 (G1, GG: 'the waallride section on level one is awful and relaly difficult and not fun'; DECIDE42 D-1): level 1 as n42-foundry (from n41-foundry; n41-foundry untouched): THE GANTRY forgiving (shorter gaps or more walls, a recovery ledge, the band visible); the chain stays the answer.", enabled: false, enabled_why: off, levelFrom: "n41-foundry", levelTo: "n42-foundry" },
  gauntlet42: { _why: "Builder W1 (G3, GG: 'make the pulse gauhtlent attack faster .7 seconds faster'; DECIDE42 D-2): the PULSE swing 2.5 s (design.n27.pulse27.swingSeconds, GG NIGHT27 'attack much slower') -> 1.8 s; the n27 row untouched; the damage x6 stays.", enabled: false, enabled_why: off, swingSeconds: 1.8, swingSeconds_why: "GG's 0.7 s faster than today's 2.5 s (CODE-FACTS G3)." },
  parry42: { _why: "Builder W1 (G4, GG: 'the parry attack timing from the flying enimies is relaly tight and you miss depsite felling like you hit it'; DECIDE42 D-3: orbs only): the DRONE orb's parry forgives (a larger reach, a mercy window after contact, a longer early buffer; ULTRAKILL 1.03b, hitfeel-web.md). W1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  charge42: { _why: "Builder W1 (G10 + G8, GG: 'if you are hgolding down fire button and charge attack is reday t go again it should start automaticaly'; 'complete animation before you can preform necxt attack'): a held fire after a charged shot starts the next charge when ready (no tap spam); the next attack waits for the reticle's turn. W1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  impact42: { _why: "Builder F1 (G9 + G15, GG: 'hits and kills need to feel more satifying'; 'a greater or mor epowerful effect when hitting it or any other enemy iwth gaughtlent'): hitstop per weapon on a hit, a bigger freeze on a kill, the gauntlet's the strongest, THE ICEBREAKER's the biggest (hitfeel-web.md's numbers). F1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  brawl42: { _why: "Builder B1 (G14, GG: 'level 2 boss might needs more attacks and needs to be more difficult'; DECIDE42 D-5): THE ICEBREAKER gains two new attacks with fair tells (red-orange >= 0.9 s), phase 2 faster; bot band 40-70 s; never more HP. B1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  arena42: { _why: "Builder K1 (G12, GG: 'way better arena deisgn for the gameplay. more elements that are not just decoration'): arena elements that change play (a hazard enemies are knocked into, breakable cover, a launch pad, ...), each with a sim rule, a view and a level-data form (arenas-web.md, ARENA-KIT.md). K1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  reticle42: { _why: "Builder W1 (G8, GG: 'animation for redical for all weapons needs to spin faster and complete animation'): every weapon's reticle turn faster (view numbers in UnityTuning.*42.cs); the attack gate is charge42's.", enabled: false, enabled_why: off },
  meter42: { _why: "Builder U1 (G7, GG: 'the meter or how the pulse or charge attack works field award and ... needs to be polished and more accurate'): the arc40 meter shows the true charge / cooldown each frame (no eased under-read, the PULSE swing cooldown shown).", enabled: false, enabled_why: off },
  style42: { _why: "Builder U1 (G13, GG: 'styule meter ... needs to feel aweosme to combo and UI needs to be animated'): the style meter redesigned: rank letters that punch in, shake and decay (DECIDE42 D-6), combo count; motion on every change.", enabled: false, enabled_why: off },
  health42: { _why: "Builder U1 (G13, GG: '... heathbar need to look better'): the health bar redesigned: a damage chunk that drains, a heal glow, motion on every change.", enabled: false, enabled_why: off },
  teach42: { _why: "Builder L1 (G6, GG: 'way too much instruction at the tsrat of the level'; DECIDE42 D-7): at most two lines in a level's first 60 s (CODE-FACTS: level 1 shows 7); the rest when the verb is first needed.", enabled: false, enabled_why: off },
  kills42: { _why: "Builder F1 (G9 / G15): the kill's view (a burst, a crunchier break, KillFeel42) and the gauntlet's shockwave ring; view only.", enabled: false, enabled_why: off },
};
d.design.n42 = n42;
const block = JSON.stringify(n42, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
const out = head + eol + "    }," + eol + "    \"n42\": " + block + eol + "  }" + eol + "}";
const chk = JSON.parse(out);
const ids = chk.design.maps.list.map((x) => x.id);
for (const [id] of levels) if (!ids.includes(id)) throw new Error("missing " + id);

// 3. the placeholder level file (both copies; L1's generator replaces them)
for (const [id, , from] of levels) {
  for (const dir of ["Design/levels/", "Assets/StreamingAssets/Design/levels/"]) {
    const dst = root + dir + id + ".json";
    if (fs.existsSync(dst)) throw new Error(dst + " exists");
    let lv = fs.readFileSync(root + "Design/levels/" + from + ".json", "utf8");
    const key = '"name":"' + from + '"';
    const n = lv.split(key).length - 1;
    if (n < 1 || n > 2) throw new Error(from + ": the name key found " + n + " times");
    lv = lv.split(key).join('"name":"' + id + '"');
    const p = JSON.parse(lv);
    if (p.name !== id || (p.data && p.data.name !== undefined && p.data.name !== id)) throw new Error(dst + ": the names did not change");
    fs.writeFileSync(dst, lv);
  }
}

fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n42 added: " + Object.keys(n42).filter((k) => k !== "_why").join(", "));
console.log("design.maps.list now " + ids.length + " rows; placeholder: " + levels.map((l) => l[0] + " (from " + l[2] + ")").join(", "));
