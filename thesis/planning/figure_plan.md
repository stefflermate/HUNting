# HUNting – Ábra- és képernyőképterv (figure_plan.md)

> Méretjelölés: S ≈ ⅙ oldal, M ≈ ⅓ oldal, L ≈ ½ oldal.
> Elkészítési mód: „diagram” = manuálisan (draw.io / PlantUML / Mermaid → PDF/PNG export); „screenshot” = a szerző készíti futó alkalmazásról; „generálható” = eszközzel előállítható.
> A diagramokon kizárólag a forráskóddal igazolt kapcsolatok szerepelhetnek (ld. repository_audit.md).

## A. Diagramok

| ID | Cím (javaslat) | Típus | Fejezet | Miért szükséges | Forrás | Mód | Méret |
|---|---|---|---|---|---|---|---|
| U-01 | A HUNting használati esetei | Use Case (3 aktor: Vendég, Vadász/Person, Társaság/Society + Admin) | 1.3 v. 3.1 | a funkciókör és a szerepek áttekintése | feature_matrix.md, appRouter.tsx | diagram | L |
| T-01 | A technológiai stack áttekintése | rétegzett stack-ábra | 2 eleje | a 2. fejezet vezérfonala | repository_audit 1. táblázat | diagram | M |
| A-01 | Háromrétegű architektúra | architektúradiagram (SPA ↔ REST/JSON ↔ API ↔ EF ↔ MSSQL + külső API-k) | 3.1 | a dolgozat központi ábrája | Program.cs, apiClient.ts | diagram | L |
| A-02 | Egy REST-kérés életútja | folyamatábra (interceptor → CORS → authN → authZ → controller → service → EF → JSON) | 3.2 | middleware-pipeline megértetése | Program.cs middleware-sorrend | diagram | M |
| A-03 | Backend-komponensdiagram | komponensdiagram (8 controller → 6 service/interfész → DbContext; typed HttpClient → OpenAI) | 4.2 | DI-kapcsolatok igazolt ábrázolása | Program.cs regisztrációk | diagram | L |
| E-01 | A VadászNet adatbázis ER-diagramja | ER (≈20 tábla: Identity-mag + Society-domain + Ads + HuntingLogs + News + Map) | 4.1 | teljes adatmodell | HuntingContext.cs, migrációk | diagram (Mermaid erDiagram kódból generálható) | egész oldal |
| E-02 | A társasági domain osztálydiagramja | UML osztály (ApplicationUser, Society, SocietyMember, SocietyMembershipRequest, SocietyRole, SocietyPost) | 7.2 | összetett kulcsok + 1–1 owner viszony | Models/Societies/*, Models/Users/* | diagram | L |
| B-01 | Bejelentkezés és tokenkibocsátás | UML szekvencia (LoginPage → AuthContext → axios → AuthController → SignInManager → JwtTokenService → localStorage) | 5.3 | az autentikáció teljes vertikuma | AuthController.cs, AuthContext.tsx | diagram | L |
| B-02 | Kérések hitelesítése | folyamatábra (Bearer-fejléc → validálás → claims → [Authorize]/szerep → erőforrás-tulajdonos ellenőrzés; 401-interceptor ág) | 5.4 | a kétszintű (szerep + tulajdonos) modell | JwtTokenService.cs, apiClient.ts | diagram | M |
| F-01 | Hirdetésfeladás képfeltöltéssel | UML szekvencia (AdForm → POST /ads → AdService → SaveChanges → képenként POST /ads/{id}/images → wwwroot/uploads) | 6.5 | többlépéses create-folyamat | AdCreatePage.tsx, AdsController.cs | diagram | L |
| F-02 | Tagsági kérelem életciklusa | UML állapotdiagram (Pending → Approved / Rejected → újranyitás Pendingre) | 7.3 | az üzleti folyamat magja | SocietyService.cs, SocietyMembershipRequest | diagram | M |
| M-01 | Térképi adatkezelés folyamata | folyamatábra (rétegválasztás → scope-kérés → tagság-ellenőrzés → markerek/polygonok → rajzolás → mentés Seq-pontokkal) | 9.4 | a legösszetettebb kliens-szerver folyamat | MapPage.tsx, MapController.cs, MapService.cs | diagram | L |
| C-01 | A chatbot-kérés feldolgozása | UML szekvencia (Chatbot → POST /Chat → OpenAiService → api.openai.com → reply) + szerep-szanitálás kiemelve | 10.2 | külső AI-integráció bemutatása | Chatbot.tsx, OpenAiService.cs | diagram | M |
| FE-01 | A frontend provider- és layoutfája | fa-ábra (main → QueryClientProvider → AuthProvider → Suspense → Router → AppLayout/AuthLayout → oldalak) | 4.3 | a React-struktúra áttekintése | App.tsx, appRouter.tsx | diagram | M |
| Q-01 | Tesztpiramis a HUNting-ban | piramis (124 xUnit / 18 Vitest-fájl / 23 Playwright) | 11 eleje | tesztstratégia összképe | tesztprojektek | diagram | S |

## B. Képernyőképek (a szerző készíti futó alkalmazásról)

| ID | Tartalom | Fejezet | Megjegyzés |
|---|---|---|---|
| SC-01 | Főoldal (hero + számlálók + FeatureGrid), desktop ÉS mobil nézet egymás mellett | 1.3 + 4.3 | a mobil nézet a reszponzivitást igazolja |
| SC-02 | Hirdetéslista szűrőkkel és lapozóval | 6.1 | látszódjon aktív kategória- és árszűrő |
| SC-03 | Hirdetés részletei képekkel + tulajdonosi gombok | 6.1 | bejelentkezett tulajdonosként |
| SC-04 | Hirdetésfeladó űrlap képelőnézettel | 6.4 | drag&drop zóna látható |
| SC-05 | Társaságkatalógus névszűréssel | 7.1 | |
| SC-06 | Társaság-részletek + tagsági kérelem űrlap | 7.1 | Person fiókkal |
| SC-07 | Társaságkezelő: függő kérelmek + jóváhagyás | 7.4 | Society fiókkal |
| SC-08 | Saját társaságaim + posztok | 7.4 | tag nézőpontból |
| SC-09 | Térkép: markerek + polygon-rajzolás közben | 9.1 | Google Maps kulcs szükséges |
| SC-10 | Vadásznapló: lista + új bejegyzés űrlap | 8.1 | |
| SC-11 | Időjárás-oldal találattal | 10.4 | OpenWeather-kulcs szükséges |
| SC-12 | Bejelentkezési oldal | 5.1 | a mojibake-javítás után készüljön! |
| SC-13 | Regisztráció Society-mezőkkel | 5.1 | fióktípus-váltó látszódjon |
| SC-14 | Admin dashboard (felhasználók tab, törlés-megerősítés) | 5.5 | admin fiókkal |
| SC-15 | Chatbot nyitva, magyar válasszal | 10.1 | OpenAI-kulcs szükséges |
| SC-16 | Profiloldal | 5.1 | |
| SC-17 | Swagger UI a végpontlistával | 4.2 v. 11.4 | Development módban |

Képernyőkép-konvenciók: egységes böngészőablak-méret (pl. 1440×900), magyar seed-adatokkal, személyes adat nélkül; fájlnév: `figures/sc-XX-nev.png`.

## C. Kódrészletek (listings – tervezett)

| ID | Tartalom | Fejezet | Fájl | ~sorok |
|---|---|---|---|---|
| L-01 | DI-regisztrációk és middleware-sorrend | 4.2 | Program.cs (kivonat) | 20 |
| L-02 | JWT-token előállítása (claimek) | 5.3 | Services/Auth/JwtTokenService.cs | 20 |
| L-03 | Erőforrás-tulajdonos ellenőrzés mintája | 5.3 | Controllers/AdsController.cs (Update) | 15 |
| L-04 | Idempotens seed-részlet | 4.1 | Data/SeedData.cs (kivonat) | 15 |
| L-05 | Lapozott-szűrt lekérdezés | 6.3 | Services/AdService.cs (GetPagedAsync) | 20 |
| L-06 | React Query lekérdezés + invalidáció | 6.4 | AdsListPage.tsx / AdCreatePage.tsx (kivonat) | 15 |
| L-07 | Axios interceptorok (Bearer + 401) | 4.3 | src/lib/apiClient.ts | 20 |
| L-08 | OpenAI-kérés összeállítása és szerep-szanitálás | 10.2 | Services/OpenAiService.cs (kivonat) | 20 |
| L-09 | Polygon-pontok mentése Seq-sorrenddel | 9.3 | Services/MapService.cs (kivonat) | 15 |
| L-10 | Mockolt HttpMessageHandler teszt | 11.1 | OpenAiServiceTests.cs (kivonat) | 15 |
| L-11 | Playwright webServer-konfiguráció | 11.3 | playwright.config.ts (kivonat) | 15 |
| L-12 | Összetett kulcs + kapcsolat konfiguráció | 7.2 | Data/HuntingContext.cs (SocietyMember kivonat) | 15 |

## D. Táblázatok (tervezett)

- 4.2: végpont-összefoglaló (controller / végpontszám / auth-szint).
- 4.1: törlési viselkedések (kapcsolat → Cascade/Restrict/SetNull).
- 11.1: teszteloszlás fájlonként (124 teszt).
- 2.x: technológia–verzió–szerep táblázat.

## Mennyiségi összkép

15 diagram + 17 képernyőkép + 12 kódrészlet + 4 táblázat ≈ a 71 oldalhoz illeszkedő ábrasűrűség (átlagosan ~1 vizuális elem / 1,5 oldal), a tervezett méretekkel együtt sem szorítja ki az érdemi szöveget (becsült össz-ábraterület ≈ 11–13 oldal-egyenérték, a fejezetenkénti oldalszámokba beleértve).
