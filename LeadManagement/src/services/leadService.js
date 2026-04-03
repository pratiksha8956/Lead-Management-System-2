import api from "./api";

// 🔐 AUTH
export const loginUser = async (credentials) => {
  const res = await api.post("/auth/login", {
    email: credentials.email,
    password: credentials.password
  });
  if (res.status >= 400) {
    const err = new Error(res.data?.message || "Login failed");
    err.response = res;
    throw err;
  }
  return res.data;
};

export const registerUser = async ({ email, password, displayName }) => {
  const res = await api.post("/auth/register", {
    email,
    password,
    displayName
  });
  if (res.status >= 400) {
    const err = new Error(res.data?.message || "Registration failed");
    err.response = res;
    throw err;
  }
  return res.data;
};

// 📋 GET ALL LEADS
export const getLeads = async (params = {}) => {
  const res = await api.get("/leads", {
    params: {
      page: params.page,
      limit: params.limit,
      status: params.status || undefined,
      source: params.source || undefined
    }
  });
  return res.data;
};

// 📄 GET ONE LEAD
export const getLeadById = async (id) => {
  const res = await api.get(`/leads/${id}`);
  return res.data;
};

// 💬 INTERACTIONS
export const getLeadInteractions = async (leadId) => {
  const res = await api.get(`/leads/${leadId}/interactions`);
  return res.data;
};

export const addLeadInteraction = async (leadId, payload) => {
  const res = await api.post(`/leads/${leadId}/interactions`, payload);
  return res.data;
};

// ➕ CREATE LEAD (FIXED)
export const createLead = async (payload) => {
  const res = await api.post("/leads", {
    name: payload.name,
    email: payload.email,
    phone: payload.phone,
    company: payload.company,
    position: payload.position,
    source: payload.source,
    priority: payload.priority,
    assignedSalesRepId: payload.assignedSalesRepId ?? null
  });

  // ✅ Always return something valid
  return res.data || { success: true };
};

// ✏️ UPDATE LEAD (FIXED)
export const updateLead = async (id, payload) => {
  const res = await api.put(`/leads/${id}`, {
    name: payload.name,
    email: payload.email,
    phone: payload.phone,
    company: payload.company,
    position: payload.position,
    status: payload.status,
    source: payload.source,
    priority: payload.priority,
    assignedSalesRepId: payload.assignedSalesRepId ?? null
  });

  return res.data || { success: true };
};

// ❌ DELETE LEAD (FIXED)
export const deleteLead = async (id) => {
  await api.delete(`/leads/${id}`);
  return { success: true };
};

// 🔄 CONVERT LEAD
export const convertLead = async (id) => {
  const res = await api.post(`/leads/${id}/convert`);
  return res.data || { success: true };
};

// 📊 ANALYTICS
export const getLeadAnalytics = async () => {
  const [byStatus, bySource, rateRes, bySalesRep] = await Promise.all([
    api.get("/analytics/by-status"),
    api.get("/analytics/by-source"),
    api.get("/analytics/conversion-rate"),
    api.get("/analytics/by-salesrep")
  ]);

  return {
    statusCounts: byStatus.data,
    sourceCounts: bySource.data,
    salesRepCounts: bySalesRep.data,
    conversionRate: rateRes.data?.conversionRate ?? 0
  };
};
