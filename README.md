# DEAD AIR — Technical Documentation

**Genre**: Narrative Horror  
**Engine**: Unity 2021 LTS+  
**Narrative Engine**: Ink (Inkle)

*Dead Air — A project by Michele Grimaldi | E-C-H-O SYSTEMS*

---

![Project Cover](Logo.png)
## DEAD AIR — Game Concept Reference Documents
- [DEAD_AIR_STORY_ARCHITECTURE](https://docs.google.com/document/d/1fydiwT6h3TYMvayOMdnoAXsqVEKncyPSYEauwPCPtxY/edit?tab=t.0#heading=h.n71ap8gqp083)
- [DEAD_AIR_CONCEPT](https://docs.google.com/document/d/19lzCzj4KluC-9Iayi9aIQmPGrRT4fcdhMTGNEUfs3tg/edit?tab=t.0)
- [DEAD AIR - Scene 1](https://docs.google.com/document/d/1AXRHu3tBfq9NYKhXJQnWr8wlpkkn18FYZ-0bdrgomoQ/edit?tab=t.0)
- [DEAD AIR - Observer Pattern Functions and Role](https://docs.google.com/document/d/1A6QolH8ZfMb7hAVnUAIxZwwX6ydyHTyNzoR31VUbvjw/edit?tab=t.0#heading=h.a0snu6hph57e)
- [DEAD AIR - Main Menu State Pattern](https://docs.google.com/document/d/1MioJsxffXy8tvQUZbcWDAk_i-x8QAun_lKWvMFsGUeM/edit?tab=t.0)
- [DEAD AIR's Feedback](https://docs.google.com/document/d/138J-VQ95gjfxAhUd8jjPW2hQqQFlGHhzHVayldid5QM/edit?tab=t.0)
- [DEAD AIR - Light GDD](https://docs.google.com/document/d/1gGWbE4u4KHXm7_cr_XzGU_ay5Biud0Npwt2-_DqQq9Y/edit?tab=t.0#heading=h.a1t2pnmr2g2)

---

## TABLE OF CONTENTS
1. [What the Game Does](#1-what-the-game-does)
2. [How the Code Works](#2-how-the-code-works)
3. [File Structure](#3-file-structure)
   - [Audio System Architecture](#35-audio-system-architecture-scriptableobject-libraries)
4. [Event Channels](#4-event-channels-communication-system)
5. [How to Write a Story](#5-how-to-write-a-story)
6. [How to Add a New Tag](#6-how-to-add-a-new-tag-eg-music)
   - [How to Add a New UI Command](#65-how-to-add-a-new-ui-command-type-safe-enums)
7. [System Visual Schema](#7-system-visual-schema)
8. [Systems and Their Roles](#8-systems-and-their-roles)
9. [Troubleshooting](#9-troubleshooting)
10. [Quick Reference](#10-quick-reference)
11. [Upcoming Improvements](#11-upcoming-improvements)
12. [Contact](#12-contact)
13. [Audio Optimization](#13-audio-optimization)

---

## 1. WHAT THE GAME DOES

You are a 911 operator in the 1990s. You answer emergency calls that grow increasingly disturbing.

**Gameplay**:
1. Choose a call from the menu
2. Listen and read the dialogue
3. Choose how to respond
4. The story continues based on your choices
- **Note**: Unity automatically optimizes audio based on folder location (see Section 13).

---

## 2. HOW THE CODE WORKS

### 2.1 Base Architecture

The game uses a **tag-driven** system: you write the story in `.ink` files, add special tags, and the game reacts automatically.

```
INK FILE (story + tags)
    ↓
PARSER (reads the tags)
    ↓
CHANNELS (inter-system communication)
    ↓
MANAGER (audio, UI, voice)
    ↓
IN-GAME EFFECT
```

**Practical example**:
```ink
911, what's your emergency? # speaker:ward # voice:ward_01

Text appears on screen + character voice audio plays
```

---

### 2.2 Available Tags

| Tag | What It Does | Example |
|-----|--------------|---------|
| `#speaker:{name}` | Changes text color | `#speaker:iris` |
| `#voice:{file}` | Plays character voice | `#voice:iris_01` |
| `#sfx:{file}` | Sound effect | `#sfx:phone_ring` |
| `#amb:{file}` | Ambient music (loop) | `#amb:dispatch_night` |
| `#amb:stop` | Stops ambient music | `#amb:stop` |
| `#ui:{command}` | Special UI command | `#ui:dead_air_screen` |

**Note (from March 2026)**: The `#speaker` and `#ui` tags internally use **type-safe enums** (`SpeakerType`, `UICommandType`) to prevent errors and improve maintainability. See [Section 6.5](#65-how-to-add-a-new-ui-command-type-safe-enums) for details.

---

## 3. FILE STRUCTURE

```
Assets/
├── Scripts/
│   ├── Narrative/
│   │   ├── StoryManager.cs          → Loads Ink and coordinates everything
│   │   └── DialogueParser.cs        → Reads tags from the Ink file
│   │
│   ├── UI/
│   │   ├── DialogueUI.cs            → Displays text and choices
│   │   └── ChoiceButton.cs          → Button for player choices
│   │
│   ├── Audio/
│   │   ├── AudioManager.cs          → SFX and Ambience
│   │   └── VoiceManager.cs          → Character voices
│   │
│   └── Events/
│       ├── Channels/                → Communication types
│       │   ├── StringEventChannel.cs
│       │   ├── VoidEventChannel.cs
│       │   └── ... (others)
│       │
│       └── ScriptableObjects/       → Communication channels (14 .asset files)
│           ├── DialogueLineChannel.asset
│           ├── SFXRequestedChannel.asset
│           └── ... (others)
│
├── Ink/
│   └── dead_air_demo_en.ink         → Main story
│
├── Audio
```

---

## 3.5 AUDIO SYSTEM ARCHITECTURE (ScriptableObject Libraries)

### What is an Audio Library?

An **Audio Library** is a ScriptableObject that holds a collection of audio files with associated IDs. It allows you to:
- Share audio clips across different scenes
- Organize audio by category (SFX, Ambience, Voice)
- Swap audio without modifying code

**Example**: `Voice_Demo_Iris.asset` contains all 10 of Iris's voice clips (`iris_01` → `iris_10`).

### How It Works
```
INK FILE: #voice:iris_01
    ↓
StoryManager reads tag
    ↓
Raises event on VoiceRequestedChannel("iris_01")
    ↓
VoiceManager receives event
    ↓
VoiceManager searches "iris_01" in Voice_Demo_Iris.asset
    ↓
Plays iris_01.wav
```

### Library Structure

| Library Type | Purpose | Example |
|--------------|---------|---------|
| **SFX Library** | Short sound effects | `SFX_Demo.asset` → phone_ring, glass_break |
| **Ambience Library** | Ambient loops | `Ambience_Demo.asset` → dispatch_night |
| **Voice Library** | Character voices | `Voice_Demo_Iris.asset` → iris_01...iris_10 |

### Adding Audio for a New Story

**STEP 1 — Create Library**:
```
Assets/Audio/Libraries/ → Right Click
→ Create → DEAD AIR → Audio → Audio Clip Library
→ Rename: "Voice_MyStory"
```

**STEP 2 — Populate Library**:
```
Voice_MyStory.asset Inspector:
├─ Library Name: "My Story Voice"
├─ Description: "Character voices for story X"
└─ Clips (Array):
    ├─ [0] id: "character_01", clip: character_01.wav
    ├─ [1] id: "character_02", clip: character_02.wav
    └─ ...
```

**STEP 3 — Assign to Manager**:
```
Scene → VoiceManager Inspector
→ Voice Libraries (Array)
→ Drag "Voice_MyStory.asset"
```

**STEP 4 — Use in Ink**:
```ink
Hello there! # voice:character_01
```

**Zero C# code changes required.**

### Advantages over Inspector Arrays

| Approach | New Story Setup | Cross-Scene Reuse | Maintainability |
|----------|----------------|-------------------|-----------------|
| **Inspector Array** (old) | 15 min (reassign everything) | No (duplication) | Difficult |
| **SO Libraries** (current) | 2 min (drag & drop) | Yes (shared) | Easy |

---

## 4. EVENT CHANNELS (Communication System)

### 4.1 What is an Event Channel?

It is a "communication bridge" between different systems. Instead of having systems talk directly to each other, we use these bridges.

**Advantages**:
- Systems are unaware of each other (you can modify one without breaking the others)
- Each system can be tested in isolation
- No memory leaks
- Easy to debug from the Unity Inspector

### 4.2 How It Works

```
StoryManager reads the tag #sfx:phone_ring
    ↓
StoryManager raises the event on the "SFXRequestedChannel"
    ↓
AudioManager is listening on that channel
    ↓
AudioManager receives "phone_ring" and plays the sound
```

### 4.3 Existing Channels (14 total)

**Dialogue**:
- `DialogueLineChannel` → Text to display
- `SpeakerLineChannel` → Who is speaking + text
- `ChoicesPresentedChannel` → List of available choices

**Audio**:
- `SFXRequestedChannel` → Sound effect to play
- `AmbienceStartChannel` → Ambience to start
- `AmbienceStopChannel` → Stop ambience
- `VoiceRequestedChannel` → Voice to play
- `VoiceStopChannel` → Stop voice

**Player Input**:
- `ContinueRequestedChannel` → Player clicks to continue
- `ChoiceSelectedChannel` → Player selects an option

**Other**:
- `UICommandChannel` → Special UI commands
- `StoryEndChannel` → Story ended
- `VoiceStartedChannel` → Voice started (with duration)
- `VoiceFinishedChannel` → Voice finished

**Location**: `Assets/Scripts/Events/ScriptableObjects/`

---

## 5. HOW TO WRITE A STORY

### 5.1 Complete .ink File Example

```ink
// IRIS CALL - The Bear

-> intro

=== intro ===
# amb:dispatch_night
# sfx:phone_ring

2 AM. Line 3 lights up.

+ [ANSWER]
    -> answer

=== answer ===
# sfx:phone_pickup

911, what's the address of your emergency? # speaker:ward

Hi... I need help with the Bear. # speaker:iris # voice:iris_01

+ [What's your name?]
    -> ask_name
+ [Where are you calling from?]
    -> ask_location

=== ask_name ===
My name is Iris. # speaker:iris # voice:iris_02
-> END

=== ask_location ===
I'm... I'm at home. # speaker:iris # voice:iris_03
-> END
```

### 5.2 Audio File Naming Rules

| Type | Format | Unity Optimization | Example |
|------|--------|--------------------|---------|
| **Voice** | `{speaker}_{number}.wav` | ADPCM, Mono, Optimize SR | `iris_01.wav` |
| **SFX** | `{description}.wav` | ADPCM, Mono, Optimize SR | `phone_ring.wav` |
| **Ambience** | `{location}.ogg` | Vorbis 70%, Streaming, Stereo | `dispatch_night.ogg` |
| **Music** | `{mood}.ogg` | Vorbis 80%, Streaming, Stereo | `tension_loop.ogg` |

**Note**: Unity automatically optimizes audio based on folder location (see **Section 13 - Audio Optimization**).

---

## 6. HOW TO ADD A NEW TAG (eg. Music)

### Example: Adding `#music:tension_loop`

#### STEP 1 — Create the Channel Asset

```
Unity → Project → Assets/Scripts/Events/ScriptableObjects
→ Right Click → Create → DEAD AIR → Events → String Event Channel
→ Rename: "MusicRequestedChannel"
```

#### STEP 2 — Edit DialogueParser.cs

Add at the top (after line 25):
```csharp
private const string TAG_MUSIC = "music:";
```

Add to the `ParsedLine` struct (after line 60):
```csharp
public string Music;
public bool HasMusic;
```

Add to the `ParseTags()` method (after line 100):
```csharp
else if (trimmedTag.StartsWith(TAG_MUSIC))
{
    result.Music = ExtractValue(trimmedTag, TAG_MUSIC);
    result.HasMusic = !string.IsNullOrEmpty(result.Music);
}
```

#### STEP 3 — Edit StoryManager.cs

Add field (after line 50):
```csharp
[SerializeField] private StringEventChannel musicRequestedChannel;
```

Add to the `ProcessLine()` method (after line 180):
```csharp
if (parsed.HasMusic && musicRequestedChannel != null)
{
    musicRequestedChannel.RaiseEvent(parsed.Music);
}
```

#### STEP 4 — Create MusicManager.cs

```csharp
using UnityEngine;
using DeadAir.Events;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private StringEventChannel musicRequestedChannel;
    
    private void OnEnable()
    {
        if (musicRequestedChannel != null)
            musicRequestedChannel.Subscribe(PlayMusic);
    }
    
    private void OnDisable()
    {
        if (musicRequestedChannel != null)
            musicRequestedChannel.Unsubscribe(PlayMusic);
    }
    
    private void PlayMusic(string musicId)
    {
        // Load and play music
        Debug.Log($"Playing music: {musicId}");
    }
}
```

#### STEP 5 — Unity Setup

1. Hierarchy → Create Empty → "MusicManager"
2. Add Component → MusicManager
3. Inspector:
   - Assign AudioSource
   - Drag "MusicRequestedChannel" into the field

4. StoryManager Inspector:
   - Drag "MusicRequestedChannel" into the field

#### STEP 6 — Use in the Ink File

```ink
=== tense_moment ===
# music:tension_loop

Ward feels something is wrong.
```

**Done!** Estimated time: 30 minutes.

---

## 6.5 HOW TO ADD A NEW UI COMMAND (Type-Safe Enums)

### What is a UI Command?

UI commands (`#ui:{command}`) are special instructions in the Ink file that trigger UI behaviors such as:
- Showing special screens (`#ui:dead_air_screen`)
- Returning to the menu (`#ui:return_to_menu`)
- Showing overlays, transitions, or other custom UI effects

**From March 2026**, UI commands use **type-safe enums** instead of fragile strings. This prevents typos and makes the code more maintainable.

---

### System Anatomy

```
INK FILE:  #ui:dead_air_screen
    ↓
DialogueParser.cs: "dead_air_screen" (string) → UICommandType.DeadAirScreen (enum)
    ↓
StoryManager.cs: UICommandType.DeadAirScreen → "dead_air_screen" (string for channel)
    ↓
UICommandChannel: Raises "dead_air_screen"
    ↓
DialogueUI.cs: "dead_air_screen" (string) → UICommandType.DeadAirScreen (enum)
    ↓
DialogueUI.cs: Switch on enum → ShowDeadAirScreen()
```

**Why this flow?**
- Event Channels still use `string` for backward compatibility
- Parser and UI convert to enum for **type safety** and **exhaustiveness check**
- A typo in the Ink file generates a runtime warning (eg. "Unknown UI command: dead_air")

---

### STEP 1 — Add New Value to the Enum

**File**: `Assets/Scripts/Narrative/DialogueParser.cs`

**FIND** the `UICommandType` enum (around line 28):

```csharp
public enum UICommandType
{
    None = 0,           // Default
    DeadAirScreen = 1,  // #ui:dead_air_screen
    ReturnToMenu = 2    // #ui:return_to_menu
}
```

**ADD** the new command (example: pause screen):

```csharp
public enum UICommandType
{
    None = 0,           // Default
    DeadAirScreen = 1,  // #ui:dead_air_screen
    ReturnToMenu = 2,   // #ui:return_to_menu
    PauseScreen = 3     // #ui:pause_screen  ← NEW COMMAND
}
```

**IMPORTANT RULES**:
- `None = 0` must **always** be the first value (safe default)
- Number progressively: 1, 2, 3, 4...
- Add a comment with the corresponding Ink tag

---

### STEP 2 — Add String Parsing

**File**: `Assets/Scripts/Narrative/DialogueParser.cs`

**FIND** the `ParseTags` method (around line 180), UI TAG block:

```csharp
else if (trimmedTag.StartsWith(TAG_UI))
{
    string? uiValue = ExtractValue(trimmedTag, TAG_UI);
    
    result = new ParsedLine
    {
        // ... other fields ...
        UICommand = uiValue?.ToLowerInvariant() switch
        {
            "dead_air_screen" => UICommandType.DeadAirScreen,
            "return_to_menu" => UICommandType.ReturnToMenu,
            _ => UICommandType.None
        }
    };
}
```

**ADD** the case for the new command:

```csharp
UICommand = uiValue?.ToLowerInvariant() switch
{
    "dead_air_screen" => UICommandType.DeadAirScreen,
    "return_to_menu" => UICommandType.ReturnToMenu,
    "pause_screen" => UICommandType.PauseScreen,  // ← ADD THIS
    _ => UICommandType.None
}
```

---

### STEP 3 — Convert Enum → String in StoryManager

**File**: `Assets/Scripts/Narrative/StoryManager.cs`

**FIND** the `ProcessLine` method (around line 233), UI EVENTS block:

```csharp
if (parsed.HasUICommand)
{
    string? commandString = parsed.UICommand switch
    {
        UICommandType.DeadAirScreen => "dead_air_screen",
        UICommandType.ReturnToMenu => "return_to_menu",
        _ => null
    };
    
    if (commandString != null)
        uiCommandChannel.RaiseEvent(commandString);
}
```

**ADD** the case for the new command:

```csharp
string? commandString = parsed.UICommand switch
{
    UICommandType.DeadAirScreen => "dead_air_screen",
    UICommandType.ReturnToMenu => "return_to_menu",
    UICommandType.PauseScreen => "pause_screen",  // ← ADD THIS
    _ => null
};
```

---

### STEP 4 — Implement UI Logic

**File**: `Assets/Scripts/UI/DialogueUI.cs`

**FIND** the `HandleUICommand` method (around line 193):

```csharp
private void HandleUICommand(string command)
{
    UICommandType commandType = command?.ToLowerInvariant() switch
    {
        "dead_air_screen" => UICommandType.DeadAirScreen,
        "return_to_menu" => UICommandType.ReturnToMenu,
        _ => UICommandType.None
    };

    switch (commandType)
    {
        case UICommandType.DeadAirScreen:
            ShowDeadAirScreen();
            break;

        case UICommandType.ReturnToMenu:
            QuitApplication();
            break;

        case UICommandType.None:
            Debug.LogWarning($"[DialogueUI] Unknown UI command: {command}");
            break;
    }
}
```

**ADD** the parsing and the case:

```csharp
// STEP 4.1 — Add parsing
UICommandType commandType = command?.ToLowerInvariant() switch
{
    "dead_air_screen" => UICommandType.DeadAirScreen,
    "return_to_menu" => UICommandType.ReturnToMenu,
    "pause_screen" => UICommandType.PauseScreen,  // ← ADD THIS
    _ => UICommandType.None
};

// STEP 4.2 — Add case
switch (commandType)
{
    case UICommandType.DeadAirScreen:
        ShowDeadAirScreen();
        break;

    case UICommandType.ReturnToMenu:
        QuitApplication();
        break;

    case UICommandType.PauseScreen:  // ← ADD THIS
        ShowPauseScreen();
        break;

    case UICommandType.None:
        Debug.LogWarning($"[DialogueUI] Unknown UI command: {command}");
        break;
}
```

**STEP 4.3 — Create the handler method**:

```csharp
private void ShowPauseScreen()
{
    if (_pauseScreen != null)
    {
        StopTypewriter();
        HideContinueIndicator();
        _pauseScreen.SetActive(true);
        Debug.Log("[DialogueUI] Pause screen active");
    }
}
```

---

### STEP 5 — Use in the Ink File

```ink
=== critical_moment ===
Ward, you need to make a decision. Now.

* [Pause and think]
    # ui:pause_screen
    → END
```

**Done!** The command is now type-safe end-to-end.

---

### Enum System Advantages

| Aspect | Before (Strings) | After (Enums) |
|--------|------------------|---------------|
| **Typo Protection** | `"dead_air_screeen"` = silent fail | Compile error if wrong enum |
| **Refactoring** | Manual Find/Replace | Automatic IDE rename |
| **Exhaustiveness** | Switch can miss cases | Compiler warns on missing case |
| **Autocomplete** | None | IDE suggests enum values |
| **Debugging** | "Unknown command: X" | Precise stacktrace + enum value |

---

### New UI Command Checklist

- [ ] **STEP 1**: Add value to `UICommandType` enum (DialogueParser.cs)
- [ ] **STEP 2**: Add string → enum parsing (DialogueParser.cs, `ParseTags`)
- [ ] **STEP 3**: Add enum → string conversion (StoryManager.cs, `ProcessLine`)
- [ ] **STEP 4**: Add parsing + case (DialogueUI.cs, `HandleUICommand`)
- [ ] **STEP 5**: Implement handler method (DialogueUI.cs, eg. `ShowPauseScreen`)
- [ ] **TEST**: Use `#ui:{command}` in Ink file and verify it works

**Estimated time**: 10 minutes per command.

---

### Technical Notes

**Why 4 modification points?**
- **DialogueParser**: Converts Ink string → enum (single source of truth)
- **StoryManager**: Converts enum → string for Event Channel (legacy compatibility)
- **DialogueUI**: Converts string → enum for type safety + implements logic

**Future**: Migrating Event Channels to use `UICommandType` directly would eliminate STEP 3 and 4.1.

**Similar Pattern**: Use the same strategy for `SpeakerType` enum when adding new characters.

---

## 7. SYSTEM VISUAL SCHEMA

```
PLAYER CLICKS "CONTINUE"
    ↓
DialogueUI raises on ContinueRequestedChannel
    ↓
StoryManager receives event
    ↓
StoryManager advances Ink story
    ↓
StoryManager reads tags (#speaker:iris #voice:iris_01)
    ↓
StoryManager raises on SpeakerLineChannel and VoiceRequestedChannel
    ↓
DialogueUI receives from SpeakerLineChannel → Displays text
VoiceManager receives from VoiceRequestedChannel → Plays audio
```

**No system talks directly to another — everything passes through the Channels**

---

## 8. SYSTEMS AND THEIR ROLES

| System | What It Does | Listens (IN) | Raises (OUT) |
|--------|-------------|--------------|--------------|
| **StoryManager** | Coordinates everything, reads Ink | ContinueRequested, ChoiceSelected | DialogueLine, SpeakerLine, SFX, Ambience, Voice, UI, StoryEnd |
| **DialogueUI** | Displays text and choices | DialogueLine, SpeakerLine, ChoicesPresented, UI, StoryEnd | ContinueRequested, ChoiceSelected, VoiceStop |
| **AudioManager** | SFX and Ambience (via Libraries) | SFXRequested, AmbienceStart, AmbienceStop | None |
| **VoiceManager** | Character voices (via Libraries) | VoiceRequested, VoiceStop | VoiceStarted, VoiceFinished |

---

## 9. TROUBLESHOOTING

### Problem: Tag not working

**Checklist**:
1. Tag written correctly in the .ink file? (`#voice:iris_01` NOT `# voice: iris_01`)
2. Channel asset created?
3. Channel assigned in StoryManager?
4. Channel assigned in the listening Manager?
5. Audio file present in the Media folder?

### Problem: No audio playing

**Checklist**:
1. AudioManager has an AudioSource assigned?
2. AudioManager has the correct Library assigned? (Inspector → SFX Libraries / Ambience Libraries)
3. Does the Library contain the clip with the correct ID? (Open Library asset → verify ID)
4. Does the file name match the ID in the Library? (`#sfx:phone_ring` → ID "phone_ring" in Library)
5. AudioSource Volume > 0?
6. Console shows `[AudioClipLibrary] X → N clips loaded`?

**If Console shows `[AudioManager] Total loaded: 0 SFX`**:
- Verify the Library is assigned in AudioManager's Inspector
- Verify the Library contains clips (is not empty)

### Problem: Text not appearing

**Checklist**:
1. DialogueUI has TextMeshPro assigned in the `_dialogueText` field?
2. DialogueUI has the `dialogueLineChannel` assigned?
3. Canvas is active in the scene?

---

## 10. QUICK REFERENCE

### Key Files to Know

| File | What It Contains |
|------|-----------------|
| `DialogueParser.cs` | Parsing of all tags |
| `StoryManager.cs` | General coordination, story advancement |
| `DialogueUI.cs` | Text and choice display |
| `AudioManager.cs` | SFX and Ambience |
| `VoiceManager.cs` | Character voices |

### Code Conventions

- **Serialized fields**: `[SerializeField] private TypeName _fieldName;`
- **Public methods**: `PascalCase` (eg. `PlayMusic`)
- **Private methods**: `PascalCase` (eg. `HandleMusicRequested`)
- **Event handlers**: `Handle` prefix (eg. `HandleDialogueLine`)
- **Constants**: `ALL_CAPS` (eg. `TAG_MUSIC`)

### Lifecycle Pattern

```csharp
private void OnEnable()
{
    // Subscribe to channels here
    channel.Subscribe(Handler);
}

private void OnDisable()
{
    // ALWAYS Unsubscribe to avoid memory leaks
    channel.Unsubscribe(Handler);
}
```

---

## 11. UPCOMING IMPROVEMENTS

**Completed**:
- [x] Audio Libraries system (ScriptableObject-based)
- [x] Singleton AudioManager with hot-reload across scenes
- [x] Audio format optimization (ADPCM, Vorbis, Streaming)
- [x] Type-Safe Enums for Speaker and UI Commands (April 2026)
- [x] DialogueParser refactoring: readonly struct, nullable strings, derived properties

**To Do**:
- [ ] Typed Event Channels (native SpeakerType, UICommandType)
- [ ] Auto-populate Libraries from folders (Editor script)
- [ ] Save progress system
- [ ] Full main menu
- [ ] Multiple stories system (selection menu)
- [ ] Library Validation Tool (duplicate ID check)

**Possible New Tags**:
- `#music:{id}` → Background music
- `#camera_shake:{intensity}` → Camera shake
- `#fade:{type}` → Screen transitions

---

## 12. CONTACT

**Developer**: Michele Grimaldi  
**Studio**: E-C-H-O SYSTEMS  
**Project**: DEAD AIR

---

## 13. AUDIO OPTIMIZATION

### Unity Import Settings by Type

DEAD AIR uses type-specific optimizations following Unity best practices:

| Type | Load Type | Compression | Sample Rate | Mono/Stereo | Memory |
|------|-----------|-------------|-------------|-------------|--------|
| **Voice** | Decompress On Load | ADPCM | Optimize (~22 kHz) | Mono | ~120 KB per 5s |
| **SFX** | Decompress On Load | ADPCM | Optimize (~22 kHz) | Mono | ~50 KB per 2s |
| **Ambience** | Streaming | Vorbis 70% | 44.1 kHz | Stereo | ~200 KB buffer |
| **Music** | Streaming | Vorbis 80% | 44.1 kHz | Stereo | ~200 KB buffer |

### Why These Choices?

**ADPCM for Voice/SFX**:
- 3.5x compression vs PCM
- Minimal CPU overhead (+5% vs PCM)
- 95% quality (dialogue tolerates artifacts)
- Zero latency (decompressed in RAM)

**Vorbis Streaming for Ambience/Music**:
- ~10x compression vs PCM
- Fixed memory (~200 KB buffer, independent of clip duration)
- Streaming from disk (no memory spikes)
- 90-95% quality (acceptable for ambient loops)

**Optimize Sample Rate**:
- Unity analyzes audio frequencies
- Automatically reduces sample rate when possible (eg. 44.1 kHz → 22 kHz)
- 50% memory saving with no perceptible quality loss

### Performance Targets Achieved
```
Memory (Idle): ~2 MB   (target: <5 MB)  OK
Memory (Playing): ~5 MB  (target: <10 MB) OK
CPU Audio: <1 ms/frame  (target: <2 ms)  OK
Disk Size: ~15 MB       (target: <50 MB) OK
Load Time: <20 ms       (target: <50 ms) OK
```

### How Optimizations Are Applied

Unity automatically applies **import settings** based on the file's folder:
- Files in `Audio/Voice/` → ADPCM, Mono, Optimize
- Files in `Audio/SFX/` → ADPCM, Mono, Optimize
- Files in `Audio/Ambience/` → Vorbis 70%, Streaming, Stereo

**No manual setup required** (managed by the AudioImportProcessor script).

---

**Document Version**: 2.2 (April 2026)  
**Architecture**: Event Channels + Audio Libraries (ScriptableObject) + Type-Safe Enums  
**Last Modified**: April 6, 2026  
**Build Version**: 0.8 (C# Types Refactoring)
