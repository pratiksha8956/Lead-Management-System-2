export function getErrorMessage(error, fallback) {
  if (!error) return fallback;

  const data = error.response?.data;
  if (data?.errors?.length) {
    const first = data.errors[0]?.errorMessage || data.errors[0]?.message;
    if (first) return first;
  }
  if (data?.message) return data.message;
  if (data?.error) return data.error;
  if (error.message === "Network Error") return "Network error. Please check your connection and try again.";

  return fallback;
}
