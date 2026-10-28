# NIGHT64 builder G notes (branch n64-U1, from A1 d86ba168)

## U1 ONE ORDER AT A TIME (design.n64.order64, OFF)

### The red (measured first, read-only, before the GREEN)
- Frame seen: `Logs/feel/n63-b12/bk/boss63/shot-N30-B63-wyrm-stagger.png` (the frame gg-proxy #3 named): "STAGGERED: HOLD LEFT CLICK, SLUG IT"
  small under the bar's right end and large in the centre at once.
- Number: NIGHT63's boss63 telemetry (FeelProbe records each UI text with its alpha per frame), frames where the fight line and the bar's
  hint are both drawn with the same text. Command: `node Tools/night/n64-u1-dup.cjs Logs/feel/n63-b12/{bf,bi,bk,bp,bs}/boss63/telemetry.jsonl`
  (the tool is this branch's 7c9623c6; `--selftest` PLANTED GREEN, 7 frames, 2 duplicates). Of 1529 frames each:

| side | boss | fight line drawn | bar hint drawn | both, same text | by text |
|---|---|---|---|---|---|
| bi | THE ICEBREAKER | 1483 | 1214 | **1173** | PARRY THE BLOCK. PUNISH STUCK FISTS 546, STAGGERED: EVERY HIT COUNTS 505, L-SHIFT ASIDE, THEN HIT IT 68, FISTS STUCK: HIT IT NOW 54 |
| bk | THE KILN WYRM | 1170 | 1196 | **391** | PUNCH THE HEAD TO STAGGER IT 295, STAGGERED: HOLD LEFT CLICK, SLUG IT 60, F ITS HEAD AS IT COMES 36 |
| bp | THE CROWN | 662 | 208 | **205** | STAGGERED: EVERY HIT COUNTS 205 |
| bs | THE LOOM | 662 | 249 | **205** | STAGGERED: EVERY HIT COUNTS 205 |
| bf | THE WICK | 662 | 0 | **0** | (its bar draws no hint) |

  gg-proxy counted 4 of 18 still shots; the telemetry shows the duplicate on 4 of 5 bosses for 13-77 % of the fight's frames.
- The slate's red proper (the counter's row-off half on the GATE build) can only be read on that build: the counter prints it.

### What changed (view only; no sim, no config number, the HUD's look untouched)
- `Assets/Scripts/Game/Order64.N64G.cs`: the rules (pure). Row on: the bar's hint is NOT drawn on a frame the fight line draws the same order
  (alpha > 0, the same text, case and edge spaces ignored); on every other frame it is drawn exactly as without the row
  (`BossRule61.HintShown`: its text, bossRule61's hold). So the order returns to the bar when the fight line fades, and an order the fight
  line never draws (it waited behind the entrance card, or a newer line replaced it) stays in the bar: every order shown, one copy at a time.
  Row off: always as without the row. The switch: the row via `NightRows.ViewOn` every frame (D1's Frame row), `-skygrindOff64` or older,
  `-skygrindNoOrder64` (any case). The per-boss counter `Order64.Count` and its two lines.
- `Assets/Scripts/Game/UiOrder64.N64G.cs`: the HUD's write and the counter. Row on: the holder set every frame. Row off: nothing written,
  except the first frame off after a frame on (D1's held off frame), which puts the hint back as without the row.
- `Assets/Scripts/Game/UiHud47.Boss.cs` Fight47Late: one line, `Order64Late();` right after `Fight47Tick(frameDt);`. That makes it the
  frame's last write on the hint: after Bar43Draw's `Text33.Set` (RenderCard38 -> RenderBar41, earlier in RenderHud33), after
  UiBossRule61's hold (BossRule61Sub) and release (BossRule61HintRelease) inside Fight47Tick, and after the queue set the fight line's alpha
  (the red team's finding: no one-frame duplicate at the rule line's release).
- Choice made: "drawn" means alpha > 0 on this frame, not "queued". A line waiting behind the entrance card (alpha 0) leaves the order in the
  bar; hiding it while queued would lose an order a newer line replaces before it is drawn (the off game shows it in the bar). Cost: an
  order that waits in the queue shows in the bar until the fight line comes in (at most Hud47FightMinS behind a live line, or the card's
  time), then moves to the centre, then returns to the bar. Not measured how often (not checked).

### The proof (the RED commit's contract)
- In-game counter, printed with the row on or off, per boss (the bar's name), on HUD frames with the bar up; same-run held frames apart:
  - `[order64] <boss> order "<text>" drawn by the fight line <n> frames: would-duplicate <w>, duplicate shown <s> | the bar's hint after it: drawn | row order64 on|off | t <s> s (FeelClock)` when the fight line stops drawing an order;
  - `[order64] <boss> | row order64 on <n> of <m> frames | would-duplicate <W> frames (...) | duplicate shown <S> frames (both drawn) | held <h> frames: would-duplicate <hw>, duplicate shown <hs> (...) | orders <o>: shown <k> (by the fight line .., in the bar .., in the bar after the fight line ..) | <why>, t <s> s (FeelClock)` after each order line, at the bar's going down, a new boss, a new run and the quit.
  - would-duplicate is defined as the row-off draw (the fight line draws the bar's order while the game without the row draws the hint), so
    one same-run run gives both numbers; duplicate shown is what is drawn now.
- Expected row OFF: duplicate shown = would-duplicate > 0. Row ON: would-duplicate > 0, duplicate shown 0; held frames: duplicate shown =
  the pairs whose shot fell on a fight-line frame (their off halves), never more; orders shown = orders, the same count as the row-off run.
- Second meter (pixels-free, from FeelProbe's UI record): `node Tools/night/n64-u1-dup.cjs <side>/boss63/telemetry.jsonl`: row on 0 both
  drawn with the same text, row off > 0 (held frames are never in telemetry.jsonl).

### Player steps (the lead's)
1. Build rows: `node Tools/night/n46-build-rows.cjs <gate> on n64.order64` (with the build's other rows).
2. Reels (sides lowercase): `bash Tools/night/n64-reels.sh <gate> uk,n57-kiln,boss63,-skygrindSameRun63,n64.order64 ul,n52-span,boss63,-skygrindSameRun63,n64.order64 ukoff,n57-kiln,boss63,-skygrindNoOrder64`
   (optional, the worst case: `ui,n52-iceline,boss63,-skygrindSameRun63,n64.order64`, THE ICEBREAKER's 1173 red frames).
3. SPOT pose: boss63 THE KILN WYRM stagger: `node Tools/night/n64-spot-pair.cjs U1 Logs/feel/<gate>/uk-on/boss63/shot-N30-B63-wyrm-stagger.png Logs/feel/<gate>/uk-off/boss63/shot-N30-B63-wyrm-stagger.png <tag>`.
   Why a player notices it there: the stagger is the fight's one big order; on NIGHT63's frame it is printed twice. Visible change: the
   small "STAGGERED: HOLD LEFT CLICK, SLUG IT" under the bar's right end (18 px) is gone in the on half; the large centre line stays. Thin by
   nature (about 360 x 20 px): the SPOT naming it is the bar.
4. Meter and proof lines: `grep "\[order64\]" Logs/player/n47-reel-<gate>-uk.log` (and -ul, -ukoff); `node Tools/night/n64-u1-dup.cjs Logs/feel/<gate>/{uk,ul,ukoff}/boss63/telemetry.jsonl` (`--selftest` first).
5. Guards: N61BossRule*, N63BossRule* and N64OrderTests in the Editor; verbs37 n57-kiln x1; fingerprints EQUAL (view only).

### Checks (worktree)
- `bash Tools/night/dn-check-wt.sh <scratch>`: RED 0 errors; GREEN 0 errors (the scratch deleted). The first GREEN run read 3384 errors:
  the scratch dir `scratchpad/dn` was shared with another builder's run at the same moment (it was emptied mid-build); re-run in
  `scratchpad/dn-g64`: 0. Builders sharing the session scratchpad need their own dn-check folder name.
- `bash Tools/night/n64-gates.sh --no-editor`: all GREEN 11:10:43 (golden 0 mismatches; fingerprints plain and --off57..--off64 EQUAL).

### Not checked
- N64OrderTests (and every other test) not run: no Editor in the worktree; traced by hand against the code.
- No Player: whether the counter's numbers come out as expected; whether the totals line printed from `Application.quitting` reaches
  Player.log (the totals are also printed after every order line, so the would-duplicate / duplicate-shown totals are complete at the last
  order's end unless the run ends while a line is drawn).
- The boss name in the lines is the bar's text (the wyrm's bar reads "KILN WYRM" on n63-b12's frame), not the boss id.
- The queued-order flicker (bar -> centre -> bar) frequency.
- Once U1 PASSES, any change to these files goes through a GATE again or does not ship (T63c, L-145).

## Leftover 2 THE FIGHT LINE IS THE FIGHT'S (design.n64.fightLine64, OFF; branch n64-L2 from port/stage3 a19ce296)

### The red (measured first, read-only)
- Frame seen: `Logs/feel/n63-b12/bf/boss63/shot-N30-B63-wick-windup.png`: "THE STACK" large in the centre under THE WICK's bar, mid-fight.
- Number: `node Tools/night/n64-l2-titles.cjs --level Design/levels/<level>.json Logs/feel/n63-b12/<side>/boss63/telemetry.jsonl`
  (this branch's 7c5612ad; `--selftest` PLANTED GREEN, 7 frames, bar 6, 3 titles): frames the fight line draws a room title while the boss
  bar is drawn, of 1529: **bf THE WICK "THE STACK" 170 (f314-f483), bp THE CROWN "THE SPIRE" 170 (f332-f501), bs THE LOOM "THE NARROWS"
  170 (f332-f501), bk THE KILN WYRM 0, bi THE ICEBREAKER 0**. Each comes in right after the rule line (bossRule61) ends. None of the three
  titles is ever drawn in the slot before the fight: the bar is up from f19 on THE WICK (my scratch listing of every UI text naming the room).
- The path: at the boss room's entry the zone banner (Ui.cs zoneEnter -> Banner -> the message slot) and the boss bar come up together;
  hud47's Fight47Late takes the slot's line into the fight line's queue (prio 1) and hides the slot; it waits behind the rule line and is
  drawn after it.

### What changed (view only; no sim, no new number, the HUD's look untouched)
- `Assets/Scripts/Game/FightLine64.N64G.cs`: the rules (pure): the switch (`-skygrindOff64` or older, `-skygrindNoFightLine64`, the row via
  `NightRows.ViewOn` every frame); `Titles` (each zone's or room's name, as posted and without its number, Copy47.StripNumber, case ignored);
  `Hide` (row on, bar up, the fight line drawn, a room title); `Later` (a hidden title held once, posted once after the fight, counted
  when the slot draws it); the per-boss `Count` and its lines.
- `Assets/Scripts/Game/UiFightLine64.N64G.cs`: the HUD's half. Row on: on a frame the queue draws a room title with the bar up, the fight
  line's alpha is set to 0 for that frame (the queue keeps its turn, its age and its time: every later line comes in on the same frame as
  without the row). When the bar goes down each held title is posted to the message slot (Post33, Ui.cs zoneEnter's own banner time). Room
  titles: every zoneEnter's name (subscribed once) and the campaign level's rooms' names.
- `Assets/Scripts/Game/UiHud33.cs` RenderHud33: one line, `if (hud47Built) FightLine64Late();` after `Fight47Late(frameDt)` and before
  `Top52()`. Deliberately NOT in UiHud47.Boss.cs, U1's file (U1's passFiles stay untouched).
- Why the hide is at draw time, not in the queue: D1's same-run toggle holds one frame with the row off. If the row kept the title out of
  the queue, the queue's history would differ and the off half could not draw the title; hiding the drawn line keeps the off half exact.
  Cost: the centre is empty for the title's 2.85 s (the boss's order shows in the bar meanwhile, order64's rule).

### The proof (the RED commit's contract)
- `[fight64] <boss> title "<text>" in the fight line mid-fight <n> frames: would draw <w>, drawn <d> | held for after the fight | row
  fightLine64 on|off | t <s> s (FeelClock)` at a title run's end; the totals `[fight64] <boss> | row fightLine64 on <n> of <m> frames |
  room-title frames in the fight line: would draw <W>, drawn <D> | held <h> frames: would draw .., drawn .. | titles mid-fight <k>: hidden
  <x>, held for after the fight <y>, posted after <p>, shown after <s> | <why>, t <s> s (FeelClock)` after each title line, at the bar's
  going down, a new boss, a new run and the quit; `[fight64] title "<text>" posted to the message slot after the fight` / `shown after the
  fight (the message slot)`.
- Expected row OFF: drawn = would draw > 0 (about 170 reel frames on THE WICK), hidden 0. Row ON: would draw > 0, drawn 0; held: drawn <=
  the pairs on a title frame (their off halves); hidden 1, held for after the fight 1; boss63's fight never ends in its 25 s hold, so
  "posted after 0" there (the post after a fight is not exercised by boss63: not checked).
- Second meter: `node Tools/night/n64-l2-titles.cjs --level Design/levels/n52-foundry.json Logs/feel/<gate>/<side>/boss63/telemetry.jsonl`:
  row on 0, row off about 170.

### Player steps (the lead's)
1. Build rows: `node Tools/night/n46-build-rows.cjs <gate> on n64.fightLine64` (with the build's other rows).
2. Reels: `bash Tools/night/n64-reels.sh <gate> lf,n52-foundry,boss63,-skygrindSameRun63,n64.fightLine64 lp,n52-spire,boss63,-skygrindSameRun63,n64.fightLine64 lfoff,n52-foundry,boss63,-skygrindNoFightLine64`.
3. SPOT pose: boss63 THE WICK wind-up shot: `lf-on` vs `lf-off` `/boss63/shot-N30-B63-wick-windup.png` (on n63-b12 it fell inside the
   title's window). Why a player notices: the centre line is the biggest text on screen mid-fight; it names the room instead of the fight.
   Visible change: the large centre "THE STACK" (about 260 x 40 px) is gone in the on half. Second pose: THE CROWN's idle shot (`lp`,
   `shot-N30-B63-crown37-idle.png`, "THE SPIRE").
4. Meter and proof lines: `grep "\[fight64\]" Logs/player/n47-reel-<gate>-{lf,lp,lfoff}.log`; the telemetry meter above (`--selftest` first).
5. Guards: N64FightLineTests, N64OrderTests and the hud47 / bossRule61 tests in the Editor; verbs37 n52-foundry x1 (the level it touches
   first; spire and span carry the same title path); fingerprints EQUAL (view only).

### Checks (worktree)
- dn-check RED 0 errors, GREEN 0 errors (own folder `scratchpad/dn-g64`, deleted after).
- `bash Tools/night/n64-gates.sh --no-editor` on 7c5612ad: all GREEN 11:26:42 (config identical, golden, fingerprints plain and
  --off57..--off64 EQUAL).

### Not checked
- N64FightLineTests not run (no Editor); traced by hand.
- No Player: the counter's numbers, the same-run pair, whether the wind-up shot still falls in the title window on the GATE build.
- The post after a fight: no reel kills a boss and keeps running; a title posted after the fight can meet the finale and the exit's own
  lines in the slot (the slot's priorities decide; not checked).
- Only room titles are held back; other slot lines (ROUND, a gate's OPEN, objective banners) still take the fight line as before.
- In a same-run run that continues after a fight, the slot's post-fight title exists only on the row-on history (the held off frame then
  shows it too): the pair is exact only during the fight.
