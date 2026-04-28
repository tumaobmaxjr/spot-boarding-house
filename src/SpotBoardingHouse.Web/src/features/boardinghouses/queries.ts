import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type { BoardingHouseDto } from "./types";

async function fetchBoardingHouses(ownerId?: string) {
  const url = ownerId
    ? `api/v1/boardinghouses?ownerId=${ownerId}`
    : "api/v1/boardinghouses";
  const { data } = await api.get<BoardingHouseDto[]>(url);
  return data;
}

export function useBoardingHousesQuery(ownerId?: string) {
  return useQuery({
    queryKey: ["boardinghouses", ownerId],
    queryFn: () => fetchBoardingHouses(ownerId),
  });
}

export function useCreateBoardingHouse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: {
      name: string;
      description?: string;
      allowedGender: string;
      address: string;
      latitude: number;
      longitude: number;
      facebookLink?: string;
      ownerId?: string;
    }) => {
      const { ownerId, ...createData } = payload;
      const params = ownerId ? `?ownerId=${ownerId}` : "";
      const { data } = await api.post<BoardingHouseDto>(
        `api/v1/boardinghouses${params}`,
        createData,
      );
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boardinghouses"] });
    },
  });
}

export function useUpdateBoardingHouse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      id,
      payload,
    }: {
      id: string;
      payload: {
        name?: string;
        description?: string;
        allowedGender?: string;
        address?: string;
        latitude?: number;
        longitude?: number;
        facebookLink?: string;
      };
    }) => {
      const { data } = await api.put<BoardingHouseDto>(
        `api/v1/boardinghouses/${id}`,
        payload,
      );
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boardinghouses"] });
    },
  });
}

export function useDeleteBoardingHouse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`api/v1/boardinghouses/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boardinghouses"] });
    },
  });
}

export function useOwnersQuery() {
  return useQuery({
    queryKey: ["owners"],
    queryFn: async () => {
      const { data } = await api.get<
        {
          id: string;
          firstName: string;
          lastName: string;
          email: string;
          roleName: string;
        }[]
      >("/api/v1/users");
      return data
        .filter((u) => u.roleName === "Owner")
        .map((u) => ({
          id: u.id,
          firstName: u.firstName,
          lastName: u.lastName,
          email: u.email,
        }));
    },
  });
}
