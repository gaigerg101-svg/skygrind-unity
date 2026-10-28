// Builds the worm boss from the three Tripo exports (Assets/Art/WormBoss/{Head,Body,Tail}). Editor only; run it from the SKYGRIND menu while the
// Editor is idle. It touches no scene and no sim code. Two outputs:
//   1. Assets/Resources/WormBossParts.asset: the three pieces baked into ONE frame (+Z forward, +Y up, centred, no stray rotation) with their textures and
//      the two shaders: what the Player draws (Game/WyrmVisual26.cs). The head's jaw mask is baked into its vertex colour (red channel).
//   2. Assets/Art/WormBoss/WormBoss.prefab: head, a chain of body segments and the tail laid end to end along -Z from the baked meshes, for a look in the Editor.
//
// NIGHT26 builder V: the first version (NIGHT26 C1) read the head's axis from the mean vertex position and chained out of the jaws, and it laid the body along
// the head's axis though the Tripo body's tube runs along X; read from the FBX files' vertices (a script, not the Editor):
//   - every piece's mesh is Z-up raw with a Lcl Rotation of -90 about X on its node: upright after import, pivot at the piece's feet;
//   - HEAD: the tube comes in high at -Z and curves down to the jaws at +Z (the jaw teeth are the LOWEST vertices and sit on the +Z side): it faces +Z;
//   - BODY: a hoop (a ring in the Z-Y plane) 0.98 long along X and 0.53 across in Z: its tube runs along X, so it is turned 90 degrees to run along Z;
//   - TAIL: runs along Z, the thick curled end (many vertices) at -Z, the thin tip (few vertices) at +Z: it is turned to put the tip at -Z.
// The bake finds all three from the vertices (so a re-export with another axis still lands right) and prints what it decided. If the Console says the head's
// jaws are on the wrong side, or the prefab's head looks backward in the Editor, set FlipHead below and run the menu item again.
using System.Collections.Generic;
using System.IO;
using Skygrind.Game;
using UnityEditor;
using UnityEngine;

public static class WormBossSetup
{
    const string Root = "Assets/Art/WormBoss";
    const string MaterialFolder = Root + "/Materials";
    const string PrefabPath = Root + "/WormBoss.prefab";
    const string PartsPath = "Assets/Resources/WormBossParts.asset";

    // Unity-only render numbers: the web has no worm boss, so there is no config.json value for any of these.
    // Number of body segments between head and tail in the prefab (a render choice; the sim's piece count is design.n24.wyrm.body.pieces).
    const int BodySegments = 12;
    // The Tripo pieces are about one unit long; the prefab root is scaled up so the boss reads as a boss in the arena (the Player scales each piece onto its box).
    const float PrefabScale = 5f;
    // Prefab material look (the Player's shader reads only the albedo; these are for the Editor's look of the prefab).
    const float Metallic = 0.3f;
    const float Smoothness = 0.35f;
    // Overrides for the bake's own decisions: turn the head / tail a half turn about Y if the Console's note or the prefab shows them backward.
    const bool FlipHead = false;
    const bool FlipTail = false;
    // The jaw mask (vertex colour red): the head's front share of its length (smoothstep edges) times its lower share of its height (1 - smoothstep edges).
    const float JawFrontLo = 0.45f, JawFrontHi = 0.80f, JawLowLo = 0.25f, JawLowHi = 0.60f;

    [MenuItem("SKYGRIND/Worm Boss/Build Prefab")]
    public static void BuildPrefab()
    {
        Debug.Log("[WormBossSetup] " + Build());
    }

    sealed class Baked
    {
        public Vector3[] v, n;
        public Vector2[] uv;
        public int[] tris;
        public Color[] colors;
        public Vector3 size;
        public string note = "";
    }

    public static string Build()
    {
        AssetDatabase.Refresh();
        var models = new Dictionary<string, GameObject>();
        var mats = new Dictionary<string, Material>();
        var texs = new Dictionary<string, Texture2D>();
        foreach (string piece in new[] { "Head", "Body", "Tail" })
        {
            string folder = Root + "/" + piece;
            string fbx = FindFirst(folder, ".fbx");
            if (fbx == null) return "missing FBX in " + folder;

            var imp = (ModelImporter)AssetImporter.GetAtPath(fbx);
            if (imp == null) return "the Editor has not imported " + fbx + " yet (run again once the import has finished)";
            // Materials are built here for URP; the FBX's own materials would import as the built-in shader (pink in URP).
            imp.materialImportMode = ModelImporterMaterialImportMode.None;
            imp.importCameras = false;
            imp.importLights = false;
            imp.importAnimation = false;
            imp.isReadable = true;
            imp.meshCompression = ModelImporterMeshCompression.Off;
            imp.SaveAndReimport();

            models[piece] = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            string baseColor = FindFirst(folder, "_basecolor");
            texs[piece] = baseColor == null ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(baseColor);
            mats[piece] = BuildMaterial(piece, folder);
        }
        AssetDatabase.SaveAssets();

        // --- bake the three pieces into one frame ----------------------------------------------------------------------------
        Baked head = Bake("Head", models["Head"]);
        Baked body = Bake("Body", models["Body"]);
        Baked tail = Bake("Tail", models["Tail"]);
        string note = "head: " + head.note + "| body: " + body.note + "| tail: " + tail.note;

        // --- the data asset the Player loads -----------------------------------------------------------------------------------
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        var parts = AssetDatabase.LoadAssetAtPath<WormBossParts>(PartsPath);
        if (parts == null)
        {
            parts = ScriptableObject.CreateInstance<WormBossParts>();
            AssetDatabase.CreateAsset(parts, PartsPath);
        }
        else
        {
            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(PartsPath))
                if (o is Mesh) Object.DestroyImmediate(o, true); // the old baked meshes: re-baked below
        }
        parts.head = SubMesh(parts, head, "WormBoss Head");
        parts.body = SubMesh(parts, body, "WormBoss Body");
        parts.tail = SubMesh(parts, tail, "WormBoss Tail");
        parts.headTex = texs["Head"];
        parts.bodyTex = texs["Body"];
        parts.tailTex = texs["Tail"];
        parts.armour = Shader.Find("SKYGRIND/Wyrm26");
        parts.aura = Shader.Find("SKYGRIND/Wyrm26Aura");
        parts.headSize = head.size;
        parts.bodySize = body.size;
        parts.tailSize = tail.size;
        parts.bakeNote = note;
        EditorUtility.SetDirty(parts);

        // --- the prefab (a look in the Editor): head at the origin facing +Z, the chain running back along -Z ------------------------------
        var root = new GameObject("WormBoss");
        float cursor = head.size.z * 0.5f;
        PlaceMesh(root.transform, "Head", parts.head, mats["Head"], 0f);
        for (int i = 0; i < BodySegments; i++)
        {
            PlaceMesh(root.transform, "Body " + (i + 1), parts.body, mats["Body"], -(cursor + body.size.z * 0.5f));
            cursor += body.size.z;
        }
        PlaceMesh(root.transform, "Tail", parts.tail, mats["Tail"], -(cursor + tail.size.z * 0.5f));
        root.transform.localScale = Vector3.one * PrefabScale;
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        string missing = parts.Ready ? "" : " WARNING: the parts asset is not Ready (a shader or a mesh is missing)";
        return "saved " + PrefabPath + " and " + PartsPath + "; " + BodySegments + " body segments; sizes (w, h, len): head " + head.size.ToString("F3") + ", body " + body.size.ToString("F3")
            + ", tail " + tail.size.ToString("F3") + "; bake: " + note + missing;
    }

    // --- the bake ----------------------------------------------------------------------------------------------------------------------

    static Baked Bake(string piece, GameObject fbx)
    {
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        inst.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        inst.transform.localScale = Vector3.one;
        var b = new Baked();
        var verts = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris = new List<int>();
        foreach (MeshFilter mf in inst.GetComponentsInChildren<MeshFilter>())
        {
            Mesh m = mf.sharedMesh;
            if (m == null) continue;
            Matrix4x4 w = mf.transform.localToWorldMatrix;
            Matrix4x4 nw = w.inverse.transpose;
            int off = verts.Count;
            Vector3[] mv = m.vertices, mn = m.normals;
            Vector2[] muv = m.uv;
            for (int i = 0; i < mv.Length; i++)
            {
                verts.Add(w.MultiplyPoint3x4(mv[i]));
                normals.Add(mn.Length == mv.Length ? nw.MultiplyVector(mn[i]).normalized : Vector3.up);
                uvs.Add(muv.Length == mv.Length ? muv[i] : Vector2.zero);
            }
            bool flip = w.determinant < 0f;
            for (int s = 0; s < m.subMeshCount; s++)
            {
                int[] t = m.GetTriangles(s);
                for (int i = 0; i + 2 < t.Length; i += 3)
                {
                    tris.Add(off + t[i]);
                    tris.Add(off + t[flip ? i + 2 : i + 1]);
                    tris.Add(off + t[flip ? i + 1 : i + 2]);
                }
            }
        }
        Object.DestroyImmediate(inst);
        b.v = verts.ToArray();
        b.n = normals.ToArray();
        b.uv = uvs.ToArray();
        b.tris = tris.ToArray();
        if (b.v.Length == 0)
        {
            b.note = "NO MESH in the FBX; ";
            b.size = Vector3.one;
            return b;
        }

        Vector3 size = BoundsOf(b.v).size;
        b.note += "imported size (x, y, z) " + size.ToString("F3") + "; ";
        // the tube runs along X (the body: a hoop 0.98 long in X, 0.53 across in Z): a quarter turn about Y puts it along Z
        if (size.x > size.z)
        {
            Apply(b, Quaternion.Euler(0f, 90f, 0f));
            b.note += "tube ran along X: turned 90 about Y; ";
        }
        if (piece == "Head")
        {
            // the jaws are the lowest vertices: they must lie on the +Z side (the head faces +Z, the way the chain's pieces' forward points)
            float zLow = MeanZ(b.v, 0.08f, true), zHigh = MeanZ(b.v, 0.08f, false);
            bool jawsBack = zLow < zHigh;
            if (jawsBack ^ FlipHead)
            {
                Apply(b, Quaternion.Euler(0f, 180f, 0f));
                b.note += "jaws were on -Z (lowest verts z " + zLow.ToString("F2") + ", highest " + zHigh.ToString("F2") + "): turned 180; ";
            }
            else b.note += "jaws on +Z (lowest verts z " + zLow.ToString("F2") + ", highest " + zHigh.ToString("F2") + "): faces +Z" + (FlipHead ? " (FlipHead set)" : "") + "; ";
        }
        else if (piece == "Tail")
        {
            // the thin tip has the fewer vertices in its outer quarter; the tip belongs at -Z (away from the head)
            Bounds bb = BoundsOf(b.v);
            int plus = 0, minus = 0;
            foreach (Vector3 p in b.v)
            {
                if (p.z > bb.max.z - bb.size.z * 0.25f) plus++;
                if (p.z < bb.min.z + bb.size.z * 0.25f) minus++;
            }
            bool tipPlus = plus < minus;
            if (tipPlus ^ FlipTail)
            {
                Apply(b, Quaternion.Euler(0f, 180f, 0f));
                b.note += "tip was at +Z (" + plus + " vs " + minus + " verts in the end quarters): turned 180; ";
            }
            else b.note += "tip at -Z (" + plus + " vs " + minus + " verts in the end quarters)" + (FlipTail ? " (FlipTail set)" : "") + "; ";
        }

        // centre: x and z on the bounds' middle, y on the vertices' mean (the tube's middle, not the spikes' or the feet)
        Bounds fb = BoundsOf(b.v);
        float meanY = 0f;
        foreach (Vector3 p in b.v) meanY += p.y;
        meanY /= b.v.Length;
        var shift = new Vector3(-fb.center.x, -meanY, -fb.center.z);
        for (int i = 0; i < b.v.Length; i++) b.v[i] += shift;
        b.size = BoundsOf(b.v).size;
        b.note += "baked size (w, h, len) " + b.size.ToString("F3") + "; ";

        // the head's jaw mask in the vertex colour (red); every other piece: white (the shader's _Jaw is 0 for them)
        b.colors = new Color[b.v.Length];
        Bounds cb = BoundsOf(b.v);
        for (int i = 0; i < b.v.Length; i++)
        {
            float jaw = 0f;
            if (piece == "Head")
            {
                float zN = (b.v[i].z - cb.min.z) / Mathf.Max(1e-5f, cb.size.z), yN = (b.v[i].y - cb.min.y) / Mathf.Max(1e-5f, cb.size.y);
                jaw = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(JawFrontLo, JawFrontHi, zN)) * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(JawLowLo, JawLowHi, yN)));
            }
            b.colors[i] = new Color(jaw, 1f, 1f, 1f);
        }
        return b;
    }

    static void Apply(Baked b, Quaternion q)
    {
        for (int i = 0; i < b.v.Length; i++)
        {
            b.v[i] = q * b.v[i];
            b.n[i] = q * b.n[i];
        }
    }

    static Bounds BoundsOf(Vector3[] v)
    {
        var bb = new Bounds(v[0], Vector3.zero);
        for (int i = 1; i < v.Length; i++) bb.Encapsulate(v[i]);
        return bb;
    }

    // the mean z of the lowest (or highest) `share` of the vertices by y
    static float MeanZ(Vector3[] v, float share, bool lowest)
    {
        var idx = new List<Vector3>(v);
        idx.Sort((a, c) => lowest ? a.y.CompareTo(c.y) : c.y.CompareTo(a.y));
        int n = Mathf.Max(1, (int)(idx.Count * share));
        float sum = 0f;
        for (int i = 0; i < n; i++) sum += idx[i].z;
        return sum / n;
    }

    static Mesh SubMesh(WormBossParts owner, Baked b, string name)
    {
        var m = new Mesh { name = name, indexFormat = b.v.Length > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
        m.vertices = b.v;
        m.normals = b.n;
        m.uv = b.uv;
        m.colors = b.colors;
        m.triangles = b.tris;
        m.RecalculateBounds();
        AssetDatabase.AddObjectToAsset(m, owner);
        return m;
    }

    static void PlaceMesh(Transform parent, string name, Mesh mesh, Material material, float z)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, 0f, z);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    static Material BuildMaterial(string piece, string folder)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder(Root, "Materials");
        string path = MaterialFolder + "/WormBoss_" + piece + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }

        string baseColor = FindFirst(folder, "_basecolor");
        if (baseColor != null) m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(baseColor));

        string normal = FindFirst(folder, "_normal");
        if (normal != null)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(normal);
            if (ti.textureType != TextureImporterType.NormalMap)
            {
                ti.textureType = TextureImporterType.NormalMap;
                ti.SaveAndReimport();
            }
            m.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normal));
            m.EnableKeyword("_NORMALMAP");
        }

        m.SetFloat("_Metallic", Metallic);
        m.SetFloat("_Smoothness", Smoothness);
        EditorUtility.SetDirty(m);
        return m;
    }

    // First project file under the folder whose name contains the token (case-insensitive), skipping .meta files.
    static string FindFirst(string folder, string token)
    {
        if (!Directory.Exists(folder)) return null;
        foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(file).ToLowerInvariant();
            if (name.EndsWith(".meta") || !name.Contains(token)) continue;
            if (token == ".fbx" && !name.EndsWith(".fbx")) continue;
            return file.Replace('\\', '/');
        }
        return null;
    }
}
