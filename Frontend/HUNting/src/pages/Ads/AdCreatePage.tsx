import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { adsApi } from "@/services/adsApi";
import type { AdPayload, AdListItem } from "@/types";
import { AdForm, type AdFormSubmitArgs } from "./AdForm";

export function AdCreatePage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [isProcessingImages, setIsProcessingImages] = useState(false);

  const mutation = useMutation<AdListItem, Error, AdPayload>({
    mutationFn: (payload) => adsApi.create(payload),
  });

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/auth/login"
        replace
        state={{ from: location.pathname }}
      />
    );
  }

  const handleSubmit = async ({ payload, newFiles }: AdFormSubmitArgs) => {
    setError(null);
    try {
      const created = await mutation.mutateAsync(payload);

      if (newFiles.length > 0) {
        setIsProcessingImages(true);
        for (const file of newFiles) {
          await adsApi.uploadImage(created.adId, file);
        }
      }

      await queryClient.invalidateQueries({ queryKey: ["ads"] });
      await queryClient.invalidateQueries({ queryKey: ["ads", created.adId] });
      navigate(`/ads/${created.adId}`, { replace: true });
    } catch (err) {
      const message =
        err instanceof Error
          ? err.message
          : "Nem sikerult elmenteni a hirdetest.";
      setError(message);
      throw err;
    } finally {
      setIsProcessingImages(false);
    }
  };

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Uj hirdetes
        </p>
        <h1 className="mt-2 font-display text-4xl text-amber-100">
          Hirdetes feladasa
        </h1>
        <p className="mt-3 max-w-2xl text-amber-200/80">
          Add meg a hirdetes reszleteit, valaszd ki a kategoriajat, es add meg a kapcsolati adatokat, hogy az erdeklodok felvehessik veled a kapcsolatot.
        </p>
      </header>

      <section className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-inner shadow-black/20">
        <AdForm
          submitLabel="Hirdetes mentese"
          isSubmitting={mutation.isPending || isProcessingImages}
          serverError={error}
          onSubmit={handleSubmit}
        />
      </section>
    </div>
  );
}
