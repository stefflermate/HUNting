import { Suspense } from "react";
import { RouterProvider } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "@/context/AuthContext";
import { queryClient } from "@/lib/queryClient";
import { appRouter } from "@/routes/appRouter";
import { FullScreenSpinner } from "@/components/layout/FullScreenSpinner";

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <Suspense fallback={<FullScreenSpinner />}>
          <RouterProvider router={appRouter} />
        </Suspense>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
