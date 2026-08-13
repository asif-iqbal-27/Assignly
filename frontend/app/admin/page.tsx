"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function AdminIndexPage() {
  const router = useRouter();

  useEffect(() => {
    if (localStorage.getItem("role") !== "Admin") {
      router.replace("/login");
      return;
    }
    router.replace("/admin/users");
  }, [router]);

  return null;
}
