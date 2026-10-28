# FORGE74 decisions (2026-10-27; the forge's calls, each with its reason; GG gave no input and asked for none)

**D-1 The focus is THE ROOMS, then the bosses' one colour language.** gg-proxy's close ranking on n73-freeze (Logs/n73/gg-proxy-close.md) puts
"the rooms are big empty boxes; nothing in them asks you to move" first: 7 dated quotes over 6 days (OWNER:62, 74, 95, 110, 112, 118; CORE:7).
NIGHT72 and NIGHT73 had no room builder, and the rows they shipped were light, colour and camera. Items 2 and 3 (bosses in the player's cyan;
cannot tell where to hit THE CROWN) are the next two. Not tonight: the hurt static (GG's own 2026-10-13 ask; DECIDE73 keeps it his call).

**D-2 The harvest first: GG's play-build rooms ON.** The four level rows (n65.foundry65, n67.iceline67, n67.kiln67, n67.spire67) carry the
eight rooms GG played in Builds/n71-play. They have been OFF since: NIGHT72's LV slices went SHORT unpaired. The forge proved:
- the repo's n67-iceline / n67-kiln / n67-spire level files are byte-identical to n71-play's (sha1 348b0b1c76, cfc4a7a260, 0390bf6182;
  spike.md S1), so turning the row ON gives GG exactly the rooms he played;
- n65-foundry is not (repo 3366a7eab4 vs play bd59e6a856): it needs NIGHT72's N0 (regenerate with b1, a8 and no 1.3 RIDE roof, FORGE72
  spike S3) before its pair;
- verbs37 with each row ON on n73-play's code (spike.md S2).

**D-3 Rooms built tonight ship inside the level rows, with a two-build pair.** A rebuilt room is a room file regenerated into its level file
(gen-slots.cjs / gen-n65-foundry.cjs, the NIGHT71 pipeline). It cannot be toggled inside one build, so its pair is a planted tour65 pose at the room's entry
door (`-skygrindTourPlant65`; the generators keep doors bit for bit) on the train build that holds it against the same planted pose on the
train build before its pick (same config, same flags; red team 4: the rooms' own tour poses differ between old and new rooms). The off switch is the
level row (`-skygrindRows72 <row>:0` restores the shipped n52 / n57 level). Every other D-1 condition holds: the blind SPOT, the guards
(verbs37 on that level, the room guards of forge71's builder brief section R, the smoke) and the lead's look.

**D-4 Rule W, the lead's call on the bosses' colour (T-153; DECIDE73's open question, aged three waves):** on a boss, cyan means "shoot
here" and nothing else. A weak point is player-usable (CLAUDE.md rule 8: cyan is player-usable), so the open weak point keeps cyan; every
other cyan part of a boss takes that boss's own non-danger identity colour (GG 2026-10-20: each boss its own palette). One row, weak74,
reversible. FORGE72's APEX lesson: when the owner's calls do not come, decide by a written rule, one reversible commit each.

**D-5 Four builders: R and S on rooms, B on bosses, L on light and sight.** Two of four on rooms is the forge experiment (FORGE-EXP below).
The lead keeps the harvest, the trains, the pairs, the flips and the close (the H train plus four pair trains; the lead's busiest stretch is 5:00-6:30, so the drafts start at 5:00 and the
reviewer runs at 5:20).

**D-6 An 8-hour clock:** BUILD 0:00-7:15, FREEZE 7:15-8:00, CAP 8:00 (Tools/night/n74-clock.cjs). Last pair train picks by 5:45, the code
freeze 6:30, the close starts 7:00, the clock stops between 7:30 and 8:00 (n72-close-check.cjs --night n74 reads its windows from capMin).

**D-7 Tools are copies, run in the forge.** Tools/night/n74-tools.cjs wrote n74-clock, -reels, -prune, -pair, -slate-set, -caps, -row-add and
-rows from NIGHT72 / 73's (spike.md S3 lists each one's forge run). The night's six view rows are in rows74.json.

**D-8 Not tonight:** enemy kinds, AI, waves (GG's); audio; Mover.cs, `simulate()` order, sim rows; a new level; the HUD layout; the hurt
static (GG's call); deleting worktrees (GG's); the room GATE as a verdict (a guard only).

**FORGE-EXP (one change):** two of four builders on rooms, each room a one-verb card (forge71 brief section R) judged by the blind spotter on a
two-build pair. Hypothesis: rebuilt rooms reach the repo's game (NIGHT71 built eight and shipped them only in the play build; NIGHT72 and
NIGHT73 shipped none). Measure (NIGHT74-REPORT section 8): the rooms ON in Builds/n74-play's default boot, counted as GG's play-build rooms
(the level rows) plus tonight's new rooms.
