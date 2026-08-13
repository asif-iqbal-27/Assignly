"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import type { SubjectDto } from "@/lib/types";

export default function NewAssignmentPage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [subjects, setSubjects] = useState<SubjectDto[]>([]);
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [subjectId, setSubjectId] = useState("");
  const [deadline, setDeadline] = useState("");
  const [maxMarks, setMaxMarks] = useState("100");
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
    if (ready) apiFetch<SubjectDto[]>("/api/subjects").then(setSubjects).catch(() => setSubjects([]));
  }, [ready]);

  const selectedSubject = subjects.find((s) => s.id === subjectId);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    if (!title || !description || !subjectId || !deadline || !maxMarks) {
      setError("All fields are required.");
      return;
    }

    setSaving(true);
    try {
      await apiFetch("/api/assignments", {
        method: "POST",
        body: JSON.stringify({
          title,
          description,
          subjectId,
          classId: selectedSubject?.classId,
          deadline: new Date(deadline).toISOString(),
          maxMarks: Number(maxMarks),
          allowLateSubmission,
          allowResubmission,
        }),
      });
      router.push("/teacher");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create assignment.");
      setSaving(false);
    }
  }

  if (!ready) return null;

  return (
    <Shell active="assignments">
      <div className="page-head">
        <div>
          <h1>Create assignment</h1>
          <p className="page-subtitle">New assignments are created as Draft. Publish them when ready.</p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}

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
            <label htmlFor="subjectId">Subject</label>
            <select id="subjectId" value={subjectId} onChange={(e) => setSubjectId(e.target.value)}>
              <option value="">Select a subject</option>
              {subjects.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name} - {s.className}
                </option>
              ))}
            </select>
          </div>
          {selectedSubject && <p className="info">Class: {selectedSubject.className}</p>}
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
              {saving ? "Creating..." : "Create assignment"}
            </button>
          </div>
        </form>
      </div>
    </Shell>
  );
}
