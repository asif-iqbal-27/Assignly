"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import { badgeClass } from "@/lib/badge";
import type { AssignmentDto } from "@/lib/types";

export default function TeacherAssignmentsPage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [assignments, setAssignments] = useState<AssignmentDto[]>([]);
  const [loadingList, setLoadingList] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    if (localStorage.getItem("role") !== "Teacher") {
      router.replace("/login");
      return;
    }
    setReady(true);
  }, [router]);

  useEffect(() => {
    if (ready) loadAssignments();
  }, [ready]);

  function loadAssignments() {
    setLoadingList(true);
    apiFetch<AssignmentDto[]>("/api/assignments")
      .then(setAssignments)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignments."))
      .finally(() => setLoadingList(false));
  }

  async function handlePublish(a: AssignmentDto) {
    setError("");
    setSuccess("");
    try {
      await apiFetch(`/api/assignments/${a.id}/publish`, { method: "POST" });
      setSuccess(`"${a.title}" published.`);
      loadAssignments();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to publish assignment.");
    }
  }

  async function handleDelete(a: AssignmentDto) {
    setError("");
    setSuccess("");
    try {
      await apiFetch(`/api/assignments/${a.id}`, { method: "DELETE" });
      setSuccess(`"${a.title}" deleted.`);
      loadAssignments();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete assignment.");
    }
  }

  if (!ready) return null;

  return (
    <Shell active="assignments">
      <div className="page-head">
        <div>
          <h1>My assignments</h1>
          <p className="page-subtitle">Assignments for subjects you're assigned to teach.</p>
        </div>
        <Link href="/teacher/assignments/new">
          <button type="button">Create assignment</button>
        </Link>
      </div>

      {error && <div className="banner banner-error">{error}</div>}
      {success && <div className="banner banner-success">{success}</div>}

      {loadingList ? (
        <p className="loading-text">Loading assignments...</p>
      ) : (
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Subject</th>
                <th>Class</th>
                <th>Deadline</th>
                <th>Max marks</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {assignments.map((a) => (
                <tr key={a.id}>
                  <td>{a.title}</td>
                  <td>{a.subjectName}</td>
                  <td>{a.className}</td>
                  <td>{new Date(a.deadline).toLocaleString()}</td>
                  <td>{a.maxMarks}</td>
                  <td>
                    <span className={badgeClass(a.status)}>{a.status}</span>
                  </td>
                  <td className="actions">
                    <Link href={`/teacher/assignments/${a.id}/edit`}>
                      <button type="button" className="secondary">
                        Edit
                      </button>
                    </Link>
                    <Link href={`/teacher/assignments/${a.id}/submissions`}>
                      <button type="button" className="secondary">
                        Submissions
                      </button>
                    </Link>
                    {a.status === "Draft" && (
                      <button type="button" onClick={() => handlePublish(a)}>
                        Publish
                      </button>
                    )}
                    <button type="button" className="danger" onClick={() => handleDelete(a)}>
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
              {assignments.length === 0 && (
                <tr className="empty-row">
                  <td colSpan={7}>No assignments yet. Create one to get started.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </Shell>
  );
}
