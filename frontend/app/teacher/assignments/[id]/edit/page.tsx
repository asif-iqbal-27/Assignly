"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import type { AssignmentDto } from "@/lib/types";

// Converts an ISO date string from the API into the "YYYY-MM-DDTHH:mm" shape
// a <input type="datetime-local"> needs, in the browser's local time.
function toDatetimeLocal(iso: string): string {
  const date = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export default function EditAssignmentPage() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const [ready, setReady] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const [subjectName, setSubjectName] = useState("");
  const [className, setClassName] = useState("");
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [deadline, setDeadline] = useState("");
  const [maxMarks, setMaxMarks] = useState("");
  const [allowLateSubmission, setAllowLateSubmission] = useState(false);
  const [allowResubmission, setAllowResubmission] = useState(false);

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
      .then((a) => {
        setSubjectName(a.subjectName);
        setClassName(a.className);
        setTitle(a.title);
        setDescription(a.description);
        setDeadline(toDatetimeLocal(a.deadline));
        setMaxMarks(String(a.maxMarks));
        setAllowLateSubmission(a.allowLateSubmission);
        setAllowResubmission(a.allowResubmission);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignment."))
      .finally(() => setLoading(false));
  }, [ready, params.id]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    if (!title || !description || !deadline || !maxMarks) {
      setError("All fields are required.");
      return;
    }

    setSaving(true);
    try {
      await apiFetch(`/api/assignments/${params.id}`, {
        method: "PUT",
        body: JSON.stringify({
          title,
          description,
          deadline: new Date(deadline).toISOString(),
          maxMarks: Number(maxMarks),
          allowLateSubmission,
          allowResubmission,
        }),
      });
      router.push("/teacher");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save assignment.");
      setSaving(false);
    }
  }

  if (!ready) return null;

  return (
    <Shell active="assignments">
      <div className="page-head">
        <div>
          <h1>Edit assignment</h1>
          <p className="page-subtitle">
            {subjectName ? `${subjectName} (${className}) - subject and class can't be changed.` : ""}
          </p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}

      {loading ? (
        <p className="loading-text">Loading assignment...</p>
      ) : (
        <div className="card card-max">
          <form className="form" onSubmit={handleSubmit}>
            <div className="form-row">
              <label htmlFor="title">Title</label>
              <input id="title" value={title} onChange={(e) => setTitle(e.target.value)} />
            </div>
            <div className="form-row">
              <label htmlFor="description">Description</label>
              <textarea id="description" value={description} onChange={(e) => setDescription(e.target.value)} />
            </div>
            <div className="form-row">
              <label htmlFor="deadline">Deadline</label>
              <input
                id="deadline"
                type="datetime-local"
                value={deadline}
                onChange={(e) => setDeadline(e.target.value)}
              />
            </div>
            <div className="form-row">
              <label htmlFor="maxMarks">Max marks</label>
              <input
                id="maxMarks"
                type="number"
                min={1}
                value={maxMarks}
                onChange={(e) => setMaxMarks(e.target.value)}
              />
            </div>
            <div className="form-row form-row-inline">
              <input
                id="allowLateSubmission"
                type="checkbox"
                checked={allowLateSubmission}
                onChange={(e) => setAllowLateSubmission(e.target.checked)}
              />
              <label htmlFor="allowLateSubmission">Allow late submission</label>
            </div>
            <div className="form-row form-row-inline">
              <input
                id="allowResubmission"
                type="checkbox"
                checked={allowResubmission}
                onChange={(e) => setAllowResubmission(e.target.checked)}
              />
              <label htmlFor="allowResubmission">Allow resubmission</label>
            </div>

            <div className="form-actions">
              <button type="submit" disabled={saving}>
                {saving ? "Saving..." : "Save changes"}
              </button>
            </div>
          </form>
        </div>
      )}
    </Shell>
  );
}
