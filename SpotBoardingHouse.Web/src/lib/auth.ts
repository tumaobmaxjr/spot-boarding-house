import { jwtDecode } from "jwt-decode";

export const TOKEN_KEY = "sbh_access_token";

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
  const decoded = decodeToken(token);
  return decoded.exp ? decoded.exp < Date.now() / 1000 : false;
}
