# RESEARCH (wave 22 forge, 2026-10-07): what the web supports for a polish, proof and finish night

Three web-only subagents, about 40 s each. **They read search snippets only and fetched almost no pages**, so every row is
"secondary / unverified against the page" unless marked. Rows the agents could not find are listed as not found; where a number is
ours, the prompt says "our tuning" and gives it a DECIDE22 verdict row.

## A. What the research supports

| # | Rule | Source | Use in NIGHT22 |
| --- | --- | --- | --- |
| R1 | Large element enters in 400 ms, exits in 200 ms; small utility elements enter in 250 ms and exit in 150 ms; exits about 1.5x faster | Material 3 motion tokens (via Flutter's mirror): https://flutter.googlesource.com/mirrors/flutter/+/HEAD/dev/tools/gen_defaults/lib/motion_template.dart ; https://davaux.codeberg.page/ui-docs/motion (secondary) | the stamp's entrance 200-300 ms (M9's target is 90-300 ms; R1's 400 ms is for a large dialog), exit 150-200 ms (our tuning); M9 measures it |
| R2 | Steam Deck: smallest character at least 9 px high at 1280x800, Valve recommends 12 px; the default layout must give access to all content; glyphs must match the input in use; playable at 30 fps at 800p | Valve: https://partner.steamgames.com/doc/steamdeck/compat (primary page; figures via a snippet) | the 16 px at 1080p rule scales to about 12 px at 800p: the pad reel at 1280x800 once, read M10 |
| R3 | Hit-stop scales with hit strength (Smash: 8 to 15 frames for a 15 % move; jabs get little); Nuclear Throne pistol: 6 px camera kick plus 4 px shake that decays fast | https://www.ssbwiki.com/Hitlag ; https://infovore.org/?p=5275 (Vlambeer talk retold) (secondary) | no new hit-stop work; the existing impact budget stays |
| R4 | Xbox accessibility: HUD text 28 px at 1080p with a player scale option; UI contrast 7:1 in high-contrast mode | https://devdocs.xbox.com/gaming/accessibility/xbox-accessibility-guidelines/101 and /102 (page title only; numbers from a summary, re-check) | NOT adopted (our rule is 16 px at 1080p); logged on DECIDE22 D-6 as GG's call |
| R5 | A kill-refilled fuse is one number that doubles as the health bar; the "+N s" gain is its own element beside the clock; low-time pattern: red under 10 s, a pulse, optionally a vignette | Mullet MadJack coverage: https://www.gamepro.de/artikel/mullet-mad-jack-preview,3408091.html ; Steam thread https://steamcommunity.com/app/2111190/discussions/0/4333103687423032749/ ; Fortnite UEFN timer doc https://dev.epicgames.com/documentation/en-us/uefn/making-an-animated-timer-in-unreal-editor-for-fortnite (secondary) | the DEADLINE bar: the number is the bar; a separate "+2.5" pop; our fuse is 18 s so the warning is the last 5 s (our tuning) |
| R6 | ULTRAKILL's Cyber Grind: enemies spawn from a golden beam of light and announce attacks; special enemies are capped by wave | https://ultrakill.wiki.gg/wiki/The_Cyber_Grind (secondary) | THE GRIND's telegraph: rising edges in DANGER are already planned; a beam is optional polish |
| R7 | Titanfall 2 players report that a grapple's "usable again" state is hard to read in the air | https://itch.io/post/12684369 (weak) | the ARCLINE chip needs its own cooldown cue, separate from "no target" |
| R8 | CoD Zombies counts rounds with tally marks (a symbolic count) | https://www.accelerated-ideas.com/news/advanced-warfare-zombies-tally-marks-spotted-on-xbox-controller.aspx (secondary) | idea only (IDEAS) |

## B. Not found (any value is our own tuning, with a verdict row)

Stamp reveal duration and ending; scale / alpha from-values for entrances and whether a pop-in is ever allowed; flash and shake decay
constants for Doom Eternal, ULTRAKILL or Hades; Superhot / Doom Eternal glory-kill / Hi-Fi Rush timer UI; any rule that separates
"on cooldown" from "out of range" for a grapple; every Switch certification number (behind Nintendo's portal; GG has no access
recorded). Suggested follow-up for a future forge: fetch GDC Vault and the Game UI Database pages directly, not through search.

## C. What our own data says (the stronger evidence for this night)

| # | Fact | Source |
| --- | --- | --- |
| O1 | 0 of 7 N21 contract passes at the first close, 1 of 7 after a late fix; every miss was "no reel scenario" | NIGHT21-LOG.md close table and addenda; L-028 |
| O2 | The slidebite reel read 0 bites because its spawn spot sat beside a jump pad at (0, -26.5): a scenario can lie about a feature | NIGHT21-LOG.md second addendum |
| O3 | The first suite run in Unity found 6 reds in code nobody had run (4 stale premises, 2 Editor timeouts) | NIGHT21-LOG.md; L-026, L-027 |
| O4 | 3 of 5 hours lost to one Editor dialog | NIGHT21-LOG.md; L-025 |
| O5 | The replay ring's 0.0000 m check is true by construction; the challenge code's mutators have no reader; the round P was never observed; the bench never ran | NIGHT21-REPORT.md |
| O6 | The ARCLINE look NIGHT21 specified (a red "nothing in range" reticle) breaks CLAUDE.md rule 8 (red-orange is danger only) | NIGHT21.md U3-35.3; persona triage item 3 |
| O7 | The pad default v3 binds Y to SPECIAL + ARCLINE and RB to weapon scrolling (`Core/Gamepad.cs:286`); NIGHT21's triage said RB, which is the v2 line | Core/Gamepad.cs; NIGHT20 pad v3 |

## D. Forge experiment (one change)

**Proof-first rows:** every feature row in NIGHT22 starts with its reel scenario and its contract check being written and run red (the
feature absent or off), and only then is it built and polished. Each row names the owner of its scenario file. Hypothesis: at least
6 of the 9 seeded contracts pass at the close (NIGHT21: 1 of 7). Measure: `passes` count in NIGHT22-FEATURES.json at the close.
