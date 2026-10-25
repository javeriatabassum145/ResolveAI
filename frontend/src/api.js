
const API_BASE_URL = (
  import.meta.env.VITE_API_BASE_URL || "https://localhost:7120"
).replace(/\/$/, "");

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(`API ${response.status}: ${message}`);
  }

  if (response.status === 204) return null;

  return response.json();
}

export function getPayments() {
  return request("/api/Payments");
}

export function getPayment(paymentId) {
  return request(`/api/Payments/${paymentId}`);
}


export function investigatePayment(paymentId, question) {
  return request(`/api/Investigations/${paymentId}`, {
    method: "POST",
    body: JSON.stringify({ question }),
  });
}
