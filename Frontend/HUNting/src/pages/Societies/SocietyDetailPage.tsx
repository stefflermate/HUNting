import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { societiesApi } from "@/services/societiesApi";
import { Button } from "@/components/ui/button";
import { Users } from "lucide-react";

export function SocietyDetailPage() {
  const { societyId } = useParams<{ societyId: string }>();
  const parsedId = Number(societyId);

  const { data, isFetching } = useQuery({
    queryKey: ["society", parsedId],
    queryFn: () => societiesApi.getById(parsedId),
    enabled: Number.isFinite(parsedId),
  });

  if (!Number.isFinite(parsedId)) {
    return (
      <div className="rounded-3xl border border-red-400/30 bg-red-950/30 p-6 text-red-100">
        Érvénytelen társaságazonosító.
      </div>
    );
  }

  if (isFetching || !data) {
    return (
      <div className="animate-pulse rounded-3xl border border-amber-200/5 bg-[#101710]/80 p-8">
        <div className="h-8 w-2/3 rounded bg-amber-200/15" />
        <div className="mt-6 h-3 w-full rounded bg-amber-200/10" />
        <div className="mt-3 h-3 w-4/5 rounded bg-amber-200/10" />
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-10 shadow-lg">
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
              Társasági profil
            </p>
            <h1 className="font-display text-4xl text-amber-100">
              {data.name}
            </h1>
            <p className="mt-3 text-amber-200/80">
              {data.description ?? "Ehhez a társasághoz még nem készült nyilvános leírás."}
            </p>
          </div>
          <div className="rounded-2xl border border-amber-400/30 bg-black/20 px-6 py-4 text-center">
            <p className="text-xs uppercase tracking-[0.3em] text-amber-300/80">
              Tagok
            </p>
            <p className="mt-2 flex items-center justify-center gap-2 font-display text-3xl text-amber-100">
              <Users className="size-6 text-amber-300" />
              {data.memberCount}
            </p>
          </div>
        </div>
      </header>

      <section className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20">
          <h2 className="font-display text-2xl text-amber-100">Kapcsolat</h2>
          <dl className="mt-4 space-y-3 text-sm text-amber-200/80">
            <div>
              <dt className="text-amber-200/60">Email</dt>
              <dd>{data.email}</dd>
            </div>
            {data.phone && (
              <div>
                <dt className="text-amber-200/60">Telefon</dt>
                <dd>{data.phone}</dd>
              </div>
            )}
            <div>
              <dt className="text-amber-200/60">Alapítva</dt>
              <dd>
                {new Date(data.createdAt).toLocaleDateString("hu-HU", {
                  year: "numeric",
                  month: "long",
                  day: "numeric",
                })}
              </dd>
            </div>
          </dl>
        </div>
        <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20">
          <h2 className="font-display text-2xl text-amber-100">
            Tagság
          </h2>
          <p className="mt-3 text-sm text-amber-200/80">
            Küldd el jelentkezésed a vadászati engedélyed adataival, korábbi tapasztalataiddal és referenciáiddal. A vezetőség minden kérelmet egyedileg bírál el.
          </p>
          <Button className="mt-6 w-full bg-amber-400 text-black hover:bg-amber-300">
            Jelentkezés
          </Button>
        </div>
      </section>
    </div>
  );
}
