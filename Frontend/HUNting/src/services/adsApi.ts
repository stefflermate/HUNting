import { apiClient } from "@/lib/apiClient";
import type {
  AdListItem,
  AdCategoryItem,
  AdStatusItem,
  AdPayload,
  AdImage,
  PaginatedResult,
} from "@/types";

type ListParams = {
  page?: number;
  take?: number;
  categoryId?: number;
  statusCode?: string;
  search?: string;
  minPrice?: number;
  maxPrice?: number;
};

async function list(
  params: ListParams = {}
): Promise<PaginatedResult<AdListItem>> {
  const { data } = await apiClient.get<PaginatedResult<AdListItem> | AdListItem[]>(
    "/ads",
    {
      params,
    }
  );

  if (Array.isArray(data)) {
    return { items: data, total: data.length, page: 1, pageSize: data.length };
  }

  return data;
}

async function getById(adId: number): Promise<AdListItem> {
  const { data } = await apiClient.get<AdListItem>(`/ads/${adId}`);
  return data;
}

async function categories(): Promise<AdCategoryItem[]> {
  const { data } = await apiClient.get<AdCategoryItem[]>("/ads/categories");
  return data;
}

async function statuses(): Promise<AdStatusItem[]> {
  const { data } = await apiClient.get<AdStatusItem[]>("/ads/statuses");
  return data;
}

export const adsApi = {
  list,
  getById,
  categories,
  statuses,
  create,
  update,
  remove,
  uploadImage,
  deleteImage,
};

async function create(payload: AdPayload): Promise<AdListItem> {
  const { data } = await apiClient.post<AdListItem>("/ads", payload);
  return data;
}

async function update(adId: number, payload: AdPayload): Promise<void> {
  const body = { adId, ...payload };
  await apiClient.put(`/ads/${adId}`, body);
}

async function remove(adId: number): Promise<void> {
  await apiClient.delete(`/ads/${adId}`);
}

async function uploadImage(adId: number, file: File): Promise<AdImage> {
  const formData = new FormData();
  formData.append("file", file);

  const { data } = await apiClient.post<AdImage>(`/ads/${adId}/images`, formData);

  return data;
}

async function deleteImage(adId: number, imageId: number): Promise<void> {
  await apiClient.delete(`/ads/${adId}/images/${imageId}`);
}
