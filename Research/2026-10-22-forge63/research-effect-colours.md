# One colour language for effects: how shipped action games do it

Web research, 2026-10-22, about 27 tool calls. Marks: **Verified (fetched)** = I read the page and the claim is on it.
**Verified (search)** = the claim is in the search tool's summary of that page; I did not open the page. **Inferred** = my
synthesis or proposal, not stated by a source. **Not checked** = the source exists but I could not read its content.

## Summary: the rules to adopt, with numbers

1. One hue = one meaning, everywhere, including on bosses. ULTRAKILL uses a pre-attack flash: yellow = parryable, blue = not parryable (Verified, fetched: https://ultrakill.wiki.gg/wiki/Parry). Players get confused where projectile colour and flash colour disagree (Verified, search: https://steamcommunity.com/app/1229490/discussions/2/3734079155590153286).
2. Saturation ladder: the environment sits at a "low, muted saturation baseline", UI uses "localized, controlled saturation", and VFX get "a wider, higher saturation ceiling" (Riot's VFX style guide as summarised by https://www.vfxapprentice.com/blog/10-league-of-legends-vfx-design-tips, Verified, fetched).
3. Value range: no pure 0 % black or 100 % white in an effect's body; only the instant primary flash gets "the highest value spikes" (same URL, Verified, fetched).
4. Timing: an effect hits peak value and saturation "within the first quarter of the effect's lifespan", then fades smoothly. "If your FX feel long, they're waaaaay too long" (same URL, Verified, fetched).
5. Attention follows importance. Riot designers rate every spell 1-10 in a "Visual Hierarchy document", separately for the caster, enemies and allies (Verified, fetched: https://realtimevfx.com/t/anatomy-of-a-league-of-legends-missile-part-2-what-are-we-communicating-with-our-vfx/13062). The factors are damage, hard CC, dodgeability and impact on play (Verified, fetched: https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/).
6. Primary vs secondary: primary = high value, clear silhouette, high opacity, sharp focal point; secondary = lower value, lower opacity, blurred shapes. Complementary accents keep a "strict dominant-to-submissive ratio" (Verified, fetched: vfxapprentice URL above).
7. Same function, same shape and colour. Roots are always a ring plus a rising cylinder, and "heals should be green" (Verified, fetched: realtimevfx URL above). Every variant must be "at least as clear as the base" ("clarity parity") (Verified, fetched: Clarity in League URL).
8. The danger telegraph owns the strongest edge contrast, with no heavy bloom, and lasts long enough to react to. Impact bursts are bright but short and never share the telegraph's shape (Verified, search: https://gamineai.com/blog/top-down-combat-vfx-readability-2026-color-timing-system-busy-screens; a low-authority 2026 blog).
9. Enforcement is one table that every effect reads. A global parameter asset means "if you change the value, everything that uses them changes automatically" (Verified, search: https://www.unrealengine.com/en-US/blog/material-parameter-collections). The Unity equivalent would be one palette asset plus global shader colours (Inferred).
10. Audit on real frames: Riot now checks every skin on every map (Verified, fetched: Clarity in League URL). A grayscale pass tests whether value alone separates the roles (Verified, search: https://www.wayline.io/learn/color-palettes/1). No source I found publishes numeric screen-coverage or bloom budgets, or per-role hue-histogram checks. If SKYGRIND adopts those, it sets its own numbers (Inferred; see section 3).

---

## 1. Role-based colour coding in shipped games

### ULTRAKILL
- Pre-attack warning flash: "Yellow flashes indicate attacks that can be parried"; "Blue flashes indicate attacks that can't be parried". Parryable ranged attacks show an "8-point yellow parry spark" and melee a 4-point spark. A successful parry freezes the game briefly and flashes the screen white, and the white flash can be turned off ("Parry Flashes"). Verified (fetched): https://ultrakill.wiki.gg/wiki/Parry
- Exception: "Not all attacks (such as the Malicious Face's Hell Orb barrages) have these indicators." Verified (fetched): https://ultrakill.wiki.gg/wiki/Parry
- Where the language breaks down: players report that the tutorial teaches "yellow parry, blue don't", and later enemies fire blue projectiles that can be parried. One enemy uses purple for its parry glow. Community answer: read the wind-up flash, not the projectile colour. Verified (search, community thread): https://steamcommunity.com/app/1229490/discussions/2/3734079155590153286
- Lesson: a role colour applied to two different things (the parry flash and a projectile's body) costs readability (Inferred).

### DOOM Eternal
- Glory-kill state: a staggered enemy glows blue, and the glow turns orange once the player is close enough to glory kill. Verified (search): https://www.trueachievements.com/game/Doom-Eternal/walkthrough/2
- Pickups by resource (search summary of the same walkthrough plus doomwiki; doomwiki returned 403 to me): health drops as blue orbs or potions; armour as green shards / helmets / chest pieces; shells red; bullets green and gold; cells purple discs; rockets orange; Argent (BFG) cells green orbs. Verified (search): https://www.trueachievements.com/game/Doom-Eternal/walkthrough/2 ; https://doomwiki.org/wiki/Supplies_(Doom_Eternal) (Not checked, 403).
- Inferred from the list above: DOOM Eternal sorts pickups by resource (every resource has its own hue and silhouette), not by a single "pickup" hue. The hues are not exclusive (green is both armour and Argent cells), so shape and size carry the rest.
- Weak points: destroying the Arachnotron's turret removes its long-range attack ("makes life easier", Hugo Martin). Damage is shown on the body ("Dämonenzerstörung"). Verified (search): https://www.gamestar.de/artikel/doom-eternal-daemonenzerstoerung-soll-kaempfe-taktischer-machen,3333370.html ; https://slayawaywithus.com/post/doom-eternal-review-revising-the-doom-slayer-to-stop-hell-s-invasion-of-earth
- A QuakeCon 2018 report says the team used colour, saturation and "cartoonish" gore so feedback reads mid-combat. Verified (search) for the report's existence: https://www.tecmundo.com.br/voxel/especiais-178525-direto-da-quakecon-doom-eternal-engrandece-o-melhor-de-doom-2016.htm ; its text is Not checked (the redirect target refused fetch).
- A specific colour code for DOOM Eternal weak points: Not checked (no source found).

### Overwatch
- Team effects in normal play stay red (enemy) and blue (ally). Projectiles and barriers are not recoloured to match team colours, so enemy damage effects are not confused with other effects. Recolouring appears only on tournament spectator clients. Verified (search, Blizzard forum thread): https://us.forums.blizzard.com/en/overwatch/t/overwatch-league-visual-effects-in-game/593284
- Gap reported by players: the colour-blind UI options did not extend to ability effects ("an enemy Tracer's blink effects are subtle and red"). Verified (search): https://eu.forums.blizzard.com/en/overwatch/t/colour-blind-accessibility-issues/30890
- A later update added separate friendly and enemy UI colours (nine options) and customisable hero outlines. Verified (search): https://www.slashgear.com/overwatch-gets-updated-colorblind-feature-with-nine-color-options-22546973
- GDC 2017 "The Art of Overwatch Evolving" (Petras, Tsang): "a vibrant world". Its saturation rules are Not checked (Vault behind login): https://gdcvault.com/play/1024268/The-Art-of-Overwatch-Evolving

### Valorant
- The enemy highlight defaults to red, with colour-blind options purple (tritanopia), yellow (deuteranopia) and yellow (protanopia). Verified (search, third-party guides): https://esports.gg/guides/valorant/how-to-change-enemy-color-in-valorant/ ; https://www.thespike.gg/valorant/tips/how-to-change-enemy-color-in-valorant
- "Red because enemies are the bad guys" is one outlet's reading, not a Riot statement. Verified (search) as that outlet's claim: https://afkgaming.com/esports/guide/how-to-change-enemy-color-in-valorant
- Riot's own Valorant VFX readability rules: Not checked (none found).

### League of Legends (Riot)
- "Heals should be green." Damage spells should be "bigger, brighter, and more threatening", with sharper shapes. Verified (fetched): https://realtimevfx.com/t/anatomy-of-a-league-of-legends-missile-part-2-what-are-we-communicating-with-our-vfx/13062
- Colour by function: healing leans green, frost blue, gunpowder orange-red. Verified (fetched, Riot guide summary): https://www.vfxapprentice.com/blog/10-league-of-legends-vfx-design-tips
- Riot's guide itself is at https://nexus.leagueoflegends.com/wp-content/uploads/2017/10/VFX_Styleguide_final_public_hidpjqwx7lqyx0pjj3ss.pdf. Not checked: it now redirects to the League home page.

### Destiny 2
- Damage numbers: white = normal, yellow = critical (precision). Verified (search): https://www.trueachievements.com/game/Destiny-2/walkthrough/2
- Enemy rank is shown by health-bar colour (red bar = normal, yellow bar = elite). Elites take far less crit damage (about 3.0x on a red bar vs about 1.5x on a yellow bar, by community testing). Verified (search): https://mp1st.com/news/destiny-2-best-pvp-weapons-reveal-interesting-weapon-statistics
- Elemental shields take more damage from a matching damage type, and the colour match is the cue. Verified (search): https://www.trueachievements.com/game/Destiny-2/walkthrough/2

### Hades
- I found no source on telegraph colours. Supergiant says it is "first and foremost a game design-led team" (Verified, search: https://www.invenglobal.com/datalab/articles/14783/supergiant-games-how-indie-developers-made-goty-hades). The 80.lv effects piece had no readable content (Not checked: https://80.lv/articles/a-behind-the-scenes-look-at-the-effects-in-hades/).

### What each role gets: synthesis
Inferred from all of the above:

| Role | Hue | Saturation | Value |
|---|---|---|---|
| Danger / enemy attack | one warm hue, reserved for it alone (Overwatch red, Valorant red, ULTRAKILL's flash pair) | high | highest at the leading edge; flash at the start of the wind-up |
| Player-owned | one cool hue (Overwatch blue) | mid-high | below danger at the moment of a telegraph |
| Reward / pickup | split by resource, each with its own hue and silhouette (DOOM Eternal) | high | pulses or glows so it reads at a distance |
| Hit confirm / crit | a separate accent from damage (Destiny yellow crits, ULTRAKILL white parry flash) | n/a | instant spike, under 1/4 of the effect's life |

Each rule keeps the roles apart only if no other system reuses the hue. ULTRAKILL's blue projectiles that can be parried show what happens when one does.

## 2. Value and saturation hierarchy

- Three tiers of saturation: environment muted, UI localised, VFX highest ceiling. Verified (fetched): https://www.vfxapprentice.com/blog/10-league-of-legends-vfx-design-tips
- Primary vs secondary elements: a "strict structural hierarchy". Primary = high colour value, clear silhouette, high opacity, sharp focal point. Secondary = lower value, lower opacity, simpler blurred shapes, subtle motion. Verified (fetched): same URL.
- No absolute extremes (0 % black, 100 % white) in the body. Mid-range palette for the body; the highest value only on the instant primary flash. Local emission glow widens the value range without changing textures. Verified (fetched): same URL.
- The leading edge of a moving effect carries "the highest brightness value and the sharpest contrast boundaries". Verified (fetched): same URL.
- Timing curve: brief wind-up, then peak value and saturation within the first quarter of the lifespan, then a smooth fade. Lingering frames hurt clarity and overdraw. Verified (fetched): same URL.
- Hierarchy of attention: "always preserve hierarchy". Flashiness has a cost; only spells with large gameplay effects may be noisier "to a degree". Riot will "never turn everything up to 11". Verified (fetched): https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/
- Hitboxes must match the effect; a large ground circle that overstates the danger is a clarity bug. Verified (fetched): same URL.
- Reserve the highest local contrast for gameplay-critical elements (telegraphs) and keep decoration one step lower in value. Verified (search, low-authority blogs): https://gamineai.com/blog/stylized-foliage-readability-top-down-games-color-script-rules-hold-up-motion-2026
- Blizzard (Chamberlin) and Riot (Keyser), GDC 2017 "Applying Artistic Principles to VFX Design": the session exists. Its content is Not checked: https://gamedeveloper.com/art/video-applying-artistic-principles-to-vfx-design-for-games
- Rocksteady, GDC "Attention Overload: Creating Readable VFX for Suicide Squad: Kill the Justice League": VFX steer attention "without overwhelming" the player; VFX are put into design early. Verified (search) for the session description: https://schedule.gdconf.com/session/attention-overload-creating-readable-vfx-for-suicide-squad-kill-the-justice-league/899824 ; the slides are Not checked.
- Numeric bloom or screen-coverage budgets from a shipped game: Not checked. None of these sources gives one.

## 3. How teams audit it

- Visual Hierarchy document: designers rate every spell 1-10 for importance, separately for caster, enemies and allies. When the ratings differ a lot, the effect is drawn differently per viewer (Morgana's R ring is shown only to her and the affected targets). Verified (fetched): https://realtimevfx.com/t/anatomy-of-a-league-of-legends-missile-part-2-what-are-we-communicating-with-our-vfx/13062
- Three questions every effect must answer: identity ("What is this ability?"), function ("What does it do?"), impact ("How much impact will it have?"). Effects with the same function share visual language. Verified (fetched): same URL.
- Clarity parity: every variant must be "at least as clear as the base". The primary feature "should never be removed or significantly altered". Every skin is checked on every map, and problems are fixed during development. Verified (fetched): https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/
- A written style guide is the reference sheet: Riot's public PDF covers gameplay, value, colour / saturation, shapes and timing. Verified (search): https://www.vfxapprentice.com/blog/creating-vfx-style-guides-for-games ; the PDF itself is Not checked (it redirects).
- Grayscale test: desaturate the frame; if the shapes still read, the values work. Verified (search): https://www.wayline.io/learn/color-palettes/1
- One global table: Unreal's Material Parameter Collection stores scalar and vector parameters that many materials read, and changing a value changes everything that uses it. Verified (search): https://www.unrealengine.com/en-US/blog/material-parameter-collections ; https://docs.unrealengine.com/5.1/en-US/PythonAPI/class/MaterialParameterCollection.html
- Automated frame checks (hue histograms per role, screen-coverage measures, bloom budgets): no shipped-studio source found. Not checked.
- Proposal for SKYGRIND (Inferred, not from a source):
  - one palette asset that holds the role colours CLAUDE.md rule 8 already names (cyan player, red-orange enemy / danger, magenta RIVAL, pale violet-white boundary), plus new role rows for pickup and hit-confirm. Every effect reads it, with no colour typed in an effect.
  - a Player-frame audit (L-093: measure pixels on the frame): classify each pixel above a saturation / value floor into a role's hue band and print per-role coverage, plus any pixel that falls outside every band (an "unaligned" colour).
  - a grayscale pass on the same frames.
  - the numbers (band widths, coverage caps) set by measuring current frames first, since no source supplies them.

## 4. Bosses: attacks and weak points

- ULTRAKILL decides parryability per hit: some bosses flash blue before one swing and yellow before the next. The Power's greatsword combo fires two parryable yellow projectiles joined by an unparryable beam (Violent and above). The Ferryman's combos go unparryable, then parryable. Verified (search): https://ultrakill.wiki.gg/wiki/Power ; https://ultrakill.fandom.com/wiki/Ferryman ; https://steamcommunity.com/app/1229490/discussions/2/3734079155590153286
- So the boss keeps the same two-colour language as the grunts, and the flash comes before every damage instance; the boss gets no new colours (Inferred from the above).
- DOOM Eternal weak points are functional: destroying the Arachnotron turret removes its long-range attack, and damage shows on the body. The weak point's colour code is Not checked. Verified (search): https://www.gamestar.de/artikel/doom-eternal-daemonenzerstoerung-soll-kaempfe-taktischer-machen,3333370.html
- Danger zones in red: Roboquest's explosion zones warn of the danger area. Verified (search): https://www.patreon.com/GameDesignerNotes/posts/roboquest-combat-140820288
- Telegraph rules: the strongest edge contrast, no heavy bloom, a reaction window; impact bursts short and never the telegraph's shape. Verified (search, low authority): https://gamineai.com/blog/top-down-combat-vfx-readability-2026-color-timing-system-busy-screens
- The effect must match the hitbox, or the telegraph misleads. Verified (fetched): https://www.leagueoflegends.com/en-gb/news/dev/clarity-in-league/
- Riot's "dodgeability" factor: an attack the player can dodge deserves more attention than one they can't. Verified (fetched): same URL.
- Arena vs boss (Inferred): the arena stays in the muted environment tier, so the boss's telegraph hue must not appear in the arena's lights or props. Under CLAUDE.md rule 8, red-orange belongs to enemies and danger only, so warm arena lighting is a risk to check on frames.

## Not found in this pass
- The full text of Riot's VFX style guide PDF, and of Valve's TF2 illustrative-rendering paper (the PDF fetched but its text could not be extracted here).
- Hades telegraph colours.
- Valorant VFX rules from Riot.
- Any published screen-coverage or bloom budget.
- Any studio's automated hue-histogram check.
