# Ball Physics System - Detailed Design

## 1. Purpose

Simulates realistic soccer ball behavior: trajectory, spin (Magnus effect), bounce, rolling friction, and collision with players/surfaces.

## 2. BallEntity

```csharp
public class BallEntity : MonoBehaviour
{
    public Rigidbody Rigidbody { get; }
    public SphereCollider Collider { get; }
    public Vector3 Velocity { get; }
    public Vector3 Spin { get; }          // angular velocity (rad/s)
    public BallState State { get; }        // Rolling, Flying, Dead, InPossession
    public PlayerEntity Owner { get; set; }
}
```

## 3. Physics Model

### 3.1 Flight (Airborne)

```
// Forces
F_gravity = (0, -9.81 * mass, 0)
F_drag = -0.47 * ρ * A * |v| * v        // quadratic drag
F_magnus = S * (ω × v)                   // spin force

// Integration (semi-implicit Euler)
v += (F_total / mass) * dt
pos += v * dt
```

Constants:
- mass = 0.43 kg (regulation ball)
- radius = 0.11 m
- ρ (air) = 1.225 kg/m³
- A = π * r² = 0.038 m²
- S (spin coeff) ≈ 0.01

### 3.2 Ground Rolling

```
F_friction = -μ * m * g * v_hat          // rolling friction
μ = 0.02 (grass)

// When |v| < 0.3 m/s → state = Dead
```

### 3.3 Bounce

```
// On ground contact (y < radius):
v.y = -v.y * restitution        // e = 0.35 for grass
v.xz *= 0.85                     // horizontal damping on bounce
```

### 3.4 Spin Decay

```
ω *= (1 - spinDrag * dt)         // spinDrag = 0.5
```

## 4. Pass Physics

### 4.1 Short Pass

```
power = holdTime.Map(0.1s..1.0s → 5..18 m/s)
launchAngle = 5° (slightly lifted)
targetVel = direction * power + up * sin(5°) * power
spin = slight backspin for stability
```

### 4.2 Through Pass

```
power = holdTime.Map(0.1s..1.0s → 8..22 m/s)
target = leadPoint (space ahead of receiver)
ground pass (no lift)
spin = forward spin (topspin) for speed
```

### 4.3 Shot

```
power = holdTime.Map(0.1s..2.0s → 10..30 m/s)
launchAngle = 10-25° based on aim
accuracy = ShootingStat / 99
errorAngle = (1 - accuracy) * maxError * Random(-1, 1)  // maxError = 8°
spin = sidespin based on curve input
```

### 4.4 Lob/Cross

```
power = holdTime.Map(0.1s..1.5s → 8..20 m/s)
launchAngle = 35-45°
target = penalty area
spin = backspin for hang time
```

## 5. Possession System

### 5.1 Ball Attachment

When a player has possession:
```
ball.Position = player.Position + playerForward * 0.5m
ball.Velocity = player.Velocity + dribblePush
ball.State = InPossession
```

### 5.2 Possession Check

Each frame, for each player near ball (< 1.5m):
```
if (player.HasBall):
    // Check tackle from opponents
    for opponent in opponentsWithin2m:
        if opponent triggers tackle:
            tackleSuccess = random() < (opponent.Defending / (opponent.Defending + player.Dribbling))
            if tackleSuccess:
                transferPossession(opponent, player)
```

### 5.3 Ball Reception

When ball enters player control zone (radius 1.5m, height 1.0m):
```
controlChance = Technique / 99
if random() < controlChance:
    player.HasBall = true
    ball.Velocity = dampenedToPlayerSpeed
else:
    ball deflects slightly (bad touch)
```

## 6. Collision Layers

| Layer | Collides With |
|-------|---------------|
| Ball | Players, Ground, GoalPosts, Net |
| Players | Players (soft), Ball |
| GoalPost | Ball (solid bounce, e=0.7) |

## 7. Goal Detection

```
// Trigger zone in goal mouth
OnTriggerEnter(Ball):
    if ball crosses goalLine within post width and under crossbar:
        GoalEvent(team)
```

## 8. Out of Bounds

```
// Touchline / goal line planes
if ball.x > fieldWidth/2 or ball.x < -fieldWidth/2:
    ThrowIn or Corner
if ball.z > fieldLength/2 or ball.z < -fieldLength/2:
    Goal kick or Corner
```

## 9. Field Dimensions (scaled)

```
fieldLength = 105m (z axis)
fieldWidth  = 68m  (x axis)
goalWidth   = 7.32m
goalHeight  = 2.44m
penaltyBox  = 16.5m deep, 40.3m wide
goalArea    = 5.5m deep, 18.32m wide
centerCircle = 9.15m radius
```

Scale factor: 1 Unity unit = 1 meter.