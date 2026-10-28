// NIGHT58 builder V: the numbers n58-v-rows.cjs writes (only yellow56's NIGHT58 dim-yellow bounds; every other number untouched).
module.exports = {
  "n56.yellow56": [
    [["dimYellow58"], {
      _why: "NIGHT58 builder V (slate V2 yellow56 on foundry a5): the dim-yellow rule's value bounds raised so the last non-parry yellow (the foundry's lava-trench edge glow, hue 49, S 0.64, V 0.81; the column top plate, hue 48, S 0.56, V 0.82: Tools/night/n58-v-yellow.py on n57-r3 foundry a5-entry) is held under the meter's S 0.5, while the parry tells (the orbs' cores V 0.93-1.00, a husk's parry halo V 0.90) stay over vNone untouched. Read instead of dimYellow.vFull / vNone unless -skygrindOff58 (Rim58.Yellow58). Simulated on the n57-r3 a357 ON stills (n58-v-yellow.py --sim): frames over 0.5 % non-parry yellow 1 / 36 -> 0 / 36 (worst 0.85 % -> 0.36 %), the orbs' share unchanged (17.39 % on spire v5-mid both). The web has no such number (no parry colour rule).",
      vFull: 0.84,
      vFull_why: "value (sRGB max channel) at or under which the cap is whole: over the lava edge's 0.81 and the plate's 0.82 medians",
      vNone: 0.89,
      vNone_why: "value at or over which nothing moves: under the husk's parry halo (0.90) and the orbs' emissive cores (0.93-1.00)"
    }, "NIGHT58 builder V: the dim-yellow bounds (Game/Render/Cap57Pass.cs via Rim58.Yellow58)"]
  ]
};
