# fifaGame - LLM Wiki

## Overview

This is the project knowledge base for the FIFA-style soccer game. All design documents are maintained here and in `docs/design/`.

## Document Index

| # | Document | Description |
|---|----------|-------------|
| 1 | [Architecture Overview](../design/01-architecture.md) | Tech stack, project structure, system layers |
| 2 | [Core Game Manager](../design/02-core-game-manager.md) | State machine, system lifecycle, event bus |
| 3 | [Player System](../design/03-player-system.md) | Player control, input, actions, stamina |
| 4 | [Ball Physics](../design/04-ball-physics.md) | Trajectory, spin, bounce, possession |
| 5 | [AI System](../design/05-ai-system.md) | Team AI, formations, decision trees, GK AI |
| 6 | [Match & Referee](../design/06-match-referee.md) | Rules, fouls, cards, set pieces, stats |
| 7 | [UI & Camera](../design/07-ui-camera.md) | HUD, menus, broadcast camera, replay |
| 8 | [Data & Audio](../design/08-data-audio.md) | Team/player data, audio system |
| 9 | [Asset Requirements](../design/09-asset-requirements.md) | Models, animations, textures, audio specs |

## Multi-Agent Roles

| Role | Model | Responsibility |
|------|-------|----------------|
| Architect / QA | GLM-5.2 (Ollama) | Design, task directives, QA testing |
| Implementation | Kimi-K3 (Ollama) | C# scripting, Unity scene building |
| Audit / Assets | MiniMax-M3 (Ollama) | Asset review, project audit |

## Development Phases

1. ✅ GitHub repository setup
2. ✅ Project scaffold (Unity 6 + URP)
3. ✅ Detailed design documents
4. ✅ Wiki registration
5. 🔄 Implementation (kimi-k3) + Audit (minimax-m3)
6. ⬜ QA testing (glm-5.2)
7. ⬜ User play guide

## Key Decisions

- **Engine:** Unity 6 (6000.5.9f1) with URP
- **Input:** Legacy Input Manager (Input System package incompatible with 6000.5.9f1)
- **Camera:** Custom broadcast controller (Cinemachine 3.1.3 incompatible)
- **Platform:** Windows 64-bit standalone
- **License:** Unity Personal, all assets CC0/free