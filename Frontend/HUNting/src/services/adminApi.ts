import { apiClient } from "@/lib/apiClient";
import type { AdminUserItem } from "@/types";

type RawAdminUser = {
  id: number;
  fullName: string;
  email: string;
  phone?: string;
  roles: string[];
  createdAt: string;
};

function mapUser(raw: RawAdminUser): AdminUserItem {
  return {
    id: raw.id,
    name: raw.fullName,
    email: raw.email,
    phone: raw.phone,
    roles: raw.roles,
    createdAt: raw.createdAt,
  };
}

async function listUsers(): Promise<AdminUserItem[]> {
  const { data } = await apiClient.get<RawAdminUser[]>("/users");
  return data.map(mapUser);
}

async function deleteUser(userId: number): Promise<void> {
  await apiClient.delete(`/users/${userId}`);
}

export const adminApi = {
  listUsers,
  deleteUser,
};
