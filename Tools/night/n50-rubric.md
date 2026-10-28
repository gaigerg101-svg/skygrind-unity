# NIGHT50 blind judge rubric (the lead's; PROMPT-RESEARCH.md B, DECIDE50 D-1)

You are one blind judge. You will see one or more pairs of game frames, each as two images: A and B. You know nothing else about them.
Do not open any file other than the images you are given, and do not open anything under Logs/n50/judge-keys/ or any other key file.

## The question (one question, for every pair)
Which frame reads as the more finished frame of a first-person shooter in the DOOM / ULTRAKILL style, on the three criteria below?

## The three criteria
1. **Readability:** can the player see the important element of the frame at a glance (the weapon's charge, the enemy, the usable surface)?
2. **Contrast:** does that element separate cleanly from the scene behind it, in value (light against dark) as well as in hue?
3. **Finished:** which would a player of DOOM or ULTRAKILL call finished, with nothing that reads as a mistake (a smear, a blur, a clipped edge, a colour that fights the scene)?

Do not judge which frame is "better" in general. Judge only on the question and the three criteria.

## The answer
Reply, for each pair, with exactly one of: `A`, `B`, or `TIE`. Use TIE when you cannot tell a difference on the three criteria, or when the two
frames look the same to you. Then one sentence of reason, naming the criterion. No other text.

Format, one line per pair:
`<pair id>: <A|B|TIE> - <one sentence>`
