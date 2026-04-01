import { jwtDecode } from "jwt-decode";

export const TOKEN_KEY = "sbh_access_token";
export const SESSION_EXPIRED_KEY = "sbh_session_expired";
export const TOKEN_EXPIRY_SKEW_SECONDS = 30;

type TokenPayload = {
  sub?: string;
  email?: string;
  role?: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string;
  exp?: number;
};

export function saveToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function markSessionExpired(): void {
  sessionStorage.setItem(SESSION_EXPIRED_KEY, "1");
}

export function consumeSessionExpired(): boolean {
  const hasExpiredSession = sessionStorage.getItem(SESSION_EXPIRED_KEY) === "1";
  sessionStorage.removeItem(SESSION_EXPIRED_KEY);
  return hasExpiredSession;
}

export function decodeToken(token: string): TokenPayload {
  return jwtDecode<TokenPayload>(token);
}

export function getRoleFromToken(token: string): string | null {
  const decoded = decodeToken(token);
  if (!decoded) return null;

  return (
    decoded.role ??
    decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
    null
  );
}

export function isTokenExpired(token: string): boolean {
  try {
    const decoded = decodeToken(token);
    if (typeof decoded.exp !== "number") {
      return true;
    }
    return decoded.exp <= Date.now() / 1000 + TOKEN_EXPIRY_SKEW_SECONDS;
  } catch {
    return true;
  }
}

export function clearAuthSession(markExpired = false): void {
  clearToken();
  if (markExpired) {
    markSessionExpired();
  }
}
