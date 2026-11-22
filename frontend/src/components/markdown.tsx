"use client";

import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";

type MarkdownProps = {
  contenido: string;
  className?: string;
};

export function Markdown({ contenido, className = "" }: MarkdownProps) {
  return (
    <div className={`markdown-content ${className}`}>
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          h1: ({ children }) => <h1 className="mb-3 mt-4 text-lg font-bold first:mt-0">{children}</h1>,
          h2: ({ children }) => <h2 className="mb-2 mt-3 text-base font-bold first:mt-0">{children}</h2>,
          h3: ({ children }) => <h3 className="mb-2 mt-3 text-sm font-bold first:mt-0">{children}</h3>,
          p: ({ children }) => <p className="mb-2 last:mb-0">{children}</p>,
          ul: ({ children }) => <ul className="mb-2 list-disc space-y-1 pl-5 last:mb-0">{children}</ul>,
          ol: ({ children }) => <ol className="mb-2 list-decimal space-y-1 pl-5 last:mb-0">{children}</ol>,
          li: ({ children }) => <li className="pl-1">{children}</li>,
          blockquote: ({ children }) => (
            <blockquote className="mb-2 border-l-4 border-emerald-300 pl-3 italic last:mb-0">{children}</blockquote>
          ),
          strong: ({ children }) => <strong className="font-bold">{children}</strong>,
          em: ({ children }) => <em className="italic">{children}</em>,
          code: ({ children, className: codeClassName }) =>
            codeClassName ? (
              <code className="block overflow-x-auto rounded-lg bg-slate-900 p-3 text-xs text-slate-100">{children}</code>
            ) : (
              <code className="rounded bg-slate-200 px-1 py-0.5 text-xs">{children}</code>
            ),
          pre: ({ children }) => <pre className="mb-2 overflow-x-auto last:mb-0">{children}</pre>,
          table: ({ children }) => (
            <div className="mb-3 overflow-x-auto last:mb-0">
              <table className="w-full border-collapse text-left">{children}</table>
            </div>
          ),
          thead: ({ children }) => <thead className="bg-slate-100">{children}</thead>,
          th: ({ children }) => <th className="border border-slate-300 px-2 py-1 font-bold">{children}</th>,
          td: ({ children }) => <td className="border border-slate-300 px-2 py-1 align-top">{children}</td>,
          hr: () => <hr className="my-3 border-slate-300" />,
          a: ({ href, children }) => (
            <a href={href} target="_blank" rel="noreferrer" className="underline underline-offset-2">
              {children}
            </a>
          ),
        }}
      >
        {contenido}
      </ReactMarkdown>
    </div>
  );
}
