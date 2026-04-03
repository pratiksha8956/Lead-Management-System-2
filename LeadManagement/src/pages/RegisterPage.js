import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Button from "../components/Button";
import Input from "../components/Input";
import Spinner from "../components/Spinner";
import { registerUser } from "../services/leadService";
import { getErrorMessage } from "../utils/error";
import "../styles/auth.css";

function RegisterPage({ setToast }) {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    displayName: "",
    email: "",
    password: "",
    confirmPassword: ""
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (form.password !== form.confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    if (form.password.length < 6) {
      setError("Password must be at least 6 characters.");
      return;
    }

    setLoading(true);
    try {
      await registerUser({
        email: form.email.trim(),
        password: form.password,
        displayName: form.displayName.trim()
      });
      setToast({ show: true, type: "success", message: "Account created. Please sign in." });
      navigate("/login", { replace: true, state: { fromRegistration: true } });
    } catch (err) {
      setError(getErrorMessage(err, "Registration failed"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card card">
        <h1>Create account</h1>
        <p>Register to access the lead workspace. You will sign in on the next screen.</p>

        {error && <div className="error-banner">{error}</div>}

        <form onSubmit={handleSubmit} className="form-grid">
          <Input
            id="displayName"
            name="displayName"
            label="Full name"
            value={form.displayName}
            onChange={handleChange}
            required
            autoComplete="name"
          />
          <Input
            id="email"
            type="email"
            name="email"
            label="Email"
            value={form.email}
            onChange={handleChange}
            required
            autoComplete="email"
          />
          <Input
            id="password"
            type="password"
            name="password"
            label="Password"
            value={form.password}
            onChange={handleChange}
            required
            autoComplete="new-password"
          />
          <Input
            id="confirmPassword"
            type="password"
            name="confirmPassword"
            label="Confirm password"
            value={form.confirmPassword}
            onChange={handleChange}
            required
            autoComplete="new-password"
          />

          <Button type="submit" disabled={loading}>{loading ? "Creating account..." : "Register"}</Button>
        </form>

        <p className="auth-switch">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>

        {loading && <Spinner message="Creating your account..." />}
      </div>
    </div>
  );
}

export default RegisterPage;
