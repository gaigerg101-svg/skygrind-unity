// NIGHT37 shared commit (the lead): adds design.n37 to Design/unity-config.json (every row off), then copy it to StreamingAssets.
// Run: node Tools/night/n37-config-block.cjs   (refuses to run twice). Builders patch ONLY their own row's text afterwards (L-049).
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
if (d.design.n37) throw new Error("design.n37 already exists");
d.design.n37 = {
  _why: "NIGHT37, the boss night: every boss but THE KILN WYRM restarted from scratch as a moving fight (GG 2026-10-12 ~19:25, verbatim in Research/2026-10-12-wave37/GG-REQUEST.md: 'I want all the bosses completely overhauled and restart from scratch except the worm boss ... it should be a consistently moving ... battle ... let's not do it traditional'). Design: Research/2026-10-12-wave37/BOSSES.md; decisions: DECIDE37.md (D-2 all four tonight). Every row is off in the shared commit and turned on only when its check passes. Off (--off37 / -skygrindOff37, and every older off switch): NIGHT36's game bit for bit. A boss row's levelFrom -> levelTo swaps its level in the campaign order while the row is on (Curriculum25.N37Levels).",
  boss37: { _why: "Builder K (NIGHT37.md K1; BOSSES.md shared rules): the Boss37 brain the four bosses share: legs with checkpoints, the stagger meter + the CYAN window (ULTRAKILL Minos Prime's stamina), the attack budget with a forced rest and the parry refund (MinosPrime.cs:342-353, :1125), the weighted pick with pity (Gabriel.cs:74-128), the habit reader (Minotaur.cs:294-296), the anti-hide timer (GabrielBase.cs:355-363); research/uk-bosses.md. Builder K adds its numbers with their _why.", enabled: false },
  wick37: { _why: "Builder W (NIGHT37.md W1; BOSSES.md 'L1 THE WICK'): level 1's boss replaced by THE WICK, a climb up a 40 m furnace stack away from rising slag; it is only hurt from the walls. Builder W adds its numbers with their _why.", enabled: false, levelFrom: "n36-foundry", levelTo: "n37-foundry" },
  stag37: { _why: "Builder S (NIGHT37.md S1; BOSSES.md 'L2 THE STAG'): THE STAG from scratch as a chase around a frozen canal loop: the slipstream, lane changes, catching it to punch its legs, the head-on parry. Level 6's old Stag is untouched. Builder S adds its numbers with their _why.", enabled: false, levelFrom: "n35-iceline", levelTo: "n37-iceline" },
  loom37: { _why: "Builder O (NIGHT37.md O1; BOSSES.md 'L4 THE LOOM'): THE LOOM from scratch as a canyon crossing with no floor: the grapple is the footing, nails in its knees torn by pulling away. Builder O adds its numbers with their _why.", enabled: false, levelFrom: "n33-span", levelTo: "n37-span" },
  crown37: { _why: "Builder C (NIGHT37.md C1; BOSSES.md 'L5 THE CROWN'): THE CROWN from scratch as a descent of a falling spire: orbiting platforms, jump-dashing through its ring to burn its gems. Builder C adds its numbers with their _why.", enabled: false, levelFrom: "n33-spire", levelTo: "n37-spire" },
};
const block = JSON.stringify(d.design.n37, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
fs.writeFileSync(file, head + eol + "    }," + eol + "    \"n37\": " + block + eol + "  }" + eol + "}");
JSON.parse(fs.readFileSync(file, "utf8"));
console.log("design.n37 added: " + Object.keys(d.design.n37).filter((k) => k !== "_why").join(", "));
