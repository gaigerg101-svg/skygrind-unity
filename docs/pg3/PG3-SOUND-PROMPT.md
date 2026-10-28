# PG3-SOUND: the sound pass and the look pass (2.5 hours, GG away; written 2026-10-28 11:00)

GG, 2026-10-28, verbatim: "spend the next 30 mins planning detaiiled prompt to follow in terms of only elleven labs related cntentm
sound effect, breakcore style msuic, very inspored by ultrakill, obviously it needs to be origional but I wnat break core style "AZALI"
also sometimes classical instrument buildup or piano ocassionaly, I wnat many many tracks good very satusfying sound effects for guns,
dash, charge attacks bosses, ect, please take you time studying game and audio deisgn with video game and spend the next 2 hours adding
sound effects polishing ect, and also using that time with subagents to imporve other visuals lighting effects ect, not just eleven labs,
music right now is way too basic. needs to be much higher quality. I am stepping away form compuet so i expect it to ocntinue for next
2.5+ hours with no stopping, I wnat many many more effects songs, tracks, and go beyond this prompt follow best methods to make this
prompt work best for project".

Order of authority: the org rules, GG's words, CLAUDE.md (rule 7 lifted for PG3 audio and Tripo models, GG 2026-10-28), this file,
the logs. This is a prompt, so it lives in docs/pg3/ (tracked) as prompt-log evidence.

## 0. The essentials (read again after any context reset)
- **Clock:** start 11:00 wall, end no earlier than 13:30 wall. `date` at every landing. Never stop to summarise; a waiting turn ends with
  a tool call or a background job pending.
- **Branch `pg3`.** Commit with explicit paths + `.meta`; restore the TMP fallback asset before each commit; never touch GG's drift
  (UnityConnectSettings, the TMP SDF fonts, Arena.unity, UIEffectProjectSettings, EditorBuildSettings); never push (GG said private repo,
  push only at the end of PG3 with everything in, a separate step).
- **The sim is untouched.** Sound and look are views. No change under Assets/Scripts/Core except none. Golden vectors and replays cannot
  move; check `bash Tools/unity-loop.sh tests` on the classes a change touches, and the smoke on each build.
- **Every change has an off switch:** sound rows `sound74` (SFX), `music74` (the music director); look rows `<name>74` in design.n74 via
  `node Tools/night/n74-row-add.cjs <row> "<why>" --no-frame`, flipped ON only after its Player proof.
- **Evidence is a file.** Sound: the `[sound74]` / `[music74]` Player log lines (what played, how often). Look: a same-build pair
  (`n74-reels.sh` ON `-skygrindRows72,<row>:1` vs OFF), the blind spotter (`skygrind-spotter`, A.png / B.png only), verbs37 PASS.
- **ElevenLabs:** key in the user env `ELEVENLABS_API_KEY` (an `sk_` key, no `user_read`, so no balance read: a 401 / 402 / 429 on a
  generation is the quota signal: log it, stop generating, keep wiring what exists). Never write the key to a file. SFX endpoint
  `POST /v1/sound-generation` (0.5-30 s; `loop`); music `POST /v1/music` (`prompt` or `composition_plan`, `music_length_ms`,
  `force_instrumental`); a 10 s test took 3.7 s (Research/2026-10-28-pg3-audio/probe/).
- **Never name an artist or a game in a prompt** (ULTRAKILL, AZALI): describe the production instead (section 2). Originality is GG's
  requirement and the API's rule.

## 1. The target, plainly
By 13:30, in a Player build of `pg3`:
1. **Music that changes with the fight** (`music74`, MusicDirector): a title theme, a level-select theme, one CALM and one COMBAT track
   per campaign level (5 levels: foundry, iceline, kiln, span, spire), one theme per boss (THE WICK, THE ICEBREAKER, THE KILN WYRM,
   THE LOOM, THE CROWN), a clear / win sting, a death sting, the results theme. Crossfade on state change (calm <-> combat 1.5 s, into a
   boss 0.8 s), music ducks under the big SFX, a low-pass under low health. At least 16 tracks, 60-180 s each, loopable.
2. **SFX v2** (`sound74`): every gun with 3 variants (round robin, pitch +-4 %, volume +-1.5 dB), layered prompts (transient + body +
   mechanical + tail), the charged shots (KESTREL pierce, THUNDERCLAP slug, gauntlet pulse) with a rising charge loop and a READY ping,
   every movement verb, enemy tells per kind, enemy deaths per kind, every boss's intro / tell / slam / hit / phase / down, the parry with a
   pitch-down hit-stop treatment, UI move / confirm / back, low-health heartbeat. At least 90 clips.
3. **Look pass** by two builders (section 5), each row proven by a pair and a SPOT before it goes ON.
4. **The papers:** PROMPT_LOG section 5 (every audio prompt, from Tools/pg3/*-log.json), the reflection's facts, this file's STATE.

## 2. The music language (original; described, never named)
Core: **emotional breakcore**: 165-185 bpm chopped and re-sequenced amen / think breaks with time-stretch stutters and glitch edits,
distorted kicks, reese and 808 sub bass, bright supersaw or plucked leads, airy pads; **and the classical turn**: some tracks open or
break down with solo piano, string ostinato, harpsichord or choir, building into the breaks (GG: "sometimes classical instrument buildup
or piano ocassionaly"). Game use: the CALM version of a level is the same key and palette, sparse (pads, piano, half-time drums or none);
the COMBAT version is the full break. Bosses are bigger: choir, organ, heavy distorted guitars over breaks, a key centre per boss.
Per level (the room's colour and verb):
- foundry (level 1, molten works, THE WICK): D minor, 174 bpm, metallic percussion, anvil hits, warm detuned piano.
- iceline (level 2, frozen rail yard, THE ICEBREAKER): F# minor, 170 bpm, glassy bells, music-box piano, icy pads, crisp breaks.
- kiln (level 3, kiln halls, THE KILN WYRM): C minor, 180 bpm, low brass, tribal toms under the breaks, heat-haze guitars.
- span (level 4, the bridge, THE LOOM): A minor, 168 bpm, harpsichord ostinato, string staccato, mechanical clicks.
- spire (level 5, the bell tower, THE CROWN): E minor, 176 bpm, church organ, choir, bells, the biggest track.
Use `composition_plan` where structure matters (intro build -> drop -> break -> drop -> outro that loops into the drop).

## 3. The sound language (what makes it satisfying; Research/2026-10-28-pg3-audio/RESEARCH.md has the sources)
- A gunshot is layers: a sharp transient click, a punchy body, a mechanical sweetener (cylinder, pump), a short room tail. The KESTREL is
  tight and bright; the THUNDERCLAP is a wall of low end; the gauntlet is a whoosh into meat and an electric crack; the TORCH is a roar.
- Reward sounds rise (major, bright, short); damage sounds fall and thud; end stings are the longest.
- Variation beats repetition: 3 variants of anything that fires more than once a second.
- Mix: music ducks 4-6 dB under boss slams, kills of big enemies and the parry; a heartbeat and a low-pass under 30 % hp; the hit-stop
  pitches the bus down.

## 4. The sound track (the lead; generation is API calls, so it runs in the background while the lead reviews the look builders)
1. Tools/pg3/sounds.json v2 (the list grows by id; a variant is `<id>-2`, `<id>-3`), Tools/pg3/music.json (tracks with prompt or
   composition_plan, length, loop). Tools/pg3/music-gen.cjs (like sfx-gen.cjs; logs to music-gen-log.json).
2. Generate in priority order: guns + charge + parry, movement, bosses, enemies, UI, then music: title, foundry combat + calm, WICK, the
   other levels, the other bosses, stings.
3. Normalize: ffmpeg loudnorm, SFX to about -16 LUFS integrated / -1 dBTP, music to about -18 LUFS (research may correct this), trim
   silence; keep originals in Research/2026-10-28-pg3-audio/raw (untracked if large).
4. Code: Pg3Audio.cs grows variants, charge loops (poll the loadout's charge fraction), per-boss maps, per-kind enemy maps, UI hooks,
   ducking, low-health filter, hit-stop pitch. MusicDirector (new file, row `music74`): state = title / select / calm / combat / boss /
   results; reads the flow, the level id, enemies alive / room locked, the boss active; crossfades two AudioSources.
5. Prove: a Player run's `[sound74]` / `[music74]` lines show each state reached and the clips played; the smoke DONE.

## 5. The look track (two builders in worktrees, `general-purpose`, background)
- **V1 WEAPON FEEL:** muzzle flash, tracers, impact sparks and decals, the charged shot's glow and its release, the shotgun's slug, the
  parry flash. Cyan is the player's colour; red-orange is enemies only.
- **V2 LIGHT AND MOTION:** the dash / slide / slam trails and the slam shockwave, per-level light polish (a rim light on enemies, a warm
  key at dusk), boss entrance light. No new flicker (the moving-flicker meter n74-flick.py on the level).
Each builder: its own `UnityTuning.PG3<V>.cs`, a view row added by the lead, code + EditMode tests, commits in its worktree; never runs
Unity, a build or a Player. The lead cherry-picks, compiles, builds `pg3-<k>`, reels ON vs OFF, spots, flips.

## 6. Trains (the lead)
- 11:45 train 1: SFX v2 wired + the first music + V1/V2 first slices: compile, tests, build pg3-b, smoke, reels, spot.
- 12:45 train 2: the rest of the music + V1/V2 second slices: build pg3-c, smoke, reels, spot, flips.
- 13:15 close: build pg3-d (the build GG plays), smoke, the papers, commit, STATE below, the report to GG.

## 7. STATE (rewrite at every landing)
- 11:00 plan written; research agent (audio design + ElevenLabs music API) and event-map agent running; pg3-a build + smoke running.
- pg3-a (818051f66 + the 28 first clips): smoke DONE; `[sound74] played 308, missing 0`.
- a557ced54: the sound pass (91 more clips: 119 effects) + MusicDirector (music74 ON); 1052b5112: 31 tracks on music_v2_5 (the first
  21 were music_v1: no model_id; kept under Research/.../music-v1), levelled (normalize.cjs, 150 clips).
- ElevenLabs limits seen: 2 concurrent requests (429 concurrent_limit_exceeded: the scripts wait and retry); SFX 0.5-30 s.
- Running: pg3-b build + smoke + reels vn (n65-foundry verbs37), bk (n67-kiln boss63); builders V1 (shot74, charge74), V2 (trail74, rim74).
- Next: compile the torch loop / headshot / multikill hooks; pick V1 / V2; pg3-c; the [music74] states check; the prompt log's section 5.
- 11:20-12:05: 128 effects + 31 tracks (music_v2_5), levelled; tell cues; 3D enemy voices; the torch loop; headshot / multikill.
  pg3-b: smoke DONE, verbs37 27/27, boss63 wyrm intro/tell (the second theme too early: fixed, health share only).
  pg3-c: shot74 ON (SPOT f0228), charge74 ON (SPOT f0244); trail74 drew an opaque cyan square (f1456): back to V2.
  pg3-d: square gone; rim74 / trail74 still invisible in frames (drawn per the probe): V2 moved both to Fx74's draw path (b538f8648);
  boss fight now boss-wyrm -> sting -> results; smoke DONE with every default row ON (NRE 0).
  A1: Pg3Volume taken (defaults 10/7/9); the AUDIO tab NOT taken (6 tabs with a pad > GG's cap of 5): GG's call in PLAY-PG3.md.
  V3: pop74 (the enemy death burst) picked, Pop74 13/13.
- pg3-e / pg3-f: pop74 ON (round 2, SPOT f0091); rim74 SHORT (3 rounds, SPOT NOTHING OBVIOUS, the fade collapses); trail74 SHORT
  (drawn per the probe, no pair catches it; the "square" was a game object in drifted frames).
- 12:28 pg3-play (HEAD a80f77984 + config): smoke DONE (NRE 0); verbs37 27/27 n65-foundry, 30/30 n67-kiln; boss63 boss-wyrm theme,
  wyrm intro / tell; kill12 pop + charged pierce + husk death; every reel NRE 0. ON by default: sound74, music74, shot74, charge74,
  pop74. Not ON: trail74, rim74 (SHORT), tripo74 (no models yet), the volume menu (GG's call: the 5-tab cap).
