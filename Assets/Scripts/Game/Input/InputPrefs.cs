// NIGHT13 U3-26.7 (agent I): where the player's key bindings and pad remap live in the Player: PlayerPrefs under the web's
// localStorage keys (bindings.js STORE_KEY "skygrind.bindings.v1"; main.js :188 "skygrind.padLayout.v1"), the same JSON
// (bindings.js createBindings / main.js padStore). The bindings store saves itself on every Set / Reset through its
// persist callback, as the web's save() does; a pad remap is saved by the caller (main.js padBinds.set / reset). If the
// settings move to a file beside the profile (U3-26.8), only this file changes.
// NIGHT19 U3-33.3 (builder P): with design.pad.n19 on (PadReader.V2) the pad layout lives under skygrind.padLayout.v2
// ({"version":2,"layout":{..}}, GamepadMath.PadLayoutV2Json); the first load with no v2 migrates a stored v1
// (GamepadMath.MigrateV1) and writes it as v2, leaving the v1 string as it was (a rollback: the block off reads it again).
// NIGHT20 U3-34.4 (builder P): with design.pad.n20 on (PadReader.V3) the layout lives under skygrind.padLayout.v3
// ({"version":3,..}); the first load with no v3 migrates the v2 store (GamepadMath.MigrateV2) and writes it as v3.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class InputPrefs
    {
        /// <summary>createBindings: config.input.bindings' defaults, the saved map over them, saved again on every change.</summary>
        public static Bindings LoadBindings(GameConfig cfg)
        {
            var b = new Bindings(cfg);
            b.Load(PlayerPrefs.HasKey(Bindings.StorageKey) ? PlayerPrefs.GetString(Bindings.StorageKey) : null);
            b.persist = SaveBindings;
            return b;
        }

        static void SaveBindings(string json)
        {
            PlayerPrefs.SetString(Bindings.StorageKey, json);
            PlayerPrefs.Save();
        }

        static string Stored(string key) => PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;

        /// <summary>main.js padStore.load: the stored pad layout, or the default when missing, corrupt or short of a needed action.</summary>
        public static PadLayout LoadPadLayout() => LoadPadLayout(false);

        /// <summary>
        /// NIGHT19: the layout for the block: off, the v1 store (NIGHT18's); on, the v2 store, a stored v1 migrated and
        /// written as v2 the first time (the v1 string kept).
        /// </summary>
        public static PadLayout LoadPadLayout(bool v2) => LoadPadLayout(v2 ? 2 : 1);

        /// <summary>
        /// NIGHT20 U3-34.4: the layout for a version (PadReader.LayoutVersion): 1 the v1 store (NIGHT18's); 2 the v2 store, a
        /// stored v1 migrated and written as v2 the first time; 3 (design.pad.n20) the v3 store, a stored v2 (or v1) migrated
        /// and written as v3 the first time. The older strings are kept (a rollback: the block off reads them again).
        /// </summary>
        public static PadLayout LoadPadLayout(int version)
        {
            if (version <= 1) return GamepadMath.ReadPadLayout(Stored(UnityTuning.PadLayoutKey));
            bool migrated;
            PadLayout layout = version >= 5
                ? GamepadMath.LoadPadLayoutV5(Stored(UnityTuning.PadLayoutKeyV5), Stored(UnityTuning.PadLayoutKeyV4), Stored(UnityTuning.PadLayoutKeyV3), Stored(UnityTuning.PadLayoutKeyV2), Stored(UnityTuning.PadLayoutKey), out migrated)
                : version == 4
                ? GamepadMath.LoadPadLayoutV4(Stored(UnityTuning.PadLayoutKeyV4), Stored(UnityTuning.PadLayoutKeyV3), Stored(UnityTuning.PadLayoutKeyV2), Stored(UnityTuning.PadLayoutKey), out migrated)
                : version == 2
                ? GamepadMath.LoadPadLayoutV2(Stored(UnityTuning.PadLayoutKeyV2), Stored(UnityTuning.PadLayoutKey), out migrated)
                : GamepadMath.LoadPadLayoutV3(Stored(UnityTuning.PadLayoutKeyV3), Stored(UnityTuning.PadLayoutKeyV2), Stored(UnityTuning.PadLayoutKey), out migrated);
            if (migrated) SavePadLayout(layout, version);
            return layout;
        }

        /// <summary>main.js padStore.save (CONTROLS > GAMEPAD's remap and RESET PAD).</summary>
        public static void SavePadLayout(PadLayout layout) => SavePadLayout(layout, 1);

        /// <summary>NIGHT19: the remap saved to the block's store (v2: the version field's JSON; v1: the web's).</summary>
        public static void SavePadLayout(PadLayout layout, bool v2) => SavePadLayout(layout, v2 ? 2 : 1);

        /// <summary>NIGHT20 / NIGHT24: the remap saved to the version's store (4, 3 and 2: the version field's JSON; 1: the web's).</summary>
        public static void SavePadLayout(PadLayout layout, int version)
        {
            string key = version >= 5 ? UnityTuning.PadLayoutKeyV5 : version == 4 ? UnityTuning.PadLayoutKeyV4 : version == 3 ? UnityTuning.PadLayoutKeyV3 : version == 2 ? UnityTuning.PadLayoutKeyV2 : UnityTuning.PadLayoutKey;
            string json = version >= 5 ? GamepadMath.PadLayoutV5Json(layout) : version == 4 ? GamepadMath.PadLayoutV4Json(layout) : version == 3 ? GamepadMath.PadLayoutV3Json(layout) : version == 2 ? GamepadMath.PadLayoutV2Json(layout) : layout.ToJson();
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }
    }
}
