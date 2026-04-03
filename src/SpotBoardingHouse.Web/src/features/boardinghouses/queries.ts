import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type { BoardingHouseDto } from "./types";

async function fetchBoardingHouses() {
  const { data } = await api.get<BoardingHouseDto[]>("api/v1/boardinghouses");
  return data;
}

export function useBoardingHousesQuery() {
  return useQuery({
    queryKey: ["boardinghouses"],
    queryFn: fetchBoardingHouses,
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
    }) => {
      const { data } = await api.post("api/v1/boardinghouses", payload);
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
      const { data } = await api.put(`api/v1/boardinghouses/${id}`, payload);
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
