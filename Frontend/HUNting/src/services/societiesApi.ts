import { apiClient } from "@/lib/apiClient";
import type {
  SocietyListItem,
  SocietyMemberItem,
  SocietyMembershipRequest,
  SocietyPost,
  MembershipRequestStatus,
} from "@/types";

type RawSociety = SocietyListItem & {
  members?: Array<unknown>;
};

type RawSocietyMember = {
  societyId: number;
  userId: number;
  roleId: number;
  joinedAt: string;
  user?: any;
  role?: { name: string };
};

type RawMembershipRequest = {
  societyId: number;
  userId: number;
  message?: string;
  status: MembershipRequestStatus;
  createdAt: string;
  processedAt?: string;
  user?: { id: number; fullName?: string; name?: string; email?: string };
};
type RawPost = SocietyPost & {
  author?: { id: number; fullName?: string; name?: string };
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

function mapMember(raw: RawSocietyMember): SocietyMemberItem {
  const mappedUser = raw.user
    ? {
        id: raw.user.id,
        fullName: raw.user.fullName ?? raw.user.name ?? "",
        email: raw.user.email,
        phone: raw.user.phone,
      }
    : undefined;

  return {
    societyId: raw.societyId,
    userId: raw.userId,
    roleId: raw.roleId,
    joinedAt: raw.joinedAt,
    roleName: raw.role?.name,
    user: mappedUser,
  };
}

function mapRequest(raw: RawMembershipRequest): SocietyMembershipRequest {
  return {
    societyId: raw.societyId,
    userId: raw.userId,
    status: raw.status,
    message: raw.message,
    createdAt: raw.createdAt,
    processedAt: raw.processedAt,
    user: raw.user
      ? {
          id: raw.user.id,
          fullName: raw.user.fullName ?? raw.user.name ?? "",
          email: raw.user.email,
        }
      : undefined,
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

async function my(): Promise<SocietyListItem[]> {
  const { data } = await apiClient.get<RawSociety[]>("/societies/my");
  return data.map(mapSociety);
}

async function join(societyId: number, message?: string): Promise<void> {
  await apiClient.post(`/societies/${societyId}/join`, {
    message,
  });
}

async function members(societyId: number): Promise<SocietyMemberItem[]> {
  const { data } = await apiClient.get<RawSocietyMember[]>(`/societies/${societyId}/members`);
  return data.map(mapMember);
}

async function requests(societyId: number): Promise<SocietyMembershipRequest[]> {
  const { data } = await apiClient.get<RawMembershipRequest[]>(`/societies/${societyId}/requests`);
  return data.map(mapRequest);
}

async function approveRequest(societyId: number, userId: number): Promise<void> {
  await apiClient.post(`/societies/${societyId}/requests/${userId}/approve`);
}

async function rejectRequest(societyId: number, userId: number): Promise<void> {
  await apiClient.post(`/societies/${societyId}/requests/${userId}/reject`);
}

function mapPost(raw: RawPost): SocietyPost {
  return {
    societyPostId: raw.societyPostId,
    societyId: raw.societyId,
    authorUserId: raw.authorUserId,
    title: raw.title,
    content: raw.content,
    imageUrl: raw.imageUrl,
    createdAt: raw.createdAt,
    author: raw.author
      ? {
          id: raw.author.id,
          fullName: raw.author.fullName ?? raw.author.name ?? "",
        }
      : undefined,
  };
}

async function posts(societyId: number): Promise<SocietyPost[]> {
  const { data } = await apiClient.get<RawPost[]>(`/societies/${societyId}/posts`);
  return data.map(mapPost);
}

async function createPost(
  societyId: number,
  payload: { title?: string; content?: string; image?: File }
): Promise<SocietyPost> {
  const formData = new FormData();
  if (payload.title) formData.append("Title", payload.title);
  if (payload.content) formData.append("Content", payload.content);
  if (payload.image) formData.append("Image", payload.image);

  const { data } = await apiClient.post<RawPost>(`/societies/${societyId}/posts`, formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });
  return mapPost(data);
}

async function deletePost(societyId: number, postId: number): Promise<void> {
  await apiClient.delete(`/societies/${societyId}/posts/${postId}`);
}

export const societiesApi = {
  list,
  getById,
  my,
  join,
  members,
  requests,
  approveRequest,
  rejectRequest,
  posts,
  createPost,
  deletePost,
};
