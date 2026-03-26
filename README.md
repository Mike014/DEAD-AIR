# DEAD AIR — Game Design Document

**Titolo**: Dead Air
**Genere**: Horror Narrativo / Audio-First Experience
**Piattaforma Target**: PC (Windows/Mac), potenzialmente Web
**Engine**: Unity 2021 LTS+
**Durata Demo**: 15-20 minuti (4-5 chiamate)

![Copertina del progetto](Logo.png)

---

## 1. HIGH CONCEPT

Un operatore del 911 lavora il turno di notte in una centrale operativa degli anni '90. Risponde a chiamate che diventano progressivamente inquietanti. Il gameplay è **audio-first**: il giocatore ascolta, sceglie risposte multiple, e subisce disturbi sia sonori (oggetti che si muovono nella stanza virtuale) che visivi (interferenze sul desktop).

*Dead Air — A project by Michele Grimaldi*
*E-C-H-O SYSTEMS*

---

## 2. ARCHITETTURA ATTUALE (pre-refactoring)

```
Assets/
├── Ink/
│   ├── dead_air_demo_en.ink        ← un unico file con TUTTE le chiamate
│   └── IT_version/dead_air_demo.ink
├── Script/
│   ├── Audio/
│   │   ├── AudioManager.cs          ← clip SFX/ambience hardcodate in Inspector
│   │   └── VoiceManager.cs          ← clip voce hardcodate in Inspector
│   ├── Events/NarrativeEvents.cs    ← hub eventi statico
│   ├── Narrative/
│   │   ├── StoryManager.cs          ← carica UN TextAsset fisso
│   │   ├── DialogueParser.cs        ← parser tag (già ben disaccoppiato)
│   │   └── TimeChoiceHandler.cs
│   └── UI/
│       ├── DialogueUI.cs
│       └── ChoiceButton.cs
```

**Limiti:**
- Per aggiungere una nuova chiamata devi aprire l'unico `.ink` e scrivere in coda — il file cresce indefinitamente.
- I clip audio sono registrati manualmente su `AudioManager` e `VoiceManager` nell'Inspector; ogni nuova storia richiede di aggiungere entry a mano.
- `StoryManager` carica un `TextAsset` fisso: non esiste modo di definire dall'Inspector quale storia caricare, in quale ordine, o con quali variabili iniziali.
- Le `TimedChoice` sono configurate solo nei tag Ink; non c'è vista d'insieme ispezionabile di tutte le scadenze temporali.

---

## 3. REFACTORING — Architettura Modulare Data-Driven

### Obiettivo

> **Aggiungere o modificare una chiamata deve richiedere soltanto:**
> 1. Scrivere un nuovo file `.ink` (con i knot della chiamata).
> 2. Creare un asset `EmergencyCallSO` nell'Inspector e configurare i campi.
> 3. Trascinare l'asset nella `CallPlaylistSO` della scena.
>
> **Zero modifiche al codice C#.**

-