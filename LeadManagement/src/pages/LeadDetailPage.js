import React, { useCallback, useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import Badge from "../components/Badge";
import Button from "../components/Button";
import ConfirmModal from "../components/ConfirmModal";
import Input from "../components/Input";
import Spinner from "../components/Spinner";
import { useAuth } from "../context/AuthContext";
import { addLeadInteraction, convertLead, deleteLead, getLeadById } from "../services/leadService";
import { getErrorMessage } from "../utils/error";
import "../styles/form.css";

function LeadDetailPage({ setToast }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const { role } = useAuth();
  const [lead, setLead] = useState(null);
  const [interactions, setInteractions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busyAction, setBusyAction] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [interactionForm, setInteractionForm] = useState({
    notes: "",
    interactionDate: "",
    followUpDate: ""
  });

  const canConvert = role === "SalesManager" || role === "Admin";

  const refreshLead = useCallback(async () => {
    const data = await getLeadById(id);
    setLead(data);
    setInteractions(Array.isArray(data.interactions) ? data.interactions : []);
  }, [id]);

  const fetchLead = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      await refreshLead();
    } catch (err) {
      setError(getErrorMessage(err, "Unable to load lead details"));
    } finally {
      setLoading(false);
    }
  }, [refreshLead]);

  useEffect(() => {
    fetchLead();
  }, [fetchLead]);

  const isConverted = lead?.status === "Converted";

  const handleConvert = async () => {
    setBusyAction(true);
    try {
      await convertLead(id);
      setToast({ show: true, type: "success", message: "Lead converted" });
      await refreshLead();
    } catch (err) {
      setToast({ show: true, type: "error", message: getErrorMessage(err, "Unable to convert lead") });
    } finally {
      setBusyAction(false);
    }
  };

  const handleDelete = async () => {
    setBusyAction(true);
    try {
      await deleteLead(id);
      setToast({ show: true, type: "success", message: "Lead deleted" });
      navigate("/leads");
    } catch (err) {
      setToast({ show: true, type: "error", message: getErrorMessage(err, "Unable to delete lead") });
    } finally {
      setBusyAction(false);
      setShowDeleteConfirm(false);
    }
  };

  const handleAddInteraction = async (e) => {
    e.preventDefault();
    if (!interactionForm.interactionDate) {
      setToast({ show: true, type: "error", message: "Interaction date is required" });
      return;
    }

    setBusyAction(true);
    try {
      await addLeadInteraction(id, {
        notes: interactionForm.notes,
        interactionDate: new Date(interactionForm.interactionDate).toISOString(),
        followUpDate: interactionForm.followUpDate ? new Date(interactionForm.followUpDate).toISOString() : null
      });
      setInteractionForm({ notes: "", interactionDate: "", followUpDate: "" });
      setToast({ show: true, type: "success", message: "Interaction saved" });
      await refreshLead();
    } catch (err) {
      setToast({ show: true, type: "error", message: getErrorMessage(err, "Unable to save interaction") });
    } finally {
      setBusyAction(false);
    }
  };

  if (loading) return <Spinner message="Loading lead detail..." />;
  if (error) return <div className="error-banner">{error}</div>;
  if (!lead) return null;

  return (
    <section className="card detail-card">
      <div className="page-header">
        <div>
          <h2>{lead.name}</h2>
          <p>{lead.position} at {lead.company}</p>
        </div>
        <div className="action-row">
          <Link to={isConverted ? "#" : `/leads/edit/${id}`} className={`btn ${isConverted ? "btn-disabled" : ""}`}>Edit</Link>
          {canConvert && <Button onClick={handleConvert} disabled={lead.status !== "Qualified" || busyAction}>Convert</Button>}
          {canConvert && (
            <Button variant="danger" onClick={() => setShowDeleteConfirm(true)} disabled={isConverted || busyAction}>
              Delete
            </Button>
          )}
        </div>
      </div>

      <div className="detail-grid">
        <div><strong>Email:</strong> {lead.email}</div>
        <div><strong>Phone:</strong> {lead.phone}</div>
        <div><strong>Status:</strong> <Badge type="status" value={lead.status} /></div>
        <div><strong>Source:</strong> {lead.source}</div>
        <div><strong>Priority:</strong> <Badge type="priority" value={lead.priority} /></div>
        {lead.assignedTo && <div><strong>Assigned to:</strong> {lead.assignedTo}</div>}
      </div>

      {isConverted && <p className="info-note">Converted leads are read-only.</p>}

      <div className="card interactions-section">
        <h3>Interactions</h3>
        {interactions.length === 0 ? (
          <p className="muted">No interactions logged yet.</p>
        ) : (
          <ul className="interaction-list">
            {interactions.map((row) => (
              <li key={row.interactionId}>
                <div><strong>{new Date(row.interactionDate).toLocaleString()}</strong></div>
                <div>{row.notes}</div>
                {row.followUpDate && <div className="muted">Follow-up: {new Date(row.followUpDate).toLocaleString()}</div>}
              </li>
            ))}
          </ul>
        )}

        {!isConverted && (
          <form className="form-grid interaction-form" onSubmit={handleAddInteraction}>
            <Input
              id="notes"
              name="notes"
              label="Notes"
              value={interactionForm.notes}
              onChange={(e) => setInteractionForm((p) => ({ ...p, notes: e.target.value }))}
            />
            <Input
              id="interactionDate"
              name="interactionDate"
              label="Interaction date"
              type="datetime-local"
              value={interactionForm.interactionDate}
              onChange={(e) => setInteractionForm((p) => ({ ...p, interactionDate: e.target.value }))}
            />
            <Input
              id="followUpDate"
              name="followUpDate"
              label="Follow-up (optional)"
              type="datetime-local"
              value={interactionForm.followUpDate}
              onChange={(e) => setInteractionForm((p) => ({ ...p, followUpDate: e.target.value }))}
            />
            <Button type="submit" disabled={busyAction}>{busyAction ? "Saving..." : "Log interaction"}</Button>
          </form>
        )}
      </div>

      <ConfirmModal
        open={showDeleteConfirm}
        title="Delete this lead?"
        message="This action cannot be undone. The lead record will be removed permanently."
        confirmLabel={busyAction ? "Deleting..." : "Delete Lead"}
        onCancel={() => setShowDeleteConfirm(false)}
        onConfirm={handleDelete}
      />
    </section>
  );
}

export default LeadDetailPage;
