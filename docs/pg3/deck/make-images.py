# PG3 deck: copies and resizes the screenshots the deck uses (game frames from the muted reels, the ChatGPT reference image, the Editor renders)
# into docs/pg3/deck/img/. Usage: python -I docs/pg3/deck/make-images.py <scratchpad dir>
import os, sys
from PIL import Image

root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
scratch = sys.argv[1]
out = os.path.join(os.path.dirname(__file__), 'img')
os.makedirs(out, exist_ok=True)
F = lambda *p: os.path.join(root, 'Logs', 'feel', *p)
shots = {
    'hero': F('pg3-tripo5', 't6', 'kill6', 'f0020.jpg'),
    'icebreaker': F('pg3-t10', 'bi', 'boss63', 'f0650.jpg'),
    'horde': F('pg3-t10', 'hk', 'horde5', 'f0005.jpg'),
    'charge': F('pg3-tripo5', 'tk', 'kill12', 'f0075.jpg'),
    'punch': F('pg3-tripo7', 'fs', 'fist', 'f0300.jpg'),
    'crown': F('pg3-t10', 'bp', 'boss63', 'f0500.jpg'),
    'longwave': F('pg3-t10', 'ws', 'swapall', 'f0175.jpg'),
    'thunderclap': F('pg3-tripo4', 'bk', 'boss63', 'f0700.jpg'),
}
for name, src in shots.items():
    im = Image.open(src).convert('RGB')
    im.thumbnail((1600, 900))
    im.save(os.path.join(out, name + '.jpg'), quality=86)
# the reference -> model -> game chain for the husk
Image.open(r'C:\Users\gaige\Downloads\Low-Poly Armored Sentinel.png').convert('RGBA').resize((700, 700)).save(os.path.join(out, 'ref-husk.png'))
Image.open(os.path.join(scratch, 'renders', 'husk-front.png')).convert('RGB').save(os.path.join(out, 'tripo-husk.png'))
game = Image.open(F('pg3-tripo7', 'fs', 'fist', 'f0300.jpg')).convert('RGB').crop((560, 120, 1360, 920)).resize((700, 700))
game.save(os.path.join(out, 'game-husk.jpg'), quality=86)
# the limb cut: rest / stride / wind-up strip (Editor renders)
Image.open(os.path.join(scratch, 'limbs.jpg')).convert('RGB').save(os.path.join(out, 'limbs.jpg'), quality=88)
for f in sorted(os.listdir(out)):
    print(f, Image.open(os.path.join(out, f)).size)
