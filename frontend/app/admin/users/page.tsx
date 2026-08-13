"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import Shell from "@/components/Shell";
import { apiFetch, ApiError } from "@/lib/api";
import { badgeClass } from "@/lib/badge";
import type { ClassDto, Role, UserDto } from "@/lib/types";

export default function AdminUsersPage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [users, setUsers] = useState<UserDto[]>([]);
  const [classes, setClasses] = useState<ClassDto[]>([]);
  const [loadingList, setLoadingList] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [saving, setSaving] = useState(false);

  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");
  const [role, setRole] = useState<Role>("Student");
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
    loadUsers();
    apiFetch<ClassDto[]>("/api/classes").then(setClasses).catch(() => setClasses([]));
  }, [ready]);

  function loadUsers() {
    setLoadingList(true);
    apiFetch<UserDto[]>("/api/users")
      .then(setUsers)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Failed to load users."))
      .finally(() => setLoadingList(false));
  }

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!userName || !email || !password || !fullName) {
      setError("All fields are required.");
      return;
    }
    if (role === "Student" && !classId) {
      setError("A class is required for a Student.");
      return;
    }

    setSaving(true);
    try {
      await apiFetch<UserDto>("/api/users", {
        method: "POST",
        body: JSON.stringify({
          userName,
          email,
          password,
          fullName,
          role,
          classId: role === "Student" ? classId : null,
        }),
      });

      setUserName("");
      setEmail("");
      setPassword("");
      setFullName("");
      setRole("Student");
      setClassId("");
      setSuccess(`User "${userName}" created.`);
      loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to create user.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDeactivate(u: UserDto) {
    setError("");
    setSuccess("");
    try {
      await apiFetch(`/api/users/${u.id}/deactivate`, { method: "PATCH" });
      setSuccess(`"${u.userName}" deactivated.`);
      loadUsers();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to deactivate user.");
    }
  }

  if (!ready) return null;

  return (
    <Shell active="users">
      <div className="page-head">
        <div>
          <h1>Users</h1>
          <p className="page-subtitle">Create Teacher and Student accounts. Admins seed the first login.</p>
        </div>
      </div>

      {error && <div className="banner banner-error">{error}</div>}
      {success && <div className="banner banner-success">{success}</div>}

      <div className="card card-max">
        <h2 style={{ marginTop: 0 }}>Create user</h2>
        <form className="form" onSubmit={handleCreate}>
          <div className="form-row">
            <label htmlFor="userName">Username</label>
            <input id="userName" value={userName} onChange={(e) => setUserName(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="email">Email</label>
            <input id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>
          <div className="form-row">
            <label htmlFor="fullName">Full name</label>
            <input id="fullName" value={fullName} onChange={(e) => setFullName(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="role">Role</label>
            <select id="role" value={role} onChange={(e) => setRole(e.target.value as Role)}>
              <option value="Admin">Admin</option>
              <option value="Teacher">Teacher</option>
              <option value="Student">Student</option>
            </select>
          </div>
          {role === "Student" && (
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
          )}
          <div className="form-actions">
            <button type="submit" disabled={saving}>
              {saving ? "Creating..." : "Create user"}
            </button>
          </div>
        </form>
      </div>

      <h2>All users</h2>
      {loadingList ? (
        <p className="loading-text">Loading users...</p>
      ) : (
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>Username</th>
                <th>Full name</th>
                <th>Email</th>
                <th>Role</th>
                <th>Class</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id}>
                  <td>{u.userName}</td>
                  <td>{u.fullName}</td>
                  <td>{u.email}</td>
                  <td>{u.role}</td>
                  <td>{u.className ?? "-"}</td>
                  <td>
                    <span className={badgeClass(u.isActive ? "Active" : "Inactive")}>
                      {u.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  <td>
                    {u.isActive && (
                      <button type="button" className="danger" onClick={() => handleDeactivate(u)}>
                        Deactivate
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {users.length === 0 && (
                <tr className="empty-row">
                  <td colSpan={7}>No users yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </Shell>
  );
}
