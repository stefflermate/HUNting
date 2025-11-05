import { apiClient } from "@/lib/apiClient";
import type { SocietyListItem } from "@/types";

type RawSociety = SocietyListItem & {
  members?: Array<unknown>;
};

function mapSociety(raw: RawSociety): SocietyListItem {
  return {
    societyId: raw.societyId,
    name: raw.name,
    email: raw.email,
    phone: raw.phone,
    description: raw.description,
    createdAt: raw.createdAt,
    memberCount: raw.memberCount ?? raw.members?.length ?? 0,
  };
}

async function list(): Promise<SocietyListItem[]> {
  const { data } = await apiClient.get<RawSociety[]>("/societies");
  return data.map(mapSociety);
}

async function getById(id: number): Promise<SocietyListItem> {
  const { data } = await apiClient.get<RawSociety>(`/societies/${id}`);
  return mapSociety(data);
}

export const societiesApi = {
  list,
  getById,
};
