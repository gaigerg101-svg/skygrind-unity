# NIGHT61 builder G: notes (S3 bossRule61, then C1 text61)

Builder G, NIGHT61, 2026-10-22. No Editor and no Player in the worktree: every Player check below is owed, and the EditMode tests are
committed but not run. Branch `n61-S3` (from port/stage3 fdf4d336).

## S3: every boss says what it wants when the fight starts (design.n61.bossRule61, view row, OFF)

### Commits (n61-S3, in order)
| sha | step |
|---|---|
| 9b6f32e4 | the test first: N61BossRuleRowTests (Core) + N61BossRuleTests (Game), UnityTuning.N61G.cs, a placeholder BossRule61.cs. RED on this commit: the row is not in the config, the placeholder is always off with no bosses, words or time |
| 5d634a02 | the row: Tools/night/n61-g-rows.cjs bossRule61 writes design.n61.bossRule61 (both config copies, byte-identical) with the five lines |
| 606c7a70 | the code: BossRule61.cs (rules), UiBossRule61.cs (draw), one-line hooks in UiHud47.cs and UiHud47.Boss.cs |
| 09e8ac89 | the proof reel `boss61` (FeelReel.N61G.cs, registered in FeelReel.N41.cs beside entrance41) |

Checks run: dn-check (Tools/night/dn-check-wt.sh) 0 errors in Skygrind.Game, Skygrind.Game.Tests, Skygrind.Core.Tests and
Assembly-CSharp-Editor after each code commit. `bash Tools/night/n61-gates.sh --no-editor` at 606c7a70: **all GREEN 12:33:18** (config
copies identical; golden 39 vector sets, 22 replays exact, 0 failing, 0 mismatches; fingerprint plain / --off57 / --off58 / --off59 /
--off60 / --off61 EQUAL). The reel commit after it touches only reel code (dn-check 0 errors; gates not re-run on it).

### The red (read from frames already in Logs/feel, main tree; nothing re-run)
- THE WICK: `Logs/feel/n60-b8/w148pad/a148/shot-N29-W48-g9-intro.png` (0.6 s into the fight, 1080p): the name card "THE WICK" only, no
  rule line. `Logs/feel/n57-r3/b1on/a155/shot-N55-wick-p1.jpg` (about 2.2 s in): the centre shows the room's name "THE STACK", no rule
  line. THE WICK does have a hint, but after its 2.5 s name-card line: a148's forced line `shot-N29-W48-g5-reveal-1080.png` shows
  "[glyph] the yellow embers back." (wickIntro47 then the reveal hint, SurvivalSim.N47.Wick.cs:126-132, 177).
- THE CROWN: `Logs/feel/n57-r3/b2on/a155/shot-N55-crown37-p1.jpg` (about 2.2 s in): the room's name "THE SPIRE", no rule line.
- THE ICEBREAKER: `Logs/feel/n60-b8/l349ice/a349/shot-N49-V-n52-iceline-boss-on.png` (time into the fight: not checked): the name card and
  the bar's small hint "PARRY THE BLOCK. PUNISH STUCK FISTS" (small caps on the bar, no button glyph).
- THE KILN WYRM: the newest fight-start frame is NIGHT56's level 3 (`Logs/feel/n56-r5/b3on/a155/shot-N55-wyrm-p1.jpg`, about 2.2 s in):
  "PUNCH THE HEAD TO STAGGER IT" at the centre and on the bar, no glyph. No n57-kiln WYRM fight-start frame was found: not checked on the
  shipping level.
- THE LOOM: no fight-start frame found in Logs/feel n55-n60: not checked.
- Why the old lines do not show: under hud47 (ON) the entrance card's rule line (entrance41's design.n41.entrance41.lines) is hidden whenever
  the boss bar is up (UiHud47.Boss.cs:88), and hud47's one fight line waits while the card is up and then shows the newest, highest line of
  its queue (UiHud47.Boss.cs:55-67, 126-157): at about 2.2 s that was the room's name (rank 1) on two of the three a155 frames.

### Each boss's rule, as the code runs it with today's ON rows (the line, then the rule with file:line)
1. **THE WICK** (`wick`): **"Parry {punch} the yellow embers, then shoot."** (7 words). A parried ember lands in full and cracks a plate
   (Wick37.cs:292, weapon parry31; TryParry Wick37.cs:395-431 drains the meter, brain.Parried(parryStagger)). wick55's stagger meter is 1 a
   leg (design.n55.wick55.numbers.stagger [1, 1, 1]), so one parry opens the WINDOW (windowSeconds 4) in which every hit lands windowGain
   2 x full (Wick37.cs:293, Wick37.N55.cs:66-74); outside it a hit chips 0.3 (Wick37.N47.cs:186-187, wick55's chip). The parry is the punch
   button: the sim parries on the "punch" MeleeEvent (SurvivalSim.Wick37.cs:43, 188-194).
2. **THE ICEBREAKER** (`brawl40`): **"Get close and swing the gauntlet {fire}."** (7). A gun lands x1 within 4 m and fades to x0.2 at 12 m
   (Brawl40.cs:7-8, 395-400; design.n40.brawl40.range); the gauntlet's swing and the punch land x1 at any range (Brawl40.cs:410) and drain
   the STAGGER meter, the swing 3, the punch 1 (Brawl40.cs:443); the meter empty opens the WINDOW and breaks the ice armour the fight starts
   in (Brawl40.cs:508-522; brawl47 armorP1 1, every hit x armorChip 0.25 until then, Brawl40.cs:413). The PULSE GAUNTLET is level 2's
   weapon; held, it swings on the fire button (the HUD reads "PULSE GAUNTLET / RMB PARRY" on the a349 frame).
3. **THE KILN WYRM** (`wyrm`): **"Swing the gauntlet {fire} at its charging head."** (8). A GAUNTLET hit (weapon "gauntlet") on the head in
   the last 0.4 s of a charge at the player staggers it (SurvivalSim.Boss30.cs:282-310, the window SurvivalSim.Boss30.cs:337,
   design.n30.wyrm30.windowSeconds); staggered, every weapon lands open x the part and the THUNDERCLAP's slug x2 more, else a gun lands
   gunScale x the part (Wyrm31.cs:99-110, bosses32 on; wyrm47 sets the window to 10 s and open to 3: SurvivalSim.N47.Wyrm.cs:131-155); the
   head is x2 (Wyrm.cs:1334). **Finding:** the F punch does NOT stagger it: it reports weapon "punch" (Combat.cs:1406) and wyrmTell48, the
   punch's stagger (SurvivalSim.N48.Brawl.cs:122-147), is OFF; WyrmPilot31.cs:29-40 swings the gauntlet. So wyrm47's existing line
   "{punch} its head as it comes." (design.n47.wyrm47.numbers.linePunch, said on the first punchable windows: SurvivalSim.Boss30.cs:339)
   names the wrong button. Not changed here (another night's row); listed for the lead / GG.
4. **THE LOOM** (`loom37`): **"Hook a lit knee {grapple}, then grapple away."** (8). The grapple's rope on a lit knee of the leg's pair
   LATCHES it (Loom37.N40.cs:155-176, Latch40); then, with a rope out, the player tearDistM (7 m) farther from it than his nearest since the
   latch TEARS it (Loom37.cs:526-536); a tear staggers it (Loom37.cs:406-414) and the torn knees open the CYAN window, every hit full, else
   loom47's chip 0.3 (Loom37.cs:345-365). A walk away does not tear (a rope must be out), hence "grapple away" rather than loom40's
   "PULL AWAY".
5. **THE CROWN** (`crown37`): **"Jump {jump} and dash {dash} through the ring."** (8). THE PASS: airborne at passSpeed or more (a jump-dash;
   a slam down through it counts too), the chest crossing the ring's plane within passRadius drains its STAGGER meter (Crown37.cs:430-443)
   and, with the TORCH burning, breaks a gem (crown47 torchPassGems 1); the meter empty opens the CYAN window, every hit full
   (Crown37.cs:270), else a gun chips 0.3 (Crown37.cs:271, crown47).

The bossRise ids: Wick37.cs:219, Brawl40.cs:297, Wyrm.cs:526, Loom37.cs:227, Crown37.cs:202 (each once, in StartFight).

### What the row does (BossRule61.cs, UiBossRule61.cs; numbers UnityTuning.N61G.cs)
- Armed by the `bossRise` of one of the five, in a campaign run (`game.sim.campaign != null`), with the row on (NightRows.ViewOn and no
  -skygrindOff61-or-older / -skygrindNoBossRule61). A line over 8 words or without a {button} is refused (logged), never shown.
- Drawn by hud47's ONE fight line (40 px em at 1080p, 26.7 px at 720p by the canvas scale; its outline, underlay, glyph sprite, sentence
  case): no new label, font, colour or place. It holds the line from the rise, comes in 0.45 s after it (entrance41's moment) under the name
  card, stands the text system's 2.0 s + 0.3 s a word (cut so in + hold + out <= 5 s: 4.47 s for 7 words, 4.77 s for 8), fades, and frees
  the line. The card does not hide it (name above, rule under it). The queue waits meanwhile and resumes after (as it waits for the card);
  a queue line that was up at the rise (e.g. the room's banner) is dropped. A bar that goes down or a new rise cuts it; an armed line with no
  bar for 3 s is dropped.
- Proof (L-089): Player.log `[boss61] line: <boss> "<raw>" -> "<resolved>" | <n> words, on screen <s> s (max 5.0), em <px> px at <WxH>
  (canvas x<k>), glyph yes, text objects 1, raw tag no`; `[boss61] line: <boss> refused | dropped`, `[boss61] cut after <s> s: <why>`.
- Off: nothing armed; both hooks return at once (NIGHT60's fight line, unchanged).

### Player steps for the lead (next session)
1. Build and the row on in that copy only: `bash Tools/night/n41-build.sh <tag>`, `git diff --stat ProjectSettings`,
   `node Tools/night/n46-build-rows.cjs <tag> on n61.bossRule61`.
2. The reel, one Player per boss (on): `bash Tools/night/n61-reels.sh <tag> b61f,n52-foundry,boss61 b61i,n52-iceline,boss61
   b61k,n57-kiln,boss61 b61s,n52-span,boss61 b61p,n52-spire,boss61`. The SPOT pair's other half (THE KILN WYRM, same build):
   `bash Tools/night/n61-reels.sh <tag> b61koff,n57-kiln,boss61,-skygrindNoBossRule61`; the pair is
   `Logs/feel/<tag>/b61k/boss61/shot-N30-B61-wyrm-1000.png` vs the same name under `b61koff` (the folder under the side: not checked).
3. Read: `Logs/feel/<tag>/summary47.txt` (want `GREEN boss61 <boss>` x5 and `OFF boss61 wyrm` with shows 0); grep the proof
   `grep "\[boss61\]" Logs/player/n47-reel-<tag>-b61*.log` (want one `line:` per boss, none in the b61koff log).
4. verbs37 x5 on that build as usual (the row draws only in boss fights).

### Not checked
- Anything in a Player: the line on screen, its pixel width (an 8-word line at 40 px is estimated ~1000 px of the 1200 px label; at TEXT
  SIZE 2.0 it may overflow the screen: not measured), the glyph's look on pad and keyboard, the 720p switch inside the reel.
- The EditMode tests (written, not run; the lead runs them RED on 9b6f32e4 and GREEN on 606c7a70).
- What shows after the rule line: THE WICK's reveal hint "{punch} the yellow embers back." was posted at 2.5 s and should show right after
  the rule line (a near repeat), and other bosses' BossLine40 entry lines likewise: not measured; C1 (text61) is where repeats are cut.
- The LOOM's red; the WYRM's red on n57-kiln.
- A stale word left in an S3 file: UiBossRule61.cs:20's comment says "the a161 reel" (the reel is `boss61`); not edited on the C1 branch
  (the lead asked C1 to keep to its own files). UnityTuning.N61G.cs's two such comments were fixed in C1's first commit.

## C1: one line of text at a time, no instruction outside level 1 after its first showing (design.n61.text61, view row, OFF)

Branch `n61-C1`, from n61-S3's tip (fa353027).

### Commits (n61-C1, in order)
| sha | step |
|---|---|
| d3362568 | the test first: N61TextRowTests (Core) + N61TextTests (Game), UnityTuning.N61G.cs's Text61WaitS, a placeholder Text61.cs. RED on this commit: the row is not in the config; the placeholder is always off and its gate passes every line |
| 5c8e4614 | the row: `node Tools/night/n61-g-rows.cjs text61` (run on this branch's copy, after bossRule61; on port/stage3 re-run the script: it appends after whatever design.n61 holds) |
| 408db731 | the code: Text61.cs (switch + pure gate), UiText61.cs, two one-line hooks in UiTeach43.cs |

dn-check 0 errors in the four projects after each code commit (the chk-G scratch deleted after each run). `bash Tools/night/n61-gates.sh
--no-editor` at 408db731: **all GREEN 13:10:29** (config copies identical; golden 0 failing, 0 mismatches, 22 replays exact; fingerprint
plain / --off57 / --off58 / --off59 / --off60 / --off61 EQUAL).

### The red (my count on the frames gg-proxy named: Logs/feel/n60-b8/b8on/a157, n57-kiln entry frames, main tree)
| room | centre text blocks | what |
|---|---|---|
| kb2 | 0 | none (the corner counter only) |
| kb2c THE CLINKER BRIDGES | 3 | the room's title (top), "Midair, aim down, then LEFT CLICK" (the teach card), the previous room's "ROOM CLEAR · UNDER PAR / 15.8S · PAR 60S" (clear55) |
| kb3 THE PRESS HALL | 2 | the title, "ROOM CLEAR · UNDER PAR / 3.4S · PAR 60S" |
| kb3c THE TAP RUN | 2 | the title, "Boost: SPACE, aim down, LEFT CLICK twice" |
| kb3d THE CRACKED FLOOR | 2 | the title, "Hold LEFT CLICK grounded for a slug" |
4 of 5 frames with two or more blocks (gg-proxy: 4 of 4; kb2 0); the previous room's ROOM CLEAR on 2 of 5.

Where each block comes from: the title is hud33's message slot (MessageSlot33, its Title channel; UiHud33.cs Slot33, hud52's top plate);
the instruction is the teach card (UiTeach43.cs Teach43Draw, fed by text36's slot, UiHud36Text.cs:102-139; brief55's teachMap words);
ROOM CLEAR is clear55's card (UiClear55.cs), which lives Clear55LifeS 2.6 s of HUD time, so it outlives a fast walk into the next room (and
a157 kills the previous room's wave just before it places the camera in the next one).

### What the row does (Text61.cs, UiText61.cs; UnityTuning.Text61WaitS)
- Outside the campaign's first level (Brief55.FirstLevel: a map the campaign order lacks, e.g. survival, counts as level 1, so nothing
  changes there) and outside a boss fight (the boss bar up), the teach card's new line waits while the message slot shows a Title-channel
  line (a room's title, a ROUND banner), fading out included, at most 3 s (then it shows anyway, logged LATE); the line on the card stays
  while it is the same line (no flicker); a line already shown this run is withheld when it comes again (one count per appearance). The key is
  the line as the slot hands it: the teachMap gives one line per verb, so "once per line" is "once per verb" for the mapped lines (two
  different lines of one verb would count as two: not checked against every level's lessons).
- Everywhere, "roomEnter" ends clear55's card at once (c55Age = -1: RenderClear55 hides it on its next frame, the path its own end takes).
- Level 1 keeps its overlay; a boss fight is hud47's: the boss's rule line (bossRule61) is the one line a boss fight allows at its start, and
  this gate never touches hud47's fight line (it returns its input whenever the boss bar is up).
- The HUD's look is unchanged: no font, colour, size or place; only when a line shows.
- Proof (L-089): `[text61] shown: <line> | map <id>` (LATE marked), `[text61] waits for the title: <line>`, `[text61] withheld repeat: <line>`,
  `[text61] room clear dropped: entering <room id>`.
- Off: Text61Filter returns its input; the roomEnter handler does nothing.

### Expected on a157 with the row on (reasoning from the code, NOT measured)
kb2c: ROOM CLEAR dropped at the entry; the instruction waits for the title (or shows alone once the title has gone): 1 block. kb3: the title
only. kb3c / kb3d: the title, then the instruction (each the first of its line in that run). a157 calls ClearBanners41 one frame before each
shot: the title starts to fade in the shot's frame, and the gate reads last frame's slot (still up), so the card does not open in that
frame. Not checked in a Player.

### Player steps for the lead (next session)
1. `node Tools/night/n46-build-rows.cjs <tag> on n61.text61` on the build that carries the pick.
2. kiln entry pair: `bash Tools/night/n61-reels.sh <tag> t61on,n57-kiln,a157 t61off,n57-kiln,a157,-skygrindNoText61`; count the centre text
   blocks on the five `shot-N29-L57-n57-kiln-<room>-entry.png` per side (want <= 1 of 5 with two or more on, and ROOM CLEAR 0 of 5); the
   SPOT pair is kb2c's entry frame on vs off.
3. Once per run outside level 1: `grep "\[text61\]" Logs/player/n47-reel-<tag>-t61on.log` (each `shown:` line once; `withheld repeat:` lines
   are the repeats that did NOT show) and the same grep on that build's verbs37 logs for iceline / kiln / span / spire.
4. Level 1 keeps its overlay: the a357 reel on n52-foundry with the row on vs off (its entry frames still show their instruction line).
5. verbs37 x5 on that build.

### Not checked
- Everything in a Player (above); the EditMode tests (written, not run: RED on d3362568, GREEN expected on 408db731).
- Whether any level after 1 posts its titles on a channel other than Title (a title on another channel would not hold the card).
- Lessons that a level re-arms on purpose (a verb taught again after a death or a restart inside one run): withheld by this row after the
  first showing; not measured how often it happens.

## S5: the style meter climbs during a kill chain: SHORT (the cause is in the sim; no code changed)

Branch `n61-S5` (from port/stage3 f8f69429). The slate's rule: a view row only; a cause in the sim (Core) ends the slice SHORT with the cause
and a proposal (no sim row turns ON mid-night). The lead accepted SHORT. No row was added and no game code changed: this branch carries these
notes and one read-only measuring script.

### The red (measured; read only)
`node Tools/night/n61-g-style-red.cjs Logs/feel/n60-freeze/{vf,vi,vk,vs,vp}/verbs37/telemetry.jsonl` (FeelProbe's ui[] texts per frame):
every frame whose HUD names a style rank names DULL: foundry 1178, iceline 52, kiln 357, span 260, spire 122 frames, **1969 of 1969 DULL**.
Awards did land in those runs ("+30 WALL JUMP", "+10 DASH JUMP", "+10 SLIDE BITE", a combo "x3"): the meter is live, it never banks a rank.

### The cause (decay, thresholds or display: with the numbers behind it)
1. **Not the display.** style42 (ON since NIGHT42) draws the sim's own shown rank, `st.rank` (StyleMeter.rank -> StyleCore.Rank), whenever the
   meter is above 0 (UiHud33.Style42.cs:175-177, 195-205), and it already animates a rank up: the letter punches in with a white flash and a
   shake (UiHud33.Style42.cs:197-200, 228, 232). A sim rank above DULL is shown above DULL: NIGHT56's Player reel a256 (n52-foundry) showed
   the letter leave DULL on a combat frame (Logs/feel/n56-r4/summary47.txt and n56-r5: "the rank left DULL on frame 35 with a husk alive").
2. **The frames.** verbs37 and a157's entry frames hold no kill chain: their awards are single movement tricks (+10 to +30, diminishing) against
   the DULL drain of 17 points a second, so the meter never holds the 90 that COOL needs. The critics' "DULL in every frame" counted these.
3. **The sim's pacing (the web's numbers, golden-pinned), which caps a one-gun chain low:**
   - a kill pays `style.base[kind + "Kill"]` (Style.cs:108): husk 30, swarmer 15, spitter 40, charger 60, drone 40, warden 70
     (PortExport/config.json:131-143), x the weapon's freshness, x the movement state, plus AIRSHOT / MULTIKILL / CLOSE CALL bonuses (Style.cs:106-119);
   - per-source diminishing: the same award from the same weapon within 4 s pays x0.6 per earlier one, floor x0.15 (StyleCore.cs:177-185,
     Style.cs:168-175; style.diminish {window 4, repeat 0.6, floor 0.15}), so the KILLs of a one-gun chain pay 30, 18, 11, 6, 5 ... (arithmetic
     from the code, not measured);
   - the drain rises with the rank: 17 + 4.8 x rank points a second (Style.cs:188, applied StyleCore.cs:350), and the shown rank drops 1 s
     after the meter falls under its band (Style.cs:190-208);
   - the thresholds: COOL 90, BOLD 240, AIRBORNE 470, SAVAGE 800, SKYGRIND 1300 (style.thresholds; RankOf, Style.cs:140-145);
   - the web's own design targets for this pacing (StyleSim.cs:5-11): one-weapon "spam: never past BOLD"; "varied" play (a swap before each kill,
     half of the kills airborne) reaches SAVAGE in 35-60 s. Pinned by the golden vectors (PortExport/vectors/styleCore.json, styleSim.json).
   - Measured: a256 (NIGHT56, a Player, n52-foundry; Logs/feel/n56-r4/summary47.txt, n56-r5): 7 KESTREL kills in a row took DULL to COOL at kill 3, and it stayed COOL through kill 7
     (top rank COOL): one rank up, never the two the slate asks for. The headless bot in real fights does climb (the night's base,
     Logs/n61/flow-base-a0.txt lines 20, 48, 70, 88, 106: rank above DULL 56.3 / 28.4 / 81.4 / 81.6 / 89.0 % of fight time on foundry / iceline /
     kiln / span / spire; top rank SAVAGE / AIRBORNE / SKYGRIND / SKYGRIND / SKYGRIND), because it plays varied (rides, airborne kills, swaps).
4. **A gap found (no fix tonight; the lead's call):** a BURROWER kill pays 0 KILL points: its kind is "burrower" (Burrower43.cs:47), the kill
   award reads `style.base["burrowerKill"]` and falls back to 0 (Style.cs:108), and neither config has a `burrowerKill` (PortExport/config.json's
   style.base, lines 131-143; Design/unity-config.json: none). n57-kiln's waves hold 4 burrowers (Design/levels/n57-kiln.json, a one-line file:
   `grep -o '"burrower"'` counts 4). The bonuses (AIRSHOT, MULTIKILL, CLOSE CALL) and the hits still pay.

### Proposal (for GG and the lead; nothing built)
- **P0, a measurement first (no change):** a combat reel with a VARIED 5-kill chain (a swap before each kill, two kills airborne) on today's
  build: if the shown rank reaches BOLD there, the slate's doneWhen 2 already holds for varied play, and the gap is one-gun play only.
- **P1, an OFF Unity sim row for GG to try** (it changes the sim, so never ON mid-night: L-010): a campaign pace over the web's meter, as
  air34 did (a Unity-only row; the web's numbers and the golden vectors untouched): KILL awards exempt from the per-source diminishing (keep it
  for HIT, PARRY and tricks: the spam it was made for), or a lower DULL drain. Its doneWhen written before any number: an a256-style one-gun
  5-kill chain reaches BOLD; n46flow's style column DULL <= 30 % of fight time on every level; golden and replays unchanged with the row off.
  This is GG's call: ULTRAKILL-style meters reward variety on purpose (the web's "spam: never past BOLD").
- **P2:** a kill value for the BURROWER (e.g. the husk's 30) in a Unity-side table, with the same OFF-row rule.

### Not checked
- No Player run tonight: the varied-chain reel (P0) is owed.
- The diminishing arithmetic in cause 3 is read from the code, not measured.
- How the bot's play compares with a human's (n46flow's pilot is a bot).
