// PG3 tripo74 (GG 2026-10-28, the AME 294 PG3 assignment: 3D models made in Tripo replace the visuals of the KESTREL, the husk and the other enemies, THE WICK and the
// other bosses; CLAUDE.md rule 7's pill-language-only rule is lifted for these models): the Unity-only render numbers of Game/TripoModels.cs and its hooks (Viewmodel.Tripo.cs,
// Enemy43View.Tripo.cs, Wick37View.Tripo.cs, Brawl40View.Tripo.cs, Crown37View.Tripo.cs, Loom37View.Tripo.cs). No gameplay number lives here: a model is view only (no collider,
// no hit box moves; the sim's shapes are the truth). The models are FBX files at Assets/Resources/Tripo/<id>.fbx (kestrel, thunderclap, husk, swarmer, spitter, charger,
// warden, drone, wick, icebreaker, crown, loom); a missing file draws nothing and changes nothing.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The view row that switches every Tripo model (design.n74.tripo74.enabled, NightRows.ViewOn; absent or off: the pills and the kit draw as before).</summary>
        public const string TripoRow = "tripo74";

        /// <summary>The Resources folder the models are loaded from (Resources.Load("Tripo/kestrel")).</summary>
        public const string TripoFolder = "Tripo/";

        // --- orientation: yaw about each model's own up axis (deg), applied before it is fitted; 0 = the model's front already faces +Z (the way a body, a boss or a gun faces) ---
        // the KESTREL and THUNDERCLAP files point their muzzle down -Z (pg3-tripo kill12 frame: the KESTREL's muzzle ring faced the camera)
        // the gauntlet's and the torch's reference images were framed the same way (front to the left); the parts are round, so their yaw does not show
        public const float TripoYawGauntlet = 180f, TripoYawTorch = 180f, TripoYawKestrelCylinder = 0f, TripoYawThunderclapPump = 0f;
        public const float TripoYawKestrel = 180f, TripoYawThunderclap = 180f, TripoYawHusk = 0f, TripoYawSwarmer = 0f, TripoYawSpitter = 0f, TripoYawCharger = 0f,
            TripoYawWarden = 0f, TripoYawDrone = 0f, TripoYawWick = 0f, TripoYawIcebreaker = 0f, TripoYawCrown = 0f, TripoYawLoom = 0f;

        /// <summary>The yaw offset (deg) of a model by its id (0 for an id with no entry).</summary>
        public static float TripoYawDeg(string id)
        {
            switch (id)
            {
                case "kestrel": return TripoYawKestrel;
                case "thunderclap": return TripoYawThunderclap;
                case "gauntlet": return TripoYawGauntlet;
                case "torch": return TripoYawTorch;
                case "kestrel-cylinder": return TripoYawKestrelCylinder;
                case "thunderclap-pump": return TripoYawThunderclapPump;
                case "husk": return TripoYawHusk;
                case "swarmer": return TripoYawSwarmer;
                case "spitter": return TripoYawSpitter;
                case "charger": return TripoYawCharger;
                case "warden": return TripoYawWarden;
                case "drone": return TripoYawDrone;
                case "wick": return TripoYawWick;
                case "icebreaker": return TripoYawIcebreaker;
                case "crown": return TripoYawCrown;
                case "loom": return TripoYawLoom;
                default: return 0f;
            }
        }

        // --- the cues the model does not replace (rule 1 of docs/PRINCIPLES.md: nothing the player had is lost) ---

        /// <summary>true: the pill parts that carry a gameplay cue stay drawn beside the model (the DANGER / PARRY / WEAK parts of Enemy40Parts, the charger's back plate, the spitter's sac, the
        /// warden's shield and arm, the drone's core; the bosses' glows are never hidden); false: every pill part hides and only the model draws.</summary>
        public static readonly bool TripoKeepCues = true;

        /// <summary>Is an Enemy40Parts role a cue (2 DANGER, 3 PARRY, 4 WEAK: UnityTuning.Models43.cs's roles) that stays drawn while a model draws?</summary>
        public static bool TripoCueRole(int role) => TripoKeepCues && (role == 2 || role == 3 || role == 4);

        /// <summary>Bit i set: the view's renderer i (the order of its `rends`) stays drawn while a model draws (a cue; 0 hides them all). spitter: the sac (swells and glows in the wind-up);
        /// charger: the back plate (the weak point); warden: the shield and the arm (the shield's state and the swing's tell); drone: the core (the charge-up).</summary>
        public static int TripoKeepPills(string kind)
        {
            if (!TripoKeepCues) return 0;
            switch (kind)
            {
                case "spitter": return 1 << 1;
                case "charger": return 1 << 2;
                case "warden": return (1 << 1) | (1 << 2);
                case "drone": return 1 << 1;
                default: return 0;
            }
        }

        // --- the glow (GG 2026-10-28: the models "look very desaturated"; Tools/pg3/tripo-tex.py grades the base maps and makes <id>_emission from the cyan / amber accents) ---
        /// <summary>The material every model draws with (URP Lit, emission and normal-map variants kept in the build); its maps are set per model.</summary>
        public const string TripoGlowMaterial = "Tripo/TripoGlow";
        /// <summary>The accents' emission at rest (x the emission map) on every model.</summary>
        public const float TripoGlowRest = 1.4f;
        /// <summary>The gun in hand's accents while it charges: + this per lit quarter (gunglow50's steps, which hide while a model shows: they sat on the pill barrel).</summary>
        public const float TripoGlowPerStep = 1.6f;
        /// <summary>The accents at the full-charge flash (gunglow50's flash frames).</summary>
        public const float TripoGlowFlash = 9f;

        // --- the enemy models' motion (GG 2026-10-28: "enemies need walking or better animations for walking and attacking"; the models are static meshes, so the motion is procedural,
        //     on the model's holder under the body's root, from the root's own measured ground speed and the view's wind-up flag; view only) ---
        /// <summary>Ground speed (m/s) at which the walk is at full size.</summary>
        public const float TripoWalkFullSpeed = 4f;
        /// <summary>Metres per step (the gait's phase advances half a cycle per step).</summary>
        public const float TripoStrideM = 0.9f;
        /// <summary>The step's bob as a share of the body's height, the side-to-side waddle (deg) and the forward lean at full walk (deg).</summary>
        public const float TripoBobShare = 0.035f, TripoWaddleDeg = 7f, TripoLeanDeg = 9f;
        /// <summary>The wind-up's rear-back (deg, at a full wind-up), how fast it builds (per s), and the strike's lunge: pitch (deg), reach (share of height), decay (per s).</summary>
        public const float TripoWindupBackDeg = 16f, TripoWindupRate = 3f, TripoStrikeDeg = 26f, TripoStrikeReach = 0.14f, TripoStrikeDecay = 5f;
        /// <summary>The drone's hover: bob (share of its size) and rate (Hz), and its bank into its motion (deg at full speed).</summary>
        public const float TripoHoverShare = 0.06f, TripoHoverHz = 0.7f, TripoDroneBankDeg = 14f;
        /// <summary>A measured speed above this (m/s) is a teleport (a spawn, a pool reuse): no walk that frame.</summary>
        public const float TripoTeleportSpeed = 40f;

        // --- the hit flash on a model: its renderers' base colour x (1 + this x the flash share) through a property block (the pill's own flash is the Enemy shader's _Flash, which a Lit material has not) ---
        public const float TripoFlashBoost = 4f;

        // --- sizes (m) ---

        /// <summary>The model's length along the gun's barrel as a share of the pill gun's (the kit body mesh's z extent): 1 = the same length, the muzzle end on the same z.</summary>
        public const float TripoGunLengthScale = 1f;

        /// <summary>THE WICK's model height as a share of the cage's height (design.n37.wick37.boss.cageHeight; the cage plates, rings, spire and keel are what it replaces): 1 = the cage's own height.</summary>
        public const float TripoWickHeightScale = 1f;

        /// <summary>true: THE WICK's model turns on the spot to face the player (the pill cage is round, a model has a front).</summary>
        public static readonly bool TripoWickFacesPlayer = true;

        /// <summary>THE ICEBREAKER's model height (m): the pill brute's drawn height (Brawl40View.BuildBody: legs 1.2 m, a hunched torso and head to about 3.6 m, the sim body's own height).</summary>
        public const float TripoIcebreakerHeight = 3.6f;

        /// <summary>THE CROWN's model diameter as a share of the pill ring's (2 x the ring radius, the sim's): 1 = the ring's own size.</summary>
        public const float TripoCrownScale = 1f;

        /// <summary>THE LOOM's model length as a share of the pill body's (Loom37BodyL, 8 m): 1 = the body's own length.</summary>
        public const float TripoLoomLengthScale = 1f;
    }
}
