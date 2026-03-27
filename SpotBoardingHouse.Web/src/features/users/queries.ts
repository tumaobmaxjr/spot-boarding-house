import { useQuery } from "@tanstack/react-query";
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
