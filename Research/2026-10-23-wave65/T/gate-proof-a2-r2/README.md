# GATE pack: a2, round r2

Written by `bash Tools/night/n65-gate-pack.sh a2 r2` at sha d602157a (from r2/gen-latest.txt's header), 2026-10-23 23:32 UTC. The round's plan: assembled at d602157a (`gen-n65-foundry.cjs --slots=latest` on the room files git shows there, in a scratch tree deleted after); the level for the --verbs65 skip: the scratch level of that assembly (levelgen + the finish inside the scratch tree); the logs: ../../../Logs/n65. Nothing in this folder names which picture is the old room.

- **table.md**: the header and the `| a2 ` row of ../../../Logs/n65/r2/table.md
- **verbs65-skip.txt**: `dotnet run --project Tools/SimHarness -c Release -- n65verb skip --level <the scratch level of the round's assembly at d602157a> --room a2 --seeds 5 --verbs65` (the round's row's verb65: pierce)
- **verbs.md**: the `n65verb` header, verdict and seed lines naming a2 from r2/verbs*.md / .txt and Research/2026-10-23-wave65/T/verb-*-red.txt
- **air.md**: `node Tools/night/n65-air.cjs <the round's plan> a2` and `node Tools/night/n65-air.cjs Design/levels/n52/foundry.plan.json a2`
- **spot-A.png / spot-B.png**: the round's and the red's iso65 pictures, both drawn now with --no-card-marks (no line65 / frames65), through `python -I C:/Users/gaige/dev/SKYGRIND-unity/Tools/night/n65-blind.py <in> <out>`; A / B by sha256("a2:d602157a")'s first hex digit; the key is Research/2026-10-23-wave65/T/gate-proof-keys/a2-r2-spot-key.txt (outside this folder)
- **card.md**: `git show d602157a:Research/2026-10-23-wave65/cards/a2.md`
- **room-diff.txt**: `git diff --no-index` of the two files as `git show d602157a:...` prints them
- **checks.txt**: r2/a2/ and red/a2/: room-check.txt, flow-A / flow-B.txt (their headers, the a2 rows, SUMMARY, STYLE), pads.txt, ride.txt
- **open-findings.md**: the BLOCKING lines (and a bare "**BLOCKING:**" block's list) inside the a2 section of r1/review-gg.md, r1/review-ld.md, cardgate-R2.md, cardgate-R3.md, cardgate-S2.md, cardgate-S3.md, cardgate.md, and every line starting "a2: "
