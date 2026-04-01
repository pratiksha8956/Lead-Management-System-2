import React, { useEffect, useMemo, useState } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis
} from "recharts";
import Spinner from "../components/Spinner";
import { getLeadAnalytics } from "../services/leadService";
import { getErrorMessage } from "../utils/error";
import "../styles/analytics.css";

function toChartData(counts) {
  return Object.entries(counts || {}).map(([namevalue, value]) => ({
    name: namevalue,
    value
  }));
}

function RechartsBarCard({ title, data }) {
  if (!data?.length) {
    return (
      <div className="card chart-card">
        <h3>{title}</h3>
        <p className="muted">No data yet.</p>
      </div>
    );
  }

  return (
    <div className="card chart-card">
      <h3>{title}</h3>
      <div className="recharts-wrap">
        <ResponsiveContainer width="100%" height={280}>
          <BarChart data={data} margin={{ top: 8, right: 8, left: 0, bottom: 8 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" tick={{ fontSize: 12 }} interval={0} angle={-20} textAnchor="end" height={60} />
            <YAxis allowDecimals={false} />
            <Tooltip />
            <Bar dataKey="value" fill="#4f46e5" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}

function StatCard({ title, value }) {
  return (
    <div className="stat-card">
      <p>{title}</p>
      <h2>{value}</h2>
    </div>
  );
}

function AnalyticsPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [statusCounts, setStatusCounts] = useState({});
  const [sourceCounts, setSourceCounts] = useState({});
  const [salesRepCounts, setSalesRepCounts] = useState({});
  const [conversionRate, setConversionRate] = useState(0);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        const analytics = await getLeadAnalytics();
        setStatusCounts(analytics.statusCounts || {});
        setSourceCounts(analytics.sourceCounts || {});
        setSalesRepCounts(analytics.salesRepCounts || {});
        setConversionRate(analytics.conversionRate || 0);
      } catch (err) {
        setError(getErrorMessage(err, "Unable to load analytics"));
      } finally {
        setLoading(false);
      }
    };

    fetchAnalytics();
  }, []);

  const formattedRate = useMemo(
    () => `${Number(conversionRate).toFixed(1)}%`,
    [conversionRate]
  );

  const totalLeads = Object.values(statusCounts).reduce((a, b) => a + b, 0);

  const statusData = useMemo(() => toChartData(statusCounts), [statusCounts]);
  const sourceData = useMemo(() => toChartData(sourceCounts), [sourceCounts]);
  const repData = useMemo(() => toChartData(salesRepCounts), [salesRepCounts]);

  if (loading) return <Spinner />;

  return (
    <section className="analytics-container">
      <div className="page-header card">
        <h2>Analytics Dashboard</h2>
        <p>Redis-backed metrics with five-minute caching on the API.</p>
      </div>

      {error && <div className="error-banner">{error}</div>}

      <div className="stats-grid">
        <StatCard title="Total Leads" value={totalLeads} />
        <StatCard title="Conversion Rate" value={formattedRate} />
        <StatCard title="Sources" value={Object.keys(sourceCounts).length} />
        <StatCard title="Sales Reps" value={Object.keys(salesRepCounts).length} />
      </div>

      <div className="analytics-grid">
        <RechartsBarCard title="Leads by Status" data={statusData} />
        <RechartsBarCard title="Leads by Source" data={sourceData} />
        <RechartsBarCard title="Leads by Sales Rep" data={repData} />
      </div>
    </section>
  );
}

export default AnalyticsPage;
