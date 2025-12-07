export type UserRole =
  | "Administrator"
  | "SocietyManager"
  | "Hunter"
  | "Guest";

export type AccountType = "Person" | "Society";

export interface OwnedSocietySummary {
  societyId: number;
  name: string;
}

export interface AuthUser {
  id: number;
  name: string;
  email: string;
  phone?: string;
  roles: UserRole[];
  accountType: AccountType;
  ownedSociety?: OwnedSocietySummary | null;
}

export interface AdminUserItem {
  id: number;
  name: string;
  email: string;
  phone?: string;
  roles: string[];
  createdAt: string;
}

export interface AdImage {
  imageId: number;
  imageUrl: string;
  sortOrder: number;
}

export interface AdListItem {
  adId: number;
  title: string;
  description?: string;
  price?: number;
  locationText?: string;
  statusCode: string;
  createdAt: string;
  contactEmail?: string;
  contactPhone?: string;
  ownerSocietyId?: number;
  ownerUserId?: number;
  category?: {
    categoryId: number;
    name: string;
  };
  images?: AdImage[];
}

export interface AdPayload {
  title: string;
  description?: string;
  categoryId?: number;
  price?: number;
  locationText?: string;
  latitude?: number;
  longitude?: number;
  statusCode?: string;
  contactEmail?: string;
  contactPhone?: string;
  ownerUserId?: number;
  ownerSocietyId?: number;
  images?: AdImage[];
}

export interface AdCategoryItem {
  categoryId: number;
  name: string;
}

export interface AdStatusItem {
  statusCode: string;
  description?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface SocietyListItem {
  societyId: number;
  name: string;
  email: string;
  phone?: string;
  description?: string;
  createdAt: string;
  memberCount: number;
}

export interface SocietyMemberItem {
  societyId: number;
  userId: number;
  roleId: number;
  joinedAt: string;
  roleName?: string;
  user?: {
    id: number;
    fullName: string;
    email?: string;
    phone?: string;
  };
}

export type MembershipRequestStatus = "Pending" | "Approved" | "Rejected";

export interface SocietyMembershipRequest {
  societyId: number;
  userId: number;
  message?: string;
  status: MembershipRequestStatus;
  createdAt: string;
  processedAt?: string;
  user?: {
    id: number;
    fullName: string;
    email?: string;
  };
}

export interface SocietyPost {
  societyPostId: number;
  societyId: number;
  authorUserId: number;
  title?: string;
  content?: string;
  imageUrl?: string;
  createdAt: string;
  author?: {
    id: number;
    fullName: string;
  };
}

export interface NewsFeedItem {
  newsFeedId: number;
  title?: string;
  content?: string;
  imageUrl?: string;
  visibilityCode: string;
  createdAt: string;
  author?: {
    id: number;
    name: string;
  };
  society?: {
    societyId: number;
    name: string;
  };
}

export interface HuntingLogItem {
  huntingLogId: number;
  occurredAt: string;
  locationText?: string;
  animalType?: string;
  shotFired: boolean;
  notes?: string;
  imageUrl?: string;
  shareWithSociety: boolean;
}

export interface HuntingLogPayload {
  occurredAt: string;
  locationText?: string;
  latitude?: number;
  longitude?: number;
  animalType?: string;
  shotFired: boolean;
  notes?: string;
  imageUrl?: string;
  shareWithSociety: boolean;
  imageFile?: File | null;
}

export interface MapIconTypeItem {
  iconTypeId: number;
  name: string;
  defaultColor?: string;
  glyph?: string;
}

export interface MapVisibilityOption {
  visibilityCode: string;
  description?: string;
}

export interface MapMarkerItem {
  markerId: number;
  iconTypeId: number;
  latitude: number;
  longitude: number;
  note?: string;
  visibilityCode: string;
  societyId?: number;
}

export interface MapPoint {
  seq?: number;
  latitude: number;
  longitude: number;
}

export interface MapPolygonItem {
  polygonId: number;
  name?: string;
  visibilityCode: string;
  strokeColor?: string;
  fillColor?: string;
  societyId?: number;
  points: MapPoint[];
}

export interface MapPathItem {
  pathId: number;
  name?: string;
  visibilityCode: string;
  strokeColor?: string;
  societyId?: number;
  points: MapPoint[];
}
