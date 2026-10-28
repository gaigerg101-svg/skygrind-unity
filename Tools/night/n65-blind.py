# NIGHT65 (the lead's; branch C): a blind copy of an iso65 picture (Tools/night/n65-iso.cjs) for the skygrind-spotter's A / B pair. iso65's
# header band (the level id, the room's name and its piece counts) and its footer line (the source plan's path) name which side is which, so
# both are cut away: the copy keeps the two isometric views, the two elevations and the legend, which iso65 draws from cameras fixed by the
# slot's rect and doors (the same for the old and the rebuilt room). It refuses a picture that is not iso65's 1600 x 1100.
#   python -I Tools/night/n65-blind.py <in.png> <out.png>
import sys
from PIL import Image

TOP, BOTTOM = 60, 1070  # iso65: the title and the pieces line end above y 60; the footer's source line starts below y 1070

def main(argv):
    if len(argv) != 3:
        print("usage: python -I Tools/night/n65-blind.py <in.png> <out.png>")
        return 1
    im = Image.open(argv[1])
    if im.size != (1600, 1100):
        print("n65-blind: %s is %dx%d, not iso65's 1600x1100: refused" % (argv[1], im.size[0], im.size[1]))
        return 1
    im.crop((0, TOP, 1600, BOTTOM)).save(argv[2])
    print("n65-blind: %s -> %s (rows %d..%d kept: the views, the elevations and the legend; the title and the source line cut)" % (argv[1], argv[2], TOP, BOTTOM))
    return 0

if __name__ == "__main__":
    sys.exit(main(sys.argv))
