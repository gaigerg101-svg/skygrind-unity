// Port of skygrind/src/core/events.js: a tiny pub/sub keyed by the web game's event names (kill, hit, damage,
// gateOpen, roundBegin, ...), so GG's AUDIO-HOOKS map carries over name for name. Payloads are small classes below
// with the web payload's field names. `Emitted` counts every name ever emitted (PORT-STATUS lists them).
// NIGHT8 U2-02: Emit no longer copies the handler list (the web copies a Set per emit). The web's semantics are kept: a
// handler removed during an emit still runs in that emit (the removal waits until the outermost emit of that name ends),
// and one added during an emit first runs on the next one. An int payload (an enemy id, a pad index) is boxed once
// per value (Boxes.Int) instead of once per emit. Some high-rate payloads are reused (HuskAttackEvent): read a payload
// inside the handler, don't keep it.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class EventBus
    {
        sealed class Slot
        {
            public readonly List<Action<object>> fns = new List<Action<object>>();
            public readonly List<Action<object>> pendingOff = new List<Action<object>>();
            public int depth; // emits of this name in progress (a handler may emit the same name again)
        }

        readonly Dictionary<string, Slot> handlers = new Dictionary<string, Slot>();
        public readonly Dictionary<string, int> Emitted = new Dictionary<string, int>(512); // sized for every name: a first emit doesn't grow it

        /// <summary>Subscribe; returns the unsubscribe call.</summary>
        public Action On(string type, Action<object> fn)
        {
            if (!handlers.TryGetValue(type, out Slot slot)) handlers[type] = slot = new Slot();
            slot.fns.Add(fn);
            return () => Off(type, fn);
        }

        public Action On<T>(string type, Action<T> fn) => On(type, o => fn((T)o));

        public void Off(string type, Action<object> fn)
        {
            if (!handlers.TryGetValue(type, out Slot slot)) return;
            if (slot.depth > 0) slot.pendingOff.Add(fn); // mid-emit: it still runs in this emit, as with the web's copy
            else slot.fns.Remove(fn);
        }

        public void Emit(string type, object payload = null)
        {
            Emitted[type] = Emitted.TryGetValue(type, out int n) ? n + 1 : 1;
            if (!handlers.TryGetValue(type, out Slot slot) || slot.fns.Count == 0) return;
            int count = slot.fns.Count; // handlers added during this emit run from the next one
            slot.depth++;
            try
            {
                for (int i = 0; i < count; i++) slot.fns[i](payload);
            }
            finally
            {
                if (--slot.depth == 0 && slot.pendingOff.Count > 0)
                {
                    foreach (Action<object> fn in slot.pendingOff) slot.fns.Remove(fn);
                    slot.pendingOff.Clear();
                }
            }
        }

        /// <summary>An int payload (an enemy id, a rank), boxed once per value.</summary>
        public void Emit(string type, int payload) => Emit(type, Boxes.Int(payload));
    }

    /// <summary>
    /// Boxed ints, one object per value (ids, ranks), so a payload or a candidate id doesn't box per use. 0..2047 are boxed
    /// up front (48 KB), so an enemy's first wind-up mid-fight allocates nothing; higher values box once, on first use.
    /// </summary>
    public static class Boxes
    {
        static object[] ints = Prefill(2048);

        static object[] Prefill(int n)
        {
            var a = new object[n];
            for (int i = 0; i < n; i++) a[i] = i;
            return a;
        }

        public static object Int(int v)
        {
            if (v < 0 || v > 1 << 20) return v;
            if (v >= ints.Length) Array.Resize(ref ints, Math.Max(v + 1, ints.Length * 2));
            return ints[v] ?? (ints[v] = v);
        }
    }

    public sealed class GateOpenEvent
    {
        public string zone;
        public string name;
        public double cost;
        public bool free;
    }

    public sealed class ZoneEnterEvent
    {
        public string id;
        public string name;
        public uint color;
    }
}
