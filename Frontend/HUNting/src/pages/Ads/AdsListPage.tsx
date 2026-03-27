import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { adsApi } from "@/services/adsApi";
import { Button } from "@/components/ui/button";
import { Search } from "lucide-react";
import { Link } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import type {
  AdCategoryItem,
  AdListItem,
  PaginatedResult,
} from "@/types";

const PAGE_SIZE = 9;

export function AdsListPage() {
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState<number | undefined>();
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [showMyAds, setShowMyAds] = useState(false);
  const [page, setPage] = useState(1);
  const { isAuthenticated, user } = useAuth();

  const { data: categories } = useQuery<AdCategoryItem[]>({
    queryKey: ["ad-categories"],
    queryFn: adsApi.categories,
  });

  useEffect(() => {
    setPage(1);
  }, [categoryId, search, minPrice, maxPrice, showMyAds]);

  const { data, isFetching } = useQuery<PaginatedResult<AdListItem>>({
    queryKey: [
      "ads",
      categoryId,
      search,
      minPrice,
      maxPrice,
      showMyAds,
      page,
    ],
    queryFn: () =>
      adsApi.list({
        categoryId,
        search: search.trim() || undefined,
        minPrice: minPrice ? Number(minPrice) : undefined,
        maxPrice: maxPrice ? Number(maxPrice) : undefined,
        ownerUserId: showMyAds && user ? user.id : undefined,
        page,
        take: PAGE_SIZE,
      }),
  });

  const pages = useMemo(() => {
    if (!data?.total) return 1;
    return Math.max(1, Math.ceil(data.total / PAGE_SIZE));
  }, [data]);

  const clearFilters = () => {
    setSearch("");
    setCategoryId(undefined);
    setMinPrice("");
    setMaxPrice("");
    setShowMyAds(false);
  };

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg backdrop-blur">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
          <div className="lg:w-1/3">
            <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
              Hirdetes piacter
            </p>
            <h1 className="font-display text-4xl text-amber-100 flex items-center justify-between">
              <span>Hirdetesek</span>
            </h1>
            <p className="mt-3 text-amber-200/80">
              Keresd meg a hozzad illo felszerelest vagy vendegvadasz meghivast.
            </p>
            {isAuthenticated && (
              <div className="mt-6">
                <Button
                  asChild
                  className="bg-amber-400 text-black hover:bg-amber-300 w-full sm:w-auto"
                >
                  <Link to="/ads/new">Uj hirdetes</Link>
                </Button>
              </div>
            )}
          </div>
          
          <div className="lg:w-2/3">
            <form
              className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4"
              onSubmit={(event) => event.preventDefault()}
            >
              <div className="sm:col-span-2 md:col-span-3 flex items-center gap-2 rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-amber-100 focus-within:border-amber-300 focus-within:ring-2 focus-within:ring-amber-300/40">
                <Search className="size-5 text-amber-300 flex-shrink-0" />
                <input
                  value={search}
                  onChange={(event) => setSearch(event.target.value)}
                  placeholder="Keress cim vagy leiras alapjan..."
                  className="w-full bg-transparent text-base text-amber-100 outline-none placeholder:text-amber-200/60"
                />
              </div>
              
              <div className="flex flex-col gap-1.5">
                <label className="text-xs uppercase tracking-wider text-amber-300/60 font-semibold px-1">Kategoria</label>
                <select
                  className="w-full min-h-[44px] rounded-xl border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40 appearance-none cursor-pointer"
                  value={categoryId ?? ""}
                  onChange={(event) => {
                    const value = event.target.value;
                    setCategoryId(value ? Number(value) : undefined);
                  }}
                >
                  <option value="">Mindegyik kategoria</option>
                  {categories?.map((category) => (
                    <option key={category.categoryId} value={category.categoryId}>
                      {category.name}
                    </option>
                  ))}
                </select>
              </div>
              
              <div className="flex flex-col gap-1.5">
                <label className="text-xs uppercase tracking-wider text-amber-300/60 font-semibold px-1">Minimum ar</label>
                <div className="relative">
                  <input
                    type="number"
                    min={0}
                    placeholder="Mettol"
                    value={minPrice}
                    onChange={(event) => setMinPrice(event.target.value)}
                    className="w-full min-h-[44px] rounded-xl border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-amber-200/50 pointer-events-none">HUF</span>
                </div>
              </div>
              
              <div className="flex flex-col gap-1.5">
                <label className="text-xs uppercase tracking-wider text-amber-300/60 font-semibold px-1">Maximum ar</label>
                <div className="relative">
                  <input
                    type="number"
                    min={0}
                    placeholder="Meddig"
                    value={maxPrice}
                    onChange={(event) => setMaxPrice(event.target.value)}
                    className="w-full min-h-[44px] rounded-xl border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-amber-200/50 pointer-events-none">HUF</span>
                </div>
              </div>

              <div className="sm:col-span-2 md:col-span-3 flex justify-between items-center mt-2">
                {isAuthenticated ? (
                  <label className="flex items-center gap-2 text-sm text-amber-200 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={showMyAds}
                      onChange={(e) => setShowMyAds(e.target.checked)}
                      className="form-checkbox rounded border-amber-400/30 bg-black/30 text-amber-400 focus:ring-amber-400/30"
                    />
                    Sajat hirdeteseim
                  </label>
                ) : (
                  <div></div>
                )}
                <Button
                  type="button"
                  variant="ghost"
                  className="text-amber-200 hover:text-amber-100 hover:bg-amber-400/10"
                  onClick={clearFilters}
                >
                  Mezok torlese
                </Button>
              </div>
            </form>
          </div>
        </div>
      </header>

      <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
        {isFetching
          ? Array.from({ length: PAGE_SIZE }).map((_, idx) => (
              <div
                key={idx}
                className="animate-pulse rounded-3xl border border-amber-200/5 bg-[#101710]/80 p-6"
              >
                <div className="h-5 w-2/3 rounded bg-amber-200/20" />
                <div className="mt-4 space-y-3">
                  <div className="h-3 rounded bg-amber-200/10" />
                  <div className="h-3 w-5/6 rounded bg-amber-200/10" />
                  <div className="h-3 w-4/6 rounded bg-amber-200/10" />
                </div>
              </div>
            ))
          : data && data.items.length > 0 ? (
              data.items.map((ad) => (
                <article
                  key={ad.adId}
                  className="flex h-full flex-col rounded-3xl border border-amber-200/10 bg-[#121b12]/80 p-6 shadow-inner shadow-black/20 transition hover:border-amber-400/50 hover:bg-[#182616]"
                >
                  <div className="space-y-3">
                    <p className="text-xs uppercase tracking-[0.25em] text-amber-300/70">
                      {ad.category?.name ?? "Nincs kategoria"}
                    </p>
                    <h2 className="font-display text-2xl text-amber-100">
                      {ad.title}
                    </h2>
                    <p className="line-clamp-3 text-sm text-amber-200/80">
                      {ad.description ?? "Nincs megadott leiras."}
                    </p>
                  </div>
                  <div className="mt-6 flex items-center justify-between text-sm text-amber-200/70">
                    <span>{ad.locationText ?? "Rugalmas helyszin"}</span>
                    <span className="font-semibold text-amber-300">
                      {typeof ad.price === "number"
                        ? `${ad.price.toLocaleString("hu-HU")} HUF`
                        : "Ar egyeztetve"}
                    </span>
                  </div>
                  <Button
                    asChild
                    size="sm"
                    className="mt-6 self-start bg-amber-400 text-black hover:bg-amber-300"
                  >
                    <Link to={`/ads/${ad.adId}`}>Reszletek</Link>
                  </Button>
                </article>
              ))
            ) : (
              <div className="col-span-full rounded-3xl border border-amber-200/10 bg-[#121b12]/80 p-8 text-center text-amber-200/80">
                Nincs a szuroknek megfelelo hirdetes.
              </div>
            )}
      </div>

      <nav className="flex items-center justify-center gap-3">
        <Button
          variant="outline"
          className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
          disabled={page === 1}
          onClick={() => setPage((p) => Math.max(1, p - 1))}
        >
          Elozo
        </Button>
        <span className="text-sm text-amber-200/80">
          {page}. oldal / osszesen {pages}
        </span>
        <Button
          variant="outline"
          className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
          disabled={page >= pages}
          onClick={() => setPage((p) => Math.min(pages, p + 1))}
        >
          Kovetkezo
        </Button>
      </nav>
    </div>
  );
}

