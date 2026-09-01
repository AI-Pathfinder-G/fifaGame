# Core Game Manager - Detailed Design

## 1. Purpose

Central singleton that drives the game's state machine, manages system lifecycle, and coordinates all subsystems.

## 2. State Machine

```
BootState
  │
  ▼
MenuState ◄──────────────────────────┐
  │                                  │
  ▼                                  │
KickoffState                         │
  │                                  │
  ▼                                  │
PlayState ◄──► SetPieceState         │
  │             │                    │
  │             ▼                    │
  │         PlayState (resume)        │
  │                                  │
  ▼                                  │
HalftimeState                         │
  │                                  │
  ▼                                  │
PlayState (2nd half)                 │
  │                                  │
  ▼                                  │
FulltimeState ───────────────────────┘
```

## 3. Interface

```csharp
public interface IGameState
{
    void Enter();
    void Update();
    void Exit();
    GameStateType NextState();
}
```

## 4. GameManager API

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameStateType CurrentState { get; }
    public MatchManager Match { get; }
    public AudioManager Audio { get; }
    public UIManager UI { get; }
    public CameraController Camera { get; }

    public void ChangeState(GameStateType state);
    public T GetSystem<T>() where T : MonoBehaviour, IGameSystem;
}
```

## 5. System Registration

All subsystems implement `IGameSystem`:

```csharp
public interface IGameSystem
{
    void Initialize();
    void Shutdown();
    bool IsInitialized { get; }
}
```

Systems register themselves in `Awake()` and are accessible via `GameManager.Instance.GetSystem<T>()`.

## 6. State Behaviors

| State | Entry Action | Update | Exit Action |
|-------|-------------|--------|-------------|
| Boot | Load resources, init systems | Progress bar | Transition to Menu |
| Menu | Show main menu UI | Handle navigation | Hide menu, load match |
| Kickoff | Place players, reset ball | Wait for kickoff input | Start play |
| Play | Enable player control + AI | Match simulation | - |
| SetPiece | Freeze play, setup set piece | Execute set piece | Resume play |
| Halftime | Show stats, swap sides | Wait for resume | Kickoff 2nd half |
| Fulltime | Show final score | Wait for input | Return to menu |

## 7. Scene Loading

| Scene | States |
|-------|--------|
| Boot.unity | BootState |
| Menu.unity | MenuState |
| Match.unity | KickoffState, PlayState, SetPieceState, HalftimeState, FulltimeState |

## 8. Event Bus

Lightweight pub/sub for cross-system communication:

```csharp
public static class GameEvents
{
    // Match
    static event Action<int, int> OnScoreChanged;      // homeScore, awayScore
    static event Action<float> OnMatchTimeChanged;      // remaining seconds
    static event Action<TeamSide> OnPossessionChanged;
    static event Action<FoulData> OnFoulCommitted;
    static event Action<CardData> OnCardShown;

    // Player
    static event Action<int> OnPlayerSwitched;          // new player index
    static event Action<PlayerAction> OnPlayerAction;

    // Match flow
    static event Action OnKickoff;
    static event Action OnHalftime;
    static event Action OnFulltime;
    static event Action<SetPieceType> OnSetPiece;
}
```