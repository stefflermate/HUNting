import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { Link, Navigate } from "react-router-dom";

export function ProfilePage() {
  const { user, logout } = useAuth();

  if (!user) {
    return <Navigate to="/auth/login" replace />;
  }

  const accountLabel = user.accountType === "Person" ? "Szemely" : "Tarsasag";

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">Profil</p>
        <h1 className="mt-2 font-display text-4xl text-amber-100">Udvozlunk ujra, {user.name}!</h1>
        <p className="mt-3 max-w-2xl text-amber-200/80">
          Innen gyorsan elerheted a hozzad tartozo tarsasagi feluleteket es attekintheted fiokod adatait.
        </p>
      </header>

      <section className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-6 shadow-inner shadow-black/20">
        <h2 className="font-display text-2xl text-amber-100">Fiok osszegzes</h2>
        <dl className="mt-4 space-y-3 text-sm text-amber-200/80">
          <div>
            <dt className="text-amber-200/60">Teljes nev</dt>
            <dd>{user.name}</dd>
          </div>
          <div>
            <dt className="text-amber-200/60">Email</dt>
            <dd>{user.email}</dd>
          </div>
          <div>
            <dt className="text-amber-200/60">Fiok tipus</dt>
            <dd>{accountLabel}</dd>
          </div>
          <div>
            <dt className="text-amber-200/60">Szerepkorok</dt>
            <dd>{user.roles.join(", ") || "Nincs szerepkor hozzarendelve"}</dd>
          </div>
        </dl>
        <div className="mt-6 flex flex-wrap gap-3">
          {user.accountType === "Person" && (
            <Button asChild className="bg-amber-400 text-black hover:bg-amber-300">
              <Link to="/societies/mine">Tarsasagaim</Link>
            </Button>
          )}
          {user.accountType === "Society" && (
            <Button asChild className="bg-amber-400 text-black hover:bg-amber-300">
              <Link to="/societies/manage">Tarsasag kezelese</Link>
            </Button>
          )}
          <Button
            variant="outline"
            className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
            onClick={() => logout()}
          >
            Kijelentkezes
          </Button>
        </div>
      </section>
    </div>
  );
}
