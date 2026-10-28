# HANDOFF: SKYGRIND-unity, PG3 sound + look pass (2026-10-28 ~13:05)

Project: SKYGRIND (Unity 6000.3.7f1, URP), `C:\Users\gaige\dev\SKYGRIND-unity`, branch **`pg3`** (local only; never pushed).
Rules: CLAUDE.md (GG lifted rule 7 for PG3: audio code + Tripo models allowed). The NIGHT74 handoff is `git show u3-n74:HANDOFF.md`.

## Goal
AME 294 Portfolio Game 3 (due Demo Day 2026-10-28): ship SKYGRIND with ElevenLabs audio, Tripo models, a public repo + release,
prompt log, trailer. This pass: "many many" breakcore tracks, powerful SFX, better visuals (GG, docs/pg3/PG3-SOUND-PROMPT.md).

## Done (committed on pg3)
- Sound row `sound74` (Assets/Scripts/Game/Pg3Audio.cs): 138 ElevenLabs effects (Assets/Resources/Audio/PG3/), variants, charge
  loop, per-kind deaths in 3D, boss beats, tell cues, UI, footsteps, wind, level ambience. Player-proven ([sound74] lines; smoke DONE).
- Music row `music74` (MusicDirector.cs): 31 tracks, music_v2_5 (…/PG3/Music/), calm / two combat / boss + enraged / stings.
  Proven on pg3-b..g reels ([music74] lines). All clips levelled (Tools/pg3/normalize.cjs).
- Look rows ON, each with a blind-spotter pair (Logs/n74/verdict-<row>.md): shot74, charge74, pop74, trail74.
- `Pg3Volume` (master/music/sfx 0-10, PlayerPrefs) wired; no menu rows (see needs user).
- Docs: docs/pg3/PLAY-PG3.md (play card, keep/drop), AUDIO_PROMPTS.md (every audio prompt), PROMPT_LOG.md, README-draft.md,
  REFERENCES.md, ATTRIBUTION.md (drafts with TODO(GG)), Research/2026-10-28-pg3-audio/RESEARCH.md.
- No change under Assets/Scripts/Core or PortExport since NIGHT74 (sim untouched).

- Layered sounds (GG 12:45, "too basic ... more powerful"): 15 clips re-made as 3-5 layer ffmpeg mixes (Tools/pg3/layers.json,
  layer-mix.cjs), re-levelled; the charge louder as it fills, hit volume / pitch by damage, the music ducks on a hurt;
  `-skygrindMute` launch flag (commit 642fa681a; compiled clean, Skygrind.Game.dll 12:50:30 after the last edit).
- **Builds/pg3-final2** (from 642fa681a) is the build GG plays; checked silent (`-skygrindMute`) 12:53-13:02: smoke DONE (232 s,
  0 NRE, 0 exceptions; [sound74] played 598, kinds 48; [music74] missing 0); verbs37 PASS foundry 27/27, iceline 29/29,
  kiln 30/30, spire 31/31; boss63 GREEN on kiln (wyrm; boss-hit played 22). Summary: Logs/feel/pg3-final2/summary47.txt (untracked).

- Sound pass 2 (GG 13:45, 30 min cap): 16 more layered SFX (plain KESTREL / THUNDERCLAP x3, gauntlet x2, torch, slam x2,
  land-hard, dash x3, husk-death; Tools/pg3/add-pass2.cjs), 5 third combat tracks `<level>-combat-c` and MusicDirector rotating 3
  combat tracks. Build **pg3-final3** (b701f6556 + these, uncommitted at build time): verbs37 foundry 27/27, boss63 GREEN, music
  missing 0 (foundry-combat-c played 43 s) — but the smoke **FAILS one step twice: attract** ("attract on" then at once
  "attract off: an input", 0 steps), while pg3-final2 passes it twice (incl. a control run at 14:10). Cause not found. Not the
  build GG plays until that is fixed: next, read Kiosk.cs's input test and what in pass 2 could feed it; then rebuild + smoke.
- docs/pg3/TRIPO-PROMPTS.html: Tripo prompts for the 4 weapons (+ KESTREL cylinder / hammer and THUNDERCLAP pump parts, a 4-segment
  charge window on the KESTREL) and the 6 enemy kinds, no bosses. Code needed for part models and gauntlet / torch hooks.

## Not done / open
- rim74 SHORT (spotter: NOTHING OBVIOUS; its fade collapses). tripo74 waits for models. Repo snapshot not pushed.
- [sound74] "missing 4" in every PG3 smoke since pg3-b (not new): 4 plays asked for an id with no clip (silent). The ids are
  built at run time (a text grep of Pg3Audio.cs finds none); not checked which. Next: log the id in Play()'s missing branch.

## Next steps
1. Needs user: listen to the layered charge / blast / hits in **Builds/pg3-final2** (no `-skygrindMute`); tick PLAY-PG3.md
   keep/drop (judge trail74's bottom glow). Until GG says sound is OK again, every check Player runs with `-skygrindMute`.
2. Needs user: the volume menu (6th settings tab breaks his 5-tab cap; built on branch pg3-a1, f279c4834 + a735da4c9).
3. Needs user: Tripo FBX files into Inbox/tripo/ (kestrel, husk, wick first); Tripo API balance is 0 (Studio credits are separate).
4. Repo: `bash Tools/pg3/snapshot.sh` → push to private github.com/gaigerg101-svg/skygrind-unity; Release with the build zip;
   needs user: flip public before the Canvas submission.
5. Trailer, itch page, slides (plan: https://claude.ai/artifact/GeeHACj2PtP34GMLbTEvvq).

## To resume
- Editor is open on the project; drive it with Tools/unity-loop.sh (compile, tests <filter>, errors). Never Play-mode recompile.
- Generators: `node Tools/pg3/sfx-gen.cjs`, `music-gen.cjs`, `layer-mix.cjs`, `normalize.cjs`, `audio-prompts.cjs` (ElevenLabs key is
  in the user's environment; 2 concurrent requests max; SFX 0.5-30 s).
- Needs user approval: push, public visibility, a sixth settings tab, anything in GG's uncommitted drift (fonts, Arena.unity,
  ProjectSettings) — never stage those.

## Do not redo
- music_v1 tracks were replaced by music_v2_5 (no model_id = v1). Boss second theme switches on health only (bossPhase fires at start).
- The "opaque cyan square" was a game object in time-drifted frames, not trail74. Blind pairs need aligned frames (L-155).
- Survival music follows the round state, not the alive count (it flapped 56 times).
