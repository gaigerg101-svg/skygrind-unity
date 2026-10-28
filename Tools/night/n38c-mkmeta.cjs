// NIGHT38 builder C2: the .meta for a new StreamingAssets level file (the DefaultImporter form n37-spire.json.meta has, a fresh guid), unless one exists.
// Usage: node Tools/night/n38c-mkmeta.cjs <file> [<file> ...]
const fs = require("fs");
const crypto = require("crypto");
for (const f of process.argv.slice(2)) {
  const m = f + ".meta";
  if (fs.existsSync(m)) { console.log("exists", m); continue; }
  const lines = ["fileFormatVersion: 2", `guid: ${crypto.randomBytes(16).toString("hex")}`, "DefaultImporter:", "  externalObjects: {}", "  userData: ", "  assetBundleName: ", "  assetBundleVariant: ", ""];
  fs.writeFileSync(m, lines.join("\n"));
  console.log("wrote", m);
}
