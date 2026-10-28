# NIGHT26 builder B: write a .cs.meta (a fresh guid) beside each given file that has none.   python Tools/night/mkmeta.py file.cs ...
import sys, uuid, os
for f in sys.argv[1:]:
    m = f + '.meta'
    if not os.path.exists(m):
        open(m, 'w', newline='\n').write('fileFormatVersion: 2\nguid: ' + uuid.uuid4().hex)
        print('meta', m)
