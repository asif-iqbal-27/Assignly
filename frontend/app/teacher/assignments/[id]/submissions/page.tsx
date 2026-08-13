"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import type { AssignmentDto, SubmissionDto, SubmissionStatus } from "@/lib/types";

interface GradeInput {
  marks: string;
  feedback: string;
}

const STATUS_OPTIONS: SubmissionStatus[] = ["Submitted", "Late", "UnderReview", "Graded"];

export default function AssignmentSubmissionsPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const [ready, setReady] = useState(false);
  const [assignment, setAssignment] = useState<AssignmentDto | null>(null);
  const [submissions, setSubmissions] = useState<SubmissionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  // One entry per submission id, holding whatever is currently typed into
  // that row's marks/feedback inputs.
  const [grades, setGrades] = useState<Record<string, GradeInput>>({});
  const [statuses, setStatuses] = useState<Record<string, SubmissionStatus>>({});

  useEffect(() => {
    if (localStorage.getItem("role") !== "Teacher") {
      router.replace("/login");
      return;
    }
    setReady(true);
  }, [router]);

  useEffect(() => {
    if (!ready) return;
    apiFetch<AssignmentDto>(`/api/assignments/${params.id}`)
      .then(setAssignment)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignment."));
    loadSubmissions();
  }, [ready, params.id]);

  function loadSubmissions() {
    setLoading(true);
    apiFetch<SubmissionDto[]>(`/api/assignments/${params.id}/submissions`)
      .then((data) => {
        setSubmissions(data);
        const nextGrades: Record<string, GradeInput> = {};
        const nextStatuses: Record<string, SubmissionStatus> = {};
        for (const s of data) {
          nextGrades[s.id] = { marks: s.marks?.toString() ?? "", feedback: s.feedback ?? "" };
          nextStatuses[s.id] = s.status;
        }
        setGrades(nextGrades);
        setStatuses(nextStatuses);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load submissions."))
      .finally(() => setLoading(false));
  }

  async function handleGrade(s: SubmissionDto) {
    setError("");
    setSuccess("");
    const input = grades[s.id];
    if (!input || input.marks === "") {
      setError("Enter marks before saving.");
      return;
    }
    try {
      await apiFetch(`/api/submissions/${s.id}/grade`, {
        method: "PATCH",
        body: JSON.stringify({ marks: Number(input.marks), feedback: input.feedback || null }),
      });
      setSuccess(`Grade saved for ${s.studentName}.`);
      loadSubmissions();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save grade.");
    }
  }

  async function handleStatusSave(s: SubmissionDto) {
    setError("");
    setSuccess("");
    try {
      await apiFetch(`/api/submissions/${s.id}/status`, {
        method: "PATCH",
        body: JSON.stringify({ status: statuses[s.id] }),
      });
      setSuccess(`Status updated for ${s.studentName}.`);
      loadSubmissions();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to update status.");
    }
  }

  if (!ready) return null;

  return (
    <Shell active="assignments">
      <div className="page-head">
        <div>
          <h1>Submissions{assignment ? ` - ${assignment.title}` : ""}</h1>
          {assignment && <p className="page-subtitle">Max marks: {assignment.maxMarks}</p>}
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}
      {success && <div className="banner banner-success">{success}</div>}

      {loading ? (
        <p className="loading-text">Loading submissions...</p>
      ) : (
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>Student</th>
                <th>Content</th>
                <th>Submitted</th>
                <th>Late</th>
                <th>Attempt</th>
                <th>Status</th>
                <th>Marks / Feedback</th>
              </tr>
            </thead>
            <tbody>
              {submissions.map((s) => (
                <tr key={s.id}>
                  <td>{s.studentName}</td>
                  <td style={{ maxWidth: 240 }}>{s.content}</td>
                  <td>{new Date(s.submittedAt).toLocaleString()}</td>
                  <td>{s.isLate ? "Yes" : "No"}</td>
                  <td>{s.attemptNumber}</td>
                  <td>
                    <div className="actions">
                      <select
                        value={statuses[s.id] ?? s.status}
                        onChange={(e) =>
                          setStatuses((prev) => ({ ...prev, [s.id]: e.target.value as SubmissionStatus }))
                        }
                      >
                        {STATUS_OPTIONS.map((opt) => (
                          <option key={opt} value={opt}>
                            {opt}
                          </option>
                        ))}
                      </select>
                      <button type="button" className="secondary" onClick={() => handleStatusSave(s)}>
                        Save
                      </button>
                    </div>
                  </td>
                  <td>
                    <div className="form-row">
                      <input
                        type="number"
                        min={0}
                        max={assignment?.maxMarks}
                        placeholder="Marks"
                        value={grades[s.id]?.marks ?? ""}
                        onChange={(e) =>
                          setGrades((prev) => ({
                            ...prev,
                            [s.id]: { ...prev[s.id], marks: e.target.value, feedback: prev[s.id]?.feedback ?? "" },
                          }))
                        }
                      />
                      <textarea
                        placeholder="Feedback"
                        value={grades[s.id]?.feedback ?? ""}
                        onChange={(e) =>
                          setGrades((prev) => ({
                            ...prev,
                            [s.id]: { ...prev[s.id], feedback: e.target.value, marks: prev[s.id]?.marks ?? "" },
                          }))
                        }
                      />
                      <button type="button" onClick={() => handleGrade(s)}>
                        Save grade
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
              {submissions.length === 0 && (
                <tr className="empty-row">
                  <td colSpan={7}>No submissions yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </Shell>
  );
}
