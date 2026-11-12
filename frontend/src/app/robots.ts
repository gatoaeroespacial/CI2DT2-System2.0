import type { MetadataRoute } from "next";
import { URL_SITIO } from "@/lib/portal";

/** Solo el portal público se indexa; la aplicación interna y la API no. */
export default function robots(): MetadataRoute.Robots {
  return {
    rules: { userAgent: "*", allow: ["/", "/portal/", "/politica-datos"], disallow: ["/api/", "/login"] },
    sitemap: `${URL_SITIO}/sitemap.xml`,
  };
}
