// NIGHT40 shared commit (the lead): adds design.n40 to Design/unity-config.json (every row off), then copies the file to
// Assets/StreamingAssets/Design/ (the Player's copy; byte-identical before this run). Refuses to run twice. Builders patch ONLY their
// own row's text afterwards (L-049), with a .cjs script.
// Run: node Tools/night/n40-config-block.cjs
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const copy = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/StreamingAssets/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the block");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
if (d.design.n40) throw new Error("design.n40 already exists");
const off = "Off until the lead has looked at its Player frames once and iterated once (NIGHT40.md method 2).";
d.design.n40 = {
  _why: "NIGHT40, the build night (GG 2026-10-13 ~22:00, verbatim in Research/2026-10-14-wave40/GG-REQUEST.md; NIGHT40.md; DECIDE40.md). Every row is off in the shared commit and turned on only when its check passes and its frames were looked at. Sim rows (start40, kiln40, cut40, charge40, ride40, iceline40, brawl40, loom40, crown40, foes40) are behind N40Blocks; arc40, pump40, rods40 and models40 are view rows (no bit). Off (--off40 / -skygrindOff40, and every older off switch): NIGHT39's game bit for bit. Level rows swap levelFrom for levelTo (Curriculum25.N40Levels); their weapons are data: startSet, awards [{room, weapon}], loom40.dropWeapons (SurvivalSim.N40.cs).",
  start40: { _why: "Builder S1 (NIGHT40.md S1; GG: 'the game needs a short delay or a required movement trigger before the first wave of enemies appears'): the first wave of a level (and after a respawn) waits for movement or a timer, then a tell before enemies can attack. S1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  kiln40: { _why: "Builder S1 (NIGHT40.md S1; GG: the boost 'needs clearer text and better explanation'; DECIDE40 D-2, GG 2026-10-13 ~23:00: 'yes the pistol and gauntlet' = level 3 starts with the shotgun, the pistol and gauntlet unlocked later): level 3 as n40-kiln (from n38-kiln; n38-kiln untouched).", enabled: false, enabled_why: off, levelFrom: "n38-kiln", levelTo: "n40-kiln", startSet: ["shotgun"], startSet_why: "GG's answer (DECIDE40 D-2): the THUNDERCLAP alone at the start; S1 adds `awards` for the revolver and the gauntlet in the zones it picks.", awards: [] },
  cut40: { _why: "Builder S1 (NIGHT40.md S1; GG: 'The sixth level should be deleted entirely, along with any mention of levels beyond it'; DECIDE40 D-4): level 6 out of the campaign order; its files stay until GG's yes (--off40 still loads them).", enabled: false, enabled_why: off, cut: ["n35-finale"] },
  charge40: { _why: "Builder W1 (NIGHT40.md W1; GG: 'charge shots for both weapons should take longer to load'; 'The shotgun also needs a charge shot'; DECIDE40 D-6): longer charges; the THUNDERCLAP's hold charges with no shot first on the ground. Overrides design.n31.charge / n35.charge35 / n39.p39 while on (those rows unchanged). W1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  ride40: { _why: "Builder R2 (NIGHT40.md R2; GG: 'Infinite wall-riding should also be implemented so players don't slowly slide down'; 'Wall-riding currently triggers too easily when climbing random platforms or stairs'; DECIDE40 D-9): every ride holds (ride38's rule), the attach asks for intent. Overrides design.n35.wallride35 / n36.wallgrip36 / n38.ride38 / n39.ride39 while on. R2 adds its numbers with their _why.", enabled: false, enabled_why: off },
  iceline40: { _why: "Builder L2 (NIGHT40.md L2; GG: level 2's rooms 'need a complete overhaul to better support the new gauntlet mechanic'; 'players should not have access to the pistol at the start of level two'): level 2 as n40-iceline (from n37-iceline; n37-iceline untouched).", enabled: false, enabled_why: off, levelFrom: "n37-iceline", levelTo: "n40-iceline", startSet: ["gauntlet"], startSet_why: "GG: no pistol at level 2's start; L2 adds `awards` with the revolver in the armoury zone.", awards: [] },
  brawl40: { _why: "Builder B2 (NIGHT40.md B2; GG: 'The second boss fight should be completely redesigned into a brawler-style encounter'): the brawler on n40-iceline's last room. B2 adds its numbers with their _why.", enabled: false, enabled_why: off },
  loom40: { _why: "Builder B4 (NIGHT40.md B4; GG: 'The STITCHER weapon should be removed from the fourth level entirely'; THE LOOM 'often runs out of the map'; no-ride zones, more in phase 2; DECIDE40 D-7, D-12): level 4 as n40-span (from n38-span; n38-span untouched), the tear by the grapple, the STITCHER out of the campaign.", enabled: false, enabled_why: off, levelFrom: "n38-span", levelTo: "n40-span", dropWeapons: ["nailgun"], dropWeapons_why: "D-7: the STITCHER leaves every campaign start set and award while loom40 is on (survival keeps it); tied to loom40 so THE LOOM never needs nails nobody has." },
  crown40: { _why: "Builder B5 (NIGHT40.md B5; GG: 'players have no idea what to do, and even I don't know how to damage it'): level 5 as n40-spire (from n38-spire; n38-spire untouched), THE CROWN taught. B5 adds its numbers with their _why.", enabled: false, enabled_why: off, levelFrom: "n38-spire", levelTo: "n40-spire" },
  foes40: { _why: "Builder E3 (NIGHT40.md E3; GG: 'a few more enemy types, including ones that fire projectiles you can deflect'; DECIDE40 D-11): the ported DRONE, WARDEN and CHARGER in the campaign, retuned (their survival numbers unchanged). E3 adds its numbers with their _why.", enabled: false, enabled_why: off },
  arc40: { _why: "Builder H1 (NIGHT40.md H1; GG: 'using a single curved bar under the reticle instead of three small bars'): a view row (no sim bit); numbers in UnityTuning.Hud40.cs.", enabled: false, enabled_why: off },
  pump40: { _why: "Builder W1 (NIGHT40.md W1; GG: 'The shotgun's firing animation should also be sped up'): a view row (no sim bit); numbers in UnityTuning.Pump40.cs.", enabled: false, enabled_why: off },
  rods40: { _why: "Builder S1 (NIGHT40.md S1; GG: 'Many arenas contain awkwardly placed white rods that you clip through'): a view filter (no sim bit, no level data edited).", enabled: false, enabled_why: off },
  models40: { _why: "Builder E3 (NIGHT40.md E3; GG: 'Enemy models should be more detailed with clearer silhouettes'): a view row (no sim bit); numbers in UnityTuning.Models40.cs.", enabled: false, enabled_why: off },
};
const block = JSON.stringify(d.design.n40, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
fs.writeFileSync(file, head + eol + "    }," + eol + "    \"n40\": " + block + eol + "  }" + eol + "}");
JSON.parse(fs.readFileSync(file, "utf8"));
fs.copyFileSync(file, copy);
console.log("design.n40 added: " + Object.keys(d.design.n40).filter((k) => k !== "_why").join(", "));
