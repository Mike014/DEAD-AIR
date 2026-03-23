// ============================================
// DEAD AIR — Demo Script
// ============================================
// Sound Design:
// - Ward: text only
// - Iris: voice (ElevenLabs Neural TTS)
// - Ambience: night dispatch center
// - SFX: diegetic (everything Ward hears)
// - No music
// ============================================
// Unity Integration:
// - #timed_choice triggers timer activation
// - #timeout:4 duration in seconds
// - #default:1 choice index if timer expires
// ============================================

VAR asked_name = false

-> part_1

// ============================================
// PART 1 — Setup
// ============================================

=== part_1 ===

# amb:dispatch_night

2 AM. Friday dragging itself into Saturday.

Jack Daniel's in a crystal glass. Neat, no ice. The caramel first, then the maple char — that burnt wood taste that cleans your throat and your head. Especially your head.

The dispatch center is almost empty tonight. Few operators, all lazy. Calls you can count on one hand.

"Ma'am, a cat in a tree is not an emergency. We'll notify the fire department."

A few robberies at the usual corner stores. Booze, food, nothing special. The patrols live off this stuff.

I wasn't sober.

The headset was bothering me. The screen light burned my eyes.

I hated that place.

But rent had to be paid.

# sfx:phone_ring

Line 3.

-> part_2

// ============================================
// PART 2 — The Call
// ============================================

=== part_2 ===

The LED blinks. One, two, three times.

-> wait_1

= wait_1
+ [WAIT]
    Maybe it'll resolve itself.
    The LED keeps blinking.
    -> wait_2
+ [ANSWER]
    -> answer

= wait_2
+ [WAIT]
    I wipe my lips. Another sip wouldn't hurt.
    The LED blinks. Insistent.
    -> wait_3
+ [ANSWER]
    -> answer

= wait_3
+ [WAIT]
    I sigh. No one else will answer for me. Not tonight.
    -> force_answer
+ [ANSWER]
    -> answer

= force_answer
+ [ANSWER]
    -> answer

= answer

# sfx:phone_pickup

I pick up the receiver. The weight of the headset on my skull.

911, what's the address of your emergency? # speaker:ward

-> part_3

// ============================================
// PART 3 — Iris
// ============================================

=== part_3 ===

Hi... I just wanted to know if the Bear is still angry. # speaker:iris

I blink. Another prank. Must be.

Sweetie, you've got the wrong number. This line isn't for playing. # speaker:ward

But the Bear is right outside the door. He told me if I talk he'll take my toys away. Are we playing the quiet game? # speaker:iris

The ice in the glass. A sip. Annoyance.

Listen, hand the phone to your mom or dad. Now. # speaker:ward

They can't. They're sleeping on the carpet. # speaker:iris

...

There's strawberry juice everywhere... the Bear spilled it all over. # speaker:iris

...

Can I have a blanket? # speaker:iris

-> part_4

// ============================================
// PART 4 — The Crooked Choice
// ============================================

=== part_4 ===

My hands are shaking. The glass is almost empty. I need to say something.

+ [Ask where she's calling from]
    -> crooked_response
+ [End the call]
    -> crooked_response

= crooked_response

Look... I don't have time for stories. If there's no adult, I have to end the call. We have procedures to follow. # speaker:ward

The words come out crooked. The alcohol speaks for me.

-> part_5

// ============================================
// PART 5 — The Turn
// ============================================

=== part_5 ===

But the Bear has my name on his list... I don't want to play anymore. # speaker:iris

Something breaks inside me. Not in the glass. Inside.

# sfx:glass_break

The glass falls. It shatters.

Shit... listen to me. This isn't a game. You need to hide. Now. Get under the bed. Don't breathe loud. # speaker:ward

He's right outside the door... he's scratching the wood. # speaker:iris

# sfx:scratch

...

I can see his shoes. They're black and shiny, like daddy's when we go to church. # speaker:iris

The realization hits me like a punch.

It's not a game. It was never a game.

The parents on the carpet. The strawberry juice. The Bear.

Close your eyes. Don't look at the shoes. Think of... think of a nice place. # speaker:ward

...

I need to know who she is. I need to—

-> the_real_choice

// ============================================
// THE REAL CHOICE
// ============================================
// Unity reads #timed_choice and activates countdown
// If it expires, forces ChooseChoiceIndex(1) -> [...]
// ============================================

=== the_real_choice ===

+ [What's your name?] # timed_choice # timeout:4 # default:1
    ~ asked_name = true
    -> part_6_name
+ [...]
    -> part_6_anonymous

// ============================================
// PART 6A — Iris has a name
// ============================================

=== part_6_name ===

What's your name? # speaker:ward

Iris. # speaker:iris

# sfx:door_open

The door opens. I hear it through the line.

The Bear is— # speaker:iris

-> ending

// ============================================
// PART 6B — Iris dies anonymous
// ============================================

=== part_6_anonymous ===

I open my mouth. Nothing comes out.

The moment passes.

# sfx:door_open

The door opens. I hear it through the line.

The Bear is— # speaker:iris

-> ending

// ============================================
// ENDING
// ============================================

=== ending ===

# sfx:scream
# amb:stop

...

Then nothing.

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
