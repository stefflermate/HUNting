export function AppFooter() {
  return (
    <footer className="mt-16 border-t border-amber-300/10 bg-[#0b120b]/90">
      <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 px-4 py-10 text-sm text-amber-200/70 md:flex-row md:items-center md:justify-between">
        <div>
          <p className="font-display text-lg text-amber-200">HUNting Portal</p>
          <p className="text-xs uppercase tracking-[0.3em] text-amber-200/50">
            Magyar vadász közösségnek építve
          </p>
        </div>
        <p>&copy; {new Date().getFullYear()} HUNting. Minden jog fenntartva.</p>
      </div>
    </footer>
  );
}
