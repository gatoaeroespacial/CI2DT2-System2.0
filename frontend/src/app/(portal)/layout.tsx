import Image from "next/image";
import Link from "next/link";
import logoCi2dt2 from "@/assets/img/LOGO_CI2DT2.png";
import { contenidoInstitucional as c } from "./contenido-institucional";

/** Layout del portal público institucional (EP-10). */
export default function LayoutPortal({ children }: LayoutProps<"/">) {
  return (
    <div className="flex min-h-screen flex-col bg-white text-slate-900">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-3">
          <Link href="/" className="flex items-center gap-3">
            <Image src={logoCi2dt2} alt="" width={40} height={40} className="h-10 w-10 object-contain" priority />
            <span>
              <span className="block text-base font-semibold text-emerald-900">{c.nombre}</span>
              <span className="block text-xs text-slate-600">{c.institucion}</span>
            </span>
          </Link>
          <nav aria-label="Portal" className="flex flex-wrap items-center gap-4 text-sm">
            <Link href="/#programas" className="text-slate-700 hover:text-emerald-800">Programas</Link>
            <Link href="/portal/proyectos" className="text-slate-700 hover:text-emerald-800">Proyectos</Link>
            <Link href="/#convocatorias" className="text-slate-700 hover:text-emerald-800">Convocatorias</Link>
            <Link href="/login" className="rounded-md bg-emerald-800 px-3 py-1.5 font-medium text-white hover:bg-emerald-900">Ingresar</Link>
          </nav>
        </div>
      </header>
      <main className="flex-1">{children}</main>
      <footer className="border-t border-slate-200 bg-slate-50">
        <div className="mx-auto flex max-w-6xl flex-wrap justify-between gap-4 px-4 py-6 text-sm text-slate-600">
          <p>{c.nombreCompleto} · {c.institucion}</p>
          <div className="flex flex-wrap gap-4">
            {c.contacto.correo && <a href={`mailto:${c.contacto.correo}`} className="hover:underline">{c.contacto.correo}</a>}
            {c.contacto.telefono && <span>{c.contacto.telefono}</span>}
            <Link href="/politica-datos" className="hover:underline">Política de tratamiento de datos</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
