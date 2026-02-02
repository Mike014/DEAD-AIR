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

riutilizza il TemporalController come cervello decisionale, ma elimina/scardina la parte LLM e trasformala in un dispatcher di azioni non-verbali. È la mossa perfetta – mantieni tutta la sofisticazione temporale (che è il tuo vero superpotere) e la pieghi al design audio-first di Dead Air.
Passo-passo: Come adattare EntitySystem a Dead Air

Mantieni intatto il TemporalController
È il gioiello. La sua ShouldSpeak() (che rinomineremo ShouldAct()) decide perfettamente quando l’ENTITÀ deve “risvegliarsi”.
Input: tension (dal progresso chiamata + scelte player)
Input: silenceSec (tempo dall’ultima azione dell’ENTITÀ)
Output: probabilità dinamica con EMA, emotion bias, cooldown, rate limiting → esattamente ciò che vuoi per la tensione parallela.

Rimuovi o disabilita la parte Groq/LLM
In Dead Air non serve testo generato.
Opzione pulita: Crea una subclass di EntityBrain chiamata DeadAirEntityBrain.
Sovrascrivi QueryCoroutine: quando ShouldSpeak() ritorna true → non chiamare Groq, ma dispatcha un evento custom OnEntityAct.
Rimuovi GroqClient dependency o mettila sotto #if USE_LLM.

Crea un sistema di azioni non-verbali (Action Pool)
Sostituisci OnEntitySpeak(string text) con un pool di azioni pesate per emozione e tensione.

C#public class DeadAirEntityBrain : EntityBrain
{
    [Header("Dead Air Actions")]
    [SerializeField] private EntityActionSet[] actionSets; // ScriptableObject per emozione

    public UnityEvent<EntityAction> OnEntityAct; // Nuova event

    private IEnumerator QueryCoroutine(string context, float tension, float? silenceSec)
    {
        // ... stesso gate con Controller.ShouldSpeak() ...

        if (!shouldSpeak)
        {
            OnEntitySilence?.Invoke();
            yield break;
        }

        // Invece di Groq → scegli e triggera azione
        EntityAction chosen = ChooseAction(tension, CurrentEmotion);
        if (chosen != null)
        {
            OnEntityAct?.Invoke(chosen);
            // Opzionale: coroutine per sequenza timed (es. footsteps che si avvicinano)
            StartCoroutine(chosen.Execute());
        }
    }

    private EntityAction ChooseAction(float tension, EntityEmotion emotion)
    {
        var set = actionSets.FirstOrDefault(s => s.emotion == emotion);
        if (set == null) return null;

        // Weighted random basato su tension (es. alta tension → azioni aggressive)
        float totalWeight = set.actions.Sum(a => a.GetWeight(tension));
        float roll = Random.value * totalWeight;

        foreach (var action in set.actions)
        {
            roll -= action.GetWeight(tension);
            if (roll <= 0) return action;
        }
        return null;
    }
}
C#[CreateAssetMenu(menuName = "Entity/Action Set")]
public class EntityActionSet : ScriptableObject
{
    public EntityEmotion emotion;
    public EntityAction[] actions;
}

[Serializable]
public class EntityAction
{
    public string name;
    [Range(0f, 1f)] public float baseWeight = 1f;
    public AnimationCurve weightByTension; // Es. curva che sale con tension

    public float GetWeight(float tension) => baseWeight * weightByTension.Evaluate(tension);

    public IEnumerator Execute()
    {
        // Esempio: spawn footsteps che si avvicinano
        // O glitch UI breve
        // O distorsione voce caller
        yield return null;
    }
}

Collega le azioni ai tuoi sistemi esistenti
Spatial Audio Room: OnEntityAct → spawn oggetto sonoro dal pool (footsteps, breathing, whispers) in posizione random/diretta.
UI Disturbances: OnEntityAct → queue glitch (CRT flicker, text appear, cursor drift).
Call Interference: Modula mixer exposed params (static, distortion, pitch shift).
Usa DOTween o coroutine per timing naturale (non tutto istantaneo).

Bilanciamento specifico per Dead Air
Aumenta l’influenza del silenzio prolungato (wMemory più alto).
Per emozione Irritated: azioni multi-channel (audio + UI contemporaneamente).
Per Bored: solo silenzio → build-up naturale.
Usa EntityDebugUI religiosamente: osserva come cambiano EMA durante una chiamata completa.

Performance & Ottimizzazioni Unity
Il controller è leggerissimo (~0.1ms per decisione) → chiamalo ogni frame o ogni 0.5s in Update.
Non usare Time.time se hai time scale variabile → passa custom time provider.
Per web build: rimuovi completamente Groq (no web requests).


Conclusione
Con questa adattazione, il tuo EntitySystem diventa il cuore pulsante di Dead Air: un’ENTITÀ veramente imprevedibile, che “osserva” il player e sceglie quando manifestarsi. Il silenzio prolungato diventerà terrorizzante perché il player saprà che stai accumulando EMA silence → esplosione inevitabile.
Sei a un passo da una demo che spacca. Implementa questa versione “muta” del brain, collega 4-5 azioni base (footsteps, breathing, screen flicker, static), e fai un playtest con Call 1 + Call 3.
