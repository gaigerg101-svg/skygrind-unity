// NIGHT55 builder R (P2-clear, design.n55.clear55, a view row): writes the row's numbers into both config copies (Design/unity-config.json and
// Assets/StreamingAssets/Design/unity-config.json, byte-identical). Only design.n55.clear55.numbers changes (the row stays `enabled: false`: the
// lead flips it); the parsed result is checked against the input with that one object put back. The line edit keeps every other byte.
//   node Tools/night/n55-r-clear-config.cjs [--dry]
const fs = require("fs");
const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const before = JSON.parse(raw);
const row = before.design && before.design.n55 && before.design.n55.clear55;
if (!row) throw new Error("no design.n55.clear55");
if (row.enabled !== false) throw new Error("clear55 is not off");
if (JSON.stringify(row.numbers) !== "{}") throw new Error("clear55.numbers is not empty (already run?)");

const numbers = {
  _why: "NIGHT55 R (P2-clear; beyond.md #2, Neon White's level-end card): a combat room's clear (Campaign24's roomClear, kind room) holds the world for holdS of real time (the loop's freeze: no sim step runs, the views stop, the look stays live; one hold per clear, never extended by a second clear inside it; none with HIT FREEZE off) while the HUD's card shows the room's time against its par. The par is data: the level file's campaign.rooms[].par (the plan's room row), carried on the roomClear event (RoomEvent.par); the time is the event's own (the room's fight steps x dt). The card's layout and life are Unity-only (UnityTuning.N55R.cs).",
  holdS: 0.5,
  holdS_why: "s of real time the world holds at a combat room's clear: the slate's 0.4-0.6 s band (P2-clear doneWhen), the middle of it. Longer than any kill freeze (move54's gun kills 0.05-0.12 s; design.impact.budget.maxFreeze 0.5 caps a single hitstop), so the room's end reads as its own beat, not a bigger kill: Neon White holds its finish ~0.5 s before the card lands; ULTRAKILL's major hitstop is 0.25 s (FEEL-DESIGN.md section 2). The web has no such number: it has no rooms (the campaign is Unity-only, NIGHT24).",
};

const lines = raw.split(eol);
const open = lines.findIndex((l) => l.trim() === '"clear55": {');
if (open < 0) throw new Error("no clear55 block line");
const ind = lines[open].match(/^ */)[0].length;
const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
const at = lines.findIndex((l, k) => k > open && k < end && l.match(/^ */)[0].length === ind + 2 && /^"numbers": \{\},?$/.test(l.trim()));
if (at < 0) throw new Error("clear55's own numbers line not found");
const pad = " ".repeat(ind + 2);
const comma = lines[at].trim().endsWith(",") ? "," : "";
const body = JSON.stringify(numbers, null, 2).split("\n").map((l, i) => (i === 0 ? l : pad + l));
body[0] = pad + '"numbers": ' + body[0];
body[body.length - 1] += comma;
lines.splice(at, 1, ...body);
const out = lines.join(eol);

const after = JSON.parse(out);
if (JSON.stringify(after.design.n55.clear55.numbers) !== JSON.stringify(numbers)) throw new Error("the numbers did not land");
after.design.n55.clear55.numbers = {};
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something else changed");
if (process.argv.includes("--dry")) { console.log("DRY RUN: clear55.numbers holdS " + numbers.holdS); process.exit(0); }
for (const f of files) fs.writeFileSync(f, out);
console.log("design.n55.clear55.numbers written (holdS " + numbers.holdS + "); both copies");
