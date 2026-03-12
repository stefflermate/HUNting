import { Outlet } from "react-router-dom";
import { AppHeader } from "@/components/layout/AppHeader";
import { AppFooter } from "@/components/layout/AppFooter";
import { Chatbot } from "@/components/Chatbot";

export function AppLayout() {
  return (
    <div className="flex min-h-screen flex-col relative">
      <AppHeader />
      <main className="flex-1">
        <div className="mx-auto w-full max-w-6xl px-4 pb-16 pt-24">
          <Outlet />
        </div>
      </main>
      <AppFooter />
      <Chatbot />
    </div>
  );
}
