// NIGHT30 shared commit (the lead): adds design.n30 to Design/unity-config.json (every row off), then copy with n24-sync.sh.
// Run: node Tools/night/n30-config-block.cjs   (refuses to run twice; checks that a parse / stringify round trip keeps the file's bytes)
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = "\r\n    }\r\n  }\r\n}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
if (d.design.n30) throw new Error("design.n30 already exists");
d.design.n30 = {
  _why: "NIGHT30 (GG 2026-10-09, after his playtest of NIGHT29: clear weapon rules and a HUD that shows them, lava where pits lie, a circular Lantern that explains itself, glide ice and a level-2 boss that punishes distance, a Wyrm head-punch stagger, LEVEL 4, a gritty look with a HUD that buzzes). Every row is off in the shared commit and turned on only when its contract passes. Off (--off30 / -skygrindOff30, and every older off switch): NIGHT29's game bit for bit. Unity-only numbers each carry a _why (the web never had these systems).",
  levels: { _why: "The rebuilt levels 1-4 (plans Design/levels/n30/*.plan.json, ids n30-*): while on, design.n25.curriculum.order points at them; off: the n29-* order. A view / flow row (no sim bit: the level a run loads is already on its tape).", enabled: false },
  rules: { _why: "Item 1, the weapon rules (DESIGN30 section A): one action at a time; a switch waits only for the ACTION in progress (a gun's shot cycle, the punch's swing), never for the whole cooldown; every cooldown (the guns, the gauntlet's heavy swing and its LT blast) runs while holstered. actionSeconds caps how long any action holds a switch (the punch's own punch.cycleTime 0.35 is the model; the web has no switch rule).", enabled: false, actionSeconds: 0.35 },
  glide: { _why: "Item 6a, ICE as a floor tag (R3: ULTRAKILL's gasoline floor; a slide keeps its entry speed and decays slowly): on an ice surface braking is very low, a dash ADDS speed up to a soft cap, a downhill adds speed, steering stays responsive. The numbers are builder S's first picks, replaced by a measured glide line (the web's ice only scales friction).", enabled: false },
  lava: { _why: "Item 3, LAVA not pits (R1: ULTRAKILL's lava burns without a teleport; D-6): a touch costs damageFraction of max health, knocks the player up by knockUp m/s, flashes hurtFlashSeconds, and cannot hurt again for graceSeconds; NO respawn (only a void fall respawns); an enemy that falls in burns and dies. The web has no lava.", enabled: false, damageFraction: 0.25, knockUp: 9, hurtFlashSeconds: 0.6, graceSeconds: 0.5 },
  lantern30: { _why: "Item 4, THE LANTERN on a ring of flat panels (DESIGN30 C1 + the reviewer's fix: 16 panels of 22.5 degrees, the Mover's ride turns about 30 degrees at most): the orbs lead the rider, a sweep marks a panel ahead of the rider, platform shots do no damage, two lines teach the rule. Builder B adds its numbers with their _why.", enabled: false },
  glacier30: { _why: "Item 6, THE GLACIER rebuilt (DESIGN30 C2): a closed ice arena two to three times the rink, glide instead of snap lanes (SkateLane29 off on this path), distance punished by homing shards and a lane sweep. Builder B adds its numbers with their _why.", enabled: false },
  wyrm30: { _why: "Item 7, the KILN WYRM head-punch stagger (D-8): a gauntlet hit on the head in the last windowSeconds of a charge: hitstopSteps of hitstop, bonusMult damage, a recoilSeconds recoil, then a different attack. The web's worm has no stagger.", enabled: false, windowSeconds: 0.4, hitstopSteps: 6, bonusMult: 2.0, recoilSeconds: 1.2 },
  span: { _why: "Item 10, LEVEL 4 THE SPAN (D-3): the ARCLINE grapple in a campaign level (grapple nodes as plan rows; the swing keeps design.n21.arcline's 0.4 s, only the yank its 15 s), the NAILGUN award, THE LOOM. Builder S / B add their numbers with their _why.", enabled: false },
  teach: { _why: "Item 12 + P0 b / d: the lesson line clears when its room is left or cleared and never shows in a boss fight; a boss explains its rule in at most two lines on the first occurrence of its tell, each at most maxShows times per fight. The web has no lessons.", enabled: false, maxShows: 2 },
  hud30: { _why: "View only (item 2 + P0 c / e / f): the weapon-state widget (READY / COOLING / CHARGING with a ring and a word), run rows hidden on TITLE / PAUSE / RESULTS, the layout pass. Unity-only view numbers live in UnityTuning.*30.cs.", enabled: false },
  look30: { _why: "View only (item 8, D-4): HUD scanlines / flicker / glow / buzz (an accessibility off switch), the world quantise + Bayer dither (subtle), pixel block off by default, the Lantern's full-frame wash removed. Unity-only view numbers live in UnityTuning.*30.cs.", enabled: false },
};
// text insertion (the file is not JSON.stringify's layout): the block goes in as the last key of `design`, indented 4
const block = JSON.stringify(d.design.n30, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
fs.writeFileSync(file, head + eol + "    }," + eol + "    \"n30\": " + block + eol + "  }" + eol + "}");
JSON.parse(fs.readFileSync(file, "utf8"));
console.log("design.n30 added: " + Object.keys(d.design.n30).filter((k) => k !== "_why").join(", "));
