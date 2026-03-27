import { useEffect, useRef, useState } from "react";
import type { ClipboardEvent, DragEvent, FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { adsApi } from "@/services/adsApi";
import { resolveAssetUrl } from "@/lib/apiClient";
import { Button } from "@/components/ui/button";
import type {
  AdPayload,
  AdCategoryItem,
  AdImage,
} from "@/types";

type ExistingImageState = AdImage & {
  markedForRemoval?: boolean;
};

type PendingImage = {
  id: string;
  file: File;
  preview: string;
};

export type AdFormSubmitArgs = {
  payload: AdPayload;
  newFiles: File[];
  removedImageIds: number[];
};

type AdFormProps = {
  initialValues?: {
    title?: string;
    description?: string;
    categoryId?: number;
    price?: number;
    locationText?: string;
    statusCode?: string;
    contactEmail?: string;
    contactPhone?: string;
  };
  initialImages?: AdImage[];
  submitLabel: string;
  isSubmitting: boolean;
  serverError?: string | null;
  onSubmit(data: AdFormSubmitArgs): Promise<void>;
};

export function AdForm({
  initialValues,
  initialImages,
  submitLabel,
  isSubmitting,
  serverError,
  onSubmit,
}: AdFormProps) {
  const [title, setTitle] = useState(initialValues?.title ?? "");
  const [description, setDescription] = useState(initialValues?.description ?? "");
  const [categoryId, setCategoryId] = useState<string>(
    initialValues?.categoryId ? String(initialValues.categoryId) : ""
  );
  const [price, setPrice] = useState(
    typeof initialValues?.price === "number" ? String(initialValues.price) : ""
  );
  const [locationText, setLocationText] = useState(initialValues?.locationText ?? "");
  const [statusCode, setStatusCode] = useState(initialValues?.statusCode ?? "ACTIVE");
  const [contactEmail, setContactEmail] = useState(initialValues?.contactEmail ?? "");
  const [contactPhone, setContactPhone] = useState(initialValues?.contactPhone ?? "");
  const [formError, setFormError] = useState<string | null>(null);

  const [existingImages, setExistingImages] = useState<ExistingImageState[]>(
    () =>
      initialImages?.map((image) => ({
        ...image,
        markedForRemoval: false,
      })) ?? []
  );
  const [pendingImages, setPendingImages] = useState<PendingImage[]>([]);

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const { data: categories } = useQuery<AdCategoryItem[]>({
    queryKey: ["ad-categories"],
    queryFn: adsApi.categories,
  });

  useEffect(() => {
    setTitle(initialValues?.title ?? "");
    setDescription(initialValues?.description ?? "");
    setCategoryId(initialValues?.categoryId ? String(initialValues.categoryId) : "");
    setPrice(
      typeof initialValues?.price === "number" ? String(initialValues.price) : ""
    );
    setLocationText(initialValues?.locationText ?? "");
    setStatusCode(initialValues?.statusCode ?? "ACTIVE");
    setContactEmail(initialValues?.contactEmail ?? "");
    setContactPhone(initialValues?.contactPhone ?? "");
  }, [
    initialValues?.title,
    initialValues?.description,
    initialValues?.categoryId,
    initialValues?.price,
    initialValues?.locationText,
    initialValues?.statusCode,
    initialValues?.contactEmail,
    initialValues?.contactPhone,
  ]);

  useEffect(() => {
    setExistingImages(
      initialImages?.map((image) => ({
        ...image,
        markedForRemoval: false,
      })) ?? []
    );
  }, [initialImages]);

  useEffect(() => {
    return () => {
      pendingImages.forEach((image) => URL.revokeObjectURL(image.preview));
    };
  }, [pendingImages]);

  const handleFiles = (files: FileList | File[]) => {
    const accepted = Array.from(files).filter((file) =>
      file.type.startsWith("image/")
    );
    if (accepted.length === 0) {
      setFormError("Csak kep fajl tamogatott (JPG, PNG, WEBP).");
      return;
    }
    setFormError(null);
    setPendingImages((current) => [
      ...current,
      ...accepted.map((file) => ({
        file,
        preview: URL.createObjectURL(file),
        id: `${file.name}-${file.size}-${file.lastModified}-${Math.random()
          .toString(36)
          .slice(2)}`,
      })),
    ]);
  };

  const handleDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    if (isSubmitting) return;
    if (event.dataTransfer.files && event.dataTransfer.files.length > 0) {
      handleFiles(event.dataTransfer.files);
      event.dataTransfer.clearData();
    }
  };

  const handlePaste = (event: ClipboardEvent<HTMLDivElement>) => {
    if (isSubmitting) return;
    const files = event.clipboardData.files;
    if (files && files.length > 0) {
      handleFiles(files);
    }
  };

  const toggleExistingImageRemoval = (imageId: number) => {
    setExistingImages((images) =>
      images.map((image) =>
        image.imageId === imageId
        ? { ...image, markedForRemoval: !image.markedForRemoval }
        : image
      )
    );
  };

  const removePendingImage = (id: string) => {
    setPendingImages((images) => {
      const toRemove = images.find((image) => image.id === id);
      if (toRemove) {
        URL.revokeObjectURL(toRemove.preview);
      }
      return images.filter((image) => image.id !== id);
    });
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setFormError(null);

    const trimmedTitle = title.trim();
    if (!trimmedTitle) {
      setFormError("Add meg a hirdetes cimet.");
      return;
    }

    let parsedPrice: number | undefined;
    if (price) {
      const value = Number(price);
      if (!Number.isFinite(value) || value < 0) {
        setFormError("Az ar csak pozitiv szam lehet.");
        return;
      }
      parsedPrice = value;
    }

    const payload: AdPayload = {
      title: trimmedTitle,
      description: description.trim() || undefined,
      categoryId: categoryId ? Number(categoryId) : undefined,
      price: parsedPrice,
      locationText: locationText.trim() || undefined,
      statusCode: statusCode || undefined,
      contactEmail: contactEmail.trim() || undefined,
      contactPhone: contactPhone.trim() || undefined,
    };

    const removedImageIds = existingImages
      .filter((image) => image.markedForRemoval)
      .map((image) => image.imageId);

    try {
      await onSubmit({
        payload,
        newFiles: pendingImages.map((image) => image.file),
        removedImageIds,
      });
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Ismeretlen hiba tortent a mentes soran.";
      setFormError(message);
    }
  };

  return (
    <form className="space-y-8" onSubmit={handleSubmit}>
      <div className="grid gap-6 md:grid-cols-2">
        <div className="md:col-span-2 space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="title">
            Cim
          </label>
          <input
            id="title"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            required
            maxLength={200}
            className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
        </div>

        <div className="md:col-span-2 space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="description">
            Leiras
          </label>
          <textarea
            id="description"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            rows={5}
            className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
        </div>

        <div className="space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="category">
            Kategoria
          </label>
          <select
            id="category"
            value={categoryId ?? ""}
            onChange={(event) => {
              const value = event.target.value;
              setCategoryId(value ?? "");
            }}
            className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-sm text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          >
            <option value="">Osszes kategoria</option>
            {categories?.map((category) => (
              <option key={category.categoryId} value={category.categoryId}>
                {category.name}
              </option>
            ))}
          </select>
        </div>

        <div className="space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="price">
            Ar (HUF)
          </label>
          <input
            id="price"
            type="number"
            min={0}
            step="0.01"
            value={price}
            onChange={(event) => setPrice(event.target.value)}
            className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
        </div>

        <div className="md:col-span-2 space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="location">
            Helyszin
          </label>
          <input
            id="location"
            value={locationText}
            onChange={(event) => setLocationText(event.target.value)}
            maxLength={200}
            className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
        </div>

        <div className="space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="contactEmail">
            Kapcsolati email
          </label>
          <input
            id="contactEmail"
            type="email"
            value={contactEmail}
            onChange={(event) => setContactEmail(event.target.value)}
            maxLength={150}
            className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
        </div>

        <div className="space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="contactPhone">
            Kapcsolati telefon
          </label>
          <input
            id="contactPhone"
            value={contactPhone}
            onChange={(event) => setContactPhone(event.target.value)}
            maxLength={30}
            className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
        </div>
      </div>

      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-semibold uppercase tracking-[0.3em] text-amber-200/80">
            Kepek
          </h2>
          <Button
            type="button"
            variant="outline"
            className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
            onClick={() => fileInputRef.current?.click()}
            disabled={isSubmitting}
          >
            Kep feltoltese
          </Button>
        </div>

        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          multiple
          className="hidden"
          onChange={(event) => {
            if (event.target.files) {
              handleFiles(event.target.files);
              event.target.value = "";
            }
          }}
        />

        <div
          className="rounded-2xl border border-dashed border-amber-400/40 bg-black/20 px-6 py-8 text-center text-sm text-amber-200/70 transition hover:border-amber-300/60 hover:bg-black/30"
          onDragOver={(event) => {
            event.preventDefault();
            event.dataTransfer.dropEffect = "copy";
          }}
          onDrop={handleDrop}
          onPaste={handlePaste}
        >
          <p className="font-medium text-amber-100">
            Huzz ide kepet vagy kattints a feltolteshez
          </p>
          <p className="mt-2 text-xs text-amber-200/60">
            Elfogadott formatumok: JPG, PNG, WEBP. A vagolapra masolt kepeket
            is ide illesztheted (Ctrl + V).
          </p>
        </div>

        {(existingImages.length > 0 || pendingImages.length > 0) && (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {existingImages.map((image) => (
              <figure
                key={image.imageId}
                className={[
                  "group relative overflow-hidden rounded-2xl border border-amber-300/15 bg-black/30",
                  image.markedForRemoval ? "opacity-60" : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
              >
                <img
                  src={resolveAssetUrl(image.imageUrl)}
                  alt="Hirdetes kep"
                  className="h-44 w-full object-cover transition group-hover:scale-[1.02]"
                />
                <div className="absolute inset-x-0 bottom-2 flex justify-center">
                  <Button
                    type="button"
                    size="sm"
                    variant={image.markedForRemoval ? "outline" : "destructive"}
                    className={
                      image.markedForRemoval
                        ? "border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
                        : "bg-red-600 text-white hover:bg-red-500"
                    }
                    onClick={() => toggleExistingImageRemoval(image.imageId)}
                    disabled={isSubmitting}
                  >
                    {image.markedForRemoval ? "Megsem" : "Eltavolitas"}
                  </Button>
                </div>
                {image.markedForRemoval && (
                  <span className="absolute inset-x-0 top-2 mx-auto w-11/12 rounded-full border border-red-400/50 bg-red-900/70 px-3 py-1 text-xs font-medium uppercase tracking-[0.2em] text-red-100 text-center">
                    Torlesre jelolve
                  </span>
                )}
              </figure>
            ))}

            {pendingImages.map((image) => (
              <figure
                key={image.id}
                className="group relative overflow-hidden rounded-2xl border border-amber-300/15 bg-black/30"
              >
                <img
                  src={image.preview}
                  alt={image.file.name}
                  className="h-44 w-full object-cover transition group-hover:scale-[1.02]"
                />
                <div className="absolute inset-x-0 bottom-2 flex justify-center">
                  <Button
                    type="button"
                    size="sm"
                    variant="destructive"
                    className="bg-red-600 text-white hover:bg-red-500"
                    onClick={() => removePendingImage(image.id)}
                    disabled={isSubmitting}
                  >
                    Eltavolitas
                  </Button>
                </div>
                <span className="absolute inset-x-0 top-2 mx-auto w-11/12 rounded-full border border-amber-400/40 bg-amber-900/60 px-3 py-1 text-xs font-medium uppercase tracking-[0.2em] text-amber-100 text-center">
                  Uj fajl
                </span>
              </figure>
            ))}
          </div>
        )}
      </section>

      {(formError || serverError) && (
        <p className="rounded-xl border border-red-400/30 bg-red-900/30 px-4 py-3 text-sm text-red-100">
          {serverError ?? formError}
        </p>
      )}

      <div className="flex justify-end">
        <Button
          type="submit"
          disabled={isSubmitting}
          className="bg-amber-400 text-black hover:bg-amber-300"
        >
          {isSubmitting ? "Mentes..." : submitLabel}
        </Button>
      </div>
    </form>
  );
}
