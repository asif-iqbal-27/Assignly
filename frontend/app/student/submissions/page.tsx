"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import { badgeClass } from "@/lib/badge";
import type { SubmissionDto } from "@/lib/types";

export default function StudentSubmissionsPage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [submissions, setSubmissions] = useState<SubmissionDto[]>([]);
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
    apiFetch<SubmissionDto[]>("/api/submissions/mine")
      .then(setSubmissions)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load submissions."))
      .finally(() => setLoading(false));
  }, [ready]);

  if (!ready) return null;

  return (
    <Shell active="submissions">
      <div className="page-head">
        <div>
          <h1>My submissions</h1>
          <p className="page-subtitle">Latest attempt per assignment, with marks and feedback once graded.</p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}

      {loading ? (
        <p className="loading-text">Loading submissions...</p>
      ) : (
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>Assignment</th>
                <th>Submitted</th>
                <th>Attempt</th>
                <th>Status</th>
                <th>Marks</th>
                <th>Feedback</th>
              </tr>
            </thead>
            <tbody>
              {submissions.map((s) => (
                <tr key={s.id}>
                  <td>{s.assignmentTitle}</td>
                  <td>{new Date(s.submittedAt).toLocaleString()}</td>
                  <td>{s.attemptNumber}</td>
                  <td>
                    <span className={badgeClass(s.status)}>{s.status}</span>{" "}
                    {s.isLate && <span className="badge badge-amber">Late</span>}
                  </td>
                  <td>{s.marks ?? "-"}</td>
                  <td>{s.feedback ?? "-"}</td>
                </tr>
              ))}
              {submissions.length === 0 && (
                <tr className="empty-row">
                  <td colSpan={6}>No submissions yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </Shell>
  );
}
