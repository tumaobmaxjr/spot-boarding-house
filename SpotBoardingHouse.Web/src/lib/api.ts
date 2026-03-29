import axios from "axios";
import { clearAuthSession, getToken } from "./auth";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;
    const url = (error?.config?.url as string | undefined)?.toLowerCase() ?? "";
    const isLoginRequest = url.includes("/api/v1/auth/login");

    if (status === 401 && !isLoginRequest) {
      clearAuthSession(true);
      if (!window.location.pathname.startsWith("/login")) {
        window.location.assign("/login?reason=session-expired");
      }
    }

    return Promise.reject(error);
  },
);
