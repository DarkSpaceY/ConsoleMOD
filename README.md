# ConsoleMOD - Readme

## Overview
ConsoleMOD is a utility mod for the SpaceFlight Simulator game that enhances the in-game console with additional commands for debugging and inspecting your rockets.

## Features
- **Parts Inspection**: Get detailed information about all parts in your current rocket
- **Joint Analysis**: View connection information between parts
- **Collision Debugging**: Toggle collision information display
- **Position Tracking**: Check your rocket's current coordinates

## Installation
1. Place the `ConsoleMOD.dll` file in your Mods folder
2. Launch the game and verify the mod loads

## Available Commands

### `loc`
Displays your rocket's current position coordinates.

### `parts`
Lists all parts in your current rocket with:
- Part indices
- Mass values
- Module information
- Saves output to `ConsoleLogs/parts.txt`

### `part [index]`
Shows detailed information about a specific part, including:
- Transform information (position/rotation)
- All modules and their properties
- Variable states and callbacks

Example: `part 3` shows details for the 4th part (zero-indexed)

### `joints`
Displays all joint connections between parts in your rocket, showing:
- Connection pairs
- Part indices and names

### `collsion`
Toggles collision information display on/off.

## Compatibility
- **Game Version**: Requires Space Flight Simulator v1.5.9.8 or later
- **Mod Version**: v1.0.0

## Troubleshooting
If commands don't work:
1. Verify the mod loaded successfully
2. Ensure you have an active rocket when using rocket-related commands
3. Check for error messages in the console

## Changelog
**v1.0.0** (Initial Release)
- Added core commands (parts, joints, collision, loc)
- Implemented detailed part inspection system

## Author
DarkSpace
