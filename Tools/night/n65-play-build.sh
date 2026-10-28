#!/usr/bin/env bash
# NIGHT65 (the lead's; NIGHT65.md section 4 step 6 and section 9 (f); branch C, DECIDE65 D-1 / D-7): a PLAY build for GG made from a
# verified build without a second 380 MB copy. Builds/<dst> is a HARD-LINK copy of Builds/<src> (`cp -al`: the same files on disk, about 0
# bytes), then the files that differ are written as independent copies (rm the link, copy afresh, so the source build is never written
# through a shared link): the runtime config (the source's, or --config <file>, e.g. HEAD's Design/unity-config.json; its design rows then
# turned on by Tools/night/n46-build-rows.cjs) and the level files (the given assemblies; --add-level when the source build does not ship a
# level yet, D-7). It prints the source's sha256s before and after (they must match) and the play build's level and config shas.
# NIGHT67 T1c (slate T1c line 2; NIGHT67.md section 4 step 6): SEVERAL levels in one play build, so it holds every PASSed room:
#   - <level.json> may be a comma list (each file's own "name" says which level it is: n67-iceline, n67-kiln, ...);
#   - --update: Builds/<dst> already exists (a play build this tool made): only the given levels (and --config / rows) are written into it,
#     each unlinked first (so neither the source build nor <dst>'s other files change); <src> names the build it was made from (checked:
#     its config and levels' sha256 must not change);
#   - BUILDS=<dir> (default Builds) for the proof (a scratch tree), never for a real play build.
#   bash Tools/night/n65-play-build.sh <src> <dst> <level.json>[,<level2.json>...] [--config <file>] [--add-level] [--update] [<night.row> ...]
#   e.g. bash Tools/night/n65-play-build.sh n67-freeze n67-play Logs/n67/play-passed/kiln/n67-kiln.json,Logs/n67/play-passed/spire/n67-spire.json --config Design/unity-config.json --add-level n67.kiln67 n67.spire67
cd "$(git rev-parse --show-toplevel)" || exit 1
src="${1:?src}"; dst="${2:?dst}"; levels="${3:?level.json[,level.json...]}"; shift 3
config=""; add=""; update=""; rows=()
while [ $# -gt 0 ]; do
  case "$1" in --config) config="$2"; shift 2 ;; --add-level) add=1; shift ;; --update) update=1; shift ;; --*) echo "n65-play-build: unknown option $1"; exit 1 ;; *) rows+=("$1"); shift ;; esac
done
B="${BUILDS:-Builds}"; S="$B/$src"; D="$B/$dst"; SA="SKYGRIND_Data/StreamingAssets/Design"
[ -f "$S/SKYGRIND.exe" ] || { echo "n65-play-build: no $S/SKYGRIND.exe"; exit 1; }
if [ -n "$update" ]; then [ -f "$D/SKYGRIND.exe" ] || { echo "n65-play-build: --update: no $D/SKYGRIND.exe (make the play build first, without --update)"; exit 1; }
else [ -e "$D" ] && { echo "n65-play-build: $D exists (refused: never overwrite a build; --update adds levels to a play build)"; exit 1; }; fi
[ -z "$config" ] || [ -f "$config" ] || { echo "n65-play-build: no config file $config"; exit 1; }
IFS=',' read -r -a LV <<< "$levels"
names=(); hads=()
for level in "${LV[@]}"; do
  [ -f "$level" ] || { echo "n65-play-build: no level file $level"; exit 1; }
  name=$(node -p "JSON.parse(require('fs').readFileSync(process.argv[1],'utf8')).name" "$level") || exit 1
  for n in "${names[@]}"; do [ "$n" = "$name" ] && { echo "n65-play-build: two level files named $name"; exit 1; }; done
  had=""; [ -f "$S/$SA/levels/$name.json" ] && had=1
  [ -n "$had" ] || [ -n "$add" ] || { echo "n65-play-build: $S has no $SA/levels/$name.json (the level must be one the build ships, or pass --add-level)"; exit 1; }
  names+=("$name"); hads+=("$had")
done
sum() { sha256sum "$1" | cut -c1-16; }
srcsums() { local s; s="config $(sum "$S/$SA/unity-config.json")"; for i in "${!names[@]}"; do [ -n "${hads[$i]}" ] && s="$s ${names[$i]} $(sum "$S/$SA/levels/${names[$i]}.json")"; done; echo "$s"; }
before=$(srcsums)
if [ -z "$update" ]; then cp -al "$S" "$D" || { echo "n65-play-build: cp -al failed"; exit 1; }; fi
if [ -z "$update" ] || [ -n "$config" ]; then
  rm "$D/$SA/unity-config.json" && cp "${config:-$S/$SA/unity-config.json}" "$D/$SA/unity-config.json" || { echo "n65-play-build: could not replace the config"; exit 1; }
fi
for i in "${!names[@]}"; do
  name="${names[$i]}"; level="${LV[$i]}"
  [ -e "$D/$SA/levels/$name.json" ] && { rm "$D/$SA/levels/$name.json" || { echo "n65-play-build: could not unlink $name"; exit 1; }; }
  cp "$level" "$D/$SA/levels/$name.json" || { echo "n65-play-build: could not write $name"; exit 1; }
done
[ ${#rows[@]} -gt 0 ] && node Tools/night/n46-build-rows.cjs "$dst" on "${rows[@]}" | tail -1
after=$(srcsums)
[ "$before" = "$after" ] || { echo "n65-play-build: THE SOURCE CHANGED ($before -> $after)"; exit 1; }
for i in "${!names[@]}"; do [ -n "${hads[$i]}" ] || [ ! -e "$S/$SA/levels/${names[$i]}.json" ] || { echo "n65-play-build: THE SOURCE GAINED ${names[$i]}.json"; exit 1; }; done
lines=""; for i in "${!names[@]}"; do tagx=""; [ -z "${hads[$i]}" ] && tagx=" (added)"; lines="$lines ${names[$i]} = $(sum "$D/$SA/levels/${names[$i]}.json") from ${LV[$i]}$tagx;"; done
cfgFrom="${config:-the source}"; [ -n "$update" ] && [ -z "$config" ] && cfgFrom="the play build's own (unchanged)"
echo "n65-play-build: $D ${update:+updated }from $S (hard links; the source's $before unchanged); levels:$lines its config $(sum "$D/$SA/unity-config.json") from $cfgFrom; rows on: ${rows[*]:-none}"
