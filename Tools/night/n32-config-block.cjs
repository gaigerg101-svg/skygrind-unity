// NIGHT32 shared commit (the lead): adds design.n32 to Design/unity-config.json (every row off), then copy with n24-sync.sh.
// Run: node Tools/night/n32-config-block.cjs   (refuses to run twice)
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
if (d.design.n32) throw new Error("design.n32 already exists");
d.design.n32 = {
  _why: "NIGHT32 (GG 2026-10-10, after playing NIGHT31: 'the bosses have way too much health', 'you can barely hit them', 'you should defeat [them] in like a minute', 'you can just spam the grapple', 'the arenas are so boring', 'the 5th level', 'the 4th level completely overhauled again', 'new arenas on existing levels', 'the first boss fight arena ... needs to be a circle'). Every row is off in the shared commit and turned on only when its contract passes. Off (--off32 / -skygrindOff32, and every older off switch): NIGHT31's game bit for bit. Unity-only numbers each carry a _why (the web never had these systems). Design: Design/levels/DESIGN32.md; research: Research/2026-10-10-wave32/.",
  bosses32: { _why: "Builder B (DESIGN32 B): every boss dies in about a minute for an average player (band 45-75 s, measured by a seeded sweep and the Player); the identity mechanic becomes a big damage bonus, never the only way to deal damage; no zero-damage state longer than 1.5 s; every hit deals at least 25 % chip; hurtboxes at least 1.3x the model. Builder B adds its numbers with their _why.", enabled: false },
  grapple32: { _why: "Builder G (DESIGN32 G, D-1): the grapple has charges (2, each back after a recharge; a minimum gap between pulls; a non-boss kill gives one back); tap-pull and hold-swing both spend one. NIGHT31's pull cooldown was 0.4 s, so it could be chained forever. Builder G adds its numbers with their _why.", enabled: false },
  balance32: { _why: "Builder G (DESIGN32 G2): the combat balance pass outside the bosses (weapon damage per second parity, room enemy health and counts, damage taken, health drops), each change measured by a harness table. Builder G adds its numbers with their _why.", enabled: false },
  arenas32: { _why: "Builder A (DESIGN32 A): boss 1's arena a true circle (64 wall segments; floor, wall and collider), and new or rebuilt arenas on levels 1-3 (smaller, vertical, a hazard edge, an enemy climb route, every room a loop). Builder A adds its numbers with their _why.", enabled: false },
  span32: { _why: "Builder S (DESIGN32 S): level 4 rebuilt from scratch around the limited grapple (small rooms, ring routes that need a charge plan, wall rides between pulls). Builder S adds its numbers with their _why.", enabled: false },
  level5: { _why: "Builder F (DESIGN32 F): level 5, the finale: every verb and all four weapons, remixed rooms, a gauntlet, then the boss arena (a true circle). Builder F adds its numbers with their _why.", enabled: false },
  boss5: { _why: "Builder B (DESIGN32 B5): level 5's boss, built to the 60-second rules from the start. Builder B adds its numbers with their _why.", enabled: false },
  grapplePips: { _why: "View only (G): the grapple's charges on the HUD (two cyan pips beside the reticle, refilling). Unity-only view numbers in UnityTuning.*32.cs.", enabled: false },
  bossMarks: { _why: "View only (B): every boss's damage spot glows cyan-white and its open window is unmistakable (the frames showed the Stag, Wyrm and Loom as dark boxes with no 'hit here').", enabled: false },
  hud32: { _why: "View only (G): the HUD errors seen in the NIGHT31 frames: the round countdown during a boss fight, the cut-off LT label ('CHARGIN'), a lesson line left from another room.", enabled: false },
};
const block = JSON.stringify(d.design.n32, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
fs.writeFileSync(file, head + eol + "    }," + eol + "    \"n32\": " + block + eol + "  }" + eol + "}");
JSON.parse(fs.readFileSync(file, "utf8"));
console.log("design.n32 added: " + Object.keys(d.design.n32).filter((k) => k !== "_why").join(", "));
