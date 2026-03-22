# Testing Data Contract

## Purpose
This document explains how test input and expected output files are organized and consumed during debug/test runs.

## Data Location Used at Runtime
- During execution, test assets are available under:
  - `Octopath-Traveler-Controller/bin/Debug/net8.0/data`
- Test groups are organized in folder pairs:
  - `<GroupName>/` for input scripts
  - `<GroupName>-Tests/` for expected output scripts
- Example pair:
  - `E1-BasicCombat/`
  - `E1-BasicCombat-Tests/`

## Input Contract (`_view.ReadLine()`)
- Each file in `<GroupName>/` (for example `001.txt`, `002.txt`) is an input script for one scenario.
- Every call to `_view.ReadLine()` consumes the next available input line from the selected input script.
- If the program requests more input lines than the script contains, the test run fails.

## Output Contract (`_view.WriteLine()`)
- Each file in `<GroupName>-Tests/` with the same name (for example `001.txt`) is the expected output script for that scenario.
- Every `_view.WriteLine(...)` call produces one output line in the generated script.
- The generated script is compared line-by-line against the expected output file.
- Any mismatch in content, order, or number of lines causes the test to fail.

## Mapping Rule
- Input file and expected file are paired by group and filename:
  - Input: `<GroupName>/<CaseId>.txt`
  - Expected: `<GroupName>-Tests/<CaseId>.txt`
- Example:
  - Input: `E1-BasicCombat/001.txt`
  - Expected: `E1-BasicCombat-Tests/001.txt`

## Practical Implications for Implementation
- Keep all game I/O routed through the `View` abstraction (`_view.ReadLine()` and `_view.WriteLine(...)`).
- Preserve output text format exactly as required by expected scripts.
- Avoid extra output lines, missing lines, or reordered lines.
- Do not edit test scripts unless explicitly requested for non-evaluation local work.
