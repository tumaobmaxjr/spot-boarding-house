import { createContext } from "react";
import type { LoginRequest } from "./types";

export type AuthContextType = {
  isAuthenticated: boolean;
  role: string | null;
  login: (payload: LoginRequest) => Promise<void>;
  logout: () => void;
};

export const AuthContext = createContext<AuthContextType | null>(null);
