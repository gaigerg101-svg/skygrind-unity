// NIGHT45 builder S1 (rows as data; NIGHT45.md card S1): the tape side of the generic rows record (Core/NightRows.cs, NightRowsTape).
// UNWIRED tonight: RunTape stays format 26 and nothing calls these. NIGHT46's G0 bumps the tape ONCE to format 27 (docs/NIGHT-ROWS.md):
//   RunTape:       public byte[] rows;   ToBytes, after w.VarI(n43):  TapeRows.Write(w, rows);
//                  FromBytes, after t.n43:  t.rows = version >= NightRowsTape.FirstTapeFormat ? TapeRows.Read(bytes, r) : null;
//   TapeRecorder:  tape.rows = sim.RowsTapeRecord();
//   TapeReplay:    sim.rowsOverride = TapeRows.Replay(sim.rowsRegistry, tape.rows);
// No later night bumps the tape for a row: the record names its rows, so a new row replays off on every older tape.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class TapeRows
    {
        /// <summary>The record, length-prefixed (VarU bytes, then the bytes; null or empty: a 0).</summary>
        public static void Write(TapeWriter w, byte[] record)
        {
            int n = record?.Length ?? 0;
            w.VarU((uint)n);
            if (n > 0) w.Bytes(record, n);
        }

        /// <summary>The record at the reader's position in the tape's bytes (null when its length is 0).</summary>
        public static byte[] Read(byte[] bytes, TapeReader r)
        {
            uint n = r.VarU();
            if (n == 0) return null;
            if (n > (uint)(bytes.Length - r.Position)) throw new TapeFormatException($"the rows record of {n} bytes runs past the tape's end at byte {r.Position}");
            var rec = new byte[n];
            Buffer.BlockCopy(bytes, r.Position, rec, 0, (int)n);
            r.Skip((int)n);
            return rec;
        }

        /// <summary>A tape's rows for its replay: null (formats 1-26, or a run before any row) = every row off; a row the tape ran ON that
        /// this build lacks is refused with a line (TapeFormatException), never a crash and never a silent other run.</summary>
        public static NightRowState Replay(NightRows registry, byte[] record)
        {
            try
            {
                return NightRowsTape.Decode(record, registry ?? NightRows.Empty);
            }
            catch (NightRowsFormatException e)
            {
                throw new TapeFormatException(e.Message);
            }
        }
    }
}
