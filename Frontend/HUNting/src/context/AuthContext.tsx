import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";
import type { AuthUser } from "@/types";
import {
  getAccessToken,
  getStoredUser,
  purgeAuthStorage,
  setAccessToken,
  setStoredUser,
} from "@/lib/authStorage";
import { authApi } from "@/services/authApi";

type LoginCredentials = {
  email: string;
  password: string;
};

type RegisterPayload = {
  name: string;
  email: string;
  password: string;
  phone?: string;
};

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login(credentials: LoginCredentials): Promise<void>;
  register(payload: RegisterPayload): Promise<void>;
  logout(): Promise<void>;
  refreshProfile(): Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<AuthUser | null>(() => getStoredUser());
  const [isLoading, setIsLoading] = useState(false);

  const login = useCallback(
    async (credentials: LoginCredentials) => {
      setIsLoading(true);
      try {
        const result = await authApi.login(credentials);
        setAccessToken(result.accessToken);
        setStoredUser(result.user);
        setUser(result.user);
      } catch (error) {
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  const register = useCallback(
    async (payload: RegisterPayload) => {
      setIsLoading(true);
      try {
        const result = await authApi.register(payload);
        setAccessToken(result.accessToken);
        setStoredUser(result.user);
        setUser(result.user);
      } catch (error) {
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  const logout = useCallback(async () => {
    try {
      const token = getAccessToken();
      if (token) {
        await authApi.logout();
      }
    } finally {
      purgeAuthStorage();
      setUser(null);
    }
  }, []);

  const refreshProfile = useCallback(async () => {
    const token = getAccessToken();
    if (!token) return;
    const profile = await authApi.me();
    setStoredUser(profile);
    setUser(profile);
  }, []);

  useEffect(() => {
    const token = getAccessToken();
    if (!token || user) return;
    refreshProfile().catch(() => {
      purgeAuthStorage();
      setUser(null);
    });
  }, [user, refreshProfile]);

  const value = useMemo(
    () => ({
      user,
      isLoading,
      isAuthenticated: Boolean(user),
      login,
      register,
      logout,
      refreshProfile,
    }),
    [user, isLoading, login, register, logout, refreshProfile]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used inside AuthProvider");
  }
  return ctx;
}
