# Match & Referee System - Detailed Design

## 1. Purpose

Manages match rules: scoring, match clock, fouls, offsides, cards, set pieces (free kicks, corners, throw-ins, goal kicks, penalties).

## 2. MatchManager

```csharp
public class MatchManager : MonoBehaviour, IGameSystem
{
    public int HomeScore { get; private set; }
    public int AwayScore { get; private set; }
    public float MatchTime { get; private set; }      // seconds elapsed
    public float HalfDuration { get; set; }            // 2700s = 45 min
    public int CurrentHalf { get; private set; }       // 1 or 2
    public TeamSide KickoffTeam { get; private set; }
    public TeamSide Possession { get; private set; }

    public void StartMatch();
    public void ScoreGoal(TeamSide team);
    public void EndHalf();
    public void EndMatch();
    public void AwardSetPiece(SetPieceType type, TeamSide team, Vector3 position);
}
```

## 3. Match Clock

```
realTimeScale = 2.0  (1 real second = 2 match seconds → 45 min = 22.5 real min)

MatchTime += Time.deltaTime * realTimeScale

if MatchTime >= HalfDuration AND CurrentHalf == 1:
    → HalftimeState
if MatchTime >= HalfDuration AND CurrentHalf == 2:
    + stoppage time
    → FulltimeState
```

Stoppage time: 1-5 minutes based on events:
- Goals: +30s
- Injuries/subs: +15s each
- Set pieces: +10s each

## 4. RefereeSystem

### 4.1 Foul Detection

```
OnTackleAttempt(tackler, ballCarrier):
    if IsFairTackle(tackler, ballCarrier):
        // Ball won cleanly
        transferPossession
    else:
        foul = new FoulData {
            Fouller = tackler,
            Victim = ballCarrier,
            Position = ballCarrier.pos,
            Severity = EvaluateSeverity(tackler, ballCarrier)
        }
        OnFoulCommitted(foul)
```

### 4.2 Foul Severity

| Severity | Condition | Card |
|----------|-----------|------|
| Minor | Late tackle, no contact to legs | No card |
| Moderate | Contact from behind, reckless | Yellow |
| Severe | Dangerous, studs up, last man | Red |
| Penalty | Foul inside penalty box by defender | Penalty + possible card |

### 4.3 Card System

```csharp
public class CardData
{
    public PlayerEntity Player { get; set; }
    public CardType Type { get; set; }    // Yellow, Red
    public float TimeIssued { get; set; }
    public string Reason { get; set; }
}
```

- 2 Yellow cards = Red (sent off)
- Red card = sent off, team plays with 10
- Accumulated yellows in tournament → suspension (future)

## 5. Set Pieces

### 5.1 Set Piece Types

```csharp
public enum SetPieceType
{
    Kickoff,
    FreeKick,
    CornerKick,
    ThrowIn,
    GoalKick,
    PenaltyKick,
    DropBall
}
```

### 5.2 Set Piece Flow

```
1. MatchManager.AwardSetPiece(type, team, pos)
2. → SetPieceState
3. Freeze ball at position
4. Position players:
   - Attacking: set piece taker + targets
   - Defending: wall/positioning
5. Wait for player input to execute
6. Execute (pass/shoot/cross)
7. → PlayState (resume open play)
```

### 5.3 Free Kick Setup

```
if distance < 25m to goal:
    - Attacking: taker, wall attackers (2-3)
    - Defending: wall (2-5 players based on distance), GK ready
elif distance > 25m:
    - Attacking: taker, receivers
    - Defending: zonal marking
```

### 5.4 Corner Setup

```
- Attacking: corner taker, 4-5 targets in penalty box
- Defending: man-marking, 1-2 on near post, GK positioned
```

### 5.5 Penalty Setup

```
- Attacking: penalty taker
- Defending: GK on line
- All other players outside penalty arc
- Ball at penalty spot (11m from goal)
```

### 5.6 Throw-In Setup

```
- Attacking: thrower at touchline, 1-2 receivers
- Defending: marker on receiver
- Ball at touchline position
```

## 6. Substitutions

```csharp
public class SubstitutionManager
{
    public int SubsUsed { get; private set; }
    public int MaxSubs { get; set; } = 5;

    public bool CanSubstitute => SubsUsed < MaxSubs;

    public void Substitute(PlayerEntity out, PlayerEntity in)
    {
        if (!CanSubstitute) return;
        // Swap data, positions
        SubsUsed++;
        OnSubstitution(out, in);
    }
}
```

## 7. Match Events Log

```csharp
public class MatchEvent
{
    public float Time { get; set; }
    public MatchEventType Type { get; set; }  // Goal, Foul, Card, Sub, Save, Shot, etc.
    public TeamSide Team { get; set; }
    public PlayerEntity Player { get; set; }
    public string Description { get; set; }
}
```

Events stored in list for:
- Commentary
- Post-match stats
- Highlight replay

## 8. Match Statistics

```csharp
public class MatchStats
{
    // Shots
    public int ShotsHome, ShotsAway;
    public int ShotsOnTargetHome, ShotsOnTargetAway;

    // Possession
    public float PossessionHome, PossessionAway;  // 0..1

    // Passing
    public int PassesHome, PassesAway;
    public int PassAccuracyHome, PassAccuracyAway;  // 0..100

    // Set pieces
    public int CornersHome, CornersAway;
    public int FreeKicksHome, FreeKicksAway;

    // Discipline
    public int FoulsHome, FoulsAway;
    public int YellowHome, YellowAway;
    public int RedHome, RedAway;
}
```

## 9. Match Result

```csharp
public class MatchResult
{
    public int HomeScore, AwayScore;
    public MatchStats Stats;
    public List<MatchEvent> Events;
    public TeamSide Winner => HomeScore > AwayScore ? TeamSide.Home :
                               AwayScore > HomeScore ? TeamSide.Away : TeamSide.None;
}
```