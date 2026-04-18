# Linux (Arch / Hyprland)

This package is intended for players using Arch Linux, Hyprland, or similar modern Linux desktops.

## Download

Use the self-contained `tar.gz` archive. It already includes the required .NET runtime, so you do not need to install one separately.

## Getting Started

1. Extract the archive.
2. Open the extracted folder.
3. Start AATool with:

```bash
./run-aatool.sh
```

On first launch, AATool creates a writable `config/` folder from `config.defaults/`.

## What To Expect

- This Linux release is built for `linux-x64` systems.
- Some overlay and settings window features are still more limited than on Windows.
- Clipboard features can use `wl-copy`, `wl-paste`, or `xclip` when those tools are installed.
- Active Minecraft window detection can use `hyprctl` or `xdotool`, with fallback behavior when available.
