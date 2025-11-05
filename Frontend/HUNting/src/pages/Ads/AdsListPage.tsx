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
  AdStatusItem,
  PaginatedResult,
} from "@/types";

const PAGE_SIZE = 9;

const fallbackStatuses: AdStatusItem[] = [
  { statusCode: "ACTIVE", description: "Aktiv" },
  { statusCode: "PENDING", description: "Ellenorzes alatt" },
  { statusCode: "SOLD", description: "Lezarva" },
];

export function AdsListPage() {
  const [status, setStatus] = useState<string | undefined>();
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState<number | undefined>();
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [page, setPage] = useState(1);
  const { isAuthenticated } = useAuth();

  const { data: categories } = useQuery<AdCategoryItem[]>({
    queryKey: ["ad-categories"],
    queryFn: adsApi.categories,
  });

  const { data: statusesResponse } = useQuery<AdStatusItem[]>({
    queryKey: ["ad-statuses"],
    queryFn: adsApi.statuses,
  });

  useEffect(() => {
    setPage(1);
  }, [status, categoryId, search, minPrice, maxPrice]);

  const { data, isFetching } = useQuery<PaginatedResult<AdListItem>>({
    queryKey: [
      "ads",
      status,
      categoryId,
      search,
      minPrice,
      maxPrice,
      page,
    ],
    queryFn: () =>
      adsApi.list({
        statusCode: status,
        categoryId,
        search: search.trim() || undefined,
        minPrice: minPrice ? Number(minPrice) : undefined,
        maxPrice: maxPrice ? Number(maxPrice) : undefined,
        page,
        take: PAGE_SIZE,
      }),
  });

  const pages = useMemo(() => {
    if (!data?.total) return 1;
    return Math.max(1, Math.ceil(data.total / PAGE_SIZE));
  }, [data]);

  const statusOptions =
    statusesResponse && statusesResponse.length > 0
      ? statusesResponse
      : fallbackStatuses;

  const clearFilters = () => {
    setStatus(undefined);
    setSearch("");
    setCategoryId(undefined);
    setMinPrice("");
    setMaxPrice("");
  };

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg backdrop-blur">
        <div className="flex flex-col gap-6 md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
              Hirdetes piacter
            </p>
            <h1 className="font-display text-4xl text-amber-100">Hirdetesek</h1>
            <p className="mt-3 max-w-2xl text-amber-200/80">
              Keresd meg a hozzad illo felszerelest vagy vendegvadasz meghivast, es szurj kategoria, statusz vagy ar alapjan.
            </p>
          </div>
          <div className="flex w-full flex-col gap-3 md:w-auto">
            <form
              className="flex w-full flex-col gap-3 md:flex-row md:items-end"
              onSubmit={(event) => event.preventDefault()}
            >
              <div className="flex items-center gap-2 rounded-xl border border-amber-400/30 bg-black/30 px-4 py-2 text-amber-100 focus-within:border-amber-300 focus-within:ring-2 focus-within:ring-amber-300/40">
                <Search className="size-4 text-amber-300" />
                <input
                  value={search}
                  onChange={(event) => setSearch(event.target.value)}
                  placeholder="Keress cim vagy leiras alapjan"
                  className="w-full bg-transparent text-sm text-amber-100 outline-none placeholder:text-amber-200/60"
                />
              </div>
              <select
                className="rounded-xl border border-amber-400/30 bg-black/30 px-4 py-2 text-sm text-amber-100 outline-none focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                value={categoryId ?? ""}
                onChange={(event) => {
                  const value = event.target.value;
                  setCategoryId(value ? Number(value) : undefined);
                }}
              >
                <option value="">Osszes kategoria</option>
                {categories?.map((category) => (
                  <option key={category.categoryId} value={category.categoryId}>
                    {category.name}
                  </option>
                ))}
              </select>
              <div className="flex gap-2">
                <input
                  type="number"
                  min={0}
                  placeholder="Minimum ar"
                  value={minPrice}
                  onChange={(event) => setMinPrice(event.target.value)}
                  className="w-28 rounded-xl border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                />
                <input
                  type="number"
                  min={0}
                  placeholder="Maximum ar"
                  value={maxPrice}
                  onChange={(event) => setMaxPrice(event.target.value)}
                  className="w-28 rounded-xl border border-amber-400/30 bg-black/30 px-3 py-2 text-sm text-amber-100 outline-none focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                />
              </div>
              <Button
                type="button"
                variant="ghost"
                className="border border-transparent text-amber-200 hover:border-amber-400/40 hover:bg-amber-400/10"
                onClick={clearFilters}
              >
                Szurok torlese
              </Button>
            </form>
            {isAuthenticated && (
              <Button
                asChild
                className="bg-amber-400 text-black hover:bg-amber-300"
              >
                <Link to="/ads/new">Uj hirdetes</Link>
              </Button>
            )}
          </div>
        </div>
      </header>

      <div className="flex flex-wrap gap-3">
        <Button
          variant={!status ? "default" : "outline"}
          className={
            !status
              ? "bg-amber-400 text-black hover:bg-amber-300"
              : "border-amber-400/30 text-amber-200 hover:bg-amber-400/10"
          }
          onClick={() => setStatus(undefined)}
        >
          Minden statusz
        </Button>
        {statusOptions.map((option) => (
          <Button
            key={option.statusCode}
            variant={option.statusCode === status ? "default" : "outline"}
            className={
              option.statusCode === status
                ? "bg-amber-400 text-black hover:bg-amber-300"
                : "border-amber-400/30 text-amber-200 hover:bg-amber-400/10"
            }
            onClick={() => setStatus(option.statusCode)}
          >
            {option.description ?? option.statusCode}
          </Button>
        ))}
      </div>

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

