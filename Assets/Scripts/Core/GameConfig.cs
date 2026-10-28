// The web game's whole config (PortExport/config.json, a JSON copy of skygrind/src/config.js) behind dotted-path
// lookups, so every gameplay number the port uses comes from the one source of truth (NIGHT7 rule 5). The movement
// sections stay typed (MovementConfig, read by PortData.LoadConfig with the same keys) because the Mover reads them
// every step; everything else is read by path once, at system construction, and kept in fields.
using System;
using System.Collections.Generic;
using System.IO;

namespace Skygrind.Core
{
    public sealed class GameConfig
    {
        public readonly Dictionary<string, object> root;
        public readonly MovementConfig movement;
        readonly Dictionary<string, object> cache = new Dictionary<string, object>();

        public GameConfig(Dictionary<string, object> json)
        {
            root = json;
            movement = PortData.LoadConfig(json);
        }

        /// <summary>
        /// config.json, plus the Unity design config (NIGHT8 U2-07) as the `design.` prefix: `designPath`, else
        /// Design/unity-config.json beside the config's folder (the repo root for PortExport/, StreamingAssets/ for the
        /// Player's copy). Throws if config.json has a `design` key of its own (the two must never collide).
        /// </summary>
        public static GameConfig Load(string path, string designPath = null)
        {
            var root = PortData.Obj(PortData.LoadJson(path));
            if (root.ContainsKey("design")) throw new InvalidDataException($"{path} has a 'design' key: the Unity design config owns that prefix (Design/unity-config.json)");
            designPath ??= DesignPath(Path.GetDirectoryName(Path.GetFullPath(path)));
            if (File.Exists(designPath)) root["design"] = PortData.Obj(PortData.Obj(PortData.LoadJson(designPath))["design"]);
            return new GameConfig(root);
        }

        /// <summary>Parse config.json's text and the design config's text (either may be null: a WebGL boot reads them by URL).</summary>
        public static GameConfig FromText(string configJson, string designJson)
        {
            var root = PortData.Obj(JsJson.Parse(configJson));
            if (root.ContainsKey("design")) throw new InvalidDataException("config.json has a 'design' key: the Unity design config owns that prefix");
            if (!string.IsNullOrEmpty(designJson)) root["design"] = PortData.Obj(PortData.Obj(JsJson.Parse(designJson))["design"]);
            return new GameConfig(root);
        }

        /// <summary>Design/unity-config.json next to a PortExport folder.</summary>
        public static string DesignPath(string portExportDir) => Path.Combine(Path.GetDirectoryName(portExportDir) ?? ".", "Design", "unity-config.json");

        /// <summary>A design flag (false when the design config is missing it, or missing: the game is then the web's).</summary>
        public bool Design(string path) => Has("design." + path) && Get("design." + path) is bool b && b;

        /// <summary>NIGHT63 D1 (the lead's hint: view rows read every frame without garbage): Design("nNN.&lt;row&gt;.enabled") without building or
        /// splitting a string. `fullPath` is that flag's whole dotted path ("design.nNN.&lt;row&gt;.enabled", made once by the caller), `nightKey`
        /// "nNN", `row` a name without a dot. The same answer as Design: Has (a live walk of the same four keys, here without Split) and Get
        /// (its cache, as before: the first call walks and caches, every later one is a dictionary hit).</summary>
        public bool DesignEnabledAt(string fullPath, string nightKey, string row)
        {
            if (!(root.TryGetValue("design", out object d) && d is Dictionary<string, object> dd && dd.TryGetValue(nightKey, out object n)
                  && n is Dictionary<string, object> nd && nd.TryGetValue(row, out object r) && r is Dictionary<string, object> rd && rd.ContainsKey("enabled"))) return false;
            return Get(fullPath) is bool b && b;
        }

        /// <summary>The value at a dotted path ("husk.hp", "zones.costs.armory"); throws with the path if it is missing.</summary>
        public object Get(string path)
        {
            if (cache.TryGetValue(path, out object hit)) return hit;
            object node = root;
            foreach (string key in path.Split('.'))
            {
                if (!(node is Dictionary<string, object> d) || !d.TryGetValue(key, out node))
                    throw new KeyNotFoundException($"config.json has no '{path}' (missing at '{key}')");
            }
            cache[path] = node;
            return node;
        }

        public bool Has(string path)
        {
            object node = root;
            foreach (string key in path.Split('.'))
            {
                if (!(node is Dictionary<string, object> d) || !d.TryGetValue(key, out node)) return false;
            }
            return true;
        }

        public double Num(string path) => PortData.Num(Get(path));

        public int Int(string path) => (int)Num(path);

        public bool Bool(string path) => (bool)Get(path);

        public string Str(string path) => (string)Get(path);

        public Dictionary<string, object> Obj(string path) => PortData.Obj(Get(path));

        public List<object> Arr(string path) => PortData.Arr(Get(path));

        public double[] Nums(string path) => Arr(path).ConvertAll(PortData.Num).ToArray();

        public List<string> Strs(string path) => Arr(path).ConvertAll(o => (string)o);

        /// <summary>A 0xRRGGBB color as r, g, b in 0..1 (config colors are integers, as in config.js).</summary>
        public (float r, float g, float b) Rgb(string path) => Hex((uint)Num(path));

        public static (float r, float g, float b) Hex(uint hex) => (((hex >> 16) & 255) / 255f, ((hex >> 8) & 255) / 255f, (hex & 255) / 255f);

        /// <summary>stepsFor (movementMath.js): a duration in whole fixed steps, at least 1.</summary>
        public int Steps(string path) => MovementMath.StepsFor(Num(path), 1 / movement.loop.simHz);

        public double Dt => 1 / movement.loop.simHz;

        /// <summary>The PortExport folder: the repo's copy in the Editor and tests, StreamingAssets in a Player.</summary>
        public static string PortExportDir(string editorProjectRoot, string streamingAssets, bool isEditor)
        {
            string repo = Path.Combine(editorProjectRoot, "PortExport");
            if (isEditor && File.Exists(Path.Combine(repo, "config.json"))) return repo;
            string shipped = Path.Combine(streamingAssets, "PortExport");
            if (File.Exists(Path.Combine(shipped, "config.json"))) return shipped;
            if (File.Exists(Path.Combine(repo, "config.json"))) return repo;
            throw new FileNotFoundException($"PortExport/config.json not found (looked in {repo} and {shipped})");
        }
    }
}
