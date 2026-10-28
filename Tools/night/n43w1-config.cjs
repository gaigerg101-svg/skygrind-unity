// NIGHT43 builder W1: writes W1's numbers into ITS OWN rows only (design.n43.parry43 / fusion43; gauntlet43's swingSeconds came in the shared
// commit), as text inside each row (L-049: no reformat of the file): a key already in the row has its value and its _why replaced in place, a
// new key is appended before the row's closing brace. Then copies the config to Assets/StreamingAssets/Design/ (the two stay byte-identical).
// Never touches "enabled" (the lead turns a row on after its frames). Re-runnable. Run from the worktree root: node Tools/night/n43w1-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..") + "/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

const rows = {
  parry43: [
    ["reachMult", 1.5, "parry42's volume (DECIDE42 D-3), kept as test 1 of three: the orb's surface within 1.5x orb.reflectRange (3 m) of the body's axis (the eye down to the capsule's lower sphere). parry43 reads its own copy so parry42 can stay off (D-10 open)."],
    ["mercyS", 0.1, "parry42's mercy (ULTRAKILL 1.03b's 3 mercy frames, doubled): a touching orb waits 0.1 s (12 steps) at the contact point before its damage; a press in that time still parries."],
    ["catchRadius", 1.5, "THE CATCH VOLUME's radius (m), test 2: ULTRAKILL catches projectiles in a 3 m sphere 3 m ahead (combat-web.md A1, Punch.cs); sized to the gauntlet's 2.4 m reach (NIGHT43.md W1: radius ~1.5 m centred ~2 m ahead). The orb's surface (orb.radius 0.35) inside it is caught: up to 2 + 1.5 + 0.35 = 3.85 m along the aim."],
    ["catchAhead", 2.0, "The catch sphere's centre, m ahead of the eye along the aim (NIGHT43.md W1: ~2 m). Its near edge reaches back to 0.5 m ahead of the eye, so nothing between the sphere and the body volume is missed."],
    ["windowS", 0.25, "THE TIME WINDOW, test 3 (combat-web.md A3: 'express the window as time (distance / closing speed), not metres, so fast and slow orbs feel the same'; the genre band for a deliberate parry is 150-200 ms of time to impact, NIGHT43.md W1 asks >= 0.2 s at 12 m/s): an orb that, flying on as it flies, touches the player within 0.25 s (30 steps; 3 m at 12 m/s) is caught from any side. 0.25 s sits just above the band because the press also has to cross input and frame latency."],
    ["bufferS", 0.15, "The early buffer (NIGHT43.md W1: 'a 0.15 s early buffer on the gauntlet swing too'; ULTRAKILL buffers a punch 6 physics frames, ~0.12 s, combat-web.md A2): a punch or a gauntlet swing that sent nothing back keeps catching for 0.15 s (18 steps) through parry42's orb buffer (Core/Parry42.cs), so a press made just before the orb reaches the volume still parries when it does."],
  ],
  fusion43: [
    ["speedMin", 14, "A melee hit is a FUSION when the player's horizontal speed at the hit is at least 14 m/s (NIGHT43.md W1: '>= ~14 m/s'): above the ride's 9-13 m/s and a sprint, under the dash's 24 m/s, so only a launched / boosted body or a dash reaches it."],
    ["dashAfterS", 0.25, "...or the hit lands during a dash or within 0.25 s (30 steps) after it ended (NIGHT43.md W1): the dash lasts 0.25 s (config dash.duration), so a press up to one dash-length late still fuses (the 'dash INTO it' lesson, iceline43)."],
    ["rideJumpS", 0.35, "...or within 0.35 s (42 steps) of a jump off a wall ride (NIGHT43.md W1: 'a ride jump-off'): about the wall jump's rise, so a jump off the wall into an enemy below fuses."],
    ["damageMult", 2, "A FUSION hit's damage x2 (NIGHT43.md W1: 'x2 damage'): the PULSE swing's 192 becomes 384 (before momentum: Combat.Damage's own melee momentum still rides on top), a husk-class body dies to one; the punch's 10 becomes 20."],
    ["bossDamageMult", 1.5, "On a boss body (Loadout.IsBoss34) the FUSION multiplies by 1.5 only: GG's boss band (DECIDE39 D-1: one to two minutes for a person, bot 40-70 s) is measured by `n43weap band --fusion` (THE ICEBREAKER's skilled pilot dashes in)."],
    ["knockSpeed", 14, "The FUSION knock, m/s away from the player (the gauntlet's 7 m/s doubled, weapons.gauntlet.knockback): the body is thrown, not pushed. A boss body ignores knocks (its own Knockback is empty)."],
    ["knockTime", 0.3, "Seconds of that knock (the gauntlet's 0.2 s x1.5): ~4 m of travel at 14 m/s with the slide's decay, out of melee reach."],
    ["staggerS", 0.6, "The FUSION stagger (NIGHT43.md W1: 'a 0.6 s stagger'): Enemy.Stun, the longer stagger wins; its attack in progress is cancelled. Boss bodies keep their own stagger rules (their Stun is empty)."],
    ["hitstopS", 0.22, "The FUSION freeze (NIGHT43.md W1: 'a hitstop tier above the gauntlet's (0.22 s)'): over impact42's gauntlet hit 0.16 s, big 0.20 s and its kill 0.22 s (equal to the kill: the fusion wins the step as its own tier); combat-web.md B10: an empowered dash impact 18-30 steps (0.15-0.25 s). ImpactRules' Fusion43 tier, out of the shared gap."],
    ["bossHitstopS", 0.28, "On a boss body (NIGHT43.md W1: 'boss 0.28 s'): over impact42's boss gauntlet hit 0.24 s and ULTRAKILL's parry TrueStop 0.25 s, under the perfect parry's 0.3 s."],
    ["perMinute", 0.08, "The FUSION freezes share the gauntlet's rolling-minute budget (impact42's gauntletPerMinute 0.08 = 4.8 s a minute; ImpactRules counts gauntlet + fusion freezes together): over it a FUSION freeze is cut to what is left (never a stall)."],
    ["reachBonus", 1.0, "While a FUSION is ready (the conditions above) the gauntlet's swing and the punch reach this many metres farther when nothing is in their normal reach (NIGHT43.md W1: 'a dash INTO an enemy ... the swing's reach +1 m while dashing'): at 24 m/s the dash covers 0.2 m a step, so the extra metre is ~5 steps of forgiveness on the press."],
    ["cooldownS", 0.5, "At most one FUSION in 0.5 s (60 steps): one dash (0.25 s + the 0.25 s after) gives one, so a punch chain inside it is not three bursts and three freezes."],
    ["holdS", 0.2, "THE HELD SWING (the lead's round 2, G19's 'a dash press 5 m+ away still misses'): a gauntlet swing that finds no body while a FUSION is ready waits up to 0.2 s (24 steps) and lands the moment a body comes into the FUSION reach (2.4 + 1 m). At 24 m/s the dash closes 4.8 m in 0.2 s, so a press at the dash's start lands on a body up to ~7-8 m ahead (`n43weap fusion`'s grid). The swing's cooldown runs from the press."],
  ],
  slug43: [
    ["knockSpeed", 18, "Every body the charged slug strikes and does not kill is thrown along the shot at 18 m/s (the charged knock today: 2 x the gauntlet's 7 = 14 m/s for 0.2 s, husks only, design.n31.charge.shotgun.knockMult): a cannon, not a push (combat-web.md B13: 'enemies launched'). Any kind: a boss body's Knockback is empty, so bosses stay put."],
    ["knockTime", 0.35, "Seconds of that throw (and of the blast's push): ~5 m of travel at 18 m/s with the knock's decay, out of the fight for a beat."],
    ["staggerS", 0.8, "The struck body's stun (Enemy.Stun, the longer wins): it cannot answer while it flies and lands (a parry stuns punch.staggerTime 1 s; the slug a little less, it also threw it)."],
    ["blastRadius", 2.0, "The impact BLAST's radius at the slug's end (the last body struck, or the wall), m to a body's hit sphere (the lead's card: ~2 m): the bodies packed behind or beside the target are caught."],
    ["blastDamage", 48, "The blast's base damage at its centre: a quarter of the slug's base 192 (8 pellets x 8 x damageMult 3). Combat.Damage's shotgun multipliers ride on it as on the slug (measured `n43weap slug`: the slug lands 624 on A, the blast 86-102 on the husks beside / behind), so the bodies packed around the target die with it (husk.hp 40) and a tank is hurt: the charged slug into a crowd is a small explosion."],
    ["blastEdge", 0.4, "The falloff: the blast deals blastDamage x 1 at the centre down to x 0.4 at blastRadius (linear)."],
    ["blastKnock", 10, "The blast pushes each body it hits outward from the impact at 10 m/s for knockTime: the crowd scatters."],
    ["blastStaggerS", 0.4, "The blast's stun on what it hits and does not kill (half the struck body's)."],
  ],
  torch43: [
    ["burnS", 3, "A body the TORCH hits burns 3 s after the last flame tick on it (the lead's card: ~3 s; DOOM Eternal's Flame Belch: burning is a state, combat-web.md C11)."],
    ["burnDps", 12, "The burn's damage per second (the TORCH itself: 6.5 per 0.1 s tick = 65 dps, weapons.torch): a body that leaves the cone keeps losing 36 HP over the 3 s, so a sweep across a crowd keeps killing after it."],
    ["burnTickS", 0.25, "Seconds between burn ticks (4 a second, 3 each): each tick is a Combat.Damage with weapon 'burn' and shock true (no freeze, no momentum; ImpactRules ignores a wave's share)."],
    ["igniteStaggerS", 0.3, "A body catching fire flinches 0.3 s (Enemy.Stun; once a burn, a refresh does not): the target changes state when it ignites (combat-web.md C11)."],
    ["rampS", 0.2, "The flame's reach grows over the first 0.2 s of a spray (combat-web.md C12 (d): 'the first 0.2 s of fire grows reach ... from 50 % to 100 %, so holding it feels like building power'); a pause of more than 1.5 flame ticks starts it again."],
    ["rampFrom", 0.6, "The reach at a spray's first tick: 0.6 x (4.2 m of the 7 m); 0.8 x at the second tick (0.1 s), the full 7 m from the third (0.2 s). Not combat-web's 0.5: at 0.5 the first tick (3.5 m) missed a husk 4 m away and its kill took 0.208 s instead of 0.175 s (`n43weap torch`); 0.6 keeps the close fight's first tick."],
  ],
};

function esc(s) { return s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"); }

for (const [row, keys] of Object.entries(rows)) {
  let start = raw.indexOf('"' + row + '": {');
  if (start < 0) throw new Error("row not found: " + row);
  for (const [k, v, why] of keys) {
    const end = raw.indexOf(eol + raw.slice(raw.lastIndexOf(eol, start) + eol.length, start) + "}", start); // the row's closing brace line (same indent)
    if (end < 0) throw new Error("no closing brace for " + row);
    let body = raw.slice(start, end);
    const kv = new RegExp('("' + esc(k) + '": )[^,\\r\\n]+(,?)');
    const kw = new RegExp('("' + esc(k) + '_why": )"(?:[^"\\\\]|\\\\.)*"');
    if (body.match(kv)) {
      body = body.replace(kv, (m, a, c) => a + JSON.stringify(v) + c).replace(kw, (m, a) => a + JSON.stringify(why));
      raw = raw.slice(0, start) + body + raw.slice(end);
      continue;
    }
    const ew = body.indexOf('"enabled_why":');
    const lineStart = raw.lastIndexOf(eol, start + ew) + eol.length;
    const indent = raw.slice(lineStart, start + ew);
    const insert = "," + eol + indent + JSON.stringify(k) + ": " + JSON.stringify(v) + "," + eol + indent + JSON.stringify(k + "_why") + ": " + JSON.stringify(why);
    raw = raw.slice(0, end) + insert + raw.slice(end);
  }
}
JSON.parse(raw); // still valid
fs.writeFileSync(file, raw);
fs.writeFileSync(copy, raw);
const d = JSON.parse(raw).design.n43;
for (const r of Object.keys(rows)) console.log(r, JSON.stringify(Object.fromEntries(Object.entries(d[r]).filter(([k]) => !k.endsWith("_why") && k !== "_why"))));
