import { apiClient } from "@/lib/apiClient";
import type { HuntingLogItem, HuntingLogPayload } from "@/types";

async function list(): Promise<HuntingLogItem[]> {
  const { data } = await apiClient.get<HuntingLogItem[]>("/huntinglogs");
  return data;
}

type CreatePayload = HuntingLogPayload & {
  imageFile?: File | null;
};

async function create(payload: CreatePayload): Promise<HuntingLogItem> {
  const formData = new FormData();
  formData.append("OccurredAt", payload.occurredAt);
  if (payload.locationText) formData.append("LocationText", payload.locationText);
  if (typeof payload.latitude === "number")
    formData.append("Latitude", String(payload.latitude));
  if (typeof payload.longitude === "number")
    formData.append("Longitude", String(payload.longitude));
  if (payload.animalType) formData.append("AnimalType", payload.animalType);
  formData.append("ShotFired", String(payload.shotFired));
  if (payload.notes) formData.append("Notes", payload.notes);
  formData.append("ShareWithSociety", String(payload.shareWithSociety));
  if (payload.imageFile) {
    formData.append("Image", payload.imageFile);
  }

  const { data } = await apiClient.post<HuntingLogItem>("/huntinglogs", formData);
  return data;
}

export const huntingLogsApi = {
  list,
  create,
};
