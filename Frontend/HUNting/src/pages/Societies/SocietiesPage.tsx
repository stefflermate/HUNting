import { useQuery } from "@tanstack/react-query";
import { societiesApi } from "@/services/societiesApi";
import { Button } from "@/components/ui/button";
import { UsersRound, Crown } from "lucide-react";
import { Link } from "react-router-dom";

export function SocietiesPage() {
  const { data, isFetching } = useQuery({
    queryKey: ["societies"],
    queryFn: () => societiesApi.list(),
  });

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Közösségi névsor
        </p>
        <h1 className="mt-2 font-display text-4xl text-amber-100">
          Vadásztársaságok
        </h1>
        <p className="mt-3 max-w-2xl text-amber-200/80">
          Böngészd a regisztrált magyar társaságokat, nézd meg a nyilvános profiljukat, és jelentkezz ahhoz a közösséghez, amelyik megfelel az elképzeléseidnek.
        </p>
      </header>

      <div className="grid gap-6 xl:grid-cols-2">
        {isFetching
          ? Array.from({ length: 4 }).map((_, idx) => (
              <div
                key={idx}
                className="animate-pulse rounded-3xl border border-amber-200/5 bg-[#101710]/80 p-6"
              >
                <div className="h-5 w-2/3 rounded bg-amber-200/20" />
                <div className="mt-4 h-3 w-full rounded bg-amber-200/10" />
                <div className="mt-3 h-3 w-5/6 rounded bg-amber-200/10" />
              </div>
            ))
          : data?.map((society) => (
              <article
                key={society.societyId}
                className="flex h-full flex-col gap-4 rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20 transition hover:border-amber-400/50 hover:bg-[#182616]"
              >
                <div className="flex items-start justify-between">
                  <div>
                    <h2 className="font-display text-2xl text-amber-100">
                      {society.name}
                    </h2>
                    <p className="mt-1 text-sm text-amber-200/70">
                      Alapítva: {new Date(society.createdAt).toLocaleDateString("hu-HU")}
                    </p>
                  </div>
                  <span className="rounded-full border border-amber-400/40 px-3 py-1 text-xs uppercase tracking-[0.2em] text-amber-200/70">
                    {society.memberCount} tag
                  </span>
                </div>
                <p className="text-sm text-amber-200/80">
                  {society.description ?? "Ehhez a társasághoz nincs nyilvános leírás."}
                </p>
                <div className="flex flex-wrap gap-3 text-sm text-amber-200/70">
                  {society.email && (
                    <span className="inline-flex items-center gap-2">
                      <Crown className="size-4 text-amber-300" />
                      {society.email}
                    </span>
                  )}
                  {society.phone && (
                    <span className="inline-flex items-center gap-2">
                      <UsersRound className="size-4 text-amber-300" />
                      {society.phone}
                    </span>
                  )}
                </div>
                <div className="mt-4 flex flex-wrap gap-3">
                  <Button
                    variant="outline"
                    className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
                    asChild
                  >
                    <Link to={`/societies/${society.societyId}`}>
                      Társaság megtekintése
                    </Link>
                  </Button>
                  <Button className="bg-amber-400 text-black hover:bg-amber-300">
                    Jelentkezés tagságra
                  </Button>
                </div>
              </article>
            ))}
      </div>
    </div>
  );
}
