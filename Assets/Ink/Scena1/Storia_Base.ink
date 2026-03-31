// ============================================
// DEAD AIR — Call 2 Script
// ============================================
// Sound Design:
// - Ward: text only
// - Caller: voice (ElevenLabs Neural TTS)
// - Iris: voice fragments, spatial audio
// - Ambience: night dispatch center
// - SFX: diegetic
// - No music
// ============================================
// Tags:
// - # amb: — ambience
// - # sfx: — effetti sonori
// - # voice: — clip vocali
// - # ui: — interfaccia
// ============================================

-> intro

// ============================================
// INTRO — L'inizio del turno
// ============================================

=== intro ===

# amb:dispatch_night

Ore 23. Martedì di un freddo marzo.

I miei colleghi erano già andati.

Il cestino all'angolo era colmo. Palle di carta roteavano attorno, statiche.

Il ronzio dello schermo CRT di fronte a me...

La puzza di chiuso. Sigaretta impregnata nella moquette sintetica.

Sospiro.

Rimetto a posto la scrivania...

+ [Sistemare la scrivania]
    -> tidy_desk
+ [Lasciare tutto com'è]
    -> messy_desk

// ============================================
// KNOT A — Sistemare la scrivania
// ============================================

=== tidy_desk ===

# sfx:drawer_open

Apro il cassetto.

La bottiglia di Jack Daniel's. Vuota.

Quella notte.

-> reflection

// ============================================
// KNOT B — Lasciare la scrivania disordinata
// ============================================

=== messy_desk ===

Lascio tutto com'è.

Accendo una sigaretta.

-> reflection

// ============================================
// RIFLESSIONE — Ward
// ============================================

=== reflection ===

Siamo piccoli davanti al creatore.

Semplicemente un numero da macello.

Dentro gli occhi di chi muore, si nasconde nell'infinito loop la vita percorsa.

Percossa dalla morte.

-> next

// ============================================
// NEXT — La chiamata
// ============================================

=== next ===

// TODO: Il telefono squilla

-> END













