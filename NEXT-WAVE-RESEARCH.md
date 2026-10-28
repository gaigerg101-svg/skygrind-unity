# NEXT-WAVE-RESEARCH.md: what SKYGRIND is missing next to the games people buy and keep playing

GG, 2026-10-03, after NIGHT11 closed: "great lets see what other features or things we are missing that should be apart
of it, resrecah what is at core of best selling and most played video games what polish, features ect make them stand
out and have players coming back, how can we take that and add more to the game that will be good, strat with resrecah
and create next goal prompt, I want way more building than testing (remember this is on unity!)"

This file is the synthesis. The evidence is in three research reports kept in `Research/` (written the same evening by
background agents, about 1,640 lines with their source lists and URLs):
- `Research/2026-10-03-retention.md`: what keeps players coming back (Steam's 2025 charts, GameAnalytics retention
  benchmarks, Steam global-achievement funnels, GDC talks, postmortems)
- `Research/2026-10-03-polish.md`: what polish makes the best action games stand out (hit feedback, the viewmodel,
  readability, lighting, ceremonies, the first five minutes)
- `Research/2026-10-03-unity.md`: what Unity 6 URP offers that the project doesn't use yet, with this project's
  constraints (a deterministic C# core, everything built in code by an agent)

Material: a Drive and Gmail search for "SKYGRIND" found nothing (2026-10-03), so the repos are the material: the earlier
research (`DESIGN-RESEARCH.md`, `ZOMBIES-ULTRAKILL-RESEARCH.md`, `NIGHT11-RESEARCH.md`) covers combat mechanics; this
wave covers what those don't: retention, polish and the Unity way to build them.

**Tags:** [V] verified in the code; [W] / [S] web source (fetched / snippet only; the URLs are in the reports); [U]
inference.

## 0. Bottom line

**What sells and what gets played** (Research/retention, sections 1-2):
- Most playtime goes to games people keep returning to: 14% of 2025's Steam playtime went to 2025 releases, 40% to games
  8+ years old [W]; Left 4 Dead 2 (2009) is in Steam's 2025 Most Played top tier [W]. Winning means joining a player's
  short rotation and staying there.
- The PC bar: D1 retention 15-16% is top quarter, D30 2.5% top tenth; median session 18 min, top quarter over 30 min
  [W]. A 20-30 minute run to exfil fits.
- Every great roguelite shows the same funnel in Steam's own achievement stats: over 90% take a first step when it pays
  within minutes (Vampire Survivors 97.8% at 1 minute), 47-73% get a first win, difficulty rungs keep 12-44% of buyers
  (Balatro Red Stake 43.8%, Brotato Danger 5 36.5%, DRG Hazard 4 34.2%), 1-6% finish everything [W].

**SKYGRIND's gaps, in order of what a player meets first:**
1. **The look has no depth.** The map's shader can't receive SSAO or point lights (`Shaders/Stone.shader` lacks
   `_SCREEN_SPACE_OCCLUSION` and the additional-light loop [V]); the post stack is bloom, ACES, a vignette [V]. The web
   game already tuned far more (`post` grade and chroma pulses, `lightPool`, `motes`, `blobShadows`, `ao`, the enemy rim)
   with every number in `PortExport/config.json` [V].
2. **The feel stops at the trigger.** The gun is bolted to the camera (no bob, sway, breath or tilts: `Viewmodel.cs`
   "not ported" [V]); kills read as cube bursts (no kill confirm, dissolve or flinch [V]); the movement FX (land puffs,
   slam rings, wall sparks, dash tint) aren't ported [V]. A study of top-selling action games' Steam reviews found hit
   stop, sound coherence and camera control separate the best-reviewed from the worst [W].
3. **No ceremonies, no first impression.** RESULTS is eight equal lines [V capture]; the round change is a banner; the
   title is static; the font is TMP's default [V]. CoD's round change, ULTRAKILL's rank tally, Balatro's counting and
   Vampire Survivors' chest make the moment the reward [S].
4. **Nothing carries across runs but a best round.** No contracts, unlocks, daily, difficulty ladder or mastery [V]. In
   Quantic Foundry's terms SKYGRIND is strong on Destruction / Challenge / Power and weak on Completion, Competition and
   Discovery [U]. The web game already designed most of the fix (`challenges.js` 24 badges, `unlocks.js`, `daily.js`,
   `mutators.js`, `finishes.js`, `seeds.js`, tips) and none of it is ported [V].
5. **Content and social for Early Access:** 6 of 18 guns, 3 of 7 enemies, 0 of 5 bosses, one map, no co-op, no gamepad,
   no Steam layer [V]. Content droughts kill games (MH Wilds -98%, Destiny 2 -80-90%) and updates bring them back
   (Helldivers 2 doubled in a day) [S]; every 2025 Top Sellers top-tier game has multiplayer [W].

**The finding that shapes the next night:** gaps 1-4 are mostly *already designed and tuned in the web game*, and URP
has the built-ins (SSAO, Forward+ point lights, Volume overrides, instanced drawing, hand-written HLSL), so they are
building work with a low testing cost: render-only views (no golden vectors needed), plus small Core state for the meta
(saved in a JSON profile) with unit tests. That matches GG's "way more building than testing".

## 1. Retention: the ranked list (Research/retention section 5)

| # | Feature | Imp / Eff | Builds on |
|---|---|---|---|
| 1 | **CONTRACTS board (30-40) with variety unlocks, shown as locks** (first unlock in under 5 minutes, then one every 1-2 runs for 2 hours; mirrored as Steam achievements later) | H / M | `challenges.js` (24), `unlocks.js`, the event bus |
| 2 | **The PAYOFF strip on DOWN / RESULTS** (the grade, personal-best deltas, the 3 nearest contract bars, mastery XP, NEXT UNLOCK): Hades' "every run counts" | H / S-M | RESULTS, records, runMoments |
| 3 | **HEAT ladder** (a win unlocks the next rung; one readable rule per rung; a score multiplier; a badge on the grade); opt-in only (Destiny 2 fell 80%+ after raising base difficulty) | H / M | `mutators.js`, the LAB, exfil |
| 4 | **DAILY seed** (one counted try, a twist, a streak), then a WEEKLY; boards friends-first, after a first clear | H / S-M local | `daily.js`, `seeds.js`, the bit-exact sim |
| 5 | **First-run onboarding**: one system per round, the first unlock in round 1-2, no punishment while learning (Hodent: 3 items at once; Warframe kept 80% past an hour) | H / M | tips, banners, light pools |
| 6 | **MASTERY track per gun**: CoD's camo ladder rebuilt to teach ULTRAKILL's verbs (FRESH, air, swap, parry, S-rank kills), finishes as rewards, counts in the tens to hundreds | H / M | arsenal and style events, `finishes.js` |
| 7 | **Grade chase**: a best grade per rung, a "P" exfil (S + SS + no down), a history strip | M / S | the grade S..D |
| 8 | **Secrets ladder**: the PROTOCOL (guided), the hidden relay + movement collectibles, one Prime secret | H / M-L | protocol.js, unlock flags |
| 9 | **Clips and sharing**: Steam Timeline events, a build card, seed code, RETRY SAME SEED, COPY SCORE LINE | M / S | seeds, the score line |
| 10 | **CODEX and named combos**, banish / skip in drafts | M / S-M | MODS, forge, arsenal |
| 11 | **A no-FOMO EA cadence**: a free major update every 8-12 weeks (map 2 first), contracts that never expire | H / L | the contracts board |
| 12 | **Co-op (2-4)**, with starting RIGS for identity first | H / L | Players (NIGHT9), the deterministic sim |

## 2. Polish: the ranked list (Research/polish section 3)

| # | Build (all render-only) | Imp / Eff |
|---|---|---|
| 1 | **The viewmodel's life**: bob, sway, breath, slide / wall tilts, trauma's share, the KESTREL's cylinder, the THUNDERCLAP's pump, afterimages (all in config) | H / S-M |
| 2 | **Readable hits**: kill confirm (`killConfirm.js`), a hitmarker grammar, the victim's vibration and flinch | H / S |
| 3 | Blocky gibs that stay: **GG's call** (enemy looks, rule 7); the web's dissolve instead tonight | H / M |
| 4 | **The enemy rim with a distance boost** (Valorant's rule: enemies always stand out) + hit flash + dissolve, one shader | H / M |
| 5 | **The premium dusk**: the split-tone grade, gradient fog, SSAO | H / S-M |
| 6 | **RESULTS as a staged tally** (outcome, round, three counting cards, the grade slam, NEW BEST stamp; any key skips) | H / M |
| 7 | **The round ceremony** (CoD's numerals burning in; the round number flashing at the end), then the box reveal | H / S-M |
| 8 | The parry's cyan glint (one colour, one meaning) | H / S |
| 9 | **Muzzle light, impact flare, tracers per pellet, shells** | M-H / S |
| 10 | **The movement FX pack**: land puffs, slam rings, wall sparks, the dash tint, streaks, chroma pulses | M-H / S |
| 11 | **Light pools and guide pulses** (light what matters: doubles as onboarding) | M / S |
| 12 | **Ambient motes that react** (the web's 900) | M / S-M |
| 13 | The settings reviewers check (rebinding, shake, tilt, enemy colour, reduce motion) | H (reviews) / M |
| 14 | **A living title and the theme fonts** | M / M |
| 15 | A points feed and optional damage numbers (off by default) | M / S-M |

Pitfalls: clutter (numbers over targets), motion sickness (every camera effect gets a slider and one "reduce motion"
preset), one meaning per colour (cyan usable, red-orange danger, magenta RIVAL), cosmetic randomness on its own seeded
stream (never the run's), a bench per item (the u3-n11 worst 1% is 5.55 ms against a 6.9 ms 144 fps bar).

## 3. The Unity way (Research/unity sections 0, 3, 4)

- **Fix `Stone.shader` first**: add `_SCREEN_SPACE_OCCLUSION` and the URP 17.3 light loop (`_CLUSTER_LIGHT_LOOP`; most
  tutorials still say `_FORWARD_PLUS`). It unlocks SSAO on the map and every point light (muzzle flash, light pools).
- **Port `post.js` onto the URP Volume** (split tone, chroma pulses, the low-HP volume, grain); the hand-typed 0.22
  vignette in `Look.cs` goes to config (rule 5).
- **Keep hand-written HLSL and objects built in code**; VFX Graph and Shader Graph are editor-window authoring, a poor fit
  for an agent on a CLI. New render passes go in by script (`beginCameraRendering` + `EnqueuePass`), Render Graph only.
- **Particles:** `Graphics.RenderMeshInstanced` with an instanced unlit shader instead of one GameObject per cube; every
  view effect on the loop's `worldDt` (hitstop and pause apply).
- **One `SKYGRIND/Enemy` shader**: rim, hit flash, dissolve, an x-ray pass (`ZTest Greater`), after the web's
  `rimGlow.js`, `dissolve.js`, `xray.js`.
- **Skip:** motion blur, depth of field, TAA / STP, Cinemachine (it would fight the golden-checked trauma shake),
  Animation Rigging (no Animator on the blocky enemies), APV / baked lighting (the map is built at load).
- **Fonts:** the theme fonts (Barlow Semi Condensed, Chakra Petch; OFL) as TMP assets made by an editor script from TTFs
  (Unity can't import the web's woff2).
- **Platform, later:** settings and the profile as JSON files (Steam Auto-Cloud syncs files, not the registry where
  PlayerPrefs lives on Windows), the gamepad with glyphs (Steam Deck Verified), a Steam layer (Unity announced official
  Steam support at GDC 2026: check before choosing Steamworks.NET or Facepunch), an IL2CPP trial gated on
  `-skygrindGolden`.

## 4. The plan: the next nights (D-19's research order, amended by this wave)

| Night | Theme | Why now |
|---|---|---|
| **N12** | **The look, the feel and the meta core**: light and lens (Stone fix, post, light pools, motes, blob shadows), the enemy shader, readable hits and the movement FX, the viewmodel's life, ceremonies (round, RESULTS tally, fonts, a living title), the JSON profile, CONTRACTS + unlocks, the PAYOFF strip, the HEAT ladder (3 rungs) and the DAILY, MASTERY tracks with finishes, first-run onboarding | gaps 1-4; all designed or tuned by the web; render-only or small Core state; building-heavy |
| N13 | The POWER guns, the hotbar and radial, THE FORGE tiers, save-and-quit (the former NIGHT12; `NIGHT13-RESEARCH.md`) | content variety for the box and the wall |
| N14 | The enemy roster (charger, drone, lancer, warden, affixes) + the round budget; HEAT rungs 4-6 by composition | content; rungs need kinds to escalate by composition, not HP |
| N15 | A boss + the secrets ladder's tiers 2-3 + gibs if GG says yes | the run's spine and discovery |
| N16 | Ship-shape: the gamepad and glyphs, rebinding and the review-checked settings, quality presets and a Deck profile, a Steam layer (achievements = contracts, Timeline clips, leaderboards for the DAILY) | the demo's platform bar |
| EA | Co-op (2-4), map 2, a free update every 8-12 weeks | the chart's social pillar; no-FOMO cadence |

**Shipped in N12 (`u3-n12`, 2026-10-04; NIGHT12-REPORT.md):** every N12 row above: light and lens (SSAO and point
lights on the map, post.js on the Volume, light pools, motes, blob shadows), the enemy shader (rim, flash, dissolve,
x-ray), readable hits and the movement / weapon FX, the viewmodel's life, the ceremonies (round burn-in, RESULTS tally,
HUD motion, a living title), the theme fonts, the JSON profile, CONTRACTS (39; the first unlock in round 2), the PAYOFF
strip, HEAT rungs 1-3, the DAILY (local), MASTERY with finishes, tips and the first-run script; plus damage numbers (off),
RETRY SAME SEED and COPY SCORE LINE (retention #9). Short: the Player bench's worst 1% is a 6.84 ms median of three on Balanced
(4.64-7.00 ms; at the 6.9 ms bar, no headroom) and a 7.19 ms median of three on the Silent plan (over; polish pitfall "a bench per item": the costliest
effect goes on a quality tier next; D-34);
LOW GRAVITY waits for a Mover gravity factor; 11 web badges wait for bosses, the ARCLINE and affixes.

## 5. Decisions for GG (proposals only; nothing here changes code without him)

| ID | Question | Recommendation | Default if no answer |
|---|---|---|---|
| D-29 | Blocky gibs that stay (DOOM / ULTRAKILL permanence) on enemy deaths: an enemy-look call (rule 7) | yes, in N15 with the boss | the web's dissolve only |
| D-30 | New packages: LitMotion (UI tweens), a Steam wrapper, a volumetric fog package | none in N12 (code tweens, URP fog); decide the Steam wrapper at N16 | none |
| D-31 | HEAT rungs beyond the web's mutators (composition rules) touch balance | the web's mutators as rungs 1-3 in N12; new rungs with the roster (N14) | the web's mutators only |
| D-32 | Contract and codex texts, secret content | mechanical texts in N12; lore only with GG | mechanical only |
| D-33 | A local DAILY in the demo before Steam boards exist | yes (the streak and the seed code still pay) | yes |
| D-13 | The theme fonts (already pending) | yes, in N12 | yes |

## Sources

The three reports in `Research/` carry every source with its URL and a fetched / snippet tag (retention: Steam's Best of
2025 lists, Steam Replay, GameAnalytics 2026 benchmarks, Steam global achievements for Vampire Survivors / Hades /
Balatro / Brotato / RoR2 / DRG / Dead Cells, Hodent's GDC 2016 talk, Game Developer's Spelunky daily piece; polish: the
impact-feel study arXiv 2208.06155, the Dead Cells GDC 2019 post-mortem, Nijman, Jonasson and Purho, Eiserloh, Sakurai,
id's GDC talk, the Game Accessibility Guidelines, Steam Deck Verified criteria; Unity: docs.unity3d.com 6000.x, URP 17
Render Graph and renderer-feature docs, Steamworks docs, Unity's GDC 2026 Steam announcement).
