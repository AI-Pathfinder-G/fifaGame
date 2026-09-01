# AI System - Detailed Design

## 1. Purpose

Controls all non-user players: teammates, opponents, and goalkeepers. Provides tactical decision-making, formation maintenance, and dynamic positioning.

## 2. Architecture

```
AIManager (Singleton)
├── TeamAI[Home]         → TeamStrategy, FormationController
├── TeamAI[Away]         → TeamStrategy, FormationController
├── PlayerAI[x22]        → Individual decision making
├── GoalkeeperAI[2]      → GK-specific logic
└── TacticalEvaluator    → Shared tactical analysis
```

## 3. TeamStrategy

### 3.1 Strategy Types

| Strategy | Mentality | Line | Pressing | Width |
|----------|-----------|------|----------|-------|
| Balanced | Medium | Medium | Medium | Normal |
| Attacking | High | High | Low | Wide |
| Defensive | Low | Deep | Low | Narrow |
| Counter | Low → High | Deep | Low | Normal |
| Possession | Medium | Medium | Low | Normal |
| High Press | High | High | High | Normal |
| Park Bus | Very Low | Very Deep | Very Low | Very Narrow |

### 3.2 Dynamic Switching

```
// Context signals
scoreDiff = myScore - oppScore
timeRemaining = matchTime / totalTime

if scoreDiff > 1 and timeRemaining < 0.3:
    strategy = Defensive
elif scoreDiff < -1 and timeRemaining < 0.3:
    strategy = Attacking
elif scoreDiff < -1 and timeRemaining > 0.7:
    strategy = Counter
else:
    strategy = Balanced
```

### 3.3 Team Instructions

```csharp
public class TeamStrategy
{
    public MentalityType Mentality { get; set; }
    public float DefensiveLine { get; set; }     // 0..1 (deep→high)
    public float PressingIntensity { get; set; }  // 0..1
    public float WidthFactor { get; set; }        // 0.7..1.3
    public float Tempo { get; set; }              // 0..1
    public bool OffsideTrap { get; set; }
}
```

## 4. FormationController

### 4.1 Formation Data

```csharp
public class FormationData
{
    public string Name { get; }               // "4-4-2", "4-3-3", etc.
    public FormationSlot[] Slots { get; }     // 10 outfield + 1 GK
}

public class FormationSlot
{
    public PositionRole Role { get; }          // GK, LB, CB, RB, LM, CM, RM, LW, ST, RW...
    public Vector2 BasePosition { get; }       // normalized 0..1 (x=width, z=length)
    public float AttackBias { get; }           // forward shift when attacking
    public float DefenseBias { get; }          // backward shift when defending
}
```

### 4.2 Formations

| Formation | Slots |
|-----------|-------|
| 4-4-2 | GK, LB, CB, CB, RB, LM, CM, CM, RM, ST, ST |
| 4-3-3 | GK, LB, CB, CB, RB, LM, CM, CM, RM, LW, ST, RW |
| 4-2-3-1 | GK, LB, CB, CB, RB, CM, CM, LM, CAM, RM, ST |
| 3-5-2 | GK, CB, CB, CB, LM, CM, CM, CM, RM, ST, ST |
| 5-3-2 | GK, LB, CB, CB, CB, RB, CM, CM, CM, ST, ST |

### 4.3 Dynamic Position Calculation

```csharp
Vector3 GetTargetPosition(PlayerEntity player, FormationSlot slot, GamePhase phase)
{
    Vector3 base = slot.BasePosition.ToWorld(fieldLength, fieldWidth);

    // Phase-based shift
    float shift = phase == GamePhase.Attack ? slot.AttackBias :
                  phase == GamePhase.Defense ? -slot.DefenseBias : 0;

    // Ball attraction
    Vector3 ballInfluence = (ballPos - base) * 0.3f * pressIntensity;

    // Width adjustment
    base.x *= widthFactor;

    return base + Vector3.forward * shift + ballInfluence;
}
```

## 5. PlayerAI Decision Making

### 5.1 Update Loop

```
each frame (for each AI player):
  1. Evaluate game phase (attack/defense/transition)
  2. Determine role context (ball carrier, receiver, marker, supporter)
  3. Run decision tree
  4. Set target position / action
  5. Move toward target
  6. Update animation
```

### 5.2 Decision Tree (Outfield)

```
PHASE = EvaluatePhase()

if PHASE == Attack:
    if self == ballCarrier:
        DECIDE_WITH_BALL()
    else:
        if IsReceiverCandidate():
            MOVE_TO_OPEN_SPACE()
        elif IsSupportCandidate():
            PROVIDE_SUPPORT()
        else:
            MAINTAIN_FORMATION(attacking)

elif PHASE == Defense:
    nearest = FindNearestToBall(team)
    if self == nearest:
        PRESS_BALL_CARRIER()
    elif IsSecondDefender():
        COVER_SPACE()
    elif IsMarkingAssignment():
        MARK_PLAYER()
    else:
        MAINTAIN_FORMATION(defending)

elif PHASE == Transition:
    REORGANIZE()
```

### 5.3 With Ball Decisions

```
DECIDE_WITH_BALL():
    distToGoal = Distance(self, oppGoal)
    shotChance = EvaluateShotChance()
    passOptions = FindPassOptions()
    dribbleSpace = EvaluateDribbleSpace()

    if distToGoal < 25m AND shotChance > 0.4:
        SHOOT(aimAtBestCorner)
    elif BestPass(passOptions).score > 0.7:
        PASS(BestPass.target)
    elif dribbleSpace > 5m:
        DRIBBLE_FORWARD()
    elif HasSafeBackPass():
        PASS(safeBackTarget)
    else:
        HOLD_AND_SHIELD()
```

### 5.4 Passing Logic

```csharp
PassOption FindBestPass(List<PlayerEntity> teammates)
{
    foreach (var mate in teammates)
    {
        float score = 0;
        // Lane clear?
        if (IsPassLaneBlocked(self, mate)) continue;
        // Forward preference
        score += (mate.pos.z - self.pos.z).MapToScore();
        // Distance preference (ideal 10-25m)
        float dist = Distance(self, mate);
        score += DistanceScore(dist);
        // Receiver openness
        score += OpennessScore(mate);
        // Advancement value
        score += AdvancementScore(mate, oppGoal);

        options.Add(new PassOption(mate, score));
    }
    return options.OrderByDescending(o => o.score).FirstOrDefault();
}
```

## 6. GoalkeeperAI

### 6.1 Positioning

```csharp
Vector3 GetGKPosition(BallEntity ball, TeamSide myTeam)
{
    Vector3 goal = MyGoalCenter;
    Vector3 toBall = (ball.pos - goal).FlattenY().normalized;

    // Narrowing angle
    float angleToBall = Vector3.Angle(Vector3.forward * Sign, toBall);
    float narrowFactor = Mathf.Clamp(angleToBall / 45f, 0, 1);

    Vector3 pos = goal + toBall * (2f + narrowFactor * 3f);
    pos.y = 0;
    return pos;
}
```

### 6.2 Shot Response

```
OnShotDetected(ball, target):
    reactionTime = 0.15s - (GK.Reflexes / 99) * 0.1s
    wait(reactionTime)

    diveDir = PredictBallDirection()
    diveDist = PredictBallDistance()

    saveChance = GKRating / (GKRating + ShotPower + ShotPlacement)
    if random() < saveChance:
        DIVE(diveDir, diveDist)
        if CatchChance > threshold:
            CATCH()
        else:
            PARRY()
    else:
        DIVE(diveDir, diveDist)  // still try, just won't reach
```

## 7. Tactical Evaluator

Shared utility for both teams:

```csharp
public static class TacticalEvaluator
{
    public static GamePhase EvaluatePhase(TeamSide team);
    public static float EvaluateShotChance(Vector3 pos, TeamSide attacking);
    public static bool IsPassLaneBlocked(Vector3 from, Vector3 to, List<PlayerEntity> opponents);
    public static PlayerEntity FindNearestToBall(List<PlayerEntity> team);
    public static float OpennessScore(PlayerEntity player, List<PlayerEntity> opponents);
    public static bool IsOffside(PlayerEntity player, Vector3 ballPos, TeamSide attacking);
}
```

## 8. Offside Detection

```
// At moment of pass
secondLastDefender = opponents.OrderBy(z).Take(2).Last()
if attacker.z > secondLastDefender.z (toward opp goal):
    OFFSIDE
```

Only checked at pass initiation, not during run.

## 9. Performance Considerations

- AI update runs at 30Hz (every 2 frames at 60fps) for non-near players
- Near-ball players update at 60Hz
- Tactical evaluation cached per frame, shared across all AI
- Formation positions precomputed, only offsets recalculated