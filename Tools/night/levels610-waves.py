"""The waves of levels 6-10 as counts per kind: edit here, then `python Tools/night/levels610-waves.py` writes levels610-waves.json and gen-levels610.py reads it."""
import json, os
H, S, SP, C, L, D, W = 'husk', 'swarmer', 'spitter', 'charger', 'lancer', 'drone', 'warden'
def n(k, c): return [k] * c
W_ = {
 '06': {
  'b1': [n(H,5)+n(S,2), n(H,6)+n(S,4)],
  'b2': [n(H,6)+n(S,2)+n(W,1), n(H,6)+n(S,4)+n(W,1), n(H,6)+n(S,5)+n(W,2)],
  'b3': [n(H,6)+n(W,1)+n(C,1), n(H,6)+n(C,2)+n(W,1), n(H,8)+n(W,2)+n(C,1)]},
 '07': {
  'w1': [n(H,5)+n(S,2), n(H,6)+n(S,3)],
  'w2': [n(H,5)+n(W,1), n(H,5)+n(W,2), n(H,5)+n(S,3)+n(W,2)],
  'w3': [n(H,2)+n(S,1), n(H,3)+n(S,1), n(H,3)+n(S,2)],
  'w4': [n(H,4)+n(S,2)+n(SP,1), n(H,3)+n(C,1)+n(W,1), n(H,3)+n(L,1)+n(D,1), n(H,3)+n(W,1)+n(C,1)+n(SP,1)+n(D,1)]},
 '08': {
  'p1': [n(H,6)+n(S,4), n(H,6)+n(S,6)],
  'p2': [n(H,6)+n(C,2)+n(S,4), n(H,6)+n(D,3)+n(C,2)],
  'p3': [n(H,8)+n(C,3)+n(S,4), n(H,6)+n(D,3)+n(C,3)+n(S,4)]},
 '09': {
  'm1': [n(H,6)+n(S,3), n(H,6)+n(SP,2)+n(S,3)],
  'm2': [n(H,5)+n(D,2)+n(SP,1), n(H,5)+n(D,2)+n(SP,2)+n(S,2)],
  'm3': [n(H,4)+n(D,1), n(H,4)+n(SP,1)+n(D,1), n(H,4)+n(SP,1)+n(D,1)+n(S,1)]},
 '10': {
  'f1': [n(H,4)+n(S,2)+n(SP,1), n(H,4)+n(S,2)+n(SP,1), n(H,4)+n(S,2)+n(SP,1)],
  'f2': [n(H,4)+n(C,1)+n(D,1), n(H,4)+n(C,1)+n(D,1), n(H,4)+n(W,1)+n(L,1)+n(S,2)],
  'f3': [n(H,4)+n(S,3)+n(W,1), n(H,4)+n(C,1)+n(L,1), n(H,4)+n(W,1)+n(C,1)+n(D,1)],
  'f4': [n(L,1), n(C,1), n(L,1)+n(C,1)]},
}
json.dump(W_, open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'levels610-waves.json'), 'w'))
