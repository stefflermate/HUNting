import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, useNavigate, useParams } from "react-router-dom";
import { adsApi } from "@/services/adsApi";
import { resolveAssetUrl } from "@/lib/apiClient";
import { Button } from "@/components/ui/button";
import { Phone, Mail, MapPin, Trash2 } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import type { AdListItem } from "@/types";

export function AdDetailPage() {
  const { adId } = useParams<{ adId: string }>();
  const parsedId = Number(adId);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);

  const {
    data,
    isLoading,
    isError,
  } = useQuery<AdListItem>({
    queryKey: ["ads", parsedId],
    queryFn: () => adsApi.getById(parsedId),
    enabled: Number.isFinite(parsedId),
    retry: false,
  });

  const deleteMutation = useMutation<void, Error, number>({
    mutationFn: (id) => adsApi.remove(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["ads"] });
      navigate("/ads");
    },
  });

  const imageDeleteMutation = useMutation<void, Error, number>({
    mutationFn: (imageId) => adsApi.deleteImage(parsedId, imageId),
    onSuccess: async (_, imageId) => {
      queryClient.setQueryData<AdListItem | undefined>(
        ["ads", parsedId],
        (previous) =>
          previous
            ? {
                ...previous,
                images: previous.images?.filter(
                  (image) => image.imageId !== imageId
                ),
              }
            : previous
      );
      await queryClient.invalidateQueries({ queryKey: ["ads"] });
    },
  });

  if (!Number.isFinite(parsedId)) {
    return (
      <div className="rounded-3xl border border-red-400/30 bg-red-950/30 p-6 text-red-100">
        Ervenytelen hirdetes azonosito.
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="animate-pulse rounded-3xl border border-amber-200/5 bg-[#101710]/80 p-8">
        <div className="h-8 w-2/3 rounded bg-amber-200/15" />
        <div className="mt-6 h-3 w-full rounded bg-amber-200/10" />
        <div className="mt-3 h-3 w-5/6 rounded bg-amber-200/10" />
        <div className="mt-3 h-3 w-4/6 rounded bg-amber-200/10" />
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="rounded-3xl border border-red-400/30 bg-red-950/30 p-6 text-red-100">
        A kivalasztott hirdetes nem talalhato.
      </div>
    );
  }

  const canManage =
    user &&
    (user.roles.includes("Administrator") || data.ownerUserId === user.id);

  const handleDelete = async () => {
    setDeleteError(null);
    const confirmed = window.confirm("Biztosan torlod a hirdetest?");
    if (!confirmed) return;
    try {
      await deleteMutation.mutateAsync(data.adId);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Nem sikerult torolni a hirdetest.";
      setDeleteError(message);
    }
  };

  const handleImageDelete = async (imageId: number) => {
    setImageError(null);
    const confirmed = window.confirm("Eltavolitod ezt a kepet?");
    if (!confirmed) return;
    try {
      await imageDeleteMutation.mutateAsync(imageId);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Nem sikerult torolni a kepet.";
      setImageError(message);
    }
  };

  return (
    <div className="grid gap-8 lg:grid-cols-[2fr,1fr]">
      <article className="space-y-6 rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <header className="space-y-4">
          <div className="space-y-2">
            <p className="text-xs uppercase tracking-[0.25em] text-amber-300/70">
              {data.category?.name ?? "Nincs kategoria"}
            </p>
            <h1 className="font-display text-4xl text-amber-100">{data.title}</h1>
          </div>

          {canManage && (
            <div className="flex flex-wrap gap-3">
              <Button
                asChild
                variant="outline"
                className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
              >
                <Link to={`/ads/${data.adId}/edit`}>Szerkesztes</Link>
              </Button>
              <Button
                type="button"
                variant="destructive"
                className="bg-red-600 text-white hover:bg-red-500"
                onClick={handleDelete}
                disabled={deleteMutation.isPending}
              >
                {deleteMutation.isPending ? "Torles..." : "Torles"}
              </Button>
            </div>
          )}

          <div className="flex flex-wrap items-center gap-4 text-sm text-amber-200/70">
            <span className="inline-flex items-center gap-2">
              <MapPin className="size-4" />
              {data.locationText ?? "Rugalmas helyszin"}
            </span>
            <span>
              Kozzetetelve:{" "}
              {new Date(data.createdAt).toLocaleDateString("hu-HU", {
                year: "numeric",
                month: "long",
                day: "numeric",
              })}
            </span>
          </div>
        </header>

        <section className="prose prose-invert max-w-none">
          <p>{data.description ?? "Ehhez a hirdeteshez nem tartozik leiras."}</p>
        </section>

        {data.images && data.images.length > 0 && (
          <section className="space-y-3">
            <div className="grid gap-4 sm:grid-cols-2">
              {data.images.map((img) => (
                <figure
                  key={img.imageId}
                  className="group relative overflow-hidden rounded-2xl border border-amber-300/10 bg-black/20 shadow-inner"
                >
                  <img
                    src={resolveAssetUrl(img.imageUrl)}
                    alt={data.title}
                    className="h-48 w-full object-cover transition group-hover:scale-[1.02]"
                  />
                  {canManage && (
                    <div className="absolute inset-x-0 bottom-2 flex justify-center">
                      <Button
                        type="button"
                        size="sm"
                        variant="destructive"
                        className="flex items-center gap-2 bg-red-600 text-white hover:bg-red-500"
                        onClick={() => handleImageDelete(img.imageId)}
                        disabled={imageDeleteMutation.isPending}
                      >
                        <Trash2 className="size-4" />
                        Kep torlese
                      </Button>
                    </div>
                  )}
                </figure>
              ))}
            </div>
            {imageError && (
              <p className="rounded-xl border border-red-400/30 bg-red-900/30 px-4 py-3 text-sm text-red-100">
                {imageError}
              </p>
            )}
          </section>
        )}
      </article>

      <aside className="space-y-6">
        <div className="rounded-3xl border border-amber-300/20 bg-[#131c13]/90 p-6 shadow-lg">
          <h2 className="font-display text-2xl text-amber-100">Kapcsolat</h2>
          <p className="mt-2 text-sm text-amber-200/80">
            Vedd fel a kapcsolatot az alabbi elerhetosegeken, egyeztess a
            reszletekrol, az arrol es a talalkozo idopontjarol.
          </p>
          <div className="mt-6 space-y-3 text-sm text-amber-200/70">
            {data.contactEmail && (
              <div className="flex items-center gap-3">
                <Mail className="size-4 text-amber-300" />
                <a href={`mailto:${data.contactEmail}`}>{data.contactEmail}</a>
              </div>
            )}
            {data.contactPhone && (
              <div className="flex items-center gap-3">
                <Phone className="size-4 text-amber-300" />
                <a href={`tel:${data.contactPhone}`}>{data.contactPhone}</a>
              </div>
            )}
          </div>
          <div className="mt-6 rounded-2xl border border-amber-400/30 bg-black/20 p-4 text-center">
            <p className="text-xs uppercase tracking-[0.3em] text-amber-300/80">
              Ar
            </p>
            <p className="mt-1 font-display text-3xl text-amber-100">
              {typeof data.price === "number"
                ? `${data.price.toLocaleString("hu-HU")} HUF`
                : "Ar egyeztetve"}
            </p>
          </div>
          <Button className="mt-6 w-full bg-amber-400 text-black hover:bg-amber-300">
            Kapcsolatfelvetel
          </Button>
          {deleteError && (
            <p className="mt-4 rounded-xl border border-red-400/30 bg-red-900/30 px-4 py-3 text-sm text-red-100">
              {deleteError}
            </p>
          )}
        </div>
      </aside>
    </div>
  );
}
