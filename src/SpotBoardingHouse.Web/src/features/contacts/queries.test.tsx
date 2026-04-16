import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  useContactsByBoardingHouseQuery,
  useCreateContact,
  useDeleteContact,
} from "./queries";

const { apiMock } = vi.hoisted(() => ({
  apiMock: {
    get: vi.fn(),
    post: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock("../../lib/api", () => ({
  api: apiMock,
}));

function createWrapper(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: React.ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  };
}

describe("contacts queries", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("loads contacts by boarding house id", async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    apiMock.get.mockResolvedValueOnce({
      data: [{ id: "c1", boardingHouseId: "bh1", phoneNumber: "0917" }],
    });

    const { result } = renderHook(() => useContactsByBoardingHouseQuery("bh1"), {
      wrapper: createWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(apiMock.get).toHaveBeenCalledWith("api/v1/contacts/boarding-house/bh1");
  });

  it("creates contact and invalidates related query keys", async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    const invalidateSpy = vi.spyOn(queryClient, "invalidateQueries");
    apiMock.post.mockResolvedValueOnce({
      data: {
        id: "c1",
        boardingHouseId: "bh1",
        phoneNumber: "0917",
        network: "globe",
        isPrimary: true,
        createdAt: new Date().toISOString(),
        updatedAt: null,
      },
    });

    const { result } = renderHook(() => useCreateContact(), {
      wrapper: createWrapper(queryClient),
    });

    await result.current.mutateAsync({
      boardingHouseId: "bh1",
      phoneNumber: "0917",
      network: "globe",
      isPrimary: true,
    });

    expect(apiMock.post).toHaveBeenCalledWith("api/v1/contacts", {
      boardingHouseId: "bh1",
      phoneNumber: "0917",
      network: "globe",
      isPrimary: true,
    });
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ["contacts", "bh1"] });
    expect(invalidateSpy).toHaveBeenCalledWith({
      queryKey: ["contacts", "detail", "c1"],
    });
  });

  it("deletes contact and removes detail cache key", async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    const removeSpy = vi.spyOn(queryClient, "removeQueries");
    apiMock.delete.mockResolvedValueOnce({});

    const { result } = renderHook(() => useDeleteContact(), {
      wrapper: createWrapper(queryClient),
    });

    await result.current.mutateAsync({ id: "c1", boardingHouseId: "bh1" });

    expect(apiMock.delete).toHaveBeenCalledWith("api/v1/contacts/c1");
    expect(removeSpy).toHaveBeenCalledWith({ queryKey: ["contacts", "detail", "c1"] });
  });
});
