import axios from "axios";
import { isTokenExpired } from "../utils/auth";

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL || "http://localhost:5000/api",
  headers: {
    "Content-Type": "application/json"
  }
});

// 🔐 REQUEST INTERCEPTOR
api.interceptors.request.use((config) => {
  const url = config.url || "";

  // Skip token for login
  if (url.includes("auth/login")) {
    return config;
  }

  const token = localStorage.getItem("token");

  if (!token || isTokenExpired(token)) {
    localStorage.removeItem("token");
    localStorage.removeItem("user");

    if (!window.location.pathname.includes("/login")) {
      window.location.href = "/login";
    }

    return config;
  }

  config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// ✅ RESPONSE INTERCEPTOR (MAIN FIX)
api.interceptors.response.use(
  (response) => {
    // Always return response normally
    return response;
  },
  (error) => {
    // 🔐 Handle unauthorized
    if (error.response?.status === 401) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");

      if (!window.location.pathname.includes("/login")) {
        window.location.href = "/login";
      }
    }

    // ✅ IMPORTANT: treat API responses as success
    if (error.response) {
      return Promise.resolve(error.response);
    }

    // ❌ Only reject if network/server crash
    return Promise.reject(error);
  }
);

export default api;