// NIGHT74 builder B (leftover 6, design.n74.kill74; Kill74.cs has the rule): THE ICEBREAKER's body root is handed to the death flash every frame the view draws it, so that on its death
// (bossDying) the solid meshes under it (the torso, head, legs, arms and fists) get their two white frames. One line; nothing here draws or changes a colour.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Brawl40View
    {
        partial void Kill74Brawl() => Kill74.Register("brawl40", bodyRoot);
    }
}
