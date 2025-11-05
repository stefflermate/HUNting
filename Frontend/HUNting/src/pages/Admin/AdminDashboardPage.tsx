import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { ShieldCheck, Users, Megaphone, Newspaper } from "lucide-react";
import { Navigate } from "react-router-dom";

const metrics = [
  { label: "Felhasználók", value: "2 540", icon: Users },
  { label: "Aktív hirdetések", value: "128", icon: Megaphone },
  { label: "Társaságok", value: "42", icon: ShieldCheck },
  { label: "Hírbejegyzések", value: "316", icon: Newspaper },
];

export function AdminDashboardPage() {
  const { user } = useAuth();

  if (!user || !user.roles.includes("Administrator")) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Admin konzol
        </p>
        <h1 className="mt-2 font-display text-4xl text-amber-100">
          Platform áttekintés
        </h1>
        <p className="mt-3 max-w-2xl text-amber-200/80">
          Kezeld a hirdetéseket, felügyeld a társaságokat, és vigyázz a közösség biztonságára. A mutatók valós időben frissülnek, ahogy a háttérszolgáltatásokkal összekapcsoljuk őket.
        </p>
      </header>

      <section className="grid gap-6 md:grid-cols-2 xl:grid-cols-4">
        {metrics.map((metric) => (
          <article
            key={metric.label}
            className="flex flex-col gap-3 rounded-3xl border border-amber-200/10 bg-[#121b12]/80 p-6 shadow-inner shadow-black/20"
          >
            <metric.icon className="size-8 text-amber-300" />
            <p className="text-sm uppercase tracking-[0.3em] text-amber-200/80">
              {metric.label}
            </p>
            <p className="font-display text-3xl text-amber-100">
              {metric.value}
            </p>
          </article>
        ))}
      </section>

      <section className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20">
          <h2 className="font-display text-2xl text-amber-100">Moderálási várólista</h2>
          <p className="mt-3 text-sm text-amber-200/80">
            Csatlakoztasd ezt a modult a moderációs végpontokhoz, hogy közzététel előtt jóváhagyd vagy elutasítsd a felhasználói tartalmakat.
          </p>
          <Button className="mt-6 bg-amber-400 text-black hover:bg-amber-300">
            Függő tételek áttekintése
          </Button>
        </div>
        <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20">
          <h2 className="font-display text-2xl text-amber-100">Rendszernapló</h2>
          <p className="mt-3 text-sm text-amber-200/80">
            Itt jelennek meg a legfrissebb felhasználói módosítások, szerepkör-hozzárendelések és API hibák, amint a telemetriai végpontok bekötésre kerülnek.
          </p>
          <Button
            variant="outline"
            className="mt-6 border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
          >
            Megfigyelési panel megnyitása
          </Button>
        </div>
      </section>
    </div>
  );
}
