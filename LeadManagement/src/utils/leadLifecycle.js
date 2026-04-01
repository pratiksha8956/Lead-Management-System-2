export function getAllowedTransitions(currentStatus) {
  if (currentStatus === "Converted") return ["Converted"];
  if (currentStatus === "Unqualified") return ["Unqualified"];

  const map = {
    New: ["New", "Contacted"],
    Contacted: ["Contacted", "Qualified", "Unqualified"],
    Qualified: ["Qualified", "Unqualified"]
  };

  return map[currentStatus] || ["New"];
}

export function isValidTransition(fromStatus, toStatus) {
  return getAllowedTransitions(fromStatus).includes(toStatus);
}
