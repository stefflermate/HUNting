import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { newsApi } from "@/services/newsApi";
import { SkeletonCard } from "@/pages/components/SkeletonCard";
import { Button } from "@/components/ui/button";

export function LatestNews() {
  const { data, isLoading } = useQuery({
    queryKey: ["news", "latest"],
    queryFn: () => newsApi.list({ take: 3 }),
  });

  return (
    <section className="space-y-8">
      <header className="flex flex-col gap-2 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
            Közösségi hírek
          </p>
          <h2 className="font-display text-3xl text-amber-100">
            Friss hírek a társaságoktól
          </h2>
          <p className="mt-2 max-w-2xl text-amber-200/80">
            Közlemények, vendégvadász-meghívók, sikeres vadászatok és szabályzatfrissítések a társaságok adminjaitól.
          </p>
        </div>
        <Button
          variant="outline"
          className="border-amber-400/60 text-amber-200 hover:bg-amber-400/10"
          asChild
        >
          <Link to="/news">Ugrás a hírekhez</Link>
        </Button>
      </header>

      <div className="grid gap-6 lg:grid-cols-3">
        {isLoading
          ? Array.from({ length: 3 }).map((_, idx) => <SkeletonCard key={idx} />)
          : data?.items.map((item) => (
              <article
                key={item.newsFeedId}
                className="flex h-full flex-col rounded-2xl border border-amber-200/10 bg-[#121b12]/80 p-6 shadow-lg transition hover:border-amber-400/40 hover:bg-[#182616]"
              >
                <div className="space-y-3">
                  <p className="text-xs uppercase tracking-[0.25em] text-amber-300/70">
                    {item.society?.name ?? "Közösségi hír"}
                  </p>
                  <h3 className="font-display text-xl text-amber-100">
                    {item.title ?? "Közlemény"}
                  </h3>
                  <p className="line-clamp-3 text-sm text-amber-200/80">
                    {item.content ?? "Ehhez a hírhez nem tartozik leírás."}
                  </p>
                </div>
                <div className="mt-6 flex items-center justify-between text-xs text-amber-200/70">
                  <span>
                    {new Date(item.createdAt).toLocaleDateString("hu-HU", {
                      month: "short",
                      day: "numeric",
                    })}
                  </span>
                  <span>{visibilityLabel(item.visibilityCode)}</span>
                </div>
              </article>
            ))}
      </div>
    </section>
  );
}

function visibilityLabel(code: string | undefined) {
  switch (code) {
    case "MEMBERS":
      return "Tagoknak";
    case "PUBLIC":
      return "Nyilvános";
    case "OFFICERS":
      return "Vezetőség";
    default:
      return code ?? "";
  }
}
