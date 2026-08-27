# Speech bark donor audit

The machine-readable inventory is `barks.tsv`. At commit time it contains 18 accepted sounds and 13 rejected source groups.

## Method

- GitHub code search covered `speechBark`, `Vocal-Barks`, `Barks/roundstart.yml`, `blooper` and related voice paths.
- ADT, Echo Protocol, Space Onyx, Utopia, Ganimed, Wega, Azure Peak, Citadel, NovaSector, Rotwood, Ochre Valley, Sandstorm and Monkestation-derived repositories were inspected.
- Repeated ADT, Citadel and BlueMoon packs were treated as one lineage instead of separate libraries.
- Candidates without per-file provenance and commercial-game extracts were recorded as rejected source groups.
- Accepted files trace to Goonstation, Paradise, BeeStation or original SS14 recordings and are processed by `process_barks.sh`.

## Reproduction

Run `Tools/_WL/Barks/process_barks.sh build` from the repository, followed by `Tools/_WL/Barks/process_barks.sh verify`. Verification requires Vorbis mono audio at 44.1 kHz, a reported duration between 100 and 301 ms, and a decoded sample peak no greater than -0.9 dBFS.
