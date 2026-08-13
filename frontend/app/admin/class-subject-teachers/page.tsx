"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import type { ClassSubjectTeacherDto, SubjectDto, UserDto } from "@/lib/types";

export default function AdminClassSubjectTeachersPage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [teachers, setTeachers] = useState<UserDto[]>([]);
  const [subjects, setSubjects] = useState<SubjectDto[]>([]);
  const [assignments, setAssignments] = useState<ClassSubjectTeacherDto[]>([]);
  const [loadingAssignments, setLoadingAssignments] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [saving, setSaving] = useState(false);

  const [newTeacherId, setNewTeacherId] = useState("");
  const [newSubjectId, setNewSubjectId] = useState("");
  const [viewTeacherId, setViewTeacherId] = useState("");

  useEffect(() => {
    if (localStorage.getItem("role") !== "Admin") {
      router.replace("/login");
      return;
    }
    setReady(true);
  }, [router]);

  useEffect(() => {
    if (!ready) return;
    apiFetch<UserDto[]>("/api/users")
      .then((users) => setTeachers(users.filter((u) => u.role === "Teacher")))
      .catch(() => setTeachers([]));
    apiFetch<SubjectDto[]>("/api/subjects").then(setSubjects).catch(() => setSubjects([]));
  }, [ready]);

  useEffect(() => {
    if (!viewTeacherId) {
      setAssignments([]);
      return;
    }
    loadAssignments(viewTeacherId);
  }, [viewTeacherId]);

  function loadAssignments(teacherId: string) {
    setLoadingAssignments(true);
    apiFetch<ClassSubjectTeacherDto[]>(`/api/class-subject-teachers/teacher/${teacherId}`)
      .then(setAssignments)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load assignments."))
      .finally(() => setLoadingAssignments(false));
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!newTeacherId || !newSubjectId) {
      setError("Select a teacher and a subject.");
      return;
    }

    setSaving(true);
    try {
      await apiFetch<ClassSubjectTeacherDto>("/api/class-subject-teachers", {
        method: "POST",
        body: JSON.stringify({ teacherId: newTeacherId, subjectId: newSubjectId }),
      });

      setSuccess("Teacher assigned to subject.");
      if (viewTeacherId === newTeacherId) {
        loadAssignments(newTeacherId);
      }
      setNewSubjectId("");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create assignment.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id: string) {
    setError("");
    setSuccess("");
    try {
      await apiFetch(`/api/class-subject-teachers/${id}`, { method: "DELETE" });
      setSuccess("Assignment removed.");
      loadAssignments(viewTeacherId);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete assignment.");
    }
  }

  if (!ready) return null;

  return (
    <Shell active="teachers">
      <div className="page-head">
        <div>
          <h1>Teacher Assignments</h1>
          <p className="page-subtitle">
            Assigns a teacher to a subject. A teacher can only manage assignments for subjects they're assigned to
            here.
          </p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}
      {success && <div className="banner banner-success">{success}</div>}

      <div className="card card-max">
        <h2 style={{ marginTop: 0 }}>Assign teacher to subject</h2>
        <form className="form" onSubmit={handleCreate}>
          <div className="form-row">
            <label htmlFor="newTeacherId">Teacher</label>
            <select id="newTeacherId" value={newTeacherId} onChange={(e) => setNewTeacherId(e.target.value)}>
              <option value="">Select a teacher</option>
              {teachers.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.fullName} ({t.userName})
                </option>
              ))}
            </select>
          </div>
          <div className="form-row">
            <label htmlFor="newSubjectId">Subject</label>
            <select id="newSubjectId" value={newSubjectId} onChange={(e) => setNewSubjectId(e.target.value)}>
              <option value="">Select a subject</option>
              {subjects.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name} - {s.className}
                </option>
              ))}
            </select>
          </div>
          <div className="form-actions">
            <button type="submit" disabled={saving}>
              {saving ? "Saving..." : "Assign"}
            </button>
          </div>
        </form>
      </div>

      <h2>View assignments for a teacher</h2>
      <div className="card card-max">
        <div className="form-row">
          <label htmlFor="viewTeacherId">Teacher</label>
          <select id="viewTeacherId" value={viewTeacherId} onChange={(e) => setViewTeacherId(e.target.value)}>
            <option value="">Select a teacher</option>
            {teachers.map((t) => (
              <option key={t.id} value={t.id}>
                {t.fullName} ({t.userName})
              </option>
            ))}
          </select>
        </div>
      </div>

      {viewTeacherId &&
        (loadingAssignments ? (
          <p className="loading-text">Loading assignments...</p>
        ) : (
          <div className="table-card">
            <table>
              <thead>
                <tr>
                  <th>Subject</th>
                  <th>Class</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {assignments.map((a) => (
                  <tr key={a.id}>
                    <td>{a.subjectName}</td>
                    <td>{subjects.find((s) => s.id === a.subjectId)?.className ?? "-"}</td>
                    <td>
                      <button type="button" className="danger" onClick={() => handleDelete(a.id)}>
                        Remove
                      </button>
                    </td>
                  </tr>
                ))}
                {assignments.length === 0 && (
                  <tr className="empty-row">
                    <td colSpan={3}>No subjects assigned to this teacher yet.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        ))}
    </Shell>
  );
}
