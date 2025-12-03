import { useState } from "react";
import { Navigate } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, Megaphone, Trash2, Users } from "lucide-react";

import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { adsApi } from "@/services/adsApi";
import { adminApi } from "@/services/adminApi";
import type { AdminUserItem } from "@/types";

type AdminView = "ads" | "users";

export function AdminDashboardPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [view, setView] = useState<AdminView>("ads");

  if (!user || !user.roles.includes("Administrator")) {
    return <Navigate to="/" replace />;
  }

  const adsQuery = useQuery({
    queryKey: ["admin", "ads"],
    queryFn: () => adsApi.list({ take: 200 }),
    enabled: view === "ads",
  });

  const usersQuery = useQuery({
    queryKey: ["admin", "users"],
    queryFn: adminApi.listUsers,
    enabled: view === "users",
  });

  const deleteAdMutation = useMutation({
    mutationFn: (adId: number) => adsApi.remove(adId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["admin", "ads"] }),
  });

  const deleteUserMutation = useMutation({
    mutationFn: (userId: number) => adminApi.deleteUser(userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["admin", "users"] }),
  });

  const confirm = (message: string) => window.confirm(message);

  const renderAds = () => {
    if (adsQuery.isLoading) {
      return (
        <div className="flex items-center gap-2 text-amber-200/70">
          <Loader2 className="size-4 animate-spin" />
          Hirdetések betöltése...
        </div>
      );
    }
    if (adsQuery.isError) {
      return <p className="text-sm text-red-300">Nem sikerült betölteni a hirdetéseket.</p>;
    }

    const items = adsQuery.data?.items ?? [];
    if (!items.length) {
      return <p className="text-sm text-amber-200/70">Jelenleg nincs hirdetés.</p>;
    }

    return (
      <div className="space-y-3">
        {items.map((ad) => (
          <article
            key={ad.adId}
            className="flex flex-col gap-2 rounded-2xl border border-amber-200/15 bg-[#0f160f] p-4 text-sm text-amber-200/80 shadow-inner shadow-black/30 lg:flex-row lg:items-center lg:justify-between"
          >
            <div>
              <p className="font-semibold text-amber-100">{ad.title}</p>
              <p className="text-xs text-amber-300/70">
                #{ad.adId} · {ad.statusCode}
              </p>
              <p className="text-xs text-amber-200/60">Létrehozva: {new Date(ad.createdAt).toLocaleString("hu-HU")}</p>
              {ad.ownerUserId && <p className="text-xs">Felhasználó tulajdonos: #{ad.ownerUserId}</p>}
              {ad.ownerSocietyId && <p className="text-xs">Társaság tulajdonos: #{ad.ownerSocietyId}</p>}
            </div>
            <Button
              variant="ghost"
              size="icon"
              className="text-red-300 hover:text-red-100"
              onClick={() => {
                if (!confirm("Biztosan törölni szeretnéd ezt a hirdetést? A művelet nem vonható vissza.")) {
                  return;
                }
                deleteAdMutation.mutate(ad.adId);
              }}
            >
              <Trash2 className="size-5" />
            </Button>
          </article>
        ))}
      </div>
    );
  };

  const renderUsers = () => {
    if (usersQuery.isLoading) {
      return (
        <div className="flex items-center gap-2 text-amber-200/70">
          <Loader2 className="size-4 animate-spin" />
          Felhasználók betöltése...
        </div>
      );
    }
    if (usersQuery.isError) {
      return <p className="text-sm text-red-300">Nem sikerült betölteni a felhasználókat.</p>;
    }

    const items = usersQuery.data ?? [];
    if (!items.length) {
      return <p className="text-sm text-amber-200/70">Jelenleg nincs felhasználó.</p>;
    }

    return (
      <div className="space-y-3">
        {items.map((entry: AdminUserItem) => (
          <article
            key={entry.id}
            className="flex flex-col gap-2 rounded-2xl border border-amber-200/15 bg-[#0f160f] p-4 text-sm text-amber-200/80 shadow-inner shadow-black/30 lg:flex-row lg:items-center lg:justify-between"
          >
            <div>
              <p className="font-semibold text-amber-100">{entry.name}</p>
              <p className="text-xs text-amber-300/70">{entry.email}</p>
              {entry.phone && <p className="text-xs">{entry.phone}</p>}
              <p className="text-xs text-amber-200/60">
                Szerepkörök: {entry.roles.length ? entry.roles.join(", ") : "Nincs"}
              </p>
            </div>
            <Button
              variant="ghost"
              size="icon"
              className="text-red-300 hover:text-red-100"
              onClick={() => {
                if (
                  !confirm(
                    "Biztosan törölni szeretnéd ezt a felhasználót? Minden hozzá kapcsolódó adat véglegesen törlődik."
                  )
                ) {
                  return;
                }
                deleteUserMutation.mutate(entry.id);
              }}
            >
              <Trash2 className="size-5" />
            </Button>
          </article>
        ))}
      </div>
    );
  };

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">Admin konzol</p>
        <h1 className="mt-2 font-display text-4xl text-amber-100">Platform felügyelet</h1>
        <p className="mt-3 max-w-2xl text-amber-200/80">
          Válaszd ki, hogy hirdetéseket vagy felhasználókat kívánsz áttekinteni, majd intézkedj a törlés gombbal. Minden
          törlés végleges, ezért a rendszer rákérdez a megerősítésre.
        </p>

        <div className="mt-6 flex flex-wrap gap-3">
          <Button
            variant={view === "ads" ? "default" : "outline"}
            className="flex items-center gap-2"
            onClick={() => setView("ads")}
          >
            <Megaphone className="size-4" />
            Hirdetések
          </Button>
          <Button
            variant={view === "users" ? "default" : "outline"}
            className="flex items-center gap-2"
            onClick={() => setView("users")}
          >
            <Users className="size-4" />
            Felhasználók
          </Button>
        </div>
      </header>

      <section className="rounded-3xl border border-amber-200/10 bg-[#121b12]/80 p-6 shadow-inner shadow-black/20">
        {view === "ads" ? renderAds() : renderUsers()}
      </section>
    </div>
  );
}
