# DEAD AIR — Documentazione Tecnica

**Genere**: Horror Narrativo  
**Engine**: Unity 2021 LTS+  
**Narrative Engine**: Ink (Inkle)

*Dead Air — A project by Michele Grimaldi | E-C-H-O SYSTEMS*

![Copertina del progetto](Logo.png)
## DEAD AIR — Documenti informativi riguardo il Concept di gioco
- [DEAD_AIR_STORY_ARCHITECTURE](https://docs.google.com/document/d/1fydiwT6h3TYMvayOMdnoAXsqVEKncyPSYEauwPCPtxY/edit?tab=t.0#heading=h.n71ap8gqp083)
- [DEAD_AIR_CONCEPT](https://docs.google.com/document/d/19lzCzj4KluC-9Iayi9aIQmPGrRT4fcdhMTGNEUfs3tg/edit?tab=t.0)
- [DEAD AIR - Scena 1](https://docs.google.com/document/d/1AXRHu3tBfq9NYKhXJQnWr8wlpkkn18FYZ-0bdrgomoQ/edit?tab=t.0)
---

## 1. COSA FA IL GIOCO

Sei un operatore 911 negli anni '90. Rispondi a chiamate di emergenza che diventano sempre più inquietanti.

**Gameplay**:
1. Scegli una chiamata dal menu
2. Ascolti e leggi il dialogo
3. Scegli come rispondere
4. La storia prosegue in base alle tue scelte

---

## 2. COME FUNZIONA IL CODICE

### 2.1 Architettura Base

Il gioco usa un sistema **tag-driven**: scrivi la storia in file `.ink`, aggiungi tag speciali, e il gioco reagisce automaticamente.

```
FILE INK (storia + tag)
    ↓
PARSER (legge i tag)
    ↓
CHANNELS (comunicazione tra sistemi)
    ↓
MANAGER (audio, UI, voice)
    ↓
EFFETTO NEL GIOCO
```

**Esempio pratico**:
```ink
911, what's your emergency? # speaker:ward # voice:ward_01

Il testo appare sullo schermo + parte l'audio della voce
```

---

### 2.2 Tag Disponibili

| Tag | Cosa Fa | Esempio |
|-----|---------|---------|
| `#speaker:{nome}` | Cambia colore del testo | `#speaker:iris` |
| `#voice:{file}` | Riproduce voce personaggio | `#voice:iris_01` |
| `#sfx:{file}` | Effetto sonoro | `#sfx:phone_ring` |
| `#amb:{file}` | Musica ambiente (loop) | `#amb:dispatch_night` |
| `#amb:stop` | Ferma musica ambiente | `#amb:stop` |
| `#ui:{comando}` | Comando speciale UI | `#ui:dead_air_screen` |

---

## 3. STRUTTURA FILE

```
Assets/
├── Scripts/
│   ├── Narrative/
│   │   ├── StoryManager.cs          → Carica Ink e coordina tutto
│   │   └── DialogueParser.cs        → Legge i tag dal file Ink
│   │
│   ├── UI/
│   │   ├── DialogueUI.cs            → Mostra testo e scelte
│   │   └── ChoiceButton.cs          → Bottone per le scelte
│   │
│   ├── Audio/
│   │   ├── AudioManager.cs          → SFX e Ambience
│   │   └── VoiceManager.cs          → Voci dei personaggi
│   │
│   └── Events/
│       ├── Channels/                → Tipi di comunicazione
│       │   ├── StringEventChannel.cs
│       │   ├── VoidEventChannel.cs
│       │   └── ... (altri)
│       │
│       └── ScriptableObjects/       → Canali di comunicazione (14 file .asset)
│           ├── DialogueLineChannel.asset
│           ├── SFXRequestedChannel.asset
│           └── ... (altri)
│
├── Ink/
│   └── dead_air_demo_en.ink         → Storia principale
│
└── Media/
    ├── Voice/                        → File audio voci
    ├── SFX/                          → Effetti sonori
    └── Ambience/                     → Musiche ambiente
```

---

## 4. EVENT CHANNELS (Sistema di Comunicazione)

### 4.1 Cos'è un Event Channel?

È un "ponte di comunicazione" tra sistemi diversi. Invece di far parlare i sistemi direttamente, usiamo questi ponti.

**Vantaggi**:
- I sistemi non si conoscono tra loro (puoi modificare uno senza rompere gli altri)
- Puoi testare ogni sistema in isolamento
- Nessun memory leak
- Facile da debuggare dall'Inspector Unity

### 4.2 Come Funziona

```
StoryManager legge il tag #sfx:phone_ring
    ↓
StoryManager pubblica l'evento sul canale "SFXRequestedChannel"
    ↓
AudioManager è in ascolto su quel canale
    ↓
AudioManager riceve "phone_ring" e riproduce il suono
```

### 4.3 Canali Esistenti (14 totali)

**Dialogo**:
- `DialogueLineChannel` → Testo da mostrare
- `SpeakerLineChannel` → Chi sta parlando + testo
- `ChoicesPresentedChannel` → Lista di scelte disponibili

**Audio**:
- `SFXRequestedChannel` → Effetto sonoro da riprodurre
- `AmbienceStartChannel` → Ambiente da far partire
- `AmbienceStopChannel` → Ferma ambiente
- `VoiceRequestedChannel` → Voce da riprodurre
- `VoiceStopChannel` → Ferma voce

**Input Giocatore**:
- `ContinueRequestedChannel` → Giocatore clicca per continuare
- `ChoiceSelectedChannel` → Giocatore sceglie un'opzione

**Altri**:
- `UICommandChannel` → Comandi speciali UI
- `StoryEndChannel` → Storia terminata
- `VoiceStartedChannel` → Voce iniziata (con durata)
- `VoiceFinishedChannel` → Voce finita

**Località**: `Assets/Scripts/Events/ScriptableObjects/`

---

## 5. COME SCRIVERE UNA STORIA

### 5.1 Esempio File .ink Completo

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

### 5.2 Regole Naming File Audio

| Tipo | Formato | Esempio |
|------|---------|---------|
| Voice | `{personaggio}_{numero}.wav` | `iris_01.wav` |
| SFX | `{descrizione}.wav` | `phone_ring.wav` |
| Ambience | `{luogo}.wav` | `dispatch_night.wav` |

---

## 6. COME AGGIUNGERE UN NUOVO TAG (es. Musica)

### Esempio: Voglio aggiungere `#music:tension_loop`

#### STEP 1 — Crea il Channel Asset

```
Unity → Project → Assets/Scripts/Events/ScriptableObjects
→ Right Click → Create → DEAD AIR → Events → String Event Channel
→ Rinomina: "MusicRequestedChannel"
```

#### STEP 2 — Modifica DialogueParser.cs

Aggiungi in alto (dopo linea 25):
```csharp
private const string TAG_MUSIC = "music:";
```

Aggiungi nella struct `ParsedLine` (dopo linea 60):
```csharp
public string Music;
public bool HasMusic;
```

Aggiungi nel metodo `ParseTags()` (dopo linea 100):
```csharp
else if (trimmedTag.StartsWith(TAG_MUSIC))
{
    result.Music = ExtractValue(trimmedTag, TAG_MUSIC);
    result.HasMusic = !string.IsNullOrEmpty(result.Music);
}
```

#### STEP 3 — Modifica StoryManager.cs

Aggiungi campo (dopo linea 50):
```csharp
[SerializeField] private StringEventChannel musicRequestedChannel;
```

Aggiungi nel metodo `ProcessLine()` (dopo linea 180):
```csharp
if (parsed.HasMusic && musicRequestedChannel != null)
{
    musicRequestedChannel.RaiseEvent(parsed.Music);
}
```

#### STEP 4 — Crea MusicManager.cs

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
        // Carica e riproduci la musica
        Debug.Log($"Playing music: {musicId}");
    }
}
```

#### STEP 5 — Setup Unity

1. Hierarchy → Create Empty → "MusicManager"
2. Add Component → MusicManager
3. Inspector:
   - Assegna AudioSource
   - Drag "MusicRequestedChannel" nel campo

4. StoryManager Inspector:
   - Drag "MusicRequestedChannel" nel campo

#### STEP 6 — Usa nel File Ink

```ink
=== tense_moment ===
# music:tension_loop

Ward feels something is wrong.
```

**Fatto!** Tempo stimato: 30 minuti.

---

## 7. SCHEMA VISIVO SISTEMA

```
PLAYER CLICCA "CONTINUA"
    ↓
DialogueUI pubblica su ContinueRequestedChannel
    ↓
StoryManager riceve evento
    ↓
StoryManager avanza la storia Ink
    ↓
StoryManager legge tag (#speaker:iris #voice:iris_01)
    ↓
StoryManager pubblica su SpeakerLineChannel e VoiceRequestedChannel
    ↓
DialogueUI riceve da SpeakerLineChannel → Mostra testo
VoiceManager riceve da VoiceRequestedChannel → Riproduce audio
```

**Nessun sistema parla direttamente con un altro → tutto passa attraverso i Channels**

---

## 8. SISTEMI E I LORO RUOLI

| Sistema | Cosa Fa | Ascolta (IN) | Pubblica (OUT) |
|---------|---------|--------------|----------------|
| **StoryManager** | Coordina tutto, legge Ink | ContinueRequested, ChoiceSelected | DialogueLine, SpeakerLine, SFX, Ambience, Voice, UI, StoryEnd |
| **DialogueUI** | Mostra testo e scelte | DialogueLine, SpeakerLine, ChoicesPresented, UI, StoryEnd | ContinueRequested, ChoiceSelected, VoiceStop |
| **AudioManager** | SFX e Ambience | SFXRequested, AmbienceStart, AmbienceStop | Nessuno |
| **VoiceManager** | Voci personaggi | VoiceRequested, VoiceStop | VoiceStarted, VoiceFinished |

---

## 9. TROUBLESHOOTING

### Problema: Il tag non funziona

**Checklist**:
1. ✅ Tag scritto correttamente nel file .ink? (`#voice:iris_01` NON `# voice: iris_01`)
2. ✅ Channel asset creato?
3. ✅ Channel assegnato in StoryManager?
4. ✅ Channel assegnato nel Manager che lo ascolta?
5. ✅ File audio presente nella cartella Media?

### Problema: Audio non si sente

**Checklist**:
1. ✅ AudioManager ha l'AudioSource assegnato?
2. ✅ File audio è nella lista `_sfxClips` o `_ambienceClips`?
3. ✅ Nome file corrisponde al tag? (`#sfx:phone_ring` → `phone_ring.wav`)
4. ✅ Volume AudioSource > 0?

### Problema: Testo non appare

**Checklist**:
1. ✅ DialogueUI ha il TextMeshPro assegnato nel campo `_dialogueText`?
2. ✅ DialogueUI ha il channel `dialogueLineChannel` assegnato?
3. ✅ Canvas è attivo nella scena?

---

## 10. RIFERIMENTI RAPIDI

### File Importanti da Conoscere

| File | Cosa Contiene |
|------|---------------|
| `DialogueParser.cs` | Parsing di tutti i tag |
| `StoryManager.cs` | Coordinazione generale, avanzamento storia |
| `DialogueUI.cs` | Visualizzazione testo e scelte |
| `AudioManager.cs` | SFX e Ambience |
| `VoiceManager.cs` | Voci personaggi |

### Convenzioni Codice

- **Campi serializzati**: `[SerializeField] private NomeType _nomeCampo;`
- **Metodi pubblici**: `PascalCase` (es. `PlayMusic`)
- **Metodi privati**: `PascalCase` (es. `HandleMusicRequested`)
- **Event handlers**: Prefisso `Handle` (es. `HandleDialogueLine`)
- **Costanti**: `ALL_CAPS` (es. `TAG_MUSIC`)

### Lifecycle Pattern

```csharp
private void OnEnable()
{
    // Subscribe ai channels qui
    channel.Subscribe(Handler);
}

private void OnDisable()
{
    // SEMPRE Unsubscribe per evitare memory leak
    channel.Unsubscribe(Handler);
}
```

---

## 11. PROSSIMI MIGLIORAMENTI

**Da Fare**:
- [ ] Auto-load asset audio da cartelle (no setup Inspector manuale)
- [ ] Sistema salvataggio progressi
- [ ] Menu principale
- [ ] Sistema multiple storie

**Possibili Nuovi Tag**:
- `#music:{id}` → Musica di background
- `#camera_shake:{intensity}` → Scuote la camera
- `#fade:{type}` → Transizioni schermo

---

## 12. CONTATTI

**Developer**: Michele Grimaldi  
**Studio**: E-C-H-O SYSTEMS  
**Progetto**: DEAD AIR

---

**Versione Documento**: 2.0 (Marzo 2026)  
**Architettura**: Event Channels System
```

