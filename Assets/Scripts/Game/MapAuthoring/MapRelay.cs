// NIGHT15 U3-28.4 (builder P): a PROTOCOL relay (data.relaySpots: id, name, x, y, z; protocol.js) or, with secret set,
// the hidden fifth relay (data.secretRelay: one per map). The transform's position is the relay.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapRelay : MapMarker
    {
        public AuthoredRelay relay = new AuthoredRelay { id = "relay", name = "RELAY" };
        public bool secret;

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(relay.at), Quaternion.identity);
            MarkPlaced();
        }

        public AuthoredRelay Read()
        {
            AuthoredRelay r = relay;
            r.at = ReadPoint(relay.at);
            return r;
        }

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, false, secret);
    }
}
