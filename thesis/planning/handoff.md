# Diplomamunka — átadási jegyzet (handoff)

> Új chatben ezt mondd: „Olvasd el a thesis/planning/handoff.md fájlt és folytassuk a diplomamunkát.”

## Mi ez a projekt

HUNting: vadász közösségi webalkalmazás (ASP.NET Core 8 + EF Core + MSSQL backend,
React 18 + Vite + TypeScript + React Query frontend). Ebből készül magyar nyelvű BME-s
diplomamunka a `thesis/` mappában, LaTeX-ben. A dolgozat **funkciócentrikus**: a gerincét a
funkciófejezetek adják (adatbázis → backend → frontend bemutatás funkciónként).

## Hol tartunk (2026-10-19 — MINDEN FEJEZET + references.bib KÉSZ)

### Kész

- `thesis/planning/` — 4 tervdokumentum, ezek a „szerződés”:
  - `thesis_outline.md` — 12 fejezetes vázlat, oldalszámokkal (Σ≈71 oldal), írási sorrenddel
  - `repository_audit.md` — a kódbázis felmérése
  - `feature_matrix.md` — funkciónkénti állapot (pl. News = csak backend, auth nélkül; Map visibility = csak címke; MapPath = előkészített)
  - `figure_plan.md` — 15 diagram + 17 screenshot (SC-01…17) + 12 listing (L-01…12) terve
- `thesis/main.tex` — **ideiglenes** gyökérfájl (report osztály, magyar babel, listings
  beállítva); mind a 12 fejezetet include-olja, `\setcounter` már NINCS, a végén
  `\bibliographystyle{plain}` + `\bibliography{references}`. Később a hivatalos
  ftsrg/thesis-template-latex sablonra költözünk, a fejezetfájlok sablonfüggetlenek.
- `thesis/references.bib` — 22 valós forrás (@misc/@book/@phdthesis, URL + „megtekintve” dátum);
  minden \cite-kulcs lefedett, nincs árva bib-tétel (ellenőrizve).
- MIND A 12 FEJEZET megírva (`thesis/chapters/`):
  - `01_bevezetes.tex` (chap:bevezetes) — 1.4 „Hasonló megoldások” kategóriaszinten íródott,
    konkrét konkurensek megnevezése a szerzővel egyeztetendő (TODO-komment a fájlban!)
  - `02_technologiak.tex` (chap:technologiak) — verziók a package.json/csproj-ból ellenőrizve;
    sok \cite
  - `03_architektura.tex` (chap:architektura) — rétegzett monolit minősítés; A-01, A-02, U-01
  - `04_kozos_rendszeralapok.tex` (chap:alapok) — adattárolás + backend + frontend alapok;
    E-01, A-03, FE-01, SC-17, SC-01; L-01, L-04, L-07
  - `05_felhasznalokezeles.tex` (chap:felhasznalok)
  - `06_hirdetesek.tex` (chap:hirdetesek)
  - `07_tarsasagok.tex` (chap:tarsasagok)
  - `08_vadasznaplo.tex` (chap:naplo)
  - `09_terkep.tex` (chap:terkep)
  - `10_chatbot_idojaras.tex` (chap:chatbot)
  - `11_teszteles.tex` (chap:teszteles) — TÉNYLEGES számok: 124 xUnit / 17 Vitest-tesztfájl /
    26 Playwright-teszt (a korábbi 18/23 becslés helyett megszámolva); L-10, L-11, Q-01;
    futási eredményeket NEM közlünk (nem futtattuk)
  - `12_osszefoglalas.tex` (chap:osszefoglalas — a 7/8/9/10. fejezet hivatkozásai rendben)
- Konzisztencia ellenőrizve szkripttel: minden \ref-nek van \label-je, minden \cite-nak
  bib-tétele, környezetek/kapcsos zárójelek kiegyensúlyozottak, listingek ASCII-only.
- Korábbi munka (külön feladat volt, nem a dolgozat része):
  - `Backend/HUNting.API.Tests/` — 124 xUnit teszt (megírva, SOHA nem futtattuk)
  - `Frontend/HUNting/e2e/` — 26 Playwright E2E teszt (megírva, nem futtattuk)

### Hátravan

1. **1.4 alfejezet:** konkrét hasonló megoldások (konkurensek) egyeztetése a szerzővel,
   majd a kategóriaszintű szöveg konkretizálása (01_bevezetes.tex, TODO-komment jelzi)
2. Ábrák (15 diagram) és screenshotok (17 db) elkészítése — user készíti, a helykitöltők
   `% TODO:` kommentjei megmondják, mi kell hová
3. ftsrg sablonra migrálás (user hozza be a repóba); a fejezetfájlok változtatás nélkül
   átemelhetők, a main.tex-et váltja ki a sablon
4. Overleaf-fordítás és korrektúra (oldalszám-ellenőrzés a tervhez képest)

## Kötelező írási szabályok (eddig is így ment)

- Minden fejezetfájl eleje: `% !TeX root = ../main.tex` + komment a hivatkozott forrásfájlokkal
- lstlisting-ekben CSAK ASCII (kódban nincs magyar ékezet) — pdfLaTeX-biztonság
- Ábrák: `\fbox{\parbox...}` helykitöltők `% TODO:` kommenttel (fordul kép nélkül is)
- Csak forráskódból ellenőrzött állítás kerülhet a szövegbe (minden fejezet előtt a releváns
  fájlokat TELJESEN el kell olvasni)
- Minden funkciófejezetben: végponttábla + őszinte „korlátok” szakasz
- Fejezetenként ~6-8 oldalnyi szöveg, kivonatolt (nem teljes) kódrészletekkel

## Környezeti korlátok

- **Ezen a gépen NINCS dotnet, node, LaTeX** — soha ne futtass buildet/tesztet/fordítást
- A user Overleafen fordít: a `thesis/chapters/` fájlokat + `main.tex`-et tölti fel kézzel
- A repó távoli: ghes.idomsoft.hu (céges GHES; volt egy SSH-gond, nem mi okoztuk, lezárva)

## Nyitott, jóvá nem hagyott ajánlatok

- `LoginPage.tsx` mojibake (hibás ékezetek) javítása — SC-12 screenshot előtt kellene
- `apiClient.ts` port 5001 → 7003 javítás
