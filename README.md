# WoWCombatLogSplit

A tool to split the `WoWCombatLog.txt` into smaller files.

## Options

- `--file -file -f` the file path to `WoWCombatLog.txt` or any other combat log file.
- `--dir -dir -d` the output location where to store the split files. _(Defaults to the same folder as the file.)_
- `--gap -gap -g` hours between combat events before considering to split the log into its own file. _(Defaults to 1.0)_

## Examples

- `.\WoWCombatLogSplit.exe -f "C:\World of Warcraft\Logs\WoWCombatLog.txt"`
  Split the file into smaller files and store those in the same location as the original.

- `.\WoWCombatLogSplit.exe -f "C:\World of Warcraft\Logs\WoWCombatLog.txt" -d "C:\World of Warcraft\LogsArchive"`
  Split the file into smaller files and store those in their own LogsArchive folder.

- `.\WoWCombatLogSplit.exe -f "C:\World of Warcraft\Logs\WoWCombatLog.txt" -g 0.5`
  Split the file into smaller files using 30 minutes as the breakpoint.

## Requirements

- `net9.0` framework.

