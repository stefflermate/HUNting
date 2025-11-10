import { useMemo, useState } from "react";
import { Link, NavLink } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Menu, X } from "lucide-react";
import { useAuth } from "@/context/AuthContext";

type NavItem = { label: string; to: string };

export function AppHeader() {
  const { isAuthenticated, user } = useAuth();
  const [open, setOpen] = useState(false);

  const navItems = useMemo<NavItem[]>(() => {
    const items: NavItem[] = [
      { label: "Kezdolap", to: "/" },
      { label: "Hirdetesek", to: "/ads" },
      { label: "Tarsasagok", to: "/societies" },
      { label: "Terkep", to: "/map" },
      { label: "Hirek", to: "/news" },
    ];

    if (isAuthenticated && user?.accountType === "Person") {
      items.push({ label: "Tarsasagaim", to: "/societies/mine" });
      items.push({ label: "Naplok", to: "/logs" });
    }

    return items;
  }, [isAuthenticated, user?.accountType]);

  return (
    <header className="fixed inset-x-0 top-0 z-50 border-b border-amber-300/10 bg-[#0b120b]/95 backdrop-blur">
      <div className="mx-auto flex h-16 w-full max-w-6xl items-center justify-between px-4">
        <Link to="/" className="flex items-center gap-2 text-lg font-semibold text-amber-200">
          <span className="font-display text-2xl text-amber-300">HUNting</span>
          <span className="hidden text-xs uppercase tracking-[0.35em] text-amber-200/60 sm:inline">
            vadasz kozossegi portal
          </span>
        </Link>

        <nav className="hidden items-center gap-6 md:flex">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                [
                  "text-sm uppercase tracking-[0.3em] transition",
                  isActive ? "text-amber-300" : "text-amber-200/70 hover:text-amber-100",
                ].join(" ")
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="hidden items-center gap-3 md:flex">
          {isAuthenticated ? (
            <>
              {user?.accountType === "Society" && (
                <Button
                  asChild
                  variant="outline"
                  className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
                >
                  <Link to="/societies/manage">Tarsasagom</Link>
                </Button>
              )}
              {user?.roles.includes("Administrator") && (
                <Button
                  asChild
                  variant="outline"
                  className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
                >
                  <Link to="/admin">Admin</Link>
                </Button>
              )}
              <Button asChild className="bg-amber-400 text-black hover:bg-amber-300">
                <Link to="/profile">Profil</Link>
              </Button>
            </>
          ) : (
            <>
              <Button asChild variant="ghost" className="text-amber-200 hover:text-amber-100">
                <Link to="/auth/login">Bejelentkezes</Link>
              </Button>
              <Button asChild className="bg-amber-400 text-black hover:bg-amber-300">
                <Link to="/auth/register">Csatlakozz</Link>
              </Button>
            </>
          )}
        </div>

        <Button variant="ghost" size="icon" className="md:hidden" onClick={() => setOpen((value) => !value)}>
          {open ? <X className="size-5" /> : <Menu className="size-5" />}
        </Button>
      </div>

      {open && (
        <nav className="border-t border-amber-300/10 bg-[#0b120b]/95 px-4 pb-6 pt-3 md:hidden">
          <div className="space-y-3">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                onClick={() => setOpen(false)}
                className={({ isActive }) =>
                  [
                    "block rounded-xl px-4 py-3 text-sm uppercase tracking-[0.3em] transition",
                    isActive
                      ? "bg-amber-400/10 text-amber-200"
                      : "text-amber-200/70 hover:bg-amber-400/10 hover:text-amber-100",
                  ].join(" ")
                }
              >
                {item.label}
              </NavLink>
            ))}
          </div>
          <div className="mt-4 flex flex-col gap-3">
            {isAuthenticated ? (
              <>
                {user?.accountType === "Society" && (
                  <Button
                    asChild
                    variant="outline"
                    className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
                    onClick={() => setOpen(false)}
                  >
                    <Link to="/societies/manage">Tarsasagom</Link>
                  </Button>
                )}
                {user?.roles.includes("Administrator") && (
                  <Button
                    asChild
                    variant="outline"
                    className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
                    onClick={() => setOpen(false)}
                  >
                    <Link to="/admin">Admin</Link>
                  </Button>
                )}
                <Button
                  asChild
                  variant="outline"
                  className="border-amber-400/40 text-amber-200 hover:bg-amber-400/10"
                  onClick={() => setOpen(false)}
                >
                  <Link to="/profile">Profil</Link>
                </Button>
              </>
            ) : (
              <>
                <Button
                  asChild
                  variant="ghost"
                  className="text-amber-200 hover:text-amber-100"
                  onClick={() => setOpen(false)}
                >
                  <Link to="/auth/login">Bejelentkezes</Link>
                </Button>
                <Button
                  asChild
                  className="bg-amber-400 text-black hover:bg-amber-300"
                  onClick={() => setOpen(false)}
                >
                  <Link to="/auth/register">Csatlakozz</Link>
                </Button>
              </>
            )}
          </div>
        </nav>
      )}
    </header>
  );
}
