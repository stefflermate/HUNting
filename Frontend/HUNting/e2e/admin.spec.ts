import { expect, test } from "@playwright/test";
import { SEEDED_USERS, loginViaApi } from "./helpers";

test.describe("Admin jogosultsag", () => {
  test("admin fiokkal elerheto az admin felulet", async ({ page, request }) => {
    await loginViaApi(page, request, SEEDED_USERS.admin.email, SEEDED_USERS.admin.password);

    await page.goto("/admin");

    await expect(page).toHaveURL(/\/admin$/);
    await expect(page.getByRole("link", { name: "Admin" })).toBeVisible();
  });

  test("vadasz fiokkal az admin oldal a kezdolapra iranyit", async ({ page, request }) => {
    await loginViaApi(page, request, SEEDED_USERS.hunter.email, SEEDED_USERS.hunter.password);

    await page.goto("/admin");

    await page.waitForURL("/");
    await expect(page.getByRole("link", { name: "Admin" })).toHaveCount(0);
  });

  test("vendegkent az admin oldal nem erheto el", async ({ page }) => {
    await page.goto("/admin");

    await page.waitForURL("/");
  });
});
