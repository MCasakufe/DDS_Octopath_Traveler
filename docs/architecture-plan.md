# Octopath Traveler - Updated MVC Architecture Plan

## 1. Purpose
This document replaces the previous outdated plan with a realistic architecture roadmap for the current repository.

Scope of this plan:
- Align architecture with the project’s real structure today.
- Enforce MVC separation using the teacher rule:
  - All user interaction (input/output and formatting) belongs to the View.
  - If a component needs the View to work, it is Controller.
  - If a component does not need the View, it is Model.
- Keep compatibility with current script-based tests while enabling growth toward full game rules from `EnunciadoGeneralEspañol.md`.

This is a migration plan, not an all-at-once refactor.

## 2. Current Repository Reality (April 2026)
Current solution:
- `Octopath-Traveler-Controller`
- `Octopath-Traveler-View`
- `Octopath-Traveler.Tests`

Current implementation status:
- E1 tests exist and run through `Game(View view, string teamsFolder)` and `Play()`.
- Team setup, basic round loop, basic attacks, and winner flow are implemented.
- Interaction and formatting are still mixed into controller-side classes (`TravelerTurnFlow`, `BattleStatePrinter`, `TeamFileMenu`, etc.).
- There is no dedicated Model project yet.

Primary architectural issue:
- Core battle logic and UI concerns are coupled, making MVC boundaries unclear and future features (skills, statuses, break system, targeting rules) harder to add safely.

## 3. Architectural Goals
1. Preserve behavior and exact output for current E1 tests.
2. Move all presentation concerns to `Octopath-Traveler-View`.
3. Isolate deterministic game logic into Model components.
4. Keep Controller as orchestration only (use-case flow + coordination).
5. Prepare the codebase for incremental implementation of full game mechanics:
- Turn priority categories.
- Weakness and shields.
- Breaking Point lifecycle.
- Active/passive/divine skills.
- Buffs/debuffs/ailments/improvements.
- BP/boost constraints.

## 4. Target MVC Separation
### 4.1 View Responsibilities
The View layer owns:
- Text content and formatting.
- Input parsing from `ReadLine`.
- Menu rendering and cancel options.
- User-facing errors/messages.

Examples of View-facing APIs to introduce:
- `MainBattleView` or `BattleConsoleView`
- `TeamSelectionView`
- `BattleOutputView`

These classes wrap the low-level `View` adapter and expose semantic methods like:
- `ShowTeamFileOptions(...)`
- `ReadTravelerAction(...)`
- `ShowRoundState(...)`
- `ShowInvalidTeamFileMessage()`

### 4.2 Controller Responsibilities
The Controller layer owns:
- Use-case sequencing (`Play`, setup flow, round loop orchestration).
- Calling View APIs for interaction.
- Calling Model services/entities for game decisions and mutations.
- Error mapping from domain exceptions/result objects into view messages.

Controller must not:
- Build output strings directly.
- Parse menu options directly from raw text.
- Implement combat formulas or rule decisions.

### 4.3 Model Responsibilities
The Model layer owns:
- Battle state and unit state.
- Combat rules and calculations.
- Queue ordering rules.
- Status application and expiration.
- Validation rules independent from UI.

Model must not:
- Reference `Octopath_Traveler_View`.
- Know about console/test script formats.

## 5. Project Structure Roadmap
Recommended target structure:
- `Octopath-Traveler-Controller`
- `Octopath-Traveler-View`
- `Octopath-Traveler-Model`
- `Octopath-Traveler.Tests`

Why this is realistic:
- The guide zip already demonstrates this split conceptually.
- It removes the current pressure to keep model logic inside controller assembly.
- It avoids circular dependencies if contracts are clean.

Dependency direction target:
- `View -> Model` (optional read-only state/DTO consumption).
- `Controller -> View`
- `Controller -> Model`
- `Tests -> Controller + View` (and optionally Model for pure unit tests).

No dependency allowed:
- `Model -> View`
- `Model -> Controller`
- `View -> Controller`

## 6. Domain Scope from Enunciado
Model modules should reflect real game systems, not generic placeholders.

Core contexts:
- Team Setup: traveler and beast constraints.
- Units: stats, resources, alive/dead state, board slot.
- Turn Engine: round lifecycle, current/next queue.
- Combat Actions: basic attack, skill, defend, flee.
- Damage Rules: physical/elemental formulas, truncation, min-zero.
- BP/Boost Rules: gain, spend, max limits, boost constraints.
- Weakness/Shields/Breaking Point.
- Skills Engine: active/passive/beast/divine with target and effect composition.
- Status Engine: buffs, debuffs, ailments, improvements, durations.
- Victory and battle termination conditions.

## 7. Incremental Migration Plan (Realistic)
### Phase 0 - Stabilization Baseline
Objective:
- Freeze expected behavior before structural changes.

Tasks:
- Ensure E1 suites are green.
- Capture current output contracts as non-negotiable.

Exit criteria:
- `TestE1_InvalidTeams`, `TestE1_BasicCombat`, `TestE1_RandomBasicCombat` pass.

### Phase 1 - MVC Boundary Pass (No New Mechanics)
Objective:
- Move interaction and formatting out of controller-side battle/team flow.

Tasks:
- Introduce high-level view components in `Octopath-Traveler-View`.
- Refactor controller to call semantic view methods.
- Remove direct `ReadLine`/`WriteLine` usage from gameplay orchestration classes except composition roots/adapters.

Likely files touched:
- Controller: `Game`, `TravelerTurnFlow`, `TeamFileMenu`, `BattleStatePrinter`, `BattleActionPrinter`.
- View: new `MainBattleView`/`TeamSelectionView` classes plus DTOs for menu selections.

Exit criteria:
- E1 outputs unchanged.
- Controller no longer formats gameplay text directly.

### Phase 2 - Model Extraction
Objective:
- Extract model logic from controller assembly into `Octopath-Traveler-Model`.

Tasks:
- Move pure domain types and services:
  - combat units,
  - battle state,
  - turn queue ordering,
  - damage calculation,
  - winner logic,
  - validation rules.
- Keep controllers as orchestrators and adapters only.

Exit criteria:
- Model has zero dependency on view.
- Controller compiles against model contracts.
- E1 suites remain green.

### Phase 3 - Turn Engine Completion
Objective:
- Implement complete queue priority algorithm from enunciado.

Includes:
- recovering from Breaking Point priority,
- Defender priority,
- priority/de-priority effects,
- speed + side + board-slot tie breakers,
- dynamic next-round queue updates after actions.

Exit criteria:
- deterministic and testable queue builder.
- dedicated tests for ordering edge cases.

### Phase 4 - Combat Rule Expansion
Objective:
- Implement weakness, shields, and Breaking Point behavior fully.

Includes:
- shield loss only on weakness hit and only if damage > 0,
- broken state spanning current + next round,
- break damage bonus,
- shield reset and recovery priority behavior.

Exit criteria:
- rule-focused tests for shield and break transitions.

### Phase 5 - Skill and Status Engine
Objective:
- Add scalable ability architecture for active, passive, beast, and divine skills.

Includes:
- target selectors (`Single`, `Enemies`, `User`, `Ally`, `Party`, `Any`),
- effect pipeline (damage, heal, revive, status, special),
- status durations and stacking,
- passive-trigger conditions,
- boost behavior by skill category.

Exit criteria:
- data-driven skills from JSON definitions.
- isolated tests per effect type and target policy.

### Phase 6 - Hardening and Maintainability
Objective:
- Clean boundaries, readability, and regression safety.

Tasks:
- remove train-wrecks,
- eliminate mixed responsibilities,
- unify naming conventions,
- improve test pyramid:
  - model unit tests,
  - controller integration tests,
  - script regression tests.

Exit criteria:
- architecture checks pass.
- no direct UI formatting leaks outside view layer.

## 8. Quality Gates per Phase
Mandatory gates for every phase:
- Build passes.
- Existing E1 script tests pass.
- No test files modified unless explicitly adding new tests.
- No output drift in legacy scenarios.

Recommended extra gates:
- New model unit tests for each new rule set.
- Snapshot/regression checks for key script cases.

## 9. Risks and Mitigations
Risk:
- Large refactors break exact output expected by script tests.

Mitigation:
- Move presentation via adapter-by-adapter migration, not full rewrite.

Risk:
- Circular dependencies when introducing model/view contracts.

Mitigation:
- Keep contracts in Model or neutral contract types; never in Controller.

Risk:
- Overengineering patterns too early.

Mitigation:
- Introduce abstractions only when a second concrete use case appears (skills/effects/targets are natural triggers).

Risk:
- Scope explosion while E1 still in progress.

Mitigation:
- Phase gates and strict out-of-scope per phase.

## 10. Immediate Next Sprint (Recommended)
This is the next realistic sprint for your current repository:
1. Complete Phase 1 only (MVC boundary pass).
2. Keep behavior 100% identical for E1 tests.
3. After green tests, start Phase 2 model extraction in small batches.

Definition of done for this sprint:
- All user interaction APIs live in `Octopath-Traveler-View`.
- Controller orchestrates interaction and model decisions only.
- E1 suites pass unchanged.
