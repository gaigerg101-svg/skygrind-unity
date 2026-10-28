// FORGE72: writes Research/2026-10-27-forge72/slate-seed.json (NIGHT72's slate; the lead copies it to Logs/n72/slate.json at the bearings).
// Every slice has the fields n72-slate.cjs needs (id, title, why, owner, row, red, doneWhen, visible, passes, state) plus track, est
// (minutes, from the measured w71 and 2026-10-26 evening rates), verdict, spot and, for a slice that turns a row ON, `on` (the close
// check's `seen` line) and the freeze tool's passSha / passFiles (filled by the lead when the pair passes).
//   node Research/2026-10-27-forge72/slate-make.cjs
const fs = require("fs");
const path = require("path");
const S = [];
const add = (o) => S.push(Object.assign({ state: "todo", passes: false, verdict: null, spot: null, passSha: null, passFiles: [] }, o));
const TOOLS = "n/a (tools)";

// ---- the lead ----
add({ id: "S0", track: "lead", owner: "lead", est: 25, title: "Bearings, the shared commit (n72-rows.cjs + Rows72.cs + its test), worktrees, four builders spawned",
  why: "Builders branch from one sha with every row pre-placed (L-049 / L-065); one build must switch any row (D-3).", row: "design.n72 (11 rows OFF); -skygrindRows72",
  red: "no generic row switch (NightRows.ReelOverride is set only by the same-run reel)", visible: TOOLS,
  doneWhen: ["the shared commit compiles (unity-loop compile: DLLs rewritten) and N63SameRun passes", "Rows72's EditMode test seen RED once then GREEN (a sim row refused; a view row and a level row switched)", "worktrees n72-L/C/B/N at the shared sha; the four builders running (their ids in STATE.md)"] });
add({ id: "S1", track: "lead", owner: "lead", est: 60, title: "THE H TRAIN: build n72-h0, the harvest pairs (harvest.md groups H1-H8), the spotter, the per-row verdicts",
  why: "About 25 built rows are OFF only because their gates never ran (off-why.txt); GG: \"I just haven't seen it\".", row: "the HV-* slices",
  red: "n71-forge72's frames (spike.md S1)", visible: TOOLS,
  doneWhen: ["Builds/n72-h0 from the shared commit; every harvest pair side run (Logs/feel/n72-h0/summary47.txt)", "each HV slice set pass (flip next train), short (with the pair's number) or handed to its builder (state stays todo, owner changed)"] });
const HV = [
  ["colours63", "n63", "C", "colour", "tour65 n52-spire + n52-foundry; boss63 n52-spire"],
  ["goal64", "n64", "C", "colour", "tour65 n52-spire (the spire doors)"],
  ["yellow56", "n56", "C", "colour", "tour65 n52-iceline; verbs37 gun frames"],
  ["decor55", "n55", "C", "colour", "tour65 n52-foundry + n57-kiln"],
  ["fixture61", "n61", "L", "light", "tour65 n52-iceline"],
  ["ssao55", "n55", "L", "light", "L1's flicker meter, still poses on two levels"],
  ["exit63", "n63", "N", "exits", "a157 n57-kiln -skygrindClearShots60"],
  ["wyrm71", "n71", "B", "boss", "boss63 n57-kiln"],
  ["voice71", "n71", "B", "boss", "boss63 n52-spire"],
  ["words71", "n71", "B", "boss", "boss63 n57-kiln"],
  ["cyan71", "n71", "B", "boss", "boss63 n57-kiln + n52-spire"],
  ["bossText55", "n55", "B", "boss", "boss63 n57-kiln + n52-spire"],
  ["hitzone47", "n47", "B", "hits", "kill12 / headshot12 reel phases"],
  ["models41", "n41", "B", "hits", "kill12 reel phase (death break-apart)"],
  ["kicks71", "n71", "B", "hits", "kill12 reel phase (kicks71's own probe line)"],
  ["finish55", "n55", "B", "finish", "a boss kill (B builds the shot if no reel has one)"],
  ["step55", "n55", "N", "camera", "verbs37 telemetry (the step-down pop; N4 reads it with cam72)"],
  ["reticle42", "n42", "B", "small", "verbs37 frames on the charge rows"],
  ["pump40", "n40", "B", "small", "verbs37 frames on the THUNDERCLAP rows"],
  ["select41", "n41", "N", "small", "the level-select frame (ui reel phase)"],
];
for (const [r, n, own, grp, pair] of HV) {
  add({ id: "HV-" + r, track: "harvest " + grp, owner: "lead", est: 8, title: "HARVEST " + n + "." + r + " ON (harvest.md " + grp + ")",
    why: "Built and OFF only until a gate that never ran (Research/2026-10-27-forge72/off-why.txt); D-1 / D-2.", row: "design." + n + "." + r, on: n + "." + r,
    red: "the same build with the row OFF (" + pair + ")", visible: "the pair named in harvest.md (" + pair + ")",
    doneWhen: ["the same-build pair on " + pair + ": the ON side with -skygrindRows72 " + r + ":1, the OFF side the repo's config (the row is OFF in it); made with n72-pair.cjs into Logs/n72/pairs/" + r + "/", "the spotter's SPOT line names a change (saved as Logs/n72/spot/" + r + "/spot.md)", "the lead's look: no new defect from gg-proxy.md's list; the H train's combined guard run (every passing row :1 together, the close's verbs37 sides and the smoke) holds", "then n72-freeze-rows.cjs " + n + "." + r + "=<the measured reason>; a row whose pair shows a defect goes to owner " + own + " (the slice moves to " + own + "; the owner starts that rework only then)"] });
}
const LV = [["foundry65", "n65", "n65-foundry"], ["iceline67", "n67", "n67-iceline"], ["kiln67", "n67", "n67-kiln"], ["spire67", "n67", "n67-spire"]];
for (const [r, n, lvl] of LV) {
  add({ id: "LV-" + r, track: "levels", owner: "lead", est: 15, title: "LEVEL ROW " + n + "." + r + " ON on its own result (" + lvl + ", PLAY71's rooms; D-4)" + (r === "foundry65" ? ": regenerated first so 1.3 RIDE has no roof" : ""),
    why: "GG's \"complete arena overhaul\" (2026-10-09); w71's play build had it ON and GG named only the 1.3 roof (D-4).", row: "design." + n + "." + r, on: n + "." + r,
    red: "the shipped level's verbs37 on the same build", visible: "tour65 on " + lvl + " vs the shipped level",
    doneWhen: ["N0 picked by path into the main tree (" + lvl + " in both copies and its slots file" + (r === "foundry65" ? "; 1.3 RIDE without its roof" : "; sha1 equal to GG's play build's file, spike.md S3") + ")", "verbs37 on " + lvl + " with -skygrindRows72 " + r + ":1 and the ULTRAKILL profile: no RED row that the shipped level passes on the same build (the shipped level's side runs with " + r + ":0, L-154)", "the smoke DONE with -skygrindRows72 " + r + ":1 (or in the combined guard run)", "passFiles name the level file in both copies and its slots file (the freeze tool requires it); the slots file stays all old (list-mode generation, as n71's play build): the report's ROW line prints SLOTS old and says the rooms are in the level file"] });
}
add({ id: "PLAY", track: "lead", owner: "lead", est: 35, title: "GG's play build Builds/n72-play (the freeze build, the repo's config) and PLAY72.md",
  why: "E72: what GG plays is the default game; the card says what changed and how to compare (the off switches).", row: "n/a",
  red: "Builds/n71-play (rows ON only in its config)", visible: "n/a (GG plays it)",
  doneWhen: ["Builds/n72-play = the freeze build (hard links); Logs/player/n72-play-smoke.log DONE", "PLAY72.md committed: each change ON, one line each, where to see it, its off switch, keep / drop boxes"] });
add({ id: "REV", track: "lead", owner: "lead", est: 25, title: "One skygrind-reviewer pass on the integrated diff (u3-n71..HEAD's game code) before the freeze",
  why: "NIGHT61's review found two bugs no gate saw; tonight every row ships ON, so each row's off switch must restore the old game.", row: "n/a",
  red: "no review", visible: TOOLS, doneWhen: ["Logs/n72/review.md written; every BUG finding fixed or logged with its reason"] });

// ---- L: LIGHT & SCREEN ----
add({ id: "L1", track: "light", owner: "L", est: 60, title: "The flicker meter: a still-camera reel phase (still72) and Tools/night/n72-flicker.py, with a planted case",
  why: "GG 2026-10-26 \"there's flickers\"; 2026-10-20 \"flickering bugs that happen constantly\". No meter has ever measured flicker (gg-proxy: a still cannot).", row: "n/a (a reel phase and a tool)",
  red: "no number", visible: TOOLS,
  doneWhen: ["still72 holds the camera at each tour65 pose for 60 rendered frames with the husks frozen and writes them", "n72-flicker.py prints per pose: the share of pixels whose luma flips sign 3+ times in 60 frames by more than 6 levels, p99.9 |dY|, and a heatmap PNG", "the planted case (a 32x32 block toggled every frame by -skygrindFlickerPlant72) reads RED, the same pose without it GREEN"] });
add({ id: "L2", track: "light", owner: "L", est: 70, title: "fix72: the flicker sources the meter names, fixed (SSAO without TAA, grain, bloom on thin strips, the CRT pass)",
  why: "GG 2026-10-26 \"flickers\"; scout-lighting.md 1g / 5; spike.md S2 ruled out the Stone shader.", row: "design.n72.fix72", on: "n72.fix72",
  red: "L1's numbers with fix72 OFF on the five levels' still poses", visible: "the worst pose's heatmap and frame pair",
  doneWhen: ["each candidate source toggled alone on one build and ranked by the meter (Research/2026-10-27-wave72/L/flicker-rank.md)", "fix72 ON: the flipping-pixel share falls by >= 50 % on the five levels' worst poses vs OFF, same build", "fingerprints EQUAL (view only); verbs37 unchanged"] });
add({ id: "L3", track: "light", owner: "L", est: 60, title: "lit72: the boss arenas and level 5 lifted inside each boss's own hue",
  why: "GG 2026-10-26 \"the boss rooms are very dark\" (asked 3 times); spike.md S1: wyrm idle 47 % dark, crown 35-43 %, spire rooms 23-30 %.", row: "design.n72.lit72", on: "n72.lit72",
  red: "boss63 on n57-kiln and n52-spire, tour65 n52-spire (n71-forge72)", visible: "boss63 bk / bp pairs; tour65 tp pair",
  doneWhen: ["dark < 30 share <= 15 % on every boss63 frame of the wyrm and THE CROWN, and <= 12 % on the spire's tour poses, with lit72 ON (same build)", "each boss frame's mean hue within 20 degrees of its OFF frame (its identity kept: GG 2026-10-20 each boss its own palette)", "blown > 235 share <= 1 %; fingerprints EQUAL"] });
add({ id: "L4", track: "light", owner: "L", est: 40, title: "calm72: no noise border at full health; the low-health edge thin and only under 30 %",
  why: "gg-proxy #10: a red speckled border 150-250 px deep at full health on wyrm frames; GG's own screenshots at 45 and 31 health show a thick red frame.", row: "design.n72.calm72", on: "n72.calm72",
  red: "boss63 bk stagger / windup frames (n71-forge72); a planted 45 % health frame", visible: "the boss63 bk stagger pair",
  doneWhen: ["the noise border's source named (file:line) and the border's pixels at full health 0 with calm72 ON", "at 45 % health no edge; under 30 % an edge at most 60 px deep at 1080p", "fingerprints EQUAL"] });
add({ id: "L5", track: "light", owner: "L", est: 50, title: "Overlapping textures: an offline coplanar-face finder over every shipped and rebuilt level, then the fixes",
  why: "GG 2026-10-26 \"textures overlapping that bug out\"; Bright71.Grow staggers boxes (407cf1874) but nothing has counted the pairs left.", row: "the level files or SurvivalBuilder's grow (no row: geometry)",
  red: "the finder's count on HEAD's levels", visible: "the worst pair's frame before / after",
  doneWhen: ["Tools/night/n72-zscan.cjs lists every coplanar overlapping face pair per level (a planted pair found; a 0.01 m gap not)", "the pairs left after Grow reach 0 on the five shipped levels and the four n65 / n67 levels, or each one left is listed with why"] });
add({ id: "L6", track: "light", owner: "L", est: 40, title: "fixture61 (harvest) made right: lights held by a housing and a beam, not floating",
  why: "gg-proxy #7; GG 2026-10-11 lights \"placed and floating\".", row: "design.n61.fixture61", on: "n61.fixture61",
  red: "tour65 n52-iceline / n52-spire with the row OFF", visible: "tour65 pairs", doneWhen: ["its own proof line '[fixture61] mounts ... unmounted <n>' with unmounted 0 on the five levels", "the spotter names the lamps on a pair"] });

// ---- C: COLOUR & DECOR ----
add({ id: "C1", track: "colour", owner: "C", est: 60, title: "colours63 (harvest) made right: the colour table read by the offenders gg-proxy names",
  why: "GG 2026-10-26 \"colors ... I just haven't seen it\" (asked 5 times); gg-proxy #2 / #3: the spire's red stripes, the wyrm's lime edge lines, the crown's danger-coloured ring.", row: "design.n63.colours63", on: "n63.colours63",
  red: "tour65 n52-spire + boss63 pairs with the row OFF", visible: "tour65 tp pair",
  doneWhen: ["its audit tool's offender list re-run; the named offenders read the role table", "red-orange pixels on non-danger surfaces (the spire's stripes) fall >= 50 % on vs off, same build", "the spotter names the colour change"] });
add({ id: "C2", track: "colour", owner: "C", est: 70, title: "palette72: each level's floor, wall and trim values apart, one accent per level, red-orange only on danger",
  why: "gg-proxy #2: \"each level is one tint\" (spire olive everywhere); GG 2026-10-20 \"lack strong color direction\".", row: "design.n72.palette72", on: "n72.palette72",
  red: "tour65 frames of the five levels (n71-forge72)", visible: "tour65 pairs on n52-spire and n57-kiln",
  doneWhen: ["in every tour pose the floor's median luma and the walls' median luma differ by >= 20 (0-255; Research/2026-10-27-wave72/C/values.md, the masks by screen region documented)", "no level's top hue covers > 60 % of its tour frames", "cyan stays the brightest saturated hue in each frame (usable first)"] });
add({ id: "C3", track: "colour", owner: "C", est: 50, title: "decor72: stripes and blocks with no job removed, or turned into route and ride-wall marks",
  why: "gg-proxy #8 (asked 5 times: \"random pillars that serve no purpose\", \"for the sake of detail\").", row: "design.n72.decor72", on: "n72.decor72",
  red: "tour65 frames (foundry a3's purple band; spire red stripes)", visible: "tour65 tf / tp pairs",
  doneWhen: ["every remaining stripe marks a ride wall, a route or a hazard (the level data's mark list printed per level)", "the spotter names it on a pair", "no collider changes (n46flow byte-equal)"] });
add({ id: "C4", track: "colour", owner: "C", est: 40, title: "The other colour harvest rows made right: goal64, yellow56, decor55 (whatever their H pairs show)",
  why: "harvest.md H1.", row: "design.n64.goal64 / n56.yellow56 / n55.decor55", red: "their H pairs", visible: "their H pairs",
  doneWhen: ["each row's defect from the H pair fixed and its pair re-taken on the next train, or SHORT with the number"] });
add({ id: "C5", track: "colour", owner: "C", est: 60, title: "Blank walls (gg-proxy #9): the biggest bare faces in view get a job (a ride mark, a route mark, a landmark; no collider change)",
  why: "GG 2026-10-26 \"other rooms and arenas that look ugly\"; gg-proxy: iceline g3 half the frame one navy wall, foundry a8 40 % a bare slab.", row: "design.n72.decor72 (the same row)",
  red: "tour65 ti g3, tf a8", visible: "tour65 pairs", doneWhen: ["the blank-face share (one flat colour region > 25 % of the frame) falls in the named rooms on vs off", "no collider change"] });

// ---- B: BOSSES & BODIES ----
add({ id: "B1", track: "boss", owner: "B", est: 75, title: "body72: THE KILN WYRM restyled inside the pill language: its presence (a rim light and value apart from the wall), spring follow-through along its pill segments, a tell glow in the danger colour; no new creature design",
  why: "gg-proxy #4: the wyrm blends into the wall and nothing shows its charge; GG 2026-10-22 \"boss animations are too rigid\". GG 2026-10-16 keeps \"the pill-shaped enemy concept\" and 2026-10-13 \"keep the worm AI the same\"; CLAUDE.md rule 7: enemy design is GG's, so the silhouette family stays. Built on wyrm71's state from the H train.", row: "design.n72.body72", on: "n72.body72",
  red: "boss63 bk with the row OFF", visible: "boss63 bk idle / windup pairs",
  doneWhen: ["the drawn body stays inside BossMotion63's hitbox guard (its [wyrm71] / [boss63] lines); the AI, timings and hit spheres untouched", "the wyrm's median luma differs from the wall behind it by >= 25 (0-255) in boss63 idle; the tell glow shows in windup and not in idle; the segments' motion RMS over the reel > OFF (the transform log)", "the silhouette at rest is the same pill segments (the lead compares the idle pair); the spotter names the worm; DECIDE72 lists body72 with its pair"] });
add({ id: "B2", track: "boss", owner: "B", est: 60, title: "bossfx72: THE CROWN visible in its fight; bursts off the crosshair; the dash ring cyan (usable), the tell cone smaller",
  why: "gg-proxy #3: THE CROWN is in none of its five frames; a 700-900 px burst sits on the crosshair; the ring is drawn in the danger colour.", row: "design.n72.bossfx72", on: "n72.bossfx72",
  red: "boss63 bp frames with the row OFF", visible: "boss63 bp pairs",
  doneWhen: ["THE CROWN's body visible in >= 4 of 5 boss63 frames (its pixels counted by its own colour band)", "no effect covers > 15 % of the frame inside 300 px of the crosshair", "the ring the text names is cyan; the tell cone <= 20 % of the frame"] });
add({ id: "B3", track: "boss", owner: "B", est: 50, title: "btext72 with words71, voice71 and bossText55: the boss line names the hand you hold, STAGGER only when staggered, one order at a time, a plate so beams never cut it",
  why: "gg-proxy #5 (\"Swing the gauntlet LEFT CLICK\" while the THUNDERCLAP is held; STAGGER shown in idle; lines cut by the beam).", row: "design.n72.btext72", on: "n72.btext72",
  red: "boss63 bk / bp frames", visible: "boss63 pairs",
  doneWhen: ["a counter line in the Player log: wrong-hand lines 0 and STAGGER-while-not-staggered 0 with the row ON (both > 0 OFF)", "boss text >= 22 px at 1080p on a dark plate", "the spotter names the text"] });
add({ id: "B4", track: "boss", owner: "B", est: 60, title: "spring72: spring secondary motion on enemies and bosses (windup squash, flinch, idle breathing, death), with hitzone47 and models41",
  why: "GG 2026-10-26 \"animations ... I just haven't seen it\"; scout-animation.md 1 (t3ssel8r springs: f 1-3 Hz body, 4-8 Hz flinch; z 0.3-0.5).", row: "design.n72.spring72", on: "n72.spring72",
  red: "kill12 / boss63 with the row OFF", visible: "a kill12 pair and a boss63 windup pair",
  doneWhen: ["render only: hitboxes and fingerprints unchanged", "the transform log's idle and windup motion RMS > OFF; squash keeps volume (product of scales 1 +- 0.02)", "the spotter names the motion on a pair"] });
add({ id: "B5", track: "boss", owner: "B", est: 40, title: "Kill feel: kicks71 and finish55 (harvest) made right",
  why: "GG 2026-10-14 \"hits and kills need to feel more satifying\"; T-77.", row: "design.n71.kicks71 / n55.finish55", red: "kill12 with the rows OFF", visible: "kill12 pair",
  doneWhen: ["kicks71's probe GREEN and an aligned kill pair (L-156) the spotter names", "finish55's boss-kill frame taken or SHORT with why"] });

// ---- N: NAVIGATION & MOVEMENT ----
add({ id: "N0", track: "move", owner: "N", est: 50, title: "The level rows' files: GG's play-build rooms assembled into the repo's levels (foundry b1 a8 with 1.3 RIDE roof-less; iceline g4; kiln kb2 kb3 kb3c kb3d; spire v6) and their slots files",
  why: "D-4: the four level rows go ON with the rooms GG played; the repo's level files are the all-old assemblies; two roof traps (spike.md S3).", row: "Design/levels/n65 and n67 level files, plans and slots (no row: data the LV slices flip)",
  red: "the repo's n65 / n67 files (every slot old; n65 a3 roof true)", visible: "n/a (the LV slices carry it)",
  doneWhen: ["`node Tools/night/gen-n65-foundry.cjs --export a3` (no --force): rooms-n52/a3.json reads roof false; `--check` PASS", "`gen-n65-foundry.cjs --run --slots=b1,a8` (list mode): the regenerated plan's a3 roof false and no slab over a3's rect at its 10 m height", "`gen-slots.cjs --level iceline --run --slots=g4`, `--level kiln --run --slots=kb2,kb3,kb3c,kb3d`, `--level spire --run --slots=v6` (list mode): each n67 file's sha1 equal to the play build's (spike.md S3)", "the slots files stay all old (n65-slot-set.cjs needs a room GATE, out tonight; list mode needs none); levels (both copies) and plans committed on n72-N with the gen logs as evidence"] });
add({ id: "N1", track: "move", owner: "N", est: 25, title: "verbs37 row 12 (bunny hop) under the ULTRAKILL profile: the reel's window fits the new air time",
  why: "iceline and kiln read 2 hops (speed kept 100 %) since ultra71; the close's player line needs every side PASS (spike.md S5).", row: "n/a (the reel)",
  red: "Logs/feel/n71-polish8 vi: RED [12] 2 hops", visible: TOOLS,
  doneWhen: ["the bar is unchanged (3 hops, speed kept per hop); only the reel's window grows to hold 3 hops of the profile's air time (after N5's profile decision); the web profile (-skygrindNoUltra71) still 3 hops", "the lead's next train: verbs37 PASS on all five shipped levels and on the n67 levels"] });
add({ id: "N2", track: "move", owner: "N", est: 60, title: "The reach audit: 5.07 m jump + 4.1 m double jump vs every room's walls and ceilings (escapes, bonk traps), then the level fixes",
  why: "The levels were built for a 1.5 m jump; GG asked for ULTRAKILL's (2026-10-26) and to keep the double jump.", row: "the level data (no row)",
  red: "HEAD's levels with the profile", visible: "a frame of each escape found",
  doneWhen: ["Tools/night/n72-reach.cjs prints per room: boundary tops under the reach (escapes) and ride lanes whose ceiling is within 1.8 m of the apex (bonks); a planted low wall found", "0 escapes on the five shipped levels and the four n65 / n67 levels (fixed in the plan, regenerated) or each listed with why"] });
add({ id: "N3", track: "move", owner: "N", est: 80, title: "goal72: the room's one line at the thing it names (verb in capitals, <= 6 words, >= 22 px), its marker, stuck hints at 10 s and 20 s that retire on success",
  why: "GG 2026-10-26 \"OK now you have to go up ... gamified and easy\"; his limit 2026-10-14 \"way too much instruction at the start\"; scout-guidance.md 1-4.", row: "design.n72.goal72", on: "n72.goal72",
  red: "tour65 frames: 7 of 19 rooms have a line, none says where to go (gg-proxy #6)", visible: "tour65 pairs (the exit-open state) and a planted stuck run",
  doneWhen: ["a Player log counter: lines shown, hints shown at 10 s / 20 s, hints retired after the verb; a planted stuck run (-skygrindStuck72) shows both hints, a normal verbs37 run shows none", "at most one line on screen; none at a level's first 5 s beyond the room name", "the spotter names the line on a pair"] });
add({ id: "N4", track: "move", owner: "N", est: 45, title: "cam72: the ULTRAKILL camera: strafe roll +-1 deg (x5 dashing or sliding), dash FOV x0.95 forward / x1.1 back, slide eye drop 0.32 m",
  why: "GG 2026-10-26 ULTRAKILL \"almost a one to one copy\"; scout-movement.md 2 (V1Camera.cs port).", row: "design.n72.cam72", on: "n72.cam72",
  red: "verbs37 telemetry with the row OFF", visible: "a verbs37 strafe / slide frame pair (the roll and the eye drop)",
  doneWhen: ["telemetry: roll, FOV and eye height follow the events with the row ON and stay at today's values OFF", "view only: fingerprints EQUAL"] });
add({ id: "N5", track: "move", owner: "N", est: 60, title: "The movement parity lab, then the profile's ground and air (friction / accel about 36, air accel about 3.6) if the lab and verbs37 agree",
  why: "D-7: our ground stops about 6x slower than ULTRAKILL's (half-life 115 ms vs 19 ms); scout-movement.md 3-4.", row: "design.ultra71.set (data only; Mover.cs untouched)",
  red: "the lab's numbers with today's profile", visible: "n/a (GG plays it; the lab prints the numbers)",
  doneWhen: ["the lab prints flat-jump apex and air time, dash distance, slide speed, slam speed, wall-jump speeds, stop time from walk speed, air turn time, each vs the scaled ULTRAKILL target", "the profile decision (a change, or NO CHANGE with the numbers) by the first train after the H train; a change only if its number moves toward the target and verbs37 holds on the five levels; the last profile commit by the third train", "after a change: FingerprintBaseline.Ultra71Hashes re-recorded from two equal -skygrindFingerprint runs (NIGHT72.md section 5 step 7); the smoke DONE"] });
add({ id: "N6", track: "move", owner: "N", est: 40, title: "exit63 (harvest) made right, and the iceline label under its row (DECIDE71 Q-3: the reel's old-map gate)",
  why: "GG 2026-10-13 doors \"like dark\"; the iceline row ON reads verbs37 25/29 on n52-iceline (L-154).", row: "design.n63.exit63", on: "n63.exit63",
  red: "a157 n57-kiln clear shots with the row OFF", visible: "the clear-shot pair",
  doneWhen: ["exit63's own gain >= 1.5x in 4 of 5 rooms (its tool) and the spotter names the door", "the iceline label: the reel's shipped-map side runs with -skygrindRows72 iceline67:0 (L-154); a reel fix is optional and only with the row off reading 29/29"] });

// FORGE72 red team 3 / 10: each builder's order (L3 lit72 is the top value; L2 needs L1's numbers from a train; N5's profile decision
// comes before the hop window and the reach audit that depend on it)
// (red team 7: a builder opens with new work; reworks of harvest rows (C1, C4, L6, B3, B4's harvest half, N6) wait for the H train's pairs)
const ORDER = { L: ["L1", "L3", "L4", "L6", "L2", "L5"], C: ["C2", "C3", "C1", "C4", "C5"], B: ["B2", "B1", "B3", "B4", "B5"], N: ["N0", "N5", "N1", "N2", "N3", "N4", "N6"] };
for (const [own, ids] of Object.entries(ORDER)) {
  const at = S.findIndex((s) => s.owner === own);
  const mine = ids.map((id) => S.find((s) => s.id === id));
  const rest = S.filter((s) => s.owner !== own);
  S.length = 0; S.push(...rest.slice(0, at), ...mine, ...rest.slice(at));
}

const seed = { _why: "NIGHT72's slate (FORGE72): the lead, then four tracks ranked; about 2x the building time with the leftovers in NIGHT72.md section 7. The lead copies it to Logs/n72/slate.json at the bearings and is its only editor (n72-slate-set.cjs / n72-slate-add.cjs). States: todo / building / measuring / critic / pass / short / blocked. A slice with `on` passes only with that row ON in both config copies (the close check's seen line). No critic per slice tonight (D-6): the verdict file is the lead's, from the pair, the SPOT and the guards.", slices: S };
fs.writeFileSync(path.join(__dirname, "slate-seed.json"), JSON.stringify(seed, null, 1) + "\n");
console.log("slate-seed.json: " + S.length + " slices; est total " + S.reduce((a, s) => a + s.est, 0) + " min; by owner " + JSON.stringify(S.reduce((m, s) => (m[s.owner] = (m[s.owner] || 0) + s.est, m), {})));
