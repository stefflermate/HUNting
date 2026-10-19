# HUNting – Repozitórium-audit (tervezési dokumentum)

> Készült: 2026-10-17. Forrás: a repó teljes, olvasás alapú átvizsgálása (backend + frontend + tesztek).
> Megjegyzés: a mintadolgozat fájlját a szerző kérésére nem dolgoztuk fel; a dolgozat szerkezete a feladatkiírásban rögzített fejezetvázat követi.
> Az itt szereplő minden állítás konkrét forrásfájlra hivatkozik. A dolgozatban csak az itt igazolt tények szerepelhetnek.

## 1. Használt technológiák (igazolt)

| Réteg | Technológia | Verzió | Igazoló fájl |
|---|---|---|---|
| Frontend | React | ^18.2.0 | `Frontend/HUNting/package.json` |
| Frontend | TypeScript | ^5.2.2 (strict) | `package.json`, `tsconfig.app.json` |
| Frontend | Vite | ^6.3.6 | `vite.config.ts` |
| Frontend | React Router DOM | ^6.29.0 | `src/routes/appRouter.tsx` |
| Frontend | TanStack React Query | ^5.62.3 | `src/lib/queryClient.ts` |
| Frontend | Axios | ^1.7.9 | `src/lib/apiClient.ts` |
| Frontend | Tailwind CSS | ^4.1.14 | `src/index.css`, `@tailwindcss/vite` |
| Frontend | @react-google-maps/api | ^2.20.1 | `src/pages/Map/MapPage.tsx` |
| Frontend | react-markdown | ^10.1.0 | `src/components/Chatbot.tsx` |
| Frontend | lucide-react, CVA, Radix Slot | — | `src/components/ui/button.tsx` |
| Backend | ASP.NET Core Web API | net8.0 | `Backend/HUNting.API/HUNting.API.csproj` |
| Backend | Entity Framework Core (SQL Server) | 8.0.6 | csproj, `Data/HuntingContext.cs` |
| Backend | ASP.NET Core Identity (int kulcsokkal) | 8.x | `Program.cs`, `Models/Users/ApplicationUser.cs` |
| Backend | JWT Bearer autentikáció | 8.x | `Services/Auth/JwtTokenService.cs`, `Configuration/JwtOptions.cs` |
| Backend | Swagger / Swashbuckle (csak Development) | — | `Program.cs` |
| Adatbázis | Microsoft SQL Server (LocalDB, „VadászNet” DB) | — | `appsettings.json` ConnectionStrings:DefaultConnection |
| Külső API | OpenAI Chat Completions (gpt-4o) | — | `Services/OpenAiService.cs` |
| Külső API | OpenWeather (közvetlen frontend hívás) | — | `src/services/weatherService.ts` |
| Külső API | Google Maps JavaScript API | — | `MapPage.tsx`, `VITE_GOOGLE_MAPS_API_KEY` |
| Teszt (BE) | xUnit 2.9.2, Moq 4.20.72, MockQueryable.Moq, EF InMemory, coverlet | net9.0 | `HUNting.API.Tests.csproj` |
| Teszt (FE) | Vitest ^4.0.18 + Testing Library + happy-dom | — | `vitest.config.ts` |
| Teszt (E2E) | Playwright ^1.49.0 (Chromium) | — | `playwright.config.ts`, `e2e/` |

**Nem található a repóban:** AutoMapper, FluentValidation, SignalR/WebSocket, react-hook-form, zod, i18n könyvtár, Leaflet, deployment/CI konfiguráció (Dockerfile, pipeline), központi exception middleware / ProblemDetails.

## 2. Tényleges architektúra

Háromrétegű (megjelenítés – üzleti logika – adattárolás), **rétegzett monolit** backenddel:

- Controller → Service → EF Core `HuntingContext` lánc, de **nem** Clean/Onion Architecture (nincs külön Domain/Application/Infrastructure projekt; egyetlen API projekt van).
- Repository minta **részlegesen**: `IGenericRepository<T>`/`GenericRepository<T>` (`Services/GenericRepository.cs`) regisztrálva, de csak a `SocietyService` használja a Society CRUD-hoz; a lekérdezések többsége közvetlen `HuntingContext`.
- Service réteg controllerenként vegyes:
  - Service-t használ: `AdsController`→`IAdService`, `MapController`→`IMapService`, `ChatController`→`IOpenAiService`, `SocietiesController`→`ISocietyService` (+ közvetlen context), `UsersController`→`IUserService` (+ `UserManager`).
  - Közvetlen `HuntingContext`/Identity-managerek: `AuthController`, `HuntingLogsController`, `NewsController`.
- DTO-leképezés kézi (statikus `From...` metódusok, pl. `MapMarkerResponse.From`); több végpont EF-entitást ad vissza közvetlenül (Ads, News, HuntingLogs, Societies).
- Aszinkron: a DB- és HTTP-műveletek túlnyomóan async/await; `IMapService` CancellationToken-t is fogad.
- Hibakezelés: akciónkénti try/catch + 400/403/500 (nincs globális middleware). Validáció: `[ApiController]` + DataAnnotations + kézi ellenőrzések (kép-kiterjesztés, tulajdonos, tagság, üres üzenet).

## 3. Projektstruktúra

```
Backend/HUNting.API/          – Web API (Controllers, Services, Models, Data, Migrations, Configuration, wwwroot/uploads)
Backend/HUNting.API.Tests/    – xUnit tesztek (Controllers/, Services/)
Frontend/HUNting/             – React+Vite SPA (src/pages, src/components, src/services, src/lib, src/context, src/routes, e2e/)
script.sql, script-adatbazis.sql – SQL szkriptek a repó gyökerében
```

## 4. Backend-végpontok (teljes lista)

### AdsController (`api/Ads`) — `Controllers/AdsController.cs`
| Végpont | Auth | Leírás |
|---|---|---|
| GET `api/Ads` | Anonim | Lapozott, szűrhető hirdetéslista (`AdQueryOptions`: Page, Take, Search, CategoryId, StatusCode, MinPrice, MaxPrice, OwnerUserId) → `PagedResult<Ad>` |
| GET `api/Ads/categories` | Anonim | Kategórialista |
| GET `api/Ads/statuses` | Anonim | Státuszlista |
| GET `api/Ads/{id}` | Anonim | Egy hirdetés (404, ha nincs) |
| POST `api/Ads` | Authorize | Létrehozás; nem admin → saját tulajdon; admin tulajdonost adhat meg |
| PUT `api/Ads/{id}` | Authorize | Módosítás; tulajdonos vagy Administrator; tulajdonosmezőt csak admin írhat |
| DELETE `api/Ads/{id}` | Authorize | Törlés; tulajdonos vagy Administrator |
| POST `api/Ads/{id}/images` | Authorize | Képfeltöltés (JPG/JPEG/PNG/WEBP, multipart) → `wwwroot/uploads/ads` |
| DELETE `api/Ads/{id}/images/{imageId}` | Authorize | Kép + helyi fájl törlése |

### AuthController (`api/Auth`) — `Controllers/AuthController.cs`
| Végpont | Auth | Leírás |
|---|---|---|
| POST `api/Auth/register` | Anonim | Person vagy Society fiók; society esetén Society entitás + (ha van „Vezeto” szerep) vezetői tagság is létrejön |
| POST `api/Auth/login` | Anonim | Hitelesítés → JWT + felhasználói adatok (`AuthResponse`) |
| POST `api/Auth/logout` | Authorize | 204; a JWT-t **nem** érvényteleníti (stateless) |
| GET `api/Auth/me` | Authorize | Aktuális felhasználó + szerepek + birtokolt society (`UserDto`) |

### ChatController (`api/Chat`) — `Controllers/ChatController.cs`
| POST `api/Chat` | **nincs attribútum** | `ChatRequest` (Message + History) → OpenAI → `{ reply }`; üres üzenet 400; hiba 500 |

### HuntingLogsController (`api/HuntingLogs`) — mind `[Authorize]`
| GET `api/HuntingLogs` (`?scope=all` adminnak) | saját naplók, admin mindet | 
| GET/PUT/DELETE `api/HuntingLogs/{id}` | tulajdonos vagy admin |
| POST `api/HuntingLogs` | multipart (`HuntingLogCreateRequest` + opcionális kép, 10 MiB limit); society fiók elutasítva |

### MapController (`api/Map`) — `Controllers/MapController.cs`
| Végpont | Auth | Leírás |
|---|---|---|
| GET `api/Map/config` | nincs attribútum | ikontípusok + láthatóságok |
| GET `api/Map/markers` / `polygons` | nincs attribútum, kézi scope-ellenőrzés | `scope=personal` (bejelentkezett) vagy `scope=society&societyId=` (tagság-ellenőrzéssel); egyébként üres lista |
| POST/PUT/DELETE markers, polygons | Authorize | CRUD; tulajdonjog-ellenőrzés a service-ben (403) ; polygon pontok rendezve (Seq) |

### NewsController (`api/News`) — **egyik végponton sincs autorizáció!**
| GET, GET/{id}, POST, PUT/{id}, DELETE/{id} | nincs | Teljes CRUD anonim módon elérhető; láthatóság (VisibilityCode) szerinti szűrés nincs |

### SocietiesController (`api/Societies`)
| Végpont | Auth | Leírás |
|---|---|---|
| GET `api/Societies`, GET `/{id}` | Anonim | Lista / részletek |
| GET `api/Societies/my` | Authorize | Saját tagságok |
| GET `/{id}/members` | Authorize | Tagok (tulajdonos vagy admin) |
| POST `/{id}/join` | Authorize | Tagsági kérelem (`MembershipRequestDto`); society fiók nem jelentkezhet; elutasított kérelem újranyitható |
| GET `/{id}/requests` | Authorize | Függő kérelmek (tulajdonos/admin) |
| POST `/{id}/requests/{userId}/approve` / `reject` | Authorize | Elfogadás („Tag” szereppel felvétel) / elutasítás |
| GET `/{id}/posts` | Authorize | Bejegyzések (tag, tulajdonos vagy admin) |
| POST `/{id}/posts` | Authorize, 10 MiB | Bejegyzés + opcionális kép (tulajdonos/admin) |
| DELETE `/{id}/posts/{postId}` | Authorize | Bejegyzés + helyi kép törlése |

### UsersController (`api/Users`) — controller-szintű `[Authorize(Roles="Administrator")]`
GET lista, GET/{id}, POST (szerepkiosztással), PUT/{id} (szerepek cseréje), DELETE/{id} → `UserResponse`.

## 5. Adatmodellek (entitások)

Identity-alap: `HuntingContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>`; a táblák átnevezve (`Users`, `Roles`, `UserRoles`, `UserClaims`, `UserLogins`, `RoleClaims`, `UserTokens`) — `Data/HuntingContext.cs` + `20251207135051_RenameIdentityTables`.

| Entitás | Kulcs | Fontos mezők / kapcsolatok |
|---|---|---|
| `ApplicationUser` (Users) | int Id | FullName, Phone, CreatedAt, `AccountType` enum (Person=0, Society=1); nav: OwnedSociety (1–1), Societies, MembershipRequests, HuntingLogs; unique Email |
| `Society` | SocietyId | Name, Email (unique), PasswordHash, Phone, Description, OwnerUserId (SetNull a tulajdonos törlésekor); nav: Members, MembershipRequests, Posts, News |
| `SocietyRole` | RoleId | Name (unique): seed: Vezeto, Gazda, Vador, Tag |
| `SocietyMember` | **összetett (SocietyId, UserId)** | RoleId (Restrict), JoinedAt (sysdatetime default); cascade society-vel és userrel |
| `SocietyMembershipRequest` | **összetett (SocietyId, UserId)** | Message, `MembershipRequestStatus` enum (Pending/Approved/Rejected), CreatedAt, ProcessedAt, ProcessedByUserId (SetNull) |
| `SocietyPost` (tábla: PostId oszlop) | SocietyPostId | SocietyId, AuthorUserId, Title, Content, ImageUrl, CreatedAt |
| `Ad` | AdId | Title, Description, CategoryId, Price (decimal?), LocationText, Lat/Lng, StatusCode, Contact mezők, OwnerUserId / OwnerSocietyId + **CK_Ads_SingleOwner check constraint** (pontosan egy tulajdonos) |
| `AdCategory` | CategoryId | Name (unique); 12 seedelt kategória |
| `AdStatus` | **string StatusCode** | ACTIVE, PENDING, SOLD, HIDDEN (seed) |
| `AdImage` | ImageId | AdId (cascade), ImageUrl, SortOrder |
| `HuntingLog` (oszlop: LogId) | HuntingLogId | UserId (cascade), OccurredAt, LocationText, Lat/Lng, AnimalType, ShotFired, Notes, ImageUrl, ShareWithSociety, CreatedAt |
| `NewsFeed` (oszlop: PostId) | NewsFeedId | SocietyId, AuthorUserId (cascade), Title, Content, ImageUrl, VisibilityCode (Restrict) |
| `NewsVisibility` | string VisibilityCode | MEMBERS, PUBLIC, OFFICERS (seed) |
| `MapIconType` | IconTypeId | Name, DefaultColor, Glyph; seed: Megfigyelopont, Eteto, Veszely, Talalkozo |
| `MapVisibility` | string VisibilityCode | PUBLIC, MEMBERS, ADMIN (seed) |
| `MapMarker` | MarkerId | CreatedByUserId (Restrict), SocietyId? (Cascade), IconTypeId (Restrict), Lat/Lng (decimal), Note, VisibilityCode |
| `MapPolygon` | PolygonId | Name, Description, VisibilityCode, StrokeColor, FillColor + `MapPolygonPoint` (PolygonId+Seq unique, cascade) |
| `MapPath` + `MapPathPoint` | PathId / PathPointId | **csak adatmodell + migráció; nincs hozzá végpont és UI** |

DTO-k: `PagedResult<T>`; controller-beli DTO-k (RegisterRequest, AuthResponse, UserDto, MapMarkerRequest/Response, MembershipRequestDto stb.); `Models/Chat/ChatRequest.cs` (nem perzisztált). **`Models/VisibilityLevel.cs` üres fájl.**

Migrációk (5): InitialIdentity (2025-10-20), AddMapEntities (2025-11-05), SocietyAccountsAndPosts (2025-11-10), RenameIdentityTables (2025-12-07), AddDescriptionToMapPolygon (2026-03-12).

Seed (`Data/SeedData.cs`, induláskor `MigrateAsync` + idempotens feltöltés): 3 app-szerep (Administrator, SocietyManager, Hunter), 11 felhasználó (1 admin + 10 vadász), 8 vadásztársaság, 4 society-szerep, ~23 tagság, 12 hirdetéskategória, 4 státusz, 7 hirdetés (külső Unsplash képekkel), 3 news-láthatóság, 2 hír, 3 térkép-láthatóság, 4 ikontípus, 4 vadásznapló. Térképobjektum nincs seedelve.

## 6. Frontend-oldalak és útvonalak

Router: `src/routes/appRouter.tsx` (RouterProvider az `App.tsx`-ben; QueryClientProvider + AuthProvider + Suspense). **Nincs router-szintű ProtectedRoute** — a védelem oldalszintű redirect/feltételes UI.

| Útvonal | Oldal | Hozzáférés |
|---|---|---|
| `/` | HomePage (hero, számlálók, FeatureGrid, LatestAds) | publikus |
| `/ads` | AdsListPage (keresés, kategória, árszűrő, lapozás) | publikus |
| `/ads/new` | AdCreatePage (AdForm, képfeltöltés) | bejelentkezés kell (redirect) |
| `/ads/:adId` | AdDetailPage (képek, kapcsolat, tulaj/admin műveletek) | publikus |
| `/ads/:adId/edit` | AdEditPage | tulajdonos/admin |
| `/societies` | SocietiesPage (névszűrés) | publikus |
| `/societies/mine` | MySocietiesPage (tagságok + posztok) | Person fiók |
| `/societies/manage` | SocietyManagementPage (kérelmek, tagok, posztok) | Society fiók + ownedSociety |
| `/societies/:societyId` | SocietyDetailPage (+ tagsági kérelem űrlap) | publikus / Person |
| `/map` | MapPage (Google Maps, marker/polygon CRUD, rétegek) | nézet publikus, szerkesztés auth |
| `/logs` | HuntingLogsPage (napló + képfeltöltés) | Person fiók |
| `/weather` | WeatherPage (OpenWeather, városkeresés) | publikus |
| `/profile` | ProfilePage | auth (redirect) |
| `/admin` | AdminDashboardPage (hirdetés- és felhasználókezelés) | Administrator (redirect `/`-re) |
| `/auth/login`, `/auth/register` | LoginPage, RegisterPage (AuthLayout) | vendég |
| `*` | NotFoundPage | — |

Állapotkezelés: `AuthContext` (localStorage: `hunting.accessToken`, `hunting.user`; mountkor `/auth/me` ha token van user nélkül), React Query (staleTime 1 perc, nincs retry/fókusz-refetch), minden más lokális state. Axios interceptorok: Bearer-token hozzáfűzés; 401 → storage törlés + hard redirect `/auth/login`.

## 7. Tesztek

- **Backend:** 124 `[Fact]` 9 fájlban (AdsControllerTests 23, SocietiesControllerTests 25, MapControllerTests 24, HuntingLogsControllerTests 16, AuthControllerTests 11, NewsControllerTests 8, UsersControllerTests 8, ChatControllerTests 5, OpenAiServiceTests 4). Technika: Moq + MockQueryable (service-mockolás), EF InMemory (Auth/HuntingLogs/Societies context-tesztek), Moq.Protected (HttpMessageHandler az OpenAI-hoz).
- **Frontend unit/komponens:** 18 Vitest-fájl (App, Chatbot, AppFooter, button, AdsList, AdDetail, Login, Register, HuntingLogs, Map, NotFound, Profile, MySocieties, SocietyDetail, SocietiesPage, SocietyManagement, Weather).
- **E2E:** 7 Playwright spec (smoke, auth, ads, hunting-logs, societies, admin, chatbot; összesen 23 teszt) + `helpers.ts`; a config elindítja a backendet (`dotnet run`) és a Vite dev szervert.

## 8. Külső integrációk

| Integráció | Hol | Mód | Konfig |
|---|---|---|---|
| OpenAI Chat Completions | backend `OpenAiService` (typed HttpClient) | POST api.openai.com/v1/chat/completions, modell: gpt-4o, magyar vadászati rendszerprompt, history-szanitálás (csak user/assistant szerep) | `OpenAI:ApiKey` |
| Google Maps JS API | frontend `MapPage` | `useLoadScript` + DrawingManager | `VITE_GOOGLE_MAPS_API_KEY` |
| OpenWeather | frontend `weatherService.ts` | **közvetlen** GET api.openweathermap.org (nem megy át a backenden) | `VITE_OPEN_WEATHER_API` |

## 9. Biztonsági mechanizmusok

- JWT (HMAC-SHA256, 60 perc, claimek: nameidentifier, name, email, role, account_type); issuer/audience/kulcs validálva, 1 perc clock skew (`JwtTokenService`, `Program.cs`).
- Identity jelszó-szabályok lazítva (min. 6 karakter, egyéb követelmények kikapcsolva); unique e-mail.
- Szerepkörök: Administrator / SocietyManager / Hunter; `UsersController` admin-only; erőforrás-szintű tulajdonos-ellenőrzések (ads, logs, map, posts).
- CORS: csak `http(s)://localhost:5173`, credentials engedve. Swagger csak Development. HTTPS-redirect a CORS előtt fut.
- Fájlfeltöltés: kiterjesztés-whitelist (jpg/jpeg/png/webp), GUID fájlnév, 10 MiB kéréslimit; statikus kiszolgálás `UseStaticFiles` (nincs auth a fájlokra).

## 10. Felismert hiányosságok (a dolgozatban korlátként / továbbfejlesztésként kezelendő)

1. **NewsController teljesen autorizálatlan** (anonim POST/PUT/DELETE) és nincs láthatóság-szűrés — backend-only funkció, frontend oldala nincs.
2. **Chat végpont anonim** — fizetős OpenAI-proxy rate limit nélkül.
3. **Map paths**: entitás + migráció van, végpont és UI nincs (előkészített funkció).
4. Map olvasó lekérdezések a `VisibilityCode`-ot nem szűrik (csak scope/tagság alapján szűrnek).
5. `Models/VisibilityLevel.cs` üres fájl.
6. `Society.Email/PasswordHash` mezők léteznek, de a bejelentkezés a user-fiókon át történik (AccountType.Society) — kettős ábrázolás.
7. Logout nem érvényteleníti a JWT-t (nincs blacklist/refresh token).
8. Frontend `apiClient.ts` dev fallback port hibás (`https://localhost:5001/api`, a backend 7003/5137).
9. `LoginPage.tsx` mojibake-sztringek („BejelentkezĂ©s”…).
10. Nincs globális hibakezelő middleware / ProblemDetails; több végpont EF-entitást ad vissza DTO helyett.
11. Nincs CI/CD, Docker, éles üzemeltetés nyoma — a dolgozat csak helyi futtatást állíthat.
12. Admin jelszó (`adminadmin`) és JWT-kulcs placeholder a repóban — fejlesztési konfigurációként dokumentálandó.
13. SocietiesPage „Jelentkezés tagságra” gombjának nincs handlere (a működő űrlap a detail oldalon van).
14. Hirdetés `statuses` API-függvény definiált, de a vizsgált oldalak nem használják.

## 11. Tisztázandó kérdések az írás megkezdése előtt

1. A mintadolgozat kihagyása miatt a feladatkiírás fejezetváza a mérvadó — megerősítendő.
2. Van-e hivatalos BME LaTeX-sablon, amit használni kell, vagy általános `report`/`memoir` alapú saját sablon készüljön?
3. Címlapadatok: szerző neve, szak, konzulens, tanszék, beadás éve.
4. A News funkciót jelen állapotában (backend-only, auth nélkül) dokumentáljuk korlátokkal, vagy a szerző javítja írás előtt?
5. Képernyőképek: a futó alkalmazásról a szerzőnek kell elkészítenie (ezen a gépen nem futtatható); kell-e hozzá lista a pontos képernyőkről (ld. figure_plan.md)?
6. Google Maps / OpenWeather / OpenAI kulcsok rendelkezésre állnak-e a képernyőképekhez?
7. A dolgozat címe maradjon-e: „HUNting – Magyar Vadászati Közösségi és Hirdetési Portál fejlesztése”?
