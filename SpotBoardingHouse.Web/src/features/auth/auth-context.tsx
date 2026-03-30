import { api } from "../../lib/api";
import {
  clearAuthSession,
  decodeToken,
  getToken,
  isTokenExpired,
  saveToken,
} from "../../lib/auth";
import type { LoginRequest, LoginResponse } from "./types";
import { AuthContext } from "./auth-context.base";
import { useMemo, useState } from "react";

function readRoleFromToken(token: string): string | null {
  if (isTokenExpired(token)) return null;

  try {
    const payload = decodeToken(token);
    return (
      payload.role ??
      payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
      null
    );
  } catch {
    return null;
  }
}

function readAuthStateFromStorage(): {
  isAuthenticated: boolean;
  role: string | null;
} {
  const token = getToken();
  if (!token) return { isAuthenticated: false, role: null };

  if (isTokenExpired(token)) {
    clearAuthSession(true);
    return { isAuthenticated: false, role: null };
  }

  return { isAuthenticated: true, role: readRoleFromToken(token) };
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const initialAuthState = readAuthStateFromStorage();
  const [isAuthenticated, setIsAuthenticated] = useState(
    initialAuthState.isAuthenticated,
  );
  const [role, setRole] = useState<string | null>(initialAuthState.role);

  const login = async (payload: LoginRequest) => {
    const { data } = await api.post<LoginResponse>(
      "/api/v1/auth/login",
      payload,
    );
    saveToken(data.accessToken);
    setIsAuthenticated(true);
    setRole(data.role || readRoleFromToken(data.accessToken));
  };

  const logout = (markExpired = false) => {
    clearAuthSession(markExpired);
    setIsAuthenticated(false);
    setRole(null);
  };

  const value = useMemo(
    () => ({ isAuthenticated, role, login, logout }),
    [isAuthenticated, role],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
