// ============================================
// DEAD AIR — Demo Script
// ============================================
// Sound Design:
// - Ward: solo testo
// - Iris: voce (ElevenLabs Neural TTS)
// - Ambience: centralino notturno
// - SFX: diegetici (tutto ciò che Ward sente)
// - Niente musica
// ============================================
// Unity Integration:
// - #timed_choice segnala attivazione timer
// - #timeout:4 durata in secondi
// - #default:1 indice scelta se scade
// ============================================

VAR asked_name = false

-> parte_1

// ============================================
// PARTE 1 — Setup
// ============================================

=== parte_1 ===

# amb:dispatch_night

Le 2 di notte. Venerdì che si trascina nel sabato.

Jack Daniel's nel bicchiere di cristallo. Liscio, senza ghiaccio. Il caramello prima, poi il carbone d'acero — quel sentore di legno bruciato che ti pulisce la gola e la testa. Soprattutto la testa.

La centrale è quasi vuota stanotte. Pochi operatori, tutti pigri. Le chiamate si contano sulle dita.

"Signora, un gatto su un albero non è un'emergenza. Avviseremo i vigili."

Qualche rapina ai soliti minimarket di quartiere. Alcol, cibo, nulla di che. Le pattuglie ci campano su queste cose.

Non ero sobrio.

Le cuffie mi davano fastidio. La luce dello schermo mi bruciava gli occhi.

Odiavo quel posto.

Ma l'affitto andava pagato.

# sfx:phone_ring

Linea 3.

-> parte_2

// ============================================
// PARTE 2 — La Chiamata
// ============================================

=== parte_2 ===

Il LED lampeggia. Una, due, tre volte.

-> attesa_1

= attesa_1
+ [ATTENDI]
    Magari si risolve da sola.
    Il LED continua a lampeggiare.
    -> attesa_2
+ [RISPONDI]
    -> rispondi

= attesa_2
+ [ATTENDI]
    Mi asciugo le labbra. Un altro sorso non farebbe male.
    Il LED lampeggia. Insistente.
    -> attesa_3
+ [RISPONDI]
    -> rispondi

= attesa_3
+ [ATTENDI]
    Sospiro. Nessuno risponderà al posto mio. Non stanotte.
    -> forza_risposta
+ [RISPONDI]
    -> rispondi

= forza_risposta
+ [RISPONDI]
    -> rispondi

= rispondi

# sfx:phone_pickup

Sollevo la cornetta. Il peso delle cuffie sulla testa.

911, qual è l'indirizzo dell'emergenza? # speaker:ward

-> parte_3

// ============================================
// PARTE 3 — Iris
// ============================================

=== parte_3 ===

Ciao... volevo solo sapere se l'Orso è ancora arrabbiato. # speaker:iris

Strabuzzo gli occhi. Un altro scherzo. Deve essere.

Piccola, hai sbagliato numero. Questa linea non è per giocare. # speaker:ward

Ma l'Orso è davanti alla porta. Mi ha detto che se parlo mi porta via i giocattoli. Stiamo facendo il gioco del silenzio? # speaker:iris

Il ghiaccio nel bicchiere. Un sorso. Fastidio.

Ascolta, passa il telefono a tua madre o a tuo padre. Ora. # speaker:ward

Non possono. Dormono sul tappeto. # speaker:iris

...

C'è tanto succo di fragola ovunque... l'Orso l'ha rovesciato tutto. # speaker:iris

...

Posso avere una coperta? # speaker:iris

-> parte_4

// ============================================
// PARTE 4 — La Scelta Storta
// ============================================

=== parte_4 ===

Le mani tremano. Il bicchiere è quasi vuoto. Devo rispondere qualcosa.

+ [Chiedi da dove chiama]
    -> risposta_storta
+ [Chiudi la chiamata]
    -> risposta_storta

= risposta_storta

Senti... non ho tempo per le storie. Se non c'è un adulto, devo chiudere la chiamata. Abbiamo procedure da seguire. # speaker:ward

Le parole mi escono storte. L'alcol parla per me.

-> parte_5

// ============================================
// PARTE 5 — La Svolta
// ============================================

=== parte_5 ===

Ma l'Orso ha il mio nome sulla sua lista... non voglio più giocare. # speaker:iris

Qualcosa si spezza dentro di me. Non nel bicchiere. Dentro.

# sfx:glass_break

Il bicchiere cade. Si rompe.

Merda... ascoltami. Non è un gioco. Devi nasconderti. Ora. Vai sotto il letto. Non respirare forte. # speaker:ward

È proprio dietro la porta... sta grattando il legno. # speaker:iris

# sfx:scratch

...

Vedo le sue scarpe. Sono scarpe nere e lucide, come quelle di papà quando andiamo in chiesa. # speaker:iris

La realizzazione mi colpisce come un pugno.

Non è un gioco. Non è mai stato un gioco.

I genitori sul tappeto. Il succo di fragola. L'Orso.

Chiudi gli occhi. Non guardare le scarpe. Pensa a... pensa a un posto bello. # speaker:ward

...

Devo sapere chi è. Devo—

-> scelta_vera

// ============================================
// LA SCELTA VERA
// ============================================
// Unity legge #timed_choice e attiva countdown
// Se scade, forza ChooseChoiceIndex(1) -> [...]
// ============================================

=== scelta_vera ===

+ [Come ti chiami?] # timed_choice # timeout:4 # default:1
    ~ asked_name = true
    -> parte_6_nome
+ [...]
    -> parte_6_anonima

// ============================================
// PARTE 6A — Iris ha un nome
// ============================================

=== parte_6_nome ===

Come ti chiami? # speaker:ward

Iris. # speaker:iris

# sfx:door_open

La porta si apre. Lo sento attraverso la linea.

L'Orso è— # speaker:iris

-> finale

// ============================================
// PARTE 6B — Iris muore anonima
// ============================================

=== parte_6_anonima ===

Apro la bocca. Niente esce.

Il momento passa.

# sfx:door_open

La porta si apre. Lo sento attraverso la linea.

L'Orso è— # speaker:iris

-> finale

// ============================================
// FINALE
// ============================================

=== finale ===

# sfx:scream
# amb:stop

...

Poi niente.

...

...

...

# sfx:dead_air
# ui:dead_air_screen

{ asked_name:
    ...
    ...Iris. # speaker:ward
}

# ui:return_to_menu

-> END
