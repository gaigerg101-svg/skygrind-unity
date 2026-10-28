// NIGHT34 builder B (the REA addendum): adds three sub-switches and their numbers to design.n34.combo34 in Design/unity-config.json, ON (the lead's
// order), by inserting text before combo34's closing brace only: no other row is re-serialised (L-049). Run from the repo (or worktree) root:
//   node Tools/night/n34b-rea-combo-config.cjs   (refuses to run twice), then cp to Assets/StreamingAssets/Design/unity-config.json and cmp.
// Sources: Research/2026-10-11-wave34/research/rea-ultrakill.md (ULTRAKILL's shipped Assembly-CSharp.dll read with ILSpy; numbers and rule shapes
// only, no code), sections "Weapon switching" and "The Whiplash".
const fs = require("fs");
const path = require("path");
const file = path.join(process.cwd(), "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const c = d.design.n34.combo34;
if (!c) throw new Error("no design.n34.combo34");
if (c.arsenal !== undefined) throw new Error("design.n34.combo34 already has the REA sub-switches");

// the combo34 object's closing brace (a scanner that skips JSON strings: the _why texts contain braces)
const key = "\"combo34\": {";
const start = raw.indexOf(key, raw.indexOf("\"n34\": {"));
if (start < 0) throw new Error("no combo34 row text");
let i = start + key.length, depth = 1, inStr = false;
for (; i < raw.length && depth > 0; i++) {
  const ch = raw[i];
  if (inStr) {
    if (ch === "\\") i++;
    else if (ch === "\"") inStr = false;
  } else if (ch === "\"") inStr = true;
  else if (ch === "{") depth++;
  else if (ch === "}") depth--;
}
const close = i - 1; // index of combo34's "}"
let lineStart = raw.lastIndexOf("\n", close) + 1;
const before = raw.slice(0, lineStart).replace(/\s+$/, ""); // up to the last property, no trailing newline
const ind = "        ";
const web = "The web has no such number: it has no weapon-switch rewards in its style meter (no ARSENAL, no QUICKDRAW) and no whip.";
const src = "Research/2026-10-11-wave34/research/rea-ultrakill.md";
const rows = [
  ["arsenal", true],
  ["arsenal_why", "Sub-switch (under the combo34 bit): ARSENAL. A kill on an enemy that was hit by two or more different owned guns (any time, not only inside a bend; the whip's hit not counted) pays arsenalStyle and emits arsenal34 (GG's sound hook). Rule shape from " + src + " 'Weapon switching' (EnemyIdentifier.cs: +50 style when an enemy dies having been hit by more than one weapon, per enemy, no time window). " + web],
  ["arsenalStyle", 20],
  ["arsenalStyle_why", "Style points for ARSENAL, before the meter's multipliers. ULTRAKILL pays 50 where a kill is about 50-100 (" + src + "); scaled to SKYGRIND's units by our kill award, style.base.huskKill 30 (PortExport/config.json): 50 / 75 (the middle of 50-100) x 30 = 20. " + web],
  ["quickdraw", true],
  ["quickdraw_why", "Sub-switch (under the combo34 bit): QUICKDRAW. A hit with the gun in hand within quickdrawS after it came into hand (the loadout's weaponRaise) pays quickdrawStyle, once per swap, and emits quickdraw34 (GG's sound hook). Rule shape from " + src + " 'Weapon switching' (Shotgun.cs, RevolverBeam.cs, Coin.cs: +50 style for a hit while the weapon's equip animation still plays). " + web],
  ["quickdrawS", 0.25],
  ["quickdrawS_why", "Seconds after the raise (whole 120 Hz steps) a hit still counts as QUICKDRAW. ULTRAKILL's window is its equip animation; " + src + " gives no seconds for it, so 0.25 s is the lead's start (about ULTRAKILL's 0.25 s tap / wheel threshold, WeaponWheelTime), to be judged in the Player. " + web],
  ["quickdrawStyle", 20],
  ["quickdrawStyle_why", "Style points for QUICKDRAW, before the meter's multipliers: ULTRAKILL's 50 scaled as arsenalStyle (50 / 75 x style.base.huskKill 30 = 20). " + web],
  ["whipReady", true],
  ["whipReady_why", "Sub-switch (under the combo34 bit): a whip latch (whip33Latch) sets the Pulse Gauntlet's punch cooldown (and its swing's cycle in hand) to at most whipReadyS, so grapple -> punch always links (Loadout.ReadyPunch34). Rule shape from " + src + " 'The Whiplash' (HookArm.cs: throwing the hook sets fistCooldown to at most 0.1). " + web],
  ["whipReadyS", 0.1],
  ["whipReadyS_why", "Seconds (whole 120 Hz steps: 12) the punch waits at most after a whip latch. " + src + " 'The Whiplash': 0.1 (HookArm.cs, fistCooldown capped at 0.1f). " + web],
];
const text = rows.map(([k, v]) => ind + JSON.stringify(k) + ": " + JSON.stringify(v)).join("," + eol);
const out = before + "," + eol + text + eol + raw.slice(lineStart);
const back = JSON.parse(out);
if (back.design.n34.combo34.arsenal !== true || back.design.n34.combo34.whipReadyS !== 0.1) throw new Error("the rows did not land");
fs.writeFileSync(file, out);
console.log("design.n34.combo34: arsenal, quickdraw, whipReady (on) and their numbers added; nothing else re-serialised");
