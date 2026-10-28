# NIGHT45 report: the arena identity night (2026-10-17, 19:05-~21:00)

**Outcome.** Three arenas you meet next were rebuilt from identity cards (a place, a 30-second problem, a landmark, a reason for
every solid). All three are ON in the repo and checked in the Player: their reels GREEN, their entrance and mid-fight frames looked at
in colour and greyscale, and verbs37 passes on all five levels (26 / 20 / 25 / 30 / 30). The whole test suite ran for the first time
in four nights: Core 1114 / 1114, plus the Game batches NIGHT44 missed, 389 / 389. **The new play build is
`Builds/n45-r4/SKYGRIND.exe`** (these three arenas). It passed verbs37 on all five levels (26 / 20 / 25 / 30 / 30) and the smoke: "DONE:
all steps passed" at 20:26, 0 NRE. That is the first full smoke pass since NIGHT43. Two earlier smokes tonight (the n44-r1 control and
n45-r3) failed the same four pad steps; a "Pro Controller" was listed both times, and also during the pass. The pass began after 197 s
of no input at the machine, against 85 s before the n45-r3 run. Why the earlier runs failed is not checked.

## The rebuilt arenas
- **Level 1, b1 THE YARD -> THE SPILL.** The crane dropped its ladle, and the slag ran across the casting yard as a 12 m river, wall
  to wall. Husks come over two 3 m bridges in single file while spitters lob from the tipped ladle's rim. You ride the crane rail
  beside a bridge to pierce the file or punch husks into the slag, then jump off the rail onto the rim. Wave 3 comes from the bank you
  entered. The landmark: the ladle's 6.5 m bowl with its glowing lip and the room's one amber lamp. *Tell a friend:* "A giant ladle
  tipped over and flooded the yard with slag; the husks come over two little bridges and you ride the crane rail beside them and
  punch them into it." Entrance frame: `Logs/feel/n45-r3/a1yard/a145/shot-N30-A145-yard-entrance.png`.
- **Level 2, g7 THE MIX -> THE THAW LOCK.** The ice line carried coal. At this lock the keepers burned it in a grate under the
  channel so the lock never froze. Three burning pits split the room, and walkers cross only on two 3 m bridges. The landmark, THE
  BOILER, is a roof-high stack in the fire, dead ahead of the door. Spitters hold the coal gantries, and wave 3 comes from the door
  side. A fall into the grate puts you back on the edge (10 damage); enemies in it burn and die. *Tell a friend:* "There's a frozen lock
  with a fire burning under it: you dash over the fire and punch the swarm off the bridge into it." Entrance frame:
  `Logs/feel/n45-r3/a2mix/a245/shot-N30-A245-entrance.png`.
- **Level 3, k5 THE RING -> THE KILN'S HEART** (the worm's own arena, wyrm45-arena). The round firing floor where the worm broke
  up through the iron. The last crew bolted cool slag plates on the rim where its body never comes. The landmark is THE KILN MOUTH,
  straight ahead from the spawn. At half health two perches crumble (the state change). *Tell a friend:* "the worm circles a furnace
  floor; you jump onto the bolted plates on the rim where it can't reach, then boost up and punch its head when it loops through the
  middle". Entrance frame: `Logs/feel/n45-r3/a3ring/a345/shot-N29-A3-ring45-entrance.png`.
- A card for next night: level 4's KNOT YARD -> THE TIE-OFF (docs/ROOMS.md; no geometry yet).

## What was cut (J5), and why it was not meaningful
- **From THE RING:** 52 of k5's 76 props: 34 generic wall trim and 18 you could never see from the boss door. The 24 kept each show
  the kiln's world (the furnace, banners, pipes, lamps, the spill).
- **From THE YARD:** arena42's grinder and cover. arena42 has been OFF since your DECIDE42 call; its pieces had no reason in b1.
- **From THE THAW LOCK,** after the walk: an ice apron (the bot slid off it into the fire 250-430 times a run) and a 0.9 m kerb (it
  trapped the player in the pit).
- **docs/RETIRED.md** lists 12 rows that have been OFF 3+ nights and are superseded. They are only listed: nothing is deleted from
  config or code, because old tapes replay through them and some still feed live rows. Six more are marked "GG decides".
- **Housekeeping:** the 56 old night prompts moved into an untracked `nights/` folder. Tonight's 12,377 numbered reel frames deleted
  after their verdicts.
- **T-62:** level 4's 7-word lesson prompt is now 6 words ("{grapple} HOOK THE KNEE. {grapple} PULL AWAY."). The Lesson tests pass
  6 / 6.

## Also built
- S1: "rows as data" (Core/NightRows.cs + tests 14 / 14, docs/NIGHT-ROWS.md). It is unwired tonight; NIGHT46's shared commit
  switches it on and then each later night's shared commit needs at most 3 files.
- C1: the memory harvest for nights 40-44 (ledgers, lessons L-065 to L-080, CORE's state line).

## Not verified
- **THE WORM fight is still too long:** the bot kills it in 134-186 s (median 161 s), before and after, against your 30-60 s bot
  band. The arena can't change that; it is the worm's health and legs. Next night.
- **The bot never punches,** so the slag / grate kills from a punch or a launch are unmeasured on THE SPILL. The grate's kill of a
  placed swarmer is seen on THE THAW LOCK.
- **THE SPILL is harder for the bot:** 10 deaths in 10 walks against 0 on the old b1. Clear rate is 10 / 10; median 63.7 s against
  51.2 s.
- The Game's own walk pilot does not avoid lava on n45-foundry (A1's hand-off). It does not matter to a human player.
- **Not done:** g3 THE SWARM; look45 (the rooms' own landmarks instead of the generic pylons); the light that changes at a state
  change (it needs new sim code: next night).
