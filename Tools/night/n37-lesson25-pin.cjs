// NIGHT37 (the lead, the pre-freeze suite's failures): (1) Lesson25Tests pins NIGHT33's and NIGHT34's orders, which name n33-span /
// n33-spire; NIGHT37's boss rows (loom37 / crown37, ON) swap those ids for n37-span / n37-spire, so those two pins run with N37 forced off
// (as NIGHT35 pinned NIGHT34's order), and a NIGHT37 assertion checks each ON boss row's swap in the six-level order. (2) builder S's
// rehearsal lesson prompt in gen-n37-iceline-boss.cjs was 7 words (Lesson25TextTests: at most 6): "THE STAG WAITS. LEARN THE LANES."
// Run once: node Tools/night/n37-lesson25-pin.cjs   (then bash Tools/night/n37s-regen.sh)
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
function patch(file, pairs) {
  let s = fs.readFileSync(root + file, "utf8");
  const crlf = s.includes("\r\n");
  if (crlf) s = s.replace(/\r\n/g, "\n");
  for (const [a, b] of pairs) {
    const n = s.split(a).length - 1;
    if (n !== 1) throw new Error(file + ": anchor found " + n + " times: " + a.slice(0, 80));
    s = s.replace(a, b);
  }
  if (crlf) s = s.replace(/\n/g, "\r\n");
  fs.writeFileSync(root + file, s);
  console.log("patched " + file);
}
const I = "            ";
patch("Assets/Tests/EditMode/Lesson25Tests.cs", [
  [I + "// NIGHT33: with kit33 on, the same five levels as their n33 files, in the same order (NIGHT34's lines34 swap pinned off here)\n",
   I + "bool was37 = N37Blocks.ForceOff;\n" +
   I + "N37Blocks.ForceOff = true; // NIGHT37: the boss rows (loom37 / crown37 ON) swap n33-span / n33-spire for n37-span / n37-spire; NIGHT33's and NIGHT34's orders are pinned with them forced off\n" +
   I + "try\n" + I + "{\n" +
   I + "// NIGHT33: with kit33 on, the same five levels as their n33 files, in the same order (NIGHT34's lines34 swap pinned off here)\n"],
  [I + "finally { N35Blocks.ForceOff = was35; }\n",
   I + "finally { N35Blocks.ForceOff = was35; }\n" +
   I + "}\n" +
   I + "finally { N37Blocks.ForceOff = was37; }\n"],
  [I + "    Assert.That(six.Count == 6 && six[5] == (string)c.Get(\"design.n35.campaign6.finaleId\") && Curriculum25.Next(c, all30, six[5]) == null, Is.True, \"NIGHT35: six levels, the finale last: \" + string.Join(\",\", six));\n" + I + "}\n",
   I + "    Assert.That(six.Count == 6 && six[5] == (string)c.Get(\"design.n35.campaign6.finaleId\") && Curriculum25.Next(c, all30, six[5]) == null, Is.True, \"NIGHT35: six levels, the finale last: \" + string.Join(\",\", six));\n" + I + "}\n" +
   I + "// NIGHT37: each boss row that is ON swaps its levelFrom for its levelTo in the order (and its levelFrom is gone); the six stay six\n" +
   I + "if (!N32Blocks.ForceOff && !N33Blocks.ForceOff && !N34Blocks.ForceOff && !N35Blocks.ForceOff && !N36Blocks.ForceOff && !N37Blocks.ForceOff && c.Design(\"n35.campaign6.enabled\"))\n" +
   I + "{\n" +
   I + "    var order37 = Curriculum25.Order(c, all30);\n" +
   I + "    foreach (string row in new[] { \"wick37\", \"stag37\", \"loom37\", \"crown37\" })\n" +
   I + "        if (c.Design(\"n37.\" + row + \".enabled\"))\n" +
   I + "            Assert.That(order37.Contains((string)c.Get(\"design.n37.\" + row + \".levelTo\")) && !order37.Contains((string)c.Get(\"design.n37.\" + row + \".levelFrom\")) && order37.Count == 6, Is.True, \"NIGHT37: \" + row + \" swaps its level: \" + string.Join(\",\", order37));\n" +
   I + "}\n"],
]);
patch("Tools/night/gen-n37-iceline-boss.cjs", [
  ["l.prompt = \"THE STAG IS AHEAD. LEARN THE LANES.\";", "l.prompt = \"THE STAG WAITS. LEARN THE LANES.\"; // NIGHT37 lead: at most 6 words (Lesson25TextTests)"],
]);
