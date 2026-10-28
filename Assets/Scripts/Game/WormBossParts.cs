// NIGHT26 builder V: the worm boss's Tripo pieces as the Player can load them (Resources.Load<WormBossParts>("WormBossParts")): the three meshes
// baked by Assets/Editor/WormBossSetup.cs (menu SKYGRIND > Worm Boss > Build Prefab) into one frame (+Z forward, +Y up, centred, the units the FBX
// imported in), their albedo textures, and the two shaders (a serialized reference keeps them in the build; Shader.Find would not find a shader that is
// not in Always Included Shaders). WyrmVisual26 draws from it. A missing asset means the old blocky worm is drawn (the row's fallback).
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class WormBossParts : ScriptableObject
    {
        public const string Resource = "WormBossParts";

        public Mesh head, body, tail;
        public Texture2D headTex, bodyTex, tailTex;
        public Shader armour, aura;
        /// <summary>The baked meshes' bounds sizes: x the width, y the height, z the length (the way it runs along the worm).</summary>
        public Vector3 headSize, bodySize, tailSize;
        /// <summary>What the bake decided (which way the head faces, how the body was turned): shown in the Console by the menu item.</summary>
        public string bakeNote;

        public bool Ready => head != null && body != null && tail != null && armour != null && aura != null
            && headSize.z > 1e-4f && bodySize.z > 1e-4f && tailSize.z > 1e-4f;
    }
}
