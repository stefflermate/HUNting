import { expect, test } from "@playwright/test";
import { SEEDED_USERS, loginViaApi, uniqueId } from "./helpers";

test.describe("Hirdetesek", () => {
  test("a hirdetes lista betolt es van talalat", async ({ page }) => {
    await page.goto("/ads");

    await expect(page.getByRole("heading", { name: "Hirdetesek" })).toBeVisible();
    await expect(page.locator("article").first()).toBeVisible();
    await expect(page.getByRole("link", { name: "Reszletek" }).first()).toBeVisible();
  });

  test("a kereso szuri a talalatokat", async ({ page }) => {
    await page.goto("/ads");

    await page.getByPlaceholder("Keress cim vagy leiras alapjan...").fill("Swarovski");

    await expect(
      page.getByRole("heading", { name: "Swarovski Z6i 2-12x50 celtavcso" })
    ).toBeVisible();
  });

  test("a reszletek oldal megnyilik a listabol", async ({ page }) => {
    await page.goto("/ads");

    const firstCard = page.locator("article").first();
    const title = (await firstCard.getByRole("heading").first().textContent()) ?? "";
    await firstCard.getByRole("link", { name: "Reszletek" }).click();

    await expect(page).toHaveURL(/\/ads\/\d+$/);
    await expect(page.getByRole("heading", { name: title.trim() })).toBeVisible();
  });

  test("vendegkent a hirdetesfeladas a login oldalra iranyit", async ({ page }) => {
    await page.goto("/ads/new");
    await expect(page).toHaveURL(/\/auth\/login/);
  });

  test("bejelentkezve uj hirdetes adhato fel", async ({ page, request }) => {
    await loginViaApi(page, request, SEEDED_USERS.hunter.email, SEEDED_USERS.hunter.password);

    const title = `E2E teszt hirdetes ${uniqueId()}`;
    await page.goto("/ads/new");
    await expect(page.getByRole("heading", { name: "Hirdetes feladasa" })).toBeVisible();

    await page.fill("#title", title);
    await page.fill("#description", "Playwright altal letrehozott teszthirdetes.");
    await page.fill("#price", "12345");
    await page.fill("#location", "Budapest");
    await page.getByRole("button", { name: "Hirdetes mentese" }).click();

    // Sikeres mentes utan a reszletek oldalra kerulunk
    await page.waitForURL(/\/ads\/\d+$/);
    await expect(page.getByRole("heading", { name: title })).toBeVisible();
    // Sajat hirdetesnel latszik a szerkesztes lehetoseg
    await expect(page.getByRole("link", { name: "Szerkesztes" })).toBeVisible();
  });
});
