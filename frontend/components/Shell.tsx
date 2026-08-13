"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

interface NavLink {
  key: string;
  href: string;
  label: string;
}

const LINKS_BY_ROLE: Record<string, NavLink[]> = {
  Admin: [
    { key: "users", href: "/admin/users", label: "Users" },
    { key: "classes", href: "/admin/classes", label: "Classes" },
    { key: "subjects", href: "/admin/subjects", label: "Subjects" },
    { key: "teachers", href: "/admin/class-subject-teachers", label: "Teacher Assignments" },
  ],
  Teacher: [{ key: "assignments", href: "/teacher", label: "Assignments" }],
  Student: [
    { key: "assignments", href: "/student", label: "Assignments" },
    { key: "submissions", href: "/student/submissions", label: "My Submissions" },
  ],
};

// The shared header + sidebar chrome for every protected page. It reads the
// logged-in user straight from localStorage (no context/store) purely to
// decide what to display - each page is still responsible for checking the
// role and redirecting before it renders this.
export default function Shell({ active, children }: { active: string; children: React.ReactNode }) {
  const router = useRouter();
  const [role, setRole] = useState<string | null>(null);
  const [userName, setUserName] = useState<string | null>(null);

  useEffect(() => {
    setRole(localStorage.getItem("role"));
    setUserName(localStorage.getItem("userName"));
  }, []);

  function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    localStorage.removeItem("userName");
    router.push("/login");
  }

  const links = role ? LINKS_BY_ROLE[role] ?? [] : [];

  return (
    <div className="shell">
      <header className="shell-header">
        <Link href="/" className="shell-brand">
          Assignly
        </Link>
        <div className="shell-user">
          {userName && (
            <span>
              <strong>{userName}</strong> ({role})
            </span>
          )}
          <button type="button" className="secondary" onClick={logout}>
            Log out
          </button>
        </div>
      </header>

      <nav className="shell-sidebar">
        {links.map((link) => (
          <Link key={link.key} href={link.href} className={link.key === active ? "active" : ""}>
            {link.label}
          </Link>
        ))}
      </nav>

      <main className="shell-main">{children}</main>
    </div>
  );
}
