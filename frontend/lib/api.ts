const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5058";

// Thrown for any non-2xx response. Pages catch this and show err.message.
export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

// The one function every page uses to talk to the backend. It attaches the
// JWT from localStorage (if there is one) and turns error responses into a
// thrown ApiError with a plain-text message pulled from the ErrorOr body.
export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = typeof window !== "undefined" ? localStorage.getItem("token") : null;

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string> | undefined),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (response.status === 401) {
    if (typeof window !== "undefined") {
      localStorage.removeItem("token");
      localStorage.removeItem("role");
      localStorage.removeItem("userName");
      window.location.href = "/login";
    }
    throw new ApiError(401, "Your session has expired. Please log in again.");
  }

  if (!response.ok) {
    let message = response.statusText;
    try {
      const body = await response.json();
      message = body?.errors?.[0]?.description ?? message;
    } catch {
      // No JSON body to read - fall back to the status text.
    }
    throw new ApiError(response.status, message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
