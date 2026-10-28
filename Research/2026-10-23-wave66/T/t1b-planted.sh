#!/usr/bin/env bash
# NIGHT66 T1b: skip v2's planted rooms, in series (one dotnet job), each saved by save.sh with its command and sha at its top.
#   forced   n52-foundry b1 with verb65 launch planted, its first launchable husk taking damage only from launch36: should read >= 1.25
#   null     the same room with launch36's damage x0 (the verb fires and does nothing): should read below 1.25
#   nofire   n52-foundry a2 (two husks, no spitter) with verb65 parry planted: the verb cannot fire, should print UNSURE
#   natural  n52-foundry a2 with verb65 pierce planted (the old room NIGHT65's red read 0.85 on: a verb that does not pay), for reference
cd "$(git rev-parse --show-toplevel)" || exit 1
S=Research/2026-10-23-wave66/T/save.sh
H="dotnet run --project Tools/SimHarness -c Release --no-build -- n65verb skip --level Design/levels/n52-foundry.json"
bash $S t1b-plant-forced $H --room b1 --plant-verb65 b1:launch --plant-forced
bash $S t1b-plant-null $H --room b1 --plant-verb65 b1:launch --plant-null
bash $S t1b-plant-nofire $H --room a2 --plant-verb65 a2:parry
bash $S t1b-plant-natural-a2-pierce $H --room a2 --plant-verb65 a2:pierce
grep -h "n65verb skip: M1\|wall time" Research/2026-10-23-wave66/T/t1b-plant-*.txt
