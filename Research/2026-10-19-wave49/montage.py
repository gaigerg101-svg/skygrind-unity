"""Contact sheet for the lead's look (NIGHT48). Usage: python -I montage.py <out.png> <cols> <tile_width> <label>=<path> ...
Each tile is the image scaled to tile_width, with its label drawn in the top-left corner. Read-only on the inputs."""
import sys
from PIL import Image, ImageDraw

out, cols, tw = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
items = []
for arg in sys.argv[4:]:
    label, _, path = arg.partition("=")
    items.append((label, path))

tiles = []
for label, path in items:
    try:
        im = Image.open(path).convert("RGB")
    except OSError as e:
        im = Image.new("RGB", (tw, int(tw * 9 / 16)), (60, 0, 0))
        ImageDraw.Draw(im).text((10, 10), "MISSING " + str(e)[:60], fill=(255, 255, 255))
    th = int(im.height * tw / im.width)
    tile = im.resize((tw, th))
    ImageDraw.Draw(tile).text((6, 4), label, fill=(255, 255, 0))
    tiles.append(tile)

rows = (len(tiles) + cols - 1) // cols
th = max(t.height for t in tiles)
sheet = Image.new("RGB", (cols * tw, rows * th), (20, 20, 20))
for i, t in enumerate(tiles):
    sheet.paste(t, ((i % cols) * tw, (i // cols) * th))
sheet.save(out)
print("wrote", out, sheet.size, len(tiles), "tiles")
