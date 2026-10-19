import { defineConfig, devices } from "@playwright/test";

// A webServer blokk indítja a backendet (dotnet run) és a frontendet (vite) is,
// ha még nem futnak — külön előkészítés nélkül futtatható: npx playwright test
export default defineConfig({
  testDir: "./e2e",
  timeout: 60_000,
  expect: { timeout: 10_000 },
  // Közös adatbázison dolgoznak a tesztek, ezért sorban futnak
  fullyParallel: false,
  workers: 1,
  retries: process.env.CI ? 2 : 0,
  reporter: [["list"], ["html", { open: "never" }]],
  use: {
    baseURL: "http://localhost:5173",
    // A backend önaláírt dev tanúsítványa miatt (https://localhost:7003)
    ignoreHTTPSErrors: true,
    screenshot: "only-on-failure",
    trace: "on-first-retry",
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
  webServer: [
    {
      command: "dotnet run --project ../../Backend/HUNting.API --launch-profile http",
      // A 307-es https-átirányítást a Playwright elfogadja "kész" jelzésként
      url: "http://localhost:5137/api/ads",
      reuseExistingServer: true,
      timeout: 180_000,
    },
    {
      command: "npm run dev -- --port 5173 --strictPort",
      url: "http://localhost:5173",
      reuseExistingServer: true,
      timeout: 120_000,
      env: {
        // Az apiClient.ts dev fallbackje (localhost:5001) nem a valós backend port
        VITE_API_BASE_URL: "https://localhost:7003/api",
      },
    },
  ],
});
