// NIGHT14 U3-27.8 (agent T): THE TAPE's bytes. A growable little-endian byte writer (grown by doubling, so a run's recording
// allocates a handful of times, never per step: PORT-RULES 6) and its reader, which throws TapeFormatException on a short
// read, so a truncated or foreign file is refused with a line, never an IndexOutOfRangeException. Doubles are written as
// their raw 64 bits (bit-exact, -0 and NaN payloads kept); integers as LEB128 varints (zigzag for signed); strings as a
// varint count of UTF-16 code units, each a varint (no Encoding buffer: the tape's strings are ASCII and rare).
// No UnityEngine use (Tools/SimHarness compiles this folder).
using System;
using System.Text;

namespace Skygrind.Game
{
    public sealed class TapeWriter
    {
        public byte[] data;
        public int length;

        /// <summary>Times the buffer grew (each one copy; the recorder starts big enough for minutes of play).</summary>
        public int grows;

        public TapeWriter(int capacity)
        {
            data = new byte[Math.Max(16, capacity)];
        }

        void Need(int n)
        {
            if (length + n <= data.Length) return;
            int cap = data.Length * 2;
            while (cap < length + n) cap *= 2;
            Array.Resize(ref data, cap);
            grows++;
        }

        public void Clear() => length = 0;

        public void Byte(byte b)
        {
            Need(1);
            data[length++] = b;
        }

        public void Bytes(byte[] src, int count)
        {
            Need(count);
            Buffer.BlockCopy(src, 0, data, length, count);
            length += count;
        }

        public void U16(int v)
        {
            Need(2);
            data[length++] = (byte)v;
            data[length++] = (byte)(v >> 8);
        }

        public void U32(uint v)
        {
            Need(4);
            for (int i = 0; i < 4; i++) data[length++] = (byte)(v >> (8 * i));
        }

        public void U64(ulong v)
        {
            Need(8);
            for (int i = 0; i < 8; i++) data[length++] = (byte)(v >> (8 * i));
        }

        public void D(double v) => U64(unchecked((ulong)BitConverter.DoubleToInt64Bits(v)));

        public void VarU(uint v)
        {
            while (v >= 0x80)
            {
                Byte((byte)(v | 0x80));
                v >>= 7;
            }
            Byte((byte)v);
        }

        public void VarI(int v) => VarU(unchecked((uint)((v << 1) ^ (v >> 31))));

        public void Str(string s)
        {
            if (s == null)
            {
                VarU(0);
                return;
            }
            VarU((uint)s.Length + 1); // 0: null
            for (int i = 0; i < s.Length; i++) VarU(s[i]);
        }
    }

    public sealed class TapeReader
    {
        readonly byte[] data;
        int pos;
        readonly int end;

        public TapeReader(byte[] data, int start, int end)
        {
            this.data = data;
            pos = start;
            this.end = Math.Min(end, data.Length);
        }

        public int Position => pos;
        public bool More => pos < end;

        void Need(int n)
        {
            if (pos + n > end) throw new TapeFormatException($"the tape is truncated (byte {pos} of {end}, {n} more wanted)");
        }

        public byte Byte()
        {
            Need(1);
            return data[pos++];
        }

        public int U16()
        {
            Need(2);
            int v = data[pos] | (data[pos + 1] << 8);
            pos += 2;
            return v;
        }

        public uint U32()
        {
            Need(4);
            uint v = 0;
            for (int i = 0; i < 4; i++) v |= (uint)data[pos++] << (8 * i);
            return v;
        }

        public ulong U64()
        {
            Need(8);
            ulong v = 0;
            for (int i = 0; i < 8; i++) v |= (ulong)data[pos++] << (8 * i);
            return v;
        }

        public double D() => BitConverter.Int64BitsToDouble(unchecked((long)U64()));

        public uint VarU()
        {
            uint v = 0;
            for (int shift = 0; shift < 35; shift += 7)
            {
                byte b = Byte();
                v |= (uint)(b & 0x7f) << shift;
                if ((b & 0x80) == 0) return v;
            }
            throw new TapeFormatException($"a varint runs past 5 bytes at byte {pos}");
        }

        public int VarI()
        {
            uint u = VarU();
            return unchecked((int)(u >> 1) ^ -(int)(u & 1));
        }

        public string Str()
        {
            uint n = VarU();
            if (n == 0) return null;
            if (n - 1 > (uint)(end - pos)) throw new TapeFormatException($"a string of {n - 1} chars runs past the tape's end at byte {pos}");
            var sb = new StringBuilder((int)(n - 1));
            for (uint i = 0; i < n - 1; i++) sb.Append((char)VarU());
            return sb.ToString();
        }

        public void Skip(int n)
        {
            Need(n);
            pos += n;
        }
    }
}
