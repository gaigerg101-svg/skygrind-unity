// NIGHT65 (the lead's; FORGE65 DECISIONS D-9, rewritten by redteam.md 12: "measured from the room files' commits and diffs (pieces with jobs
// added per room per round, rounds reached), not self-tagged minutes"). For each rebuilt room it walks the commits that touched
// Design/levels/n65/rooms/<id>.json on the current branch (oldest first; the first is compared with Design/levels/n65/rooms-n52/<id>.json, the
// n52 room) and prints per commit the pieces ADDED, MOVED (the same piece, another shape or place) and DROPPED, over boxes / ramps / pads /
// pits / tunnels / grapple / lights / props. A piece is named by the first capitalised phrase of its `why` ("THE BREECH"), else its list
// and index; every piece's `job` is counted (a piece with no job is printed). Read-only.
//   node Tools/night/n65-d9.cjs [rooms=a2,a3,a5,b1,a8,a4]
const { execFileSync } = require("child_process");
const fs = require("fs");

const rooms = (process.argv[2] || "a2,a3,a5,b1,a8,a4").split(",");
const LISTS = ["boxes", "ramps", "pads", "pits", "tunnels", "grapple", "lights", "props"];
const git = (args) => execFileSync("git", args, { encoding: "utf8", maxBuffer: 64 << 20 });

function pieces(room) {
  const out = new Map();
  for (const l of LISTS) {
    (room[l] || []).forEach((p, i) => {
      const m = /\b(THE [A-Z0-9' -]{2,}?)(?=[ (:,.]|$)/.exec(p.why || "") || null;
      let name = l + ":" + (m ? m[1].trim() : "#" + i);
      while (out.has(name)) name += "'";
      const shape = JSON.stringify(Object.fromEntries(Object.entries(p).filter(([k]) => k !== "why" && k !== "job")));
      out.set(name, { shape, job: p.job || null });
    });
  }
  return out;
}

let grand = { added: 0, moved: 0, dropped: 0, commits: 0 };
for (const id of rooms) {
  const path = "Design/levels/n65/rooms/" + id + ".json";
  const base = "Design/levels/n65/rooms-n52/" + id + ".json";
  if (!fs.existsSync(base)) { console.log(id + ": no " + base); continue; }
  let prev = pieces(JSON.parse(fs.readFileSync(base, "utf8")));
  const shas = git(["log", "--reverse", "--format=%h %s", "--", path]).trim().split("\n").filter(Boolean);
  console.log("== " + id + " (" + shas.length + " commits; the n52 room: " + prev.size + " pieces)");
  let tot = { added: 0, moved: 0, dropped: 0, changing: 0 };
  for (const line of shas) {
    const sha = line.split(" ")[0];
    let cur;
    try { cur = pieces(JSON.parse(git(["show", sha + ":" + path]))); } catch (e) { console.log("  " + sha + " unreadable"); continue; }
    const added = [...cur.keys()].filter((k) => !prev.has(k));
    const dropped = [...prev.keys()].filter((k) => !cur.has(k));
    const moved = [...cur.keys()].filter((k) => prev.has(k) && prev.get(k).shape !== cur.get(k).shape);
    const nojob = [...cur.entries()].filter(([k, v]) => !v.job && !k.startsWith("lights:")).map(([k]) => k); // lights carry no job key (R8 counts solids)
    if (added.length + moved.length + dropped.length) tot.changing++;
    tot.added += added.length; tot.moved += moved.length; tot.dropped += dropped.length;
    console.log("  " + sha + " +" + added.length + " ~" + moved.length + " -" + dropped.length + " (" + cur.size + " pieces" + (nojob.length ? ", NO JOB: " + nojob.join(", ") : "") + ") " + line.slice(sha.length + 1, sha.length + 90));
    prev = cur;
  }
  console.log("  " + id + ": added " + tot.added + ", moved " + tot.moved + ", dropped " + tot.dropped + " over " + tot.changing + " changing commits; now " + prev.size + " pieces");
  grand.added += tot.added; grand.moved += tot.moved; grand.dropped += tot.dropped; grand.commits += tot.changing;
}
console.log("n65-d9: rooms " + rooms.join(",") + ": added " + grand.added + ", moved " + grand.moved + ", dropped " + grand.dropped + " over " + grand.commits + " changing commits (the share of pieces added or moved: " + Math.round(100 * (grand.added + grand.moved) / Math.max(1, grand.added + grand.moved + grand.dropped)) + " %)");
