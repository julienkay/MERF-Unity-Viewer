> **Warning**
> **!!! Work in progress !!!**  
> Does not work, but could be fun to fix.

# MERF Unity Viewer

This repository contains the source code for a Unity port of the web viewer for [MERF - Memory-Efficient Radiance Fields for
Real-time View Synthesis in Unbounded Scenes](https://creiser.github.io/merf/)[^1]

*Please note, that this is an unofficial port. I am not affiliated with the original authors or their institution.*

## Known Issues

- Everything mostly works up to calculating the diffuse term of the volume. But the *'evaluateNetwork()'* function in the shader seems to return unreasonably high values, probably due to some coordinate space or other platform-specific differences between the original Three.js viewer and Unity. Contributions welcome.

## Acknowledgements

[^1]: [Christian Reiser and Richard Szeliski and Dor Verbin and Pratul P. Srinivasan and Ben Mildenhall and Andreas Geiger and Jonathan T. Barron and Peter Hedman. MERF: Memory-Efficient Radiance Fields for Real-time View Synthesis in Unbounded Scenes. SIGGRAPH, 2023](https://creiser.github.io/merf/)
