#!/usr/bin/env bash
set -euo pipefail

root=$(git rev-parse --show-toplevel)
manifest="$root/Tools/_WL/Barks/barks.tsv"
mode=${1:-verify}

command -v ffmpeg >/dev/null
command -v ffprobe >/dev/null

build_bark() {
    local input=$1
    local output=$2
    mkdir -p "$(dirname "$output")"
    ffmpeg -hide_banner -loglevel error -nostdin -y -i "$input" \
        -af "silenceremove=start_periods=1:start_duration=0.005:start_threshold=-50dB,atrim=duration=0.295,afade=t=in:st=0:d=0.004,areverse,afade=t=in:st=0:d=0.015,areverse,loudnorm=I=-20:TP=-1:LRA=7,alimiter=limit=0.841395:attack=1:release=10:level=false" \
        -ac 1 -ar 44100 -c:a libvorbis -q:a 4 "$output"
}

verify_bark() {
    local output=$1
    local probe
    probe=$(ffprobe -v error -select_streams a:0 \
        -show_entries stream=codec_name,channels,sample_rate,duration \
        -of csv=p=0 "$output")
    IFS=',' read -r codec sample_rate channels duration <<< "$probe"

    [[ "$codec" == "vorbis" ]] || { echo "$output: codec is $codec" >&2; return 1; }
    [[ "$sample_rate" == "44100" ]] || { echo "$output: sample rate is $sample_rate" >&2; return 1; }
    [[ "$channels" == "1" ]] || { echo "$output: channel count is $channels" >&2; return 1; }
    awk -v value="$duration" 'BEGIN { exit !(value >= 0.1 && value <= 0.301) }' || {
        echo "$output: duration is $duration" >&2
        return 1
    }

    local peak
    peak=$(ffmpeg -hide_banner -nostats -nostdin -i "$output" -af volumedetect -f null - 2>&1 |
        sed -n 's/.*max_volume: \([-0-9.]*\) dB.*/\1/p')
    awk -v value="$peak" 'BEGIN { exit !(value <= -0.9) }' || {
        echo "$output: peak is $peak dB" >&2
        return 1
    }
}

accepted=0
rejected=0
while IFS=$'\t' read -r status id category source_repo commit source_path output license reason; do
    [[ "$status" == "status" || -z "$status" ]] && continue
    if [[ "$status" != "accepted" ]]; then
        ((rejected += 1))
        continue
    fi

    ((accepted += 1))
    case "$mode" in
        build)
            build_bark "$root/$source_path" "$root/$output"
            ;;
        verify)
            verify_bark "$root/$output"
            ;;
        *)
            echo "usage: $0 [build|verify]" >&2
            exit 2
            ;;
    esac
done < "$manifest"

echo "$mode: $accepted accepted bark(s), $rejected rejected source group(s)"
