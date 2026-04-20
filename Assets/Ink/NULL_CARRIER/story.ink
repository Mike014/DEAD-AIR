// ============================================
// NULL CARRIER
// Un frammento di DEAD AIR — E-C-H-O SYSTEMS
// The Eye Jam II — tema: Decay
// ============================================
// Tag system (da DialogueParser.cs di DEAD AIR):
// #amb:nome    — loop ambience (avvia/sostituisce)
// #amb:stop    — ferma ambience
// #sfx:nome    — effetto sonoro one-shot
// #voice:nome  — clip vocale personaggio
// #ui:comando  — comando interfaccia
//
// FREQUENZA TARGET: 4625 kHz — UVB-76 / The Buzzer
// Combinazione corretta:  4 - 6 - 2 - 5
// Combinazione sbagliata: 4 - 6 - 5 - 2
// Posizioni 1 e 2: ordine indicato da Mael
// Posizioni 3 e 4: da dedurre dal giocatore
// ============================================
// === freq_X ===
// #amb: + #sfx:          → audio di ingresso
// [testo apertura]       → il ricordo/momento

// + [Domanda 1]          → sticky, divert allo stitch
// + [Domanda 2]          → sticky, divert allo stitch  
// + [Domanda 3]          → sticky, divert allo stitch
// + {tutte visitate}     → [Continua...] condizionato
// - -> freq_X            → fallback, riporta al loop

// = domanda_1            → stitch con testo
// -> freq_X
// = domanda_2
// -> freq_X
// = domanda_3
// -> freq_X
// = conclusione          → rivela il numero
// #sfx:static_crackle
// -> hub

VAR freq_one_number = 4
VAR freq_two_number = 6
VAR freq_three_number = 2
VAR freq_four_number = 5

-> apertura

// ============================================
// APERTURA
// ============================================

=== apertura ===
#amb:harlan_radio_room
#sfx:radio_static_low

La statica riempie il garage.
Un anno che non apro le finestre.
Non mi lasciano solo — i suoni, le frequenze, il ronzio.
Il taccuino di Mael e' aperto sul banco.
Quattro frequenze. Le ultime cose che ha scritto.
Stanotte provo ancora.

-> hub

// ============================================
// HUB — lista frequenze dagli appunti di Mael
// Sequenza stopping:
// prima visita  → frase completa
// seconda visita → frase breve
// terza visita+ → silenzio
// ============================================

=== hub ===
#sfx:paper_rustle

{La scrittura di Mael. Quattro frequenze sulla carta.|Il taccuino.|}

+ [Frequenza 1 — {freq_one > 0: esplorata|---.---}]
    -> freq_one
+ [Frequenza 2 — {freq_two > 0: esplorata|---.---}]
    -> freq_two
+ [Frequenza 3 — {freq_three > 0: esplorata|---.---}]
    -> freq_three
+ [Frequenza 4 — {freq_four > 0: esplorata|---.---}]
    -> freq_four

+ {freq_one > 0 && freq_two > 0 && freq_three > 0 && freq_four > 0}
    [Componi la frequenza finale]
    -> finale_loop

* -> hub

// ============================================
// FREQUENZA 1 — numero: 4
// Ricordo: il bar, il primo incontro.
// Tre domande sticky — tutte esplorabili.
// [Continua...] si sblocca solo dopo le tre.
// ============================================

=== freq_one_intro ===
#amb:freq_signal_warm
#sfx:radio_tune

Non smettevi di parlare del tuo lavoro.
Il cappotto di jeans. Fuori posto in quel bar.
Fuori posto ovunque, in realta'.
Mi era sembrata una qualita'.

-> freq_one

=== freq_one ===
+ [Cosa stava studiando in quella zona?]
    -> freq_one.cosa_studiava
+ [Come mai eri solo?]
    -> freq_one.come_mai_solo
+ [Com'e' finita quella sera?]
    -> freq_one.come_finita
+ {freq_one.cosa_studiava > 0 && freq_one.come_mai_solo > 0 && freq_one.come_finita > 0}
    [Continua...]
    -> freq_one.conclusione
- -> freq_one

= cosa_studiava
Anomalie elettromagnetiche, dicevi.
Zone dove la bussola impazzisce.
Dove le radio captano segnali senza sorgente.
Ridevi mentre lo spiegavi.
Come se fosse la cosa piu' normale del mondo 
...sentire voci dal nulla.
-> freq_one

= come_mai_solo
Non te l'ho mai chiesto direttamente.
Ma lo sapevo.
Chi sceglie di lavorare in posti dove non arriva nessun segnale non sta scappando dal lavoro.
-> freq_one

= come_finita
Non lo so spiegare con precisione.
A un certo punto avevi smesso di parlare del lavoro.
Mi guardavi.
Fuori pioveva.
Nessuno dei due si era alzato per andarsene.
-> freq_one

= conclusione
Nel taccuino, accanto alla frequenza, hai scritto un numero.
Solo quello. Nessuna spiegazione.
Il primo: 4.
#sfx:static_crackle
-> hub

// ============================================
// FREQUENZA 2 — numero: 6
// Tono: leggibile, ma qualcosa non torna.
// ============================================

=== freq_two_intro ===
#amb:freq_signal_warm
#sfx:radio_tune

-> freq_two

=== freq_two ===

// [TESTO FREQ 2] — da scrivere.

#sfx:static_crackle
-> hub

// ============================================
// FREQUENZA 3 — numero: 2
// Solo il numero. Nessun ordine.
// Tono: freddo, distorto — decay inizia.
// ============================================

=== freq_three ===
#amb:freq_signal_cold
#sfx:radio_tune

// [TESTO FREQ 3] — da scrivere.

#sfx:static_heavy
-> hub

// ============================================
// FREQUENZA 4 — numero: 5
// Solo il numero. Nessun ordine.
// Tono: quasi statica pura.
// ============================================

=== freq_four ===
#amb:freq_signal_cold
#sfx:radio_tune

// [TESTO FREQ 4] — da scrivere.

#sfx:static_heavy
-> hub

// ============================================
// LOOP FINALE
// Numeri: 4, 6, 2, 5
// Posizioni 1 e 2 fisse (Mael).
// Posizioni 3 e 4 da ordinare (giocatore).
// ============================================

=== finale_loop ===
#amb:harlan_radio_room
#sfx:radio_static_low

// [TESTO INTRO LOOP] — da scrivere.

+ [4 - 6 - 5 - 2]
    #sfx:static_silence
    -> finale_loop

* [4 - 6 - 2 - 5]
    -> finale

// ============================================
// FINALE
// "L'ossessione per i morti consuma i vivi."
// ============================================

=== finale ===
#amb:stop
#sfx:carrier_signal
#ui:dead_air_screen

// [TESTO FINALE] — da scrivere.

-> END