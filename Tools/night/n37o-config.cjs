// NIGHT37 builder O (NIGHT37.md O1): writes builder O's own rows in Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/unity-config.json. L-049: text edits only (never a whole-file JSON.stringify); the file must still parse; idempotent.
//   1. design.n37.loom37: the row's text replaced (its `enabled`, levelFrom, levelTo and any key the lead added are kept): THE LOOM's numbers;
//   2. design.maps.list: an n37-span line right after n33-span's;
//   3. design.n31.unlocks.levels.rows: an n37-span row right after n33-span's (n 4: the NAILGUN, as n33-span).
// Run from the repo (or worktree) root: node Tools/night/n37o-config.cjs [root]
const fs = require("fs");
const path = require("path");
const ROOT = process.argv[2] || ".";
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");

const B = "Research/2026-10-12-wave37/BOSSES.md 'L4 THE LOOM'";
const ROW = {
  _why: "Builder O (NIGHT37.md O1; " + B + "): THE LOOM from scratch as a canyon crossing with no floor (level 4 = n37-span: n33-span's rooms s1..s7, two rehearsal rooms o1 THREAD LINE / o2 SWING ANCHOR and o3 THE CANYON, Tools/night/gen-n37-span-boss.cjs). "
    + "The grapple is the footing; nails in its knees are torn by pulling away. Game/SurvivalSim.Loom37.cs, Core/Enemies/Loom37.cs, Game/Loom37View.cs. Off (row off, --off37): level 4 is n33-span and its old LOOM.",
  hp: 750,
  hp_why: "THE LOOM's health. Damage lands in full only in the CYAN window (D-5, Boss37 Scale): three 2.5 s windows of the NAILGUN (rate 12 x 9 = 108 a second, PortExport weapons.nailgun) are about 810; leg 3's window ends in the fall whatever is left (BOSSES.md rule 3: no health sponge): a first pick, tuned on the band (bot p50 15-22 s).",
  fallY: -6,
  fallY_why: "m: feet under this height inside THE CANYON = a fall (the tower tops are at 8, the ledges at 0, the void floor at -24): the player is put back on the last anchor ledge stood on (BOSSES.md: 'fall = back to the last anchor ledge'; D-6 the leg's checkpoint).",
  fallCostHp: 20,
  fallCostHp_why: "health a canyon fall costs (" + B + ": '-20 HP').",
  cutDelayS: 3,
  cutDelayS_why: "s: a thread anchor goes dark this long after THE LOOM passes it (its station moves north of the anchor) (" + B + ": 'nodes go dark 3 s after it passes').",
  cutPulseS: 1,
  cutPulseS_why: "s: the last part of cutDelayS the anchor pulses RED before it goes dark (" + B + ": 'RED pulse first'; NIGHT36 tells: a wind-up of 1.0-1.4 s reads, BOSSES.md rule 7).",
  hideS: 4,
  hideS_why: "s on one tower (its top or its faces) before THE LOOM severs that tower's anchors (" + B + ": 'Hide rule: if you stay on one tower > 4 s, it severs that tower's anchors').",
  tearRefill: true,
  tearRefill_why: "a torn knee gives one grapple charge back (design.n32.grapple32 G32Give). NEW (the web has no grapple charges): MEASURED (loom37 fight --trace, 2026-10-12): with two pulls a leg (to the pair, across it) and design.n32.grapple32 recharging one charge in 4 s, the bot waited on charges ~8 s a leg (kill 30-34 s, the band wants 15-22). The damage move feeds the footing, as a kill refills the charge on other levels (killRefill).",
  rideRefillS: 0.8,
  rideRefillS_why: "s of one wall ride inside a loom37 room that gives one grapple charge back (once a ride). NEW (the web has no grapple charges; design.n32.grapple32 recharges one in 4 s): BOSSES.md L4's chain is 'pull, ride, pull' over a void with no enemies to refill on kills; a ride of 0.8 s (8 m at the ride's 10.45 m/s, n35-ride-distances.json) winds the next pull. A first pick, to check on the bot's crossing.",
  arriveUpMax: 3,
  arriveUpMax_why: "m/s: a pull's arrival inside a loom37 room keeps at most this much upward speed (MEASURED, loom37 fight --trace: a pull up to an anchor 8 m above kept ~24 m/s up after the zip's release and flew 14 m over it; design.n35.grapple35 clamps only the horizontal, to arriveKeepMax 4). 3 = grapple35's arrivePop: the pull 'stops there' (GG 2026-10-11, design.n35.grapple35._why). A jump or a dash cancel before the arrival keeps its speed.",
  swingAmpM: 7,
  swingAmpM_why: "m: the o2 SWING ANCHOR's swing east-west about its rest (the room is 28 m wide; 7 m keeps it 17.5 m from the ledge, in the 40 m reach). Ours.",
  swingPeriodS: 3.2,
  swingPeriodS_why: "s: one full swing of the o2 anchor (slow enough to aim at: half a period 1.6 s, one grapple flight 0.06-0.1 s). Ours.",
  kneeRadius: 1.3,
  kneeRadius_why: "m: a knee's hit sphere (NIGHT32's design.n32 loom jointRadius 1.3 for the old LOOM's joints, the drawing grows with it).",
  nailsToTear: 3,
  nailsToTear_why: "nails stuck in one knee before a pull-away tears it (the NAILGUN's nails stick: three is a short burst; " + B + ": 'nail a knee joint (nails stick), then pull yourself AWAY'). Ours.",
  tearDistM: 7,
  tearDistM_why: "m: with nailsToTear in a knee, the player's distance from it growing past this (while pulled by the grapple, or LT's detonate) tears it. Ours (a pull covers 7 m in 0.16 s at design.n31.grapplePull.speed 44).",
  nailHoldS: 6,
  nailHoldS_why: "s: stuck nails fall out of a knee after this long without a tear (the tear must follow the nailing). Ours.",
  tearStagger: 3,
  tearStagger_why: "stagger drained by one torn knee: two knees empty a leg's meter (kit.stagger 6 each leg): " + B + " 'every 2 joints torn = the leg pair goes limp; at stagger empty the spinneret opens CYAN'.",
  parryStagger: 1.5,
  parryStagger_why: "stagger drained by a spool parried or whipped back (Boss37Brain.Parried; half a tear: the parry buys time, the tear is the damage move).",
  stationSpeed: [36, 40, 36],
  stationSpeed_why: "m/s THE LOOM glides to its leg's station (and onto you after the anti-hide). MEASURED (loom37 fight --trace): at 5 / 8 m/s it was still crossing the gap when the next leg began, its knees (moving grapple nodes) sat on the tower-to-tower pull line and the pilot hooked a knee over the void 3 times; 36 m/s covers a station step (36 m) inside the 1.0 s leg change (kit.legChangeSeconds); leg 2 faster (" + B + ").",
  bodyY: 20,
  bodyY_why: "m: THE LOOM's body height over the canyon's ledges (tower tops are at 8: the knees hang between 9 and 15, inside a pull from a tower top). Ours.",
  kit: {
    _why: "Boss37 kit overrides (Boss37Params.From reads design.n37.loom37.kit.<key> over design.n37.boss37).",
    stagger: [6, 6, 6],
    stagger_why: "two torn knees a leg (tearStagger 3): six knees, three legs (" + B + ").",
    autoLegs: false,
    autoLegs_why: "the legs are places (the stations): Game/SurvivalSim.Loom37.cs calls NextLeg when a CYAN window closes.",
    legChangeSeconds: 1.0,
    legChangeSeconds_why: "MEASURED (loom37 band, 2026-10-12 22:08): 23.4 s p50 with the kit's 1.5 s leg change (two changes): the station step is a glide (stationSpeed 36 covers 36 m in 1 s), so the change is shorter. The band wants 15-22 s.",
    windowSeconds: 2.2,
    windowSeconds_why: "the CYAN window, a little under the kit's 2.5 s: three windows a fight (one a leg) and the NAILGUN unloads at 108 a second (PortExport weapons.nailgun rate x damage). MEASURED band 23.4 s p50 before (with legChangeSeconds 1.5).",
    chipScale: 0.02,
    chipScale_why: "MEASURED (loom37 fight, 2026-10-12 first run): at the kit's 0.1 a bot that never tore a knee killed it with THUNDERCLAP chip in 32 s (772 damage, 41 hits): the damage must be the tear and the window (BOSSES.md rule 2). 0.02: chip is a scratch.",
  },
  attacks: {
    _why: "THE LOOM's attacks (Boss37Attack.From): " + B + ".",
    lash: { kind: "red", windup: 1.2, active: 0.35, recover: 0.6, cost: 2, weight: 1, minDist: 0, maxDist: 60, legs: [1, 2, 3], habit: "any", _why: "THREAD LASH (RED 1.2 s): a thread whips across the span along the player's line: be off the line (pull to another node). 30 damage on a hit." },
    spool: { kind: "yellow", windup: 1.2, active: 1.2, recover: 0.5, cost: 1, weight: 1.2, minDist: 6, maxDist: 60, legs: [1, 2, 3], habit: "any", _why: "SPOOL SHOT (YELLOW 1.2 s): a weighted spool at the player: parry it back or whip it (Parried: refund + parryStagger). 25 damage on a hit." },
    web: { kind: "red", windup: 1.4, active: 2.5, recover: 0.6, cost: 3, weight: 0.8, minDist: 0, maxDist: 80, legs: [2, 3], habit: "camp", _why: "WEB WALL (RED 1.4 s): a sheet between the tower pair ahead (a layer swap): the route changes, ride round the tower. Legs 2-3." },
  },
  lashDamage: 30,
  lashDamage_why: "health a thread lash takes when it lands on the player (a RED hit; BOSSES.md rule 7: the dodge colour costs). Ours.",
  lashWidthM: 2.5,
  lashWidthM_why: "m: the lash's half width about the line from THE LOOM to where the player was at its wind-up's end. Ours.",
  spoolDamage: 25,
  spoolDamage_why: "health a spool takes when it hits. Ours.",
  spoolSpeed: 22,
  spoolSpeed_why: "m/s the spool flies (slow enough to parry at 20 m: 0.9 s). Ours.",
  lashLength: 60,
  lashLength_why: "m: how far the lash's line reaches past the body along its aim (the canyon is 120 m: it covers the span ahead). Ours.",
  spoolRadius: 0.8,
  spoolRadius_why: "m: the spool's hit radius (with the player's 0.6 m chest). Ours.",
  parryReach: 3.5,
  parryReach_why: "m: a punch meets a spool this close (NIGHT36 Lantern36's parryReach, design.n36.boss36).",
  parryConeDeg: 60,
  parryConeDeg_why: "deg: the punch's cone for a spool (as NIGHT36's orbs).",
  parryReturnMul: 1.5,
  parryReturnMul_why: "a parried spool flies home at spoolSpeed x this (it reads as sent back harder). Ours.",
  hideLeg2Scale: 0.75,
  hideLeg2Scale_why: "legs 2-3 sever a tower after hideS x this (" + B + ": 'leg 2: it moves faster and cuts more threads'). Ours.",
  revealS: 2,
  revealS_why: "s of the walk-in reveal before its first attack (rule 1, walkin36; the player stands on the start ledge and sees the canyon).",
  dieS: 2.5,
  dieS_why: "s of the fall into the canyon before the room clears.",
  kneeSpanX: 9,
  kneeSpanX_why: "m: the knees' distance east / west of the body: over the gaps between the towers' inner faces (the towers stand at 9-14 m off the centre line), so a knee is 5-8 m from a tower top: nail it from the ledge, pull away to the next anchor.",
  kneeDropY: 6,
  kneeDropY_why: "m: the knees hang this far under the body (bodyY 20 -> knees at 14: 6 m over the tower tops at 8).",
  kneeSpreadZ: 7,
  kneeSpreadZ_why: "m between the three knee pairs along the canyon (pair 1 south, toward the player; pair 3 north).",
  footSpanX: 21,
  footSpanX_why: "m: the feet grip the canyon walls (22 m off the centre line).",
  footDropY: 12,
  footDropY_why: "m: the feet's height under the body.",
  bobM: 0.8,
  bobM_why: "m: the knees bob up and down (moving nodes: the aim must follow).",
  bobPeriodS: 2.4,
  bobPeriodS_why: "s: one bob.",
  bodyHalfW: 3,
  bodyHalfW_why: "m: the body's hit radius (the spinneret is under it).",
  bodyHalfH: 1.6,
  bodyHalfH_why: "m: the body's half height (the spinneret hangs under it).",
};

function eolOf(t) { return t.includes("\r\n") ? "\r\n" : "\n"; }
function closeOf(t, open, o = "{", c = "}") {
  let depth = 0, inStr = false;
  for (let i = open; i < t.length; i++) {
    const ch = t[i];
    if (inStr) { if (ch === "\\") i++; else if (ch === "\"") inStr = false; continue; }
    if (ch === "\"") inStr = true;
    else if (ch === o) depth++;
    else if (ch === c && --depth === 0) return i;
  }
  throw new Error("unbalanced");
}

let t = fs.readFileSync(CFG, "utf8");
const eol = eolOf(t);

// 1. the loom37 row
{
  const n37 = t.indexOf("\"n37\": {");
  if (n37 < 0) throw new Error("no design.n37 block: merge port/stage3 first");
  const n37Close = closeOf(t, t.indexOf("{", n37));
  const key = t.indexOf("\"loom37\": {", n37);
  if (key < 0 || key > n37Close) throw new Error("no loom37 row inside design.n37");
  const open = t.indexOf("{", key), close = closeOf(t, open);
  const old = JSON.parse(t.slice(open, close + 1));
  const indent = t.slice(t.lastIndexOf("\n", key) + 1, key);
  const row = { _why: ROW._why, enabled: old.enabled === true, levelFrom: old.levelFrom || "n33-span", levelTo: old.levelTo || "n37-span" };
  for (const [k, v] of Object.entries(ROW)) if (k !== "_why") row[k] = v;
  for (const [k, v] of Object.entries(old)) if (!(k in row)) row[k] = v;
  const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  t = t.slice(0, open) + body + t.slice(close + 1);
}

// 2. design.maps.list
if (!t.includes("{ \"id\": \"n37-span\"")) {
  const a = "{ \"id\": \"n33-span\", \"label\": \"LEVEL 4 THE SPAN\", \"file\": \"Design/levels/n33-span.json\", \"campaignOnly\": true },";
  const at = t.indexOf(a);
  if (at < 0 || t.indexOf(a, at + 1) >= 0) throw new Error("maps.list: n33-span's line not found exactly once");
  const indent = t.slice(t.lastIndexOf("\n", at) + 1, at);
  const line = "{ \"id\": \"n37-span\", \"label\": \"LEVEL 4 THE SPAN\", \"file\": \"Design/levels/n37-span.json\", \"campaignOnly\": true },";
  t = t.slice(0, at + a.length) + eol + indent + line + t.slice(at + a.length);
}

// 3. design.n31.unlocks.levels.rows
if (!/"id": "n37-span",\s*"earns"/.test(t)) {
  const idAt = t.search(/"n": 4,\s*"id": "n33-span",\s*"earns"/);
  if (idAt < 0) throw new Error("unlocks: n33-span's row not found");
  const open = t.lastIndexOf("{", idAt), close = closeOf(t, open);
  const objIndent = t.slice(t.lastIndexOf("\n", open) + 1, open);
  const inner = objIndent + "  ";
  const row = ["{", `${inner}"n": 4,`, `${inner}"id": "n37-span",`, `${inner}"earns": "nailgun",`, `${inner}"note": "LEVEL 4 THE SPAN with THE LOOM from scratch (NIGHT37 O, design.n37.loom37 swaps n33-span for it): n33-span's rooms, two rehearsal rooms and THE CANYON. The NAILGUN (also a pickup in room s1), as n33-span."`, `${objIndent}}`].join(eol);
  t = t.slice(0, close + 1) + "," + eol + objIndent + row + t.slice(close + 1);
}

JSON.parse(t);
fs.writeFileSync(CFG, t);
fs.writeFileSync(SA, t);
const c = JSON.parse(t);
console.log("loom37:", Object.keys(c.design.n37.loom37).filter((k) => !k.endsWith("_why")).join(", "), "| enabled", c.design.n37.loom37.enabled);
console.log("maps.list n37-span:", c.design.maps.list.some((m) => m.id === "n37-span"));
console.log("unlocks row n37-span:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n37-span"));
console.log("copied to", SA);
