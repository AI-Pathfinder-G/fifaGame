# UI & Camera System - Detailed Design

## 1. UI System

### 1.1 UI Screens

| Screen | Scene | Purpose |
|--------|-------|---------|
| Splash | Boot | Loading progress |
| MainMenu | Menu | Play / Settings / Quit |
| TeamSelect | Menu | Choose teams, formation |
| MatchSettings | Menu | Difficulty, half length, weather |
| HUD | Match | Scoreboard, clock, radar, stamina |
| PauseMenu | Match | Resume / Restart / Quit |
| Halftime | Match | Stats summary |
| Fulltime | Match | Final score, events, replay |
| Settings | Menu/Match | Audio, graphics, controls |

### 1.2 HUD Layout

```
┌─────────────────────────────────────────────────┐
│  [HOME 2]  67:23  [1 AWAY]     [POSSESSION BAR]  │
│                                                   │
│              GAME VIEW                            │
│                                                   │
│                                          ┌──────┐ │
│                                          │RADAR │ │
│                                          └──────┘ │
│  [STAMINA BAR]                       [PLAYER NAME]│
└─────────────────────────────────────────────────┘
```

### 1.3 Scoreboard

```csharp
public class ScoreboardUI : MonoBehaviour
{
    [SerializeField] TMP_Text homeScoreText;
    [SerializeField] TMP_Text awayScoreText;
    [SerializeField] TMP_Text clockText;
    [SerializeField] Image possessionBar;

    void OnEnable()
    {
        GameEvents.OnScoreChanged += UpdateScore;
        GameEvents.OnMatchTimeChanged += UpdateClock;
        GameEvents.OnPossessionChanged += UpdatePossession;
    }

    void UpdateScore(int home, int away) { ... }
    void UpdateClock(float time) { ... }
    void UpdatePossession(TeamSide side) { ... }
}
```

### 1.4 Mini-Radar

```
- Top-down 2D representation of field
- 22 dots (11 per team, different colors)
- Ball dot (white)
- Scale: field 105x68 → radar 150x97 pixels
- Update at 10Hz
```

### 1.5 Stamina Indicator

```
- Arc bar above controlled player
- Color: green (>50%) → yellow (20-50%) → red (<20%)
- Show on controlled player only
```

### 1.6 Main Menu Navigation

```
MainMenu
├── Play Match
│   ├── Team Select (Home)
│   ├── Team Select (Away)
│   ├── Formation Select
│   └── Match Settings
│       └── Kickoff
├── Settings
│   ├── Audio
│   ├── Graphics
│   └── Controls
└── Quit
```

### 1.7 UIManager

```csharp
public class UIManager : MonoBehaviour, IGameSystem
{
    public UIScreen CurrentScreen { get; private set; }
    public Dictionary<UIScreenType, UIScreen> Screens { get; }

    public void ShowScreen(UIScreenType type);
    public void HideScreen(UIScreenType type);
    public void ShowHUD(bool show);
    public void ShowPauseMenu();
    public void UpdateScoreboard(int home, int away, float time);
    public void UpdateRadar(List<PlayerEntity> allPlayers, BallEntity ball);
    public void ShowMessage(string text, float duration);
}
```

## 2. Camera System

### 2.1 Camera Modes

| Mode | Usage | Description |
|------|-------|-------------|
| Broadcast | Default play | Elevated side view, follows ball horizontally |
| Telephoto | Goal approach | Zoomed on ball carrier near goal |
| Touchline | Throw-ins | Low angle from touchline |
| Goal | After goal | Focus on scorer, celebration |
| Kickoff | Kickoff | Wide angle center field |
| Set Piece | Free kicks, corners | Positioned for set piece view |
| Replay | Goal replay | Multi-angle playback |

### 2.2 BroadcastCameraController

```csharp
public class BroadcastCameraController : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] float height = 25f;
    [SerializeField] float distance = 35f;      // behind play
    [SerializeField] float sideOffset = 0f;

    [Header("Following")]
    [SerializeField] float followSpeed = 3f;
    [SerializeField] float rotationSpeed = 2f;
    [SerializeField] float lookAhead = 5f;      // anticipate ball movement

    [Header("Bounds")]
    [SerializeField] float minX = -40f;
    [SerializeField] float maxX = 40f;
    [SerializeField] float minZ = -60f;
    [SerializeField] float maxZ = 60f;

    Transform target;  // ball or controlled player

    void LateUpdate()
    {
        if (target == null) return;

        // Target position with look-ahead
        Vector3 focus = target.position + target.forward * lookAhead;
        focus.x = Mathf.Clamp(focus.x, minX, maxX);
        focus.z = Mathf.Clamp(focus.z, minZ, maxZ);

        // Desired camera position
        Vector3 desired = focus + Vector3.up * height - Vector3.forward * distance;
        desired.x += sideOffset;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);

        // Look at focus
        Quaternion targetRot = Quaternion.LookRotation(focus - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget) { target = newTarget; }
    public void SetMode(CameraMode mode) { ... }
}
```

### 2.3 Camera Switching Logic

```
EvaluateCameraMode():
    if gameState == Kickoff:
        mode = Kickoff
    elif gameState == SetPiece:
        mode = SetPiece (based on type)
    elif gameState == Fulltime:
        mode = Goal (if recent goal) or Goal (celebration)
    elif ball near penalty box:
        mode = Telephoto
    elif event == Goal:
        mode = Goal (celebration, 5s)
    elif event == ThrowIn:
        mode = Touchline
    else:
        mode = Broadcast
```

### 2.4 Replay System

```
OnGoal:
    1. Save last 5 seconds of camera positions + ball positions
    2. Switch to Goal cam (celebration, 3s)
    3. Play replay from 3 angles:
       - Broadcast angle (real-time)
       - Behind goal (slow-mo 0.3x)
       - Side angle (slow-mo 0.5x)
    4. Resume normal camera
```

## 3. UI Prefab List

| Prefab | Components |
|--------|-----------|
| ScoreboardPanel | TMP_Text x3, Image (possession bar) |
| RadarPanel | RawImage (render texture) |
| StaminaArc | Image (filled arc) |
| PlayerNameTag | TMP_Text, Image (background) |
| MenuButton | Button, TMP_Text |
| TeamSelectCard | Image, TMP_Text, Toggle |
| FormationDiagram | Image, dots for positions |
| SettingsSlider | Slider, TMP_Text |
| MessagePopup | TMP_Text, Animator |
| PauseMenuPanel | Button x3, TMP_Text |