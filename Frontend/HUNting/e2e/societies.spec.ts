import { expect, test } from "@playwright/test";
import { registerPersonViaApi } from "./helpers";

test.describe("Tarsasagok", () => {
  test("a tarsasag lista betolt es kereheto", async ({ page }) => {
    await page.goto("/societies");

    await expect(page.getByRole("heading", { name: "Vadásztársaságok" })).toBeVisible();

    await page.getByPlaceholder("Keresés név alapján...").fill("Bakonyi");
    await expect(
      page.getByRole("heading", { name: "Bakonyi Vadasztarsasag" })
    ).toBeVisible();
  });

  test("a tarsasag reszletei megnyithatok", async ({ page }) => {
    await page.goto("/societies");

    await page.getByPlaceholder("Keresés név alapján...").fill("Bakonyi");
    const card = page.locator("article", { hasText: "Bakonyi Vadasztarsasag" });
    await card.getByRole("link", { name: "Társaság megtekintése" }).click();

    await expect(page).toHaveURL(/\/societies\/\d+$/);
    await expect(page.getByText("Bakonyi Vadasztarsasag").first()).toBeVisible();
  });

  test("friss szemely fiok tagsagi jelentkezest kuldhet", async ({ page, request }) => {
    // Friss fiok kell, hogy ne legyen mar folyamatban levo jelentkezes
    await registerPersonViaApi(page, request);

    await page.goto("/societies");
    await page.getByPlaceholder("Keresés név alapján...").fill("Bakonyi");
    const card = page.locator("article", { hasText: "Bakonyi Vadasztarsasag" });
    await card.getByRole("link", { name: "Társaság megtekintése" }).click();
    await page.waitForURL(/\/societies\/\d+$/);

    await page.fill("#join-message", "Playwright E2E tagsagi jelentkezes.");
    await page.getByRole("button", { name: "Jelentkezes bekuldese" }).click();

    await expect(
      page.getByText("A jelentkezesed beert. A vezetoseg hamarosan valaszol.")
    ).toBeVisible();
  });
});
