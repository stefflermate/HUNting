import { expect, test } from "@playwright/test";

test.describe("Chatbot widget", () => {
  test("a chatbot megnyithato es fogadokesz", async ({ page }) => {
    await page.goto("/");

    // Az indito gombnak nincs szoveges cimkeje, csak Lucide ikonja
    await page.locator("button:has(svg.lucide-message-circle)").click();

    await expect(page.getByText("Vadászati Asszisztens")).toBeVisible();
    await expect(
      page.getByText(
        "Üdvözlöm! Miben segíthetek a vadászattal, fegyverekkel vagy az 1996-os vadászati törvénnyel kapcsolatban?"
      )
    ).toBeVisible();

    const input = page.getByPlaceholder("Írj egy üzenetet...");
    await expect(input).toBeVisible();
    await input.fill("Teszt uzenet");
    // A kuldes gomb csak kitoltott mezonel aktiv (nem kuldjuk el, mert OpenAI kulcsot igenyelne)
    await expect(page.locator("button:has(svg.lucide-send)")).toBeEnabled();
  });

  test("az auth oldalakon nincs chatbot", async ({ page }) => {
    await page.goto("/auth/login");

    await expect(page.locator("button:has(svg.lucide-message-circle)")).toHaveCount(0);
  });
});
