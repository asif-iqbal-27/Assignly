"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import type { AssignmentDto } from "@/lib/types";

export default function StudentAssignmentsPage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [assignments, setAssignments] = useState<AssignmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (localStorage.getItem("role") !== "Student") {
      router.replace("/login");
      return;
    }
    setReady(true);
  }, [router]);

  useEffect(() => {
    if (!ready) return;
    apiFetch<AssignmentDto[]>("/api/assignments")
      .then(setAssignments)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignments."))
      .finally(() => setLoading(false));
  }, [ready]);

  if (!ready) return null;

  return (
    <Shell active="assignments">
      <div className="page-head">
        <div>
          <h1>Assignments</h1>
          <p className="page-subtitle">Published assignments for your class.</p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}

      {loading ? (
        <p className="loading-text">Loading assignments...</p>
      ) : (
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Subject</th>
                <th>Deadline</th>
                <th>Max marks</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {assignments.map((a) => (
                <tr key={a.id}>
                  <td>{a.title}</td>
                  <td>{a.subjectName}</td>
                  <td>{new Date(a.deadline).toLocaleString()}</td>
                  <td>{a.maxMarks}</td>
                  <td>
                    <Link href={`/student/assignments/${a.id}`}>
                      <button type="button" className="secondary">
                        View
                      </button>
                    </Link>
                  </td>
                </tr>
              ))}
              {assignments.length === 0 && (
                <tr className="empty-row">
                  <td colSpan={5}>No assignments published yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </Shell>
  );
}
