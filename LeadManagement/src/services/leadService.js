import api from "./api";

export const loginUser = async (credentials) => {
  const { data } = await api.post("/auth/login", {
    email: credentials.email,
    password: credentials.password
  });
  return data;
};

export const getLeads = async (params = {}) => {
  const { data } = await api.get("/leads", {
    params: {
      page: params.page,
      limit: params.limit,
      status: params.status || undefined,
      source: params.source || undefined
    }
  });
  return data;
};

export const getLeadById = async (id) => {
  const { data } = await api.get(`/leads/${id}`);
  return data;
};

export const getLeadInteractions = async (leadId) => {
  const { data } = await api.get(`/leads/${leadId}/interactions`);
  return data;
};

export const addLeadInteraction = async (leadId, payload) => {
  const { data } = await api.post(`/leads/${leadId}/interactions`, payload);
  return data;
};

export const createLead = async (payload) => {
  const { data } = await api.post("/leads", {
    name: payload.name,
    email: payload.email,
    phone: payload.phone,
    company: payload.company,
    position: payload.position,
    source: payload.source,
    priority: payload.priority,
    assignedSalesRepId: payload.assignedSalesRepId ?? null
  });
  return data;
};

export const updateLead = async (id, payload) => {
  const { data } = await api.put(`/leads/${id}`, {
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
  return data;
};

export const deleteLead = async (id) => {
  await api.delete(`/leads/${id}`);
  return { success: true };
};

export const convertLead = async (id) => {
  const { data } = await api.post(`/leads/${id}/convert`);
  return data;
};

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
