import React from "react";
import { Link } from "react-router-dom";
import "../styles/table.css";

function LeadTable({ leads, authRole, token, refreshLeads }) {

  // ✅ Convert Lead
  const handleConvert = async (id) => {
    try {
      await fetch(`http://localhost:5000/api/leads/${id}/convert`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      });

      refreshLeads(); // reload data
    } catch (error) {
      console.error("Convert error:", error);
    }
  };

  // ✅ Delete Lead
  const handleDelete = async (id) => {
    try {
      await fetch(`http://localhost:5000/api/leads/${id}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      refreshLeads();
    } catch (error) {
      console.error("Delete error:", error);
    }
  };

  return (
    <div className="table-wrapper">
      <table className="lead-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Status</th>
            <th>Source</th>
            <th>Priority</th>
            <th>Actions</th>
          </tr>
        </thead>

        <tbody>
          {leads.length === 0 ? (
            <tr>
              <td className="empty-cell" colSpan="6">
                No leads found.
              </td>
            </tr>
          ) : (
            leads.map((lead) => {
              const id = lead.id || lead._id;
              const converted = lead.status === "Converted";
              const canEdit = !converted;

              return (
                <tr key={id}>
                  <td>{lead.name}</td>
                  <td>{lead.email}</td>

                  <td>
                    <span className={`status-badge status-${lead.status?.toLowerCase()}`}>
                      {lead.status}
                    </span>
                  </td>

                  <td>{lead.source}</td>

                  <td>
                    <span className={`priority-badge priority-${lead.priority?.toLowerCase()}`}>
                      {lead.priority}
                    </span>
                  </td>

                  <td className="actions-cell">
                    
                    {/* View */}
                    <Link className="btn btn-small" to={`/leads/${id}`}>
                      View
                    </Link>

                    {/* Edit */}
                    <Link
                      className={`btn btn-small ${!canEdit ? "btn-disabled" : ""}`}
                      to={canEdit ? `/leads/edit/${id}` : "#"}
                      onClick={(e) => !canEdit && e.preventDefault()}
                    >
                      Edit
                    </Link>

                    {/* Convert */}
                    {authRole !== "SalesRep" && !converted && (
                      <button
                        className="btn btn-small btn-convert"
                        onClick={() => handleConvert(id)}
                      >
                        Convert
                      </button>
                    )}

                    {/* Delete */}
                    {!converted && (
                      <button
                        className="btn btn-small btn-delete"
                        onClick={() => handleDelete(id)}
                      >
                        Delete
                      </button>
                    )}

                    {/* After Converted */}
                    {converted && (
                      <span className="converted-label">Converted</span>
                    )}
                  </td>
                </tr>
              );
            })
          )}
        </tbody>
      </table>
    </div>
  );
}

export default LeadTable;