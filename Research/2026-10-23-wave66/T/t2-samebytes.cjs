// NIGHT66 T2: two level files compared apart from their names (name, _why, generatedBy): the all-old assembly with b2's slot against the
// tracked n65-foundry.json (made before T2, b2 then a kept room).   node Research/2026-10-23-wave66/T/t2-samebytes.cjs <a.json> <b.json>
const fs = require("fs");
const [a, b] = process.argv.slice(2).map((p) => JSON.parse(fs.readFileSync(p, "utf8")));
const strip = (o) => { const c = Object.assign({}, o); delete c.name; delete c._why; delete c.generatedBy; return JSON.stringify(c); };
const sa = strip(a), sb = strip(b);
const keys = [...new Set(Object.keys(a).concat(Object.keys(b)))].filter((k) => !["name", "_why", "generatedBy"].includes(k) && JSON.stringify(a[k]) !== JSON.stringify(b[k]));
console.log("t2-samebytes: " + (sa === sb ? "EQUAL apart from name / _why / generatedBy" : "DIFFER in " + keys.join(", ")) + " (" + sa.length + " / " + sb.length + " chars of the rest); generatedBy: " + String(a.generatedBy).split(";")[0].slice(0, 160) + " | vs | " + String(b.generatedBy).split(";")[0].slice(0, 160));
process.exit(sa === sb ? 0 : 1);
