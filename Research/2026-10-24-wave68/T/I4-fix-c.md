# I4 fix (c): a --fresh pre-run tied to the level it played (builder T, NIGHT68; Logs/n68/verdict-I4.md P3)

- Skip v2 given `--room-file <file>` also prints `# level sha256: <hex> (<--level>; the level this skip played)`. It uses the same
  normalisation as the room-file line: UTF-8 bytes with a BOM kept, CR LF -> LF, sha256, lowercase hex. The chain's fresh pre-runs, which
  pass --room-file, get it with no change on their side.
- The GATE pack takes `--fresh <file>` only when every one of these holds, each anchored at a line's start:
  - `# room-file sha256:` equals the round's room-files record.
  - `# level sha256:` equals the sha256 of the level file the pack itself would run (the round's saved level).
  - Skip v2's own header line reads `n65verb skip v2: level ..., room <id>, ... seeds 21..30 (--seeds 10)`.
  - The old fallbacks are gone: the unanchored `room-file sha256 <hex>` and a `--seed 21` grep anywhere in the file.
  - Otherwise the pack says `NOT taken (<why>)` and runs the fresh skip itself.

Dry runs (Logs/n68/I4c/, NIGHT67's r3 spire round, v2):
- `fresh-v2.txt`: the pre-run printed both lines: room-file 16aaf627..., level 06cb852f.... M2 FRESH FAIL, median 1.13, exercised 8 / 10.
  The I2 fix round's launch pilot is in this DLL, so this differs from I4's 1.32 / 6 of 10.
- `gate-taken.txt`: taken, "its room-file sha256 16aaf627b6aa23f8 equals the round's record ... and its level sha256 06cb852f7bde332a the
  level this pack runs". Its verbs65-skip.txt has `# exit: 1`, from the M2 FRESH FAIL line.
- `gate-wronglevel.txt` (`fresh-v2-planted-wrong-level.txt`, the level hash's first digit changed): "NOT taken (its level sha256
  16cb852f7bde332a differs from the level this pack runs (06cb852f7bde332a, ...))". The pack ran `--seed 21 --seeds 10` itself and got the
  same M2 FRESH FAIL, 1.13, 8 / 10.
- `gate-old.txt` (I4's own pre-run, made before this fix): "NOT taken (no `# level sha256:` line in it (a pre-run from before I4 (c)))".
  The pack ran the skip itself.
- Every pack: MISSING 1 (no level review of r3-spire). The spot pictures and keys of these dry packs were deleted.
