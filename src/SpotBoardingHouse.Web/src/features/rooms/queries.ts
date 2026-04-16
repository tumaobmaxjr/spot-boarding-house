import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type { CreateRoomPayload, RoomDto, UpdateRoomPayload } from "./types";

async function fetchRooms() {
  const { data } = await api.get<RoomDto[]>("api/v1/rooms");
  return data;
}

export function useRoomsQuery() {
  return useQuery({
    queryKey: ["rooms"],
    queryFn: fetchRooms,
  });
}

export function useCreateRoom() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: CreateRoomPayload) => {
      const { data } = await api.post<RoomDto>("api/v1/rooms", payload);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
}

export function useUpdateRoom() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      payload,
    }: {
      id: string;
      payload: UpdateRoomPayload;
    }) => {
      const { data } = await api.put<RoomDto>(`api/v1/rooms/${id}`, payload);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
}

export function useDeleteRoom() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`api/v1/rooms/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
}
