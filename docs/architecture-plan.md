# Architecture Plan

## Purpose
This document defines the target project structure and working rules for a graded coding evaluation. The goal is to evolve game behavior safely while preserving evaluation compatibility.

## Current Modules
- `Octopath-Traveler-Controller`
  - Owns game flow, orchestration, and battle logic.
  - Entry point creates `View` and `Game`, then executes `Play()`.
- `Octopath-Traveler-View`
  - Owns input/output abstraction.
  - Provides implementations for console, testing, and manual testing modes.
- `Octopath-Traveler.Tests`
  - Runs scenario-based automated checks against expected scripts.
  - Acts as evaluation baseline.

## Non-Modifiable Areas
- `Octopath-Traveler.Tests/**` (unless explicitly requested for local-only work)
- `data/*-Tests/**`
- Expected output script files used by tests

## Design Principles
1. Controller contains game rules.
2. View contains only I/O behavior.
3. Tests validate externally visible behavior.
4. Public APIs and project structure remain stable.
5. Changes are incremental and minimal.

## I/O Contract
- Game logic must not call `Console.WriteLine` / `Console.ReadLine` directly.
- Use `View` abstraction:
  - `_view.WriteLine(...)`
  - `_view.ReadLine()`

## Recommended Implementation Order
1. Fix failing behavior with smallest possible change.
2. Keep outputs deterministic and test-friendly.
3. Refine naming/readability only when behavior is already correct.
4. Run targeted tests after each meaningful change.

## Done Criteria
- Behavior matches test expectations.
- No forbidden files changed.
- Architecture boundaries are preserved.
- Any residual risk is documented.
