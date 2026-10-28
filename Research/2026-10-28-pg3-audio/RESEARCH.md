# PG3 audio research: ElevenLabs Music + SFX APIs, breakcore vocabulary, fast-FPS audio practice (2026-10-28)

Scope: what the PG3 sound pass needs to know before it spends more credits. Web + docs only; no code changed.

Evidence tags used below:
- **[doc]** read on an official ElevenLabs / Unity / Audiokinetic page today (pages were read through a summarising fetcher, so exact numbers should be re-checked on the page before a budget decision).
- **[proj]** observed in this project's own API calls (`Tools/pg3/*-log.json`, `Research/2026-10-28-pg3-audio/probe/headers.txt`).
- **[sec]** secondary source: blog, forum, fan wiki, vendor page. Treat as practitioner opinion.
- **[inf]** my inference or craft judgement, not found in a source. Tune by ear.
- **not checked** = I did not find it; do not rely on it.

---

## 0. What this changes in the current pipeline (read this first)

1. **`music-gen.cjs` sends no `model_id`, so every one of the 21 tracks was made by `music_v1`** [proj: request body has `prompt`, `music_length_ms`, `force_instrumental` only; doc: `model_id` default is `music_v1`]. The API now offers `music_v2` and `music_v2_5`; the docs call v2.5 "the most advanced model" and the ElevenLabs music skill uses it by default [doc]. v2/v2.5 add chunk-based composition plans (each chunk its own styles and duration, "durations are always enforced"), audio reference, and conditioning on a stored song. **Recommendation: A/B one track (e.g. `foundry-combat`) with `"model_id":"music_v2_5"` before generating more.** Cost: ~2 min of audio.
2. **Studio vs Indie game licence clause in the Music terms (Last updated 09 Oct 2026)** [doc, read via fetcher, GG should read it himself]: Output is licensed for "all online and offline commercial use ... except film, TV, radio, and Studio Games" on Free through Enterprise Music Lite. A **Studio Game** is "a video game ... commercialized by sale, advertising, or any other form of monetization, but does not include an **Indie Game**". An **Indie Game** has "generated revenue of less than $500,000 since its first commercial release" and the developer's "revenue from all games was less than $1,000,000 in its most recently completed financial year" (measured after platform fees and refunds). If a game stops qualifying, "you have 60 days" to get a plan that permits Studio Games, after which new copies cannot carry the Output (copies already distributed may). So a Steam release is allowed while SKYGRIND is under those revenue lines; above them it needs the Enterprise Music plan. Also: streaming-platform distribution of the tracks (a soundtrack on Spotify) is Prohibited on Free and Starter, Permitted from Creator up; building a "Music Library" for third parties is Prohibited Free-Scale. Soundtrack-as-DLC sale on Steam: **not checked**.
3. **MP3 is the wrong container for the loops.** MP3 carries encoder delay/padding; Unity re-encodes imports and users report silence at loop points; the sources agree Vorbis/FLAC/PCM are the gapless formats [sec]. Ask ElevenLabs for `pcm_48000` (music; tier gate for 48 kHz PCM not stated) or `wav_48000` (SFX), import as Vorbis or PCM, and verify the loop point in a Player. If the plan cannot do PCM, trim and crossfade the loop offline (section 2.5).
4. **SFX `duration_seconds` minimum is 0.5, not 0.1** (the overview page says 0.1; the API reference and the `hit` 400 in `sfx-gen-log.json` say 0.5) [doc + proj]. Already clamped in `sfx-gen.cjs`. For true 80-150 ms UI ticks, generate 0.5 s and trim offline.
5. **Concurrency on this plan is 2** (`concurrent_limit_exceeded` 429 on the first music run) [proj]. Music and SFX share it. The current retry loops are right.
6. **Stem separation exists** (`POST /v1/music/stem-separation`, `stem_variation_id` = `two_stems_v1` or `six_stems_v1`, ZIP out) [doc for endpoint; the two ids are from doc mirrors, not the English page]. Use it to turn a combat render into sample-aligned layers (drums / bass / other) for vertical layering, instead of crossfading two independently generated tracks that will never share a beat grid (section 4.7).
7. **Weapons are never ducked; music is.** DOOM (2016): "Weapon sounds never get ducked by anything, not even VO" [sec, ADSR interview]. PG3-SOUND-PROMPT section 3 already ducks music, not guns; keep it that way.
8. **Prompt rule, exactly:** Music terms section 2.1 forbids as input: any artist's real or stage name, any songwriter, any song title, any album title, any music publisher or record label, or a distinctive part of a song's lyrics [doc]. Game titles and genre words are not on the list, but the project rule (never name ULTRAKILL or AZALI) is the safe line and costs nothing. "amen break", "think break", "reese", "supersaw" passed on every music request in `music-gen-log.json` (15 x 200 and one concurrency 429, no `bad_prompt`) [proj].
9. **Budget**: 21 tracks = 36.75 min; 119 SFX clips = 181.6 s of requested audio [computed from music.json / sounds*.json]. Prices disagree across ElevenLabs pages (section 1.5): music 900 credits/min = 33,075 credits (more than one Starter month of 30k) or $5.5 at the API page's $0.15/min or $11 at a third party's $0.30/min. Read the real number off the account's credit counter after one call; the plan's key has no `user_read` so the script cannot.

---

## 1. ElevenLabs API reference (as of today)

### 1.1 Music: compose

- `POST https://api.elevenlabs.io/v1/music` (regional hosts exist: `api.us.`, `api.eu.residency.`, `api.in.residency.`, `api.sg.residency.elevenlabs.io`). Auth header `xi-api-key`. Source: https://elevenlabs.io/docs/api-reference/music/compose [doc]
- Query: `output_format` (default `auto` = `mp3_44100_128` for v1, `mp3_48000_192` for v2 models), `enable_logging` (false = zero retention, enterprise only).
- Body (JSON), mutually exclusive inputs:

| Field | Notes |
|---|---|
| `prompt` | text; cannot be combined with `composition_plan` |
| `composition_plan` | object; `MusicPrompt` for `music_v1` (`positive_global_styles`, `negative_global_styles`, `sections[]`) or `CompositionPlan` for v2/v2.5 (`chunks[]`) |
| `music_length_ms` | only with `prompt`; **3,000 to 600,000** per the API reference; omit and the model picks the length. The overview and pricing pages say max 5 min, a doc-mirror guide says 10 s min. Use 10 s to 5 min to be safe [conflict] |
| `model_id` | `music_v1` (**API default**), `music_v2`, `music_v2_5` |
| `force_instrumental` | bool; only with `prompt`. With a plan, put "instrumental / no vocals" in styles and `negative_styles: ["vocals","lyrics"]` |
| `seed` | only with a plan; "aims for consistency, exact reproducibility not guaranteed" |
| `respect_sections_durations` | v1 plans only; v2/v2.5 always enforce chunk durations |
| `store_for_inpainting` | bool; stores the render so its `song_id` can be reused |
| `finetune_id` | custom finetune (training $1.50 each on the API page) |
| `sign_with_c2pa` | mp3 only |

- Response: audio bytes in the requested format; the **`song-id` response header** carries the id [proj: probe/headers.txt shows `song-id: kf5GADKU...`; `music-gen.cjs` already logs it].
- Output formats: `mp3_{22050_32, 24000_48, 44100_32/64/96/128/192, 48000_128/192/240/320}`, `pcm_{8000..48000}`, `opus_48000_{32,64,96,128,192}`, `ulaw_8000`, `alaw_8000`. MP3 192 kbps needs Creator+, PCM 44.1 kHz needs Pro+ [doc, on the SFX and stem pages; same tier note likely applies; the project got `mp3_44100_192` 200s].
- Latency [proj]: 8 to 21 s of wall time for 90 to 170 s tracks.

### 1.2 Composition plan (music_v2 / v2.5) [doc: https://elevenlabs.io/docs/eleven-api/guides/how-to/music/composition-plans]

- `chunks[]`, **max 30 chunks**, each chunk 3,000 to 120,000 ms, plan total 3 s to 10 min.
- Chunk fields: `text` (section name in `[brackets]`, lyric lines, `(phonetic sounds)`, `{inline directions}` e.g. `{guitar solo}` `{scratching}`), `duration_ms`, `positive_styles` (max 50), `negative_styles` (max 50, default empty), `context_adherence` (`low` / `medium` / `high`, default `high`: "how closely the chunk follows its surrounding chunks"), optional `conditioning_ref` {`song_id`, `range`{`start_ms`,`end_ms`}} with `condition_strength` (`low`/`medium` default/`high`/`xhigh`), reference at most 30,000 ms, min range 50 ms [doc: inpainting guide].
- Style rules from the page: first chunk's styles "set the overall tone and genre"; "Aim for at least 6-7 styles in early chunks"; be specific ("80 BPM", "warm acoustic guitar with light fingerpicking"); "use negative styles liberally"; append generic quality terms ("great production quality"); **styles must be English**, lyrics any language; use `{braces}` for short cues, `positive_styles` for anything that holds for the whole chunk.
- Build a plan from a prompt first: SDK `music.composition_plan.create(prompt, music_length_ms, model_id)` returns the JSON, edit it, then `compose(composition_plan=...)`. The REST path of that call: **not checked** (SDK name only).
- Skeleton for a looping combat track (durations are bars x 4 x 60000 / bpm; at 174 bpm 1 bar = 1.379 s, 8 bars = 11.03 s, 16 bars = 22.07 s):

```json
{ "chunks": [
 { "text": "[Intro]\n{solo piano, rubato}", "duration_ms": 11034,
   "positive_styles": ["emotional breakcore","174 BPM","D minor","solo felt piano","wide reverb","cinematic","great production quality"],
   "negative_styles": ["vocals","lyrics","four-on-the-floor kick"], "context_adherence": "high" },
 { "text": "[Build]\n{string ostinato enters, snare roll, filter sweep}", "duration_ms": 11034,
   "positive_styles": ["string staccato ostinato","accelerating snare roll","rising noise riser","174 BPM","D minor"],
   "negative_styles": ["vocals","fade out"], "context_adherence": "high" },
 { "text": "[Drop]\n{chopped amen break, reese bass}", "duration_ms": 22068,
   "positive_styles": ["chopped and re-sequenced amen and think breaks","time-stretch stutters","distorted punchy kick","reese bass","detuned supersaw lead","174 BPM","D minor","loud modern mix"],
   "negative_styles": ["vocals","slow tempo","fade out"], "context_adherence": "high" },
 { "text": "[Loop tail]\n{ends on the same chord and drum phrase the intro starts on}", "duration_ms": 11034,
   "positive_styles": ["break continues","no fade out","hard stop on the downbeat"], "negative_styles": ["fade out","vocals"], "context_adherence": "medium" } ] }
```
(Untested here; the chunk idea is documented, the content is a proposal [inf].)

### 1.3 Other music endpoints

| Endpoint | What | Source |
|---|---|---|
| `POST /v1/music/detailed` | same body plus `with_timestamps`, `with_waveform_visual`; **multipart/mixed** response: audio + JSON (`composition_plan`, `song_metadata` {title, description, genres, languages, is_explicit}). Use it to capture the plan the model chose for a good prompt, then edit that plan | https://elevenlabs.io/docs/api-reference/music/compose-detailed [doc] |
| `music.composition_plan.create` | prompt -> plan JSON, no audio (and no credit cost stated: **not checked**) | https://elevenlabs.io/docs/eleven-api/guides/cookbooks/music [doc] |
| `POST /v1/music/stem-separation` | multipart `file` (+ optional `stem_variation_id`, `sign_with_c2pa`); returns a **ZIP** of stems; "might have high latency"; size/duration limits not stated; ids `two_stems_v1`, `six_stems_v1` (default six) from doc mirrors | https://elevenlabs.io/docs/api-reference/music/separate-stems [doc] |
| `music.upload` | upload a file, get a `song_id`, optionally extract a plan | inpainting guide [doc] |
| inpainting / extend | plan mixing generation chunks and audio-reference chunks (`song_id` + range) to keep a slice and regenerate around it; the guide shows "extend" and "make a loop by repeating a slice with a short glue chunk". One skill page says inpainting is limited to enterprise clients; the guide states no plan limit: **unresolved** | https://elevenlabs.io/docs/eleven-api/guides/how-to/music/inpainting.md |
| `music.stream` | chunked audio as it generates, paid plans | streaming guide [doc] |
| video-to-music, finetunes | exist; not relevant here | ElevenLabs music skill [doc] |

Audio Reference (UI/API, v2+): about 30 s upload, steers sound/mood/tempo/instruments, **does not remix or change genre**, every upload is copyright screened and "designed for music you've made yourself" [doc]. Useful for keeping calm and combat tracks of one level in the same key and palette: feed the combat track's best 30 s as the reference for the calm track (as `conditioning_ref` with `condition_strength` `medium`), keep the prompt's "half-time, sparse, no breakbeat" change.

### 1.4 Errors and limits

- 401 bad key, 422 invalid params, 429 rate limit [doc]. 429 body in this project: `{"detail":{"type":"rate_limit_error","code":"concurrent_limit_exceeded",...}}` = plan concurrency is 2 [proj]. TTS concurrency by plan from ElevenLabs (Free 2, Starter 3, Creator 5, Pro 10, Scale/Business 15) is the same ladder; music/SFX-specific limits **not checked** [sec: elevenlabs help article 19571824571921]. `system_busy` 429 = retry.
- Copyright filter: a prompt or plan that names a band/musician or uses copyrighted lyrics returns **`bad_prompt`** with a `prompt_suggestion` (or **`bad_composition_plan`** with `composition_plan_suggestion`); harmful content gets no suggestion [doc: music cookbook]. Handle both: log the suggestion, do not auto-retry with it unreviewed.
- Plan requirement: Music API paid only [doc]. Commercial use "Starter+" [doc: pricing]; see section 0 item 2 for the game clause.

### 1.5 Pricing (conflicting; verify on the account)

| Source | Music | Sound effects |
|---|---|---|
| elevenlabs.io/pricing (in-app credits) [doc] | **900 credits per minute** | **200 credits per generation** |
| elevenlabs.io/docs/overview/capabilities/sound-effects [doc] | n/a | "Setting a duration costs 40 credits per second" (so a 0.5-1.5 s one-shot would be 20-60 credits, cheaper than auto-length 200) |
| elevenlabs.io/pricing/api (USD) [doc] | **$0.150 per minute**, finetune $1.50 | **$0.120 per generation/min** (the page contradicts itself: table says per minute, FAQ says "billed per generation") |
| developer.puter.com guide, 23 Jun 2026 [sec] | $0.30 per minute | $0.12 per generation |
| fal.ai / other resellers [sec] | $0.80/min and others | n/a |

Monthly credits: Free 10k, Starter 30k, Creator 121k, Pro 600k, Scale 1.8M, Business 6M [doc + sec agree]. At 900 credits/min that is about 11 / 33 / 134 / 667 / 2,000 / 6,667 music minutes. For this project: 36.75 min of music = 33k credits; 119 SFX clips at 200 = 23.8k credits, or at 40 credits/s about 7.3k. Cheapest sensible path: always pass `duration_seconds`, never auto. **Measure: note the credit counter before and after one 10 s music call and one 1 s SFX call and write the real rates into PROMPT_LOG.**

### 1.6 Style without names (what to write instead)

Neutral production phrases that carry the same information (all accepted by the API so far where used) [proj + inf]:
- "emotional breakcore, 174 BPM, D minor"
- "chopped and re-sequenced amen and think breaks, time-stretch stutters, glitch edits, stutter fills, snare rolls"
- "distorted punchy kick, growling reese bass, 808 sub"
- "bright detuned supersaw lead, plucked arpeggios, airy pads, felt piano melody"
- "string ostinato, choir pads, church organ, harpsichord, timpani rolls" for the classical turn
- "heavy downtuned distorted guitar riffs" for boss tracks
- "ambient interlude, half-time, filtered breakbeat ghosting quietly underneath" for calm
- "nostalgic, atmospheric, sentimental" is how Bandcamp Daily describes the 2020s revival [sec]; "bittersweet", "melancholic" are safe mood words.
Never write: artist, producer, song, album, label, publisher names; "in the style of X"; "sounds like [game]". Section 2.1 of the terms plus 3.3 (no replicating an identifiable recording artist's voice or likeness) [doc].

### 1.7 Sound Effects API

- `POST https://api.elevenlabs.io/v1/sound-generation` [doc: https://elevenlabs.io/docs/api-reference/text-to-sound-effects/convert]. Query `output_format` (default `mp3_44100_128`; also `wav_{8000..48000}`, `pcm_*`, `opus_*`, `ulaw/alaw_8000`; MP3 192 kbps = Creator+, PCM/WAV 44.1 kHz = Pro+), `enable_logging`.
- Body: `text` (required; the playground caps prompts at **450 characters**), `duration_seconds` (**0.5 to 30**, null = model guesses), `prompt_influence` (**0 to 1, default 0.3**; higher = more literal, less variable), `loop` (bool; seamless loop; the ElevenLabs skill says v2 only), `model_id` (`eleven_text_to_sound_v2` default, `eleven_text_to_sound_v3`). No `seed` field in the reference: **a repeat call with the same text is a new roll**, which is how variants are made. v3 differences (quality, loop support, price): **not checked**; worth one A/B on `fire-kestrel`.
- Playground produces 4 candidates per click; the API returns 1 per call [doc + inf].
- Output: MP3 for all, WAV 48 kHz for non-looping effects via the playground [doc]. A looped effect is built for repeat playback; sustained consistent sounds loop best, one-time events do not [sec].
- Terms the docs name as understood: impact, whoosh, ambience, one-shot, loop, stem, braam, glitch, drone [doc: overview].
- Project settings now: `prompt_influence 0.5`, mp3_44100_128, no `model_id` [proj].

---

## 2. Prompt-writing guidance

### 2.1 Music (official) [doc: https://elevenlabs.io/docs/overview/capabilities/music/best-practices]
- Decide five things: genre, mood, instrumentation, tempo, production era; "any you leave open, the model fills with its most statistically likely choice".
- Give **BPM and key**: "the model holds both precisely enough to layer with other material".
- Production vocabulary changes the mix: sidechained, close-mic'd, bone-dry, tape saturation, plate reverb; era words ("1990s jungle") set production character.
- Narrate arrangement in order like briefing a band ("start with", "just", "then", "bring in"); **mark silences** or the model fills them.
- Loops/stems: state **bars, BPM and key, and what is excluded** ("No melody, just drums" keeps a drum break a drum break); "solo" before an instrument isolates it.
- Timing cues: "lyrics begin at 15 seconds", "instrumental only after 1:45"; add "instrumental only" or use `force_instrumental`.
- Short evocative prompts can be more creative than long ones; long ones give control (official page) while a third-party guide says concise beats verbose [sec: fal.ai]. Section-by-section is recommended for full songs: a ~30 s foundation, then sections that reference it [sec: fal.ai; help center].
- Length: 30 s for exploring and loops, auto for songs with natural endings, fixed for exact timing [sec: fal.ai].
- Include/exclude style tags (e.g. exclude "four-on-the-floor kick" to keep a breakbeat feel) [sec/doc help center].

### 2.2 Music (our template) [inf, built from the above]
`<mood> <genre>, <BPM> BPM, <key>. <arrangement in time order with seconds or bars>. <drums>. <bass>. <lead/harmony>. <classical/piano layer>. <mix words>. Instrumental only. Ends on the same chord it starts on, no fade out.` The existing `music.json` prompts already follow this; the missing pieces are (a) bar/time cues for the build, (b) an explicit exclusion line ("no vocals, no four-on-the-floor kick, no fade out"), (c) the v2.5 plan for tracks that need an exact build -> drop -> loop shape.

### 2.3 SFX (official + community)
- Simple prompts for one sound; detail helps ("high-quality, professionally recorded footsteps on grass, sound effects foley"); for **complex or sequential sounds generate each part separately and combine in an editor** [doc: playground + help center].
- Specific over vague ("Heavy rain on a tin roof"), combine elements, name style ("8-bit retro jump sound"), add mood/context [doc: elevenlabs/skills sound-effects].
- A sequence can be described in order ("a sword drawn then clashing") but gets less reliable than one event [doc: overview].
- Add `one-shot` to forbid a repeating pattern; add `loop` / a sustained noun only for loops [doc term list + sec].
- Keep the exclusion in the text: "no music, no voice, no reverb tail" (dry layers for engine reverb) [inf].
- Raise `prompt_influence` (0.5-0.8) for one-shots that must match a brief, lower (0.2-0.3) for variety across re-rolls [doc: higher = literal; the rest inf]. A cheap variant strategy: same text, `prompt_influence` 0.3, three calls.

### 2.4 SFX prompt skeleton (proposal) [inf]
`<source/material> <action>, <transient character>, <low-end character>, <tail/space>, <perspective>, one-shot, no music, no voice`. Example for the revolver crack layer: "Sharp metallic revolver crack, tight snappy transient, no low end, very short dry decay, close mic, one-shot, no music".

### 2.5 Getting loops
- Music: ask for the loop in the prompt (bars, BPM, key, "ends on the same chord it begins on, no fade out, no intro silence"). The model does not guarantee sample-exact loop points, so finish offline: find a bar-aligned loop (bar = 240000/bpm ms; at 174 bpm 1.379 s), cut start and end on zero crossings or crossfade 30-100 ms, bounce to WAV, import Vorbis/PCM, test the wrap in a Player [inf + sec].
- v2/v2.5 plans can repeat a slice with a glue chunk to create a loop [doc: inpainting guide].
- SFX: `loop: true`, `duration_seconds` 2-4 s for engines/beds (flame, slide, charge, heartbeat); sustained noun phrases ("steady roaring gas flame, no pops that stand out"); verify the join; trim silence.

---

## 3. Breakcore production vocabulary (neutral terms), and why a shooter's soundtrack works

### 3.1 The genre in production terms
- **Drums are the defining trait**: built mainly from the Amen break and other classic jungle and hip-hop breaks, pushed to high tempo; producers cut up and re-sequence breaks, distort and loop them, or add delay/chorus to change their timbre [sec: Wikipedia Breakcore; emastered.com]. "Think" breaks are another classic source [sec: Bandcamp Daily].
- **Roots**: jungle, hardcore, drum and bass (mid-late 1990s) [sec: cavac.at summary of Wikipedia]. Drum and bass sits at "usually 160-180 BPM" (early forms ~130, ~155-165 by 1993) [sec: Wikipedia DnB]. Speedcore is "about 300 to 600" BPM [sec: Wikipedia Speedcore], so breakcore is the middle band. Bandcamp Daily says some breaks and kicks "occasionally push 200 BPM" and cites "gabber kicks at 200 BPM" [sec]. **A hard breakcore BPM range is not stated by any source I read**; the 165-185 core used in PG3-SOUND-PROMPT is practitioner convention. Peak 190-200+ only for climaxes [inf].
- **Techniques (words that map to sound)**: chop, slice, re-sequence, time-stretch (stretch a break to tempo then slice), pitch the break to the grid, stutter / glitch edit (repeat a 1/16 or 1/32 slice), machine-gun kicks and snare rolls, layered breakbeats, pitched snare rises, "bent" breaks, bit-crush / lo-fi, breaks that "decay into static" [sec: Bandcamp Daily lists, Wikipedia, emastered].
- **Kick and bass**: heavily boosted/distorted kicks (gabber-style; producers overdrive kicks until they resemble square waves [sec: Wikipedia Speedcore]), Reese bass, acid bass, hoover [sec: Wikipedia Breakcore], 808/sub under the break. Reese origin: a late-1980s techno bassline later sampled across jungle/DnB [sec: Wikipedia DnB].
- **Melody is not defined by the genre**: sources range from harpsichord and operatic vocals to classical sampling, violins/saxophones, sampled video game and anime sounds, "twinkly synth swells" and "atmospheric synths" [sec: emastered; Bandcamp Daily]. Some artists record live instruments with the breaks [sec: emastered]. Related named styles that matter as vocabulary: *baroquecore* (breakcore + metal + baroque), *raggacore*, *breakcorpse* (noisy, distorted), *lolicore* (high BPM, pitched anime-vocal samples), *internetcore/glitchcore* [sec: emastered].
- **2020s "revival" traits**: "nostalgic, atmospheric, and sentimental"; video game/anime/Y2K aesthetics; the Amen break plus atmospheric synths; a later strand pushes back toward complex breakbeats [sec: Bandcamp Daily 2022 and 2025 via Wikipedia]. Moods listed: "waver between bliss and unease", "melancholic contemplation", "bombastic set pieces", "chaotic fusillade", an ambient passage cut by hard gabber kicks.

### 3.2 "Emotional breakcore with classical instruments" in neutral terms
The single review of a 2025 album in this lane (not an authoritative source) describes **classical instruments layered with electronic production, piano melodies, occasional saxophone and choir over fast electronic drums**, recurring motifs between tracks, and a half that is more ambient [sec: thecynamite1.blogspot.com, Dec 2025]. A fan page says a track in the lane was made with "piano, breakcore drums and effects" [sec, low reliability]. Production-term translation for prompts [inf]:
1. Solo piano (felt, rubato, reverb-soaked) states the theme in D/F#/A/C/E minor, then **the same theme is replayed** on the supersaw lead or plucked arp when the break drops. One motif per track is what makes it "emotional" rather than "noisy".
2. Build with strings (staccato ostinato), choir pads, timpani/snare rolls, filter sweep and noise riser; pitch and rate of the snare roll accelerate (8th -> 16th -> 32nd).
3. Drop: chopped break at full speed, distorted kick, reese + sub, lead plays the motif. Keep a 2nd-half **breakdown** to piano + pad + half-time hits, then rebuild (what the ULTRAKILL-style boss prompts in `music.json` already do).
4. Ambient interlude between hard sections (pads, tape hiss, a filtered break ghost) gives the player's ear a rest.
5. Key and BPM hold across a track; tempo changes happen by half-time/double-time feel rather than a new BPM.

### 3.3 What makes ULTRAKILL-type music work in gameplay (production terms) [sec: ultrakill.wiki.gg/wiki/Soundtrack, ultrakill.fandom.com/wiki/Music]
- The soundtrack "mixes industrial metal and breakcore"; calmer tracks mostly sit in secret levels.
- **Most music is split into calm and combat variants that switch with combat state** (the wiki says the exact trigger logic is undocumented). Some tracks **progress by place or event**: drone in the tutorial, a guitar hit when the revolver is picked up, then the combat loop; "progresses once the player enters the lower floor"; "progresses every 2 rooms"; ten phases for one boss.
- **Boss themes are per boss**, some with two phases or their own calm/combat variants.
- Instruments: Amen break samples, recorded guitar and bass over MIDI soundfonts [sec: wiki + a Steam post attributed to the composer], unusual sources (a 1908 recording, a death whistle), frequent time-signature changes in some tracks, slowed-down reuse of earlier material.
- An earlier experiment that changed music by equipped weapon was reportedly dropped because it broke tempo and intensity [sec, fan wiki].
- What to copy as a design principle: one **key and palette per level**; calm = same harmony with the drums removed or half-time; combat = full break; boss = bigger, new key centre, choir/organ/guitar; progression through the level instead of loop-forever. All already in PG3-SOUND-PROMPT section 2.

### 3.4 Prompt phrasing map for the 3 track roles [inf]
- COMBAT: "Instrumental emotional breakcore for a fast first-person fight, <key>, <bpm> BPM. Relentless chopped and re-sequenced amen and think breaks with glitch edits and stutter fills, distorted punchy kicks, reese and 808 sub bass, detuned supersaw hook that restates the piano motif, short piano stabs between phrases. Loud modern mix, no vocals, no fade out."
- CALM: "Instrumental ambient interlude, same key, half the tempo feel (<bpm/2> BPM), felt piano motif, slow pads, sub drone, filtered breakbeat ghosting very quietly under it, sparse, no vocals."
- BOSS: "Instrumental boss theme, <key>, <bpm> BPM. 0:00-0:12 organ, choir and timpani swell; 0:12 chopped breaks, blast-beat fills, downtuned distorted guitar riff, roaring reese; 1:10 breakdown with solo harpsichord and strings rebuilding into the full break; final drop with the theme in the choir."

---

## 4. Game-audio practice for a fast first-person shooter

### 4.1 Gunshot layering [sec: liortega.com FPS gun sound design; violetrecording.com; DuttonAudio blog]
- Five layers, controllable separately: **transient** (the click/crack; clip before limiting to keep it sharp), **body** (2-3 mic signals, EQ + saturation), **sub/weight** (about 80 Hz translates on laptops), **mechanical** (cylinder, hammer, pump; a mech hit just before the transient makes the gun feel bigger), **tail** (reverb/environment; keep dry shot separate so the engine can add room reverb; make indoor/outdoor/distant tails).
- Failure modes: strong body with no transient = muffled; sharp transient with no body = thin/toy-like. Small timing offsets between transient and body noticeably change perceived size.
- Sci-fi/energy weapon: "synth tone with fast pitch and filter modulation" plus a noise/electric texture plus a **real mechanical layer**; Titanfall 2 gave sci-fi guns physical credibility by treating them like ballistic guns, e.g. processed animal barks layered into an energy shotgun, and swapping tails per room type [sec: asoundeffect.com Titanfall 2 interview].
- Sweeteners: the 4th/5th layer that carries the character (electric zap, bell, whip crack); keep under -6 dB below body so it flavours, not leads [inf].
- Print layers at their intended in-engine level [sec].

### 4.2 Variation
- Rules of thumb [sec: bugnet.io, uhiyama-lab, others]: **4-8 variations** for frequently-played sounds (3-5 for footsteps); random pick that **never repeats the last index**; subtle per-play pitch about **+-2-5 %** (~35-85 cents) and small volume jitter. No authoritative source gives exact numbers; PG3's +-4 % pitch, +-1.5 dB is in the band. Pitch jitter on a gunshot's transient shifts perceived size; prefer jitter on tail/sweetener and less (+-2 %) on the transient [inf].
- With layered shots, vary the layers independently (3 body x 3 mech x 2 tail = 18 combinations from 8 clips) [inf, supported by the "layers should hold multiple variations" workflow].

### 4.3 Voice limiting and priority
- Wwise: a voice limit is counted in **voices not events**, lowest priority are kicked first, virtualised voices keep their state without mixing; default below-threshold behaviour "Kill if finite else virtual"; Audiokinetic's CPU guidance budgets under about 80 channels on PC/consoles [sec: Audiokinetic Q&A and blog].
- Unity [doc: Audio settings reference]: **Max Real Voices** = how many play at once ("every frame, the loudest voices are picked to play"); **Max Virtual Voices** "should always be larger than the number of voices played by the game". Default values: not stated on the page (the Editor's project settings show them). Use `AudioSource.priority` (0 = highest) on UI, player gun, hit confirms, boss; lower on distant enemy foley.
- Cap per-sound instances for rapid-fire sounds: torch 1 loop, hit tick max 3-4 simultaneous, enemy footsteps/hurt per type 2-3 [inf].
- Overwatch needed an "importance" system because plain HDR mixing means "the loudest sound wins": it dims low-threat sounds based on who is looking at you, shooting at you, and distance [sec: PC Gamer on the GDC 2016 talk]. Titanfall 2 prioritised by friend/enemy, whether a weapon is aimed toward you, and the player's health, with attenuation + filtering for occlusion [sec: asoundeffect.com]. Both are enemy-aware mixing; for SKYGRIND: boost tells of enemies with line of sight and in the aimed-at cone; thin out far-field spam.

### 4.4 Ducking
- DOOM 2016 audio lead: "Weapon sounds never get ducked by anything, not even VO"; weapons "the main character"; the mix was built around them [sec: ADSR interview with Chad Mossholder and Ben Carney]. So duck **music and ambience**, not guns.
- Unity AudioMixer: put a **Duck Volume** on the Music group and a **Send** on the SFX group pointing at it; Duck Volume is a sidechain compressor; a failure mode is a Send that targets the wrong group. One tutorial's start values: threshold -40 dB, ratio "300 %", attack 100 ms, release 200 ms, recovery in about 2 s [sec; starting point only]. For PG3: duck music **4-6 dB** under boss slams, big kills and parry (plan in PG3-SOUND-PROMPT) with ~20-40 ms attack, 400-800 ms release [inf]; gate on events with a mixer snapshot or an exposed parameter rather than a sidechain on every gunshot (the gun rate would pump the music).

### 4.5 Hit confirm vs kill confirm
- The Overwatch hit-pip brief: "cut through the mix but not feel like it comes from any hero"; built from a reversed, lightly tweaked beer-can sound [sec: PC Gamer / Kotaku on the source book]. Lesson: the confirm is a short, bright, non-weapon-coloured transient that sits above the gun's spectrum.
- Design (inf): **hit** = 40-80 ms bright click (3-6 kHz) + a tiny low thud underneath, 3-4 variants, rising pitch if hits chain within 0.4 s (semitone steps, cap +5); **headshot/weak-point** = same click plus a bell/ping tail (the cyan "shoot here" rule), higher; **kill** = longer (300-700 ms) with a low impact + rising chime, one per enemy class (see section 5), distinct from hit; **multikill** = a rising stinger; **rank-up** = a short ascending 3-note figure. Reward sounds rise and are major/bright, damage falls and thuds (PG3-SOUND-PROMPT section 3).
- Keep hit ticks out of the music's band by pitching them to the music key: a minor-key track with a tick at the tonic's octave harmonises instead of clashing [inf].

### 4.6 Low health, hit stop, slow motion, parry, charge, UI
- **Low health**: one Unity pattern is a Master low-pass blended by health between two snapshots, default = max cutoff, filtered = **150-300 Hz** (very strong), `TransitionToSnapshots` 0.2 s, blend weight from `threshold (50) - health` [sec: gamedevbeginner.com]. Another dev says the cutoff should move gradually to sound natural [sec: moddb]. For an action game keep the gun transients audible: low-pass the **music and ambience** harder (1-2 kHz at 10 % hp) and the SFX bus only mildly (4-6 kHz), add the heartbeat loop (60 -> 110 bpm as hp falls) and a slight sub boost [inf]. Titanfall 2 feeds player health into its mix parameters [sec].
- **Hit stop (30-80 ms freeze)**: no source found specific to hit stop. Treatment [inf]: do not stop the AudioListener; let the impact layer play and duck the music 3-6 dB and low-pass it to ~3 kHz for the freeze, release over 150-250 ms.
- **Slow motion**: lowering pitch is the usual shortcut; sharp sounds like gunshots **lose punch when stretched**, so designers sometimes swap in different sounds [sec: CRIWARE blog]; applying both pitch shift and a low-pass to all sounds scaled to how far time slowed, removed on speed-up, is one jam dev's approach [sec: itch.io]; Unreal clamps global pitch at ~0.4 (not Unity; Unity pitch range: **not checked**) [sec]. For SKYGRIND's very slow ride/zoom (GG: "extremely slow"): bus pitch to roughly timeScale^0.5 (not 1:1, e.g. 0.25 time -> 0.5-0.6 pitch), low-pass cutoff scaled with it, music at 0.8-0.9 of that pitch drop, plus a dedicated slow-mo sub-whoosh bed so punch is not lost; every shot fired in slow-mo plays the normal-speed transient layer only (skip the body) [inf].
- **Parry**: sources are community commentary: a correct Sekiro deflect gives "a distinct metallic clang", brighter and higher for blades, duller for punches [sec: gamepur.com, Steam thread]; a player-feedback post on another game complains that a parry sounded like a heavy attack swing, i.e. the success sound must be unmistakably different from any attack [sec: Enshrouded upvote board]. ULTRAKILL telegraphs a parryable attack with a yellow flash and a sound cue and an unparryable one with blue and a different cue; a parry briefly freezes the game and flashes the screen [sec: ultrakill.wiki.gg/wiki/Parry]. Design (inf): **tell cue** (rising two-tone chirp in the window) -> **parry hit** (bright metallic clang + ring tail + a downward sub thump, played at +3 dB over other SFX, 2 variants) -> **hit-stop duck** -> a short shimmering reward sweep; the failed parry is a dull thud with no ring.
- **Charge-up**: no source found. Design (inf): a looped tone/noise bed whose `AudioSource.pitch` is driven by the charge fraction (e.g. 0.7 -> 1.6) and whose low-pass opens as it fills (PG3 already plans to "poll the loadout's charge fraction"), then a **ready ping** at 100 %, then the release crack. Two-stage charges get one detent click at the step.
- **UI**: no authoritative source found. Short (60-200 ms), dry, music-key-aligned tones; move < confirm < back in loudness-weight; three variants of "move" only if it can chain fast [inf]. Trim a 0.5 s API render to length offline.

### 4.7 Adaptive music: horizontal vs vertical
- **Horizontal re-sequencing**: switch between loopable sections (calm loop, tension loop, victory sting) at beat/bar boundaries so the change lands musically. **Vertical layering**: synchronised stems (drums, bass, lead, pads) mixed by intensity on a shared timeline. **Stingers**: short phrases triggered by events, quantised to the beat. A simple start: one base loop plus one intensity layer driven by a single gameplay parameter [sec: Audiokinetic interactive-music blog, bugnet beginner guide]. A one-bar musically timed fade between playlists is common in Wwise projects [sec: Audiokinetic Q&A]; no source gives a standard crossfade time.
- **Fit to PG3**: the calm and combat tracks are separate renders, so their beat grids do not align. Options, best first [inf]:
  1. **Layering from one render**: generate each level's combat track, `stem-separation` it into drums / bass / other (six-stem id gives more), play the full mix in combat and fade drums + bass out to calm (the "other"/pads/piano stem keeps playing). Same render = same grid, so a 0.5-1 s fade is seamless. Calm = stems 'other' only, plus a separately generated calm bed under it.
  2. **Reference-conditioned pair**: generate calm with `conditioning_ref` on the combat track so tempo/key/palette agree; still crossfade without beat-matching: 1.5-2 s equal-power fade (1 bar at 174 bpm = 1.38 s, 1.5 s is about a bar).
  3. **Entry stingers**: a short riser or cymbal swell (1-2 s) over the crossfade hides the grid mismatch when entering a fight; a cut to the calm bed after a kill-all room goes through a "last-enemy" hit-stop duck.
- Boss entry: fade out the level track in 0.8 s, 1-bar organ/choir swell, boss track starts at its first chunk; boss track phases = the plan's chunks as separate files (intro / main loop / phase 2 / defeat sting) so the director can jump chunks at phase changes (ULTRAKILL splits one boss fight into up to ten phases) [sec].
- Keep one music source stream per state, not both always decoding: decode cost for 2-3 long Vorbis streams is fine on PC, check on Switch [inf].

### 4.8 Loudness [sec/doc: Designing Sound; Sony via IESD summary]
- Console target **-24 LUFS integrated (+-2 LU)**, true peak **-1 dBTP**, max loudness range ~20 LU, measured with ITU-R BS.1770 over at least 30 minutes of representative gameplay; portable/handheld **-18 LUFS (+-2)**. The older IESD recommendation said -23 LUFS, handheld -16/-18 [Designing Sound 2013; Audio Media International]. Nintendo's current Switch target: **not checked**.
- It is a **whole-mix** target. Practice: normalise assets (Firelight, via Designing Sound), carry through submixes, measure the Player's output. Short one-shots cannot be measured in LUFS reliably (BS.1770 gating uses 400 ms blocks) so normalise SFX by peak/short-term level and trim by ear [inf].
- Starting mix [inf]: music stems ~ -20 LUFS each, SFX bus such that gunfire peaks at about -6 to -3 dBFS and the whole 30-min Player capture lands at about -18 to -20 LUFS integrated (handheld-friendly, also fine on PC); master limiter at -1 dBTP; one exposed master trim for the platform. PG3-SOUND-PROMPT's "SFX -16 LUFS, music -18 LUFS" per file is higher than the whole-mix targets above: fine if the master/bus gains pull the capture to target, but check with a capture.
- Hit-confirm and parry cues should be the loudest-perceived short sounds after the guns; ambience and UI the quietest.

---

## 5. Recommended sound list (layers, variants, prompt cues)

Notation: **L1/L2/L3** = layers to generate separately (ElevenLabs recommends generating components separately [doc]) and pre-mix offline with ffmpeg `amix` + a 0-15 ms offset (or play as separate AudioSources for runtime randomisation). **xN** = round-robin variants (separate API calls, `prompt_influence` 0.3-0.5). Durations are `duration_seconds` to request (min 0.5, trim after). Existing ids from `Tools/pg3/sounds*.json` in brackets. All dry (tail kept short) so engine reverb adds the room [sec].

### 5.1 Weapons

| Verb / event | Layers (2-3) | Variants | Notes |
|---|---|---|---|
| **KESTREL revolver shot** [fire-kestrel, -2, -3] | L1 sharp metallic crack, tight snappy transient, no low end (0.5 s); L2 punchy short low thump, 120 Hz body, dry (0.6 s); L3 cylinder/hammer click mech 0.5 s played 10-20 ms *before* L1 | **x5** (fires ~2-4 /s); mech x3 | tail = one shared 0.7 s short room tail, ducks under next shot |
| **Charge loop + READY ping** [charge-loop, charge-ready] | L1 rising electric hum loop 2-3 s (`loop`), pitch driven 0.7 -> 1.6 by charge fraction; L2 high crystalline shimmer whose volume rises; **ping** = 0.5 s bright two-note bell | loop x2 (one per weapon colour), ping x2 | Ping must land within 50 ms of the ready state; silent until charging |
| **KESTREL charged pierce shot** [fire-kestrel-charged] | L1 deep boom + beam zap whoosh (1.5 s); L2 piercing high "railgun" crack with a downward pitch sweep; L3 sub drop + metallic ring tail | **x3** | most expensive sound of the revolver; sidechain music -4 dB |
| **Ricochet** [ricochet] | L1 bright ping off metal with a falling pitch whine; L2 tiny spark crackle | x3 | cyan-coloured ring |
| **THUNDERCLAP pump shotgun shot** [fire-thunderclap -2 -3] | L1 enormous low blast, boomy body (1 s); L2 crunchy distortion crack on the transient; L3 pellet-spread airy hiss + short slap tail | **x4** | wall of low end; 40-60 Hz sub boosted in L1 |
| **Pump** [pump] | L1 two crisp metal clacks (rack back / forward) 0.5 s; L2 shell eject tink | **x3** | pitch +-2 %, never ducked |
| **Slug** [fire-slug] | L1 heavier single crack with a long low rumble; L2 pressure-wave "thud-whoom" and a ricochet-like whistle; L3 impact crunch for the hit | x3 | distinct from the pellet blast so the player hears which ammo they fired |
| **Gauntlet punch** [fire-gauntlet, -2] | L1 air whoosh into hard meaty impact; L2 electric crackle; L3 servo/hydraulic click at start | **x4** | impact layer is the confirm; pitch-up on the 3rd chain punch [inf] |
| **Gauntlet parry** [parry, parry-2] | tell cue (rising two-tone chirp) at window open; L1 bright metallic clang + ring tail 1.2 s; L2 downward sub thump; L3 shimmering reward sweep 0.5 s after | clang **x3**, chirp x2 | the loudest SFX after the guns; triggers hit-stop duck and music -5 dB |
| **Pulse blast** [gauntlet-pulse] | L1 charged energy release shockwave boom (1.4 s); L2 rising-then-collapsing air sweep; L3 debris scatter | x3 | |
| **Flamethrower** [fire-torch, torch-loop] | start: ignition whump 0.5 s; **loop**: steady roaring gas flame, low rumble, soft crackle, 2-3 s `loop`, volume follows heat; stop: gas-valve cut + flame sputter 0.6 s | start x3, loop x2, stop x3 | one loop voice only; ramp volume with heat; no per-shot sound |
| **Weapon swap / ADS** [weapon-swap, ads-in] | quick metallic sliding click + soft cloth/holster | x3 | ADS adds a low-passed 'lens-in' tone 0.3 s |
| **Overheat / empty** | dull click / hiss | x2 | |

### 5.2 Movement

| Event | Layers | Variants | Notes |
|---|---|---|---|
| **Dash** [dash, dash-2, dash-fail] | L1 fast air whoosh with a bright upward filter sweep (0.5 s); L2 low cloth/thump on takeoff; L3 sub swoosh | **x4**; fail = soft dull sputter x2 | stamina-ready tick on recharge [stamina-full] |
| **Slide** [slide-start, slide-loop] | start: scrape + whoosh 0.6 s; **loop**: continuous friction/grit slide, pitch with speed (`loop`, 2 s); end: scrape-off + small thud | start x3, loop x2, end x2 | loop pitch/volume from speed; stop abruptly when airborne |
| **Wall jump** [wall-jump] | L1 shoe/boots scuff on concrete + quick push whoosh; L2 short rising 'boing' tone (stylised, the player's cyan) | **x4** | pitch +-3 %; louder than a floor jump |
| **Wall ride/run** [wallride] | scraping loop with fabric/skid, pitch with speed | loop x2, start x2 | |
| **Jump / double jump / land** [jump, jump-2, double-jump, land, land-2, land-hard] | jump: short effort whoosh; double: airy burst + sparkle; land: soft thud, hard land: deep body thud + debris | jump x4, double x3, land x4 (soft) + x3 (hard) | speed-scale the land by fall velocity |
| **Grapple** [grapple, grapple-attach, grapple-release, ropeFire] | fire: sharp launch 'thwip' + cable whir (0.6 s); attach: metallic clamp thunk + short ring; **reel loop**: taut cable creak/whine rising with speed (`loop`); release: snap + slack rattle | fire x3, attach x3, loop x2, release x2 | reel loop pitch follows rope tension |
| **Ground slam** [slam, slam-2] | L1 heavy downward whoosh (the fall); L2 huge ground-impact boom + sub drop, crack of concrete; L3 radial shockwave ring 0.8 s + debris pebbles | **x3** | -6 dB music duck 600 ms; slam shockwave visuals sync |
| **Jump pad / bounce** [jump-pad] | spring/boing + whoosh | x3 | |
| **Stamina full** | tiny bright 2-note chirp | x1 | one of the quietest cues |

### 5.3 Combat feedback

| Event | Layers | Variants | Notes |
|---|---|---|---|
| **Hit confirm** [hit, hit-2, hit-3] | bright click (3-6 kHz, ~60 ms) + tiny thud under it | **x4**, pitch +1 semitone per hit in a 0.4 s chain (cap +5) | max 3 simultaneous voices |
| **Headshot / weak point** [headshot] | click + bell ping + short shimmer | x3 | higher and longer than the hit; the "cyan = shoot here" language |
| **Kill** [kill, kill-2, kill-3 + per-kind] | low crunch/shatter (the enemy's material) + rising chime | x3 generic + per-kind (below) | 300-700 ms, clearly longer than a hit |
| **Multikill / chain** [multikill] | rising 3-note stinger + whoosh | x2 | style-rank-up [rank-up] separate, brighter |
| **Player hurt** [hurt] | dull impact + short bodily grunt-free thud + low-pass wobble | x3 | **no voice**, low-pass the SFX bus 200 ms |
| **Low health** [heartbeat-loop] | heartbeat loop 60 bpm, low and dry (`loop`); optional tinnitus-ish tone at <15 % | loop x2 (60 and 100 bpm) | crossfade between loops with hp |
| **Death / lose** [lose] | long falling sweep + sub drop + reverb tail | x1-2 | pairs with the death sting music |
| **Pickups / points / buy / power-up** | arcade-bright chimes, short | x3 each for points/pickup, x1-2 others | rising = good |

### 5.4 Enemies (per kind: tell, attack, death)

Principle: the **tell** is the most important enemy sound: readable 0.3-0.8 s before the attack, distinct per kind, red-orange/danger-coloured (rule 8) in timbre: rough, falling, low. Death sounds are the kind's material and are longer than a kill-confirm.
| Kind | Tell | Attack | Death | Variants |
|---|---|---|---|---|
| husk [husk-attack, husk-death] | low gravelly wind-up groan, rising | stone-fist slam | stone shatter + dust | tell x3, attack x3, death x4 |
| swarmer | high chitter, short | pounce hiss + thump | squelchy pop | x4 each (high rate: strict voice cap 4) |
| spitter [spitter-tell, spitter-volley] | gurgle swell | wet launch 'thwup' | splat | x3 |
| charger [charger-charge] | stomp + metallic rev-up | thundering charge loop-ish 1.2 s | heavy collapse + metal rattle | x3 |
| warden | shield hum rises | [warden-block] metallic deflect clang | shield shatter + slump | x3 |
| drone [drone-shot] | whine up | zap shot | electric fizz + crash | x3 |
| burrower [burrow-emerge] | ground rumble, rising | emerge burst + debris | collapse | x3 |
| spawn telegraph [spawn-telegraph] | rising magical/portal whoosh 1 s | | | x2 |

### 5.5 Bosses

Layers common to all: **intro** (4 s: entrance stinger + roar/mechanism), **tell** (1.2 s, the readable warning per attack), **slam/attack** (heavy, with sub), **hit** (impact on boss; weak-point version with a bell), **stagger/window** (a falling groan + "open" ping), **phase change** (2 s transformation), **down** (4-5 s: collapse + long tail + resonant stinger). Each boss has its own material so players learn the phase from sound alone.

| Boss | Material and motif | Intro / tell / slam / down cues |
|---|---|---|
| **THE WICK** (flame-cage) [wick-*] | crackling fire, iron bars, gas roar, furnace thud | intro: iron cage slam + ignition whump + roar; tell: rising gas hiss + bar-clank; slam: fire burst boom; down: cage collapse + guttering flame fade-out |
| **THE ICEBREAKER** (ice golem) [icebreaker-*] | cracking ice, glassy shards, heavy stone-ice footfalls, frost wind | intro: ice crack + deep groan; tell: groaning creak + high glass chime; slam: massive crack-boom + shards tinkle; down: shattering collapse |
| **THE KILN WYRM** (giant worm) [wyrm-*] | wet-stone dragging, deep rumble, hiss/screech, rock debris | intro: burrowing rumble + roar; tell: ground vibration swell; slam: body slam + rock rain; down: long dying screech + collapse |
| **THE LOOM** (six-legged machine) [loom-*] | six-legged metallic stomps (rhythmic 6-step), servo whines, taut-thread twang, clockwork ticks | intro: ratcheting mechanical wake-up; tell: thread-tension whine; slam: leg stomp cluster; down: servo wind-down + clatter |
| **THE CROWN** (spinning blades) [crown-*] | blade whirr, sharp metallic shing, ringing bells (spire), organ swell | intro: bell + blade-spool ring-up; tell: rising whirr pitch; slam: shing + impact; down: blades slowing (pitch fall) + final bell |
| shared: phase [boss-phase], stun [boss-stun], window [boss-window], shield [boss-shield], volley [boss-volley], roar [boss-roar], hit [boss-hit, -2] | | |

Variants: intro x1-2, tell x3 per attack kind, slam x3, hit x4 (fast), phase x2, down x2.

### 5.6 UI and flow
[ui-move, ui-confirm, ui-back, round-begin, room-enter, room-clear, checkpoint, gate-open, exfil-call, box-open, win, retry]: short dry tones keyed to the music (e.g. D minor stack), move x3 (chainable), confirm x2, back x2, others x1-2. Stingers (clear, death, campaign-complete) are music-length cues generated with the Music API: 4-8 s, `music_length_ms` 5000-8000, "ends on a hard stop".

### 5.7 Music list (what to add beyond the 21 tracks now in `music.json`)
- **Per level (5)**: combat, calm (exist) + **a "tension" layer** or a second combat variant (higher intensity, new fill density) generated with the same plan (v2.5 `seed` fixed), to switch to when the wave count or style rank is high; boss intro cue (organ/choir swell 8-12 s); boss phase-2 variant of the boss track (higher BPM +6, drums doubled) [inf].
- **Title, select, results (exist)**, plus **a piano-only variant of title** (menu idle) and **an ambient pause-menu bed** (low-passed combat).
- **Stingers (generate with `music_length_ms` 3000-8000)**: round start, last-enemy-alive, boss defeated, perfect clear, level up/rank, death. Each in the level's key if possible.
- **Interludes**: 30-60 s ambient pieces (pads, tape noise, piano) for loading screens / hub: "ambient interlude" with the filtered break ghost.
- **Stem plan**: for the 5 combat tracks and 5 boss tracks, run stem separation (six-stem) and ship drums / bass / other as three layers (adds 10 x 2 files).
- **Count target**: 21 now -> 40 with stings/variants/interludes. At 900 credits/min, 40 x ~70 s avg = ~47 min = ~42k credits (Creator month = 121k).

---

## 6. Not checked / open questions
1. Real credit price of a music minute and an SFX call on this account (pages disagree); `eleven_text_to_sound_v3` details; whether v2.5 `prompt` generation beats v1 on our prompts (needs the A/B in item 0.1).
2. Whether the plan's PCM/WAV output is available (tier gates; 48 kHz tier not stated) and whether the stem endpoint is open to our tier.
3. Inpainting plan limits (enterprise-only claim vs guide) and `composition_plan.create` REST path.
4. Soundtrack-as-DLC and the exact reading of Studio Game vs Indie Game for SKYGRIND on Steam: GG to read https://elevenlabs.io/music-terms (Sections 2.1, 3.3, 4.2, 4.4, 4.7, 4.8 and Exhibit A).
5. Nintendo Switch loudness spec; Unity defaults for Max Real/Virtual Voices; Unity `AudioSource.pitch` range for deep slow-mo.
6. No authoritative production breakdown of AZALI-lane tracks or of ULTRAKILL's sound effects exists in what I found; section 3.2 and 5 design values marked [inf] come from craft judgement and need blind A/B in a Player (L-001: feel is done when measured).

---

## Sources

ElevenLabs (official)
- Compose Music API: https://elevenlabs.io/docs/api-reference/music/compose
- Compose detailed: https://elevenlabs.io/docs/api-reference/music/compose-detailed
- Separate stems: https://elevenlabs.io/docs/api-reference/music/separate-stems
- Music overview: https://elevenlabs.io/docs/overview/capabilities/music
- Music prompting best practices: https://elevenlabs.io/docs/overview/capabilities/music/best-practices
- Music quickstart / cookbook: https://elevenlabs.io/docs/eleven-api/guides/cookbooks/music
- Composition plans: https://elevenlabs.io/docs/eleven-api/guides/how-to/music/composition-plans
- Music inpainting: https://elevenlabs.io/docs/eleven-api/guides/how-to/music/inpainting.md
- Music streaming: https://elevenlabs.io/docs/eleven-api/guides/how-to/music/streaming.md
- Help center, prompting Eleven Music: https://elevenlabs.io/docs/help-center/product/core-capabilities/music/are-there-any-best-practices-for-prompting-eleven-music
- Music terms (updated 09 Oct 2026): https://elevenlabs.io/music-terms and https://elevenlabs.io/eleven-music-model-specific-terms
- Sound effects API: https://elevenlabs.io/docs/api-reference/text-to-sound-effects/convert
- Sound effects overview: https://elevenlabs.io/docs/overview/capabilities/sound-effects
- Sound effects playground: https://elevenlabs.io/docs/eleven-creative/playground/sound-effects.md
- How to prompt SFX (help center): https://elevenlabs.io/docs/help-center/product/core-capabilities/sound-effects/how-do-i-prompt-for-sound-effects.md
- ElevenLabs skills (sound-effects, music): https://raw.githubusercontent.com/elevenlabs/skills/main/sound-effects/SKILL.md and .../music/SKILL.md
- Pricing: https://elevenlabs.io/pricing and https://elevenlabs.io/pricing/api; 429 help article: https://help.elevenlabs.io/hc/en-us/articles/19571824571921-API-Error-Code-429
- Index used to find pages: https://elevenlabs.io/docs/llms.txt

Secondary
- Third-party prompt guide: https://fal.ai/learn/biz/eleven-music-prompt-guide ; price listing: https://developer.puter.com/tutorials/elevenlabs-api-pricing/
- Breakcore: https://en.wikipedia.org/wiki/Breakcore ; https://emastered.com/blog/what-is-breakcore-music ; Bandcamp Daily https://daily.bandcamp.com/lists/breakcore-revival-list and https://daily.bandcamp.com/lists/new-breakcore-guide ; DnB https://en.wikipedia.org/wiki/Drum_and_bass ; speedcore https://en.wikipedia.org/wiki/Speedcore
- Emotional/classical lane (single review): https://thecynamite1.blogspot.com/2025/12/7-december-2025-voices-from-true-world.html
- ULTRAKILL music: https://ultrakill.wiki.gg/wiki/Soundtrack ; https://ultrakill.fandom.com/wiki/Music ; https://ultrakill.wiki.gg/wiki/Parry ; Hakita interview https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
- Gun design: https://www.liortega.com/post/first-person-shooter-gun-sound-design ; https://violetrecording.com/how-to-design-weapon-sounds/ ; https://duttonaudioproject2.blogs.lincoln.ac.uk/?p=115
- DOOM 2016 audio Q&A: https://www.adsrsounds.com/news/the-sound-design-of-doom-exclusive-qa/ ; Mick Gordon GDC 2017 summary: https://gdconf.com/news/gdc-2017-get-behind-scenes-look-composing-music-doom
- Titanfall 2 audio: https://www.asoundeffect.com/titanfall-2-sound/
- Overwatch: https://www.pcgamer.com/how-overwatch-uses-sound-to-pinpoint-threats-you-cant-see ; https://www.pcgamer.com/overwatchs-hit-sound-effect-is-apparently-powered-by-beer/ ; GDC Vault talk "Overwatch - The Elusive Goal: Play by Sound": https://gdcvault.com/play/1023010/contactUs
- Variation counts: https://bugnet.io/blog/how-to-implement-footstep-variation-to-avoid-repetition ; https://uhiyama-lab.com/en/notes/ue/sound-cue-vs-sound-wave-best-practices/
- Voice limiting (Wwise): https://blog.audiokinetic.com/how-to-get-a-hold-on-your-voices-optimizing-for-cpu-part-1/ ; Unity Audio settings: https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/editor-settings-reference/comp-manager-group/class-audio-manager ; Unity AudioMixer inspectors (Duck Volume): https://docs.unity.com/en-us/engine/6000.3/manual/audio/mixer/inspectors.md ; ducking troubleshooting: https://bugnet.io/blog/fix-unity-audio-mixer-ducking-compressor-not-triggering
- Low-health filter: https://gamedevbeginner.com/create-a-low-health-audio-filter-in-unity-using-the-audio-mixer-transition-to-snapshots/ ; slow-mo audio: https://blog.criware.com/?p=3012 and https://forums.unrealengine.com/t/how-can-i-slow-down-all-sounds-and-assign-normal-sensitivity-while-in-slow-motion/288557
- Parry commentary: https://vip-develop.gamepur.com/?p=8339 ; https://enshrouded.featureupvote.com/suggestions/728656/parry-audio-feedback
- Adaptive music: https://www.audiokinetic.com/en/blog/how-to-use-interactive-music-to-score-gameplay/ ; https://bugnet.io/blog/adaptive-music-a-beginners-guide ; Wwise transition threads https://blog.audiokinetic.com/qa/2611/transition-segments-alongisde-previous-playlist-containers
- Loudness: https://designingsound.org/2013/02/loudness-in-game-audio/ ; https://designingsound.org/2013/02/20/different-loudness-ranges-for-console-and-mobile-games ; IESD levels https://www.audiogang.org/wp-content/uploads/2015/04/IESD-Mix-Ref-Levels-v03.02.pdf ; Audiokinetic mastering https://audiokinetic.com/en/mastering-a-game-with-wwise-part1
- MP3 loop gaps: https://forum.unity.com/threads/gapless-mp3-playback.66090/ ; https://www.jwhitham.org/2023/09/system-shock-soundtrack-gapless.html

Project evidence: `Tools/pg3/music-gen.cjs`, `Tools/pg3/sfx-gen.cjs`, `Tools/pg3/music-gen-log.json`, `Tools/pg3/sfx-gen-log.json`, `Research/2026-10-28-pg3-audio/probe/headers.txt`, `docs/pg3/PG3-SOUND-PROMPT.md`.
