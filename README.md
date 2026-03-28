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

## 3. CONVENZIONI INK

### 3.1 Tag Supportati

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

### 3.2 Esempio File .ink

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

### 4. Workflow CI/CD

```bash
# Pre-build validation (Unity CLI)
Unity -batchmode -executeMethod CallValidatorTool.ValidateAllCallsCLI -quit

# Exit code 0 = OK, 1 = errori trovati
```

---

## 5. CONTATTI

**Lead Developer**: Michele Grimaldi  
**Project**: DEAD AIR  
**Studio**: E-C-H-O SYSTEMS
