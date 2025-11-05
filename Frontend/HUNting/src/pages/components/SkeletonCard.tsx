export function SkeletonCard() {
  return (
    <div className="animate-pulse rounded-2xl border border-amber-200/5 bg-[#101710]/80 p-6">
      <div className="h-5 w-2/3 rounded bg-amber-200/20" />
      <div className="mt-4 space-y-3">
        <div className="h-3 rounded bg-amber-200/10" />
        <div className="h-3 w-5/6 rounded bg-amber-200/10" />
        <div className="h-3 w-4/6 rounded bg-amber-200/10" />
      </div>
      <div className="mt-6 flex justify-between">
        <div className="h-3 w-1/3 rounded bg-amber-200/10" />
        <div className="h-3 w-1/4 rounded bg-amber-200/10" />
      </div>
      <div className="mt-6 h-9 w-32 rounded bg-amber-200/15" />
    </div>
  );
}
