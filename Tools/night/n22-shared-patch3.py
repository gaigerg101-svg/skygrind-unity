import io, sys
def patch(path, pairs):
    s = io.open(path, 'r', encoding='utf-8', newline='').read()
    crlf = chr(13) + chr(10) in s
    for old, new in pairs:
        if crlf:
            old = old.replace(chr(10), chr(13) + chr(10)); new = new.replace(chr(10), chr(13) + chr(10))
        if s.count(old) != 1:
            print('FAIL', path, s.count(old), old[:60]); sys.exit(1)
        s = s.replace(old, new)
    io.open(path, 'w', encoding='utf-8', newline='').write(s); print('ok', path)

patch('Tools/SimHarness/Harness.N21.cs', [
 ("        byte[] b6 = tape.ToBytes();\n        int version = BitConverter.ToInt32(b6, RunTape.Magic.Length);\n        bool v6 = version == 6 && RunTape.FromBytes(b6).n21 == 0;\n        Console.WriteLine($\"tape format {version} (want 6), n21 byte read back {RunTape.FromBytes(b6).n21}: {(v6 ? \"PASS\" : \"FAIL\")}\");\n",
  "        byte[] b7 = tape.ToBytes(); // NIGHT22: the current format is 7 (premise changed: the NIGHT21 checks below read format 6, derived from it)\n        byte[] b6 = AsFormat6(b7);\n        int version = BitConverter.ToInt32(b7, RunTape.Magic.Length);\n        bool v6 = version == RunTape.FormatVersion && RunTape.FromBytes(b6).n21 == 0 && BitConverter.ToInt32(b6, RunTape.Magic.Length) == 6;\n        Console.WriteLine($\"tape format {version} (want {RunTape.FormatVersion}), derived format 6 n21 byte read back {RunTape.FromBytes(b6).n21}: {(v6 ? \"PASS\" : \"FAIL\")}\");\n"),
])

patch('Assets/Tests/EditMode/Game/N21SharedTests.cs', [
 ("            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(6));\n            RunTape back = RunTape.FromBytes(b);\n            Assert.That(back.n21, Is.EqualTo(0b101101));",
  "            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion), \"NIGHT22: the current format is 7; format 6's n21 byte is read below\");\n            RunTape back = RunTape.FromBytes(b);\n            Assert.That(back.n21, Is.EqualTo(0b101101));"),
 ("        public static byte[] AsFormat5(byte[] b6)\n        {\n            var r = new TapeReader(",
  "        public static byte[] AsFormat5(byte[] b6)\n        {\n            b6 = N22SharedTests.AsFormat6(b6); // NIGHT22: the current format is 7; a format 6 tape passes through\n            var r = new TapeReader("),
])
print('done')
