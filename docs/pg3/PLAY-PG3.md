# PLAY-PG3: what changed while you were away (2026-10-28), and how to judge it

Every change has an off switch: launch with `-skygrindRows72 <row>:0` (several: `sound74:0+music74:0`). Keep / drop boxes are yours.

| row | what | where to hear / see it | off switch | keep | drop |
|---|---|---|---|---|---|
| sound74 | **Listen first (you asked for these at 12:45):** the charge, the blasts, the hits, kills, hurt and parry are re-made as 3-5 layer mixes (sub, crack, mechanism, tail): the charged KESTREL, the THUNDERCLAP slug, the gauntlet pulse, the charge whine + READY ping, 3 hits, 3 kills, 2 hurts, the boss hit, parry. The charge whine grows from 45 % to full volume as it fills; a hit's volume (60-100 %) and pitch (a little lower) scale with its damage; the music ducks for 0.35 s when you are hurt. Also new since the last card: footsteps, a wind rush that grows with speed, a quiet ambience bed per level. 138 ElevenLabs effects in all: 3 variants of every gun shot, the charged KESTREL pierce, the THUNDERCLAP slug, the gauntlet pulse, a rising charge whine + a READY ping, parry, headshot, multikill, a death per enemy kind (positioned in 3D), tells (a bright parry cue on a yellow tell, a hard warning on a red one), each boss's intro / tell / slam / down, movement (jump, double jump, wall jump, dash, slide scrape loop, hard landing, slam, grapple, pads), UI move / confirm / back, a heartbeat under 30 % health | anywhere; level 1 (n65-foundry) has every verb | `sound74:0` | [ ] | [ ] |
| music74 | 31 original tracks (ElevenLabs Music, music_v2_5): title, level select, results, ending; per level a CALM track and TWO COMBAT tracks that alternate fight by fight; per boss a theme and an enraged second theme under half health; clear / death stings. Crossfades 1.5 s (0.8 s into a boss), ducks under big hits, muffles under low health, pitches down in slow motion | play level 1: calm in corridors, combat when a room locks, THE WICK's theme at the boss | `music74:0` | [ ] | [ ] |
| shot74 | muzzle flashes (core, stars, spikes, a brief light), hot tracers per pellet, world impact rings + cooling decals, a bigger burst on enemy hits; the THUNDERCLAP's blast reads as a wall | shoot anything | `shot74:0` | [ ] | [ ] |
| pop74 | the enemy death burst: a white-hot core flash, a thick red-orange ring, 12-24 shards thrown wide, embers, a glowing scorch; bigger for a headshot / melee kill, biggest with a second ring for a multikill (blind SPOT: "a big bright orange-yellow flash with a wide orange ring ... where the enemy died") | kill anything that isn't a boss | `pop74:0` | [ ] | [ ] |
| trail74 | movement you can see: cyan floor rails and sparks while sliding, 2 dash echoes of the gun, the slam's ring + dust + a floor pool, wall-jump puffs (blind SPOT: "bright cyan glowing smears and sparks on the floor ... a cyan glow along the bottom of the screen"). **Judge first:** while sliding the rails read as a glow along the bottom edge; it is floor geometry, not your retired screen-border speed lines, but it is the closest thing to them | slide down a corridor on level 1 | `trail74:0` | [ ] | [ ] |
| charge74 | the charge glow at the gun, a READY flash, the charged pierce as a thick beam with marching rings and a burst, the slug's fat comet, the pulse rings, the parry flash with a small screen kick | hold to charge the KESTREL and release. **Judge:** the release burst hides the target for about 0.1 s | `charge74:0` | [ ] | [ ] |

**Your call: the volume menu.** The volume code is in (`Pg3Volume`: MASTER 10, MUSIC 7, SFX 9 by default, saved in PlayerPrefs), but
its menu rows are not: every settings tab is full (GAME and VIDEO 8 front rows, ACCESS 7-8, ADVANCED fits one more), and an AUDIO
tab makes 6 tabs with a pad, past your 5-tab cap (NIGHT16: "way too many options"; MenusTests). The tab is built on branch `pg3-a1`
(f279c4834 + a735da4c9): say "take the audio tab" and it goes in, or name 3 rows to move under ADVANCED.

Not ON yet (each stays OFF until a blind pair names it; the reason is in Logs/n74/verdict-*.md or below):
- rim74 (an enemy halo and the boss entrance light): three rounds; the blind spotter saw NOTHING OBVIOUS; its fade still collapses
  (Logs/n74/verdict-rim74.md). Try: `-skygrindRows72 rim74:1`.
- tripo74 (your Tripo models): waiting for the FBX files in Inbox/tripo/.

The build: **Builds/pg3-final2/SKYGRIND.exe** (commit 642fa681a, every row above ON by default). Checked silent
(`-skygrindMute`, a launch flag that is never saved): smoke DONE (all steps, 0 errors); verbs37 PASS on all four levels
(foundry 27/27, iceline 29/29, kiln 30/30, spire 31/31); the wyrm boss GREEN (its theme played, the layered boss hit 22 times).
Run it without `-skygrindMute` to hear it.

Sources: the prompts are in docs/pg3/AUDIO_PROMPTS.md (every ElevenLabs prompt, length, loudness) and docs/pg3/PG3-SOUND-PROMPT.md
(the plan this pass followed). The research behind the mix is Research/2026-10-28-pg3-audio/RESEARCH.md. Read its licence note on
ElevenLabs Music for games (an "Indie Game" clause, under $500k lifetime revenue) before a Steam release.
