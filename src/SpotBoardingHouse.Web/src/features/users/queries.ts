import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type { UserDto } from "./types";

async function fetchUsers() {
  const { data } = await api.get<UserDto[]>("api/v1/users");
  return data;
}

export function useUsersQuery() {
  return useQuery({
    queryKey: ["users"],
    queryFn: fetchUsers,
  });
}

export function useRolesQuery() {
  return useQuery({
    queryKey: ["roles"],
    queryFn: async () => {
      const { data } =
        await api.get<{ id: string; name: string }[]>("api/v1/roles");
      return data;
    },
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: {
      roleId: string;
      firstName: string;
      lastName: string;
      email: string;
      password: string;
    }) => {
      const { data } = await api.post("api/v1/users", payload);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      id,
      payload,
    }: {
      id: string;
      payload: {
        roleId?: string;
        firstName?: string;
        lastName?: string;
        email?: string;
        password?: string;
      };
    }) => {
      const { data } = await api.put(`api/v1/users/${id}`, payload);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}

export function useDeleteUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`api/v1/users/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}
