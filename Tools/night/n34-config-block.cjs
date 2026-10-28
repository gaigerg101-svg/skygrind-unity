// NIGHT34 shared commit (the lead): adds design.n34 to Design/unity-config.json (every row off), then copy with n24-sync.sh.
// Run: node Tools/night/n34-config-block.cjs   (refuses to run twice)
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
if (d.design.n34) throw new Error("design.n34 already exists");
d.design.n34 = {
  _why: "NIGHT34 (GG 2026-10-11, after playing n33: the grapple triggers 'a dramatic slow-motion effect with a clear visual overlay' so players 'switch weapons in real time and chain fluid, expressive combos ... as the world bends around you'; remove 'that little diamond indicator'; 'an ULTRAKILL-style weapon display that shows your full arsenal'; the game is 'cluttered with detail for the sake of detail' and 'the arenas don't incentivize creative movement'; the Pulse Gauntlet 'a slightly larger active hitbox when striking bosses like the worm'; movement, UI, responsiveness and flow 'dramatically better'). Every row is off in the shared commit and turned on only when its check passes. Off (--off34 / -skygrindOff34, and every older off switch): NIGHT33's game bit for bit. Unity-only numbers each carry a _why naming their source (NIGHT34.md method 1: reference first). Plan: NIGHT34.md; defaults: DECIDE34.md; research: Research/2026-10-11-wave34/.",
  bend34: { _why: "Builder B (NIGHT34.md B1): the grapple BEND. Every latch (node, wall, the whip on an enemy) gives a SHORT BEND; a full BEND METER gives the FULL BEND (kills refill it, never time). Inside a bend the weapon swap is instant. split (bendSplit34): the world steps 1 in N while the player steps every step; off: the global path (the whole sim eased, Bend34Share). The web has no slow motion of its own (its killSlowMo is a view effect). Builder B adds its numbers with their _why.", enabled: false, split: false },
  bendLook: { _why: "View only (builder H, NIGHT34.md H1): the bend's post grade (desaturate toward a cool grey, red-orange and cyan kept by hue, the sky dimmed, a step deeper per kill), the entry FOV thump, the meter's slash. Unity-only view numbers in UnityTuning.Bend34.cs.", enabled: false },
  rail34: { _why: "View only (builder H): the ARSENAL RAIL bottom right: every owned weapon a slot, the hand larger and cyan with its LT verb, the highlight slides on a swap, a holstered gun turning ready pulses once. Unity-only view numbers in UnityTuning.Hud34.cs.", enabled: false },
  nomark34: { _why: "View only (builder H): Grapple31View's target diamond is not drawn (GG: 'Remove that little diamond indicator ... it breaks immersion and isn't needed'); the node ring stays.", enabled: false },
  lines34: { _why: "View and flow (builder A, NIGHT34.md A1): fight rooms rebuilt around movement lines (a wall-run wall, a grapple node and a bhop lane that chain, two or three floors, a climb route per floor), spacings from SKYGRIND's own measured jump / dash / ride / grapple distances. While on, the campaign plays each rebuilt level's n34 file (levelSwap, applied after NIGHT33's swap in Curriculum25.N34Levels; a level not in the map keeps its n33 file).", enabled: false, levelSwap: {} },
  calm34: { _why: "View only (builder A): dressing props with no collider and no purpose are not placed (at least 40 % fewer per level, counted); cover, climbs, devices and fixtures that light a room stay.", enabled: false },
  flow34: { _why: "Builder M (NIGHT34.md M1): movement patterns from RESEARCH.md table M that SKYGRIND lacks, each its own sub-switch, in Mover.cs branches only (with the row off every golden vector and trajectory is bit-exact). Builder M adds its numbers with their _why.", enabled: false },
  reach34: { _why: "Builder M (NIGHT34.md M1): the Pulse Gauntlet's punch and blast reach a boss with bossMargin m added to the boss body's radius (inflate the enemy's hurtbox, keep the player's: RESEARCH.md finding 7). The web has no boss margin. Builder M adds its numbers with their _why.", enabled: false },
};
const block = JSON.stringify(d.design.n34, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
fs.writeFileSync(file, head + eol + "    }," + eol + "    \"n34\": " + block + eol + "  }" + eol + "}");
JSON.parse(fs.readFileSync(file, "utf8"));
console.log("design.n34 added: " + Object.keys(d.design.n34).filter((k) => k !== "_why").join(", "));
