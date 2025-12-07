import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { FeatureGrid } from "@/pages/components/FeatureGrid";
import { LatestAds } from "@/pages/components/LatestAds";
import { LatestNews } from "@/pages/components/LatestNews";

export function HomePage() {
  return (
    <div className="space-y-24">
      <section className="relative isolate overflow-hidden rounded-3xl bg-gradient-to-r from-[#1b2a1c] via-[#223424] to-[#1b2a1c] p-10 shadow-2xl lg:flex lg:items-center lg:justify-between lg:p-16">
        <div className="max-w-xl space-y-6 text-left">
          <p className="text-sm uppercase tracking-[0.35em] text-amber-300">
            Vadász közösség
          </p>
          <h1 className="font-display text-4xl leading-tight text-amber-100 sm:text-5xl">
            Találd meg a következő vadászmeghívást, vagy kezeld könnyedén a társaságodat.
          </h1>
          <p className="text-lg text-amber-200/80">
            Egyetlen digitális otthon vadászoknak, vendégeknek és társaságoknak. Adj fel hirdetéseket,
            szervezd a kimenőket, vezesd a naplót, és értesülj minden újdonságról a magyar vadász közösségre szabott hírfolyamunkon.
          </p>
          <div className="flex flex-wrap gap-3">
            <Button asChild size="lg" className="bg-amber-400 text-black">
              <Link to="/ads">Aktív hirdetések böngészése</Link>
            </Button>
            <Button
              asChild
              variant="outline"
              size="lg"
              className="border-amber-300 text-amber-200 hover:bg-amber-300/10"
            >
              <Link to="/societies">Ismerd meg a társaságokat</Link>
            </Button>
          </div>
        </div>
        <div className="mt-10 max-w-md shrink-0 lg:mt-0 lg:max-w-lg">
          <div className="rounded-2xl border border-amber-300/10 bg-black/20 p-6 shadow-lg backdrop-blur">
            <div className="grid gap-4 sm:grid-cols-2">
              <StatCard label="Aktív hirdetések" value="128" trend="+12 ezen a héten" />
              <StatCard label="Regisztrált társaságok" value="42" trend="4 új ebben a hónapban" />
              <StatCard label="Csatlakozott vadászok" value="2 540" trend="+180 az elmúlt 30 napban" />
              <StatCard label="Közelgő események" value="19" trend="Naponta frissítve" />
            </div>
          </div>
        </div>
        <div className="absolute -left-24 top-1/2 h-80 w-80 -translate-y-1/2 rounded-full bg-amber-400/20 blur-3xl" />
        <div className="absolute -right-16 bottom-0 h-72 w-72 rounded-full bg-amber-500/10 blur-3xl" />
      </section>

      <FeatureGrid />
      <LatestAds />
      <LatestNews />
    </div>
  );
}

function StatCard({
  label,
  value,
  trend,
}: {
  label: string;
  value: string;
  trend: string;
}) {
  return (
    <div className="rounded-xl bg-[#131d13]/80 p-4 shadow-inner shadow-black/30">
      <p className="text-xs uppercase tracking-[0.25em] text-amber-300/70">
        {label}
      </p>
      <p className="mt-2 font-display text-2xl text-amber-100">{value}</p>
      <p className="text-xs text-amber-200/60">{trend}</p>
    </div>
  );
}
