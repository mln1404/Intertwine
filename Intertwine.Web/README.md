# Intertwine Web

A responsive Vue 3 + TypeScript frontend for the existing Intertwine ASP.NET Core API. Components use the Composition API with `<script setup>`, shared reactive state, and a typed Fetch API layer. No additional runtime dependencies are required.

## Run

Requires Node.js 22.12+ (Node.js 24 recommended).

```powershell
npm install
npm run dev
```

Open the URL printed by Vite. The default is a **clearly labelled demo preview** with sample questions, an editable sample profile, and simulated credit top-ups. Demo answers, profile changes, and credits are held in memory and reset on reload. Demo actions do not contact the API.

## Connect to the Intertwine API

```powershell
Copy-Item .env.example .env.local
```

Set `VITE_DEMO_MODE=false` in `.env.local`, then restart Vite. Start the `Intertwine.API` project from the same solution with its SQL Server and Redis dependencies. The Vite development proxy forwards `/api` to `http://localhost:5288`; override `API_PROXY_TARGET` if needed. Keep `VITE_API_URL` empty when using this proxy. If ASP.NET redirects HTTP to HTTPS, use the HTTP launch profile or configure a trusted HTTPS target.

The sign-in dialog always connects to the real API, including from demo mode. Successful sign-in clears sample state and loads the authenticated user's data. Access tokens are held in `sessionStorage` for the current browser tab and removed on sign-out or an expired-session response. Nothing reads the old starter's `localStorage.accessToken` entry.

## Screens and behaviour

- **For you:** local-date Daily Question, selectable answers, submission feedback, a completed state, and session activity.
- **Explore questions:** search, category filters derived from the question list, and question details loaded when opened.
- **My wallet:** balance, currency selection, available credit packages, and a confirmation before a top-up. Ambiguous failures prompt a balance check; purchases are never automatically retried.
- **My profile:** view and edit the supported name fields, with an explicit typed confirmation before deleting the profile.
- **Accounts:** registration, sign-in, sign-out, validation messages, and expired-session handling.
- Responsive desktop sidebar/mobile bottom navigation, labelled form inputs, native modal focus containment, keyboard navigation, loading/error/empty states, and reduced-motion support.

Hash navigation (`#today`, `#questions`, `#wallet`, `#profile`) supports browser back/forward and static hosting without a route fallback.

## API contracts and current backend limits

| Operation                   | Endpoint                                               |
| --------------------------- | ------------------------------------------------------ |
| Daily question (anonymous)  | `GET /api/questions/daily?localDate=YYYY-MM-DD`        |
| Question summaries          | `GET /api/questions`                                   |
| Question and answer options | `GET /api/questions/{id}`                              |
| Answer submission           | `POST /api/questions/{id}/answer?localDate=YYYY-MM-DD` |
| Authentication              | `POST /api/Auth/register`, `POST /api/Auth/login`      |
| Profile                     | `GET`, `PUT`, `DELETE /api/UserProfile/me`             |
| Current answers             | `GET /api/user-answers/me`                             |
| Daily activity              | `GET /api/daily-activity/me?localDate=YYYY-MM-DD`      |
| Wallet                      | `GET /api/wallet`                                      |
| Packages                    | `GET /api/credit-packages?currencyCode=AUD`            |
| Top-up                      | `POST /api/wallet/top-up`                              |

Answers send `{ answerId }` and an `Idempotency-Key` header. The key is reused for the same user/question/answer/date after network errors, server failures, and conflicts within the current page session. The browser supplies its **local calendar date**, including across UTC boundaries. Day changes refresh the daily question; a submission from the previous date is rejected locally and asks the user to reopen it.

The server remains authoritative for the one Daily Question action and two non-daily actions per date. Both new and changed non-daily answers consume an action. On sign-in and refresh, the UI restores current selections and the supplied local date's activity from the API. No invented history, streaks, matches, or cross-device usage totals are displayed.

Registration returns success without a token, so users sign in separately. The backend creates the Identity account, `UserProfile`, and zero-balance `UserWallet` during registration. Deleting a profile does not delete its Identity account.

The existing wallet implementation records a completed `IntertwineDemo` payment; it does not charge a card. Both live and preview wallet screens disclose this. A real checkout requires backend payment integration.

## Verify and build

```powershell
npm test
npm run build
npm run preview
```

The nine isolated contract tests cover UTC date boundaries, request shape, registration/login, question details, idempotency-key reuse, server limit errors, wallet/profile requests, session expiry, and demo daily rules. They use the actual TypeScript modules with mocked HTTP responses; they do not connect to real accounts, databases, or payment providers. The test runner transpiles temporary modules using the existing TypeScript dependency and removes only its own temporary directory.

The UI was also checked in-browser for daily submission/locking, search, question dialogs, new and changed answers, the daily action limit, demo top-ups, profile editing, and a 390px mobile layout. Full live API integration requires the backend and a provisioned test account.

Production output is in `dist/`. The development proxy is not included in the production bundle or the preview server. Serve `/api` through your production reverse proxy, or set `VITE_API_URL` at build time to an HTTPS API origin with an appropriate CORS policy. Set `VITE_DEMO_MODE=false` for a live deployment. Typography uses Google Fonts with local sans-serif fallbacks; all artwork and icons are local SVG.

## Source layout

```text
src/
  api/              Typed HTTP client and question endpoints
  components/       Shared icons, dialogs, authentication, answer form
  composables/      Shared session, questions, profile, and wallet state
  data/             Explicit sample data for preview mode
  models/           Types aligned with backend DTOs
  utils/            Local calendar date and answer-request construction
  views/            Questions, wallet, and profile screens
  App.vue           Navigation and application shell
  style.css         Responsive visual design
tests/              Isolated API and state contract tests
```
