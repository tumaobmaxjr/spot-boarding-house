import { api } from "../../lib/api";

export function resolveMediaUrl(url: string): string {
  if (!url) return url;
  if (/^https?:\/\//i.test(url)) return url;

  const base = api.defaults.baseURL ?? window.location.origin;
  return new URL(url, base).toString();
}
