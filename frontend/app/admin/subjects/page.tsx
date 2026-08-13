"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import type { ClassDto, SubjectDto } from "@/lib/types";

export default function AdminSubjectsPage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [subjects, setSubjects] = useState<SubjectDto[]>([]);
  const [classes, setClasses] = useState<ClassDto[]>([]);
  const [loadingList, setLoadingList] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [saving, setSaving] = useState(false);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [classId, setClassId] = useState("");

  useEffect(() => {
    if (localStorage.getItem("role") !== "Admin") {
      router.replace("/login");
      return;
    }
    setReady(true);
  }, [router]);

  useEffect(() => {
    if (!ready) return;
    loadSubjects();
    apiFetch<ClassDto[]>("/api/classes").then(setClasses).catch(() => setClasses([]));
  }, [ready]);

  function loadSubjects() {
    setLoadingList(true);
    apiFetch<SubjectDto[]>("/api/subjects")
      .then(setSubjects)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load subjects."))
      .finally(() => setLoadingList(false));
  }

  function startEdit(s: SubjectDto) {
    setEditingId(s.id);
    setName(s.name);
    setClassId(s.classId);
    setSuccess("");
  }

  function resetForm() {
    setEditingId(null);
    setName("");
    setClassId("");
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!name || !classId) {
      setError("Name and class are required.");
      return;
    }

    setSaving(true);
    try {
      const body = JSON.stringify({ name, classId });
      if (editingId) {
        await apiFetch<SubjectDto>(`/api/subjects/${editingId}`, { method: "PUT", body });
        setSuccess(`"${name}" updated.`);
      } else {
        await apiFetch<SubjectDto>("/api/subjects", { method: "POST", body });
        setSuccess(`"${name}" created.`);
      }
      resetForm();
      loadSubjects();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save subject.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(s: SubjectDto) {
    setError("");
    setSuccess("");
    try {
      await apiFetch(`/api/subjects/${s.id}`, { method: "DELETE" });
      setSuccess(`"${s.name}" deleted.`);
      loadSubjects();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete subject.");
    }
  }

  if (!ready) return null;

  return (
    <Shell active="subjects">
      <div className="page-head">
        <div>
          <h1>Subjects</h1>
          <p className="page-subtitle">Each subject belongs to exactly one class.</p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}
      {success && <div className="banner banner-success">{success}</div>}

      <div className="card card-max">
        <h2 style={{ marginTop: 0 }}>{editingId ? "Edit subject" : "Create subject"}</h2>
        <form className="form" onSubmit={handleSubmit}>
          <div className="form-row">
            <label htmlFor="name">Name</label>
            <input id="name" value={name} onChange={(e) => setName(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="classId">Class</label>
            <select id="classId" value={classId} onChange={(e) => setClassId(e.target.value)}>
              <option value="">Select a class</option>
              {classes.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name} {c.section ?? ""}
                </option>
              ))}
            </select>
          </div>
          <div className="form-actions">
            <button type="submit" disabled={saving}>
              {saving ? "Saving..." : editingId ? "Save changes" : "Create subject"}
            </button>
            {editingId && (
              <button type="button" className="secondary" onClick={resetForm}>
                Cancel
              </button>
            )}
          </div>
        </form>
      </div>

      <h2>All subjects</h2>
      {loadingList ? (
        <p className="loading-text">Loading subjects...</p>
      ) : (
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Class</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {subjects.map((s) => (
                <tr key={s.id}>
                  <td>{s.name}</td>
                  <td>{s.className}</td>
                  <td className="actions">
                    <button type="button" className="secondary" onClick={() => startEdit(s)}>
                      Edit
                    </button>
                    <button type="button" className="danger" onClick={() => handleDelete(s)}>
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
              {subjects.length === 0 && (
                <tr className="empty-row">
                  <td colSpan={3}>No subjects yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </Shell>
  );
}
