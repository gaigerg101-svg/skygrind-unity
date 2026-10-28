# NIGHT63 builder V: Tools/night/n63-sheet.py, contact sheets of same-run pairs for GG's report (tooling, no game change)

Branch n63-SHEET from port/stage3 at 4fee5e2f.

## What it does
For each pair it lays the ON frame and its OFF twin side by side, ON | OFF. The ON frame is `<...>/<side>/<scen>/<shot>.png`; the OFF twin is
`<...>/<side>-off/<scen>/<shot>.png` (D1's same-run layout), or an explicit `--off <path>`. Each half is downscaled to 960 px wide (540 tall
from 1920 x 1080). A one-line caption sits above each pair: the row, the pose's label, and the changed px. The changed px is the toggle
line's when given; otherwise the tool measures it with n62-gun-yellow.py's rule (a pixel whose largest channel moves more than 24 of 255) and
says "measured here". Under the caption, ON / OFF tags carry the file names. Plain PIL text (its bundled default font). One sheet per row:
`Logs/n63/sheets/<row>.png` (or `--out <dir>`). It reads only the frames named and imports only numpy and PIL (`python -I` safe).

## Use
- `python -I Tools/night/n63-sheet.py --row <row> --pair <on.png> "<label>" [<changed px>] [--off <off.png>] [--pair ...] [--row <row2> ...] [--out <dir>]`
- `python -I Tools/night/n63-sheet.py --spec <file>`: one pair a line, tab-separated: `<row>`, `<on.png>`, `<label>`, then optionally
  `<changed px>` and `<off.png>`. `#` comments and blank lines are skipped. A sheet per row, in first-seen order.
- `python -I Tools/night/n63-sheet.py --selftest`: 9 / 9 PASS (measured), written into a temporary folder that is removed after:
  - two identical frames give a sheet whose halves match pixel for pixel;
  - each half is 960 x 540;
  - identical frames measure 0 px; a planted 10 x 10 change measures exactly 100;
  - a given px is printed as given;
  - the command line, the spec file and two stacked poses parse and lay out.

## Checked on real frames
n63-b4's same-run pairs from b63fS / b63fS-off (written to my scratchpad, then deleted):
- THE WICK's stagger measured 37,345 px, the lead's toggle-line number exactly.
- THE WICK's flinch, given as 22,253, printed as given.
- The sheet opened and read: caption, tags, both halves at 960 px.

## Shas
The tool and these notes, one commit (in the report message).
