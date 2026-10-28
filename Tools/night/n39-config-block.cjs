// NIGHT39 shared commit (the lead): adds design.n39 to Design/unity-config.json (every row off), then copies the file to
// Assets/StreamingAssets/Design/ (the Player's copy; byte-identical before this run). Refuses to run twice. Builders patch ONLY their
// own row's text afterwards (L-049), with a .cjs script.
// Run: node Tools/night/n39-config-block.cjs
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const copy = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/StreamingAssets/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the block");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
if (d.design.n39) throw new Error("design.n39 already exists");
d.design.n39 = {
  _why: "NIGHT39, the feel night (GG 2026-10-13 ~14:05, verbatim in Research/2026-10-14-wave39/GG-REQUEST.md; NIGHT39.md; DECIDE39.md). Every row is off in the shared commit and turned on only when its check passes. Sim rows (p39, ride39, boss39) are behind N39Blocks; slow39 is loop pacing the sim never sees (no bit). Off (--off39 / -skygrindOff39, and every older off switch): NIGHT38's game bit for bit.",
  p39: { _why: "Builder P1 (NIGHT39.md P1; GG: 'if I'm holding it down it should intelligently prioritize the charge shot instead of firing the fast shot first'): the KESTREL's tap fires on a quick release, a held press charges with no tap first (tap36's fire-on-press with the row off; the THUNDERCLAP keeps tap36). Builder P1 adds its numbers with their _why (in this row only).", enabled: false },
  ride39: { _why: "Builder R1 (NIGHT39.md R1; GG: 'the wall riding speed is a little too fast—you gain speed too quickly—and the jump height should be increased slightly'): overrides of design.n35.wallride35's speedBuff / exitBoost / chainCap / upLow / upHigh while on (wallride35 itself unchanged, so --off39 gives NIGHT38's ride). Builder R1 adds its numbers with their _why.", enabled: false },
  boss39: { _why: "Builder B1 (NIGHT39.md B1; DECIDE39 D-1; GG: 'all bosses need more balanced health bars so each fight lasts around one to two minutes, maybe three if you're slow'): per-boss overrides (health, stagger meter, window, budget, the worm's pool mult) while on (design.n37.* and the worm's rows unchanged). Builder B1 adds its numbers with their _why.", enabled: false },
  slow39: { _why: "Builder F1 (NIGHT39.md F1; GG: 'when you're wall riding and zooming in ... it needs to be extremely slow every time'; 'if you grapple onto an enemy the game should slow down with the same effect'; the parry 'emphasized more and slowed down'): loop pacing (the sim still steps at 120 Hz; tape and fingerprint unaffected), so no N39Blocks bit; off = slowAim27's NIGHT38 behaviour. Builder F1 adds its numbers with their _why.", enabled: false },
};
const block = JSON.stringify(d.design.n39, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
fs.writeFileSync(file, head + eol + "    }," + eol + "    \"n39\": " + block + eol + "  }" + eol + "}");
JSON.parse(fs.readFileSync(file, "utf8"));
fs.copyFileSync(file, copy);
console.log("design.n39 added: " + Object.keys(d.design.n39).filter((k) => k !== "_why").join(", "));
