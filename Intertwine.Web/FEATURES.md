# Profiles, questions and Sparks

## Implemented flows

- Edit avatar name, first name, middle name and last name through `PUT /api/UserProfile/me`.
- Deactivate a profile through `POST /api/UserProfile/me/deactivate`. Profile data is retained, and the next successful login reactivates it.
- Question cards, daily questions, dialogs and profile answers show category dots and names using the category colors returned by the API.
- `GET /api/user-answers/me` returns current selections with question text, answer text and categories. The dedicated My Answers section displays them without loading the question library or Daily Question.
- Data-bound sections use a shared loading state and do not render editable controls, balances, packages, payments, or answer cards before their initial requests settle.
- Two non-daily answers/updates are free per local date. Subsequent actions cost 10 Sparks each. The UI requires a checked spending consent box; the API requires `spendSparks: true` in the existing answer request. Same-answer resubmissions are rejected. The Daily Question remains once per date.
- The server stages the answer, activity, wallet debit and signed `CreditSpend` ledger entry in one EF SaveChanges transaction. Wallet row versions and activity/answer concurrency tokens reject overlapping writes. The migration only updates EF metadata; its SQL Up/Down operations are empty.
- Get Sparky (`#wallet`) loads currencies and packages from the API and adds a chosen package through the simulated top-up endpoint. No real payment is taken, and backend purchases persist.
- Spark balances link to `#payments`. `GET /api/wallet/payments?page=1` returns the authenticated user's purchase snapshots, newest first, 20 per page. This is purchase history, not a complete spending ledger.
- The UI uses Sparks and the supplied amber palette. Existing database/API names such as `CreditBalance` and `Credits` remain compatible with existing clients.

## Suggested personality feature (not implemented)

Start with optional self-reported personality badges and a visibility choice. Use a normalized `PersonalitySystem` table, `PersonalityType` table, and a `UserPersonality` association (profile, type, source, updated date, visibility), unique per user and system. MBTI selections can use the 16 four-letter codes; Enneagram can use types 1–9, with an optional wing later. Include “Not sure” by allowing no selection.

Use the types to suggest conversation starters about communication, decisions, and handling disagreement. Let users choose whether to share their type. Keep matching based primarily on their actual answers and preferences; do not infer a type from ordinary daily answers or present type combinations as a guaranteed compatibility score.

For a later questionnaire, design a separate, versioned assessment and consent flow with its own responses and scoring. Use original questions, or obtain appropriate permission for an official instrument; do not copy a commercial assessment into daily questions.

Background reading: [Myers & Briggs Foundation: relationships](https://www.myersbriggs.org/type-in-my-life/personality-type-and-relationships/) and [Enneagram Institute: type combinations](https://www.enneagraminstitute.com/the-enneagram-type-combinations/). These discuss communication and relationship patterns; the product approach above is a design recommendation.
