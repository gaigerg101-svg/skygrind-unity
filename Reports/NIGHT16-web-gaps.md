# NIGHT16: what the web still does that the port doesn't (two read-only sweeps, 2026-10-05 17:35-17:48)

Builder U swept the web's UI (`src/ui/*.js`, `main.js` where it drives the UI): 43 rows. The HUD rows are U's own; the
menu rows (tagged A) came from a read-only research agent (menus.js, tooltips.js, tooltipText.js, loading.js, theme.js,
settings.js), two of them spot-checked. Builder V swept feel outside the HUD (viewmodel.js, player.js, fx/*, effects.js,
impactFx.js, killConfirm.js, the weapons' visuals): 32 rows, the biggest claims re-checked in the port's code. Both
against the port at `a427318`; nothing was run. Port lines are as of that commit.

Rows taken the same night are marked with their FEEL-DESIGN.md section 5 entry (#41-#61). Impact 1-5; cost S / M / L.

## The UI (builder U)

| # | Web file:line | What the player sees | Port status | Impact | Cost | Notes |
|---|---|---|---|---|---|---|
| 1 A | menus.js:198-202, 221 | Mouse hover: the label turns cyan, a cyan left edge appears, the button slides 8 px over 120 ms; cards rise 2 px | Partial: a ColorTint on a near-black panel only (Ui.cs:858-862, UiSettings.cs:291-295, UiMods.cs:28-32); no pointer-enter handler anywhere; the focus frame (UiNav.cs:431) follows keys and pad only | 4 | S | A select-on-hover component reusing the focus frame |
| 2 U | hud.js:185, :320-321, :607-612; styleMeter.js:12, :109-110 | The cyan chip "×1.2 AIR / SLIDE / WALL" by the rank while the state multiplier applies | Absent (StyleCore.cs:193 has the multiplier) | 3 | S | #39 left: its place vs the award feed's box |
| 3 U | hud.js:183, :288, :476, :676-677 | The rank-up's screen-edge glow in the rank's colour (style.glowTime 0.3, glowOpacity 0.55, x FLASH INTENSITY) | Absent | 3 | S | #29 left: rule 8 (S / SS colours), HANDOFF D-70 |
| 4 A | tooltips.js:111-122; main.js:1405 | A hold key lists YOUR PERKS (icon, name, effect) and YOUR MODS mid-screen | Absent; the key is read (CombatReader.cs:145-146, PadReader.cs:323-326) | 3 | S | The input half is done |
| 5 A | menus.js:455, 257 | SETTINGS: every row's description under it | Partial: one line, the key / pad-selected row's (UiSettings.cs:161); hover doesn't select | 3 | S | |
| 6 A | menus.js:283, 1146-1154; main.js:1276-1292, 1352-1354, 1413 | A 0.15 s fade on every run start, restart and quit | Partial | 3 | S | #54 taken (the "in" half) |
| 7 A | tooltips.js:93-109; tooltipText.js:36-48, 144-163; main.js:2108-2142 | Crosshair cards at a perk machine (8 m), a wall buy, the radial's gun: effect with numbers, price, status, HOLD E | Absent: the prompt is NAME · PRICE (Ui.cs:1316-1345) | 4 | M | You can't learn a perk before paying; 16 px floor |
| 8 U | hud.js:487, :450-456 | Feed: zero-point tricks dimmed; "+ NAME points" in the rank's colour | Partial | 2 | S | #46 taken |
| 9 U | hud.js:179-180, :602 | The rank bar's pulse, faster at higher ranks | Absent (StyleMath.RankPulseRate ported) | 2 | S | #47 taken |
| 10 U | hud.js:139, :848-855 | Owned mods as chips "VAM×2" in the perk row | Absent | 2 | S | #60 taken |
| 11 A | menus.js:566-570, 1058 | RESULTS: "ROUND N BEATS YOUR BEST (k MORE) · NEXT UNLOCK: X" | Partial (DOWN has the target) | 2 | S | #61 left: RESULTS is UiResults' tally; k has no source there |
| 12 A | menus.js:631-634 | A CREDITS screen | Absent | 2 | S | The font licences ship in Assets/Fonts, shown nowhere |
| 13 A | menus.js:541-547, 636-639, 784-785 | A first-run HOW TO PLAY card from the live keys | Absent (the port teaches in-run: UiOnboarding.cs) | 2 | S | May be a deliberate change |
| 14 A | menus.js:580-598 | CHALLENGES: every mod's tier, LOCKED, pick rates, "k / n UNLOCKED" | Absent; the counts exist (Core/Mods.cs:86-96, 190) | 2 | S | |
| 15 A | menus.js:1001-1003 | PAUSED's YOUR KEYS lists pad buttons on a pad | Partial | 2 | S | #56: ready on u-small2-n16, not integrated |
| 16 A | menus.js:786-790 | R restarts from PAUSED | Absent | 2 | S | #48 taken |
| 17 A | index.html:113-116; loading.js:51-56 | A themed loading screen | Absent (Unity's splash, then TITLE) | 2 | S | A Personal licence may force the splash |
| 18 A | menus.js:201, 205 | The focused button's label turns cyan | Partial | 2 | S | #55: ready on u-small2-n16 |
| 19 U | hud.js:818-826; viewmodel.js:930-932 | HUD STYLE ATTACHED (the web's default): the points and weapon block sway with the gun | Absent: no row, always CLASSIC | 3 | M | #40 left; the plan below |
| 20 U | powerUpHud.js:1-20 | Power-up chips on the right edge with a draining bar and a slide-in | Partial: one centred line (UiPowerUps.cs:21) | 3 | M | Rule 8 for their colours |
| 21 A | menus.js:420-426, 998-1000; tooltips.js:70-89 | PAUSED shows owned perk, mod and weapon chips with hover cards | Absent | 3 | M | Needs row 7's card text |
| 22 A | menus.js:446-448 | SETTINGS range rows: a draggable slider plus -/+ | Partial: -/+ only (sensitivity: 56 clicks end to end) | 3 | M | |
| 23 A | config.js:2292-2308 | Rows HUD STYLE, ZOOM toggle, SHAPE CUES, STYLE AURA, SHOW FPS | Absent: loaded (SettingsView.cs:71-98), not in UiSettings' Shown, not read | 3 | M | ZOOM toggle and SHAPE CUES are accessibility |
| 24 A | menus.js:812-863, 1034-1036 | RESULTS: a local top-10 with initials | Absent (no text field in the port) | 3 | M | |
| 25 A | menus.js:1067-1084, 571-578 | RESULTS breakdown: kills by weapon and type, BOUGHT, chips, MELEE KILLS | Absent (RunStats lacks most; meleeKills counted, not shown) | 3 | M | |
| 26 U | hud.js:56, :494-495, :735 | The zone name under the top line | Partial (the centre banner) | 1 | S | |
| 27 A | menus.js:561-565, 1057 | RESULTS' DAILY line | Partial (a toast) | 1 | S | |
| 28 A | menus.js:1037, 1088 | "RETRY (R)" | Partial | 1 | S | #49 taken |
| 29 A | menus.js:770-775 | Tab / Shift+Tab move the focus | Absent | 1 | S | |
| 30 A | menus.js:203, 417, 670 | Disabled buttons at 35 %, skipped by the focus | Partial | 1 | S | #57: ready on u-small2-n16 |
| 31 A | menus.js:413-414 | MODS' instruction line and "OWNED xN OF max" footers | Partial | 1 | S | |
| 32 A | menus.js:302-303, 736-737 | CONTROLS' KEYS RESET / PAD RESET notes; the waiting cell in gold | Partial | 1 | S | |
| 33 A | menus.js:441 | A click anywhere on a settings row acts | Partial | 1 | S | |
| 34 A | menus.js:332, 1157-1160 | An FPS readout (SHOW FPS) | Absent | 1 | S | |
| 35 A | menus.js:975-976 | TITLE: "BEST ROUND N" on the Survival card | Partial (grades per HEAT rung) | 1 | S | A different design |
| 36 U | hud.js:458-460 | The newest feed line slides in 40 px | Absent (one TMP text) | 2 | M | One text per line |
| 37 U | hud.js:201-207 | The rank bar's fill pattern per rank (a colour-blind cue) | Absent | 2 | M | |
| 38 U | hud.js:176-178, :193-195, :600, :605-606 | Top rank: a flowing gradient and a chromatic shimmer | Absent | 2 | M | Rule 8: magenta (RIVAL only) |
| 39 U | hud.js:154-155, :144; icons.js:175 | Weapon silhouettes by the name and in the hotbar | Absent (words) | 2 | M | Mostly rectangles |
| 40 A | menus.js:393, 611-614, 1049 | TITLE's SURVIVAL MUTATORS | Absent | 2 | M | HEAT may replace it |
| 41 A | menus.js:394, 727-728, 762-764 | A typed SURVIVAL SEED box | Partial (RETRY SAME SEED / COPY SEED) | 2 | M | A text field, pad typing |
| 42 A | menus.js:185, 236-238 | Sub-screens as a framed panel over a dim with a blur | Partial (full-screen plates) | 2 | M | |

Not applicable: the boss bar / card (no bosses), the ARCLINE reticle (no ARCLINE in Survival), `score` (ENDLESS only),
`phase` (DEBUG only), signs.js (the web's training mode).

**HUD STYLE's plan (U):** the frame layer's weapon plate (UiHotbar.cs:125, its edge :127), points plate
(UiHudMotion.cs:64, its edge :66) and points label (Ui.cs:334, reparented onto the plate with hudMotion); the values
layer's weapon name (Ui.cs:332), points (Ui.cs:333) and weaponBlock (UiHotbar.cs:137, a nested canvas). A mount per
layer is clean if: each is a full-stretch RectTransform with zero offsets; each carries its own nested Canvas (moving it
every frame must not re-batch the static frame canvas, NIGHT8 U2-03); the reparenting runs once at the constructor's
end; the roll pivots on the block's centre. Watch: elements added later straight onto frame / values won't follow;
sub-pixel shimmer (round to 0.1 px); REDUCED MOTION should zero it. Cost M (a Viewmodel getter for the sway + bob).

## Feel outside the HUD (builder V)

| # | Web file:line | What the player sees | Port status | Impact | Cost | Notes |
|---|---|---|---|---|---|---|
| 1 | fx/particles.js:61, :135 | Bursts at their sizes | Partial: the size dropped | 3 | S | #41 taken |
| 2 | combat/effects.js:74-96, :241-264; config flashShapes | A muzzle star per gun in its family's colour; the 2nd frame at half | Partial: a plain quad, one colour, fixed scale (Viewmodel.cs:250-259, :865-875) | 3 | S | Strobes 12 / s on the STITCHER: D-71 first |
| 3 | (WCAG) | Flashes outside the night's FlashGate: the muzzle, the kill pop, the rail flash, the enemy's white flash; the kill confirm scaled not gated (KillConfirm.cs:170) | the web gates none | 3 | S | HANDOFF D-71 |
| 4 | main.js:2210-2218; world/arena.js:315-320; sky.js:216-220; worldMaterial.js:13, :342; player.js:36 | The world answers the rank: the neon brightens, a pulse runs along the trims, the horizon warms at S+, the speed lines take the rank colour | Partial: only the bloom (PostFx.cs:176) | 4 | M | Rule 8 at S+: D-70 |
| 5 | impact.js -> impactFx.js:48, :59-63, :108-123 | The shock ring: additive HDR, 18 cubes (9 on Low) at 1.1 | Partial | 2 | S | #51 taken (the count); the material left |
| 6 | gunFx.js:120-125 | 6 family sparks where a melee hit lands | Absent | 2 | S | #52 taken |
| 7 | player.js:124, :160; viewmodel.js:151, :571-574 | STEADY's kick and punch x0.7 | Absent | 2 | S | #42 taken (STEADY itself isn't buyable yet) |
| 8 | (port-only) | REDUCED MOTION on the enemy flinch and the shells | Gaps | 2 | S | #43 taken |
| 9 | quality.js particleCap | Effects capped on Low | Absent | 2 | S | #50 taken |
| 10 | effects.js:172-189 | A THUNDERCLAP full miss: no sparks | A port bug: sparks and a light in mid-air | 2 | S | #44 taken |
| 11 | fx/barrier.js:333-400; player.js:203-206 | Touching / shooting a field: a hex flash, ripples, a small FOV pinch | Absent (Field.shader "later"; CameraFeel.cs:11-12) | 3 | M | Through FlashGate; the pinch off with reduced motion |
| 12 | gunFx.js:96-102 | OVERLOAD's lightning burst | Absent; the combo fires (Arsenal.cs:413-434) | 3 | M | #58 taken (a burst stand-in); the arcs need row 17 |
| 13 | fx/aura.js:1-404 | The style aura on the held gun (rim, arcs by rank, motes, the core pulse, the SAVAGE+ edge crackle, SKYGRIND's leaps) | Absent; auraIntensity parsed, unused | 4 | L | D-70 first; gate the crackle; the web allocates per leap |
| 14 | fx/barrier.js (gates); zones.js:361 | A locked gate dissolves open over 1.1 s | Absent (the mesh rebuilt) | 2 | M | |
| 15 | hud.js hudAttach; viewmodel.js:930-931 | The HUD sways with the gun | Absent | 2 | M | U's row 19 |
| 16 | effects.js:132-149, :178-180 | One decal per shotgun pellet | Partial: one per shot | 2 | M | Needs the sim's pellet hits |
| 17 | fx/arcs.js:1-179 | The lightning pool (jagged ribbons, a white core) | Absent as a module (TrapViews.cs's zig-zag) | 2 | M | Rows 12, 13 build on it |
| 18 | (port bug) | Spawn beams pulsing on their own | A shared material | 1 | S | #45 taken |
| 19 | viewmodel.js:863 | The core gem turning | Effectively absent (a sphere gem) | 1 | S | Needs the faceted gem |
| 20 | effects.js:88, :254-259 | The muzzle light at the muzzle | Partial | 1 | S | #59 taken |
| 21 | xray.js (perks.js:39) | Perk machines through walls with X-RAY | Absent | 1 | S | |
| 22 | enemyKind.js:67; main.js:1522 | The death burst at mid-height | Partial | 1 | S | #53 taken (swarmer, spitter) |
| 23 | fx/post.js:7-8, :61-97 | God rays (High only) | Absent (the sun disc only) | 2 | L | A custom URP pass (D-30); frame time |
| 24 | fx/dissolve.js; viewmodel.js:653, :821 | The swapped-in gun builds out of cells | Absent; superseded by the per-gun draws | 1 | M | Skip |

Gated on gameplay the port doesn't have (they come with it): the other 12 guns (projectiles, fire, chains, beams, rigs:
projectiles.js, gunFx.js, impactFx.js, viewmodel.js :381-525); the ARCLINE's rope in Survival (fx/rope.js; the arena
has a straight line); boss / affix / RIVAL telegraphs (fx/telegraph.js); the DISCUS catch and boss shakes; rims and
x-ray for drone / boss / rival kinds; trials and their ghost; Duel's orbs; STATIC's arcs.

Already matching the web (checked so they stay out of the queue): the camera's FOVs, rolls, landing dip, kicks, the
aim-lock pinch and the trauma sources; tracers, the impact flare, the LONGWAVE's laser / dot / beam, the ricochet
line; the enemies' hit flash and stagger; killConfirm, lightPool, motes, blobShadows, post / grade, trauma, kicks,
springs.

**Cross-cutting:** (1) rule 8 blocks the web's player-side effects in the rank colours at S / SS (D-70). (2)
`Core/Combat.cs` allocates per ShotEvent / HitEvent / MeleeEvent (`:765`, `:894`, `:955`, `:990`, `:1036-1082`,
`:1156`, `:1163`) and `Arsenal.cs` :275-349 / :416-418 likewise, against PORT-RULES 6 (`Projectiles.cs` :505-507 already
reuses its events). (3) The COMET's blast 0xffa040 and the THUNDERCLAP's amber 0xffb000 sit close to the danger family.
