
const API_BASE_URL = "https://localhost:7120";

export async function getPayments() {
  const response = await fetch(`${API_BASE_URL}/api/Payments`);

  if (!response.ok) {
    throw new Error(`Failed to load payments: ${response.status}`);
  }

  return await response.json();
}
