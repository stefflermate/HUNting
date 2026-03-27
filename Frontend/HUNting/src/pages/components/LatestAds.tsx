import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { adsApi } from "@/services/adsApi";
import { SkeletonCard } from "@/pages/components/SkeletonCard";
import { Button } from "@/components/ui/button";

export function LatestAds() {
  const { data, isLoading } = useQuery({
    queryKey: ["ads", "latest"],
    queryFn: () => adsApi.list({ take: 3 }),
  });

  return (
    <section className="space-y-8">
      <header className="flex flex-col gap-2 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
            Hirdetesek
          </p>
          <h2 className="font-display text-3xl text-amber-100">
            Legutobbi hirdetesek
          </h2>
          <p className="mt-2 max-w-2xl text-amber-200/80">
            Frissen feladott hirdetések társaságoktól és megbízható vadászoktól. Szűrj régióra, vadfajra vagy tapasztalati szintre a hirdetések között.
          </p>
        </div>
        <Button
          variant="outline"
          className="border-amber-400/60 text-amber-200 hover:bg-amber-400/10"
          asChild
        >
          <Link to="/ads">Összes hirdetés</Link>
        </Button>
      </header>

      <div className="grid gap-6 lg:grid-cols-3">
        {isLoading
          ? Array.from({ length: 3 }).map((_, idx) => <SkeletonCard key={idx} />)
          : data?.items.map((ad) => (
              <article
                key={ad.adId}
                className="flex h-full flex-col rounded-2xl border border-amber-200/10 bg-[#121b12]/80 p-6 shadow-lg transition hover:border-amber-400/40 hover:bg-[#182616]"
              >
                <div className="space-y-3">
                  <h3 className="font-display text-xl text-amber-100">
                    {ad.title}
                  </h3>
                  <p className="line-clamp-3 text-sm text-amber-200/80">
                    {ad.description ?? "Nincs megadott leírás."}
                  </p>
                </div>
                <div className="mt-6 flex items-center justify-between text-sm text-amber-200/70">
                  <span>{ad.locationText ?? "Rugalmas helyszín"}</span>
                  <span className="font-semibold text-amber-300">
                    {typeof ad.price === "number"
                      ? `${ad.price.toLocaleString("hu-HU")} HUF`
                      : "Ár egyeztetve"}
                  </span>
                </div>
                <Button
                  asChild
                  size="sm"
                  className="mt-6 self-start bg-amber-400 text-black hover:bg-amber-300"
                >
                  <Link to={`/ads/${ad.adId}`}>Részletek</Link>
                </Button>
              </article>
            ))}
      </div>
    </section>
  );
}
