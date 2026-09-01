# fifaGame

FIFA-style 3D soccer game built with Unity 6 (Personal License).

## Tech Stack

- **Engine:** Unity 6 (6000.5.9f1, URP)
- **Language:** C# (.NET Standard 2.1)
- **Input:** Legacy Input Manager (Input System 패키지 호환성 이슈로 빌트인 사용)
- **Camera:** Custom broadcast camera controller
- **VCS:** Git + GitHub

## Multi-Agent Development

| Role | Model | Scope |
|------|-------|-------|
| Architect / QA | GLM-5.2 (Ollama Cloud) | Design, directives, QA testing |
| Implementation | Kimi-K3 (Ollama Cloud) | C# scripting, scene building, game logic |
| Audit / Assets | MiniMax-M3 (Ollama Cloud) | Asset review, project audit |

## Project Structure

```
fifaGame/
├── Assets/
│   ├── Scripts/
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Materials/
│   ├── Models/
│   ├── Animations/
│   ├── Audio/
│   ├── UI/
│   └── Settings/
├── docs/
│   ├── design/
│   └── wiki/
├── Packages/
└── ProjectSettings/
```

## License

Personal use only. All third-party assets must respect their respective licenses.