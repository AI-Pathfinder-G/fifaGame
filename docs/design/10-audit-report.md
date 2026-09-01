## Audit Report

### 1. Code Quality (score: 7/10)

**Strengths:**
- Consistent PascalCase for public members and methods.
- Good use of XML `<summary>` doc comments on critical classes (BallEntity, BallPhysics, BallCollisionHandler, MatchManager, RefereeSystem, SetPieceController, SubstitutionManager).
- Clear method decomposition; e.g., `PlayerAI` cleanly separates decision phases.
- Constants are well-named and grouped with comments explaining domain meaning (regulation pitch dimensions, ball mass/radius).

**Weaknesses:**
- Private field naming is inconsistent: `_camelCase` is used in `GameManager`, `BroadcastCameraController`, `UIManager`, `ScoreboardUI` while `camelCase` (no underscore) is used in `AIManager`, `PlayerAI`, `GoalkeeperAI`, `TacticalEvaluator`, `MatchManager`. Pick one and apply project-wide.
- `Magic number` `Time.deltaTime * 2f` in `MatchManager.Update` (`MatchManager.cs:117`) is undocumented and unexplained.
- `Flatten(Vector3 pos)` in `SetPieceController` is a vague name; `SnapToGround` would be clearer.
- Several comments drift from behavior: `RefereeSystem.cs` claims "Home attacks toward +Z" while `TacticalEvaluator.cs` says "Home attacks +X". At least one is wrong.

### 2. Architecture (score: 6/10)

**Strengths:**
- Namespaces are well-organized (`SoccerGame.AI`, `SoccerGame.Core`, `SoccerGame.Match`, `SoccerGame.Ball`, `SoccerGame.Data`, etc.).
- Clean `IGameState`/`IGameSystem` separation; `StateFactory` provides centralized state creation.
- Decoupling via `GameEvents` for cross-system communication.

**Weaknesses:**
- **Coordinate-system inconsistency** is the single biggest architectural problem. `TacticalEvaluator`, `FormationController`, `PlayerAI`, and `FormationData` treat `+X` as the attack direction, while `SetPieceController` and `RefereeSystem` (and the doc comments in `BallPhysics` and `PlayerAnimator`) treat `+Z` as the attack direction. These two world conventions cannot both be correct; the game will break on the pitch (corners, penalties, goal kicks all placed on the wrong axis).
- `GameEvents` is a static class with manual subscribe/unsubscribe; this is fragile compared to UnityEvents or a properly-owned event bus. The `ClearAll()` method (`GameEvents.cs:104`) is a footgun because it nukes every listener at once when `GameManager.OnDestroy` runs.
- `GameManager.RegisterSystems()` (`GameManager.cs:58-66`) only finds active `MonoBehaviour`s; any IGameSystem disabled or instantiated later is silently skipped.
- `PlayerEntity` exposes `HasBall` as a public mutable `bool` while `BallEntity` also tracks `Owner`. Two sources of truth for possession is a classic desync risk.
- `PlayerAI.KickBall` (`PlayerAI.cs:208-218`) writes directly to the ball's `Rigidbody`, bypassing `BallEntity.Release()`. Same in `BallCollisionHandler.BounceOffPost`. The encapsulation promised by `BallEntity` is being violated from outside.

### 3. Unity Best Practices (score: 7/10)

**Strengths:**
- Uses modern Unity 6 APIs: `rb.linearVelocity` (in most files), `FindObjectsByType` (Unity 2023+ replacement for `FindObjectsOfType`), `RequireComponent` attributes, `[SerializeField]` for inspector exposure.
- `BroadcastCameraController` uses `1f - Mathf.Exp(-rate * dt)` for frame-rate independent smoothing — excellent.
- Good Awake/Start/OnEnable/OnDisable separation.

**Weaknesses:**
- `PlayerController.cs` (entire file) and `PlayerAnimator.cs:18` still use the deprecated `Rigidbody.velocity` property. In Unity 6 this should be `linearVelocity`. Will produce compiler warnings.
- `AIManager.Awake()` calls `Initialize()` which constructs new `TeamStrategy()` instances, silently overwriting any inspector-configured values on the `[SerializeField]` fields (`AIManager.cs:21-25`). SerializeField initializers are usually a hint, but here they are dead code.
- `MainMenuController` subscribes to button events in `Start` but unsubscribes in `OnDestroy` — inconsistent with the rest of the codebase (which uses `OnEnable`/`OnDisable`). Buttons will survive scene reloads if the controller is destroyed first, leaking delegates.
- `PlayerAI.Start` calls `CachePlayers()` which uses `Object.FindObjectsByType<PlayerEntity>` — works, but breaks if `PlayerEntity` instances are spawned after `Start` (the AI will never see new players).
- `BroadcastCameraController.target` is a `Transform` reference; in Unity 6 this is fine, but the field is not exposed via a property for runtime mutation.

### 4. Performance (score: 7/10)

**Strengths:**
- `PlayerAI.MakeDecision` is throttled to `aiUpdateInterval` (default 50ms), avoiding per-frame reasoning cost.
- `BallPhysics` correctly uses `FixedUpdate` with `Time.fixedDeltaTime`.
- `BroadcastCameraController` does not allocate; uses struct math.
- `RadarUI` reuses pooled `RectTransform` dots instead of instantiating per frame.

**Weaknesses:**
- `TacticalEvaluator.OpennessScore`, `IsPassLaneBlocked`, and `FindNearestToBall` perform linear scans with `Vector3.Distance` (sqrt) per call. For 11v11 that's 22 opponents × every decision tick; using sqrMagnitude is a trivial win.
- `PlayerAI.IsUnderPressure`, `TeamHasBall`, `OpponentHasBall` each iterate the full squad every decision tick (twice per tick). Cache possession state on `BallEntity.Owner` instead.
- `PlayerAI.FindBestPassOption` iterates all teammates computing per-pair work — fine for 10 teammates, but combined with the above it adds up.
- `BallPhysics` calls `rb.AddForce` multiple times per FixedUpdate with `ForceMode.Acceleration`; consolidating into one accumulated force would be marginally faster and clearer.
- `UIManager.UpdateRadar` is called per UI update but no caller is wired up — dead path until somebody drives it.
- `MatchStats.NormalizePossession` is fine, but `MatchManager` writes possession seconds continuously; for long matches this is fine but consider snapshotting once per second.

### 5. Bug Risks (score: 4/10)

**Critical bugs:**

1. **`MatchManager` does not subscribe to `GameEvents.OnGoalScored`** to call `ScoreGoal`. When `BallCollisionHandler` raises the event, nothing updates the score. The match will never register goals. (`MatchManager.cs` entire file — no `SubscribeGoalScored` anywhere; `ScoreGoal` exists but is unreachable through events.)
2. **Coordinate-system mismatch** between `TacticalEvaluator`/`FormationController` (`+X = attack`) and `SetPieceController`/`RefereeSystem` (`+Z = attack`). Set-piece placement, throw-ins, goal kicks, corners, and penalties are placed on the wrong axis.
3. **`AIManager.Initialize()` overwrites `[SerializeField]` defaults** (`AIManager.cs:21-25`). Inspector-configured strategies are discarded on `Awake`.
4. **Two possession sources of truth**: `BallEntity.Owner` and `PlayerEntity.HasBall` are not synchronized. `PlayerAI.DecideWithBall` checks `player.HasBall` but `BallCollisionHandler` sets `ball.SetOwner`. A player who just gained possession will not have `HasBall = true` until somebody sets it.

**High-severity:**

5. `RefereeSystem.CheckOffside` (`RefereeSystem.cs:122-131`) is a simplified, incorrect implementation that uses `Z` axis; `TacticalEvaluator.IsOffside` is the proper one. The RefereeSystem version will produce wrong calls (or never trigger, since it doesn't appear to be invoked from anywhere).
6. `BallCollisionHandler.OnTriggerEnter` only checks the tag; it never validates that `other` is actually inside the trigger volume at the time of contact (multiple goals could fire on a fast-moving ball that penetrates twice in one frame).
7. `GameEvents.ClearAll()` (`GameEvents.cs:104`) wipes every subscriber when `GameManager.OnDestroy` runs, but any UI element still alive will have dangling closures pointing at null-ified event fields. Subsequent `RaiseX` calls won't NRE (because `?.Invoke`), but any work the UI expected to keep doing is silently gone.
8. `PlayerController.Rb.velocity` and `PlayerAnimator.cs:18` use the deprecated API; Unity 6 may treat this as an error or warning depending on project settings.
9. `BallPhysics` captures `position` and `velocity` at the start of `FixedUpdate`, then later writes `rb.position = new Vector3(position.x, Radius, position.z)` (`BallPhysics.cs:80-83`) — using a now-stale X/Z after gravity has applied. Minor but real.
10. `MatchManager.EndHalf` resets `MatchTime = 0` but does not call `setPieceController.SetupKickoff` — second-half kickoff is only triggered by `BeginSecondHalf`, which is never invoked from the state machine.
11. `PlayerAI.cs:1` has a UTF-8 BOM (`\ufeff`). Harmless but unclean; will cause problems if files are concatenated or processed by string tools.

**Medium:**

12. `RefereeSystem.fouls` and `cards` lists grow unbounded across the match.
13. `AudioManager.HandleGoalScored` receives `team` (a team name) but discards it — UI can't show which team scored.
14. `BallCollisionHandler.HandlePlayerCollision` passes `"None"` to `RaisePossessionChanged`, but `TeamSide.None` exists as a value yet no listener handles it consistently.
15. `MainMenuController.OnQuit` uses `Application.Quit` which on some platforms (mobile) does nothing; worth a fallback log.

### 6. Consistency (score: 5/10)

- **Field naming**: mixed `_camelCase` vs `camelCase` (see Code Quality).
- **Axis convention**: catastrophic mismatch described above (`+X` vs `+Z`).
- **Data encapsulation**: `PlayerEntity` uses public mutable fields (`Data`, `Team`, `Role`, `FieldNumber`) while every other class uses `[SerializeField] private` with public properties.
- **Fully qualified type names**: `Data/FormationData.cs` and `Data/FormationSlot.cs` use `SoccerGame.Core.TeamSide` despite being in the same project; just `using SoccerGame.Core;` would suffice.
- **Subscribe/unsubscribe patterns**: `OnEnable`/`OnDisable` in `AudioManager` and `UIManager`; `Start`/`OnDestroy` in `MainMenuController`; `Awake`/`OnDestroy` in `GameManager`. Pick one.
- **Public setters vs auto-properties**: `MatchManager.HalfDuration` has a custom setter, `MatchStats` fields are all public mutable, `PlayerStats.Overall()` is a method while the rest of the codebase exposes data via properties.
- **Comment style**: Some classes have XML doc, others have none. Some methods have no comment despite non-obvious behavior (`AdjustStrategy`, `ClassifySeverity`).

### 7. Completeness (score: 5/10)

**Missing functionality:**

- No wiring from `BallCollisionHandler` → `MatchManager.ScoreGoal`. (Likely the biggest gap.)
- No `MatchManager` subscription to its own `OnKickoff`/`OnGoalScored` events; the manager only *raises* them.
- No second-half kickoff trigger from state machine; `BeginSecondHalf` is never called automatically.
- No restart-after-goal sequence: `ScoreGoal` increments score and calls `SetupKickoff`, but the ball has not been returned to the center spot's spawn position — `SetupKickoff` does that, but only on next call.
- `Enums.cs` defines `MentalityType.VeryDefensive` and `VeryAttacking`, `GamePhase.TransitionToDefense` and `SetPiece`, `SetPieceType.None` and `DropBall` — none are referenced anywhere in the code shown.
- `PlayerStats.Overall()` averages only six attributes out of 25; GKs and forwards will get the same formula.
- `RadarUI` has no caller; `UIManager.UpdateRadar` exists but isn't invoked.
- `GoalTrigger.OnTriggerEnter` is empty (`Core/GoalTrigger.cs:14-16`) — purpose unclear; either remove or document.
- No AI-vs-AI or kickoff selection. No team selection UI handler — `MainMenuController.OnPlayMatch` opens a panel but nothing wires the chosen teams into `MatchManager`.
- `MatchManager.ScoreGoal` does not record shot/shot-on-target stats; `MatchStats.RegisterShot` exists but is never called.
- `RefereeSystem.EvaluateTackle` is never called from anywhere visible — no tackle detection system actually invokes it.
- No replay/highlight system, no substitution UI.
- No stamina UI; stamina is tracked in `PlayerController` but never broadcast.

**Incomplete implementations:**
- `BallPhysics.Aerodynamics` calculates forces correctly but does not clamp velocity magnitude — Magnus force can compound at high spin.
- `PlayerAI` is slot-based but no system assigns `SetSlotIndex` from the formation.
- `FormationController.GetTargetPosition` uses `player` only to provide a fallback position; otherwise the parameter is unused.

### 8. Game Logic (score: 6/10)

**Strengths:**
- Real regulation constants: pitch 105×68, goal 7.32×2.44, penalty spot 11m, ball mass 0.43kg, radius 0.11m, drag coeff 0.47 for sphere.
- Offside rule correctly uses the second-last defender (`TacticalEvaluator.IsOffside`) and a "level with ball" exception.
- AI strategy with `urgency` parameter correctly ramps attacking/pressing/tempo late in the game when trailing, and does the opposite when leading — solid football logic.
- AI decision tree is sensible: shoot only at good chance + close, pass under pressure, dribble only with space ahead, hold otherwise.

**Weaknesses:**
- Coordinate-system confusion breaks all set-piece placement (see Architecture).
- Penalty spot placement uses Z=±44 in `SetPieceController` but `FieldDimensions.PenaltySpotDist = 11`. The relationship between pitch X and Z is implicit and undocumented.
- `SetPieceController.cornerX = 30`, `cornerZ = 55` — corner arcs are at the touchline/goal-line intersection; these values should be derived from field dimensions in `FieldDimensions` rather than hard-coded.
- `MatchManager.halfDuration = 2700` (45 min × 60s) is correct, but then `MatchManager.Update` doubles delta (`Time.deltaTime * 2f`). So 22.5 real minutes = 90 game minutes. This is undocumented and unclear whether it's an oversight or design — it will affect all time-based logic (AI urgency, stamina drain).
- `MatchManager.EndHalf` and `EndMatch` do not record final stats.
- Stamina drain/regen rates are constant — no curve based on stats or game time.
- `MatchManager.ScoreGoal` triggers `SetupKickoff` immediately — no kickoff animation, no reset of player positions to halves.
- `PlayerEntity.Stamina` clamps 0..1 but is drained in `PlayerController` without consideration of `Acceleration`/`Pace` stats.
- `PlayerAI` doesn't differentiate behavior by `PositionRole` — goalkeepers, defenders, and strikers run the same decision tree. The formation slot index is assigned but ignored in decision-making.
- `MatchManager.HalfDuration` is settable but never used to validate total match time. There is no extra time, no stoppage time, no injury time.
- `MatchStats.NormalizePossession` divides by total seconds then multiplies by 100; minor: division-then-multiplication can be reordered for clarity.
- No offside enforcement actually calls a flag; `RefereeSystem.CheckOffside` is unused and would give wrong results anyway.

---

### Critical Issues (must fix)

1. **Wire `MatchManager.ScoreGoal` to the goal event.** Currently `BallCollisionHandler` raises `OnGoalScored` but no listener increments the score.
   - Files: `Match/MatchManager.cs`, `Ball/BallCollisionHandler.cs`, `Audio/AudioManager.cs`, `UI/UIManager.cs`.

2. **Resolve the X/Z coordinate-system conflict** between AI/Formation modules (`+X = attack`) and SetPiece/Referee modules (`+Z = attack`). Choose one axis convention and propagate it.
   - Files: `AI/TacticalEvaluator.cs`, `AI/FormationController.cs`, `AI/PlayerAI.cs`, `Match/SetPieceController.cs`, `Match/RefereeSystem.cs`, `Ball/BallPhysics.cs`.

3. **Fix `AIManager.Initialize()` overwriting inspector values** (`AIManager.cs:21-25`). Either remove the `Initialize()` body, guard it with a `_initialized` flag, or only initialize if the serialized field is null.

4. **Synchronize `PlayerEntity.HasBall` with `BallEntity.Owner`** or remove the redundant flag. `PlayerAI.DecideWithBall` will misfire when `HasBall` is stale.

5. **Replace deprecated `Rigidbody.velocity`** with `linearVelocity` in `Player/PlayerController.cs` and `Player/PlayerAnimator.cs:18`.

6. **Auto-trigger second-half kickoff** from state machine: `MatchManager.EndHalf` resets state but never re-enters `KickoffState`/`PlayState`.

7. **Fix `RefereeSystem.CheckOffside`** to either delegate to `TacticalEvaluator.IsOffside` or be removed. As written it is wrong.

---

### Recommendations (should fix)

- Standardize on one private-field naming convention (`_camelCase` is more common in modern C# codebases).
- Replace `GameEvents.ClearAll()` with a deterministic unsubscribe pattern; consider `UnityEvent`-based or ScriptableObject event channels.
- Make `PlayerEntity` use `[SerializeField] private` with public properties, matching the rest of the codebase.
- Cache the team possession check on `BallEntity.Owner` rather than iterating teammates every decision tick.
- Use `sqrMagnitude` instead of `Distance` in `TacticalEvaluator` to avoid `sqrt` in hot paths.
- Strip the UTF-8 BOM from `AI/PlayerAI.cs:1`.
- Implement `MatchStats.RegisterShot` invocation in `PlayerAI.Shoot` and on goal events.
- Add doc comments explaining why `MatchManager.Update` multiplies delta by 2 — and consider whether that's actually desired.
- Add a `RegisterSystems()` callback the project can call from a bootstrap MonoBehaviour instead of `GameManager` scanning the scene for `IGameSystem` instances.
- Add a `Reset()` method to `MatchManager` that is called between halves and that resets player positions, ball state, and AI decisions.
- Replace `string`-based action names in `GameEvents.OnPlayerAction` with an `enum` to avoid string typos.
- Decrement `SubsUsed` or use proper red-card tracking when a player is sent off in `RefereeSystem.IssueCard`.
- Add an XML doc comment header to every public class for documentation generation.

---

### Overall Score: 45/80

### Summary
This is a well-structured Unity 6 soccer codebase with sensible separation into AI, Ball, Match, Player, Core, Data, Audio, UI, and Camera namespaces, plus realistic physics constants and a coherent AI strategy system. However, it is not shippable as-is: the **goal-scoring path is unwired** (the `BallCollisionHandler` raises `OnGoalScored` but `MatchManager.ScoreGoal` has no listener), there is a **fundamental X-vs-Z axis mismatch** between the AI/formation layer and the set-piece/referee layer, and `AIManager.Initialize()` silently overwrites inspector-configured values. Several Unity 6 deprecations, stale state-machine transitions, and an unused RefereeSystem offside check need attention before this can run end-to-end.