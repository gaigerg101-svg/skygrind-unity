# g8: air hand-offs (Tools/night/n65-air.cjs)

## The round's plan (the round's saved plan (Logs/n67/dry-pack2-iceline/iceline.plan.json))

```
n65-air n67-iceline g8 "2.8 THE STAMPEDE" (C:\Users\gaige\dev\SKYGRIND-unity\.claude\worktrees\agent-af48e888c59fa3626\Logs\n67\dry-pack2-iceline\iceline.plan.json):
  numbers: jump v0 8.33 m/s, g 23.15 m/s2 (fall x1.35), double v0 7.50, dash 24 m/s x 0.25 s = 6.0 m, dash-jump v0 6.67 with g x0.5 for 0.5 s at 24 m/s, run-up 8 m/s, wall jump out 5 up 8.33 (wallfling42 on), ride 9.20 m/s, jump-off out 8 +1 up 7.30 (ride39 on), capsule 0.35 m; a ride held 2.5 m over its base
  air pieces 5:
    D1   deck  top 3.0 m over [-8.0, -250.0, -3.0, -246.0] | line: THE GALLERY (west, 3 m, 5 x 4 m; EXT53: 
    D2   deck  top 5.5 m over [3.0, -250.0, 11.0, -246.0] | line: THE SIGNAL BOX (east, 5.5 m, 8 x 4 m; th
    R3   ride  face x = -9.4 (+x), -246.0..-223.0 (23.0 m), band 0.0..12.0, held at 2.5 | line: the WEST LANE WALL (ride, cyan, 23 m; EX
    R4   ride  face x = 10.4 (-x), -246.0..-223.0 (23.0 m), band 0.0..12.0, held at 2.5 | line: the EAST LANE WALL (ride, cyan, 23 m x 5
    P5   pad   at (-8.0, 0.0, -243.0) -> (7.0, 5.5, -247.0) (on the floor) | line
  hand-offs 7 (piece -> piece without touching the floor, the easiest move first):
    D1 -> D2  dash-jump + double, margin 18.65 m (gap 6.4 m)
    D1 -> R3  jump, margin 3.39 m of height (gap 1.1 m); also jump + double 3.39, dash 2.50, dash-jump 2.82, dash-jump + double 2.82
    D1 -> R4  dash-jump, margin 4.37 m of height (gap 13.1 m); also dash-jump + double 4.44
    D2 -> D1  jump, margin 0.52 m (gap 6.4 m); also jump + double 3.65, dash 6.05, dash-jump 19.05, dash-jump + double 28.45
    D2 -> R3  dash, margin 3.13 m of height (gap 12.1 m); also dash-jump 4.33, dash-jump + double 4.33
    R3 -> D1  jump-off, margin 2.65 m (gap 1.8 m); also jump-off + double 6.62, wall jump 1.25, wall jump + double 3.50, dash 8.38
    P5 -> D2  pad (gap 15.5 m)
  longest floor-free chain: 5 piece(s): P5 -> D2 -> R3 -> D1 -> R4 (opened by a floor pad)
  note: a dash-jump keeps the dash's 24 m/s for its whole flight (Mover.cs:1080; air accel never slows a body: MovementMath.Accelerate), so its reach is long; no obstacle, line-of-sight or air-control test
  flight pieces (rides + pads) on one floor-free path: 3 of 3: P5 -> D2 -> R3 -> D1 -> R4; without the dash-jump: 2: P5 -> D2 -> D1 -> R3
  without the dash-jump: hand-offs 5, longest chain 4: P5 -> D2 -> D1 -> R3, dead ends 1/4 (R4)
n65-air g8: air pieces 5 (rides 2, decks 2, ledges 0, pads 1), hand-offs 7, longest floor-free chain 5, dead ends 1/4 (R4) | without the dash-jump: hand-offs 5, chain 4, dead ends 1/4 | flights handed on: 3 of 3 on one path (2 without the dash-jump)
```

## The red's plan (Design/levels/n52/iceline.plan.json)

```
n65-air n52-iceline g8 "2.8 THE STAMPEDE" (Design/levels/n52/iceline.plan.json):
  numbers: jump v0 8.33 m/s, g 23.15 m/s2 (fall x1.35), double v0 7.50, dash 24 m/s x 0.25 s = 6.0 m, dash-jump v0 6.67 with g x0.5 for 0.5 s at 24 m/s, run-up 8 m/s, wall jump out 5 up 8.33 (wallfling42 on), ride 9.20 m/s, jump-off out 8 +1 up 7.30 (ride39 on), capsule 0.35 m; a ride held 2.5 m over its base
  air pieces 5:
    D1   deck  top 3.0 m over [-8.0, -250.0, -3.0, -246.0] | line: THE GALLERY (west, 3 m, 5 x 4 m; EXT53: 
    D2   deck  top 5.5 m over [3.0, -250.0, 11.0, -246.0] | line: THE SIGNAL BOX (east, 5.5 m, 8 x 4 m; th
    R3   ride  face x = -9.4 (+x), -246.0..-223.0 (23.0 m), band 0.0..12.0, held at 2.5 | line: the WEST LANE WALL (ride, cyan, 23 m; EX
    R4   ride  face x = 10.4 (-x), -246.0..-223.0 (23.0 m), band 0.0..12.0, held at 2.5 | line: the EAST LANE WALL (ride, cyan, 23 m x 5
    P5   pad   at (-8.0, 0.0, -243.0) -> (7.0, 5.5, -247.0) (on the floor) | line
  hand-offs 7 (piece -> piece without touching the floor, the easiest move first):
    D1 -> D2  dash-jump + double, margin 18.65 m (gap 6.4 m)
    D1 -> R3  jump, margin 3.39 m of height (gap 1.1 m); also jump + double 3.39, dash 2.50, dash-jump 2.82, dash-jump + double 2.82
    D1 -> R4  dash-jump, margin 4.37 m of height (gap 13.1 m); also dash-jump + double 4.44
    D2 -> D1  jump, margin 0.52 m (gap 6.4 m); also jump + double 3.65, dash 6.05, dash-jump 19.05, dash-jump + double 28.45
    D2 -> R3  dash, margin 3.13 m of height (gap 12.1 m); also dash-jump 4.33, dash-jump + double 4.33
    R3 -> D1  jump-off, margin 2.65 m (gap 1.8 m); also jump-off + double 6.62, wall jump 1.25, wall jump + double 3.50, dash 8.38
    P5 -> D2  pad (gap 15.5 m)
  longest floor-free chain: 5 piece(s): P5 -> D2 -> R3 -> D1 -> R4 (opened by a floor pad)
  note: a dash-jump keeps the dash's 24 m/s for its whole flight (Mover.cs:1080; air accel never slows a body: MovementMath.Accelerate), so its reach is long; no obstacle, line-of-sight or air-control test
  flight pieces (rides + pads) on one floor-free path: 3 of 3: P5 -> D2 -> R3 -> D1 -> R4; without the dash-jump: 2: P5 -> D2 -> D1 -> R3
  without the dash-jump: hand-offs 5, longest chain 4: P5 -> D2 -> D1 -> R3, dead ends 1/4 (R4)
n65-air g8: air pieces 5 (rides 2, decks 2, ledges 0, pads 1), hand-offs 7, longest floor-free chain 5, dead ends 1/4 (R4) | without the dash-jump: hand-offs 5, chain 4, dead ends 1/4 | flights handed on: 3 of 3 on one path (2 without the dash-jump)
```
