import { useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { Link, Navigate, useLocation } from "react-router-dom";

export function RegisterPage() {
  const { register, isLoading, isAuthenticated } = useAuth();
  const location = useLocation();
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [phone, setPhone] = useState("");
  const [error, setError] = useState<string | null>(null);

  const from = (location.state as { from?: string } | null)?.from ?? "/";

  if (isAuthenticated) {
    return <Navigate to={from} replace />;
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    try {
      await register({ name, email, password, phone });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "A regisztráció sikertelen";
      setError(message);
    }
  };

  return (
    <div className="mx-auto max-w-md space-y-8 rounded-3xl border border-amber-200/10 bg-[#121b12]/95 p-8 shadow-xl">
      <header className="space-y-2 text-center">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Csatlakozz a kĂ¶zĂ¶ssĂ©ghez
        </p>
        <h1 className="font-display text-3xl text-amber-100">FiĂłk lĂ©trehozĂˇsa</h1>
        <p className="text-sm text-amber-200/70">
          RegisztrĂˇlj egyĂ©ni vadĂˇszkĂ©nt, kapj e-mail Ă©rtesĂ­tĂ©seket, Ă©s jelentkezz tĂˇrsasĂˇgokhoz.
        </p>
      </header>

      <form className="space-y-6" onSubmit={handleSubmit}>
        <div className="space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="name">
            Teljes nĂ©v
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
          <label className="text-sm text-amber-200/80" htmlFor="phone">
            TelefonszĂˇm (nem kĂ¶telezĹ‘)
          </label>
          <input
            id="phone"
            value={phone}
            onChange={(event) => setPhone(event.target.value)}
            className="w-full rounded-lg border border-amber-400/20 bg-black/30 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
        </div>
        <div className="space-y-2">
          <label className="text-sm text-amber-200/80" htmlFor="password">
            JelszĂł
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
          {isLoading ? "FiĂłk lĂ©trehozĂˇsa..." : "FiĂłk lĂ©trehozĂˇsa"}
        </Button>
      </form>

      <footer className="text-center text-sm text-amber-200/70">
        MĂˇr van fiĂłkod?{" "}
        <Link to="/auth/login" className="text-amber-200 hover:text-amber-100">
          BejelentkezĂ©s
        </Link>
      </footer>
    </div>
  );
}


