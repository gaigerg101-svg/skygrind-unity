# I2 fix round: no punch into a parry (builder T, NIGHT68; Logs/n68/verdict-I2.md)

The critic's FAIL: on kb3, the 40 hp husks died to PARRY punches (43-53 dealt), and a parry never launches (Combat.cs
`launch = launch36 != null && !parry && Wants`). Shipped kb3's M3 was RED on 2 of 3 seeds.

The lead's call: the pilot and the trigger must not punch a body whose punch would be a parry. Line 2 is judged on "a launched body that
survived the punch" on shipped kb3, seeds 1-3; the pit half is judged on the rebuilt kb3. This round is its own slice and is not in
tonight's room verdicts.

## What changed (Tools/SimHarness only)
- `Harness.N68.Parry.cs` V68ParryWhy: would a punch on this body now be a parry? It reads the game's two tests without changing them.
  - The body is winding up: `CombatMath.IsParry(windupSteps)`.
  - Or parry36's early buffer would take it: Parry36.cs Imminent36. That means a plain Husk, not winding up, staggered or knocked;
    grounded, not leaping, not a stand-in; its brain letting it attack; its cooldown at or under parry36's bufferSteps; and the player
    inside its meleeRange.
- V68PunchParryWhy: the punch hits the nearest body in its sweep, not always the target. On shipped kb3 seed 3, the first GREEN try's
  punch went to another husk that was winding up. So the punch is held while the target, or any body in the punch's reach (range + 0.5 m,
  cone + 20 deg), would be parried.
- The probe (`n65verb launch`):
  - A target whose punch would be a parry now is skipped at selection. The reason is printed: "skipped: ... its punch would be a parry
    now: ...".
  - At the punch the press is held while it would be a parry.
  - The punch is never pressed off speed: it waits for the dash to start, and dashes again once the dash's window is spent.
  - The punch line prints "held N steps while it would be a parry (why), dashes re-made M".
- The verb pilot: the launch trigger skips such a body (the skipped line gives "(would parry: ...)"), and the episode holds its punch while
  it holds. The episode's 0.8 s still bounds it. The launch line prints "punch held N steps while it would be a parry".

## The pair (Logs/n68/I2fix/)
- RED (`kb3-shipped-seedN-red.txt`, abc64720, the I2 rule without the parry hold):
  - Seed 1: dealt 24.4, survived, launched (GREEN).
  - Seeds 2 and 3: dealt 48.7 / 43.1, KILLED, a PARRY, launch36 0 (RED).
- GREEN (`kb3-shipped-seedN-green.txt`):
  - Seed 1: as before, GREEN, thrown 3.7 m to a wall.
  - Seed 2: held 219 steps while the target wound up, then 39 dashes re-made. Dealt 16.0, survived, not a parry; launch36 1, thrown 1.5 m.
    GREEN.
  - Seed 3: held 129 steps while another husk in reach (#2) wound up. The punch then went to that other body, not the target, and launched
    it ("no punch landed on the target ... not a parry"). launch36 1; the burst killed 2 bodies, the target among them. GREEN.
  - So a launched body that survived the punch on 3 of 3 seeds. On seed 3 it is not the probe's chosen target, which is said in the file.
  - In a pit / in lava: no on all three. Shipped kb3 has no pit; the lead moved that half to the rebuilt room.
- `pair-green.txt`: the planted pair is unchanged. A (hp 11) is skipped, B (hp 400) is launched 12.8 m, and nothing is held: the pair's
  attacks are held off.

## The whole-level walk
- `flow-i2fix.txt` / `flow-i2fix-diff.txt`: EQUAL to the g7 slice's walk on all five levels; against flow-base-a0 only I3's named
  n52-iceline change (n46flow without --verbs65 builds no verb pilot; the probe is not in the walk).

Not checked: the verb pilot's skip v2 on a launch room with the parry hold (the episode's 0.8 s may now end more episodes unpunched); the
held steps' count on more seeds.
