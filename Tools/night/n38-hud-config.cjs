// NIGHT38 builder H: writes ONLY the rows design.n38.hud38 and design.n38.juice38 of Design/unity-config.json (L-049: a builder patches its own rows),
// then copies the file byte for byte to Assets/StreamingAssets/Design/unity-config.json (the Player's copy; the two must stay identical).
// Run from the repo (or a worktree): node Tools/night/n38-hud-config.cjs          (the juice38 numbers; each row's `enabled` kept as it is)
//                                    node Tools/night/n38-hud-config.cjs --on     (the same, and both rows' `enabled` set true: the lead's frames)
//                                    node Tools/night/n38-hud-config.cjs --off    (both rows' `enabled` set false)
// Every other row's text is left exactly as it is: the script replaces the two rows' own text spans only.
const fs = require("fs");
const path = require("path");
const repo = path.resolve(__dirname, "..", "..");
const file = path.join(repo, "Design", "unity-config.json");
const copy = path.join(repo, "Assets", "StreamingAssets", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const cfg = JSON.parse(raw);
const n38 = cfg.design && cfg.design.n38;
if (!n38 || !n38.hud38 || !n38.juice38) throw new Error("design.n38.hud38 / juice38 missing (the NIGHT38 shared commit adds them)");
const want = process.argv.includes("--on") ? true : process.argv.includes("--off") ? false : null;

const hud38 = {
  _why: n38.hud38._why,
  enabled: want === null ? n38.hud38.enabled : want,
};

const juice38 = {
  _why: n38.juice38._why + " Builder H's numbers (ULTRAKILL's decompiled rules, Research/2026-10-13-wave38/research/uk-hud-feel.md sections 3-4; render numbers in Game/UnityTuning.Juice38.cs). The shake is ULTRAKILL's CameraShake: amounts in its units, the bigger replaces (never adds), linear decay shakeDecay per real second, the camera moving min(shake, 1) of its full turn (a shake above 1 only lasts longer).",
  enabled: want === null ? n38.juice38.enabled : want,
  entranceShake: 2.5,
  entranceShake_why: "A boss entrance (every boss's bossRise, and boss38Entrance for an explicit call): ULTRAKILL's boss stomps are 2-3 (Minotaur 1.5-3, Minos 2-3, Geryon 3); 2.5 holds the full turn for 0.5 s and fades out by 0.83 s at shakeDecay 3.",
  entranceHitstop: 0.12,
  entranceHitstop_why: "s: the world freezes this long on the entrance (through SurvivalGame.Hitstop: the impact layer's Sim tier, inside its budget: 0.2 s a second, max 0.5 s); ULTRAKILL's parry TrueStop is 0.25 s, a boss entrance is a beat, not a stop.",
  entranceFlicker: 0.3,
  entranceFlicker_why: "s: the full-screen CRT flicker on an entrance (PLAN.md section 4: Crt29's strength spiked ~0.3 s): two brightness pulses (<= 3 flashes a second, the game's FlashGate), deeper scanlines, a tear, a colour split.",
  entranceGap: 3,
  entranceGap_why: "s: a second entrance inside this is the same entrance (a boss's resume start after its walk-in, a bossRise and a boss38Entrance on one beat): no second shake / flicker / hit-stop.",
  moveShake: 1.5,
  moveShake_why: "A big boss move (the worm's dig and erupt, the Stag's crash, the Loom's fall, the Crown's shatter, the Wick's dive): ULTRAKILL's Minotaur slam / explosion-in-range 1.5 (0.5 s of decay at 3 / s).",
  moveFlicker: 0.2,
  moveFlicker_why: "s: the flicker on a big move (one pulse in 0.2 s: shorter than the entrance's).",
  moves: ["wick37Dive:dive", "stag37Crash", "loom37Down", "crown37Shatter"],
  moves_why: "The big moves by their sim events (event:stage when only one stage is the move: wick37Dive's tell / dive / stall; the rest any stage). The worm's dig and erupt come from its bossTell (wyrmTells) at the tell's seconds.",
  wyrmTells: ["crack", "dive"],
  wyrmTells_why: "THE KILN WYRM's bossTell names for its dig (crack: where it dives into the floor) and its erupt (dive: where it comes out of the wall); the shake lands when the tell resolves (its `seconds` of world time later; the Wyrm is not edited).",
  shakeDecay: 3,
  shakeDecay_why: "Per real second, linear (ULTRAKILL CameraController.cs: 1.0 lasts 0.33 s, 3.0 lasts 1 s).",
  hurtShakePer: 20,
  hurtShakePer_why: "A hit taken shakes damage / this (ULTRAKILL NewMovement.cs: damage / 20 on a 100 HP player; ours is health.max 100 too), between hurtShakeMin and hurtShakeMax.",
  hurtShakeMin: 0.15,
  hurtShakeMin_why: "Every hit shakes at least this (ULTRAKILL's integer division drops hits under 20; GG asked for a hit to be felt: PLAN.md 'a small shake').",
  hurtShakeMax: 0.6,
  hurtShakeMax_why: "A hit never shakes more than this (a small shake: a boss move is 1.5).",
  hurtStaticMin: 0.5,
  hurtStaticMin_why: "The static burst's level on a hit (ULTRAKILL's hurt screen: alpha 0.5, and the i-frames last that many seconds: the overlay IS the timer); it decays at staticDecay per s, so it shows for level / staticDecay s.",
  hurtStaticMax: 0.8,
  hurtStaticMax_why: "The level at hurtHeavyAt damage and above (ULTRAKILL: 0.8 for 50+ damage); between, by the damage's share.",
  hurtHeavyAt: 50,
  hurtHeavyAt_why: "Damage at which the static is hurtStaticMax (ULTRAKILL's 50 on a 100 HP player).",
  staticDecay: 1,
  staticDecay_why: "Per real second, linear (ULTRAKILL's hurt screen alpha falls 1 per second).",
  traumaBoss_why: "With the row on, feel.trauma.boss (the web's 0.5, read by nothing until now) is added to the web's trauma shake on each of feel.trauma.bossEvents (bossSlam, bossPhase, bossDown), as fx/trauma.js meant.",
};

function rowSpan(text, name) {
  const key = '      "' + name + '": {';
  const at = text.indexOf(key);
  if (at < 0) throw new Error("row not found: " + name);
  let depth = 0;
  for (let i = at + key.length - 1; i < text.length; i++) {
    const ch = text[i];
    if (ch === '"') {
      for (i++; i < text.length && text[i] !== '"'; i++) if (text[i] === "\\") i++;
      continue;
    }
    if (ch === "{") depth++;
    else if (ch === "}" && --depth === 0) return [at, i + 1];
  }
  throw new Error("row not closed: " + name);
}

function block(name, obj) {
  return ('      "' + name + '": ' + JSON.stringify(obj, null, 2)).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
}

let out = raw;
for (const [name, obj] of [["hud38", hud38], ["juice38", juice38]]) {
  const [a, b] = rowSpan(out, name);
  out = out.slice(0, a) + block(name, obj) + out.slice(b);
}
const back = JSON.parse(out);
for (const k of Object.keys(cfg.design.n38)) {
  if (k === "hud38" || k === "juice38") continue;
  if (JSON.stringify(back.design.n38[k]) !== JSON.stringify(cfg.design.n38[k])) throw new Error("another row changed: " + k);
}
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("hud38.enabled=" + back.design.n38.hud38.enabled + " juice38.enabled=" + back.design.n38.juice38.enabled + "; juice38 keys: " + Object.keys(back.design.n38.juice38).filter((k) => !k.endsWith("_why")).join(", "));
