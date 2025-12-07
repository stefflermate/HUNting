import type { AuthUser } from "@/types";

const TOKEN_KEY = "hunting.accessToken";
const USER_KEY = "hunting.user";

export type StoredUser = AuthUser;

export function getAccessToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setAccessToken(token: string) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearAccessToken() {
  localStorage.removeItem(TOKEN_KEY);
}

export function getStoredUser(): StoredUser | null {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as Partial<StoredUser>;
    if (!parsed.accountType) {
      parsed.accountType = "Person";
    }
    return parsed as StoredUser;
  } catch {
    return null;
  }
}

export function setStoredUser(user: StoredUser) {
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearStoredUser() {
  localStorage.removeItem(USER_KEY);
}

export function purgeAuthStorage() {
  clearAccessToken();
  clearStoredUser();
}
