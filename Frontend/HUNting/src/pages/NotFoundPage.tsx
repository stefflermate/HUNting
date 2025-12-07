import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

export function NotFoundPage() {
  return (
    <div className="mx-auto max-w-lg space-y-6 rounded-3xl border border-amber-200/10 bg-[#121b12]/95 p-10 text-center shadow-xl">
      <h1 className="font-display text-5xl text-amber-100">404</h1>
      <p className="text-lg text-amber-200/80">
        We could not find the trail you were tracking. The page might have been
        removed or moved under a new route.
      </p>
      <Button asChild className="bg-amber-400 text-black hover:bg-amber-300">
        <Link to="/">Vissza a kezdőlapra</Link>
      </Button>
    </div>
  );
}

