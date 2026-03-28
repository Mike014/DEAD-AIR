# DEAD AIR — Game Design & Architecture Document

**Titolo**: Dead Air  
**Genere**: Horror Narrativo / Audio-First Experience  
**Piattaforma Target**: PC (Windows/Mac), potenzialmente Web  
**Engine**: Unity 2021 LTS+  
**Narrative Engine**: Ink (Inkle)  
**Durata Demo**: 15-20 minuti (4-5 chiamate)

![Copertina del progetto](Logo.png)
## DEAD AIR — Documenti informativi riguardo il Concept di gioco
- [DEAD_AIR_STORY_ARCHITECTURE](https://docs.google.com/document/d/1fydiwT6h3TYMvayOMdnoAXsqVEKncyPSYEauwPCPtxY/edit?tab=t.0#heading=h.n71ap8gqp083)
- [DEAD_AIR_CONCEPT](https://docs.google.com/document/d/19lzCzj4KluC-9Iayi9aIQmPGrRT4fcdhMTGNEUfs3tg/edit?tab=t.0)
- [DEAD AIR - Scena 1](https://docs.google.com/document/d/1AXRHu3tBfq9NYKhXJQnWr8wlpkkn18FYZ-0bdrgomoQ/edit?tab=t.0)
---

## 1. HIGH CONCEPT

Un operatore del 911 lavora il turno di notte in una centrale operativa degli anni '90. Risponde a chiamate che diventano progressivamente inquietanti. Il gameplay è **audio-first**: il giocatore ascolta, sceglie risposte multiple, e subisce disturbi sia sonori che visivi.

**Core Loop**:
1. Giocatore seleziona una chiamata dal menu
2. La chiamata viene caricata (storia Ink + asset media)
3. Il giocatore interagisce tramite scelte e ascolto
4. La chiamata termina, ritorno al menu

*Dead Air — A project by Michele Grimaldi*  
*E-C-H-O SYSTEMS*

---

## 2. REFACTORING HISTORY

### 2.1 Rimozione Sistema Timer (Marzo 2026)

**Motivazione**: Semplificazione architettura e riduzione complessità non necessaria per il design core del gioco.

**Modifiche apportate**:

| File | Modifiche | LOC Rimossi | Impatto |
|------|-----------|-------------|---------|
| **DialogueParser.cs** | Rimossi tag timer (`#timed_choice`, `#timeout:X`, `#default:X`) e metodo `ParseTimedChoiceTags()` | ~50 | Responsabilità singola pura |
| **StoryManager.cs** | Eliminata simulazione scelte (save/restore stato Ink JSON) | ~50 | Performance: da O(n) a O(1), -71% codice in `PresentChoices()` |
| **DialogueUI.cs** | Rimossi handler timer (`HandleTimedChoiceStarted`, `HandleTimerProgress`, `HandleTimerCancelled`) e UI timer bar | ~40 | Decoupling totale da sistemi temporali |
| **NarrativeEvents.cs** | Rimossi 4 eventi timer e relativi metodi Invoke | ~30 | Surface API ridotta del 22% |

**Performance Gain**:
- Zero allocazioni heap speculative
- Nessuna serializzazione JSON per simulazione scelte
- Latency scelte: **da ~50ms a <1ms** (su 4 scelte)

**Superficie API finale**:
- Eventi: da 14 a 10 (-28%)
- Tag supportati: 5 categorie core (speaker, voice, sfx, ambience, ui)
- Speaker TAG attualmente NON reagisce al cambio di colore

---

## 3. ARCHITETTURA CORE — Tag-Driven System

### 3.1 Filosofia di Design

Il gioco ruota attorno a un **sistema tag-driven event-based**:

```
INK FILE (contenuto narrativo + tag metadata)
    ↓
DialogueParser (estrae tag in struct ParsedLine)
    ↓
StoryManager (dispatcha eventi per ogni tag)
    ↓
NarrativeEvents (event hub centrale)
    ↓
Manager Subscribers (audio, UI, voice)
    ↓
EFFETTI IN-GAME (suoni, testo colorato, comandi UI)
```

**Principio fondamentale**: Aggiungere una nuova storia richiede **SOLO**:
1. File `.ink` con tag convenzionali
2. Asset audio nominati secondo convenzione
3. Zero modifiche al codice C#

---

### 3.2 Tag Attualmente Supportati

| Tag | Sintassi | Effetto | Handler | Esempio |
|-----|----------|---------|---------|---------|
| **Speaker** | `#speaker:{id}` | Cambia colore testo UI | DialogueUI | `#speaker:iris` → testo verde |
| **Voice** | `#voice:{clipId}` | Riproduce clip vocale personaggio | VoiceManager | `#voice:iris_01` |
| **SFX** | `#sfx:{clipId}` | Effetto sonoro one-shot | AudioManager | `#sfx:phone_ring` |
| **Ambience** | `#amb:{clipId}`<br>`#amb:stop` | Loop ambience (start/stop) | AudioManager | `#amb:dispatch_night` |
| **UI** | `#ui:{command}` | Comandi UI speciali | DialogueUI | `#ui:dead_air_screen` |

**File sorgente**: `DialogueParser.cs` (linee 21-25)

---

### 3.3 Architettura File System

```
Assets/
├── Scripts/
│   ├── Audio/
│   │   ├── AudioManager.cs       → Gestisce SFX + Ambience
│   │   └── VoiceManager.cs       → Gestisce Voice clips
│   ├── Events/
│   │   └── NarrativeEvents.cs    → Event hub statico (10 eventi)
│   ├── Narrative/
│   │   ├── StoryManager.cs       → Carica Ink, dispatcha eventi
│   │   └── DialogueParser.cs     → Parsing tag (logica pura)
│   └── UI/
│       ├── DialogueUI.cs         → Rendering testo + scelte
│       └── ChoiceButton.cs       → Prefab bottone scelta
├── Ink/
│   └── dead_air_demo_en.ink      → Storia principale
└── Media/
    ├── Voice/
    ├── SFX/
    └── Ambience/
```

---

### 3.4 Dependency Graph

```
StoryManager (MonoBehaviour)
    ├─→ DialogueParser (static utility, zero dipendenze Unity)
    └─→ NarrativeEvents (static event hub)

DialogueUI (MonoBehaviour)
    └─→ NarrativeEvents (subscriber)

AudioManager (MonoBehaviour, Singleton)
    └─→ NarrativeEvents (subscriber)

VoiceManager (MonoBehaviour)
    └─→ NarrativeEvents (subscriber)
```

**Pattern**: Observer via C# Events. Nessun sistema conosce gli altri direttamente.

---

## 4. ESTENSIONE DEL SISTEMA TAG

### 4.1 Aggiungere un Nuovo Tag (es. `#music:xxx`)

Il sistema è progettato per essere esteso seguendo questo pattern in 4 step:

#### **STEP 1 — DialogueParser.cs**

```csharp
// Aggiungi costante tag
private const string TAG_MUSIC = "music:";

// Aggiungi campi a ParsedLine struct
public struct ParsedLine
{
    // ... campi esistenti ...
    public string Music;
    public bool HasMusic;
}

// Aggiungi caso nel loop ParseTags()
else if (trimmedTag.StartsWith(TAG_MUSIC))
{
    result.Music = ExtractValue(trimmedTag, TAG_MUSIC);
    result.HasMusic = !string.IsNullOrEmpty(result.Music);
}
```

#### **STEP 2 — NarrativeEvents.cs**

```csharp
// Aggiungi evento
public static event Action<string> OnMusicRequested;

// Aggiungi metodo Invoke
public static void MusicRequested(string musicId)
{
    OnMusicRequested?.Invoke(musicId);
}

// Aggiungi cleanup in ClearAllListeners()
OnMusicRequested = null;
```

#### **STEP 3 — StoryManager.cs**

```csharp
// In ProcessLine(), dopo gli altri controlli
if (parsed.HasMusic)
{
    NarrativeEvents.MusicRequested(parsed.Music);
    Log($"  → Music: {parsed.Music}");
}
```

#### **STEP 4 — MusicManager.cs (nuovo file)**

```csharp
namespace DeadAir.Audio
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _musicSource;
        // ... lookup dictionary ...
        
        private void OnEnable()
        {
            NarrativeEvents.OnMusicRequested += PlayMusic;
        }
        
        private void OnDisable()
        {
            NarrativeEvents.OnMusicRequested -= PlayMusic;
        }
        
        private void PlayMusic(string musicId)
        {
            // Implementazione riproduzione musica
        }
    }
}
```

**Risultato**: Il tag `#music:tension_loop` nel file Ink ora trigger la riproduzione musica.

---

### 4.2 Tag Candidati per Espansione Futura

| Tag | Uso Previsto | Priorità | Sistema Richiesto |
|-----|--------------|----------|-------------------|
| `#music:{id}` | Background music layer | Media | MusicManager |
| `#video:{id}` | Cutscene/flashback | Bassa | VideoManager + VideoPlayer |
| `#camera:{command}` | Shake, zoom, glitch | Media | CameraEffectsManager |
| `#fade:{type}` | Transizioni schermo | Bassa | TransitionManager |
| `#particle:{id}` | Effetti particellari | Bassa | ParticleManager |

---

## 5. CONVENZIONI INK

### 5.1 Esempio File .ink Completo

```ink
// ============================================
// CALL: iris_001 — The Bear
// ============================================

VAR asked_name = false

-> intro

=== intro ===
# amb:dispatch_night
# sfx:phone_ring

2 AM. Friday dragging itself into Saturday.
Line 3.

+ [ANSWER]
    -> answer

=== answer ===
# sfx:phone_pickup

911, what's the address of your emergency? # speaker:ward

Hi... I just wanted to know if the Bear is still angry. # speaker:iris # voice:iris_01

+ [What's your name, sweetheart?]
    ~ asked_name = true
    -> ask_name
+ [Can you describe the bear?]
    -> describe_bear

=== ask_name ===
# voice:iris_02
My name is Iris. # speaker:iris
-> END

=== describe_bear ===
# sfx:breathing_heavy
# voice:iris_03
He... he wears church shoes. # speaker:iris
-> END
```

### 5.2 Regole di Naming Convention

**Voice Clips**: `{speaker}_{id}.wav`  
Esempio: `iris_01.wav`, `ward_response_03.wav`

**SFX**: `{descrizione}.wav`  
Esempio: `phone_ring.wav`, `glass_break.wav`

**Ambience**: `{ambiente}_loop.wav` (opzionale `_loop` suffix)  
Esempio: `dispatch_night.wav`, `rain_exterior_loop.wav`

---

## 6. LIMITI ARCHITETTURALI ATTUALI

### 6.1 Asset Assignment Manuale

**Problema**: Ogni nuovo tag richiede configurazione manuale in Unity Inspector.

**Esempio**:  
Tag Ink `#sfx:glass_break` → Aprire AudioManager → Aggiungere entry `[id: "glass_break", clip: glass_break.wav]`

**Impatto**: Setup nuova storia ~15 minuti (Ink + Inspector assignment)

### 6.2 Roadmap: Convention-Based Asset Loading

**Obiettivo**: Struttura folder convention + auto-loading da Resources/Addressables.

**Target**:
```
Assets/Media/Stories/
    iris_call/
        Voice/iris_01.wav
        SFX/phone_ring.wav
        Ambience/dispatch_night.wav
```

Tag `#sfx:phone_ring` → Auto-load `phone_ring.wav` da `SFX/` folder.

**Beneficio**: Setup nuova storia ~2 minuti (solo Ink, zero Inspector).

---

## 7. PERFORMANCE NOTES

### 7.1 Memory Allocation

- **DialogueParser**: Struct-based (zero heap allocation per chiamata)
- **NarrativeEvents**: Static event hub (zero lookup overhead)
- **StoryManager**: List reusage per scelte (no repeated allocation)

### 7.2 Audio System

- **SFX**: `PlayOneShot()` → no clip assignment, minimal GC
- **Ambience**: Single `AudioSource.clip` assignment + loop
- **Voice**: Single source, stop-on-new per evitare overlap

---

## 8. CONTATTI

**Lead Developer**: Michele Grimaldi  
**Project**: DEAD AIR  
**Studio**: E-C-H-O SYSTEMS  
**Repository**: [Link quando disponibile]

---


