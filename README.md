# DEAD AIR — Game Design & Architecture Document

**Titolo**: Dead Air  
**Genere**: Horror Narrativo / Audio-First Experience  
**Piattaforma Target**: PC (Windows/Mac), potenzialmente Web  
**Engine**: Unity 2021 LTS+  
**Narrative Engine**: Ink (Inkle)  
**Durata Demo**: 15-20 minuti (4-5 chiamate)

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

## 2. ARCHITETTURA ATTUALE (Legacy)

```
Assets/
├── Ink/
│   ├── dead_air_demo_en.ink        ← file monolitico
│   └── IT_version/dead_air_demo.ink
├── Script/
│   ├── Audio/
│   │   ├── AudioManager.cs          ← clip hardcodate in Inspector
│   │   └── VoiceManager.cs          ← clip hardcodate in Inspector
│   ├── Events/NarrativeEvents.cs    ← hub eventi statico
│   ├── Narrative/
│   │   ├── StoryManager.cs          ← carica UN TextAsset fisso
│   │   ├── DialogueParser.cs        ← parser tag (stateless, OK)
│   │   └── TimeChoiceHandler.cs
│   └── UI/
│       ├── DialogueUI.cs
│       └── ChoiceButton.cs
```

### Limiti Identificati

| Problema | Impatto | Priorità |
|----------|---------|----------|
| File `.ink` monolitico | Difficile da gestire, merge conflicts | Alta |
| Audio hardcodato in Inspector | Ogni nuova chiamata = configurazione manuale | Alta |
| `StoryManager` con `TextAsset` fisso | Non supporta chiamate multiple | Alta |
| Eventi statici globali | Non scala per handler specifici per chiamata | Media |
| Nessun concetto di "chiamata" come unità | Manca astrazione fondamentale | Alta |

---

## 3. ARCHITETTURA TARGET (Data-Driven)

### 3.1 Obiettivo

> **Aggiungere una nuova chiamata richiede soltanto:**
> 1. Scrivere un file `.ink` seguendo le convenzioni
> 2. Creare asset audio nella cartella corretta (naming convention)
> 3. Creare un `EmergencyCallSO` nell'Inspector
> 4. Aggiungere l'asset al menu
>
> **Zero modifiche al codice C#.**

### 3.2 Struttura Cartelle Target

```
Assets/
├── Data/
│   ├── Calls/                          ← ScriptableObject per chiamata
│   │   ├── iris_001.asset
│   │   ├── highway_002.asset
│   │   └── ...
│   ├── Events/                         ← Event Channels (SO)
│   │   ├── AudioEventChannel.asset
│   │   ├── DialogueEventChannel.asset
│   │   ├── UIEventChannel.asset
│   │   └── StoryFlowEventChannel.asset
│   └── Config/
│       └── GameConfig.asset            ← settings globali
│
├── Media/
│   └── Calls/
│       └── {callId}/                   ← una cartella per chiamata
│           ├── Voice/
│           │   ├── iris_01.wav
│           │   └── iris_02.wav
│           ├── SFX/
│           │   ├── phone_ring.wav
│           │   └── glass_break.wav
│           ├── Ambience/
│           │   └── dispatch_night.wav
│           ├── Music/
│           │   └── tension_loop.wav
│           └── Video/
│               └── flashback_01.mp4
│
├── Ink/
│   └── Calls/
│       ├── iris_001.ink                ← un file per chiamata
│       ├── highway_002.ink
│       └── ...
│
├── Scripts/
│   ├── Core/
│   │   ├── CallLoader.cs               ← carica EmergencyCallSO
│   │   ├── TagHandlerRegistry.cs       ← registry handler attivi
│   │   └── MediaResolver.cs            ← risolve path Addressables
│   ├── Data/
│   │   ├── EmergencyCallSO.cs          ← definizione chiamata
│   │   └── EventChannels/
│   │       ├── AudioEventChannel.cs
│   │       ├── DialogueEventChannel.cs
│   │       ├── UIEventChannel.cs
│   │       └── StoryFlowEventChannel.cs
│   ├── Handlers/
│   │   ├── ITagHandler.cs              ← interfaccia comune
│   │   ├── VoiceHandler.cs
│   │   ├── SFXHandler.cs
│   │   ├── AmbienceHandler.cs
│   │   ├── MusicHandler.cs
│   │   └── VideoHandler.cs
│   ├── Narrative/
│   │   ├── StoryManager.cs             ← refactored
│   │   ├── DialogueParser.cs           ← invariato
│   │   └── TimedChoiceHandler.cs
│   ├── UI/
│   │   ├── DialogueUI.cs
│   │   ├── ChoiceButton.cs
│   │   └── CallMenuUI.cs               ← nuovo: menu selezione
│   └── Editor/
│       └── CallValidatorTool.cs        ← validazione pre-build
│
└── AddressableAssetsData/
    └── ... (configurazione Addressables)
```

---

## 4. COMPONENTI CHIAVE

### 4.1 EmergencyCallSO

L'unità fondamentale del sistema. Rappresenta tutto ciò che serve per eseguire una chiamata.

```csharp
[CreateAssetMenu(fileName = "NewCall", menuName = "DEAD AIR/Emergency Call")]
public class EmergencyCallSO : ScriptableObject
{
    [Header("Identificazione")]
    public string callId;               // "iris_001" — usato per path media
    public string displayName;          // "The Bear" — mostrato nel menu
    
    [Header("Narrative")]
    public TextAsset inkFile;           // file .ink compilato (.json)
    
    [Header("Required Handlers")]
    public string[] requiredTags;       // ["voice", "sfx", "amb", "video"]
    
    [Header("Metadata")]
    public string description;          // breve sinossi per menu
    public Sprite thumbnail;            // immagine per menu
    public float estimatedDuration;     // minuti (per UI)
    public int difficulty;              // 1-5 (opzionale)
}
```

**Workflow**:
1. Il narrative designer scrive `iris_001.ink`
2. Compila in `.json` tramite Inky o build pipeline
3. Crea `iris_001.asset` nell'Inspector
4. Imposta `callId = "iris_001"`, trascina il `.json`, dichiara `requiredTags`

### 4.2 Event Channels (ScriptableObject)

Sostituiscono `NarrativeEvents` statico. Ogni canale è un asset condiviso tra chi emette e chi ascolta.

**Canali previsti**:

| Canale | Eventi | Emittente | Listener |
|--------|--------|-----------|----------|
| `AudioEventChannel` | SFXRequested, VoiceRequested, AmbienceStart, AmbienceStop, MusicStart, MusicStop | StoryManager | SFXHandler, VoiceHandler, AmbienceHandler, MusicHandler |
| `DialogueEventChannel` | DialogueLine, SpeakerLine, ChoicesPresented | StoryManager | DialogueUI |
| `UIEventChannel` | UICommand, TimerProgress, TimerCancelled | StoryManager, TimedChoiceHandler | DialogueUI |
| `StoryFlowEventChannel` | ContinueRequested, ChoiceSelected, StoryEnd | DialogueUI, TimedChoiceHandler | StoryManager |

**Vantaggi**:
- Testabilità: puoi creare mock channels per unit test
- Disaccoppiamento: nessun riferimento statico
- Ispezionabilità: vedi le connessioni nell'Inspector
- Nessun memory leak: SO non vengono distrutti tra scene

### 4.3 TagHandlerRegistry

Registry centrale che mappa tag name → handler attivo.

```csharp
public class TagHandlerRegistry : MonoBehaviour
{
    private Dictionary<string, ITagHandler> _handlers;
    
    public void Register(string tagName, ITagHandler handler);
    public void Unregister(string tagName);
    public ITagHandler GetHandler(string tagName);
    public bool HasHandler(string tagName);
    public void ActivateHandlersForCall(EmergencyCallSO call);
}
```

**Flusso**:
1. Scena contiene `VoiceHandler`, `SFXHandler`, etc. (disabilitati di default)
2. Ogni handler chiama `Registry.Register("voice", this)` in `Awake()`
3. `CallLoader` carica `EmergencyCallSO`
4. `CallLoader` chiama `Registry.ActivateHandlersForCall(call)`
5. Il registry abilita solo gli handler in `call.requiredTags`

### 4.4 MediaResolver

Risolve i path Addressables seguendo la convenzione.

```csharp
public class MediaResolver
{
    // Pattern: "Media/Calls/{callId}/{mediaType}/{clipId}"
    
    public AsyncOperationHandle<AudioClip> LoadVoice(string callId, string clipId);
    public AsyncOperationHandle<AudioClip> LoadSFX(string callId, string clipId);
    public AsyncOperationHandle<AudioClip> LoadAmbience(string callId, string clipId);
    public AsyncOperationHandle<AudioClip> LoadMusic(string callId, string clipId);
    public AsyncOperationHandle<VideoClip> LoadVideo(string callId, string clipId);
    
    public void ReleaseAll();  // chiamato a fine chiamata
}
```

**Addressables Setup**:
- Ogni cartella `Media/Calls/{callId}/` è un **Addressable Group**
- Indirizzo: `Calls/{callId}/Voice/{clipId}` (senza estensione)
- Il gruppo viene caricato on-demand quando la chiamata inizia
- Viene rilasciato quando la chiamata termina

### 4.5 CallLoader

Orchestratore che carica una chiamata e prepara il sistema.

```csharp
public class CallLoader : MonoBehaviour
{
    [SerializeField] private TagHandlerRegistry _registry;
    [SerializeField] private StoryManager _storyManager;
    [SerializeField] private MediaResolver _mediaResolver;
    
    public async UniTask LoadCall(EmergencyCallSO call)
    {
        // 1. Attiva handler richiesti
        _registry.ActivateHandlersForCall(call);
        
        // 2. Imposta callId sul MediaResolver
        _mediaResolver.SetCurrentCall(call.callId);
        
        // 3. Pre-carica asset Addressables (opzionale)
        await _mediaResolver.PreloadCallAssets(call.callId);
        
        // 4. Inizializza StoryManager con il file Ink
        _storyManager.Initialize(call.inkFile);
        
        // 5. Avvia la storia
        _storyManager.StartStory();
    }
    
    public void UnloadCall()
    {
        _storyManager.Stop();
        _mediaResolver.ReleaseAll();
        _registry.DeactivateAll();
    }
}
```

---

## 5. CONVENZIONI INK

### 5.1 Tag Supportati

| Tag | Formato | Esempio | Handler |
|-----|---------|---------|---------|
| Speaker | `#speaker:{id}` | `#speaker:iris` | DialogueUI |
| Voice | `#voice:{clipId}` | `#voice:iris_01` | VoiceHandler |
| SFX | `#sfx:{clipId}` | `#sfx:phone_ring` | SFXHandler |
| Ambience Start | `#amb:{clipId}` | `#amb:dispatch_night` | AmbienceHandler |
| Ambience Stop | `#amb:stop` | `#amb:stop` | AmbienceHandler |
| Music Start | `#music:{clipId}` | `#music:tension_loop` | MusicHandler |
| Music Stop | `#music:stop` | `#music:stop` | MusicHandler |
| Video | `#video:{clipId}` | `#video:flashback_01` | VideoHandler |
| UI Command | `#ui:{command}` | `#ui:dead_air_screen` | DialogueUI |
| Timed Choice | `#timed_choice` | `#timed_choice` | TimedChoiceHandler |
| Timeout | `#timeout:{seconds}` | `#timeout:4` | TimedChoiceHandler |
| Default Choice | `#default:{index}` | `#default:1` | TimedChoiceHandler |

### 5.2 Esempio File .ink

```ink
// ============================================
// CALL: iris_001 — The Bear
// ============================================

VAR asked_name = false

-> intro

=== intro ===
# amb:dispatch_night

2 AM. Friday dragging itself into Saturday.

# sfx:phone_ring

Line 3.

+ [ANSWER]
    -> answer

=== answer ===
# sfx:phone_pickup

911, what's the address of your emergency? # speaker:ward

Hi... I just wanted to know if the Bear is still angry. # speaker:iris # voice:iris_01

-> END
```

### 5.3 Regole per Narrative Designer

1. **Un file `.ink` per chiamata** — mai mescolare storie
2. **`callId` deve corrispondere** — nome file, cartella media, asset SO
3. **Tag audio = file esistente** — `#voice:iris_01` → `Voice/iris_01.wav`
4. **Speaker dichiarati** — ogni linea parlata ha `#speaker:`
5. **Variabili locali** — nessuna variabile globale cross-chiamata

---

## 6. VALIDAZIONE EDITOR

### 6.1 CallValidatorTool

Tool editor che analizza un `EmergencyCallSO` e verifica:

1. **Parsing Ink**: Estrae tutti i tag dal file `.ink`
2. **Verifica Media**: Per ogni `#voice:X`, `#sfx:X`, etc. verifica che il file esista nel path Addressables
3. **Report**: Genera lista di errori/warning

```csharp
// Menu: DEAD AIR > Validate Call
// Menu: DEAD AIR > Validate All Calls

public class CallValidatorTool : EditorWindow
{
    public ValidationReport ValidateCall(EmergencyCallSO call);
    public ValidationReport ValidateAllCalls();
}

public class ValidationReport
{
    public List<string> Errors;      // bloccanti
    public List<string> Warnings;    // non bloccanti
    public bool IsValid => Errors.Count == 0;
}
```

### 6.2 Errori Rilevati

| Tipo | Esempio | Severità |
|------|---------|----------|
| File mancante | `#voice:iris_99` ma `iris_99.wav` non esiste | Error |
| Handler non dichiarato | `#video:X` ma `"video"` non in `requiredTags` | Warning |
| Tag malformato | `#voice:` (valore vuoto) | Error |
| callId mismatch | `callId = "iris"` ma cartella è `iris_001/` | Error |

### 6.3 Workflow CI/CD

```bash
# Pre-build validation (Unity CLI)
Unity -batchmode -executeMethod CallValidatorTool.ValidateAllCallsCLI -quit

# Exit code 0 = OK, 1 = errori trovati
```

---

## 7. FLUSSO RUNTIME

```
┌─────────────────────────────────────────────────────────────────┐
│                         MAIN MENU                                │
│                                                                  │
│   [Call 1: The Bear]  [Call 2: Highway]  [Call 3: ???]          │
└──────────────────────────────┬──────────────────────────────────┘
                               │ Player clicks "The Bear"
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                        CALL LOADER                               │
│                                                                  │
│  1. Load EmergencyCallSO (iris_001.asset)                       │
│  2. Activate required handlers (voice, sfx, amb)                │
│  3. Set MediaResolver.currentCallId = "iris_001"                │
│  4. Initialize StoryManager with ink file                       │
│  5. Start story                                                 │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                       STORY RUNTIME                              │
│                                                                  │
│  StoryManager.Continue()                                        │
│       │                                                         │
│       ├─► Parse tags (DialogueParser)                           │
│       │                                                         │
│       ├─► #voice:iris_01                                        │
│       │       └─► AudioEventChannel.RaiseVoiceRequested()       │
│       │               └─► VoiceHandler.OnVoiceRequested()       │
│       │                       └─► MediaResolver.LoadVoice()     │
│       │                               └─► Play audio            │
│       │                                                         │
│       ├─► #speaker:iris + text                                  │
│       │       └─► DialogueEventChannel.RaiseSpeakerLine()       │
│       │               └─► DialogueUI.ShowText()                 │
│       │                                                         │
│       └─► Choices available                                     │
│               └─► DialogueEventChannel.RaiseChoicesPresented()  │
│                       └─► DialogueUI.ShowChoices()              │
└──────────────────────────────┬──────────────────────────────────┘
                               │ Story reaches -> END
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                       CALL UNLOAD                                │
│                                                                  │
│  1. StoryFlowEventChannel.RaiseStoryEnd()                       │
│  2. MediaResolver.ReleaseAll()                                  │
│  3. TagHandlerRegistry.DeactivateAll()                          │
│  4. Return to Main Menu                                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 8. MIGRAZIONE (Step-by-Step)

### Fase 1: Fondamenta (Settimana 1)
- [ ] Creare `EmergencyCallSO.cs`
- [ ] Creare struttura cartelle `Media/Calls/iris_001/`
- [ ] Spostare asset audio esistenti nella nuova struttura
- [ ] Configurare Addressables base

### Fase 2: Event Channels (Settimana 2)
- [ ] Creare classi Event Channel (Audio, Dialogue, UI, StoryFlow)
- [ ] Creare asset `.asset` per ogni canale
- [ ] Refactor `StoryManager` per usare channels invece di `NarrativeEvents`
- [ ] Refactor `DialogueUI` per sottoscriversi ai channels

### Fase 3: Handler System (Settimana 3)
- [ ] Creare interfaccia `ITagHandler`
- [ ] Creare `TagHandlerRegistry`
- [ ] Refactor `VoiceManager` → `VoiceHandler` (implementa `ITagHandler`)
- [ ] Refactor `AudioManager` → `SFXHandler` + `AmbienceHandler` + `MusicHandler`
- [ ] Creare `VideoHandler` (nuovo)

### Fase 4: Media Resolution (Settimana 4)
- [ ] Creare `MediaResolver`
- [ ] Configurare Addressables groups per chiamata
- [ ] Integrare `MediaResolver` negli handler
- [ ] Rimuovere array Inspector da handler

### Fase 5: Call Loading (Settimana 5)
- [ ] Creare `CallLoader`
- [ ] Creare `CallMenuUI`
- [ ] Creare primo `EmergencyCallSO` (iris_001)
- [ ] Test end-to-end

### Fase 6: Tooling (Settimana 6)
- [ ] Creare `CallValidatorTool`
- [ ] Integrare in CI/CD
- [ ] Documentazione per team

---

## 9. DECISIONI ARCHITETTURALI

| Decisione | Scelta | Rationale |
|-----------|--------|-----------|
| Struttura chiamate | Episodiche, isolate | Semplicità, nessuna persistenza cross-call |
| Selezione chiamata | Menu (giocatore sceglie) | Libertà di esplorazione |
| Asset audio | Convenzione rigida per path | Zero configurazione Inspector, meno errori |
| Scope asset | Tutto locale per chiamata | Isolamento totale, nessuna dipendenza |
| Sistema caricamento | Addressables | Scalabilità memoria per 50+ chiamate |
| Event dispatch | ScriptableObject Channels | Disaccoppiamento, testabilità, ispezionabilità |
| Raggruppamento eventi | Per dominio (4 canali) | Bilanciamento granularità/gestibilità |
| Handler discovery | Registry esplicito | Controllo preciso, debugging facilitato |
| Handler activation | Dichiarati in EmergencyCallSO | Solo handler necessari attivi |
| Validazione | Editor-time (tool) | Runtime fiducioso, errori catturati prima |
| Livello validazione | Parsing Ink + verifica file | Coverage completo senza overhead |

---

## 10. APPENDICE: Tag Handler Interface

```csharp
public interface ITagHandler
{
    /// <summary>
    /// Nome del tag gestito (es. "voice", "sfx", "video")
    /// </summary>
    string TagName { get; }
    
    /// <summary>
    /// Chiamato quando il sistema attiva questo handler per una chiamata
    /// </summary>
    void Activate(string callId);
    
    /// <summary>
    /// Chiamato quando la chiamata termina
    /// </summary>
    void Deactivate();
    
    /// <summary>
    /// Stato corrente
    /// </summary>
    bool IsActive { get; }
}
```

---

## 11. CONTATTI

**Lead Developer**: Michele Grimaldi  
**Project**: DEAD AIR  
**Studio**: E-C-H-O SYSTEMS
