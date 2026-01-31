# DEAD AIR — Game Design Document

**Titolo**: Dead Air
**Genere**: Horror Narrativo / Audio-First Experience
**Piattaforma Target**: PC (Windows/Mac), potenzialmente Web
**Engine**: Unity 2021 LTS+
**Durata Demo**: 15-20 minuti (4-5 chiamate)

![Copertina del progetto](Gemini_Generated_Image_p2cpe0p2cpe0p2cp.png)

---

## 1. HIGH CONCEPT

Un operatore del 911 lavora il turno di notte in una centrale operativa degli anni '90. Risponde a chiamate che diventano progressivamente inquietanti. Il gameplay è **audio-first**: il giocatore ascolta, sceglie risposte multiple, e subisce disturbi sia sonori (oggetti che si muovono nella stanza virtuale) che visivi (interferenze sul desktop).

**Ispirazione principale**: Serie "Calls" (Apple TV+, 2021) — narrativa pura attraverso audio e trascrizioni visuali.

**Differenziatore**: Sistema ENTITÀ che decide autonomamente quando e come disturbare il giocatore, creando tensione non-scriptata.

---

## 2. PILLARS

### 2.1 Audio as Gameplay
L'audio non è accompagnamento — è il gioco. Il giocatore deve **ascoltare** per capire cosa sta succedendo. La voce del caller, i rumori nella stanza, il silenzio: tutto comunica.

### 2.2 Silence as Tension
Il sistema ENTITÀ usa il silenzio come meccanica. I momenti di quiete non sono vuoti — sono ENTITÀ che osserva, che sceglie di non agire. Quando agisce, il contrasto è maggiore.

### 2.3 Spatial Presence
Il giocatore è **al centro** di una stanza virtuale (Dolby Atmos-style bounding box). Suoni si muovono intorno a lui — passi, respiri, sussurri. Non vede nulla, ma sente tutto.

### 2.4 UI as Diegesis
Il desktop Windows 95 non è solo interfaccia — è parte del mondo di gioco. ENTITÀ può manifestarsi attraverso glitch, testi che appaiono, icone che si muovono.

---

## 3. GAMEPLAY LOOP

```
┌─────────────────────────────────────────────────────────────┐
│                     SESSIONE DI GIOCO                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  1. IDLE                                                    │
│     └── Desktop vuoto, ambiente sonoro della centrale       │
│     └── ENTITÀ: bassa probabilità di azione                 │
│                                                             │
│  2. CHIAMATA IN ARRIVO                                      │
│     └── Squillo telefono (spazializzato)                    │
│     └── UI: notifica incoming call                          │
│                                                             │
│  3. RISPOSTA                                                │
│     └── Player clicca per rispondere                        │
│     └── Audio: dial tone → connessione                      │
│                                                             │
│  4. DIALOGO                                                 │
│     └── Voce caller (ElevenLabs, pre-generata)              │
│     └── Player sceglie tra 2-4 risposte multiple            │
│     └── Branching narrativo                                 │
│     └── ENTITÀ: tick ogni frame, può:                       │
│         • Spawnare suoni nella stanza                       │
│         • Interferire con UI                                │
│         • Distorcere la voce del caller                     │
│                                                             │
│  5. FINE CHIAMATA                                           │
│     └── Caller riattacca (o viene interrotto)               │
│     └── Ritorno a IDLE                                      │
│     └── Tensione base aumenta                               │
│                                                             │
│  6. REPEAT (4-5 chiamate)                                   │
│                                                             │
│  7. FINALE                                                  │
│     └── Basato su scelte cumulative + stato ENTITÀ          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. STRUTTURA CHIAMATE (DEMO)

| # | Tipo | Tensione Base | Descrizione | ENTITÀ Behavior |
|---|------|---------------|-------------|-----------------|
| 1 | Normale | 0.2 | Chiamata credibile, problema domestico banale | Quasi assente, occasionale rumore ambientale |
| 2 | Strana | 0.4 | Qualcosa non torna nella storia del caller | Inizia a manifestarsi, suoni sporadici |
| 3 | Inquietante | 0.6 | Il caller descrive cose impossibili | Più attivo, primi disturbi UI |
| 4 | Paranormale | 0.8 | Chiaramente sovrannaturale | Molto attivo, interferenze frequenti |
| 5 | Climax | 1.0 | ENTITÀ prende il controllo | Azioni estreme, possibile interruzione chiamata |

### 4.1 Esempio Struttura Chiamata (Call 3 — Inquietante)

```
CALLER: Donna anziana, voce tremante
CALLER_ID: "SCONOSCIUTO"
TENSIONE_BASE: 0.6

NODO_1:
  Audio: "Pronto? C'è qualcuno in casa mia. Lo sento camminare."
  Scelte:
    A) "Signora, rimanga calma. Può descrivermi cosa sente?"
    B) "Ha verificato se ci sono familiari in casa?"
    C) "Chiuda a chiave la porta e si nasconda."

NODO_2A (se scelta A):
  Audio: "Passi. Lenti. Vengono dal corridoio. Ma..."
  [pausa]
  Audio: "...ma il corridoio è al secondo piano. E io sono al terzo."
  Scelte:
    A) "Signora, la sua casa ha tre piani?"
    B) "Rimanga in linea, invio una pattuglia."

NODO_2B (se scelta B):
  Audio: "Mio marito è morto tre anni fa. Sono sola."
  [pausa]
  Audio: "Aspetti... lo sento respirare. È fuori dalla porta."
  Scelte:
    A) "Non apra quella porta."
    B) "Può vedere dalla serratura?"

[continua con branching...]

POSSIBILI FINALI:
  - NORMALE: Pattuglia arriva, casa vuota
  - DISTURBANTE: Linea cade, silenzio
  - PARANORMALE: La voce cambia, non è più la donna
  - ENTITY_TAKEOVER: ENTITÀ interrompe, parla direttamente
```

---

## 5. SISTEMA AUDIO SPAZIALE

### 5.1 Virtual Room (Dolby Atmos Concept)

Il giocatore è posizionato al centro di una stanza virtuale 3D. Non vede la stanza, ma la **sente**.

```
                    CEILING (Height Layer)
                         Lh ─── Rh
                        /         \
                       /   [creaks, drips]
                      /             \
        SURROUND    Ls ─── PLAYER ─── Rs    SURROUND
        (whispers,   |    (centro)    |     (breathing,
         footsteps)  |                |      knocks)
                     |                |
                    Lrs ─────────── Rrs
                         REAR
                    
        FRONT        L ───── C ───── R
                    (phone, (caller  (phone,
                     dial)   voice)   static)
```

### 5.2 Configurazione 7.1.2 Virtuale

| Speaker | Posizione | Angolo | Uso |
|---------|-----------|--------|-----|
| L | Front Left | -30° | Telefono, ambiente |
| R | Front Right | +30° | Telefono, ambiente |
| C | Center | 0° | Voce caller (primaria) |
| Ls | Surround Left | -90° | Passi, movimento |
| Rs | Surround Right | +90° | Respiri, presenza |
| Lrs | Rear Left | -150° | Sussurri, minaccia |
| Rrs | Rear Right | +150° | Sussurri, minaccia |
| Lh | Height Left | -30°, +45° elevazione | Scricchiolii soffitto |
| Rh | Height Right | +30°, +45° elevazione | Gocce, rumori sopra |

### 5.3 Audio Object Pool

| Oggetto | Movimento | Spawn Conditions | Note |
|---------|-----------|------------------|------|
| Footsteps | Orizzontale, lento | tension > 0.3 | Si avvicinano gradualmente |
| Breathing | Statico | tension > 0.4 | Posizione random, rimane |
| Whispers | Circolare | tension > 0.5 | Gira intorno al player |
| Knocks | Discreto (pareti) | tension > 0.5 | 3 colpi, pausa, ripete |
| Creaks | Random (ceiling) | tension > 0.6 | Sporadico |
| Heartbeat | Centro (player) | tension > 0.7 | Crescendo |
| Static | Centro (phone) | ENTITÀ decide | Interferenza chiamata |
| DistantScream | Random, lontano | tension > 0.9, raro | Climax only |

---

## 6. SISTEMA ENTITÀ

### 6.1 Ruolo

ENTITÀ non è il caller. ENTITÀ è la **presenza nella stanza** — qualcosa che osserva l'operatore mentre lavora. Non interagisce con la narrativa delle chiamate; crea un secondo layer di tensione parallelo.

### 6.2 Input

| Parametro | Fonte | Range |
|-----------|-------|-------|
| tension | Numero chiamata + progresso | 0.0 - 1.0 |
| silence_sec | Tempo dall'ultima manifestazione | 0 - ∞ |
| call_active | Chiamata in corso? | bool |
| player_choice_tension | Modificatore dalla scelta | -0.1 / +0.1 |

### 6.3 Output Channels

**Audio (Spatial Room)**:
- Spawn oggetti sonori nel bounding box
- Movimento oggetti esistenti
- Layering di suoni ambientali

**UI (Desktop)**:
- Glitch visivi (CRT distortion)
- Testo che appare ("Ti vedo", "Dietro di te")
- Icone che si muovono
- Finestre che si aprono
- Cursore che si muove da solo (raro, climax)

**Call Interference**:
- Static sulla linea
- Distorsione voce caller
- Echo innaturale
- Drop della chiamata (estremo)

### 6.4 Comportamento per Emozione

| Stato ENTITÀ | Probabilità Base | Canali Preferiti | Intensità |
|--------------|------------------|------------------|-----------|
| CURIOUS | Media | Audio ambientale | Sottile |
| BORED | Bassa | Silenzio | Minima |
| IRRITATED | Alta | UI + Call interference | Aggressiva |

---

## 7. SISTEMA UI

### 7.1 Desktop Windows 95

**Elementi fissi**:
- Sfondo: colore solido (teal/grigio scuro)
- Icone: Telefono, Cartella Casi, Cestino, Impostazioni
- Taskbar: Start button, clock (ora reale di gioco), tray icons
- Bordi finestra: stile Windows 95 autentico

**Phone Application**:
- Finestra dedicata, sempre visibile durante chiamata
- Caller ID display
- Timer chiamata
- Area transcript (testo caller)
- Bottoni risposta multipla
- Bottone "Riattacca"

### 7.2 Effetti Visivi

| Effetto | Trigger | Intensità |
|---------|---------|-----------|
| CRT Scanlines | Sempre attivo | Costante, leggero |
| Screen Flicker | ENTITÀ action | Breve, raro |
| Color Shift | Alta tensione | Graduale |
| Glitch Blocks | ENTITÀ irritated | Aggressivo |
| Text Appear | ENTITÀ action | Fade in/out |
| Icon Movement | ENTITÀ action | Lento, sottile |

### 7.3 Disturbi UI (Pool)

| Disturbo | Frequenza | Descrizione |
|----------|-----------|-------------|
| TypeText | Media | Testo appare lettera per lettera in posizione random |
| GlitchScreen | Media | Schermo trema per 0.2-0.5s |
| MoveIcon | Rara | Un'icona si sposta lentamente |
| OpenNotepad | Rara | Notepad si apre con messaggio |
| FlickerClock | Media | L'orologio mostra orari sbagliati |
| StaticOverlay | Durante call | Static visivo sovrapposto |
| CursorDrift | Molto rara | Il cursore si muove da solo (climax) |

---

## 8. AUDIO DESIGN

### 8.1 Voce Caller (ElevenLabs)

**Voci necessarie** (4-5 caller distinti):
1. Donna anziana (Call 3) — tremante, vulnerabile
2. Uomo adulto (Call 1) — normale, banale
3. Bambino (Call 4) — inquietante, troppo calmo
4. Voce distorta (Call 5) — ENTITÀ o posseduto
5. Persona in panico (Call 2) — respiro affannoso

**Processing realtime**:
- Pitch shift (per distorsione)
- Reverb (per sensazione di distanza/spazio)
- Low-pass filter (per simulare linea telefonica)
- Granular (per frammentazione paranormale)

### 8.2 Ambiente Centrale Operativa

**Bed layer (costante)**:
- Hum elettrico monitor CRT
- Ventola computer
- Ticchettio orologio (se silenzio)
- Ronzio luci fluorescenti distanti

### 8.3 Oggetti Sonori (Pool)

Tutti i suoni devono essere:
- Mono (per spazializzazione corretta)
- Dry (niente riverbero pre-applicato)
- Normalizzati
- Con varianti (3-5 per tipo)

---

## 9. FASI DI SVILUPPO

### FASE 1: Validazione ENTITÀ System

**Obiettivo**: Verificare che il package Unity funzioni correttamente.

**Cosa fare**:
- Importare EntitySystem package in progetto Unity vuoto
- Creare scena di test minimale
- Configurare TemporalConfig con valori default
- Implementare test harness con slider tensione + display stato
- Verificare gate, emozioni, transizioni
- Documentare eventuali bug/modifiche

**Deliverable**: Scena Unity con EntityBrain funzionante + EntityDebugUI

---

### FASE 2: Virtual Audio Room

**Obiettivo**: Implementare il bounding box spaziale basato sul paper Dolby Atmos.

**Cosa fare**:
- Creare SpatialAudioRoom component
- Implementare VBAP (Vector Base Amplitude Panning)
- Creare AudioObjectPool
- Creare AudioObjectDefinition ScriptableObject
- Test con suoni placeholder
- Collegare a ENTITÀ

**Deliverable**: Stanza virtuale con oggetti sonori controllati da ENTITÀ

---

### FASE 3: UI Desktop Retro

**Obiettivo**: Creare l'interfaccia Windows 95 funzionale.

**Cosa fare**:
- Raccogliere reference Windows 95
- Creare UI base (desktop, taskbar, icone)
- Creare Phone Application window
- Implementare CRT shader
- Creare sistema disturbi UI
- Collegare a ENTITÀ

**Deliverable**: Desktop funzionale con effetti glitch controllati da ENTITÀ

---

### FASE 4: Call System Base

**Obiettivo**: Sistema di chiamate scriptate con branching.

**Cosa fare**:
- Definire struttura dati (ScriptableObjects)
- Creare CallManager (state machine)
- Implementare DialogueRunner
- Creare Call 1 completa come test
- Integrare con ENTITÀ

**Deliverable**: Una chiamata completa giocabile

---

### FASE 5: Voice Processing

**Obiettivo**: Effetti audio realtime sulla voce caller.

**Cosa fare**:
- Implementare AudioMixer setup
- Creare effetti controllabili (filter, distortion, pitch)
- Collegare a ENTITÀ

**Deliverable**: Voce caller con processing dinamico

---

### FASE 6: Content Creation

**Obiettivo**: Creare le 4-5 chiamate della demo.

**Cosa fare**:
- Scrivere script completi per tutte le chiamate
- Generare audio ElevenLabs
- Creare sound effects
- Implementare tutto in Unity
- Bilanciare tensione/ENTITÀ

**Deliverable**: Demo completa

---

### FASE 7: Polish & Testing

**Obiettivo**: Rifinitura e test.

**Cosa fare**:
- Playtest
- Bilanciamento ENTITÀ
- Audio mix finale
- Bug fixing
- Build per itch.io

**Deliverable**: Build demo pubblicabile

---

## 10. TECH STACK

| Area | Tecnologia |
|------|------------|
| Engine | Unity 2021 LTS+ |
| Audio Spaziale | Unity Audio + custom VBAP |
| UI | Unity UI (Canvas) |
| Dialogue | ScriptableObject + custom runner |
| TTS | ElevenLabs (pre-generato) |
| AI Agentic | EntitySystem (custom package) |
| Shaders | CRT effect (shader graph o custom) |
| Version Control | Git + GitHub |

---

## 11. REFERENCE

### Visual
- Windows 95/98 screenshots
- CRT monitor photography
- Serie "Calls" visual style (waveforms, abstract)

### Audio
- Serie "Calls" sound design
- Horror game ambience (Phasmophobia, Visage)
- Phone call recordings vintage

### Narrative
- Serie "Calls" episode structure
- Creepypasta phone-based stories
- SCP Foundation tone

---

## 12. PROSSIMI STEP

1. **Importare EntitySystem** in progetto Unity nuovo
2. **Creare scena test** con EntityBrain + EntityDebugUI
3. **Verificare funzionamento** del sistema
4. **Procedere con Fase 2** quando pronto

---

*Dead Air — A project by Michele Grimaldi*
*E-C-H-O SYSTEMS*

**Tagline**: "The silence on the line isn't a bug. It's a choice."
