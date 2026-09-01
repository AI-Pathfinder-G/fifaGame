# Data Model & Audio System - Detailed Design

## 1. Data Model

### 1.1 TeamData

```csharp
[CreateAssetMenu(fileName = "Team", menuName = "Soccer/Team")]
public class TeamData : ScriptableObject
{
    [Header("Identity")]
    public string TeamName;
    public string ShortName;           // 3-letter abbreviation
    public Sprite Logo;
    public Color PrimaryColor;
    public Color SecondaryColor;

    [Header("Formation")]
    public FormationData DefaultFormation;

    [Header("Roster")]
    public PlayerData[] StartingXI;    // 11 players
    public PlayerData[] Substitutes;   // 7-12 subs

    [Header("Ratings")]
    public int AttackRating;
    public int MidfieldRating;
    public int DefenseRating;
    public int OverallRating;

    [Header("Strategy")]
    public TeamStrategyData PreferredStrategy;
}
```

### 1.2 PlayerData

```csharp
[CreateAssetMenu(fileName = "Player", menuName = "Soccer/Player")]
public class PlayerData : ScriptableObject
{
    [Header("Identity")]
    public string PlayerName;
    public int FieldNumber;
    public PositionRole PreferredRole;
    public Sprite Portrait;

    [Header("Physical")]
    public float Height;      // meters
    public float Weight;      // kg

    [Header("Attributes (1-99)")]
    public PlayerStats Stats;

    [Header("Role")]
    public bool IsGoalkeeper;
}
```

### 1.3 PlayerStats

```csharp
[Serializable]
public struct PlayerStats
{
    // Physical
    public int Pace;          // speed
    public int Acceleration;
    public int Stamina;
    public int Strength;
    public int Jumping;

    // Technical
    public int Shooting;
    public int Finishing;
    public int Passing;
    public int Crossing;
    public int Dribbling;
    public int BallControl;
    public int Technique;

    // Defensive
    public int Defending;
    public int Tackling;
    public int Heading;
    public int Interception;

    // Mental
    public int Vision;
    public int Composure;
    public int Aggression;
    public int Positioning;

    // Goalkeeper
    public int GKReflexes;
    public int GKDiving;
    public int GKHandling;
    public int GKKicking;
    public int GKPositioning;

    public int Overall()
    {
        // Weighted average based on position
        // Simplified: average of all relevant stats
        return (Pace + Shooting + Passing + Dribbling + Defending + Physical) / 6;
    }
}
```

### 1.4 FormationData

```csharp
[CreateAssetMenu(fileName = "Formation", menuName = "Soccer/Formation")]
public class FormationData : ScriptableObject
{
    public string Name;                   // "4-4-2"
    public FormationSlot[] Slots;         // 11 slots

    public Vector3 GetWorldPosition(int slotIndex, float fieldLength, float fieldWidth, TeamSide side)
    {
        var slot = Slots[slotIndex];
        Vector3 pos = new Vector3(
            (slot.BasePosition.x - 0.5f) * fieldWidth,
            0,
            (slot.BasePosition.y - 0.5f) * fieldLength
        );
        if (side == TeamSide.Away) pos.z = -pos.z;  // mirror for away
        return pos;
    }
}

[Serializable]
public struct FormationSlot
{
    public PositionRole Role;
    public Vector2 BasePosition;    // normalized 0..1 (x=width, y=length)
    public float AttackBias;
    public float DefenseBias;
}
```

### 1.5 Enums

```csharp
public enum TeamSide { Home, Away, None }
public enum PositionRole { GK, LB, LCB, CB, RCB, RB, LM, LCM, CM, RCM, RM, LW, LF, CF, RF, RW, ST }
public enum GamePhase { Attack, Defense, TransitionToAttack, TransitionToDefense, SetPiece }
public enum MentalityType { VeryDefensive, Defensive, Balanced, Attacking, VeryAttacking }
```

## 2. Audio System

### 2.1 AudioManager

```csharp
public class AudioManager : MonoBehaviour, IGameSystem
{
    [Header("Mixers")]
    [SerializeField] AudioMixer masterMixer;

    [Header("Sources")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource crowdSource;
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource commentarySource;

    [Header("Audio Clips")]
    [SerializeField] AudioClip[] kickSounds;
    [SerializeField] AudioClip[] tackleSounds;
    [SerializeField] AudioClip whistleShort;
    [SerializeField] AudioClip whistleLong;
    [SerializeField] AudioClip goalCrowd;
    [SerializeField] AudioClip crowdAmbient;
    [SerializeField] AudioClip[] crowdReactions;
    [SerializeField] AudioClip menuBGM;
    [SerializeField] AudioClip matchBGM;

    public void PlayKick(float power) { ... }
    public void PlayTackle() { ... }
    public void PlayWhistle(bool longWhistle) { ... }
    public void PlayGoalCrowd() { ... }
    public void SetCrowdIntensity(float intensity) { ... }  // 0..1
    public void PlayBGM(AudioClip clip) { ... }
    public void SetVolume(string channel, float value) { ... }
}
```

### 2.2 Sound Categories

| Category | Source | Volume Control | Trigger |
|----------|--------|---------------|---------|
| SFX | sfxSource | "SFXVolume" | Game events |
| Crowd | crowdSource | "CrowdVolume" | Continuous + reactions |
| BGM | bgmSource | "BGMVolume" | Menu/match |
| Commentary | commentarySource | "CommentaryVolume" | Key events |

### 2.3 Event → Sound Mapping

| Event | Sound | Pitch Variation |
|-------|-------|-----------------|
| Ball kicked | kickSounds[random] | +5% per power level |
| Tackle | tackleSounds[random] | 0.95-1.05 |
| Foul whistle | whistleShort | 1.0 |
| Goal whistle | whistleLong | 1.0 |
| Kickoff | whistleShort | 1.0 |
| Halftime | whistleLong x2 | 1.0 |
| Fulltime | whistleLong x3 | 1.0 |
| Goal scored | goalCrowd | 1.0 |
| Near miss | crowdReactions[0] | 1.0 |
| Save | crowdReactions[1] | 1.0 |
| Attack buildup | crowd intensity ↑ | - |
| Defense pressure | crowd intensity ↑ | - |

### 2.4 Crowd Dynamics

```
baseIntensity = 0.3
+ 0.2 if home team attacking
+ 0.3 if near goal
+ 0.4 on goal
- 0.1 if away team attacking

crowdSource.volume = baseIntensity * crowdVolumeSetting
```

### 2.5 Audio Assets Needed

| Asset | Source | License |
|-------|--------|---------|
| Kick sounds (5) | freesound.org | CC0 |
| Tackle sounds (3) | freesound.org | CC0 |
| Whistle (short/long) | freesound.org | CC0 |
| Crowd ambient | freesound.org | CC0 |
| Crowd reactions (5) | freesound.org | CC0 |
| Menu BGM | CC0 or generated | CC0 |
| Match BGM (optional) | CC0 or none | - |