import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Assignly",
  description: "Assignment & Submission Management System",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
