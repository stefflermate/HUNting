import { expect, test } from "@playwright/test";

test.describe("Alap navigacio (smoke)", () => {
  test("a kezdolap betolt a fo elemekkel", async ({ page }) => {
    await page.goto("/");

    await expect(
      page.getByRole("heading", {
        name: "Találd meg a következő vadászmeghívást, vagy kezeld könnyedén a társaságodat.",
      })
    ).toBeVisible();

    await expect(page.getByRole("link", { name: "Aktív hirdetések böngészése" })).toBeVisible();
    await expect(page.getByText("HUNting Portal")).toBeVisible();
  });

  test("a fejlecbol minden fo oldal elerheto", async ({ page }) => {
    await page.goto("/");

    const header = page.locator("header");
    await expect(header.getByRole("link", { name: "Kezdolap" })).toBeVisible();
    await expect(header.getByRole("link", { name: "Hirdetesek" })).toBeVisible();
    await expect(header.getByRole("link", { name: "Tarsasagok" })).toBeVisible();
    await expect(header.getByRole("link", { name: "Terkep" })).toBeVisible();
    await expect(header.getByRole("link", { name: "Idojaras" })).toBeVisible();

    await header.getByRole("link", { name: "Hirdetesek" }).click();
    await expect(page).toHaveURL(/\/ads$/);
    await expect(page.getByRole("heading", { name: "Hirdetesek" })).toBeVisible();
  });

  test("vendegkent a belepes es regisztracio linkek latszanak", async ({ page }) => {
    await page.goto("/");

    await expect(page.getByRole("link", { name: "Bejelentkezes" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Csatlakozz" })).toBeVisible();
  });

  test("ismeretlen utvonalon a 404 oldal jelenik meg", async ({ page }) => {
    await page.goto("/nincs-ilyen-oldal");

    await expect(page.getByRole("heading", { name: "404" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Vissza a kezdőlapra" })).toBeVisible();
  });
});
