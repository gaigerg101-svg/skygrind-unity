// NIGHT29 (builder R): write a Unity .meta (the minimal form this repo's scripts use: format version + a fresh guid) for each file path given,
// unless the .meta exists. Usage: node Tools/night/n29-mkmeta.cjs <file> [<file> ...]
const fs = require("fs");
const crypto = require("crypto");
for (const f of process.argv.slice(2)) {
  const m = f + ".meta";
  if (fs.existsSync(m)) { console.log("exists", m); continue; }
  fs.writeFileSync(m, `fileFormatVersion: 2\nguid: ${crypto.randomBytes(16).toString("hex")}`);
  console.log("wrote", m);
}
