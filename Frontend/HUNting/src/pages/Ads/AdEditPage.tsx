import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Navigate, useLocation, useNavigate, useParams } from "react-router-dom";
import { adsApi } from "@/services/adsApi";
import { useAuth } from "@/context/AuthContext";
import type { AdPayload, AdListItem } from "@/types";
import { AdForm, type AdFormSubmitArgs } from "./AdForm";

export function AdEditPage() {
  const { adId } = useParams<{ adId: string }>();
  const parsedId = Number(adId);
  const { user, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [isProcessingImages, setIsProcessingImages] = useState(false);

  const { data, isLoading, isError } = useQuery<AdListItem>({
    queryKey: ["ads", parsedId],
    queryFn: () => adsApi.getById(parsedId),
    enabled: Number.isFinite(parsedId),
  });

  const mutation = useMutation<void, Error, AdPayload>({
    mutationFn: (payload) => adsApi.update(parsedId, payload),
  });

  if (!Number.isFinite(parsedId)) {
    return (
      <div className="rounded-3xl border border-red-400/30 bg-red-950/30 p-6 text-red-100">
        Ervenytelen hirdetes azonosito.
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/auth/login"
        replace
        state={{ from: location.pathname }}
      />
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
        A hirdetes nem talalhato vagy nem erheto el.
      </div>
    );
  }

  const isOwner =
    user?.roles.includes("Administrator") || (user && data.ownerUserId === user.id);

  if (!isOwner) {
    return (
      <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 text-amber-100">
        Ez a hirdetes nem a te fiokodhoz tartozik, ezert nem modosithatod.
      </div>
    );
  }

  // Szándékosan nem useMemo: a feltételes return-ök után hook nem hívható
  // (Rules of Hooks), az AdForm pedig mezőnként figyeli a változásokat.
  const initialValues = {
    title: data.title,
    description: data.description,
    categoryId: data.category?.categoryId,
    price: data.price,
    locationText: data.locationText,
    statusCode: data.statusCode,
    contactEmail: data.contactEmail,
    contactPhone: data.contactPhone,
  };

  const handleSubmit = async ({
    payload,
    newFiles,
    removedImageIds,
  }: AdFormSubmitArgs) => {
    setError(null);
    const hasImageChanges =
      newFiles.length > 0 || removedImageIds.length > 0;
    try {
      await mutation.mutateAsync({
        ...payload,
        ownerUserId: data.ownerUserId,
        ownerSocietyId: data.ownerSocietyId,
      });

      if (hasImageChanges) {
        setIsProcessingImages(true);
      }

      for (const imageId of removedImageIds) {
        await adsApi.deleteImage(parsedId, imageId);
      }

      for (const file of newFiles) {
        await adsApi.uploadImage(parsedId, file);
      }

      await queryClient.invalidateQueries({ queryKey: ["ads"] });
      await queryClient.invalidateQueries({ queryKey: ["ads", parsedId] });
      navigate(`/ads/${parsedId}`, { replace: true });
    } catch (err) {
      const message =
        err instanceof Error
          ? err.message
          : "Nem sikerult frissiteni a hirdetest.";
      setError(message);
      throw err;
    } finally {
      if (hasImageChanges) {
        setIsProcessingImages(false);
      }
    }
  };

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Hirdetes szerkesztese
        </p>
        <h1 className="mt-2 font-display text-4xl text-amber-100">
          {data.title}
        </h1>
        <p className="mt-3 max-w-2xl text-amber-200/80">
          Modositsd a hirdetes adatait, majd mentsd el a valtoztatasokat. A hirdetes azonnal frissulni fog a listaban.
        </p>
      </header>

      <section className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-inner shadow-black/20">
        <AdForm
          initialValues={initialValues}
          initialImages={data.images ?? []}
          submitLabel="Valtoztatasok mentese"
          isSubmitting={mutation.isPending || isProcessingImages}
          serverError={error}
          onSubmit={handleSubmit}
        />
      </section>
    </div>
  );
}
