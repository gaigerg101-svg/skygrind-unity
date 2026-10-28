// NIGHT74 S: does JSON.stringify(JSON.parse(file), null, 1) reproduce a room file (LF) byte for byte? (the patch scripts write rooms that way)
//   node Research/2026-10-27-wave74/S/roundtrip-check.cjs <room.json> [...]
const fs = require("fs");
for (const f of process.argv.slice(2)) {
  const raw = fs.readFileSync(f, "utf8").replace(/\r\n/g, "\n");
  const out = JSON.stringify(JSON.parse(raw), null, 1);
  const same = out === raw || out + "\n" === raw;
  console.log(f + ": " + (same ? "round trip EXACT" : "DIFFERS at " + [...out].findIndex((c, i) => c !== raw[i])) + " (trailing newline " + raw.endsWith("\n") + ")");
}
