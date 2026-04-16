import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type { CreateMediaPayload, MediaDto, UpdateMediaPayload } from "./types";

export function useMediaByBoardingHouseQuery(boardingHouseId?: string) {
  return useQuery({
    queryKey: ["media", "boarding-house", boardingHouseId],
    enabled: !!boardingHouseId,
    queryFn: async () => {
      const { data } = await api.get<MediaDto[]>(
        `api/v1/media/boarding-house/${boardingHouseId}`,
      );
      return data;
    },
  });
}

export function useMediaByIdQuery(id?: string) {
  return useQuery({
    queryKey: ["media", "detail", id],
    enabled: !!id,
    queryFn: async () => {
      const { data } = await api.get<MediaDto>(`api/v1/media/${id}`);
      return data;
    },
  });
}

export function useCreateMedia() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: CreateMediaPayload) => {
      const { data } = await api.post<MediaDto>("api/v1/media", payload);
      return data;
    },
    onSuccess: (created) => {
      if (created.boardingHouseId) {
        queryClient.invalidateQueries({
          queryKey: ["media", "boarding-house", created.boardingHouseId],
        });
      }

      queryClient.invalidateQueries({
        queryKey: ["media", "detail", created.id],
      });
    },
  });
}

export function useUpdateMedia() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      payload,
    }: {
      id: string;
      payload: UpdateMediaPayload;
    }) => {
      const { data } = await api.put<MediaDto>(`api/v1/media/${id}`, payload);
      return data;
    },
    onSuccess: (updated) => {
      if (updated.boardingHouseId) {
        queryClient.invalidateQueries({
          queryKey: ["media", "boarding-house", updated.boardingHouseId],
        });
      }

      queryClient.invalidateQueries({
        queryKey: ["media", "detail", updated.id],
      });
    },
  });
}

export function useDeleteMedia() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      boardingHouseId,
    }: {
      id: string;
      boardingHouseId?: string;
    }) => {
      await api.delete(`api/v1/media/${id}`);
      return { id, boardingHouseId };
    },
    onSuccess: ({ id, boardingHouseId }) => {
      if (boardingHouseId) {
        queryClient.invalidateQueries({
          queryKey: ["media", "boarding-house", boardingHouseId],
        });
      }

      queryClient.removeQueries({ queryKey: ["media", "detail", id] });
    },
  });
}
