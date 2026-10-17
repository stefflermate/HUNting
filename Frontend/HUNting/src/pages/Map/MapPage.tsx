import { useCallback, useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  GoogleMap,
  MarkerF,
  PolygonF,
  InfoWindowF,
  useLoadScript,
  DrawingManagerF,
} from "@react-google-maps/api";
import { mapApi } from "@/services/mapApi";
import { societiesApi } from "@/services/societiesApi";
import type { MapPoint } from "@/types";
import { Button } from "@/components/ui/button";
import { Loader2, MapPin, Shapes, Trash2 } from "lucide-react";
import { useAuth } from "@/context/AuthContext";

type DraftMode = "view" | "marker" | "polygon";

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
  description: string;
  visibilityCode: string;
  strokeColor: string;
  fillColor: string;
};

type MapLayerSelection =
  | { scope: "none" }
  | { scope: "personal" }
  | { scope: "society"; societyId: number };

type LayerEntry = {
  key: string;
  label: string;
  description: string;
  selection: MapLayerSelection;
};

function isSameSelection(a: MapLayerSelection, b: MapLayerSelection) {
  if (a.scope !== b.scope) return false;
  if (a.scope === "society" && b.scope === "society") {
    return a.societyId === b.societyId;
  }
  return true;
}

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
  description: "",
  visibilityCode: "PUBLIC",
  strokeColor: "#22c55e",
  fillColor: "#22c55e55",
});

export function MapPage() {
  const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;
  const { isAuthenticated } = useAuth();
  const queryClient = useQueryClient();

  const [mode, setMode] = useState<DraftMode>("view");
  const [markerDraft, setMarkerDraft] = useState<MarkerDraft | null>(null);
  const [polygonDraft, setPolygonDraft] = useState<PolygonDraft | null>(null);
  const [layerSelection, setLayerSelection] = useState<MapLayerSelection>({ scope: "none" });
  const [mapInstance, setMapInstance] = useState<google.maps.Map | null>(null);
  const [selectedMarkerId, setSelectedMarkerId] = useState<number | null>(null);
  const [selectedPolygon, setSelectedPolygon] = useState<{
    polygonId: number;
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

  const layerParams = useMemo(() => {
    if (layerSelection.scope === "personal") {
      return { scope: "personal" } as const;
    }
    if (layerSelection.scope === "society") {
      return { scope: "society", societyId: layerSelection.societyId } as const;
    }
    return null;
  }, [layerSelection]);

  const markersQuery = useQuery({
    queryKey: [
      "map",
      "markers",
      layerParams?.scope ?? "none",
      layerParams && layerParams.scope === "society" ? layerParams.societyId : null,
    ],
    queryFn: () => mapApi.listMarkers(layerParams!),
    enabled: Boolean(apiKey) && Boolean(layerParams),
  });

  const polygonsQuery = useQuery({
    queryKey: [
      "map",
      "polygons",
      layerParams?.scope ?? "none",
      layerParams && layerParams.scope === "society" ? layerParams.societyId : null,
    ],
    queryFn: () => mapApi.listPolygons(layerParams!),
    enabled: Boolean(apiKey) && Boolean(layerParams),
  });

  const { data: mySocieties } = useQuery({
    queryKey: ["societies", "mine"],
    queryFn: societiesApi.my,
    enabled: isAuthenticated,
  });

  const markers = markersQuery.data ?? [];
  const polygons = polygonsQuery.data ?? [];

  const layerEntries = useMemo<LayerEntry[]>(() => {
    const entries: LayerEntry[] = [
      {
        key: "personal",
        label: "Saját jelölések",
        description: "Csak általad mentett pontok és területek.",
        selection: { scope: "personal" },
      },
    ];
    (mySocieties ?? []).forEach((society) => {
      entries.push({
        key: `society-${society.societyId}`,
        label: society.name,
        description: "A társaságod közös rétege.",
        selection: { scope: "society", societyId: society.societyId },
      });
    });
    return entries;
  }, [mySocieties]);

  const activeLayerLabel = useMemo(() => {
    if (layerSelection.scope === "personal") {
      return "Saját jelölések";
    }
    if (layerSelection.scope === "society") {
      const society = mySocieties?.find((s) => s.societyId === layerSelection.societyId);
      return society ? `Társaság: ${society.name}` : "Társasági réteg";
    }
    return "Nincs kiválasztva";
  }, [layerSelection, mySocieties]);

  const handleLayerSelect = useCallback(
    (selection: MapLayerSelection) => {
      if (isSameSelection(selection, layerSelection)) return;
      setLayerSelection(selection);
      setMode("view");
      setMarkerDraft(null);
      setPolygonDraft(null);
      setSelectedMarkerId(null);
      setSelectedPolygon(null);
    },
    [layerSelection]
  );

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

  useEffect(() => {
    if (invalidMarkerCount || invalidPolygonCount) {
      console.warn("[MapPage] Invalid map geometries filtered out", {
        invalidMarkerCount,
        invalidPolygonCount,
        rawMarkers: markers,
        rawPolygons: polygons,
      });
    }
  }, [invalidMarkerCount, invalidPolygonCount, markers, polygons]);

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
  }, [mapInstance, normalizedMarkers, normalizedPolygons]);

  const handleMapClick = useCallback(
    (event: google.maps.MapMouseEvent) => {
      if (mode !== "marker") return;
      if (!layerParams) return;
      if (!event.latLng) return;

      setMarkerDraft({
        latitude: event.latLng.lat(),
        longitude: event.latLng.lng(),
        iconTypeId: iconTypes[0]?.iconTypeId ?? null,
        note: "",
        visibilityCode: "PUBLIC",
      });
    },
    [iconTypes, mode, layerParams]
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
      societyId: layerParams?.scope === "society" ? layerParams.societyId : undefined,
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
      description: polygonDraft.description || undefined,
      visibilityCode: polygonDraft.visibilityCode,
      strokeColor: polygonDraft.strokeColor,
      fillColor: polygonDraft.fillColor,
      societyId: layerParams?.scope === "society" ? layerParams.societyId : undefined,
      points: polygonDraft.points,
    });
    setPolygonDraft(null);
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

    return {
      drawingMode:
        mode === "polygon"
          ? google.maps.drawing.OverlayType.POLYGON
          : null,
      polygonOptions,
      drawingControl: false,
    };
  }, [mode, polygonDraft?.fillColor, polygonDraft?.strokeColor]);

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
            {polygon.description && <p className="text-xs text-slate-700">{polygon.description}</p>}
          </div>
        ),
      };
    }

    return null;
  }, [
    iconTypes,
    normalizedMarkers,
    normalizedPolygons,
    selectedMarkerId,
    selectedPolygon,
  ]);

  const MarkerList = () => (
    <div className="space-y-2">
      <header className="flex items-center justify-between">
        <h3 className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-200/70">Jelölések</h3>
        <span className="text-xs text-amber-200/60">{normalizedMarkers.length} db</span>
      </header>
      <div className="space-y-2">
        {normalizedMarkers.map((marker) => (
          <article
            key={marker.markerId}
            className="flex cursor-pointer items-start justify-between rounded-xl border border-amber-200/10 bg-[#0f160f] p-3 text-sm text-amber-200/80 transition hover:border-amber-400/40 hover:bg-[#172017]"
            onClick={() => {
              setSelectedMarkerId(marker.markerId);
              setSelectedPolygon(null);
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
                onClick={(event) => {
                  event.stopPropagation();
                  markerDeleteMutation.mutate(marker.markerId);
                }}
              >
                <Trash2 className="size-4" />
              </Button>
            )}
          </article>
        ))}
        {!normalizedMarkers.length && (
          <p className="text-xs text-amber-200/60">Ehhez a réteghez még nincs rögzített pont.</p>
        )}
      </div>
    </div>
  );

  const PolygonList = () => (
    <div className="space-y-2">
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
                onClick={(event) => {
                  event.stopPropagation();
                  polygonDeleteMutation.mutate(polygon.polygonId);
                }}
              >
                <Trash2 className="size-4" />
              </Button>
            )}
          </article>
        ))}
        {!normalizedPolygons.length && (
          <p className="text-xs text-amber-200/60">Ehhez a réteghez még nincs rögzített terület.</p>
        )}
      </div>
    </div>
  );

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

  const hasLayerSelected = Boolean(layerParams);
  const actionDisabled = !isAuthenticated || !hasLayerSelected;
  const layerSelectValue =
    layerSelection.scope === "none"
      ? ""
      : layerSelection.scope === "personal"
      ? "personal"
      : `society-${layerSelection.societyId}`;

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
            {isAuthenticated && (
              <DrawingManagerF options={drawingOptions} onPolygonComplete={handlePolygonComplete} />
            )}

            {normalizedMarkers.map((marker) => (
              <MarkerF
                key={marker.markerId}
                position={{ lat: marker.latitude, lng: marker.longitude }}
                onClick={() => {
                  setSelectedMarkerId(marker.markerId);
                  setSelectedPolygon(null);
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

            {polygonDraft && mode === "polygon" && polygonDraft.points.length > 2 && (
              <PolygonF
                path={polygonDraft.points.map((p) => ({ lat: p.latitude, lng: p.longitude }))}
                options={{
                  strokeColor: polygonDraft.strokeColor ?? "#22c55e",
                  strokeOpacity: 1,
                  strokeWeight: 2,
                  fillColor: polygonDraft.fillColor ?? "#22c55e55",
                  fillOpacity: polygonDraft.fillColor ? 0.45 : 0.25,
                  clickable: false,
                }}
              />
            )}

            {infoWindowData && (
              <InfoWindowF
                position={infoWindowData.position}
                onCloseClick={() => {
                  setSelectedMarkerId(null);
                  setSelectedPolygon(null);
                }}
              >
                {infoWindowData.content}
              </InfoWindowF>
            )}
          </GoogleMap>
        )}
        {!hasLayerSelected && isLoaded && (
          <div className="absolute inset-0 z-10 flex flex-col items-center justify-center bg-black/60 px-6 text-center text-amber-100">
            <p className="text-lg font-display">Válassz megjelenítendő réteget</p>
            <p className="mt-2 text-sm text-amber-200/80">
              A jobb oldali listából választhatod ki, hogy a saját vagy valamelyik társaság elemeit szeretnéd megjeleníteni.
            </p>
          </div>
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

        <div className="space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="layer-select">
            Aktív réteg kiválasztása
          </label>
          <select
            id="layer-select"
            value={layerSelectValue}
            onChange={(event) => {
              const value = event.target.value;
              if (value === "") {
                handleLayerSelect({ scope: "none" });
              } else if (value === "personal") {
                handleLayerSelect({ scope: "personal" });
              } else if (value.startsWith("society-")) {
                const [, id] = value.split("-");
                handleLayerSelect({ scope: "society", societyId: Number(id) });
              }
            }}
            className="w-full rounded-xl border border-amber-200/20 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none"
          >
            <option value="">-- Nincs kiválasztva --</option>
            <option value="personal">Saját jelöléseim</option>
            {(mySocieties ?? []).map((society) => (
              <option key={society.societyId} value={`society-${society.societyId}`}>
                {society.name}
              </option>
            ))}
          </select>
          <p className="text-xs text-amber-200/70">Aktív: {activeLayerLabel}</p>
        </div>

        {(invalidMarkerCount || invalidPolygonCount) && (
          <div className="rounded-xl border border-amber-400/30 bg-amber-400/10 p-3 text-xs text-amber-100">
            <p className="font-semibold text-amber-200">Figyelem</p>
            <p>
              {invalidMarkerCount ? `${invalidMarkerCount} pont` : ""}
              {invalidMarkerCount && invalidPolygonCount ? ", " : ""}
              {invalidPolygonCount ? `${invalidPolygonCount} terület` : ""}
              {" "}érvénytelen koordinátákat tartalmazott, ezért nem jelent meg a térképen.
            </p>
          </div>
        )}

        <div className="space-y-3">
          <div className="grid gap-2 sm:grid-cols-2">
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
          </div>
          {mode === "marker" && (
            <div className="flex items-center justify-between gap-4 rounded-xl bg-amber-400/10 p-3">
              <p className="text-xs text-amber-200/70">Kattints a térképre a pont elhelyezéséhez, majd töltsd ki az adatokat.</p>
              <Button size="sm" variant="ghost" className="h-8 shrink-0 text-amber-200 hover:text-amber-100" onClick={() => { setMode("view"); setMarkerDraft(null); }}>Mégse</Button>
            </div>
          )}
          {mode === "polygon" && (
            <div className="flex items-center justify-between gap-4 rounded-xl bg-amber-400/10 p-3">
              <p className="text-xs text-amber-200/70">
                Rajzold meg a terület körvonalát a térképen. Dupla kattintással zárhatod a poligont.
              </p>
              <Button size="sm" variant="ghost" className="h-8 shrink-0 text-amber-200 hover:text-amber-100" onClick={() => { setMode("view"); setPolygonDraft(null); }}>Mégse</Button>
            </div>
          )}
        </div>

        <div className="space-y-4">
          {layerEntries.map((entry) => {
            const isActive = isSameSelection(entry.selection, layerSelection);
            return (
              <div
                key={entry.key}
                className={`rounded-2xl border border-amber-200/10 bg-[#0f160f] p-4 transition ${
                  isActive ? "border-amber-400/40" : ""
                }`}
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <p className="font-semibold text-amber-100">{entry.label}</p>
                    <p className="text-xs text-amber-200/70">{entry.description}</p>
                  </div>
                  <div className="flex gap-2">
                    {isActive && (
                      <span className="rounded-full border border-emerald-400/40 px-3 py-1 text-xs text-emerald-200">
                        Aktív
                      </span>
                    )}
                    <Button
                      variant={isActive ? "default" : "outline"}
                      onClick={() => handleLayerSelect(entry.selection)}
                      className="text-xs uppercase tracking-[0.3em]"
                    >
                      {isActive ? "Frissítés" : "Megnyitás"}
                    </Button>
                  </div>
                </div>
                {isActive ? (
                  <div className="mt-4 space-y-6">
                    <MarkerList />
                    <PolygonList />
                  </div>
                ) : (
                  <p className="mt-4 text-xs text-amber-200/60">
                    A „Megnyitás” gombbal töltheted be ezt a réteget a térképen.
                  </p>
                )}
              </div>
            );
          })}
        </div>

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
              <Button type="button" variant="ghost" onClick={() => {
                setMarkerDraft(null);
                setMode("view");
              }}>
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
            <label className="text-sm text-amber-200/80">Leírás</label>
            <textarea
              className="w-full rounded-lg border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none resize-none"
              rows={2}
              value={polygonDraft.description}
              onChange={(event) => setPolygonDraft((prev) => (prev ? { ...prev, description: event.target.value } : prev))}
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
              <Button type="button" variant="ghost" onClick={() => {
                setPolygonDraft(null);
                setMode("view");
              }}>
              Mégse
            </Button>
            <Button type="submit" disabled={polygonMutation.isPending}>
              {polygonMutation.isPending ? "Mentés..." : "Terület mentése"}
            </Button>
          </div>
        </form>
      )}

    </div>
  );
}

