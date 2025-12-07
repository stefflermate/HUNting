import { useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { Link, Navigate, useLocation } from "react-router-dom";
import type { AccountType } from "@/types";

export function RegisterPage() {
  const { register, isLoading, isAuthenticated } = useAuth();
  const location = useLocation();
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [phone, setPhone] = useState("");
  const [accountType, setAccountType] = useState<AccountType>("Person");
  const [societyName, setSocietyName] = useState("");
  const [societyEmail, setSocietyEmail] = useState("");
  const [societyPhone, setSocietyPhone] = useState("");
  const [societyDescription, setSocietyDescription] = useState("");
  const [error, setError] = useState<string | null>(null);

  const from = (location.state as { from?: string } | null)?.from ?? "/";

  if (isAuthenticated) {
    return <Navigate to={from} replace />;
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    if (accountType === "Society" && !societyName.trim()) {
      setError("Add meg a tarsasag nevet.");
      return;
    }

    try {
      await register({
        name,
        email,
        password,
        phone,
        accountType,
        society:
          accountType === "Society"
            ? {
                name: societyName.trim(),
                email: societyEmail.trim() || undefined,
                phone: societyPhone.trim() || undefined,
                description: societyDescription.trim() || undefined,
              }
            : undefined,
      });
    } catch (err) {
      const message = err instanceof Error ? err.message : "A regisztracio sikertelen";
      setError(message);
    }
  };

  return (
    <div className="mx-auto max-w-2xl space-y-8 rounded-3xl border border-amber-200/10 bg-[#121b12]/95 p-8 shadow-xl">
      <header className="space-y-2 text-center">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Csatlakozz a kozosseghez
        </p>
        <h1 className="font-display text-3xl text-amber-100">Fiok letrehozasa</h1>
        <p className="text-sm text-amber-200/70">
          Regisztralj szemelyes vadasz fiokkal vagy hozz letre tarsasagi profilt a tagok kezelesehez.
        </p>
      </header>

      <form className="space-y-6" onSubmit={handleSubmit}>
        <div className="space-y-2">
          <label className="text-sm text-amber-200/80">Fiok tipus</label>
          <div className="grid grid-cols-2 gap-2 rounded-xl border border-amber-400/20 p-1">
            {(["Person", "Society"] as AccountType[]).map((type) => (
              <button
                key={type}
                type="button"
                className={[
                  "rounded-lg px-4 py-2 text-sm font-semibold uppercase tracking-[0.2em] transition",
                  accountType === type
                    ? "bg-amber-400 text-black"
                    : "text-amber-200/70 hover:text-amber-100",
                ].join(" ")}
                onClick={() => setAccountType(type)}
              >
                {type === "Person" ? "Szemely" : "Tarsasag"}
              </button>
            ))}
          </div>
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <div className="space-y-2">
            <label className="text-sm text-amber-200/80" htmlFor="name">
              Teljes nev
            </label>
            <input
              id="name"
              required
              value={name}
              onChange={(event) => setName(event.target.value)}
              className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
            />
          </div>
          <div className="space-y-2">
            <label className="text-sm text-amber-200/80" htmlFor="phone">
              Telefon (opcionalis)
            </label>
            <input
              id="phone"
              value={phone}
              onChange={(event) => setPhone(event.target.value)}
              className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
            />
          </div>
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <div className="space-y-2">
            <label className="text-sm text-amber-200/80" htmlFor="email">
              Email
            </label>
            <input
              id="email"
              type="email"
              required
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
            />
          </div>
          <div className="space-y-2">
            <label className="text-sm text-amber-200/80" htmlFor="password">
              Jelszo
            </label>
            <input
              id="password"
              type="password"
              minLength={6}
              required
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
            />
          </div>
        </div>

        {accountType === "Society" && (
          <div className="space-y-4 rounded-2xl border border-amber-400/20 bg-black/20 p-4">
            <h2 className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
              Tarsasagi adatok
            </h2>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2 md:col-span-2">
                <label className="text-sm text-amber-200/80" htmlFor="societyName">
                  Tarsasag neve
                </label>
                <input
                  id="societyName"
                  required
                  value={societyName}
                  onChange={(event) => setSocietyName(event.target.value)}
                  className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm text-amber-200/80" htmlFor="societyEmail">
                  Kapcsolat email (opcionalis)
                </label>
                <input
                  id="societyEmail"
                  type="email"
                  value={societyEmail}
                  onChange={(event) => setSocietyEmail(event.target.value)}
                  className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm text-amber-200/80" htmlFor="societyPhone">
                  Kapcsolat telefon (opcionalis)
                </label>
                <input
                  id="societyPhone"
                  value={societyPhone}
                  onChange={(event) => setSocietyPhone(event.target.value)}
                  className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
                />
              </div>
            </div>
            <div className="space-y-2">
              <label className="text-sm text-amber-200/80" htmlFor="societyDescription">
                Rovid bemutatkozas
              </label>
              <textarea
                id="societyDescription"
                rows={4}
                value={societyDescription}
                onChange={(event) => setSocietyDescription(event.target.value)}
                className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
              />
            </div>
          </div>
        )}

        {error && (
          <p className="rounded-xl border border-red-400/40 bg-red-900/40 px-4 py-3 text-sm text-red-100">
            {error}
          </p>
        )}

        <Button
          type="submit"
          className="w-full bg-amber-400 text-black hover:bg-amber-300"
          disabled={isLoading}
        >
          {isLoading ? "Fiok letrehozasa..." : "Fiok letrehozasa"}
        </Button>
      </form>

      <footer className="text-center text-sm text-amber-200/70">
        Mar van fiokod?{" "}
        <Link to="/auth/login" className="text-amber-200 hover:text-amber-100">
          Bejelentkezes
        </Link>
      </footer>
    </div>
  );
}
