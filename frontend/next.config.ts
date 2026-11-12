import type { NextConfig } from "next";

// URL de la API .NET (MonsterASP). El navegador solo habla con el dominio del frontend:
// /api/v1/* se reenvía a la API, así la cookie HttpOnly del token de refresco es del
// mismo sitio y puede usar SameSite=Strict.
const apiUrl = process.env.API_URL ?? "http://localhost:5132";

const nextConfig: NextConfig = {
  poweredByHeader: false,
  experimental: {
    agentUpgrade: "security",
  },
  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
  async rewrites() {
    return [{ source: "/api/v1/:path*", destination: `${apiUrl}/api/v1/:path*` }];
  },
};

export default nextConfig;
