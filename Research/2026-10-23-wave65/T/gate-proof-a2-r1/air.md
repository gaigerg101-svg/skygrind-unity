# a2: air hand-offs (Tools/night/n65-air.cjs)

## The round's plan (assembled at 0d163c65 (`gen-n65-foundry.cjs --slots=latest` on the room files git shows there, in a scratch tree deleted after))

```
n65-air n65-foundry a2 "1.2 KESTREL" (<scratch>\tree\Design\levels\n65\foundry.plan.json):
  numbers: jump v0 8.33 m/s, g 23.15 m/s2 (fall x1.35), double v0 7.50, dash 24 m/s x 0.25 s = 6.0 m, dash-jump v0 6.67 with g x0.5 for 0.5 s at 24 m/s, run-up 8 m/s, wall jump out 5 up 8.33 (wallfling42 on), ride 9.20 m/s, jump-off out 8 +1 up 7.30 (ride39 on), capsule 0.35 m; a ride held 2.5 m over its base
  air pieces 3:
    D1   deck  top 2.5 m over [2.5, -55.0, 7.0, -49.5] | line: THE BREECH (deck 2.5 m, NIGHT65 R): wave
    L2   ledge top 2.2 m over [-5.0, -44.0, -2.5, -40.5] | cover: THE PRESS (hull 2.2 m, NIGHT65 R): the f
    L3   ledge top 1.8 m over [-7.0, -50.0, -4.5, -48.5] | cover: THE CARTRIDGE STACK (hull 1.8 m, NIGHT65
  hand-offs 6 (piece -> piece without touching the floor, the easiest move first):
    D1 -> L2  jump + double, margin 1.15 m (gap 7.8 m); also dash 3.15, dash-jump 13.82, dash-jump + double 23.82
    D1 -> L3  jump + double, margin 1.78 m (gap 7.4 m); also dash 3.92, dash-jump 15.05, dash-jump + double 24.85
    L2 -> D1  jump + double, margin 0.75 m (gap 7.8 m); also dash 2.62, dash-jump 12.62, dash-jump + double 22.82
    L2 -> L3  jump, margin 0.75 m (gap 4.9 m); also jump + double 4.08, dash 6.22, dash-jump 16.95, dash-jump + double 26.95
    L3 -> D1  jump + double, margin 0.92 m (gap 7.4 m); also dash 2.52, dash-jump 12.05, dash-jump + double 22.45
    L3 -> L2  jump, margin 0.08 m (gap 4.9 m); also jump + double 3.62, dash 5.42, dash-jump 15.35, dash-jump + double 25.55
  longest floor-free chain: 3 piece(s): D1 -> L2 -> L3
  note: a dash-jump keeps the dash's 24 m/s for its whole flight (Mover.cs:1080; air accel never slows a body: MovementMath.Accelerate), so its reach is long; no obstacle, line-of-sight or air-control test
  flight pieces (rides + pads) on one floor-free path: 0 of 0: D1 -> L2 -> L3; without the dash-jump: 0: D1 -> L2 -> L3
  without the dash-jump: hand-offs 6, longest chain 3: D1 -> L2 -> L3, dead ends 0/3
n65-air a2: air pieces 3 (rides 0, decks 1, ledges 2, pads 0), hand-offs 6, longest floor-free chain 3, dead ends 0/3 | without the dash-jump: hand-offs 6, chain 3, dead ends 0/3 | flights handed on: 0 of 0 on one path (0 without the dash-jump)
```

## The red's plan (Design/levels/n52/foundry.plan.json)

```
n65-air n52-foundry a2 "1.2 KESTREL" (Design/levels/n52/foundry.plan.json):
  numbers: jump v0 8.33 m/s, g 23.15 m/s2 (fall x1.35), double v0 7.50, dash 24 m/s x 0.25 s = 6.0 m, dash-jump v0 6.67 with g x0.5 for 0.5 s at 24 m/s, run-up 8 m/s, wall jump out 5 up 8.33 (wallfling42 on), ride 9.20 m/s, jump-off out 8 +1 up 7.30 (ride39 on), capsule 0.35 m; a ride held 2.5 m over its base
  air pieces 1:
    D1   deck  top 2.0 m over [-7.0, -55.0, -1.0, -50.0] | line: the north deck (2 m): where the first hu
  hand-offs 0 (piece -> piece without touching the floor, the easiest move first):
  longest floor-free chain: 1 piece(s): D1
  note: a dash-jump keeps the dash's 24 m/s for its whole flight (Mover.cs:1080; air accel never slows a body: MovementMath.Accelerate), so its reach is long; no obstacle, line-of-sight or air-control test
  flight pieces (rides + pads) on one floor-free path: 0 of 0: D1; without the dash-jump: 0: D1
  without the dash-jump: hand-offs 0, longest chain 1: D1, dead ends 1/1 (D1)
n65-air a2: air pieces 1 (rides 0, decks 1, ledges 0, pads 0), hand-offs 0, longest floor-free chain 1, dead ends 1/1 (D1) | without the dash-jump: hand-offs 0, chain 1, dead ends 1/1 | flights handed on: 0 of 0 on one path (0 without the dash-jump)
```
