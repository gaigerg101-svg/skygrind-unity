// NIGHT31 builder W: adds the numbers of design.n31.charge (item 2a + P0 a; the row stays off) to Design/unity-config.json. The web has no
// charge shot, so every number is Unity-side (CLAUDE.md rule 5). Run from the repo (or worktree) root:
//   node Tools/night/n31w-config-charge.cjs     (refuses to run twice; edits only the charge block's text; the lead syncs StreamingAssets)
const fs = require("fs");
const path = require("path");
const file = path.resolve(__dirname, "..", "..", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const c = d.design && d.design.n31 && d.design.n31.charge;
if (!c) throw new Error("design.n31.charge missing (the shared commit adds it)");
if (c.holdSeconds !== undefined) throw new Error("design.n31.charge already has its numbers");
const anchor = '      "charge": {' + eol + '        "_why": ' + JSON.stringify(c._why) + "," + eol + '        "enabled": false' + eol + "      }";
const at = raw.indexOf(anchor);
if (at < 0 || raw.indexOf(anchor, at + 1) >= 0) throw new Error("the charge block's text is not where it was expected (once)");
const block = {
  _why: c._why,
  enabled: false,
  holdSeconds: 0.4,
  holdSeconds_why: "RT held this long (whole steps) on the KESTREL or the THUNDERCLAP starts a charge; a tap fires at the press as today (DESIGN31 A, NIGHT27's 0.4 s).",
  autoReleaseSeconds: 0.5,
  autoReleaseSeconds_why: "P0 a: a FULL charge still held this long fires itself (NIGHT27 waited for a release forever, so a held trigger killed nothing: the smoke's virtual pad and the attract bot failed). Long enough to aim a full charge, short enough that holding RT never stalls the gun more than hold + charge + this (1.4 s on the KESTREL). Guess, logged.",
  knockHusksOnly: true,
  knockHusksOnly_why: "R7 (the 15 s crowd-control rule): the slug's forward knock acts on husks only, never a boss (a knock on a husk is the gun's own hit, as the THUNDERCLAP's volley already knocks).",
  meleeCancels: true,
  meleeCancels_why: "R8 (the w29 fire / melee lockout): a melee press while a charge builds cancels it, nothing spent.",
  revolver: {
    _why: "The KESTREL's charged shot (DESIGN31 A): charge fullSeconds after the hold, the release (or autoReleaseSeconds at full) fires damageMult x a normal shot through `pierce` bodies (NIGHT30's numbers), then cooldownSeconds before the next charge (a normal shot is never blocked). fullSeconds 0.5 from R3 (ULTRAKILL's piercer about 0.5 s); cooldown 4 s a guess (R3 piercer 2.5 s, NIGHT27 10 s).",
    fullSeconds: 0.5,
    damageMult: 4.0,
    pierce: 3,
    cooldownSeconds: 4.0,
  },
  shotgun: {
    _why: "The THUNDERCLAP's charged blast (DESIGN31 A): charge fullSeconds after the hold, the release fires the pellet volley at damageMult x with a forward knock (knockMult x the gauntlet's; husks only, R7), then cooldownSeconds. 0.7 s and 6 s are guesses (NIGHT27 0.8 s / 14 s).",
    fullSeconds: 0.7,
    damageMult: 3.0,
    knockMult: 2.0,
    cooldownSeconds: 6.0,
  },
};
const text = JSON.stringify(block, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, at) + '      "charge": ' + text + raw.slice(at + anchor.length);
fs.writeFileSync(file, out);
const back = JSON.parse(fs.readFileSync(file, "utf8")).design.n31.charge;
console.log("design.n31.charge: hold " + back.holdSeconds + " s, KESTREL " + back.revolver.fullSeconds + " s / " + back.revolver.cooldownSeconds + " s, THUNDERCLAP " + back.shotgun.fullSeconds + " s / " + back.shotgun.cooldownSeconds + " s, auto release " + back.autoReleaseSeconds + " s; enabled " + back.enabled);
