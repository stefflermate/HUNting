import { useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { Link, Navigate, useLocation } from "react-router-dom";

export function LoginPage() {
  const { login, isLoading, isAuthenticated } = useAuth();
  const location = useLocation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  const from = (location.state as { from?: string } | null)?.from ?? "/";

  if (isAuthenticated) {
    return <Navigate to={from} replace />;
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    try {
      await login({ email, password });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Sikertelen bejelentkezés";
      setError(message);
    }
  };

  return (
    <div className="mx-auto max-w-md space-y-8 rounded-3xl border border-amber-200/10 bg-[#121b12]/95 p-8 shadow-xl">
      <header className="space-y-2 text-center">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          Ă–rĂĽlĂĽnk, hogy visszatĂ©rtĂ©l
        </p>
        <h1 className="font-display text-3xl text-amber-100">BejelentkezĂ©s</h1>
        <p className="text-sm text-amber-200/70">
          Jelentkezz be az e-mail cĂ­meddel Ă©s a regisztrĂˇciĂł sorĂˇn megadott jelszĂłval.
        </p>
      </header>

      <form className="space-y-6" onSubmit={handleSubmit}>
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
            JelszĂł
          </label>
          <input
            id="password"
            type="password"
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
          {isLoading ? "BejelentkezĂ©s..." : "BejelentkezĂ©s"}
        </Button>
      </form>

      <footer className="text-center text-sm text-amber-200/70">
        MĂ©g nincs fiĂłkod?{" "}
        <Link to="/auth/register" className="text-amber-200 hover:text-amber-100">
          RegisztrĂˇciĂł
        </Link>
      </footer>
    </div>
  );
}



