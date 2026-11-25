import type { Metadata } from "next";
import { NoEncontrado } from "@/components/no-encontrado";

export const metadata: Metadata = { title: "Página no encontrada · CI²DT²" };

export default function NotFound() {
  return (
    <main className="min-h-screen bg-slate-50">
      <NoEncontrado />
    </main>
  );
}
