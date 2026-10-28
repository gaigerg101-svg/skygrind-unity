// NIGHT47 builder L: writes builder L's rows' numbers (design.n47.loom47.numbers, design.n47.crown47.numbers) into both config copies
// (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json, byte-identical). Touches ONLY those rows' "numbers" objects;
// throws if any other part of the config would change. Re-runnable (it replaces the numbers object each time). `--dry` prints, writes nothing.
// Usage: node Tools/night/n47-l-numbers.cjs [--dry]
"use strict";
const fs = require("fs");
const path = require("path");
const ROOT = path.resolve(__dirname, "..", "..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"].map((f) => path.join(ROOT, f));
const dry = process.argv.includes("--dry");

const W = (v, why) => [v, why];
const NUMBERS = {
  loom47: {
    chipScale: W(0.3, "x every hit outside the CYAN window (was design.n37.loom37.kit.chipScale 0.02: GG 'you can barely damage it even while firing consistently'; NIGHT47.md section 3: chip >= 30 % of full). Measured before: chip was 2 % of the dealt (n47loom base)."),
    windowS: W(4, "s the CYAN window stays open (was kit.windowSeconds 2.2): DESIGN-RULES B-2 / Birkhead (Research/2026-10-18-wave47/L.md): a punish window longer than a combo plus the travel to the spinneret."),
    coreDropM: W(4, "m the body sinks toward the player while the window is open (the spinneret comes to you: the whip / the guns reach it from a post)."),
    coreDropSpeed: W(8, "m/s of that sink (0.5 s for 4 m: a move the eye follows)."),
    seamDamage: W(60, "gun damage into a latched knee's seam while the player is inside a BEND that tears it (PortExport weapons: the KESTREL tap 25, the THUNDERCLAP 8 x 8: three taps or one close blast inside a 2.5 s bend). The pull-away tear (VERBS 46) is unchanged."),
    bendOnTear: W(0.5, "BEND meter (0..1, design.n34.bend34) given back by a tear: the right move pays the slow motion (Witch Time's rule, L.md); two right moves = a FULL bend on the next hook. Today the meter refills only on kills and THE LOOM's fight has none."),
    bendOnParry: W(0.5, "BEND meter given back by a parried spool (the YELLOW right move), as bendOnTear."),
    bendAtPhase: W(1, "the BEND meter is at least this at each phase's start (boss37Leg fight / start): the first hook of every phase is a FULL bend."),
    fallHp: W(5, "health a fall into THE LOOM's void costs while loom47's fight runs (was design.n37.loom37.fallCostHp 20; A-4 'a little health'; n46-span's fight-room lip returns cost 5). MEASURED round 1 (n47lhuman, n47-span, 40 seeds, at 20): 25.5 falls a fight, taken median 399, 93 deaths, p90 192 s: the human model's late rope moves fall, and each fall took a fifth of the bar."),
    latchR: W(2.2, "m: a hook whose anchor lands within this of a lit knee latches it (was the knee's hit radius design.n37.loom37.kneeRadius 1.3; the knees bob 0.8 m and glide): the game's forgiveness for a hook thrown at a moving knee (the lead's round-2 note: fix it in the game, not the pilot)."),
    ruleLineDelayS: W(3, "s after the fight's begin THE LOOM's rule line ('HOOK A LIT KNEE ...', loom40's) is said while loom47 runs (was at once): the lead's n47-r0d frame showed the door's boss-name banner printed over it; NIGHT47 section 3: one message at a time."),
    finaleS: W(0.35, "s THE FINALE BEAT: the sim's hitstop the step THE LOOM starts to fall (the card's kill beat; NIGHT47 section 6's R4 list 'a boss finale beat: a short slow motion on the last hit'), once a fight; past C-2's top tier (0.28 s) because it ends the fight."),
    netY: W(1, "(round 4b: -3 measured a throw cut at y -1.8 within 0.07 s by a surface at the floor plane y 0 over the void, so the net sits over it) m: THE WARP NET's height (THE LOOM's own threads under the frame; the holds' tops are at 6, the start ledge at 0, the fall line design.n37.loom37.fallY -6): a player falling through it in the room is caught and thrown back up (the lead's round-4 ask 1: ~21 falls a fight for the human-model pilot)."),
    netUp: W(24, "m/s up the net throws a caught player (about the holds' height from the net, with the BEND's slow motion to hook)."),
    netSide: W(6, "m/s of the throw toward the nearest live knee of the phase (the hook to make next)."),
    netApexAbove: W(2, "m the net's throw rises over the higher of the net and the last hold's top: the throw is ballistic and LANDS on that hold (round 4b; a throw toward the nearest knee, netUp 24 / netSide 6, measured 67.6 catches and 33.6 falls a fight: it landed nowhere). netUp / netSide are used only when no hold is known."),
    netHp: W(2, "health a catch costs (A-4: a little health; a lip return costs fallHp 5)."),
    netChain: W(2, "catches in a row without standing on a hold; the next fall goes through to the lip return (no endless bouncing)."),
    diveWindup: W(1.3, "s SHUTTLE DIVE's tell (the nose lamps, the lit line to your perch): inside the 1.0-1.5 s reaction band (Birkhead, L.md)."),
    diveActive: W(1.0, "s the dive's lunge from the body's place to your marked perch (eased)."),
    diveStick: W(1.5, "s it sticks nose-down at the perch (its recover): the punish, its low knees in reach."),
    diveDamage: W(35, "damage of a dive that reaches you (the lash is 30: the dive is the bigger, slower read)."),
    diveRadius: W(3.5, "m from the nose (the body's underside) to your chest that counts as a hit while it lunges."),
    diveKnock: W(9, "m/s flat knock away from the body on a hit (off the perch: a fall is a lip return)."),
    diveNoseY: W(3, "m over your feet the body's underside stops (it sticks over the perch, not in it)."),
    diveWeight: W(1, "the brain's pick weight (Boss37 PICK)."),
    diveCost: W(2, "the brain's budget cost (Boss37 BUDGET), as the lash's."),
    diveLegs: W([2, 3], "phases (legs) it is in: P2 THE WEAVE and P3 THE UNRAVEL (L-cards.md section 1)."),
    cageWindup: W(1.5, "s WARP CAGE's tell (four threads rise round you and tighten)."),
    cageActive: W(0.3, "s the snap."),
    cageRecover: W(0.8, "s after the snap."),
    cageDamage: W(25, "damage when the cage snaps on you (inside the ring, no rope attached)."),
    cageRadius: W(5, "m the ring's radius round where you stood at the tell's start (a dash 6 m or a hook out)."),
    cageHalfH: W(3, "m over / under the ring's middle that is still inside it (a jump-dash up and out escapes)."),
    cageWeight: W(0.9, "the brain's pick weight."),
    cageCost: W(2, "the brain's budget cost."),
    cageLegs: W([2, 3], "phases it is in: P2 and P3."),
    bobWindup: W(1.2, "s BOBBIN RAIN's tell (the drum shakes)."),
    bobActive: W(1.0, "s the drop's active time (the three spools fall)."),
    bobRecover: W(0.6, "s after."),
    bobDamage: W(20, "damage of a bobbin that lands on you."),
    bobSpacing: W(4, "m between the three bobbins, across your path."),
    bobHeight: W(9, "m over your feet they drop from."),
    bobSpeed: W(14, "m/s they fall (0.64 s from 9 m: the tell plus the fall is ~1.8 s to read the yellow one)."),
    bobLead: W(0.5, "s of your velocity the line leads you by."),
    bobWeight: W(2.5, "the brain's pick weight (1.1 measured round 1: 0 bobbins in 10 fights; phase 3 is short and its spool is forced for the owed parry, so the remix's new read needs the weight)."),
    bobCost: W(2, "the brain's budget cost."),
    bobLegs: W([3], "phases it is in: P3 only (the remix's new read)."),
  },
  kneeHold47: {
    holdS: W(0.5, "s of a BEND's start THE LOOM holds still (body, knees, the dive): the lead's round-2 note ('the knee holding still for the first 0.5 s of a bend'); Celeste's forgiveness windows are ~0.1 s at full speed, a BEND runs the world at 0.25 (design.n34.bend34.scale), so 0.5 s of player time is ~0.125 s of world time held."),
  },
  crown47: {
    chipScale: W(0.3, "x a gun hit on the gems outside the CYAN window (was 0: the view's BLOCKED; NIGHT47.md section 3: chip >= 30 % of full, 'every gun counts')."),
    torchChip: W(0.6, "x a TORCH hit outside the window (level 5's weapon is the close answer: R-1's 3 s verb)."),
    chipFloor: W(0.1, "share of a gem's hp the chip stops at: the chip lands and shows on the bar, but only a pass's window, a parry or the finisher breaks a gem. MEASURED round 1 without it: chip was 74 % of the damage dealt and the human-model pilot's kill 45.9 s (band 90-150)."),
    stagger: W([7, 8, 8], "the stagger meter per phase ([6, 7, 7] measured round 2 at 90.3 s median with bladeWindup 1.4, [6, 7, 8] 90.2 s: on the band's edge; [6, 6, 6] measured 85.9 s median with bladeDamage 15) (was design.n37.crown37.kit.stagger [4, 4, 4]; a pass drains passStagger 2, a parry parryStagger 2): three passes (or two and a parry) open a window. MEASURED round 1 at [4, 4, 4] with passGems 0 / chipFloor 0.1: the human-model pilot's median 79.1 s, p90 93.3 (the band wants 90-150); the window's gem cap was not the limit (windowGems 1 or 2: the same)."),
    bladeDamage: W(15, "BLADE RING's damage (was design.n37.crown37.attacks.bladeRing.damage 25): MEASURED round 1 (n47lhuman, 40 seeds, the longer fight): blade 172 hits = ~4300 of ~6800 damage taken, 49 deaths; a 2-3 minute fight takes more hits, so each costs less."),
    bladeWindup: W(1.4, "s BLADE RING's tell (was 1.2): MEASURED round 1 (n47lhuman crown, 40 seeds): blade 183 hits of the ~5400 damage taken's ~2700, 38 deaths in 40 fights (8 before the row): the killer attack; the human model reads a tell 250 ms late, so the cut at chest height needs the longer wind-up (inside the 1.0-1.5 s band)."),
    slamSpeed: W(20, "m/s downward at which a crossing of the ring's plane counts as a pass even with no flat speed: a slam (VERBS 7; PortExport slam.speed 48) or a long drop THROUGH it (the card's P2: 'you pass it by DROPPING through it')."),
    slamGems: W(0, "gems a slam / drop pass breaks: 0, as a plain pass (it cracks the ring, the window breaks the gems). MEASURED round 2 at 1: the human-model pilot's median 88.2 s (band 90-150): the drops through the ring in the shaft are frequent."),
    finaleS: W(0.35, "s THE FINALE BEAT: the sim's hitstop the step THE CROWN starts to die (the finisher through the opening, or its last gem), once a fight (as loom47's)."),
    volleyDeg: W(14, "deg SHARD VOLLEY's two RED shards fan out either side of the YELLOW one (phase 3: GEM SHARD remixed; dodge the red, punch the yellow back)."),
    passGems: W(0, "gems a pass breaks (was design.n37.crown37.crown.passGems 1): the pass CRACKS the ring (its stagger drain, the window), the window's guns break the gems (Doom's 'the move opens, the guns punish')."),
    torchPassGems: W(1, "gems a pass with the TORCH burning breaks (was torchGems 2): the TORCH keeps its bonus."),
    windowS: W(3.5, "s the CYAN window after the stagger empties (was kit.windowSeconds): a burst plus the travel (B-2)."),
    windowGems: W(1, "gems gunfire can break in one window (measured round 1: 2 gave the human-model pilot a 78.9 s median, p90 93.3): the length comes from passes (VERBS 8), never HP. Measured before: the human-model pilot killed THE CROWN in a median 43.6 s (n47lhuman, n46-spire)."),
    fallHp: W(3, "health a fall costs (5 measured round 2: 6.2 caught falls a fight = ~30 of the ~97 taken, 19 deaths in 40); the fall is CAUGHT (the leg's checkpoint, the leg's pieces back, the brain not restarted). Measured before: the casual pilot fell 103 times in 10 fights (n47crown base)."),
    shardWindup: W(1.2, "s GEM SHARD's tell (was 1.0: the 1.0-1.5 s band's middle, Birkhead)."),
    fireWindup: W(1.3, "s CROWN FIRE's tell: the clapper heats, the floor line shows the sweep."),
    sweepS: W(1.5, "s the flame's sweep."),
    ventS: W(1.5, "s the vent after it (its recover): the ring hangs still: the easiest pass (the punish)."),
    arcDeg: W(90, "deg the cone sweeps across the player's side."),
    halfDeg: W(14, "deg the cone's half width."),
    reachM: W(9, "m the flame reaches (flat) from the ring."),
    overM: W(0.5, "m over the ring's centre the feet must be to be above the flame (the answer: get over it, through the ring)."),
    tickS: W(0.25, "s between two damage ticks."),
    tickDmg: W(6, "damage a tick (8 measured round 1: the human-model pilot died 31 times in 40 fights, 8 before the row)."),
    burnS: W(1.5, "s of the burn state after a tick (the view)."),
    fireWeight: W(1.1, "the brain's pick weight."),
    fireCost: W(2, "the brain's budget cost."),
    fireLegs: W([1, 2, 3], "phases it is in: P1 THE SUMMIT and P3 THE PLAZA as CROWN FIRE, P2 THE SHAFT as FIRE SPIRAL (spiralArcDeg: L-cards.md section 2)."),
    spiralArcDeg: W(360, "deg FIRE SPIRAL sweeps in phase 2 (CROWN FIRE's shaft form: a full turn round the core over sweepS; the answer is to be over the ring or out of reach, the floor line shows the whole turn)."),
    tollWindup: W(1.3, "s BELL TOLL's tell: the ring stops and swings back."),
    tollSpeed: W(14, "m/s the wave grows."),
    tollMaxR: W(30, "m the wave's last radius (its active time = tollMaxR / tollSpeed)."),
    tollBand: W(1.0, "m either side of the wave's front that hits."),
    tollDamage: W(10, "(15 measured round 2: 56 hits in 131 tolls, the human model hops late) damage of the wave on a grounded player (20 measured round 1 with the deaths above)."),
    tollPush: W(6, "m/s push away from the ring on a hit."),
    tollRecover: W(1.5, "s after the toll: the ring hangs (a pass)."),
    tollWeight: W(1, "the brain's pick weight (a ground habit attack: Boss37 HABIT)."),
    tollCost: W(2, "the brain's budget cost."),
    tollLegs: W([2, 3], "phases it is in: P2 THE SHAFT and P3."),
  },
};

// builder L's own NEW rows (the lead's round-4 OK: "your second R4 idea ... its own row"): created after the crown47 row when missing, all off
const NEW_ROWS = {
  kneeHold47: {
    sim: true,
    _why: "Builder L's R4 idea (sim row; the lead's round-2 note 'the knee holding still for the first 0.5 s of a bend', Celeste's forgiveness: coyote / buffer windows, Research/2026-10-18-wave47/L.md): while a BEND's first holdS runs, THE LOOM's body and knees hold still (the glide, the bob and the dive wait), so the knee you hooked is where you aim the seam shot. Needs loom47 (the seam).",
    enabled: false,
    enabled_why: "Off until the lead has looked at its frames and its gates pass (NIGHT47.md section 1).",
    numbers: {},
  },
};

function addRows(raw) {
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  let out = raw;
  for (const [row, def] of Object.entries(NEW_ROWS)) {
    if (out.indexOf('"' + row + '": {') >= 0) continue;
    const at = out.indexOf('"crown47": {');
    if (at < 0) throw new Error("crown47 row not found");
    let i = out.indexOf("{", at), depth = 0, j = i, inStr = false;
    for (; j < out.length; j++) {
      const ch = out[j];
      if (inStr) { if (ch === "\\") j++; else if (ch === '"') inStr = false; continue; }
      if (ch === '"') inStr = true;
      else if (ch === "{" || ch === "[") depth++;
      else if (ch === "}" || ch === "]") { depth--; if (depth === 0) break; }
    }
    const lineStart = out.lastIndexOf("\n", at) + 1;
    const indent = out.slice(lineStart, at);
    const body = JSON.stringify(def, null, 2).split("\n").map((l, k) => (k === 0 ? l : indent + l)).join(eol);
    out = out.slice(0, j + 1) + "," + eol + indent + '"' + row + '": ' + body + out.slice(j + 1);
  }
  return out;
}

function block(nums, indent, eol) {
  const o = {};
  for (const [k, [v, why]] of Object.entries(nums)) { o[k] = v; o[k + "_why"] = why; }
  const lines = JSON.stringify(o, null, 2).split("\n");
  return lines.map((l, i) => (i === 0 ? l : indent + l)).join(eol);
}

function patch(raw) {
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  let out = raw;
  for (const [row, nums] of Object.entries(NUMBERS)) {
    const at = out.indexOf('"' + row + '": {');
    if (at < 0) throw new Error("row " + row + " not found");
    const key = out.indexOf('"numbers": ', at);
    const nextRow = out.indexOf('\n      "', at + 10);
    if (key < 0 || (nextRow > 0 && key > nextRow)) throw new Error("row " + row + ": no numbers key inside it");
    const lineStart = out.lastIndexOf("\n", key) + 1;
    const indent = out.slice(lineStart, key);
    let i = key + '"numbers": '.length;
    if (out[i] !== "{") throw new Error("row " + row + ": numbers is not an object");
    let depth = 0, j = i, inStr = false;
    for (; j < out.length; j++) {
      const ch = out[j];
      if (inStr) { if (ch === "\\") j++; else if (ch === '"') inStr = false; continue; }
      if (ch === '"') inStr = true;
      else if (ch === "{" || ch === "[") depth++;
      else if (ch === "}" || ch === "]") { depth--; if (depth === 0) break; }
    }
    out = out.slice(0, i) + block(nums, indent, eol) + out.slice(j + 1);
  }
  return out;
}

const raws = FILES.map((f) => fs.readFileSync(f, "utf8"));
if (raws[0] !== raws[1]) throw new Error("the two config copies differ before the patch");
const withRows = addRows(raws[0]);
const next = patch(withRows);
const a = JSON.parse(withRows), b = JSON.parse(next);
{
  const a0 = JSON.parse(raws[0]), ar = JSON.parse(withRows);
  for (const row of Object.keys(NEW_ROWS)) { if (!a0.design.n47[row]) { if (JSON.stringify(ar.design.n47[row]) !== JSON.stringify(NEW_ROWS[row])) throw new Error(row + ": new row did not round-trip"); delete ar.design.n47[row]; } }
  if (JSON.stringify(ar) !== JSON.stringify(a0)) throw new Error("adding builder L's rows changed something else");
}
for (const row of Object.keys(NUMBERS)) {
  const want = {};
  for (const [k, [v, why]] of Object.entries(NUMBERS[row])) { want[k] = v; want[k + "_why"] = why; }
  if (JSON.stringify(b.design.n47[row].numbers) !== JSON.stringify(want)) throw new Error(row + ": numbers did not round-trip");
  a.design.n47[row].numbers = want;
}
if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error("something outside builder L's numbers changed");
console.log(`n47-l-numbers: ${Object.keys(NUMBERS).join(", ")}: ${Object.values(NUMBERS).map((n) => Object.keys(n).length).join(" / ")} numbers; ${next === raws[0] ? "already in" : dry ? "would change (dry run)" : "written to both copies"}`);
if (!dry && next !== raws[0]) for (const f of FILES) fs.writeFileSync(f, next);
