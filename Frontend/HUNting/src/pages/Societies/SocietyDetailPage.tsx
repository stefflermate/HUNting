import { useState } from "react";
import { useParams } from "react-router-dom";
import { useMutation, useQuery } from "@tanstack/react-query";
import { societiesApi } from "@/services/societiesApi";
import { Button } from "@/components/ui/button";
import { Users } from "lucide-react";
import { useAuth } from "@/context/AuthContext";

export function SocietyDetailPage() {
  const { societyId } = useParams<{ societyId: string }>();
  const parsedId = Number(societyId);
  const { user, isAuthenticated } = useAuth();
  const [message, setMessage] = useState("");
  const [success, setSuccess] = useState<string | null>(null);
  const canJoin = isAuthenticated && user?.accountType === "Person";

  const { data, isFetching } = useQuery({
    queryKey: ["society", parsedId],
    queryFn: () => societiesApi.getById(parsedId),
    enabled: Number.isFinite(parsedId),
  });

  const joinMutation = useMutation({
    mutationFn: (payload: { societyId: number; message?: string }) =>
      societiesApi.join(payload.societyId, payload.message),
    onSuccess: () => {
      setSuccess("A jelentkezesed beert. A vezetoseg hamarosan valaszol.");
      setMessage("");
    },
  });

  if (!Number.isFinite(parsedId)) {
    return (
      <div className="rounded-3xl border border-red-400/30 bg-red-950/30 p-6 text-red-100">
        Ervenytelen tarsasagazonosito.
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
            <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">Tarsasagi profil</p>
            <h1 className="font-display text-4xl text-amber-100">{data.name}</h1>
            <p className="mt-3 text-amber-200/80">
              {data.description ?? "Ehhez a tarsasaghoz meg nem keszult nyilvanos leiras."}
            </p>
          </div>
          <div className="rounded-2xl border border-amber-400/30 bg-black/20 px-6 py-4 text-center">
            <p className="text-xs uppercase tracking-[0.3em] text-amber-300/80">Tagok</p>
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
              <dt className="text-amber-200/60">Alapitva</dt>
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
          <h2 className="font-display text-2xl text-amber-100">Tagsag</h2>
          <p className="mt-3 text-sm text-amber-200/80">
            Kuldd el rovid bemutatkozasod es miert szeretnel csatlakozni. A vezetoseg minden kerselmet egyedileg biral el.
          </p>
          {success && (
            <p className="mt-4 rounded-xl border border-emerald-400/40 bg-emerald-900/30 px-4 py-3 text-sm text-emerald-100">
              {success}
            </p>
          )}
          {canJoin ? (
            <form
              className="mt-6 space-y-4"
              onSubmit={(event) => {
                event.preventDefault();
                joinMutation.mutate({
                  societyId: parsedId,
                  message: message.trim() || undefined,
                });
              }}
            >
              <label className="text-sm text-amber-200/80" htmlFor="join-message">
                Uzenet (opcionalis)
              </label>
              <textarea
                id="join-message"
                rows={4}
                value={message}
                onChange={(event) => setMessage(event.target.value)}
                className="w-full rounded-xl border border-amber-400/30 bg-black/30 px-4 py-3 text-sm text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
              />
              <Button
                type="submit"
                className="w-full bg-amber-400 text-black hover:bg-amber-300"
                disabled={joinMutation.isPending}
              >
                {joinMutation.isPending ? "Jelentkezes folyamatban..." : "Jelentkezes bekuldese"}
              </Button>
            </form>
          ) : (
            <div className="mt-6 rounded-xl border border-amber-400/20 bg-black/20 px-4 py-3 text-sm text-amber-200/70">
              {isAuthenticated
                ? "Tarsasagi fiokkal nem lehet mas csoporthoz csatlakozni."
                : "A jelentkezeshez jelentkezz be szemely fiokkal."}
            </div>
          )}
        </div>
      </section>
    </div>
  );
}
