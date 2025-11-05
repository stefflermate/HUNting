import { Shield, Map, Megaphone, Users } from "lucide-react";

const features = [
  {
    icon: Shield,
    title: "Szerepkör alapú biztonság",
    description:
      "Az Identity-alapú folyamatok különválasztják az admin, a társaság és a vadász feladatait, így minden átlátható és ellenőrizhető.",
  },
  {
    icon: Map,
    title: "Interaktív területtérkép",
    description:
      "Rajzolj területhatárokat, útvonalakat és jelölőket, hogy valós időben tudd koordinálni a terepi munkát.",
  },
  {
    icon: Megaphone,
    title: "Hatékony hirdetéskezelés",
    description:
      "Tegyük közzé a vezetett vadászatokat, felszerelések listáit vagy vendégvadász-meghívókat képekkel és időzítésekkel.",
  },
  {
    icon: Users,
    title: "Közösség az első",
    description:
      "A tagok megoszthatják vadásznaplóikat, hozzászólhatnak a hírekhez, és mindenki időben értesül a közös kimenőkről.",
  },
];

export function FeatureGrid() {
  return (
    <section className="rounded-3xl border border-amber-200/10 bg-black/20 p-10 shadow-xl backdrop-blur">
      <header className="mx-auto mb-12 max-w-2xl text-center">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">
          A platform alappillérei
        </p>
        <h2 className="mt-3 font-display text-3xl text-amber-100">
          Modern vadásztársaságoknak és elhivatott vendégeknek készült
        </h2>
        <p className="mt-4 text-amber-200/80">
          Kezeld a tagságot és az adminisztrációt, tegyél közzé lehetőségeket, és tájékoztass mindenkit egyetlen elegáns felületen.
        </p>
      </header>
      <div className="grid gap-6 md:grid-cols-2">
        {features.map((feature) => (
          <article
            key={feature.title}
            className="group flex gap-4 rounded-2xl border border-amber-300/10 bg-[#131d13]/80 p-6 transition hover:border-amber-400/40 hover:bg-[#1b291c]"
          >
            <div className="flex size-12 shrink-0 items-center justify-center rounded-full bg-amber-400/10 text-amber-300 transition group-hover:bg-amber-400/20">
              <feature.icon className="size-6" />
            </div>
            <div>
              <h3 className="font-display text-xl text-amber-100">
                {feature.title}
              </h3>
              <p className="mt-2 text-sm text-amber-200/80">
                {feature.description}
              </p>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
