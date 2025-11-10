import { apiClient } from "@/lib/apiClient";
import type { AccountType, AuthUser } from "@/types";
import { clearAccessToken } from "@/lib/authStorage";

type LoginResponse = {
  accessToken: string;
  user: AuthUser;
};

type LoginRequest = {
  email: string;
  password: string;
};

type RegisterRequest = {
  name: string;
  email: string;
  password: string;
  phone?: string;
  accountType: AccountType;
  society?: {
    name: string;
    email?: string;
    phone?: string;
    description?: string;
  };
};

async function login(payload: LoginRequest): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>("/auth/login", payload);
  return data;
}

async function register(payload: RegisterRequest): Promise<LoginResponse> {
  const body = {
    fullName: payload.name,
    email: payload.email,
    password: payload.password,
    phone: payload.phone,
    accountType: payload.accountType,
    society: payload.society
      ? {
          name: payload.society.name,
          email: payload.society.email,
          phone: payload.society.phone,
          description: payload.society.description,
        }
      : undefined,
  };
  const { data } = await apiClient.post<LoginResponse>("/auth/register", body);
  return data;
}

async function logout() {
  try {
    await apiClient.post("/auth/logout");
  } finally {
    clearAccessToken();
  }
}

async function me(): Promise<AuthUser> {
  const { data } = await apiClient.get<AuthUser>("/auth/me");
  return data;
}

export const authApi = {
  login,
  register,
  logout,
  me,
};
