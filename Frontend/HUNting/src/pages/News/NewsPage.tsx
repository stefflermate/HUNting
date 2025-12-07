import { useQuery } from "@tanstack/react-query";
import { newsApi } from "@/services/newsApi";
import { resolveAssetUrl } from "@/lib/apiClient";

export function NewsPage() {
  const { data, isFetching } = useQuery({
    queryKey: ["news"],
    queryFn: () => newsApi.list({ take: 20 }),
  });

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Kronika
        </p>
        <h1 className="mt-2 font-display text-4xl text-amber-100">
          Hirfolyam
        </h1>
        <p className="mt-3 max-w-2xl text-amber-200/80">
          Kovetd a tarsasagok es adminisztratorok kozlemenyeit, vendegvadasz-meghivait es sikertorteneteit egy helyen.
        </p>
      </header>

      <div className="space-y-6">
        {isFetching
          ? Array.from({ length: 4 }).map((_, idx) => (
              <div
                key={idx}
                className="animate-pulse rounded-3xl border border-amber-200/5 bg-[#101710]/80 p-6"
              >
                <div className="h-4 w-1/4 rounded bg-amber-200/20" />
                <div className="mt-4 h-6 w-3/4 rounded bg-amber-200/15" />
                <div className="mt-3 h-3 w-full rounded bg-amber-200/10" />
                <div className="mt-3 h-3 w-5/6 rounded bg-amber-200/10" />
              </div>
            ))
          : data?.items.map((item) => (
              <article
                key={item.newsFeedId}
                className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20 transition hover:border-amber-400/50 hover:bg-[#182616]"
              >
                <header className="flex flex-wrap items-center justify-between gap-4">
                  <div>
                    <p className="text-xs uppercase tracking-[0.25em] text-amber-300/70">
                      {item.society?.name ?? "Kozossegi hir"}
                    </p>
                    <h2 className="font-display text-2xl text-amber-100">
                      {item.title ?? "Kozlemeny"}
                    </h2>
                  </div>
                  <p className="text-xs uppercase tracking-[0.25em] text-amber-200/60">
                    {new Date(item.createdAt).toLocaleString("hu-HU")}
                  </p>
                </header>
                <div className="mt-4 space-y-3 text-sm text-amber-200/80">
                  <p>{item.content ?? "Ehhez a hirhez nem tartozik leiras."}</p>
                  {item.imageUrl && (
                    <img
                      src={resolveAssetUrl(item.imageUrl)}
                      alt={item.title ?? "Hir illusztracio"}
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
