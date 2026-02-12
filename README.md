# FirebaseDotNetApi

ASP.NET Core 9.0 Web API that validates Firebase Authentication ID tokens with the Firebase Admin SDK and exposes a simple `WeatherController` for demoing protected vs. public endpoints. A lightweight vanilla JS frontend (in `frontend/`) lets you sign up/log in with Firebase Auth, grab an ID token, and exercise the secured API call from the browser.

## Prerequisites

- .NET SDK 9.0+
- A Firebase project with Email/Password authentication enabled
- Service account credentials with the **Firebase Admin SDK** role
- Any static file server for the frontend (e.g., VS Code Live Server, `npx serve`)

## Backend setup

1. Restore packages:
   ```bash
   dotnet restore
   ```
2. Download a service account JSON from **Firebase Console → Project Settings → Service Accounts → Generate new private key** and save it as `FirebaseDotNetApi/serviceAccountKey.json`. Keep this file out of source control.
3. Update the `firebaseProjectId` constant inside [FirebaseDotNetApi/Program.cs](FirebaseDotNetApi/Program.cs#L31) so it matches your project ID.
4. (Optional) Adjust the allowed CORS origin in [FirebaseDotNetApi/Program.cs](FirebaseDotNetApi/Program.cs#L11-L20) if your frontend runs somewhere other than `http://localhost:5500`.

## Run the API

```bash
cd FirebaseDotNetApi
dotnet run
```

The app listens on the default ASP.NET Kestrel ports (check the console output or [FirebaseDotNetApi/Properties/launchSettings.json](FirebaseDotNetApi/Properties/launchSettings.json)). HTTPS is required when calling from browsers unless you configure otherwise; trust the .NET dev certificate via `dotnet dev-certs https --trust` if you have not already.

### Endpoints

- `GET /weather/public` → no auth required.
- `GET /weather` → requires an `Authorization: Bearer <Firebase ID token>` header. The handler echoes the authenticated user ID/email from the JWT claims.

## Frontend setup

1. Open `frontend/index.html` in your editor and confirm the `firebaseConfig` matches the same Firebase project used by the backend.
2. Serve the folder with any static server so that CORS and module imports work:
   ```bash
   npx serve frontend --listen 5500
   # or use the VS Code Live Server extension on port 5500
   ```
3. Visit `http://localhost:5500` (or your chosen port), create an account, log in, and use the **Call /weather** button to hit the protected API once you have the backend running.

## Manual testing

1. Grab an ID token using the frontend or Firebase CLI.
2. Call the protected endpoint:
   ```bash
   curl -k https://localhost:7165/weather \
     -H "Authorization: Bearer <PASTE_ID_TOKEN>"
   ```
3. Expect a JSON response with your UID, email, and a server timestamp. If you see `401 Unauthorized`, verify the token came from the same Firebase project and that the API process can read `serviceAccountKey.json`.

## Notes

- The build currently emits a warning about `GoogleCredential.FromFile(...)` being deprecated. For production, consider switching to `CredentialFactory` as suggested in the warning message.
- Never commit `serviceAccountKey.json` or Firebase API keys that you do not intend to share publicly.
- If you need additional controllers, decorate protected actions with `[Authorize]` and public ones with `[AllowAnonymous]` similar to [FirebaseDotNetApi/Contorllers/WeatherController.cs](FirebaseDotNetApi/Contorllers/WeatherController.cs).
