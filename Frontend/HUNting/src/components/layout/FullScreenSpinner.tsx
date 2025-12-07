export function FullScreenSpinner() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-[#0b120b]">
      <div className="size-16 animate-spin rounded-full border-4 border-amber-400/40 border-t-amber-400" />
    </div>
  );
}
