import { apiClient } from "@/lib/apiClient";
import type {
  MapIconTypeItem,
  MapMarkerItem,
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
  description?: string;
  visibilityCode?: string;
  strokeColor?: string;
  fillColor?: string;
  societyId?: number;
  points: MapPoint[];
};

type MapConfigResponse = {
  iconTypes: MapIconTypeItem[];
  visibilities: MapVisibilityOption[];
};

export type MapLayerParams =
  | { scope: "personal" }
  | { scope: "society"; societyId: number };

function buildLayerQuery(params: MapLayerParams) {
  const search = new URLSearchParams();
  search.set("scope", params.scope);
  if (params.scope === "society") {
    search.set("societyId", String(params.societyId));
  }
  const query = search.toString();
  return query ? `?${query}` : "";
}

async function getConfig(): Promise<MapConfigResponse> {
  const { data } = await apiClient.get<MapConfigResponse>("/map/config");
  return data;
}

async function listMarkers(params: MapLayerParams): Promise<MapMarkerItem[]> {
  const { data } = await apiClient.get<MapMarkerItem[]>(`/map/markers${buildLayerQuery(params)}`);
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

async function listPolygons(params: MapLayerParams): Promise<MapPolygonItem[]> {
  const { data } = await apiClient.get<MapPolygonItem[]>(`/map/polygons${buildLayerQuery(params)}`);
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
};
