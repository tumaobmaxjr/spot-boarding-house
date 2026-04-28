import { useQuery } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type { PublicBoardingHouseDetailDto, PublicBoardingHouseDto } from "./types";

export function usePublicBoardingHousesQuery() {
  return useQuery({
    queryKey: ["public", "boarding-houses"],
    queryFn: async () => {
      const { data } = await api.get<PublicBoardingHouseDto[]>(
        "api/v1/public/boarding-houses",
      );
      return data;
    },
  });
}

export function usePublicBoardingHouseDetailQuery(id?: string) {
  return useQuery({
    queryKey: ["public", "boarding-houses", id],
    enabled: !!id,
    queryFn: async () => {
      const { data } = await api.get<PublicBoardingHouseDetailDto>(
        `api/v1/public/boarding-houses/${id}`,
      );
      return data;
    },
  });
}
