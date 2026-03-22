# Octopath Traveler - High-Level Architecture Plan

## Purpose and Scope
This document defines a flexible, editable architecture direction for the game while keeping the current 3-project solution (`Octopath-Traveler-Controller`, `Octopath-Traveler-View`, and `Octopath-Traveler.Tests`).

It is intentionally high-level: it defines boundaries, candidate type locations, and feature ownership without prescribing low-level implementation details.

Architecture constraints aligned with GoF, Clean Code, and Clean Architecture:
- Dependency rule: `Presentation/Infrastructure -> Application -> Domain`.
- Domain must not depend on `View`, JSON adapters, or framework/runtime details.
- Use-case orchestration belongs to `Application`; business invariants and core rules belong to `Domain`.
- Pattern use is intentional and minimal: apply GoF patterns only when they reduce conditional complexity, duplication, or coupling.

This plan is architecture guidance only, not an immediate API or folder-creation commitment.

Document validation checklist:
- File content is fully in English.
- All required sections are present in the requested order.
- The future tree explicitly places major game entities from the assignment.
- Feature mapping covers major combat requirements from the assignment.
- Assumptions and open questions are explicitly separated.

## Proposed Future Directory Tree
```text
Octopath-Traveler/
+- Octopath-Traveler-Controller/
|  +- Program.cs
|  +- Game/
|  |  +- Game.cs
|  |  \- GameLoop.cs
|  +- Domain/
|  |  +- Units/
|  |  |  +- Travelers/         (traveler entities and traveler-specific rules)
|  |  |  +- Beasts/            (beast entities, weaknesses, shields)
|  |  |  +- Teams/             (PlayerTeam, EnemyTeam composition rules)
|  |  |  +- Stats/             (combat stats and derived values)
|  |  |  +- Resources/         (HP/SP/BP lifecycle rules)
|  |  |  \- Weapons/           (weapon models and weapon types)
|  |  +- Combat/
|  |  |  +- Board/             (battle board model)
|  |  |  +- Positions/         (board slots and left-to-right ordering data)
|  |  |  +- Rounds/            (round state and transitions)
|  |  |  +- Turns/             (turn state and turn progression)
|  |  |  +- TurnQueue/         (queue building and priority categories)
|  |  |  +- Actions/           (basic attack, skill use, defend, flee)
|  |  |  \- BreakingPoint/     (break lifecycle state and transitions)
|  |  +- Abilities/
|  |  |  +- Abstractions/      (IAbility, IActiveAbility, IPassiveAbility, IBeastAbility, IDivineAbility)
|  |  |  +- Active/            (traveler active abilities)
|  |  |  +- Passive/           (traveler passive abilities)
|  |  |  +- Beast/             (beast ability definitions)
|  |  |  +- Divine/            (divine ability definitions)
|  |  |  +- Effects/           (damage, heal, revive, buffs/debuffs, ailments, special effects)
|  |  |  \- Targeting/         (Single, Enemies, User, Ally, Party, Any selectors)
|  |  +- Statuses/
|  |  |  +- Buffs/
|  |  |  +- Debuffs/
|  |  |  +- Aliments/
|  |  |  +- Improvements/
|  |  |  \- PriorityEffects/   (priority and de-priority effects)
|  |  \- ValueObjects/
|  |     +- AttackType/
|  |     +- DamageCategory/
|  |     +- TargetType/
|  |     +- WeaknessProfile/
|  |     +- ShieldCounter/
|  |     +- BP/
|  |     +- SP/
|  |     +- HP/
|  |     +- Speed/
|  |     \- BoardSlot/
|  +- Application/
|  |  +- Services/             (TurnOrder, Damage, Boost, BreakingPoint, Victory)
|  |  +- UseCases/             (RunBattle, ResolveTurn)
|  |  +- Factories/            (UnitFactory, AbilityFactory, TeamFactory)
|  |  \- Interfaces/           (repository/provider abstractions)
|  +- Infrastructure/
|  |  +- Data/Json/            (JSON repositories and loaders)
|  |  \- Mapping/              (DTO <-> domain mapping)
|  \- Presentation/
|     +- Input/                (command parsing/validation from View input)
|     \- Output/               (battle text formatting for View output)
+- Octopath-Traveler-View/     (I/O abstraction implementations only)
\- Octopath-Traveler.Tests/    (behavioral tests through public flow)
```

## Suggested Locations for Key Types
| Type or Interface | Suggested Location | Notes |
| --- | --- | --- |
| `Ability`, `IAbility`, `IActiveAbility`, `IPassiveAbility`, `IBeastAbility` | `Octopath-Traveler-Controller/Domain/Abilities/Abstractions` | Stable contract point for all base ability categories. |
| `IDivineAbility`, `DivineAbility` | `Octopath-Traveler-Controller/Domain/Abilities/Divine` | Distinct seam for divine behavior if kept separate from regular active abilities. |
| `IAbilityEffect` | `Octopath-Traveler-Controller/Domain/Abilities/Effects` | Common effect contract (damage, heal, revive, statuses, special effects). |
| `ITargetSelector` | `Octopath-Traveler-Controller/Domain/Abilities/Targeting` | Isolates target rules (`Single`, `Enemies`, `User`, `Ally`, `Party`, `Any`). |
| `Traveler`, `Beast` | `Octopath-Traveler-Controller/Domain/Units/Travelers` and `.../Beasts` | Core unit models and per-unit combat behavior. |
| `PlayerTeam`, `EnemyTeam` | `Octopath-Traveler-Controller/Domain/Units/Teams` | Team composition and duplicate constraints. |
| `Weapon`, `IWeapon`, `WeaponType` | `Octopath-Traveler-Controller/Domain/Units/Weapons` | Basic attack capability and physical type ownership. |
| `TurnQueue` | `Octopath-Traveler-Controller/Domain/Combat/TurnQueue` | Encapsulates queue inputs and ordered outputs. |
| `BattleBoard`, `IBattleBoard` | `Octopath-Traveler-Controller/Domain/Combat/Board` | Board occupancy and combat-side positioning. |
| `BoardSlot` | `Octopath-Traveler-Controller/Domain/ValueObjects/BoardSlot` | Left-to-right tie-break identity and position value object. |
| `WeaknessProfile`, `IWeaknessPolicy` | `Octopath-Traveler-Controller/Domain/ValueObjects/WeaknessProfile` | Weakness lookup and shield-hit validity rules. |
| `ShieldState`, `BreakingPointState` | `Octopath-Traveler-Controller/Domain/Combat/BreakingPoint` | Explicit state transitions for shield depletion and recovery priority. |
| `DamageCalculator`, `IDamageCalculator` | `Octopath-Traveler-Controller/Application/Services` | Centralizes formula, weakness/break multipliers, truncation, and min-zero behavior. |
| `ITurnOrderService` | `Octopath-Traveler-Controller/Application/Services` | Orchestrates round queue creation and in-round next-round updates. |
| `ITurnAction` | `Octopath-Traveler-Controller/Domain/Combat/Actions` | Command-like action boundary for turn execution paths. |
| `TeamFactory`, `ITeamFactory`, `AbilityFactory` | `Octopath-Traveler-Controller/Application/Factories` | Controlled construction from loaded data and validated composition. |
| `Json*Repository`, `IUnitRepository`, `IAbilityRepository` | `Octopath-Traveler-Controller/Infrastructure/Data/Json` | Data access seam for JSON-backed definitions. |

Recommended architecture seams (guidance, not immediate API commitment):
`IAbility`, `IAbilityEffect`, `ITargetSelector`, `ITurnOrderService`, `IDamageCalculator`, `ITeamFactory`, `IUnitRepository`, `IAbilityRepository`, `IWeapon`, `IWeaknessPolicy`, `IBattleBoard`, `ITurnAction`, `IDivineAbility`.

## Main Modules and Responsibilities
- `Domain`: Combat model and rules vocabulary (units, teams, board, turns, abilities, statuses, value objects), independent from I/O and storage details.
- `Application`: Orchestrates use cases, coordinates domain interactions, and exposes interfaces required by outer layers.
- `Infrastructure`: Implements adapters for JSON loading and DTO-to-domain mapping behind application/domain interfaces.
- `Presentation`: Translates `View` input into commands and formats use-case/domain outcomes into user-visible output lines.
- `View` project: Contains only input/output implementations (`ConsoleView`, `TestingView`, `ManualTestingView`) and no game-rule logic.
- Design guardrail: Prefer behavior-rich domain entities/value objects over procedural "manager" services.

## Feature-to-Module Mapping
| Game feature or requirement | Primary module ownership | Typical files or components |
| --- | --- | --- |
| Team setup validation (1-4 travelers, 1-5 beasts, no duplicates) | `Application` + `Domain` | `Application/Factories/TeamFactory`, `Domain/Units/Teams/*` |
| Traveler and beast entity modeling (stats/resources/skills) | `Domain` | `Domain/Units/Travelers/*`, `Domain/Units/Beasts/*`, `Domain/Units/Stats/*`, `Domain/Units/Resources/*` |
| Basic attack weapon handling and physical attack typing | `Domain` | `Domain/Units/Weapons/*`, `Domain/ValueObjects/AttackType/*` |
| Board positioning and left-to-right tie breakers | `Domain` + `Application` | `Domain/Combat/Board/*`, `Domain/Combat/Positions/*`, `Application/Services/TurnOrderService` |
| Turn rounds and queue priority categories | `Application` + `Domain` | `Application/Services/TurnOrderService`, `Domain/Combat/TurnQueue/*`, `Domain/Combat/Rounds/*` |
| Traveler actions (basic attack, active skill, defend, flee) | `Domain` + `Application` | `Domain/Combat/Actions/*`, `Application/UseCases/ResolveTurn` |
| Beast automatic skill execution | `Domain` + `Application` | `Domain/Abilities/Beast/*`, `Application/UseCases/ResolveTurn` |
| Active/passive/divine ability modeling and special effects | `Domain` | `Domain/Abilities/Active/*`, `Domain/Abilities/Passive/*`, `Domain/Abilities/Divine/*`, `Domain/Abilities/Effects/*` |
| BP and boosting constraints, end-of-round BP gain | `Application` + `Domain` | `Application/Services/BoostService`, `Domain/ValueObjects/BP/*`, round-end policies |
| Weakness representation and shield-hit counting | `Domain` + `Application` | `Domain/ValueObjects/WeaknessProfile/*`, `Domain/Combat/BreakingPoint/*` |
| Shields and Breaking Point lifecycle | `Domain` + `Application` | `Domain/Combat/BreakingPoint/*`, `Application/Services/BreakingPointService` |
| Priority/de-priority effects from statuses | `Domain` + `Application` | `Domain/Statuses/PriorityEffects/*`, `Application/Services/TurnOrderService` |
| Damage formula, weakness/break multipliers, floor truncation, minimum 0 | `Application` + `Domain` | `Application/Services/DamageCalculator`, stat/attack value objects |
| Target rules and effect composition (`Single`, `Enemies`, etc.) | `Domain` | `Domain/Abilities/Targeting/*`, `Domain/Abilities/Effects/*` |
| Win/lose conditions and dead-unit behavior | `Application` + `Domain` | `Application/Services/VictoryService`, `Domain/Combat/BattleState` |
| Input/output flow through `View` abstraction | `Presentation` + `View` | `Presentation/Input/*`, `Presentation/Output/*`, `Octopath-Traveler-View/*` |

## GoF Pattern Candidates
- `Strategy`: Isolate variable policies for turn-order resolution, target selection, and damage-modifier composition.
- `Command`: Represent each turn action (`BasicAttack`, `UseAbility`, `Defend`, `Flee`) as executable units behind `ITurnAction`.
- `State`: Model beast break lifecycle and unit participation lifecycle (`Normal`, `Broken`, `Dead`, temporarily disabled states).
- `Composite`: Compose multi-effect abilities as ordered effect chains (for example damage + status + resource change).
- `Factory Method` (or `Abstract Factory`): Create units/teams/abilities from external definitions while keeping construction logic out of use cases.
- `Observer`: Publish combat events (round start/end, turn start/end, action resolved, damage applied) for passive-trigger reactions.

## Assumptions
- The current 3-project solution (`Controller`, `View`, `Tests`) remains unchanged.
- This is a planning artifact only; no code implementation or folder creation is implied now.
- User interaction continues through the existing `View` abstraction; no game rules are moved to `View`.
- Intention-revealing names, SRP boundaries, and small focused classes/methods are preferred.
- Cross-layer shortcuts are avoided; outer layers must not bypass `Application` to mutate core rules.
- New patterns are introduced only when they measurably reduce duplication, conditionals, or coupling.

## Open Questions
- Should divine abilities remain a distinct ability branch (`IDivineAbility`) or be modeled as constrained active abilities?
- Should weakness logic stay purely as a value object (`WeaknessProfile`) or expose a policy seam (`IWeaknessPolicy`) for future variants?
- Should board/position logic be centralized in `BattleBoard` only, or split with dedicated queue-facing position services?
- Which event granularity is required for passive triggers to remain clear without over-fragmenting the event model?
- Should JSON remain the single long-term data source, or should scenario-focused fixtures become a first-class source later?
