# fifaGame - Architecture Design Document

## 1. Project Overview

A FIFA-style 3D soccer game built with Unity 6, targeting PC (Windows) as primary platform.

### 1.1 Design Goals
- Realistic soccer match experience (11v11)
- Player control with responsive input (pass, shoot, dribble, tackle, sprint)
- AI-controlled teammates and opponents with positional awareness
- Broadcast-style camera system
- Match management (score, time, fouls, offsides, set pieces)
- Menu system (team selection, formation, match settings)

### 1.2 Non-Goals (MVP scope)
- Online multiplayer (single-player vs AI only)
- Career mode / player progression
- Licensed teams / players (generic teams)
- Microtransactions

## 2. Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Engine | Unity | 6000.5.9f1 |
| Render Pipeline | URP (Universal Render Pipeline) | 17.2.0 |
| Input | Legacy Input Manager | Built-in |
| Camera | Custom (BroadcastCameraController) | Built-in |
| UI | uGUI + TextMeshPro | 2.0.0 / 3.0.7 |
| Language | C# | .NET Standard 2.1 |
| VCS | Git + GitHub | - |

## 3. Project Structure

```
UnityProject/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # Game management, state machine
│   │   ├── Player/         # Player control, input handling
│   │   ├── AI/             # AI behavior, decision making
│   │   ├── Ball/           # Ball physics, trajectory
│   │   ├── Match/          # Match rules, referee, scoring
│   │   ├── Camera/         # Camera system, broadcast logic
│   │   ├── UI/             # Menu, HUD, scoreboard
│   │   ├── Data/           # Team data, player stats, formations
│   │   ├── Audio/          # Sound management
│   │   └── Utils/          # Math helpers, extensions
│   ├── Scenes/
│   │   ├── Boot.unity      # Initialization, loading
│   │   ├── Menu.unity      # Main menu
│   │   └── Match.unity     # Gameplay scene
│   ├── Prefabs/
│   │   ├── Player.prefab
│   │   ├── Ball.prefab
│   │   ├── Stadium.prefab
│   │   └── UI/
│   ├── Materials/
│   ├── Models/
│   ├── Animations/
│   │   ├── Player/
│   │   └── Goalkeeper/
│   ├── Audio/
│   │   ├── SFX/
│   │   └── BGM/
│   ├── UI/
│   │   ├── Sprites/
│   │   └── Fonts/
│   └── Settings/
│       └── URP/
├── Packages/
└── ProjectSettings/
```

## 4. Core Architecture

### 4.1 Pattern: State-Driven Game Manager

```
GameManager (Singleton)
├── BootState      → Load assets, initialize systems
├── MenuState      → Team select, formation, settings
├── KickoffState   → Setup teams, place players, kickoff
├── PlayState      → Active match simulation
├── SetPieceState  → Free kick, corner, throw-in
├── HalftimeState  → Break, stats display
└── FulltimeState  → Final score, replay, menu return
```

### 4.2 System Layers

```
┌─────────────────────────────────┐
│         UI / HUD Layer          │  Scoreboard, radar, menu
├─────────────────────────────────┤
│      Camera System (Cinemachine)│  Broadcast cam, follow cam
├─────────────────────────────────┤
│     Match Manager (Rules)       │  Score, time, fouls, offside
├─────────────────────────────────┤
│  Player System    │  AI System  │  Control + behavior
├─────────────────────────────────┤
│       Ball Physics System       │  Trajectory, spin, bounce
├─────────────────────────────────┤
│     Animation System            │  Animator + Rigging
├─────────────────────────────────┤
│        Audio System             │  SFX + BGM + Crowd
└─────────────────────────────────┘
```

### 4.3 Key Scripts (planned)

| Script | Responsibility |
|--------|---------------|
| `GameManager.cs` | State machine, orchestrates all systems |
| `MatchManager.cs` | Score, match clock, half management |
| `PlayerController.cs` | Input → movement, actions |
| `PlayerAI.cs` | AI decision tree for non-controlled players |
| `GoalkeeperAI.cs` | Specialized GK behavior |
| `BallPhysics.cs` | Ball trajectory, collision, spin |
| `FormationManager.cs` | Position assignment per formation |
| `CameraController.cs` | Broadcast camera logic |
| `InputManager.cs` | Input System wrapper |
| `UIManager.cs` | HUD, scoreboard, menu navigation |
| `RefereeSystem.cs` | Foul detection, offside, cards |
| `TeamData.cs` | Team roster, ratings, formation |
| `AudioManager.cs` | Crowd, whistle, kick sounds |

## 5. Input Scheme

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD | Left Stick |
| Sprint | Shift (hold) | RT (hold) |
| Pass | A | A / Cross |
| Through Pass | Q | Y / Triangle |
| Shoot | S | B / Circle |
| Lob/Cross | E | X / Square |
| Tackle | D | RB / R1 |
| Switch Player | Tab | LB / L1 |
| Sprint + Skill | Shift + F | RS click |
| Pause | Esc | Start |

## 6. Data Model

### 6.1 Team
- TeamName, TeamColors (primary/secondary), Formation (4-4-2, 4-3-3, etc.)
- Roster: List of PlayerData (11 starters + subs)

### 6.2 PlayerData
- Name, Position (GK/DEF/MID/FWD), OverallRating (1-99)
- Stats: Pace, Shooting, Passing, Dribbling, Defending, Physical
- Number, MeshColor

### 6.3 Formation
- FormationName, Positions: List of (role, fieldPosition)
- Dynamic adjustment based on ball possession

## 7. Physics & Collision

- **Ball**: Sphere collider + custom trajectory (spin, dip, lift)
- **Players**: Capsule collider + Rigidbody (kinematic for AI, dynamic for controlled)
- **Tackle detection**: Trigger zones, timing-based
- **Goal detection**: Trigger zone in goal mouth

## 8. Performance Targets

| Metric | Target |
|--------|--------|
| Frame rate | 60 FPS (1080p) |
| Draw calls | < 2000 |
| Player poly count | < 15k per player |
| Stadium LOD | 3 levels |

## 9. Build & Deployment

- **Platform**: Standalone Windows 64-bit
- **Build method**: Unity CLI batch mode (`unity build --target StandaloneWindows64`)
- **CI**: GitHub Actions (future)

## 10. Development Phases

| Phase | Scope | Owner |
|-------|-------|-------|
| Phase 1 | Core framework + match scene + player movement | kimi-k3 |
| Phase 2 | Ball physics + passing + shooting | kimi-k3 |
| Phase 3 | AI system + formation | kimi-k3 |
| Phase 4 | UI + menu + HUD | kimi-k3 |
| Phase 5 | Audio + polish + camera | kimi-k3 |
| Audit | Asset review + code audit + project audit | minimax-m3 |
| QA | Playtest + bug report + fix verification | glm-5.2 |