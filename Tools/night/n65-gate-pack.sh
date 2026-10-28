#!/usr/bin/env bash
# NIGHT65 builder T: the GATE evidence pack for one room (Tools/night/n65-gate-pack.cjs; its head lists the files and the options).
#   bash Tools/night/n65-gate-pack.sh <id> <round-tag> [options]
cd "$(git rev-parse --show-toplevel)" || exit 1
node Tools/night/n65-gate-pack.cjs "$@"
