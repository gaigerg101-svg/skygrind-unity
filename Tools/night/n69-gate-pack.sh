#!/usr/bin/env bash
# NIGHT69 (FORGE66's copy of NIGHT65 builder T's n69-gate-pack.sh): the GATE evidence pack for one room (Tools/night/n69-gate-pack.cjs; its head lists the files and the options).
#   bash Tools/night/n69-gate-pack.sh <id> <round-tag> [options]
cd "$(git rev-parse --show-toplevel)" || exit 1
node Tools/night/n69-gate-pack.cjs "$@"
