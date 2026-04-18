# Linux (Arch)

This guide is for players using this Arch Linux focused fork of AATool.

The goal of this fork is simple: keep the tracker usable on Arch Linux without asking players to install a specific .NET runtime first.

## What This Fork Is

This is an unofficial fork of AATool aimed at Linux players, especially:

- Arch Linux users

The original AATool project was built for Windows. This fork keeps the tracker working on Linux as much as possible, but some Windows-only parts are still limited.

## What Works Right Now

- the main tracker window
- local world tracking
- custom saves folder tracking
- tracking a specific world directly
- live updates when Minecraft writes advancements or stats
- first-run config bootstrapping from defaults

## Current Differences Compared To Windows

- The old Windows settings dialogs are not the main setup path on Linux. You should edit the JSON config files instead.
- Overlay support is still more limited than on Windows.
- The Windows updater flow is not used the same way on Linux.
- Some cosmetic/network helper behavior is simplified on Linux.
- Automatic active-window detection exists, but for launcher users a custom saves path is often more reliable.

If you just want the tracker to work, the most reliable Linux setup is usually:

- `Source = 1` for a custom saves folder, or
- `Source = 2` for one exact world folder

## Installation

Use the Linux `tar.gz` release for this fork.

To extract it from the terminal, use:

```bash
tar -xzf AATool-arch-linux-<version>.tar.gz
```

1. Download the `tar.gz` file.
2. Extract it anywhere you like.
3. Open the extracted folder.
4. Start AATool with:

```bash
./run-aatool.sh
```

On first launch, AATool creates a writable `app/config/` folder from `app/config.defaults/`.

## Folder Layout

Inside the extracted folder the files you might care about are:

- `run-aatool.sh` - the launcher script
- `app/` - the actual program folder, including assets, configs, logs, and bundled libraries
- `README.md` - the main project readme
- `README-ARCH-LINUX.md` - the Linux-specific player guide
- `LICENSE.md` - the GPL license text

As a player, the folders you will care about most are inside `app/`:

- `app/config/`
- `app/config.defaults/`
- `app/logs/`

## Config Concept

This AATool fork uses two config folders:

- `app/config.defaults/` contains the shipped default settings
- `app/config/` contains your own live settings

How this works:

1. The app ships with default config templates in `app/config.defaults/`
2. On first launch, AATool creates matching files in `app/config/`
3. From that point on, you should edit the files in `app/config/`, not `app/config.defaults/`

If a config file gets broken, the easiest fix is usually:

1. close AATool
2. delete the broken file from `app/config/`
3. launch AATool again so it recreates it from `app/config.defaults/`

## Safe Editing Tips

- Close AATool before editing JSON files.
- Keep the JSON format valid.
- Use full paths
- If a path contains spaces, that is okay.

## Tracking Setup

The most important file for most players is:

- `app/config/config_tracking.json`

The most important setting in that file is `Source`.

### `Source`

This tells AATool how it should decide which world to track.

- `0 = ActiveInstance`
  - AATool tries to detect the currently active Minecraft instance automatically.
  - This is convenient when it works, but it depends on Linux desktop/window detection.

- `1 = CustomSavesPath`
  - AATool tracks the most recently updated world inside one saves folder.
  - This is usually the best option for PrismLauncher users.

- `2 = SpecificWorld`
  - AATool tracks one exact world folder only.
  - This is best when you want zero ambiguity.

### Recommended setups

For most Linux players:

- use `Source = 1` if you want AATool to follow the newest world inside one instance folder
- use `Source = 2` if you only want one specific world tracked

## Config File Reference

This section explains the config files in plain player language.

### `app/config/config_tracking.json`

This file controls what world is tracked and how progress is interpreted.

- `GameCategory`
  - selects the tracker category, such as `All Advancements`, `All Blocks`, or others

- `GameVersion`
  - selects the Minecraft version rules/objectives the tracker should use

- `AutoDetectVersion`
  - if `true`, AATool tries to detect the Minecraft version automatically when possible

- `UseSftp`
  - enables remote server tracking over SFTP
  - most players should leave this `false`

- `Source`
  - chooses how the world is selected
  - `0 = ActiveInstance`, `1 = CustomSavesPath`, `2 = SpecificWorld`

- `CustomSavesPath`
  - the folder containing many worlds
  - AATool tracks the most recently updated world inside it when `Source = 1`

- `CustomWorldPath`
  - the exact world folder to track when `Source = 2`

- `ManualChecklistMode`
  - switches the tracker into manual mode instead of reading the Minecraft save directly
  - most players should leave this `false`

- `Filter`
  - controls whether progress is shown for everyone combined or only one player
  - `0 = Solo`, `1 = Combined`

- `SoloFilterName`
  - the player name to focus on when `Filter = Solo`

- `BroadcastProgress`
  - used for multiplayer/co-op broadcasting features

- `OpenTrackerKey`
  - optional key used for external tracker integrations

- `OpenTrackerUrl`
  - optional URL used for external tracker integrations

- `LastSession`, `LastPlayer`, `LastUuid`, `CurrentRunnerProfileId`, `CurrentRunnerProfileName`, `LastOpenedAllBlocks`
  - mostly internal or remembered values
  - most players should leave these alone

### `app/config/config_main.json`

This file controls the main tracker window appearance and behavior.

- `FpsCap`
  - frame rate cap for the app window

- `DisplayScale`
  - scales the whole UI larger or smaller

- `AllowUserResizing`
  - if `true`, the window can be manually resized

- `HideCompletedAdvancements`
  - hides finished advancements from the list

- `HideCompletedCriteria`
  - hides completed sub-criteria

- `ShowBasicAdvancements`
  - shows easier/basic advancements in the tracker

- `ShowAmbientGlow`
  - enables extra background glow effects

- `ShowMyBadge`
  - shows your profile badge when available

- `RainbowMode`
  - enables animated rainbow colors

- `CloseFramesOnSelection`
  - automatically closes some detail frames after making a selection

- `Layout`
  - controls the general tracker layout
  - common values include `relaxed`, `compact`, `vertical`, `optimized`

- `FrameStyle`
  - controls the frame theme around objectives

- `PrideFrameList`
  - used when you choose a multi-pride frame setup

- `ProgressBarStyle`
  - controls the style of progress bars

- `RefreshIcon`
  - changes the icon used for refresh state or refresh controls

- `InfoPanel`
  - controls what side/info panel is shown by default

- `PreferredPlayerBadge`
  - your preferred badge style when multiple are available

- `PreferredPlayerFrame`
  - your preferred player frame style

- `BackColor`, `TextColor`, `BorderColor`
  - manual color overrides for the main window

- `StartupArrangement`
  - controls where the window appears at startup
  - `0 = Centered`, `1 = Remember`, `2 = TopLeft`, `3 = TopRight`, `4 = BottomLeft`, `5 = BottomRight`

- `LastWindowPosition`
  - remembered automatically when using `Remember`
  - most players should not edit this by hand

- `StartupDisplay`
  - which monitor to start on

- `AlwaysOnTop`
  - keeps the main tracker window above other windows

- `RenameToNotchApple`
  - cosmetic legacy setting

- `CompactMode`
  - legacy migration setting, usually not something players need to touch

### `app/config/config_overlay.json`

This file controls the stream/overlay tracker view. On Linux this feature is still more limited than on Windows.

- `Enabled`
  - turns the overlay feature on or off

- `ShowLabels`
  - shows text labels for overlay items

- `ShowCriteria`
  - shows sub-criteria in the overlay

- `ShowPickups`
  - shows tracked item pickups

- `ShowIgt`
  - shows in-game time

- `ShowLastRefresh`
  - shows when the tracker last refreshed

- `RightToLeft`
  - flips overlay layout direction

- `PickupsOpposite`
  - moves pickup display to the opposite side

- `LastRefreshOpposite`
  - moves last-refresh text to the opposite side

- `ClarifyAmbiguous`
  - adds extra clarifying text for objectives that may be confusing

- `Position`
  - overlay position preset

- `FrameStyle`
  - overlay frame theme

- `PinnedObjectiveList`
  - remembers pinned overlay objectives
  - advanced users can edit this, but most players should leave it alone

- `Speed`
  - animation or movement speed for overlay behavior

- `Width`
  - target overlay width

- `GreenScreen`
  - chroma key background color for stream setup

- `CustomTextColor`, `CustomBackColor`, `CustomBorderColor`
  - custom overlay colors

- `StartupArrangement`, `LastWindowPosition`, `StartupDisplay`
  - overlay window placement settings

### `app/config/config_network.json`

This file is for co-op/network identity and connection settings.

- `MinecraftName`
  - your in-game Minecraft username

- `PreferredName`
  - display name used by the app/community features

- `Pronouns`
  - optional profile field

- `Password`
  - optional co-op lobby password

- `IP`
  - host IP to connect to

- `Port`
  - network port used for co-op

- `AutoServerIP`
  - automatically determine the host IP when possible

- `IsServer`
  - remembers whether you are currently acting as server/host

Most solo players can ignore this file unless they want co-op features.

### `app/config/config_notes.json`

This file stores settings for the notes feature.

- `Enabled`
  - turns the notes feature on or off

- `AlwaysOnTop`
  - keeps the notes window above other windows

- `Width`, `Height`
  - default notes window size

On Linux this feature is more limited than in the original Windows version.

### `app/config/config_sftp.json`

This file is only needed for remote server tracking over SFTP.

- `Host`
  - server hostname or IP

- `Username`
  - SFTP login username

- `Password`
  - SFTP login password

- `Port`
  - SFTP port, usually `22`

- `AutoSaveMinutes`
  - expected autosave timing for remote sync workflows

- `Linux`
  - remote environment hint used by the tool

- `ServerRoot`
  - root path of the remote Minecraft server files

Most players should leave this entire file alone unless they know they need SFTP tracking.

## Practical Examples

### Example 1: PrismLauncher instance tracking

Use this when you want AATool to follow whichever world inside one Prism instance you are currently playing.

```json
"Source": {
  "Value": 1
},
"CustomSavesPath": {
  "Value": "/home/yourname/.local/share/PrismLauncher/instances/1.16.1 Speedrunning/minecraft/saves"
}
```

### Example 2: One exact world only

Use this when you want AATool to stay locked to one world folder.

```json
"Source": {
  "Value": 2
},
"CustomWorldPath": {
  "Value": "/home/yourname/.minecraft/saves/My World"
}
```

### Example 3: Standard `.minecraft/saves`

Use this when you just want AATool to watch your normal Minecraft saves folder.

```json
"Source": {
  "Value": 1
},
"CustomSavesPath": {
  "Value": "~/.minecraft/saves"
}
```

## Troubleshooting

### AATool starts but does not track any world

- Check `app/config/config_tracking.json`
- Make sure `Source` matches the way you want to track
- Make sure `CustomSavesPath` or `CustomWorldPath` points to a real folder
- If using PrismLauncher, prefer `Source = 1` with the exact instance saves folder

### AATool loads a world on startup but does not update live

- Pause and unpause Minecraft once to force a save
- Make sure the world is actually inside the tracked folder
- If using a launcher instance, confirm the path points to that exact instance’s `saves` folder

### AATool tracks the wrong world

- With `Source = 1`, AATool tracks the most recently updated world inside the folder
- If you want one exact world only, switch to `Source = 2`
