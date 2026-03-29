import { useQuery } from "@tanstack/react-query";
import { api } from "../../lib/api";
import type { RoleDto } from "./types";

async function fetchRoles() {
  const { data } = await api.get<RoleDto[]>("api/v1/roles");
  return data;
}

export function useRolesQuery() {
  return useQuery({
    queryKey: ["roles"],
    queryFn: fetchRoles,
  });
}
