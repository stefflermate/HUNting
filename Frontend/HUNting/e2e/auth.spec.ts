import { expect, test } from "@playwright/test";
import { SEEDED_USERS, loginViaApi, uniqueId } from "./helpers";

test.describe("Hitelesites", () => {
  test("uj szemely fiok regisztralhato a felileten", async ({ page }) => {
    const id = uniqueId();
    await page.goto("/auth/register");

    await page.fill("#name", `E2E Regisztracio ${id}`);
    await page.fill("#email", `e2e.reg.${id}@example.hu`);
    await page.fill("#password", "E2eTeszt123!");
    await page.getByRole("button", { name: "Fiok letrehozasa" }).click();

    // Sikeres regisztracio utan a kezdolapra kerulunk, bejelentkezve
    await page.waitForURL("/");
    await expect(page.getByRole("link", { name: "Profil" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Naplok" })).toBeVisible();
  });

  test("seedelt vadasz be tud jelentkezni a felileten", async ({ page }) => {
    await page.goto("/auth/login");

    await page.fill("#email", SEEDED_USERS.hunter.email);
    await page.fill("#password", SEEDED_USERS.hunter.password);
    await page.locator('button[type="submit"]').click();

    await page.waitForURL("/");
    await expect(page.getByRole("link", { name: "Profil" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Bejelentkezes" })).toHaveCount(0);
  });

  test("hibas jelszoval nem lehet bejelentkezni", async ({ page }) => {
    await page.goto("/auth/login");

    await page.fill("#email", SEEDED_USERS.hunter.email);
    await page.fill("#password", "biztosan-rossz-jelszo");
    await page.locator('button[type="submit"]').click();

    // A 401-es valasz utan a login oldalon maradunk, vendegkent
    await expect(page).toHaveURL(/\/auth\/login/);
    await page.goto("/");
    await expect(page.getByRole("link", { name: "Bejelentkezes" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Profil" })).toHaveCount(0);
  });

  test("kijelentkezes utan vendeg allapotba kerulunk", async ({ page, request }) => {
    await loginViaApi(page, request, SEEDED_USERS.hunter.email, SEEDED_USERS.hunter.password);

    await page.goto("/profile");
    await page.getByRole("button", { name: "Kijelentkezes" }).click();

    // A profil oldal vedelme a login oldalra iranyit
    await page.waitForURL(/\/auth\/login/);
    await page.goto("/");
    await expect(page.getByRole("link", { name: "Bejelentkezes" })).toBeVisible();
  });

  test("vedett oldal vendegkent a login oldalra iranyit", async ({ page }) => {
    await page.goto("/logs");
    await expect(page).toHaveURL(/\/auth\/login/);
  });
});
