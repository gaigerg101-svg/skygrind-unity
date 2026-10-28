# DESIGN-RESEARCH.md: what the best games do, and what SKYGRIND does next (Unity only)

Researched 2026-10-02 for GG ("you decide... more research on best game design mechanics, everything, and see what we can
still do... Unity only"). Five parallel research passes covered:
- movement-shooter combat
- wave / horde survival
- roguelite progression and retention
- Unity 6 engineering
- handoff methods (in HANDOFF.md)

I verified every claim about SKYGRIND's own numbers against `PortExport/config.json` or the Unity code.

**Tags:**
- **[V]:** I checked it in this repo today.
- **[R]:** from the research, with its source below (not re-opened by me).
- **[E]:** my estimate.

Drive and Gmail had no SKYGRIND material, so the local repos are the ground truth.

## 1. Bottom line

SKYGRIND already copies its best exemplars number for number [V]:
- the dash: 3 pips regenerating at 0.7/s (ULTRAKILL Standard)
- weapon freshness: -0.7 per use, +0.5 to the others, +0.1/s (ULTRAKILL)
- points: 10 / 60 / 100 / 130 (CoD Zombies)
- at most 24 alive (CoD)
- a BUILD_UP / PEAK / RELAX director (Left 4 Dead)

The gaps are not in the core verbs. They are in five places:

1. **Nothing contests the player's space.** Only melee chasers exist, so movement collapses into kiting and there is
   nothing to parry at range.
   - Every combat source names ranged and area-denial enemies with readable, parryable attacks as the next step.
   - The web already has the SPITTER tuned [V]: globs 16 m/s, puddles 3 m / 4 s / 10 dps.
2. **Points stop mattering around round 6-8.** The five gates cost 6,500 in total [V], and the only other sink is one
   wall gun [V].
   - Every horde game adds sinks as HP grows: Pack-a-Punch, perks, the box [R].
   - The web has them ready: perks, the FORGE (5000, x1.8), kill drops.
3. **The first 15 minutes have friction the web had already fixed.**
   - In Unity: no settings (sensitivity / FOV), restart behind DOWN + RESULTS, no death recap, no damage direction
     [V: Ui.cs].
   - A Next Fest demo is played a median 18 minutes, and reviews judge controls first [R].
4. **Feedback is thin.**
   - The trauma shake (fx/trauma.js) isn't ported [V].
   - Freshness is invisible [V].
   - The parry hitstop is 4x shorter than ULTRAKILL's [R].
   - No muzzle flash, tracers or impacts yet [V: PORT-STATUS].
5. **The director's valley can vanish.**
   - Intensity decays even while enemies are on you [V: decay 0.05/s always].
   - RELAX starts while the peak's enemies are still alive [V].
   - L4D has a PEAK FADE state and pauses decay during engagement [R: Booth GDC 2009].

**So NIGHT8 builds the Demo Day build** (`tasks.json` stage 2a), in this order:
1. foundations: zero-alloc frames, the HUD, settings, restart and recap
2. the SPITTER with parryable globs and speed-scaled enemy aim
3. the economy sinks: perks, kill drops, MODS
4. the feel parity pass, and two research additions that change no earlier decision: PEAK FADE and a perfect parry
5. a Unity Web build check, so GG can choose what peers play on Oct 7

Design changes that would reverse GG's earlier calls or re-tune the web's balance are in section 4. They go in as a LAB
(experiments) settings tab, off by default, so Demo Day peers can A/B them.

## 2. Ranked mechanics: research to SKYGRIND

Impact: H / M / L. Effort in Unity: S (an item), M (a phase), L (several nights). "Where" is the task in `tasks.json`.

| # | Mechanic | Exemplars and evidence | SKYGRIND now | Plan | Imp / Eff | Where |
|---|---|---|---|---|---|---|
| 1 | **Ranged and area-denial enemies with readable, parryable attacks** | Doom Eternal's demons as "chess pieces" to "move the player around" [R: Hugo Martin, Gamereactor]. ULTRAKILL difficulty comes "from composition, not count" [R: Hakita interview]. L4D2's Spitter was made to scatter campers [R: bit-tech]. Projectiles slow enough to weave through [R: Dark Ages interview] | Husk and swarmer only; ai/aim.js not ported [V] | Port the SPITTER: globs, puddles, a punch reflects globs (the web's `returnToSender` award), and enemy aim that misses more at speed. Later: charger, lancer (aim beam over 1 s), warden | H / M | U2-10, then U3 |
| 2 | **Economy sinks that grow** | Classic Pack-a-Punch 5000; BO6 tiers 5k / 15k / 30k [R: game8]. Perks: Jugg 2500, cap 4 [R]. Kill drops with a pity timer (KF2 Zed Time rises until guaranteed) [R: PCGamesN] | 6,500 of gates plus one 750 gun [V] | Port 5 perk machines, kill drops (web: 1.5%, x2 after 30 s, x4 after 60 s [V config]), MODS drafts; FORGE next | H / M | U2-11..13, U2-17 |
| 3 | **Zero-friction start and restart** | Neon White: one-key restart; with a loading screen "unplayable" [R]. Dead Cells: "Most reviews talk about controls before" gameplay [R: GDC 2019 slides]. Next Fest demo median 18 min [R: HTMAG] | No settings, restart behind DOWN + RESULTS, no recap [V] | Settings screen from the web's keys, instant restart, death recap, damage direction, personal bests | H / S-M | U2-04, U2-05 |
| 4 | **Layered hit and kill confirmation** | Eiserloh's trauma shake (GDC 2016) [R]. Hitstop scales with impact and is capped (Sakurai) [R]. ULTRAKILL's major hitstop 0.25 s on a parry [R] | Kill hitstop 0.04 s [V]; no trauma shake [V]; no muzzle / tracer / impact [V] | Port fx/trauma.js (config has it [V]), the muzzle flash, tracers and impacts (viewmodel.js), speed lines, and the freshness tier on the HUD | H / M | U2-06 |
| 5 | **A perfect parry inside the normal one** | ULTRAKILL's parry is "(almost) purely offensive" [R]. Dark Ages parry windows run 200-600 ms by difficulty [R]. Parry style +100 [R] | The whole 0.5 s wind-up counts the same [V: IsParry] | Last 0.15 s of the wind-up: heal to max (not +15), a 0.15 s hitstop and white flash, a PERFECT PARRY award. Numbers go in the Unity design config | H / S | U2-07 |
| 6 | **Peak fade, and no intensity decay during engagement** | Booth: "Amplitude (difficulty) is not changed, frequency (pacing) is"; SUSTAIN PEAK 3-5 s, then PEAK FADE until "a natural break", then RELAX 30-45 s [R: Booth GDC 2009 PDF] | Decays always; RELAX starts with up to 24 alive [V] | Pause decay while a token holder is committed within 6 m; PEAK_FADE: no spawns until intensity < 0.6 and at most 2 engaged nearby (6 s cap). Behind `design.director.peakFade`, so the directorMath golden vectors run with it off | H / S | U2-08 |
| 7 | **Help finding the last enemies** | BO6: "the last zombie didn't show up or I couldn't find him" [R: Steam thread] | Web `markersAt: 3` [V]; Unity has no markers | Port the markers | M / S | U2-09 |
| 8 | **Verb-slot drafts with priced offer controls** | Hades: pick 1 of 3, one god per slot [R]. Balatro reroll $5, +$1 each, reset per shop [R]. Banish and skip in Vampire Survivors and Megabonk [R] | Web MODS: 19, 3 offers, 1 reroll for 1500 [V] | Port the MODS as they are now; verb slots, an escalating reroll, skip and banish come later | H / M | U2-13, then U3 |
| 9 | **Speed as armour, speed as damage** | DOOM 2016's AI misses players moving at top speed [R: 80.lv]. Turbo Overkill's chainsaw leg scales with speed [R]. The web's momentum: melee up to x3, guns x1.35 [V config] | Momentum not ported [V] | The enemy aim from #1 scales spread with speed; port momentum (momentumMath has golden vectors) | H / M | U2-10 aim, U3 momentum |
| 10 | **ARCLINE pulls enemies** | Doom's meathook pulls you to enemies [R]. ULTRAKILL's Whiplash pulls light enemies in and you to heavy ones [R] | `arcline.enemyPull 22` not ported [V]; ARCLINE costs 5500 | Port the enemy pull. Consider the ARCLINE earlier or cheaper (GG) | H / M | U3 |
| 11 | **Debut rounds** | CoD dogs: R5-8, then every 4-6, the last drops a Max Ammo [R]. BO6 spaces its specials R6 / 9 / 11 / 16 [R] | Web debuts: 6 kinds in rounds 3-9, none after [V config] | A NEW: KIND card; a debut round mostly the new kind. Respacing is GG's call (section 4) | H / S | U3 |
| 12 | **A run shape with an ending** | BO6 exfil R11 / 16 / 21 [R]. Space Marine 2: extract every 5 waves [R]. Returnal had to patch in a suspend [R]. Vampire Survivors runs 15 / 20 / 30 min [R] | Endless; no win state | EXTRACT after boss rounds from R10, a ported results screen, an overtime curve from R25. GG's call (section 4) | H / M | U5 |
| 13 | **Bosses as the run's spine** | Gears 3 Horde: a boss every 10 waves [R]. L4D boss events are shuffled, with no repeats [R] | Web: BULWARK 5, MONOLITH 10, RIVAL PRIME 15, ORBITER 20, HIVE 25 [V: PORT-STATUS]; none in Unity | BULWARK at R5 with a SUPPLY break first | H / L | U4 |
| 14 | **Variety-only meta, visible goals** | Reviews punish stat grind (Gunfire Reborn, Witchfire, Hades II's currencies) [R]. Hades: Escaped Tartarus 81.8%, 15 prophecies 22.1% [R: Steam stats] | Web `unlocks.js`: "variety, never stats" [R] | A contracts board (30-40) mirrored as Steam achievements; an unlock every 1-2 runs in the first 2 h | H / M | U6 |
| 15 | **Daily seed on a shared board, trials with medals and ghosts** | Spelunky's daily made single player social [R]. Neon White reveals medals and boards as you earn them [R: Vice]. Devil Daggers and Trackmania replay ghosts [R]. Steam leaderboards and AttachLeaderboardUGC [R: Steamworks] | Bit-exact movement (16 / 16 replays) [V]; web daily and ghosts exist [R] | Daily and Trials boards; neighbour-rank ghosts in Trials. The bit-exact sim is SKYGRIND's differentiator | H / M | U6 |
| 16 | **Difficulty that pays (a heat sheet)** | Hades: each Heat point unlocks rewards [R]. BO7 relics 1 / 2 / 3 [R]. Balatro first win 71.6%, Gold stake 12.2% [R] | Web mutators unlock by best round [R] | The mutators as a heat sheet with a score multiplier; cumulative levels after a win state exists | M / M | U5 |
| 17 | **Shareable results** | Balatro and Vampire Survivors grew through streams [R] | Web seed codes [R] | A build card on the results, plus RETRY SAME SEED | M / S | U5 |

## 3. Unity engineering findings

| Finding | Evidence | Fix | Where |
|---|---|---|---|
| `EventBus.Emit` allocates a handler array on every emit | [V] `list.ToArray()`, EventBus.cs line 33 | Index loop with deferred removal; pool or struct payloads | U2-02 |
| The HUD allocates every frame | [V] interpolated strings, a `ps +=` loop, a new StringBuilder per frame, ToHtmlStringRGB | Update only when a value changes, with `SetText`; cache the colour hex; one StringBuilder; split canvases by change rate; raycasters only on menus | U2-03 |
| Play mode rewrites the TMP fallback font asset | [V] three times in NIGHT7; the pips use ■ □ · | A static atlas with every glyph used (or images for the pips) | U2-03 |
| No frame cap, vsync 0 [V], so incremental GC gets only its time slice | [R] Unity GC docs | Apply `settings.fpsCap` and vsync from settings; a GC-counter readout in the bench | U2-02, U2-04 |
| `System.Math` in the sim: Atan2 11, Exp 7, Tan 6, Acos 6, Asin 2, Pow 1, Atan 1 [V] | Exact in the Editor today [V: golden 55 / 55]; not proven in the Player, IL2CPP or Proton [R] | Port them to fdlibm in `JsMath`; run golden and replays in the built Player | U3 eng |
| A MaterialPropertyBlock per enemy breaks the SRP Batcher and the GPU Resident Drawer | [R] Unity 6.3 docs; [V] EnemyViews.cs | One Shader Graph enemy shader on Renderer Shader User Value (6.3): flash, rim, dissolve; then the GPU Resident Drawer | U3 eng |
| The overlay viewmodel camera costs CPU per camera | [R] forum PSA | URP Render Objects with a camera FOV override (Unity's FPS sample) | U3 eng |
| Mono goes away in 6.8; CoreCLR is strictly IEEE | [R] Unity CoreCLR guide | Ship on 6.3 LTS, or re-validate golden after any upgrade | standing |
| Audio when GG starts | [R] FMOD for Unity 2.03 (free Indie tier, check the EULA) | An AudioBridge: web event name to FMOD EventReference in a ScriptableObject GG edits | GG |
| Steam | [R] Steamworks.NET (SDK 1.64); Deck Verified needs pad glyphs and text 9 px or larger at 800p | Leaderboards, achievements, cloud, Timeline; the gamepad first | U6 |

## 4. Calls for GG (proposals that change an earlier decision or GG's domain)

Each is in the LAB tab off by default (if built), so peers can A/B it on Oct 7.

1. **Health from aggression.**
   - The proposal:
     - A close kill heals 10-20: a punch or gauntlet kill, or any kill within 4 m.
     - Regen slows to about 10-12 HP/s.
     - About 30% of each hit becomes "hard damage" that drains faster at high style ranks (ULTRAKILL: 35%,
       1 s + dmg/20 [R]).
   - Why it's GG's call: the web removed close-kill heals in 3.0 in favour of regen [V: DECISIONS.md 2026-09-29].
   - Evidence: the strongest in the combat research (Doom's "push forward combat" [R], ULTRAKILL [R]).
2. **Freshness with teeth.**
   - The proposal: FRESH x1.5, STALE x0.5, DULL x0 (ULTRAKILL [R]). Today: FRESH x1.1, no DULL [V].
   - Why it's GG's call: it re-tunes style income.
3. **Style ramps with movement.**
   - The proposal: 2-3x while airborne, sliding or wall riding, collapsing fast on the ground (ULTRAKILL [R]).
     Today a flat 1.2 [V].
4. **READY from round 1** (today `readyFrom: 3` [V]), and a squad interval that tightens per round:
   2.2 s x 0.96^(r-1), with a 1.0 s floor.
5. **A run shape:** EXTRACT at about R10-15, an overtime curve from R25, and a suspend at the break.
6. **Debut spacing:** swarmer 3, drone 4, spitter 6, charger 8, lancer 11, warden 13, elites 16. Today: 3-9 [V].
7. **A telegraph colour grammar** (enemy visuals are GG's):
   - amber only for "punch now"
   - a different hue for the swarmer's unparryable pounce
   - a finisher glow on a staggered husk in reach
   - per-category recolouring for colour-blind players
   - Evidence: ULTRAKILL yellow / blue, Dark Ages green [R].
8. **The ARCLINE's price** (5500) or its availability earlier, since it's the signature movement verb.

## 5. The calendar this plan works backward from

From the web repo's docs [V]:
- **Oct 7:** Portfolio Game 2, Demo Day 2 peer playtest (SPEC: a browser game, submitted on itch.io).
- **Oct 28:** Game 3, co-op (near-final build Oct 21; a cut decision Oct 14).
- **Steam Next Fest Feb 2027:** register by Jan 10, 2027; a public store page and a public demo.

The retention research [R] adds:
- the demo ships 2-3 months before a fest
- under 1,000 wishlists "drowned" in June 2026
- a content-capped demo beats a round-capped one (Balatro)

So: a store page by early November, and a demo of Survival to about R10 with the daily and two trials.

## 6. Pitfalls the research agrees on

- **Movement that outpaces the enemies.** Fights become trivial when nothing forces you to move [R].
- **Late-game HP sponges.** CoD past R30. Push density and cadence, not HP; keep HP linear to about R20, because the
  FORGE is calibrated to it [R/V].
- **Runs that are too long, or have no ending.** Returnal, Mewgenics, Halls of Torment [R].
- **Too many systems at once.** BO6's "FAR TOO MUCH". Bring systems in one at a time with the web's tips [R].
- **Stat-grind meta-progression.** Variety only [R].
- **Untelegraphed hitscan and off-screen damage** [R].
- **Aim assist that's too strong** (Apex cut it to 0.4) [R]. SNAP following a husk to your feet is U1 risk 7 [V].
- **Leaderboard cheating.** Validate with replays; keep assist and LAB runs off the board [R].
- **Determinism.** Every new roll comes from the run's seeded streams [V: CLAUDE.md rule 6].

## 7. Sources (the key ones per section; the research reports had about 250 in total)

**Combat:**
- https://80.lv/articles/cyber-demons-the-ai-of-doom
- https://www.gamereactor.eu/doom-eternals-demons-broken-down-by-hugo-martin/
- https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/
- https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
- https://ultrakill.wiki.gg/wiki/Style
- https://ultrakill.wiki.gg/wiki/Parry
- https://ultrakill.wiki.gg/wiki/Hard_Damage
- https://ultrakill.wiki.gg/wiki/Whiplash
- https://ultrakill.wiki.gg/wiki/The_Cyber_Grind
- https://gamingbolt.com/doom-the-dark-ages-interview-medieval-setting-combat-changes-and-more
- https://www.theflagshipeclipse.com/2025/05/15/doom-the-dark-ages-default-settings-suck-heres-how-to-fix-them/
- https://www.gamedeveloper.com/design/designer-interview-getting-i-titanfall-i-s-controls-just-right
- https://tech4gamers.com/how-neon-white-perfected-the-use-of-timers/
- https://gamingbolt.com/turbo-overkill-interview-inspirations-level-design-weapons-and-more
- https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/
- http://www.mathforgameprogrammers.com/gdc2016/GDC2016_Eiserloh_Squirrel_JuicingYourCameras.pdf
- https://www.pcgamesn.com/apex-legends/aim-assist-nerf-season-22

**Survival:**
- https://steamcdn-a.akamaihd.net/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf
- https://jackhanke.github.io/posts/zombies/
- https://callofduty.fandom.com/wiki/Mystery_Box
- https://game8.co/games/Call-of-Duty-Black-Ops-6/archives/481913
- https://game8.co/games/Call-of-Duty-Black-Ops-6/archives/472213
- https://www.pcgamesn.com/killing-floor-2/killing-floor-2-zed-time-trigger
- https://riskofrain2.wiki.gg/wiki/Directors
- https://brotato.wiki.spellsandguns.com/Waves
- https://megabonk.org/guides/mechanics/timer/
- https://bit-tech.net/reviews/gaming/pc/left-4-dead-2-review/3/
- https://steamcommunity.com/app/1938090/discussions/0/4693406768754078535

**Progression and retention:**
- https://media.gdcvault.com/gdc2019/presentations/Benard-Sebastian-DeepCells.pdf
- https://balatrowiki.org/w/Shop
- https://balatrowiki.org/w/Stakes
- https://localthunk.com/blog/balatro-timeline-3aarh
- https://www.gamedeveloper.com/design/the-understated-genius-of-the-i-spelunky-i-daily-challenge
- https://www.vice.com/en/article/neon-whites-developers-being-too-good-at-their-own-game-caused-real-problems/
- https://partner.steamgames.com/doc/features/leaderboards
- https://howtomarketagame.com/2022/11/04/steam-next-fest-october-2022-how-did-games-perform/
- https://howtomarketagame.com/2026/07/13/did-ai-slop-ruin-steam-next-fest-june-2026/
- https://www.gamedeveloper.com/design/housemarque-adds-suspend-cycles-to-i-returnal-i-so-players-can-take-a-breather
- https://www.gdcvault.com/play/1025731/-Slay-the-Spire-Metrics

**Unity:**
- https://docs.unity3d.com/6000.3/Documentation/Manual/renderer-shader-user-value-intro.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/urp/gpu-resident-drawer.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/urp/renderer-features/renderer-feature-render-objects.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/performance-incremental-garbage-collection.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
- https://discussions.unity.com/t/textmesh-pro-text-vs-settext-setchararray-significant-performance-differences/921981
- https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279
- https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/ActionBindings.html
- https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.2/manual/measure-method.html
- https://partner.steamgames.com/doc/steamhardware/compat
- https://partner.steamgames.com/doc/accessibility_features
- https://assetstore.unity.com/packages/tools/audio/fmod-for-unity-2-03-311497
- https://github.com/rlabrecque/Steamworks.NET/releases
