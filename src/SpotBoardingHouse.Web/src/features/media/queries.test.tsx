import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  useCreateMedia,
  useDeleteMedia,
  useMediaByBoardingHouseQuery,
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

describe("media queries", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("loads media by boarding house id", async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    apiMock.get.mockResolvedValueOnce({
      data: [{ id: "m1", boardingHouseId: "bh1", url: "https://example.com/1.jpg" }],
    });

    const { result } = renderHook(() => useMediaByBoardingHouseQuery("bh1"), {
      wrapper: createWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(apiMock.get).toHaveBeenCalledWith("api/v1/media/boarding-house/bh1");
  });

  it("creates media and invalidates boarding house media key", async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    const invalidateSpy = vi.spyOn(queryClient, "invalidateQueries");
    apiMock.post.mockResolvedValueOnce({
      data: {
        id: "m1",
        boardingHouseId: "bh1",
        roomId: null,
        url: "https://example.com/1.jpg",
        isPrimary: true,
        displayOrder: 1,
        createdAt: new Date().toISOString(),
        updatedAt: null,
      },
    });

    const { result } = renderHook(() => useCreateMedia(), {
      wrapper: createWrapper(queryClient),
    });

    await result.current.mutateAsync({
      boardingHouseId: "bh1",
      url: "https://example.com/1.jpg",
      isPrimary: true,
      displayOrder: 1,
    });

    expect(apiMock.post).toHaveBeenCalledWith("api/v1/media", {
      boardingHouseId: "bh1",
      url: "https://example.com/1.jpg",
      isPrimary: true,
      displayOrder: 1,
    });
    expect(invalidateSpy).toHaveBeenCalledWith({
      queryKey: ["media", "boarding-house", "bh1"],
    });
    expect(invalidateSpy).toHaveBeenCalledWith({
      queryKey: ["media", "detail", "m1"],
    });
  });

  it("deletes media and removes detail cache key", async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    const removeSpy = vi.spyOn(queryClient, "removeQueries");
    apiMock.delete.mockResolvedValueOnce({});

    const { result } = renderHook(() => useDeleteMedia(), {
      wrapper: createWrapper(queryClient),
    });

    await result.current.mutateAsync({ id: "m1", boardingHouseId: "bh1" });

    expect(apiMock.delete).toHaveBeenCalledWith("api/v1/media/m1");
    expect(removeSpy).toHaveBeenCalledWith({ queryKey: ["media", "detail", "m1"] });
  });
});
