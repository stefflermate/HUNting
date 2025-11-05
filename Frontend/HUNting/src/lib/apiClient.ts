import axios from "axios";
import { getAccessToken } from "./authStorage";

const baseURL =
  import.meta.env.VITE_API_BASE_URL ??
  (import.meta.env.DEV ? "https://localhost:5001/api" : "/api");

export const apiClient = axios.create({
  baseURL,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const fallbackMessage = "Request failed";
    if (error.response?.data?.message) {
      return Promise.reject(new Error(error.response.data.message));
    }
    if (error.response?.data?.title) {
      return Promise.reject(new Error(error.response.data.title));
    }
    if (typeof error.message === "string") {
      return Promise.reject(new Error(error.message));
    }
    return Promise.reject(new Error(fallbackMessage));
  }
);

const assetBase =
  typeof baseURL === "string" && baseURL.startsWith("http")
    ? baseURL.replace(/\/api\/?$/, "")
    : typeof window !== "undefined"
    ? window.location.origin.replace(/\/$/, "")
    : "";

export function resolveAssetUrl(path?: string | null) {
  if (!path) return path ?? undefined;
  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const normalized = path.startsWith("/") ? path : `/${path}`;
  return `${assetBase}${normalized}`;
}
