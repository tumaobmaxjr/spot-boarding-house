import { api } from "../../lib/api";
import { clearToken, decodeToken, getToken, saveToken } from "../../lib/auth";
import type { LoginRequest, LoginResponse } from "./types";
import { AuthContext } from "./auth-context.base";
import { useMemo, useState } from "react";

function readRoleFromStorage(): string | null {
  const token = getToken();
  if (!token) return null;

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

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(!!getToken());
  const [role, setRole] = useState<string | null>(readRoleFromStorage());

  const login = async (payload: LoginRequest) => {
    const { data } = await api.post<LoginResponse>(
      "/api/v1/Auth/login",
      payload,
    );
    saveToken(data.accessToken);
    setIsAuthenticated(true);
    setRole(data.role || readRoleFromStorage());
  };

  const logout = () => {
    clearToken();
    setIsAuthenticated(false);
    setRole(null);
  };

  const value = useMemo(
    () => ({ isAuthenticated, role, login, logout }),
    [isAuthenticated, role],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
