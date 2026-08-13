"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import { badgeClass } from "@/lib/badge";
import type { AssignmentDto, SubmissionDto } from "@/lib/types";

export default function StudentAssignmentDetailPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const [ready, setReady] = useState(false);
  const [assignment, setAssignment] = useState<AssignmentDto | null>(null);
  const [mySubmission, setMySubmission] = useState<SubmissionDto | null>(null);
  const [content, setContent] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (localStorage.getItem("role") !== "Student") {
      router.replace("/login");
      return;
    }
    setReady(true);
  }, [router]);

  useEffect(() => {
    if (!ready) return;
    load();
  }, [ready, params.id]);

  async function load() {
    setError("");
    try {
      const [a, mine] = await Promise.all([
        apiFetch<AssignmentDto>(`/api/assignments/${params.id}`),
        apiFetch<SubmissionDto[]>("/api/submissions/mine"),
      ]);
      setAssignment(a);
      const existing = mine.find((s) => s.assignmentId === params.id) ?? null;
      setMySubmission(existing);
      setContent(existing?.content ?? "");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load assignment.");
    } finally {
      setLoading(false);
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!content) {
      setError("Content is required.");
      return;
    }

    setSaving(true);
    try {
      if (mySubmission) {
        await apiFetch(`/api/submissions/${mySubmission.id}`, {
          method: "PUT",
          body: JSON.stringify({ content }),
        });
        setSuccess("Resubmitted successfully.");
      } else {
        await apiFetch(`/api/assignments/${params.id}/submissions`, {
          method: "POST",
          body: JSON.stringify({ content }),
        });
        setSuccess("Submitted successfully.");
      }
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to submit.");
    } finally {
      setSaving(false);
    }
  }

  if (!ready) return null;

  if (loading) {
    return (
      <Shell active="assignments">
        <p className="loading-text">Loading assignment...</p>
      </Shell>
    );
  }

  if (!assignment) {
    return (
      <Shell active="assignments">
        <div className="banner banner-error">{error || "Assignment not found."}</div>
      </Shell>
    );
  }

  const deadlinePassed = new Date() > new Date(assignment.deadline);
  const canResubmit = assignment.allowResubmission && !deadlinePassed;

  return (
    <Shell active="assignments">
      <div className="page-head">
        <div>
          <h1>{assignment.title}</h1>
          <p className="page-subtitle">
            {assignment.subjectName} | Deadline: {new Date(assignment.deadline).toLocaleString()} | Max marks:{" "}
            {assignment.maxMarks}
          </p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}
      {success && <div className="banner banner-success">{success}</div>}

      <div className="card card-max">
        <p>{assignment.description}</p>
        <p className="info">
          Late submission: {assignment.allowLateSubmission ? "allowed" : "not allowed"} | Resubmission:{" "}
          {assignment.allowResubmission ? "allowed" : "not allowed"}
        </p>
      </div>

      {mySubmission && (
        <div className="card card-max">
          <h2 style={{ marginTop: 0 }}>Your submission</h2>
          <p>
            <span className={badgeClass(mySubmission.status)}>{mySubmission.status}</span>{" "}
            {mySubmission.isLate && <span className="badge badge-amber">Late</span>} Attempt #
            {mySubmission.attemptNumber}
          </p>
          <p>Submitted: {new Date(mySubmission.submittedAt).toLocaleString()}</p>
          {mySubmission.marks !== null && (
            <p>
              Marks: {mySubmission.marks} / {assignment.maxMarks}
            </p>
          )}
          {mySubmission.feedback && <p>Feedback: {mySubmission.feedback}</p>}
        </div>
      )}

      {(!mySubmission || canResubmit) && (
        <div className="card card-max">
          <h2 style={{ marginTop: 0 }}>{mySubmission ? "Resubmit" : "Submit"}</h2>
          <form className="form" onSubmit={handleSubmit}>
            <div className="form-row">
              <label htmlFor="content">Content</label>
              <textarea id="content" value={content} onChange={(e) => setContent(e.target.value)} />
            </div>
            <div className="form-actions">
              <button type="submit" disabled={saving}>
                {saving ? "Saving..." : mySubmission ? "Resubmit" : "Submit"}
              </button>
            </div>
          </form>
        </div>
      )}

      {mySubmission && !canResubmit && (
        <p className="info">
          {deadlinePassed
            ? "The deadline has passed, so this submission can no longer be changed."
            : "This assignment does not allow resubmission."}
        </p>
      )}
    </Shell>
  );
}
