// NIGHT44 builder V2 (design.n44.vis44): the view row's switch. On when design.n44.vis44 is enabled and NIGHT44 is not off
// (N44View.RowOn); the look44 reel forces it off / on to compare the same moments (Forced). Off = NIGHT43's look exactly.
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Vis44
    {
        /// <summary>The look44 reel's override (null: the config decides).</summary>
        public static bool? Forced;

        /// <summary>The reel's probe: roomEnter drops of the lesson line (T-69).</summary>
        public static int RoomDrops;

        static GameConfig lastCfg;
        static bool lastOn;

        /// <summary>vis44 on in this session (read once per config: the views ask every frame, no string built per frame).</summary>
        public static bool On(GameConfig cfg)
        {
            if (Forced.HasValue) return Forced.Value;
            if (!ReferenceEquals(cfg, lastCfg))
            {
                lastCfg = cfg;
                lastOn = N44View.RowOn(cfg, "vis44");
            }
            return lastOn;
        }
    }
}
