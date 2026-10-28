// NIGHT31 shared commit (the lead): adds design.n31 to Design/unity-config.json (every row off), then copy with n24-sync.sh.
// Run: node Tools/night/n31-config-block.cjs   (refuses to run twice)
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
if (d.design.n31) throw new Error("design.n31 already exists");
d.design.n31 = {
  _why: "NIGHT31 (GG 2026-10-10, after his playtest of NIGHT30: 'not a fan of a lot of the bosses', the grapple 'doesn't pull you in', the level-2 boss 'boring, you can camp', the charge shot gone, LT must do something on every weapon, level 1 the pistol only and one weapon per level). Every row is off in the shared commit and turned on only when its contract passes. Off (--off31 / -skygrindOff31, and every older off switch): NIGHT30's game bit for bit. Unity-only numbers each carry a _why (the web never had these systems). Design: Design/levels/DESIGN31.md.",
  grapplePull: { _why: "Item 1 (DESIGN31 B, D-3): a TAP of the grapple pulls you in (Arcline21's zip on a node or surface: straight at it, rope taut, released at the web's arcline.zip.arrive with the speed kept plus an upward pop); a HOLD is NIGHT30's swing. The web's grapple pulls only toward a hooked big enemy. Builder G adds its numbers with their _why.", enabled: false },
  charge: { _why: "Item 2a (DESIGN31 A, D-4): the charge shot (design.n27.chargeShots27's rules) back on the KESTREL (pierces) and the THUNDERCLAP (slug) with re-picked charge and cooldown times (R3: ULTRAKILL's piercer about 0.5 s charge, 2.5 s cooldown; n27 had 10 s / 14 s). Builder W adds its numbers with their _why.", enabled: false },
  ltAlways: { _why: "Item 2b (DESIGN31 A, D-8): every weapon's LT does something at every moment: the gauntlet's LT before its five pips is a parry that returns a projectile (no crowd control: no 15 s cooldown needed). Builder W adds its numbers with their _why.", enabled: false },
  unlocks: { _why: "Item 3 (DESIGN31 A, D-1 as the lead changed it): level 1 the KESTREL only; level 2 awards the PULSE gauntlet (room i1); level 3 the THUNDERCLAP; level 4 the NAILGUN; the fist is always there. Off: NIGHT30's start (KESTREL + gauntlet).", enabled: false },
  lantern31: { _why: "Item 6a (DESIGN31 C1): the Lantern's shield breaks only to a charged KESTREL shot from a wall panel; platforms sink at half health. Builder B adds its numbers with their _why.", enabled: false },
  stag31: { _why: "Item 6b (DESIGN31 C2, D-2): THE STAG replaces the Glacier as level 2's boss (a charging ice ram: glide sideways, punch its open back). Builder B adds its numbers with their _why.", enabled: false },
  wyrm31: { _why: "Item 6c (DESIGN31 C3): the Wyrm's feints, gun damage only while staggered, a far-range spit, crumbling platforms. Builder B adds its numbers with their _why.", enabled: false },
  loom31: { _why: "Item 6d (DESIGN31 C4): the Loom's joints are pull-in targets, a deck camper calls a strike, the deck's middle breaks at half health. Builder B adds its numbers with their _why.", enabled: false },
  grappleMark: { _why: "View only (item 1b): the cyan diamond on the target the grapple would take this frame; the FOV kick and the retracting rope during a pull. Unity-only view numbers in UnityTuning.*31.cs.", enabled: false },
  lowHealthEdge: { _why: "View only (P0 d, D-6): the low-health red only at the screen edges (PostFx + the two Ui.cs vignettes), GG asked that effects not touch the whole screen.", enabled: false },
  arcRing: { _why: "View only (P0 b): the old Arcline21View cubes hidden in campaign maps so NIGHT30's GrappleNodes30 ring shows.", enabled: false },
};
const block = JSON.stringify(d.design.n31, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
fs.writeFileSync(file, head + eol + "    }," + eol + "    \"n31\": " + block + eol + "  }" + eol + "}");
JSON.parse(fs.readFileSync(file, "utf8"));
console.log("design.n31 added: " + Object.keys(d.design.n31).filter((k) => k !== "_why").join(", "));
