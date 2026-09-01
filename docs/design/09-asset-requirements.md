# Asset Requirements - Detailed Specification

## 1. 3D Models

### 1.1 Stadium

| Asset | Description | Poly Budget | Source |
|-------|-------------|-------------|--------|
| Stadium shell | Bowl, stands, roof structure | 50k tris | Unity Asset Store (free) / Blender |
| Pitch | Flat plane with grass texture | 500 tris | Unity terrain / custom mesh |
| Goal frame | 2 posts + crossbar + net | 2k tris | Blender (simple) |
| Corner flags | 4 flags | 200 tris | Blender |
| Field markings | Lines (center circle, penalty box, etc.) | Decal / texture | Texture |

### 1.2 Players

| Asset | Description | Poly Budget | Source |
|-------|-------------|-------------|--------|
| Player base mesh | Generic humanoid, low-poly | 8-12k tris | Mixamo / Asset Store |
| Player LOD1 | Reduced | 4k tris | Auto-generated |
| Player LOD2 | Low | 1.5k tris | Auto-generated |
| Goalkeeper mesh | Same body, different pose set | 8-12k tris | Mixamo |
| Hair/head variants | 5 hairstyles | 500 tris each | Blender |

### 1.3 Ball

| Asset | Description | Poly Budget | Source |
|-------|-------------|-------------|--------|
| Soccer ball | Standard pentagon pattern | 500 tris | Blender / Asset Store |

## 2. Animations

### 2.1 Player Animations (Mixamo)

| Animation | Mixamo Name | Duration | Retarget |
|-----------|-------------|----------|----------|
| Idle | "Idle" | loop | Yes |
| Walk | "Walk" | loop | Yes |
| Run | "Run" | loop | Yes |
| Sprint | "Sprint" | loop | Yes |
| Dribble idle | "Soccer Dribble Idle" | loop | Yes |
| Dribble forward | "Soccer Dribble" | loop | Yes |
| Pass | "Soccer Pass" | 0.5s | Yes |
| Shoot | "Soccer Shoot" | 0.8s | Yes |
| Tackle | "Soccer Tackle" | 0.6s | Yes |
| Header | "Header" | 0.5s | Yes |
| Jump | "Jump" | 0.8s | Yes |
| Goal celebration 1 | "Victory Dance" | 3s | Yes |
| Goal celebration 2 | "Cheer" | 2s | Yes |
| Goal celebration 3 | "Backflip" | 2.5s | Yes |
| Hurt/Fall | "Fall Backwards" | 1s | Yes |
| Turn left | "Turn Left" | 0.3s | Yes |
| Turn right | "Turn Right" | 0.3s | Yes |

### 2.2 Goalkeeper Animations

| Animation | Mixamo Name | Duration |
|-----------|-------------|----------|
| GK Idle | "Goalkeeper Idle" | loop |
| GK Ready | "Goalkeeper Ready" | loop |
| GK Dive Left | "Goalkeeper Dive Left" | 0.8s |
| GK Dive Right | "Goalkeeper Dive Right" | 0.8s |
| GK Jump | "Goalkeeper Jump" | 0.8s |
| GK Catch | "Goalkeeper Catch" | 0.6s |
| GK Goal Kick | "Goalkeeper Goal Kick" | 1s |
| GK Throw | "Goalkeeper Throw" | 0.6s |

### 2.3 Animation Controller

```
PlayerAnimator.controller
├── Base Layer: Locomotion
│   ├── Idle (blend)
│   ├── Walk (blend)
│   ├── Run (blend)
│   ├── Sprint (blend)
│   └── Dribble (blend tree, speed × direction)
├── Action Layer: Actions
│   ├── Pass (trigger)
│   ├── Shoot (trigger)
│   ├── Tackle (trigger)
│   ├── Header (trigger)
│   └── Celebration (trigger)
└── GK Layer (goalkeepers only)
    ├── GK Idle
    ├── GK Ready
    ├── GK Dive L/R (trigger)
    ├── GK Jump (trigger)
    └── GK Catch (trigger)
```

## 3. Textures & Materials

### 3.1 Field

| Texture | Resolution | Maps | Source |
|---------|-----------|------|--------|
| Grass | 2048x2048 | Albedo, Normal | CC0 texture / generated |
| Field lines | 1024x1024 | Albedo (alpha) | Generated |

### 3.2 Players

| Texture | Resolution | Maps | Source |
|---------|-----------|------|--------|
| Skin | 1024x1024 | Albedo, Normal | CC0 |
| Kit (home/away) | 1024x1024 | Albedo | Generated per team color |
| Shoes | 256x256 | Albedo | CC0 |

### 3.3 Stadium

| Texture | Resolution | Source |
|---------|-----------|--------|
| Stand seats | 512x512 | CC0 |
| Roof | 512x512 | CC0 |
| Advertising boards | 1024x256 | Generated (generic) |

### 3.4 Ball

| Texture | Resolution | Source |
|---------|-----------|--------|
| Ball pattern | 512x512 | Generated / CC0 |

## 4. UI Assets

| Asset | Size | Format | Source |
|-------|------|--------|--------|
| Menu background | 1920x1080 | PNG | Generated |
| Team logos (generic) | 128x128 | PNG | Generated |
| Formation diagram dots | 32x32 | PNG | Generated |
| Scoreboard frame | 512x128 | PNG | Generated |
| Buttons (normal/hover/pressed) | 256x64 | PNG | Generated |
| Slider handle | 32x32 | PNG | Generated |
| Stamina arc | 128x128 | PNG | Generated |
| Radar frame | 256x128 | PNG | Generated |
| Flag icons | 64x64 | PNG | Generated |

## 5. Audio Assets

| Asset | Format | Duration | Source |
|-------|--------|----------|--------|
| Kick (5 variations) | WAV | 0.2s each | freesound.org CC0 |
| Tackle (3 variations) | WAV | 0.3s each | freesound.org CC0 |
| Whistle short | WAV | 0.3s | freesound.org CC0 |
| Whistle long | WAV | 0.8s | freesound.org CC0 |
| Crowd ambient | WAV/OGG | 30s loop | freesound.org CC0 |
| Crowd cheer | WAV | 3s | freesound.org CC0 |
| Crowd boo | WAV | 2s | freesound.org CC0 |
| Menu BGM | OGG | 60s loop | CC0 or generated |
| Match intro | OGG | 10s | CC0 |

## 6. Asset Procurement Plan

### 6.1 Free Sources

| Source | Assets | URL |
|--------|--------|-----|
| Mixamo | Player + GK animations | https://www.mixamo.com |
| Unity Asset Store (free) | Stadium, misc | https://assetstore.unity.com (free filter) |
| freesound.org | All SFX | https://freesound.org (CC0 filter) |
| Kenney.nl | UI elements | https://kenney.nl |
| OpenGameArt.org | Textures, misc | https://opengameart.org (CC0) |
| Poly Pizza | 3D models | https://poly.pizza |

### 6.2 Procedural Generation

| Asset | Method | Tool |
|-------|--------|------|
| Field lines | Script-generated texture | C# / Krita |
| Team kits | Material with color parameter | Unity Shader |
| UI elements | Script-generated / Krita | Krita |
| Team logos | Simple geometric shapes | Krita |
| Ball pattern | UV-mapped texture | Blender |

## 7. Asset Audit Checklist (for minimax-m3)

- [ ] All assets use CC0 or compatible free license
- [ ] No trademarked logos or player likenesses
- [ ] Poly counts within budget
- [ ] Texture sizes within budget
- [ ] Animations properly retargeted
- [ ] Audio files are 16-bit, 44.1kHz (or 48kHz)
- [ ] All assets referenced in scripts exist in project
- [ ] No missing references in prefabs
- [ ] Asset naming convention consistent
- [ ] Folder structure matches design doc