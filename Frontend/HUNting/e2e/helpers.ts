import { expect, type APIRequestContext, type Page } from "@playwright/test";

export const API_URL = process.env.E2E_API_URL ?? "https://localhost:7003/api";

// A SeedData.cs által létrehozott fiókok
export const SEEDED_USERS = {
  admin: { email: "admin@gmail.com", password: "adminadmin" },
  hunter: { email: "bence.kovacs@example.hu", password: "Hunter123!" },
};

export function uniqueId(): string {
  return `${Date.now()}${Math.floor(Math.random() * 1000)}`;
}

type AuthResponse = {
  accessToken: string;
  user: Record<string, unknown>;
};

// A frontend authStorage.ts kulcsaival tölti fel a localStorage-t,
// így a page betöltésekor az AuthContext bejelentkezett állapotból indul
async function injectAuth(page: Page, auth: AuthResponse): Promise<void> {
  await page.addInitScript(
    ([token, user]) => {
      localStorage.setItem("hunting.accessToken", token);
      localStorage.setItem("hunting.user", user);
    },
    [auth.accessToken, JSON.stringify(auth.user)]
  );
}

export async function loginViaApi(
  page: Page,
  request: APIRequestContext,
  email: string,
  password: string
): Promise<void> {
  const response = await request.post(`${API_URL}/auth/login`, {
    data: { email, password },
  });
  expect(response.ok(), `Sikertelen API bejelentkezes: ${email}`).toBeTruthy();
  const auth = (await response.json()) as AuthResponse;
  await injectAuth(page, auth);
}

// Friss, egyedi Person fiók regisztrálása API-n át, majd bejelentkeztetés —
// determinisztikus tesztekhez (pl. első tagsági jelentkezés)
export async function registerPersonViaApi(
  page: Page,
  request: APIRequestContext
): Promise<{ email: string; password: string; fullName: string }> {
  const id = uniqueId();
  const email = `e2e.user.${id}@example.hu`;
  const password = "E2eTeszt123!";
  const fullName = `E2E Teszt ${id}`;

  const response = await request.post(`${API_URL}/auth/register`, {
    data: {
      fullName,
      email,
      password,
      phone: null,
      accountType: "Person",
    },
  });
  expect(response.ok(), "Sikertelen API regisztracio").toBeTruthy();
  const auth = (await response.json()) as AuthResponse;
  await injectAuth(page, auth);

  return { email, password, fullName };
}
