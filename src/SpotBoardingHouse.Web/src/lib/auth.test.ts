import { describe, expect, it, vi } from "vitest";
import {
  TOKEN_EXPIRY_SKEW_SECONDS,
  clearAuthSession,
  isTokenExpired,
} from "./auth";

function createToken(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: "none", typ: "JWT" }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.sig`;
}

describe("auth utils", () => {
  it("treats token with missing exp as expired", () => {
    const token = createToken({ sub: "123" });
    expect(isTokenExpired(token)).toBe(true);
  });

  it("treats invalid token as expired", () => {
    expect(isTokenExpired("not-a-jwt")).toBe(true);
  });

  it("applies expiry skew buffer", () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date("2026-03-29T00:00:00.000Z"));

    const nowSeconds = Math.floor(Date.now() / 1000);
    const tokenInsideBuffer = createToken({
      exp: nowSeconds + TOKEN_EXPIRY_SKEW_SECONDS - 1,
    });
    const tokenOutsideBuffer = createToken({
      exp: nowSeconds + TOKEN_EXPIRY_SKEW_SECONDS + 120,
    });

    expect(isTokenExpired(tokenInsideBuffer)).toBe(true);
    expect(isTokenExpired(tokenOutsideBuffer)).toBe(false);

    vi.useRealTimers();
  });

  it("marks session as expired when clearing with markExpired", () => {
    localStorage.setItem("sbh_access_token", "abc");
    clearAuthSession(true);

    expect(localStorage.getItem("sbh_access_token")).toBeNull();
    expect(sessionStorage.getItem("sbh_session_expired")).toBe("1");
  });
});
