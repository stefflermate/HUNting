import { expect, test } from "@playwright/test";
import { SEEDED_USERS, loginViaApi, uniqueId } from "./helpers";

test.describe("Vadasznaplo", () => {
  test("vendegkent a naplo a login oldalra iranyit", async ({ page }) => {
    await page.goto("/logs");
    await expect(page).toHaveURL(/\/auth\/login/);
  });

  test("bejelentkezve a naplo oldal betolt", async ({ page, request }) => {
    await loginViaApi(page, request, SEEDED_USERS.hunter.email, SEEDED_USERS.hunter.password);

    await page.goto("/logs");

    await expect(page.getByRole("heading", { name: "Naplobejegyzesek" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Uj bejegyzes" })).toBeVisible();
  });

  test("uj naplobejegyzes rogzitheto", async ({ page, request }) => {
    await loginViaApi(page, request, SEEDED_USERS.hunter.email, SEEDED_USERS.hunter.password);

    const location = `E2E les ${uniqueId()}`;
    await page.goto("/logs");

    await page.getByRole("button", { name: "Uj bejegyzes" }).click();
    await page.fill("#locationText", location);
    await page.fill("#animalType", "Oz");
    await page.fill("#notes", "Playwright altal rogzitett bejegyzes.");
    await page.getByRole("button", { name: "Bejegyzes mentese" }).click();

    // Az uj bejegyzes a lista elejere kerul
    await expect(page.getByRole("heading", { name: location })).toBeVisible();
  });

  test("a bejegyzes reszletei lenyithatok", async ({ page, request }) => {
    await loginViaApi(page, request, SEEDED_USERS.hunter.email, SEEDED_USERS.hunter.password);

    const location = `E2E reszletek ${uniqueId()}`;
    await page.goto("/logs");

    await page.getByRole("button", { name: "Uj bejegyzes" }).click();
    await page.fill("#locationText", location);
    await page.fill("#animalType", "Szarvas");
    await page.getByRole("button", { name: "Bejegyzes mentese" }).click();

    const entry = page.locator("article", { hasText: location });
    await entry.click();

    await expect(entry.getByText("Vad: Szarvas")).toBeVisible();
  });
});
