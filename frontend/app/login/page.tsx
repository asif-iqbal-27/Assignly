"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { apiFetch, ApiError } from "@/lib/api";
import type { AuthResponse } from "@/lib/types";

export default function LoginPage() {
  const router = useRouter();
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    if (!userName || !password) {
      setError("Username and password are required.");
      return;
    }

    setLoading(true);
    try {
      const auth = await apiFetch<AuthResponse>("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ userName, password }),
      });

      localStorage.setItem("token", auth.token);
      localStorage.setItem("role", auth.role);
      localStorage.setItem("userName", auth.userName);

      if (auth.role === "Admin") {
        router.push("/admin/users");
      } else if (auth.role === "Teacher") {
        router.push("/teacher");
      } else {
        router.push("/student");
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Login failed.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      <div className="card card-max" style={{ width: "100%" }}>
        <h1>Assignly</h1>
        <p className="page-subtitle">Sign in to continue</p>

        <form className="form" onSubmit={handleSubmit}>
          {error && <div className="banner banner-error">{error}</div>}

          <div className="form-row">
            <label htmlFor="userName">Username</label>
            <input
              id="userName"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              autoComplete="username"
            />
          </div>
          <div className="form-row">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
            />
          </div>
          <button type="submit" disabled={loading}>
            {loading ? "Logging in..." : "Log in"}
          </button>
        </form>
      </div>
    </div>
  );
}
