import { useCallback, useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  GoogleMap,
  MarkerF,
  PolygonF,
  PolylineF,
  InfoWindow,
  useLoadScript,
  DrawingManager,
} from "@react-google-maps/api";
import { mapApi } from "@/services/mapApi";
import type { MapPoint } from "@/types";
import { Button } from "@/components/ui/button";
import { Loader2, MapPin, PencilRuler, Shapes, Trash2 } from "lucide-react";
import { useAuth } from "@/context/AuthContext";

type DraftMode = "view" | "marker" | "polygon" | "path";

type MarkerDraft = {
  latitude: number;
  longitude: number;
  iconTypeId: number | null;
  note: string;
  visibilityCode: string;
};

type PolygonDraft = {
  points: MapPoint[];
  name: string;
  visibilityCode: string;
  strokeColor: string;
  fillColor: string;
};

type PathDraft = {
  points: MapPoint[];
  name: string;
  visibilityCode: string;
  strokeColor: string;
};

const DEFAULT_CENTER = { lat: 47.1625, lng: 19.5033 };
const DEFAULT_ZOOM = 7;

const toNumber = (value: unknown): number => {
  if (typeof value === "number") {
    return value;
  }
  if (typeof value === "string") {
    const normalized = value.replace(",", ".");
    const parsed = Number(normalized);
    return parsed;
  }
  return NaN;
};

const defaultPolygonDraft = (): PolygonDraft => ({
  points: [],
  name: "",
  visibilityCode: "PUBLIC",
  strokeColor: "#22c55e",
  fillColor: "#22c55e55",
});

const defaultPathDraft = (): PathDraft => ({
  points: [],
  name: "",
  visibilityCode: "PUBLIC",
  strokeColor: "#3b82f6",
});

export function MapPage() {
  const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;
  const { isAuthenticated } = useAuth();
  const queryClient = useQueryClient();

  const [mode, setMode] = useState<DraftMode>("view");
  const [markerDraft, setMarkerDraft] = useState<MarkerDraft | null>(null);
  const [polygonDraft, setPolygonDraft] = useState<PolygonDraft | null>(null);
  const [pathDraft, setPathDraft] = useState<PathDraft | null>(null);
  const [mapInstance, setMapInstance] = useState<google.maps.Map | null>(null);
  const [selectedMarkerId, setSelectedMarkerId] = useState<number | null>(null);
  const [selectedPolygon, setSelectedPolygon] = useState<{
    polygonId: number;
    position: google.maps.LatLngLiteral;
  } | null>(null);
  const [selectedPath, setSelectedPath] = useState<{
    pathId: number;
    position: google.maps.LatLngLiteral;
  } | null>(null);

  const { isLoaded, loadError } = useLoadScript({
    googleMapsApiKey: apiKey ?? "",
    libraries: ["drawing"],
    language: "hu",
    region: "HU",
  });

  const { data: config } = useQuery({
    queryKey: ["map", "config"],
    queryFn: mapApi.getConfig,
    enabled: Boolean(apiKey),
  });

  const { data: markers } = useQuery({
    queryKey: ["map", "markers"],
    queryFn: mapApi.listMarkers,
    enabled: Boolean(apiKey),
  });

  const { data: polygons } = useQuery({
    queryKey: ["map", "polygons"],
    queryFn: mapApi.listPolygons,
    enabled: Boolean(apiKey),
  });

  const { data: paths } = useQuery({
    queryKey: ["map", "paths"],
    queryFn: mapApi.listPaths,
    enabled: Boolean(apiKey),
  });

  const markerMutation = useMutation({
    mutationFn: (payload: Parameters<typeof mapApi.createMarker>[0]) => mapApi.createMarker(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["map", "markers"] }),
  });

  const markerDeleteMutation = useMutation({
    mutationFn: mapApi.deleteMarker,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["map", "markers"] }),
  });

  const polygonMutation = useMutation({
    mutationFn: (payload: Parameters<typeof mapApi.createPolygon>[0]) => mapApi.createPolygon(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["map", "polygons"] }),
  });

  const polygonDeleteMutation = useMutation({
    mutationFn: mapApi.deletePolygon,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["map", "polygons"] }),
  });

  const pathMutation = useMutation({
    mutationFn: (payload: Parameters<typeof mapApi.createPath>[0]) => mapApi.createPath(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["map", "paths"] }),
  });

  const pathDeleteMutation = useMutation({
    mutationFn: mapApi.deletePath,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["map", "paths"] }),
  });

  const iconTypes = config?.iconTypes ?? [];
  const visibilities = config?.visibilities ?? [];

  const {
    normalizedMarkers,
    invalidMarkerCount,
  } = useMemo(() => {
    let invalid = 0;
    const result = (markers ?? [])
      .map((marker) => ({
        ...marker,
        latitude: toNumber(marker.latitude),
        longitude: toNumber(marker.longitude),
      }))
      .filter((marker) => {
        const isValid = Number.isFinite(marker.latitude) && Number.isFinite(marker.longitude);
        if (!isValid) invalid += 1;
        return isValid;
      });
    return { normalizedMarkers: result, invalidMarkerCount: invalid };
  }, [markers]);

  const {
    normalizedPolygons,
    invalidPolygonCount,
  } = useMemo(() => {
    let invalid = 0;
    const result = (polygons ?? [])
      .map((polygon) => ({
        ...polygon,
        points: polygon.points
          .map((p) => ({
            ...p,
            latitude: toNumber(p.latitude),
            longitude: toNumber(p.longitude),
          }))
          .filter((p) => Number.isFinite(p.latitude) && Number.isFinite(p.longitude))
          .sort(
            (a, b) => (a.seq ?? 0) - (b.seq ?? 0)
          ),
      }))
      .filter((polygon) => {
        const isValid = polygon.points.length >= 3;
        if (!isValid) invalid += 1;
        return isValid;
      });
    return { normalizedPolygons: result, invalidPolygonCount: invalid };
  }, [polygons]);

  const {
    normalizedPaths,
    invalidPathCount,
  } = useMemo(() => {
    let invalid = 0;
    const result = (paths ?? [])
      .map((path) => ({
        ...path,
        points: path.points
          .map((p) => ({
            ...p,
            latitude: toNumber(p.latitude),
            longitude: toNumber(p.longitude),
          }))
          .filter((p) => Number.isFinite(p.latitude) && Number.isFinite(p.longitude))
          .sort((a, b) => (a.seq ?? 0) - (b.seq ?? 0)),
      }))
      .filter((path) => {
        const isValid = path.points.length >= 2;
        if (!isValid) invalid += 1;
        return isValid;
      });
    return { normalizedPaths: result, invalidPathCount: invalid };
  }, [paths]);

  useEffect(() => {
    if (invalidMarkerCount || invalidPolygonCount || invalidPathCount) {
      console.warn("[MapPage] Invalid map geometries filtered out", {
        invalidMarkerCount,
        invalidPolygonCount,
        invalidPathCount,
        rawMarkers: markers,
        rawPolygons: polygons,
        rawPaths: paths,
      });
    }
  }, [invalidMarkerCount, invalidPolygonCount, invalidPathCount, markers, polygons, paths]);

  const focusOnPosition = useCallback(
    (position: google.maps.LatLngLiteral, zoom = 13) => {
      if (!mapInstance) return;
      mapInstance.panTo(position);
      mapInstance.setZoom(zoom);
    },
    [mapInstance]
  );

  const focusOnBounds = useCallback(
    (points: MapPoint[]) => {
      if (!mapInstance || points.length === 0) return;
      const bounds = new google.maps.LatLngBounds();
      points.forEach((point) => bounds.extend({ lat: point.latitude, lng: point.longitude }));
      mapInstance.fitBounds(bounds);
    },
    [mapInstance]
  );

  const centroidFromPoints = useCallback((points: MapPoint[]): google.maps.LatLngLiteral => {
    if (!points.length) return DEFAULT_CENTER;
    const lat = points.reduce((sum, p) => sum + p.latitude, 0) / points.length;
    const lng = points.reduce((sum, p) => sum + p.longitude, 0) / points.length;
    return { lat, lng };
  }, []);

  useEffect(() => {
    if (!mapInstance) return;

    const bounds = new google.maps.LatLngBounds();
    normalizedMarkers.forEach((marker) => bounds.extend({ lat: marker.latitude, lng: marker.longitude }));
    normalizedPolygons.forEach((polygon) =>
      polygon.points.forEach((point) => bounds.extend({ lat: point.latitude, lng: point.longitude }))
    );
    normalizedPaths.forEach((path) =>
      path.points.forEach((point) => bounds.extend({ lat: point.latitude, lng: point.longitude }))
    );

    if (!bounds.isEmpty()) {
      mapInstance.fitBounds(bounds);
      const listener = google.maps.event.addListenerOnce(mapInstance, "idle", () => {
        const zoom = mapInstance.getZoom();
        if (zoom && zoom > 15) {
          mapInstance.setZoom(15);
        }
      });
      return () => google.maps.event.removeListener(listener);
    }
  }, [mapInstance, normalizedMarkers, normalizedPolygons, normalizedPaths]);

  const handleMapClick = useCallback(
    (event: google.maps.MapMouseEvent) => {
      if (mode !== "marker") return;
      if (!event.latLng) return;

      setMarkerDraft({
        latitude: event.latLng.lat(),
        longitude: event.latLng.lng(),
        iconTypeId: iconTypes[0]?.iconTypeId ?? null,
        note: "",
        visibilityCode: "PUBLIC",
      });
    },
    [iconTypes, mode]
  );

  const handleMarkerDraftSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!markerDraft) return;
    if (!markerDraft.iconTypeId) return;

    await markerMutation.mutateAsync({
      iconTypeId: markerDraft.iconTypeId,
      latitude: markerDraft.latitude,
      longitude: markerDraft.longitude,
      note: markerDraft.note || undefined,
      visibilityCode: markerDraft.visibilityCode,
    });

    setMarkerDraft(null);
    setMode("view");
  };

  const handlePolygonComplete = useCallback((polygon: google.maps.Polygon) => {
    const path = polygon.getPath();
    const points: MapPoint[] = [];
    for (let i = 0; i < path.getLength(); i += 1) {
      const latLng = path.getAt(i);
      points.push({ latitude: latLng.lat(), longitude: latLng.lng() });
    }
    polygon.setMap(null);
    if (points.length > 2) {
      setPolygonDraft({
        ...defaultPolygonDraft(),
        points,
      });
    }
  }, []);

  const handlePolygonSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!polygonDraft) return;
    if (polygonDraft.points.length < 3) return;

    await polygonMutation.mutateAsync({
      name: polygonDraft.name || undefined,
      visibilityCode: polygonDraft.visibilityCode,
      strokeColor: polygonDraft.strokeColor,
      fillColor: polygonDraft.fillColor,
      points: polygonDraft.points,
    });
    setPolygonDraft(null);
    setMode("view");
  };

  const handlePathComplete = useCallback((polyline: google.maps.Polyline) => {
    const path = polyline.getPath();
    const points: MapPoint[] = [];
    for (let i = 0; i < path.getLength(); i += 1) {
      const latLng = path.getAt(i);
      points.push({ latitude: latLng.lat(), longitude: latLng.lng() });
    }
    polyline.setMap(null);
    if (points.length > 1) {
      setPathDraft({
        ...defaultPathDraft(),
        points,
      });
    }
  }, []);

  const handlePathSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!pathDraft) return;
    if (pathDraft.points.length < 2) return;

    await pathMutation.mutateAsync({
      name: pathDraft.name || undefined,
      visibilityCode: pathDraft.visibilityCode,
      strokeColor: pathDraft.strokeColor,
      points: pathDraft.points,
    });
    setPathDraft(null);
    setMode("view");
  };

  const drawingOptions = useMemo((): google.maps.drawing.DrawingManagerOptions => {
    const polygonOptions: google.maps.PolygonOptions = {
      fillColor: polygonDraft?.fillColor ?? "#22c55e55",
      fillOpacity: 0.35,
      strokeColor: polygonDraft?.strokeColor ?? "#22c55e",
      strokeWeight: 2,
      clickable: false,
      editable: false,
    };

    const pathOptions: google.maps.PolylineOptions = {
      strokeColor: pathDraft?.strokeColor ?? "#3b82f6",
      strokeWeight: 3,
      clickable: false,
      editable: false,
    };

    return {
      drawingMode:
        mode === "polygon"
          ? google.maps.drawing.OverlayType.POLYGON
          : mode === "path"
          ? google.maps.drawing.OverlayType.POLYLINE
          : null,
      polygonOptions,
      polylineOptions: pathOptions,
      drawingControl: false,
    };
  }, [mode, pathDraft?.strokeColor, polygonDraft?.fillColor, polygonDraft?.strokeColor]);

  const infoWindowData = useMemo(() => {
    if (selectedMarkerId !== null) {
      const marker = normalizedMarkers.find((m) => m.markerId === selectedMarkerId);
      if (!marker) return null;
      return {
        position: { lat: marker.latitude, lng: marker.longitude },
        content: (
          <div className="min-w-40 space-y-1 text-sm text-slate-800">
            <p className="font-semibold">
              {iconTypes.find((i) => i.iconTypeId === marker.iconTypeId)?.name ?? "Pont"}
            </p>
            <p className="text-xs text-slate-600">{marker.visibilityCode}</p>
            <p className="text-xs">
              {marker.latitude.toFixed(5)}, {marker.longitude.toFixed(5)}
            </p>
            {marker.note && <p className="text-xs text-slate-700">{marker.note}</p>}
          </div>
        ),
      };
    }

    if (selectedPolygon) {
      const polygon = normalizedPolygons.find((p) => p.polygonId === selectedPolygon.polygonId);
      if (!polygon) return null;
      return {
        position: selectedPolygon.position,
        content: (
          <div className="min-w-48 space-y-1 text-sm text-slate-800">
            <p className="font-semibold">{polygon.name || "Megnevezetlen terület"}</p>
            <p className="text-xs text-slate-600">{polygon.visibilityCode}</p>
            <p className="text-xs">Pontok száma: {polygon.points.length}</p>
            {polygon.strokeColor && (
              <p className="text-xs">
                Körvonal: <span style={{ color: polygon.strokeColor }}>{polygon.strokeColor}</span>
              </p>
            )}
          </div>
        ),
      };
    }

    if (selectedPath) {
      const path = normalizedPaths.find((p) => p.pathId === selectedPath.pathId);
      if (!path) return null;
      return {
        position: selectedPath.position,
        content: (
          <div className="min-w-44 space-y-1 text-sm text-slate-800">
            <p className="font-semibold">{path.name || "Megnevezetlen útvonal"}</p>
            <p className="text-xs text-slate-600">{path.visibilityCode}</p>
            <p className="text-xs">Pontok száma: {path.points.length}</p>
            {path.strokeColor && (
              <p className="text-xs">
                Szín: <span style={{ color: path.strokeColor }}>{path.strokeColor}</span>
              </p>
            )}
          </div>
        ),
      };
    }

    return null;
  }, [
    iconTypes,
    normalizedMarkers,
    normalizedPaths,
    normalizedPolygons,
    selectedMarkerId,
    selectedPath,
    selectedPolygon,
  ]);

  if (!apiKey) {
    return (
      <div className="space-y-4">
        <h1 className="font-display text-3xl text-amber-100">Térkép</h1>
        <p className="text-sm text-amber-200/80">
          Kérlek állítsd be a <code>VITE_GOOGLE_MAPS_API_KEY</code> környezeti változót, majd indítsd újra a fejlesztői
          szervert.
        </p>
      </div>
    );
  }

  if (loadError) {
    return (
      <div className="space-y-3 text-red-200">
        <h2 className="font-display text-2xl">Nem sikerült betölteni a térképet</h2>
        <p className="text-sm">Ellenőrizd, hogy a Google Maps API kulcs érvényes-e, és engedélyezve vannak-e a szükséges API-k.</p>
      </div>
    );
  }

  const actionDisabled = !isAuthenticated;

  return (
    <div className="grid gap-6 lg:grid-cols-[2fr,1fr]">
      <div className="relative h-[70vh] overflow-hidden rounded-3xl border border-amber-200/10 bg-[#0f140f]">
        {!isLoaded ? (
          <div className="flex h-full items-center justify-center text-amber-200/70">
            <Loader2 className="mr-2 size-5 animate-spin" />
            Térkép betöltése...
          </div>
        ) : (
          <GoogleMap
            center={DEFAULT_CENTER}
            zoom={DEFAULT_ZOOM}
            mapContainerClassName="h-full w-full"
            onLoad={(map) => setMapInstance(map)}
            onClick={handleMapClick}
            options={{
              disableDefaultUI: true,
              zoomControl: true,
              mapTypeControl: true,
              fullscreenControl: true,
            }}
          >
            {mode !== "view" && isAuthenticated && (
              <DrawingManager
                options={drawingOptions}
                onPolygonComplete={handlePolygonComplete}
                onPolylineComplete={handlePathComplete}
              />
            )}

            {normalizedMarkers.map((marker) => (
              <MarkerF
                key={marker.markerId}
                position={{ lat: marker.latitude, lng: marker.longitude }}
                onClick={() => {
                  setSelectedMarkerId(marker.markerId);
                  setSelectedPolygon(null);
                  setSelectedPath(null);
                  focusOnPosition({ lat: marker.latitude, lng: marker.longitude }, 14);
                }}
                icon={
                  {
                    path: google.maps.SymbolPath.CIRCLE,
                    scale: 7,
                    fillColor: iconTypes.find((i) => i.iconTypeId === marker.iconTypeId)?.defaultColor ?? "#f97316",
                    fillOpacity: 1,
                    strokeColor: "#0f172a",
                    strokeWeight: 1,
                  } as google.maps.Symbol
                }
              />
            ))}

            {normalizedPolygons.map((polygon) => (
              <PolygonF
                key={polygon.polygonId}
                path={polygon.points.map((p) => ({ lat: p.latitude, lng: p.longitude }))}
                onClick={(event) => {
                  const position = event.latLng
                    ? { lat: event.latLng.lat(), lng: event.latLng.lng() }
                    : centroidFromPoints(polygon.points);
                  setSelectedPolygon({ polygonId: polygon.polygonId, position });
                  setSelectedPath(null);
                  setSelectedMarkerId(null);
                }}
                options={{
                  strokeColor: polygon.strokeColor ?? "#22c55e",
                  strokeOpacity: 1,
                  strokeWeight: 2,
                  fillColor: polygon.fillColor ?? "#22c55e55",
                  fillOpacity: polygon.fillColor ? 0.45 : 0.25,
                }}
              />
            ))}
            {normalizedPaths.map((path) => (
              <PolylineF
                key={path.pathId}
                path={path.points.map((p) => ({ lat: p.latitude, lng: p.longitude }))}
                onClick={(event) => {
                  const position = event.latLng
                    ? { lat: event.latLng.lat(), lng: event.latLng.lng() }
                    : centroidFromPoints(path.points);
                  setSelectedPath({ pathId: path.pathId, position });
                  setSelectedPolygon(null);
                  setSelectedMarkerId(null);
                }}
                options={{
                  strokeColor: path.strokeColor ?? "#f59e0b",
                  strokeOpacity: 1,
                  strokeWeight: 4,
                  geodesic: true,
                }}
              />
            ))}

            {infoWindowData && (
              <InfoWindow
                position={infoWindowData.position}
                onCloseClick={() => {
                  setSelectedMarkerId(null);
                  setSelectedPolygon(null);
                  setSelectedPath(null);
                }}
              >
                {infoWindowData.content}
              </InfoWindow>
            )}
          </GoogleMap>
        )}
      </div>

      <aside className="space-y-6 rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/30">
        <div>
          <p className="text-xs uppercase tracking-[0.3em] text-amber-200/70">Térkép eszköztár</p>
          <h2 className="mt-2 font-display text-2xl text-amber-100">Rétegek és jelölések</h2>
          {!isAuthenticated && (
            <p className="mt-2 text-sm text-amber-200/70">
              A térkép szerkesztéséhez jelentkezz be. Jelenleg a jelölések csak megtekinthetők.
            </p>
          )}
        </div>

        {(invalidMarkerCount || invalidPolygonCount || invalidPathCount) && (
          <div className="rounded-xl border border-amber-400/30 bg-amber-400/10 p-3 text-xs text-amber-100">
            <p className="font-semibold text-amber-200">Figyelem</p>
            <p>
              {invalidMarkerCount ? `${invalidMarkerCount} pont` : ""}
              {invalidMarkerCount && (invalidPolygonCount || invalidPathCount) ? ", " : ""}
              {invalidPolygonCount ? `${invalidPolygonCount} terület` : ""}
              {invalidPolygonCount && invalidPathCount ? ", " : ""}
              {invalidPathCount ? `${invalidPathCount} útvonal` : ""}
              {" "}érvénytelen koordinátákat tartalmazott, ezért nem jelent meg a térképen.
            </p>
          </div>
        )}

        <div className="space-y-3">
          <div className="grid gap-2 sm:grid-cols-3">
            <Button
              variant={mode === "marker" ? "default" : "outline"}
              disabled={actionDisabled}
              onClick={() => setMode((m) => (m === "marker" ? "view" : "marker"))}
            >
              <MapPin className="mr-2 size-4" />
              Új pont
            </Button>
            <Button
              variant={mode === "polygon" ? "default" : "outline"}
              disabled={actionDisabled}
              onClick={() => {
                setPolygonDraft(defaultPolygonDraft());
                setMode((m) => (m === "polygon" ? "view" : "polygon"));
              }}
            >
              <Shapes className="mr-2 size-4" />
              Terület
            </Button>
            <Button
              variant={mode === "path" ? "default" : "outline"}
              disabled={actionDisabled}
              onClick={() => {
                setPathDraft(defaultPathDraft());
                setMode((m) => (m === "path" ? "view" : "path"));
              }}
            >
              <PencilRuler className="mr-2 size-4" />
              Útvonal
            </Button>
          </div>
          {mode === "marker" && (
            <p className="text-xs text-amber-200/70">Kattints a térképre a pont elhelyezéséhez, majd töltsd ki az adatokat.</p>
          )}
          {mode === "polygon" && (
            <p className="text-xs text-amber-200/70">
              Rajzold meg a terület körvonalát a térképen. Dupla kattintással zárhatod a poligont.
            </p>
          )}
          {mode === "path" && (
            <p className="text-xs text-amber-200/70">
              Rajzold meg a szabadkézi vonalat. Dupla kattintással fejezheted be.
            </p>
          )}
        </div>

        <section className="space-y-3">
          <header className="flex items-center justify-between">
            <h3 className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-200/70">Jelölések</h3>
            <span className="text-xs text-amber-200/60">{markers?.length ?? 0} db</span>
          </header>
          <div className="space-y-2">
            {normalizedMarkers.map((marker) => (
              <article
                key={marker.markerId}
                className="flex cursor-pointer items-start justify-between rounded-xl border border-amber-200/10 bg-[#0f160f] p-3 text-sm text-amber-200/80 transition hover:border-amber-400/40 hover:bg-[#172017]"
                onClick={() => {
                  setSelectedMarkerId(marker.markerId);
                  setSelectedPolygon(null);
                  setSelectedPath(null);
                  focusOnPosition({ lat: marker.latitude, lng: marker.longitude }, 14);
                }}
              >
                <div>
                  <p className="font-semibold text-amber-100">
                    {iconTypes.find((i) => i.iconTypeId === marker.iconTypeId)?.name ?? "Pont"}{" "}
                    <span className="ml-2 text-xs text-amber-300/70">{marker.visibilityCode}</span>
                  </p>
                  <p className="text-xs">
                    {marker.latitude.toFixed(5)}, {marker.longitude.toFixed(5)}
                  </p>
                  {marker.note && <p className="mt-1 text-xs">{marker.note}</p>}
                </div>
                {isAuthenticated && (
                  <Button
                    size="icon-sm"
                    variant="ghost"
                    className="text-red-300 hover:text-red-200"
                    onClick={() => markerDeleteMutation.mutate(marker.markerId)}
                  >
                    <Trash2 className="size-4" />
                  </Button>
                )}
              </article>
            ))}
            {!normalizedMarkers.length && <p className="text-xs text-amber-200/60">Nincs rögzített pont.</p>}
          </div>
        </section>

        <section className="space-y-3">
          <header className="flex items-center justify-between">
            <h3 className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-200/70">Területek</h3>
            <span className="text-xs text-amber-200/60">{normalizedPolygons.length} db</span>
          </header>
          <div className="space-y-2">
            {normalizedPolygons.map((polygon) => (
              <article
                key={polygon.polygonId}
                className="flex cursor-pointer items-start justify-between rounded-xl border border-amber-200/10 bg-[#0f160f] p-3 text-sm text-amber-200/80 transition hover:border-amber-400/40 hover:bg-[#172017]"
                onClick={() => {
                  focusOnBounds(polygon.points);
                  setSelectedPolygon({
                    polygonId: polygon.polygonId,
                    position: centroidFromPoints(polygon.points),
                  });
                  setSelectedMarkerId(null);
                  setSelectedPath(null);
                }}
              >
                <div>
                  <p className="font-semibold text-amber-100">{polygon.name || "Megnevezetlen terület"}</p>
                  <p className="text-xs text-amber-300/70">{polygon.visibilityCode}</p>
                  <p className="text-xs">Pontok: {polygon.points.length}</p>
                </div>
                {isAuthenticated && (
                  <Button
                    size="icon-sm"
                    variant="ghost"
                    className="text-red-300 hover:text-red-200"
                    onClick={() => polygonDeleteMutation.mutate(polygon.polygonId)}
                  >
                    <Trash2 className="size-4" />
                  </Button>
                )}
              </article>
            ))}
            {!normalizedPolygons.length && <p className="text-xs text-amber-200/60">Nincs rögzített terület.</p>}
          </div>
        </section>

        <section className="space-y-3">
          <header className="flex items-center justify-between">
            <h3 className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-200/70">Útvonalak</h3>
            <span className="text-xs text-amber-200/60">{normalizedPaths.length} db</span>
          </header>
          <div className="space-y-2">
            {normalizedPaths.map((path) => (
              <article
                key={path.pathId}
                className="flex cursor-pointer items-start justify-between rounded-xl border border-amber-200/10 bg-[#0f160f] p-3 text-sm text-amber-200/80 transition hover:border-amber-400/40 hover:bg-[#172017]"
                onClick={() => {
                  focusOnBounds(path.points);
                  setSelectedPath({
                    pathId: path.pathId,
                    position: centroidFromPoints(path.points),
                  });
                  setSelectedMarkerId(null);
                  setSelectedPolygon(null);
                }}
              >
                <div>
                  <p className="font-semibold text-amber-100">{path.name || "Megnevezetlen útvonal"}</p>
                  <p className="text-xs text-amber-300/70">{path.visibilityCode}</p>
                  <p className="text-xs">Pontok: {path.points.length}</p>
                </div>
                {isAuthenticated && (
                  <Button
                    size="icon-sm"
                    variant="ghost"
                    className="text-red-300 hover:text-red-200"
                    onClick={() => pathDeleteMutation.mutate(path.pathId)}
                  >
                    <Trash2 className="size-4" />
                  </Button>
                )}
              </article>
            ))}
            {!normalizedPaths.length && <p className="text-xs text-amber-200/60">Nincs rögzített útvonal.</p>}
          </div>
        </section>
      </aside>

      {/* Marker draft form */}
      {markerDraft && (
        <form
          onSubmit={handleMarkerDraftSubmit}
          className="fixed bottom-6 left-1/2 z-20 w-full max-w-md -translate-x-1/2 space-y-4 rounded-3xl border border-amber-200/20 bg-[#121b12]/95 p-6 shadow-2xl shadow-black/40 backdrop-blur"
        >
          <h3 className="font-display text-xl text-amber-100">Új pont hozzáadása</h3>
          <div className="text-xs text-amber-200/70">
            Koordináták: {markerDraft.latitude.toFixed(5)}, {markerDraft.longitude.toFixed(5)}
          </div>

          <div className="space-y-2">
            <label className="text-sm text-amber-200/80">Piktogram</label>
            <select
              className="w-full rounded-lg border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none"
              value={markerDraft.iconTypeId ?? ""}
              onChange={(event) =>
                setMarkerDraft((prev) =>
                  prev ? { ...prev, iconTypeId: event.target.value ? Number(event.target.value) : null } : prev
                )
              }
              required
            >
              <option value="">Válassz kategóriát</option>
              {iconTypes.map((icon) => (
                <option key={icon.iconTypeId} value={icon.iconTypeId}>
                  {icon.name}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label className="text-sm text-amber-200/80">Jegyzet</label>
            <textarea
              className="w-full rounded-lg border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none"
              rows={3}
              value={markerDraft.note}
              onChange={(event) =>
                setMarkerDraft((prev) => (prev ? { ...prev, note: event.target.value } : prev))
              }
            />
          </div>

          <div className="space-y-2">
            <label className="text-sm text-amber-200/80">Láthatóság</label>
            <select
              className="w-full rounded-lg border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none"
              value={markerDraft.visibilityCode}
              onChange={(event) =>
                setMarkerDraft((prev) => (prev ? { ...prev, visibilityCode: event.target.value } : prev))
              }
            >
              {visibilities.map((visibility) => (
                <option key={visibility.visibilityCode} value={visibility.visibilityCode}>
                  {visibility.description ?? visibility.visibilityCode}
                </option>
              ))}
            </select>
          </div>

          <div className="flex justify-end gap-3">
            <Button type="button" variant="ghost" onClick={() => setMarkerDraft(null)}>
              Mégse
            </Button>
            <Button type="submit" disabled={markerMutation.isPending}>
              {markerMutation.isPending ? "Mentés..." : "Pont mentése"}
            </Button>
          </div>
        </form>
      )}

      {polygonDraft && mode === "polygon" && (
        <form
          onSubmit={handlePolygonSubmit}
          className="fixed bottom-6 left-6 z-20 w-full max-w-sm space-y-4 rounded-3xl border border-amber-200/20 bg-[#121b12]/95 p-6 shadow-2xl shadow-black/40 backdrop-blur"
        >
          <h3 className="font-display text-xl text-amber-100">Terület mentése</h3>
          <p className="text-xs text-amber-200/70">Pontok száma: {polygonDraft.points.length}</p>

          <div className="space-y-2">
            <label className="text-sm text-amber-200/80">Név</label>
            <input
              className="w-full rounded-lg border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none"
              value={polygonDraft.name}
              onChange={(event) => setPolygonDraft((prev) => (prev ? { ...prev, name: event.target.value } : prev))}
            />
          </div>

          <div className="space-y-2">
            <label className="text-sm text-amber-200/80">Láthatóság</label>
            <select
              className="w-full rounded-lg border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none"
              value={polygonDraft.visibilityCode}
              onChange={(event) =>
                setPolygonDraft((prev) => (prev ? { ...prev, visibilityCode: event.target.value } : prev))
              }
            >
              {visibilities.map((visibility) => (
                <option key={visibility.visibilityCode} value={visibility.visibilityCode}>
                  {visibility.description ?? visibility.visibilityCode}
                </option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-2">
              <label className="text-sm text-amber-200/80">Körvonal színe</label>
              <input
                type="color"
                className="flex h-10 w-full items-center justify-center rounded-lg border border-amber-400/30 bg-black/30"
                value={polygonDraft.strokeColor}
                onChange={(event) =>
                  setPolygonDraft((prev) => (prev ? { ...prev, strokeColor: event.target.value } : prev))
                }
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm text-amber-200/80">Kitöltés színe</label>
              <input
                type="color"
                className="flex h-10 w-full items-center justify-center rounded-lg border border-amber-400/30 bg-black/30"
                value={polygonDraft.fillColor.replace(/(..)$/, "")}
                onChange={(event) =>
                  setPolygonDraft((prev) =>
                    prev ? { ...prev, fillColor: `${event.target.value}55` } : prev
                  )
                }
              />
            </div>
          </div>

          <div className="flex justify-end gap-3">
            <Button type="button" variant="ghost" onClick={() => setPolygonDraft(null)}>
              Mégse
            </Button>
            <Button type="submit" disabled={polygonMutation.isPending}>
              {polygonMutation.isPending ? "Mentés..." : "Terület mentése"}
            </Button>
          </div>
        </form>
      )}

      {pathDraft && mode === "path" && (
        <form
          onSubmit={handlePathSubmit}
          className="fixed bottom-6 left-6 z-20 w-full max-w-sm space-y-4 rounded-3xl border border-amber-200/20 bg-[#121b12]/95 p-6 shadow-2xl shadow-black/40 backdrop-blur"
        >
          <h3 className="font-display text-xl text-amber-100">Útvonal mentése</h3>
          <p className="text-xs text-amber-200/70">Pontok száma: {pathDraft.points.length}</p>

          <div className="space-y-2">
            <label className="text-sm text-amber-200/80">Név</label>
            <input
              className="w-full rounded-lg border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none"
              value={pathDraft.name}
              onChange={(event) => setPathDraft((prev) => (prev ? { ...prev, name: event.target.value } : prev))}
            />
          </div>

          <div className="space-y-2">
            <label className="text-sm text-amber-200/80">Láthatóság</label>
            <select
              className="w-full rounded-lg border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none"
              value={pathDraft.visibilityCode}
              onChange={(event) =>
                setPathDraft((prev) => (prev ? { ...prev, visibilityCode: event.target.value } : prev))
              }
            >
              {visibilities.map((visibility) => (
                <option key={visibility.visibilityCode} value={visibility.visibilityCode}>
                  {visibility.description ?? visibility.visibilityCode}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label className="text-sm text-amber-200/80">Vonal színe</label>
            <input
              type="color"
              className="flex h-10 w-full items-center justify-center rounded-lg border border-amber-400/30 bg-black/30"
              value={pathDraft.strokeColor}
              onChange={(event) =>
                setPathDraft((prev) => (prev ? { ...prev, strokeColor: event.target.value } : prev))
              }
            />
          </div>

          <div className="flex justify-end gap-3">
            <Button type="button" variant="ghost" onClick={() => setPathDraft(null)}>
              Mégse
            </Button>
            <Button type="submit" disabled={pathMutation.isPending}>
              {pathMutation.isPending ? "Mentés..." : "Útvonal mentése"}
            </Button>
          </div>
        </form>
      )}
    </div>
  );
}

