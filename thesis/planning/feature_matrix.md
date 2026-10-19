# HUNting – Funkciómátrix (feature_matrix.md)

> Állapotkategóriák: ✅ Megvalósított és forráskóddal igazolt · 🟡 Részben megvalósított · 🔵 Csak előkészített/tervezett · ❓ Nem egyértelmű.
> Fejezetszámok a `thesis_outline.md` szerint.

## 1. Regisztráció és bejelentkezés — ✅

- **Frontend:** `src/pages/Auth/LoginPage.tsx`, `RegisterPage.tsx`, `src/context/AuthContext.tsx`, `src/lib/authStorage.ts`, `src/services/authApi.ts`
- **Backend:** `Controllers/AuthController.cs`, `Services/Auth/JwtTokenService.cs`, `Configuration/JwtOptions.cs`
- **Végpontok:** POST `api/Auth/register`, POST `api/Auth/login`, POST `api/Auth/logout`, GET `api/Auth/me`
- **Entitások:** ApplicationUser, Society (society-regisztrációnál), SocietyMember, SocietyRole
- **Tesztek:** AuthControllerTests (11), LoginPage/RegisterPage.test.tsx, e2e/auth.spec.ts (5)
- **Fejezet:** 5 (5.2–5.4)
- **Hiányosság:** logout nem érvényteleníti a JWT-t; LoginPage mojibake-sztringek; jelszó-szabályok lazítva (min. 6).

## 2. Felhasználói profil — ✅

- **Frontend:** `src/pages/Profile/ProfilePage.tsx` (tárolt identitás + szerepek + society-linkek, kijelentkezés)
- **Backend:** GET `api/Auth/me`
- **Tesztek:** ProfilePage.test.tsx, e2e/auth.spec.ts (logout)
- **Fejezet:** 5 (5.1, 5.4)
- **Megjegyzés:** nincs profilszerkesztés (név/telefon módosítása UI-ból) — továbbfejlesztési pont.

## 3. Hirdetési piactér (lista, keresés, részletek) — ✅

- **Frontend:** `src/pages/Ads/AdsListPage.tsx`, `AdDetailPage.tsx`, `src/services/adsApi.ts`, `src/pages/components/LatestAds.tsx`
- **Backend:** `Controllers/AdsController.cs`, `Services/AdService.cs`, `Services/AdQueryOptions.cs`, `Models/PagedResult.cs`
- **Végpontok:** GET `api/Ads` (lapozás+szűrés), GET `api/Ads/{id}`, GET `categories`, GET `statuses`
- **Entitások:** Ad, AdCategory, AdStatus, AdImage
- **Tesztek:** AdsControllerTests (23), AdsListPage/AdDetailPage.test.tsx, e2e/ads.spec.ts (5)
- **Fejezet:** 6 (6.1–6.4)
- **Megjegyzés:** `statuses` API-függvény definiált a kliensben, de a vizsgált oldalak nem használják.

## 4. Hirdetés létrehozása/szerkesztése/törlése képekkel — ✅

- **Frontend:** `AdCreatePage.tsx`, `AdEditPage.tsx`, `AdForm.tsx` (drag&drop, paste, előnézet)
- **Backend:** POST/PUT/DELETE `api/Ads/...`, POST `api/Ads/{id}/images`, DELETE `.../images/{imageId}`; fájlok: `wwwroot/uploads/ads`
- **Biztonság:** tulajdonos vagy Administrator; CK_Ads_SingleOwner; kiterjesztés-whitelist, GUID-név, 10 MiB
- **Tesztek:** AdsControllerTests (képes ágak mockolt IFormFile-lal), e2e/ads.spec.ts (create)
- **Fejezet:** 6 (6.3–6.5), 4.2 (közös fájlfeltöltési minta)

## 5. Vadásztársaság-katalógus és részletek — ✅

- **Frontend:** `SocietiesPage.tsx` (kliensoldali névszűrés), `SocietyDetailPage.tsx`
- **Backend:** GET `api/Societies`, GET `api/Societies/{id}` (anonim)
- **Entitások:** Society
- **Tesztek:** SocietiesControllerTests, SocietiesPage/SocietyDetailPage.test.tsx, e2e/societies.spec.ts
- **Fejezet:** 7 (7.1, 7.4)
- **Hiányosság:** a listaoldali „Jelentkezés tagságra” gombnak nincs handlere (a működő űrlap a detail oldalon van).

## 6. Tagsági kérelmek (jelentkezés, jóváhagyás, elutasítás) — ✅

- **Frontend:** `SocietyDetailPage.tsx` (join űrlap), `SocietyManagementPage.tsx` (kérelmek kezelése), `MySocietiesPage.tsx`
- **Backend:** POST `/{id}/join`, GET `/{id}/requests`, POST `.../approve`, POST `.../reject`, GET `/{id}/members`, GET `api/Societies/my`; `Services/SocietyService.cs`
- **Entitások:** SocietyMembershipRequest (összetett kulcs, Pending/Approved/Rejected), SocietyMember, SocietyRole („Tag” szereppel felvétel)
- **Tesztek:** SocietiesControllerTests (25-ből a request-ágak), SocietyManagementPage.test.tsx, e2e/societies.spec.ts (join friss userrel)
- **Fejezet:** 7.3
- **Megjegyzés:** elutasított kérelem újra beadható (újranyitás) — dokumentálandó életciklus.

## 7. Társasági bejegyzések (SocietyPost) — ✅

- **Frontend:** `SocietyManagementPage.tsx` (létrehozás képpel, törlés), `MySocietiesPage.tsx` (olvasás tagként)
- **Backend:** GET/POST `/{id}/posts`, DELETE `/{id}/posts/{postId}`; fájlok: `wwwroot/uploads/societies`
- **Entitások:** SocietyPost
- **Tesztek:** SocietiesControllerTests posts-ágak, SocietyManagementPage.test.tsx
- **Fejezet:** 7.3–7.4

## 8. Hírek (NewsFeed) — 🟡 részben: csak backend, frontend UI nincs

- **Frontend:** NINCS oldal, nincs service-hívás (csak a `NewsFeedItem` típus létezik a `types/index.ts`-ben)
- **Backend:** `Controllers/NewsController.cs` — teljes CRUD, **autorizáció nélkül**; láthatóság-szűrés nincs
- **Entitások:** NewsFeed, NewsVisibility (MEMBERS/PUBLIC/OFFICERS); 2 seedelt hír
- **Tesztek:** NewsControllerTests (8)
- **Fejezet:** 7.5 (elhatárolás + korlát), 11.5
- **Teendő a dolgozatban:** kifejezetten backend-előkészítésként dokumentálni; NEM állítható felhasználói hírfolyam-funkció.

## 9. Térkép: markerek — ✅

- **Frontend:** `src/pages/Map/MapPage.tsx` (Google Maps, kattintásos felvétel, ikonszín, info-ablak, törlés), `src/services/mapApi.ts`
- **Backend:** GET/POST/PUT/DELETE `api/Map/markers`, GET `api/Map/config`; `Services/MapService.cs`
- **Entitások:** MapMarker, MapIconType, MapVisibility
- **Tesztek:** MapControllerTests (24-ből marker-ágak), MapPage.test.tsx
- **Fejezet:** 9
- **Megjegyzés:** `updateMarker` API-függvény létezik a kliensben, de a térképoldal nem kínál szerkesztő UI-t (csak létrehozás/törlés) — a PUT végpont backend-oldalon kész.

## 10. Térkép: területek (polygonok) — ✅

- **Frontend:** `MapPage.tsx` DrawingManagerF rajzolás, név/leírás/szín/láthatóság mentése, törlés; min. 3 pont validáció
- **Backend:** GET/POST/PUT/DELETE `api/Map/polygons`; pontok Seq-rendezéssel, (PolygonId,Seq) unique
- **Entitások:** MapPolygon, MapPolygonPoint
- **Tesztek:** MapControllerTests polygon-ágak, e2e nincs (Google Maps API-kulcs-függő)
- **Fejezet:** 9
- **Megjegyzés:** polygon-szerkesztés UI szintén nincs (update végpont kész).

## 11. Térkép: útvonalak (paths) — 🔵 csak előkészített

- **Frontend:** nincs UI, nincs API-függvény (csak `MapPathItem` típus)
- **Backend:** NINCS végpont; entitás + migráció létezik (MapPath, MapPathPoint — AddMapEntities)
- **Fejezet:** 12.2 (továbbfejlesztés); 9.2-ben az adatmodell megemlítve „előkészített” jelöléssel
- **Szabály:** működő funkcióként tilos bemutatni.

## 12. Személyes vs. társasági térképrétegek, láthatóság — 🟡

- **Megvalósult:** scope=personal/society lekérdezés tagság-ellenőrzéssel (`MapService.UserBelongsToSocietyAsync`); láthatósági kód tárolása és UI-választó
- **Hiányzik:** a lekérdezések a `VisibilityCode`-ot NEM szűrik (PUBLIC/MEMBERS/ADMIN jelenleg címkeként működik)
- **Fejezet:** 9.5 — a korlátot explicit le kell írni.

## 13. Vadásznapló — ✅

- **Frontend:** `HuntingLogsPage.tsx` (lista, lenyitható részletek, űrlap képfeltöltéssel), `huntingLogsApi.ts`
- **Backend:** `HuntingLogsController.cs` (CRUD, scope=all adminnak, society-fiók tiltva, kép: `uploads/logs`)
- **Entitások:** HuntingLog
- **Tesztek:** HuntingLogsControllerTests (16), HuntingLogsPage.test.tsx, e2e/hunting-logs.spec.ts (4)
- **Fejezet:** 8
- **Megjegyzés:** a kliens create-payloadja `shotFired=false`, `shareWithSociety=false` fix értékkel küld — a mezők UI-vezérlése hiányzik (korlát); PUT/DELETE végpont kész, de a UI nem kínál szerkesztést/törlést.

## 14. Időjárás — ✅ (frontend-only architektúrával)

- **Frontend:** `WeatherPage.tsx`, `weatherService.ts` — KÖZVETLEN OpenWeather-hívás (`VITE_OPEN_WEATHER_API`)
- **Backend:** nincs időjárás-végpont/konfiguráció
- **Tesztek:** WeatherPage.test.tsx
- **Fejezet:** 10.4
- **Korlát:** kliensoldali API-kulcs-kitettség; backend-proxy mint továbbfejlesztés.

## 15. AI-chatbot — ✅

- **Frontend:** `src/components/Chatbot.tsx` (lebegő widget AppLayoutban, react-markdown, history komponens-state-ben)
- **Backend:** `ChatController.cs` + `OpenAiService.cs` (gpt-4o, magyar vadászati rendszerprompt, szerep-szanitálás, typed HttpClient)
- **Végpont:** POST `api/Chat`
- **Tesztek:** ChatControllerTests (5), OpenAiServiceTests (4), Chatbot.test.tsx, e2e/chatbot.spec.ts (2)
- **Fejezet:** 10.1–10.3
- **Korlát:** a végpont anonim, nincs rate limit; history nem perzisztált.

## 16. Adminisztrációs felület — ✅

- **Frontend:** `AdminDashboardPage.tsx` (tabok: hirdetések, felhasználók; törlés megerősítéssel), `adminApi.ts`
- **Backend:** `UsersController.cs` (admin-only CRUD szerepkezeléssel), AdsController admin-ágai, HuntingLogs `scope=all`
- **Tesztek:** UsersControllerTests (8), e2e/admin.spec.ts (3)
- **Fejezet:** 5.5
- **Megjegyzés:** admin felhasználó-létrehozás/szerkesztés végpont kész (POST/PUT `api/Users`), de a dashboard csak listáz+töröl — UI-korlát.

## 17. Főoldal / aggregátumok — ✅

- **Frontend:** `HomePage.tsx` (aktív hirdetések száma, társaságszám, kategóriaszám; FeatureGrid, LatestAds)
- **Backend:** meglévő listázó végpontokból számol
- **Tesztek:** App.test.tsx, e2e/smoke.spec.ts
- **Fejezet:** 1.3 (képernyőkép), 4.3

## Összegzés

| Állapot | Funkciók |
|---|---|
| ✅ Megvalósított | 1, 2, 3, 4, 5, 6, 7, 9, 10, 13, 14, 15, 16, 17 |
| 🟡 Részben | 8 (News: backend-only), 12 (láthatóság-szűrés hiánya) |
| 🔵 Előkészített | 11 (Map paths) |
| ❓ Tisztázandó | — (nem maradt nyitott besorolás) |
