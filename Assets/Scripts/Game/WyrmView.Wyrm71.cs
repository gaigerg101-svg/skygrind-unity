// NIGHT71 builder B (slate B3, design.n71.wyrm71; the row and its parameters: Wyrm71.cs). THE KILN WYRM's half: WyrmView.N63V.cs DrawMotion63
// asks Motion71 for the frame's motion instead of calling the rig with bossMotion63's own parameters, and Params71 for the body pieces'
// parameters. Row on: design.n71.wyrm71's parameters (read once per config); off: bossMotion63's (p63, exactly NIGHT63's path). The rig is the
// same object either way, so its springs, flinch and breath clocks run on through a same-run toggle.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class WyrmView
    {
        BossMotionParams63 p71;
        GameConfig p71For;
        bool on71;
        int said71 = -1; // the row's state last printed (-1: not yet): the proof line on the first drawn frame and at each change

        /// <summary>The rig's frame with the row's parameters (on) or bossMotion63's (off). Read every frame.</summary>
        Motion63 Motion71(Vector3 rigid, Vector3 player, float size)
        {
            on71 = game != null && Wyrm71.On(game.cfg);
            int now71 = on71 ? 1 : 0;
            if (now71 != said71) { said71 = now71; Debug.Log(Wyrm71.Line(game != null ? game.cfg : null, on71)); }
            if (!on71) return rig63.Frame(game, ref p63, ref p63For, rigid, player, size, out BossAttack63 _);
            if (!ReferenceEquals(p71For, game.cfg)) { p71 = Wyrm71.Params(game.cfg); p71For = game.cfg; }
            return rig63.Frame(game, ref p71, ref p71For, rigid, player, size, out BossAttack63 _);
        }

        /// <summary>The parameters the frame was drawn with (after Motion71).</summary>
        BossMotionParams63 Params71 => on71 && p71 != null ? p71 : p63;
    }
}
