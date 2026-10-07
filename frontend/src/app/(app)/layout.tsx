import { Suspense } from "react";
import { AppShell } from "@/components/app-shell";
import { Cargando } from "@/components/ui";

export default function LayoutAutenticado({ children }: LayoutProps<"/">) {
  return (
    <Suspense fallback={<Cargando />}>
      <AppShell>{children}</AppShell>
    </Suspense>
  );
}
