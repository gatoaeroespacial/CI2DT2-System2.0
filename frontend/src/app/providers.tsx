"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState, type ReactNode } from "react";
import { Toaster } from "sonner";
import { ApiError } from "@/lib/api";

export function Providers({ children }: { children: ReactNode }) {
  const [cliente] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 30_000,
            refetchOnWindowFocus: false,
            // No se reintentan los errores de autorización ni de validación.
            retry: (intentos, error) => !(error instanceof ApiError && error.status < 500) && intentos < 2,
          },
        },
      }),
  );

  return (
    <QueryClientProvider client={cliente}>
      {children}
      <Toaster richColors position="top-right" />
    </QueryClientProvider>
  );
}
