import { useEffect, useRef, useState } from "react";
import type { FormEvent, ChangeEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { huntingLogsApi } from "@/services/huntingLogsApi";
import { resolveAssetUrl } from "@/lib/apiClient";
import { Button } from "@/components/ui/button";
import { PlusCircle, Upload, X } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import type { HuntingLogItem, HuntingLogPayload } from "@/types";

function toLocalInputValue(date: Date) {
  const offset = date.getTimezoneOffset();
  const local = new Date(date.getTime() - offset * 60 * 1000);
  return local.toISOString().slice(0, 16);
}

export function HuntingLogsPage() {
  const { data, isFetching } = useQuery({
    queryKey: ["huntingLogs"],
    queryFn: () => huntingLogsApi.list(),
  });

  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [occurredAt, setOccurredAt] = useState(() => toLocalInputValue(new Date()));
  const [locationText, setLocationText] = useState("");
  const [animalType, setAnimalType] = useState("");
  const [notes, setNotes] = useState("");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [shotFired, setShotFired] = useState(false);
  const [shareWithSociety, setShareWithSociety] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    return () => {
      if (imagePreview) {
        URL.revokeObjectURL(imagePreview);
      }
    };
  }, [imagePreview]);

  const mutation = useMutation<HuntingLogItem, Error, HuntingLogPayload>({
    mutationFn: (payload) => huntingLogsApi.create(payload),
    onSuccess: async (created) => {
      queryClient.setQueryData<HuntingLogItem[] | undefined>(
        ["huntingLogs"],
        (previous) =>
          previous
            ? [
                created,
                ...previous.filter(
                  (log) => log.huntingLogId !== created.huntingLogId
                ),
              ]
            : [created]
      );
      setIsFormOpen(false);
      setOccurredAt(toLocalInputValue(new Date()));
      setLocationText("");
      setAnimalType("");
      setNotes("");
      handleFileSelection(null);
      setShotFired(false);
      setShareWithSociety(false);
    },
  });

  const handleToggleForm = () => {
    if (!isAuthenticated) {
      navigate("/auth/login", { state: { from: "/logs" }, replace: true });
      return;
    }
    setIsFormOpen((value) => !value);
  };

  function handleFileSelection(file: File | null | undefined) {
    if (imagePreview) {
      URL.revokeObjectURL(imagePreview);
      setImagePreview(null);
    }

    if (file) {
      if (!file.type.startsWith("image/")) {
        setError("Csak kep fajl toltheto fel.");
        if (fileInputRef.current) {
          fileInputRef.current.value = "";
        }
        return;
      }
      setError(null);
      setImageFile(file);
      setImagePreview(URL.createObjectURL(file));
    } else {
      setImageFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!occurredAt) {
      setError("Add meg a vadaszat idopontjat.");
      return;
    }

    setError(null);
    const payload: HuntingLogPayload = {
      occurredAt: new Date(occurredAt).toISOString(),
      locationText: locationText.trim() || undefined,
      animalType: animalType.trim() || undefined,
      notes: notes.trim() || undefined,
      shotFired,
      shareWithSociety,
      imageFile: imageFile ?? undefined,
    };

    try {
      await mutation.mutateAsync(payload);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Nem sikerult elmenteni a bejegyzest.";
      setError(message);
    }
  };

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
              Szemelyes vadasznaplo
            </p>
            <h1 className="font-display text-4xl text-amber-100">Naplobejegyzesek</h1>
            <p className="mt-3 max-w-2xl text-amber-200/80">
              Jegyezd fel minden vadaszat kulonleges pillanatat, es dontsd el, hogy a tarsasagoddal is megosztod-e.
            </p>
          </div>
          <Button
            className="w-full gap-2 bg-amber-400 text-black hover:bg-amber-300 md:w-auto"
            onClick={handleToggleForm}
          >
            <PlusCircle className="size-4" />
            {isFormOpen ? "Megsem" : "Uj bejegyzes"}
          </Button>
        </div>
      </header>

      {isFormOpen && (
        <section className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20">
          <form className="space-y-6" onSubmit={handleSubmit}>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <label className="text-sm text-amber-200/80" htmlFor="occurredAt">
                  Idopont
                </label>
                <input
                  id="occurredAt"
                  type="datetime-local"
                  value={occurredAt}
                  onChange={(event) => setOccurredAt(event.target.value)}
                  className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                  required
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm text-amber-200/80" htmlFor="locationText">
                  Helyszin
                </label>
                <input
                  id="locationText"
                  value={locationText}
                  onChange={(event) => setLocationText(event.target.value)}
                  className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm text-amber-200/80" htmlFor="animalType">
                  Vad
                </label>
                <input
                  id="animalType"
                  value={animalType}
                  onChange={(event) => setAnimalType(event.target.value)}
                  className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm text-amber-200/80" htmlFor="image">
                  Kep feltoltese (opcionalis)
                </label>
                <div className="flex flex-col gap-3 rounded-xl border border-amber-400/30 bg-black/20 p-4 text-sm text-amber-200/70">
                  <div className="flex flex-col gap-2">
                    <Button
                      type="button"
                      variant="outline"
                      className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={mutation.isPending}
                >
                  <Upload className="mr-2 size-4" />
                  Kep valasztasa
                    </Button>
                    <input
                      ref={fileInputRef}
                      id="image"
                      type="file"
                      accept="image/*"
                      className="hidden"
                      onChange={(event: ChangeEvent<HTMLInputElement>) => {
                        const file = event.target.files?.[0];
                        handleFileSelection(file);
                      }}
                    />
                  </div>
                  {imagePreview && (
                    <div className="relative overflow-hidden rounded-xl border border-amber-400/30">
                      <img
                        src={imagePreview}
                        alt="Feltoltendo kep"
                        className="h-48 w-full object-cover"
                      />
                      <Button
                        type="button"
                        size="sm"
                        variant="destructive"
                        className="absolute right-2 top-2 flex items-center gap-2 bg-red-600 text-white hover:bg-red-500"
                        onClick={() => {
                          handleFileSelection(null);
                        }}
                      >
                        <X className="size-4" />
                        Kep eltavolitasa
                      </Button>
                    </div>
                  )}
                  <div
                    onDragOver={(event) => {
                      event.preventDefault();
                      event.dataTransfer.dropEffect = "copy";
                    }}
                    onDrop={(event) => {
                      event.preventDefault();
                      const file = event.dataTransfer.files?.[0];
                      handleFileSelection(file);
                    }}
                    className="rounded-lg border border-dashed border-amber-400/30 bg-black/10 p-4 text-center text-xs text-amber-200/60"
                  >
                    Huzd ide a kepet vagy kattints a gombra a feltolteshez. Tamogatott
                    formatumok: JPG, JPEG, PNG, WEBP.
                  </div>
                </div>
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm text-amber-200/80" htmlFor="notes">
                Megjegyzes
              </label>
              <textarea
                id="notes"
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
                rows={4}
                className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
              />
            </div>

            <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
              <label className="flex items-center gap-2 text-sm text-amber-200/80">
                <input
                  type="checkbox"
                  checked={shotFired}
                  onChange={(event) => setShotFired(event.target.checked)}
                  className="size-4 rounded border border-amber-400/40 bg-black/30"
                />
                Lovessel zartad le
              </label>
              <label className="flex items-center gap-2 text-sm text-amber-200/80">
                <input
                  type="checkbox"
                  checked={shareWithSociety}
                  onChange={(event) => setShareWithSociety(event.target.checked)}
                  className="size-4 rounded border border-amber-400/40 bg-black/30"
                />
                Megosztas a tarsasaggal
              </label>
            </div>

            {(error || mutation.isError) && (
              <p className="rounded-xl border border-red-400/30 bg-red-900/30 px-4 py-3 text-sm text-red-100">
                {error ??
                  (mutation.error instanceof Error
                    ? mutation.error.message
                    : "Ismeretlen hiba tortent.")}
              </p>
            )}

            <div className="flex justify-end gap-3">
              <Button
                type="button"
                variant="ghost"
                className="text-amber-200 hover:text-amber-100"
                onClick={() => setIsFormOpen(false)}
              >
                Megsem
              </Button>
              <Button
                type="submit"
                className="bg-amber-400 text-black hover:bg-amber-300"
                disabled={mutation.isPending}
              >
                {mutation.isPending ? "Mentes..." : "Bejegyzes mentese"}
              </Button>
            </div>
          </form>
        </section>
      )}

      <div className="space-y-4">
        {isFetching
          ? Array.from({ length: 4 }).map((_, idx) => (
              <div
                key={idx}
                className="animate-pulse rounded-3xl border border-amber-200/5 bg-[#101710]/80 p-6"
              >
                <div className="h-4 w-1/4 rounded bg-amber-200/20" />
                <div className="mt-4 h-6 w-3/4 rounded bg-amber-200/15" />
                <div className="mt-3 h-3 w-full rounded bg-amber-200/10" />
              </div>
            ))
          : data?.map((log) => (
              <article
                key={log.huntingLogId}
                className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20 transition hover:border-amber-400/50 hover:bg-[#182616]"
              >
                <header className="flex flex-wrap items-center justify-between gap-4">
                  <div>
                    <p className="text-xs uppercase tracking-[0.25em] text-amber-300/70">
                      {new Date(log.occurredAt).toLocaleDateString("hu-HU")}
                    </p>
                    <h2 className="font-display text-2xl text-amber-100">
                      {log.locationText ?? "Ismeretlen helyszin"}
                    </h2>
                  </div>
                  <span className="rounded-full border border-amber-400/40 px-3 py-1 text-xs uppercase tracking-[0.2em] text-amber-200/70">
                    {log.shareWithSociety ? "Megosztva" : "Privat"}
                  </span>
                </header>
                <div className="mt-4 space-y-3 text-sm text-amber-200/80">
                  {log.animalType && (
                    <p className="font-semibold text-amber-200">
                      Vad: {log.animalType}
                    </p>
                  )}
                  {log.notes && <p>{log.notes}</p>}
                  {log.imageUrl && (
                    <img
                      src={resolveAssetUrl(log.imageUrl)}
                      alt="Hunting log"
                      className="rounded-2xl border border-amber-200/10"
                    />
                  )}
                </div>
              </article>
            ))}
      </div>
    </div>
  );
}
