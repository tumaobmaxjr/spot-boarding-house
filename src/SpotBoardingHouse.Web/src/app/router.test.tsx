import { describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { AppRouter } from "./router";
import {
  AuthContext,
  type AuthContextType,
} from "../features/auth/auth-context.base";
import * as authLib from "../lib/auth";

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

    render(
      <MemoryRouter initialEntries={["/admin/users"]}>
        <AuthContext.Provider value={value}>
          <AppRouter />
        </AuthContext.Provider>
      </MemoryRouter>,
    );

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
