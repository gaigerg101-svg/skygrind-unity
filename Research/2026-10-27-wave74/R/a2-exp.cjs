// NIGHT74 R: one-off experiments on a2's room file, to find what drives the walk bot's stall (diagnosis only; never committed as a room).
// Usage: node Research/2026-10-27-wave74/R/a2-exp.cjs <name> <in.json> <out.json>      writes the mutated room file to <out.json>
const fs = require("fs");
const [name, inp, out] = process.argv.slice(2);
const f = JSON.parse(fs.readFileSync(inp, "utf8"));
const r = f.row;
function moveStand(at) {
  r.standIns = [{ at, mode: "static" }]; r.standIns31 = [{ at, mode: "static" }];
  r.standIns36 = [Object.assign({}, r.standIns36[0], { at })];
}
const E = {
  base() {},
  // the fair red: the old room (give <in.json> = rooms-n52/<id>.json) with bot43 false, so the generic autopilot drives it as it drives a
  // rebuilt room (the old row has no bot43 key: Walk38Foundry / Walk43Foundry's id scripts walk it, the stall of the plain red is theirs)
  bot43false() { r.bot43 = false; },
  // stand-ins back on the old floor line on the door's axis (the red's), everything else as the candidate
  "standins-floor"() {
    const s = [{ at: [0, 0, -44], mode: "static" }, { at: [0, 0, -46], mode: "static" }, { at: [0, 0, -48], mode: "static" }];
    r.standIns = s; r.standIns31 = s;
    r.standIns36 = s.map((o) => Object.assign({ job: "marker", why: "diagnosis" }, o));
  },
  // one stand-in (the first), the lesson counts one body fewer
  "one-standin"() {
    r.standIns = r.standIns.slice(0, 1); r.standIns31 = r.standIns31.slice(0, 1); r.standIns36 = r.standIns36.slice(0, 1);
    r.lessons = r.lessons.map((l) => (l.kind === "husksDead" ? Object.assign({}, l, { n: l.n - 1 }) : l));
    r.lessons31 = r.lessons31.map((l) => (l.kind === "husksDead" ? Object.assign({}, l, { n: l.n - 1 }) : l));
  },
  // one stand-in, its place on the breech moved: a = the south edge, b = the middle, c = the north end (the file's own is (3.9, 2.5, -50.2))
  "s1-a"() { E["one-standin"](); moveStand([4.5, 2.5, -50.1]); },
  "s1-b"() { E["one-standin"](); moveStand([5.0, 2.5, -52.2]); },
  "s1-c"() { E["one-standin"](); moveStand([5.6, 2.5, -54.0]); },
  // one stand-in on the floor 8 m ahead and 4 m east of the door's axis (reachable, no deck between)
  "one-standin-floor"() {
    const s = [{ at: [4, 0, -45], mode: "static" }];
    r.standIns = s; r.standIns31 = s; r.standIns36 = [Object.assign({ job: "marker", why: "diagnosis" }, s[0])];
    r.lessons = r.lessons.map((l) => (l.kind === "husksDead" ? Object.assign({}, l, { n: l.n - 1 }) : l));
    r.lessons31 = r.lessons31.map((l) => (l.kind === "husksDead" ? Object.assign({}, l, { n: l.n - 1 }) : l));
  },
  // n52's wave (two husks): the stall's wave-count share
  "n52-waves"() {
    r.waves = [["husk", "husk"]]; r.waveNodes43 = [r.waveNodes43[0]];
    r.lessons = r.lessons.map((l) => (l.kind === "husksDead" ? Object.assign({}, l, { n: 4 }) : l));
    r.lessons31 = r.lessons31.map((l) => (l.kind === "husksDead" ? Object.assign({}, l, { n: 4 }) : l));
  },
  // compact: THE BREECH moved 6 m south along the east wall (z -50..-43.5), the feed ramp straight up its south end beside the entry, the rack
  // and wave 1 on it: the fight 5-12 m from the spawn instead of 16-19 m (the stall hypothesis: standing at range)
  compact() {
    f.boxes = f.boxes.filter((b) => b.job !== "climb");
    const br = f.boxes.find((b) => b.min[0] === 2.5 && b.max[0] === 7);
    br.min = [2.5, 0, -50]; br.max = [7, 2.5, -43.5];
    f.ramps[0].from = [5.25, -38.8]; f.ramps[0].to = [5.25, -43.5]; f.ramps[0].width = 3.5;
    f.pads[0].target = [5.5, 2.5, -47];
    const s = [{ at: [4.9, 2.5, -47.6], mode: "static" }, { at: [5.16, 2.5, -48.4], mode: "static" }];
    r.standIns = s; r.standIns31 = s;
    r.standIns36 = s.map((o) => Object.assign({ job: "marker", why: "diagnosis" }, o));
    const w1 = [[6.5, 2.5, -45], [3.2, 2.5, -45], [6.6, 2.5, -49], [3.2, 2.5, -49.5], [6.6, 2.5, -47], [4.4, 2.5, -49.6]];
    r.waveNodes43 = [w1, r.waveNodes43[1]]; r.highNodes = w1;
  },
  // r74: the rules meter's three REDs on THE RANGE (LOOPS 1, CLIMB, EXPOSE 62 %): THE BREECH carries its feed step (x 1.5..7), the ramp lands on it
  // flush, THE CARTRIDGE STACK stands free (x -5.2..-2.7, z -50.5..-49: a ring round it), wave 2 steps out from behind it
  r74() {
    f.boxes = f.boxes.filter((b) => b.job !== "climb");
    const br = f.boxes.find((b) => b.min[0] === 2.5 && b.max[0] === 7);
    br.min = [1.5, 0, -55];
    f.ramps[0].to = [1.97, -49.4];
    const st = f.boxes.find((b) => b.min[0] === -7 && b.max[0] === -4.5);
    st.min = [-5.2, 0, -50.5]; st.max = [-2.7, 2.4, -49];
    const w2 = [[-4.4, 0, -51.4], [-3.4, 0, -51.6]];
    r.nodes = w2; r.waveNodes43 = [r.waveNodes43[0], w2];
    // the third walk proof went round the old wall-hugging stack on its east side; now round the free stack's west side (x -6.1, 0.9 m clear of both)
    r.proofs[2].points = [[0, 0, -37], [-1.8, 0, -41], [-1.8, 0, -43.5], [-2.2, 0, -47.5], [-6.1, 0, -47.5], [-6.1, 0, -51.5], [-3, 0, -52], [0, 0, -53]];
  },
  // no stand-ins at all
  "no-standins"() { r.standIns = []; r.standIns31 = []; r.standIns36 = []; r.lessons = [r.lessons[0]]; r.lessons31 = [r.lessons31[0]]; },
  // wave 2 only (the husks of wave 1 on the floor in front of the press)
  "wave1-floor"() {
    r.waveNodes43 = [[[-1, 0, -44], [1, 0, -46], [3, 0, -45], [5, 0, -47]], r.waveNodes43[1]];
    r.highNodes = [];
  },
};
// "a+b" applies a then b
for (const n of name.split("+")) {
  if (!E[n]) { console.error("unknown experiment " + n + " (" + Object.keys(E).join(", ") + ")"); process.exit(2); }
  E[n]();
}
fs.writeFileSync(out, JSON.stringify(f, null, 1) + "\n");
console.log("wrote " + out + " (" + name + ")");
