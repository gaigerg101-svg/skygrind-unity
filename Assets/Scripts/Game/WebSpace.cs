// Core runs in the web game's coordinates (three.js: right-handed, yaw 0 looks toward −z). Unity is left-handed, so
// the view flips z: web (x, y, z) is Unity (x, y, −z). Web −z (forward at yaw 0) becomes Unity +z, web right stays
// +x, and the world is not mirrored. Every crossing between Core and Unity goes through here.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class WebSpace
    {
        public static Vector3 ToUnity(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z);

        public static Vec3 FromUnity(Vector3 v) => new Vec3(v.x, v.y, -v.z);

        /// <summary>
        /// The Unity camera rotation for a web look. three.js 'YXZ' (yaw, then pitch, then roll) matches
        /// Quaternion.Euler's order; the z flip negates yaw and pitch, and three.js already rolls by −rollRight.
        /// </summary>
        public static Quaternion LookRotation(double yaw, double pitch, double rollRightDeg) =>
            Quaternion.Euler((float)(-pitch * Mathf.Rad2Deg), (float)(-yaw * Mathf.Rad2Deg), (float)-rollRightDeg);
    }
}
