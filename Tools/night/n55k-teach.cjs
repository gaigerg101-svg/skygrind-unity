// NIGHT55 builder K (slate P1-teachText, row design.n55.brief55): the level text read from a reel's telemetry (verbs37 on each level).
// For each line the centre card shows (UI/HUD 33/Teach card 43/Verb + Rule, alpha >= 0.3): its frames on screen, its words, whether it carries
// a bound glyph (a button name in the text, or a pad glyph sprite: TMP's private-use character U+E000-U+F8FF in the text), whether it is the basic jump ("B to jump") outside level 1; and every frame a toast or the hint (UI/Meta overlay/*) overlaps
// the room counter's plate (UI/HUD 33/Counter 43/*), with the overlap in px. The thresholds: words <= 6, a glyph, <= 360 frames a line, no
// basic jump after level 1, 0 px overlap.
//   node Tools/night/n55k-teach.cjs <telemetry.jsonl> <level: foundry|iceline|kiln|span|spire> [<telemetry.jsonl> <level> ...]
const fs = require("fs");
const args = process.argv.slice(2);
if (!args.length || args.length % 2) throw new Error("usage: n55k-teach.cjs <telemetry.jsonl> <level> ...");
const BUTTONS = /\b(A|B|X|Y|ZL|ZR|L|R|LB|RB|LT|RT|LS|RS|L3|R3|SPACE|SHIFT|CTRL|ALT|TAB|LMB|RMB|MMB|MOUSE\d?|Q|E|F|C|V|G|Z|LEFT_CLICK|RIGHT_CLICK|MIDDLE_CLICK|L-SHIFT|R-SHIFT|L-ALT|R-ALT|CAPS|ENTER|WHEEL)\b/;
// NIGHT57 V (V2-brief55): a reel run without a pad (n57-r1 verbs37: "pad": false) shows the keyboard's names; a two-word name ("LEFT CLICK",
// Bindings' fire / altFire) is ONE glyph (the rule: "a glyph is a word"), so it is joined before the words are counted and the glyph is looked for
const KEY_NAMES = [["LEFT CLICK", "LEFT_CLICK"], ["RIGHT CLICK", "RIGHT_CLICK"], ["MIDDLE CLICK", "MIDDLE_CLICK"]];
const joinKeys = (t) => KEY_NAMES.reduce((s, [a, b]) => s.split(a).join(b), t);
let allOk = true;
for (let a = 0; a < args.length; a += 2) {
  const file = args[a], level = args[a + 1];
  const rows = fs.readFileSync(file, "utf8").trim().split("\n").map((l) => JSON.parse(l)).filter((r) => r.type === "frame" && r.ui);
  const lines = []; // { text, frames, first }
  let cur = null, overlapFrames = 0, overlapMax = 0, cardFrames = 0;
  for (const r of rows) {
    const verb = r.ui.find((u) => u.path === "UI/HUD 33/Teach card 43/Verb/Text");
    const rule = r.ui.find((u) => u.path === "UI/HUD 33/Teach card 43/Rule/Text");
    const shown = verb && verb.text && verb.alpha >= 0.3;
    if (shown) {
      cardFrames++;
      const t = (verb.text + (rule && rule.text && rule.alpha >= 0.3 ? " " + rule.text : "")).trim();
      const same = cur && (t.startsWith(cur.text) || cur.text.startsWith(t) || t === cur.text);
      if (same) {
        cur.frames++;
        if (t.length > cur.text.length) cur.text = t;
        cur.seen[t] = (cur.seen[t] || 0) + 1;
      } else {
        cur = { text: t, frames: 1, first: r.f, seen: { [t]: 1 } };
        lines.push(cur);
      }
    } else cur = null;
    // the counter's plate vs every meta overlay card (the hint, the toasts)
    const counter = r.ui.filter((u) => u.path.startsWith("UI/HUD 33/Counter 43/") && u.alpha > 0.05);
    const cards = r.ui.filter((u) => u.path.startsWith("UI/Meta overlay/") && u.alpha > 0.05 && !u.path.slice("UI/Meta overlay/".length).includes("/"));
    if (counter.length && cards.length) {
      const cx0 = Math.min(...counter.map((u) => u.rect[0])), cy0 = Math.min(...counter.map((u) => u.rect[1]));
      const cx1 = Math.max(...counter.map((u) => u.rect[0] + u.rect[2])), cy1 = Math.max(...counter.map((u) => u.rect[1] + u.rect[3]));
      let worst = 0;
      for (const c of cards) {
        const ox = Math.min(cx1, c.rect[0] + c.rect[2]) - Math.max(cx0, c.rect[0]), oy = Math.min(cy1, c.rect[1] + c.rect[3]) - Math.max(cy0, c.rect[1]);
        if (ox > 0 && oy > 0) worst = Math.max(worst, oy);
      }
      if (worst > 0) { overlapFrames++; overlapMax = Math.max(overlapMax, worst); }
    }
  }
  // the same text shown again later: one entry per showing; the verdict per showing
  console.log(`== ${file} (${level}): ${rows.length} frames, the card shown in ${cardFrames}`);
  let bad = 0;
  for (const l of lines) {
    // the line as it stood: the text seen in the most frames (the reveal's partial strings and a stale tail are a few frames each)
    l.text = Object.entries(l.seen).sort((p, q) => q[1] - p[1])[0][0];
    const words = joinKeys(l.text).split(/\s+/).filter((w) => /[A-Za-z0-9-]/.test(w)).length;
    const glyph = BUTTONS.test(joinKeys(l.text)) || /[-]/.test(l.text);
    const jump = level !== "foundry" && /^(press\s+)?(\S+\s+)?to jump\.?$/i.test(l.text.trim());
    const okL = words <= 6 && glyph && l.frames <= 360 && !jump;
    if (!okL) bad++;
    console.log(`  ${okL ? "ok " : "BAD"} f${l.first} ${String(l.frames).padStart(4)} frames, ${words} words, glyph ${glyph ? "yes" : "NO"}${jump ? ", BASIC JUMP after level 1" : ""}: ${JSON.stringify(l.text)}`);
  }
  const okO = overlapFrames === 0;
  console.log(`  lines ${lines.length}, bad ${bad}; a card over the counter in ${overlapFrames} frames (worst ${overlapMax.toFixed(1)} px)${okO ? "" : " BAD"}`);
  if (bad || !okO) allOk = false;
}
console.log(allOk ? "GREEN teachText" : "RED teachText");
