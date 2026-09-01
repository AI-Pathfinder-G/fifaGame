# Player System - Detailed Design

## 1. Purpose

Handles all player-related logic: movement, actions (pass, shoot, dribble, tackle), input mapping, animation triggering, and stat-driven behavior.

## 2. Class Hierarchy

```
MonoBehaviour
├── PlayerEntity          # Data container + references
├── PlayerController      # Active player (user-controlled)
├── PlayerAI              #Inactive player (AI-controlled teammate)
├── GoalkeeperController  # GK-specific control (user + AI)
└── PlayerAnimator        # Animation bridge
```

## 3. PlayerEntity

```csharp
public class PlayerEntity : MonoBehaviour
{
    public PlayerData Data { get; set; }
    public TeamSide Team { get; set; }
    public PositionRole Role { get; set; }
    public int FieldNumber { get; set; }

    public Rigidbody Rigidbody { get; }
    public Animator Animator { get; }
    public PlayerAnimator AnimBridge { get; }

    public bool IsUserControlled { get; set; }
    public bool HasBall { get; set; }
    public float Stamina { get; set; }   // 0..1
    public bool IsSprinting { get; set; }

    public Vector3 FormationPosition { get; set; }  // home position
    public Vector3 TargetPosition { get; set; }     // AI target
}
```

## 4. PlayerController (User Input)

### 4.1 Input Mapping (Legacy Input)

```csharp
// Movement
float moveX = Input.GetAxis("Horizontal");
float moveZ = Input.GetAxis("Vertical");

// Sprint
bool sprint = Input.GetKey(KeyCode.LeftShift);

// Actions
bool pass      = Input.GetKeyDown(KeyCode.A);       // short pass
bool through   = Input.GetKeyDown(KeyCode.Q);       // through pass
bool shoot     = Input.GetKeyDown(KeyCode.S);       // shoot
bool cross     = Input.GetKeyDown(KeyCode.E);       // lob/cross
bool tackle    = Input.GetKeyDown(KeyCode.D);       // tackle
bool switchP   = Input.GetKeyDown(KeyCode.Tab);     // switch player
bool skill     = Input.GetKey(KeyCode.F);           // skill move modifier
```

### 4.2 Movement Model

```csharp
// Base speed from stats
float baseSpeed = Data.Stats.Pace.MapToSpeed();  // 50..99 → 5..8 m/s
float speed = IsSprinting ? baseSpeed * 1.4f : baseSpeed;

// Stamina drain
if (IsSprinting) Stamina = Mathf.Max(0, Stamina - sprintDrain * dt);

// Direction from camera-relative input
Vector3 camForward = cameraTransform.forward.FlattenY();
Vector3 camRight = cameraTransform.right.FlattenY();
Vector3 moveDir = (camForward * moveZ + camRight * moveX).normalized;

// Apply
Rigidbody.linearVelocity = moveDir * speed;
```

### 4.3 Action System

| Action | Trigger | Power | Accuracy | Target |
|--------|---------|-------|----------|--------|
| Short Pass | A | Hold duration (0.1-1.0s) | Based on Passing stat | Nearest teammate in direction |
| Through Pass | Q | Hold duration | Passing stat × 0.9 | Leading space in direction |
| Shoot | S | Hold duration (0.1-2.0s) | Shooting stat | Goal direction |
| Lob/Cross | E | Hold duration | Passing stat × 0.8 | Target area (penalty box) |
| Tackle | D | Instant | Defending stat | Nearest opponent with ball |
| Skill Move | F + direction | Instant | Dribbling stat | Evade opponent |

### 4.4 Ball Control

- **Dribble**: Ball stays within 0.5-1.5m of player, pushed forward each frame
- **First Touch**: Ball decelerates on reception based on Technique stat
- **Ball loss**: If opponent within tackle range and wins tackle check → possession change

## 5. PlayerAI (Non-controlled)

### 5.1 Decision Tree

```
IF has_ball:
  IF near_goal AND shot_chance:
    → Shoot
  IF teammate_open AND pass_lane_clear:
    → Pass
  IF space_ahead:
    → Dribble forward
  ELSE:
    → Hold and shield
ELSE IF team_has_ball:
  IF am_receiver:
    → Move to open space
  ELSE:
    → Maintain formation, shift toward play
ELSE (opponent has_ball):
  IF am_nearest_defender:
    → Press ball carrier
  IF am_second_defender:
    → Cover space
  ELSE:
    → Hold formation, mark zone
```

### 5.2 Formation Positioning

Each player has a `FormationPosition` (home spot). AI moves toward:
```
targetPos = formationPos + formationOffset(ballPos, gamePhase)
```

Offset scales with:
- Attack phase: push forward
- Defense phase: drop back
- Ball side: shift toward ball

### 5.3 AI Difficulty

| Level | Reaction Time | Decision Quality | Error Rate |
|-------|--------------|-----------------|------------|
| Easy | 0.5s | Basic heuristics | 30% |
| Medium | 0.3s | Good heuristics | 15% |
| Hard | 0.15s | Advanced + prediction | 5% |

## 6. GoalkeeperController

### 6.1 Positioning

```
gkPos = goalCenter + clamp(ballPos - goalCenter) * reactionFactor
gkPos.y = goalLine
```

### 6.2 Save Logic

- Dive trigger: shot within X° and Y range
- Save chance: based on GK Rating vs Shot Power/Placement
- Catch vs parry: based on GK Catching stat

## 7. PlayerAnimator

### 7.1 Parameters

| Param | Type | Purpose |
|-------|------|---------|
| Speed | Float | Blend idle/walk/run/sprint |
| Direction | Float | Blend left/forward/right |
| HasBall | Bool | Switch to dribbling anims |
| IsSprinting | Bool | Sprint blend |
| TriggerPass | Trigger | Play pass animation |
| TriggerShoot | Trigger | Play shot animation |
| TriggerTackle | Trigger | Play tackle animation |
| TriggerJump | Trigger | GK dive/jump |
| TriggerCelebrate | Trigger | Goal celebration |

### 7.2 Animation States

```
Locomotion
├── Idle
├── Walk
├── Run
├── Sprint
├── Dribble (blend tree)
├── Pass
├── Shoot
├── Tackle
├── Header
└── Celebration
```

## 8. Stamina System

| State | Drain/sec | Regen/sec |
|-------|-----------|-----------|
| Idle | 0 | +0.05 |
| Walk | 0 | +0.03 |
| Run | 0.02 | 0 |
| Sprint | 0.08 | 0 |
| Recovery (stamina < 0.2) | forced walk | +0.04 |

Stamina < 0.2 → cannot sprint; < 0.05 → forced walk.