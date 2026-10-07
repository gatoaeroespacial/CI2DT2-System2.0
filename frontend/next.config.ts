import type { NextConfig } from "next";

// URL de la API .NET (MonsterASP). El navegador solo habla con el dominio del frontend:
// /api/v1/* se reenvía a la API, así la cookie HttpOnly del token de refresco es del
// mismo sitio y puede usar SameSite=Strict.
const apiUrl = process.env.API_URL ?? "http://localhost:5132";

const nextConfig: NextConfig = {
  poweredByHeader: false,
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
  // El portal público llega en el Sprint 4; por ahora la raíz lleva al inicio de sesión.
  async redirects() {
    return [{ source: "/", destination: "/login", permanent: false }];
  },
  async rewrites() {
    return [{ source: "/api/v1/:path*", destination: `${apiUrl}/api/v1/:path*` }];
  },
};

export default nextConfig;
