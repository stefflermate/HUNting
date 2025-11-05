import { apiClient } from "@/lib/apiClient";
import type {
  MapIconTypeItem,
  MapMarkerItem,
  MapPathItem,
  MapPoint,
  MapPolygonItem,
  MapVisibilityOption,
} from "@/types";

type MapMarkerPayload = {
  iconTypeId: number;
  latitude: number;
  longitude: number;
  note?: string;
  visibilityCode?: string;
  societyId?: number;
};

type MapPolygonPayload = {
  name?: string;
  visibilityCode?: string;
  strokeColor?: string;
  fillColor?: string;
  societyId?: number;
  points: MapPoint[];
};

type MapPathPayload = {
  name?: string;
  visibilityCode?: string;
  strokeColor?: string;
  societyId?: number;
  points: MapPoint[];
};

type MapConfigResponse = {
  iconTypes: MapIconTypeItem[];
  visibilities: MapVisibilityOption[];
};

async function getConfig(): Promise<MapConfigResponse> {
  const { data } = await apiClient.get<MapConfigResponse>("/map/config");
  return data;
}

async function listMarkers(): Promise<MapMarkerItem[]> {
  const { data } = await apiClient.get<MapMarkerItem[]>("/map/markers");
  return data;
}

async function createMarker(payload: MapMarkerPayload): Promise<MapMarkerItem> {
  const { data } = await apiClient.post<MapMarkerItem>("/map/markers", payload);
  return data;
}

async function updateMarker(markerId: number, payload: MapMarkerPayload): Promise<MapMarkerItem> {
  const { data } = await apiClient.put<MapMarkerItem>(`/map/markers/${markerId}`, payload);
  return data;
}

async function deleteMarker(markerId: number): Promise<void> {
  await apiClient.delete(`/map/markers/${markerId}`);
}

async function listPolygons(): Promise<MapPolygonItem[]> {
  const { data } = await apiClient.get<MapPolygonItem[]>("/map/polygons");
  return data;
}

async function createPolygon(payload: MapPolygonPayload): Promise<MapPolygonItem> {
  const { data } = await apiClient.post<MapPolygonItem>("/map/polygons", payload);
  return data;
}

async function updatePolygon(polygonId: number, payload: MapPolygonPayload): Promise<MapPolygonItem> {
  const { data } = await apiClient.put<MapPolygonItem>(`/map/polygons/${polygonId}`, payload);
  return data;
}

async function deletePolygon(polygonId: number): Promise<void> {
  await apiClient.delete(`/map/polygons/${polygonId}`);
}

async function listPaths(): Promise<MapPathItem[]> {
  const { data } = await apiClient.get<MapPathItem[]>("/map/paths");
  return data;
}

async function createPath(payload: MapPathPayload): Promise<MapPathItem> {
  const { data } = await apiClient.post<MapPathItem>("/map/paths", payload);
  return data;
}

async function updatePath(pathId: number, payload: MapPathPayload): Promise<MapPathItem> {
  const { data } = await apiClient.put<MapPathItem>(`/map/paths/${pathId}`, payload);
  return data;
}

async function deletePath(pathId: number): Promise<void> {
  await apiClient.delete(`/map/paths/${pathId}`);
}

export const mapApi = {
  getConfig,
  listMarkers,
  createMarker,
  updateMarker,
  deleteMarker,
  listPolygons,
  createPolygon,
  updatePolygon,
  deletePolygon,
  listPaths,
  createPath,
  updatePath,
  deletePath,
};

