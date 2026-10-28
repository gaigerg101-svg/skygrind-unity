// NIGHT20 U3-34.9 A1 (builder EN): the sim's side of the web's telegraph language (src/fx/telegraph.js: "shared by every
// boss"), so a Core boss calls `kit.telegraph` as bulwark.js does. The signatures are Game/Telegraph20.cs's (builder V's
// port: circle / line / lane / hazard / flash / threat, move, hot, cancel, progress, clear), in web space (Core's
// coordinates). The sim never reads a telegraph back except by the id it was given (move / cancel), so the null one
// (the harness, the tests, a run with no view) changes nothing a run does. Colours: 0 is TELEGRAPH.danger (the view's
// red-orange); any other value is a hex the view draws as it is (the module's other colours live in the view).
namespace Skygrind.Core
{
    public interface ITelegraph
    {
        /// <summary>A red-orange circle filling over `time` s (hits when full); hold: s it stays full (-1: config's).</summary>
        int Circle(double x, double y, double z, double r, double time, uint hex = 0, double hold = -1);

        /// <summary>A red-orange line (a charge path, a sweep) from (x, z) along (dx, dz) for `length` m, `width` wide, filling.</summary>
        int Line(double x, double y, double z, double dx, double dz, double length, double width, double time, uint hex = 0, double hold = -1);

        /// <summary>A blue outlined safe lane for `life` s.</summary>
        int Lane(double x, double y, double z, double dx, double dz, double length, double width, double life);

        /// <summary>A green-white hazard circle for `warn` s, then hot for `life` s.</summary>
        int Hazard(double x, double y, double z, double r, double warn, double life);

        /// <summary>A white flash at a point (the hit lands next frame).</summary>
        void Flash(double x, double y, double z, double radius = 1);

        /// <summary>An off-screen attack from a world point (a screen-edge arrow in its colour for `time` s; -1: config's).</summary>
        void Threat(double x, double y, double z, uint hex = 0, double time = -1);

        /// <summary>Move a shape (a charge line that follows its boss).</summary>
        void Move(int id, double x, double y, double z);

        /// <summary>Move a line and turn it along (dx, dz).</summary>
        void Move(int id, double x, double y, double z, double dx, double dz);

        /// <summary>Is a hazard hot yet?</summary>
        bool Hot(int id);

        void Cancel(int id);

        /// <summary>How full a shape is, 0..1 (-1: no such shape).</summary>
        double Progress(int id);

        /// <summary>Every shape gone (bossKit.clear, bossKit.down).</summary>
        void Clear();
    }

    /// <summary>No view: ids counted, nothing drawn (the harness, the tests).</summary>
    public sealed class NullTelegraph : ITelegraph
    {
        int next;

        public int Circle(double x, double y, double z, double r, double time, uint hex = 0, double hold = -1) => ++next;
        public int Line(double x, double y, double z, double dx, double dz, double length, double width, double time, uint hex = 0, double hold = -1) => ++next;
        public int Lane(double x, double y, double z, double dx, double dz, double length, double width, double life) => ++next;
        public int Hazard(double x, double y, double z, double r, double warn, double life) => ++next;
        public void Flash(double x, double y, double z, double radius = 1) { }
        public void Threat(double x, double y, double z, uint hex = 0, double time = -1) { }
        public void Move(int id, double x, double y, double z) { }
        public void Move(int id, double x, double y, double z, double dx, double dz) { }
        public bool Hot(int id) => false;
        public void Cancel(int id) { }
        public double Progress(int id) => -1;
        public void Clear() { }
    }
}
