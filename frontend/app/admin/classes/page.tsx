"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import type { ClassDto } from "@/lib/types";

export default function AdminClassesPage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [classes, setClasses] = useState<ClassDto[]>([]);
  const [loadingList, setLoadingList] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [saving, setSaving] = useState(false);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [section, setSection] = useState("");
  const [description, setDescription] = useState("");

  useEffect(() => {
    if (localStorage.getItem("role") !== "Admin") {
      router.replace("/login");
      return;
    }
    setReady(true);
  }, [router]);

  useEffect(() => {
    if (ready) loadClasses();
  }, [ready]);

  function loadClasses() {
    setLoadingList(true);
    apiFetch<ClassDto[]>("/api/classes")
      .then(setClasses)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load classes."))
      .finally(() => setLoadingList(false));
  }

  function startEdit(c: ClassDto) {
    setEditingId(c.id);
    setName(c.name);
    setSection(c.section ?? "");
    setDescription(c.description ?? "");
    setSuccess("");
  }

  function resetForm() {
    setEditingId(null);
    setName("");
    setSection("");
    setDescription("");
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!name) {
      setError("Name is required.");
      return;
    }

    setSaving(true);
    try {
      const body = JSON.stringify({ name, section: section || null, description: description || null });
      if (editingId) {
        await apiFetch<ClassDto>(`/api/classes/${editingId}`, { method: "PUT", body });
        setSuccess(`"${name}" updated.`);
      } else {
        await apiFetch<ClassDto>("/api/classes", { method: "POST", body });
        setSuccess(`"${name}" created.`);
      }
      resetForm();
      loadClasses();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to save class.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(c: ClassDto) {
    setError("");
    setSuccess("");
    try {
      await apiFetch(`/api/classes/${c.id}`, { method: "DELETE" });
      setSuccess(`"${c.name}" deleted.`);
      loadClasses();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete class.");
    }
  }

  if (!ready) return null;

  return (
    <Shell active="classes">
      <div className="page-head">
        <div>
          <h1>Classes</h1>
          <p className="page-subtitle">Manage the school's classes.</p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}
      {success && <div className="banner banner-success">{success}</div>}

      <div className="card card-max">
        <h2 style={{ marginTop: 0 }}>{editingId ? "Edit class" : "Create class"}</h2>
        <form className="form" onSubmit={handleSubmit}>
          <div className="form-row">
            <label htmlFor="name">Name</label>
            <input id="name" value={name} onChange={(e) => setName(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="section">Section</label>
            <input id="section" value={section} onChange={(e) => setSection(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="description">Description</label>
            <input id="description" value={description} onChange={(e) => setDescription(e.target.value)} />
          </div>
          <div className="form-actions">
            <button type="submit" disabled={saving}>
              {saving ? "Saving..." : editingId ? "Save changes" : "Create class"}
            </button>
            {editingId && (
              <button type="button" className="secondary" onClick={resetForm}>
                Cancel
              </button>
            )}
          </div>
        </form>
      </div>

      <h2>All classes</h2>
      {loadingList ? (
        <p className="loading-text">Loading classes...</p>
      ) : (
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Section</th>
                <th>Description</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {classes.map((c) => (
                <tr key={c.id}>
                  <td>{c.name}</td>
                  <td>{c.section ?? "-"}</td>
                  <td>{c.description ?? "-"}</td>
                  <td className="actions">
                    <button type="button" className="secondary" onClick={() => startEdit(c)}>
                      Edit
                    </button>
                    <button type="button" className="danger" onClick={() => handleDelete(c)}>
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
              {classes.length === 0 && (
                <tr className="empty-row">
                  <td colSpan={4}>No classes yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </Shell>
  );
}
