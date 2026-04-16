import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type {
  ContactDto,
  CreateContactPayload,
  UpdateContactPayload,
} from "./types";

export function useContactsByBoardingHouseQuery(boardingHouseId?: string) {
  return useQuery({
    queryKey: ["contacts", boardingHouseId],
    enabled: !!boardingHouseId,
    queryFn: async () => {
      const { data } = await api.get<ContactDto[]>(
        `api/v1/contacts/boarding-house/${boardingHouseId}`,
      );
      return data;
    },
  });
}

export function useContactByIdQuery(id?: string) {
  return useQuery({
    queryKey: ["contacts", "detail", id],
    enabled: !!id,
    queryFn: async () => {
      const { data } = await api.get<ContactDto>(`api/v1/contacts/${id}`);
      return data;
    },
  });
}

export function useCreateContact() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: CreateContactPayload) => {
      const { data } = await api.post<ContactDto>("api/v1/contacts", payload);
      return data;
    },
    onSuccess: (created) => {
      queryClient.invalidateQueries({
        queryKey: ["contacts", created.boardingHouseId],
      });
      queryClient.invalidateQueries({
        queryKey: ["contacts", "detail", created.id],
      });
    },
  });
}

export function useUpdateContact() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      payload,
    }: {
      id: string;
      payload: UpdateContactPayload;
    }) => {
      const { data } = await api.put<ContactDto>(`api/v1/contacts/${id}`, payload);
      return data;
    },
    onSuccess: (updated) => {
      queryClient.invalidateQueries({
        queryKey: ["contacts", updated.boardingHouseId],
      });
      queryClient.invalidateQueries({
        queryKey: ["contacts", "detail", updated.id],
      });
    },
  });
}

export function useDeleteContact() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      id,
      boardingHouseId,
    }: {
      id: string;
      boardingHouseId: string;
    }) => {
      await api.delete(`api/v1/contacts/${id}`);
      return { id, boardingHouseId };
    },
    onSuccess: ({ id, boardingHouseId }) => {
      queryClient.invalidateQueries({ queryKey: ["contacts", boardingHouseId] });
      queryClient.removeQueries({ queryKey: ["contacts", "detail", id] });
    },
  });
}
