const API_URL = (
  import.meta.env.VITE_API_URL || "https://localhost:7120/api"
).replace(/\/$/, "");

async function request(url, options = {}) {
  const response = await fetch(`${API_URL}${url}`, {
    ...options,
    headers: {
      ...(options.body ? { "Content-Type": "application/json" } : {}),
      ...options.headers,
    },
  });

  const text = await response.text();

  let data;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }

  if (!response.ok) {
    const message =
      data?.message ||
      data?.error ||
      (typeof data === "string" ? data : null) ||
      `Request failed (${response.status})`;

    throw new Error(message);
  }

  return data;
}

export function getPayments() {
  return request("/Payments");
}

export function getPayment(id) {
  return request(`/Payments/${id}`);
}

export function getDashboard() {
  return request("/Dashboard");
}

export function investigatePayment(paymentId, question) {
  return request(`/AI/investigate/${paymentId}`, {
    method: "POST",
    body: JSON.stringify({ question }),
  });
}