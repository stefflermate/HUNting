# HUNting – Diplomamunka-írási terv (thesis_outline.md)

> Cél: ~71 oldal érdemi törzsszöveg, magyar nyelven, LaTeX-ben. A címlap, nyilatkozat, tartalomjegyzék, irodalomjegyzék és mellékletek ezen felül értendők.
> Szerkezeti alap: **funkcióközpontú felépítés** — a dolgozat gerince a funkciófejezetek sora (5–10. fejezet), mindegyik az adatbázistól a frontendig mutatja be az adott funkciót. A gerincbe nem férő témák a gerinc ELŐTT (1–4.: bevezetés, technológiák, architektúra, közös alapok) és UTÁNA (11–12.: tesztelés, összefoglalás) kaptak helyet. (A mintadolgozat-fájl a szerző kérésére nem került feldolgozásra.)
> Ábra-azonosítók: ld. `figure_plan.md`. Funkció-állapotok: ld. `feature_matrix.md`. Forrásigazolás: ld. `repository_audit.md`.

## Terjedelmi összesítő (ellenőrzött: Σ = 71 oldal)

| # | Fejezet | Jelleg | Oldal |
|---|---|---|---:|
| 1 | Bevezetés | keret (gerinc előtt) | 5 |
| 2 | Felhasznált technológiák | keret (gerinc előtt) | 7 |
| 3 | Rendszerarchitektúra | keret (gerinc előtt) | 4 |
| 4 | Közös rendszeralapok | keret (gerinc előtt) | 9 |
| 5 | Felhasználókezelés, jogosultságok és adminisztráció | **funkció (gerinc)** | 8 |
| 6 | Hirdetési piactér | **funkció (gerinc)** | 7 |
| 7 | Vadásztársaságok és tagságkezelés | **funkció (gerinc)** | 8 |
| 8 | Vadásznapló | **funkció (gerinc)** | 4 |
| 9 | Térképes funkciók | **funkció (gerinc)** | 7 |
| 10 | AI-chatbot és időjárás-integráció | **funkció (gerinc)** | 5 |
| 11 | Tesztelés és üzemeltetés | keret (gerinc után) | 5 |
| 12 | Összefoglalás és továbbfejlesztési lehetőségek | keret (gerinc után) | 2 |
| — | **Összesen** | | **71** |

(Irodalomjegyzék: +2–3 oldal, a 71-en felül. A funkció-gerinc 39 oldal ≈ a dolgozat 55%-a.)

---

## 1. Bevezetés (5 oldal) — keret, gerinc előtt

**Cél:** a probléma és a motiváció megalapozása, a célok kijelölése, a funkcióközpontú szerkezet ismertetése.

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 1.1 Motiváció és problémafelvetés | 1,25 | A magyar vadásztársadalom szervezési igényei: társasági kommunikáció, eszköz-adásvétel, területi nyilvántartás, naplóvezetés széttagoltsága | — |
| 1.2 A vadászati terület digitalizációs lehetőségei | 0,75 | Meglévő csatornák (fórumok, közösségi média, papíralapú napló) korlátai; egy integrált portál hozzáadott értéke | hivatkozások: OMVK/ jogszabályi kontextus általánosan, óvatos állításokkal |
| 1.3 Az alkalmazás céljai és célcsoportjai | 1 | Vadászok (Person) és vadásztársaságok (Society) mint két fióktípus; a 6 fő funkcióterület (= az 5–10. fejezetek) felsorolása | `Models/Users/AccountType.cs`; SC-01 képernyőkép (főoldal) |
| 1.4 Hasonló megoldások összehasonlítása | 1 | 2–3 létező megoldás (pl. nemzetközi vadásznapló-appok, hirdetési portálok) és a HUNting elhatárolása — csak ellenőrizhető, publikus információk alapján | webes források |
| 1.5 A dolgozat felépítése | 1 | A funkcióközpontú gerinc (5–10.) és a keretfejezetek logikájának bemutatása: minden funkció adatbázistól a frontendig | — |

**Nyitott kérdés:** a hasonló megoldások listáját a szerzővel egyeztetni kell (mely konkurenseket ismeri/mely relevánsak).

## 2. Felhasznált technológiák (7 oldal) — keret, gerinc előtt

**Cél:** minden ténylegesen használt technológia bemutatása + a választás indoklása; a funkcióspecifikus részletek a gerincfejezetekben.

| Alfejezet | Oldal | Tartalom | Forrás |
|---|---:|---|---|
| 2.1 A frontend technológiái | 3 | React (deklaratív UI, hookok), TypeScript (strict mód, közös típusok), Vite (dev szerver, HMR, `@` alias), TanStack Query (szerverállapot, cache; staleTime 1 perc, nincs retry), React Router, Axios, Tailwind 4 | `src/App.tsx`, `tsconfig.app.json`, `vite.config.ts`, `src/lib/queryClient.ts` |
| 2.2 A backend technológiái | 2,5 | ASP.NET Core 8 Web API (middleware-pipeline, DI, `[ApiController]`; miért .NET), C# nyelvi eszközök (async/await, nullable referencia-típusok), Entity Framework Core (ORM, Code First, LINQ, change tracking) | `Program.cs`, `Data/HuntingContext.cs` |
| 2.3 Microsoft SQL Server | 0,75 | relációs motor, LocalDB fejlesztéshez; a „VadászNet” adatbázis | `appsettings.json` |
| 2.4 Külső szolgáltatások | 0,75 | Google Maps JS API, OpenAI Chat Completions, OpenWeather — szerepük és integrációs pontjuk | `MapPage.tsx`, `OpenAiService.cs`, `weatherService.ts` |

**Ábrák:** T-01 technológiai áttekintő ábra (stack-diagram).
**Hivatkozások:** React docs, TypeScript Handbook, Microsoft Learn (ASP.NET Core, EF Core), SQL Server docs, TanStack docs, Vite docs, Google/OpenAI/OpenWeather hivatalos API-dokumentáció.

## 3. Rendszerarchitektúra (4 oldal) — keret, gerinc előtt

**Cél:** a háromrétegű felépítés és a rétegek közti kommunikáció mérnöki bemutatása; annak tisztázása, hogy rétegzett monolitról van szó (nem Clean Architecture).

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 3.1 A háromrétegű architektúra áttekintése | 1,5 | rétegek felelőssége (React SPA – Web API – MSSQL), a repó két fő projektje; rétegzett monolit — őszinte minősítés | A-01, U-01 |
| 3.2 REST API és adatfolyamok | 1,5 | erőforrás-alapú útvonalak, HTTP-igék, státuszkódok, JSON-szerializáció (string enumok, ciklusvédelem); A-02 kérés-válasz folyamatábra | `Program.cs` JSON-beállítások; A-02 |
| 3.3 Tervezési döntések és kompromisszumok | 1 | monolit vs. mikroszolgáltatás, kézi DTO-leképezés vs. AutoMapper, oldalszintű vs. router-szintű védelem; a funkcióközpontú dokumentálás indoklása | — |

**Ábrák:** A-01 háromrétegű architektúra, A-02 API kérés-válasz folyamat, U-01 Use Case diagram (ide vagy az 1. fejezetbe).

## 4. Közös rendszeralapok (9 oldal) — keret, gerinc előtt

**Cél:** a funkciófüggetlen, minden funkció által használt infrastruktúra EGYSZERI bemutatása, hogy az 5–10. funkciófejezetek ismétlés nélkül hivatkozhassanak rá.

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 4.1 Adattárolási alapok | 3 | Code First + `HuntingContext : IdentityDbContext<...,int>` (DbSet-ek, OnModelCreating); Identity-táblák átnevezése (AspNet* → Users, Roles…); teljes ER-áttekintés + törlésiviselkedés-táblázat (cascade/restrict/set null); az 5 migráció idővonala és workflow-ja (dotnet ef); `SeedData.SeedAsync` (MigrateAsync + idempotens feltöltés: 11 user, 8 társaság, 7 hirdetés…) | `Data/HuntingContext.cs`, `Migrations/`, `Data/SeedData.cs`; E-01 ER-diagram; L-04 |
| 4.2 Backend-alapok | 3 | `Program.cs`: DI-regisztrációk, middleware-sorrend, CORS, Swagger (L-01); controller–service–(részleges) repository minta és `IGenericRepository<T>` korlátai; JWT-infrastruktúra (`JwtOptions`, bearer-validáció — a kibocsátás az 5.3-ban); hibakezelés/validáció közös mintái (`[ApiController]`, DataAnnotations, try/catch); közös fájlfeltöltési minta (kiterjesztés-whitelist, GUID-név, `wwwroot/uploads/*`, 10 MiB); végpont-összefoglaló táblázat (8 controller, ~45 végpont) | `Program.cs`, `Services/GenericRepository.cs`, `Configuration/JwtOptions.cs`; A-03 komponensdiagram; SC-17 (Swagger) |
| 4.3 Frontend-alapok | 3 | provider-fa (`main.tsx` → QueryClient, Auth, Suspense, Router); routing-tábla + AppLayout/AuthLayout + lazy loading; `AuthContext` + `authStorage` (localStorage-kulcsok); React Query konvenciók (query-kulcsok, invalidáció); `apiClient` interceptorok (Bearer + 401-kezelés, `resolveAssetUrl`) (L-07); Tailwind 4 téma (erdőzöld/arany, Inter/Playfair), reszponzivitás, mobil menü | `App.tsx`, `appRouter.tsx`, `AuthContext.tsx`, `apiClient.ts`, `index.css`; FE-01 provider-fa; SC-01 mobil+desktop |

**Ábrák:** E-01 (teljes ER), A-03 (komponensdiagram), FE-01 (provider-fa), törlésiviselkedés- és végpont-táblázat.

---

# A dolgozat gerince: funkciófejezetek (5–10.)

Minden funkciófejezet **azonos belső sablont** követ, az adatbázistól a frontendig:

1. **X.1 A funkció célja és felhasználói működése** — milyen igényt elégít ki, hogyan használja a felhasználó (+ képernyőkép)
2. **X.2 Adatmodell** — érintett entitások, kapcsolatok, megszorítások
3. **X.3 Backend-megvalósítás** — végpontok, service-logika, jogosultságok
4. **X.4 Frontend-megvalósítás** — oldalak, állapotkezelés, API-hívások
5. **X.5 Adatfolyam, biztonság és korlátok** — a teljes út a felhasználói művelettől az adatbázisig és vissza (+ ábra, kódrészlet)

A tesztekre minden fejezet röviden utal; a tesztstratégia összefoglalása a 11. fejezetben található. A közös infrastruktúrára (4. fejezet) kereszthivatkozással hivatkozunk, nem ismételjük.

## 5. Felhasználókezelés, jogosultságok és adminisztráció (8 oldal) — funkció

**Cél:** az első gerincfejezet: regisztráció, bejelentkezés, profil és admin-funkciók teljes vertikuma; itt kerül bemutatásra a jogosultsági modell, amelyre a többi funkciófejezet hivatkozik.

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 5.1 Cél és felhasználói működés | 1 | kettős fióktípus (Person/Society), regisztráció → bejelentkezés → profil útvonal | SC-12, SC-13, SC-16 |
| 5.2 Adatmodell | 1,5 | `ApplicationUser` (IdentityUser<int> + FullName, Phone, AccountType), Identity-szereptáblák, `Society` 1–1 owner-kapcsolat (SetNull), unique e-mail | `Models/Users/*`, `HuntingContext.cs` |
| 5.3 Backend-megvalósítás | 2,5 | register (Person/Society ág: Society entitás + „Vezeto” tagság létrehozása), login + tokenkibocsátás (claimek, HMAC-SHA256, 60 perc) (L-02), me/logout; három app-szerep; erőforrás-tulajdonos minta (L-03); society-fiók korlátozásai | `AuthController.cs`, `JwtTokenService.cs`; B-01 szekvencia |
| 5.4 Frontend-megvalósítás | 1,5 | LoginPage/RegisterPage/ProfilePage, AuthContext-folyamatok (login/register/refreshProfile), 401-interceptor hatása, oldalszintű védelem mintája | `LoginPage.tsx`, `RegisterPage.tsx`, `ProfilePage.tsx`, `AuthContext.tsx`; B-02 folyamatábra |
| 5.5 Adminisztrációs felület | 1 | `UsersController` admin-CRUD szerepkezeléssel; AdminDashboardPage (hirdetés+felhasználó listázás, törlés megerősítéssel); HuntingLogs `scope=all` | `UsersController.cs`, `AdminDashboardPage.tsx`, `adminApi.ts`; SC-14 |
| 5.6 Adatfolyam, biztonság és korlátok | 0,5 | lazított jelszó-szabályok, logout nem érvényteleníti a JWT-t, admin-jelszó mint dev-konfiguráció | `Program.cs`, `SeedData.cs` |

## 6. Hirdetési piactér (7 oldal) — funkció

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 6.1 Cél és felhasználói működés | 0,75 | böngészés → szűrés → részletek → feladás → kezelés folyamata | SC-02…SC-04 |
| 6.2 Adatmodell | 1,25 | `Ad`, `AdCategory` (unique név), `AdStatus` (string kulcs), `AdImage` (SortOrder); CK_Ads_SingleOwner check constraint (pontosan egy tulajdonos) | `Models/Ads/*`, `HuntingContext.cs` |
| 6.3 Backend-megvalósítás | 2 | `AdsController` + `AdService`; lapozott-szűrt lekérdezés (`AdQueryOptions` + `PagedResult<T>`, Take-normalizálás) (L-05); képfeltöltés GUID-névvel az `uploads/ads` alá; tulajdonos/admin jogosultsági ágak (admin tulajdonost állíthat) | `AdsController.cs`, `AdService.cs` |
| 6.4 Frontend-megvalósítás | 2 | AdsListPage (szűrők, lapozás, szűrőfüggő query-kulcs), AdForm (drag&drop, paste, előnézet, validáció), AdDetail/AdEdit; React Query invalidációs minták (L-06) | `AdsListPage.tsx`, `AdForm.tsx`, `AdDetailPage.tsx`, `AdEditPage.tsx` |
| 6.5 Adatfolyam, biztonság és korlátok | 1 | hirdetésfeladás képfeltöltéssel lépésről lépésre (F-01 szekvencia): POST /ads → képenként POST /ads/{id}/images; fájlvalidálás, tulajdonjog-ellenőrzés | F-01 |

## 7. Vadásztársaságok és tagságkezelés (8 oldal) — funkció

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 7.1 Cél és felhasználói működés | 0,75 | katalógus → jelentkezés → jóváhagyás → tagság → posztok olvasása/írása | SC-05…SC-08 |
| 7.2 Adatmodell | 1,75 | `Society`, `SocietyRole`, `SocietyMember` (összetett kulcs!) (L-12), `SocietyMembershipRequest` (összetett kulcs + státusz-enum), `SocietyPost`; törlési szabályok | `Models/Societies/*`; E-02 osztálydiagram |
| 7.3 Backend-megvalósítás | 2,5 | `SocietiesController` + `SocietyService`; kérelem-életciklus (Pending→Approved/Rejected, elutasított újranyitása) (F-02 állapotdiagram); jóváhagyás „Tag” society-szereppel; tag-/kérelemlisták jogosultságai (tulajdonos/admin); posztok képpel (`uploads/societies`) | `SocietiesController.cs`, `SocietyService.cs` |
| 7.4 Frontend-megvalósítás | 2 | SocietiesPage (kliensoldali névszűrés), SocietyDetailPage (join űrlap Person-fióknak), SocietyManagementPage (kérelmek/tagok/posztok a Society-fióknak), MySocietiesPage (tagságok + posztok) | `SocietiesPage.tsx`, `SocietyDetailPage.tsx`, `SocietyManagementPage.tsx`, `MySocietiesPage.tsx` |
| 7.5 A hírek (NewsFeed) állapota és korlátok | 1 | backend-only CRUD autorizáció nélkül, nincs frontend-oldal — elhatárolás a SocietyPost-tól; korlátként, nem felhasználói funkcióként dokumentálva | `NewsController.cs`; feature_matrix 8. pont |

## 8. Vadásznapló (4 oldal) — funkció

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 8.1 Cél és felhasználói működés | 0,75 | személyes vadászati napló vezetése képekkel | SC-10 |
| 8.2 Adatmodell | 0,75 | `HuntingLog` (LogId oszlopnév, kaszkád törlés a userrel, opcionális koordináták) | `Models/Hunting/HuntingLog.cs` |
| 8.3 Backend-megvalósítás | 1,25 | `HuntingLogsController`: multipart létrehozás (10 MiB), `scope=all` adminnak, society-fiók tiltása, tulajdonos/admin ágak, képtörlés fájllal együtt | `HuntingLogsController.cs` |
| 8.4 Frontend-megvalósítás, adatfolyam és korlátok | 1,25 | HuntingLogsPage (lista, lenyitható részletek, űrlap képelőnézettel), cache-beszúrás mutáció után; korlátok: `shotFired`/`shareWithSociety` fix false a kliensből, nincs szerkesztő/törlő UI (a végpontok készen állnak) | `HuntingLogsPage.tsx`, `huntingLogsApi.ts` |

## 9. Térképes funkciók (7 oldal) — funkció

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 9.1 Cél és felhasználói működés | 0,75 | megfigyelőpontok, etetők, veszélyhelyek és területek rögzítése személyes/társasági rétegeken | SC-09 |
| 9.2 Adatmodell | 1,5 | `MapMarker`, `MapPolygon` + `MapPolygonPoint` (PolygonId+Seq unique index), `MapIconType`, `MapVisibility` (string kódok); `MapPath`/`MapPathPoint` mint ELŐKÉSZÍTETT (entitás+migráció van, végpont/UI nincs) | `Models/Map/*`, `HuntingContext.cs` |
| 9.3 Backend-megvalósítás | 2 | `MapController` + `MapService`: scope-alapú lekérdezés (personal/society) tagság-ellenőrzéssel, marker/polygon CRUD tulajdonjog-ellenőrzéssel, polygon-pontok cseréje Seq-sorrendben (L-09), config-végpont (ikontípusok+láthatóságok) | `MapController.cs`, `MapService.cs` |
| 9.4 Frontend-megvalósítás | 2 | Google Maps integráció (`useLoadScript`, MarkerF/PolygonF/InfoWindowF/DrawingManagerF), kattintásos marker-felvétel, polygon-rajzolás, rétegválasztó (`societiesApi.my`), koordináta- és min. 3 pont-validálás; M-01 folyamatábra | `MapPage.tsx`, `mapApi.ts`; M-01 |
| 9.5 Adatfolyam, biztonság és korlátok | 0,75 | a `VisibilityCode` jelenleg nem szűr a lekérdezésekben (címkeként működik); nincs szerkesztő UI (PUT végpontok készen); E2E-teszt hiánya (API-kulcs-függés) | — |

## 10. AI-chatbot és időjárás-integráció (5 oldal) — funkció

| Alfejezet | Oldal | Tartalom | Forrás / ábra |
|---|---:|---|---|
| 10.1 A chatbot célja és felhasználói működése | 0,75 | lebegő asszisztens-widget vadászati kérdésekhez, minden AppLayout-oldalon | SC-15 |
| 10.2 Chatbot backend | 1,5 | `OpenAiService` typed HttpClienttel: gpt-4o, magyar vadászati rendszerprompt, history szerep-szanitálása (csak user/assistant) (L-08), hibakezelés+logolás; `ChatController` (üres üzenet 400, hiba 500) | `OpenAiService.cs`, `ChatController.cs`; C-01 szekvencia |
| 10.3 Chatbot frontend | 1 | `Chatbot.tsx`: üzenetlista, react-markdown render, history komponens-state-ben (nem perzisztált), auth-oldalakon nem jelenik meg | `Chatbot.tsx` |
| 10.4 Időjárás-integráció | 1,25 | WeatherPage + weatherService: KÖZVETLEN OpenWeather-hívás (q, metric, lang=hu), találat-megjelenítés; frontend-only architektúra kompromisszuma | `weatherService.ts`, `WeatherPage.tsx`; SC-11 |
| 10.5 Biztonsági korlátok | 0,5 | anonim chat-végpont (költség, rate limit hiánya), kliensoldali OpenWeather-kulcs kitettsége | — |

**Megjegyzés:** e fejezet funkciói nem használnak saját adatbázis-entitást (a chatbot állapot nélküli, az időjárás kliensoldali) — ezt a dolgozat explicit kimondja, az egységes sablon X.2 pontja itt ennek indoklása.

## 11. Tesztelés és üzemeltetés (5 oldal) — keret, gerinc után

| Alfejezet | Oldal | Tartalom | Forrás |
|---|---:|---|---|
| 11.1 Backend egységtesztek | 1,5 | xUnit + Moq + MockQueryable + EF InMemory stratégia; 124 teszt eloszlása (Q-01 tesztpiramis); kiemelt példák: OpenAiService HttpMessageHandler-mock (L-10), HuntingLogs jogosultsági ágak | `HUNting.API.Tests/` |
| 11.2 Frontend komponenstesztek | 1 | Vitest + Testing Library + happy-dom; 18 tesztfájl; mockolási minták (API-modulok, AuthContext) | `src/**/*.test.tsx`, `vitest.config.ts` |
| 11.3 Végponttól végpontig tesztek | 1 | Playwright: 7 spec / 23 teszt, webServer-konfiguráció (backend+frontend automatikus indítás) (L-11), seedelt fiókok használata, friss felhasználó regisztrálása ismételhetőséghez | `playwright.config.ts`, `e2e/` |
| 11.4 Helyi futtatás és konfiguráció | 1 | portok (7003/5137, 5173), CORS, LocalDB kapcsolat, seed indításkor, Swagger; szükséges kulcsok (OpenAI, Google Maps, OpenWeather); dev-cert | `launchSettings.json`, `appsettings*.json` |
| 11.5 Ismert korlátok | 0,5 | éles üzemeltetés nincs; News-autorizáció hiánya; token-visszavonás hiánya — őszintén dokumentálva | repository_audit 10. pont |

**Fontos szabály:** teszteredmény-számokat (futási idő, pass/fail) csak akkor közlünk, ha a szerző ténylegesen lefuttatta és megadja azokat.

## 12. Összefoglalás és továbbfejlesztési lehetőségek (2 oldal) — keret, gerinc után

- 12.1 Eredmények (1): megvalósult célok funkciónként (az 5–10. fejezetek tükrében); mit igazol a tesztkészlet.
- 12.2 Továbbfejlesztés (1): News UI + autorizáció, Map paths befejezése, token-refresh/visszavonás, értesítések, üzenetküldés tagok közt, mobil app, CI/CD + konténerizálás, időjárás backend-proxy, i18n.

## Irodalomjegyzék (a 71 oldalon felül)

Valós, visszakereshető források: Microsoft Learn (ASP.NET Core, EF Core, Identity), react.dev, TypeScript Handbook, Vite docs, TanStack Query docs, React Router docs, axios docs, Tailwind docs, RFC 7519 (JWT), Fielding REST-disszertáció, OWASP Top 10, Fowler: Patterns of Enterprise Application Architecture (Repository), xUnit/Moq/Vitest/Playwright/Testing Library docs, Google Maps JS API docs, OpenAI API reference, OpenWeather API docs, SQL Server docs. BibTeX-ben, elérési dátummal.

---

## Javasolt írási sorrend

1. **4. fejezet** (közös alapok) → 2. **5. fejezet** (felhasználókezelés) → 3. **6. fejezet** (hirdetések) → 4. **7. fejezet** (társaságok) → 5. **8. fejezet** (vadásznapló) → 6. **9. fejezet** (térkép) → 7. **10. fejezet** (chatbot+időjárás) → 8. **3. fejezet** (architektúra — a részletek ismeretében) → 9. **11. fejezet** (tesztelés) → 10. **2. fejezet** (technológiák) → 11. **1. fejezet** (bevezetés) → 12. **12. fejezet** (összefoglalás) → irodalomjegyzék folyamatosan.

Indoklás: először a közös alapok, majd a gerinc funkciófejezetei sorban (így a kereszthivatkozások előre mutathatnak); a keretező fejezetek a kész tartalomra hivatkozva pontosabbak lesznek.

## Tervezett LaTeX-struktúra (még nem hozzuk létre!)

```
thesis/
├── main.tex                  % dokumentumosztály, magyar babel/polyglossia, csomagok, \include-ok
├── references.bib            % BibTeX-források
├── chapters/01_bevezetes.tex … 12_osszefoglalas.tex   % fejezetenként egy fájl (12 db)
├── figures/                  % PDF/PNG ábrák + képernyőképek
├── listings/                 % hosszabb kódrészletek külön fájlban
├── planning/                 % (ez a mappa, már létezik)
└── reference/                % mintadolgozat (nem feldolgozott)
```

Technikai terv: `report` osztály 12pt, A4, egyoldalas; `[magyar]{babel}` + `T1`/UTF-8 (pdfLaTeX) vagy XeLaTeX+fontspec; `listings` vagy `minted` kódkiemelés C#/TSX-hez magyar ékezetbiztos beállítással; `biblatex` (backend=biber); `hyperref`+`cleveref` kereszthivatkozások; `graphicx`, `booktabs`, `caption` magyar feliratokkal („x.y. ábra”). Ha van kötelező BME-sablon, az felülírja ezeket.

## Alfejezetenkénti kötelező elemek (minőségi checklist íráskor)

- [ ] Minden állítás mögött konkrét fájl/osztály/metódus-hivatkozás áll.
- [ ] Megvalósított vs. előkészített funkció egyértelműen elválasztva (feature_matrix szerint).
- [ ] Kódrészlet max. ~25 sor, magyarázattal; számozott listing.
- [ ] Ábrák számozva, szövegből hivatkozva.
- [ ] Nincs kitalált végpont, tábla, teszteredmény, üzemeltetési állítás.
