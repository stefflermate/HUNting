import { useState } from "react";
import { Navigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { societiesApi } from "@/services/societiesApi";
import { useAuth } from "@/context/AuthContext";
import { resolveAssetUrl } from "@/lib/apiClient";

export function MySocietiesPage() {
  const { user, isAuthenticated } = useAuth();
  const [selectedSociety, setSelectedSociety] = useState<number | null>(null);

  if (!isAuthenticated) {
    return <Navigate to="/auth/login" replace />;
  }

  if (user?.accountType !== "Person") {
    return (
      <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 text-amber-100">
        Ez az oldal csak szemely fiokkal erheto el.
      </div>
    );
  }

  const societiesQuery = useQuery({
    queryKey: ["my-societies"],
    queryFn: () => societiesApi.my(),
  });

  const postsQuery = useQuery({
    queryKey: ["society-posts", selectedSociety],
    enabled: Boolean(selectedSociety),
    queryFn: () => societiesApi.posts(selectedSociety!),
  });

  const societies = societiesQuery.data ?? [];

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">Tarsasagaim</p>
        <h1 className="font-display text-4xl text-amber-100">Csatlakozott kozossegek</h1>
        <p className="mt-3 text-sm text-amber-200/80">
          Valassz egy tarsasagot, hogy megtekintsd a belso posztjaikat.
        </p>
      </header>

      <section className="grid gap-8 lg:grid-cols-[300px,1fr]">
        <div className="space-y-4">
          {societies.length === 0 && (
            <div className="rounded-2xl border border-amber-200/10 bg-black/20 p-4 text-sm text-amber-200/70">
              Meg nem vagy tagja egyetlen tarsasagnak sem.
            </div>
          )}
          {societies.map((society) => (
            <button
              key={society.societyId}
              type="button"
              onClick={() => setSelectedSociety(society.societyId)}
              className={[
                "w-full rounded-2xl border px-4 py-3 text-left transition",
                selectedSociety === society.societyId
                  ? "border-amber-400 bg-amber-400/10 text-amber-100"
                  : "border-amber-400/20 bg-black/20 text-amber-200/80 hover:border-amber-400/40",
              ].join(" ")}
            >
              <p className="font-semibold">{society.name}</p>
              <p className="text-xs uppercase tracking-[0.3em] text-amber-200/60">
                {society.memberCount} tag
              </p>
            </button>
          ))}
        </div>

        <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6">
          {!selectedSociety && (
            <p className="text-sm text-amber-200/70">
              Valaszd ki azt a tarsasagot, amelynek posztjait latni szeretned.
            </p>
          )}
          {selectedSociety && postsQuery.isLoading && (
            <p className="text-sm text-amber-200/70">Posztok betoltese...</p>
          )}
          {selectedSociety && postsQuery.data && postsQuery.data.length === 0 && (
            <p className="text-sm text-amber-200/70">Meg nincsenek megosztott posztok.</p>
          )}
          {selectedSociety && postsQuery.data && postsQuery.data.length > 0 && (
            <div className="space-y-6">
              {postsQuery.data.map((post) => (
                <article
                  key={post.societyPostId}
                  className="space-y-3 rounded-2xl border border-amber-400/10 bg-black/20 p-4"
                >
                  <header>
                    <p className="text-xs uppercase tracking-[0.3em] text-amber-200/60">
                      {post.author?.fullName ?? "Ismeretlen szerzo"}
                    </p>
                    <p className="text-sm text-amber-200/50">
                      {new Date(post.createdAt).toLocaleString("hu-HU")}
                    </p>
                  </header>
                  {post.title && <h3 className="font-display text-xl text-amber-100">{post.title}</h3>}
                  {post.content && <p className="text-sm text-amber-200/80">{post.content}</p>}
                  {post.imageUrl && (
                    <img
                      src={resolveAssetUrl(post.imageUrl)}
                      alt={post.title ?? "Tarsasagi poszt kep"}
                      className="max-h-80 w-full rounded-xl object-cover"
                    />
                  )}
                </article>
              ))}
            </div>
          )}
        </div>
      </section>
    </div>
  );
}
