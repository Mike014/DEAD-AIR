### 1. Valutazione Generale e Priorità
Il concetto è oro per una demo breve (15-20 min): bassa barriera visiva, alto impatto emotivo, perfetto per itch.io o festival (tipo IGF o IndieCade). Il rischio principale è bilanciare l’ENTITÀ: deve sentirsi “viva” e imprevedibile, ma non frustrante (es. troppi disturbi che coprono il dialogo caller).

**Priorità immediate** (seguendo le tue fasi):
- FASE 1 (Validazione ENTITÀ): Fallo ora, è il cuore del gioco.
- Poi FASE 2 (Virtual Audio Room): Qui puoi shine con la tua expertise in VBAP/Atmos simulation.
- Evita di bloccarti sulla UI retro perfetta all’inizio – usa placeholder (es. canvas grigio con icone basic).

Consiglio strategico: Punta a una Vertical Slice con Call 1 + Call 3 complete + ENTITÀ attiva. Playtestala presto con amici (headphones obbligatorie!) per validare la tensione.

### 2. Sistema ENTITÀ – Il Core Mechanic
Questo è il tuo “agentic system” – un AI semplice ma efficace. Non serve ML complesso (troppo heavy per demo); usa un tick-based decision maker con stati emotivi.

**Ragionamento passo-passo**:
- Input: tension (0-1), silence_sec, call_active, choice_modifiers.
- Stati: Curious (esplora audio sottile), Bored (aumenta silenzio → build-up), Irritated (esplosa in multi-channel: audio + UI + call interference).
- Transizioni: Usa probability curves (AnimationCurve in inspector) per rendere non-lineare. Es. probabilità azione = base * (1 + tension) * f(silence_sec).

**Implementazione consigliata in C#**:
Crea un EntityBrain MonoBehaviour singleton.

```csharp
public class EntityBrain : MonoBehaviour {
    [SerializeField] private AnimationCurve actionProbabilityCurve; // X: tension, Y: multiplier
    [SerializeField] private float baseActionChance = 0.05f; // Per frame in idle
    private float currentTension = 0f;
    private float silenceTimer = 0f;
    private EntityState currentState = EntityState.Curious;

    private void Update() {
        silenceTimer += Time.deltaTime;
        float actionRoll = Random.value;
        float modifiedChance = baseActionChance * actionProbabilityCurve.Evaluate(currentTension) * (1 + silenceTimer / 60f); // Esempio: più silenzio, più chance

        if (actionRoll < modifiedChance) {
            TriggerAction();
            silenceTimer = 0f; // Reset per build-up
        }
    }

    private void TriggerAction() {
        switch (currentState) {
            case EntityState.Curious: SpawnSubtleAudio(); break;
            case EntityState.Irritated: MultiChannelBurst(); break;
            // Aggiungi weighting random basato su tension
        }
        // Qui aggiorna stato con hysteresis (non switchare troppo spesso)
    }
}
```

**Lezione Unity profonda**: Usa coroutine per azioni timed (es. footsteps che si avvicinano gradualmente). Collega a un Event System custom (ScriptableObject events) così ENTITÀ dispatcha “OnEntityAction” che SpatialRoom e UIController ascoltano. Questo decouple tutto – scalabile per future feature.

Bilanciamento: Playtest con debug UI (slider tension manuali). L’ENTITÀ deve “osservare” più che punire – il silenzio prolungato deve far dubitare il player: “È un bug o mi sta guardando?”

### 3. Virtual Audio Room & Spatial Presence
Qui applichi direttamente i tuoi white paper (Dolby Atmos simulation, VBAP). Perfetto per te.

**Ragionamento**:
- Target: Headphones binaural (90% giocatori horror). Unity native spatializer è ok per basic 3D, ma per true object-based 7.1.2 virtuale + height, considera plugin.
- Custom VBAP è valido per controllo totale, ma costoso in CPU se molti objects.

**Consigli pratici 2026**:
- Non reinventare tutto: Usa Microsoft Spatial Audio plugin (open-source su GitHub, aggiornato 2024-2025) o Meta XR Audio SDK (ha Ambisonics sample eccellente). Supportano HRTF personalizzabile e room modeling.
- Se resti pure Unity: Implementa VBAP semplice per i tuoi 9 virtual speakers (L/C/R, Ls/Rs, Lrs/Rrs, Lh/Rh).
  - Crea 9 AudioSource “virtuali” fissi intorno al player (non renderizzati).
  - Per ogni sound object: Calcola direction vector → risolvi gains con matrice 3x3 (per triplet speakers) → applica volume.
  - Pool: Usa Object Pooling (non Instantiate/Destroy ogni suono – kill performance).

**Codice base VBAP snippet** (da estendere):
```csharp
Vector3 direction = (objectPos - listenerPos).normalized;
float[] gains = CalculateVBAPGains(direction, speakerDirections); // Implementa algoritmo Pulkki
for (int i = 0; i < virtualSpeakers.Length; i++) {
    virtualSpeakers[i].volume = gains[i];
}
```

Per phone voice: Sempre centro (C channel), ma con low-pass + static per linea vintage.

**Ottimizzazione**: Tutti suoni mono/dry. Usa AudioMixer groups per layering (bed costante vs dynamic objects). Per web build, testa WebGL audio limitations (latency alta – pre-load tutto).

### 4. UI Desktop Retro & Disturbi
uGUI è perfetto per Windows 95 style.

- Elementi: Canvas World Space? No – Screen Space Overlay per performance.
- CRT shader: Shader Graph → scanlines + curvature + flicker parametrico (collega a ENTITÀ via material.SetFloat).
- Disturbi pool: Crea un DisturbanceManager che queue azioni (es. MoveIcon lentamente con DOTween – free e potente).
- Cursor drift: Raro, ma terrorizzante – usa EventSystem.SetSelectedGameObject(null) + custom cursor che lerp verso random pos.

### 5. Call System & Voice Processing
Struttura ScriptableObjects per nodi dialogo è ottima (Yarn Spinner alternative se vuoi branching visuale).

- ElevenLabs pre-gen: Sicuro per demo (latenza zero). Genera varianti (normal/distorted).
- Realtime processing: AudioMixer con exposed parameters (low-pass cutoff, pitch, distortion). ENTITÀ modula via script:
```csharp
mixer.SetFloat("DistortionAmount", entityIrritationLevel);
```

---
