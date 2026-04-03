import React, { useEffect, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import Button from "../components/Button";
import Input from "../components/Input";
import Spinner from "../components/Spinner";
import { useAuth } from "../context/AuthContext";
import { loginUser } from "../services/leadService";
import { getErrorMessage } from "../utils/error";
import "../styles/auth.css";

function LoginPage({ setToast }) {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const [form, setForm] = useState({ email: "", password: "" });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");

  useEffect(() => {
    if (location.state?.fromRegistration) {
      setInfo("Registration complete. Sign in with your new account.");
      navigate(location.pathname, { replace: true, state: {} });
    }
  }, [location, navigate]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const response = await loginUser(form);
      if (!response?.token) {
        setError("Invalid credentials");
        return;
      }
      const token = response.token;
      const u = response.user || {};
      const user = {
        userId: u.userId,
        name: u.name || u.email,
        email: u.email,
        role: u.role,
        salesRepId: u.salesRepId ?? null
      };

      login({ token, user });
      setToast({ show: true, type: "success", message: "Login successful" });
      navigate("/leads", { replace: true });
    } catch (err) {
      setError(getErrorMessage(err, "Invalid credentials"));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card card">
        <h1>Login</h1>
        <p>Sign in to manage your leads</p>

        {info && <div className="info-banner">{info}</div>}
        {error && <div className="error-banner">{error}</div>}

        <form onSubmit={handleSubmit} className="form-grid">
          <Input id="email" type="email" name="email" label="Email" value={form.email} onChange={handleChange} required />
          <Input id="password" type="password" name="password" label="Password" value={form.password} onChange={handleChange} required />

          <Button type="submit" disabled={loading}>{loading ? "Logging in..." : "Login"}</Button>
        </form>

        <p className="auth-switch">
          New here? <Link to="/register">Create an account</Link>
        </p>

        {loading && <Spinner message="Authenticating..." />}
      </div>
    </div>
  );
}

export default LoginPage;
