import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { AppRouter } from "./router";
import {
  AuthContext,
  type AuthContextType,
} from "../features/auth/auth-context.base";
import * as authLib from "../lib/auth";

function renderWithAuth(
  value: AuthContextType,
  initialEntries: string[] = ["/"],
) {
  render(
    <MemoryRouter initialEntries={initialEntries}>
      <AuthContext.Provider value={value}>
        <AppRouter />
      </AuthContext.Provider>
    </MemoryRouter>,
  );
}

describe("ProtectedAdminRoute", () => {
  it("logs out and redirects to login when token is expired", async () => {
    vi.spyOn(authLib, "getToken").mockReturnValue("expired-token");
    vi.spyOn(authLib, "isTokenExpired").mockReturnValue(true);

    const logout = vi.fn();
    const value: AuthContextType = {
      isAuthenticated: false,
      role: null,
      login: vi.fn(),
      register: vi.fn(),
      logout,
    };

    renderWithAuth(value, ["/admin/users"]);

    await waitFor(() => {
      expect(logout).toHaveBeenCalledWith(true);
    });
    expect(
      screen.getByRole("heading", {
        name: "Sign in",
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Session expired, please sign in again."),
    ).toBeInTheDocument();
  });
});

describe("ProtectedOwnerRoute", () => {
  it("redirects Owner to boarding houses on home route", () => {
    const value: AuthContextType = {
      isAuthenticated: true,
      role: "Owner",
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
    };

    renderWithAuth(value, ["/"]);

    expect(window.location.pathname).toBe("/boarding-houses");
  });

  it("redirects Admin to admin users on home route", () => {
    const value: AuthContextType = {
      isAuthenticated: true,
      role: "Admin",
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
    };

    renderWithAuth(value, ["/"]);

    expect(window.location.pathname).toBe("/admin/users");
  });
});
