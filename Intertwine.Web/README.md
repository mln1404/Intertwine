# Intertwine Web

A responsive Vue 3 + TypeScript frontend for the existing Intertwine ASP.NET Core API. Components use the Composition API with `<script setup>`, shared reactive state, and a typed Fetch API layer. No additional runtime dependencies are required.

## Run

Requires Node.js 22.12+ (Node.js 24 recommended).

```powershell
npm install
npm run dev
```

Open the URL printed by Vite. Signed-out visitors see the public Intertwine home and login/registration screen. Questions, answers, profile information, Spark balance, packages, and payment history are loaded only after authentication.

## Connect to the Intertwine API

```powershell
Copy-Item .env.example .env.local
```

Start the `Intertwine.API` project from the same solution with its SQL Server and Redis dependencies. The Vite development proxy forwards `/api` to `https://localhost:7279`, matching the API's default HTTPS launch profile; override `API_PROXY_TARGET` if needed. Keep `VITE_API_URL` empty when using this proxy. The proxy accepts the local ASP.NET Core development certificate without disabling HTTPS validation in the browser or API.

The home-page sign-in form connects to the real API. Successful sign-in loads the authenticated user's data. Access tokens are held in `sessionStorage` for the current browser tab and removed on sign-out or an expired-session response.

## Screens and behaviour

- **For you:** local-date Daily Question, selectable answers, submission feedback, a completed state, and session activity.
- **Explore questions:** search, category filters derived from the question list, and question details loaded when opened.
- **Get Sparky:** balance, API-provided currencies, available credit packages, and simulated one-click top-ups. Ambiguous failures prompt a balance check; purchases are never automatically retried.
- **Payment history:** authenticated purchase history from the wallet API.
- **My answers:** the user's answered questions, selected answers, and color-coded categories.
- **My profile:** view and edit the supported name fields, plus a confirmation modal for reversible profile deactivation.
- **Accounts:** registration, sign-in, sign-out, validation messages, and expired-session handling.
- Responsive desktop sidebar/mobile bottom navigation, labelled form inputs, native modal focus containment, keyboard navigation, loading/error/empty states, and reduced-motion support.

Vue Router uses history-mode paths (`/today`, `/questions`, `/wallet`, `/payments`, `/answers`, `/profile`) for browser back/forward navigation. Configure the production host to serve `index.html` for these frontend paths.

## API contracts and current backend limits

| Operation                   | Endpoint                                               |
| --------------------------- | ------------------------------------------------------ |
| Daily question (anonymous)  | `GET /api/questions/daily?localDate=YYYY-MM-DD`        |
| Question summaries          | `GET /api/questions`                                   |
| Question and answer options | `GET /api/questions/{id}`                              |
| Answer submission           | `POST /api/questions/{id}/answer?localDate=YYYY-MM-DD` |
| Authentication              | `POST /api/Auth/register`, `POST /api/Auth/login`      |
| Profile                     | `GET`, `PUT`, `POST /api/UserProfile/me`               |
| Deactivate profile          | `POST /api/UserProfile/me/deactivate`                  |
| Current answers             | `GET /api/user-answers/me`                             |
| Daily activity              | `GET /api/daily-activity/me?localDate=YYYY-MM-DD`      |
| Wallet                      | `GET /api/wallet`                                      |
| Packages                    | `GET /api/credit-packages?currencyCode=AUD`            |
| Currencies                  | `GET /api/currencies`                                  |
| Top-up                      | `POST /api/wallet/top-up`                              |
| Payment history             | `GET /api/wallet/payments?page=1`                      |

Answers send `{ answerId }` and an `Idempotency-Key` header. The key is reused for the same user/question/answer/date after network errors, server failures, and conflicts within the current page session. The browser supplies its **local calendar date**, including across UTC boundaries. Day changes refresh the daily question; a submission from the previous date is rejected locally and asks the user to reopen it.

The server remains authoritative for the one Daily Question action and two non-daily actions per date. Both new and changed non-daily answers consume an action. On sign-in and refresh, the UI restores current selections and the supplied local date's activity from the API. No invented history, streaks, matches, or cross-device usage totals are displayed.

Registration returns success without a token, so users sign in separately. The backend creates the Identity account, `UserProfile`, and zero-balance `UserWallet` during registration. Deactivation sets `UserProfile.IsActive` to false without deleting profile data or the Identity account; the next successful login reactivates it.

The existing wallet implementation records a completed `IntertwineDemo` payment; it does not charge a card. The wallet screen discloses this. A real checkout requires backend payment integration.

## Verify and build

```powershell
npm test
npm run build
npm run preview
```

The isolated contract tests cover UTC date boundaries, request shape, registration/login, question details, idempotency-key reuse, server limit errors, wallet/currency/package/profile requests, session expiry, and signed-out API isolation. They use the actual TypeScript modules with mocked HTTP responses; they do not connect to real accounts, databases, Redis, or payment providers.

Full live integration requires the backend, SQL Server, Redis, and a provisioned account. Empty API results remain empty and display explicit messages such as “No Daily Question” and “No Credit Packages found.”

Production output is in `dist/`. The development proxy is not included in the production bundle or the preview server. Serve `/api` through your production reverse proxy, or set `VITE_API_URL` at build time to an HTTPS API origin with an appropriate CORS policy. Typography uses Google Fonts with local sans-serif fallbacks; all artwork and icons are local SVG.

## Source layout

```text
src/
  api/              Typed HTTP client and question endpoints
  components/       Shared icons, dialogs, loading state, authentication, answer form
  composables/      Shared session, questions, profile, and wallet state
  models/           Types aligned with backend DTOs
  utils/            Local calendar date and answer-request construction
  views/            Questions, answers, wallet, payments, and profile screens
  App.vue           Navigation and application shell
  style.css         Responsive visual design
tests/              Isolated API and state contract tests
```
