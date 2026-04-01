import { beforeEach, describe, expect, it, vi } from "vitest";

const clearAuthSessionMock = vi.fn();

vi.mock("./auth", () => ({
  getToken: vi.fn(() => "token"),
  clearAuthSession: clearAuthSessionMock,
}));

describe("api response interceptor", () => {
  beforeEach(() => {
    clearAuthSessionMock.mockReset();
  });

  it("clears auth and redirects on 401 for protected requests", async () => {
    vi.resetModules();
    const assignMock = vi.fn();
    vi.stubGlobal("location", {
      ...window.location,
      assign: assignMock,
      pathname: "/admin/users",
    });
    const { api } = await import("./api");
    const rejected = (api.interceptors.response as unknown as {
      handlers: Array<{ rejected?: (error: unknown) => Promise<never> }>;
    }).handlers[0]?.rejected;

    const error = {
      response: { status: 401 },
      config: { url: "/api/v1/users" },
    };

    await expect(rejected?.(error)).rejects.toEqual(error);
    expect(clearAuthSessionMock).toHaveBeenCalledWith(true);
    expect(assignMock).toHaveBeenCalledWith("/login?reason=session-expired");
    vi.unstubAllGlobals();
  });

  it("does not force redirect on login 401", async () => {
    vi.resetModules();
    const assignMock = vi.fn();
    vi.stubGlobal("location", {
      ...window.location,
      assign: assignMock,
      pathname: "/admin/users",
    });
    const { api } = await import("./api");
    const rejected = (api.interceptors.response as unknown as {
      handlers: Array<{ rejected?: (error: unknown) => Promise<never> }>;
    }).handlers[0]?.rejected;

    const error = {
      response: { status: 401 },
      config: { url: "/api/v1/Auth/login" },
    };

    await expect(rejected?.(error)).rejects.toEqual(error);
    expect(clearAuthSessionMock).not.toHaveBeenCalled();
    expect(assignMock).not.toHaveBeenCalled();
    vi.unstubAllGlobals();
  });
});
