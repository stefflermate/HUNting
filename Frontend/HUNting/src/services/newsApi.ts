import { apiClient } from "@/lib/apiClient";
import type { NewsFeedItem } from "@/types";

type PaginatedResponse<T> = {
  items: T[];
  total: number;
};

type ListParams = {
  page?: number;
  take?: number;
  societyId?: number;
};

async function list(
  params: ListParams = {}
): Promise<PaginatedResponse<NewsFeedItem>> {
  const { data } = await apiClient.get<PaginatedResponse<NewsFeedItem> | NewsFeedItem[]>(
    "/news",
    {
      params,
    }
  );

  if (Array.isArray(data)) {
    return { items: data, total: data.length };
  }

  return data;
}

async function getById(id: number): Promise<NewsFeedItem> {
  const { data } = await apiClient.get<NewsFeedItem>(`/news/${id}`);
  return data;
}

export const newsApi = {
  list,
  getById,
};
