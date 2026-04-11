// ============================================
// > grep "i love you"
// Dialogue Jam 2026 - DIALOGUE MODE
// ============================================
// 
// CHARACTERS:
// - ORFEO: Waiter, former actor, 32-35 years old
// - EURIDICE: Entrepreneur, 30-33 years old
//
// SETTING: Restaurant hallway (kitchen → bathroom)
// TIME: 2 years after breakup
//
// ============================================
// SOUND DESIGN (optional for Unity):
// - Ambience: restaurant background
// - SFX: door, footsteps, breathing
// - Music: none
// ============================================
// TAGS (optional - Unity audio system):
// #amb: — ambience
// #sfx: — sound effects  
// #voice: — voice clips
// ============================================

// ============================================
// SCENE 1: ENCOUNTER
// ============================================

EURIDICE: Orfeo.

ORFEO: ...

EURIDICE: I didn't know you worked here.

	+ [ORFEO: "Yeah..."] ORFEO: Yeah...
	
	+ [ORFEO: "..."] ORFEO: ...

- EURIDICE: How are you?

	+ [ORFEO: "Dressed as a waiter."] ORFEO: Dressed as a waiter.
	
	+ [ORFEO: "How do I look?"] ORFEO: How do I look?

- EURIDICE: I'm just trying to be nice.

	+ [ORFEO: "Oh, nice."] ORFEO: Oh, nice.
	
	+ [ORFEO: "Sorry."] ORFEO: Sorry.

- EURIDICE: Do you still act?

	+ [ORFEO: "Check the costume."] ORFEO: Check the costume.
		-> branch_orfeo_sarcastic
	
	+ [ORFEO: "Sometimes."] ORFEO: Sometimes.
		-> branch_orfeo_vulnerable

// ============================================
// SARCASTIC BRANCH - Orfeo defensive/cynical
// ============================================

=== branch_orfeo_sarcastic ===

EURIDICE: Orfeo, I...

EURIDICE: It's been a long time.

	+ [ORFEO: "Yes. Exactly two years."] ORFEO: Yes. Exactly two years.
	
	+ [ORFEO: "Long enough."] ORFEO: Long enough.

- EURIDICE: I had to move on.

	+ [ORFEO: "Without looking back."] ORFEO: Without looking back.
	
	+ [ORFEO: "And you moved on well."] ORFEO: And you moved on well.

- EURIDICE: We were broke, Orfeo.

EURIDICE: Both of us.

EURIDICE: I wanted a better life.

	+ [ORFEO: "At the expense of mine?"] ORFEO: At the expense of mine?
	
	+ [ORFEO: "Kind of selfish, don't you think?"] ORFEO: Kind of selfish, don't you think?

- // Player chooses whether Euridice opens up or defends

	+ [EURIDICE: "Yes."]
		-> branch_euridice_vulnerable
	
	+ [EURIDICE: "And you?"]
		-> branch_euridice_defensive

-> END

// ============================================
// VULNERABLE BRANCH - Orfeo opens up
// ============================================

=== branch_orfeo_vulnerable ===

EURIDICE: Really?

EURIDICE: I'm glad.

EURIDICE: That you still believe in your work.

	+ [ORFEO: "Believe."] ORFEO: Believe.
	
	+ [ORFEO: "It's all I have."] ORFEO: It's all I have.

- EURIDICE: Orfeo...let's not start...

	+ [ORFEO: "I was really bad."] ORFEO: I was really bad.
	
	+ [ORFEO: "I was a mess."] ORFEO: I was a mess.

- ORFEO: I barely ate or slept.

ORFEO: I stayed with my parents for a while.

EURIDICE: I'm sorry.

	+ [ORFEO: "I saw a therapist for a long time."] ORFEO: I saw a therapist for a long time.
	
	+ [ORFEO: "My therapist helped me a lot."] ORFEO: My therapist helped me a lot.

- EURIDICE: I didn't know.

EURIDICE: Orfeo, I...

EURIDICE: ...

EURIDICE: We were broke, Orfeo.

EURIDICE: Both of us.

EURIDICE: I wanted a better life.

	+ [ORFEO: "I felt inadequate."] ORFEO: I felt inadequate.
	
	+ [ORFEO: "I felt so alone."] ORFEO: I felt so alone.

- ORFEO: I was afraid to meet new people.

ORFEO: I was afraid to build a "new life".

// Player chooses Euridice's response

	+ [EURIDICE: "Me too."]
		EURIDICE: Me too.
		EURIDICE: I was afraid too.
		-> branch_euridice_vulnerable
	
	+ [EURIDICE: "I couldn't stay."]
		EURIDICE: I couldn't stay for you.
		EURIDICE: I had to think of myself.
		-> branch_euridice_defensive

-> END

=== branch_euridice_vulnerable ===

EURIDICE: Yes...

EURIDICE: I was selfish.

	+ [ORFEO: "What did you really want from me?"] ORFEO: What did you really want from me?
	
	+ [ORFEO: "..."] ORFEO: ...

- EURIDICE: I wanted not to be afraid.

EURIDICE: Afraid of not making it.

EURIDICE: Of being broke forever.

EURIDICE: I wanted you to quit theater.

EURIDICE: I don't know.

EURIDICE: I wanted everything.

	+ [ORFEO: "And in all of that, did you want me?"] ORFEO: And in all of that, did you want me?
	
	+ [ORFEO: "And in all of that, where was I?"] ORFEO: And in all of that, where was I?

- EURIDICE: I...

EURIDICE: I don't know if what I want is a good life...

EURIDICE: Or someone who loves me.

	+ [ORFEO: "You contradicted yourself."] ORFEO: You contradicted yourself.
	
	+ [ORFEO: "Maybe you don't even know what you really want?"] ORFEO: Maybe you don't even know what you really want?

- EURIDICE: ...

EURIDICE: I remember you were obsessed with this work.

EURIDICE: I remember you neglected me sometimes.

EURIDICE: I remember I was always second.

EURIDICE: I remember you didn't see me.

EURIDICE: I feel "seen" with my new partner.

	+ [ORFEO: "You just feel precious, admired with him."] ORFEO: You just feel precious, admired with him.
	
	+ [ORFEO: "I didn't make you feel important enough."] ORFEO: I didn't make you feel important enough.

- EURIDICE: I travel a lot with him.

EURIDICE: We go to amazing restaurants...

	+ [ORFEO: "Material things don't last."] ORFEO: Material things don't last.
	
	+ [ORFEO: "Basing your life only on that, what's the point?"] ORFEO: Basing your life only on that, what's the point?

- EURIDICE: It's not just that...

EURIDICE: It's that he...

EURIDICE: I don't know.

EURIDICE: I think about you.

EURIDICE: Always.

EURIDICE: I never stopped thinking about you.

EURIDICE: I still have your necklace.

EURIDICE: Whatever I do, I think of you.

EURIDICE: I hate you for that.

EURIDICE: I can't forget you.

	+ [ORFEO: "..."] ORFEO: ...
	
	+ [ORFEO: "..."] ORFEO: ...

- EURIDICE: I thought I could bury it.

EURIDICE: I thought it would be temporary...

	+ [ORFEO: "What do you mean?"] ORFEO: What do you mean?
	
	+ [ORFEO: "What are you talking about?"] ORFEO: What are you talking about?

- EURIDICE: The pain.

EURIDICE: I remember that even though I didn't like your lifestyle...

EURIDICE: I remember watching you drive.

EURIDICE: I remember admiring your stubbornness.

EURIDICE: You knew that job would leave you broke.

EURIDICE: But you loved it and kept believing.

	+ [ORFEO: "I loved you the same way."] ORFEO: I loved you the same way.
	
	+ [ORFEO: "It's the same way I loved you."] ORFEO: It's the same way I loved you.

- EURIDICE: I never told you.

EURIDICE: Maybe that's what we were missing.

EURIDICE: Maybe...we never really said it.

EURIDICE: Maybe properly.

EURIDICE: I love you.

ORFEO: I never told you either.

	+ [ORFEO: "I love you."] ORFEO: I love you.
	
	+ [ORFEO: "I love you."] ORFEO: I love you.

- EURIDICE: We never really said it.

-> END

=== branch_euridice_defensive ===

EURIDICE: And you?

EURIDICE: What did you do for us?

	+ [ORFEO: "I was poor, not incapable of loving."] ORFEO: I was poor, not incapable of loving.
	
	+ [ORFEO: "Not enough."] ORFEO: Not enough.

- EURIDICE: I felt alone.

EURIDICE: Every day with you.

EURIDICE: Invisible.

EURIDICE: You and your "dream".

	+ [ORFEO: "It's not a dream."] ORFEO: It's not a dream.
	
	+ [ORFEO: "It's my job."] ORFEO: It's my job.

- EURIDICE: You preferred your job over me.

	+ [ORFEO: "That's not true."] ORFEO: That's not true.
	
	+ [ORFEO: "..."] ORFEO: ...

- EURIDICE: I remember you were obsessed with this work.

EURIDICE: I remember you neglected me sometimes.

EURIDICE: I remember I was always second.

EURIDICE: I remember you didn't see me.

EURIDICE: I feel "seen" with my new partner.

	+ [ORFEO: "You just feel precious, admired with him."] ORFEO: You just feel precious, admired with him.
	
	+ [ORFEO: "I didn't make you feel important enough."] ORFEO: I didn't make you feel important enough.

- EURIDICE: I travel a lot with him.

EURIDICE: We go to amazing restaurants...

	+ [ORFEO: "Material things don't last."] ORFEO: Material things don't last.
	
	+ [ORFEO: "Basing your life only on that, what's the point?"] ORFEO: Basing your life only on that, what's the point?

- EURIDICE: It's not just that...

EURIDICE: I feel happy...

	+ [ORFEO: "You're lying."] ORFEO: You're lying.
	
	+ [ORFEO: "That's not what you really believe."] ORFEO: That's not what you really believe.

- EURIDICE: YOU THINK YOU KNOW WHAT I REALLY WANT?

EURIDICE: WHEN YOU DON'T EVEN KNOW WHAT YOU WANT?

EURIDICE: You work as a part-time waiter.

EURIDICE: You live in a room with roommates who don't love you.

EURIDICE: You're still dreaming at 35 of becoming a successful actor.

EURIDICE: You're just a child who hasn't grown up yet...

	+ [ORFEO: "You still wear my necklace."] ORFEO: You still wear my necklace.
	
	+ [ORFEO: "Do you still have my necklace?"] ORFEO: Do you still have my necklace?

- EURIDICE: ...

EURIDICE: I hate you.

EURIDICE: Because I can't forget you.

EURIDICE: Because I feel guilty.

EURIDICE: It's your fault.

EURIDICE: Because you never fix things, you always leave them second to your ego.

	+ [ORFEO: "..."] ORFEO: ...
	
	+ [ORFEO: "..."] ORFEO: ...

- EURIDICE: I hate you because I love you.

EURIDICE: Because you don't fit into my perfect life.

EURIDICE: I hate you because you can't help being in love with me even though I'm a bitch who made you suffer.

EURIDICE: I hate you, because you make me question what love really means...

EURIDICE: ...

EURIDICE: ...enough...

EURIDICE: I have to go...

	+ [ORFEO: "Wait, don't go..."] ORFEO: Wait, don't go...
		EURIDICE: No.
		EURIDICE: It's too late.
	
	+ [ORFEO: "...I love you..."] ORFEO: ...I love you...
		EURIDICE: ...
		EURIDICE: I know.
		EURIDICE: But it's not enough.
		
- -> END