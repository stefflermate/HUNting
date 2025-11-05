import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { Navigate } from "react-router-dom";

export function ProfilePage() {
  const { user, logout } = useAuth();

  if (!user) {
    return <Navigate to="/auth/login" replace />;
  }

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Profil
        </p>
        <h1 className="mt-2 font-display text-4xl text-amber-100">
          Üdvözlünk újra, {user.name}!
        </h1>
        <p className="mt-3 max-w-2xl text-amber-200/80">
          Itt fog megjelenni a tagsági szerepköröd, a belépett társaságok listája és a meghívók, amint az ehhez kapcsolódó végpontok elkészülnek.
        </p>
      </header>

      <section className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20">
        <h2 className="font-display text-2xl text-amber-100">Fiókösszegzés</h2>
        <dl className="mt-4 space-y-3 text-sm text-amber-200/80">
          <div>
            <dt className="text-amber-200/60">Teljes név</dt>
            <dd>{user.name}</dd>
          </div>
          <div>
            <dt className="text-amber-200/60">Email</dt>
            <dd>{user.email}</dd>
          </div>
          <div>
            <dt className="text-amber-200/60">Szerepkörök</dt>
            <dd>{user.roles.join(", ") || "Nincs szerepkör hozzárendelve"}</dd>
          </div>
        </dl>
        <Button
          variant="outline"
          className="mt-6 border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
          onClick={() => logout()}
        >
          Kijelentkezés
        </Button>
      </section>
    </div>
  );
}
